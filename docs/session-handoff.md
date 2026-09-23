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

- The Gitar comment repeats the OQ-231 mismatch. D-895 pauses Gitar answers.
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

- The PR waits for the review of the other provider (T-4). The gitar pause of D-895 holds.

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

- The PR waits for the review of the other provider (T-4). The gitar pause of D-895 holds.

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
- Then the PR waits for the review of the other provider (T-4). The gitar pause of D-895 holds.

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

## Session 210: 2026-09-22, Codex

Author: Codex
Session: reviewer PR #58. Repository: the-thing-below. Branch: `docs/pr-gitar-pause`. PR: #58. Role: reviewer. Base: `871624e`.

### What this session did, and why

- Reviewed the policy change and reversal procedure of PR #58.
- Confirmed that Claude Code authored the PR and that this review uses the other provider (T-4, D-17).
- Inspected all 11 changed paths. The effective head is `3239ba1`; the later commit changes only handoff metadata (D-589, D-610).
- Wrote `docs/reviews/pr-58.md` with no finding and verdict `Ready for owner merge` for effective head `3239ba1`.
- Corrected the PR Documents line for `docs/reviews/`.

### The state of the build

- The remote head before this review was `ab77727321783167d51915e873733b1f5c6220da`. The review metadata reached `origin` as `47cdd7553406eae34e98eb6a8a5f46929f5cca65`; the effective head is `3239ba1`.
- `make ste-check` passed with 0 findings. CI passed each applicable check, including review-gate. The docs-only matrix legs skipped as expected.

### What is in flight

- The review record and this entry were pushed. The owner can merge after reading the review record.

### Traps and gotchas

- The pause of D-895 makes a Gitar pass optional. It does not remove the review by the other provider.
- The Gitar dashboard approved the effective head with no issue. The pause of D-895 does not require a response.

### The questions that block progress

None.

### The next concrete action

The owner reads the review record and merges PR #58.

## Session 209: 2026-09-22, Claude Code

Author: Claude Code
Session: author of the gitar pause, PR #58. Repository: the-thing-below. Branch: docs/pr-gitar-pause. Role: author. Base: `871624e`.

### What this session did, and why

- The owner asked for a pause of the gitar pass, because the gitar subscription expires. The pause must be easy to reverse.
- Checked the machine rules first. Branch protection on `main` requires eight checks, and none of them is gitar. No workflow and no review-gate rule reads gitar. Thus the pause changes rule text alone.
- The owner answered three questions. A docs PR takes the label after ste-check is green. The branch name has no number. One PR ends the pause when the owner says that gitar is back.
- Added D-895, and a note of a revision in part on D-14, D-66, D-67, D-586, D-587, and D-705.
- Added a pause clause to `CLAUDE.md`, `AGENTS.md`, the PR template, three skills, and the session-context runbook. Each clause cites D-895.
- The runbook section "The end of the gitar pause" gives the steps that end the pause.

### The state of the build

- ste-check gives 0 findings. The PR changes docs alone, so CI skips the build jobs (D-595).
- The first push of PR #58 holds this entry. Base `871624e`.

### What is in flight

- PR #58 waits for the review of the other provider, because it adds and revises decision rows (D-401). No label applies.
- Gitar still ran. Its automatic pass approved `3239ba1` at 22:33:42 UTC with no comment and no thread. The dashboard edit came after the push at 22:32:13 UTC, so the pass is current.
- CI at `3239ba1`: every required check passes except review-gate. Review-gate faults on RG 3 alone, because the review record does not exist yet.

### Traps and gotchas

- `CLAUDE.md` was 1 byte under its 16 KB limit. The pause paragraph replaces the first paragraph of the review section, and the file is now 16,363 bytes.
- The end PR must not revert the register, the handoff, or the review records. The runbook steps restore them from `HEAD`.
- Before the review, the `docs/reviews/` line of the Documents section takes the "No change needed" form. A `Changed:` line gave an RG 7 fault.
- The PR title holds "pause the gitar pass", because the runbook finds the squash commit by that text.

### The questions that block progress

None.

### The next concrete action

The other provider reviews PR #58 and writes its review record. The author answers each finding.

# Session handoff
