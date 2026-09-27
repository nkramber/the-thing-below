## Session 366: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-16, round 3. Repository: the-thing-below. Branch: `feat/pr-16-dungeon-parts`. PR: #91. Role: author. Base: `78b0d24`.

### What this session did, and why

- Gitar approved `6a9eb1ae` with no thread. Its CI claim, RG 3, has its answer in a PR comment (D-964).
- The cross-provider review of `6a9eb1ae` gave `Changes required` with P2-1: a resume of another build could revive an enemy that the memory of the map holds dead. The finding has full merit. `MapPatrols.Resume` now reads the memory, and a snapshot of this build with such an enemy alive refuses the load (D-555, D-1111). The answer is `docs/reviews/pr-91-response.md`.

### The state of the build

- `make verify` passed at the correction: 4,196 tests, format, lint, STE, identity, bots, content, atlas, and smoke.
- The remote head before this push is `dce3ed8`, the review record.

### What is in flight

- The Gitar pass and the CI legs of the correction, then `make codex-review PR=91` again.

### Traps and gotchas

- The map memory is the source of a dead enemy on each resume. The stored enemy values still give its place.

### The questions that block progress

None for PR-16. OQ-251 blocks PR-35.

### The next concrete action

Wait for the Gitar poll and the checks of the correction, then run `make codex-review PR=91` in the background.

## Session 365: 2026-09-27, Codex

Author: Codex
Session: review PR #91 (PR-16). Repository: the-thing-below. Branch: `review/pr-91`, which tracks `origin/feat/pr-16-dungeon-parts`. Role: reviewer. Base: `78b0d24`.

### What this session did, and why

- Reviewed effective head `6a9eb1a` and found a save-resume gap between the patrol values and the memory of a map (D-555, D-1111).
- Recorded P2-1 in `docs/reviews/pr-91.md`. The memory can mark a patrol dead while resume starts it alive after content drift.
- Gitar approved the head and made a CI-analysis claim about RG 3. The author answered each CI-analysis claim (D-964).

### The state of the build

- `make verify` passes locally, including 4,194 tests, bots, smoke, lint, identity, content, atlas, and STE.
- CI run `36338585597` passes each required leg except `review-gate`, which faults because the head has no review record. The remote head before this metadata commit is `6a9eb1a`.

### What is in flight

- This review record and handoff entry need one metadata commit and a push to `feat/pr-16-dungeon-parts`.
- The review verdict is `Changes required` until the save-resume gap is fixed and reviewed.

### Traps and gotchas

- `MapState.Resume` restores patrol values without the memory of the map. A later content revision can activate a patrol that the memory holds as dead.

### The questions that block progress

None for PR-16. OQ-251 blocks PR-35.

### The next concrete action

Fix P2-1, add its regression test, and run the PR checks.

## Session 364: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-16, round 2. Repository: the-thing-below. Branch: `feat/pr-16-dungeon-parts`. PR: #91. Role: author. Base: `78b0d24`.

### What this session did, and why

- Gitar approved `7c0483b` with no thread. Its one CI claim, RG 3 of `review-gate`, has its answer in a PR comment: the review record comes with the cross-provider review (D-964).
- CI run 36337887982 passed each leg but `screen-test` and `review-gate`. The author read the 45 changed captures and took them as the baseline: the form line of D-1209 with the space of the owner, the Items window with the key items, the map screen with the waystone and the exit, and the walk, scroll, and pit frames that now draw the waystone and the cache chest (D-733, D-784).

### The state of the build

- Each build, test, smoke, replay-identity, bots, det-lint, night-gate, and ste-check leg passed on `7c0483b`.
- The remote head before this push is `7c0483b`.

### What is in flight

- The CI legs of the baseline commit, then `make codex-review PR=91`.

### Traps and gotchas

- The baseline commit changes screens alone, so it moves the effective head, and Gitar reviews it again.

### The questions that block progress

None for PR-16. OQ-251 blocks PR-35.

### The next concrete action

Wait for the Gitar poll and the checks of the baseline commit, then run `make codex-review PR=91` in the background.

## Session 363: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-16, round 1. Repository: the-thing-below. Branch: `feat/pr-16-dungeon-parts`. PR: PR-16, the GitHub number follows the push. Role: author. Base: `78b0d24`.

### What this session did, and why

- The owner answered the design of the dungeon parts: D-1216 to D-1225. The save point and the hub waystone merge into one kind that saves alone (D-1221). PR-19 takes the free rest (D-1218).
- Core: solid doors, chests, and save points; the door and lock rule with the kept key and the Theft drill; the chest with the fallback and the kept rest; the exit; the memory of each map; the reopen flags; notices with values. Save format 18, and the simulation version 35.
- Game: the art of the doors, the chests, and the exit; the save window of a save point; the Keyring in the Items window; the exit mark beside a walked tile.
- The bots take doors, chests, save points, and the exit last. The owner approved one space between the AP cost and AP in the form line, with no record.

### The state of the build

- Local: `make build`, all 4,200 tests, `make format`, `make lint`, the STE check, `make bots`, and `make smoke` pass. The content hash and the identity file are rewritten.
- The remote head before this push is `78b0d24`, the base.

### What is in flight

- The first push of PR-16, then the Gitar pass and the CI legs. The screen baselines change in CI: the menu items, the map screen, the hub, and the save window.

### Traps and gotchas

- The hall door moved from (11, 4) to (27, 6), so the battle walk and its captures keep their ticks.
- A patrol sees a lead that touches it, diagonal tiles too, so a test map keeps each patrol off the paths.
- `make sheet` writes no frame of 1080 rows on a screen smaller than that. The 1x frames are read.
- Core refuses a list literal with elements, because it pulls in `System.Runtime.InteropServices`.

### The questions that block progress

None for PR-16. OQ-251 blocks PR-35.

### The next concrete action

Open the PR, run the Gitar poll, take the CI screen baselines, and answer each Gitar item before `make codex-review`.

## Session 362: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-107, round 5. Repository: the-thing-below. Branch: `feat/pr-107-ability-power`. PR: #90. Role: author. Base: `48ed83b`.

### What this session did, and why

- The cross-provider review approved the effective head `b9258bcf`: `Ready for owner merge`, with no open finding (T-4, D-17). The record is `docs/reviews/pr-90.md`, in `bac52b5` and `4e261f4`.
- Gitar approved `b9258bcf` with its one finding closed. Each Gitar item has its answer: the thread in `818c593`, and the three CI claims on RG 3 in PR comments (D-964).
- The PR description now marks the `docs/reviews/` row as Changed.

### The state of the build

- CI on `b9258bcf` passed each check except `review-gate`, which waited for the record.
- The remote head before this push is `4e261f4`, the review record.

### What is in flight

- The checks of this metadata commit, then the merge question to the owner (D-933, D-942). The PR then waits for the auto-merge (D-930).

### Traps and gotchas

- The archive move of the review record dropped the title line of `docs/session-handoff-archive.md`. This commit puts it back. A move script that takes the first line as the title must check it.
- The full `make sheet` fails to join past 65,535 rows, and `make sheet FIXTURE=menu` works. PR-107 changes no capture count.

### The questions that block progress

None for PR-107. OQ-251 blocks PR-35.

### The next concrete action

Ask the owner to confirm the merge with the summary of D-942, then turn on the auto-merge (D-930).

## Session 361: 2026-09-27, Codex

Author: Codex
Session: review PR #90 (PR-107). Repository: the-thing-below. Branch: `review/pr-90`, tracking `origin/feat/pr-107-ability-power`. Role: reviewer. Base: `48ed83b`.

### What this session did, and why

- Reviewed effective head `b9258bc` and traced AP combat regains, save migration, UI updates, and the gear line (D-1197 to D-1215).
- Verified the Gitar finding fixed in `818c593`, its regression test, and the answered RG 3 claim (D-964).
- Wrote `docs/reviews/pr-90.md` with `Ready for owner merge` for the effective head.

### The state of the build

- Local `make verify` passed, including 4,175 tests.
- CI passed all jobs except `review-gate`, which reports RG 3 because the review record is not yet published.
- The remote head before this metadata commit is `b9258bc`.

### What is in flight

- Push the review record and this handoff to `feat/pr-107-ability-power`, then verify the remote head and review-gate result.

### Traps and gotchas

- The review branch is `review/pr-90`; push with `git push origin HEAD:feat/pr-107-ability-power`.
- The session moves session 351 to the archive to keep 10 entries in the handoff (D-18, D-607).

### The questions that block progress

None for PR-107. OQ-251 blocks PR-35.

### The next concrete action

Run `make where`, commit the review record and handoff together, push to the PR branch, fetch, and verify the remote head with `gh pr view`.

## Session 360: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-107, round 4. Repository: the-thing-below. Branch: `feat/pr-107-ability-power`. PR: #90. Role: author. Base: `48ed83b`.

### What this session did, and why

- Gitar approved `3e990d3` with no new finding.
- The screen-test job of CI run 36303234544 named four captures: `menu-gear-1x`, `menu-gear-fill-1080`, `menu-gear-pack-1x`, and `menu-gear-pack-fill-1080`. Each shows "Marrek - Level 1" with one space on each side of the hyphen, and no other change. This round commits them as the new baseline (D-733, D-1214).

### The state of the build

- CI on `3e990d3` passed each check except `screen-test`, for the four frames above, and `review-gate`, for RG 3 alone.
- The remote head before this push is `3e990d3`.

### What is in flight

- The Gitar pass of this push, then `make codex-review PR=90` when every check but `review-gate` is green.

### Traps and gotchas

- The full `make sheet` fails to join past 65,535 rows, and `make sheet FIXTURE=menu` works. PR-107 changes no capture count.

### The questions that block progress

None for PR-107. OQ-251 blocks PR-35.

### The next concrete action

Run the Gitar poll, wait for green CI, and start the cross-provider review with `make codex-review PR=90`.

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
