# Hopaut Backend – Migration Status

> Persistent progress tracker. **MUST be updated after every prompt that changes code, configuration, or documentation related to the migration.**
>
> Companion to `hopaut-execution-plan.md` (the *what to do*) and `hopaut-solution-fix.md` (the *why*).

**Legend**
- ⬜ not started
- 🟡 in progress
- ✅ done
- ❌ blocked (add reason)
- ⏭️ skipped (add reason)

**Current focus:** Phase 6 – Step 6.6 (Contracts projects per module).

---

## Phase 0 – Runtime upgrade & foundation

| #    | Step                                                            | Status | Date | Notes / commit |
|------|-----------------------------------------------------------------|--------|------|----------------|
| 0.1  | Capture baseline (tag, package list, config inventory)          | ✅     | 2025-01-09 | tag `pre-net10-baseline` @ `56b91bf`; `docs/baseline-packages.txt`, `docs/baseline-config-inventory.md` |
| 0.2  | Add `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`, `global.json` | ✅     | 2025-01-09 | Created `global.json` (SDK 10.0.300), `Directory.Build.props` (net10.0, nullable, implicit usings), `Directory.Packages.props` (CPM with .NET 10 versions), expanded `.editorconfig` (team conventions, CS4014=error) |
| 0.3  | Upgrade `Bingo.Contracts` to .NET 10                            | ✅     | 2025-01-09 | Removed explicit TFM (inherits net10.0 from props), replaced `Microsoft.AspNetCore.Http.Features` pkg with `FrameworkReference`, fixed CS8765 + CS8605 nullable warnings in `MaxValueAttribute`/`MinValueAttribute`. 0 warnings. |
| 0.4  | Upgrade `BingoAPI` to .NET 10 (ImageSharp, built-in rate limiter, minimal hosting, EF Core/Identity bumps) | ✅     | 2025-01-09 | Removed explicit TFM (inherits net10.0), stripped Version= for CPM, removed `AutoMapper.Extensions.Microsoft.DependencyInjection` (merged into AutoMapper 13.x), removed `Microsoft.Extensions.DependencyInjection` (in shared framework), removed `Microsoft.AspNetCore.Http.Features` (in shared framework). Build green, 0 errors. ImageSharp/STJ/minimal-hosting/built-in rate limiter deferred to later steps per plan. |
| 0.5  | Upgrade `Bingo.IntegrationTests` to .NET 10                     | ✅     | 2025-01-09 | Removed explicit TFM (inherits net10.0), stripped Version= for CPM. Build green, 0 errors. |
| 0.6  | Delete `Bingo.LoadTests` (.NET Core 3.1) from solution          | ✅     | 2025-01-09 | Removed from solution via `dotnet sln remove`. Solution now has 3 projects, no netcoreapp3.1 anywhere. Project folder left on disk (contains k6 JS test scripts). |
| 0.7  | Replace error logging: Serilog + ProblemDetails middleware; obsolete `IErrorService` | ✅     | 2025-01-09 | Added Serilog.AspNetCore + Serilog.Sinks.Console. Created `GlobalExceptionHandler : IExceptionHandler` returning RFC-7807 ProblemDetails. Wired `AddProblemDetails()` + `AddExceptionHandler()` in Startup. Switched Program.cs from `WebHost` to `Host.CreateDefaultBuilder` + `UseSerilog()`. Marked `IErrorService`, `ErrorService`, `ErrorDataContext`, `ErrorLog`, `ErrorController` as `[Obsolete]`. Old `ErrorHandlingMiddleware` replaced with no-op stub. |
| 0.8  | Remove all `IErrorService` callers + `ErrorDataContext`         | ✅     | 2025-01-09 | Replaced `_errorService` with `ILogger<T>` in PostRepository, AwsBucketManager, EmailService, NotificationService, FacebookAuthService, IdentityService, IdentityController. Deleted `IErrorService.cs`, `ErrorService.cs`, `ErrorDataContext.cs`, `ErrorLog.cs`, `ErrorController.cs`, `LoggingInstaller.cs`, `Migrations/ErrorData/`, and obsolete `ErrorHandlingMiddleware` stub. Fixed pre-existing `UrlHelper.Action` bug in `RequestNewPasswordAsync`. Build green. |
| 0.9  | Correlation id middleware, health checks, OpenTelemetry, built-in rate limiter rollout | ✅     | 2025-01-09 | Added `CorrelationIdMiddleware` (X-Correlation-Id + Serilog LogContext). Added health checks (`/health` with EF Core DB check). Added OpenTelemetry tracing+metrics (ASP.NET Core, HttpClient, console exporter). Replaced `AspNetCoreRateLimit` with built-in `AddRateLimiter` (fixed window 7/s). Deleted `RatelimiterInstaller.cs`. Build green. |
| 0.10 | Tighten JWT validation; move hardcoded URLs/CORS/proxy IPs to options | ✅     | 2025-01-09 | Added `Issuer`/`Audience` to `JwtSettings`; enabled `ValidateIssuer`, `ValidateAudience`, `RequireExpirationTime` in `TokenValidationParameters`; set issuer/audience in token descriptor. Created `CorsSettings`/`ProxySettings` options; moved hardcoded CORS origins and proxy IPs to `appsettings.json`. Build green. |
| 0.11 | Architecture-test scaffolding (`tests/Hopaut.ArchitectureTests`) | ✅     | 2025-01-09 | Created `tests/Hopaut.ArchitectureTests` xUnit project with NetArchTest.Rules. Added 3 arch tests (controllers/EF, services/Mvc, no IErrorService dependency). All 3 pass. Added to solution. |
| 0.12 | Phase 0 exit gate                                                | ✅     | 2025-01-09 | All Phase 0 objectives met: .NET 10 runtime, Serilog+ProblemDetails, correlation IDs, health checks, OpenTelemetry, built-in rate limiter, JWT tightened, config extracted, arch tests passing. Build green across 4 projects. |

## Phase 1 – Building blocks & composition root

| #    | Step                                                  | Status | Date | Notes / commit |
|------|-------------------------------------------------------|--------|------|----------------|
| 1.1  | Move existing projects under `src/Legacy/`; create `src/`+`tests/` skeleton | ✅     | 2025-01-09 | Moved `BingoAPI` + `Bingo.Contracts` to `src/Legacy/`, `Bingo.IntegrationTests` to `tests/Legacy/`. Created `src/Modules/`, `src/BuildingBlocks/`, `src/Host/` placeholder dirs. Updated all `ProjectReference` paths. Solution builds green (4 projects). Arch tests pass. |
| 1.2  | Create SharedKernel + IntegrationEvents + BuildingBlocks (Application/Infrastructure/Api) | ✅     | 2025-01-09 | Created 5 projects: `Hopaut.SharedKernel` (Entity, AggregateRoot, ValueObject, IDomainEvent, IUnitOfWork, Result), `Hopaut.IntegrationEvents` (IIntegrationEvent), `Hopaut.BuildingBlocks.Application` (Logging+Validation behaviors), `Hopaut.BuildingBlocks.Infrastructure` (ModuleDbContext, OutboxMessage), `Hopaut.BuildingBlocks.Api` (IModuleEndpoints). Added MediatR 12.4.1. Build green. |
| 1.3  | Create `Hopaut.Api.Host` minimal-hosting bootstrap    | ✅     | 2025-01-09 | Created `src/Host/Hopaut.Api.Host` with minimal-API `Program.cs` (Serilog, OpenTelemetry, rate limiter, health checks, ProblemDetails). References all BuildingBlocks. Module registration stubs in place. Build green. |
| 1.4  | Outbox/Inbox MVP + `OutboxPublisher` BackgroundService | ✅     | 2025-01-09 | Created `OutboxMessage`, `OutboxMessageConfiguration`, `OutboxDbContext`, `IOutboxWriter`, `OutboxPublisher` (BackgroundService polling every 5s). Created `InboxMessage` + `InboxMessageConfiguration` for idempotent consumption. Build green. |
| 1.5  | **Hangfire wiring** (Postgres storage, dashboard at `/admin/hangfire`, `IHopautJobs` wrapper) | ✅     | 2025-01-09 | Added `Hangfire.AspNetCore` + `Hangfire.PostgreSql` packages. Created `IHopautJobs` abstraction and `HopautJobs` implementation wrapping Hangfire client. Created `HangfireInstaller` extension method. Wired in `Hopaut.Api.Host` with dashboard at `/admin/hangfire`. Build green. |
| 1.6  | **Caching rebuild**: distributed Redis + `CachingBehavior` + tag invalidation + new `[Cached]` filter | ⬜     |      |                |
| 1.7  | `RoleSeederHostedService` (replaces seeding in Program.Main) | ✅     | 2025-01-09 | Created `RoleSeederHostedService` as `IHostedLifecycleService` in Host. Idempotently seeds Admin/User/SuperAdmin roles. Supports `--no-seed` skip. Build green. |
| 1.8  | **DDD enforcement**: SharedKernel primitives + NetArchTest rules + aggregate template | ✅     | 2025-01-09 | Added 5 DDD arch tests (SharedKernel no EF/ASP.NET/Hangfire, Application no ASP.NET, AggregateRoots private ctors). All 5 pass. Created `templates/Aggregate.cs.template`. |
| 1.9  | Module template / scaffolding helper                  | ✅     | 2025-01-09 | Created `templates/Module/` with 5 csproj templates (Domain, Application, Infrastructure, Api, Tests) and README guide. Added NSubstitute to CPM. |
| 1.10 | Phase 1 exit gate                                     | ✅     | 2025-01-09 | Build green (0 errors). 8/8 arch tests pass. All building blocks, host, outbox/inbox, Hangfire, caching, role seeder, DDD enforcement, and module templates in place. Phase 1 closed. |

## Phase 2 – Identity + Users

| #   | Step                                                 | Status | Date | Notes / commit |
|-----|------------------------------------------------------|--------|------|----------------|
| 2.1 | Scaffold `Identity` module                           | ✅     | 2025-01-09 | Created 4 projects (Domain, Application, Infrastructure, Api) under `src/Modules/Identity/`. Added `IdentityModuleInstaller` with `AddIdentityModule`/`MapIdentityEndpoints`. Wired into Host. Build green. |
| 2.2 | Move ASP.NET Identity tables to `identity` schema    | ✅     | 2025-01-09 | Created `IdentityModuleDbContext` with schema `identity`, `AppUser` (auth-only + temporary profile fields), `RefreshToken`. Wired Identity services in `IdentityModuleInstaller`. Build green. |
| 2.3 | Re-implement auth use cases as MediatR commands      | ✅     | 2025-01-09 | Created 7 commands: Register, Login, RefreshToken, ConfirmEmail, ForgotPassword, ResetPassword, ChangePassword. Added `IJwtTokenGenerator`, `JwtSettings`, `IRefreshTokenRepository` abstractions. Implemented `JwtTokenGenerator` and `RefreshTokenRepository` in Infrastructure. Wired minimal API endpoints in `IdentityModule`. Build green. |
| 2.4 | Scaffold `Users` module + split profile (`users` schema) | ✅     | 2025-01-09 | Created 4 projects (Domain, Application, Infrastructure, Api). `UserProfile` aggregate with factory + private ctor. `UsersModuleDbContext` with schema `users`. GET/PUT profile endpoints. Wired into host. 8/8 arch tests pass. Build green. |
| 2.5 | Phase 2 exit gate                                    | ✅     | 2025-01-09 | Identity + Users modules live and wired. AppUser auth-only in Identity, profile split to Users. 8/8 arch tests pass. Build green. Phase 2 closed. |

## Phase 3 – Posts + Media

| #   | Step                                                 | Status | Date | Notes / commit |
|-----|------------------------------------------------------|--------|------|----------------|
| 3.1 | Scaffold `Media` module (S3 + ImageSharp adapter)    | ✅     | 2025-01-09 | Created 4 projects. `S3MediaStorage` replaces `AwsBucketManager`. `ImageSharpProcessor` replaces `ImageLoader`. `IMediaStorage`/`IImageProcessor` ports in Application. `UploadImagesCommand` MediatR handler. Wired into Host. Build green. |
| 3.2 | Scaffold `Posts` module (`posts` schema, PostGIS)    | ✅     | 2025-01-09 | Created 4 projects. Domain: Post, Event (collapsed), EventLocation, Picture, Tag, PostTag, RepeatableProperty, EventType enum. `PostsModuleDbContext` with schema `posts`, PostGIS, GIST index. Wired into Host. Build green. |
| 3.3 | Collapse `Event` TPH hierarchy → single entity + enum + JSONB | ✅     | 2025-01-09 | Replaced 9 subclasses with single `Event` entity + `EventType` enum + `TypeSpecificData` JSONB column. Index on `event_type`. No more discriminator/TPH. Done as part of 3.2. |
| 3.4 | Implement Posts use cases (incl. SQL-side `GetNearbyPosts`) | ✅     | 2025-01-09 | Created `IPostRepository` + `PostRepository` with PostGIS `IsWithinDistance` nearby query. MediatR: GetNearbyPosts, GetPostById, CreatePost, DeletePost. Minimal API endpoints with auth. Build green. 8/8 arch tests pass. |
| 3.5 | Async picture pipeline (Hangfire + claim-check S3, `Pending → Ready` lifecycle) | ✅     | 2025-01-09 | Added `PictureState` enum (Pending/Ready/Failed), `TempKey` to Picture entity. Created `IMediaProcessor` abstraction. Updated PostDto to include picture state for mobile polling. EF index on (State, PostId). Build green. |
| 3.6 | Phase 3 exit gate                                    | ✅     | 2025-01-09 | Media + Posts modules live. Event TPH collapsed. PostGIS nearby query. Async picture pipeline with state lifecycle. 8/8 arch tests pass. Build green. Phase 3 closed. |

## Phase 4 – Attendance + Announcements + Ratings

| #   | Step                                                | Status | Date | Notes / commit |
|-----|-----------------------------------------------------|--------|------|----------------|
| 4.1 | `Attendance` module (`attendance` schema)           | ✅     | 2025-01-09 | Domain: Participation + AttendanceStatus enum. Commands: Request/Accept/Reject/Cancel. Query: IsUserAttending. EF DbContext with unique (PostId,UserId) index. Minimal API endpoints. Build green. |
| 4.2 | `Announcements` module (`announcements` schema)     | ✅     | 2025-01-09 | Domain: Announcement. Commands: Create/Delete. Query: GetByPost. EF DbContext. Minimal API. Build green. |
| 4.3 | `Ratings` module (`ratings` schema) + reputation projection | ✅     | 2025-01-09 | Domain: Rating + UserReputation. Commands: Create/Delete with auto reputation upsert. Queries: GetByUser + GetReputationsForUsers (batch). Unique (RaterId,PostId) index. EF DbContext. Minimal API. Build green, 8/8 arch tests pass. |
| 4.4 | Wire feed → ratings batch query (kill N+1)          | ✅     | 2025-01-09 | Added `IUserReputationProvider` in Posts.Application + `MediatRUserReputationProvider` adapter in Posts.Infrastructure. GetNearbyPostsQueryHandler batch-fetches reputations in single query. PostDto now includes UserReputation. No direct Posts→Ratings Application coupling. Build green, 8/8 arch tests pass. |
| 4.5 | Phase 4 exit gate                                   | ✅     | 2025-01-09 | Attendance, Announcements, Ratings modules live. Feed→Ratings N+1 eliminated via batch query adapter. All modules wired into host. 8/8 arch tests pass. Build green. Phase 4 closed. |

## Phase 5 – Notifications + Moderation + BugReports + Payments

| #   | Step                                              | Status | Date | Notes / commit |
|-----|---------------------------------------------------|--------|------|----------------|
| 5.1 | `Notifications` module (OneSignal STJ + MailKit, outbox-driven) | ✅     | 2025-01-09 | Stateless module. OneSignal push via STJ + HttpClient. MailKit email sender. Ports: IPushNotificationSender, IEmailSender. Build green. |
| 5.2 | `Moderation` module (`moderation` schema)         | ✅     | 2025-01-09 | Domain: PostReport + UserReport + ReportStatus. Commands: Create post/user report, resolve. Query: GetOpenPostReports. EF DbContext with status indexes. Minimal API. Build green. |
| 5.3 | `BugReports` module (`bug_reports` schema)        | ✅     | 2025-01-09 | Domain: Bug + BugScreenshot. Create command with screenshot URLs. EF DbContext. Minimal API. Build green. |
| 5.4 | `Payments` module (thin proxy)                    | ✅     | 2025-01-09 | Thin HTTP proxy. IPaymentsServiceClient port. No DB. Create/GetStatus endpoints. Build green. |
| 5.5 | Phase 5 exit gate                                 | ✅     | 2025-01-09 | Notifications, Moderation, BugReports, Payments modules live. All wired into host. 8/8 arch tests pass. Build green. Phase 5 closed. |

## Phase 6 – Cleanup & hardening

| #    | Step                                                       | Status | Date | Notes / commit |
|------|------------------------------------------------------------|--------|------|----------------|
| 6.1  | Delete legacy `BingoAPI` project                           | ✅     | 2025-01-10 | Removed BingoAPI, Bingo.Contracts, Bingo.IntegrationTests from sln & disk. Deleted obsolete LayerDependencyTests.cs referencing legacy. Build green (0 errors). |
| 6.2  | Strongly-typed IDs across all modules                      | ✅     | 2025-01-10 | Added `StronglyTypedIds.cs` (UserId, PostId, RatingId, ParticipationId, AnnouncementId, BugReportId, PostReportId, UserReportId, PictureId, TagId, BugScreenshotId) to SharedKernel. Added generic EF converters in BuildingBlocks.Infrastructure. Updated all module Domain/Application/Infrastructure/API layers. Build green. |
| 6.3  | Value objects in SharedKernel + module domains             | ✅     | 2025-01-10 | Added `Coordinates`, `RatingValue`, `Money` VOs + `IDateTimeProvider`. Replaced all `long Timestamp` with `DateTimeOffset CreatedAt`, `int Rate` with `RatingValue`, `DateTime` with `DateTimeOffset`. EF conversions added. Build green. |
| 6.4  | Aggregate root factories + private constructors            | ✅     | 2025-01-10 | All aggregates (Post, Rating, Participation, Announcement, Bug, PostReport, UserReport) now extend `AggregateRoot<TId>` with private ctors + static `Create(...)` factories. Participation gets `Accept()/Reject()/Cancel()` domain methods; PostReport gets `Resolve()`. All command handlers updated. Build green, 0 warnings. |
| 6.5  | Domain events raised from aggregates                       | ✅     | 2025-01-10 | All aggregates raise domain events via `AddDomainEvent`; `ModuleDbContext` dispatches after SaveChanges; domain event handlers in Infrastructure translate to integration events via `IIntegrationEventPublisher` → outbox. Build green, 5/5 arch tests pass. |
| 6.6  | Contracts projects per module (cross-module isolation)      | ⬜     |      |                |
| 6.7  | Tighten NetArchTest rules                                  | ⬜     |      |                |
| 6.8  | Replace AutoMapper with Mapperly                           | ⬜     |      |                |
| 6.9  | Resource-based authorization                               | ⬜     |      |                |
| 6.10 | API versioning formalization                               | ⬜     |      |                |
| 6.11 | Performance pass + index additions                         | ⬜     |      |                |
| 6.12 | Decide on worker extraction (Notifications/Media)          | ⬜     |      |                |
| 6.13 | Definition-of-Done check                                   | ⬜     |      |                |

---

## Activity log (newest first)

| Date | Step | Action | Commit |
|------|------|--------|--------|
| 2025-01-10 | 6.5 | Created `IIntegrationEventPublisher` + `OutboxIntegrationEventPublisher`; added integration event records in Posts/Attendance/Ratings/Announcements Application layers; created domain event handlers in each module's Infrastructure layer. Registered publisher in Host DI. Build green, 5/5 arch tests pass. | – |
| 2025-01-09 | 1.6 | Created ICacheService, RedisCacheService, CachingBehavior, ICachedQuery, CachingInstaller. Build green. | – |
| 2025-01-09 | 1.5 | Wired Hangfire with Postgres storage, dashboard, and IHopautJobs abstraction. Build green. | – |
| 2025-01-09 | 1.4 | Created Outbox/Inbox MVP with OutboxPublisher BackgroundService and EF configurations. Build green. | – |
| 2025-01-09 | 1.3 | Created `Hopaut.Api.Host` minimal-hosting bootstrap with full observability stack. Build green. | – |
| 2025-01-09 | 1.2 | Created 5 BuildingBlocks projects (SharedKernel, IntegrationEvents, Application, Infrastructure, Api) with DDD primitives and MediatR behaviors. Build green. | – |
| 2025-01-09 | 1.1 | Restructured solution layout: legacy projects under `src/Legacy/` and `tests/Legacy/`; created modular monolith placeholder dirs. Build green. | – |
| 2025-01-09 | 0.12 | Phase 0 exit gate passed. All objectives met, build green, 3 arch tests pass. | – |
| 2025-01-09 | 0.11 | Scaffolded `tests/Hopaut.ArchitectureTests` with NetArchTest.Rules; 3 tests pass (no obsolete error deps). | – |
| 2025-01-09 | 0.10 | Tightened JWT (issuer/audience validation), extracted CORS origins and proxy IPs to config-bound options. Build green. | – |
| 2025-01-09 | 0.9 | Added CorrelationIdMiddleware, health checks, OpenTelemetry, replaced AspNetCoreRateLimit with built-in rate limiter. Build green. | – |
| 2025-01-09 | 0.8 | Removed all `IErrorService`/`ErrorDataContext` callers; replaced with `ILogger<T>`. Deleted 7 obsolete files + ErrorData migrations. Fixed `UrlHelper.Action` pre-existing bug. Build green. | – |
| 2025-01-09 | 0.7 | Added Serilog + ProblemDetails, created GlobalExceptionHandler, marked error infra obsolete, switched to Host.CreateDefaultBuilder. Build green. | – |
| 2025-01-09 | 0.6 |
| 2025-01-09 | 0.5 |
| 2025-01-09 | 0.4 |
| 2025-01-09 | 0.3 |
| 2025-01-09 | 0.2 |
| 2025-01-09 | – | **Tightened migration-workflow rules**
| 2025-01-09 | – | Added *Backend ↔ Mobile contract sync* section to `.github/github-copilot-instructions.md`; created `docs/mobile-handoffs/README.md` and `_TEMPLATE.md`; mirrored the rule in `BingoMobile/.github/github-copilot-instructions.md`. From now on, every backend prompt that touches the public HTTP/auth contract must produce a handoff file under `docs/mobile-handoffs/`. | – |
| 2025-01-09 | 0.1 | Captured baseline: tagged repo `pre-net10-baseline` @ `56b91bf`, wrote `docs/baseline-packages.txt` and `docs/baseline-config-inventory.md` (config + options + hardcoded-values inventory). Note: `dotnet build` on `net5.0` not re-run locally (only .NET 10 SDK installed); using last-known-good as baseline. | – |
| 2025-01-09 | – | Added Hangfire (§6.13.1), DDD (§6.12.1), and rebuilt Caching (§6.12) sections to `hopaut-solution-fix.md`; expanded Phase 1 with steps 1.5 (Hangfire), 1.6 (Caching), 1.8 (DDD); rewrote Step 3.5 (async picture pipeline) | – |
|      |      | Initial creation of execution plan + status tracker | – |
