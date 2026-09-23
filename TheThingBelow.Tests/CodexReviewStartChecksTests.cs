using System;
using TheThingBelow.Tools.CodexReview;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The checks that refuse a review run before it starts (D-926, T-2).</summary>
public sealed class CodexReviewStartChecksTests
{
    private const string Branch = "feat/pr-95-codex-review";
    private const string Head = "1111111111111111111111111111111111111111";
    private const string Other = "2222222222222222222222222222222222222222";

    private static readonly CheckoutFacts Ready =
        new CheckoutFacts(63, StartChecks.OpenState, Branch, Head, Branch, Head, Head, string.Empty);

    [Fact]
    public void AnOpenPullRequestAndAMatchingCleanCheckoutStart()
    {
        Assert.Empty(StartChecks.Check(Ready));
    }

    [Theory]
    [InlineData("MERGED")]
    [InlineData("CLOSED")]
    public void APullRequestThatIsNotOpenIsRefused(string state)
    {
        CheckoutFacts facts = Ready with { State = state };

        Assert.Contains(StartChecks.Check(facts), reason => reason.Contains(state, StringComparison.Ordinal));
    }

    [Fact]
    public void ACheckoutOnAnotherBranchIsRefused()
    {
        CheckoutFacts facts = Ready with { LocalBranch = "main" };

        Assert.Contains(StartChecks.Check(facts), reason => reason.Contains("the checkout is on `main`", StringComparison.Ordinal));
    }

    [Fact]
    public void ACheckoutWithNoBranchIsRefused()
    {
        CheckoutFacts facts = Ready with { LocalBranch = null };

        Assert.Contains(StartChecks.Check(facts), reason => reason.Contains("no branch", StringComparison.Ordinal));
    }

    [Fact]
    public void ALocalHeadThatDiffersFromOriginIsRefused()
    {
        CheckoutFacts facts = Ready with { LocalHead = Other };

        Assert.Contains(StartChecks.Check(facts), reason => reason.Contains("Push or pull first", StringComparison.Ordinal));
    }

    [Fact]
    public void AnOriginHeadThatDiffersFromGitHubIsRefused()
    {
        CheckoutFacts facts = Ready with { GitHubHead = Other };

        Assert.Contains(StartChecks.Check(facts), reason => reason.Contains("GitHub gives the head", StringComparison.Ordinal));
    }

    [Fact]
    public void AWorkingTreeWithChangesIsRefused()
    {
        CheckoutFacts facts = Ready with { Status = " M Makefile\n?? docs/reviews/pr-62-response.md\n" };

        string reason = Assert.Single(StartChecks.Check(facts));

        Assert.Contains("?? docs/reviews/pr-62-response.md", reason, StringComparison.Ordinal);
    }
}
