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

## Session 205: 2026-09-22, Claude Code

Author: Claude Code
Session: author PR-57, the baseline round. Repository: the-thing-below. Branch: `feat/pr-57-effects`. PR: #56. Role: author. Base: `8aaf0f6`.

### What this session did, and why

- CI run 35741921184 on `f3f2e1f` failed as planned. Each leg failed the six baseline tests alone, and the screen-test job found the six new captures and 132 changed pixels in `battle-blow-1x.png`.
- The two capture runs on lavapipe matched each other, so the seek of a burst gives one picture on CI too.
- Read each of the seven frames of the `screen-captures` artifact, and committed them to `screens/baseline/` (D-733, D-784). The heavy frames move the picture by -4, 4, 1, and 0 art pixels, and the number stays still.

### The state of the build

- The remote head before this round is `f3f2e1f`. This round adds the seven baselines and this entry.
- The review-gate check fails on RG 3 alone: no review record at `docs/reviews/pr-56.md` yet.

### What is in flight

- The CI run of this push, and the gitar pass on it.
- The Codex review in `docs/reviews/pr-56.md`.

### Traps and gotchas

- The baselines come from the CI artifact alone. The Mac draws other pixels with the same renderer.

### The questions that block progress

None.

### The next concrete action

Follow the `gitar-review` skill on this push, and answer each comment. Then tell the owner that the PR is ready for Codex.

## Session 204: 2026-09-22, Claude Code

Author: Claude Code
Session: author PR-57. Repository: the-thing-below. Branch: `feat/pr-57-effects`. PR: #56. Role: author. Base: `8aaf0f6`.

### What this session did, and why

- Asked OQ-98 and OQ-99. D-875 sets `GPUParticles2D`, and D-876 shakes the battle picture alone.
- Found that no heavy blow and no spell exist before PR-12, and asked. D-877 to D-883 set the heavy blow, the spell flash in PR-12, the hit files, the hit-stop, the hit flash, the sparks of the brute, and the battle file. D-884 moves the hurt flinch to PR-17.
- Built the records and readers of Core (`TheThingBelow.Core/Effects/`), the particle row of the budget, the battle file, and the two hit files.
- Moved the timings of PR-10 into the battle file. Game builds one particle node for each palette key, and seeks each burst to its age in ticks.
- Added six battle captures: blood, sparks, a frame inside the hit-stop, and the heavy blow at full, reduced, and off.
- Read each battle frame of `make sheet FIXTURE=battle` on the Mac. Two runs gave the same bytes.

### The state of the build

- Local: build, format, det-lint, content hash, replay identity, atlas, smoke, and ste-check pass. The smoke line counts 8 particle nodes.
- 2063 of 2069 tests pass. The six failures are the baselines of the new captures, which the first CI run makes (D-733).
- The branch holds `f2a0f57` and this entry. The push of this round sets the remote head.

### What is in flight

- The screen-test job fails on the six new captures and on the changed `battle-blow-1x.png`. The author reads each frame of the `screen-captures` artifact and commits them to `screens/baseline/`.
- The gitar pass, then the Codex review in `docs/reviews/pr-56.md`.

### Traps and gotchas

- The walk to the deep room queues a step only when no step runs. A step queued inside a step runs after it and carries the party past the turn. The hall walk keeps its old queue, so its baselines stay.
- The capture fixture stages each heavy blow as the real hit of the fight with the weak affinity, because no move carries an element before PR-12.
- A burst seeks with `Restart(keepSeed: true)` and `RequestParticlesProcess`, at speed zero. The seed comes from the tick when the event started.

### The questions that block progress

None.

### The next concrete action

Push, open the PR, and wait for CI. Read and commit the baselines from the artifact, then follow the `gitar-review` skill.

## Session 203: 2026-09-22, Codex

Author: Codex
Session: reviewer PR-55, repeat review. Repository: the-thing-below. Branch: `feat/pr-63-settings`. PR: #55. Role: reviewer. Base: `daeccfe`.

### What this session did, and why

- Recomputed the effective head. The initial record named `d6b3606`, but `c974fe4` changes eight substantive screen baselines.
- Verified the original screen-test trigger and the correction. The three settings captures and five `ui` captures now pass the current CI screen-test.
- Updated `docs/reviews/pr-55.md` with no finding and the verdict `Ready for owner merge` for effective head `c974fe4`.

### The state of the build

- The implementation checks passed at `d6b3606` with 1,987 non-Smoke tests and all local gates.
- CI run 35691479039 passed the corrected baselines and all current checks. The remote tip is metadata after effective head `c974fe4`.

### What is in flight

- The repeat-review record and this handoff entry need commit and push.
- The owner can merge after review-gate passes for effective head `c974fe4`.

### Traps and gotchas

- Screen baselines are substantive review paths. Metadata commits after `c974fe4` do not change the effective head.

### The questions that block progress

None.

### The next concrete action

Commit and push the repeat review and handoff. Fetch and verify the remote head and review-gate check.

## Session 202: 2026-09-22, Codex

Author: Codex
Session: reviewer PR-55, initial review. Repository: the-thing-below. Branch: `feat/pr-63-settings`. PR: #55. Role: reviewer. Base: `daeccfe`.

### What this session did, and why

- Reviewed the settings file, input remap, settings screen, runtime application, tests, and screen baselines of PR #55.
- Verified the provider gate. Claude Code authored the PR, and Codex reviewed it.
- Wrote `docs/reviews/pr-55.md` with no finding and the verdict `Ready for owner merge` for effective head `d6b3606`.

### The state of the build

- `make verify` passed at `d6b3606` with 1,987 non-Smoke tests, 0 STE findings, 0 det-lint findings, matching replay identity and content hash, matching atlas, and a green smoke session.
- CI run 35689788015 passed its build, test, format, det-lint, replay identity, smoke, screen-test, and coverage jobs. Gitar approved the tip with no finding.

### What is in flight

- The review record and this handoff entry need commit and push.
- The owner can merge after the review-gate check turns green.

### Traps and gotchas

- The review targets effective head `d6b3606`. The later baseline and handoff commits are metadata only.

### The questions that block progress

None.

### The next concrete action

Commit and push the review record and handoff. Fetch and verify that the review-gate check covers effective head `d6b3606`.
