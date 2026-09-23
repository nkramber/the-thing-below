# Phase roadmap: Phase 2, First playable

Status: **active focused phase roadmap, which PR #11 merged on 2026-09-16.** This file gives each item of Phase 2 its scope, its exit tests, its review focus, and its questions (D-144, D-485, D-487). The area files say how each part works, and each entry names the area file that it cites. This file supersedes no earlier file. Written 2026-09-16 in ASD-STE100.

The design doc holds the thesis of the game, the system map (section 3), and the cost model (section 4). It also holds the guardrails (section 6) and the global order of every PR (section 8). This file cites each decision by its id and never restates it. The index of this folder is `docs/roadmaps/readme.md`.

This file states no external fact. Each external fact of Phase 2 lives in the area file that holds its part, with the date of its check.

Text rules: this file follows ASD-STE100 (D-10). Tables are exempt from sentence-length counts.

## 1. Thesis

Phase 2 turns the machine of Phase 1 into a game that the owner plays. It ends at Gate 2. There the owner walks the village, one hub, and one dungeon on the desktop and on the Deck. Then the owner signs off on feel (D-51, D-92, D-362).

Phase 2 is the largest phase of the plan. It holds 54 PRs, and 51 of them land before Gate 2. Each system, each tool, and each group of screens takes an id of its own (D-486, G-8). The order follows one rule: a PR lands right before the first PR that needs it.

Four lines of work run through the phase. The walk comes first: the frame, the map, the camera, and the enemies on it (PR-61, PR-7, PR-8). The fight follows, with the enemy record and the screen (PR-9, PR-80, PR-66, PR-10). The light and the effects then land, each right after the first map scene or battle scene that it needs (PR-48 to PR-60, D-520). The build, the story, and the audio close the phase, and PR-17 writes the content that the owner plays.

Three PRs land after the Gate 2 build, and this file holds them. They are the store page work of PR-75 and PR-76, and the capture of PR-74 that takes its screenshots (D-550, D-551). The store page goes public at Gate 2 in the Coming Soon state (D-471).

## 5. Findings that bind this phase

The register in section 5 of `docs/design.md` holds every finding. These rows bind an item of Phase 2.

| # | Finding | Binds |
|---|---|---|
| F-7 | A three-character party fights short-handed after a down | PR-16 and M-4: the reserve and the swap of D-58 |
| F-8 | An empty MP pool left a caster with no action | PR-9 and PR-12: the basic attack of D-359 |
| F-67 | A 32-pixel sprite at 1x covers 4.0 mm on the Deck | PR-7 and M-6: the scale of the frame comes from the probe of D-621 (OQ-183) |
| F-23 | A headless session draws nothing, so no CI job can capture a screen | PR-41: the Linux job under Xvfb with a pinned Mesa (D-172) |
| F-24 | A 32-pixel tile holds four times the pixels of the earlier plan | M-6: the Deck at the frame of 1280 by 720 (D-568) |
| F-26 | Four gates rested on a run that draws nothing | PR-41, PR-7, and PR-8: the captures of D-172. PR-37 is retired (D-618) |
| F-31 | Every rendered audio file in git would pass 500 MB | PR-38: the build renders the audio, and git holds the hashes (D-432) |
| F-37 | GitHub starts a schedule only from the default branch | PR-49: the command proves itself in Tests (D-500) |
| F-38 | Double math differs by platform, and a literal with no suffix is a double | PR-48: integer math with an integer square root (D-502) |
| F-41 | Four rules of GitHub Actions meet the CI plan | PR-49: OQ-81 the age of a result, and OQ-82 the time of the night |
| F-42 | No command-line option installs the export templates | PR-54: the job unpacks them from the cache of D-596 |
| F-44 | A full-screen grid holds over a million palette keys | PR-55: a large picture places drawn pieces (D-516) |
| F-45 | Three Godot defaults fight the pixel art | PR-7: the Nearest filter, and a check after each such call |
| F-46 | Godot 2D light fails in silence in three ways | PR-56: a texture check, a height on each light, and the budget test |
| F-47 | Glow can reach a lit sprite in both HDR and SDR | PR-59: a threshold above the brightest lit art, which the load holds (D-910, D-915) |
| F-48 | Godot has no stretch mode that upscales in whole steps, then fits | PR-61: a `SubViewport` at 1x, and both steps in Game (D-232) |
| F-49 | Three font defaults of Godot fight a pixel font | PR-61: the load from bytes and the font settings, with a test |
| F-50 | Five input facts of Godot meet the plan | PR-61, PR-62, and PR-63: intents from events and a saved remap |
| F-51 | Four Godot defaults of the tile map fight the plan | PR-7: the tile size, the region size, and the two switches |
| F-52 | The camera centers a small map, and its smoothing can run twice in a frame | PR-7: a test locks the centering, and the tick moves the camera |
| F-95 | The ground drew over the feet of a sprite inside a step north or south | PR-89: the ground draws below every sprite, and the screen test holds the walk (D-782, D-783) |
| F-53 | The evaluator of D-534 has no measurement | PR-11: the PR reports the cost of a turn before Gate 2 |
| F-54 | The end of the job system left the stats with no source | PR-67: each character carries its own stat curve (D-537) |
| F-55 | The story scene runner needs the flags that PR-18 held in Phase 3 | PR-68: the flag set and the condition form move here (D-544) |
| F-56 | Two Godot audio calls report a failure in the log alone | PR-69 and PR-70: a check on each stream, and a count of its own |

## 7. Roadmap

Each entry below gives one item of Phase 2 its scope, its exit tests, its review focus, and its questions. The area file of each entry says how the part works (D-144). Each entry ends with a plain-English paragraph for a reader who does not know the code.

An exit test is a test or a job that the PR adds and that must pass before the merge. The PR gate of `CLAUDE.md` still applies to each PR, and these tests are the ones that this PR alone can fail.

Every PR that changes Core also keeps the seven steps of section 7.14 of `area-core.md`. Those steps cover the simulation version, the stream, the state hash, and the snapshot with its migration. They also cover the identity run, the content record, and the string id. The entries below name a step only where it needs an exit test of its own.

### 7.1 PR-54: the export job

Area files: `area-ci.md` section 7.11, `area-release.md` section 7.2.

**Scope.**

- The export job of D-449, right before PR-7, so the merge of PR-7 exports the first walkable build (D-503).
- A run on each merge to `main`, and on each pull request that changes one of the five export paths (D-512, D-692, D-699).
- Three exports, one on each leg: Windows and Linux on x86_64, and the universal macOS build (D-481, D-482).
- The unpack of the .NET export templates into the editor data folder of the runner (F-42, D-596).
- A headless smoke session on each export (D-512).
- The three notices of D-467 in `licenses/`, which the job copies into each export (D-691, D-693).
- One packed archive for each leg, and a build artifact that lasts 90 days (D-449).

**Out of scope.**

- The macOS signature and the notarization (PR-79, D-553). The preset signs ad hoc alone.
- The release workflow and the GitHub Release (PR-31).
- The third-party notices of the engine (OQ-198), and the retention of Phase 6 (OQ-199).
- This job is not a line of the PR gate (D-449).

**Exit tests.**

1. Each leg exports its build from a clean checkout.
2. Each export starts with `--headless`. Its smoke session ends with no log error and with the exit code 0 (F-92).
3. Each export holds the three license files of D-467.
4. A pull request that changes an export preset runs the job.
5. Each build artifact appears on the workflow run.
6. A test reads each preset, the project setting of F-74, and the trigger paths of D-692 and D-699.

**Review focus.**

- The template file matches its SHA-512, and the job never takes it from an unpinned source (D-511).
- The export presets name the three targets of D-481 and nothing else.
- Each export needs no include filter, because Game embeds `content/` and each font in its assembly (D-508, F-73).
- The macOS preset takes the universal binary format, and the project turns the ETC2 ASTC import setting on (F-74).
- The macOS preset signs with the command of the Xcode tools, and not with the built-in signer (F-90).
- The smoke step of the export keeps the exit code of the build, and no caller of the session drops it (F-92, D-694).
- The job packs one archive for each leg, because an artifact upload drops the execute bit.
- The owner can download the Linux artifact for a Deck play (D-458).

**Questions.** OQ-83 is resolved (D-596). OQ-198 and OQ-199 block no part of this PR.

> *In plain English:* from the first walkable build on, every merge makes a game that the owner can run on the desktop or the handheld. Each build also starts once in CI.

### 7.2 PR-61: the UI base, the frame, the fonts, and the intents

Area files: `area-ui-input.md` sections 7.1 to 7.5, 7.9, and 7.10.

**Scope.**

- The one 16:9 frame of 1280 by 720, with black bars for every other shape, the Deck included (D-228, D-568).
- The world in a `SubViewport` of 640 by 360, scaled by 2 into the frame, so the frame holds 20 by 11.25 tiles (D-633, D-634).
- Both steps of the fit that Godot cannot make (D-230, D-232, D-573, F-48).
- The body size setting with its two values, and its default on each screen (D-707).
- The two fonts in a fonts folder under `content/`, read from the bytes of the Game assembly (D-508, D-713).
- The six font settings of a pixel font, and one bitmap strike pinned for each size (D-710, F-49).
- The text helper that puts a string table entry on screen, which det-lint guards (D-499, G-7).
- The UI style file, and the Godot `Theme` that Game builds from it at load (D-527, G-6).
- The input map, and an intent from each input event, never from a poll (D-84, D-493, F-50).
- The glyph sets for the keyboard, Xbox, PlayStation, and the Deck, and the name table that picks one (D-222, D-561, D-711). PR-55 removed them (D-815).
- The message of a crash on screen, through the text helper, with a placeholder address in the reserved `.invalid` domain (D-170, D-559, D-712).
- The review sheets of the window frames and the four glyph sets (D-514, G-25). PR-55 removed the glyphs (D-815).

**Out of scope.**

- The menu windows (PR-62) and the settings screen with the remap (PR-63).
- The dialogue box (PR-36) and the map HUD (PR-7).
- The screen captures of the frame and the fit. PR-41 creates the screen-test job and takes them (D-172, G-16).

**Exit tests.**

1. A test locks the size of the frame and of both fit modes on three screens (D-232, D-568, F-48).
2. Those screens are 1280 by 800, 1920 by 1080, and 2560 by 1440.
3. A test reads back the stretch settings and the filter, and the smoke session reads back each font setting (F-45, F-49, D-710).
4. A test proves that the fixture panel holds the longest string of the string table (D-241).
5. det-lint fails a Godot text property outside the text helper.
6. A test proves that no intent comes from a poll of the input singleton (F-50).
7. A prompt shows the glyph of the last device, for each of the four sets. PR-55 removed the prompts (D-815).
8. No text falls below 9 pixels on the Deck frame (D-459).
9. A crash shows its message through the text helper, and det-lint passes (D-499, D-559).
10. The review sheets of the window frames and the glyph sets reach the PR description (D-514).
11. The owner approves that art batch (G-25).
12. A test locks the world viewport at 640 by 360, and the frame at 20 by 11.25 tiles (D-633, D-634).
13. A test proves that each panel holds its text at both body values (D-707, G-28).
14. A test reads the default body value of each of the four screens of M-8 (D-707).

**Review focus.**

- The two steps of the fit of D-573, and the world viewport of 640 by 360 under them (D-634, F-48).
- Font oversampling stays off, and each font setting has a test (F-49).
- The `Theme` comes from the style file, and no theme resource file exists (D-527, G-6).
- Every screen shows the same part of the map, so no screen shape gains knowledge (D-566, D-568).

**Questions.** D-573 resolved OQ-105, D-710 resolved OQ-104, and D-711 resolved OQ-107, and D-815 superseded D-711. OQ-57 stays open, and D-712 gives the crash message a placeholder address.

> *In plain English:* this builds the picture frame of the game. It sets one fixed size that the handheld shows exactly, the two fonts, and the look of every menu. It also turns keys and buttons into choices that the rules understand.

### 7.3 PR-7: the tile map, the movement, the sight, and the map scene

Area files: `area-exploration.md` sections 7.1 to 7.5, `area-ui-input.md` section 7.7, `area-art.md` section 7.7.

**Scope.**

- The map file of D-528, which holds the terrain rows and every thing that a rule reads (D-39, D-41).
- Those things are the doors, the locks, the chests, the traps, the save points, the spawn points, and the markers (D-386).
- The time of day of the map, which a story flag can change (D-442).
- Tile-locked movement in four directions, and the sight of the party and of a patrol, in Core (D-100, D-106, D-716, D-718, D-719). PR-55 removed the sight of the party (D-814).
- The record of each tile that the party walked, in Core and in the snapshot, which the map screen of PR-62 reads (D-567).
- The map scene in Game, with the tiles from the atlas and the Nearest filter (F-45). The view holds 20 by 11.25 tiles (D-633).
- The camera on the lead, with the limits of a large map and the centering of a small map (D-106, D-717, F-52).
- The map in the place of the demo panel of PR-61, with the row of button prompts (D-722). PR-55 removed the row (D-815).
- The first content: one fixture dungeon, and the drawing of each tile kind.

**Out of scope.**

- The enemies on the map (PR-8) and the dungeon parts (PR-16, PR-64).
- The map HUD, which PR-64 builds with the health mark and the status mark (D-212, D-390, D-721).
- The rules of a door, a lock, a chest, a trap, and a save point (PR-16, PR-64).
- The light (PR-56) and the edge tiles (PR-53).
- The story scene triggers, which PR-68 reads from the same file (D-528).

**Exit tests.**

1. The party walks the fixture dungeon on the three systems, with a keyboard and with a gamepad.
2. The camera never scrolls past the edge of a map larger than the view.
3. A test locks the centering of a map smaller than the view (F-52).
4. A test reads back the tile size, the region size, the collision switch, and the navigation switch (F-51).
5. A thing on a tile of the wrong kind fails the load with the map, the position, and the kind.
6. A property test over one thousand seeds proves that the walked-tile record never forgets a walked tile (D-567).
7. The replay of a walk gives the same state hash on every leg.

**Review focus.**

- Game moves the camera from the tick, never from the smoothing of Godot (D-203, D-717, F-52).
- Each call that reports a failure in the log alone gets a check right after it (F-45, T-2).
- Game draws through a tile map layer, and it builds the `TileSetAtlasSource` from the tile page at load (D-667).
- The sort value of a sprite larger than one tile (D-206, OQ-115).

**Questions.** None. D-715 resolved OQ-89, D-716 resolved OQ-117, D-717 resolved OQ-118, and D-718 to D-720 resolved OQ-114. D-566 resolved OQ-116, and D-667 resolved OQ-86.

> *In plain English:* this is the first thing that the owner can open and move in. The dungeon is a grid of tiles, the party walks it one tile at a time, and the view follows.

### 7.4 PR-45: the debug assembly and the console

Area file: `area-core.md` section 7.13.

**Scope.**

- The debug assembly, which holds the console and the extra intent handlers of the seam of PR-6 (D-260, D-492).
- The console of D-171, which a development build opens and a release build lacks. The key is backquote, and the world keeps its ticks (D-725).
- The four commands of D-724: `reveal`, `hash`, `where`, and `help`.
- The seam of Game, which loads the assembly by name outside the `ExportRelease` configuration (D-723).
- The mark that a debug intent carries in the run record (D-171).
- The test that a release export never loads the assembly (D-492, D-726).

**Out of scope.**

- The capture (PR-74) and the sound room (PR-71), which land later behind the same seam.
- No change to Core, which names no debug assembly (D-492).
- A command that takes a value, because an intent carries an id alone (D-493, D-724).

**Exit tests.**

1. A development build opens the console and runs each command.
2. A release export holds no reference to the debug assembly.
3. A record with a debug intent carries its mark (D-171).
4. A host with no debug handler refuses that record with the tick and the intent.

**Review focus.**

- Core names no debug assembly, and no conditional compilation enters Core (D-260, F-27).
- Each console command that changes the run makes an intent, so a cheat still replays (D-724).
- The seam is text and reflection, so a test holds each name of it (D-723).
- The commands stay engine-free, and det-lint reads them with the rules of Core (D-724, T-7).

**Questions.** OQ-210 to OQ-215, which D-723 to D-728 answer.

> *In plain English:* cheats and test commands live in a separate part that the shipped game never contains. A run that used one still replays, and the record says so.

### 7.5 PR-41: the screen-test job

Area files: `area-ci.md` section 7.12, `area-effects.md` section 7.13, `area-ui-input.md` section 7.13.

**Scope.**

- The Linux job of D-172, which runs Godot under Xvfb with the Mobile renderer. It draws on lavapipe, the software Vulkan driver of a pinned Mesa (F-23, D-729, D-730, D-731).
- The `--capture` argument of Game, which draws each fixture and writes one PNG for each capture (D-732).
- The two fixtures: the map screen, and the UI panel with the window frame and the longest string (D-734).
- The capture of each fixture at 1x, and both fit modes at 1080 and 1440 screen rows (D-232, D-568).
- The `screens` command of Tools, which compares decoded pixels with the committed baseline of `screens/baseline` (F-19, D-736).
- The fixed sources of change at capture: the fixture seed, and a session that runs no tick (T-7).
- The artifact of every run, from which the author commits a new baseline by hand (D-733).
- The desktop command that makes a contact sheet with the real renderer (D-172, D-735).

**Out of scope.**

- The effect captures, which each effect PR adds (PR-56 to PR-60).
- No change to the game, beyond the fixture scenes that the job captures.

**Exit tests.**

1. The job passes on the map scene of PR-7.
2. The job captures the frame of PR-61 at 1x, and both fit modes at 1080 and 1440 screen rows (D-232, D-568).
3. One changed pixel of the baseline fails the job.
4. Two runs of the job give the same frames.
5. The job fails on an error line in the Godot log (T-2).
6. The contact sheet command runs on the Mac of the owner.

**Review focus.**

- The Mesa pin of D-730, its decision row under G-13, and the read back of each installed version.
- The job runs the Mobile renderer of the shipped build on a software Vulkan driver (D-616, D-731). A fallback to another driver fails the job (T-2).
- The baseline holds both fit modes at both screen row counts (D-232, D-568).
- The capture at 1080 rows proves the rule of D-573. Every pixel keeps the same size, with a slight softness at pixel edges.

**Questions.** D-729 resolved OQ-79 on 2026-09-20, and D-730 holds the Mesa pin.

> *In plain English:* the computers that check each change have no screen. This job gives one of them a software screen with a fixed picture, so a broken screen fails before it merges.

### 7.6 PR-8: the enemies on the map

Area file: `area-exploration.md` sections 7.6 and 7.7.

**Scope.**

- The `enemies` array of the map file, with the id, the group, the size, the step, and the sight of each enemy (D-738, D-752 to D-754).
- Fixed enemies and patrols in one record, where a route of one tile stands still, and no random encounter (D-37, D-739, D-740).
- The area of a large enemy, the random step inside it, and the proof at load that its body fits everywhere in it (D-206, D-209, D-741).
- The routes by the time of day of the map, and the enemies that a time keeps off the map (D-193, D-442, D-743).
- The sight of each enemy, the mark for a beat, and then the start of the encounter (D-208, D-718, D-745).
- The block of every tile of a body, and the step of the party into one, which starts the encounter at once (D-206, D-747).
- The side that reached the other from behind, which the encounter records (D-265, D-746).
- The grace time after a flee, and the console command that ends an encounter until PR-9 (D-381, D-748, D-749).
- The enemies in the snapshot, at save format 3, with the reader of each older format (D-654, D-750).
- The sprites of the enemies and the mark in Game, with one fixture drawing for the enemies (D-737, D-744).

**Out of scope.**

- The fight itself and the hand-off of the encounter (PR-9, D-531), and the battle screen (PR-10).
- The transition over the hand-off (PR-60).
- The three views and the two-frame walk of a moving enemy, which PR-17 draws with the enemies of the first playable (D-207, D-744).
- The enemy groups and their profiles (PR-11, D-535), and the stats of an enemy (PR-80, D-557).

**Exit tests.**

1. A property test over one thousand seeds proves that a patrol never leaves its route.
2. The same test proves that a patrol never sees through a wall.
3. The time of day of the map picks the route (D-193, D-442).
4. A large enemy never leaves its area, and a load proves the fit (D-209).
5. A fled group starts no battle inside its grace time (D-381).
6. A replay of a run with the enemies gives the same state hash on every leg (G-5).
7. The screen test holds a baseline with the lead and each enemy that the party sees (F-94, D-733). Each live enemy draws (D-814).

**Review focus.**

- The sight rule of D-718 and D-719 covers the party and a patrol with one implementation (T-1). PR-55 removed the party half (D-814).
- The rule of D-737 gives a large enemy one sort value at its front row. Each map sprite sits at the south edge of that row (F-94).
- The grace time and the beat count ticks in Core, and no clock reaches either one (G-3).
- The order of the tick is fixed: the beat, the party, the encounter of a step, and then the enemies (D-168, T-7).
- No map system ticks while an encounter runs, and the wanted direction of the party still ends each tick (D-531).

**Questions.** None. D-737 resolved OQ-115, and D-718 to D-720 resolved OQ-114. D-738 to D-754 answer the questions that this PR raised.

> *In plain English:* enemies stand and walk in the dungeon where you can see them. You choose the fight, you sneak past, or they catch you.

### 7.7 PR-9: the battle core and the timeline

Area files: `area-battle.md` sections 7.1, 7.2, 7.3, 7.5, and 7.9.

**Scope.**

- The encounter state and the timeline, where each action pushes its user back by a delay (D-29, D-376).
- One to three characters against up to six enemies, each side in a front row and a back row (D-31, D-336, D-377).
- The basic attack of every character, and damage in fixed-point (D-359, D-169).
- Haste, slow, and heavy actions as timeline shifts, and the stun push (D-376).
- The reach rule: melee takes the front row while anyone stands in it (D-377).
- The row step, the item use, and the flee, each with its delay and its rule (D-378, D-380, D-382).
- The down, the party wipe when every character who fights goes down, and the reload of the newer save (D-36, D-231, D-397).
- The hand-off of the encounter, where no map system ticks during a battle (D-531).
- Fixture enemies with fixture stats, until PR-80 builds the enemy record (D-557).
- The pace of one turn: Core resolves at once and emits events, and Game drains the queue (D-532).
- The defend action, the hit roll, and the miss chance of the speed gap (D-755, D-772, D-773).
- The wave: six enemies on the field at most, and up to twelve in a group (D-758 to D-762, D-778).
- The half damage of a melee attack from the back row (D-779).
- The fixture files: the characters, the groups, the draught, and the numbers of D-777 (D-765, D-766, D-775).
- The test that each group of a map exists (D-766).
- The target and the item of an intent, with record format 2 (D-764, D-780). The party in the snapshot, with save format 4 (D-765).
- The console commands of each action, which replace the flee command of PR-8 (D-767).
- The reload of the newer save after a wipe, or a new start with no save (D-776).

**Out of scope.**

- The enemy record with its stats and its abilities (PR-80, D-557).
- The elements and the statuses (PR-66, D-533).
- The battle screen (PR-10) and the evaluator (PR-11).
- The boss phases (PR-20) and the balance pass (PR-30).

**Exit tests.**

1. A property test over one thousand seeds proves that the timeline never stalls.
2. Melee never reaches a back row while its front row stands (D-377).
3. No flee starts in a boss fight (D-378).
4. A failed flee costs the turn (D-378).
5. A wipe with a healthy reserve still reloads, for a party of one, two, or three (D-336, D-397).
6. A test proves that the event queue of Game always drains (D-532).
7. The replay of a fixture fight gives the same state hash on every leg.
8. A test proves that no map system moves during a battle (D-531).
9. A waiting enemy steps into its row when an enemy falls, one attack push out (D-759 to D-761, D-778).
10. A group of thirteen enemies fails the load, and a map that names an absent group fails the load (D-762, D-766).
11. A save of format 3 still loads, and a record of format 1 fails with its line (D-166, D-764, D-765).

**Review focus.**

- No wait intent enters a fight, and the input gate lives in Game (D-532).
- Each delay counts ticks with integer math alone (D-164, G-2).
- Content holds the delay of each action in one place (D-757).
- The basic attack needs no lesson and no MP (D-359, F-8).

**Questions.** None. D-755 to D-777 answer OQ-124, OQ-125, OQ-126, OQ-132, and OQ-133, and the gaps that the start of PR-9 found.

> *In plain English:* this is the fight itself, with the order of turns shaped by speed. Nothing draws it yet.

### 7.8 PR-89: the walk fault, and the frames inside a step

Area files: `area-exploration.md` section 7.3, `area-ci.md` section 7.12.

**Scope.**

- The fault of F-95: the ground layer takes no part in the sort, and it draws below every sprite (D-783).
- The `walk` fixture of the capture session: one frame at 1x after each tick of one step north and one step south (D-782).
- The `--fixture <name>` argument of Game, `make sheet FIXTURE=<name>`, and `make walk` (D-782).
- The Godot build inside `make sheet`, so no capture shows an old build.
- The baseline of each walk frame, from the artifact of the screen-test job (D-733).
- The PR gate line of the visual review, and its check in the `pr-review` skill (D-784).

**Out of scope.**

- A wall or a thing that stands in front of a sprite, which takes a layer of its own (D-783).
- A walk animation of the lead. PR-17 draws the art of the first playable.
- A step east or west in the walk. The feet then stay on the edge of a row, and F-95 never showed there.

**Exit tests.**

1. The walk frames of the old code show the legs of the lead cut by the floor (F-95).
2. The screen-test job compares each walk frame with its baseline, and the lead draws whole in each one.
3. Two capture sessions give the same walk frames (T-7).
4. The smoke session reads back the sort and the Z index of the ground, and fails on another value (T-2).
5. A test locks the walk list: each tick of each step, in order, with the intent on tick 1 alone.
6. An unknown fixture name fails the capture session with the list of fixtures (T-2).

**Review focus.**

- The walk gives the run the time of one tick, and never the frame time of the engine (T-7, G-3).
- No Core file changes, so the simulation version stays (G-17).
- The author read each walk frame before the hand-over, and the PR records it (D-784).

**Questions.** None. D-782 to D-784 answer the start questions of the PR.

> *In plain English:* the character lost its legs for part of each step up or down the screen. The floor drew on top of it. The floor now always draws underneath. The screen test now takes a picture after every beat of a step, so a fault like this fails before it merges.

### 7.9 PR-80: the enemy record

Area file: `area-battle.md` section 7.7.

**Scope.**

- The enemy record in content: the stats of each enemy and the ids of its abilities (D-557). Each enemy has one file under `content/rules/enemies/` (D-786).
- The ability file `content/rules/abilities.json`, which holds each ability id alone (D-785).
- The strict reader of the record and of the ability file in Core, with a load test (D-177, G-6).
- The switch of PR-9 from fixture stats to the record, with the same ids (D-557, D-786).
- The ability ids stay data: each enemy keeps the basic attack (D-787).
- The size of each enemy on its record, and the load check of each map patrol against the largest enemy of its group (D-754, D-788, D-789).

**Out of scope.**

- The element table of each enemy, which PR-66 adds to the record (D-533).
- The profile, the steal list, and the group file (PR-11, D-65, D-535).
- The enemies of the first playable, which PR-17 writes.
- An action that reads an ability id. PR-11 gives an enemy ability its effect, and PR-12 gives a lesson its effect (D-787, D-955).

**Exit tests.**

1. A fixture enemy record loads, and PR-9 fights it in place of its fixture stats (D-557).
2. A record with an absent field fails the load with the file and the field (T-2).
3. A record that names an absent ability id fails the load with the file and the id (D-785).
4. A number with a fraction in a record fails the load (G-2).
5. The replay of a fixture fight against the record gives the same state hash on every leg.
6. A map patrol with another size than the largest enemy of its group fails the load, with both sizes (D-754, D-788).

**Review focus.**

- The record holds the stats and the ability ids alone, so the profile of PR-11 keeps its own file (D-557, G-8).
- The simulation version bumps, and the identity file gains a run (G-17, D-504).
- Each new content id is permanent (D-166).

**Questions.** None. D-758 answers OQ-132 with the wave. D-785 to D-787 answer the start questions, and D-788 and D-789 answer the size questions of the review.

> *In plain English:* each enemy gets its numbers and its list of moves in a data file of its own. The fight reads that file in place of the placeholder numbers. The moves do nothing yet, and a later change makes the enemies use them.

### 7.10 PR-66: the elements and the statuses

Area file: `area-battle.md` section 7.4.

**Scope.**

- The eight elements, and the element table of each enemy record, with one affinity for each element: normal, weak, resist, or absorb (D-74, D-794 to D-797).
- The ten statuses in a fight, each with the tick of its end (D-75, D-798).
- The refresh of a second copy, and the cancel of haste and slow (D-800).
- The shares of poison, bleed, and regen, and the sleep that a strike wakes (D-799, D-802, D-803).
- The stun push, blind, silence, and shell (D-804, D-806, D-810).
- The immune list of each enemy record (D-805).
- A move with an element and a status chance (D-793, D-807).
- A call of Core that gives a status, for the tests and the identity run (D-793).
- Poison, blind, and silence on each character after the fight, in the party state and in save format 5 (D-390, D-792).
- The numbers in the battle rules file (D-808), and the order of the rates on one hit (D-809).

**Out of scope.**

- The element table of gear (PR-13, D-790).
- The bonus of an aptitude and of a side aptitude (PR-12, D-791).
- What poison, blind, and silence do on the map (PR-64, D-393, D-792).
- The icons of the elements and the statuses (PR-10, D-811). D-870 removed the shape variants.
- A lesson that gives an element or a status in play, and the refusal of a rite of a silenced holder (PR-12, D-793, D-806).
- The cure rites, which PR-12 gives to Mend (D-394).

**Exit tests.**

1. A property test over one thousand seeds proves each element against weakness, resist, and absorb.
2. The same test proves the start and the end of each of the ten statuses.
3. A status that ends with its fight is absent after the fight (D-390).
4. Poison, blind, and silence remain after the fight (D-390).
5. Every damage number comes from integer math, and det-lint proves it.

**Review focus.**

- Holy and dark carry no claim of a god in any string (D-158, G-20). PR-66 adds no player string.
- Each roll draws on the battle stream in the order of D-807, and an immune target draws none.
- The simulation version bumps, and the identity file gains a run (G-17, D-504).
- The save format goes to 5, with a reader of format 4 and a stored save of format 5 (D-166, D-654).

**Questions.** None. D-790 to D-811 answer the start questions of the session.

> *In plain English:* fire, ice, and six more elements meet armor that likes or hates each one. Poison, blindness, and silence follow you out of the fight.

### 7.11 PR-55: the large pictures

Area files: `area-art.md` section 7.5, `area-tools.md` section 7.5.

**Scope.**

- The large picture format, which places drawn pieces at pixel positions, with repeats (D-516, F-44). A picture offers no mirror and no other operation on a piece (D-812).
- The load test of the format, and a failure that names the file and the entry (T-2).
- The render of a large picture as a PNG in Tools, and the draw in Game (D-518).
- Full-screen art that covers the frame of 1280 by 720 (D-568).
- Four changes that the owner added after the merge of PR #49:
  - Every key of an open debug console reaches its entry, which draws in the frame viewport (D-725).
  - Escape closes an open console, and it ends a session of a development build (D-813).
  - Game draws every live enemy at any distance from the party (D-814).
  - The game shows no button prompt (D-815).
- Three fixes that the owner added during the work:
  - Each capture holds the sRGB colors of the screen, and not the linear colors of HDR 2D (D-188).
  - Game draws each slide at the part of a tick that the frame reached, and the held step reaches each tick (D-820).
  - A step lasts 16, 32, or 64 ticks, so each tick moves a sprite by the same count of pixels (D-821).

**Out of scope.**

- The backdrop content, which PR-10 and PR-17 write (D-205).
- The store images (PR-76, D-475).
- The normal map of each piece (PR-48, D-516).

**Exit tests.**

1. A test decodes the render of a fixture large picture and compares its pixels with its pieces (F-19).
2. A large picture that names an absent piece fails with the file and the entry.
3. The fixture backdrop covers 640 by 360 art pixels with no gap, the frame at 2x (D-568, D-816).
4. det-lint finds no float type in the render code (D-502).
5. The smoke session types a console line through key events of the root viewport, then closes it with Escape.
6. A test gives the route of each key: the console, the close, the quit, and the game (D-813).
7. The smoke session fails when a live enemy draws no sprite (D-814).
8. The content load refuses the device table of the prompts (D-815).
9. A test finds a palette color in each pixel of each 1x baseline of the world.
10. A frame of two ticks gives the held step to both ticks (D-820).
11. The load refuses an enemy step of any count other than 16, 32, or 64 (D-821).

**Review focus.**

- A picture offers a place and a repeat alone (D-812).
- The author reads each frame of `make sheet` and `make walk` for the enemies and the prompts (D-784).
- Each piece stays small enough to draw by hand and to check (F-44).
- The render compares pixels, never bytes (F-19).

**Questions.** D-812 resolved OQ-91.

> *In plain English:* a battle background is too big to write as one text picture. The game builds it like a stage set from small drawn parts.

### 7.12 PR-10: the battle scene

Area files: `area-battle.md` section 7.10, `area-ui-input.md` sections 7.1 and 7.5, `area-effects.md` section 7.8.

**Scope.**

- The side view: the enemies on the left, the party on the right, each side in two rows (D-111, D-377). A row stands in two lanes, so six fit on the ground (D-759).
- The timeline strip across the top, and the command menu and the status at the bottom (D-111).
- The command of the player from the keyboard and the gamepad: an action, then an item or a target (D-827). The mouse waits for OQ-110 and PR-62.
- The attack pose on an action, and the lunge of an enemy (D-96, D-108, D-832).
- The color flash on a hit, and the damage number over its target (D-96, D-213).
- A pointer over the target under the cursor (D-833).
- One message line for each action, in the game voice, from the string table (G-7, G-20).
- A short bar of health under each enemy, with no number (D-826).
- The fixture backdrop of PR-55 behind every fight, with its sway (D-205, D-831).
- The hit flash as a `.gdshader` file of the Game project, which never uses the normal map member (D-183, D-825, the external facts of `area-effects.md`).
- The pace of the screen: each event plays for a count of ticks, and each timing is a constant of Game (D-829).
- A win shows no line of its own, and the summary of PR-67 follows the fight (D-835). The step reads "Back up" or "Step forward" from the row of the actor (D-836).
- The fixture art: Marrek with an idle frame and an attack pose, the grunt, and the brute (D-828).
- The 18 icons of the elements and the statuses, and a check of the palette color of each (D-214, D-811). The icon of each status shows beside the health of its holder (D-830).

**Out of scope.**

- The blood, the sparks, the shake, and the hit-stop (PR-57, D-186). PR-57 also moves each timing into the battle file (D-829, D-883).
- The light on the battle scene (PR-56) and the battle music (PR-70, PR-72).
- The boss phases (PR-20).
- The hurt flinch and the down pose of a character (PR-17, D-200, D-828, D-884).
- The backdrop of a place, and the link from a map to it (PR-17, D-831).
- The mouse on the menu, and the remembered cursor (PR-62, PR-63, D-226).

**Exit tests.**

1. A screen test renders a fixture battle: the menu at both body sizes, the pointer, and a blow (D-172).
2. The owner reads a fight from the screen alone.
3. Each message comes from the string table, and det-lint proves it.
4. A test proves that each panel holds its longest string (D-241).
5. The health of an enemy reads from the screen as a short bar (D-826).
6. The view of the screen matches the state of the rules after every tick, over a loop of seeds (D-532).
7. Each intent of the menu passes the rules, and a move of the cursor makes no intent (D-493, D-827).

**Review focus.**

- The strip shows six turns (D-756), and a row draws up to six enemies (D-759).
- The author reads each icon in `make sheet`, and confirms or changes its palette color (D-784, D-811).
- Shader code lives in a `.gdshader` file (D-825).
- The backdrop drift never moves a rule, and no rule waits for it (D-522, G-23).

**Questions.** None. D-825 answered OQ-103, and D-826 answered OQ-131.

> *In plain English:* the fight appears on screen: who acts next, who is low, and what you can do. You pick each action with the keys or a pad, and every line reads in the voice of the game.

### 7.13 PR-48: the normal maps

Area files: `area-tools.md` section 7.7, `area-effects.md` section 7.5, `area-art.md` section 7.11.

**Scope.**

- The normal-map command, which builds a normal map for each sprite, tile, and piece from its drawing file (D-183, D-516). The height of a pixel comes from the rim of D-840 and the height of its color (D-838).
- The optional override grid, which the artist writes by hand (D-184, D-839).
- Integer math with an integer square root, so every leg gives the same pixels (D-502, F-38).
- The normal-map atlas, which uses the atlas index of the color atlas (D-184, D-517).
- The review sheet, which draws each sprite under eight fixed light directions (D-514, D-521, D-841).

**Out of scope.**

- Light in the engine (PR-56). The review sheet computes its light in Tools (D-521).
- A normal map for a portrait, an icon, or a window frame, which take no scene light (D-210).

**Exit tests.**

1. The pixel test of the color atlas also covers the normal-map atlas, on every leg (F-19).
2. Each frame sits at the same place in both atlases (D-184).
3. An override grid replaces the built normal map of its frame.
4. det-lint finds no float type in the command (D-502).
5. The review sheet reaches the PR description, and the owner approves the batch (G-25).

**Review focus.**

- The integer square root gives the same result on x86_64 and on Apple silicon (F-38).
- The sheet names each drawing on it, so the owner can name a fix (D-514).
- From this PR on, each art PR commits the normal-map atlas with its test.

**Questions.** None. D-668 set the form of a review sheet in PR-34. The owner answered the details of the command on 2026-09-21 (D-838 to D-841).

> *In plain English:* a normal map tells the light which way each pixel faces. A tool builds it from the drawing, and the owner checks a sheet of each sprite lit from eight sides.

### 7.14 PR-56: the light and the shadows

Area file: `area-effects.md` sections 7.4 and 7.6.

**Scope.**

- The light setup of one map and one time of day: the ambient light, the changes, the added lights, and the battle light (D-442, D-843).
- The decor file of each map and the kind file of each decor kind, with the wall torch as the first kind (D-843, D-844).
- Each color of light as a palette key with a strength in basis points (D-846).
- The ambient light as the one canvas modulate of the world.
- The point lights of torches, waystones, and spells, each with a light texture that Game builds and checks (D-183, F-46).
- The carried light on the lead, its switch in Game, and the `torch` command of the console (D-847, D-851).
- A height on each light, because a light at height zero gives no light to a flat normal (F-46).
- Hard shadows from walls, with a shape from the terrain that lets each wall face take light (D-183, D-845, D-852).
- A shadow at the feet of each figure, with a pair of Godot lights for each source (D-853).
- The light row of 24 lights with shadows, after a new Deck sweep, and 50% more light on each source (D-854, D-855).
- The normal map of each sprite, tile, and piece through a canvas texture, with the Nearest filter on both atlases (F-45).
- The effect budget file, its strict reader in Core, and the budget test with its light rows (D-517, D-523, D-842).
- The light of a battle: the ambient light and one key light from the setup of the map where the fight began (D-205, D-850).

**Out of scope.**

- Particles and the battle effects (PR-57), and glow (PR-59).
- The light of a puzzle, which PR-21 drives from Core state (D-41).
- The light setups of the first playable (PR-17).
- The torch item and its rules (PR-91, D-848).
- The tilt-shift blur, the vignette, and the light shafts (PR-92, D-849).

**Exit tests.**

1. A screen test captures a lit fixture map and a lit fixture battle (D-172).
2. A light with no texture fails at load with its id (F-46).
3. The budget test fails a view with more than 15 lights on one canvas item (F-46).
4. A map that passes a row of the budget file fails the budget test, with the file and the count.
5. A light setup that names an absent map id fails with the file and the id.
6. No shader on lit art uses the normal map member of Godot.
7. A change of an absent piece fails with the file and the id (D-843).
8. A test proves that the change of a light setup wins over the default of the decor kind (D-843).
9. A wall torch off a wall, or with no floor south, fails with the file and the id (D-844).
10. The budget test counts the carried light in each view (D-842, D-847).
11. The `torch` command turns the carried light on and off, and it sends no intent (D-851).
12. A wall of one tile shadows the far face of itself and every tile behind it (D-852).
13. The budget test counts two lights for each source (D-853).

**Review focus.**

- The budget rows match the numbers that the Deck test measured (D-523).
- The budget test counts the worst view of the Deck (D-842).
- Each torch lights itself, and a change names a piece and never a tile (D-843, D-844).
- The carried light follows the drawn place of the lead, and no rule reads it (D-847, G-1).
- The Deck sweep of 24 paired lights holds 60 frames per second before the merge (D-854, G-14). The sweep of 2026-09-21 held (F-96).
- The light setup stays outside the content hash, so new light never breaks a record (D-495, D-519).

**Questions.** OQ-94 to OQ-97, resolved by D-842, D-843, D-845, and D-846.

> *In plain English:* each place gets its light from a small file: how dark it is, and how each torch glows. Each torch lights itself, so a moved torch keeps its light. Walls throw hard shadows, and the party carries a light of its own.

### 7.15 PR-93: CI on a docs-only change

Area file: `area-ci.md` section 7.1.

**Scope.**

- The skip set: the set of D-600, with `CLAUDE.md` and `AGENTS.md` added (D-857). `.claude/settings.json` stays in it through `.claude/`.
- Rules AGENTS 1 and AGENTS 2 of ste-check. They read the rule of D-20 and the count of the Smoke filter option on every PR (D-857).
- On a PR that changes docs alone, each job except ste-check, review-gate, and Gitar skips (D-595, D-856).
- A push that changes docs alone skips each other job when each check that it skips passed on the previous head (D-856, D-858).
- The `changed-paths` command of Tools holds the rule, and the workflow collects the facts from git and the GitHub API (D-859).

**Out of scope.**

- A change of the jobs of a code PR.

**Exit tests.**

1. A docs-only PR runs ste-check and review-gate, and each build job skips. The required checks pass.
2. A docs-only push after a green head skips the same jobs. A red review-gate on that head does not count (D-858).
3. A docs-only push after a red head runs every job. So does a head with a check that has no run or did not complete.
4. A push that changes one code path runs every job.
5. A change to `CLAUDE.md` alone fails ste-check with rule AGENTS 1.

**Review focus.**

- A skipped job reports `Success`, so a skip never follows a head that failed (T-2).
- The previous head comes from the push event, and a force push reads the whole PR again.
- The list of skipped checks in the command matches the jobs of the workflow, and a test holds it.

**Questions.** OQ-219, resolved by D-857.

> *In plain English:* a change of the documents alone never waits for the builds and the game tests again. The text check, the review check, and the automated review still run.

### 7.16 PR-63: the settings and the accessibility settings

Area file: `area-ui-input.md` sections 7.11 and 7.12.

**Scope.**

- The settings screen with five groups: display, audio, controls, battle, and accessibility (D-214, D-226).
- Display: the window mode, the scale of D-232, and the body size of D-707 (D-232, D-618, D-865).
- Audio: the master, music, effects, and ambience volumes, the mute in the background, and the mono toggle (D-435).
- Controls: the remap, the stick dead zone, and the vibration setting (D-214, D-434).
- Battle: the message speed and the remembered cursor (D-226).
- The three accessibility settings: the flash and shake reduction, the text speed and skip, and the remap (D-214, D-870).
- The menu action opens the settings screen until PR-62, and the world pauses (D-871).
- The settings file outside the save files, which never enters a run record (D-494, D-860, T-7). It is `settings.json` at the root of the folder of D-465.
- The format version of the settings file, and one migration step with a fixture file for each new setting (D-570).
- The mouse moves the cursor of the screen, and a click chooses (D-872).
- The battle screen reads the message speed and the remembered cursor, and a press of confirm shows the next message (D-866, D-873).

**Out of scope.**

- The effects that the reduction turns down, which PR-57 to PR-60 add.
- The audio that the volumes control (PR-69, PR-70).
- The vibration, which PR-70 plays (D-434).
- The type-out of the dialogue box, which reads the text speed (PR-36, D-864).
- The title screen and its settings entry (PR-33).

**Exit tests.**

1. Each setting saves and loads through the settings file.
2. A remap lasts across a restart, because Godot does not save one (F-50).
3. A remap conflict blocks the save and the exit of the screen, and the screen shows each conflict (D-862).
4. The fixture file of format 1 loads (D-869). A raise of the format with no migration step and no fixture file fails a test (D-570).
5. A key that no version declares fails the load with the file and the key (T-2, D-570).
6. No setting reaches a run record, and a test proves it (T-7).
7. The stick dead zone is 0.5 for every action by default, and the slider goes from 0.2 to 0.8 (D-861, F-50).
8. A screen test captures the settings screen.
9. The menu action opens the settings screen, and the cancel action closes it when no conflict stays (D-862, D-871).
10. The mouse moves the cursor, and a click chooses the item under the pointer (D-872).

**Review focus.**

- The settings file follows D-860.
- The default `ui_*` actions keep their events and take no remap, because Godot cannot remove one and the menus must stay reachable (D-862, F-50).
- Vibration turns off for any player, and it has limits on macOS (F-50).
- The screen closes only when no binding conflict stays (D-862).

**Questions.** None. D-860 to D-863 answer OQ-106, OQ-109, OQ-108, and OQ-100. D-864 answers OQ-112, and D-872 answers OQ-110. D-865 to D-871 set the other values.

> *In plain English:* one screen holds every choice about the game: the picture, the sound, the buttons, and the pace of battle. A player who needs calm can turn the flashes and the shakes down.

### 7.17 PR-57: the effect files, the particles, and the battle effects

Area file: `area-effects.md` sections 7.7 and 7.8.

**Scope.**

- The effect file: the emitters, the palette colors, and the timings in ticks (D-182, D-266).
- The record and the strict reader of an effect file in Core, which no rule reads (D-517).
- The Godot particle nodes that Game builds from each effect file at load, with no resource file (D-182, G-6).
- The blood or the sparks of a hit, from the hit file that serves the target (D-186, D-879, D-882).
- The short screen shake and the brief hit-stop of a heavy blow: a hit on a weakness (D-186, D-876, D-877, D-880).
- The battle file, which holds the timings of PR-10, the shake, and the hit-stop (D-829, D-883).
- The reduced form of each shake, under the setting of PR-63. The hit flash of PR-10 stays at each level (D-214, D-863, D-881).
- The particle rows of the effect budget (D-523).

**Out of scope.**

- The ambient effects (PR-58), glow (PR-59), and the transitions (PR-60).
- The flash of a spell, which PR-12 builds with the spells (D-878).
- The vibration of a heavy blow, which PR-63 holds as a setting and PR-70 plays (D-434).

**Exit tests.**

1. A screen test captures each battle effect, in both reduced forms (D-214).
2. An effect file with a bad field fails the load with the file and the field.
3. The budget test counts each live emitter against the particle rows (D-523).
4. A test proves that no rule reads the length of an effect (D-522).
5. Each hit file names the content ids that it serves, and a test fails an absent id. A test also fails a combatant with no hit file (D-879, D-883).

**Review focus.**

- D-875 sets `GPUParticles2D`, and each emitter of a capture takes a fixed seed (D-172).
- The Mobile renderer of CI holds every particle feature of the Deck, so each capture shows the effect that the Deck draws (D-731).
- A particle color is a palette key, so the screen keeps one palette (D-181).

**Questions.** None. D-875 answers OQ-98, D-876 answers OQ-99, and D-863 answers OQ-100. D-877 to D-883 set the heavy blow, the hit files, the hit-stop, and the battle file.

> *In plain English:* a burst of sparks is a small data file: how many bits, which colors, and how long. A hit in battle shows blood, sparks, and a jolt, and it passes fast.

### 7.18 PR-58: the ambient effects

Area file: `area-effects.md` section 7.9.

**Scope.**

- The four ambient kinds of region one (D-187). They are snow and wind, fog and mist, fire with embers and smoke, and dust with drips and motes.
- The weather of each map, which its time of day never changes (D-202, D-442).
- The ambient effects over a battle backdrop (D-205).
- The point light that a fire can carry. A fire is a decor kind with its light and its emitters (D-183, D-888).
- The flame, the embers, and the changing light of each wall torch and of the carried light (D-890, D-891). Glow on a torch waits for PR-59.
- The full-screen rows of the effect budget for fog and the other full-screen kinds (D-523).

**Out of scope.**

- The ambience sound, which PR-70 plays (D-424).
- The ambient effects of the first playable, which PR-17 writes (D-520).
- Glow, which PR-59 adds (OQ-102).

**Exit tests.**

1. A screen test captures each of the four kinds, from the test ambient files of the fixture dungeon (D-889).
2. Fog draws above the figures (D-885). The load fails a fog that makes an enemy too faint (D-886).
3. The budget test counts each full-screen pass (D-523).
4. An ambient effect file that names an absent map id fails with the file and the id.
5. Each tick shows one picture of each torch light, and two torches change out of step (D-891).

**Review focus.**

- The contrast test of D-886 keeps an enemy visible through fog (D-187, D-885).
- The weather of a map matches its ambience sound, which PR-70 adds (D-424).
- The ambient effects draw over the backdrop, not under it (D-205).

**Questions.** None. D-885 resolved OQ-101.

> *In plain English:* each place has its own weather: snow in the pass, smoke by a fire, dust in the mine. The weather never hides an enemy that the player needs to see.

### 7.19 PR-94: the procedural fog

Area file: `area-effects.md` section 7.9.

**Scope.**

- A noise shader of 1 to 3 layers in place of the text grid of D-887 (D-897). The owner refused the grid, because it repeats over the view (F-101).
- One full-screen pass for all the layers of a fog, and the budget count of one pass for each fog (D-898).
- A fade from clear to full in 2 to 8 steps, over blocks of art pixels, in the pale key `L` (D-900, D-903, D-907). The owner refused the hard bands of round 1 (F-102).
- An even density over the view, about 45 percent at the thickest, and a floor of 17 for the fog test (D-902, D-905, D-906).
- Clear ground between the banks of the fixture fog (D-908).
- The strongest layer wins where layers overlap (D-899).
- A new fog capture file of three layers for the fixture dungeon (D-889).

**Out of scope.**

- The glow (PR-59).
- The fog of the first places, which PR-17 writes (D-520).

**Exit tests.**

1. The reader takes the noise, the fade, the steps, the blocks, the strength, and the drift of each layer. It refuses each value outside its limits (T-2).
2. A fog of three layers keeps inside a budget of one pass (D-898).
3. The contrast test reads the full strength of each layer against a floor of 17 (D-885, D-892, D-899, D-906).
4. No shader reads `TIME`, and each uniform of the fog pass has its name in the shader (F-100, D-825).
5. The screen test captures the fog on the map and in a fight (D-172, D-889).

**Review focus.**

- The fog keeps one palette key for each layer, and its steps and blocks keep the pixel look of the art (G-27, D-907).
- The shader reads the drift that Game gives it, so one tick gives one picture (F-100, T-7).

**Questions.** None. D-896 to D-906 and D-908 resolved OQ-220 to OQ-231.

> *In plain English:* the fog of PR-58 was one small picture, repeated over the screen, and the repeat showed. The new fog grows from a soft noise over the whole world. It never repeats, and its edges fade like mist, in the steps and blocks of the pixel art.

### 7.20 PR-59: the glow

Area file: `area-effects.md` section 7.10.

**Scope.**

- A soft glow on light sources alone, first on the fire of each wall torch (D-188, D-912).
- HDR 2D in the world view, with the glow of Godot and a threshold above the brightest lit art (D-910, D-915).
- A glow rectangle over each flame that pulses on a wave of the tick, in place of a flicker (D-913, D-915).
- A smooth bloom, the second exception to G-27 after the fog (D-911).
- The fog, the hit bursts, and the marks in an overlay view above the glow, so the fog never glows (D-916).
- The rule that sprites, tiles, and the UI never glow (D-188, D-210, F-47).
- One glow pass in the row of full-screen passes, on every map and every fight (D-523).

**Out of scope.**

- The glow of spells, waystones, and the thing below, which the PRs of their content add (D-912).
- The glow of the carried torch, which waits for the torch in the hand of PR-91 (D-912).
- A fog that takes the scene light, which the reader refuses until a PR needs one (D-916).
- The transitions (PR-60).

**Exit tests.**

1. A screen test captures a lit fixture scene with glow.
2. A bright light on a pale sprite never makes that sprite glow (F-47).
3. The budget test counts the glow pass (D-523).
4. The captures show the glow of the Deck, because CI runs the Mobile renderer too (D-731).
5. A seed loop proves that the bound of the lit art holds the light of Godot on each pixel (D-910).
6. The pulse of each glow stays inside its depth, moves with no jump, and differs from torch to torch (D-913).
7. A screen test captures the fog of the fixture as PR-94 drew it, above the glow (D-916).

**Review focus.**

- The bound of the lit art never falls below the light of Godot, so a sprite or a tile never glows (F-47).
- The threshold of the glow file is 70000 basis points of linear light, and the bound of the fixture dungeon is below it (F-47).
- The view of the world turns linear light into sRGB, and each glow rectangle reaches Godot as sRGB (F-103).
- No node above the glow draws in the world view, and each parent takes the layer too (F-105, D-916).

**Questions.** None. D-910 to D-916 resolved OQ-102 and OQ-232.

> *In plain English:* flames give off a soft haze of light that swells and fades, and the people and walls that they light stay crisp. The fog drifts over the haze, as it did before, and never glows.

### 7.21 PR-92: the HD-2D passes

Area file: `area-effects.md` section 7.17.

**Scope.**

- A tilt-shift blur at the top and the bottom of the frame (D-849).
- A vignette at the edges of the frame (D-849).
- Light shafts in the world: a shaft kind on a wall, under a drawing of its opening (D-849, D-918, D-924).
- A smooth mode and a stepped mode of the three passes, which the owner compares (D-917).
- A still shaft from a barred window on the fixture dungeon, in both modes (D-924, D-925).
- The blur and the vignette on every map and every fight (D-920).
- The shafts in the overlay above the fog, and the blur and the vignette over that picture. The marks draw in a mark view above them (D-919).
- The pass row of the effect budget from 3 to 6, from a new Deck sweep on the branch `spike/deck-test` (D-523, D-922, D-923, G-14).

**Out of scope.**

- The glow (PR-59), and the transitions (PR-60).
- A 3D scene, which D-849 keeps out of the plan.
- Light shafts from the ceiling onto a floor tile, which a later PR adds (D-924, OQ-241).

**Exit tests.**

1. A screen test captures a lit fixture map with the three passes (D-172).
2. The budget test counts each pass (D-523).
3. The UI stays sharp, with no blur, no vignette, and no shaft (D-210).
4. The PR holds the Deck sweep of the heavier stack, before and after the passes (G-14).
5. The screen test captures the map and a fight in the stepped mode too (D-917).
6. The budget counts a map with no weather, and a shaft pass only on a map with a shaft (D-918, D-920).
7. The load refuses a shaft off a wall, a shaft of no kind, and a ninth shaft (D-918, T-2).

**Review focus.**

- The budget rows match the new sweep (D-617, G-14).
- Each pass draws the world alone, and the marks and the UI stay sharp (D-208, D-210, D-919).
- No shader reads `TIME`, and each shaft stands still (F-100, D-925).
- The load refuses a shaft kind that no drawing draws, so no beam comes out of a bare wall (D-924).
- Each node of the mark view, and each parent of it, takes the layer of the marks (F-105, D-919).

**Questions.** None. D-917 to D-925 resolved OQ-233 to OQ-240. OQ-241 names the PR of the ceiling shafts, and it blocks nothing here.

> *In plain English:* the edges of the view blur a little, the corners fall dark, and shafts of light cut through the dark. The Deck proves it can hold this before the change lands.

### 7.22 PR-95: the automated review and the gated auto-merge

Area file: `area-ci.md` section 7.20.

**Scope.**

- `make codex-review PR=<n>` and the `codex-review` command of Tools (D-926, D-927). The command installs the CLI, probes the model, checks the start, and runs the review in a worktree.
- A handoff entry for each review run (D-928).
- No API key in the environment of a Codex process, and a ChatGPT login before each run (D-932).
- The `Open at:` line of each finding, and the three-strike stop (D-929).
- The gated auto-merge that the author session turns on after the confirmation of the owner (D-930, D-933).
- The merge settings of the repository and their record: the auto-merge, the conversation resolution, and `screen-test` as a required context (D-931).
- The documents of the new loop: `CLAUDE.md`, `AGENTS.md`, the skills, the PR template, and `docs/runbooks/merge.md`.

**Out of scope.**

- A change of the depth of the review, or of the parts of the record that `review-gate` reads (D-612).
- A change of a workflow file.
- The first auto-merge, which comes on the next PR (D-931).

**Exit tests.**

1. Each refusal of the command gives the fault code and names its cause (T-2).
2. A finding open in rounds 1 and 2 passes, and a finding open in round 3 stops (D-929).
3. A finding that a fix closed and a later round opened again keeps its count (D-929).
4. A stale record, an absent record, and an error of the CLI each give the fault code (D-610).
5. The review arguments pass the model, the effort, and the sandbox on the command line (D-926).
6. The first live run reviews this PR, and the record gives its verdict for the effective head.
7. The live settings match `docs/runbooks/branch-protection.json` before the merge (D-931).
8. A Codex process gets no API key variable, and a CLI with no ChatGPT login refuses the run (D-932).

**Review focus.**

- No approval of an older head gives the approval code (D-610).
- The command never takes the effort from the configuration file of the CLI (D-926).
- The worktree leaves the author checkout as it was, and each external command fails with its context (T-2).

**Questions.** None. D-926 to D-933 hold the answers of 2026-09-23.

> *In plain English:* one command now starts the review of the other provider and reads its verdict. A finding that comes back three times stops the loop for the owner. A PR merges itself only when each check is green, the review approves it, and each automated comment has its answer.

### 7.23 PR-60: the transitions

Area file: `area-effects.md` section 7.11.

**Scope.**

- The library of ten transitions, each 60 ticks long (D-195, D-941).
- The transition table: one transition for each fixed kind, and a pool of each region for its common encounters (D-196, D-934, D-940).
- The pick from the pool, with a hash of the seed and the tick, and no repeat of the last pick (D-935).
- The regions of the table, which name their maps (D-936).
- The kind of an encounter, in the order boss, wrong thing, ambush, elite, common (D-937).
- The transition as an effect file with its shader in a `.gdshader` file of Game (D-182, D-191, D-825).
- The fade into the fight from the cover color, and the fade back to the map (D-938, D-939).
- The color split under the flash and shake reduction (D-195, D-214, D-863).
- The wait at the end of a battle, which a wait intent ends after the fade back (D-522, D-938).
- The full-screen row of the effect budget for a transition (D-523, D-923).

**Out of scope.**

- The transitions of later regions (D-194).
- Every effect of the frame, with the style of G-27 (PR-56 to PR-60).
- The mark of a wrong thing, which the PR of the wrong things adds (D-937).

**Exit tests.**

1. A screen test captures each of the ten transitions.
2. The color split has a reduced form, and the test captures both (D-214).
3. A transition table that names an absent encounter kind fails with the file and the kind.
4. A test proves that the map waits for the wait intent, and never for a timer (D-522, G-23).
5. The budget test counts each transition pass (D-523).

**Review focus.**

- Shader code lives in a `.gdshader` file (D-825).
- A replay never waits, because the wait intent sits in the record (D-493, D-522).
- Snow whiteout fits region one, and the pool of region one holds it (D-194, D-934).
- No pool holds the transition of a fixed kind, and each map of the rules belongs to one region (D-934, D-936).

**Questions.** None. D-825 resolved OQ-103. D-934 to D-941 hold the answers of 2026-09-23.

> *In plain English:* each fight starts with a screen effect, such as shattered glass or a whiteout of snow. A boss, an elite, an ambush, and a wrong thing each have their own effect. Common fights draw a new effect from the set of the region each time.

### 7.24 PR-96: the rules of the review and merge loop

Area file: `area-ci.md` section 7.21.

**Scope.**

- The summary in four sections before the merge question: What, How, CI, and Codex review, inside the question block of `AskUserQuestion` (D-942).
- RG 5 of the `review-gate` command accepts an approved head when each later commit changes paths of the skip set alone (D-857, D-943).
- The Gitar pass of each commit of documents alone, with an answer to each comment and each claim of the pass (D-944).
- The documents of the loop: `CLAUDE.md`, `AGENTS.md`, the skills, and `docs/runbooks/merge.md`.

**Out of scope.**

- A change of the metadata set, of the effective head, or of the head check of the `codex-review` command (D-610).
- A change of a workflow file, or of the label rules (D-401, D-700).

**Exit tests.**

1. A commit of documents alone after the approval keeps RG 5 green, also with a decision row and `.claude/settings.json` (D-943).
2. A commit outside the skip set after a commit of documents alone needs a new review (D-943).
3. A PR of metadata alone has no head that a record can name (D-578, D-610).

**Review focus.**

- No commit outside the skip set can follow the approved head that RG 5 accepts (D-943).
- The `codex-review` command still compares the record with the effective head alone (D-610).

**Questions.** None. D-942 to D-944 hold the answers of 2026-09-23.

> *In plain English:* the owner reads a short summary inside the merge question. A fix of the documents after the review needs no second review, and Gitar still reads it.

### 7.25 PR-97: the Gitar pause and the flag that skips the Gitar check

Area file: `area-ci.md` section 7.22.

**Scope.**

- The pause of the Gitar requirement. No step waits for a Gitar pass, and a Gitar review thread or finding stops the session until the owner saw it (D-945).
- One read of the Gitar output before each `make codex-review` run and before the merge question (D-945).
- The `review-override` label of a docs-only PR with no Gitar approval (D-945).
- The flag `--skip-gitar-review` of the `codex-review` command, and the form `make codex-review PR=<n> -- --skip-gitar-review` (D-946).
- One marker on each line of the pause, so that one command lists each line, and the steps that end the pause (D-945).

**Out of scope.**

- A change of the branch protection. Conversation resolution stays on, so an open Gitar thread still blocks the merge (D-931).
- A change of a workflow file or of the `review-gate` command. No required check reads a Gitar result.

**Exit tests.**

1. With the flag, the command reads no Gitar fact and gives no Gitar reason (D-946).
2. Without the flag, an absent Gitar pass still refuses the review (D-926).
3. With the flag, the prompt of the reviewer says that no complete Gitar pass is a condition (D-946).
4. The Makefile target passes the goal of the flag to the command (D-946).

**Review focus.**

- The flag skips the Gitar check alone. Each other refusal of the command stands (D-926).
- Each line of the pause holds the marker, and no text of D-946 holds it (D-945).

**Questions.** None. D-945 and D-946 hold the answers of 2026-09-23.

> *In plain English:* Gitar can still comment, but no PR waits for it. A session stops and tells the owner when Gitar finds a problem. The review command takes a flag that skips the Gitar check, and the flag stays after the pause.

### 7.26 PR-11: the evaluator, the profiles, and the groups

Area file: `area-battle.md` sections 7.6 and 7.7.

**Scope.**

- The evaluator that scores every legal action by its simulated outcome (D-65, D-377).
- The depth of D-534: each legal action, and the best reply of the next character on the timeline (D-960). Each score takes the expected outcome (D-959).
- The profile file with its term weights, one file for each profile, and its validator (D-65, D-956, D-958, G-21).
- The fields of an enemy move in the ability file, and the defend and the step of an enemy (D-955).
- The steal list of items and gold, and the base chance of a steal, on each profile (D-383, D-949).
- The group file of each region, with the rows and the profile of each enemy, and the region field of a map (D-535, D-957).
- The fixture profiles that prove the evaluator. PR-17 writes the profiles of the first playable.
- The Tools command that times an enemy turn, with a limit of 1 ms at the 95th percentile on the Steam Deck (D-961, F-53).

**Out of scope.**

- The boss phases (PR-20, D-533).
- The balance pass over the weights (PR-30).

**Exit tests.**

1. A fixture enemy with a protector profile heals its ally before it attacks.
2. A check fight runs for each entry of each group, and an empty list of actions fails the load (D-948, D-962).
3. A map that names a group absent from the region file fails with the map and the id (D-535, D-766).
4. A property test over one thousand seeds proves that the evaluator never stalls a turn.
5. On a tie of two scores, the evaluator draws from its own stream, and a seed loop locks it (D-947).
6. The PR reports the count of legal actions, and the turn time on the desktop and the Deck (D-961).

**Review focus.**

- The cost of an enemy turn holds the limit of D-961 on the Steam Deck (D-161, F-53).
- A miss of that target changes the depth or the profiles in this PR (G-14).
- The evaluator draws from one seeded stream, and its order of work never changes (G-4, T-7).

**Questions.** None. D-947 to D-950 answer OQ-127, OQ-128, and OQ-129, and D-955 to D-961 set the shape of the PR.

> *In plain English:* each enemy tries every move it can make, imagines your best answer, and picks the move that leaves it best off. That is what makes the fights hard.

### 7.27 PR-98: the waiting enemies at the edge of the field

Area files: `area-battle.md` sections 7.7 and 7.10.

**Scope.**

- Each waiting enemy of the fight, in one column at the left edge, behind the back row of the enemies (D-951, D-953).
- The order of the column: the top holds the next enemy that steps in (D-760, D-778, D-953).
- Each waiting enemy at full size, in a darker shade of the palette (D-954, G-27).
- The two rows of the enemies move a little to the right, to give the column its place (D-953, D-568).
- The step of a waiting enemy from the column into its row, when an enemy on the field falls (D-761, D-778).

**Out of scope.**

- A change to the rules of the wave. PR-9 holds the wave, and Core does not change (D-758 to D-762).
- A waiting enemy as a target. The menu of PR-10 never offers one (D-954).

**Exit tests.**

1. A screen test renders a fixture fight with waiting enemies, and the baseline holds the column (D-172).
2. The column shows the waiting enemies in the order of the group (D-760).
3. After an enemy falls, the top enemy of the column steps into its row, and the column moves up (D-778).
4. The target menu offers no waiting enemy (D-954).
5. A column taller than the field follows the answer of OQ-243, and a test proves it.

**Review focus.**

- The author reads each frame of `make sheet` and confirms that each waiting enemy reads as not yet in the fight (D-784).
- The column reads on the Steam Deck at 1x (D-92, G-19).
- Game reads the waiting enemies from the state of the rules alone, and no timer of Godot starts a step (D-100, D-532).

**Questions.** OQ-243.

> *In plain English:* today an enemy can step into a fight with no warning, and a plan that the player made goes wrong. This change shows each enemy that waits, dimmed at the left edge, so the player plans for the whole group.

### 7.28 PR-67: the character level, the experience, MP, and the stat curves

Area file: `area-progression.md` sections 7.1, 7.2, and 7.3.

**Scope.**

- The character level from experience, and half experience for the reserve and for a downed character (D-34, D-73, D-387).
- The shrink of the experience of an enemy as the party outlevels it (D-388, OQ-136).
- The start level of a character who joins late, from content (D-363).
- MP, and its recovery at a hub, at a save point once for the place, and from scarce items (D-42, D-389, D-555).
- The stat curve of each character in content: the health, the MP, the attack, the defense, and the speed at each level (D-537, F-54).
- The level-up sting event, which PR-70 plays (D-422).
- The summary after a fight: the experience and each level-up. PR-13 and PR-65 add their loot to it (D-835).

**Out of scope.**

- The lessons, the slots, and the aptitudes (PR-12).
- The balance of the eight curves (PR-30, D-299).

**Exit tests.**

1. A property test proves that the experience from one enemy falls as the level of the party rises (D-388).
2. A character in reserve and a downed character each earn half (D-73, D-387).
3. A save point restores MP once for the place, and no health (D-389, D-555).
4. A curve with a number that is not an integer fails the load (G-2, D-169).
5. A character who joins late starts at the level that content names (D-363).
6. The snapshot holds the level, the experience, and the MP of each character.

**Review focus.**

- The answer of OQ-134 sets the shape of a curve, and each rate stays in basis points (D-169).
- The answer of OQ-135 sets the MP that a save point and a rest restore.
- The curves differ from each other, so the cast reads as people before any gear (D-33, D-537).

**Questions.** OQ-134, OQ-135, and OQ-136.

> *In plain English:* a fight makes each character stronger, and the people who wait or fall behind still learn a little. Each person grows on their own line.

### 7.29 PR-62: the menu windows and the dungeon map screen

Area file: `area-ui-input.md` sections 7.6 and 7.7.

**Scope.**

- The main list, which opens one window for each task: party, lessons, gear, items, status, and save (D-211).
- The settings entry of the main list, which opens the settings screen of PR-63. The menu action then opens the main list (D-871).
- The party window, which sets the starting row of each character, and the snapshot that keeps the row (D-377, D-558).
- The status window, which reads the state of PR-9, PR-12, and PR-67 (D-569).
- The window stack, where back closes one window and the map stays visible behind (D-211).
- The pause of the world while a menu is open (D-162, D-650).
- The mouse on menus alone, which makes the same intent as a key or a button (D-219, D-493). The mouse moves the cursor, and a click chooses (D-872).
- The dungeon map screen, which draws each tile that the party walked (D-567, OQ-111).
- The notice that slides in at the top edge, and the notice log in the menu (D-221, OQ-113).

**Out of scope.**

- The content of the lesson, gear, item, service, and save windows, which PR-12, PR-13, PR-14, and PR-16 add (D-525).
- The settings screen (PR-63) and the dialogue box (PR-36).

**Exit tests.**

1. A fixture menu opens, stacks a second window, and closes each with back.
2. A screen test captures the stack and the dungeon map screen.
3. A menu action makes an intent, and the record holds no cursor move (D-493).
4. The mouse, the keyboard, and the gamepad each move the same cursor (D-219).
5. A test proves that the world does not run while a menu is open, and that the tick rises (D-162, D-650).
6. The dungeon map screen shows each walked tile, with the doors, the save points, and the exits on it (D-567).
7. The party window sets the row of a character, and a fight starts with that row (D-377, D-558).
8. The row survives a save and a load, through a snapshot format bump and its migration (D-166, D-558).
9. The status window shows the level, the MP, and the stats of each character (D-569).

**Review focus.**

- D-872 sets the cursor rules, and OQ-111 holds the scale of the map screen.
- The answer of OQ-113 sets which notices the log keeps, and how many.
- Each later system PR adds one window to this stack (D-525).

**Questions.** OQ-111 and OQ-113. D-872 resolved OQ-110. D-650 resolved OQ-64.

> *In plain English:* menus are windows that stack on each other, and the world stops while one is open. A second screen draws each tile of the dungeon that the party walked.

### 7.30 PR-68: the story scene format, the story scene runner, the flags, and the conditions

Area file: `area-story.md` sections 7.1, 7.2, 7.3, and 7.5.

**Scope.**

- The story scene format: a JSON list of steps, such as move, face, wait, say, choose, and set flag (D-173, OQ-144).
- The join step, which adds a cast member to the party (D-342, D-563).
- The story scene runner in Core, which holds the step index and every flag that a step sets (D-540).
- The wait intent that Game sends at the end of a move, a face, or a line (D-493, D-522).
- The story flag, a name that is on or off, and the set of the flags that are on (D-542).
- The declaration of every flag id in content, and a load that fails on an id that no file declares (OQ-147).
- One condition form for every reader: a story scene step, a route, a hub line, a quest, and an enemy group (D-543).
- The story scene triggers in the map file, each with its condition (D-528, OQ-148).
- The story scene state in the snapshot, the state hash, and the migration set (D-166, G-5).

**Out of scope.**

- The dialogue box and the portraits (PR-36, D-541).
- The branches, the choice effects, and the lost ally (PR-18, D-544).
- The quest state and the personal tasks (PR-19).
- The screenplay tool (PR-50, D-545).

**Exit tests.**

1. A property test over one thousand seeds replays a run with story scenes to the same end-state hash.
2. A scripted intent list answers each wait intent at once, and it plays a fixture scene to its end.
3. A step that names an absent string id fails with the story scene, the step, and the id.
4. A condition that names an undeclared flag id fails at load with the file and the id.
5. A story scene that plays once sets its flag, and its condition then refuses it (D-542).
6. A trigger fires from the tick, and two replays start the story scene at the same tick.
7. The snapshot carries the flag set through a migration.
8. A join step adds a fixture cast member to the party, and the snapshot keeps the party (D-563).

**Review focus.**

- D-540 revises D-114 in part, and the PR cites the revision.
- One parser, one test, and one error message cover every reader of a condition (T-1, D-543).
- The bots of PR-15 answer the same wait intent, so they play every story scene when PR-15 lands (D-64, D-540, G-16).
- Core reads no clock, so the length of a step comes from content or from the wait intent (G-3, OQ-145).
- A story scene names no art and no track (D-519, D-548).

**Questions.** OQ-144, OQ-145, OQ-146, OQ-147, OQ-148, and OQ-149.

> *In plain English:* a story scene is a list of simple steps in a data file: walk here, say this line, ask this question. The rules run it, so a robot can play it and a replay always matches.

### 7.31 PR-50: the screenplay tool

Area files: `area-tools.md` section 7.10, `area-story.md` section 7.8.

**Scope.**

- The `screenplay` command, which prints each story scene script as a screenplay with the text of each string id (D-173, G-25).
- The look-up of a cue in the audio file, because a story scene names no cue (D-548).
- The attachment of the output to the PR description, where the owner approves the batch (D-57, G-25).

**Out of scope.**

- No game code. The tool reads content alone.
- The audio files themselves (PR-38, PR-70, PR-72).

**Exit tests.**

1. The command prints a fixture scene with each line in the order of the steps.
2. A story scene that names an absent string id fails with the story scene, the step, and the id.
3. The output reaches the PR description of a fixture batch.
4. A cue in an audio file appears beside its line.

**Review focus.**

- The tool needs the story scene format and the string table alone, so it lands right after PR-68 (D-545).
- Every player string follows the `game-text-style` skill (D-63, G-20).

**Questions.** None.

> *In plain English:* a tool prints each story scene in the shape of a film script. The owner reads the story as a story before anybody builds it.

### 7.32 PR-12: the lessons, the slots, and the aptitudes

Area file: `area-progression.md` sections 7.4, 7.5, and 7.6.

**Scope.**

- The lesson, a rite or a drill that any character equips to gain an ability (D-272, D-275, D-278).
- The fields of a lesson on each entry of the ability file of PR-80, with the same ids (D-785).
- The lesson slots on the character, which grow with the character level (D-356, OQ-137).
- The swap of lessons at a hub and at a save point, which holds for the dungeon visit (D-356).
- The points that every equipped lesson gains from each battle won, and half for a reserve character (D-357).
- The named forms of each lesson, and the point total that opens each form (D-539).
- The growth that belongs to the character, not to the lesson (D-361).
- The eight kinds, the main aptitude of each character, and the bonus of a lesson of that kind, in basis points (D-274, D-281, D-358, D-791).
- The half bonus of a side aptitude, which reads the same table (D-360, D-791).
- The side aptitude behind a story flag of PR-68, with an empty mark in the menu before the unlock (D-282, D-283, D-538, D-556).
- The Mend rites and the cure rites that also work from the menu outside battle (D-391).
- The element and the status chance of each lesson move, which fill the move fields of PR-66 (D-793).
- The flash of a spell, with a point light of PR-56 for its length, its effect files, its reduced forms, and its captures (D-183, D-186, D-863, D-878).
- The refusal of a rite of a silenced holder, in a fight and from the menu (D-393, D-806).
- The lesson window in the stack of PR-62.

**Out of scope.**

- The quest state that unlocks a side aptitude in play (PR-19, D-538).
- The lessons of region one (PR-42, D-304) and the balance pass (PR-30).

**Exit tests.**

1. A character equips a lesson and uses its ability in a fixture battle.
2. A story flag of PR-68 unlocks a side aptitude, and the menu shows an empty mark before it (D-283, D-556).
3. An equipped lesson gains points from a fixture battle, used or not (D-357).
4. A lesson passed to a new character starts at its first form for that character (D-361).
5. A lesson passed back to a character resumes at the level of that character (D-361).
6. A cure rite works from the menu outside battle, and silence stops it (D-391, D-393).

**Review focus.**

- The answer of OQ-137 sets the slots at each level, and OQ-138 the points of a battle.
- The answer of OQ-139 settles two copies of one lesson in one party.
- No lesson ever copies itself, so the loot table stays finite (D-45, D-357).
- The balance must hold with any side aptitude absent (D-282, D-304).

**Questions.** OQ-137, OQ-138, and OQ-139.

> *In plain English:* abilities come from rites and drills that anybody can carry. Use one long enough and it opens a stronger form, and that progress belongs to the person who carried it.

### 7.33 PR-13: the gear, the items, and the inventory

Area file: `area-progression.md` sections 7.8 and 7.9.

**Scope.**

- The six equipment slots: the weapon, the shield or off-hand, the head, the body, and two accessories (D-44).
- Gear that any character wears, because the aptitudes carry the difference (D-374).
- The element table of each piece of gear, which PR-66 builds for the enemy record (D-790, D-794).
- Fixed, hand-authored gear with a few rarity tiers, and no random affix and no crafting (D-45, OQ-143).
- The pack, with a small fixed number of each item (D-382, OQ-142).
- The item use on a turn, which restores less in a fight than outside one (D-382).
- The steal action: the roll, the Theft term, the clamp, the failure that costs the turn, and the stolen entry in the pack (D-949, D-950).
- The items that a fight gives, on the summary after the fight of PR-67 (D-835).
- The find over the stack limit, which stays in its chest and which the save records (D-385).
- The gear window and the item window in the stack of PR-62.

**Out of scope.**

- The shop and the gold (PR-65, D-530).
- The chests that hold gear and items (PR-16).
- The items of region one (PR-42 and the content PRs).

**Exit tests.**

1. A character equips and removes gear in each of the six slots.
2. The screen shows each empty slot (D-44).
3. A pickup over the stack limit leaves a remainder, and the pack names it (D-385). PR-16 builds the chest that holds it.
4. An item restores less in a fight than outside one (D-382).
5. A test proves that two accessories with one effect follow the rule of OQ-141.
6. The snapshot holds the pack and the slots.

**Review focus.**

- The answer of OQ-140 sets what a piece of gear changes, and OQ-143 what a rarity tier changes.
- The gear of Elio leaves the game with him, and content marks it (D-364).
- A steal takes one entry from the list of an enemy, which PR-11 holds (D-383). The chance adds the Theft term to the base chance of the profile (D-949).

**Questions.** OQ-140, OQ-141, OQ-142, and OQ-143.

> *In plain English:* six slots, and anyone can wear anything. What you find is what the author placed, so a good weapon is a real event.

### 7.34 PR-91: the torch item

Area file: `area-exploration.md` section 7.17.

**Scope.**

- The torch as an item of Core in the pack of PR-13, which never burns out (D-848).
- The intent that lights the torch or puts it out (D-848).
- The sight of the party in the dark, with a lit torch and with none (D-848, OQ-217).
- The longer sight of an enemy toward a lit torch (D-848, OQ-218).
- The switch of the carried light, which follows the state of the torch (D-847).
- A simulation version bump, because the rules change (G-17).

**Out of scope.**

- The braziers of a puzzle (PR-21, D-41).
- The light itself, which PR-56 builds (D-847).

**Exit tests.**

1. A seed loop proves the sight of the party with a lit torch and with none.
2. A seed loop proves that an enemy sees a lit torch from farther away.
3. A replay with the intent of the torch gives the same state hash on every CI leg (G-5).
4. The carried light draws while the torch burns, and it goes dark when the player puts the torch out.

**Review focus.**

- The answer of OQ-217 meets D-566, which puts no fog of war on a map.
- The light of the screen never reaches a rule of sight (G-1).

**Questions.** OQ-217 and OQ-218.

> *In plain English:* the torch becomes a real item. Dark places need it, and guards see it from far away, so the player chooses between light and stealth.

### 7.35 PR-14: the hub map, the NPCs, and the services

Area file: `area-exploration.md` section 7.11.

**Scope.**

- The hub as a walkable map with NPC sprites, on the same code path as a dungeon (D-112).
- The hub content format, with the services that each hub offers (D-28, D-59).
- The rest, which restores health and MP and cures poison, blind, and silence (D-42, D-390).
- The save, the party swap, and the lesson swap at the hub (D-59, D-62, D-356).
- A condition of PR-68 on each service, so a story flag can close one (D-543, D-544, D-556).
- The service screens in the window stack of PR-62.
- The village as a start area with no shop and no rest (D-369).

**Out of scope.**

- The shop and the gold (PR-65, D-530).
- The hub lines that the dialogue box shows (PR-36).
- The hub content of the first playable (PR-17).

**Exit tests.**

1. A fixture group of four characters, three of them in the party, walks the hub and rests (D-362).
2. The group swaps the reserve and a lesson, then saves (D-356).
3. The save reloads to the same state hash.
4. The lead moves to the reserve, and the lead still walks the map with the camera on it (D-292, D-306).
5. A rest cures poison, blind, and silence (D-390).
6. A hub that offers no rest refuses the rest, and the screen says so.
7. A hub file that names an absent service fails with the file and the service.
8. A story flag closes a fixture service, and the hub refuses it (D-543).

**Review focus.**

- One code path draws a hub and a dungeon (D-112, T-1).
- The hanging cells are a dungeon under a hub, and the same map rules cover it (D-244).
- The party swap keeps the lead on the map in every case (D-292, D-306).

**Questions.** None. OQ-121 blocks the shop of PR-65.

> *In plain English:* the hub is a place you walk through, where the party recovers and reshapes itself before the next dungeon. Every hub has a different shape.

### 7.36 PR-65: the shop and the gold

Area file: `area-exploration.md` section 7.12.

**Scope.**

- The gold economy: gold from enemies and from treasure, which buys gear, items, and rest (D-60).
- The gold that a fight gives, on the summary after the fight of PR-67 (D-835).
- The shop screen in the window stack of PR-62.
- The shop stock in content, with its prices and its buy-back rule (D-60, OQ-121).
- A shop that sells a lesson too (D-365).
- A shop that a story flag closes or opens, such as the shops of the mining town (D-319, D-331).

**Out of scope.**

- The people of the story who teach or give a lesson (PR-42 and the content PRs).
- The balance of the prices (PR-30, G-14).

**Exit tests.**

1. A fixture party buys gear, an item, and a lesson, and the gold falls by the price.
2. A buy-back follows the rule of OQ-121.
3. A purchase over the stack limit fails, and the screen says why (D-385).
4. A story flag closes a shop, and the shop refuses the party (D-319).
5. The snapshot holds the gold and the stock that remains.

**Review focus.**

- The condition of a shop uses the one condition form of PR-68 (D-543).
- The stock and the prices sit in content, never in code (D-116, G-6).
- The balance pass of PR-30 tunes each number later (G-14).

**Questions.** OQ-121.

> *In plain English:* every fight pays a little, and the gold buys gear, supplies, and a bed. Some shops close for good when the story turns.

### 7.37 PR-36: the dialogue box, the portraits, and the story scene on screen

Area files: `area-story.md` section 7.4, `area-ui-input.md` section 7.8.

**Scope.**

- The dialogue box at the bottom, with the portrait, a name plate, and the choices (D-109, D-114, D-223).
- The type-out at the chosen speed, in silence (D-223, D-864).
- The draw of each story scene step: a sprite that moves and faces, from the tick of Core (D-540, F-52).
- The choice as an intent, whose result Core holds (D-493, D-540).
- Fixture portraits as 64 by 64 grids, because PR-28 and PR-29 draw the cast (D-234).
- The skip, which the accessibility settings of PR-63 hold (D-214, OQ-150).

**Out of scope.**

- The story scene format and the runner (PR-68, D-541).
- The portraits of the cast (PR-28, PR-29).
- The music cue of a story scene (PR-70, D-548).

**Exit tests.**

1. A fixture scene walks two sprites, shows a line with a portrait, and records a choice.
2. A screen test captures the box with a portrait and with choices.
3. A test proves that Game moves a story scene sprite from the tick, never from a timer (G-23, F-52).
4. The type-out follows the fixed layout of D-709 at each speed of D-864.
5. The skip follows the rule of OQ-150, and the player never loses a choice.
6. Each string comes from the string table, and det-lint proves it.

**Review focus.**

- The answer of OQ-151 lays the choices out, and the count fits the height of the frame of D-568.
- The player speaks the choices of the lead, and the map always follows the lead (D-267, D-292).
- The box types in silence, and no beep plays (D-223).

**Questions.** OQ-150 and OQ-151. D-864 answers OQ-112.

> *In plain English:* people walk, turn, and speak on the map you already walk on. Their words appear in a box at the bottom, with a face beside them.

### 7.38 PR-15: the headless runner and the bots

Area files: `area-tools.md` section 7.8, `area-ci.md` section 7.13.

**Scope.**

- The headless runner in Tools, which plays a run from a seed and a policy with no Godot (D-64, D-100).
- The two policies: random and greedy (D-64).
- The run record of each run, through Storage (D-494).
- The four end states: complete, softlock, crash, and budget, each failure with its seed (D-64, OQ-74).
- The bot job, which runs both policies on each of the three legs (D-505, OQ-80).
- The upload of the run record of a failed run (T-7).
- The line for the bot job in the PR gate (G-16).

**Out of scope.**

- The night job and the night gate (PR-49, D-496).
- The `playtest-bot` agent, which already exists and drives this runner (D-21).

**Exit tests.**

1. A few hundred runs of each policy complete on each leg, with no crash and no softlock.
2. A planted softlock fails the job with its seed, its policy, and its leg.
3. A planted crash fails the job the same way.
4. The job uploads the run record of each failed run, and a replay of it repeats the failure.
5. A policy makes the same intents that Game makes, and a test proves it (D-493).
6. The runner draws its random numbers outside the rule streams (G-4).

**Review focus.**

- The answer of OQ-74 sets how the runner finds a softlock.
- The answer of OQ-80 sets the count of runs on each leg, against the CI time of D-505.
- The bots play every story scene, because a bot answers each wait intent (D-540).

**Questions.** OQ-74 and OQ-80.

> *In plain English:* simple robots play the game with no screen. They make the same choices a player makes, and every crash they find comes with the seed that repeats it.

### 7.39 PR-49: the night job and the night gate

Area files: `area-tools.md` section 7.9, `area-ci.md` sections 7.14 and 7.15.

**Scope.**

- The night job on a schedule from `main`: ten thousand runs on Linux, and two thousand each on Windows and macOS (D-507, OQ-82).
- The night record of each leg, as an artifact of its run (D-509).
- The `night-gate` command, which fails a PR with no success record from a night inside 48 hours (G-22).
- The night gate job, which finds the newest night through the GitHub API and never reads the checkout (D-509).
- The pass for a night on the exact head commit of a PR, and the pass for a docs-only PR (D-510, D-513).
- The line for the `night-gate` job in the PR gate (G-16).

**Out of scope.**

- The headless runner and the policies (PR-15).
- The M-3 numbers, which seven nights give later.

**Exit tests.**

1. The command passes a fixture night record with every leg green inside 48 hours.
2. It fails an absent record, a stale record, and a failed record, each with its case (T-2).
3. It passes a fixture docs-only PR (D-513).
4. It passes a fixture PR whose head commit has a success record of its own (D-510).
5. It fails a PR that carries a night record in its own checkout (D-509).
6. The PR description shows the output of each fixture (D-500).

**Review focus.**

- The live check cannot run on this PR, and the PR says so (F-37, G-16, D-500).
- The answer of OQ-81 keeps a result current until the merge (F-41).
- The answer of OQ-84 sets which seeds a night plays.
- A leg near the 6-hour limit of GitHub splits into more than one job (D-507).

**Questions.** OQ-81, OQ-82, and OQ-84.

> *In plain English:* every night the robots play thousands of runs on all three systems. No change merges unless a recent night ended with no crash and no dead end.

### 7.40 PR-16: the dungeon parts, the death, and the save points

Area file: `area-exploration.md` section 7.8.

**Scope.**

- The treasure, the locked doors, and the keys (D-41).
- The save points, which save, swap the party, and swap the lessons (D-36, D-58, D-356).
- The MP that a save point restores once for the place, and the health that it does not (D-389, D-555).
- The Theft drill that opens a lock that the map marks as pickable, where a story lock always needs its key (D-386).
- The chest that keeps what the party cannot carry (D-385).
- The dungeon exit, which returns the party to the region map.
- The killed enemy that stays dead until a story event reopens the place (D-555).
- The save window in the stack of PR-62.

**Out of scope.**

- The traps, the hazards, and the statuses on the map (PR-64, D-529).
- The puzzles and the secrets (PR-21).
- The dungeon content of the first playable (PR-17).

**Exit tests.**

1. A bot run that wipes reloads and continues (D-231).
2. A two-character party after a down still reaches the exit of the fixture dungeon (F-7).
3. A save point restores MP once for the place (D-555).
4. A second use restores none, after an exit and a return too (D-555).
5. A Theft drill opens a pickable lock, and it never opens a story lock (D-386).
6. A killed enemy stays dead after the party leaves the dungeon and returns (D-555).
7. A story event that reopens a fixture place brings its enemies and its MP restore back (D-555).
8. A chest over the stack limit keeps the rest, and the save records it (D-385).

**Review focus.**

- The reserve and the swap at a save point answer F-7, and the balance holds with fresh MP (D-356).
- The exit to the region map waits for PR-35, and the PR states what it does until then.
- The snapshot carries the open chests, the open doors, and the dead enemies.

**Questions.** None. OQ-119 and OQ-120 block PR-64.

> *In plain English:* the dungeon gains its chests, doors, keys, and resting stones. A thief can pick some locks, and the story keeps its own doors shut until you find the key.

### 7.41 PR-64: the traps, the hazards, and the statuses on the map

Area file: `area-exploration.md` section 7.9.

**Scope.**

- The traps of D-41, apart from the parts of PR-16 (D-529, OQ-119).
- The hazards of region one (OQ-120).
- The Theft drill that reveals and disarms a trap (D-386).
- What poison, blind, and silence do after a battle, until a cure or a rest at a hub. PR-66 keeps them on each character (D-390, D-792).
- The poison that ticks on the map and can down a character (D-392).
- The silence that stops a rite from the menu, and the blind that does nothing outside battle (D-393).
- The wipe when poison downs every character who fights, even with a healthy reserve (D-397).
- The map HUD: the health mark and the status mark at the edge (D-212, D-390, D-721).

**Out of scope.**

- The statuses inside a fight, and the three that each character keeps after it (PR-66, D-533, D-792).
- The puzzles and the secrets (PR-21).

**Exit tests.**

1. A property test over one thousand seeds proves each trap and each hazard rule.
2. A Theft drill reveals and disarms a fixture trap (D-386).
3. A fixture party that poison downs on the map wipes and reloads (D-397).
4. The same rule holds for a party of one, two, or three (D-336).
5. Silence stops a rite from the menu, and a rest cures it (D-390, D-393).
6. The bots play the fixture dungeon with these rules (D-64). PR-17 runs the bots over each map of the first playable.
7. The snapshot holds each status that lasts on the map.

**Review focus.**

- The answer of OQ-119 sets what each trap does, and OQ-120 the hazards of region one.
- A healthy reserve never saves a party that poison downs (D-397).
- Poison ticks on the tick of Core, and no clock reaches it (G-3).

**Questions.** OQ-119 and OQ-120.

> *In plain English:* the dungeon itself can hurt you. Poison still hurts while you walk, and a party can go down between fights.

### 7.42 PR-35: the region map

Area file: `area-exploration.md` section 7.13.

**Scope.**

- The region map screen of nodes and routes, where the party moves node to node (D-113).
- The route that a story flag opens and closes, through the condition form of PR-68 (D-40, D-329, D-543).
- The autosave on each arrival at a node (D-224).
- The one track of the region map, and no sign of night (D-430, D-445).
- One hub and one dungeon as the first nodes.
- The layout of region one, which follows `docs/world/places.md` (D-250, D-255, D-371).

**Out of scope.**

- The other nodes of region one (PR-23 to PR-27, PR-81).
- The cost of a route in time, because no clock runs (D-442).

**Exit tests.**

1. A closed route refuses the move, and the screen shows why.
2. A replay reproduces the path through the nodes.
3. The autosave writes on each arrival, and it reloads to the same state hash (D-224).
4. A screen test captures the region map.
5. A route that names an absent node id fails with the file and the id.

**Review focus.**

- The route condition uses the one condition form of PR-68 (D-543).
- The answer of OQ-122 sets the format of the map and the cost of a route.
- The screen shows no sign of night, because the story sets the time (D-445).

**Questions.** OQ-122.

> *In plain English:* between places the party travels on a map of the region, along roads that the story opens and closes.

### 7.43 PR-37: retired

PR-37 held the CRT shader and its toggle, which have no purpose after D-618. No later item takes the id (G-10). This entry exists so that a reader of the sequence finds the gap and its reason.

### 7.44 PR-38: the audio synthesizer and the first sounds

Area file: `area-audio.md` sections 7.1, 7.2, and 7.11.

**Scope.**

- The synthesizer in Tools as new code, with instrument voices, filters, and reverb (D-101, D-277, D-412, D-423).
- Integer math alone, so a render gives the same bytes on every leg (D-432, D-502, OQ-158).
- The track format of tracker rows, and the sound effect format of a parameter file (D-438, OQ-156, OQ-157).
- The render into the Game assembly at build, beside the content files (D-547).
- The committed list with the hash of each render, and no audio file in git (D-432, F-31).
- The `listen` command, which renders a batch and plays it for the owner (D-439).
- Six sound effects and one track for the first dungeon.

**Out of scope.**

- The audio player in Game (PR-69, D-546).
- The rules of what plays when (PR-70) and the sound room (PR-71).
- The music of the first playable (PR-72, D-549).

**Exit tests.**

1. Each render matches its hash on every CI leg (D-432).
2. A changed parameter fails the hash test with the file.
3. A track file and a sound file each validate against a schema, and a bad field fails the load.
4. det-lint finds no float type, no clock, and no OS random in the synthesizer (D-496, D-502).
5. The `listen` command renders a batch and plays it on the Mac.
6. A test proves that no rendered audio file enters git (F-31).

**Review focus.**

- The tracker rows and the parameter files stay outside the content hash (D-495).
- The build time grows, and the PR reports the change (D-432, D-547).
- The answer of OQ-158 sets the sample rate, the bit depth, and the channels.

**Questions.** OQ-156, OQ-157, and OQ-158.

> *In plain English:* music and sound start as rows of numbers in a text file. A tool of ours turns those rows into sound, the same way on every computer.

### 7.45 PR-69: the audio player base

Area file: `area-audio.md` sections 7.2 and 7.3.

**Scope.**

- The audio player in Game, which plays a track and a sound effect (D-546).
- The stream that Game makes from the rendered bytes of its own assembly, with a check on every return (D-547, F-56).
- The four audio buses: master, music, sound effects, and ambience, with the four volumes of D-435.
- The mono mixdown as a setting (D-435, OQ-161).
- The mute when the window loses focus, on by default (D-435).

**Out of scope.**

- The rules of what plays when (PR-70, D-546).
- The sound room (PR-71) and the tracks of the first playable (PR-72).

**Exit tests.**

1. A fixture track and a fixture sound effect play through the right bus.
2. A stream from data that is not WAV fails with the id of the render (F-56).
3. Each of the four volumes changes its bus alone.
4. The mono mixdown follows the rule of OQ-161, and a test reads the output.
5. The game mutes when its window loses focus, and it sounds again on focus (D-435).
6. A test proves that Core reads no track and no sound (G-1, G-23).

**Review focus.**

- Every return of the WAV load gets a check, because Godot reports the failure in the log alone (F-56, T-2).
- The audio settings live in the audio group of PR-63 (D-226, D-526).
- Every audio rule lives in Game (G-23).

**Questions.** OQ-161.

> *In plain English:* this part makes sound come out. It sets the volumes, and it mutes the game when the window loses focus.

### 7.46 PR-70: the rules of what plays when

Area file: `area-audio.md` sections 7.4 to 7.10.

**Scope.**

- The music of a place: one track for each map, battle, story scene, and menu (D-413).
- The night version of a place track where the story sets dusk or night, with the phrase end and the crossfade (D-428, D-443, OQ-159, OQ-160).
- The three battle tracks of a region, and the quiet and the resume after a fight (D-415, D-429).
- The music that plays on under every in-game menu (D-421).
- The cue of a story scene, from a small set of mood tracks (D-418, D-548).
- The main theme, the four faction themes, and the five character themes of region one (D-419, D-427).
- The four stings: a wipe, a level up, a victory, and a key find (D-422).
- The ambience of each map in two layers, and the four kinds of map sound (D-424, D-425, OQ-162, OQ-163, OQ-166).
- The sound of each of the eight kinds of ability, with an element layer (D-426, OQ-164).
- The vibration at heavy moments alone (D-434).
- The audio file that names the content ids that it serves, where no rule file names a track (D-548).
- The menu sounds: a cursor tick, a confirm, a cancel, and a refusal (D-431).

**Out of scope.**

- The tracks and the sounds themselves (PR-72, PR-73).
- The sound room (PR-71).

**Exit tests.**

1. A place track loops while the party stays, and a change of map changes the track.
2. A time-of-day change finishes the musical phrase, then it crossfades (D-428).
3. A fight starts the battle track, and the place music resumes after the quiet (D-429).
4. The audio player holds its own count from the tick, and it never reads the playback position as exact (F-56).
5. Each of the four stings plays on its event, and no rule waits for a sting (D-522).
6. A test fails an audio file that names a content id which no content file declares.
7. A test proves that no rule file names a track, a sting, an ambience, or a sound effect (D-548).

**Review focus.**

- D-548 revises D-418 in part, and the PR cites the revision.
- A music batch touches no rule file, so it never changes the content hash (D-495).
- The vibration setting of PR-63 turns every vibration off (D-434).
- The dialogue box types in silence (D-223).

**Questions.** OQ-159, OQ-160, OQ-162, OQ-163, OQ-164, and OQ-166.

> *In plain English:* every place has its own music, a low bed of wind or fire under it, and its own footsteps. The music changes when the story turns the day to night.

### 7.47 PR-71: the sound room

Area file: `area-audio.md` section 7.11.

**Scope.**

- The sound room in a development build, which plays every track and every sound with a map or a battle (D-439).
- The place behind the seam of the debug assembly of PR-45 (D-260, D-492).
- The list of what the room shows (OQ-165).

**Out of scope.**

- The `listen` command, which PR-38 holds (D-439).
- The tracks of the first playable (PR-72).

**Exit tests.**

1. The sound room plays each track and each sound effect of the build.
2. A release export never loads the sound room, and the test of PR-45 covers it (D-492).
3. The room plays a track over a fixture map and over a fixture battle.

**Review focus.**

- The room lands before the first large batch of PR-72, so the owner hears it in place (D-546).
- The room makes intents, as every other debug feature does (D-171, D-493).

**Questions.** OQ-165.

> *In plain English:* the owner listens to every piece of music before it ships. One tool plays a batch on the desk, and this one plays it inside the game.

### 7.48 PR-51: the PNG import

Area file: `area-tools.md` section 7.11.

**Scope.**

- The `import` command with two modes: the hand-edit mode and the generator mode (D-688).
- The hand-edit mode, which reads a PNG that the owner edited by hand (D-107, D-515).
- The write of the frame of its drawing file again, from the pixels of that PNG.
- A failure of the hand-edit mode on a pixel with a color outside the palette, with the file, the pixel, and the color (T-2).
- The generator mode, which reads a picture of the Sprite Fusion generator (D-686, F-86).
- The removal of the blank border of that picture, and a new frame of 32 or 64 pixels (D-689, F-87).
- The map of each pixel to the nearest color of the palette of 64, with the count of the mapped pixels (D-181, D-688, F-89).
- A failure of the generator mode when the content does not fit the frame of 64 pixels, with the file and the size (D-689, T-2).

**Out of scope.**

- The atlas build (PR-34) and the normal maps (PR-48).
- No near color, and no new palette entry in the hand-edit mode. That mode never picks one (D-688, T-2).
- No scale of a picture in either mode. The generator mode crops the blank border and sets the frame (D-689).

**Exit tests.**

1. A round trip of a fixture frame through a PNG gives the same grid.
2. A pixel outside the palette fails in the hand-edit mode, with the file, the pixel, and the color.
3. An indexed PNG fails, because the PNG code refuses one (D-176).
4. The rebuilt atlas matches the pixels of the new grid (F-19).
5. A fixture of 42 pixels with content of 30 pixels gives a frame of 32 pixels.
6. A fixture with content of 70 pixels fails with the file and the size.
7. The generator mode maps a pixel outside the palette to the nearest color, and it reports the count.

**Review focus.**

- The hand-edit mode never guesses a color, which keeps the palette closed (D-181, D-688, T-2).
- The generator mode reports the count of the pixels that it mapped, so no map is silent (T-2).
- A hand edit exports as RGB or RGBA, and the runbook says so.
- Neither mode scales a picture, because a scale of pixel art makes new colors and soft edges (D-689).

**Questions.** None. D-688 and D-689 set the two modes.

> *In plain English:* the owner can fix a sprite in a paint program, and this tool writes the edited image as a text grid again. It refuses any color that the palette lacks. A second mode reads a picture from the art tool, trims it, and pulls each color to the closest palette color.

### 7.49 PR-52: the map preview

Area file: `area-tools.md` section 7.12.

**Scope.**

- The `preview` command, which renders a map file as a PNG from the atlas (D-165).
- The attachment of each preview to the PR description, for the approval of the owner (D-514, G-25).

**Out of scope.**

- The edge tiles, which PR-53 adds to the preview (D-501).
- The light and the effects of a place, which the preview never draws.

**Exit tests.**

1. The command renders a fixture map as a PNG.
2. A test compares the decoded pixels of that render with the atlas and the map (F-19).
3. A map that names an absent tile id fails with the map and the id.
4. The preview reaches the PR description of a fixture map batch (G-25).

**Review focus.**

- The command uses the PNG code of PR-47 and the atlas of PR-34 (D-176, T-1).
- From PR-53 on, the preview draws the edge tiles of each map (D-501).

**Questions.** None.

> *In plain English:* maps are text files too. This tool draws a map as a picture, so the owner can see and approve a place before anyone walks it.

### 7.50 PR-53: the tile-edge tool

Area file: `area-tools.md` section 7.13.

**Scope.**

- The `edges` command, which picks the edge and corner tile for each position from the terrain and the edge rules (D-204).
- The edge rules in content, outside the rule files (D-495, D-501).
- One edge file for each map, which the repository commits (D-501).
- The read of the map and its edge file in Game, where Core reads the map alone (D-501, G-1).
- The edge tiles in the map preview of PR-52.

**Out of scope.**

- The edge drawings themselves, which PR-17 and the later art PRs add.
- No rule reads an edge file, so the content hash never sees one (D-495).

**Exit tests.**

1. A test proves that each committed edge file matches its map and the edge rules (D-501).
2. A terrain pattern that no rule covers fails with the map, the position, and the pattern.
3. Game draws a fixture map with its edge tiles, and a screen test captures it.
4. A test proves that Core reads no edge file (G-1).

**Review focus.**

- The edge file stays outside the content hash, so a new border drawing never breaks a record (D-495, D-501).
- The map preview of PR-52 draws the same edges as Game (T-1).

**Questions.** None.

> *In plain English:* a map names the ground, such as snow or rock, and this tool picks the right border tile for each edge. The picks live in a file of their own.

### 7.51 PR-72: the music and the sounds of the first playable

Area file: `area-audio.md` section 7.12.

**Scope.**

- The tracks of the village, the mining town, and the hanging cells (D-362, D-369).
- The three battle tracks of region one: common, boss, and wrong things (D-415).
- The main theme, and the themes of the cast members of the first playable (D-419, D-427).
- The footsteps of each kind of ground that the first playable needs (D-425, OQ-166).
- The map sounds and the menu sounds of D-425 and D-431.
- The ambience of each map of the first playable (D-424).

**Out of scope.**

- The rest of region one (PR-73, D-549).
- The night version of the mining town, which the flight of Phase 4 needs (D-333, D-443).

**Exit tests.**

1. Each render matches its hash on every CI leg (D-432).
2. Each audio file names the content ids that it serves, and no id is absent (D-548).
3. The sound room plays each new track and each new sound (PR-71).
4. The owner approves the batch in the PR description, after a listen (D-57, D-433, G-25).

**Review focus.**

- The answer of OQ-167 sets the list of the tracks.
- Each track borrows a theme, and the PR description names which (D-419).
- Music from note files is unproven, so a batch that fails the ear costs a rewrite (D-433).

**Questions.** OQ-166 and OQ-167.

> *In plain English:* the music arrives in two batches. This is the first: enough for the first thing that the owner plays.

### 7.52 PR-17: the village, the first hub, and the first dungeon

Area files: every area file. The content PR touches each area.

**Scope.**

- The village and the land near it, the mining town, and the hanging cells, as content (D-28, D-39, D-313, D-369, D-370).
- The tile sets, the layouts, and the edge files of each map (D-110, D-501).
- The enemies with their sprites, their profiles, and their groups (D-535).
- The three views and the two-frame walk of each moving enemy, and a flip for each standing one (D-207, D-744).
- The drawing of the mark of a sight, in the place of the two rectangles of PR-8 (D-208, D-744).
- The backdrop of each place with fights, as a large picture (D-205, D-516).
- The light setup of each map, at its time of day (D-442, D-519).
- The ambient effects of each place (D-187, D-520).
- The treasure, the shop stock, the NPC sprites, and the sprite set of Marrek (D-292).
- The hurt flinch and the down pose of each party member in battle, in one art batch (D-200, D-884).
- A placeholder story scene in the village (D-292).
- The text of Marrek, Bergit, and Dagvar, and of the lessons of the first playable, in the voice (D-362, G-20).
- The normal map of each new drawing (D-183, D-521).

**Out of scope.**

- The arc content of region one (PR-28, PR-29) and the other places (PR-23 to PR-27, PR-81).
- The portraits of the cast (PR-28, PR-29). PR-36 uses fixture portraits.
- The rest of the music of region one (PR-73).
- A boss. The first playable ends when Dagvar joins, and PR-20 builds the phase layer on a fixture boss (D-564).

**Exit tests.**

1. The owner plays from the village until Dagvar joins in the hanging cells (D-362).
2. The play runs on the desktop and on the Deck (D-92).
3. The budget test passes for every map and every battle place of the first playable (D-523).
4. The bots play each map with no crash and no softlock (D-64).
5. Each map preview and each review sheet reaches the PR description (D-514, G-25).
6. Every string comes from the string table, and the owner approves each text batch (D-57, G-7, G-20).
7. Each new content file loads, and no id is absent.

**Review focus.**

- The text follows the `game-text-style` skill, and no line names an agent or a model (D-63, T-6).
- The places follow `docs/world/places.md` (D-250, D-371).
- The art batches carry their review sheets, and the owner approves each one (D-514, G-25).

**Questions.** None. Every question of the systems above closes before this PR.

> *In plain English:* the first real place to play. Everything before this was machinery.

### 7.53 M-3, M-4, and M-6: the measurements of the phase

Area file: none. The cost model in section 4 of `docs/design.md` holds each row.

**Scope.**

- M-3 records the wall time of each leg, and the crash and softlock counts of seven nights (D-507, D-509).
- M-4 records the turns of each encounter and the party downs of each dungeon, by bot policy.
- M-6 records the frame time of the first playable on the Deck, against 60 frames each second (D-161).
- M-6 also records the readability of the font and the sprites, at the scale that OQ-183 sets (D-92, D-621).
- After M-4 reports, the owner sets the M-4 band that Gate 2 checks (D-571).

**Out of scope.**

- No change to the game. A miss changes the content or the budget in a PR of its own (G-14).

**Exit tests.**

1. M-3 copies the numbers of seven nights into the cost model, before GitHub deletes the records (D-509).
2. M-4 states the turns of an encounter and the downs of a dungeon, which bind the resource numbers of D-35.
3. M-6 states the frame time on the Deck, with every effect on (D-617).
4. M-6 states the readability at 1x, and the text stays at 9 pixels or taller (D-459).

**Review focus.** Each measurement reaches the cost model with its date and its method (G-14).

**Questions.** None.

> *In plain English:* three sets of numbers close the phase. They are the cost of the robots each night, the length of a fight, and the speed on the handheld.

### 7.54 Gate 2: the first playable

**The gate.** Gate 2 passes when every line holds:

1. The owner plays from the village until Dagvar joins, on the desktop and on the Deck (D-362).
2. The owner signs off on feel (D-52, D-92).
3. The M-4 numbers land inside the band that the owner set after M-4 (D-571).
4. M-6 records 60 frames each second on the Deck (D-161, D-617).
5. M-6 reads the text and the sprites on the Deck, at the scale that OQ-183 sets (D-621).
6. A miss of that reading reopens OQ-183.
7. Every job of the PR gate is green on every leg (D-481).
8. The `screen-test`, bot, and `night-gate` jobs are green (D-172, D-505, G-22).
9. The budget test passes for every place of the first playable (D-523).

**After the gate.** The owner pays the Steam Direct fee, and the store page goes public as Coming Soon (D-471). Sections 7.54 to 7.56 hold the work that the page needs.

> *In plain English:* at this point the game is a game. The owner walks a village, fights in a mine, and says whether it feels right.

### 7.55 PR-74: the capture

Area file: `area-release.md` section 7.6.

**Scope.**

- The capture in a development build, which replays a run record into PNG frames and a WAV file (D-476, D-551).
- The fixed effect seeds, so a shot repeats exactly (T-7, D-175).
- The place behind the seam of the debug assembly of PR-45 (D-260, D-492).
- The input of the command, and its output format (OQ-171, OQ-172).

**Out of scope.**

- The trailer, which the owner cuts in a video editor (D-476).
- The screenshots themselves (PR-76, D-550).
- No CI job, because a capture needs a window (F-23, D-172).

**Exit tests.**

1. Two captures of one run record give the same frames.
2. The capture writes the audio of the run beside the frames.
3. A release export never loads the capture, and the test of PR-45 covers it (D-492).
4. A capture of a fixture record runs on the Mac of the owner.

**Review focus.**

- Movie Maker output stays identical on faster hardware, and the PR states the check (D-175).
- Movie Maker clamps the window to the display, and the PR states the size that it uses.
- The capture makes intents from the record alone, and it changes no rule (T-7).

**Questions.** OQ-171 and OQ-172.

> *In plain English:* the game can replay a recorded run and write every frame to disk. That gives the same picture each time, so a screenshot or a trailer shot is repeatable.

### 7.56 PR-75: the store text and the owner steps

Area file: `area-release.md` section 7.7.

**Scope.**

- The store text: the short description, the long description, and the feature list (D-452, D-550).
- The draft under the `game-text-style` skill, and the approval of the owner in the PR (D-57, G-20, G-25).
- The checklist of the steps that only the owner can do (D-550).
- The Steam Direct fee, which starts a wait of 30 days before a release (D-85, D-471).
- The studio name, and the search of it with the game name on Steam and in the trademark registers (D-408, D-450, OQ-57).
- The search of the EUIPO, TMview, and WIPO registers, which answered no query from a script (D-408).
- The answer to the AI disclosure of the content survey, before the review of the store page (D-477, OQ-59).

**Out of scope.**

- The store art and the screenshots (PR-76, D-550).
- The system requirements, which M-6 gives (D-482).
- The game never shows the store text, so G-7 does not bind it (D-452).

**Exit tests.**

1. The owner approves the store text in the PR description (D-57, G-25).
2. The checklist names each owner step with its cost and its wait (D-85, D-455).
3. The checklist names each register, and the PR records the result that the owner reports (D-408).
4. The AI disclosure answer of OQ-59 enters the checklist.

**Review focus.**

- The text never names an agent, a harness, or a model (T-6).
- The studio name of OQ-57 also settles the crash address of PR-44 and the credits of PR-33 (D-473).
- Valve wants a page in the Coming Soon state for two weeks before a release (D-471).

**Questions.** OQ-57 and OQ-59.

> *In plain English:* the shop page words get written and approved like any other text in the game. The owner pays the fee and answers the questions that only Valve asks.

### 7.57 PR-76: the store art and the screenshots

Area files: `area-release.md` section 7.8, `area-art.md` section 7.5.

**Scope.**

- The capsules, the logo, and the library images, each as a large picture of drawn pieces (D-475, D-516).
- At least five screenshots from the frame at 2x, 2560 by 1440, which is 16:9 and larger than 1920 by 1080 (D-568, F-34).
- The capture of PR-74, which takes each screenshot from a run record (D-551).
- The capsules of the demo app, which mark it as a demo, in the same art batch (D-475, D-478).
- The review sheets of the art batch, which `gh` attaches to the PR description (D-514, G-25).
- The sizes of the store images, and the choice of the five screenshots (OQ-173, OQ-174).

**Out of scope.**

- The store text and the owner steps (PR-75).
- The trailer (D-476).

**Exit tests.**

1. Each store image renders from its pieces, and a test compares the pixels (F-19).
2. Each screenshot comes from a run record, and a second take gives the same image (D-551).
3. No screenshot holds a bar or a crop (D-568, F-34).
4. The owner approves the art batch from its review sheets (D-514, G-25).

**Review focus.**

- The answer of OQ-173 sets each size that Valve asks for.
- A capsule shows only game art, the game name, and an official subtitle (D-475).
- The screenshots show real play, not a posed shot (D-475).

**Questions.** OQ-173 and OQ-174.

> *In plain English:* a session draws the pictures on the shop page the same way as everything else in the game. The screenshots come from real play.
## 8. Sequence

The global order lives in section 8 of `docs/design.md`, and PR #11 set it (D-488). Phase 2 holds this order:

1. The owner sets the fonts: Terminus TTF and Terminus TTF Bold (D-263, D-264).
2. PR-54, PR-61, PR-7, PR-45, PR-41, PR-8: the export job, the frame, the map, and the enemies.
3. PR-9, PR-89, PR-80, PR-66, PR-55, PR-10: the fight, the walk fault, the enemy record, and the screen (D-557, D-782).
4. PR-48, PR-56, PR-93, PR-63, PR-57, PR-58, PR-94, PR-59, PR-92, PR-95, PR-60, PR-96, PR-97: the normal maps, the light, the settings, the effects, and the automated review.
5. PR-11, PR-98, PR-67, PR-62: the enemies that think, the waiting enemies on screen, the character level, and the menu windows.
6. PR-68, PR-50: the story scenes, the flags, and the screenplay tool, before the first PR that reads a flag (D-556).
7. PR-12, PR-13, PR-91, PR-14, PR-65: the build of a party, the torch, the hub, and the shop.
8. PR-36: the dialogue box.
9. PR-15, PR-49: the bots, the night job, and the night gate.
10. Owner: require the bot and `night-gate` checks on `main` after their first runs.
11. PR-16, PR-64, PR-35: the dungeon and the region map.
12. PR-38, PR-69, PR-70, PR-71: the audio tool, the player, the rules, and the sound room.
13. PR-51, PR-52, PR-53: the PNG import, the map preview, and the tile-edge tool.
14. PR-72: the music and the sounds of the first playable.
15. PR-17: the village, the mining town, and the hanging cells.
16. M-3, M-4, M-6: the night numbers, the encounter numbers, and the Deck.
17. Owner: set the M-4 band from the M-4 numbers, before the sign-off (D-571).
18. **← GATE 2 (first playable).** Section 7.54 holds each line.
19. PR-74, PR-75, PR-76: the capture, the store text, and the store art.
20. Owner: pay the Steam Direct fee, and put the store page public as Coming Soon (D-471).

The next phase file is `phase-3-story-systems.md`.

## 9. Open questions

The register is `docs/questions.md` (D-19). These questions block an item of Phase 2, and each PR asks its questions when it starts (D-487):

| Question | Subject | Blocks |
|---|---|---|
| OQ-57 | The studio name | PR-61 and PR-75 |
| OQ-59 | The AI disclosure of the content survey | PR-75 |
| OQ-64 | The tick while a menu is open. Resolved by D-650 | PR-62 |
| OQ-74 | How the runner finds a softlock | PR-15 |
| OQ-79 | How the screen-test job pins Mesa | PR-41 |
| OQ-80 | The count of bot runs on each PR | PR-15 |
| OQ-81 | How the night gate result stays current | PR-49 |
| OQ-82 | The time of the night | PR-49 |
| OQ-83 | How CI gets the Godot editor and the templates, resolved by D-596 | PR-54 |
| OQ-84 | The seeds of the night | PR-49 |
| OQ-86 | How the atlas places tiles, and how Game draws a map | PR-7, answered by D-667 |
| OQ-89 | Pixel snap in Game | PR-7 |
| OQ-91 | The operations of a large picture on a piece, resolved by D-812 | PR-55 |
| OQ-94 | How the budget test counts one view, resolved by D-842 | PR-56 |
| OQ-95 | Where a torch light comes from, resolved by D-843 | PR-56 |
| OQ-96 | Where the shape of a shadow comes from, resolved by D-845 | PR-56 |
| OQ-97 | The colors of light, resolved by D-846 | PR-56 |
| OQ-98 | GPU particles or CPU particles, resolved by D-875 | PR-57 |
| OQ-99 | What a screen shake moves, resolved by D-876 | PR-57 |
| OQ-100 | The reduced form of a flash and a shake, resolved by D-863 | PR-57 and PR-63 |
| OQ-101 | How fog keeps an enemy visible, resolved by D-885 | PR-58 |
| OQ-102 | How glow stays off sprites, resolved by D-910 | PR-59 |
| OQ-103 | Where shader code lives, resolved by D-825 | PR-10 and PR-60 |
| OQ-104 | The font settings and the load from bytes | PR-61 |
| OQ-106 | Where the settings file lives, and its form, resolved by D-860 | PR-63 |
| OQ-107 | How Game knows the last device of the player | PR-61 |
| OQ-108 | Where a remap lives, and what a conflict does, resolved by D-862 | PR-63 |
| OQ-109 | The dead zone of a stick, resolved by D-861 | PR-63 |
| OQ-110 | The cursor rules of a menu, resolved by D-872 | PR-62 |
| OQ-111 | The scale of the dungeon map screen | PR-62 |
| OQ-112 | The text speeds and the type-out of the dialogue box, resolved by D-864 | PR-36 |
| OQ-113 | The notice log | PR-62 |
| OQ-114 | The rule of sight for the party and a patrol | PR-7 and PR-8 |
| OQ-115 | How a large enemy holds its tiles and sorts | PR-8 |
| OQ-117 | A diagonal step on the map | PR-7 |
| OQ-118 | The limits of the camera on a small map | PR-7 |
| OQ-119 | What a trap does, and what a Theft drill does to it | PR-64 |
| OQ-120 | The hazards of region one | PR-64 |
| OQ-121 | The prices, the buy-back, and the stock of a shop | PR-65 |
| OQ-122 | The format of the region map, and the cost of a route | PR-35 |
| OQ-124 | A defend action. Resolved by D-755 | PR-9 |
| OQ-125 | How many turns the timeline strip shows. Resolved by D-756 | PR-9 and PR-10 |
| OQ-126 | Where the delay of each action lives. Resolved by D-757 | PR-9 |
| OQ-127 | The tie-break of two equal scores. Resolved by D-947 | PR-11 |
| OQ-128 | What makes a profile unable to act. Resolved by D-948 | PR-11 |
| OQ-129 | The chance of a steal, and the cost of a failure. Resolved by D-949 and D-950 | PR-11 and PR-13 |
| OQ-131 | How the screen shows the health of an enemy, resolved by D-826 | PR-10 |
| OQ-132 | A group larger than its rows. Resolved by D-758 | PR-9 and PR-11 |
| OQ-133 | The flee chance and the grace time. Resolved by D-748 and D-763 | PR-9 |
| OQ-134 | The shape of a stat curve | PR-67 |
| OQ-135 | The MP that a save point and a rest restore | PR-67 |
| OQ-136 | The shrink of the experience of an enemy | PR-67 |
| OQ-137 | The lesson slots at each level | PR-12 |
| OQ-138 | The points that a lesson gains from a battle | PR-12 |
| OQ-139 | Two copies of one lesson in one party | PR-12 |
| OQ-140 | What a piece of gear changes | PR-13 |
| OQ-141 | Two accessories with one effect | PR-13 |
| OQ-142 | The stack limit of each item | PR-13 |
| OQ-143 | What a rarity tier changes | PR-13 |
| OQ-217 | How far the party sees in the dark | PR-91 |
| OQ-218 | How much farther an enemy sees a lit torch | PR-91 |
| OQ-219 | The paths of the docs-only set | PR-93 |
| OQ-144 | The full step list of a story scene script | PR-68 |
| OQ-145 | How a step that takes time ends | PR-68 |
| OQ-146 | The shape of a condition | PR-68 |
| OQ-147 | Where content declares each flag id | PR-68 |
| OQ-148 | What fires a story scene trigger | PR-68 |
| OQ-149 | Whether a story scene step starts a battle | PR-68 |
| OQ-150 | How the player skips a story scene | PR-36 |
| OQ-151 | How the choices lay out in the dialogue box | PR-36 |
| OQ-156 | The instrument voices of the synthesizer | PR-38 |
| OQ-157 | The schema of the tracker rows | PR-38 |
| OQ-158 | The sample rate, the bit depth, and the channels | PR-38 |
| OQ-159 | How a track marks the end of a musical phrase | PR-70 |
| OQ-160 | The length of a crossfade | PR-70 |
| OQ-161 | How the mono mixdown works | PR-69 |
| OQ-162 | The shape of the ambience of a map | PR-70 |
| OQ-163 | The volume of the ambience under the music | PR-70 |
| OQ-164 | How an element layers on the sound of its kind | PR-70 |
| OQ-165 | What the sound room shows | PR-71 |
| OQ-166 | The kinds of ground that need their own footsteps | PR-70 and PR-72 |
| OQ-167 | The tracks of the first playable | PR-72 |
| OQ-171 | What the capture command takes | PR-74 |
| OQ-172 | The output format of the capture | PR-74 |
| OQ-173 | The sizes of the store images | PR-76 |
| OQ-174 | Which five screenshots | PR-76 |
| OQ-220 to OQ-231 | The place, the form, the passes, the overlap, the edges, the resolution, the spread, the color, the test floor, the strength, and the coverage of the procedural fog, resolved by D-896 to D-906 and D-908 | PR-94 |
| OQ-232 | The glow that stays, resolved by D-915 | PR-59 |
| OQ-242 | The waiting enemies of a fight. Resolved by D-951 | PR-98 |
| OQ-243 | A column of the waiting enemies, taller than the field | PR-98 |

No open question blocks this file.
