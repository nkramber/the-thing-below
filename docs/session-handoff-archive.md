# Session handoff archive

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

## Session 107: 2026-09-18, Codex

Author: Codex
Session: review PR #27, PR-4, integer math, streams, state hash, and identity job. Repository: the-thing-below. Branch: `feat/pr-4-core-math-and-identity`. Role: reviewer. Base: `772468a`.

### What this session did, and why

- Reviewed the full 38-path diff from `772468a` to effective head `a963e4f` against section 7.11 and D-641 to D-645.
- Verified the malformed identity file correction from the trigger through the command and regression tests.
- Wrote `docs/reviews/pr-27.md` with verdict `Ready for owner merge` for `a963e4f`.

### The state of the build

- `main` and the merge base are `772468a`. The PR tip is `0cc4ddc`; its effective head is `a963e4f` (D-610).
- `make verify` passes: 290 tests, format, `det-lint`, `ste-check`, replay identity, and the Godot smoke session.
- Thirteen CI checks pass on tip `0cc4ddc`. `review-gate` reports only RG 3 because this review record is not yet on the branch.
- Gitar approves the current effective head. Its one finding is fixed, and its thread is resolved.

### What is in flight

The review record and this handoff entry are published. `review-gate` passes on `e1bfc72`.

### Traps and gotchas

- The code correction is `a963e4f`. The later commits change only handoff and review metadata, so they do not move the effective head.
- OQ-60, OQ-61, and OQ-62 close with D-641 to D-645.
- This PR changes code, so the `review-override` label does not apply (D-401).
- The next ids are D-646, OQ-184, F-78, L-16, G-29, M-9, and Session 108.

### The questions that block progress

None for PR #27. D-641 to D-645 answer the questions of section 7.11.

### The next concrete action

The owner can merge PR #27.

## Session 106: 2026-09-18, Claude Code

Author: Claude Code
Session: the second round of PR-4, the answer to the Gitar pass. Repository: the-thing-below. Branch: `feat/pr-4-core-math-and-identity`. PR: #27. Role: author. Base: `772468a`.

### What this session did, and why

- The Gitar pass approved the head `1bb9125` with one finding, and the session read it as a claim.
- A run of the command against a malformed identity file reproduced the fault, so the finding has full merit.
- `IdentityFile.Read` throws `InvalidDataException`, which does not derive from `IOException`. The catch filter named `IOException` alone.
- A malformed line thus escaped as an unhandled exception with a stack trace, in place of the one-line fault report that the comment promised (T-2).
- The catch filter now names `InvalidDataException` too.
- Four new tests run the malformed lines and the absent file through the command path. The three malformed cases fail on the old filter (T-3).

### The state of the build

- `main` is `772468a`. The head of PR #27 is `3f8b803`, and the round before it was `1bb9125`.
- `make verify` passes: the build with 0 warnings, 290 tests, the format check, `det-lint`, `ste-check`, the identity check, and the smoke session.
- On `3f8b803`, 14 CI checks pass: the three build legs, the three smoke legs, the three `replay-identity` legs, `changed paths`, `ste-check`, `det-lint`, the coverage report, and the Gitar check.
- The three `replay-identity` legs give the same four hashes as this machine, which is exit test 5 of section 7.11.
- `review-gate` gives one fault, RG 3: the head holds no review record at `docs/reviews/pr-27.md`. The review of Codex clears it.
- The first `review-gate` run also gave RG 7, because the `docs/reviews/` row took no form of D-581. A correction of the PR description cleared it.

### What is in flight

The review of Codex. The Gitar pass approves the head `3f8b803`, and it gives no open finding.

- Automatic reviews stay paused on the Gitar trial, so the push wait of three minutes ended with no pass. The comment `Gitar review` at 14:36:38Z started a manual pass.
- Gitar replied `On it` at 14:37:02Z, and it then replaced the dashboard comment. The new id is `5731578321`, with the edit time 14:37:25Z.
- The edit time is later than the reply time, so the pass covers the head (D-603).
- The Gitar check on `3f8b803` completed with success in 44 seconds.
- The dashboard reads `Approved`, with 1 closed finding and none open. The one review thread is resolved.

### Traps and gotchas

- `InvalidDataException` derives from `SystemException`, and `FileNotFoundException` derives from `IOException`. A catch filter of file faults must name the first one itself.
- The vectors of PCG32 and xxHash64 are the published values of each reference implementation. A second implementation from the specification gave the same values on 2026-09-18.
- D-645 puts the SHA-256 of D-644 in PR-5, beside the content hash that calls it.
- A change of a hash in the identity file also needs a higher `SimulationVersion.Current` (G-17, D-504).
- This PR changes code, so the label of D-401 does not apply.
- The next ids are D-646, OQ-184, F-78, L-16, G-29, M-9, and Session 107.

### The questions that block progress

None. D-641 to D-645 answered every question of this PR.

### The next concrete action

Hand PR #27 to Codex for the review of T-4. This PR changes code, so the label of D-401 does not apply.

## Session 105: 2026-09-18, Claude Code

Author: Claude Code
Session: the first round of PR-4. Repository: the-thing-below. Branch: `feat/pr-4-core-math-and-identity`. PR: the one PR of PR-4, which GitHub numbers at the push. Role: author. Base: `772468a`.

### What this session did, and why

- The session asked OQ-60, OQ-61, and OQ-62 before any code, and D-641 to D-645 hold the answers.
- Core gains its first rules: `BasisPoints`, `Pcg32`, `RandomStreams`, `XxHash64`, `StateHasher`, and `IdentitySet`.
- `RunContext`, `SimulationException`, and `CoreAssert` carry the context of every error (T-2, G-18).
- `SimulationVersion.Current` starts at 1, and each later Core change raises it (G-17).
- The `replay-identity` command of Tools compares each state hash with the committed identity file, and `--write` writes that file again.
- The `replay-identity` job runs on the three CI legs, and `make verify` gains the same check.
- Tools now references Core, because the command computes the hashes.

### The state of the build

- `main` is `772468a`, and this branch starts from it.
- `make verify` passes: the build with 0 warnings, 286 tests, the format check, `det-lint`, `ste-check`, the identity check, and the smoke session.
- The Release build gives the same four hashes, and it proves that the assertion helper still throws (exit test 8).
- The identity file is `TheThingBelow.Tests/identity/replay-identity.txt`, with four runs.
- Two commits hold the work: the code, then the documents.

### What is in flight

The push, the Gitar pass, and the review of Codex. This PR changes code, so the label of D-401 does not apply.

### Traps and gotchas

- The vectors of PCG32 and xxHash64 are the published values of each reference implementation. A second implementation from the specification gave the same values on 2026-09-18.
- D-645 puts the SHA-256 of D-644 in PR-5, beside the content hash that calls it. PR-4 ships xxHash64 alone.
- A change of a hash in the identity file also needs a higher `SimulationVersion.Current` (G-17, D-504).
- `make verify` runs `build` before `identity`, because the command reads the build output of Tools.
- The next ids are D-646, OQ-184, F-78, L-16, G-29, M-9, and Session 106.

### The questions that block progress

None. D-641 to D-645 answered every question of this PR.

### The next concrete action

Push the branch, open the PR, and answer the Gitar pass. Then hand the PR to Codex for the review of T-4.

## Session 104: 2026-09-18, Codex

Author: Codex
Session: review PR #26, PR-82, the Mobile renderer. Repository: the-thing-below. Branch: `feat/pr-82-mobile-renderer`. Role: reviewer. Base: `e54810a`.

### What this session did, and why

- Reviewed the renderer setting and its regression tests against D-616 and the exit tests of section 7.3.
- Wrote `docs/reviews/pr-26.md` with verdict `Ready for owner merge` for effective head `6b7bde6`.
- Confirmed the provider gate, the current Gitar approval, and that no comment thread needs an answer.

### The state of the build

- `main` and the merge base are `e54810a`. The PR tip is `9de398a`; its effective head is `6b7bde6` (D-610).
- Local build, 189 tests, format, `det-lint`, `ste-check`, Godot editor build, and smoke session pass.
- All required CI jobs pass on `9de398a`. `review-gate` fails because the review record is not yet on the branch.
- Gitar approves `6b7bde6`. Its dashboard comment was edited after the implementation push, and no review threads exist.

### What is in flight

The review record and handoff entry need a commit and push. Then verify `review-gate` on the metadata tip.

### Traps and gotchas

- The pull request branch name says PR-82, while GitHub numbers it PR #26. Use the GitHub number in review records (D-17).
- Commits `2419ce4` and `9de398a` change only handoff metadata, so the effective head remains `6b7bde6` (D-610).
- Gitar functional validation is not enabled.
- The next ids are D-641, OQ-184, F-78, L-16, G-29, M-9, and Session 105.

### The questions that block progress

None. D-616 selects Mobile, and section 7.3 names the renderer work.

### The next concrete action

Commit and push the review record and this handoff entry. Check that `review-gate` passes.

## Session 103: 2026-09-18, Claude Code

Author: Claude Code
Session: the first round of PR-82. Repository: the-thing-below. Branch: `feat/pr-82-mobile-renderer`. PR: the one PR of PR-82, which GitHub numbers at the push. Role: author. Base: `e54810a`.

### What this session did, and why

- D-616 picked the Mobile renderer from the Deck test, and PR-82 writes it into the Game project.
- `TheThingBelow.Game/project.godot` now sets `renderer/rendering_method="mobile"`.
- The feature tag list now names `Mobile` in place of `Forward Plus`, because the Godot editor reads that list.
- The header comment names the Deck test and its result, in place of the provisional text of D-599.
- `GameProjectRendererTests` reads the committed project file and locks both lines.
- The test guards the setting, because the Godot editor writes this file and can write the default of Godot into it again.

### The state of the build

- `main` is `e54810a`, and this branch starts from it.
- `make verify` passes: the build with 0 warnings, 189 tests, the format check, `det-lint`, `ste-check`, and the smoke session.
- The smoke session prints `smoke: the renderer is mobile`, which is exit test 2 of section 7.3 of the phase file.
- The two new tests fail on the setting of PR-1, which is the regression check of T-3.
- The Godot editor build ran on the new setting, and it wrote no change into the project file.
- PR #26 holds this work, and its head is `2419ce4`.
- The 12 CI checks pass on `2419ce4`: the three build legs, the three smoke legs, `changed paths`, `ste-check`, `det-lint`, the coverage report, and the Gitar check.
- `review-gate` gives one fault, RG 3: the head holds no review record at `docs/reviews/pr-26.md`. The review of Codex clears it.

### What is in flight

The review of Codex. The Gitar pass approves the head `2419ce4`, and it gives no finding.

- The push of `2419ce4` was at 07:47:30Z, and the dashboard comment `5726936964` has the edit time 07:49:12Z.
- The edit time is later than the push time, so the pass covers the head (D-603).
- The Gitar check passed in 1 minute and 9 seconds, and the summary names this diff.
- The thread list of the pull request is empty, so no comment waits for an answer.
- The dashboard carries the pause note of the Gitar trial beside the approval.

### Traps and gotchas

- The setting `renderer/rendering_method.mobile` stays as it is. It serves Android and iOS, which D-481 excludes.
- HDR 2D works under Mobile, so the glow of D-188 stays live (D-188, D-616).
- The screen tests of CI keep the Compatibility renderer, whatever this PR sets (D-172).
- This PR changes code, so the label of D-401 does not apply, and Codex reviews it.
- The first `review-gate` run gave RG 7 too, because the `docs/reviews/` line of the PR took no form of D-581. A correction of the PR description cleared it.
- The next ids are D-641, OQ-184, F-78, L-16, G-29, M-9, and Session 104.

### The questions that block progress

None. The Deck test answered D-160, and D-616 holds the pick.

### The next concrete action

Hand PR #26 to Codex for the review of T-4. This PR changes code, so the label of D-401 does not apply.

## Session 102: 2026-09-18, Codex

Author: Codex
Session: repeat review PR #25 at effective head `ac34b5f`. Repository: the-thing-below. Branch: `docs/pr-86-screen-scale-answers`. Role: reviewer. Base: `b3ec2b4`.

### What this session did, and why

- Rechecked P2-1 against its original trigger and the ordered-list sweep.
- Confirmed both Phase 1 lists now run from 1 to 23 with no duplicate item number.
- Confirmed the stale section 7.19 reference now names 7.20, and the round ends at D-640.
- Read both Gitar claims, both author replies, and Gitar's confirmation. Both threads are resolved.
- Updated `docs/reviews/pr-25.md` to close P2-1 for effective head `ac34b5f`.

### The state of the build

- `main` and the merge base are `b3ec2b4`. The PR tip is `118551e`; its effective head is `ac34b5f` (D-610).
- `make verify` passes at the local tip: build with 0 warnings, 187 tests, format, `det-lint`, `ste-check`, and smoke.
- GitHub reports `changed paths` and `ste-check` as passing. Docs-only build, test, format, coverage, lint, and smoke jobs skip (D-600).
- Gitar approves `ac34b5f`, with 2 closed findings and none open. Its current dashboard follows the `On it` reply.
- The review record and handoff were published as `301b507`. `review-gate`, `changed paths`, and `ste-check` pass on that head.
- Docs-only build, test, format, coverage, det-lint, and smoke jobs skip under D-600. The `review-gate` check is green.

### What is in flight

The verdict and handoff are published. The owner can merge PR #25.

### Traps and gotchas

- The new effective head changes two roadmap paths. The later Gitar and handoff commits change only metadata (D-610).
- The ordered-list sweep checks a class of defects that `ste-check` does not read.
- `deck-test/` and `screen-scale-probe/` remain untracked and outside the PR.
- The next ids are D-641, OQ-184, F-78, L-16, G-29, M-9, and Session 103.

### The questions that block progress

None. OQ-183 closed with D-633 and D-639.

### The next concrete action

The review applies to effective head `ac34b5f`; the owner can merge PR #25.

## Session 101: 2026-09-18, Claude Code

Author: Claude Code
Session: the answer to the review of PR #25, in the same session as Session 99 (D-582). Repository: the-thing-below. Branch: `docs/pr-86-screen-scale-answers`. Role: author. Base: `b3ec2b4`.

### What this session did, and why

- The review of `83c17f7` gives `Changes required` with one finding, P2-1.
- P2-1 has full merit. The PR-86 entry added a line to the Phase 1 sequence and left each later number as it was, so two steps held the number 11.
- A sweep of every ordered list found the same defect in the Phase 1 list of `docs/design.md`, which the review did not name. Two steps held the number 10 there.
- The same sweep found two stale references of the insert: the gate line named section 7.19, and section 7.8 named the round as D-626 to D-638.
- `docs/reviews/pr-25-response.md` holds each disposition, the regression check, and the evidence.
- This session published the review record and the Session 100 entry, because the review session had a read-only `.git` directory (D-602).

### The state of the build

- `main` and the merge base are `b3ec2b4`. The head before this round was `9c13790`.
- `make verify` passed: the build with 0 warnings, 187 tests, the format check, `det-lint`, `ste-check`, and the smoke session.
- The review records `make verify` as a failure at build after 5:00. That result did not reproduce here, and Session 98 recorded the same stop on a repeat review.
- The ordered-list sweep now prints no duplicate item number in any list.

### What is in flight

The repeat review of Codex at the effective head `ac34b5f`. This round changed two roadmap paths, so it moved the effective head (D-610).

The Gitar pass approves that head, with 2 closed findings and none open.

- No automatic review started on the new head, because the trial of Gitar paused them. The Gitar check was absent, and the dashboard kept the edit time of the head before it.
- The push was at 07:16:37Z, the request at 07:20:10Z, and the reply `On it` at 07:20:33Z.
- Gitar replaced the dashboard comment, and the new id `5726614038` has the edit time 07:20:54Z.
- Each time is later than the one before it, so the pass covers the effective head (D-603).
- The summary of the pass repeats the words of the pass before it, which a pass with no new finding can do. The three times prove that it is current, so this round asked for no second review.
- `review-gate` gives two faults, RG 4 and RG 5. The record holds the verdict `Changes required` for the head before this round. The repeat review clears both.

### Traps and gotchas

- A renumber of one ordered list can break a sibling list and a self-reference. The PR-86 insert broke three places, and the review named one. A sweep of every ordered list catches the class, and `ste-check` reads no item number.
- Gate 1 moved from section 7.19 to section 7.20 when the PR-86 entry took 7.9. A citation of a section number of a phase file needs a check after any insert.
- The review could not export the PR comments, so it did not read the two inline replies to the Gitar findings. Both threads are resolved.
- `deck-test/` and `screen-scale-probe/` stay untracked, and the two probe exports stay out of git.
- The next ids are D-641, OQ-184, F-78, L-16, G-29, M-9, and Session 102.

### The questions that block progress

None. OQ-183 closed with D-633 and D-639.

### The next concrete action

Push the round, answer the Gitar pass on the new head, and ask Codex for the repeat review.

## Session 100: 2026-09-18, Codex

Author: Codex
Session: review PR #25, roadmap PR-86, the screen scale answers. Repository: the-thing-below. Branch: `docs/pr-86-screen-scale-answers`. Role: reviewer. Base: `b3ec2b4`.

### What this session did, and why

- Confirmed that Claude Code authored the PR, so Codex meets the other-provider gate (T-4, D-17).
- Reviewed the 15-path documentation diff, the PR description, the screen scale decisions, and the PR-86 roadmap entry.
- Found P2-1: the Phase 1 sequence numbers the owner step and PR-46 as item 11.
- Wrote `docs/reviews/pr-25.md` with verdict `Changes required` for effective head `83c17f7`.

### The state of the build

- `main` and the merge base are `b3ec2b4`. The PR tip is `9c13790`; its effective head is `83c17f7` (D-610).
- GitHub reports `changed paths` and `ste-check` as passing. The build, tests, format, coverage, det-lint, and smoke checks skip for this docs-only PR (D-600).
- `review-gate` fails because the review record is not yet on the branch.
- `make verify` failed at build after 5:00 with 0 warnings, 0 errors, and no diagnostics. The author reports a successful run in Session 99.
- `ste-check` passes with 0 findings after the review record and handoff edits.
- The Gitar dashboard approves `83c17f7` with two closed findings and none open. Its inline replies were not available in this session.

### What is in flight

The author needs to correct P2-1. Git metadata is read-only in this environment, so the review record and this entry need publication from a writable session.

### Traps and gotchas

- The metadata tip `9c13790` changes only the handoff. The effective head remains `83c17f7` (D-610).
- `git fetch` could not write `.git/FETCH_HEAD`. The GitHub API also failed during the required complete comment export.
- `deck-test/` and `screen-scale-probe/` remain untracked and outside the PR.
- Session 90 moved to the archive to keep ten entries in this file (D-18).
- The next ids are D-641, OQ-184, F-78, L-16, G-29, M-9, and Session 101.

### The questions that block progress

None. The open item is P2-1, which requires a correction to the sequence.

### The next concrete action

Correct the Phase 1 sequence, publish the review record and this entry, then ask Codex to review the corrected effective head.

## Session 99: 2026-09-18, Claude Code

Author: Claude Code
Session: the answers of the screen scale probe. Repository: the-thing-below. Branch: `docs/pr-86-screen-scale-answers`. Role: author. Base: `b3ec2b4`.

### What this session did, and why

- Read `screen-scale-probe/handover.md` on the spike branch, which holds the owner answers of the probe.
- Found that answer 2 and answer 3 of OQ-183 disagree on a 1920 by 1080 screen, which D-568 makes a screen that must look good.
- Found that the probe computed the fit with integer division, so it could never draw the 1.5x fit of that screen.
- Patched the spike with two fit modes, and proved both in a window of 1920 by 1080 on the Mac (D-638).
- The owner then ran the new Windows build on a 27-inch 1080p screen and picked the UI at 2x.
- Wrote D-626 to D-640, F-68 to F-77, G-28, the M-8 table of four screens, and the close of OQ-183.
- Applied the answers to nine live documents, four skills, and the PR-86 roadmap entry.

### The state of the build

- `main` is `b3ec2b4`. PR #25 is open, and the effective head is `83c17f7`.
- The spike branch `spike/screen-scale-probe` is at `f314243`, and it never merges (D-597, D-621).
- `make verify` passed: the build with 0 warnings, 187 tests, the format check, `det-lint`, `ste-check`, and the smoke session.
- The `ste-check`, `changed paths`, and `Gitar` checks pass. The docs-only jobs skip under D-600.
- `review-gate` gives one fault, RG 3, because the head holds no record at `docs/reviews/pr-25.md`. The Codex review clears it.

### What is in flight

The Codex review. This PR changes decision rows, so the `review-override` label does not apply (D-401, D-609).

The Gitar pass approves the effective head `83c17f7`, with 2 closed findings and none open.

- The push of `83c17f7` was at 06:46:44Z, and the Gitar check on it started at 06:47:16Z.
- Gitar replaced the dashboard comment, and the new id `5726292453` has the edit time 06:47:54Z.
- Each time is later than the one before it, so the pass covers the effective head (D-603).
- Both review threads are resolved, and Gitar resolved each one itself.

### Traps and gotchas

- Two claims of the spike handover did not survive the check, and the record now holds the refutation. D-508 refutes the claim that the export of the game needs an include filter, because Game embeds content and each font in its own assembly. The 1080p run moved the UI boundary from a fit of 1x to a fit of 2x.
- A 27-inch 1080p screen and a 27-inch 4K screen give one line of body text the same apparent size, and the owner picked two different UI values. The count of device pixels sets the value, not the apparent size (F-77). A later session must not read the arcminute numbers of M-8 as the rule.
- The PR-86 entry moved 11 sections of `docs/roadmaps/phase-1-foundations.md` by one. No document outside this PR cites a section number of that file above 7.8.
- The option list of OQ-183 still names the frame at 40 by 22.5 tiles. That list records the options of 2026-09-17, and D-633 closed the question.
- The row of D-599 keeps the bare name `project.godot`, because a decision row is a record of one day (D-637).
- `deck-test/` and `screen-scale-probe/` stay untracked on this branch. The two probe exports are above 150 MB and stay out of git.
- The next ids are D-641, OQ-184, F-78, L-16, G-29, M-9, and Session 100.

### The questions that block progress

None. OQ-183 closed with D-633 and D-639, which unblocks PR-7, PR-34, and PR-61.

### The next concrete action

Push the branch, open the PR, answer the Gitar pass, and hand the PR to Codex for the review.

## Session 98: 2026-09-18, Codex

Author: Codex
Session: repeat review of PR #24 at effective head `5f5129a`. Repository: the-thing-below. Branch: `docs/pr-85-deck-test-and-style`. Role: reviewer. Base: `2874b70`.

### What this session did, and why

- Rechecked P2-1 against its original trigger and regression check.
- Verified the author revised D-139, D-161, D-172, D-214, D-520, and D-526.
- Verified the effects roadmap no longer schedules PR-37 or describes the CRT pass.
- Set P2-1 and P2-2 to fixed in the review record. The verdict is ready for owner merge.

### The state of the build

- `main` and the merge base are `2874b70`. Effective head: `5f5129a`. The review record was published at `6826312`.
- The author reports `make verify` passed with 187 tests. The local repeat-review build produced no output for 7:41 and was stopped.
- The changed-path and `ste-check` jobs pass. Docs-only build, test, format, coverage, lint, and smoke jobs skip under D-600.
- The current Gitar pass approves `5f5129a`, with one closed finding and no open finding. The live inline-comment query returned zero.
- `deck-test/` remains untracked and untouched.

### What is in flight

The review record and this handoff are published. CI and `review-gate` pass on `6826312`.

### Traps and gotchas

- The metadata commit does not move the effective head (D-610).
- D-618 retires the CRT pass and PR-37. D-619 keeps two transition names only.
- A later metadata commit must retain the current verdict and head for `review-gate` (D-610).
- The next ids are D-626, OQ-184, F-68, L-16, G-28, M-9, and Session 99.

### The questions that block progress

None for PR #24. OQ-183 blocks PR-7 and PR-34.

### The next concrete action

The owner can merge PR #24 after reading the current review verdict.

## Session 96: 2026-09-18, Codex

Author: Codex
Session: review of PR #24 at effective head `7426dc9`. Repository: the-thing-below. Branch: `docs/pr-85-deck-test-and-style`. Role: reviewer. Base: `2874b70`.

### What this session did, and why

- Reviewed the full 17-path documentation diff and the current Gitar pass.
- Found that D-618 retired the CRT, but the effects roadmap still named PR-37 and described the old-monitor pass.
- Found remaining CRT references in D-172, D-214, and `area-effects.md`.
- Found a separate failure in the PR description: its Documents lines did not use the required forms of D-581. The reviewer corrected the lines and recorded each edit.
- The review record gives the verdict and evidence for this head.

### The state of the build

- `main` and the merge base are `2874b70`. Effective head: `7426dc9`. Remote metadata tip at verification: `a20b9c4`.
- `ste-check` passes with 0 findings. `make verify` stopped at a build with no diagnostic output, so its result is inconclusive.
- CI skips build, test, format, coverage, lint, and smoke for this documentation-only PR under D-600.
- The current Gitar pass approves `7426dc9`, with one closed finding and no open finding. No inline thread is open.
- `deck-test/` remains untracked and untouched.

### What is in flight

The review record and this handoff are published. The author needs to correct the remaining CRT contracts before the owner can merge.

### Traps and gotchas

- The review commit changes only metadata paths, so the effective head remains `7426dc9` (D-610).
- The Gitar summary repeats older decision ids, but the push, request, reply, and dashboard times prove the pass covers this head (D-603).
- D-618 supersedes the CRT parts of D-88, D-105, D-120, and D-240; D-619 retains two transition names only.
- The next ids are D-626, OQ-184, F-68, L-16, G-28, M-9, and Session 97.

### The questions that block progress

None for PR #24. OQ-183 blocks PR-7 and PR-34.

### The next concrete action

The author corrects P2-1, then requests a repeat review of the new effective head.

## Session 93: 2026-09-17, Claude Code

Author: Claude Code
Session: PR-85, the result of the Deck test and the look. Repository: the-thing-below. Branch: `docs/pr-85-deck-test-and-style`. PR: #24, which is PR-85 of the roadmap. Role: author. Base: `2874b70`.

### What this session did, and why

- The owner ran the Deck test of D-160 on an OLED Deck and gave the reports to this session.
- Recorded the result and eight more owner answers as D-616 to D-624.
- D-616: the game uses the Mobile renderer, which won each of the 20 stages. PR-82 writes it.
- D-617: the first effect budget, 15 lights, 8192 particles, and 3 full-screen passes. F-66 marks each row as a floor.
- D-618: the CRT leaves the plan. PR-37 is retired, and 13 documents lost the pass, the toggle, and its captures.
- D-620 and D-621: two tests before PR-34, the Sprite Fusion art test and the screen scale probe.
- D-622 and G-27: every effect draws with the palette and hard edges, which answers the question of the owner about the fog.
- D-623: the Mac is the venue of each visual test, and the Deck takes the answers that need the Deck.
- F-67 and OQ-183: a 32-pixel sprite covers 4.0 mm on the Deck, and the scale of the frame is now an open question.

### The state of the build

- `main` is `2874b70`, and PR #23 merged before this session.
- The `ste-check` command passes on every live document, with 0 findings.
- The branch holds one commit, and it changes documents alone.

### What is in flight

The push, the Gitar pass, and the review of Codex. This PR revises decision rows, so the label of D-401 does not apply.

### Traps and gotchas

- Each citation of D-88, D-105, D-120, or D-240 must name D-618, or REF 3 fails.
- PR-37 keeps a retired entry in `phase-2-first-playable.md`, which keeps the id in the register (G-10).
- Section 7 of `phase-1-foundations.md` gained three entries, so each later section number moved.
- The flag `--attach` of `gh` refuses a text file, so the reports go in the description and in a comment (D-624).
- `deck-test/` stays untracked, as it was before this session.
- The next ids are D-625, OQ-184, F-68, L-16, G-28, M-9, and Session 94.

### The questions that block progress

OQ-183 blocks PR-7 and PR-34. The probe of D-621 answers it, and that probe needs the owner and three screens.

### The next concrete action

Answer the Gitar pass on PR #24, then hand the PR to Codex for the review of T-4.

## Session 92: 2026-09-17, Codex

Author: Codex
Session: re-review PR #23, roadmap PR-46, the `det-lint` command. Repository: the-thing-below. Branch: `feat/pr-46-det-lint`. Role: reviewer. Base: `0fbecab`.

### What this session did, and why

- Re-reviewed P2-1 at effective head `70df5ef` against the trigger from the earlier review.
- Verified the fix in `SceneTextRule`, the regression test, the response file, and the live command probe.
- The regression probe now reports one DL 9 finding and exits 1. `make verify` passes with 187 tests.
- Verified the current Gitar pass after its request, reply, and dashboard update. No finding or open thread exists.
- Updated `docs/reviews/pr-23.md` to close P2-1 and assess effective head `70df5ef`.

### The state of the build

- `main` and the merge base are `0fbecab`. The current PR head is `f62113e`; its effective head is `70df5ef` (D-610).
- `make verify` passes: build with 0 warnings, 187 tests, format, `det-lint`, `ste-check`, and smoke.
- GitHub checks pass except `review-gate`, which fails RG 4 and RG 5 while the published review record still holds the old verdict and head.
- The updated review record and this entry were published as `2e4cc38`. All 11 checks pass on that head, including `review-gate`.
- This follow-up metadata commit records the publication and check verification. The effective head remains `70df5ef` (D-610).

### What is in flight

The repeat review is published and `review-gate` passes. The owner can merge PR #23.

### Traps and gotchas

- The correction changes the effective head from `f5eba68` to `70df5ef`. Later handoff commits do not change it.
- The current Gitar summary repeats the old count of 186 tests. The local suite and the author response verify 187.
- `deck-test/` remains untracked and untouched.

### The questions that block progress

None for PR #23. The finding is fixed, and the review gate waits for the published verdict.

### The next concrete action

The owner can merge PR #23. The review applies to effective head `70df5ef`.

## Session 91: 2026-09-17, Claude Code

Author: Claude Code
Session: the answer to the review of PR #23, in the same session as Session 89 (D-582). Repository: the-thing-below. Branch: `feat/pr-46-det-lint`. PR: #23. Role: author. Base: `0fbecab`.

### What this session did, and why

- The review of `f5eba68` gives the verdict `Changes required` with one finding, P2-1.
- The finding: DL 9 read the value of a scene property as `[^"]*`, which stops at the first quote. Godot writes a quote inside a string value as `\"`, so a line such as `text = "Say \"hello\""` matched no part of the pattern, and the rule read no property.
- The claim reproduces. The probe gave `0 finding(s)` and an exit code of 0 on the old code.
- The finding has full merit. A player string with a quote in a scene file is a supported case of D-499 and G-7.
- The correction: the value part of the pattern is now `(?:[^"\\]|\\.)*`, which reads an escaped character as one unit. One line of `TheThingBelow.Tools/DetLint/SceneTextRule.cs` changes.
- One regression test, `ATextValueWithAnEscapedQuoteFails`. It fails on the old code with 186 tests and passes on the new code with 187.
- The probe of the reviewer now gives one DL 9 finding, and the command exits 1.
- `docs/reviews/pr-23-response.md` holds the disposition and the evidence.

### The state of the build

- `main` is `0fbecab`. The head before this round is `44f99a8`, and the PR is #23.
- `make verify` passes: the build with 0 warnings, 187 tests, the format check, `det-lint` with 0 findings, `ste-check` with 0 findings, and the smoke session.
- On `70df5ef`, every CI check passes except `review-gate`: three build legs, three smoke legs, the changed paths job, the coverage report, `ste-check`, `det-lint`, and the Gitar check. RG 4 fails, because the record still gives the verdict `Changes required`.

### What is in flight

The repeat review of Codex at the effective head `70df5ef`. The Gitar pass of that head gives the verdict `Approved` with no finding and no open thread.

### Traps and gotchas

- The correction moves the effective head, so the review of `f5eba68` no longer covers the head. The repeat review reads the new head (D-603).
- RG 4 stays red until the record of the repeat review gives `Ready for owner merge`.
- A scene file has no comment syntax, so the pattern of DL 9 reads a whole line and needs no comment rule.
- `deck-test/` stays untracked, as it was before this session.
- Gitar deleted the dashboard comment of the first pass and posted a new one with the id 5721871935. The summary of the new pass repeats the words of the first one, and its three times prove that the pass is current.
- The push wait and each poll of Gitar run in the background, and never in the foreground.
- The next ids are D-616, OQ-183, F-66, L-16, G-27, PR-85, M-8, and Session 92.

### The questions that block progress

None for this PR. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### The next concrete action

The repeat review of Codex at the effective head `70df5ef`. The record at `docs/reviews/pr-23.md` sets P2-1 to closed and gives a verdict for that head, and RG 4 passes with it.

## Session 90: 2026-09-17, Codex

Author: Codex
Session: review PR #23, roadmap PR-46, the `det-lint` command. Repository: the-thing-below. Branch: `feat/pr-46-det-lint`. Role: reviewer. Base: `0fbecab`.

### What this session did, and why

- Reviewed the complete change from effective head `f5eba68` against section 7.7 of the phase roadmap and the affected contracts.
- Confirmed that Claude Code authored the change and Codex meets the opposite-provider gate (T-4, D-17).
- Found P2-1: DL 9 misses a scene text value when the value contains an escaped quote. The probe exited 0 with no finding, against D-499 and G-7.
- Corrected the stale PR description snapshot. It now names head `764390c` and the checks that GitHub reports.
- Wrote `docs/reviews/pr-23.md` with verdict `Changes required` for `f5eba68`.

### The state of the build

- `main` is `0fbecab`. The PR head is `764390c`, and its effective head is `f5eba68` (D-610).
- `make verify` passes locally: build, 186 tests, format, `det-lint`, `ste-check`, and smoke.
- GitHub checks pass on `764390c` except `review-gate`, which fails RG 3 because the review record was absent before this commit.
- The review record, handoff entry, and archived Session 80 were published as `b63cfb0`. GitHub confirms that head. All checks pass except `review-gate`, which fails RG 4 because the verdict is `Changes required`.
- This follow-up metadata commit records the publication verification. The effective head stays `f5eba68` (D-610).

### What is in flight

The author needs to correct P2-1 and add a regression test. The reviewer then repeats the review of PR #23.

### Traps and gotchas

- The scene pattern in `SceneTextRule` stops at a quote even when a backslash escapes it. A scene value with an escaped quote bypasses DL 9.
- Gitar approved the effective head but reported no rule coverage and no functional validation. The review checked those claims against the diff and local gates.
- `deck-test/` remains untracked and untouched.

### The questions that block progress

No owner question blocks progress. P2-1 needs the author's correction.

### The next concrete action

The author corrects P2-1 on PR #23. Codex reviews the correction against its new effective head.

## Session 89: 2026-09-17, Claude Code

Author: Claude Code
Session: PR-46, the `det-lint` command and its job. Repository: the-thing-below. Branch: `feat/pr-46-det-lint`. PR: #23. Role: author. Base: `0fbecab`.

### What this session did, and why

- Asked the owner OQ-70 and OQ-71, which blocked PR-46. The owner took the recommended option of each one, and D-614 and D-615 record the answers.
- Wrote the `det-lint` command in Tools on the Roslyn compiler library, version 5.9.0 of `Microsoft.CodeAnalysis.CSharp` (D-498). The command reads the type of each expression, so a real literal with no suffix fails too (F-38).
- Ten rules: DL 0 for a compilation error, DL 1 to DL 7 for Core, and DL 8 and DL 9 for Game. Section 7.4 of `docs/roadmaps/area-tools.md` holds the table.
- Added the `det-lint` job to the Linux leg of CI and the `lint` target of `make verify`.
- The first live run found a real fault: `CoreAssembly.Self` gave a `System.Reflection.Assembly` from Core. No code read it, because the reference test of G-1 reads the built file (F-61). The member now gives the name of the assembly, and that test reads the name.
- F-65 joins the finding register: Godot.NET.Sdk writes the build output of a Godot project to `.godot/mono/temp/bin/<configuration>/`, and that folder holds `GodotSharp.dll` from the NuGet restore.

### The state of the build

- `main` is `0fbecab`. The head of PR #23 is `f5eba68` before this entry.
- `make verify` passes: the build with 0 warnings, 186 tests, the format check, `det-lint` with 0 findings, `ste-check` with 0 findings, and the smoke session.
- Every CI check passes on `88215f8`: three build legs, three smoke legs, the changed paths job, the coverage report, `ste-check`, and the new `det-lint` job. The `det-lint` job takes 26 seconds.
- `review-gate` fails on RG 3 alone, because no record exists at `docs/reviews/pr-23.md` yet. RG 1, RG 2, RG 6, RG 7, and RG 8 pass.

### What is in flight

The Codex review of the effective head `f5eba68`. The Gitar pass gives the verdict `Approved` with no finding and no open thread. This PR changes code and `.github/workflows/`, so no label exempts it (D-560).

### Traps and gotchas

- The lint needs a build first. `ReferenceSet` reads the Godot assembly from the Game build output, and the error of an absent folder names the build command (T-2).
- The `bin` folder of the Game project stays empty. F-65 names the real output folder.
- DL 4 passes `typeof(X)` alone, because the content reader of PR-5 needs it in an attribute (F-36). It fails each member of `Type` that reflects.
- `GodotTextRule.TextHelperType` holds the name of the text helper of PR-61. That PR confirms the name or changes the constant (G-16).
- `deck-test/` stays untracked, as it was before this session.
- The commit `88215f8` changes the two handoff files alone, so the effective head stays `f5eba68` (D-610).
- Gitar paused automatic reviews for the trial, and the pass of this PR ran on the first push.
- The next ids are D-616, OQ-183, F-66, L-16, G-27, PR-85, M-8, and Session 90.

### The questions that block progress

None for this PR. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### The next concrete action

The Codex review of PR #23 at the effective head `f5eba68`. The review record goes to `docs/reviews/pr-23.md`, and RG 3 passes with it.

## Session 88: 2026-09-17, Codex

Author: Codex
Session: review PR #22 at effective head `1f07b6b`. Repository: the-thing-below. Branch: `feat/pr-84-context-budget`. Role: reviewer. Base: `9787b2d`.

### What this session did, and why

- Reviewed PR #22, the context budget checks in `ste-check`, against the five exit tests of section 7.6 of `docs/roadmaps/phase-1-foundations.md`.
- Verified the Gitar heading-boundary finding and its regression test. The shared detector fixes the trigger.
- Updated the PR test count to 136 after local verification.
- Wrote `docs/reviews/pr-22.md` with the verdict for effective head `1f07b6b`.

### The state of the build

- `main` is `9787b2d`. The PR branch is `feat/pr-84-context-budget`, with effective head `1f07b6b` and metadata tip `b55f8e6` before this review commit.
- Local build, 136 tests, format check, and `ste-check` pass. CI build, test, format, smoke, changed paths, coverage, and `ste-check` pass on `1f07b6b`.
- The initial `review-gate` run failed RG 3 because the review record was not on the head. After the metadata push, all ten checks pass, including `review-gate`.

### What is in flight

The review record and this entry are pushed as `d7f6fb3`. GitHub reports this commit as the head, the tree has no staged or tracked changes, and all ten checks pass, including `review-gate`.

### Traps and gotchas

- `deck-test/` was untracked before review work and remains untouched.
- A commit that changes only this PR's review record and handoff files does not move the effective head (D-610).
- The PR description now records the verified test count of 136.

### The questions that block progress

None for this PR. Future checks remain named with their creating PR in the PR gate (G-16).

### The next concrete action

The review is published at `d7f6fb3`, and all checks pass. The owner can merge PR #22.
## Session 87: 2026-09-17, Claude Code

Author: Claude Code
Session: the answer to the Gitar pass of PR #22, in the same session as Session 86 (D-582).
Repository: the-thing-below. Branch: `feat/pr-84-context-budget`. PR: #22. Role: author. Base: `9787b2d`.

### What this session did, and why

- The Gitar pass on `bdf9e06` gives the verdict `Approved with suggestions` and one finding.
- The finding: `SizeRules` read a session heading as the prefix `## Session `, and `SessionNumberRules` reads it as the form `## Session N:`. The two rules of one command disagreed on where an entry starts.
- The claim reproduces. A line of the top entry such as `## Session numbering rules` ended the measured region, so SIZE 2 undercounted the entry and could miss a real fault.
- The finding has full merit. `SessionNumberRules.IsSessionHeading` is now the one detector, and `SizeRules` calls it. The fault message of an absent entry names the form of a heading.
- One regression test, `ASubHeadingInsideTheTopEntryDoesNotEndIt`. It fails on the old code with 135 tests and passes on the new code with 136.
- The Gitar pass approves the new head `1f07b6b` with the verdict `Approved` and `1 closed / 1 findings`. Its one thread is resolved, and no thread is open.
- The first live run of the `review-gate` check found a real fault (F-37, D-500). The `docs/reviews/` line of the Documents section was not one of the three forms of D-581, and RG 7 failed. The line is now the `Changed:` form, and RG 7 passes.

### The state of the build

- `main` is `9787b2d`. The head before this round was `bdf9e06`, and the PR is #22.
- The nine CI checks pass on `1f07b6b`: three build legs, three smoke legs, the changed paths job, the coverage report, and `ste-check`.
- `review-gate` fails on RG 3 alone, because no record exists at `docs/reviews/pr-22.md` yet. RG 1, RG 2, RG 6, RG 7, and RG 8 pass.
- `make verify` passes: the build, 136 tests, the format check, the STE check with 0 findings, and the smoke session.

### What is in flight

The Codex review of the effective head `1f07b6b`.

### Traps and gotchas

- The Gitar trial paused automatic reviews. The first round got an automatic review at 20:12:31Z, and this round got none. The round needed a `Gitar review` comment at 20:23:50Z, and the reply came at 20:24:11Z.
- Gitar deleted the dashboard comment and posted a new one with the id 5720742291. The older id gives HTTP 404. Read the newest id in each check.
- RG 3 fails until the review record lands. That is the gate of T-4, not a fault of this PR.
- `CLAUDE.md` holds 152 free bytes under the 16 KB limit (D-613).
- The next ids are D-614, OQ-183, F-65, L-16, G-27, PR-85, M-8, and Session 88.

### The questions that block progress

None for this PR. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### The next concrete action

Codex reviews PR #22 at the effective head `1f07b6b`, and writes `docs/reviews/pr-22.md`.

## Session 85: 2026-09-17, Codex

Author: Codex
Session: repeat review of PR #21 at effective head `89d125e`. Repository: the-thing-below. Branch: `feat/pr-3-review-gate`. Role: reviewer. Base: `04953e4`.

### What this session did, and why

- Read the author response and verified the P1-4 correction against its trigger.
- Verified that D-612 requires one bold span in the Verdict section, and that the new rule enforces it.
- Set P1-1, P1-2, P1-3, P1-4, and P2-1 to fixed in `89d125e`.
- Ran the 14 focused review-gate tests and `make verify`. All checks passed, including 123 tests and Godot smoke.
- Verified all nine CI checks pass on metadata tip `1d3398b`. The Gitar pass approves effective head `89d125e`; its report says no rules were evaluated and functional validation was not enabled.
- Updated the review record with `Ready for owner merge` for effective head `89d125e`.

### The state of the build

- `main` and the merge base are `04953e4`. The effective head is `89d125e`, and the remote tip is metadata commit `1d3398b`.
- `make verify` passes: build, 123 tests, format, STE with 0 findings, and Godot smoke.
- All nine CI checks pass on `1d3398b`. The unrelated untracked `deck-test/` remains untouched.

### What is in flight

The review record and this entry need a commit and push to the PR branch.

### Traps and gotchas

- D-612 closes the scope of RG 4. A review that requires a check of Verdict prose needs a new owner answer.
- PR-3 has no live `review-gate` check. D-500 accepts the command tests as evidence, and the first live run is on the next PR.
- The next ids are D-613, OQ-183, F-65, L-16, G-27, PR-85, M-8, and Session 86.

### The questions that block progress

None for PR #21. OQ-3 applies after the first live check run. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### The next concrete action

Commit this review and handoff, push, then verify the published head.

## Session 84: 2026-09-17, Claude Code

Author: Claude Code
Session: the answer to the third review round of PR #21, in the same session as Session 78, Session 80, and Session 82 (D-582).
Repository: the-thing-below. Branch: `feat/pr-3-review-gate`. PR: #21. Role: author. Base: `04953e4`.

### What this session did, and why

- The review of `b466a64` marks P1-1, P1-2, P1-3, and P2-1 as fixed. It adds P1-4, and it keeps the verdict `Changes required`.
- P1-4: the rule counted the bold spans that match a verdict name, so `**Not Ready for owner merge.**` beside the approved name passed. The claim reproduces, and it has full merit.
- The `## Verdict` section now holds one bold span, and that span is the verdict name. A second bold span gives a fault, whatever its text.
- This was the third round of findings on RG 4, so the session asked the owner to settle the scope of the rule (D-19). The answer is D-612, and it is the rule above. A check of the prose of the section stays out of scope.
- A count of the bold spans of the `## Verdict` section of each of the 16 records of `docs/reviews/` gives one span, so the new rule needs no change to any record.
- Added one test for the new trigger. It fails on each older version of the rule.

### The state of the build

- `main` is `04953e4`. The head before this round was `b466a64`, and the remote tip was `1126bc2`.
- `make verify` passes: the build, 123 tests, the format check, the STE check with 0 findings, and the smoke session.
- This round changes code, a test, a decision row, and one skill reference file, so the effective head moves to `89d125e`.
- The nine CI checks pass on `89d125e`: three build legs, three smoke legs, the changed paths job, the coverage report, and `ste-check`.
- The Gitar pass approves `89d125e` with the verdict `Approved` and no finding. The pass needed a request again. Gitar replied `On it` at `19:33:14Z`, and its new dashboard comment `5720109237` has the edit time `19:35:01Z`. No thread is open.
- Session 74 moves to the archive. The handoff keeps the 10 newest entries (D-18, D-607).

### What is in flight

The repeat review of the other provider at the effective head `89d125e`.

### Traps and gotchas

- D-612 closes the scope of RG 4. A finding that asks the rule to read the prose of the section needs a new owner answer first.
- The `## Verdict` section of a record takes no bold word in its prose. The reference file says so.
- The head of this PR gets no automatic Gitar pass, because the trial paused them. Each round needs a `Gitar review` comment after the push wait of three minutes.
- The next ids are D-613, OQ-183, F-65, L-16, G-27, PR-85, M-8, and Session 85.

### The questions that block progress

None for PR #21. OQ-3 comes right after the merge of this PR. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### The next concrete action

Get a Gitar pass of the new head, then get the repeat review of the other provider.

## Session 83: 2026-09-17, Codex

Author: Codex
Session: repeat review of PR #21 at effective head `b466a64`. Repository: the-thing-below. Branch: `feat/pr-3-review-gate`. Role: reviewer. Base: `04953e4`.

### What this session did, and why

- Re-read the response file and verified the correction of P1-3 against its trigger.
- Set P1-3 to fixed in `b466a64`. The two regression tests for a second recognized verdict fault, and the earlier-verdict guard passes.
- Found P1-4: RG 4 passes an approved verdict followed by a bold negation. A probe against the current code returns `Pass`.
- Verified author-provider evidence in Sessions 78, 80, and 82. Codex is the eligible reviewer (T-4, D-17).
- The review record keeps one current verdict and the earlier verdict history.

### The state of the build

- `main` and the merge base are `04953e4`. The prior effective head was `3a75767`, and the new effective head is `b466a64`.
- The PR tip before this review was `c313e61`, a metadata commit. All nine CI checks pass on that tip.
- `make verify` passes at `b466a64`: build, 122 tests, format, 0 STE findings, and Godot smoke.
- Gitar approved `b466a64` at `19:08:59Z` with no finding. Its report says no rules were evaluated and functional validation was not enabled. No inline review comment exists.
- The unrelated untracked `deck-test/` remains untouched.

### What is in flight

P1-4 remains open. The PR needs a correction and another Codex review.

### Traps and gotchas

- RG 4 counts recognized verdict names and ignores other bold spans. A negated verdict can follow an approved verdict without a fault.
- PR-3 has no live `review-gate` check because GitHub starts the workflow from `main` alone (F-37, D-500).

### The questions that block progress

None for PR #21. OQ-3 remains for the owner after the first live check run.

### The next concrete action

Correct P1-4, then have Codex review the new effective head.

## Session 82: 2026-09-17, Claude Code

Author: Claude Code
Session: the answer to the repeat review of PR #21, in the same session as Session 78 and Session 80 (D-582).
Repository: the-thing-below. Branch: `feat/pr-3-review-gate`. PR: #21. Role: author. Base: `04953e4`.

### What this session did, and why

- The repeat review of `3a75767` marks P1-1, P1-2, and P2-1 as fixed. It adds one finding, P1-3, and it keeps the verdict `Changes required`.
- P1-3: RG 4 read the first bold verdict line and stopped. A section with the approved verdict and then `**Changes required.**` passed the gate. The claim reproduces, and it has full merit.
- The rule now reads each bold name of the `## Verdict` section. More than one verdict name gives a fault, on one line or on two lines.
- The correction text of the finding says that the line holds the approved verdict alone. A line with no prose after the name breaks the skeleton of the `pr-review` reference file, which each record of PR #19, PR #20, and PR #21 follows. The round applied the words of the regression check of the finding instead, and `docs/reviews/pr-21-response.md` gives the evidence.
- Added three tests. Two of them fail on the old rule. The third proves that an earlier verdict in its own section still passes, which is the shape of a repeat review record.
- The `pr-review` reference file names the new rule, and it says that an earlier verdict goes in a section of its own (D-579).

### The state of the build

- `main` is `04953e4`. The head before this round was `3a75767`, and the remote tip was `a8f91fe`.
- `make verify` passes: the build, 122 tests, the format check, the STE check with 0 findings, and the smoke session.
- This round changes code, tests, and one skill reference file, so the effective head moves to `b466a64`.
- The nine CI checks pass on `b466a64`: three build legs, three smoke legs, the changed paths job, the coverage report, and `ste-check`.
- The Gitar pass approves `b466a64` with the verdict `Approved` and no finding. The pass needed a request again. Gitar replied `On it` at `19:07:27Z`, and its new dashboard comment `5719795670` has the edit time `19:08:51Z`. No thread is open.
- Session 72 moves to the archive. The handoff keeps the 10 newest entries (D-18, D-607).

### What is in flight

The repeat review of the other provider at the effective head `b466a64`.

### Traps and gotchas

- The `## Verdict` section of a record now gives one verdict name in bold. A repeat review puts each earlier verdict in `## Earlier verdicts`, as the record of this PR does.
- The guard test `AnEarlierVerdictInAnotherSectionPasses` passes on both versions of the rule. It is a guard of the record shape, and not a regression test of P1-3.
- The head of this PR gets no automatic Gitar pass, because the trial paused them. Each round needs a `Gitar review` comment after the push wait.
- The next ids are D-612, OQ-183, F-65, L-16, G-27, PR-85, M-8, and Session 83.

### The questions that block progress

None for PR #21. OQ-3 comes right after the merge of this PR. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### The next concrete action

Get a Gitar pass of the new head, then get the repeat review of the other provider.

## Session 81: 2026-09-17, Codex

Author: Codex
Session: repeat review PR #21 at effective head `3a75767`. Repository: the-thing-below. Branch: `feat/pr-3-review-gate`. Role: reviewer. Base: `04953e4`.

### What this session did, and why

- Re-reviewed the three findings from Session 79 against the correction commit `3a75767`.
- Set P1-1, P1-2, and P2-1 to fixed in `3a75767` after their regression cases passed.
- Found P1-3: RG 4 accepts the first approved verdict and ignores a conflicting later verdict line.
- Corrected four stale facts in the PR description: the test count, the solution count, the verification count, and the session handoff row.
- Verified the author provider from Sessions 78 and 80. Codex remains the eligible reviewer under T-4 and D-17.

### The state of the build

- `main` and the merge base are `04953e4`. The effective code head is `3a75767`, and the remote tip before this review publication is `0a811b1`.
- `make verify` passes with 119 tests, clean format, 0 STE findings, and a successful smoke session.
- All nine CI checks pass on metadata tip `0a811b1`.
- The repeat review record and this handoff were published at `c9b17ab`.
- The Gitar pass approves effective head `3a75767` with no finding, and no inline review comment exists.
- The unrelated untracked `deck-test/` stays untouched.

### What is in flight

P1-3 remains open. The PR needs another correction and a third review round.

### Traps and gotchas

- RG 4 reads the first bold verdict line. It does not check for a second conflicting verdict line or for conflicting text after the approved line.
- PR #21 cannot run its live `review-gate` check because GitHub starts the workflow from `main` alone (F-37, D-500).
- The current Gitar approval covers effective head `3a75767`, even though the remote tip is a metadata commit.

### The questions that block progress

None for PR #21. OQ-3 remains for the owner after the first live check run.

### The next concrete action

Correct P1-3, then have Codex review the new effective head.

## Session 80: 2026-09-17, Claude Code

Author: Claude Code
Session: the answer to the review of PR #21, in the same session as Session 78 (D-582).
Repository: the-thing-below. Branch: `feat/pr-3-review-gate`. PR: #21. Role: author. Base: `04953e4`.

### What this session did, and why

- The review of `docs/reviews/pr-21.md` gives `Changes required` for head `40ea275`, with three findings. Each one reproduces, and each one has full merit.
- P1-1: RG 4 read the whole Verdict section, so `**Not Ready for owner merge.**` passed. The rule now reads the verdict line, which starts with the name in bold.
- P1-2: RG 5 read every line of the record, so a head field of another section passed a record with no head field in its Identity list. The rule now reads the `## Identity` list alone.
- P2-1: RG 8 held `a separate pr` and not `a separate pull request`. The rule now reads `pull request` as `pr`, which covers every phrase of the set at one time.
- Added seven regression tests. Each one fails on the old code, and the round proved that with `git stash` (T-3).
- The `pr-review` reference file and the `one-pr-one-session` skill changed with the command, because the two hold the form that the command reads (D-579).
- `docs/reviews/pr-21-response.md` holds the disposition and the evidence of each finding.
- Put the title back at the top of `docs/session-handoff-archive.md`. The commit `1bdaa89` moved an entry above it, and the title left the file.

### The state of the build

- `main` is `04953e4`. The head before this round was `40ea275`, and the remote tip was `755cd67`, which holds the review record.
- `make verify` passes: the build, 119 tests, the format check, the STE check with 0 findings, and the smoke session.
- This round changes code, tests, and two skill files, so the effective head moves to `3a75767`.
- The nine CI checks pass on `3a75767`: three build legs, three smoke legs, the changed paths job, the coverage report, and `ste-check`.
- The Gitar pass approves `3a75767` with the verdict `Approved` and no finding. The pass needed a request, because the head got no automatic pass in the wait of five minutes. Gitar replied `On it` at `18:32:04Z`, and it posted a new dashboard comment `5719382142` with the edit time `18:34:50Z`. No thread is open.
- Session 70 moves to the archive. The handoff keeps the 10 newest entries (D-18, D-607).

### What is in flight

The repeat review of the other provider at the effective head `3a75767`.

### Traps and gotchas

- A review record must now hold the verdict name in bold on its own line, and the head field in the `## Identity` list. An older record in another form fails RG 4 or RG 5.
- The proof that a regression test fails on the old code needs the old source of the tool alone. The tests build against both, because they use the public members of the rules.
- The first test of P1-2 used a stale head in the Identity list, and the old rule failed that record for another reason. The test now uses an Identity list with no head field, which is the true trigger.
- The next ids are D-612, OQ-183, F-65, L-16, G-27, PR-85, M-8, and Session 81.

### The questions that block progress

None for PR #21. OQ-3 comes right after the merge of this PR. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### The next concrete action

Answer the Gitar pass of the new head, then get the repeat review of the other provider.

## Session 79: 2026-09-17, Codex

Author: Codex
Session: review PR #21 at effective head `40ea275`. Repository: the-thing-below. Branch: `feat/pr-3-review-gate`. Role: reviewer. Base: `04953e4`.

### What this session did, and why

- Reviewed PR #21, the review gate, from its merge base to effective head `40ea275`.
- Added three findings to `docs/reviews/pr-21.md`: a negated approval can pass RG 4, an out-of-section head can pass RG 5, and RG 8 misses spelled-out deferrals.
- Corrected the `docs/reviews/` line in the PR description after the review record landed.
- Verified the author provider from Session 78 and the opposite-provider rule of T-4 and D-17.
- Checked all 28 changed paths, the workflow trust boundary, the command rules, the test fixtures, and the Documents section.

### The state of the build

- `main` is `04953e4`, and the implementation head is `40ea275`.
- `make verify` passes locally with 113 tests, clean format, 0 STE findings, and a successful smoke session.
- All nine CI checks pass on PR tip `d5d9332`. The live review-gate check is absent on PR-3 by design (F-37, D-500).
- All nine CI checks also pass on metadata tip `75b5f63`, after the review record and corrected Documents row were pushed.
- The Gitar pass is current on `40ea275`, and it reports approval with no finding.
- The remote PR tip before this review publication was `d5d9332`. The untracked `deck-test/` stays untouched.
- The review record and this handoff are published at `1bdaa89`. The record gives the findings for the author to correct.

### What is in flight

The PR waits for the author to correct the findings and for a repeat review.

### Traps and gotchas

- A review record and handoff commit do not move the effective head (D-610).
- `make verify` cannot run the live review-gate workflow on this PR. The first live run is on the next PR (D-500).
- The absent checks are det-lint (PR-46), replay identity (PR-4), screen test (PR-41), bot (PR-15), and night gate (PR-49).

### The questions that block progress

None for the findings. OQ-3 remains for the owner after the first live check run.

### The next concrete action

The author corrects the findings, then Codex reviews the new effective head.

## Session 78: 2026-09-17, Claude Code

Author: Claude Code
Session: PR-3, the review gate. Repository: the-thing-below. Branch: `feat/pr-3-review-gate`. PR: #21. Role: author. Base: `04953e4`.

### What this session did, and why

- Asked the owner OQ-69, OQ-181, and OQ-182 before any change (D-19). The answers are D-609, D-610, and D-611.
- Wrote the `review-gate` command in Tools, with eight rules from RG 1 to RG 8 (D-15, D-579).
- RG 1 and RG 2 read the override label. RG 3 to RG 5 read the review record. RG 6 to RG 8 read the documents.
- The command takes one JSON file with the facts of the PR, and one folder with the files of the head.
- Added `.github/workflows/review-gate.yml` on `pull_request_target`. The job runs from `main`, and it never builds the head.
- Added 60 tests. They hold each exit test of the roadmap entry, and the report of each fixture is in the PR description (D-500).
- D-610 made the metadata set the four paths of the PR. The `pr-review` reference, the `gitar-review` skill, and the `one-pr-one-session` skill hold the new set.
- D-611 made PR-84, the context budget check in the `ste-check` command, right after PR-3. The roadmaps and the sequence hold it.

### The state of the build

- `main` is `04953e4`. This branch holds the commit of this entry.
- `make verify` passes: the build, 113 tests, the format check, the STE check with 0 findings, and the smoke session.
- The nine CI checks pass on the head `40ea275`: three build legs, three smoke legs, the changed paths job, the coverage report, and `ste-check`.
- The Gitar pass approves `40ea275` with the verdict `Approved` and no finding. The dashboard comment `5718999630` has the edit time `18:03:21Z`, which is after the push. No thread is open.
- The `review-gate` check is absent from the head, because GitHub starts its trigger from `main` alone (F-37, D-500).
- The unrelated untracked `deck-test/` stays untouched.

### What is in flight

The review of the other provider at the effective head `40ea275`.

### Traps and gotchas

- GitHub starts `pull_request_target` from the default branch alone, so the live check cannot run on this PR (F-37). The first live run is the next PR (D-500).
- The command reads the description of the PR. An edit of the description changes the result, so the workflow also runs on the `edited` type.
- The deferral rule reads the Documents lines alone, and it reads a set of phrases. `DocumentRules.DeferralPhrases` holds each one.
- The next ids are D-612, OQ-183, F-65, L-16, G-27, PR-85, M-8, and Session 79.

### The questions that block progress

None for PR #21. OQ-3 comes right after the merge of this PR. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### The next concrete action

Open PR #21, answer the Gitar pass, then hand the PR to the other provider.

## Session 77: 2026-09-17, Codex

Author: Codex
Session: review PR #20 at effective head `e800f4c`. Repository: the-thing-below. Branch: `feat/pr-2-ste-checker`. Role: reviewer. Base: `9b84158`.

### What this session did, and why

- Verified Claude Code authored PR #20, so Codex meets the cross-provider gate (T-4, D-17).
- Read the full diff, the PR description and comments, the PR-2 roadmap entry, and the affected decisions and questions (D-589).
- Ran `make verify`: build, 53 tests, format, STE check, and Godot smoke all passed.
- Verified all nine CI checks pass on PR tip `2e316e0`. The effective head remains `e800f4c` because later commits change metadata paths alone.
- Gitar's finding on three missed contractions is fixed and resolved in `e800f4c`.
- Wrote `docs/reviews/pr-20.md` with `Ready for owner merge` for `e800f4c`.

### State of the build

- `main` is `9b84158`. The effective head is `e800f4c`, and the remote tip before this review commit is `2e316e0`.
- Local `make verify` passes. All nine CI checks pass on the remote tip.
- The unrelated untracked `deck-test/` remains untouched.

### In flight

The review record and this entry are on the PR branch, and GitHub reports the current head.

### Traps and gotchas

- The review covers effective head `e800f4c`; later metadata commits do not change it.
- The next ids are D-609, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 78.

### Open questions that block progress

None for PR #20. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

The review record and handoff are on the remote. The owner can merge PR #20.

## Session 76: 2026-09-17, Claude Code

Author: Claude Code
Session: the answer to the Gitar pass of PR #20, in the same session as Session 75 (D-582).
Repository: the-thing-below. Branch: `feat/pr-2-ste-checker`. PR: #20. Role: author. Base: `9b84158`.

### What this session did, and why

- The Gitar pass on head `a155ebc` gives `Approved with suggestions` with one finding.
- The finding says that the contraction rule misses `he's`, `she's`, and `who's`. The claim holds. A run of the pattern on each form gives no match, and the table of the skill promises a pronoun with `'s`.
- Fixed the pattern. The possessive of each of these pronouns has no apostrophe, so each form is always a contraction: its, his, hers, and whose.
- Added seven tests: four forms that fail the rule, and three possessives that pass it. Three of the four fail on the old pattern.
- Hoisted two patterns that a method built on each call. The result does not change, and the tool no longer compiles a pattern in a loop (T-1).

### State of the build

- `main` is `9b84158`. The head before this round was `a155ebc`, and the ten checks passed on it.
- `make verify` passes on this round: the build, 53 tests, the format check, the STE check with 0 findings, and the smoke session.
- This round changes code and a test, so the effective head moved to `e800f4c`.
- Session 66 moves to the archive. The handoff keeps the 10 newest entries (D-18, D-607).
- The nine CI checks pass on `e800f4c`: three build legs, three smoke legs, the changed paths job, the coverage report, and `ste-check`.
- The Gitar pass approves `e800f4c` with the verdict `Approved`, and it names the fix. The dashboard comment `5718363510` has the edit time `17:11:42Z`, which is after the push time `17:10:29Z`. The one thread is resolved, and no thread is open.

### In flight

The Codex review of PR #20 at the effective head `e800f4c`.

### Traps and gotchas

- Automatic Gitar reviews are paused on this trial, and the pass on `a155ebc` still ran. Read the Gitar check on the head before a `Gitar review` comment.
- The reply to the thread names the commit that fixes the finding.
- The next ids are D-609, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 77.

### Open questions that block progress

None for PR #20. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Get the Gitar pass on the new head, then hand PR #20 to Codex for the review.

## Session 75: 2026-09-17, Claude Code

Author: Claude Code
Session: PR-2, the STE checker in C#. Repository: the-thing-below. Branch: `feat/pr-2-ste-checker`. Role: author. Base: `9b84158`.

### What this session did, and why

- Asked the owner OQ-67 and OQ-68 before any change (D-19). The answers are D-604 and D-605.
- Asked three more questions that the work raised, and the answers are D-606, D-607, and D-608.
- Wrote the `ste-check` command of Tools as new code (D-101, D-277). It holds the eight writing rules of the `ste-writing` skill, the comment rule of F-11, the reference check, and the session number check.
- The writing rules give the same result as the interim Python script on every live document. The run gives 0 findings.
- Retired `docs/tools/ste-check.py`. The Makefile target, the `ste-check` CI job, `CLAUDE.md`, `AGENTS.md`, the skill, and the two runbooks now name the command.
- Corrected six citations that the new reference check found: two dead paths in decision rows, one bare `SKILL.md`, one external path under `.github/`, and two range markers that read as a citation of D-1.
- Moved Session 65 and Session 64 to the archive. The handoff held 11 entries, and D-18 keeps 10.

### State of the build

- `main` is `9b84158`. The branch is `feat/pr-2-ste-checker`.
- `make verify` passes: the build, 46 tests, the format check, the STE check with 0 findings, and the Godot smoke session.
- The new command runs the whole checkout in about one second.
- The unrelated untracked `deck-test/` remains untouched.

### In flight

The first push of PR-2, then the Gitar pass, then the Codex review.

### Traps and gotchas

- The reference check reads a path in backticks. Write a branch name, an external path, and a refused file name without backticks, or name the PR that creates the file (G-16).
- A line that names a `PR-#` marks every path on that line. The mark is broad by design.
- The checker reads the working tree, and not the staged files (D-608). A fault in an unstaged file stops the commit.
- The rule MD 1 fails an HTML comment across lines. The removal of such a comment hides prose from every rule (F-11).
- The next ids are D-609, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 76.

### Open questions that block progress

None for PR-2. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Push the branch, open PR-2, and get the Gitar pass on the head.


## Session 74: 2026-09-17, Codex

Author: Codex
Session: repeat review of PR #19 at effective head `7732b1b`, on branch `docs/pr-83-skip-set-decision`. Role: reviewer. Base: `ee4305a`.

### What this session did, and why

- Read the response file and the prior review record (D-588, D-589).
- Verified that Session 64 names Claude Code as the author. Codex is eligible under T-4 and D-17.
- Verified the earlier P2-1 correction against its original trigger. It stays fixed in `6f82d26`.
- Reviewed D-603 and the Gitar skill change. A Gitar pass on the effective head remains current across metadata commits.
- Ran `make verify`. The build, 8 tests, format, STE, and Godot smoke checks passed.
- Verified all 9 current CI checks pass on PR tip `784ebbf`.
- Verified the Gitar pass approves effective head `7732b1b`, its request and dashboard times satisfy the freshness rule, and there are zero review threads.
- Set the verdict to `Ready for owner merge` for effective head `7732b1b`.
- Moved Session 63 to the archive (D-18).

### State of the build

- `main` is `ee4305a`. The effective head is `7732b1b`, and the metadata tip is `784ebbf`.
- `make verify` passes. The nine live CI checks pass on the tip. The Gitar pass covers the effective head under D-603.
- The unrelated untracked `deck-test/` remains untouched.

### In flight

The review record and this entry are on the PR branch, and GitHub reports the current head. The verdict applies to effective head `7732b1b`.

### Traps and gotchas

- Metadata-only commits do not stale the Gitar pass under D-603. A commit outside the metadata set moves the effective head.
- The PR tip may move after the review record is pushed. Recheck the effective head and the published record.
- The next ids are D-604, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 75.

### Open questions that block progress

None for PR #19. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Wait for the owner merge of PR #19.

## Session 73: 2026-09-17, Claude Code

Author: Claude Code
Session: the answer to the block of the Gitar freshness rule of PR #19, in the same session as Sessions 64, 66, 68, 69, and 71 (D-582).
Repository: the-thing-below. Branch: `docs/pr-83-skip-set-decision`. PR: #19. Role: author. Base: `ee4305a`.

### What this session did, and why

- The review round of Session 72 sets P2-1 to fixed, and it gives `Blocked` for one reason: the Gitar pass names `6f82d26`, and the tip was `b862cfb`.
- The block is correct under the text of the `gitar-review` skill, and that rule cannot pass. The repository requires a record of each Gitar pass in the handoff, which is a metadata commit, so each pass was stale at the moment of its record.
- The review gives the evidence itself. It pushed `a11f6d7` and `168e602` after it wrote the block, and `git diff --stat 6f82d26..168e602` gives three metadata paths alone. The tip has no Gitar check run.
- The owner answered the question. D-603 sets the rule: a Gitar pass covers the effective head, and a metadata commit does not make it stale.
- The `gitar-review` skill follows D-603. It gets the terms of the effective head and the metadata set, a new first condition, and the command that proves the effective head.

### State of the build

- `main` is `ee4305a`. The effective head before this round was `6f82d26`, and the ten checks passed on it.
- `make verify` passes on the Mac of the owner for this round.
- This round changes `docs/decisions.md` and a skill, so the effective head moved to `7732b1b`.
- The ten checks pass on `7732b1b`, the Gitar check included. The Gitar pass approves that head with no finding, and the PR has zero review threads.
- The freshness check passes: the reply "On it" came at `14:44:17Z`, and the dashboard comment `5716314911` has the edit time `14:45:06Z`. Under D-603, this record does not make the pass stale.

### In flight

The repeat review of PR #19 at the effective head `7732b1b`, which gives the verdict.

### Traps and gotchas

- A rule that reads the branch tip fights a rule that reads the effective head. The record of a pass then makes the pass stale, and each side of the review moves the tip.
- D-603 does not weaken the pass. A commit outside the metadata set still needs a new pass.
- Automatic Gitar reviews are paused on this trial. Each new head needs a `Gitar review` comment after the push wait of three minutes.
- The next ids are D-604, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 74.

### Open questions that block progress

None for PR #19. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Get the repeat review of PR #19 from Codex at the effective head `7732b1b`, under D-602 and D-603.

## Session 72: 2026-09-17, Codex

Author: Codex
Session: repeat review of PR #19 at effective head `6f82d26`, on branch `docs/pr-83-skip-set-decision`. Role: reviewer. Base: `ee4305a`.

### What this session did, and why

- Read the response file and the prior review record (D-588, D-589).
- Verified that Session 64 names Claude Code as the author. Codex is the eligible reviewer (T-4, D-17).
- Re-read the original P2-1 trigger. The correction moves its exception into the merged-PR stop condition and names the bound PR.
- Set P2-1 to fixed in `6f82d26`. The instructions now allow the prompt for the bound PR and stop on another PR's merge.
- Ran `make verify`. The build, 8 tests, format, STE, and smoke checks passed.
- All 9 live CI checks pass on PR tip `b862cfb`.
- The Gitar dashboard approves `6f82d26`, but metadata commit `b862cfb` followed that review. The current PR tip has no Gitar run, so the freshness gate blocks approval.
- Updated `docs/reviews/pr-19.md` for effective head `6f82d26` and corrected the stale handoff fact in the PR description.

### State of the build

- `main` is `ee4305a`. The effective head is `6f82d26`, and PR tip `b862cfb` was the branch head at the start of this review.
- `make verify` passes. The 9 current CI checks pass on `b862cfb`.
- Gitar approves `6f82d26`; the approval is stale for the current PR tip.
- This round changes only the review and handoff metadata. The unrelated untracked `deck-test/` remains untouched.

### In flight

P2-1 is fixed. The review waits for a Gitar pass that matches the current PR tip.

### Traps and gotchas

- A Gitar pass after the code push becomes stale when a later metadata commit moves the PR tip.
- The dashboard update came before metadata commit `b862cfb`, and its check run names `6f82d26`.
- The next ids are D-603, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 73.

### Open questions that block progress

No owner question blocks PR #19. The stale Gitar pass blocks approval.

### Next concrete action

Get a fresh Gitar pass on the branch tip, then repeat the review.

## Session 71: 2026-09-17, Claude Code

Author: Claude Code
Session: the answer to the finding P2-1 of PR #19, in the same session as Sessions 64, 66, 68, and 69 (D-582).
Repository: the-thing-below. Branch: `docs/pr-83-skip-set-decision`. PR: #19. Role: author. Base: `ee4305a`.

### What this session did, and why

- The review round of Session 70 gives the verdict `Changes required` for the effective head `38aa19f`, with one finding. That session had network access, and it pushed its own record.
- P2-1 has full merit. Step 1 of the `one-pr-one-session` skill stops a session when the conversation holds a merged PR. The round of D-601 put the exception in a later paragraph, and the word "this rule" did not name the condition. A session that reads the list stops before it writes the prompt of D-601.
- The correction puts the exception in the condition itself, and it names the bound PR. The paragraph now says to write the prompt for the bound PR and its merge message alone.
- `docs/reviews/pr-19-response.md` holds the answer, the correction, and the regression check. It also notes two lines of the record that the verification of the same record refutes.
- The correction changes a skill, so the effective head moves again. The PR needs a new Gitar pass and a repeat review.

### State of the build

- `main` is `ee4305a`. The effective head before this round was `38aa19f`, and the ten checks passed on it.
- `make verify` passes on the Mac of the owner for this round: the build, 8 tests, the format check, the STE check, and the smoke session.
- The ten checks pass on `6f82d26`, the Gitar check included. The Gitar pass approves that head with no finding, and the PR has zero review threads.
- The freshness check passes: the reply "Running the review now" came at `13:02:11Z`, and the new dashboard comment `5714796386` has the edit time `13:02:51Z`.

### In flight

The repeat review of PR #19 at the effective head `6f82d26`, which gives the verdict.

### Traps and gotchas

- An exception that sits under a list, and not in the condition, does not change the condition. Put the exception in the line that stops the work.
- Automatic Gitar reviews are paused on this trial. A new head needs a `Gitar review` comment after the push wait of three minutes.
- The review session of this round reached GitHub, and the earlier two did not. The environment of that harness is not stable, and D-602 covers the case with no network.
- The next ids are D-603, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 72.

### Open questions that block progress

None for PR #19. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Get the repeat review of PR #19 from Codex at the effective head `6f82d26`.
## Session 70: 2026-09-17, Codex

Author: Codex
Session: repeat review of PR #19 at effective head `38aa19f`, on branch `docs/pr-83-skip-set-decision`. Role: reviewer. Base: `ee4305a`.

### What this session did, and why

- Read the PR review instructions, earlier review record, and response file.
- Verified that Session 64 names Claude Code as the author. Codex is the eligible reviewer (T-4, D-17).
- Reviewed all 11 changed paths, including the new D-602 review procedure.
- Found P2-1: the merged-PR stop rule conflicts with the transitional prompt that D-601 requires.
- Ran `make verify`. The build, 8 tests, format, STE, and smoke checks passed.
- Verified that all 9 live CI checks pass on PR head `cd5ee36`.
- The Gitar dashboard approves effective head `38aa19f`, but no Gitar run exists on current PR head `cd5ee36`. Its pass is stale.
- Updated `docs/reviews/pr-19.md` with the finding and the verdict `Changes required` for effective head `38aa19f`.

### State of the build

- `main` is `ee4305a`. The effective head is `38aa19f`, and the remote PR tip is `cd5ee36`.
- `make verify` passes. All 9 live CI checks pass on `cd5ee36`.
- The latest Gitar approval is for `38aa19f`, so the automated review is stale for the branch tip.
- The review and handoff commit `cd5ee36` changes metadata only, so it does not move effective head `38aa19f`. The unrelated untracked `deck-test/` remains untouched.

### In flight

P2-1 needs a correction. A current Gitar review also needs to cover the branch tip before merge.

### Traps and gotchas

- Step 1 of the one-PR skill blocks work when the bound PR is merged. Step 6 requires a prompt after that merge (D-601).
- The PR tip can move through a metadata commit while its effective head stays `38aa19f`.
- D-602 does not apply because this review session has network and writable Git metadata.
- The next ids are D-603, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 71.

### Open questions that block progress

No owner question blocks PR #19. P2-1 and the stale Gitar review block the merge.

### Next concrete action

Correct P2-1, push the correction, complete the Gitar pass, then repeat the Codex review (D-582).

## Session 69: 2026-09-17, Claude Code

Author: Claude Code
Session: the owner answer for the review gate of PR #19, in the same session as Sessions 64, 66, and 68 (D-582).
Repository: the-thing-below. Branch: `docs/pr-83-skip-set-decision`. PR: #19. Role: author. Base: `ee4305a`.

### What this session did, and why

- Two review rounds of PR #19 gave no finding and no verdict, because the review session has no GitHub access and a read-only `.git` directory.
- The session asked the owner and gave four options. The owner chose the verdict from the local evidence, and asked for the rule in the `pr-review` skill.
- D-602 records the rule. A review session with no network can give `Ready for owner merge` when the local branch holds the effective head, the review reads the whole diff, and each local check runs. The record marks the evidence of the author and lists each item that the session cannot verify.
- The `pr-review` skill gets the section "A review with no network", and the Verification and Verdicts sections point to it.
- The change moves the effective head, because it changes `docs/decisions.md` and `.claude/skills/`. The PR needs a new Gitar pass and a new review round.

### State of the build

- `main` is `ee4305a`. The new effective head is the head of this round.
- The ten checks pass on `38aa19f`, the Gitar check included. The Gitar pass approves that head with no finding, and the PR has zero review threads.
- The freshness check passes for the new head: the reply "Running the review now" came at `12:29:51Z`, and the new dashboard comment `5714387972` has the edit time `12:31:10Z`.
- `make verify` passes on the Mac of the owner.

### In flight

The review round of PR #19 at the effective head `38aa19f`, which gives the verdict under D-602.

### Traps and gotchas

- A decision that comes from a review can move the effective head. This round does, so the earlier Gitar pass and the earlier review rounds do not cover it.
- Automatic Gitar reviews are paused on this trial, so the new head needs a `Gitar review` comment after the push wait.
- D-602 gives no permission to skip a local check. A session that cannot run a check names it in the record.
- The next ids are D-603, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 70.

### Open questions that block progress

None for PR #19. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Get the review round of PR #19 from Codex at the effective head `38aa19f`, under D-602.
# Session handoff archive

## Session 68: 2026-09-17, Claude Code

Author: Claude Code
Session: the answer to the repeat review of PR #19, in the same session as Sessions 64 and 66 (D-582).
Repository: the-thing-below. Branch: `docs/pr-83-skip-set-decision`. PR: #19. Role: author. Base: `ee4305a`.

### What this session did, and why

- The repeat review of Session 67 gives no finding and keeps the `Blocked` verdict. The reason is the environment of the review session: no GitHub access, and a read-only `.git` directory.
- This round commits the updated `docs/reviews/pr-19.md` and the Session 67 entry with no change to their text, because that session cannot commit or push.
- `docs/reviews/pr-19-response.md` gets a section for the repeat review. It corrects two points of the record: the response file is the work of the author and not of the owner, and the record names the repository `natekramber/the-thing-below`, which does not exist.
- The round moves the Session 57 and Session 58 entries to `docs/session-handoff-archive.md`, because the file keeps the 10 newest entries (D-18).
- Two review rounds now give no finding and no verdict. The session asked the owner how to unblock the gate of T-4.

### State of the build

- `main` is `ee4305a`. The effective head is `c065a11`, and this round changes `docs/reviews/` and `docs/session-handoff.md` alone.
- The nine CI checks pass, and the Gitar pass approves `c065a11` with no finding.
- `make verify` passes on the Mac of the owner.

### In flight

The answer of the owner about the review gate of PR #19.

### Traps and gotchas

- The review session of this machine has no network and a read-only `.git` directory. A review in that environment can read the diff, and it cannot verify the PR, the comments, or the checks.
- An author cannot clear a `Blocked` verdict, and evidence from the author is not independent evidence for the reviewer.
- The next ids are D-602, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 69.

### Open questions that block progress

The review gate of PR #19 blocks the merge. The session asked the owner, and the answer becomes D-602 or OQ-183.

### Next concrete action

Get the answer of the owner about the review gate, then apply it to PR #19.

## Session 67: 2026-09-17, Codex

Author: Codex
Session: repeat review of PR #19 at effective head `c065a11`, on branch `docs/pr-83-skip-set-decision`. Role: reviewer. Base: `ee4305a`.

### What this session did, and why

- Re-read the review instructions and the existing PR #19 review record and response file.
- The local branch advanced to `d6d1529`. Commits after `c065a11` only change review and session handoff records, so the effective head remains `c065a11`.
- The response file reports the PR head, Gitar result, comments, checks, and build result. GitHub access failed again, so this session could not verify those claims independently.
- The review record still gives `Blocked`. It also records that the Git index is read-only, so this session cannot commit or push its update.

### State of the build

- `main` points to `ee4305a`. The local branch and its tracking ref point to `d6d15294f7fe4772e693ccf949a0154056102a1d`.
- The effective head remains `c065a11` because later commits change metadata only.
- `git diff --check` passes for the PR changes. The previous session's STE and identity checks pass; its build result remains unknown from this environment.
- GitHub checks, comments, PR state, and live remote head remain unverified.

### In flight

The review remains blocked until GitHub evidence can be verified and the updated record can be committed and pushed.

### Traps and gotchas

- `gh` cannot connect to `api.github.com`.
- `git fetch` cannot write `.git/FETCH_HEAD`, and Git cannot create `.git/index.lock`.
- The response file is author-provided evidence, not independent confirmation by this review.
- The next ids are D-602, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 68.

### Open questions that block progress

No owner question blocks PR #19. GitHub access and Git metadata write access block the final review publication.

### Next concrete action

Restore GitHub API and Git write access. Verify the PR comments and checks, then commit and push the existing review record and this entry.

## Session 66: 2026-09-17, Claude Code

Author: Claude Code
Session: the answer to the review of PR #19, in the same session as Session 64 (D-582).
Repository: the-thing-below. Branch: `docs/pr-83-skip-set-decision`. PR: #19. Role: author. Base: `ee4305a`.

### What this session did, and why

- The review of Session 65 gives no finding on the change. Its verdict is `Blocked`, because that session had no GitHub access, no writable `.git/FETCH_HEAD`, and no build result.
- `docs/reviews/pr-19-response.md` gives each piece of evidence that the review session could not get: the head, the state, the comment export, the thread count, the nine checks, and the result of `make verify`.
- The review session could not push. This round commits `docs/reviews/pr-19.md` and the Session 65 entry with no change to their text, so the PR holds each record of its review (D-577).
- The round moves the Session 55 and Session 56 entries to `docs/session-handoff-archive.md`, because the file keeps the 10 newest entries (D-18).

### State of the build

- `main` is `ee4305a`. The PR head on GitHub is `0bcd246`, the state is `OPEN`, and the merge state is `CLEAN`.
- The effective head is `c065a11`. This round changes `docs/reviews/` and `docs/session-handoff.md` alone, which is the metadata set, so the effective head does not move.
- The nine CI checks pass, and the Gitar pass approves `c065a11` with no finding. The comment export gives one comment, which is the Gitar dashboard, and zero review threads.
- `make verify` passes on the Mac of the owner in 9 seconds with a warm build.

### In flight

The repeat review of PR #19 at the effective head `c065a11` (T-4, D-17). Only the reviewer can set the verdict.

### Traps and gotchas

- A `Blocked` verdict can name no defect. This one names missing evidence of the review session, and the author cannot clear it.
- The review session read 30 seconds of `dotnet build` as an unknown result. A cold build takes longer than that, and a restore with no network fails.
- A metadata commit does not move the effective head, and it needs no new Gitar request. Automatic Gitar reviews are paused on this trial, so a request costs one of a limited set.
- The next ids are D-602, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 67.

### Open questions that block progress

No owner question blocks PR #19. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3. OQ-182 blocks nothing.

### Next concrete action

Get the repeat review of PR #19 from Codex at the effective head `c065a11`.

## Session 65: 2026-09-16, Codex

Author: Codex
Session: review of PR #19 at effective head `c065a11`, on branch `docs/pr-83-skip-set-decision`. Role: reviewer. Base: `ee4305a`.

### What this session did, and why

- Read the handoff, the one-PR, PR-review, project-contract, review-record, commit, and STE instructions.
- Verified the provider gate. Session 64 names Claude Code as the PR author, so Codex is eligible under T-4 and D-17.
- Inspected all eight changed paths and checked the D-600 path set against the workflow and the equal agent files against D-20.
- The STE check and whitespace check pass. The build did not finish within 30 seconds.
- GitHub API access failed. The PR description, current comments, live checks, and remote head could not be verified.
- The review record gives `Blocked` for effective head `c065a11`, because required current PR evidence is unavailable.

### State of the build

- `main` points to `ee4305a`. The checked-out PR branch and its tracking ref point to `0bcd246`.
- The effective head is `c065a11`; later commits only change metadata.
- The STE check gives zero findings, the diff check passes, and `CLAUDE.md` equals `AGENTS.md`.
- The build result is unknown. GitHub checks and the current remote head were not available.
- The unrelated untracked `deck-test/` directory remains untouched.

### In flight

The PR review is blocked on GitHub evidence and cannot reach its hand-over point.

### Traps and gotchas

- `git fetch` failed because `.git/FETCH_HEAD` is not writable in this environment.
- `gh` could not connect to `api.github.com`, so the required comment export produced no file.
- Do not treat handoff reports of a green Gitar pass and CI as independent verification.
- The next ids are D-602, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 66.

### Open questions that block progress

No owner question blocks PR #19. The missing network evidence blocks a final review verdict.

### Next concrete action

Restore GitHub API access, export the PR description and all comments in one command, verify the live checks and remote head, then update this review record.

## Session 64: 2026-09-16, Claude Code

Author: Claude Code
Session: PR-83, the decision row of the docs-only skip set and the transitional prompt rule.
Repository: the-thing-below. Branch: `docs/pr-83-skip-set-decision`. PR: #19, which is PR-83 of the roadmap. Role: author. Base: `ee4305a`.

### What this session did, and why

- PR #18 merged as `ee4305a`. Its `changed-paths` job holds a skip set, and no decision recorded the paths. The reasoning was a comment of `.github/workflows/ci.yml` alone, and no file in `docs/` named it.
- D-600 records the skip set: `docs/`, `.claude/`, `README.md`, `LICENSE`, and `.github/pull_request_template.md`. `CLAUDE.md` and `AGENTS.md` stay out of it, because `AgentFileTests` reads both files and fails when they differ (D-20). A skipped job reports `Success`, so a skip of the agent files would pass the PR shape that breaks D-20 most often.
- The skip set and the override set of D-16 are not the same set. The override set holds both agent files, and `LICENSE` is in the skip set alone.
- The comment of the `changed-paths` job and a bullet of `docs/roadmaps/area-ci.md` now cite D-600.
- D-601 records the transitional prompt. After the owner says `Merged PR #x` for the PR of the session, the session writes one fenced block for the next clean session, and then it ends. `CLAUDE.md` and `AGENTS.md` hold the rule, and step 6 of the `one-pr-one-session` skill holds the template.
- The owner approved the two concerns of this PR before the work started. G-8 refuses a second concern without that answer.

### State of the build

- `main` is `ee4305a`. The branch `docs/pr-83-skip-set-decision` sits on that base.
- `make verify` passes on the Mac of the owner: the build, 8 tests, the format check, the STE check, and the smoke session.
- The PR adds two decision rows and changes `.github/workflows/`, so the `review-override` label does not apply (D-401, D-560). The PR needs the Codex review.
- The nine CI checks pass on `c065a11`, and the Gitar pass approves that head with no finding. The head of command B is `c065a11`, and the dashboard edit at `2026-09-17T03:55:13Z` is later than the push at `2026-09-17T03:54:29Z`. The PR has zero review threads.
- The build and test legs ran on this PR and did not skip, because the PR changes the agent files. This is the rule of D-600 at work.
- The change to `CLAUDE.md` and `AGENTS.md` keeps the two files identical, and it takes both files out of the skip set of D-600. The build and test job runs on this PR.

### In flight

The Codex review of PR #19 at the effective head `c065a11` (T-4, D-17). The commit of this record changes `docs/session-handoff.md` alone, which is in the metadata set, so it does not move the effective head.

### Traps and gotchas

- The session verified each claim of D-600 against the code: the `case` pattern of the job, the paths that `AgentFileTests` reads, and the run time of the three legs on PR #18, which was 37 to 79 seconds. The `ste-check` job holds no skip condition, so a docs PR still gets the STE check.
- `CLAUDE.md` is 16823 bytes, and the size check of OQ-182 proposes a limit of 16 KB. The file passed that limit on `main` at `ee4305a`, before this PR. OQ-182 has no answer, and no check exists.
- The uncommitted work of the merged branch `feat/pr-1-scaffold` is in a git stash of this machine. The patch of the owner replaced it. Drop the stash after the merge.
- A skipped job reports `Success`. A path rule that is too wide passes a PR that ran no check.
- The next ids are D-602, OQ-183, F-65, L-16, G-27, PR-84, M-8, and Session 65.

### Open questions that block progress

None for PR-83. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3. OQ-182 blocks nothing. The owner has not run the Deck test, so PR-82 waits and the renderer stays provisional (D-599).

### Next concrete action

Hand PR #19 to Codex for the review, and write `docs/reviews/pr-19.md`.

## Session 63: 2026-09-16, Codex

Author: Codex
Session: repeat review of PR #18 at effective head `d8c31b8`, on branch `feat/pr-1-scaffold`. Role: reviewer. Base: `9f27f12`.

### What this session did, and why

- Read the start set and the repeat-review, review-record, commit, and STE instructions.
- Verified that Claude Code authored PR #18. Codex remains the eligible reviewer under T-4 and D-17.
- Verified that the current effective head is `d8c31b8`; the later commits only change metadata.
- Read the P2-4 answer with the regression table. Each says the old target runs without end, and a frame limit alone would leave a false pass.
- Checked that the Makefile and CI set a frame limit and require the success line.
- The latest Gitar dashboard approves `d8c31b8`. All nine CI checks pass on the current branch tip.
- The saved GraphQL query reports zero unresolved threads. A fresh export failed to connect.
- The owner confirms that zero inline threads remain unresolved.
- Set P2-4 to fixed in `d8c31b8`. The verdict is `Ready for owner merge` for that head.
- The handoff held ten entries. Session 53 moved to the archive (D-18).

### State of the build

- `main` is `9f27f12`. The PR branch tip before this review commit is `47a7e1639104b69d726c993c4404e4bcbbe552fd`.
- The effective head is `d8c31b8`. The response, design row, and roadmap row now state the same two-fault behavior.
- All nine checks pass on the branch tip: changed paths, STE check, build/test/format on three platforms, coverage, and smoke on three platforms.
- The Gitar dashboard approves the effective head with one closed finding and no open issues.
- No uncommitted source or project changes exist. The unrelated untracked `deck-test/` directory remains unchanged.

### In flight

The review record and this entry need one metadata commit and push. The verdict applies to effective head `d8c31b8`.

### Traps and gotchas

- A frame limit and a success-line check are both needed for the smoke contract (F-64).
- The API did not return inline threads in this session. The response file records the last successful query and result.
- Do not add unrelated paths to the review commit. Keep `deck-test/` untouched.
- The next ids are D-600, OQ-183, F-65, L-16, G-27, PR-83, M-8, and Session 64.

### Open questions and accepted risks

None for PR #18.

### Next concrete action

Wait for the owner merge of PR #18.

## Session 62: 2026-09-16, Claude Code

Author: Claude Code
Session: the answer to the repeat Codex review of PR #18, in the session that authored it (D-582).
Repository: the-thing-below. Branch: `feat/pr-1-scaffold`. PR: #18. Role: author. Base: `9f27f12`.

### What this session did, and why

- Read `docs/reviews/pr-18.md` at head `6e0622a`. The repeat review withdrew P2-1, set P2-2 and P2-3 to fixed, and opened P2-4. The verdict is `Blocked`.
- P2-4 has full merit. The response file said that the `smoke` target of `0c402dd` reported success for a broken session, and its table said that the case runs without end. Both cannot hold.
- The table holds. With no frame limit the session waits until a kill, so the target reports no result.
- The sentence conflated the two faults of the old target. The target has no frame limit, and it reads no log. A frame limit alone turns the first fault into the second, which is a false pass. The correction needs both parts.
- `docs/reviews/pr-18-response.md` now names the two faults, agrees with its table, and holds a P2-4 section.
- The same wrong sentence was in the F-64 row of `docs/design.md`, which this PR wrote. That row and the F-64 row of `phase-1-foundations.md` now name the two faults.
- The review could not enumerate the inline threads, because its API calls failed. The response file now holds the exact query and its result, which is 0 unresolved threads.

### State of the build

- `main` is `9f27f12`. The effective head stays `6e0622a`, and the Gitar pass approves it.
- This round changes `docs/reviews/pr-18-response.md`, `docs/design.md`, `docs/roadmaps/phase-1-foundations.md`, and this file.
- `docs/design.md` and `docs/roadmaps/` are outside the metadata set, so this commit moves the effective head and needs a new Gitar review.
- `make verify` passes on the Mac of the owner. The nine CI checks passed on `6e0622a` and on `d8c31b8`.
- The new effective head is `d8c31b8`. The Gitar pass of that head gives `Approved`, with 1 comment, 1 with merit, 0 open issues, and 0 unresolved threads. Commit `0c402dd` answered that comment.
- That review is current. The head matches, and the dashboard edit time of 03:04:49Z is later than the push time of 03:00:57Z and later than the reply of 03:04:30Z.

### In flight

The repeat Codex review of effective head `d8c31b8` (T-4, D-17). The Gitar pass of that head is complete and approves it.

The commit that holds this entry changes `docs/session-handoff.md` alone, so it is a metadata commit and it does not move the effective head.

### Traps and gotchas

- A response file can hold a prose claim and a table that disagree. Read them together before the commit, and give one result for one command and one trigger (T-5).
- Two faults in one command can compose. Name each one, and say which fault a partial correction leaves.
- A wrong claim in a response file can also sit in the design register. Grep for the sentence, and not for the id.
- The metadata set is `docs/reviews/`, `docs/session-handoff.md`, and `docs/session-handoff-archive.md`. A round that also changes `docs/design.md` moves the effective head.
- Gitar replaced its dashboard comment in each round of this PR. The id moved from `5706916715` to `5707572860`, and then to `5707837154`. Read the newest id in each check.
- The next ids are D-600, OQ-183, F-65, L-16, G-27, PR-83, M-8, and Session 63.

### Open questions that block progress

None for PR #18. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Hand PR #18 back to Codex for the repeat review of `d8c31b8`. The session stays bound to PR #18 and answers each finding (D-582).


## Session 61: 2026-09-16, Codex

Author: Codex
Session: repeat review of PR #18 at effective head `6e0622a`, on branch `feat/pr-1-scaffold`. Role: reviewer. Base: `9f27f12`.

### What this session did, and why

- Read the current handoff, the PR response, and the repeat-review, review-record, contract, Gitar, and commit skills.
- Verified the provider gate. Session 60 names Claude Code as the author, and Codex remains the opposite provider (T-4, D-17).
- Verified that `a56a7ce` changes the handoff alone, so the effective head is `6e0622a`.
- Reproduced the F-60 build callback failure. Godot logged the error and returned 1, so P2-1's exact trigger is withdrawn.
- Verified P2-2. The Documents line now names both actual review files in the `Changed:` form.
- Verified the corrected smoke path. `make smoke` passed on a healthy tree and failed when a wrapper removed the managed assembly during the real Godot session.
- Found P2-4 in the response file: its prose says the old target reported success, but its table says that case ran without end.
- CI run 35175182671 passed all nine checks on tip `a56a7ce`.
- GitHub API calls for inline review threads failed. The current Gitar dashboard summary says the pass approved the correction head, with one closed finding and no open issue.
- Updated `docs/reviews/pr-18.md`. The current verdict remains `Blocked` for head `6e0622a`.
- The handoff held ten entries before this one, so Session 51 moves to the archive (D-18).

### State of the build

- `main` is `9f27f12` (PR #17). PR #18 is open on `feat/pr-1-scaffold`.
- The effective head is `6e0622a`. The current remote tip is `a56a7ce`, a metadata commit.
- CI run 35175182671 passed all nine checks on the remote tip.
- The healthy smoke run passed. The missing-assembly regression failed on the absent success line, as required.
- The inline review-thread export remains incomplete.

### In flight

P2-1 is withdrawn, P2-2 and P2-3 are fixed, and P2-4 remains open. The review cannot reach its hand-over point until the response text is corrected and the remaining review evidence is complete.

### Traps and gotchas

- The editor returns 1 on the F-60 build callback error. The original evidence read the code of `tail` through a pipe.
- A session with no loadable boot assembly waits without end. The new frame limit ends the session, and the missing success line fails the smoke check (F-64).
- `docs/reviews/pr-18-response.md` line 28 conflicts with the regression table at line 47.
- The next ids are D-600, OQ-183, F-64, L-16, G-27, PR-83, M-8, and Session 62.

### Open questions that block progress

No owner question blocks the review. P2-4 and the inline comment export remain unresolved.

### Next concrete action

Correct the conflicting statement in the response file. Then repeat the review of PR #18 at its new effective head.

## Session 60: 2026-09-16, Claude Code

Author: Claude Code
Session: the answer to the Codex review of PR #18, in the same session that authored it (D-582).
Repository: the-thing-below. Branch: `feat/pr-1-scaffold`. PR: #18. Role: author. Base: `9f27f12`.

### What this session did, and why

- Read `docs/reviews/pr-18.md`. The verdict was `Blocked` for head `0c402dd`, with P2-1 and P2-2 open.
- P2-1 has partial merit. Its trigger does not reproduce, and the defect that it aims at is real.
- The editor gives an exit code of 1 when its build callback fails, and not 0. The first measurement of this PR read `$?` after a pipe to `tail`, so it read the exit code of `tail`. F-60 carried that wrong claim, and the row now marks that part refuted and keeps it.
- The verification found the real failure. A headless session whose managed assembly does not load never reaches `Quit`, and it runs without end. With `--quit-after` it ends with an exit code of 0 and no success line. This is F-64.
- The `smoke` target of the Makefile now writes each log to a file, fails on a nonzero build code, runs the session with `--quit-after 600`, and fails when the success line is absent.
- The `smoke` job of CI runs the session with `--quit-after 600` too, so a broken session fails in seconds and not at the time limit of 30 minutes.
- P2-2 has full merit. The `docs/reviews/` line of the Documents section matched none of the three forms of D-581, and it named a placeholder path. The PR description now names `docs/reviews/pr-18.md` and `docs/reviews/pr-18-response.md` in the `Changed` form.
- `docs/reviews/pr-18-response.md` records each disposition, the evidence, and the regression checks.

### State of the build

- `main` is `9f27f12`. The branch holds the scaffold, the two corrections, and the review records.
- `make verify` passes on the Mac of the owner: the build, the 8 tests, `dotnet format`, the STE check with 0 findings, and the smoke session.
- The regression checks pass. `make smoke` gives 2 on a failed Godot build, gives 2 in about 6.5 seconds on a boot class that the scene cannot instantiate, and gives 0 on a healthy tree.
- The Gitar review of `0c402dd` gave `Approved`, with 1 finding closed and 0 unresolved threads.
- The Gitar pass of the correction head `6e0622a` gives `Approved`, with 1 comment, 1 with merit, and 0 open issues. Commit `0c402dd` answered that comment, and the thread is resolved.
- That review is current. The head matches, the dashboard edit time of 02:33:02Z is later than the push time of 02:25:14Z and later than the `On it` reply of 02:29:01Z.
- CI run on `6e0622a` passed each of the nine checks, the three smoke legs with `--quit-after` included.

### In flight

The repeat Codex review of PR #18 at effective head `6e0622a` (T-4, D-17). The Gitar pass of that head is complete and approves it.

The commit that holds this entry changes `docs/session-handoff.md` alone, so it is a metadata commit and it does not move the effective head.

### Traps and gotchas

- A measurement of an exit code through a pipe reads the exit code of the last command of the pipe. Redirect to a file, or set `pipefail`, before you record an exit code as evidence.
- A headless Godot session that cannot instantiate its boot script waits without end. Always give `--quit-after` to a session that a check runs (F-64).
- An exit code of 0 from a smoke session proves nothing. The success line in the log is the proof (T-2).
- A finding can name a real defect through a trigger that does not reproduce. Reproduce the trigger, then look for the defect that the finding aims at.
- Gitar replaced its dashboard comment during this round. The id moved from `5706916715` to `5707572860`. Read the newest id in each check, and never a saved one.
- A string comparison with `\>` inside `[ ]` fails in zsh. Use `sort`, or read the times in Python.
- The next ids are D-600, OQ-183, F-65, L-16, G-27, PR-83, M-8, and Session 61.

### Open questions that block progress

None for PR #18. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Hand PR #18 back to Codex for the repeat review of `6e0622a`. The session stays bound to PR #18 and answers each finding (D-582).

## Session 59: 2026-09-16, Codex

Author: Codex
Session: follow-up verification for the PR #18 review at effective head `0c402dd`, on branch `feat/pr-1-scaffold`. Role: reviewer. Base: `9f27f12`.

### What this session did, and why

- Verified that review commit `a9a3d07` is the remote branch tip and does not change the effective head.
- Verified that CI run 35171555340 passed all nine checks on the pushed review commit.
- Read the current Gitar PR comment through `gh pr view`. Its macOS finding is fixed in `0c402dd`, and the smoke job passes on macOS.
- Tried the required export of issue comments, review bodies, and inline threads. GitHub API access failed for the thread data.
- Updated `docs/reviews/pr-18.md` with the final CI results, the verified push, and the Gitar claim.
- The handoff held ten entries before this one, so Session 49 moves to the archive (D-18).

### State of the build

- `main` is `9f27f12` (PR #17). PR #18 is open on `feat/pr-1-scaffold`.
- The effective head is `0c402dd`. The review metadata commit `a9a3d07` is the remote tip.
- CI run 35171555340 passed all nine checks on the remote tip.
- The required inline thread export remains unavailable. The local `make verify` command did not complete.

### In flight

PR #18 has two open P2 findings. The review verdict is Blocked until the findings and required comment evidence are resolved.

### Traps and gotchas

- `gh pr view` returned the issue comment and review summaries, but the API calls for inline review threads failed.
- The workspace has an untracked `deck-test/` directory. It remains unchanged.
- The next ids are D-600, OQ-183, F-64, L-16, G-27, PR-83, M-8, and Session 60.

### Open questions that block progress

No owner question blocks the review. GitHub API access blocks the remaining thread evidence.

### Next concrete action

Export all inline threads when GitHub API access works, verify each claim and reply, and then update the review record.

## Session 58: 2026-09-16, Codex

Author: Codex
Session: review of PR #18 at effective head `0c402dd`, on branch `feat/pr-1-scaffold`. Role: reviewer. Base: `9f27f12`.

### What this session did, and why

- Read the top handoff entry, the one-PR skill, the PR review skill, the contract reference, the review-record reference, and the STE skill.
- Verified the provider gate. Session 57 names Claude Code as the author, and Codex is the eligible reviewer (T-4, D-17).
- Verified that `b32a971` changes the handoff alone, so the effective head remains `0c402dd`.
- Inspected all 34 paths of the effective diff and checked the PR description and current CI run.
- Found P2-1: the local smoke target can accept the Godot build callback failure that F-60 documents.
- Found P2-2: the PR description's review Documents line does not use the form of D-581.
- The required comment export failed twice because GitHub's API did not connect. Local `make verify` stalled in `dotnet build` and was interrupted.
- Wrote `docs/reviews/pr-18.md` with the verdict `Blocked` for effective head `0c402dd`.
- Moved Session 48 to the archive because this entry makes it the eleventh handoff entry (D-18).

### State of the build

- `main` is `9f27f12` (PR #17). PR #18 is open on `feat/pr-1-scaffold`.
- CI run 35169864293 on tip `b32a971` passed all nine checks. Its implementation head is `0c402dd`.
- The local build produced no output for 60 seconds. The review interrupted it, so local tests, format, STE, and smoke did not run.
- The comment export did not complete. The existing PR comments remain unverified.

### In flight

PR #18 needs both findings corrected. The review also needs the full comment export before it can give a final verdict.

### Traps and gotchas

- The Godot editor can return 0 when its build callback fails. The CI workflow reads the log, but the Makefile does not (F-60, T-2).
- The PR description line for `docs/reviews/` uses a placeholder path and omits the required `Changed:` form (D-577, D-581).
- The working tree held an untracked `deck-test/` directory before the review. It remains unchanged.
- The next ids are D-600, OQ-183, F-64, L-16, G-27, PR-83, M-8, and Session 59.

### Open questions that block progress

No owner question blocks the review. GitHub API access blocks the required comment evidence.

### Next concrete action

Export every comment on PR #18 when GitHub API access works. Verify the claims, then complete the review of the same head.

## Session 57: 2026-09-16, Claude Code

Author: Claude Code
Session: PR-1, the repository scaffold, which is the first code of the repository.
Repository: the-thing-below. Branch: `feat/pr-1-scaffold`. PR: PR-1. Role: author. Base: `9f27f12`.

### What this session did, and why

- Read the top handoff entry, `CLAUDE.md`, and the `one-pr-one-session`, `ste-writing`, `design-doc-style`, and `csharp-conventions` skills.
- Read `deck-test/handover.md` and `deck-test/readme.md` on `spike/deck-test`. That step has no PR, so no document held its eight owner answers.
- Wrote D-592 to D-599 for those answers, and resolved OQ-75, OQ-76, OQ-77, OQ-78, OQ-83, OQ-92, and OQ-93.
- D-599 revises D-160 in part. The renderer pick comes after PR-1, and PR-1 sets Forward+ as a provisional renderer. PR-82 sets the picked renderer in one line.
- Added the cost model rows of the Deck test to section 4 of `docs/design.md`, with M-7 for the frame time of the test scene.
- Added the PR-82 entry to `phase-1-foundations.md`, section 7.3, and renumbered the later entries of section 7.
- Built the scaffold: `TheThingBelow.slnx`, `global.json`, `Directory.Build.props`, the Makefile, the pre-commit hook, the four projects of D-217, and the CI workflow.
- Wrote eight tests: the agent-file match test, three Core reference tests, and four tests of the tools command line.
- Opened PR #18. The first CI run failed two smoke legs, and a second commit corrected the workflow (F-62, F-63).

### State of the build

- `main` is `9f27f12` (PR #17). The branch `feat/pr-1-scaffold` holds the scaffold.
- `make verify` passes on the Mac of the owner: the build, the 8 tests, `dotnet format`, the STE check with 0 findings, and the Godot smoke session.
- The smoke session prints the renderer as `forward_plus` and the frame as 1280 by 720, and it ends with no error.
- The CI workflow holds five jobs: `changed-paths`, `build-test-format`, `coverage`, `smoke`, and `ste-check`.
- The first run of PR #18, 35169279617 on `0b319f8`, failed `smoke (macos-26)` and `smoke (windows-2025)`. Each other check passed.
- The second run, 35169532179 on `0c402dd`, passed each of the nine checks, the two corrected legs included.
- The Gitar review of `0c402dd` gives `Approved`, with 1 finding closed and no open issue. That review is current: the head matches, and the dashboard edit time of 01:14:25Z is later than the push time of 01:10:42Z.

### In flight

The Codex review of PR #18 (T-4, D-17). The PR adds decision rows, so the `review-override` label does not apply (D-401). It also changes `.github/workflows/`, which is never exempt (D-560).

The effective head is `0c402dd`. The commit that holds this entry changes `docs/session-handoff.md` alone, so it is a metadata commit and it does not move the effective head.

### Traps and gotchas

- The Godot editor writes `net8.0` into a `.csproj` that holds no target framework, over `Directory.Build.props` (F-60). The Game project pins `net10.0` in its own file, and `.gitignore` holds `*.csproj.old`.
- The Godot editor build gives an exit code of 0 when its build callback fails. The smoke job reads the log for `build callback failed` (F-60, T-2).
- A coverage run instruments the Core copy in the test output folder and adds `System.Threading` to it (F-61). The reference test reads the file that the Core project built.
- The compiler writes no metadata entry for a project reference that no code uses. A second test reads the Core project file, so an added reference fails (F-61).
- The generated entry point of `xunit.v3` runs the console runner unless `UseMicrosoftTestingPlatformRunner` is on. The console runner reads no Coverlet option.
- Coverlet 10 takes `--coverlet`, and not `--coverage`. It writes its file to the results directory, and it takes no output path.
- The macOS archive of Godot holds `Godot_mono.app`, and not `Godot.app` (F-62). The find pattern of the smoke job reads `*.app/Contents/MacOS/Godot`.
- The git-bash of the Windows runner carries `sha512sum` and no `shasum` (F-63). The checksum step reads the digest itself and names both values.
- Gitar found the macOS fault of F-62 by reading the workflow, and CI found the same fault by running it. Gitar found no fault in the Windows checksum step, which only the Windows runner showed.
- Gitar paused automatic reviews for the trial period, and a review still ran on each push of this PR. Read the dashboard comment, and never the pause note alone.
- The owner ran no Deck test yet. PR-82 waits for that run.
- The next ids are D-600, OQ-183, F-64, L-16, G-27, PR-83, M-8, and Session 58.

### Open questions that block progress

None for PR-1. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Hand PR #18 to Codex for the review of T-4. The session stays bound to PR #18 and answers each finding (D-582).

## Session 56: 2026-09-16, Claude Code

Author: Claude Code
Session: the Deck test of step 7.1 of `docs/roadmaps/phase-1-foundations.md`. That step has no PR and no review.
Repository: the-thing-below. Branch: `spike/deck-test`. PR: none. Role: author of a spike.

### What this session did, and why

- Built the throwaway test scene of D-160 and D-523 on the branch `spike/deck-test`, which never merges (D-597).
- The scene draws the load of D-160 at the frame of 1280 by 720: normal maps, point lights with shadows, glow, the four ambient kinds, fog, the CRT pass, and a wipe transition.
- The sweep runs 20 stages. Each stage holds 60 warm-up frames and 300 measured frames.
- `scripts/FrameMeter.cs` reads the time of each frame and counts each frame over 16.667 milliseconds (D-598).
- Ran the fetch script for the export templates, and the SHA-512 matched (D-596).
- The owner answered eight questions. `deck-test/handover.md` holds each one, and PR-1 records them as D-592 to D-599.

### State of the build

- The scene, the shaders, the meter, and the report all work. A run on the Mac proved them.
- The native Linux export `build/DeckTest.x86_64` exists. No machine ran it.
- `main` held no code during this session.

### In flight

The run on the Deck, which the owner does. That run answers D-160 and gives the first effect budget of D-523.

### Traps and gotchas

- The Godot export needs a solution file beside `project.godot`. With none, the export writes an ELF file, exits 0, and packs no managed assembly.
- An exit code of 0 hides an export fault. The export gives `completed with warnings` and exits 0. PR-54 must read the log.
- macOS caps the frame rate whatever the vsync setting says. A Mac run reports 16.67 milliseconds in every stage, and the report refuses to give a budget (T-2).
- Only the Deck run answers D-160. A Mac run tests the scene and the report, and nothing else.
- Godot drops each light past 15 on one canvas item with no message (F-46). The sweep stops the light row at 15.

### Open questions that block progress

None. OQ-92 and OQ-93 closed with D-597 and D-598.

### Next concrete action

The owner copies `build/DeckTest.x86_64` and `run-deck-test.sh` to the Deck and runs `./run-deck-test.sh`. PR-82 then sets the renderer.

## Session 55: 2026-09-16, Codex

Author: Codex
Session: repeat review of PR #16 at effective head `e3e6611`, after the owner confirmed Gitar approval.
Repository: the-thing-below. Branch: `docs/pr-16-context-budget`. PR: #16. Role: reviewer. Base: `2bc7d56`.

### What this session did, and why

- Read the current handoff and the repeat-review, review-record, commit, and STE instructions.
- Verified that the local branch and fetched remote branch both point to `e3e6611`. This commit changes the runbook, so it is the effective head.
- Verified the provider gate. Claude Code authored the PR, and Codex reviews it (T-4, D-17).
- Reproduced P2-2 in a scratch repository under `set -e`. The corrected command committed review and handoff records with no eligible STE file.
- Set P2-2 to fixed in `e3e6611`. The full interim STE check, diff check, and identity check pass.
- The owner confirmed Gitar approval of the current changes. GitHub API access failed, so the dashboard comment and check query could not be read independently.
- Updated `docs/reviews/pr-16.md`, preserved the earlier verdicts, and set the current verdict to `Ready for owner merge` for `e3e6611`.

### State of the build

- No code, solution, or Makefile exists. `main` is `2bc7d56` (PR #15).
- PR #16 is open on `docs/pr-16-context-budget`. The review and handoff commit `86b76b6` was pushed and verified as the remote head. The effective head remains `e3e6611`.
- The current verdict is `Ready for owner merge`, based on the fixed findings and the owner's Gitar confirmation.

### In flight

The review record and this entry are committed and pushed to PR #16. This entry records the verified push.

### Traps and gotchas

- GitHub API access failed during this session. The owner confirmed the current Gitar approval.
- The remote fetch succeeded. The PR page and comment API did not respond.
- The next ids are D-592, OQ-183, F-60, L-16, G-27, PR-82, M-7, and Session 56.

### Open questions that block progress

None for PR #16. OQ-182 blocks nothing. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Push this metadata update, fetch the remote, and verify the clean branch status and PR head.

## Session 54: 2026-09-16, Claude Code

Author: Claude Code
Session: the answer to the repeat review of PR #16, in the same conversation as Sessions 50 and 52 (D-582).
Repository: the-thing-below. Branch: `docs/pr-16-context-budget`. PR: #16. Role: author. Base: `2bc7d56`.

### What this session did, and why

- The owner asked the session to address the review feedback again. The repeat review of Session 53 set P2-1 to fixed and gave `Blocked` for head `d2479ce`, with P2-2 open.
- Before that review, a manual Gitar review approved `d2479ce` with 0 findings. The first wait stopped at the "On it" reply, and the second at the new dashboard comment, which had a new id.
- P2-2 has partial merit. The runbook block commits in a plain run with dated records alone. It made no commit under `set -e`, or with the `files=` line joined by `&&`, in bash and in zsh. Both providers ran it in a joined form.
- The filter now treats a `grep` status of 1 as an empty list, and a status of 2 still fails (D-585, T-2). The regression check fails on the old text under `set -e` and passes on the new text in each of the eight runs. A checker finding still stops the commit.
- The stale Gitar review has full merit, and a new request follows this push. The absent check runs have no merit as a blocker, because PR-1 and later PRs create the checks (G-16).
- `docs/reviews/pr-16-response.md` records both answers.
- The handoff held ten entries before this one, so Session 44 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code, solution, or Makefile exists. `main` is `2bc7d56` (PR #15).
- PR #16 is open on branch `docs/pr-16-context-budget`. The commit that holds this entry changes the runbook, so it is the new effective head.
- The full interim STE check gives 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #16 waits for a current Gitar review of the new head, then the repeat Codex review of P2-2 (T-4, D-17).

### Traps and gotchas

- A review commit makes the Gitar review of the effective head stale for the branch head. The author requests a new Gitar review after its next push.
- A shell run with `set -e` stops at a command substitution that returns nonzero. Test runbook commands in the plain form and under `set -e`.
- Gitar can replace the dashboard comment with a new id. Read the newest dashboard comment in each check.
- The next ids are D-592, OQ-183, F-60, L-16, G-27, PR-82, M-7, and Session 55.

### Open questions that block progress

None for PR #16. OQ-182 blocks nothing. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

The author gets a current Gitar review of the new head and answers each finding. Then the Codex reviewer repeats the review of P2-2.

## Session 53: 2026-09-16, Codex

Author: Codex
Session: repeat review of PR #16 at effective head `d2479ce`.
Repository: the-thing-below. Branch: `docs/pr-16-context-budget`. PR: #16. Role: reviewer. Base: `2bc7d56`.

### What this session did, and why

- Read the top handoff entry and the one-PR, repeat-review, review-record, and STE skills.
- Verified Claude Code authored PR #16 and its correction. Codex remains eligible under T-4 and D-17.
- Verified the base and head with GitHub PR metadata. The effective head is `d2479ce` because it changes the runbook.
- Reproduced P2-1 against the old and corrected runbook commands in bash and zsh. The old command returned 0 and left a partial file. The corrected command returned 1 and left no file.
- Set P2-1 to fixed in `d2479ce` and updated the existing review record.
- Found P2-2 in the staged-file commit command. With only review and handoff records staged, `grep -v` returns 1 and stops the `&&` chain before the commit.
- The current comment export confirms Gitar approved `d2479ce` with 0 findings and 0 threads. That pass predates the review metadata push to `60260ef`, so it is stale for the current branch head. GitHub reports no check runs.
- The handoff held ten entries. Session 43 moved word for word to the archive (D-18).

### State of the build

- No code, solution, or Makefile exists. `main` is `2bc7d56` (PR #15).
- PR #16 is open on `docs/pr-16-context-budget`, with metadata head `60260ef` and base `2bc7d56` per `gh pr view`. The effective implementation head remains `d2479ce`.
- The interim STE check, diff check, identity check, and P2-1 regression check pass.
- The P2-2 reproduction fails the documented commit chain for this metadata-only commit. A local safe empty-list handling lets the review records commit without skipping any eligible STE file.
- The review verdict is Blocked for open P2-2, the stale Gitar review, and absent check runs.

### In flight

PR #16 needs the author to fix P2-2 and push the correction. The author then gets a current Gitar review and required check results. The current review record says Blocked for effective head `d2479ce`.

### Traps and gotchas

- `git fetch` failed under the default sandbox, then succeeded with elevated access.
- The current Gitar dashboard approval is for `d2479ce`; the later review metadata push makes it stale for the branch head.
- GitHub reports no check runs for the branch.
- The documented commit pipeline fails when its path filter finds no STE-eligible staged Markdown files.
- The regression harness is `/tmp/pr16_export_regression.sh`.
- The next ids are D-592, OQ-183, F-60, L-16, G-27, PR-82, M-7, and Session 54.

### Open questions that block progress

No owner question blocks PR #16. Fresh Gitar and CI evidence remains unavailable.

### Next concrete action

The author fixes P2-2 so an empty eligible-file list does not stop the commit command. Then the author pushes the correction, requests a current Gitar review, and checks why no CI jobs report. Codex repeats the review at the new effective head.

Sessions older than the 10 in `docs/session-handoff.md`, newest first (D-18). Move an entry here word for word.

## Session 52: 2026-09-16, Claude Code

Author: Claude Code
Session: the answer to the review of PR #16, in the same conversation as Session 50 (D-582).
Repository: the-thing-below. Branch: `docs/pr-16-context-budget`. PR: #16. Role: author. Base: `2bc7d56`.

### What this session did, and why

- The owner asked the session to address the review feedback. The Codex review of Session 51 gave `Changes required` for head `45e960d`, with one finding, P2-1.
- Before the review, the session requested a manual Gitar review of `45e960d`. It waited with the one wait command of D-586 two times: the first wait stopped at the placeholder comment, and the second at the review. Gitar approved with 0 findings and 0 threads.
- P2-1 has full merit. The comment export of `docs/runbooks/session-context.md` returned 0 and left a comments file after a failed GitHub call. A fake `gh` reproduced it in bash and in zsh.
- The export now runs in one `&&` chain into a part file, renames the file only after every call passes, and fails with a message otherwise (T-2, D-589). The regression check fails on the old runbook text and passes on the new text in both shells. The real `gh` run saved three comments.
- `docs/reviews/pr-16-response.md` records the answer.
- The handoff held ten entries before this one, so Session 42 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code, solution, or Makefile exists. `main` is `2bc7d56` (PR #15).
- PR #16 is open on branch `docs/pr-16-context-budget`. The commit that holds this entry changes the runbook, so it is the new effective head.
- The full interim STE check gives 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #16 waits for a current Gitar review of the new head, then the repeat Codex review of P2-1 (T-4, D-17).

### Traps and gotchas

- The first Gitar comment after a request can be a placeholder with the pause note and a spinner. Wait again with `since` at its time (D-586).
- The review commit of Session 51 came from the same checkout. `git fetch` alone did not show it, because the local branch already held it.
- The push line of `docs/reviews/pr-16.md` holds the placeholder `<review metadata sha>`. The response file asks the reviewer to correct it.
- The next ids are D-592, OQ-183, F-60, L-16, G-27, PR-82, M-7, and Session 53.

### Open questions that block progress

None for PR #16. OQ-182 blocks nothing. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

The author gets a current Gitar review of the new head and answers each finding. Then the Codex reviewer repeats the review of P2-1.

## Session 51: 2026-09-16, Codex

Author: Codex
Session: review of PR #16 at effective head `45e960d`.
Repository: the-thing-below. Branch: `docs/pr-16-context-budget`. PR: #16. Role: reviewer. Base: `2bc7d56`.

### What this session did, and why

- Read the start set, the one-PR skill, the PR review skill, the STE skill, and the review record and commit references.
- Verified that Claude Code authored the PR from Session 50. Codex is the eligible reviewer under T-4 and D-17.
- Recomputed the effective head as `45e960d`. It is the only commit after the base and changes substantive paths.
- Inspected all 18 paths in the diff, the decisions D-583 to D-591, F-59, and OQ-182.
- Found P2-1: the PR comment export can return success after an earlier GitHub retrieval fails. A shell reproduction returned 0 after a failed command and a successful command.
- Verified the Gitar dashboard approval, its check, and the absence of review threads. Corrected the stale dashboard timestamp in the PR description.
- The full interim STE check passes with 0 findings. `git diff --check` passes, and `AGENTS.md` and `CLAUDE.md` are identical.
- The handoff held ten entries before this one, so Session 41 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code, solution, or Makefile exists. `main` is `2bc7d56` (PR #15).
- PR #16 is open on `docs/pr-16-context-budget`. Its remote head and effective head are `45e960d` before this review commit.
- Gitar approved this head. The cross-provider review found P2-1, recorded in `docs/reviews/pr-16.md`.
- `make verify` is unavailable because the Makefile does not exist. The direct STE and diff checks pass.

### In flight

PR #16 needs the author to fix P2-1 and run the regression check. The review record and this handoff entry are ready to commit and push.

### Traps and gotchas

- The comments export must fail if any API request fails. Otherwise a partial file can appear complete.
- The PR description timestamp now matches the Gitar dashboard update at 21:23:53Z.
- The review applies to effective head `45e960d`, not the metadata tip that will publish this record.
- The next ids are D-592, OQ-183, F-60, L-16, G-27, PR-82, M-7, and Session 52.

### Open questions that block progress

None for PR #16. OQ-182 blocks nothing. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

The author makes each failed GitHub retrieval fail the comment-export command, tests that a partial export returns nonzero, and requests a repeat Codex review after the fix.

## Session 50: 2026-09-16, Claude Code

Author: Claude Code
Session: the token audit of the repository, then PR #16, the session context budget.
Repository: the-thing-below. Branch: `docs/pr-16-context-budget`. PR: #16. Role: author. Base: `2bc7d56` (PR #15).

### What this session did, and why

- The owner asked for a read-only audit of token use. The audit read the usage records of 10 Claude Code sessions and 10 Codex sessions of this repository, and the size of every instruction file.
- Each harness call sends the whole context again. The Claude Code sessions sent a median of 350k tokens in each call, and a maximum of 886k, with no context compaction. Whole reads of the read order held about 29% of the carried context. A call for each STE check cost 19% of the input tokens, and a call for each GitHub poll cost 14%. `CLAUDE.md` cost about 1.5%.
- The owner said "Implement all fixes as recommended." D-583 to D-591 record the answers. OQ-182 asks where a size check goes. F-59 records the finding.
- The start set replaces the whole read order at the start (D-583, D-584). The STE check runs in the commit command (D-585). One command waits for Gitar (D-586). The session tells the owner when it is ready for a context compaction (D-587).
- `pr-review` split into a core of 17,922 bytes and five reference files (D-588, D-589). The glossary of the project areas moved word for word to `ste-writing/references/glossary.md` (D-590). D-591 covers scripts and edits.
- `docs/runbooks/session-context.md` holds the evidence and the commands. Each command ran on this machine, in zsh.
- The auto mode classifier of the harness refused the edits of the session skill and of `CLAUDE.md` two times. The owner then approved the edits in the conversation.
- A separate evaluator ran the changed rules on four requests and found five defects. All five had merit, and this PR fixes them: a handoff push during the Gitar wait, the lost full STE check, unset variables in the comments command, the provider gate against D-584, and the wait after "On it".
- The shared `gitar-review` skill did not change, because it is the same file in each repo.
- The handoff held ten entries before this one, so Session 40 moved word for word to the top of `docs/session-handoff-archive.md` (D-18). A repeated rule line between two entries left this file.

### State of the build

- No code, solution, or Makefile exists. `main` is `2bc7d56` (PR #15).
- PR #16 is open on branch `docs/pr-16-context-budget`. The commit that holds this entry is its effective head.
- The full interim STE check gives 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- The start set fell from 55,786 bytes to 28,491 bytes, about 20.7k to 10.6k tokens at 2.7 bytes for each token. `CLAUDE.md` grew from 15,001 to 16,052 bytes.

### In flight

PR #16 waits for a Gitar pass and for the Codex review, because it adds decision rows (T-4, D-17, D-401). For this work the owner told the session to set aside the Gitar procedure, so no Gitar request ran.

### Traps and gotchas

- The shell of this machine is zsh. zsh does not split `$files` into words, so the commit command pipes the file list to `xargs`.
- The harness gives the compaction command to the owner alone. A session cannot compact itself, so D-587 tells the owner.
- A push while Gitar reviews makes the review stale. Commit the handoff entry of a round before the push of that round.
- The checker does not read the glossary. A session that writes about a project area loads `references/glossary.md` (D-590).
- The audit scripts lived in the scratch folder of the session. No tool of this repository reads the usage records (D-99).
- The next ids are D-592, OQ-183, F-60, L-16, G-27, PR-82, M-7, and Session 51.

### Open questions that block progress

None for PR #16. OQ-182 blocks nothing. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

The author gets a current Gitar review of the PR #16 head and answers each finding. Then the Codex review of PR #16 runs.

## Session 49: 2026-09-16, Codex

Author: Codex
Session: repeat cross-provider review of PR #14 at effective head 7e65c7e.
Repository: the-thing-below. Branch: docs/pr-14-one-pr-one-session. PR: #14. Role: reviewer. Base: 26152c5.

### What this session did, and why

- Read the handoff first, then the one-PR, review, STE, and gitar skills. Read the prior review, its response, the complete correction diff, the affected contracts, the PR description, and all PR comments.
- Verified the provider gate under T-4 and D-17. Session 46 identifies Claude Code as the author. Session 48 identifies Claude Code as the correction author. Codex is the eligible reviewer.
- Recomputed the effective head. 7e65c7e changes substantive paths. The review and handoff commits change only metadata paths.
- Reproduced P2-1 and checked its correction. The gate rejects a deferral of this PR's own documents or records, and it permits a line that names PR-3 as the owner of independent roadmap work. D-579, the PR template, and PR-3 exit tests 10 and 11 agree.
- Checked the D-582 session-end changes and the author and reviewer instructions. They agree with the revised decision.
- Verified the latest manual Gitar review after the correction push. Its dashboard reports approval, its check passes, and no review thread or formal PR review remains.
- Updated docs/reviews/pr-14.md, kept the earlier Changes required verdict under Earlier verdicts, and set the current verdict to Ready for owner merge for 7e65c7e.
- Corrected the PR description's other-provider checkbox after recording the verified verdict.
- The handoff held ten entries before this one, so Session 39 moved word for word to the top of docs/session-handoff-archive.md (D-18).

### State of the build

- No code, solution, or Makefile exists. main is 26152c5 (PR #13).
- PR #14 is open on docs/pr-14-one-pr-one-session. Its remote tip before this review is 7e65c7e, and its effective head is 7e65c7e.
- The interim STE check passes with 0 findings, git diff --check passes, and CLAUDE.md and AGENTS.md stay identical.
- Build and test commands did not run because the repository has no code, solution, or Makefile.

### In flight

The cross-provider review is ready for owner merge at effective head 7e65c7e (T-4, D-17). The PR still needs a current Gitar review of its latest metadata tip before merge. The last Gitar dashboard edit predates the review publication, and GitHub reports no check on the new tip.

### Traps and gotchas

- Completion line 8 now applies only to documents and records of the current PR. A line that assigns independent roadmap work to its owner PR passes under D-579 and G-16.
- P2-1 stays in the review history as fixed. The earlier verdict remains under Earlier verdicts.
- The next ids are D-583, OQ-182, F-59, L-16, G-27, PR-82, M-7, and Session 50.

### Open questions that block progress

None for PR #14. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

The author requests a current Gitar review of the latest PR tip and answers each finding. P2-1 is fixed; the Gitar review is the remaining merge gate.

## Session 48: 2026-09-16, Claude Code

Author: Claude Code
Session: the answer to the review of PR #14, on branch `docs/pr-14-one-pr-one-session`. Role: author, in the same conversation as Session 46 (D-582). Base: `26152c5`.

### What this session did, and why

- The owner asked the session to answer the review of Session 47. The skill of this PR blocked the answer, because the author session had reached its hand-over point. The owner said: "Addressing Codex/gitar review feedback does NOT qualify for a new session."
- D-582 records that rule and revises D-576 in part. The author session now answers each gitar comment and each review, and it ends at `Ready for owner merge` or the label.
- P2-1 has full merit. Completion line 8 rejected any line that names another PR, so it rejected PR #14 and the PR template. The gate now rejects only a document or a record of this PR that waits for another PR.
- The same boundary reaches `CLAUDE.md`, `AGENTS.md`, the PR template, D-579, and the PR-3 scope. PR-3 gains exit test 11, which passes a PR that names the PR of an absent check.
- `pr-review`, the glossary, and `docs/design.md` follow D-582.
- `docs/reviews/pr-14-response.md` records both answers.
- A separate evaluator ran the corrected skill on seven requests, the two fixtures of the review included. The deferral failed, and the PR that names PR-3 passed. Its notes found a clash that the first correction made: the reviewer could repeat a review, yet it ended at its first record. The reviewer now ends at the same verdict as the author. A correction author comes from the provider of the author (T-4).
- The handoff held ten entries before this one, so Session 38 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `26152c5` (PR #13).
- PR #14 is open on branch `docs/pr-14-one-pr-one-session`. The commit that holds this entry changes paths outside the metadata set, so it is the new effective head.
- The interim STE check passes with 0 findings, the skill validator passes, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- Size: `CLAUDE.md` is 15,004 bytes, and the skill is 8588 bytes.

### In flight

PR #14 waits for a current gitar review of the new head, then the repeat Codex review of P2-1 (T-4, D-17). This author session stays bound to PR #14 and answers each finding (D-582).

### Traps and gotchas

- A line that names the PR of independent roadmap work is not a deferral (G-16). Only a document or a record of the current PR can defer.
- D-579 changed its text inside this PR, before any merge. The response file says why.
- The author session and the reviewer session each end at `Ready for owner merge` for the effective head, or at the label, not at the request for a review (D-582). Each round of a session adds a new handoff entry.
- The next ids are D-583, OQ-182, F-59, L-16, G-27, PR-82, M-7, and Session 49.

### Open questions that block progress

None for PR #14. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

Get a current gitar review of the new head and answer each finding. Then a Codex session repeats the review of P2-1 from `docs/reviews/pr-14-response.md`.

## Session 47: 2026-09-16, Codex

Author: Codex
Session: review of PR #14 at effective head `9837c1d`, on branch `docs/pr-14-one-pr-one-session`. Role: reviewer. Base: `26152c5`.

### What this session did, and why

- Read the handoff first, then the review, session, STE, and gitar skills. Read the complete diff, the affected contracts, the PR description, and every PR comment.
- Verified the provider gate under T-4 and D-17. Session 46 identifies Claude Code as the author of the substantive change.
- Recomputed the effective head. `9837c1d` changes the substantive paths. The later commit `6bafb15` changes the handoff alone.
- Applied the new skill to this PR and the standard PR template. Found P2-1: completion line 8 rejects any line that gives work to another PR.
- Verified the trigger against D-579 and G-16. PR #14 assigns the machine enforcement to PR-3, and the template names each PR that creates an absent check.
- Inspected the external GitHub claim. The official events page lists the `edited` type for `pull_request_target`.
- Verified the current gitar pass. The dashboard edit follows the second request, the check succeeded, and no review thread exists.
- Wrote `docs/reviews/pr-14.md` with the verdict `Changes required` for effective head `9837c1d`.
- The handoff held ten entries before this one, so Session 37 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `26152c5` (PR #13).
- PR #14 is open on branch `docs/pr-14-one-pr-one-session`. Its effective head is `9837c1d`.
- The interim STE check passes with 0 findings. Both diff checks are clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- D-1 through D-581 and OQ-1 through OQ-181 are gap-free.

### In flight

PR #14 needs the P2-1 correction. The review record applies to effective head `9837c1d` (T-4, D-17).

### Traps and gotchas

- D-577 forbids a later PR from carrying a document of the current PR. It does not forbid a roadmap from assigning independent work to its owner PR.
- D-579 assigns the machine enforcement to PR-3. G-16 requires each absent check to name its creator PR.
- The correction must keep the negative deferral case and add the valid future-owner case. A word search for `later PR` cannot decide the meaning alone.
- The next ids are D-582, OQ-182, F-59, L-16, G-27, PR-82, M-7, and Session 48.

### Open questions that block progress

None for P2-1. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

The author limits completion line 8 to the documents and records of the current PR. The response adds both regression fixtures, and a new clean Codex session repeats the review.

## Session 46: 2026-09-16, Claude Code

Author: Claude Code
Session: PR #14, the rule of one PR in one session, on branch `docs/pr-14-one-pr-one-session`. Role: author. Base: `26152c5`.

### What this session did, and why

- The owner asked for one clean session for each PR, and for each PR as the complete unit of its work, to keep the context of each session small. The same instruction went to three other repositories of the owner. Each of those repositories gets its own session and its own PR.
- The harness of this session had no tool to start a top-level session in another project. So this session wrote this repository alone, and it gave the owner the prompts for the other three.
- The new skill `.claude/skills/one-pr-one-session/SKILL.md` holds the session binding, the start gate, the documents gate, the merge facts, the completion gate, and an enforcement table.
- `CLAUDE.md` and `AGENTS.md` require the skill before any PR work. The D-18 line "A documentation PR can follow the merge" is gone. The PR gate line on documents points at the skill.
- The owner answered three questions: the document rules go into PR-3 (D-579), a docs PR with its own concern stays allowed (D-580), and the Documents lines use three STE forms (D-581). D-576 to D-578 record the owner instruction. D-18 and D-15 carry `Revised in part` notes.
- `docs/design.md` gains the session pass line, F-58, and G-26. The PR-3 line of Phase 1 and the sequence position change.
- The PR-3 entry of `phase-1-foundations.md` gains scope lines, exit tests 8 to 11, and review focus lines. `area-tools.md` and `area-ci.md` follow, and `area-tools.md` gains a dated fact on the `edited` type of `pull_request_target`.
- `pr-review` loads the new skill, checks the Documents section, and ends the session after the end gate. The glossary gains "clean session", "Documents section", and "hand-over point". The PR template gains the rows and a no-deferral line.
- A forward test with a separate evaluator ran the skill on ten realistic requests. Each of the ten gave the result that the rule needs. Its notes added a refusal result for a PR that records an earlier PR, the gates of a reviewer, a meaning of substantive work, a rule for a second concern, and the form of each line. It also found a gap in the metadata set of `pr-review`, which OQ-181 holds.
- The handoff held ten entries before this one, so Session 36 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `26152c5` (PR #13).
- PR #14 is open on branch `docs/pr-14-one-pr-one-session`. Its effective head is `9837c1d`. The later commit that records the gitar pass changes this file alone, so it is a metadata commit.
- The gitar pass on `9837c1d` approved with no comment: 0 comments, 0 with merit, and no thread. The dashboard edit came after the push, and its summary names this change.
- The interim STE check passes with 0 findings, the skill validator passes, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- Size: `CLAUDE.md` grows from 14,735 to 14,976 bytes. The skill is 7,550 bytes, and a session loads it for PR work alone.

### In flight

PR #14 waits for the Codex review at effective head `9837c1d`. It changes rows of `docs/decisions.md`, so it takes the review and not the label (D-401). A manual gitar review of the metadata commit confirms that the pass is current. This session ends at that hand-over point (D-576).

### Traps and gotchas

- No check can see the conversation of a session (F-58). The binding and the clean start stay agent-enforced and owner-enforced. PR-3 enforces the document rules from its merge on (D-579).
- The handoff says "open" for a PR that the owner merges later. Git holds the merge. Do not open a PR to correct it (D-578).
- A PR-3 session must keep the rows of the skill table and the rows of the command in step. Exit tests 8 to 11 of PR-3 name them.
- The next ids are D-582, OQ-182, F-59, L-16, G-27, PR-82, M-7, and Session 47.

### Open questions that block progress

None for PR #14. OQ-179 blocks PR-5, OQ-180 blocks PR-81, and OQ-181 blocks PR-3.

### Next concrete action

A new clean Codex session reviews PR #14 at its effective head under the `pr-review` skill (T-4, D-17). Then the owner merges.

## Session 45: 2026-09-16, Claude Code

Author: Claude Code
Session: PR #13, the link from the guidance to the new `gitar-review` skill, on branch `docs/pr-13-gitar-review`.

### What this session did, and why

- The owner added the shared skill `.claude/skills/gitar-review/SKILL.md` and asked that `pr-review` and the other guidance link to it, with no copy of its procedure and no wrong text.
- The skill adds the proof that a review is current. The older text in `CLAUDE.md` and `pr-review` asked for `Gitar review` only on a pause, and it did not check that the review covers the head.
- `CLAUDE.md` and `AGENTS.md`: the skill list names `gitar-review`. The section "Automated review pass" points to the skill and keeps only the rules of this repo. The PR gate line asks for a current review.
- `pr-review`: the section "The automated pass" points to the skill and keeps the rules of this repo. The reviewer checks with the read commands of the skill that the pass is current.
- `.github/pull_request_template.md`: the gitar line asks for a current review.
- The commit adds the skill file as the owner wrote it. It passes the STE check with 0 findings.
- The owner answered two questions. No decision row records the skill, so `docs/decisions.md` stays as it is. The repo keeps the spelling `gitar`, and the shared skill keeps `Gitar`.
- The handoff held ten entries before this one, so Session 35 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `b292624` (PR #12).
- PR #13 is open on branch `docs/pr-13-gitar-review`. Its head is the commit that holds this entry.
- The interim STE check passes with 0 findings, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #13 waits for a current gitar review under the `gitar-review` skill. It changes paths in the override set alone and no decision row, so the session applies the `review-override` label after the pass approves the head (D-16, D-67, D-401).

### Traps and gotchas

- The `gitar-review` skill is the same file in each repo. Do not edit it here. Put a rule of this repo in `CLAUDE.md` or `pr-review`.
- A rule of this repo wins over the skill. Step 20 of the skill tells the owner that the PR is ready to merge. Here the PR goes to the other provider, or it takes the label.
- The D-14 row still names the pause as the trigger for `Gitar review`. The owner chose no decision row, so the skill carries the wider trigger.
- The dated records keep the old procedure text. Do not correct them.
- The next ids are D-576, OQ-181, F-58, L-16, G-26, PR-82, M-7, and Session 46.

### Open questions that block progress

None for PR #13. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### Next concrete action

Get a current gitar review of PR #13 and answer each finding under the `gitar-review` skill. Apply the `review-override` label after the pass approves the head. Then the owner merges.

## Session 44: 2026-09-16, Codex

Author: Codex
Session: third repeat cross-provider review of PR #12 at effective head `ff04f87`.

### What this session did, and why

- Read the current handoff first, then the review response, the review and STE skills, the correction diff, the affected contracts, and the PR comments.
- Verified the provider gate under T-4 and D-17. Session 43 identifies Claude Code as the author of the substantive P2-7 correction.
- Recomputed the effective head. `ff04f87` is the newest substantive commit. The later commits `54edbe5` and `ef5a8b0` change only review and handoff metadata.
- Reproduced P2-7 and its regression check. The art and effects ownership rows now name PR-81 and D-575, and the PR-81 budget test now covers each map and each battle place under D-523.
- Checked the adjacent group scopes. The PR-35 and PR-17 exclusions now include PR-81. The remaining narrower references either record historical text or give PR-81 its own row.
- Verified the automated finding and its correction. The final pass on `ef5a8b0` confirms the four-file count, and every review thread is resolved.
- Updated `docs/reviews/pr-12.md`, preserved the three earlier verdicts, and set the current verdict to `Ready for owner merge` for `ff04f87`.
- Corrected the stale other-provider checkbox in the PR description after the verdict became current.
- The handoff held ten entries before this one, so Session 34 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. The target tip of `main` is `6910017`, and the PR merge base is `c4d39fe`.
- PR #12 is open on branch `docs/pr-12-critic`. Before this review commit, its remote tip is `ef5a8b0`, and its effective head is `ff04f87`.
- The interim STE check passes with 0 findings, both diff checks are clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- GitHub reports no check run. The automated pass reports approval with three closed findings and no open finding.

### In flight

PR #12 is ready for owner merge. The review record applies to effective head `ff04f87` (T-4, D-17).

### Traps and gotchas

- The verdict covers the effective head `ff04f87`, not the later metadata tip.
- D-523 applies the effect budget to each map and each battle place. The boss of a one-map dungeon still creates a battle place.
- A new PR that joins a named group must join its ownership tables and its scope limits.
- OQ-180 blocks PR-81, not PR #12.
- The next ids are D-576, OQ-181, F-58, L-16, G-26, PR-82, M-7, and Session 45.

### Open questions that block progress

None for PR #12. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### Next concrete action

The owner can merge PR #12.

## Session 43: 2026-09-16, Claude Code

Author: Claude Code
Session: the answer to the second repeat review of PR #12, on branch `docs/pr-12-critic`.

### What this session did, and why

- The session read the second repeat review of Session 42. It set P2-6 to `fixed in 0fbfa82`, and it added P2-7 with the verdict `Changes required` for head `0fbfa82`.
- P2-7 has full merit. The art and effects tables gave the later places to PR-23 to PR-27 alone, and the budget exit test of PR-81 left out its battle place, which the boss of D-575 needs.
- The art row and the effects row now name PR-81 and cite D-575. The budget test of PR-81 now uses the boundary of the other dungeon builds: each map and each battle place.
- A scan for the same class found two more lines in Phase 2, the scope limits of PR-35 and PR-17. The sealed door is a story gate on the region map, so both lines now name PR-81.
- `docs/reviews/pr-12-response.md` gained the answer of this round.
- The handoff held ten entries before this one, so Session 33 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `c4d39fe` (PR #11).
- PR #12 is open on branch `docs/pr-12-critic`. Its remote head is the commit that holds this entry.
- The correction changes four roadmap files outside the metadata set, so the effective head moves off `0fbfa82` to the commit of this answer.
- The automated pass on `ff04f87` reported `Approved with suggestions`, with one finding. It had merit: the response and this entry said three roadmap files, and the commit changed four. The same count was wrong for the first round too, and a follow-up commit corrected all three lines.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #12 waits for the repeat cross-provider review of P2-7 at the effective head `ff04f87` (T-4, D-17). The automated pass is complete, and no comment of it waits for an answer (D-14, D-66).

### Traps and gotchas

- A new PR that joins a group of PRs must join every table and every scope limit that names the group. PR-81 joined the sequence first and the ownership tables later.
- The budget test of a dungeon covers each map and each battle place (D-523).
- The next ids are D-576, OQ-181, F-58, L-16, G-26, PR-82, M-7, and Session 44.

### Open questions that block progress

None for PR #12. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### Next concrete action

A Codex session repeats the review of PR #12 at the effective head `ff04f87`. It reads the P2-7 section of `docs/reviews/pr-12-response.md`, checks the trigger and the regression check, and writes the verdict (T-4, D-17).

## Session 42: 2026-09-16, Codex

Author: Codex
Session: second repeat cross-provider review of PR #12 at effective head `0fbfa82`.

### What this session did, and why

- Read the current handoff first, then the review response, the review and STE skills, the correction diff, the changed contracts, and the PR comments.
- Verified the provider gate under T-4 and D-17. Session 41 identifies Claude Code as the author of the D-575 correction.
- Recomputed the effective head. `0fbfa82` is the newest substantive commit, and `74f9477` changes only the handoff metadata.
- Reproduced P2-6 and its regression check. D-575 fixes the dungeon count and classification in the decisions, design, roadmaps, and world files.
- Found one new adjacent contract defect, P2-7. The PR-81 budget test omits its boss battle place, and the art and effects tables omit PR-81.
- Updated `docs/reviews/pr-12.md`, preserved both earlier verdicts, and set the current verdict to `Changes required` for `0fbfa82`.
- Corrected the stale PR title, the automated-pass checkbox, and the D-# and OQ-# ranges in the PR description.
- The handoff held ten entries before this one, so Session 32 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. The target tip of `main` is `6910017`, and the PR merge base is `c4d39fe`.
- PR #12 is open on branch `docs/pr-12-critic`. Before this review commit, its remote tip is `74f9477`, and its effective head is `0fbfa82`.
- The interim STE check passes with 0 findings, both diff checks are clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- GitHub reports no check run. The automated pass reports approval with two closed findings and no open finding.

### In flight

PR #12 needs the P2-7 roadmap correction, then another repeat cross-provider review at the new effective head.

### Traps and gotchas

- A full dungeon contract reaches the phase file and each affected area ownership table.
- D-523 requires the effect-budget test for every map and battle place. A boss adds a battle place even when the place has one map.
- P2-6 stays fixed. The next correction must not reopen the five-dungeon count or the order of play.
- OQ-180 blocks PR-81, not PR #12. D-487 permits a future PR question in the roadmap.
- The next ids are D-576, OQ-181, F-58, L-16, G-26, PR-82, M-7, and Session 43.

### Open questions that block progress

None. P2-7 needs no owner decision. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### Next concrete action

The author adds PR-81 to the later-place rows of `area-art.md` and `area-effects.md`. The author also adds its battle place to the PR-81 budget exit test, updates the response file, and requests another review.

## Session 41: 2026-09-16, Claude Code

Author: Claude Code
Session: the answer to the repeat review of PR #12, on branch `docs/pr-12-critic`.

### What this session did, and why

- The session read the repeat review of Session 40. It set P1-1 and P2-1 to P2-5 to `fixed in d31ae6b`, and it added P2-6 with the verdict `Changes required` for head `d31ae6b`.
- The automated pass also closed its one finding and reported `Approved`.
- P2-6 has full merit. D-56, D-244, and D-313 counted four dungeons, and D-562 called the sealed gallery "a dungeon map" with no revision of those rows.
- The owner classified the gallery as the fifth dungeon, with the full dungeon contract (D-575). The session recommended a passage that only uses the dungeon-map format, and the owner chose a dungeon.
- The session applied D-575: marks on D-56, D-244, D-313, D-562, and D-564, the dungeon contract in the PR-81 entry, and the count of five in the design doc, two area files, Phase 3, `docs/world/places.md`, and `docs/world/arc.md`.
- A scan for the old count found three more rows, D-327, D-346, and D-369, and each now names D-575.
- A fifth dungeon needs a boss, and no document named one, so OQ-180 holds that question for PR-81 (D-487).
- `docs/reviews/pr-12-response.md` gained the answer of this round.
- The handoff held ten entries before this one, so Session 31 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `c4d39fe` (PR #11).
- PR #12 is open on branch `docs/pr-12-critic`. Its remote head is the commit that holds this entry.
- The correction changes files outside the metadata set, so the effective head moves off `d31ae6b` to the commit of this answer.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- The ids stay gap-free: D-1 to D-575, and OQ-1 to OQ-180.

### In flight

PR #12 waits for the repeat cross-provider review of P2-6 (T-4, D-17).

- The automated pass on `6a43f1a` reported `Approved with suggestions`, with one new finding. It had merit: the plain-English paragraph of Phase 4 in `docs/design.md` still said "Four more dungeons". The session corrected it to five dungeon builds, and a scan for other forms of the count found no other current contract (D-14, D-66).
- The correction is `0fbfa82`, the effective head. The pass on it reported `Approved`, with both of its findings closed and none open, so no comment waits for an answer.

### Traps and gotchas

- Region one has two hubs and five dungeons (D-575). The hanging cells count once, although the party visits them twice (D-327).
- The order of play is the hanging cells, the deep mine, the second visit to the cells, the sealed gallery, the refuge, the mining town by night, the border fort, and the ice crossing (D-313, D-574, D-575).
- The target of six to eight hours of D-56 stands with five dungeons. M-5 measures it, and a miss changes content in a PR of its own.
- A revised count reaches more rows than the rows that set it. A scan for the old words reads the Effect column of every decision too.
- A reviewer session can archive a handoff entry too. Before a session archives the oldest entry, it reads the handoff and takes the oldest session that the handoff still holds.
- The next ids are D-576, OQ-181, F-58, L-16, G-26, PR-82, M-7, and Session 42.

### Open questions that block progress

None for PR #12. OQ-179 blocks PR-5, and OQ-180 blocks PR-81.

### Next concrete action

A Codex session repeats the review of PR #12 at the effective head `0fbfa82`. It reads the P2-6 section of `docs/reviews/pr-12-response.md`, checks the trigger and the regression check, and writes the verdict (T-4, D-17). The automated pass is complete.

## Session 40: 2026-09-16, Codex

Author: Codex
Session: repeat cross-provider review of PR #12 at effective head `d31ae6b`.

### What this session did, and why

- Read the current handoff first, then the review response, the review skill, the STE skill, the correction diff, the original triggers, the adjacent contracts, and the PR comments.
- Verified the provider gate under T-4 and D-17. Session 39 identifies the other provider as the author of the substantive corrections.
- Recomputed the effective head. `d31ae6b` is the newest substantive commit. The later commits `aa3c5e2` and `c8e7e48` change only the review and handoff metadata paths.
- Verified P1-1 and P2-1 through P2-5 against their original triggers and regression checks. Each earlier finding is fixed in `d31ae6b`. The archive response correction in `aa3c5e2` also matches the committed archive.
- Checked the added PR-27 move. D-574, the design sequences, the Phase 4 sequence, the exploration roadmap, the places file, and the arc agree that PR-27 follows PR-81 and precedes PR-25.
- Found one new adjacent contract defect, P2-6. D-56, D-244, and D-313 define four dungeons. D-562 and the places file classify the sealed gallery as another dungeon without a revision mark.
- Updated `docs/reviews/pr-12.md`, preserved the earlier verdict, and set the current verdict to `Changes required` for `d31ae6b`.
- Corrected two stale PR facts. The title and the description now end the decision range at D-574.
- The handoff held ten entries before this one, so Session 30 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. The current target tip of `main` is `6910017`, and the PR merge base is `c4d39fe`.
- PR #12 is open on `docs/pr-12-critic`. Before this review commit, its remote tip is `c8e7e48`, and its effective head is `d31ae6b`.
- The interim STE check passes with 0 findings, both diff checks are clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- GitHub reports no check run on the branch. The repeat automated pass has one fixed and resolved finding and no open finding.

### In flight

PR #12 needs the owner classification and correction in P2-6, then another repeat cross-provider review at the new effective head.

### Traps and gotchas

- A dungeon count and a map implementation kind are separate only when a current decision says so. D-562 now calls the gallery a dungeon map, and the places table calls it a dungeon.
- If the sealed gallery is a fifth dungeon, the correction must revise D-56, D-244, and D-313. If it is a route, D-562 and its consumers must state that it only uses the dungeon-map format.
- The six earlier findings stay fixed. A response to P2-6 must not reopen their corrected contracts.
- The review verdict covers `d31ae6b`, not the metadata tip.
- The next ids are D-575, OQ-180, F-58, L-16, G-26, PR-82, M-7, and Session 41.

### Open questions that block progress

The owner must classify the sealed gallery as a fifth dungeon or as a route that uses the dungeon-map format. OQ-179 continues to block PR-5.

### Next concrete action

The author asks the owner to classify the sealed gallery, records the answer, corrects P2-6 and its adjacent consumers, and updates `docs/reviews/pr-12-response.md`. Then a Codex session repeats the review at the new effective head.

## Session 39: 2026-09-16, Claude Code

Author: Claude Code
Session: the answer to the review of PR #12, and the place of PR-27, on branch `docs/pr-12-critic`.

### What this session did, and why

- The session read `docs/reviews/pr-12.md`, the `Changes required` verdict of Session 38 for head `84b4128`. The owner also asked the session to examine the order of PR-27. The session assessed each finding against the evidence before it changed a file (the `pr-review` skill).
- Five findings have full merit, and one has partial merit. `docs/reviews/pr-12-response.md` holds each disposition, its evidence, and its regression check.
- P1-1: the rule of D-568, "no blur and no uneven pixels", cannot hold at a scale of 1.5. The session had written that absolute, and the owner had said "It won't be perfect pixel scaling, but it MUST look good." The owner chose even pixel sizes with a slight softness (D-573), which resolves OQ-105.
- P2-1: the scan of Session 37 left out the decision register and skipped each line that already named D-568, so it passed stale frame contracts. A corrected scan found the four that the review listed and five more, and each now names D-568.
- P2-2 and P2-3: OQ-114, OQ-135, the PR-30 exit test, Gate 4, and OQ-93 now match D-566, D-555, D-571, and D-572. D-42 gained its mark for D-555.
- P2-4 has partial merit. The PR description and a new dated line named the design-critic agent as the source of work, and both now name the pass of D-484. The name of the pass stays, because D-484 names it as a plan item.
- P2-5: the archive step of Sessions 35 and 37 inserted a session above a named older session, not at the top. The archive now runs from Session 29 down to Session 1, with no text changed, and this session inserted Session 29 at the top.
- The place of PR-27: the second hub is the refuge of the old faith (D-243, `docs/world/arc.md`). The party reaches it right after the sealed gallery (D-331), and it serves the fort and the ice (D-356). The owner moved PR-27 right after PR-81 (D-574).
- The handoff held ten entries before this one, so Session 29 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `c4d39fe` (PR #11).
- PR #12 is open on branch `docs/pr-12-critic`. Its remote head is the commit that holds this entry.
- The corrections change files outside the metadata set, so the effective head moves off `84b4128` to the commit of this answer. The repeat review reads that head.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- The phase lists of section 7, the sequence of section 8, and the five phase sequences give the same 79 PR items.

### In flight

PR #12 waits for the repeat cross-provider review (T-4, D-17).

- The effective head is `d31ae6b`. The commits above it change `docs/reviews/` and `docs/session-handoff.md` alone.
- The automated pass on `d31ae6b` reported `Approved with suggestions`, with one comment. That comment had merit: the response gave the archive range as Session 28 down to Session 1, and the same commit had archived Session 29 at the top. `aa3c5e2` corrected both lines, and the thread is resolved (D-14, D-66).
- An on-demand pass writes no check run, so the evidence of the pass is its review comment.

### Traps and gotchas

- A scan for a superseded rule must read `docs/decisions.md` too, and it must show each hit with its context. A filter that skips a line because it names the new decision passes a line that names both.
- The size 1280 by 800 stays true in three places: the Deck screen, the rule of Valve for text at 1280 by 800, and the display list of the fit test of PR-61. Each is a fact about the Deck, not about the frame.
- The archive step inserts a moved session at the top of the archive, above the newest archived session.
- An owner statement that accepts a tradeoff stays a tradeoff in the decision row. Never turn "must look good" into an absolute rule.
- The next ids are D-575, OQ-180, F-58, L-16, G-26, PR-82, M-7, and Session 40.

### Open questions that block progress

None for PR #12. OQ-179 blocks PR-5.

### Next concrete action

A Codex session repeats the review of PR #12 at the effective head `d31ae6b`. It reads `docs/reviews/pr-12-response.md`, checks each trigger and regression check, and writes the verdict (T-4, D-17). The automated pass is complete, and no comment of it waits for an answer.

## Session 38: 2026-09-16, Codex

Author: Codex
Session: cross-provider review of PR #12 at effective head `84b4128`.

### What this session did, and why

- Read the current handoff first, then the review skill, the STE skill, the complete PR diff, the changed contracts, the PR description, and the PR comments.
- Verified the provider gate under T-4 and D-17. Session 37 identifies the other provider as the author of the substantive changes.
- Recomputed the effective head. `84b4128` is the newest substantive commit. The four commits above it change only `docs/session-handoff.md` and `docs/session-handoff-archive.md`.
- Found six blockers. D-568 requires a 1.5 scale with neither blur nor uneven pixels, which no raster filter can supply. Current consumers retain the old frame, fog, save-point, and M-4 band rules. OQ-93 assigns Deck test instrumentation to a story scene. The PR description attributes the critic work to an agent. Session 27 is out of order in the handoff archive.
- Added `docs/reviews/pr-12.md` with the verdict `Changes required` for `84b4128`.
- Verified the PR order and id coverage, the roadmap list numbers, the guidance identity, the interim STE check, and `git diff --check`. The automated pass has one approval comment and no inline thread.
- The handoff held ten entries before this one, so Session 28 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. The current target tip of `main` is `6910017`, and the PR merge base is `c4d39fe`.
- PR #12 is open on `docs/pr-12-critic`. Before the review commit, its remote tip is `d4330d1`, and its effective head is `84b4128`.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- GitHub reports no check run on the branch. The automated-pass issue comment reports `Approved` and no issue.

### In flight

PR #12 needs the six corrections in `docs/reviews/pr-12.md`, then a repeat cross-provider review at the new effective head.

### Traps and gotchas

- A 1280 by 720 image cannot scale to 1920 by 1080 with both equal source-pixel widths and no sample mixing. OQ-105 needs an owner choice that relaxes one property.
- D-568 supersedes D-480. Current implementation contracts must use the one 1280 by 720 frame, not only add a citation.
- D-555, D-566, and D-571 also need propagation into the questions and exit tests that implement them.
- The term correction in OQ-93 is `test scene`, not `story scene`.
- A PR description cannot credit an agent as the source of work, even when the named agent is a configured repository tool (T-6, D-22).
- D-18 requires every archive block to stay newest first. Session 27 is below Sessions 26 and 24 and needs a block-only move.
- The review verdict covers `84b4128`, not the metadata tip.
- The next ids are D-573, OQ-180, F-58, L-16, G-26, PR-82, M-7, and Session 39.

### Open questions that block progress

OQ-105 blocks PR-61 and needs an owner answer that resolves P1-1. OQ-179 continues to block PR-5.

### Next concrete action

The author assesses each finding, asks the owner to settle the 1080p tradeoff in OQ-105, corrects the findings with their adjacent consumers, and writes `docs/reviews/pr-12-response.md`. Then a Codex session repeats the review at the new effective head.

## Session 37: 2026-09-16, Claude Code

Author: Claude Code
Session: the design-critic pass of PR #12 on the merged plan, the owner answers, and the fixes, on branch `docs/pr-12-critic`.

### What this session did, and why

- The owner merged PR #11 as `c4d39fe` on 2026-09-16. The session synced `main` and started PR #12, the design-critic pass of D-484 and D-490.
- The merged plan holds about 5,800 lines of roadmap files and 554 decisions, so the session ran the `design-critic` agent in five slices at once: the foundations, the frame and the effects, the play systems, the story and the release, and the registers.
- The five reports held 52 defects. The session checked each high-impact claim against the files before it acted, and F-57 records the pass.
- The owner answered 18 questions in five batches (D-555 to D-572). Four answers went against the recommendation, and two were answers that no option offered:
  - Nothing resets when the party leaves a dungeon, until a story event (D-555). That closes a loop that refilled every resource for one walk.
  - PR-80 holds the enemy record as a PR of its own (D-557).
  - The game has no fog of war (D-566), and the dungeon map screen shows the walked tiles (D-567).
  - The game draws one 16:9 frame of 1280 by 720, with black bars on the Deck, and 1920 by 1080 must look good (D-568). D-568 supersedes D-480.
  - Each use of "scene" names its kind: story scene, map scene, battle scene, or hub scene (D-572).
- The other answers took the recommendation. PR-68 and PR-50 move before PR-12 (D-556), the party window sets the starting row (D-558), and PR-61 shows the crash message and holds the input map (D-559, D-561). A workflow change takes the other review (D-560), PR-81 builds the sealed gallery (D-562), and a scene step joins a cast member (D-563). The first playable holds no boss (D-564), PR-39 lands after PR-78 (D-565), and the status window, the settings version, and the M-4 band have owners (D-569 to D-571).
- The session applied every answer and every forced fix across `docs/design.md`, the registers, the 17 roadmap files, the agent files, the PR template, three skills, the `playtest-bot` agent file, a runbook, and five world files. Twenty earlier decision rows gained their revision marks, and the note on D-543 gained its correction.
- Four fixes needed evidence, not a vote:
  - A read of the Valve Deck page on 2026-09-16 confirmed all four Verified rules. `docs/design.md` held two, so the session recorded the other two with the source and the date.
  - The note that the session wrote on D-543 while it answered the review of PR #11 was wrong: it put PR-68 before PR-14. D-556 now makes it true, and the row says so.
  - The rebuild of D-554 removed text that five area citations still pointed at. Each now cites a decision.
  - The critic claim that the reflection switch on Core reaches no program needs code to check. OQ-179 holds it for PR-5.
- The session placed two items with no question, because a stated rule forces each one. PR-23 draws the sprite frames of Ottild and Elio, under the rule of `area-art.md` that the frames land before each join. The night light setup of the mining town goes to the arc batch that writes the night pass, under D-442. The owner can move either.
- The handoff held ten entries before this one, so Session 27 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `c4d39fe` (PR #11).
- The branch `docs/pr-12-critic` sits on `main`, and its remote head is the commit that holds this entry. A count of its commits goes stale with each fix to this entry, so the entry gives none.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- The ids stay gap-free: D-1 to D-572, OQ-1 to OQ-179, and F-1 to F-57.
- A check proves that the phase lists of section 7, the sequence of section 8, and the five phase sequences give the same 79 PR items. Every active id from PR-1 to PR-81 has one place, and every numbered list in `docs/roadmaps/` counts from 1.

### In flight

PR #12 is open and waits for the cross-provider review.

- The automated pass is complete. The `Gitar` check run on the PR head `b9b0f24` completed with the conclusion `success`, and the review comment reported `Approved` with no issue (D-14, D-66).
- The pass left no inline comment, so no comment waits for an answer.
- The effective head is `84b4128`. The commits above it change `docs/session-handoff.md` alone, so each is a metadata commit and keeps that approval (the `pr-review` skill).
- PR #12 adds decision rows, so it takes the review of the other provider and never the `review-override` label (D-401).

### Traps and gotchas

- PR #12 changes `.github/pull_request_template.md` and the agent files, not `.github/workflows/`. D-560 moves workflow changes out of the override set from now on.
- D-568 supersedes D-480. A new citation of D-480 must name D-568. The external facts at 1280 by 800 stay, because they state the size of the Deck screen and the rule of Valve.
- A bare "scene" is not a term (D-572). The dated records, the resolved questions, the quoted source text, "scene light", "test scene", and "Godot scene file" keep their words.
- A renumber helper of this session once restarted a list at its anchor. The session repaired three lists and then checked every list of `docs/roadmaps/`. A later session that inserts a numbered item renumbers the whole list and runs that check.
- Phase 2 holds 46 PRs, and 43 of them land before Gate 2.
- The order of PR-27, the second hub, against the flight through the refuge is not checked. A later critic pass can read it.
- The next ids are D-573, OQ-180, F-58, L-16, G-26, PR-82, M-7, and Session 38.

### Open questions that block progress

None for PR #12. OQ-179 blocks PR-5. Every other detail question stays with the PR that the registers name (D-487).

### Next concrete action

A Codex session reviews PR #12 at the effective head `84b4128` and writes `docs/reviews/pr-12.md` (T-4, D-17, D-401). The automated pass is complete, and no comment of it waits for an answer.

## Session 36: 2026-09-16, Codex

Author: Codex
Session: repeat cross-provider review of PR #11 at effective head `6266d54`.

### What this session did, and why

- Read the current handoff first, then the review response, the review skill, the STE skill, the corrected diff, the roadmap contracts, and the PR comments.
- Verified that Claude Code made the substantive PR changes and that Codex remains the eligible opposite provider under T-4 and D-17.
- Recomputed the effective head. `6266d54` is the newest substantive commit. The later commit `7899972` changes only `docs/session-handoff.md`.
- Reproduced both original triggers and verified their corrections. PR-61 now tests the fit without the later screen-test job, names its fixture panel, and leaves screen captures to PR-41. D-543 now records D-544 as a partial revision and gives PR-68 sole ownership of the condition form.
- Verified the additional PR-68 correction. Its exit test uses a scripted intent list, while the later bot dependency stays in its review focus.
- The exit-test dependency scan, PR-id coverage check, order check, interim STE check, `git diff --check`, and guidance identity check pass.
- Updated `docs/reviews/pr-11.md`, preserved P1-1 and P2-1 under `## Earlier verdicts`, and set the verdict `Ready for owner merge` for `6266d54`.

### State of the build

- No code exists. `main` is `63803d9` (PR #10).
- PR #11 is open on `docs/pr-11-roadmaps`. Its remote tip is `7899972`, and its effective head is `6266d54`.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #11 is ready for owner merge. PR #12 holds the next design-critic pass after the merge (D-484, D-490).

### Traps and gotchas

- The review verdict covers `6266d54`, not the metadata tip `7899972`.
- P1-1 had partial merit. The screen-capture part was fixed. The fixture-panel part remains owned by PR-61.
- P2-1 is fixed by the explicit revision note in D-543. A later decision must keep the same revision form.
- The next ids are D-555, OQ-179, F-57, L-16, G-26, PR-80, M-7, and Session 37.

### Open questions that block progress

None for PR #11. OQ-60 to OQ-178 remain assigned to later PRs.

### Next concrete action

The owner can merge PR #11. Then a fresh session starts PR #12 after the merge.

## Session 35: 2026-09-16, Claude Code

Author: Claude Code
Session: the answer to the PR #11 review findings, on branch `docs/pr-11-roadmaps`.

### What this session did, and why

- The session read `docs/reviews/pr-11.md`, the `Changes required` verdict of Session 34 for head `848de1e`. It assessed each finding against the evidence before it changed a file (the `pr-review` skill).
- P1-1 has partial merit.
  - The part with merit: exit test 1 of PR-61 asked for a screen test, and PR-41 creates that job four PRs later. The order cannot move, because D-524 puts PR-61 right before PR-7, and PR-41 captures the map scene that PR-7 builds. So the test moves. PR-61 now computes both views and both steps of the fit in a headless test, and PR-41 takes the captures.
  - The part with no merit: the finding says that the panels of PR-61 wait for PR-62. Section 7.4 of `area-ui-input.md` gives the panel-sizing rule to PR-61, and PR-62 owns the window stack alone. The response quotes that section. The exit test now reads "the fixture panel" for precision, and the owner of the rule stands.
- A scan for the class of defect that P1-1 names found one more instance that the review did not list. Exit test 2 of PR-68 used the bots, and PR-15 lands three PRs later. The test now uses a scripted intent list, which needs no bot policy.
- P2-1 has full merit. D-544 moved the condition form from PR-18 to PR-68 and left no revision mark on D-543. The Effect column of D-543 now carries `Revised in part by D-544`, with the part that changed and the parts that stand. The stale fixture-condition line came out with it.
- The regression check of P2-1 asked that every roadmap citation name PR-68. A search found twelve citations of D-543 outside the register and the dated records. Each cites the one-form rule or names PR-68, so no roadmap text needed a change. The defect sat in the register row alone.
- The session wrote `docs/reviews/pr-11-response.md` with the disposition, the evidence, and the regression check of each finding.
- The session added no decision, no question, and no finding. Nothing in the answer needed an owner choice.
- The handoff held ten entries before this one, so Session 25 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `63803d9` (PR #10).
- PR #11 is open on branch `docs/pr-11-roadmaps`. Its remote head is the commit that holds this entry.
- The corrections change `docs/decisions.md` and `docs/roadmaps/phase-2-first-playable.md`, which sit outside the metadata set. So the effective head moves off `848de1e` to this commit, and the repeat review reads the new head.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #11 waits for the repeat cross-provider review.

- The automated pass ran again on the corrected head `6266d54` and reported `Approved`, with no open finding and no new comment (D-14, D-66). The head landed at 14:08:46 UTC, the request went out at 14:08:57, and the result came at 14:09:36.
- The pass left two comments over the life of the PR. One had merit, and `1bca609` answered it. The other was the acknowledgement of a request.
- The two review findings need a Codex session to verify each original trigger and set each finding to `fixed`.

### Traps and gotchas

- The corrections move the effective head, so the repeat review must name the new head, and never `848de1e` (the `pr-review` skill).
- An exit test of a PR can only use a tool or a job that exists when that PR lands. A script now checks that rule over all five phase files, and the response records it.
- Two lines of that scan are false matches. PR-59 and PR-37 use "captures" as the verb of the screen-test job of PR-41, which lands before each of them.
- A decision that is revised in part stays citable, so the twelve citations of D-543 stand (`CLAUDE.md`).
- An on-demand pass from the comment `Gitar review` writes no check run. Its result is the review comment itself.
- The next ids are D-555, OQ-179, F-57, L-16, G-26, PR-80, M-7, and Session 36.

### Open questions that block progress

None. OQ-60 to OQ-178 stay with the later PRs that the phase files name (D-487).

### Next concrete action

A Codex session repeats the review of PR #11 at the new effective head. It reads `docs/reviews/pr-11-response.md`, verifies the trigger and the regression check of P1-1 and P2-1, sets the status of each finding, and writes the verdict for that head (T-4, D-17, the `pr-review` skill).

## Session 34: 2026-09-16, Codex

Author: Codex
Session: cross-provider review of PR #11 at effective head `848de1e`.

### What this session did, and why

- Read the current handoff first, then the review skill, the STE skill, the design, the decision register, the questions register, the roadmaps, the prior review records, and the PR comments.
- Verified that Claude Code made the substantive PR changes and that Codex is the eligible opposite provider under T-4 and D-17.
- Recomputed the effective head. `848de1e` is the newest substantive commit. The three later commits change only `docs/session-handoff.md`.
- Inspected the complete PR diff and found two blockers. PR-61 requires a screen test before PR-41 creates it and a panel test before PR-62 creates the panels. D-543 assigns the condition format to PR-18, while D-544 assigns it to PR-68 without an explicit revision note.
- Ran the interim STE check, `git diff --check`, and the guidance identity check. All passed. No executable checks exist yet.
- Added `docs/reviews/pr-11.md` with the verdict `Changes required` for `848de1e`.

### State of the build

- No code exists. `main` is `63803d9` (PR #10).
- PR #11 is open on `docs/pr-11-roadmaps`. Its remote tip is `e92e329`, and its effective head is `848de1e`.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #11 needs the two roadmap corrections in `docs/reviews/pr-11.md`, then a repeat cross-provider review at the new effective head.

### Traps and gotchas

- A review record names the newest substantive commit, not a later handoff-only commit.
- PR-61 must not require the screen-test job of PR-41 or the panels of PR-62 unless the sequence changes with it.
- D-544 must explicitly revise D-543 or D-543 must name PR-68 as the condition-format owner.
- The next ids are D-555, OQ-179, F-57, L-16, G-26, PR-80, M-7, and Session 35.

### Open questions that block progress

None for this review. OQ-60 to OQ-178 remain assigned to later PRs.

### Next concrete action

The author corrects the PR-61 exit-test ownership and the D-543 decision note. Then a Codex session repeats the review and verifies the corrected triggers.

## Session 33: 2026-09-16, Claude Code

Author: Claude Code
Session: the five phase files of PR #11, the rebuild of sections 7 and 8, and the open of PR #11.

### What this session did, and why

- The session resumed PR #11 from the handoff of Session 32. The remote head was `32193bd`, and no other session pushed after it.
- The working tree held an uncommitted block from Session 32: the new `docs/roadmaps/readme.md`, plus edits to two skills, the agent files, `docs/questions.md`, and the runbook. The session checked each edit, ran the STE check, and committed the block as `544c7f2`.
- The session wrote the five phase files of D-485. Each PR entry gives a scope, an out-of-scope list, numbered exit tests, a review focus, its questions, and the area files that it cites (D-144, D-487).
  - `phase-1-foundations.md`: the Deck test, PR-1, PR-2, PR-3, PR-46, PR-4, PR-5, PR-6, PR-43, PR-44, PR-47, PR-34, M-1, M-2, and Gate 1.
  - `phase-2-first-playable.md`: 42 PRs, from PR-54 to PR-17, then M-3, M-4, M-6, Gate 2, and the store block of PR-74, PR-75, and PR-76.
  - `phase-3-story-systems.md`: PR-18, PR-19, PR-20, PR-21, the retired PR-22, and Gate 3.
  - `phase-4-region-one.md`: PR-23 to PR-27, PR-42, PR-73, PR-28, PR-29, PR-77, PR-30, M-5, and Gate 4.
  - `phase-5-first-release.md`: PR-31, PR-33, PR-39, PR-78, PR-79, PR-40, the two owner steps, the trailer, the retired PR-32, and Gate 5.
- A check of the files proves the coverage. Every active id from PR-1 to PR-79 has one entry in one phase file and one place in one phase sequence. PR-22 and PR-32 are retired, and each has a short entry that states the gap (G-10).
- The session then asked the one question that the rebuild could not settle: the shape of section 7 with 79 PR ids. The owner chose the high-level index, which the session recommended (D-554).
- The session rebuilt sections 7 and 8 of `docs/design.md` on that answer (D-488, D-554). Section 7 lost about 40 technical paragraphs, which now live in the phase files alone. Each phase in section 7 keeps its gate, its ordered item list with one line for each item, its plain-English paragraph, and a link to its phase file. Section 8 takes the order that the phase files hold.
- A check proves that the phase lists of section 7, the sequence of section 8, and the section 8 of each phase file give the same 77 ids in the same order.
- The rebuild also fixed each stale text that the traps of Session 32 named. G-22 now names PR-49. The read order of `CLAUDE.md` and `AGENTS.md` no longer says that a docs PR creates `docs/roadmaps/`, and its `det-lint` command line names PR-46. The PR gate of the agent files and the PR template gained the `screen-test` line of PR-41 and the bot line of PR-15, and both now name PR-46 and PR-49 (G-16, D-496).
- The status paragraph of `docs/roadmaps/readme.md` is gone, as D-488 asks.
- The session opened PR #11 at head `848de1e`, with the full PR template in its description. The PR adds decision rows, so it takes the review of the other provider and never the `review-override` label (D-401).
- The handoff held ten entries before this one, so Session 23 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `63803d9` (PR #10).
- PR #11 is open on branch `docs/pr-11-roadmaps`. The branch holds twenty commits on `main`, and its remote head is the commit that holds this entry.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- The diff against `main` is 31 files: 18 new files in `docs/roadmaps/`, and 13 changed files.

### In flight

PR #11 passed the automated pass and waits for the cross-provider review.

- The effective head is `848de1e`. The two commits above it change `docs/session-handoff.md` alone, so each is a metadata commit and neither moves the effective head (the `pr-review` skill).
- The `Gitar` check run on `848de1e` completed with the conclusion `success`. An on-demand pass at 13:52 UTC read the corrected handoff and reported `Approved`, with one finding closed and none open (D-14, D-66).
- The pass left one inline comment, on a wrong citation in this entry. The author fixed it in `1bca609`, replied, and resolved the thread.
- Next comes the Codex review, which writes `docs/reviews/pr-11.md` with a verdict for the effective head `848de1e` (T-4, D-17). This PR adds decision rows, so no label exempts it (D-401).

### Traps and gotchas

- PR #11 adds decision rows, so the `review-override` label never applies to it (D-401). It needs the Codex review.
- Count a gitar pass only from a check run on the head SHA, never from a dashboard page. A push that changes a path outside the metadata set needs a new pass.
- A commit that changes `docs/reviews/`, `docs/session-handoff.md`, or `docs/session-handoff-archive.md` alone is a metadata commit, and it never moves the effective head (the `pr-review` skill). A handoff commit above an approved head keeps that approval.
- An on-demand pass from the comment `Gitar review` writes no check run. Its result is the review comment itself, and the check run stays on the last automatic pass.
- Section 7 of `docs/design.md` is now an index. A session that wants the scope or the exit tests of a PR reads its phase file, and it never adds a paragraph back to section 7 (D-554).
- The five phase files and the twelve area files each cite the other set. A change to a PR id, an order, or a gate must change the phase file, the area file, and sections 7 and 8 together.
- `docs/roadmaps/readme.md` is the index of the folder, and the read order of the agent files points at it.
- D-540 revises D-114 in part, and D-548 revises D-418 in part. A citation of either must name the revision.
- The evaluator of D-534 still has no measurement (F-53). PR-11 reports the cost of a turn before Gate 2.
- New PR ids stay at PR-43 to PR-79. The next id is PR-80.
- The next ids are D-555, OQ-179, F-57, L-16, G-26, PR-80, M-7, and Session 34.

### Open questions that block progress

None. The phase files carry every blocked question into a table in their section 9, and the register `docs/questions.md` holds each one. Phase 1 lists 28, Phase 2 lists 85, Phase 3 lists 6, Phase 4 lists 1, and Phase 5 lists 10. Each one blocks a later PR, and that PR asks it when it starts (D-487).

### Next concrete action

A Codex session reviews PR #11 at the effective head `848de1e` and writes `docs/reviews/pr-11.md` (T-4, D-17, D-401). The automated pass is complete, and no comment of it waits for an answer. After PR #11 merges, PR #12 holds the next design-critic pass on the merged plan (D-484, D-490).

## Session 32: 2026-09-16, Claude Code

Author: Claude Code
Session: the last three area files of PR #11, `area-story.md`, `area-audio.md`, and `area-release.md`, on branch `docs/pr-11-roadmaps`.

### What this session did, and why

- The session resumed PR #11 from the handoff of Session 31. The remote head was `95e3276`, and no other session pushed after it.
- The owner asked for the three files in one session: story, then audio, then release. All three belong to PR #11, so this entry covers the three (D-18).
- The session read the design, the story, audio, and release decision blocks, the questions register, the nine finished area files, the skills, and `docs/world/arc.md`.
- Five checks of the Godot source and docs at `4.7.2-stable` gave the audio facts. The story and release areas needed no new check. The release facts come from `docs/design.md`, read 2026-09-14.
- The story block: the owner took all six recommendations, in two batches.
  - Core runs each scene and holds its step index, and Game draws each step and sends a wait intent (D-540). That lets the bots and the night gate play every scene.
  - PR-36 splits, and PR-68 takes the scene format and the scene runner (D-541).
  - A story flag is a name that is on or off, and one condition form serves every reader (D-542, D-543).
  - PR-68 also takes the flag set, which closes the gap that F-55 records (D-544). PR-50 lands right after PR-68 (D-545).
- The audio block: the owner took all four recommendations.
  - Three PRs take the audio work that PR-38 leaves: PR-69 the player, PR-70 the rules, and PR-71 the sound room (D-546).
  - The build renders each track into the Game assembly, beside the content files (D-547). That answers the open note of D-508.
  - An audio file names the content ids that it serves, and a rule file names no track (D-548). That revises D-418 in part.
  - PR-72 and PR-73 hold the music and the sounds of region one (D-549).
- The release block: the owner took all four recommendations.
  - The store page work splits into PR-75, the text and the owner steps, and PR-76, the art and the screenshots (D-550).
  - PR-74 adds the capture, right before PR-75, so one path takes each screenshot and each trailer shot (D-551).
  - PR-77 holds the credits roll, right after PR-29, so the owner sees it at Gate 4 (D-552).
  - PR-40 splits into PR-78, the Steamworks code, PR-79, the macOS signature in CI, and PR-40, the Steam publish (D-553).
- F-55 and F-56 record the two findings. F-55 is the flag set that sat one phase after its first reader. F-56 is a Godot audio call that returns an empty reference with a message in the log alone, and a playback position that Godot does not report exactly.
- The session filed 35 detail questions, OQ-144 to OQ-178, as D-487 asks. None blocks PR #11.
- The session wrote three area files. It updated `docs/design.md` (three dated lines, four system map rows, the cost model, F-55, and F-56), `area-core.md`, `area-tools.md`, `area-ci.md`, `area-art.md`, `area-effects.md`, `area-ui-input.md`, `area-exploration.md`, `area-progression.md`, the `ste-writing` skill, and fifteen earlier decision rows with notes.
- Two rules entered the files with no question, because an earlier decision forces each one. A scene names no art, as D-519 asks. Game moves a scene sprite from the tick of Core, never from a timer of Godot, as it moves the camera (F-52, G-23). The owner can ask for another rule.
- The handoff held ten entries before this one, so Session 22 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `63803d9` (PR #10).
- The branch `docs/pr-11-roadmaps` holds twelve commits on `main`, and its remote head is the commit that holds this entry. No PR is open, because PR #11 opens when the roadmaps and the rebuild are complete (D-489).
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #11: the twelve area files of D-485 are done. Next come the five phase files, then the rebuild of sections 7 and 8 of `docs/design.md` (D-488). Then PR #11 opens.

### Traps and gotchas

- The rebuild changes more than sections 7 and 8. These texts still name the old plan: G-22 in `docs/design.md`, the PR gate of `CLAUDE.md` and `AGENTS.md` at lines 130 and 145 to 147, and lines 12, 13, and 15 of `.github/pull_request_template.md`. Section 7 still puts the export job in PR-7, the night gate in PR-15, and det-lint in PR-4, and M-3 reads one night count.
- Section 7 also needs every new PR in order. The story, audio, and release adds are: PR-68 right before PR-36, PR-50 right after PR-68, PR-69 and PR-70 right after PR-38, PR-71 after PR-70, PR-72 right before PR-17, PR-74 right after Gate 2, then PR-75 and PR-76, PR-77 right after PR-29, PR-73 in Phase 4, and PR-78 and PR-79 before PR-40 in Phase 5.
- Nine PRs lost work to a new id. The five of Session 31 stand, and four more join: the scene format and the runner left PR-36 for PR-68, the store page work left PR-40 for PR-75 and PR-76, the macOS signature left PR-40 for PR-79, and the Steamworks code left PR-40 for PR-78 (D-541, D-550, D-553). Section 7 still gives each one to the old PR.
- Section 7 also holds four texts that a later decision contradicts. The PR-36 entry says "Implement the runner in Game and the choice result in Core", which D-540 revises. The PR-18 entry gives PR-18 the flag set, which D-544 moves to PR-68. The PR-38 entry and the store page entry still say that the roadmaps PR gives the ids, which D-546, D-549, and D-550 answer. The PR-33 entry still leaves the credits roll unplaced, which D-552 answers.
- The PR-7 entry of section 7 still says that the roadmaps PR gives the export job its id, which D-503 answered in Session 29.
- After PR #11 merges, line 17 of `CLAUDE.md` and `AGENTS.md` is stale: `docs/roadmaps/` exists, and the roadmaps docs PR no longer creates it.
- `docs/roadmaps/readme.md` is the index of the folder. Its last paragraph, "Status, 2026-09-16", describes the phase files as absent, and the rebuild session removes that paragraph. The rebuild also adds a link from section 7 of `docs/design.md` to that index, which the `design-doc-style` skill asks for and section 7 does not have.
- Section 8 needs the owner step for the Apple Developer Program before PR-79, not before PR-40 (D-455, D-553).
- D-540 revises D-114 in part, and D-548 revises D-418 in part. A citation of either must name the revision.
- The evaluator of D-534 still has no measurement (F-53). PR-11 reports the cost of a turn before Gate 2.
- New PR ids so far: PR-43 to PR-79, 37 of the about 20 that D-486 named. The next id is PR-80.
- The next ids are D-554, OQ-179, F-57, L-16, G-26, PR-80, M-7, and Session 33.

### Open questions that block progress

None for PR #11. OQ-168 to OQ-178 block PR-6, PR-31, PR-33, PR-74, PR-76, PR-78, PR-79, and PR-40. OQ-156 to OQ-167 block PR-38, PR-69, PR-70, PR-71, and PR-72. OQ-144 to OQ-155 block PR-68, PR-36, PR-19, and PR-20. OQ-134 to OQ-143 block PR-67, PR-12, and PR-13. OQ-124 to OQ-133 block PR-9, PR-10, PR-11, and PR-20. OQ-114 to OQ-123 block PR-7, PR-8, PR-21, PR-35, PR-64, and PR-65. OQ-104 to OQ-113 block PR-61, PR-62, PR-63, and PR-36. OQ-92 to OQ-103 block the Deck test, PR-56 to PR-59, PR-37, and PR-10. OQ-85 to OQ-91 block PR-34, PR-7, PR-33, and PR-55. OQ-75 to OQ-84 block PR-1, PR-15, PR-41, PR-49, and PR-54. OQ-67 to OQ-72 and OQ-74 block PR-2, PR-3, PR-46, PR-47, and PR-15. OQ-60 to OQ-66 block PR-4, PR-5, PR-6, and PR-43. OQ-57 blocks PR-75, PR-33, and PR-44, OQ-58 blocks PR-78, OQ-59 blocks PR-75, and OQ-3 waits for PR-3.

### Next concrete action

A session continues PR #11 on `docs/pr-11-roadmaps`. It reads the twelve area files and D-483 to D-490, then it writes the five phase files of D-485: `phase-1-foundations.md`, `phase-2-first-playable.md`, `phase-3-story-systems.md`, `phase-4-region-one.md`, and `phase-5-first-release.md`. Each phase entry gives its PR a scope, exit tests, a review focus, its questions, and the area file that it cites (D-144, D-487).

## Session 31: 2026-09-16, Claude Code

Author: Claude Code
Session: the fifth to the ninth area files of PR #11, `area-effects.md`, `area-ui-input.md`, `area-exploration.md`, `area-battle.md`, and `area-progression.md`, on branch `docs/pr-11-roadmaps`.

### What this session did, and why

- The session resumed PR #11 from the handoff of Session 30. The remote head was `c2c169b`, and no other session pushed after it.
- The owner asked for one area after another in one session: effects, then UI and input, then exploration, then battle, then progression. All five files belong to PR #11, so this entry covers the five (D-18).
- The session read the design, the whole decision register, the questions register, the four finished area files, the skills, the runbook, the PR template, the places file of the world, and the `game-text-style` skill.
- Seven read-only research agents read the Godot source and docs at `4.7.2-stable`, the Steam and SteamOS pages, and WCAG 2.2. The session checked each key fact again before it entered a document. The battle and progression areas needed no external fact, because Core holds every rule of a fight and of a build.
- The effects block: the owner took all four contract recommendations.
  - Every effect PR lands before PR-17, each right after the first scene that it needs (D-520). PR-56 is light and shadows, PR-57 the effect files with particles and the battle effects, PR-58 the ambient effects, PR-59 glow, and PR-60 the transitions.
  - OQ-73 closed with a review sheet of eight fixed light directions, so PR-48 lands right before PR-56 (D-521).
  - No rule waits for an effect. Where the world waits for one, Game sends a wait intent, and the run record holds it (D-522).
  - The Deck test also measures an effect budget, and a test fails content that passes it (D-523).
- The UI and input block: the owner took all four contract recommendations.
  - PR-61 builds the UI base right before PR-7, and PR-62 the menu windows right before PR-12 (D-524, D-525).
  - PR-63 builds the settings and the four accessibility settings in Phase 2, right before PR-57 (D-526).
  - One JSON file holds the UI style, and Game builds the Godot `Theme` from it (D-527).
- The exploration block: the owner took three recommendations and chose one split against the recommendation.
  - One rule file holds each map, with its terrain rows and every thing that a rule reads (D-528).
  - PR-16 splits, and PR-64 takes the traps, the hazards, and the statuses on the map (D-529).
  - The shop leaves PR-14 for PR-65, against the recommendation of one PR for the whole hub (D-530).
  - One run holds the map state and the battle state, and no map system ticks during a battle (D-531).
- The battle block: the owner took three recommendations and chose the deeper evaluator against the recommendation.
  - Core resolves each action at once and emits events. Game plays the queue and takes the next command when it drains, so no wait intent enters a fight (D-532).
  - PR-9 splits, and PR-66 takes the eight elements and the ten statuses (D-533).
  - The evaluator simulates each legal action and the strongest reply of the other side (D-534). The session recommended one action ahead alone.
  - A group file for each region holds each enemy group, and a map names a group by its id (D-535).
- The progression block: the owner took all four contract recommendations.
  - PR-12 splits, and PR-67 takes the character level, the experience, and MP, right before PR-12 (D-536).
  - Each character carries its own stat curve in content (D-537). That closes a gap that this session found, which F-54 records.
  - The quest state of PR-19 holds every personal task, and a side aptitude reads a story flag (D-538).
  - Each lesson lists its named forms, and each form names the point total that opens it (D-539).
- F-46 to F-54 record the findings of the five blocks: three silent failures of 2D light, a glow that can reach a lit sprite, a fit that Godot cannot make, three font defaults, five input facts, four tile map defaults, a camera that centers a small map with no doc behind it, an evaluator with no measurement, and stats with no source after the end of the job system.
- The session filed 52 detail questions, OQ-92 to OQ-143, as D-487 asks. None blocks PR #11.
- The session wrote five area files. It updated `docs/design.md` (five dated lines, the system map, the cost model, and F-46 to F-54), `area-core.md`, `area-tools.md`, `area-ci.md`, `area-art.md`, the runbook of the machine, the `ste-writing`, `csharp-conventions`, and `pr-review` skills, and ten earlier decision rows with notes.
- Three rules entered the files with no question, because an earlier decision or a verified fact forces each one. An effect file names the content ids that it serves, and a rule file never names an effect, as D-519 asks for art (D-495). Game makes each intent from an input event, never from a poll of `Input`, because a poll sees input that a menu already took (F-50, D-493). Game moves the camera from the tick, because the smoothing of Godot can run more than once in a frame (F-52). The owner can ask for another rule.
- The handoff held ten entries before this one, so Session 21 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `63803d9` (PR #10).
- The branch `docs/pr-11-roadmaps` holds nine commits on `main`, and its remote head is the commit that holds this entry. No PR is open, because PR #11 opens when the roadmaps and the rebuild are complete (D-489).
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #11: nine of twelve area files are done. Next comes `area-story.md`, then `area-audio.md` and `area-release.md` (D-485). Then come the five phase files and the rebuild of sections 7 and 8 (D-488).

### Traps and gotchas

- The rebuild changes more than sections 7 and 8. These texts still name the old plan: G-22 in `docs/design.md`, the PR gate of `CLAUDE.md` and `AGENTS.md` at lines 130 and 145 to 147, and lines 12, 13, and 15 of `.github/pull_request_template.md`. Section 7 still puts the export job in PR-7, the night gate in PR-15, and det-lint in PR-4, and M-3 reads one night count.
- Section 7 also needs the new PRs in order: PR-61 before PR-7, PR-55 before PR-10, PR-48 and PR-56 between PR-10 and PR-11, PR-63 before PR-57, PR-57 to PR-60 after it, PR-66 right after PR-9, PR-67 right before PR-12, PR-62 before PR-12, PR-65 after PR-13, and PR-64 after PR-16. Section 8 needs the Deck test with its effect budget before PR-1.
- Five PRs lost work to a new id: the settings left PR-33 for PR-63, the shop left PR-14 for PR-65, the traps left PR-16 for PR-64, the elements and statuses left PR-9 for PR-66, and the levels and MP left PR-12 for PR-67 (D-526, D-529, D-530, D-533, D-536). Section 7 still gives each one to the old PR.
- The evaluator of D-534 has no measurement, and the same code runs on the Deck and through a night of fourteen thousand runs (F-53). PR-11 reports the cost of a turn before Gate 2.
- Godot drops a light past 15 on one canvas item with no message (F-46). Godot has no fit like the fit of D-232 (F-48), and it centers a map smaller than the view with only the source behind it (F-52).
- `area-story.md` must hold the flags, the branches, the quests, the personal tasks of D-538, the scene format that the screenplay tool of PR-50 prints, and the scenes that play on a map. `area-audio.md` holds the tracks, the stings, and the sound room. `area-release.md` holds the store work at Gate 2, the trailer capture with fixed particle seeds, and the studio mark of OQ-90.
- New PR ids so far: PR-43 to PR-67, 25 of the about 20 that D-486 named. The next id is PR-68.
- The next ids are D-540, OQ-144, F-55, L-16, G-26, PR-68, M-7, and Session 32.

### Open questions that block progress

None for PR #11. OQ-134 to OQ-143 block PR-67, PR-12, and PR-13. OQ-124 to OQ-133 block PR-9, PR-10, PR-11, and PR-20. OQ-114 to OQ-123 block PR-7, PR-8, PR-21, PR-35, PR-64, and PR-65. OQ-104 to OQ-113 block PR-61, PR-62, PR-63, and PR-36. OQ-92 to OQ-103 block the Deck test, PR-56 to PR-59, PR-37, and PR-10. OQ-85 to OQ-91 block PR-34, PR-7, PR-33, and PR-55. OQ-75 to OQ-84 block PR-1, PR-15, PR-41, PR-49, and PR-54. OQ-67 to OQ-72 and OQ-74 block PR-2, PR-3, PR-46, PR-47, and PR-15. OQ-60 to OQ-66 block PR-4, PR-5, PR-6, and PR-43. OQ-57 and OQ-59 block the store page at Gate 2, OQ-58 blocks PR-40, and OQ-3 waits for PR-3.

### Next concrete action

A session continues PR #11 on `docs/pr-11-roadmaps`. It reads the nine finished area files, D-520 to D-539, and the story entries of `docs/design.md`, such as D-40, D-114, D-173, D-282, D-329, D-350, and D-352 to D-355. Then it writes `docs/roadmaps/area-story.md`, asks the contract questions, and files detail questions with their PRs (D-487, D-488).

## Session 30: 2026-09-15, Claude Code

Author: Claude Code
Session: the fourth area file of PR #11, `docs/roadmaps/area-art.md`, on branch `docs/pr-11-roadmaps`.

### What this session did, and why

- The session resumed PR #11 from the handoff of Session 29. The remote head was `4d68a01`, and no other session pushed after it.
- The owner reported two changes to the repository settings: "Allow rebase merging" is off, and "Require actions to be pinned to a full-length commit SHA" is on. `gh api` confirmed both, and squash merging is the only merge method. The runbook, `area-ci.md`, D-8, and D-511 record it.
- The session read the design, the whole decision register, the questions register, the three finished area files, the skills, the runbook, the PR template, the palette, and the old atlas script.
- Two read-only research agents read GitHub pages and the Godot docs and source at `4.7.2-stable`. The session checked each key fact again before it entered a document: the `gh` help and release notes, the GitHub docs source, the README of `upload-artifact`, and the Godot XML, shader, and C++ files.
- A new fact settled the trap of Session 29: `gh` 2.99.0 of 2026-09-01 uploads an image into a PR description with `--attach`, and the Mac has 2.100.0.
- The owner took all six contract recommendations in two batches:
  - The owner sees each art batch as review sheets that `gh --attach` puts in the PR description, and no review image enters git (D-514).
  - A drawing is one JSON file with rows as strings (D-515). This settles G-6 and D-116 against the `.grid` files of D-119 and D-404.
  - A large picture places drawn pieces (D-516), because one full-screen grid holds about 1.1 million palette keys (F-44). PR-55 builds the format right before PR-10 (D-518).
  - Core holds the record of every content file, a file that no rule reads included (D-517).
  - An art file names the content ids that it draws, and a rule file never names art (D-519).
- The question of D-516 named the file a layout. A layout already names a map file (D-39, D-386), so the rows and the documents use the term large picture (D-12). The Effect column of D-516 says so, and the owner can ask for another term.
- The Godot checks removed one question: both renderer families correct the normal map of a sprite that `flip_h` mirrors, so the atlas holds no mirrored frame. F-45 records three Godot defaults that meet the pixel art.
- The session filed seven detail questions for PR-34, PR-7, PR-33, and PR-55 (OQ-85 to OQ-91), as D-487 asks. None blocks PR #11.
- The session wrote `docs/roadmaps/area-art.md`. It updated `docs/design.md` (a dated line, the system map, the cost model, F-44, F-45, G-24, and G-25), `area-tools.md`, `area-core.md`, `area-ci.md`, the runbook of the machine, the `ste-writing`, `csharp-conventions`, and `pr-review` skills, and eleven earlier decision rows with notes.
- The handoff held ten entries before this one, so Session 20 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `63803d9` (PR #10).
- The branch `docs/pr-11-roadmaps` holds four commits on `main`, and its remote head is the commit that holds this entry. No PR is open, because PR #11 opens when the roadmaps and the rebuild are complete (D-489).
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.
- On 2026-09-14, `gh api` showed `allow_rebase_merge` false, `allow_merge_commit` false, and `sha_pinning_required` true.

### In flight

PR #11: four of twelve area files are done. Next comes `area-effects.md`, then the other seven area files in the order of D-485. Then come the five phase files and the rebuild of sections 7 and 8 (D-488).

### Traps and gotchas

- The rebuild changes more than sections 7 and 8. These texts still name the old plan:
  - G-22 in `docs/design.md`: PR-49 (D-496), the night counts (D-507), a night on the head of a PR (D-510), and docs-only PRs (D-513).
  - The PR gate of `CLAUDE.md` and `AGENTS.md`, lines 145 to 147: det-lint in PR-46, the identity file (D-504), the night gate in PR-49, and a new line for the bot runs of PR-15 (D-505). Line 130 still says det-lint comes "after PR-4".
  - Lines 12, 13, and 15 of `.github/pull_request_template.md`, with the same changes and the bot line.
  - Section 7 still puts the export job in PR-7 and the night gate in PR-15, and M-3 reads one night count. The owner step of D-511 is done, so section 8 needs no step for it.
  - Section 7 still gives PR-34 the normal maps and "the grid schema", and PR-10 "A backdrop per place" as one grid. PR-17 names the sprite set of Marrek alone, and section 7.9 of `area-art.md` adds the frames of Bergit and Dagvar (D-200, D-362). PR-55 needs an entry right before PR-10 (D-518).
- Keep the word layout for map files alone. The glossary of the `ste-writing` skill has the new art terms: drawing file, piece, large picture, atlas index, and review sheet.
- The dated row D-204 says "edge pieces" for edge tiles. `area-tools.md` now says "border tile", because piece is a glossary term.
- `area-effects.md` meets three facts: a custom shader that writes `NORMAL_MAP` gets no flip correction (issue 101277), 2D light draws at the pixel size of the Viewport whatever the texture filter (the 2D lights tutorial, read 2026-09-14), and each piece of a large picture has a normal map (D-516).
- `area-ui-input.md` must set the stretch, because the 4.7 editor writes `canvas_items` and `expand` into a new project (F-45). The fonts load from the Game assembly (D-508). A research agent read in the source that `FontFile.data` takes the bytes of a dynamic font, and no doc or test confirms it.
- OQ-86 meets `area-exploration.md`: a `TileMapLayer` needs its tiles on a regular grid in the atlas.
- The boot splash image is a PNG path that the engine reads at the start, so the studio mark of D-468 cannot come from the atlas (OQ-90).
- The samples readme still describes the `.grid` files, and the note on D-404 carries D-515. Sessions skip `docs/samples/` (D-403).
- New PR ids so far: PR-43 to PR-55, thirteen of about 20 (D-486). The next id is PR-56.
- The next ids are D-520, OQ-92, F-46, L-16, G-26, PR-56, M-7, and Session 31.

### Open questions that block progress

None for PR #11. OQ-85 to OQ-91 block PR-34, PR-7, PR-33, and PR-55. OQ-75 to OQ-84 block PR-1, PR-15, PR-41, PR-49, and PR-54. OQ-67 to OQ-74 block PR-2, PR-3, PR-46, PR-47, PR-48, and PR-15. OQ-60 to OQ-66 block PR-4, PR-5, PR-6, and PR-43. OQ-57 and OQ-59 block the store page at Gate 2, OQ-58 blocks PR-40, and OQ-3 waits for PR-3.

### Next concrete action

A session continues PR #11 on `docs/pr-11-roadmaps`. It reads `docs/roadmaps/area-core.md`, `area-tools.md`, `area-ci.md`, `area-art.md`, D-491 to D-519, and the effects entries of `docs/design.md`, such as D-139, D-160, D-180 to D-196, and D-240. Then it writes `docs/roadmaps/area-effects.md`, places PR-48 and the first PR that draws light, asks the contract questions, and files detail questions with their PRs (D-487, D-488).

## Session 29: 2026-09-14, Claude Code

Author: Claude Code
Session: the third area file of PR #11, `docs/roadmaps/area-ci.md`, on branch `docs/pr-11-roadmaps`.

### What this session did, and why

- The session resumed PR #11 from the handoff of Session 28. The remote head was `bad1797`, and no other session pushed after it.
- The session read the design, the whole decision register, the questions register, `area-core.md`, `area-tools.md`, the skills, the agent file of the playtest bot, the PR template, and the runbook of the machine. It read the decision register of what-you-carry for its night record and night gate rules, for the process alone (D-277).
- Three read-only research agents read GitHub, Godot, Microsoft, xUnit, Coverlet, and ReportGenerator pages. The session fetched eight key pages again before a fact entered a document. It also read the NuGet API, the Godot release files and their SHA-512 list, the licenses of five actions, and the settings of the repository through `gh api`.
- The session asked twelve contract questions in three batches (D-487), and the owner took nine recommendations:
  - The export job is PR-54, right before PR-7 (D-503). It also runs on a PR that changes the export, and each export runs the smoke session (D-512).
  - The replay-identity job compares each leg with a committed identity file (D-504).
  - PR-1 publishes the coverage report (D-506).
  - The Game assembly embeds `content/`, and Tools holds the one reader of the folder (D-508).
  - The night record is an artifact of the run of the night job (D-509). A night on the head commit of a PR passes that PR (D-510), and a docs-only PR passes the night gate (D-513).
  - Workflows use five actions of the `actions` organization of GitHub, each pinned to a full commit SHA, and D-511 is their G-13 entry.
- The owner chose against three recommendations:
  - The bot runs play on all three CI legs, not on Linux alone (D-505).
  - A night plays ten thousand runs on Linux and two thousand each on Windows and macOS (D-507). The session recommended ten thousand on each leg, and then one thousand on the other two. D-507 revises D-64 in part, and the session confirmed the final count before the row.
- F-40 records that the test command of `CLAUDE.md` works in VSTest mode alone. F-41 records four rules of GitHub Actions, F-42 two Godot export facts, and F-43 a night gate that blocked its own fix.
- The session filed ten detail questions for PR-1, PR-15, PR-41, PR-49, and PR-54 (OQ-75 to OQ-84), as D-487 asks. None blocks PR #11.
- The session updated `docs/design.md`: a dated line, the system map, the cost model, and F-40 to F-43. It updated `area-core.md`, `area-tools.md`, the `ste-writing`, `csharp-conventions`, and `pr-review` skills, the runbook of the machine, and six earlier decision rows with notes.
- The handoff held ten entries before this one, so Session 19 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `63803d9` (PR #10).
- The branch `docs/pr-11-roadmaps` holds three commits on `main`, and its remote head is the commit that holds this entry. No PR is open, because PR #11 opens when the roadmaps and the rebuild are complete (D-489).
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #11: three of twelve area files are done. Next comes `area-art.md`, then the other eight area files in the order of D-485. Then come the five phase files and the rebuild of sections 7 and 8 (D-488).

### Traps and gotchas

- The rebuild changes more than sections 7 and 8. These texts still name the old plan:
  - G-22 in `docs/design.md`: PR-49 (D-496), the night counts (D-507), a night on the head of a PR (D-510), and docs-only PRs (D-513).
  - The PR gate of `CLAUDE.md` and `AGENTS.md`, lines 145 to 147: det-lint in PR-46, the identity file (D-504), the night gate in PR-49, and a new line for the bot runs of PR-15 (D-505). Line 130 still says det-lint comes "after PR-4".
  - Lines 12, 13, and 15 of `.github/pull_request_template.md`, with the same changes and the bot line.
  - Section 7 still puts the export job in PR-7 and the night gate in PR-15, and M-3 reads one night count. Section 8 needs the owner step of D-511 before PR-1.
- The owner must enable "Require actions to be pinned to a full-length commit SHA" before PR-1 (D-511). The setting was off on 2026-09-14, and step 1 of the owner actions in the runbook names it.
- `gh api` showed "Allow rebase merging" on, on 2026-09-14. The runbook asks the owner to turn it off (D-8).
- D-508 changes three later area files. `area-art.md` plans the atlas as bytes loaded at run time (`Image.LoadPngFromBuffer`), not as an imported texture. `area-audio.md` says where rendered audio lives, because a large render does not suit the assembly. The phase file gives PR-5 work in Game and Tools, not in Core alone.
- The embedded resource names must match on every leg. Check the path separator on Windows in PR-5.
- F-40: if OQ-75 picks MTP, PR-1 changes the test commands of `CLAUDE.md`, `AGENTS.md`, and the `csharp-conventions` skill.
- GitHub disables a schedule after 60 days with no activity in a public repository, and the night gate then fails every PR (F-41).
- New PR ids so far: PR-43 to PR-54, twelve of about 20 (D-486). The next id is PR-55. The owner chose the fuller option three times in this block. Show running counts of runs, legs, and CI time in each batch.
- Contract questions for `area-art.md` from earlier sessions: how the owner sees an image batch in a PR (G-25), and whether `gh` can put an image in a PR description. Check it before a question assumes it.
- The next ids are D-514, OQ-85, F-44, L-16, G-26, PR-55, M-7, and Session 30.

### Open questions that block progress

None for PR #11. OQ-75 to OQ-84 block PR-1, PR-15, PR-41, PR-49, and PR-54. OQ-67 to OQ-74 block PR-2, PR-3, PR-46, PR-47, PR-48, and PR-15. OQ-60 to OQ-66 block PR-4, PR-5, PR-6, and PR-43. OQ-57 and OQ-59 block the store page at Gate 2, OQ-58 blocks PR-40, and OQ-3 waits for PR-3.

### Next concrete action

A session continues PR #11 on `docs/pr-11-roadmaps`. It reads `docs/roadmaps/area-core.md`, `area-tools.md`, `area-ci.md`, D-491 to D-513, and the graphics entries of `docs/design.md`. Then it writes `docs/roadmaps/area-art.md`, asks the contract questions, and files detail questions with their PRs (D-487, D-488).

## Session 28: 2026-09-14, Claude Code

Author: Claude Code
Session: the second area file of PR #11, `docs/roadmaps/area-tools.md`, on branch `docs/pr-11-roadmaps`.

### What this session did, and why

- The session resumed PR #11 from the handoff of Session 27. The remote head was `4e16d05`, and no other session pushed after it.
- The session read the design, the whole decision register, the questions register, `area-core.md`, the skills, the agent files, the interim tools, the PR template, and the runbook of the machine.
- The session asked seven contract questions in two batches (D-487), and the owner took each recommendation:
  - Four tools leave the PRs that they share: det-lint becomes PR-46, the PNG code PR-47, the normal maps PR-48, and the night gate PR-49 (D-496).
  - Four tools with no PR take ids and land right before their first user: the screenplay tool PR-50, the PNG import PR-51, the map preview PR-52, and the tile-edge tool PR-53 (D-497).
  - det-lint reads code through the Roslyn compiler library, and the row of D-498 is the decision entry of the package (G-13).
  - Game shows player text through one text helper, and det-lint fails a Godot text property outside it (D-499).
  - GitHub starts `pull_request_target` and `schedule` only from `main`, so PR-3 and PR-49 prove their commands in Tests, and G-16 gains a note (D-500, F-37).
  - The tile-edge tool writes one edge file per map outside the rule files (D-501).
  - A tool whose output a test compares on every CI leg uses integer math (D-502).
- The session read thirteen sources before a fact entered a document: the GitHub trigger rules, two NuGet pages and the NuGet API, eight .NET and C# pages, and the PNG standard. A page summary of NuGet named xunit.v3 4.0.0 as the latest version, and the NuGet API lists 4.0.1, so no xUnit version entered a document.
- F-38 records two float facts of .NET. F-39 records that the default string order of .NET follows the culture and the ICU version of the machine.
- F-39 corrects `area-core.md`: its section 7.5 named `SortedDictionary` for a fixed order and named no comparer. G-4 already asks for a fixed order, and among the built-in comparers only an ordinal comparison stays fixed across machines. So the session added the rule with no question. The owner can still ask for another rule.
- The session filed eight detail questions for PR-2, PR-3, PR-46, PR-47, PR-48, and PR-15 (OQ-67 to OQ-74), as D-487 asks. None blocks PR #11.
- The session updated `docs/design.md`: a dated line, the system map, F-35 to F-39, and a note on G-16. It updated `area-core.md`, the `csharp-conventions`, `pr-review`, and `ste-writing` skills, and 14 earlier decision rows with notes.
- The handoff held ten entries before this one, so Session 18 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `63803d9` (PR #10).
- The branch `docs/pr-11-roadmaps` holds two commits on `main`, and its remote head is the commit that holds this entry. No PR is open, because PR #11 opens when the roadmaps and the rebuild are complete (D-489).
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #11: two of twelve area files are done. Next comes `area-ci.md`, then the other nine area files in the order of D-485. Then come the five phase files and the rebuild of sections 7 and 8 (D-488).

### Traps and gotchas

- The rebuild changes more than sections 7 and 8, because D-496 moves det-lint to PR-46 and the night gate to PR-49. These texts still name the old PRs: G-22 in `docs/design.md`, lines 130, 145, and 147 of `CLAUDE.md` and `AGENTS.md`, and lines 12 and 15 of the PR template. Change them in the rebuild, not before.
- Sections 7 and 8 of `docs/design.md` still show PR-6 whole, PR-4 with det-lint, PR-15 with the night gate, and PR-34 with the PNG code and the normal maps.
- New PR ids so far: PR-43 to PR-53, eleven of about 20 (D-486). The next id is PR-54. The owner took every split in this block, so the count can pass 20. Show running counts in each batch.
- Contract questions for `area-ci.md`, from Session 27 and this session:
  - Where the exported Game reads its content files, and where the host code that reads content files lives.
  - The PR id of the export job of D-449, which PR-45 follows (D-492).
  - The PR and the package of the coverage report of D-174 (G-13).
  - How the `replay-identity` job compares the CI legs: committed hashes, or a compare across legs.
  - Where the night record lives, and how the `night-gate` check finds it (D-500).
  - The job for the few hundred bot runs on each PR (D-64).
- A detail question for PR-1 belongs in `area-ci.md`: `CLAUDE.md` names xUnit, and no decision row justifies a test package (G-13). On 2026-09-14 the NuGet API listed xunit.v3 4.0.1 under Apache-2.0.
- `area-art.md` must settle how the owner sees an image batch in a PR (G-25). The swatch sheet, the atlas, the map preview, and the normal-map preview are PNG files. Check whether `gh` can put an image in a PR description before a question assumes it.
- `area-audio.md` must settle how the listen command of D-439 plays sound from a console program.
- OQ-70: det-lint needs the Godot assembly to read Game types, so PR-46 needs a Game build or the Godot package.
- A page summary can misstate a version. Check each package fact against the NuGet API at `api.nuget.org/v3-flatcontainer/<id>/index.json`.
- The next ids are D-503, OQ-75, F-40, L-16, G-26, PR-54, M-7, and Session 29.

### Open questions that block progress

None for PR #11. OQ-67 to OQ-74 block PR-2, PR-3, PR-46, PR-47, PR-48, and PR-15. OQ-60 to OQ-66 block PR-4, PR-5, PR-6, and PR-43. OQ-57 and OQ-59 block the store page at Gate 2, OQ-58 blocks PR-40, and OQ-3 waits for PR-3.

### Next concrete action

A session continues PR #11 on `docs/pr-11-roadmaps`. It reads `docs/roadmaps/area-core.md`, `docs/roadmaps/area-tools.md`, D-491 to D-502, and the Phase 1 entries of `docs/design.md`. Then it writes `docs/roadmaps/area-ci.md`, asks the contract questions above, and files detail questions with their PRs (D-487, D-488).

## Session 27: 2026-09-14, Claude Code

Author: Claude Code
Session: the first area file of PR #11, `docs/roadmaps/area-core.md`, on branch `docs/pr-11-roadmaps`.

### What this session did, and why

- Session 26 (Codex) gave PR #10 the verdict `Ready for owner merge` at `7eb2abc`. The owner merged PR #10 as `63803d9`.
- The session started PR #11 on the new branch `docs/pr-11-roadmaps` from `63803d9`, and it wrote an area file first (D-488).
- The session read the design, the whole decision register, the questions register, the skills, and one focused roadmap of what-you-carry, for its document shape alone (D-277).
- The session asked four contract questions (D-487), and the owner took each recommendation:
  - PR-6 splits into three PRs. PR-6 keeps the loop, the intents, the run record, and replay. PR-43 takes the save files, and PR-44 takes crash files and log files (D-491).
  - PR-6 adds the debug seam, and PR-45 creates the debug assembly right after PR-7 (D-492).
  - The run record holds intents, with no device kind (D-493).
  - A sixth project, `TheThingBelow.Storage`, holds the file code (D-494).
- A fifth question came up while the file took shape, and the owner took the recommendation: the content hash covers the rule files alone (D-495).
- The session read eight sources on .NET, Git for Windows, the GitHub runner image, and git before a fact entered a document. F-35 records that the .NET hash classes call OS libraries and that a string hash code can change between runs. F-36 records that System.Text.Json uses reflection by default.
- A check found that `.gitattributes` already sets `eol=lf` from PR #1, so the Windows CI leg needs no finding for line ends. The area file cites the rule beside the default of Git for Windows.
- The session filed seven detail questions for PR-4, PR-5, PR-6, and PR-43 (OQ-60 to OQ-66), as D-487 asks. None blocks PR #11.
- The session wrote `docs/roadmaps/area-core.md` and added 18 revision notes to earlier rows. It updated the system map, the finding register, and a dated line in `docs/design.md`, and the `csharp-conventions` and `pr-review` skills.
- The handoff held ten entries before this one, so Session 17 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `63803d9` (PR #10).
- The branch `docs/pr-11-roadmaps` holds one commit on `main`, and its remote head is the commit that holds this entry. No PR is open, because PR #11 opens when the roadmaps and the rebuild are complete (D-489).
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #11: one of twelve area files is done. Next come `area-tools.md` and `area-ci.md`, then the other nine area files in the order of D-485. Then come the five phase files and the rebuild of sections 7 and 8 (D-488).

### Traps and gotchas

- Sections 7 and 8 of `docs/design.md` still show PR-6 whole, with no PR-43, PR-44, or PR-45. The rebuild at the end of PR #11 changes them (D-488, D-491, D-492). Do not edit them before the phase files exist.
- New PR ids so far: PR-43, PR-44, and PR-45, three of about 20 (D-486). The next id is PR-46.
- The export job of D-449 still needs its PR id in `area-ci.md`, and PR-45 comes after that job (D-492).
- Where the exported Game reads its content files is open. Godot exports a JSON file only through the filter for non-resource files, and `content/` sits outside the Game project folder (D-118). The session did not check whether an export can take a file from outside that folder. Ask it as a contract question in `area-ci.md` (D-487).
- Storage holds the file code for saves, records, crash files, and logs (D-494). The reader of content files for Tools and Tests has no home yet. Settle it with the content question above.
- D-495 keeps the string text out of the content hash. A rule file that names a string id still needs the id test of section 7.7 of the area file.
- The system map now splits the snapshot bytes of Core from the files of Storage. A later area file keeps that split.
- Every later Core PR carries the seven steps of section 7.14 of the area file. The phase files turn them into exit tests.
- The owner took every recommendation in this block. Still show running counts of PR ids and files in each batch (Session 23).
- The next ids are D-496, OQ-67, F-37, L-16, G-26, PR-46, M-7, and Session 28.

### Open questions that block progress

None for PR #11. OQ-60 to OQ-66 block PR-4, PR-5, PR-6, and PR-43. OQ-57 and OQ-59 block the store page at Gate 2, OQ-58 blocks PR-40, and OQ-3 waits for PR-3.

### Next concrete action

A session continues PR #11 on `docs/pr-11-roadmaps`. It reads `docs/roadmaps/area-core.md`, D-491 to D-495, and the Phase 1 entries of `docs/design.md`. Then it writes `docs/roadmaps/area-tools.md`, asks contract questions alone, and files detail questions with their PRs (D-487, D-488).

## Session 26: 2026-09-14, Codex

Author: Codex
Session: repeat review of PR #10 at effective head `7eb2abc`.

### What this session did, and why

- Verified the new base, effective head, changed paths, provider gate, response file, and existing PR comments.
- Read the correction diff, the complete PR diff, the design and decision contracts, the question register, the skills, the agent guidance, the handoff archive, and the PR description.
- Reproduced P2-1. D-488 now uses `Revised in part by D-490`, names PR #11 as the changed part, and keeps the writing order.
- Confirmed D-490 names D-488 among the revised decisions and that the design and handoff identify PR #11.
- Confirmed the repeat Gitar check passed on `7eb2abc` with no new comment.
- Updated `docs/reviews/pr-10.md`, preserved P2-1 with its earlier evidence, and set the verdict to `Ready for owner merge` for `7eb2abc`.
- Ran the interim STE check with 0 findings, `git diff --check`, and the guidance identity check.

### State of the build

- No code exists. `main` is `4f37c99` (PR #9).
- PR #10 is open on `docs/pr-10-roadmaps`. Its effective head is `7eb2abc`.
- The interim STE check passes with 0 findings. The repeat review metadata and handoff are pushed in `e8cc765`.

### In flight

PR #10 is ready for owner merge. Then PR #11 starts the roadmaps on a new branch.

### Traps and gotchas

- D-488 and D-489 keep `PR #10` in their topic columns as dated text. Their revision notes carry the current PR number.
- The build, test, format, det-lint, replay-identity, smoke, night-gate, and review-gate checks do not exist until the PRs named in `AGENTS.md` create them.
- The next ids are D-491, OQ-60, F-35, L-16, G-26, PR-43, M-7, and Session 27.

### Open questions that block progress

None for PR #10. OQ-57 and OQ-59 block the store page at Gate 2, OQ-58 blocks PR-40, and OQ-3 waits for PR-3.

### Next concrete action

The owner can merge PR #10. Then a fresh session starts PR #11 with `docs/roadmaps/area-core.md`.

## Session 25: 2026-09-14, Claude Code

Author: Claude Code
Session: the answer to the review of PR #10, on branch `docs/pr-10-roadmaps`.

### What this session did, and why

- Session 24 (Codex) reviewed PR #10 at `9355d62` and gave `Changes required` with P2-1: D-488 kept the PR number that D-490 changed, and D-490 did not mark D-488 as revised in part.
- The owner asked the session to address the feedback. The session fetched the branch at `571e39e`, read the review record, and found no other open comment or thread.
- P2-1, full merit: the Effect column of D-490 now revises D-484, D-488, and D-489 in part. The note on D-488 now uses the marker `Revised in part by D-490`, names the PR number as the changed part, and keeps the order of the work.
- A search of the live documents found PR #11 as the PR of the roadmaps and the rebuild in D-484, D-488, D-489, D-490, the dated line and step 2 of section 8 in `docs/design.md`, and the handoff.
- `docs/reviews/pr-10-response.md` records the disposition.
- The handoff held ten entries before this one, because Session 24 moved Session 14 to the archive. Session 15 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `4f37c99` (PR #9).
- PR #10 is open on `docs/pr-10-roadmaps`. The commit that holds this entry is the new effective head, because it changes `docs/decisions.md`.
- The interim STE check passes with 0 findings, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #10 answers a new gitar pass on the new head. Then a Codex session runs the repeat review of PR #10 and updates `docs/reviews/pr-10.md`. The owner merges. Then PR #11 starts the roadmaps on a new branch (D-488, D-490).

### Traps and gotchas

- After a push, the first `Gitar review` request re-runs the previous head, and it can complete an existing check run again rather than start a new one. Send the second request when the dashboard updates or an old run completes again with no run on the new head.
- A reviewer session can move an old entry to the archive. Count the handoff entries before a rotation, and never assume the count.
- D-488 and D-489 keep "PR #10" in their topic column as dated text. Their revision notes carry the current PR number.
- The next ids are D-491, OQ-60, F-35, L-16, G-26, PR-43, M-7, and Session 26.

### Open questions that block progress

None for PR #10. OQ-57 and OQ-59 block the store page at Gate 2, OQ-58 blocks PR-40, and OQ-3 waits for PR-3.

### Next concrete action

This session answers the gitar pass on the new head and records it in the PR description. Then a Codex session runs the repeat review of PR #10 under the `pr-review` skill. The owner merges. Then a fresh session starts PR #11 with `docs/roadmaps/area-core.md`.

## Session 24: 2026-09-14, Codex

Author: Codex
Session: cross-provider review of PR #10 at effective head `9355d62`.

### What this session did, and why

- Verified the PR target, base, merge base, branch, effective head, changed paths, provider gate, and existing PR comments.
- Read the complete diff, the design and decision contracts, the questions register, the skills, the agent guidance, the handoff archive, and the PR description.
- Confirmed the final Gitar check passed on `9355d62` and that its one suggestion was fixed in that commit.
- Found P2-1: D-488 still names PR #10, while D-490 says D-488 binds PR #11 without a revision note for D-488.
- Ran the interim STE check with 0 findings, `git diff --check`, and the guidance identity check.
- Added `docs/reviews/pr-10.md` with the verdict `Changes required` for `9355d62`.

### State of the build

- No code exists. `main` is `4f37c99` (PR #9).
- PR #10 is open on `docs/pr-10-roadmaps`. Its effective head is `9355d62`.
- The interim STE check passes with 0 findings. The review record and handoff are pushed in `95fd404`.

### In flight

PR #10 needs the D-488 revision note and a repeat review. The owner merges after the verdict covers the new effective head.

### Traps and gotchas

- D-490 must revise D-488 in part, not only D-484 and D-489. The writing order stays unchanged, and only the PR number changes to PR #11.
- The build, test, format, det-lint, replay-identity, smoke, night-gate, and review-gate checks do not exist until the PRs named in `AGENTS.md` create them.
- The next ids are D-491, OQ-60, F-35, L-16, G-26, PR-43, M-7, and Session 25.

### Open questions that block progress

None for PR #10. OQ-57 and OQ-59 block the store page at Gate 2, OQ-58 blocks PR-40, and OQ-3 waits for PR-3.

### Next concrete action

The author adds the D-488 revision note and runs a repeat Gitar pass. Then a Codex session updates `docs/reviews/pr-10.md` for the new effective head.

## Session 23: 2026-09-14, Claude Code

Author: Claude Code
Session: the shape of the roadmaps after PR #9 merged, as docs PR #10 on branch `docs/pr-10-roadmaps`.

### What this session did, and why

- Session 22 (Codex) reviewed PR #9 at `5097e8a` with no finding. The owner merged PR #9 as `4f37c99`.
- The same harness run as Session 21 started the roadmaps docs PR (D-399). It asked OQ-56 first, and the owner chose no change to the sequence (D-483).
- The session read every text that sends work to the roadmaps. The list holds PR ids for the export job, the store page work, the credits roll, the trailer capture, the debug assembly, the sound room, and the mood cues. It also holds the items of the technical, graphics, UI, and systems roadmaps.
- The owner set the shape of the work:
  - Two docs PRs for the roadmaps work: one for the roadmaps and the rebuild of sections 7 and 8, and a later one for the design-critic pass (D-484).
  - Twelve area files and five phase files in `docs/roadmaps/`, with the names in D-485.
  - One new PR id per system or tool, about 20, from PR-43 (D-486).
  - The roadmaps ask contract questions, and they file detail questions with the PR they block (D-487).
  - Areas first, then phases, then the rebuild (D-488). The roadmaps PR opens when its work is complete (D-489).
- The owner asked whether every document was current, and why no PR was open. The session quoted D-489, and the owner chose to merge the shape now as PR #10 (D-490). The roadmaps move to PR #11, and the critic pass to PR #12.
- The session brought the design current: a dated line, the file set in section 7, and step 2 of section 8. Notes on D-399, D-484, D-488, and D-489 record the later answers. No roadmap file exists yet, and the area files wait for a fresh context.
- The gitar pass on `05dd81a` approved with one suggestion, which had merit: the D-485 row sat after D-487. The commit that holds this revision of the entry moves the row between D-484 and D-486, and the reply on the thread names it.
- The handoff held eleven entries before this one, because Session 22 added its entry and moved none. Sessions 13 and 12 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `4f37c99` (PR #9).
- PR #10 is open on `docs/pr-10-roadmaps` with D-483 to D-490. The remote head is the commit that holds this entry.
- The interim STE check passes with 0 findings.

### In flight

PR #10 answers the gitar pass. It adds decision rows, so a Codex session reviews it, and no `review-override` label applies (D-401). The owner merges. Then PR #11 starts the roadmaps on a new branch (D-488, D-490).

### Traps and gotchas

- D-484 and D-490 put the design-critic pass in PR #12. Do not run it in PR #10 or PR #11.
- D-487: in PR #11, ask a question only when it changes the order, a gate, or a contract between PRs. File each detail question in `docs/questions.md` with the PR it blocks.
- New PR ids start at PR-43 (D-486). PR-22 and PR-32 stay retired (G-10).
- Each file follows the focused roadmap template of the `design-doc-style` skill: the status header and sections 1, 5, 7, 8, and 9. Each phase entry lists its scope, exit tests, review focus, filed questions, and area file (D-144).
- The owner chose finer shapes than the session recommended twice in this block: two PRs, and twelve area files. Show running counts of files and PR ids in each batch.
- D-489 now binds PR #11: push its branch at each session end, and open it when the roadmaps and the rebuild are complete (D-490). PR #11 needs a branch of its own, such as `docs/pr-11-roadmaps`.
- The next ids are D-491, OQ-60, F-35, L-16, G-26, PR-43, M-7, and Session 24.

### Open questions that block progress

None for PR #10. OQ-57 and OQ-59 block the store page at Gate 2, OQ-58 blocks PR-40, and OQ-3 waits for PR-3.

### Next concrete action

This session answers the gitar pass on PR #10. Then a Codex session reviews PR #10 under the `pr-review` skill and writes `docs/reviews/pr-10.md` (D-401). The owner merges. Then a session starts PR #11 on a new branch. It reads D-144, D-145, D-484 to D-490, the `design-doc-style` skill, and section 7 of `docs/design.md`, and it writes `docs/roadmaps/area-core.md` first (D-488).

## Session 22: 2026-09-14, Codex

Author: Codex
Session: cross-provider review of PR #9 at effective head `5097e8a`.

### What this session did, and why

- Verified the PR target, base, merge base, branch, effective head, changed paths, provider gate, and all existing PR comments.
- Read the complete diff, the release entries of the design and decision documents, the questions register, the changed skills and guidance, the runbook, the world note, the handoff archive, the PR description, and prior review records.
- Checked D-480, D-481, and D-482 against their revision notes. The release facts, the aspect ratios, the supported targets, the macOS export form, and the sequence agree.
- Found no in-scope finding. The review record is `docs/reviews/pr-9.md` with the verdict `Ready for owner merge` for `5097e8a`.

### State of the build

- No code exists. `main` is `f4a1c6b` (PR #8).
- PR #9 is open on `docs/pr-9-release-block`. The effective head is `5097e8a`. This entry and the review record are metadata.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #9 waits for the owner to merge. The roadmaps docs PR follows (D-399).

### Traps and gotchas

- D-184 excludes only `docs/reviews/`, `docs/session-handoff.md`, and `docs/session-handoff-archive.md`. The effective head is `5097e8a`, not this metadata commit.
- D-481 supersedes the arm64 Windows and Linux exports and their five CI legs. D-482 keeps a universal macOS export while supporting Apple silicon alone.
- GitHub API calls and `git fetch origin` hit environment errors during this review. The PR metadata and existing remote-tracking refs still identified the reviewed commits and Gitar result.
- The build, test, format, det-lint, replay-identity, smoke, night-gate, and review-gate checks do not exist until the PRs named in `AGENTS.md` create them.
- The next ids are D-483, OQ-60, F-35, L-16, G-26, PR-43, M-7, and Session 23.

### Open questions that block progress

None for PR #9. OQ-56 waits for the roadmaps PR. OQ-57 and OQ-59 block the store page at Gate 2. OQ-58 blocks PR-40. OQ-3 waits for PR-3.

### Next concrete action

Push this review record and handoff entry. Then the owner can merge PR #9. The next session starts the roadmaps docs PR and asks OQ-56 first.

## Session 21: 2026-09-14, Claude Code

Author: Claude Code
Session: the release block of the full plan, and two aspect ratios, on branch `docs/pr-9-release-block`.

### What this session did, and why

- Session 20 (Codex) reviewed PR #8 at `511203c` with no finding. The owner merged PR #8 as `f4a1c6b` and asked what comes next.
- The handoff and D-399 put the release block docs PR next. The first lever of OQ-56 moves that PR after the first playable, so the session asked first. The owner kept the order (D-447).
- Three read-only research agents read Steamworks, Apple, Microsoft, GitHub, and Godot pages. The session fetched each key page again and checked the quotes before a fact entered a document.
- The release block ran in twelve batches, D-447 to D-480:
  - Versions and builds: 0.MINOR.PATCH until 1.0.0, and exports on every merge from PR-7 (D-448, D-449). The owner first added arm64 builds and arm64 CI legs (D-464, D-474).
  - Signing: macOS notarized on Steam from PR-40, and Windows unsigned (D-455, D-463). F-32 records the Apple fee that the cost model lacked.
  - GitHub: prologue tags alone on GitHub Releases, until the Steam demo (D-457, D-470). The repository goes private before paid content (D-456).
  - Steam: the native Linux build on the Deck, the rating Verified, engine input with one Steamworks call for glyphs, and Auto-Cloud on the folder `the-thing-below` (D-458 to D-461, D-465). PR-40 picks the binding (D-462, OQ-58).
  - Store: the store page at Gate 2, store text and capsule grids by sessions, a trailer from replays, one Next Fest, and the demo name "The Thing Below: Prologue" (D-452, D-471, D-472, D-475, D-476, D-478).
  - Studio and players: a studio name picked before the store page (OQ-57), a studio mark on the splash, credits in three places, crash files to a studio email, and trusted players after Gate 4 (D-450, D-451, D-467 to D-469, D-473). Achievements come with the full game alone (D-466).
  - The AI disclosure of the Steam content survey waits for OQ-59, before the store page review at Gate 2 (D-477).
- Mid-block, the owner asked for a variety of aspect ratios and a revision of D-229. After four answers in a few minutes, the game supports 16:10 and 16:9 alone, with black bars on every other shape (D-480). The answer lands in this PR, and `CLAUDE.md` and `AGENTS.md` no longer list exceptions to G-8 (D-479).
- After gitar approved `980e96c` with 0 comments, the owner cut the scope to four targets: Windows and Linux on x86_64, macOS on Apple silicon, and the Steam Deck (D-481). D-481 supersedes D-464 and D-474. The macOS build stays the official universal build, and the game supports Apple silicon alone (D-482).
- F-33 records five gaps that the block closed, and F-34 records the screenshot format of Steam.
- The session updated `docs/design.md`, `docs/questions.md`, `CLAUDE.md`, `AGENTS.md`, the PR template, three skills, the dev-machine runbook, and `docs/world/setting.md`.
- The handoff held eleven entries before this one, because Session 20 added its entry and moved none. Sessions 11 and 10 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `f4a1c6b` (PR #8).
- PR #9 is open on `docs/pr-9-release-block`. The remote head is the commit that holds this entry.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #9 answers the gitar pass. It adds decision rows, so a Codex session reviews it, and no `review-override` label applies (D-401). The owner merges. Then the roadmaps docs PR starts (D-399).

### Traps and gotchas

- The PR holds two concerns on owner instruction (D-479). The description names the second concern, so a reviewer does not read it as a break of G-8.
- D-480 took several answers: a wider view, a limit at 16:9, a crop of narrow screens, then 16:9 alone, then 16:9 and 16:10. Only the last answer is a row. Any text that names 21:9, 4:3, or a crop is stale.
- D-471 moves the Steam Direct fee, the EU and WIPO name checks, the store text, and the capsule art to Gate 2. OQ-57 and OQ-59 now block the store page at Gate 2, and OQ-57 also blocks the crash address of D-473.
- The roadmaps PR gives PR ids to the export job after PR-7, the store page work after Gate 2, the credits roll, and the trailer capture.
- The Steamworks pages do not say how Auto-Cloud settles a conflict or whether a demo app needs a fee. PR-40 checks both.
- D-464 and D-474 are superseded inside this PR. Any text that names arm64 builds for Windows or Linux, five exports, or five CI legs is stale.
- The owner often gives a custom answer that widens the scope. Ask the limits in the next batch, and confirm the final state before the rows.
- Gitar runs one pass by itself on a new PR. After a later push, post `Gitar review`, and count a pass only from a `Gitar` check run on the head.
- The next ids are D-483, OQ-60, F-35, L-16, G-26, PR-43, M-7, and Session 22.

### Open questions that block progress

None for PR #9. OQ-57 and OQ-59 block the store page at Gate 2, and OQ-58 blocks PR-40. OQ-56 waits for the roadmaps PR, and OQ-3 waits for PR-3.

### Next concrete action

This session answers the gitar pass on PR #9. Then a Codex session reviews PR #9 under the `pr-review` skill and writes `docs/reviews/pr-9.md` (D-401). The owner merges. Then a session starts the roadmaps docs PR, asks OQ-56 first, and rebuilds sections 7 and 8 of `docs/design.md` (D-399).

## Session 20: 2026-09-14, Codex

Author: Codex
Session: cross-provider review of PR #8 at effective head `511203c`, on branch `docs/pr-8-docs-current`.

### What this session did, and why

- Verified the PR target, base, merge base, branch, effective head, changed paths, author provider, and the automated pass.
- Confirmed the provider gate. Session 19 identifies Claude Code as the author of the substantive changes, and Codex is the eligible reviewer.
- Read the complete diff, the design sequence, the decision and question registers, the project guidance, the handoff archive, and the PR description.
- Checked D-446 against D-442 and D-193. The partial revision leaves the other time rules of D-442 current.
- Confirmed that `AGENTS.md` and `CLAUDE.md` stay identical, the handoff has ten current sessions, and Sessions 9 and 8 moved word for word to the archive.
- Wrote `docs/reviews/pr-8.md` with no finding and the verdict `Ready for owner merge` for `511203c`.

### State of the build

- No code exists. `main` is `cf2b197` (PR #7).
- PR #8 is open on `docs/pr-8-docs-current`. The remote head is the review commit that holds this entry and `docs/reviews/pr-8.md`.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `AGENTS.md` and `CLAUDE.md` are identical.

### In flight

PR #8 is ready for the owner to merge. OQ-56 blocks only the roadmaps PR's rebuild of section 8. The release block docs PR follows this PR (D-399).

### Traps and gotchas

- The effective head is `511203c`, not the later metadata commit that publishes the review record (D-184).
- D-446 revises the rule for wrong things only. D-442 still sets the time of day for maps, and D-443 still governs night versions of place music.
- The build, test, format, det-lint, replay-identity, smoke, night-gate, and review-gate checks do not exist yet. The PRs named in `AGENTS.md` create them.
- OQ-3 remains open for branch protection and does not block this documentation PR.

### Open questions that block progress

OQ-56 waits for the roadmaps PR. OQ-3 waits for PR-3.

### Next concrete action

The owner merges PR #8. Then a session starts the release block docs PR and reads D-53, D-85, D-93, D-143, and the Phase 5 entries of `docs/design.md` before it asks the release questions.

## Session 19: 2026-09-14, Claude Code

Author: Claude Code
Session: the audit of every document before a context reset, after PR #7 merged, on branch `docs/pr-8-docs-current`.

### What this session did, and why

- Session 18 (Codex) reviewed PR #7 at `138e5cf` with no finding and the verdict `Ready for owner merge`. The owner merged PR #7 as `cf2b197`.
- The owner asked: "Ensure ALL docs are up to date in preparation for context reset."
- A search of the live documents for the old clock, the old audio rules, and the state of PR #7 found two stale texts:
  - `CLAUDE.md` and `AGENTS.md` named PR #2 as the one exception to G-8, but D-437 made PR #7 a second exception.
  - Step 2 of section 8 in `docs/design.md` did not show PR #6 and PR #7 as merged.
- Two owner items lived only in the conversation, and a reset would lose them. The session asked both:
  - The reading of D-442 on the wrong things. The owner chose placement by the story, at any time of day (D-446). D-442, D-193, and the effect of D-414 gained notes.
  - The levers for an earlier playable build. The owner filed them as OQ-56 for the roadmaps PR.
- The skills, the agents, the runbooks, the README, the PR template, and the world files hold no stale text. There, "clock" means the wall clock, and "phase" means a roadmap phase or a boss phase.
- `docs/design.md` gained a dated line for this pass.
- The handoff held eleven entries before this one, because Session 18 added its entry and moved none. Sessions 9 and 8 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `cf2b197` (PR #7).
- PR #8 is open on `docs/pr-8-docs-current`. The remote head is the commit that holds this entry.
- The interim STE check passes with 0 findings, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #8 answers the gitar pass. It adds D-446, so it takes a Codex review, and no `review-override` label applies (D-401). The owner merges. Then the release block docs PR starts (D-399).

### Traps and gotchas

- The push line of `docs/reviews/pr-7.md` keeps the placeholder `<review metadata sha>`. The review commit is `a0cf252`. The record belongs to the reviewer, so this session left it as it is.
- The wrong things have no time rule now (D-446). Outside the dated records and the superseded rows, a text that says they walk after dusk or at night is stale.
- OQ-56 blocks only the rebuild of section 8 in the roadmaps PR. The release block docs PR comes first, unless the owner answers OQ-56 sooner.
- Gitar runs one pass by itself on a new PR. After a later push, post `Gitar review`. When a new dashboard appears with no `Gitar` check run on the head, post the second request within a minute.
- In the audio block, the owner often picked the fullest option, then cut scope for cost. Show the running count of tracks, light setups, or tests in each batch.
- The next ids are D-447, OQ-57, F-32, L-16, G-26, PR-43, M-7, and Session 20.

### Open questions that block progress

None for PR #8. OQ-3 waits for PR-3. OQ-56 waits for the roadmaps PR.

### Next concrete action

This session answers the gitar pass on PR #8. Then a Codex session reviews PR #8 under the `pr-review` skill and writes `docs/reviews/pr-8.md` (D-401). The owner merges. Then a session starts the release block as its own docs PR (D-262, D-399). It reads D-53, D-85, D-93, D-143, and the Phase 5 entries of `docs/design.md`, then asks the owner the release questions in batches.

## Session 18: 2026-09-14, Codex

Author: Codex
Session: review of PR #7 at effective head `138e5cf`, on branch `docs/pr-7-audio-block`.

### What this session did, and why

- Read the handoff, the project instructions, the `pr-review` and `ste-writing` skills, the design, decisions, questions, world, review, and PR documents.
- Verified the provider gate: Claude Code authored the substantive PR changes, and Codex reviewed them.
- Reviewed the complete PR diff. The audio decisions, the time-of-day supersession chain, the roadmap entries, the glossary, and the document dispositions agree at `138e5cf`.
- Verified the Gitar comment. The added D-190 note in `138e5cf` completes the back-reference to D-436.
- The effective head is `138e5cf`, not `f8ca3c3`, because `138e5cf` changes `docs/decisions.md`, which is outside the metadata set.
- The review record is `docs/reviews/pr-7.md`. It has no finding and gives the verdict `Ready for owner merge` for `138e5cf`.

### State of the build

- No code exists. `main` is `11498f1` (PR #6).
- PR #7 is open on `docs/pr-7-audio-block`. The remote head is the review commit that holds this entry and `docs/reviews/pr-7.md`.
- The interim STE check passes with 0 findings, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #7 waits for the owner to merge. Then the release block docs PR starts, followed by the roadmaps docs PR (D-399).

### Traps and gotchas

- A commit that changes `docs/decisions.md` is substantive. The effective head rule excludes only `docs/reviews/`, `docs/session-handoff.md`, and `docs/session-handoff-archive.md`.
- The Gitar dashboard comment is not the only evidence of a pass. The Gitar check passed, but `gh pr checks` also reported a GitHub API connection error.
- The build, test, format, Godot, and later gate checks do not exist until the PRs named in `docs/design.md` create them.

### Open questions that block progress

None for PR #7. OQ-3 waits for PR-3. The EUIPO, TMview, and WIPO checks wait for PR-40.

### Next concrete action

The owner merges PR #7. Then a session starts the release block docs PR.

## Session 17: 2026-09-14, Claude Code

Author: Claude Code
Session: the audio block of the full plan, and the change to a time of day that the story sets, on branch `docs/pr-7-audio-block`.

### What this session did, and why

- PR #6 merged as `11498f1`. On the owner instruction "continue work", the same harness run started the audio block, the next docs PR of D-399.
- The owner asked when a build to play and feel comes. From section 7 of `docs/design.md`: PR-7 is the first build to walk in, PR-10 and PR-11 give fights, and PR-17 is the first playable.
- PR-17 comes after three docs PRs and 23 code PRs. At 2 to 4 sessions per reviewed PR, that is about 50 to 100 sessions.
- The session named three levers for the roadmaps PR: the release docs PR later, PR-2 and PR-3 after PR-7, or a throwaway feel prototype. The owner chose none yet.
- The audio block ran in eight batches:
  - Style: 16-bit synthesized instrument voices for the music and the sound effects (D-412, D-423). D-87 keeps our own tool and no licensed sound.
  - Music everywhere: place tracks, three battle tracks per region, mood cues and key cues for scenes, and ten themes in region one (D-413, D-415, D-418, D-419).
  - Stings for a wipe, a level up, a victory, and a key find. Ambience under the music, map sounds, a sound family per kind, and soft menu sounds (D-422, D-424 to D-426, D-431).
  - The main theme on the title screen and at the end of region one (D-427). The place music plays on under menus, and after a battle the ambience plays alone before the track resumes (D-421, D-429).
  - The build renders the audio, and the repository commits a hash list, not WAV files (D-432, F-31). Sessions write the music as tracker rows, and the owner hears each batch with a listen command, and later in a sound room (D-433, D-438, D-439).
  - Vibration at heavy moments alone, and four more audio settings (D-434, D-435).
- Mid-block, the owner stopped time in dungeons, and set rest and travel rules (D-436, D-440, D-441). Minutes later, the owner removed the day clock: the story sets the time of day of each map (D-442).
- D-442 supersedes D-190, D-192, D-197, D-198, D-436, D-440, and D-441. D-443 and D-444 cut the night versions of the music, and D-445 removes the sun or moon mark from the HUD.
- The owner put the clock answer in this PR, so the PR holds two concerns (D-437, G-8).
- The session stated three readings. The owner kept two: ambush and elite fights play the common battle track (D-415), and the refuge counts as a cave and the sealed gallery as a mine (D-417). The third has no answer yet: the wrong things walk only on a map set to dusk or night (D-442).
- The session updated `docs/design.md`: a dated line, the system map, F-31, the cost model, PR-7, PR-8, PR-18, PR-33, PR-36, PR-38, and step 2 of section 8.
- It also updated the glossary of the `ste-writing` skill, the open items of `docs/world/setting.md`, and OQ-10 in `docs/questions.md`, and it added revision notes to 14 earlier rows.
- The handoff held ten entries before this one, so Session 7 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).
- The gitar pass on `f8ca3c3` approved with one suggestion: 1 comment, with merit. D-190 lacked its note for D-436, and the commit that holds this revision of the entry adds it. A script check found no other target row of D-412 to D-445 without its note.

### State of the build

- No code exists. `main` is `11498f1` (PR #6).
- PR #7 is open on `docs/pr-7-audio-block`. The remote head is the commit that holds this entry.
- The interim STE check passes with 0 findings, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #7 answers the gitar pass. It adds decision rows, so no `review-override` label applies, and a Codex session reviews it (D-401). The owner merges. Then the release block docs PR, then the roadmaps docs PR (D-399).

### Traps and gotchas

- The PR holds two concerns on owner instruction (D-437). The PR description cites D-437, so a reviewer does not read the second concern as a break of G-8.
- D-442 ends the day clock. Outside the dated records, a text that names a phase of the day, the day clock, or a wait at a save point is now stale. Roadmap phases and boss phases stay. The glossary sets "time of day".
- Five rows of this PR are superseded inside the same PR: D-416, D-420, D-436, D-440, and D-441. They stay as a dated record, as D-395 and D-396 did.
- Region one needs about 20 tracks. Each night beat that the arc adds can need a night version of a place track (D-443).
- The synthesizer must render the same bytes on all three platforms, so it needs integer math (D-432). The PR-38 text carries that.
- The roadmaps PR gives PR ids to the rest of the audio player, the first music, and the sound room (PR-38, D-399).
- In this block, the owner often picked the fullest option, then cut scope for cost. Show the running count of tracks, light setups, or tests in each batch.
- The next ids are D-446, OQ-56, F-32, L-16, G-26, PR-43, M-7, and Session 18.

### Open questions that block progress

None for PR #7. OQ-3 waits for PR-3. The levers for an earlier playable build wait for the owner, and the roadmaps PR is their place.

### Next concrete action

This session answers the gitar pass on PR #7. Then a Codex session reviews PR #7 under the `pr-review` skill and writes `docs/reviews/pr-7.md` (D-401). The owner merges. Then a session starts the release block as its own docs PR (D-399).

## Session 16: 2026-09-14, Claude Code

Author: Claude Code
Session: the first session in the new checkout `/Volumes/SSD-1TB/the-thing-below`, and a docs PR that closes `docs/runbooks/rename-and-move.md`, on branch `docs/pr-6-close-move`.

### What this session did, and why

- PR #5 merged as `aee6f35` at 17:09:52Z, with the `review-override` label. The Session 15 entry went in before the merge, so it does not record what came after.
- After the merge, Session 15 ran two steps of the runbook:
  - Step 9: the clone to `/Volumes/SSD-1TB/the-thing-below`, clean at `aee6f35`.
  - Step 10: the copy of the local session notes from `~/.claude/projects/-Users-nate-Repos-terminal-rpg/memory/` to `~/.claude/projects/-Volumes-SSD-1TB-the-thing-below/memory/`.
- Step 11: this session opened in the new checkout. The interim STE check passed with 0 findings. `diff -r` of the two notes folders found no difference, and the session read its notes from the new folder.
- Before step 12, the session checked that the old checkout `~/Repos/terminal-rpg` held nothing that GitHub lacks:
  - The tree of its last branch tip `152fa62` is the tree of `aee6f35`.
  - `git ls-remote` shows each of its five local branch tips on GitHub, as `refs/pull/1/head` to `refs/pull/5/head`.
  - It had no stash, no untracked or ignored file, no `.claude/settings.local.json`, and no hook.
- Step 12: the owner deleted the old checkout. A check at 17:24:12Z found no folder at that path.
- The docs PR makes the documents show the move as complete:
  - `docs/runbooks/rename-and-move.md`: the status is complete, and steps 7 to 12 are marked done.
  - `docs/design.md`: a dated line for the move pass, and step 2 of section 8 shows PR #5 merged and the checkout on the SSD.
  - `docs/runbooks/dev-machine.md`: a dated fact for the checkout path.
- The PR changes no decision row. The owner answer on step 12 carries out a step that D-216 and D-400 already set, so it adds no row (D-68).
- The handoff held ten entries before this one, so Session 6 moved word for word to the top of `docs/session-handoff.md` (D-18).

### State of the build

- No code exists. `main` is `aee6f35` (PR #5).
- The checkout is `/Volumes/SSD-1TB/the-thing-below`. The old checkout no longer exists.
- PR #6 is open on `docs/pr-6-close-move`. The remote head is the commit that holds this entry.
- The interim STE check passes with 0 findings, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #6 answers the gitar pass. It changes no decision row, and every path is in the override set, so the session applies the `review-override` label after the pass approves the head (D-67, D-401). The owner merges. Then the audio block docs PR starts (D-399).

### Traps and gotchas

- The checkout is on the external SSD. A session cannot open it when the Mac does not show `/Volumes/SSD-1TB`.
- The local session notes key on the checkout path, now `~/.claude/projects/-Volumes-SSD-1TB-the-thing-below/memory/`. The old folder `-Users-nate-Repos-terminal-rpg` still exists, and no session reads it now.
- Gitar runs one pass by itself when a new PR opens, even while the automatic passes are paused (PR #5).
- After a later push, post `Gitar review` two times. The first request runs the pass on the older head, and the second runs it on the new head (Session 13).
- Count a pass only when a `Gitar` check run on the head commit ends. The dashboard comment is not proof.
- The dated records keep `terminal-rpg` and `~/Repos/terminal-rpg` on purpose.
- The next ids are D-412, OQ-56, F-31, L-16, G-26, PR-43, M-7, and Session 17.

### Open questions that block progress

None for PR #6. OQ-3 waits for PR-3.

### Next concrete action

This session answers the gitar pass on PR #6, and applies the `review-override` label when the pass approves the head. The owner merges. Then a session starts the audio block, the next docs PR (D-262, D-399). It reads the audio rows first: D-87, D-115, D-223, D-226, and PR-38 in `docs/design.md`. Then it asks the owner the audio questions in batches and records each answer. That PR adds decision rows, so the other provider reviews it (D-401).

## Session 15: 2026-09-14, Claude Code

Author: Claude Code
Session: the rename PR, steps 4 to 7 of `docs/runbooks/rename-and-move.md`, on branch `docs/pr-5-rename`.

### What this session did, and why

- PR #4 merged as `a16a83e` after the repeat review of Session 14. No other PR was open, so step 4 of the runbook started (D-411).
- The session started `docs/pr-5-rename` from `main`. The next GitHub number was 5.
- A search of every tracked file found 41 mentions of the working title. The dated records keep theirs: `docs/archive/`, the handoff, and the rows of D-9, D-72, and D-102 (step 6).
- The live documents now use the tentative name The Thing Below, and the commands use the names of D-217:
  - `CLAUDE.md` and `AGENTS.md`: the title, the sentence on the project names, and 10 command names each. The two files stay identical.
  - `README.md`: the title, and "The name is tentative."
  - `docs/design.md`: the title, the thesis, step 2 of section 8, and a dated line for the rename pass.
  - The `csharp-conventions` and `ste-writing` skills: the command names, and the technical name of the game.
  - `docs/runbooks/rename-and-move.md`: the status, and steps 4 to 6 marked done.
- The runbook keeps the old names in its table of names and in step 1, because those lines record the change.
- OQ-7 already names D-215, D-217, and D-410, so the questions register needs no note. No owner question came up for this PR (D-68), and the PR changes no decision row.
- The handoff held eleven entries before this one, because Session 14 added its entry and moved none. Sessions 5 and 4 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `a16a83e` (PR #4).
- PR #5 is open on `docs/pr-5-rename`. The remote head is the commit that holds this entry.
- The interim STE check passes with 0 findings, and `CLAUDE.md` and `AGENTS.md` stay identical.
- The local checkout is still `~/Repos/terminal-rpg`.

### In flight

PR #5 answers the gitar pass. It changes no decision row, and every path is in the override set, so the session applies the `review-override` label after the pass approves the head (D-67, D-401). The owner merges. Then steps 9 to 12 of the runbook follow: the clone to the SSD, the copy of the session notes, a new session in the new checkout, and the removal of the old checkout by the owner.

### Traps and gotchas

- Post `Gitar review` after each push, and count the pass only from a `Gitar` check run on the head (Session 13). A request runs the pass on the PR head at the previous request, so a second request can be necessary.
- The label needs a new approval after each push (D-67).
- Step 10 copies `~/.claude/projects/-Users-nate-Repos-terminal-rpg/memory/` to the folder of the new path, probably `-Volumes-SSD-1TB-the-thing-below`. Check the folder name after the first session in the new checkout.
- The dated records and the table of names in the runbook keep `terminal-rpg` on purpose. A search for the old name finds them.
- The next ids are D-412, OQ-56, F-31, L-16, G-26, PR-43, M-7, and Session 16.

### Open questions that block progress

None for PR #5. OQ-3 waits for PR-3.

### Next concrete action

This session answers the gitar pass on PR #5 and applies the `review-override` label when the pass approves the head. The owner merges. Then a session runs steps 9 to 12 of `docs/runbooks/rename-and-move.md`.

## Session 14: 2026-09-14, Codex

Author: Codex
Session: repeat review of PR #4 at effective head `919a865`.

### What this session did, and why

- Read the current handoff, the prior review, the response file, the new substantive diff, and the current PR metadata.
- Verified the provider gate remains eligible. Claude Code authored the changes, and Codex reviewed them.
- Reproduced both prior corrections. The design-doc skill now matches the numbered sections of `docs/design.md`, and the name-search facts now include sources, queries, results, and dates.
- Confirmed the automated pass completed successfully on `919a865` with no issue comments.
- Updated `docs/reviews/pr-4.md`, preserved P2-1 and P2-2 with their earlier evidence, and set the verdict to `Ready for owner merge` for `919a865`.

### State of the build

- No code exists. `main` is `d29921d` (PR #3).
- PR #4 is open. Its effective head is `919a865`; later commits contain metadata only.
- The interim STE check passes with 0 findings, `git diff --check` is clean, and `AGENTS.md` and `CLAUDE.md` remain identical.

### In flight

PR #4 is ready for owner merge. After merge, step 4 of the rename runbook starts the rename PR (D-411).

### Traps and gotchas

- The live `gh pr checks` call returned a GitHub API connection error during this review. The handoff records the successful Gitar check run on `919a865`.
- The review applies to `919a865`, not the later handoff-only tip `09ace6c`.
- OQ-3 remains open for branch protection.

### Open questions that block progress

None for PR #4.

### Next concrete action

The owner can merge PR #4. Then run step 4 onward of `docs/runbooks/rename-and-move.md`.

## Session 13: 2026-09-14, Claude Code

Author: Claude Code
Session: the gitar pass on the answer to the review of PR #4, on branch `docs/pr-4-docs-audit`.

### What this session did, and why

- Session 12 pushed `919a865` at 15:02:16Z. The pass on that head had no result when that session stopped, so this session read the result on GitHub.
- The repository activity API shows no push after `919a865`. Three `Gitar review` requests came after that push, the last at 15:36:57Z.
- Gitar edited its dashboard comment at 15:37:15Z. The comment says approved, with no issue found. The `Gitar` check run on `919a865` ended with `success` at 15:37:22Z.
- The PR holds 0 review threads, 0 line comments, and 0 reviews. The pass on `919a865` has 0 comments, 0 with merit, and no fix commit.
- The pass is complete (the `pr-review` skill, "The automated pass"). The PR adds decisions, so no `review-override` label applies (D-401).
- The handoff held ten entries, so Session 3 moved word for word to the top of `docs/session-handoff-archive.md` (D-18).
- After the first push of this entry as `92926a6`, the session requested a pass with `Gitar review` at 15:46:41Z, 41 seconds after the push.
- Gitar ran the pass on `919a865` again, not on `92926a6`. The check run on `919a865` started at 15:46:46Z and ended with `success` at 15:47:22Z.
- Gitar deleted its dashboard comment and posted a new one at 15:47:20Z. The new comment says approved and repeats the old summary word for word.
- The check suite of gitar on `92926a6` stayed `queued`, with 0 check runs. The poll of the session waited for a check run on `92926a6`, and it failed at its time limit.
- The owner saw the new dashboard comment first. The session at first read it as a pass on `92926a6`, then the check runs showed the old head.
- This revision of the entry corrects the traps on the result of a pass. The PR description records the result of each pass on the tip.
- The session pushed that revision as `4ce3138` at 16:08:16Z. It waited 5 minutes, then requested a pass at 16:13:17Z.
- Gitar ran the pass on `92926a6`, not on `4ce3138`. The check run on `92926a6` started at 16:13:22Z and ended with `success` at 16:13:58Z.
- The poll saw the new dashboard comment with no check run on `4ce3138`, and it reported that at once.
- The check runs of every request fit one rule: a request runs the pass on the commit that was the PR head at the request before it.
- This third revision of the entry records that rule in the traps.

### State of the build

- No code exists. `main` is `d29921d` (PR #3).
- PR #4 is open. The remote head is the commit that holds this entry, above `919a865`.
- The effective head stays `919a865`, because the commit that holds this entry changes the handoff files alone (the `pr-review` skill).
- The gitar pass on `919a865` is complete. The PR description records the pass on the tip that holds this entry.
- The interim STE check passes with 0 findings.

### In flight

PR #4 waits for a gitar pass on the tip that holds this entry. Then a Codex session runs the repeat review of `919a865` (the `pr-review` skill, "Repeat review procedure"). The owner merges. Then step 4 of the rename runbook starts the rename PR (D-411).

### Traps and gotchas

- Count a gitar pass only when a `Gitar` check run on the head commit starts after the request and ends. `gh api repos/{owner}/{repo}/commits/<sha>/check-runs` reads it.
- The dashboard comment is not proof. Requests 3 seconds, 41 seconds, and 5 minutes after a push ran the pass on the older commits `cb6e96e`, `919a865`, and `92926a6`. Each dashboard said approved.
- A request runs the pass on the PR head at the previous request, so a wait after a push does not help. After a push, post `Gitar review` and wait for its check run.
- Then post `Gitar review` again. That second request runs the pass on the new head.
- A check suite of gitar in the state `queued`, with 0 check runs, means that no pass ran on that commit. `2072219`, `c73c19f`, and `92926a6` show that state.
- Gitar can edit its dashboard comment or replace it with a new one. Read the newest gitar comment that contains "Code Review".
- The REST API names the bot `gitar-bot[bot]`, and `gh pr view` names it `gitar-bot`. A filter on one exact login finds nothing in the other form.
- Automatic passes of gitar are paused for the period. Post `Gitar review` after each push, and wait for the check run on the head.
- The next ids are D-412, OQ-56, F-31, L-16, G-26, PR-43, M-7, and Session 14.

### Open questions that block progress

None for PR #4. OQ-3 waits for PR-3.

### Next concrete action

This session gets a `Gitar` check run on the tip that holds this entry, and the PR description records it. Then a Codex session runs the repeat review of PR #4 at `919a865` and updates `docs/reviews/pr-4.md`. The owner merges. Then a session runs step 4 onward of `docs/runbooks/rename-and-move.md`.

## Session 12: 2026-09-14, Claude Code

Author: Claude Code
Session: the answer to the review of PR #4, on branch `docs/pr-4-docs-audit`.

### What this session did, and why

- The review in `docs/reviews/pr-4.md` gave `Changes required` at `cb6e96e`, with P2-1 and P2-2. The session pulled the two review commits first.
- P2-1, full merit: the template of the `design-doc-style` skill numbered the status header as item 1, so every section number sat one above the headings of `docs/design.md`. The status header is now unnumbered, and the list numbers 1 to 9 match the headings.
- P2-2, full merit: the name search had no dated source in the repository. The session ran each check again and wrote the URLs, the queries, the results, and the controls into the external facts of `docs/design.md`. D-408 and the rename runbook point there.
- `docs/reviews/pr-4-response.md` records both dispositions.
- The handoff held eleven entries before this one, so Sessions 1 and 2 moved to the top of `docs/session-handoff-archive.md` (D-18).

### State of the build

- No code exists. `main` is `d29921d` (PR #3).
- PR #4 is open. The commit that holds this entry is the new effective head, above the review commits `2072219` and `c73c19f`.
- The interim STE check passes with 0 findings, and `CLAUDE.md` and `AGENTS.md` stay identical.

### In flight

PR #4 answers a new gitar pass, then takes a repeat review on the new effective head (the `pr-review` skill). The owner merges. Then step 4 of the rename runbook starts the rename PR (D-411).

### Traps and gotchas

- The USPTO search service has no public documentation. The POST body in the external facts worked on 2026-09-14, and its controls prove the `WM` field. A later change of the service can break the query.
- The EUIPO, TMview, and WIPO checks stay open for PR-40 (D-408).
- Automatic passes of gitar are paused for the period. Post `Gitar review` after each push.
- The next ids are D-412, OQ-56, F-31, L-16, G-26, PR-43, M-7, and Session 13.

### Open questions that block progress

None for PR #4. OQ-3 waits for PR-3.

### Next concrete action

This session answers the gitar pass on the new head. Then a Codex session runs the repeat review of PR #4 and updates `docs/reviews/pr-4.md`. The owner merges.

## Session 11: 2026-09-14, Codex

Author: Codex
Session: cross-provider review of PR #4 at effective head `cb6e96e`.

### What this session did, and why

- Verified the PR target, base, merge base, branch, effective head, changed paths, provider gate, and existing PR comments.
- Read the complete diff, the design roadmap, the decision and question registers, the changed skills and agent files, the runbooks, the world files, and the PR description.
- Confirmed the automated pass approved the head with no issue comments. The local interim STE check passes with 0 findings, `git diff --check` is clean, and `AGENTS.md` and `CLAUDE.md` remain identical.
- Found P2-1: the changed `design-doc-style` skill gives section numbers that do not match `docs/design.md`.
- Found P2-2: the material name-search record has no dated primary-source links or repeatable query record in the repository documents.
- Wrote `docs/reviews/pr-4.md` with the verdict `Changes required` for `cb6e96e`.

### State of the build

- No code exists. `main` is `d29921d` (PR #3).
- PR #4 is open at `cb6e96e` on `docs/pr-4-docs-audit`.
- The interim STE check passes with 0 findings. No solution, Makefile, CI, review-gate, or Godot project exists yet.

### In flight

PR #4 waits for the author to correct P2-1 and P2-2, push the changes, request the automated pass, and take a repeat review at the new effective head.

### Traps and gotchas

- `docs/design.md` uses section 5 for the defect register, section 6 for guardrails, and section 7 for the roadmap. The changed design-doc skill says 6, 7, and 8.
- The repository name search remains tentative because EUIPO, TMview, and WIPO did not answer. PR-40 owns the later verification.
- The review record is metadata. A later metadata commit does not change the effective implementation head, but a substantive correction does.

### Open questions that block progress

None. OQ-3 remains open for branch protection and does not block this review.

### Next concrete action

Correct P2-1 and P2-2, run the focused checks and the interim STE check, then repeat the review on the new effective head.

## Session 10: 2026-09-14, Claude Code

Author: Claude Code
Session: the start of the rename runbook after PR #3 merged, then a docs PR that makes every document current before the rename PR, on branch `docs/pr-4-docs-audit`.

### What this session did, and why

- PR #3 merged as `d29921d`. The preconditions of `docs/runbooks/rename-and-move.md` held: no open PR, the SSD mounted, and the target path and the GitHub name free.
- A search for "The Thing Below" found no game on Steam and no United States mark. A free jam game on itch.io is called "The Thing Beneath", and its devlog once says "The Thing Below". The EU and WIPO registers did not answer. The owner chose to go ahead and to start now (D-408, D-409).
- Steps 1 to 3 of the runbook ran: the GitHub repository is `nkramber/the-thing-below`, and the local `origin` points at it. The owner's instruction "Ensure ALL docs are up to date before you rename/move repo" arrived after those steps. The owner kept the new name (D-410) and chose a current-state audit in its own docs PR before the rename PR (D-411).
- Three read-only audit agents read the design, the world files with the questions register, and the process files. Two scripts checked the revision notes of the decision register and the file paths in the documents. The session verified each finding against its source before a change.
- The fixes cover these files:
  - `docs/design.md`: the status header, the system map, F-2, F-3, F-9, T-4, PR-1, PR-6, PR-10, PR-14, PR-17, PR-34, PR-37, PR-40, the Phase 2 gate, and section 8.
  - `docs/questions.md`: nine notes.
  - `docs/world/`: three items.
  - `CLAUDE.md` and `AGENTS.md`: the override set and the Python exceptions.
  - The PR template, four skills, and one agent file.
  - Both runbooks, the docstring of `docs/tools/ste-check.py`, and the Rust block of `.gitignore`.
- D-78 gained its note for D-98. F-30 records the audit.
- Findings the session did not change: the open item on the two months after region one in `places.md` already defers to region two (D-353). PR-35 keeps "one hub and one dungeon" as the first nodes, because no decision says whether the village is a node.

### State of the build

- No code exists. `main` is `d29921d` (PR #3) on `nkramber/the-thing-below`.
- Branch `docs/pr-4-docs-audit` holds one commit above `main`, the commit that holds this entry.
- The interim STE check passes with 0 findings.
- The local checkout is still `~/Repos/terminal-rpg`. The documents keep the working title until the rename PR.

### In flight

PR #4, the docs audit, answers the gitar pass, then takes a Codex review, because it adds D-408 to D-411 (D-401). The owner merges. Then step 4 of the runbook starts the rename PR, and the clone to the SSD and the copy of the session notes follow (D-400, D-411).

### Traps and gotchas

- The GitHub repository has a new name. The old URL redirects, but set `origin` to `git@github.com:nkramber/the-thing-below.git` in any other checkout.
- The rename PR swaps the title and the project names alone. This PR already changed the facts about the GitHub repository in `docs/design.md` and `docs/runbooks/dev-machine.md`.
- The session notes of Claude Code key on the checkout path. Step 10 of the runbook copies the memory folder after the clone.
- Do not renumber the steps of `docs/runbooks/rename-and-move.md`: D-400 cites step 10 by number.
- `grep` on this machine is `ugrep`, which rejects a long bounded repeat such as `.{0,120}`. Use Python for a context search.
- Automatic passes of gitar are paused for the period. Post `Gitar review` after each push.
- The next ids are D-412, OQ-56, F-31, L-16, G-26, PR-43, M-7, and Session 11.

### Open questions that block progress

None for PR #4. OQ-3 waits for PR-3.

### Next concrete action

This session answers the gitar pass on PR #4. Then a Codex session reviews PR #4 under the `pr-review` skill and writes `docs/reviews/pr-4.md` (D-401). The owner merges. Then a session runs step 4 onward of `docs/runbooks/rename-and-move.md`.

## Session 9: 2026-09-14, Codex

Author: Codex
Session: cross-provider review of PR #3 at effective head `f684ed5`.

### What this session did, and why

- Verified the PR target, base, merge base, branch, effective head, changed paths, and all three substantive commits.
- Confirmed the provider gate. The handoff identifies Claude Code as the author, and Codex is the reviewer.
- Read the complete diff, the design roadmap, the decision and question registers, the cast file, the project guidance, the atlas script, the sample readme, the five grids, and both review sheets.
- Confirmed that the sample grids have 32 rows of 32 characters, all keys exist in the 48-color palette, and the visual sheets match the stated sample.
- Confirmed that the deleted 16 by 16 files have no broken current consumer. The retained atlas script fails with the documented contextual error until PR-34 ports it.
- Wrote `docs/reviews/pr-3.md` with the verdict `Ready for owner merge`.

### State of the build

- No code exists. `main` is `7375310` (PR #2).
- The effective head is `f684ed5`. The review commit and this handoff entry are metadata commits and do not change that head.
- The interim STE check passes with 0 findings. `git diff --check origin/main...HEAD` is clean.
- PR #3 is open. The automated pass approved the final head with zero issues. No CI or review-gate checks exist yet.

### In flight

PR #3 is ready for owner merge. After merge, the next work is the rename and move in `docs/runbooks/rename-and-move.md` (D-400).

### Traps and gotchas

- Skip `docs/samples/` during automatic exploration (D-403), except when the owner or the handoff points to it.
- PR #3 is the GitHub PR number for the sprite sample. Roadmap PR-3 is the later review-gate item.
- The interim atlas tool now fails with `no .grid file` because D-405 removed the old content. PR-34 ports the tool to 32 by 32 grids.
- Automatic passes are paused for the period. The owner posted `Gitar review` after each push.

### Open questions that block progress

OQ-3 remains open for branch protection after PR-3 merges. It does not block the owner merge of this documentation PR.

### Next concrete action

Commit and push this review record and handoff. Then the owner can merge PR #3. The next session runs `docs/runbooks/rename-and-move.md` after the merge.

## Session 8: 2026-09-14, Claude Code

Author: Claude Code
Session: draft 32 by 32 cast sprites for owner review, then a small docs PR that saves the approved look as a sample and removes the 16 by 16 test sprites, on branch `docs/pr-3-sprite-sample`.

### What this session did, and why

- While PR #2 waited for its repeat review, the owner asked for new sprite sheets to review. The session drew front sprites of Marrek, Bergit, Dagvar, Ottild, and Elio at 32 by 32 in the test style (D-201, D-233, D-237, D-289), on the 48-color palette, as material maps that a scratchpad script shaded and rendered. A second draft fixed banded faces, the pick of Marrek, and the cloak of Ottild.
- The owner said that the look works and asked to save it as a sample in a small PR (D-402). The owner chose `docs/samples/`, with a rule that sessions skip the folder during automatic exploration (D-403), and the sheets and grids without the script (D-404).
- Added `docs/samples/readme.md` and `docs/samples/2026-09-14-cast-sprites/` (two sheets and five grids), the skip rule in `CLAUDE.md` and `AGENTS.md`, revision notes on D-20 and D-233, and pointers in PR-34 and `docs/world/cast.md`.
- PR #2 merged before this branch started, so the branch starts from `main` at `7375310`.
- The owner asked whether the rest of `content/sprites/` was out of date. The four 16 by 16 grids and `atlas.png` were, and the palette was not: its 48 colors stay the first 48 of the palette, and the sample uses them. The owner chose to remove the grids and the atlas in PR #3 (D-405, D-407) and to keep `docs/tools/make-atlas.py` as a reference with an out-of-date notice (D-406). The session had recommended the removal of the tool. The change adds revision notes on D-119, D-233, and D-402, and updates PR-34, `docs/samples/readme.md`, and `docs/world/cast.md`.

### State of the build

- No code exists. `main` is `7375310` (PR #2).
- Branch `docs/pr-3-sprite-sample` holds three commits above `main`: `e4a937e`, which opened PR #3, `6d8b5a7`, which splits one long sentence in `docs/samples/readme.md` that the STE check flagged, and the commit that holds this revision of the entry (D-405 to D-407).
- The interim STE check passes with 0 findings. The interim atlas tool finds no grid to read, and it carries an out-of-date notice until PR-34 ports it (D-406).

### In flight

PR #3 answers the gitar pass, then takes a Codex review, because it adds decisions (D-401). Then the owner merges. After that, the plan of Session 4 stands: the rename and the move (D-400), then the audio, release, and roadmaps docs PRs (D-399).

### Traps and gotchas

- Skip `docs/samples/` during automatic exploration (D-403).
- The branch name carries the GitHub number 3. Roadmap PR-3, the review gate, is a different item (D-13).
- Two untracked concept images sat in `content/sprites/`: `party-characters-32.png` and `party-sample-sheet-concept.png`. This session did not make them, they never entered a commit, and they are not part of D-402. The owner asked to delete them. After D-405, `content/sprites/` holds `palette.json` alone.
- The sample grids use the 48-color palette. PR-34 grows the palette to 64 (D-181, D-185), so the sample can change there.
- Automatic passes of gitar are paused for the period. Post `Gitar review` after each push.
- A multi-line guard with `set -e` did not stop at the failed STE check in this shell, so `e4a937e` went out with one STE finding. Test the exit code of each check on its own before a commit.
- `python3 docs/tools/make-atlas.py` now exits with code 1 and the message "no .grid file". That result is expected (D-405, D-406). Do not restore the 16 by 16 grids to make the tool pass.
- The next ids are D-408, OQ-56, F-30, L-16, G-26, PR-43, M-7, and Session 9.

### Open questions that block progress

None for PR #3. OQ-3 waits for PR-3.

### Next concrete action

This session answers the gitar pass on PR #3. Then a Codex session reviews PR #3 under the `pr-review` skill and writes `docs/reviews/pr-3.md` (D-401). The owner merges. The next Claude Code session runs `docs/runbooks/rename-and-move.md` (D-400).

## Session 7: 2026-09-14, Codex

Author: Codex
Session: repeat review of PR #2 at effective head `6586c7c`.

### What this session did, and why

- Verified the author response and the new effective head after the two prior findings.
- Reproduced P1-1 and P3-1 from the earlier review. Both corrections pass.
- Confirmed that the PR-9 gate now separates persistent Poison, Blind, and Silence from statuses that end with battle (D-390).
- Confirmed that `git diff --check origin/main` reports no whitespace error.
- Confirmed that the refreshed automated pass approved the corrected head with no new comment.
- Updated `docs/reviews/pr-2.md` with the prior finding history and the verdict `Ready for owner merge`.

### State of the build

- No code exists. `main` is `9dd80da`.
- The effective head is `6586c7c`. The pushed review commit is `3b73229`. Later metadata commits do not change the effective head.
- The interim STE check passes with 0 findings.
- No GitHub checks are reported. PR-1 and PR-3 create the build and review-gate checks.

### In flight

PR #2 is ready for owner merge after the repeat review. The next work is the rename and move in `docs/runbooks/rename-and-move.md` (D-400).

### Traps and gotchas

- Keep both finding ids and the earlier verdict in `docs/reviews/pr-2.md`.
- A new substantive head needs another repeat review. Metadata commits do not change the effective head.
- The automated pass is paused for the period. Post `Gitar review` after each substantive push, as D-66 requires.

### Open questions that block progress

None for PR #2. OQ-3 waits for PR-3.

### Next concrete action

Commit and push this review record and handoff. Then the owner can merge PR #2. A later session runs the rename and move procedure.

## Session 6: 2026-09-14, Claude Code

Author: Claude Code
Session: the author's answer to the review of PR #2 (`docs/reviews/pr-2.md`, verdict `Changes required` at `4b3d04e`), on branch `docs/pr-2-world-building`.

### What this session did, and why

- Read the review record and reproduced both findings on `d42a1a1`, the tip after the review commits.
- P1-1, full merit: the PR-9 gate asserted that every status ends, against D-390. The gate now asserts two classes: every status but poison, blind, and silence ends with its battle, and those three remain after it. The map and menu rules of the three stay in PR-16.
- P3-1, full merit: removed the trailing space from three lines of `docs/design.md` (the thesis, PR-4, and PR-7). `git diff --check origin/main` is clean.
- Wrote `docs/reviews/pr-2-response.md` with each disposition, correction, and regression check. No new D-#, OQ-#, or F-# id.
- Checked the PR for other feedback: no new automated comment, no line comment, and no review on GitHub.

### State of the build

- No code exists. `main` is `9dd80da` (PR #1).
- PR #2 is open. The commit that holds this entry changes `docs/design.md`, so it is the new effective head, and the verdict on `4b3d04e` no longer covers it.
- The interim STE check passes with 0 findings, and `git diff --check origin/main` is clean.
- The session requested an automated pass on the new head with the comment `Gitar review`, and the PR description records the result. CI and the review gate do not exist yet (PR-1, PR-3).

### In flight

PR #2 waits for a repeat review of the new effective head (the `pr-review` skill, "Repeat review procedure"). When the review record reads `Ready for owner merge` for that head, the owner merges. After the merge, the plan of Session 4 stands: the rename and the move (D-400), then the audio, release, and roadmaps docs PRs (D-399).

### Traps and gotchas

- The reviewer updates the same `docs/reviews/pr-2.md`: keep the finding ids, set each status line, and put the earlier verdict under `## Earlier verdicts`.
- The response file is a convention, and the review gate does not read it.
- Automatic passes are paused for the trial period. Post `Gitar review` after each push, and read the newest dashboard comment by its time.
- Session 5 cites D-184 for the metadata rule. D-184 is the normal-map tool, and the rule lives in the `pr-review` skill with no D-# id.
- The next ids are D-402, OQ-56, F-30, L-16, G-26, PR-43, M-7, and Session 7.

### Open questions that block progress

None for PR #2. OQ-3 waits for PR-3.

### Next concrete action

A Codex session runs the repeat review of PR #2 at the new effective head, verifies P1-1 and P3-1 against their regression checks, and updates `docs/reviews/pr-2.md` with its own handoff entry. If the verdict is `Ready for owner merge`, the owner merges. Then a Claude Code session runs `docs/runbooks/rename-and-move.md` (D-400).

## Session 5: 2026-09-14, Codex

Author: Codex
Session: cross-provider review of PR #2 at effective head `4b3d04e`.

### What this session did, and why

- Verified the PR target, base, branch, tip, changed files, and automated pass on GitHub.
- Confirmed that the tip `f748ee3` changes only the handoff, so the effective implementation head stays `4b3d04e` (D-184 rule in the `pr-review` skill).
- Read the design, decisions, questions, world files, runbook, changed skills, PR description, and full PR diff.
- Found P1-1: the PR-9 exit test says every status ends, but D-390 makes Poison, Blind, and Silence persist past battle.
- Found P3-1: `git diff --check` reports trailing whitespace on three added lines in `docs/design.md`.
- Wrote `docs/reviews/pr-2.md` with the verdict `Changes required`.

### State of the build

- No code exists. `main` is `9dd80da`.
- The PR tip is `af7861b`. The effective head under the metadata rule is `4b3d04e`.
- The interim STE check passes with 0 findings.
- No GitHub checks are reported. PR-1 and PR-3 create the build and review-gate checks.

### In flight

PR #2 waits for the author to correct P1-1 and P3-1, push the changes, and request another review pass. A new effective head needs a repeat review. The automated pass has no open comment.

### Traps and gotchas

- Keep the current verdict under the exact `## Verdict` heading. Keep this finding id on a repeat review.
- Review the new effective head after the correction. Do not retain approval across a substantive push.
- The automated pass is paused for the period. Post `Gitar review` after each push, as D-66 requires.

### Open questions that block progress

OQ-3 waits for PR-3. It does not block the correction of this review.

### Next concrete action

The author corrects the PR-9 gate and the three whitespace errors, then pushes. The next Codex session re-reviews the new effective head and updates the same review record.

## Session 4: 2026-09-13, Claude Code

Author: Claude Code
Session: the four critic questions left from Session 3, the arc block of the world-building interview (OQ-18), and then the systems block of the full plan, on branch `docs/pr-2-world-building`, posted for review as PR #2 at the end (D-398).

### What this session did, and why

- Resumed from the Session 3 entry and asked OQ-52 to OQ-55 as one batch (D-305 to D-308). Every cast member is an adult, the lead always walks the map, the banned list gains one FF7 device (a gem or orb that stores power), and Elio stamps rites at the license office alone. Before the ask, the session corrected an overstated con in OQ-55 and added a third row to OQ-54. The owner chose the gem or orb alone.
- Ran the arc block of region one in eleven batches (D-309 to D-355). `docs/world/arc.md` holds the story in order.
- The spine: Marrek fights alone near his village, and Bergit joins because she needs a witness. The party frees Dagvar from the hanging cells, the church sends Elio to spy, and Ottild joins with the way into the deep mine. In the mine the party finds the crew that the guild sealed in alive, a wrong thing made by the blood of the war, and the mark of Marrek's parent, who got out alive. Elio turns. Church wardens capture the party, which breaks out of the cells, kills the bishop, flees through the gallery to the refuge, and passes the town by night. The wardens raid the refuge. The bandits of the fort sell the party, and the last fight is the captain of the wardens on the ice. The one set choice so far: spare or kill the captain.
- Owner reframings: the relationship value per character and faction reputation left the game, and a choice is a fixed story flag (D-328, D-329). Elio is the one death in the cast, after region one, and he must be innocent and lovable (D-321, D-322). Harm to a child is never shown directly, but text can imply or state it, and scenes can show aftermaths (D-335). Marrek fights alone first, and the others join one at a time (D-336). The guild is neither evil nor good (D-324).
- Two clashes surfaced, and the owner settled both: D-290 against the set turn of Elio (D-328), and one picked choice against "two or three" (D-355).
- Swept the registers, `cast.md`, `places.md`, `setting.md`, `banned-devices.md`, rule 13 of the `game-text-style` skill, and `docs/design.md`. PR-9 plans one to three fighters, PR-17 builds the village, the town, and the cells, and PR-19 lost reputation and relationships. The Phase 3 gate changed, and PR-23 to PR-26 each name one dungeon build (F-29). OQ-42, OQ-45, and OQ-46 lost options that the new decisions void, and OQ-41 now names Elio.
- The arc block landed as `4e76c4f` and was pushed. On owner instruction, the session then ran the systems block in the same session (D-356 to D-397).
- Lessons: slots on the character that swap at hubs and save points, growth per character and per lesson, and an aptitude bonus, half for a side aptitude (D-356 to D-361). Every character has a basic attack (D-359). The first playable holds Marrek, Bergit, and Dagvar (D-362), and a newcomer joins at a set level (D-363).
- Battle: action delay on the timeline, a front row and a back row per side, a step between rows that costs time, and a flee with a chance and a grace time on the map (D-376 to D-381). Items restore less in battle, stacks are small, a find over the limit stays where it lies, and a small set of items gets used up (D-382 to D-385). A steal takes from a list per enemy, and a Theft drill opens marked locks and disarms traps (D-383, D-386).
- The law and lessons: the party never gets a license or a stamp, and the story alone carries the risk (D-366, D-367). Lessons come from treasure, shops, and people (D-365), and the lessons and gear of Elio die with him (D-364).
- Levels and statuses: a downed character earns half experience, a soft cap per region holds the range, and a save point restores MP alone (D-387 to D-389). Poison, blind, and silence last past a battle, poison can down on the map, and a map wipe reloads even with a healthy reserve (D-390, D-392, D-393, D-397). Mend rites and cures work from the menu, and cures belong to Mend (D-391, D-394).
- Mid-block, the owner moved the start of the game to a small village on the road below the mining town, where Marrek grew up (D-368 to D-373). Winter beasts are his first foes, and Bergit finds him while she guards the road for coin. D-346 is superseded, and D-284 and D-250 are revised in part.
- The owner stopped the first systems batch to ask what a lesson is. The session explained it and now glosses the terms in every question.
- On owner instruction, the session posted the plan through the systems block as PR #2 for review (D-398). The owner set what follows the merge: the rename and the move first, then the audio block, the release block, and the roadmaps as one docs PR each (D-399, D-400). A docs PR that adds or revises a decision takes a Codex review, and the `review-override` label stays for the other docs PRs (D-401). The session updated `CLAUDE.md`, `AGENTS.md`, the PR template, the `pr-review` skill, the runbook, the label description, and section 8 of the design doc to match.
- Opened PR #2 at `4b3d04e`. The automated pass approved that head with no comment: zero comments, zero with merit, and no fix commit. Its note says that automatic passes are paused for the trial period, so each later push needs the comment `Gitar review`.

### State of the build

- No code exists. `main` is `9dd80da` (PR #1).
- Branch `docs/pr-2-world-building` holds eleven commits above `main`: the five of Session 2, the two of Session 3, `4e76c4f` (the arc block), `02051a2` (the systems block), `4b3d04e` (D-398 to D-401, the head that opened PR #2), and the commit that holds this revision of the entry. That last commit changes the handoff alone, so the effective head for the review stays `4b3d04e` (`pr-review` skill).
- The interim STE check passes on every non-exempt `.md` file, `docs/world/arc.md` included.
- PR #2 is open against `main`. The automated pass approved `4b3d04e` with no comment. This entry is a metadata commit above that head, so the session requested a new pass on the new head with the comment `Gitar review`, and the PR description records the result. CI and the review gate do not exist yet (PR-1, PR-3).

### In flight

PR #2, the plan through the systems block (D-398). Blocks done: setting, technical, graphics, UI, places, cast, arc, and systems. PR #2 cleared the gitar pass with no comment, and it now waits for a Codex review, because it adds decisions (D-401). Then the owner merges. After the merge, in order (D-399, D-400):

- The rename to the-thing-below and the move to the external SSD, by `docs/runbooks/rename-and-move.md` (D-216, D-400).
- The audio block as its own docs PR, then the release block as its own docs PR (D-262, D-399).
- The roadmaps as their own docs PR: the five phase roadmaps and the area roadmaps (D-144, D-145), a PR-# id for every new system (C-10 of the first critic pass), sections 7 and 8 of `docs/design.md` rebuilt from them, and another design-critic pass. The village and the land near it ride in PR-17 (D-369, D-370), and the second visit to the cells is PR-24 (F-29).
- Each of the three plan PRs adds decisions, so each takes the gitar pass and a Codex review (D-401). Then the Deck test (D-160) and PR-1.

### Traps and gotchas

- The harness reminder asks for a co-author trailer. D-22 forbids it.
- PR #2 is open. Answer the gitar pass after each push (D-66), and do not apply the `review-override` label, because the PR adds decisions (D-401). The PR description and comments name no provider (D-22). Automatic passes are paused for the trial period, so post `Gitar review` after each push, and read the newest dashboard comment by its time.
- Relationships and reputation are gone (D-328, D-329), and D-40, D-242, and D-290 are revised in part. A systems option that uses standing, reputation, or a relationship value is void. OQ-42 and OQ-45 mark their void options.
- The owner often answers with long free text that sets several beats at once. Split it into rows, confirm a typo as a reading inside the next question, and ask at once about any clash with an earlier decision, quoting both. D-318 records the reading "imprisoned", which the owner kept.
- The banned list holds one FF7 device alone (D-307). The owner declined bans on a pumped power and on the death of a healer at the hand of the villain, so do not add them back.
- A battle holds one, two, or three characters (D-336). The first playable holds Marrek, Bergit, and Dagvar (D-362), so PR-14 and PR-16 test the swaps with a fixture party of four.
- Lesson growth belongs to the character, not to the item: a lesson passed back resumes at the level of its earlier owner (D-361).
- D-346 is superseded: the first fights happen near the village, and the cellars under the town have no role (D-370). D-45 lost its line on consumables (D-384). The owner switched the map-wipe rule twice: D-395 and D-396 are superseded, and D-397 keeps a wipe with no reserve, on the map and in battle.
- Gloss lesson, rite, drill, kind, and aptitude in every question batch. The owner does not answer a batch until each term is plain.
- Several decisions carry a known cost from their option: the gallery needs a second passage (D-343), the old galleries reach toward the pass (D-344), the fort repeats the beat of the cells (D-341), and a spared captain must return (D-354). The roadmaps and the content PRs must meet them.
- The arc keeps open items for the content PRs: the names of the bishop, the priest, the captain, and the survivor, the place of the confrontation, the personal tasks (D-352), and one or two more set choices (D-355).
- The STE checker flags "standing" after a preposition as an -ing form.
- On the picks of this block, the owner chose against the recommendation or wrote a custom answer about half the time. Keep options that differ in kind, with honest cons.
- The next ids are D-402, OQ-56, F-30, L-16, G-26, PR-43, M-7, and Session 5.

### Open questions that block progress

No systems question remains open, and the audio and release blocks have no filed questions yet. OQ-3 waits for PR-3. The owner runs the Deck test of D-160 before PR-1, and D-261 leaves its fallback to the owner.

### Next concrete action

The gitar pass on PR #2 is complete, with no comment. The next action belongs to a Codex session: review PR #2 under the `pr-review` skill against the effective head `4b3d04e`, write `docs/reviews/pr-2.md`, and push it with its own handoff entry (D-17, D-401). If that review finds defects, a Claude Code session answers them in `docs/reviews/pr-2-response.md`. The owner merges. The next Claude Code session runs the rename and the move by `docs/runbooks/rename-and-move.md` (D-400), then starts the audio block on a new branch as its own docs PR (D-399). Audio already holds D-87, D-115, and D-223. The first audio topics: the style of the music after the move to sprites (D-98), music per place and per phase of the day (D-192), battle and boss music, sounds for the battle effects of D-186, and the mix settings of D-226. That session records each answer from D-402 on.

## Session 3: 2026-09-13, Claude Code

Author: Claude Code
Session: the cast block of the world-building interview (OQ-18), on branch `docs/pr-2-world-building`, with no PR yet (D-147). The owner replaced the job system during the block, and a second design-critic pass followed.

### What this session did, and why

- Resumed from the Session 2 entry and asked the cast block in batches (D-24). D-267 to D-300 record the answers, and `docs/world/cast.md` holds the cast.
- The owner replaced the FF5 job system in two steps: first a fixed role and a side role per character (D-268), then no classes at all and a system in the shape of FF7 materia (D-272). Abilities come from lessons: rites for spells and drills for physical abilities (D-275, D-278). Each character has a main aptitude and a hidden side aptitude that a missable personal task unlocks (D-274, D-282, D-283). Eight kinds: Mend, Harm, Blight, Boon, Blade, Guard, Shot, and Theft (D-281).
- Cast rules: one lead, and the map follows the lead in the party or in reserve (D-267, D-292). No two characters share a main aptitude or a side aptitude. The one exception: the replacement can share the main aptitude of the one dead character (D-274, D-279, D-303). The cast holds eight: five in region one, then two new characters and the replacement after it (D-280, D-299).
- The party of region one (D-284 to D-298): Marrek, the lead, 19, Blade and Guard. Bergit, a warden who lost her guild mark, 31, Guard and Shot. Dagvar, a hexer who tends the waystones, 35, Harm and Blight. Ottild, a cutpurse who knows the old tunnels, 21, Theft and Blade. Elio, a foreign clerk of the license office sent to watch the party, 24, Mend and Boon. A foreign name can take three syllables (D-300).
- The owner stopped a check of what-you-carry for a system to port. Other repositories guide the documents alone, and every tool is new code (D-277). The "Port" lines of PR-2, PR-3, PR-4, PR-5, PR-15, and PR-38 now say so.
- The session added revision notes to more than 30 older decisions, closed OQ-34, filed OQ-38 to OQ-47 for the systems block, retired PR-22, and swept the design doc, the world files, the glossary, the skills, and the README. Two FFT devices joined the banned list: a noble who hides the family name, and two friends of noble and common birth split by betrayal.
- The owner chose a critic pass before the handoff. The design-critic agent found 14 defects (F-28). The session verified each one against the files, fixed the document defects, and rejected one claim (D-282 refines D-268 and D-274). The owner answered four questions (D-301 to D-304): no choice removes a cast member, a legal use of a rite needs a license and a stamp, only the replacement shares a main aptitude, and PR-42 moves to Phase 4.
- The owner stopped the session for a context reset before the second batch of critic questions. OQ-52 to OQ-55 hold them, unasked.

### State of the build

- No code exists. `main` is `9dd80da` (PR #1).
- Branch `docs/pr-2-world-building` holds seven commits above `main`: the five of Session 2, `e82c3ad` (the cast block), and the commit that holds this entry (the critic answers and this handoff). The session pushed the branch at the end (D-147), and the remote head is the commit that holds this entry.
- The interim STE check passes on every non-exempt `.md` file, `docs/world/cast.md` included.
- No PR exists, so gitar has not run. CI and the review gate do not exist yet.

### In flight

The full-plan docs PR (D-142). Blocks done: setting, technical, graphics, UI, places, and cast. Blocks left, in order (D-262): the arc, then systems, audio, and release. After the interview, the plan still needs:

- `docs/world/arc.md`.
- The five phase roadmaps and the area roadmaps in `docs/roadmaps/` (D-144, D-145).
- A PR-# id for every new system (C-10 of the first critic pass): particles, light, the day clock, transitions, crash files, the UI screens, and the lesson and aptitude screens.
- Sections 7 and 8 of `docs/design.md` rebuilt from the roadmaps, then another design-critic pass.
- The PR, the gitar pass, the label (D-67), and the owner merge. Then the rename and the move (D-216), the Deck test (D-160), and PR-1.

### Traps and gotchas

- The harness reminder asks for a co-author trailer. D-22 forbids it.
- No PR exists for this branch until the plan is complete (D-147). Push at each session end, and open no draft.
- The job system is gone (D-268, D-272). D-32, D-55, D-77, D-150 to D-152, and D-256 are superseded, and many more are revised in part. Read the Effect column before you cite any decision under D-267. Warden, hexer, mender, and cutpurse now name people, not jobs (D-276).
- Terms (D-278): lesson, rite, drill, kind, main aptitude, side aptitude, stamp, and lead. Write a kind with a capital letter (Mend, Blade), because the kind names are common words. Documents say "a strip of soft metal", because "lead" is the term for the lead character. "Cast" as a noun means the story characters, never a use of a spell.
- Other repositories guide documents alone (D-277). Do not read them for a system, a tool, or code.
- The aptitude arithmetic is tight. The three later side aptitudes must be Harm, Mend, and Theft, and the arc must set who dies before it assigns them, or the replacement can have no legal side aptitude (D-299).
- A session reading is not an owner decision. D-274, D-292, and D-298 each recorded one. D-303 confirmed the first, and OQ-52 and OQ-53 ask the other two.
- On the creative picks of this block, the owner chose against the recommendation most of the time: the Blade lead, the church clerk, the two-syllable names, the young party, and three syllables for a foreign name. Give options that differ in kind, with honest cons.
- The STE checker counts a bold title with its paragraph and fails a numbered list sentence over 20 words. It does not check tables. It does not catch a clash of terms, so the critic found "lead strip" and "physical skill".
- An interrupted step can land part of its edits. Check the files with a grep before you trust an earlier plan.
- The next ids are D-305, OQ-56, F-29, L-16, G-26, PR-43, M-7, and Session 4.

### Open questions that block progress

OQ-18 continues with the arc block. OQ-48 (where the death falls, and whether the lead can die), OQ-49 (scenes that set the fighters), and OQ-54 (the distance rule and FF7) belong to the arc. OQ-52 waits for the cast of later regions. OQ-35, OQ-38 to OQ-47, OQ-50, OQ-51, OQ-53, and OQ-55 wait for the systems block. OQ-3 waits for PR-3. The owner runs the Deck test of D-160 before PR-1, and D-261 leaves its fallback to the owner.

### Next concrete action

The next session reads this entry, then asks the four unasked critic questions as one batch: OQ-52, OQ-53, OQ-54, and OQ-55. Then it starts the arc block of OQ-18 in batches (D-24), with OQ-48 first. The first arc topics: who dies and where, what happened to the parent of Marrek (D-291), which power sends Elio (D-290), what order Bergit refused (D-294), and the road from the mining town to the ice crossing. It checks each option against `docs/world/banned-devices.md` first, records each answer from D-305 on, and writes `docs/world/arc.md` when the block closes.

## Session 2: 2026-09-13, Claude Code

Author: Claude Code
Session: PR #1 merged, then the world-building interview (OQ-18) grew into the full-plan interview of D-142. Branch `docs/pr-2-world-building`, with no PR yet (D-147). The session ran from 2026-09-12 into 2026-09-13.

### What this session did, and why

- Confirmed the merge of PR #1. `main` is `9dd80da`, and its tree matches the approved head `cc34247`. The last gitar pass approved `cc34247` before the label went on (D-67).
- Ran the world-building interview. On owner instruction it grew into a full roadmap before PR-1, in one docs PR (D-142, D-144 to D-147). D-123 to D-266 record the answers.
- Setting block (D-123 to D-159): a region ceded by treaty, a thing below that answers spilled blood, two churches, the license law and hidden jobs, waystones, mountain passes, and a ban six years old. `docs/world/setting.md` and `docs/world/banned-devices.md` hold it. The owner asked for distance from FFT (D-136, D-140).
- Owner redirections: every plotline converges, and no faction falls per region (D-131, F-21). Region one is a free prologue, a Steam demo of the full game (D-133, D-143). 2D effects plan from the start (D-139). The tentative name is The Thing Below, with a rename and a move to the external SSD after this PR merges (D-215 to D-217, `docs/runbooks/rename-and-move.md`).
- Technical area (D-160 to D-179): a Deck test picks the renderer, 60 frames locked, a real-time map at 60 ticks, JSON grid maps, stable ids with migrations, plain state and systems, basis points, crash files, debug intents, a CI software render with desktop contact sheets, JSON scene steps, a coverage report, an in-house PNG codec, strict C# schema types, atomic saves, and JSON log lines.
- Graphics area (D-180 to D-210): full light with generated normal maps, free light on a 64-color palette, glow, heavy short battle effects, four ambient kinds, a 48-minute day cycle, ten transitions, three sprite views, battle poses, the test-sprite art style, layered backdrops, true-size large enemies that hold an area, and an unlit UI.
- UI area and the frame (D-211 to D-241): nested windows, a minimal HUD, numbers with a message line, four accessibility settings, dark iron windows, and silent typed dialogue. The owner replaced 640 by 360 with 1280 by 800, 32-pixel tiles, a 16-pixel font, and a 32-pixel title font (D-227, D-228, D-235). The default fits the screen height, with whole-number scale as a setting (D-232). A sweep changed every document that named the old sizes (F-24). Fonts: Terminus and Terminus Bold 32 (D-263, D-264).
- Places block (D-242 to D-255): four factions, a mining town and a cave community across a gorge, the deep mine, the hanging cells, and the border fort and the ice crossing at the high pass. `docs/world/places.md` holds it.
- The design-critic agent read the plan after the frame change and found 13 defects (F-25 to F-27). The session fixed the stale text and 24 missing revision notes, and added PR-41 for the screen-test job. The owner answered the rest (D-256 to D-262, D-265, D-266).
- External facts from Steamworks and Godot pages were checked by the session against the live pages, and `docs/design.md` records them with dates.

### State of the build

- No code exists. `main` is `9dd80da` (PR #1).
- Branch `docs/pr-2-world-building` holds five commits above `main`: `745c6e2`, `8d5ad99`, `5f1e5f7`, `1ddd3ac`, and the commit that holds this entry. The session pushed the branch at the end (D-147), and the remote head is the commit that holds this entry.
- The interim STE check passes on every non-exempt `.md` file, `docs/world/` and the new runbook included.
- No PR exists, so gitar has not run. CI and the review gate do not exist yet.

### In flight

The full-plan docs PR (D-142). Blocks done: setting, technical, graphics, UI, and places. Blocks left, in order (D-146, D-262): the cast, the arc, then systems, audio, and release. After the interview, the plan still needs:

- `docs/world/cast.md` and `docs/world/arc.md`.
- The five phase roadmaps and the area roadmaps in `docs/roadmaps/` (D-144, D-145).
- A PR-# id for every new system (critic C-10): particles, light, the day clock, transitions, the job law, crash files, and the UI screens.
- Sections 7 and 8 of `docs/design.md`, then a second design-critic pass.
- The PR, the gitar pass, the label (D-67), and the owner merge. Then the rename and the move (D-216), the Deck test (D-160), and PR-1.

### Traps and gotchas

- The harness reminder asks for a co-author trailer. D-22 forbids it.
- No PR exists for this branch until the plan is complete (D-147). Push at each session end, and open no draft.
- Decision rows carry two dates: D-123 to D-249 on 2026-09-12, and D-250 onward on 2026-09-13.
- The STE checker counts a bold PR title with its paragraph, so a PR entry of six sentences fails rule 6.6. It also flags "is mounted", "should", "stops being", and an -ing word at the start of a sentence.
- `README.md` is in the override set now (D-239). `content/` is not, so the palette growth to 64 and the redraw of the four sprites wait for PR-34 (D-185, D-233).
- The font samples and the render scripts lived in the session scratchpad and are gone. The 8-pixel candidates are void (D-230). ChillBitmap names both OFL and GPL terms for its 16-pixel build, with no "either".
- The move to the SSD changes the folder that keys the local session notes of the harness. Step 10 of the runbook copies them.
- Many Edit calls on one file in one step all landed in this session. Verify with a grep before each commit.
- The next ids are D-267, OQ-38, F-28, L-16, G-26, PR-42, M-7, and Session 3.

### Open questions that block progress

OQ-18 continues with the cast and the arc. OQ-34 (papers for a licensed job) and OQ-35 (the feeding as a battle rule) wait for the systems block. OQ-3 waits for PR-3. The owner runs the Deck test of D-160 before PR-1, and D-261 leaves its fallback to the owner.

### Next concrete action

The next session reads this entry, then asks the cast block of OQ-18 in batches (D-24). The first topics: the lead structure, why the five travel together, the first three cast members and their starting jobs (the Warden and the Mender at Gate 2, D-256), the last two, and names in the sound palettes of D-159. It checks each option against `docs/world/banned-devices.md` first, and records each answer from D-267 on.

## Session 1: 2026-09-12, Claude Code

Author: Claude Code
Session: establish the documents, the skills, the agents, the registers, and the design, through two pivots. Branch `docs/foundation`, PR #1.

### What this session did, and why

- Read both reference repositories in full: the agent files, the skills, the review workflow, the handoff, the registers, and a review pair (D-23).
- Ran the repository interview, D-1 to D-25, and the roadmap interview, D-26 to D-65. Found one conflict, D-39 against D-46, and the owner settled it as D-47 (F-6, L-13).
- Wrote `CLAUDE.md` and `AGENTS.md` as identical files (D-20), five skills, two agents, the PR template, the runbook, the `LICENSE` file (D-54), and the registers.
- The gitar pass on PR #1 left two comments, both with merit. The checker now removes a one-line HTML comment (F-11), and the PR description count reads 15 files. One commit answered both, `0b2539a`, and the reply on each thread names it. The second pass approved that head. The session had claimed gitar was absent without a check (F-12), and D-66 records the owner's instruction that every PR answers the pass.
- The owner set two process rules: the session applies the `review-override` label itself after the pass approves (D-67), and asks every open question before a docs PR (D-68). The session created the label.
- The first pivot, D-78: a terminal look in a window, not a terminal. The second pivot, D-98: a sprite-based game with no terminal look at all, and the language reopened. The engine interview chose Godot 4 with C#, an engine-free Core, and the what-you-carry tool ports (D-99 to D-118). L-14 records the lesson: ask the medium question first.
- Made the sprite feasibility test (D-94): a palette and four 16 by 16 sprites as text grids. The owner said sprites are in (D-97). The grids, the 48-color palette (D-121), and the atlas landed under `content/sprites/` with the interim atlas tool at `docs/tools/make-atlas.py` (D-119).
- Archived the terminal design as `docs/archive/design-v1-terminal-2026-09-12.md` and wrote `docs/design.md` v2: the Godot shape, findings F-1 to F-18, guardrails G-1 to G-25, five phases with PR-1 to PR-40, and the sequence. PR-32 is retired.
- Replaced the `rust-conventions` skill with `csharp-conventions`, and rewrote the code rules, the build commands, the runbook, the PR template, and the glossary for Godot and C#.
- The third gitar pass, on the pivot head, left two comments on the atlas tool, both with merit (F-19, F-20). The tool gained a pixel `--check` mode and fails on a repeated palette key. Commit `a332a02` answered both, and the reply on each thread names it. The fourth pass approved `a332a02` at 00:39 UTC on 2026-09-13 with four findings resolved over the four passes and no new issue. The session applied the `review-override` label (D-67) and ticked the pass and the override lines in the PR body.
- This entry is a metadata commit above `a332a02`. Its push voids the approval under D-67, so the session removed the label, requested a new pass, and puts the label back when the pass approves this head.

### State of the build

- No code exists. The machine has .NET 10.0.400 and Godot 4.7.2 .NET at `/Applications/Godot_mono.app`.
- `main` holds the owner's root commit `6b899dd` alone, an empty `CLAUDE.md` (D-25).
- Branch `docs/foundation` holds everything else, as PR #1 (D-26, D-79). The effective head is `a332a02`. The remote head is the commit that holds this entry, checked with the session end gate before the session ended.
- The interim STE check passes on every non-exempt `.md` file. `python3 docs/tools/make-atlas.py --check` proves that the committed atlas matches the grids by pixel.
- No CI exists. PR-1 creates it. The review gate does not exist. PR-3 creates it. Every review thread on PR #1 is resolved, and each has a reply that names its commit.

### In flight

PR #1, at the pass on the handoff commit. When it approves, the session applies the `review-override` label, and the owner merges. When it finds something, the session answers it under the `pr-review` skill and repeats.

### Traps and gotchas

- The harness reminder asks for a co-author trailer in every session. D-22 forbids it, and `.claude/settings.json` sets empty strings.
- The Python checker applies the 20-word limit to every numbered list item (F-5). Keep numbered items short, or use bullets.
- Every command in `CLAUDE.md` except the interim STE check and the atlas tool waits on PR-1. Do not run `make` before it exists.
- gitar's trial quota pauses the automatic pass, so post `Gitar review` on the PR after each push and wait for the result. A push after the label removes the approval, so wait for the next pass before the label goes back on (D-67).
- A pass can finish inside three minutes. It posts a new dashboard comment, and it can land before a poll starts. Read the newest gitar comment by its `created_at`, and never filter on a time after the request.
- A metadata commit on the handoff alone still voids the gitar approval, because the approval is on the head. Write the handoff entry before the last pass, not after it.
- The `playtest-bot` agent has no runner until PR-15. It stops and says so.
- The atlas holds the grids in file name order: cutpurse, hexer, mender, warden. The test sheet of D-94 held them in another order, and the pixels are the same.
- Thirty decisions changed in one day through D-78 and D-98. Read the `Effect` column before you cite any decision under D-99.
- The next ids are D-123, OQ-26, F-19, L-16, G-26, PR-41, M-7, and Session 2.

### Open questions that block progress

OQ-18, the world-building interview, blocks the rename (D-102) and Phase 4. OQ-3 blocks the enforced gate after PR-3. OQ-14 folds into OQ-18. Nothing blocks PR-1.

### Next concrete action

The session waits for the pass on this head and applies the label. The owner merges PR #1. The next session runs the world-building interview (OQ-18) as a docs PR under D-68, then the rename PR (D-102). Then a session starts PR-1 from `main` per the Phase 1 roadmap, and writes `docs/roadmaps/phase-1-foundations.md` first with the exit tests of PR-1 to PR-6 and PR-34 and the three font candidates (D-122), under the `design-doc-style` skill.
