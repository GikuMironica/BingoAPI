# Handover – Hopaut architecture migration

**Written:** 2026-11-17 by HopautArchitect, updated the same day after the user's decisions · **Backend:** `migrate/net10` @ `42be344` + the commit that updates this file (tree clean: everything is committed, including the user's `.github` agent setup in `4307963`) · **Mobile:** `dev` @ `b20be13` (tree clean, 10 commits ahead of `origin/dev`)

## 1. State
Phase 1 (audit) is complete: `TODO.md` holds 56 findings, the BingoMobile contract inventory (43 rows, all unsynced), decisions D-001…D-015 and blocker B-002 (B-001 resolved). The ledger was reconciled (32 ✅ steps reopened with the step that closes them) and the plan was extended with Phases 7–11. In Phase 7, steps 7.1, 6.7, 7.2, 7.3 and 7.4 are ✅ (closing 0.6, 1.8, 3.2, 6.1, 6.4, 6.8). Running the system for the first time exposed and fixed three runtime defects: the Host could not start (F-054), four module EF models could not be built (F-056), and the outbox context was never registered (F-055, partly). The session stopped because its context budget was nearly used.

**User decisions (2026-11-17, after the first handover):**
- **ImageSharp:** Hopaut is a **non-profit**, so it uses ImageSharp under the Apache-2.0 terms of the Six Labors Split License. Upgraded 3.1.7 → **3.2.0**, which fixes all seven advisories (B-001 resolved, D-014, `42be344`). ImageSharp 3.2 checks for a license key in Release builds; the free Community key is blocker **B-002** (§6).
- **Hangfire:** LGPL-3.0 license **confirmed**, so upgrades are license-cleared (D-010).
- **Commit everything:** done. The `.github` agent setup is committed verbatim (`4307963`), and BingoAPI references now point to `.github/copilot-instructions.md`.
- **One big merge into `master` at the end:** planned as **Step 11.3**, which runs last after 6.13. It is a single `git merge --no-ff migrate/net10` with no intermediate merges, and the agent never pushes (D-015).

## 2. Resume point
- **Next step:** 7.5 – `TimeProvider`, Guid v7, SharedKernel base types, ID parsing/JSON (`hopaut-execution-plan.md` § Step 7.5)
- **Next action:** load the clean-architecture-ddd and dotnet-10-migration skills, mark 7.5 🟡 in the ledger, then start in `src/BuildingBlocks/Hopaut.SharedKernel`: `IDomainEvent`/`DomainEventBase` (`Guid.CreateVersion7()`, `DateTimeOffset OccurredOnUtc`), `Entity<TId>` transient-aware equality, `AggregateRoot<TId>.Raise` (protected, replaces public `AddDomainEvent`; update every aggregate factory), strongly-typed IDs with `IParsable<T>` rejecting ≤ 0 / empty, a System.Text.Json converter factory, typed EF converters replacing `StronglyTypedIdConverters` (`dynamic`/`Activator`). Then remove clocks and `Guid.NewGuid` listed in F-021 and delete the `ARCH-SHAREDKERNEL-EVENT-RAISER` baseline entry in `tests/Hopaut.ArchitectureTests/ArchitectureBaseline.cs`.
- **Half-done:** None.

## 3. Validation status

| Command | Result | Date |
|---------|--------|------|
| `dotnet build Bingo.sln` | 0 errors; 1 warning (the Six Labors license notice ImageSharp 3.2 emits in Debug builds, B-002); 0 compiler warnings | 2026-11-17 |
| `dotnet test Bingo.sln` (container setup from §6) | 86 passed / 0 failed / 0 skipped (33 architecture, 39 building-block unit, 2 Media unit, 12 integration) | 2026-11-17 |
| `dotnet list Bingo.sln package --vulnerable --include-transitive` | none | 2026-11-17 |
| `dotnet tool run dotnet-ef migrations has-pending-model-changes` (all 9 contexts) | no pending changes | 2026-11-17 |
| `dotnet build src/Modules/Media/Hopaut.Modules.Media.Infrastructure -c Release` | fails without a key: `No Six Labors license found` (B-002). With a deliberately invalid dummy `sixlabors.lic`, the validator reads and rejects the key both locally and inside a Podman container build (`--secret id=sixlabors_lic`), so the plumbing works | 2026-11-17 |
| Level 4 (`-c Release -p:HopautStrictWarnings=true`, `dotnet format --verify-no-changes`) | not run yet (Phase 7 gate 7.8); `-c Release` needs the B-002 key | – |

## 4. Schema changes

| Module | Migration | Applied where | Data-conversion script |
|--------|-----------|---------------|------------------------|
| Identity (`identity`) | `InitialCreate` | Testcontainers PostGIS only; Development via `Database:MigrateOnStartup` | None. (legacy data → Step 11.1) |
| Users (`users`) | `InitialCreate` | same | None. (11.1) |
| Posts (`posts`, PostGIS extension) | `InitialCreate` | same | None. (11.1) |
| Attendance (`attendance`) | `InitialCreate` | same | None. (11.1) |
| Announcements (`announcements`) | `InitialCreate` | same | None. (11.1) |
| Ratings (`ratings`) | `InitialCreate` | same | None. (11.1) |
| Moderation (`moderation`) | `InitialCreate` | same | None. (11.1) |
| BugReports (`bug_reports`) | `InitialCreate` | same | None. (11.1) |
| Transitional outbox (`messaging`) | `InitialCreate` | same | None. (replaced in 9.1) |

## 5. Contract changes (backend ↔ mobile)

None. No endpoint changed its route, payload or status codes. The Host could not start before F-054 was fixed, so no client ever reached these endpoints. Per D-007 every inventory row stays `unsynced` until its Phase 10 step.

## 6. Open decisions and blockers
- **B-002 Six Labors Community license key** (blocks Release builds, `dotnet publish`, the Docker image and the 7.8 gate; Debug builds and tests are unaffected) → unblock action: the user applies for the free Community license at https://licensing.sixlabors.com/ (Hopaut qualifies as a non-profit; an assembly-scoped key must cover `Hopaut.Modules.Media.Infrastructure`). Then supply it either as the `SixLaborsLicenseKey` environment variable (CI) or by saving it as `sixlabors.lic` at the repository root (git-ignored and docker-ignored; picked up by `Directory.Build.props`; container builds pass it with `--secret id=sixlabors_lic,src=sixlabors.lic`, see the Dockerfile header). Never commit the key.
- **Resolved 2026-11-17:** B-001 (ImageSharp license, non-profit → 3.2.0), the D-010 Hangfire license (confirmed), and the foreign `.github` changes (committed in `4307963`).
- **Final merge (D-015, Step 11.3):** only after 6.13, one `git merge --no-ff migrate/net10` into `master`, done locally. The user pushes. BingoMobile `master` is the user's to merge.
- **Container runtime on this machine** (not a blocker): Podman's Docker-API pipe fails (`win-sshproxy.exe failed to start`). Before integration tests run `podman machine ssh "sudo systemd-run --unit=hopaut-podman-tcp podman system service --time=0 tcp://127.0.0.1:2375"`, then set `$env:DOCKER_HOST='tcp://127.0.0.1:2375'; $env:TESTCONTAINERS_RYUK_DISABLED='true'`. Stop it afterwards with `podman machine ssh "sudo systemctl stop hopaut-podman-tcp"` (stopped at the end of this session). Details: `tests/README.md`.
- **Podman build secrets on Windows:** the Windows client cannot stage `--secret` files (`creating temp file … podman-build-secret`), so build the image inside the machine: `podman machine ssh "cd /mnt/c/Users/ghmi/personal/BingoAPI && sudo podman build --secret id=sixlabors_lic,src=sixlabors.lic -t hopaut-api ."`.
- **D-011** xunit.v3 stays on 3.2.x (VSTest); moving to 4.x requires switching `dotnet test` to Microsoft Testing Platform mode – a separate, user-visible step.

## 7. Prompt – next backend session

```text
Agent: HopautArchitect (workspace C:\Users\ghmi\personal\BingoAPI)
Resume from HANDOVER.md. Next step: 7.5 – TimeProvider, Guid v7, SharedKernel base types, ID parsing/JSON. Next action: load the clean-architecture-ddd and dotnet-10-migration skills, mark 7.5 🟡, and start with the SharedKernel changes listed in HANDOVER.md §2.
Do not redo completed work; verify the validation status in HANDOVER.md §3 first (start the Podman API helper from §6 before `dotnet test Bingo.sln`).
Then continue with 7.6, 7.7, 6.6, 6.9 and the 7.8 gate per hopaut-execution-plan.md (the 7.8 Release build needs the Six Labors key, B-002).
Commit everything you change on migrate/net10, one commit per step. The user wants one big `git merge --no-ff migrate/net10` into master only at the very end (Step 11.3, D-015): never merge earlier, never push.
```

## 8. Prompt – BingoMobile session (mobile client work)

```text
Workspace: C:\Users\ghmi\personal\BingoMobile, branch dev.
There are no pending backend contract changes as of 2026-11-17: no file in C:\Users\ghmi\personal\BingoAPI\docs\mobile-handoffs\ has Status `pending`.
Do not change routes, models, repositories or error handling for backend reasons in this session, and do not change UI/UX.
For context only: the backend's new module endpoints do not yet match the routes in lib/config/constants/api.dart (PascalCase, `Data` envelope, Unix seconds). They will be synchronised module by module in backend Phase 10, each with a handoff file and an `applied <date> <commit>` status. Continue the mobile plan in mobile-migration-status.md instead (Phase 0 is blocked at Step 0.3-H, HERE SDK).
Commit locally on dev only. At the very end of the migration the user merges dev into master themselves (backend Step 11.3 / D-015); do not touch master.
```
