# Handover – Hopaut architecture migration

**Written:** 2026-11-17 by HopautArchitect · **Backend:** `migrate/net10` @ `5fd133d` + the commit that adds this file (tree clean) · **Mobile:** `dev` @ `b20be13` (tree clean, unchanged this session)

## 1. State
Phase 7 is done except for the Release build of the 7.8 gate (B-002). This session completed **7.6** (`6dbbb7c`), **7.7** (`7fbd734`), **6.6** (`1d33227`: Contracts projects for Posts, Attendance, Announcements and Ratings; installers in Infrastructure; F-023/F-052 fixed; module template refreshed) and **6.9** (`0a850c1`: `IResourceAuthorizer` port + ASP.NET Core adapter, generic resource-owner policy, per-module policies, Admin-only report endpoints; F-009–F-012 and F-062 fixed), and ran the **7.8** gate (`5fd133d`: strict Debug build clean, Release strict build fails only on B-002, solution-wide format clean after F-063, vulnerable scan clean). 7.8 stays 🟡 until the Six Labors key exists. This file is a checkpoint written before Phase 8; a later commit of this session that finishes or checkpoints 8.1 replaces or deletes it.

## 2. Resume point
- **Next step:** 8.1 – SharedKernel value-object catalog (`hopaut-execution-plan.md` § Step 8.1 and the Phase 8 checklist)
- **Next action:** run `git status`; if the tree is clean, load the clean-architecture-ddd and dotnet-10-migration skills (pattern code: `.github/skills/clean-architecture-ddd/references/patterns.md` §3), mark 8.1 🟡, then:
  1. Replace `src/BuildingBlocks/Hopaut.SharedKernel/ValueObjects.cs` with valid-by-construction `sealed record`s (private constructor + `static Result<T> Create(...)`, normalisation on creation, an error catalog with stable `Error.Validation` codes): `Coordinates` (WGS-84, finite; replaces the `IsValid` flag; unused today), `TimeRange` (end after start, UTC), `Currency` (ISO-4217 format) + `Money` (amount ≥ 0, 2 decimals; unused today), `RatingValue` (1–5; today it throws and is used by `Rating.Rate`, the Ratings EF converter and `CreateRatingCommandHandler`, so an out-of-range rate is a 500 today – keep the wire, map the failure to a validation error), `EmailAddress`, `PostTitle` / `Description` (length rules from the Posts EF configuration), `Capacity` (> 0), `RadiusMeters` (bounded), `PageSize` (default 20, max 100) / `Cursor` (opaque base64url), `MediaKey` (safe S3 key: no leading `/`, no `..`, allow-listed characters).
  2. EF complex-type mapping helpers in BuildingBlocks.Infrastructure for the value objects modules will persist (Money, TimeRange; RatingValue converter that goes through the factory).
  3. Unit tests in `tests/Hopaut.BuildingBlocks.UnitTests/SharedKernel/` for every factory: valid input, each invalid case, normalisation.
  4. Ladder: `dotnet build Bingo.sln`, `dotnet test tests/Hopaut.ArchitectureTests`, `dotnet test Bingo.sln` (Podman helper from §6), `has-pending-model-changes` ×9, `dotnet format --verify-no-changes` (solution-wide is clean since 7.8, keep it so); then ledger + TODO + commit.
- **Half-done:** None at the time of writing. If `git status` shows uncommitted 8.1 changes, a later part of this session was interrupted: review them against the list above before continuing.

## 3. Validation status

| Command | Result | Date |
|---------|--------|------|
| `dotnet build Bingo.sln` | 0 errors; 1 warning (Six Labors license notice of ImageSharp 3.2 in Debug builds, B-002) | 2026-11-17 |
| `dotnet build Bingo.sln -p:HopautStrictWarnings=true` | 0 errors, 0 compiler warnings | 2026-11-17 |
| `dotnet build Bingo.sln -c Release -p:HopautStrictWarnings=true` | 51 assemblies build; 1 error: `No Six Labors license found` (B-002) | 2026-11-17 |
| `dotnet test Bingo.sln` (Podman helper from §6) | 235 passed / 0 failed / 0 skipped (125 building-block unit, 43 architecture, 2 Media unit, 65 integration) | 2026-11-17 |
| `has-pending-model-changes` ×9 (8 module contexts + `OutboxDbContext`) | no pending changes ×9 | 2026-11-17 |
| `dotnet list Bingo.sln package --vulnerable --include-transitive` | none (56 projects) | 2026-11-17 |
| `dotnet format Bingo.sln --verify-no-changes` (solution-wide) | exit 0 | 2026-11-17 |

## 4. Schema changes

| Module | Migration | Applied where | Data-conversion script |
|--------|-----------|---------------|------------------------|
| Posts | `20261117173750_PostIdSequence` | Testcontainers only (opt-in Development via `Database:MigrateOnStartup`) | None; Step 11.1 must re-run the `setval` after importing legacy rows (D-019) |
| Ratings | `20261117173752_RatingIdSequence` | Testcontainers only | as above |
| Attendance | `20261117173754_ParticipationIdSequence` | Testcontainers only | as above |
| Announcements | `20261117173756_AnnouncementIdSequence` | Testcontainers only | as above |
| Moderation | `20261117173758_ReportIdSequences` | Testcontainers only | as above |
| BugReports | `20261117173801_BugIdSequence` | Testcontainers only | as above |

No schema change since these (6.6, 6.9 and 7.8 changed no model).

## 5. Contract changes (backend ↔ mobile)

None for BingoMobile: it does not call the module endpoints yet (contract inventory in `TODO.md`, D-007), so every inventory row stays `unsynced` until its Phase 10 step. Observable module-endpoint behaviour to list in the Phase 10 handoffs (in addition to the 7.7 items: 401 without a valid token, `X-Correlation-Id`, `/health/live` + `/health/ready`):
- 6.9 authorization (D-024): deleting a post needs its host or an admin; creating or deleting an announcement needs the post host or an admin (403 `announcements.announcement.not_post_host` on create, 404 on delete); accepting or rejecting an attendee needs the post host (admins too get 404); deleting a rating needs its author or an admin (the legacy API allowed only admins); profile and picture updates need the profile owner or an admin; listing open post reports and resolving a report need the Admin role (403 otherwise).
- Attendance request/cancel/check and post deletion read the caller from the token through `ICurrentUser` (no body or route user id for the caller).

## 6. Open decisions and blockers
- **B-002 Six Labors Community license key** (blocks Release builds, `dotnet publish`, the Docker image and the rest of the 7.8 gate; Debug builds and tests are unaffected) → unblock action: the user applies for the free Community license at https://licensing.sixlabors.com/ (Hopaut qualifies as a non-profit; an assembly-scoped key must cover `Hopaut.Modules.Media.Infrastructure`) and supplies it as the `SixLaborsLicenseKey` environment variable or the git-ignored repository-root `sixlabors.lic`. Never commit the key. Then run `dotnet build Bingo.sln -c Release -p:HopautStrictWarnings=true` and flip 7.8 ✅.
- **Running the Host locally requires `JwtSettings:Secret`** (≥ 32 characters) via `dotnet user-secrets` or `JwtSettings__Secret` (README.md); `docker compose` refuses to start without it.
- **Container runtime on this machine** (not a blocker): before integration tests run `podman machine ssh "sudo systemd-run --unit=hopaut-podman-tcp podman system service --time=0 tcp://127.0.0.1:2375"`, then set `$env:DOCKER_HOST='tcp://127.0.0.1:2375'; $env:TESTCONTAINERS_RYUK_DISABLED='true'` in the same PowerShell command as `dotnet test`. Stop it afterwards with `podman machine ssh "sudo systemctl stop hopaut-podman-tcp"`. Details: `tests/README.md`.
- **Final merge (D-015, Step 11.3):** only after 6.13, one `git merge --no-ff migrate/net10` into `master`, done locally; the user pushes. BingoMobile `master` is the user's to merge.
- **D-011** xunit.v3 stays on 3.2.x (VSTest); moving to 4.x needs Microsoft Testing Platform mode as its own step.

## 7. Prompt – next backend session

```text
Agent: HopautArchitect (workspace C:\Users\ghmi\personal\BingoAPI)
Resume from HANDOVER.md. Next step: 8.1 – SharedKernel value-object catalog (plan § Step 8.1 and the Phase 8 checklist). Next action: git status; load the clean-architecture-ddd and dotnet-10-migration skills, mark 8.1 🟡, then follow HANDOVER.md §2 items 1–4.
Do not redo completed work; verify the validation status in HANDOVER.md §3 first (start the Podman API helper from §6 before `dotnet test Bingo.sln`).
Then continue with Phase 8 (8.2 → 8.12) per hopaut-execution-plan.md. 7.8 only needs the Release build once the user supplies the Six Labors key (B-002).
Commit everything you change on migrate/net10, one commit per step. The user wants one big `git merge --no-ff migrate/net10` into master only at the very end (Step 11.3, D-015): never merge earlier, never push.
```

## 8. Prompt – BingoMobile session (mobile client work)

```text
Workspace: C:\Users\ghmi\personal\BingoMobile, branch dev.
There are no pending backend contract changes as of 2026-11-17: no file in C:\Users\ghmi\personal\BingoAPI\docs\mobile-handoffs\ has Status `pending`.
Do not change routes, models, repositories or error handling for backend reasons in this session, and do not change UI/UX.
For context only: the backend's module endpoints do not yet match lib/config/constants/api.dart (PascalCase, `Data` envelope, Unix seconds). They are synchronised module by module in backend Phase 10, each with a handoff file and an `applied <date> <commit>` status. Continue the mobile plan in mobile-migration-status.md instead (Phase 0 is blocked at Step 0.3-H, HERE SDK).
Commit locally on dev only. At the very end of the migration the user merges dev into master themselves (backend Step 11.3 / D-015); do not touch master.
```
