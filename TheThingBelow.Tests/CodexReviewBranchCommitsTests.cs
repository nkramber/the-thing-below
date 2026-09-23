using System;
using System.Collections.Generic;
using TheThingBelow.Tools.CodexReview;
using TheThingBelow.Tools.ReviewGate;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The commits of a pull request, and its effective head after a review commit (D-610).</summary>
public sealed class CodexReviewBranchCommitsTests
{
    private const string Code = "1111111111111111111111111111111111111111";
    private const string Review = "2222222222222222222222222222222222222222";

    [Fact]
    public void TheLogOutputGivesEachCommitWithItsPaths()
    {
        string log =
            $"commit:{Code}\n\nMakefile\nTheThingBelow.Tools/Program.cs\n" +
            $"commit:{Review}\n\ndocs/reviews/pr-63.md\ndocs/session-handoff.md\n";

        IReadOnlyList<CommitFacts> commits = BranchCommits.Parse(log);

        Assert.Equal(2, commits.Count);
        Assert.Equal(Code, commits[0].Sha);
        Assert.Equal(["Makefile", "TheThingBelow.Tools/Program.cs"], commits[0].Files);
        Assert.Equal(["docs/reviews/pr-63.md", "docs/session-handoff.md"], commits[1].Files);
    }

    /// <summary>The review commit of the reviewer is metadata, so the record names the code commit.</summary>
    [Fact]
    public void TheReviewCommitDoesNotMoveTheEffectiveHead()
    {
        string log = $"commit:{Code}\nMakefile\ncommit:{Review}\ndocs/reviews/pr-63.md\ndocs/session-handoff.md\n";

        CommitFacts? head = EffectiveHead.Find(BranchCommits.Parse(log), 63);

        Assert.Equal(Code, head?.Sha);
    }

    [Fact]
    public void APathBeforeTheFirstCommitIsAFault()
    {
        Assert.Throws<InvalidOperationException>(() => BranchCommits.Parse("Makefile\n"));
    }

    [Fact]
    public void TheLogArgumentsListTheCommitsOldestFirst()
    {
        IReadOnlyList<string> arguments = BranchCommits.LogArguments("abc1234..origin/feat/pr-95-codex-review");

        Assert.Equal("log", arguments[0]);
        Assert.Contains("--reverse", arguments);
        Assert.Contains("--name-only", arguments);
        Assert.Contains($"--format={BranchCommits.CommitMarker}%H", arguments);
        Assert.Equal("abc1234..origin/feat/pr-95-codex-review", arguments[^1]);
    }
}
