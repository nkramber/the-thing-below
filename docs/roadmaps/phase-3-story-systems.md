# Phase roadmap: Phase 3, Story systems

Status: **active focused phase roadmap, which PR #11 merged on 2026-09-16.** This file gives each item of Phase 3 its scope, its exit tests, its review focus, and its questions (D-144, D-485, D-487). The area files say how each part works, and each entry names the area file that it cites. This file supersedes no earlier file. Written 2026-09-16 in ASD-STE100.

The design doc holds the thesis of the game, the system map (section 3), and the cost model (section 4). It also holds the guardrails (section 6) and the global order of every PR (section 8). This file cites each decision by its id and never restates it. The index of this folder is `docs/roadmaps/readme.md`.

This file states no external fact. Each external fact of Phase 3 lives in the area file that holds its part, with the date of its check.

Text rules: this file follows ASD-STE100 (D-10). Tables are exempt from sentence-length counts.

## 1. Thesis

Phase 3 teaches the game to remember a choice and to answer it. Phase 2 gave the story scene runner, the story flags, and one condition form (D-544). Phase 3 builds the readers of those flags: the branches, the quests, and the tasks that open a hidden talent.

Two other systems close the phase, because each one needs the parts of Phase 2 and no content of Phase 4. They are the boss phases over the evaluator, and the puzzles and secrets of a dungeon.

Phase 3 is short: four PRs. It writes systems, not content. The content of region one comes in Phase 4, and each PR here ships with a fixture that proves its rule (T-3).

Gate 3 asks one play: a branch that closes a route, and a hub that changes with an earlier choice (D-329).

## 5. Findings that bind this phase

The register in section 5 of `docs/design.md` holds every finding. These rows bind an item of Phase 3.

| # | Finding | Binds |
|---|---|---|
| F-22 | Three recorded answers sit close to the plot devices of one game | PR-18: the choice effects keep the shapes and ban the devices, and PR-28 and PR-29 write each set choice (D-136, D-140) |
| F-28 | The design critic found that a player choice could remove a cast member | PR-18: D-301 forbids it, and the four flag effects hold instead |

No other row of the register binds an item of Phase 3.

## 7. Roadmap

Each entry below gives one item of Phase 3 its scope, its exit tests, its review focus, and its questions. The area file of each entry says how the part works (D-144). Each entry ends with a plain-English paragraph for a reader who does not know the code.

An exit test is a test or a job that the PR adds and that must pass before the merge. The PR gate of `CLAUDE.md` still applies to each PR, and these tests are the ones that this PR alone can fail.

Every PR of this phase changes Core, so each one keeps the seven steps of section 7.14 of `area-core.md`.

### 7.1 PR-18: the branches and the choice effects

Area file: `area-story.md` section 7.6.

**Scope.**

- The branch conditions in content, on the one condition form of PR-68 (D-40, D-329, D-543).
- The four flag effects: a closed route, a lost ally outside the cast, a changed hub, and a new time of day (D-301, D-442).
- The fixture branches that prove each of the four flag effects.
- The flags of the prologue in the save, which region two reads (D-163, D-353).

**Out of scope.**

- The flag set and the condition form, which PR-68 built (D-544).
- The quest state and the personal tasks (PR-19).
- The set choices of region one and the choice on the ice crossing, which PR-28 and PR-29 write with their story scenes (D-350, D-354, D-355).
- The story scene content of region one (PR-28, PR-29).

**Exit tests.**

1. A fixture branch closes a route on the region map, and the screen says why (D-113).
2. A replay of that run reproduces the branch.
3. A fixture choice changes a hub, and the hub holds the change across a save and a load.
4. A fixture choice sets a new time of day, and the light and the music follow (D-428, D-442).
5. A test proves that no choice removes a cast member (D-301).
6. The snapshot carries each flag through its migration (D-166).

**Review focus.**

- No choice effect copies a device from `docs/world/banned-devices.md`, and each set choice of PR-28 and PR-29 keeps that rule (D-136, D-140, F-22).
- The lost ally sits outside the cast, because the one death in the cast is Elio (D-279, D-321).
- Each branch reads the same condition form as a route, a hub line, and a quest (D-543, T-1).

**Questions.** None. OQ-146 settled the condition form in PR-68.

> *In plain English:* a few times in the story you decide something, and the game remembers it for good. A road closes, a town changes, or somebody lives.

### 7.2 PR-19: the quests, the rumor board, and the personal tasks

Area files: `area-story.md` section 7.7, `area-progression.md` section 7.6.

**Scope.**

- The quest state, which holds every task and which the save keeps as one list (D-538).
- The rumor board NPC in the hub (D-59).
- The personal task of each character, and the story flag that unlocks a side aptitude (D-282, D-538).
- The close of a missed task at the end of its region, and the result in the save (D-375).
- The quest window in the stack of PR-62.

**Out of scope.**

- The content of each personal task, which PR-28 and PR-29 write (D-352).
- No reputation and no relationship value, which D-329 removed.

**Exit tests.**

1. A fixture quest completes, and the quest state records it.
2. A hub line changes with a story flag (D-329).
3. A finished task unlocks a side aptitude, and the menu then shows it (D-282, D-283).
4. A missed task closes at the end of its region, and the save carries the result (D-375).
5. A task in the mining town becomes impossible at the breakout, with no notice (D-319).
6. A property test over one thousand seeds proves that the balance holds with any side aptitude absent (D-282, D-304).

**Review focus.**

- The fixture flag of PR-12 leaves for the real flag of this PR (D-538).
- The answer of OQ-152 sets what a quest holds, and OQ-154 the end of a region.
- The quest state joins the snapshot, and an older save migrates (D-166).

**Questions.** OQ-152, OQ-153, and OQ-154.

> *In plain English:* a board in town lists work, and each person in the party has one private errand. Finish the errand and they show a second talent.

### 7.3 PR-20: the boss phases and the signature moves

Area files: `area-battle.md` section 7.8, `area-story.md` section 7.2.

**Scope.**

- The scripted phase layer over the evaluator, where a phase changes the profile and adds a move (D-65).
- A fixture boss, with no content of the first playable (D-564).
- The rule that no party flees from a boss (D-378).
- What starts a phase, from the answer of OQ-130.

**Out of scope.**

- The bosses of region one, with their sprites and their backdrops, which PR-23 to PR-26 write (D-564).
- The balance of the boss numbers (PR-30).

**Exit tests.**

1. The fixture boss changes phase at the scripted threshold in every one of one thousand seeds.
2. A phase change swaps the profile, and the evaluator then scores the new move.
3. No flee starts in the boss fight (D-378).
4. The replay of a boss fight gives the same state hash on every leg.
5. A boss file that names an absent profile id fails with the file and the id.

**Review focus.**

- The answer of OQ-155 settles whether a story scene can play inside a battle.
- The phase layer never reads a clock, and it counts ticks alone (G-3).
- A boss of region one is a bandit, a church warden, or a wrong thing (D-155, D-310).

**Questions.** OQ-130 and OQ-155.

> *In plain English:* a boss does not just have more health. At set moments it changes how it thinks, and it gains a move that the player never saw.

### 7.4 PR-21: the puzzles and the secrets

Area file: `area-exploration.md` section 7.10.

**Scope.**

- The switches, the pushable blocks, the light and dark, the hidden rooms, and the secret markers (D-41).
- The state of a puzzle of light and dark in Core, which Game draws as light (D-41).
- The switches, the blocks, and the secret markers in the map file (D-528).
- How the player finds a secret, from the answer of OQ-123.

**Out of scope.**

- The puzzles of region one, which PR-23 to PR-26 write.
- The light setup of a map, which PR-56 holds (D-519).

**Exit tests.**

1. A fixture puzzle opens a door.
2. A hidden room stays hidden until the party finds it.
3. A pushable block never leaves the map, and a property test over one thousand seeds proves it.
4. The light of a puzzle comes from Core state, and a screen test captures both states (D-41).
5. The snapshot holds each switch, each block position, and each found secret.

**Review focus.**

- Game draws the puzzle light from Core state, never from its own timer (G-23).
- The answer of OQ-123 keeps a secret findable with no outside guide.
- A map file that names an absent switch id fails with the map and the id.

**Questions.** OQ-123.

> *In plain English:* dungeons hold switches, blocks to push, dark rooms, and rooms that a straight walk never finds.

### 7.5 PR-22: retired

PR-22 held jobs five to eight, which have no purpose after D-268. No later item takes the id (G-10). This entry exists so that a reader of the sequence finds the gap and its reason.

> *In plain English:* one planned change no longer exists, and its number stays empty forever, so old notes never point at new work.

### 7.6 Gate 3: the story systems

**The gate.** Gate 3 passes when every line holds:

1. The owner plays a branch that closes a route (D-329).
2. The owner plays a hub that changes with an earlier choice (D-329).
3. A finished personal task unlocks a side aptitude in play (D-282).
4. A fixture boss changes phase at its threshold in the play (D-564).
5. Every job of the PR gate is green on every leg (D-481).
6. The bots play each fixture with no crash and no softlock (D-64).

**What the gate does not ask.** No content of region one, and no sign-off on the length of the story. Gate 4 holds those (D-56).

> *In plain English:* at this point the game answers what you did. A road you closed stays closed, and a town you changed stays changed.

## 8. Sequence

The global order lives in section 8 of `docs/design.md`, and PR #11 set it (D-488). Phase 3 holds this order:

1. PR-18: the branches and the choice effects.
2. PR-19: the quests, the rumor board, and the personal tasks.
3. PR-20: the boss phases and the signature moves.
4. PR-21: the puzzles and the secrets.
5. **← GATE 3 (story systems).** Section 7.6 holds each line.

PR-74, PR-75, and PR-76 land before this phase, right after the Gate 2 build. The file `phase-2-first-playable.md` holds them (D-550, D-551). The next phase file is `phase-4-region-one.md`.

## 9. Open questions

The register is `docs/questions.md` (D-19). These questions block an item of Phase 3, and each PR asks its questions when it starts (D-487):

| Question | Subject | Blocks |
|---|---|---|
| OQ-123 | How the player finds a secret | PR-21 |
| OQ-130 | What starts a boss phase | PR-20 |
| OQ-152 | What a quest holds | PR-19 |
| OQ-153 | What the rumor board shows | PR-19 |
| OQ-154 | What ends a region for a missed task | PR-19 |
| OQ-155 | Whether a story scene can play inside a battle | PR-20 |

No open question blocks this file.
