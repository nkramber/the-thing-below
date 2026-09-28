## Session 385: 2026-09-28, Codex

Author: Codex
Session: repeat review PR #95 (PR-110). Repository: the-thing-below. Branch: `review/pr-95`, tracking `origin/feat/pr-110-region-one-overworld`. Role: reviewer. Base: `5e6fb493c0d92db79409109941b47078179ca544`.

### What this session did, and why

- Re-reviewed effective head `0d1b34b3ca7db19302ab3e4f5638185cb997d88e`. Verified D-1303's finding-round fix and its regression tests.
- Confirmed P2-1 stays fixed. Updated `docs/reviews/pr-95.md` to approve the new head and answer the Gitar CI-analysis item about RG 5 (D-964).
- Ran `make verify`: 4,442 tests passed with no failures or skips. CI implementation checks passed on all legs.

### The state of the build

- The remote head is `0d1b34b3ca7db19302ab3e4f5638185cb997d88e`. The CI jobs pass except review-gate, which fails RG 5 against the old record.

### What is in flight

- Commit this review record and handoff entry together. Push with `git push origin HEAD:feat/pr-110-region-one-overworld`.

### Traps and gotchas

- The new head changes Tools, so the review must name it. The metadata commit leaves the effective head unchanged (D-610).

### The questions that block progress

None for PR-110.

### The next concrete action

Run `make where` and the STE check. Commit the review record and handoff entry, push, fetch, then verify the PR head and review-gate result.

## Session 384: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-110, round 4. Repository: the-thing-below. Branch: `feat/pr-110-region-one-overworld`. PR: #95. Role: author. Base: `5e6fb49`.

### What this session did, and why

- The second Codex review, session 383, found P2-1 fixed and gave `Ready for owner merge` for `1d66127`. The `codex-review` command then gave a fault: P2-1 was closed and listed the effective head, which a fix of the description alone cannot move.
- The owner chose to fix the tool in this PR (D-1303). `FindingRounds.CheckHeads` now accepts a closed finding at the effective head when the record before the round held it open there. The command reads that record before the review. Three tests of the finding rounds and one of the outcome are the regression tests.
- Recorded D-1303, which revises D-929 in part, and the exception in the `pr-review` skill.

### The state of the build

- The tests of the review tool pass. The full checks run before the push.

### What is in flight

- The Gitar pass and CI of this round, then `make codex-review PR=95`, which reviews the new Tools code.

### Traps and gotchas

- This round moves the effective head, because it changes Tools. Gitar and the Codex review read it again.

### The questions that block progress

None.

### The next concrete action

Run the Gitar poll, wait for CI, then run `make codex-review PR=95` in the background.

## Session 383: 2026-09-28, Codex

Author: Codex
Session: repeat review PR #95 (PR-110). Repository: the-thing-below. Branch: `review/pr-95`, tracking `origin/feat/pr-110-region-one-overworld`. Role: reviewer. Base: `5e6fb493c0d92db79409109941b47078179ca544`.

### What this session did, and why

- Re-reviewed effective head `1d6612758d84c020615d561b2f785389a901a945`. The new PR description attaches six art sheets for all 23 drawings, which closes P2-1 (D-514).
- Updated `docs/reviews/pr-95.md` and retained the earlier verdict and finding history.
- Read each drawing sheet. The art review evidence meets D-514, D-668, and G-25.

### The state of the build

- All implementation checks pass on metadata tip `67526933d7ccac94070b0962ae1fd8da5d2f2d9c`. The prior review-gate run failed RG 4 because the record still said `Changes required`.
- The remote head before this commit is `67526933d7ccac94070b0962ae1fd8da5d2f2d9c`.

### What is in flight

- Publish this review record and handoff as one metadata commit. The fresh review-gate result must read this verdict.

### Traps and gotchas

- The review branch is `review/pr-95`. Push with `git push origin HEAD:feat/pr-110-region-one-overworld`.
- The metadata commits leave the effective head at `1d6612758d84c020615d561b2f785389a901a945` (D-610).

### The questions that block progress

None for PR-110.

### The next concrete action

Push the metadata commit, fetch, and verify the PR head and review-gate result.

## Session 382: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-110, round 3. Repository: the-thing-below. Branch: `feat/pr-110-region-one-overworld`. PR: #95. Role: author. Base: `5e6fb49`.

### What this session did, and why

- Answered the Gitar pass of `1d66127`: the code review approved it, and the CI analysis claimed RG 3 alone, which the reply confirmed from the log. Two comments, none that needed a change.
- Answered P2-1 of the Codex review of session 381, full merit: the art review sheets of the 23 drawings are now in the PR description (D-514). `docs/reviews/pr-95-response.md` records it.

### The state of the build

- Every check of `1d66127` passes on each leg, except review-gate, which waits for the verdict.

### What is in flight

- The Gitar pass of this round, then `make codex-review PR=95` again.

### Traps and gotchas

- The commits of this round change the metadata set alone, so the effective head stays `1d66127` (D-610).

### The questions that block progress

None.

### The next concrete action

Run the Gitar poll, then run `make codex-review PR=95` in the background.

## Session 381: 2026-09-28, Codex

Author: Codex
Session: review PR #95 (PR-110). Repository: the-thing-below. Branch: `review/pr-95`, tracking `origin/feat/pr-110-region-one-overworld`. Role: reviewer. Base: `5e6fb493c0d92db79409109941b47078179ca544`.

### What this session did, and why

- Reviewed effective head `1d6612758d84c020615d561b2f785389a901a945` against the region-one overworld scope and contracts.
- Read every changed path, the PR comments, and the required screen-test artifact. The review found P2-1: the PR description lacks the required art review sheets for 23 drawings.
- Recorded `Changes required` in `docs/reviews/pr-95.md`. Gitar's CI-analysis claim about RG 3 matches the job log and has the author's answer (D-964).

### The state of the build

- Local `make verify` passed with 4,438 tests and no skips. The implementation checks pass in CI on the effective head.
- The remote head before this metadata commit is `1d6612758d84c020615d561b2f785389a901a945`. `review-gate` failed RG 3 before the record existed, and this record leaves RG 4 red until P2-1 closes.

### What is in flight

- The author must attach the required art review sheets and request a repeat review (D-582).

### Traps and gotchas

- The review branch is `review/pr-95`. Push with `git push origin HEAD:feat/pr-110-region-one-overworld`.
- The current review record applies to effective head `1d6612758d84c020615d561b2f785389a901a945`.

### The questions that block progress

None for PR-110.

### The next concrete action

Attach the review sheets for the 23 drawings to the PR description, list each drawing and the commit shown, then request a repeat review.
## Session 380: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-110, round 2. Repository: the-thing-below. Branch: `feat/pr-110-region-one-overworld`. PR: #95. Role: author. Base: `5e6fb49`.

### What this session did, and why

- Read the Gitar pass of `5ab1c85`: the code review approved it with no finding. Its CI analysis names RG 3 of `review-gate`, which waits for the record of the cross-provider review (T-4).
- Took the six new baselines from the screen-test artifact of run 36377274609 (D-733): the three `region-one` frames, and the three `overworld` frames that the frosted grass changes. The author read each frame.

### The state of the build

- On each CI leg, the three absent baselines were the only failed tests, so each leg makes the same map as the committed one (D-1296). The replay identity passed on each leg.

### What is in flight

- The Gitar pass of this round, then `make codex-review PR=95`.

### Traps and gotchas

- The baselines of this round move the effective head, so the Gitar pass runs again.

### The questions that block progress

None.

### The next concrete action

Answer each Gitar item of this round, then run `make codex-review PR=95` in the background.

## Session 379: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-110, round 1. Repository: the-thing-below. Branch: `feat/pr-110-region-one-overworld`. PR: opened by this round. Role: author. Base: `5e6fb49`.

### What this session did, and why

- Asked the owner each question of the layout, the terrain, the zones, the gates, and the art. D-1270 to D-1302 record the answers. D-1289 corrects a premise of the session in D-1286.
- Built the one overworld of the game with region one at 160 by 128 (D-1274, D-1297), from a generator in Tools with a settings file (D-1294 to D-1296).
- Added the tile kinds road, snowfield, gorge, snow peak, and bridge, the frosted grass, and the thing kind `mark` (D-1271, D-1277, D-1278, D-1302). The simulation version is 39.
- Gave each zone its region, which names its group file and its pool (D-1285, D-1289). The fixture maps moved to `region.fixture` in the transition table.
- Filed PR-111, the treasure of the overworld, right after PR-110 (D-1298).

### The state of the build

- 4,434 tests pass locally. The three new screen baselines of `region-one` come from CI (D-733), and the fixture overworld baselines change with the frosted grass.
- The owner approved the terrain, the marks, the landmarks, and the map at each review stop of D-1287.

### What is in flight

- The first push, the Gitar pass, and the new baselines from the screen-test artifact.

### Traps and gotchas

- Never edit the rows of `content/rules/maps/overworld.json` by hand. Change `content/worldgen/overworld.json`, run `make overworld`, and read the map. A test fails a hand edit.
- The generator uses integer math and PCG streams alone, so each CI leg makes the same map.
- The content set reads the settings file only when it is present. The generator test reads the checkout file itself.
- Python stays out of the repository (D-406). The draft scripts of this session lived in the scratchpad alone.

### The questions that block progress

None.

### The next concrete action

Push, run the Gitar poll, then commit the CI baselines of the screen-test artifact after reading each frame.

## Session 378: 2026-09-28, Codex

Author: Codex
Session: review PR #94 (PR-109). Repository: the-thing-below. Branch: `review/pr-94`, which tracks `origin/feat/pr-109-overworld-encounters`. Role: reviewer. Base: `db0320b3`.

### What this session did, and why

- Reviewed effective head `1819fab9` against the PR-109 scope and contracts. Found no defect and recorded `Ready for owner merge` in `docs/reviews/pr-94.md` (T-4, D-17).
- Verified the Gitar CI-analysis claim against run 36364168926. RG 3 alone failed because the review record was not yet present (D-964).
- Corrected the `docs/reviews/` Documents row in the PR description (D-577).

### The state of the build

- `make verify` passed with 4,369 tests, no failures, and no skips. CI implementation checks passed at `1819fab9` on all three systems.
- The remote head before this metadata commit is `1819fab9`. The Gitar pass is current; its CI-analysis claim has the author's answer.

### What is in flight

- Commit the review record and this handoff entry together. Push with `git push origin HEAD:feat/pr-109-overworld-encounters`, then verify the remote head and review-gate.

### Traps and gotchas

- The review record names the effective head, `1819fab9`, and the metadata commit does not change it.
- OQ-254 tracks the greedy wipe loop and blocks no PR. PR-90 owns its resolution (D-1260).

### The questions that block progress

None for PR-109.

### The next concrete action

Run `make where`, `make ste-check`, and `git diff --cached --check`. Commit the review record and handoff files together, push to the PR branch, then verify the remote head and checks.

## Session 377: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-109, round 1. Repository: the-thing-below. Branch: `feat/pr-109-overworld-encounters`. PR: opened by this round. Role: author. Base: `db0320b`.

### What this session did, and why

- Asked the owner the questions of the step counter, the zones, and the snapshot. D-1261 to D-1269 record the answers.
- Built the invisible encounters of the overworld: the danger count, the encounter stream, the zone grid, and the zone list (D-1249, D-1261 to D-1266). Save format 20 holds the count, and the simulation version is 38.
- Gave the fixture overworld its four zones (D-1267).
- Fixed a fault of PR-64 in the same method: a step onto the encounter trap of the fixture dungeon stopped Game (D-1268).

### The state of the build

- `make verify` passed locally with 4,369 tests. The remote head is this round.
- 2,000 greedy runs: 1,785 reach the goal, and the longest takes 4,555 ticks, so the budget of D-1259 holds. 215 runs play the whole budget (OQ-254).

### What is in flight

- The Gitar pass of this round, then `make codex-review`.

### Traps and gotchas

- Each map file holds `zones` and `zone_grid`. A hub or a dungeon holds both empty.
- The readers of save formats 16 to 19 now take the seed of the header, because each one opens the encounter stream.
- `SnapshotLines.AsFormatNineteen` drops the count and the encounter stream for a test of an older reader.

### The questions that block progress

None.

### The next concrete action

Answer each Gitar item of this round, then run `make codex-review PR=<n>` in the background.

## Session 376: 2026-09-27, Codex

Author: Codex
Session: review PR #93 (PR-35). Repository: the-thing-below. Branch: `review/pr-93`, tracking `origin/feat/pr-35-region-map`. Role: reviewer. Base: `9d1d73f`.

### What this session did, and why

- Reviewed the full diff at effective head `5f2ba3b4c9d7c29ad49666a6b6fa23ba4f26d17f` against the PR-35 scope, exit tests, and contracts (D-1242 to D-1260).
- Found no defect. Recorded `Ready for owner merge` in `docs/reviews/pr-93.md`.
- Read the changed screen-test frames. No visual fault was found.
- Corrected the Documents row of the PR description to name the review record (D-577).

### The state of the build

- `make verify` passed locally with 4,318 tests and no skips. CI run 36359846898 passed every implementation check on all three systems.
- The implementation head is `5f2ba3b4c9d7c29ad49666a6b6fa23ba4f26d17f`. Metadata head `ed09141` passed review-gate, night-gate, changed-paths, STE, and Gitar. Its implementation matrix jobs skipped.

### What is in flight

- Commit this record and handoff update together. Push to `feat/pr-35-region-map`, then verify the remote head and checks.

### Traps and gotchas

- The Gitar CI claim reports the missing review record. The job log confirms RG 3 is its only fault, and the author answered it (D-964).
- OQ-254 holds the greedy bot wipe loop and blocks no work in PR-35 (D-1260).

### The questions that block progress

None for PR-35.

### The next concrete action

Run `make where`, `make ste-check`, and `git diff --cached --check`. Commit the review record and handoff files, push, then verify the remote head and checks.
