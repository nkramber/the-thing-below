# Import a PNG into a drawing file

Status: active runbook. Written 2026-09-28 for PR-51 (D-688, D-689, D-1310 to D-1316). Written in ASD-STE100 (D-10).

The drawing files are the source of the atlas, and no one edits the atlas by hand (D-107). This runbook gives the two paths from a PNG to a drawing file. The first path is a hand edit of one frame. The second path is a picture of the Sprite Fusion generator (D-686). Each path replaces one frame of an existing drawing file, and the file keeps its id, its page, its draws, and its ticks (D-1311).

Run each command from the root of the checkout. The short form `tools` below means this command:

```bash
dotnet run --project TheThingBelow.Tools/TheThingBelow.Tools.csproj --
```

## Edit one frame by hand

1. Write the frame to a PNG: `tools frame-png --root . --drawing <file> --frame <n> --out <png>`. The command never writes over a file.
2. Open the PNG in a paint program. The PNG is RGBA at 1x, with alpha 0 on each transparent pixel.
3. Paint with the colors of the palette alone. The swatch sheet of `atlas --sheets <folder>` shows each color.
4. Use a pencil tool with no soft edge. A pixel of partial alpha fails the import (D-1315).
5. Save the PNG as RGB or RGBA with 8 bits for each channel. Do not save an indexed PNG (D-176).
6. Import the PNG: `tools import --root . --mode hand-edit --png <png> --drawing <file> --frame <n>`.
7. Build the atlas again: `tools atlas --root .` (D-1316).
8. Read the review sheets of `tools atlas --root . --sheets <folder>` before the commit (D-514).

The hand-edit mode never picks a near color. A pixel with a color outside the palette fails, and the message names the file, the pixel, and the color (D-688). The PNG must have the size of the drawing, because the mode crops nothing and scales nothing (D-689).

## Import a picture of the generator

1. Write a stub drawing file for a new subject: the id, the page, the draws, and one frame of dots.
2. Set the size of the stub to 32 by 32 or 64 by 64 pixels. That size is the frame (D-1310).
3. Import the picture: `tools import --root . --mode generator --png <png> --drawing <file> --frame <n>`.
4. Read the report. It gives the place of the content and the count of the mapped pixels (D-688).
5. Build the atlas again: `tools atlas --root .` (D-1316).
6. Read the review sheets before the commit (D-514).

The generator mode removes the blank border of the picture, which is each row and each column of alpha 0. It puts the content at the center of the frame. The extra pixel of an odd space goes to the right and to the bottom (D-1312). A character can then stand above the bottom row, and a hand edit moves it down.

The mode maps each pixel to the color of the palette with the smallest squared RGB distance. A tie goes to the lower index of the palette (D-1314). The count of the report tells how many pixels took a near color.

Content above the frame fails with the file and the size. The mode never scales a picture, because a scale of pixel art makes new colors and soft edges (D-689). Redraw the picture by hand to fit the frame, or call the generator again (D-1310).

## Errors

| Message | Cause | Fix |
|---|---|---|
| `the color type is 3` | An indexed PNG | Save the PNG as RGB or RGBA |
| `holds the alpha` | A soft brush or a soft eraser | Paint the pixel again with alpha 0 or 255 |
| `the palette holds no such color` | A color outside the palette in a hand edit | Paint the pixel again with a color of the palette |
| `crops nothing and scales nothing` | A hand-edit PNG of another size | Write the frame again with `frame-png`, then edit it |
| `the content is` | Content above the frame of the drawing | Redraw the picture, or call the generator again |
| `has no frame` | A frame number above the last frame | Count the frames from 0 |
| `the output already exists` | A `frame-png` output that names a file, a link to a drawing included | Remove the old PNG, or name another output file |
