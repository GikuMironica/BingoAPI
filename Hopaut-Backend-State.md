# Hopaut Backend – Current State Analysis (BingoAPI)

> Scope: full-solution architectural and design review of the `BingoAPI` repository (geosocial networking back-end for the Hopaut Flutter mobile app).
> Goal: produce a faithful "as-is" picture before proposing the modular-monolith / Clean Architecture refactor (see `hopaut-solution-fix.md`).

---

## 1. Solution Layout

```
BingoAPI.sln
├── BingoAPI/                 (net5.0  – ASP.NET Core Web API, the monolith)
├── Bingo.Contracts/          (net5.0  – DTOs: Requests / Responses / ApiRoutes)
├── Bingo.IntegrationTests/   (net5.0  – xUnit + WebApplicationFactory)
└── Bingo.LoadTests/          (netcoreapp3.1 – placeholder, effectively empty)
```

Target frameworks: **.NET 5** (out of support since May-2022) and **.NET Core 3.1** (out of support since Dec-2022). The whole solution is on unsupported runtimes.

### 1.1 BingoAPI internal folder layout

```
BingoAPI/
├── Controllers/             11 controllers, fat – contain business + auth + workflow logic
├── Models/                  EF Core entities (Post, Event hierarchy, AppUser, Rating, …)
│   └── SqlRepository/       Repository<T> implementations + interfaces (data access)
├── Data/                    DataContext, ErrorDataContext (two DbContexts)
├── Services/                Application/infra services jumbled together
│                            (IdentityService, EmailService, AwsBucketManager,
│                             NotificationService (OneSignal), FacebookAuthService,
│                             ImageLoader, UriService, ErrorService, etc.)
├── Domain/                  Plain DTO/result/filter classes (mis-named “Domain”)
├── CustomMapper/            Hand-rolled mappers + AutoMapper coexisting
├── MappingProfiles/         AutoMapper Profiles
├── Validators/              FluentValidation validators
├── Filters/                 ValidationFilter
├── Middleware/              ErrorHandlingMiddleware
├── Cache/                   CachedAttribute, RedisCacheSettings
├── Helpers/                 Static helpers (PaginationHelpers, RoleCheckingHelper)
├── Installers/              IInstaller pattern (Db, Mc, Cache, Swagger, Facebook, Logging, …)
├── Extensions/              GeneralExtensions, InstallerExtensions
├── ExternalLogin/           Facebook DTOs
├── Options/                 Strongly-typed settings (Jwt, Aws, Email, …)
├── CustomValidation/        UpdatedPostDetailsWatcher
├── Migrations/              ~30 EF migrations (also a sub-folder for ErrorData)
└── Startup.cs / Program.cs  Classic .NET 5 Startup hosting
```

There is **one project**, `BingoAPI`, that contains everything: HTTP, business, persistence, integrations, file IO, cache, identity, push notifications, e-mail, Facebook auth, S3, payments proxy. It is a textbook "big ball of mud" monolith.

---

## 2. Architecture Pattern Used

The intended pattern is a **layered (n-tier) monolith**:

```
Controllers → Services / Repositories → DataContext (EF Core) → PostgreSQL (PostGIS)
                                     ↘ External: AWS S3, OneSignal, Facebook, SMTP, MyServices (payments)
```

In practice the layering is **not enforced**:

* Controllers reach directly into repositories *and* `UserManager<AppUser>` *and* services *and* AutoMapper *and* domain helpers.
* Repositories call `UserManager` and `IErrorService`; they are not pure data-access.
* Services depend on `DataContext` directly (`IdentityService` injects `DataContext`).
* `Models/SqlRepository/*` mixes EF entities (the model) with EF queries (the repository) in the same folder/namespace.

So while the name suggests "Models + Repositories", architecturally there is no meaningful boundary – it is one assembly, one DI graph, one DbContext, with circular conceptual dependencies.

### 2.1 Cross-cutting infrastructure

* **DI**: built-in MS DI, with a custom *Installer* convention (`IInstaller` + assembly scanning in `InstallerExtensions`). Reasonable, but used as an excuse to avoid composition-root discipline.
* **Validation**: FluentValidation 9 + a global `ValidationFilter`. Good intent.
* **Caching**: custom `CachedAttribute` + Redis (`Microsoft.Extensions.Caching.StackExchangeRedis`).
* **Rate limiting**: AspNetCoreRateLimit (IP-based).
* **Logging**: only Console + Debug providers; no Serilog/structured logging; no correlation id.
* **Error handling**: `ErrorHandlingMiddleware` writes to a *second* `ErrorDataContext` (separate DB) – questionable choice, see §5.
* **Auth**: ASP.NET Identity + JWT bearer, Facebook external login, refresh tokens persisted in `DataContext`.
* **Docs**: Swashbuckle Swagger (good).

---

## 3. Domain Model (Bounded-context view)

Although there is no explicit DDD, the entities cluster naturally into the following implicit contexts:

| Implicit context | Entities |
|---|---|
| **Identity / Users** | `AppUser` (extends `IdentityUser`), `RefreshToken`, ASP.NET Identity tables |
| **Posts / Events** (the “feed”) | `Post`, `Event` (TPH abstract), `HouseParty`, `Bar`, `Club`, `CarMeet`, `BikerMeet`, `BicycleMeet`, `Sport (Marathons)`, `StreetParty`, `Other`, `EventLocation`, `Picture`, `RepeatableProperty`, `Tag`, `PostTags` |
| **Attendance** | `Participation` (Post ↔ AppUser, with state) |
| **Announcements** | `Announcement` (host → attendees) |
| **Ratings** | `Rating` (rater → host) |
| **Moderation** | `Report` (post report), `UserReport` |
| **Bug reports** | `Bug`, `BugScreenshot` |
| **Errors / observability** | `ErrorLog` (separate `ErrorDataContext`) |
| **Payments** (proxy only) | no entities; `PaymentsController` forwards to an external "MyServices" gateway |

### 3.1 Persistence model highlights

* **TPH inheritance** for `Event` with discriminator `event_type` and 9 concrete subclasses. Slot logic via `virtual GetSlotsIfAny()`.
* **PostGIS** spatial indexing via `NetTopologySuite` on `EventLocation.Location` (good, real domain need).
* `Post` has an `ActiveFlag` (int, used as bool) – primitive obsession.
* Times are `Int64` Unix seconds everywhere instead of `DateTime`/`DateTimeOffset` – pervasive primitive obsession and "time math" duplicated across controllers/repositories.
* All EF relationships are configured fluently in `DataContext.OnModelCreating` (one giant method).
* All migrations live next to the API; some old ones are excluded via `<Compile Remove>` in the csproj.
* Two `DbContext`s sharing the same provider (Identity tables get duplicated – `ErrorDataContext` also inherits `IdentityDbContext`!).

---

## 4. Good Parts (what to keep / build on)

These are genuinely sensible things already in the codebase:

1. **Contracts project** (`Bingo.Contracts`) cleanly separates wire DTOs and `ApiRoutes` constants. This is the seed of a Public API layer.
2. **PostGIS + NetTopologySuite** for "events near me" – correct technology choice for a geosocial feed.
3. **Strongly-typed options pattern** (`IOptions<T>`) for Jwt / Aws / Email / OneSignal / Environment / EventTypes / Swagger.
4. **`IInstaller` convention + assembly scan** keeps `Startup.ConfigureServices` short and modular per concern.
5. **Global `ValidationFilter` + FluentValidation** removes per-controller `ModelState` boilerplate – correct pattern.
6. **`CachedAttribute` filter + Redis** for response caching is a clean, declarative cross-cutting solution.
7. **Custom claims-based authorization policy** (`CreateEditPost`) shows correct ASP.NET authorization usage.
8. **Refresh-token rotation + JWT** is implemented correctly enough for a v1.
9. **Forwarded headers + HSTS + HTTPS redirect** are configured.
10. **Swagger** with XML comments enabled and `GenerateDocumentationFile=true`.
11. Has an **integration test project** with a `WebApplicationFactory` setup, which gives a launching pad for refactor-with-confidence.

---

## 5. Flaws, Anti-Patterns & Pattern Breaches

The following list is exhaustive – we will use it to drive the refactor backlog in `hopaut-solution-fix.md`.

### 5.1 Architectural / structural

| # | Problem | Evidence | Why it hurts |
|---|---|---|---|
| A1 | **Single-project monolith**, no module boundaries | All ~140 files under `BingoAPI/` | Any change can touch anything; tests broaden; merge conflicts; no enforceable contracts |
| A2 | **Anaemic “Domain” folder** – it actually contains DTOs/result/filter classes (`AuthenticationResult`, `PaginationFilter`, `GetPostsFilter`, `ImageUploadResult`, …), not domain logic | `BingoAPI/Domain/*` | Mis-named layer; no real domain layer exists |
| A3 | **Models = EF entities = "domain"** | `BingoAPI/Models/*` referenced by controllers, services, repos, mappers, AutoMapper | EF model leaks into the HTTP layer; cannot evolve persistence and domain independently |
| A4 | **No application service layer** – business workflows live in controllers | `PostController.Create/Update/Delete` orchestrate user lookup, role check, image processing, S3, repo, notifications, mappers all inline | Controllers are 300–500 LOC; untestable without HTTP context |
| A5 | **Two `DbContext` instances** (`DataContext`, `ErrorDataContext`) both inheriting `IdentityDbContext` | `Data/ErrorDataContext.cs` | Identity schema duplicated; no transactional consistency; confusing |
| A6 | **No CQRS / no MediatR**, but write-paths are heavy and would benefit from explicit commands and pipeline behaviours (validation, logging, transactions) | All controllers | Cross-cutting concerns are repeated in every action |
| A7 | **No outbox / no message bus**: side-effects (e-mail, push, S3 cleanup) are fired in-process inside the same request, often *after* DB commit, with no retry, no idempotency | `IdentityService.RegisterAsync`, `PostController.Update/Delete` | Lost notifications on failure; partial state on crash mid-action |
| A8 | **Synchronous fan-out to external systems** (S3, OneSignal, SMTP, Facebook Graph, MyServices) inside HTTP requests | `AwsBucketManager`, `NotificationService`, `EmailService` | High p95 latency; cascading failures; back-pressure on user requests |
| A9 | **Out-of-support TFMs** (.NET 5 + .NET Core 3.1) | `*.csproj` | No security patches; can't use modern packages, minimal hosting model, generic-host metrics, etc. |
| A10 | **Composition root scattered** across 9 installers but each installer reaches into multiple concerns (e.g. `McInstaller` registers MVC, JWT, AuthZ policies, `IIdentityService`, `IUriService`, HttpClient) | `Installers/McInstaller.cs` | Defeats the purpose of `IInstaller` per-concern split |
| A11 | **TPH + 9 subclasses + magic-string discriminators** with no polymorphic behaviour beyond `GetSlotsIfAny()` | `DataContext.OnModelCreating`, `Models/*Meet*.cs`, etc. | Cost of TPH is paid (sparse table) without the OO benefits; type filter implemented as `GetType().Name == "HouseParty"` string comparisons in `PostRepository.FilterByType` |
| A12 | **In-memory filtering after a paginated query** | `PostRepository.GetAllAsync` then `FilterByType` `RemoveAll` | Pagination is broken: filters are applied *after* the page is materialised |
| A13 | **`UserManager<AppUser>` injected into controllers** for read-only profile lookups | `ProfileController`, `UserController`, `PostController`, … | Controllers couple to Identity infra; fat constructors (8–15 deps). |

### 5.2 Repository / data-access

| # | Problem | Evidence |
|---|---|---|
| D1 | **Repository pattern over `IQueryable`/EF without a Unit-of-Work** – every repo calls `SaveChangesAsync()` independently | `PostRepository.AddAsync`, `EventAttendanceRepository`, … |
| D2 | **Manual `BeginTransactionAsync` + `CommitTransactionAsync`** wrapping a single `SaveChangesAsync` call | `PostRepository.AddAsync/UpdateAsync` | Transaction wraps nothing extra; duplicates EF behaviour |
| D3 | **Catch-all `try/while(tryAgain)` retry loop swallowing all exceptions** in `AddAsync`/`UpdateAsync` | `PostRepository.AddAsync` | Hides real errors; treats every exception as "duplicate tag"; no logging of non-tag errors |
| D4 | **`IRepository<T>` over-generic interface** with `Task<bool> AddAsync(T)` returning a bool from `SaveChanges > 0` | `Models/SqlRepository/IRepository.cs` | Booleans hide failure reasons; encourages “if-not-result-return-BadRequest” without context |
| D5 | **N+1 queries** in controller loops calling repo per item | `PostController.GetMyActiveEvents`/`GetAll` → `await _ratingRepository.GetUserRating(post.UserId)` inside `foreach` |
| D6 | **`AsNoTracking` mixed with later updates** and inconsistent eager-loading strategy | `PostRepository.GetAllAsync` |
| D7 | **Hand-rolled tag de-dup with retry** instead of an upsert / unique constraint handled at SQL level | `PostRepository.AddNewTagsAsync` + `HandleInsertTagException` |
| D8 | **Time stored as Unix seconds (`Int64`)** with arithmetic like `15778476 + DateTimeOffset.UtcNow.ToLocalTime().ToUnixTimeSeconds()` | `PostController.GetAll` | Magic numbers (`15778476` ≈ 6 months in seconds); timezone bugs |
| D9 | **`UserManager` injected into a repository** | `PostRepository` ctor | Repository now needs an HTTP/Identity stack to instantiate, breaks unit testability |
| D10 | **No async cancellation tokens** anywhere in repos/controllers | All async methods | Cannot cooperatively cancel slow queries |
| D11 | **EF `Include` chains are duplicated** across repo methods | `PostRepository.GetAllAsync` (two near-identical branches for `tag == "%"`) |

### 5.3 Controllers / API layer

| # | Problem | Evidence |
|---|---|---|
| C1 | **God controllers** – `PostController` is ~500 LOC and orchestrates auth, validation, mapping, image IO, S3, repository, notifications | `PostController.cs` |
| C2 | **Authorization done by ad-hoc method calls** (`IsPostOwnerOrAdminAsync`, `RoleCheckingHelper.IsUserAdmin`) instead of authorization policies / requirements | `PostController.Update/Delete`, `UserController.Get` |
| C3 | **`StatusCode(403, new SingleError {...})`** copy-pasted everywhere instead of `Forbid()` / `Problem()` | most controllers |
| C4 | **Inheriting from `Controller`** (which brings View support) instead of `ControllerBase` for pure APIs | every controller |
| C5 | **Default MVC route `{controller}/{action}/{id?}` left enabled** in `Startup.Configure` even though all routes are attribute-routed | `Startup.cs` |
| C6 | **CORS hardcoded to localhost** (`http://localhost:4200`, `http://localhost:3000`) | `Startup.ConfigureServices` |
| C7 | **WebPortal URLs hardcoded as `const string`** (`https://localhost:3000/account/...`) | `IdentityController` |
| C8 | **Magic “Reason_1 / Reason_2” strings** as user-visible error messages | `PostController.Create` |
| C9 | **Returning 400 from `_postRepository.AddAsync` returning false** – no information to the caller | many controllers |
| C10 | **`[FromForm]` for create/update post but `[FromBody]` elsewhere** with no consistent media-type story | `PostController` |

### 5.4 Identity / security

| # | Problem | Evidence |
|---|---|---|
| S1 | **JWT settings**: `ValidateIssuer = false`, `ValidateAudience = false`, `RequireExpirationTime = false`, signing secret read from config as plain string | `McInstaller` |
| S2 | **`IUrlHelper` injected in `IdentityService`** to build confirmation URLs – brittle, wrong layer | `IdentityService` ctor |
| S3 | **`HttpContext` reached in services** via `IHttpContextAccessor` to get user id / log errors | `AwsBucketManager`, `NotificationService`, `IdentityService` |
| S4 | **No anti-forgery / no CSRF strategy** documented (mobile-only is OK, but web portal exists) |
| S5 | **Lockout = 15 attempts / 15 min** – generous; password requirements weakened (`RequireDigit = false`, `RequireNonAlphanumeric = false`) | `DbInstaller` |
| S6 | **Password reset URL composed in controller** with hardcoded base URL | `IdentityController` |
| S7 | **Facebook secret & AWS keys read from `IConfiguration` directly** (no Key Vault / Secret Manager wired in production) | `Options/*Settings.cs` |
| S8 | **Forwarded headers**: `KnownProxies` hardcoded to `10.0.0.100` in `Startup` | `Startup.ConfigureServices` |

### 5.5 Cross-cutting / observability

| # | Problem | Evidence |
|---|---|---|
| X1 | **Custom error logging into a separate DB** instead of using `ILogger` + Serilog/Seq/AppInsights/CloudWatch | `ErrorService`, `ErrorDataContext`, `ErrorHandlingMiddleware` |
| X2 | **Errors are persisted with `await _errorService.AddErrorAsync(...)` from inside catch blocks** in services and repositories | `AwsBucketManager`, `FacebookAuthService`, `PostRepository` | Couples every service to a DB; if logging fails the original request fails |
| X3 | **Exceptions silently swallowed** in `PostRepository.AddAsync/UpdateAsync` and in `ErrorHandlingMiddleware.Invoke` (the assignment `var error = await HandleExceptionAsync(...)` is not used) | as named |
| X4 | **No distributed tracing, no correlation id** | – |
| X5 | **Health checks not registered** | `Startup.cs` |
| X6 | **Logging providers limited to Console + Debug**; no structured logging | `Program.cs` |
| X7 | **No metrics** (Prometheus / OpenTelemetry) |

### 5.6 Mapping / DTO layer

| # | Problem |
|---|---|
| M1 | Two parallel mapping systems: AutoMapper `Profile`s (`MappingProfiles/*`) **and** hand-rolled mappers (`CustomMapper/*Mapper.cs` with their own `IXxxMapper` interfaces). |
| M2 | Some custom mappers receive AutoMapper as a parameter (`MapPostFilterRequestToDomain(IMapper mapper, ...)`) – tight coupling between the two systems. |
| M3 | Response models live in `Bingo.Contracts` but mapping happens in the API project that references entities → contracts can never be packaged independently. |
| M4 | `PostsPaginationQuery` / `PaginationFilter` / `PaginationQuery` – three near-identical pagination shapes. |

### 5.7 Notifications / external integrations

| # | Problem |
|---|---|
| N1 | `NotificationService` constructs anonymous JSON payloads (`object obj = new { app_id, contents, ... }`) and serializes with Newtonsoft – stringly-typed integration with OneSignal. |
| N2 | `_request = new HttpRequestMessage(...)` is stored as a field but `HttpRequestMessage` instances cannot be reused across sends – latent bug. |
| N3 | `_httpClient` mutated in constructor (`DefaultRequestHeaders.Add`) – not safe with `IHttpClientFactory` long-lived clients. |
| N4 | `AwsBucketManager` keeps a **static `IAmazonS3 _s3Client`** field but assigns it in the **instance constructor** – race / re-init for every DI activation. |
| N5 | `EmailService` instantiates a single `SmtpClient` and reuses it as a singleton-like field – not thread-safe in older .NET; should use `MailKit` or `System.Net.Mail.SmtpClient` per-send (and SmtpClient is officially obsolete for new development). |
| N6 | `FacebookAuthService` builds URLs with `string.Format` + raw token interpolation – minor injection risk and untestable. |
| N7 | `MyServicesHttpClient` payments proxy – all logic lives in `PaymentsController`; half of it is commented out. |

### 5.8 Validation / business rules in the wrong place

| # | Problem |
|---|---|
| V1 | "Basic user can't have more than 1 active event at a time" enforced in `PostController.Create` with magic strings. Belongs to a domain rule. |
| V2 | "Can't make available slots less than participants number" enforced in `PostController.Update`. |
| V3 | `UpdatedPostDetailsWatcher` is invoked from the controller to decide whether to fire notifications – workflow logic in a side helper. |

### 5.9 Tests

| # | Problem |
|---|---|
| T1 | Integration tests are the only test type – no unit tests, no domain tests. |
| T2 | `Bingo.LoadTests` is empty / netcoreapp3.1 – effectively dead. |
| T3 | Tests likely talk to a real Postgres (`WebApplicationFactory` + `DataContext`); flaky and slow without a fixture/Testcontainers strategy. |
| T4 | No builders / object-mothers for entities – tests will be brittle. |

### 5.10 Build / DevEx

| # | Problem |
|---|---|
| B1 | `Bingo.LoadTests` targets `netcoreapp3.1` while the rest target `net5.0`. |
| B2 | No central `Directory.Packages.props` / `Directory.Build.props`; package versions diverge. |
| B3 | No nullable enable globally (`Models/Post.cs` has `#nullable enable` mid-file – inconsistent). |
| B4 | No analyzers / EditorConfig enforcement visible. |
| B5 | `BingoAPI.csproj` excludes ~9 old migrations via `<Compile Remove>` – brittle. |
| B6 | Newtonsoft.Json used everywhere instead of `System.Text.Json`. |

---

## 6. Dependency Topology (current)

```
                       ┌─────────────────────────────────────────┐
                       │                BingoAPI                 │
                       │                                         │
 HTTP ─► Controllers ─►│  Services ─► Repositories ─► DataContext│─► PostgreSQL/PostGIS
                       │     │                                   │
                       │     ├─► AwsBucketManager  ──────────────┼─► AWS S3
                       │     ├─► NotificationService ────────────┼─► OneSignal
                       │     ├─► EmailService (SmtpClient) ──────┼─► SMTP
                       │     ├─► FacebookAuthService ────────────┼─► Facebook Graph
                       │     ├─► MyServicesHttpClient ───────────┼─► Payments gateway
                       │     └─► ErrorService ───► ErrorDataContext─► PostgreSQL (errors db)
                       │                                         │
                       └─────────────────────────────────────────┘
              ▲                                       ▲
              │                                       │
       Bingo.Contracts                       Bingo.IntegrationTests
       (Requests/Responses/                  (WebApplicationFactory)
        ApiRoutes)
```

There are **no internal seams** – everything in `BingoAPI` may reference everything else, and frequently does.

---

## 7. Risk Heatmap (current state, qualitative)

| Area | Risk | Severity |
|---|---|---|
| Out-of-support runtime (.NET 5 / 3.1) | security / supply-chain | 🔴 Critical |
| Side-effects without outbox/retry (push, e-mail, S3) | data inconsistency | 🔴 High |
| Single DbContext + repos commit ad-hoc (no UoW) | partial writes | 🔴 High |
| Errors swallowed in repo retry loop | silent data loss | 🔴 High |
| Auth: no issuer/audience validation, no expiration required | account takeover | 🟠 High |
| Hardcoded URLs / KnownProxies / CORS origins | env coupling | 🟠 Medium |
| Static `IAmazonS3` re-assigned per ctor | latent races | 🟠 Medium |
| Pagination broken by post-filter `RemoveAll` | wrong feed results | 🟠 Medium |
| N+1 in feed endpoints | performance | 🟠 Medium |
| Two DbContexts both Identity-based | schema drift | 🟡 Medium |
| Empty load test project, no unit tests | regression risk during refactor | 🟡 Medium |
| Mixed AutoMapper + custom mappers | maintainability | 🟡 Low |

---

## 8. TL;DR of the current state

* It’s a **classic .NET 5 layered monolith that grew without a domain model**: controllers do everything, repositories pretend to be a UoW but each commits independently, “Domain” is just DTOs, and `Models` is just EF.
* It has the **right ingredients** (Contracts project, Installers, FluentValidation, Cached attribute, PostGIS, Options pattern, Identity+JWT, Swagger, integration tests) – but **no boundaries** between features, **no application layer**, **no async/eventing**, and **two unsupported TFMs**.
* The biggest hazards are **data consistency** (no UoW + side-effects after commit + swallowed exceptions) and **security/maintenance** (unsupported runtimes, weak JWT validation, hardcoded URLs/keys).

The companion document `hopaut-solution-fix.md` turns this analysis into a concrete refactor plan to a **modular monolith with Clean Architecture per module**, including module catalog, shared kernel, dependency rules, technology choices (MediatR? message broker? S3 pipeline?), and a phased migration roadmap.
