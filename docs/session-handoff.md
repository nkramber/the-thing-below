## Session 229: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #62, round 1. Repository: the-thing-below. Branch: `feat/pr-92-hd2d-passes`. PR: #62. Role: reviewer. Base: `05dcc3d`.

### What this session did, and why

- Reviewed the full diff from `05dcc3d` through effective head `238bafe`.
- Verified the provider gate. Claude Code authored the PR, and Codex reviewed it.
- Checked the readers, render path, shaders, effect budget, decisions, tests, CI, and all 74 screen captures.
- Added `docs/reviews/pr-62.md`. The verdict is blocked because the author has not answered the current Gitar comment.

### The state of the build

- `main` is `05dcc3d`. The remote PR head is `09f2281`, and the effective head is `238bafe`.
- The focused tests pass with 230 tests. CI run 35823669111 passes each implementation check on the configured legs.
- Metadata CI run 35825994991 passes its applicable checks. Review-gate run 35825994798 passes RG 1 to RG 3 and RG 5 to RG 8. RG 4 reads the required `Blocked` verdict.
- Gitar passes on `09f2281`; its dashboard still reports the RG 4 failure. The author has not replied.

### What is in flight

- The PR waits for the author to answer the Gitar comment, and for the next review of the response.

### Traps and gotchas

- The first Gitar pass flagged the absent review path. The latest pass reports the `Blocked` verdict under RG 4.
- OQ-241 asks which later PR adds ceiling shafts, and blocks nothing here.

### The questions that block progress

None. OQ-241 blocks no progress on PR #62.

### The next concrete action

The author answers the Gitar comment, then the other provider reviews the response.

## Session 228: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-92, round 2. Repository: the-thing-below. Branch: `feat/pr-92-hd2d-passes`. PR: #62. Role: author. Base: `05dcc3d`.

### What this session did, and why

- The owner read the fixture shafts and said that a beam from a bare wall makes no sense. D-924 keeps wall shafts only under an opening, such as a window (OQ-240).
- The owner kept the still beam. D-925 removes the shimmer, its fields, the shimmer shaft, and the stepped still captures, and `LightWave` went back into `GlowPass`.
- A shaft kind now needs a drawing, and the map draws it on the wall. The fixture dungeon holds one barred window at (6, 1), with `drawing.decor_fixture_window` in the atlas.
- The owner wants ceiling shafts later. OQ-241 asks which PR adds them.

### The state of the build

- `main` is `05dcc3d`. The tests, format, lint, identity, content, atlas, and smoke pass on this machine. The author read `make sheet` for the map fixture.
- CI run 35821954258 on `efbf7dd`: the two capture runs matched on all 74 captures. The 48 baselines that the window changed come from its artifact (D-733).
- Gitar approved `efbf7dd` with no finding. CI and gitar passed on `8b299b3`, with review-gate waiting for the review record.
- The owner turned on SSH on the Deck, and this session ran the sweep of `spike/deck-test` there at the owner's request. The results are `a6f9f0b` on that branch, and they hold the pass row of 6 (F-106).

### What is in flight

- The PR waits for CI on the baseline commit and the review of the other provider.

### Traps and gotchas

- The window sits on the brick face of the wall tile, rows 16 to 27. The cap above the face is rows 0 to 15.
- A shaft sprite draws unshaded, as a torch does, so the opening stays bright in the dark.
- Over SSH, the Deck takes no `.bashrc`, so set `DOTNET_ROOT`, `PATH`, and `GODOT` by hand, and `DISPLAY=:0` for the sweep. Godot then falls back to Wayland.
- The owner should stop SSH on the Deck after this PR: `sudo systemctl stop sshd`.

### The questions that block progress

None. OQ-241 blocks no PR yet.

### The next concrete action

The other provider reviews PR #62.

## Session 227: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-92, round 1. Repository: the-thing-below. Branch: `feat/pr-92-hd2d-passes`. PR: #62. Role: author. Base: `05dcc3d`.

### What this session did, and why

- Asked the questions of PR-92 and recorded OQ-233 to OQ-239 and D-917 to D-923. D-917, D-919, and D-923 revise D-622, D-916, and D-617 in part.
- Built the tilt-shift blur, the vignette, and the light shafts, each in a smooth mode and a stepped mode (D-917). `content/effects/hd2d.json` holds the mode.
- A shaft is a shaft kind in `content/decor/shafts/`, and a decor file places it on a wall (D-918). The fixture dungeon holds a still shaft and a shaft that shimmers (D-921).
- The shafts draw in the overlay. A scene view joins the world and the overlay, and the frame draws it with the blur and the vignette. The marks moved to a mark view above them (D-919).
- The budget counts the passes of each map, a map with no weather included, and the pass row is 6 (D-920, D-923). The simulation version is 15.
- Added the stages `pass-look`, `full-load-24-look`, and `budget-rows` to `spike/deck-test` as `6bb1595` (D-922).
- Added stepped captures of the map, a fight, and the still fixture. The author read the frames of `make sheet`.

### The state of the build

- `main` is `05dcc3d`. `make verify` passed on this machine except the 6 new baselines.
- CI run 35820532779 on `2d67459`: the two capture runs matched on all 78 captures. The 72 baselines that changed come from its artifact (D-733). The 5 ui captures and the picture capture did not change.
- Gitar approved `2d67459` with no finding.

### What is in flight

- The owner runs the Deck sweep of `spike/deck-test`. The pass row of 6 stands only when `full-load-24-look` and `budget-rows` hold 60 frames per second (G-14).
- The PR waits for CI on the baseline commit and the review of the other provider.

### Traps and gotchas

- Godot takes no default value for a uniform array. The spike copy of the shaft shader uses constants.
- Two walk fixtures can follow each other in the capture list, so the session rebuilds the run when the fixture changes.
- The view of the scene reads with a linear filter. The blur shader reads each sharp pixel at the middle of its art pixel.

### The questions that block progress

None. The Deck sweep is a measurement, not a question.

### The next concrete action

Read the Deck reports, commit them to `spike/deck-test`, and record the numbers in the PR. Then commit the baselines from the CI artifact.

## Session 226: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #61, round 1. Repository: the-thing-below. Branch: `feat/pr-59-glow`. PR: #61. Role: reviewer. Base: `430c8e9`.

### What this session did, and why

- Reviewed the complete diff from `430c8e9` to effective head `0d55d93`.
- Verified the provider gate. Claude Code authored the PR, and Codex reviewed it.
- Checked the glow reader, light bound, rendering path, overlay, budget, tests, decisions, and captures.
- Added `docs/reviews/pr-61.md` with no finding, and published the review record.

### The state of the build

- The remote head of `main` is `430c8e9`. Metadata commit `89a606a` reached the branch, and the effective head stays `0d55d93`.
- `make verify` passed with 2,221 tests. `make sheet` wrote all 72 captures.
- CI run 35815610824 passed every applicable implementation check on every leg. Gitar approved the effective head.
- Metadata run 35816940918 and review-gate run 35816940027 passed. The fresh gate passed RG 1 to RG 8.

### What is in flight

- PR #61 is ready for owner merge.

### Traps and gotchas

- The first review-gate run failed because this review record was not in the diff yet. The metadata run passed after the record reached the branch.
- The fog, hit bursts, and marks use the overlay view above the glow (D-916).

### The questions that block progress

None. OQ-102 and OQ-232 are resolved by D-910 and D-915.

### The next concrete action

The owner merges PR #61.

## Session 225: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-59, rounds 1 to 3. Repository: the-thing-below. Branch: `feat/pr-59-glow`. PR: #61. Role: author. Base: `430c8e9`.

### What this session did, and why

- Asked OQ-102, and the owner chose HDR 2D with a threshold and a smooth bloom, fire first (D-910 to D-912).
- Round 1 built HDR 2D, the bound of the lit art below the threshold, and a seed loop against the light curve of Game (F-103, F-104).
- The owner asked for the glow pass of our own, a stepped mode, and a pulse in place of the flicker. Round 2 built them (D-913, D-914, F-105).
- The owner compared both and kept HDR 2D at half intensity (D-915, OQ-232). The pulse stays on a glow rectangle of 8 by 8 over each wall torch flame.
- The owner said the fog must not glow. The fog, the hit bursts, and the marks now draw in an overlay view with no HDR 2D, above the glow (D-916). The reader refuses a lit fog.

### The state of the build

- `main` is `430c8e9`. The branch holds rounds 1 to 3. `make verify` passes on this machine, and `make sheet` wrote every capture with no error line.
- CI run 35815275241 on `6b612f2` passed every job but the baseline step of screen-test. Its two runs matched on all 72 captures. The 66 world baselines that changed come from its artifact (D-733).

### What is in flight

- PR #61 waits for CI on the baseline commit, gitar, and the review of the other provider.

### Traps and gotchas

- The glow of Godot averages its first step over about 8 by 8 art pixels. A smaller or dimmer source gives no glow.
- A view draws an item only when each parent shares its layer (F-105). `GlowPass.LiftAboveGlow` sets the layer on the node, its children, and its parents.
- An object initializer of a texture rect sets `ExpandMode` before `Size`.
- Do not redirect `make smoke` output into `artifacts/smoke.log`. The target writes that file itself, and the loop filled 20 GB.

### The questions that block progress

None. OQ-102 and OQ-232 are resolved.

### The next concrete action

Answer each gitar finding on PR #61, then hand the PR to the other provider.

## Session 224: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #60, round 3. Repository: the-thing-below. Branch: `docs/pr-60-revert-gitar-pause`. PR: #60. Role: reviewer. Base: `a20d32f`.

### What this session did, and why

- Re-reviewed PR #60 at effective head `bff8622`.
- The owner’s later instruction in the feedback says to keep the dated records. D-909 records this override and ends the pause.
- Verified that the PR #58 and PR #59 review records, and the PR #59 handoff entries, match `origin/main`. Sessions 209 and 210 remain in the archive.
- Marked P2-1 fixed and changed the current verdict to `Ready for owner merge`.

### The state of the build

- The remote head of `main` is `a20d32f`. The effective head of PR #60 is `bff8622`.
- `make ste-check` passed with 0 findings. CI passed all applicable implementation checks, and Gitar approved the head. Review-gate failed RG 4 and RG 5 because the review record still named the prior verdict and head.
- The updated review and this handoff need commit and push. A fresh review-gate result must be checked.

### What is in flight

- PR #60 waits for the review-gate result on the updated review record.

### Traps and gotchas

- The initial owner choice removed the dated records. The later choice kept them, and D-909 records that resolution.
- The effective head includes the restored decision and history files. Metadata commits do not change it.

### The questions that block progress

None.

### The next concrete action

Commit and push the repeat review. Then verify the branch head and fresh review-gate result.

## Session 223: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR #60, round 2. Repository: the-thing-below. Branch: `docs/pr-60-revert-gitar-pause`. PR: #60. Role: author. Base: `a20d32f`.

### What this session did, and why

- Answered P2-1 of `docs/reviews/pr-60.md` in `docs/reviews/pr-60-response.md`, with full merit.
- P2-1 conflicted with the first answer of the owner, so the session asked again. The owner chose to keep the dated records (D-19, D-24).
- Restored `docs/reviews/pr-58.md`, `docs/reviews/pr-59.md`, and each handoff entry to their text on `main`.
- Added D-909, which ends the pause. D-895 reads `Superseded by D-909`, and the six revised rows name D-909.
- Moved sessions 213 to 210 to the archive, to keep the 10 newest entries.
- Answered the gitar comment about RG 4.

### The state of the build

- The remote head of `main` is `a20d32f`. The PR head before this round is `1c6b4c7`, and the commit of this round is the effective head.
- `make ste-check` gives 0 findings. The rule files stay the same as at `8d98c46~1`.

### What is in flight

- PR #60 waits for the gitar pass on the new head and for the repeat review of the other provider.

### Traps and gotchas

- The text of session 220 names the removal of the records. Round 2 put them back, and the entry of session 220 stays as history.
- The finding cites D-10 for the dated-record rule. The rule is in the `ste-writing` skill and in `docs/runbooks/rename-and-move.md`.

### The questions that block progress

None.

### The next concrete action

Follow the `gitar-review` skill on the new head. Then the other provider repeats the review of PR #60.

## Session 222: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #60, round 2. Repository: the-thing-below. Branch: `docs/pr-60-revert-gitar-pause`. PR: #60. Role: reviewer. Base: `a20d32f`.

### What this session did, and why

- Read the checks and Gitar dashboard after review metadata head `503094f`.
- Confirmed RG 3 and RG 5 to RG 8 pass. RG 4 fails because the review verdict is `Changes required`.
- Updated `docs/reviews/pr-60.md` with the metadata-run result and current Gitar status.

### The state of the build

- The remote head of `main` is `a20d32f`. The effective head of PR #60 remains `aa1a10f`.
- CI passed each applicable check except review-gate RG 4. Gitar approved the code review. The latest dashboard reports RG 4, and the author has not answered that comment.
- The updated review and this handoff need commit and push.

### What is in flight

- PR #60 needs the author to restore the historical records and follow the end-of-pause decision procedure from the base runbook.
- The author also needs to answer the latest Gitar comment about RG 4.

### Traps and gotchas

- D-10 calls dated records history. Do not rewrite them to remove a decision that later changed.
- The PR #58 review and sessions 209 and 210 belong in the live records.
- RG 4 stays red while the review verdict is `Changes required`.

### The questions that block progress

None.

### The next concrete action

The author corrects P2-1 and answers the Gitar comment. Then this reviewer reassesses PR #60.

## Session 221: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #60. Repository: the-thing-below. Branch: `docs/pr-60-revert-gitar-pause`. PR: #60. Role: reviewer. Base: `a20d32f`.

### What this session did, and why

- Reviewed the reversion of the Gitar pause at effective head `aa1a10f`.
- Found that PR #60 deletes the PR #58 review and sessions 209 and 210. It also edits dated PR #59 records. D-10 says that a rewrite falsifies a dated record.
- Added `docs/reviews/pr-60.md` with finding P2-1 and verdict `Changes required`.

### The state of the build

- The remote head of `main` is `a20d32f`.
- At `aa1a10f`, `make ste-check` passed with 0 findings. Changed-path CI passed its listed checks, and the Gitar check passed. Review-gate failed RG 3 because the review record did not yet exist.
- The review and this handoff need commit and push. The effective head remains `aa1a10f`.

### What is in flight

- PR #60 needs the author to restore the historical records and follow the end-of-pause decision procedure from the base runbook.

### Traps and gotchas

- D-10 calls dated records history. Do not rewrite them to remove a decision that later changed.
- The PR #58 review and sessions 209 and 210 belong in the live records.

### The questions that block progress

None.

### The next concrete action

The author corrects P2-1. Then this reviewer reassesses PR #60.

## Session 220: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR #60. Repository: the-thing-below. Branch: `docs/pr-60-revert-gitar-pause`. PR: #60. Role: author. Base: `a20d32f`.

### What this session did, and why

- The owner told the session to undo PR #58, the pause of the gitar pass, and to put each file back as it was.
- Reverted the squash commit `8d98c46`. The four skills, `CLAUDE.md`, `AGENTS.md`, the PR template, and the runbook are the same as at `8d98c46~1`.
- Restored D-14, D-66, D-67, D-586, D-587, and D-705 to their text at `8d98c46~1`. Removed the pause row, its heading, and the review record of PR #58.
- Removed the handoff entries of sessions 209 and 210. Removed each citation of the pause row from the records of PR #59.
- Kept D-896 to D-908 of PR #59. They now continue the table of D-894.
- The owner chose to remove the records too, and not to add a row that ends the pause. Thus no decision row records this revert.

### The state of the build

- The remote head of `main` is `a20d32f`, the merge of PR #59.
- PR #58 changed no workflow and no branch protection rule. The eight required checks of `main` read no gitar result. Thus this PR changes no CI.

### What is in flight

- PR #60 waits for the gitar pass and for the review of the other provider. It changes decision rows, so the label of D-401 does not apply.

### Traps and gotchas

- The rules of D-14 and D-66 hold again. Each PR, this PR included, waits for the gitar pass and answers it.
- The session numbers 209 and 210 are now absent from both handoff files.
- Sessions 199 to 208 stay in the archive. The limit of 10 entries keeps them there.

### The questions that block progress

None.

### The next concrete action

Follow the `gitar-review` skill on PR #60. Then the other provider reviews PR #60.
