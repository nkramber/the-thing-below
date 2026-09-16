# Area roadmap: Progression

Status: **focused area roadmap, draft in PR #11.** This file says how a character grows, and it names the PR that builds each part (D-144, D-485). The phase files give each PR its scope, its exit tests, and its review focus. This file cites each decision by its id and never restates it. It supersedes no earlier file. Written 2026-09-16 in ASD-STE100.

The design doc holds the system map (section 3), the cost model (section 4), and the guardrails (section 6). The file `area-core.md` holds the content reader, the snapshot, and the state hash. The file `area-battle.md` holds the fight that spends what a character carries. The file `area-exploration.md` holds the chests, the shops, and the save points that fill it. The file `area-story.md` holds the quests and the personal tasks, and `area-ui-input.md` holds the menus that show a build.

External facts: this file needs none. Core holds every rule of the build, and the facts of `area-core.md` bind the code that carries them (G-1, T-7).

Text rules: this file follows ASD-STE100 (D-10). Tables are exempt from sentence-length counts.

## 1. Thesis

A character grows in two ways. The character level raises the stats from a curve in content, and lessons give every ability (D-34, D-278, D-537). Any character equips any lesson, and each character does one kind of ability best (D-274, D-356). That is the build decision of the game, because no job and no gear limit holds a character back (D-268, D-374).

The order of the area follows dependency. PR-67 lands first with the character level, the experience, and MP, because every fight reads those numbers (D-536). PR-12 then adds the lesson slots, the growth, and the two aptitudes. PR-13 adds the gear and the items. PR-19 gives each personal task its state, PR-42 writes the lessons of region one, and PR-30 balances every number.

## 5. Findings that bind this area

The register in section 5 of `docs/design.md` holds every finding. These rows bind the progression area.

| # | Finding | Binds |
|---|---|---|
| F-7 | A fallen character stays down until a hub, and three fight | PR-67: half experience for the reserve and the downed (D-73, D-387) |
| F-8 | A caster with empty MP had no action | PR-67: MP and its recovery, with the basic attack of D-359 |
| F-54 | The end of the job system left the stats with no source | PR-67: a stat curve for each character (D-537) |

## 7. Roadmap

Each part below says how one part of the build works, which decisions set it, and which PR builds it. The phase files give the scope and the exit tests of each PR (D-144). Each part ends with a plain-English paragraph for a reader who does not know the code.

### 7.1 The character level and the experience

Built by PR-67. Phase file: `phase-2-first-playable.md`.

- A character gains experience from each battle won, and the character level follows it (D-34).
- A character in reserve earns half, and a downed character earns half (D-73, D-387).
- The experience from an enemy shrinks as the party outlevels it, which is a soft cap for each region (D-388). OQ-136 holds the numbers.
- A character who joins late starts at a set level in content (D-363).
- The level raises the stats through the curve of section 7.3, and it raises the lesson slots of section 7.4 (D-356).
- A level up plays its sting (D-422).
- Property tests over one thousand seeds prove that no run passes the soft cap of its region (PR-67 gate).

> *In plain English:* a fight makes each character stronger, and the people who wait or fall behind still learn a little. The same weak enemies soon give almost nothing.

### 7.2 MP and its recovery

Built by PR-67. Phase file: `phase-2-first-playable.md`.

- Each character holds MP, and a rite spends it (D-42).
- MP comes back at a hub, at a save point once for each visit, and from scarce items (D-42, D-257, D-389).
- A save point restores no health, so health stays the scarce resource inside a dungeon (D-389).
- Every character can attack with the weapon in hand, so an empty MP pool never leaves a dead turn (D-359, F-8).
- A fresh character from the reserve brings its own MP at a save point, and the balance of D-35 must hold with it (D-356).
- OQ-135 holds how much MP a save point and a rest restore.

> *In plain English:* spells run on MP, and MP is scarce until you reach a town. A caster with an empty pool can still swing a weapon.

### 7.3 The stat curve of each character

Built by PR-67. Phase file: `phase-2-first-playable.md`.

- Each character carries its own stat curve in content: the health, the MP, the attack, the defense, and the speed at each level (D-537).
- The end of the job system left the stats with no source, and D-537 closes that gap (F-54).
- The cast reads as people before any gear, so one character is tough and another is frail (D-33, D-537).
- The death of Elio costs a shape that no other character holds, and the replacement brings a curve of its own (D-270, D-321).
- Every number in a curve is an integer, and content writes each rate in basis points (D-169, G-2).
- OQ-134 holds the shape of a curve.
- PR-30 balances the eight curves against the M-4 band (G-14, D-299).

> *In plain English:* each person in the cast grows on their own line: some take blows, some cast, some move first. That difference exists before anybody equips anything.

### 7.4 Lessons, slots, and growth

Built by PR-12. Phase file: `phase-2-first-playable.md`.

- A lesson is a rite or a drill that any character equips to gain an ability (D-272, D-275, D-278).
- Lesson slots sit on the character, and the slot count grows with the character level (D-356). OQ-137 holds the count at each level.
- The player swaps lessons at a hub or at a save point, and the load before a dungeon holds for the visit (D-356).
- Every equipped lesson gains points from each battle won, used or not, and a reserve character gains half (D-357).
- Each lesson lists its named forms, and each form names the point total that opens it (D-539). A form is a new ability, not a larger number.
- The growth belongs to the character, not to the lesson. A lesson passed to a new character starts at its first form for that character, and it resumes where that character left it (D-361).
- OQ-138 holds the points of a battle, and OQ-139 holds two copies of one lesson in one party.
- No lesson ever copies itself, so the loot table stays finite (D-45, D-357).

> *In plain English:* abilities come from rites and drills that anybody can carry. Use one long enough and it opens a stronger form, and that progress belongs to the person who carried it.

### 7.5 The aptitudes

Built by PR-12. Phase file: `phase-2-first-playable.md`.

- Eight kinds cover every ability: Mend, Harm, Blight, Boon, Blade, Guard, Shot, and Theft (D-281).
- Each character has one main aptitude, and no two characters share one. A replacement can share the main aptitude of the character who died (D-274, D-303).
- A lesson of the kind of the aptitude works better, by a bonus in basis points (D-358).
- A side aptitude gives half that bonus (D-360).
- Any character uses any lesson, and the right character uses it best (D-274, D-358).
- The eight kinds all reach region one, across the five characters (D-293).

> *In plain English:* everyone can carry any rite, but each person is best at one family of them. That is what makes a party of three feel like a choice.

### 7.6 The personal task and the side aptitude

Built by PR-12 and PR-19. Phase files: `phase-2-first-playable.md` and `phase-3-story-systems.md`.

- Each character hides a side aptitude until a personal task unlocks it (D-282, D-283).
- The menu shows an empty mark before the unlock, so the player cannot plan a party around it (D-283).
- The quest state of PR-19 holds every task, and the aptitude reads a story flag (D-538).
- PR-12 ships with the side aptitude behind a fixture flag, because PR-19 comes later (D-538, T-3).
- A missed task closes when its region ends, and the save carries the result (D-375).
- The balance must hold with any side aptitude absent, and the bots test each one in turn (D-282, D-304).
- PR-28 and PR-29 write the content of each personal task (D-352).

> *In plain English:* every character hides a second talent that a personal errand unlocks. Miss the errand and the region ends without it, and the game still has to be winnable.

### 7.7 Where lessons come from

Built by PR-12, PR-16, PR-65, and PR-42. Phase files: `phase-2-first-playable.md` and `phase-4-region-one.md`.

- Three sources give lessons: the treasure of a dungeon, the shops of a hub, and the people of the story (D-365).
- The old-faith hexer and the fence of the cave community teach or give lessons (D-246, D-365).
- A shop can sell a second copy of a lesson, so content plans for copies and progress stays with the character (D-361, D-365).
- The party never gets a license or a stamp, so every rite that it uses breaks the law (D-366).
- PR-42 writes the lessons of region one across the eight kinds, with their icons and their text (D-304, G-20).
- Every lesson has a place in a dungeon, a hub, or a scene, and the gate of PR-42 proves it (D-304).

> *In plain English:* you find rites in chests, buy them in towns, and earn them from people. The party never has the papers that make using them legal.

### 7.8 Gear

Built by PR-13. Phase file: `phase-2-first-playable.md`.

- Six slots hold gear: the weapon, the shield or off-hand, the head, the body, and two accessories (D-44).
- Nothing limits what a character wears, because the aptitudes carry the difference (D-374).
- Gear is fixed and hand-authored, with a few rarity tiers, and no random affix and no crafting exist (D-45).
- OQ-140 holds what a piece of gear changes, and OQ-141 holds two accessories with one effect.
- The gear of Elio leaves the game with him (D-364).
- The screen shows each empty slot, and `area-ui-input.md` holds that screen (D-44, PR-13 gate).

> *In plain English:* six slots, and anyone can wear anything. What you find is what the author placed, so a good weapon is a real event.

### 7.9 Items and the pack

Built by PR-13. Phase file: `phase-2-first-playable.md`.

- The pack holds a small, fixed number of each item (D-382). OQ-142 holds the limit.
- A small, hand-placed set of items gets used up: MP draughts, healing, cures for statuses, and the rare revive (D-384).
- Any character can use an item on a turn, and an item restores less in a fight than outside one (D-382).
- A find over the stack limit stays in its chest, and the save records what remains (D-385).
- Mend rites and cure rites also work from the menu outside a fight, and silence stops them (D-391, D-393).
- OQ-143 holds what a rarity tier changes.
- A steal takes one entry from the list of an enemy, and `area-battle.md` holds the steal (D-383).

> *In plain English:* you carry a few of each thing, and a chest keeps what will not fit. Potions are scarce and worth less in the middle of a fight.

### 7.10 What the death of Elio costs

Built by the content of Phase 4 and later. Phase file: `phase-4-region-one.md`.

- The story kills Elio after region one, and the lessons and the gear that he carries leave the game (D-321, D-364).
- The death costs the main Mend of region one, and later content must offer lessons that take the place of his (D-270, D-364, D-394).
- The replacement can share his main aptitude, and its side aptitude differs (D-274, D-303).
- The save of the prologue carries all five characters, because the death falls after region one (D-163, D-309).
- A player who loaded Elio heavily loses a large share of the build in one scene (D-364).

> *In plain English:* when Elio dies, everything he carried goes with him. The game has to stay winnable for a player who gave him the best rites.

### 7.11 Progression in the tests

Built by PR-67, PR-12, PR-13, and PR-15. Phase files: `phase-2-first-playable.md` and every later phase file.

- Each rule takes a seed loop of one thousand seeds, and each failure names its seed (T-3, G-4).
- Each PR adds its fixture run and its hash to the identity file (G-5, D-504).
- The bots play the fixture dungeon with each side aptitude absent in turn (D-282, D-304).
- M-4 records the turns of an encounter and the downs of a dungeon, which set the numbers of the curves (M-4, D-35).
- PR-30 tunes the curves, the prices, the stack limits, and the shrink, with a number before and after (G-14).
- A save from an older snapshot format loads through its migration, with a fixture save (D-166).
- Every Core change here bumps the simulation version (G-17).

> *In plain English:* the numbers of growth are not guesses. Robots play thousands of runs, and the results set the curves.

### 7.12 Progression by PR

| PR | Rules | Decisions |
|---|---|---|
| PR-67 | The character level, the experience, the shrink, MP, and the stat curves | D-34, D-388, D-536, D-537 |
| PR-12 | The lesson slots, the growth, the forms, and the two aptitudes | D-356 to D-361, D-539 |
| PR-13 | The six gear slots, the inventory, and the items | D-44, D-45, D-382 |
| PR-19 | The quest state that holds each personal task | D-282, D-538 |
| PR-16 and PR-65 | The chests and the shops that give lessons, gear, and items | D-365, D-530 |
| PR-42 | The lessons of region one, with their icons and their text | D-304 |
| PR-28 and PR-29 | The content of each personal task | D-352 |
| PR-30 | The balance pass over every number | D-35, G-14 |

### 7.13 Progression that other area files hold

| Part | Area file | PR |
|---|---|---|
| The content reader, the snapshot, and the migrations | `area-core.md` | PR-5 and PR-43 |
| The fight that spends MP, items, and gear | `area-battle.md` | PR-9 and PR-66 |
| The chests, the save points, and the shops | `area-exploration.md` | PR-16 and PR-65 |
| The quests, the flags, and the scenes of each task | `area-story.md` | PR-68, PR-18, and PR-19 |
| The party, lesson, gear, item, and status screens | `area-ui-input.md` | PR-62 |
| The icons of the lessons, the elements, and the statuses | `area-art.md` | PR-42 |

### 7.14 The contract of every later progression PR

Each later PR that adds or changes a rule of growth keeps this list. The phase files make exit tests from it.

1. Put the rule in Core, with integer math and basis points (D-169, G-2).
2. Give each new content entry a permanent id (D-166).
3. Prove the rule with a seed loop of one thousand seeds (T-3).
4. Add the new state to the state hash, the snapshot, and its migration (G-5, D-166).
5. Bump the simulation version (G-17).
6. Keep every player string in the string table, in the voice (G-7, G-20).
7. Report the numbers before and after a balance change (G-14).

> *In plain English:* every new rule of growth is a plain function over whole numbers. A test plays it a thousand times, and a save survives the change.

## 8. Sequence

The global order lives in section 8 of `docs/design.md`, and the rebuild of PR #11 sets it (D-488). The progression work keeps this order inside it:

1. PR-9 and PR-66: the fight that reads the numbers (`area-battle.md`).
2. PR-10: the battle screen.
3. PR-11: the evaluator and the profiles.
4. PR-67: the character level, the experience, MP, and the stat curves (D-536).
5. PR-12: the lesson slots, the growth, the forms, and the aptitudes.
6. PR-13: the gear, the inventory, and the items.
7. PR-14 and PR-65: the hub and the shop that sell them (`area-exploration.md`).
8. PR-16: the chests that hold them.
9. PR-17: the lessons, the gear, and the items of the first playable.
10. M-4: the turns of an encounter and the downs of a dungeon.
11. **← GATE 2 (first playable).**
12. PR-19: the quest state and each personal task, in Phase 3.
13. PR-42: the lessons of region one, in Phase 4.
14. PR-28 and PR-29: the content of each personal task.
15. PR-30: the balance pass over every number.

## 9. Open questions

The register is `docs/questions.md` (D-19). These questions block progression PRs, and each PR asks its questions when it starts (D-487):

- OQ-134: the shape of a stat curve. Blocks PR-67.
- OQ-135: the MP that a save point and a rest restore. Blocks PR-67.
- OQ-136: the shrink of the experience of an enemy. Blocks PR-67.
- OQ-137: the lesson slots at each level. Blocks PR-12.
- OQ-138: the points that a lesson gains from a battle. Blocks PR-12.
- OQ-139: two copies of one lesson in one party. Blocks PR-12.
- OQ-140: what a piece of gear changes. Blocks PR-13.
- OQ-141: two accessories with one effect. Blocks PR-13.
- OQ-142: the stack limit of each item. Blocks PR-13.
- OQ-143: what a rarity tier changes. Blocks PR-13.

No open question blocks this file.
