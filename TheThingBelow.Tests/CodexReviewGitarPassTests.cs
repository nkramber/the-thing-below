using System;
using System.Collections.Generic;
using TheThingBelow.Tools.CodexReview;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The machine part of a complete Gitar pass, which a review run needs first (D-14, D-705).</summary>
public sealed class CodexReviewGitarPassTests
{
    private const string Push = "2026-09-23T10:00:00Z";
    private const string BeforePush = "2026-09-23T09:59:00Z";
    private const string AfterPush = "2026-09-23T10:04:00Z";
    private const string Later = "2026-09-23T10:09:00Z";

    [Fact]
    public void ACurrentPassWithNoOpenThreadIsComplete()
    {
        GitarFacts facts = Facts([Dashboard(AfterPush)], [], [new GitarThread(true, GitarPass.GraphLogin)]);

        Assert.Empty(GitarPass.Check(facts));
    }

    [Fact]
    public void NoDashboardIsNotComplete()
    {
        GitarFacts facts = Facts([], [], []);

        Assert.Contains(GitarPass.Check(facts), reason => reason.Contains("no Gitar dashboard", StringComparison.Ordinal));
    }

    [Fact]
    public void ADashboardOfAnEarlierPushIsStale()
    {
        GitarFacts facts = Facts([Dashboard(BeforePush)], [], []);

        Assert.Contains(GitarPass.Check(facts), reason => reason.Contains("stale", StringComparison.Ordinal));
    }

    [Fact]
    public void ADashboardOfAnotherAuthorDoesNotCount()
    {
        GitarComment copy = new GitarComment("nkramber", AfterPush, AfterPush, true, false);
        GitarFacts facts = Facts([copy], [], []);

        Assert.Contains(GitarPass.Check(facts), reason => reason.Contains("no Gitar dashboard", StringComparison.Ordinal));
    }

    [Fact]
    public void ARequestWithNoLaterDashboardEditIsNotComplete()
    {
        GitarComment request = new GitarComment("nkramber", Later, Later, false, true);
        GitarFacts facts = Facts([Dashboard(AfterPush), request], [], []);

        Assert.Contains(GitarPass.Check(facts), reason => reason.Contains("manual review did not end", StringComparison.Ordinal));
    }

    [Fact]
    public void ARequestWithALaterDashboardEditIsComplete()
    {
        GitarComment request = new GitarComment("nkramber", AfterPush, AfterPush, false, true);
        GitarFacts facts = Facts([request, Dashboard(Later)], [], []);

        Assert.Empty(GitarPass.Check(facts));
    }

    [Theory]
    [InlineData("queued")]
    [InlineData("in_progress")]
    public void ARunningGitarCheckIsNotComplete(string status)
    {
        GitarFacts facts = Facts([Dashboard(AfterPush)], [status], []);

        Assert.Contains(GitarPass.Check(facts), reason => reason.Contains(status, StringComparison.Ordinal));
    }

    [Fact]
    public void ACompletedGitarCheckIsComplete()
    {
        GitarFacts facts = Facts([Dashboard(AfterPush)], ["completed"], []);

        Assert.Empty(GitarPass.Check(facts));
    }

    [Fact]
    public void AnOpenThreadOfGitarIsNotComplete()
    {
        GitarFacts facts = Facts([Dashboard(AfterPush)], [], [new GitarThread(false, GitarPass.GraphLogin)]);

        Assert.Contains(GitarPass.Check(facts), reason => reason.Contains("1 review thread(s)", StringComparison.Ordinal));
    }

    /// <summary>
    /// Regression of P2-1 of the review of PR #63 on `ca9dd85`. The query read the first 100
    /// threads alone, so an open thread on a later page passed.
    /// </summary>
    [Fact]
    public void AnOpenThreadAfterTheFirstHundredIsNotComplete()
    {
        List<GitarThread> threads = [];
        for (int index = 0; index < 100; index++)
        {
            threads.Add(new GitarThread(true, GitarPass.GraphLogin));
        }

        threads.Add(new GitarThread(false, GitarPass.GraphLogin));
        GitarFacts facts = Facts([Dashboard(AfterPush)], [], threads);

        Assert.Contains(GitarPass.Check(facts), reason => reason.Contains("1 review thread(s)", StringComparison.Ordinal));
    }

    [Fact]
    public void TheThreadQueryReadsEveryPage()
    {
        IReadOnlyList<string> arguments = GitarPass.ThreadArguments("nkramber", "the-thing-below", "63");
        string query = Assert.Single(arguments, argument => argument.StartsWith("query=", StringComparison.Ordinal));

        Assert.Contains("--paginate", arguments);
        Assert.Contains("$endCursor: String", query, StringComparison.Ordinal);
        Assert.Contains("after: $endCursor", query, StringComparison.Ordinal);
        Assert.Contains("pageInfo { hasNextPage endCursor }", query, StringComparison.Ordinal);
    }

    [Fact]
    public void EachRestReadTakesEveryPage()
    {
        IReadOnlyList<string>[] reads =
        [
            GitarPass.CommentArguments("nkramber/the-thing-below", "63"),
            GitarPass.CheckRunArguments("nkramber/the-thing-below", "ca9dd85"),
            GitarPass.CheckSuiteArguments("nkramber/the-thing-below", "ca9dd85"),
        ];

        foreach (IReadOnlyList<string> arguments in reads)
        {
            Assert.Contains("--paginate", arguments);
            Assert.Contains(arguments, argument => argument.EndsWith("?per_page=100", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void AnUnknownPushTimeIsNotComplete()
    {
        GitarFacts facts = new GitarFacts([Dashboard(AfterPush)], [], [], null);

        Assert.Contains(GitarPass.Check(facts), reason => reason.Contains("push time is unknown", StringComparison.Ordinal));
    }

    [Fact]
    public void ATimeOutsideTheUtcFormIsAFault()
    {
        GitarFacts facts = Facts([Dashboard("2026-09-23 10:04")], [], []);

        Assert.Throws<InvalidOperationException>(() => GitarPass.Check(facts));
    }

    [Fact]
    public void TheCommentLinesOfTheCommandParse()
    {
        string lines =
            "{\"created\":\"2026-09-23T06:49:33Z\",\"dashboard\":true,\"login\":\"gitar-bot[bot]\",\"request\":false,\"updated\":\"2026-09-23T06:50:01Z\"}\n" +
            "{\"created\":\"2026-09-23T06:51:00Z\",\"dashboard\":false,\"login\":\"nkramber\",\"request\":true,\"updated\":\"2026-09-23T06:51:00Z\"}\n";

        IReadOnlyList<GitarComment> comments = GitarPass.ParseComments(lines);

        Assert.Equal(2, comments.Count);
        Assert.Equal(new GitarComment(GitarPass.RestLogin, "2026-09-23T06:49:33Z", "2026-09-23T06:50:01Z", true, false), comments[0]);
        Assert.True(comments[1].IsRequest);
    }

    [Fact]
    public void ACommentLineWithAnAbsentFieldIsAFault()
    {
        string line = "{\"created\":\"2026-09-23T06:49:33Z\",\"login\":\"gitar-bot[bot]\",\"request\":false,\"updated\":\"2026-09-23T06:50:01Z\"}";

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => GitarPass.ParseComments(line));

        Assert.Contains("`dashboard`", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheThreadLinesOfTheCommandParse()
    {
        IReadOnlyList<GitarThread> threads = GitarPass.ParseThreads("{\"author\":\"gitar-bot\",\"resolved\":false}\n");

        Assert.Equal(new GitarThread(false, GitarPass.GraphLogin), Assert.Single(threads));
    }

    [Fact]
    public void TheEarliestSuiteTimeIsThePushTime()
    {
        Assert.Equal(Push, GitarPass.Earliest($"{AfterPush}\n{Push}\n{Later}\n"));
        Assert.Null(GitarPass.Earliest(string.Empty));
    }

    private static GitarComment Dashboard(string updated)
    {
        return new GitarComment(GitarPass.RestLogin, BeforePush, updated, true, false);
    }

    private static GitarFacts Facts(IReadOnlyList<GitarComment> comments, IReadOnlyList<string> statuses, IReadOnlyList<GitarThread> threads)
    {
        return new GitarFacts(comments, statuses, threads, Push);
    }
}
