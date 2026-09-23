using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public void ACommitOfDocumentsAloneKeepsTheEarlierHeadsReviewable()
    {
        const string Documents = "3333333333333333333333333333333333333333";
        List<CommitFacts> commits =
        [
            new CommitFacts(Code, ["TheThingBelow.Core/Rules.cs"]),
            new CommitFacts(Metadata, ["docs/reviews/pr-21.md", "docs/session-handoff.md"]),
            new CommitFacts(Documents, ["docs/decisions.md", ".claude/settings.json", "LICENSE"]),
        ];

        IReadOnlyList<CommitFacts> heads = EffectiveHead.ReviewableHeads(commits, 21);

        Assert.Equal([Documents, Code], heads.Select(head => head.Sha));
    }

    [Fact]
    public void ACommitOutsideTheSkipSetEndsTheReviewableHeads()
    {
        const string Documents = "3333333333333333333333333333333333333333";
        const string Workflow = "4444444444444444444444444444444444444444";
        List<CommitFacts> commits =
        [
            new CommitFacts(Code, ["TheThingBelow.Core/Rules.cs"]),
            new CommitFacts(Workflow, [".github/workflows/ci.yml"]),
            new CommitFacts(Documents, ["docs/design.md"]),
        ];

        IReadOnlyList<CommitFacts> heads = EffectiveHead.ReviewableHeads(commits, 21);

        Assert.Equal([Documents, Workflow], heads.Select(head => head.Sha));
    }

    [Fact]
    public void APullRequestOfMetadataAloneHasNoReviewableHead()
    {
        List<CommitFacts> commits = [new CommitFacts(Metadata, ["docs/session-handoff.md"])];

        Assert.Empty(EffectiveHead.ReviewableHeads(commits, 21));
    }

    [Fact]
    public void APullRequestOfMetadataAloneHasNoEffectiveHead()
    {
        List<CommitFacts> commits = [new CommitFacts(Metadata, ["docs/session-handoff.md"])];

        Assert.Null(EffectiveHead.Find(commits, 21));
    }
}

/// <summary>The rules of the `review-override` label (D-16, D-71, D-239, D-401, D-560, D-609, D-700).</summary>
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
    [InlineData(".claude/agents/design-critic.md")]
    [InlineData(".claude/settings.json.md")]
    [InlineData("docs/.claude/settings.json")]
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
    [InlineData(".claude/settings.json")]
    public void EachPathOutsideTheEligibleSetFails(string path)
    {
        PullRequestFacts facts = ReviewGateFixture.LabeledFacts() with { Files = [path] };

        Assert.Equal(GateResult.Fault, OverrideRules.CheckPaths(facts).Result);
    }

    [Fact]
    public void TheHarnessSettingsFileTakesTheReviewAndTheFaultNamesD700()
    {
        // The regression test of D-700: the old code took the file as a path of `.claude/`.
        // The file can hold a hook that runs a command in each session. It stands second
        // here, so an eligible path before it hides nothing.
        PullRequestFacts facts = ReviewGateFixture.LabeledFacts() with
        {
            Files = [".claude/skills/pr-review/SKILL.md", OverrideRules.HarnessSettingsPath],
        };

        GateCheck check = OverrideRules.CheckPaths(facts);

        Assert.Equal(GateResult.Fault, check.Result);
        Assert.Contains("`.claude/settings.json`", check.Detail, StringComparison.Ordinal);
        Assert.Contains("D-700", check.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void AWorkflowPathKeepsItsOwnReason()
    {
        PullRequestFacts facts = ReviewGateFixture.LabeledFacts() with { Files = [".github/workflows/ci.yml"] };

        GateCheck check = OverrideRules.CheckPaths(facts);

        Assert.Contains("D-560", check.Detail, StringComparison.Ordinal);
        Assert.DoesNotContain("D-700", check.Detail, StringComparison.Ordinal);
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
    public void AVerdictLineOfBlockedFailsWhenTheProseNamesTheApprovedVerdict()
    {
        string text = "## Verdict\n\n**Blocked.** The review needs the head. Ready for owner merge is not the result.\n";

        GateCheck check = ReviewRecordRules.CheckVerdict("docs/reviews/pr-21.md", text);

        Assert.Equal(GateResult.Fault, check.Result);
        Assert.Contains("is `Blocked`", check.Detail, StringComparison.Ordinal);
    }

    // The regression test of P1-1 of `docs/reviews/pr-21.md`. The rule read the section as one
    // text, so a record that refuses the merge held the approved verdict as a part of a word group.
    [Fact]
    public void ANegatedApprovedVerdictFails()
    {
        string text = "## Verdict\n\n**Not Ready for owner merge.** This verdict applies to head `1111111`.\n";

        GateCheck check = ReviewRecordRules.CheckVerdict("docs/reviews/pr-21.md", text);

        Assert.Equal(GateResult.Fault, check.Result);
        Assert.Contains("Not Ready for owner merge", check.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void AVerdictSectionWithNoBoldVerdictLineFails()
    {
        string text = "## Verdict\n\nReady for owner merge. This verdict applies to head `1111111`.\n";

        GateCheck check = ReviewRecordRules.CheckVerdict("docs/reviews/pr-21.md", text);

        Assert.Equal(GateResult.Fault, check.Result);
        Assert.Contains("holds no verdict line", check.Detail, StringComparison.Ordinal);
    }

    // The regression tests of P1-3 of `docs/reviews/pr-21.md`. The rule read the first bold
    // name alone, so a second verdict after the approved one passed the gate.
    [Fact]
    public void ASecondVerdictLineAfterTheApprovedVerdictFails()
    {
        string text = string.Join(
            '\n',
            "## Verdict",
            string.Empty,
            "**Ready for owner merge.** This verdict applies to head `1111111`.",
            "**Changes required.** The record needs the new head.",
            string.Empty);

        GateCheck check = ReviewRecordRules.CheckVerdict("docs/reviews/pr-21.md", text);

        Assert.Equal(GateResult.Fault, check.Result);
        Assert.Contains("gives 2 bold names", check.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void ASecondVerdictOnTheLineOfTheApprovedVerdictFails()
    {
        string text = "## Verdict\n\n**Ready for owner merge.** The owner can merge. **Changes required.**\n";

        GateCheck check = ReviewRecordRules.CheckVerdict("docs/reviews/pr-21.md", text);

        Assert.Equal(GateResult.Fault, check.Result);
        Assert.Contains("gives 2 bold names", check.Detail, StringComparison.Ordinal);
    }

    // The regression test of P1-4 of `docs/reviews/pr-21.md`. The rule counted the verdict names
    // alone, so a bold negation beside the approved name passed.
    [Fact]
    public void ABoldNegationAfterTheApprovedVerdictFails()
    {
        string text = string.Join(
            '\n',
            "## Verdict",
            string.Empty,
            "**Ready for owner merge.** This verdict applies to head `1111111`.",
            "**Not Ready for owner merge.** The record needs the new head.",
            string.Empty);

        GateCheck check = ReviewRecordRules.CheckVerdict("docs/reviews/pr-21.md", text);

        Assert.Equal(GateResult.Fault, check.Result);
        Assert.Contains("Not Ready for owner merge", check.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEarlierVerdictInAnotherSectionPasses()
    {
        string text = string.Join(
            '\n',
            "## Earlier verdicts",
            string.Empty,
            "**Changes required.** This verdict applied to head `2222222`.",
            string.Empty,
            "## Verdict",
            string.Empty,
            "**Ready for owner merge.** This verdict applies to head `1111111`.",
            string.Empty);

        GateCheck check = ReviewRecordRules.CheckVerdict("docs/reviews/pr-21.md", text);

        Assert.Equal(GateResult.Pass, check.Result);
    }

    // The regression test of P1-2 of `docs/reviews/pr-21.md`. The rule read every line of the
    // record, so a head field of another section passed a stale Identity list.
    [Fact]
    public void AHeadFieldOutsideTheIdentityListFails()
    {
        // The Identity list holds no head field, and another section names the effective head.
        string text = string.Join(
            '\n',
            "## Identity",
            string.Empty,
            "- PR: 21",
            "- Target: `main`",
            string.Empty,
            "## Verification",
            string.Empty,
            $"- Head: `{Head}` is the head of origin/feat/pr-3-review-gate.",
            string.Empty);

        GateCheck check = ReviewRecordRules.CheckHead(
            "docs/reviews/pr-21.md",
            text,
            [new CommitFacts(Head, ["docs/design.md"])]);

        Assert.Equal(GateResult.Fault, check.Result);
        Assert.Contains("holds no head field", check.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void ARecordWithNoIdentitySectionFails()
    {
        GateCheck check = ReviewRecordRules.CheckHead(
            "docs/reviews/pr-21.md",
            $"# PR-21 review\n\n- Head: `{Head}`\n",
            [new CommitFacts(Head, ["docs/design.md"])]);

        Assert.Equal(GateResult.Fault, check.Result);
        Assert.Contains("no `## Identity` section", check.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void AShortHashOfTheEffectiveHeadPasses()
    {
        string text = ReviewGateFixture.Record(Head[..8], "Ready for owner merge");

        GateCheck check = ReviewRecordRules.CheckHead(
            "docs/reviews/pr-21.md",
            text,
            [new CommitFacts(Head, ["docs/design.md"])]);

        Assert.Equal(GateResult.Pass, check.Result);
    }

    [Fact]
    public void AHashShorterThanSevenLettersFails()
    {
        string text = ReviewGateFixture.Record(Head[..5], "Ready for owner merge");

        GateCheck check = ReviewRecordRules.CheckHead(
            "docs/reviews/pr-21.md",
            text,
            [new CommitFacts(Head, ["docs/design.md"])]);

        Assert.Equal(GateResult.Fault, check.Result);
        Assert.Contains("shorter than 7", check.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void ARecordWithNoHeadFieldFails()
    {
        GateCheck check = ReviewRecordRules.CheckHead(
            "docs/reviews/pr-21.md",
            "## Identity\n\n- PR: 21\n",
            [new CommitFacts(Head, ["docs/design.md"])]);

        Assert.Equal(GateResult.Fault, check.Result);
        Assert.Contains("holds no head field", check.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void APullRequestOfMetadataAloneFailsTheHeadRule()
    {
        GateCheck check = ReviewRecordRules.CheckHead("docs/reviews/pr-21.md", "## Identity\n", []);

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

    /// <summary>A line reads true against the diff, and not in its form alone (D-577).</summary>
    [Fact]
    public void AChangedLineWithNoChangedPathOfItsRowIsAFault()
    {
        string? fault = DocumentRules.TruthFault(
            "docs/roadmaps/",
            "Changed: docs/roadmaps/phase-1-foundations.md. The scope of PR-5.",
            ["docs/design.md", "docs/session-handoff.md"]);

        Assert.NotNull(fault);
        Assert.Contains("docs/roadmaps/", fault, StringComparison.Ordinal);
    }

    [Fact]
    public void ANoChangeLineWithAChangedPathOfItsRowIsAFault()
    {
        string? fault = DocumentRules.TruthFault(
            "docs/world/",
            "No change needed because docs/world/ holds no lore of this PR.",
            ["docs/world/cast.md", "docs/session-handoff.md"]);

        Assert.NotNull(fault);
        Assert.Contains("docs/world/cast.md", fault, StringComparison.Ordinal);
    }

    [Fact]
    public void ANoChangeLineOfTheReviewsRowPassesWhenTheReviewerAddedTheRecord()
    {
        // The reviewer commits the record after the author wrote the description.
        string? fault = DocumentRules.TruthFault(
            "docs/reviews/",
            "No change needed because docs/reviews/pr-21.md comes from the reviewer.",
            ["docs/reviews/pr-21.md", "docs/session-handoff.md"]);

        Assert.Null(fault);
    }

    [Theory]
    [InlineData("CLAUDE.md and AGENTS.md", "AGENTS.md")]
    [InlineData(".claude/skills/ and .claude/agents/", ".claude/agents/design-critic.md")]
    public void ARowOfTwoNamesReadsBothPaths(string row, string file)
    {
        string? fault = DocumentRules.TruthFault(row, "Changed: " + file + ". A rule of the sessions.", [file]);

        Assert.Null(fault);
    }

    [Fact]
    public void TheSectionRuleReadsTheTruthOfEachLine()
    {
        // The passing facts change `docs/design.md`, so a line that says no change fails RG 7.
        Dictionary<string, string?> changes = new(StringComparer.Ordinal)
        {
            ["docs/design.md"] = "No change needed because docs/design.md holds no rule of this PR.",
        };
        PullRequestFacts facts = ReviewGateFixture.PassingFacts() with { Body = ReviewGateFixture.Body(changes) };

        IReadOnlyList<GateCheck> checks = DocumentRules.CheckSection(facts);

        Assert.Equal(GateResult.Fault, checks[0].Result);
        Assert.Contains("docs/design.md", checks[0].Detail, StringComparison.Ordinal);
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
    // The regression test of P2-1 of `docs/reviews/pr-21.md`. The rule read `pr` alone, so each
    // spelled-out form of a phrase passed.
    [InlineData("No change needed because a separate pull request holds `docs/design.md`.")]
    [InlineData("Changed: `docs/design.md`. A later pull request adds the section.")]
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
