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

## Session 201: 2026-09-22, Claude Code

Author: Claude Code
Session: author PR-63, round 2. Repository: the-thing-below. Branch: `feat/pr-63-settings`. PR: #55. Role: author. Base: `daeccfe`.

### What this session did, and why

- Read CI run 35689386604 of head `d6b3606`. Every job passed except screen-test, which named 8 captures: the 3 settings captures with no baseline, and the 5 captures of the `ui` fixture.
- The `ui` fixture draws the longest plain string of the table (D-241), and that string is now `settings.help`. The author read `ui-1x` and `settings-conflict-1x` of the artifact, and both fit.
- Committed the 8 captures of the artifact as the baseline (D-733). The `screens` command then matched all 52 captures.
- The Gitar pass of `d6b3606` approved with no finding. The review-gate fault is RG 3 alone: the review record of the other provider does not exist yet.

### The state of the build

- The baseline commit `c974fe4` and this entry make the push of this round. Every CI job except screen-test passed on `d6b3606`.

### What is in flight

- The Gitar pass of the new effective head, then the Codex review of `docs/reviews/pr-55.md`.
- The owner reads the text batch of the settings strings in the PR description (D-57).

### Traps and gotchas

- A new plain string that is longer than `settings.help` changes the 5 captures of the `ui` fixture again.

### The questions that block progress

None.

### The next concrete action

Run the Gitar wait for the push of this round, and prove that the review of the effective head is current.

## Session 200: 2026-09-22, Claude Code

Author: Claude Code
Session: author PR-63, round 1. Repository: the-thing-below. Branch: `feat/pr-63-settings`. PR: #55. Role: author. Base: `daeccfe`.

### What this session did, and why

- Asked the owner OQ-100, OQ-106, OQ-108, and OQ-109, and recorded the answers as D-860 to D-863. The first batch did not show the options of `docs/questions.md`, so the session asked OQ-100, OQ-108, and OQ-109 again with both sets of options. OQ-108 took a third answer from the owner: a conflict blocks the save and the exit, and the `ui_*` actions take no remap (D-862).
- Asked the values of the settings, and recorded D-864 to D-874. D-864 answers OQ-112, and D-872 answers OQ-110. D-870 removes the shape icons, because the 18 icons of PR-10 already differ in shape. D-873 corrects D-866: the session offered example times and did not read the pace of PR-10 first. D-874 stores the body size as auto, small, or large.
- Built the settings file in Storage: `GameSettings`, `ControlBindings`, `SettingsText`, `SettingsFormat`, and `SettingsStore`, with a fixture of format 1 (D-860, D-869).
- Built the settings screen in Game: `SettingsMenu` holds the rows and the rules, and `SettingsScreen` draws them. The menu action opens the screen (D-871), and the mouse moves the cursor (D-872).
- Applied the settings: the input map and the dead zone, the window mode, the fit, the body size, the message speed of the battle screen, the confirm skip, and the remembered cursor (`CommandMemory`).
- Added three captures of the settings screen, a smoke step, and tests for each new type.
- Read each settings frame of `make sheet`: `settings-1x`, `settings-fill-1080`, and `settings-conflict-1x`. The first layout overlapped the help line at a body of 32 and cut one button name. The second layout fits at both body sizes.

### The state of the build

- `make verify` passed locally with 1,987 tests outside Smoke, 0 findings of ste-check and det-lint, and the smoke session green.
- No file of Core changed, so the simulation version stays (G-17).
- The remote head is the push of this round on `feat/pr-63-settings`.

### What is in flight

- The screen-test job has no baseline for the three settings captures. The author commits the captures of the CI artifact as the baseline after the first run (D-733).
- The Gitar pass of the first push, then the review of the other provider.
- The owner reads the text batch of the settings strings in the PR description (D-57).

### Traps and gotchas

- The menu action on the map opens the settings screen until PR-62. In a fight, the menu intent goes to the rules as before.
- The close intent of the menu goes by its id, because the open intent can still wait in the queue in the same frame.
- The frame shows through a texture, so the mouse reads its frame pixel from `ScreenFit.ToFrame`.
- The volumes, the mono toggle, the mute, the vibration, the text speed, and the reduction save and load, and no code reads them yet. PR-36, PR-57 to PR-60, PR-69, and PR-70 read them.

### The questions that block progress

None.

### The next concrete action

Download the captures of the first screen-test run, commit the three settings baselines, and push. Then run the Gitar wait of `docs/runbooks/session-context.md`.

## Session 199: 2026-09-22, Codex

Author: Codex
Session: reviewer PR-93, repeat review. Repository: the-thing-below. Branch: `chore/pr-93-docs-only-ci`. PR: #54. Role: reviewer. Base: `d1a03f7`.

### What this session did, and why

- Read the response to P1-1 and recomputed the effective head as `c9429c4`. The intervening commits change only review and handoff metadata.
- Verified the live synchronize-event evidence in run 35680701238. The job logged a non-empty `BEFORE_REF` and returned `documents-alone: true`.
- Verified runs 35681799485 and 35681880270. Two consecutive docs-only pushes both skipped each build job, and each gate reported success.
- Updated `docs/reviews/pr-54.md`: P1-1 is withdrawn, the earlier verdict is preserved, and the current verdict is `Ready for owner merge` for effective head `c9429c4`.

### The state of the build

- The effective implementation head is `c9429c4`. The author reports `make verify` passed with 1,924 tests outside Smoke. The live regression runs passed.

### What is in flight

- The repeat-review record and this handoff entry need commit and push.

### Traps and gotchas

- The event payload documentation was incomplete for this field. Live workflow output is the decisive evidence for the trigger.

### The questions that block progress

None.

### The next concrete action

Commit and push the repeat-review record and handoff. Verify the review-gate check covers effective head `c9429c4`.

## Session 198: 2026-09-22, Claude Code

Author: Claude Code
Session: author PR-93, round 3. Repository: the-thing-below. Branch: `chore/pr-93-docs-only-ci`. PR: #54. Role: author. Base: `d1a03f7`.

### What this session did, and why

- Answered the review of session 197 in `docs/reviews/pr-54-response.md`.
- P1-1 has no merit. CI run 35680701238, on `43c2edc` with the event `pull_request` and the action `synchronize`, read `BEFORE_REF: c9429c4…` from `github.event.before`. It wrote the push facts and gave `documents-alone: true`.
- The push of this entry is the second docs-only push in a row after the green head `c9429c4`. The review asked for that run as its regression check.

### The state of the build

- No code change. The effective head stays `c9429c4`, and `make verify` passed there with 1,924 tests.
- Gitar approved `c9429c4`. The remote head is the push of this entry.

### What is in flight

- The CI run of this push. Its `changed paths` job must give `documents-alone: true` from the previous head `43c2edc`.
- The repeat review of the other provider.

### Traps and gotchas

- The previous head `43c2edc` skipped each build job. Thus its required checks come from the gate jobs of `c9429c4`, and this run proves the fix of round 2 live.
- That did not happen. The reviewer pushed `3ba4dad` at 02:58:18Z, and the push of `38cface` at 02:59:44Z cancelled its run. Each gate of `3ba4dad` then showed `failure`, so run 35681498346 of `38cface` ran every job, as D-858 requires. Every job passed.
- Two spaced metadata pushes follow: A after the green `38cface`, and B after A skips. B is the live check of two docs-only pushes in a row. Each push waits until the run before it completes.
- Push A, `54e8c5c`: run 35681799485 gave `documents-alone: true`, each build job skipped, and each gate reported `success`. Push B holds this line, and its previous head is A.
- Push B, `bed989b`: run 35681880270 read `BEFORE_REF: 54e8c5c…` and gave `documents-alone: true`. Two docs-only pushes in a row both skip. `docs/reviews/pr-54-response.md` records both runs.

### The questions that block progress

None.

### The next concrete action

The other provider repeats the review at effective head `c9429c4`.
