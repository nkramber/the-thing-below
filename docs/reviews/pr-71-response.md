# PR-71 response

Date: 2026-09-24

Author: Claude Code. This file answers the review of `docs/reviews/pr-71.md` on the effective head `f8868ae`.

## Round 1

### P2-1: Snapshot restore accepts an undeclared story actor

Disposition: full merit.

Evidence: `StoryState.ResumeScene` checked a repeated actor, the ground of its tile, and a second actor on one tile. It never read the cast of the build. The reader of the snapshot text checks the kind `character` alone. A stored actor with the id `character.unknown` thus reached the story state, and no record of the battle fixture holds it (D-166, T-2).

Correction: `StoryContent.Load` keeps the id of each character of the battle fixture, and `StoryContent.HoldsCast` reads it. `StoryState.ResumeScene` refuses an actor that the cast of the build does not hold, and the message names the source and the id (D-1006).

Regression check: `StorySnapshotTests.AStoryStateThatNoRunMakesFailsTheLoad` gains the case "an actor that no character of this build holds". The case fails on `f8868ae` and passes on the correction. `make verify` passes on the Mac.

## New ids

None.
