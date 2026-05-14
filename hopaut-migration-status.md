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

**Current focus:** Phase 1 – Step 1.1 (Move existing projects under `src/Legacy/`; create `src/`+`tests/` skeleton).

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
| 1.1  | Move existing projects under `src/Legacy/`; create `src/`+`tests/` skeleton | ⬜     |      |                |
| 1.2  | Create SharedKernel + IntegrationEvents + BuildingBlocks (Application/Infrastructure/Api) | ⬜     |      |                |
| 1.3  | Create `Hopaut.Api.Host` minimal-hosting bootstrap    | ⬜     |      |                |
| 1.4  | Outbox/Inbox MVP + `OutboxPublisher` BackgroundService | ⬜     |      |                |
| 1.5  | **Hangfire wiring** (Postgres storage, dashboard at `/admin/hangfire`, `IHopautJobs` wrapper) | ⬜     |      |                |
| 1.6  | **Caching rebuild**: distributed Redis + `CachingBehavior` + tag invalidation + new `[Cached]` filter | ⬜     |      |                |
| 1.7  | `RoleSeederHostedService` (replaces seeding in Program.Main) | ⬜     |      |                |
| 1.8  | **DDD enforcement**: SharedKernel primitives + NetArchTest rules + aggregate template | ⬜     |      |                |
| 1.9  | Module template / scaffolding helper                  | ⬜     |      |                |
| 1.10 | Phase 1 exit gate                                     | ⬜     |      |                |

## Phase 2 – Identity + Users

| #   | Step                                                 | Status | Date | Notes / commit |
|-----|------------------------------------------------------|--------|------|----------------|
| 2.1 | Scaffold `Identity` module                           | ⬜     |      |                |
| 2.2 | Move ASP.NET Identity tables to `identity` schema    | ⬜     |      |                |
| 2.3 | Re-implement auth use cases as MediatR commands      | ⬜     |      |                |
| 2.4 | Scaffold `Users` module + split profile (`users` schema) | ⬜     |      |                |
| 2.5 | Phase 2 exit gate                                    | ⬜     |      |                |

## Phase 3 – Posts + Media

| #   | Step                                                 | Status | Date | Notes / commit |
|-----|------------------------------------------------------|--------|------|----------------|
| 3.1 | Scaffold `Media` module (S3 + ImageSharp adapter)    | ⬜     |      |                |
| 3.2 | Scaffold `Posts` module (`posts` schema, PostGIS)    | ⬜     |      |                |
| 3.3 | Collapse `Event` TPH hierarchy → single entity + enum + JSONB | ⬜     |      |                |
| 3.4 | Implement Posts use cases (incl. SQL-side `GetNearbyPosts`) | ⬜     |      |                |
| 3.5 | Async picture pipeline (Hangfire + claim-check S3, `Pending → Ready` lifecycle) | ⬜     |      |                |
| 3.6 | Phase 3 exit gate                                    | ⬜     |      |                |

## Phase 4 – Attendance + Announcements + Ratings

| #   | Step                                                | Status | Date | Notes / commit |
|-----|-----------------------------------------------------|--------|------|----------------|
| 4.1 | `Attendance` module (`attendance` schema)           | ⬜     |      |                |
| 4.2 | `Announcements` module (`announcements` schema)     | ⬜     |      |                |
| 4.3 | `Ratings` module (`ratings` schema) + reputation projection | ⬜     |      |                |
| 4.4 | Wire feed → ratings batch query (kill N+1)          | ⬜     |      |                |
| 4.5 | Phase 4 exit gate                                   | ⬜     |      |                |

## Phase 5 – Notifications + Moderation + BugReports + Payments

| #   | Step                                              | Status | Date | Notes / commit |
|-----|---------------------------------------------------|--------|------|----------------|
| 5.1 | `Notifications` module (OneSignal STJ + MailKit, outbox-driven) | ⬜     |      |                |
| 5.2 | `Moderation` module (`moderation` schema)         | ⬜     |      |                |
| 5.3 | `BugReports` module (`bug_reports` schema)        | ⬜     |      |                |
| 5.4 | `Payments` module (thin proxy)                    | ⬜     |      |                |
| 5.5 | Phase 5 exit gate                                 | ⬜     |      |                |

## Phase 6 – Cleanup & hardening

| #   | Step                                              | Status | Date | Notes / commit |
|-----|---------------------------------------------------|--------|------|----------------|
| 6.1 | Delete legacy `BingoAPI` project                  | ⬜     |      |                |
| 6.2 | Tighten NetArchTest rules                         | ⬜     |      |                |
| 6.3 | Performance pass + index additions                | ⬜     |      |                |
| 6.4 | Decide on worker extraction (Notifications/Media) | ⬜     |      |                |
| 6.5 | Definition-of-Done check                          | ⬜     |      |                |

---

## Activity log (newest first)

| Date | Step | Action | Commit |
|------|------|--------|--------|
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
