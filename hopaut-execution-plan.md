# Hopaut Backend – Execution Plan

> Companion to `hopaut-solution-fix.md` (the **what / why**) and `hopaut-migration-status.md` (the **where are we now**).
> This document is the **how**: ordered, executable steps grouped into phases. Every step has a clear *Goal*, *Actions*, *Acceptance criteria*, and *Status checkbox to update in `hopaut-migration-status.md`*.
>
> **Working contract**
> 1. Resume work by attaching the three files: `.github/github-copilot-instructions.md`, `hopaut-execution-plan.md`, `hopaut-migration-status.md`.
> 2. Trigger with `/migrate #hopaut-execution-plan.md` (or just point at the next pending step).
> 3. After every step that changes code or configuration, **update `hopaut-migration-status.md`** (mark the step done, add a one-line note + commit hash if applicable).
> 4. Never skip ahead more than one step without explicit user approval. Never run a full build unless the user asks.

---

## Phase 0 – Runtime upgrade & foundation

> **Step 0** is the .NET 5 / .NET Core 3.1 → **.NET 10** upgrade. Everything else in this plan assumes the LTS runtime. Done first because (a) .NET 5 is unsupported and (b) several later phases depend on EF Core 8+/9+ features (TPC, JSONB owned entities, `Microsoft.AspNetCore.RateLimiting`).

### Step 0.1 – Capture baseline
* **Goal:** know what we are about to break.
* **Actions:**
  * Take a snapshot of the green build + green integration tests on `master`. Tag it `pre-net10-baseline`.
  * Record current package versions (output of `dotnet list package`) into `docs/baseline-packages.txt`.
  * Inventory `wwwroot/Configurations/*.json`, `appsettings*.json`, environment variables consumed.
* **Acceptance:** tag exists, baseline files committed.
* **Status field:** `0.1` in migration-status.

### Step 0.2 – Add solution-wide build infrastructure
* **Goal:** centralise TFM, language, analyzers, package versions.
* **Actions:**
  * Create `Directory.Build.props` at repo root: `TargetFramework=net10.0`, `LangVersion=latest`, `Nullable=enable`, `TreatWarningsAsErrors=true` (initially scoped to new projects via condition), `ImplicitUsings=enable`.
  * Create `Directory.Packages.props` enabling **Central Package Management** (`<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>`) and pin .NET 10 / EF Core / Microsoft.AspNetCore.* / Npgsql.EFCore.PostgreSQL / NetTopologySuite to the latest stable versions compatible with .NET 10.
  * Create `.editorconfig` matching the conventions in `.github/github-copilot-instructions.md` (sort System usings first, no `this.`, treat CS4014 as error).
  * Add `global.json` pinning the .NET 10 SDK.
* **Acceptance:** `dotnet --version` matches `global.json`; solution still loads in VS 2026.
* **Status field:** `0.2`.

### Step 0.3 – Upgrade `Bingo.Contracts` to .NET 10
* **Goal:** smallest, lowest-risk project first; it is referenced by all others.
* **Actions:**
  * Convert csproj to use central package versions, drop explicit `<TargetFramework>` (inherit from props), enable nullable.
  * Fix any nullable warnings.
* **Acceptance:** `dotnet build Bingo.Contracts/Bingo.Contracts.csproj` succeeds.
* **Status field:** `0.3`.

### Step 0.4 – Upgrade `BingoAPI` to .NET 10
* **Goal:** the monolith on LTS, MVC + EF Core preserved.
* **Actions:**
  * Update csproj to .NET 10. Bump packages: ASP.NET Core, EF Core (latest), Npgsql.EntityFrameworkCore.PostgreSQL, NetTopologySuite, Microsoft.AspNetCore.Identity.*, Swashbuckle, AutoMapper, FluentValidation.AspNetCore.
  * Replace **`System.Drawing.Common`** with **`SixLabors.ImageSharp`** (only used in `ImageLoader` for image processing). Cross-platform safe.
  * Replace **`AspNetCoreRateLimit`** with built-in **`Microsoft.AspNetCore.RateLimiting`**. Move config out of `RatelimiterInstaller`.
  * Replace **`Newtonsoft.Json`** wiring in `McInstaller.AddNewtonsoftJson(...)` with `AddJsonOptions(...)` using **System.Text.Json** (Newtonsoft attributes will be removed gradually – step deferred).
  * `Startup.cs` + `Program.cs` → **minimal hosting** (`WebApplication.CreateBuilder`). Keep the `IInstaller` convention; just call it from `Program.cs`.
  * Fix EF Core breaking changes: `OnModelCreating` PostGIS config, TPH discriminator API, `DbSet<Sport> Marathons` rename inconsistency, etc.
  * Fix ASP.NET Identity breaking changes (`AddDefaultIdentity` overloads).
  * Fix nullable warnings file-by-file (use `#nullable disable` only as temporary scaffolding with a TODO).
* **Acceptance:** API builds, starts, swagger UI renders, all `Bingo.IntegrationTests` pass.
* **Status field:** `0.4`.

### Step 0.5 – Upgrade `Bingo.IntegrationTests` to .NET 10
* **Actions:**
  * Bump xUnit, Microsoft.NET.Test.Sdk, FluentAssertions (or migrate to AwesomeAssertions in a later phase – not now).
  * Re-target `WebApplicationFactory<Program>` (since `Startup` is gone after 0.4 minimal hosting).
  * Make sure tests still run against the same Postgres test instance.
* **Acceptance:** `dotnet test Bingo.IntegrationTests` green.
* **Status field:** `0.5`.

### Step 0.6 – Decide fate of `Bingo.LoadTests`
* **Actions:** delete the empty netcoreapp3.1 project from the solution. (Recreate later as NBomber/k6 outside `src/`.)
* **Acceptance:** solution has 3 projects, no .NET Core 3.1 references anywhere.
* **Status field:** `0.6`.

### Step 0.7 – Replace error-logging infrastructure
* **Goal:** stop writing to `ErrorDataContext`; start writing structured logs.
* **Actions:**
  * Add **Serilog** (`Serilog.AspNetCore`, `Serilog.Sinks.Console` JSON formatter) wired in `Program.cs`.
  * Replace `ErrorHandlingMiddleware` body with a generic exception → **ProblemDetails** middleware (`AddProblemDetails()` + a custom `IExceptionHandler`).
  * Mark `IErrorService`, `ErrorService`, `ErrorDataContext`, `ErrorLog`, `ErrorController` as `[Obsolete]` – do **not** remove yet (still referenced from many catch-blocks). They will be deleted in step 0.8.
* **Acceptance:** unhandled exceptions return RFC-7807 ProblemDetails, are logged with stack trace + correlation id; ErrorLog table stops getting new rows in dev.
* **Status field:** `0.7`.

### Step 0.8 – Rip out `IErrorService` callers
* **Actions:** mechanically replace every `await _errorService.AddErrorAsync(...)` with `_logger.LogError(ex, "...", ...)`. Remove the obsolete classes, `ErrorDataContext`, `ErrorController`, the `ErrorData` migrations folder. Drop the connection string entry.
* **Acceptance:** `IErrorService` symbol does not exist; build green.
* **Status field:** `0.8`.

### Step 0.9 – Cross-cutting middleware basics
* **Actions:**
  * Add **correlation id** middleware (`X-Correlation-Id` header, push to Serilog `LogContext`, propagate to outgoing HttpClient via DelegatingHandler).
  * Add **health checks**: `AddHealthChecks().AddNpgSql(...).AddRedis(...)`. Expose `/health/live` and `/health/ready`.
  * Add **OpenTelemetry**: tracing + metrics, OTLP exporter (no backend wired yet, just emit).
  * Add the new built-in **rate limiter** policies (replace AspNetCoreRateLimit at runtime; keep behaviour parity for the IP-fixed-window policy).
* **Acceptance:** `/health/ready` returns 200 with green sub-checks; correlation id appears in every log line.
* **Status field:** `0.9`.

### Step 0.10 – Tighten JWT and remove hardcoded URLs
* **Actions:**
  * In `McInstaller`, set `ValidateIssuer = true`, `ValidateAudience = true`, `RequireExpirationTime = true`. Add `ValidIssuer` / `ValidAudience` config under `JwtSettings`.
  * Move the `WebPortalRelativeUrl` consts in `IdentityController` to a `WebPortalOptions` bound from configuration.
  * Move `Startup.MyAllowSpecificOrigins` CORS origins to `CorsOptions` bound from configuration.
  * Move `KnownProxies = 10.0.0.100` to configuration.
* **Acceptance:** no hardcoded URLs in C# files; auth still works for the mobile client.
* **Status field:** `0.10`.

### Step 0.11 – Architecture-test scaffolding
* **Actions:** add `tests/Hopaut.ArchitectureTests` (xUnit + NetArchTest). Initial rules are permissive (`BingoAPI` excluded). They start enforcing as new modules appear.
* **Acceptance:** `dotnet test tests/Hopaut.ArchitectureTests` green.
* **Status field:** `0.11`.

### Step 0.12 – Phase 0 exit gate
* **Actions:** verify all of: `0.1`–`0.11` boxes ticked, integration tests green, Swagger live, no .NET 5 / .NET Core 3.1 references in `*.csproj`, `dotnet list package --outdated` shows no out-of-support transitive packages.
* **Status field:** `0.12 – Phase 0 closed`.

---

## Phase 1 – Building blocks & composition root

### Step 1.1 – Create `src/` layout and move existing projects
* **Actions:** physically move `BingoAPI/`, `Bingo.Contracts/`, `Bingo.IntegrationTests/` under `src/Legacy/` (rename only on disk and in the .sln; namespaces unchanged). Create empty folders: `src/Bootstrap/`, `src/Shared/`, `src/Modules/`, `tests/`.
* **Status field:** `1.1`.

### Step 1.2 – Shared kernel + building blocks projects
* **Actions:** create class libraries:
  * `Hopaut.SharedKernel` – `Result`, `Result<T>`, `Error`, `Entity`, `AggregateRoot`, `IDomainEvent`, `ValueObject`, `Money`, `Coordinates`, `TimeRange`, strongly-typed ids.
  * `Hopaut.IntegrationEvents` – `IntegrationEvent` base only.
  * `Hopaut.BuildingBlocks.Application` – MediatR pkg ref; `ICommand`/`IQuery`/`IQueryHandler`; pipeline behaviours (`ValidationBehavior`, `LoggingBehavior`, `UnitOfWorkBehavior`, `CachingBehavior`); `IUnitOfWork`, `IDateTimeProvider`, `ICurrentUser`, `IIntegrationEventBus`, `PaginationRequest`, `PagedResult<T>`.
  * `Hopaut.BuildingBlocks.Infrastructure` – `OutboxMessage`, `InboxMessage`, EF configurations, `OutboxPublisher : BackgroundService`, Redis cache adapter, Serilog/OpenTelemetry registration extensions, ProblemDetails exception middleware, JWT helpers.
  * `Hopaut.BuildingBlocks.Api` – endpoint conventions, `Result.ToHttp()`, common authorization policies.
* **Acceptance:** all five build; NetArchTest rule added: `Hopaut.SharedKernel` references nothing in `Microsoft.EntityFrameworkCore` or `Microsoft.AspNetCore`.
* **Status field:** `1.2`.

### Step 1.3 – `Hopaut.Api.Host`
* **Actions:** new ASP.NET Core minimal-hosting project. Wires Serilog, OpenTelemetry, ProblemDetails, health checks, rate limiting, CORS, swagger. Exposes `services.AddHopautModules(configuration)` extension that today calls only the legacy `BingoAPI` installer chain (via a temporary adapter). Future modules will register here.
* **Acceptance:** running `Hopaut.Api.Host` boots and serves the same endpoints as the legacy `BingoAPI` (because it forwards to it during the strangler-fig period).
* **Status field:** `1.3`.

### Step 1.4 – Outbox/Inbox MVP
* **Actions:** implement `OutboxMessage` table in a dedicated schema `messaging`, write `OutboxPublisher` that scans pending rows and dispatches to MediatR `INotification`s. `IIntegrationEventBus.Publish(...)` writes to outbox in the current `DbContext` (will be wired per module). `InboxMessage` for idempotency on the consumer side.
* **Acceptance:** an integration test publishes a fake event from a transaction, the publisher delivers it once, retries on transient failure.
* **Status field:** `1.4`.

### Step 1.5 – Hangfire wiring
* **Goal:** background-job infrastructure ready before Phase 2/3 need it.
* **Actions:**
  * Add `Hangfire.AspNetCore` + `Hangfire.PostgreSql`. Configure storage on the existing Postgres connection, schema `hangfire`. Run `Hangfire` migrations on startup.
  * `services.AddHangfire(...)` + `services.AddHangfireServer(o => o.WorkerCount = ...)` in `Hopaut.Api.Host`.
  * Mount the dashboard at `/admin/hangfire` behind a `IDashboardAuthorizationFilter` that requires JWT + Admin role.
  * Define `IBackgroundJobClient` wrapper `IHopautJobs` (so handlers depend on an abstraction, not on Hangfire static API – keeps Application layer Hangfire-free).
  * Add a smoke recurring job (`HealthPingJob`) every minute in dev only, to validate.
* **Acceptance:** dashboard reachable for admin JWT, denied otherwise; smoke job visible as Succeeded.
* **Status field:** `1.5`.

### Step 1.6 – Caching: distributed Redis + MediatR `CachingBehavior`
* **Goal:** kill the broken `CachedAttribute` and provide one canonical caching path.
* **Actions:**
  * Add `Microsoft.Extensions.Caching.StackExchangeRedis`; configure `IDistributedCache` against the existing Redis instance.
  * In `Hopaut.BuildingBlocks.Application` add:
    * `ICacheableQuery` (`CacheKey`, `Ttl`, `Tags`).
    * `CachingBehavior<TQuery, TResponse>` reading/writing through `IDistributedCache` with **per-key `SemaphoreSlim` + Redis `LockTakeAsync`** stampede protection.
    * `ICacheInvalidator` with `InvalidateTagsAsync(IEnumerable<string>)` backed by a Redis SET `tag:{tag}` containing all keys for that tag.
  * In `Hopaut.BuildingBlocks.Api` add a new `[Cached(seconds, varyByUser=true)]` action filter for legacy MVC endpoints during transition. Key = `route + sortedQuery + (varyByUser ? userId : "anon") + acceptLanguage`. Sets `Vary: Authorization, Accept-Language` and `ETag`.
  * Replace the legacy `CachedAttribute` callsites with the new filter (no behaviour change beyond correctness of the key).
  * Emit OpenTelemetry counters `cache_hits_total{name}`, `cache_misses_total{name}`, `cache_evictions_total{name}`.
  * Wire a `CacheInvalidationJob` (Hangfire) that command handlers can enqueue via `ICacheInvalidator.InvalidateTagsAsync(...)` after commit.
* **Acceptance:** integration test hitting the same cached endpoint twice shows 1 miss + 1 hit; a write that emits `post:{id}` invalidation drops the corresponding key; load test on a cached endpoint shows no stampede on cold start.
* **Status field:** `1.6`.

### Step 1.7 – Role seeder hosted service
* **Actions:** move the `Program.Main` role-seeding code (Admin/User/SuperAdmin) into `RoleSeederHostedService` in `Hopaut.Api.Host`. Idempotent. Skipped when `--no-seed` is passed.
* **Status field:** `1.7`.

### Step 1.8 – DDD enforcement rules
* **Goal:** make the *DDD per module* contract from `hopaut-solution-fix.md §6.12.1` visible in code and CI.
* **Actions:**
  * In `Hopaut.SharedKernel`: define `Entity<TId>`, `AggregateRoot<TId>`, `ValueObject`, `IDomainEvent`, `IHasDomainEvents`, strongly-typed id base (`record struct XxxId(Guid Value)`).
  * Add NetArchTest rules:
    * `Domain` projects do not reference `Microsoft.EntityFrameworkCore`, `Microsoft.AspNetCore.*`, `Hangfire*`, `MediatR`.
    * Public constructors on aggregates are forbidden (must use static `Create` factories) – enforced via reflection rule (`AllAggregates_ShouldHaveOnlyPrivateOrFactoryAccessibleConstructors`).
    * `Application` projects do not reference `Microsoft.AspNetCore.*` or any concrete EF type besides `IUnitOfWork`.
  * Document the aggregate template under `templates/Aggregate.cs.template`.
* **Acceptance:** rules green on the (still empty) modules; a deliberately bad sample fails the test.
* **Status field:** `1.8`.

### Step 1.9 – Module template
* **Actions:** create a `dotnet new` template (or just a `templates/Module/` folder used by hand) that scaffolds the 5 csproj per module with the right project references and NetArchTest hooks.
* **Status field:** `1.9`.

### Step 1.10 – Phase 1 exit gate
* **Actions:** verify legacy endpoints still work via `Hopaut.Api.Host`, new building blocks have ≥80% unit coverage, Hangfire dashboard live, caching behaviour verified, NetArchTest green.
* **Status field:** `1.10 – Phase 1 closed`.

---

## Phase 2 – Identity + Users modules

### Step 2.1 – Scaffold `Identity` module
* **Actions:** create the 5 projects under `src/Modules/Identity/`. Add `services.AddIdentityModule(configuration)` extension. Module owns Postgres schema `identity`.
* **Status field:** `2.1`.

### Step 2.2 – Move ASP.NET Identity into the module
* **Actions:** create `IdentityDbContext` inside `Hopaut.Modules.Identity.Infrastructure` (separate from old `DataContext`). Migrate Identity tables to schema `identity`. Move `RefreshToken`. Keep `AppUser` here but **only** with auth fields; profile fields move in step 2.4.
* **Acceptance:** EF migration generated; data migration script copies users from old DataContext into the new schema.
* **Status field:** `2.2`.

### Step 2.3 – Re-implement auth use cases as commands
* **Actions:** `RegisterUserCommand`, `LoginCommand`, `RefreshTokenCommand`, `LoginWithFacebookCommand`, `ConfirmEmailCommand`, `ForgotPasswordCommand`, `ResetPasswordCommand`, `ChangePasswordCommand`, `AddPasswordCommand`, `GrantPostClaimCommand`, `RevokePostClaimCommand`. Endpoints in `Hopaut.Modules.Identity.Api` mirror today’s `ApiRoutes.Identity.*`.
* **Strict JWT** parameters from Step 0.10 stay.
* **Side-effects (welcome e-mail, confirmation link)** become `UserRegisteredIntegrationEvent` written to the outbox.
* **Acceptance:** all `IdentityControllerTest` integration tests pass against the new module; legacy `IdentityController` removed.
* **Status field:** `2.3`.

### Step 2.4 – Scaffold `Users` module + split profile
* **Actions:** create `Users` module under schema `users`. `UserProfile` aggregate keyed by Identity user id. Subscribe to `UserRegisteredIntegrationEvent` (idempotent inbox) → seed profile row. Migration copies `FirstName`, `LastName`, `ProfilePicture`, `Description`, `RegistrationTimeStamp` from `identity.AspNetUsers` into `users.user_profile`. Read endpoints (`GET /api/v1/users/{userId}`, profile update, profile picture update) move here.
* **Acceptance:** `UserControllerTest` + `ProfileController` tests green; legacy `ProfileController` and parts of `UserController` removed.
* **Status field:** `2.4`.

### Step 2.5 – Phase 2 exit gate
* **Actions:** Identity + Users live, old `AppUser` is auth-only, NetArchTest tightened to forbid references from outside `Identity`/`Users` to those modules’ Domain/Application/Infrastructure.
* **Status field:** `2.5 – Phase 2 closed`.

---

## Phase 3 – Posts + Media

### Step 3.1 – Scaffold `Media` module first (Posts depends on it)
* **Actions:** `Hopaut.Modules.Media.*`. Move `AwsBucketManager` → `S3MediaStorage` adapter. Replace static `IAmazonS3` field with `services.AddAWSService<IAmazonS3>()` singleton. Replace `ImageLoader` with an `ImageSharp` implementation. Public `IMediaUploader` port consumed via DI from `Posts`. `MediaUploadedIntegrationEvent` published per upload.
* **Status field:** `3.1`.

### Step 3.2 – Scaffold `Posts` module under schema `posts`
* **Actions:** create projects, EF DbContext, configurations. Move `Post`, `EventLocation`, `Picture`, `Tag`, `PostTags`, `RepeatableProperty`. PostGIS index moves with the schema migration.
* **Status field:** `3.2`.

### Step 3.3 – Collapse `Event` hierarchy
* **Goal:** kill TPH + the 9 subclasses + the `GetType().Name` filter (see TPH/TPC discussion at the top of the conversation).
* **Actions:**
  * Replace `Event` + 9 subclasses with a single `Event` entity holding: `EventType` (enum), optional `Slots`, optional `TypeSpecificData` (`JsonDocument`/owned-as-JSON column).
  * EF Core 9: configure the JSON column on Postgres `jsonb`.
  * Migration: data conversion query `INSERT INTO posts.event SELECT id, ..., CASE event_type WHEN 'house_type' THEN 1 WHEN 'bar_type' THEN 2 ... END, ...`.
  * Drop the discriminator + the subclass tables/columns.
* **Why not TPC?** Subclasses add no behaviour today; TPC would still leave 9 tables and `UNION ALL` queries. Collapse is simpler and lets us index on `event_type` for the feed.
* **Acceptance:** `EventControllerTest`-equivalent integration tests green; SQL `EXPLAIN` on the feed query shows `WHERE event_type = ...` predicate hitting the new index.
* **Status field:** `3.3`.

### Step 3.4 – Implement Posts use cases
* **Actions:** `CreatePostCommand`, `UpdatePostCommand`, `DeletePostCommand`, `DisablePostCommand`, `GetPostByIdQuery`, `GetMyActivePostsQuery`, `GetMyInactivePostsQuery`, **`GetNearbyPostsQuery`** (the “near-me” feed) – with **all filtering and pagination in SQL** (no in-memory `RemoveAll`, no client-side `Skip/Take`). Spatial filter via NetTopologySuite. Time fields become `DateTimeOffset` (UTC) backed by a one-shot data migration converting Unix seconds.
* Side-effects: emit `PostCreatedIntegrationEvent`, `PostUpdatedIntegrationEvent { ChangedFields }`, `PostDeletedIntegrationEvent { ParticipantIds, Title }` via outbox.
* **Acceptance:** `PostControllerTests` integration tests pass; legacy `PostController` removed.
* **Status field:** `3.4`.

### Step 3.5 – Async picture pipeline (Hangfire + claim-check S3)
* **Goal:** create-post must return ≤ 100ms even with multiple images attached.
* **Actions:**
  * `CreatePostCommandHandler` writes raw image bytes to S3 under `tmp/{postId}/{guid}.{ext}` synchronously (cheap), creates `Picture` rows in **`Pending`** state with the `tmp` key, and enqueues `BackgroundJob.Enqueue<IMediaProcessor>(p => p.ProcessAsync(postId, tempKeys, default))`.
  * `IMediaProcessor.ProcessAsync` (lives in `Media.Application`):
    * Downloads from `tmp/`, runs **ImageSharp** (resize to display + thumbnail), writes finals to `posts/{postId}/{guid}.{variant}.webp`, updates `Picture` rows to `Ready` with final URLs, deletes `tmp/` keys, publishes `MediaProcessedIntegrationEvent` via outbox.
    * On unrecoverable failure → `Picture.State = Failed`, publishes `MediaFailedIntegrationEvent`.
  * `DeletePostCommandHandler` writes `PostDeletedIntegrationEvent` → Media subscribes → enqueues `DeleteS3ObjectsJob` (no synchronous S3 in delete path).
  * Add a recurring Hangfire job `CleanupOrphanTmpUploadsJob` (hourly) that deletes `tmp/` keys older than 24h.
  * Add a recurring Hangfire job `RetryFailedPicturesJob` (every 15 min) that re-enqueues `Pending` pictures stuck > 10 min.
  * `GET /posts/{id}` returns pictures with their `state` so the mobile can poll/show placeholders.
* **Acceptance:** create-post p95 ≤ 100ms with 5 attached images; pictures land Ready within ≤ 5s for typical sizes; deleting a post never blocks on S3.
* **Status field:** `3.5`.

### Step 3.6 – Phase 3 exit gate
* **Actions:** TPH gone, `Int64` Unix time gone from Posts schema, feed query fast (target ≤ 50ms p95 for 10k posts at radius 15km), NetArchTest tightened, no `BingoAPI/Models/*Meet*.cs` files left.
* **Status field:** `3.6 – Phase 3 closed`.

---

## Phase 4 – Attendance + Announcements + Ratings

### Step 4.1 – `Attendance` module under schema `attendance`
* **Actions:** move `Participation`. Implement `RequestAttendanceCommand`, `AcceptAttendanceCommand`, `RejectAttendanceCommand`, `CancelAttendanceCommand`, `IsUserAttendingQuery`. Publish corresponding integration events. Subscribe to `PostDeletedIntegrationEvent` (cleanup) and `PostUpdatedIntegrationEvent` (slot reduction guard).
* **Acceptance:** `AttendedEventsControllerTest` and `EventAttendeesControllerTest` integration tests green.
* **Status field:** `4.1`.

### Step 4.2 – `Announcements` module under schema `announcements`
* **Actions:** move `Announcement`. Implement CRUD use cases. Subscribe to `AttendanceAcceptedIntegrationEvent` for inbox visibility. Publish `AnnouncementCreatedIntegrationEvent`.
* **Acceptance:** `AnnouncementControllerTest` integration tests green.
* **Status field:** `4.2`.

### Step 4.3 – `Ratings` module under schema `ratings`
* **Actions:** move `Rating`. Use cases for create + read + delete. Subscribe to `RatingCreatedIntegrationEvent` to maintain a `user_reputation` projection table (sum + count + avg per `RatedUserId`).
* **Status field:** `4.3`.

### Step 4.4 – Wire feed → ratings via query
* **Actions:** rewrite `GetNearbyPostsQuery` to call `Ratings.Application.GetRatingsForUsersQuery` (batch) once per page, eliminating the per-post `await _ratingRepository.GetUserRating(post.UserId)` N+1.
* **Acceptance:** SQL trace shows ≤ 2 queries per feed call.
* **Status field:** `4.4`.

### Step 4.5 – Phase 4 exit gate
* **Status field:** `4.5 – Phase 4 closed`.

---

## Phase 5 – Notifications + Moderation + BugReports + Payments

### Step 5.1 – `Notifications` module
* **Actions:** stateless module subscribing to integration events via inbox. OneSignal HTTP client built from a strongly-typed `OneSignalNotificationRequest` record and `System.Text.Json`. SMTP replaced with **MailKit** (or SES/SendGrid depending on hosting decision; see open question 5 in `hopaut-solution-fix.md`). All sends triggered by outbox-driven background worker.
* **Acceptance:** sending an event publishes a push and an e-mail; failures retry with exponential backoff; idempotency by `EventId`.
* **Status field:** `5.1`.

### Step 5.2 – `Moderation` module under schema `moderation`
* **Actions:** move `Report`, `UserReport`. Subscribe to `*DeletedIntegrationEvent` to close open reports. Use cases for create + list + resolve.
* **Acceptance:** `ReportControllerTest` and `UserReportControllerTest` integration tests green.
* **Status field:** `5.2`.

### Step 5.3 – `BugReports` module under schema `bug_reports`
* **Actions:** move `Bug`, `BugScreenshot`. Screenshots upload via `Media`.
* **Status field:** `5.3`.

### Step 5.4 – `Payments` module
* **Actions:** move `MyServicesHttpClient`, `MyServicesRoutes`, `PaymentsController`. Keep thin proxy shape. No DB.
* **Status field:** `5.4`.

### Step 5.5 – Phase 5 exit gate
* **Status field:** `5.5 – Phase 5 closed`.

---

## Phase 6 – Cleanup & hardening

### Step 6.1 – Delete `src/Legacy/BingoAPI`
* **Actions:** verify zero traffic to legacy endpoints (logs); delete the project and its installers, mappers, repositories, services, models, migrations.
* **Status field:** `6.1`.

### Step 6.2 – Strongly-typed IDs across all modules
* **Goal:** eliminate primitive obsession for identifiers (§6.12.1: strongly-typed ids kill accidental `int → int` parameter swaps).
* **Actions:**
  * In `Hopaut.SharedKernel` add: `record struct PostId(int Value)`, `record struct UserId(string Value)`, `record struct RatingId(int Value)`, `record struct ParticipationId(int Value)`, `record struct AnnouncementId(int Value)`, `record struct BugReportId(int Value)`, `record struct PostReportId(int Value)`, `record struct UserReportId(int Value)`, `record struct PictureId(int Value)`.
  * Add an EF Core `ValueConverter<TId, TPrimitive>` generic helper in `Hopaut.BuildingBlocks.Infrastructure` for seamless DB mapping.
  * Replace raw `int Id` / `string UserId` in every module's Domain entities with their strongly-typed equivalents.
  * Update EF configurations, repositories, command/query handlers, and DTOs to use the new types.
  * Update API endpoints where primitives are bound from route parameters (add custom `TryParse` on the record structs for minimal API binding).
* **Acceptance:** no raw `int`/`string` used as an entity identifier in any `*.Domain` project; build green; all arch tests pass.
* **Status field:** `6.2`.

### Step 6.3 – Value objects in SharedKernel + module domains
* **Goal:** replace remaining primitive obsession with immutable value objects (§3.1, §6.12.1).
* **Actions:**
  * In `Hopaut.SharedKernel` add value objects: `Coordinates` (Latitude, Longitude – replaces raw `double` pairs), `TimeRange` (Start, End – `DateTimeOffset`), `Money` (Amount, Currency).
  * In Posts.Domain: replace `double Longitude/Latitude` in `EventLocation` with `Coordinates`; replace `long PostTime/EventTime/EndTime` with `DateTimeOffset` properties (backed by `TimeRange` where applicable).
  * In Ratings.Domain: replace `int Rate` with a `RatingValue` value object (1–5 range enforced in constructor).
  * In Attendance.Domain: ensure `AttendanceStatus` is treated as a value object (already an enum, but validate transitions via a domain method).
  * In all modules: replace `long Timestamp` with `DateTimeOffset` (UTC). Add `IDateTimeProvider` usage in factories/handlers.
  * Update EF configurations with `HasConversion` for each value object.
* **Acceptance:** no `Int64` timestamp fields remain in any Domain project; `Coordinates` is used everywhere instead of raw doubles; build green.
* **Status field:** `6.3`.

### Step 6.4 – Aggregate root factories + private constructors
* **Goal:** enforce aggregate invariants at creation time (§6.12.1: "public constructors on aggregates are forbidden").
* **Actions:**
  * For each aggregate root (`Post`, `Participation`, `Announcement`, `Rating`, `Bug`, `PostReport`, `UserReport`):
    * Make constructor(s) `private` (or `internal` for EF).
    * Add a static `Create(...)` factory method that validates invariants and raises a domain event (e.g. `PostCreatedDomainEvent`).
  * Update all command handlers to call `Xxx.Create(...)` instead of `new Xxx { ... }`.
  * Update EF configurations: use `HasNoKey()` or configure parameterless private constructor access via `entity.HasConstructorBinding(...)` or `UsePropertyAccessMode`.
  * Tighten the existing NetArchTest rule `AggregateRoots_Should_Have_Private_Constructors` to scan all module assemblies (not just empty ones).
* **Acceptance:** NetArchTest rule passes; no `new Post()` or `new Rating()` outside the aggregate; build green.
* **Status field:** `6.4`.

### Step 6.5 – Domain events raised from aggregates
* **Goal:** aggregates raise `IDomainEvent` on state changes; `UnitOfWorkBehavior` dispatches them after commit (§6.12.1).
* **Actions:**
  * Make all aggregates inherit `AggregateRoot<TId>` (which provides `AddDomainEvent`).
  * Define domain events per module:
    * Posts: `PostCreatedDomainEvent`, `PostDeletedDomainEvent`, `PictureStateChangedDomainEvent`.
    * Attendance: `AttendanceRequestedDomainEvent`, `AttendanceAcceptedDomainEvent`, `AttendanceRejectedDomainEvent`.
    * Ratings: `RatingCreatedDomainEvent`, `RatingDeletedDomainEvent`.
    * Announcements: `AnnouncementCreatedDomainEvent`.
  * In each module's `Infrastructure` `DbContext.SaveChangesAsync` override (or via a MediatR `UnitOfWorkBehavior`): dispatch domain events after commit, then clear.
  * Domain events that must cross module boundaries are translated into integration events (e.g. `PostCreatedDomainEvent` → `PostCreatedIntegrationEvent` via an event handler in the same module).
* **Acceptance:** domain events are raised and dispatched in at least Posts + Attendance + Ratings; integration tests verify side-effects triggered by domain events; build green.
* **Status field:** `6.5`.

### Step 6.6 – Contracts projects per module
* **Goal:** formalize cross-module public surface (§1 layout: `Hopaut.Modules.<X>.Contracts`).
* **Actions:**
  * For each module, create a `Hopaut.Modules.<X>.Contracts` project containing:
    * Public DTOs consumed by other modules (e.g. `UserSummaryDto`, `PostSummaryDto`).
    * Integration event records (move from inline definitions to dedicated Contracts project).
    * Query interfaces for cross-module reads (e.g. `IGetReputationsForUsersQuery`).
  * Update cross-module references: other modules reference only `*.Contracts`, never `*.Application` or `*.Domain`.
  * Remove the current `Posts.Infrastructure → Ratings.Application` reference; replace with `Posts.Infrastructure → Ratings.Contracts`.
  * Add NetArchTest rule: `NoModuleReferencesAnotherModuleNonContracts`.
* **Acceptance:** no module references another module's Domain/Application/Infrastructure directly; only Contracts; NetArchTest enforces this; build green.
* **Status field:** `6.6`.

### Step 6.7 – Tighten NetArchTest rules
* **Actions:** add `NoModuleReferencesAnotherModuleNonContracts` rule (enforced after 6.6). Add `DomainHasNoEfReferences`, `ApplicationHasNoAspNetCoreReferences`, `ContractsHasNoInfrastructureReferences`. Make CI fail on violation.
* **Status field:** `6.7`.

### Step 6.8 – Replace AutoMapper with Mapperly
* **Goal:** source-generated, compile-time-checked mapping (§6.9).
* **Actions:**
  * Add `Riok.Mapperly` to `Directory.Packages.props`.
  * In each module's `Api` or `Application` layer, create a `[Mapper]` partial class that maps Domain entities → DTOs and command requests → domain objects.
  * Remove AutoMapper package references and all `Profile` classes.
  * Verify no reflection-based mapping remains.
* **Acceptance:** `AutoMapper` package removed from `Directory.Packages.props`; all mapping is compile-time generated; build green.
* **Status field:** `6.8`.

### Step 6.9 – Resource-based authorization
* **Goal:** replace ad-hoc `IsPostOwnerOrAdminAsync` with proper policy-based auth (§6.8).
* **Actions:**
  * Define authorization requirements: `PostOwnerOrAdminRequirement`, `AnnouncementOwnerRequirement`, `RatingOwnerRequirement`.
  * Implement `IAuthorizationHandler<TRequirement, TResource>` for each.
  * Register policies: `MustOwnPost`, `MustOwnAnnouncement`, `MustOwnRating`.
  * Replace manual ownership checks in command handlers / endpoints with `[Authorize(Policy = "...")]` or `AuthorizationService.AuthorizeAsync(...)`.
* **Acceptance:** no manual `if (post.UserId != currentUser)` checks in handlers; policies are unit-testable; build green.
* **Status field:** `6.9`.

### Step 6.10 – API versioning formalization
* **Goal:** the URL already says `/api/v1` – formalize it (§7).
* **Actions:**
  * Add `Asp.Versioning.Http` + `Asp.Versioning.Mvc.ApiExplorer` to `Directory.Packages.props`.
  * Configure API versioning in the host: default version 1.0, URL segment strategy.
  * Tag all endpoint groups with `ApiVersion(1, 0)`.
  * Swagger generates per-version docs.
* **Acceptance:** Swagger shows v1 doc; adding a v2 endpoint in the future requires only a new group + version attribute; build green.
* **Status field:** `6.10`.

### Step 6.11 – Performance pass
* **Actions:** profile `GetNearbyPostsQuery`, `CreatePostCommand`, `LoginCommand`, `GetMyActivePostsQuery`. Add missing indexes (use the `// TODO: add index` comments produced during the migration).
* **Status field:** `6.11`.

### Step 6.12 – Decide worker extraction
* **Actions:** decide whether to extract `Notifications` and `Media` into separate worker processes (`Hopaut.Worker.Outbox`, `Hopaut.Worker.Media`) or keep them in-process. Document the decision; if extracted, swap `IIntegrationEventBus` from `InProcess` to MassTransit/SQS in `Hopaut.Api.Host` only.
* **Status field:** `6.12`.

### Step 6.13 – Definition-of-Done check
* **Actions:** walk every bullet of `hopaut-solution-fix.md` §13 and tick it off in `hopaut-migration-status.md`:
  * All projects target the same supported .NET LTS. ✓
  * No project named `BingoAPI` exists. ✓
  * Each module has Domain / Application / Infrastructure / Api / Contracts and ≥ 70% Domain+Application unit-test coverage.
  * No cross-module reference except to `*.Contracts`. Verified by NetArchTest in CI.
  * All write endpoints go through MediatR commands with the standard pipeline.
  * Outbox is the only path for integration events.
  * No `Int64` time fields in the domain.
  * No `ErrorLog` table.
  * Logs are structured JSON with correlation id; traces visible in OTLP backend.
  * Health endpoints `/health/live`, `/health/ready` are green in production.
  * The mobile app continues to work against `/api/v1` without changes.
  * Strongly-typed IDs everywhere; no primitive obsession for identifiers.
  * All aggregates use factories; no public constructors.
  * Domain events dispatched after commit.
  * Value objects for coordinates, time, money, rating values.
* **Status field:** `6.13 – Project complete`.

---

## Cross-phase invariants (apply to every step)

* No step is "done" until **`hopaut-migration-status.md` is updated** with: status (✅ / 🟡 in-progress / ❌ blocked), date, short note, optional commit hash.
* Never run `dotnet build` or `dotnet test` for the whole solution unless asked.
* When a step changes a public contract (DTO / route), bump nothing yet – we stay on `/api/v1` until a v2 is explicitly planned.
* When a step touches data, ship the EF migration **and** a one-shot data conversion script in the same commit.
* Every new module must come with: at least one Domain unit test, one Application handler test, one Infrastructure repository test (Testcontainers), and one Api endpoint test (WebApplicationFactory).

---

## How to resume work in a new chat (copy-paste)

> Attach: `.github/github-copilot-instructions.md`, `hopaut-execution-plan.md`, `hopaut-migration-status.md`.
>
> Prompt: `/migrate #hopaut-execution-plan.md – continue from the next pending step in hopaut-migration-status.md. Do exactly one step. Then update hopaut-migration-status.md.`
