using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Streams;

namespace TheThingBelow.Tools.Evaluator;

/// <summary>The numbers of one measurement of the cost of an enemy turn (D-961, F-53).</summary>
/// <param name="Turns">The count of enemy turns that the numbers read.</param>
/// <param name="MostActions">The largest count of legal actions of one turn.</param>
/// <param name="MeanActions">The mean count of legal actions of one turn, rounded down.</param>
/// <param name="Median">The time of the median turn, in microseconds.</param>
/// <param name="Percentile95">The time of the turn at the 95th percentile, in microseconds.</param>
/// <param name="Slowest">The time of the slowest turn, in microseconds.</param>
public sealed record CostReport(int Turns, int MostActions, int MeanActions, long Median, long Percentile95, long Slowest);

/// <summary>
/// The `evaluator-cost` command. It plays the worst fight of D-961, six enemies against three
/// characters, and it times the choice of the evaluator for each enemy turn (F-53, G-14). The
/// limit is 1 ms at the 95th percentile on the Steam Deck.
/// </summary>
/// <remarks>
/// The command times <see cref="BattleEvaluator.Choose"/> alone, because the rules of a turn
/// resolve one action and draw a few rolls. Each timed call opens a new stream of the
/// evaluator, so the fight itself never reads a timed call. Run it from a Release build: a
/// Debug build is slower, and its numbers say nothing of the Deck.
/// </remarks>
public static class EvaluatorCostCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = "evaluator-cost";

    /// <summary>The option that names the root of the checkout, which holds the rules file.</summary>
    public const string RootOption = "--root";

    /// <summary>The option that sets the count of enemy turns to time.</summary>
    public const string TurnsOption = "--turns";

    /// <summary>The count of enemy turns that the command times when no option sets it.</summary>
    public const int DefaultTurns = 5000;

    /// <summary>The limit of one enemy turn at the 95th percentile, in microseconds (D-961).</summary>
    public const long LimitMicroseconds = 1000;

    /// <summary>The count of enemy turns that run before the timed turns, so the compiler has done its work.</summary>
    private const int WarmUpTurns = 500;

    /// <summary>Times the enemy turns of the worst fight, writes the report, and compares the 95th percentile with the limit.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer that takes each line of the report.</param>
    /// <param name="errors">The writer that takes each fault.</param>
    /// <returns>0 when the 95th percentile holds the limit, and 1 on a fault or a miss.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(Name, args, [RootOption, TurnsOption], [], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string turnsText = options.ValueOr(TurnsOption, DefaultTurns.ToString(CultureInfo.InvariantCulture));
        if (!int.TryParse(turnsText, NumberStyles.None, CultureInfo.InvariantCulture, out int turns) || turns <= 0)
        {
            errors.WriteLine($"Error: {Name} takes {TurnsOption} <count>, a count of enemy turns above zero, and it read '{turnsText}' (T-2).");
            return Program.FaultExitCode;
        }

        BattleContent content;
        try
        {
            content = CostFight.Content(options.ValueOr(RootOption, "."));
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException or ContentException)
        {
            errors.WriteLine($"Error: {Name} reads the rules file of the checkout: {fault.Message}");
            return Program.FaultExitCode;
        }

        CostReport report = Measure(content, turns);
        output.WriteLine($"{Name}: {report.Turns} enemy turns of six enemies against three characters (D-961).");
        output.WriteLine($"  legal actions of one turn: {report.MostActions} at most, {report.MeanActions} on average");
        output.WriteLine($"  one enemy turn: median {report.Median} us, 95th percentile {report.Percentile95} us, slowest {report.Slowest} us");
        output.WriteLine($"  limit at the 95th percentile: {LimitMicroseconds} us on the Steam Deck (D-961)");
        if (report.Percentile95 > LimitMicroseconds)
        {
            errors.WriteLine($"Error: the 95th percentile of {report.Percentile95} us passes the limit of {LimitMicroseconds} us. A miss changes the depth or the profiles (D-961, G-14).");
            return Program.FaultExitCode;
        }

        return 0;
    }

    /// <summary>Plays the worst fight on each seed in turn, and times the choice of each enemy turn.</summary>
    /// <param name="content">The battle content of <see cref="CostFight"/>.</param>
    /// <param name="turns">The count of enemy turns to time, after the warm-up.</param>
    /// <returns>The report.</returns>
    public static CostReport Measure(BattleContent content, int turns)
    {
        ArgumentNullException.ThrowIfNull(content);

        List<long> ticks = [];
        long actions = 0;
        int mostActions = 0;
        int played = 0;
        for (ulong seed = 1; ticks.Count < turns; seed += 1)
        {
            Simulation run = IntoFight(seed, content);
            while (ticks.Count < turns && run.State.Battle is Battle battle && battle.Outcome == BattleOutcome.Running)
            {
                foreach (Combatant enemy in battle.Enemies)
                {
                    if (enemy.Place != CombatantPlace.Field || ticks.Count >= turns)
                    {
                        continue;
                    }

                    RandomStream stream = RandomStreams.Open(seed, StreamId.Evaluator);
                    RunContext context = run.State.Context($"{Name}/{enemy.Target.Describe()}");
                    long start = Stopwatch.GetTimestamp();
                    _ = BattleEvaluator.Choose(battle, enemy, content, stream, context);
                    long elapsed = Stopwatch.GetTimestamp() - start;

                    played += 1;
                    if (played <= WarmUpTurns)
                    {
                        continue;
                    }

                    int count = BattleEvaluator.LegalActions(battle, enemy, content).Count;
                    actions += count;
                    mostActions = Math.Max(mostActions, count);
                    ticks.Add(elapsed);
                }

                run.Step([Intent.OfPlayer(IntentIds.BattleAttack, battle.MeleeTargets(BattleSide.Enemy)[0].Target, null)]);
            }
        }

        ticks.Sort();
        return new CostReport(
            ticks.Count,
            mostActions,
            (int)(actions / ticks.Count),
            Microseconds(ticks[(ticks.Count - 1) / 2]),
            Microseconds(ticks[PercentileIndex(ticks.Count, 95)]),
            Microseconds(ticks[^1]));
    }

    /// <summary>Gives the index of a percentile in a sorted list: the nearest rank, from zero.</summary>
    /// <param name="count">The count of the list, above zero.</param>
    /// <param name="percent">The percentile, from 1 to 100.</param>
    /// <returns>The index.</returns>
    public static int PercentileIndex(int count, int percent)
    {
        int rank = (int)(((long)count * percent + 99) / 100);
        return Math.Max(rank, 1) - 1;
    }

    private static Simulation IntoFight(ulong seed, BattleContent content)
    {
        Simulation run = Simulation.Start(seed, CostFight.Map(), content, DebugIntentHandlers.None);
        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        if (run.State.Battle is null)
        {
            throw new InvalidOperationException($"Seed {seed}: the step east into the guard of the cost fight started no battle (T-2).");
        }

        return run;
    }

    private static long Microseconds(long ticks) => ticks * 1_000_000 / Stopwatch.Frequency;
}
