using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The export workflow of PR-54. It runs on each merge to `main`, and on each pull request
/// that changes one of the five export paths (D-449, D-512, D-692, D-699).
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

    /// <summary>
    /// The paths that start the job on a pull request: the four of D-692 in its order, then
    /// the Game project file, which carries `content/` into each export (D-699, D-508).
    /// </summary>
    private static readonly string[] TriggerPaths =
    [
        ".github/workflows/export.yml",
        "TheThingBelow.Game/export_presets.cfg",
        "TheThingBelow.Game/project.godot",
        "licenses/**",
        "TheThingBelow.Game/TheThingBelow.Game.csproj",
    ];

    [Fact]
    public void ThePullRequestTriggerHoldsTheFiveExportPathsOfD692AndD699()
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
                $"file and no such folder (D-692, D-699, T-2).");
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

        // The license of the game itself goes beside the notices (D-695).
        Assert.Contains("cp LICENSE export/LICENSE", text, StringComparison.Ordinal);
    }

    [Fact]
    public void ACancelAppliesToAPullRequestAlone()
    {
        string text = File.ReadAllText(RepositoryRoot.PathTo(ExportWorkflowPath));

        // A cancel of a push to `main` drops the build artifact of that merge, and D-449 asks
        // for the export of every merge.
        Assert.Contains(
            "cancel-in-progress: ${{ github.event_name == 'pull_request' }}",
            text,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// The regression test of F-110. GitHub keeps one waiting run in a group and cancels an
    /// older waiting run, even with no cancel of the run in progress. One group for `main`
    /// thus dropped the export of the middle merge of three quick merges.
    /// </summary>
    [Theory]
    [InlineData(ExportWorkflowPath, "export-")]
    [InlineData(CiWorkflowPath, "ci-")]
    public void EachPushToMainTakesAGroupOfItsOwnCommit(string workflowPath, string prefix)
    {
        string text = File.ReadAllText(RepositoryRoot.PathTo(workflowPath));

        Assert.Contains(
            $"  group: {prefix}${{{{ github.workflow }}}}-${{{{ github.event_name == 'pull_request' && github.ref || github.sha }}}}\n",
            text.ReplaceLineEndings("\n"),
            StringComparison.Ordinal);
    }

    /// <summary>
    /// The regression test of F-111. Under `set -e`, a nonzero code of the editor ended the
    /// step before it printed the log, so a failed export showed no reason.
    /// </summary>
    [Theory]
    [InlineData("Import the project", "> import.log 2>&1", "cat import.log")]
    [InlineData("Export the build of this leg", "> export.log 2>&1", "cat export.log")]
    public void EachEditorStepKeepsItsCodeAndPrintsItsLogFirst(string stepName, string redirect, string print)
    {
        string[] lines = WorkflowText.RunBlockOf(ExportWorkflowPath, stepName);

        int run = Array.FindIndex(lines, line => line.Contains(redirect, StringComparison.Ordinal));
        Assert.True(run >= 0, $"The step '{stepName}' holds no line with '{redirect}' (T-2).");
        Assert.Contains("|| status=$?", lines[run], StringComparison.Ordinal);

        int printed = Array.FindIndex(lines, run, line => line.Trim() == print);
        int checkedCode = Array.FindIndex(lines, run, line => line.Contains("if [ \"$status\" != \"0\" ]; then", StringComparison.Ordinal));
        Assert.True(printed > run, $"The step '{stepName}' does not print its log after the editor runs (F-111).");
        Assert.True(checkedCode > printed, $"The step '{stepName}' reads the code of the editor before it prints the log (F-111).");
    }

    [Fact]
    public void TheSmokeStepKeepsTheExitCodeOfTheExportedGame()
    {
        string[] lines = WorkflowText.RunBlockOf(ExportWorkflowPath, SmokeStepName);

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
        // The exact test of the code is the one line that keeps the code, so the assertion
        // names it and not a `exit 1` that a log check holds too.
        string tail = string.Join('\n', lines.Skip(start + 1));
        Assert.Contains("if [ \"$status\" != \"0\" ]; then", tail, StringComparison.Ordinal);
    }

    [Fact]
    public void TheArtifactLastsTheNinetyDaysOfD449()
    {
        string text = File.ReadAllText(RepositoryRoot.PathTo(ExportWorkflowPath));

        Assert.Contains("  ARTIFACT_DAYS: \"90\"", text, StringComparison.Ordinal);
        Assert.Contains("retention-days: ${{ env.ARTIFACT_DAYS }}", text, StringComparison.Ordinal);
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
