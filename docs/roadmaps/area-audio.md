# Area roadmap: Audio

Status: **active focused area roadmap, which PR #11 merged on 2026-09-16.** This file says how the game makes and plays its music and its sounds, and it names the PR that builds each part (D-144, D-485). The phase files give each PR its scope, its exit tests, and its review focus. This file cites each decision by its id and never restates it. It supersedes no earlier file. Written 2026-09-16 in ASD-STE100.

The design doc holds the system map (section 3), the cost model (section 4), and the guardrails (section 6). The file `area-tools.md` holds the synthesizer and the listen command, and `area-ci.md` holds the job that renders and checks each hash. The file `area-core.md` holds the content reader and the events that Core emits. The file `area-story.md` holds the story scene that a cue serves, and `area-battle.md` the fight that makes each sound. The file `area-exploration.md` holds the maps, and `area-ui-input.md` the settings screen and the menu sounds.

External facts, each with the date of its check. The Godot facts come from `godotengine/godot` at the tag `4.7.2-stable`:

- `AudioStreamWAV.load_from_buffer` is a static method. It takes a byte array of WAV data and an options dictionary, and it returns an `AudioStreamWAV`. Source: `doc/classes/AudioStreamWAV.xml`, read 2026-09-16.
- On data that is not WAV, that method returns an empty reference and prints the reason. The messages include "Not a WAV file. File should start with 'RIFF'" and "Format not supported for WAVE file (not PCM). Save WAVE files as uncompressed PCM or IEEE float instead." Source: `scene/resources/audio_stream_wav.cpp`, read 2026-09-16.
- `AudioStreamWAV` holds `data`, "Contains the audio data in bytes", with `loop_mode`, `loop_begin`, `loop_end` in samples, `mix_rate`, `stereo`, and `format`. Source: `doc/classes/AudioStreamWAV.xml`, read 2026-09-16.
- `AudioStreamPlayer.get_playback_position` "Returns the position in the [AudioStream] of the latest sound, in seconds. Returns 0.0 if no sounds are playing." Its note says: "The position is not always accurate, as the [AudioServer] does not mix audio every processed frame. To get more accurate results, add [method AudioServer.get_time_since_last_mix] to the returned position." Source: `doc/classes/AudioStreamPlayer.xml`, read 2026-09-16.
- The `finished` signal of `AudioStreamPlayer` "Emitted when a sound finishes playing without interruptions. This signal is [i]not[/i] emitted when calling [method stop], or when exiting the tree while sounds are playing." Source: `doc/classes/AudioStreamPlayer.xml`, read 2026-09-16.

Text rules: this file follows ASD-STE100 (D-10). Tables are exempt from sentence-length counts.

## 1. Thesis

Every sound in the game comes from a text file that a tool of our own renders (D-87, D-115, D-412). A track is tracker rows, as a sprite is rows of palette keys (D-438). No licensed asset enters the repository, and no rendered file enters git (D-432).

The music has a 16-bit style: synthesized voices that imitate strings, brass, choir, bells, and drums, with echo (D-412). The sound effects share that style (D-423). Music plays everywhere, and each map mixes its ambience under it (D-413, D-424).

Audio is cosmetic. No rule of Core reads a track, a sound, or a length of one, as no rule reads an effect (D-522, G-23). Core emits its events, and Game answers each one with a sound.

The order of the area follows dependency. PR-38 writes the synthesizer, the formats, and the first sounds. PR-69 builds the audio player. PR-70 adds every rule of what plays when, and PR-71 gives the owner the sound room. PR-72 and PR-73 write the music and the sounds of the game (D-546, D-549).

## 5. Findings that bind this area

The register in section 5 of `docs/design.md` holds every finding. These rows bind the audio area.

| # | Finding | Binds |
|---|---|---|
| F-31 | Rendered WAV files in git cost over 500 MB for region one | PR-38: the build renders the audio, and a hash list replaces the files (D-432) |
| F-38 | Double math can differ by platform | PR-38: the synthesizer uses integer math, and det-lint reads it (D-502) |
| F-42 | The Godot export reads the project folder alone | PR-38 and PR-69: the Game assembly carries each render (D-547) |
| F-56 | Two Godot audio calls meet the plan, and one fails in silence | PR-69 and PR-70: a check on each stream, and no exact position (D-428, D-429) |

## 7. Roadmap

Each part below says how one part of the audio works, which decisions set it, and which PR builds it. The phase files give the scope and the exit tests of each PR (D-144). Each part ends with a plain-English paragraph for a reader who does not know the code.

### 7.1 The synthesizer and the note formats

Built by PR-38. Phase file: `phase-2-first-playable.md`.

- The synthesizer lives in Tools as new code, and it renders instrument voices with filters and reverb (D-101, D-277, D-412, D-423).
- It uses integer math alone, so a render gives the same bytes on every CI leg (D-432, D-502, F-38). The `det-lint` tool reads its code with the Core rules (D-496, D-502).
- A track is a grid of tracker rows: one row for each step, and one column for each voice (D-438).
- A sound effect is a parameter file (D-438). Both formats validate against a schema at load (G-6).
- Tracker rows and parameter files stay outside the content hash, because no rule reads them (D-495).
- PR-38 writes six sound effects and one track for the first dungeon.
- OQ-156 holds the instrument voices, OQ-157 the schema of the rows, and OQ-158 the shape of a render.

> *In plain English:* music and sound start as rows of numbers in a text file. A tool of ours turns those rows into sound, the same way on every computer.

### 7.2 Where a render lives

Built by PR-38 and PR-69. Phase files: `phase-2-first-playable.md`.

- The repository holds the note files and the parameter files, and a text list with the hash of each render (D-432). No audio file enters git.
- The build renders each track and each sound effect into the Game assembly as .NET resources, beside the content files (D-547, D-508).
- A test checks each render against its hash, on every CI leg (D-432, D-481). A changed parameter fails that test.
- Game makes each stream from those bytes with the static method that takes a byte array of WAV data (the external facts above).
- That method returns an empty reference on data that is not WAV, and it prints the reason to the log alone. Game checks every return and fails with the id of the render (T-2, F-56).
- The build takes longer, and a change to a track needs a build of the C# projects before the editor plays it (D-432, D-547).

> *In plain English:* no sound file sits in the repository. The build makes them from the text files each time, and a list of fingerprints proves that nothing changed by accident.

### 7.3 The audio player base

Built by PR-69. Phase file: `phase-2-first-playable.md`.

- PR-69 lands right after PR-38, and it plays a track and a sound effect (D-546).
- Audio buses carry the master, the music, the sound effects, and the ambience, and the four volumes of D-435 set them.
- A mono mixdown is a setting, and the audio player makes it (D-435). OQ-161 holds how.
- The game mutes itself when its window loses focus, and that setting is on by default (D-435).
- Every audio rule lives in Game, and Core never reads a track or a sound (G-1, G-23).
- The settings screen of PR-63 holds the audio group, and `area-ui-input.md` holds that screen (D-226, D-526).

> *In plain English:* this part makes sound come out. It sets the volumes, and it mutes the game when the window loses focus.

### 7.4 The music of a place

Built by PR-70. Phase file: `phase-2-first-playable.md`.

- Music plays everywhere: every map, battle, story scene, and menu has a track (D-413). A place track loops for as long as the party stays.
- Content sets the time of day of each map, and a story flag can change it (D-442). No clock runs.
- A place has a night version of its track only if the story sets it at dusk or night (D-443). Caves and mines keep one version (D-417).
- At a change of the time of day, the place track finishes its musical phrase, then it crossfades to the other version (D-428). OQ-159 holds the phrase mark, and OQ-160 the length of the crossfade.
- The region map plays one track, and it gives no sign of night (D-430).
- The music of the place plays on under every in-game menu (D-421). That covers the party menu, the gear menu, the save point menu, and the hub services.
- The HUD shows no sun and no moon mark, because the light and the music show the time (D-445).

> *In plain English:* every place has its own music, and a few places have a second version for the dark. The music changes when the story turns the day to night.

### 7.5 Battle music, and what follows a fight

Built by PR-70. Phase file: `phase-2-first-playable.md`.

- Each region has three battle tracks: one for common fights, one for boss fights, and one for fights with wrong things (D-415). Ambush and elite fights play the common track.
- Each battle track keeps one version, so a fight at night sounds like a fight by day (D-444).
- The audio player keeps the position of each place track while a fight runs (D-429).
- After a battle, and after the victory sting when the party wins, the ambience of the place plays alone for a few seconds (D-429). Then the place track fades back in at the point where the battle cut in.
- That point comes from a position that Godot does not report exactly (F-56, the external facts above). The audio player must hold its own count from the tick, and it must never read the position as exact.
- The file `area-battle.md` holds the fight, and one run holds the map state and the battle state (D-531).

> *In plain English:* fights have their own music, and when the fight ends the place is quiet for a moment. Then its music comes back where it left off.

### 7.6 Story scenes, the menus, and the main theme

Built by PR-70. Phase file: `phase-2-first-playable.md`.

- A story scene keeps the place music unless a cue serves it (D-418, D-548). A cue comes from a small set of mood tracks, such as tension, grief, and menace.
- A few key story scenes have a track of their own (D-418). The file `area-story.md` holds the story scene.
- The title screen and the last story scene of region one play the main theme (D-427). The region map has a track of its own.
- Three sets of themes run through the music: the main theme, a theme for each faction, and a theme for each cast member (D-419). Other tracks borrow a theme.
- Region one holds ten themes: the main theme, four faction themes, and five character themes (D-419).
- Menus make soft, short sounds: a cursor tick, a confirm, a cancel, and a refusal (D-431). The dialogue box types in silence (D-223).

> *In plain English:* a short tune stands for each group and each person, and other music quotes it. Story scenes borrow a mood track when the place music does not fit.

### 7.7 The stings

Built by PR-70. Phase file: `phase-2-first-playable.md`.

- Four stings play, and each is a short musical piece of a few seconds (D-422). They are a party wipe, a level up, a victory after a battle, and a key find.
- The wipe sting plays before the reload (D-231).
- The victory sting plays after every won fight, beside the weight that D-126 gives each killing.
- Content marks which finds count as a key find: key items, lessons, and rare gear (D-422).
- Core emits the event, and Game plays the sting. No rule waits for a sting (D-522, G-23).

> *In plain English:* four moments get a few seconds of music of their own. They are a win, a level, a rare find, and the end of a run.

### 7.8 Ambience and the sounds of a map

Built by PR-70. Phase file: `phase-2-first-playable.md`.

- Each map plays its ambience low under its music, and each map mixes two layers (D-424).
- The ambience matches the ambient effects of the map: snow and wind, fog and mist, fire and smoke, and dust and drips (D-187, D-202, D-424). The file `area-effects.md` holds the effects.
- Four kinds of map sound exist (D-425):
  - Doors, chests, and traps.
  - A short alarm with the sight mark over a patrol.
  - Footsteps that change with the ground.
  - A softer mix of the ambience and the music near a save point.
- The audio player softens the mix by the distance to a waystone (D-425).
- Each kind of ground needs its own steps, such as snow, rock, wood, and water (D-425). OQ-166 holds the list.
- OQ-162 holds the shape of an ambience, and OQ-163 its volume under the music.

> *In plain English:* every place has a low bed of wind, water, or fire under its music. Your steps change with the ground, and a save point feels quiet.

### 7.9 The sounds of a fight

Built by PR-70. Phase file: `phase-2-first-playable.md`.

- Each of the eight kinds has a base sound, and each ability varies the sound of its kind (D-426). An element adds its own layer.
- Abilities of one kind sound alike by design (D-281, D-426).
- Core resolves each action at once and emits its events, and Game plays the sound of each event (D-532).
- The gamepad vibrates at heavy moments alone (D-434). Those are a heavy blow with its screen shake, a party member down, a boss phase change, and a party wipe.
- Vibration is a setting in the controls group, and `area-ui-input.md` holds it (D-226, D-434).
- OQ-164 holds how an element layers on the sound of its kind.

> *In plain English:* every family of ability has its own sound, and fire, ice, or lightning adds a layer over it. Heavy hits shake the controller.

### 7.10 Who names a track

Built by PR-70. Phase file: `phase-2-first-playable.md`.

- An audio file names the content ids that it serves: the maps, the battles, the menus, and the story scenes (D-548).
- A rule file never names a track, a sting, an ambience, or a sound effect, as D-519 asks for art and D-495 for effects.
- The cue of a story scene lives in the audio file, keyed by the story scene and the step (D-548). That revises D-418 in part.
- PR-70 adds the look-up of a cue to the screenplay tool of PR-50, which prints the cue beside its step (D-173, D-1014, G-25).
- A music batch touches no rule file, so it never changes the content hash and never breaks a stored record (D-495).
- A test fails an audio file that names a content id which no content file declares (T-2).

> *In plain English:* the music files say which places and fights they belong to. The rules of the game never mention music, so new music cannot break an old save.

### 7.11 How the owner hears a batch

Built by PR-38 and PR-71. Phase file: `phase-2-first-playable.md`.

- Sessions write every track and theme as note files, and the owner approves each batch by ear (D-433).
- The listen command in Tools renders a batch and plays each sound (D-439). It comes with PR-38.
- PR-71 adds the sound room in a development build, which plays every track and sound with a map or a battle (D-439). It lands before the first large batch of PR-72 (D-546).
- The sound room lives behind the seam of the debug assembly, and a release export never loads it (D-171, D-260, D-492).
- The PR description of each sound batch lists every track and its purpose (G-25). The owner approves it in that PR (D-57).
- Music from note files that a session writes is unproven, so a batch that fails the ear of the owner costs a rewrite (D-433).
- OQ-165 holds what the sound room shows.

> *In plain English:* the owner listens to every piece of music before it ships. One tool plays a batch on the desk, and one plays it inside the game.

### 7.12 The music and the sounds of region one

Built by PR-72 and PR-73. Phase files: `phase-2-first-playable.md` and `phase-4-region-one.md`.

- PR-72 holds the tracks, the themes, and the sounds of the first playable, after PR-17 (D-549, D-1079).
- The first playable needs the village, the mining town, the hanging cells, and the three battle tracks (D-362, D-369, D-415).
- PR-73 holds the rest of region one, in Phase 4 beside PR-42 (D-549). That includes the ambience of each place that Phase 4 adds, as PR-72 holds it for the first playable (D-424).
- Region one needs about 20 tracks and ten themes (D-419, D-432, D-443, D-444).
- The mining town needs a night version for the night pass of the flight (D-333, D-443).
- Each later region adds its tracks, its three battle tracks, and a theme for each new cast member (D-299, D-419).
- OQ-167 holds the list of the tracks of the first playable.

> *In plain English:* the music arrives in two batches: enough for the first thing the owner plays, then the rest of the first region.

### 7.13 Audio in the tests

Built by PR-38, PR-69, PR-70, and PR-15. Phase files: `phase-2-first-playable.md` and every later phase file.

- Every render matches its hash on every CI leg, and a changed parameter fails the hash test (D-432, the exit tests of PR-38).
- A test proves that the embedded set of renders matches the note files, file by file (D-508, D-547).
- A load of a render that is not WAV fails with the id of the render, not with a silent absence of sound (T-2, F-56).
- A test reads back each audio bus and each default volume, as `area-ui-input.md` does for the font settings (T-2, F-49).
- A test fails an audio file that names a content id which no content file declares (D-548, T-2).
- The headless runner plays with no audio, because Tools holds no Godot (D-64, D-100). No bot run measures sound.
- The smoke session boots with audio on, and it quits with no error in the log (D-117, PR-1).
- No audio change bumps the simulation version, because no rule reads a sound (G-17, G-23).

> *In plain English:* the tests prove that each sound is the sound the author wrote, and that a broken one is loud about it. Robots play in silence.

### 7.14 Audio by PR

| PR | Work | Decisions |
|---|---|---|
| PR-38 | The synthesizer, the two formats, the hash list, the listen command, and the first sounds | D-412, D-423, D-432, D-438, D-439 |
| PR-69 | The audio player base: the buses, the volumes, the mixdown, and a stream from bytes | D-435, D-546, D-547 |
| PR-70 | Every rule of what plays when, and every kind of sound | D-413 to D-431, D-546, D-548 |
| PR-71 | The sound room in a development build | D-260, D-439, D-546 |
| PR-72 | The tracks, the themes, and the sounds of the first playable | D-433, D-549 |
| PR-73 | The rest of the music and the sounds of region one | D-419, D-549 |

### 7.15 Audio that other area files hold

| Part | Area file | PR |
|---|---|---|
| The synthesizer and the listen command in Tools | `area-tools.md` | PR-38 |
| The hash test of each render, inside the test job | `area-ci.md` | PR-38 |
| The content embed in the Game assembly | `area-ci.md` and `area-core.md` | PR-5 |
| The debug assembly that holds the sound room | `area-core.md` | PR-45 |
| The settings screen with the audio group | `area-ui-input.md` | PR-63 |
| The vibration setting in the controls group | `area-ui-input.md` | PR-63 |
| The ambient effects that each ambience matches | `area-effects.md` | PR-58 |
| The battle events that each sound answers | `area-battle.md` | PR-9 and PR-66 |
| The story scenes that a cue serves | `area-story.md` | PR-68 and PR-36 |
| The title screen that plays the main theme | `area-release.md` | PR-33 |

### 7.16 The contract of every later audio PR

Each later PR that adds or changes a sound keeps this list. The phase files make exit tests from it.

1. Write the track or the sound as a text file in content, and never as a rendered file (D-432).
2. Add the hash of its render to the committed list, and prove it on every CI leg (D-432, D-481).
3. Keep every number of the synthesizer an integer (D-502, G-2).
4. Name the content ids that the audio file serves, and change no rule file (D-548).
5. Check every stream that Game makes, and fail with the id of the render (T-2, F-56).
6. Put the batch in the PR description, and take the approval of the owner by ear (D-433, G-25).
7. Add no rule to Core, because no rule reads a sound (G-1, G-23).

> *In plain English:* every new sound is a text file with a fingerprint. The rules of the game stay untouched, and the owner listens before it ships.

## 8. Sequence

The global order lives in section 8 of `docs/design.md`, and PR #11 set it (D-488). The audio work keeps this order inside it:

1. PR-5: the content reader and the content embed in the Game assembly (`area-core.md`).
2. PR-45: the debug assembly that the sound room needs (`area-core.md`).
3. PR-63: the settings screen with the audio group (`area-ui-input.md`).
4. PR-17: the content of the first playable, before its music (D-1079).
5. PR-38: the synthesizer, the two formats, the hash list, the listen command, and the first sounds.
6. PR-69: the audio player base (D-546).
7. PR-70: every rule of what plays when.
8. PR-71: the sound room in a development build.
9. PR-72: the tracks, the themes, and the sounds of the first playable (D-549).
10. **← GATE 2 (first playable).** The owner signs off on feel, with the music of PR-72 (D-52).
11. PR-73: the rest of the music and the sounds of region one, in Phase 4.
12. **← GATE 4 (region one).**

## 9. Open questions

The register is `docs/questions.md` (D-19). These questions block audio PRs, and each PR asks its questions when it starts (D-487):

- OQ-156: the instrument voices of the synthesizer. Blocks PR-38.
- OQ-157: the schema of the tracker rows. Blocks PR-38.
- OQ-158: the sample rate, the bit depth, and the channels of a render. Blocks PR-38.
- OQ-159: how a track marks the end of a musical phrase. Blocks PR-70.
- OQ-160: the length of a crossfade. Blocks PR-70.
- OQ-161: how the mono mixdown works. Blocks PR-69.
- OQ-162: the shape of the ambience of a map. Blocks PR-70.
- OQ-163: the volume of the ambience under the music. Blocks PR-70.
- OQ-164: how an element layers on the sound of its kind. Blocks PR-70.
- OQ-165: what the sound room shows. Blocks PR-71.
- OQ-166: the kinds of ground that need their own footsteps. Blocks PR-72.
- OQ-167: the tracks of the first playable. Blocks PR-72.

No open question blocks this file.
