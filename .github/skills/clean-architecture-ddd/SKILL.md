---
name: clean-architecture-ddd
description: "Clean Architecture + DDD rules for the Hopaut modular monolith (BingoAPI). Use when: creating or changing aggregates, entities, value objects, strongly-typed IDs, domain events, Result/Error, commands, queries, handlers, repositories or module layering; fixing primitive obsession, anemic domain models, public setters, or business logic in endpoints and handlers; writing domain or handler unit tests."
user-invocable: false
---

# Clean Architecture + DDD

Leading word: **invariants**. An aggregate can never be observed in an invalid state, and only the domain decides what valid means.

Code for every pattern below: [patterns.md](./references/patterns.md).

## 1. Layers and the dependency rule

| Project | May reference | Never references |
|---------|---------------|------------------|
| `Hopaut.SharedKernel` | `Mediator.Abstractions` (MediatR until the swap) | EF Core, ASP.NET Core, Hangfire, MassTransit, any module |
| `Modules.<X>.Domain` | SharedKernel; NetTopologySuite (recorded decision, spatial only) | Application, Infrastructure, Api, EF Core, any other module |
| `Modules.<X>.Application` | own Domain, SharedKernel, BuildingBlocks.Application, other modules' `Contracts` | EF Core, ASP.NET Core, MassTransit, Hangfire, Infrastructure, Api |
| `Modules.<X>.Contracts` | SharedKernel, IntegrationEvents | every Domain / Application / Infrastructure / Api project |
| `Modules.<X>.Infrastructure` | own Application + Domain, BuildingBlocks.Infrastructure, other modules' `Contracts` | Api; other modules' non-Contracts projects |
| `Modules.<X>.Api` | own Application + Contracts, BuildingBlocks.Api | Infrastructure; other modules' non-Contracts projects |
| `Hopaut.Api.Host` | everything (composition root only) | business logic |

- Every row is enforced by a NetArchTest rule in `tests/Hopaut.ArchitectureTests`. A layering rule without a test is unfinished work.
- Cross-module traffic is either an integration event (async) or a read-only query interface in the owning module's `Contracts`, implemented in its Infrastructure. Never cross-module entities, joins or DbContexts.
- Modules without a domain (Notifications, Payments) keep Application ports + Infrastructure adapters only; do not invent a Domain project for them.

## 2. Aggregates

- **One aggregate per transaction.** A command changes exactly one aggregate; others react to its domain events (same module) or integration events (other modules).
- **Reference other aggregates by ID only.** No navigation properties between aggregates and no child → root back-references (`Picture.Post` is a defect).
- **Construction:** private parameterless constructor for EF, plus a public static factory (`Create`, or a domain verb such as `Request`) that receives IDs, value objects and `DateTimeOffset now`, enforces every invariant and raises the creation event. Return `Result<T>` when a business rule can reject creation; throw `ArgumentException` only for programmer errors.
- **Identity is known before any event is raised.** New aggregates get `Guid.CreateVersion7()`-based IDs in the factory. Aggregates with existing `int` keys receive the next value from a per-aggregate Postgres sequence through their repository (`NextIdAsync`) and map the key `ValueGeneratedNever()`, so wire IDs stay unchanged. An event carrying a default ID is a bug.
- **State:** `{ get; private set; }` only; no public setters in Domain projects. Collections are a private `List<T>` exposed as `IReadOnlyList<T>`. No integer flags for state (`ActiveFlag`); use an enum or derive it from value objects.
- **Behavior:** intention-revealing methods (`Accept`, `Reschedule`, `AttachPicture`) that check the transition, mutate, raise an event and return `Result` when the rule can fail. `SetX` / `UpdateX` methods that copy fields are anemic; replace them with domain verbs.
- Children change only through the root, and only the root raises events (`Raise` is `protected` on `AggregateRoot`).
- **Time** arrives as a parameter (`DateTimeOffset now`, taken from `TimeProvider` in the handler). The domain never reads a clock.
- **Concurrency:** aggregates edited by more than one actor carry `uint Version` mapped to Postgres `xmin`; a concurrency exception maps to `ErrorType.Conflict`.
- Keep aggregates small; split when a collection can grow without bound.

## 3. Value objects and strongly-typed IDs

- A value object is immutable, equal by value and **valid by construction**: private constructor + `static Result<T> Create(...)`. No `IsValid` flags, no public constructor that skips validation.
- Prefer `sealed record` classes for value objects with invariants (a `default` struct bypasses validation); use `readonly record struct` only for IDs and value objects whose default is valid.
- Normalize on creation: trim text, upper-case currency codes, convert times to UTC.
- Hopaut catalog (create when you touch the concept): `Coordinates` (WGS-84 ranges), `TimeRange` (start before end, UTC), `Money` (`Amount` ≥ 0 + `Currency` ISO-4217 value object), `RatingValue` (1–5), `EmailAddress`, `PostTitle` / `Description` (length rules), `Capacity` (slots > 0), `RadiusMeters` (bounded), `MediaKey`, `PageSize` / `Cursor`.
- Strongly-typed IDs (`readonly record struct XxxId(T Value)`) implement `IParsable<XxxId>` so minimal APIs bind route values directly, and their `TryParse` rejects non-positive ints and empty strings or Guids.
- Primitives are allowed only at the edges: wire DTOs, integration-event payloads in Contracts, EF configurations and tests. Anywhere else, a domain concept typed as `string`, `int`, `long`, `double` or `Guid` is primitive obsession.
- Fixing primitive obsession must not change the wire by accident. Map value objects to the existing wire shape in the Api mapper; change the wire only through the rest-api-standards contract procedure.

## 4. Domain events

- `sealed record XxxDomainEvent(...) : IDomainEvent`, named in the past tense, carrying IDs and the facts handlers need; never the aggregate instance.
- Raised only inside aggregate methods via `Raise(...)`.
- Dispatched **before the module's single `SaveChangesAsync`**, so handlers' changes (same-module aggregates, outbox rows) commit atomically with the aggregate. Mechanics: messaging-integration skill.
- Domain event handlers never call external systems (push, e-mail, S3); they publish an integration event instead.
- `EventId` from `Guid.CreateVersion7()`; `OccurredOnUtc` as `DateTimeOffset`.

## 5. Result and Error

- Expected failures are values: `Result` / `Result<T>` carrying `Error(Code, Description, Type)`. Exceptions are for bugs and infrastructure faults only.
- `ErrorType` → HTTP: `Validation` 400, `Unauthorized` 401, `Forbidden` 403, `NotFound` 404, `Conflict` 409 (current state or concurrency), `Rule` 422 (domain rule on a valid request), `Failure` 500. The mapping lives only in BuildingBlocks.Api.
- Error codes are stable, lower-case and dotted: `<module>.<entity>.<reason>` (`attendance.participation.not_pending`). BingoMobile keys messages off them, so renaming one is a contract change.
- Each module owns a static error catalog per aggregate (`ParticipationErrors.NotPending(id)`); no ad-hoc string errors.
- `Result<T>.Value` throws on failure; always branch on `IsSuccess` first.

## 6. Application layer

- Commands change one aggregate and return `Result` or `Result<TId>`; queries never change state and return `Result<TDto>`. Both are `sealed record`s.
- Inside commands and queries, IDs are strongly typed. Other inputs are value objects, or primitives that the handler converts through value-object factories, collecting all failures into one validation error.
- **Handler shape:** load → authorize the resource → call a domain method → `Add` new aggregates → return. No business rules, no `SaveChangesAsync` (the UnitOfWork behavior commits), no `HttpContext` (use `ICurrentUser`), no clock (use `TimeProvider`), and always propagate `CancellationToken`.
- Shape validation (required, lengths, ranges) lives in FluentValidation validators run by `ValidationBehavior`. Invariants live in the domain; do not duplicate them in validators.
- Queries use a read port in Application (`IPostReadModel`) implemented in Infrastructure with `AsNoTracking()` projections. Never load aggregates to serve reads; filter and page in SQL.
- Resource-based authorization (owner or host checks) goes through policies / `IAuthorizationService` or a dedicated port (plan Step 6.9), not scattered `if (x.UserId != current)` checks.
- Ports live in Application, adapters in Infrastructure. One repository per aggregate root: `GetByIdAsync`, `Add`, `Remove` (+ `NextIdAsync` where needed). No generic repository, no leaked `IQueryable`, no `Update` method (change tracking handles it).

## 7. Persistence mapping

- Each module's DbContext maps only its own aggregates, in its own schema, plus its outbox and inbox tables.
- Value objects map as EF Core complex types; IDs through converters; `xmin` for concurrency; one `IEntityTypeConfiguration<T>` per aggregate. Never put data annotations in Domain.
- Changing a column type or representation requires an EF migration plus a data-conversion script in the same commit (plan cross-phase invariant).

## 8. Detection list

Search `src/**` with these patterns during audits and before closing a module.

| Problem | Pattern / where | Fix |
|---------|-----------------|-----|
| Public setters in Domain | `\{ get; set; \}` in `*.Domain` | private set + domain verb |
| Mutable collections exposed | `public List<` in `*.Domain` | private list + `IReadOnlyList<T>` |
| Clock in domain or handlers | `DateTime(Offset)?\.(Utc)?Now` | `now` parameter / `TimeProvider` |
| Unix timestamps | `long \w*(Time\|Stamp\|Date)` | `DateTimeOffset` (wire change → contract procedure) |
| Raw IDs | `(string\|int\|Guid) \w*Id\b` in Domain/Application signatures | strongly-typed ID |
| Domain values as primitives | `double\?? \w*Price`, `int\?? Currency`, `int ActiveFlag`, `string \w*(Email\|Url\|Title)` | value object / enum |
| Event raised with default ID | `AddDomainEvent\(new \w+\(\w+\.Id` inside a factory before the ID is set | assign ID first |
| Anyone can raise events | `public void AddDomainEvent` | `protected Raise` on `AggregateRoot` |
| Transient-entity equality | `Equals` comparing `Id` without a default-ID check | treat transient entities as unequal |
| Validation-flag value objects | `bool IsValid` on a value object | validate in the factory |
| Business rules in endpoints/handlers | conditionals on domain state in `*.Api/**` or handlers | aggregate / value object / domain service |
| `SaveChangesAsync` in handlers | `SaveChangesAsync` in `*.Application/**` | UnitOfWork behavior |
| Sync over async | `\.Result\b`, `\.Wait\(\)`, `GetAwaiter\(\)\.GetResult\(\)` | `await` |
| `async void` | `async void` | `async Task` |
| Swallowed exceptions | `catch \(Exception` without rethrow or typed handling | targeted catch, `Result`, rethrow |
| Exceptions for expected flow | `throw new \w*(NotFound\|Invalid)\w*Exception` in Application | `Result` + `Error` |
| Service location | `GetRequiredService\|GetService` outside the composition root / background scopes | constructor injection |
| Null-forgiving to silence the compiler | `= default!;`, `null!`, `!\.` | model optionality honestly |
| Magic strings | role / claim / policy literals | constants |

## 9. Testing obligations

- **Domain:** every invariant, transition and raised event has a pure unit test (no mocks, no database). Names follow `Method_Scenario_Expected`.
- **Application:** every handler has tests for success and each failure path, using NSubstitute fakes for ports and `FakeTimeProvider` for time.
- **Infrastructure:** repository and read-model tests run against Testcontainers Postgres + PostGIS for filtering, paging and spatial queries.
- **Architecture:** one NetArchTest per row of the layer table, plus "no public setters in Domain" and "aggregates have no public constructors".
- Target ≥ 70 % line coverage for Domain + Application per module (plan Definition of Done).

## Completion criterion

A domain or application change is complete only when:
- no Domain type in the touched module exposes a public setter or a mutable collection;
- every touched domain concept is a value object or strongly-typed ID, and the wire contract is unchanged or synced;
- every state change goes through a domain method covered by a unit test;
- handlers contain orchestration only, and every failure is a typed `Error` with a stable code;
- the architecture tests cover the touched rules and pass.
