## Session 314: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR #82 (PR-105), round 3. Repository: the-thing-below. Branch: `fix/pr-105-save-drift`. PR: #82. Role: author. Base: `640ad98`.

### What this session did, and why

- The review of `0e71601` in `docs/reviews/pr-82.md` gave `Changes required` for P2-1: the conflict cell under the cursor kept the cursor color, against D-1119.
- The owner chose the look (D-1120): the warning color with an outline of 2 pixels in the cursor color. `SettingsMenu.LookOf` and `ShowSlot` apply it, and a test holds the four looks.
- The conflict capture now makes two conflicts, with the cursor on the cell of back. `docs/reviews/pr-82-response.md` answers the finding.

### The state of the build

- `make verify` passed at this head on the Mac. The effective head moves with this round.

### What is in flight

- The Gitar pass and the CI of this push. The `screen-test` job will flag `settings-conflict-1x`, and the next round takes the new frame as its baseline (D-733).

### Traps and gotchas

- The capture session of this Mac stops at the first 1080 frame, so read the conflict frame in the CI artifact.

### The questions that block progress

None for this PR.

### The next concrete action

Read the new conflict frame of the artifact, take it as the baseline, push, and run `make codex-review PR=82` after green CI.

## Session 313: 2026-09-25, Codex

Author: Codex
Session: reviewer PR #82 (PR-105), round 1. Repository: the-thing-below. Local branch: `review/pr-82`; PR branch: `fix/pr-105-save-drift`. Role: reviewer. Base: `640ad98`.

### What this session did, and why

- Reviewed effective head `0e71601` and inspected all 74 changed paths.
- P2-1 finds that the selected remap conflict stays yellow, but D-1119 requires each conflict cell to use the warning color.
- Verified the Gitar CI-analysis answer against the RG 3 log. The CI failure names the missing review record, which this session supplies.
- Updated the PR Documents row to name `docs/reviews/pr-82.md`.

### The state of the build

- `make verify` passed on macOS arm64: 3397 tests, format, lint, STE, identity, content, atlas, and smoke.
- CI passed the implementation checks at effective head `0e71601`. `review-gate` failed RG 3 before this record existed.

### What is in flight

- This review record and this handoff entry are committed together and pushed to `fix/pr-105-save-drift`.
- The current Gitar dashboard approves the head. Its CI-analysis item has the author answer, and no review thread stays open (D-964).
- The author must answer P2-1, correct the selected conflict color, and request a repeat review.

### Traps and gotchas

- The CI artifact shows the chosen Back binding in yellow and the conflicting Confirm binding in red. D-1119 requires the warning color on both conflict cells.
- Saves before format 14 lack a story step id and use the stored index, as D-1112 directs.

### The questions that block progress

None. P2-1 blocks approval. P3-9, P3-24, P3-25, P3-26, P3-36, and P3-37 remain outside PR-105.

### The next concrete action

The author answers P2-1 with the `pr-review` skill, then pushes one correction round for repeat review.

## Session 312: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR #82 (PR-105), round 2. Repository: the-thing-below. Branch: `fix/pr-105-save-drift`. PR: #82. Role: author. Base: `640ad98`.

### What this session did, and why

- Gitar approved the round 1 head `1cd661d` with no thread. Its CI analysis named the RG 3 fault of `review-gate`, and a PR comment answers it: the review record comes with the review of the other provider.
- Each CI check of `1cd661d` passed but `screen-test` and `review-gate`. The coverage job checked the SHA-512 of the ReportGenerator package (D-1116).
- `screen-test` found one changed frame, `settings-conflict-1x`, in 956 pixels. The author read the frame of the artifact: the confirm cell of the gamepad takes the warning color, the chosen cell of back stays yellow, and the line holds no count for one conflict (D-1119). This round takes that frame as the baseline (D-733).

### The state of the build

- The round 1 head is `1cd661d`. This round adds the new baseline and this entry.

### What is in flight

- The Gitar pass of this push, the CI of the new head, and then `make codex-review PR=82`.

### Traps and gotchas

- The baseline is outside the metadata set, so it moves the effective head, and it needs a new Gitar pass.

### The questions that block progress

None for this PR. P3-9, P3-24, P3-25, P3-26, P3-36, and P3-37 stay open in the report.

### The next concrete action

When every check but `review-gate` passes, run `make codex-review PR=82` in the background.

## Session 311: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR #82 (PR-105), round 1. Repository: the-thing-below. Branch: `fix/pr-105-save-drift`. PR: #82. Role: author. Base: `640ad98`.

### What this session did, and why

- The owner assigned the open findings of the repository review of 2026-09-24, with no limit of one concern (D-1109). The owner answered each question of P2-3, P3-22, P3-4, P3-33, and P3-23 (D-1110 to D-1116, D-1119).
- A save of another build follows an edit of a map or a story scene, and a party rule change refuses it (D-1110 to D-1113). Each story step takes an id, and save format 14 and record format 4 hold it.
- A wipe reloads a save of its own run alone (D-1114). `GameRun.Save` bounds the run record (D-1115). The coverage job checks its package (D-1116).
- After six findings, the owner asked for three more: det-lint (D-1117), the draw checks (D-1118), and the remap conflicts (D-1119).
- P3-35 and P3-39 needed no answer: a steal checks its remainder, each damage cut checks its overflow, and stale marks and comments match the code.

### The state of the build

- The simulation version is 29, and the identity file changed in 9 runs. `make verify` passed on the Mac: 3397 tests, format, det-lint, the STE check, identity, content, atlas, and smoke.
- The remote head is `640ad98` until the first push of this branch.

### What is in flight

- The first push, the Gitar pass, and then `make codex-review`.

### Traps and gotchas

- The capture session of this Mac stops at the first 1080 frame, because the display is smaller. Read the conflict frame in the artifact of the CI screen-test job.
- `settings-conflict-1x` changes: the confirm cell takes the warning color. Take the new baseline from that artifact (D-733).

### The questions that block progress

None for this PR. P3-9, P3-24, P3-25, P3-26, P3-36, and P3-37 stay open in the report.

### The next concrete action

Push the branch, open the PR, and follow the `gitar-review` skill.

## Session 310: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR #81 (PR-104), round 3. Repository: the-thing-below. Branch: `fix/pr-104-boot-and-rules`. PR: #81. Role: author. Base: `4aad522`.

### What this session did, and why

- The repeat review gave `Blocked` for one Gitar item: the CI analysis of the RG 4 faults, which each `Blocked` record causes. The PR comment answers each review-gate job of the analysis.
- The third review gives `Ready for owner merge` for the effective head `f66e314` in `docs/reviews/pr-81.md`, with no open finding. Each check of the record commit `e278b02` passed, `review-gate` included.
- The report of the owner marks the nine findings of this PR as `COMPLETE - PR #81`: P2-2, P2-5, P3-1, P3-7, P3-18, P3-19, P3-20, P3-27, and P3-34.
- The owner confirmed the merge after the summary in four sections (D-933, D-942).

### The state of the build

- The effective head is `f66e314`, and the review approves it. This entry is a commit of the metadata set, so the approval stands (D-610).

### What is in flight

- The Gitar pass of this commit, and then the gated auto-merge.

### Traps and gotchas

- Each `Blocked` record makes `review-gate` fail at RG 4, and the CI analysis of Gitar then names that fault. Answer it on the PR before the next review.

### The questions that block progress

None for this PR. P2-3, P3-4, P3-8, P3-9, P3-17, P3-22 to P3-26, P3-33, P3-35 to P3-37, and P3-39 stay open in the report.

### The next concrete action

After the merge, write the transitional prompt of step 6 of the `one-pr-one-session` skill.

## Session 309: 2026-09-25, Codex

Author: Codex
Session: reviewer PR #81 (PR-104), round 3. Repository: the-thing-below. Local branch: `review/pr-81`; PR branch: `fix/pr-104-boot-and-rules`. PR: #81. Role: reviewer. Base: `4aad522`.

### What this session did, and why

- Re-reviewed PR #81 at effective head `f66e314`.
- Verified the Steam Deck measurement and each author answer to the Gitar dashboard and CI analysis items.
- The CI analysis names three review-gate jobs. Each fails RG 4 because the review record says `Blocked`.
- The implementation checks pass, and the review record now gives `Ready for owner merge`.

### The state of the build

- Effective head `f66e314`. Metadata head `c18ed2f` has green implementation checks, STE, and Gitar. The review-gate job fails RG 4 because the prior record says `Blocked`.

### What is in flight

- This review record and handoff entry will be committed together and pushed to `fix/pr-104-boot-and-rules`.

### Traps and gotchas

- RG 4 correctly rejects the earlier `Blocked` verdict. The next gate run must read this updated record.

### The questions that block progress

None.

### The next concrete action

Commit and push the review metadata, then verify the remote head and review-gate result.

## Session 308: 2026-09-25, Codex

Author: Codex
Session: reviewer PR #81 (PR-104), round 2. Repository: the-thing-below. Local branch: `review/pr-81`; PR branch: `fix/pr-104-boot-and-rules`. PR: #81. Role: reviewer. Base: `4aad522`.

### What this session did, and why

- Re-reviewed PR #81 at effective head `f66e314`.
- Verified the owner Steam Deck measurement in the author response and the prior Gitar dashboard answer.
- The Gitar CI analysis claim names review-gate jobs. The current log shows only RG 4 fails because the review verdict is `Blocked`.
- The review remains `Blocked` until the author answers this Gitar item (D-964).

### The state of the build

- Effective head `f66e314`. Metadata head `5297e6c` has green implementation checks, STE, and Gitar. `review-gate` fails RG 4 because the verdict is `Blocked`.

### What is in flight

- This review record and handoff entry are committed and pushed to `fix/pr-104-boot-and-rules`.
- The author must answer the Gitar CI analysis item before a repeat review.

### Traps and gotchas

- The Gitar CI analysis calls the RG 4 result a validation fault. The log shows the gate correctly rejects the current `Blocked` verdict.

### The questions that block progress

None. The author answer to the Gitar item is required evidence under D-964.

### The next concrete action

The author answers the Gitar CI analysis item, then requests a repeat review.

## Session 307: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR #81 (PR-104), round 2. Repository: the-thing-below. Branch: `fix/pr-104-boot-and-rules`. PR: #81. Role: author. Base: `4aad522`.

### What this session did, and why

- The screen test of `830f146` found three fight captures that moved: the enemies read the lessons of Marrek, so the fight ends on another tick (D-1101). The author read each frame, and `f66e314` takes them as new baselines (D-733, D-784).
- The owner asked for the Deck run of D-961. On the Steam Deck in Release, one enemy turn took a p95 of 128 us and 120 us in two runs, below the limit of 1000 us.
- Gitar approved `f66e314` with no thread. Each Gitar item has its answer on the PR.
- The review of the other provider found no defect and gave `Blocked` for the Deck run and a Gitar note. `docs/reviews/pr-81-response.md` answers both.

### The state of the build

- Every check of `f66e314` passed but `review-gate`, which waited for the record. The effective head is `f66e314`.

### What is in flight

- The repeat review through `make codex-review PR=81`.

### Traps and gotchas

- A rule change of the evaluator moves the fight captures, and the screen test then fails until new baselines land.
- The review commit dropped the title line of `docs/session-handoff-archive.md`, as on PR #80. This commit puts it back.

### The questions that block progress

None.

### The next concrete action

Read the outcome of the repeat review.

## Session 306: 2026-09-25, Codex

Author: Codex
Session: reviewer PR #81 (PR-104), round 1. Repository: the-thing-below. Local branch: `review/pr-81`; PR branch: `fix/pr-104-boot-and-rules`. PR: #81. Role: reviewer. Base: `4aad522`.

### What this session did, and why

- Reviewed all 50 changed paths from merge base `4aad522` through effective head `f66e314`.
- Verified the changed battle captures against the CI artifact. No visual fault appeared.
- `make verify` passed locally with 3340 tests. The implementation CI checks passed at `f66e314`.
- The review record gives `Blocked`: D-961 requires an owner run of `evaluator-cost` on the Steam Deck before merge, and one Gitar dashboard finding has no author answer (D-964).

### The state of the build

- Effective head `f66e314`. CI run `36154860584` passed the implementation checks. Metadata run `36156272679` passed its applicable checks. The live `review-gate` passes RG 3, RG 5 to RG 8, and fails RG 4 because the verdict is `Blocked`.

### What is in flight

- The review record and this handoff entry are committed together and pushed to `fix/pr-104-boot-and-rules`.
- The owner Steam Deck measurement and the author answer to the Gitar dashboard finding remain pending.

### Traps and gotchas

- D-1099 accepts replacement of an older refused settings file. The author still needs to answer Gitar's matching dashboard item (D-964).

### The questions that block progress

None. The Steam Deck measurement and the Gitar answer are required evidence, not open design questions.

### The next concrete action

The author answers the Gitar item and the owner runs `evaluator-cost` on the Steam Deck. Then request a repeat review.
## Session 305: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR-104, round 1. Repository: the-thing-below. Branch: `fix/pr-104-boot-and-rules`. PR: the one PR intent of this branch, before GitHub gives a number. Role: author. Base: `4aad522`.

### What this session did, and why

- The owner answered each question of the findings in this PR (D-1098 to D-1108). G-8 does not bind this PR (D-1098).
- P2-2: a settings file that fails to load goes aside as settings.refused.json, and the start runs on the defaults with a message. A crash before the screen shows its message on a frame of the default display.
- P2-5: the reply of the evaluator scores the best legal strike, lesson strikes included. The cost fixture holds lessons.
- P3-1, P3-18, P3-19, P3-20: the enemy phase has a bound, six states fail at load or resume, a tile trigger plays before a step into an enemy, and a step into a marking enemy takes the side of the beat.
- P3-7 and P3-34: the crash message names its folder, and the absorb lines take the words of the owner.
- P3-27: G-17 states that a reader of Core counts.
- The simulation version rises to 28, and the identity file changes with it.

### The state of the build

- `make verify` passed on this Mac: 3340 tests, format, lint, the STE check, identity, content, atlas, and smoke.
- Each new regression test failed on `4aad522`, and the two lock tests of P3-1 hung there.

### What is in flight

- The first push, then the Gitar pass, the Deck run of `evaluator-cost` (D-961), and the review of the other provider.

### Traps and gotchas

- A collection expression of a `List` in Core reads `CollectionsMarshal`, and the reference test of G-1 fails. Use a collection initializer.
- The settings message names the whole file for a value outside its range, because the range check keeps no field.

### The questions that block progress

None for this PR. P2-3, P3-9, and P3-35 stay open in the report of the owner.

### The next concrete action

Push, then run the Gitar poll of the `gitar-review` skill.

