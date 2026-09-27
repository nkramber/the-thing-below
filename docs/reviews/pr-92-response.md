# PR #92 response: the traps, the hazards, and the statuses on the map (PR-64)

Status: author response to the review of `869cc599ee1c3c9e4fc4412a66f311af16b7788a`. Written in ASD-STE100.

## P2-1: A damage trap gives no notice when it downs a fighter

Disposition: full merit.

Evidence: the trigger reproduced. A damage trap downed one fighter of two at 1 health, and the run posted `notice.trap_damage` alone. `TrapRules.HurtEach` dropped the down that `MapHarmRules.Hurt` gave, and the harm of poison and bad air posts `notice.fell_on_map` for the same event (D-392, D-1241).

Correction:

- `TrapRules.HurtEach` tells whether the harm downed a fighter.
- A damage trap posts `MapHarmRules.FellNotice` after its own notice when it downed a fighter and the party did not wipe. The drain of a wipe tells a down of the whole party, so a wipe posts no notice of a down (D-397).

Regression check:

- `TrapRulesTests.ADamageTrapThatDownsOneFighterWhileAnotherStandsPostsTheDownNotice` fails on `869cc59`, where the notice list holds the damage notice alone. It passes now.
- `TrapRulesTests.ADamageTrapThatWipesThePartyPostsNoDownNotice` proves the wipe case.
- The build, 4271 tests, format, det-lint, and the replay identity passed at the correction commit.

## New ids

None. The correction applies D-392, D-397, and D-1241. The simulation version of this PR, 36, covers the change (G-17).

## Final head

The head of the correction commit, which the handoff entry of session 372 records.
