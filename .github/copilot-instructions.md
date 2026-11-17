# Copilot Instructions – Hopaut Backend (BingoAPI)

At the start of every new conversation, explicitly mention the context you are using (files, configuration, environment) before answering. When writing production code, follow **Production code**. When writing tests, follow **Test code**. When refactoring toward the target architecture, follow **Target architecture rules**.

> Companion documents:
> * `Hopaut-Backend-State.md` – current ("as-is") architecture and full list of flaws.
> * `hopaut-solution-fix.md` – target Modular Monolith + Clean Architecture plan.
> * `hopaut-execution-plan.md` – ordered phases / steps.
> * `hopaut-migration-status.md` – persistent progress tracker (must be updated after every prompt).
> * `docs/mobile-handoffs/` – one markdown prompt per backend change that affects the public HTTP/SignalR/auth contract; consumed by the sibling `BingoMobile` repo. See *Backend ↔ Mobile contract sync* below.

---

## General Rules

**IMPORTANT:** Do **NOT** build the solution or projects automatically after every prompt. Only build when the user explicitly asks.

**IMPORTANT:** Always read specialised instruction files first when present:

- All files in `/.github/instructions/*.instructions.md` contain task-specific rules.

If the task touches a specialised area, read the corresponding instruction file **FIRST** before generating code.

## Migration Workflow (mandatory when working on the modular-monolith / .NET 10 refactor)

The refactor is governed by three persistent files at the repo root:

1. `.github/github-copilot-instructions.md` – this file (behavioral rules).
2. `hopaut-execution-plan.md` – ordered phases and steps (the **plan**).
3. `hopaut-migration-status.md` – persistent progress tracker (the **state**).

### Status files — exact disk paths

| File | Absolute path |
|------|--------------|
| **Backend status** | `C:\Users\ghmi\personal\BingoAPI\hopaut-migration-status.md` |
| **Mobile status** | `C:\Users\ghmi\personal\BingoMobile\mobile-migration-status.md` |

### Rules

* Resume work by attaching all three files to the chat. The user will typically say `/migrate #hopaut-execution-plan.md`.
* Always read `hopaut-migration-status.md` first to determine the next pending step. Do **one step at a time** unless the user explicitly approves a batch.
* Follow the step exactly as defined in `hopaut-execution-plan.md` (Goal / Actions / Acceptance criteria).

### ⚠️ MANDATORY AFTER EVERY PROMPT — NO EXCEPTIONS

**After _any_ prompt that produces _any_ of the following, you MUST update the relevant status file(s) before the response ends:**

| What changed | Which file(s) to update |
|---|---|
| Any source code file added, edited, or deleted | `hopaut-migration-status.md` |
| Any configuration file added, edited, or deleted (`.csproj`, `appsettings`, `props`, `json`, `yaml`) | `hopaut-migration-status.md` |
| Any migration documentation file edited (`hopaut-execution-plan.md`, `hopaut-solution-fix.md`, etc.) | `hopaut-migration-status.md` |
| Any public API/contract change (route, DTO, auth, error envelope, headers) | `hopaut-migration-status.md` **+** `C:\Users\ghmi\personal\BingoMobile\mobile-migration-status.md` **+** a new file under `docs/mobile-handoffs/` |
| Any mobile documentation file edited (`mobile-solution-fix.md`, etc.) — *when working in the mobile workspace* | `C:\Users\ghmi\personal\BingoMobile\mobile-migration-status.md` |

**This rule applies to the very smallest change** — one-line config edit, a single renamed field, adding one NuGet package, renaming a file. If _anything_ changed, the status file changes too.

### How to update the status file

For **every** qualifying prompt, perform **all four** of these actions in `hopaut-migration-status.md`:

1. **Flip the step status** in the phase table:
   * `⬜` → `🟡` when work starts but is not finished.
   * `🟡` → `✅` when the Acceptance criteria are met (or explicitly waived by the user).
   * `🟡` / `⬜` → `❌ <reason>` if blocked.
   * `⬜` → `⏭️ <reason>` if intentionally skipped with user approval.
2. **Fill in the Date column** (`YYYY-MM-DD`, UTC).
3. **Fill in the Notes / commit column** — one line: what was done and the git commit hash if available (write `–` if not yet committed).
4. **Prepend a new row to the Activity log table** at the bottom of the file:

   ```
   | YYYY-MM-DD | <step id or –> | <one-line description of what changed> | <commit hash or –> |
   ```

If the mobile status file also needs updating (contract change, or you are working in the mobile workspace), apply the same four actions there too, then confirm both files were updated at the end of your response.

### When step cannot complete in one prompt

Mark the step `🟡` in progress. In the Notes column write: `in progress – <what remains>`. Return and finish it in the next prompt.

### Never

* Invent new phases/steps without proposing them to the user first. If approved, update `hopaut-execution-plan.md` **and** `hopaut-migration-status.md` in the same change.
* Tick a step `✅` unless its Acceptance criteria are demonstrably met or the user explicitly waives them.
* Skip updating the status file, even for trivial changes.


## Backend ↔ Mobile contract sync (mandatory)

The Hopaut Flutter app (`BingoMobile`, sibling repo at `..\BingoMobile`) is the **only** consumer of this API. Any backend change that alters the *observable contract* between server and mobile **must** generate a handoff prompt file so the mobile assistant can apply matching changes on its side.

### What counts as a contract change (triggers a handoff)

Anything the mobile app can see over the wire or in its auth/storage assumptions, including:

* Routes added, removed, renamed, or re-versioned (anything in `Bingo.Contracts/V1/ApiRoutes.cs` or future per-module endpoints).
* Request or response DTO shape changes (fields added/removed/renamed/retyped, nullability, enum value changes, union/discriminator changes) in `Bingo.Contracts` or any module's `Contracts`/`Api`.
* Status code / error envelope changes (e.g. legacy `SingleError` → RFC 7807 `ProblemDetails`, new error codes, new `Result` failure codes surfaced as HTTP).
* Auth changes: JWT issuer/audience, claim names, token lifetime, refresh-token endpoint behaviour, Facebook / OTP / external-login flow.
* Pagination / sorting / filtering query-param changes (e.g. cursor instead of offset, new spatial filter args).
* Time representation changes (`Int64` Unix seconds → `DateTimeOffset` ISO-8601), units (meters/km), or coordinate SRID/format.
* Multipart upload contract changes (field names, allowed content types, async picture lifecycle `Pending → Ready` once Phase 3 ships).
* Push / notification payload schema changes (OneSignal `data` payload).
* Caching headers / `ETag` / `Vary` semantics that the client must respect.
* Rate-limit / throttling header or status changes.

If you are unsure whether a change is contract-affecting, **assume it is** and write the handoff.

### What does NOT trigger a handoff

* Internal refactors with no observable diff (renaming a service, moving a class between projects, swapping a mapper, EF migration that does not change the JSON output).
* Test-only changes.
* Documentation/status-tracker updates.
* Performance work that preserves shape and semantics.

### How to write the handoff

1. **Create one file per change** under `docs/mobile-handoffs/` named:
   `YYYY-MM-DD-<phase>-<step>-<short-slug>.md`
   (e.g. `2025-01-15-phase3-step3.5-async-picture-pipeline.md`).
2. Use the template at `docs/mobile-handoffs/_TEMPLATE.md`. It is structured as a **drop-in prompt** the user can paste into the mobile workspace chat — the first line is `# Mobile handoff prompt` and the body is written in the second person, addressing the mobile assistant.
3. Required sections:
   * **Source change** – backend commit hash (or branch + step id from `hopaut-migration-status.md`) and which Phase/Step in `hopaut-execution-plan.md` produced it.
   * **Affected endpoints** – HTTP method + route + summary of change (added / changed / removed / deprecated). Reference exact route constants from `Bingo.Contracts/V1/ApiRoutes.cs` (or the new module `Api` project).
   * **Wire-format diff** – before/after JSON snippets for each changed DTO. Call out renamed fields, type changes, nullability, enum changes.
   * **Behavioural diff** – status codes, error envelope, pagination, auth, headers, retry semantics.
   * **Backwards compatibility window** – is the old shape still served? For how long? Any feature flag / `Accept: application/vnd.hopaut.v2+json` toggle?
   * **Required mobile changes** – concrete file paths in `BingoMobile` to update (DTOs in `lib/data/models/` today, future `features/<x>/data/`), interceptors, routing, push handlers, etc. Cross-reference the relevant Phase/Step in the mobile `mobile-solution-fix.md` and `mobile-migration-status.md`.
   * **Suggested mobile validation** – which screens to smoke-test, which integration tests to run.
   * **Status** – `pending` until the user confirms the mobile prompt was applied; then flip to `applied YYYY-MM-DD` with the mobile commit hash.
4. **Update `hopaut-migration-status.md`** in the same change:
   * Add a row to the *Activity log* mentioning the handoff filename.
   * If the step that produced the change has a *Mobile handoff* note column, fill it.
5. **Never ship a contract-affecting backend change without writing the handoff in the same prompt.** If you forget, the next prompt's first action is to write it before doing anything else.

### Cross-repo conventions to keep stable (so handoffs are mechanical)

* Routes stay declared as constants in `Bingo.Contracts/V1/ApiRoutes.cs` (legacy) or in the per-module `Api` project (target). Do not inline route literals.
* Wire DTOs live in `Bingo.Contracts` (legacy) or per-module `Contracts`/`Api` (target). The mobile app mirrors them in `lib/data/models/` (legacy) and `features/<x>/data/dto/` (target). Field names in JSON must remain `camelCase` unless explicitly versioned.
* Time fields: today `Int64` Unix seconds, target `DateTimeOffset` (UTC, ISO-8601). The switchover is Phase 4 / Step 4.9 on mobile and ships paired with the matching backend module migration; produce a handoff for each module that flips.
* Error envelope: today `SingleError` for legacy endpoints, `ProblemDetails` for new endpoints. Document which envelope a route uses in the handoff.
* Auth claims / JWT shape changes always require a handoff (mobile parses `sub`, role claims, expiry; refresh-token endpoint shape is consumed by `DioService` / future `DioClient`).

---

# Project Context

* **Product:** Hopaut – a geosocial networking back-end for a Flutter mobile app (events near me, attendance, ratings, announcements, moderation).
* **Repo:** `BingoAPI` (legacy name, kept on disk; logical product name is Hopaut).
* **Current solution:**
  * `BingoAPI` – ASP.NET Core Web API (the monolith).
  * `Bingo.Contracts` – shared DTOs / `ApiRoutes`.
  * `Bingo.IntegrationTests` – xUnit + WebApplicationFactory.
  * `Bingo.LoadTests` – placeholder.
* **Current TFMs:** `net5.0` for API/Contracts/IntegrationTests, `netcoreapp3.1` for LoadTests. Both **out of support** – migrating to current LTS.
* **Persistence:** PostgreSQL + PostGIS via Npgsql + `NetTopologySuite`. EF Core 5.
* **Identity:** ASP.NET Identity + JWT bearer + Facebook external login + refresh tokens.
* **External systems:** AWS S3 (post / profile pictures), OneSignal (push), SMTP (transactional e-mail), Facebook Graph (auth), MyServices (payments gateway proxy).
* **Cross-cutting:** Redis response cache (`CachedAttribute`), AspNetCoreRateLimit, FluentValidation + global `ValidationFilter`, AutoMapper + hand-rolled mappers, `IInstaller` convention for DI registration, custom `ErrorHandlingMiddleware` writing to a separate `ErrorDataContext`.

---

# Production code

## Today's stack (use what exists; do not add new patterns ad-hoc)

* C# 9 / .NET 5 / ASP.NET Core MVC controllers (until the migration to LTS).
* EF Core 5 with `DataContext : IdentityDbContext` and TPH for `Event`.
* AutoMapper 10 + custom `IXxxMapper` classes under `BingoAPI/CustomMapper/`.
* FluentValidation 9, Newtonsoft.Json (still pinned by MVC).
* DI registrations live in `BingoAPI/Installers/*` via the `IInstaller` interface; do not add registrations in `Startup.ConfigureServices` directly – add or extend an installer.
* Strongly-typed options bound from `appsettings.json` and the JSON files under `wwwroot/Configurations`, `wwwroot/EmailTemplates`, `wwwroot/NotificationTemplates`. New configuration goes through `IOptions<T>`.
* All controllers inherit `Controller` and use attribute routing; routes are constants in `Bingo.Contracts/V1/ApiRoutes.cs` – never hardcode route strings.
* All request/response DTOs live under `Bingo.Contracts/V1/Requests` and `Bingo.Contracts/V1/Responses`; never put DTOs inside `BingoAPI`.

## Naming conventions (current code)

* Controllers: `XxxController` ending; route constants under `ApiRoutes.<Area>.<Action>`.
* Repositories: `IXxxRepository` + `XxxRepository` under `BingoAPI/Models/SqlRepository/`.
* Services: `IXxxService` + `XxxService` under `BingoAPI/Services/`.
* Entities: under `BingoAPI/Models/`. Do **not** mix new entities with non-EF classes.
* Async methods always suffix with `Async`.
* Result/filter/query DTOs that are **not** wire DTOs go under `BingoAPI/Domain/`.

## Hard rules for new code in the legacy project

1. **No new business logic in controllers.** Add it to a service under `BingoAPI/Services/` or, when refactoring an area, into the matching new module (see Target architecture).
2. **Do not add new exception-swallowing retry loops.** Use targeted `catch` per exception type, log via `ILogger<T>` (not `IErrorService`), and rethrow when the operation cannot be completed.
3. **Do not add new code that depends on `IErrorService` / `ErrorDataContext` / `ErrorLog`.** Those are scheduled for removal. Use `ILogger<T>` instead.
4. **Do not introduce new `DateTime`/`Int64` time math.** New time-bearing fields use `DateTimeOffset` (UTC). When touching existing `Int64` fields, add a TODO comment referencing the migration plan.
5. **Do not call `_postRepository.AddAsync(post)` and discard the result.** Translate failure into a meaningful HTTP response.
6. **Do not call `UserManager<AppUser>` from a repository.** If a repository needs a user id, take it as a parameter.
7. **Do not call external systems (S3, OneSignal, SMTP) inside a `try { … } catch { return; }` that hides the failure.** Surface failures to the caller or queue the work for retry.

## ASP.NET Core conventions in this codebase

* Controllers must declare `[Produces("application/json")]` and `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "...")]` where applicable; copy the pattern of `PostController` / `EventAttendeesController`.
* Use `HttpContext.GetUserId()` (extension in `BingoAPI/Extensions/GeneralExtensions.cs`) to read the authenticated user id; do not parse claims inline.
* Validation: write a `FluentValidation.AbstractValidator<TRequest>` under `BingoAPI/Validators/`. The global `ValidationFilter` will short-circuit on invalid models.
* Caching: annotate read endpoints with `[Cached(seconds)]`. Caching is keyed off URL + query.
* Mapping: prefer the existing `IDomainToResponseMapper` / `IRequestToDomainMapper` / `ICreatePostRequestMapper` / `IUpdatePostToDomain` for post-related mappings. Otherwise, AutoMapper. Do not introduce a third mapping mechanism.

## Data access (current pattern – constrained)

* Inject `DataContext` only into repositories and into `IdentityService` (existing exception). Do not inject `DataContext` into new services or controllers.
* Repositories live in `BingoAPI/Models/SqlRepository/`. Each new repo implements its own interface; do not extend `IRepository<T>` for new aggregates – it is on the deprecation list.
* Always include needed navigation properties via `Include` explicitly. Use `AsNoTracking()` for read-only queries.
* For new feed/list endpoints push **filtering and pagination into the SQL query**. In-memory `RemoveAll`/`Skip`/`Take` after the query is forbidden.
* Spatial filtering uses `NetTopologySuite.Geometries.Point` with `SRID = 4326` and `IsWithinDistance(location, meters)`.

## Errors and responses

* For new endpoints, use `Microsoft.AspNetCore.Mvc.ProblemDetails` (or the existing `SingleError` for parity with mobile clients in legacy endpoints). Do not invent new error envelopes.
* Use `Forbid()` instead of `StatusCode(StatusCodes.Status403Forbidden, …)` when the user is authenticated but not authorized.

## Logging

* Inject `ILogger<T>`. Use structured properties (`logger.LogWarning("Post {PostId} not found for user {UserId}", postId, userId)`).
* Never write to `Console.WriteLine`. Never persist log entries via `IErrorService` in new code.

---

# Target architecture rules (apply when creating *new modules* / refactoring an area)

These rules are mandatory for any code created under `src/Modules/<X>/` once the new solution layout (see `hopaut-solution-fix.md`) is in place.

## Module layout

```
Modules/<X>/
├── Hopaut.Modules.<X>.Domain          # entities, value objects, domain events. NO EF / NO ASP.NET refs.
├── Hopaut.Modules.<X>.Application     # MediatR commands/queries, validators, ports (interfaces).
├── Hopaut.Modules.<X>.Infrastructure  # EF DbContext + configurations, repositories, external adapters, migrations.
├── Hopaut.Modules.<X>.Api             # endpoints, authorization policies, request/response models.
└── Hopaut.Modules.<X>.Contracts       # PUBLIC integration events + DTOs other modules can consume.
```

Allowed dependencies inside a module: `Api → Application → Domain`, `Infrastructure → Application → Domain`. Cross-module references are allowed **only** to another module's `Contracts` project. Enforced by NetArchTest.

## CQRS via MediatR

* Every state change is a `Command` with a handler. Every read is a `Query` with a handler.
* Pipeline behaviours in `Hopaut.BuildingBlocks.Application` provide cross-cutting: `ValidationBehavior`, `LoggingBehavior`, `UnitOfWorkBehavior`, `CachingBehavior`.
* Handlers return `Result` / `Result<T>` (from `Hopaut.SharedKernel`).

```csharp
// Target pattern – inside a module's Api layer
public sealed class CreatePostEndpoint
{
    public static async Task<IResult> Handle(
        CreatePostRequest request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CreatePostCommand(request), ct);
        return result.ToHttp(); // 201 with location, or ProblemDetails
    }
}
```

## Command handler rules

1. Commit only after all processing is complete (one transaction per command, owned by `UnitOfWorkBehavior`).
2. Do not publish integration events directly. Append to **Outbox** in the same transaction; the `OutboxPublisher` background service publishes after commit.
3. No business logic in the handler – delegate to aggregate methods or domain services.
4. Always accept and propagate `CancellationToken`.

```csharp
public sealed class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, Result<PostId>>
{
    public async Task<Result<PostId>> Handle(CreatePostCommand cmd, CancellationToken ct)
    {
        var host = await users.GetSummaryAsync(currentUser.Id, ct);
        var post = Post.Create(host, cmd.Request, dateTime.UtcNow);

        await postRepository.AddAsync(post, ct);
        outbox.Add(new PostCreatedIntegrationEvent(post.Id, host.UserId, post.EventTime, post.EndTime));

        return Result.Success(post.Id); // commit happens in UnitOfWorkBehavior
    }
}
```

## Integration event handler rules

1. Must be **idempotent** (use the Inbox pattern + `EventId`).
2. Use `INotificationHandler<TIntegrationEvent>`.
3. Side-effects to external systems (push, e-mail, S3) live in dedicated modules (`Notifications`, `Media`).

## Data access in the new modules

* Each module owns its **Postgres schema** and its **own `DbContext`**. No cross-module entities, no cross-module joins.
* Repositories return aggregates for write paths. Read paths use direct `IQueryable` projections inside query handlers, no repository wrapper.
* Forbidden: `EF.Functions.*` outside of an EF-Infrastructure project, `AsNoTracking` outside read paths, `SaveChangesAsync` called from anything except the `UnitOfWorkBehavior`.
* Use `DateTimeOffset` (UTC) and value objects from `SharedKernel` (`Coordinates`, `TimeRange`, `Money`). Never `Int64` Unix seconds.

## Authorization

* Resource-based via `IAuthorizationHandler<TRequirement, TResource>`. The current `IsPostOwnerOrAdminAsync` calls in controllers are the anti-pattern to remove.
* Roles & policies registered per module via `services.AddXxxModule(configuration)`.

## Mapping

* Use **Mapperly** (source generator) inside each module. Do not bring AutoMapper into new modules.
* Wire DTOs only in `Hopaut.Modules.<X>.Contracts` (for cross-module / cross-process) and in `Hopaut.Modules.<X>.Api` (for HTTP).

## Error handling

* No custom exceptions for control flow. Use `Result<T>`.
* Programmer errors (`NullReferenceException`, `KeyNotFoundException`) are bugs – let them bubble; the global ProblemDetails middleware logs and returns 500.
* Expected errors (validation, authorization, not-found) become `Error` codes inside `Result`.
* External I/O failures: catch the *specific* exception, log, fall back or retry via outbox.

## Validation

```csharp
public sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Request.Title).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Request.EventTime).GreaterThan(_ => DateTimeOffset.UtcNow);
    }
}
```

## Performance index hints

When adding a query that filters on non-PK columns, leave a TODO with the proposed index:

```csharp
// TODO: add index — CREATE INDEX IX_posts_post_user_active_endtime ON posts.post (user_id, active_flag, end_time);
return await db.Posts
    .Where(p => p.UserId == userId && p.IsActive && p.EndTime > now)
    .ToListAsync(ct);
```

## C# / style

* Nullable reference types **on** in every project.
* `dotnet_sort_system_directives_first = true`.
* Treat `CS4014` (unawaited `Task`) as error.
* `this.` qualification not required.
* Use file-scoped namespaces, primary constructors and collection expressions where the project's TFM allows.
* No XML doc comments required, but they are nice on public Contracts surface.

---

# Test code

## Stack (target)

* **xUnit v3** – `[Fact]`, `[Theory]`, `[ClassFixture]`.
* **AwesomeAssertions** – fluent assertions.
* **Moq** when no concrete test double is provided.
* **Testcontainers** for Postgres (`Testcontainers.PostgreSql`), Redis, and LocalStack S3.
* **Builder pattern** per aggregate, lives next to the unit-test project of its module.

## Stack (legacy `Bingo.IntegrationTests` until decommissioned)

* xUnit + `WebApplicationFactory<Startup>` against a real Postgres. Keep tests green during the refactor; do not add new test infrastructure here.

## Test naming and structure

* Naming: `GivenPrecondition_WhenAction_ThenExpected`.
* Structure: `// Arrange`, `// Act`, `// Assert`.
* One assertion or one `AssertionScope` per test.
* Use `TestContext.Current.CancellationToken` (xUnit v3) for all async calls in new tests.

## Test types

| Type | Indicators | Primary use |
|---|---|---|
| Unit (Domain) | No mocks, no DB, builds aggregates directly | Invariants, business rules |
| Unit (Application) | MediatR handler + Moq / in-memory test doubles | Handler behaviour |
| Integration (Module) | Real DbContext via Testcontainers Postgres | Persistence, mappers, queries |
| E2E | `WebApplicationFactory` + real Postgres + Redis | Cross-module HTTP flows |

## Builder pattern (target)

```csharp
public sealed class PostBuilder
{
    private readonly Post _post;

    private PostBuilder(PostId id) => _post = Post.RehydrateForTest(id, /* sane defaults */);

    public static PostBuilder CreateWithId(PostId id) => new(id);
    public PostBuilder WithEventTime(DateTimeOffset value) { _post.SetEventTime(value); return this; }
    public Post Build() => _post;
}
```

## Test data conventions

| Type | Convention |
|---|---|
| `string` | Short literal (`"post-123"`, `"alice@hopaut.app"`) |
| `decimal` | Use `_` separators (`1_500m`) |
| `Guid` | `Guid.NewGuid()` unless used as a stable key, then a fixed value |
| `DateTimeOffset` | `new DateTimeOffset(2024, 1, 9, 0, 0, 0, TimeSpan.Zero)` |
| `bool` | Alternate `true`/`false` to exercise both branches |
| Collections | Collection expressions (`[item1, item2]`) |
| Enum | Pick a non-default member |

## Hard rules

* Do not change assertions or expected values unless the production code's observable output demonstrably changed.
* Do not redesign business logic to make a test pass.
* Do not change entity constructors or `[InlineData]` inputs unless the production model changed.
* Preserve original test intent at all times.

## Test fix discipline

When fixing failing tests, fix **one test at a time**:

1. Run all tests, collect the full failure list.
2. Fix the first failing test.
3. Run only that test (filter by `FullyQualifiedName`) and verify green before moving on.
4. After 3 consecutive failed fix attempts on the same test, report and move on.

```bash
dotnet test <ProjectPath> --no-build --filter "FullyQualifiedName~<TestClass>.<TestMethod>"
```

---

# Quick reference – where things live today

| Concern | Today | Target (`hopaut-solution-fix.md`) |
|---|---|---|
| Routes | `Bingo.Contracts/V1/ApiRoutes.cs` | Per-module `Api` project (Minimal API endpoints) |
| Wire DTOs | `Bingo.Contracts/V1/Requests` & `Responses` | Per-module `Contracts` (cross-module) + `Api` (HTTP-only) |
| Entities | `BingoAPI/Models/` | Per-module `Domain` |
| Repositories | `BingoAPI/Models/SqlRepository/` | Per-module `Infrastructure`; interfaces in `Application` |
| Services | `BingoAPI/Services/` | Per-module `Application` (use cases) and `Infrastructure` (adapters) |
| Validators | `BingoAPI/Validators/` | Per-module `Application` |
| Mappers | `BingoAPI/CustomMapper/` + `MappingProfiles/` | Mapperly classes per module |
| DI registrations | `BingoAPI/Installers/*` | `services.AddXxxModule(configuration)` per module, called from `Hopaut.Api.Host` |
| Cross-cutting | `BingoAPI/Middleware`, `Filters`, `Cache` | `Hopaut.BuildingBlocks.*` |
| Errors | `IErrorService` + `ErrorDataContext` | Serilog + ProblemDetails middleware (deprecate the table) |
| Time | `Int64` Unix seconds | `DateTimeOffset` + `IDateTimeProvider` |
