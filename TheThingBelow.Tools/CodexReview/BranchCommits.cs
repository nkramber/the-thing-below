using System;
using System.Collections.Generic;
using TheThingBelow.Tools.ReviewGate;

namespace TheThingBelow.Tools.CodexReview;

/// <summary>
/// The commits of a pull request, as git lists them. The `codex-review` command reads the
/// effective head from them with the rule of the review gate (D-610).
/// </summary>
public static class BranchCommits
{
    /// <summary>The start of the line that names each commit in the format of <see cref="LogArguments"/>.</summary>
    public const string CommitMarker = "commit:";

    /// <summary>Gives the arguments of the `git log` run that lists the commits of a range, oldest first.</summary>
    /// <param name="range">The range, such as `abc1234..origin/feat/pr-95-codex-review`.</param>
    /// <returns>The arguments after the program name.</returns>
    public static IReadOnlyList<string> LogArguments(string range)
    {
        ArgumentException.ThrowIfNullOrEmpty(range);
        return ["log", "--reverse", "--no-renames", $"--format={CommitMarker}%H", "--name-only", range];
    }

    /// <summary>Reads the output of the `git log` run of <see cref="LogArguments"/>.</summary>
    /// <param name="text">The output: one line for each commit, then one line for each path that it changes.</param>
    /// <returns>Each commit with its paths, oldest first.</returns>
    /// <exception cref="InvalidOperationException">A path comes before the first commit line (T-2).</exception>
    public static IReadOnlyList<CommitFacts> Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        List<CommitFacts> commits = [];
        string? sha = null;
        List<string> files = [];
        foreach (string line in GitarPass.SplitLines(text))
        {
            if (line.StartsWith(CommitMarker, StringComparison.Ordinal))
            {
                if (sha is not null)
                {
                    commits.Add(new CommitFacts(sha, files));
                }

                sha = line[CommitMarker.Length..];
                files = [];
            }
            else if (sha is null)
            {
                throw new InvalidOperationException($"The `git log` output names the path '{line}' before a commit (T-2).");
            }
            else
            {
                files.Add(line);
            }
        }

        if (sha is not null)
        {
            commits.Add(new CommitFacts(sha, files));
        }

        return commits;
    }
}
