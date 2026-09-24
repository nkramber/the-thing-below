## Session 271: 2026-09-24, Codex

Author: Codex
Session: reviewer PR #74, round 1. Repository: the-thing-below. Branch: `feat/pr-13-gear-items`. PR: #74. Role: reviewer. Base: `b2bc579`.

### What this session did, and why

- Reviewed PR #74 from merge base `b2bc579` through effective head `15cc89c`.
- Confirmed Claude Code authored the PR, so Codex passes the provider gate (T-4, D-17).
- Traced gear, items, pack limits, steal and drop behavior, save migration, replay state, menus, and content validation across all 132 changed paths.
- Read all 28 changed frames from the CI screen-captures artifact. No visual fault was found (D-733, D-784).
- Added `docs/reviews/pr-74.md` with `Ready for owner merge` for `15cc89c`. Corrected the Documents row of the PR description.
- `make verify` passed with 3,135 tests.

### The state of the build

- Base and merge base: `b2bc579`. Effective head and remote head before this metadata commit: `15cc89c`.
- CI run `36023732533` passed build, tests, format, smoke, replay identity, det-lint, coverage, STE, and screen-test. `review-gate` failed because the review record was absent.

### What is in flight

- This metadata commit holds the review record and this entry. Fresh review-gate evidence must pass after publication.

### Traps and gotchas

- The Gitar comment is only a status notice. It has no item and needs no answer (D-964). The owner requested `--skip-gitar-review` (D-946).
- The changed frame review used the configured CI artifact, not local rendering.

### The questions that block progress

None for PR-13. OQ-247 and OQ-248 remain with PR-99.

### The next concrete action

Push the metadata commit to `feat/pr-13-gear-items`, fetch, confirm no commits are ahead, and verify the remote head and fresh `review-gate` result.
## Session 270: 2026-09-24, Claude Code

Author: Claude Code
Session: author PR-13, round 1. Repository: the-thing-below. Branch: `feat/pr-13-gear-items`. PR: #74. Role: author. Base: `b2bc579`.

### What this session did, and why

- Asked OQ-140 to OQ-143 and each question of the scope. D-1036 to D-1050 record the answers. OQ-247 and OQ-248 are new, and PR-99 holds them with the lesson swap anywhere (D-1041, D-1050).
- Built the item file, the gear file, the six gear slots, the pack of items and spare gear with a stack limit for each record, the gold, the four item effects, the steal of a Theft drill, and the drops of a win in Core.
- Built the gear window, the item window, the stats with the gear in the status window, the lines of the new events, and the debug command `stock`.
- Raised the simulation version to 23 and the save format to 11, with a stored save of format 11. Rewrote the content hash and the identity file.
- The owner added gear to the steal list: the gear chance of the profile under a cap of 5%, 15%, or 25% for each success, the check of room, and a gold entry that comes back (D-1051).
- The smoke session walks the two new entries of the main list.

### The state of the build

- Base `b2bc579`. `make format`, `make lint`, `make smoke`, and the STE check pass. 3,135 tests: the only failures are the three baselines of the new menu captures. CI run 36020519177 failed the smoke walk, which this round repairs.

### What is in flight

- The PR takes 28 baselines from the artifact of CI run 36020519177, each read first. Its two sessions differed by one level in `map-fire-1x`, `scroll-09`, `still-240`, and `battle-spell-full-1x`, the flake of OQ-246. The PR leaves the first three baselines alone. The rerun of run 36021738678 agreed across its two sessions and matched every baseline but `battle-spell-full-1x`, which the PR then took from that run.

### Traps and gotchas

- `ContentId` compares by reference. A test compares the values (F-39).
- The fixture of the tests gives Marrek no gear and profiles no drops, so the older battle numbers stay.
- The fixture of the checkout gives Marrek the pick and the coat, so the menu and battle captures change.

### The questions that block progress

None for PR-13. OQ-247 and OQ-248 block PR-99.

### The next concrete action

Wait for CI on the new head, and rerun the screen test on a flake of OQ-246. Then run `make codex-review PR=74 -- --skip-gitar-review`.

## Session 269: 2026-09-24, Codex

Author: Codex
Session: reviewer PR #73, round 1. Repository: the-thing-below. Branch: `feat/pr-12-lessons`. PR: #73. Role: reviewer. Base: `5f1164c`.

### What this session did, and why

- Reviewed PR #73 from merge base `5f1164c` through effective head `2d08989`. The author is Claude Code, so Codex passes the provider gate (T-4, D-17).
- Traced lesson loading and validation, battle and menu use, lesson growth, swap ownership, save migration, replay state, spell effects, and the affected UI flows. Inspected all 144 changed paths.
- Read the nine affected CI screen captures. No visual fault was found (D-733, D-784).
- Ran `make verify`: all 3,010 tests and local checks passed. CI run 36004087356 passed the implementation checks on macOS, Ubuntu, and Windows. The rerun screen test passed.
- Added `docs/reviews/pr-73.md` with `Ready for owner merge` for `2d08989`.

### The state of the build

- Base: `5f1164c`. Effective head: `2d08989`. Remote PR head before this metadata commit: `a4987ec`.
- CI run 36004087356 passed build, test, and format; smoke; replay identity; det-lint; STE; coverage; and the screen test at the effective head. Run 36005990820 passed implementation checks at the metadata tip. Review-gate awaits this record.

### What is in flight

- This metadata commit holds the review record and this entry. After publication, a fresh review-gate result must pass.

### Traps and gotchas

- The only Gitar comment says it is working and has no item, so it needs no answer and does not block the verdict (D-964). The user requested `--skip-gitar-review`; no Gitar pass was required (D-945, D-946).
- OQ-246 records a known one-level screen-test flake. The rerun of the reviewed commit passed (D-733).

### The questions that block progress

None for PR-12. OQ-245 blocks PR-42 only. OQ-246 blocks no PR.

### The next concrete action

Fetch the branch, verify the pushed metadata head and the fresh `review-gate` result, then report the review verdict for PR #73.

## Session 268: 2026-09-24, Claude Code

Author: Claude Code
Session: author PR #73, round 1. Repository: the-thing-below. Branch: `feat/pr-12-lessons`. PR: #73. Role: author. Base: `5f1164c`.

### What this session did, and why

- Asked OQ-137 to OQ-139 and 13 follow-up questions. D-1018 to D-1035 record each answer, and OQ-245 holds the Guard effect.
- Core: the lesson file, the eight kinds, the cure and the boon, the slots, the owned lesson set, the swap place, the lesson points with the lesson-level shrink, the aptitude bonus, the lesson use, and the cast from the menu. Save format 10 and simulation version 22.
- Game: the Lessons command with its lesson and form lists, the command menu in two rows of three, the lesson window, and the flash of each spell at three levels.
- Tests: 3,010, with the lesson rules, the cursor, the spells, format 10, and the `lessons` identity run.

### The state of the build

- `main` is `5f1164c`. The branch holds the docs, the code, and this entry.
- CI run 36002016629 at `6a0d8d3` passed smoke, det-lint, replay identity, and STE on each leg. The tests failed on the seven missing baselines alone. The next commit adds 35 reviewed baselines from its `screen-captures` artifact.

### What is in flight

- `scroll-09.png` differed by one level in 592 pixels of a dim band in two CI runs, with the same bytes. The local renderer draws it the same on `main` and on this branch, and no map code changed. The PR takes the CI frame as its baseline (D-733). The next run found one-level differences in `map-fire-1x` and `battle-spell-full-1x` too, and a rerun of that commit passed. OQ-246 holds the flake.
- The Codex review through `make codex-review PR=73 -- --skip-gitar-review` (D-945, D-946).

### Traps and gotchas

- The 1080-row captures fail on a screen shorter than 1080 rows. For a local `make sheet`, drop them from a copy of the capture list and put the file back.
- `ContentId` compares by reference. Compare `Value` with an ordinal comparison.
- A spread of an `IReadOnlyList` into an array makes Core call `System.Linq`, and the reference test fails.
- The test lessons take the checkout ids, so a checkout run replays on the test content.

### The questions that block progress

None. OQ-245 blocks PR-42, and OQ-246 blocks no PR.

### The next concrete action

When CI is green but for the review gate, run `make codex-review PR=73 -- --skip-gitar-review`. The Gitar comment so far is a status notice with no item (D-964).

## Session 267: 2026-09-24, Codex

Author: Codex
Session: reviewer PR #72, round 1. Repository: the-thing-below. Branch: `feat/pr-50-screenplay-tool`. PR: #72. Role: reviewer. Base: `d0bb297`.

### What this session did, and why

- Reviewed PR #72 at effective head `396fae9`. The author is Claude Code, so Codex passes the provider gate (T-4, D-17).
- Traced the screenplay command, batch, text output, body replacement, and base archive flow. No finding.
- Ran `make verify`; all 2,879 tests and local checks passed. CI implementation checks passed on each platform.
- Added `docs/reviews/pr-72.md` with `Ready for owner merge` for the effective head.

### The state of the build

- `main` is `d0bb297`. The effective head and remote head before this metadata commit are `396fae9`.
- CI run `35951520857` passed implementation checks on macOS, Ubuntu, and Windows. Run `35951520861` failed RG 3 because the review record did not exist yet.

### What is in flight

- This metadata commit holds the review record and this handoff entry. A fresh `review-gate` result must pass after publication.

### Traps and gotchas

- The Gitar comment is a free-plan status notice, with no item, so it does not block the verdict (D-964).
- The user requested `--skip-gitar-review`; no Gitar pass was required (D-945, D-946).
- Session 257 moved to `docs/session-handoff-archive.md` to keep the 10 newest sessions here.

### The questions that block progress

None.

### The next concrete action

Commit and push the review record and handoff together. Fetch, then verify the remote head and the new `review-gate` result.

## Session 266: 2026-09-24, Claude Code

Author: Claude Code
Session: author PR #72, round 1. Repository: the-thing-below. Branch: `feat/pr-50-screenplay-tool`. PR: #72. Role: author. Base: `d0bb297`.

### What this session did, and why

- Asked the owner four gaps that the story scene format of PR-68 leaves for the screenplay, and recorded D-1014 to D-1017. Each answer took the recommendation.
- D-1014 revises D-548 in part: PR-70 adds the cue look-up, because no audio file format exists before PR-38 and PR-70.
- Built the `screenplay` command of Tools: the batch against the base folder (D-1015), the layout (D-1017), and the marked section of a PR body file (D-1016).
- Added `make screenplay BODY=<file>`, which fills the base folder with `git archive` from the merge base.
- Updated the PR-50 and PR-70 blocks, the tools, story, and audio area files, the design sequence, the glossary, the runbook, and the review contract.

### The state of the build

- `main` is `d0bb297`. The push of this round carries this entry.
- `make verify` passed on macOS arm64 with 2,879 tests, the smoke session included.

### What is in flight

- PR #72 waits for CI, one read of the Gitar output, and `make codex-review PR=72 -- --skip-gitar-review` (D-945, D-946).

### Traps and gotchas

- No story scene ships in content yet, so a run on this branch prints "No story scene changes".
- `CLAUDE.md` holds 16,382 of 16,384 bytes. The command of the screenplay lives in `docs/runbooks/dev-machine.md` for that reason.
- The body count reads UTF-16 units, which is never below the count of GitHub.

### The questions that block progress

None for PR-50.

### The next concrete action

Read the CI result of the pushed head, read the Gitar output one time, then run `make codex-review PR=72 -- --skip-gitar-review` in the background.

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
