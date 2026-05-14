# Baseline configuration inventory

Captured for Phase 0 / Step 0.1 of `hopaut-execution-plan.md`.

* Repo HEAD at capture: `56b91bf1c383b73da2da08490cd8d4eeedc0543a` on `master`.
* Tag created: `pre-net10-baseline` (annotated, points at the same commit).
* Installed SDK at capture machine: `10.0.203` (only SDK present; no `global.json` yet).
* Solution projects (4):
  * `BingoAPI/BingoAPI.csproj` – `net5.0`
  * `Bingo.Contracts/Bingo.Contracts.csproj` – `net5.0`
  * `Bingo.IntegrationTests/Bingo.IntegrationTests.csproj` – `net5.0`
  * `Bingo.LoadTests/Bingo.LoadTests.csproj` – `netcoreapp3.1` (empty, slated for deletion in Step 0.6)
* Top-level package list: see `docs/baseline-packages.txt`.
  * Known critical advisory at baseline: `System.Drawing.Common 5.0.1`
    (GHSA-rxg9-xrhp-64gj). Replaced by ImageSharp in Step 0.4.

## Configuration files

| Path | Purpose |
|------|---------|
| `BingoAPI/appsettings.json` | Production defaults; binds all options sections below. |
| `BingoAPI/appsettings.Development.json` | Logging override only. |
| `BingoAPI/wwwroot/Configurations/Currencies.json` | Static currency list loaded on startup. |
| `BingoAPI/wwwroot/Configurations/EventTypes.json` | Static event-type catalog loaded on startup. |

## Options sections bound from configuration

Source: `appsettings.json` + installer classes under `BingoAPI/Installers/`.

| Section | Consumer (today) | Notes for migration |
|---------|------------------|---------------------|
| `Logging` | `Program.CreateHostBuilder` default logging | Replaced by Serilog in Step 0.7. |
| `SwaggerOptions` | `SwaggerInstaller` | Keep. |
| `RedisCacheSettings` (`Enabled`, `ConnectionString`) | `RedisInstaller`, `CachedAttribute` | Drives Step 1.6 caching rebuild. |
| `ConnectionStrings:PostgreConnection` | `DataContext` | Main Postgres/PostGIS DB. |
| `ConnectionStrings:ErrorPGConnection` | `ErrorDataContext` | Removed in Step 0.8. |
| `JwtSettings` (`Secret`, `TokenLifetime`) | `McInstaller`, `IdentityService` | Tighten in Step 0.10 (add issuer/audience). |
| `FacebookAuthSettings` (`AppID`, `AppSecret`) | `IdentityService` Facebook flow | Move into Identity module in Phase 2. |
| `ApplicationEmail` (`EmailAddress`, `Sender`, `Password`, `SmtpClient`, `Port`, `SSL`) | `EmailService` | Notifications module (Phase 5). |
| `AWSImageBucket` (`aws_access_key_id`, `aws_secret_access_key`, `bucketName`, `contentFormat`) | `AwsBucketManager` | Media module (Phase 3). Replace inline credentials with IAM/role + KeyVault later. |
| `OneSignalNotification` (`AppId`, `EndPoint`, `Authorization`) | `NotificationService` | Notifications module (Phase 5). |
| `IpRateLimiting` | `AspNetCoreRateLimit` | Replaced by built-in rate limiter in Step 0.4 / 0.9. |
| `AllowedHosts` | Host filtering | Keep. |
| `EnvironmentOptions` (`Environment`, `Server`, `Swagger`) | Misc. installers, throttling, email gating | Consolidate into hosting environment + feature flags during Phase 1. |
| `MyServicesSettings:PaymentGatewayKey` | `Payments` flow | Payments module (Phase 5). |

## Environment variables / runtime inputs

* `ASPNETCORE_ENVIRONMENT` – standard.
* `ASPNETCORE_URLS` – standard.
* No custom `Environment.GetEnvironmentVariable(...)` lookups in source (verified via search).

## Hardcoded values to migrate to configuration (tracked under Step 0.10)

* `Startup.MyAllowSpecificOrigins` – CORS origins constant.
* `IdentityController.WebPortalRelativeUrl*` – portal URLs.
* `Startup.ConfigureServices` – `KnownProxies = 10.0.0.100`.
* JWT `ValidateIssuer = false`, `ValidateAudience = false` – fixed in Step 0.10.

## Build/test baseline status

* Build/integration-test green snapshot is **not** yet produced in this capture.
  The .NET 5 targeting pack is no longer installed on the capture machine
  (only the .NET 10 SDK is present), so `dotnet build` against `net5.0`
  cannot be executed locally without re-installing the .NET 5 runtime.
  We are therefore recording the **last known good** state as the
  `pre-net10-baseline` tag and proceeding with Step 0.2; if a green run is
  required for compliance, install the .NET 5 runtime and re-tag.
