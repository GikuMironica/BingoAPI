---
name: rest-api-standards
description: "Strict REST and wire-contract standards for Hopaut minimal-API endpoints and the BingoMobile Flutter client. Use when: adding or changing endpoints, routes, HTTP methods, status codes, request or response DTOs, ProblemDetails and error codes, validation responses, pagination, filtering, API versioning (Asp.Versioning), OpenAPI documents, endpoint authorization, or any change that alters the wire contract and must be synchronized to BingoMobile."
user-invocable: false
---

# REST API standards and contract sync

Leading word: **contract**. The wire format is a promise to BingoMobile; it changes only on purpose, with the client updated in the same batch.

Code: [endpoint-patterns.md](./references/endpoint-patterns.md).

## 1. Wire reality today (verify per endpoint)

BingoMobile was written against the deleted legacy controllers. Its hand-written Dart models and repositories expect, for example:
- PascalCase JSON keys (`json['Rate']`, `data['PostId']`),
- responses wrapped in `{ "Data": … }` (`response.data['Data']`),
- legacy `SingleError` errors and UI messages chosen by status code (403 means "already rated"),
- verb-style routes from `lib/config/constants/api.dart` (`/acceptattendee`, `/fetchallattendee`, `/unattend`, POST used for reads),
- Unix-second timestamps.

The new modules (System.Text.Json camelCase, no envelope, new routes) match none of this. Treat every module's endpoints as **unsynced** until the contract inventory in TODO.md proves otherwise.

## 2. Target wire conventions

- JSON camelCase; no envelope. A single resource is the body; collections use the paging shapes in §6.
- Errors: RFC 9457 `application/problem+json` (§4).
- Time: ISO-8601 `DateTimeOffset` in UTC (`2026-11-17T09:30:00Z`).
- Enums: camelCase strings.
- IDs: primitives (`int`, `string`, `Guid`). A strongly-typed ID must never serialize as `{ "value": 5 }`.
- Money `{ "amount": 12.5, "currency": "EUR" }`; coordinates `{ "latitude": 47.37, "longitude": 8.54 }`.
- Wire DTOs are `sealed record`s in `Modules.<X>.Api/Contracts/V1/` named `XxxRequest` / `XxxResponse`, with explicit mapping (Mapperly or extension methods). Never serialize domain types, Application DTOs or anonymous objects.

## 3. Resources and routes

- Base path `/api/v1/` + plural kebab-case nouns; nesting at most one level (`/posts/{postId}/attendees`).
- No verbs in paths. State transitions `PATCH` the resource with the target state; queries over a collection are query parameters, not path segments (`/posts?latitude=…&longitude=…&radiusMeters=…`, not `/posts/nearby`).
- The caller's own resources use the `me` alias (`/users/me/posts`).
- Auth flows under `/api/v1/identity/` are the only action-style endpoints allowed; keep their naming consistent.
- Route templates are constants in a per-module `XxxRoutes` class (repository rule); never inline literals.

Illustrative targets (confirm against the contract inventory before implementing):

| Legacy (`api.dart`) | Target |
|---------------------|--------|
| `/fetchallattendee`, `/fetchallaccepted`, `/fetchallpending` | `GET /api/v1/posts/{postId}/attendees?status=pending` |
| `/acceptattendee`, `/rejectattendee` | `PATCH /api/v1/posts/{postId}/attendees/{userId}` `{ "status": "accepted" }` |
| `/attend`, `/unattend` | `PUT` / `DELETE /api/v1/posts/{postId}/attendees/me` |
| `POST /ratings/{id}` (read) | `GET /api/v1/ratings/{ratingId}` |
| `/ratings/userId/{userId}` | `GET /api/v1/users/{userId}/ratings` |
| `/posts/myactive`, `/posts/myinactive` | `GET /api/v1/users/me/posts?status=active` |
| `/announcements/postId/{id}` | `GET /api/v1/posts/{postId}/announcements` |
| `/reports`, `/userreports` | `POST /api/v1/posts/{postId}/reports`, `POST /api/v1/users/{userId}/reports` |
| `/bugs` | `POST /api/v1/bug-reports` |

## 4. Methods, status codes and errors

| Method | Success | Notes |
|--------|---------|-------|
| GET | 200 | 404 when missing or not visible to the caller |
| POST (create) | 201 + `Location` + body | 409 on duplicates |
| PUT | 200 with body, or 201 when it created | idempotent |
| PATCH | 200 with body, or 204 | 409 on an invalid state transition |
| DELETE | 204 | idempotent; 404 if it never existed |

Failures map from `ErrorType` (clean-architecture-ddd §5): 400 validation (`errors` map), 401 unauthenticated, 403 forbidden, 404, 409 state or concurrency conflict, 422 domain rule, 429 with `Retry-After`, 500 without internals.

Problem details: `title`, `status`, `detail` (safe text), `instance`, plus extensions `errorCode` (stable dotted code), `traceId`, and `errors` (camelCase field paths) for validation. Binding failures and unhandled exceptions must also produce `application/problem+json`. Never return `SingleError` from new code, and never leak exception messages, types or stack traces.

## 5. Endpoint implementation

- Endpoints are thin: bind → map to a command or query with strongly-typed IDs → `sender.Send` → map `Result` to `TypedResults`. No business logic, no DbContext, no repositories.
- Return typed unions (`Results<Ok<PostResponse>, ProblemHttpResult>`) so OpenAPI metadata is inferred and the compiler checks every branch.
- Every endpoint declares `.WithName("<Module>_<Action>")`, `.WithSummary(…)` and the problem responses it can produce (`.ProducesProblem(404)`, `.ProducesValidationProblem()`).
- Authorization is on by default (`RequireAuthorization()` on the module group); `AllowAnonymous()` is explicit and justified. Ownership checks are resource-based and live behind the handler (plan Step 6.9).
- The current user comes from `ICurrentUser`; never trust a user ID sent in the body or query.
- Accept a `CancellationToken` and pass it on.

## 6. Validation, paging, filtering

- Exactly one validation mechanism: FluentValidation in the Mediator `ValidationBehavior`. Do not add `AddValidation()` / DataAnnotations in parallel.
- Feeds and long lists use cursor paging: `?cursor=&limit=` → `{ "items": [...], "nextCursor": "…" }`. Small admin lists may use `?page=&pageSize=` → `{ "items", "page", "pageSize", "totalCount" }`. Default limit 20, maximum 100, enforced by the validator.
- Filters and sort are query parameters (`?status=active&sort=-startsAt`); unknown values are a 400, never silently ignored.

## 7. Versioning, deprecation, OpenAPI

- URL-segment versioning `/api/v{version:apiVersion}` through `Asp.Versioning.Http` + `Asp.Versioning.Mvc.ApiExplorer` (plan Step 6.10); default and only version today: 1.
- A route kept for compatibility is marked deprecated with `Deprecation` and `Sunset` headers and has a removal step in the plan.
- OpenAPI: built-in `Microsoft.AspNetCore.OpenApi` (OpenAPI 3.1), one document per version (`AddOpenApi("v1")`), XML doc comments from `*.Api` projects, Scalar UI in Development only. Confirm how the installed Asp.Versioning release integrates with the built-in OpenAPI generator before wiring it.
- **Contract snapshot:** `docs/openapi/v1.json` is committed, and a test compares it with the generated document. Any diff in that file is, by definition, a contract change and triggers §9.

## 8. Security (OWASP API Top 10)

- Object-level authorization on every handler that reads or changes user-owned data (BOLA is the top API risk).
- No mass assignment: explicit request DTOs only, never bind into domain or persistence types.
- Rate-limit policies per endpoint group; stricter on auth and uploads.
- Uploads: size limits, allow-listed content types verified by content sniffing, never trust file names.
- No secrets, tokens or personal data in logs, problem details or URLs.

## 9. Contract change classification and mobile sync

| Additive (non-breaking) | Breaking |
|-------------------------|----------|
| new endpoint; new optional request field; new response field; new error code on a new path | removed or renamed route, field or enum value; type or format change (Unix seconds → ISO-8601); request field becoming required; status-code or error-code change; envelope or casing change; auth, claim or token change; paging semantics change |

**D-API-1 Compatibility window** (confirmed by the user on 2026-11-17): the modular monolith is not yet deployed, so BingoMobile `dev` and the backend ship together; breaking changes to `/api/v1` are allowed when the mobile client is updated in the same batch, and no legacy aliases are added. Released store builds (`master`, v1.2.17) are not kept working.

BingoMobile facts that shape the sync (`C:\Users\ghmi\personal\BingoMobile`, branch `dev`):
- Routes: `lib/config/constants/api.dart`. Models: `lib/data/models/*.dart` with hand-written `fromJson` / `toJson` (no code generation; edit directly). Calls: `lib/data/repositories/*_repository.dart`. HTTP plumbing and token refresh: `lib/services/dio_service.dart`, `lib/config/dio_base_options.dart`.
- User-facing messages come from generated `LocaleKeys` (`lib/generated/locale_keys.g.dart`); reuse existing keys, and record new keys as pending mobile work instead of hand-editing generated files.
- The mobile code base is mid-migration with dependency resolution blocked (mobile Step 0.3-H). `fvm dart format <files>` works; `fvm flutter analyze` / `fvm flutter test` may not, and the reason goes into the handoff.
- Preserve UI/UX; change only data, network and error-mapping code.

Procedure: the agent's *Contract sync procedure* plus `docs/mobile-handoffs/_TEMPLATE.md` (one handoff per change, before/after JSON for every changed DTO, exact Dart file paths).

## 10. Detection list

| Problem | Where to look | Fix |
|---------|---------------|-----|
| Verbs or views in paths | `Map(Get\|Post\|Put\|Patch\|Delete)\("/[^"]*(nearby\|my\|fetch\|accept\|reject\|attend)` | resource + query parameters / PATCH |
| Anonymous or untyped responses | `Results\.(Ok\|Created)\(new \{` | typed response record + `TypedResults` |
| `Result` not mapped to HTTP | `Results.Ok(result)` returning a `Result` | `ToProblem()` mapping |
| Domain or Application types on the wire | endpoint return types | wire DTO + mapper |
| Missing auth | groups without `RequireAuthorization` | auth by default |
| User ID from the request | `userId` in request DTOs for the caller | `ICurrentUser` |
| Claims parsed inline | `User.FindFirst(` in endpoints | `ICurrentUser` |
| Unbounded lists | collection endpoints without a limit | paging |
| Inline route strings | literal paths in `Map*` | `XxxRoutes` constants |
| Missing OpenAPI metadata | `Map*` without `WithName` | metadata + snapshot |

## 11. Tests

- Per endpoint (WebApplicationFactory + Testcontainers): the success status and JSON shape including property names, 400 problem with `errors` keys, 401 without a token, 403 for another user's resource, 404, and 409 / 422 where applicable.
- The OpenAPI snapshot test.
- Contract tests for every DTO shared with BingoMobile, asserting the exact JSON field names and types so accidental renames fail the build.

## Completion criterion

An API change is complete only when:
- routes, methods and status codes follow §3–§4, and every failure is `application/problem+json` with an `errorCode`;
- endpoints are thin, typed, documented and authorized by default;
- the OpenAPI snapshot is updated in the same commit as the change;
- every contract change has a handoff file and BingoMobile is updated (`applied`), or the gap is recorded as `pending` in HANDOVER.md;
- endpoint, contract and snapshot tests pass.
