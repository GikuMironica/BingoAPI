# Handover – Hopaut architecture migration

**Written:** 2026-11-17 by HopautArchitect · **Backend:** `migrate/net10` @ `3e14aed` + the commit that adds this file (tree clean) · **Mobile:** `dev` @ `b20be13` (tree clean, unchanged this session)

## 1. State
Phase 7 is done except for the Release build of the 7.8 gate (B-002). This session completed **7.6** (`6dbbb7c`), **7.7** (`7fbd734`), **6.6** (`1d33227`), **6.9** (`0a850c1`), the **7.8** gate run (`5fd133d`, 7.8 stays 🟡 for B-002) and **8.1** (`c96d062`: SharedKernel value-object catalog, `ISingleValueObject` + `ValueObjectConverter`, `MapMoney`/`MapTimeRange`, F-064 fixed). **8.2 – Posts** is 🟡: batch 1 (`42b6c68`: aggregate encapsulation, `PostStatus`, value objects, verbs, host from the caller, migration `PostStatusAndRequiredTitle`, 39 baseline entries removed, Posts unit-test project) and batch 2 (`3e14aed`: `Money` entrance fee, `LegacyCurrencies`, migration `EntranceFeeMoney` with two-way data conversion) are done; batches 3 and 4 remain. A later commit that finishes 8.2 deletes this file.

## 2. Resume point
- **Next step:** 8.2 – Posts (`hopaut-execution-plan.md` § Phase 8 checklist and § Step 8.2)
- **Next action:** `git status`; load clean-architecture-ddd, dotnet-10-migration and rest-api-standards; continue with batch 3 below (batches 1–2 are committed); each batch ends with a green build, `dotnet test Bingo.sln` and a commit (ledger note "in progress – <what remains>").
- **Survey (2026-11-17):** `src/Modules/Posts` is ~800 lines. `Post` has private setters but public `List<Picture> Pictures` / `List<PostTag> Tags` and `int ActiveFlag` (1 active); children `Event`, `EventLocation`, `Picture`, `PostTag`, `Tag`, `RepeatableProperty` have public setters and child → root back-references (40 `ARCH-DOMAIN-SETTER`/`-COLLECTION` baseline entries, F-020). `Event` keeps `double? EntrancePrice` + `int? Currency` (legacy enum index 0–24, `[Range(0,24)]` in `pre-net10-baseline:Bingo.Contracts/V1/Requests/Post/CreatePostRequest.cs`), `string? Title`, `string Description`, `string? Requirements` (legacy max 500), `int? Slots`, `TypeSpecificData` (jsonb). `CreatePostCommand` takes the host `UserId` from the body (F-013) and its `TagNames` are ignored by the handler (tags are never saved – record as a finding). `GetNearbyAsync` loads aggregates with four `Include`s, no bound on `limit`/radius beyond the endpoint defaults (10 km, 50). Legacy rules: title ≤ 30, description 10–5000, event start not more than 30 min in the past, duration ≥ 15 min and < 12 h, ≤ 20 tags of ≤ 20 chars, radius 1–15 km (`pre-net10-baseline:BingoAPI/Validators/CreatePostValidator.cs`); legacy `PostRepository` (`pre-net10-baseline:BingoAPI/Models/SqlRepository/PostRepository.cs`) defines active/inactive and disable semantics – read it before defining `PostStatus`.
- **Batches:**
  1. *Encapsulation without schema change:* create `tests/Modules/Posts/Hopaut.Modules.Posts.UnitTests` from `templates/Module` (add to `Bingo.sln`); children get private setters and lose their back-references (`HasOne(...).WithOne()` keeps the FKs, so no migration); `Post.Pictures`/`Tags` become private lists exposed as `IReadOnlyList`; `PostStatus` enum mapped to the existing `ActiveFlag` column values (rename the column only with a migration); verbs `Create` (value objects from 8.1: `TimeRange` onto `EventTime`/`EndTime`, `PostTitle`, `PostDescription`, `Capacity`, `Coordinates` → `Point` with SRID 4326), `Reschedule`, `Disable`, `AttachPicture`, `MarkPictureReady`/`MarkPictureFailed`, `Delete` (raises `PostDeletedDomainEvent`, closes F-059 for posts); errors in `PostErrors`; host from `ICurrentUser` in the endpoint (F-013 for posts, wire unchanged: the body field is ignored or removed only in 10.4); domain + handler unit tests; remove the Posts baseline entries.
  2. *Money:* `Money? EntranceFee` via `MapMoney` (new columns) + a migration that converts `EntrancePrice`/`Currency` with the legacy enum → ISO-4217 table (find the legacy enum in `pre-net10-baseline`), Down restores; the wire keeps `entrancePrice` + `currency` (map in the DTO).
  3. *Use cases:* `UpdatePost` (`posts.post.owner`; new `Post.Update`/`Retag` verbs), `DisablePost` (endpoint `HopautPolicies.Admin`, `Post.Disable` exists), `GetMyPosts(status)` – active = `Status == Active && end > now`, inactive = `Disabled || end <= now`, scoped to the caller (the legacy inactive query `p.UserId == userId && p.ActiveFlag == 0 || p.EndTime < now` returned every user's ended posts: record it as a finding and do not copy it); tags persisted (F-065): `Tag` is its own aggregate, so give it sequence-assigned IDs (`posts.tag_ids`, D-019 pattern incl. `setval` and identity restore in `Down`), normalise names (trim, ≤ 20 characters, ≤ 20 per post, duplicates ignored), `ITagRepository.GetOrCreateAsync(names)`, `PostTag` created by `Post` with the `TagId` only; new endpoints are additive (no handoff, D-007); `xmin` concurrency (`uint Version` + `IsRowVersion()`; check how `DbUpdateConcurrencyException` surfaces today and map it to 409).
  4. *Read model:* `IPostReadModel` (`AsNoTracking` projection to `PostDto`, tag names joined by `TagId`, then drop `PostTag.Tag`) for nearby (`RadiusMeters`, `PageSize` ≤ 100, keyset cursor on distance + id via `Cursor.Encode`/`Decode`) and by-id; reputations batched as today; the endpoint keeps returning the list until 10.4; integration tests for bounds and paging; coverage ≥ 70 % for Posts Domain + Application (`dotnet test tests/Modules/Posts/Hopaut.Modules.Posts.UnitTests --collect:"XPlat Code Coverage"`, read the cobertura line rate per assembly); remove the 8.2 TODO comments; then 8.2 ✅ and delete this file.
- **Half-done:** batches 1 and 2 are committed (see the ledger's 8.2 row); continue with batch 3.

## 3. Validation status

| Command | Result | Date |
|---------|--------|------|
| `dotnet build Bingo.sln -p:HopautStrictWarnings=true` | 0 errors, 0 compiler warnings (1 B-002 MSBuild notice) | 2026-11-17 |
| `dotnet build Bingo.sln -c Release -p:HopautStrictWarnings=true` | 51 assemblies build; 1 error: `No Six Labors license found` (B-002) | 2026-11-17 |
| `dotnet test Bingo.sln` (Podman helper from §6) | 401 passed / 0 failed / 0 skipped (243 building-block unit, 43 architecture, 41 Posts unit, 2 Media unit, 72 integration) | 2026-11-17 |
| `has-pending-model-changes` ×9 | no pending changes ×9 | 2026-11-17 |
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

| Posts | `20261117195201_PostStatusAndRequiredTitle` | Testcontainers only | In the migration: `ActiveFlag` renamed to `Status` (same values); blank titles → 'Untitled event', longer ones trimmed to 30 characters before `Title` becomes NOT NULL |
| Posts | `20261117195922_EntranceFeeMoney` | Testcontainers only | In the migration, both directions: legacy currency index → ISO code (index 8 → BGN, a price without currency → EUR), NaN/infinite prices dropped, amounts rounded and clamped; `PostsMigrationTests` runs it up and down |

Step 11.1 (legacy import) must map legacy `ActiveFlag`/`EntrancePrice`/`Currency` the same way.

## 5. Contract changes (backend ↔ mobile)

None for BingoMobile: it does not call the module endpoints yet (contract inventory in `TODO.md`, D-007), so every inventory row stays `unsynced` until its Phase 10 step. Module-endpoint behaviour the Phase 10 handoffs must list (besides the 7.7 items: 401 without a valid token, `X-Correlation-Id`, `/health/live` + `/health/ready`):
- 6.9 authorization (D-024): deleting a post needs its host or an admin; creating or deleting an announcement needs the post host or an admin (403 `announcements.announcement.not_post_host` on create, 404 on delete); accepting or rejecting an attendee needs the post host; deleting a rating needs its author or an admin (legacy: admins only); profile and picture updates need the owner or an admin; listing and resolving post reports need the Admin role.
- 8.1: `POST /api/v1/ratings` with a rate outside 1–5 answers 400 problem details `shared.rating_value.out_of_range` (was 500).
- 8.2: `POST /api/v1/posts` makes the caller the host (a body `userId` is ignored); every invalid field comes back in one 400 validation problem (`errors` keys such as `event.title`, `event.description`, `event.slots`, `location`, `endTime`, `event.entrancePrice`); schedule rules answer 422 (`posts.post.end_required`, `too_short` < 15 min, `too_long` ≥ 12 h, `starts_in_the_past` > 30 min); a price needs a currency index 0–24; `activeFlag` is now derived from the post status (same values).

## 6. Open decisions and blockers
- **B-002 Six Labors Community license key** (blocks Release builds, `dotnet publish`, the Docker image and the rest of 7.8) → unblock action: the user applies for the free Community license at https://licensing.sixlabors.com/ (Hopaut qualifies as a non-profit; an assembly-scoped key must cover `Hopaut.Modules.Media.Infrastructure`) and supplies it as the `SixLaborsLicenseKey` environment variable or the git-ignored repository-root `sixlabors.lic`. Never commit the key. Then run `dotnet build Bingo.sln -c Release -p:HopautStrictWarnings=true` and flip 7.8 ✅.
- **Running the Host locally requires `JwtSettings:Secret`** (≥ 32 characters) via `dotnet user-secrets` or `JwtSettings__Secret` (README.md).
- **Container runtime on this machine:** `podman machine ssh "sudo systemd-run --unit=hopaut-podman-tcp podman system service --time=0 tcp://127.0.0.1:2375"`, then `$env:DOCKER_HOST='tcp://127.0.0.1:2375'; $env:TESTCONTAINERS_RYUK_DISABLED='true'` in the same PowerShell command as `dotnet test`; stop with `podman machine ssh "sudo systemctl stop hopaut-podman-tcp"` (`tests/README.md`).
- **Final merge (D-015, Step 11.3):** only after 6.13, one `git merge --no-ff migrate/net10` into `master`, done locally; the user pushes.
- **D-011** xunit.v3 stays on 3.2.x (VSTest).

## 7. Prompt – next backend session

```text
Agent: HopautArchitect (workspace C:\Users\ghmi\personal\BingoAPI)
Resume from HANDOVER.md. Next step: 8.2 – Posts (plan § Phase 8 checklist and § Step 8.2). Next action: git status; load the clean-architecture-ddd, dotnet-10-migration and rest-api-standards skills, 8.2 is already 🟡 with batches 1–2 committed; work through HANDOVER.md §2 batches 3–4, committing after each green batch.
Do not redo completed work; verify the validation status in HANDOVER.md §3 first (start the Podman API helper from §6 before `dotnet test Bingo.sln`).
Then continue with Phase 8 (8.3 → 8.12). 7.8 only needs the Release build once the user supplies the Six Labors key (B-002).
Commit everything you change on migrate/net10. The user wants one big `git merge --no-ff migrate/net10` into master only at the very end (Step 11.3, D-015): never merge earlier, never push.
```

## 8. Prompt – BingoMobile session (mobile client work)

```text
Workspace: C:\Users\ghmi\personal\BingoMobile, branch dev.
There are no pending backend contract changes as of 2026-11-17: no file in C:\Users\ghmi\personal\BingoAPI\docs\mobile-handoffs\ has Status `pending`.
Do not change routes, models, repositories or error handling for backend reasons in this session, and do not change UI/UX.
For context only: the backend's module endpoints do not yet match lib/config/constants/api.dart (PascalCase, `Data` envelope, Unix seconds). They are synchronised module by module in backend Phase 10, each with a handoff file and an `applied <date> <commit>` status. Continue the mobile plan in mobile-migration-status.md instead (Phase 0 is blocked at Step 0.3-H, HERE SDK).
Commit locally on dev only. At the very end of the migration the user merges dev into master themselves (backend Step 11.3 / D-015); do not touch master.
```
