# Area roadmap: Exploration

Status: **focused area roadmap, draft in PR #11.** This file says how the maps and the places of the game work, and it names the PR that builds each part (D-144, D-485). The phase files give each PR its scope, its exit tests, and its review focus. This file cites each decision by its id and never restates it. It supersedes no earlier file. Written 2026-09-16 in ASD-STE100.

The design doc holds the system map (section 3), the cost model (section 4), and the guardrails (section 6). The file `area-core.md` holds the tick, the intents, and the snapshot, and `area-battle.md` holds the fight that an encounter starts. The file `area-ui-input.md` holds the frame, the camera input, the map HUD, and the menus. The file `area-effects.md` holds the light setup of each map, and `area-art.md` holds the tile sets. The file `area-tools.md` holds the map preview and the tile-edge tool.

External facts, each with the date of its check. The Godot facts come from `godotengine/godot` at the tag `4.7.2-stable`:

- `TileSet.tile_size` and `TileSetAtlasSource.texture_region_size` both have the default `Vector2i(16, 16)`. Sources: `doc/classes/TileSet.xml` and `doc/classes/TileSetAtlasSource.xml`, read 2026-09-16.
- `TileMapLayer.collision_enabled`, `navigation_enabled`, and `occlusion_enabled` all have the default `true`. The coordinates of a layer "are limited to 16-bit signed integers", from `-32768` to `32767`. Source: `doc/classes/TileMapLayer.xml`, read 2026-09-16.
- In a `TileSetAtlasSource`, "Each tile in the grid must be exposed using [method create_tile]", and that method returns no value. Source: `doc/classes/TileSetAtlasSource.xml`, read 2026-09-16.
- With `y_sort_enabled`, "this and child [CanvasItem] nodes with a higher Y position are rendered in front of nodes with a lower Y position". Also: "Nodes sort relative to each other only if they are on the same [member z_index]." Source: `doc/classes/CanvasItem.xml`, read 2026-09-16.
- A tile takes one sort value. The code adds the center of its cell, the `y_sort_origin` of the tile, and the same value of the layer. The tile value is the "Vertical point of the tile used for determining y-sorted order". Sources: `scene/2d/tile_map_layer.cpp` line 552 and `doc/classes/TileData.xml`, read 2026-09-16.
- A `Camera2D` has the limits `-10000000` and `10000000`, `limit_enabled` `true`, the anchor mode 1, and `position_smoothing_enabled` `false`. Source: `doc/classes/Camera2D.xml`, read 2026-09-16.
- When the limit rectangle is smaller than the view, the camera centers the view. The code reads: "Split the difference horizontally (center it)". No page of the docs states this. Source: `scene/2d/camera_2d.cpp` lines 231 to 241, read 2026-09-16.
- The same function carries a FIXME: "smoothing is not currently applied only once per frame / tick, therefore ... which will result in some haphazard results". The class text adds that the position of the node "doesn't represent the actual position of the screen, which may differ due to applied smoothing or limits". Sources: `scene/2d/camera_2d.cpp` lines 194 to 198 and `doc/classes/Camera2D.xml`, read 2026-09-16.
- Outside the editor, the camera reads the size of the view from the visible rect of its viewport. Source: `scene/2d/camera_2d.cpp` lines 745 to 751, read 2026-09-16.

Text rules: this file follows ASD-STE100 (D-10). Tables are exempt from sentence-length counts.

## 1. Thesis

Exploration is the part of the game between the fights. The party walks a tile map, sees each enemy before it fights, and opens what a dungeon holds (D-37, D-41, D-106). Core holds every rule: the tiles, the movement, the sight, the patrols, and the dungeon parts. Game draws the map and slides each step across the ticks of that step (D-203). One rule file holds each map, and the generated files beside it never reach the content hash (D-495, D-528).

The order of the area follows the first screen. PR-7 builds the map, the movement, the sight, and the map scene. PR-8 adds the enemies that walk it, and the battle PRs follow. The dungeon parts split across PR-16 and PR-64, the hub across PR-14 and PR-65, and the region map lands in PR-35 (D-529, D-530). The puzzles and the secrets of PR-21 close the area in Phase 3.

## 5. Findings that bind this area

The register in section 5 of `docs/design.md` holds every finding. These rows bind the exploration area.

| # | Finding | Binds |
|---|---|---|
| F-6 | D-39 took seeded variation on a replay premise that D-46 removed | Every map: the author places each thing by hand (D-47) |
| F-7 | A fallen character stays down until a hub, and three fight | PR-16: the reserve and the swap at a save point (D-58) |
| F-23 | `--headless` draws nothing | PR-41 and PR-7: the map scene meets a screen test (D-172) |
| F-46 | Godot drops a light past 15 on one canvas item with no message | PR-56: a map layer draws 256 tiles as one canvas item |
| F-51 | Four Godot defaults meet the tile map | PR-7: the tile size, the region size, and the two switches |
| F-52 | The camera centers a small map, and no doc states it | PR-7: a test locks it, and the tick moves the camera |

## 7. Roadmap

Each part below says how one part of exploration works, which decisions set it, and which PR builds it. The phase files give the scope and the exit tests of each PR (D-144). Each part ends with a plain-English paragraph for a reader who does not know the code.

### 7.1 What one map is made of

Built by PR-7, and kept by every content PR. Phase file: `phase-2-first-playable.md`.

- One rule file holds each map (D-528). It holds the terrain rows as characters, like a drawing file (D-165, D-515).
- The same file holds every thing that a rule reads (D-41, D-528). Those are the doors, the locks, the chests, the traps, the save points, the spawn points, and the markers (D-386).
- The file also holds the time of day of the map, which the story can change with a flag (D-442).
- Three files sit beside it and stay out of the content hash (D-495). They are the edge file of PR-53, the light setup of PR-56, and the art of D-519 (D-501).
- A thing that sits on a tile of the wrong kind fails the load, with the map, the position, and the kind (T-2).
- The author writes each map by hand, and no seed changes it (D-39, D-47).
- The map preview of PR-52 draws the map as a PNG for the owner, with the edge tiles of PR-53 (D-165, D-204).

> *In plain English:* each place is one text file: the ground as rows of letters, and lists of the doors, chests, and traps on it. A tool draws it as a picture, so the owner approves the place before anybody walks it.

### 7.2 The tile map and movement

Built by PR-7. Phase file: `phase-2-first-playable.md`.

- Core holds the tile map, the movement, and the sight, and it keeps every position on a whole tile (D-100, D-106).
- A step takes a fixed count of ticks, and Game slides the sprite across them (D-164, D-203).
- The map runs in real time, and the patrols walk on the tick whether or not the player moves (D-162).
- A menu pauses the world (D-162). OQ-64 holds what the tick does while a menu is open.
- The lead walks the map in every case, in the party or in the reserve (D-292, D-306).
- OQ-117 holds whether a step can go diagonally.
- A door, a lock, a chest, or a save point answers the step into it. The intent names the thing by its id (D-493).

> *In plain English:* the party moves one tile at a time, and the screen slides it smoothly between tiles. The rules only ever see whole tiles, so a replay always lands on the same square.

### 7.3 Drawing a map in Game

Built by PR-7. Phase file: `phase-2-first-playable.md`.

- Game draws the tiles of the map from the atlas, with the edge tiles of the edge file (D-501, `area-art.md` section 7.7).
- OQ-86 holds whether Game draws a map through a `TileMapLayer` of Godot or draws each tile itself.
- With a `TileMapLayer`, PR-7 sets the tile size and the region size to 32, because both Godot defaults are 16 (F-51).
- PR-7 also turns off the collisions and the navigation of each layer, because no rule reads them (F-51, G-1, G-23).
- Godot sorts each canvas item by one Y value, and a tile takes the center of its cell (the external facts above).
- A sprite of more than one tile needs its sort value at the front row of its body (D-206). OQ-115 holds the rule.
- A layer holds coordinates from `-32768` to `32767`, which every map of the game fits (the external facts above).
- The fog draws over each tile that the party never saw (section 7.5).

> *In plain English:* the ground, the walls, and the borders come from one packed image. Each figure draws in front of what is behind it, so a character can walk behind a pillar.

### 7.4 The camera

Built by PR-7. Phase file: `phase-2-first-playable.md`.

- The camera follows the lead, and it never scrolls past the edge of a map larger than the view (D-106, PR-7 in `docs/design.md`).
- A map smaller than the view sits centered. Godot centers it, and no page of the docs states that, so a test locks the behavior (F-52).
- Game moves the camera from the tick of the step, never from the smoothing of Godot (D-203, F-52). That smoothing can run more than once in a frame.
- The camera reads the size of the view from the viewport, so both views of D-480 work with one rule (the external facts above).
- The camera lives in Game and never reaches Core (G-23, D-106).
- OQ-118 holds whether Godot or Game computes the limits of a small map. OQ-89 holds the pixel snap of each sprite.

> *In plain English:* the view follows the party, and it stops at the edge of the place. A place smaller than the screen sits in the middle. The game moves the view itself, so every replay shows the same picture.

### 7.5 Sight, fog, and the time of day

Built by PR-7. Phase file: `phase-2-first-playable.md`.

- Core computes what the party sees, and the fog covers each tile that the party never saw (PR-7 in `docs/design.md`).
- A patrol sees the party by its own sight, and a wall stops it (D-37, PR-8 gate).
- OQ-114 holds the rule of sight, and OQ-116 holds what the fog remembers between visits.
- The time of day of the map sets the sight range, the routes, and the enemies, and the story sets the time (D-193, D-442).
- The wrong things keep no time rule, and the story places each one (D-446).
- A story flag can change the time of day while the party stands on the map. The light and the music then change on the spot (D-428, D-442).
- The light of the screen never reaches a rule of sight (G-1, `area-effects.md` section 7.1).

> *In plain English:* the party knows the tiles that it saw, and the rest of the map stays dark. The story says whether a place is in daylight or at night, and that changes what walks there.

### 7.6 Enemies on the map

Built by PR-8. Phase file: `phase-2-first-playable.md`.

- Enemies stand or walk their routes on the map, and no encounter is random (D-37).
- A patrol that sees the party shows a mark for a beat, and then the encounter starts (D-208).
- Whoever reaches the other from behind acts first in the fight (D-265).
- An enemy that moves has three views with a two-frame walk, and one that stands flips (D-207).
- An elite holds two by two tiles, and a boss three by three, on the map as in battle (D-206, D-236).
- A large enemy keeps its place inside its own area, and a load proves that its body fits everywhere in that area (D-209, T-2).
- After a flee, the group returns to its route, and no battle with it starts for a short grace time (D-381).
- Property tests over one thousand seeds prove that a patrol never leaves its route and never sees through a wall (PR-8 gate).

> *In plain English:* you see every enemy before it sees you. Sneak past it, take it from behind for the first blow, or walk away and it goes back to its rounds.

### 7.7 The encounter hand-off

Built by PR-8, PR-9, and PR-60. Phase file: `phase-2-first-playable.md`.

- One run holds the map state and the battle state together (D-531).
- While a battle runs, no map system ticks. The patrols, the poison on the map, and the grace time all stand still (D-531).
- The party returns to the tile that it left, and one state hash covers the map and the battle (D-531, G-5).
- A test proves that no map system moves during a battle (D-531, T-3).
- The transition of PR-60 plays over the hand-off, and the kind of the encounter picks it (D-196).
- After the battle, the map waits for the screen, and a wait intent ends the wait (D-522).
- A killed enemy stays dead until the party leaves the dungeon (D-257).
- A party wipe reloads the newer of the slot save and the autosave (D-231).

> *In plain English:* the map freezes while a fight runs, so nothing sneaks up during the fight. When the fight ends, the party stands exactly where it was.

### 7.8 Dungeon parts

Built by PR-16. Phase file: `phase-2-first-playable.md`.

- PR-16 builds the treasure, the locked doors, the keys, and the save points (D-41, D-529).
- A save point saves, swaps the party, and swaps the lessons (D-36, D-58, D-356).
- A save point restores MP once for each visit, and no health (D-257, D-389).
- A Theft drill on one of the three who fight opens a lock that the map marks as pickable. A story lock always needs its key (D-386).
- A chest over the stack limit keeps what the party cannot carry, and the save records what remains (D-385).
- The exit of the dungeon returns the party to the region map (PR-16 in `docs/design.md`).
- A bot run that wipes reloads and continues, and a two-character party can still reach the exit (PR-16 gate).

> *In plain English:* dungeons gain their chests, doors, keys, and resting stones. A thief can pick some locks, and the story keeps its own doors shut until you find the key.

### 7.9 Traps, hazards, and statuses on the map

Built by PR-64. Phase file: `phase-2-first-playable.md`.

- PR-64 builds the traps and the hazards of D-41, apart from the parts of PR-16 (D-529).
- A Theft drill reveals and disarms a trap (D-386). OQ-119 holds what each trap does.
- OQ-120 holds which hazards region one holds.
- Poison, blind, and silence last past a battle, until a cure or a rest at a hub (D-390).
- Poison ticks on the map and can down a character (D-392). Blind does nothing outside battle, and silence stops a rite from the menu (D-393).
- When poison downs all three who fight, the party wipes, even with a healthy reserve (D-397).
- Property tests over one thousand seeds prove each rule, and the bots play the maps of the first playable (D-64).

> *In plain English:* the dungeon itself can hurt you. Poison still hurts while you walk, and a party can go down between fights.

### 7.10 Puzzles and secrets

Built by PR-21. Phase file: `phase-3-story-systems.md`.

- PR-21 builds the switches, the pushable blocks, the light and dark, the hidden rooms, and the secret markers (D-41).
- A puzzle of light and dark keeps its state in Core, and Game draws the light from that state (D-41, `area-effects.md` section 7.1).
- The map file holds each switch, each block, and each secret marker (D-528).
- OQ-123 holds how the player finds a secret.
- A fixture puzzle opens a door, and a hidden room stays hidden until the party finds it (PR-21 gate).

> *In plain English:* dungeons hold switches, blocks to push, dark rooms, and rooms that a straight walk never finds.

### 7.11 Hubs and services

Built by PR-14. Phase file: `phase-2-first-playable.md`.

- A hub is a walkable map with NPC sprites, and one code path draws a hub and a dungeon (D-112).
- PR-14 builds the hub map, the NPCs, the rest, the save, and the party swap (D-59, D-530).
- A rest restores health and MP, and it cures poison, blind, and silence (D-42, D-390).
- Each hub has a shape of its own, so no hub offers every service (D-28, D-59).
- The hanging cells are a dungeon under a hub, and the same map rules cover it (D-244, D-112).
- The village is a start area with no shop and no rest (D-369).
- A scene can play on a hub map or a dungeon map, and `area-story.md` holds the scene runner (D-114).

> *In plain English:* a hub is a place you walk through, with people to talk to, a bed, and a stone to save at. Every hub has a different shape.

### 7.12 The shop and the gold

Built by PR-65. Phase file: `phase-2-first-playable.md`.

- PR-65 builds the shop and the gold economy, after the items of PR-13 (D-530).
- Gold comes from enemies and from treasure, and it buys gear, items, and rest (D-60).
- A shop sells lessons too, and the people of the story teach or give others (D-365).
- The shops of the mining town close to the party at the breakout, and the refuge opens only during the flight (D-319, D-331, D-365).
- OQ-121 holds the prices, the buy-back, and the stock of a shop.
- The balance pass of PR-30 tunes the numbers of the economy (D-60, G-14).

> *In plain English:* every fight pays a little, and the gold buys gear, supplies, and a bed. Some shops close for good when the story turns.

### 7.13 The region map

Built by PR-35. Phase file: `phase-2-first-playable.md`.

- The region map is a screen of nodes and routes, and the party moves node to node (D-113).
- A route opens and closes with a story flag (D-40, D-113, D-329).
- The autosave writes on each arrival at a node (D-224).
- No clock runs, so a route costs no time (D-442). OQ-122 holds the format and the cost of a route.
- The region map plays one track, and it shows no sign of night (D-430, D-445).
- The layout of region one follows `docs/world/places.md` (D-250, D-255, D-371).
- A closed route refuses the move, the screen says why, and a replay reproduces the path (PR-35 gate).

> *In plain English:* between places the party travels on a map of the region, along roads that the story opens and closes.

### 7.14 Maps by PR

| PR | Maps and rules | Decisions |
|---|---|---|
| PR-7 | The map format, the movement, the sight, the fog, the camera, and a fixture dungeon | D-106, D-165, D-528 |
| PR-8 | The enemies on the map, the patrols, the sight mark, and the grace time | D-37, D-208, D-381 |
| PR-16 | The treasure, the doors, the keys, and the save points | D-41, D-529 |
| PR-64 | The traps, the hazards, and the statuses that last on the map | D-390 to D-393, D-529 |
| PR-14 | The hub map, the NPCs, the rest, the save, and the party swap | D-59, D-112, D-530 |
| PR-65 | The shop and the gold economy | D-60, D-530 |
| PR-35 | The region map, its nodes, and its routes | D-113 |
| PR-21 | The switches, the blocks, the light and dark, and the secrets | D-41 |
| PR-17 | The village, the land near it, the mining town, and the hanging cells | D-313, D-362, D-369 |
| PR-23 to PR-27 | The deep mine, the second visit to the cells, the border fort, the ice crossing, and the second hub | D-313, D-327 |

### 7.15 Exploration that other area files hold

| Part | Area file | PR |
|---|---|---|
| The tick, the intents, the snapshot, and the save | `area-core.md` | PR-6 and PR-43 |
| The battle that an encounter starts | `area-battle.md` | PR-9 |
| The frame, the camera input, the map HUD, and the dungeon map screen | `area-ui-input.md` | PR-61 and PR-62 |
| The light setup and the ambient effects of each map | `area-effects.md` | PR-56 and PR-58 |
| The tile sets, the edge tiles, and the map preview | `area-art.md` and `area-tools.md` | PR-17, PR-52, and PR-53 |
| The scenes that play on a map | `area-story.md` | PR-36 |
| The lessons, the gear, and the items that a chest holds | `area-progression.md` | PR-12 and PR-13 |

### 7.16 The contract of every later exploration PR

Each later PR that adds a map rule or a place keeps this list. The phase files make exit tests from it.

1. Put every rule in Core, and keep each position on a whole tile (D-100, D-106).
2. Hold each place in one rule file, with its things and its time of day (D-528).
3. Keep the generated files and the art out of the content hash (D-495, D-501).
4. Stop every map system while a battle runs (D-531).
5. Prove each rule with a seed loop of one thousand seeds (T-3, G-4).
6. Draw the map from the atlas, with no physics and no navigation (F-51, G-23).
7. Add the new state to the state hash, the snapshot, and the identity file (G-5, `area-core.md` section 7.14).

> *In plain English:* every new place and every new rule follows the same seven steps. The rules stay in one place, each map is one file, and every change proves itself over a thousand runs.

## 8. Sequence

The global order lives in section 8 of `docs/design.md`, and the rebuild of PR #11 sets it (D-488). The exploration work keeps this order inside it:

1. PR-61: the UI base (`area-ui-input.md`).
2. PR-7: the map format, the movement, the sight, the fog, the camera, and the map scene.
3. PR-41: the screen test of the map scene.
4. PR-8: the enemies on the map.
5. PR-9 and PR-10: the battle and its screen (`area-battle.md`).
6. PR-62: the menu windows and the dungeon map screen.
7. PR-12 and PR-13: the lessons and the items that a chest gives.
8. PR-14: the hub map and its services.
9. PR-65: the shop and the gold economy, after PR-13 (D-530).
10. PR-16: the treasure, the doors, the keys, and the save points.
11. PR-64: the traps, the hazards, and the statuses on the map (D-529).
12. PR-35: the region map.
13. PR-52 and PR-53: the map preview and the tile-edge tool, before PR-17 (D-497).
14. PR-17: the village, the town, and the first dungeons.
15. **← GATE 2 (first playable).**
16. PR-21: the puzzles and the secrets, in Phase 3.
17. PR-23 to PR-27: the other places of region one, in Phase 4.

## 9. Open questions

The register is `docs/questions.md` (D-19). These questions block exploration PRs, and each PR asks its questions when it starts (D-487):

- OQ-114: the rule of sight for the party and a patrol. Blocks PR-7 and PR-8.
- OQ-115: how a large enemy holds its tiles and sorts on screen. Blocks PR-8.
- OQ-116: what the fog remembers, and where it lives. Blocks PR-7 and PR-43.
- OQ-117: a diagonal step on the map. Blocks PR-7.
- OQ-118: the limits of the camera on a map smaller than the view. Blocks PR-7.
- OQ-119: what a trap does, and what a Theft drill does to it. Blocks PR-64.
- OQ-120: the hazards of region one. Blocks PR-64.
- OQ-121: the prices, the buy-back, and the stock of a shop. Blocks PR-65.
- OQ-122: the format of the region map, and the cost of a route. Blocks PR-35.
- OQ-123: how the player finds a secret. Blocks PR-21.
- OQ-86: how the atlas places tiles, and how Game draws a map. Blocks PR-34 and PR-7.
- OQ-89: pixel snap in Game. Blocks PR-7.
- OQ-64: the tick while a menu is open. Blocks PR-6.

No open question blocks this file.
