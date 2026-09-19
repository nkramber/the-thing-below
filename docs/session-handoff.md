# Session handoff

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

## Session 120: 2026-09-18, Claude Code

Author: Claude Code
Session: PR-47, the PNG reader and the PNG writer. Repository: the-thing-below. Branch: `feat/pr-47-png-code`. Role: author. Base: `a607280`.

### What this session did, and why

- The session asked the owner three questions before any change, and D-663 to D-665 answer them.
- D-663 keeps the CRC-32 in Tools, so the PR adds no package (OQ-72, G-13).
- D-664 puts the five row filters in the reader and the filter None in the writer, because an image editor writes filtered rows (OQ-195).
- D-665 puts two files of an outside encoder in git, and each failure case is bytes of the test (OQ-196).
- `TheThingBelow.Tools/Png` holds `Crc32`, `PngColorKind`, `PngException`, `PngImage`, `PngRowFilter`, `PngReader`, and `PngWriter`.
- The reader refuses a critical chunk that it cannot read, and it skips an ancillary chunk such as `sRGB` or `iTXt`.
- Each failure names the file and the reason: the bit depth, the color type, the interlace method, the chunk name of a wrong CRC-32, and a short file (T-2).
- F-82 records a latent fault that this PR exposed. The Tools scan of det-lint asked `ReferenceSet.WithOutputOf` for the build output of Tools, which added no reference, because the process of the command already holds each of those assemblies. The scan now takes `ReferenceSet.Framework()`, as the Core scan does.

### The state of the build

- `main` is `a607280`, and the branch starts there.
- `make verify` passes with 706 tests and 0 build warnings. Format, `det-lint`, `ste-check`, replay identity, content hash, the Godot build, and the bounded smoke session pass.
- det-lint now reads `TheThingBelow.Tools/Png` with the rules of D-502, and it names Atlas, Audio, and NormalMaps as absent (G-16).
- `SimulationVersion.Current` stays 3. The PR changes no code of Core (G-17).

### What is in flight

The review of Codex. The Gitar pass approved the head `e45dcc9` with no code finding, and its CI block raised two claims. RG 7 had full merit: three rows of the Documents section gave no path, and the description now gives each one a path. RG 3 waits for the review record, which the review of Codex writes on this branch.

### Traps and gotchas

- The two committed files come from Apple ImageIO, which chose the row filters Sub and Paeth, and added the `sRGB`, `eXIf`, and `iTXt` chunks. A new file needs the method of the `PngReaderTests` remarks.
- The `PngImage` constructor copies the pixel bytes, and it reads the count of bytes in long math, because two sizes at the limit pass the range of an int.
- The writer holds no text and no date, so two runs give the same file. The compressed bytes still follow the version of `ZLibStream`, so no test compares PNG bytes (F-19).
- A PR that adds the first file of `TheThingBelow.Tools/Atlas`, `Audio`, or `NormalMaps` reads the same det-lint path that F-82 fixed.
- The RG 7 rule needs a `/` or a `.md` in the reason of each `No change needed because` line, which `DocumentRules.HoldsPath` reads. PR-44 hit the same rule.
- The next ids are D-666, OQ-197, F-83, L-16, G-29, M-9, and Session 121.

### The questions that block progress

None.

### The next concrete action

Hand PR #32 to Codex for the cross-provider review (T-4, D-17).

## Session 119: 2026-09-18, Codex

Author: Codex
Session: review PR #31, PR-44, the crash files and the log files. Repository: the-thing-below. Branch: `feat/pr-44-crash-and-log-files`. Role: reviewer. Base: `cb93ecd`.

### What this session did, and why

- Reviewed the complete PR-31 diff from merge base `cb93ecd` to effective head `f4a7a01`.
- Confirmed that Claude Code authored the PR and Codex reviewed it.
- Traced crash text, log text, file retention, personal-path hiding, callback recovery, replay records, and the Core boundary.
- Found no actionable finding. Wrote `docs/reviews/pr-31.md` with the verdict `Ready for owner merge`.
- Corrected the PR description row for `docs/reviews/` to the D-581 form after the Gitar RG-7 comment.

### The state of the build

- The metadata tip is `1398035`, and the effective implementation head is `f4a7a01`.
- `make verify` passes with 625 tests and 0 build warnings. Format, `det-lint`, `ste-check`, replay identity, content hash, Godot build, and the bounded smoke session pass.
- GitHub reports the implementation checks and Gitar pass green. The review-gate check failed before the review record existed and must pass after publication.

### What is in flight

The published review record and handoff need a fresh `review-gate` run after the metadata push.

### Traps and gotchas

- The effective head excludes only the review and handoff metadata commit after `f4a7a01`.
- PR-61 adds the on-screen crash message. PR-15 adds the Tools crash-file reader.
- The PR description had an invalid `docs/reviews/` Documents row. The review corrected it.

### The questions that block progress

None.

### The next concrete action

Run `gh pr checks 31` and verify the remote head and green review gate.
