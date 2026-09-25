# PR 82 response

The author answer to the review of `0e71601a8ef762a9a5e0032e8346c682f0e06c18` in `docs/reviews/pr-82.md`. The review gave `Changes required` for one finding.

## P2-1: the selected conflicting binding loses its warning color

Disposition: full merit.

Evidence: the trigger reproduced. At `0e71601`, `SettingsScreen.ShowSlot` painted the cell under the cursor in the cursor color and skipped the warning color for it. The artifact frame `settings-conflict-1x` of that head shows the chosen cell of back in yellow. D-1119 says that each cell whose binding sits in a conflict takes the warning color.

Correction: the owner chose the look of the cursor on such a cell (D-1120). `SettingsMenu.LookOf` gives one of four looks, and `ShowSlot` applies it. A cell of a conflict under the cursor draws its text in the warning color with an outline of 2 frame pixels in the cursor color. The capture of the conflict frame now makes two conflicts, back and confirm on the A button, and the menu and the map on the M key, with the cursor on the cell of back.

Regression check:

- `SettingsMenuTests.EachCellOfAConflictTakesTheWarningLookUnderTheCursorToo` holds the four looks. The case of a chosen cell in a conflict gives `ConflictUnderCursor`, where the old code gave the cursor look.
- The capture session refuses the conflict frame when the menu holds another count than two conflicts.
- The `screen-test` job of the head of this round reads the rendered frame. The author reads that frame and takes it as the baseline in the next round (D-733).

## Ids

D-1120 is new. No new F-# id: F-146 names D-1120 beside D-1119.

## Final head

The head of this answer is the commit that holds this file. The new baseline of the conflict frame comes in the next round, after the screen-test job of this head.
