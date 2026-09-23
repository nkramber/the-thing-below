## Session 237: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-60, round 1. Repository: the-thing-below. Branch: `feat/pr-60-transitions`. PR: #64. Role: author. Base: `ca8c549`.

### What this session did, and why

- Asked the questions of PR-60 in two batches and recorded D-934 to D-941. D-934 revises D-196 in part: a pool of each region takes the place of the default of each region, for common encounters alone.
- Built the ten transitions as effect files, each 60 ticks long, and the transition table with the fixed kinds, the pool of region one, and the fade (D-195, D-934, D-936, D-940, D-941).
- Core reads the kind of an encounter in the order of D-937, and it picks from the pool with a hash of the seed and the start tick, with no repeat of the last pick (D-935). The simulation version is 16.
- Game holds the events of the fight for the 60 ticks of the transition. The fight then fades in from the cover color, and after a win or a flee the map fades back in before the wait intent (D-522, D-938, D-939).
- The pass of the hand-off draws last in the frame, above the UI. The budget counts one transition pass on every map (D-523, D-923).
- Added eleven captures: each look halfway through, and the color split at the reduced level, where the fade takes its place (D-863). The author read each frame of `make sheet FIXTURE=transition`, and reworked the snow whiteout, which read as static.
- The owner asked how a player sees the count of the enemies of a fight. The battle screen draws no waiting enemy and no count (D-758, D-778). OQ-242 holds the question, at the request of the owner.

### The state of the build

- `main` is `ca8c549`. This commit is the first push of the branch.
- `make build`, `make format`, `make lint`, `make smoke`, and the STE check pass on this machine. `make test` passes except the eleven new captures, which have no baseline yet.

### What is in flight

- The screen-test job of the first push gives the eleven baselines, and the author commits them from its artifact (D-733).
- The Gitar pass, then `make codex-review PR=64`.

### Traps and gotchas

- The transition shaders read the frame under them with `hint_screen_texture`, so the pass must stay the last child of the frame viewport.
- A test set with its own maps needs `EffectFixtures.WithMapsOf`, because each map of the rules belongs to one region (D-936).
- The party loses the hall fight at the fixture seed, so the test of exit test 4 flees.

### The questions that block progress

None. OQ-242 waits for the owner and blocks nothing in PR-60.

### The next concrete action

Read the screen-test artifact of the first push, commit the eleven baselines, and answer the Gitar pass.

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

## Session 232: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR #63, round 1. Repository: the-thing-below. Branch: `feat/pr-95-codex-review`. PR: #63. Role: author. Base: `097ea32`.

### What this session did, and why

- Checked the precondition: PR #62 merged as `097ea32`. `docs/reviews/pr-62-response.md` is on `main` through PR #62, so no untracked file of PR #62 stayed in the checkout.
- Asked the owner six questions, and recorded the direction and the answers as D-926 to D-931. The effect columns of D-8, D-576, D-578, D-582, and D-601 name each revision.
- Built `make codex-review PR=<n>` and the `codex-review` command of Tools, with 79 tests. It covers the install and the version of the CLI, the model probe, the start checks, the Gitar pass, the worktree, the outcome codes, and the three-strike count.
- Added the `Open at:` line to the record format, and the loop to `CLAUDE.md`, `AGENTS.md`, the skills, the PR template, `docs/runbooks/merge.md`, and `docs/runbooks/branch-protection.json`.
- Added PR-95 to section 8 of `docs/design.md`, to the phase-2 file as section 7.22, and to `area-ci.md` as section 7.20. G-15 names the auto-merge.

### The state of the build

- `main` is `097ea32`. The branch holds one commit on it, and this entry is in that commit.
- `make verify` passed on this machine before the commit.
- A run of `make codex-review PR=62` installed CLI 0.156.1, passed the probe, and refused PR #62 with four reasons, as the command must.

### What is in flight

- The Gitar pass of PR #63, then the first live run `make codex-review PR=63`, then the answers.
- The three settings of D-931 wait for the approval of the owner, before the hand-over. The owner merges PR #63 by hand.

### Traps and gotchas

- Make gives exit 2 for each failed target. Read the last line of the command: `codex-review: outcome <name> (exit <code>)`.
- The pre-commit hook refuses a checkout with no branch. Thus the worktree takes the local branch `review/pr-<n>`, and the reviewer pushes with `HEAD:<branch>`.
- `CLAUDE.md` holds 16,333 bytes of the limit of 16,384.
- The record of this PR needs an `Open at:` line in each finding, because the command reads it.
- This PR changes no workflow file. The review applies because Tools and the Makefile are outside the override set.

### The questions that block progress

None. D-926 to D-931 hold the answers of 2026-09-23.

### The next concrete action

Answer the Gitar pass of PR #63. Then run `make codex-review PR=63` in the background.

## Session 231: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #62, round 2. Repository: the-thing-below. Branch: `feat/pr-92-hd2d-passes`. PR: #62. Role: reviewer. Base: `05dcc3d`.

### What this session did, and why

- Recomputed the effective head. The PR tip is `3c3de99`, and its changes since `238bafe` are review and handoff metadata only.
- Read the author's answers to each claim of the Gitar CI analysis and checked their cited commits and job results.
- Rechecked the prior review, the complete implementation scope, and the current Gitar pass. The code review approves the implementation, and there are no open review threads.
- Ran `make verify`: 2,277 tests passed; format, det-lint, ste-check, replay identity, content hash, atlas, and smoke passed.
- Updated `docs/reviews/pr-62.md`, retained the earlier `Blocked` verdict, and gave `Ready for owner merge` for effective head `238bafe`.

### The state of the build

- `main` is `05dcc3d`. Before this review commit, the remote PR head was `3c3de99` and the effective head was `238bafe`.
- CI run 35823669111 passed the implementation checks on `238bafe`. Metadata run 35826831792 passed applicable checks, with implementation legs skipped. Review-gate run 35826840307 failed only RG 4 because the previous record still said `Blocked`.
- Review-gate run 35828245270 passed RG 1 to RG 8 after publication at `d366f7e`. Metadata run 35828245787 and Gitar passed on that head; the Gitar review approves the code.

### What is in flight

- The review is ready for the owner to merge.

### Traps and gotchas

- Metadata-only pushes skip platform build, replay identity, and screen-test legs. The implementation evidence remains run 35823669111 on `238bafe`.
- The Gitar analysis on `3c3de99` approves the code. Its reported CI fault is RG 4 reading the prior review verdict.

### The questions that block progress

None. OQ-241 asks which later PR adds ceiling shafts and blocks no progress here (D-924).

### The next concrete action

The owner merges PR #62.

## Session 230: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-92, round 3. Repository: the-thing-below. Branch: `feat/pr-92-hd2d-passes`. PR: #62. Role: author. Base: `05dcc3d`.

### What this session did, and why

- The review of session 229 gave `Blocked` for `238bafe`, with no finding in the code. The author had handed over with the CI analysis of the Gitar dashboard unanswered (D-14, D-67).
- Answered each claim of that analysis in a PR comment, and wrote `docs/reviews/pr-62-response.md`.
- The Gitar pass on this PR: 6 claims of the CI analysis over 4 heads, and no review thread. 3 claims had merit: the absent baselines (`5747c01`, `8b299b3`), the RG 7 line (PR description), and RG 4 (this answer). RG 6, RG 3, and the coverage failure had no merit for the author.
- Every code review of the pass approved, from `2d67459` to `26405dd`.

### The state of the build

- `main` is `05dcc3d`. The effective head is `238bafe`, and this commit is in the metadata set (D-610).
- CI passes every implementation check on `238bafe`. Review-gate passes all but RG 4, which reads the `Blocked` verdict.

### What is in flight

- The other provider repeats the review of `238bafe` with this response.

### Traps and gotchas

- The CI analysis in the Gitar dashboard is a comment of the pass, also when the code review approves. Answer each claim on the PR before the hand-over, and record the counts here.

### The questions that block progress

None. OQ-241 blocks no progress on PR #62.

### The next concrete action

The other provider repeats the review of PR #62.

## Session 229: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #62, round 1. Repository: the-thing-below. Branch: `feat/pr-92-hd2d-passes`. PR: #62. Role: reviewer. Base: `05dcc3d`.

### What this session did, and why

- Reviewed the full diff from `05dcc3d` through effective head `238bafe`.
- Verified the provider gate. Claude Code authored the PR, and Codex reviewed it.
- Checked the readers, render path, shaders, effect budget, decisions, tests, CI, and all 74 screen captures.
- Added `docs/reviews/pr-62.md`. The verdict is blocked because the author has not answered the current Gitar comment.

### The state of the build

- `main` is `05dcc3d`. The remote PR head is `09f2281`, and the effective head is `238bafe`.
- The focused tests pass with 230 tests. CI run 35823669111 passes each implementation check on the configured legs.
- Metadata CI run 35825994991 passes its applicable checks. Review-gate run 35825994798 passes RG 1 to RG 3 and RG 5 to RG 8. RG 4 reads the required `Blocked` verdict.
- Gitar passes on `09f2281`; its dashboard still reports the RG 4 failure. The author has not replied.

### What is in flight

- The PR waits for the author to answer the Gitar comment, and for the next review of the response.

### Traps and gotchas

- The first Gitar pass flagged the absent review path. The latest pass reports the `Blocked` verdict under RG 4.
- OQ-241 asks which later PR adds ceiling shafts, and blocks nothing here.

### The questions that block progress

None. OQ-241 blocks no progress on PR #62.

### The next concrete action

The author answers the Gitar comment, then the other provider reviews the response.

## Session 228: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-92, round 2. Repository: the-thing-below. Branch: `feat/pr-92-hd2d-passes`. PR: #62. Role: author. Base: `05dcc3d`.

### What this session did, and why

- The owner read the fixture shafts and said that a beam from a bare wall makes no sense. D-924 keeps wall shafts only under an opening, such as a window (OQ-240).
- The owner kept the still beam. D-925 removes the shimmer, its fields, the shimmer shaft, and the stepped still captures, and `LightWave` went back into `GlowPass`.
- A shaft kind now needs a drawing, and the map draws it on the wall. The fixture dungeon holds one barred window at (6, 1), with `drawing.decor_fixture_window` in the atlas.
- The owner wants ceiling shafts later. OQ-241 asks which PR adds them.

### The state of the build

- `main` is `05dcc3d`. The tests, format, lint, identity, content, atlas, and smoke pass on this machine. The author read `make sheet` for the map fixture.
- CI run 35821954258 on `efbf7dd`: the two capture runs matched on all 74 captures. The 48 baselines that the window changed come from its artifact (D-733).
- Gitar approved `efbf7dd` with no finding. CI and gitar passed on `8b299b3`, with review-gate waiting for the review record.
- The owner turned on SSH on the Deck, and this session ran the sweep of `spike/deck-test` there at the owner's request. The results are `a6f9f0b` on that branch, and they hold the pass row of 6 (F-106).

### What is in flight

- The PR waits for CI on the baseline commit and the review of the other provider.

### Traps and gotchas

- The window sits on the brick face of the wall tile, rows 16 to 27. The cap above the face is rows 0 to 15.
- A shaft sprite draws unshaded, as a torch does, so the opening stays bright in the dark.
- Over SSH, the Deck takes no `.bashrc`, so set `DOTNET_ROOT`, `PATH`, and `GODOT` by hand, and `DISPLAY=:0` for the sweep. Godot then falls back to Wayland.
- The owner should stop SSH on the Deck after this PR: `sudo systemctl stop sshd`.

### The questions that block progress

None. OQ-241 blocks no PR yet.

### The next concrete action

The other provider reviews PR #62.
