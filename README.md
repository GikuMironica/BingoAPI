# BingoAPI

Backend of Hopaut (.NET 10 modular monolith, `src/Host/Hopaut.Api.Host`). Migration plan and progress: `hopaut-execution-plan.md`, `hopaut-migration-status.md`, `TODO.md`.

## Running the API locally

Required configuration (never commit secrets; use `dotnet user-secrets` or environment variables):

| Setting | How to provide it |
|---------|-------------------|
| `ConnectionStrings:DefaultConnection` | PostgreSQL + PostGIS connection string |
| `JwtSettings:Secret` | at least 32 random characters: `dotnet user-secrets set "JwtSettings:Secret" "<secret>" --project src/Host/Hopaut.Api.Host`, or `JwtSettings__Secret` in the environment / `.env` for `docker compose`. The Host refuses to start without it. |

Optional: `ConnectionStrings:Redis` (otherwise an in-memory cache), `OTEL_EXPORTER_OTLP_ENDPOINT` and the other standard `OTEL_*` variables (OTLP export of traces and metrics), `ProxySettings:KnownProxies` / `ProxySettings:KnownNetworks` (trusted reverse proxies for forwarded headers), `Cors:AllowedOrigins` (CORS stays off when empty). All settings are validated at startup.

Operational endpoints: `/health/live` (process), `/health/ready` (databases and cache), `/admin/hangfire` (administrators only). Every response carries an `X-Correlation-Id` header (a well-formed incoming one is kept) that also appears on every log event of the request.

Tests: see `tests/README.md`.
