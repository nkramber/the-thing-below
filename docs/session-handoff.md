## Session 286: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR-101, round 3. Repository: the-thing-below. Branch: `fix/pr-101-torch-and-pad`. PR: #78 (PR-101). Role: author. Base: `c154ddd`.

### What this session did, and why

- The Gitar pass of `7aef6dc` approved, and Gitar closed its finding of round 1 as fixed.
- The screen-test job of `7aef6dc` failed on `map-fire-1x.png` alone: 232 pixels, one level each. The render of this machine and of CI differ there by one level. The baseline takes the capture of the job artifact 10846167929 (D-733). The other 107 captures of the artifact match the baseline.
- Each other CI check of `7aef6dc` passed or was in progress. `review-gate` waits for the review record alone.

### The state of the build

- Base `c154ddd`. The reviewed Gitar head `7aef6dc`. The remote head is the commit of this entry.
- The job artifact of CI downloads through the proxy of this machine with the API address of the artifact.

### What is in flight

- The Gitar pass and the screen-test job of this head. Then the owner runs `make codex-review PR=78`.

### Traps and gotchas

- `map-fire-1x.png` and `still-240.png` can differ by one level between this machine and CI. Take a changed capture from the artifact of the job, never from this machine alone.

### The questions that block progress

None.

### The next concrete action

Read the screen-test job and the Gitar pass of this head. When both pass, tell the owner to run `make codex-review PR=78`.

## Session 285: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR-101, round 2. Repository: the-thing-below. Branch: `fix/pr-101-torch-and-pad`. PR: #78 (PR-101). Role: author. Base: `c154ddd`.

### What this session did, and why

- The Gitar pass of `ed7f21c` approved with one finding: the halo adds onto lit art, and the bound of lit art left the halo out (F-47). The finding has full merit.
- `BrightestLight.OnMap` now takes the halo of each fire of the map. A halo adds its middle to each tile that its circle can reach, and the carried halo adds to each tile (D-1075).
- The new bound refused the fixture dungeon: two halos reach the tile (6, 0), for 70691 of 70000. The halo of the wall torch went from 5000 to 4500, and the bound now gives 69707. The halo gives 95% of the light of the old rectangle.
- The 78 captures of the baseline changed again, and the author read the pit frames. The other frames change by the halo alone.
- `review-gate` failed on RG 3 alone, because no review record exists. A comment on the PR says so.

### The state of the build

- Base `c154ddd`. The reviewed Gitar head `ed7f21c`. The remote head is the commit of this entry.
- On this machine: build, 3,217 tests, format, det-lint, identity, and the screen comparison pass. The simulation version stays 26.

### What is in flight

- The Gitar pass of this head. Then the owner runs `make codex-review PR=78`.

### Traps and gotchas

- The bound of lit art counts the full middle of a halo over its whole circle. Two torches 128 pixels apart both reach the tile between them. A brighter halo or a closer pair of torches fails the load.
- This machine has no `codex` or `gh` command. The GitHub tools of the session read the checks and the comments.

### The questions that block progress

None.

### The next concrete action

Read the Gitar pass of the new head, reply on the Gitar thread with the fix commit, and answer each new Gitar item. The owner runs `make codex-review PR=78`.

## Session 284: 2026-09-25, Claude Code

Author: Claude Code
Session: author PR-101, round 1. Repository: the-thing-below. Branch: `fix/pr-101-torch-and-pad`. PR: PR-101, with the GitHub number from the open. Role: author. Base: `c154ddd`.

### What this session did, and why

- The owner asked for three fixes and one change of order in one PR. The owner accepted the two concerns in one PR, and the PR description records it.
- The wall torch drew a box over its flame: the glow rectangle of D-915. A soft round halo behind the flame took its place, below the glow threshold. It gives 105% of the light that the rectangle spread (D-1075).
- The carried torch got a flame of six rows in the map drawing, a denser flame stream, and the flame orange at 16000 with a range of 240 (D-1076).
- Pad input: each binding matches every device, `ui_accept` and `ui_cancel` take the A and the B buttons, and a press gate passes the first press of each hold alone. Game logs each pad that connects (D-1077, F-107).
- A key press, a pad button, or a stick push hides the mouse pointer, and a mouse move shows it (D-1078).
- The audio PRs PR-38, PR-69, PR-70, PR-71, and PR-72 moved to right after PR-17 (D-1079).
- The simulation version went to 26, because the reader of the glow changed. The identity file and 78 captures of the baseline changed with it.

### The state of the build

- Base `c154ddd`. The remote head is the commit of this entry.
- On this machine: build, 3,214 tests, format, det-lint, identity, content hash, atlas, smoke, and `ste-check` pass. The capture session under Xvfb and lavapipe, with the Mesa version of CI, matched 108 of 109 captures of the base commit.

### What is in flight

- The Gitar pass of the head, then `make codex-review PR=<n>`.

### Traps and gotchas

- Godot 4.7.2 gives a new pad event the device 0 and a new key event the device 16. A binding of device 0 matched the first pad alone (F-107).
- The default `ui_accept` and `ui_cancel` of Godot 4.7.2 hold no pad button (F-107).
- No Steam Deck was at hand. The smoke session proves each rule with the events of a pad of device 3. The cause of the repeat of a held Start on the Deck is not proven. The pad log names the devices of the Deck at the next test.
- `still-240.png` of the base commit differed from the baseline by one level in 296 pixels on this machine. When the screen-test job fails on one capture alone, take the capture from the artifact of the job (D-733).
- A glow halo above the threshold clips to full light and draws a box again. The load refuses it (D-1075).

### The questions that block progress

None.

### The next concrete action

Read the Gitar pass of the PR head, and answer each Gitar item. This machine has no `codex` or `gh` command, so the owner runs `make codex-review PR=<n>` for the cross-provider review.

## Session 283: 2026-09-24, Codex

Author: Codex
Session: reviewer PR #77, round 2. Repository: the-thing-below. Branch: `docs/pr-100-gitar-resume`. PR: #77 (PR-100). Role: reviewer. Base: `58dadb5`.

### What this session did, and why

- Re-reviewed PR #77 at effective head `066901e` after the author fixed P2-1.
- Verified that command E reads the Gitar completion time and rejects a check after 900 seconds (D-1074).
- Checked the current Gitar pass and the CI results. The Gitar dashboard approves the head with no finding (D-964).
- Updated `docs/reviews/pr-77.md`. It closes P2-1 and gives `Ready for owner merge` for `066901e`.

### The state of the build

- Base and merge base: `58dadb5`. Effective and remote head before this metadata commit: `066901e`.
- CI run `36071384377` passed build, test, format, smoke, det-lint, replay identity, screen-test, coverage, changed-paths, and STE. Matrix jobs skipped for this documentation change.
- `review-gate` run `36071381473` failed only because the review record held the prior verdict. It must read this record after publication.

### What is in flight

- This metadata commit holds the repeat review and this entry. Fresh `review-gate` evidence must pass after publication.

### Traps and gotchas

- The review record keeps `Changes required` under `Earlier verdicts` and one current verdict under `Verdict`.
- Session 273 moves to `docs/session-handoff-archive.md` to keep 10 entries in this file.

### The questions that block progress

None.

### The next concrete action

Commit and push the review record and this entry to `docs/pr-100-gitar-resume`. Fetch, confirm the remote head, and read the new `review-gate` result.

## Session 282: 2026-09-24, Claude Code

Author: Claude Code
Session: author PR-100, round 2. Repository: the-thing-below. Branch: `docs/pr-100-gitar-resume`. PR: #77 (PR-100). Role: author. Base: `58dadb5`.

### What this session did, and why

- The Gitar poll of round 1 printed `completed at 375 s`. The Gitar code review approved with no thread.
- Answered the one Gitar item, a claim of the CI analysis on `review-gate`. RG 3 and RG 7 failed because the review record did not exist yet. No change.
- `make codex-review PR=77` ran with no skip flag and gave `Changes required` with P2-1: command E accepted a check that completed after 15 minutes.
- Answered P2-1 with full merit in `docs/reviews/pr-77-response.md`. Command E now reads the completion time and prints `not complete` for a check that completed after 900 seconds.

### The state of the build

- Base `58dadb5`. The reviewed head `87cf4cc`, and the review record `7bc72b7`. The remote head is the commit of this entry.
- `ste-check` gives 0 findings. Each other CI check was green on `87cf4cc`.

### What is in flight

- The Gitar pass of this head, with command E. Then `make codex-review PR=77` for round 2.

### Traps and gotchas

- `done` is a shell keyword, so command E names the completion age `took`.
- A Gitar check of round 1 started about three minutes after the push. The fallback of D-1074 comments `Gitar review` only when no check exists at three minutes.

### The questions that block progress

None.

### The next concrete action

Run command E of the `gitar-review` skill in the background for PR #77. Answer each Gitar item, then run `make codex-review PR=77` in the background.

## Session 281: 2026-09-24, Codex

Author: Codex
Session: reviewer PR #77, round 1. Repository: the-thing-below. Branch: `docs/pr-100-gitar-resume`. PR: #77 (PR-100). Role: reviewer. Base: `58dadb5`.

### What this session did, and why

- Reviewed PR #77 from merge base `58dadb5` through effective head `87cf4cc`.
- Confirmed Claude Code authored the PR, so Codex passes the provider gate (T-4, D-17).
- Inspected all 15 changed paths and checked the Gitar poll, decision updates, roadmap, and handoff changes against their contracts.
- Verified the Gitar CI claim. RG 3 and RG 7 failed because the review record was absent; this metadata commit adds it (D-577, D-581, D-964).
- Found that command E can accept a Gitar check that completed after the 15-minute limit (D-1074). The review record gives `Changes required` for `87cf4cc`.

### The state of the build

- Base and merge base: `58dadb5`. Effective and remote head before this metadata commit: `87cf4cc`.
- CI run `36070042782` passed the applicable build, test, format, smoke, det-lint, replay identity, screen, coverage, changed-paths, and STE checks. The matrix jobs skipped for this documentation change.
- `review-gate` run `36070042856` failed because the review record was absent. The new run must read this record.

### What is in flight

- This metadata commit holds the review record and this entry. The author must address P2-1 before the PR can receive approval.

### Traps and gotchas

- Command E checks `completed` before it checks the 900-second limit, and it omits `completed_at`.
- The Gitar CI claim is answered in the PR discussion. The row becomes true when this review record lands.

### The questions that block progress

None.

### The next concrete action

Commit and push this review record and handoff entry to `docs/pr-100-gitar-resume`. Fetch, verify the remote head, and read the fresh `review-gate` result.

## Session 280: 2026-09-24, Claude Code

Author: Claude Code
Session: author PR-100, round 1. Repository: the-thing-below. Branch: `docs/pr-100-gitar-resume`. PR: #77 (PR-100). Role: author. Base: `58dadb5`.

### What this session did, and why

- The owner ended the Gitar pause and asked for a Gitar poll after each push. D-1073 supersedes D-945, and D-1074 sets the poll.
- The owner answered four questions: PR-100 on a docs branch, a `Gitar review` comment when no Gitar check exists three minutes after the push, a stop at 15 minutes, and a direct answer of each finding with no stop.
- The owner said again that the fix of the session wins when it is better than the fix of Gitar. D-1072 stands in full, and its Effect column quotes the words.
- Removed each line of the pause and the steps that ended it in `docs/runbooks/merge.md`. The marker command lists no line.
- Rewrote the push wait and command E of the `gitar-review` skill. Command E waits 60 seconds, then reads the Gitar check every 20 seconds. Its result line is `completed`, `no check`, `not complete`, or `read failed`.
- Updated `CLAUDE.md`, `AGENTS.md`, `docs/runbooks/session-context.md`, the design status header, both sequences, and the blocks in `phase-2-first-playable.md` section 7.36 and `area-ci.md` section 7.23.
- Named D-1073 beside each live citation of D-945, for rule REF 3.

### The state of the build

- Base `58dadb5`. The remote head is the commit of this entry.
- `ste-check` gives 0 findings. Command E printed `completed` for PR #76 and `read failed` for a PR that does not exist.
- No code changes. The `codex-review` command already refuses a Gitar pass that is not complete.

### What is in flight

- The Gitar pass of this head, with command E. Then `make codex-review PR=77`. The PR changes decision rows, so the `review-override` label does not apply (D-401).

### Traps and gotchas

- The `gitar-review` skill of each other repository still holds the wait of three minutes. This PR changes this repository alone.
- The check-runs API gives 30 runs on each page. Command E filters by `check_name=Gitar`, so a code head with many CI legs still shows the Gitar check.

### The questions that block progress

None.

### The next concrete action

Run command E of the `gitar-review` skill in the background for PR #77. Answer each Gitar item, then run `make codex-review PR=77` in the background.

## Session 279: 2026-09-24, Codex

Author: Codex
Session: reviewer PR #76, round 1. Repository: the-thing-below. Branch: `feat/pr-91-torch-item`. PR: #76. Role: reviewer. Base: `15aad83`.

### What this session did, and why

- Reviewed PR #76 at effective head `4b4681c`. Claude Code authored the substantive commits, so Codex passes the provider gate (T-4, D-17).
- Inspected all 184 changed paths, including Core sight and torch rules, Game fades and input, save and settings migration, tests, content, screen baselines, and project documents.
- Confirmed the Gitar sight finding fix: the range grows in 24 ticks, the mark stays visible during the fade, and the regression test covers guards 3 to 6 tiles away (D-720, D-1062, D-1063).
- Confirmed the Gitar CI analysis item is answered by the corrected `docs/runbooks/merge.md` row in the PR description (D-577, D-581, D-964).
- Published `docs/reviews/pr-76.md` with `Ready for owner merge` for `4b4681c`.

### The state of the build

- Base and merge base: `15aad83`. Effective and remote head before this metadata commit: `4b4681c`.
- `make verify` passed on macOS arm64: 3,202 tests, format, det-lint, STE, replay identity, content, atlas, and smoke.
- CI run `36066356708` passed all implementation checks. `review-gate` waited for this review record. The Gitar code and CI items have answers.

### What is in flight

- This commit holds the review record and this entry. It must be pushed and verified.

### Traps and gotchas

- OQ-246 records a one-level screen-test flake and blocks no PR.
- `make sheet` with no fixture can exceed the PNG height limit. Use `FIXTURE=` for one fixture at a time (D-735).

### The questions that block progress

None. OQ-246 blocks no PR.

### The next concrete action

Push this metadata commit to `feat/pr-91-torch-item`. Fetch, check the branch status, confirm the PR head with `gh pr view`, and read the new `review-gate` result.

## Session 278: 2026-09-24, Claude Code

Author: Claude Code
Session: author PR-91, round 1. Repository: the-thing-below. Branch: `feat/pr-91-torch-item`. PR: #76. Role: author. Base: `15aad83`.

### What this session did, and why

- Asked the owner OQ-217, OQ-218, and each open question of the scope. D-1062 to D-1071 record the answers, and OQ-217 and OQ-218 close.
- Core: the `dark` field of each map file, the sight of the party on a dark map, the torch bonus of each patrol, the torch intents, the torch state in the save of format 13, and simulation version 25.
- Game: the `torch` action on the T key and the Y button, with the settings file at format 3. The torch in the hand, the carried light that follows the torch, and `SightFade` for each enemy of a dark map.
- The console lost its `torch` command (D-1071). The identity set gained the `torch` run.
- The author read the frames of `make walk`, the pit fixture, and the settings fixture on this machine.
- Gitar found that a held torch let a patrol see the party while the screen still faded the patrol in. The range now grows in 24 ticks, inside the beat of 30, and the mark draws with a fading patrol. A regression test holds it.
- The owner set D-1072: when the fix of Gitar differs or has a flaw, the session applies its own fix with no question.

### The state of the build

- Local head before this entry: `04fa13c`. `make verify` parts passed on this machine: build, 3197 tests, format, lint, STE, identity, content, atlas, and smoke.
- CI run `36061445475` passed each job but `screen-test` and `review-gate`. The screen-test flake of OQ-246 took reruns, then the job reached the baseline step.
- The new baselines come from that artifact: 90 changed captures and `pit-torch-1x`. The battle frames move one level, because the patrols of the fixture now see 2 tiles and the fight starts later (D-1067). The smoke fight ends at tick 2443, and at tick 2186 on `main`.

### What is in flight

- PR #76 is open. The fix of the Gitar finding waits for CI, then `make codex-review PR=76 -- --skip-gitar-review` runs.

### Traps and gotchas

- `make sheet` with no fixture writes every capture and then fails in the join of the sheet: the joined picture passes the height limit of 65535 pixels. Take one fixture at a time with `FIXTURE=`.
- `make sheet` clears `artifacts/captures` on each run.
- A Perl substitution with `|` as its delimiter reads an escaped `\|\|` in its pattern as an empty choice. Use another delimiter.

### The questions that block progress

None. OQ-246 stays open and blocks no PR.

### The next concrete action

When CI of the fix commit passes each job but `review-gate`, run `make codex-review PR=76 -- --skip-gitar-review` in the background (D-926, D-1061).

## Session 277: 2026-09-24, Codex

Author: Codex
Session: reviewer PR #75, round 3. Repository: the-thing-below. Branch: `feat/pr-99-stats-absorb-swap`. PR: #75. Role: reviewer. Base: `f1ab753`.

### What this session did, and why

- Re-reviewed PR #75 at effective head `8392c44`. Claude Code authored the PR, so Codex passes the provider gate (T-4, D-17).
- Confirmed P2-1 is fixed. The regression test passed in the prior round, and current CI passed at this head.
- Inspected D-1061, all 86 changed paths, and the current CI results. The review record now gives `Ready for owner merge`.

### The state of the build

- Base and merge base: `f1ab753`. Effective and remote head before this metadata commit: `8392c44`.
- CI run `36046700241` passed the changed-path checks, screen-test, smoke, coverage, and ste-check. Review-gate waits for this record.

### What is in flight

- This commit holds the review record and this handoff entry. It must be pushed and verified.

### Traps and gotchas

- Local `dotnet test` reported no test projects for the Microsoft.Testing.Platform setup. Current CI build and test passed.
- Gitar's status notice has no item and needs no answer (D-964). The pass was skipped under D-946.

### The questions that block progress

OQ-246 remains open and blocks no PR.

### The next concrete action

Push the metadata commit to `feat/pr-99-stats-absorb-swap`. Fetch, verify the branch status and PR head, and read the review-gate result.
