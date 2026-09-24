## Session 265: 2026-09-24, Codex

Author: Codex
Session: reviewer PR #71, round 2. Repository: the-thing-below. Branch: `feat/pr-68-story-scenes`. PR: #71. Role: reviewer. Base: `31f172e`.

### What this session did, and why

- Re-reviewed the correction of P2-1 from `f8868ae` to `498137c`.
- Confirmed that stored story actors must name a character of this build (D-166, D-1006).
- Ran the snapshot tests and `make verify`. Both passed.
- Updated `docs/reviews/pr-71.md` and corrected the verified test count and handoff line in the PR description.

### The state of the build

- Base: `31f172e`. Effective head: `498137c`. Remote head before this metadata commit: `498137c`.
- Local verification passed with 2,852 tests. CI run `35949615173` passed all implementation checks on macOS, Ubuntu, and Windows.

### What is in flight

- The review record and this entry are one metadata commit. This round pushes it to `feat/pr-68-story-scenes`.

### Traps and gotchas

- The Gitar status notice has no review item. D-964 says it needs no answer and does not block the verdict.
- The author correction passed the original undeclared-actor trigger and the valid actor snapshot case.

### The questions that block progress

None.

### The next concrete action

Fetch the branch, verify the pushed metadata head, and report the review verdict for PR #71.

## Session 264: 2026-09-24, Claude Code

Author: Claude Code
Session: author PR #71, round 2. Repository: the-thing-below. Branch: `feat/pr-68-story-scenes`. PR: #71. Role: author. Base: `31f172e`.

### What this session did, and why

- Read the review record of `f8868ae`: `Changes required`, with one finding, P2-1.
- P2-1 has full merit. A stored story scene actor with an id that no character of the build holds passed the load. `StoryContent.HoldsCast` now gives the cast of the build, and `StoryState.ResumeScene` refuses such an actor (D-166, D-1006).
- Added the regression case to `StorySnapshotTests`. It fails on `f8868ae` and passes on the correction.
- Wrote `docs/reviews/pr-71-response.md`.
- Moved the title of `docs/session-handoff-archive.md` back to the top. The review commit put Session 253 above it.

### The state of the build

- `main` is `31f172e`. The remote head before this round is `b6fd45a`, the review record of Session 263. The push of this round carries the correction, the response, and this entry.
- `make verify` passed on macOS arm64 with 2,852 tests, the smoke session included. CI of round 1 passed every job except the review gate.

### What is in flight

- PR #71 waits for CI of this round, one read of the Gitar output, and `make codex-review PR=71 -- --skip-gitar-review` (D-945, D-946).

### Traps and gotchas

- The traps of Session 262 stand: a move intent on the tick of an arrival chains a step, no story scene ships in content, and the battle of a story scene names the story scene in its enemy field.
- The Gitar comment of round 1 is a plan notice with no item, so it needs no answer (D-964).

### The questions that block progress

None for PR-68.

### The next concrete action

Read the CI result of the pushed head, read the Gitar output one time, then run `make codex-review PR=71 -- --skip-gitar-review` in the background.

## Session 263: 2026-09-24, Codex

Author: Codex
Session: reviewer PR #71, round 1. Repository: the-thing-below. Branch: `feat/pr-68-story-scenes`. PR: #71. Role: reviewer. Base: `31f172e`.

### What this session did, and why

- Reviewed PR #71 from merge base `31f172e` through effective head `f8868ae`.
- Confirmed Claude Code authored the change, so Codex passes the provider gate (T-4, D-17).
- Traced story content, triggers, simulation, battles, joins, saves, snapshots, and replay.
- Found P2-1: snapshot restore accepts a shown actor id that the content does not declare.
- Added `docs/reviews/pr-71.md` with `Changes required` for `f8868ae`.
- Corrected the Documents row of the PR description.

### The state of the build

- Base: `31f172e`. Effective head: `f8868ae`. Remote head before this metadata commit: `ebd6f6b`.
- `make verify` passed on macOS arm64 with 2,851 tests. CI run `35948186000` passed implementation checks on macOS, Ubuntu, and Windows. `review-gate` failed because the review record was absent.

### What is in flight

- The review record and this handoff entry are one metadata commit. The review requires a corrected effective head and a repeat review.

### Traps and gotchas

- The only Gitar comment is a free-plan status notice with no item. D-964 says it needs no answer and does not block the verdict.
- No screen changed, so no screen-test frames needed visual review.

### The questions that block progress

None.

### The next concrete action

Correct P2-1, publish the correction, and start a repeat review of PR #71.

## Session 262: 2026-09-24, Claude Code

Author: Claude Code
Session: author PR #71, round 1. Repository: the-thing-below. Branch: `feat/pr-68-story-scenes`. PR: #71. Role: author. Base: `31f172e`.

### What this session did, and why

- Asked the owner OQ-144 to OQ-149 and nine rules of the runner, and recorded D-997 to D-1013. The owner chose the tree form of a condition against the recommendation (D-1001), and added the pause of D-1009.
- Built the story scene format, the runner in Core, the flag file, the condition tree, the four trigger kinds of a map, the scene battle, and the join (D-540, D-563).
- The snapshot gained the story state. Save format 9 and simulation version 21 hold it, with a fixture save of format 9 and a story run in the identity set.
- Added the tests of the ten exit tests of PR-68, the seed loop of one thousand seeds included.
- Moved the fire of the talk trigger and the NPC actor to PR-14, the pause screen to PR-36, and the step condition to PR-18 (D-1005, D-1007, D-1010).

### The state of the build

- `main` is `31f172e`. The branch head before this entry is `f8868ae`, and the push of this round carries this entry.
- `make verify` passed on macOS arm64 with 2,851 tests, the smoke session included.

### What is in flight

- PR #71 waits for CI, one read of the Gitar output, and `make codex-review PR=71 -- --skip-gitar-review` (D-945, D-946).

### Traps and gotchas

- A move intent on the tick of an arrival chains the next step. A bot script moves only while the lead stands.
- No story scene ships in content. Game draws no step before PR-36, so a trigger in the fixture dungeon stops the smoke session.
- The battle of a start battle step names its story scene in the enemy field of the battle. The encounter code of Game reads a patrol, so PR-36 draws that battle start.
- The word actor alone names the combatant of a turn. The glossary term is story scene actor.
- The map field `triggers` is required. A test map needs `"triggers": []`.

### The questions that block progress

None for PR-68.

### The next concrete action

Read the CI result of the pushed head, read the Gitar output one time, then run `make codex-review PR=71 -- --skip-gitar-review` in the background.

## Session 261: 2026-09-24, Codex

Author: Codex
Session: reviewer PR #70, round 1. Repository: the-thing-below. Branch: `feat/pr-62-menu-windows`. PR: #70. Role: reviewer. Base: `a929c59`.

### What this session did, and why

- Reviewed PR #70 from merge base `a929c59` through effective head `654b678`.
- Confirmed Claude Code authored the change, so Codex passes the provider gate (T-4, D-17).
- Read all changed paths and traced the menu, map, notice, save, replay, and settings contracts. Read all 16 changed screen frames from CI.
- Added `docs/reviews/pr-70.md` with `Ready for owner merge` for `654b678`. Corrected the Documents row of the PR description.

### The state of the build

- `main` and the PR base are `a929c59`. The effective head and remote head before this metadata commit are `654b678`.
- `make verify` passed on macOS arm64 with 2,703 tests. CI run 35940703848 passed implementation checks on macOS, Ubuntu, and Windows. Review-gate failed because the review record was absent.

### What is in flight

- This metadata commit holds the review record and this handoff entry. Fresh review-gate and metadata-tip CI checks passed after publication.

### Traps and gotchas

- The Gitar status notice has no item. The review-thread query returned no threads, so D-964 requires no answer.
- The CI screen artifact has 16 changed frames. All were read for this review (D-733, D-784).

### The questions that block progress

None for PR-70.

### The next concrete action

The review record and handoff are committed and pushed together. The remote head is `2888448`; review-gate and all applicable metadata-tip checks passed.

## Session 260: 2026-09-24, Claude Code

Author: Claude Code
Session: author PR-62, round 1. Repository: the-thing-below. Branch: `feat/pr-62-menu-windows`. PR: #70. Role: author. Base: `a929c59`.

### What this session did, and why

- Asked OQ-111 and OQ-113 first, then the gaps that the code found: the map action, the log place, the dim entries, the notice source, the status sheet, the list order, the exit mark, and the time of a notice. D-982 to D-996 record the answers.
- Core: the notice file, the notice rule, the notice log of 30 entries, and the row intent of the party window. Save format 8 and simulation version 20. The debug console posts a notice with `notice` and `aside`.
- Storage: settings format 2, whose step adds the map action with M and Back.
- Game: the main list, the party, status, and log windows, the dungeon map screen, the notice box, and the map action. The settings screen opens from the main list.

### The state of the build

- `main` is `a929c59`. The branch holds three decision commits, the Core commit `10985be`, and the Game commit `13829ba`.
- CI run 35939130614 failed on the 8 new captures and 8 changed ones alone: the settings frames gain the Map row, and the ui frames show the notice line as the longest plain string. The baseline commit takes those 16 files from its `screen-captures` artifact (D-733). Smoke, identity, det-lint, and STE passed on every leg.
- Local: build, format, det-lint, STE, identity, content, and smoke pass. 2703 of 2703 tests pass.
- The author read each new frame of `make sheet FIXTURE=menu` and `FIXTURE=notice` (D-784).

### What is in flight

- CI on the commit of the 4 stable baselines from run 35940125266, whose two capture runs matched. Then the Codex review.

### Traps and gotchas

- `RunState.Start`, `Resume`, `Simulation`, and `RunReplay.Play` take the notice file. Tests use `TestBattles.Notices`.
- The capture session builds no input map, so the menu captures build each view and send no event.
- A capture that shows the map seeks its particles to the tick of the run. The first menu frames did not, and `menu-list-fill-1080` moved by one color step between runs.
- The glossary refuses "banner". The box at the top edge is the notice box.

### The questions that block progress

None. The PR description holds the game text batch for the owner (D-57).

### The next concrete action

Wait for CI on the baseline commit to finish green except `review-gate`. Read Gitar once under D-945, then run `make codex-review PR=70 -- --skip-gitar-review`.

## Session 259: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #69, round 1. Repository: the-thing-below. Branch: `feat/pr-67-character-level`. PR: #69. Role: reviewer. Base: `5b56d3d`.

### What this session did, and why

- Reviewed PR #69 from merge base `5b56d3d` through effective head `9604e8f`.
- Confirmed Claude Code authored the change, so Codex passes the provider gate (T-4, D-17).
- Traced experience, levels, MP, save migration, replay state, battle events, and the battle view. Inspected all 81 changed paths.
- Read all 18 changed battle frames from the screen-test artifact. No visual fault was found (D-733, D-784).
- Added `docs/reviews/pr-69.md` with `Ready for owner merge` for `9604e8f`.

### The state of the build

- `main` and the PR base are `5b56d3d`. The remote PR head before this metadata commit is `9604e8f`.
- Focused tests passed, 89 of 89, after `make build` created the Game assembly.
- CI run 35931706529 passed the implementation checks on every platform. Review-gate failed because the review record was absent.

### What is in flight

- This metadata commit holds the review record and this handoff entry. Fresh review-gate and metadata-tip CI checks passed after publication.

### Traps and gotchas

- Gitar's only comment is a status notice without an item. D-964 says it needs no answer and does not block the verdict.
- Focused tests that read the Game assembly need `make build` first.

### The questions that block progress

None for PR-69.

### The next concrete action

The review record and handoff are committed and pushed together. The remote head is `2888448`; review-gate and all applicable metadata-tip checks passed.

## Session 258: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-67, round 1. Repository: the-thing-below. Branch: `feat/pr-67-character-level`. PR: #69. Role: author. Base: `5b56d3d`.

### What this session did, and why

- Asked OQ-134, OQ-135, OQ-136, and the new OQ-244 before any code, then the numbers, the summary, and the text. D-966 to D-981 record the answers.
- The owner revised D-387: a downed character earns no experience (D-974). The summary rises above each head on the battle screen, and each character shows a health bar and an MP bar (D-975, D-976).
- Core: the stat curve and the join level of each character, the enemy level and experience, the cut, the gap, and the experience table. It also adds the award at a win, the level-up fill, and the restore rules of D-970. Save format 7 and simulation version 19.
- Game: the text of the summary, the party bars, and "HP 60/60 MP 8/8" on the bottom line. The captures `battle-experience-1x`, `battle-level-up-rise-1x`, and `battle-level-up-1x` are new.

### The state of the build

- `main` is `5b56d3d`. PR #69 holds the decision commit, the round commit `b03ef23`, and the baseline commit.
- CI run 35931067896 failed on the three new frames and 15 changed battle frames alone. The baseline commit holds those 18 files from its `screen-captures` artifact (D-733). Format, det-lint, STE, identity, content, atlas, and smoke pass.

### What is in flight

- CI on the baseline commit. Then one read of Gitar, and the Codex review.

### Traps and gotchas

- The glossary term "share" means the part of health that poison moves. The experience code says "shrunk", and the glossary now holds the new terms of PR-67.
- The capture session builds a new screen for each frame, so the message line of a summary frame is empty. The game keeps the last line.
- The two level-up frames stage a level-up on the view. The fixture fight gives 12 experience, and level 2 takes 20 (D-977).
- The status panel holds a name of 8 characters at body 32 (D-981).

### The questions that block progress

None.

### The next concrete action

Wait for CI on the baseline commit to finish green except `review-gate`. Read Gitar once under D-945, then run `make codex-review PR=69 -- --skip-gitar-review`.

## Session 257: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #68, round 3. Repository: the-thing-below. Branch: `feat/pr-98-waiting-enemies`. PR: #68. Role: reviewer. Base: `3223bcf`.

### What this session did, and why

- Re-reviewed PR #68 at effective head `146f605`. The author added D-964, which says that a Gitar comment with no item needs no answer.
- Confirmed Claude Code authored the substantive changes. Codex passes the provider gate (T-4, D-17).
- Read the PR comments. The only Gitar comment is a status notice with no item, so it does not block the verdict (D-964).
- Ran `make verify`; all 2,524 tests and the local checks passed. CI implementation checks passed. Updated `docs/reviews/pr-68.md` to `Ready for owner merge`.

### The state of the build

- `main` and the PR base are `3223bcf`. The effective head and remote head before this metadata commit are `146f605`.
- CI run 35923363333 passed the implementation checks. CI run 35923364396 failed RG 5 because the review record named `caaea8f`, not the new effective head `146f605`.

### What is in flight

- This metadata commit holds the review record and this handoff entry. A fresh `review-gate` result must pass after publication.

### Traps and gotchas

- The Gitar status notice has no item and needs no answer under D-964.
- A new prompt and test change made `146f605` the effective head. The older approval covers only `caaea8f`.

### The questions that block progress

None. OQ-243 is resolved by D-963.

### The next concrete action

Commit and push the review record and handoff together. Fetch, then verify the remote head and the new `review-gate` result.

## Session 256: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-98, round 3. Repository: the-thing-below. Branch: `feat/pr-98-waiting-enemies`. PR: #68. Role: author. Base: `3223bcf`.

### What this session did, and why

- Read the record of Session 255: `Ready for owner merge` for `caaea8f`. The owner stopped the merge first, because the review of round 1 blocked on a Gitar status notice alone.
- Asked the owner two questions. The owner put a new Gitar rule in PR-98, an exception to G-8 (D-965). The owner chose that neither side acts on a Gitar comment with no item (D-964).
- D-964: a Gitar item is a review thread, a finding of the dashboard, or a claim of the CI analysis. A comment with no item needs no answer, and the reviewer ignores it. D-14, D-66, D-67, D-944, and D-946 are revised in part.
- Tools: every prompt of `make codex-review` now holds the D-964 line, with the flag of D-946 or without it. A test reads both forms.
- Rules: `CLAUDE.md`, `AGENTS.md`, the `pr-review`, `gitar-review`, `one-pr-one-session`, and `ste-writing` skills, `docs/runbooks/merge.md`, the PR template, and `docs/design.md`.

### The state of the build

- `main` is `3223bcf`. The effective head moves with the change of Tools, so the approval of `caaea8f` no longer covers the head.
- `make build`, `make test` (2524 tests), `make format`, and `make ste-check` passed on macOS arm64.

### What is in flight

- The CI run of this push, then `make codex-review PR=68 -- --skip-gitar-review`.
- After an approval, the merge question to the owner in four sections (D-942).

### Traps and gotchas

- `CLAUDE.md` and `AGENTS.md` are 16382 bytes, 2 bytes under the limit of 16 KB. A later rule line needs a cut elsewhere.
- The stopped review of round 2 pushed its approval before the stop. The new head needs a new review.

### The questions that block progress

None.

### The next concrete action

Wait for CI on the head, read the Gitar output one time, then run `make codex-review PR=68 -- --skip-gitar-review`.
