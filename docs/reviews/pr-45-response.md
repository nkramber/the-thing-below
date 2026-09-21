# PR-45 review response

Date: 2026-09-21

The author answers `docs/reviews/pr-45.md`, the review of head `e83e2d6` with the verdict `Changes required`.

## P2-1: Large area coordinates can bypass patrol layout validation

Disposition: full merit.

Reproduction: the new theory `PatrolLayoutTests.AnAreaWhoseEdgePassesTheRangeOfAnIntFailsTheLoad` ran on the old code of `e83e2d6`. The area `x: 2147483640, width: 100` and the area `y: 2147483640, height: 100` each loaded with no error. The sum of the coordinate and the side wrapped below zero, so the edge check passed and the fit loop ran no pass. The third case, `x: 2147483647, width: 1`, failed the load with another message.

Correction: commit `801d6aa`. `PatrolLayout.CheckArea` in `TheThingBelow.Core/Maps/PatrolLayout.cs` compares each side with the room that the map leaves: `area.Width > map.Width - area.X`. The reader already refuses a value below zero, and a map holds at most 256 tiles on a side, so no difference can wrap. After this check, each sum of the fit loop stays at or below the side of the map. The check holds D-209, D-741, and T-2.

Regression check: the theory holds three cases, and each one fails the load with a `ContentException` that names the enemy and the size of the map. All three cases fail on `e83e2d6` and pass on `801d6aa`. `make verify` passes with 1393 tests, and the identity file and the content hash stay the same.

Simulation version: this PR already raises the version from 5 to 6 for the rules of PR-8 (G-17). The fix changes the load of an invalid map alone, and a valid map loads and runs as before, so the version stays at 6.

## New ids

None.

## Final head

The effective head is `801d6aa`. The commit after it changes the metadata set alone: this file and the handoff (D-610).
