# PR-41 review response

Date: 2026-09-20

Author response to `docs/reviews/pr-41.md`, which gave the verdict `Changes required` for head `c8ea1ca4d3e3335b5f392bef26779a6a6c724a83`.

## P1-1: The Menu action cannot close the menu

Disposition: **full merit on the defect, partial merit on the consequence. Corrected.**

The trigger reproduces. `Boot.ReadInput` called `InputActions.IntentOf(action, false)` with a constant, so the `true` branch was unreachable from Game and the menu action made `intent.open_menu` on every press.

The consequence needs one correction of fact. This head sends no intent to the run at all, because `ReadInput` writes a log line alone until PR-7 adds the map rules (D-561). Thus no player of this build could reach a menu to be trapped in. The defect is real, and it would have become the stated consequence at the first PR that queues the intent.

The correction takes the state from the run, which holds the one source of it, and not from a copy in the host:

- `GameRun.MenuOpen` gives `Simulation.State.MenuOpen` (D-650).
- `GameRun.IntentOf(action)` makes the intent of one action and reads that state.
- `Boot.ReadInput` calls `run.IntentOf(action)`.

A copy of the menu state in the host would drift from the run after a replay or a load, and the action would then send the wrong intent. The review named a copy in the caller as the correction. This answer keeps the same contract with one source instead of two.

Regression checks, in `TheThingBelow.Tests`:

- `GameRunTests.TheMenuActionOpensTheMenuAndThenClosesIt` sends the menu action two times through a real run and reads `intent.open_menu` and then `intent.close_menu`. That is the input-path test that the review asked for, with no engine (D-614).
- `GameRunTests.TheMenuStateOfTheRunFollowsTheIntents` pins the one source of the state.
- `GameRunTests.EveryOtherActionMakesOneIntentWhateverTheMenuDoes` pins that the menu action alone reads the state.
- `InputIntentTests.NoCallSiteOfTheInputMapPassesAConstantMenuState` fails on the old code. It reads each file of `TheThingBelow.Game/scripts` and refuses the text `IntentOf(action, false)`, which the old `Boot.cs` held.

## P1-2: PR-41 does not create the required screen-test job

Disposition: **no merit. Refuted.**

The finding reads the GitHub number of this pull request as a roadmap id. They are two numbering systems, and the `one-pr-one-session` skill states it in step 1: "PR: `#<n>`, the GitHub number, or the one PR intent before GitHub gives a number. A roadmap id such as PR-5 is a different number."

This pull request is GitHub #41. Its roadmap id is **PR-61**, section 7.2 of `docs/roadmaps/phase-2-first-playable.md`. The screen-test job belongs to roadmap **PR-41**, which is section 7.5 of the same file, a later entry of the same phase.

The evidence:

- `grep -n '^### 7\..* PR-41:' docs/roadmaps/` gives `docs/roadmaps/phase-2-first-playable.md:234:### 7.5 PR-41: the screen-test job`. Section 7.2 of that file is `PR-61: the UI base, the frame, the fonts, and the intents`.
- The out-of-scope list of section 7.2 names it: "The screen captures of the frame and the fit. PR-41 creates the screen-test job and takes them (D-172, G-16)."
- The PR gate of `CLAUDE.md` holds the line "The `screen-test` job is green: each fixture screen matches the committed baseline (D-172, F-23). PR-41 creates it." That line is the form of G-16: each check that does not exist yet has a line that names the PR that creates it. It is a repository-wide statement, and it predates this pull request.
- The review itself gives the decisive evidence in its own "Open questions" section: "OQ-79 remains open and names how the screen-test job pins Mesa. The roadmap says it blocks PR-41." A pull request that an open question blocks cannot be the pull request that is ready to merge. OQ-79 blocks the future roadmap PR-41, and D-19 forbids a session from picking a default for it.

The scope of this pull request is section 7.2 of the phase roadmap, and the session answered every open question of that section before it wrote a change (D-19). No contract asks this pull request for the screen-test job.

No correction follows from this finding.

## The state after the corrections

- `make verify` passes: the build, 1050 tests, the format check, det-lint and STE with 0 findings, the replay identity on simulation version 4, the content hash, the atlas check, and the Godot smoke session.
- The simulation version stays at 4. `GameRun.MenuOpen` reads a value that `RunState` already held, and `GameRun.IntentOf` adds no rule to Core (G-17).
- The correction touches `TheThingBelow.Game/scripts/GameRun.cs`, `TheThingBelow.Game/scripts/Boot.cs`, `TheThingBelow.Tests/GameRunTests.cs`, and `TheThingBelow.Tests/InputIntentTests.cs`.
