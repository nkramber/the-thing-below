## Session 353: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-108, round 6. Repository: the-thing-below. Branch: `feat/pr-108-night-recovery`. PR: #89. Role: author. Base: `a8ba710`.

### What this session did, and why

- Review round 2 of `4779565` closed P1-1 and gave `Changes required` with P2-1: the promotion wait read one page of 50 runs, so an old open run could fall outside it.
- `8179c98` asks the API for each open status with every page, and the ids go through one sorted file (D-1202). `docs/reviews/pr-89-response.md` holds the answer, with full merit.
- Gitar approved `548969b` with its three findings closed.

### The state of the build

- Local build, format, and STE are clean. The workflow tests pass.
- The remote head before this push is `74c5dca`, the review metadata of round 2.

### What is in flight

- The Gitar pass of this push, then `make codex-review PR=89`, round 3 of the review.
- After the merge: `make night-watch-install` on the Mac, and one test message through `notify.yml`.

### Traps and gotchas

- `notify.yml`, `night-promote.yml`, and the facts action first run from `main`, after the merge (F-37).
- P1-1 is closed. P2-1 counts one round. A third open round of one finding stops the loop (D-929).

### The questions that block progress

None for PR-108. OQ-252 blocks PR-107, and OQ-251 blocks PR-35.

### The next concrete action

Run the Gitar poll. When every check except `review-gate` is green, run `make codex-review PR=89` in the background.

## Session 352: 2026-09-27, Codex

Author: Codex
Session: repeat review PR #89 (PR-108). Repository: the-thing-below. Branch: `review/pr-89`, which tracks `origin/feat/pr-108-night-recovery`. Role: reviewer. Base: `a8ba710`.

### What this session did, and why

- Verified that P1-1 is fixed in `4779565`: the promotion wait resets its 15-minute limit when the open-run set changes (D-1202, T-2).
- Found P2-1: the wait reads one page of 50 runs and can miss an older open run. The review records the new finding and keeps the prior verdict in history.
- Verified all three Gitar code findings are closed. The author answered the current CI-analysis item, and the focused workflow tests pass (D-964).

### The state of the build

- `NightWorkflowTests` passes 17/17. CI run `36298048312` passes build, test, and format on each leg, bots, smoke, replay identity, screen test, det-lint, and STE. Night-gate run `36298047521` passes.
- Review-gate run `36298047516` reports RG 4 and RG 5 from the prior review record. The remote head before metadata commit `7193214` was `548969b`. GitHub confirmed `7193214` as the remote head after its push.

### What is in flight

- The author must correct P2-1 and request another review.

### Traps and gotchas

- The workflow API query reads only 50 runs. A completed newer page can hide an older open run.
- The newest Gitar dashboard is current on effective head `4779565`. Its three code findings are closed.

### The questions that block progress

None for PR-108. OQ-251 blocks PR-35, and OQ-252 blocks PR-107.

### The next concrete action

Read P2-1, add a pagination regression test, and request a repeat review of PR #89.

## Session 351: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-108, round 5. Repository: the-thing-below. Branch: `feat/pr-108-night-recovery`. PR: #89. Role: author. Base: `a8ba710`.

### What this session did, and why

- Gitar approved `f5a46c4` with one suggestion: the limit of 15 minutes of the promotion wait counted from the start, so a long queue could stop a run that moved.
- `4779565` starts the limit again each time the set of open earlier runs changes. The limit stops a stalled queue alone, and the job timeout of 40 minutes stays the hard limit (D-1202, T-2).
- The review gate faults on RG 4 and RG 5, because the record of round 1 names `e8a8cd9` with `Changes required`. The next Codex round writes the new record.

### The state of the build

- Local build, format, and STE are clean. The workflow tests pass.
- The remote head before this push is `f5a46c4`.

### What is in flight

- The Gitar pass of this push, then `make codex-review PR=89`, round 2 of the review.
- After the merge: `make night-watch-install` on the Mac, and one test message through `notify.yml`.

### Traps and gotchas

- `notify.yml`, `night-promote.yml`, and the facts action first run from `main`, after the merge (F-37).
- P1-1 counts one round. A third open round of one finding stops the loop (D-929).

### The questions that block progress

None for PR-108. OQ-252 blocks PR-107, and OQ-251 blocks PR-35.

### The next concrete action

Run the Gitar poll. When every check except `review-gate` is green, run `make codex-review PR=89` in the background.

## Session 350: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-108, round 4. Repository: the-thing-below. Branch: `feat/pr-108-night-recovery`. PR: #89. Role: author. Base: `a8ba710`.

### What this session did, and why

- The Codex review of `e8a8cd9` gave `Changes required` with one finding, P1-1: the concurrency group of `night-promote` kept one pending run, so a push could lose its promotion.
- The correction removes the group. Each run waits for each earlier run, then reads the facts, so the checks follow the order of the pushes (D-1202). `docs/reviews/pr-89-response.md` holds the answer, with full merit.
- The `codex-review` command stopped after the record landed, because the record named the handoff commit and not the effective head `9944c29` (D-610). The next round writes a new record.
- Gitar approved `e8a8cd9` with both of its findings closed.

### The state of the build

- Local build, format, and STE are clean. The workflow tests pass.
- The remote head before this push is `ce97bb0`, the review metadata.

### What is in flight

- The Gitar pass of this push, then `make codex-review PR=89` again.
- After the merge: `make night-watch-install` on the Mac, and one test message through `notify.yml`.

### Traps and gotchas

- `notify.yml`, `night-promote.yml`, and the facts action first run from `main`, after the merge (F-37).
- P1-1 counts one round. A third open round of one finding stops the loop (D-929).

### The questions that block progress

None for PR-108. OQ-252 blocks PR-107, and OQ-251 blocks PR-35.

### The next concrete action

Run the Gitar poll. When every check is green, run `make codex-review PR=89` in the background.

## Session 349: 2026-09-27, Codex

Author: Codex
Session: review PR #89 (PR-108). Repository: the-thing-below. Branch: `review/pr-89`, which tracks `origin/feat/pr-108-night-recovery`. Role: reviewer. Base: `a8ba710`.

### What this session did, and why

- Reviewed effective head `e8a8cd9` and found that the `night-promote` concurrency group can cancel a pending push before its promotion runs (D-1202).
- Confirmed the two Gitar watcher findings are fixed and confirmed by Gitar. The relevant watcher tests pass.
- Updated the PR Documents row and wrote the review record. The verdict is `Changes required` for P1-1.
- Pushed the review record and handoff as metadata. The session-end check confirmed the remote head.

### The state of the build

- `make verify` passes with 4,155 tests, format, lint, STE, replay identity, bots, content hash, atlas, and smoke.
- CI at `e8a8cd9` passes all product checks. `review-gate` reports RG 3 alone because the review record was not on the head at that time.
- The remote head before the metadata commit is `e8a8cd9`.

### What is in flight

- The author must fix P1-1 and request a repeat review.

### Traps and gotchas

- `night-promote` uses one concurrency group with no multi-run queue. GitHub replaces pending runs by default.
- The screen-test artifact shows the form line in `battle-forms-1x` and `battle-forms-fill-1080`.

### The questions that block progress

None for PR-108. OQ-252 blocks PR-107, and OQ-251 blocks PR-35.

### The next concrete action

The author fixes P1-1. Then run `make codex-review PR=89` for a repeat review.

## Session 348: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-108, round 3. Repository: the-thing-below. Branch: `feat/pr-108-night-recovery`. PR: #89. Role: author. Base: `a8ba710`.

### What this session did, and why

- The second Gitar pass found one edge case: a failed start Pushover stopped the start after the worktree existed, and each retry left one more worktree.
- `9944c29` makes each Pushover of the watcher best-effort, with a log line for a failed send. A start that fails before its session removes its worktree and its mark (D-1205, T-2).
- The watcher command now takes its program runners as delegates, so three tests drive the start with fakes of git, gh, and Claude Code (T-3).
- The review gate now fails on RG 3 alone, the review record that this PR waits for.

### The state of the build

- Local build, format, and STE are clean. The 19 watcher tests pass.
- The remote head before this push is `c6a9563`.

### What is in flight

- The Gitar pass of this push, then `make codex-review PR=89`.
- After the merge: `make night-watch-install` on the Mac, and one test message through `notify.yml`.

### Traps and gotchas

- `notify.yml`, `night-promote.yml`, and the facts action first run from `main`, after the merge (F-37).
- A command that ends with `cat` and no input waits forever in the shell of the harness.

### The questions that block progress

None for PR-108. OQ-252 blocks PR-107, and OQ-251 blocks PR-35.

### The next concrete action

Run the Gitar poll. When every check is green, run `make codex-review PR=89` in the background.

## Session 347: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-108, round 2. Repository: the-thing-below. Branch: `feat/pr-108-night-recovery`. PR: #89. Role: author. Base: `a8ba710`.

### What this session did, and why

- The Gitar pass found one bug: a stop of a fix session after the mark sent no Pushover, and a fault before the session kept the mark. `1521218` fixes both, with a regression test (D-1205, T-2).
- The CI analysis of Gitar named RG 7: the `CLAUDE.md` row gave no path after `Changed:`. The PR description now names the paths (D-581).
- The Windows leg failed one test: the launchd log paths took a backslash. The paths now join with a slash.
- The baselines `battle-forms-1x` and `battle-forms-fill-1080` come from the capture of CI run 36295590285 (D-733). Both frames read "4MP - Fire on one foe, either row.", with no other change (D-784).

### The state of the build

- Local build, format, and STE are clean. The watcher tests pass.
- The remote head before this push is `ef6eae8`.

### What is in flight

- The Gitar pass of this push, then `make codex-review PR=89`.
- After the merge: `make night-watch-install` on the Mac, and one test message through `notify.yml`.

### Traps and gotchas

- `notify.yml`, `night-promote.yml`, and the facts action first run from `main`, after the merge (F-37).
- A command that ends with `cat` and no input waits forever in the shell of the harness.

### The questions that block progress

None for PR-108. OQ-252 blocks PR-107, and OQ-251 blocks PR-35.

### The next concrete action

Run the Gitar poll. When every check is green, run `make codex-review PR=89` in the background.

## Session 346: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-108. Repository: the-thing-below. Branch: `feat/pr-108-night-recovery`. PR: the one PR of PR-108. Role: author. Base: `a8ba710`.

### What this session did, and why

- The owner answers of 2026-09-27 are D-1200 to D-1209. OQ-253 is filed and resolved by D-1204. PR-107 now follows PR-108 (D-1200).
- The alert: a job of the night workflow sends one Pushover message for a failed leg (D-1201). The `notify` workflow sends the other messages (D-1207).
- The gate fix: the walk of D-1204, and the `night-promote` workflow of D-1202 and D-1203. The action `night-facts` reads the facts for the gate and the promotion.
- The watcher: the `night-watch` command, its launchd job, and the `night-fix` skill (D-1205 to D-1208).
- The form line: `battle.form_help` reads `{mp}MP - {text}` (D-1209).
- `docs/design.md` M-3 keeps the numbers of night 1, run 36290389944.

### The state of the build

- `make test` passed 4,151 tests. Build, format, lint, and STE are clean.
- The remote head before this push is `a8ba710`, the base.

### What is in flight

- The baselines `battle-forms-1x` and `battle-forms-fill-1080` come from the CI capture of this PR (D-733). Read each frame before the commit (D-784).
- The Gitar pass, then `make codex-review`.
- After the merge: `make night-watch-install` on the Mac, and one test message through `notify.yml`.

### Traps and gotchas

- `notify.yml`, `night-promote.yml`, and the facts action first run from `main`, after the merge (F-37). The tests read their text.
- `CLAUDE.md` sits near its 16 KB limit. The skill list now points to the folder.
- The watcher needs a desktop session of the owner, because launchd starts the agents there alone.

### The questions that block progress

None for PR-108. OQ-252 blocks PR-107, and OQ-251 blocks PR-35.

### The next concrete action

Push, open the PR, run the Gitar poll, and commit the two baselines from the CI capture.

## Session 345: 2026-09-27, Codex

Author: Codex
Session: review PR #88 (PR-49). Repository: the-thing-below. Branch: `review/pr-88`, which tracks `origin/feat/pr-49-night-gate`. Role: reviewer. Base: `defedf6`.

### What this session did, and why

- The cross-provider review found no defect at effective head `758ff43` (T-4, D-17).
- The stale green head-night Gitar finding is fixed in `b1851c3`. Its regression tests pass, and Gitar verified the fix (G-22, D-1188, D-964).
- The CI analysis claim names RG 3 alone: the review record that this session adds (D-17, D-926).
- The PR description now names `docs/reviews/pr-88.md` in its Documents section (D-577, D-581).
- The review read each changed battle frame from the CI capture artifact. No visual fault appeared (D-784).

### The state of the build

- Local `dotnet build` passed with 0 warnings and 0 errors. `make test` passed 4,068 tests.
- CI run `36288648659` passed the build, tests, format, smoke, det-lint, replay-identity, screen-test, bots, STE, and coverage checks.
- The remote branch head before this metadata commit is `758ff43`. `review-gate` awaits this review record.

### What is in flight

- The review record and this entry need one metadata commit and a push to `feat/pr-49-night-gate`.
- The verdict is `Ready for owner merge`. The PR then waits for the owner merge (D-930).

### Traps and gotchas

- The live `night-gate` check does not run on PR-88. It first runs from `main` on a later PR (F-37, D-500).
- After the merge, the PR-49 author session runs the first night on `main` and adds the required check (D-1192).

### The questions that block progress

None for PR #88.

### The next concrete action

Commit and push this review record and entry together, then verify the remote head.
## Session 344: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-49, round 1. Repository: the-thing-below. Branch: `feat/pr-49-night-gate`. PR: the one PR-49 intent, with no GitHub number before the push. Role: author. Base: `defedf6`.

### What this session did, and why

- The owner answered OQ-81, OQ-82, and OQ-84 (D-1188 to D-1190), the night counts (D-1191), and the join of `night-gate` to the protection of `main` (D-1192). G-22 now counts the 48 hours from the last push.
- The session built the night: the workflow `night`, the `night` command, the night record, the `night-gate` command, and the workflow `night-gate` on `pull_request_target`. The runbook is `docs/runbooks/night.md`.
- A measure found F-155: the `bots` command kept each run record, 5.8 GB at 20,000 runs. The bot totals keep none now, and 60,000 runs held 182 MB.
- The owner added two battle screen changes to this PR (D-1193): one list of forms with the cost in the message box (D-1195), and a letter on each enemy of a kind that a fight holds twice (D-1194).
- The owner set ability power, AP, for PR-107 right after PR-49 (D-1196 to D-1199). OQ-252 blocks PR-107.
- The session corrected two slips of PR-15 on owner approval: the resolution lines of OQ-74 and OQ-80, and a stray `\n` in F-154.
- The owner note: the first item of PR-35 settles OQ-251, the node map of D-113 or a walkable overworld in the style of Final Fantasy VI, with OQ-122. Each later transitional prompt names OQ-251 until PR-35 starts.

### The state of the build

- The remote head of `main` is `defedf6`. The branch holds the night commit and the UI commit, and it goes to origin with this entry.
- `make verify` passed on the UI commit on this machine.

### What is in flight

- The screen-test job fails on the frames of the battle menu until the session commits the CI captures as the new baselines (D-733).
- The Gitar pass, then `make codex-review`.

### Traps and gotchas

- `make sheet` with no fixture fails: the contact sheet passes 65,535 rows. `make sheet FIXTURE=battle` works.
- The live night gate cannot run on this PR (F-37, D-500). After the merge, the session runs the first night on `main` and then requires the check (D-1192).
- The night counts come from one bot job. A slower rate needs a new measure (G-14).

### The questions that block progress

None for PR-49. OQ-252 blocks PR-107, and OQ-251 blocks PR-35.

### The next concrete action

Push, open the PR, commit the CI captures of the changed battle frames as baselines, and answer the Gitar pass.
