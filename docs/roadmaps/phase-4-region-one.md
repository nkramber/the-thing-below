# Phase roadmap: Phase 4, Region one content

Status: **focused phase roadmap, draft in PR #11.** This file gives each item of Phase 4 its scope, its exit tests, its review focus, and its questions (D-144, D-485, D-487). The area files say how each part works, and each entry names the area file that it cites. This file supersedes no earlier file. Written 2026-09-16 in ASD-STE100.

The design doc holds the thesis of the game, the system map (section 3), and the cost model (section 4). It also holds the guardrails (section 6) and the global order of every PR (section 8). This file cites each decision by its id and never restates it. The index of this folder is `docs/roadmaps/readme.md`.

This file states no external fact. Each external fact of Phase 4 lives in the area file that holds its part, with the date of its check.

Text rules: this file follows ASD-STE100 (D-10). Tables are exempt from sentence-length counts.

## 1. Thesis

Phase 4 writes the free prologue. Every system exists after Phase 3, so this phase adds content alone. It adds four more dungeon builds, a second hub, the lessons, the music, and the story of region one.

The order follows the order of play (D-313). A place lands before the scenes that play on it, and the lessons land before the balance pass that tunes them. The arc comes in two batches, because one batch of that size cannot take a careful review (L-1, D-57).

Two rules shape every PR of this phase. The owner approves each art batch, each text batch, and each music batch inside its own PR (D-57, G-25). No PR here changes a rule of Core. The balance pass of PR-30 is the one item that moves a number that a replay reads.

Gate 4 asks one play: the owner plays region one end to end on the desktop and on the Deck, and signs off (D-56). Then a few trusted players play the build artifacts (D-469).

## 5. Findings that bind this phase

The register in section 5 of `docs/design.md` holds every finding. These rows bind an item of Phase 4.

| # | Finding | Binds |
|---|---|---|
| F-7 | A three-character party fights short-handed after a down | PR-30: the numbers hold with a short-handed party (D-58) |
| F-21 | No text said how an arc relates to the main story | PR-28 and PR-29: one part of the main story, as a free prologue (D-131, D-133) |
| F-22 | Three recorded answers sit close to the plot devices of one game | PR-28 and PR-29: the banned devices of `docs/world/banned-devices.md` |
| F-29 | Four ids held three dungeons | PR-23 to PR-26: four dungeon builds, each named in the order of play (D-313, D-327) |
| F-54 | The end of the job system left the stats with no source | PR-30: the eight stat curves against the M-4 band (D-537) |

## 7. Roadmap

Each entry below gives one item of Phase 4 its scope, its exit tests, its review focus, and its questions. The area file of each entry says how the part works (D-144). Each entry ends with a plain-English paragraph for a reader who does not know the code.

An exit test is a test or a job that the PR adds and that must pass before the merge. The PR gate of `CLAUDE.md` still applies to each PR, and these tests are the ones that this PR alone can fail.

Every content PR of this phase keeps one list. It loads, it draws, it sounds, it reads in the voice, and the bots play it with no crash and no softlock (D-64, G-20, G-25).

### 7.1 PR-23 to PR-26: the dungeons two to four, and the return to the cells

Area files: `area-exploration.md` sections 7.1 and 7.8, `area-art.md` sections 7.3 to 7.5, `area-battle.md` section 7.7.

One content PR for each dungeon build, in the order of play (D-313). PR-23 is the deep mine. PR-24 is the second visit to the hanging cells (D-327, F-29). PR-25 is the border fort. PR-26 is the ice crossing.

**Scope of each PR.**

- The tile set of the place, and the edge rules that its terrain needs (D-110, D-204).
- The map files, with their doors, locks, chests, traps, save points, and secret markers (D-528).
- The enemies with their sprites, their profiles, and their groups (D-535).
- One boss with its sprite, its phases, and its backdrop (D-65, D-205).
- The treasure, the puzzles, and the secrets of the place (D-41).
- The light setup of each map, at its time of day (D-442, D-519).
- The ambient effects and the ambience of the place (D-187, D-424).
- The normal map of each new drawing (D-183, D-521).

**Out of scope of each PR.**

- The scenes that play on the place, which PR-28 and PR-29 write (D-352).
- The lessons that the place holds, which PR-42 writes (D-304).
- The music of the place, which PR-73 holds (D-549).

**Exit tests.**

1. Every content file of the PR loads, and no id is absent.
2. The map preview and the review sheets reach the PR description (D-514, G-25).
3. The budget test passes for each map and each battle place (D-523).
4. The bots play each map with no crash and no softlock (D-64).
5. The boss changes phase at its threshold in one thousand seeds (PR-20).
6. Each edge file matches its map and the edge rules (D-501).
7. Every string comes from the string table, in the voice (G-7, G-20).

**Review focus.**

- The place follows `docs/world/places.md` (D-250, D-255, D-371).
- PR-24 is a second visit, so it reuses the tile set of the first and changes the state (D-327).
- The owner approves each art batch from its review sheets (D-514, G-25).
- The bots reach every chest, every door, and the exit of the place.

**Questions.** None. Each system question closed before Phase 4.

> *In plain English:* four more places to walk and fight in, each with its own ground, its own enemies, and one boss. The story that happens in them comes later.

### 7.2 PR-27: the second hub

Area file: `area-exploration.md` section 7.11.

**Scope.**

- A hub of another shape than the first, with a different set of services (D-28, D-59).
- Its tile set, its map, its NPC sprites, and its shop stock (D-112, D-365).
- Its light setup, its ambient effects, and its ambience (D-424, D-519).
- The scene triggers of the hub, with their conditions (D-528, D-543).

**Out of scope.**

- The scenes themselves (PR-28, PR-29).
- The rumor board content, which the quests of PR-19 and the arc PRs fill.

**Exit tests.**

1. A fixture party walks the hub and uses each service that the hub offers.
2. A service that the hub lacks never appears on its screen (D-59).
3. The map preview and the review sheets reach the PR description (D-514).
4. The bots play the hub with no crash and no softlock (D-64).
5. Every string comes from the string table, in the voice (G-7, G-20).

**Review focus.**

- The hub differs in shape from the first hub, so no hub offers every service (D-28, D-59).
- The place follows `docs/world/places.md` (D-250, D-371).

**Questions.** None.

> *In plain English:* a second town, built on a different plan from the first. What it offers and what it lacks are part of the story.

### 7.3 PR-42: the lessons of region one

Area file: `area-progression.md` section 7.7.

**Scope.**

- The rites and drills of region one as content, across the eight kinds (D-275, D-281, D-304).
- The icons of each lesson, and its text in the voice (G-20).
- The named forms of each lesson, and the point total that opens each form (D-539).
- A place for every lesson: a dungeon, a hub, or a scene (D-304, D-365).

**Out of scope.**

- The lesson system itself, which PR-12 built.
- The balance of the numbers (PR-30, G-14).

**Exit tests.**

1. Every lesson has a place in a dungeon, a hub, or a scene (D-304).
2. The eight kinds all reach region one, across the five characters (D-293).
3. Bot runs of region one with each side aptitude absent in turn stay inside the M-4 band (D-282).
4. A lesson that names an absent icon or string id fails the load with its id.
5. The owner approves the text batch and the icon batch in the PR (D-57, G-25).

**Review focus.**

- A shop can sell a second copy of a lesson, so content plans for copies (D-361, D-365).
- No lesson copies itself, so the loot table stays finite (D-45, D-357).
- The party never gets a license or a stamp, so every rite that it uses breaks the law (D-366).

**Questions.** None.

> *In plain English:* every rite and drill that the first region offers, with its picture and its words. Each one sits in a chest, a shop, or the hands of a person.

### 7.4 PR-73: the rest of the music and the sounds of region one

Area file: `area-audio.md` section 7.12.

**Scope.**

- The tracks of the places that Phase 4 adds (D-419, D-549).
- The night version of the mining town, for the night pass of the flight (D-333, D-443).
- The remaining themes of region one, to ten in all (D-419).
- The sounds that the new places and the new abilities need (D-425, D-426).

**Out of scope.**

- The tracks of the first playable, which PR-72 holds (D-549).
- The tracks of later regions (D-299, D-419).

**Exit tests.**

1. Each render matches its hash on every CI leg (D-432).
2. Each audio file names the content ids that it serves, and no id is absent (D-548).
3. The night version crossfades at the phrase end of its day version (D-428).
4. The owner approves the batch in the PR description, after a listen (D-57, D-433, G-25).
5. Region one holds about 20 tracks and ten themes (D-419, D-444).

**Review focus.**

- Each track borrows a theme, and the PR description names which (D-419).
- A music batch touches no rule file, so it never changes the content hash (D-495).
- The sound room of PR-71 plays each new track in place (D-439).

**Questions.** None. OQ-167 closed the list of the first playable in PR-72.

> *In plain English:* the second and last batch of music for the first region, with a night version of the mining town for the escape.

### 7.5 PR-28 and PR-29: the arc of region one

Area files: `area-story.md` sections 7.9 and 7.10, `area-progression.md` sections 7.6 and 7.10.

Two content PRs hold the story of region one, in two batches (D-56, D-57, D-350).

**Scope of the two PRs.**

- The scenes of the arc, which follow `docs/world/arc.md` step by step (D-309 onward).
- The set choices of region one, two or three in all, and one or two more that these PRs propose (D-350, D-355).
- The portrait of each cast member and each named NPC, as a 64 by 64 grid (D-109, D-234).
- The personal task of each character, which these PRs propose for approval (D-282, D-352).
- The cast text of the five characters, in the voice (D-342, G-20).
- The open items of `docs/world/arc.md`, such as the names of the bishop, the priest, and the captain.

**Out of scope.**

- The credits roll (PR-77, D-552).
- The death of Elio, which falls after region one (D-321).
- The balance of the numbers (PR-30).

**Exit tests.**

1. The screenplay tool prints each scene, and the owner approves each batch (D-173, G-25).
2. Each scene loads, and no string id, flag id, or sprite id is absent.
3. A replay of a run through each scene gives the same end-state hash (G-5).
4. The bots play every scene, because each wait intent answers at once (D-540, G-22).
5. The party grows from Marrek alone to five characters, in the order of D-342.
6. Each proposed personal task and each proposed set choice reaches the owner in its PR (D-352, D-355).

**Review focus.**

- No scene copies a device from `docs/world/banned-devices.md` (D-136, D-140, F-22).
- No text and no scene shows harm to a child directly (D-335).
- Each killing carries weight, and no scene treats one as nothing (D-126).
- Elio is an innocent type, and every line, portrait, and sprite of him keeps that rule (D-322).
- The region ends on the ice crossing with the choice of D-354 (D-345, D-353).

**Questions.** None. The open items of `docs/world/arc.md` come to the owner inside these PRs (D-352, D-355).

> *In plain English:* a world file holds the whole first part of the story. Two content changes turn it into scenes, faces, and lines that the player reads.

### 7.6 PR-77: the credits roll

Area file: `area-release.md` section 7.5.

**Scope.**

- The credits roll as a scene, which plays after the last scene of region one (D-467, D-552).
- The text of the roll, its timing, and the license notices (D-467).
- The main theme under the roll (D-427).
- The place right after PR-29, so the owner sees the roll at Gate 4 (D-552).

**Out of scope.**

- The credits screen of the title menu (PR-33, D-552).
- The license files in each export, which PR-54 already carries (D-467).

**Exit tests.**

1. The roll plays after the last scene of region one, and the player can skip it (D-214).
2. The roll names the studio, and no agent, harness, or model (D-450, T-6).
3. The Godot notice and each font notice reach the player (D-467).
4. Every line of the roll comes from the string table (G-7).
5. A screen test captures the roll in both views.

**Review focus.**

- The answer of OQ-57 gives the studio name, and the roll waits for it (D-450).
- Each OFL font needs its notice and its license with every copy (D-263, D-467).
- The roll is a scene, so the scene runner of PR-68 plays it (D-540).

**Questions.** OQ-57.

> *In plain English:* the game says who made it and which free tools it uses, at the end of the first story. The same text also sits in a menu and in a file beside the program.

### 7.7 PR-30: the balance pass

Area files: `area-progression.md` section 7.3, `area-battle.md` section 7.6, `area-exploration.md` section 7.12.

**Scope.**

- The numbers of D-35, D-60, D-382, and D-388, tuned on the M-4 band and the night runs.
- The eight stat curves of the cast, against the M-4 band (D-537, F-54).
- The prices, the stock, and the buy-back of each shop (D-60).
- The weights of each enemy profile, where a fight reads wrong (D-65).
- A report of each number before and after (G-14).

**Out of scope.**

- No new rule and no new content. This PR moves numbers alone.
- The balance of later regions, which their own phases hold.

**Exit tests.**

1. Each changed number reaches the PR description with its value before and after (G-14).
2. The M-4 numbers land inside the band that the Gate 2 sign-off set.
3. A night of bot runs over region one ends with no crash and no softlock (D-507).
4. The numbers hold with a short-handed party after a down (D-58, F-7).
5. The numbers hold with each side aptitude absent in turn (D-282, D-304).
6. The simulation version bumps, and the identity file gains its new hashes (G-17, D-504).

**Review focus.**

- Each change rests on a measurement, not on taste (G-14).
- The reviewer reads each changed hash of the identity file (D-504).
- A fresh character from the reserve brings its own MP, and the balance holds with it (D-356).

**Questions.** None.

> *In plain English:* the last pass over every number of the first region. Robots and the owner play, and the numbers move to match what they found.

### 7.8 M-5: the play time of region one

Area file: none. The cost model in section 4 of `docs/design.md` holds the row.

**Scope.**

- M-5 records the play time of the owner from the first hub to the end of the arc (D-56).
- The target is six to eight hours.

**Out of scope.**

- No change to the game. A miss changes the content in a PR of its own.

**Exit tests.**

1. The cost model holds the play time with its date and its build.
2. The PR that answers a miss names the content that it cuts or adds.

**Review focus.** The measurement reaches the cost model before Gate 4 (G-14).

**Questions.** None.

> *In plain English:* the owner times one full play of the first region. The target is six to eight hours, and a big miss changes the content.

### 7.9 Gate 4: region one

**The gate.** Gate 4 passes when every line holds:

1. The owner plays region one end to end on the desktop and on the Deck (D-56, D-92).
2. The owner signs off, and the owner sees the credits roll at the end (D-552).
3. M-5 records a play time of six to eight hours (D-56).
4. The M-4 numbers land inside the band of the Gate 2 sign-off.
5. Every job of the PR gate is green on every leg (D-481).
6. A night of bot runs over region one ends with no crash and no softlock (D-507).
7. The budget test passes for every place of region one (D-523).

**After the gate.** A few players whom the owner picks play the CI build artifacts and send their notes outside Steam (D-469). No Steam Playtest runs. Each player installs an unsigned build with the steps of the runbook, and each build carries the license files (D-463, D-467). Their notes feed the fixes before the release.

> *In plain English:* the free part of the game is complete. The owner plays all of it, then a few trusted people play it and write back.

## 8. Sequence

The global order lives in section 8 of `docs/design.md`, and the rebuild of PR #11 sets it (D-488). Phase 4 holds this order:

1. PR-23: the deep mine.
2. PR-24: the second visit to the hanging cells (D-327).
3. PR-25: the border fort.
4. PR-26: the ice crossing.
5. PR-27: the second hub.
6. PR-42: the lessons of region one.
7. PR-73: the rest of the music and the sounds of region one (D-549).
8. PR-28: the arc, the first batch.
9. PR-29: the arc, the second batch.
10. PR-77: the credits roll, right after PR-29 (D-552).
11. PR-30: the balance pass.
12. M-5: the play time of region one.
13. **← GATE 4 (region one).** Section 7.9 holds each line.
14. The trusted players play the build artifacts (D-469).

The next phase file is `phase-5-first-release.md`.

## 9. Open questions

The register is `docs/questions.md` (D-19). These questions block an item of Phase 4, and each PR asks its questions when it starts (D-487):

| Question | Subject | Blocks |
|---|---|---|
| OQ-57 | The studio name | PR-77 |

The open items of `docs/world/arc.md` are not questions of the register. PR-28 and PR-29 propose each one to the owner inside the PR that needs it (D-352, D-355).

No open question blocks this file.
