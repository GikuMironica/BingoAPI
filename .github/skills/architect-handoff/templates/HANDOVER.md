# Handover – Hopaut architecture migration

**Written:** YYYY-MM-DD by HopautArchitect · **Backend:** `<branch>` @ `<commit>` (tree clean | dirty: <why>) · **Mobile:** `dev` @ `<commit>` (tree clean | dirty: <why>)

## 1. State
<Three to five sentences: what is done, what is in flight, and why the session stopped.>

## 2. Resume point
- **Next step:** <step id> – <title> (`hopaut-execution-plan.md` § <step id>)
- **Next action:** <the exact first thing to do>
- **Half-done:** <files or changes started but not finished, or "None.">

## 3. Validation status

| Command | Result | Date |
|---------|--------|------|
| `dotnet build Bingo.sln` | <ok / errors> | YYYY-MM-DD |
| `dotnet test Bingo.sln` | <passed / failed / skipped> | YYYY-MM-DD |

## 4. Schema changes

| Module | Migration | Applied where | Data-conversion script |
|--------|-----------|---------------|------------------------|
| <module> | `<MigrationName>` | Testcontainers only / local | <path or "None."> |

## 5. Contract changes (backend ↔ mobile)

| Endpoint | Change | Breaking? | Handoff file | Mobile status |
|----------|--------|-----------|--------------|---------------|
| `<METHOD> /api/v1/...` | <added / changed / removed> | yes / no | `docs/mobile-handoffs/<file>.md` | applied `<commit>` / pending |

## 6. Open decisions and blockers
- <item> → unblock action: <what the user or the next session must do>

## 7. Prompt – next backend session

```text
Agent: HopautArchitect (workspace C:\Users\ghmi\personal\BingoAPI)
Resume from HANDOVER.md. Next step: <step id> – <title>. Next action: <action>.
Do not redo completed work; verify the validation status in HANDOVER.md §3 first.
```

## 8. Prompt – BingoMobile session (mobile client work)

```text
Workspace: C:\Users\ghmi\personal\BingoMobile, branch dev.
Apply the backend contract changes below. Do not continue the mobile playbook in this session, and do not change UI/UX.
For each handoff, read it from C:\Users\ghmi\personal\BingoAPI\docs\mobile-handoffs\<file>.md and update:
- routes: lib/config/constants/api.dart
- models: lib/data/models/<file>.dart (fromJson / toJson field names and types)
- calls and error mapping: lib/data/repositories/<file>.dart, lib/services/dio_service.dart
Pending handoffs (oldest first):
1. <file> – <one-line summary of the wire change>
Validate with `fvm dart format` on changed files, plus `fvm flutter analyze` and `fvm flutter test` if dependency resolution works; report why if not.
Commit locally on dev, add an activity row to mobile-migration-status.md, and reply with the commit hash per handoff
so the next backend session can set each handoff's Status to `applied <date> <commit>`.
```
