# Roadmaps

Status: the index of the focused roadmaps. Owner: Nate. Started 2026-09-16 (D-144, D-485). Written in ASD-STE100 (D-10).

This folder expands the roadmap of `docs/design.md`. It never replaces it. The global order of every PR lives in section 8 of that file, and each file here cites a decision and never restates it.

## Where to start

1. Section 8 of `docs/design.md`: the strict order of every PR, with the five gates. Read this first for the question "what comes next".
2. Section 7 of `docs/design.md`: the phase that holds a PR, and the gate of that phase.
3. A phase file below: the scope, the exit tests, the review focus, and the questions of one PR.
4. An area file below: how one area works, and which PR builds each part.

## The two axes

A phase file and an area file cut the same plan two ways. Each PR appears in one phase file and in one or more area files.

| Kind | Axis | Answers |
|---|---|---|
| Phase file | time | What does this PR build, and what must it pass? |
| Area file | subject | How does this area work, and which PR builds each part? |

The area files have no order against each other. Art and battle are subjects, not steps. Two links carry an area back to the time axis:

- Each entry in section 7 of an area file names its phase file.
- Section 8 of an area file gives the order inside that area, and it points at section 8 of `docs/design.md` for the global order.

## The phase files

Five files, one for each phase (D-485). Each entry gives a PR its scope, its exit tests, its review focus, its questions, and the area file that it cites (D-144, D-487).

| File | Phase | Gate |
|---|---|---|
| `phase-1-foundations.md` | Foundations | Every CI leg green, the smoke session green, no play |
| `phase-2-first-playable.md` | First playable | The owner plays the village, one hub, and one dungeon |
| `phase-3-story-systems.md` | Story systems | The owner plays a branch that closes a route |
| `phase-4-region-one.md` | Region one content | The owner plays region one end to end |
| `phase-5-first-release.md` | First release | A tagged build runs, then the Steam demo on the Deck |

## The area files

Twelve files in four groups (D-485).

| Group | File | What it holds |
|---|---|---|
| Technical and testing | `area-core.md` | The engine-free rules, the loop, the record, the save, and the state hash |
| Technical and testing | `area-tools.md` | Every command that is not the game, the gate tools included |
| Technical and testing | `area-ci.md` | Every CI job, the export job, and the night |
| Graphics and effects | `area-art.md` | The drawings, the palette, the atlas, and the large pictures |
| Graphics and effects | `area-effects.md` | Light, particles, glow, the transitions, and the effect budget |
| UI and input | `area-ui-input.md` | The UI base, the menus, the settings, the intents, and the glyphs |
| Systems, audio, release | `area-exploration.md` | The maps, the dungeons, the hubs, the shop, and the region map |
| Systems, audio, release | `area-battle.md` | The timeline, the actions, the elements, the statuses, and the evaluator |
| Systems, audio, release | `area-progression.md` | The character level, MP, the lessons, the aptitudes, the gear, and the items |
| Systems, audio, release | `area-story.md` | The scenes, the story flags, the conditions, the quests, and the arc |
| Systems, audio, release | `area-audio.md` | The synthesizer, the audio player, the music, and the sounds |
| Systems, audio, release | `area-release.md` | The versions, the exports, the store, Steam, and the credits |

## The shape of a file

Each file here keeps the status header and the sections 1, 5, 7, 8, and 9 of the design doc template. The `design-doc-style` skill holds that template, and a session loads it before it writes or edits a file here.
