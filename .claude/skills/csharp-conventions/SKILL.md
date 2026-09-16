---
name: csharp-conventions
description: The C# rules of this repo. The engine-free Core, integer math, error context, no silent failure, test shape, Godot boundaries, and dependency policy. Load before you write or review C#.
---

# C# conventions skill

Load this skill before you write or review C# in this repo (D-21, D-99). It applies the tenets to C# and Godot. The scaffold PR, PR-1, creates the solution, and this skill needs a revision pass after it merges.

## Project boundaries

- `Core` is the simulation. It has no reference to Godot, the file system, the network, the clock, or the OS (G-1, D-100). It takes a seed, content bytes, and intents, and it returns state, bytes, and events (D-168, D-493).
- `Game` is the Godot project. It reads `Core` state, draws it, plays audio, and turns input into intents. Godot physics, timers, and navigation never feed the simulation.
- `Tools` holds the gate tools: the STE checker, the review gate, det-lint, and the night gate. It also holds the headless runner and the PNG code (D-496). Its content tools are the atlas, the normal maps, the PNG import, and the audio synthesizer. The map preview, the tile-edge tool, and the screenplay tool complete the set (D-497).
- A tool whose output a test compares on every CI leg uses integer math, as `Core` does (D-502).
- `Tests` holds the xUnit tests for `Core`, `Storage`, and `Tools`, and the smoke test that starts the Game headless.
- The debug assembly is the fifth project, and only development builds reference it (D-260).
- `Storage` is the sixth project. It holds the file code for saves, run records, crash files, and log files. Game and Tools reference it, and `Core` never does (D-494).
- A test asserts the reference list of `Core`. A new package in any project needs a decision entry (G-13).

## Shape of Core code (D-168)

- State is plain C# records and classes with no engine types.
- Each system is a static class. It takes the state and the intents of one tick, changes the state in a fixed order, and emits events for `Game`.
- No framework, and no entity component system library in `Core`.

## Determinism in Core (T-7)

- No `float`, `double`, or `decimal`. Percentages, multipliers, and rates use fixed-point integers. Content writes each fraction in basis points, where 10000 means 100% (D-169). Name the scale in the type or the constant, for example `BasisPoints`.
- No `System.Random`, `DateTime`, `Stopwatch`, or `Environment.TickCount`. The seed and the tick are the only sources of randomness and time (G-3).
- One random stream per subsystem, split from the run seed. A subsystem never borrows another stream.
- Iterate in a fixed order. Use `List<T>` and `SortedDictionary<TKey, TValue>`, and never `Dictionary<TKey, TValue>` or `HashSet<T>`, where the order reaches the state (G-4).
- Every `Core` behavior change bumps the simulation version constant (G-17).
- Check every arithmetic operation that a content value can drive with `checked`. An overflow is an error with context, never a wrap.
- No reflection, no `dynamic`, no LINQ in a hot loop, no conditional compilation in `Core`.
- No hash from `GetHashCode` or from the .NET hash classes in `Core`. Use the hash function that `Core` holds (F-35).
- Order strings by an ordinal comparison alone. A `SortedDictionary` with string keys takes `StringComparer.Ordinal`, because the default order follows the culture of the machine (F-39).
- Debug intents and their handlers live in a separate debug assembly that only development builds reference (D-260).

## Errors (T-2)

- No empty `catch`. No `catch` that logs and continues without a decision that names it.
- Every exception type carries its context. Inside a run that is the seed, the tick, and the entity ids. Outside one it is the file path and the field.
- An absent value is an error, never a default. `GetValueOrDefault` on a content field is a finding.
- Assertions stay on in release builds. Use the project assertion helper, never `Debug.Assert`.
- Nullable reference types are on, and warnings are errors.

## Content (D-116)

- Content is JSON. A schema validates each file at load and in a test. An unknown field and an absent field are both errors.
- The JSON reader of `Core` runs with no runtime reflection (F-36).
- Game reads content bytes from the resources of its own assembly, and `Tools` holds the one reader of the `content/` folder (D-508). A resource read that returns null fails with the resource name (T-2).
- `Core` holds the record of every content file, also the palette and the atlas index, which no rule reads (D-517).
- An art file names the content ids that it draws, and a rule file never names art (D-519).
- An audio file names the content ids that it serves, and a rule file never names a track (D-548).
- A load error names the file, the field, and the reason.
- One test loads every file under `content/` and fails on the first error.
- Player-visible text is a string id, never a string literal in code (G-7). Game puts it on screen through the one text helper, and det-lint fails a Godot text property outside it (D-499).
- No `.tres` or `.res` file holds game data. The Godot project holds Godot scene files and settings alone.

## Godot

- A screen is a C# class that builds its nodes in code, or a minimal Godot scene file that holds layout alone. Game data never lives in a Godot scene file.
- The Game loop calls `Core` at a fixed rate. Game draws each step as a slide between tiles, and `Core` positions stay on whole tiles (D-106, D-203).
- The camera, the CRT shader, the audio, and the input map live in `Game` and never reach `Core`.
- No rule waits for an effect. Game counts the ticks of an effect on its fixed-step clock, and it sends a wait intent at the end where the world waits (D-266, D-522).
- `Core` runs each story scene and holds its step index. Game draws each step, and it sends the same wait intent when the step ends (D-540).
- Each effect file is JSON with integer values, and a test fails a map or a battle that passes the effect budget (D-517, D-523).
- Game draws the world in a `SubViewport` at 1x, and it builds both steps of the fit itself (D-232, F-48).
- Game builds its `Theme` in code from the UI style file, and no `.tres` theme file exists (D-527, G-6).
- Game loads each font from the bytes of its own assembly into `FontFile.Data`, because Godot 4.7.2 has no byte-array load method (D-508, F-49).
- Game makes each audio stream from the rendered bytes of its own assembly, and it checks every return (D-547, F-56).
- Game draws art from the atlas bytes with the Nearest filter, and no Godot resource file holds art (D-508, F-45).
- Some Godot calls report a failure in the log alone, such as `ImageTexture.CreateFromImage` and the WAV load of F-56. Each one gets a check right after it (T-2, F-45, F-56).
- The Godot editor writes files: `project.godot`, `.csproj` target frameworks, and `.import` files. The PR review reads each one it touches.

## Style

- `dotnet format` clean. Warnings as errors. Suppress a warning only with a comment that names the reason, next to the pragma.
- Explicit over implicit. No interface for a single implementation (T-1). Two concrete cases before an abstraction.
- Helpers go one level deep. A reader understands a method from the method and the signatures of its helpers.
- Name a method for what it does. Name a type for what it is. Avoid a `Manager`, a `Handler`, or a `Util`.
- Public members have a doc comment that states the contract, not the implementation.
- Comments explain a decision or a trap, and they cite a D-# id when one applies. Never a comment that repeats the code.

## Tests (T-3)

- xUnit. One test class per type under test.
- A property test is a seed loop: iterate a fixed seed range, assert the property, and name the seed in the failure message.
- A bug fix ships with a regression test that fails on the old code. The PR description names the test.
- A test asserts the contract, not a copy of the implementation.
- The Smoke category starts the Godot build. CI runs it in the smoke workflow alone.

## Commands

The Makefile is the entry point after PR-1 (D-3). The raw commands, with the names of D-217:

```
dotnet build TheThingBelow.slnx
dotnet test TheThingBelow.slnx --no-build --filter "Category!=Smoke"
dotnet format TheThingBelow.slnx --verify-no-changes
dotnet run --project TheThingBelow.Tools/TheThingBelow.Tools.csproj -- ste-check --root .
dotnet run --project TheThingBelow.Tools/TheThingBelow.Tools.csproj -- det-lint --root .
/Applications/Godot_mono.app/Contents/MacOS/Godot --headless --path TheThingBelow.Game -- --smoke
```

Pin the SDK in `global.json`. Pin the Godot version in the runbook and in CI.
