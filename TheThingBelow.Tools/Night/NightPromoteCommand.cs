using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace TheThingBelow.Tools.Night;

/// <summary>
/// The `night-promote` command of PR-108 (D-1202, D-1203). The `night-promote` workflow runs it on
/// each push to `main`. It writes the folder of the artifact `night-promotion` when the push
/// promotes the night of its PR, and it writes nothing when the push promotes nothing.
/// </summary>
public static class NightPromoteCommand
{
    /// <summary>The name of the command.</summary>
    public const string Name = "night-promote";

    /// <summary>The option of the pushed commit of `main`.</summary>
    public const string MergeCommitOption = "--merge-commit";

    /// <summary>The option of the number of the merged PR. It is absent when the push holds no merged PR.</summary>
    public const string PullRequestOption = "--pull-request";

    /// <summary>The option of the JSON file of the commits of the PR, oldest first. It comes with the PR alone.</summary>
    public const string CommitsOption = "--commits";

    /// <summary>The option of the folder of the newest completed night of `main`.</summary>
    public const string MainNightOption = "--main-night";

    /// <summary>The option of the folder of the newest promotion before the push.</summary>
    public const string PromotionOption = "--promotion";

    /// <summary>The option of the compare status of the promotion against the night of `main`. It is absent unless both exist.</summary>
    public const string PromotionOrderOption = "--promotion-order";

    /// <summary>The option of the folder of the newest completed night on a commit of the walk of the PR.</summary>
    public const string BranchNightOption = "--branch-night";

    /// <summary>The option of the text file of each path in which the tree of the merge commit differs from the tree of the night commit.</summary>
    public const string TreeDifferenceOption = "--tree-difference";

    /// <summary>The option of the time of the check, in UTC.</summary>
    public const string NowOption = "--now";

    /// <summary>The option of the new folder of the artifact of the promotion.</summary>
    public const string OutOption = "--out";

    /// <summary>Runs the command.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer of each line of the check.</param>
    /// <param name="errors">The writer of each error.</param>
    /// <returns>The exit code: 0 when the check ran, with or with no promotion, else <see cref="Program.FaultExitCode"/>.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(
            Name,
            args,
            [MergeCommitOption, PullRequestOption, CommitsOption, MainNightOption, PromotionOption, PromotionOrderOption, BranchNightOption, TreeDifferenceOption, NowOption, OutOption],
            [],
            errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string? merge = options.Value(MergeCommitOption);
        string? mainFolder = options.Value(MainNightOption);
        string? promotionFolder = options.Value(PromotionOption);
        string? branchFolder = options.Value(BranchNightOption);
        string? nowText = options.Value(NowOption);
        string? outFolder = options.Value(OutOption);
        if (merge is null || mainFolder is null || promotionFolder is null || branchFolder is null || nowText is null || outFolder is null)
        {
            errors.WriteLine($"Error: {Name} needs {MergeCommitOption}, {MainNightOption}, {PromotionOption}, {BranchNightOption}, {NowOption}, and {OutOption}.");
            return Program.FaultExitCode;
        }

        if (!NightJson.IsCommit(merge) || !NightJson.TryTime(nowText, out DateTimeOffset now))
        {
            errors.WriteLine($"Error: {Name} needs {MergeCommitOption} with a full commit hash and {NowOption} with a UTC time, and it read '{merge}' and '{nowText}'.");
            return Program.FaultExitCode;
        }

        if (Directory.Exists(outFolder))
        {
            errors.WriteLine($"Error: {Name} writes the new folder '{outFolder}', and the folder exists (T-2).");
            return Program.FaultExitCode;
        }

        NightPromoteVerdict verdict;
        try
        {
            verdict = NightPromote.Check(FactsOf(options, merge, mainFolder, promotionFolder, branchFolder), now);
            foreach (string line in verdict.Lines)
            {
                output.WriteLine(line);
            }

            if (verdict.Promotion is null)
            {
                output.WriteLine("The push promotes nothing.");
                return 0;
            }

            verdict.Promotion.Write(branchFolder, outFolder);
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            errors.WriteLine($"Error: {Name} stopped: {fault.Message}");
            return Program.FaultExitCode;
        }

        output.WriteLine($"The promotion is in '{outFolder}'.");
        return 0;
    }

    private static NightPromoteFacts FactsOf(OptionParser options, string merge, string mainFolder, string promotionFolder, string branchFolder)
    {
        NightMainFacts main = NightGateCommand.MainFactsOf(mainFolder, promotionFolder, options.Value(PromotionOrderOption));
        string? pullRequestText = options.Value(PullRequestOption);
        if (pullRequestText is null)
        {
            return new NightPromoteFacts(merge, null, main, null, [], []);
        }

        if (!long.TryParse(pullRequestText, NumberStyles.None, CultureInfo.InvariantCulture, out long pullRequest) || pullRequest < 1)
        {
            throw new InvalidOperationException($"{PullRequestOption} holds '{pullRequestText}', and a PR number is a whole number from 1 (T-2).");
        }

        string commitsPath = options.Value(CommitsOption)
            ?? throw new InvalidOperationException($"{PullRequestOption} comes with {CommitsOption}, the commits of the PR (D-1204, T-2).");
        IReadOnlyList<NightCommit> commits = NightWalk.ReadCommits(commitsPath);
        IReadOnlyList<string> candidates = NightWalk.CandidatesOf(commits, commits[^1].Sha);
        NightEvidence? night = NightEvidence.Read(branchFolder);
        if (night is null)
        {
            return new NightPromoteFacts(merge, pullRequest, main, null, candidates, []);
        }

        string treePath = options.Value(TreeDifferenceOption)
            ?? throw new InvalidOperationException($"A branch night comes with {TreeDifferenceOption}, the paths in which the two trees differ (D-1202, T-2).");
        List<string> paths = [];
        foreach (string line in File.ReadAllLines(treePath))
        {
            if (line.Trim().Length > 0)
            {
                paths.Add(line.Trim());
            }
        }

        return new NightPromoteFacts(merge, pullRequest, main, night, candidates, paths);
    }
}
