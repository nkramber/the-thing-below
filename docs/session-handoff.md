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

## Session 219: 2026-09-23, Codex

Author: Codex
Session: reviewer PR-94, round 2. Repository: the-thing-below. Branch: `feat/pr-94-fog`. PR: #59. Role: reviewer. Base: `8d98c46`.

### What this session did, and why

- Re-reviewed the correction to P2-1 at effective head `2842f0e`.
- Verified that D-908 records the owner's coverage choice, resolves OQ-231, and matches the fixture and roadmap updates.
- Ran `make verify`, checked the 72 screen captures, and read the current map and battle fog frames.
- Updated `docs/reviews/pr-59.md` with the fixed finding, earlier verdict, current verdict, and repeat-review evidence.

### The state of the build

- The remote head of `main` is `8d98c46`; the remote PR head and effective head are `2842f0e`.
- `make verify` passed with 2,174 tests. The build, format, det-lint, ste-check, replay identity, content hash, atlas, and smoke checks passed.
- CI run 35803716443 passed the implementation checks, including screen-test, on the configured legs.
- Review-gate at `2842f0e` failed RG 4 and RG 5 because the review record still named its prior `Blocked` verdict and head `8011192`. This session updates both fields.

### What is in flight

- The updated review record and handoff entry need commit and push.
- The fresh review-gate result needs verification after the push.

### Traps and gotchas

- D-895 pauses Gitar replies. The Gitar check passed, and the current Gitar comment reports the stale review-gate fields.
- Bot and night-gate checks do not exist yet; PR-15 and PR-49 create them (G-16).

### The questions that block progress

None. D-908 resolves OQ-231.

### The next concrete action

Run `make where`, commit the review record and this handoff, push, then fetch and verify the remote head and review-gate.

## Session 218: 2026-09-22, Claude Code

Author: Claude Code
Session: author PR-94, round 7. Repository: the-thing-below. Branch: `feat/pr-94-fog`. PR: #59. Role: author. Base: `8d98c46`.

### What this session did, and why

- Committed the baselines of `map-fog-1x` and `battle-fog-1x` from the artifact of CI run 35803303503 at `205a8ee` (D-733). The two runs of that job matched on all 72 captures, and only the two fog frames differed from the old baseline.
- The final head of `docs/reviews/pr-59-response.md` names this commit.

### The state of the build

- The remote head of `main` is `8d98c46`. The PR head before this round is `205a8ee`, and the commit of this round is the effective head.
- CI on `205a8ee` passed each check except screen-test, on the two fog frames alone, and review-gate, which reads the `Blocked` verdict of `8011192`.

### What is in flight

- The PR waits for the repeat review of the other provider on the answer to P2-1 (T-4).

### Traps and gotchas

- None new.

### The questions that block progress

None.

### The next concrete action

The other provider reviews the answer to P2-1, and updates `docs/reviews/pr-59.md`.

## Session 217: 2026-09-22, Claude Code

Author: Claude Code
Session: author PR-94, round 6, the answer to the review. Repository: the-thing-below. Branch: `feat/pr-94-fog`. PR: #59. Role: author. Base: `8d98c46`.

### What this session did, and why

- Answered the review of session 216, which gave `Blocked` for head `8011192`. P2-1 has full merit: D-907 closed OQ-231, and it named no coverage (D-19).
- Asked the owner the coverage question again. The owner chose a little less coverage, and D-908 records it and resolves OQ-231. D-907 now answers no question.
- The wide banks of the fixture fog start at 4800 in place of 4300, and the smaller clouds start at 5400 in place of 5000.
- The author read `map-fog-1x` and `battle-fog-1x` from `make sheet`. Each frame shows more clear ground between the banks (D-784).
- Wrote `docs/reviews/pr-59-response.md`.

### The state of the build

- The remote head of `main` is `8d98c46`. The PR head before this round is `cd20431`, the metadata commit of the review.
- `make test` (2,174 tests), `make format`, `make lint`, `make identity`, `make content`, `make smoke`, and `make ste-check` pass on this machine.

### What is in flight

- The screen-test job gives new baselines for the two fog frames, and the author commits them from the artifact.
- Then the other provider reviews the correction again (T-4).

### Traps and gotchas

- An answer of the owner that names another subject than the question does not resolve the question. Ask the question again (D-19).

### The questions that block progress

None.

### The next concrete action

Commit the fog baselines from the CI artifact. Then the other provider reviews the correction.
