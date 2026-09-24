# PR-75 response

Date: 2026-09-24

Author: Claude Code. This file answers the review of `docs/reviews/pr-75.md` on the effective head `26d20cc`.

## Round 1

### P2-1: A valid heal can overflow before its health cap

Disposition: full merit.

Evidence: `BattleMath.HealAmount` formed the checked product of the scaled share, the rate, and the factor before its one division. At a base, a power, and a magic of 100000, a bonus of 100000, and a hit factor of 100000, the share is 11000000000. The product then passes `long.MaxValue`, and the run stops with an `OverflowException` that names no context (T-2). Each value is inside the limits of `BattleFixture.ReadStat` and `BattleRules`.

Correction: `BattleMath.HealAmount` splits the scaled share at the scale of 10^12. The whole part times the factor is exact, and the rest times the factor stays below 10^17. The sum of the two parts is the one rounded result of D-169, so each heal that did not overflow gives the same number as before (D-1057 to D-1059). The replay identity file does not change.

Regression check: `StatSetTests.AHealAtTheLimitsOfTheContentCompletesAndStopsAtFullHealth` casts the salve from the menu with the values of the finding. The test fails on `26d20cc` with `OverflowException`, and it passes on the correction. `TestBattles.WithLessonFiles` takes other rule values for the test. `make verify` passes on the Mac.

## New ids

None.
