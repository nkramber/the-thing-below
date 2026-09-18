# Phase roadmap: Phase 1, Foundations

Status: **active focused phase roadmap, which PR #11 merged on 2026-09-16.** This file gives each item of Phase 1 its scope, its exit tests, its review focus, and its questions (D-144, D-485, D-487). The area files say how each part works, and each entry names the area file that it cites. This file supersedes no earlier file. Written 2026-09-16 in ASD-STE100.

The design doc holds the thesis of the game, the system map (section 3), and the cost model (section 4). It also holds the guardrails (section 6) and the global order of every PR (section 8). This file cites each decision by its id and never restates it. The index of this folder is `docs/roadmaps/readme.md`.

This file states no external fact. Each external fact of Phase 1 lives in the area file that holds its part, with the date of its check.

Text rules: this file follows ASD-STE100 (D-10). Tables are exempt from sentence-length counts.

## 1. Thesis

Phase 1 builds the machine that every later phase trusts. It ships no play. At the end of it, each change to the game meets a test, a lint, and a review gate (T-3, T-4). Three CI legs and the Mac then agree on one state hash (T-7).

The order follows dependency. The gate tools come first, because each later PR passes them (PR-1, PR-2, PR-3, PR-46). Then come the rules that every system computes with: integer math, the streams, and the state hash (PR-4). Content and the string table follow, because each rule reads content (PR-5).

The tick, the run record, and replay come next, because a run needs rules and content (PR-6). The files close the code work: the saves and then the crash and log files (PR-43, PR-44). The art pipeline lands last, because no screen draws yet (PR-47, PR-34).

Gate 1 is a foundation gate with no play (D-52). It asks one thing: every check is green on every leg, and the three legs and the Mac agree on each state hash.

## 5. Findings that bind this phase

The register in section 5 of `docs/design.md` holds every finding. These rows bind an item of Phase 1.

| # | Finding | Binds |
|---|---|---|
| F-2 | The borrowed gate tools are C# and run nowhere here | PR-2 and PR-3: both tools as new code (D-101, D-277) |
| F-5 | The two checkers disagree on the rule for a numbered item | PR-2: one rule, and the skill text follows it (D-604) |
| F-10 | The run record grows with no limit over a long play | PR-6: a snapshot plus the intents after it (OQ-65) |
| F-11 | The interim checker read an HTML comment as prose | PR-2: the new checker carries the rule, and MD 1 fails a comment across lines |
| F-17 | The 32-color palette had too few free colors | PR-34: the palette of 64 colors (D-181) |
| F-19 | An atlas cannot match byte for byte across encoders | PR-34: a pixel test, never a byte test |
| F-20 | The interim atlas tool kept the last of two equal palette keys | PR-34: a repeated key fails with the key |
| F-24 | A 32-pixel tile holds four times the pixels of the earlier plan | The Deck test: the load runs at the frame of 1280 by 720 (D-568) |
| F-25 | A quit autosave can trap a run (C-3), and a Core patch refuses old saves (C-4) | PR-43: the resume file of D-258 and the load of D-259 |
| F-27 | The debug console of D-171 meets the rule of no conditional compilation in Core | PR-6: the seam of D-260 and D-492 |
| F-35 | Two hash paths of .NET break G-1 and T-7 | PR-4 and PR-5: a hash function that Core holds (OQ-61, OQ-62) |
| F-36 | The JSON support of .NET uses reflection by default | PR-5: a reader with no runtime reflection |
| F-37 | GitHub starts `pull_request_target` only from the default branch | PR-3: the command proves itself in Tests (D-500) |
| F-38 | A real literal with no suffix is a double, and double math differs by platform | PR-46: det-lint reads types through Roslyn (D-498) |
| F-39 | The default string order of .NET follows the culture and the ICU version | PR-4 and PR-46: an ordinal order for every string order in Core |
| F-40 | The test command of `CLAUDE.md` works in VSTest mode alone | PR-1: D-592 picks the MTP mode, and the commands follow |
| F-41 | Four rules of GitHub Actions meet the CI plan | PR-1: D-595 puts the skip condition on each job, and never on the workflow |
| F-42 | The Godot export walks the project folder alone, and `content/` lies outside it | PR-5: the Game assembly carries the content files (D-508) |
| F-58 | No check can see the conversation of a session | PR-3: the document rules read the diff and the description alone (D-579) |
| F-60 | The Godot editor writes `net8.0` into a `.csproj` that holds no target framework | PR-1: the Game project pins `net10.0` in its own file. The exit-code part of this finding is refuted |
| F-61 | A coverage run instruments the Core copy and adds references to it | PR-1: the reference test reads the file that the Core project built |
| F-64 | A headless session whose managed assembly does not load runs without end. A frame limit alone makes it end with an exit code of 0 and no success line | PR-1: the smoke session runs with `--quit-after`, and each caller reads the success line. Both parts are needed |

## 7. Roadmap

Each entry below gives one item of Phase 1 its scope, its exit tests, its review focus, and its questions. The area file of each entry says how the part works (D-144). Each entry ends with a plain-English paragraph for a reader who does not know the code.

An exit test is a test or a job that the PR adds and that must pass before the merge. The PR gate of `CLAUDE.md` still applies to each PR, and these tests are the ones that this PR alone can fail.

### 7.1 The Deck test

Owner and a session, before PR-1. Area file: `area-effects.md`, sections 7.3 and 7.4.

**Scope.**

- A throwaway scene runs on the Deck of the owner under Forward+ and under Mobile (D-160, D-161).
- The test scene runs as a native Linux export at the frame of 1280 by 720, with the load of D-160 (D-228, D-458, D-568).
- The test picks the renderer, and it measures the effect budget (D-160, D-523).
- Done on 2026-09-17 on an OLED Deck. Mobile won each of the 20 stages, and PR-82 sets it (D-616).
- The run gave the first effect budget, and no stage missed the target (D-617, F-66).
- M-7 holds the numbers, and the four reports attach to the PR description of PR-85 (D-624).

**Out of scope.**

- The test scene never merges to `main` (D-160).
- The budget file and its test come with PR-56 (D-523).

**Exit tests.**

1. One renderer holds 60 frames each second with the full load (D-161).
2. The test records the model of the Deck and its refresh rate.
3. The test records the budget numbers: lights, particles, and full-screen passes.
4. If neither renderer holds 60 frames each second, the owner decides then (D-261).

**Review focus.** This step has no PR and no review. The session records the numbers in a decision row and in the cost model.

**State.** The test scene, the meter, and the native Linux export are ready on `spike/deck-test` (D-597, D-598). No machine ran the export. The run on the Deck is the next step, and the owner does it. A run on the Mac gives no numbers, because macOS caps the frame rate.

**Questions.** OQ-92 and OQ-93 are resolved (D-597, D-598).

> *In plain English:* before any code, a small test scene with every kind of effect runs on the owner's handheld. It picks the faster of two graphics modes and finds how much the machine can draw at full speed.

### 7.2 PR-1: the repository scaffold

Area files: `area-ci.md` sections 7.1 to 7.6, `area-tools.md` section 7.1, `area-core.md` section 7.1.

**Scope.**

- The solution and the four projects of D-118 and D-217: Core, Game, Tools, and Tests.
- `global.json` pins the .NET SDK, and the runbook and CI pin the Godot editor (D-99, D-511).
- `Directory.Build.props` sets nullable reference types on and warnings as errors.
- The Makefile with `verify`, `where`, `hooks`, `test`, `lint`, `ste-check`, and `run` (D-3).
- The pre-commit hook that refuses a commit on `main` and a document that fails the STE check (D-8, D-25).
- The build, test, and format job on the three legs of D-481.
- The `smoke` job that installs the pinned Godot editor, checks its SHA-512, and runs the headless session (D-117).
- The `ste-check` job on the interim Python checker, on the Linux leg alone (D-10, G-12).
- The coverage report on every PR, with no number that fails the build (D-174, D-506).
- Two tests: `CLAUDE.md` equals `AGENTS.md` (D-20), and Core has the reference list of G-1.
- Forward+ in the Game project, as a provisional renderer. PR-82 sets the Mobile renderer of D-616 (D-599).
- A boot Godot scene file and the `--smoke` argument, which boot the engine and quit, so the smoke job can pass (D-117, G-16).

**Out of scope.**

- No game system, no screen, no content file, and no Core rule.
- det-lint (PR-46), the identity job (PR-4), the export job (PR-54), and the screen tests (PR-41).

**Exit tests.**

1. `make verify` passes on the Mac of the owner.
2. The build, test, and format job passes on each of the three legs.
3. The smoke job starts the editor with `--headless` on each leg and quits with no log error.
4. The `ste-check` job passes on every live document.
5. The agent-file match test fails on a changed copy of `AGENTS.md`.
6. The Core reference test fails on an added project reference.
7. The coverage report appears on the PR.

**Review focus.**

- Each action pins a full commit SHA, and each other download checks its SHA-512 (D-511).
- Each job sets a time limit and the least permissions that it needs (T-2).
- The test command matches the runner mode of OQ-75, in the Makefile and in the agent files (F-40).
- The pre-commit hook refuses a commit on `main` on a fresh checkout (D-25).

**Questions.** OQ-75, OQ-76, OQ-77, OQ-78, and OQ-83 are resolved (D-592 to D-596).

> *In plain English:* this makes the empty project with its four parts and the checks that every later change must pass. It adds nothing that plays, so it changes no behavior.

### 7.3 PR-82: the renderer of the Deck test

Area file: `area-effects.md`, sections 7.3 and 7.4.

**Scope.**

- The Mobile renderer in `project.godot` of the Game project (D-160, D-599, D-616).
- The reports of the Deck run, which the PR description of PR-85 holds (D-624).

**Out of scope.**

- Every other setting of the Game project. PR-1 holds them (D-599).
- The effect budget file and its test, which come with PR-56 (D-523).

**Exit tests.**

1. The build, test, format, and smoke jobs stay green with the new renderer.
2. The smoke session prints `smoke: the renderer is mobile`, which `TheThingBelow.Game/scripts/Boot.cs` reads from the setting.
3. The decision row names the frame time of each renderer at the full load (D-161, D-616).

**Review focus.**

- The diff changes the renderer line alone.
- The setting `renderer/rendering_method.mobile` serves Android and iOS, which this game never ships, so it stays as it is (D-481).
- The numbers in the decision row match the reports of the Deck (D-624).

**Questions.** None. The Deck run answers D-160.

> *In plain English:* after the owner runs the test on the handheld, this changes one setting to the graphics mode that ran faster. Nothing else changes.

### 7.4 PR-2: the STE checker in C#

Area files: `area-tools.md` section 7.2, `area-ci.md` section 7.5.

**Scope.**

- The `ste-check` command in Tools as new code (D-101, D-277).
- The rules of the checker table in the `ste-writing` skill, and the comment rule of F-11.
- The reference check, and the session-number check of D-18 and L-12.
- One rule for a numbered item, from D-604, and a skill text that follows it (F-5).
- The move of the `ste-check` job to the command, and the retirement of the interim Python script (D-10).

**Out of scope.**

- det-lint (PR-46) and the review gate (PR-3).
- No new writing rule beyond the answers of D-604 to D-608.

**Exit tests.**

1. The command passes on every live document of the repository.
2. One fixture file fails each rule, and the finding names the file, the line, and the rule.
3. One fixture file passes each rule.
4. The command skips the four dated records that the skill names.
5. The session-number check fails a repeated number and an entry out of order.
6. The reference check fails a citation of an id that no register holds.

**Review focus.**

- The tool and the skill state one rule for a numbered item (F-5, D-604).
- The rule set matches the skill table, rule for rule.
- The command lines in `CLAUDE.md` and `AGENTS.md` change together (D-20).

**Questions.** None. D-604 to D-608 answer OQ-67 and OQ-68, and they set the three scope rules of the command.

> *In plain English:* every document must pass a check for plain technical English. This moves the check from a borrowed script into the language of the project, with the same rules and a few more.

### 7.5 PR-3: the review gate

Area files: `area-tools.md` section 7.3, `area-ci.md` section 7.7.

**Scope.**

- The `review-gate` command in Tools as new code (D-15, D-101, D-277).
- The input of the command: one JSON file with the facts of the PR, and one folder with the files of the head.
- The workflow on `pull_request_target`, which runs from `main` and reads the PR head as data alone (D-15).
- The three rules of the `pr-review` skill: the record exists, the verdict is `Ready for owner merge`, and the head field names the effective head.
- The metadata set of D-610, which holds the two review files of this PR and the two handoff files.
- The override rules of D-16, D-71, D-239, D-401, and D-560, with the eligible path set. `.github/workflows/` sits outside that set.
- The document rules of D-579. The handoff changes, and no line defers a document or a record of the PR (D-577). The Documents section has a line in a form of D-581 for each required row.
- A check run as the result, and a second run when a label or the PR description changes (D-67, D-579).

**Out of scope.**

- The night gate (PR-49) and det-lint (PR-46).
- No change to the labels of the repository, which already exist (D-66, D-67).

**Exit tests.**

1. The command passes a fixture PR with an approved record for its head.
2. It fails a fixture PR whose record names an older head.
3. It passes a documentation PR with the label that changes no decision row.
4. It fails a PR with the label that changes a row of `docs/decisions.md` (D-401).
5. It fails a PR outside the override set that carries the label.
6. It fails a PR that changes `.github/workflows/` and carries the label (D-560).
7. A metadata commit does not move the effective head.
8. It fails a PR whose diff does not change `docs/session-handoff.md` (D-579).
9. It fails a PR whose Documents section lacks a required row, or holds a line in no form of D-581.
10. It fails a PR that defers one of its own documents or records to a later PR (D-577, D-578).
11. It passes a PR with complete documents that names the PR of an absent check (G-16).
12. It passes a PR with a line of the form "No change needed because" and a specific reason (D-581).
13. The PR description shows the output of each fixture (D-500).

**Review focus.**

- The workflow never runs code from the head, and its token holds the least access (D-15).
- The live check cannot run on this PR, and the PR says so (F-37, G-16, D-500).
- D-609 sets what counts as a change to a decision row: a line of a decision table.
- The document rules read the diff and the description alone. No check can prove a clean session (F-58).
- The required rows match the table of the `one-pr-one-session` skill, and the skill and the command change together.

**Questions.** D-609 answers OQ-69, and D-610 answers OQ-181. OQ-3 comes right after the merge.

> *In plain English:* this adds a check that turns red when a change has no approved review from the other provider. It also turns red when a change leaves its notes or documents for later. It reads each change as data and never runs it, so a change cannot approve itself.

### 7.6 PR-84: the context budget check

Area files: `area-tools.md` section 7.2, `area-ci.md` section 7.5.

**Scope.**

- The size rules of the context budget in the `ste-check` command (D-611).
- Three limits: `CLAUDE.md` at 16 KB, the top entry of the handoff at 5 KB, and a skill file at 36 KB (D-583).
- One finding for each file above its limit, with the size of the file and the limit.
- The limits in one place of the code, and the same numbers in the `ste-writing` skill.

**Out of scope.**

- No new writing rule, and no change to the `review-gate` command of PR-3.
- No limit for a document that no session reads at the start of a session (D-584).

**Exit tests.**

1. The command passes on every live document of the repository.
2. A fixture `CLAUDE.md` above 16 KB fails, and the finding names the size and the limit.
3. A fixture handoff with a top entry above 5 KB fails.
4. A fixture skill file above 36 KB fails.
5. A fixture file of the exact size of its limit passes.

**Review focus.**

- The size comes from the bytes of the file, so each CI leg reads the same number.
- The handoff rule reads the top entry alone, and not the whole file.

**Questions.** None. D-611 sets the command and the PR.

> *In plain English:* the rules of this repository must stay small enough for a session to read at the start. This adds a check that turns red when one of the three files grows past its limit.

### 7.7 PR-85: the Deck test result and the look

Area files: `area-effects.md` sections 7.3, 7.4, and 7.12, and `area-art.md` section 7.1.

**Scope.**

- The decision rows of the round, D-616 to D-624, and the questions that they answer.
- The result of the Deck test in the cost model, as M-7, with the four reports on the PR (D-624).
- The removal of the CRT from every live document, and the retirement of PR-37 (D-618).
- The two tests that come before PR-34: the screen scale probe and the Sprite Fusion test (D-620, D-621).
- The new rows F-66, F-67, G-27, M-8, and OQ-183.

**Out of scope.**

- The one line of `project.godot`, which PR-82 holds (D-616).
- The two tests themselves, which the owner runs with a session of their own (D-620, D-621).
- The budget file and its test, which PR-56 holds (D-617).
- Every code, content, and art change.

**Exit tests.**

1. The `ste-check` command passes on every live document.
2. No live document plans a CRT pass, and each citation of D-88, D-105, D-120, or D-240 names D-618.
3. Each new id resolves: F-66, F-67, G-27, M-8, OQ-183, and PR-85.
4. The `review-gate` check passes, and the Documents section has a line for each row.
5. The four report files of the Deck run appear on the PR description.

**Review focus.**

- The numbers of M-7 match the four reports, and the budget rows match the sweep.
- Each superseded decision names the decision that superseded it (D-606).
- The removal of the CRT leaves no gate, no test, and no setting that reads it.

**Questions.** None. D-616 to D-624 hold each answer of this round.

> *In plain English:* the owner ran the speed test on the Steam Deck and answered a batch of questions. This writes each answer into the documents, and it takes the old-monitor look out of the plan.

### 7.8 PR-46: det-lint

Area files: `area-tools.md` section 7.4, `area-ci.md` section 7.8.

**Scope.**

- The `det-lint` command in Tools as new code, before the first Core code (D-101, D-277, D-496).
- The read of C# through the Roslyn compiler library, on the type of each expression (D-498, F-38).
- The Core rules: no float type, no clock, no OS random, no reflection, and neither hash path of F-35 (G-2, G-3).
- The ordinal rule for every string order in Core (G-4, F-39).
- The Game rule: no Godot text property outside the text helper, and no text value in a Godot scene file (D-499, G-7).
- The Core rules for float types, the clock, and OS random over the tool code of D-502.
- The `det-lint` job on the Linux leg, and the `det-lint` step of `make verify`.

**Out of scope.**

- No Core rule and no Game screen. The command reads fixtures and the empty projects.
- The STE check (PR-2) and the review gate (PR-3).

**Exit tests.**

1. A fixture with a `double` in Core fails, and the finding names the file and the line.
2. A fixture with a clock read, an OS random, or a reflection call in Core fails.
3. A fixture that calls `GetHashCode` or a .NET hash class in Core fails.
4. A fixture with a string order and no ordinal comparison in Core fails.
5. A fixture that sets a Godot text property outside the text helper fails.
6. A Godot scene file with a text value fails.
7. One fixture passes each rule.
8. The job prints one line per finding and exits 1.

**Review focus.**

- The lint reads types, not words, so a literal with no suffix fails too (F-38).
- The lint finds the Godot assembly in the build output of the Game project (D-614, F-65).
- The lint fails a walk of a `Dictionary` or a `HashSet` in Core, and it passes a lookup by key (D-615).
- The package `Microsoft.CodeAnalysis.CSharp` has its decision row, and Core keeps no reference (D-498, G-13).

**Questions.** None. D-614 answers OQ-70, and D-615 answers OQ-71.

> *In plain English:* two computers can disagree on decimal math and on the order of words. This tool reads the rules code as the compiler does and refuses anything that can make two machines disagree.

### 7.9 PR-4: integer math, the streams, the state hash, and the identity job

Area files: `area-core.md` sections 7.1 to 7.6 and 7.10, `area-ci.md` section 7.9.

**Scope.**

- The fixed-point types, each with its scale in its name (D-169, G-2).
- The rounding rule of OQ-60, in one place that every system calls.
- One seeded random stream for each subsystem, split from the run seed (G-4).
- The hash function of OQ-62, which Core holds, and the state hash over the whole state in a fixed order.
- The ordinal comparer for every string order in Core (F-39).
- The exception types that carry context, and the assertion helper that stays on in a release export (T-2, G-18).
- The simulation version constant (G-17).
- The `replay-identity` job, the identity file in Tests, and the Tools command that writes the file again (D-504).
- The identity check in `make verify`. PR-46 already added the det-lint step (D-496).

**Out of scope.**

- det-lint itself (PR-46), content (PR-5), and the tick (PR-6).
- Log lines and crash files, which Core never writes (PR-44, D-491).

**Exit tests.**

1. Fixed test vectors of each fixed-point operation give the same result on every leg.
2. A division by zero throws with the seed, the tick, and the ids.
3. An overflow in a checked context throws with its context.
4. Two subsystem streams of one seed never give the same sequence.
5. One seed gives one state hash on the three legs and on the Mac (D-504).
6. A changed expected hash fails the job until the identity file changes on purpose.
7. det-lint passes on the new Core code.
8. A Release build of Tests proves that the assertion helper still throws (T-2, G-18).

**Review focus.**

- The simulation version starts at its first value, and G-17 binds each later Core PR.
- No call reaches `GetHashCode` or a .NET hash class from Core (F-35).
- Each string order in Core is ordinal, and det-lint proves it (F-39).
- The rounding rule of OQ-60 has one implementation, not one for each system (T-1).
- A release export cannot run here, because PR-54 creates the export job (D-503, G-16). PR-54 adds the export check.

**Questions.** OQ-60, OQ-61, and OQ-62.

> *In plain English:* different computers can give different answers for decimal math. This adds our own whole-number math and a check that proves the same result on every machine.

### 7.10 PR-5: content, the content hash, and the string table

Area files: `area-core.md` section 7.7, `area-ci.md` section 7.10, `area-tools.md` section 7.1.

**Scope.**

- One C# record for each content type, with a strict reader (D-116, D-177, G-6).
- A reader with no runtime reflection, with the reflection switch in the place that OQ-179 sets (F-36).
- The permanent content id, in the form of OQ-63 (D-166).
- The content hash over the rule files alone, and the layout of `content/` that draws the line (D-495).
- The string table, from an id to text, with a test for each id that content names (D-167, G-7).
- The embed of `content/` in the Game assembly, the folder reader in Tools, and the match test (D-508).
- The `eol=lf` rule in `.gitattributes`, so each checkout holds the same bytes.

**Out of scope.**

- The content of the game. This PR ships fixture files alone.
- The atlas and the palette (PR-34), and the maps (PR-7).

**Exit tests.**

1. A content file with an absent field fails the load with the file and the field.
2. A file with an unknown field fails the load.
3. A number with a fraction or an exponent fails the load (G-2).
4. A repeated content id fails a test with the id.
5. The content hash is the same on the three legs and on the Mac.
6. The embedded resources match the files of `content/` by name and by bytes.
7. A read of a resource that the assembly lacks fails with the resource name.
8. A string id that content names and the table lacks fails a test.

**Review focus.**

- No reflection path survives, in the reader or in a fallback (F-36).
- The hash covers the rule files alone, and a test proves that no other file reaches it (D-495).
- The line-end rule holds on the Windows leg (the external facts of `area-core.md`).
- The Godot export needs no filter for content, because the assembly carries it (F-42).

**Questions.** OQ-62, OQ-63, and OQ-179.

> *In plain English:* every enemy, item, and map lives in a strict data file. A gap or a typo stops the load with the file and the field, instead of a silent zero.

### 7.11 PR-6: the tick, the intents, the run record, and replay

Area files: `area-core.md` sections 7.8, 7.9, and 7.13, `area-ui-input.md` section 7.9, `area-release.md` section 7.1.

**Scope.**

- The fixed-rate loop that calls Core 60 times a second, with a tick count in Core (D-164, G-3).
- The intent, which names a choice in content ids and state ids (D-84, D-493).
- The run record header: the format version, the simulation version, the content hash, the seed, the initial state, and the game version (G-5, D-448).
- The recorder, the replay, and the compaction rule of a snapshot plus the intents after it (F-10, OQ-65).
- The encoding of OQ-66.
- The Core seam that takes extra intent handlers from the host, and the mark of a debug intent (D-171, D-260, D-492).
- A replay run in the identity set (D-504).

**Out of scope.**

- The save files (PR-43) and the crash and log files (PR-44).
- The debug assembly and the console (PR-45).
- The input map, the glyphs, and the remap, which live in Game (PR-61, PR-63).

**Exit tests.**

1. A property test over one thousand seeds replays each record to the same end-state hash.
2. A record of another simulation version fails with both values.
3. A record of another content hash fails with both values.
4. A record with a debug intent fails on a host with no debug handler, and the report names the tick.
5. The record size stays bounded over a long fixture run (F-10).
6. The replay run of the identity set passes on the three legs and on the Mac.
7. det-lint finds no clock read and no OS random in Core.

**Review focus.**

- The header holds each field of D-448, and OQ-168 settles where the game version comes from.
- Core names no debug assembly, and a release host passes no handler (D-492).
- The snapshot moment of OQ-65 keeps the record bounded and the replay exact.
- Game makes each intent from an input event, never from a poll (F-50).

**Questions.** OQ-64, OQ-65, OQ-66, and OQ-168.

> *In plain English:* the game writes down its start state and every choice after it. That record plays any run again on any machine, so every bug becomes repeatable on demand.

### 7.12 PR-43: the Storage project, the snapshots, and the saves

Area files: `area-core.md` section 7.11, `area-exploration.md` section 7.4.

**Scope.**

- The sixth project, `TheThingBelow.Storage`, which holds the file code (D-494).
- The save folder of D-62 and D-465, with one slot save, one autosave, and a one-use resume file (D-258).
- The snapshot bytes from Core, with the tick and the position of every stream (D-166).
- The snapshot format version, one migration step to each next version, and a fixture save of each older format.
- The safe write: a temporary file with a checksum, then one replace (D-178).
- A load that reads the snapshot alone, so a new simulation version never refuses a save (D-259).

**Out of scope.**

- The crash files and the log files (PR-44).
- The moment of each save in Game, which the save point of PR-16 sets (D-224).
- The import of a prologue save, which Phase 6 holds (D-163).

**Exit tests.**

1. A save reloads to the same state hash.
2. A write that stops in the middle leaves the old save intact (D-494).
3. A checksum failure names the file and the reason.
4. A stored save of each older format loads through its migration.
5. A save from an older simulation version loads (D-259).
6. The resume file works one time, and a second use fails with its reason (D-258).
7. Core holds no reference to Storage, and the Core reference test proves it (G-1).

**Review focus.**

- One copy of the safe write exists, with its test (D-494, T-1).
- The folder name is right on each of the three systems (D-465, F-33).
- The migration test reads a real stored save, not a save that the test just wrote.

**Questions.** OQ-65 and OQ-66.

> *In plain English:* a save is a full picture of the game at one moment. A crash during a save never destroys the old one, and a save from an older build still loads through a converter.

### 7.13 PR-44: the crash files and the log files

Area files: `area-core.md` section 7.12, `area-release.md` section 7.10.

**Scope.**

- The crash file beside the save, written through Storage on a crash or a failed assertion (D-170, T-2).
- The content of the crash file: the error with its context, the versions, and the run record (D-448).
- A log line on a crash, and an exit. PR-61 adds the message on screen through the text helper (D-559).
- The log entries that a step of Core returns, with the tick and the subsystem (D-179).
- One JSON object for each log line, written by Storage, with the wall-clock time from Game (D-179).

**Out of scope.**

- The message on screen and the studio address of D-473, which PR-61 adds with the text helper (D-559).
- The title screen and its version line (PR-33).
- The night records and the bot reports (PR-15, PR-49).

**Exit tests.**

1. An error in Core writes a crash file that holds its context.
2. A failed assertion writes the same file.
3. The crash file holds no personal data (D-170).
4. A test loads the record of a crash file and reaches the same state hash.
5. Each log line parses as one JSON object.
6. Core adds no time value and no file path to a log entry (G-1, G-3).
7. A crash writes its file and a log line, and it draws no Godot text property, so det-lint passes (D-499, D-559).

**Review focus.**

- No empty catch, and no error that hides the first error (T-2, G-18).
- PR-44 shows no message, so no Godot text property appears before the text helper of PR-61 exists (D-499, D-559).
- The crash path runs with no content loaded, because a load failure can start it.

**Questions.** None. OQ-57 blocks the address, which PR-61 adds (D-559).

> *In plain English:* when the game stops with an error, it leaves one file that holds everything a replay needs. Logs are plain one-line notes that the tools can read.

### 7.14 PR-47: the PNG code

Area file: `area-tools.md` section 7.5.

**Scope.**

- A PNG reader and a PNG writer in Tools for 8-bit RGB and RGBA images (D-176).
- The compression through the `ZLibStream` class of .NET.
- The CRC-32 of each chunk, from the answer of OQ-72.
- A clear failure on a PNG of another kind, with the file and the reason (T-2).

**Out of scope.**

- The atlas (PR-34), the normal maps (PR-48), and the PNG import (PR-51).
- No image reaches Game, which draws nothing yet.

**Exit tests.**

1. A round trip of a fixture image gives the same pixels.
2. An indexed PNG fails with the file and the reason.
3. A 16-bit PNG fails with the file and the reason.
4. A truncated file fails with the file and the reason.
5. A file with a wrong CRC-32 fails with the chunk name.
6. Each test compares decoded pixels and never PNG bytes (F-19).

**Review focus.**

- The answer of OQ-72 settles the package or the hand code, and a package needs its decision row (G-13).
- The code uses integer math alone, and det-lint proves it (D-502).
- Each failure carries the file and the reason (T-2).

**Questions.** OQ-72.

> *In plain English:* every picture that the tools make or read is a PNG file. The project writes its own small PNG code, so a new version of a library never breaks a picture test.

### 7.15 The screen scale probe

Owner and a session, before PR-7 and PR-34. Area files: `area-ui-input.md` section 7.2, and `area-art.md` section 7.1.

**Scope.**

- One mock frame of 1280 by 720, from the approved sample grids and the body font (D-402, D-621).
- The frame at 1x and at 2x, on the Deck, on a 27-inch 4K screen, and on a 32-inch 1440p screen.
- The owner picks the scale of the world and the scale of the UI, and OQ-183 takes the answer.
- M-8 records the apparent size of a sprite and of the body font on each screen.

**Out of scope.**

- The probe never merges to `main`, and it changes no setting of the Game project (D-597).
- The art batch of PR-34, and the map and battle layouts that follow the answer.

**Exit tests.**

1. The owner reads the same mock frame on all three screens, at both scales.
2. A decision row records the scale of the world and the scale of the UI, and it resolves OQ-183.
3. M-8 holds the numbers of each screen.

**Review focus.**

- The probe draws the same content at each scale, so the owner compares the scale alone.
- D-37 gives each screen the view of the Deck, so one answer binds every screen.

**Questions.** OQ-183.

> *In plain English:* a 32-pixel figure is about 4 mm tall on the Deck, which is small. This shows the owner the same picture at two sizes on three screens, before any art or map takes the size as fixed.

### 7.16 The Sprite Fusion test

Owner and a session, before PR-34. Area file: `area-art.md` section 7.1.

**Scope.**

- The same subjects, drawn by a session as text grids and made by the Sprite Fusion generator (D-620).
- One sheet with both sets, and the owner picks the source of the art.
- The cost, the license, and the path into the pipeline of any tool that wins (G-13, D-620).

**Out of scope.**

- The branch spike/sprite-fusion never merges, and no paid tool joins CI (D-620).
- The atlas command and the art batch of PR-34.
- The PNG import of PR-51, which reads a picture back into a text grid.

**Exit tests.**

1. The sheet holds both sets of the same subjects, at the same size.
2. The owner picks the source, and a decision row records the pick.
3. A pick of Sprite Fusion also records the cost, the license, and the path through PR-51.

**Review focus.**

- A generated picture enters the pipeline as a text grid alone, and the palette of 64 colors binds it (D-181).
- G-24 keeps the text grid as the source of every picture, and a change to it needs a decision row.

**Questions.** None. D-620 sets the test, and the pick needs a row of its own.

> *In plain English:* the owner wants to compare pictures that a session draws with pictures that an outside tool makes. This test draws the same things both ways and lets the owner choose.

### 7.17 PR-34: the atlas, the palette, and the drawing files

Area files: `area-art.md` sections 7.1 to 7.4 and 7.6, `area-tools.md` section 7.6.

**Scope.**

- The `atlas` command that renders the drawing files into the atlas in `content/sprites/` (D-107, D-119, D-406, D-515).
- The grid schema of D-108 and D-109, with the sizes of D-228, D-234, and D-236.
- The frame list of a sprite, with the frame time of OQ-88.
- The five cast grids, which come from the approved sample in `docs/samples/` (D-233, D-402, D-405).
- The palette of 64 colors, with the swatch sheet for the approval of the owner (D-181, D-185, D-238, F-17).
- The atlas index, and the record of it in Core that no rule reads (D-517).
- The review sheets of the batch, which `gh` attaches to the PR description (D-514, G-25).
- The pixel test of F-19, and the retirement of `docs/tools/make-atlas.py`.

**Out of scope.**

- The normal maps and their override grids (PR-48, D-496).
- The large pictures (PR-55) and the map preview (PR-52).
- Game draws nothing yet. PR-7 loads the atlas.

**Exit tests.**

1. The command builds the atlas from the five cast grids.
2. The pixel test decodes the committed atlas and matches the grids on every leg (F-19, G-24).
3. The same test reads the atlas index and finds each frame.
4. A grid with an unknown key fails with the file, the line, and the column (F-20).
5. A palette with a repeated key fails with the key (F-20).
6. A grid of a size that no rule allows fails with the file and the size.
7. det-lint finds no float type in the command (D-502).

**Review focus.**

- The palette holds 64 colors, and each element and status has one (F-17, D-181).
- The test compares pixels, never bytes (F-19).
- The answer of OQ-85 sets the pages, and OQ-86 sets how the atlas places a tile.
- The swatch sheet and the review sheets reach the PR description, and the owner approves the batch (G-25).

**Questions.** OQ-85, OQ-86, OQ-87, and OQ-88.

> *In plain English:* every picture in the game starts as a text file of letters, one for each pixel. This command turns the letters into the one image that the engine draws, and a test proves that they still match.

### 7.18 M-1 and M-2: the first measurements

Area file: none. The cost model in section 4 of `docs/design.md` holds both rows.

**Scope.**

- M-1 records the harness usage of each of the first ten code PRs, in the order of section 8.
- The ten are PR-1, PR-2, PR-3, PR-46, PR-4, PR-5, PR-6, PR-43, PR-44, and PR-47. PR-34 is the eleventh.
- M-2 records the wall time of each CI job on each leg, for the same ten PRs.

**Out of scope.**

- No budget and no limit. These two measurements report, and a later decision can act (G-14).

**Exit tests.**

1. Each of the first ten code PRs adds its two numbers to the cost model.
2. The Gate 1 report states the mean and the worst case of each number.

**Review focus.** Each PR review confirms that the two numbers reached the cost model.

**Questions.** None.

> *In plain English:* the first ten changes each record what they cost in machine time and in money. That gives the owner real numbers before the plan grows.

### 7.19 Gate 1: the foundation gate

**The gate.** Gate 1 passes when every line holds:

1. The build, test, and format job is green on the three legs (D-481).
2. The smoke job is green on the three legs.
3. The `ste-check` job is green.
4. The `review-gate` check is green, and the owner requires it on `main` (OQ-3).
5. The `det-lint` job is green.
6. The `replay-identity` job is green, and the three legs and the Mac agree with the identity file (D-504).
7. `make verify` passes on the Mac.
8. The atlas pixel test passes on the three legs (F-19).
9. The cost model holds the M-1 and M-2 numbers of the first ten code PRs.

**What the gate does not ask.** No play, no screen, and no sign-off on feel. Gates 2 to 5 hold those (D-52).

> *In plain English:* at this point the game does nothing that a player can see. Every check that guards the project is live, and four machines agree on the result of the same run.

## 8. Sequence

The global order lives in section 8 of `docs/design.md`, and PR #11 set it (D-488). Phase 1 holds this order:

1. Owner: enable the repository setting that requires a SHA pin for an action (D-511). Done on 2026-09-14.
2. Owner and a session: the Deck test, which picks the renderer and measures the budget (D-160, D-523). Done on 2026-09-17.
3. Owner: run the Deck test on the Linux export (D-458). Done on 2026-09-17 (D-616, D-617).
4. PR-1: the scaffold, the four projects, the gate jobs, and the local gate.
5. PR-82: the Mobile renderer of the Deck test, which is one line of `project.godot` (D-599, D-616).
6. PR-2: the STE checker in C#.
7. PR-3: the review gate.
8. PR-84: the context budget check, right after PR-3 (D-611).
9. PR-85: the result of the Deck test, the removal of the CRT, and the two tests before PR-34 (D-616).
10. Owner: require the checks on `main` (OQ-3).
11. PR-46: det-lint, before the first Core code (D-496).
12. PR-4: integer math, the streams, the state hash, and the identity job.
13. PR-5: content, the content hash, and the string table.
14. PR-6: the tick, the intents, the run record, and replay.
15. PR-43: the Storage project, the snapshots, and the saves.
16. PR-44: the crash files and the log files.
17. PR-47: the PNG code, right before the atlas (D-496).
18. Owner and a session: the screen scale probe on three screens (D-621, OQ-183).
19. Owner and a session: the Sprite Fusion test of the art (D-620).
20. PR-34: the atlas, the palette, and the drawing files.
21. M-1 and M-2: the numbers of the first ten code PRs, from PR-1 to PR-47 in the order above.
22. **← GATE 1 (foundation).** Section 7.19 holds each line.

The next phase file is `phase-2-first-playable.md`. Between the two, the owner sets the fonts: Terminus TTF and Terminus TTF Bold (D-263, D-264).

## 9. Open questions

The register is `docs/questions.md` (D-19). These questions block an item of Phase 1, and each PR asks its questions when it starts (D-487):

| Question | Subject | Blocks |
|---|---|---|
| OQ-3 | The required checks on `main` | Waits for PR-3 |
| OQ-60 | The rounding rule of fixed-point math | PR-4 |
| OQ-61 | The random generator and the stream split | PR-4 |
| OQ-62 | The hash function of Core | PR-4 and PR-5 |
| OQ-63 | The form of a content id | PR-5 |
| OQ-64 | The tick while a menu is open | PR-6 |
| OQ-65 | When the run record takes a new snapshot | PR-6 and PR-43 |
| OQ-66 | The encoding of records and snapshots | PR-6 and PR-43 |
| OQ-67 | The rule for numbered items | Answered by D-604 |
| OQ-68 | What the reference check fails | Answered by D-605 |
| OQ-69 | What counts as a change to a decision row | PR-3, answered by D-609 |
| OQ-181 | The paths of the metadata set | PR-3, answered by D-610 |
| OQ-182 | Where the context budget check goes | PR-84, answered by D-611 |
| OQ-70 | How det-lint finds the Godot assembly | PR-46, answered by D-614 |
| OQ-71 | Which collection uses det-lint fails in Core | PR-46, answered by D-615 |
| OQ-72 | The CRC-32 of the PNG code | PR-47 |
| OQ-75 | The test runner mode | PR-1, resolved by D-592 |
| OQ-76 | The coverage package and the form of the report | PR-1, resolved by D-593 |
| OQ-77 | The runner labels of the CI legs | PR-1, resolved by D-594 |
| OQ-78 | The required checks on a docs PR | PR-1, resolved by D-595 |
| OQ-83 | How CI gets the Godot editor | PR-1, resolved by D-596 |
| OQ-85 | The pages of the atlas | PR-34 |
| OQ-86 | How the atlas places tiles | PR-34 |
| OQ-87 | The form of a review sheet | PR-34 |
| OQ-88 | The unit of the time of a frame | PR-34 |
| OQ-92 | Where the source of the Deck test scene lives | The Deck test, resolved by D-597 |
| OQ-93 | How the owner reads the frame time on the Deck | The Deck test, resolved by D-598 |
| OQ-168 | Where the game version lives in the build | PR-6 and PR-31 |
| OQ-183 | The scale of the frame on a screen | PR-7 and PR-34, and the probe of D-621 answers it |
| OQ-179 | Where the reflection switch of the JSON reader lives | PR-5 |

No open question blocks this file.
