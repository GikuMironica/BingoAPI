# Mobile handoff prompt

> **How to use this file:** open the sibling `BingoMobile` workspace in a new chat, attach `mobile-solution-fix.md` and `mobile-migration-status.md`, and paste this entire file as the prompt. The mobile assistant will apply the changes, update `mobile-migration-status.md`, and reply with the mobile commit hash. Then come back here and update the **Status** section at the bottom.

> The body below is written in the second person, addressing the mobile assistant.

---

## 1. Source change (backend)

* **Backend repo:** `BingoAPI`
* **Branch / commit:** `<branch>` @ `<commit-hash>` (or `pre-net10-baseline`)
* **Plan reference:** Phase `<n>` / Step `<n.m>` in `hopaut-execution-plan.md`
* **Status row updated in:** `hopaut-migration-status.md` (Activity log entry `<YYYY-MM-DD>`)

## 2. Affected endpoints

| Method | Route | Change | Backend route constant |
|--------|-------|--------|------------------------|
| `GET`  | `/api/v1/...` | added \| changed \| removed \| deprecated | `ApiRoutes.<Area>.<Action>` |

## 3. Wire-format diff

For each changed DTO, show both shapes.

### `<DtoName>` request

Before:

```json
{ "...": "..." }
```

After:

```json
{ "...": "..." }
```

Notes:
* Renamed fields: `<old> → <new>`
* Type changes: `<field>: <oldType> → <newType>`
* Nullability: `<field>` is now required / optional
* Enum changes: ...

### `<DtoName>` response

Before / After (same format as above).

## 4. Behavioural diff

* Status codes: `<old>` → `<new>` for `<scenario>`.
* Error envelope: `SingleError` → `ProblemDetails` (or vice versa) for these routes.
* Pagination: offset → cursor; new params `<...>`.
* Auth: claim names, JWT issuer/audience, refresh-token endpoint changes.
* Headers: new `Vary`, `ETag`, `Retry-After`, rate-limit headers.
* Retry / idempotency semantics.

## 5. Backwards compatibility window

* Is the old shape still served? `yes/no`
* For how long? `<dates / release tag>`
* Feature flag or content negotiation? `<header / query param>`
* Recommended mobile minimum app version after switchover: `<version>`

## 6. Required mobile changes

> Cross-reference `mobile-solution-fix.md` Phase `<n>` / Step `<n.m>` and the corresponding rows in `mobile-migration-status.md`.

* DTO updates:
  * Legacy: `lib/data/models/<file>.dart`
  * Target: `lib/features/<feature>/data/dto/<file>.dart` (`freezed` + `json_serializable`)
* Repository / data source: `lib/features/<feature>/data/<repo_or_source>.dart`
* Domain entity / mapper: `lib/features/<feature>/domain/...`
* Routing (if a deeplink/path changed): `lib/config/routes/...` or `core/routing/app_router.dart`
* `DioClient` interceptors (auth/refresh/error) if envelope or status codes changed.
* Push handler (`core/notifications/push_service.dart`) if OneSignal payload changed.
* i18n keys for any new error code surfaced to UI (`assets/translations/*.json` + `LocaleKeys`).

## 7. Suggested mobile validation

* Smoke-test screens: `<screen list>`
* Integration tests to run: `<test names>`
* Manual: login, refresh-token replay, create event with images, attendance request/accept, push receipt.

## 8. Status

* `pending`
* When applied on mobile: replace with `applied <YYYY-MM-DD> <BingoMobile commit hash>`.
