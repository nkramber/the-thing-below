## Session 325: 2026-09-26, Codex

Author: Codex
Session: reviewer PR #84 (PR-14). Repository: the-thing-below. Local branch: `review/pr-84`; PR branch: `feat/pr-14-hub`. PR: #84. Role: reviewer. Base: `c7e6191`.

### What this session did, and why

- Reviewed effective head `6c7837f` and wrote `docs/reviews/pr-84.md`.
- Verified the Gitar CI-analysis answer: RG 3 alone failed because the review record was absent. The author answered the item, and the CI log confirms the cause (D-964).
- `make verify` passed with 3,803 tests. Inspected the CI screen captures for the hub, Rest, Save, Party, and main-list frames (D-784).
- Corrected the PR Documents row to name `docs/reviews/pr-84.md`.

### The state of the build

- The effective head is `6c7837f`. The metadata tip is `ba92a31`, and its fresh `review-gate` check passed. All implementation checks passed on the effective head.

### What is in flight

- This entry and the review record are published on `feat/pr-14-hub`. The fresh Gitar pass has no item, and the live `review-gate` passed.

### Traps and gotchas

- `make sheet` stops on this Mac at `map-fill-1080`, because the screen gives 1920 by 955. Read the frames of the CI artifact.

### The questions that block progress

OQ-249 remains open for PR-17.

### The next concrete action

Verify the final metadata push, then end this review session.

## Session 324: 2026-09-26, Claude Code

Author: Claude Code
Session: author PR #84 (PR-14), round 3. Repository: the-thing-below. Branch: `feat/pr-14-hub`. PR: #84. Role: author. Base: `c7e6191`.

### What this session did, and why

- Gitar approved `ac14bc2`. The screen compare of `ac14bc2` matched 134 frames and missed the 4 baselines of `menu-rest` and `menu-save` alone.
- The author read those 4 frames of the artifact (D-784). The lead faces the keeper under the Rest window, and the lead faces the waystone under the Save window, with no clip. This round commits the 4 baselines (D-733).

### The state of the build

- The head of this push holds every baseline. Each check but `review-gate` waits for this push.

### What is in flight

- The checks and the Gitar pass of this push, then `make codex-review PR=84` (D-926).

### Traps and gotchas

- `make sheet` stops on this Mac at `map-fill-1080`, because the screen gives 1920 by 955. Read the frames of the CI artifact.

### The questions that block progress

None. OQ-249 blocks PR-17.

### The next concrete action

When each check but `review-gate` passes and Gitar completes, run `make codex-review PR=84` in the background.

## Session 323: 2026-09-26, Claude Code

Author: Claude Code
Session: author PR #84 (PR-14), round 2. Repository: the-thing-below. Branch: `feat/pr-14-hub`. PR: #84. Role: author. Base: `c7e6191`.

### What this session did, and why

- Gitar approved `609e122` with no thread. Its CI analysis named one `review-gate` fault, RG 3, the absent review record, and a PR comment answers it.
- The `screen-captures` artifact of `609e122` differed in 25 frames. The author read the hub frames, the Rest and Save frames, and the menu frames (D-784). The 9 menu frames differ by the same pixel count, which is the Save line of the main list (D-1143).
- The Rest and Save captures drew their window with the lead at the spawn. `d03a305` walks the lead to face the keeper and the waystone, and it sends a real confirm.
- This round commits 21 baselines from that artifact: the 3 hub frames and the 18 menu frames (D-733).

### The state of the build

- The head of this push holds the work. The 4 baselines of `menu-rest` and `menu-save` wait for the artifact of this push.

### What is in flight

- The artifact of this push gives the 4 last baselines. Then the Gitar pass, and `make codex-review PR=84`.

### Traps and gotchas

- `make sheet` stops on this Mac at `map-fill-1080`, because the screen gives 1920 by 955. Read the frames of the CI artifact.

### The questions that block progress

None. OQ-249 blocks PR-17.

### The next concrete action

Read the `menu-rest` and `menu-save` frames of the artifact of this push, and commit their 4 baselines.

## Session 322: 2026-09-26, Claude Code

Author: Claude Code
Session: author PR-14, round 1. Repository: the-thing-below. Branch: `feat/pr-14-hub`. PR: the one PR of PR-14, which this push opens. Role: author. Base: `c7e6191`.

### What this session did, and why

- The owner confirmed the scope of PR-14 and answered 18 questions, D-1131 to D-1148. The roadmaps, the design, the glossary, `docs/world/cast.md`, and `docs/runbooks/dev-machine.md` follow them. OQ-249 holds the experience of the reserve on screen, and it blocks PR-17.
- Core: the map kind, the NPCs and their movement, the solid service point, the services with a condition, the confirm rule, the rest, the save request, the reserve and the swap anywhere outside a fight, the NPC as a story scene actor, the walk home, and the entry to a map with the autosave of a hub.
- Debug: the `goto <map id>` command (D-1133). Game: the NPCs on the map screen, the service windows, the reserve in the Party window, the save writes, and no save entry in the main list (D-1143).
- Versions: simulation 31, save format 15, record format 5.

### The state of the build

- The head of this push holds the work. Each local check passes except 7 baseline cases: `hub-1x`, `hub-fill-800`, `hub-fill-1080`, and `menu-rest` and `menu-save` at `-1x` and `-fill-1080`.
- The main list lost a line, so each capture of a menu window changes, such as `menu-list` and `menu-party`. The CI compare names each one.

### What is in flight

- The `screen-captures` artifact of the CI screen-test job gives each new and changed baseline (D-733). Read each frame, and commit the baselines.
- Then the Gitar pass, and the review of `make codex-review`.

### Traps and gotchas

- `make sheet` crashes on this Mac at `map-fill-1080`: the screen gives 1920 by 955, not 1920 by 1080. Read the frames of the CI artifact.
- A `goto` resets the walked tiles and the dead enemies of the map that the party leaves. PR-35 owns the memory of each map.
- The fixture hub holds no story trigger, because Game sends no `story_step_end` before PR-36.

### The questions that block progress

None. OQ-249 blocks PR-17.

### The next concrete action

Take the baselines from the `screen-captures` artifact, read each frame, and commit them. Then run the Gitar poll of the `gitar-review` skill.

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
