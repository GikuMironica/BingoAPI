# Mobile handoffs

This folder is the **cross-repo bridge** between Hopaut backend (`BingoAPI`) and Hopaut mobile (`BingoMobile`).

Whenever a backend change alters the public HTTP / SignalR / auth contract, the assistant working in `BingoAPI` writes a handoff prompt in this folder. The user then pastes that prompt into the assistant chat in the sibling `BingoMobile` workspace, which uses it to update the Flutter client to match.

## Rules

* **One file per change.** Naming: `YYYY-MM-DD-<phase>-<step>-<short-slug>.md`.
* **Use `_TEMPLATE.md` as the starting point.**
* **Status field** in the file moves through:
  * `pending` – written, not yet applied on mobile.
  * `applied YYYY-MM-DD <commit>` – mobile applied the change.
  * `superseded by <other-handoff>` – contract changed again before mobile caught up.
* **Never edit an applied handoff.** Write a new file instead.
* **Index this folder by date.** The newest file at the top of the activity log in `hopaut-migration-status.md` should match the newest file here.

## What triggers a handoff

See *Backend ↔ Mobile contract sync* in `.github/github-copilot-instructions.md`.

## What does not

Internal refactors, tests, docs, performance work that preserves shape and semantics. When in doubt, write the handoff.
