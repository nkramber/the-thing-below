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

## Session 368: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-16, round 4. Repository: the-thing-below. Branch: `feat/pr-16-dungeon-parts`. PR: #91. Role: author. Base: `78b0d24`.

### What this session did, and why

- The repeat review approved the effective head `9ad7f4b0`: `Ready for owner merge`, with no open finding (T-4, D-17). The record is `docs/reviews/pr-91.md`, in `3df2092`.
- Gitar approved `9ad7f4b0` and the review commit with no thread. Each CI claim on RG 3 to RG 5 has its answer in a PR comment (D-964).

### The state of the build

- CI run 36340357804 on `9ad7f4b0` passed each leg. The `review-gate` run 36341304057 passed on `3df2092`.
- The remote head before this push is `3df2092`, the review record.

### What is in flight

- The checks of this metadata commit, then the merge question to the owner (D-933, D-942). The PR then waits for the auto-merge (D-930).

### Traps and gotchas

- The review commit adds a handoff entry of its own, so the author reads the top number again before each entry.

### The questions that block progress

None for PR-16. OQ-251 blocks PR-35.

### The next concrete action

Ask the owner to confirm the merge with the summary of D-942, then turn on the auto-merge (D-930).

## Session 367: 2026-09-27, Codex

Author: Codex
Session: review PR #91 (PR-16), round 2. Repository: the-thing-below. Branch: `review/pr-91`, which tracks `origin/feat/pr-16-dungeon-parts`. Role: reviewer. Base: `78b0d24`.

### What this session did, and why

- Re-reviewed effective head `9ad7f4b` and verified the fix for P2-1. The resume keeps a newly placed enemy dead when map memory holds it dead (D-555, D-1111).
- Updated `docs/reviews/pr-91.md` to close P2-1 and approve the effective head. The new regression test fails on the prior head.

### The state of the build

- `make verify` passed locally, with 4,196 tests, format, lint, STE, identity, bots, content, atlas, and smoke.
- CI passed each required leg except `review-gate`, which still read the old review record. The remote head before this metadata commit is `9ad7f4b`.

### What is in flight

- The review record and this entry need one metadata commit and a push to `feat/pr-16-dungeon-parts`.
- The CI checks of that metadata commit then need verification.

### Traps and gotchas

- The metadata commit does not change effective head `9ad7f4b`.
- The Gitar CI claim names RG 4 and RG 5 from the old review record. The author answered that claim in a PR comment (D-964).

### The questions that block progress

None for PR-16. OQ-251 blocks PR-35.

### The next concrete action

Commit the review record and this entry together, push to the PR branch, then verify the remote head and checks.

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
