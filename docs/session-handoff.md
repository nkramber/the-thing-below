# Session handoff

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

## Session 153: 2026-09-21, Claude Code

Author: Claude Code
Session: PR-8, the enemies on the map, round 2. Repository: the-thing-below. Branch: `feat/pr-8-map-enemies`. PR: #45. Role: author. Base: `626d2fe`.

### What this session did, and why

- Read the first CI run of PR #45. The `screen-test` job failed on the five map frames alone, because the lead now draws (F-94). The five UI frames match the baseline by pixel.
- Downloaded the artifact `screen-captures` of the run `35555586770`, and read each map frame. Each one shows Marrek on the spawn tile, crisp at each scale. No enemy is in the sight of the party on this night map, as D-719 wants.
- Committed the five map frames as the new baseline (D-733).
- Attached the review sheet of the map sprite page to the PR description, with the fixture enemy (D-514).

### The state of the build

- On the first run, every leg of the build, test, and format job that finished passed, and so did the smoke, replay identity, det-lint, and STE jobs.
- The `review-gate` check fails on RG 3 alone until the review record lands.
- The remote head of `main` is `626d2fe`.

### What is in flight

The push of this round runs CI again. The gitar pass follows, and then the PR goes to the cross-provider review.

### Traps and gotchas

- The logs of a job stay locked while its run is still in progress, but the artifact is ready at once.
- The map frames of PR #44 held no character, and the owner approved them. A dark frame hides a missing sprite, so read each frame at a crop.

### The questions that block progress

None.

### The next concrete action

Wait for CI and the gitar pass on the new head, answer each gitar comment, and hand PR #45 to the cross-provider review.

## Session 152: 2026-09-21, Claude Code

Author: Claude Code
Session: PR-8, the enemies on the map. Repository: the-thing-below. Branch: `feat/pr-8-map-enemies`. Role: author. Base: `626d2fe`.

### What this session did, and why

- Asked 18 questions before any change, and the owner took each recommendation (D-737 to D-754). OQ-115 blocked the PR, and D-737 closes it.
- Core: the `enemies` array of the map file, the route and the area of each enemy, the walk, the sight with the beat, the block and the step into a body, the side from behind, the grace time, and save format 3.
- The console gains `flee`, which ends an encounter as a flee until PR-9 (D-749). With no encounter, it writes a warning and changes nothing, so the smoke session and a bot never stop.
- Game draws each enemy that the party sees, and the mark of a sight (D-719, D-744).
- F-94: every map sprite drew behind its own floor tile, so the baseline of PR #44 shows no character. Each map sprite now sits at the south edge of its front row (D-737).

### The state of the build

- `make verify` passes on the Mac with 1390 tests, the smoke session included.
- The simulation version is 6, the save format is 3, and the identity file and the content hash are new.
- The remote head of `main` is `626d2fe`.

### What is in flight

The PR opens in this round. The screen-test job must fail on the old baseline, because the lead now draws (F-94). The next round commits the frames of its artifact as the new baseline (D-733), and then the gitar pass runs.

### Traps and gotchas

- A spread of an `IReadOnlyList` into an array calls `System.Linq`, and the reference test of Core fails. Copy with a loop.
- `make test` runs with no build. Run `make build` first, or a test reads an old assembly.
- The fixture drawing of the enemy comes from this session by hand, as the fixture tiles of PR-7 did. The owner reads its sheet in the PR, and PR-17 draws each real enemy (D-686, D-744).
- The step of an enemy starts on the tick that it arrives, so a step of 30 ticks lands 31 ticks after the start of the run.

### The questions that block progress

None.

### The next concrete action

Read the result of the `screen-test` job, download its artifact, read each frame, and commit the new baseline. Then run the gitar wait.

## Session 151: 2026-09-20, Codex

Author: Codex
Session: review PR #44, the screen-test job. Repository: the-thing-below. Branch: `feat/pr-41-screen-test`. Role: reviewer. Base: `1e0c6b1`.

### What this session did, and why

- Reviewed PR #44 at effective head `233b890`.
- Verified the opposite-provider gate, the complete diff, the PR comments, the PR-41 roadmap exit tests, the capture lifecycle, the renderer checks, the deterministic two-run comparison, the baseline comparison, and the contact-sheet command.
- Found no in-scope defect.
- Added `docs/reviews/pr-44.md` with the verdict `Ready for owner merge`.

### The state of the build

- `make verify` passes with 1272 tests and all local gates.
- GitHub passes the build, test, format, smoke, replay identity, det-lint, STE, coverage, changed-paths, screen-test, and Gitar checks at tip `023211d`. Two Windows jobs were still in progress when read.
- The effective implementation head is `233b890`. The later commits change only handoff and review metadata.
- The review record and handoff are on remote head `4858ab7`. The review-gate check should pass after CI reads this metadata commit.

### What is in flight

The pull request waits for the owner merge. The review applies to effective head `233b890`.

### Traps and gotchas

- The GitHub tip is `023211d`, but the two commits after `233b890` change only metadata paths.
- A later Mesa pin needs a new baseline in the same PR.
- A PR that changes `.github/workflows/` never takes the review-override label.

### The questions that block progress

None. OQ-115 blocks PR-8 alone.

### The next concrete action

Verify the review-gate check and the remote branch state.
