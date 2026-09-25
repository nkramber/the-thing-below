using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The workflows collect the paths and the diff of a PR with git, and the commands of Tools
/// decide from them (D-859, D-15). These tests read the committed workflow text, because no
/// test runs the git layer of a workflow (F-109, F-110).
/// </summary>
public sealed class WorkflowPathListTests
{
    /// <summary>The folder of the workflow files.</summary>
    private const string WorkflowFolder = ".github/workflows";

    /// <summary>The workflow of the review gate (D-15).</summary>
    private const string ReviewGateWorkflowPath = ".github/workflows/review-gate.yml";

    /// <summary>The step that writes the facts of the PR for the review gate.</summary>
    private const string FactsStepName = "Write the facts of the PR";

    /// <summary>The git commands that can list the paths of a change.</summary>
    private static readonly string[] PathCommands = ["git diff", "git show", "git log"];

    /// <summary>
    /// The regression test of F-109. Git finds renames by default, and a rename then lists its
    /// new path alone. A move of a test file into `docs/` then read as a change of documents
    /// alone, so CI skipped and the label passed.
    /// </summary>
    [Fact]
    public void EachGitListOfPathsInAWorkflowTakesNoRenames()
    {
        List<string> lists = [];
        List<string> faults = [];
        foreach (string path in Directory.GetFiles(RepositoryRoot.PathTo(WorkflowFolder), "*.yml").Order(StringComparer.Ordinal))
        {
            string[] lines = File.ReadAllLines(path);
            for (int index = 0; index < lines.Length; index += 1)
            {
                string line = lines[index].Trim();
                if (line.StartsWith('#') || !line.Contains("--name-only", StringComparison.Ordinal)
                    || !PathCommands.Any(command => line.Contains(command, StringComparison.Ordinal)))
                {
                    continue;
                }

                string where = $"{Path.GetFileName(path)}:{index + 1}";
                lists.Add(where);
                if (!line.Contains("--no-renames", StringComparison.Ordinal))
                {
                    faults.Add(where);
                }
            }
        }

        // Four lists exist: two of the changed-paths job and two of the review gate. A count
        // below that means the search broke, and the test would pass on nothing (T-2).
        Assert.True(lists.Count >= 4, $"The search found {lists.Count} git lists of paths: {string.Join(", ", lists)}.");
        Assert.True(
            faults.Count == 0,
            $"These git lists of paths find renames, and a rename then hides the path that it left: {string.Join(", ", faults)} (F-109).");
    }

    /// <summary>
    /// The regression test of F-110. Linux refuses one argument above 128 KiB, and the step
    /// passed the diff of `docs/decisions.md` to jq as one argument.
    /// </summary>
    [Theory]
    [InlineData("--rawfile body ")]
    [InlineData("--rawfile decisionsDiff ")]
    [InlineData("--slurpfile files ")]
    [InlineData("--slurpfile commits ")]
    public void TheFactsStepPassesEachValueThatGrowsThroughAFile(string option)
    {
        string[] block = WorkflowText.RunBlockOf(ReviewGateWorkflowPath, FactsStepName);

        Assert.Contains(block, line => line.Contains(option, StringComparison.Ordinal));
    }

    [Fact]
    public void TheFactsStepPassesOnlyTheSmallValuesAsArguments()
    {
        string[] block = WorkflowText.RunBlockOf(ReviewGateWorkflowPath, FactsStepName);
        string[] small = ["--argjson number ", "--argjson labels ", "--arg sha "];

        string[] arguments = block
            .Select(line => line.Trim())
            .Where(line => line.StartsWith("--arg", StringComparison.Ordinal) || line.Contains(" --arg", StringComparison.Ordinal))
            .ToArray();

        Assert.NotEmpty(arguments);
        foreach (string line in arguments)
        {
            Assert.True(
                small.Any(option => line.Contains(option, StringComparison.Ordinal)),
                $"The line '{line}' passes a value to jq as one argument, and only the number, the labels, and one sha stay small (F-110).");
        }
    }
}
