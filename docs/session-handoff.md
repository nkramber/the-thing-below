## Session 253: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #68, round 1. Repository: the-thing-below. Branch: `feat/pr-98-waiting-enemies`. PR: #68. Role: reviewer. Base: `3223bcf`.

### What this session did, and why

- Reviewed PR #68 from base `3223bcf` through effective head `caaea8f`.
- Verified Claude Code authored the changes and Codex passes the provider gate (T-4, D-17).
- Checked the load limit, the screen layout, the step-in order, the target menu, the simulation version, and the replay identity.
- Ran `make verify`, checked CI, and read all 15 affected battle frames of the screen-test artifact.
- Found no code defect. The existing Gitar status comment has no author answer. The review is Blocked until the author answers it.
- Corrected the Documents row of the PR description and added `docs/reviews/pr-68.md`.
- Committed and pushed the review and handoff as metadata commit `52aa9a8`. The fresh review-gate check failed RG 4 because the verdict is Blocked.

### The state of the build

- `main` and the PR base are `3223bcf`. The effective head is `caaea8f`.
- `make verify` passed on macOS arm64 with 2,522 tests. CI run 35920519185 passed the implementation checks on each platform.
- The initial `review-gate` check failed because this record did not exist. After publication, RG 3 and RG 5 to RG 8 passed. RG 4 failed because the verdict is Blocked.
- CI run 35921902597 passed the metadata checks. The platform matrix jobs skipped because the commit changed metadata paths alone.

### What is in flight

- The author needs to answer the existing Gitar status comment.
- The owner needs to start a fresh review after the author answers.

### Traps and gotchas

- The Gitar comment says “Gitar is working.” It has no thread or finding, but the user requires an answer to each existing Gitar comment.
- The Gitar pass itself is not a review condition under D-945 and D-946.

### The questions that block progress

OQ-243 is resolved by D-963. The unanswered Gitar status comment blocks approval.

### The next concrete action

The author answers the Gitar status comment. Then start a fresh review of PR #68.

## Session 252: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-98, round 1. Repository: the-thing-below. Branch: `feat/pr-98-waiting-enemies`. PR: #68. Role: author. Base: `3223bcf`.

### What this session did, and why

- Asked the owner OQ-243. The session found that four elite bodies fit in the waiting column, and not six as D-954 said. The owner chose the load failure (D-963).
- Recorded D-963, marked D-954 as revised in part, resolved OQ-243, and updated `docs/design.md` and both roadmaps.
- Core: the load refuses a group whose waiting column is taller than 288 art pixels. The error names the group, the height, and the limit. The simulation version is 18, and the identity file changed with it (G-17).
- Game: each waiting enemy draws at full size in the dim color, in a column at the left edge. The column stands on the bottom of its room, and the next enemy that steps in stands at the top. The rows of the enemies stand 40 columns to the right.
- Read the frames `battle-waiting-1x`, `battle-sparks-1x`, and `battle-target-1x` of `make sheet FIXTURE=battle` (D-784). A column in the middle of its room put a lone grunt in the wall above the ground, so the column now stands on the bottom. Exit test 3 of the roadmap changed with it.
- Added the `waiting-1x` capture of the fight of the deep room, and tests of the load check, the column, the step in, and the target menu.

### The state of the build

- `main` is `3223bcf`. The branch holds the code, the tests, and the documents of PR-98.
- `make verify` passed on macOS arm64, except the baseline of `battle-waiting-1x.png`, which only the CI artifact gives (D-733).
- CI run 35919855654 passed smoke, replay-identity, det-lint, and ste-check on each leg. The build legs and screen-test failed on the new battle frames alone. The 15 battle frames of its `screen-captures` artifact are the new baseline, and the author read each one (D-784).

### What is in flight

- The CI run on the baseline commit.
- The Codex review: `make codex-review PR=68 -- --skip-gitar-review` (D-946).

### Traps and gotchas

- The patrol of a test map takes the size of the largest enemy of the tests content, so a test wave with a brute fails the map. The wave test uses grunts alone.
- The elite group of the tests holds its waiting grunts in slots 3 and 4. The group of the content holds its waiting grunt in slot 1.
- The waiting grunt reads very dark: the grunt art is dark, the dim color multiplies it, and the left edge takes little of the key light. The owner judges the shade from the sheet.

### The questions that block progress

None.

### The next concrete action

Read the CI result of the baseline commit, read the Gitar output one time, then run `make codex-review PR=68 -- --skip-gitar-review` in the background (D-945, D-946).

## Session 251: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #67, round 3. Repository: the-thing-below. Branch: `feat/pr-11-evaluator`. PR: #67. Role: reviewer. Base: `d429d03`.

### What this session did, and why

- Re-reviewed PR #67 through effective head `a1f145d` after the cost correction and Deck measurement.
- Verified P2-1 is fixed at `ad83e20`. The author recorded two Deck runs at 74 us and 103 us, both below the D-961 limit.
- Verified the answer to the existing Gitar status comment. It has no thread or finding.
- Updated the review record, corrected the test count and Documents row in the PR description, and set the verdict to Ready for owner merge.

### The state of the build

- `main` and the PR base are `d429d03`. The effective head is `a1f145d`.
- `make verify` passed on macOS arm64 with 2,507 tests. CI run 35911353733 passed the shared checks. Platform implementation jobs passed at `ad83e20`.
- The review-gate check failed against the prior blocked review record. A fresh result after this metadata commit is required.

### What is in flight

- The review record and this handoff entry need one metadata commit and a push to `feat/pr-11-evaluator`.
- Read the new review-gate result after the push.

### Traps and gotchas

- The effective head is `a1f145d`, because the Deck procedure and F-53 changed outside the metadata set.
- The Deck result comes from two Release runs of the cost command on the code at `ad83e20`.

### The questions that block progress

OQ-243 applies to PR-98 alone (D-951 to D-954).

### The next concrete action

Run the metadata commit and push gate, then confirm the published review-gate result.

## Session 250: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-11, round 3. Repository: the-thing-below. Branch: `feat/pr-11-evaluator`. PR: #67. Role: author. Base: `d429d03`.

### What this session did, and why

- Read the record of Session 249: `Blocked` for `ad83e20`, with P2-1 fixed and no open finding.
- Answered the Gitar status comment in a PR comment. It held no thread, no finding, and no claim (D-945, D-946).
- Ran `evaluator-cost` on the Steam Deck over SSH, at the request of the owner. Two Release runs gave 74 us and 103 us at the 95th percentile, inside the limit of D-961.
- Recorded the numbers in F-53 and the response file, and wrote the SSH steps into `docs/runbooks/dev-machine.md`.

### The state of the build

- `main` is `d429d03`. The effective head `ad83e20` holds the code. The commit of this round changes documents alone (D-943).

### What is in flight

- A new `make codex-review PR=67 -- --skip-gitar-review` on the record of this round.
- After an approval, the merge question to the owner in four sections (D-942).

### Traps and gotchas

- The command sandbox blocks the local network. An SSH call to the Deck at `10.0.0.46` runs outside the sandbox.
- The Deck has no `make`, so the Deck runs the `dotnet run` form of each target.

### The questions that block progress

None for PR-11. OQ-243 blocks PR-98.

### The next concrete action

Run `make codex-review PR=67 -- --skip-gitar-review`, and read its outcome.

## Session 249: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #67, round 2. Repository: the-thing-below. Branch: `feat/pr-11-evaluator`. PR: #67. Role: reviewer. Base: `d429d03`.

### What this session did, and why

- Re-reviewed PR #67 from merge base `d429d03` through effective head `ad83e20`.
- Verified that `BattleTurns.EnemyAct` measures the whole enemy action on a copy of the run, and that the regression test checks the copy boundary.
- Closed P2-1 as fixed in `ad83e20`. The Deck measurement and the answer to the existing Gitar comment remain outstanding.
- Updated `docs/reviews/pr-67.md` with a blocked verdict for the unresolved merge requirements.

### The state of the build

- `main` and the PR base are `d429d03`. The remote PR head before this metadata commit is `ad83e20`.
- `make verify` passed on macOS arm64 with 2,507 tests. CI run 35908764586 passed the implementation checks on every platform. The old review record caused `review-gate` to fail.

### What is in flight

- The review record and handoff entry are on the PR branch in commit `d0eca6a`. The fresh review-gate check is pending.
- The owner needs the Steam Deck measurement from `make evaluator-cost` (D-961). The author needs to answer the existing Gitar comment.

### Traps and gotchas

- The correction fixes the timed work. The test proves that the timer call changes only its copy of the run.
- The `review-gate` failure reads the old record. The next run must read this record on the metadata tip.

### The questions that block progress

OQ-243 applies to PR-98 alone (D-951 to D-954). The Steam Deck measurement and the answer to the Gitar comment remain required for PR-67.

### The next concrete action

Read the fresh review-gate result. The owner needs to supply the Deck measurement and the author needs to answer the Gitar comment.

## Session 248: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-11, round 2. Repository: the-thing-below. Branch: `feat/pr-11-evaluator`. PR: #67. Role: author. Base: `d429d03`.

### What this session did, and why

- Read CI on `f92eb3e`: each check passed except `review-gate`, which waited for the record.
- Read the Gitar output one time: a notice alone, with no thread and no finding (D-945).
- Ran `make codex-review PR=67 -- --skip-gitar-review`. The outcome was `changes-required` with P2-1.
- Answered P2-1 with full merit in `docs/reviews/pr-67-response.md`. The cost command now times the whole enemy turn on a copy of the run, through the new `BattleTurns.EnemyAct`.
- The Mac gives 52 us at the 95th percentile for a whole enemy turn.

### The state of the build

- `main` is `d429d03`. The record of Session 247 gives `Changes required` for `f92eb3e`. The correction commit of this round follows it.

### What is in flight

- CI on the correction, one Gitar read, and a new `make codex-review PR=67 -- --skip-gitar-review`.
- The owner run of `make evaluator-cost` on the Deck, before the merge (D-961).

### Traps and gotchas

- `BattleTurns.EnemyAct` plays one enemy action for any enemy on the field. The rules call the private turn for the enemy whose turn begins.
- The slowest sample of the command can pass 1 ms on the Mac. The limit reads the 95th percentile alone (D-961).

### The questions that block progress

None for PR-11. OQ-243 blocks PR-98.

### The next concrete action

Read CI on the correction commit, read the Gitar output one time, then run `make codex-review PR=67 -- --skip-gitar-review`.

## Session 247: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #67, round 1. Repository: the-thing-below. Branch: `feat/pr-11-evaluator`. PR: #67. Role: reviewer. Base: `d429d03`.

### What this session did, and why

- Reviewed PR #67 from merge base `d429d03` through effective head `f92eb3e`, across 75 changed paths.
- Confirmed Claude Code authored the substantive changes, so the Codex reviewer passes the provider gate (T-4, D-17).
- Inspected the evaluator, content readers, save migration, identity run, UI changes, cost tool, all changed paths, and the seven changed battle frames.
- Found that `evaluator-cost` times `BattleEvaluator.Choose` alone, although D-961 limits a complete enemy turn. Recorded P2-1.
- The existing Gitar status comment has no author answer. The Deck cost run also remains pending (D-961).
- Added `docs/reviews/pr-67.md` with `Changes required` for `f92eb3e`.

### The state of the build

- `main` and the merge base are `d429d03`. The remote PR head before this metadata commit is `f92eb3e`.
- `make verify` passed on macOS arm64 with 2,506 tests. CI run 35907108806 passed the implementation checks and screen-test on `f92eb3e`.

### What is in flight

- The author must correct P2-1, answer the Gitar status comment, and provide the Deck measurement before the merge.
- The PR needs another Codex review after a substantive correction.

### Traps and gotchas

- D-945 and D-946 remove the Gitar pass as a review condition. An existing Gitar comment still needs an answer.
- The cost tool measures action selection alone, so its current number is not a full enemy-turn measurement.

### The questions that block progress

OQ-243 blocks PR-98 alone (D-951 to D-954). No open question changes PR-11.

### The next concrete action

Correct the timed operation and its test, answer the existing Gitar status comment, and run the cost command on the Steam Deck.

## Session 246: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-11, round 1. Repository: the-thing-below. Branch: `feat/pr-11-evaluator`. PR: #67. Role: author. Base: `d429d03`.

### What this session did, and why

- Asked the owner OQ-127, OQ-128, OQ-129, and OQ-242, and six more questions of the shape of the PR. D-947 to D-962 record each answer.
- Built the evaluator of D-65 and D-534: the legal actions of an enemy, the expected score of D-959, the reply of D-960, and the tie draw from the new stream of D-947.
- Gave an enemy ability its effect (D-955), and moved the groups and the profiles to their own files (D-956, D-957). Each map names its region.
- Added save format 6 with the stream of the evaluator, and simulation version 17 (G-17).
- Added the `evaluator-cost` command and `make evaluator-cost` (D-961). The Mac gave 43 us at the 95th percentile, with 15 legal actions at most.
- Added PR-98 to the roadmap for the waiting enemies on screen (D-951 to D-954), and filed OQ-243.
- Opened PR #67. CI run 35906133812 passed each check except `screen-test` and `review-gate`. Seven battle captures took new numbers and a new strip, and the baseline took them from the artifact (D-733).
- Read the Gitar output one time: the notice of the free plan alone, with no thread and no finding (D-945).

### The state of the build

- `main` is `d429d03`. The PR branch holds the decisions commit `8953836` and the code commit of this round.
- `make verify` passed on the Mac. The fixture pair wins 300 of 300 fights, and the fixture elite 101 of 300, with the attack alone.

### What is in flight

- CI on the baseline commit, then `make codex-review PR=67 -- --skip-gitar-review`.
- The owner run of `make evaluator-cost` on the Deck, before the merge (D-961).

### Traps and gotchas

- The screen tests can differ, because the enemy turns changed. The baseline comes from the CI artifact alone (D-733).
- `CLAUDE.md` is 7 bytes under the SIZE 1 limit, so the new target lives in `docs/runbooks/dev-machine.md` alone.
- The test helper `BattleRuns.Map` names the region of the group from the test content.

### The questions that block progress

None for PR-11. OQ-243 blocks PR-98.

### The next concrete action

Read CI on the baseline commit, read the Gitar output one time, then run `make codex-review PR=67 -- --skip-gitar-review`.

## Session 245: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-97, hand-over. Repository: the-thing-below. Branch: `feat/pr-97-gitar-pause`. PR: #66. Role: author. Base: `919547b`.

### What this session did, and why

- Read CI run 35894484623 on `d5ca603`. Each check passed except `review-gate`, which waited for the record.
- Read the Gitar output one time under D-945. It held a notice of the free plan alone, with no thread, no finding, and no claim. Thus no stop.
- Ran `make codex-review PR=66 -- --skip-gitar-review`. The command skipped the Gitar check (D-946), and the outcome was `approve`.
- Read the record of Session 244: `Ready for owner merge` for the effective head `d5ca603`, with no finding.

### The state of the build

- `main` is `919547b`. The effective head is `d5ca603`. Each later commit changes the metadata set alone.

### What is in flight

- The confirmation of the owner, then the auto-merge of PR #66 (D-930).
- After the merge, the transitional prompt of step 6.

### Traps and gotchas

- The pause of D-945 holds after the merge. A Gitar thread or finding stops each session at once.
- Each run of the review command takes `-- --skip-gitar-review` while the pause holds.

### The questions that block progress

None.

### The next concrete action

Read the Gitar output one time, post the summary in four sections inside the merge question, and turn on the auto-merge after the confirmation of the owner.

## Session 244: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #66, round 1. Repository: the-thing-below. Branch: `feat/pr-97-gitar-pause`. Role: reviewer. Base: `919547b`.

### What this session did, and why

- Reviewed PR #66 from merge base `919547b` through effective head `d5ca603`, across 20 changed paths.
- Confirmed Claude Code authored the substantive changes, and Codex passes the provider gate (T-4, D-17).
- Verified the Gitar skip flag, its refusal behavior without the flag, its prompt text, and the Make target (D-945, D-946).
- Found no in-scope defect. Added `docs/reviews/pr-66.md` with `Ready for owner merge` for `d5ca603`.
- The only Gitar comment is a free-plan notice with no review claim or finding.

### The state of the build

- `main` and the merge base are `919547b`. The remote PR head before this metadata commit is `d5ca603`.
- `make verify` passed on this machine with 2,451 tests. CI run 35894484623 passed its implementation checks on `d5ca603`.
- `review-gate` failed before publication because the review record was absent. Fresh run 35896052282 passed on metadata tip `5420609`; metadata CI run 35896057841 passed the applicable checks.

### What is in flight

- This metadata commit holds the review record and this entry. The remote head and fresh checks are verified.

### Traps and gotchas

- The Gitar pass is not a condition of this review (D-945, D-946).
- The PR description now marks `docs/reviews/pr-66.md` as changed.

### The questions that block progress

None.

### The next concrete action

The owner can review the verdict and merge PR #66.
