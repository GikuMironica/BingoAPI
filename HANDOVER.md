# Handover – Hopaut architecture migration

**Written:** 2026-11-17 by HopautArchitect · **Backend:** `migrate/net10` @ `8a4da1e` + the commit that adds this file (tree clean) · **Mobile:** `dev` @ `b20be13` (tree clean, unchanged this session)

## 1. State
Phase 7 continues: steps 7.1, 6.7, 7.2, 7.3, 7.4 and now **7.5** are ✅ (`8a4da1e`). 7.5 removed every hidden clock and `Guid.NewGuid` from `src` (`TimeProvider` + `DateTimeOffset now`, Guid v7 from the injected time, D-017), rebuilt the SharedKernel base types (transient-aware `Entity`, protected `AggregateRoot.Raise`, UTC/v7 event bases) and made all 13 strongly-typed IDs `IParsable` with validation, a JSON converter factory (registered per boundary later, D-016) and typed EF converters. A new `HandlerResolutionTests` exposed **F-058** (refresh handler cannot be constructed → 7.7), and the review exposed **F-057** (Infrastructure domain-event handlers and all pipeline behaviors are never registered → 7.6). The session stopped before 7.6 because that step is a solution-wide conversion and the context budget was largely used.

## 2. Resume point
- **Next step:** 7.6 – MediatR → Mediator, pipeline and module unit of work (`hopaut-execution-plan.md` § Step 7.6)
- **Next action:** load the messaging-integration, clean-architecture-ddd and dotnet-10-migration skills; mark 7.6 🟡 in the ledger; license-check `Mediator.Abstractions` / `Mediator.SourceGenerator` 3.x on nuget.org and record it as D-018 in `TODO.md`. Then follow the skill's MediatR → Mediator recipe. Facts already established:
  - 9 installers call `AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<…Application type>())` (Identity, Users, Media, Posts, Attendance, Announcements, Ratings, Moderation, BugReports); replace them with one `AddMediator(...)` in the Host that lists every Application **and** the Posts/Attendance/Ratings/Announcements Infrastructure assemblies (F-057); scoped lifetime.
  - 41 handlers: 32 request handlers in `*.Application` + 9 `INotificationHandler<…DomainEvent>` in `*.Infrastructure/DomainEventHandlers`. `IDomainEvent` (SharedKernel) derives from `MediatR.INotification`.
  - No `IPipelineBehavior` is registered today; `LoggingBehavior`, `ValidationBehavior` (throws, F-016) and `CachingBehavior` exist in `Hopaut.BuildingBlocks.Application`. FluentValidation validators are not registered (F-016).
  - `Result`/`Result<TValue> : Result` (`src/BuildingBlocks/Hopaut.SharedKernel/Result.cs:46,81`) do not yet implement the `IResultBase` / `IFailureFactory<TSelf>` hooks the behaviors need (clean-architecture-ddd `references/patterns.md` §1).
  - `ModuleDbContext.SaveChangesAsync` dispatches domain events after `base.SaveChangesAsync` (F-006); D-004 requires dispatch before the single save, looping until none remain, publishing by runtime type.
  - Handlers call repository `SaveChangesAsync` (F-034); endpoints must keep today's HTTP behaviour (D-007).
  - `tests/Hopaut.IntegrationTests/HandlerResolutionTests.cs` enumerates MediatR `IRequestHandler<,>`: switch it to Mediator's handler interfaces, extend it to notification handlers once F-057 is fixed, keep the F-058 entry until 7.7, and add the skill's test that a concrete domain-event handler runs.
- **Half-done:** None.

## 3. Validation status

| Command | Result | Date |
|---------|--------|------|
| `dotnet build Bingo.sln` | 0 errors; 1 warning (Six Labors license notice of ImageSharp 3.2 in Debug builds, B-002); 0 compiler warnings | 2026-11-17 |
| `dotnet test Bingo.sln` (container setup from §6) | 129 passed / 0 failed / 0 skipped (81 building-block unit, 33 architecture, 2 Media unit, 13 integration) | 2026-11-17 |
| `dotnet tool run dotnet-ef migrations has-pending-model-changes -p <Infrastructure project> -s src/Host/Hopaut.Api.Host -c <Context> --no-build` (8 module contexts + `OutboxDbContext` in `src/BuildingBlocks/Hopaut.BuildingBlocks.Infrastructure`) | no pending changes ×9 | 2026-11-17 |
| `dotnet list Bingo.sln package --vulnerable --include-transitive` | none (52 projects) | 2026-11-17 |
| `dotnet format Bingo.sln --verify-no-changes --include <files changed in 7.5>` | clean (import order fixed in touched files) | 2026-11-17 |
| Level 4 (`-c Release -p:HopautStrictWarnings=true`, solution-wide `dotnet format --verify-no-changes`) | not run yet (Phase 7 gate 7.8); `-c Release` needs the B-002 key | – |

## 4. Schema changes

None this session: 7.5 changed no EF model (typed converters are drop-in; `has-pending-model-changes` clean ×9). Existing baseline migrations (`InitialCreate` for Identity, Users, Posts, Attendance, Announcements, Ratings, Moderation, BugReports and the transitional `messaging` outbox) are applied only to Testcontainers PostGIS and, opt-in, Development (`Database:MigrateOnStartup`); legacy data conversion stays with Step 11.1.

## 5. Contract changes (backend ↔ mobile)

None. Routes, JSON shapes and status codes are unchanged: the ID JSON converter is not registered for HTTP until Step 10.1 (D-016), and JWT claims keep their names and semantics (`jti` is now a v7 Guid, `iat`/`nbf` are set explicitly to the same time). Per D-007 every contract-inventory row stays `unsynced` until its Phase 10 step.

## 6. Open decisions and blockers
- **B-002 Six Labors Community license key** (blocks Release builds, `dotnet publish`, the Docker image and the 7.8 gate; Debug builds and tests are unaffected) → unblock action: the user applies for the free Community license at https://licensing.sixlabors.com/ (Hopaut qualifies as a non-profit; an assembly-scoped key must cover `Hopaut.Modules.Media.Infrastructure`) and supplies it as the `SixLaborsLicenseKey` environment variable or the git-ignored repository-root `sixlabors.lic`. Never commit the key.
- **F-058** (refresh handler unresolvable; no configured, validated `JwtSettings:Secret`) is baselined in `HandlerResolutionTests` and fixed by 7.7 together with JWT bearer authentication (F-004); remove its baseline entry there.
- **Container runtime on this machine** (not a blocker): before integration tests run `podman machine ssh "sudo systemd-run --unit=hopaut-podman-tcp podman system service --time=0 tcp://127.0.0.1:2375"`, then set `$env:DOCKER_HOST='tcp://127.0.0.1:2375'; $env:TESTCONTAINERS_RYUK_DISABLED='true'` in the same PowerShell command as `dotnet test`. Stop it afterwards with `podman machine ssh "sudo systemctl stop hopaut-podman-tcp"` (stopped at the end of this session). Details: `tests/README.md`.
- **Final merge (D-015, Step 11.3):** only after 6.13, one `git merge --no-ff migrate/net10` into `master`, done locally; the user pushes. BingoMobile `master` is the user's to merge.
- **D-011** xunit.v3 stays on 3.2.x (VSTest); moving to 4.x needs Microsoft Testing Platform mode as its own step.

## 7. Prompt – next backend session

```text
Agent: HopautArchitect (workspace C:\Users\ghmi\personal\BingoAPI)
Resume from HANDOVER.md. Next step: 7.6 – MediatR → Mediator, pipeline and module unit of work. Next action: load the messaging-integration, clean-architecture-ddd and dotnet-10-migration skills, mark 7.6 🟡, license-check Mediator 3.x (record D-018), then apply the facts listed in HANDOVER.md §2 (includes F-057 and adapting HandlerResolutionTests).
Do not redo completed work; verify the validation status in HANDOVER.md §3 first (start the Podman API helper from §6 before `dotnet test Bingo.sln`).
Then continue with 7.7 (includes F-058), 6.6, 6.9 and the 7.8 gate per hopaut-execution-plan.md (the 7.8 Release build needs the Six Labors key, B-002).
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
