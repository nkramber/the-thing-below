using System;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Runs;
using TheThingBelow.Tools.Bots;
using Xunit;

namespace TheThingBelow.Tests;

public sealed class BotSummaryTests
{
    [Fact]
    public void TheLineNamesThePolicyTheSeedTheEndAndEachBattle()
    {
        BotResult result = Result(42, BotEnd.Complete, 610, [new BattleCount(12, BattleOutcome.Won), new BattleCount(3, BattleOutcome.Fled)]);

        Assert.Equal("greedy seed=42 end=complete tick=610 played=610 goal=610 wipes=0 battles=12:won,3:fled", BotSummary.LineOf(result));
    }

    [Fact]
    public void TheSummaryGivesTheEndsTheGoalAndTheSpreadOfTheTurns()
    {
        // D-1182: the minimum, the median, and the maximum turns in a battle, and each outcome.
        BotResult[] results =
        [
            Result(1, BotEnd.Complete, 600, [new BattleCount(10, BattleOutcome.Won)]),
            Result(2, BotEnd.Budget, 900, [new BattleCount(20, BattleOutcome.Won), new BattleCount(4, BattleOutcome.Wiped)]),
            Result(3, BotEnd.Complete, 700, []),
        ];

        string summary = BotSummary.Markdown(BotPolicyKind.Greedy, "ubuntu-24.04", results, 7);

        Assert.Contains("### Bot runs: greedy on ubuntu-24.04", summary, StringComparison.Ordinal);
        Assert.Contains("- Runs: 3, in 7 seconds.", summary, StringComparison.Ordinal);
        Assert.Contains("- Ends: budget 1, complete 2.", summary, StringComparison.Ordinal);
        Assert.Contains("- Played ticks to the goal: minimum 600, median 600, maximum 700.", summary, StringComparison.Ordinal);
        Assert.Contains("- Battles: 3. Outcomes: wiped 1, won 2.", summary, StringComparison.Ordinal);
        Assert.Contains("- Turns in a battle: minimum 4, median 10, maximum 20.", summary, StringComparison.Ordinal);
    }

    private static BotResult Result(ulong seed, BotEnd end, long played, BattleCount[] battles)
    {
        RunRecorder recorder = new(RunHeader.ForThisBuild("hash", seed), BattleRuns.IntoBattle(1, "group.fixture_pair").Snapshot());
        long? goal = end == BotEnd.Complete ? played : null;
        return new BotResult(seed, BotPolicyKind.Greedy, end, played, played, goal, 0, null, battles, recorder.Build(), 0);
    }
}
