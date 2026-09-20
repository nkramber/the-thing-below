# Session handoff

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

## Session 142: 2026-09-20, Claude Code

Author: Claude Code
Session: PR-61, the UI base. Repository: the-thing-below. Branch: `feat/pr-61-ui-base`. Role: author. Base: `938ab7b`.

### What this session did, and why

- Asked the three open questions of the PR and got each owner answer (D-19). D-707 to D-713 record them, with the probe answers of 2026-09-19.
- D-707 revises D-639 and G-28 in part: the floor of 2 device pixels goes, and the setting gives two body sizes, 24 and 32 frame pixels. D-708 revises D-241 in part. D-263, D-264, and D-228 gain a note on the font size.
- Added the two fonts under `content/fonts/`, the UI style file, the device table, and 13 UI drawings.
- Core gained `FontStrikes`, `UiStyle`, and `DeviceNames`. The content set now refuses a build with no strike for a body size, a style that names an absent drawing, or a glyph set with a hole.
- Game gained the `Ui` namespace: the frame of 1280 by 720, both steps of the fit of D-573, the world viewport of 640 by 360 at 2x, the fonts, the theme, the one text helper of D-499, the input map, the intents, the glyph sets, and the crash message.
- The smoke session builds the UI base at both body sizes and reads each font setting back, so every CI leg proves D-710 inside the engine.
- The owner approved the art batch of 13 drawings on 2026-09-20 (D-714, G-25). The review sheets are in the description of PR #41 (D-514).

### The state of the build

- `make verify` passes on the Mac: 1046 tests, the format check, det-lint and STE with 0 findings, the replay identity on simulation version 4, the content hash, the atlas check, and the smoke session.
- PR #41 is open. Every CI check passes but `review-gate`, which faults on RG 3 until the review record exists. That is the normal state of a PR before its review.
- The gitar pass of head `c8ea1ca` approved the code review with no finding. Its CI note found a real fault in the `docs/reviews/` row of the Documents section, and the description now holds the `No change needed because` form. A local gate run gives RG 7 pass and RG 8 pass.
- The content hash did not move: the fonts and the UI files sit outside `content/rules/` (D-495, D-648).

### What is in flight

The PR waits for the review of Codex. No label applies, because the PR adds decision rows and code (D-401, D-560).

### Traps and gotchas

- A font size with no bitmap strike draws the traced outline in silence (F-49). `FontStrikes.RequireSize` and the pinned `FixedSize` of `GameFonts` each refuse it.
- The simulation version stays at 4. PR-61 adds intent ids that no rule of Core reads, and it changes no state.
- `content/ui/devices.json` names the buttons. A new button needs its drawing in all four sets, and its label in the string table.
- The crash address is a placeholder in the reserved `.invalid` domain. OQ-57 stays open, and it blocks PR-33 and PR-75 (D-712).
- The session deleted `HANDOFF-PR-61.md`, the untracked note of 2026-09-19. Its answers are in D-707 to D-713.

### The questions that block progress

None. OQ-57 stays open, and the placeholder of D-712 unblocks this PR.

### The next concrete action

Hand PR #41 to Codex for the review.

## Session 141: 2026-09-20, Codex

Author: Codex
Session: review PR #40, the audit fixes. Repository: the-thing-below. Branch: `fix/audit-fixes`. Role: reviewer. Base: `4a472c5`.

### What this session did, and why

- Reviewed PR #40 at effective head `828e5b0` after the author completed the audit fixes.
- Verified the opposite-provider gate, the full changed path set, the existing Gitar comment and answer, the changed contracts, and the affected callers.
- Ran `make verify`. It passed with 953 tests and all local gates.
- Added `docs/reviews/pr-40.md` with the verdict `Ready for owner merge`.

### The state of the build

- The local build, tests, format, det-lint, STE, replay identity, content hash, atlas, and smoke checks pass.
- The PR head is `eb1c906`. The review record and handoff entry are on the remote branch.

### What is in flight

The review-gate check is pending after the metadata push. The other required checks are also pending on the new head.

### Traps and gotchas

- The effective head is `828e5b0`. The review publication commit changes only the metadata set.
- The existing review-gate failure is expected before the review record exists.
- `HANDOFF-PR-61.md` is an unrelated untracked note. Do not delete it.

### The questions that block progress

None.

### The next concrete action

Wait for the revision-matched checks, then verify the review-gate result.

## Session 140: 2026-09-20, Claude Code

Author: Claude Code
Session: the audit fixes PR (D-696). Repository: the-thing-below. Branch: `fix/audit-fixes`. Role: author. Base: `4a472c5`.

### What this session did, and why

- The session before this one ran a principal-level audit of the whole repository at `4a472c5`. The owner asked for every fix in one PR (D-696). That session wrote the commit `78e7f3a` and an untracked note, and no entry. This entry holds the content of that note.
- `docs/reviews/audit-2026-09-20.md` holds each finding, A-1 to A-28, with its state. F-93 is the finding of the design register. D-695 to D-706 hold the owner answers, and OQ-200 to OQ-209 are closed.
- This session wrote the three code items that waited for it, each with a test that fails on the old code (T-3):
  - D-702: `DocumentSet` reads `git ls-files -z` when the root holds `.git`, and `TrackedFiles` is new. A `git` run that fails is an error with the root and the exit code. A root with no git data keeps the read of the folder tree. `SteCheckTrackedFileTests` holds 8 tests.
  - D-699: `export.yml` holds five trigger paths, and `ExportWorkflowTests` pins them.
  - D-700: `OverrideRules` refuses `.claude/settings.json` before the folder match, and RG 1 gives that path a reason with D-700.
- It corrected each document that said the opposite: two runbooks, two skills, one reference file, three roadmaps, both agent files, the PR template, the hook, the `Makefile`, and `ci.yml`.
- It ran `make smoke` after the changes to `Boot.cs` and `GameRun.cs`, and it moved Session 130 to the archive.

### The state of the build

- `make verify` passes on the Mac: 953 tests, the format check, det-lint and STE with 0 findings, the replay identity on simulation version 4, the content hash, the atlas check, and the smoke session.
- The branch holds `78e7f3a` and the commit of this entry. The remote head is that commit, on `origin/fix/audit-fixes`.
- `CLAUDE.md` holds 16220 of 16384 bytes.

### What is in flight

The PR waits for gitar, and then for the review of Codex. No label applies, because the PR changes decision rows, workflows, and code (D-401, D-560).

### Traps and gotchas

- The STE check reads the index for the file set, and the working tree for the text. A staged new file takes the rules. The new tests run `git`, so each CI leg needs it on the path.
- `HANDOFF-PR-61.md` is the untracked note of another session. Do not delete it.
- The identity file changed for simulation version 4. The three CI legs must agree with it.
- The live `review-gate` check runs the code of `main`, so the new RG 7 first reads the next PR (F-37).
- `rollForward` of `global.json` is `latestPatch` now. No owner answer covers it. Revert it if the reviewer objects.
- D-704: game text says "party" for the travelers, and the glossary keeps "party" for the characters in battle. Ask the owner when the two uses collide.
- D-706: a direct push to `main` fails for the owner too.
- The trial of gitar ends about 2026-09-23 (D-685). The GPL license keeps the free reviews (D-695).
- Sessions 86, 94, 95, 97, and 119 exist in no file.

### The questions that block progress

None.

### The next concrete action

Wait three minutes for gitar (D-705), answer each comment, and tell the owner that the PR is ready for Codex.

## Session 139: 2026-09-20, Codex

Author: Codex
Session: repeat review PR #39, the export job. Repository: the-thing-below. Branch: `feat/pr-54-export-job`. Role: reviewer. Base: `fb17f87`.

### What this session did, and why

- Reopened PR #39 at effective head `82d1afd`.
- Verified the provider gate and read the author response file.
- Rechecked P1-1 against its original trigger. The export command and the `Makefile` smoke target now keep the process exit code.
- Ran `make verify`. It passed with 837 tests and clean format, det-lint, STE, replay identity, content hash, and smoke checks.
- Confirmed the revision-matched CI checks and all three export legs pass. Gitar passes.
- Updated `docs/reviews/pr-39.md`, kept the earlier verdict, and set the current verdict to `Ready for owner merge`.

### The state of the build

- The effective head is `82d1afd`. The review-gate check waits for this review record.
- The earlier finding P1-1 is fixed in `82d1afd`.

### What is in flight

The repeat-review record and this handoff entry need a metadata commit and push. The current review-gate check still reads the earlier record.

### Traps and gotchas

- The effective head is `82d1afd`, not the later metadata commit that will publish this review.
- The author response also repairs the same exit-status fault in `Makefile` under owner decision D-694.

### The questions that block progress

None. OQ-198 and OQ-199 remain open but do not block this review.

### The next concrete action

Run STE and the diff check, commit the review record and this entry, push, and verify the remote tip and review-gate result.
