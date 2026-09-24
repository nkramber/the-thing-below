# Area roadmap: Story

Status: **active focused area roadmap, which PR #11 merged on 2026-09-16.** This file says how the game tells its story, and it names the PR that builds each part (D-144, D-485). The phase files give each PR its scope, its exit tests, and its review focus. This file cites each decision by its id and never restates it. It supersedes no earlier file. Written 2026-09-16 in ASD-STE100.

The design doc holds the system map (section 3), the cost model (section 4), and the guardrails (section 6). The file `area-core.md` holds the content reader, the snapshot, and the state hash. The file `area-ui-input.md` holds the screen that draws the dialogue box, and `area-art.md` holds the portraits. The file `area-exploration.md` holds the maps that a story scene plays on, and `area-progression.md` holds the side aptitude that a personal task unlocks. The file `area-tools.md` holds the screenplay tool, and `area-audio.md` holds the cue of a story scene.

The world files hold the story itself. The file `docs/world/arc.md` holds the arc of region one, `docs/world/cast.md` the five characters, and `docs/world/places.md` the places. The file `docs/world/banned-devices.md` holds the shapes that no story scene copies (D-123, D-140).

External facts: this file needs none. Core runs every story scene after D-540, and the facts of `area-core.md` bind the code that carries it (G-1, T-7). The Godot facts that the dialogue box and the sprite moves need live in `area-ui-input.md` and `area-effects.md`.

Text rules: this file follows ASD-STE100 (D-10). Game text has its own voice, and the `game-text-style` skill holds it (D-11, D-63). Tables are exempt from sentence-length counts.

## 1. Thesis

The story is fixed, and the player shapes a small part of it. A choice sets a story flag, and a story scene, a route, a hub line, or a quest reads that flag (D-329, D-350). No relationship value and no faction reputation exist (D-328). The text volume is the largest cost of the game (D-40). Every string sits in the string table, and every batch takes the approval of the owner (D-57, G-7, G-20).

Core runs each story scene, and Game draws it (D-540). That choice makes a story scene a rule, not a picture. The bots of PR-15 play every story scene, and the night of PR-49 reaches every part of region one. A replay reproduces each flag that a story scene sets (G-5, T-7).

The order of the area follows dependency. PR-68 lands first with the story scene format, the story scene runner, the flag set, and the condition form (D-544). A map, a route, a hub, and a quest all read a condition. PR-36 then draws the story scene, and PR-50 prints each story scene as a screenplay for the owner.

PR-18 adds the branches and the choice effects, and PR-19 adds the quests and the personal tasks. PR-28 and PR-29 write the arc of region one.

## 5. Findings that bind this area

The register in section 5 of `docs/design.md` holds every finding. These rows bind the story area.

| # | Finding | Binds |
|---|---|---|
| F-21 | No text linked an arc to the main story of the game | PR-28 and PR-29: region one is the prologue, and every plotline converges at the end (D-131, D-133) |
| F-22 | Three recorded answers sat close to the plot devices of one game | Every story scene: the banned list of `docs/world/banned-devices.md` (D-136, D-140) |
| F-28 | A choice of the player could remove a cast member | PR-18: a lost ally is a faction or a person outside the cast (D-301) |
| F-29 | Four dungeon ids covered three dungeons | PR-28 and PR-29: the second visit to the hanging cells is a story beat (D-327) |
| F-55 | A story scene step sets a flag in Phase 2, and PR-18 defined the flags in Phase 3 | PR-68: the flag set and the condition form land with the story scene runner (D-544) |

## 7. Roadmap

Each part below says how one part of the story works, which decisions set it, and which PR builds it. The phase files give the scope and the exit tests of each PR (D-144). Each part ends with a plain-English paragraph for a reader who does not know the code.

### 7.1 The story scene format

Built by PR-68. Phase file: `phase-2-first-playable.md`.

- A story scene is a JSON list of steps, such as move, face, wait, say, choose, and set flag (D-173). Each step that shows text points at a string id (G-7).
- The story scene files are rule files, so a change to one changes the content hash (D-495).
- A join step adds a cast member to the party, and it changes the party state in the snapshot (D-342, D-563).
- A story scene names no track and no cue. The audio file names the story scene and the step that its cue serves (D-548, `area-audio.md`).
- A story scene names no art. An art file names the content ids that it draws, as D-519 asks.
- The schema validates each story scene at load, and an absent field is an error (G-6, T-2). A step that names an absent string id, flag id, cast member, or enemy group fails with the story scene, the step, and the id.
- A script holds eleven kinds of step, and a start battle step names an enemy group (D-997, D-998). A pick of a choose step sets the flag of its option (D-1007).
- A step acts on the lead, or on a cast member that a show step put on a marker. Each shown cast member leaves at the end of the story scene (D-1006).

> *In plain English:* a story scene is a list of simple steps in a data file. It says walk here, face there, say this line, and ask this question. Nothing about it is code.

### 7.2 The story scene runner

Built by PR-68. Phase file: `phase-2-first-playable.md`.

- Core runs the story scene script and holds the step index and every flag that a step sets (D-540). Game draws each step.
- Game sends a wait intent when a move, a face, or a line ends (D-493, D-522). An effect that the world waits for uses the same intent.
- A bot answers each wait intent at once, so the bots and the night gate play every story scene (D-64, G-22).
- The story scene state joins the snapshot, the state hash, and the migration set (G-5, D-166).
- Core never reads a clock, so the length of a step comes from content or from the wait intent (G-3). A wait step names its ticks, and every other step that Game animates ends on the wait intent (D-1000, D-1013).
- The map holds still while a story scene runs. The start button pauses the story scene, and PR-36 draws the pause screen (D-1009, D-1010).
- Property tests over one thousand seeds prove that a replay of a run with story scenes gives the same end-state hash (T-3, G-5).
- D-114 put the runner in the engine layer, and D-540 revises that part. The engine layer still draws each step.

> *In plain English:* the rules of the game decide what happens in a story scene, and the screen only shows it. That way a robot can play a story scene, and a replay always gives the same result.

### 7.3 Story flags and conditions

Built by PR-68, and used by PR-18, PR-14, PR-35, and PR-19. Phase file: `phase-2-first-playable.md`.

- A story flag is a name that is on or off, and Core holds the set of the flags that are on (D-542).
- The flag file `content/rules/flags.json` declares every flag id with one line of prose (D-1003). A load fails on an id that the file does not declare (T-2).
- One condition form serves every reader (D-543). The readers are a story scene step, a route of the region map, a hub line, a quest, and an enemy group.
- Core validates each condition at load against the declared ids, so one parser, one test, and one error message cover every reader (T-1, T-2).
- PR-68 takes the flag set and the condition form, because a story scene step sets a flag and PR-18 lands in Phase 3 (D-544, F-55).
- A flag id is permanent, and the snapshot of the prologue carries each flag into the full game (D-163, D-166).
- No count and no value hide in a flag, because D-329 removed the relationship value and the faction reputation.
- A condition is a tree of all, any, and not over flag leaves. The always leaf marks a thing that no flag gates, so no reader holds an absent condition (D-1001, D-1002).

> *In plain English:* what the game remembers about your choices is a list of names that are on. Every part of the game reads that list the same way.

### 7.4 The story scene on screen

Built by PR-36, on the base of PR-61. Phase file: `phase-2-first-playable.md`.

- The dialogue box sits at the bottom, with the portrait, a name plate, and the choices (D-109, D-114, D-223). The file `area-ui-input.md` holds the box.
- The text types out at the chosen speed, in silence (D-223). The speed and the skip are accessibility settings of PR-63 (D-214).
- Game moves a story scene sprite from the tick of Core, never from a timer of Godot, as it moves the camera (G-23, F-52).
- Game sends a choice as an intent, and Core sets the flag of the result (D-493, D-540).
- One portrait for each cast member and each named NPC, as a 64 by 64 grid (D-109, D-234, D-270). PR-36 uses fixture portraits, because PR-28 and PR-29 draw the cast.
- The player speaks the dialogue choices of the lead, and the map always follows the lead (D-267, D-292).
- OQ-150 holds the skip, and OQ-151 holds the layout of the choices.

> *In plain English:* people walk, turn, and speak on the map you already walk on. Their words appear in a box at the bottom, with a face beside them.

### 7.5 Where a story scene starts

Built by PR-68, and used by PR-7, PR-14, and PR-16. Phase file: `phase-2-first-playable.md`.

- The map file lists each story scene trigger with its condition, because one rule file holds every thing that a rule reads (D-528).
- A story scene plays on a hub map or a dungeon map, and one code path draws both (D-112, D-114, `area-exploration.md` section 7.11).
- A trigger fires from the tick of Core, so a replay starts each story scene at the same tick (T-7, G-5).
- A story scene that plays once sets a flag, and its condition then refuses it (D-542).
- The village holds a placeholder story scene until the arc content lands (D-292, PR-17).
- Four kinds of event fire a trigger: a tile, a talk with an NPC, the entry to the map, and a won battle (D-1004). A battle end trigger names a patrol of its map (D-1011). PR-14 fires the talk kind with the NPCs (D-1005).

> *In plain English:* a story scene starts because you walked somewhere, spoke to somebody, or finished a fight. The map file says where and when.

### 7.6 Branches and the choices of region one

Built by PR-18. Phase file: `phase-3-story-systems.md`.

- PR-18 adds the branch conditions in content and the choice effects (D-40, D-329).
- Four flag effects exist: a closed route, a lost ally outside the cast, a changed hub, and a new time of day (D-301, D-442).
- A choice of the player never removes a cast member. The one death in the cast is Elio, after region one (D-279, D-301, D-321).
- Region one holds two or three set choices, and PR-28 and PR-29 write each one with its story scene (D-350). The choice on the ice crossing is one of them: the party spares the beaten captain of the wardens, or kills him (D-354).
- PR-18 proves each of the four flag effects on a fixture branch.
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
- PR-12 reads a story flag of PR-68, which lands first, and PR-19 unlocks the side aptitude in play (D-538, D-556).
- The balance must hold with any side aptitude absent, and the bots test each one in turn (D-282, D-304).
- PR-28 and PR-29 write the content of each personal task (D-352).
- OQ-152 holds what a quest holds, OQ-153 the rumor board, and OQ-154 the end of a region.

> *In plain English:* a board in town lists work, and each person in the party has one private errand. Finish the errand and they show a second talent.

### 7.8 The screenplay and the approval of a batch

Built by PR-50. Phase file: `phase-2-first-playable.md`.

- The command prints each story scene script as a screenplay, with the text of each string id (D-173, G-25). The file `area-tools.md` holds the tool.
- It lands right after PR-68, because it needs the story scene format and the string table alone (D-545).
- A story scene that names an absent string id fails with the story scene, the step, and the id (T-2).
- One run prints each story scene that the PR changes, into a marked section of the PR body file (D-1015, D-1016, D-1017).
- The owner approves each text batch in its PR description, in full (D-57, G-25).
- Every player string follows the `game-text-style` skill (D-63, G-20).

> *In plain English:* a tool prints each story scene in the shape of a film script. The owner reads the story as a story before anybody builds it.

### 7.9 The arc of region one as content

Built by PR-17, PR-28, and PR-29. Phase files: `phase-2-first-playable.md` and `phase-4-region-one.md`.

- The file `docs/world/arc.md` holds the arc, step by step, and each step cites its decision (D-309 onward).
- PR-17 writes the text of Marrek, Bergit, and Dagvar and a placeholder story scene, for the first playable (D-292, D-362).
- PR-28 and PR-29 write region one in two batches (D-56, D-57, D-350, D-352). Each batch holds story scenes, set choices, portraits, personal tasks, and cast text.
- The party grows from Marrek alone to five characters, in the order that D-342 sets.
- The region ends on the ice crossing with the choice of D-354, and two months pass before region two (D-345, D-353).
- Elio is an innocent type, and the player loves him (D-322). Every story scene, line, portrait, and sprite of Elio keeps that rule.
- The open items of `docs/world/arc.md` name what region one still needs, such as the names of the bishop, the priest, and the captain.
- M-5 records the play time of the owner from the first hub to the end of the arc (D-56). The target is six to eight hours.

> *In plain English:* a world file holds the whole first part of the story. Two content changes turn it into story scenes, faces, and lines you read.

### 7.10 The limits on story content

Built by every content PR with text. Phase files: every phase file from `phase-2-first-playable.md`.

- No text and no story scene shows harm to a child directly. Text can imply that harm, or state it with no direct detail (D-335).
- A story scene can show the aftermath of violence, deaths of every kind included (D-335).
- The limit on sexual violence stands (D-126).
- The world copies no device on the list in `docs/world/banned-devices.md` (D-136, D-140, F-22).
- Region one shows the harm that the users of the thing below leave, and it never shows who they are (D-310).
- Each killing carries weight, and no story scene treats one as nothing (D-126).
- Rule 13 of the `game-text-style` skill carries these limits (D-70, D-335).

> *In plain English:* the story is grim, and it has lines it does not cross. A document holds those lines, and no session decides them by taste.

### 7.11 The story in the tests

Built by PR-68, PR-18, PR-19, PR-15, and PR-49. Phase files: `phase-2-first-playable.md` and every later phase file.

- Each rule takes a seed loop of one thousand seeds, and each failure names its seed (T-3, G-4).
- A fixture scene runs to its end, and a replay of that run gives the same state hash (G-5, D-504).
- Each PR adds its fixture run and its hash to the identity file (G-5, D-504).
- A bot answers each wait intent, so the night of PR-49 plays every story scene of region one (D-64, G-22).
- A fixture branch closes a route on the region map, and a replay reproduces the branch (the exit tests of PR-18).
- A fixture quest completes, a hub line changes with a story flag, and a finished task unlocks a side aptitude (the exit tests of PR-19).
- The bots play the fixture dungeon with each side aptitude absent in turn (D-282, D-304).
- A save from an older snapshot format loads through its migration, with a fixture save (D-166).
- Every Core change here bumps the simulation version (G-17).

> *In plain English:* robots play through every story scene and every branch thousands of times. A story that can trap a player shows up as a red job, not as a complaint.

### 7.12 Story by PR

| PR | Rules | Decisions |
|---|---|---|
| PR-68 | The story scene format, the story scene runner, the join step, the flag set, and the condition form | D-114, D-173, D-540 to D-544, D-563 |
| PR-36 | The dialogue box, the portraits, and the story scene presentation | D-109, D-114, D-223, D-541 |
| PR-50 | The screenplay tool | D-173, D-545 |
| PR-18 | The branches, the choice effects, and the lost ally | D-40, D-301, D-329 |
| PR-19 | The quest state, the rumor board, and the personal tasks | D-59, D-282, D-375, D-538 |
| PR-17 | The text of three characters and a placeholder story scene | D-292, D-362 |
| PR-28 and PR-29 | The story scenes, the choices, the joins, the portraits, the tasks, and the night light of the mining town | D-56, D-57, D-350 to D-355, D-442, D-563 |

### 7.13 Story that other area files hold

| Part | Area file | PR |
|---|---|---|
| The content reader, the snapshot, and the migrations | `area-core.md` | PR-5 and PR-43 |
| The UI base, the dialogue box, and the text speed | `area-ui-input.md` | PR-61, PR-36, and PR-63 |
| The portraits and the sprites that a story scene moves | `area-art.md` | PR-34 and PR-17 |
| The maps and the triggers that a story scene plays on | `area-exploration.md` | PR-7, PR-14, and PR-16 |
| The routes of the region map that a flag closes | `area-exploration.md` | PR-35 |
| The side aptitude that a personal task unlocks | `area-progression.md` | PR-12 |
| The boss phases with their scripted moves | `area-battle.md` | PR-20 |
| The screenplay tool and the headless runner | `area-tools.md` | PR-50 and PR-15 |
| The cue of a story scene and the main theme | `area-audio.md` | PR-70 |
| The credits roll after the last story scene of region one | `area-release.md` | PR-77 |

### 7.14 The contract of every later story PR

Each later PR that adds or changes a rule of the story keeps this list. The phase files make exit tests from it.

1. Put the rule in Core, and leave the pictures and the sound in Game (D-540, G-1, G-23).
2. Give each new flag id, story scene id, and quest id a permanent name (D-166).
3. Declare every new flag id in content, and fail the load on an id that no file declares (T-2).
4. Use the one condition form for every new reader (D-543).
5. Prove the rule with a seed loop of one thousand seeds (T-3).
6. Add the new state to the state hash, the snapshot, and its migration (G-5, D-166).
7. Bump the simulation version (G-17).
8. Keep every player string in the string table, in the voice, and put the batch in the PR description (G-7, G-20, G-25).
9. Keep the limits of section 7.10 (D-126, D-335).

> *In plain English:* every new piece of story is data that the rules read. The screen draws it, and a test proves that it plays the same way twice.

## 8. Sequence

The global order lives in section 8 of `docs/design.md`, and PR #11 set it (D-488). The story work keeps this order inside it:

1. PR-5: the content reader and the string table (`area-core.md`).
2. PR-7: the first map that a story scene can play on (`area-exploration.md`).
3. PR-68: the story scene format and runner, the join step, the flags, and the conditions, before PR-12 (D-556, D-563).
4. PR-50: the screenplay tool (D-545).
5. PR-14: the first hub, whose services read a condition of PR-68 (`area-exploration.md`).
6. PR-36: the dialogue box, the portraits, and the story scene presentation.
7. PR-35: the region map, with routes that a condition closes.
8. PR-17: the text of three characters and a placeholder story scene.
9. **← GATE 2 (first playable).**
10. PR-18: the branches, the choice effects, and the lost ally, in Phase 3.
11. PR-19: the quest state, the rumor board, and the personal tasks.
12. PR-20: the boss phases, with their scripted moves (`area-battle.md`).
13. **← GATE 3 (story systems).**
14. PR-81, PR-28, and PR-29: the sealed gallery and the arc of region one, in Phase 4 (D-562).
15. M-5: the play time of the owner through region one.
16. **← GATE 4 (region one).**

## 9. Open questions

The register is `docs/questions.md` (D-19). These questions block story PRs, and each PR asks its questions when it starts (D-487):

- OQ-144: the full step list of a story scene script. Resolved 2026-09-23 by D-997.
- OQ-145: how a step that takes time ends. Resolved 2026-09-23 by D-1000.
- OQ-146: the shape of a condition. Resolved 2026-09-23 by D-1001 and D-1002.
- OQ-147: where content declares each flag id. Resolved 2026-09-23 by D-1003.
- OQ-148: what fires a story scene trigger. Resolved 2026-09-23 by D-1004.
- OQ-149: whether a story scene step starts a battle. Resolved 2026-09-23 by D-998 and D-999.
- OQ-150: how the player skips a story scene. Blocks PR-36.
- OQ-151: how the choices lay out in the dialogue box. Blocks PR-36.
- OQ-152: what a quest holds. Blocks PR-19.
- OQ-153: what the rumor board shows. Blocks PR-19.
- OQ-154: what ends a region for a missed task. Blocks PR-19.
- OQ-155: whether a story scene can play inside a battle. Blocks PR-20.

No open question blocks this file.
