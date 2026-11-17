# Handover – Hopaut architecture migration

**Written:** 2026-11-17 by HopautArchitect · **Backend:** `migrate/net10` @ `1afc17a` + the commit that adds this file (tree clean) · **Mobile:** `dev` @ `b20be13` (tree clean, unchanged this session)

## 1. State
Step **8.2 – Posts is ✅** (commits `42b6c68`, `3e14aed`, `bd9caa7`, `884ffca`, `fb2963a`, `dafd90d`, `4ba35e0`, `1afc17a`). This session delivered batches 3a–4b: per-aggregate optimistic concurrency for every module (`xmin`, children touch their root, conflicts → 409, D-027), tags as an aggregate (F-065), `UpdatePost` with the legacy update rules (D-028), `DisablePost` + caller-scoped `GetMyPosts` and the bounded feed on a keyset read model (D-029; F-067, F-068), and one-to-one children keyed by `PostId`. Nothing is half-done. The session stopped because its context budget is the only proxy for the user's "stop at 90 % of the Copilot credits" instruction.

## 2. Resume point
- **Next step:** 8.3 – Attendance (`hopaut-execution-plan.md` § Phase 8 checklist and § Step 8.3; `TODO.md` *Now*)
- **Next action:** `git status`; start the Podman helper (§6); load clean-architecture-ddd, messaging-integration and rest-api-standards; survey `src/Modules/Attendance` (~25 files) and the legacy rules, then work in committed batches like 8.2.
- **Survey (2026-11-17):** `Participation` (`Domain/Participation.cs`) has a rule-free public `Create`, and `Accept`/`Reject`/`Cancel` return `bool` (handlers turn `false` into `ParticipationErrors.NotPending`/`AlreadyCancelled`); `RequestAttendanceCommandHandler` only checks duplicates (→ `attendance.participation.already_requested`, Conflict); endpoints (`Api/AttendanceModule.cs`) map most failures to empty 404/409 (D-021, keep until 10.5); `AttendanceRepository.DeleteByPostAsync` uses `ExecuteDeleteAsync` (TODO for 9.3); `IAttendanceQueries.CountAcceptedAsync` (Attendance.Contracts) exists since 8.2 batch 3c; the unique index `(PostId, UserId)` exists. Legacy rules at tag `pre-net10-baseline`: `BingoAPI/Controllers/AttendedEventsController.cs` `Attend` (registered user with first and last name; post exists; not the host — "Cant join own event"; the repository it calls refuses when no slots are left, the user already applied or the event has ended — locate that repository method first), `UnAttend` ("User did not applied"); `BingoAPI/Models/SqlRepository/EventParticipantsRepository.cs` `AcceptAttendee` (when the event has slots, accepted < slots, else accept freely), lists of all / accepted / pending attendees for the host and active / inactive attended posts for the user.
- **Suggested batches:** (1) `Hopaut.Modules.Attendance.UnitTests` from `templates/Module` (add to `Bingo.sln`), `Participation.Request(id, post facts, requester, now)` returning `Result` with the rules above (post facts from a new Posts.Contracts read port: exists, status active, end > now, host, slots — Posts must not count attendees; Attendance passes its own accepted count), `Accept`/`Reject`/`Cancel` returning `Result` with catalog errors, `xmin` on `Participation` (D-027, migration), domain + handler tests; (2) paged attendee list (`GET /api/v1/posts/{postId}/attendees?status=` is the 10.5 target; add the query + read model now, endpoint additive) and my-attendances list on a read model with SQL keysets (D-029 pattern: `SqlQuery` page of keys, then projection); (3) coverage ≥ 70 % for Attendance Domain + Application, architecture suite green, `dotnet test Bingo.sln`, then 8.3 ✅. Record the first-and-last-name rule as a decision (it needs a Users read port; consider deferring to 8.8) and every module-behaviour change under the contract inventory's "Module-endpoint behaviour" list.
- **Half-done:** None.

## 3. Validation status

| Command | Result | Date |
|---------|--------|------|
| `dotnet build Bingo.sln -p:HopautStrictWarnings=true` | 0 errors, 0 compiler warnings (1 B-002 MSBuild notice) | 2026-11-17 |
| `dotnet build Bingo.sln -c Release -p:HopautStrictWarnings=true` | not re-run; still blocked by B-002 (`No Six Labors license found`) | 2026-11-17 |
| `dotnet test Bingo.sln` (Podman helper from §6) | 471 passed / 0 failed / 0 skipped (244 building-block unit, 43 architecture, 99 Posts unit, 2 Media unit, 83 integration) | 2026-11-17 |
| `dotnet test tests/Modules/Posts/Hopaut.Modules.Posts.UnitTests --collect:"XPlat Code Coverage"` | line rate Posts.Domain 97.0 %, Posts.Application 97.7 % | 2026-11-17 |
| `has-pending-model-changes` (Posts, Attendance) | no pending changes | 2026-11-17 |
| `dotnet list Bingo.sln package --vulnerable --include-transitive` | none (57 projects) | 2026-11-17 |
| `dotnet format Bingo.sln --verify-no-changes --include <changed files>` | exit 0 | 2026-11-17 |

## 4. Schema changes

All migrations below are applied only to Testcontainers databases (and to a local Development database when `Database:MigrateOnStartup` is set); plan Step 11.1 lists the matching legacy-import conversions.

| Module | Migration | Applied where | Data-conversion script |
|--------|-----------|---------------|------------------------|
| Posts, Ratings, Attendance, Announcements, Moderation, BugReports | `…IdSequence` ×6 (7.6) | Testcontainers only | None; Step 11.1 re-runs each `setval` after the import (D-019) |
| Posts | `20261117195201_PostStatusAndRequiredTitle` | Testcontainers only | In the migration: `ActiveFlag` → `Status`, blank titles → 'Untitled event', titles cut to 30 |
| Posts | `20261117195922_EntranceFeeMoney` | Testcontainers only | In the migration, both directions (D-026); `PostsMigrationTests` |
| Posts | `20261117204341_PostVersion` | Testcontainers only | None (`xmin` is a system column; no SQL) |
| Posts | `20261117204938_TagNamesAndIdSequence` | Testcontainers only | In the migration: names trimmed, lower-cased, cut to 20, blanks dropped, duplicates merged; `posts.tag_ids` + `setval`; Down restores the identity (normalisation is one-way); `PostsMigrationTests` |
| Posts | `20261117211632_ChildrenKeyedByPost` | Testcontainers only | None needed (`PostId` was unique); Down re-adds identity keys |

## 5. Contract changes (backend ↔ mobile)

No handoff is due: BingoMobile does not call the module endpoints yet (D-007), so every contract-inventory row stays `unsynced` until its Phase 10 step. The behaviour the Phase 10 handoffs must describe is listed in `TODO.md` → *Contract inventory* → "Module-endpoint behaviour already changed before its Phase 10 sync" (this session added: 409 `general.concurrency_conflict`; saved `tagNames`; `PATCH /api/v1/posts/{postId}`; `PATCH /api/v1/posts/{postId}/status`; `GET /api/v1/users/me/posts`; the bounded feed without ended posts).

| Endpoint | Change | Breaking? | Handoff file | Mobile status |
|----------|--------|-----------|--------------|---------------|
| None this session | – | – | – | – |

## 6. Open decisions and blockers
- **B-002 Six Labors Community license key** (blocks Release builds, `dotnet publish`, the Docker image and the rest of 7.8) → unblock action: the user applies for the free Community license at https://licensing.sixlabors.com/ (non-profit; an assembly-scoped key must cover `Hopaut.Modules.Media.Infrastructure`) and supplies it as the `SixLaborsLicenseKey` environment variable or the git-ignored repository-root `sixlabors.lic`. Never commit the key. Then run `dotnet build Bingo.sln -c Release -p:HopautStrictWarnings=true` and flip 7.8 ✅.
- **Running the Host locally** needs `JwtSettings:Secret` (≥ 32 characters) via `dotnet user-secrets` or `JwtSettings__Secret` (README.md).
- **Container runtime on this machine:** `podman machine ssh "sudo systemd-run --unit=hopaut-podman-tcp podman system service --time=0 tcp://127.0.0.1:2375"`, then `$env:DOCKER_HOST='tcp://127.0.0.1:2375'; $env:TESTCONTAINERS_RYUK_DISABLED='true'` in the same PowerShell command as `dotnet test`; stop with `podman machine ssh "sudo systemctl stop hopaut-podman-tcp"` (`tests/README.md`). Stopped at the end of this session.
- **EF Core limitation (D-029):** do not compose LINQ over `FromSql` for entities with complex types (`42703 … Schedule_End`); page keys with `Database.SqlQuery<T>` and project by ID instead.
- **Final merge (D-015, Step 11.3):** only after 6.13, one `git merge --no-ff migrate/net10` into `master`, done locally; the user pushes.
- **D-011** xunit.v3 stays on 3.2.x (VSTest).

## 7. Prompt – next backend session

```text
Agent: HopautArchitect (workspace C:\Users\ghmi\personal\BingoAPI)
Resume from HANDOVER.md. Next step: 8.3 – Attendance (plan § Phase 8 checklist and § Step 8.3; TODO.md Now). Next action: git status; start the Podman API helper from HANDOVER.md §6; load the clean-architecture-ddd, messaging-integration and rest-api-standards skills; follow HANDOVER.md §2 (survey, then the suggested batches), committing after each green batch.
Do not redo completed work; verify the validation status in HANDOVER.md §3 first (dotnet build Bingo.sln -p:HopautStrictWarnings=true; dotnet test Bingo.sln with the Podman variables).
Reuse the 8.2 patterns: Result-returning verbs with a per-aggregate error catalog, xmin concurrency (D-027), keyset read models via SqlQuery + projection (D-029), value objects from SharedKernel, unit tests from templates/Module.
Then continue with Phase 8 (8.4 → 8.12). 7.8 only needs the Release build once the user supplies the Six Labors key (B-002).
Commit everything on migrate/net10. The user wants one big `git merge --no-ff migrate/net10` into master only at the very end (Step 11.3, D-015): never merge earlier, never push.
```

## 8. Prompt – BingoMobile session (mobile client work)

```text
Workspace: C:\Users\ghmi\personal\BingoMobile, branch dev.
There are no pending backend contract changes as of 2026-11-17: no file in C:\Users\ghmi\personal\BingoAPI\docs\mobile-handoffs\ has Status `pending`.
Do not change routes, models, repositories or error handling for backend reasons in this session, and do not change UI/UX.
For context only: the backend's module endpoints do not yet match lib/config/constants/api.dart (PascalCase, `Data` envelope, Unix seconds). They are synchronised module by module in backend Phase 10, each with a handoff file and an `applied <date> <commit>` status. Continue the mobile plan in mobile-migration-status.md instead (Phase 0 is blocked at Step 0.3-H, HERE SDK).
Commit locally on dev only. At the very end of the migration the user merges dev into master themselves (backend Step 11.3 / D-015); do not touch master.
```
