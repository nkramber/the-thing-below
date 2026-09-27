using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Runs;
using TheThingBelow.Tools.Bots;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The totals of the runs of one policy (D-1182, D-1191).</summary>
public sealed class BotTotalsTests
{
    [Fact]
    public void TheTotalsCountEachEndAndEachFailure()
    {
        BotTotals totals = new();

        totals.Add(Result(1, BotEnd.Complete, [new BattleCount(10, BattleOutcome.Won)]));
        totals.Add(Result(2, BotEnd.Softlock, []));
        totals.Add(Result(3, BotEnd.Crash, [new BattleCount(4, BattleOutcome.Wiped)]));
        totals.Add(Result(4, BotEnd.Complete, []));

        Assert.Equal(4, totals.Runs);
        Assert.Equal(2, totals.Failures);
        Assert.Equal(2, totals.CountOf(BotEnd.Complete));
        Assert.Equal(1, totals.CountOf(BotEnd.Softlock));
        Assert.Equal(1, totals.CountOf(BotEnd.Crash));
        Assert.Equal(0, totals.CountOf(BotEnd.Budget));
        Assert.Equal([10L, 4L], totals.Turns);
    }

    /// <summary>
    /// The regression test of the memory of a night. The bot job kept each result with its run
    /// record until the summary, and 20,000 random runs held 5.8 GB. A night of D-1191 plays more
    /// runs on a runner with 7 GB (G-14). No field of the totals can hold a result or a record.
    /// </summary>
    [Fact]
    public void TheTotalsKeepNoRunRecord()
    {
        FieldInfo[] fields = typeof(BotTotals).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        Assert.NotEmpty(fields);
        foreach (FieldInfo field in fields)
        {
            Assert.False(
                Holds(field.FieldType, typeof(BotResult)) || Holds(field.FieldType, typeof(RunRecord)),
                $"The field '{field.Name}' of {nameof(BotTotals)} can hold a run result or a run record, so a night keeps each record in memory.");
        }
    }

    private static bool Holds(Type type, Type kept) =>
        type == kept || type.GetGenericArguments().Any(argument => Holds(argument, kept)) || (type.IsArray && Holds(type.GetElementType()!, kept));

    private static BotResult Result(ulong seed, BotEnd end, BattleCount[] battles)
    {
        RunRecorder recorder = new(RunHeader.ForThisBuild("hash", seed), BattleRuns.IntoBattle(1, "group.fixture_pair").Snapshot());
        long? goal = end == BotEnd.Complete ? 600 : null;
        return new BotResult(seed, BotPolicyKind.Greedy, end, 600, 600, goal, 0, null, battles, recorder.Build(), 0);
    }
}
