# Session handoff

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

## Session 148: 2026-09-20, Claude Code

Author: Claude Code
Session: PR-45, the debug assembly and the console. Repository: the-thing-below. Branch: `feat/pr-45-debug-assembly`. Role: author. Base: `ea2fec5`.

### What this session did, and why

- Asked the six questions of PR-45 first, and the owner answered each one (D-723 to D-728). Section 7.4 said "None", and the seam, the command set, the key, the proof of a release export, the kind of a debug id, and one stale file each needed an answer.
- `TheThingBelow.Debug` is the sixth project. It holds the four commands, the console screen, and one entry type (D-260, D-723, D-724).
- Game references it outside the `ExportRelease` configuration, and `DebugSeam` loads it by name. Thus no conditional compilation enters Game or Core, and F-27 of `area-core.md` closes (D-723).
- The backquote key opens the console, the world keeps its ticks, and the game makes no intent while the console is open (D-725).
- det-lint reads the commands folder with the float, clock, and OS random rules, because a handler changes a run inside a tick (D-724, T-7).
- The smoke session builds the console, reads the focus, and runs every command inside the engine. A release export reports an absent console, and the export job reads the file names of each build too (D-726).

### The state of the build

- `make verify` passes on the Mac: 1213 tests, the format check, det-lint and STE with 0 findings, the replay identity, the content hash, the atlas check, and the smoke session.
- No file of Core changed, so the simulation version stands at 5 (G-17).
- The remote head of `main` is `ea2fec5`. The branch holds four commits, and the effective head is `fd9e0ae`.
- CI passes on `fd9e0ae` on every leg: build, test, and format, smoke, replay identity, det-lint, ste-check, and coverage.
- The export job passes on Ubuntu, Windows, and macOS. Each export holds no file of the debug assembly, and each release build reports an absent console (D-726). The job runs on this pull request, because the pull request changes the export workflow and the project file of Game (D-699).

### What is in flight

The pull request is #43, and it waits for the review of Codex at the effective head `fd9e0ae`.

The gitar pass of `fd9e0ae` approved the code review, and it closed its one finding. That finding was a stale comment paragraph of `DescribeConsole`, which the move of the console check behind the seam left. The commit `fd9e0ae` drops it, and the answer sits on the thread of `TheThingBelow.Game/scripts/Boot.cs`.

The `review-gate` check gives one fault, RG 3, because the head holds no `docs/reviews/pr-43.md`. That fault clears with the review record. RG 1, RG 2, RG 6, RG 7, and RG 8 pass.

### Traps and gotchas

- The seam is text and reflection, so a rename on one side alone gives no compile error. `DebugSeamTests` reads each name from the built Game assembly and finds each member of the entry (D-723).
- The debug project takes no Godot source generator, so no type of it derives from a Godot node. The console builds engine nodes and connects to their signals.
- Tests references neither Game nor the debug project. It loads each built assembly from a path that the project file writes (D-614).
- The key press needs a play session. The smoke session covers the load, the nodes, the focus, the typed line, and every command, and PR-41 adds the screen test.
- The export job runs on this pull request, because the pull request changes the export workflow and the project file of Game (D-699).
- `dotnet build TheThingBelow.Game/TheThingBelow.Game.csproj --configuration ExportRelease` writes a release output, and that folder holds no debug assembly.

### The questions that block progress

None. OQ-115 blocks PR-8 alone.

### The next concrete action

Start the review of PR #43 at the effective head `fd9e0ae`.

## Session 147: 2026-09-20, Codex

Author: Codex
Session: review PR #42, the tile map. Repository: the-thing-below. Branch: `feat/pr-7-tile-map`. Role: reviewer. Base: `2a8115b`.

### What this session did, and why

- Reviewed PR #42 at effective head `2ead9c8`.
- Verified the opposite-provider gate, the complete 83-path diff, the PR comments, the PR-7 roadmap scope and exit tests, the changed contracts, and the save and replay migration.
- Verified the menu-opening crash correction and its regression tests.
- Added `docs/reviews/pr-42.md` with the verdict `Ready for owner merge`.

### The state of the build

- `make verify` passes at `2ead9c8` with 1182 tests, format, det-lint, STE, replay identity, content hash, atlas, and smoke.
- CI passes on Ubuntu, Windows, and macOS for build, test, format, replay identity, smoke, coverage, det-lint, STE, and changed paths. Gitar approves the current head.
- The review-gate check has the expected RG 3 fault until this review record is pushed.

### What is in flight

- This review record and this handoff entry need one metadata commit and push.

### Traps and gotchas

- The effective code head is `2ead9c8`. The tip `e4b311f` is metadata-only.
- The map HUD belongs to PR-64. The screen-test job belongs to PR-41.

### The questions that block progress

None. OQ-115 blocks PR-8 alone.

### The next concrete action

Commit and push the review record and this handoff entry. Then verify the remote head and review-gate result.

## Session 146: 2026-09-20, Claude Code

Author: Claude Code
Session: PR-7, the tile map. Repository: the-thing-below. Branch: `feat/pr-7-tile-map`. Role: author. Base: `2a8115b`.

### What this session did, and why

- Asked the four open questions of PR-7 first, and the owner answered each one (D-715 to D-720). The sight question took a second pass, because D-208 already said that the facing carries the sight of a patrol.
- Core gained the map rule file, the four-direction step, the two sight rules, and the record of every walked tile (D-528, D-716, D-718, D-719, D-567).
- The party on a tile map replaced the patrol of the first world. The snapshot took save format 2, and the simulation version rose to 5 (D-166, G-17).
- Game gained the map scene, the tile set from the tile page, the place of the view, and the held step (D-667, D-717, D-716).
- Two scope answers landed: PR-64 takes the whole map HUD, and the map takes the place of the demo panel of PR-61 (D-721, D-722).

### The state of the build

- `make verify` passes on the Mac: 1179 tests, the format check, det-lint and STE with 0 findings, the replay identity on simulation version 5, the content hash, the atlas check, and the smoke session.
- The remote head of `main` is `2a8115b`. The branch holds three commits and needs its push.

### What is in flight

The PR is #42, and it waits for the review of Codex at the effective head `2ead9c8`.

The gitar pass of `2ead9c8` approved the code review, and it closed its one finding. That finding is the crash below, and `2ead9c8` fixes it. The pass of the earlier head `28b06c6` raised it, and the answer sits on the thread of `TheThingBelow.Game/scripts/Boot.cs`.

Every CI check passes on every leg. The `review-gate` check gives one fault, RG 3, because the head holds no `docs/reviews/pr-42.md`. That fault clears with the review record. RG 1, RG 2, RG 6, RG 7, and RG 8 pass.

### Traps and gotchas

- A press of the menu button with a direction held crashed the run, and `2ead9c8` fixes it. The host reads input before it runs the ticks of a frame, so the queue held the open-menu intent while the menu state of the run was still closed. `GameRun.MenuOpenNextTick` now gives the state with the queued intents applied.
- The prompt of this session said that `docs/reviews/pr-41.md` still held `Changes required`. It does not. Session 145 wrote `Ready for owner merge` before the merge, so no correction was necessary.
- The save fixture of format 2 holds a step in progress, so a resume reads the step ticks too.
- `RunScripts.Make` now walks the party, so a change to it moves the save fixture of format 2 and no other stored file.
- Tests takes no reference to Game, so the camera tests and the held-step tests read the built assembly by reflection (D-614).

### The questions that block progress

None. OQ-115 blocks PR-8 alone.

### The next concrete action

Start the review of PR #42 at the effective head `2ead9c8`.

## Session 145: 2026-09-20, Codex

Author: Codex
Session: repeat review PR #41, the UI base. Repository: the-thing-below. Branch: `feat/pr-61-ui-base`. Role: reviewer. Base: `938ab7b`.

### What this session did, and why

- Reopened PR #41 at effective head `d6f5e00` after the author answered the prior review.
- Verified P1-1 against its original trigger and the real-run regression tests. The correction reads the menu state from `RunState` through `GameRun`.
- Withdrew P1-2. The prior review confused GitHub PR #41 with roadmap PR-41. This change is roadmap PR-61. The screen-test job belongs to later roadmap PR-41, section 7.5.
- Updated `docs/reviews/pr-41.md` with the verdict `Ready for owner merge`.

### The state of the build

- `make verify` passes with 1050 tests and all local gates.
- The remote metadata tip is `74f3c97`. The effective code head is `d6f5e00`.

### What is in flight

- The repeat review record and this handoff entry need one metadata commit and push.

### Traps and gotchas

- Keep the earlier `Changes required` verdict under `## Earlier verdicts`.
- The review file uses GitHub PR number 41. The roadmap scope uses PR-61.

### The questions that block progress

None. OQ-79 blocks later roadmap PR-41 only.

### The next concrete action

Commit and push the repeat review record and this handoff entry. Then verify the remote head and review-gate result.

## Session 144: 2026-09-20, Claude Code

Author: Claude Code
Session: PR-61, the UI base. Repository: the-thing-below. Branch: `feat/pr-61-ui-base`. Role: author. Base: `938ab7b`.

### What this session did, and why

- Answered the review of `docs/reviews/pr-41.md`, which gave `Changes required` for head `c8ea1ca`. `docs/reviews/pr-41-response.md` holds each disposition with its evidence.
- P1-1 has full merit on the defect. `Boot.ReadInput` passed a constant menu state, so the menu action made `intent.open_menu` on every press. `GameRun.MenuOpen` and `GameRun.IntentOf` now read the one source of that state, which is `RunState`, and `Boot` calls the run. A copy in the host would drift from the run after a replay or a load.
- The consequence of P1-1 needed one correction of fact: this head queues no intent, so no player could reach a menu. The defect was real and it would have become the stated consequence at the first PR that queues the intent.
- P1-2 has no merit. The finding read the GitHub number 41 as the roadmap id PR-41. This PR is roadmap PR-61, section 7.2 of the phase-2 file. The screen-test job is roadmap PR-41, section 7.5 of the same file, and OQ-79 blocks it.
- Four regression tests landed. One of them, `InputIntentTests.NoCallSiteOfTheInputMapPassesAConstantMenuState`, fails on the old code.

### The state of the build

- `make verify` passes on the Mac: 1050 tests, the format check, det-lint and STE with 0 findings, the replay identity on simulation version 4, the content hash, the atlas check, and the smoke session.
- The gitar pass of head `d6f5e00` approved the code review with no finding, and it opened no thread. The earlier pass of `6c5571a` did the same, and its CI note has its answer on the PR.
- The simulation version stays at 4. `GameRun.MenuOpen` reads a value that `RunState` already held, and no rule of Core changed (G-17).

### What is in flight

The PR waits for the repeat review of Codex at the new effective head `d6f5e00`. The `review-gate` check gives RG 4 and RG 5 faults, because the record still holds the verdict `Changes required` for head `c8ea1ca`. Both clear with the repeat review. RG 3, RG 7, and RG 8 pass. The CI legs run on the new head.

### Traps and gotchas

- Two numbering systems meet on this PR. The GitHub number is 41, and the roadmap id is PR-61. The review record file takes the GitHub number, and every roadmap line takes the roadmap id.
- The response file refutes P1-2 and never deletes it. The reviewer sets a refuted finding to `withdrawn` and keeps the evidence.
- `GameRun` now references the `Ui` namespace of Game for the input map. Core still holds no reference to either.

### The questions that block progress

None. OQ-79 blocks roadmap PR-41, which is a later PR, and it blocks no line of this one.

### The next concrete action

Start the repeat review of PR #41 at the new effective head.

## Session 143: 2026-09-20, Codex

Author: Codex
Session: review PR #41, the UI base. Repository: the-thing-below. Branch: `feat/pr-61-ui-base`. Role: reviewer. Base: `938ab7b`.

### What this session did, and why

- Reviewed PR #41 at effective head `c8ea1ca` after the author completed the UI base and the Gitar pass.
- Verified the opposite-provider gate, the complete 81-path diff, the PR comments, the affected contracts, and the roadmap exit tests.
- Found two blocking defects: `Boot.ReadInput` always passes `menuOpen: false`, and the PR does not add the screen-test workflow assigned to PR-41.
- Added `docs/reviews/pr-41.md` with the verdict `Changes required`.

### The state of the build

- `make verify` passes on the Mac with 1046 tests and all local gates.
- Revision-matched build, test, format, det-lint, replay identity, smoke, export, coverage, STE, and Gitar checks pass.
- `review-gate` is expected to fail until the review record is pushed. No `screen-test` check exists on the PR.

### What is in flight

- The review waits for the author to correct P1-1 and P1-2, push the corrections, and request a re-review.

### Traps and gotchas

- The effective head is `c8ea1ca`. The two later commits change only the metadata set.
- The PR title says PR-61, but the GitHub PR number is 41. The review record uses `pr-41.md`.
- The roadmap says OQ-79 blocks the screen-test job. The PR does not answer it or create the job.

### The questions that block progress

- OQ-79 remains open. It blocks the missing screen-test job.

### The next concrete action

Author fixes P1-1 and P1-2, then starts a re-review of PR #41 at the new effective head.
