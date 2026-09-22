# Glossary of the project areas

Part of the `ste-writing` skill (D-590). Load this file before you write or review text about the game, the world, the art, the audio, the effects, the UI, the story, the tools, CI, or the release. `SKILL.md` holds the process terms.

One term per concept (D-12). Add a row for each term the owner sets, with the refused synonyms.

Game terms from the roadmap interview of 2026-09-12:

| Term | Use for | Do not use |
|---|---|---|
| lesson | a rite or a drill that a character equips to gain an ability (D-278) | job, class, materia, skill book |
| rite | a lesson for a spell, on a page or a strip of soft metal (D-275) | scroll, spellbook |
| drill | a lesson for a physical ability, as a written form (D-275) | technique, manual |
| kind | one of the eight families of ability, written with a capital letter: Mend, Harm, Blight, Boon, Blade, Guard, Shot, Theft (D-281) | school, family, when the text means these eight |
| main aptitude | the kind that a character does best (D-274) | class, role, specialty |
| side aptitude | the second kind of a character, which a personal task unlocks (D-282) | side role, side job, subclass |
| lead | the one character whom story scenes center on (D-267) | hero, protagonist, main character |
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
| patrol | one enemy that a map places, with its routes, its size, and its group (D-740, D-752) | mob, spawn, guard, map enemy |
| route | the list of tiles that one patrol walks (D-739). `docs/design.md` and D-113 use the same word for a link of the region map | path, waypoint list |
| area | the rectangle that holds a large enemy, in the place of a route (D-209, D-741) | zone, pen, region, for this rectangle |
| body | the tiles that one enemy holds: one, two by two, or three by three (D-206, D-737) | footprint, hitbox |
| timeline | the visible turn order in battle (D-29) | queue, initiative |
| turn | one action of one combatant on the timeline | move, when the text means one turn, and round |
| save point | the place in a dungeon that saves and swaps the party (D-36, D-58) | checkpoint, shrine, in documents |
| down | the state of a fallen character (D-36) | dead, KO, unconscious |
| push | the ticks that one action adds to the next turn of its user (D-376, D-768) | cooldown, recovery |
| delay | the number of an action in content that a push reads, in ticks at speed 100 (D-757) | cost, speed cost |
| slot | the place of a combatant on its side, from zero (D-764) | position, index, in prose |
| field | the enemies that stand in a battle, six at most (D-759) | board, arena |
| wave | the waiting enemies of a group, which step in as others fall (D-758, D-778) | reinforcement, spawn |
| strip | the six turns of the timeline that the screen shows (D-756) | bar, queue |
| command menu | the menu of the actions of a character on its turn (D-111, D-827) | action bar, battle menu |
| message line | the one line of the battle screen that states each event (D-213) | log, text box |
| hit flash | the short change of a sprite to one color when a hit strikes it (D-96, D-825) | blink, flicker |
| pointer | the drawing that marks the target under the cursor (D-833) | arrow, cursor, when the text means the drawing |
| lane | one of the two columns of places inside a row of the battle screen (D-759) | column, file |
| lunge | the short slide of an enemy toward the party when it acts (D-832) | bump, charge |
| battle view | the fight as the battle screen shows it, which follows the events that the screen played (D-532) | model, mirror, and view alone, which names the part of the map on screen |
| defend | the action that cuts the damage until the next turn of the character (D-755) | guard, which names a lesson kind (D-377), block |
| move | one kind of strike: its delay, its power, its element, and its status chance. The basic attack is one move (D-376, D-793) | skill, technique, attack, when the text means the kind |
| affinity | how a combatant takes a hit of one element: normal, weak, resist, or absorb (D-794) | weakness, when the text means the whole set, and resistance |
| element table | the eight affinities of one enemy record, and later of one piece of gear (D-790, D-794) | affinity row, element chart |
| immune list | the statuses that an enemy record refuses (D-805) | resistances, immunities |
| share | the part of full health that poison, bleed, or regen moves at a turn, in basis points (D-803, D-808) | tick damage, DoT |
| gear | items in equipment slots (D-44) | equipment, armor, as the set |
| item | a thing in the inventory that is not gear (D-45) | consumable, object |
| gold | the currency (D-60) | money, coins, gil |
| character level | the level from experience (D-34) | level, alone |
| profile | an enemy's personality data (D-65) | personality, brain |
| enemy record | the content file of one enemy: its body size, its stats, and the ids of its abilities (D-557, D-754, D-786) | enemy definition, monster file, stat block |
| ability file | the content file that holds each ability id (D-785) | ability table, move list |
| evaluator | the tactical scorer in core (D-65) | planner, AI, alone |
| tile | one 32 by 32 map position (D-228) | cell, square, glyph |
| sprite | the drawing of a character, an enemy, or an item (D-107) | glyph, icon, image |
| grid | the rows of palette keys of one frame in a drawing file, for a sprite, a tile, a portrait, or a piece (D-107, D-515) | matrix, bitmap |
| atlas | the PNG the tool renders from every grid (D-107) | sheet, texture |
| portrait | the 64 by 64 face in the dialogue box (D-109, D-234) | avatar, face |
| backdrop | the battle background of a place (D-111) | background, stage |
| region map | the node and route screen between places (D-113) | overworld, world map |
| story scene | a scripted story beat on the map, which Core runs (D-114, D-540, D-572) | cutscene, event, scene alone |
| map scene | the Game screen that draws a map (D-572) | scene alone |
| battle scene | the Game screen that draws a fight (D-572) | scene alone |
| hub scene | the Game screen that draws a hub (D-572) | scene alone |
| scene light | the 2D light of the world, which the UI never takes (D-183, D-210) | lighting |
| Godot scene file | the `.tscn` file of a node tree in the Game project | scene, which names a story beat (D-114) |

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
| track | a piece of music that loops, for a place, a battle, or a story scene (D-413) | song, tune, when the text means the file |
| night version | the version of a place track that plays at dusk or night (D-443) | night track, variant |
| theme | a short recurring tune that other tracks borrow (D-419) | leitmotif, motif |
| cue | a track that the audio file names for a story scene and its step, from the mood set or for a key story scene (D-418, D-548) | scene track, stinger |
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
| override grid | the optional file of the `normals` folder under `content/sprites/` that sets the direction of a pixel of a normal map by a numpad digit (D-184, D-839) | normal override, direction map |
| normal-map atlas | the pages `sprites/normal-map-<page>.png`, with each frame at its place on the color page (D-184, D-517) | normal atlas, lighting atlas |
| light setup | the ambient light and the lights of one map at one time of day (D-442) | lighting, light rig, light map |
| effect file | the JSON file of one effect: its emitters, its palette colors, and its timings in ticks (D-182, D-266) | effect resource, particle file |
| effect budget | the committed limits of lights with shadows, live particles, and full-screen passes that hold 60 frames per second on the Deck (D-523) | frame budget, perf budget |
| full-screen pass | an effect that redraws the whole frame, such as fog, glow, or a transition (D-523) | post-process, when the text means these |
| wait intent | the intent that Game sends when an effect that the world waits for ends (D-522) | continue intent, done signal |
| transition | one of the full-screen effects of D-195 that start a battle (D-191, D-196) | wipe, which names a party wipe (D-36), and screen change |
| hit-stop | the brief freeze of the battle picture on a heavy blow (D-186) | freeze frame, hitlag |
| decor piece | a drawn thing that a decor file places at a tile, such as a wall torch, which no rule reads (D-844) | prop, doodad, decoration |
| decor file | the file beside a map that places each decor piece of that map (D-844) | prop file, decor layer |
| decor kind | the kind of a decor piece, with its default light in its kind file (D-843) | decor type, prop kind |
| carried light | the light on the lead that follows its drawn place, which the torch turns on (D-847, D-848) | player light, torch light, when the text means this light |
| key light | the one point light of a battle, from the light setup of its map (D-850) | battle lamp, sun |
| HD-2D look | the art target of D-849: dark ambient light, pools of light, normal maps, hard shadows, and the passes of PR-92 | Octopath style, 2.5D |

UI and input terms from the roadmaps PR of 2026-09-16:

| Term | Use for | Do not use |
|---|---|---|
| view | the part of the map that the frame of 1280 by 720 shows (D-568) | resolution, screen |
| world scale | the multiplier that the world draws at, fixed at 2x on every screen (D-633) | zoom, magnification |
| UI scale | the multiplier that the UI draws at, 1x or 2x, which a display setting gives the player (D-639) | text size, font size |
| fit mode | one of the two ways that the probe of D-621 puts the frame on a screen, whole or fill (D-638) | scale mode |
| fit | the scale of the frame to the screen of the player (D-232) | scaling, and stretch, which names the Godot setting |
| UI style file | the content file of the font sizes, the colors, and the frame drawings, which Game turns into a Godot `Theme` (D-527) | theme file, skin |
| button prompt | the glyph of a button on the screen, which the game never shows (D-815) | icon, when the text means a button |
| notice | the one-line message that slides in at the top edge of the screen (D-221) | toast, banner |
| window frame | the drawn border of a menu window (D-220) | panel, and border, when the text means this drawing |

Story terms from the roadmaps PR of 2026-09-16:

| Term | Use for | Do not use |
|---|---|---|
| story scene step | one entry of a story scene script, such as a move, a line, a choice, or a join (D-173, D-563) | command, action, when the text means a step |
| story flag | a name that is on or off, which a choice or a story scene sets (D-329, D-542) | switch, variable, state bit |
| condition | the one content form that reads the story flags, which every reader uses (D-543) | requirement, gate, predicate |
| quest | one entry of the quest state, a personal task included (D-59, D-538) | mission, task, when the text means the entry |
| personal task | the quest of one character that unlocks the side aptitude (D-282) | side quest, character quest |
| rumor board | the NPC in a hub that shows the open quests (D-59) | quest board, notice board |

Audio terms from the roadmaps PR of 2026-09-16:

| Term | Use for | Do not use |
|---|---|---|
| audio file | the JSON file that names the content ids a track, a sting, an ambience, or a sound effect serves (D-548) | music file, sound map |
| audio bus | one channel of the audio player that a volume setting sets (D-435) | channel, group, mixer |
| sound room | the screen in a development build that plays every track and sound (D-439) | jukebox, audio test |

Release terms from the roadmaps PR of 2026-09-16:

| Term | Use for | Do not use |
|---|---|---|
| capture | the development-build command that replays a run record into PNG frames and a WAV file (D-476, D-551) | recorder, screen capture |
| store image | a capsule, a logo, or a library image of the store page (D-475) | asset, art, when the text means these |
| credits roll | the story scene that plays the credits after the last story scene of region one (D-467, D-552) | end credits, roll, alone |
