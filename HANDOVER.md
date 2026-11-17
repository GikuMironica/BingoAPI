# Handover – Hopaut architecture migration

**Written:** 2026-11-17 by HopautArchitect · **Backend:** `migrate/net10` @ `6b671da` + the commit that adds this file (tree clean) · **Mobile:** `dev` @ `b20be13` (tree clean, unchanged this session)

## 1. State
Steps **8.3 – Attendance** (`ddd38f3`, `f05b495`) and **8.4 – Ratings** (`6b671da`) are ✅ this session: the legacy attendance and rating rules are domain verbs returning `Result` (D-030, D-031), both aggregates and the reputation read model carry `xmin`, new keyset read models serve the attendee, my-attendance and rating reads, and two module unit-test projects exist. Step **8.5 – Announcements** is surveyed but not started (no code changed). The session stopped at a checkpoint because its context budget is the only proxy for the user's "stop at 90 % of the Copilot credits" instruction.

## 2. Resume point
- **Next step:** 8.5 – Announcements (`hopaut-execution-plan.md` § Phase 8 checklist and § Step 8.5; `TODO.md` *Now*, finding F-074)
- **Next action:** `git status`; start the Podman helper (§6); load clean-architecture-ddd, messaging-integration and rest-api-standards; read the module (`src/Modules/Announcements`: `Announcement.cs`, `AnnouncementErrors.cs`, the create / delete handlers with `PostHostAuthorization`, `GetAnnouncementsByPost`, `AnnouncementsModule.cs`) and the legacy `BingoAPI/Controllers/AnnouncementController.cs` + its repository at tag `pre-net10-baseline`.
- **Survey (2026-11-17), legacy rules:** read one (`GET /announcements/{id}`) or a post's list — only the post's host, its accepted attendees or an administrator (403 otherwise; the module checks nothing: F-074); create — host or admin (participants are then notified, 9.4); update (`PUT /announcements/{id}`, missing in the module) and delete — host or admin (404 when missing); inbox (posts the caller attends that have announcements) and outbox (posts the caller hosts that have announcements) return `MiniPostForAnnouncements` lists. Today the module has `CreateAnnouncement` and `DeleteAnnouncement` (host or admin through `announcements.post.host` since 6.9) and an unauthorized `GetAnnouncementsByPost`.
- **Suggested batches:** (1) `Hopaut.Modules.Announcements.UnitTests` (add to `Bingo.sln` under `tests/Modules/Announcements`), an `AnnouncementMessage` value object (legacy length rule — look it up in `Bingo.Contracts/V1/Requests/Announcement*` at the tag), `Announcement.Create` / `Edit` / `Delete` returning `Result` with events, `UpdateAnnouncement` (host or admin) and `GetAnnouncementById` use cases, read visibility for host / accepted attendee (Attendance.Contracts `IsAcceptedAsync`) / admin (F-074), `xmin` (host and admins edit), migration with data conversion if the message column changes; (2) keyset read models (D-029) for the per-post list and the inbox / outbox (attended post IDs from Attendance.Contracts `GetAcceptedPostIdsAsync`; hosted posts need a Posts.Contracts port), additive endpoints; (3) gates as in 8.3/8.4 (coverage ≥ 70 %, architecture suite, `dotnet test Bingo.sln`, strict build, `has-pending-model-changes`, format), then ledger + TODO (*Module-endpoint behaviour* list for 10.6) and commit.
- **Half-done:** None.

## 3. Validation status

| Command | Result | Date |
|---------|--------|------|
| `dotnet build Bingo.sln -p:HopautStrictWarnings=true` | 0 errors, 0 compiler warnings (1 B-002 MSBuild notice) | 2026-11-17 |
| `dotnet build Bingo.sln -c Release -p:HopautStrictWarnings=true` | not re-run; still blocked by B-002 (`No Six Labors license found`) | 2026-11-17 |
| `dotnet test Bingo.sln` (Podman helper from §6) | 573 passed / 0 failed / 0 skipped (244 building-block unit, 43 architecture, 103 Posts unit, 63 Attendance unit, 25 Ratings unit, 2 Media unit, 93 integration) | 2026-11-17 |
| Coverage (`--collect:"XPlat Code Coverage"` on the module unit tests) | Attendance Domain 99.0 % / Application 99.2 %; Ratings Domain 98.1 % / Application 98.0 % | 2026-11-17 |
| `has-pending-model-changes` (Attendance, Posts, Ratings) | no pending changes | 2026-11-17 |
| `dotnet format Bingo.sln --verify-no-changes --include <changed files>` | exit 0 | 2026-11-17 |
| `dotnet list Bingo.sln package --vulnerable --include-transitive` | not re-run (no package changes this session); last run clean (57 projects) | 2026-11-17 |

## 4. Schema changes

All migrations below are applied only to Testcontainers databases (and to a local Development database when `Database:MigrateOnStartup` is set); plan Step 11.1 lists the matching legacy-import conversions.

| Module | Migration | Applied where | Data-conversion script |
|--------|-----------|---------------|------------------------|
| Posts, Ratings, Attendance, Announcements, Moderation, BugReports | `…IdSequence` ×6 (7.6) | Testcontainers only | None; Step 11.1 re-runs each `setval` after the import (D-019) |
| Posts | `PostStatusAndRequiredTitle`, `EntranceFeeMoney`, `PostVersion`, `TagNamesAndIdSequence`, `ChildrenKeyedByPost` (8.2) | Testcontainers only | In the migrations (see the 8.2 ledger row); `PostsMigrationTests` |
| Attendance | `20261117231901_ParticipationVersion` (8.3) | Testcontainers only | None (`xmin` is a system column; no SQL) |
| Ratings | `20261117233938_RatingFeedbackAndReputationVersion` (8.4) | Testcontainers only | In the migration: feedback trimmed, cut to 300, blank → NULL (one-way), then `varchar(300)`; reputation `xmin` (no SQL); `RatingsMigrationTests` |

## 5. Contract changes (backend ↔ mobile)

No handoff is due: BingoMobile does not call the module endpoints yet (D-007), so every contract-inventory row stays `unsynced` until its Phase 10 step. The behaviour the Phase 10 handoffs must describe is listed in `TODO.md` → *Contract inventory* → "Module-endpoint behaviour already changed before its Phase 10 sync" (this session added the 8.3 Attendance and 8.4 Ratings items, including the new `GET /api/v1/posts/{postId}/attendees`, `GET /api/v1/users/me/attendances` and `GET /api/v1/ratings/{ratingId}`).

| Endpoint | Change | Breaking? | Handoff file | Mobile status |
|----------|--------|-----------|--------------|---------------|
| None this session | – | – | – | – |

## 6. Open decisions and blockers
- **B-002 Six Labors Community license key** (blocks Release builds, `dotnet publish`, the Docker image and the rest of 7.8) → unblock action: the user applies for the free Community license at https://licensing.sixlabors.com/ (non-profit; an assembly-scoped key must cover `Hopaut.Modules.Media.Infrastructure`) and supplies it as the `SixLaborsLicenseKey` environment variable or the git-ignored repository-root `sixlabors.lic`. Never commit the key. Then run `dotnet build Bingo.sln -c Release -p:HopautStrictWarnings=true` and flip 7.8 ✅.
- **Running the Host locally** needs `JwtSettings:Secret` (≥ 32 characters) via `dotnet user-secrets` or `JwtSettings__Secret` (README.md).
- **Container runtime on this machine:** `podman machine ssh "sudo systemd-run --unit=hopaut-podman-tcp podman system service --time=0 tcp://127.0.0.1:2375"`, then `$env:DOCKER_HOST='tcp://127.0.0.1:2375'; $env:TESTCONTAINERS_RYUK_DISABLED='true'` in the same PowerShell command as `dotnet test`; stop with `podman machine ssh "sudo systemctl stop hopaut-podman-tcp"` (`tests/README.md`). Stopped at the end of this session.
- **EF migrations:** `dotnet ef migrations add <Name> --project src\Modules\<X>\Hopaut.Modules.<X>.Infrastructure --startup-project src\Host\Hopaut.Api.Host --context <X>ModuleDbContext --output-dir Migrations` (only the Host references `Microsoft.EntityFrameworkCore.Design`).
- **Layer rule to remember:** `*.Api` must not reference its own Domain (`ARCH-LAYER-API`), so Application DTOs carry wire-ready values (e.g. `Attendee.Status` as a string).
- **Open findings created this session:** F-070 (first-and-last-name rule for attend and create post → 8.8 with a Users.Contracts read port), F-071 (concurrent acceptances can exceed slots → 9.3 capacity guard), F-074 (announcement reads unauthorized → 8.5).
- **EF Core limitation (D-029):** do not compose LINQ over `FromSql` for entities with complex types; page keys with `Database.SqlQuery<T>` and project by ID instead.
- **Final merge (D-015, Step 11.3):** only after 6.13, one `git merge --no-ff migrate/net10` into `master`, done locally; the user pushes.

## 7. Prompt – next backend session

```text
Agent: HopautArchitect (workspace C:\Users\ghmi\personal\BingoAPI)
Resume from HANDOVER.md. Next step: 8.5 – Announcements (plan § Phase 8 checklist and § Step 8.5; TODO.md Now; finding F-074). Next action: git status; start the Podman API helper from HANDOVER.md §6; load the clean-architecture-ddd, messaging-integration and rest-api-standards skills; follow HANDOVER.md §2 (survey facts, then the suggested batches), committing after each green batch.
Do not redo completed work; verify the validation status in HANDOVER.md §3 first (dotnet build Bingo.sln -p:HopautStrictWarnings=true; dotnet test Bingo.sln with the Podman variables).
Reuse the 8.2–8.4 patterns: Result-returning verbs with a per-aggregate error catalog, facts from other modules through their Contracts ports, xmin concurrency (D-027), keyset read models via SqlQuery (D-029), value objects with migrations that convert stored data, unit tests from templates/Module, architecture baseline entries removed as they are fixed.
Then continue with Phase 8 (8.6 → 8.12). 7.8 only needs the Release build once the user supplies the Six Labors key (B-002).
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
