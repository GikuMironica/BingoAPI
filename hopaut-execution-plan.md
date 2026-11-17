# Hopaut Backend – Execution Plan

> Companion to `hopaut-solution-fix.md` (the **what / why**) and `hopaut-migration-status.md` (the **where are we now**).
> This document is the **how**: ordered, executable steps grouped into phases. Every step has a clear *Goal*, *Actions*, *Acceptance criteria*, and *Status checkbox to update in `hopaut-migration-status.md`*.
>
> **Working contract**
> 1. Resume work by attaching the three files: `.github/copilot-instructions.md`, `hopaut-execution-plan.md`, `hopaut-migration-status.md`.
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
* **Re-scoped 2026-11-17:** the legacy `[Cached]`-filter actions are void (legacy deleted in 6.1). Target: `HybridCache` (`Microsoft.Extensions.Caching.Hybrid`) with Redis as L2 and tag invalidation, behind a Mediator `CachingBehavior` for `ICachedQuery`; replaces the custom `ICacheService`/`RedisCacheService`. Runs in Phase 9 after Step 7.6 (dotnet-10-migration skill §3). Acceptance unchanged except the legacy endpoint becomes a cached query.
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
  * **Added 2026-11-17 (F-052):** move each module's service registration (`AddXxxModule`) from `*.Api` into `*.Infrastructure`; `*.Api` keeps only `MapXxxEndpoints`, so it no longer references its Infrastructure, its Domain or EF Core. Remove the matching `ARCH-LAYER-API` and `ARCH-LAYER-INFRASTRUCTURE` entries from `tests/Hopaut.ArchitectureTests/ArchitectureBaseline.cs` (the cross-module rule already exists since 6.7).
* **Acceptance:** no module references another module's Domain/Application/Infrastructure directly; only Contracts; NetArchTest enforces this; build green.
* **Status field:** `6.6`.

### Step 6.7 – Tighten NetArchTest rules
* **Actions:** add `NoModuleReferencesAnotherModuleNonContracts` rule (enforced after 6.6). Add `DomainHasNoEfReferences`, `ApplicationHasNoAspNetCoreReferences`, `ContractsHasNoInfrastructureReferences`. Make CI fail on violation.
* **Extended 2026-11-17 (decisions D-002, D-005 in `TODO.md`):** runs right after Step 7.1, before 6.6, as a *ratchet*:
  * `tests/Hopaut.ArchitectureTests` references every module project and checks one rule per row of the layer table in `.github/skills/clean-architecture-ddd/SKILL.md` §1, plus: no public setters in `*.Domain`, aggregates have no public constructors (scanning module Domain assemblies, not SharedKernel), Domain/Application free of EF Core, ASP.NET Core, Hangfire and MassTransit, and modules reference other modules only through `*.Contracts`.
  * Known violations live in one baseline file; every entry carries the finding ID that fixes it. The suite fails on any violation missing from the baseline **and** on any baseline entry that no longer occurs, so the baseline can only shrink.
  * A negative test per rule proves that the rule reports a deliberately bad fixture type.
* **Acceptance:** `dotnet test tests/Hopaut.ArchitectureTests` green; every baseline entry maps to an open finding in `TODO.md`; deleting a baseline entry whose violation still exists fails the suite.
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
* **Implemented 2026-11-17 (D-024 in `TODO.md`):** Application port `IResourceAuthorizer` (handlers: load → authorize → domain verb) with an ASP.NET Core adapter that evaluates named policies for the request principal (anonymous without a request); one generic `ResourceOwnerRequirement<TResource>` + handler in BuildingBlocks.Infrastructure, registered per module with `AddResourceOwnerPolicy<TResource>(name, ownerOf, administratorsAllowed)`. Policies: `posts.post.owner` (`MustOwnPost`: host or admin; delete post), `announcements.post.host` (`MustOwnAnnouncement`: post host or admin; create/delete announcement), `attendance.post.host` (post host only; accept/reject attendee), `ratings.rating.owner` (`MustOwnRating`: rater or admin; delete rating), `users.profile.owner` (profile owner or admin; profile and picture updates); report listing and resolution require `HopautPolicies.Admin`. The post host reaches Announcements and Attendance through Posts.Contracts `IPostHostQueries`. Closes the authorization parts of 8.3, 8.4, 8.5, 8.6 and 8.8 (F-009–F-012).
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

## Phase 7 – Audit remediation: safety net & foundation

> Added 2026-11-17 by the HopautArchitect Phase-1 audit (findings `F-###` and decisions `D-###` live in `TODO.md`). Every step that the audit reopened in `hopaut-migration-status.md` names the Phase 7–11 step that closes it.
>
> **Execution order:** 7.1 → 6.7 → 7.2 → 7.3 → 7.4 → 7.5 → 7.6 → 7.7 → 6.6 → 6.9 → 7.8. Steps 6.6, 6.7 and 6.9 keep their IDs but run here, because the Phase 8 module steps depend on the safety net, on Contracts query interfaces and on the authorization port (D-005).
>
> **Wire contract during Phases 7–9:** module endpoints keep today's HTTP behaviour; BingoMobile does not call them yet (it targets the legacy routes, see the contract inventory). Contract changes happen per module in Phase 10, together with BingoMobile (D-007).

### Step 7.1 – Repository hygiene and supply-chain fixes
* **Goal:** no dead legacy artefacts, CPM lists only used or planned packages, no known-vulnerable package except the license-blocked one.
* **Actions:**
  * Delete the tracked zero-byte legacy stubs (`BingoAPI/BingoAPI.csproj`, `BingoAPI/Startup.cs`, `BingoAPI/appsettings.json`, `Bingo.IntegrationTests/Bingo.IntegrationTests.csproj`) and `Bingo.LoadTests/Bingo.LoadTests.csproj` (netcoreapp3.1); move the k6 scripts to `tests/load/`.
  * Remove unused `PackageVersion` entries (AutoMapper ×2, Swashbuckle ×3, `Microsoft.AspNetCore.Mvc.NewtonsoftJson`, `Microsoft.AspNetCore.Identity.UI`, `Microsoft.AspNetCore.Http.Features`, EF Core InMemory/Tools, `Microsoft.Extensions.DependencyInjection`, `Flurl.Http`, `System.Drawing.Common`, `System.Data.SqlClient`, VS container tools, `Xunit.Priority`, `System.Net.Http.Json`). Keep entries planned by later steps (`Microsoft.AspNetCore.Authentication.JwtBearer` → 7.7; `Microsoft.AspNetCore.Mvc.Testing`, `NSubstitute` → 7.2).
  * Enable `CentralPackageTransitivePinningEnabled`; pin patched versions of vulnerable transitive packages (`Newtonsoft.Json`, `System.Security.Cryptography.Pkcs`); upgrade MailKit and OpenTelemetry.* to patched versions after a license check recorded in `TODO.md`.
  * Rewrite `Dockerfile` (multi-stage .NET 10 SDK → ASP.NET runtime, `src/Host/Hopaut.Api.Host`, non-root user) and `docker-compose.yml` (PostGIS and Redis images, credentials from environment variables).
  * `SixLabors.ImageSharp` stays at its current version until blocker B-001 is resolved.
* **Acceptance:** `dotnet list Bingo.sln package --vulnerable --include-transitive` reports only `SixLabors.ImageSharp`; `git grep -nE "netcoreapp3\.1|BingoAPI\.csproj" -- ":!*.md"` finds nothing; no project references AutoMapper; build and tests green.
* **Closes:** 0.6, 6.1, 6.8.
* **Status field:** `7.1`.

### Step 7.2 – Test pyramid scaffolding
* **Goal:** a place and a harness for every test type before any refactoring starts.
* **Actions:**
  * Add license-checked test packages to CPM: `xunit.v3`, `xunit.runner.visualstudio` 3.x, `AwesomeAssertions`, `NSubstitute`, `Microsoft.Extensions.TimeProvider.Testing`, `Testcontainers.PostgreSql`, `Microsoft.AspNetCore.Mvc.Testing`; move `Hopaut.ArchitectureTests` to xUnit v3 + AwesomeAssertions and drop FluentAssertions.
  * Create `tests/Hopaut.BuildingBlocks.UnitTests` (SharedKernel + BuildingBlocks) and `tests/Hopaut.IntegrationTests` (Testcontainers PostGIS fixture, `WebApplicationFactory<Program>` fixture, test JWT helper).
  * Document the container runtime (Docker, or Podman with `DOCKER_HOST`) in `tests/README.md`.
  * Module unit-test projects (`tests/Modules/<X>/Hopaut.Modules.<X>.UnitTests`) are created by the Phase 8 module steps from `templates/Module`.
* **Acceptance:** `dotnet test Bingo.sln` runs the BuildingBlocks unit tests and an integration smoke test (Host boots against a PostGIS container; `/health` returns 200) green.
* **Status field:** `7.2`.

### Step 7.3 – Baseline EF Core migrations per module
* **Goal:** every module schema is reproducible from migrations; no `EnsureCreated`.
* **Actions:**
  * Add a local tool manifest with `dotnet-ef` 10.x and `Microsoft.EntityFrameworkCore.Design` (private assets) to the Host.
  * Add an `IDesignTimeDbContextFactory` per module DbContext (connection string from `HOPAUT_DESIGN_CONNECTION`, placeholder otherwise).
  * Generate `InitialCreate` for Identity, Users, Posts (PostGIS extension), Attendance, Announcements, Ratings, Moderation, BugReports and the transitional `OutboxDbContext` (removed in 9.1); each history table lives in its module schema; review `dotnet ef migrations script`.
  * Apply migrations at startup only when `Database:MigrateOnStartup` is true (Development and tests); production applies reviewed scripts.
* **Acceptance:** an integration test migrates every context on a PostGIS container; `dotnet ef migrations has-pending-model-changes` is clean for every context.
* **Closes (partly):** 2.2, 3.2, 3.3 (data conversion stays with 11.1).
* **Status field:** `7.3`.

### Step 7.4 – `Result`/`Error` foundation, problem details and `ICurrentUser`
* **Goal:** expected failures are typed values with stable codes; the HTTP edge maps them in one place.
* **Actions:**
  * SharedKernel: `ErrorType` (Validation, Unauthorized, Forbidden, NotFound, Conflict, Rule, Failure), `Error(Code, Description, Type)`, `ValidationError` with field errors, `Result` / `Result<T>` whose `Value` throws on failure.
  * BuildingBlocks.Api: `ErrorType` → status mapping, `Result` → `TypedResults` / problem helpers, problem-details customisation (`errorCode`, `traceId`), an `IExceptionHandler` returning a 500 problem without internals, and problem+json for binding failures.
  * BuildingBlocks.Application: `ICurrentUser` port; BuildingBlocks.Api: claims-based implementation with one claim-type constant (replaces inline `"id"` parsing and its `""` / `"unknown"` fallbacks).
  * Existing endpoints keep today's HTTP behaviour (D-007); modules adopt the helpers in Phases 8 and 10.
* **Acceptance:** unit tests cover every `ErrorType` mapping, `Result` semantics and the exception handler (no exception message, type or stack trace in the body); build and tests green.
* **Closes (partly):** 0.7.
* **Status field:** `7.4`.

### Step 7.5 – `TimeProvider`, Guid v7 and SharedKernel base types
* **Goal:** no hidden clocks or random IDs; only aggregates raise events; IDs bind and serialize as primitives.
* **Actions:**
  * Register `TimeProvider.System`; replace `IDateTimeProvider` and every `DateTime(Offset).UtcNow` in `src` (domain factories and verbs receive `DateTimeOffset now`).
  * Replace `Guid.NewGuid()` with `Guid.CreateVersion7()`; the domain-event base carries `EventId` (v7) and `OccurredOnUtc` (`DateTimeOffset`).
  * `Entity<TId>`: transient-aware equality. `AggregateRoot<TId>`: `protected Raise(...)` replaces the public `AddDomainEvent`.
  * Strongly-typed IDs implement `IParsable<T>`, reject non-positive ints and empty strings/Guids, and serialize as primitives through a System.Text.Json converter factory; typed EF converters replace the `dynamic`/`Activator` ones.
* **Acceptance:** `git grep -nE "DateTime(Offset)?\.(UtcNow|Now)|Guid\.NewGuid\(" -- src` is empty; unit tests for ID parsing, JSON round-trip and entity equality; architecture rule "no public `AddDomainEvent`" green.
* **Status field:** `7.5`.

### Step 7.6 – MediatR → Mediator, pipeline and module unit of work
* **Goal:** compile-time-checked messaging with one transaction per command (messaging-integration skill §3–§4).
* **Actions:**
  * License-check `Mediator.Abstractions` / `Mediator.SourceGenerator` 3.x; follow the skill's MediatR → Mediator recipe (generator only in the Host, scoped lifetime, generator diagnostics as errors).
  * Commands and queries become `ICommand<Result…>` / `IQuery<Result…>` with one command marker per module; endpoints map results back to today's HTTP responses (D-007).
  * Behaviors: Logging → Validation (returns `ValidationError`, never throws) → Caching (existing `ICachedQuery`) → per-module UnitOfWork (one save on success, none on failure).
  * `ModuleDbContext.SaveChangesAsync` dispatches domain events **before** `base.SaveChangesAsync`, looping until none remain (D-004); repositories and handlers stop calling `SaveChangesAsync`.
  * Replace `FluentValidation.AspNetCore` with `FluentValidation` + `FluentValidation.DependencyInjectionExtensions`; register every module's validators.
* **Acceptance:** MediatR is gone from CPM and an architecture test forbids it; unit tests prove validation short-circuits and the unit of work saves exactly once and only on success; an integration test proves a failing domain-event handler leaves no committed row.
* **Closes (partly):** 6.5.
* **Status field:** `7.6`.

### Step 7.7 – Host hardening: authentication, observability, options
* **Goal:** the Host enforces JWT authentication and production-grade cross-cutting concerns.
* **Actions:**
  * `AddAuthentication().AddJwtBearer(...)` from validated `JwtSettings` (issuer, audience, signing key from user-secrets or environment variables, lifetime validation, small clock skew); role and policy constants (`Admin`, `User`, `SuperAdmin`).
  * Correlation-id middleware (`X-Correlation-Id`, Serilog `LogContext`, propagation to outgoing `HttpClient` calls).
  * `/health/live` (no checks) and `/health/ready` (module DbContexts, Redis), separated by tags.
  * OpenTelemetry OTLP exporter configured through the standard `OTEL_*` variables; console exporter only in Development.
  * Every options type uses `BindConfiguration().ValidateDataAnnotations().ValidateOnStart()`; `ValidateScopes` / `ValidateOnBuild` in Development.
  * Hangfire dashboard behind an Admin-only `IDashboardAuthorizationFilter`; forwarded headers from `ProxySettings`; CORS only when origins are configured.
* **Acceptance:** integration tests — a protected endpoint returns 401 without a token and with a wrong-issuer, wrong-audience or expired token, and succeeds with a valid token; `/health/live` and `/health/ready` return 200; every response carries `X-Correlation-Id`; the dashboard denies non-admins; startup fails when the JWT key is missing.
* **Closes:** 0.9, 0.10. **Closes (partly):** 0.7, 1.5.
* **Status field:** `7.7`.

### Step 7.8 – Phase 7 exit gate
* **Actions:** run verification-ladder level 4 (`dotnet build Bingo.sln -c Release -p:HopautStrictWarnings=true`, `dotnet format Bingo.sln --verify-no-changes`, `dotnet list Bingo.sln package --vulnerable --include-transitive`); confirm 7.1–7.7, 6.6, 6.7 and 6.9 are ✅ and that the reopened steps they close are ✅.
* **Prerequisite (2026-11-17):** Release builds need the free Six Labors Community license key for ImageSharp 3.2 (blocker B-002 in `TODO.md`), supplied as the `SixLaborsLicenseKey` environment variable or the git-ignored repository-root `sixlabors.lic`.
* **Status field:** `7.8 – Phase 7 closed`.

---

## Phase 8 – Domain model refactoring (per module)

> Every Phase 8 module step applies this checklist (clean-architecture-ddd skill), then its module-specific actions:
> 1. Create `tests/Modules/<X>/Hopaut.Modules.<X>.UnitTests` from `templates/Module` (Domain + Application tests).
> 2. Aggregates: private setters, private collections exposed read-only, no child → root back-references, enums or value objects instead of flags and primitives, IDs known before any event is raised (int keys: per-aggregate Postgres sequence through the repository's `NextIdAsync` + `ValueGeneratedNever()`; Guid keys: `Guid.CreateVersion7()`). Done in Step 7.6 for the root aggregates Post, Participation, Announcement, Rating, PostReport, UserReport and Bug (D-019); child entities still use database keys.
> 3. Invariants and transitions return `Result` with errors from a per-aggregate catalog (`<module>.<entity>.<reason>` codes).
> 4. Handlers: load → authorize (6.9 port) → domain method → add; `ICurrentUser`, `TimeProvider`, `CancellationToken`; no `SaveChangesAsync`.
> 5. Persistence: value objects as complex types, typed ID converters, `xmin` concurrency where several actors edit; one EF migration per model change.
> 6. Wire unchanged (map value objects to today's shape); contract changes wait for Phase 10 (D-007).
> 7. Remove the module's entries from the architecture baseline.
>
> **Acceptance for every module step:** the skill's *Completion criterion* holds for the module; Domain + Application line coverage ≥ 70 % (coverlet); architecture tests green without the module's baseline entries; `dotnet test Bingo.sln` green.

### Step 8.1 – SharedKernel value-object catalog
* **Goal:** valid-by-construction building blocks for the module steps.
* **Actions:** `Coordinates` (WGS-84 ranges, `Result` factory; replaces the `IsValid` flag), `TimeRange` (start before end, UTC), `Money` + `Currency` (ISO-4217, amount ≥ 0), `RatingValue` (1–5, `Result` factory instead of throwing), `EmailAddress`, bounded text (`PostTitle`, `Description`), `Capacity`, `RadiusMeters`, `PageSize` / `Cursor`, `MediaKey`; EF complex-type mapping helpers.
* **Acceptance:** unit tests for every factory (valid input, each invalid case, normalisation); no value object exposes a constructor that skips validation.
* **Status field:** `8.1`.

### Step 8.2 – Posts
* **Actions:** `Post` owns event data, location, pictures, tags and the repeatable property (remove `Picture.Post`, `Event.Post`, `EventLocation.Post`, `RepeatableProperty.Post`, `Tag.Posts`); `ActiveFlag` → `PostStatus`; `TimeRange`, `Money`, `Capacity`, `Coordinates` and text value objects; verbs `Create`, `Update`, `Reschedule`, `Disable`, `AttachPicture`, `MarkPictureReady` / `MarkPictureFailed`, `Delete`; missing use cases `UpdatePost`, `DisablePost`, `GetMyPosts(status)`; the nearby query is bounded (`RadiusMeters`, limit ≤ 100, cursor) and served by an `AsNoTracking` read model; ownership through 6.9.
* **Closes (partly):** 3.4, 6.2, 6.3, 6.4.
* **Status field:** `8.2`.

### Step 8.3 – Attendance
* **Actions:** `Participation.Request` checks (through Posts.Contracts) that the post exists, is active and has capacity, and that the requester is not the host; `Accept` / `Reject` only by the host and `Cancel` only by the participant, all returning `Result` (`attendance.participation.not_pending`, …); duplicate request → `Conflict`; paged attendee and my-attendances queries. *(Host-only accept/reject through `attendance.post.host` and the caller-scoped cancel are in place since 6.9.)*
* **Closes (partly):** 4.1.
* **Status field:** `8.3`.

### Step 8.4 – Ratings
* **Actions:** the handler builds `RatingValue` from the raw input (failures collected into one validation error) and `Rating.Create` takes it; rater from `ICurrentUser`; eligibility rules recovered from the legacy rating logic at tag `pre-net10-baseline`; delete only by the rater (or Admin) *(in place since 6.9: `ratings.rating.owner`)*; duplicate → `Conflict`; the reputation update moves to the 9.3 projection (remove the synchronous `UpsertAsync`).
* **Closes (partly):** 4.3.
* **Status field:** `8.4`.

### Step 8.5 – Announcements
* **Actions:** only the post host creates, updates and deletes (Posts.Contracts + 6.9; create and delete use `announcements.post.host` since 6.9, `UpdateAnnouncement` must use it too); message value object; `UpdateAnnouncement` and `GetAnnouncementById` use cases (BingoMobile calls them); paged list per post; inbox / outbox read models for 10.6.
* **Closes (partly):** 4.2.
* **Status field:** `8.5`.

### Step 8.6 – Moderation
* **Actions:** reporter from `ICurrentUser`; duplicate report → `Conflict`; `Resolve` returns `Result` and requires the Admin policy *(endpoint-level `HopautPolicies.Admin` on listing and resolving since 6.9)*; paged open-report query.
* **Closes (partly):** 5.2.
* **Status field:** `8.6`.

### Step 8.7 – BugReports
* **Actions:** `Bug` owns its screenshots as `MediaKey` values (no public setters on `BugScreenshot`); reporter from `ICurrentUser`; status enum with guarded transitions.
* **Status field:** `8.7`.

### Step 8.8 – Users
* **Actions:** `UserProfile` keyed by the identity `UserId`; `RegistrationTimeStamp` (`long`) → `RegisteredAt` (`DateTimeOffset`) with a migration and conversion script; profile verbs (`Rename`, `Describe`, `ChangePicture`, `RemovePicture`; decide where the phone number lives); every mutation acts on `ICurrentUser` (`me`) *(until then, profile and picture updates are limited to the owner or an admin by `users.profile.owner` since 6.9)*; account deletion raises `UserDeleted`.
* **Closes (partly):** 2.4, 6.3.
* **Status field:** `8.8`.

### Step 8.9 – Identity
* **Actions:** Identity.Domain free of framework packages (`AppUser : IdentityUser` moves to Infrastructure); `RefreshToken` aggregate with rotation and reuse detection; `AuthResult` → `Result` with codes (`identity.credentials.invalid`, `identity.email.not_confirmed`, `identity.user.locked_out`, `identity.user.email_taken`); emit `sub` next to `id`; missing use cases `LoginWithFacebook` (typed Graph client with resilience) and `AddPassword`; decide and record the fate of `Grant/RevokePostClaim` (superseded by 6.9); `UserRegistered` integration event.
* **Closes (partly):** 2.3, 6.2, 6.3.
* **Status field:** `8.9`.

### Step 8.10 – Media
* **Actions:** `MediaAsset` with a Guid v7 ID and a `now` parameter; server-generated `MediaKey` (no client `bucketPath`); size limits and a content-type allow-list verified by content sniffing; the storage adapter returns `Result` and lets transient faults surface; options validated on start.
* **Closes (partly):** 3.1.
* **Status field:** `8.10`.

### Step 8.11 – Notifications and Payments adapters
* **Actions:** OneSignal, MailKit and Payments adapters stop swallowing failures (transient → exception so the relay retries, permanent → `Result`); typed clients with `AddStandardResilienceHandler`; Payments never returns exception messages; options validated on start; adapter tests with fake handlers.
* **Closes (partly):** 5.1.
* **Status field:** `8.11`.

### Step 8.12 – Phase 8 exit gate
* **Actions:** verification-ladder level 4; the architecture baseline holds no Domain/Application entries; close 6.2, 6.3 and 6.4 once their remaining work is done.
* **Status field:** `8.12 – Phase 8 closed`.

---

## Phase 9 – Messaging and integration

> **Execution order:** 9.1 → 9.2 → 9.3 → 9.4 → 9.5 → 1.6 → 9.6 (messaging-integration skill; decisions D-MSG-1…3).

### Step 9.1 – Per-module transactional outbox and inbox
* **Goal:** an integration event commits in the same save as the change that caused it.
* **Actions:** `outbox_messages` and `inbox_messages` in every module schema (stable versioned type name from an event registry, JSON payload, `DateTimeOffset` timestamps, `Attempts`, `NextAttemptOnUtc`, `LastError`, `DeadLetteredOnUtc`); `OutboxWriter<TModuleDbContext>` used by domain-event handlers; delete `OutboxDbContext` and the global `IIntegrationEventPublisher`; one migration per module.
* **Acceptance:** integration tests — a command commits aggregate + outbox row atomically; a failure after the handler persists neither.
* **Status field:** `9.1`.

### Step 9.2 – Outbox relay and `IIntegrationEventBus`
* **Actions:** one `OutboxRelay<TModuleDbContext>` per module (`PeriodicTimer`, a scope per batch, due rows by `OccurredOnUtc`, publish then mark processed, exponential backoff, dead-letter after `MaxAttempts` with an Error log); `IIntegrationEventBus` with `InProcessIntegrationEventBus` (fresh scope per message); single-instance LINQ claiming recorded as a decision (`FOR UPDATE SKIP LOCKED` once scaled out).
* **Acceptance:** integration tests — the relay publishes and marks processed; a failing consumer is retried with backoff and dead-lettered after the maximum.
* **Closes:** 1.4, 6.5.
* **Status field:** `9.2`.

### Step 9.3 – Idempotent consumers between modules
* **Actions:** inbox `TryAcceptAsync(eventId, consumer)` plus a command of the consuming module, saved in one unit of work. Subscriptions: Users ← `UserRegistered` (seed profile), `UserDeleted`; Attendance ← `PostDeleted` (remove participations), `PostUpdated` (capacity guard); Announcements ← `PostDeleted`; Moderation ← `PostDeleted` / `UserDeleted` (close reports); Ratings ← `RatingCreated` / `RatingDeleted` (reputation projection).
* **Acceptance:** one duplicate-delivery test per consumer proves a single effect.
* **Closes:** 2.4, 4.1, 4.2, 4.3, 5.2 (together with their Phase 8 and 10 parts).
* **Status field:** `9.3`.

### Step 9.4 – Notifications consumers
* **Actions:** push through the OneSignal external user id for `AttendanceRequested` (host), `AttendanceAccepted` (attendee), `AnnouncementCreated` (accepted attendees, via Attendance.Contracts) and `PostDeleted` (attendees); e-mail for `UserRegistered` (confirmation link) and password reset; templates recovered from the legacy `wwwroot` at tag `pre-net10-baseline`; the push `data` keys are documented for BingoMobile.
* **Acceptance:** consumer tests with fake senders; a duplicate delivery sends once; a transient sender failure is retried by the relay.
* **Closes:** 5.1.
* **Status field:** `9.4`.

### Step 9.5 – Media pipeline (claim-check + Hangfire)
* **Actions:** the Step 3.5 design: Posts stores `tmp/` keys and `Pending` pictures; Media consumes the post event and enqueues a Hangfire job (never from a command handler); the job resizes with the image library, writes the final objects and raises `MediaProcessed` / `MediaFailed`; a Posts consumer marks pictures `Ready` / `Failed`; recurring `CleanupOrphanTmpUploads` and `RetryStuckPictures`; `PostDeleted` → S3 delete job.
* **Acceptance:** integration tests with a fake storage adapter for the ready, failed, retry and delete paths; create-post latency measured and recorded.
* **Closes:** 3.1, 3.5.
* **Status field:** `9.5`.

### Step 9.6 – Phase 9 exit gate
* **Actions:** verification-ladder level 4; the messaging-integration *Completion criterion* holds; Step 1.6 is ✅.
* **Status field:** `9.6 – Phase 9 closed`.

---

## Phase 10 – REST normalization and BingoMobile sync

> Each module step follows the rest-api-standards skill and the HopautArchitect *Contract sync procedure*: route constants per module (`XxxRoutes`), wire DTOs in `Modules.<X>.Api/Contracts/V1`, `TypedResults` unions, problem details with `errorCode`, authorization by default, paging, endpoint and contract tests, the OpenAPI snapshot, a handoff in `docs/mobile-handoffs/`, and the matching BingoMobile change on `dev` (routes in `lib/config/constants/api.dart`, models in `lib/data/models/`, calls in `lib/data/repositories/`, error mapping in `lib/services/dio_service.dart`). Target routes come from the contract inventory in `TODO.md`.
>
> **Acceptance for every module step:** the skill's *Completion criterion*; every inventory row of the module is `synced` (handoff `applied`) or `pending-mobile` with a recorded reason.

### Step 10.1 – API foundation: OpenAPI 3.1, versioning, JSON conventions
* **Actions:** `Microsoft.AspNetCore.OpenApi` (one document per version) and Scalar UI in Development; `Asp.Versioning.Http` + `Asp.Versioning.Mvc.ApiExplorer` with URL segment `/api/v{version:apiVersion}` (default 1); JSON camelCase, enums as camelCase strings, ID converters, ISO-8601 UTC times; problem details for binding failures; endpoint metadata conventions; `GenerateDocumentationFile` for `*.Api`; `docs/openapi/v1.json` snapshot with a comparison test.
* **Closes:** 6.10, 0.12 (Swagger criterion).
* **Status field:** `10.1`.

### Step 10.2 – Identity endpoints + BingoMobile
* **Actions:** register, login, Facebook login, refresh, forgot / reset / change / add password and confirm e-mail under `/api/v1/identity/` (kebab-case); token response and `identity.*` problem codes replace `FailReason`; JWT claims synced with `lib/data/models/identity.dart`.
* **Status field:** `10.2`.

### Step 10.3 – Users endpoints + BingoMobile
* **Actions:** `GET /api/v1/users/me`, `GET /api/v1/users/{userId}`, `PUT /api/v1/users/me`, `PUT` / `DELETE /api/v1/users/me/profile-picture` (multipart upload), `DELETE /api/v1/users/me`.
* **Status field:** `10.3`.

### Step 10.4 – Posts and tags endpoints + BingoMobile
* **Actions:** `GET /api/v1/posts` (spatial filter + cursor paging), `GET` / `PUT` / `DELETE /api/v1/posts/{postId}`, `POST /api/v1/posts` (multipart), `GET /api/v1/users/me/posts?status=`, `GET /api/v1/tags?search=`.
* **Status field:** `10.4`.

### Step 10.5 – Attendance endpoints + BingoMobile
* **Actions:** `GET /api/v1/posts/{postId}/attendees?status=`, `PUT` / `DELETE /api/v1/posts/{postId}/attendees/me`, `PATCH /api/v1/posts/{postId}/attendees/{userId}`, `GET /api/v1/users/me/attendances?status=`.
* **Status field:** `10.5`.

### Step 10.6 – Announcements endpoints + BingoMobile
* **Actions:** `GET` / `POST /api/v1/posts/{postId}/announcements`, `GET` / `PUT` / `DELETE /api/v1/announcements/{announcementId}`, inbox / outbox threads under `/api/v1/users/me/`.
* **Status field:** `10.6`.

### Step 10.7 – Ratings endpoints + BingoMobile
* **Actions:** `GET /api/v1/ratings/{ratingId}`, `GET /api/v1/users/{userId}/ratings`, `POST /api/v1/ratings`, `DELETE /api/v1/ratings/{ratingId}`.
* **Status field:** `10.7`.

### Step 10.8 – Moderation and bug-report endpoints + BingoMobile
* **Actions:** `POST /api/v1/posts/{postId}/reports`, `POST /api/v1/users/{userId}/reports`, `POST /api/v1/bug-reports` (multipart), admin report listing and resolution under the Admin policy.
* **Status field:** `10.8`.

### Step 10.9 – Media and Payments endpoint normalization
* **Actions:** align the remaining Media and Payments endpoints with the standards (201 + `Location`, problem details, typed responses); no BingoMobile consumer today.
* **Status field:** `10.9`.

### Step 10.10 – Phase 10 exit gate
* **Actions:** every contract-inventory row is `synced` or `pending-mobile` with a reason; the OpenAPI snapshot test is green; both ledgers list every handoff.
* **Status field:** `10.10 – Phase 10 closed`.

---

## Phase 11 – Release readiness

> After 11.1 and 11.2, Steps 6.11 (performance pass), 6.12 (worker-extraction decision) and 6.13 (Definition of Done) close the project; Step 11.3 (final merge into `master`) runs last.

### Step 11.1 – Legacy data conversion
* **Goal:** production data moves from the legacy `public` schema into the module schemas without loss.
* **Actions:** one idempotent SQL script per module (identity + users, posts including TPH → `event_type` and Unix seconds → `timestamptz`, attendance, announcements, ratings + reputation, moderation, bug reports), derived from the legacy model at tag `pre-net10-baseline` or from a schema-only dump supplied by the user; each script ends by moving every aggregate ID sequence past the imported keys (`SELECT setval('<schema>.<sequence>', COALESCE(MAX("Id"), 0) + 1, false)` for `posts.post_ids`, `attendance.participation_ids`, `announcements.announcement_ids`, `ratings.rating_ids`, `moderation.post_report_ids`, `moderation.user_report_ids`, `bug_reports.bug_ids`; decision D-019); runbook `docs/db/legacy-data-migration.md` with verification queries (row counts, spot checks).
* **Acceptance:** the scripts run twice against a local PostGIS container seeded with a synthetic legacy dataset and produce identical, verified results. Never run against a non-local database.
* **Closes:** the data parts of 2.2, 2.4, 3.3, 3.4.
* **Status field:** `11.1`.

### Step 11.2 – Strict build by default
* **Actions:** `TreatWarningsAsErrors`, `AnalysisLevel=latest-recommended`, `EnforceCodeStyleInBuild`, `NuGetAudit` + `NuGetAuditMode=all` in `Directory.Build.props`; fix warnings instead of suppressing them; `dotnet format` clean.
* **Acceptance:** `dotnet build Bingo.sln -c Release` has zero warnings; `dotnet format Bingo.sln --verify-no-changes` is clean.
* **Status field:** `11.2`.

### Step 11.3 – Final merge into `master` (runs last, after 6.13)
* **Goal:** the whole migration lands on `master` in one merge (user request 2026-11-17, decision D-015 in `TODO.md`); there are no intermediate merges.
* **Actions:**
  * Preconditions: every step in `hopaut-migration-status.md` is ✅ or ⏭️ with a reason; verification-ladder level 4 is green on `migrate/net10`; every handoff in `docs/mobile-handoffs/` is `applied`; `HANDOVER.md` is deleted.
  * Locally: `git switch master`, then `git merge --no-ff migrate/net10` with the message `Merge migrate/net10: Hopaut modular monolith on .NET 10`, which keeps every step commit in history. Re-run level 3 on the merge result. Resolve conflicts only after review; never rebase, squash or force.
  * Do not push; the user pushes and opens or merges any pull request. BingoMobile's `master` is outside the agent's remit, so the user merges BingoMobile `dev` there, or authorises it explicitly at that time.
* **Acceptance:** `master` contains `migrate/net10` through one `--no-ff` merge commit, and `dotnet build Bingo.sln` + `dotnet test Bingo.sln` are green on `master`.
* **Status field:** `11.3 – Migration merged`.

---

## Cross-phase invariants (apply to every step)

* No step is "done" until **`hopaut-migration-status.md` is updated** with: status (✅ / 🟡 in-progress / ❌ blocked), date, short note, optional commit hash.
* Never run `dotnet build` or `dotnet test` for the whole solution unless asked.
* When a step changes a public contract (DTO / route), bump nothing yet – we stay on `/api/v1` until a v2 is explicitly planned.
* When a step touches data, ship the EF migration **and** a one-shot data conversion script in the same commit.
* Every new module must come with: at least one Domain unit test, one Application handler test, one Infrastructure repository test (Testcontainers), and one Api endpoint test (WebApplicationFactory).
* **Supersessions (2026-11-17, HopautArchitect audit, see `TODO.md`):**
  * Domain events are dispatched **before** the module's single `SaveChangesAsync`, so they commit atomically with the aggregate and its outbox rows. This replaces the "after commit" wording in Steps 6.5 and 6.13 (D-004).
  * In HopautArchitect sessions builds and tests are mandatory gates (agent precedence), overriding "never run `dotnet build` / `dotnet test` unless asked".

---

## How to resume work in a new chat (copy-paste)

> Attach: `.github/copilot-instructions.md`, `hopaut-execution-plan.md`, `hopaut-migration-status.md`.
>
> Prompt: `/migrate #hopaut-execution-plan.md – continue from the next pending step in hopaut-migration-status.md. Do exactly one step. Then update hopaut-migration-status.md.`
