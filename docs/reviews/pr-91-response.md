# PR #91 response: the dungeon parts (PR-16)

Status: author response to the review of `6a9eb1ae32fca8a41830ce88c46fba7725bacbd5`. Written in ASD-STE100.

## P2-1: Resume can revive an enemy that map memory keeps dead

Disposition: full merit.

Evidence: the trigger reproduced. A snapshot of another build lacked values for `patrol.two`, which the edited map placed, and the memory of the map held it dead. `MapPatrols.Resume` did not read the memory, so `ResumeById` started the patrol alive (D-555, D-1111).

Correction:

- `MapPatrols.Resume` takes the memory of the map, and `MapState.Resume` passes it.
- A new step, `KeepDead`, puts dead each enemy that the memory holds dead. For a snapshot of another build, it logs the change and ends a mark on that enemy (D-1111, D-1113).
- A snapshot of this build that holds such an enemy alive refuses the load, because no rule of this build makes that state (T-2).

Regression check:

- `ResumeDriftTests.AnotherBuildKeepsANewlyPlacedEnemyDeadThatTheMemoryOfTheMapHoldsDead` fails on the old code, where the enemy starts alive. It passes now. A reopen and an entry bring the enemy back.
- `ResumeDriftTests.ASnapshotOfThisBuildThatHoldsAliveAnEnemyThatTheMemoryHoldsDeadFails` proves the refusal.
- `make verify` passed at the correction commit.

## New ids

None. The correction applies D-555 and D-1111. The simulation version of this PR, 35, covers the change (G-17).
