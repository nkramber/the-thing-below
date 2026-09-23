using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TheThingBelow.Tools.CodexReview;

/// <summary>The result of one run of an external program.</summary>
/// <param name="ExitCode">The exit code of the program.</param>
/// <param name="Output">The standard output, or an empty text when a file took it.</param>
/// <param name="Error">The standard error.</param>
public sealed record ProgramResult(int ExitCode, string Output, string Error);

/// <summary>
/// Runs the programs of the `codex-review` command: git, gh, npm, and the Codex CLI. The
/// standard input of each program is closed, so no program waits for an answer, and the Codex
/// CLI adds no stdin block to the prompt (D-926).
/// </summary>
public static class ExternalProgram
{
    /// <summary>Runs one program to its end.</summary>
    /// <param name="program">The program name or its full path.</param>
    /// <param name="arguments">Each argument, with no shell between them.</param>
    /// <param name="workingDirectory">The folder in which the program runs.</param>
    /// <param name="outputFile">
    /// The file that takes the standard output as the program writes it, or null to keep the
    /// output in the result.
    /// </param>
    /// <returns>The exit code and the text of the two streams.</returns>
    /// <exception cref="InvalidOperationException">The program did not start (T-2).</exception>
    public static ProgramResult Run(
        string program,
        IReadOnlyList<string> arguments,
        string workingDirectory,
        string? outputFile)
    {
        ArgumentException.ThrowIfNullOrEmpty(program);
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentException.ThrowIfNullOrEmpty(workingDirectory);

        ProcessStartInfo start = new ProcessStartInfo(program)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };
        foreach (string argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using Process process = Start(start, program, workingDirectory);
        process.StandardInput.Close();

        // Both streams drain at the same time, or a full pipe stops the program.
        Task<string> errorText = process.StandardError.ReadToEndAsync();
        string output = string.Empty;
        if (outputFile is null)
        {
            output = process.StandardOutput.ReadToEnd();
        }
        else
        {
            using FileStream file = File.Create(outputFile);
            process.StandardOutput.BaseStream.CopyTo(file);
        }

        string error = errorText.GetAwaiter().GetResult();
        process.WaitForExit();
        return new ProgramResult(process.ExitCode, output, error);
    }

    /// <summary>Runs one program, and fails when its exit code is not 0.</summary>
    /// <param name="program">The program name or its full path.</param>
    /// <param name="arguments">Each argument, with no shell between them.</param>
    /// <param name="workingDirectory">The folder in which the program runs.</param>
    /// <returns>The standard output, with the white space at each end removed.</returns>
    /// <exception cref="InvalidOperationException">
    /// The program did not start, or it gave an exit code other than 0. The message names the
    /// command, the folder, the code, and the error text (T-2).
    /// </exception>
    public static string RunChecked(string program, IReadOnlyList<string> arguments, string workingDirectory)
    {
        ProgramResult result = Run(program, arguments, workingDirectory, null);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"`{Describe(program, arguments)}` gave the exit code {result.ExitCode} in '{workingDirectory}'. {result.Error.Trim()}");
        }

        return result.Output.Trim();
    }

    /// <summary>Gives the text of one command line, for a message.</summary>
    /// <param name="program">The program name or its full path.</param>
    /// <param name="arguments">Each argument.</param>
    /// <returns>The program and its arguments, with one space between each part.</returns>
    public static string Describe(string program, IReadOnlyList<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        return arguments.Count == 0 ? program : $"{program} {string.Join(' ', arguments)}";
    }

    private static Process Start(ProcessStartInfo start, string program, string workingDirectory)
    {
        try
        {
            Process? process = Process.Start(start);
            if (process is null)
            {
                throw new InvalidOperationException(
                    $"The program `{program}` gave no process in '{workingDirectory}' (T-2).");
            }

            return process;
        }
        catch (Win32Exception fault)
        {
            throw new InvalidOperationException(
                $"The program `{program}` did not start in '{workingDirectory}' (T-2). {fault.Message}",
                fault);
        }
    }
}
