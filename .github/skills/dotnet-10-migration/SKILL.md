---
name: dotnet-10-migration
description: ".NET 10 / C# 14 modernization rules for the Hopaut backend (BingoAPI). Use when: editing csproj, Directory.Build.props, Directory.Packages.props or global.json; adding, upgrading or removing NuGet packages and checking their licenses; replacing legacy libraries (MediatR, AutoMapper, Swashbuckle, FluentValidation.AspNetCore, Newtonsoft.Json, System.Drawing, Flurl, custom clocks); configuring DI, options, hosting, caching, HttpClient, health checks; EF Core 10 mappings and migrations; applying C# 14 syntax; performance tuning; enabling analyzers or warnings-as-errors."
user-invocable: false
---

# .NET 10 migration and modernization

Leading word: **compile-time**. Every mistake should surface at build time: nullable analysis, analyzers, source generators, typed results and validated options.

## 1. Platform baseline

- .NET 10 LTS (supported until November 2028), C# 14, SDK pinned by `global.json` (`10.0.300`, `rollForward: latestFeature`).
- EF Core 10 (LTS) + Npgsql 10 + NetTopologySuite; ASP.NET Core 10 minimal APIs.
- Facts drift. Before relying on a framework feature or package behavior, confirm it on learn.microsoft.com or the package repository and record what you verified in TODO.md *Decisions*.

## 2. Build configuration

Target end state for `Directory.Build.props`; approach it step by step, fixing warnings instead of suppressing them:

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <LangVersion>latest</LangVersion>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <AnalysisLevel>latest-recommended</AnalysisLevel>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  <NuGetAudit>true</NuGetAudit>
  <NuGetAuditMode>all</NuGetAuditMode>
</PropertyGroup>
```

- Today `TreatWarningsAsErrors` is opt-in through `-p:HopautStrictWarnings=true`. Make it the default once the solution builds clean that way (Phase 5 gate at the latest).
- `<NoWarn>` and `#pragma warning disable` are allowed only for a documented false positive, with a one-line reason and a TODO.md decision.
- Central Package Management: versions live only in `Directory.Packages.props`; never `Version=` in a csproj. Use `CentralPackageTransitivePinningEnabled` to pin a vulnerable transitive dependency.
- `GenerateDocumentationFile` for `*.Api` projects (XML comments feed OpenAPI); suppress CS1591 only there.
- `Bingo.sln` → `.slnx` (`dotnet sln migrate`) is optional and only worth doing when no tooling still needs `.sln`.

## 3. Package policy and license gate

Before adding or upgrading any package, read its license on nuget.org. MIT, Apache-2.0 or BSD → proceed. Commercial, dual or unclear → do not add it; record a blocker with the open alternative.

| Today | Target | Why |
|-------|--------|-----|
| MediatR 12.x | `Mediator.Abstractions` + `Mediator.SourceGenerator` 3.x (MIT) | MediatR ≥ 13 is commercial; source generation gives compile-time handler checks and `ValueTask` (messaging-integration skill) |
| AutoMapper | `Riok.Mapperly` (source-generated) or explicit mapping methods | runtime reflection; AutoMapper ≥ 15 is commercial (plan Step 6.8) |
| Swashbuckle.* | `Microsoft.AspNetCore.OpenApi` (OpenAPI 3.1) + `Scalar.AspNetCore` UI in Development | first-party in .NET 10 (rest-api-standards skill) |
| FluentValidation.AspNetCore | `FluentValidation` + `FluentValidation.DependencyInjectionExtensions`, run in `ValidationBehavior` | ASP.NET auto-validation package is deprecated; keep exactly one validation mechanism |
| Newtonsoft.Json | System.Text.Json | built-in; check custom converters for `Utf8JsonReader.HasValueSequence` handling |
| System.Drawing.Common | SixLabors.ImageSharp (already referenced) | Windows-only since .NET 7; ImageSharp uses the Six Labors Split License, so confirm eligibility |
| Flurl.Http | typed `HttpClient` via `IHttpClientFactory` + `Microsoft.Extensions.Http.Resilience` (`AddStandardResilienceHandler`) | DI, resilience, testability |
| `IDateTimeProvider` | `System.TimeProvider`; `FakeTimeProvider` (`Microsoft.Extensions.TimeProvider.Testing`) in tests | BCL standard |
| custom Redis `ICacheService` | `HybridCache` (`Microsoft.Extensions.Caching.Hybrid`) with Redis as L2 | stampede protection and tag invalidation (plan Step 1.6) |
| FluentAssertions 7.x | stay below 8 (8+ is commercial) or move to an Apache-2.0 fork | license |
| MassTransit (not yet used) | 8.x (Apache-2.0) only; 9+ needs a user-supplied license | messaging-integration skill |

Every package change: CPM entry updated, unused packages removed, `dotnet list Bingo.sln package --vulnerable --include-transitive` clean.

## 4. C# 14 and modern syntax

| Use | For | Not for |
|-----|-----|---------|
| Primary constructors | DI in handlers, services, endpoint classes | aggregates and value objects (parameters stay mutable, no validation) |
| Collection expressions `[]` | initializing lists, arrays, spans | |
| `field` keyword | property-level normalization without a manual backing field | hiding business rules in setters |
| Extension members (`extension(T receiver) { … }`) | grouping mapping helpers, `Result` → HTTP helpers, endpoint registration | business logic |
| Null-conditional assignment (`x?.Y = z`) | optional updates | |
| `required` + `init` | DTOs and options | entities (use factories) |
| Records | DTOs, commands, queries, events, value objects | entities and aggregates |
| `sealed` by default | every class not designed for inheritance | |
| Switch expressions / patterns | `ErrorType` → status mapping, state transitions | long if/else chains |
| `Guid.CreateVersion7()` | new Guid keys, event and outbox IDs | |
| `System.Threading.Lock` | locking | `lock (object)` |
| `TimeProvider` | time | `DateTime.Now` / `DateTime.UtcNow` |
| `FrozenDictionary`, `SearchValues` | static hot-path lookups | |
| Raw string literals | SQL, JSON test fixtures | |

## 5. Dependency injection and hosting

- Each module exposes `AddXxxModule(IServiceCollection, IConfiguration)` and `MapXxxEndpoints(IEndpointRouteBuilder)`; the Host only composes.
- Options: `services.AddOptions<T>().BindConfiguration("Section").ValidateDataAnnotations().ValidateOnStart();`. Business code never reads `IConfiguration`. Secrets come from user-secrets or environment variables, never committed appsettings.
- Lifetimes: DbContexts and Mediator handlers are scoped; singletons must not depend on scoped services. Keep `ValidateScopes` / `ValidateOnBuild` on in Development.
- Never call `BuildServiceProvider()` during registration. No service locator in business code; `IServiceScopeFactory.CreateAsyncScope()` only in background services.
- `HttpClient` only through typed clients with `AddStandardResilienceHandler()`; never `new HttpClient()`.
- Background work: `BackgroundService` + `PeriodicTimer`, honoring the stopping token.
- Health: `/health/live` (process) and `/health/ready` (database, Redis), separated by tags.
- Unhandled exceptions: `IExceptionHandler` + `AddProblemDetails()`; never leak stack traces or exception types.
- Structured logging through `ILogger<T>` (Serilog sink); prefer `[LoggerMessage]` source-generated methods on hot paths; never log secrets, tokens or personal data.

## 6. EF Core 10

- One DbContext per module with its own schema (`HasDefaultSchema("posts")`) and its own migrations history table in that schema (`npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "posts")`).
- Value objects map as complex types (`ComplexProperty`); EF Core 10 supports optional, nested, struct and JSON complex types. Prefer complex types over owned entities for value objects.
- Strongly-typed IDs map through value converters (shared helpers in BuildingBlocks.Infrastructure); client- or sequence-assigned keys use `ValueGeneratedNever()`.
- Concurrency: `uint Version` + `IsRowVersion()` maps to Postgres `xmin`.
- Reads: `AsNoTracking()` + projection to DTOs; filtering and paging in SQL; `AsSplitQuery()` against cartesian explosion; the `LeftJoin` operator for left joins.
- `ExecuteUpdateAsync` / `ExecuteDeleteAsync` bypass invariants and domain events; use them only for maintenance jobs and read models, never for aggregate state changes.
- Spatial: NetTopologySuite `Point`, SRID 4326, `geography (point)` column so distances are meters, GIST index.
- Migrations: one per change, named for intent, generated with `dotnet ef migrations add <Name> -p <module Infrastructure csproj> -s src/Host/Hopaut.Api.Host -c <Module>ModuleDbContext`. Review the generated SQL (`dotnet ef migrations script`) and ship a data-conversion script when the representation changes. Never `EnsureCreated` outside tests. If `dotnet-ef` is missing, add a local tool manifest (`dotnet new tool-manifest`, `dotnet tool install dotnet-ef`).

## 7. Performance

- Async all the way with `CancellationToken` propagated; no sync-over-async.
- No N+1: batch cross-module reads and project in SQL.
- Bounded pagination on every collection endpoint.
- Response compression and output caching only where measured useful; `HybridCache` for expensive queries with tag invalidation on writes.
- Measure before optimizing: `dotnet-counters` / OpenTelemetry metrics; record evidence for every performance change.

## 8. Commands (repository root, PowerShell)

```powershell
dotnet build Bingo.sln
dotnet test Bingo.sln
dotnet build Bingo.sln -c Release -p:HopautStrictWarnings=true
dotnet format Bingo.sln --verify-no-changes
dotnet list Bingo.sln package --vulnerable --include-transitive
dotnet list Bingo.sln package --outdated
```

## Completion criterion

A platform change is complete only when:
- the solution builds with zero warnings in the touched projects (zero warnings solution-wide at phase gates with `HopautStrictWarnings`);
- every added or upgraded package is CPM-managed, license-checked and free of known vulnerabilities;
- no obsolete API, legacy library or suppressed warning was introduced;
- options are validated at startup and no secret is committed;
- all tests pass.
