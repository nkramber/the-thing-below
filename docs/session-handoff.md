# Session handoff

Rule (D-18): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md` (D-18). At the start, read the top entry alone (D-584).

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

## Session 114: 2026-09-18, Claude Code

Author: Claude Code
Session: PR-6, the answer to the review of Codex. Repository: the-thing-below. Branch: `feat/pr-6-tick-and-run-record`. PR: #29. Role: author. Base: `1960cf3`.

### What this session did, and why

- The review gave one finding, P2-1, and the session read it as a claim.
- The session wrote the regression tests first and ran them against the reviewed head `ee1e6ea`. Three tests failed, so the finding reproduces and has full merit.
- The reader made a snapshot on line 2 and checked it later, inside the constructor of `RunRecord`. `Read` then gave `RunRecordException.ForRecord`, which names no line.
- Thus every fault of a snapshot lost its line, and the parity of an increment had no check. An even increment reached `Pcg32.FromSnapshot`, which threw a bare error.
- `RunSnapshot.Check` now refuses an even increment, and the message names the stream and the value (T-2, G-18).
- `RunRecordText.ReadSnapshot` now calls `Check` inside the read of line 2, so every fault of a snapshot names that line.
- `docs/reviews/pr-29-response.md` holds the disposition, the evidence, and the reason that the simulation version stands.

### The state of the build

- `main` is `1960cf3`. The PR is #29, and the reviewed head was `ee1e6ea`.
- `make verify` passes: the build with 0 warnings, 477 tests, the format check, `det-lint`, `ste-check`, the identity check, the content hash, and the smoke session.
- `SimulationVersion.Current` stays at 3. The correction adds a refusal alone, and it changes no rule that makes a state (G-17).
- The identity file matches for all 5 runs, the content hash stays `5ce12c64...f3c15f3`, and the smoke state hash stays `0x82def31590ae6c3b`.

### What is in flight

The repeat review of Codex. The Gitar pass approves the head `db432de`, and it gives no finding.

- The automatic pass started 3 seconds after the push, and the Gitar check on `db432de` completed with success.
- Gitar replaced the dashboard comment, and the new id is `5737525695` with the edit time 23:45:27Z. That time is later than the push at 23:44:16Z, so the pass covers the head (D-603).
- The dashboard reads `Approved` with no issue, and the PR holds no review thread.
- Fourteen CI checks pass. `review-gate` gives RG 4 and RG 5, because the record still reads `Changes required` for `ee1e6ea`. The repeat review clears both.

### Traps and gotchas

- A check of a value after its line read loses the line. Each line read of `RunRecordText` must hold every check of its own line.
- `Pcg32.FromSnapshot` refuses an even increment, and `RunSnapshot.Check` now holds the same rule. A change to one needs the other.
- The three regression tests fail on `ee1e6ea` for three different reasons. The response file names each reason.
- The next ids are D-654, OQ-186, F-80, L-16, G-29, M-9, and Session 115.

### The questions that block progress

None.

### The next concrete action

Hand PR #29 back to Codex for the repeat review of the head `db432de`.

## Session 113: 2026-09-18, Codex

Author: Codex
Session: review PR #29, PR-6, the tick, the intents, the run record, and replay. Repository: the-thing-below. Branch: `feat/pr-6-tick-and-run-record`. Role: reviewer. Base: `1960cf3`.

### What this session did, and why

- Reviewed the complete diff from the merge base to effective head `ee1e6ea`.
- Found P2-1: snapshot validation accepts an even random-stream increment, and replay then throws without record-line context (T-2, G-18, D-259).
- Ran a disposable probe. The record reader accepted an even increment, and replay threw a bare `ArgumentException`.
- Wrote `docs/reviews/pr-29.md` with verdict `Changes required` for `ee1e6ea`.

### The state of the build

- `main` and the merge base are `1960cf3`. The effective head is `ee1e6ea`, and the remote tip is `3a4b728`.
- `make verify` passes with 475 tests and 0 build warnings. Format, `det-lint`, `ste-check`, replay identity, content hash, and smoke pass.
- The review record is published at `49c4462`. All GitHub checks pass except `review-gate` RG 4, because the verdict is `Changes required`.
- The Gitar check passes on `49c4462`. Its dashboard approves the code with no finding, and the PR has no review thread (D-603).

### What is in flight

The review record and this handoff entry were published at `49c4462`. P2-1 remains open, so the PR is not ready for owner merge.

### Traps and gotchas

- The tip commit `3a4b728` changes only review and handoff metadata. The effective head remains `ee1e6ea` (D-610).
- `RunSnapshot.Check` verifies stream order, but not that a PCG32 increment is odd. `Pcg32.FromSnapshot` rejects the value later, without the record line.
- The record text and the snapshot line are valid JSON. The defect is the missing semantic check after JSON parsing (D-652).

### The questions that block progress

None.

### The next concrete action

The author validates odd stream increments with snapshot context and adds a regression test. Codex then reviews the correction.

## Session 112: 2026-09-18, Claude Code

Author: Claude Code
Session: PR-6, the tick, the intents, the run record, and replay. Repository: the-thing-below. Branch: `feat/pr-6-tick-and-run-record`. Role: author. Base: `1960cf3`.

### What this session did, and why

- Asked the owner the four blocking questions of PR-6 in one batch, and recorded each answer (D-19, D-24).
- D-650: the tick rises while a menu is open, and the world systems skip their work. The tick is the one time line, so no record needs a second order value.
- D-651: the record takes a new snapshot at each save, and it drops every intent before it (F-10).
- D-652: records and snapshots are JSON text, with one object on each line, as the logs are (D-179).
- D-653: one constant in Core holds the game version, and `GameVersion.Tag` gives the release tag.
- Core gained `TheThingBelow.Core/Runs/`: the intent, the state, the world rules, the simulation, the snapshot, the recorder, the replay, and the text of a record.
- The debug seam takes the handlers of the host, and Core names no debug assembly (D-260, D-492).
- `ContentReader` gained the reads of a 64-bit number, of true and false, and of a hexadecimal 64-bit value. One strict reader now reads the content files and the record.
- Game gained `FixedStepLoop` and `GameRun`, and `Boot` steps the run on each frame (D-164).
- The identity set gained the `replay` run, which plays a script, writes the text, reads it again, and replays it.

### The state of the build

- `main` is `1960cf3`. The branch head is `3a70273` before the documents of this entry.
- `make verify` passes: the build with 0 warnings, 475 tests, the format check, `det-lint`, `ste-check`, the identity check, the content hash, and the smoke session.
- `SimulationVersion.Current` is 3, because the tick and the world of one tick are the first rules that change a state (G-17).
- The identity file gained `replay 0x2350D21C3F5A9E84`, and `state-hash` moved to `0xCC5817D745E5344C`, because that run hashes the simulation version.
- The content hash stays `5ce12c64...f3c15f3`, because no rule file changed.

### What is in flight

The Codex review of PR #29. The Gitar pass approves the head `ee1e6ea`, and it gives no finding.

- The automatic pass started 8 seconds after the push, and the Gitar check on `ee1e6ea` completed with success at 22:31:11Z.
- The dashboard comment `5736968934` has the edit time 22:31:05Z, which is later than the push, so the pass covers the head (D-603).
- The dashboard reads `Approved` with no issue, and the PR holds no review thread.
- The CI analysis of Gitar found a real fault of the description: the `docs/reviews/` row held prose and no form of D-581, so RG 7 failed. The row now takes the `Changed:` form, and RG 7 passes.
- Fourteen CI checks pass. `review-gate` gives RG 3 alone, because the head holds no `docs/reviews/pr-29.md`. The review of Codex clears it.

### Traps and gotchas

- A spread of an `IReadOnlyList` in Core calls `System.Linq`, which G-1 refuses. `RunRecorder.Step` copies the list itself.
- The last line of a record holds the end tick. A run takes many ticks after its last intent, so a replay that stopped at the last intent line would give another state.
- `RunSnapshot` is a record with a list, so `==` compares that list by reference. A test compares the state hash or the text.
- The world of Phase 1 is one patrol on a fixed beat. PR-7 replaces it with the tile map.
- The next ids are D-654, OQ-186, F-80, L-16, G-29, M-9, and Session 113.

### The questions that block progress

None.

### The next concrete action

Hand PR #29 to Codex for the cross-provider review (T-4, D-17).
