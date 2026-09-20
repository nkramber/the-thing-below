using System.Collections.Generic;
using System.Linq;
using TheThingBelow.Tools.ReviewGate;
using TheThingBelow.Tools.SteCheck;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// DOCS 1: the Documents rows of the PR template, of the `one-pr-one-session` skill, and of
/// the review gate are one list (D-579, D-581). The check runs on every PR, also on a
/// documents PR whose build and test jobs skip.
/// </summary>
public sealed class SteCheckDocumentRowTests
{
    [Fact]
    public void AFixtureWithMatchingRowsGivesNoFinding()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();

        IReadOnlyList<Finding> findings = DocumentRowRules.Check(DocumentSet.Read(checkout.Root));

        Assert.Empty(findings);
    }

    [Fact]
    public void ATemplateWithARowAbsentIsAFinding()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        List<string> lines = ["# The fixture template", string.Empty, "## Documents", string.Empty];
        lines.AddRange(DocumentRules.RequiredRows.Take(DocumentRules.RequiredRows.Count - 1).Select(row => $"- `{row}`:"));
        checkout.Write(DocumentRowRules.TemplatePath, [.. lines]);

        IReadOnlyList<Finding> findings = DocumentRowRules.Check(DocumentSet.Read(checkout.Root));

        Finding found = Assert.Single(findings);
        Assert.Equal("DOCS 1", found.Rule);
        Assert.Equal(DocumentRowRules.TemplatePath, found.File);
        Assert.Contains("11 Documents rows", found.Detail);
    }

    [Fact]
    public void ASkillWithRowsInAnotherOrderIsAFinding()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        List<string> lines = ["# The fixture skill", string.Empty, "## 3. Documents gate", string.Empty, "| Row | Changes |", "|---|---|"];
        lines.AddRange(DocumentRules.RequiredRows.Reverse().Select(row => $"| `{row}` | a change |"));
        checkout.Write(DocumentRowRules.SkillPath, [.. lines]);

        IReadOnlyList<Finding> findings = DocumentRowRules.Check(DocumentSet.Read(checkout.Root));

        Finding found = Assert.Single(findings);
        Assert.Equal("DOCS 1", found.Rule);
        Assert.Equal(DocumentRowRules.SkillPath, found.File);
        Assert.Contains("row 1", found.Detail);
    }

    [Fact]
    public void TheRowsOfThisCheckoutMatchTheGate()
    {
        IReadOnlyList<Finding> findings = DocumentRowRules.Check(DocumentSet.Read(RepositoryRoot.Find()));

        Assert.Empty(findings);
    }

    [Theory]
    [InlineData("- `docs/design.md`:", "docs/design.md")]
    [InlineData("- `CLAUDE.md` and `AGENTS.md`:", "CLAUDE.md and AGENTS.md")]
    [InlineData("<!-- a comment -->", null)]
    public void ATemplateLineGivesItsRow(string line, string? row)
    {
        Assert.Equal(row, DocumentRowRules.TemplateRowOf(line));
    }

    [Theory]
    [InlineData("| `docs/design.md` | intent, a guardrail |", "docs/design.md")]
    [InlineData("| `.claude/skills/` and `.claude/agents/` | a procedure |", ".claude/skills/ and .claude/agents/")]
    [InlineData("| Document or category | Changes when the PR changes |", null)]
    [InlineData("|---|---|", null)]
    public void ASkillLineGivesItsRow(string line, string? row)
    {
        Assert.Equal(row, DocumentRowRules.SkillRowOf(line));
    }
}
