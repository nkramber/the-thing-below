using System;
using System.IO;
using TheThingBelow.Tools;
using TheThingBelow.Tools.Night;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The walk of D-1204 and the `night-walk` command.</summary>
public sealed class NightWalkTests
{
    private const string First = "1111111111111111111111111111111111111111";

    private const string Second = "2222222222222222222222222222222222222222";

    private const string Third = "3333333333333333333333333333333333333333";

    [Fact]
    public void CommitsOfDocumentsAloneAfterACodeCommitReachBackToIt()
    {
        NightCommit[] commits =
        [
            new(First, 1, ["TheThingBelow.Core/Battle.cs"]),
            new(Second, 1, ["docs/reviews/pr-90.md", "docs/session-handoff.md"]),
            new(Third, 1, ["docs/decisions.md"]),
        ];

        Assert.Equal([Third, Second, First], NightWalk.CandidatesOf(commits, Third));
    }

    [Fact]
    public void ACodeCommitEndsTheWalk()
    {
        NightCommit[] commits =
        [
            new(First, 1, ["TheThingBelow.Core/Battle.cs"]),
            new(Second, 1, ["TheThingBelow.Core/Battle.cs", "docs/design.md"]),
            new(Third, 1, ["docs/decisions.md"]),
        ];

        Assert.Equal([Third, Second], NightWalk.CandidatesOf(commits, Third));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(0)]
    public void ACommitWithOtherThanOneParentEndsTheWalk(int parents)
    {
        // A merge of `main` brings code of other PRs, even when the API lists doc paths alone.
        NightCommit[] commits = [new(First, 1, ["TheThingBelow.Core/Battle.cs"]), new(Second, parents, ["docs/design.md"])];

        Assert.Equal([Second], NightWalk.CandidatesOf(commits, Second));
    }

    [Fact]
    public void ACommitWithNoPathEndsTheWalk()
    {
        NightCommit[] commits = [new(First, 1, ["TheThingBelow.Core/Battle.cs"]), new(Second, 1, [])];

        Assert.Equal([Second], NightWalk.CandidatesOf(commits, Second));
    }

    [Theory]
    [InlineData(".github/workflows/night.yml")]
    [InlineData(".claude/settings.json")]
    [InlineData("LICENSE")]
    public void APathOutsideTheDocsOnlySetEndsTheWalk(string path)
    {
        // D-1204 takes the paths of a docs-only PR (D-513), not the skip set of D-943.
        NightCommit[] commits = [new(First, 1, ["TheThingBelow.Core/Battle.cs"]), new(Second, 1, [path])];

        Assert.Equal([Second], NightWalk.CandidatesOf(commits, Second));
    }

    [Fact]
    public void AListThatEndsWithAnotherCommitFails()
    {
        NightCommit[] commits = [new(First, 1, ["a.cs"]), new(Second, 1, ["b.cs"])];

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => NightWalk.CandidatesOf(commits, Third));

        Assert.Contains($"the head of the PR is {Third}", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCommandWritesTheWalkNewestFirst()
    {
        string path = Path.Combine(Path.GetTempPath(), "night-walk-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            NightGateFixture.WriteCommitsFile(path, new NightCommit(First, 1, ["TheThingBelow.Core/Battle.cs"]), new NightCommit(Second, 1, ["docs/design.md"]));
            using StringWriter output = new();
            using StringWriter errors = new();

            int exitCode = Program.Run([NightWalkCommand.Name, "--commits", path, "--head", Second], output, errors);

            Assert.Equal(0, exitCode);
            Assert.Equal([Second, First], output.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void TheCommandWithABadFileNamesTheFault()
    {
        string path = Path.Combine(Path.GetTempPath(), "night-walk-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(path, """[{"sha": "short", "parents": 1, "files": []}]""");
            using StringWriter output = new();
            using StringWriter errors = new();

            int exitCode = Program.Run([NightWalkCommand.Name, "--commits", path, "--head", Second], output, errors);

            Assert.Equal(Program.FaultExitCode, exitCode);
            Assert.Contains("full commit hash", errors.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void TheCommandWithNoOptionNamesEachOption()
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        Assert.Equal(Program.FaultExitCode, Program.Run([NightWalkCommand.Name], output, errors));
        Assert.Contains("needs --commits and --head", errors.ToString(), StringComparison.Ordinal);
    }
}
