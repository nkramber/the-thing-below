# Session handoff

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

## Session 171: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-66, the answer to the Gitar pass. Repository: the-thing-below. Branch: `feat/pr-66-elements-statuses`. PR: #49. Role: author. Base: `74c3a64`.

### What this session did, and why

- Opened PR #49 at `911a7d3`. The Gitar pass of that head approved with one finding, and its check passed at 16:57:09Z.
- The finding had merit: a stun on the character whose turn is open began that turn again, so poison, bleed, or regen acted two times.
- Commit `34e6272` makes `GiveStatus` refuse that stun, because no strike reaches the character whose turn is open. The test `AStunOnTheCharacterWhoseTurnIsOpenIsAnErrorAndChangesNothing` proves it.
- `GiveStatus` takes no log now, because it no longer runs the loop.

### The state of the build

- 1625 tests pass. The identity hashes do not change.
- At `911a7d3`, every CI job passed except `review-gate`, which waits for the review record of RG 3.

### What is in flight

The push of this round waits for a current Gitar pass. Then the PR goes to the review of Codex (D-401).

### Traps and gotchas

- In play, no strike reaches the character whose turn is open. PR-12 keeps that true, or it asks the owner for the rule of a stun on the actor.
- The `review-gate` fault of RG 3 clears only with `docs/reviews/pr-49.md`.

### The questions that block progress

None.

### The next concrete action

Reply on the Gitar thread with `34e6272`, prove the next pass current, and hand the PR to Codex.

## Session 170: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-66, the elements and the statuses. Repository: the-thing-below. Branch: `feat/pr-66-elements-statuses`. PR: the PR of PR-66, which this round opens. Role: author. Base: `74c3a64`.

### What this session did, and why

- Asked the owner 22 start questions, and recorded the answers as D-790 to D-811. D-533 is revised in part by D-792.
- Moved the gear table to PR-13, the aptitude bonus to PR-12, and the icons to PR-10 (D-790, D-791, D-811).
- Added the element table and the immune list to each enemy record, and the ten statuses to each combatant, on the timeline.
- Poison, blind, and silence stay on each character after a fight, in save format 5 with a reader of format 4.
- Raised the simulation version to 9, and added the identity run `statuses`.
- Added the tests of the five exit tests, with seed loops of 1000 seeds.

### The state of the build

- `make verify` passed before the commits, with 1625 tests. The later edits touched comments and one blank line.
- The remote head is `74c3a64` on `main`. This round pushes the branch and opens the PR.

### What is in flight

The PR waits for the Gitar pass, then for the review of Codex, because it adds decisions (D-401).

### Traps and gotchas

- A turn now begins before the choice of a character: the timeline moves, statuses end, shares act, and a sleeper passes. `Act` reads the open turn.
- A stun on the character whose turn is open ends that turn, and `GiveStatus` runs the loop again.
- PR-66 changes no screen, so the visual review of D-784 has no frame to read.
- The perl edits of this session broke two files on an unbalanced brace. Use the Edit tool for C# blocks.

### The questions that block progress

None.

### The next concrete action

Push, open the PR, and follow the `gitar-review` skill.

## Session 169: 2026-09-21, Codex

Author: Codex
Session: repeat review PR-48, the enemy record correction. Repository: the-thing-below. Branch: `feat/pr-80-enemy-record`. PR: #48. Role: reviewer. Base: `86528a3`.

### What this session did, and why

- Reopened PR #48 after the author answered P1-1 at effective head `ef02f4a`.
- Read the response file, recomputed the effective head, inspected the full correction diff, and verified the original mismatch trigger and the waiting-enemy boundary.
- Ran `make verify`. It passed with 1587 tests and all local gates.
- Updated `docs/reviews/pr-48.md`: P1-1 is fixed, and the verdict is `Ready for owner merge` for `ef02f4a`.

### The state of the build

- The effective head is `ef02f4a`. The remote metadata tip is `ebffa5e`.
- GitHub CI and Gitar pass at `d887605c`. Review-gate waits for this updated review record.

### What is in flight

The repeat review record and this handoff entry are pushed at `ebffa5e`.

### Traps and gotchas

- The size check selects the largest enemy in the group, including waiting enemies, as D-788 requires.
- The review verdict targets `ef02f4a`, not the later metadata commits.

### The questions that block progress

None.

### The next concrete action

Commit and push the repeat review record and handoff entry. Then verify the remote head and review-gate result.

## Session 168: 2026-09-21, Claude Code

Author: Claude Code
Session: PR-80, the enemy record, the answer to the review. Repository: the-thing-below. Branch: `feat/pr-80-enemy-record`. PR: #48. Role: author. Base: `86528a3`.

### What this session did, and why

- Answered P1-1 of `docs/reviews/pr-48.md` with full merit: D-754 asks the enemy record for the size, and the load to fail a map that disagrees.
- Asked the owner which record of a group sets the size, and the size of each fixture enemy. The answers are D-788 and D-789.
- Commit `ef02f4a`: the `size` field of `EnemyRecord`, the size check in `BattleContent.RequireGroupsOf`, the records, the tests, and the documents.
- Wrote `docs/reviews/pr-48-response.md`.
- Gitar reviewed `f8cc578` at 15:50:12 UTC, after the push at 15:48:31 UTC: no issues, no review thread, 0 comments with merit. CI passes, and review-gate fails on RG 4 and RG 5 alone, which wait for the repeat review.

### The state of the build

- The effective head is `ef02f4a`. `make verify` passes with 1587 tests. The identity hashes stay, and the content hash changes.

### What is in flight

The repeat review by Codex of effective head `ef02f4a`.

### Traps and gotchas

- The test guard of `BattleRuns.Map` takes the size of its group. An elite guard holds an area of 3 by 2 tiles, because an area must leave room to move (D-209).
- The tests pair the checkout map with the test records, so the test brute stays elite, as in `content/`.

### The questions that block progress

None.

### The next concrete action

The owner starts a Codex session for the repeat review of PR #48 at `ef02f4a`. This author session answers each finding.

## Session 167: 2026-09-21, Codex

Author: Codex
Session: review PR-48, the enemy record. Repository: the-thing-below. Branch: `feat/pr-80-enemy-record`. PR: #48. Role: reviewer. Base: `86528a3`.

### What this session did, and why

- Reviewed PR #48 at effective head `fd97eb2`.
- Inspected the complete diff, the PR comments, the PR-80 roadmap scope and exit tests, the affected Core, content, test, identity, and document paths, and the D-754 contract.
- Found that the enemy record omits body size and that content loading does not compare map patrol size with the enemy record.
- Added `docs/reviews/pr-48.md` with finding P1-1 and the verdict `Changes required`.

### The state of the build

- The effective head is `fd97eb2`. The current branch tip is metadata commit `4385650`.
- `make verify` passes locally with 1583 tests. GitHub CI and Gitar pass at `2a8da97`, except `review-gate`, which waits for the review record.

### What is in flight

The review record and this handoff entry are pushed at `4385650`. The author must add the D-754 size field and the map-to-record consistency test and validation.

### Traps and gotchas

- `Patrol` already stores the map size, but `EnemyRecord` has no size member.
- A metadata commit does not move the effective head. The review targets `fd97eb2`, not `2a8da97`.

### The questions that block progress

None.

### The next concrete action

Commit and push the review record and handoff entry. Then the author answers P1-1 in a new round.

## Session 166: 2026-09-21, Claude Code

Author: Claude Code
Session: PR-80, the enemy record, the Gitar round. Repository: the-thing-below. Branch: `feat/pr-80-enemy-record`. PR: #48. Role: author. Base: `86528a3`.

### What this session did, and why

- Opened PR #48 at `fd97eb2`, and ran the push wait and the Gitar wait (D-586, D-705).
- The Gitar pass is current: the head is `fd97eb2`, and the dashboard edit at 14:55:50 UTC comes after the push at 14:52:59 UTC. The review found no issues, and the PR has no review thread.
- Answered the CI note of the dashboard in a PR comment: the review-gate check fails on RG 3 alone, because no review record exists yet.

### The state of the build

- The effective head is `fd97eb2`. This entry is a metadata commit, and it does not move the effective head (D-610).
- CI at `fd97eb2`: build, test, and format, smoke, det-lint, replay-identity, screen-test, and ste-check pass on every leg. The review-gate check waits for `docs/reviews/pr-48.md`.

### What is in flight

The Codex review of PR #48. The PR adds D-785 to D-787, so the `review-override` label does not apply (D-401).

### Traps and gotchas

- Session 165 holds the traps of the change: the ordinal order of the enemy files, and the check of a group entry against a record in another file.

### The questions that block progress

None.

### The next concrete action

The owner starts a Codex session to review PR #48 at effective head `fd97eb2`. This author session answers each finding of that review.

## Session 165: 2026-09-21, Claude Code

Author: Claude Code
Session: PR-80, the enemy record. Repository: the-thing-below. Branch: `feat/pr-80-enemy-record`. PR: the one PR of PR-80, with no GitHub number at this commit. Role: author. Base: `86528a3`.

### What this session did, and why

- Asked the owner three start questions, and recorded the answers as D-785 to D-787. No file held an ability id, so exit test 3 had no list to check.
- Added the ability file `content/rules/abilities.json` and one record file for each enemy under `content/rules/enemies/`. The two fixture enemies moved there with the same ids.
- Core: `AbilityList` and `EnemyRecord` read the files. `BattleContent` now holds the records and the ability file, and it checks each id between the files.
- The identity set gains the `enemy-record` run. The simulation version goes from 7 to 8.
- Tests: `EnemyRecordTests` holds exit tests 1 to 4, and the identity file holds exit test 5.
- Docs: the PR-80 entry, a PR-12 scope line, area-battle section 7.7, a design pass line, and two glossary rows.

### The state of the build

- The local head is the commit of this entry, on base `86528a3`. `make verify` passes with 1583 tests.
- At version 7 the old runs give their old hashes, so the move of the enemies changes no fight. The version bump alone moves `battle`, `replay`, and `state-hash`.

### What is in flight

The push, the PR, and the gitar pass. The PR changes decision rows, so it goes to Codex for review.

### Traps and gotchas

- The content set reads files in the ordinal order of the paths. A repeated enemy id thus fails on the file with the later path.
- A group entry and its enemy record now lie in two files. `BattleContent` checks the id, and the error names the battle fixture file.
- No screen changes, so the visual review of D-784 does not apply.

### The questions that block progress

None.

### The next concrete action

Push the branch, open the PR, and follow the `gitar-review` skill.

## Session 164: 2026-09-21, Codex

Author: Codex
Session: review PR-47, the ground draw order, walk captures, fullscreen launch, and frame fit. Repository: the-thing-below. Branch: `fix/pr-89-walk-texture`. PR: #47. Role: reviewer. Base: `ce06eda`.

### What this session did, and why

- Reviewed PR #47 at effective head `4c1bdec`.
- Inspected the complete diff, the PR description and comments, the PR-89 roadmap scope and exit tests, the affected Game and Tests paths, the capture baselines, and the changed documents.
- Verified the ground-layer order, deterministic walk capture sequence, fullscreen startup, 1080-row frame fit, baseline coverage, and the visual-review record.
- Added `docs/reviews/pr-47.md` with the verdict `Ready for owner merge`.

### The state of the build

- Remote head of the PR branch: `4c1bdec`.
- `make verify` passes with 1555 tests, no failures, and no skips.
- GitHub CI and Gitar pass at `4c1bdec`. The review-gate check waits for this review record.

### What is in flight

The review record and this handoff entry need a push. After the push, the owner can wait for review-gate and merge the PR.

### Traps and gotchas

- The review target is `4c1bdec`, not the metadata commit that publishes this record.
- The capture session stays windowed so it can set exact screen sizes. Only the play session enters borderless fullscreen.

### The questions that block progress

None.

### The next concrete action

Push the review record and handoff entry. Verify the remote head and the review-gate result.
