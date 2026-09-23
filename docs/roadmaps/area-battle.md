# Area roadmap: Battle

Status: **active focused area roadmap, which PR #11 merged on 2026-09-16.** This file says how a fight works, and it names the PR that builds each part (D-144, D-485). The phase files give each PR its scope, its exit tests, and its review focus. This file cites each decision by its id and never restates it. It supersedes no earlier file. Written 2026-09-16 in ASD-STE100.

The design doc holds the system map (section 3), the cost model (section 4), and the guardrails (section 6). The file `area-core.md` holds the tick, the intents, the streams, and the state hash that every rule here uses. The file `area-exploration.md` holds the map that starts a fight, and `area-progression.md` holds the lessons, the gear, and the items that a fight spends. The file `area-ui-input.md` holds the battle screen, and `area-effects.md` holds the blood, the flash, and the shake.

External facts: this file needs none. Core holds every rule of a fight, and the facts of `area-core.md` bind the code that carries them (G-1, T-7).

Text rules: this file follows ASD-STE100 (D-10). Tables are exempt from sentence-length counts.

## 1. Thesis

A fight is hard for two reasons: the enemies think, and the resources run out (D-35). One to three characters stand against up to six enemies, each side in two rows (D-31, D-336, D-377). Speed sets the order on a visible timeline, and each action pushes its user back on it (D-29, D-376). Every rule lives in Core, so the same fight plays the same way for a player, a bot, and a replay (D-100, T-7).

The order of the area follows the rules. PR-9 builds the timeline, the actions, the damage, and the rows. PR-66 adds the eight elements and the ten statuses (D-533). PR-10 draws the fight, and PR-11 makes the enemies think. PR-20 gives a boss its phases in Phase 3, and PR-30 tunes every number in Phase 4.

## 5. Findings that bind this area

The register in section 5 of `docs/design.md` holds every finding. These rows bind the battle area.

| # | Finding | Binds |
|---|---|---|
| F-7 | A fallen character stays down until a hub, and three fight | PR-9: a fight with one or two characters (D-58, D-336) |
| F-8 | A caster with empty MP had no action | PR-9: a basic attack for every character (D-359) |
| F-23 | `--headless` draws nothing | PR-41 and PR-10: the battle screen meets a screen test (D-172) |
| F-53 | The evaluator of D-534 has no measurement of its cost | PR-11: the cost of a turn, before Gate 2 (G-14) |

## 7. Roadmap

Each part below says how one part of a fight works, which decisions set it, and which PR builds it. The phase files give the scope and the exit tests of each PR (D-144). Each part ends with a plain-English paragraph for a reader who does not know the code.

### 7.1 The shape of a fight

Built by PR-9. Phase file: `phase-2-first-playable.md`.

- One to three characters fight, and the player always picks them (D-31, D-336, D-351).
- Up to six enemies stand against them, from the group of the encounter (D-31, D-535).
- Each side has a front row and a back row (D-377). A character or an enemy stands in one row.
- The party window of PR-62 sets the starting row of each character, and the snapshot keeps it (D-558).
- Melee reaches the front row alone, while anyone stands in it. Shot drills and rites reach either row (D-377). A melee attack from the back row deals half damage (D-779).
- The map pauses while the fight runs, and one run holds both states (D-531).
- Whoever reached the other from behind acts first (D-265).
- The encounter ends in a win, a flee, or a wipe (D-36, D-378).

> *In plain English:* three of your people face up to six enemies, each side in two lines. The front line takes the blows, and the back line needs a bow or a spell to reach.

### 7.2 The timeline

Built by PR-9. Phase file: `phase-2-first-playable.md`.

- Each action pushes its user back on the timeline by an amount that the action and the speed of the user set (D-376).
- The strip shows the next several turns, so each choice reads before the player makes it (D-29, D-376). It shows six turns (D-756).
- A heavy action pushes further, haste shortens each push, and slow lengthens it (D-29, D-376).
- A stun pushes an enemy back on the strip (D-376).
- A fast character can act twice before a slow one, which the balance of PR-30 must hold (D-376, M-4).
- The delays count ticks, and Core computes them with integer math alone (D-164, D-169, G-2).
- Content holds the delay of each action and each item (D-757). The push is the delay times 100, divided by the speed, and haste and slow multiply it (D-768).
- On one ready tick, the higher speed acts first, then the party, then the lower slot (D-769). The side that came from behind starts at tick 0 (D-770).
- Property tests over one thousand seeds prove that the timeline never stalls (the exit tests of PR-9).

> *In plain English:* turn order is a strip across the top that you can read ahead. A heavy swing buys its power with a longer wait.

### 7.3 The actions

Built by PR-9. Phase file: `phase-2-first-playable.md`.

- Every character can attack with the weapon in hand, and no lesson gives that attack (D-359).
- A lesson gives each other ability, and `area-progression.md` holds the lessons (D-272, D-278).
- A rite costs MP, and MP comes back at a hub, at a save point, and from scarce items (D-42, D-257, D-389).
- Any character can use an item on their turn, and the use costs an action. An item restores less in a fight than outside one (D-382).
- A character can step to the other row, and the step costs a light delay (D-380).
- Any character can try to flee. The chance rises with the speed of the party, a failure costs the turn, and no party flees from a boss (D-378).
- After a flee, the group returns to its route, and no fight with it starts for a short grace time (D-381). The grace time is 300 ticks (D-748). The flee chance reads the speed gap of the two sides (D-763).
- A character can defend, which cuts the damage until the next turn of that character (D-755).

> *In plain English:* attack, use a rite or a drill, take an item, change row, or flee. A flight is always possible, never free, and never open against a boss.

### 7.4 Damage, elements, and statuses

Built by PR-66. Phase file: `phase-2-first-playable.md`.

- PR-66 adds the eight elements with weakness, resist, and absorb, and the ten statuses (D-74, D-75, D-533).
- Damage uses fixed-point integers, and content writes each rate in basis points (D-169, G-2).
- Each enemy record holds one affinity for each element, and a list of the statuses that it refuses (D-794, D-805). PR-13 gives gear the same table (D-790).
- A move holds one element or none, and a status with its chance or none (D-793, D-796). The basic attack holds neither.
- One hit takes the rate of its affinity, then the back row, the defend, and the shell cut. An absorb heals the hit, and no cut applies to it (D-795, D-809).
- A hit rolls the miss, then the hit factor, then the status chance, on the battle stream (D-807).
- Each status holds the tick of its end on the timeline, and a second copy resets the end. Haste and slow cancel each other (D-798, D-800).
- Poison, bleed, and regen act at the start of each turn of the holder. A sleeper passes its turn, and a strike wakes it. A stun pushes the next turn once (D-799, D-802, D-803, D-810).
- Blind adds a miss chance past the ceiling, silence marks the holder for PR-12, and shell cuts an elemental hit (D-804, D-806).
- A down clears every status (D-801).
- An aptitude adds a bonus to a lesson of its kind, and a side aptitude adds half. PR-12 holds the bonus (D-358, D-360, D-791).
- Holy and dark are two ways that the thing below answers, and the text never calls them proof of a god (D-158).
- Every status but poison, blind, and silence ends with its fight (D-390).
- Poison, blind, and silence last until a cure or a rest at a hub. Each character keeps them in the save, and `area-exploration.md` holds what they do on the map (D-390, D-393, D-792).
- A rite that cures an affliction belongs to Mend (D-394).
- Property tests over one thousand seeds prove each element and each status (the exit tests of PR-66).

> *In plain English:* fire, ice, and six more elements meet armor that likes or hates each one. Poison, blindness, and silence follow you out of the fight.

### 7.5 Down, wipe, and the reload

Built by PR-9. Phase file: `phase-2-first-playable.md`.

- A fallen character stays down until a hub or a rare item (D-36). A down is a battle fact, and no story line names it (D-135).
- A downed character earns half experience, as a character in reserve does (D-73, D-387).
- When every character who fights goes down, the party wipes, even with a healthy reserve (D-336, D-397).
- A wipe reloads the newer of the slot save and the autosave (D-231). With no save, the run starts again from its start (D-776).
- The wipe screen drains to dark and shows one terse line, and a press reloads (D-225).
- The wipe sting plays before the reload (D-422).
- A dungeon visit with a down runs short-handed, and the reserve waits for a save point or a hub (D-58, F-7).

> *In plain English:* a character who falls stays down until you reach a town. If all three fall, you reload, and the game does not soften that.

### 7.6 The evaluator

Built by PR-11. Phase file: `phase-2-first-playable.md`.

- The evaluator scores every legal action of an enemy by its simulated outcome: damage, kills, threat, healing, timeline shift, and row placement (D-65, D-377).
- It simulates each legal action and the strongest answer of the other side, one action ahead with one reply (D-534).
- The reply is the best action of the next character on the timeline (D-960). Each score takes the expected outcome, and no roll (D-959).
- The score adds each weight times its term (D-958). The damage term is the expected health that the action takes, and the kill term counts the expected kills in basis points.
- The heal term is the health that a heal restores. The threat term is the expected health that the reply takes, and the score subtracts it (D-960).
- The timeline term is the push of the action in ticks, and the score subtracts it. The row term counts the change of the enemies that melee cannot reach.
- The cost grows with the count of legal actions times the answers (F-53). One enemy turn takes at most 1 ms at the 95th percentile on the Steam Deck (D-961, G-14).
- The evaluator uses one seeded stream, and its order of work never changes (G-4, T-7). On a tie of two scores, the evaluator draws one action from its own stream (D-947).
- A profile reweights the terms of the score (D-65). PR-11 ships the weights alone, and the first trait comes with the first enemy that needs one (D-958).
- The evaluator lives in Core, and it never reads a screen, a clock, or an effect (G-1, G-3).
- M-3, M-4, and M-6 measure the cost in a night, in an encounter, and on the Deck (D-507, D-161).

> *In plain English:* each enemy tries every move that it can make, imagines your best answer, and picks the move that leaves it best off. That is what makes the fights hard.

### 7.7 Profiles and groups

Built by PR-11 and PR-9. Phase file: `phase-2-first-playable.md`.

- Each entry of a group names a personality profile: the weights of the score terms (D-65, D-958). Each profile has one file under `content/rules/profiles/` (D-956).
- Every profile validates at load, and a profile that can never act fails that load (G-21, T-2). A check fight against one fixture party member finds a profile with no legal action (D-948).
- Each profile carries a steal list of items and some gold, and a human enemy carries what a person carries (D-383). Each profile gives the base chance of a steal, and PR-13 builds the steal action with the Theft term (D-949, D-950).
- A group file for each region holds each enemy group: its enemies, their rows, and their profiles (D-535). Each map names its region, and the group files live under `content/rules/groups/` (D-957).
- A map names a group by its id, and a test proves that each named group exists (D-528, D-535). PR-9 holds that test on its fixture group file, and PR-11 grows the file (D-766).
- PR-11 proves the evaluator on fixture profiles, and PR-17 writes the profiles of the first playable.
- PR-80 holds the enemy record: the stats of each enemy and the ids of its abilities. PR-66 adds the element table to it (D-557).
- Each enemy has one record file, and each ability id of a record names an entry of the ability file (D-785, D-786). PR-11 gives an enemy ability its effect: a strike or a heal, with a delay, an element, and a reach (D-955).
- Each record gives the size of the body. The size of a map patrol equals the largest enemy of its group, and the load fails another size (D-754, D-788).
- A group holds up to twelve enemies. Up to six stand on the field, in any split of the two rows. An entry that waits steps in when an enemy falls (D-758 to D-762, D-778).

> *In plain English:* each kind of enemy weighs the same choices differently, so a brute and a healer act unlike each other. The groups they come in live in one file for each region.

### 7.8 Boss phases

Built by PR-20. Phase file: `phase-3-story-systems.md`.

- A scripted phase layer sits over the evaluator. A phase changes the profile and adds a move (D-65).
- PR-20 builds the phase layer on a fixture boss. The first playable holds no boss, and the bosses of region one come with PR-23 to PR-26 and PR-81 (D-564, D-575).
- No party flees from a boss (D-378).
- OQ-130 holds what starts a phase.
- The boss changes phase at the scripted threshold in every one of one thousand seeds (the exit tests of PR-20).
- A boss of region one is a bandit, a church warden, or a wrong thing, and the bishop closes the breakout (D-155, D-310, D-319).

> *In plain English:* a boss does not just have more health. At set moments it changes how it thinks, and it gains a move that the player never saw.

### 7.9 The pace of one turn

Built by PR-9 and PR-10. Phase file: `phase-2-first-playable.md`.

- Core resolves each action the moment that the intent arrives, and it emits its events (D-168, D-532).
- Game queues those events and plays them in order, with the poses, the blood, the numbers, and the hit-stop (D-186, D-213, D-532).
- Game takes the next command when the queue is empty, and no wait intent enters a fight (D-532).
- A test proves that the queue always drains, because the input gate lives in Game (T-2, D-532).
- A bot and a replay send their intents at full speed, because no effect holds the rules (D-532, D-64).
- The map waits for the screen only at the end of the fight, through the wait intent of D-522. Game sends it when the queue drains after a win or a flee.
- No rule reads the length of an effect (D-522, `area-effects.md` section 7.1).

> *In plain English:* the rules settle a blow at once, and the screen then shows it. The game waits for your next order only after the picture catches up.

### 7.10 The battle screen and its effects

Built by PR-10, PR-98, and PR-57. Phase file: `phase-2-first-playable.md`.

- The side view puts the enemies on the left and the party on the right, each side in two rows (D-111, D-377).
- The timeline strip runs across the top, and the command menu and the status sit at the bottom (D-111).
- The player picks each action, item, and target from the keyboard or the gamepad, and a pointer marks the target (D-827, D-833).
- A short bar under each enemy shows its health, with no number (D-826).
- PR-98 draws each waiting enemy at full size and darker, in one column at the left edge, behind the back row (D-951 to D-954). The load fails a group whose column is taller than the field (D-963).
- A damage number pops over its target, and one message line states the action in the game voice (D-213, G-20).
- The attack pose plays on an action, and a color flash marks a hit (D-96, D-108).
- The backdrop of the place drifts behind the fight, in the light of the time of day of the map (D-205, D-442). Every fight draws the fixture backdrop until the place art of PR-17 (D-831).
- PR-57 adds the blood, the sparks, the shake, and the hit-stop, and `area-effects.md` holds them (D-186). A heavy blow is a hit on a weakness (D-877).
- The battle track of the region plays, and a sting marks the victory (D-415, D-422).
- Every string comes from the string table through the text helper (G-7, D-499).

> *In plain English:* the fight reads at a glance: who acts next, who stands low, and what you can do. The screen work lives in the UI and effects plans.

### 7.11 Battle in the tests

Built by PR-9, PR-11, and PR-15. Phase files: `phase-2-first-playable.md` and every later phase file.

- Each rule takes a seed loop of one thousand seeds, and each failure names its seed (T-3, G-4).
- The replay-identity set gains a fixture fight from PR-9 on, and each later battle PR adds a run (G-5, D-504).
- The bots play the fixture dungeon on every CI leg, and a crash or a softlock names its seed (D-64, D-505).
- M-4 records the turns of an encounter and the downs of a dungeon, by bot policy (M-4, D-35).
- PR-30 tunes the numbers of D-35, D-60, D-382, and D-388 on the M-4 band, with a number before and after (G-14).
- PR-90 reports the win rate and the turns of each encounter, and a night fails when one leaves its band (D-822).
- A screen test captures a fixture fight (D-172).
- Every Core change here bumps the simulation version (G-17).

> *In plain English:* robots play thousands of fights to find the ones that never end, and the numbers they bring back set the difficulty.

### 7.12 Battle by PR

| PR | Rules | Decisions |
|---|---|---|
| PR-9 | The timeline, the actions, the defend, the damage, the rows, the wave, the flee, the row change, and the item use | D-376 to D-382, D-533, D-755 to D-781 |
| PR-66 | The eight elements and the ten statuses | D-74, D-75, D-390, D-533, D-790 to D-811 |
| PR-10 | The battle screen | D-111, D-213 |
| PR-11 | The evaluator, the profiles, and the groups | D-65, D-534, D-535, D-947 to D-950 |
| PR-98 | The waiting enemies on the battle screen | D-951 to D-954, D-963 |
| PR-57 | The blood, the sparks, the shake, and the hit-stop | D-186 |
| PR-20 | The boss phases and the signature moves | D-65 |
| PR-12 | The lessons and the aptitudes that a fight uses | D-272, D-358 |
| PR-13 | The gear and the items that a fight spends | D-44, D-382 |
| PR-17 | The enemies and the groups of the first places | D-313, D-362 |
| PR-90 | The balance harness, which measures each encounter | D-822 |
| PR-30 | The balance pass over every number | D-35, G-14 |

### 7.13 Battle that other area files hold

| Part | Area file | PR |
|---|---|---|
| The tick, the streams, the state hash, and the snapshot | `area-core.md` | PR-4 and PR-6 |
| The map that starts a fight, and the pause during it | `area-exploration.md` | PR-8 |
| The lessons, the aptitudes, the gear, and the items | `area-progression.md` | PR-12 and PR-13 |
| The battle screen, the timeline strip, and the command menu | `area-ui-input.md` | PR-10 and PR-61 |
| The blood, the sparks, the shake, the hit-stop, the spell flash, and the transition | `area-effects.md` | PR-57, PR-12, and PR-60 |
| The battle tracks, the stings, and the ability sounds | `area-audio.md` | PR-70 |
| The sprites of each enemy and the backdrop of each place | `area-art.md` | PR-17 and PR-55 |

### 7.14 The contract of every later battle PR

Each later PR that adds or changes a battle rule keeps this list. The phase files make exit tests from it.

1. Put the rule in Core, with integer math alone (D-100, G-2).
2. Draw each random number from the stream of the battle (G-4).
3. Prove the rule with a seed loop of one thousand seeds (T-3).
4. Add the fixture fight and its hash to the identity file (G-5, D-504).
5. Bump the simulation version (G-17).
6. Keep every player string in the string table (G-7, G-20).
7. Report the cost of a turn when the rule adds legal actions (F-53, G-14).

> *In plain English:* every new battle rule is a plain function that the robots can play a thousand times. It proves itself by seed, not by opinion.

## 8. Sequence

The global order lives in section 8 of `docs/design.md`, and PR #11 set it (D-488). The battle work keeps this order inside it:

1. PR-7 and PR-8: the map and the enemies that start a fight (`area-exploration.md`).
2. PR-9: the timeline, the actions, the damage, and the rows.
3. PR-80: the enemy record, with the stats and the ability ids (D-557).
4. PR-66: the eight elements and the ten statuses (D-533).
5. PR-10: the battle screen.
6. PR-48, PR-56, and PR-57: the normal maps, the light, and the battle effects (`area-effects.md`).
7. PR-11: the evaluator, the profiles, and the groups, with the cost of a turn (F-53).
8. PR-98: the waiting enemies at the left edge of the field (D-951 to D-954).
9. PR-12 and PR-13: the lessons, the gear, and the items that a fight uses.
10. PR-15: the bots that play the fixture dungeon.
11. PR-16 and PR-64: the dungeon parts around the fights.
12. PR-17: the enemies and the groups of the first playable.
13. M-4: the turns of an encounter and the downs of a dungeon.
14. **← GATE 2 (first playable).**
15. PR-20: the boss phases, in Phase 3.
16. PR-30: the balance pass, in Phase 4.

## 9. Open questions

The register is `docs/questions.md` (D-19). These questions block battle PRs, and each PR asks its questions when it starts (D-487):

- OQ-124: a defend action. Resolved by D-755.
- OQ-125: how many turns the timeline strip shows. Resolved by D-756.
- OQ-126: where the delay of each action lives. Resolved by D-757.
- OQ-127: the tie-break of two equal scores. Resolved by D-947.
- OQ-128: what makes a profile unable to act. Resolved by D-948.
- OQ-129: the chance of a steal, and the cost of a failure. Resolved by D-949 and D-950.
- OQ-130: what starts a boss phase. Blocks PR-20.
- OQ-131: how the screen shows the health of an enemy. Resolved by D-826.
- OQ-132: a group larger than its rows. Resolved by D-758.
- OQ-133: the flee chance and the grace time. Resolved by D-748 and D-763.
- OQ-242: the waiting enemies of a fight. Resolved by D-951.
- OQ-243: a column of the waiting enemies, taller than the field. Resolved by D-963.

No open question blocks this file.
