using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Storage;
using TheThingBelow.Tools.Content;

namespace TheThingBelow.Tools.Bots;

/// <summary>
/// The `bots` command: the headless runner of PR-15 (D-64, D-1179 to D-1184). It plays a range of
/// seeds with one policy, or it replays the record of a failed run.
/// </summary>
/// <remarks>
/// A play writes the result line of each run, the Markdown summary, and the record of each failed
/// run into the output folder, and it fails on a softlock or a crash with the seed, the policy,
/// and the leg (T-2, T-7). A replay plays a record through Core and checks its end for the same
/// softlock, or it repeats the error of the crash.
/// </remarks>
public static class BotsCommand
{
    /// <summary>The name of the command.</summary>
    public const string Name = "bots";

    /// <summary>The option of the checkout root.</summary>
    public const string RootOption = "--root";

    /// <summary>The option of the policy: `random` or `greedy`.</summary>
    public const string PolicyOption = "--policy";

    /// <summary>The option of the count of runs.</summary>
    public const string RunsOption = "--runs";

    /// <summary>The option of the first seed. Each later run takes the next seed.</summary>
    public const string FirstSeedOption = "--first-seed";

    /// <summary>The option of the output folder.</summary>
    public const string OutOption = "--out";

    /// <summary>The option of the name of the leg, which each failure line names.</summary>
    public const string LegOption = "--leg";

    /// <summary>The option of the record file to replay.</summary>
    public const string ReplayOption = "--replay";

    /// <summary>The name of the folder of the records of the failed runs, inside the output folder.</summary>
    public const string RecordsFolder = "records";

    /// <summary>Runs the command.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer of the result.</param>
    /// <param name="errors">The writer of each error line.</param>
    /// <returns>The exit code: 0 when no run failed, else <see cref="Program.FaultExitCode"/>.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(Name, args, [RootOption, PolicyOption, RunsOption, FirstSeedOption, OutOption, LegOption, ReplayOption], [], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string root = options.ValueOr(RootOption, ".");
        try
        {
            ContentSet content = ContentSet.Load(ContentFolder.Read(root));
            content.Bots.RequireStartsOf(MapSet.Of(content.Maps));
            if (options.Value(ReplayOption) is string record)
            {
                return Replay(content, record, AcceptedSources.OfCore, output, errors);
            }

            return PlanOf(options, errors) is BotPlan plan ? Play(content, plan, AcceptedSources.OfCore, PolicyOf, output, errors) : Program.FaultExitCode;
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException or ContentException or StorageException or RunRecordException)
        {
            errors.WriteLine($"Error: {Name} stopped on the root '{root}': {fault.Message}");
            return Program.FaultExitCode;
        }
    }

    /// <summary>
    /// Plays the runs of a plan, writes the result lines, the summary, and the record of each
    /// failed run, and writes a line for each failure with its seed, its policy, and its leg (T-2).
    /// </summary>
    /// <param name="content">The content of this build.</param>
    /// <param name="plan">The plan.</param>
    /// <param name="accepted">The source of the accepted intents: the query of Core, or the plant of a test.</param>
    /// <param name="policyOf">Makes the policy of the run of one seed.</param>
    /// <param name="output">The writer of the summary.</param>
    /// <param name="errors">The writer of each failure line.</param>
    /// <returns>The exit code: 0 when no run failed, else <see cref="Program.FaultExitCode"/>.</returns>
    public static int Play(ContentSet content, BotPlan plan, AcceptedSource accepted, Func<BotPolicyKind, ulong, IBotPolicy> policyOf, TextWriter output, TextWriter errors) =>
        PlayAll(content, plan, accepted, policyOf, output, errors).Totals.Failures == 0 ? 0 : Program.FaultExitCode;

    /// <summary>
    /// Plays the runs of a plan as <see cref="Play"/> does, and gives the totals of the runs and
    /// their wall time. The `night` command writes the night record from them (D-509).
    /// </summary>
    /// <param name="content">The content of this build.</param>
    /// <param name="plan">The plan.</param>
    /// <param name="accepted">The source of the accepted intents: the query of Core, or the plant of a test.</param>
    /// <param name="policyOf">Makes the policy of the run of one seed.</param>
    /// <param name="output">The writer of the summary.</param>
    /// <param name="errors">The writer of each failure line.</param>
    /// <returns>The totals of the runs, and their wall time in whole seconds.</returns>
    public static BotPlayed PlayAll(ContentSet content, BotPlan plan, AcceptedSource accepted, Func<BotPolicyKind, ulong, IBotPolicy> policyOf, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(accepted);
        ArgumentNullException.ThrowIfNull(policyOf);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        string name = BotPolicyKinds.NameOf(plan.Policy);
        Directory.CreateDirectory(plan.OutFolder);
        RunRecordStore records = new(Path.Combine(plan.OutFolder, RecordsFolder));

        // Tools reads the wall clock to measure the count of D-1180. No rule reads it (G-3, G-14).
        Stopwatch watch = Stopwatch.StartNew();
        BotTotals totals = new();
        List<string> lines = [];
        for (int index = 0; index < plan.Runs; index += 1)
        {
            ulong seed = checked(plan.FirstSeed + (ulong)index);
            BotResult result = PlayOne(content, seed, policyOf(plan.Policy, seed), accepted, plan.OutFolder);

            // The totals keep no run record, so the memory of a night stays flat (D-1191, G-14).
            totals.Add(result);
            lines.Add(BotSummary.LineOf(result));
            if (BotEnds.Fails(result.End))
            {
                string path = records.Write(RecordNameOf(plan.Policy, seed), result.Record);
                errors.WriteLine($"Error: the {name} bot run of seed {seed} on the leg {plan.Leg} ended as {BotEnds.NameOf(result.End)} at tick {result.Tick}. {result.Message} The record is '{path}'.");
            }
        }

        long seconds = watch.ElapsedMilliseconds / 1000;
        File.WriteAllLines(Path.Combine(plan.OutFolder, $"results-{name}.txt"), lines);
        string summary = BotSummary.Markdown(plan.Policy, plan.Leg, totals, seconds);
        File.WriteAllText(Path.Combine(plan.OutFolder, $"summary-{name}.md"), summary);
        output.Write(summary);
        return new BotPlayed(totals, seconds);
    }

    /// <summary>Gives the name of the record file of a failed run, such as `greedy-42`.</summary>
    /// <param name="policy">The policy of the run.</param>
    /// <param name="seed">The seed of the run.</param>
    /// <returns>The name with no folder and no file type.</returns>
    public static string RecordNameOf(BotPolicyKind policy, ulong seed) =>
        $"{BotPolicyKinds.NameOf(policy)}-{seed.ToString(CultureInfo.InvariantCulture)}";

    /// <summary>Makes the policy of the bot job for the run of one seed (D-64, D-1183).</summary>
    /// <param name="policy">The policy.</param>
    /// <param name="seed">The seed of the run.</param>
    /// <returns>The policy.</returns>
    public static IBotPolicy PolicyOf(BotPolicyKind policy, ulong seed) =>
        policy == BotPolicyKind.Random ? new RandomPolicy(seed) : new GreedyPolicy();

    /// <summary>
    /// Replays a record and reports whether its failure repeats: the error of a crash, or a
    /// softlock at the end tick (T-7, D-1179).
    /// </summary>
    /// <param name="content">The content of this build.</param>
    /// <param name="path">The full path of the record file.</param>
    /// <param name="accepted">The source of the accepted intents, the same source as the run that failed.</param>
    /// <param name="output">The writer of a replay with no failure.</param>
    /// <param name="errors">The writer of the failure that repeats.</param>
    /// <returns><see cref="Program.FaultExitCode"/> when the failure repeats, else 0.</returns>
    public static int Replay(ContentSet content, string path, AcceptedSource accepted, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(accepted);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        RunRecord record = RunRecordStore.Read(path);
        RunState end;
        try
        {
            end = RunReplay.Play(record, content.Hash, MapSet.Of(content.Maps), content.Battle, content.Notices, content.Story, DebugIntentHandlers.None);
        }
        catch (SimulationException fault)
        {
            errors.WriteLine($"Error: the replay of '{path}' repeats the crash: {fault.Message}");
            return Program.FaultExitCode;
        }

        Simulation simulation = Simulation.Resume(record.Header.Seed, end.Snapshot(), MapSet.Of(content.Maps), content.Battle, content.Notices, content.Story, DebugIntentHandlers.None);
        if (SoftlockCheck.Holds(content, simulation, accepted(simulation)))
        {
            errors.WriteLine($"Error: the replay of '{path}' repeats the softlock at tick {simulation.Tick} (D-1179).");
            return Program.FaultExitCode;
        }

        output.WriteLine($"The replay of '{path}' reached tick {simulation.Tick} with no crash and no softlock.");
        return 0;
    }

    private static BotPlan? PlanOf(OptionParser options, TextWriter errors)
    {
        if (!BotPolicyKinds.TryOf(options.Value(PolicyOption) ?? string.Empty, out BotPolicyKind policy))
        {
            errors.WriteLine($"Error: {Name} needs {PolicyOption} with the value random or greedy.");
            return null;
        }

        if (!int.TryParse(options.Value(RunsOption) ?? string.Empty, NumberStyles.None, CultureInfo.InvariantCulture, out int runs) || runs < 1)
        {
            errors.WriteLine($"Error: {Name} needs {RunsOption} with a whole number of 1 or more.");
            return null;
        }

        if (!ulong.TryParse(options.ValueOr(FirstSeedOption, "1"), NumberStyles.None, CultureInfo.InvariantCulture, out ulong firstSeed))
        {
            errors.WriteLine($"Error: {Name} needs {FirstSeedOption} with a whole number of 0 or more.");
            return null;
        }

        if (options.Value(OutOption) is not string outFolder)
        {
            errors.WriteLine($"Error: {Name} needs {OutOption} with the output folder.");
            return null;
        }

        return new BotPlan(policy, runs, firstSeed, outFolder, options.ValueOr(LegOption, "local"));
    }

    private static BotResult PlayOne(ContentSet content, ulong seed, IBotPolicy policy, AcceptedSource accepted, string outFolder)
    {
        string saves = Path.Combine(outFolder, "saves", RecordNameOf(policy.Kind, seed));
        try
        {
            return BotRun.Play(content, seed, policy, accepted, saves);
        }
        finally
        {
            if (Directory.Exists(saves))
            {
                Directory.Delete(saves, recursive: true);
            }
        }
    }
}
