using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using TheThingBelow.Tools.Notify;

namespace TheThingBelow.Tools.Night;

/// <summary>
/// The `night-alert` command of PR-108 (D-1201). The alert job of the night workflow runs it after
/// a leg fails. It sends one Pushover message with each failed leg, the first seed, and the link.
/// </summary>
public static class NightAlertCommand
{
    /// <summary>The name of the command.</summary>
    public const string Name = "night-alert";

    /// <summary>The option of the JSON file of the jobs of the night run.</summary>
    public const string JobsOption = "--jobs";

    /// <summary>The option of the first seed. It is absent when no leg reached the seed step.</summary>
    public const string FirstSeedOption = "--first-seed";

    /// <summary>The option of the branch of the night run.</summary>
    public const string BranchOption = "--branch";

    /// <summary>The option of the commit of the night run.</summary>
    public const string CommitOption = "--commit";

    /// <summary>The option of the link of the night run.</summary>
    public const string RunLinkOption = "--run-link";

    /// <summary>Runs the command with the network and the environment of this process.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer of the line of a sent message.</param>
    /// <param name="errors">The writer of each error.</param>
    /// <returns>The exit code: 0 when Pushover took the alert, else <see cref="Program.FaultExitCode"/>.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        using SocketsHttpHandler handler = new();
        return Run(args, output, errors, Environment.GetEnvironmentVariable, handler);
    }

    /// <summary>Runs the command with the given environment and HTTP handler.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer of the line of a sent message.</param>
    /// <param name="errors">The writer of each error.</param>
    /// <param name="variable">Gives the value of one variable of the environment, or null.</param>
    /// <param name="handler">The HTTP handler: the network in a command, and a fake in a test.</param>
    /// <returns>The exit code: 0 when Pushover took the alert, else <see cref="Program.FaultExitCode"/>.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors, Func<string, string?> variable, HttpMessageHandler handler)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(Name, args, [JobsOption, FirstSeedOption, BranchOption, CommitOption, RunLinkOption], [], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string? jobsPath = options.Value(JobsOption);
        string? branch = options.Value(BranchOption);
        string? commit = options.Value(CommitOption);
        string? runLink = options.Value(RunLinkOption);
        if (jobsPath is null || branch is null || commit is null || runLink is null)
        {
            errors.WriteLine($"Error: {Name} needs {JobsOption}, {BranchOption}, {CommitOption}, and {RunLinkOption}. {FirstSeedOption} is absent when no leg reached the seed step.");
            return Program.FaultExitCode;
        }

        ulong? firstSeed = null;
        string? seedText = options.Value(FirstSeedOption);
        if (seedText is not null)
        {
            if (!ulong.TryParse(seedText, NumberStyles.None, CultureInfo.InvariantCulture, out ulong seed))
            {
                errors.WriteLine($"Error: {Name} needs {FirstSeedOption} with a whole number, and it read '{seedText}' (D-1190).");
                return Program.FaultExitCode;
            }

            firstSeed = seed;
        }

        PushoverMessage message;
        try
        {
            message = NightAlert.MessageOf(new NightAlertFacts(NightAlert.ReadJobs(jobsPath), firstSeed, branch, commit, runLink));
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            errors.WriteLine($"Error: {Name} stopped: {fault.Message}");
            return Program.FaultExitCode;
        }

        return PushoverCommand.Send(message, output, errors, variable, handler, Name);
    }
}
