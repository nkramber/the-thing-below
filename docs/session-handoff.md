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

## Session 243: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-97, round 1. Repository: the-thing-below. Branch: `feat/pr-97-gitar-pause`. PR: PR-97. Role: author. Base: `919547b`.

### What this session did, and why

- The owner paused the Gitar requirement, and asked for a flag that skips the Gitar check of `make codex-review` (D-945, D-946).
- The owner chose the form `make codex-review PR=<n> -- --skip-gitar-review`, the read at each gate, threads and findings alone as feedback, and the label with no Gitar approval.
- The `codex-review` command takes `--skip-gitar-review`. With it, the command reads no Gitar fact, and the prompt of the reviewer says that no Gitar pass is a condition.
- Each pause text is one whole line with the marker `Gitar pause (D-945)`. The section "The end of the Gitar pause" of `docs/runbooks/merge.md` gives the steps that remove the pause.
- PR-97 is in the roadmaps and in `docs/design.md`, after PR-96.

### The state of the build

- `main` is `919547b`. `make build`, `make test`, `make format`, and the STE check pass on this machine.

### What is in flight

- The CI run of the first push, then one read of the Gitar output, then `make codex-review PR=<n> -- --skip-gitar-review`.

### Traps and gotchas

- A Gitar review thread or a finding of the dashboard stops the session at once. Tell the owner before any other step (D-945).
- `make codex-review PR=<n> --skip-gitar-review` with no `--` fails, because make reads the flag as its own option.
- `CLAUDE.md` is near the 16 KB limit of SIZE 1. Two sentences left it in this PR.

### The questions that block progress

None.

### The next concrete action

Wait for CI, read the Gitar output one time, and start `make codex-review PR=<n> -- --skip-gitar-review` in the background.

## Session 242: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-96, hand-over. Repository: the-thing-below. Branch: `docs/pr-96-review-loop-rules`. PR: #65. Role: author. Base: `e6eb27a`.

### What this session did, and why

- Read the review record of Session 241: `Ready for owner merge` for the effective head `aba8774`, with no finding.
- Read the checks of the metadata tip `6cf9e9b`. Each check passed or skipped by the path rules, and `review-gate` passed.
- Read the merge conditions of `docs/runbooks/merge.md`. Each one holds except the confirmation of the owner (D-933, D-942).

### The state of the build

- `main` is `e6eb27a`. The effective head is `aba8774`, and `git diff --stat aba8774..6cf9e9b` lists `docs/reviews/pr-65.md`, `docs/session-handoff.md`, and `docs/session-handoff-archive.md` alone.
- CI run 35889338283 passed each job on `aba8774`.

### What is in flight

- The confirmation of the owner, then the auto-merge of PR #65 (D-930).
- After the merge, the transitional prompt of step 6.

### Traps and gotchas

- A push outside the skip set after the confirmation moves the effective head, and the loop starts again (D-943).

### The questions that block progress

None. OQ-242 waits for the owner and blocks nothing.

### The next concrete action

Post the summary in four sections inside the merge question, and turn on the auto-merge after the confirmation of the owner.

## Session 241: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #65, round 1. Repository: the-thing-below. Branch: `docs/pr-96-review-loop-rules`. Role: reviewer. Base: `e6eb27a`.

### What this session did, and why

- Reviewed PR #65 from base `e6eb27a` through effective head `aba8774`, across 19 changed paths.
- Confirmed Claude Code authored the substantive changes, and Codex passes the provider gate (T-4, D-17).
- Verified the RG 5 reviewable-head walk, the regression tests, the review outcome check, and the docs and decision updates (D-857, D-942 to D-944).
- Added `docs/reviews/pr-65.md` with no findings and `Ready for owner merge` for `aba8774`. The owner authorized this review to proceed without Gitar feedback or replies.

### The state of the build

- `main` is `e6eb27a`. The remote PR head is `aba8774` before this review record is published.
- `make verify` passed on this machine: 2,446 tests and every local gate passed.
- CI run 35889338283 passed every implementation job, including all legs of build, smoke, and replay identity, plus screen-test and STE.
- The Gitar status passed. No Gitar dashboard or actionable feedback appeared. Review-gate run 35889338357 failed because the review record was absent.
- Published metadata commit `3410c6a`; fresh review-gate run 35890585889 passed. The effective head stays `aba8774`.

### What is in flight

The owner can review the verdict and merge the PR.

### Traps and gotchas

- The owner’s exception applies to this review only. D-944 remains the project rule for later commits.
- The `make codex-review` Gitar precheck needs a dashboard comment, which this PR does not hold.

### The questions that block progress

None. OQ-242 does not affect PR-96.

### The next concrete action

The owner reviews the verdict and merges the PR.
## Session 240: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-96, round 1. Repository: the-thing-below. Branch: `docs/pr-96-review-loop-rules`. PR: #65. Role: author. Base: `e6eb27a`.

### What this session did, and why

- Asked the owner the two questions of rule 2 and the text of a labeled PR, and recorded D-942 to D-944. The owner chose the skip set of D-857 and no new review for a change of a decision row. The session recommended the override set and a new review for a decision row.
- D-942: the summary before a merge has four sections, What, How, CI, and Codex review, inside the question block of `AskUserQuestion`. Revises D-933 in part.
- D-943: after an approval, a commit of the skip set alone keeps the approval. `EffectiveHead.ReviewableHeads` gives the effective head and each earlier head after which each commit is in the skip set, and RG 5 accepts each one. The metadata set and the effective head of D-610 stay, because the Gitar pass reads them (D-944). The `codex-review` command still compares the record with the effective head alone. Revises D-610, D-401, and D-700 in part.
- D-944: a commit of documents alone still gets its Gitar pass, and the author answers each comment and each claim.
- Documents: `docs/runbooks/merge.md` (a new section for a commit of documents alone, and the summary), the `one-pr-one-session`, `gitar-review`, and `pr-review` skills, `CLAUDE.md`, `AGENTS.md`, the PR template, `docs/design.md`, and a new PR-96 block in the phase 2 file and in `area-ci.md`.
- Five new tests. `ACommitOfDocumentsAloneAfterTheApprovalKeepsTheGateGreen` fails on the old code, because the old RG 5 read the effective head alone.

### The state of the build

- `main` is `e6eb27a`. This round pushes the first commit of the PR.
- `make build`, `make test`, `make lint`, `make format`, and the STE check pass on this machine.

### What is in flight

- The Gitar pass of round 1, then `make codex-review PR=65`. The PR changes Tools code and decision rows, so it takes the review (D-401).

### Traps and gotchas

- `CLAUDE.md` is 16380 bytes, 4 bytes under SIZE 1. The next rule for sessions needs a cut first.
- Two sets stay apart: the override set of the label (D-16, D-700) and the skip set of D-943 (D-857).
- A commit of documents alone after the auto-merge is on: turn off the auto-merge first, because the Gitar pass of the new head must come before the merge (D-930, D-944).

### The questions that block progress

None. OQ-242 waits for the owner and blocks nothing.

### The next concrete action

Wait for the Gitar pass of round 1, answer each comment, then run `make codex-review PR=65` in the background.

## Session 239: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-60, hand-over. Repository: the-thing-below. Branch: `feat/pr-60-transitions`. PR: #64. Role: author. Base: `ca8c549`.

### What this session did, and why

- Ran `make codex-review PR=64` on the effective head `875fe53`. The outcome was `approve`, and the reviewer pushed `044d77c` with `Ready for owner merge` for `875fe53` and no finding.
- Read the whole Gitar dashboard of `875fe53`: the code review approved, and its one finding is closed. Two PR comments answer the two CI claims, both the `review-gate` fault of the absent record.
- Read the merge conditions of `docs/runbooks/merge.md`. Each one holds except the confirmation of the owner.

### The state of the build

- `main` is `ca8c549`. The effective head is `875fe53`, and `git diff --stat 875fe53..044d77c` lists `docs/reviews/pr-64.md`, `docs/session-handoff.md`, and `docs/session-handoff-archive.md` alone.
- CI run 35884251448 passed each job on `875fe53`. On `044d77c`, `review-gate` passed, and the jobs of code skipped on the metadata commit.

### What is in flight

- The confirmation of the owner, then the auto-merge of PR #64 (D-930, D-933).
- After the merge, the transitional prompt of step 6. It names the next PR: the rules of the review and merge loop of Session 237. The owner added a third rule to that PR: a PR or a commit of documents alone still gets its Gitar pass, and the author answers each comment and each claim of the pass (D-14, D-66, D-67). Only the review of the other provider is exempt.

### Traps and gotchas

- A push of code after the confirmation moves the effective head, and the loop starts again at the Gitar pass.

### The questions that block progress

None. OQ-242 waits for the owner and blocks nothing.

### The next concrete action

Post the summary in four sections inside the merge question, and turn on the auto-merge after the confirmation of the owner.
