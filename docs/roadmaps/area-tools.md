# Area roadmap: Tools

Status: **active focused area roadmap, which PR #11 merged on 2026-09-16.** This file says how the Tools project works, and it names the PR that builds each tool (D-144, D-485). The phase files give each PR its scope, its exit tests, and its review focus. This file cites each decision by its id and never restates it. It supersedes no earlier file. Written 2026-09-14 in ASD-STE100.

The design doc holds the system map (section 3), the cost model (section 4), and the guardrails (section 6). The file `area-core.md` holds the Core contracts that det-lint enforces, and `area-ci.md` holds the jobs that run each tool. The files `area-art.md`, `area-exploration.md`, `area-story.md`, and `area-audio.md` hold the formats that the content tools read.

External facts, each with the date of its check:

- A real literal "without a suffix or with the `d` or `D` suffix is a `double`". Source: `https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types`, read 2026-09-14.
- The results of double math "might differ slightly by platform because of the loss of precision of the Double type". Source: `https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-double`, read 2026-09-14.
- The string method `CompareTo` performs "a word (case-sensitive and culture-sensitive) comparison using the current culture". The default comparer of `SortedDictionary` is `Comparer<T>.Default`, and for a string key it uses the `IComparable<T>` form of `CompareTo`. Sources: `https://learn.microsoft.com/en-us/dotnet/api/system.string.compareto`, `https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.sorteddictionary-2.-ctor`, and `https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.comparer-1.default`, read 2026-09-14.
- String sort order "differs between NLS and ICU", and "Moving between versions of ICU can subtly impact app behavior". On Linux, .NET tries to load "the latest installed version of ICU from the system". Source: `https://learn.microsoft.com/en-us/dotnet/core/extensions/globalization-icu`, read 2026-09-14.
- The package `Microsoft.CodeAnalysis.CSharp` carries the MIT license from Microsoft. Its latest stable version on NuGet is 5.9.0, published 2026-08-17. Sources: `https://www.nuget.org/packages/Microsoft.CodeAnalysis.CSharp` and the NuGet API files of the package, read 2026-09-14.
- The API page of `ZLibStream` names the assembly `System.IO.Compression.dll` and no package, from .NET 6 on. The API page of `Crc32` names the package `System.IO.Hashing`. Sources: `https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.zlibstream` and `https://learn.microsoft.com/en-us/dotnet/api/system.io.hashing.crc32`, read 2026-09-14.
- A PNG chunk ends with "A four-byte CRC calculated on the preceding bytes in the chunk", which covers the chunk type and the data and not the length. Source: `https://www.w3.org/TR/png-3/`, the W3C Recommendation of 2025-06-24, read 2026-09-14.
- `pull_request_target` runs "in the context of the default branch of the base repository". The events `pull_request_target`, `schedule`, and `workflow_dispatch` each "will only trigger a workflow run if the workflow file exists on the default branch". The activity types of `pull_request_target` include `labeled` and `unlabeled`. Source: `https://docs.github.com/en/actions/reference/workflows-and-actions/events-that-trigger-workflows`, read 2026-09-14.
- The activity types of `pull_request_target` also include `edited`. "By default, a workflow only runs when a `pull_request_target` event's activity type is `opened`, `synchronize`, or `reopened`." Source: the events page above, read 2026-09-16.

Text rules: this file follows ASD-STE100 (D-10). Tables are exempt from sentence-length counts.

## 1. Thesis

Tools is the one console project for every command that is not the game (D-101, D-118). Gate tools pass or fail a PR: the STE checker, the review gate, det-lint, and the night gate. The headless runner plays runs with no screen, and the night gate reads its results. Content tools turn the text sources of content into pictures and readable text for the owner, and into the files that Game reads. A tool never decides an outcome of play, because Core decides every outcome (D-100).

The order of the area follows the first user of each tool (D-496, D-497). The gate tools come first, because every later PR must pass them. The `det-lint` command lands before the first Core code of PR-4 (D-496). The PNG code and the atlas close Phase 1, and the night gate follows the bots of PR-15. Each other tool lands right before the first PR that needs it.

## 5. Findings that bind this area

The register in section 5 of `docs/design.md` holds every finding. These rows bind the Tools area.

| # | Finding | Binds |
|---|---|---|
| F-5 | The two checkers apply the limit for numbered items in different places | PR-2: one rule (D-604) |
| F-11 | The interim checker read an HTML comment as prose | PR-2: the rule MD 1 fails a comment across lines |
| F-19 | Compressed PNG bytes depend on the encoder | PR-34 and PR-48: tests compare decoded pixels |
| F-20 | A repeated palette key passed in silence | PR-34: a repeated key fails |
| F-35 | Two hash paths of .NET break Core rules | PR-46: det-lint fails both paths in Core |
| F-36 | The JSON support of .NET uses reflection by default | PR-46: det-lint fails reflection in Core |
| F-37 | Two GitHub triggers start only from `main`, so their checks cannot run on the PR that creates them | PR-3 and PR-49: a proof in Tests (D-500) |
| F-38 | A double hides in C# with no keyword, and double results can differ by platform | PR-46 and PR-48: a lint that reads types, and integer math (D-498, D-502) |
| F-39 | The default string order of .NET follows the culture and the ICU version of the machine | PR-4 and PR-46: an ordinal order for strings in Core |
| F-58 | No check can see the conversation of a session | PR-3: the document rules read the diff and the description alone (D-579) |
| F-82 | The Tools scan of det-lint asked for a reference set that its own process already holds | PR-47: the scan takes the framework list (D-614) |
| F-83 | Five commands read an empty option value with no check at the parse, and five values ended with a stack trace | PR-34 and PR-87: every command reads its option values through one helper (D-678, D-679) |
| F-87 | The Sprite Fusion generator does not hold the size of the call, and it returned up to 42 pixels | PR-51: the generator mode crops the border and sets a frame of 32 or 64 pixels (D-689) |
| F-89 | A picture of that generator uses 206 to 1275 colors, and almost none of them is a palette color | PR-51: the generator mode maps each pixel to the nearest color (D-688) |

## 7. Roadmap

Each part below says how one tool works, which decisions set it, and which PR builds it. The phase files give the scope and the exit tests of each PR (D-144). Each part ends with a plain-English paragraph for a reader who does not know the code.

### 7.1 The Tools project

Built by PR-1 and PR-2. Phase file: `phase-1-foundations.md`.

- PR-1 creates Tools as a console project with no command, and PR-2 adds the first command (D-118).
- One program holds every command, and the first argument names the command, as the commands in `CLAUDE.md` show.
- Tools references Core and Storage. It reads Core state, and it writes records through Storage (D-494).
- Tools holds the one reader of the `content/` folder for the tools and Tests, and PR-5 adds it (D-508). Game reads content from its own assembly.
- PR-5 adds the `content-hash` command. It loads every content file, and it compares the hash of the rule files with a committed file (G-5, D-648).
- A run of that command with `--write` writes the committed file again, as the `replay-identity` command does. The review then reads the new value (G-17).
- Each package in Tools needs a decision (G-13). D-498 is the first, for det-lint.
- A gate tool prints one line per finding with the file, the line, the rule id, and what it saw. It exits 1 on any finding (the `ste-writing` skill).
- A tool that cannot finish names the file and the reason, and it exits with a code other than 0 (T-2).
- Every command reads its option values through the one helper `OptionValue.ReportEmpty`, and PR-87 adds it (D-679).
- An empty option value reads at the parse, and never as a stack trace. A path check further down throws a type that no command catches (F-83).
- Tests holds the tests of each command, with a fixture that passes and a fixture that fails each rule (T-3).

> *In plain English:* Tools is one program with many commands, and none of them is the game. Each command checks, plays, or draws something, and each failure says exactly what went wrong.

### 7.2 The STE checker

Built by PR-2, and PR-84 adds the size rules. Phase file: `phase-1-foundations.md`.

- The `ste-check` command is new code (D-101, D-277). PR-2 retired the Python script `docs/tools/ste-check.py`, and the `ste-check` job now runs the command (D-10).
- It carries the rules of the checker table in the `ste-writing` skill and the comment rule of F-11.
- It skips the dated records that the skill names.
- F-5 needed one rule for numbered items. D-604 sets it: the limit reads every numbered item, under any heading.
- The reference check reads each id, each path of this repository, and each superseded decision that a live document cites (D-605, D-606).
- The session number check fails a number that appears twice in the handoff and its archive, and an entry out of order (D-18, L-12). It also fails more than 10 entries in the handoff (D-607).
- PR-84 adds the size rules of the context budget (D-611). The limits are 16 KB for `CLAUDE.md`, 5 KB for the top entry of the handoff, and 36 KB for a skill file.
- The rules SIZE 1, SIZE 2, and SIZE 3 give one finding for each file above its limit. The finding names the size and the limit.
- `AGENTS.md` takes the limit of `CLAUDE.md`, because D-20 keeps the two files identical and D-583 names each one in the start set.
- SIZE 3 reads every `.md` file of `.claude/skills/`. SIZE 2 reads the top entry alone, from its heading to the next heading (D-584).
- The count reads the lines of a file, and each line ending counts as one byte. Thus every CI leg reads the same number.
- `TheThingBelow.Tools/SteCheck/SizeRules.cs` holds the three limits, and the `ste-writing` skill repeats them.
- SIZE 2 and the session number check share one detector of a session heading. Thus the two rules agree on where an entry starts (D-18, L-12).

> *In plain English:* every document must pass a check for plain technical English. This moves the check from a borrowed script into the language of the project, with the same rules and a few more.

### 7.3 The review gate

Built by PR-3. Phase file: `phase-1-foundations.md`.

- The `review-gate` command is new code, and its workflow runs on `pull_request_target` (D-15, D-101, D-277).
- The workflow runs the command from `main` and reads the files of the PR head as data. It never runs code from the head (D-15).
- The command applies the three rules of the `pr-review` skill. The record exists, the verdict is `Ready for owner merge`, and the head field names the effective head.
- The command passes a PR in the override set with the `review-override` label that changes no decision row (D-16, D-71, D-239, D-401). A change to a row is a change to a line of a decision table (D-609). The command refuses the label on a PR that changes `.claude/settings.json` (D-700).
- A PR that changes `.github/workflows/` fails on the label, because each gate lives in a workflow file (D-560).
- The command applies the document rules of D-579. The handoff changes, and the Documents section has a line for each required row (D-581). No line defers a document or a record of the PR (D-577). A line that names the PR of independent roadmap work passes (G-16).
- The workflow also runs when a label or the PR description changes, because both change the result (D-67, D-579). The type `edited` needs its own line in the workflow.
- A metadata commit never moves the effective head. The metadata set holds the two review files of the PR and the two handoff files (D-610).
- The workflow gives the command one JSON file with the facts of the PR, and one folder with the files of the head.
- GitHub starts this trigger only from `main`, so the check cannot run on PR-3 (F-37). PR-3 proves the command in Tests, and the live check first runs on the next PR (D-500).
- After PR-3 merges, the owner requires the check on `main` (OQ-3).

> *In plain English:* this check turns red when a change has no approved review from the other provider. It also turns red when a change leaves its documents for later. It reads each change as data and never runs it, so a change cannot approve itself.

### 7.4 det-lint

Built by PR-46. Phase file: `phase-1-foundations.md`.

- The `det-lint` command is new code (D-101, D-277). It lands before PR-4, so the first Core code meets it (D-496).
- It reads C# through the Roslyn compiler library, and it checks the type of each expression, not the words (D-498, F-38).
- In Core, it fails a float type, the clock, OS random, reflection, and the two hash paths of F-35 (G-2, G-3, F-36).
- In Core, it fails a string order that does not use an ordinal comparison (G-4, F-39). It fails a walk of a `Dictionary` or a `HashSet`, and it passes a lookup by key (D-615).
- In Game, it fails a Godot text property outside the text helper, and a text value in a scene file (D-499, G-7). It fails each member of a Godot type whose name holds the word `Text` or the word `Title`. A committed list holds the other members that draw text, such as `DrawString` (D-614).
- It reads the code of the atlas, the normal maps, the PNG reader and writer, and the synthesizer (D-502). There it applies the Core rules for float types, the clock, and OS random.
- D-502 binds each tool whose output a test compares on every leg, so a later tool of that kind joins the list.
- PR-46 proves each rule on a fixture that breaks it, such as a fixture with `double` (G-16).
- Each tool of D-502 keeps its code in its own folder of Tools. The command holds the folder and the PR of each one: `Atlas` (PR-34), `Audio` (PR-38), `NormalMaps` (PR-48), and `Png` (PR-47). The command names each absent folder with its PR, and it fails nothing (G-16).
- A compilation error is rule DL 0. The command gives no rule result for a file set that it cannot compile. A rule on an error type is silent and wrong (T-2).
- `area-ci.md` holds the `det-lint` job.

The rules, with the id that each finding carries:

| Rule | What it fails | Where |
|---|---|---|
| DL 0 | A compilation error, so no rule reads that file set | Every project |
| DL 1 | A float type: `float`, `double`, `decimal`, or `Half` | Core, D-502 tools |
| DL 2 | A clock: a date type, a time type, `Stopwatch`, or the tick count | Core, D-502 tools |
| DL 3 | An OS random: `Random` or a new identifier | Core, D-502 tools |
| DL 4 | Reflection: the reflection namespace, `Activator`, a member of `Type`, or `dynamic` | Core |
| DL 5 | A hash path of F-35: `GetHashCode`, `HashCode`, or a hash class of .NET | Core |
| DL 6 | A string order or a string comparison with no ordinal comparison | Core |
| DL 7 | A walk of a `Dictionary` or a `HashSet`, or of an interface of either type | Core |
| DL 8 | A Godot text member outside the one text helper | Game |
| DL 9 | A text value in a Godot scene file | Game |
| DL 10 | A thread, a task, or a SIMD vector | Core |

- DL 4 passes `typeof(X)` alone, because an attribute of the content reader takes it (F-36). It fails each member of `Type` that reflects.
- DL 6 also fails a comparison that follows a culture, such as `CompareTo` or `StartsWith` with no comparison value (F-39).
- DL 7 reads one expression at a time. It reads the interfaces of the two types too, because a field of an interface type hides the collection (D-615).
- DL 1 also reads the type that an expression converts to, so a whole number that a call takes as a `double` fails (F-38).
- DL 8 also reads a call that sets a property by a name that holds `text` or `title`. It reads each menu item call of the committed list too (D-614).
- DL 9 reads the last part of a property name, because Godot writes the text of a menu item as `item_0/text`.
- A compile error of the Tools scan stays in the report whatever file it names, so a scan that read nothing never passes (F-93).

> *In plain English:* two computers can disagree on decimal math and on the order of words. This tool reads the rules code as the compiler does and refuses anything that can make two machines disagree. It also refuses on-screen text that skips the string table.

### 7.5 The PNG code

Built by PR-47. Phase file: `phase-1-foundations.md`.

- A small PNG reader and writer in Tools handles 8-bit RGB and RGBA images, with tests on fixture files (D-176). It lands right before PR-34 (D-496).
- It compresses and decompresses the image data through the `ZLibStream` class of .NET (the external facts above).
- Each PNG chunk ends with a CRC-32, and Tools holds a short CRC-32 of its own for it (D-663).
- The reader restores each of the five row filters, and the writer writes the filter None (D-664).
- Two files of an outside encoder prove the reader, and the test builds the bytes of each failure (D-665).
- A PNG of another kind, such as an indexed PNG, fails with the file and the reason (D-176, T-2).
- A test compares decoded pixels and never PNG bytes, because the compressed bytes depend on the encoder (F-19).
- The atlas, the normal maps, the PNG import, the map preview, and the frame compare of the screen tests use it (D-176).

> *In plain English:* every picture that the tools make or read is a PNG file. The project writes its own small PNG code, and the tests compare pixels, so a new version of a library never breaks a picture test.

### 7.6 The atlas

Built by PR-34. Phase file: `phase-1-foundations.md`.

- The `atlas` command renders the drawing files into the atlas in `content/sprites/`, and it replaces the interim script of D-406 (D-107, D-119, D-515).
- Beside the atlas, the command writes the atlas index, the place of each frame in the atlas. Core holds its record (D-517). The `--check` option compares the committed atlas and writes no file.
- `area-art.md` holds the drawing files, the frames, the sizes, and the palette. This file holds the command.
- The command uses integer math alone, and each color is a palette lookup (D-502).
- A grid with an unknown key fails with the file, the line, and the column. A palette with a repeated key fails with the key (F-20, T-2).
- A test decodes the committed atlas and compares its pixels with the drawing files on every CI leg (F-19, G-24). The same test reads the atlas index.
- The command also renders the swatch sheet and the review sheets of a batch, and the session attaches them to the PR description (D-185, D-514, D-668). The `--sheets` option names the folder, which lies outside git.

> *In plain English:* every picture in the game starts as a text grid of letters. This command turns the grids into the one image that the engine draws, and a test proves that the image still matches the letters.

### 7.7 Normal maps

Built by PR-48. Phase file: `phase-2-first-playable.md`.

- The command builds a normal map for each grid from its shape, with an optional override grid (D-183, D-184). It lands right before PR-56, the first PR that draws light (D-520, D-521).
- It uses integer math alone, with an integer square root, so every CI leg gives the same pixels (D-502, F-38).
- The normal-map atlas takes the same pixel test as the color atlas (D-184, F-19).
- A review sheet draws each sprite under eight fixed light directions for the owner, and the session attaches it to the PR description (D-514, D-521).
- Each piece of a large picture gets a normal map too (D-516).

> *In plain English:* a normal map tells the light which way each pixel faces, so a torch can light one side of a face. The tool builds it from the drawing with whole-number math, so every computer makes the same map.

### 7.8 The headless runner and the bots

Built by PR-15. Phase file: `phase-2-first-playable.md`.

- The runner plays runs in Tools with no Godot, from a seed and a policy (D-64, D-100). A policy makes the same intents that Game makes (D-493).
- The two policies are random and greedy (D-64). A policy takes its random numbers from a source outside the rule streams (G-4, `area-core.md` section 7.4).
- Each run writes its run record through Storage (D-494). It ends as complete, softlock, crash, or budget, and each failure names its seed. OQ-74 holds how the runner finds a softlock.
- A few hundred runs play on each PR, on every CI leg (D-64, D-505). OQ-80 holds the count, and `area-ci.md` holds the job.
- The `playtest-bot` agent drives the runner and reports what it finds (D-21).

> *In plain English:* simple robots play the game with no screen. They make the same choices that a player makes, and every crash they find comes with the seed that repeats it.

### 7.9 The night gate

Built by PR-49. Phase file: `phase-2-first-playable.md`.

- The night job plays ten thousand runs on Linux, and two thousand each on Windows and macOS (D-507). Each leg writes a night record (D-509). It lands right after PR-15 (D-496).
- The `night-gate` command fails a PR when no success record comes from a night inside the last 48 hours (G-22).
- The night job runs on `schedule`, which GitHub starts only from `main` (F-37). PR-49 proves the command in Tests on a fixture record, and the live check first runs after the first night (D-500).
- The night record is an artifact of its run, and the check finds it through the GitHub API (D-509).
- A night on the head commit of a PR passes that PR alone, and a docs-only PR passes the gate (D-510, D-513). `area-ci.md` holds the jobs.
- M-3 records the wall time and the crash and softlock counts of the first seven nights.

> *In plain English:* every night the robots play thousands of runs on all three systems. No change merges unless a recent night ended with no crash and no softlock.

### 7.10 The screenplay tool

Built by PR-50. Phase file: `phase-2-first-playable.md`.

- The command prints each story scene script as a screenplay, with the text of each string id, for the PR description (D-173, G-25). It lands right after PR-68, because it needs the scene format and the string table alone (D-545).
- A story scene that names an absent string id fails with the story scene, the step, and the id (T-2).
- The tool looks each cue up in the audio file, because a story scene names no cue (D-548).
- `area-story.md` holds the story scene format.

> *In plain English:* a story scene is a list of steps in a data file. This tool prints it like a script, so the owner reads the story scene as a story before approval.

### 7.11 The PNG import

Built by PR-51. Phase file: `phase-2-first-playable.md`.

- The command holds two modes: the hand-edit mode and the generator mode (D-688). It lands before PR-17 (D-497).
- The hand-edit mode reads a PNG that the owner edited by hand, and it writes the frame of its drawing file again (D-107, D-515).
- A pixel with a color outside the palette fails in that mode, with the file, the pixel, and the color. It never picks a near color (T-2).
- The generator mode reads a picture of the Sprite Fusion generator (D-686). It crops the blank border, then it sets a frame of 32 or 64 pixels (D-689).
- That mode maps each pixel to the nearest color of the palette of 64, and it reports the count of the mapped pixels (D-181, D-688).
- The generator mode fails when the content does not fit the frame of 64 pixels, with the file and the size (D-689, T-2).
- The PNG code refuses an indexed PNG, so a hand edit exports as RGB or RGBA (D-176).

> *In plain English:* the owner can fix a sprite in a paint program, and this tool writes the edited image as a text grid again. It refuses any color that the palette lacks. A second mode reads a picture from the art tool, trims it, and pulls each color to the closest palette color.

### 7.12 The map preview

Built by PR-52. Phase file: `phase-2-first-playable.md`.

- The command renders a map file as a PNG from the atlas, for the owner's approval (D-165). It lands before PR-17 (D-497).
- The session attaches each preview to the PR description (D-514).
- `area-exploration.md` holds the layout format.
- From PR-53 on, the preview draws the tiles of the edge file of each map (D-501).

> *In plain English:* maps are text files too. This tool draws a map as a picture, so the owner can see and approve a place before anyone walks it.

### 7.13 The tile-edge tool

Built by PR-53. Phase file: `phase-2-first-playable.md`.

- The command picks the edge and corner tile for each position from the terrain of a map and the edge rules in content (D-204). It lands before PR-17, after the map preview (D-497).
- It writes one edge file per map outside the rule files, and the repository commits it (D-501). The content hash never reads an edge file (D-495).
- A test proves that each edge file matches its map and the edge rules (D-501).
- Game reads the map and its edge file, and Core reads the map alone (D-501, G-1).
- A terrain pattern that no rule covers fails with the map, the position, and the pattern (T-2).

> *In plain English:* a map names the ground, such as snow or rock, and this tool picks the right border tile for each edge. The picks live in a file of their own, so a new border drawing never breaks an old replay.

### 7.14 Tools that other area files hold

The table names the tools that live in Tools while another area file holds their format or their job.

| Tool | Area file | PR |
|---|---|---|
| Audio synthesizer, hash list, and listen command | `area-audio.md` | PR-38 |
| The sound room in a development build | `area-audio.md` | PR-71 |
| The runs that the `replay-identity` job compares | `area-ci.md` | PR-4 |
| The `screens` command: the frame compare and the contact sheet | `area-ci.md` | PR-41 |
| The render of large pictures | `area-art.md` | PR-55 |

The synthesizer keeps the integer math of D-432, and det-lint reads its code (D-502).

### 7.15 The contract of every later tool PR

Each later tool PR keeps this list. The phase files make exit tests from it.

1. Give the tool its own PR id (D-486).
2. Add a decision for each new package (G-13).
3. Name the file, the place, and the reason in each failure (T-2).
4. Add a fixture that passes and a fixture that fails each rule (T-3).
5. Use integer math when a test compares the output on every CI leg (D-502).
6. Compare decoded pixels, never PNG bytes (F-19).
7. Name each player string by its id alone (G-7).

> *In plain English:* every new tool follows the same seven steps. Each one fails loudly, proves itself with good and bad samples, and gives the same answer on every computer.

### 7.16 The balance harness

Built by PR-90. Phase file: `phase-4-region-one.md`.

- A third policy plays as a careful player, beside the random and greedy policies of PR-15 (D-64, D-822).
- The `balance` command plays a loop of seeds and reports the metrics of each encounter, dungeon, item, and lesson (D-822). OQ-216 holds the metrics.
- A content file holds one band for each metric, and the owner sets each band from the first report (D-571, D-822).
- The report uses integer math, so every CI leg gives the same numbers (D-502).

> *In plain English:* a careful robot measures how hard the game is. The owner sets the limits, and the nightly check keeps each later change inside them.

## 8. Sequence

The global order lives in section 8 of `docs/design.md`, and PR #11 set it (D-488). The Tools PRs keep this order inside it:

1. PR-1: the Tools project, with no command.
2. PR-2: the STE checker.
3. PR-3: the review gate. Its live check first runs on the next PR (D-500).
4. PR-84: the context budget check in the `ste-check` command (D-611).
5. PR-46: det-lint, before the first Core code (D-496).
6. PR-4, PR-5, PR-6, PR-43, and PR-44: the Core PRs of `area-core.md`.
7. PR-47: the PNG code, right before the atlas (D-496).
8. PR-34: the atlas.
9. PR-87: the empty option value of every command, before Gate 1 (D-674, D-678).
10. **← GATE 1 (foundation).** The gate tools and the atlas test pass on every CI leg.
11. PR-55: the render of large pictures, right before PR-10 (D-518).
12. PR-48: the normal maps, right after PR-10 and right before PR-56, the first PR that draws light (D-520, D-521).
13. PR-50: the screenplay tool, right after PR-68 (D-545).
14. PR-15: the headless runner and the bots.
15. PR-49: the night gate. Its live check first runs after the first night (D-500).
16. PR-51, PR-52, and PR-53: the PNG import, the map preview, and the tile-edge tool, before PR-17 (D-497).
17. **← GATE 2 (first playable).**
18. PR-90: the balance harness, first in Phase 4 (D-822).

## 9. Open questions

The register is `docs/questions.md` (D-19). These questions block Tools PRs, and each PR asks its questions when it starts (D-487):

- D-604 answers OQ-67, and it sets the rule for a numbered item. D-605 answers OQ-68, and it sets what the reference check fails.
- D-609 answers OQ-69, and a change to a decision row is a change to a line of a decision table.
- D-610 answers OQ-181, and the metadata set holds four paths of the PR.
- D-611 answers OQ-182, and the size rules of the context budget go in PR-84.
- D-614 answers OQ-70, and the text rule reads the Godot assembly of the Game build output.
- D-615 answers OQ-71, and det-lint fails a walk of either type in Core.
- D-663 answers OQ-72, and Tools holds a CRC-32 of its own. D-664 and D-665 answer OQ-195 and OQ-196.
- OQ-74: how the runner finds a softlock. Blocks PR-15.
- OQ-216: the metrics, the bands, and the policy of the balance harness. Blocks PR-90.
- OQ-3: the required checks on `main`. Waits for PR-3.
