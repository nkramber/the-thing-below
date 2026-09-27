using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Tools.CodexReview;

namespace TheThingBelow.Tools.Watch;

/// <summary>
/// The `night-watch-install` command of PR-108 (D-1205). It publishes Tools into the folder of the
/// watcher, writes the launchd file, and loads the job. A new run replaces the job, so the owner
/// runs it again after a change of the watcher reaches `main`.
/// </summary>
public static class NightWatchInstallCommand
{
    /// <summary>The name of the command.</summary>
    public const string Name = "night-watch-install";

    /// <summary>The time that the publish of Tools can take.</summary>
    public static readonly TimeSpan PublishLimit = TimeSpan.FromMinutes(10);

    /// <summary>Runs the command.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer of each step.</param>
    /// <param name="errors">The writer of each error.</param>
    /// <returns>The exit code: 0 when the job loaded, else <see cref="Program.FaultExitCode"/>.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(Name, args, [NightWatchCommand.RepositoryOption, NightWatchCommand.StateOption], [], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string? repository = options.Value(NightWatchCommand.RepositoryOption);
        string? state = options.Value(NightWatchCommand.StateOption);
        if (repository is null || state is null)
        {
            errors.WriteLine($"Error: {Name} needs {NightWatchCommand.RepositoryOption} and {NightWatchCommand.StateOption}.");
            return Program.FaultExitCode;
        }

        try
        {
            Install(Path.GetFullPath(repository), Path.GetFullPath(state), output);
            return 0;
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            errors.WriteLine($"Error: {Name} stopped: {fault.Message}");
            return Program.FaultExitCode;
        }
    }

    /// <summary>Finds a program in the folders of a PATH.</summary>
    /// <param name="program">The name of the program.</param>
    /// <param name="pathVariable">The PATH.</param>
    /// <returns>The full path of the first match.</returns>
    /// <exception cref="InvalidOperationException">No folder of the PATH holds the program (T-2).</exception>
    public static string Find(string program, string pathVariable)
    {
        ArgumentException.ThrowIfNullOrEmpty(program);
        ArgumentNullException.ThrowIfNull(pathVariable);
        foreach (string folder in pathVariable.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            string candidate = Path.Combine(folder, program);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException($"No folder of the PATH holds the program '{program}', and the watcher needs it (D-1205, T-2).");
    }

    private static void Install(string repository, string state, TextWriter output)
    {
        string home = Environment.GetEnvironmentVariable("HOME")
            ?? throw new InvalidOperationException("The variable HOME is absent, and the launchd file goes under it (T-2).");
        string pathVariable = Environment.GetEnvironmentVariable("PATH")
            ?? throw new InvalidOperationException("The variable PATH is absent, and the job needs it (T-2).");

        // Each program of the loop must be on the PATH of the job: the check fails at the install,
        // not in the middle of a night (T-2).
        foreach (string program in new[] { "gh", "git", "claude", "codex", "make" })
        {
            output.WriteLine($"Found {program} at {Find(program, pathVariable)}.");
        }

        string dotnet = Find("dotnet", pathVariable);
        string bin = Path.Combine(state, "bin");
        Directory.CreateDirectory(state);
        ExternalProgram.RunChecked(
            dotnet,
            ["publish", Path.Combine(repository, "TheThingBelow.Tools", "TheThingBelow.Tools.csproj"), "--configuration", "Release", "--output", bin, "--verbosity", "quiet"],
            repository,
            PublishLimit);
        output.WriteLine($"Published Tools to {bin}.");

        NightWatchJob job = new(dotnet, Path.Combine(bin, "TheThingBelow.Tools.dll"), repository, state, pathVariable, home);
        string plist = NightWatchPlist.PathOf(home);
        Directory.CreateDirectory(Path.GetDirectoryName(plist)!);
        File.WriteAllText(plist, NightWatchPlist.TextOf(job));
        output.WriteLine($"Wrote {plist}.");

        string user = ExternalProgram.RunChecked("id", ["-u"], state);
        ProgramResult bootout = ExternalProgram.Run("launchctl", ["bootout", $"gui/{user}/{NightWatchPlist.Label}"], state, null, ExternalProgram.StepLimit);
        output.WriteLine(bootout.ExitCode == 0 ? "Unloaded the earlier job." : "No earlier job was loaded.");
        ExternalProgram.RunChecked("launchctl", ["bootstrap", $"gui/{user}", plist], state);
        output.WriteLine($"Loaded the job {NightWatchPlist.Label}. It starts at minute 0 and minute 30 of each hour.");
    }
}
