# Session handoff

## Session 169: 2026-09-21, Codex

Author: Codex
Session: repeat review PR-48, the enemy record correction. Repository: the-thing-below. Branch: `feat/pr-80-enemy-record`. PR: #48. Role: reviewer. Base: `86528a3`.

### What this session did, and why

- Reopened PR #48 after the author answered P1-1 at effective head `ef02f4a`.
- Read the response file, recomputed the effective head, inspected the full correction diff, and verified the original mismatch trigger and the waiting-enemy boundary.
- Ran `make verify`. It passed with 1587 tests and all local gates.
- Updated `docs/reviews/pr-48.md`: P1-1 is fixed, and the verdict is `Ready for owner merge` for `ef02f4a`.

### The state of the build

- The effective head is `ef02f4a`. The remote metadata tip is `d887605c`.
- GitHub CI and Gitar pass at `d887605c`. Review-gate waits for this updated review record.

### What is in flight

The repeat review record and this handoff entry need one metadata commit and push.

### Traps and gotchas

- The size check selects the largest enemy in the group, including waiting enemies, as D-788 requires.
- The review verdict targets `ef02f4a`, not the later metadata commits.

### The questions that block progress

None.

### The next concrete action

Commit and push the repeat review record and handoff entry. Then verify the remote head and review-gate result.

## Session 168: 2026-09-21, Claude Code

Author: Claude Code
Session: PR-80, the enemy record, the answer to the review. Repository: the-thing-below. Branch: `feat/pr-80-enemy-record`. PR: #48. Role: author. Base: `86528a3`.

### What this session did, and why

- Answered P1-1 of `docs/reviews/pr-48.md` with full merit: D-754 asks the enemy record for the size, and the load to fail a map that disagrees.
- Asked the owner which record of a group sets the size, and the size of each fixture enemy. The answers are D-788 and D-789.
- Commit `ef02f4a`: the `size` field of `EnemyRecord`, the size check in `BattleContent.RequireGroupsOf`, the records, the tests, and the documents.
- Wrote `docs/reviews/pr-48-response.md`.
- Gitar reviewed `f8cc578` at 15:50:12 UTC, after the push at 15:48:31 UTC: no issues, no review thread, 0 comments with merit. CI passes, and review-gate fails on RG 4 and RG 5 alone, which wait for the repeat review.

### The state of the build

- The effective head is `ef02f4a`. `make verify` passes with 1587 tests. The identity hashes stay, and the content hash changes.

### What is in flight

The repeat review by Codex of effective head `ef02f4a`.

### Traps and gotchas

- The test guard of `BattleRuns.Map` takes the size of its group. An elite guard holds an area of 3 by 2 tiles, because an area must leave room to move (D-209).
- The tests pair the checkout map with the test records, so the test brute stays elite, as in `content/`.

### The questions that block progress

None.

### The next concrete action

The owner starts a Codex session for the repeat review of PR #48 at `ef02f4a`. This author session answers each finding.

## Session 167: 2026-09-21, Codex

Author: Codex
Session: review PR-48, the enemy record. Repository: the-thing-below. Branch: `feat/pr-80-enemy-record`. PR: #48. Role: reviewer. Base: `86528a3`.

### What this session did, and why

- Reviewed PR #48 at effective head `fd97eb2`.
- Inspected the complete diff, the PR comments, the PR-80 roadmap scope and exit tests, the affected Core, content, test, identity, and document paths, and the D-754 contract.
- Found that the enemy record omits body size and that content loading does not compare map patrol size with the enemy record.
- Added `docs/reviews/pr-48.md` with finding P1-1 and the verdict `Changes required`.

### The state of the build

- The effective head is `fd97eb2`. The current branch tip is metadata commit `4385650`.
- `make verify` passes locally with 1583 tests. GitHub CI and Gitar pass at `2a8da97`, except `review-gate`, which waits for the review record.

### What is in flight

The review record and this handoff entry are pushed at `4385650`. The author must add the D-754 size field and the map-to-record consistency test and validation.

### Traps and gotchas

- `Patrol` already stores the map size, but `EnemyRecord` has no size member.
- A metadata commit does not move the effective head. The review targets `fd97eb2`, not `2a8da97`.

### The questions that block progress

None.

### The next concrete action

Commit and push the review record and handoff entry. Then the author answers P1-1 in a new round.

## Session 166: 2026-09-21, Claude Code

Author: Claude Code
Session: PR-80, the enemy record, the Gitar round. Repository: the-thing-below. Branch: `feat/pr-80-enemy-record`. PR: #48. Role: author. Base: `86528a3`.

### What this session did, and why

- Opened PR #48 at `fd97eb2`, and ran the push wait and the Gitar wait (D-586, D-705).
- The Gitar pass is current: the head is `fd97eb2`, and the dashboard edit at 14:55:50 UTC comes after the push at 14:52:59 UTC. The review found no issues, and the PR has no review thread.
- Answered the CI note of the dashboard in a PR comment: the review-gate check fails on RG 3 alone, because no review record exists yet.

### The state of the build

- The effective head is `fd97eb2`. This entry is a metadata commit, and it does not move the effective head (D-610).
- CI at `fd97eb2`: build, test, and format, smoke, det-lint, replay-identity, screen-test, and ste-check pass on every leg. The review-gate check waits for `docs/reviews/pr-48.md`.

### What is in flight

The Codex review of PR #48. The PR adds D-785 to D-787, so the `review-override` label does not apply (D-401).

### Traps and gotchas

- Session 165 holds the traps of the change: the ordinal order of the enemy files, and the check of a group entry against a record in another file.

### The questions that block progress

None.

### The next concrete action

The owner starts a Codex session to review PR #48 at effective head `fd97eb2`. This author session answers each finding of that review.

## Session 165: 2026-09-21, Claude Code

Author: Claude Code
Session: PR-80, the enemy record. Repository: the-thing-below. Branch: `feat/pr-80-enemy-record`. PR: the one PR of PR-80, with no GitHub number at this commit. Role: author. Base: `86528a3`.

### What this session did, and why

- Asked the owner three start questions, and recorded the answers as D-785 to D-787. No file held an ability id, so exit test 3 had no list to check.
- Added the ability file `content/rules/abilities.json` and one record file for each enemy under `content/rules/enemies/`. The two fixture enemies moved there with the same ids.
- Core: `AbilityList` and `EnemyRecord` read the files. `BattleContent` now holds the records and the ability file, and it checks each id between the files.
- The identity set gains the `enemy-record` run. The simulation version goes from 7 to 8.
- Tests: `EnemyRecordTests` holds exit tests 1 to 4, and the identity file holds exit test 5.
- Docs: the PR-80 entry, a PR-12 scope line, area-battle section 7.7, a design pass line, and two glossary rows.

### The state of the build

- The local head is the commit of this entry, on base `86528a3`. `make verify` passes with 1583 tests.
- At version 7 the old runs give their old hashes, so the move of the enemies changes no fight. The version bump alone moves `battle`, `replay`, and `state-hash`.

### What is in flight

The push, the PR, and the gitar pass. The PR changes decision rows, so it goes to Codex for review.

### Traps and gotchas

- The content set reads files in the ordinal order of the paths. A repeated enemy id thus fails on the file with the later path.
- A group entry and its enemy record now lie in two files. `BattleContent` checks the id, and the error names the battle fixture file.
- No screen changes, so the visual review of D-784 does not apply.

### The questions that block progress

None.

### The next concrete action

Push the branch, open the PR, and follow the `gitar-review` skill.

## Session 164: 2026-09-21, Codex

Author: Codex
Session: review PR-47, the ground draw order, walk captures, fullscreen launch, and frame fit. Repository: the-thing-below. Branch: `fix/pr-89-walk-texture`. PR: #47. Role: reviewer. Base: `ce06eda`.

### What this session did, and why

- Reviewed PR #47 at effective head `4c1bdec`.
- Inspected the complete diff, the PR description and comments, the PR-89 roadmap scope and exit tests, the affected Game and Tests paths, the capture baselines, and the changed documents.
- Verified the ground-layer order, deterministic walk capture sequence, fullscreen startup, 1080-row frame fit, baseline coverage, and the visual-review record.
- Added `docs/reviews/pr-47.md` with the verdict `Ready for owner merge`.

### The state of the build

- Remote head of the PR branch: `4c1bdec`.
- `make verify` passes with 1555 tests, no failures, and no skips.
- GitHub CI and Gitar pass at `4c1bdec`. The review-gate check waits for this review record.

### What is in flight

The review record and this handoff entry need a push. After the push, the owner can wait for review-gate and merge the PR.

### Traps and gotchas

- The review target is `4c1bdec`, not the metadata commit that publishes this record.
- The capture session stays windowed so it can set exact screen sizes. Only the play session enters borderless fullscreen.

### The questions that block progress

None.

### The next concrete action

Push the review record and handoff entry. Verify the remote head and the review-gate result.

## Session 163: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-89, round 4: the fill baselines at 1080 rows. Repository: the-thing-below. Branch: `fix/pr-89-walk-texture`. PR: #47. Role: author. Base: `ce06eda`.

### What this session did, and why

- At `1df0418`, every CI leg passed except `screen-test` and `review-gate`. Gitar reported "No issues found" on that head.
- The screen test failed on `map-fill-1080.png` and `ui-fill-1080.png` alone, as planned. The old baselines held the cut frame of the fit fault.
- Read both new captures of run 35602012534 (D-733, D-784). The map shows all 20 columns and the prompt row. The window frame of the ui fixture shows on all four edges.
- The other 40 captures of that run match their baselines byte for byte. Committed the two new baselines.

### The state of the build

- Remote head of `main`: `ce06eda`. Local: 1555 of 1555 tests pass.

### What is in flight

The push of this round, then the Gitar pass of the new head, and the Codex review, which adds `docs/reviews/pr-47.md`.

### Traps and gotchas

The traps of Sessions 160 to 162 stand.

### The questions that block progress

None.

### The next concrete action

Confirm the green screen test and the Gitar pass on the new head. Then tell the owner that PR #47 is ready for the Codex review.

## Session 162: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-89, round 3: the launch in borderless fullscreen. Repository: the-thing-below. Branch: `fix/pr-89-walk-texture`. PR: #47. Role: author. Base: `ce06eda`.

### What this session did, and why

- Round 2 went green at `6652b0d`: every CI leg, the screen test, and Gitar with "No issues found". Only `review-gate` waits for the review record.
- The owner asked that the game launch in borderless fullscreen, the development build included, with the chosen ratios of text, UI, and world. The owner put the change in this PR. The PR description holds the record of that choice.
- `Boot` sets the fullscreen mode of Godot for the play session, and it logs the mode at the start. A mode other than fullscreen is an error line (T-2).
- `Boot` builds the screen again when a size change moves the default body size (D-707), because on some systems the switch ends after the first frame.
- The CI captures showed a fault on `main`: at 1920 by 1080 in the fill mode, the frame drew at 2560 by 1440 and the window cut it, with no prompt row (D-573). The screen view kept the size of its texture. Both texture rects of `FrameRoot` now ignore the texture size.
- A project setting of fullscreen failed: Godot ignores `--windowed` when the project asks for fullscreen, so the capture session lost its window sizes. The project keeps the windowed default, and a test locks that.

### The state of the build

- Remote head of `main`: `ce06eda`. Local: 1555 of 1555 tests pass. Format, lint, and `make walk` pass.
- The play session of this Mac logs "the window opened in borderless fullscreen" at 1920 by 1080.

### What is in flight

The push of this round. The screen test fails on `map-fill-1080.png` and `ui-fill-1080.png` until their new baselines land from the artifact.

### Traps and gotchas

- The movie mode of Godot scales its frames to 1280 by 720, and its colors differ. It shows the layout of the fullscreen session, and not its pixels.
- `screencapture` has no screen permission in this session.

### The questions that block progress

None.

### The next concrete action

Read the two new fill-1080 captures of the artifact, commit them to `screens/baseline/`, and answer the Gitar pass.

## Session 161: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-89, round 2: the walk baselines and the Gitar answer. Repository: the-thing-below. Branch: `fix/pr-89-walk-texture`. PR: #47. Role: author. Base: `ce06eda`.

### What this session did, and why

- The first CI run of `f4eaabe` failed only where the plan said: 32 walk frames with no baseline, six `TheBaselineHoldsThisCapture` cases on each test leg, and RG 3 of `review-gate`. The 10 still captures matched their baselines.
- Downloaded the `screen-captures` artifact of run 35600042818. Read all 32 walk frames as one strip of the lead, and one full frame (D-733, D-784). The lead draws whole in each frame.
- Committed the 32 walk PNGs to `screens/baseline/`.
- Gitar approved `f4eaabe` with one finding: `ExpectedNames` claimed every file but held 6 of the 32 walk frames. Full merit. The list now holds all 42 names, so each walk baseline has its own test case.

### The state of the build

- Remote head of `main`: `ce06eda`. Local: 1553 of 1553 tests pass, and format passes.
- At `f4eaabe`, CI passed smoke, replay-identity, det-lint, and ste-check on every leg.

### What is in flight

The push of this round. After it: the screen-test job, the Gitar pass of the new head, and the Codex review, which adds `docs/reviews/pr-47.md`.

### Traps and gotchas

- The traps of Session 160 stand: `CLAUDE.md` is one byte under 16 KB, and this Mac cannot hold the 1080-row captures.
- In `ScreenCapturesTests`, `StillNames` must stay above `ExpectedNames`, for the same static order as `WalkSteps` in `ScreenCaptures`.

### The questions that block progress

None.

### The next concrete action

Wait for Gitar and CI on the new head, and answer each finding. Then tell the owner that PR #47 is ready for the Codex review.

## Session 160: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-89, the walk fault and the frames inside a step. Repository: the-thing-below. Branch: `fix/pr-89-walk-texture`. PR: the one PR of PR-89, opened in this session. Role: author. Base: `ce06eda`.

### What this session did, and why

- The owner saw a fault on a walk north or south in `make run`. The owner asked for a capture that a session can read by itself while the lead moves (D-782).
- Added the `walk` fixture: one frame at 1x after each tick of one step north and one step south. Added `--fixture <name>`, `make sheet FIXTURE=<name>`, and `make walk`. `make sheet` now builds the Godot solution first.
- The walk frames proved the cause (F-95). Each ground tile sorted at the center of its cell, so the floor cut the legs for half of each step north or south.
- The fix: the ground layer takes no part in the sort, and it draws at the Z index -1 (D-783). The smoke session reads both values back.
- The author read all 32 walk frames before and after the fix. Before: legs cut in `walk-north-09`, `walk-north-12`, and `walk-south-06`. After: the lead draws whole in every frame.
- Recorded D-782 to D-784, F-95, the PR-89 entry of the phase-2 file, and the PR gate line of the visual review (D-784).

### The state of the build

- Remote head of `main`: `ce06eda`. No Core file changes, so the simulation version stays (G-17).
- Local: build, format, lint, STE check, and smoke pass. 1495 of 1501 tests pass.
- The six failed tests are `TheBaselineHoldsThisCapture` for the walk frames. The baseline PNGs come from the artifact of the first screen-test run (D-733).

### What is in flight

The first push of the PR. The screen-test job fails until the 32 walk baselines land from its artifact.

### Traps and gotchas

- `CLAUDE.md` and `AGENTS.md` hold 16383 bytes, one byte under the 16 KB limit of SIZE 1. The next edit must make room first.
- The screen of this Mac gives 955 rows, so `make sheet` fails on the 1080-row capture. Use `make walk`, or a larger screen.
- In `ScreenCaptures`, `WalkSteps` must stay above `All`. A static property takes its value in the order of the file.

### The questions that block progress

None.

### The next concrete action

Download the `screen-captures` artifact, read each walk frame, and commit the 32 walk PNGs to `screens/baseline/`. Then answer the gitar pass.
