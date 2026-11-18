# Hopaut Architecture TODO

> Owned by the HopautArchitect agent. Step status lives only in `hopaut-migration-status.md`; this file holds the working queue, audit findings, contract inventory, decisions and blockers.

**Last updated:** YYYY-MM-DD · **Phase:** 1–5 · **Current step:** <step id>

## Now
- [ ] <step id> – <one-line goal> (skills: <skill names>)

## Next
- [ ] <step id> – <one-line goal>

## Findings

Types: bug · smell · primitive-obsession · anemic-model · layering · messaging · rest · security · perf · test-gap · ledger

| ID | Type | Location | Finding | Evidence | Fix in step | Status |
|----|------|----------|---------|----------|-------------|--------|
| F-001 | bug | `src/...` | <what is wrong> | <command, test or line> | <step id> | open |

## Contract inventory

| Mobile constant / call | Mobile file | Backend today | Target endpoint | Breaking? | Handoff | Status |
|------------------------|-------------|---------------|-----------------|-----------|---------|--------|
| `API.RATINGS` read | `lib/data/repositories/rating_repository.dart` | <route or "missing"> | `GET /api/v1/ratings/{ratingId}` | yes / no | <file or –> | unsynced · synced · pending-mobile |

## Decisions

| ID | Date | Decision | Why | Rejected alternatives |
|----|------|----------|-----|-----------------------|
| D-001 | YYYY-MM-DD | <decision> | <evidence or constraint> | <alternatives> |

## Blockers
- None.

## Baseline
- Build: `dotnet build Bingo.sln` → <result> (YYYY-MM-DD)
- Tests: `dotnet test Bingo.sln` → <passed / failed / skipped> (YYYY-MM-DD)

## Done (latest 10)
- YYYY-MM-DD <step id>: <what changed>; validated by <command or test>; commit <hash>
