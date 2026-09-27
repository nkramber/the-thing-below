using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Storage;
using TheThingBelow.Tools.Bots;
using TheThingBelow.Tools.Content;

namespace TheThingBelow.Tools.Night;

/// <summary>The plan of one leg of a night (D-1190, D-1191).</summary>
/// <param name="GreedyRuns">The count of runs of the greedy policy.</param>
/// <param name="RandomRuns">The count of runs of the random policy.</param>
/// <param name="FirstSeed">The first seed. Each policy starts its runs at it.</param>
/// <param name="Leg">The runner label of the leg.</param>
/// <param name="Commit">The full hash of the commit that the night plays.</param>
/// <param name="OutFolder">The folder of the results, the summaries, the records of failed runs, and the night record.</param>
public sealed record NightPlan(int GreedyRuns, int RandomRuns, ulong FirstSeed, string Leg, string Commit, string OutFolder)
{
    /// <summary>Gives the count of runs of one policy.</summary>
    /// <param name="policy">The policy.</param>
    /// <returns>The count.</returns>
    public int RunsOf(BotPolicyKind policy) => policy == BotPolicyKind.Greedy ? this.GreedyRuns : this.RandomRuns;
}

/// <summary>
/// The `night` command: one leg of the night job of PR-49 (D-507, D-509). It plays the greedy
/// runs, then the random runs, of one seed range, and it writes the night record of the leg.
/// </summary>
/// <remarks>
/// The random runs play after a failure of the greedy runs too, so one night reports both. The
/// command writes the night record in each case, and it fails when a run ended as a softlock or a
/// crash (T-2). The record of each failed run replays with the `--replay` option of `bots` (T-7).
/// </remarks>
public static class NightCommand
{
    /// <summary>The name of the command.</summary>
    public const string Name = "night";

    /// <summary>The option of the checkout root.</summary>
    public const string RootOption = "--root";

    /// <summary>The option of the count of runs of the greedy policy.</summary>
    public const string GreedyRunsOption = "--greedy-runs";

    /// <summary>The option of the count of runs of the random policy.</summary>
    public const string RandomRunsOption = "--random-runs";

    /// <summary>The option of the first seed.</summary>
    public const string FirstSeedOption = "--first-seed";

    /// <summary>The option of the runner label of the leg.</summary>
    public const string LegOption = "--leg";

    /// <summary>The option of the full hash of the commit that the night plays.</summary>
    public const string CommitOption = "--commit";

    /// <summary>The option of the output folder.</summary>
    public const string OutOption = "--out";

    /// <summary>Runs the command.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer of the summaries and the result.</param>
    /// <param name="errors">The writer of each error line.</param>
    /// <returns>The exit code: 0 when no run failed, else <see cref="Program.FaultExitCode"/>.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(Name, args, [RootOption, GreedyRunsOption, RandomRunsOption, FirstSeedOption, LegOption, CommitOption, OutOption], [], errors);
        if (options is null || PlanOf(options, errors) is not NightPlan plan)
        {
            return Program.FaultExitCode;
        }

        string root = options.ValueOr(RootOption, ".");
        try
        {
            ContentSet content = ContentSet.Load(ContentFolder.Read(root));
            content.Bots.RequireStartsOf(MapSet.Of(content.Maps));
            return Play(content, plan, AcceptedSources.OfCore, BotsCommand.PolicyOf, output, errors);
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException or ContentException or StorageException or RunRecordException)
        {
            errors.WriteLine($"Error: {Name} stopped on the root '{root}' for the leg {plan.Leg}: {fault.Message}");
            return Program.FaultExitCode;
        }
    }

    /// <summary>Plays each policy of a night on one leg, and writes the night record of the leg (D-509).</summary>
    /// <param name="content">The content of this build.</param>
    /// <param name="plan">The plan of the leg.</param>
    /// <param name="accepted">The source of the accepted intents: the query of Core, or the plant of a test.</param>
    /// <param name="policyOf">Makes the policy of the run of one seed.</param>
    /// <param name="output">The writer of the summaries and the result.</param>
    /// <param name="errors">The writer of each failure line.</param>
    /// <returns>The exit code: 0 when no run failed, else <see cref="Program.FaultExitCode"/>.</returns>
    public static int Play(ContentSet content, NightPlan plan, AcceptedSource accepted, Func<BotPolicyKind, ulong, IBotPolicy> policyOf, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        List<NightPolicyCounts> policies = [];
        foreach (BotPolicyKind policy in NightRecord.PolicyOrder)
        {
            int runs = plan.RunsOf(policy);
            BotPlan botPlan = new(policy, runs, plan.FirstSeed, plan.OutFolder, plan.Leg);
            BotPlayed played = BotsCommand.PlayAll(content, botPlan, accepted, policyOf, output, errors);
            BotTotals totals = played.Totals;
            policies.Add(new NightPolicyCounts(
                policy,
                runs,
                totals.CountOf(BotEnd.Complete),
                totals.CountOf(BotEnd.Softlock),
                totals.CountOf(BotEnd.Crash),
                totals.CountOf(BotEnd.Budget),
                played.Seconds));
        }

        NightRecord record = new(plan.Commit, plan.Leg, plan.FirstSeed, policies);
        string path = Path.Combine(plan.OutFolder, NightLegs.FileOf(plan.Leg));
        record.Write(path);
        output.WriteLine($"The night record of the leg {plan.Leg} is '{path}', with the status {record.Status}.");
        if (record.Status == NightRecord.Success)
        {
            return 0;
        }

        errors.WriteLine($"Error: the night on the leg {plan.Leg} at the commit {plan.Commit} has a softlock or a crash. The lines above name each seed.");
        return Program.FaultExitCode;
    }

    private static NightPlan? PlanOf(OptionParser options, TextWriter errors)
    {
        if (RunsOf(options, GreedyRunsOption, errors) is not int greedyRuns || RunsOf(options, RandomRunsOption, errors) is not int randomRuns)
        {
            return null;
        }

        if (!ulong.TryParse(options.Value(FirstSeedOption) ?? string.Empty, NumberStyles.None, CultureInfo.InvariantCulture, out ulong firstSeed))
        {
            errors.WriteLine($"Error: {Name} needs {FirstSeedOption} with a whole number of 0 or more (D-1190).");
            return null;
        }

        string leg = options.Value(LegOption) ?? string.Empty;
        if (!Contains(NightLegs.Labels, leg))
        {
            errors.WriteLine($"Error: {Name} needs {LegOption} with the label of a leg of the night: {string.Join(", ", NightLegs.Labels)}.");
            return null;
        }

        string commit = options.Value(CommitOption) ?? string.Empty;
        if (!NightJson.IsCommit(commit))
        {
            errors.WriteLine($"Error: {Name} needs {CommitOption} with the full hash of the commit, and it read '{commit}'.");
            return null;
        }

        if (options.Value(OutOption) is not string outFolder)
        {
            errors.WriteLine($"Error: {Name} needs {OutOption} with the output folder.");
            return null;
        }

        return new NightPlan(greedyRuns, randomRuns, firstSeed, leg, commit, outFolder);
    }

    private static int? RunsOf(OptionParser options, string option, TextWriter errors)
    {
        if (int.TryParse(options.Value(option) ?? string.Empty, NumberStyles.None, CultureInfo.InvariantCulture, out int runs) && runs >= 1)
        {
            return runs;
        }

        errors.WriteLine($"Error: {Name} needs {option} with a whole number of 1 or more (D-1191).");
        return null;
    }

    private static bool Contains(IReadOnlyList<string> labels, string leg)
    {
        foreach (string label in labels)
        {
            if (string.Equals(label, leg, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
