using System;
using System.Collections.Generic;
using System.IO;

namespace TheThingBelow.Tools.Night;

/// <summary>
/// The `night-gate` command of PR-49 (G-22, D-509, D-510, D-513, D-1188). It reads the facts of a
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

    /// <summary>The option of the folder of the newest completed night on the head commit.</summary>
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

        OptionParser? options = OptionParser.Read(Name, args, [PullRequestOption, MainNightOption, HeadNightOption, NowOption], [], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string? pullRequestPath = options.Value(PullRequestOption);
        string? mainFolder = options.Value(MainNightOption);
        string? headFolder = options.Value(HeadNightOption);
        string? nowText = options.Value(NowOption);
        if (pullRequestPath is null || mainFolder is null || headFolder is null || nowText is null)
        {
            errors.WriteLine($"Error: {Name} needs {PullRequestOption}, {MainNightOption}, {HeadNightOption}, and {NowOption}.");
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
            verdict = NightGate.Check(NightPullRequest.Read(pullRequestPath), NightEvidence.Read(mainFolder), NightEvidence.Read(headFolder), now);
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
}
