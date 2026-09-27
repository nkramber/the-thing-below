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
