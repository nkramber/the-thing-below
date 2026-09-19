# Session handoff

## Session 130: 2026-09-19, Claude Code

Author: Claude Code
Session: the refutation of F-84 and D-673. Repository: the-thing-below. Branch: `docs/pr-88-refute-f-84`. Role: author. Base: `122f3ef`.

### What this session did, and why

- F-84 said that PR #33 merged with no review record, and that RG 3 reports nothing when the file is absent. Both claims are false.
- `docs/reviews/pr-33.md` arrived in the squash commit 3204545 of PR #33 itself, and `git log --follow` gives that one commit.
- The record names Claude Code as the author and Codex as the reviewer, with the verdict for head `d1b2305`.
- `TheThingBelow.Tools/ReviewGate/ReviewRecordRules.cs` faults on an absent file, and it has one commit, 9787b2d of PR #21.
- The gate of PR #33 faulted at head `d1b2305`, then passed at head `320a9bd` after the record landed. It showed both halves of the behavior on the PR that F-84 accuses.
- RG 3 also faulted on PR #35 and passed after that record landed.
- D-680 supersedes D-673, and F-84 now reads `✅ doc` with the evidence and the date.
- D-681 records the branch protection of `main` as an owner action beside this PR. OQ-3 stays open until the protection is live.
- The PR takes no `PR-#` id and no roadmap entry, because it is a document-only correction (D-680).
- The stale out-of-scope bullet of section 7.20 of `docs/roadmaps/phase-1-foundations.md` is gone.

### The state of the build

- `make verify` passes with 794 tests, 0 warnings, and 0 findings from det-lint and the STE check.
- Replay identity, content hash, and the bounded smoke session pass.
- No code change. This PR changes text alone.

### What is in flight

The branch needs its first push and the automated pass of gitar. The PR revises a decision row, so the `review-override` label of D-401 does not apply and the Codex review follows (T-4).

### Traps and gotchas

- The archive and the older handoff entries still read F-84 and D-673 as live. They are dated records, and a rewrite falsifies them (D-10).
- The branch name holds `pr-88`, and no register defines that id. A branch name takes no reference rule (D-605).
- `main` takes any push until the owner enables the protection of D-681.
- Session 119 is absent from both handoff files. The check reads order and duplicates, not a gap.

### The questions that block progress

None. OQ-3 stays open as an owner action, and it blocks line 4 of Gate 1, not this PR.

### The next concrete action

Push the branch, open the PR, and answer the automated pass of gitar.

## Session 129: 2026-09-19, Codex

Author: Codex
Session: repeat review PR #35, the empty option value of every Tools command. Repository: the-thing-below. Branch: `fix/pr-87-content-hash-empty-option`. Role: reviewer. Base: `f896dc3`.

### What this session did, and why

- Reopened the review at effective head `8eacf73`.
- Verified P2-1. The records now state seven option values, five affected commands, and nine regression rows.
- Checked the base trigger, the correction, and the adjacent whitespace boundary.
- Preserved the earlier verdict in `docs/reviews/pr-35.md` and set the current verdict to `Ready for owner merge`.

### The state of the build

- `make verify` passes with 794 tests, 0 warnings, and 0 findings from det-lint and the STE check.
- Replay identity, content hash, and the bounded smoke session pass.
- All nine empty-option probes return exit code 1, name the option, and produce no stack trace.
- `det-lint --root " "` returns a contextual missing-root error without a stack trace.

### What is in flight

The repeat-review record needs a metadata commit and push. The current review-gate run fails because the old record still names `4e9338d` and `Changes required`.

### Traps and gotchas

- The Gitar dashboard summary still says six option values, but its current timestamp covers `8eacf73`. The repository records and the PR description hold the corrected count of seven.
- The effective head is `8eacf73`. Metadata commits after it do not change the review target.

### The questions that block progress

None for this PR. OQ-3 remains an owner question for Gate 1.

### The next concrete action

Commit and push the repeat-review record and this handoff entry. Then verify the remote head and the review-gate result.

## Session 128: 2026-09-19, Claude Code

Author: Claude Code
Session: PR-87, the answer to the Codex review of PR #35. Repository: the-thing-below. Branch: `fix/pr-87-content-hash-empty-option`. Role: author. Base: `f896dc3`.

### What this session did, and why

- P2-1 of `docs/reviews/pr-35.md` has full merit. The records said six option values and listed seven.
- A probe of the base commit `f896dc3` in a worktree measured each of the nine rows again.
- That probe refuted a second count that the review did not name. The records said four commands, and the number is five.
- Five of the seven values end with a stack trace. The other two give a message that names no empty option.
- `det-lint --root` named an absent folder, and `review-gate --head-files` named an access fault of the path.
- F-83, D-678, the roadmap entry, the area file, and the PR description now hold the two corrected counts.
- The exit tests of section 7.20 now state the nine rows of the theory, and which seven fail on the old code.

### The state of the build

- The effective head before this round is `4e9338d`, and the Codex verdict names it.
- `make verify` passes with 794 tests, 0 warnings, and 0 findings from det-lint and the STE check.
- No code changes in this round. The round corrects text alone.

### What is in flight

The repeat review of Codex for the new head. The pass of gitar for the new head.

### Traps and gotchas

- A `git stash push` of a committed change saves nothing, and a probe then measures the new code and reads as the old. Use a worktree at the base commit.
- `review-gate --head-files ""` alone gives the message for the two absent options, and not a stack trace. The stack trace needs both options, with one empty.
- `det-lint --root ""` never ended with a stack trace. A folder check further down named the absent folder.
- Six call sites hold the check, and seven option values pass through them. D-679 counts the call sites.

### The questions that block progress

None for this PR. OQ-3 remains an owner question for Gate 1.

### The next concrete action

Push the correction, answer the pass of gitar, and ask Codex for the repeat review.

## Session 127: 2026-09-19, Codex

Author: Codex
Session: review PR #35, the empty option value of every Tools command. Repository: the-thing-below. Branch: `fix/pr-87-content-hash-empty-option`. Role: reviewer. Base: `f896dc3`.

### What this session did, and why

- Reviewed the complete PR-35 diff from merge base `f896dc3` to effective head `4e9338d`.
- Confirmed the cross-provider gate. Claude Code authored the PR, and Codex reviewed it.
- Verified the shared empty-option parser, all command call sites, regression tests, decision rows, roadmap entries, and handoff records.
- Found P2-1. The records say six option values, but they list seven.

### The state of the build

- The PR tip is `c6e755b`, and the effective implementation head is `4e9338d`.
- `make verify` passes with 794 tests, 0 warnings, and 0 findings from det-lint and the STE check. Replay identity, content hash, and the bounded smoke session pass.
- All nine empty-option probes return exit code 1, name the option, and produce no stack trace.

### What is in flight

The review record `docs/reviews/pr-35.md` records `Changes required` for P2-1. The review record and this handoff entry are not yet published to the PR branch.

### Traps and gotchas

- The implementation covers seven non-atlas option values and two atlas option values. The text says six in multiple places.
- The effective head is `4e9338d`, not the metadata tip `c6e755b`.

### The questions that block progress

None for this PR. OQ-3 remains an owner question for Gate 1.

### The next concrete action

The author corrects the repeated scope count. Then the review reruns the document and review-gate checks before it publishes a final verdict.

Rule (D-18): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md` (D-18). At the start, read the top entry alone (D-584).

## Session 126: 2026-09-19, Claude Code

Author: Claude Code
Session: PR-87, the empty option value of the Tools commands. Repository: the-thing-below. Branch: `fix/pr-87-content-hash-empty-option`. Role: author. Base: `f896dc3`.

### What this session did, and why

- The session asked the owner three questions before any change. D-677 to D-679 answer them.
- A probe of every command found the fault in five commands and seven option values, and F-83 recorded one.
- D-678 sets the scope: every Tools command, and not `content-hash` alone.
- `OptionValue.ReportEmpty` is the one place of the check, and every command reads its option values through it (D-679).
- The `atlas` command moves from its own copy of the check to that helper.
- Section 7.20 of `docs/roadmaps/phase-1-foundations.md` holds PR-87, and Gate 1 moves to 7.21.
- F-83 now records the full set and reads as fixed.

### The state of the build

- `main` is `f896dc3`, and the branch starts there.
- `make verify` passes with 794 tests, 0 warnings, and 0 findings from det-lint and the STE check.
- The suite grew by 12 tests: 9 rows of the command theory and 3 for the helper.
- Every one of the seven option values gives the fault exit code and names the option.

### What is in flight

The review of Codex. The pass of gitar approved the head `4e9338d` with no finding and no open thread.

`review-gate` names RG 3 alone: the review record of Codex at `docs/reviews/pr-35.md`. RG 1, RG 2, and RG 6 to RG 8 pass. Every other check passes on every CI leg.

The first head failed RG 7 with two faults of the PR description. The row `docs/reviews/` had no form of D-581, and it named the roadmap id and not the GitHub number. The row `.claude/skills/` named no path after the reason. The description now holds both forms, and RG 7 passes. No commit changed, so the pass of gitar stands.

### Traps and gotchas

- Seven of the nine theory rows fail on the old code. The two `atlas` rows pass, because PR-34 fixed that command.
- `det-lint --root ""` gave a clean error before this PR, from a folder check further down. The message changes to the parse message.
- The insert of one item renumbered the sequence list of section 7 of `docs/design.md`. The diff is large, and the order does not change.
- No Core behavior changes, so the simulation version stands (G-17).

### The questions that block progress

None for this PR. OQ-3 remains an owner question for Gate 1.

### The next concrete action

The Codex review of PR #35. The reviewer writes `docs/reviews/pr-35.md` with the verdict for the effective head, and RG 3 then passes.

## Session 125: 2026-09-19, Codex

Author: Codex
Session: review PR #34, M-1 and M-2 cost model numbers. Repository: the-thing-below. Branch: `docs/m1-m2-cost-model`. Role: reviewer. Base: `3204545`.

### What this session did, and why

- Reviewed the complete PR-34 diff from merge base `3204545` to effective head `2434847`.
- Confirmed the cross-provider gate. Claude Code authored the PR, and Codex reviewed it.
- Checked the cost tables, arithmetic, document order, decision rows, runbook procedure, Documents section, and the answered gitar finding.
- Wrote `docs/reviews/pr-34.md` with the verdict `Ready for owner merge`.

### The state of the build

- The effective head is `2434847`. The metadata tip is `2789416`.
- `make verify` passes locally with 782 tests, 0 warnings, and 0 findings from det-lint and STE check. Replay identity, content hash, and the bounded smoke session pass.
- The M-2 table arithmetic supports its rounded means and its two-to-three-times Windows summary.

### What is in flight

The review record and this handoff entry are published. The review-gate check is pending its updated result.

### Traps and gotchas

- The effective head is `2434847`, not the metadata tip `1d9fe5b`.
- The first gitar comment found the old Windows ratio statement. The current head records the corrected range and mean.

### The questions that block progress

None for this PR. OQ-3 remains an owner question for Gate 1.

### The next concrete action

Commit and push the review record and this handoff entry. Then fetch and verify the remote head.

## Session 124: 2026-09-19, Claude Code

Author: Claude Code
Session: M-1 and M-2, the cost model numbers of the first ten code PRs. Repository: the-thing-below. Branch: `docs/m1-m2-cost-model`. Role: author. Base: `3204545`.

### What this session did, and why

- The session asked the owner six questions before any change. D-671 to D-676 answer them.
- D-672 sets the M-1 number: the total context tokens of a PR, per harness.
- Section 4 of `docs/design.md` now holds the M-1 table and the M-2 table of the ten PRs, with a mean row.
- M-1: the mean code PR takes 68.4 million context tokens. PR-1 is the worst at 127.1 million.
- M-2: the mean green `ci` run spends 505 seconds across its jobs, and the clock of the run is 133 seconds.
- Windows takes two to three times the seconds of Linux, and 2.4 times at the mean.
- D-671 records the approval of the owner for the 16 colors of D-185, which came after the merge of PR #33.
- D-673 and F-84 record that PR #33 merged with no review record, against T-4 and D-17.
- D-674 puts the fix of F-83 in the session right after this one, and before Gate 1.
- D-675 keeps the Sprite Fusion test and moves it after M-2. D-676 adds it to Gate 1 as line 10.
- The Measures section of `docs/runbooks/session-context.md` now says how to read both numbers again.

### The state of the build

- `main` is `3204545`, and the branch starts there. The effective head is `2434847`.
- `make ste-check` gives 0 findings, and the `ste-check` job passes.
- The code jobs skip, because the changed-paths job reads this PR as a docs-only PR (D-513).
- `review-gate` names RG 3 alone: the review record of Codex. RG 1, RG 2, and RG 6 to RG 8 pass.
- The PR changes documents alone. No code, content, or test file changes.
- The PR adds decision rows, so the label of D-401 does not apply. The PR needs the Codex review (T-4).

### What is in flight

The review of Codex. The pass of gitar approved the effective head `2434847` with no open finding, and it resolved its one thread itself.

That finding had full merit. The first head said that Windows takes about twice the seconds of Linux on every PR, and the M-2 table refutes it. The ratios run 2.04 to 3.00, and PR-3 is 3.00. Both sentences now give the range and the mean.

The pass also read the `review-gate` failure of the first head, which named RG 7. The line of the row `docs/reviews/` had no form of D-581. The PR description now names `docs/reviews/pr-34.md`, and RG 7 passes.

### Traps and gotchas

- The M-1 number counts a cache read, so it reads far larger than the money cost. D-672 gives the reason.
- M-1 attributes each record by its branch. Work on `main` counts for no PR.
- PR-4 holds a second branch, feat/pr-4-core-math-streams, which opened no PR. Its tokens are in the PR-4 row.
- The M-2 table reads the newest green `ci` run of each PR. Each PR started the workflow 3 to 16 times.
- The owner reversed the first answer on the Sprite Fusion test inside the session. The test stays (D-675).
- The owner chose the tenth Gate 1 line against the recommendation of the session. D-676 records both sides.
- No tool of this repository reads a harness record (D-99), so the scripts of this session stay outside the checkout.
- 7.17 of the phase file keeps its number, because D-13 refuses a renumber. Its position line names D-675.
- The next ids are D-677, OQ-197, F-85, L-16, G-29, M-9, and Session 125.

### The questions that block progress

None. OQ-3 still blocks Gate 1, and it belongs to the owner.

### The next concrete action

Push the branch, open the PR, and answer the pass of gitar. Then hand the PR to Codex for the review.

## Session 123: 2026-09-19, Codex

Author: Codex
Session: review PR #33, PR-34, the atlas, the palette, and the drawing files. Repository: the-thing-below. Branch: `feat/pr-34-atlas-and-palette`. Role: reviewer. Base: `9863da3`.

### What this session did, and why

- Reviewed the complete PR-33 diff from merge base `9863da3` to effective head `d1b2305`.
- Confirmed the cross-provider gate. Claude Code authored the PR, and Codex reviewed it.
- Traced the drawing and atlas readers, deterministic layout, pixel comparison, content-set checks, error paths, palette validation, committed content, and Documents section.
- Found no actionable finding. Wrote `docs/reviews/pr-33.md` with the verdict `Ready for owner merge`.

### The state of the build

- The implementation head is `d1b2305`. The metadata tip is `abad4da`.
- `make verify` passes locally with 782 non-smoke tests and 0 build warnings. The atlas check, format, det-lint, STE check, replay identity, content hash, Godot build, and bounded smoke session pass.
- The focused empty-option probes return contextual errors with exit code 1.

### What is in flight

The review record and this handoff entry are ready to publish. The final Windows and macOS CI results were still in progress when the review ran.

### Traps and gotchas

- The effective head is `d1b2305`, not the metadata tip `09ec5a8`.
- The review record must keep the effective head because the review commit changes only metadata paths.

### The questions that block progress

None.

### The next concrete action

Commit and push the review record and this handoff entry. The owner can merge after the remaining PR checks pass.

## Session 122: 2026-09-19, Claude Code

Author: Claude Code
Session: PR-34, the atlas, the palette, and the drawing files. Repository: the-thing-below. Branch: `feat/pr-34-atlas-and-palette`. Role: author. Base: `9863da3`.

### What this session did, and why

- The session asked the owner five questions before any change. D-666 to D-670 answer them.
- D-666 gives each kind of drawing its own page, 2048 pixels at most on a side, so one art change rewrites one page.
- D-667 puts a tile on a strict grid of 32 by 32 cells, which a `TileSetAtlasSource` reads with no translation.
- D-668 keeps the form of the sample sheet: each drawing at 1x and 6x, on a night ground and on a snow ground.
- D-669 sets the time of a frame in ticks, 60 to a second. D-670 gives a cast member the kind `cast`.
- Core gained `AtlasPages`, `Drawing`, and `AtlasIndex`, with the strict reader of each file (D-515, D-517).
- `ContentSet` now reads every drawing file and the index, and it refuses a key that the palette lacks and an index that does not match the drawings.
- `TheThingBelow.Tools/Atlas` holds `AtlasLayout`, `AtlasCanvas`, `AtlasIndexText`, `SheetFont`, `SwatchSheet`, `ReviewSheet`, and `AtlasCommand`.
- The palette holds the 64 colors of D-181, with the 16 colors of D-185 at index 48 to 63.
- The five approved cast grids of `docs/samples/` are drawing files under `content/sprites/drawings/cast/`.
- The PR retires the interim atlas script, which D-406 kept as a reference.

### The state of the build

- `main` is `9863da3`, and the branch starts there. The effective head is `d1b2305`.
- Every CI check passes but `review-gate`, which names RG 3 alone: the review record of Codex. The pixel test of the atlas passes on Windows, Linux, and macOS (D-481, G-24).
- `make verify` passes with 780 tests and 0 build warnings. Format, `det-lint`, `ste-check`, replay identity, content hash, the Godot build, and the bounded smoke session pass.
- det-lint reads `TheThingBelow.Tools/Atlas` with the rules of D-502, and it names Audio and NormalMaps as absent (G-16).
- `SimulationVersion.Current` stays 3. No rule file reads the palette or the atlas, so the content hash does not move (D-495, G-17).

### What is in flight

The review of Codex. The pass of gitar approved the effective head `d1b2305` with no open finding, and it resolved its one thread itself. That finding had full merit. `atlas --root ""` and `atlas --sheets ""` ended with a stack trace, against T-2. The command now reads an empty option value at the parse, as `det-lint` does. The pass proposed a catch of `ArgumentException`, which would hide a fault of the code, so the fix reads the value instead. F-83 records the same shape in the `content-hash` command, which needs a PR of its own (G-8). The CI block of the pass names RG 3, which waits for the review record of Codex.

The two sheets are in the PR description (D-514, D-668, G-25).

### Traps and gotchas

- `CLAUDE.md` and `AGENTS.md` reached the 16 KB limit of D-611. The command list now names the Makefile targets, which are the same commands.
- The Game project embeds `content/**/*.png` now, so the atlas page travels in the assembly (D-508).
- A page of few drawings is only as large as the drawings need. A tile page keeps the full grid width, so a new tile moves no other tile (D-667).
- The atlas command reads the drawing files itself, not through `ContentSet`, because a content set needs the index that the command writes.
- 7.17 of the phase file puts the Sprite Fusion test before PR-34 (D-620). It did not run. The pipeline takes a text grid from any source, so the pick changes no code of this PR.
- The next ids are D-671, OQ-197, F-83, L-16, G-29, M-9, and Session 123.

### The questions that block progress

None.

### The next concrete action

Push the branch, open the PR, attach the two sheets, and answer the pass of gitar.

## Session 121: 2026-09-19, Codex

Author: Codex
Session: review PR #32, PR-47, the PNG reader and the PNG writer. Repository: the-thing-below. Branch: `feat/pr-47-png-code`. Role: reviewer. Base: `a607280`.

### What this session did, and why

- Reviewed the complete PR-32 diff from merge base `a607280` to effective head `e45dcc9`.
- Confirmed that Claude Code authored the implementation and Codex reviewed it.
- Traced PNG chunk parsing, CRC-32 checks, size limits, zlib output length, row filters, image ownership, writer output, file errors, and the det-lint reference fix.
- Found no actionable finding. Wrote `docs/reviews/pr-32.md` with the verdict `Ready for owner merge`.

### The state of the build

- The implementation head is `e45dcc9`. The metadata tip is `5d14a83`.
- `make verify` passes with 706 tests and 0 build warnings. Format, `det-lint`, `ste-check`, replay identity, content hash, Godot build, and the bounded smoke session pass.
- GitHub reports the implementation checks green. The review-gate check passes after the review record push.

### What is in flight

The remote review-gate passes for metadata tip `caec10d`. Gitar remains pending on the metadata tip, and its current approved dashboard covers effective head `e45dcc9`.

### Traps and gotchas

- PR #32 is roadmap PR-47. The review record uses GitHub PR number 32.
- The review commit changes only the metadata set, so it does not move the effective head.
- The review-gate check was red before the review record existed. It passes after the metadata push.

### The questions that block progress

None.

### The next concrete action

The review-gate check reads `docs/reviews/pr-32.md` for effective head `e45dcc9`. The owner can merge after the remaining PR checks pass.
