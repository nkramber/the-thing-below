using System;
using System.Collections.Generic;
using TheThingBelow.Tools.CodexReview;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The rounds of each finding and the three-strike rule (D-929).</summary>
public sealed class CodexReviewFindingRoundsTests
{
    private const string Path = "docs/reviews/pr-63.md";
    private const string Round1 = "1111111111111111111111111111111111111111";
    private const string Round2 = "2222222222222222222222222222222222222222";
    private const string Round3 = "3333333333333333333333333333333333333333";
    private const string Round4 = "4444444444444444444444444444444444444444";

    [Fact]
    public void AFindingOpenInRoundsOneAndTwoPasses()
    {
        string record = Record(Finding("P1-1", "open", Round1, Round2));

        IReadOnlyList<ReviewFinding> findings = FindingRounds.Read(Path, record);
        FindingRounds.CheckHeads(Path, findings, Round2);

        Assert.Equal(2, findings[0].OpenHeads.Count);
        Assert.Empty(FindingRounds.Strikes(findings));
    }

    [Fact]
    public void AFindingOpenInRoundThreeStops()
    {
        string record = Record(Finding("P1-1", "open", Round1, Round2, Round3));

        IReadOnlyList<ReviewFinding> findings = FindingRounds.Read(Path, record);
        FindingRounds.CheckHeads(Path, findings, Round3);

        ReviewFinding strike = Assert.Single(FindingRounds.Strikes(findings));
        Assert.Equal("P1-1", strike.Id);
    }

    /// <summary>
    /// The owner counts the same id one time for each round in which it is open, and the rounds
    /// need not follow each other (D-929). A fix that closes the finding keeps its count.
    /// </summary>
    [Fact]
    public void AFindingThatAFixClosedAndARoundReopenedKeepsItsCount()
    {
        string reopened = Record(Finding("P2-1", "open", Round1, Round3));
        IReadOnlyList<ReviewFinding> atRound3 = FindingRounds.Read(Path, reopened);
        FindingRounds.CheckHeads(Path, atRound3, Round3);
        Assert.Empty(FindingRounds.Strikes(atRound3));

        string again = Record(Finding("P2-1", "open", Round1, Round3, Round4));
        IReadOnlyList<ReviewFinding> atRound4 = FindingRounds.Read(Path, again);
        FindingRounds.CheckHeads(Path, atRound4, Round4);
        Assert.Equal("P2-1", Assert.Single(FindingRounds.Strikes(atRound4)).Id);
    }

    [Fact]
    public void AFixedFindingWithThreeRoundsDoesNotStop()
    {
        string record = Record(Finding("P1-1", "fixed in `4444444`", Round1, Round2, Round3));

        IReadOnlyList<ReviewFinding> findings = FindingRounds.Read(Path, record);
        FindingRounds.CheckHeads(Path, findings, Round4);

        Assert.False(findings[0].IsOpen);
        Assert.Empty(FindingRounds.Strikes(findings));
    }

    [Fact]
    public void ARecordWithNoFindingGivesNone()
    {
        string record = Record("No finding.");

        Assert.Empty(FindingRounds.Read(Path, record));
    }

    [Fact]
    public void AFindingWithNoOpenAtLineIsAFault()
    {
        string record = Record("### P1-1: a fault\n\nStatus: open.\n\nFile: `a.cs:1`.");

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => FindingRounds.Read(Path, record));

        Assert.Contains("P1-1 has no `Open at:` line", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFindingWithNoStatusLineIsAFault()
    {
        string record = Record("### P1-1: a fault\n\nOpen at: `1111111`.");

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => FindingRounds.Read(Path, record));

        Assert.Contains("P1-1 has no `Status:` line", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnOpenFindingThatDoesNotListTheEffectiveHeadIsAFault()
    {
        string record = Record(Finding("P1-1", "open", Round1));
        IReadOnlyList<ReviewFinding> findings = FindingRounds.Read(Path, record);

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(
            () => FindingRounds.CheckHeads(Path, findings, Round2));

        Assert.Contains("P1-1 is open", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AClosedFindingThatListsTheEffectiveHeadIsAFault()
    {
        string record = Record(Finding("P1-1", "withdrawn", Round1, Round2));
        IReadOnlyList<ReviewFinding> findings = FindingRounds.Read(Path, record);

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(
            () => FindingRounds.CheckHeads(Path, findings, Round2));

        Assert.Contains("P1-1 is not open", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AHeadListedTwoTimesIsAFault()
    {
        string record = Record("### P1-1: a fault\n\nStatus: open.\n\nOpen at: `1111111`, `1111111111`.");

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => FindingRounds.Read(Path, record));

        Assert.Contains("two times", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AShortHashIsAFault()
    {
        string record = Record("### P1-1: a fault\n\nStatus: open.\n\nOpen at: `111`.");

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => FindingRounds.Read(Path, record));

        Assert.Contains("shorter than 7 letters", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AHeadingWithNoFindingIdIsAFault()
    {
        string record = Record("### A note\n\nStatus: open.\n\nOpen at: `1111111`.");

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => FindingRounds.Read(Path, record));

        Assert.Contains("names no finding id", fault.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Regression of P2-2 of the review of PR #63 on `ca9dd85`. Two sections of one id each
    /// held fewer than three heads, and the count missed their three rounds together (D-929).
    /// </summary>
    [Fact]
    public void AnIdThatComesTwoTimesIsAFault()
    {
        string record = Record(
            Finding("P1-1", "open", Round1, Round3) + "\n\n" + Finding("P1-1", "open", Round2, Round3));

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => FindingRounds.Read(Path, record));

        Assert.Contains("holds P1-1 two times", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARecordWithNoFindingsSectionIsAFault()
    {
        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(
            () => FindingRounds.Read(Path, "# PR-63 review\n\n## Verdict\n\n**Blocked.**\n"));

        Assert.Contains("no `## Findings` section", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCountReadsEachFindingAlone()
    {
        string record = Record(
            Finding("P1-1", "open", Round1, Round2, Round3) + "\n\n" + Finding("P2-1", "open", Round3));

        IReadOnlyList<ReviewFinding> findings = FindingRounds.Read(Path, record);
        FindingRounds.CheckHeads(Path, findings, Round3);

        Assert.Equal(["P1-1", "P2-1"], [findings[0].Id, findings[1].Id]);
        Assert.Equal("P1-1", Assert.Single(FindingRounds.Strikes(findings)).Id);
    }

    /// <summary>Gives the text of one finding in the form of the `pr-review` skill.</summary>
    /// <param name="id">The id of the finding.</param>
    /// <param name="status">The status text, such as `open`.</param>
    /// <param name="heads">The heads of the `Open at:` line.</param>
    /// <returns>The Markdown text of the finding.</returns>
    public static string Finding(string id, string status, params string[] heads)
    {
        List<string> listed = [];
        foreach (string head in heads)
        {
            listed.Add($"`{head[..7]}`");
        }

        return $"### {id}: a defect\n\nStatus: {status}.\n\n{FindingRounds.OpenAtLabel} {string.Join(", ", listed)}.\n\nFile: `a.cs:1`.";
    }

    /// <summary>Gives a review record with one Findings section.</summary>
    /// <param name="findings">The text of the Findings section.</param>
    /// <returns>The Markdown text of the record.</returns>
    public static string Record(string findings)
    {
        return $"# PR-63 review\n\n## Identity\n\n- Head: `{Round1[..7]}`\n\n## Findings\n\n{findings}\n\n## Out of scope\n\nNone.\n";
    }
}
