using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheThingBelow.Tools.ChangedPaths;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The decision of the first CI job: whether each job except ste-check, review-gate, and
/// Gitar skips (D-595, D-856, D-857, D-858). Exit tests 1 to 4 of PR-93 each have a test here.
/// </summary>
public sealed class ChangedPathsCommandTests
{
    private static readonly string[] DocsPaths = ["docs/design.md", ".claude/skills/pr-review/SKILL.md"];

    private static readonly string[] CodePaths = ["docs/design.md", "TheThingBelow.Core/Tick.cs"];

    [Theory]
    [InlineData("docs/session-handoff.md")]
    [InlineData("docs/reviews/pr-93.md")]
    [InlineData(".claude/skills/ste-writing/SKILL.md")]
    [InlineData(".claude/settings.json")]
    [InlineData("CLAUDE.md")]
    [InlineData("AGENTS.md")]
    [InlineData("README.md")]
    [InlineData("LICENSE")]
    [InlineData(".github/pull_request_template.md")]
    public void EachPathOfTheSkipSetIsInside(string path)
    {
        Assert.True(DocumentsAlonePaths.Holds(path), $"The path '{path}' is in the set of D-600 and D-857.");
    }

    [Theory]
    [InlineData(".github/workflows/ci.yml")]
    [InlineData("content/rules/items.json")]
    [InlineData("TheThingBelow.Tools/Program.cs")]
    [InlineData("Makefile")]
    [InlineData("docs")]
    [InlineData("xdocs/design.md")]
    [InlineData("sub/CLAUDE.md")]
    [InlineData("CLAUDE.md.bak")]
    public void EachOtherPathIsOutside(string path)
    {
        Assert.False(DocumentsAlonePaths.Holds(path), $"The path '{path}' is not in the skip set.");
    }

    [Fact]
    public void ADocsOnlyPullRequestSkips()
    {
        // Exit test 1.
        SkipDecision decision = ChangedPathsCommand.Decide("pull_request", DocsPaths, null, null);

        Assert.True(decision.DocumentsAlone, decision.Reason);
    }

    [Fact]
    public void AnAgentFilePullRequestSkips()
    {
        // D-857: the ste-check job reads the rule of D-20, so the two files join the set.
        SkipDecision decision = ChangedPathsCommand.Decide("pull_request", ["CLAUDE.md", "AGENTS.md"], null, null);

        Assert.True(decision.DocumentsAlone, decision.Reason);
    }

    [Fact]
    public void ADocsOnlyPushAfterAGreenHeadSkips()
    {
        // Exit test 2.
        SkipDecision decision = ChangedPathsCommand.Decide("pull_request", CodePaths, DocsPaths, GreenChecks());

        Assert.True(decision.DocumentsAlone, decision.Reason);
    }

    [Fact]
    public void ADocsOnlyPushAfterAGreenHeadSkipsWhenTheReviewGateIsRed()
    {
        // D-858: review-gate and ste-check run on every head, so their result on the previous head
        // does not count.
        List<CheckRunFacts> checks = [.. GreenChecks(), Run("review-gate", "completed", "failure"), Run("ste-check", "completed", "failure")];

        SkipDecision decision = ChangedPathsCommand.Decide("pull_request", CodePaths, DocsPaths, checks);

        Assert.True(decision.DocumentsAlone, decision.Reason);
    }

    [Theory]
    [InlineData("failure")]
    [InlineData("cancelled")]
    [InlineData("timed_out")]
    [InlineData("skipped")]
    [InlineData("neutral")]
    public void ADocsOnlyPushAfterARedHeadRunsEveryJob(string conclusion)
    {
        // Exit test 3. Each skipped check in turn ends without success.
        foreach (string name in PreviousHeadChecks.SkippedCheckNames)
        {
            List<CheckRunFacts> checks = GreenChecks().Where(run => run.Name != name).ToList();
            checks.Add(Run(name, "completed", conclusion));

            SkipDecision decision = ChangedPathsCommand.Decide("pull_request", CodePaths, DocsPaths, checks);

            Assert.False(decision.DocumentsAlone, $"The check '{name}' ended with '{conclusion}'.");
            Assert.Contains(name, decision.Reason, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ADocsOnlyPushAfterAHeadWithAnAbsentCheckRunsEveryJob()
    {
        // An absent run is a fault, never a pass (T-2).
        foreach (string name in PreviousHeadChecks.SkippedCheckNames)
        {
            List<CheckRunFacts> checks = GreenChecks().Where(run => run.Name != name).ToList();

            SkipDecision decision = ChangedPathsCommand.Decide("pull_request", CodePaths, DocsPaths, checks);

            Assert.False(decision.DocumentsAlone, $"The check '{name}' has no run.");
            Assert.Contains($"'{name}' has no run", decision.Reason, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ADocsOnlyPushAfterAHeadThatStillRunsRunsEveryJob()
    {
        // A new push cancels the run of the previous head, so its checks never complete.
        List<CheckRunFacts> checks = GreenChecks().Where(run => run.Name != "smoke").ToList();
        checks.Add(Run("smoke", "in_progress", null));

        SkipDecision decision = ChangedPathsCommand.Decide("pull_request", CodePaths, DocsPaths, checks);

        Assert.False(decision.DocumentsAlone, decision.Reason);
        Assert.Contains("'smoke' has the status 'in_progress'", decision.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEarlierFailureOfOneNameRunsEveryJob()
    {
        List<CheckRunFacts> checks = [.. GreenChecks(), Run("det-lint", "completed", "failure")];

        SkipDecision decision = ChangedPathsCommand.Decide("pull_request", CodePaths, DocsPaths, checks);

        Assert.False(decision.DocumentsAlone, decision.Reason);
    }

    [Fact]
    public void ACheckFromAnotherAppDoesNotCount()
    {
        List<CheckRunFacts> checks = GreenChecks().Where(run => run.Name != "det-lint").ToList();
        checks.Add(new CheckRunFacts("det-lint", "completed", "success", "another-app"));

        SkipDecision decision = ChangedPathsCommand.Decide("pull_request", CodePaths, DocsPaths, checks);

        Assert.False(decision.DocumentsAlone, decision.Reason);
    }

    [Fact]
    public void APushThatChangesOneCodePathRunsEveryJob()
    {
        // Exit test 4.
        SkipDecision decision = ChangedPathsCommand.Decide("pull_request", CodePaths, CodePaths, GreenChecks());

        Assert.False(decision.DocumentsAlone, decision.Reason);
        Assert.Contains("TheThingBelow.Core/Tick.cs", decision.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void APullRequestWithNoPreviousHeadReadsTheWholePullRequest()
    {
        // A new PR, a reopen, and a force push give no previous head that is an ancestor.
        SkipDecision decision = ChangedPathsCommand.Decide("pull_request", CodePaths, null, null);

        Assert.False(decision.DocumentsAlone, decision.Reason);
    }

    [Fact]
    public void AnEmptyChangeRunsEveryJob()
    {
        Assert.False(ChangedPathsCommand.Decide("pull_request", [], null, null).DocumentsAlone);
        Assert.False(ChangedPathsCommand.Decide("pull_request", CodePaths, [], GreenChecks()).DocumentsAlone);
    }

    [Fact]
    public void APushToMainRunsEveryJob()
    {
        SkipDecision decision = ChangedPathsCommand.Decide("push", null, null, null);

        Assert.False(decision.DocumentsAlone, decision.Reason);
    }

    [Fact]
    public void FactsThatDoNotFitTheEventStopTheCommand()
    {
        Assert.Throws<InvalidOperationException>(() => ChangedPathsCommand.Decide("push", DocsPaths, null, null));
        Assert.Throws<InvalidOperationException>(() => ChangedPathsCommand.Decide("pull_request", null, null, null));
        Assert.Throws<InvalidOperationException>(() => ChangedPathsCommand.Decide("pull_request", CodePaths, DocsPaths, null));
        Assert.Throws<InvalidOperationException>(() => ChangedPathsCommand.Decide("pull_request", CodePaths, null, GreenChecks()));
    }

    [Fact]
    public void TheCommandReadsTheFilesAndWritesTheOutputLine()
    {
        using TemporaryFolder folder = new TemporaryFolder();
        string pullRequestPaths = folder.Write("pr-paths.txt", string.Join('\n', CodePaths) + "\n");
        string pushPaths = folder.Write("push-paths.txt", string.Join('\n', DocsPaths) + "\n");
        string checks = folder.Write(
            "previous-checks.jsonl",
            string.Join('\n', PreviousHeadChecks.SkippedCheckNames.Select(
                name => $"{{\"name\":\"{name}\",\"status\":\"completed\",\"conclusion\":\"success\",\"app\":\"github-actions\"}}")) + "\n");
        string outputFile = folder.Write("output.txt", "earlier=1\n");
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Tools.Program.Run(
            ["changed-paths", "--event-name", "pull_request", "--pr-paths", pullRequestPaths,
             "--push-paths", pushPaths, "--previous-checks", checks, "--output", outputFile],
            output,
            errors);

        Assert.True(exitCode == 0, errors.ToString());
        Assert.Equal("earlier=1\ndocuments-alone=true\n", File.ReadAllText(outputFile));
    }

    [Theory]
    [InlineData("{\"name\":\"smoke\",\"status\":\"completed\",\"app\":\"github-actions\"}", "conclusion")]
    [InlineData("{\"name\":\"smoke\",\"conclusion\":null,\"app\":\"github-actions\"}", "status")]
    [InlineData("[1]", "Array")]
    [InlineData("{not json", "")]
    public void ABadCheckFileStopsTheCommand(string line, string detail)
    {
        using TemporaryFolder folder = new TemporaryFolder();
        string pullRequestPaths = folder.Write("pr-paths.txt", "Makefile\n");
        string checks = folder.Write("previous-checks.jsonl", line + "\n");
        string outputFile = folder.Write("output.txt", string.Empty);
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Tools.Program.Run(
            ["changed-paths", "--event-name", "pull_request", "--pr-paths", pullRequestPaths,
             "--push-paths", pullRequestPaths, "--previous-checks", checks, "--output", outputFile],
            output,
            errors);

        Assert.Equal(1, exitCode);
        Assert.Contains(detail, errors.ToString(), StringComparison.Ordinal);
        Assert.Equal(string.Empty, File.ReadAllText(outputFile));
    }

    [Fact]
    public void AnAbsentOutputOptionStopsTheCommand()
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Tools.Program.Run(["changed-paths", "--event-name", "push"], output, errors);

        Assert.Equal(1, exitCode);
        Assert.Contains("--output", errors.ToString(), StringComparison.Ordinal);
    }

    private static List<CheckRunFacts> GreenChecks()
    {
        return PreviousHeadChecks.SkippedCheckNames.Select(name => Run(name, "completed", "success")).ToList();
    }

    private static CheckRunFacts Run(string name, string status, string? conclusion)
    {
        return new CheckRunFacts(name, status, conclusion, PreviousHeadChecks.ActionsApp);
    }

    /// <summary>A temporary folder that goes away at the end of the test.</summary>
    private sealed class TemporaryFolder : IDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), "changed-paths-" + Guid.NewGuid().ToString("N"));

        public TemporaryFolder() => Directory.CreateDirectory(root);

        public string Write(string name, string text)
        {
            string path = Path.Combine(root, name);
            File.WriteAllText(path, text);
            return path;
        }

        public void Dispose() => Directory.Delete(root, recursive: true);
    }
}
