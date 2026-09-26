# PR 87 response

The author answer to the review of `66dcf4db073d96abdf4e7bc1443ec8241b87e219` in `docs/reviews/pr-87.md`. The review gave `Changes required` for one finding.

## P2-1: softlock sampling can skip a softlocked state

Disposition: full merit.

Evidence: the trigger holds. At `66dcf4d`, `BotRun.PlayTick` ran the softlock check only when the count of played ticks was a multiple of 60. The note of that constant said that a softlock holds from its tick on. No rule proves it: the world step goes on while the player has no effect, so an NPC or a patrol can move, and a later state can accept an intent that changes the state. Such a state meets D-1179, and the old rule could miss it.

Correction:

- `BotRun.PlayTick` checks each state before the policy acts. The constant `SoftlockCheckTicks` is gone.
- `SoftlockCheck.Holds` skips the trial when the state accepts a toggle: the open or the close of the menu, or the pause or the end of the pause of a story scene. Each toggle flips a flag that the state hash reads (`MenuOpen` of the run, `Paused` of the story), so its trial always gives another hash. `SoftlockCheck.HoldsByTrial` keeps the full trial, and the check uses it when no toggle is accepted, such as in the battle of a story scene.
- The measure of G-14, 1,000 runs of each policy on this machine in a Release build: the greedy runs took 6 seconds with the check on each tick, and 8 seconds with the check each 60 ticks. The random runs took 5 and 8 seconds. The ends were the same. The counts of D-1180 stay, and the CI run of this head gives the time of each leg.

Regression check:

- `BotsCommandTests.ASoftlockThatClearsOnTheNextTickFailsTheJobAtItsTick` plants a source with no intent at tick 7 alone. The run must end as softlock at tick 7, write its record, and the replay with the same source must repeat the softlock at tick 7.
- With the rule of `66dcf4d` in `BotRun` (a check when the count of played ticks is a multiple of 60), the test failed: "Assert.Equal() Failure: Values differ". With the correction, it passes.
- `SoftlockCheckTests.TheToggleRuleGivesTheAnswerOfTheTrialAtEachSampledState` plays both policies from 12 seeds over both start maps, and compares the toggle rule with the full trial at more than 1,000 states. The two paths give the same answer at each state.

## Ids

No new D-# id and no new F-# id.

## Final head

The head of this answer is the commit that holds this file.
