# Session handoff

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
- The PR head is `828e5b0`. The review record waits for the metadata commit and push.

### What is in flight

The review record and this handoff entry need one metadata commit and push. The review-gate check should pass after the push.

### Traps and gotchas

- The effective head is `828e5b0`. The review publication commit changes only the metadata set.
- The existing review-gate failure is expected before the review record exists.
- `HANDOFF-PR-61.md` is an unrelated untracked note. Do not delete it.

### The questions that block progress

None.

### The next concrete action

Run the STE check and diff check, commit the review record and this entry, push, fetch, and verify the remote head and review-gate result.

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

## Session 134: 2026-09-19, Claude Code

Author: Claude Code
Session: the Sprite Fusion test, item 24 of section 8 of the phase file. Repository: the-thing-below. Branch: `spike/sprite-fusion`. Role: spike author. Base: `db518fa`.

### What this session did, and why

- Ran the test of D-620 and D-675. The owner picked the subjects: the approved map sprite of Marrek as the anchor, and three subjects with no art before the test.
- Drew four drawing files under `spike/session/content/sprites/`, and rendered them with the atlas command at `--root spike/session`. No file of `content/` changed.
- Read the pages of the supplier for the API, the cost, and the terms. The owner gave the API key, and the session made eight calls: one cold call and one style call for each subject.
- The style call sent the approved cast sprites as style references. The anchor call never sent the sprite of Marrek.
- Built the comparison sheet, the repeat sheet of the tile, and a contact sheet for each of the eight calls.
- The owner picked the generator for every picture, and the owner kept the Starter plan.

### The state of the build

- `make ste-check` passes at tip `e3c50b4` and after the commit of this entry.
- The branch holds two commits over `db518fa`, and it never merges (D-620).
- The remote head of `main` is `db518fa`.

### What is in flight

The documents PR of the pick. The file `spike/pick.md` holds each draft row and each change of a document.

### Traps and gotchas

- The branch never merges. Nothing of the test reaches `main` except through the documents PR.
- `spike/generated/` holds 84 pictures from the tool. No picture enters `content/` before the import of PR-51 gives it a frame and the palette.
- The tool holds no size: a call for 32 pixels returned up to 42 pixels.
- The tool draws a tile as a framed block, so a floor of its tiles shows a grid. The owner read this before the pick.
- The API key is at `~/.config/sprite-fusion/api-key`. No file of the repository holds it.
- The session used a scratch builder outside the repository for the portrait grid. No Python file entered the repository (D-99).

### The questions that block progress

None.

### The next concrete action

A new clean session opens the documents PR that carries the two decision rows, the four findings, the changes of the documents, and this entry.

## Session 133: 2026-09-19, Codex

Author: Codex
Session: review PR #37, the stable check names of the CI matrix jobs. Repository: the-thing-below. Branch: `fix/pr-88-ci-matrix-check-names`. Role: reviewer. Base: `51a040f`.

### What this session did, and why

- Recomputed PR #37 at effective head `9927be7`. The later commit `8a041e3` changes only metadata.
- Confirmed the provider gate. Claude Code authored the PR, and Codex reviewed it.
- Inspected the complete nine-path diff. The three gate jobs preserve the D-595 condition on their matrix jobs, fail on an unexpected skip or a fault of `changed-paths`, and report the stable names of D-682 and D-683.
- Ran the focused workflow tests and the full local verification. No finding remains.
- Wrote `docs/reviews/pr-37.md` with the verdict `Ready for owner merge` for effective head `9927be7`.

### The state of the build

- `make verify` passes at tip `8a041e3` with 806 tests, 0 failures, 0 warnings, and clean format, det-lint, STE, replay identity, content hash, and smoke checks.
- The revision-matched CI checks pass for all matrix legs and all stable gate jobs. `review-gate` waits for this review record.
- The remote branch head is `8a041e3`.

### What is in flight

The review record and this handoff entry need a commit and push. After the remote gate reads the record, the PR is ready for owner merge.

### Traps and gotchas

- The verdict targets effective head `9927be7`, not metadata tip `8a041e3`.
- The owner adds the three stable names to branch protection after the PR merges, as D-685 states.

### The questions that block progress

None.

### The next concrete action

Commit and push the review record and handoff entry. Then fetch and verify that the remote head and review-gate result match.

## Session 132: 2026-09-19, Claude Code

Author: Claude Code
Session: PR-88, the stable check names of the CI matrix jobs. Repository: the-thing-below. Branch: `fix/pr-88-ci-matrix-check-names`. Role: author. Base: `51a040f`.

### What this session did, and why

- Branch protection is live on `main`, and a required check matches by name. The three matrix jobs report two different name sets, so no name of a matrix job can be a required check (F-85, OQ-197).
- The session read the check runs of the head of PR #35 and the head of PR #36. The code PR gives three leg names for each family. The docs-only PR gives one check run with the literal name template.
- Each matrix job keeps its condition of D-595. A gate job of each family always runs and reports one stable name (D-682, D-683).
- Each gate job reads the result of `changed-paths` too, so a skip that no condition asked for fails the gate (T-2).
- `TheThingBelow.Tests/CiWorkflowGateTests.cs` holds the rule. Seven of its twelve rows fail on the workflow file before this PR.
- OQ-3 is closed, because the protection is live. The read of the protection endpoint gives the five checks of D-681.
- Gate 1 gains a line for the required-check set, and it moves to section 7.22 of the phase file (D-684, D-685).

### The state of the build

- `make verify` passes with 806 tests, 0 failures, 0 warnings, and clean format, det-lint, STE, replay identity, content hash, and smoke checks.
- The remote head of `main` is `51a040f`.

### What is in flight

The Codex review of PR #37. This PR changes `.github/workflows/`, so it is never exempt (D-185, D-560).

- The automated pass of head `9927be7` approved the code review and opened no thread. Its CI block named one fault of RG 7, and the answer is the comment of the PR.
- The fix is proven on the head. The check runs hold `build, test, and format`, `replay-identity`, and `smoke` as literal names, each `success`, beside the three leg names.
- `RG 3` faults, because the head holds no record at `docs/reviews/pr-37.md`. It passes when the review record lands.

### Traps and gotchas

- The owner adds `build, test, and format`, `smoke`, and `replay-identity` to the required checks of `main` after this PR merges. Each name first reports on this PR.
- `Gitar` stays unrequired. Its trial ends about 2026-09-23.
- The gate job reads `always()`. A gate that a condition skips would report Success and hide a red leg.

### The questions that block progress

None. D-682 to D-685 hold the four answers of this PR.

### The next concrete action

Codex reviews PR #37 and writes `docs/reviews/pr-37.md`. Then the owner adds the three names to the required checks of `main` after the merge (D-685).
