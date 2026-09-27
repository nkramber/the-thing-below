## Session 349: 2026-09-27, Codex

Author: Codex
Session: review PR #89 (PR-108). Repository: the-thing-below. Branch: `review/pr-89`, which tracks `origin/feat/pr-108-night-recovery`. Role: reviewer. Base: `a8ba710`.

### What this session did, and why

- Reviewed effective head `e8a8cd9` and found that the `night-promote` concurrency group can cancel a pending push before its promotion runs (D-1202).
- Confirmed the two Gitar watcher findings are fixed and confirmed by Gitar. The relevant watcher tests pass.
- Updated the PR Documents row and wrote the review record. The verdict is `Changes required` for P1-1.

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

## Session 343: 2026-09-26, Claude Code

Author: Claude Code
Session: author PR-15, round 4, the hand-over. Repository: the-thing-below. Branch: `feat/pr-15-bots`. PR: #87. Role: author. Base: `f383aa9`.

### What this session did, and why

- The repeat review gave `Ready for owner merge` for `f812808`, with P2-1 fixed. Every check of the review commit `5543ce6` passed, `review-gate` included, and Gitar passed. The Gitar pass of this PR held three CI-analysis claims about `review-gate`, one on each code head, and none had merit: the faults were RG 3, then RG 4 and RG 5, which the review records answer.
- The owner confirmed the merge after the summary in four sections (D-942).
- The owner chose to add `bots` to the required checks of `main` in this PR (D-1186). The session changed `docs/runbooks/branch-protection.json` and the live setting together, and the compare of the runbook shows that they match.
- The owner chose that the next PR that changes Game commits `TheThingBelow.Game/scripts/Ui/DialogueChange.cs.uid` (D-1187). The file is in `/tmp/pr15-aside/` now.

### The state of the build

- The remote head before this commit is `5543ce6`, the review record of `f812808`. This commit changes documents alone, so the approval stays (D-943).

### What is in flight

- The Gitar pass of this commit (D-944), then the gated auto-merge (D-930).

### Traps and gotchas

- Godot writes `DialogueChange.cs.uid` again each time it opens the project, such as in the smoke session of `make verify`. Move it out of the tree before `make codex-review` until a Game PR commits it (D-1187).
- The `bots` check now gates each PR, and a docs-only PR passes it through its gate job (D-858).

### The questions that block progress

None.

### The next concrete action

After the merge, write the transitional prompt of PR #87 alone.

## Session 342: 2026-09-26, Codex

Author: Codex
Session: repeat review PR #87 after the P2-1 correction. Repository: the-thing-below. Branch: `review/pr-87`. PR: #87. Role: reviewer. Base: `f383aa9`.

### What this session did, and why

- Verified that the runner checks each state and that the tick 7 regression test records and replays the softlock (D-1179, T-3).
- Marked P2-1 fixed in `docs/reviews/pr-87.md`. Verified the current Gitar CI-analysis claim against run `36279907132`; the log names RG 4 and RG 5 from the prior review record (D-964).

### The state of the build

- The local build, 17 focused tests, format, and STE check pass. CI run `36279906549` passes substantive jobs on every leg. The remote head before this metadata commit is `f812808`.

### What is in flight

- This metadata commit and its push. The PR then waits for the owner merge.

### Traps and gotchas

- Review-gate run `36279907132` reads the prior review record. RG 4 and RG 5 wait for this update.

### The questions that block progress

None.

### The next concrete action

The owner reads the updated record and confirms the merge.

## Session 341: 2026-09-26, Claude Code

Author: Claude Code
Session: author PR-15, round 3, the answer to the review. Repository: the-thing-below. Branch: `feat/pr-15-bots`. PR: #87. Role: author. Base: `f383aa9`.

### What this session did, and why

- Gitar approved `66dcf4d` with no finding. The author answered its CI-analysis claim on the PR: the one fault of `review-gate` was RG 3, the absent record (D-964). Two claims in all, one on each head, and neither had merit.
- The cross-provider review of `66dcf4d` gave `Changes required` for P2-1: the check each 60 played ticks could miss a softlock that clears on a later tick. The finding has full merit. `docs/reviews/pr-87-response.md` holds the answer.
- The runner now checks each state. The check skips the trial when the state accepts a toggle, which always changes the hash, and a seed loop proves that the two paths agree. The check on each tick ran faster than the old sampling.
- Moved the untracked `TheThingBelow.Game/scripts/Ui/DialogueChange.cs.uid` to `/tmp/pr15-aside/`, because the review refuses a tree with an untracked file. PR-36 added `DialogueChange.cs` with no `.uid`, and the repo tracks 91 such files. The owner decides where that file goes.

### The state of the build

- `make verify` passes on this machine. The remote head before this commit is `3e8f772`, the review record.

### What is in flight

- The Gitar pass of this push, then the repeat review of `make codex-review PR=87`.

### Traps and gotchas

- The review command refuses a tree with an untracked file. Godot writes a `.uid` for a new script when the editor opens the project.
- The owner adds `bots` to the required checks of `main` after its first run (section 7.16 of `docs/roadmaps/area-ci.md`).

### The questions that block progress

None.

### The next concrete action

Wait for the Gitar pass and the CI of this push. Then run `make codex-review PR=87` in the background.

## Session 340: 2026-09-26, Codex

Author: Codex
Session: review PR #87. Repository: the-thing-below. Branch: `review/pr-87`. PR: #87. Role: reviewer. Base: `f383aa9`.

### What this session did, and why

- Reviewed effective head `66dcf4d`. P2-1 finds that 60-tick softlock sampling can miss a softlocked state between samples (D-1179).
- Verified the Gitar CI-analysis item against the review-gate log. RG 3 waits for the review record, and the author answered the claim about unchecked boxes (D-964).
- Added `docs/reviews/pr-87.md` and corrected the PR Documents row.

### The state of the build

- `make verify` passes with 4,003 tests. CI run `36278779725` passed all substantive jobs on every leg. Review-gate run `36278780411` waits for the review record. The remote head before this metadata commit is `66dcf4d`.

### What is in flight

- The PR needs a correction for P2-1 and a repeat review.

### Traps and gotchas

- The runner checks softlocks every 60 played ticks. The exit condition of D-1179 applies to each state.
- A clean Gitar approval has no item and does not block the verdict (D-964).

### The questions that block progress

None.

### The next concrete action

Correct P2-1, add its regression test, and request a repeat review of PR #87.
