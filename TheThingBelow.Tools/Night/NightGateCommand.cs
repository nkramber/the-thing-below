using System;
using System.Collections.Generic;
using System.IO;

namespace TheThingBelow.Tools.Night;

/// <summary>
/// The `night-gate` command of PR-49 and PR-108 (G-22, D-509, D-510, D-513, D-1188, D-1202, D-1204). It reads the facts of a
/// PR and the nights that the gate job downloaded through the GitHub API, and it fails a PR with
/// no success record from a night inside 48 hours.
/// </summary>
/// <remarks>
/// The command reads no file of the checkout of the PR. The gate job runs on
/// `pull_request_target`, so the job and this command come from `main` (D-509, F-37).
/// </remarks>
public static class NightGateCommand
{
    /// <summary>The name of the command.</summary>
    public const string Name = "night-gate";

    /// <summary>The option of the JSON file of the facts of the PR.</summary>
    public const string PullRequestOption = "--pull-request";

    /// <summary>The option of the folder of the newest completed night on `main`.</summary>
    public const string MainNightOption = "--main-night";

    /// <summary>The option of the JSON file of the commits of the PR, oldest first (D-1204).</summary>
    public const string CommitsOption = "--commits";

    /// <summary>The option of the folder of the newest promotion of `main` (D-1202).</summary>
    public const string PromotionOption = "--promotion";

    /// <summary>The option of the compare status of the merge commit of the promotion against the commit of the night of `main`. It is absent unless both exist.</summary>
    public const string PromotionOrderOption = "--promotion-order";

    /// <summary>The option of the folder of the newest completed night on a commit of the walk of D-1204.</summary>
    public const string HeadNightOption = "--head-night";

    /// <summary>The option of the time of the run of the gate, in UTC, such as `2026-09-27T09:00:00Z`.</summary>
    public const string NowOption = "--now";

    /// <summary>Runs the command.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer of the lines of a pass.</param>
    /// <param name="errors">The writer of the lines of a failure and of each error.</param>
    /// <returns>The exit code: 0 when the PR passes, else <see cref="Program.FaultExitCode"/>.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(Name, args, [PullRequestOption, CommitsOption, MainNightOption, PromotionOption, PromotionOrderOption, HeadNightOption, NowOption], [], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string? pullRequestPath = options.Value(PullRequestOption);
        string? commitsPath = options.Value(CommitsOption);
        string? mainFolder = options.Value(MainNightOption);
        string? promotionFolder = options.Value(PromotionOption);
        string? headFolder = options.Value(HeadNightOption);
        string? nowText = options.Value(NowOption);
        if (pullRequestPath is null || commitsPath is null || mainFolder is null || promotionFolder is null || headFolder is null || nowText is null)
        {
            errors.WriteLine($"Error: {Name} needs {PullRequestOption}, {CommitsOption}, {MainNightOption}, {PromotionOption}, {HeadNightOption}, and {NowOption}. {PromotionOrderOption} comes with a night of `main` and a promotion alone.");
            return Program.FaultExitCode;
        }

        if (!NightJson.TryTime(nowText, out DateTimeOffset now))
        {
            errors.WriteLine($"Error: {Name} needs {NowOption} with a UTC time such as 2026-09-27T09:00:00Z, and it read '{nowText}'.");
            return Program.FaultExitCode;
        }

        NightVerdict verdict;
        try
        {
            NightPullRequest pullRequest = NightPullRequest.Read(pullRequestPath);
            IReadOnlyList<string> candidates = NightWalk.CandidatesOf(NightWalk.ReadCommits(commitsPath), pullRequest.Head);
            NightMainFacts main = MainFactsOf(mainFolder, promotionFolder, options.Value(PromotionOrderOption));
            verdict = NightGate.Check(pullRequest, candidates, main, NightEvidence.Read(headFolder), now);
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            errors.WriteLine($"Error: {Name} stopped: {fault.Message}");
            return Program.FaultExitCode;
        }

        if (verdict.Passes)
        {
            foreach (string line in verdict.Lines)
            {
                output.WriteLine(line);
            }

            output.WriteLine("The night gate passes.");
            return 0;
        }

        foreach (string line in verdict.Lines)
        {
            errors.WriteLine(line);
        }

        errors.WriteLine("Error: the night gate fails. `docs/runbooks/night.md` gives the steps.");
        return Program.FaultExitCode;
    }

    /// <summary>Reads the evidence of `main`: the newest night, the newest promotion, and the order of their commits (D-1202).</summary>
    /// <param name="mainFolder">The folder of the newest completed night of `main`.</param>
    /// <param name="promotionFolder">The folder of the newest promotion.</param>
    /// <param name="orderText">The compare status of the two commits, or null when either is absent.</param>
    /// <returns>The facts.</returns>
    /// <exception cref="InvalidOperationException">A folder holds no valid facts, or the order does not fit the pair (T-2).</exception>
    public static NightMainFacts MainFactsOf(string mainFolder, string promotionFolder, string? orderText)
    {
        NightOrder order = orderText is null ? NightOrder.None : NightPromotion.OrderOf(orderText);
        return NightMainFacts.Of(NightEvidence.Read(mainFolder), NightPromotion.Read(promotionFolder), order);
    }
}
