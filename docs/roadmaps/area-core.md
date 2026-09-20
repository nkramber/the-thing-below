# Area roadmap: Core

Status: **active focused area roadmap, which PR #11 merged on 2026-09-16.** This file says how the Core project works, and it names the PR that builds each part (D-144, D-485). The phase files give each PR its scope, its exit tests, and its review focus. This file cites each decision by its id and never restates it. It supersedes no earlier file. Written 2026-09-14 in ASD-STE100.

The design doc holds the thesis of the game, the system map (section 3), the cost model (section 4), and the guardrails (section 6). The `csharp-conventions` skill gives the code rules that apply these contracts. The files `area-tools.md` and `area-ci.md` hold the tools and the CI jobs that enforce them.

External facts, each with the date of its check:

- The .NET hash classes "defer to the OS libraries". Source: `https://learn.microsoft.com/en-us/dotnet/standard/security/cross-platform-cryptography`, read 2026-09-14.
- The hash code of a string "is not guaranteed to be stable", and "two subsequent runs of the same program may return different hash codes". Source: `https://learn.microsoft.com/en-us/dotnet/api/system.string.gethashcode`, read 2026-09-14.
- "System.Text.Json uses reflection by default". With the MSBuild property `JsonSerializerIsReflectionEnabledByDefault` set to `false`, a call that needs reflection throws an `InvalidOperationException`. Source: `https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation`, read 2026-09-14.
- From .NET 8, the JSON reader can refuse a property with no matching member, and it throws a `JsonException`. Source: `https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/missing-members`, read 2026-09-14.
- For integers, the C# `/` operator gives the quotient "rounded toward zero", and a division by zero throws a `DivideByZeroException`. By default, integer arithmetic runs in an unchecked context, and a `checked` context throws an `OverflowException` on an overflow. Source: `https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/arithmetic-operators`, read 2026-09-14.
- The Git for Windows installer defaults to the line-ending choice `CRLFAlways`, which maps to `core.autocrlf` set to `true`. The GitHub runner image script installs Git with no line-ending option. Sources: `https://raw.githubusercontent.com/git-for-windows/build-extra/main/installer/install.iss` and `https://raw.githubusercontent.com/actions/runner-images/main/images/windows/scripts/build/Install-Git.ps1`, read 2026-09-14.
- The variables `core.autocrlf` and `core.eol` set the line ends of a checkout only "If the `eol` attribute is unspecified for a file". Source: `https://git-scm.com/docs/gitattributes`, read 2026-09-14.

Text rules: this file follows ASD-STE100 (D-10). Tables are exempt from sentence-length counts.

## 1. Thesis

Core is the game with no screen. It holds every rule that decides an outcome (D-100). A rule gives the same result in the Game, in the bots, in a replay, and on every CI leg (T-7, D-481). Exploration, battle, progression, and story put their rules in Core, and each area keeps the contracts of this file. Game, Tools, and Storage give Core bytes and intents, and they show or write what Core returns (D-493, D-494).

The order of the area follows dependency. Integer math, random streams, and the state hash come first, because every rule computes with them (PR-4). Content and the string table come next, because every rule reads content (PR-5). The tick, the run record, and replay follow, because a run needs rules and content (PR-6). Saves, crash files, and log files close Phase 1, because they write what the loop makes (PR-43, PR-44, D-491). The debug assembly waits for the first screen (PR-45, D-492).

## 5. Findings that bind this area

The register in section 5 of `docs/design.md` holds every finding. These rows bind the Core area.

| # | Finding | Binds |
|---|---|---|
| F-10 | The run record grows with no limit over a long play | PR-6: a snapshot at each save, plus the intents after it (D-651) |
| F-25 | A quit autosave can trap a run (C-3), and a Core patch refuses old saves (C-4) | PR-43: the resume file of D-258 and the load of D-259 |
| F-27 | The debug console of D-171 meets the rule of no conditional compilation in Core | PR-6 and PR-45: the seam and the assembly of D-260 and D-492 |
| F-35 | Two hash paths of .NET break G-1 and T-7 | PR-4: xxHash64 in Core. PR-5: the SHA-256 of the content hash (D-644, D-645) |
| F-36 | The JSON support of .NET uses reflection by default | PR-5: a reader with no runtime reflection |
| F-39 | The default string order of .NET follows the culture and the ICU version of the machine | PR-4: an ordinal comparer for every string order in Core |

## 7. Roadmap

Each part below says how one part of Core works, which decisions set it, and which PR builds it. The phase files give the scope and the exit tests of each PR (D-144). Each part ends with a plain-English paragraph for a reader who does not know the code.

### 7.1 The boundary

Built by PR-1 and PR-4. Phase file: `phase-1-foundations.md`.

- Core is a class library with no package reference and no project reference, and a test asserts that list (G-1, D-100). PR-1 adds the test.
- Core opens no file, no socket, and no process, and it reads no clock and no OS random (G-1, G-3). It takes bytes, a seed, and intents, and it returns state, bytes, and events (D-168, D-493).
- Game, Tools, Storage, Tests, and the debug assembly reference Core, and Core references none of them (D-260, D-494).
- The host gives Core the bytes of each content file. Game reads them from its own assembly, and Tools and Tests read `content/` through the reader in Tools (D-508). Core never sees a disk path (G-1).
- The det-lint of PR-46 enforces G-2 and G-3 in Core, and F-35 adds the two hash paths (D-496). The file `area-tools.md` holds the lint rules.

> *In plain English:* Core is the rules of the game with no screen, no files, and no clock. Everything else hands it data, then shows or saves what it decides.

### 7.2 The shape of Core code

Built by PR-4 and every later Core PR. Phase files: every phase file.

- D-168 sets the shape: plain state, one static class per system, and events for Game.
- One step of the loop runs the systems in one fixed order (G-4). A change of that order changes behavior, so it bumps the simulation version (G-17).
- Events carry what happened to Game (D-168). The state hash reads the state and never the events.

> *In plain English:* each rule system is a plain function over plain data. It runs in the same order every step, so two computers always agree.

### 7.3 Integer math

Built by PR-4. Phase file: `phase-1-foundations.md`.

- Core holds no float type (G-2). Content writes a fraction in basis points, and a fixed-point type in Core names its scale (D-169).
- A product or a quotient rounds toward zero in every system (D-641). `BasisPoints` holds the one multiply and the one divide, and no system writes its own.
- Core uses `checked` arithmetic wherever a content value can drive a result, and an overflow throws with its context (T-2). By default, C# integer math wraps in silence (the external facts above).
- A zero divisor fails with the seed, the tick, and the ids (T-2).
- PR-4 proves the type with fixed test vectors, and the replay-identity job compares the results on every CI leg (G-5).

> *In plain English:* the rules never use decimal math, which can differ between machines. Every percentage is a whole number, and every overflow stops the run with a clear report.

### 7.4 Random streams

Built by PR-4. Phase file: `phase-1-foundations.md`.

- Each subsystem draws from its own stream, split from the run seed (G-4). A subsystem never draws from the stream of another.
- The split never uses `GetHashCode`, because a string hash can change between two runs (F-35). The generator is PCG32, and each stream takes its seed from the run seed and its stream number alone (D-642, D-643).
- Game effects, the bot policies, and the screen tests take their random numbers from sources outside Core. A draw for a particle never moves a rule stream (G-4, G-23).
- The host picks the seed of a new run: Game for play, and Tools for the bots (G-3, D-64).
- The snapshot holds the position of every stream, so a load continues the same sequence (D-259, PR-43).

> *In plain English:* every dice roll comes from a numbered stream that starts from the seed of the run. The same seed gives the same rolls, and a sparkle on screen never steals a roll from a fight.

### 7.5 Order and hashes

Built by PR-4. Phase file: `phase-1-foundations.md`.

- Every loop that reaches the state walks a fixed order. `List<T>` and `SortedDictionary<TKey, TValue>` keep that order, and `Dictionary<TKey, TValue>` and `HashSet<T>` never reach the state (G-4).
- A `SortedDictionary` with string keys takes `StringComparer.Ordinal`. The default comparer follows the culture and the ICU version of the machine (F-39). The det-lint of PR-46 fails every other string order in Core.
- The state hash reads the whole state in that fixed order, with xxHash64 (D-644, F-35). PR-4 created it, and each later Core PR adds its state to it.
- The simulation version is a constant in Core. PR-4 creates it, and each Core behavior change bumps it (G-17).

> *In plain English:* the game computes one number from its whole state, the state hash. Two machines that play the same run must get the same number, and CI checks that on every change.

### 7.6 Errors and assertions

Built by PR-4. Phase file: `phase-1-foundations.md`.

- A Core error throws an exception type that carries its context (T-2, G-18). Inside a run, the context is the seed, the tick, and the entity ids. Outside a run, it is the file and the field.
- Assertions use the project helper, and they stay on in a release export (T-2). A release export drops every `Debug.Assert` (the external facts of `docs/design.md`).
- PR-4 creates the helper and the first exception types, because it holds the first Core code.
- Core writes no log line and no crash file. PR-44 adds the log entries that a step returns, and Storage writes them (D-179, D-491, D-494). Core holds the text of a log line and of a crash line, and it makes no time and no path (G-1, G-3).
- Before PR-44, an error in Core throws with its context and stops the run, and the test or the host shows it.

> *In plain English:* when a rule breaks, the game stops and says exactly where: which run, which step, and which thing. It never guesses a value and continues.

### 7.7 Content and the string table

Built by PR-5. Phase file: `phase-1-foundations.md`.

- Each content type is a C# record, and a strict reader refuses an absent field, an unknown field, and a wrong type (D-116, D-177, G-6).
- Core reads each file with a hand reader on `Utf8JsonReader`, and Core calls `JsonSerializer` nowhere (D-647, F-36).
- `Directory.Build.props` sets `JsonSerializerIsReflectionEnabledByDefault` to `false` for every project, and a test reads the switch back (D-647). The det-lint of PR-46 bans reflection in Core (D-496).
- A number in content is an integer. A number with a fraction or an exponent fails the load, with the file and the field (D-169, G-2).
- Every content entry has a permanent id that no later entry takes (D-166). An id is a lowercase kind, a dot, and a lowercase name (D-646).
- Each rule record owns the kind of its entry ids, and the reader refuses an entry of another kind (D-646). A field that points at another record keeps the kind of that record.
- The content hash covers the rule files alone (D-495). One folder, `content/rules/`, holds every rule file, and a test proves that no other file reaches the hash (D-648).
- The `content-hash` command of Tools loads every content file and compares the hash with a committed file. The `--write` option writes that file again (D-648).
- Core holds one fixture rule record until the first real rule record replaces it (D-649). Its files give the hash, the id rules, and the string-id rule real data to read.
- PR-8 writes the first real rule record. It removes the fixture record, the two fixture files, and the three fixture labels of the string table. It then writes the content hash again (D-649, D-166).
- PR-5 also writes the SHA-256 that makes the content hash, in Core code beside its one caller (D-644, D-645). Its test holds the published vectors of the reference implementation.
- The content hash reads the same bytes on every CI leg. The `eol=lf` rule of `.gitattributes` keeps each checkout on LF line ends, where Git for Windows otherwise defaults to CRLF (the external facts above).
- Game embeds the files of `content/` in its assembly, and a test proves that the embedded set matches the folder (D-508). PR-5 adds the embed, the folder reader in Tools, and the test, and `area-ci.md` holds the details.
- The string table maps ids to text, and Core events name string ids alone (G-7, D-167). Game reads the text for an id. A test proves that each string id that content names exists in the table (T-2).
- A string id takes the same form as a content id (D-646). The table refuses a repeated id, and it reads each id in ordinal order (F-39).
- Core also holds the record of each content file that no rule reads, such as the palette and the atlas index (D-517). The content hash still reads the rule files alone (D-495).
- An art file names the content ids that it draws, and a rule file never names art (D-519). `area-art.md` holds the art files.

> *In plain English:* every enemy, item, and map lives in a strict data file. A gap or a typo stops the load with the file and the field, and an art or text change never breaks an old replay.

### 7.8 The tick and intents

Built by PR-6. Phase file: `phase-1-foundations.md`.

- A fixed-step clock in Game calls Core 60 times a second (D-164). Core counts ticks and reads no clock (G-3).
- Game makes an intent from each key, button, and mouse action, and Core reads intents alone (D-493, G-23). The input map, remapping, and the device kind stay in Game (D-214, D-222).
- An intent names what the player chose in content ids and state ids, and never a screen position or a key (D-493).
- Game makes each intent from an input event, never from a poll of `Input` (F-50). A poll sees input that a menu already took. `area-ui-input.md` holds the input.
- A menu pauses the world (D-162). A menu action is an intent too, and the tick rises while a menu is open (D-650).
- The mouse works on menus alone, and a mouse action on a menu makes the same intent as a key or a button (D-219, D-493).
- Game makes no intent from a Godot timer, physics, or navigation (G-23).
- No rule waits for an effect. Where the world waits for one, Game counts the ticks of the effect on its fixed-step clock (D-266). At the end, Game sends a wait intent (D-522). `area-effects.md` holds the effects.

> *In plain English:* the rules never see keys or sticks. They see decisions, such as "step north" or "use this item", so a bot, a replay, and a player all speak the same language.

### 7.9 The run record and replay

Built by PR-6. Phase file: `phase-1-foundations.md`.

- The record header holds the format version, the simulation version, the content hash, the seed, the initial state, and the game version (G-5, D-448).
- The record holds the intents of each tick, and a debug intent carries a mark (D-171, D-492, D-493).
- The record keeps a snapshot and the intents after it, so its size stays bounded (F-10). The record takes a new snapshot at each save (D-651), and it is JSON text (D-652).
- A replay of a record on the same simulation version and content hash reproduces the state hash (G-5). A mismatch stops with a report that names both values (T-2).
- Replay and the bots run in Tools and Tests with no Godot (D-100, D-493). The replay viewer of development builds plays a record in Game (D-175).
- The replay-identity job runs a fixed set of records on every CI leg and compares each hash with the committed identity file (G-5, D-481, D-504). The file `area-ci.md` holds the job, and each later Core PR adds a fixture run and its expected hash.

> *In plain English:* the game stores the start of a run and every choice after it. A replay of that record gives the same run on any machine, so every bug can happen again on demand.

### 7.10 Versions

Built by PR-4, PR-6, and PR-43. Phase file: `phase-1-foundations.md`.

| Version | Where it lives | What changes it | What it guards |
|---|---|---|---|
| Simulation version | A constant in Core (PR-4) | Every Core behavior change (G-17) | The replay of a record |
| Record format version | The record header (PR-6) | A change to the layout of the record | The read of a record |
| Snapshot format version | The snapshot (PR-43) | A change to the layout of the snapshot, with a migration (D-166) | The load of a save |
| Content hash | The record header (PR-6) | A change to a rule file (D-495) | The replay of a record |
| Game version | The record header, the crash file, and the title screen (PR-6, PR-44, PR-33) | Each build that ships (D-448, D-454) | Nothing. It is a label for people |

A load of a save reads the snapshot alone, so a new simulation version never refuses a save (D-259). A replay needs the build that made the record, so the crash file names the game version (D-259, D-448).

> *In plain English:* five numbers tell the game what an old file needs. A patch can change the rules and still load every save, and a bug report still names the exact build.

### 7.11 Snapshots and saves

Built by PR-43. Phase file: `phase-1-foundations.md`.

- Core makes the snapshot bytes and loads a state from them, and Storage writes and reads the files (D-494). Game picks the moment of each save (D-224).
- A snapshot holds content ids and state, and never a copy of content (D-166). It holds the tick and the position of every stream (section 7.4).
- The save folder, the slot save, the autosave, and the resume file follow D-62, D-258, D-465, and D-656.
- A save file takes two lines of JSON: the header with the checksum, and the snapshot (D-655). The checksum is the SHA-256 digest of the bytes of line 2.
- A save writes a temporary file with a checksum, then replaces the old save in one step (D-178). A test in Tests cuts a write in half against Storage (D-494).
- A load reads the snapshot alone (D-259). Each format version has its reader, and a test loads a stored save of each version (D-166, D-654).
- Godot and Storage must give one user folder, and Game compares the two at the start of every session (D-657, F-33).
- From PR-43 on, every Core PR that changes the snapshot bumps its format version and adds a migration and a fixture save (D-166).
- The full game imports the last snapshot of the prologue in Phase 6 (D-163). Each snapshot format of the prologue stays ready for that import.

> *In plain English:* a save is a full picture of the game at one moment. The game writes it safely, so a crash during a save never destroys the old one. A save from an older build still loads, because each change to its shape ships with a converter and a test.

### 7.12 Crash files and log files

Built by PR-44. Phase file: `phase-1-foundations.md`.

- On a crash or a failed assertion, Game writes a crash file to the crashes folder of the user folder through Storage (T-2, D-658). Then it writes a log line and exits with the crash code. PR-61 adds the message on screen through the text helper, with the address of D-473 (D-559).
- The crash file holds the error with its context, the versions, and the run record, and no personal data (D-170). Line 1 is the crash object, and the lines of the record follow it (D-661). A crash before a run holds no record, and line 1 carries the versions itself (D-661, T-2).
- A file error carries its path. Thus the writer hides every folder of the person in the text of an error and of a stack (D-170, T-2).
- A step of Core returns its log entries with the tick and the subsystem, and Storage writes each entry as one JSON line (D-179). Game adds the wall-clock time, and Core never does (D-179).
- The log holds four levels. A menu change takes the info level, and a beat of the patrol the debug level. A log file holds the info level and above, and a session argument adds the debug lines (D-660).
- One log file belongs to one session, in the logs folder. Each of the two folders keeps the newest 10 files (D-658, D-659).
- The name of a crash file and of a log file carries the stamp of the wall-clock time in UTC. Storage reads no clock, and the host passes each time (D-658, G-3).

> *In plain English:* when the game crashes, it leaves a file for the player to email, with everything that a replay of the run needs. Logs are simple one-line notes that the tools can read.

### 7.13 The debug seam

Built by PR-6 and PR-45. Phase files: `phase-1-foundations.md` and `phase-2-first-playable.md`.

- Core takes a list of extra intent handlers from the host at the start of a run (D-260). Core never names the debug assembly (D-492).
- A development build passes the debug handlers, and a release build passes none (D-260). A debug intent in a record carries a mark (D-171).
- A host with no debug handlers refuses a record with a debug intent. The report names the intent and the tick (T-2).
- PR-45 creates the debug assembly, the console, and the test that a release export never loads the assembly (D-492).
- Two later features live behind the same seam: the capture of PR-74, and the sound room of PR-71 (D-439, D-546, D-551).

> *In plain English:* cheats exist only in development builds, as a separate part that the shipped game never contains. A run that used a cheat still replays, and the record says so.

### 7.14 The contract of every later Core PR

Each Core PR after Phase 1 keeps this list. The phase files make exit tests from it.

1. Bump the simulation version for any change to Core behavior (G-17).
2. Draw random numbers from a stream of the new system alone (G-4).
3. Add the new state to the state hash and to the snapshot (sections 7.5 and 7.11).
4. Bump the snapshot format version, and add a migration with its fixture save (D-166).
5. Add a fixture run and its expected hash to the identity file (G-5, D-504).
6. Give each new content type a C# record and a load test (D-177, G-6).
7. Name every player string by its id alone (G-7).

The systems that later areas add to Core are below. Each area file confirms its rows when it lands.

| System | Area file | First PR |
|---|---|---|
| Tile map, movement, sight, and the time of day of a map | `area-exploration.md` | PR-7 |
| Enemies on the map | `area-exploration.md` | PR-8 |
| Battle and the timeline | `area-battle.md` | PR-9 |
| The enemy record: stats and ability ids | `area-battle.md` | PR-80 |
| The elements and the statuses | `area-battle.md` | PR-66 |
| The evaluator and profiles | `area-battle.md` | PR-11 |
| The character level, the experience, MP, and the stat curves | `area-progression.md` | PR-67 |
| Lessons, the slots, the growth, and the aptitudes | `area-progression.md` | PR-12 |
| Gear, items, and the inventory | `area-progression.md` | PR-13 |
| Hub services | `area-exploration.md` | PR-14 |
| Story scenes, the story scene runner, the join step, and the choices | `area-story.md` | PR-68 |
| Dungeon parts, downs, and save points | `area-exploration.md` | PR-16 |
| Traps, hazards, and the statuses that last on the map | `area-exploration.md` | PR-64 |
| The shop and the gold economy | `area-exploration.md` | PR-65 |
| The region map | `area-exploration.md` | PR-35 |
| Story flags and conditions | `area-story.md` | PR-68 |
| Story branches and the choice effects | `area-story.md` | PR-18 |
| Quests and personal tasks | `area-story.md` | PR-19 |
| Boss phases | `area-battle.md` | PR-20 |
| Puzzles and secrets | `area-exploration.md` | PR-21 |

> *In plain English:* every later rule, from a sword swing to a story choice, follows the same seven steps. That keeps the whole game replayable, saveable, and testable as it grows.

## 8. Sequence

The global order lives in section 8 of `docs/design.md`, and PR #11 set it (D-488). The Core PRs keep this order inside it:

1. PR-1: the Core project and the test of its reference list.
2. PR-4: integer math, the streams, the state hash, the errors, and the simulation version.
3. PR-5: content, content ids, the content hash, and the string table.
4. PR-6: the tick, intents, the run record, replay, and the debug seam.
5. PR-43: the storage project, the snapshot versions and migrations, and the save files.
6. PR-44: crash files and log files.
7. **← GATE 1 (foundation).** The identity job and the tests pass on every CI leg.
8. PR-7: the first Core system of a later area, and the first screen.
9. PR-45: the debug assembly and the console, right after PR-7 (D-492).
10. The later Core PRs of section 7.14, in the order of the phase files.

## 9. Open questions

The register is `docs/questions.md` (D-19). These questions block Core PRs, and each PR asks its questions when it starts (D-487):

- OQ-60, OQ-61, and OQ-62 closed on 2026-09-18 with D-641 to D-645.
- OQ-63 is resolved (D-646). OQ-179 is resolved (D-647).
- OQ-64: the tick while a menu is open. Resolved 2026-09-18 by D-650.
- OQ-65: when the run record takes a new snapshot. Resolved 2026-09-18 by D-651.
- OQ-66: the encoding of records and snapshots. Resolved 2026-09-18 by D-652.
- OQ-57: the studio name. Blocks the crash address, which PR-61 adds (D-473, D-559).

No open question blocks this file.
