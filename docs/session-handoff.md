# Session handoff

## Session 202: 2026-09-22, Codex

Author: Codex
Session: reviewer PR-55, initial review. Repository: the-thing-below. Branch: `feat/pr-63-settings`. PR: #55. Role: reviewer. Base: `daeccfe`.

### What this session did, and why

- Reviewed the settings file, input remap, settings screen, runtime application, tests, and screen baselines of PR #55.
- Verified the provider gate. Claude Code authored the PR, and Codex reviewed it.
- Wrote `docs/reviews/pr-55.md` with no finding and the verdict `Ready for owner merge` for effective head `d6b3606`.

### The state of the build

- `make verify` passed at `d6b3606` with 1,987 non-Smoke tests, 0 STE findings, 0 det-lint findings, matching replay identity and content hash, matching atlas, and a green smoke session.
- CI run 35689788015 passed its build, test, format, det-lint, replay identity, smoke, screen-test, and coverage jobs. Gitar approved the tip with no finding.

### What is in flight

- The review record and this handoff entry need commit and push.
- The owner can merge after the review-gate check turns green.

### Traps and gotchas

- The review targets effective head `d6b3606`. The later baseline and handoff commits are metadata only.

### The questions that block progress

None.

### The next concrete action

Commit and push the review record and handoff. Fetch and verify that the review-gate check covers effective head `d6b3606`.

## Session 201: 2026-09-22, Claude Code

Author: Claude Code
Session: author PR-63, round 2. Repository: the-thing-below. Branch: `feat/pr-63-settings`. PR: #55. Role: author. Base: `daeccfe`.

### What this session did, and why

- Read CI run 35689386604 of head `d6b3606`. Every job passed except screen-test, which named 8 captures: the 3 settings captures with no baseline, and the 5 captures of the `ui` fixture.
- The `ui` fixture draws the longest plain string of the table (D-241), and that string is now `settings.help`. The author read `ui-1x` and `settings-conflict-1x` of the artifact, and both fit.
- Committed the 8 captures of the artifact as the baseline (D-733). The `screens` command then matched all 52 captures.
- The Gitar pass of `d6b3606` approved with no finding. The review-gate fault is RG 3 alone: the review record of the other provider does not exist yet.

### The state of the build

- The baseline commit `c974fe4` and this entry make the push of this round. Every CI job except screen-test passed on `d6b3606`.

### What is in flight

- The Gitar pass of the new effective head, then the Codex review of `docs/reviews/pr-55.md`.
- The owner reads the text batch of the settings strings in the PR description (D-57).

### Traps and gotchas

- A new plain string that is longer than `settings.help` changes the 5 captures of the `ui` fixture again.

### The questions that block progress

None.

### The next concrete action

Run the Gitar wait for the push of this round, and prove that the review of the effective head is current.

## Session 200: 2026-09-22, Claude Code

Author: Claude Code
Session: author PR-63, round 1. Repository: the-thing-below. Branch: `feat/pr-63-settings`. PR: #55. Role: author. Base: `daeccfe`.

### What this session did, and why

- Asked the owner OQ-100, OQ-106, OQ-108, and OQ-109, and recorded the answers as D-860 to D-863. The first batch did not show the options of `docs/questions.md`, so the session asked OQ-100, OQ-108, and OQ-109 again with both sets of options. OQ-108 took a third answer from the owner: a conflict blocks the save and the exit, and the `ui_*` actions take no remap (D-862).
- Asked the values of the settings, and recorded D-864 to D-874. D-864 answers OQ-112, and D-872 answers OQ-110. D-870 removes the shape icons, because the 18 icons of PR-10 already differ in shape. D-873 corrects D-866: the session offered example times and did not read the pace of PR-10 first. D-874 stores the body size as auto, small, or large.
- Built the settings file in Storage: `GameSettings`, `ControlBindings`, `SettingsText`, `SettingsFormat`, and `SettingsStore`, with a fixture of format 1 (D-860, D-869).
- Built the settings screen in Game: `SettingsMenu` holds the rows and the rules, and `SettingsScreen` draws them. The menu action opens the screen (D-871), and the mouse moves the cursor (D-872).
- Applied the settings: the input map and the dead zone, the window mode, the fit, the body size, the message speed of the battle screen, the confirm skip, and the remembered cursor (`CommandMemory`).
- Added three captures of the settings screen, a smoke step, and tests for each new type.
- Read each settings frame of `make sheet`: `settings-1x`, `settings-fill-1080`, and `settings-conflict-1x`. The first layout overlapped the help line at a body of 32 and cut one button name. The second layout fits at both body sizes.

### The state of the build

- `make verify` passed locally with 1,987 tests outside Smoke, 0 findings of ste-check and det-lint, and the smoke session green.
- No file of Core changed, so the simulation version stays (G-17).
- The remote head is the push of this round on `feat/pr-63-settings`.

### What is in flight

- The screen-test job has no baseline for the three settings captures. The author commits the captures of the CI artifact as the baseline after the first run (D-733).
- The Gitar pass of the first push, then the review of the other provider.
- The owner reads the text batch of the settings strings in the PR description (D-57).

### Traps and gotchas

- The menu action on the map opens the settings screen until PR-62. In a fight, the menu intent goes to the rules as before.
- The close intent of the menu goes by its id, because the open intent can still wait in the queue in the same frame.
- The frame shows through a texture, so the mouse reads its frame pixel from `ScreenFit.ToFrame`.
- The volumes, the mono toggle, the mute, the vibration, the text speed, and the reduction save and load, and no code reads them yet. PR-36, PR-57 to PR-60, PR-69, and PR-70 read them.

### The questions that block progress

None.

### The next concrete action

Download the captures of the first screen-test run, commit the three settings baselines, and push. Then run the Gitar wait of `docs/runbooks/session-context.md`.

## Session 199: 2026-09-22, Codex

Author: Codex
Session: reviewer PR-93, repeat review. Repository: the-thing-below. Branch: `chore/pr-93-docs-only-ci`. PR: #54. Role: reviewer. Base: `d1a03f7`.

### What this session did, and why

- Read the response to P1-1 and recomputed the effective head as `c9429c4`. The intervening commits change only review and handoff metadata.
- Verified the live synchronize-event evidence in run 35680701238. The job logged a non-empty `BEFORE_REF` and returned `documents-alone: true`.
- Verified runs 35681799485 and 35681880270. Two consecutive docs-only pushes both skipped each build job, and each gate reported success.
- Updated `docs/reviews/pr-54.md`: P1-1 is withdrawn, the earlier verdict is preserved, and the current verdict is `Ready for owner merge` for effective head `c9429c4`.

### The state of the build

- The effective implementation head is `c9429c4`. The author reports `make verify` passed with 1,924 tests outside Smoke. The live regression runs passed.

### What is in flight

- The repeat-review record and this handoff entry need commit and push.

### Traps and gotchas

- The event payload documentation was incomplete for this field. Live workflow output is the decisive evidence for the trigger.

### The questions that block progress

None.

### The next concrete action

Commit and push the repeat-review record and handoff. Verify the review-gate check covers effective head `c9429c4`.

## Session 198: 2026-09-22, Claude Code

Author: Claude Code
Session: author PR-93, round 3. Repository: the-thing-below. Branch: `chore/pr-93-docs-only-ci`. PR: #54. Role: author. Base: `d1a03f7`.

### What this session did, and why

- Answered the review of session 197 in `docs/reviews/pr-54-response.md`.
- P1-1 has no merit. CI run 35680701238, on `43c2edc` with the event `pull_request` and the action `synchronize`, read `BEFORE_REF: c9429c4…` from `github.event.before`. It wrote the push facts and gave `documents-alone: true`.
- The push of this entry is the second docs-only push in a row after the green head `c9429c4`. The review asked for that run as its regression check.

### The state of the build

- No code change. The effective head stays `c9429c4`, and `make verify` passed there with 1,924 tests.
- Gitar approved `c9429c4`. The remote head is the push of this entry.

### What is in flight

- The CI run of this push. Its `changed paths` job must give `documents-alone: true` from the previous head `43c2edc`.
- The repeat review of the other provider.

### Traps and gotchas

- The previous head `43c2edc` skipped each build job. Thus its required checks come from the gate jobs of `c9429c4`, and this run proves the fix of round 2 live.
- That did not happen. The reviewer pushed `3ba4dad` at 02:58:18Z, and the push of `38cface` at 02:59:44Z cancelled its run. Each gate of `3ba4dad` then showed `failure`, so run 35681498346 of `38cface` ran every job, as D-858 requires. Every job passed.
- Two spaced metadata pushes follow: A after the green `38cface`, and B after A skips. B is the live check of two docs-only pushes in a row. Each push waits until the run before it completes.
- Push A, `54e8c5c`: run 35681799485 gave `documents-alone: true`, each build job skipped, and each gate reported `success`. Push B holds this line, and its previous head is A.
- Push B, `bed989b`: run 35681880270 read `BEFORE_REF: 54e8c5c…` and gave `documents-alone: true`. Two docs-only pushes in a row both skip. `docs/reviews/pr-54-response.md` records both runs.

### The questions that block progress

None.

### The next concrete action

The other provider repeats the review at effective head `c9429c4`.

## Session 197: 2026-09-22, Codex

Author: Codex
Session: reviewer PR-93, initial review. Repository: the-thing-below. Branch: `chore/pr-93-docs-only-ci`. PR: #54. Role: reviewer. Base: `d1a03f7`.

### What this session did, and why

- Reviewed the complete PR-54 diff through effective head `c9429c4`.
- Verified the provider gate, the Gitar correction, the changed-paths tests, the CI gate tests, and `make verify`.
- Found that `.github/workflows/ci.yml` reads `github.event.before` for a `pull_request` synchronize event. GitHub documents that field for push payloads, so the workflow does not receive the previous PR head and does not perform the advertised consecutive-docs-push skip.
- Added `docs/reviews/pr-54.md` with finding P1-1 and verdict `Changes required`.

### The state of the build

- `make verify` passes with 1,924 tests outside the Smoke category and 0 ste-check findings. The remote effective head is `c9429c4`.

### What is in flight

- The review record and this handoff entry need commit and push.

### Traps and gotchas

- A local unit test can inject previous-head facts, so it does not prove that the GitHub event supplies `github.event.before`.

### The questions that block progress

None.

### The next concrete action

The author must correct the previous-head source and add an event-shape regression check. Then Codex must repeat the review at the new effective head.

## Session 196: 2026-09-22, Claude Code

Author: Claude Code
Session: author PR-93, round 2. Repository: the-thing-below. Branch: `chore/pr-93-docs-only-ci`. PR: #54. Role: author. Base: `d1a03f7`.

### What this session did, and why

- Answered the two Gitar items of round 1.
- The RG 7 fault: the `docs/reviews/` row of the description named no path. The edit of the description fixed it, and review-gate now fails on RG 3 alone.
- The finding: a plain job that its condition skips reports the conclusion `skipped`, not `success`. Thus after one docs-only push, the rule of D-858 read `coverage report`, `det-lint`, and `screen-test` as no pass, and the next docs-only push ran every job. Full merit.
- The fix: a gate job with `always()` for each of the three jobs, as the matrix families have (D-682). Each gate takes the required name, and each job takes the name `<name> (run)`. D-858 stays true as written.
- The regression test `EachJobThatADocsOnlyChangeSkipsHasOneGateJob` fails on the workflow of `d0ca1bc` and passes now. `area-ci.md` section 7.19 lists the three new gates.

### The state of the build

- `make verify` passes with 1,924 tests outside the Smoke category, 0 ste-check findings, and the smoke session.
- The CI run of `d0ca1bc` passed every job on every leg. The remote head is the push of this entry.

### What is in flight

- The Gitar pass approved `c9429c4` at 2026-09-22T02:45:51Z, with 1 finding closed and no new finding. The Gitar check of that head passed.
- The review of the other provider (D-401, D-560). The next push holds this entry alone, so it is the first live docs-only push after a green head.

### Traps and gotchas

- Branch protection requires `coverage report` and `det-lint` by name. The gate jobs keep those names, so the protection needs no change. The old job names move to `<name> (run)`.
- A fault in `changed-paths` skips each plain job. Each new gate now fails in that case, where the old check showed `skipped` (T-2).
- Session 195 said that a skip on a skip holds. That was wrong for the three plain jobs before this round.

### The questions that block progress

None.

### The next concrete action

Read the `changed paths` log of the push of this entry: the build jobs skip, and each gate reports `success`. Then the other provider reviews PR #54 at effective head `c9429c4`.

## Session 195: 2026-09-22, Claude Code

Author: Claude Code
Session: author PR-93, round 1. Repository: the-thing-below. Branch: `chore/pr-93-docs-only-ci`. PR: the PR-93 intent, and GitHub gives the number at the open. Role: author. Base: `d1a03f7`.

### What this session did, and why

- Asked OQ-219 and two more questions that the work found. D-857, D-858, and D-859 hold the answers.
- D-857: `CLAUDE.md` and `AGENTS.md` join the skip set. `.claude/settings.json` was in the set already through `.claude/`.
- Added rules AGENTS 1 and AGENTS 2 to ste-check. AGENTS 1 reads the rule of D-20. AGENTS 2 reads the count of the Smoke filter option that `TestFilterTests` reads. A docs-only PR skips that test, so ste-check reads the same facts on every PR.
- D-858: a docs-only push skips the other jobs when each check that it skips passed on the previous head. review-gate and ste-check do not count.
- D-859: the new `changed-paths` command of Tools decides. The workflow collects the paths and the check runs of the `before` commit, and the job gets `checks: read`.
- Updated the `ste-writing` and `one-pr-one-session` skills, `area-ci.md` section 7.1, and section 7.15 of the phase 2 file.

### The state of the build

- `make verify` passes with 1,920 tests outside the Smoke category, 0 ste-check findings, and the smoke session.
- A local run of the two workflow steps against PR #53 gave the expected answer in four cases: a green head, a cancelled head, a force push, and a push to `main`.
- The remote head is the push of this entry.

### What is in flight

- The Gitar pass, then the review of the other provider. The PR changes `.github/workflows/` and adds decision rows, so no label applies (D-401, D-560).

### Traps and gotchas

- The run of a new push cancels the run of the previous head. A quick docs push after a code push thus runs every job, because the checks of the previous head show `cancelled`.
- The first CI run of this PR runs every job, because the PR changes code.
- Branch protection does not require `screen-test`, but the skip rule reads it. The rule is safe, because an absent run fails it.
- The shell step reads `github.event.before`. That field exists only on the `synchronize` action.

### The questions that block progress

None.

### The next concrete action

Open the PR. Load the `gitar-review` skill, wait for Gitar, and answer each comment. Then tell the owner that the PR is ready for the other provider.

## Session 194: 2026-09-21, Codex

Author: Codex
Session: reviewer PR-53, repeat review. Repository: the-thing-below. Branch: `feat/pr-56-light-and-shadows`. PR: #53. Role: reviewer. Base: `e0cc485`.

### What this session did, and why

- Re-reviewed the complete PR-53 diff through effective head `30868a8`.
- Verified the wall-face, figure-shadow, paired-light, budget, baseline, and Deck-sweep corrections against their original triggers and boundary tests.
- Updated `docs/reviews/pr-53.md` with the current verdict `Ready for owner merge`.

### The state of the build

- `make verify` passes with 1,862 non-smoke tests and all local gates. The remote effective head is `30868a8`.

### What is in flight

- The updated review record, this handoff entry, and the archive move need commit and push.

### Traps and gotchas

- Review-gate remains red until this repeat-review record reaches the PR head.

### The questions that block progress

None.

### The next concrete action

Commit the repeat review and both handoff files. Push, fetch, and verify the remote head and review-gate check.

## Session 193: 2026-09-21, Claude Code

Author: Claude Code
Session: author PR-56, round 7. Repository: the-thing-below. Branch: `feat/pr-56-light-and-shadows`. PR: #53. Role: author. Base: `e0cc485`.

### What this session did, and why

- The owner built the Deck test on the Deck and pushed the reports to `spike/deck-test` (commit `82bd091`). The branch also gained `build-on-deck.sh` and a Linux path in the template script.
- Each stage held 60 frames per second under both renderers. Under Mobile, `pairs-24` gave 4.17 ms and `full-load-24` 4.55 ms at the 95th percentile, with no frame over 16.667 ms. F-96 records it, and the light row of 24 stands (D-854, G-14).
- The owner asked whether the Deck test matches the game. It does not: it is the stress scene of the renderer pick, and it lights four times the pixels of the world of the game. The session offered a measure mode inside the game, and the owner has not answered.

### The state of the build

- Docs alone change this round. The remote head is this round.

### What is in flight

- The repeat review of Codex on the code head `f4b071b`.

### Traps and gotchas

- A path in backticks of another branch fails the reference check.
- The owner pushes from the Deck with SSH through `gh auth login -p ssh`.

### The questions that block progress

None for PR-56. The measure mode inside the game waits for an owner answer.

### The next concrete action

Wait for Gitar on this head, then tell the owner that PR #53 is ready for the repeat review.
