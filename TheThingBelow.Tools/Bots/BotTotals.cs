using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;

namespace TheThingBelow.Tools.Bots;

/// <summary>
/// The totals of the runs of one policy on one leg: the count of each end, the played ticks to
/// the goal, and the turns and the outcome of each battle (D-1182). Each run adds its numbers, and
/// the totals keep no run record.
/// </summary>
/// <remarks>
/// The bot job once kept each result with its run record until the summary. At 20,000 random runs
/// that held 5.8 GB, and a night of D-1191 plays more runs than that on a runner with 7 GB (G-14).
/// </remarks>
public sealed class BotTotals
{
    private readonly SortedDictionary<string, int> ends = new(StringComparer.Ordinal);
    private readonly SortedDictionary<string, int> outcomes = new(StringComparer.Ordinal);
    private readonly List<long> goals = [];
    private readonly List<long> turns = [];

    /// <summary>Gets the count of runs that the totals hold.</summary>
    public int Runs { get; private set; }

    /// <summary>Gets the count of runs that ended as a softlock or a crash (T-2).</summary>
    public int Failures { get; private set; }

    /// <summary>Gets the count of each end by its name, in ordinal order. An end with no run has no entry.</summary>
    public IReadOnlyDictionary<string, int> Ends => this.ends;

    /// <summary>Gets the count of each battle outcome by its name, in ordinal order.</summary>
    public IReadOnlyDictionary<string, int> Outcomes => this.outcomes;

    /// <summary>Gets the played ticks to the goal of each run that reached it, in the order of the runs.</summary>
    public IReadOnlyList<long> Goals => this.goals;

    /// <summary>Gets the count of turns of each battle, in the order of the runs.</summary>
    public IReadOnlyList<long> Turns => this.turns;

    /// <summary>Adds the numbers of one run.</summary>
    /// <param name="result">The result of the run.</param>
    public void Add(BotResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        this.Runs += 1;
        if (BotEnds.Fails(result.End))
        {
            this.Failures += 1;
        }

        Count(this.ends, BotEnds.NameOf(result.End));
        if (result.GoalPlayed is long goal)
        {
            this.goals.Add(goal);
        }

        foreach (BattleCount battle in result.Battles)
        {
            this.turns.Add(battle.Turns);
            Count(this.outcomes, Battle.OutcomeName(battle.Outcome));
        }
    }

    /// <summary>Gives the count of runs with one end.</summary>
    /// <param name="end">The end.</param>
    /// <returns>The count, which is 0 when no run had that end.</returns>
    public int CountOf(BotEnd end) => this.ends.TryGetValue(BotEnds.NameOf(end), out int count) ? count : 0;

    private static void Count(SortedDictionary<string, int> counts, string name) =>
        counts[name] = counts.TryGetValue(name, out int count) ? count + 1 : 1;
}

/// <summary>The totals of the runs of one plan, and their wall time (D-1180, D-1191).</summary>
/// <param name="Totals">The totals of the runs.</param>
/// <param name="Seconds">The wall time of the runs, in whole seconds. No rule reads it (G-3, G-14).</param>
public sealed record BotPlayed(BotTotals Totals, long Seconds);
