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


## Session 238: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #64, round 1. Repository: the-thing-below. Branch: `feat/pr-60-transitions`. PR: #64. Role: reviewer. Base: `ca8c549`.

### What this session did, and why

- Reviewed PR #64 from base `ca8c549` through effective head `875fe53`, across 99 changed paths.
- Confirmed Claude Code authored the substantive changes, and the Codex reviewer passes the provider gate (T-4, D-17).
- Verified the tick-after-wait crash fix and its regression test. Reviewed the transition content, deterministic pick, hand-off phases, shaders, budget, captures, and roadmap tests.
- Read the current CI run and inspected its screen artifact. Every implementation check passed, including all platform legs, screen-test, and Gitar.
- Added `docs/reviews/pr-64.md` with no findings and `Ready for owner merge` for `875fe53`.

### The state of the build

- `main` and the PR base are `ca8c549`. The effective head is `875fe53`. The remote branch tip before publication is `f3b4855`.
- `make verify` passed locally: 2,441 tests and every local gate passed.
- CI run 35884251448 passed build, test, format, smoke, replay identity, coverage, det-lint, STE, changed paths, and screen-test. Gitar passed. The pre-publication review-gate failure awaits its normal rerun after the record lands.
- The review record and this entry need one metadata commit and a push to `feat/pr-60-transitions`.

### What is in flight

- Publish the review record and this handoff entry together.
- Verify the remote branch head and the fresh review-gate result.

### Traps and gotchas

- The effective head is `875fe53`; later commits change handoff metadata alone.
- Push with `git push origin HEAD:feat/pr-60-transitions`.

### The questions that block progress

OQ-242 does not affect PR-60. It asks how the screen shows the count of waiting enemies.

### The next concrete action

Commit the review record and handoff entry together. Push, fetch, and verify the remote head.

## Session 237: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-60, rounds 1 to 3. Repository: the-thing-below. Branch: `feat/pr-60-transitions`. PR: #64. Role: author. Base: `ca8c549`.

### What this session did, and why

- Asked the questions of PR-60 in two batches and recorded D-934 to D-941. D-934 revises D-196 in part: a pool of each region takes the place of the default of each region, for common encounters alone.
- Built the ten transitions as effect files, each 60 ticks long, and the transition table with the fixed kinds, the pool of region one, and the fade (D-195, D-934, D-936, D-940, D-941).
- Core reads the kind of an encounter in the order of D-937, and it picks from the pool with a hash of the seed and the start tick, with no repeat of the last pick (D-935). The simulation version is 16.
- Game holds the events of the fight for the 60 ticks of the transition. The fight then fades in from the cover color, and after a win or a flee the map fades back in before the wait intent (D-522, D-938, D-939).
- The pass of the hand-off draws last in the frame, above the UI. The budget counts one transition pass on every map (D-523, D-923).
- Added eleven captures: each look halfway through, and the color split at the reduced level, where the fade takes its place (D-863). The author read each frame of `make sheet FIXTURE=transition`, and reworked the snow whiteout, which read as static.
- Committed 25 baselines from the artifact of CI run 35878686156 (D-733): the 11 transition captures and the 14 battle captures. The fixture fight now reaches its first command 80 ticks later, after the transition and the fade, so the drift of the backdrop and the fog moved. The author read the old and new frames, and no other part changed. The whole artifact of 85 captures matches the committed baseline.
- Answered the Gitar pass on `f2ac63d`: one finding and one CI claim. The finding had full merit: a fight that starts on the tick after the wait intent, in the same frame, met the phase `Waiting` and threw. The run now leaves the fight inside the tick loop, and `AFightOnTheTickAfterTheWaitIntentStartsItsTransition` fails on the old code. The CI claim named the review-gate fault of the absent review record, which `make codex-review PR=64` writes (D-926), and a PR comment answers it.
- The Gitar pass on `875fe53` approved the head, with the one finding closed and its thread resolved. Its CI analysis named the same `review-gate` fault of the absent record, and a second PR comment answers it. Every other job of `875fe53` passed.
- The owner asked how a player sees the count of the enemies of a fight. The battle screen draws no waiting enemy and no count (D-758, D-778). OQ-242 holds the question, at the request of the owner.

### The state of the build

- `main` is `ca8c549`. Round 1 pushed `479bd36`, and CI run 35878686156 failed on the missing baselines alone. Round 2 added OQ-242 and the baselines. Round 3 fixes the finding of the Gitar pass on `f2ac63d`.
- `make build`, `make test` with 2441 tests, `make format`, `make lint`, `make smoke`, and the STE check pass on this machine.

### What is in flight

- `make codex-review PR=64` on the effective head `875fe53`.
- The owner set the one concern of the next PR, the rules of the review and merge loop, with two rules. First, before the question of a merge, the author posts a summary in four sections, What, How, CI, and Codex review. CI says green or not, and Codex review gives the verdict: `Ready for owner merge`, `Blocked`, or `Changes required`. The summary sits inside the question block of the merge question, so the owner sees it with the question. The rule revises D-933 in part, the one paragraph, and the transitional prompt of PR-60 names it. Second, when `review-gate` is green and a new commit changes documents alone, `review-gate` stays green, and the PR needs no new review of the other provider. That rule widens the metadata set of D-603 and D-610. The owner chose one PR for both rules.

### Traps and gotchas

- The transition shaders read the frame under them with `hint_screen_texture`, so the pass must stay the last child of the frame viewport.
- A test set with its own maps needs `EffectFixtures.WithMapsOf`, because each map of the rules belongs to one region (D-936).
- The party loses the hall fight at the fixture seed, so the test of exit test 4 flees.
- A change of the length of a transition or of the fade moves every battle capture, because the fixture fight starts later.

### The questions that block progress

None. OQ-242 waits for the owner and blocks nothing in PR-60.

### The next concrete action

Run `make codex-review PR=64` on the effective head `875fe53`, and answer its outcome.



## Session 236: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR #63, hand-over. Repository: the-thing-below. Branch: `feat/pr-95-codex-review`. PR: #63. Role: author. Base: `097ea32`.

### What this session did, and why

- Ran round 2 of `make codex-review PR=63` on `a7902e7`. The run checked the ChatGPT login, removed the API key variables, and gave `approve`. The reviewer pushed `50e5664`, with `Ready for owner merge` for `a7902e7` and P2-1 and P2-2 fixed.
- Answered the Gitar pass on `a7902e7`: the code review approved with no thread. Its CI claim named RG 4 and RG 5 of the round 1 record, and a PR comment answers it. The two faults cleared with the round 2 record.
- Read the live merge settings again. The compare command of `docs/runbooks/merge.md` matched `docs/runbooks/branch-protection.json`.

### The state of the build

- `main` is `097ea32`. Before this commit, the remote head was `50e5664`, and the effective head is `a7902e7`.
- CI run 35834375973 passed each job on `a7902e7`. Run 35835364953 passed on `50e5664`, and `review-gate` passed there with RG 1 to RG 8.
- The Gitar pass approves `a7902e7`, and no thread is open. Two PR comments answer its two CI claims.
- The PR waits for the owner merge (D-931). The summary of one paragraph goes to the owner before the merge (D-933).

### What is in flight

- The confirmation of the owner, then the owner merge of PR #63.
- After the merge, the transitional prompt of step 6.

### Traps and gotchas

- The first auto-merge comes on the next PR. Before `gh pr merge <n> --auto --squash`, post the summary and get the confirmation of the owner (D-933).
- Never pass `forced_login_method` to the CLI. A mismatch logs the CLI out (D-932).
- Make gives exit 2 for each failed target. Read the outcome line of the command.

### The questions that block progress

None.

### The next concrete action

The owner confirms the summary and merges PR #63.



## Session 235: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #63, round 2. Repository: the-thing-below. Branch: `feat/pr-95-codex-review`. PR: #63. Role: reviewer. Base: `097ea32`.

### What this session did, and why

- Re-read the round 1 findings and the response file, then reviewed PR #63 at effective head `a7902e7`.
- Verified P2-1: the Gitar reads now paginate comments, check runs, check suites, and review threads. The regression tests pass.
- Verified P2-2: a repeated finding id now gives a fault. The regression test passes.
- Reviewed the API-key removal, ChatGPT login check, owner confirmation, and the updates to D-932 and D-933.
- Updated `docs/reviews/pr-63.md`. Both findings are fixed, and the verdict is `Ready for owner merge`.

### The state of the build

- `main` is `097ea32`. Before this commit, the remote PR head was `a7902e7`.
- `make build`, `make test` (2,371 tests), `make format`, `make ste-check`, and `make lint` passed locally.
- CI run 35834375973 passed implementation checks on `a7902e7`. Gitar passed. The review-gate failure read the old verdict and head, before this review commit.

### What is in flight

- This review record and entry need one metadata commit and a push to `feat/pr-95-codex-review`.
- Fresh review-gate and metadata checks must pass after publication.

### Traps and gotchas

- Metadata commits do not change the effective head (D-610).
- Review-gate run 35834379314 failed only because the record still named `Changes required` and `ca9dd85`.
- Keep each Codex process free of `OPENAI_API_KEY` and `CODEX_API_KEY` (D-932).

### The questions that block progress

None. D-926 to D-933 record the owner answers.

### The next concrete action

Commit this entry with the review record, push once, then verify the remote head and the fresh review-gate result.



## Session 234: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR #63, round 2. Repository: the-thing-below. Branch: `feat/pr-95-codex-review`. PR: #63. Role: author. Base: `097ea32`.

### What this session did, and why

- Ran the first live `make codex-review PR=63` on `ca9dd85`. The reviewer pushed `49480f1`, and the command gave `changes-required` with P2-1 and P2-2 open.
- Fixed P2-1: every GitHub read of the command now takes every page. Fixed P2-2: an id that comes two times in a record is a fault. `docs/reviews/pr-63-response.md` answers both.
- Recorded two owner directions. D-932: no API key in a Codex process, and a ChatGPT login before each run. D-933: the owner confirms each merge after a summary of one paragraph.
- Applied the three settings of D-931 after the approval of the owner. The compare command of `docs/runbooks/merge.md` matched the record.
- Answered the Gitar pass on `ca9dd85`: the code review approved with no thread, and one CI claim (RG 7) had merit. The description line changed, and a PR comment answers it.

### The state of the build

- `main` is `097ea32`. Before this commit, the remote head was `49480f1`, the review commit.
- CI passed each job on `ca9dd85`. The Codex tests pass locally: 91 of them.

### What is in flight

- The Gitar pass of the new head, then `make codex-review PR=63` for round 2.
- After an approval: the summary of one paragraph and the confirmation of the owner. The owner merges PR #63 by hand (D-931, D-933).

### Traps and gotchas

- A test of `-c forced_login_method="api"` logged the CLI out, and each Codex process of the machine failed until the owner logged in again. Never pass that option (D-932).
- `codex login status` writes to the error stream. The command reads both streams.
- Make gives exit 2 for each failed target. Read the outcome line of the command.
- `CLAUDE.md` holds 16,371 bytes of the limit of 16,384.
- The `docs/reviews/` line of the description takes the form `No change needed because` until a record lands. A `Changed:` line fails RG 7.
- The review commit `49480f1` removed the title line of `docs/session-handoff-archive.md`. This round restored it.

### The questions that block progress

None. D-926 to D-933 hold the answers of 2026-09-23.

### The next concrete action

Answer the Gitar pass of the new head. Then run `make codex-review PR=63` in the background.



## Session 233: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #63, round 1. Repository: the-thing-below. Branch: `feat/pr-95-codex-review`. PR: #63. Role: reviewer. Base: `097ea32`.

### What this session did, and why

- Reviewed all 37 changed paths from base `097ea32` through effective head `ca9dd85`.
- Confirmed Claude Code authored the PR, and the Codex reviewer passes the provider gate (T-4, D-17).
- Found that the Gitar pass check reads only 100 review threads, and that duplicate finding IDs split the three-strike count (D-14, D-929).
- Ran `make verify`: 2,359 tests passed, and each local check passed.
- Read the live CI checks. Each implementation check passed. Review-gate failed RG 3 because the review record was absent.
- Added `docs/reviews/pr-63.md` with two open findings and the verdict `Changes required`.

### The state of the build

- The base and merge base are `097ea32`. The effective code head is `ca9dd85`.
- Local verification passed. Live build, test, format, coverage, lint, identity, screen-test, smoke, and STE checks passed on their CI legs.
- The Gitar check passed on `ca9dd85`. The live merge settings match `docs/runbooks/branch-protection.json` (D-931).

### What is in flight

- The author needs to fix P2-1 and P2-2 in `docs/reviews/pr-63.md`, then request a repeat review.
- This metadata commit publishes the review record and this entry. The `docs/reviews/` Documents row needs correction after publication.

### Traps and gotchas

- Metadata commits do not change effective head `ca9dd85` (D-610).
- The pre-publication review-gate failure is RG 3 because the review record is absent. RG 4 and RG 5 skip until the record lands.

### The questions that block progress

None. The PR-95 roadmap lists no open question.

### The next concrete action

The author fixes both findings and requests a repeat review of PR #63.
