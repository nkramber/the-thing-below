## Session 359: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-107, round 3. Repository: the-thing-below. Branch: `feat/pr-107-ability-power`. PR: #90. Role: author. Base: `48ed83b`.

### What this session did, and why

- The owner asked why the gear window put more space before the hyphen of "Marrek - Level 1" than after it. The three fixed columns of D-1170 caused it: a name of 10 cells, then a hyphen of 3 cells.
- The owner chose one string (D-1214) and put the fix in PR-107 with a waiver of G-8 for this PR alone (D-1215). `menu.gear_who` reads "{name} - Level {level}", and `menu.dash` is gone. `GearView` takes the string table, as `LessonsView` does.
- `MenuLayoutTests.TheLineOfTheCharacterInTheGearWindowIsOneStringThatFits` holds the text and the fit. The local capture `menu-gear-fill-1080` shows "Marrek - Level 1".
- Gitar approved `f72a2fc`, with its one finding closed and its CI claim on RG 3 answered on the PR.

### The state of the build

- CI on `f72a2fc` passed each check except `review-gate`, for RG 3 alone.
- Local: build, 4,175 tests, format, and det-lint pass. The content hash does not change, because the strings are outside `content/rules/`.
- The remote head before this push is `f72a2fc`.

### What is in flight

- The screen-test job fails on the gear captures, and its artifact gives the new baselines (D-733). Then the Gitar pass, and `make codex-review PR=90`.

### Traps and gotchas

- `make sheet FIXTURE=menu` joins its sheet, and the full `make sheet` fails to join past 65,535 rows. PR-107 changes no capture count.

### The questions that block progress

None for PR-107. OQ-251 blocks PR-35.

### The next concrete action

Take the gear baselines from the capture artifact of CI, push them, run the Gitar poll, and then start the cross-provider review.

## Session 358: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-107, round 2. Repository: the-thing-below. Branch: `feat/pr-107-ability-power`. PR: #90. Role: author. Base: `48ed83b`.

### What this session did, and why

- Gitar approved `5a107ae` with one finding: a regain rate of 0 still gave 1 AP through the floor. `818c593` makes the rules file refuse a rate of 0, with regression rows in `BattleFixtureTests`. Gitar offered an early return in `Regain`. The load refusal is better, because a 0 that loads and does nothing is a silent fault (T-2).
- The Gitar CI claim on `review-gate` is RG 3 alone: no review record yet. It needs no fix.
- The screen-test job of CI run 36302235212 named 31 captures. Each one shows AP in place of MP. `battle-lessons-1x` and `battle-lessons-fill-1080` also show "2AP - A hard cut at the front row.", because Hew now costs AP. This round commits the 31 files as the new baseline (D-733).

### The state of the build

- CI on `5a107ae` passed each check except `screen-test`, for the frames above, and `review-gate`, for RG 3.
- The remote head before this push is `5a107ae`.

### What is in flight

- The Gitar pass of this push, then `make codex-review PR=90` when every check but `review-gate` is green.

### Traps and gotchas

- `make sheet` captures each frame, then fails to join the sheet: the joined image passes 65,535 rows. PR-107 changes no capture, so the fault was on `main` before it.

### The questions that block progress

None for PR-107. OQ-251 blocks PR-35.

### The next concrete action

Answer the Gitar thread with `818c593`, run the Gitar poll, and then start the cross-provider review.

## Session 357: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-107. Repository: the-thing-below. Branch: `feat/pr-107-ability-power`. PR: the one PR of PR-107. Role: author. Base: `48ed83b`.

### What this session did, and why

- The owner answered OQ-252 and three new questions: both regains from one blow (D-1210), no message for a regain (D-1211), Bolt and Pilfer at 2 AP (D-1212), and a floor of 1 AP for each form (D-1213).
- Ability power, AP, replaces MP in Core, content, Game, Tools, and the string table (D-1197). The fixture costs follow D-1199 and D-1212.
- A fall of an enemy gives each character who is not down 10% of full AP. A basic attack that hits gives the attacker 5%, before the fall regain (D-1198, D-1210). The rules file holds `hit_regain` and `fall_regain`. A silent `Regain` event keeps the AP bar true (D-1211).
- Save format 17 names the pool `ap`, and formats 7 to 16 read `mp` as AP. The fixture `format-17.json` comes from the resumed run of format 16. The simulation version is 34, and the identity file and the content hash are new.
- `AbilityPowerTests` holds exit tests 1 to 4, the rule of D-1210, the cap at full AP, and a loop of 1,000 seeds. `SaveFixtureTests` holds exit test 5.
- The design doc, five area files, the phase files of Phase 2 and Phase 4, the glossary, and the `game-text-style` skill say AP.

### The state of the build

- Local: build, 4,169 tests, format, det-lint, the STE check, the content hash, and the identity file pass.
- The remote head before this push is `48ed83b`, the merge of PR-108.

### What is in flight

- The first push, the PR, and the Gitar pass. The screen-test job fails on the frames that show AP, and its artifact gives the new baselines (D-733).

### Traps and gotchas

- The string `battle.form_entry_free` is now `battle.form_name`: the battle list shows the name alone, and no form is free (D-1195, D-1213).
- `SnapshotLines.AsFormatSixteen` of Tests renames `ap` to `mp`, and each older converter calls it.
- A regain at full AP adds no event. A test that counts events must expect none there.
- `make sheet` captures each frame, then fails to join the sheet: the joined image passes 65,535 rows. PR-107 changes no capture, so the fault was on `main` before it. Read the frames in `artifacts/captures/`.

### The questions that block progress

None for PR-107. OQ-251 blocks PR-35.

### The next concrete action

Push, open the PR, run the Gitar poll, and then take the screen baselines from the capture artifact of CI.

## Session 356: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-108, round 8. Repository: the-thing-below. Branch: `feat/pr-108-night-recovery`. PR: #89. Role: author. Base: `a8ba710`.

### What this session did, and why

- Review round 3 approved the effective head `132942b`: `Ready for owner merge`, with P1-1 and P2-1 fixed (T-4, D-17).
- Gitar approved `2bb095b` with its four findings closed, and each Gitar item has its answer (D-964).
- The Gitar pass had four findings with merit, answered in `1521218`, `9944c29`, `4779565`, and `132942b`. The review had two, answered in `cdc3ed7` and `8179c98`.

### The state of the build

- CI on `2bb095b` passed each check except `review-gate`, which waited for the record of round 3.
- The remote head before this push is `0ac3061`, the review record of round 3.

### What is in flight

- The checks of this metadata commit, then the merge question to the owner (D-933, D-942).
- After the merge: `make night-watch-install` on the Mac, one test message through `notify.yml`, and the transitional prompt.

### Traps and gotchas

- `notify.yml`, `night-promote.yml`, and the facts action first run from `main`, after the merge (F-37).

### The questions that block progress

None for PR-108. OQ-252 blocks PR-107, and OQ-251 blocks PR-35.

### The next concrete action

Ask the owner to confirm the merge with the summary of D-942, then turn on the auto-merge (D-930).

## Session 355: 2026-09-27, Codex

Author: Codex
Session: repeat review PR #89 (PR-108). Repository: the-thing-below. Branch: `review/pr-89`, which tracks `origin/feat/pr-108-night-recovery`. Role: reviewer. Base: `a8ba710`.

### What this session did, and why

- Reviewed effective head `132942b` and verified P2-1: the promotion wait reads all pages and queries statuses in lifecycle order (D-1202, T-3).
- Gitar confirmed all four code findings closed. The latest CI-analysis claim names RG 4 and RG 5 from the prior review record, and this record updates that head and verdict (D-964).
- Updated the review record for `132942b`. No in-scope finding remains.

### The state of the build

- `NightWorkflowTests` passes 17/17. CI run `36299379234` passes build, test, and format on each leg, bots, smoke, replay identity, screen test, det-lint, and STE. Night-gate run `36299379312` passes.
- The remote head before this metadata commit is `2bb095b`. Review-gate run `36299379320` reads the prior record and faults only on RG 4 and RG 5.

### What is in flight

- This review record and entry need one metadata commit and a push to `feat/pr-108-night-recovery`.
- The PR waits for the new review-gate result, then for the owner merge (D-930).

### Traps and gotchas

- The workflow and test changes in this round do not change the screen.
- `notify.yml`, `night-promote.yml`, and the facts action first run from `main`, after the merge (F-37).

### The questions that block progress

None for PR-108. OQ-252 blocks PR-107, and OQ-251 blocks PR-35.

### The next concrete action

Commit and push this review record and entry together. Then verify the new review-gate result and the remote head.

## Session 354: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-108, round 7. Repository: the-thing-below. Branch: `feat/pr-108-night-recovery`. PR: #89. Role: author. Base: `a8ba710`.

### What this session did, and why

- Gitar approved `cd04546` with one suggestion: the status queries of the promotion wait ran out of the order of the life of a run, so a run that moved on between two queries could fall between them.
- `132942b` asks for `requested`, `pending`, `waiting`, `queued`, and `in_progress` in that order, so a run that moves on shows in the later query (D-1202).
- The review gate faults on RG 4 and RG 5 until review round 3 writes the new record.

### The state of the build

- Local build, format, and STE are clean. The workflow tests pass.
- The remote head before this push is `cd04546`.

### What is in flight

- The Gitar pass of this push, then `make codex-review PR=89`, round 3 of the review.
- After the merge: `make night-watch-install` on the Mac, and one test message through `notify.yml`.

### Traps and gotchas

- `notify.yml`, `night-promote.yml`, and the facts action first run from `main`, after the merge (F-37).
- P2-1 counts one round. A third open round of one finding stops the loop (D-929).

### The questions that block progress

None for PR-108. OQ-252 blocks PR-107, and OQ-251 blocks PR-35.

### The next concrete action

Run the Gitar poll. When every check except `review-gate` is green, run `make codex-review PR=89` in the background.

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
