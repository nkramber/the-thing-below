# Session handoff

Rule (D-18): this file keeps the 10 newest sessions, newest first. At the end of a session, add a new entry at the top. Move any entry beyond the tenth to the top of `docs/session-handoff-archive.md` (D-18). At the start, read the top entry alone (D-584).

## Session 117: 2026-09-18, Codex

Author: Codex
Session: review PR #30, PR-43, the Storage project, the snapshots, and the saves. Repository: the-thing-below. Branch: `feat/pr-43-storage-and-saves`. Role: reviewer. Base: `3a7340f`.

### What this session did, and why

- Reviewed the complete PR-30 diff from merge base `3a7340f` to effective head `d596c1c`.
- Confirmed the provider gate. Claude Code authored the PR, and Codex reviewed it.
- Traced the save format, checksum, format dispatch, safe write, resume removal, platform folder rules, Core boundary, and Godot boot check.
- Found no actionable finding. Wrote `docs/reviews/pr-30.md` with the verdict `Ready for owner merge`.

### The state of the build

- The metadata tip is `e3ff30b`, and the effective implementation head is `d596c1c`.
- `make verify` passes with 536 tests and 0 build warnings. Format, `det-lint`, `ste-check`, replay identity, content hash, Godot build, and the bounded smoke session pass.
- GitHub reports the implementation checks and Gitar pass green. The current `review-gate` run fails because the review record was absent. The new record targets `d596c1c`.

### What is in flight

The review record and this handoff entry need a metadata commit and push. A fresh `review-gate` run must then verify the published record.

### Traps and gotchas

- The effective head excludes only the review and handoff metadata commits after `d596c1c`.
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

## Session 111: 2026-09-18, Codex

Author: Codex
Session: repeat review of PR #28, PR-5, content, the content hash, and the string table. Repository: the-thing-below. Branch: `feat/pr-5-content-and-string-table`. Role: reviewer. Base: `efd6a53`.

### What this session did, and why

- Re-reviewed the correction to P2-1 at effective head `2b1f7f8`.
- Confirmed the fixture record owns the `fixture` id kind and the content reader rejects ids of another kind with file and field context (D-646).
- Re-ran the invalid-id trigger; it exits 1. Confirmed the adjacent valid `label` id case passes.
- `make verify` passed with 406 tests, 0 build warnings, and 0 errors.
- Updated `docs/reviews/pr-28.md`; the current verdict is `Ready for owner merge`, and the earlier verdict remains in the history.

### The state of the build

- The merge base is `efd6a53`. The effective head is `2b1f7f8`; the published metadata head is `0f820a5`.
- Thirteen GitHub checks pass. `review-gate` reports RG 4 and RG 5 because the published review record still has the earlier verdict. The published review clears those conditions.
- Gitar's current dashboard comment `5733434645` approves the corrected head with no open finding.

### What is in flight

The review record and this handoff entry are published at `0f820a5`. Checks were in progress at the first check snapshot; this follow-up records the publication verification. Refresh the checks and confirm the review gate passes.

### Traps and gotchas

- The effective implementation head remains `2b1f7f8`; the later commit `2e866f6` changes review and handoff metadata (D-610).
- The old-head regression failures are author-reported in `docs/reviews/pr-28-response.md`; this session independently confirmed the fixed trigger and adjacent valid case.
- The next ids are D-650, OQ-186, F-80, L-16, G-29, M-9, and Session 112.

### The questions that block progress

None.

### The next concrete action

Refresh GitHub checks after this metadata update, confirm the review gate passes, then hand the PR to the owner for merge.

## Session 110: 2026-09-18, Claude Code

Author: Claude Code
Session: PR-5, the answer to the review of Codex. Repository: the-thing-below. Branch: `feat/pr-5-content-and-string-table`. PR: #28. Role: author. Base: `efd6a53`.

### What this session did, and why

- The review gave one finding, P2-1, and the session read it as a claim.
- A run of `content-hash --write` over a copy of `content/` with the id `enemy.cave_rat` in a fixture file exited 0, so the finding reproduces and has full merit.
- D-646 names three tests for this PR, and the PR wrote the form test and the repeated-id test alone. The kind test was absent.
- Each rule record now owns the kind of its entry ids. `RuleFixture.IdKind` is `fixture`, and `ContentReader.ReadContentId(kind)` refuses another kind.
- The `label` field keeps the kind-free read, because a string id names where the player reads the text (G-7).
- Five new tests cover the rule. The three cases of `AnEntryIdOfAnotherKindFails` fail on the old head `55eb083` (T-3).

### The state of the build

- `main` is `efd6a53`. The PR is #28, and its tip and effective head are `2b1f7f8`.
- `make verify` passes: the build with 0 warnings, 406 tests, the format check, `det-lint`, `ste-check`, the identity check, the content hash, and the smoke session.
- The content hash stays `5ce12c64...f3c15f3`, because no rule file changed.
- `SimulationVersion.Current` stays at 2. The response file gives the reason under G-17.

### What is in flight

The repeat review of Codex. The Gitar pass approves the head `2b1f7f8`, and it gives no open finding.

- The push wait of three minutes ended with no automatic pass, because the trial keeps them paused. The comment `Gitar review` at 17:05:31Z started a manual pass.
- Gitar replied at 17:06:34Z, and it then replaced the dashboard comment. The new id is `5733434645`, with the edit time 17:06:42Z.
- The edit time is later than the reply time, so the pass covers the head (D-603). The Gitar check on `2b1f7f8` completed with success.
- The dashboard reads `Approved` with no issue, and the PR holds no review thread.
- Fourteen CI checks pass, the Gitar check included. `review-gate` gives RG 4 and RG 5, because the record still reads `Changes required` for `55eb083`. The repeat review clears both.

### Traps and gotchas

- The kind of an entry id comes from the record, not from the path. The folder name is plural, so a path gives no kind.
- A test holds the `label` field open to another kind, so a later change cannot make the two fields one case by accident.
- A run of the new tests against the old head needs the constant written out as a literal, because `RuleFixture.IdKind` does not exist there.
- `CLAUDE.md` and `AGENTS.md` sit at 16375 bytes, against a limit of 16384. A new line needs a trim first.
- The next ids are D-650, OQ-186, F-80, L-16, G-29, M-9, and Session 111.

### The questions that block progress

None.

### The next concrete action

Codex reviews PR #28 again and writes the verdict for `2b1f7f8`.

## Session 109: 2026-09-18, Codex

Author: Codex
Session: review PR #28, PR-5, content, the content hash, and the string table. Repository: the-thing-below. Branch: `feat/pr-5-content-and-string-table`. Role: reviewer. Base: `efd6a53`.

### What this session did, and why

- Reviewed all 52 changed paths from the merge base to implementation head `55eb083`.
- Found that Core accepts an id whose kind does not agree with the rule file (D-646).
- Verified the trigger in a disposable checkout: `enemy.cave_rat` in `rules/fixtures/tools.json` passed the content-hash command.
- Wrote `docs/reviews/pr-28.md` with verdict `Changes required` for `55eb083`.

### The state of the build

- `main` and the merge base are `efd6a53`. PR #28 has metadata tip `ad0bdd3` and effective head `55eb083` (D-610).
- `make verify` passes: 401 tests, format, `det-lint`, `ste-check`, replay identity, content hash, and Godot smoke.
- Thirteen remote checks pass on tip `ad0bdd3`. `review-gate` reports only RG 4 because the verdict is `Changes required`.
- Gitar approves the effective head, with no open finding. Its dashboard update is later than the implementation push.

### What is in flight

The review record and this handoff entry are published at `ad0bdd3`. The author must correct P2-1 before merge. Codex then repeats the review on the corrected head.

### Traps and gotchas

- Session 108 named `de496df` as the effective head. The only changes after implementation commit `55eb083` are handoff metadata, so D-610 leaves `55eb083` as the effective head.
- The disposable kind-mismatch probe changed no repository file.
- The next ids are D-650, OQ-186, F-80, L-16, G-29, M-9, and Session 110.

### The questions that block progress

None. OQ-63, OQ-179, OQ-184, and OQ-185 close with D-646 to D-649.

### The next concrete action

The author corrects P2-1, then asks Codex to repeat the review.

## Session 108: 2026-09-18, Claude Code

Author: Claude Code
Session: PR-5, content, the content hash, and the string table. Repository: the-thing-below. Branch: `feat/pr-5-content-and-string-table`. Role: author. Base: `efd6a53`.

### What this session did, and why

- Asked the owner OQ-63 and OQ-179 first, and wrote no code before the answers (D-19, D-24).
- D-646 sets the content id as a kind, a dot, and a name. D-647 sets a hand reader and the reflection switch.
- Two more questions came from the work. D-648 draws the hash line at `content/rules/`, and D-649 ships one fixture rule record.
- Wrote the strict reader on `Utf8JsonReader`, the SHA-256 of D-644, the content hash, the string table, and the embed.
- Added the `content-hash` command of Tools, the committed hash file, and a step of the `replay-identity` job.

### The state of the build

- `main` is `efd6a53`. The PR is #28, and its tip and effective head are `de496df`.
- Fourteen CI checks pass: the three build legs, the three smoke legs, the three `replay-identity` legs, `changed paths`, `ste-check`, `det-lint`, the coverage report, and the Gitar check.
- The three `replay-identity` legs also compare the content hash, so the three legs and the Mac give one value.
- `review-gate` gives one fault, RG 3: the head holds no review record at `docs/reviews/pr-28.md`. The review of Codex clears it.
- `make verify` passes: the build with 0 warnings, 401 tests, the format check, `det-lint`, `ste-check`, the identity check, the content hash, and the smoke session.
- The smoke session reads 4 content files from the Game assembly and gives the same hash as the folder.
- `SimulationVersion.Current` is 2, and the `state-hash` run of the identity file moved with it (G-17).

### What is in flight

The review of Codex. The Gitar pass approves the head `de496df`, and it gives no open finding.

- Automatic reviews stay paused on the Gitar trial, and an automatic pass still ran. The Gitar check started at 16:20:23Z and completed with success.
- The dashboard comment `5732921703` has the edit time 16:23:31Z, later than the push, so the pass covers the head (D-603).
- The dashboard reads `Approved`, with no open finding, and the PR holds no review thread.
- This PR changes code, so the `review-override` label does not apply (D-401).

### Traps and gotchas

- The reflection switch reaches every program of the solution, and it broke 27 review-gate tests (F-78). The fixture now writes its file with `Utf8JsonWriter`.
- A project reference from Tests to Game breaks the det-lint fixtures (F-79). The embedded-content test loads the built Game assembly instead.
- `CLAUDE.md` and `AGENTS.md` sit at 16375 bytes, against a limit of 16384. A new line needs a trim first.
- A change of a rule file needs `content-hash --root . --write` and a review of the new value.
- The next ids are D-650, OQ-186, F-80, L-16, G-29, M-9, and Session 109.

### The questions that block progress

None. D-646 to D-649 answer each question of section 7.12.

### The next concrete action

Codex reviews PR #28 and writes `docs/reviews/pr-28.md`.
