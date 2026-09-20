using System;
using System.IO;
using System.Linq;

namespace TheThingBelow.Tests;

/// <summary>
/// Reads the text of a workflow file for the tests of the CI gates. The tests read the file as
/// text and not as YAML, so each read names the step and the block that it expects (T-2).
/// </summary>
internal static class WorkflowText
{
    /// <summary>Reads the lines of the `run` block of one named step of a workflow.</summary>
    /// <param name="workflowPath">The path of the workflow under the root of the checkout.</param>
    /// <param name="stepName">The value of the `name` key of the step.</param>
    /// <returns>The lines of the block, with the indent of the block removed.</returns>
    /// <exception cref="InvalidOperationException">The workflow holds no such step or block.</exception>
    public static string[] RunBlockOf(string workflowPath, string stepName)
    {
        string path = RepositoryRoot.PathTo(workflowPath);
        string[] lines = File.ReadAllLines(path);

        int step = Array.FindIndex(lines, line => line.Trim() == $"- name: {stepName}");
        if (step < 0)
        {
            throw new InvalidOperationException(
                $"The workflow '{path}' holds no step named '{stepName}' (T-2).");
        }

        int run = Array.FindIndex(lines, step, line => line.Trim() == "run: |");
        if (run < 0)
        {
            throw new InvalidOperationException(
                $"The step '{stepName}' of '{path}' holds no `run` block (T-2).");
        }

        // The block ends at the first line that carries text at the indent of `run` or less.
        int indent = lines[run].Length - lines[run].TrimStart().Length;
        return lines
            .Skip(run + 1)
            .TakeWhile(line => line.Trim().Length == 0 ||
                               line.Length - line.TrimStart().Length > indent)
            .ToArray();
    }

    /// <summary>Gives the first line of a `run` block that holds text.</summary>
    /// <param name="block">The lines of the block.</param>
    /// <returns>The first line with text, with its indent removed.</returns>
    /// <exception cref="InvalidOperationException">The block holds no text (T-2).</exception>
    public static string FirstCommandOf(string[] block)
    {
        string? first = block.FirstOrDefault(line => line.Trim().Length > 0);
        return first?.Trim() ?? throw new InvalidOperationException("The run block holds no command (T-2).");
    }
}
