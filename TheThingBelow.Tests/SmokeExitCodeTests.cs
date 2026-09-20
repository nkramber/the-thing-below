using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// Three places start the smoke session of the game: the `smoke` target of the `Makefile`,
/// the smoke job of `.github/workflows/ci.yml`, and the export job of
/// `.github/workflows/export.yml`. Each one must fail when the session fails (T-2, D-694).
/// </summary>
/// <remarks>
/// A command that ends with `|| true` drops the exit code of the session. The caller then
/// reads the log alone, and it passes a session that wrote the success line and then failed.
/// The success line stays a condition too, because a session whose managed assembly does not
/// load gives an exit code of 0 and writes no line (F-64).
/// </remarks>
public sealed class SmokeExitCodeTests
{
    /// <summary>The end of the command line that starts a smoke session.</summary>
    private const string SmokeCommandMark = "-- --smoke";

    /// <summary>The file of each caller of the smoke session.</summary>
    public static TheoryData<string> Callers { get; } = new TheoryData<string>
    {
        "Makefile",
        ".github/workflows/ci.yml",
        ".github/workflows/export.yml",
    };

    [Theory]
    [MemberData(nameof(Callers))]
    public void NoCommandThatStartsTheSmokeSessionDropsItsExitCode(string path)
    {
        string[] commands = LogicalCommandsOf(path)
            .Where(command => command.Contains(SmokeCommandMark, StringComparison.Ordinal))
            .ToArray();

        Assert.True(
            commands.Length == 1,
            $"The file '{path}' holds {commands.Length} commands with '{SmokeCommandMark}', " +
            $"and one starts the smoke session (T-2).");
        Assert.DoesNotContain("|| true", commands[0], StringComparison.Ordinal);
    }

    [Fact]
    public void TheMakefileSmokeTargetFailsOnANonzeroExitCode()
    {
        string recipe = string.Join('\n', RecipeOf("Makefile", "smoke"));

        // A recipe of make reads `$$` as one dollar sign of the shell.
        Assert.Contains("|| status=$$?", recipe, StringComparison.Ordinal);
        Assert.Contains("if [ \"$$status\" != \"0\" ]; then", recipe, StringComparison.Ordinal);
    }

    /// <summary>
    /// Reads the commands of a file, and joins each line that a backslash continues. A shell
    /// reads such lines as one command, so a rule of one command reads the joined text.
    /// </summary>
    /// <param name="relativePath">The path of the file under the root, with forward slashes.</param>
    /// <returns>One string for each command of the file.</returns>
    private static IReadOnlyList<string> LogicalCommandsOf(string relativePath)
    {
        List<string> commands = [];
        string current = string.Empty;

        foreach (string line in File.ReadAllLines(RepositoryRoot.PathTo(relativePath)))
        {
            string text = line.TrimEnd();
            if (text.EndsWith('\\'))
            {
                current += text[..^1] + " ";
                continue;
            }

            commands.Add(current + text);
            current = string.Empty;
        }

        if (current.Length > 0)
        {
            commands.Add(current);
        }

        return commands;
    }

    /// <summary>Reads the recipe lines of one target of a makefile.</summary>
    /// <param name="relativePath">The path of the makefile under the root of the checkout.</param>
    /// <param name="target">The name of the target, such as `smoke`.</param>
    /// <returns>The lines of the recipe, which each start with a tab.</returns>
    /// <exception cref="InvalidOperationException">The file holds no such target (T-2).</exception>
    private static IReadOnlyList<string> RecipeOf(string relativePath, string target)
    {
        string path = RepositoryRoot.PathTo(relativePath);
        string[] lines = File.ReadAllLines(path);

        int start = Array.IndexOf(lines, $"{target}:");
        if (start < 0)
        {
            throw new InvalidOperationException(
                $"The file '{path}' holds no target '{target}:' (T-2).");
        }

        return lines
            .Skip(start + 1)
            .TakeWhile(line => line.StartsWith('\t'))
            .ToArray();
    }
}
