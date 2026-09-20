using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TheThingBelow.Tools.SteCheck;

/// <summary>
/// The files that git tracks in a checkout. The `ste-check` command reads these files alone,
/// so an untracked note fails no local run and no commit (D-702).
/// </summary>
public static class TrackedFiles
{
    /// <summary>Reads whether the root holds git data, as a `.git` folder or a `.git` file.</summary>
    /// <param name="fullRoot">The full path of the root of the checkout.</param>
    /// <returns>True when git tracks the files of this root. A test fixture gives false.</returns>
    public static bool HoldsGitData(string fullRoot)
    {
        ArgumentException.ThrowIfNullOrEmpty(fullRoot);

        // A linked worktree and a submodule each hold `.git` as a file that names the folder.
        string gitPath = Path.Combine(fullRoot, ".git");
        return Directory.Exists(gitPath) || File.Exists(gitPath);
    }

    /// <summary>Lists each file of the index, which holds a staged new file too.</summary>
    /// <param name="fullRoot">The full path of the root of the checkout.</param>
    /// <returns>Each tracked path from the root, with forward slashes, in the order of git.</returns>
    /// <exception cref="InvalidOperationException">
    /// The `git` program did not start, or it gave an exit code other than 0. No read of the
    /// folder tree replaces a run that fails (T-2).
    /// </exception>
    public static IReadOnlyList<string> List(string fullRoot)
    {
        ArgumentException.ThrowIfNullOrEmpty(fullRoot);

        // The `-z` option ends each path with a NUL character, and git then quotes no path.
        ProcessStartInfo start = new ProcessStartInfo("git")
        {
            WorkingDirectory = fullRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };
        start.ArgumentList.Add("ls-files");
        start.ArgumentList.Add("-z");

        using Process process = Start(start, fullRoot);

        // The error stream drains at the same time as the output, or a full pipe stops git.
        Task<string> errorText = process.StandardError.ReadToEndAsync();
        string pathText = process.StandardOutput.ReadToEnd();
        string reason = errorText.GetAwaiter().GetResult().Trim();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"`git ls-files` gave the exit code {process.ExitCode} in the root '{fullRoot}' (T-2, D-702). {reason}");
        }

        return pathText.Split('\0', StringSplitOptions.RemoveEmptyEntries);
    }

    private static Process Start(ProcessStartInfo start, string fullRoot)
    {
        try
        {
            Process? process = Process.Start(start);
            if (process is null)
            {
                throw new InvalidOperationException(
                    $"The `git` program gave no process for the root '{fullRoot}' (T-2, D-702).");
            }

            return process;
        }
        catch (Win32Exception fault)
        {
            throw new InvalidOperationException(
                $"The root '{fullRoot}' holds git data, and the `git` program did not start (T-2, D-702). {fault.Message}",
                fault);
        }
    }
}
