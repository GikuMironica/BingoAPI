# Handover – Hopaut architecture migration

**Written:** 2026-11-17 by HopautArchitect · **Backend:** `migrate/net10` @ `7fbd734` + the commit that adds this file (tree clean) · **Mobile:** `dev` @ `b20be13` (tree clean, unchanged this session)

## 1. State
Phase 7 continues. This session completed **7.6** (`6dbbb7c`: MediatR → Mediator 3.0.2, `Result` everywhere, Logging → Validation → Caching → per-module unit of work, domain events dispatched before the single save, sequence-assigned IDs for the seven `int`-key aggregates with six migrations) and **7.7** (`7fbd734`: JWT bearer from validated options, correlation id, `/health/live` + `/health/ready`, OTLP, options validated on start, Admin-only Hangfire dashboard, forwarded headers/CORS). Both are ✅ in the ledger with evidence; 0.7, 0.9 and 0.10 closed. This file is a checkpoint written before the structural Step 6.6; if a later commit of this session completes 6.6 it deletes this file.

## 2. Resume point
- **Next step:** 6.6 – Contracts projects per module (`hopaut-execution-plan.md` § Step 6.6, including the F-052 addendum)
- **Next action:** run `git status`; if the tree is clean, load the messaging-integration, clean-architecture-ddd and dotnet-10-migration skills, mark 6.6 🟡, then:
  1. Create `src/Modules/<X>/Hopaut.Modules.<X>.Contracts` (SharedKernel + IntegrationEvents references only) for every module; add them to `Bingo.sln` (`dotnet sln add`). The architecture tests pick them up automatically (glob references).
  2. Move the integration events from `*.Application/IntegrationEvents/` (Posts, Attendance, Announcements, Ratings) into their Contracts project; the Infrastructure domain-event handlers keep writing them through `IIntegrationEventPublisher`.
  3. Ratings.Contracts: read-only `IUserReputationQueries.GetAsync(IReadOnlyCollection<UserId>, CancellationToken)` + DTO, implemented in Ratings.Infrastructure (reuse `IUserReputationRepository.GetBatchAsync`) and registered by the Ratings installer; Posts.Infrastructure's `RatingsUserReputationProvider` uses it instead of sending `GetReputationsForUsersQuery`; replace the `Posts.Infrastructure → Ratings.Application` project reference with `→ Ratings.Contracts` (F-023).
  4. Move every `AddXxxModule` installer from `*.Api` into `*.Infrastructure` (Identity's JWT bearer wiring moves with it; Infrastructure may use ASP.NET Core); `*.Api` keeps `MapXxxEndpoints`; the Host then references each module's Infrastructure project directly. Remove the 12 `ARCH-LAYER-API` (F-052) entries and the `ARCH-LAYER-INFRASTRUCTURE` (F-023) entry from `tests/Hopaut.ArchitectureTests/ArchitectureBaseline.cs` as they disappear, and close F-023/F-052 in `TODO.md`.
  5. Ladder: `dotnet build Bingo.sln`, `dotnet test tests/Hopaut.ArchitectureTests`, `dotnet test Bingo.sln` (Podman helper from §6), `has-pending-model-changes` ×9 (command in `TODO.md` *Baseline*), `dotnet format --verify-no-changes --include <changed files>`; then ledger + TODO + commit.
- **Half-done:** None at the time of writing. If `git status` shows uncommitted 6.6 changes, a later part of this session was interrupted: review them against the list above before continuing.

## 3. Validation status

| Command | Result | Date |
|---------|--------|------|
| `dotnet build Bingo.sln` | 0 errors; 1 warning (Six Labors license notice of ImageSharp 3.2 in Debug builds, B-002) | 2026-11-17 |
| `dotnet test Bingo.sln` (Podman helper from §6) | 183 passed / 0 failed / 0 skipped (101 building-block unit, 43 architecture, 2 Media unit, 37 integration) | 2026-11-17 |
| `has-pending-model-changes` ×9 (8 module contexts + `OutboxDbContext`) | no pending changes ×9 (after 7.6; Identity and Posts re-checked after 7.7) | 2026-11-17 |
| `dotnet list Bingo.sln package --vulnerable --include-transitive` | none (52 projects) | 2026-11-17 |
| `dotnet format Bingo.sln --verify-no-changes --include <files changed in 7.6/7.7>` and all 39 migration files | clean | 2026-11-17 |
| Level 4 (`-c Release -p:HopautStrictWarnings=true`, solution-wide `dotnet format --verify-no-changes`) | not run yet (Phase 7 gate 7.8); `-c Release` needs the B-002 key | – |

## 4. Schema changes

| Module | Migration | Applied where | Data-conversion script |
|--------|-----------|---------------|------------------------|
| Posts | `20261117173750_PostIdSequence` | Testcontainers only (opt-in Development via `Database:MigrateOnStartup`) | None; Step 11.1 must re-run the `setval` after importing legacy rows (D-019) |
| Ratings | `20261117173752_RatingIdSequence` | Testcontainers only | as above |
| Attendance | `20261117173754_ParticipationIdSequence` | Testcontainers only | as above |
| Announcements | `20261117173756_AnnouncementIdSequence` | Testcontainers only | as above |
| Moderation | `20261117173758_ReportIdSequences` | Testcontainers only | as above |
| BugReports | `20261117173801_BugIdSequence` | Testcontainers only | as above |

Each creates `<schema>.<aggregate>_ids`, drops the identity of `"Id"` and continues after `MAX("Id")`; `Down` restores the identity after the highest ID (SQL reviewed with `dotnet ef migrations script`).

## 5. Contract changes (backend ↔ mobile)

None for BingoMobile: it does not call the module endpoints yet (contract inventory in `TODO.md`, D-007), so every inventory row stays `unsynced` until its Phase 10 step. Observable module-endpoint changes this session, to be listed in the Phase 10 handoffs: protected endpoints now answer 401 without a valid bearer token (before 7.7 they could not authenticate at all); invalid Identity login/register input gets 400 with validation messages (D-020); responses carry `X-Correlation-Id`; `/health` became `/health/live` + `/health/ready`. JWT claims are unchanged (`sub` still missing, F-042 → 8.9/10.2).

## 6. Open decisions and blockers
- **B-002 Six Labors Community license key** (blocks Release builds, `dotnet publish`, the Docker image and the 7.8 gate; Debug builds and tests are unaffected) → unblock action: the user applies for the free Community license at https://licensing.sixlabors.com/ (Hopaut qualifies as a non-profit; an assembly-scoped key must cover `Hopaut.Modules.Media.Infrastructure`) and supplies it as the `SixLaborsLicenseKey` environment variable or the git-ignored repository-root `sixlabors.lic`. Never commit the key.
- **Running the Host locally now requires `JwtSettings:Secret`** (≥ 32 characters) via `dotnet user-secrets` or `JwtSettings__Secret` (README.md); `docker compose` refuses to start without it.
- **Container runtime on this machine** (not a blocker): before integration tests run `podman machine ssh "sudo systemd-run --unit=hopaut-podman-tcp podman system service --time=0 tcp://127.0.0.1:2375"`, then set `$env:DOCKER_HOST='tcp://127.0.0.1:2375'; $env:TESTCONTAINERS_RYUK_DISABLED='true'` in the same PowerShell command as `dotnet test`. Stop it afterwards with `podman machine ssh "sudo systemctl stop hopaut-podman-tcp"`. Details: `tests/README.md`.
- **Final merge (D-015, Step 11.3):** only after 6.13, one `git merge --no-ff migrate/net10` into `master`, done locally; the user pushes. BingoMobile `master` is the user's to merge.
- **D-011** xunit.v3 stays on 3.2.x (VSTest); moving to 4.x needs Microsoft Testing Platform mode as its own step.

## 7. Prompt – next backend session

```text
Agent: HopautArchitect (workspace C:\Users\ghmi\personal\BingoAPI)
Resume from HANDOVER.md. Next step: 6.6 – Contracts projects per module (plan § Step 6.6 incl. the F-052 addendum). Next action: git status; load the messaging-integration, clean-architecture-ddd and dotnet-10-migration skills, mark 6.6 🟡, then follow HANDOVER.md §2 items 1–5.
Do not redo completed work; verify the validation status in HANDOVER.md §3 first (start the Podman API helper from §6 before `dotnet test Bingo.sln`).
Then continue with 6.9 and the 7.8 gate per hopaut-execution-plan.md (the 7.8 Release build needs the Six Labors key, B-002).
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
