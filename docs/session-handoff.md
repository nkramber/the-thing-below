## Session 294: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR-103, round 1. Repository: the-thing-below. Branch: `fix/pr-103-input-and-grace`. PR: the one PR intent of this branch, before GitHub gives a number. Role: author. Base: `5c1db06`.

### What this session did, and why

- The owner put more than one finding of the repository review of 2026-09-24 in one PR (D-1090). This PR holds P1-1, P2-6, P2-4, P3-2, and P3-5. Each one reproduced at `5c1db06`.
- P1-1: the menu action in a fight pauses the fight, with a dim and "Paused" above the hand-off (D-1083, F-116). The playback, the hand-off, and the battle screen count the world tick, so the pause holds them. No command menu opens under the pause, and no fight ends under it.
- P1-1, story path: the menu action, the map action, and the held step make no intent while a story scene runs.
- P2-6: a hold of a step ends when its last source comes up. A loss of the focus holds the world and forgets each hold (D-1084, F-117).
- P2-4: a step into a group inside its grace time starts no encounter (D-1085, F-118). P3-2: a move intent ends with its tick (F-119). The simulation version rises to 27, and the identity file changes with it.
- P3-5: the crash message draws on a new top layer above the hand-off, and a resize after a crash builds nothing (F-120).
- `ScreenHandOffTests.AFightOnTheTickAfterTheWaitIntentStartsItsTransition` stepped into the fled group after the wait intent, which D-1085 refuses. The test now steps into another patrol.

### The state of the build

- `make build`, `make test` (3272 passed), `make format`, `make lint`, `make identity`, `make content`, `make ste-check`, and `make smoke` pass on this machine. The smoke session holds its fight paused for 120 frames.
- The remote head holds this entry.

### What is in flight

- The first Gitar pass and the CI of the first push.

### Traps and gotchas

- Perl with the `|` delimiter and a `\|` in the pattern reads an alternation, and it wrote text at the head of `Boot.cs`. Use the Edit tool for C# code.
- The screen-test baselines hold no capture of the pause. The fight clock changes no capture while no menu opened before the fight.
- The engine sends a focus notification before `Boot._Ready` opens the log. The first build of the focus rule crashed there, and only a session with a window showed it. The smoke session has no window, so `make sheet` found it.
- `make sheet` fails on `main` too: 108 captures make a sheet taller than 65535 pixels. The capture session itself writes every frame.

### The questions that block progress

None for this PR. The owner asked for suggestions on P2-2, P2-3, P2-5, P3-7, P3-9, P3-18, P3-19, P3-20, P3-27, and P3-34, and each next PR asks them first.

### The next concrete action

Poll the Gitar check of the first push, and answer each Gitar item.

## Session 293: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR #79 (PR-102), round 4. Repository: the-thing-below. Branch: `fix/pr-102-gate-hygiene`. PR: #79. Role: author. Base: `5b42cf0`.

### What this session did, and why

- Gitar approved the merge head `9501183` with no new finding. The CI claim of RG 3 has its answer.
- Every CI check passed on `9501183` except RG 3, `screen-test` included.
- `make codex-review PR=79` ran. The record `docs/reviews/pr-79.md` gives `Ready for owner merge` for `9501183` with no finding, and `review-gate` passed on `81769c3`.
- The command gave `fault`: it named `f81f010` as the effective head. `BranchCommits` runs `git log --name-only`, and git lists no path for a merge commit by default. With `--first-parent -m`, the merge lists the files of PR-101. The gate reads the merge through `git show` and passed.
- The report of the owner marks P2-1, P3-10, P3-11, P3-29, P3-30, P3-31, and P3-32 as complete in PR #79, and P1-2 as the decision D-1082.

### The state of the build

- Effective head `9501183`. The remote head holds this metadata commit.

### What is in flight

- The merge question to the owner (D-933, D-942).

### Traps and gotchas

- A merge of `main` into a PR branch makes `make codex-review` give `fault` until `BranchCommits` reads merge commits. The owner put that fix in the PR of finding P3-9 (D-1089).
- The session asks the merge question only after every check of the current tip is green and its Gitar pass is complete.

### The questions that block progress

None.

### The next concrete action

Wait for green CI and the Gitar pass of the tip, then ask the owner to confirm the merge. Then turn on the auto-merge under `docs/runbooks/merge.md`.

## Session 292: 2026-09-25, Codex

Author: Codex
Session: reviewer PR #79, round 1. Repository: the-thing-below. Local branch: `review/pr-79`; PR branch: `fix/pr-102-gate-hygiene`. PR: #79 (PR-102). Role: reviewer. Base: `5b42cf0`.

### What this session did, and why

- Reviewed all 29 changed paths from merge base `5b42cf0` through effective head `9501183`.
- Verified the Gitar case-variant fix and the author answers to the RG 3 CI claims.
- No in-scope defect was found. The review record gives `Ready for owner merge` for `9501183`.

### The state of the build

- The remote code head is `9501183`. `make verify` passed on macOS arm64: 3,256 tests and all local gates.
- CI passed build, test, format, smoke, det-lint, replay identity, export, screen-test, and STE. RG 3 failed because the review record was absent.

### What is in flight

- This review record and handoff entry will be committed together and pushed to the PR branch.
- A fresh `review-gate` check must read the published record.

### Traps and gotchas

- The RG 3 failure is the expected state before this review record reaches the PR head.
- The Gitar clean approval has no item and needs no answer (D-964).

### The questions that block progress

None.

### The next concrete action

Commit and push the review record and handoff entry. Fetch, confirm the remote head, and read the fresh review-gate result.

## Session 291: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR #79 (PR-102), round 3. Repository: the-thing-below. Branch: `fix/pr-102-gate-hygiene`. PR: #79. Role: author. Base: `5b42cf0` after the merge of `main`.

### What this session did, and why

- The owner merged PR #78 and asked for a merge of `main`, not a rebase (D-1088).
- PR #78 took D-1080, F-108, and Sessions 287 and 288. The ids of PR-102 thus rose by one: D-1081 to D-1088, F-109 to F-115, and Sessions 289 and 290.
- PR-102 moved to phase section 7.38 and to item 39 of the design sequence. Each later heading and item rose by one.
- Each "PR #78" in the records of PR-102 now reads PR-101.

### The state of the build

- Merge of `origin/main` at `5b42cf0`. D-1080 of PR-101 lets the screen compare allow one level on each channel, which covers the failure of D-1088.

### What is in flight

- This round pushes the merge. Then the Gitar poll, the CI checks with `screen-test`, and `make codex-review PR=79`.

### Traps and gotchas

- The owner can switch the shared checkout to another PR. Check the branch before each write.

### The questions that block progress

None.

### The next concrete action

Push, run the Gitar poll, and wait for green CI. Then run `make codex-review PR=79` in the background.

## Session 290: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR #79 (PR-102), round 2. Repository: the-thing-below. Branch: `fix/pr-102-gate-hygiene`. PR: #79. Role: author. Base: `c154ddd`.

### What this session did, and why

- Read the Gitar pass of `fa68db5`. It completed in 113 seconds and approved with one finding.
- Gitar finding: the settings refusal compared with case. macOS and Windows open `.claude/Settings.local.json` as the same file. The compare now ignores case, as Gitar proposed, with case-variant tests (D-1086).
- Gitar CI claim: `review-gate` gave one fault. The log shows RG 3 alone, because `docs/reviews/pr-79.md` waits for the review. No change.
- CI on Windows failed `AStopAtTheLimitEndsEachChildOfTheProgram`. The fault came after 30 seconds, because Windows loses the parent link of a child of Git Bash. The read of the output now has a bound of 5 seconds after the end or the stop (D-1087, F-115). A new test covers a child that holds the output after a normal end.

### The state of the build

- Round 1 head `fa68db5`: every CI job passed except the Windows test and RG 3. `make smoke` passed on the Mac.

### What is in flight

- Round 2 head `74b8020`: Gitar approved, and its thread is resolved. Each Gitar CI claim has its answer (RG 3 alone). Every CI job passed except `screen-test` and RG 3.
- `screen-test` failed 3 of 3 attempts in "Compare the two runs": `still-240` and `menu-status-1x` differ by one level between two captures of one job. `main` fails the same way since PR-91. The owner chose to wait for PR #78 (D-1088).
- PR-101 merged as `5b42cf0`.

### Traps and gotchas

- The owner switched the shared checkout at `/Volumes/SSD-1TB/the-thing-below` to PR #78. PR #79 waits in the worktree `/Volumes/SSD-1TB/the-thing-below-pr102`.
- PR #78 head `55ba1b2` lets the screen compare allow one level on each channel, which covers the difference of D-1088.
- The `review-gate` job runs the workflow of `main`, so the new facts step of F-110 first runs after the merge.
- The owner asked for a merge of `main`, not a rebase. The notes of Session 289 stand for the conflicts: phase section 7.38, design sequence item 39, and PR-101 in place of "PR #78".

### The questions that block progress

None.

### The next concrete action

Wait for the owner to confirm the merge of PR #78. Then check out this branch in the shared checkout, merge `origin/main`, push, and restart CI. Then the Gitar poll, a green `screen-test`, and `make codex-review PR=79` in the background.

## Session 289: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR #79 (PR-102), round 1. Repository: the-thing-below. Branch: `fix/pr-102-gate-hygiene`. PR: #79. Role: author. Base: `c154ddd`.

### What this session did, and why

- The owner gave a repository review report, outside the repository, with answers. For this task, a PR holds several findings (D-1081).
- The owner chose the gate findings alone, because PR #78 changes the input code of P1-1 and P2-6.
- Fixed P2-1 (F-109), P3-10 (F-110), P3-11 (F-111), P3-30 (F-112), P3-29 (F-113), P3-31 (F-114, D-1086), and P3-32 (F-115, D-1087).
- Recorded the answers on P1-2 (D-1082), P1-1 (D-1083), P2-6 (D-1084), and P2-4 (D-1085). No Game or Core code changed.
- A scratch repository reproduced P2-1 and P3-10 on the old step, and the new step passed. The 20 new test cases failed on the old code.

### The state of the build

- Base `c154ddd`. `make build test format lint identity content atlas` passed locally: 3235 tests. `make smoke` did not run, and CI runs it.

### What is in flight

- This round opens PR #79, and it waits for Gitar, then `make codex-review PR=79`.
- PR #78 (PR-101) is open. It uses D-1075 to D-1079, F-107, Sessions 284 to 286, and phase section 7.37.

### Traps and gotchas

- When PR #78 merges first, rebase. PR-102 moves to phase section 7.38 and to item 39 of the design sequence. Renumber each later item.
- Cite PR-101 in place of "PR #78" after that merge. A citation of PR-101 fails REF 1 until then.
- `.claude/settings.local.json` takes no backticks in a document, because REF 2 finds no such file.
- `CLAUDE.md` holds 16355 of 16384 bytes.

### The questions that block progress

None for PR-102. The owner asked for suggestions to approve on P2-2, P2-3, P2-5, P3-7, P3-9, P3-18, P3-19, P3-20, P3-27, and P3-34. The PR that takes each finding asks them.

### The next concrete action

Push, open PR #79, and run the Gitar poll of the `gitar-review` skill in the background.

## Session 288: 2026-09-25, Codex

Author: Codex
Session: reviewer PR-101, round 1. Repository: the-thing-below. Local branch: `review/pr-78`; PR branch: `fix/pr-101-torch-and-pad`. PR: #78. Role: reviewer. Base: `c154ddd`.

### What this session did, and why

- Reviewed the effective head `55ba1b2` of PR #78 against the PR-101 roadmap and its exit tests. No in-scope defect was found.
- Verified the Gitar halo finding's fix at `7aef6dc`. The current Gitar CI analysis reported the missing RG 3 review record; this session adds that record. The clean approval has no item (D-964).
- Read all 78 changed screen-test artifact frames from CI run `36094314170`. No visual fault was found.

### The state of the build

- Base and merge base `c154ddd`; effective head and remote code head `55ba1b2` before this metadata commit.
- `make verify` passed on this machine: build, 3,219 tests, format, det-lint, ste-check, replay identity, content hash, atlas, and smoke.
- CI run `36094314170` passed the implementation checks and Gitar. `review-gate` failed at RG 3 before this review record existed; a fresh result follows publication.

### What is in flight

- This review record and handoff are committed together and pushed to the PR branch. The remote head and fresh `review-gate` result must be verified.

### Traps and gotchas

- The failure of `review-gate` at the implementation head is the expected missing-record state. The metadata commit adds the record and must trigger a fresh gate.
- No physical Steam Deck was available. The smoke session covers device 3 input events, and the game logs pad device ids for a later Deck check (D-1077).

### The questions that block progress

OQ-246 remains open for the cause of the one-level screen-test variance. D-1080 sets the one-level compare and leaves the cause for later; OQ-246 blocks no PR.

### The next concrete action

Push the metadata commit to `fix/pr-101-torch-and-pad`, fetch, confirm the branch has no ahead commit and `gh pr view` names the pushed head, then read the fresh review-gate and Gitar results.

## Session 287: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR-101, round 4. Repository: the-thing-below. Branch: `fix/pr-101-torch-and-pad`. PR: #78 (PR-101). Role: author. Base: `c154ddd`.

### What this session did, and why

- The screen-test job of `d8b967e` failed on `map-fire-1x.png` again, with the other value of the same 232 pixels. The baseline of round 3 came from one CI run, and the next run drew the other value.
- The local capture session gave both values: the map fixture alone gave one, and the full list gave the other, each time. The base commit `c154ddd` held the same flaw in `map-fire-1x` and `map-stepped-1x`. The pixels lie on the columns of the wall shadows beside the doorways (F-108). This is the flake of OQ-246.
- The owner chose both options: the compare allows one level now, and the cause stays open (D-1080). `ScreenCompare` passes a pixel when each channel lies within one level, and two new tests hold the rule.
- D-1080 revises D-172 and D-1061 in part. OQ-246 is answered in part.

### The state of the build

- Base `c154ddd`. The Gitar pass approved `d8b967e`. The remote head is the commit of this entry.
- On this machine: build, 3,219 tests, and format pass. The new compare passes both CI values of `map-fire-1x` and the local full list.

### What is in flight

- The Gitar pass and the CI checks of this head. Then the owner runs `make codex-review PR=78`.

### Traps and gotchas

- The state of one capture reaches the next capture. A frame of one fixture alone can differ by one level from the same frame in the full list.
- A step of two levels still fails the job, so a real change of a screen still needs a new baseline from the artifact (D-733).

### The questions that block progress

None. OQ-246 stays open for the cause, and it blocks no PR.

### The next concrete action

Read the Gitar pass and the screen-test job of this head. When both pass, tell the owner to run `make codex-review PR=78`.

## Session 286: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR-101, round 3. Repository: the-thing-below. Branch: `fix/pr-101-torch-and-pad`. PR: #78 (PR-101). Role: author. Base: `c154ddd`.

### What this session did, and why

- The Gitar pass of `7aef6dc` approved, and Gitar closed its finding of round 1 as fixed.
- The screen-test job of `7aef6dc` failed on `map-fire-1x.png` alone: 232 pixels, one level each. The render of this machine and of CI differ there by one level. The baseline takes the capture of the job artifact 10846167929 (D-733). The other 107 captures of the artifact match the baseline.
- Each other CI check of `7aef6dc` passed or was in progress. `review-gate` waits for the review record alone.

### The state of the build

- Base `c154ddd`. The reviewed Gitar head `7aef6dc`. The remote head is the commit of this entry.
- The job artifact of CI downloads through the proxy of this machine with the API address of the artifact.

### What is in flight

- The Gitar pass and the screen-test job of this head. Then the owner runs `make codex-review PR=78`.

### Traps and gotchas

- `map-fire-1x.png` and `still-240.png` can differ by one level between this machine and CI. Take a changed capture from the artifact of the job, never from this machine alone.

### The questions that block progress

None.

### The next concrete action

Read the screen-test job and the Gitar pass of this head. When both pass, tell the owner to run `make codex-review PR=78`.

## Session 285: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR-101, round 2. Repository: the-thing-below. Branch: `fix/pr-101-torch-and-pad`. PR: #78 (PR-101). Role: author. Base: `c154ddd`.

### What this session did, and why

- The Gitar pass of `ed7f21c` approved with one finding: the halo adds onto lit art, and the bound of lit art left the halo out (F-47). The finding has full merit.
- `BrightestLight.OnMap` now takes the halo of each fire of the map. A halo adds its middle to each tile that its circle can reach, and the carried halo adds to each tile (D-1075).
- The new bound refused the fixture dungeon: two halos reach the tile (6, 0), for 70691 of 70000. The halo of the wall torch went from 5000 to 4500, and the bound now gives 69707. The halo gives 95% of the light of the old rectangle.
- The 78 captures of the baseline changed again, and the author read the pit frames. The other frames change by the halo alone.
- `review-gate` failed on RG 3 alone, because no review record exists. A comment on the PR says so.

### The state of the build

- Base `c154ddd`. The reviewed Gitar head `ed7f21c`. The remote head is the commit of this entry.
- On this machine: build, 3,217 tests, format, det-lint, identity, and the screen comparison pass. The simulation version stays 26.

### What is in flight

- The Gitar pass of this head. Then the owner runs `make codex-review PR=78`.

### Traps and gotchas

- The bound of lit art counts the full middle of a halo over its whole circle. Two torches 128 pixels apart both reach the tile between them. A brighter halo or a closer pair of torches fails the load.
- This machine has no `codex` or `gh` command. The GitHub tools of the session read the checks and the comments.

### The questions that block progress

None.

### The next concrete action

Read the Gitar pass of the new head, reply on the Gitar thread with the fix commit, and answer each new Gitar item. The owner runs `make codex-review PR=78`.
