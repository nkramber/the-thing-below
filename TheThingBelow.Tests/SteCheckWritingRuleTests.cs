using System.Collections.Generic;
using TheThingBelow.Tools.SteCheck;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The writing rules of the `ste-check` command. One fixture fails each rule one time, and one
/// fixture passes every rule (PR-2 exit tests 2 and 3).
/// </summary>
public sealed class SteCheckWritingRuleTests
{
    [Fact]
    public void TheFailingFixtureFailsEachRuleOnItsOwnLine()
    {
        IReadOnlyList<string> document = SteCheckFixtures.BuildFailingDocument();

        IReadOnlyList<Finding> findings = WritingRules.Check("fixture.md", document);

        foreach (SteCheckFixtures.Case failing in SteCheckFixtures.FailingCases)
        {
            int line = SteCheckFixtures.LineOf(failing.Rule);
            Assert.Contains(
                findings,
                found => found.Rule == failing.Rule
                    && found.Line == line
                    && found.File == "fixture.md");
        }
    }

    [Fact]
    public void EveryFindingNamesTheFileTheLineAndTheRule()
    {
        IReadOnlyList<string> document = SteCheckFixtures.BuildFailingDocument();

        IReadOnlyList<Finding> findings = WritingRules.Check("docs/fixture.md", document);

        Assert.NotEmpty(findings);
        Assert.All(findings, found =>
        {
            Assert.Equal("docs/fixture.md", found.File);
            Assert.InRange(found.Line, 1, document.Count);
            Assert.NotEmpty(found.Rule);
            Assert.NotEmpty(found.Detail);
            Assert.StartsWith("docs/fixture.md:", found.ToString(), System.StringComparison.Ordinal);
        });
    }

    /// <summary>An irregular participle that the never-a-participle list also holds is a dead entry.</summary>
    [Fact]
    public void NoIrregularParticipleIsAlsoNeverAParticiple()
    {
        Assert.Empty(EnglishWords.DeadIrregularWords());
    }

    [Fact]
    public void ThePassingFixtureGivesNoFinding()
    {
        IReadOnlyList<Finding> findings = WritingRules.Check("fixture.md", SteCheckFixtures.PassingDocument);

        Assert.Empty(findings);
    }

    [Theory]
    [InlineData("1. This numbered item holds one sentence of more than twenty words, and the rule of a numbered item allows twenty words.", "STE 5.1")]
    [InlineData("- This bullet item holds one sentence of more than twenty five words, and the rule of a sentence outside a numbered item allows twenty five words at most in it.", "STE 6.3")]
    public void TheWordLimitOfANumberedItemIsLowerThanTheLimitOfABulletItem(string line, string rule)
    {
        IReadOnlyList<Finding> findings = WritingRules.Check("fixture.md", [line]);

        Assert.Contains(findings, found => found.Rule == rule);
    }

    [Fact]
    public void ANumberedItemUnderAnyHeadingTakesTheLimitOfTwentyWords()
    {
        // D-604: the limit reads every numbered item, and no heading changes it (F-5, OQ-67).
        IReadOnlyList<string> document =
        [
            "## A heading that names no procedure",
            string.Empty,
            "1. This numbered item holds one sentence of more than twenty words, and the rule of a numbered item allows twenty words.",
        ];

        IReadOnlyList<Finding> findings = WritingRules.Check("fixture.md", document);

        Assert.Contains(findings, found => found.Rule == "STE 5.1" && found.Line == 3);
    }

    [Fact]
    public void ACommentOnOneLineHidesNoProseFromTheRules()
    {
        // F-11: the interim checker read an HTML comment as prose, and it raised four findings.
        IReadOnlyList<string> document =
            ["<!-- The session should read this; it isn't prose, and the text is written here. -->"];

        IReadOnlyList<Finding> findings = WritingRules.Check("fixture.md", document);

        Assert.Empty(findings);
    }

    [Theory]
    [InlineData("The reviewer says that he's ready.")]
    [InlineData("The reviewer says that she's ready.")]
    [InlineData("The record names who's on the PR.")]
    [InlineData("The record says that it's ready.")]
    public void APronounWithAnApostropheAndAnSIsAContraction(string line)
    {
        // The possessive of each of these pronouns has no apostrophe: its, his, hers, and whose.
        IReadOnlyList<Finding> findings = WritingRules.Check("fixture.md", [line]);

        Assert.Contains(findings, found => found.Rule == "STE 4.2");
    }

    [Theory]
    [InlineData("The answer of the owner stands, and the owner's word is the rule.")]
    [InlineData("The record names whose branch holds the head.")]
    [InlineData("The record names its branch and its head.")]
    public void APossessiveIsNotAContraction(string line)
    {
        IReadOnlyList<Finding> findings = WritingRules.Check("fixture.md", [line]);

        Assert.DoesNotContain(findings, found => found.Rule == "STE 4.2");
    }

    [Fact]
    public void ATableAndAFencedBlockTakeNoRule()
    {
        IReadOnlyList<string> document =
        [
            "| A cell that should hold a semicolon; and a passive that is written here | yes |",
            string.Empty,
            "```",
            "The code should hold a semicolon; and the text is written by the tool.",
            "```",
        ];

        IReadOnlyList<Finding> findings = WritingRules.Check("fixture.md", document);

        Assert.Empty(findings);
    }
}
