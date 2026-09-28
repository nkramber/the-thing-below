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
