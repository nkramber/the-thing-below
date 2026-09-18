# The screen scale probe

Status: a throwaway spike. Owner: Nate. Written in ASD-STE100 (D-10).

This project shows one mock frame of 1280 by 720 at four scale states (D-621). The owner reads
the frame on three screens, and then picks the scale of the world and the scale of the UI
(OQ-183). The branch `spike/screen-scale-probe` holds the project, and it never merges to
`main` (D-597, D-621). The step is section 7.8 of `docs/roadmaps/phase-1-foundations.md`.

## What the frame holds

- The world: a stone hall with a door, a step, four pillars, snow, rocks, and an ice pond.
- The party: the five cast sprites that the owner approved as a sample (D-402).
- The UI: a party row with numbers, and a dialogue window with three lines of body text.
- The body font: Terminus TTF at its native 16 pixels, from its OFL release (D-263).

The tiles in `tiles/` are throwaway. They are not art of the game, and PR-34 draws the real
tiles. The five sprite grids in `sprites/` are copies of the sample of D-402.

## The four states

One key steps through the four states. The world scale and the UI scale are separate, because
OQ-183 asks two questions.

| State | The world | The UI | Tiles in the frame |
|---|---|---|---|
| 1 | 1x | 1x | 40 by 22.5 |
| 2 | 2x | 2x | 20 by 11.25 |
| 3 | 2x | 1x | 20 by 11.25 |
| 4 | 1x | 2x | 40 by 22.5 |

## The keys

| Key | Button of the pad | What it does |
|---|---|---|
| Space, or the right arrow | A | The next state |
| The left arrow | B | The state before |
| W | X | Marks the current world scale as the pick of the owner |
| U | Y | Marks the current UI scale as the pick of the owner |
| H | View | Hides the panel of the probe, or shows it again |
| R | none | Writes the report now |
| Escape, or Q | Menu | Writes the report and stops |

The Deck in desktop mode has no keyboard, so each command takes a button of the pad. The panel
of the probe names the two forms of each command.

## How to run it

The probe needs three flags. They are the name of the screen, the diagonal of the screen in
inches, and the distance from the eye to the screen in centimeters. A run script holds these
numbers for each machine. With no diagonal, the report gives no size in millimeters (T-2).

### The Steam Deck

1. Copy `build/ScreenScaleProbe.x86_64` and `build/run-probe-deck.sh` to one folder on the Deck.
2. Start the Deck in desktop mode, and open a terminal in that folder.
3. Run `chmod +x run-probe-deck.sh`, because a copy through a USB stick loses the mode.
4. Run `./run-probe-deck.sh`. The first argument sets the distance, and the default is 45 cm.
5. The script reads the model of the Deck, and it sets the diagonal from that model.
6. Read the frame with the buttons of the pad, and press Menu at the end.

### The Windows machine with the 32-inch 1440p screen

1. Copy `build/windows/ScreenScaleProbe.exe` and `build/run-probe-windows.bat` to one folder.
2. Read the two numbers at the top of the batch file, and correct them for the screen.
3. Run the batch file. The first argument sets the distance, and the default is 70 cm.

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
the screen, a row for each of the four states, and the marks of the owner. The file holds the
rows of M-8. A second run of the same screen writes a second file, and no run overwrites
another (T-2).

## Traps

- A picture of the frame in a viewer proves nothing. The system maps a picture pixel to a point
  on a screen of high density, and not to a device pixel. Thus the probe draws at full screen.
- On macOS the full screen gives the true pixel count, and a window does not. A windowed run
  reports that the frame does not fill the screen, and exit test 4 fails for that run.
- The frame fits a 16:9 screen at a whole number: 3x at 3840 by 2160, and 2x at 2560 by 1440.
  The Deck gets 1x with a bar of 40 pixels above and below (D-568).
- The probe needs the owner at three machines. It cannot finish without that.
- The spike keeps its own props file, so it takes no rule of the solution of the game.
- An export drops each file that Godot does not import. Each preset holds an include filter for
  the `.grid` files and for `probe.map`. Without it, an exported build stops at the first
  absent file, and a run of the editor does not show the fault.
