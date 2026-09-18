# Phase roadmap: Phase 2, First playable

Status: **active focused phase roadmap, which PR #11 merged on 2026-09-16.** This file gives each item of Phase 2 its scope, its exit tests, its review focus, and its questions (D-144, D-485, D-487). The area files say how each part works, and each entry names the area file that it cites. This file supersedes no earlier file. Written 2026-09-16 in ASD-STE100.

The design doc holds the thesis of the game, the system map (section 3), and the cost model (section 4). It also holds the guardrails (section 6) and the global order of every PR (section 8). This file cites each decision by its id and never restates it. The index of this folder is `docs/roadmaps/readme.md`.

This file states no external fact. Each external fact of Phase 2 lives in the area file that holds its part, with the date of its check.

Text rules: this file follows ASD-STE100 (D-10). Tables are exempt from sentence-length counts.

## 1. Thesis

Phase 2 turns the machine of Phase 1 into a game that the owner plays. It ends at Gate 2. There the owner walks the village, one hub, and one dungeon on the desktop and on the Deck. Then the owner signs off on feel (D-51, D-92, D-362).

Phase 2 is the largest phase of the plan. It holds 46 PRs, and 43 of them land before Gate 2. Each system, each tool, and each group of screens takes an id of its own (D-486, G-8). The order follows one rule: a PR lands right before the first PR that needs it.

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
| F-47 | Glow can reach a lit sprite in both HDR and SDR | PR-59: OQ-102 holds how glow stays off sprites and tiles |
| F-48 | Godot has no stretch mode that upscales in whole steps, then fits | PR-61: a `SubViewport` at 1x, and both steps in Game (D-232) |
| F-49 | Three font defaults of Godot fight a pixel font | PR-61: the load from bytes and the font settings, with a test |
| F-50 | Five input facts of Godot meet the plan | PR-61, PR-62, and PR-63: intents from events, glyph sets, and a saved remap |
| F-51 | Four Godot defaults of the tile map fight the plan | PR-7: the tile size, the region size, and the two switches |
| F-52 | The camera centers a small map, and its smoothing can run twice in a frame | PR-7: a test locks the centering, and the tick moves the camera |
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
- A run on each merge to `main`, and on each PR that changes the workflow, the presets, or the export code (D-512).
- Three exports, one on each leg: Windows and Linux on x86_64, and the universal macOS build (D-481, D-482).
- The unpack of the .NET export templates into the editor data folder of the runner (F-42, D-596).
- A headless smoke session on each export (D-512).
- The license files of D-467 in each export, and a build artifact that lasts 90 days (D-449).

**Out of scope.**

- The macOS signature and the notarization (PR-79, D-553).
- The release workflow and the GitHub Release (PR-31).
- This job is not a line of the PR gate (D-449).

**Exit tests.**

1. Each leg exports its build from a clean checkout.
2. Each export starts with `--headless` and ends the smoke session with no log error.
3. Each export holds the license files of D-467.
4. A PR that changes an export preset runs the job.
5. Each build artifact appears on the workflow run.

**Review focus.**

- The template file matches its SHA-512, and the job never takes it from an unpinned source (D-511).
- The export presets name the three targets of D-481 and nothing else.
- The owner can download the Linux artifact for a Deck play (D-458).

**Questions.** OQ-83 is resolved (D-596).

> *In plain English:* from the first walkable build on, every merge makes a game that the owner can run on the desktop or the handheld. Each build also starts once in CI.

### 7.2 PR-61: the UI base, the frame, the fonts, and the intents

Area files: `area-ui-input.md` sections 7.1 to 7.5, 7.9, and 7.10.

**Scope.**

- The one 16:9 frame of 1280 by 720, with black bars for every other shape, the Deck included (D-228, D-568).
- The world in a `SubViewport` at 1x, and both steps of the fit that Godot cannot make (D-230, D-232, D-573, F-48).
- The two fonts from the bytes of the Game assembly, with the antialiasing, the hinting, and the subpixel settings of a pixel font (D-263, D-264, D-508, F-49).
- The text helper that puts a string table entry on screen, which det-lint guards (D-499, G-7).
- The UI style file, and the Godot `Theme` that Game builds from it at load (D-527, G-6).
- The input map, and an intent from each input event, never from a poll (D-84, D-493, F-50).
- The glyph sets for the keyboard, Xbox, PlayStation, and the Deck, and the rule for the last device (D-222, D-561, OQ-107).
- The message of a crash on screen, through the text helper, with the studio address of D-473 (D-170, D-559).
- The review sheets of the window frames and the four glyph sets (D-514, G-25).

**Out of scope.**

- The menu windows (PR-62) and the settings screen with the remap (PR-63).
- The dialogue box (PR-36) and the map HUD (PR-7).
- The screen captures of the frame and the fit. PR-41 creates the screen-test job and takes them (D-172, G-16).
- The Steamworks controller type (PR-78, D-553).

**Exit tests.**

1. A test locks the size of the frame and of both fit modes on three screens (D-232, D-568, F-48).
2. Those screens are 1280 by 800, 1920 by 1080, and 2560 by 1440.
3. A test reads back the stretch settings, the filter, and the three font settings (F-45, F-49).
4. A test proves that the fixture panel holds the longest string of the string table (D-241).
5. det-lint fails a Godot text property outside the text helper.
6. A test proves that no intent comes from a poll of the input singleton (F-50).
7. A prompt shows the glyph of the last device, for each of the four sets.
8. No text falls below 9 pixels on the Deck frame (D-459).
9. A crash shows its message through the text helper, and det-lint passes (D-499, D-559).
10. The review sheets of the window frames and the glyph sets reach the PR description (D-514).
11. The owner approves that art batch (G-25).

**Review focus.**

- The two steps of the fit of D-573, and the 1x frame under them (F-48).
- Font oversampling stays off, and each font setting has a test (F-49).
- The `Theme` comes from the style file, and no theme resource file exists (D-527, G-6).
- Every screen shows the same part of the map, so no screen shape gains knowledge (D-566, D-568).

**Questions.** OQ-57, OQ-104, and OQ-107. D-573 resolved OQ-105.

> *In plain English:* this builds the picture frame of the game. It sets one fixed size that the handheld shows exactly, the two fonts, and the look of every menu. It also turns keys and buttons into choices that the rules understand.

### 7.3 PR-7: the tile map, the movement, the sight, and the map scene

Area files: `area-exploration.md` sections 7.1 to 7.5, `area-ui-input.md` section 7.7, `area-art.md` section 7.7.

**Scope.**

- The map file of D-528, which holds the terrain rows and every thing that a rule reads (D-39, D-41).
- Those things are the doors, the locks, the chests, the traps, the save points, the spawn points, and the markers (D-386).
- The time of day of the map, which a story flag can change (D-442).
- Tile-locked movement and sight, in Core (D-100, D-106). No fog of war covers a map (D-566).
- The record of each tile that the party walked, in Core and in the snapshot, which the map screen of PR-62 reads (D-567).
- The map scene in Game, with the tiles from the atlas and the Nearest filter (F-45).
- The camera on the lead, with the limits of a large map and the centering of a small map (D-106, F-52).
- The map HUD: the health mark and the status mark at the edge (D-212, D-390).
- The first content: one fixture dungeon.

**Out of scope.**

- The enemies on the map (PR-8) and the dungeon parts (PR-16, PR-64).
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

- Game moves the camera from the tick, never from the smoothing of Godot (D-203, F-52).
- Each call that reports a failure in the log alone gets a check right after it (F-45, T-2).
- The answer of OQ-86 settles whether Game draws through a tile map layer or draws each tile.
- The sort value of a sprite larger than one tile (D-206, OQ-115).

**Questions.** OQ-86, OQ-89, OQ-114, OQ-117, and OQ-118. D-566 resolved OQ-116.

> *In plain English:* this is the first thing that the owner can open and move in. The dungeon is a grid of tiles, the party walks it one tile at a time, and the view follows.

### 7.4 PR-45: the debug assembly and the console

Area file: `area-core.md` section 7.13.

**Scope.**

- The debug assembly, which holds the console and the extra intent handlers of the seam of PR-6 (D-260, D-492).
- The console of D-171, which a development build opens and a release build lacks.
- The mark that a debug intent carries in the run record (D-171).
- The test that a release export never loads the assembly (D-492).

**Out of scope.**

- The capture (PR-74) and the sound room (PR-71), which land later behind the same seam.
- No change to Core, which names no debug assembly (D-492).

**Exit tests.**

1. A development build opens the console and runs each command.
2. A release export holds no reference to the debug assembly.
3. A record with a debug intent carries its mark (D-171).
4. A host with no debug handler refuses that record with the tick and the intent.

**Review focus.**

- Core names no debug assembly, and no conditional compilation enters Core (D-260, F-27).
- Each console command makes an intent, so a cheat still replays.

**Questions.** None.

> *In plain English:* cheats and test commands live in a separate part that the shipped game never contains. A run that used one still replays, and the record says so.

### 7.5 PR-41: the screen-test job

Area files: `area-ci.md` section 7.12, `area-effects.md` section 7.13, `area-ui-input.md` section 7.13.

**Scope.**

- The Linux job of D-172, which runs Godot under Xvfb with the OpenGL driver and a pinned Mesa (F-23, OQ-79).
- The capture of each fixture scene at 1x, and both fit modes at 1080 and 1440 screen rows (D-232, D-568).
- The frame compare in Tools, which compares decoded pixels with a committed CI baseline (F-19).
- The fixed sources of change at capture: the particle seeds and the time of day.
- The desktop command that makes a contact sheet with the real renderer (D-172).

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

- The Mesa pin, from the answer of OQ-79, and its decision row (G-13).
- The Compatibility renderer of CI differs from the Deck, and the PR says where (D-172).
- The baseline holds both fit modes at both screen row counts (D-232, D-568).
- The capture at 1080 rows proves the rule of D-573. Every pixel keeps the same size, with a slight softness at pixel edges.

**Questions.** OQ-79.

> *In plain English:* the computers that check each change have no screen. This job gives one of them a software screen with a fixed picture, so a broken screen fails before it merges.

### 7.6 PR-8: the enemies on the map

Area file: `area-exploration.md` sections 7.6 and 7.7.

**Scope.**

- Fixed enemies and patrols with their own sight, and no random encounter (D-37).
- The sight mark for a beat, then the start of the encounter (D-208).
- The first blow to the side that reached the other from behind (D-265).
- The return of a fled group to its route, with a grace time before the next fight (D-381).
- Three views with a two-frame walk for an enemy that moves, and a flip for one that stands (D-108, D-207).
- The area of a large enemy, and the proof that its body fits everywhere in that area (D-206, D-209).

**Out of scope.**

- The fight itself and the hand-off of the encounter (PR-9, D-531), and the battle screen (PR-10).
- The transition over the hand-off (PR-60).
- The enemy groups of the first playable (PR-17).

**Exit tests.**

1. A property test over one thousand seeds proves that a patrol never leaves its route.
2. The same test proves that a patrol never sees through a wall.
3. The time of day of the map picks the route (D-193, D-442).
4. A large enemy never leaves its area, and a load proves the fit (D-209).
5. A fled group starts no battle inside its grace time (D-381).

**Review focus.**

- The sight rule of OQ-114 covers the party and a patrol with one implementation (T-1).
- The rule of OQ-115 gives a large enemy one sort value at its front row.
- The grace time counts ticks in Core, and no clock reaches it (G-3).

**Questions.** OQ-114 and OQ-115.

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

**Review focus.**

- No wait intent enters a fight, and the input gate lives in Game (D-532).
- Each delay counts ticks with integer math alone (D-164, G-2).
- The answer of OQ-126 puts the delay of each action in one place.
- The basic attack needs no lesson and no MP (D-359, F-8).

**Questions.** OQ-124, OQ-125, OQ-126, OQ-132, and OQ-133.

> *In plain English:* this is the fight itself, with the order of turns shaped by speed. Nothing draws it yet.

### 7.8 PR-80: the enemy record

Area file: `area-battle.md` section 7.7.

**Scope.**

- The enemy record in content: the stats of each enemy and the ids of its abilities (D-557).
- The strict reader of the record in Core, with a load test (D-177, G-6).
- The switch of PR-9 from fixture stats to the record (D-557).

**Out of scope.**

- The element table of each enemy, which PR-66 adds to the record (D-533).
- The profile, the steal list, and the group file (PR-11, D-65, D-535).
- The enemies of the first playable, which PR-17 writes.

**Exit tests.**

1. A fixture enemy record loads, and PR-9 fights it in place of its fixture stats (D-557).
2. A record with an absent field fails the load with the file and the field (T-2).
3. A record that names an absent ability id fails the load with the file and the id.
4. A number with a fraction in a record fails the load (G-2).
5. The replay of a fixture fight against the record gives the same state hash on every leg.

**Review focus.**

- The record holds the stats and the ability ids alone, so the profile of PR-11 keeps its own file (D-557, G-8).
- The simulation version bumps, and the identity file gains a run (G-17, D-504).
- Each new content id is permanent (D-166).

**Questions.** None. OQ-132 holds a group larger than its rows, and PR-9 and PR-11 ask it.

> *In plain English:* each enemy gets its numbers and its list of moves in a data file. The fight reads that file in place of the placeholder numbers.

### 7.9 PR-66: the elements and the statuses

Area file: `area-battle.md` section 7.4.

**Scope.**

- The eight elements, each with weakness, resist, and absorb (D-74).
- The ten statuses, and the rule that every status but poison, blind, and silence ends with its fight (D-75, D-390).
- The aptitude bonus and the half bonus of a side aptitude, in basis points (D-358, D-360).
- The element table of each enemy, on the record of PR-80, and of each piece of gear (D-557).

**Out of scope.**

- What poison, blind, and silence do on the map (PR-64, D-393).
- The shape icons of the accessibility settings (PR-63, D-214).
- The cure rites, which PR-12 gives to Mend (D-394).

**Exit tests.**

1. A property test over one thousand seeds proves each element against weakness, resist, and absorb.
2. The same test proves the start and the end of each of the ten statuses.
3. A status that ends with its fight is absent after the fight (D-390).
4. Poison, blind, and silence remain after the fight (D-390).
5. Every damage number comes from integer math, and det-lint proves it.

**Review focus.**

- Holy and dark carry no claim of a god in any string (D-158, G-20).
- The bonus of an aptitude reads the same table as a side aptitude, at half (D-360).
- The simulation version bumps, and the identity file gains a run (G-17, D-504).

**Questions.** None. OQ-134 to OQ-143 block the numbers, and PR-67 and PR-12 hold them.

> *In plain English:* fire, ice, and six more elements meet armor that likes or hates each one. Poison, blindness, and silence follow you out of the fight.

### 7.10 PR-55: the large pictures

Area files: `area-art.md` section 7.5, `area-tools.md` section 7.5.

**Scope.**

- The large picture format, which places drawn pieces at pixel positions, with repeats (D-516, F-44).
- The load test of the format, and a failure that names the file and the entry (T-2).
- The render of a large picture as a PNG in Tools, and the draw in Game (D-518).
- Full-screen art that covers the frame of 1280 by 720 (D-568).

**Out of scope.**

- The backdrop content, which PR-10 and PR-17 write (D-205).
- The store images (PR-76, D-475).
- The normal map of each piece (PR-48, D-516).

**Exit tests.**

1. A test decodes the render of a fixture large picture and compares its pixels with its pieces (F-19).
2. A large picture that names an absent piece fails with the file and the entry.
3. The render covers the frame of 1280 by 720 with no gap (D-568).
4. det-lint finds no float type in the render code (D-502).

**Review focus.**

- The answer of OQ-91 sets which operations a picture offers on a piece.
- Each piece stays small enough to draw by hand and to check (F-44).
- The render compares pixels, never bytes (F-19).

**Questions.** OQ-91.

> *In plain English:* a battle background is too big to write as one text picture. The game builds it like a stage set from small drawn parts.

### 7.11 PR-10: the battle scene

Area files: `area-battle.md` section 7.10, `area-ui-input.md` sections 7.1 and 7.5, `area-effects.md` section 7.8.

**Scope.**

- The side view: the enemies on the left, the party on the right, each side in two rows (D-111, D-377).
- The timeline strip across the top, and the command menu and the status at the bottom (D-111).
- The attack pose on an action, the color flash on a hit, and the damage number over its target (D-96, D-108, D-213).
- One message line for each action, in the game voice, from the string table (G-7, G-20).
- The backdrop of the place, as a large picture of PR-55, with its drift (D-205).
- The shader of the hit flash, which never uses the normal map member (D-183, the external facts of `area-effects.md`).

**Out of scope.**

- The blood, the sparks, the shake, and the hit-stop (PR-57, D-186).
- The light on the battle scene (PR-56) and the battle music (PR-70, PR-72).
- The boss phases (PR-20).

**Exit tests.**

1. A screen test renders a fixture battle (D-172).
2. The owner reads a fight from the screen alone.
3. Each message comes from the string table, and det-lint proves it.
4. A test proves that each panel holds its longest string (D-241).
5. The health of an enemy reads from the screen, in the form of OQ-131.

**Review focus.**

- The answer of OQ-125 sets how many turns the strip shows.
- The answer of OQ-103 sets where shader code lives.
- The backdrop drift never moves a rule, and no rule waits for it (D-522, G-23).

**Questions.** OQ-103, OQ-125, and OQ-131.

> *In plain English:* the fight appears on screen: who acts next, who is low, and what you can do. Every line reads in the voice of the game.

### 7.12 PR-48: the normal maps

Area files: `area-tools.md` section 7.7, `area-effects.md` section 7.5, `area-art.md` section 7.11.

**Scope.**

- The normal-map command, which builds a normal map for each sprite, tile, and piece from its drawing file (D-183, D-516).
- The optional override grid, which the artist writes by hand (D-184).
- Integer math with an integer square root, so every leg gives the same pixels (D-502, F-38).
- The normal-map atlas, which uses the atlas index of the color atlas (D-184, D-517).
- The review sheet, which draws each sprite under eight fixed light directions (D-514, D-521).

**Out of scope.**

- Light in the engine (PR-56). The review sheet computes its light in Tools (D-521).
- A normal map for a portrait, an icon, a glyph, or a window frame, which take no scene light (D-210).

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

**Questions.** None. OQ-87 set the form of a review sheet in PR-34.

> *In plain English:* a normal map tells the light which way each pixel faces. A tool builds it from the drawing, and the owner checks a sheet of each sprite lit from eight sides.

### 7.13 PR-56: the light and the shadows

Area file: `area-effects.md` sections 7.4 and 7.6.

**Scope.**

- The light setup file: the ambient light and the point lights of one map at one time of day (D-442, D-519).
- The ambient light as the one canvas modulate of the world.
- The point lights of torches, waystones, and spells, each with a light texture that Game builds and checks (D-183, F-46).
- A height on each light, because a light at height zero gives no light to a flat normal (F-46).
- Hard shadows from walls (D-183, OQ-96).
- The normal map of each sprite, tile, and piece through a canvas texture, with the Nearest filter on both atlases (F-45).
- The effect budget file, its strict reader in Core, and the budget test with its light rows (D-517, D-523).
- The light of the battle backdrop, which comes from the map where the fight began (D-205).

**Out of scope.**

- Particles and the battle effects (PR-57), and glow (PR-59).
- The light of a puzzle, which PR-21 drives from Core state (D-41).
- The light setups of the first playable (PR-17).

**Exit tests.**

1. A screen test captures a lit fixture map and a lit fixture battle (D-172).
2. A light with no texture fails at load with its id (F-46).
3. The budget test fails a view with more than 15 lights on one canvas item (F-46).
4. A map that passes a row of the budget file fails the budget test, with the file and the count.
5. A light setup that names an absent map id fails with the file and the id.
6. No shader on lit art uses the normal map member of Godot.

**Review focus.**

- The budget rows match the numbers that the Deck test measured (D-523).
- The answer of OQ-94 settles how the test counts one view.
- The answer of OQ-95 settles where a torch light comes from, and OQ-97 its color.
- The light setup stays outside the content hash, so new light never breaks a record (D-495, D-519).

**Questions.** OQ-94, OQ-95, OQ-96, and OQ-97.

> *In plain English:* each place gets its light from a small file: how dark it is and where each torch glows. Walls throw hard shadows, and each sprite catches light on the side that faces the flame.

### 7.14 PR-63: the settings and the accessibility settings

Area file: `area-ui-input.md` sections 7.11 and 7.12.

**Scope.**

- The settings screen with four groups: display, audio, controls, and battle (D-226).
- Display: the window mode and the scale of D-232 (D-232, D-618).
- Audio: the master, music, effects, and ambience volumes, the mute in the background, and the mono toggle (D-435).
- Controls: the remap, the stick dead zone, and the vibration setting (D-214, D-434).
- Battle: the message speed and the remembered cursor (D-226).
- The four accessibility settings: the flash and shake reduction, the text speed and skip, the shape icons, and the remap (D-214).
- The 18 shape drawings of 16 by 16, one for each element and each status (D-74, D-75).
- The settings file outside the save files, which never enters a run record (D-494, T-7, OQ-106).
- The format version of the settings file, and one migration step with a fixture file for each new setting (D-570).
- The review sheets of the 18 shape drawings (D-514, G-25).

**Out of scope.**

- The effects that the reduction turns down, which PR-57 to PR-60 add.
- The audio that the volumes control (PR-69, PR-70).
- The title screen and its settings entry (PR-33).

**Exit tests.**

1. Each setting saves and loads through the settings file.
2. A remap lasts across a restart, because Godot does not save one (F-50).
3. A remap conflict follows the rule of OQ-108, and the screen states it.
4. A settings file of an older format version loads through its migration step (D-570).
5. A key that no version declares fails the load with the file and the key (T-2, D-570).
6. No setting reaches a run record, and a test proves it (T-7).
7. The stick dead zone takes the value of OQ-109, not the value of the docs (F-50).
8. A screen test captures the settings screen.
9. The review sheets of the 18 shape drawings reach the PR description (D-514).
10. The owner approves that art batch (G-25).

**Review focus.**

- The answer of OQ-106 sets the place and the form of the settings file.
- The default `ui_*` actions keep their events, because Godot cannot remove one (F-50).
- Vibration turns off for any player, and it has limits on macOS (F-50).
- The shape icons cover all eight elements and all ten statuses (D-214).

**Questions.** OQ-100, OQ-106, OQ-108, and OQ-109.

> *In plain English:* one screen holds every choice about the game: the picture, the sound, the buttons, and the pace of battle. A player who needs calm can turn the flashes and the shakes down.

### 7.15 PR-57: the effect files, the particles, and the battle effects

Area file: `area-effects.md` sections 7.7 and 7.8.

**Scope.**

- The effect file: the emitters, the palette colors, and the timings in ticks (D-182, D-266).
- The record and the strict reader of an effect file in Core, which no rule reads (D-517).
- The Godot particle nodes that Game builds from each effect file at load, with no resource file (D-182, G-6).
- The blood and the sparks of a hit, the short screen shake of a heavy blow, and the brief hit-stop (D-186).
- The flash of a spell, with a point light of PR-56 for its length (D-183, D-186).
- The reduced form of each flash and each shake, under the setting of PR-63 (D-214, OQ-100).
- The particle rows of the effect budget (D-523).

**Out of scope.**

- The ambient effects (PR-58), glow (PR-59), and the transitions (PR-60).
- The vibration of a heavy blow, which PR-63 holds as a setting and PR-70 plays (D-434).

**Exit tests.**

1. A screen test captures each battle effect, in both reduced forms (D-214).
2. An effect file with a bad field fails the load with the file and the field.
3. The budget test counts each live emitter against the particle rows (D-523).
4. A test proves that no rule reads the length of an effect (D-522).
5. Each effect file names the content ids that it serves, and a test fails an absent id.

**Review focus.**

- The answer of OQ-98 settles the particle node kind, and the fixed seed of a capture (D-172).
- The Compatibility renderer lacks two particle features, and the PR names each capture that differs (D-172).
- A particle color is a palette key, so the screen keeps one palette (D-181).

**Questions.** OQ-98, OQ-99, and OQ-100.

> *In plain English:* a burst of sparks is a small data file: how many bits, which colors, and how long. A hit in battle shows blood, sparks, and a jolt, and it passes fast.

### 7.16 PR-58: the ambient effects

Area file: `area-effects.md` section 7.9.

**Scope.**

- The four ambient kinds of region one (D-187). They are snow and wind, fog and mist, fire with embers and smoke, and dust with drips and motes.
- The weather of each map, which its time of day never changes (D-202, D-442).
- The ambient effects over a battle backdrop (D-205).
- The point light that a fire can carry (D-183).
- The full-screen rows of the effect budget for fog and the other full-screen kinds (D-523).

**Out of scope.**

- The ambience sound, which PR-70 plays (D-424).
- The ambient effects of the first playable, which PR-17 writes (D-520).

**Exit tests.**

1. A screen test captures each of the four kinds.
2. Fog never hides an enemy that the player must see, by the rule of OQ-101.
3. The budget test counts each full-screen pass (D-523).
4. An ambient effect file that names an absent map id fails with the file and the id.

**Review focus.**

- The answer of OQ-101 keeps an enemy visible through fog (D-187).
- The weather of a map matches its ambience sound, which PR-70 adds (D-424).
- The ambient effects draw over the backdrop, not under it (D-205).

**Questions.** OQ-101.

> *In plain English:* each place has its own weather: snow in the pass, smoke by a fire, dust in the mine. The weather never hides an enemy that the player needs to see.

### 7.17 PR-59: the glow

Area file: `area-effects.md` section 7.10.

**Scope.**

- A soft glow on fire, spells, waystones, and the thing below (D-188).
- The rule that sprites, tiles, and the UI never glow (D-188, D-210).
- The glow row of the effect budget, because glow is a full-screen pass (D-523).

**Out of scope.**

- The transitions (PR-60).
- The glow of later regions, which their content adds.

**Exit tests.**

1. A screen test captures a lit fixture scene with glow.
2. A bright light on a pale sprite never makes that sprite glow (F-47).
3. The budget test counts the glow pass (D-523).
4. The PR names each capture where the Compatibility renderer differs from the Deck (D-172).

**Review focus.**

- The answer of OQ-102 keeps glow off sprites and tiles, in both HDR and SDR (F-47).
- Godot adds light with no upper clamp, so the PR states the threshold that it sets (F-47).

**Questions.** OQ-102.

> *In plain English:* flames and magic give off a soft haze of light, and the people and walls that they light stay crisp.

### 7.18 PR-60: the transitions

Area file: `area-effects.md` section 7.11.

**Scope.**

- The library of ten transitions (D-195).
- The content table that assigns a transition to each kind of encounter, with a default for each region (D-196).
- The transition as an effect file with its shader in Game (D-182, D-191, OQ-103).
- The color split under the flash and shake reduction (D-195, D-214).
- The wait at the end of a battle, which a wait intent ends (D-522).
- The full-screen row of the effect budget for a transition (D-523).

**Out of scope.**

- The transitions of later regions (D-194).
- Every effect of the frame, with the style of G-27 (PR-56 to PR-60).

**Exit tests.**

1. A screen test captures each of the ten transitions.
2. The color split has a reduced form, and the test captures both (D-214).
3. A transition table that names an absent encounter kind fails with the file and the kind.
4. A test proves that the map waits for the wait intent, and never for a timer (D-522, G-23).
5. The budget test counts each transition pass (D-523).

**Review focus.**

- The answer of OQ-103 sets where shader code lives.
- A replay never waits, because the wait intent sits in the record (D-493, D-522).
- Snow whiteout fits region one, and the table names its default (D-194, D-196).

**Questions.** OQ-103.

> *In plain English:* each fight starts with a screen effect, such as shattered glass or a whiteout of snow. The kind of fight picks the effect, so a boss always looks different.

### 7.19 PR-11: the evaluator, the profiles, and the groups

Area file: `area-battle.md` sections 7.6 and 7.7.

**Scope.**

- The evaluator that scores every legal action by its simulated outcome (D-65, D-377).
- The depth of D-534: each legal action, and the strongest answer of the other side.
- The profile content format with its term weights and its traits, and its validator (D-65, G-21).
- The steal list of items and gold on each profile (D-383).
- The group file of each region, which holds each enemy group with its rows and its profiles (D-535).
- The fixture profiles that prove the evaluator. PR-17 writes the profiles of the first playable.
- The measurement of the cost of a turn, before Gate 2 (F-53, G-14).

**Out of scope.**

- The boss phases (PR-20, D-533).
- The balance pass over the weights (PR-30).

**Exit tests.**

1. A fixture enemy with a protector profile heals its ally before it attacks.
2. A profile with no legal action fails the load, by the rule of OQ-128.
3. A map that names an absent group id fails with the map and the id (D-535).
4. A property test over one thousand seeds proves that the evaluator never stalls a turn.
5. The tie-break of two equal scores follows OQ-127, and a test locks it.
6. The PR reports the count of legal actions and the time of a turn (F-53).

**Review focus.**

- The cost of a turn holds on the Deck at 60 frames each second (D-161, F-53).
- A miss of that target changes the depth or the profiles in this PR (G-14).
- The evaluator draws from one seeded stream, and its order of work never changes (G-4, T-7).

**Questions.** OQ-127, OQ-128, OQ-129, and OQ-132.

> *In plain English:* each enemy tries every move it can make, imagines your best answer, and picks the move that leaves it best off. That is what makes the fights hard.

### 7.20 PR-67: the character level, the experience, MP, and the stat curves

Area file: `area-progression.md` sections 7.1, 7.2, and 7.3.

**Scope.**

- The character level from experience, and half experience for the reserve and for a downed character (D-34, D-73, D-387).
- The shrink of the experience of an enemy as the party outlevels it (D-388, OQ-136).
- The start level of a character who joins late, from content (D-363).
- MP, and its recovery at a hub, at a save point once for the place, and from scarce items (D-42, D-389, D-555).
- The stat curve of each character in content: the health, the MP, the attack, the defense, and the speed at each level (D-537, F-54).
- The level-up sting event, which PR-70 plays (D-422).

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

### 7.21 PR-62: the menu windows and the dungeon map screen

Area file: `area-ui-input.md` sections 7.6 and 7.7.

**Scope.**

- The main list, which opens one window for each task: party, lessons, gear, items, status, and save (D-211).
- The party window, which sets the starting row of each character, and the snapshot that keeps the row (D-377, D-558).
- The status window, which reads the state of PR-9, PR-12, and PR-67 (D-569).
- The window stack, where back closes one window and the map stays visible behind (D-211).
- The pause of the world while a menu is open (D-162, OQ-64).
- The mouse on menus alone, which makes the same intent as a key or a button (D-219, D-493, OQ-110).
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
5. A test proves that the world does not tick while a menu is open (D-162, OQ-64).
6. The dungeon map screen shows each walked tile, with the doors, the save points, and the exits on it (D-567).
7. The party window sets the row of a character, and a fight starts with that row (D-377, D-558).
8. The row survives a save and a load, through a snapshot format bump and its migration (D-166, D-558).
9. The status window shows the level, the MP, and the stats of each character (D-569).

**Review focus.**

- The answer of OQ-110 sets the cursor rules, and OQ-111 the scale of the map screen.
- The answer of OQ-113 sets which notices the log keeps, and how many.
- Each later system PR adds one window to this stack (D-525).

**Questions.** OQ-64, OQ-110, OQ-111, and OQ-113.

> *In plain English:* menus are windows that stack on each other, and the world stops while one is open. A second screen draws each tile of the dungeon that the party walked.

### 7.22 PR-68: the story scene format, the story scene runner, the flags, and the conditions

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

### 7.23 PR-50: the screenplay tool

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

### 7.24 PR-12: the lessons, the slots, and the aptitudes

Area file: `area-progression.md` sections 7.4, 7.5, and 7.6.

**Scope.**

- The lesson, a rite or a drill that any character equips to gain an ability (D-272, D-275, D-278).
- The lesson slots on the character, which grow with the character level (D-356, OQ-137).
- The swap of lessons at a hub and at a save point, which holds for the dungeon visit (D-356).
- The points that every equipped lesson gains from each battle won, and half for a reserve character (D-357).
- The named forms of each lesson, and the point total that opens each form (D-539).
- The growth that belongs to the character, not to the lesson (D-361).
- The eight kinds, the main aptitude of each character, and the bonus of a lesson of that kind (D-274, D-281, D-358).
- The side aptitude behind a story flag of PR-68, with an empty mark in the menu before the unlock (D-282, D-283, D-538, D-556).
- The Mend rites and the cure rites that also work from the menu outside battle (D-391).
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

### 7.25 PR-13: the gear, the items, and the inventory

Area file: `area-progression.md` sections 7.8 and 7.9.

**Scope.**

- The six equipment slots: the weapon, the shield or off-hand, the head, the body, and two accessories (D-44).
- Gear that any character wears, because the aptitudes carry the difference (D-374).
- Fixed, hand-authored gear with a few rarity tiers, and no random affix and no crafting (D-45, OQ-143).
- The pack, with a small fixed number of each item (D-382, OQ-142).
- The item use on a turn, which restores less in a fight than outside one (D-382).
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
- A steal takes one entry from the list of an enemy, which PR-11 holds (D-383).

**Questions.** OQ-140, OQ-141, OQ-142, and OQ-143.

> *In plain English:* six slots, and anyone can wear anything. What you find is what the author placed, so a good weapon is a real event.

### 7.26 PR-14: the hub map, the NPCs, and the services

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

### 7.27 PR-65: the shop and the gold

Area file: `area-exploration.md` section 7.12.

**Scope.**

- The gold economy: gold from enemies and from treasure, which buys gear, items, and rest (D-60).
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

### 7.28 PR-36: the dialogue box, the portraits, and the story scene on screen

Area files: `area-story.md` section 7.4, `area-ui-input.md` section 7.8.

**Scope.**

- The dialogue box at the bottom, with the portrait, a name plate, and the choices (D-109, D-114, D-223).
- The type-out at the chosen speed, in silence (D-223, OQ-112).
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
4. The type-out follows the layout rule of OQ-112 at each speed.
5. The skip follows the rule of OQ-150, and the player never loses a choice.
6. Each string comes from the string table, and det-lint proves it.

**Review focus.**

- The answer of OQ-151 lays the choices out, and the count fits the height of the frame of D-568.
- The player speaks the choices of the lead, and the map always follows the lead (D-267, D-292).
- The box types in silence, and no beep plays (D-223).

**Questions.** OQ-112, OQ-150, and OQ-151.

> *In plain English:* people walk, turn, and speak on the map you already walk on. Their words appear in a box at the bottom, with a face beside them.

### 7.29 PR-15: the headless runner and the bots

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

### 7.30 PR-49: the night job and the night gate

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

### 7.31 PR-16: the dungeon parts, the death, and the save points

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

### 7.32 PR-64: the traps, the hazards, and the statuses on the map

Area file: `area-exploration.md` section 7.9.

**Scope.**

- The traps of D-41, apart from the parts of PR-16 (D-529, OQ-119).
- The hazards of region one (OQ-120).
- The Theft drill that reveals and disarms a trap (D-386).
- Poison, blind, and silence that last past a battle until a cure or a rest at a hub (D-390).
- The poison that ticks on the map and can down a character (D-392).
- The silence that stops a rite from the menu, and the blind that does nothing outside battle (D-393).
- The wipe when poison downs every character who fights, even with a healthy reserve (D-397).
- The status mark in the map HUD (D-390).

**Out of scope.**

- The statuses inside a fight (PR-66, D-533).
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

### 7.33 PR-35: the region map

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

### 7.34 PR-37: retired

PR-37 held the CRT shader and its toggle, which have no purpose after D-618. No later item takes the id (G-10). This entry exists so that a reader of the sequence finds the gap and its reason.

### 7.35 PR-38: the audio synthesizer and the first sounds

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

### 7.36 PR-69: the audio player base

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

### 7.37 PR-70: the rules of what plays when

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

### 7.38 PR-71: the sound room

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

### 7.39 PR-51: the PNG import

Area file: `area-tools.md` section 7.11.

**Scope.**

- The `import` command, which reads a PNG that the owner edited by hand (D-107, D-515).
- The write of the frame of its drawing file again, from the pixels of that PNG.
- A failure on a pixel with a color outside the palette, with the file, the pixel, and the color (T-2).

**Out of scope.**

- The atlas build (PR-34) and the normal maps (PR-48).
- No near color, and no new palette entry. The command never picks one (T-2).

**Exit tests.**

1. A round trip of a fixture frame through a PNG gives the same grid.
2. A pixel outside the palette fails with the file, the pixel, and the color.
3. An indexed PNG fails, because the PNG code refuses one (D-176).
4. The rebuilt atlas matches the pixels of the new grid (F-19).

**Review focus.**

- The command never guesses a color, which keeps the palette closed (D-181, T-2).
- A hand edit exports as RGB or RGBA, and the runbook says so.

**Questions.** None.

> *In plain English:* the owner can fix a sprite in a paint program. This tool writes the edited image as a text grid again, and it refuses any color that the palette lacks.

### 7.40 PR-52: the map preview

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

### 7.41 PR-53: the tile-edge tool

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

### 7.42 PR-72: the music and the sounds of the first playable

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

### 7.43 PR-17: the village, the first hub, and the first dungeon

Area files: every area file. The content PR touches each area.

**Scope.**

- The village and the land near it, the mining town, and the hanging cells, as content (D-28, D-39, D-313, D-369, D-370).
- The tile sets, the layouts, and the edge files of each map (D-110, D-501).
- The enemies with their sprites, their profiles, and their groups (D-535).
- The backdrop of each place with fights, as a large picture (D-205, D-516).
- The light setup of each map, at its time of day (D-442, D-519).
- The ambient effects of each place (D-187, D-520).
- The treasure, the shop stock, the NPC sprites, and the sprite set of Marrek (D-292).
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

### 7.44 M-3, M-4, and M-6: the measurements of the phase

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

### 7.45 Gate 2: the first playable

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

**After the gate.** The owner pays the Steam Direct fee, and the store page goes public as Coming Soon (D-471). Sections 7.46 to 7.48 hold the work that the page needs.

> *In plain English:* at this point the game is a game. The owner walks a village, fights in a mine, and says whether it feels right.

### 7.46 PR-74: the capture

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

### 7.47 PR-75: the store text and the owner steps

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

### 7.48 PR-76: the store art and the screenshots

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
3. PR-9, PR-80, PR-66, PR-55, PR-10: the fight, the enemy record, and the screen (D-557).
4. PR-48, PR-56, PR-63, PR-57, PR-58, PR-59, PR-60: the normal maps, the light, the settings, and the effects.
5. PR-11, PR-67, PR-62: the enemies that think, the character level, and the menu windows.
6. PR-68, PR-50: the story scenes, the flags, and the screenplay tool, before the first PR that reads a flag (D-556).
7. PR-12, PR-13, PR-14, PR-65: the build of a party, the hub, and the shop.
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
18. **← GATE 2 (first playable).** Section 7.45 holds each line.
19. PR-74, PR-75, PR-76: the capture, the store text, and the store art.
20. Owner: pay the Steam Direct fee, and put the store page public as Coming Soon (D-471).

The next phase file is `phase-3-story-systems.md`.

## 9. Open questions

The register is `docs/questions.md` (D-19). These questions block an item of Phase 2, and each PR asks its questions when it starts (D-487):

| Question | Subject | Blocks |
|---|---|---|
| OQ-57 | The studio name | PR-61 and PR-75 |
| OQ-59 | The AI disclosure of the content survey | PR-75 |
| OQ-64 | The tick while a menu is open | PR-62 |
| OQ-74 | How the runner finds a softlock | PR-15 |
| OQ-79 | How the screen-test job pins Mesa | PR-41 |
| OQ-80 | The count of bot runs on each PR | PR-15 |
| OQ-81 | How the night gate result stays current | PR-49 |
| OQ-82 | The time of the night | PR-49 |
| OQ-83 | How CI gets the Godot editor and the templates, resolved by D-596 | PR-54 |
| OQ-84 | The seeds of the night | PR-49 |
| OQ-86 | How the atlas places tiles, and how Game draws a map | PR-7 |
| OQ-89 | Pixel snap in Game | PR-7 |
| OQ-91 | The operations of a large picture on a piece | PR-55 |
| OQ-94 | How the budget test counts one view | PR-56 |
| OQ-95 | Where a torch light comes from | PR-56 |
| OQ-96 | Where the shape of a shadow comes from | PR-56 |
| OQ-97 | The colors of light | PR-56 |
| OQ-98 | GPU particles or CPU particles | PR-57 |
| OQ-99 | What a screen shake moves | PR-57 |
| OQ-100 | The reduced form of a flash and a shake | PR-57 and PR-63 |
| OQ-101 | How fog keeps an enemy visible | PR-58 |
| OQ-102 | How glow stays off sprites | PR-59 |
| OQ-103 | Where shader code lives | PR-10 and PR-60 |
| OQ-104 | The font settings and the load from bytes | PR-61 |
| OQ-106 | Where the settings file lives, and its form | PR-63 |
| OQ-107 | How Game knows the last device of the player | PR-61 |
| OQ-108 | Where a remap lives, and what a conflict does | PR-63 |
| OQ-109 | The dead zone of a stick | PR-63 |
| OQ-110 | The cursor rules of a menu | PR-62 |
| OQ-111 | The scale of the dungeon map screen | PR-62 |
| OQ-112 | The text speeds and the type-out of the dialogue box | PR-36 |
| OQ-113 | The notice log | PR-62 |
| OQ-114 | The rule of sight for the party and a patrol | PR-7 and PR-8 |
| OQ-115 | How a large enemy holds its tiles and sorts | PR-8 |
| OQ-117 | A diagonal step on the map | PR-7 |
| OQ-118 | The limits of the camera on a small map | PR-7 |
| OQ-119 | What a trap does, and what a Theft drill does to it | PR-64 |
| OQ-120 | The hazards of region one | PR-64 |
| OQ-121 | The prices, the buy-back, and the stock of a shop | PR-65 |
| OQ-122 | The format of the region map, and the cost of a route | PR-35 |
| OQ-124 | A defend action | PR-9 |
| OQ-125 | How many turns the timeline strip shows | PR-9 and PR-10 |
| OQ-126 | Where the delay of each action lives | PR-9 |
| OQ-127 | The tie-break of two equal scores | PR-11 |
| OQ-128 | What makes a profile unable to act | PR-11 |
| OQ-129 | The chance of a steal, and the cost of a failure | PR-11 |
| OQ-131 | How the screen shows the health of an enemy | PR-10 |
| OQ-132 | A group larger than its rows | PR-9 and PR-11 |
| OQ-133 | The flee chance and the grace time | PR-9 |
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

No open question blocks this file.
