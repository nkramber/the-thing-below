---
name: ste-writing
description: Write and review text in ASD-STE100 Simplified Technical English. Load before you write any .md, skill, or agent file in this repo. Holds the project glossary, one term per concept.
---

# STE writing skill

Use this skill before you write text in this repo. The owner requires ASD-STE100 for every doc, skill, and agent file (D-10). Game text that the player reads is exempt and has its own voice (D-11).

Source: ASD-STE100 Issue 8 (2021-04-30), Part 1, Writing rules. Issue 9 (2025-01) supersedes it with the same 53 rules. The full standard is free at https://www.asd-ste100.org/. This skill gives the 53 rules in short form. It does not copy the dictionary.

## Procedure

1. Write the text.
2. Check each sentence against the checklist below.
3. Correct each sentence that fails.
4. Run the checker on the file.
5. Read the text again as a reader who does not know the subject.

## Checklist (the rules that fail most often)

- Max 20 words in a procedural sentence. Max 25 words in a descriptive sentence (5.1, 6.3).
- One instruction per sentence (5.2).
- Instructions in the imperative: "Load the file." Not "The file should be loaded." (5.3).
- Active voice in procedures. Active voice as much as possible in descriptions (3.6).
- No "-ing" verb forms. "Sync the data" not "Syncing the data". The rules permit an "-ing" word only in a technical name (3.5).
- No helping verbs for complex tenses: "we did", not "we have been doing" (3.4).
- Tenses allowed: infinitive, imperative, simple present, simple past, past participle as adjective, future (3.2).
- No semicolons (8.1).
- No contractions (4.2).
- Max three words in a noun cluster. Write longer names in full, then use hyphens or a short name (2.1, 2.2).
- Use "the", "a", "this" before nouns (2.3).
- One term per concept. Do not use synonyms for variety (1.11, 9.4).
- Each paragraph: one topic, max six sentences (6.5, 6.6).
- Use vertical lists for complex content (4.3).
- Start a safety note with the risk word: WARNING, CAUTION (7.1).
- Notes give information, not instructions (5.5).
- American English spelling (1.14).
- No phrasal verbs: "remove" not "take out" (9.3).
- Do not use a technical name as a verb (1.7). Write "make a backup", not "backup the data".

## The 53 rules in short form

### Section 1 - Words
- 1.1 Use only approved dictionary words, technical names, and technical verbs.
- 1.2 Use approved words only as the part of speech given.
- 1.3 Use approved words only with their approved meaning.
- 1.4 Use only approved forms of verbs and adjectives.
- 1.5 You can use words that fit a technical name category.
- 1.6 Use an unapproved word only when it is a technical name or part of one.
- 1.7 Do not use technical names as verbs.
- 1.8 Use technical names that agree with approved nomenclature.
- 1.9 Select technical names that are short and easy to understand.
- 1.10 Do not use slang or jargon as technical names.
- 1.11 Do not use different technical names for the same item.
- 1.12 You can use verbs that fit a technical verb category.
- 1.13 Do not use technical verbs as nouns.
- 1.14 Use American English spelling, unless an official directive says otherwise.

### Section 2 - Noun clusters
- 2.1 Write noun clusters of max three words.
- 2.2 Write a long technical name in full, then give a short name or use hyphens.
- 2.3 Use an article or demonstrative adjective before a noun.

### Section 3 - Verbs
- 3.1 Use only verb forms given in the dictionary.
- 3.2 Make only: infinitive, imperative, simple present, simple past, past participle as adjective, future.
- 3.3 Use the past participle only as an adjective.
- 3.4 Do not use helping verbs to make complex verb structures.
- 3.5 Use the "-ing" form only as a technical name or in a technical name.
- 3.6 Use the active voice in procedures. Use it as much as possible in descriptions.
- 3.7 Use an approved verb to describe an action, not a noun.

### Section 4 - Sentences
- 4.1 Write short and clear sentences.
- 4.2 Do not omit words or use contractions to make sentences shorter.
- 4.3 Use a vertical list for complex text.
- 4.4 Use connecting words to connect sentences with related topics.

### Section 5 - Procedures
- 5.1 Max 20 words in each sentence.
- 5.2 One instruction in each sentence, unless actions occur at the same time.
- 5.3 Write instructions in the imperative.
- 5.4 Divide a descriptive statement from the command with a comma.
- 5.5 Write notes only to give information, not instructions.

### Section 6 - Descriptions
- 6.1 Give information gradually.
- 6.2 Use key words and phrases to organize the text.
- 6.3 Max 25 words in each sentence.
- 6.4 Use paragraphs to show related information.
- 6.5 Each paragraph has only one topic.
- 6.6 No paragraph has more than six sentences.

### Section 7 - Safety instructions
- 7.1 Use a word such as "WARNING" or "CAUTION" to identify the risk level.
- 7.2 Start a safety instruction with a clear command or condition.
- 7.3 Give an explanation that shows the risk or the possible result.

### Section 8 - Punctuation and word count
- 8.1 Use all standard punctuation except the semicolon.
- 8.2 Use hyphens to connect closely related words.
- 8.3 Use parentheses for references, item identifiers, step identifiers, abbreviations, and singular/plural forms.
- 8.4 In a vertical list, a colon counts as the end of a sentence.
- 8.5 Text in parentheses counts as one word.
- 8.6 Count each number, unit, abbreviation, identifier, quoted text, and title as one word.
- 8.7 A hyphenated word counts as one word.

### Section 9 - Writing practices
- 9.1 Use a different construction when a word-for-word replacement is not enough.
- 9.2 Use each approved word correctly.
- 9.3 Do not make phrasal verbs.
- 9.4 Use a consistent style for terminology and wording.

## Technical names in this project

The rules permit these as written. They are technical names (rule 1.5):

- The tentative name, with capitals: The Thing Below (D-215). The repository: the-thing-below (D-217). The dated records keep the working title terminal-rpg (D-9).
- Tools and platforms: Godot, C#, .NET, xUnit, dotnet format, JSON, Steam, Steamworks, Steam Deck, Aseprite, Makefile, GitHub Actions, gitar, Python.
- Release services: Steam Input, Steam Cloud, Auto-Cloud, Steam Playtest, Next Fest, Steam Linux Runtime, Proton, Movie Maker, Gatekeeper, SmartScreen, notarization.
- The two harnesses: Claude Code, Codex.
- Project names: Core, Game, Tools, Tests, once PR-1 creates them (D-118).
- Process terms: session handoff, decision register, questions register, PR gate, cross-provider review, review record, response file, effective head, property test, seed loop, replay, state hash, simulation version, content hash, string table, night gate, smoke session.
- Art terms: sprite, tile, tile set, atlas, palette, portrait, backdrop, grid, frame, flip, pixel font.
- The standard itself: ASD-STE100, STE.
- Code identifiers in backticks.

## Glossary

One term per concept (D-12). The roadmap interview fills the game terms. Add a row for each term the owner sets, with the refused synonyms.

| Term | Use for | Do not use |
|---|---|---|
| owner | the person who owns the repository and answers every question | user, maintainer, Nate in prose |
| session | one harness invocation | run, conversation |
| provider | Anthropic or OpenAI, as the source of a harness | vendor, model |
| PR | a GitHub pull request | MR, change request |
| run | one play of the game from a seed | playthrough, game |
| tick | one simulation step | frame, turn, unless the design sets turn |
| seed | the integer that starts a run's random streams | random seed |
| record | the file that holds a run's seed and inputs | replay file, log |
| replay | a run driven from a record | playback |

Game terms from the roadmap interview of 2026-09-12:

| Term | Use for | Do not use |
|---|---|---|
| lesson | a rite or a drill that a character equips to gain an ability (D-278) | job, class, materia, skill book |
| rite | a lesson for a spell, on a page or a strip of soft metal (D-275) | scroll, spellbook |
| drill | a lesson for a physical ability, as a written form (D-275) | technique, manual |
| kind | one of the eight families of ability, written with a capital letter: Mend, Harm, Blight, Boon, Blade, Guard, Shot, Theft (D-281) | school, family, when the text means these eight |
| main aptitude | the kind that a character does best (D-274) | class, role, specialty |
| side aptitude | the second kind of a character, which a personal task unlocks (D-282) | side role, side job, subclass |
| lead | the one character whom scenes center on (D-267) | hero, protagonist, main character |
| ability | an action that a lesson gives (D-272, D-278) | skill, technique, move |
| spell | an ability that costs MP (D-42) | magic, cast |
| party | the one to three characters in battle (D-31, D-336) | team, group |
| reserve | the characters who wait outside the party (D-58) | bench, backup |
| cast | the story characters who can join the party, eight in the whole game (D-33, D-299) | roster, heroes |
| hub | a settlement with services, of any shape (D-28) | town, city, base |
| dungeon | an authored area with enemies and a goal (D-39) | level, zone, map |
| region | a slice of the game with hubs and dungeons (D-56) | chapter, act, world |
| arc | the part of the main story that one region tells (D-56, D-131) | plot, chapter |
| encounter | one battle against one enemy group | fight, combat, when a noun |
| timeline | the visible turn order in battle (D-29) | queue, initiative |
| turn | one action of one combatant on the timeline | move, round |
| save point | the place in a dungeon that saves and swaps the party (D-36, D-58) | checkpoint, shrine, in documents |
| down | the state of a fallen character (D-36) | dead, KO, unconscious |
| gear | items in equipment slots (D-44) | equipment, armor, as the set |
| item | a thing in the inventory that is not gear (D-45) | consumable, object |
| gold | the currency (D-60) | money, coins, gil |
| character level | the level from experience (D-34) | level, alone |
| profile | an enemy's personality data (D-65) | personality, brain |
| evaluator | the tactical scorer in core (D-65) | planner, AI, alone |
| tile | one 32 by 32 map position (D-228) | cell, square, glyph |
| sprite | the drawing of a character, an enemy, or an item (D-107) | glyph, icon, image |
| grid | the rows of palette keys of one frame in a drawing file, for a sprite, a tile, a portrait, or a piece (D-107, D-515) | matrix, bitmap |
| atlas | the PNG the tool renders from every grid (D-107) | sheet, texture |
| portrait | the 64 by 64 face in the dialogue box (D-109, D-234) | avatar, face |
| backdrop | the battle background of a place (D-111) | background, stage |
| region map | the node and route screen between places (D-113) | overworld, world map |
| scene | a scripted story beat on the map (D-114) | cutscene, event |

World terms from the world-building interview of 2026-09-12:

| Term | Use for | Do not use |
|---|---|---|
| the thing below | the power under the old ground that answers spilled blood (D-128). With capitals, The Thing Below is the tentative name of the game (D-215) | the old evil, the demon |
| wrong things | the rare creatures that appear near spilled blood (D-155) | monsters, demons |
| waystone | the standing stone of an older age, as an object in the world (D-134, D-141) | save point, when the text means the stone |
| foreign church | the church of the enemy crown, which holds the license law (D-137) | new church, the church, alone |
| stamp | the mark of the foreign church on a rite, which a legal use needs together with a license (D-302) | seal, license, when the text means the mark on a rite |
| old faith | the banned faith of region one (D-137) | old church, pagans |

Audio and time terms from the audio block of 2026-09-14:

| Term | Use for | Do not use |
|---|---|---|
| track | a piece of music that loops, for a place, a battle, or a scene (D-413) | song, tune, when the text means the file |
| night version | the version of a place track that plays at dusk or night (D-443) | night track, variant |
| theme | a short recurring tune that other tracks borrow (D-419) | leitmotif, motif |
| cue | a track that a scene script names, from the mood set or for a key scene (D-418) | scene track, stinger |
| sting | a short musical piece of a few seconds for an event (D-422) | jingle, fanfare |
| ambience | the low background sound of a map (D-424) | ambient sound, soundscape |
| sound effect | a short sound for an action or an event (D-423) | SFX, and effect alone when the text means a sound |
| tracker rows | the note format of a track: one row per step, one column per voice (D-438) | pattern, score |
| time of day | the dawn, day, dusk, or night that content and the story set for a map (D-442) | phase or clock, when the text means the time of day |

Release terms from the release block of 2026-09-14:

| Term | Use for | Do not use |
|---|---|---|
| prologue | region one, free on Steam as the demo of the full game (D-133, D-143) | episode, chapter, free version |
| demo | the Steam app that carries the prologue (D-143, D-478) | trial, sample |
| export | a build of the Game project for one system and one CPU architecture (D-481) | binary, package |
| build artifact | an export that CI keeps from a merge to `main` (D-449) | nightly, snapshot |
| release tag | a git tag of the form `v0.5.0` on a build that ships (D-448) | version tag |
| CI leg | one runner system of the CI matrix, three in all (D-481) | platform, when the text means a runner |
| store page | the Steam page of the full game (D-471) | store presence, product page |
| store text | the short description, the long description, and the feature list of the store page (D-452) | copy, marketing text |
| release notes | the player notes of a release tag or a Steam update (D-453) | changelog, patch notes |
| studio name | the developer and publisher name on the store page and in the credits (D-450) | company, team |
| trusted player | a player whom the owner picks to play a gate build before release (D-469) | tester, playtester |

Tools terms from the roadmaps PR of 2026-09-14:

| Term | Use for | Do not use |
|---|---|---|
| edge file | the generated file of the edge and corner tiles of one map, outside the rule files (D-501) | tile cache, edge map |
| text helper | the one Game helper that puts a string table entry on screen (D-499) | text wrapper, label helper |

CI terms from the roadmaps PR of 2026-09-14:

| Term | Use for | Do not use |
|---|---|---|
| identity file | the committed file that lists each run of the replay-identity set and its expected state hash (D-504) | golden file, baseline, hash list |
| night record | the result file that one leg of a night uploads as an artifact of its run (D-509) | night result, night report |
| docs-only PR | a PR that changes only `docs/`, `README.md`, `CLAUDE.md`, `AGENTS.md`, `.claude/`, and the PR template (D-513) | documentation PR, when the text means this set |

Art terms from the roadmaps PR of 2026-09-14:

| Term | Use for | Do not use |
|---|---|---|
| drawing file | the JSON file of one drawing: its id, its size, the content ids that it draws, and its frames of grids (D-515, D-519) | grid file, sprite file |
| piece | a drawing file that a large picture places, such as a 64 by 64 rock (D-516) | part, chunk, or tile, when the text means a piece |
| large picture | the JSON file that places pieces to make a backdrop layer, full-screen art, or a store image (D-516) | layout, which names a map file (D-39), and composition |
| atlas index | the committed file that gives the place of each frame in the atlas (D-517) | frame list, atlas map |
| review sheet | a PNG that a tool renders to show an art batch to the owner, attached to the PR description (D-514) | contact sheet, when the text means art |

Effects terms from the roadmaps PR of 2026-09-15:

| Term | Use for | Do not use |
|---|---|---|
| normal map | the image that tells 2D light which way each pixel of a drawing faces (D-183, D-184) | bump map, normal texture |
| light setup | the ambient light and the lights of one map at one time of day (D-442) | lighting, light rig, light map |
| effect file | the JSON file of one effect: its emitters, its palette colors, and its timings in ticks (D-182, D-266) | effect resource, particle file |
| effect budget | the committed limits of lights with shadows, live particles, and full-screen passes that hold 60 frames per second on the Deck (D-523) | frame budget, perf budget |
| full-screen pass | an effect that redraws the whole frame, such as fog, glow, a transition, or the CRT (D-523) | post-process, when the text means these |
| wait intent | the intent that Game sends when an effect that the world waits for ends (D-522) | continue intent, done signal |
| transition | one of the full-screen effects of D-195 that start a battle (D-191, D-196) | wipe, which names a party wipe (D-36), and screen change |
| hit-stop | the brief freeze of the battle picture on a heavy blow (D-186) | freeze frame, hitlag |

UI and input terms from the roadmaps PR of 2026-09-16:

| Term | Use for | Do not use |
|---|---|---|
| view | the 16:10 shape or the 16:9 shape of the frame that the player sees (D-480) | resolution, when the text means one of these two |
| fit | the scale of the frame to the screen of the player (D-232) | scaling, and stretch, which names the Godot setting |
| UI style file | the content file of the font sizes, the colors, and the frame drawings, which Game turns into a Godot `Theme` (D-527) | theme file, skin |
| button prompt | the glyph of the button that a line of text names (D-222) | icon, when the text means a button |
| notice | the one-line message that slides in at the top edge of the screen (D-221) | toast, banner |
| window frame | the drawn border of a menu window (D-220) | panel, and border, when the text means this drawing |

## The checker

Until PR-2, the Python script `docs/tools/ste-check.py` is the checker (D-10). Run it before you commit:

```
python3 docs/tools/ste-check.py $(git ls-files '*.md' | grep -v -e '^docs/reviews/' -e '^docs/session-handoff' -e '^docs/archive/')
```

After PR-2, the C# tool replaces it:

```
dotnet run --project TheThingBelow.Tools/TheThingBelow.Tools.csproj -- ste-check --root .
```

The command prints one line per finding: the file, the line, the rule id, and what the rule saw. It exits 1 on any finding. The rules and the exemptions:

| Rule id | What the checker flags |
|---|---|
| STE 5.1 | More than 20 words in a sentence of a numbered list item |
| STE 6.3 | More than 25 words in any other sentence |
| STE 8.1 | A semicolon |
| STE 4.2 | A contraction: `n't`, or a pronoun with `'s`, `'re`, `'ve`, `'ll`, `'d`, or `'m`. A possessive passes |
| STE 3.6 | Passive voice: is, are, was, were, be, been, or being, then a past participle. Two adverbs can stand between them |
| STE 3.2/3.4 | A helper verb: should, would, could, might, may, shall, ought. Also has, have, or had before a participle |
| STE 3.5 | An -ing form as the first word of a sentence, or after a preposition or a helper word |
| STE 6.6 | More than six sentences in a paragraph |

Dated records are exempt by path: `docs/reviews/`, `docs/session-handoff.md`, `docs/session-handoff-archive.md`, and `docs/archive/`. A dated record is history, and a rewrite falsifies it.

The passive and participle rules are heuristics. A past participle is an irregular form from a list, or a word that ends in "ed". "is closed" is a finding, and so is "is required". Rewrite the sentence with the actor as the subject: "the build needs the SDK". "must", "can", and "will" pass, because the standard approves them.

An -ing word that is a noun or a technical name passes: nothing, during, warning, heading, finding, and a list in the script. A hyphenated word never counts as an -ing form. To add a technical name, add it to `ING_ALLOW` in the script, and to the C# list after PR-2.

## Markdown notes

- Tables, fenced code blocks, and front matter are exempt from every rule. Keep cell text short.
- Headings are titles. They count as one word (8.6). The checker reads no rule on a heading.
- Text in backticks, in double quotes, or in parentheses is one word (8.5, 8.6). The grammar rules do not read inside it.
- An HTML comment on one line is not prose, and the checker removes it. Keep each comment on one line, because a comment across lines is not supported.
- A numbered list item is a procedural step. Rule 5.1 applies, max 20 words.
- A bullet list item is one unit. Rule 6.3 applies, max 25 words.
- The "plain-English" paragraphs in the design doc are descriptive text. Rule 6.3 applies.
