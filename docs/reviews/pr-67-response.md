# PR-67 response

Date: 2026-09-23

Author: Claude Code. This file answers the review of `docs/reviews/pr-67.md` on the effective head `f92eb3e`.

## Round 1

### P2-1: The cost gate times the evaluator choice alone

Disposition: full merit.

Evidence: `EvaluatorCostCommand.Measure` stopped its timer after `BattleEvaluator.Choose`. The effect of the action, its events, and the end check of `BattleTurns.EnemyTurn` stayed outside the timer. D-961 limits one enemy turn, and the choice is one part of that turn.

Correction: `BattleTurns.EnemyAct` is a public entry to the same enemy turn that the rules play: the choice of the evaluator, then the strike, the heal, the defend, or the step, with its events and the end check (D-65, D-955). The command makes a copy of the run from its snapshot outside the timer, and it times `EvaluatorCostCommand.PlayEnemyTurn` on the copy. The fight that the command plays never reads a timed turn.

Regression check: `EvaluatorCostCommandTests.TheTimedTurnAppliesTheActionOnACopyOfTheRun`. The timed turn gives an action event of the enemy, and it pushes the enemy on the timeline of the copy. The original run keeps its timeline and its events. `make evaluator-cost` on the Mac in Release now gives 24 us at the median and 52 us at the 95th percentile, with 15 legal actions at most. The Deck run of the owner gives the number that D-961 limits.

## New ids

None.
