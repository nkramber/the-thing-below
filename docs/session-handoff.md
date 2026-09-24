## Session 260: 2026-09-24, Claude Code

Author: Claude Code
Session: author PR-62, round 1. Repository: the-thing-below. Branch: `feat/pr-62-menu-windows`. PR: #70. Role: author. Base: `a929c59`.

### What this session did, and why

- Asked OQ-111 and OQ-113 first, then the gaps that the code found: the map action, the log place, the dim entries, the notice source, the status sheet, the list order, the exit mark, and the time of a notice. D-982 to D-996 record the answers.
- Core: the notice file, the notice rule, the notice log of 30 entries, and the row intent of the party window. Save format 8 and simulation version 20. The debug console posts a notice with `notice` and `aside`.
- Storage: settings format 2, whose step adds the map action with M and Back.
- Game: the main list, the party, status, and log windows, the dungeon map screen, the notice box, and the map action. The settings screen opens from the main list.

### The state of the build

- `main` is `a929c59`. The branch holds three decision commits, the Core commit `10985be`, and the Game commit `13829ba`.
- Local: build, format, det-lint, STE, identity, content, and smoke pass. 2695 tests pass. The 8 new captures have no baseline yet, so 8 baseline tests fail.
- The author read each new frame of `make sheet FIXTURE=menu` and `FIXTURE=notice` (D-784).

### What is in flight

- The first CI run. It fails on the 8 absent baselines alone. Then the baseline commit from its `screen-captures` artifact (D-733), one read of Gitar, and the Codex review.

### Traps and gotchas

- `RunState.Start`, `Resume`, `Simulation`, and `RunReplay.Play` take the notice file. Tests use `TestBattles.Notices`.
- The capture session builds no input map, so the menu captures build each view and send no event.
- The glossary refuses "banner". The box at the top edge is the notice box.

### The questions that block progress

None. The PR description holds the game text batch for the owner (D-57).

### The next concrete action

Commit the 8 baselines from the artifact of the first CI run. Then wait for CI, read Gitar once under D-945, and run `make codex-review PR=70 -- --skip-gitar-review`.

## Session 259: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #69, round 1. Repository: the-thing-below. Branch: `feat/pr-67-character-level`. PR: #69. Role: reviewer. Base: `5b56d3d`.

### What this session did, and why

- Reviewed PR #69 from merge base `5b56d3d` through effective head `9604e8f`.
- Confirmed Claude Code authored the change, so Codex passes the provider gate (T-4, D-17).
- Traced experience, levels, MP, save migration, replay state, battle events, and the battle view. Inspected all 81 changed paths.
- Read all 18 changed battle frames from the screen-test artifact. No visual fault was found (D-733, D-784).
- Added `docs/reviews/pr-69.md` with `Ready for owner merge` for `9604e8f`.

### The state of the build

- `main` and the PR base are `5b56d3d`. The remote PR head before this metadata commit is `9604e8f`.
- Focused tests passed, 89 of 89, after `make build` created the Game assembly.
- CI run 35931706529 passed the implementation checks on every platform. Review-gate failed because the review record was absent.

### What is in flight

- This metadata commit holds the review record and this handoff entry. The push must make the new review-gate check pass.

### Traps and gotchas

- Gitar's only comment is a status notice without an item. D-964 says it needs no answer and does not block the verdict.
- Focused tests that read the Game assembly need `make build` first.

### The questions that block progress

None for PR-69.

### The next concrete action

Commit and push the review record and handoff together. Fetch, verify the remote head, and confirm the fresh review-gate result.

## Session 258: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-67, round 1. Repository: the-thing-below. Branch: `feat/pr-67-character-level`. PR: #69. Role: author. Base: `5b56d3d`.

### What this session did, and why

- Asked OQ-134, OQ-135, OQ-136, and the new OQ-244 before any code, then the numbers, the summary, and the text. D-966 to D-981 record the answers.
- The owner revised D-387: a downed character earns no experience (D-974). The summary rises above each head on the battle screen, and each character shows a health bar and an MP bar (D-975, D-976).
- Core: the stat curve and the join level of each character, the enemy level and experience, the cut, the gap, and the experience table. It also adds the award at a win, the level-up fill, and the restore rules of D-970. Save format 7 and simulation version 19.
- Game: the text of the summary, the party bars, and "HP 60/60 MP 8/8" on the bottom line. The captures `battle-experience-1x`, `battle-level-up-rise-1x`, and `battle-level-up-1x` are new.

### The state of the build

- `main` is `5b56d3d`. PR #69 holds the decision commit, the round commit `b03ef23`, and the baseline commit.
- CI run 35931067896 failed on the three new frames and 15 changed battle frames alone. The baseline commit holds those 18 files from its `screen-captures` artifact (D-733). Format, det-lint, STE, identity, content, atlas, and smoke pass.

### What is in flight

- CI on the baseline commit. Then one read of Gitar, and the Codex review.

### Traps and gotchas

- The glossary term "share" means the part of health that poison moves. The experience code says "shrunk", and the glossary now holds the new terms of PR-67.
- The capture session builds a new screen for each frame, so the message line of a summary frame is empty. The game keeps the last line.
- The two level-up frames stage a level-up on the view. The fixture fight gives 12 experience, and level 2 takes 20 (D-977).
- The status panel holds a name of 8 characters at body 32 (D-981).

### The questions that block progress

None.

### The next concrete action

Wait for CI on the baseline commit to finish green except `review-gate`. Read Gitar once under D-945, then run `make codex-review PR=69 -- --skip-gitar-review`.

## Session 257: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #68, round 3. Repository: the-thing-below. Branch: `feat/pr-98-waiting-enemies`. PR: #68. Role: reviewer. Base: `3223bcf`.

### What this session did, and why

- Re-reviewed PR #68 at effective head `146f605`. The author added D-964, which says that a Gitar comment with no item needs no answer.
- Confirmed Claude Code authored the substantive changes. Codex passes the provider gate (T-4, D-17).
- Read the PR comments. The only Gitar comment is a status notice with no item, so it does not block the verdict (D-964).
- Ran `make verify`; all 2,524 tests and the local checks passed. CI implementation checks passed. Updated `docs/reviews/pr-68.md` to `Ready for owner merge`.

### The state of the build

- `main` and the PR base are `3223bcf`. The effective head and remote head before this metadata commit are `146f605`.
- CI run 35923363333 passed the implementation checks. CI run 35923364396 failed RG 5 because the review record named `caaea8f`, not the new effective head `146f605`.

### What is in flight

- This metadata commit holds the review record and this handoff entry. A fresh `review-gate` result must pass after publication.

### Traps and gotchas

- The Gitar status notice has no item and needs no answer under D-964.
- A new prompt and test change made `146f605` the effective head. The older approval covers only `caaea8f`.

### The questions that block progress

None. OQ-243 is resolved by D-963.

### The next concrete action

Commit and push the review record and handoff together. Fetch, then verify the remote head and the new `review-gate` result.

## Session 256: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-98, round 3. Repository: the-thing-below. Branch: `feat/pr-98-waiting-enemies`. PR: #68. Role: author. Base: `3223bcf`.

### What this session did, and why

- Read the record of Session 255: `Ready for owner merge` for `caaea8f`. The owner stopped the merge first, because the review of round 1 blocked on a Gitar status notice alone.
- Asked the owner two questions. The owner put a new Gitar rule in PR-98, an exception to G-8 (D-965). The owner chose that neither side acts on a Gitar comment with no item (D-964).
- D-964: a Gitar item is a review thread, a finding of the dashboard, or a claim of the CI analysis. A comment with no item needs no answer, and the reviewer ignores it. D-14, D-66, D-67, D-944, and D-946 are revised in part.
- Tools: every prompt of `make codex-review` now holds the D-964 line, with the flag of D-946 or without it. A test reads both forms.
- Rules: `CLAUDE.md`, `AGENTS.md`, the `pr-review`, `gitar-review`, `one-pr-one-session`, and `ste-writing` skills, `docs/runbooks/merge.md`, the PR template, and `docs/design.md`.

### The state of the build

- `main` is `3223bcf`. The effective head moves with the change of Tools, so the approval of `caaea8f` no longer covers the head.
- `make build`, `make test` (2524 tests), `make format`, and `make ste-check` passed on macOS arm64.

### What is in flight

- The CI run of this push, then `make codex-review PR=68 -- --skip-gitar-review`.
- After an approval, the merge question to the owner in four sections (D-942).

### Traps and gotchas

- `CLAUDE.md` and `AGENTS.md` are 16382 bytes, 2 bytes under the limit of 16 KB. A later rule line needs a cut elsewhere.
- The stopped review of round 2 pushed its approval before the stop. The new head needs a new review.

### The questions that block progress

None.

### The next concrete action

Wait for CI on the head, read the Gitar output one time, then run `make codex-review PR=68 -- --skip-gitar-review`.

## Session 255: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #68, round 2. Repository: the-thing-below. Branch: `feat/pr-98-waiting-enemies`. PR: #68. Role: reviewer. Base: `3223bcf`.

### What this session did, and why

- Re-reviewed PR #68 at effective head `caaea8f`. Commits after that head change review and handoff metadata alone (D-610).
- Confirmed that Claude Code authored the change and Codex passes the provider gate (T-4, D-17).
- Verified the owner's answer to the existing Gitar status notice. No review or inline comments exist (D-945, D-946).
- Updated `docs/reviews/pr-68.md` with the current verdict and the prior verdict history.

### The state of the build

- `main` is `3223bcf`. The effective head is `caaea8f`. The remote tip before this metadata commit is `fde91a6`.
- CI run 35920519185 passed the implementation checks at `caaea8f`. CI run 35922327323 passed the applicable checks at `fde91a6`, but `review-gate` read the prior Blocked verdict and failed.

### What is in flight

- This metadata commit holds the updated review and this handoff entry. A fresh `review-gate` result must pass after the push.

### Traps and gotchas

- The effective head remains `caaea8f`. The commits after it change only paths in the metadata set (D-610).
- The Gitar pass is not a review condition under D-945 and D-946. The author answered the existing status notice.

### The questions that block progress

None. OQ-243 is resolved by D-963.

### The next concrete action

Push this metadata commit, then verify the remote head and the fresh `review-gate` result.

## Session 254: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-98, round 2. Repository: the-thing-below. Branch: `feat/pr-98-waiting-enemies`. PR: #68. Role: author. Base: `3223bcf`.

### What this session did, and why

- Read the record of Session 253: `Blocked` for `caaea8f`, with no finding. The one open item is the Gitar status notice, which had no answer.
- Answered that notice in a PR comment. It holds no thread, no finding, and no claim (D-945, D-946).

### The state of the build

- `main` is `3223bcf`. The effective head is `caaea8f`. The commits after it change the metadata set alone (D-610).
- CI passed each check at `caaea8f` except `review-gate`, which reads the verdict.

### What is in flight

- A new `make codex-review PR=68 -- --skip-gitar-review` on the answer of this round.
- After an approval, the merge question to the owner in four sections (D-942).

### Traps and gotchas

- A Gitar status notice counts as a comment that needs an answer, even with no finding.

### The questions that block progress

None.

### The next concrete action

Run `make codex-review PR=68 -- --skip-gitar-review`, and read its outcome.

## Session 253: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #68, round 1. Repository: the-thing-below. Branch: `feat/pr-98-waiting-enemies`. PR: #68. Role: reviewer. Base: `3223bcf`.

### What this session did, and why

- Reviewed PR #68 from base `3223bcf` through effective head `caaea8f`.
- Verified Claude Code authored the changes and Codex passes the provider gate (T-4, D-17).
- Checked the load limit, the screen layout, the step-in order, the target menu, the simulation version, and the replay identity.
- Ran `make verify`, checked CI, and read all 15 affected battle frames of the screen-test artifact.
- Found no code defect. The existing Gitar status comment has no author answer. The review is Blocked until the author answers it.
- Corrected the Documents row of the PR description and added `docs/reviews/pr-68.md`.
- Committed and pushed the review and handoff as metadata commit `52aa9a8`. The fresh review-gate check failed RG 4 because the verdict is Blocked.

### The state of the build

- `main` and the PR base are `3223bcf`. The effective head is `caaea8f`.
- `make verify` passed on macOS arm64 with 2,522 tests. CI run 35920519185 passed the implementation checks on each platform.
- The initial `review-gate` check failed because this record did not exist. After publication, RG 3 and RG 5 to RG 8 passed. RG 4 failed because the verdict is Blocked.
- CI run 35921902597 passed the metadata checks. The platform matrix jobs skipped because the commit changed metadata paths alone.

### What is in flight

- The author needs to answer the existing Gitar status comment.
- The owner needs to start a fresh review after the author answers.

### Traps and gotchas

- The Gitar comment says “Gitar is working.” It has no thread or finding, but the user requires an answer to each existing Gitar comment.
- The Gitar pass itself is not a review condition under D-945 and D-946.

### The questions that block progress

OQ-243 is resolved by D-963. The unanswered Gitar status comment blocks approval.

### The next concrete action

The author answers the Gitar status comment. Then start a fresh review of PR #68.

## Session 252: 2026-09-23, Claude Code

Author: Claude Code
Session: author PR-98, round 1. Repository: the-thing-below. Branch: `feat/pr-98-waiting-enemies`. PR: #68. Role: author. Base: `3223bcf`.

### What this session did, and why

- Asked the owner OQ-243. The session found that four elite bodies fit in the waiting column, and not six as D-954 said. The owner chose the load failure (D-963).
- Recorded D-963, marked D-954 as revised in part, resolved OQ-243, and updated `docs/design.md` and both roadmaps.
- Core: the load refuses a group whose waiting column is taller than 288 art pixels. The error names the group, the height, and the limit. The simulation version is 18, and the identity file changed with it (G-17).
- Game: each waiting enemy draws at full size in the dim color, in a column at the left edge. The column stands on the bottom of its room, and the next enemy that steps in stands at the top. The rows of the enemies stand 40 columns to the right.
- Read the frames `battle-waiting-1x`, `battle-sparks-1x`, and `battle-target-1x` of `make sheet FIXTURE=battle` (D-784). A column in the middle of its room put a lone grunt in the wall above the ground, so the column now stands on the bottom. Exit test 3 of the roadmap changed with it.
- Added the `waiting-1x` capture of the fight of the deep room, and tests of the load check, the column, the step in, and the target menu.

### The state of the build

- `main` is `3223bcf`. The branch holds the code, the tests, and the documents of PR-98.
- `make verify` passed on macOS arm64, except the baseline of `battle-waiting-1x.png`, which only the CI artifact gives (D-733).
- CI run 35919855654 passed smoke, replay-identity, det-lint, and ste-check on each leg. The build legs and screen-test failed on the new battle frames alone. The 15 battle frames of its `screen-captures` artifact are the new baseline, and the author read each one (D-784).

### What is in flight

- The CI run on the baseline commit.
- The Codex review: `make codex-review PR=68 -- --skip-gitar-review` (D-946).

### Traps and gotchas

- The patrol of a test map takes the size of the largest enemy of the tests content, so a test wave with a brute fails the map. The wave test uses grunts alone.
- The elite group of the tests holds its waiting grunts in slots 3 and 4. The group of the content holds its waiting grunt in slot 1.
- The waiting grunt reads very dark: the grunt art is dark, the dim color multiplies it, and the left edge takes little of the key light. The owner judges the shade from the sheet.

### The questions that block progress

None.

### The next concrete action

Read the CI result of the baseline commit, read the Gitar output one time, then run `make codex-review PR=68 -- --skip-gitar-review` in the background (D-945, D-946).

## Session 251: 2026-09-23, Codex

Author: Codex
Session: reviewer PR #67, round 3. Repository: the-thing-below. Branch: `feat/pr-11-evaluator`. PR: #67. Role: reviewer. Base: `d429d03`.

### What this session did, and why

- Re-reviewed PR #67 through effective head `a1f145d` after the cost correction and Deck measurement.
- Verified P2-1 is fixed at `ad83e20`. The author recorded two Deck runs at 74 us and 103 us, both below the D-961 limit.
- Verified the answer to the existing Gitar status comment. It has no thread or finding.
- Updated the review record, corrected the test count and Documents row in the PR description, and set the verdict to Ready for owner merge.

### The state of the build

- `main` and the PR base are `d429d03`. The effective head is `a1f145d`.
- `make verify` passed on macOS arm64 with 2,507 tests. CI run 35911353733 passed the shared checks. Platform implementation jobs passed at `ad83e20`.
- The review-gate check failed against the prior blocked review record. A fresh result after this metadata commit is required.

### What is in flight

- The review record and this handoff entry need one metadata commit and a push to `feat/pr-11-evaluator`.
- Read the new review-gate result after the push.

### Traps and gotchas

- The effective head is `a1f145d`, because the Deck procedure and F-53 changed outside the metadata set.
- The Deck result comes from two Release runs of the cost command on the code at `ad83e20`.

### The questions that block progress

OQ-243 applies to PR-98 alone (D-951 to D-954).

### The next concrete action

Run the metadata commit and push gate, then confirm the published review-gate result.
