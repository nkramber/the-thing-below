# Area roadmap: Story

Status: **focused area roadmap, draft in PR #11.** This file says how the game tells its story, and it names the PR that builds each part (D-144, D-485). The phase files give each PR its scope, its exit tests, and its review focus. This file cites each decision by its id and never restates it. It supersedes no earlier file. Written 2026-09-16 in ASD-STE100.

The design doc holds the system map (section 3), the cost model (section 4), and the guardrails (section 6). The file `area-core.md` holds the content reader, the snapshot, and the state hash. The file `area-ui-input.md` holds the screen that draws the dialogue box, and `area-art.md` holds the portraits. The file `area-exploration.md` holds the maps that a scene plays on, and `area-progression.md` holds the side aptitude that a personal task unlocks. The file `area-tools.md` holds the screenplay tool, and `area-audio.md` holds the cue of a scene.

The world files hold the story itself. The file `docs/world/arc.md` holds the arc of region one, `docs/world/cast.md` the five characters, `docs/world/places.md` the places, and `docs/world/banned-devices.md` the shapes that no scene copies (D-123, D-140).

External facts: this file needs none. Core runs every scene after D-540, and the facts of `area-core.md` bind the code that carries it (G-1, T-7). The Godot facts that the dialogue box and the sprite moves need live in `area-ui-input.md` and `area-effects.md`.

Text rules: this file follows ASD-STE100 (D-10). Game text has its own voice, and the `game-text-style` skill holds it (D-11, D-63). Tables are exempt from sentence-length counts.

## 1. Thesis

The story is fixed, and the player shapes a small part of it. A choice sets a story flag, and a scene, a route, a hub line, or a quest reads that flag (D-329, D-350). No relationship value and no faction reputation exist (D-328). The text volume is the largest cost of the game, so every string sits in the string table and every batch takes the approval of the owner (D-40, D-57, G-7, G-20).

Core runs each scene, and Game draws it (D-540). That choice makes a scene a rule, not a picture. The bots of PR-15 play every scene, the night of PR-49 reaches every part of region one, and a replay reproduces each flag that a scene sets (G-5, T-7).

The order of the area follows dependency. PR-68 lands first with the scene format, the scene runner, the flag set, and the condition form, because a map, a route, a hub, and a quest all read a condition (D-544). PR-36 then draws the scene. PR-50 prints each scene as a screenplay for the owner. PR-18 adds the branches and the choice effects, and PR-19 adds the quests and the personal tasks. PR-28 and PR-29 write the arc of region one.

## 5. Findings that bind this area

The register in section 5 of `docs/design.md` holds every finding. These rows bind the story area.

| # | Finding | Binds |
|---|---|---|
| F-21 | No text linked an arc to the main story of the game | PR-28 and PR-29: region one is the prologue, and every plotline converges at the end (D-131, D-133) |
| F-22 | Three recorded answers sat close to the plot devices of one game | Every scene: the banned list of `docs/world/banned-devices.md` (D-136, D-140) |
| F-28 | A choice of the player could remove a cast member | PR-18: a lost ally is a faction or a person outside the cast (D-301) |
| F-29 | Four dungeon ids covered three dungeons | PR-28 and PR-29: the second visit to the hanging cells is a story beat (D-327) |
| F-55 | A scene step sets a flag in Phase 2, and PR-18 defined the flags in Phase 3 | PR-68: the flag set and the condition form land with the scene runner (D-544) |

## 7. Roadmap

Each part below says how one part of the story works, which decisions set it, and which PR builds it. The phase files give the scope and the exit tests of each PR (D-144). Each part ends with a plain-English paragraph for a reader who does not know the code.

### 7.1 The scene format

Built by PR-68. Phase file: `phase-2-first-playable.md`.

- A scene is a JSON list of steps, such as move, face, wait, say, choose, and set flag (D-173). Each step that shows text points at a string id (G-7).
- The scene files are rule files, so a change to one changes the content hash (D-495).
- A scene names its cue when the music must change, and the audio file names no scene (D-418, `area-audio.md`).
- A scene names no art. An art file names the content ids that it draws, as D-519 asks.
- The schema validates each scene at load, and an absent field is an error (G-6, T-2). A step that names an absent string id, flag id, or sprite id fails with the scene, the step, and the id.
- OQ-144 holds the full step list, and OQ-149 holds a step that starts a battle.

> *In plain English:* a scene is a list of simple steps in a data file: walk here, face there, say this line, ask this question. Nothing about it is code.

### 7.2 The scene runner

Built by PR-68. Phase file: `phase-2-first-playable.md`.

- Core runs the scene script and holds the step index and every flag that a step sets (D-540). Game draws each step.
- Game sends a wait intent when a move, a face, or a line ends, as it does for an effect that the world waits for (D-522, D-493).
- A bot answers each wait intent at once, so the bots and the night gate play every scene (D-64, G-22).
- The scene state joins the snapshot, the state hash, and the migration set (G-5, D-166).
- Core never reads a clock, so the length of a step comes from content or from the wait intent (G-3). OQ-145 holds which.
- Property tests over one thousand seeds prove that a replay of a run with scenes gives the same end-state hash (T-3, G-5).
- D-114 put the runner in the engine layer, and D-540 revises that part. The engine layer still draws each step.

> *In plain English:* the rules of the game decide what happens in a scene, and the screen only shows it. That way a robot can play a scene, and a replay always gives the same result.

### 7.3 Story flags and conditions

Built by PR-68, and used by PR-18, PR-14, PR-35, and PR-19. Phase file: `phase-2-first-playable.md`.

- A story flag is a name that is on or off, and Core holds the set of the flags that are on (D-542).
- Content declares every flag id, and a load fails on an id that no file declares (T-2). OQ-147 holds where the declaration lives.
- One condition form serves every reader: a scene step, a route of the region map, a hub line, a quest, and an enemy group (D-543).
- Core validates each condition at load against the declared ids, so one parser, one test, and one error message cover every reader (T-1, T-2).
- PR-68 takes the flag set and the condition form, because a scene step sets a flag and PR-18 lands in Phase 3 (D-544, F-55).
- A flag id is permanent, and the snapshot of the prologue carries each flag into the full game (D-163, D-166).
- No count and no value hide in a flag, because D-329 removed the relationship value and the faction reputation.
- OQ-146 holds the shape of a condition.

> *In plain English:* what the game remembers about your choices is a list of names that are on. Every part of the game reads that list the same way.

### 7.4 The scene on screen

Built by PR-36, on the base of PR-61. Phase file: `phase-2-first-playable.md`.

- The dialogue box sits at the bottom, with the portrait, a name plate, and the choices (D-109, D-114, D-223). The file `area-ui-input.md` holds the box.
- The text types out at the chosen speed, in silence (D-223). The speed and the skip are accessibility settings of PR-63 (D-214).
- Game moves a scene sprite from the tick of Core, never from a timer of Godot, as it moves the camera (G-23, F-52).
- Game sends a choice as an intent, and Core sets the flag of the result (D-493, D-540).
- One portrait for each cast member and each named NPC, as a 64 by 64 grid (D-109, D-234, D-270). PR-36 uses fixture portraits, because PR-28 and PR-29 draw the cast.
- The player speaks the dialogue choices of the lead, and the map always follows the lead (D-267, D-292).
- OQ-150 holds the skip, and OQ-151 holds the layout of the choices.

> *In plain English:* people walk, turn, and speak on the map you already walk on. Their words appear in a box at the bottom, with a face beside them.

### 7.5 Where a scene starts

Built by PR-68, and used by PR-7, PR-14, and PR-16. Phase file: `phase-2-first-playable.md`.

- The map file lists each scene trigger with its condition, because one rule file holds every thing that a rule reads (D-528).
- A scene plays on a hub map or a dungeon map, and one code path draws both (D-112, D-114, `area-exploration.md` section 7.11).
- A trigger fires from the tick of Core, so a replay starts each scene at the same tick (T-7, G-5).
- A scene that plays once sets a flag, and its condition then refuses it (D-542).
- The village holds a placeholder scene until the arc content lands (D-292, PR-17).
- OQ-148 holds what fires a trigger: a tile, a talk, the entry to a map, or a fight that ends.

> *In plain English:* a scene starts because you walked somewhere, spoke to somebody, or finished a fight. The map file says where and when.

### 7.6 Branches and the choices of region one

Built by PR-18. Phase file: `phase-3-story-systems.md`.

- PR-18 adds the branch conditions in content and the choice effects (D-40, D-329).
- Four flag effects exist: a closed route, a lost ally outside the cast, a changed hub, and a new time of day (D-301, D-442).
- A choice of the player never removes a cast member. The one death in the cast is Elio, after region one (D-279, D-301, D-321).
- Region one holds two or three set choices (D-350). The choice on the ice crossing is set: the party spares the beaten captain of the wardens, or kills him (D-354).
- PR-28 and PR-29 propose one or two more set choices for the approval of the owner (D-355).
- The save of the prologue carries each flag, and region two reads them (D-163, D-353).
- The gate of Phase 3 is a branch that closes a route and a hub that changes with an earlier choice (D-329).

> *In plain English:* a few times in the story you decide something, and the game remembers it for good. A road closes, a town changes, or somebody lives.

### 7.7 Quests, the rumor board, and the personal tasks

Built by PR-19. Phase file: `phase-3-story-systems.md`.

- PR-19 adds the quest state and the rumor board NPC in the hub (D-59).
- The quest state holds every task, the personal tasks included, and the save keeps one list (D-538).
- Each character hides a side aptitude until a personal task unlocks it, and the side aptitude reads a story flag (D-282, D-538).
- The menu shows an empty mark before the unlock (D-283). The file `area-progression.md` holds the aptitudes.
- A missed task closes when its region ends, and the save carries the result (D-375). A task in the mining town becomes impossible at the breakout, with no notice (D-319).
- PR-12 ships with the side aptitude behind a fixture flag, because PR-19 comes later (D-538, T-3).
- The balance must hold with any side aptitude absent, and the bots test each one in turn (D-282, D-304).
- PR-28 and PR-29 write the content of each personal task (D-352).
- OQ-152 holds what a quest holds, OQ-153 the rumor board, and OQ-154 the end of a region.

> *In plain English:* a board in town lists work, and each person in the party has one private errand. Finish the errand and they show a second talent.

### 7.8 The screenplay and the approval of a batch

Built by PR-50. Phase file: `phase-2-first-playable.md`.

- The command prints each scene script as a screenplay, with the text of each string id (D-173, G-25). The file `area-tools.md` holds the tool.
- It lands right after PR-68, because it needs the scene format and the string table alone (D-545).
- A scene that names an absent string id fails with the scene, the step, and the id (T-2).
- The owner approves each text batch in its PR description, in full (D-57, G-25).
- Every player string follows the `game-text-style` skill (D-63, G-20).

> *In plain English:* a tool prints each scene the way a script is printed, so the owner reads the story as a story before anybody builds it.

### 7.9 The arc of region one as content

Built by PR-17, PR-28, and PR-29. Phase files: `phase-2-first-playable.md` and `phase-4-region-one.md`.

- The file `docs/world/arc.md` holds the arc, step by step, and each step cites its decision (D-309 onward).
- PR-17 writes the text of Marrek, Bergit, and Dagvar and a placeholder scene, for the first playable (D-292, D-362).
- PR-28 and PR-29 write the scenes, the set choices, the portraits, the personal tasks, and the cast text of region one, in two batches (D-56, D-57, D-350, D-352).
- The party grows from Marrek alone to five characters, in the order that D-342 sets.
- The region ends on the ice crossing with the choice of D-354, and two months pass before region two (D-345, D-353).
- Elio is an innocent type, and the player loves him (D-322). Every scene, line, portrait, and sprite of Elio keeps that rule.
- The open items of `docs/world/arc.md` name what region one still needs, such as the names of the bishop, the priest, and the captain.
- M-5 records the play time of the owner from the first hub to the end of the arc, against the six to eight hours of D-56.

> *In plain English:* the whole first part of the story is written down in a world file. Two content changes turn it into scenes, faces, and lines you read.

### 7.10 The limits on story content

Built by every content PR with text. Phase files: every phase file from `phase-2-first-playable.md`.

- No text and no scene shows harm to a child directly. Text can imply that harm, or state it with no direct detail (D-335).
- A scene can show the aftermath of violence, deaths of every kind included (D-335).
- The limit on sexual violence stands (D-126).
- The world copies no device on the list in `docs/world/banned-devices.md` (D-136, D-140, F-22).
- Region one shows the harm that the users of the thing below leave, and it never shows who they are (D-310).
- Each killing carries weight, and no scene treats one as nothing (D-126).
- Rule 13 of the `game-text-style` skill carries these limits (D-70, D-335).

> *In plain English:* the story is grim, and it has lines it does not cross. The rules for those lines are written down, not left to taste.

### 7.11 The story in the tests

Built by PR-68, PR-18, PR-19, PR-15, and PR-49. Phase files: `phase-2-first-playable.md` and every later phase file.

- Each rule takes a seed loop of one thousand seeds, and each failure names its seed (T-3, G-4).
- A fixture scene runs to its end, and a replay of that run gives the same state hash (G-5, D-504).
- Each PR adds its fixture run and its hash to the identity file (G-5, D-504).
- A bot answers each wait intent, so the night of PR-49 plays every scene of region one (D-64, G-22).
- A fixture branch closes a route on the region map, and a replay reproduces the branch (PR-18 gate).
- A fixture quest completes, a hub line changes with a story flag, and a finished task unlocks a side aptitude (PR-19 gate).
- The bots play the fixture dungeon with each side aptitude absent in turn (D-282, D-304).
- A save from an older snapshot format loads through its migration, with a fixture save (D-166).
- Every Core change here bumps the simulation version (G-17).

> *In plain English:* robots play through every scene and every branch thousands of times. A story that can trap a player shows up as a red job, not as a complaint.

### 7.12 Story by PR

| PR | Rules | Decisions |
|---|---|---|
| PR-68 | The scene format, the scene runner, the flag set, and the condition form | D-114, D-173, D-540 to D-544 |
| PR-36 | The dialogue box, the portraits, and the scene presentation | D-109, D-114, D-223, D-541 |
| PR-50 | The screenplay tool | D-173, D-545 |
| PR-18 | The branches, the choice effects, and the lost ally | D-40, D-301, D-329 |
| PR-19 | The quest state, the rumor board, and the personal tasks | D-59, D-282, D-375, D-538 |
| PR-17 | The text of three characters and a placeholder scene | D-292, D-362 |
| PR-28 and PR-29 | The scenes, the choices, the portraits, and the tasks of region one | D-56, D-57, D-350 to D-355 |

### 7.13 Story that other area files hold

| Part | Area file | PR |
|---|---|---|
| The content reader, the snapshot, and the migrations | `area-core.md` | PR-5 and PR-43 |
| The UI base, the dialogue box, and the text speed | `area-ui-input.md` | PR-61, PR-36, and PR-63 |
| The portraits and the sprites that a scene moves | `area-art.md` | PR-34 and PR-17 |
| The maps and the triggers that a scene plays on | `area-exploration.md` | PR-7, PR-14, and PR-16 |
| The routes of the region map that a flag closes | `area-exploration.md` | PR-35 |
| The side aptitude that a personal task unlocks | `area-progression.md` | PR-12 |
| The boss phases with their scripted moves | `area-battle.md` | PR-20 |
| The screenplay tool and the headless runner | `area-tools.md` | PR-50 and PR-15 |
| The cue of a scene and the main theme | `area-audio.md` | The PRs that `area-audio.md` names |
| The credits roll after the last scene of region one | `area-release.md` | The PR that `area-release.md` names |

### 7.14 The contract of every later story PR

Each later PR that adds or changes a rule of the story keeps this list. The phase files make exit tests from it.

1. Put the rule in Core, and leave the pictures and the sound in Game (D-540, G-1, G-23).
2. Give each new flag id, scene id, and quest id a permanent name (D-166).
3. Declare every new flag id in content, and fail the load on an id that no file declares (T-2).
4. Use the one condition form for every new reader (D-543).
5. Prove the rule with a seed loop of one thousand seeds (T-3).
6. Add the new state to the state hash, the snapshot, and its migration (G-5, D-166).
7. Bump the simulation version (G-17).
8. Keep every player string in the string table, in the voice, and put the batch in the PR description (G-7, G-20, G-25).
9. Keep the limits of section 7.10 (D-126, D-335).

> *In plain English:* every new piece of story is data that the rules read. The screen draws it, and a test proves that it plays the same way twice.

## 8. Sequence

The global order lives in section 8 of `docs/design.md`, and the rebuild of PR #11 sets it (D-488). The story work keeps this order inside it:

1. PR-5: the content reader and the string table (`area-core.md`).
2. PR-7: the first map that a scene can play on (`area-exploration.md`).
3. PR-14: the first hub, with its NPCs (`area-exploration.md`).
4. PR-68: the scene format, the scene runner, the flag set, and the condition form (D-541, D-544).
5. PR-50: the screenplay tool (D-545).
6. PR-36: the dialogue box, the portraits, and the scene presentation.
7. PR-35: the region map, with routes that a condition closes.
8. PR-17: the text of three characters and a placeholder scene.
9. **← GATE 2 (first playable).**
10. PR-18: the branches, the choice effects, and the lost ally, in Phase 3.
11. PR-19: the quest state, the rumor board, and the personal tasks.
12. PR-20: the boss phases, with their scripted moves (`area-battle.md`).
13. **← GATE 3 (story systems).**
14. PR-28 and PR-29: the arc of region one, in Phase 4.
15. M-5: the play time of the owner through region one.
16. **← GATE 4 (region one).**

## 9. Open questions

The register is `docs/questions.md` (D-19). These questions block story PRs, and each PR asks its questions when it starts (D-487):

- OQ-144: the full step list of a scene script. Blocks PR-68.
- OQ-145: how a step that takes time ends. Blocks PR-68.
- OQ-146: the shape of a condition. Blocks PR-68.
- OQ-147: where content declares each flag id. Blocks PR-68.
- OQ-148: what fires a scene trigger. Blocks PR-68.
- OQ-149: whether a scene step starts a battle. Blocks PR-68.
- OQ-150: how the player skips a scene. Blocks PR-36.
- OQ-151: how the choices lay out in the dialogue box. Blocks PR-36.
- OQ-152: what a quest holds. Blocks PR-19.
- OQ-153: what the rumor board shows. Blocks PR-19.
- OQ-154: what ends a region for a missed task. Blocks PR-19.
- OQ-155: whether a scene can play inside a battle. Blocks PR-20.

No open question blocks this file.
