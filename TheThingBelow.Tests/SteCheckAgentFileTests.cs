using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Tools.SteCheck;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The rule of ste-check that `CLAUDE.md` and `AGENTS.md` stay identical (D-20, D-857). A
/// docs-only PR skips the build and test job, so this rule is the check of D-20 on that PR.
/// </summary>
public sealed class SteCheckAgentFileTests
{
    [Fact]
    public void TwoIdenticalFilesGiveNoFinding()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();

        IReadOnlyList<Finding> findings = AgentFileRules.Check(DocumentSet.Read(checkout.Root));

        Assert.Empty(findings);
    }

    [Fact]
    public void AChangeToOneFileAloneIsAFindingAtTheLineThatDiffers()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write(
            "AGENTS.md",
            "# The fixture instructions",
            string.Empty,
            "This file holds a rule that the other file does not hold.");

        IReadOnlyList<Finding> findings = AgentFileRules.Check(DocumentSet.Read(checkout.Root));

        Finding found = Assert.Single(findings);
        Assert.Equal(AgentFileRules.RuleId, found.Rule);
        Assert.Equal("AGENTS.md", found.File);
        Assert.Equal(3, found.Line);
        Assert.Contains("D-20", found.Detail, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("AGENTS.md", 6, "holds 6 lines, and `CLAUDE.md` holds 5")]
    [InlineData("CLAUDE.md", 6, "holds 5 lines, and `CLAUDE.md` holds 6")]
    public void ALineAddedToOneFileAloneIsAFinding(string path, int line, string detail)
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Append(path, "An added rule.");

        IReadOnlyList<Finding> findings = AgentFileRules.Check(DocumentSet.Read(checkout.Root));

        Finding found = Assert.Single(findings);
        Assert.Equal("AGENTS.md", found.File);
        Assert.Equal(line, found.Line);
        Assert.Contains(detail, found.Detail, StringComparison.Ordinal);
    }

    /// <summary>
    /// The regression test of F-113. The old rule read the lines alone, so a change of the last
    /// line end, or of each line end, passed ste-check and failed `AgentFileTests` on `main`.
    /// </summary>
    [Theory]
    [InlineData("# Rules\nA rule.\n", "# Rules\nA rule.", 2)]
    [InlineData("# Rules\nA rule.", "# Rules\nA rule.\n", 2)]
    [InlineData("# Rules\nA rule.\n", "# Rules\r\nA rule.\r\n", 2)]
    [InlineData("# Rules\nA rule.\n", "# Rules\nA rule.\n\n", 3)]
    public void TwoFilesThatDifferInALineEndAloneGiveAFinding(string claudeText, string agentsText, int line)
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        File.WriteAllText(Path.Combine(checkout.Root, "CLAUDE.md"), claudeText);
        File.WriteAllText(Path.Combine(checkout.Root, "AGENTS.md"), agentsText);

        IReadOnlyList<Finding> findings = AgentFileRules.Check(DocumentSet.Read(checkout.Root));

        Finding found = Assert.Single(findings);
        Assert.Equal(AgentFileRules.RuleId, found.Rule);
        Assert.Equal("AGENTS.md", found.File);
        Assert.Equal(line, found.Line);
    }

    [Fact]
    public void TwoFilesWithTheSameBytesGiveNoFinding()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        File.WriteAllText(Path.Combine(checkout.Root, "CLAUDE.md"), "# Rules\r\nA rule.");
        File.WriteAllText(Path.Combine(checkout.Root, "AGENTS.md"), "# Rules\r\nA rule.");

        Assert.Empty(AgentFileRules.Check(DocumentSet.Read(checkout.Root)));
    }

    [Theory]
    [InlineData("AGENTS.md")]
    [InlineData("CLAUDE.md")]
    public void AnAbsentFileStopsTheCheck(string path)
    {
        // An absent file is an error, never a pass (T-2).
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        File.Delete(Path.Combine(checkout.Root, path));

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(
            () => AgentFileRules.Check(DocumentSet.Read(checkout.Root)));

        Assert.Contains(path, fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCommandReportsTheFinding()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Append("CLAUDE.md", "An added rule.");
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = SteCheckCommand.Run([SteCheckCommand.RootOption, checkout.Root], output, errors);

        Assert.Equal(1, exitCode);
        Assert.Contains($"AGENTS.md:6: rule {AgentFileRules.RuleId}:", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void EachFileWithTheTestCommandOneTimeGivesNoFinding()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();

        IReadOnlyList<Finding> findings = AgentFileRules.CheckTestCommand(DocumentSet.Read(checkout.Root));

        Assert.Empty(findings);
    }

    [Theory]
    [InlineData("CLAUDE.md")]
    [InlineData("AGENTS.md")]
    [InlineData(".claude/skills/csharp-conventions/SKILL.md")]
    public void AFileWithNoTestCommandIsAFinding(string path)
    {
        // A docs-only PR skips `TestFilterTests`, so this rule reads the same count (D-857).
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write(path, "# The fixture file", string.Empty, "This file lost its test command.");

        IReadOnlyList<Finding> findings = AgentFileRules.CheckTestCommand(DocumentSet.Read(checkout.Root));

        Finding found = Assert.Single(findings);
        Assert.Equal(AgentFileRules.TestCommandRuleId, found.Rule);
        Assert.Equal(path, found.File);
        Assert.Contains("0 time(s)", found.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void AFileWithTheTestCommandTwoTimesIsAFindingAtTheFirstLine()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Append(".claude/skills/csharp-conventions/SKILL.md", "- Again: `dotnet test -- " + AgentFileRules.ExcludeSmoke + "`");

        IReadOnlyList<Finding> findings = AgentFileRules.CheckTestCommand(DocumentSet.Read(checkout.Root));

        Finding found = Assert.Single(findings);
        Assert.Equal(3, found.Line);
        Assert.Contains("2 time(s)", found.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAbsentFileOfTheTestCommandStopsTheCheck()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        File.Delete(Path.Combine(checkout.Root, ".claude", "skills", "csharp-conventions", "SKILL.md"));

        Assert.Throws<InvalidOperationException>(
            () => AgentFileRules.CheckTestCommand(DocumentSet.Read(checkout.Root)));
    }

    [Fact]
    public void TheRuleReadsTheOptionAndTheFilesOfTestFilterTests()
    {
        // `TestFilterTests` holds the count of each file, and each file of the skip set in it
        // holds the option one time. The rule must read the same option and the same files.
        Assert.Equal(TestCategories.ExcludeSmoke, AgentFileRules.ExcludeSmoke);
        Assert.Equal(
            ["CLAUDE.md", "AGENTS.md", ".claude/skills/csharp-conventions/SKILL.md"],
            AgentFileRules.TestCommandPaths);
    }

    [Fact]
    public void TheCommittedFilesPassTheRule()
    {
        IReadOnlyList<Finding> findings = AgentFileRules.CheckTestCommand(DocumentSet.Read(RepositoryRoot.PathTo(".")));

        Assert.Empty(findings);
    }
}
