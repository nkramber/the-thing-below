# Session handoff

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

## Session 159: 2026-09-21, Codex

Author: Codex
Session: review PR-9, the battle core and the timeline. Repository: the-thing-below. Branch: `feat/pr-9-battle-core`. PR: #46. Role: reviewer. Base: `c6cc71c`.

### What this session did, and why

- Reviewed PR #46 at effective head `c480baa`.
- Inspected the complete 78-path diff, the PR comments, the PR-9 roadmap scope and exit tests, and the affected Core, Game, debug, content, save, replay, test, and document paths.
- Verified the timeline, action, row, wave, wipe, event queue, map freeze, migration, content, and replay contracts.
- Added `docs/reviews/pr-46.md` with the verdict `Ready for owner merge`.

### The state of the build

- `make verify` passes with 1485 tests and all local gates.
- GitHub CI and Gitar pass at `c480baa`. The `review-gate` check fails only because the review record was absent before this session.
- The effective implementation head is `c480baa`.
- The remote head of `main` is `c6cc71c`.

### What is in flight

PR #46 waits for the final Gitar and review-gate results after the metadata push. The review verdict remains for effective head `c480baa`.

### Traps and gotchas

- The review target is `c480baa`. The review commit changes only the review record and this handoff entry.
- The smoke battle wipes and exercises the reload path. The tests also cover a successful flee and the wait intent.

### The questions that block progress

None.

### The next concrete action

Verify the final Gitar and review-gate results at metadata tip `d837238`. The owner merges after the checks pass.

## Session 158: 2026-09-21, Claude Code

Author: Claude Code
Session: PR-9, the battle core and the timeline. Repository: the-thing-below. Branch: `feat/pr-9-battle-core`. PR: #46. Role: author. Base: `c6cc71c`.

### What this session did, and why

- Asked the five open questions and each gap that the code showed. D-755 to D-781 hold the answers.
- Built the battle in Core: the timeline, five actions, the rows, the wave, the wipe, and the wait intent.
- Added content files, record format 2, save format 4, simulation version 7, and a battle identity run.
- Replaced the `flee` command with five battle commands. Game drains the event queue, and a wipe reloads.
- Found and fixed a snapshot that shared the pack array. `ASnapshotKeepsTheCountOfThePackOfItsTick` guards it.
- Measured the fixture fights over 1000 seeds, and D-781 corrected the elite.

### The state of the build

- `make verify` passes locally with 1485 tests, the smoke battle included.
- The remote head is the push of this entry. CI and gitar have not run yet.

### What is in flight

The first push, the PR, and the gitar pass. The Codex review follows.

### Traps and gotchas

- A snapshot must copy each list of the live state. A shared list changes the start of a record.
- The smoke battle wipes on the fixture seed, so it runs the reload path. A test covers a flee.
- `TestBattles` holds its own rules and groups. A change in `content/` moves no test.
- The owner asked for a separate PR after PR-9: a texture fault on a walk north or south, and capture frames inside a step.

### The questions that block progress

None.

### The next concrete action

Push, open PR #46, and follow the `gitar-review` skill.

## Session 157: 2026-09-21, Codex

Author: Codex
Session: repeat review PR-8, the enemies on the map. Repository: the-thing-below. Branch: `feat/pr-8-map-enemies`. PR: #45. Role: reviewer. Base: `626d2fe`.

### What this session did, and why

- Reopened PR #45 at effective head `801d6aa` after the author corrected P2-1 from the review of `e83e2d6`.
- Verified the original overflow trigger, the subtraction-based correction, the three-case regression theory, and the full affected consumer path.
- Updated `docs/reviews/pr-45.md` with the verdict `Ready for owner merge`.

### The state of the build

- `make verify` passes locally with 1,393 tests and all local gates.
- CI passes the build, test, format, smoke, replay identity, screen-test, det-lint, STE, coverage, changed-paths, and Gitar checks at PR tip `06ad10b`.
- The effective implementation head is `801d6aa`.
- The remote head of `main` is `626d2fe`.

### What is in flight

The review record and this handoff entry need one metadata commit and push.

### Traps and gotchas

- The effective head is `801d6aa`. The tip `06ad10b` changes only review metadata.
- The area edge check depends on the reader refusing negative coordinates and dimensions.

### The questions that block progress

None.

### The next concrete action

Commit and push the repeat review record and this handoff entry. Then verify the remote head and review-gate result.

## Session 156: 2026-09-21, Claude Code

Author: Claude Code
Session: PR-8, the enemies on the map, the answer to the review. Repository: the-thing-below. Branch: `feat/pr-8-map-enemies`. PR: #45. Role: author. Base: `626d2fe`.

### What this session did, and why

- Answered `docs/reviews/pr-45.md`, which gave `Changes required` at `e83e2d6` for one finding.
- P2-1 has full merit. An area whose coordinate and side sum past the range of an `int` loaded with no error. Commit `801d6aa` compares each side with the room that the map leaves, and a theory of three cases holds the regression.
- `docs/reviews/pr-45-response.md` records the disposition and the evidence.

### The state of the build

- `make verify` passes with 1393 tests. The identity file and the content hash stay the same, and the simulation version stays at 6.
- The effective head is `801d6aa`.
- The remote head of `main` is `626d2fe`.

### What is in flight

The push of this round runs CI and the gitar pass on `801d6aa`. PR #45 then waits for a repeat review of that head.

### Traps and gotchas

- The reader of an area refuses a value below zero. The edge check relies on that, so a change of that reader needs a new look at the check.

### The questions that block progress

None.

### The next concrete action

Wait for CI and the gitar pass on `801d6aa`, answer each gitar comment, and hand PR #45 to the repeat review.

## Session 155: 2026-09-21, Codex

Author: Codex
Session: review PR-8, the enemies on the map. Repository: the-thing-below. Branch: `feat/pr-8-map-enemies`. PR: #45. Role: reviewer. Base: `626d2fe`.

### What this session did, and why

- Reviewed PR #45 at effective head `e83e2d6` after the author metadata tip `111fbe1`.
- Inspected the complete diff, the PR comments, the PR-8 contracts and exit tests, and the affected Core, Debug, Game, content, test, identity, baseline, and document paths.
- Found P2-1 in `PatrolLayout.CheckArea`: unchecked area-bound arithmetic can bypass the required map-boundary and body-fit checks for malformed oversized content.

### The state of the build

- `make verify` passes locally with 1,390 tests and all local gates.
- CI passes the build, test, format, smoke, replay identity, screen-test, det-lint, STE, coverage, changed-paths, and Gitar checks at PR tip `111fbe1`.
- The review-gate check fails only because the review record was absent before this session.
- The remote head of `main` is `626d2fe`.

### What is in flight

PR #45 needs a correction for P2-1 and a repeat cross-provider review at the new effective head.

### Traps and gotchas

- The effective implementation head is `e83e2d6`, not the metadata tip `111fbe1`.
- The area layout check must protect every positive coordinate and dimension from `int` overflow.

### The questions that block progress

None.

### The next concrete action

Correct P2-1 with a regression test, then request a repeat review of the new effective head.

## Session 154: 2026-09-21, Claude Code

Author: Claude Code
Session: PR-8, the enemies on the map, round 3. Repository: the-thing-below. Branch: `feat/pr-8-map-enemies`. PR: #45. Role: author. Base: `626d2fe`.

### What this session did, and why

- Recorded the gitar pass and the CI result of the effective head `e83e2d6`. This commit changes the metadata set alone, so the effective head stays `e83e2d6` (D-610).

### The state of the build

- CI passes on every leg at `e83e2d6`: the build, test, and format job, the smoke job, and the replay identity job on each of the three legs. The screen-test, det-lint, STE, coverage, and changed-paths jobs pass too.
- The `screen-test` job passes on the new map baseline.
- The gitar pass of `e83e2d6` is current. The dashboard edit at 02:57:40 UTC comes after the push at 02:55:25 UTC. Its code review found no issue and opened no thread. Its CI note describes the first run, which the new baseline answered.
- The `review-gate` check gives RG 3 alone, because the head holds no `docs/reviews/pr-45.md`. RG 1, RG 2, and RG 6 to RG 8 pass.
- The remote head of `main` is `626d2fe`.

### What is in flight

PR #45 waits for the cross-provider review at the effective head `e83e2d6`. The PR changes code, so the label of D-401 never applies.

### Traps and gotchas

- The review reads the 18 answers of D-737 to D-754, and the fix of F-94 in `TheThingBelow.Game/scripts/Ui/MapScreen.cs`.
- The `flee` command with no encounter writes a warning and changes nothing, so a bot or the smoke session never stops (D-749).

### The questions that block progress

None.

### The next concrete action

Hand PR #45 to the cross-provider review. Answer each finding in this session (D-582).
