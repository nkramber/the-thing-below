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

- The Gitar check passed, and the current Gitar comment reports the stale review-gate fields.
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

## Session 216: 2026-09-22, Codex

Author: Codex
Session: reviewer PR-94, round 1. Repository: the-thing-below. Branch: `feat/pr-94-fog`. PR: #59. Role: reviewer. Base: `8d98c46`.

### What this session did, and why

- Reviewed the full diff from `8d98c46` to effective head `8011192`.
- Verified the provider gate. Claude Code authored the PR, and Codex reviewed it.
- Checked the fog reader, render path, shader, budget, contrast checks, tests, project records, CI, and screen captures.
- Wrote `docs/reviews/pr-59.md` with one P2 finding about OQ-231.

### The state of the build

- The remote head of `main` is `8d98c46`. The remote PR head is `c40c559`, and the effective head is `8011192`.
- `make verify` passed with 2,174 tests. All other local checks passed.
- CI run 35801129146 passed each implementation check on every leg. The review gate waits for the review record.
- All 72 screen captures match the committed baselines.
- Metadata CI run 35802108533 passed its applicable checks. It skipped the implementation matrix legs.
- Metadata review-gate run 35802107959 passed RG 1 to RG 3 and RG 5 to RG 8. RG 4 failed because this review has the required `Blocked` verdict.

### What is in flight

- OQ-231 needs the owner's coverage choice. The PR cannot close this question until the decision enters the records.
- The owner must answer OQ-231 before the review can approve the PR.

### Traps and gotchas

- The Gitar comment repeats the OQ-231 mismatch.
- The review commit changes only metadata paths, so the effective head stays `8011192`.

### The questions that block progress

OQ-231 asks whether fog coverage should decrease, stay the same, or increase. D-907 does not answer it.

### The next concrete action

The owner answers OQ-231. The author records the answer and updates the fog content if needed.

## Session 215: 2026-09-22, Claude Code

Author: Claude Code
Session: author PR-94, round 5. Repository: the-thing-below. Branch: `feat/pr-94-fog`. PR: #59. Role: author. Base: `8d98c46`.

### What this session did, and why

- The owner read the fog of round 4 and approved it: "Fog looks good" (D-622, D-623).
- Committed the baselines of `map-fog-1x` and `battle-fog-1x` from the artifact of CI run 35800692949 (D-733). The two runs of that job matched on all 72 captures, and only the two fog frames differed from the old baseline.

### The state of the build

- The remote head of `main` is `8d98c46`. The PR head before this round is `5b4ee61`, and the effective head is the commit of this round.
- CI on `5b4ee61` passed each check except screen-test, on the two fog frames alone, and review-gate, which waits for the review record.
- `screens --captures <artifact> --baseline screens/baseline` gives a match on all 72 captures.

### What is in flight

- The PR waits for the review of the other provider (T-4).

### Traps and gotchas

- None new. The entries of sessions 211 to 214 hold the traps of this PR.

### The questions that block progress

None.

### The next concrete action

The other provider reviews PR #59 and writes `docs/reviews/pr-59.md`.

## Session 214: 2026-09-22, Claude Code

Author: Claude Code
Session: author PR-94, round 4. Repository: the-thing-below. Branch: `feat/pr-94-fog`. PR: #59. Role: author. Base: `8d98c46`.

### What this session did, and why

- The owner read the soft fog of round 3 and asked for more of the pixel look of the art, between the soft fog and the bands of round 1. OQ-231 and D-907 record it, and D-900 and D-901 have a revision in part.
- `FogLayer` takes `steps` (2 to 8) and `cell_size` (1 to 8). The shader reads the noise at the north-west pixel of each block, and it rounds the fade down to its step, so a block below the first step stays clear.
- The capture fog takes 4 steps and blocks of 2 art pixels in each layer.
- The session read `map-fog-1x` and `battle-fog-1x` from `make sheet`. The fog keeps the cloud shapes of round 3, in blocks of 2 pixels and 4 hard steps of strength.

### The state of the build

- The remote head of `main` is `8d98c46`. The PR head before this round is `374aae8`.
- CI on `374aae8` passed each check except screen-test, on the two fog frames alone, and review-gate, which waits for the review record.
- `make test` (2,174 tests), `make format`, `make lint`, `make identity`, `make content`, `make smoke`, and `make ste-check` pass on this machine.

### What is in flight

- The screen-test job gives new baselines for the two fog frames, and the author commits them from the artifact.
- The owner reads the new fog. The steps, the block size, and the coverage are values of the content file.

### Traps and gotchas

- A change of a content file after the last build fails `TheEmbeddedSetMatchesTheFolderByBytes` until the next build (D-508).

### The questions that block progress

None.

### The next concrete action

Commit the fog baselines from the CI artifact. Then the owner reads the fog, and the other provider reviews PR #59.

## Session 213: 2026-09-22, Claude Code

Author: Claude Code
Session: author PR-94, round 3. Repository: the-thing-below. Branch: `feat/pr-94-fog`. PR: #59. Role: author. Base: `8d98c46`.

### What this session did, and why

- The owner refused the fog of round 1 and sent a reference picture of soft mist (F-102). The owner answered OQ-224 to OQ-230 (D-900 to D-906).
- D-900: the fog fades smoothly, and it alone leaves the hard-edge rule of G-27 and D-622. D-901: the fog draws at the pixel size of the art. D-902: the density is even. D-903: the fixture fog takes the pale key `L`. D-905: the thickest part is about 45 percent.
- D-904 set the floor of the fog test to 16. At 16, the pair of the fixture enemy with a gap of exactly 16 on `K` entered the test, and every fog failed. The session reported the fault, and D-906 supersedes D-904 with a floor of 17. A probe measured 45 percent as a pass and 47 percent as a fail.
- Core: `FogLayer` takes `from`, `to`, and `strength` in place of the bands, and it has no `cell_size`. `FogBand` is gone. `FogContrast.LeastGap` is 17.
- The shader reads a value noise of 5 octaves with a quintic curve, and a `smoothstep` fades each layer from `from` to `to`. The strongest layer wins at each pixel (D-899).
- The capture fog holds three layers of `L`: wide banks at 45 percent, smaller clouds at 35 percent, and wisps at 25 percent.

### The state of the build

- The remote head of `main` is `8d98c46`. The PR head before this round is `0a5a8c2`.
- `make test` (2,171 tests), `make format`, `make lint`, `make identity`, `make content`, `make smoke`, and `make ste-check` pass on this machine.
- The session read `map-fog-1x` and `battle-fog-1x` from `make sheet`. The fog shows soft pale clouds with no repeat and no hard band. The four `still` frames use the shipped dust, which holds no fog.

### What is in flight

- The screen-test job gives new baselines for the two fog frames again, and the author commits them from the artifact.
- The owner reads the new fog. The coverage of the three layers is a value of the content file, and the owner can ask for a change.

### Traps and gotchas

- The fog test skips a pair whose gap is below the floor with no fog (D-892). A floor at or below the gap of an art pair brings that pair into the test, and then any fog fails.
- The simulation version stays at 13, because this PR raised it already. The state hashes do not read the fog, so the identity file does not change.

### The questions that block progress

None.

### The next concrete action

Commit the fog baselines from the CI artifact. Then the owner reads the fog, and the other provider reviews PR #59.

## Session 212: 2026-09-22, Claude Code

Author: Claude Code
Session: author PR-94, round 2. Repository: the-thing-below. Branch: `feat/pr-94-fog`. PR: #59. Role: author. Base: `8d98c46`.

### What this session did, and why

- Committed the new baselines of `map-fog-1x` and `battle-fog-1x` from the artifact of CI run 35796366574 (D-733). The two runs of that job matched on all 72 captures, and only the two fog frames differed from the old baseline.
- Read the new `map-fog-1x` baseline. The software renderer of CI draws the same shapes as the renderer of this machine, so the integer hash gives one noise on both.

### The state of the build

- The remote head of `main` is `8d98c46`. The PR head before this round is `b3d451c`.
- CI on `b3d451c` passed build, test, and format, smoke, det-lint, replay-identity, and ste-check on every leg. The screen-test job failed on the two fog frames alone, and this round commits their baselines. The review-gate check fails until the review record lands.
- `screens --captures <artifact> --baseline screens/baseline` gives a match on all 72 captures.

### What is in flight

- The PR waits for the review of the other provider (T-4).

### Traps and gotchas

- `make sheet FIXTURE=<name>` deletes `artifacts/captures/` first, so a run of one fixture removes the frames of the others.

### The questions that block progress

None.

### The next concrete action

The other provider reviews PR #59 and writes `docs/reviews/pr-59.md`.

## Session 211: 2026-09-22, Claude Code

Author: Claude Code
Session: author PR-94, round 1. Repository: the-thing-below. Branch: `feat/pr-94-fog`. PR: #59. Role: author. Base: `8d98c46`.

### What this session did, and why

- The owner refused the fog of PR-58 in a play session, because the grid repeats over the view (F-101). The owner named a procedural fog of several layers as the next work.
- Filed OQ-220 to OQ-223, and the owner took each recommendation. D-896: PR-94 comes before PR-59. D-897: a noise shader replaces the text grid, and D-887 has a revision in part. D-898: one pass draws 1 to 3 layers, and the budget counts one pass for each fog. D-899: the strongest band wins where layers overlap.
- Core: `FogLayer` reads a key, 1 to 3 bands of `from` and `strength`, a scale, a block size, a seed, and a drift. `FogBand` is new. The reader refuses a fourth layer, and `AmbientEffect.FullScreenPasses` counts one pass for a fog.
- Game: `FogPass` replaces `FogSheets`. One `ColorRect` covers the view, and `fog.gdshader` or `fog_lit.gdshader` draws it. The two shaders include `fog_noise.gdshaderinc`, and they differ in the scene light alone.
- The shader reads two octaves of value noise at each world pixel, with an integer hash. Game gives each layer the world pixel of the view minus its drift at the tick, so the shader never reads `TIME` (F-100).
- The capture file of the fog holds three layers now. Tests cover the reader, the pass count, the uniform names, and the rule that no shader reads `TIME`.
- Raised the simulation version to 13, and wrote the identity file again (G-17, D-504).
- Updated `area-effects.md`, the PR-94 block of the phase 2 file, the design list and F-101, the budget comment, and the `csharp-conventions` skill.

### The state of the build

- The remote head of `main` is `8d98c46`. The branch holds one commit of round 1.
- `make build`, `make test` (2,173 tests), `make format`, `make lint`, `make atlas`, `make identity`, `make content`, `make smoke`, and `make ste-check` pass on this machine.
- `make sheet` wrote `map-fog-1x` and `battle-fog-1x` with the new fog, and the session read each frame. The four `still` frames use the shipped dust, which holds no fog, so this PR does not change them.

### What is in flight

- The PR waits for CI. The screen-test job then gives new baselines for `map-fog-1x` and `battle-fog-1x`, and the author commits them from the artifact.
- Then the PR waits for the review of the other provider (T-4).

### Traps and gotchas

- The old grid fog moved west for a positive `drift_x`, because the region of the sprite moved east. The new pass moves the shapes east for a positive value, as `FogLayer` states.
- A probe gave the shipped dust file the three layers for one `make sheet FIXTURE=still` run, then put the file back. In the four frames, the wisps of `drift_x: -4` moved 24 screen pixels west in 3 seconds, and the lit shader took the torch light.
- An unlit canvas item needs `render_mode unshaded`, which a uniform cannot switch. That is why two fog shaders exist.
- PR-57 and PR-58 changed Core readers and kept the simulation version at 12. This PR raised it to 13.
- The title `# Session handoff` sits below session 207, and not at the top of the file. This entry does not move it.

### The questions that block progress

None.

### The next concrete action

Take the new fog baselines from the CI artifact of the screen-test job, commit them, and ask the other provider for the review.

# Session handoff
