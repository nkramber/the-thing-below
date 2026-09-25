## Session 288: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR #79 (PR-102), round 2. Repository: the-thing-below. Branch: `fix/pr-102-gate-hygiene`. PR: #79. Role: author. Base: `c154ddd`.

### What this session did, and why

- Read the Gitar pass of `fa68db5`. It completed in 113 seconds and approved with one finding.
- Gitar finding: the settings refusal compared with case. macOS and Windows open `.claude/Settings.local.json` as the same file. The compare now ignores case, as Gitar proposed, with case-variant tests (D-1085).
- Gitar CI claim: `review-gate` gave one fault. The log shows RG 3 alone, because `docs/reviews/pr-79.md` waits for the review. No change.
- CI on Windows failed `AStopAtTheLimitEndsEachChildOfTheProgram`. The fault came after 30 seconds, because Windows loses the parent link of a child of Git Bash. The read of the output now has a bound of 5 seconds after the end or the stop (D-1086, F-114). A new test covers a child that holds the output after a normal end.

### The state of the build

- Round 1 head `fa68db5`: every CI job passed except the Windows test and RG 3. `make smoke` passed on the Mac.

### What is in flight

- This round pushes the two fixes. Then the Gitar poll, a reply on the thread, and `make codex-review PR=79`.
- PR #78 (PR-101) is still open.

### Traps and gotchas

- The `review-gate` job runs the workflow of `main`, so the new facts step of F-109 first runs after the merge.
- The rebase notes of Session 287 stand: phase section 7.38, design sequence item 39, and PR-101 in place of "PR #78".

### The questions that block progress

None.

### The next concrete action

Push, run the Gitar poll, reply on the Gitar thread with the fix commit, then run `make codex-review PR=79` in the background.

## Session 287: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR #79 (PR-102), round 1. Repository: the-thing-below. Branch: `fix/pr-102-gate-hygiene`. PR: #79. Role: author. Base: `c154ddd`.

### What this session did, and why

- The owner gave a repository review report, outside the repository, with answers. For this task, a PR holds several findings (D-1080).
- The owner chose the gate findings alone, because PR #78 changes the input code of P1-1 and P2-6.
- Fixed P2-1 (F-108), P3-10 (F-109), P3-11 (F-110), P3-30 (F-111), P3-29 (F-112), P3-31 (F-113, D-1085), and P3-32 (F-114, D-1086).
- Recorded the answers on P1-2 (D-1081), P1-1 (D-1082), P2-6 (D-1083), and P2-4 (D-1084). No Game or Core code changed.
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

## Session 283: 2026-09-24, Codex

Author: Codex
Session: reviewer PR #77, round 2. Repository: the-thing-below. Branch: `docs/pr-100-gitar-resume`. PR: #77 (PR-100). Role: reviewer. Base: `58dadb5`.

### What this session did, and why

- Re-reviewed PR #77 at effective head `066901e` after the author fixed P2-1.
- Verified that command E reads the Gitar completion time and rejects a check after 900 seconds (D-1074).
- Checked the current Gitar pass and the CI results. The Gitar dashboard approves the head with no finding (D-964).
- Updated `docs/reviews/pr-77.md`. It closes P2-1 and gives `Ready for owner merge` for `066901e`.

### The state of the build

- Base and merge base: `58dadb5`. Effective and remote head before this metadata commit: `066901e`.
- CI run `36071384377` passed build, test, format, smoke, det-lint, replay identity, screen-test, coverage, changed-paths, and STE. Matrix jobs skipped for this documentation change.
- `review-gate` run `36071381473` failed only because the review record held the prior verdict. It must read this record after publication.

### What is in flight

- This metadata commit holds the repeat review and this entry. Fresh `review-gate` evidence must pass after publication.

### Traps and gotchas

- The review record keeps `Changes required` under `Earlier verdicts` and one current verdict under `Verdict`.
- Session 273 moves to `docs/session-handoff-archive.md` to keep 10 entries in this file.

### The questions that block progress

None.

### The next concrete action

Commit and push the review record and this entry to `docs/pr-100-gitar-resume`. Fetch, confirm the remote head, and read the new `review-gate` result.

## Session 282: 2026-09-24, Claude Code

Author: Claude Code
Session: author PR-100, round 2. Repository: the-thing-below. Branch: `docs/pr-100-gitar-resume`. PR: #77 (PR-100). Role: author. Base: `58dadb5`.

### What this session did, and why

- The Gitar poll of round 1 printed `completed at 375 s`. The Gitar code review approved with no thread.
- Answered the one Gitar item, a claim of the CI analysis on `review-gate`. RG 3 and RG 7 failed because the review record did not exist yet. No change.
- `make codex-review PR=77` ran with no skip flag and gave `Changes required` with P2-1: command E accepted a check that completed after 15 minutes.
- Answered P2-1 with full merit in `docs/reviews/pr-77-response.md`. Command E now reads the completion time and prints `not complete` for a check that completed after 900 seconds.

### The state of the build

- Base `58dadb5`. The reviewed head `87cf4cc`, and the review record `7bc72b7`. The remote head is the commit of this entry.
- `ste-check` gives 0 findings. Each other CI check was green on `87cf4cc`.

### What is in flight

- The Gitar pass of this head, with command E. Then `make codex-review PR=77` for round 2.

### Traps and gotchas

- `done` is a shell keyword, so command E names the completion age `took`.
- A Gitar check of round 1 started about three minutes after the push. The fallback of D-1074 comments `Gitar review` only when no check exists at three minutes.

### The questions that block progress

None.

### The next concrete action

Run command E of the `gitar-review` skill in the background for PR #77. Answer each Gitar item, then run `make codex-review PR=77` in the background.

## Session 281: 2026-09-24, Codex

Author: Codex
Session: reviewer PR #77, round 1. Repository: the-thing-below. Branch: `docs/pr-100-gitar-resume`. PR: #77 (PR-100). Role: reviewer. Base: `58dadb5`.

### What this session did, and why

- Reviewed PR #77 from merge base `58dadb5` through effective head `87cf4cc`.
- Confirmed Claude Code authored the PR, so Codex passes the provider gate (T-4, D-17).
- Inspected all 15 changed paths and checked the Gitar poll, decision updates, roadmap, and handoff changes against their contracts.
- Verified the Gitar CI claim. RG 3 and RG 7 failed because the review record was absent; this metadata commit adds it (D-577, D-581, D-964).
- Found that command E can accept a Gitar check that completed after the 15-minute limit (D-1074). The review record gives `Changes required` for `87cf4cc`.

### The state of the build

- Base and merge base: `58dadb5`. Effective and remote head before this metadata commit: `87cf4cc`.
- CI run `36070042782` passed the applicable build, test, format, smoke, det-lint, replay identity, screen, coverage, changed-paths, and STE checks. The matrix jobs skipped for this documentation change.
- `review-gate` run `36070042856` failed because the review record was absent. The new run must read this record.

### What is in flight

- This metadata commit holds the review record and this entry. The author must address P2-1 before the PR can receive approval.

### Traps and gotchas

- Command E checks `completed` before it checks the 900-second limit, and it omits `completed_at`.
- The Gitar CI claim is answered in the PR discussion. The row becomes true when this review record lands.

### The questions that block progress

None.

### The next concrete action

Commit and push this review record and handoff entry to `docs/pr-100-gitar-resume`. Fetch, verify the remote head, and read the fresh `review-gate` result.

## Session 280: 2026-09-24, Claude Code

Author: Claude Code
Session: author PR-100, round 1. Repository: the-thing-below. Branch: `docs/pr-100-gitar-resume`. PR: #77 (PR-100). Role: author. Base: `58dadb5`.

### What this session did, and why

- The owner ended the Gitar pause and asked for a Gitar poll after each push. D-1073 supersedes D-945, and D-1074 sets the poll.
- The owner answered four questions: PR-100 on a docs branch, a `Gitar review` comment when no Gitar check exists three minutes after the push, a stop at 15 minutes, and a direct answer of each finding with no stop.
- The owner said again that the fix of the session wins when it is better than the fix of Gitar. D-1072 stands in full, and its Effect column quotes the words.
- Removed each line of the pause and the steps that ended it in `docs/runbooks/merge.md`. The marker command lists no line.
- Rewrote the push wait and command E of the `gitar-review` skill. Command E waits 60 seconds, then reads the Gitar check every 20 seconds. Its result line is `completed`, `no check`, `not complete`, or `read failed`.
- Updated `CLAUDE.md`, `AGENTS.md`, `docs/runbooks/session-context.md`, the design status header, both sequences, and the blocks in `phase-2-first-playable.md` section 7.36 and `area-ci.md` section 7.23.
- Named D-1073 beside each live citation of D-945, for rule REF 3.

### The state of the build

- Base `58dadb5`. The remote head is the commit of this entry.
- `ste-check` gives 0 findings. Command E printed `completed` for PR #76 and `read failed` for a PR that does not exist.
- No code changes. The `codex-review` command already refuses a Gitar pass that is not complete.

### What is in flight

- The Gitar pass of this head, with command E. Then `make codex-review PR=77`. The PR changes decision rows, so the `review-override` label does not apply (D-401).

### Traps and gotchas

- The `gitar-review` skill of each other repository still holds the wait of three minutes. This PR changes this repository alone.
- The check-runs API gives 30 runs on each page. Command E filters by `check_name=Gitar`, so a code head with many CI legs still shows the Gitar check.

### The questions that block progress

None.

### The next concrete action

Run command E of the `gitar-review` skill in the background for PR #77. Answer each Gitar item, then run `make codex-review PR=77` in the background.

## Session 279: 2026-09-24, Codex

Author: Codex
Session: reviewer PR #76, round 1. Repository: the-thing-below. Branch: `feat/pr-91-torch-item`. PR: #76. Role: reviewer. Base: `15aad83`.

### What this session did, and why

- Reviewed PR #76 at effective head `4b4681c`. Claude Code authored the substantive commits, so Codex passes the provider gate (T-4, D-17).
- Inspected all 184 changed paths, including Core sight and torch rules, Game fades and input, save and settings migration, tests, content, screen baselines, and project documents.
- Confirmed the Gitar sight finding fix: the range grows in 24 ticks, the mark stays visible during the fade, and the regression test covers guards 3 to 6 tiles away (D-720, D-1062, D-1063).
- Confirmed the Gitar CI analysis item is answered by the corrected `docs/runbooks/merge.md` row in the PR description (D-577, D-581, D-964).
- Published `docs/reviews/pr-76.md` with `Ready for owner merge` for `4b4681c`.

### The state of the build

- Base and merge base: `15aad83`. Effective and remote head before this metadata commit: `4b4681c`.
- `make verify` passed on macOS arm64: 3,202 tests, format, det-lint, STE, replay identity, content, atlas, and smoke.
- CI run `36066356708` passed all implementation checks. `review-gate` waited for this review record. The Gitar code and CI items have answers.

### What is in flight

- This commit holds the review record and this entry. It must be pushed and verified.

### Traps and gotchas

- OQ-246 records a one-level screen-test flake and blocks no PR.
- `make sheet` with no fixture can exceed the PNG height limit. Use `FIXTURE=` for one fixture at a time (D-735).

### The questions that block progress

None. OQ-246 blocks no PR.

### The next concrete action

Push this metadata commit to `feat/pr-91-torch-item`. Fetch, check the branch status, confirm the PR head with `gh pr view`, and read the new `review-gate` result.
## Session 278: 2026-09-24, Claude Code

Author: Claude Code
Session: author PR-91, round 1. Repository: the-thing-below. Branch: `feat/pr-91-torch-item`. PR: #76. Role: author. Base: `15aad83`.

### What this session did, and why

- Asked the owner OQ-217, OQ-218, and each open question of the scope. D-1062 to D-1071 record the answers, and OQ-217 and OQ-218 close.
- Core: the `dark` field of each map file, the sight of the party on a dark map, the torch bonus of each patrol, the torch intents, the torch state in the save of format 13, and simulation version 25.
- Game: the `torch` action on the T key and the Y button, with the settings file at format 3. The torch in the hand, the carried light that follows the torch, and `SightFade` for each enemy of a dark map.
- The console lost its `torch` command (D-1071). The identity set gained the `torch` run.
- The author read the frames of `make walk`, the pit fixture, and the settings fixture on this machine.
- Gitar found that a held torch let a patrol see the party while the screen still faded the patrol in. The range now grows in 24 ticks, inside the beat of 30, and the mark draws with a fading patrol. A regression test holds it.
- The owner set D-1072: when the fix of Gitar differs or has a flaw, the session applies its own fix with no question.

### The state of the build

- Local head before this entry: `04fa13c`. `make verify` parts passed on this machine: build, 3197 tests, format, lint, STE, identity, content, atlas, and smoke.
- CI run `36061445475` passed each job but `screen-test` and `review-gate`. The screen-test flake of OQ-246 took reruns, then the job reached the baseline step.
- The new baselines come from that artifact: 90 changed captures and `pit-torch-1x`. The battle frames move one level, because the patrols of the fixture now see 2 tiles and the fight starts later (D-1067). The smoke fight ends at tick 2443, and at tick 2186 on `main`.

### What is in flight

- PR #76 is open. The fix of the Gitar finding waits for CI, then `make codex-review PR=76 -- --skip-gitar-review` runs.

### Traps and gotchas

- `make sheet` with no fixture writes every capture and then fails in the join of the sheet: the joined picture passes the height limit of 65535 pixels. Take one fixture at a time with `FIXTURE=`.
- `make sheet` clears `artifacts/captures` on each run.
- A Perl substitution with `|` as its delimiter reads an escaped `\|\|` in its pattern as an empty choice. Use another delimiter.

### The questions that block progress

None. OQ-246 stays open and blocks no PR.

### The next concrete action

When CI of the fix commit passes each job but `review-gate`, run `make codex-review PR=76 -- --skip-gitar-review` in the background (D-926, D-1061).

## Session 277: 2026-09-24, Codex

Author: Codex
Session: reviewer PR #75, round 3. Repository: the-thing-below. Branch: `feat/pr-99-stats-absorb-swap`. PR: #75. Role: reviewer. Base: `f1ab753`.

### What this session did, and why

- Re-reviewed PR #75 at effective head `8392c44`. Claude Code authored the PR, so Codex passes the provider gate (T-4, D-17).
- Confirmed P2-1 is fixed. The regression test passed in the prior round, and current CI passed at this head.
- Inspected D-1061, all 86 changed paths, and the current CI results. The review record now gives `Ready for owner merge`.

### The state of the build

- Base and merge base: `f1ab753`. Effective and remote head before this metadata commit: `8392c44`.
- CI run `36046700241` passed the changed-path checks, screen-test, smoke, coverage, and ste-check. Review-gate waits for this record.

### What is in flight

- This commit holds the review record and this handoff entry. It must be pushed and verified.

### Traps and gotchas

- Local `dotnet test` reported no test projects for the Microsoft.Testing.Platform setup. Current CI build and test passed.
- Gitar's status notice has no item and needs no answer (D-964). The pass was skipped under D-946.

### The questions that block progress

OQ-246 remains open and blocks no PR.

### The next concrete action

Push the metadata commit to `feat/pr-99-stats-absorb-swap`. Fetch, verify the branch status and PR head, and read the review-gate result.

## Session 276: 2026-09-24, Claude Code

Author: Claude Code
Session: author PR-99, round 3. Repository: the-thing-below. Branch: `feat/pr-99-stats-absorb-swap`. PR: #75. Role: author. Base: `f1ab753`.

### What this session did, and why

- The repeat review of Session 275 gave `Blocked` on `15945f2`. P2-1 was fixed, and screen-test had failed on the three frames of OQ-246.
- The review ran after a failed screen-test. The owner set D-1061: the review starts only when each check but `review-gate` passes, and a flake gets reruns first.
- The owner approved the string batch as written (D-57), and the PR description says so.
- Screen-test attempt 3 of run `36041907957` passed on `15945f2`. The CI run `36044147120` of the review commit `d6c4f3a` was cancelled, and its rerun passed each job.

### The state of the build

- Remote head before this commit: `d6c4f3a`. Every check but `review-gate` passed on it.

### What is in flight

- The repeat review, after the CI of this commit passes.

### Traps and gotchas

- A rerun of an old run can cancel the run of a newer commit of the PR, because the runs share a concurrency group. Read the run of the PR head with `gh pr checks`.
- A chain of a watch and a review must stop on a red check (D-1061).

### The questions that block progress

None. OQ-246 stays open and blocks no PR.

### The next concrete action

Wait for every check but `review-gate` to pass on this commit. Then run `make codex-review PR=75 -- --skip-gitar-review`.
