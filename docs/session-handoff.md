# Session handoff

## Session 196: 2026-09-22, Claude Code

Author: Claude Code
Session: author PR-93, round 2. Repository: the-thing-below. Branch: `chore/pr-93-docs-only-ci`. PR: #54. Role: author. Base: `d1a03f7`.

### What this session did, and why

- Answered the two Gitar items of round 1.
- The RG 7 fault: the `docs/reviews/` row of the description named no path. The edit of the description fixed it, and review-gate now fails on RG 3 alone.
- The finding: a plain job that its condition skips reports the conclusion `skipped`, not `success`. Thus after one docs-only push, the rule of D-858 read `coverage report`, `det-lint`, and `screen-test` as no pass, and the next docs-only push ran every job. Full merit.
- The fix: a gate job with `always()` for each of the three jobs, as the matrix families have (D-682). Each gate takes the required name, and each job takes the name `<name> (run)`. D-858 stays true as written.
- The regression test `EachJobThatADocsOnlyChangeSkipsHasOneGateJob` fails on the workflow of `d0ca1bc` and passes now. `area-ci.md` section 7.19 lists the three new gates.

### The state of the build

- `make verify` passes with 1,924 tests outside the Smoke category, 0 ste-check findings, and the smoke session.
- The CI run of `d0ca1bc` passed every job on every leg. The remote head is the push of this entry.

### What is in flight

- The Gitar pass of this push, then the review of the other provider (D-401, D-560).

### Traps and gotchas

- Branch protection requires `coverage report` and `det-lint` by name. The gate jobs keep those names, so the protection needs no change. The old job names move to `<name> (run)`.
- A fault in `changed-paths` skips each plain job. Each new gate now fails in that case, where the old check showed `skipped` (T-2).
- Session 195 said that a skip on a skip holds. That was wrong for the three plain jobs before this round.

### The questions that block progress

None.

### The next concrete action

Wait for Gitar on the new head, and answer each comment. Then tell the owner that PR #54 is ready for the other provider.

## Session 195: 2026-09-22, Claude Code

Author: Claude Code
Session: author PR-93, round 1. Repository: the-thing-below. Branch: `chore/pr-93-docs-only-ci`. PR: the PR-93 intent, and GitHub gives the number at the open. Role: author. Base: `d1a03f7`.

### What this session did, and why

- Asked OQ-219 and two more questions that the work found. D-857, D-858, and D-859 hold the answers.
- D-857: `CLAUDE.md` and `AGENTS.md` join the skip set. `.claude/settings.json` was in the set already through `.claude/`.
- Added rules AGENTS 1 and AGENTS 2 to ste-check. AGENTS 1 reads the rule of D-20. AGENTS 2 reads the count of the Smoke filter option that `TestFilterTests` reads. A docs-only PR skips that test, so ste-check reads the same facts on every PR.
- D-858: a docs-only push skips the other jobs when each check that it skips passed on the previous head. review-gate and ste-check do not count.
- D-859: the new `changed-paths` command of Tools decides. The workflow collects the paths and the check runs of the `before` commit, and the job gets `checks: read`.
- Updated the `ste-writing` and `one-pr-one-session` skills, `area-ci.md` section 7.1, and section 7.15 of the phase 2 file.

### The state of the build

- `make verify` passes with 1,920 tests outside the Smoke category, 0 ste-check findings, and the smoke session.
- A local run of the two workflow steps against PR #53 gave the expected answer in four cases: a green head, a cancelled head, a force push, and a push to `main`.
- The remote head is the push of this entry.

### What is in flight

- The Gitar pass, then the review of the other provider. The PR changes `.github/workflows/` and adds decision rows, so no label applies (D-401, D-560).

### Traps and gotchas

- The run of a new push cancels the run of the previous head. A quick docs push after a code push thus runs every job, because the checks of the previous head show `cancelled`.
- The first CI run of this PR runs every job, because the PR changes code.
- Branch protection does not require `screen-test`, but the skip rule reads it. The rule is safe, because an absent run fails it.
- The shell step reads `github.event.before`. That field exists only on the `synchronize` action.

### The questions that block progress

None.

### The next concrete action

Open the PR. Load the `gitar-review` skill, wait for Gitar, and answer each comment. Then tell the owner that the PR is ready for the other provider.

## Session 194: 2026-09-21, Codex

Author: Codex
Session: reviewer PR-53, repeat review. Repository: the-thing-below. Branch: `feat/pr-56-light-and-shadows`. PR: #53. Role: reviewer. Base: `e0cc485`.

### What this session did, and why

- Re-reviewed the complete PR-53 diff through effective head `30868a8`.
- Verified the wall-face, figure-shadow, paired-light, budget, baseline, and Deck-sweep corrections against their original triggers and boundary tests.
- Updated `docs/reviews/pr-53.md` with the current verdict `Ready for owner merge`.

### The state of the build

- `make verify` passes with 1,862 non-smoke tests and all local gates. The remote effective head is `30868a8`.

### What is in flight

- The updated review record, this handoff entry, and the archive move need commit and push.

### Traps and gotchas

- Review-gate remains red until this repeat-review record reaches the PR head.

### The questions that block progress

None.

### The next concrete action

Commit the repeat review and both handoff files. Push, fetch, and verify the remote head and review-gate check.

## Session 193: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-56, round 7. Repository: the-thing-below. Branch: `feat/pr-56-light-and-shadows`. PR: #53. Role: author. Base: `e0cc485`.

### What this session did, and why

- The owner built the Deck test on the Deck and pushed the reports to `spike/deck-test` (commit `82bd091`). The branch also gained `build-on-deck.sh` and a Linux path in the template script.
- Each stage held 60 frames per second under both renderers. Under Mobile, `pairs-24` gave 4.17 ms and `full-load-24` 4.55 ms at the 95th percentile, with no frame over 16.667 ms. F-96 records it, and the light row of 24 stands (D-854, G-14).
- The owner asked whether the Deck test matches the game. It does not: it is the stress scene of the renderer pick, and it lights four times the pixels of the world of the game. The session offered a measure mode inside the game, and the owner has not answered.

### The state of the build

- Docs alone change this round. The remote head is this round.

### What is in flight

- The repeat review of Codex on the code head `f4b071b`.

### Traps and gotchas

- A path in backticks of another branch fails the reference check.
- The owner pushes from the Deck with SSH through `gh auth login -p ssh`.

### The questions that block progress

None for PR-56. The measure mode inside the game waits for an owner answer.

### The next concrete action

Wait for Gitar on this head, then tell the owner that PR #53 is ready for the repeat review.

## Session 192: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-56, round 6. Repository: the-thing-below. Branch: `feat/pr-56-light-and-shadows`. PR: #53. Role: author. Base: `e0cc485`.

### What this session did, and why

- Gitar reviewed `45e8464` with the verdict "Approved with suggestions" and one finding: the doc comment of `FeetShadow` named its six-point polygon an octagon. The finding holds, and this round names it a hexagon.
- Each CI job of `45e8464` passed except review-gate, which waits for the repeat review.

### The state of the build

- The change is one word of a comment. The remote head is this round.

### What is in flight

- The repeat review of Codex, and the Deck test of D-854.

### Traps and gotchas

None new.

### The questions that block progress

The Deck result of D-854.

### The next concrete action

Reply on the Gitar thread with this commit, prove that the review of this head is current, and tell the owner that PR #53 is ready for the repeat review.

## Session 191: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-56, round 5. Repository: the-thing-below. Branch: `feat/pr-56-light-and-shadows`. PR: #53. Role: author. Base: `e0cc485`.

### What this session did, and why

- CI run 35668891800 of `d31b896` passed each job except screen-test and review-gate. The screen-test job failed on 43 lit captures, because the wall faces and the figure shadows change each frame.
- The author read the map, walk, and battle frames of its `screen-captures` artifact. They match the local sheet, and this round commits them as the baseline (D-733).

### The state of the build

- `make verify` and `make smoke` pass. The remote head is this round.

### What is in flight

- Gitar on this head, and the repeat review of Codex. The review of `6095f70` is stale.
- The owner runs the Deck test of D-854.

### Traps and gotchas

- The review-gate check fails until the repeat review names the effective head.

### The questions that block progress

The Deck result of D-854.

### The next concrete action

Prove that the Gitar review of this head is current. Then tell the owner that PR #53 is ready for the repeat review.

## Session 190: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-56, round 4. Repository: the-thing-below. Branch: `feat/pr-56-light-and-shadows`. PR: #53. Role: author. Base: `e0cc485`.

### What this session did, and why

- The owner read the lit map and asked for lit walls with no light behind a wall of one tile, for figures that cast shadows, and for 50% more light. D-852 to D-855 record the answers.
- Core gives the shape of each wall from the terrain: a south face of 24 pixels, other faces of 8, a strip of 2 in a wall of one tile, and a full tile at a corner.
- Each source is a pair of Godot lights, and each figure blocks light at its feet. The light row is 24, and each source counts two.
- The Deck test on `spike/deck-test` (commit `3f9fdda`) gains the stages `pairs-4` to `pairs-24` and `full-load-24`. A run on the Mac proved that each stage runs.
- The owner asked for CI to skip docs-only changes. D-856 and PR-93 hold it, and OQ-219 blocks it.

### The state of the build

- `make verify` passes with 1,862 tests, and `make smoke` passes. The remote head is this round.

### What is in flight

- The owner runs the Deck test. PR-56 merges only when `pairs-24` and `full-load-24` hold 60 frames per second under Mobile (D-854, G-14).
- The Codex review of `6095f70` gave `Ready for owner merge`, and this round moves the effective head, so the review repeats.
- The screen-test job fails on this push, because the walls and the shadows change each lit frame. The next round commits the new baseline.

### Traps and gotchas

- One Godot light cannot light a figure and keep the shadow of that figure off it. The pair and the light masks of `WorldLights` solve it.
- macOS has no `timeout` command.

### The questions that block progress

The Deck result of D-854.

### The next concrete action

Commit the baseline from the `screen-captures` artifact, then answer Gitar.

## Session 189: 2026-09-21, Codex

Author: Codex
Session: reviewer PR-53, round 1. Repository: the-thing-below. Branch: `feat/pr-56-light-and-shadows`. PR: #53. Role: reviewer. Base: `e0cc485`.

### What this session did, and why

- Reviewed the complete PR-53 diff from merge base `e0cc485` through effective head `6095f70`.
- Verified the opposite-provider gate, light content and budget contracts, normal-map atlas wiring, map and battle lighting, documents, tests, CI results, and changed screen baselines.
- Added `docs/reviews/pr-53.md` with the verdict `Ready for owner merge`.

### The state of the build

- `make verify` passes with 1,856 non-smoke tests and all local gates. The remote implementation head is `6095f70`.

### What is in flight

- The review record and this handoff entry need commit and push.

### Traps and gotchas

- The review-gate check remains red until the metadata commit reaches the PR head.

### The questions that block progress

None.

### The next concrete action

Commit the review record and both handoff files. Push, fetch, and verify the remote head and the review-gate check.

## Session 188: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-56, round 3. Repository: the-thing-below. Branch: `feat/pr-56-light-and-shadows`. PR: #53. Role: author. Base: `e0cc485`.

### What this session did, and why

- Gitar approved the code of `96298b7` with no thread. Its CI note named a real failure: `CaptureColorsTests` read each pixel of the lit map and walk baselines as a palette color.
- Light blends to any color (D-181), so a lit capture holds colors outside the palette. The test now reads the unlit world capture, `picture-1x.png`, alone. That capture still guards the sRGB conversion of PR-55 and the Nearest filter.
- The old test fails on the new baseline, which proves the change.

### The state of the build

- 1,856 tests pass locally against the new baseline. The remote head is this round.

### What is in flight

- The push wait of Gitar on this head, then the Codex review of `docs/reviews/pr-53.md`.

### Traps and gotchas

- Run the tests after a new baseline lands. `make verify` read the old baseline and passed.

### The questions that block progress

None.

### The next concrete action

Prove that the Gitar review of this head is current, then tell the owner that PR #53 is ready for the Codex review.

## Session 187: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-56, round 2. Repository: the-thing-below. Branch: `feat/pr-56-light-and-shadows`. PR: #53. Role: author. Base: `e0cc485`.

### What this session did, and why

- The first CI run passed each build, smoke, det-lint, identity, and STE job on every leg. The screen-test job failed on 43 captures, because the light changes every map, walk, and battle frame.
- The author read the map, walk, and battle frames of the `screen-captures` artifact of run 35664815392. They match the local sheet, and this round commits them as the baseline (D-733).
- The UI and picture captures match the old baseline, because they take no scene light (D-210).

### The state of the build

- `make verify` and `make smoke` pass. The remote head is this round.

### What is in flight

- Gitar reviews this head, and the Codex review of `docs/reviews/pr-53.md` follows.

### Traps and gotchas

- The `review-gate` check fails until the review record lands.

### The questions that block progress

None.

### The next concrete action

Wait for Gitar with the push wait, prove that the review is current, and answer each finding.
