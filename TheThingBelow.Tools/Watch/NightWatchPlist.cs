using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;

namespace TheThingBelow.Tools.Watch;

/// <summary>The values of the launchd file of the watcher (D-1205).</summary>
/// <param name="Dotnet">The full path of the `dotnet` program.</param>
/// <param name="ToolsAssembly">The full path of the published assembly of Tools.</param>
/// <param name="Repository">The full path of the checkout of the repository.</param>
/// <param name="State">The full path of the folder of the watcher.</param>
/// <param name="PathVariable">The PATH of the job, with gh, git, claude, codex, dotnet, and make.</param>
/// <param name="Home">The home folder of the owner, where gh and Claude Code keep their settings.</param>
public sealed record NightWatchJob(string Dotnet, string ToolsAssembly, string Repository, string State, string PathVariable, string Home);

/// <summary>
/// The launchd file of the watcher (D-1205). The job starts at minute 0 and minute 30 of each
/// hour of the local clock. The offset of US Central time is a whole count of hours, so the first
/// start after the night is 05:30 UTC.
/// </summary>
public static class NightWatchPlist
{
    /// <summary>The label of the launchd job.</summary>
    public const string Label = "com.thethingbelow.night-watch";

    /// <summary>The minutes of each hour at which the job starts.</summary>
    public static readonly IReadOnlyList<int> StartMinutes = [0, 30];

    /// <summary>Gives the path of the launchd file in the folder of the agents of the owner.</summary>
    /// <param name="home">The home folder of the owner.</param>
    /// <returns>The path.</returns>
    public static string PathOf(string home)
    {
        ArgumentException.ThrowIfNullOrEmpty(home);
        return Path.Combine(home, "Library", "LaunchAgents", Label + ".plist");
    }

    /// <summary>Gives the text of the launchd file.</summary>
    /// <param name="job">The values of the job.</param>
    /// <returns>The text, with each value escaped for XML.</returns>
    public static string TextOf(NightWatchJob job)
    {
        ArgumentNullException.ThrowIfNull(job);

        StringBuilder text = new();
        text.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
        text.Append("<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\n");
        text.Append("<plist version=\"1.0\">\n<dict>\n");
        text.Append($"  <key>Label</key>\n  <string>{Label}</string>\n");
        text.Append("  <key>ProgramArguments</key>\n  <array>\n");
        foreach (string argument in new[] { job.Dotnet, job.ToolsAssembly, NightWatchCommand.Name, NightWatchCommand.RepositoryOption, job.Repository, NightWatchCommand.StateOption, job.State })
        {
            text.Append($"    <string>{Escape(argument)}</string>\n");
        }

        text.Append("  </array>\n  <key>StartCalendarInterval</key>\n  <array>\n");
        foreach (int minute in StartMinutes)
        {
            text.Append($"    <dict>\n      <key>Minute</key>\n      <integer>{minute}</integer>\n    </dict>\n");
        }

        text.Append("  </array>\n  <key>EnvironmentVariables</key>\n  <dict>\n");
        text.Append($"    <key>PATH</key>\n    <string>{Escape(job.PathVariable)}</string>\n");
        text.Append($"    <key>HOME</key>\n    <string>{Escape(job.Home)}</string>\n");
        text.Append("  </dict>\n");
        // launchd reads macOS paths alone, so each log path joins with a slash on every host of the tests.
        text.Append($"  <key>WorkingDirectory</key>\n  <string>{Escape(job.State)}</string>\n");
        text.Append($"  <key>StandardOutPath</key>\n  <string>{Escape(job.State + "/launchd-out.log")}</string>\n");
        text.Append($"  <key>StandardErrorPath</key>\n  <string>{Escape(job.State + "/launchd-error.log")}</string>\n");
        text.Append("</dict>\n</plist>\n");
        return text.ToString();
    }

    private static string Escape(string value) => SecurityElement.Escape(value);
}
