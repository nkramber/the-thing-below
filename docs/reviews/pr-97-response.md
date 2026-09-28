# PR-97 response

Date: 2026-09-28

Author: Claude Code. This file answers the review of `docs/reviews/pr-97.md` at head `0cbf3ca8ca9fef24187344199707fc723ee8b2e6`.

## P2-1: `frame-png` can overwrite its source drawing

Disposition: full merit.

- The trigger reproduced. The new test `AnOutputPathOfTheDrawingFileFailsAndKeepsTheDrawing` gave exit code 0 on `0cbf3ca`, where it expects 1.
- Correction: `TheThingBelow.Tools/Import/FramePngCommand.cs` compares the full paths of `--drawing` and `--out` before any read or write. When they name one file, the command fails, names both paths, and writes nothing (T-2, D-1313).
- The compare ignores case, because the disk of the Mac ignores the case of a name. A refusal of two Linux names that differ by case alone costs a second name, and it never costs a drawing.
- The test gives the output as `<folder>/./walk.json`, so the compare reads full paths and not the text of the option. It checks the exit code, both paths in the message, and each byte of the drawing.
- The `import` command has no such path. It reads the PNG before it writes, and the PNG reader refuses a JSON file, so a `--png` of the drawing file writes nothing.
- `docs/runbooks/art-import.md` adds the message to its table of errors.
- Regression check: the test passes after the correction. `FramePngCommandTests` and `ImportCommandTests` give 16 of 16, and `make verify` passed before the push.

## New ids

None.

## Final head

The commit that adds this file, the correction, and session 392.

## P2-1, round 2: an alias of the drawing file

Date: 2026-09-28. This part answers the repeat review at head `7ca7b25106e18b8ed8234b0c66824c9e2e21d411`.

Disposition: full merit.

- The trigger reproduced. A symbolic link `alias.png` to a copy of `marrek-map-front.json` took the PNG bytes, and the command gave exit code 0.
- The compare of full paths of round 1 cannot find every alias. A symbolic link, a hard link, and a folder link each name the drawing by another path, and .NET gives no compare of file identity on every system.
- Correction: `frame-png` never writes over a file. It opens the output with the mode `CreateNew`, so the system refuses each name that exists, whatever the alias (T-2, D-1313). The compare of round 1 goes, because this rule covers it.
- The cost: a second write of one frame to one path needs the removal of the old PNG first. The message says so, and the runbook table names it.
- Regression checks: `AnOutputLinkToTheDrawingFileFailsAndKeepsTheDrawing` makes a symbolic link to the drawing and checks each byte of the drawing. `AnExistingOutputFileFailsAndKeepsItsBytes` covers any existing file, and a hard link to the drawing is such a file. The test of the same path stays. `FramePngCommandTests` and `ImportCommandTests` give 18 of 18.
- The correction changes `TheThingBelow.Tools/Import/FramePngCommand.cs`, `TheThingBelow.Tests/FramePngCommandTests.cs`, `docs/runbooks/art-import.md`, and section 7.52 of `docs/roadmaps/phase-2-first-playable.md`.

## Final head, round 2

The commit that adds this part, the correction, and session 394.
