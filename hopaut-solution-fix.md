# Hopaut Backend – Refactor Plan to a Modular Monolith with Clean Architecture

> Companion to `Hopaut-Backend-State.md`. This document is the **target architecture and migration plan** for the BingoAPI codebase.
>
> Goal: turn a tangled .NET 5 monolith into a **modular monolith** on .NET 10 (LTS-track) where every module follows **Clean Architecture**, modules are isolated, and cross-module communication is explicit (in-process bus today, network/queue tomorrow).

---

## 0. Guiding Principles

1. **Modules first, layers second.** Vertical slices (Identity, Posts, Attendance, …) are the primary boundary; Clean-Architecture layers (Domain / Application / Infrastructure / Api) are *inside* each module.
2. **One DB, one process – isolated schemas per module.** Each module owns its tables under a Postgres schema (`identity`, `posts`, `attendance`, …). No module reads another module’s tables directly.
3. **Inter-module communication is explicit.** Modules call each other only through:
   * Public **Contracts** (DTOs + integration events) exposed by the source module.
   * An **in-process message bus** (MediatR notifications today; can be swapped for MassTransit/RabbitMQ/SQS later without changing handlers).
4. **No shared EF entities.** Each module has its own `DbContext` and its own entities; Identity user is referenced *by id only* across modules.
5. **All persistence behind the module’s Application layer**, no `DbContext` injected into controllers or services from other modules.
6. **Outbox** for integration events that trigger external side-effects (push, e-mail, S3 cleanup).
7. **Async by default** for external I/O (push notifications, e-mail, S3 deletion) via background workers consuming the outbox.

---

## 1. Target Solution Layout

```
Hopaut.sln
├── src/
│   ├── Bootstrap/
│   │   └── Hopaut.Api.Host/                          # ASP.NET Core composition root (Program.cs)
│   │
│   ├── Shared/
│   │   ├── Hopaut.SharedKernel/                      # primitives: Result, Error, ValueObject, EntityId, Money, Coordinates, DomainEvent base
│   │   ├── Hopaut.BuildingBlocks.Application/        # MediatR pipeline behaviours (Validation, Logging, Transaction, Caching), CQRS interfaces, IUnitOfWork, IDateTime, IUserContext, IIntegrationEventBus
│   │   ├── Hopaut.BuildingBlocks.Infrastructure/     # EF Core base (UoW, Outbox, Inbox), Redis cache, OpenTelemetry, Serilog wiring, generic exception middleware
│   │   ├── Hopaut.BuildingBlocks.Api/                # Minimal-API endpoint conventions, ProblemDetails, common filters, auth helpers
│   │   └── Hopaut.IntegrationEvents/                 # Cross-module integration event contracts (only)
│   │
│   └── Modules/
│       ├── Identity/
│       │   ├── Hopaut.Modules.Identity.Domain/
│       │   ├── Hopaut.Modules.Identity.Application/
│       │   ├── Hopaut.Modules.Identity.Infrastructure/
│       │   ├── Hopaut.Modules.Identity.Api/
│       │   └── Hopaut.Modules.Identity.Contracts/    # Public DTOs/events exposed to other modules + tests
│       ├── Users/                # Profile, avatar, public profile data (split off from Identity)
│       ├── Posts/                # Post + Event hierarchy, location, pictures, tags, "near-me" feed
│       ├── Attendance/           # Participation, accept/reject, slots, rules
│       ├── Announcements/        # Host → attendees announcements
│       ├── Ratings/              # Post-event ratings, user reputation
│       ├── Moderation/           # Post reports + user reports
│       ├── BugReports/           # Bug + screenshots
│       ├── Notifications/        # OneSignal adapter, email adapter (consumes integration events)
│       ├── Media/                # S3 upload/delete, image processing pipeline
│       └── Payments/             # Proxy to MyServices gateway (still thin)
│
├── tests/
│   ├── Hopaut.ArchitectureTests/                     # NetArchTest – enforces module isolation rules
│   ├── Hopaut.UnitTests.<Module>/                    # per module, no infra
│   ├── Hopaut.IntegrationTests.<Module>/             # per module, Testcontainers (Postgres + Redis + LocalStack S3)
│   └── Hopaut.E2ETests/                              # cross-module API tests via WebApplicationFactory
│
└── build/
    ├── Directory.Build.props      # nullable enable, treat warnings as errors, analyzers, langversion
    ├── Directory.Packages.props   # central package management
    └── .editorconfig              # team style
```

### Per-module project rules (Clean Architecture inside the module)

```
Modules/<X>/
├── Hopaut.Modules.<X>.Domain          # entities, value objects, domain events, domain services. NO EF, NO ASP.NET refs.
├── Hopaut.Modules.<X>.Application     # use cases (Commands/Queries + Handlers via MediatR), validators, DTOs internal to the module, ports (interfaces).
├── Hopaut.Modules.<X>.Infrastructure  # EF Core DbContext + configurations, repository implementations, external adapters, migrations.
├── Hopaut.Modules.<X>.Api             # endpoints (Minimal API or Controllers), authorization policies, request/response models, swagger.
└── Hopaut.Modules.<X>.Contracts       # PUBLIC: integration events + DTOs other modules / clients can consume. The only project other modules may reference.
```

**Allowed dependency arrows (enforced by NetArchTest):**

```
Api ─► Application ─► Domain
Infrastructure ─► Application ─► Domain
Api ─► Infrastructure (composition root only, registers DI)
Contracts ◄── Api,Application,Infrastructure  (anyone in the same module can publish)
OtherModule.Application ─► <X>.Contracts   (the ONLY allowed cross-module reference)
```

Disallowed: any reference to `<X>.Domain`, `<X>.Application`, `<X>.Infrastructure` from outside module `<X>`.

---

## 2. Module Catalog (responsibilities, owned data, public surface)

### 2.1 Identity
* **Owns:** ASP.NET Identity tables, `RefreshToken`, JWT issuance, password reset, email confirmation, Facebook external login, claims/roles management.
* **Public Contracts:** `UserRegisteredIntegrationEvent`, `UserEmailConfirmedIntegrationEvent`, `UserPasswordResetRequestedIntegrationEvent`. DTOs: `UserSummaryDto { UserId, Email, DisplayName }`.
* **Why split from Users?** Identity is auth concerns (password hash, lockout, tokens). Users module is profile data and what other modules actually read.

### 2.2 Users (Profiles)
* **Owns:** `UserProfile` (FirstName, LastName, ProfilePicture, Description, RegisteredAt) – keyed by Identity user id (no FK in DB to keep schemas isolated; consistency by convention + integration events).
* **Subscribes to:** `UserRegisteredIntegrationEvent` to create the profile row.
* **Public Contracts:** `UserProfileDto`, `UserProfileUpdatedIntegrationEvent`.

### 2.3 Posts
* **Owns:** `Post`, `Event` (TPH or TPC – see §6.4), `EventLocation`, `Picture`, `Tag`, `PostTags`, `RepeatableProperty`. PostGIS index lives here.
* **Public Contracts:**
  * `PostCreatedIntegrationEvent { PostId, HostUserId, EventTime, EndTime }`
  * `PostUpdatedIntegrationEvent { PostId, ChangedFields }`
  * `PostDeletedIntegrationEvent { PostId, ParticipantIds, Title }`
  * `PostDto`, `PostSummaryDto`
* **Depends on:** `Users.Contracts` for host display name (or fetches via query bus).
* **Notes:** the “near-me” feed is the only place where N+1 hurts the most – will be fixed with a single projected query joining ratings (ratings module exposes a query handler returning `IDictionary<UserId, double>`).

### 2.4 Attendance
* **Owns:** `Participation` (PostId, UserId, Status: Pending/Accepted/Rejected), slot accounting.
* **Subscribes to:** `PostDeletedIntegrationEvent` (cleanup), `PostUpdatedIntegrationEvent` (slot reduction validation).
* **Publishes:** `AttendanceRequestedIntegrationEvent`, `AttendanceAcceptedIntegrationEvent`, `AttendanceRejectedIntegrationEvent`, `AttendanceCancelledIntegrationEvent`.

### 2.5 Announcements
* **Owns:** `Announcement` rows, inbox/outbox views.
* **Subscribes to:** `AttendanceAcceptedIntegrationEvent` (to grant inbox visibility).
* **Publishes:** `AnnouncementCreatedIntegrationEvent` (consumed by Notifications).

### 2.6 Ratings
* **Owns:** `Rating { Id, RatedUserId, RaterUserId, Value, PostId }`. Aggregate “user reputation” is a read model maintained on event.
* **Public query:** `GetUserAverageRatingQuery` / `GetRatingsForUsersQuery` (batch).

### 2.7 Moderation
* **Owns:** `PostReport`, `UserReport`. Workflow states.
* **Subscribes to:** all `*DeletedIntegrationEvent` to close open reports.

### 2.8 BugReports
* **Owns:** `Bug`, `BugScreenshot` (screenshots are uploaded via Media module → URLs only stored here).

### 2.9 Notifications
* **Owns:** OneSignal adapter, e-mail templates. Stateless except for outbox/inbox.
* **Subscribes to:** every `*IntegrationEvent` that requires a user-facing message.
* **No public contracts** – it is a sink.

### 2.10 Media
* **Owns:** S3 client, image processing (`ImageLoader`), bucket settings, file naming, lifecycle.
* **Public Contracts:** `IMediaUploader` (port – consumed via Application bus, not direct DI), `MediaUploadedIntegrationEvent`. Or simpler: a typed query handler `UploadImagesCommand` returning URLs.
* **Subscribes to:** `PostDeletedIntegrationEvent` (delete pictures), `UserProfileDeletedIntegrationEvent` (delete avatar).

### 2.11 Payments
* **Owns:** `MyServices` HTTP client + token relay.
* **No DB ownership** today; could grow to own `Order`/`PaymentIntent` later.

---

## 3. Shared Kernel – what really lives in `Shared/`

Keep **Shared** small. Only put things that are stable, value-typed, and cannot belong to a single module.

### 3.1 `Hopaut.SharedKernel` (Domain primitives)

* `Result`, `Result<T>`, `Error`
* `Entity`, `AggregateRoot`, `IDomainEvent`
* `ValueObject` base
* `Money`, `Coordinates (Lat/Lng)`, `TimeRange`, `UnixTimestamp` value objects (kills the `Int64` Unix-seconds primitive obsession in one place)
* `IUserId`, strongly-typed ids per aggregate (`PostId`, `RatingId`, …)

### 3.2 `Hopaut.BuildingBlocks.Application`

* `ICommand`, `IQuery`, `ICommandHandler`, `IQueryHandler` (thin wrappers over MediatR if MediatR is adopted)
* MediatR pipeline behaviours: `ValidationBehavior` (FluentValidation), `LoggingBehavior`, `UnitOfWorkBehavior`, `CachingBehavior`
* `IUnitOfWork`, `IDateTimeProvider`, `ICurrentUser`, `IIntegrationEventBus`
* `PaginationRequest`, `PagedResult<T>` (the one canonical pagination shape – replaces the three current ones)

### 3.3 `Hopaut.BuildingBlocks.Infrastructure`

* `OutboxMessage`, `InboxMessage` entities + EF Core configuration
* Generic `OutboxPublisher` `BackgroundService`
* Redis cache abstractions + adapter
* Serilog + OpenTelemetry registration extensions
* Generic exception → ProblemDetails middleware
* JWT auth registration helpers

### 3.4 `Hopaut.BuildingBlocks.Api`

* Endpoint conventions (versioning, tags, auth defaults)
* Common authorization policies: `MustBeAuthenticated`, `MustBeAdmin`, `MustOwnResource<T>`
* `Result<T>` → `IResult` mapping for Minimal APIs

### 3.5 `Hopaut.IntegrationEvents`

* Just the **base class** `IntegrationEvent { Guid Id; DateTimeOffset OccurredOn; }`. Concrete events live in each module’s `*.Contracts` project, never in shared.

> What does **NOT** belong to Shared: AutoMapper profiles, EF entity classes, controllers, validators, domain logic. That all lives inside modules.

---

## 4. Inter-Module Dependencies (size & shape)

The honest answer: dependencies between modules will exist, but they are **small and one-directional** if we follow the rules.

### 4.1 Synchronous (compile-time) dependencies

Each arrow means "Module A.Application references Module B.Contracts to call a query handler / read a DTO".

```
Posts.Application         ─► Users.Contracts        (host display name)
Posts.Application         ─► Ratings.Contracts      (batch reputation lookup for the feed)
Attendance.Application    ─► Posts.Contracts        (validate the post exists, get host id, get slots)
Attendance.Application    ─► Users.Contracts        (display attendee profile)
Announcements.Application ─► Attendance.Contracts   (who can read this announcement)
Announcements.Application ─► Posts.Contracts        (post existence + ownership check)
Ratings.Application       ─► Attendance.Contracts   (only attendees who attended can rate)
Moderation.Application    ─► Posts.Contracts        (report a post → fetch summary)
Moderation.Application    ─► Users.Contracts
Notifications.Application ─► Users.Contracts        (resolve push tokens / locale)
BugReports.Application    ─► Media.Contracts        (upload screenshots)
Posts.Application         ─► Media.Contracts        (upload pictures)
Users.Application         ─► Media.Contracts        (upload avatar)
```

That’s ~12 directed edges. None are cyclic. The *Contracts* projects are leaf packages (no transitive infra), so the build graph stays a DAG.

### 4.2 Asynchronous (event) dependencies

Async is many-to-many but deliberately **decoupled** – publishers don’t know subscribers:

| Publisher | Event | Subscriber(s) |
|---|---|---|
| Identity | `UserRegisteredIntegrationEvent` | Users (creates profile), Notifications (welcome mail) |
| Identity | `UserPasswordResetRequestedIntegrationEvent` | Notifications |
| Users | `UserProfileUpdatedIntegrationEvent` | – (read model rebuild if any) |
| Posts | `PostCreatedIntegrationEvent` | Notifications (followers, future) |
| Posts | `PostUpdatedIntegrationEvent` | Attendance, Notifications |
| Posts | `PostDeletedIntegrationEvent` | Attendance, Announcements, Moderation, Media, Notifications |
| Attendance | `AttendanceRequestedIntegrationEvent` | Notifications (notify host) |
| Attendance | `AttendanceAcceptedIntegrationEvent` | Notifications, Announcements |
| Attendance | `AttendanceRejectedIntegrationEvent` | Notifications |
| Announcements | `AnnouncementCreatedIntegrationEvent` | Notifications |
| Ratings | `RatingCreatedIntegrationEvent` | Users (reputation read model), Notifications |

Total ~11 events. Each module subscribes to ≤ 5. Mental size: **manageable**.

### 4.3 “How nasty?” verdict

* **Compile-time fan-in** is dominated by `Users.Contracts` and `Posts.Contracts` – this is healthy, those *are* the core nouns of a geosocial app.
* **Runtime fan-out** is dominated by `Notifications` consuming most events – also healthy, that’s the whole point of a notifications module.
* No module reaches into another’s DB. No module references another’s Domain/Infrastructure/Application. The result is a sparse DAG with explicit seams.

---

## 5. Trade-offs of going Modular Monolith + Clean Architecture

### Pros
* **Future-proofs extraction to microservices** without a big-bang rewrite – a module can be lifted out by promoting its `Contracts` package and its in-process bus subscription to a real broker.
* **Independent reasoning**: a developer touching Attendance does not need to load Posts’ EF model.
* **Test pyramid becomes possible**: Domain → fast unit tests; Application → handler tests with in-memory test doubles; Infrastructure → Testcontainers; Api → WebApplicationFactory per module.
* **Enforceable boundaries** (NetArchTest) – violations break CI, not just code review.
* **Cross-cutting fixes** (auth, validation, transactions, outbox) become MediatR pipeline behaviours instead of copy-paste.
* **Outbox** removes today’s lost-notification class of bugs.
* **Per-schema migrations** make rollbacks safer.

### Cons / costs
* **More projects**. A 10-module modular monolith with 5 projects each is ~50 csproj files. We mitigate with templates and central package management.
* **Initial mapping overhead**: where today a controller calls a repo, tomorrow it sends a command and gets a `Result<T>`. More boilerplate, more clarity.
* **Eventual consistency**: things like “profile created on register” become async. We must handle the (rare) case where a user authenticates before their profile row exists. Solution: the Users handler is idempotent and the Identity flow can synchronously seed a minimal profile if needed.
* **Two test infrastructures**: per-module integration tests + cross-module E2E. More to maintain.
* **Outbox = additional table + background worker**. Worth it.
* **Strict isolation may require duplication**: e.g. each module that needs `UserId` will reference `Users.Contracts`; we accept that small duplication over big shared models.

### Things we explicitly do NOT change at first
* Still **one process**, one Postgres instance.
* Still **JWT + Identity**.
* Still **Redis** for response cache.
* Still **PostGIS**.

Microservices, Kafka, gRPC – not now.

---

## 6. Technology Choices (the explicit "should we add this?" list)

### 6.1 MediatR – **YES, adopt.**
* Use it inside each module as the in-process command/query/notification dispatcher.
* All cross-cutting concerns (validation, logging, transactions, outbox dispatch, response caching) become **pipeline behaviours** in `BuildingBlocks.Application`.
* Removes 80% of the controller boilerplate (auth checks, model state, error mapping), and is the natural place to publish domain events.

### 6.2 CQRS – **YES, but pragmatic CQRS.**
* Same DB, same DbContext per module, but **commands** mutate via aggregates, **queries** project directly to DTOs (Dapper or `EF.AsNoTracking().Select(...)`).
* No separate read store (yet). Read models for ratings/reputation can be plain projection tables maintained by event handlers.

### 6.3 Message broker – **NO at first, design for YES.**
* For v1 use **MediatR `INotification` + Outbox + a hosted `OutboxPublisher`** that, in-process, calls the same MediatR notifications. This already gives durability and retry.
* Define an `IIntegrationEventBus` abstraction now, with an `InProcessIntegrationEventBus` implementation. When real cross-process needs appear (e.g. Notifications becomes a separate worker, or images move to a dedicated service), swap to **MassTransit + RabbitMQ** or **AWS SNS/SQS** with a single composition-root change.
* **When a broker becomes valuable:**
  * Image upload pipeline (resize → S3 → thumbnail) → SQS + worker.
  * Push notifications → SQS so the API thread never waits on OneSignal.
  * Future fan-out (followers feed) → SNS/Kafka.

### 6.4 EF Core inheritance for `Event` – **migrate TPH → TPC** (or ditch the hierarchy).
* Today: TPH with a sparse table and 9 subclasses whose only behaviour is `GetSlotsIfAny()`. Most subclasses add nothing.
* Recommendation: **collapse to a single `Event` entity with an `EventType` enum and an optional `Slots` value**. Type-specific data (rare) goes into JSONB columns (Postgres native, EF supports `JsonDocument`/owned-entity-as-json).
* This kills the `GetType().Name == "HouseParty"` string comparisons in `PostRepository.FilterByType` and unbreaks pagination (filter goes into the SQL WHERE).

### 6.5 Time – **DateTimeOffset everywhere** (UTC).
* Stop storing Unix `Int64`. Add an `IDateTimeProvider` for testability. Provide a one-shot migration to convert columns.

### 6.6 Logging / observability – **Serilog + OpenTelemetry.**
* Structured logs to console (JSON) for container log collectors.
* OpenTelemetry traces + metrics, OTLP exporter (works with Jaeger, Tempo, Datadog, AppInsights, CloudWatch, …).
* Drop `ErrorService` + `ErrorDataContext` entirely. Errors become structured log events with a correlation id.

### 6.7 ProblemDetails – **YES.**
* Replace `SingleError`, `ErrorResponse`, `AuthFailedResponse` with RFC-7807 ProblemDetails (with custom extensions for `failReason`). Mobile client gets a single, predictable error shape.

### 6.8 Authorization – **policy-based + resource-based.**
* Replace `IsPostOwnerOrAdminAsync` ad-hoc calls with `IAuthorizationHandler<PostOwnerOrAdminRequirement, Post>`.
* Move "basic user can have only 1 active post" into a domain rule + a policy/validator, not a controller `if`.

### 6.9 AutoMapper / hand-rolled mappers – **drop AutoMapper, use Mapperly (source generator).**
* Source-generated, compile-time-checked, zero reflection, ~free at runtime. Replaces both AutoMapper Profiles and the `CustomMapper/*Mapper.cs` zoo.

### 6.10 JSON – **System.Text.Json.** Drop Newtonsoft.

### 6.11 Validation – **keep FluentValidation**, register validators per module assembly, and run them via the MediatR `ValidationBehavior`.

### 6.12 Caching – **rebuild the broken cache: keep Redis, two-layer strategy.**

> Today's `CachedAttribute` does not work properly: cache key is URL+QS only (ignores user/role/locale → personalised data leaks across users), no invalidation on writes, no per-key TTL strategy, no stampede protection, no metrics. Replace it.

* **Layer 1 – HTTP response cache (read endpoints only).**
  * Implement a thin `[Cached(seconds, varyByUser=true)]` action filter that:
    * Builds the key as `route + sortedQuery + (varyByUser ? userId : "anon") + acceptLanguage`.
    * Stores compressed JSON in Redis with the declared TTL.
    * Adds `Vary: Authorization, Accept-Language` and `ETag` headers.
    * Records hit/miss counters via OpenTelemetry.
* **Layer 2 – MediatR `CachingBehavior<TQuery, TResponse>` (target architecture).**
  * Triggered when a query handler implements `ICacheableQuery` (declares `CacheKey`, `Ttl`, `Tags`).
  * Stores the *handler result* (the DTO graph), not the HTTP response, so multiple endpoints hitting the same query share the cache.
  * Uses `IDistributedCache` (Redis) backed by `Microsoft.Extensions.Caching.StackExchangeRedis`.
  * **Stampede protection:** a per-key in-process `SemaphoreSlim` (`StackExchange.Redis` `LockTakeAsync` for cross-instance) so only one handler runs on a cold miss.
* **Invalidation – tag-based.**
  * Every `ICacheableQuery` declares cache tags (e.g. `["post:123", "user:456:feed"]`).
  * Command handlers publish a `CacheInvalidationRequest` after commit (in the same outbox) listing the tags to evict; an `InvalidateCacheJob` (Hangfire) clears all keys for those tags via a Redis SET-of-keys-per-tag pattern.
  * Replaces the current "no invalidation, just hope the TTL expires" behaviour.
* **What gets cached at start:**
  * `GetNearbyPostsQuery` – TTL 30s, varied by `(userId, lat-rounded, lng-rounded, radius, page)`.
  * `GetPostByIdQuery` – TTL 60s, tag `post:{id}`, invalidated on update/delete/attendance change.
  * `GetUserProfileQuery` – TTL 5min, tag `user:{id}`, invalidated on profile update.
  * `GetUserAverageRatingQuery` – TTL 5min, tag `user:{id}:rating`, invalidated on new rating.
  * Static lookup data (tag list, event types) – TTL 1h.
* **What is NEVER cached:** anything write, anything personalised that doesn't include `userId` in the key, anything returning more than the user is allowed to see.
* **Observability:** expose `cache_hits_total{name=...}` / `cache_misses_total` / `cache_evictions_total` metrics; ship a Grafana dashboard tile during Phase 0.

### 6.12.1 Domain-Driven Design – **YES, light DDD per module.**

> The plan was already trending toward DDD (aggregates, value objects, domain events). Making it explicit removes ambiguity and gives every developer the same vocabulary.

* **Strategic DDD applied to module boundaries.** Each module is a **Bounded Context**: it owns its language, its model, its schema. Cross-context communication uses **integration events** (translation between contexts). The "module catalog" (§2) is the **context map**.
* **Tactical DDD inside `*.Domain`:**
  * **Aggregate Roots** – `Post`, `Participation`, `Announcement`, `Rating`, `Bug`, `Report`, `UserProfile`, `User` (identity). Only the root has a public constructor; child entities are mutated through root methods to keep invariants.
  * **Entities vs Value Objects** – `Coordinates`, `TimeRange`, `Money`, `Address`, `Slots`, `EmailAddress`, `PhoneNumber` are immutable value objects in `Hopaut.SharedKernel` or per-module domain. No more anaemic POCOs with public setters everywhere.
  * **Strongly-typed ids** – `PostId`, `RatingId`, `UserId`, `AnnouncementId` (record struct over `Guid`/`int`) – kills accidental `int → int` parameter swaps.
  * **Domain Events** – aggregates raise `IDomainEvent`s on state changes (e.g. `PostCreated`, `AttendanceAccepted`); `UnitOfWorkBehavior` dispatches them after commit. Distinct from **integration events** (cross-context, durable, outbox).
  * **Domain Services** – stateless services for logic that doesn't belong on a single aggregate (e.g. `PostSlotPolicy` – "basic users may have one active post").
  * **Specifications** – encapsulated query/business rules (`ActivePostsForUserSpec`, `EventInRadiusSpec`).
  * **Repositories** – return aggregate roots only, expose intention-revealing methods (`AddAsync`, `GetByIdAsync`, `RemoveAsync`); never `IQueryable<Post>` from a repo. Read-side queries bypass repos and project directly.
  * **Factories** – `Post.Create(host, request, clock)` is a static factory that enforces invariants instead of letting the constructor be called with half-filled state.
* **Anti-corruption layers.** When a module needs data from another (e.g. Posts needs `UserSummaryDto` from Users), it does **not** import the foreign aggregate; it consumes the foreign module's `*.Contracts` DTO (anti-corruption). If the foreign shape changes, only the contract translation breaks.
* **Pragmatism guard-rails.** We do not adopt event sourcing, do not introduce CQRS read stores beyond projection tables, do not require every operation to go through a domain event. DDD here is *strategic boundaries + clean aggregates* — not academic ceremony.

### 6.13 S3 / image pipeline – **YES, async via Hangfire (decision finalised).**
* The HTTP request never blocks on image processing. Flow:
  1. Mobile uploads image bytes as `multipart/form-data` to `POST /posts`.
  2. `CreatePostCommandHandler` writes the post + `Picture` rows in **`Pending`** state, uploads the **raw bytes to S3 under a `tmp/` prefix** (claim-check pattern – cheap, fast, ~200ms), returns `201 { postId, pictures: [...pending] }` immediately.
  3. Handler enqueues `BackgroundJob.Enqueue<IMediaProcessor>(p => p.ProcessAsync(postId, tempKeys, CancellationToken.None))`.
  4. Hangfire worker picks it up, downloads from `tmp/`, runs **ImageSharp** (resize / compress / thumbnails), writes finals to `posts/{postId}/...`, updates `Picture` rows to `Ready`, deletes `tmp/` keys, publishes `MediaProcessedIntegrationEvent` via outbox.
  5. Mobile polls `GET /posts/{id}` (or subscribes to a push) for `pictures.state == "Ready"`.
* **Why Hangfire and not a broker?** Zero new infra (jobs persisted in our existing Postgres in schema `hangfire`), built-in retries with exponential backoff, dashboard for ops, easily idempotent via the job id. A real broker would force a separate consumer service which is overkill for one-deploy modular monolith. The `IIntegrationEventBus` abstraction stays so a broker can be swapped in later without touching handlers.
* `System.Drawing.Common` (currently referenced) is **Windows-only** since .NET 6 – replaced with `SixLabors.ImageSharp`.

### 6.13.1 Hangfire – background jobs
* **Storage:** Postgres (`Hangfire.PostgreSql`), schema `hangfire`. No new container.
* **Server:** hosted in `Hopaut.Api.Host` for now (`AddHangfireServer()` with worker count tuned per env). Can be extracted to a dedicated `Hopaut.Worker` host later by removing the `AddHangfireServer()` call from the API host.
* **Dashboard:** mounted at `/admin/hangfire`, gated by an `AdminOnly` authorization filter (uses the same JWT + Admin role).
* **Patterns:**
  * `BackgroundJob.Enqueue<TJob>(...)` – fire-and-forget (image processing).
  * `RecurringJob.AddOrUpdate<TJob>(...)` – scheduled (expired-post deactivation, cleanup of `tmp/` orphans).
  * Jobs are thin: they just `IMediator.Send(new XxxCommand(...))`. **Business logic stays in MediatR handlers.**
* **Idempotency:** every job receives a deterministic id (e.g. `"process-post-images:{postId}"` via `BackgroundJob.Enqueue` with a unique constraint check) and the handler is safe to re-run. The `Picture.State` machine (`Pending → Processing → Ready` / `Failed`) makes retries safe.
* **Use cases (at start):**
  * `ProcessPostImagesJob` (Phase 3) – the one above.
  * `ProcessProfilePictureJob` (Phase 2) – same pattern for avatars.
  * `DeleteS3ObjectsJob` (Phase 3) – consumes `PostDeletedIntegrationEvent`.
  * `SendPushNotificationJob` (Phase 5) – outbox-driven; OneSignal call lives in a job, not in a request thread.
  * `SendEmailJob` (Phase 5) – MailKit/SES via job, not in the register flow.
  * `DeactivateExpiredPostsJob` (recurring, every 5 min) – replaces today's `ActiveFlag` time math at request time.

### 6.14 Background work – **`IHostedService` / `BackgroundService`** for: Outbox publisher, Inbox consumer, role seeder (replaces the seeding code in `Program.Main`), expired-post deactivation (today probably done by `ActiveFlag` and time math).

### 6.15 Health checks – **YES.** `AddHealthChecks().AddNpgSql().AddRedis().AddS3()`.

### 6.16 Testing – **Testcontainers for Postgres/Redis/LocalStack-S3** + xUnit v3 + AwesomeAssertions. One integration project per module; one E2E project for cross-module flows.

### 6.17 Architectural enforcement – **NetArchTest** rules in CI: no module references another module’s non-Contracts project; Domain has no infra refs; Application has no ASP.NET refs.

### 6.18 Configuration / secrets – **Azure Key Vault or AWS Secrets Manager** wired through `IConfiguration` providers, depending on host. Local dev uses User Secrets.

### 6.19 .NET runtime – **upgrade to .NET 10** (or current LTS). All projects on the same TFM. Adopt minimal hosting (`WebApplication.CreateBuilder`).

### 6.20 Container / deploy – Dockerfile per host (`Hopaut.Api.Host`), and per worker (`Hopaut.Worker.Outbox`, `Hopaut.Worker.Media` when split). Compose file with Postgres + Redis + LocalStack for local dev.

---

## 7. Things to ADD

* **Hangfire (Postgres storage)** for background jobs: image processing, S3 cleanup, push/email send, recurring deactivation, cache invalidation. Dashboard at `/admin/hangfire` (Admin only).
* **Working caching layer** – two-layer (HTTP filter + MediatR `CachingBehavior`) on Redis with **tag-based invalidation**, stampede protection, and metrics. Replaces the broken `CachedAttribute`.
* **Outbox / Inbox** tables and a `BackgroundService` publisher.
* **Idempotency middleware** on POST endpoints (header `Idempotency-Key`).
* **Correlation id** middleware that flows into Serilog and outgoing HTTP / SQS messages.
* **Rate limiting** via the built-in `Microsoft.AspNetCore.RateLimiting` (replaces AspNetCoreRateLimit).
* **Response caching policy** + Redis distributed cache via the `CachingBehavior`.
* **Domain events** inside aggregates, dispatched by the `UnitOfWorkBehavior` after commit.
* **Resource-based auth** for Post / Announcement / Rating ownership.
* **Sentry-style error reporting** via Serilog sink (no more `ErrorLog` table).
* **API versioning** package (`Asp.Versioning`) – the URL already says `/v1`, formalize it.
* **`Asp.Versioning` + `Swashbuckle`** documenting v1 (and prepping v2 for breaking DTO changes).
* **OpenAPI client codegen** for the Flutter app – ship `swagger.json` to a known URL on each release.
* **Feature flags** (`Microsoft.FeatureManagement`) – useful for ramping the new endpoints during migration.

## 8. Things to REMOVE / REPLACE

* `ErrorService`, `ErrorDataContext`, `ErrorLog`, `ErrorController`, `ErrorHandlingMiddleware` (current impl). Replaced by Serilog + ProblemDetails middleware.
* `IRepository<T>` + per-entity `IXxxRepository` generic abstraction. Replaced by aggregate-specific repositories whose interfaces live in `Application` and whose implementation lives in `Infrastructure`. Some queries become direct `IQueryable` projections inside query handlers; no need for a "repository" wrapper for read paths.
* The retry-loop + exception-swallowing in `PostRepository`. Replaced by an upsert-by-name strategy on `Tag` (or a unique constraint with a single `ON CONFLICT DO NOTHING` upsert via a raw SQL command) and explicit error handling.
* `RoleCheckingHelper` static. Replaced by an `IUserContext` + claim checks in policies.
* `UpdatedPostDetailsWatcher`. Logic moves into the `UpdatePostCommandHandler` and emits `PostUpdatedIntegrationEvent` with `ChangedFields`.
* AutoMapper. Replaced by Mapperly per module.
* Newtonsoft.Json. Replaced by System.Text.Json.
* `System.Drawing.Common`. Replaced by `SixLabors.ImageSharp`.
* AspNetCoreRateLimit. Replaced by ASP.NET Core built-in.
* Hardcoded WebPortal URLs. Move to typed options bound from configuration.
* `EnvironmentOptions.Environment` int (0/1/2) – use `IHostEnvironment` properly.
* `Bingo.LoadTests` empty project. Either delete or rebuild on **NBomber** / **k6**.
* `Bingo.Contracts/MyServicesRoutes` mixed with public contracts – move to the Payments module.

---

## 9. Migration Roadmap (phased, low-risk)

The plan is to migrate **strangler-fig style**: stand up the new host, move modules one at a time, keep old controllers running until each module has parity, then delete.

### Phase 0 – Foundation (1–2 weeks)
1. Add `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`, nullable on, treat warnings as errors.
2. Upgrade all projects to **.NET 10** (or current LTS). Fix breaking changes.
3. Replace `System.Drawing.Common` with ImageSharp.
4. Replace AspNetCoreRateLimit with built-in rate limiting.
5. Replace error logging with **Serilog + ProblemDetails middleware**. Delete `ErrorDataContext` and the `ErrorLog` table (after archiving).
6. Add **OpenTelemetry**, health checks, correlation id middleware.
7. Write **NetArchTest** baseline (initially permissive, will tighten per module).

### Phase 1 – Building blocks & composition root (1 week)
1. Create `Hopaut.SharedKernel`, `Hopaut.BuildingBlocks.Application`, `Hopaut.BuildingBlocks.Infrastructure`, `Hopaut.BuildingBlocks.Api`, `Hopaut.IntegrationEvents`, `Hopaut.Api.Host`.
2. Wire MediatR with `ValidationBehavior`, `LoggingBehavior`, `UnitOfWorkBehavior`, `CachingBehavior`.
3. Implement `OutboxMessage` + `OutboxPublisher` `BackgroundService` (in-process bus implementation).
4. **Wire Hangfire** (`Hangfire.PostgreSql`, schema `hangfire`) + dashboard at `/admin/hangfire` with admin-only filter.
5. **Rebuild caching:** distributed Redis cache abstraction, `CachingBehavior<TQuery,TResponse>` with tag-based invalidation, stampede lock; the new `[Cached]` action filter for legacy endpoints during transition.
6. Move `Program.Main` role-seeding into a `RoleSeederHostedService`.
7. The old `BingoAPI` project keeps running side-by-side as legacy host; new endpoints will be added to `Hopaut.Api.Host`.

### Phase 2 – Identity + Users (2 weeks)
1. Create `Identity` module. Move `IdentityService`, `FacebookAuthService`, `IdentityController`, `RefreshToken`. Schema → `identity`.
2. Split `AppUser` profile fields (`FirstName`, `LastName`, `ProfilePicture`, `Description`, `RegistrationTimeStamp`) into the `Users` module under schema `users`. Migration copies data.
3. Identity emits `UserRegisteredIntegrationEvent`; Users handler creates profile.
4. Replace JWT validation with strict settings (`ValidateIssuer`, `ValidateAudience`, `RequireExpirationTime`).
5. Move WebPortal URLs to typed options.
6. Old controllers proxied/removed.

### Phase 3 – Posts + Media (3 weeks)
1. Create `Posts` module under schema `posts`. Migrate entities and PostGIS configuration.
2. Collapse Event TPH to `Event` + `EventType` enum (+ JSONB for type-specific data).
3. Implement `GetNearbyPostsQuery` with proper SQL filtering (no in-memory `RemoveAll`). Pagination uses cursor or correct skip/take.
4. Create `Media` module; move `AwsBucketManager` + `ImageLoader`. S3 client becomes a typed singleton (`AddAWSService<IAmazonS3>()`).
5. Picture upload during create-post still synchronous via `IMediaUploader` port.
6. `PostDeletedIntegrationEvent` triggers Media to delete S3 objects via outbox.

### Phase 4 – Attendance + Announcements + Ratings (2 weeks)
1. `Attendance` module under schema `attendance`. Move `Participation`, `EventAttendanceRepository`, `EventParticipantsRepository` logic into command handlers.
2. `Announcements` module under schema `announcements`.
3. `Ratings` module under schema `ratings`. Build a `UserReputation` projection table updated by `RatingCreatedIntegrationEvent`.
4. Posts feed query joins reputation in a single SQL query – fixes the N+1.

### Phase 5 – Notifications + Moderation + BugReports + Payments (2 weeks)
1. `Notifications` module: subscribes to integration events, sends OneSignal/email asynchronously via outbox-driven worker.
2. Replace `SmtpClient` with **MailKit** (SmtpClient is obsolete) or a transactional email provider (SES/SendGrid). Wire via configuration.
3. `Moderation` module under schema `moderation`.
4. `BugReports` module under schema `bug_reports`.
5. `Payments` module: keep thin proxy; isolate `MyServicesRoutes`.

### Phase 6 – Cleanup (1 week)
1. Delete old `BingoAPI` project after parity is verified by E2E tests.
2. Tighten NetArchTest rules to forbid all cross-module non-Contracts references.
3. Run a full performance pass with profiling on the feed endpoint and post create/update.
4. Decide whether to extract `Notifications` and `Media` into separate worker processes (the contracts and outbox already make this trivial).

Total: **~10–12 weeks** of focused work for one experienced developer, faster with two.

---

## 10. Concrete Anti-pattern → Fix Cheatsheet

| Today | Tomorrow |
|---|---|
| Controller orchestrates user check + repo + S3 + notifications | Controller is 5 lines: bind request → `mediator.Send(command)` → map `Result` to HTTP |
| `IRepository<T>.AddAsync` returns `bool` | `Result<TId>` from a command handler, with explicit error codes |
| `Int64` Unix seconds + `15778476` magic number | `DateTimeOffset` + `IDateTimeProvider` + `TimeRange` value object |
| `GetType().Name == "HouseParty"` filter | `Where(e => e.EventType == EventType.HouseParty)` in SQL |
| `try { ... } catch { tryAgain = true; }` | Single upsert SQL or explicit handled `DbUpdateException` per case |
| `IsPostOwnerOrAdminAsync` called in controller | `[Authorize(Policy="PostOwnerOrAdmin")]` resource-based policy |
| `_errorService.AddErrorAsync(new ErrorLog{...})` | `_logger.LogError(ex, "...")` + structured properties |
| `await SmtpClient.SendMailAsync` inside register flow | `UserRegisteredIntegrationEvent` → outbox → Notifications worker → MailKit |
| Two DbContexts both Identity-based | One DbContext per module; Identity owns Identity tables, others own their own schemas |
| AutoMapper Profile + hand-rolled mapper for the same DTO | Mapperly partial method per module |
| `Bingo.Contracts` references the API entities indirectly | `<Module>.Contracts` projects per module, only DTOs + integration events |
| 9 `Installers` registering everything for the monolith | Each module exposes `services.AddPostsModule(configuration)` extension; host calls them all |
| `Newtonsoft.Json` payload built ad-hoc for OneSignal | Strongly-typed `OneSignalNotificationRequest` record + `System.Text.Json` |

---

## 11. Risks of the Refactor & Mitigations

| Risk | Mitigation |
|---|---|
| Big-bang rewrite | Strangler-fig: new host runs side by side; route traffic per controller as each module reaches parity. |
| Mobile app contract breakage | Keep `/api/v1` URLs identical until v2; ProblemDetails added as a *superset* of the current shapes during transition. |
| Data migration (Identity split, time conversion, schema move) | Each migration ships as an idempotent EF migration + a one-shot data copy script, gated by a feature flag. Backfill in batches. |
| Outbox doubles writes during migration | Acceptable; deduplicate at consumer with Inbox table. |
| Team unfamiliar with MediatR / CQRS | Pair-programming + a sample module (Identity) used as the template. |
| Test infrastructure drift | Standardize Testcontainers fixtures in `BuildingBlocks.Tests` once. |

---

## 12. Open Questions (require product input)

1. Is the Web Portal still in scope (the hardcoded `localhost:3000` suggests yes)? Affects auth flows and CORS.
2. Will Payments grow beyond a proxy? If yes, it deserves its own DB and integration events.
3. Real volume / read-write ratio of the “near-me” feed – decides whether we need a denormalized read model in v1.
4. Is OneSignal locked-in or can we move to FCM/APNs directly? Affects the Notifications adapter.
5. Hosting target: AWS (S3 already), Azure, on-prem? Affects Secrets, queueing (SQS vs Service Bus vs RabbitMQ), and observability backend.

---

## 13. Definition of Done for the Refactor

* All projects target the same supported .NET LTS.
* No project named `BingoAPI` exists.
* Each module has Domain / Application / Infrastructure / Api / Contracts and ≥ 70% Domain+Application unit-test coverage.
* No cross-module reference except to `*.Contracts`. Verified by NetArchTest in CI.
* All write endpoints go through MediatR commands with the standard pipeline.
* Outbox is the only path for integration events.
* No `Int64` time fields in the domain.
* No `ErrorLog` table.
* Logs are structured JSON with correlation id; traces visible in OTLP backend.
* Health endpoints `/health/live`, `/health/ready` are green in production.
* The mobile app continues to work against `/api/v1` without changes.

---

This is the plan. The accompanying `Hopaut-Backend-State.md` justifies every move with a concrete reference into today’s code.
