using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Tools.ReviewGate;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The effective head of a pull request (D-603, D-610).</summary>
public sealed class ReviewGateHeadTests
{
    private const string Code = "1111111111111111111111111111111111111111";
    private const string Metadata = "2222222222222222222222222222222222222222";

    [Fact]
    public void AMetadataCommitDoesNotMoveTheEffectiveHead()
    {
        List<CommitFacts> commits =
        [
            new CommitFacts(Code, ["docs/design.md"]),
            new CommitFacts(Metadata, ["docs/reviews/pr-21.md", "docs/session-handoff.md"]),
        ];

        CommitFacts? head = EffectiveHead.Find(commits, 21);

        Assert.Equal(Code, head?.Sha);
    }

    [Fact]
    public void ARecordOfAnotherPullRequestMovesTheEffectiveHead()
    {
        List<CommitFacts> commits =
        [
            new CommitFacts(Code, ["docs/design.md"]),
            new CommitFacts(Metadata, ["docs/reviews/pr-12.md"]),
        ];

        CommitFacts? head = EffectiveHead.Find(commits, 21);

        Assert.Equal(Metadata, head?.Sha);
    }

    [Fact]
    public void TheMetadataSetHoldsTheFourPathsOfThePullRequest()
    {
        IReadOnlyList<string> paths = EffectiveHead.MetadataPaths(21);

        Assert.Equal(
            ["docs/reviews/pr-21.md", "docs/reviews/pr-21-response.md", "docs/session-handoff.md", "docs/session-handoff-archive.md"],
            paths);
    }

    [Fact]
    public void APullRequestOfMetadataAloneHasNoEffectiveHead()
    {
        List<CommitFacts> commits = [new CommitFacts(Metadata, ["docs/session-handoff.md"])];

        Assert.Null(EffectiveHead.Find(commits, 21));
    }
}

/// <summary>The rules of the `review-override` label (D-16, D-71, D-239, D-401, D-560, D-609).</summary>
public sealed class ReviewGateOverrideRuleTests
{
    [Fact]
    public void AnUnlabeledPullRequestPassesBothLabelRules()
    {
        PullRequestFacts facts = ReviewGateFixture.PassingFacts();

        Assert.Equal(GateResult.Pass, OverrideRules.CheckPaths(facts).Result);
        Assert.Equal(GateResult.Pass, OverrideRules.CheckDecisionRows(facts).Result);
    }

    [Theory]
    [InlineData("docs/design.md")]
    [InlineData(".claude/skills/pr-review/SKILL.md")]
    [InlineData("CLAUDE.md")]
    [InlineData("AGENTS.md")]
    [InlineData("README.md")]
    [InlineData(".github/pull_request_template.md")]
    public void EachPathOfTheEligibleSetPasses(string path)
    {
        PullRequestFacts facts = ReviewGateFixture.LabeledFacts() with { Files = [path] };

        Assert.Equal(GateResult.Pass, OverrideRules.CheckPaths(facts).Result);
    }

    [Theory]
    [InlineData(".github/workflows/ci.yml")]
    [InlineData("Makefile")]
    [InlineData("content/items.json")]
    [InlineData("LICENSE")]
    public void EachPathOutsideTheEligibleSetFails(string path)
    {
        PullRequestFacts facts = ReviewGateFixture.LabeledFacts() with { Files = [path] };

        Assert.Equal(GateResult.Fault, OverrideRules.CheckPaths(facts).Result);
    }

    [Fact]
    public void TheHeadLineAndTheDividerLineOfATableAreNoDecisionRows()
    {
        Assert.False(OverrideRules.IsDecisionRow("| Id | Date | Topic | Decision | Effect |"));
        Assert.False(OverrideRules.IsDecisionRow("|---|---|---|---|---|"));
        Assert.False(OverrideRules.IsDecisionRow("The text above a table."));
        Assert.True(OverrideRules.IsDecisionRow("| D-1 | 2026-09-12 | A topic | An answer. | An effect. |"));
    }

    [Fact]
    public void AChangeToTheTextAboveTheTablesIsNoRowChange()
    {
        string diff = string.Join(
            '\n',
            "--- a/docs/decisions.md",
            "+++ b/docs/decisions.md",
            "@@ -3,3 +3,3 @@",
            "-Status: active register. Owner: Nate.",
            "+Status: the active register. Owner: Nate.",
            " | D-1 | 2026-09-12 | A topic | An answer. | An effect. |");

        Assert.Empty(OverrideRules.ChangedDecisionRows(diff));
    }

    [Fact]
    public void AChangedRowOfADecisionTableIsARowChange()
    {
        string diff = string.Join(
            '\n',
            "--- a/docs/decisions.md",
            "+++ b/docs/decisions.md",
            "@@ -3,3 +3,3 @@",
            "-| D-1 | 2026-09-12 | A topic | An answer. | An effect. |",
            "+| D-1 | 2026-09-12 | A topic | An answer. | An effect. Revised by D-2. |");

        Assert.Equal(2, OverrideRules.ChangedDecisionRows(diff).Count);
    }
}

/// <summary>The three rules of the review record (D-15, D-17).</summary>
public sealed class ReviewGateRecordRuleTests
{
    private const string Head = "1111111111111111111111111111111111111111";

    [Fact]
    public void ARecordWithNoVerdictSectionFails()
    {
        GateCheck check = ReviewRecordRules.CheckVerdict("docs/reviews/pr-21.md", "# PR-21 review\n");

        Assert.Equal(GateResult.Fault, check.Result);
        Assert.Contains("no `## Verdict` section", check.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void ARecordWithTwoVerdictNamesFails()
    {
        string text = "## Verdict\n\n**Blocked.** The review needs the head. Ready for owner merge is not the result.\n";

        GateCheck check = ReviewRecordRules.CheckVerdict("docs/reviews/pr-21.md", text);

        Assert.Equal(GateResult.Fault, check.Result);
        Assert.Contains("names 2 verdicts", check.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void AShortHashOfTheEffectiveHeadPasses()
    {
        string text = ReviewGateFixture.Record(Head[..8], "Ready for owner merge");

        GateCheck check = ReviewRecordRules.CheckHead(
            "docs/reviews/pr-21.md",
            text,
            new CommitFacts(Head, ["docs/design.md"]));

        Assert.Equal(GateResult.Pass, check.Result);
    }

    [Fact]
    public void AHashShorterThanSevenLettersFails()
    {
        string text = ReviewGateFixture.Record(Head[..5], "Ready for owner merge");

        GateCheck check = ReviewRecordRules.CheckHead(
            "docs/reviews/pr-21.md",
            text,
            new CommitFacts(Head, ["docs/design.md"]));

        Assert.Equal(GateResult.Fault, check.Result);
        Assert.Contains("shorter than 7", check.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void ARecordWithNoHeadFieldFails()
    {
        GateCheck check = ReviewRecordRules.CheckHead(
            "docs/reviews/pr-21.md",
            "## Identity\n\n- PR: 21\n",
            new CommitFacts(Head, ["docs/design.md"]));

        Assert.Equal(GateResult.Fault, check.Result);
        Assert.Contains("holds no head field", check.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void APullRequestOfMetadataAloneFailsTheHeadRule()
    {
        GateCheck check = ReviewRecordRules.CheckHead("docs/reviews/pr-21.md", "## Identity\n", null);

        Assert.Equal(GateResult.Fault, check.Result);
        Assert.Contains("no effective head", check.Detail, StringComparison.Ordinal);
    }
}

/// <summary>The document rules of the description (D-577, D-579, D-581).</summary>
public sealed class ReviewGateDocumentRuleTests
{
    [Fact]
    public void TheRequiredRowsMatchThePullRequestTemplate()
    {
        string template = File.ReadAllText(RepositoryRoot.PathTo(".github/pull_request_template.md"));
        IReadOnlyList<string>? lines = MarkdownSection.ReadLines(template, DocumentRules.SectionHeading);

        Assert.NotNull(lines);
        foreach (string row in DocumentRules.RequiredRows)
        {
            Assert.NotNull(DocumentRules.ReadRow(lines, row));
        }
    }

    [Fact]
    public void TheRequiredRowsMatchTheTableOfTheSkill()
    {
        string skill = File.ReadAllText(RepositoryRoot.PathTo(".claude/skills/one-pr-one-session/SKILL.md"));

        foreach (string row in DocumentRules.RequiredRows)
        {
            string cell = "| `" + string.Join("` and `", row.Split(" and ")) + "` |";
            Assert.Contains(cell, skill, StringComparison.Ordinal);
        }
    }

    [Theory]
    [InlineData("Changed: `docs/design.md`. The section on the gate names the command.")]
    [InlineData("No change needed because `docs/design.md` holds no rule of this PR.")]
    [InlineData("Not applicable because this PR holds no lore and no place.")]
    public void EachFormOfDecision581Passes(string content)
    {
        Assert.Null(DocumentRules.FormFault("docs/design.md", content));
    }

    [Theory]
    [InlineData("")]
    [InlineData("no documentation impact")]
    [InlineData("Changed: `docs/design.md`")]
    [InlineData("No change needed because nothing changes")]
    [InlineData("Not applicable because no")]
    public void EachLineOutsideTheFormsFails(string content)
    {
        Assert.NotNull(DocumentRules.FormFault("docs/design.md", content));
    }

    [Fact]
    public void TheHandoffRowTakesTheChangedFormAlone()
    {
        string line = "No change needed because `docs/session-handoff.md` holds the entry of Session 1.";

        Assert.NotNull(DocumentRules.FormFault(DocumentRules.HandoffPath, line));
        Assert.Null(DocumentRules.FormFault(
            DocumentRules.HandoffPath,
            "Changed: `docs/session-handoff.md`. The entry of this session is at the top."));
    }

    [Theory]
    [InlineData("Changed: `docs/design.md`. A later PR adds the section.")]
    [InlineData("No change needed because `docs/design.md` gets its section after the merge.")]
    [InlineData("Changed: `docs/design.md`. The roadmap entry is TBD.")]
    [InlineData("No change needed because a docs PR records `docs/design.md`.")]
    public void EachDeferralOfADocumentOfThisPullRequestFails(string content)
    {
        Assert.NotNull(DocumentRules.DeferralPhrase(content));
    }

    [Fact]
    public void ALineThatNamesThePullRequestOfAnAbsentCheckHoldsNoDeferral()
    {
        string content = "No change needed because `docs/design.md` names PR-46 for the det-lint job (G-16).";

        Assert.Null(DocumentRules.DeferralPhrase(content));
    }

    [Fact]
    public void ADescriptionWithNoHandoffChangeFails()
    {
        PullRequestFacts facts = ReviewGateFixture.PassingFacts() with { Files = ["docs/design.md"] };

        Assert.Equal(GateResult.Fault, DocumentRules.CheckHandoff(facts).Result);
    }
}
