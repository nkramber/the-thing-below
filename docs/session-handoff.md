# Session handoff

Rule (D-18): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md` (D-18). At the start, read the top entry alone (D-584).

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

- `main` is `3204545`, and the branch starts there. `make ste-check` gives 0 findings.
- The PR changes documents alone. No code, content, or test file changes.
- The PR adds decision rows, so the label of D-401 does not apply. The PR needs the Codex review (T-4).

### What is in flight

The push, the pass of gitar, and the Codex review.

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

## Session 118: 2026-09-18, Claude Code

Author: Claude Code
Session: PR-44, the crash files and the log files. Repository: the-thing-below. Branch: `feat/pr-44-crash-and-log-files`. Role: author. Base: `cb93ecd`.

### What this session did, and why

- The session asked the owner six questions before any change. It confirmed that OQ-57 blocks PR-61 alone, because D-559 moved the crash message and the address of D-473 to that PR.
- D-658 to D-662 answer the five new questions: the folders and the names, the count of files, the levels and what a step logs, the lines of a crash file, and the simulation version.
- A step of Core returns its log entries (D-179). A menu change takes the info level, and a beat of the patrol the debug level (D-660).
- Core holds `LogEntry`, `LogLine`, and `LogLineText`, and it adds no time and no path. Core holds `CrashReport` and `CrashText` too.
- Storage holds `LogStore`, `CrashStore`, the time text, the folder rules, and the rule that hides the folders of the person (D-170).
- One crash file holds the crash line and then the lines of the record, so the player sends one file and the report keeps its replay (D-661).
- Game catches the error of each callback, writes the crash file, writes one log line, and exits with the code 1 (D-559, T-2).
- The smoke session writes the log file, opens and closes the menu, and writes and reads one crash file. Each CI leg thus reads the whole path (D-117).
- F-81 records a wrong pointer: section 7.15 of the phase file sent this PR to section 7.10 of `area-release.md`, and the right section is 7.1.

### The state of the build

- `main` is `cb93ecd`, and the branch starts there.
- `make verify` passes with 625 tests and 0 build warnings. Format, `det-lint`, `ste-check`, replay identity, content hash, and the bounded smoke session pass.
- The smoke session of this machine wrote 3 log lines and one crash file with a record that ends at tick 120.
- `SimulationVersion.Current` stays 3, which D-662 sets. The identity file and the fixture save of format 1 need no change (G-17).

### What is in flight

The review of Codex. The Gitar pass approved the head `e45dcc9` with no code finding, and its CI block raised two claims. RG 7 had full merit: three rows of the Documents section gave no path, and the description now gives each one a path. RG 3 waits for the review record, which the review of Codex writes on this branch.

### Traps and gotchas

- `fault.GetType().Name` is a member of `System.Reflection` for `det-lint` rule DL 4. Thus Core takes the name of the type of an error as an argument, and Storage reads it (F-36, D-647).
- The JSON writer escapes `<` and `>`, so the placeholder of a hidden folder is `(user-folder)` and a person reads the path in the file.
- Storage reads the clock nowhere. Each caller passes a UTC time, and a time of another kind is an error (T-3).
- A crash file with a stack holds line feeds inside one JSON string, and the physical line stays one line.
- Tools takes no reference to Storage yet, because no command reads a crash file. PR-15 adds it with the headless runner (D-494).
- The next ids are D-663, OQ-195, F-82, L-16, G-29, M-9, and Session 119.

### The questions that block progress

None.

### The next concrete action

Run `make where`, commit the work, push one time, and answer the Gitar pass with the `gitar-review` skill.

## Session 117: 2026-09-18, Codex

Author: Codex
Session: review PR #30, PR-43, the Storage project, the snapshots, and the saves. Repository: the-thing-below. Branch: `feat/pr-43-storage-and-saves`. Role: reviewer. Base: `3a7340f`.

### What this session did, and why

- Reviewed the complete PR-30 diff from merge base `3a7340f` to effective head `d1d2a47`.
- Confirmed the provider gate. Claude Code authored the PR, and Codex reviewed it.
- Traced the save format, checksum, format dispatch, safe write, resume removal, platform folder rules, Core boundary, and Godot boot check.
- Found no actionable finding. Wrote `docs/reviews/pr-30.md` with the verdict `Ready for owner merge`.

### The state of the build

- The metadata tip is `e3ff30b`, and the effective implementation head is `d1d2a47`.
- `make verify` passes with 536 tests and 0 build warnings. Format, `det-lint`, `ste-check`, replay identity, content hash, Godot build, and the bounded smoke session pass.
- GitHub reports the implementation checks and Gitar pass green. The current `review-gate` run fails because the review record named the wrong effective head. The new record targets `d1d2a47`.

### What is in flight

The review record and this handoff entry need a metadata commit and push. A fresh `review-gate` run must then verify the published record.

### Traps and gotchas

- The effective head excludes only the review and handoff metadata commit after `d1d2a47`.
- PR-16 adds the Game save-load integration. PR-44 adds crash and log files.

### The questions that block progress

None.

### The next concrete action

Run `make where`, commit the review record and handoff entry, push, fetch, and verify the review gate and remote head.

## Session 116: 2026-09-18, Claude Code

Author: Claude Code
Session: PR-43, the Storage project, the snapshots, and the saves. Repository: the-thing-below. Branch: `feat/pr-43-storage-and-saves`. Role: author. Base: `3a7340f`.

### What this session did, and why

- The session asked the owner four questions before any change, and D-654 to D-657 answered them.
- `TheThingBelow.Storage` is the sixth project of the solution (D-494). Core declares no reference, and a new test proves it.
- Core holds the text of a save: two lines of JSON, the header with the checksum, and the snapshot (D-655).
- `RunSnapshotText` holds the one writer and the one reader of a snapshot line, and `RunRecordText` calls it (T-1).
- Storage holds the folder rule of the three systems, the three save files, and the one safe write of D-178.
- The stored save `TheThingBelow.Tests/saves/format-1.json` names simulation version 1, and this build loads it (D-259).
- Game sets the custom user folder of Godot, and Boot compares that folder with the rule of Storage (D-657, F-33).
- The first push failed the format check on the Windows leg alone, and F-80 records the reason.

### The state of the build

- `main` is `3a7340f`, and the branch starts there.
- `make verify` passes with 536 tests and 0 build warnings. Format, `det-lint`, `ste-check`, replay identity, content hash, and the smoke session pass.
- The head is `d1d2a47`, and it passes every CI leg. The `review-gate` check gives RG 3 alone, because the head holds no review record.
- The first head `d776f5e` failed the format step of Windows alone, which F-80 explains.
- The smoke session printed the save folder of this machine, so the check of D-657 ran against the real Godot folder.
- `SimulationVersion.Current` stays 3. The PR adds a text form and file code, and it changes no rule that makes a state (G-17).

### What is in flight

The review of Codex. The Gitar pass approves the head `d1d2a47` and gives no finding.

- The Gitar check on that head completed with success 2 seconds after the push.
- Gitar replaced the dashboard comment, and the new id is `5738261979` with the edit time 01:29:38Z. That time is later than the push at 01:28:48Z, so the pass covers the head (D-603).
- The dashboard reads `Approved` with no issue, and the PR holds no review thread.

### Traps and gotchas

- Tools takes no reference to Storage yet, because no command of Tools reads or writes a save. PR-44 adds it with the crash files (D-494).
- The three save names have no backticks in the documents. The reference rule reads a bare name with a file type as a path of the repository.
- `SaveText.Write` refuses a header of another format version, so a test of an old format must change the text itself.
- The fixture test fails a raise of the format version with no stored save. A PR that changes the snapshot commits a fixture (D-166, D-654).
- A comment between the arrow of an expression body and its expression fails `dotnet format` on Windows alone (F-80). The Mac gives no finding.
- The next ids are D-658, OQ-190, F-81, L-16, G-29, M-9, and Session 117.

### The questions that block progress

None.

### The next concrete action

Open the pull request with its Documents section, then answer the Gitar pass.

## Session 115: 2026-09-18, Codex

Author: Codex
Session: repeat review PR #29, PR-6, the tick, the intents, the run record, and replay. Repository: the-thing-below. Branch: `feat/pr-6-tick-and-run-record`. Role: reviewer. Base: `1960cf3`.

### What this session did, and why

- Re-reviewed P2-1 against its original trigger and the correction at effective head `db432de`.
- Confirmed that even increments fail during line 2 parsing, and that a too-few-stream snapshot also names line 2.
- Confirmed the three regression tests fail on `ee1e6ea` and pass on `db432de`.
- Updated `docs/reviews/pr-29.md` with the fixed finding and verdict `Ready for owner merge`.

### The state of the build

- `main` and the merge base are `1960cf3`. The effective head is `db432de`, and the metadata tip is `7f19d2d`.
- `make verify` passes with 477 tests and 0 build warnings. Format, `det-lint`, `ste-check`, replay identity, content hash, and smoke pass.
- Gitar approves the corrected head with no finding and no open review thread.

### What is in flight

The review record and this handoff entry need a metadata commit and push. The corrected implementation is ready for owner merge after publication.

### Traps and gotchas

- The effective head is `db432de`; later commits change only review and handoff metadata (D-610).
- `RunSnapshot.Check` now rejects even increments, and `RunRecordText.ReadSnapshot` checks inside line 2 parsing so all snapshot faults retain line context.

### The questions that block progress

None.

### The next concrete action

Commit and push the repeat review record and handoff, then verify the green review gate.
