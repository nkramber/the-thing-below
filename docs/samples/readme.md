# Samples

Status: folder of dated art samples. Owner: Nate. Started 2026-09-14 (D-403). Written in ASD-STE100 (D-10).

This folder keeps art that the owner wants to look at later. A sample is not game content: no loader, tool, or test reads it. Sessions skip this folder during automatic exploration, and they read it only when the owner or the handoff points to it (D-403).

Each sample has its own dated folder:

| Folder | What it holds | Decision |
|---|---|---|
| `2026-09-14-cast-sprites/` | The five cast members of region one as 32 by 32 front sprites: two review sheets and five grids | D-402, D-404 |

## 2026-09-14-cast-sprites

- `cast-front.png`: Marrek, Bergit, Dagvar, Ottild, and Elio from left to right, at 1x and 6x, on a night ground and on a snow ground.
- `redraw-compare.png`: each 16 by 16 test sprite beside its 32 by 32 redraw. The warden became Bergit, the hexer Dagvar, the cutpurse Ottild, and the mender Elio (D-289).
- `marrek.grid`, `bergit.grid`, `dagvar.grid`, `ottild.grid`, and `elio.grid`: 32 lines of 32 palette keys each, from `content/sprites/palette.json`. A dot is transparent.

The grids follow the test style (D-201, D-237). Each material has an outline in a dark shade of itself, light comes from the left, and no pixel uses a dither. They use the 48 colors of the current palette. Elio carries a plain brass stripe, because the foreign church has no symbol yet.

The sample shows the front view alone. The two other map views, the walk frames, and the battle poses come later (D-199, D-200). PR-34 started from this sample (D-402). It converted the five grids into drawing files under `content/sprites/drawings/cast/`, and it retired the interim atlas tool (D-406, D-515).
