---
name: architect-handoff
description: "State, progress and handover files for the HopautArchitect migration. Use when: creating or updating TODO.md, recording audit findings, decisions or blockers, updating hopaut-migration-status.md, maintaining the backend-to-mobile contract inventory, writing a mobile handoff in docs/mobile-handoffs, or writing HANDOVER.md before a session ends with work remaining."
user-invocable: false
---

# Architect state and handover

Leading word: **resumable**. Any session may be the last of its context window; the files below must let a new session, with no memory, continue exactly where this one stopped.

## File map

| File | Purpose | Lifecycle |
|------|---------|-----------|
| `TODO.md` (repo root) | Working queue, audit findings, contract inventory, decisions, blockers | Created in Phase 1 from [templates/TODO.md](./templates/TODO.md); updated after every work item |
| `hopaut-migration-status.md` | Step ledger: status, date, notes, commit per step + activity log | Existing rules in `.github/github-copilot-instructions.md`; updated after every change |
| `hopaut-execution-plan.md` | Step definitions (Goal / Actions / Acceptance) | Append Phase 7+ steps; never rewrite completed steps |
| `docs/mobile-handoffs/YYYY-MM-DD-<phase>-<step>-<slug>.md` | One per contract change | From `docs/mobile-handoffs/_TEMPLATE.md`; never edit once `applied` |
| `C:\Users\ghmi\personal\BingoMobile\mobile-migration-status.md` | Mobile ledger | Activity row for every contract change you apply or hand off |
| `HANDOVER.md` (repo root) | Resume point for the next session | From [templates/HANDOVER.md](./templates/HANDOVER.md) whenever a session stops with work left; deleted when everything is done |

## Separation of concerns

- **Step status lives only in the ledger.** TODO.md references step IDs; it never repeats their status text, so the two cannot drift.
- TODO.md holds what the plan does not: finding IDs (`F-###`) with locations and target steps, decisions (`D-###`, plus the skills' `D-MSG-*` / `D-API-*` defaults when overridden), blockers, and the contract inventory.
- Every finding is linked to the step that fixes it. A finding without a step means a step is missing from the plan.

## Writing rules

- Write for a reader with zero context: absolute paths for anything outside the repository, step IDs, file paths, exact commands.
- Facts carry evidence: the command and its result, the test name, or the commit hash.
- Dates are UTC `YYYY-MM-DD`.
- Concise: tables and bullets, no narrative; refer to other files instead of copying them.
- Never include secrets, tokens, connection strings or personal data.

## When to update what

| Event | TODO.md | Ledger | Handoff | HANDOVER.md |
|-------|---------|--------|---------|-------------|
| Work item started | move to *Now* | step 🟡 + date + remaining work | | |
| Finding discovered | add `F-###` | | | |
| Decision taken or default overridden | add `D-###` | activity row if it changes the plan | | |
| Step completed | *Done* + close findings | ✅ + notes + commit + activity row | | |
| Contract changed | update inventory row | activity row naming the handoff | new file | |
| Blocked | *Blockers* with the unblock action | ❌ + reason | | |
| Session ends with work left | refresh *Now* / *Next* | consistent with TODO.md | | write or overwrite |
| Session resumes from HANDOVER.md | merge any facts it holds that TODO.md lacks | | | delete once absorbed and the work continues |

## HANDOVER.md requirements

Every section of the template is mandatory; write "None." rather than deleting a section. It must contain:
- the exact resume point: step ID, the next concrete action, and anything half-done;
- validation status: the last build and test commands with their results;
- schema changes: each EF migration with where it has been applied, plus any data-conversion script;
- contract changes: each endpoint with its handoff file and mobile status;
- open decisions and blockers, each with the unblock action;
- two ready-to-paste prompts, one for the next backend session and one for BingoMobile that is precise enough to complete the mobile client work without opening the backend repository.

## Completion criterion

State files are correct only when a new session reading HANDOVER.md (if present), TODO.md and the ledger, and nothing else, can name the next step, the next action and the current validation status, and no step status in TODO.md contradicts the ledger.
