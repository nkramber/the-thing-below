# Session handoff

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

## Session 150: 2026-09-20, Claude Code

Author: Claude Code
Session: PR-41, the screen-test job. Repository: the-thing-below. Branch: `feat/pr-41-screen-test`. Role: author. Base: `1e0c6b1`.

### What this session did, and why

- Asked the seven questions of PR-41 first, and the owner answered each one (D-729 to D-736). OQ-79 blocked the PR, and the register options needed one more (D-19).
- The job installs Mesa from one pinned timestamp of the snapshot service of Ubuntu, and it reads back each version. The snapshot service keeps every timestamp, so the pin can never drop out of the archive (D-729, D-730).
- The job draws with the Mobile renderer on lavapipe, and not with the Compatibility renderer of D-172. D-616 picked Mobile for every shipped build five days after D-172, so each capture now shows the renderer of the player (D-731).
- The `--capture` argument of Game writes one PNG for each capture of `ScreenCaptures` (D-732). The fixtures are the map screen and a UI panel (D-734).
- The `screens` command of Tools compares decoded pixels, or it joins the captures into a contact sheet (D-735, D-736, F-19).
- A baseline comes from the renderer of CI, so the artifact of the job gives each new frame and the author commits it by hand (D-733).

### The state of the build

- `make verify` passes on the Mac with 1272 tests, and `make sheet` writes the sheet, so exit test 6 holds.
- CI passes on every leg at the head `233b890`, and the `screen-test` job is green.
- The job reports the rendering method `mobile` and the rendering driver `vulkan` on `llvmpipe (LLVM 20.1.2, 256 bits)`, so D-731 holds.
- The two runs of the job give the same frames, so exit test 4 holds.
- `screens/baseline` holds the 10 files of the artifact of the run `35549963049`, and they take 184 KB.
- No file of Core changed, so the simulation version stands (G-17).
- The remote head of `main` is `1e0c6b1`.

### What is in flight

The pull request is #44, and it waits for the review of Codex at the effective head `233b890`. The gitar pass of that head found no issue, and the pull request holds no review thread.

The `review-gate` check gives one fault, RG 3, because the head holds no `docs/reviews/pr-44.md`. That fault clears with the review record. RG 1, RG 2, RG 6, RG 7, and RG 8 pass. RG 7 failed one time, because the `docs/reviews/` row of the description held no form of D-581, and the description now holds that form.

Three rounds came before the green run, and each one found a real fault:

- The readiness check of Xvfb called `xdpyinfo`, which the image of the runner does not hold. The job now starts the screen with `xvfb-run`, which also ends the background process of the step.
- The driver file of lavapipe is `lvp_icd.json` on this image, and not `lvp_icd.x86_64.json`. The loader then held no driver, and Godot fell back to OpenGL. The job now finds the file and fails when the pin installs none.
- A session with a window opens the audio driver of the system, and the runner has no sound card. The session now takes the dummy audio driver.

### Traps and gotchas

- The Mac cannot write a baseline. It is Apple silicon, and the baseline comes from the software Vulkan driver of Linux (D-733).
- The capture session runs no tick, so the frame time of the engine reaches no capture (T-7).
- Each capture builds its fixture again, because the default body size follows the fit of the screen (D-707).
- At 1440 rows both fit modes give one picture, because the frame reaches that screen at a whole scale of 2 (D-568).
- A move of the Mesa pin needs a new baseline in the same PR (D-730).

### The questions that block progress

None. OQ-115 blocks PR-8 alone.

### The next concrete action

Hand PR #44 to Codex for the cross-provider review. A PR that changes `.github/workflows/` never takes the label of D-401 (D-700).

## Session 149: 2026-09-20, Codex

Author: Codex
Session: review PR #43, the debug assembly and the console. Repository: the-thing-below. Branch: `feat/pr-45-debug-assembly`. Role: reviewer. Base: `ea2fec5`.

### What this session did, and why

- Reviewed PR #43 at effective head `fd9e0ae`.
- Verified the opposite-provider gate, the complete diff, the PR comments, the PR-45 roadmap exit tests, the debug seam, command dispatch, replay mark, release exclusion, console focus, and export checks.
- Verified the closed Gitar finding and found no additional in-scope defect.
- Added `docs/reviews/pr-43.md` with the verdict `Ready for owner merge`.

### The state of the build

- `make verify` passes with 1213 tests and all local gates.
- GitHub passes build, test, format, smoke, replay identity, det-lint, STE, coverage, changed paths, and all three export legs for the PR.
- Gitar approves the effective head `fd9e0ae`, and its one finding is closed.
- The review-gate check is expected to fail until the review record is pushed. RG 3 is the only missing record condition.

### What is in flight

- The review record and this handoff entry are pushed in `32fa076`.

### Traps and gotchas

- The effective code head is `fd9e0ae`. Commits `5cb8912` and `99c8c5c` change metadata only.
- PR #43 is roadmap PR-45. Do not confuse the GitHub number with the roadmap number.

### The questions that block progress

None. OQ-115 blocks PR-8 alone.

### The next concrete action

Fetch and verify that the review-gate check is green and that no branch commits remain ahead of the remote.
