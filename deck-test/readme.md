# The Deck test

Status: a throwaway spike. Owner: Nate. Written in ASD-STE100 (D-10).

This project measures the renderer on the Steam Deck (D-160), and it measures the effect
budget (D-523). It never merges to `main`. The branch `spike/deck-test` holds it, so a later
session can run the test again before M-6 (OQ-92).

## What the test measures

The test scene draws the frame of 1280 by 720 with 32-pixel tiles (D-568). The load holds
each part that D-160 and D-228 name:

- A tile layer with a normal map on every tile, so light wraps around a shape (D-183).
- Point lights with hard shadows from wall occluders (D-183).
- A soft glow on light brighter than white (D-188).
- The four ambient kinds of region one: snow, embers, smoke, and dust (D-187).
- Fog and mist as a full-screen pass (D-187, D-523).
- The CRT pass with the scanlines of D-240, on by default (D-120).
- A wipe transition over the full load.

The scene measures itself and writes a report (OQ-93). It reads the time of each frame, and
it counts each frame longer than 16.667 ms, the budget of 60 frames each second (D-161).

## The sweep

The test runs 20 stages. Each stage holds 60 warm-up frames and 300 measured frames. Nothing
caps the frame rate, so each frame shows its true cost.

| Stage group | What it gives |
|---|---|
| `world-only` to `pass-crt-glow-fog` | The fixed cost of the map and of each full-screen pass |
| `lights-1` to `lights-15` | The light row of the effect budget (D-523) |
| `particles-512` to `particles-8192` | The particle row of the effect budget (D-523) |
| `full-load`, `full-load-transition`, `worst-case` | The load of D-160, with and without a transition |

The sweep stops the light row at 15, because Godot drops each light past 15 on one canvas
item with no message (F-46).

## How to run it on the Deck

1. Export the native Linux build on the Mac (D-458):

   ```
   /Applications/Godot_mono.app/Contents/MacOS/Godot --headless --path . \
     --export-release "Linux x86_64" build/DeckTest.x86_64
   ```

2. Copy `build/DeckTest.x86_64` and `run-deck-test.sh` to one folder on the Deck.
3. Start the Deck in desktop mode, open a terminal, and run `./run-deck-test.sh`.
4. The script runs both renderers and puts every report in `reports/`.

The script also records the model of the Deck and the refresh rate of its screen. SteamOS
names the LCD Deck `Jupiter` and the OLED Deck `Galileo`.

## How to read the result

The renderer that holds 60 frames each second with more room wins (D-160, D-161). Compare
the `full-load` stage of each report first. Then read the budget rows at the end of each
report for the effect budget of D-523.

If neither renderer holds 60 frames each second, the owner decides then (D-261).

## The files

| File | What it holds |
|---|---|
| `scripts/Main.cs` | The sweep, the report, and the budget rows |
| `scripts/SceneRig.cs` | The world, the lights, the particles, and the passes |
| `scripts/FrameMeter.cs` | The time of each frame, and the count over the budget |
| `scripts/TextureFactory.cs` | Every texture, made in memory, so the spike commits no binary |
| `shaders/` | The CRT pass, the fog pass, and the transition |
| `fetch-export-templates.sh` | Gets the export templates and checks the SHA-512 (OQ-83) |
| `run-deck-test.sh` | Runs both renderers on the Deck |
