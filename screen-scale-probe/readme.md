# The screen scale probe

Status: a throwaway spike. Owner: Nate. Written in ASD-STE100 (D-10).

This project shows one mock frame of 1280 by 720 at three combinations (D-621). The owner read
the frame on four screens, and then picked the scale of the world and the sizes of the text
(OQ-183). The file `handover.md` holds each pick. The branch `spike/screen-scale-probe` holds the project, and it never merges to
`main` (D-597, D-621). The step is section 7.8 of `docs/roadmaps/phase-1-foundations.md`.

## What the frame holds

- The world: a stone hall with a door, a step, four pillars, snow, rocks, and an ice pond.
- The party: the five cast sprites that the owner approved as a sample (D-402).
- The UI: a party row with numbers, and a dialogue window with three lines of body text.
- The body font: Terminus TTF at its native 16 pixels, from its OFL release (D-263).

The tiles in `tiles/` are throwaway. They are not art of the game, and PR-34 draws the real
tiles. The five sprite grids in `sprites/` are copies of the sample of D-402.

## The three combinations

One key steps through the three combinations. The world draws at 2x in each one, because D-633
sets that scale and no setting changes it. The body size is the one value that changes.

Terminus carries a bitmap at 12, 14, 16, 18, 20, 22, 24, 28, and 32 pixels alone. Thus every
text size below is a bitmap at a whole-number scale. A size with no bitmap falls back to the
traced outline, and the glyph loses its square pixel (F-49, D-230). The probe draws each line at
the size of the bitmap, under the scale, and never at the product of the two.

| State | Body text | Title text | Tiles in the frame |
|---|---|---|---|
| 1 | 24 (24x1) | 48 (24x2) | 20 by 11.25 |
| 2 | 32 (32x1) | 64 (32x2) | 20 by 11.25 |
| 3 | 48 (24x2) | 96 (32x3) | 20 by 11.25 |

A title is twice its body. 32 is the largest strike, so each title doubles or triples a smaller
one. Thus each title draws on a coarser pixel grid than its body.

A body of 24 and a body of 32 draw their own strike at 1x. A stem is one frame pixel there, and
one device pixel on the Deck. Those two bodies miss the floor of D-639, and the panel gives a
note. Each carries four times the glyph detail of a doubled smaller strike.

A body of 48 doubles the 24 strike. A glyph pixel takes 2 frame pixels there, and the floor of
D-639 holds.

The border of a panel follows the glyph: 1 frame pixel at a body of 24 and 32, and 2 at 48.

The text budget falls as the body grows. The panel gives both counts for each combination.

| Body text | Characters in a dialogue line | Characters across the frame |
|---|---|---|
| 24 | 104 | 106 |
| 32 | 76 | 80 |
| 48 | 49 | 53 |

The box keeps the three lines of the `game-text-style` skill at every size. The panel gives a
note when the sample needs more lines than three (D-635).

## The two fit modes

A second key changes the fit mode. The mode sets how the frame goes on the screen.

| Mode | What it draws |
|---|---|
| whole | The largest whole-number fit, centered, with black bars around it (D-568) |
| fill | The fit of D-573: a whole-number scale up with Nearest, then a scale down with linear |

On a screen whose fit is a whole number, the two modes draw the same picture. The Deck, a 1440p
screen, and a 4K screen each take a whole-number fit. A 1920 by 1080 screen takes a fit of 1.5x.
There mode whole holds the frame at 1x with wide bars, and mode fill fills the screen.

The probe starts in mode fill. The panel names the mode, the whole-number fit, and the fit of
D-573, and the report holds a state table for each mode.

## The keys

| Key | Button of the pad | What it does |
|---|---|---|
| Space, or the right arrow | A | The next combination |
| The left arrow | B | The combination before |
| W | X | Marks the current world scale as the pick of the owner |
| U | Y | Marks the current UI scale as the pick of the owner |
| F | R1 | The next fit mode, whole or fill |
| H | View | Hides the panel of the probe, or shows it again |
| R | none | Writes the report now |
| Escape, or Q | Menu | Writes the report and stops |

The Deck in desktop mode has no keyboard, so each command takes a button of the pad. The panel
of the probe names the two forms of each command.

## How to run it

The probe needs three flags. They are the name of the screen, the diagonal of the screen in
inches, and the distance from the eye to the screen in centimeters. A run script holds these
numbers for each machine. With no diagonal, the report gives no size in millimeters (T-2).

Two more flags help a check on the machine of the author. The flag `--fit=whole` or `--fit=fill`
sets the mode of the first frame. The flag `--windowed=1920x1080` makes a window of that size,
which proves the code of a fractional fit on a screen of another size.

### The exports for the Deck and Windows

The folder `build/` holds the three run scripts and both exports. Each script reads the binary
beside it, so the binary and the script stay in one flat folder. Git keeps the three scripts
and ignores each export. Thus a change of the code needs the two commands below. Run them from
this folder on the Mac.

```
/Applications/Godot_mono.app/Contents/MacOS/Godot --headless --path . \
  --export-release "Linux x86_64" build/ScreenScaleProbe.x86_64
/Applications/Godot_mono.app/Contents/MacOS/Godot --headless --path . \
  --export-release "Windows x86_64" build/ScreenScaleProbe.exe
```

Each export embeds its pack, and no other file goes with it. Copy `build/` to the machine, or
copy the one binary and the one script of that machine. Never make a second copy of a script
in a folder below `build/`, because the script then reads the wrong folder.

### The Steam Deck

1. Copy the folder `build/` to the Deck, into a folder of its own.
2. Start the Deck in desktop mode, and open a terminal in that folder.
3. Run `chmod +x run-probe-deck.sh ScreenScaleProbe.x86_64`. A copy through a stick loses the mode.
4. Run `./run-probe-deck.sh`. The first argument sets the distance, and the default is 45 cm.
5. The script reads the model of the Deck, and it sets the diagonal from that model.
6. Read the frame with the buttons of the pad, and press Menu at the end.

The Deck is the one screen with a fit of 1x. Thus it is the one screen that shows a stem of one
device pixel, and the one screen that answers the floor of D-639.

### The Windows machine with the 32-inch 1440p screen

1. Copy the folder `build/` to the Windows machine, into a folder of its own.
2. Read the two numbers at the top of `run-probe-windows.bat`, and correct them for the screen.
3. Run the batch file. The first argument sets the distance, and the default is 70 cm.

### The Windows machine with a 1920 by 1080 screen

1. Copy the folder `build/` to the Windows machine, into a folder of its own.
2. Correct the diagonal in `run-probe-1080p.bat`, or give it as the second argument.
3. Run the batch file. The probe starts in mode fill, at a fit of 1.5x.
4. Press F or the right shoulder button, and compare mode fill with mode whole.
5. Mark the text pick in the mode that the game will draw, which is mode fill.

### The Mac with the 27-inch 4K screen

1. Run `./run-probe-mac.sh` from this folder. It starts the installed editor at full screen.
2. The first argument sets the distance, and the default is 70 cm.
3. The editor and an export draw the same pixels, so this run gives the same measurement.
4. `export_presets.cfg` also holds a macOS preset, for a machine with no editor.

## How to read the frame

- The panel of the probe gives the state, the pixel counts, the fit, and the apparent sizes.
- The bar under the panel is 100 mm or 50 mm long. Hold a ruler to the glass, and confirm it.
- The small checkerboard has one device pixel for each square. A blur there shows a scaled screen.
- The panel names the count of characters that one line of the dialogue box holds.
- Press H to hide the panel, and then read the frame as a player reads it.

## How the report works

Each run writes one Markdown file in `reports/`, beside the build. The file holds the facts of
the screen, a row for each of the three combinations, and the marks of the owner. The file holds the
rows of M-8. A second run of the same screen writes a second file, and no run overwrites
another (T-2).

## Traps

- A picture of the frame in a viewer proves nothing. The system maps a picture pixel to a point
  on a screen of high density, and not to a device pixel. Thus the probe draws at full screen.
- On macOS the full screen gives the true pixel count, and a window does not. A windowed run
  reports that the frame does not fill the screen, and exit test 4 fails for that run.
- The frame fits a 16:9 screen at a whole number: 3x at 3840 by 2160, and 2x at 2560 by 1440.
  The Deck gets 1x with a bar of 40 pixels above and below (D-568).
- A 1920 by 1080 screen takes a fit of 1.5x, which is not a whole number. Mode fill draws it.
- The probe needs the owner at three machines. It cannot finish without that.
- The spike keeps its own props file, so it takes no rule of the solution of the game.
- An export drops each file that Godot does not import. Each preset holds an include filter for
  the `.grid` files and for `probe.map`. Without it, an exported build stops at the first
  absent file, and a run of the editor does not show the fault.

These two traps come with the fit modes:

- The ruler bar and the pixel check draw on the panel, in device pixels, outside the frame
  viewport. Thus the fit of the frame never changes their size on the glass.
- A windowed run reads the diagonal of the whole screen, and not of the window. Thus the
  millimeters of such a run belong to that screen, and they stand for no other screen.
