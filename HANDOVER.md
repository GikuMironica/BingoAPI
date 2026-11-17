# Handover – Hopaut architecture migration

**Written:** 2026-11-17 by HopautArchitect · **Backend:** `migrate/net10` @ `8286bd4` + the handover commit (tree dirty only with foreign work: `.github/github-copilot-instructions.md` deleted and untracked `.github/agents/`, `.github/skills/`, `.github/copilot-instructions.md` – the user's agent setup, never touched) · **Mobile:** `dev` @ `b20be13` (tree clean, 10 commits ahead of `origin/dev`)

## 1. State
Phase 1 (audit) is complete: `TODO.md` holds 56 findings, the BingoMobile contract inventory (43 rows, all unsynced), decisions D-001…D-013 and blocker B-001. The ledger was reconciled (32 ✅ steps reopened with the step that closes them) and the plan was extended with Phases 7–11. In Phase 7, steps 7.1, 6.7, 7.2, 7.3 and 7.4 are ✅ (closing 0.6, 1.8, 3.2, 6.1, 6.4, 6.8). Running the system for the first time exposed and fixed three runtime defects: the Host could not start (F-054), four module EF models could not be built (F-056), and the outbox context was never registered (F-055, partly). The session stopped because its context budget was nearly used.

## 2. Resume point
- **Next step:** 7.5 – `TimeProvider`, Guid v7, SharedKernel base types, ID parsing/JSON (`hopaut-execution-plan.md` § Step 7.5)
- **Next action:** load the clean-architecture-ddd and dotnet-10-migration skills, mark 7.5 🟡 in the ledger, then start in `src/BuildingBlocks/Hopaut.SharedKernel`: `IDomainEvent`/`DomainEventBase` (`Guid.CreateVersion7()`, `DateTimeOffset OccurredOnUtc`), `Entity<TId>` transient-aware equality, `AggregateRoot<TId>.Raise` (protected, replaces public `AddDomainEvent`; update every aggregate factory), strongly-typed IDs with `IParsable<T>` rejecting ≤ 0 / empty, a System.Text.Json converter factory, typed EF converters replacing `StronglyTypedIdConverters` (`dynamic`/`Activator`). Then remove clocks and `Guid.NewGuid` listed in F-021 and delete the `ARCH-SHAREDKERNEL-EVENT-RAISER` baseline entry in `tests/Hopaut.ArchitectureTests/ArchitectureBaseline.cs`.
- **Half-done:** None.

## 3. Validation status

| Command | Result | Date |
|---------|--------|------|
| `dotnet build Bingo.sln` | 0 errors; 70 warnings, all NU1902/NU1903 for SixLabors.ImageSharp 3.1.7 (B-001); 0 compiler warnings | 2026-11-17 |
| `dotnet test Bingo.sln` (container setup from §6) | 84 passed / 0 failed / 0 skipped (33 architecture, 39 unit, 12 integration) | 2026-11-17 |
| `dotnet list Bingo.sln package --vulnerable --include-transitive` | only SixLabors.ImageSharp | 2026-11-17 |
| `dotnet tool run dotnet-ef migrations has-pending-model-changes` (all 9 contexts) | no pending changes | 2026-11-17 |
| Level 4 (`-c Release -p:HopautStrictWarnings=true`, `dotnet format --verify-no-changes`) | not run yet (Phase 7 gate 7.8); the strict build will fail on ImageSharp NU1903 until B-001 is resolved | – |

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
- **B-001 ImageSharp license** → unblock action: the user confirms Hopaut qualifies for the Apache-2.0 terms of the Six Labors Split License (open source, or < 1 M USD annual revenue, or non-profit) so 3.1.7 can be upgraded to the latest patched 3.1.x, **or** approves replacing ImageSharp with SkiaSharp (MIT) in Steps 8.10/9.5. Until then the Media projects carry high-severity advisories.
- **D-010 Hangfire license** (LGPL-3.0 / commercial) → unblock action: confirm before any Hangfire upgrade (none needed now).
- **Container runtime on this machine** (not a blocker): Podman's Docker-API pipe fails (`win-sshproxy.exe failed to start`). Before integration tests run `podman machine ssh "sudo systemd-run --unit=hopaut-podman-tcp podman system service --time=0 tcp://127.0.0.1:2375"`, then set `$env:DOCKER_HOST='tcp://127.0.0.1:2375'; $env:TESTCONTAINERS_RYUK_DISABLED='true'`. Stop it afterwards with `podman machine ssh "sudo systemctl stop hopaut-podman-tcp"` (stopped at the end of this session). Details: `tests/README.md`.
- **Foreign uncommitted `.github` changes** → unblock action: the user commits their agent/skills setup (or asks the agent to); HopautArchitect never stages them.
- **D-011** xunit.v3 stays on 3.2.x (VSTest); moving to 4.x requires switching `dotnet test` to Microsoft Testing Platform mode – a separate, user-visible step.

## 7. Prompt – next backend session

```text
Agent: HopautArchitect (workspace C:\Users\ghmi\personal\BingoAPI)
Resume from HANDOVER.md. Next step: 7.5 – TimeProvider, Guid v7, SharedKernel base types, ID parsing/JSON. Next action: load the clean-architecture-ddd and dotnet-10-migration skills, mark 7.5 🟡, and start with the SharedKernel changes listed in HANDOVER.md §2.
Do not redo completed work; verify the validation status in HANDOVER.md §3 first (start the Podman API helper from §6 before `dotnet test Bingo.sln`).
Then continue with 7.6, 7.7, 6.6, 6.9 and the 7.8 gate per hopaut-execution-plan.md.
```

## 8. Prompt – BingoMobile session (mobile client work)

```text
Workspace: C:\Users\ghmi\personal\BingoMobile, branch dev.
There are no pending backend contract changes as of 2026-11-17: no file in C:\Users\ghmi\personal\BingoAPI\docs\mobile-handoffs\ has Status `pending`.
Do not change routes, models, repositories or error handling for backend reasons in this session, and do not change UI/UX.
For context only: the backend's new module endpoints do not yet match the routes in lib/config/constants/api.dart (PascalCase, `Data` envelope, Unix seconds). They will be synchronised module by module in backend Phase 10, each with a handoff file and an `applied <date> <commit>` status. Continue the mobile plan in mobile-migration-status.md instead (Phase 0 is blocked at Step 0.3-H, HERE SDK).
```
