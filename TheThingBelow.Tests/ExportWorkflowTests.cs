using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The export workflow of PR-54. It runs on each merge to `main`, and on each pull request
/// that changes one of the four export paths (D-449, D-512, D-692).
/// </summary>
/// <remarks>
/// The job is not a line of the PR gate, so a fault in its trigger or in its matrix shows on
/// a merge and not before it. These tests read the committed workflow files instead (T-2).
/// </remarks>
public sealed class ExportWorkflowTests
{
    /// <summary>The workflow that exports the three builds.</summary>
    private const string ExportWorkflowPath = ".github/workflows/export.yml";

    /// <summary>The workflow whose smoke job downloads the same Godot editor archives.</summary>
    private const string CiWorkflowPath = ".github/workflows/ci.yml";

    /// <summary>The step that starts the exported game and reads the result of the session.</summary>
    private const string SmokeStepName = "Run the headless smoke session on the export";

    /// <summary>The end of the command line that starts the smoke session of a build.</summary>
    private const string SmokeCommandMark = "-- --smoke";

    /// <summary>The paths that start the job on a pull request, in the order of D-692.</summary>
    private static readonly string[] TriggerPaths =
    [
        ".github/workflows/export.yml",
        "TheThingBelow.Game/export_presets.cfg",
        "TheThingBelow.Game/project.godot",
        "licenses/**",
    ];

    [Fact]
    public void ThePullRequestTriggerHoldsTheFourExportPathsOfD692()
    {
        string[] lines = File.ReadAllLines(RepositoryRoot.PathTo(ExportWorkflowPath));
        int start = Array.IndexOf(lines, "    paths:");
        Assert.True(start >= 0, $"The workflow '{ExportWorkflowPath}' holds no `paths` key (T-2).");

        string[] paths = lines
            .Skip(start + 1)
            .TakeWhile(line => line.StartsWith("      - ", StringComparison.Ordinal))
            .Select(line => line["      - ".Length..].Trim())
            .ToArray();

        Assert.Equal(TriggerPaths, paths);
    }

    [Fact]
    public void EachTriggerPathNamesAFileOrAFolderOfTheCheckout()
    {
        foreach (string path in TriggerPaths)
        {
            // A path filter that names no path starts no run, and GitHub reports no fault.
            // The `**` of a folder filter is no part of the name on disk (T-2).
            string onDisk = RepositoryRoot.PathTo(path.Replace("/**", string.Empty));
            Assert.True(
                File.Exists(onDisk) || Directory.Exists(onDisk),
                $"The trigger path '{path}' names '{onDisk}', and the checkout holds no such " +
                $"file and no such folder (D-692, T-2).");
        }
    }

    [Fact]
    public void EachPresetOfTheMatrixNamesAPresetOfTheFile()
    {
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> sections =
            GodotConfigFile.Read("TheThingBelow.Game/export_presets.cfg");
        string[] names = sections
            .Where(section => section.Value.ContainsKey("name"))
            .Select(section => section.Value["name"].Trim('"'))
            .ToArray();

        string[] matrix = ValuesOf(ExportWorkflowPath, "preset");

        Assert.Equal(3, matrix.Length);
        foreach (string preset in matrix)
        {
            Assert.Contains(preset, names);
        }
    }

    [Fact]
    public void EachEditorDigestMatchesTheSmokeJobOfTheCiWorkflow()
    {
        string[] exportDigests = ValuesOf(ExportWorkflowPath, "sha512");
        string[] ciDigests = ValuesOf(CiWorkflowPath, "sha512");

        // Both workflows download the same three archives under one cache key. A version bump
        // that changes one file and not the other would give two editors (D-511, D-596, T-2).
        Assert.Equal(3, exportDigests.Length);
        Assert.Equal(ciDigests.Order(StringComparer.Ordinal), exportDigests.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void TheTemplateDigestHoldsTheLengthOfASha512()
    {
        string[] lines = File.ReadAllLines(RepositoryRoot.PathTo(ExportWorkflowPath));
        string mark = "  TEMPLATES_SHA512: ";
        string digest = lines
            .Single(line => line.StartsWith(mark, StringComparison.Ordinal))[mark.Length..]
            .Trim('"');

        Assert.Equal(128, digest.Length);
        Assert.All(digest, letter => Assert.Contains(letter, "0123456789abcdef"));
    }

    [Fact]
    public void TheJobCopiesTheLicenseFolderIntoEachExport()
    {
        string text = File.ReadAllText(RepositoryRoot.PathTo(ExportWorkflowPath));

        // Every export carries the notices of D-467, and the Godot export packs the project
        // folder alone, so one step of the job copies the folder beside the build (F-42).
        Assert.Contains("cp -R licenses export/licenses", text, StringComparison.Ordinal);
    }

    [Fact]
    public void ACancelAppliesToAPullRequestAlone()
    {
        string text = File.ReadAllText(RepositoryRoot.PathTo(ExportWorkflowPath));

        // Every push to `main` shares one concurrency group, because `github.ref` is the same
        // for each merge. A cancel there drops the build artifact of the earlier merge, and
        // D-449 asks for the export of every merge.
        Assert.Contains(
            "cancel-in-progress: ${{ github.event_name == 'pull_request' }}",
            text,
            StringComparison.Ordinal);
    }

    [Fact]
    public void TheSmokeStepKeepsTheExitCodeOfTheExportedGame()
    {
        string[] lines = RunBlockOf(ExportWorkflowPath, SmokeStepName);

        int start = Array.FindIndex(lines, line => line.Contains(SmokeCommandMark, StringComparison.Ordinal));
        Assert.True(
            start >= 0,
            $"The step '{SmokeStepName}' runs no command that holds '{SmokeCommandMark}' (T-2).");
        Assert.Equal(
            1,
            lines.Count(line => line.Contains(SmokeCommandMark, StringComparison.Ordinal)));

        // The step keeps the exit code of the build. `SmokeExitCodeTests` holds the rule
        // that no caller of the smoke session drops that code (T-2, D-694).
        Assert.Contains("status=$?", lines[start], StringComparison.Ordinal);

        // The log checks run first, so the step reads the code after them and fails the leg.
        string tail = string.Join('\n', lines.Skip(start + 1));
        Assert.Contains("$status", tail, StringComparison.Ordinal);
        Assert.Contains("exit 1", tail, StringComparison.Ordinal);
    }

    [Fact]
    public void TheArtifactLastsTheNinetyDaysOfD449()
    {
        string text = File.ReadAllText(RepositoryRoot.PathTo(ExportWorkflowPath));

        Assert.Contains("  ARTIFACT_DAYS: \"90\"", text, StringComparison.Ordinal);
        Assert.Contains("retention-days: ${{ env.ARTIFACT_DAYS }}", text, StringComparison.Ordinal);
    }

    /// <summary>Reads the lines of the `run` block of one named step of a workflow.</summary>
    /// <param name="workflowPath">The path of the workflow under the root of the checkout.</param>
    /// <param name="stepName">The value of the `name` key of the step.</param>
    /// <returns>The lines of the block, with the indent of the block removed.</returns>
    /// <exception cref="InvalidOperationException">The workflow holds no such step or block.</exception>
    private static string[] RunBlockOf(string workflowPath, string stepName)
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

    /// <summary>Reads every value of one matrix key of a workflow file.</summary>
    /// <param name="workflowPath">The path of the workflow under the root of the checkout.</param>
    /// <param name="key">The name of the key, such as `preset`.</param>
    /// <returns>The values, in the order of the file, with no quotation marks.</returns>
    private static string[] ValuesOf(string workflowPath, string key)
    {
        string mark = $"{key}: ";
        return File.ReadAllLines(RepositoryRoot.PathTo(workflowPath))
            .Select(line => line.Trim())
            .Where(line => line.StartsWith(mark, StringComparison.Ordinal))
            .Select(line => line[mark.Length..].Trim().Trim('"'))
            .ToArray();
    }
}
