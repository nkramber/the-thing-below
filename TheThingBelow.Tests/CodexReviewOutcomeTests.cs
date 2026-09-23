using System;
using TheThingBelow.Tools;
using TheThingBelow.Tools.CodexReview;
using TheThingBelow.Tools.ReviewGate;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The outcome of one review run, and its exit code (D-926, D-929).</summary>
public sealed class CodexReviewOutcomeTests
{
    private const string Path = "docs/reviews/pr-63.md";
    private const string Head = "3333333333333333333333333333333333333333";
    private const string Earlier = "1111111111111111111111111111111111111111";
    private const string Middle = "2222222222222222222222222222222222222222";

    private static readonly CommitFacts EffectiveHead = new CommitFacts(Head, ["TheThingBelow.Tools/Program.cs"]);

    [Fact]
    public void EachOutcomeHasItsOwnExitCode()
    {
        int[] codes =
        [
            ReviewOutcome.ApproveExitCode,
            ReviewOutcome.ChangesRequiredExitCode,
            ReviewOutcome.ThreeStrikesExitCode,
            Program.FaultExitCode,
        ];

        Assert.Equal(codes.Length, new System.Collections.Generic.HashSet<int>(codes).Count);
        Assert.Equal(0, ReviewOutcome.ApproveExitCode);
    }

    [Fact]
    public void AnApprovalOfTheEffectiveHeadGivesZero()
    {
        string record = Record(Head, "Ready for owner merge", "No finding.");

        ReviewOutcome outcome = ReviewOutcome.Decide(0, Path, record, EffectiveHead);

        Assert.Equal(ReviewOutcomeKind.Approve, outcome.Kind);
        Assert.Equal(0, outcome.ExitCode);
        Assert.Empty(outcome.OpenFindings);
    }

    [Theory]
    [InlineData("Changes required")]
    [InlineData("Blocked")]
    public void AVerdictThatRefusesTheMergeGivesTwo(string verdict)
    {
        string record = Record(Head, verdict, CodexReviewFindingRoundsTests.Finding("P1-1", "open", Head));

        ReviewOutcome outcome = ReviewOutcome.Decide(0, Path, record, EffectiveHead);

        Assert.Equal(ReviewOutcomeKind.ChangesRequired, outcome.Kind);
        Assert.Equal(ReviewOutcome.ChangesRequiredExitCode, outcome.ExitCode);
        Assert.Equal(verdict, outcome.Verdict);
        Assert.Equal("P1-1", Assert.Single(outcome.OpenFindings));
    }

    [Fact]
    public void AFindingOpenForTheThirdTimeGivesTheStrikeCode()
    {
        string finding = CodexReviewFindingRoundsTests.Finding("P2-1", "open", Earlier, Middle, Head);
        string record = Record(Head, "Changes required", finding);

        ReviewOutcome outcome = ReviewOutcome.Decide(0, Path, record, EffectiveHead);

        Assert.Equal(ReviewOutcomeKind.ThreeStrikes, outcome.Kind);
        Assert.Equal(ReviewOutcome.ThreeStrikesExitCode, outcome.ExitCode);
        Assert.Equal("P2-1", Assert.Single(outcome.Strikes).Id);
    }

    /// <summary>The stop wins over an approval, because the owner decides each third return (D-929).</summary>
    [Fact]
    public void TheStrikeWinsOverAnApproval()
    {
        string finding = CodexReviewFindingRoundsTests.Finding("P3-1", "open", Earlier, Middle, Head);
        string record = Record(Head, "Ready for owner merge", finding);

        ReviewOutcome outcome = ReviewOutcome.Decide(0, Path, record, EffectiveHead);

        Assert.Equal(ReviewOutcomeKind.ThreeStrikes, outcome.Kind);
    }

    [Fact]
    public void AnErrorOfTheCliIsAFault()
    {
        string record = Record(Head, "Ready for owner merge", "No finding.");

        ReviewOutcome outcome = ReviewOutcome.Decide(1, Path, record, EffectiveHead);

        Assert.Equal(ReviewOutcomeKind.Fault, outcome.Kind);
        Assert.Equal(Program.FaultExitCode, outcome.ExitCode);
        Assert.Null(outcome.Verdict);
    }

    [Fact]
    public void NoRecordOnOriginIsAFault()
    {
        ReviewOutcome outcome = ReviewOutcome.Decide(0, Path, null, EffectiveHead);

        Assert.Equal(ReviewOutcomeKind.Fault, outcome.Kind);
        Assert.Contains("no review record", outcome.Reason, StringComparison.Ordinal);
    }

    /// <summary>A stale approval never gives the approval code, so it never turns on the auto-merge (D-610, D-930).</summary>
    [Fact]
    public void AnApprovalOfAnOlderHeadIsAFault()
    {
        string record = Record(Earlier, "Ready for owner merge", "No finding.");

        ReviewOutcome outcome = ReviewOutcome.Decide(0, Path, record, EffectiveHead);

        Assert.Equal(ReviewOutcomeKind.Fault, outcome.Kind);
        Assert.Contains("effective head", outcome.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void ABranchWithNoEffectiveHeadIsAFault()
    {
        string record = Record(Head, "Ready for owner merge", "No finding.");

        ReviewOutcome outcome = ReviewOutcome.Decide(0, Path, record, null);

        Assert.Equal(ReviewOutcomeKind.Fault, outcome.Kind);
    }

    [Fact]
    public void TwoVerdictNamesAreAFault()
    {
        string record = Record(Head, "Ready for owner merge", "No finding.")
            .Replace("This verdict", "**Blocked.** This verdict", StringComparison.Ordinal);

        ReviewOutcome outcome = ReviewOutcome.Decide(0, Path, record, EffectiveHead);

        Assert.Equal(ReviewOutcomeKind.Fault, outcome.Kind);
        Assert.Contains("2 bold names", outcome.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void AMalformedFindingIsAFault()
    {
        string record = Record(Head, "Changes required", "### P1-1: a defect\n\nStatus: open.");

        ReviewOutcome outcome = ReviewOutcome.Decide(0, Path, record, EffectiveHead);

        Assert.Equal(ReviewOutcomeKind.Fault, outcome.Kind);
        Assert.Contains("Open at:", outcome.Reason, StringComparison.Ordinal);
    }

    private static string Record(string head, string verdict, string findings)
    {
        return
            "# PR-63 review\n\n" +
            $"## Identity\n\n- PR: 63\n- Head: `{head[..7]}`\n\n" +
            $"## Findings\n\n{findings}\n\n" +
            "## Out of scope\n\nNone.\n\n" +
            $"## Verdict\n\n**{verdict}.** This verdict applies to head `{head[..7]}`.\n";
    }
}
