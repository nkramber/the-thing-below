using System;
using System.IO;
using TheThingBelow.Tools;
using TheThingBelow.Tools.Content;
using TheThingBelow.Tools.Screenplay;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The `screenplay` command writes the screenplay of the changed story scenes into the
/// Screenplay section of a PR body file (D-173, D-1015, D-1016, G-25).
/// </summary>
public sealed class ScreenplayCommandTests
{
    private const string ScenePath = "rules/scenes/test_screen.json";

    private const string SceneFile = """
    {
     "comment": "A line of Marrek, a pause, and a line with no speaker.",
     "id": "scene.test_screen",
     "steps": [
      { "id": "step.s1", "kind": "say", "speaker": "character.marrek", "line": "line.test_screen_door" },
      { "id": "step.s2", "kind": "wait", "ticks": 3 },
      { "id": "step.s3", "kind": "say", "speaker": "none", "line": "line.test_screen_quiet" }
     ]
    }
    """;

    private const string Body = "## Summary\n\nA fixture batch.\n\n## Documents\n\n- A line.\n";

    [Fact]
    public void TheOutputReachesThePullRequestBodyFileOfAFixtureBatch()
    {
        // Exit test 3 of PR-50 (D-1016).
        using ContentCheckout head = ContentCheckout.Copy();
        using ContentCheckout baseFolder = ContentCheckout.Copy();
        head.WriteRuleFile(ScenePath, SceneFile);
        AddStrings(head, """{ "id": "line.test_screen_door", "text": "The door holds." }, { "id": "line.test_screen_quiet", "text": "Nothing moves." },""");
        string bodyFile = BodyFile(head);

        int exitCode = Run(head, baseFolder, bodyFile, out string output, out string errors);

        Assert.Equal(string.Empty, errors);
        Assert.Equal(0, exitCode);
        Assert.Contains("changed story scenes: 1. Removed story scene files: 0.", output, StringComparison.Ordinal);

        string expected = Body + """

            <!-- screenplay:start -->
            ## Screenplay

            Changed story scenes: 1. Removed story scene files: 0 (D-1015).

            ### scene.test_screen (`rules/scenes/test_screen.json`)

            `[0]` **MARREK**
            > The door holds.

            `[1]` *Pause: 3 ticks.*

            `[2]` *(no speaker)*
            > *Nothing moves.*
            <!-- screenplay:end -->

            """.ReplaceLineEndings("\n");
        Assert.Equal(expected, File.ReadAllText(bodyFile));
    }

    [Fact]
    public void ASecondRunReplacesTheSection()
    {
        using ContentCheckout head = ContentCheckout.Copy();
        using ContentCheckout baseFolder = ContentCheckout.Copy();
        head.WriteRuleFile(ScenePath, SceneFile);
        AddStrings(head, """{ "id": "line.test_screen_door", "text": "The door holds." }, { "id": "line.test_screen_quiet", "text": "Nothing moves." },""");
        string bodyFile = BodyFile(head);
        _ = Run(head, baseFolder, bodyFile, out _, out _);

        // The base now holds the story scene, so the batch is empty.
        baseFolder.WriteRuleFile(ScenePath, SceneFile);
        AddStrings(baseFolder, """{ "id": "line.test_screen_door", "text": "The door holds." }, { "id": "line.test_screen_quiet", "text": "Nothing moves." },""");
        int exitCode = Run(head, baseFolder, bodyFile, out _, out string errors);

        Assert.Equal(string.Empty, errors);
        Assert.Equal(0, exitCode);
        Assert.Equal(
            Body + "\n<!-- screenplay:start -->\n## Screenplay\n\nNo story scene changes in this PR (D-1015).\n<!-- screenplay:end -->\n",
            File.ReadAllText(bodyFile));
    }

    [Fact]
    public void AStorySceneThatNamesAnAbsentStringIdFailsWithTheStorySceneTheStepAndTheId()
    {
        // Exit test 2 of PR-50, through the load of the head (G-7, T-2).
        using ContentCheckout head = ContentCheckout.Copy();
        using ContentCheckout baseFolder = ContentCheckout.Copy();
        head.WriteRuleFile(ScenePath, SceneFile);
        AddStrings(head, """{ "id": "line.test_screen_quiet", "text": "Nothing moves." },""");
        string bodyFile = BodyFile(head);

        int exitCode = Run(head, baseFolder, bodyFile, out _, out string errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains(ScenePath, errors, StringComparison.Ordinal);
        Assert.Contains("scene.test_screen.steps[0].line", errors, StringComparison.Ordinal);
        Assert.Contains("line.test_screen_door", errors, StringComparison.Ordinal);
        Assert.Equal(Body, File.ReadAllText(bodyFile));
    }

    [Fact]
    public void TheCommandNeedsTheBaseAndTheBody()
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = Program.Run([ScreenplayCommand.Name, "--root", "."], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("--base <folder> and --body <file>", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void AnAbsentBaseFolderFailsAndNamesIt()
    {
        using ContentCheckout head = ContentCheckout.Copy();
        string bodyFile = BodyFile(head);
        string absent = Path.Combine(head.Root, "no-base");
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = ScreenplayCommand.Run(["--root", head.Root, "--base", absent, "--body", bodyFile], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains(absent, errors.ToString(), StringComparison.Ordinal);
    }

    private static int Run(ContentCheckout head, ContentCheckout baseFolder, string bodyFile, out string output, out string errors)
    {
        using StringWriter outputWriter = new();
        using StringWriter errorWriter = new();

        int exitCode = ScreenplayCommand.Run(["--root", head.Root, "--base", baseFolder.Root, "--body", bodyFile], outputWriter, errorWriter);

        output = outputWriter.ToString();
        errors = errorWriter.ToString();
        return exitCode;
    }

    private static string BodyFile(ContentCheckout checkout)
    {
        string path = Path.Combine(checkout.Root, "body.md");
        File.WriteAllText(path, Body);
        return path;
    }

    /// <summary>Adds entries at the start of the string list of the copy, which takes no order (G-7).</summary>
    private static void AddStrings(ContentCheckout checkout, string entries)
    {
        string path = Path.Combine(checkout.Root, ContentFolder.FolderName, "strings", "en.json");
        string text = File.ReadAllText(path);
        const string ListStart = "\"strings\": [";
        Assert.Contains(ListStart, text, StringComparison.Ordinal);
        File.WriteAllText(path, text.Replace(ListStart, ListStart + "\n  " + entries, StringComparison.Ordinal));
    }
}
