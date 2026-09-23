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

## Session 208: 2026-09-22, Codex

Author: Codex
Session: reviewer PR-58, PR #57. Repository: the-thing-below. Branch: `feat/pr-58-ambient`. Role: reviewer. Base: `3f9ea43`.

### What this session did, and why

- Reviewed the full diff from `3f9ea43` to effective head `9d57dc0` (D-589).
- Verified the cross-provider gate. Session 207 names Claude Code as author, and this session reviews as Codex (T-4, D-17).
- Traced ambient content, fog contrast, weather motion, particle nodes, torch light, capture timing, wall shadows, and their tests (D-187, D-852, D-885 to D-894, F-97 to F-100).
- Viewed the 72-frame CI screen-test artifact. No visual fault was found (D-784).
- Wrote `docs/reviews/pr-57.md` with no finding and verdict `Ready for owner merge` for effective head `9d57dc0`.

### The state of the build

- `make verify` passed with 2,162 tests and no failure.
- CI run 35776490164 passed every implementation check at `9d57dc0`. The review-gate run failed only because the review record did not exist yet.
- The remote head before the review commit is `9d57dc0`.

### What is in flight

- The review record and this handoff entry need commit and push.
- The PR description needs the corrected Documents line for `docs/reviews/`.
- Fetch and verify the remote head and the new review-gate result.

### Traps and gotchas

- PR #57 is roadmap PR-58, on branch `feat/pr-58-ambient`.
- The two absent jobs, bot and night-gate, have creator lines under G-16 in the PR description.
- The review record uses the effective head. The metadata commit does not move it (D-610).

### The questions that block progress

None.

### The next concrete action

Commit and push the review record and this handoff entry. Correct the PR Documents line, then fetch and verify the remote head and review-gate check.

## Session 207: 2026-09-22, Claude Code

Author: Claude Code
Session: author PR-58. Repository: the-thing-below. Branch: `feat/pr-58-ambient`. Role: author. Base: `3f9ea43`.

### What this session did, and why

- Fixed the torch light that climbed a passage. A wall beside a doorway takes an L shape now: its band, and a column of 2 pixels to the south edge of the tile (D-852). The owner approved this second concern, and it takes no decision row.
- Asked the owner OQ-101 and seven more questions. D-885 to D-892 hold the answers.
- Built the ambient files: the four kinds, the weather of each map, the fog above the figures, and its contrast test (D-187, D-885, D-887).
- Gave each wall torch and the carried light a flame, embers, smoke, and a light of steps (D-890, D-891).
- Added the row of full-screen passes to the effect budget, with its checks (D-523, D-617).
- Added 14 captures: each kind on the map and over a fight, the pit room, three frames of a step that scrolls the view, and four still frames (D-889, D-894, F-97).
- Answered each Gitar pass. A node takes its seed from its name, and the region of a node holds one view on each side.
- Fixed four faults of the play sessions of the owner. F-97: a stream rode the view, and each node keeps `local_coords` on now and moves its start box alone.
- F-98: every torch went out in another room, because Godot stops a system whose region leaves the screen. A node holds the map, one view on each side, and a margin now.
- F-99: the light of a torch snapped in a doorway. The light keeps its place now, and the flame alone jumps (D-891).
- F-100: Godot advances a particle system about one second at a time, so no long stream could hold its motion. A probe of the frames found it. D-893 answers it: Game draws each mote of a weather itself, from a pure function of the tick in integer math.
- Each mote falls, sways as a sheet of paper falls, lands, and lies still for 5 seconds. Ten tests of Core hold that motion.
- A mote holds two grays, from `TheThingBelow.Game/shaders/mote_light.gdshader`: the iron `g` with no light, and the steel `G` in full light (D-825). The shader reads the strength of a light and never its color, and a frame holds `#5A5566` and `#7D7788`.

### The state of the build

- `make verify` and `make smoke` pass on this machine, with 2,162 tests and no failure.
- The smoke session reads the light of each torch over 120 ticks, and it walks a run into the room below (F-98, F-99).

### What is in flight

- The PR holds the baselines of the artifact of the run 35774928391, at the head `6e95b39`: 14 new frames and 52 changed frames. The CI run of the head `fff1d19` passed every job: the three legs of build and test, det-lint, replay-identity, smoke, screen-test, and ste-check.
- The Gitar pass of the head `fff1d19` approved with two open findings. This session answered both: the region of a particle node now holds one view on each side, and the smoke session reads the light of each torch over 120 ticks (F-99).
- The PR needs a new Gitar pass after this push, and then the review of the other provider.

### Traps and gotchas

- A fog over an enemy and over the floor shrinks each luma gap by the same part, so the color of the fog never changes the result of the test (D-892).
- The fixture map foe holds a gap of 16 on the floor key `K` with no fog, so the test skips that pair and reads the loss of the fog alone (D-892).
- The fire of a torch and the burst of a hit stay on the particles of Godot, because each one lives under 2 seconds (D-893). A weather never does.
- A probe that paints each mote in one key and reads the frames found F-97, F-98, and F-100. The still fixture of D-894 holds that ground.
- The capture session takes 68 captures, and each one waits 8 frames. The `sheet` target of the Makefile holds its own frame limit of 1200, as the screen-test job does.
- The three frames of the scroll fixture hold the evidence of F-97. A particle that rides the view stands at the same place of each frame.
- The smoke session walks a run of its own into the room below, and it fails on a node whose region leaves the view, and on a torch with no energy (F-98).
- The capture files of `content/effects/ambient-captures/` never reach a map of the shipped build (D-889).

### The questions that block progress

None.

### The next concrete action

Push the branch, open the PR, and answer the gitar pass. Then take the new and changed baselines from the CI artifact and commit them.

# Session handoff

## Session 206: 2026-09-22, Codex

Author: Codex
Session: reviewer PR-56. Repository: the-thing-below. Branch: `feat/pr-57-effects`. PR: #56. Role: reviewer. Base: `8aaf0f6`.

### What this session did, and why

- Reviewed the complete diff from `8aaf0f6` to effective head `f1b42b5`.
- Verified the provider gate. Claude Code authored the PR, and Codex reviewed it.
- Traced effect loading, cross-file validation, particle construction and seeking, battle timing, hit-stop, shake, settings updates, and screen capture coverage.
- Wrote `docs/reviews/pr-56.md` with no finding and the verdict `Ready for owner merge` for effective head `f1b42b5`.

### The state of the build

- `make verify` passed with 2,069 non-Smoke tests, 0 STE findings, matching replay identity and content hash, matching atlas, and a green smoke session.
- CI run 35742614648 passed the implementation checks, screen-test, and smoke on all current legs. Gitar approved the head. The review-gate check failed only because the review record did not yet exist.
- The remote head before this review commit is `f1b42b5`.

### What is in flight

- The review record and this handoff entry need commit and push.

### Traps and gotchas

- The review targets effective head `f1b42b5`. The review commit changes only the metadata set and does not move that head.
- The screen baselines come from the CI artifact, while the Mac uses another renderer path.

### The questions that block progress

None.

### The next concrete action

Commit and push the review record and this handoff entry. Fetch and verify the remote head and review-gate check.
