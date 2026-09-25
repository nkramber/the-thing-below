## Session 321: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR #83 (PR-106), round 3. Repository: the-thing-below. Branch: `fix/pr-106-gate-trust`. PR: #83. Role: author. Base: `d875322`.

### What this session did, and why

- The review of `make codex-review` gives `Ready for owner merge` for the effective head `b93b796` in `docs/reviews/pr-83.md`, with no open finding.
- Each check of `b93b796` passed, and the `review-gate` check waited for the record alone. Gitar approved the head with no thread.
- The report of the owner marks the six findings of this PR as `COMPLETE - PR #83`: P3-9, P3-24, P3-25, P3-26, P3-36, and P3-37. No finding of the report stays open.

### The state of the build

- The effective head is `b93b796`, and the review approves it. This entry is a commit of the metadata set, so the approval stands (D-610).

### What is in flight

- The Gitar pass of this commit, the `review-gate` check, and the merge confirmation of the owner (D-933).
- The owner sets the `Gitar` context in the live protection of `main` (D-1123).

### Traps and gotchas

- The live `review-gate` check of this PR runs the old workflow of `main` (F-37). The first-parent list reads its first PR after the merge.

### The questions that block progress

None. OQ-246 holds the cause of the screen flake.

### The next concrete action

After the merge, write the transitional prompt of step 6 of the `one-pr-one-session` skill.

## Session 320: 2026-09-25, Codex

Author: Codex
Session: reviewer PR #83 (PR-106). Repository: the-thing-below. Local branch: `review/pr-83`; PR branch: `fix/pr-106-gate-trust`. PR: #83. Role: reviewer. Base: `d875322`.

### What this session did, and why

- Reviewed the full change at effective head `b93b796` and wrote `docs/reviews/pr-83.md`.
- Verified the Gitar CI-analysis answer. RG 3 alone failed because the review record was not on the PR before this review (F-37, D-964).
- `make verify` passed with 3,492 tests. CI passed the implementation checks on all three systems.
- Inspected all 127 frames in the CI artifact. The new Deck, body-24, settings, and crash frames show no visual fault.

### The state of the build

- The remote head before this metadata commit is `b93b796`. The local build and implementation CI pass at that head.

### What is in flight

- This review record and this entry are committed together and pushed to `fix/pr-106-gate-trust`.
- The review gives `Ready for owner merge` for `b93b796`. The live `review-gate` check must read the published record.

### Traps and gotchas

- The `review-gate` check fails RG 3 before the review record reaches the PR. RG 4 and RG 5 then skip (F-37).
- The Gitar code approval has no item. The CI-analysis item has the author's answer, and needs no further reply (D-964).

### The questions that block progress

OQ-246 remains open for the cause of a one-level screen-test flake. PR-106 adds diagnostics and package pins (D-1080, D-1127).

### The next concrete action

Read the live `review-gate` result after the metadata push, then end this review session.

## Session 319: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR #83 (PR-106), round 2. Repository: the-thing-below. Branch: `fix/pr-106-gate-trust`. PR: #83. Role: author. Base: `d875322`.

### What this session did, and why

- The first CI run of `5839bf8` held the 19 new frames with no baseline. The author read each frame of the artifact, and each one holds its text (D-784).
- The shared light texture of D-1129 moved 15 lit frames by one or two levels. A rerun on another CPU gave the same bytes in all 127 frames, so the 15 frames take new baselines too. OQ-246 records the evidence.
- The Gitar pass approved with no thread. Its CI analysis found that `ReviewGateMergeTests` could not remove the read-only object files of git on Windows. The test now clears the flag first.
- D-1123 now says that the owner sets the `Gitar` context of the live protection.

### The state of the build

- The remote head is the push of this round. All 127 captures match the baseline on the Mac compare of the artifact.

### What is in flight

- The Gitar pass of this push, then `make codex-review PR=83`.
- The owner sets the `Gitar` context in the live protection of `main` (D-1123).

### Traps and gotchas

- A change that shares a light texture moves lit frames by one level. The report of D-1127 names each one.

### The questions that block progress

None. OQ-246 holds the cause of the screen flake.

### The next concrete action

Answer the items of the Gitar CI analysis on the PR, then run `make codex-review PR=83` when CI is green.

## Session 318: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR-106, round 1. Repository: the-thing-below. Branch: `fix/pr-106-gate-trust`. PR: the one PR of PR-106. Role: author. Base: `d875322`.

### What this session did, and why

- The owner put the six open findings of the repository review of 2026-09-24 in one PR (D-1121). G-8 stands for every other PR.
- P3-9: both tools list the commits of the first parent, a merge lists its paths, and a record names the full hash (D-1125). A settings file of `.claude/` after an approval needs a new review (D-1122). `Gitar` joins the required checks (D-1123), and no stamp goes into a record (D-1124).
- P3-37: the map draws the start of the tick while the world holds (D-1126).
- P3-24: the remap screen names the key of the layout through a string id (D-1128).
- P3-25: the screen compare names each step of one level, and the job pins the loader and Xvfb and logs its CPU (D-1127).
- P3-26: both sessions run a planted crash, and 19 new captures add body 24 and 1280 by 800 (D-1130).
- P3-36: the budgets count the flicker and the spell burst, and each light texture builds one time (D-1129). The simulation version rises to 30.

### The state of the build

- On the Mac, build, format, lint, identity, content, atlas, and smoke pass. `make test` fails 19 cases of `TheBaselineHoldsThisCapture`, one for each new capture with no baseline.
- The remote head is the push of this round.

### What is in flight

- The baselines of the 19 new captures come from the `screen-captures` artifact of the first CI run. The author reads each frame before the commit (D-784).
- The owner sets the `Gitar` context in the live protection of `main` (D-1123).

### Traps and gotchas

- The live `review-gate` check of this PR runs the workflow of `main`, so it reads the old commit list (F-37).
- A worktree of an agent under `.claude/worktrees/` is a git repository. The local exclude file of this checkout lists the folder.
- The headless display writes an error line on a read of the keyboard layout, so the remap screen reads the physical key there.

### The questions that block progress

None. OQ-246 holds the cause of the screen flake.

### The next concrete action

Take the 19 baselines from the artifact, read each frame, commit them, and push. Then run the Gitar pass and `make codex-review`.

## Session 317: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR #82 (PR-105), round 5. Repository: the-thing-below. Branch: `fix/pr-105-save-drift`. PR: #82. Role: author. Base: `640ad98`.

### What this session did, and why

- The repeat review gives `Ready for owner merge` for the effective head `e17c964` in `docs/reviews/pr-82.md`. P2-1 is fixed in `af1d924` (D-1120).
- Each check of the record commit `74abf43` passed, `review-gate` included, and Gitar approved it with no item.
- The report of the owner marks the nine findings of this PR as `COMPLETE - PR #82`: P2-3, P3-4, P3-8, P3-17, P3-22, P3-23, P3-33, P3-35, and P3-39.
- The owner confirmed the merge after the summary in four sections (D-933, D-942).

### The state of the build

- The effective head is `e17c964`, and the review approves it. This entry is a commit of the metadata set, so the approval stands (D-610).

### What is in flight

- The Gitar pass of this commit, and then the gated auto-merge.

### Traps and gotchas

- A stale record makes RG 4 and RG 5 fail, and the CI analysis of Gitar names them. Answer each one on the PR before the next review.

### The questions that block progress

None for this PR. P3-9, P3-24, P3-25, P3-26, P3-36, and P3-37 stay open in the report.

### The next concrete action

After the merge, write the transitional prompt of step 6 of the `one-pr-one-session` skill.

## Session 316: 2026-09-25, Codex

Author: Codex
Session: reviewer PR #82 (PR-105), round 2. Repository: the-thing-below. Local branch: `review/pr-82`; PR branch: `fix/pr-105-save-drift`. PR: #82. Role: reviewer. Base: `640ad98`.

### What this session did, and why

- Re-reviewed the P2-1 correction at effective head `e17c964`.
- The selected conflict cell now uses the warning color and a 2-pixel cursor outline (D-1119, D-1120). The screen-test artifact shows two conflicts and the line `1 of 2`.
- The Gitar CI-analysis item has its answer. Its log names only stale RG 4 and RG 5, which this record replaces (D-964).

### The state of the build

- `make verify` passed on macOS arm64: 3401 tests, format, lint, STE, identity, content, atlas, and smoke. CI implementation checks passed at `e17c964`.
- This metadata commit updates the verdict and this entry. The remote head before the commit is `e17c964`.

### What is in flight

- This record and this handoff entry are committed together and pushed to `fix/pr-105-save-drift`.
- The record gives `Ready for owner merge` for `e17c964`. The owner can merge when `review-gate` reads the record and passes.

### Traps and gotchas

- The `review-gate` failure at RG 4 and RG 5 reads the old review record. The new metadata commit supplies the updated verdict and effective head.

### The questions that block progress

None for this PR.

### The next concrete action

Read the new `review-gate` result after the metadata push. The owner can merge when it passes.

## Session 315: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR #82 (PR-105), round 4. Repository: the-thing-below. Branch: `fix/pr-105-save-drift`. PR: #82. Role: author. Base: `640ad98`.

### What this session did, and why

- Gitar approved `af1d924` with no thread. Its CI analysis named `review-gate`, and a PR comment answers it: RG 4 and RG 5 read the record of `0e71601`, which the next review replaces.
- Each CI check of `af1d924` passed but `screen-test` and `review-gate`. `screen-test` found `settings-conflict-1x` alone, in 2054 pixels, and its two runs matched.
- The author read the frame of the artifact: four red cells for two conflicts, the cell of back in red with a yellow outline, and the line "1 of 2" (D-1119, D-1120). This round takes the frame as the baseline (D-733).

### The state of the build

- `make verify` passed at `af1d924` on the Mac. This round adds the baseline and this entry.

### What is in flight

- The Gitar pass and the CI of this push, and then `make codex-review PR=82`.

### Traps and gotchas

- The baseline moves the effective head, so it needs its own Gitar pass.

### The questions that block progress

None for this PR.

### The next concrete action

When every check but `review-gate` passes, run `make codex-review PR=82` in the background.

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

