using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace TheThingBelow.Tools.ChangedPaths;

/// <summary>The answer of the `changed-paths` command, with the reason that the log shows.</summary>
/// <param name="DocumentsAlone">True when each job except ste-check, review-gate, and Gitar skips.</param>
/// <param name="Reason">Why the command gave that answer, for the reader of the CI log.</param>
public sealed record SkipDecision(bool DocumentsAlone, string Reason);

/// <summary>
/// The `changed-paths` command of the first CI job. The workflow collects the facts from git
/// and from GitHub, and this command decides whether the other jobs skip (D-595, D-856, D-859).
/// </summary>
public static class ChangedPathsCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = "changed-paths";

    /// <summary>The option that names the event of the run, such as `pull_request`.</summary>
    public const string EventOption = "--event-name";

    /// <summary>The option that names the file of the paths that the PR changes from its merge base.</summary>
    public const string PullRequestPathsOption = "--pr-paths";

    /// <summary>
    /// The option that names the file of the paths that the push changes from the previous head.
    /// The workflow writes it only when the previous head is an ancestor of the new head.
    /// </summary>
    public const string PushPathsOption = "--push-paths";

    /// <summary>The option that names the file of the check runs of the previous head, as JSON lines.</summary>
    public const string PreviousChecksOption = "--previous-checks";

    /// <summary>The option that names the output file of the step, which the command adds one line to.</summary>
    public const string OutputOption = "--output";

    /// <summary>The event of a pull request. Every other event runs every job (D-595).</summary>
    public const string PullRequestEvent = "pull_request";

    /// <summary>The name of the output that the other jobs read in their condition.</summary>
    public const string OutputName = "documents-alone";

    /// <summary>Reads the facts, decides, and writes the output line.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer that takes the decision and its reason.</param>
    /// <param name="errors">The writer that takes each fault of the run.</param>
    /// <returns>0 when the command gave a decision, and 1 on a fault.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(
            Name,
            args,
            [EventOption, PullRequestPathsOption, PushPathsOption, PreviousChecksOption, OutputOption],
            [],
            errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string? eventName = options.Value(EventOption);
        string? outputPath = options.Value(OutputOption);
        if (eventName is null || outputPath is null)
        {
            errors.WriteLine($"Error: {Name} needs {EventOption} and {OutputOption}.");
            return Program.FaultExitCode;
        }

        try
        {
            SkipDecision decision = DecideFromFiles(eventName, options);
            File.AppendAllText(outputPath, $"{OutputName}={(decision.DocumentsAlone ? "true" : "false")}\n");
            output.WriteLine(decision.Reason);
            output.WriteLine($"{OutputName}: {(decision.DocumentsAlone ? "true" : "false")}");
            return 0;
        }
        catch (Exception fault) when (fault is InvalidOperationException or IOException or UnauthorizedAccessException or JsonException)
        {
            errors.WriteLine($"Error: {Name} stopped. {fault.Message}");
            return Program.FaultExitCode;
        }
    }

    /// <summary>
    /// Decides whether the other jobs skip. A PR that changes docs alone skips them (D-595).
    /// A push that changes docs alone skips them when each skipped check passed on the previous
    /// head (D-856, D-858). Every other change runs every job.
    /// </summary>
    /// <param name="eventName">The event of the run.</param>
    /// <param name="pullRequestPaths">The paths of the PR from its merge base. Null on an event that is not a PR.</param>
    /// <param name="pushPaths">The paths of the push from the previous head, or null when no previous head is an ancestor.</param>
    /// <param name="previousChecks">The check runs of the previous head. Null exactly when the push paths are null.</param>
    /// <returns>The decision and its reason.</returns>
    /// <exception cref="InvalidOperationException">The facts do not fit the event.</exception>
    public static SkipDecision Decide(
        string eventName,
        IReadOnlyList<string>? pullRequestPaths,
        IReadOnlyList<string>? pushPaths,
        IReadOnlyList<CheckRunFacts>? previousChecks)
    {
        ArgumentException.ThrowIfNullOrEmpty(eventName);

        if (!string.Equals(eventName, PullRequestEvent, StringComparison.Ordinal))
        {
            if (pullRequestPaths is not null || pushPaths is not null || previousChecks is not null)
            {
                throw new InvalidOperationException(
                    $"The event '{eventName}' is not a PR, and it takes no path file and no check file (T-2).");
            }

            return new SkipDecision(false, $"The event '{eventName}' runs every job. A path rule skips a job of a PR alone (D-595).");
        }

        if (pullRequestPaths is null)
        {
            throw new InvalidOperationException($"A PR needs the file of {PullRequestPathsOption} (T-2).");
        }

        if ((pushPaths is null) != (previousChecks is null))
        {
            throw new InvalidOperationException(
                $"The options {PushPathsOption} and {PreviousChecksOption} go together, and the command got one of them (T-2).");
        }

        // An empty change skips nothing. An empty commit is the one way to run the checks again.
        if (pullRequestPaths.Count == 0)
        {
            return new SkipDecision(false, "The PR changes no path, so every job runs.");
        }

        string? codePath = DocumentsAlonePaths.FirstPathOutside(pullRequestPaths);
        if (codePath is null)
        {
            return new SkipDecision(true, "The PR changes docs alone, so each other job skips (D-595, D-857).");
        }

        if (pushPaths is null || previousChecks is null)
        {
            return new SkipDecision(
                false,
                $"The PR changes '{codePath}', and no previous head is an ancestor of this head. A new PR, a reopen, and a force push read the whole PR, so every job runs (D-856).");
        }

        if (pushPaths.Count == 0)
        {
            return new SkipDecision(false, "The push changes no path, so every job runs.");
        }

        string? pushCodePath = DocumentsAlonePaths.FirstPathOutside(pushPaths);
        if (pushCodePath is not null)
        {
            return new SkipDecision(false, $"The push changes '{pushCodePath}', so every job runs.");
        }

        IReadOnlyList<string> notPassed = PreviousHeadChecks.FindNotPassed(previousChecks);
        if (notPassed.Count > 0)
        {
            return new SkipDecision(
                false,
                $"The push changes docs alone, and the previous head did not pass: {string.Join(", ", notPassed)}. Every job runs (D-858).");
        }

        return new SkipDecision(
            true,
            "The push changes docs alone, and each skipped check passed on the previous head. Each other job skips (D-856, D-858).");
    }

    private static SkipDecision DecideFromFiles(string eventName, OptionParser options)
    {
        string? pullRequestPathsFile = options.Value(PullRequestPathsOption);
        string? pushPathsFile = options.Value(PushPathsOption);
        string? previousChecksFile = options.Value(PreviousChecksOption);

        IReadOnlyList<string>? pullRequestPaths = pullRequestPathsFile is null ? null : ReadPaths(pullRequestPathsFile);
        IReadOnlyList<string>? pushPaths = pushPathsFile is null ? null : ReadPaths(pushPathsFile);
        IReadOnlyList<CheckRunFacts>? previousChecks =
            previousChecksFile is null ? null : PreviousHeadChecks.Read(previousChecksFile);

        return Decide(eventName, pullRequestPaths, pushPaths, previousChecks);
    }

    private static List<string> ReadPaths(string path)
    {
        List<string> paths = [];
        foreach (string line in File.ReadAllLines(path))
        {
            if (line.Length > 0)
            {
                paths.Add(line);
            }
        }

        return paths;
    }
}
