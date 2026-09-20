# The handover of the screen scale probe

Status: the record of a spike session. Owner: Nate. Written in ASD-STE100 (D-10).

This file keeps the owner answers and the state of the probe. The step is section 7.8 of
`docs/roadmaps/phase-1-foundations.md`, and it has no PR and no review (D-621). Thus no
document in `docs/` holds these answers yet. The next PR reads this file and records each row.
The Deck test of D-160 set the same pattern, and PR-1 recorded its answers.

## The second round of 2026-09-19

The owner reopened the text half of the answer before PR-61 starts. The probe now shows nine
combinations: three world scales and three text sizes. The first round and its answers below
stay as they are, and D-633 and D-639 stand until the owner picks again.

| Question | The answer | Note |
|---|---|---|
| The body sizes | 24, 32, and 48 frame pixels | 24 draws the 24 strike at 1x, and the other two double a finer strike |
| The title sizes | 32, 48, and 64 frame pixels | One step above the body, at the scale of the body |
| The world scales | 1x, 1.5x, and 2x | 1.5x is the one combination that is not pixel-exact |
| What ships to the player | The text size alone | A player world scale would break D-568: every screen shows the same part of the map |
| Where the probe lives | This branch, with no PR | The same pattern as the first round (D-621) |

The owner asked whether one text size can remove the UI scale setting of D-639. A body of 32
or 48 can. Each doubles a strike, so a glyph pixel takes 2 frame pixels. Thus the floor of
D-639 holds on the Deck at a fit of 1x. A body of 24 draws the 24 strike at 1x and misses that
floor.

The cost is the text budget. A dialogue line holds 104 characters at a body of 24. It holds 76
at a body of 32, and 49 at a body of 48 (D-635).

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

## The runs of the owner, 2026-09-18

The owner ran the probe on the three screens, and marked these picks:

| Screen | The state | The world | The UI |
|---|---|---|---|
| OLED Deck, at 1x fit | State 2 | 2x | 2x |
| 27-inch 4K screen, at 3x fit | State 3 | 2x | 1x |
| 32-inch 1440p screen, at 2x fit | State 3 | 2x | 1x |

## The answers to OQ-183

Each answer below needs a decision row. The owner gave them on 2026-09-18.

1. The world draws at 2x on every screen, so the frame holds 20 by 11.25 tiles.
2. The UI never draws below 2 device pixels for each art pixel.
3. Thus the UI scale is 2x at a frame fit of 1x, and 1x above that fit.
4. One rule covers every screen shape, and no table of screens is necessary.
5. A display setting gives the player two UI values, 1x and 2x, and no other value.
6. The rule of item 2 sets the default value of that setting on each screen.
7. Each UI layout must hold at both values, on every screen shape.
8. The dialogue limit of the `game-text-style` skill drops from 80 characters to 76.
9. M-8 records the computed numbers, and not a measurement of each run.

The three picks sit in one band of apparent size, from 30 to 43 arcminutes for a line of body
text. The states that the owner refused sit far outside it. The Deck at 1x gives 15 arcminutes,
and a desktop screen at 2x gives 73 to 86 arcminutes.

## M-8: the apparent size on each screen

The numbers come from the geometry of each screen. The distance is 45 cm for the Deck and 70 cm
for each desktop screen, which the owner accepted as the assumption of M-8 (exit test 3).

| Screen | Fit | Millimeters for one device pixel | A sprite at world 1x | A sprite at world 2x | A body line at UI 1x | A body line at UI 2x |
|---|---|---|---|---|---|---|
| OLED Deck, 1280 by 800, 7.4 inches | 1x | 0.1245 | 3.98 mm, 30.4' | 7.97 mm, 60.9' | 1.99 mm, 15.2' | 3.98 mm, 30.4' |
| 27-inch 4K screen, 3840 by 2160 | 3x | 0.1557 | 14.94 mm, 73.4' | 29.89 mm, 146.8' | 7.47 mm, 36.7' | 14.94 mm, 73.4' |
| 32-inch 1440p screen, 2560 by 1440 | 2x | 0.2724 | 17.43 mm, 85.6' | 34.87 mm, 171.2' | 8.72 mm, 42.8' | 17.43 mm, 85.6' |

Each screen takes the world at 2x. The Deck takes the UI at 2x, and each desktop screen takes
the UI at 1x. The apparent size of a sprite goes from 61 to 171 arcminutes across the three
screens. One tile count for every screen gives that spread, and D-37 asks for it.

## The 1080p run of 2026-09-18

The first analysis of the nine answers found a conflict. Answer 2 gives the UI a floor of 2
device pixels for each art pixel. Answer 3 gives the UI 1x above a fit of 1x. A 1920 by 1080
screen takes a fit of 1.5x, so answer 3 gives 1.5 device pixels there, under the floor of
answer 2. D-568 makes that screen a screen which must look good.

The owner asked for a measurement of the 1080p screen. The probe of the first build gave no
such measurement. It computed the fit with integer division, so it drew a 1080p screen at 1x
with wide bars. The fit of D-573 never reached the glass.

This session added the two fit modes above, and it proved them in a window of 1920 by 1080 on
the Mac. Mode fill takes the frame through the two steps of D-573. The owner then runs the new
Windows build on the 1080p screen, and the pick of that run closes the last part of OQ-183.

| Fit mode | Fit | A body line at UI 1x | A body line at UI 2x |
|---|---|---|---|
| whole | 1x | 2.20 mm, 10.8' | 4.39 mm, 21.6' |
| fill | 1.5x | 3.29 mm, 16.2' | 6.59 mm, 32.3' |

The table holds the numbers of the check run, on a 23.8-inch 4K screen. A real 23.8-inch 1080p
screen has a device pixel two times as wide, so it doubles each millimeter and each arcminute.
There a body line at UI 1x gives 6.59 mm and 32.3 arcminutes, inside the band of the three
picks. A body line at UI 2x gives 13.17 mm and 64.7 arcminutes, far above that band.

## The state of the probe

- The project builds with no warning, and the two exports carry the managed assembly.
- `build/ScreenScaleProbe.x86_64` is the native Linux export for the Deck (D-458).
- `build/windows/ScreenScaleProbe.exe` is the Windows export for the 32-inch 1440p screen.
- `run-probe-mac.sh` starts the installed editor at full screen on the Mac.
- The two builds stay out of git, because each one is above 150 MB. Copy them from this Mac.
- A run on the Mac at full screen proved the frame, the four states, the ruler, and the report.
- A run of an exported build proved the flags, the packed files, and the report of an export.
- `export_presets.cfg` also holds a macOS preset, which serves as a fallback for the Mac.
- The owner ran the probe on all three screens on 2026-09-18, and the picks are above.
- The two fit modes came later on 2026-09-18, and both exports carry them.
- `build/windows/run-probe-1080p.bat` runs the probe on a 1920 by 1080 screen.
- The two exports and the Mac run all carry the same code.

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
20. The Deck in desktop mode has no keyboard, and the probe needs six commands.
21. Thus each command takes a button of the pad as well: A, B, X, Y, View, and Menu.

## The handoff entry of this session

This branch never merges, so `docs/session-handoff.md` on `main` holds no entry for this
session. The next PR copies the entry below to the top of that file, as Session 99. It takes
the next free session number, if another session lands first.

- What the session did: built the probe of D-621, and the owner ran it on three screens.
- The state of the build: the probe is complete. `main` is `b3ec2b4`, and the spike head holds it.
- What is in flight: nothing. The step is complete, and OQ-183 has its answer.
- Traps: the findings above, and the trap list of `readme.md`.
- The questions that block progress: none. OQ-183 closes with the nine answers above.
- The next concrete action: the next PR records the answers, and then PR-7 and PR-34 start.

## What the next PR records

1. One decision row for each of the seven owner answers above.
2. The answer to OQ-183: the scale of the world, and the scale of the UI.
3. The rows of M-8 in section 5 of `docs/design.md`, from the three reports.
4. The findings above as new F- rows.
5. The revision of the dialogue limit in the `game-text-style` skill, if the UI takes 2x.
6. The entry of this session in `docs/session-handoff.md`.
7. The exact citation of findings 9 to 12, because `main` still holds the bare name.
8. Finding 13 in the export work of PR-40 and PR-54, which own the presets of the game.
9. The nine answers to OQ-183 as decision rows, and the close of OQ-183.
10. The revision of D-568: the frame holds 20 by 11.25 tiles, and no longer 40 by 22.5.
11. The 32-pixel tile, the 16-pixel font, and the frame of 1280 by 720 stand under that revision.
12. The M-8 table above in section 5 of `docs/design.md`, with its assumed distances.
13. The limit of 76 characters in the `game-text-style` skill, for a dialogue line.
14. A check of each other limit of that skill that names 80 characters, such as a lore entry.
15. The UI scale setting in `docs/roadmaps/area-ui-input.md`, beside the fit of PR-61.
16. Two UI values in the screen tests of PR-41, for each screen shape.
17. The map size of PR-7 and the art batch of PR-34, which now read 20 by 11.25 tiles.
