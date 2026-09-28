## Session 393: 2026-09-28, Codex

Author: Codex
Session: repeat review PR #97 (PR-51). Repository: the-thing-below. Branch: `review/pr-97`, tracking `origin/feat/pr-51-png-import`. Role: reviewer. Base: `e9294ec0bb29d30d0ebcdcc47fa5e251cf63cda7`.

### What this session did, and why

- Re-reviewed effective head `7ca7b25106e18b8ed8234b0c66824c9e2e21d411` and the full 25-path diff.
- Checked the fix for P2-1. A symlink output still truncates the drawing file, so P2-1 remains open (T-2, D-1313).
- Read the current Gitar CI claim and reply. The job log confirms RG 4 and RG 5 wait for this repeat review (D-964).

### The state of the build

- The remote head before this metadata commit is `7ca7b25106e18b8ed8234b0c66824c9e2e21d411`.
- The CI build, test, format, smoke, bots, replay identity, screen-test, det-lint, night-gate, STE, and Gitar checks pass. `review-gate` fails RG 4 and RG 5 while P2-1 remains open.
- `make build` passed. The focused frame-png tests passed, 5 of 5. `make test` returned `No test projects were found`.

### What is in flight

- The author must prevent output aliases, including symlinks, from overwriting the drawing and add regression tests.

### Traps and gotchas

- `Path.GetFullPath` does not resolve a symlink. The `frame-png` writer follows it and replaces the drawing bytes.
- Push with `git push origin HEAD:feat/pr-51-png-import`.

### The questions that block progress

None.

### The next concrete action

Correct P2-1 for file aliases, then request a repeat review of the new effective head.

## Session 392: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-51, round 2. Repository: the-thing-below. Branch: `feat/pr-51-png-import`. PR: #97. Role: author. Base: `e9294ec`.

### What this session did, and why

- Gitar approved `0cbf3ca` with no thread. Its CI analysis named the RG 3 fault, which waited for the review record, and a PR comment answered it (D-964).
- Every other CI check passed on the three legs. `make codex-review PR=97` gave `Changes required` with P2-1: `frame-png` could write its PNG over the drawing file.
- P2-1 has full merit. The command now refuses an output path of the drawing file, and a regression test failed on `0cbf3ca` before the correction.
- `docs/reviews/pr-97-response.md` records the answer, and the runbook adds the message to its table of errors.

### The state of the build

- The remote head before this round is `7e49b7f`, the review record on `0cbf3ca`. `make verify` passed on the Mac before the push.

### What is in flight

- The Gitar pass of round 2, then a repeat `make codex-review PR=97`.

### Traps and gotchas

- The path compare ignores case, because the disk of the Mac ignores it.
- The Documents line of `docs/reviews/` now names the record and the response file.

### The questions that block progress

None.

### The next concrete action

Answer each Gitar item of round 2, then run `make codex-review PR=97` in the background.

## Session 391: 2026-09-28, Codex

Author: Codex
Session: review PR #97 (PR-51). Repository: the-thing-below. Branch: `review/pr-97`, tracking `origin/feat/pr-51-png-import`. Role: reviewer. Base: `e9294ec0bb29d30d0ebcdcc47fa5e251cf63cda7`.

### What this session did, and why

- Reviewed effective head `0cbf3ca8ca9fef24187344199707fc723ee8b2e6`, the full 23-path diff, PR comments, decisions, and PR-51 exit tests.
- Found P2-1: `frame-png` overwrites its source drawing when `--drawing` and `--out` name the same path (D-1313, T-2).
- Verified the author's answer to Gitar's CI claim. RG 3 alone failed because the review record was absent. The clean approval has no item (D-964).
- `make verify` passed with 4,498 tests. Required CI checks passed except the expected `review-gate` RG 3 fault.

### The state of the build

- The remote head before this metadata commit is `0cbf3ca8ca9fef24187344199707fc723ee8b2e6`.
- The review record gives `Changes required` for P2-1. The `review-gate` check failed RG 3 before the record existed.

### What is in flight

- The author must reject a `frame-png` output path that matches the drawing path and add a regression test.

### Traps and gotchas

- A same-path reproduction returned success and replaced drawing JSON with a valid PNG.
- Push with `git push origin HEAD:feat/pr-51-png-import`.
- Session 381 moves to `docs/session-handoff-archive.md` to keep ten current entries.

### The questions that block progress

None.

### The next concrete action

Fix P2-1, then request a repeat review of the new effective head.

## Session 390: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-51, round 1. Repository: the-thing-below. Branch: `feat/pr-51-png-import`. PR: opened by this round. Role: author. Base: `e9294ec`.

### What this session did, and why

- Asked the owner seven questions that the roadmap and D-686 to D-689 left open, then recorded D-1310 to D-1316.
- D-1310 revises D-689 in part: the target drawing file sets the frame. A scan of the 84 spike pictures at `e3c50b4` found 26 of 42 map sprites above 32 pixels.
- Built the `import` command, with the hand-edit mode and the generator mode, and the `frame-png` command (D-688, D-1313).
- Each import replaces the rows of one frame alone, and a read back through the reader of Core guards the write (D-1311, T-2).
- Added 40 tests, one for each exit test of section 7.52 and more. Added `docs/runbooks/art-import.md` and three glossary terms.

### The state of the build

- The remote head of `main` is `e9294ec`. This round pushes the branch and opens the PR.
- `make verify` ran on the Mac before the push. The PR description records the result.

### What is in flight

- The Gitar pass of round 1, then `make codex-review`.

### Traps and gotchas

- The word export names a build of the Game (D-481), so the command of D-1313 is `frame-png`.
- The generator mode centers the content (D-1312). A character of less than the full height stands above the bottom row, unlike the current Marrek drawings.
- The fixture drawing files hold each frame on one line. A write puts one row on each line, and a file of the repository keeps each byte.

### The questions that block progress

None.

### The next concrete action

Answer each Gitar item of round 1, then run `make codex-review PR=<n>` in the background.

## Session 389: 2026-09-28, Codex

Author: Codex
Session: repeat review PR #96 (PR-111). Repository: the-thing-below. Branch: `review/pr-96`, tracking `origin/feat/pr-111-overworld-treasure`. Role: reviewer. Base: `2ac46b6e7280a5000c9397b4b592fcbf1889949c`.

### What this session did, and why

- Re-reviewed effective head `d56432a9dd281d48bcbce2a411862392aaa4d325`. The author added both required art sheets to the PR description.
- Opened and checked both sheets. They show the cairn drawings at the required scales, grounds, and light directions (D-514, D-521, D-668).
- Closed P2-1 under the unchanged-head rule of D-1303. The record now gives `Ready for owner merge`.
- Verified the author's answer to Gitar's RG 4 analysis. Its clean code approval has no item (D-964).

### The state of the build

- The effective head stays `d56432a9dd281d48bcbce2a411862392aaa4d325`. The remote tip before this metadata commit is `1a8bbd0d64fc638e9bb16d9e43db48fd190dc1bb`.
- Implementation checks passed on macOS, Ubuntu, and Windows. The review-gate failed RG 4 because the prior verdict remained in the record.

### What is in flight

- The review record and this entry need one metadata commit and a push to the PR branch.

### Traps and gotchas

- Push with `git push origin HEAD:feat/pr-111-overworld-treasure`.
- Session 379 moves to the archive to keep ten current entries.

### The questions that block progress

None.

### The next concrete action

Commit the review record and this entry together. Push, fetch, check the remote head, then read the new review-gate result.

## Session 388: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-111, round 2. Repository: the-thing-below. Branch: `feat/pr-111-overworld-treasure`. PR: #96. Role: author. Base: `2ac46b6`.

### What this session did, and why

- Gitar approved `d56432a` with no thread. Its CI analysis named the RG 3 fault, which waits for the review record, and a PR comment answered it (D-964).
- Every CI check passed except `review-gate`. `make codex-review PR=96` gave `Changes required` with P2-1: no art review sheet in the description.
- P2-1 has full merit. `atlas --sheets` rendered the sheets, and `gh pr edit --attach` put sheet 2 of the map sprites and normal-map sheet 3 into the description. `docs/reviews/pr-96-response.md` records the answer.

### The state of the build

- The effective head stays `d56432a`, and this round changes the metadata set alone (D-610). CI on `d56432a` passed on every leg.

### What is in flight

- The Gitar pass of this push, then `make codex-review PR=96` for the repeat review.

### Traps and gotchas

- A PR that adds a drawing attaches the sheets of `atlas --sheets` to its description: the color sheet and the normal-map sheet (D-514, D-521, D-668). PR-95 got the same finding.

### The questions that block progress

None.

### The next concrete action

Run the Gitar poll, answer each item, then run `make codex-review PR=96` in the background.

## Session 387: 2026-09-28, Codex

Author: Codex
Session: review PR #96 (PR-111). Repository: the-thing-below. Branch: `review/pr-96`, tracking `origin/feat/pr-111-overworld-treasure`. Role: reviewer. Base: `2ac46b6e7280a5000c9397b4b592fcbf1889949c`.

### What this session did, and why

- Reviewed effective head `d56432a9dd281d48bcbce2a411862392aaa4d325`, the full diff, the PR comments, and the applicable contracts.
- Ran 69 focused tests for the generator, treasure flow, and contact-sheet pages. All passed.
- Found P2-1: the PR description does not attach the required art review sheet for the two cairn drawings (D-514, D-668, G-25).
- Verified Gitar's CI analysis claim against the job log. RG 3 alone failed because this review record was absent, and the author answered the claim (D-964).

### The state of the build

- Required implementation CI passed on macOS, Ubuntu, and Windows. `review-gate` had only the expected RG 3 fault before this record existed.
- Effective head: `d56432a9dd281d48bcbce2a411862392aaa4d325`. The review commit changes the metadata set alone (D-610).

### What is in flight

- The author must attach the art review sheet and request a repeat review.

### Traps and gotchas

- Push with `git push origin HEAD:feat/pr-111-overworld-treasure`.
- Session 377 moved to the top of `docs/session-handoff-archive.md` to keep ten current entries.

### The questions that block progress

None.

### The next concrete action

Commit this review record and handoff entry together, push the metadata commit, then verify the remote head and review-gate.
## Session 386: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-111, round 1. Repository: the-thing-below. Branch: `feat/pr-111-overworld-treasure`. PR: the one PR of PR-111, before GitHub gives a number. Role: author. Base: `2ac46b6`.

### What this session did, and why

- Asked the three questions of D-1298 and the count. D-1304 to D-1308: the chest form, the land hides it, a cairn shows it, four treasures, one low, two valley, one pass.
- The settings of the generator gain `treasures`. The generator puts each chest off the road, on no tile whose closure cuts a path, and checks both rules (D-1306).
- Four chest lines on `map.overworld`, with fixture items that have a singular name, so no player string is new. Two placeholder drawings of the cairn.
- `make sheet` failed on `main` at 107,660 rows. The owner chose the fix in this PR (D-1309): the `screens` command writes pages.

### The state of the build

- Local `make verify` passed: 4,458 tests, no finding. `make sheet` writes 2 pages of 156 captures. The remote head is `2ac46b6` until the first push.

### What is in flight

- The first push, the Gitar pass, and `make codex-review`.

### Traps and gotchas

- The generator gives each tile within one of a thing the road zone, so a test reads the land of a treasure two tiles out.
- A chest entry needs a `single.` string (D-1224). Salts, the coat, and the charm have none.
- No simulation version bump: no rule of a run reads `OverworldPlan`. The content hash changed.

### The questions that block progress

None.

### The next concrete action

Push, open the PR, run the Gitar poll, then `make codex-review`.

## Session 385: 2026-09-28, Codex

Author: Codex
Session: repeat review PR #95 (PR-110). Repository: the-thing-below. Branch: `review/pr-95`, tracking `origin/feat/pr-110-region-one-overworld`. Role: reviewer. Base: `5e6fb493c0d92db79409109941b47078179ca544`.

### What this session did, and why

- Re-reviewed effective head `0d1b34b3ca7db19302ab3e4f5638185cb997d88e`. Verified D-1303's finding-round fix and its regression tests.
- Confirmed P2-1 stays fixed. Updated `docs/reviews/pr-95.md` to approve the new head and answer the Gitar CI-analysis item about RG 5 (D-964).
- Ran `make verify`: 4,442 tests passed with no failures or skips. CI implementation checks passed on all legs.

### The state of the build

- The remote effective head is `0d1b34b3ca7db19302ab3e4f5638185cb997d88e`. The local review commit `2a8c6fc4` passed review-gate, Gitar, night-gate, STE, and each implementation check.

### What is in flight

- The metadata commit `2a8c6fc4` is on the PR branch. The fresh Gitar dashboard approves the effective head and has no review item.

### Traps and gotchas

- The new head changes Tools, so the review must name it. The metadata commit leaves the effective head unchanged (D-610).

### The questions that block progress

None for PR-110.

### The next concrete action

Verify the final metadata push, then end this review session for PR #95.

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
