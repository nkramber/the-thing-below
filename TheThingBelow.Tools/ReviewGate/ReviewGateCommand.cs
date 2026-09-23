using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace TheThingBelow.Tools.ReviewGate;

/// <summary>
/// The `review-gate` command. It reads the facts of one pull request and the files of its head,
/// and it gives one line for each rule from RG 1 to RG 8 (D-15, D-579). The workflow runs the
/// command from `main` on `pull_request_target`, and the command never runs code of the head.
/// </summary>
public static class ReviewGateCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = "review-gate";

    /// <summary>The option that names the JSON file with the facts of the pull request.</summary>
    public const string PullRequestOption = "--pull-request";

    /// <summary>The option that names the folder with the files of the head.</summary>
    public const string HeadFilesOption = "--head-files";

    /// <summary>Reads one pull request and writes the result of each rule.</summary>
    /// <param name="args">The arguments after the command name. Both options are necessary.</param>
    /// <param name="output">The writer that takes the report.</param>
    /// <param name="errors">The writer that takes each fault of the run.</param>
    /// <returns>0 when every rule passes, and 1 when one rule finds a fault.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(Name, args, [PullRequestOption, HeadFilesOption], [], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string? factsPath = options.Value(PullRequestOption);
        string? headFilesRoot = options.Value(HeadFilesOption);
        if (factsPath is null || headFilesRoot is null)
        {
            errors.WriteLine(
                $"Error: {Name} needs {PullRequestOption} <file> and {HeadFilesOption} <folder>. An absent option is an error (T-2).");
            return Program.FaultExitCode;
        }

        try
        {
            return Report(factsPath, headFilesRoot, output, errors);
        }
        catch (Exception fault) when (fault is InvalidOperationException or JsonException or IOException or UnauthorizedAccessException)
        {
            errors.WriteLine($"Error: {Name} stopped on '{factsPath}'. {fault.Message}");
            return Program.FaultExitCode;
        }
    }

    /// <summary>Runs every rule against one pull request.</summary>
    /// <param name="facts">The facts of the pull request.</param>
    /// <param name="headFilesRoot">The folder that holds the files of the head, as data alone.</param>
    /// <returns>The result of each rule, from RG 1 to RG 8.</returns>
    public static IReadOnlyList<GateCheck> Check(PullRequestFacts facts, string headFilesRoot)
    {
        ArgumentNullException.ThrowIfNull(facts);
        ArgumentException.ThrowIfNullOrEmpty(headFilesRoot);

        GateCheck paths = OverrideRules.CheckPaths(facts);
        GateCheck rows = OverrideRules.CheckDecisionRows(facts);
        List<GateCheck> checks = [paths, rows];

        bool labelCovers = OverrideRules.CarriesLabel(facts)
            && paths.Result == GateResult.Pass
            && rows.Result == GateResult.Pass;
        if (labelCovers)
        {
            string reason =
                $"the `{OverrideRules.LabelName}` label covers this PR, so the review of the other provider is not necessary (D-401).";
            checks.Add(new GateCheck("RG 3", GateResult.Skip, reason));
            checks.Add(new GateCheck("RG 4", GateResult.Skip, reason));
            checks.Add(new GateCheck("RG 5", GateResult.Skip, reason));
        }
        else
        {
            checks.AddRange(ReviewRecordRules.Check(
                facts,
                headFilesRoot,
                EffectiveHead.ReviewableHeads(facts.Commits, facts.Number)));
        }

        checks.Add(DocumentRules.CheckHandoff(facts));
        checks.AddRange(DocumentRules.CheckSection(facts));
        return checks;
    }

    private static int Report(string factsPath, string headFilesRoot, TextWriter output, TextWriter errors)
    {
        PullRequestFacts facts = PullRequestFacts.Read(factsPath);
        if (!Directory.Exists(headFilesRoot))
        {
            errors.WriteLine($"Error: the folder '{headFilesRoot}' of {HeadFilesOption} does not exist.");
            return Program.FaultExitCode;
        }

        IReadOnlyList<GateCheck> checks = Check(facts, headFilesRoot);
        string number = facts.Number.ToString(CultureInfo.InvariantCulture);
        output.WriteLine(
            $"{Name}: PR #{number}, {facts.Files.Count} changed path(s), {facts.Commits.Count} commit(s).");

        int faults = 0;
        foreach (GateCheck check in checks)
        {
            output.WriteLine(check.ToString());
            if (check.Result == GateResult.Fault)
            {
                faults++;
            }
        }

        output.WriteLine(faults == 0 ? $"{Name}: pass" : $"{Name}: {faults} fault(s)");
        return faults > 0 ? Program.FaultExitCode : 0;
    }
}
