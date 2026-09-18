# The handover of the screen scale probe

Status: the record of a spike session. Owner: Nate. Written in ASD-STE100 (D-10).

This file keeps the owner answers and the state of the probe. The step is section 7.8 of
`docs/roadmaps/phase-1-foundations.md`, and it has no PR and no review (D-621). Thus no
document in `docs/` holds these answers yet. The next PR reads this file and records each row.
The Deck test of D-160 set the same pattern, and PR-1 recorded its answers.

## The owner answers of 2026-09-18

Each answer needs a decision row, from the next free D- id.

| Question | The answer | Note |
|---|---|---|
| What the mock frame holds | A map frame with a dialogue box, and one frame alone | It matches exit test 1 of section 7.8 |
| The ground of the frame | Throwaway tiles in the spike folder, on the palette of D-181 | Not art of the game. PR-34 draws the real tiles |
| The scale states | Four states, and one key steps through them | The two mixed states answer the UI half of OQ-183 |
| The 32-inch 1440p screen | A Windows build as well, as D-458 and section 7.8 name | The templates were already on the Mac, so it cost no download |
| The measurement | The probe computes it, and a ruler bar on the glass confirms it | Each run takes the diagonal and the distance as flags |
| The pick of the owner | One key marks the pick, and the report file holds it | The three reports then hold the answer to OQ-183 |
| The build on the Mac | A run from the installed editor, with a script | The editor and an export draw the same pixels |

## The state of the probe

- The project builds with no warning, and the two exports carry the managed assembly.
- `build/ScreenScaleProbe.x86_64` is the native Linux export for the Deck (D-458).
- `build/windows/ScreenScaleProbe.exe` is the Windows export for the 32-inch 1440p screen.
- `run-probe-mac.sh` starts the installed editor at full screen on the Mac.
- The two builds stay out of git, because each one is above 150 MB. Copy them from this Mac.
- A run on the Mac at full screen proved the frame, the four states, the ruler, and the report.
- A run of an exported build proved the flags, the packed files, and the report of an export.
- `export_presets.cfg` also holds a macOS preset, which serves as a fallback for the Mac.
- No machine ran the Deck build or the Windows build. The owner does those two runs.

## What the probe already shows

The numbers below come from the geometry of each screen, and the run on the Mac confirmed the
third column. The probe prints the same numbers on each screen, and each run writes them.

| Screen | Fit of the frame | A sprite at world 1x | A sprite at world 2x |
|---|---|---|---|
| OLED Deck, 1280 by 800, 7.4 inches, at 45 cm | 1x | 3.98 mm, 30.4 arcminutes | 7.97 mm, 60.9 arcminutes |
| 27-inch 4K screen, 3840 by 2160, at 70 cm | 3x | 14.94 mm, 73.4 arcminutes | 29.89 mm, 146.8 arcminutes |
| 32-inch 1440p screen, 2560 by 1440, at 70 cm | 2x | 17.43 mm, 85.6 arcminutes | 34.87 mm, 171.2 arcminutes |

The apparent size on a desktop screen is already above two times the apparent size on the Deck
at the same state. The fit of the frame gives that result, because a desktop screen takes 2x or
3x. Thus the Deck sets the floor, as D-92 says, and the desktop screens take the same tile
count from D-37.

## The findings of this session

Each finding needs a row in section 5 of `docs/design.md`, from the next free F- id.

1. On macOS the full screen gives the true pixel count. The Mac reported 3840 by 2160.
2. The system reports a screen scale of 2 there, and the frame fits at 3x.
3. A windowed run reports 1280 by 720 alone, so exit test 4 fails for it.
4. The dialogue box holds 156 characters at the UI scale of 1x, and 76 at 2x.
5. Thus the limit of 80 characters in the `game-text-style` skill holds at 1x alone.
6. The export templates of the Deck test cover each platform, so Windows cost no download.
7. The probe measures the line box of 16 pixels, and not the cap height of a glyph.
8. Thus the probe gives a larger number than the glyph number of F-67.
9. The REF 2 rule of `ste-check` reads a bare file name as one file of the checkout.
10. This spike adds a second `project.godot`, so that name matched two files.
11. The pre-commit hook then refused the commit, and the owner chose the exact citation.
12. Two files now cite `TheThingBelow.Game/project.godot`, and a second Godot project needs that form.
13. An export drops each file that Godot does not import.
14. The `.grid` files and `probe.map` left the first two exports.
15. Thus each preset now holds an include filter for them.
16. The palette came through, because Godot imports JSON.
17. The export of the game meets the same rule, because `content/` holds text grids (D-107, D-116).
18. The macOS export needs the universal binary format, and the ETC2 ASTC import setting.
19. A run of the editor did not show finding 13, and a run of the export showed it at once.

## The handoff entry of this session

This branch never merges, so `docs/session-handoff.md` on `main` holds no entry for this
session. The next PR copies the entry below to the top of that file, as Session 99. It takes
the next free session number, if another session lands first.

- What the session did: built the screen scale probe of D-621 on `spike/screen-scale-probe`.
- The state of the build: the two exports and the Mac script are ready. `main` is `b3ec2b4`.
- What is in flight: the three runs of the owner, which answer OQ-183 and fill M-8.
- Traps: the findings above, and the trap list of `readme.md`.
- The questions that block progress: OQ-183, which the runs answer. It blocks PR-7 and PR-34.
- The next concrete action: the owner runs the probe on the Deck, on the Mac, and on Windows.

## What the next PR records

1. One decision row for each of the seven owner answers above.
2. The answer to OQ-183: the scale of the world, and the scale of the UI.
3. The rows of M-8 in section 5 of `docs/design.md`, from the three reports.
4. The findings above as new F- rows.
5. The revision of the dialogue limit in the `game-text-style` skill, if the UI takes 2x.
6. The entry of this session in `docs/session-handoff.md`.
7. The exact citation of findings 9 to 12, because `main` still holds the bare name.
8. Finding 13 in the export work of PR-40 and PR-54, which own the presets of the game.
