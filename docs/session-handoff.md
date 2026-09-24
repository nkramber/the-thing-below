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

## Session 276: 2026-09-24, Claude Code

Author: Claude Code
Session: author PR-99, round 3. Repository: the-thing-below. Branch: `feat/pr-99-stats-absorb-swap`. PR: #75. Role: author. Base: `f1ab753`.

### What this session did, and why

- The repeat review of Session 275 gave `Blocked` on `15945f2`. P2-1 was fixed, and screen-test had failed on the three frames of OQ-246.
- The review ran after a failed screen-test. The owner set D-1061: the review starts only when each check but `review-gate` passes, and a flake gets reruns first.
- The owner approved the string batch as written (D-57), and the PR description says so.
- Screen-test attempt 3 of run `36041907957` passed on `15945f2`. The CI run `36044147120` of the review commit `d6c4f3a` was cancelled, and its rerun passed each job.

### The state of the build

- Remote head before this commit: `d6c4f3a`. Every check but `review-gate` passed on it.

### What is in flight

- The repeat review, after the CI of this commit passes.

### Traps and gotchas

- A rerun of an old run can cancel the run of a newer commit of the PR, because the runs share a concurrency group. Read the run of the PR head with `gh pr checks`.
- A chain of a watch and a review must stop on a red check (D-1061).

### The questions that block progress

None. OQ-246 stays open and blocks no PR.

### The next concrete action

Wait for every check but `review-gate` to pass on this commit. Then run `make codex-review PR=75 -- --skip-gitar-review`.

## Session 275: 2026-09-24, Codex

Author: Codex
Session: reviewer PR #75, round 2. Repository: the-thing-below. Branch: `feat/pr-99-stats-absorb-swap`. PR: #75. Role: reviewer. Base: `f1ab753`.

### What this session did, and why

- Re-reviewed the heal correction from `26d20cc` to `15945f2`.
- Verified that the regression test passes at the content limits. The old product overflowed before the health cap.
- `make verify` passed on macOS arm64 with 3,153 tests.
- Updated `docs/reviews/pr-75.md`. P2-1 is fixed in `15945f2`.

### The state of the build

- Base and merge base: `f1ab753`. Effective head: `15945f2`. Remote metadata tip before this follow-up update: `3cdfad6`.
- CI run `36041907957` passed implementation checks except `screen-test`, which failed on three captures. The rerun repeated the same differences. The remote still points to `15945f2`.

### What is in flight

- The review record and this entry were pushed in metadata commit `3cdfad6`. The PR still needs its three screen-test differences resolved.

### Traps and gotchas

- The screen differences are a few channel values. The three capture frames look unchanged against their baselines.
- Gitar's only comment says “Gitar is working”. It has no item and does not block the verdict (D-964). The pass was skipped under D-946.

### The questions that block progress

None. OQ-247 and OQ-248 resolve in D-1052 and D-1055.

### The next concrete action

The author needs to resolve the three CI screen differences before approval. The metadata push triggered fresh CI checks.

## Session 274: 2026-09-24, Claude Code

Author: Claude Code
Session: author PR-99, round 2. Repository: the-thing-below. Branch: `feat/pr-99-stats-absorb-swap`. PR: #75. Role: author. Base: `f1ab753`.

### What this session did, and why

- Answered P2-1 of `docs/reviews/pr-75.md` with full merit. The heal math overflowed a `long` at the content limits before its health cap (T-2).
- `BattleMath.HealAmount` splits the scaled share at the scale of 10^12, so the product stays in a `long` and the one rounding of D-169 holds. Each heal that did not overflow gives the same number, and the identity file does not change.
- `StatSetTests.AHealAtTheLimitsOfTheContentCompletesAndStopsAtFullHealth` fails on `26d20cc` with `OverflowException` and passes on the correction. `docs/reviews/pr-75-response.md` records the answer.

### The state of the build

- `make verify` passed on the Mac: 3,153 tests, format, det-lint, STE, identity, content, atlas, and smoke.
- Round 1 CI run `36039388273` at `26d20cc` passed each job but `review-gate`, which waited for the record.

### What is in flight

- The repeat review of `make codex-review PR=75 -- --skip-gitar-review` (D-946).

### Traps and gotchas

- `make sheet` still fails to join the frames: the sheet passes the PNG height limit. The fault is older than this PR.
- Gitar posted a status notice alone, with no item (D-964).

### The questions that block progress

None. The owner has not yet approved the string batch of the PR description (D-57).

### The next concrete action

Push, confirm the remote head, and run the repeat review. On `approve`, ask the owner to confirm the merge with the summary of D-942.

## Session 273: 2026-09-24, Codex

Author: Codex
Session: reviewer PR #75, round 1. Repository: the-thing-below. Branch: `feat/pr-99-stats-absorb-swap`. PR: #75. Role: reviewer. Base: `f1ab753`.

### What this session did, and why

- Reviewed PR #75 from merge base `f1ab753` through effective head `26d20cc`.
- Confirmed Claude Code authored the PR, so Codex passes the provider gate (T-4, D-17).
- Traced all 84 changed paths across stats, strikes, heals, absorbs, lesson swaps, saves, replay, content, UI, and tests.
- Read all eight changed screen frames from CI. No visual fault was found (D-731, D-784).
- Found P2-1: a valid heal can overflow before its health cap.
- Added `docs/reviews/pr-75.md` with `Changes required` for `26d20cc`.
- `make verify` passed with 3,152 tests.

### The state of the build

- Base and merge base: `f1ab753`. Effective head and remote head before this metadata commit: `26d20cc`.
- CI run `36039388273` passed the build, test, and format matrix; smoke on all three legs; det-lint; replay identity on all three legs; coverage; screen-test; and ste-check. `review-gate` failed because the review record was absent.

### What is in flight

- This metadata commit holds the review record and this entry. The author needs to correct P2-1 and request a repeat review.

### Traps and gotchas

- The only Gitar comment says “Gitar is working.” It has no item and needs no answer (D-964). The owner requested `--skip-gitar-review`; the pass was not required (D-946).
- Maximum valid content values make the intermediate product exceed `long`, but the final heal fits `int`.

### The questions that block progress

None. OQ-247 and OQ-248 resolve in D-1052 and D-1055.

### The next concrete action

Correct P2-1, push the correction with a new handoff entry, then run a repeat review of PR #75.

## Session 272: 2026-09-24, Claude Code

Author: Claude Code
Session: author PR-99, round 1. Repository: the-thing-below. Branch: `feat/pr-99-stats-absorb-swap`. PR: #75. Role: author. Base: `f1ab753`.

### What this session did, and why

- Asked OQ-247, OQ-248, and each question of the scope. D-1052 to D-1060 hold the answers. D-356, D-979, and D-1041 carry the revisions in their Effect column.
- Core: seven stats. A strike field sets attack against defense or magic against resistance. A heal is a base plus a share of magic, and it rolls the hit factor. An absorb heals a quarter of the hit, at least 1.
- The swap place, its debug command, and its snapshot field are gone. Save format 12 drops the field, and formats 10 and 11 check it and drop it. The simulation version is 24.
- Game: MAG and RES in the status window and the level-up lines. The gear window shows a line of trial stats in grey, green, and red.
- Content, the test fixture, the identity file, the content hash, and `TheThingBelow.Tests/saves/format-12.json` follow. `StatSetTests` proves the new rules.

### The state of the build

- `make verify` parts ran on this machine: build, 3,152 tests green, format, det-lint, STE, and smoke.
- Frames read one at a time from `artifacts/captures`: the gear window in both stages, the status window, both lesson frames, and the level-up rise. Each reads right.
- The battle-experience frame moved: the grunt mend now heals more, so Marrek ends the fight at 40 health, not 47.

### What is in flight

- CI run `36038453605` passed every job but two. `screen-test` differed in 8 frames, and the baselines of this round come from its artifact (D-731). `review-gate` waits for the review record.

### Traps and gotchas

- `make sheet` captures each frame, then fails to join them: the sheet is 76,628 pixels high, and PNG holds 65,535. This PR adds no frame, so the fault comes from an earlier frame count.
- The test fixture sets magic to attack and resistance to defense, so the old damage numbers hold. `StatSetTests` sets them apart.
- The gear sum of the tests: the weak charm now adds 2 magic and costs 1 resistance.

### The questions that block progress

None.

### The next concrete action

Read the CI result of the baselines. Then read Gitar one time, and run `make codex-review PR=<n> -- --skip-gitar-review` (D-945, D-946).
