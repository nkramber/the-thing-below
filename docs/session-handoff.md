# Session handoff

## Session 186: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-56, the light and the shadows. Repository: the-thing-below. Branch: `feat/pr-56-light-and-shadows`. PR: #53. Role: author. Base: `e0cc485`.

### What this session did, and why

- The owner answered OQ-94 to OQ-97 and added two requests: the party carries a torch, and the art target is the HD-2D look. D-842 to D-851 record each answer.
- PR-91 (the torch item, after PR-13) and PR-92 (three HD-2D passes, after PR-59) join the roadmaps. OQ-217 and OQ-218 block PR-91.
- Core reads the decor kinds, the decor files, the light setups, the carried light, and the effect budget. The load checks each file against the maps and the palette. It also checks the worst view of the Deck and the 15 lights of one canvas item.
- The simulation version is 12, and the identity file is new (G-17).
- Game draws the lit atlas pages, the full-tile wall shadows, the torches, the carried light, and the key light of a battle. The `torch` command of the console switches the carried light with no intent.

### The state of the build

- `make verify` passes with 1,891 tests. `make smoke` passes. `make sheet` shows the lit map, walk, and battle frames.
- The remote head is the first push of this branch.

### What is in flight

- The screen-test job fails on the first push, because every map, walk, and battle frame changes. The next round commits the baseline from the `screen-captures` artifact (D-733).
- Gitar and the Codex review wait for the PR.

### Traps and gotchas

- Do not send the output of `make smoke` to `artifacts/smoke.log`. The target writes that file and then `cat`s it, so the file grows with no end.
- `OccluderPolygon2D.CullMode` Clockwise keeps each wall dark. CounterClockwise lights the wall tile of each torch alone, as a flat block.
- The collection expression `[0, last]` in Core pulls in `System.Runtime.InteropServices`, and the Core reference test fails.
- zsh arrays count from 1.

### The questions that block progress

None for PR-56. OQ-217 records a clash for PR-91: D-566 puts no fog of war on a map, and D-848 makes the torch needed to see in the dark.

### The next concrete action

Download the `screen-captures` artifact of the first CI run, read each frame, and commit the new baseline. Then answer gitar.

## Session 185: 2026-09-21, Codex

Author: Codex
Session: review PR-48, normal maps. Repository: the-thing-below. Branch: `feat/pr-48-normal-maps`. PR: #52. Role: reviewer. Base: `9e9dc59`.

### What this session did, and why

- Reviewed the complete PR-52 diff from merge base `9e9dc59` through effective head `2f41276`.
- Verified the opposite-provider gate, normal-map generation, override validation, atlas integration, capture input fix, documents, and tests.
- Added `docs/reviews/pr-52.md` with the verdict `Ready for owner merge`.

### The state of the build

- `make verify` passes with 1,839 tests and all local gates.
- GitHub checks pass at `2f41276` except the review gate, which waits for this review record.

### What is in flight

- This review record and this handoff entry need commit and push.

### Traps and gotchas

- Gitar reports no code issue, but functional validation is disabled.
- The review-gate failure is expected until this record reaches the PR head.

### The questions that block progress

None.

### The next concrete action

Commit the review record and handoff files. Push, fetch, and verify the remote head and the review-gate check.

## Session 184: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-48, round 1. Repository: the-thing-below. Branch: `feat/pr-48-normal-maps`. PR: #52. Role: author. Base: `9e9dc59`.

### What this session did, and why

- Built the normal maps of PR-48 (D-183, D-184, D-502). The `atlas` command writes `sprites/normal-map-<page>.png` beside each page of tiles, map sprites, battle sprites, and pieces, and `--check` compares them by pixel (F-19). Portraits and the UI take none (D-210).
- Asked the owner four questions and recorded the answers: the height of each palette color (D-838), the override grid (D-839), the rim of 4 pixels (D-840), and the form of the review sheet (D-841).
- Fixed the second concern that the owner approved in Session 182. `Boot` now checks the run before it reads the held steps, so a key in the capture window writes no InputMap error line. The capture session pushes one stray step key as its regression test.
- Raised the simulation version to 11, because the content reader refuses new shapes (G-17). The identity file changed with it (D-504).

### The state of the build

- `make verify` passes on this machine. The remote head is the commit of this entry on `feat/pr-48-normal-maps`.
- The capture of the old `Boot` code failed on "The InputMap action "step_north" doesn't exist". With the fix, `make sheet` passes with no error line.
- The branch captures match the captures of `main` pixel for pixel, 49 of 49. Six captures of this Mac differ from the committed baselines by one color level on `main` too, so this PR does not cause that difference.

### What is in flight

- The Gitar pass of this push, and the review of the other provider.
- The owner approval of the normal-map sheets in the PR description (exit test 5, G-25).

### Traps and gotchas

- The row of nine copies of a 64-pixel piece is wider than 1600 pixels. D-841 records the wrong count of the question.
- The drawing reader refuses a tile that is not 32 by 32, so a small test drawing goes on the `map_sprites` page.
- A picture or an atlas fixture of a lit page needs its `normal-map-` page, or the content set refuses the set.

### The questions that block progress

None. The owner approval of the sheets is exit test 5.

### The next concrete action

Follow the `gitar-review` skill for this push. Ask the owner to approve the normal-map sheets. Then tell the owner that PR #52 is ready for the other provider.

## Session 183: 2026-09-21, Codex

Author: Codex
Session: reviewer PR-51, round 1. Repository: the-thing-below. Branch: `feat/pr-10-battle-scene`. PR: #51. Role: reviewer. Base: `8b10888`.

### What this session did, and why

- Reviewed the complete PR-51 diff from merge base `8b10888` through effective head `3859a74`.
- Verified the opposite-provider gate, the battle event flow, the command menu, the string table, the screen layout, and the tests.
- Added `docs/reviews/pr-51.md` with the verdict `Ready for owner merge`.

### The state of the build

- `make verify` passed with 1781 tests and all local gates.
- GitHub checks pass for `3859a74` except the review gate, which waits for the review record.

### What is in flight

- This review record and this handoff entry need commit and push.

### Traps and gotchas

- Gitar reports no issues, but its functional validation is disabled. The review uses the local contract-specific tests and smoke output.

### The questions that block progress

None.

### The next concrete action

Commit the review record and handoff files. Push, fetch, and verify the remote head and the review-gate check.

## Session 182: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-10, round 5. Repository: the-thing-below. Branch: `feat/pr-10-battle-scene`. PR: #51. Role: author. Base: `8b10888`.

### What this session did, and why

- The owner approved the text batch, with one change: a resisted hit reads "{target} takes {amount}. Barely a mark." (D-837).
- Round 4 head `5b0ab39` passed Gitar with no issue and every CI job except the review gate.

### The state of the build

- This round changes one string, a decision row, the design pass line, and this entry. No capture shows a resisted hit, so no baseline changes.
- 1781 tests pass on this machine. The review gate waits for `docs/reviews/pr-51.md`.

### What is in flight

- The CI run and the Gitar pass of the round-5 push, then the review of the other provider.

### Traps and gotchas

- `make sheet` can fail on "The InputMap action ... doesn't exist" when an input event reaches the capture window. The capture session builds no input map, and `Boot` reads the held steps before it checks the run. CI has no input.
- The owner approved a second concern for the next PR: the fix of the fault above. The next PR carries it beside its own concern. G-8 yields to this owner approval, and the fix needs no decision row. Name both concerns in the PR description.

### The questions that block progress

None.

### The next concrete action

Follow the `gitar-review` skill for the round-5 push. Then tell the owner that PR #51 is ready for the other provider.

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
