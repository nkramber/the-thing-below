# Session handoff

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

## Session 138: 2026-09-19, Codex

Author: Codex
Session: review PR #39, the export job. Repository: the-thing-below. Branch: `feat/pr-54-export-job`. Role: reviewer. Base: `fb17f87`.

### What this session did, and why

- Recomputed PR #39 at effective head `671d712`. The later commit `620f690` changes only the handoff metadata.
- Confirmed the provider gate. Claude Code authored the PR, and Codex reviewed it.
- Inspected the complete 18-path diff, the workflow, export presets, project setting, tests, licenses, decisions, questions, roadmaps, and handoff records.
- Ran `make verify`. It passed with 832 tests and clean build, format, det-lint, STE, replay identity, content hash, and smoke checks.
- Found P1-1: the exported smoke process is followed by `|| true`, so a nonzero process status is discarded.
- Wrote `docs/reviews/pr-39.md` with the verdict `Changes required` for effective head `671d712`.

### The state of the build

- The remote PR head is `620f690`, and the effective implementation head is `671d712`.
- The automated pass is current at `671d712` and approved after its cancellation finding was fixed.
- The review-gate check waits for this review record.

### What is in flight

The author must preserve the exported process status and rerun the export checks. The review record and this entry need a commit and push for the current review round.

### Traps and gotchas

- The review verdict targets `671d712`, not the metadata tip `620f690`.
- The repository test command is `make verify`. A direct `dotnet test` filter discovered zero tests and is failed evidence.
- The existing CI smoke job uses `pipefail` and preserves the process status. The export workflow must keep that property while also checking the success line.

### The questions that block progress

None. OQ-198 and OQ-199 remain open but do not block this review.

### The next concrete action

Correct P1-1, push the author correction, and rerun the review at the new effective head.

## Session 137: 2026-09-19, Claude Code

Author: Claude Code
Session: PR-54, the export job. Repository: the-thing-below. Branch: `feat/pr-54-export-job`. Role: author. Base: `fb17f87`.

### What this session did, and why

- Added `.github/workflows/export.yml`: three legs, the editor and the template downloads with a SHA-512 check, the import, the export, the license copy, the smoke session, one packed archive, and a 90-day artifact (D-449, D-512, D-596).
- Added `TheThingBelow.Game/export_presets.cfg` with the three targets of D-481 and D-482, and no include filter (D-508, F-73).
- Added `licenses/` with the three notices of D-467, each one a copy of its upstream text.
- Ran every export on the Mac of the owner. The macOS export found two faults, and F-74 and F-90 record them.
- Added 25 tests in four files: the presets, the project settings, the workflow triggers, the digests, and the license set.
- The owner answered four questions, and D-690 to D-693 hold them. OQ-198 and OQ-199 are new.
- Answered the automated pass. It found one bug: each push to `main` shares one concurrency group, so a merge cancelled the export of the merge before it. The cancel now applies to a pull request alone, and a test reads the value.
- Corrected the `docs/reviews/` row of the PR description. It held no form of D-581, and RG 7 faulted on it, as PR #38 did.
- Answered the cross-provider review of `671d712`. Its one finding, P1-1, has full merit: the export step ran the game with `|| true` and dropped the exit code. The step now keeps the code and reads it after the log checks.
- The `smoke` target of the `Makefile` held the same fault, and the owner put the repair in this PR. D-694 records that exception to G-8, and F-92 records the fault.
- Added `TheThingBelow.Tests/SmokeExitCodeTests.cs`, which holds the rule for the three callers of the session. `docs/reviews/pr-39-response.md` holds each disposition.

### The state of the build

- `make verify` passes on this machine with 837 tests, and `ste-check` gives 0 findings.
- The remote head of `main` is `fb17f87`, and this branch starts there. The PR is #39.
- The review record `docs/reviews/pr-39.md` gives `Changes required` for `671d712`. This round answers its one finding.
- Every CI check passed at `620f690`, the three export legs included. `review-gate` held RG 3 alone, which the review record cleared.
- The three artifacts are live: 62 MB for Linux, 69 MB for Windows, and 123 MB for macOS. Each one expires on 2026-12-18.
- The five exit tests of section 7.1 all ran in CI: the export, the smoke session on the export, the three license files, the trigger of this PR, and the artifacts.

### What is in flight

The push of this round, a new automated pass, and the repeat review of the new head. The PR changes code and `.github/workflows/`, so no label of D-401 applies.

### Traps and gotchas

- F-74 is a project setting, and not a preset option. The macOS export fails with the ETC2 ASTC import setting off in `TheThingBelow.Game/project.godot`.
- F-90: the built-in signer of Godot writes a macOS signature that the kernel refuses. The game dies with signal 9 and no output, and `codesign --verify` calls that signature valid.
- The executable in the macOS bundle takes the application name, "The Thing Below", and not the name of the export file. The job finds it.
- An artifact upload drops the execute bit, so the job packs one archive for each leg.
- The carry-over note of the last prompt is wrong: `main` holds `docs/reviews/pr-38.md`, and PR #38 took its Codex review.
- The OQ-59 correction goes to its own documents PR (D-690).
- D-694 puts a second concern in this PR, the `Makefile` fix. It is an owner exception to G-8, and no later PR takes such a fix without one.
- A push to this PR runs the three exports again, because GitHub reads a path filter of a pull request against the whole diff of the pull request.

### The questions that block progress

None. OQ-198 blocks PR-31, and OQ-199 blocks the move of D-456 in Phase 6.

### The next concrete action

Push this round, answer the new automated pass, and ask for the repeat review of the new head.

## Session 136: 2026-09-19, Codex

Author: Codex
Session: review PR #38, the Sprite Fusion test pick. Repository: the-thing-below. Branch: `docs/pr-89-sprite-fusion-pick`. Role: reviewer. Base: `db518fa`.

### What this session did, and why

- Recomputed PR #38 at effective head `83e561c`. The later commit `cae4e90` changes only metadata.
- Confirmed the cross-provider gate. Claude Code authored the PR, and Codex reviewed it.
- Inspected the complete eight-path diff, the four new decision rows, the four findings, the roadmap changes, and the handoff rotation.
- Verified the corrected credit arithmetic: 495 credits started, 165 credits were spent, and 330 credits remain.
- Wrote `docs/reviews/pr-38.md` with the verdict `Ready for owner merge`.

### The state of the build

- `make verify` passes with 806 tests, 0 failures, 0 warnings, and clean format, det-lint, STE, replay identity, content hash, and bounded smoke checks.
- The effective head is `83e561c`. The metadata tip is `cae4e90`.
- The automated pass approved `83e561c` with no open thread. Its two findings were answered, and RG 7 passes.
- `review-gate` waits for this review record.

### What is in flight

The review record and this handoff entry need a commit and push. After the remote gate reads the record, the PR is ready for owner merge.

### Traps and gotchas

- The verdict targets effective head `83e561c`, not metadata tip `cae4e90`.
- D-687 includes a 45-credit bonus. The arithmetic is 495 minus 165 equals 330.
- PR-51 owns the PNG import implementation and its tests. This PR records its requirements only.

### The questions that block progress

None.

### The next concrete action

Commit and push the review record and this handoff entry. Then fetch and verify the remote head and review-gate result.

## Session 135: 2026-09-19, Claude Code

Author: Claude Code
Session: the documents PR of the Sprite Fusion pick. Repository: the-thing-below. Branch: `docs/pr-89-sprite-fusion-pick`. Role: author. Base: `db518fa`.

### What this session did, and why

- Carried the pick of the Sprite Fusion test from `spike/pick.md` on the spike branch to `main`. That branch never merges (D-620).
- Added D-686 and D-687: the generator draws every picture, and the Starter plan stays at 9 USD each month.
- The carry found a conflict. The pick gives PR-51 a map to the nearest palette color, and section 7.39 refused a near color (T-2). The owner answered, and D-688 and D-689 record the two answers.
- Added F-86 to F-89 to the register of `docs/design.md`, and short rows to the findings tables of the two area files.
- Updated section 7.1 of `docs/roadmaps/area-art.md`, section 7.11 of `docs/roadmaps/area-tools.md`, sections 7.17 and 7.22 of the phase-1 file, and section 7.39 of the phase-2 file.
- Marked the test done in the three sequence lists, and marked line 10 of Gate 1 met by D-686.
- Carried the entry of Session 134 from the spike branch, and moved the entries of Session 124 and Session 125 to the archive.
- Answered the automated pass. It read the three credit figures of D-687 as a contradiction on a 450-credit plan. The owner said that a bonus of 45 credits arrived, so the row now records 495 credits at the start.
- Fixed the `docs/reviews/` row of the PR description. It held no form of D-581, and RG 7 faulted on it.

### The state of the build

- `make ste-check` passes at the commit of this entry. The PR changes documents alone.
- The remote head of `main` is `db518fa`, and this branch starts there.
- The automated pass approved the head `83e561c` at 22:57 UTC, with one finding closed and no open thread. The CI summary of that pass still names the old RG 7 fault, and the log of the job gives `RG 7 pass`.
- The `review-gate` check holds one fault: RG 3, which asks for `docs/reviews/pr-38.md`. The Codex review record clears it.

### What is in flight

The PR waits for the Codex review. The PR adds decision rows, so the label of D-401 does not apply. RG 3 faults until the record `docs/reviews/pr-38.md` lands, which is the normal state before a review.

### Traps and gotchas

- The draft of D-686 in `spike/pick.md` said that the pick revises D-57 in part. D-57 covers the story text and not a picture, so D-686 drops that claim. D-107 takes the revision alone.
- The pick draft named two decision rows. The conflict of section 7.39 needed an owner answer, so the PR carries four.
- No picture of `spike/generated/` reaches `content/`. The import of PR-51 gives each one a frame and the palette (G-24).
- The spend of 165 credits is 11 calls at 15 credits, and the test made eight calls. The dashboard of the supplier gives the figure, and the row says so.
- This PR takes no `PR-#` id and no roadmap entry, as the documents PR of D-680 did.

### The questions that block progress

None.

### The next concrete action

Push the branch, open the PR, and answer the pass of gitar. Then hand the PR to Codex for the review.
