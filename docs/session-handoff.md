# Session handoff

## Session 181: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-10, round 4. Repository: the-thing-below. Branch: `feat/pr-10-battle-scene`. PR: #51. Role: author. Base: `8b10888`.

### What this session did, and why

- Committed the `battle-menu-1x` and `battle-menu-fill-1080` baselines from the `screen-captures` artifact of the round-3 run. The author read both frames, and the step label alone changed.
- Corrected the case of one word in the review pass line of `docs/design.md`.

### The state of the build

- Round 3 head `f7fb2aa`: Gitar found no issue, and every CI job passed except `screen-test`, which waited for these two baselines, and the review gate, which waits for `docs/reviews/pr-51.md`.
- 1781 tests pass on this machine.

### What is in flight

- The CI run and the Gitar pass of the round-4 push.
- The review of the other provider, and the owner approval of the rest of the text batch.

### Traps and gotchas

- `make sheet` can fail on "The InputMap action ... doesn't exist" when an input event reaches the capture window. The capture session builds no input map, and `Boot` reads the held steps before it checks the run. CI has no input.
- The owner approved a second concern for the next PR: the fix of the fault above. The next PR carries it beside its own concern. G-8 yields to this owner approval, and the fix needs no decision row. Name both concerns in the PR description.

### The questions that block progress

None.

### The next concrete action

Follow the `gitar-review` skill for the round-4 push. Then tell the owner that PR #51 is ready for the other provider.

## Session 180: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-10, round 3. Repository: the-thing-below. Branch: `feat/pr-10-battle-scene`. PR: #51. Role: author. Base: `8b10888`.

### What this session did, and why

- The owner approved the art batch and played a fight on screen (D-834).
- A win shows no line, and PR-67 builds the summary of the loot and the level-ups after a fight, with loot from PR-13 and PR-65 (D-835). `battle.won` left the table.
- The step reads "Back up" in the front row and "Step forward" in the back row, and the step lines use the same words (D-836). `BattleCommands.ActorRow` holds the row.
- The roadmap blocks of PR-67, PR-13, and PR-65 gained the summary, and the PR-10 block gained D-835 and D-836.

### The state of the build

- Round 2 head `601bfb0`: Gitar found no issue, and every CI job passed except the review gate, which waits for `docs/reviews/pr-51.md`.
- This round: 1781 tests, format, lint, smoke, and the STE check pass on this machine. The author read the new `battle-menu-1x` frame.
- The two menu baselines change with the new label, so the screen-test job of this push fails until round 4 commits them.

### What is in flight

- The CI run and the Gitar pass of the round-3 push.
- The owner approval of the rest of the text batch.

### Traps and gotchas

- `make sheet` can fail on "The InputMap action ... doesn't exist" when an input event reaches the capture window. The capture session builds no input map, and `Boot` reads the held steps before it checks the run. The fault predates PR-10, and CI has no input. Keep the mouse off the window.
- Each round of a session adds its own handoff entry.

### The questions that block progress

None.

### The next concrete action

Commit the two `battle-menu-*` baselines from the `screen-captures` artifact of this push. Then follow the `gitar-review` skill, and tell the owner that PR #51 is ready for the other provider.

## Session 179: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-10, round 1. Repository: the-thing-below. Branch: `feat/pr-10-battle-scene`. PR: #51. Role: author. Base: `8b10888`.

### What this session did, and why

- Asked OQ-103 and OQ-131, then seven scope questions. Recorded D-825 to D-833, and D-831 revises D-819 in part.
- Built the battle screen: `BattleScreen`, `BattleView`, `BattleCommands`, `BattleMessages`, `BattleLayout`, `BattleTimes`, and `BattleWalk` in Game, with the hit flash in `TheThingBelow.Game/shaders/hit_flash.gdshader`.
- `GameRun` plays each event for its ticks, and it keeps the view of the fight (D-532, D-829).
- Drew the fixture art batch: Marrek in battle with his attack pose, the grunt, the brute, the pointer, and 18 icons (D-828, D-830, D-833). The strings of the screen joined the table.
- The smoke session fights through the menu, and the capture list holds the `battle` fixture.

### The state of the build

- `make format`, `make lint`, `make smoke`, and the STE check pass on this machine. `make sheet FIXTURE=battle` wrote four frames, and the author read each one.
- Round 1 pushed `b72563f`. Gitar found no issue in its review of that head. The CI run failed on the four absent `battle-*` baselines, and the review gate waits for the review record.
- Round 2 commits the four baselines from the artifact of that run (D-733). The author read each frame, and each one matches the local capture.

### What is in flight

- The CI run and the gitar pass of the round-2 push.
- The review of the other provider, and the owner approval of the art batch and the text batch.

### Traps and gotchas

- The rules resolve each enemy turn at once. The screen draws `GameRun.BattleView`, never the state, or a hit shows before its blow.
- An enemy that went down leaves its row, so a wave never puts a seventh combatant in one row (D-759).
- The glossary term is `battle view`, because `view` names the part of the map on screen.
- The art generator ran from a scratch folder outside the repository. The drawing files are the source.

### The questions that block progress

None. The owner approves the art batch and the text batch from the PR description (D-57, G-25).

### The next concrete action

Follow the `gitar-review` skill for the round-2 push. Then tell the owner that PR #51 is ready for the other provider.

## Session 178: 2026-09-21, Codex

Author: Codex
Session: reviewer PR-50, round 1. Repository: the-thing-below. Branch: `feat/pr-55-large-pictures`. PR: #50. Role: reviewer. Base: `27fb790`.

### What this session did, and why

- Refreshed the review after the handoff-rule update in `45de10f`.
- Rechecked the implementation-only change since the assessed head `e290570`. The changed paths are the review skill, its commit reference, and D-824.
- Prepared the PR-50 review record with no findings and a `Ready for owner merge` verdict.

### The state of the build

- The implementation head `e290570` passed `make verify` and every required CI job except the review gate.
- The effective head is `45de10f`. The later review-record and handoff commits are metadata commits and do not move it.

### What is in flight

- The review record, this entry, and the handoff archive move need commit and push.

### Traps and gotchas

- Session 168 is the oldest live entry and must move to the archive. Keep its text unchanged.

### The questions that block progress

None.

### The next concrete action

Commit the review record and both handoff files. Push, fetch, and verify the remote head and the review-gate check.

## Session 177: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-55, round 5. Repository: the-thing-below. Branch: `feat/pr-55-large-pictures`. PR: #50. Role: author. Base: `27fb790`.

### What this session did, and why

- The review of PR #50 prepared its record on `e290570` and could not commit it. The scope limits of the `pr-review` skill allowed the record and the handoff entry alone, and a valid handoff also moves an entry to the archive.
- The owner set D-824: a reviewer changes both handoff files as the author does, and never changes the words of another entry. The `pr-review` skill and its reference `commit-and-end.md` hold the rule.
- The prepared review edits were uncommitted in this checkout. `docs/reviews/pr-50.md` stays in place, untracked. The handoff files of that review are saved outside the repository, and the handoff files hold the committed state again.

### The state of the build

- `e290570` passed every CI job except the review gate, which waits for `docs/reviews/pr-50.md`. This round changes a skill, a decision row, and the handoff alone.

### What is in flight

- The review commits its record and its entry as Session 178, under D-824.

### Traps and gotchas

- The prepared entry moved Session 168 to the archive and kept Session 167, so HANDOFF 2 failed. The oldest entry moves first.

### The questions that block progress

None.

### The next concrete action

Read the review record when it lands, and answer each finding.

## Session 176: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-55, round 4. Repository: the-thing-below. Branch: `feat/pr-55-large-pictures`. PR: #50. Role: author. Base: `27fb790`.

### What this session did, and why

- The review sheets of the four fixture pieces and the render of the backdrop reached the PR description. The owner approved them (D-823).
- Gitar approved `59692d2` with one finding: two items 7 in the Gate 4 list of `phase-4-region-one.md`. The item of the budget test is 8 now.
- The CI note of Gitar named RG 7 again. Both review-gate jobs on `59692d2` report RG 7 pass, and RG 3 alone waits for the Codex review record.

### The state of the build

- Every CI job passed on `59692d2` except the review gate, which waits for `docs/reviews/pr-50.md`. 1712 tests pass on this machine.

### What is in flight

- The push of this round, the reply on the Gitar thread, and a Gitar pass on the new head. Then the PR leaves draft for the Codex review.

### Traps and gotchas

- The Gitar CI note reads the review-gate jobs of older heads. Read the log of the job of the current head.

### The questions that block progress

None.

### The next concrete action

Reply on the Gitar thread with the fix commit, prove the next Gitar pass current, and mark the PR ready for the Codex review.

## Session 175: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-55, round 3. Repository: the-thing-below. Branch: `feat/pr-55-large-pictures`. PR: #50. Role: author. Base: `27fb790`.

### What this session did, and why

- Copied the 45 baselines of the CI artifact of `c4988bb` into `screens/baseline`, after a read of each frame. Each world pixel is a palette color.
- Gitar found no code issue on `c4988bb`. Its CI note on the `docs/reviews/` row read an older job, and RG 7 passes on this head.
- The owner asked for an automated balance PR before Act 1, and approved PR-90, the balance harness, first in Phase 4 (D-822). OQ-216 holds its metrics, bands, and policy.
- The review sheets of the four fixture pieces go to the PR description for the owner approval (D-514, D-819, G-25).

### The state of the build

- 1712 tests pass. Build, format, lint, STE, identity, content, atlas, and smoke pass. RG 3 waits for the Codex review record.

### What is in flight

- The push of this round, and the Gitar pass on it. Then the PR leaves draft for the Codex review.

### Traps and gotchas

- The PR holds nine concerns under the override of the owner, and the PR description notes it once.
- A baseline comes from the CI artifact alone (the readme of `screens/baseline`).

### The questions that block progress

- The owner approval of the four fixture pieces (G-25).

### The next concrete action

Answer the Gitar pass on this head, and then ask the owner to approve the fixture art.

## Session 174: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-55, round 2. Repository: the-thing-below. Branch: `feat/pr-55-large-pictures`. PR: #50. Role: author. Base: `27fb790`.

### What this session did, and why

- The large picture format of D-812 and D-816 to D-819: the Core record and checks, the `picture` command, `PictureView` in Game, and the `picture` screen fixture.
- Four fixture pieces wait for the owner approval through their review sheets in the PR description (D-514, G-25).
- The owner added three fixes to this PR:
  - The captures saved the linear colors of HDR 2D, so each baseline and sheet showed the game much darker than the screen. `CaptureColors` writes sRGB now.
  - Game draws each slide at the part of a tick, and the held step reaches each tick of a frame (D-820). A 144 Hz screen showed a hitch between two tiles.
  - A step lasts 16, 32, or 64 ticks, and the party takes 16 (D-821).
- Gitar found no issue in round 1. It named the `docs/reviews/` row of the PR description, and the row now takes a form of D-581.

### The state of the build

- `make build`, format, lint, STE, identity, content, atlas, and smoke pass. The tests fail only on the baselines, which come from the CI artifact.

### What is in flight

- The push of this round, then the new baselines from the `screen-test` artifact: 45 captures in sRGB, with 17 walk frames each step.

### Traps and gotchas

- A test finds a palette color in each pixel of each 1x world baseline, so an old dark baseline fails it.
- `GameRun.Advance` takes a function for the held step. A test or a tool with no player passes null.
- The owner asked if a PR adds automated balance tuning before Act 1. The answer waits for the next question.

### The questions that block progress

None.

### The next concrete action

Copy the baselines from the CI artifact, read each frame, and answer gitar.

## Session 173: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-55, the large pictures, with four owner changes first. Repository: the-thing-below. Branch: `feat/pr-55-large-pictures`. PR: the PR-55 PR. Role: author. Base: `27fb790`.

### What this session did, and why

- OQ-91: the owner chose a place and a repeat alone, with no mirror (D-812).
- The owner approved four changes first, after the merge of PR #49:
  - The console draws in the frame viewport, and no key reached it. The host now pushes each key of an open console there (D-725).
  - Escape closes an open console. On the map of a development build, it ends the session (D-813).
  - Game draws every live enemy at any distance. D-814 revises D-719 in part, and the range stays as the ceiling of D-720.
  - The game shows no button prompt (D-815). The row, the device table, the tracker, and the 12 glyph drawings left the build.
- The simulation version is 10, and the identity file is new (G-17).
- The author read all 42 frames of `make sheet` and `make walk`. The prompt row is gone, and the east enemy draws in each frame.
- The format of large pictures is not started yet.

### The state of the build

- Remote head: the push of this entry. `make verify` passed before the commit.
- The `screen-test` baselines still show the old frames. The job fails until the new captures replace them.

### What is in flight

- The PR waits for gitar and the CI legs. The screen baselines come from the artifact of the `screen-test` job (the readme of `screens/baseline`).

### Traps and gotchas

- A key of the frame viewport never arrives by itself, because the screen shows that viewport through a texture. Use `FrameRoot.PushToLayer`.
- The smoke console check pushes key events into the root viewport. A check that sets the text of the entry passes on the old fault.
- D-815 closed OQ-176, and PR-78 has no controller type call now.

### The questions that block progress

None.

### The next concrete action

Answer gitar, copy the new screen baselines from the CI artifact, and then build the large picture format of D-812.

## Session 172: 2026-09-21, Codex

Author: Codex
Session: review PR-49, the elements and the statuses. Repository: the-thing-below. Branch: `feat/pr-66-elements-statuses`. PR: #49. Role: reviewer. Base: `74c3a64`.

### What this session did, and why

- Reviewed the complete PR diff from merge base `74c3a64`.
- Verified the Gitar correction at `34e6272` and the regression test for a stun on the open turn.
- Added `docs/reviews/pr-49.md` with the verdict for effective head `34e6272`.

### The state of the build

- `make verify` passed with 1625 tests and all local gates.
- GitHub checks passed for the implementation head. The review-gate check waits for the review record.

### What is in flight

- The review record and this handoff entry are pushed. GitHub review-gate passes for the effective head.

### Traps and gotchas

- The effective head is `34e6272`. Commit `3690b61` changes only handoff metadata.
- The review-gate check reads `docs/reviews/pr-49.md` from the PR head.

### The questions that block progress

None.

### The next concrete action

Wait for the remaining GitHub checks, then verify the final PR head and check results.
