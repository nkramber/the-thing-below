using System;
using System.Collections.Generic;
using System.IO;

namespace TheThingBelow.Tools.Night;

/// <summary>
/// The `night-walk` command of PR-108 (D-1204). The night gate and the promotion run it to find
/// each commit of a PR whose night passes the head, and they read the newest night on those commits.
/// </summary>
public static class NightWalkCommand
{
    /// <summary>The name of the command.</summary>
    public const string Name = "night-walk";

    /// <summary>The option of the JSON file of the commits of the PR, oldest first.</summary>
    public const string CommitsOption = "--commits";

    /// <summary>The option of the head commit of the PR.</summary>
    public const string HeadOption = "--head";

    /// <summary>Runs the command, and writes each commit of the walk on its own line, newest first.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer of the commits.</param>
    /// <param name="errors">The writer of each error.</param>
    /// <returns>The exit code: 0 when the walk ran, else <see cref="Program.FaultExitCode"/>.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(Name, args, [CommitsOption, HeadOption], [], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string? commitsPath = options.Value(CommitsOption);
        string? head = options.Value(HeadOption);
        if (commitsPath is null || head is null)
        {
            errors.WriteLine($"Error: {Name} needs {CommitsOption} and {HeadOption}.");
            return Program.FaultExitCode;
        }

        IReadOnlyList<string> candidates;
        try
        {
            candidates = NightWalk.CandidatesOf(NightWalk.ReadCommits(commitsPath), head);
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            errors.WriteLine($"Error: {Name} stopped: {fault.Message}");
            return Program.FaultExitCode;
        }

        foreach (string candidate in candidates)
        {
            output.WriteLine(candidate);
        }

        return 0;
    }
}
