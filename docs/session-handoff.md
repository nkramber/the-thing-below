## Session 378: 2026-09-28, Codex

Author: Codex
Session: review PR #94 (PR-109). Repository: the-thing-below. Branch: `review/pr-94`, which tracks `origin/feat/pr-109-overworld-encounters`. Role: reviewer. Base: `db0320b3`.

### What this session did, and why

- Reviewed effective head `1819fab9` against the PR-109 scope and contracts. Found no defect and recorded `Ready for owner merge` in `docs/reviews/pr-94.md` (T-4, D-17).
- Verified the Gitar CI-analysis claim against run 36364168926. RG 3 alone failed because the review record was not yet present (D-964).
- Corrected the `docs/reviews/` Documents row in the PR description (D-577).

### The state of the build

- `make verify` passed with 4,369 tests, no failures, and no skips. CI implementation checks passed at `1819fab9` on all three systems.
- The remote head before this metadata commit is `1819fab9`. The Gitar pass is current; its CI-analysis claim has the author's answer.

### What is in flight

- Commit the review record and this handoff entry together. Push with `git push origin HEAD:feat/pr-109-overworld-encounters`, then verify the remote head and review-gate.

### Traps and gotchas

- The review record names the effective head, `1819fab9`, and the metadata commit does not change it.
- OQ-254 tracks the greedy wipe loop and blocks no PR. PR-90 owns its resolution (D-1260).

### The questions that block progress

None for PR-109.

### The next concrete action

Run `make where`, `make ste-check`, and `git diff --cached --check`. Commit the review record and handoff files together, push to the PR branch, then verify the remote head and checks.

## Session 377: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-109, round 1. Repository: the-thing-below. Branch: `feat/pr-109-overworld-encounters`. PR: opened by this round. Role: author. Base: `db0320b`.

### What this session did, and why

- Asked the owner the questions of the step counter, the zones, and the snapshot. D-1261 to D-1269 record the answers.
- Built the invisible encounters of the overworld: the danger count, the encounter stream, the zone grid, and the zone list (D-1249, D-1261 to D-1266). Save format 20 holds the count, and the simulation version is 38.
- Gave the fixture overworld its four zones (D-1267).
- Fixed a fault of PR-64 in the same method: a step onto the encounter trap of the fixture dungeon stopped Game (D-1268).

### The state of the build

- `make verify` passed locally with 4,369 tests. The remote head is this round.
- 2,000 greedy runs: 1,785 reach the goal, and the longest takes 4,555 ticks, so the budget of D-1259 holds. 215 runs play the whole budget (OQ-254).

### What is in flight

- The Gitar pass of this round, then `make codex-review`.

### Traps and gotchas

- Each map file holds `zones` and `zone_grid`. A hub or a dungeon holds both empty.
- The readers of save formats 16 to 19 now take the seed of the header, because each one opens the encounter stream.
- `SnapshotLines.AsFormatNineteen` drops the count and the encounter stream for a test of an older reader.

### The questions that block progress

None.

### The next concrete action

Answer each Gitar item of this round, then run `make codex-review PR=<n>` in the background.

## Session 376: 2026-09-27, Codex

Author: Codex
Session: review PR #93 (PR-35). Repository: the-thing-below. Branch: `review/pr-93`, tracking `origin/feat/pr-35-region-map`. Role: reviewer. Base: `9d1d73f`.

### What this session did, and why

- Reviewed the full diff at effective head `5f2ba3b4c9d7c29ad49666a6b6fa23ba4f26d17f` against the PR-35 scope, exit tests, and contracts (D-1242 to D-1260).
- Found no defect. Recorded `Ready for owner merge` in `docs/reviews/pr-93.md`.
- Read the changed screen-test frames. No visual fault was found.
- Corrected the Documents row of the PR description to name the review record (D-577).

### The state of the build

- `make verify` passed locally with 4,318 tests and no skips. CI run 36359846898 passed every implementation check on all three systems.
- The implementation head is `5f2ba3b4c9d7c29ad49666a6b6fa23ba4f26d17f`. Metadata head `ed09141` passed review-gate, night-gate, changed-paths, STE, and Gitar. Its implementation matrix jobs skipped.

### What is in flight

- Commit this record and handoff update together. Push to `feat/pr-35-region-map`, then verify the remote head and checks.

### Traps and gotchas

- The Gitar CI claim reports the missing review record. The job log confirms RG 3 is its only fault, and the author answered it (D-964).
- OQ-254 holds the greedy bot wipe loop and blocks no work in PR-35 (D-1260).

### The questions that block progress

None for PR-35.

### The next concrete action

Run `make where`, `make ste-check`, and `git diff --cached --check`. Commit the review record and handoff files, push, then verify the remote head and checks.

## Session 375: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-35, round 1. Repository: the-thing-below. Branch: `feat/pr-35-region-map`. PR: #93. Role: author. Base: `9d1d73f`.

### What this session did, and why

- The owner answered OQ-251 and OQ-122: a walkable overworld at the scale of Final Fantasy VI replaces the node map (D-1242 to D-1258). D-113 is superseded, and D-37, D-224, D-430, and D-543 are revised in part.
- PR-35 builds the overworld: the map kind, four tile kinds, the entrance, the gate with its notice, the exit marker, and the autosave on the entry (D-1243, D-1246, D-1255 to D-1257). The fixture overworld joins the fixture dungeon and the fixture hub.
- PR-109 (the invisible encounters) and PR-110 (the overworld of region one) join Phase 2 (D-1248, D-1254).
- Each bot policy has its own tick budget: greedy 13,665, random 2,349. The greedy counts of CI and the night job are halved (D-1259, D-1260). OQ-254 holds the wipe loop of the greedy bot.
- The greedy path now crosses no exit or entrance except its target, with a regression test.

### The state of the build

- CI run 36359224796 on `5799c14` passed the bots, the smoke, the replay identity, the det-lint, and the STE jobs on each leg. The tests failed on the three absent `overworld` baselines alone. The simulation version is 37.
- The author read each frame of `make sheet FIXTURE=overworld`.

### What is in flight

- Gitar approved `5799c14` with no thread, and a PR comment answers its claim on RG 3. The second push adds the three `overworld` baselines and 12 hub baselines from the CI artifact: the hub exit now draws on the east wall of the yard (D-733). All 153 CI captures match them. Then `make codex-review PR=93`.

### Traps and gotchas

- The rats trigger of the hub sits on (18,10), and the capture walks cross row 5. The hub exit sits on (18,6) to stay off both.
- The greedy bot can loop on a wipe after a save at the waystone with a hurt party. About one dungeon run in five plays its whole budget (OQ-254). The owner first chose a reset on a reload, and the session measured it, found no gain, and removed it (D-1260).

### The questions that block progress

None for PR-35.

### The next concrete action

Run the Gitar poll of the second push, wait for its checks, then run `make codex-review PR=93` in the background (D-926).

## Session 374: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-64, round 4. Repository: the-thing-below. Branch: `feat/pr-64-traps-hazards`. PR: #92. Role: author. Base: `b7eb8bc`.

### What this session did, and why

- The repeat review approved the effective head `fe1c5ec7`: `Ready for owner merge`, with no open finding (T-4, D-17). The record is `docs/reviews/pr-92.md`, in `65b4053`.
- Gitar approved `fe1c5ec7` with no thread. Each CI claim on RG 3 to RG 5 has its answer in a PR comment (D-964).

### The state of the build

- CI on `fe1c5ec7` passed each job except the review-gate, which the record now answers.
- The remote head before this push is `65b4053`, the review record.

### What is in flight

- The checks and the Gitar pass of this metadata commit, then the merge question to the owner (D-933, D-942). The PR then waits for the auto-merge (D-930).

### Traps and gotchas

- The review commit adds a handoff entry of its own, so the author reads the top number again before each entry.

### The questions that block progress

None for PR-64. OQ-251 blocks PR-35.

### The next concrete action

Ask the owner to confirm the merge with the summary of D-942, then turn on the auto-merge (D-930).

## Session 373: 2026-09-27, Codex

Author: Codex
Session: repeat review PR #92 (PR-64). Repository: the-thing-below. Branch: `review/pr-92`, tracking `origin/feat/pr-64-traps-hazards`. Role: reviewer. Base: `b7eb8bc`.

### What this session did, and why

- Re-reviewed the fix for P2-1 at effective head `fe1c5ec77e990e82d4f74510e51e07ed0e343926`.
- The damage trap posts the down notice when one fighter falls and another stands. The wipe case posts no extra down notice (D-392, D-397, D-1241).
- Updated `docs/reviews/pr-92.md` to close P2-1 and approve the effective head. The new down-notice test fails on `869cc599`.
- Read the Gitar CI claim. The author answered both review-gate faults, and the log confirms them (D-964).

### The state of the build

- `TrapRulesTests` passed 19/19 on `fe1c5ec7`. The CI implementation checks passed on all three systems.
- The remote implementation head is `fe1c5ec77e990e82d4f74510e51e07ed0e343926`. The review-gate check waits for this metadata commit.

### What is in flight

- Commit the review record and this handoff entry as one metadata commit. Push to `feat/pr-64-traps-hazards`, then verify the remote head and review-gate result.

### Traps and gotchas

- The full `make sheet` fails in the join, because the sheet passes 65535 rows. `make sheet FIXTURE=pit` joins one fixture.
- The detached base test at `869cc599` used the two new tests. The down-notice test failed there, as required.

### The questions that block progress

None for PR-64. OQ-251 blocks PR-35.

### The next concrete action

Run `make where` and `make ste-check`. Commit the review record and handoff, push with `git push origin HEAD:feat/pr-64-traps-hazards`, then fetch and verify the PR head.

## Session 372: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-64, round 3. Repository: the-thing-below. Branch: `feat/pr-64-traps-hazards`. PR: #92. Role: author. Base: `b7eb8bc`.

### What this session did, and why

- Gitar approved `869cc59` with no thread. A PR comment answers its RG 3 claim again (D-964).
- The review of `869cc59` gave `Changes required` with one finding, P2-1: a damage trap that downs a fighter posted no notice of the down.
- P2-1 has full merit. A damage trap now posts `notice.fell_on_map` after its own notice when a fighter goes down and the party does not wipe. Two tests of `TrapRulesTests` prove it, and the first fails on `869cc59`. `docs/reviews/pr-92-response.md` records the answer.

### The state of the build

- Local: the build, 4271 tests, format, det-lint, and the replay identity pass.
- The remote head before this push is `33bca20`, the review record.

### What is in flight

- The CI and the Gitar pass of this push, then the repeat review with `make codex-review PR=92` (D-926).

### Traps and gotchas

- The full `make sheet` fails in the join, because the sheet passes 65535 rows. `make sheet FIXTURE=pit` joins one fixture.

### The questions that block progress

None for PR-64. OQ-251 blocks PR-35.

### The next concrete action

Wait for green CI and the Gitar pass on this head, then run `make codex-review PR=92` in the background.

## Session 371: 2026-09-27, Codex

Author: Codex
Session: review PR #92 (PR-64). Repository: the-thing-below. Branch: `review/pr-92`, tracking `origin/feat/pr-64-traps-hazards`. Role: reviewer. Base: `b7eb8bc`.

### What this session did, and why

- Reviewed effective head `869cc599ee1c3c9e4fc4412a66f311af16b7788a`, the traps, hazards, map status effects, wipe, saves, and HUD (D-1226 to D-1241).
- Inspected all 92 changed baseline frames in CI artifact `screen-captures`. No visual fault was found.
- Wrote `docs/reviews/pr-92.md` with `Changes required` for the effective head. P2-1 finds a missing down notice when a damage trap downs a fighter but does not wipe the party.
- Answered the review-gate evidence in the review record. RG 3 failed before publication because the record was absent (D-964).

### The state of the build

- Local `make verify` passed with 4,269 tests and no skips.
- CI run 36347245729 passed each implementation job on the effective head. The review-gate result awaits the metadata commit.
- The first remote metadata head was `0250ca3f9f982a46843da5d7120c7e531760b578`. The effective head remains `869cc599ee1c3c9e4fc4412a66f311af16b7788a`.

### What is in flight

- The author must correct P2-1. Then repeat the review of the new effective head (D-582).

### Traps and gotchas

- The review branch is `review/pr-92`; push with `git push origin HEAD:feat/pr-64-traps-hazards`.
- The full `make sheet` join exceeds 65,535 rows. The CI capture artifact holds the frames.

### The questions that block progress

None for PR-64. OQ-251 blocks PR-35.

### The next concrete action

Wait for the author correction, then review its regression test and update this review record.

## Session 370: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-64, round 2. Repository: the-thing-below. Branch: `feat/pr-64-traps-hazards`. PR: #92. Role: author. Base: `b7eb8bc`.

### What this session did, and why

- Gitar approved `8d652b0` with no thread. Its CI analysis named the RG 3 fault, which waits for the review record, and a PR comment answers it (D-964).
- CI run 36346460741 passed each job except the screen-test job. The 91 changed frames show the snow, the ice, or the new `pit-trap-1x` frame. The two frames of the battle pointer change in 12 and 27 pixels, because the `ui` page grew with the faces.
- This round commits the 92 baselines of the capture artifact of that run (D-733). The captures of the artifact match them.

### The state of the build

- The remote head before this push is `8d652b0`.

### What is in flight

- The CI of this push and its Gitar pass, then `make codex-review PR=92` (D-926).

### Traps and gotchas

- The full `make sheet` fails in the join, because the sheet passes 65535 rows. `make sheet FIXTURE=pit` joins one fixture.

### The questions that block progress

None for PR-64. OQ-251 blocks PR-35.

### The next concrete action

Wait for green CI and the Gitar pass on this head, then run `make codex-review PR=92` in the background.

## Session 369: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-64, round 1. Repository: the-thing-below. Branch: `feat/pr-64-traps-hazards`. PR: the PR-64 intent, with no GitHub number before the push. Role: author. Base: `b7eb8bc`.

### What this session did, and why

- The owner answered OQ-119, OQ-120, and the batches of detail: D-1226 to D-1241, with the look of the ice and the text batch.
- Core: a trap fires one time on the arrival of the lead: a share of full health, a lasting status, or a fight in which the enemies act first. The memory of the map keeps it spent (D-1226, D-1229 to D-1231). A Theft drill shows a trap at 2 steps, and a confirm disarms it (D-1228).
- Core: deep snow doubles a step, and ice slides the lead until a stop. A load check proves that each field of ice has a way out (D-1232, D-1233).
- Core: each 60 world ticks, poison hurts each poisoned character, the reserve included, and bad air hurts each fighter. A down of each fighter holds a wipe on the map (D-397, D-1234 to D-1236). Save format 19, simulation version 36.
- Game: the map HUD at the top left, the trap looks, the drain of a wipe on the map, and the frame `pit-trap-1x`. The art holds 3 tiles, 2 trap looks, and 5 faces (D-1237 to D-1239).

### The state of the build

- Local: the build, 4269 tests, format, det-lint, content, identity, atlas, smoke, and the bots pass. `make sheet` wrote every frame, and its joined sheet passes the PNG height limit, so the session read the frames alone.
- The remote head before this push is `b7eb8bc`, the base.

### What is in flight

- The first push, the PR, the Gitar poll, and the screen baselines from the capture artifact of CI.

### Traps and gotchas

- `RunState.MapWiped` comes from the party alone: no battle and no fighter who stands. A test that downs the only fighter on the map now meets a wipe, so two item tests take a partner.
- The owner chose darker snow and ice (D-1240). The ice now sits in the dark, and the snow on a base of snow shade still reads pale.
- The full `make sheet` fails in the join, because the sheet passes 65535 rows. `make sheet FIXTURE=pit` joins one fixture.

### The questions that block progress

None for PR-64. OQ-251 blocks PR-35.

### The next concrete action

Push, open the PR, run the Gitar poll, and then commit the baselines of CI.
