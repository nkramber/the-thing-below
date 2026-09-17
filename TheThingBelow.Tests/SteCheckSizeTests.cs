using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Tools.SteCheck;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The size rules of the context budget (D-583, D-611). A session reads the start set in full,
/// so the instructions file, the top handoff entry, and each skill file stay under a byte limit.
/// </summary>
public sealed class SteCheckSizeTests
{
    [Fact]
    public void AStartSetUnderEveryLimitGivesNoFinding()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();

        IReadOnlyList<Finding> findings = SizeRules.Check(DocumentSet.Read(checkout.Root));

        Assert.Empty(findings);
    }

    [Theory]
    [InlineData("AGENTS.md")]
    [InlineData("CLAUDE.md")]
    public void AnInstructionsFileAboveTheLimitIsAFinding(string path)
    {
        // Exit test 2 of PR-84: the finding names the size and the limit (D-611).
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.WriteOfSize(path, SizeRules.InstructionsLimitBytes + 1);

        IReadOnlyList<Finding> findings = SizeRules.Check(DocumentSet.Read(checkout.Root));

        Finding found = Assert.Single(findings);
        Assert.Equal("SIZE 1", found.Rule);
        Assert.Equal(path, found.File);
        Assert.Equal(1, found.Line);
        Assert.Contains($"holds {SizeRules.InstructionsLimitBytes + 1} bytes", found.Detail, StringComparison.Ordinal);
        Assert.Contains($"the limit is {SizeRules.InstructionsLimitBytes} bytes", found.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void ATopHandoffEntryAboveTheLimitIsAFinding()
    {
        // Exit test 3 of PR-84. The finding names the line of the heading of the entry.
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write("docs/session-handoff.md", [.. BuildHandoff(SizeRules.HandoffEntryLimitBytes + 1, 40)]);

        IReadOnlyList<Finding> findings = SizeRules.Check(DocumentSet.Read(checkout.Root));

        Finding found = Assert.Single(findings);
        Assert.Equal("SIZE 2", found.Rule);
        Assert.Equal("docs/session-handoff.md", found.File);
        Assert.Equal(3, found.Line);
        Assert.Contains($"the top entry holds {SizeRules.HandoffEntryLimitBytes + 1} bytes", found.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void AnOlderHandoffEntryAboveTheLimitGivesNoFinding()
    {
        // The rule reads the top entry alone, because a session reads that entry alone (D-584).
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write("docs/session-handoff.md", [.. BuildHandoff(40, SizeRules.HandoffEntryLimitBytes + 1)]);

        IReadOnlyList<Finding> findings = SizeRules.Check(DocumentSet.Read(checkout.Root));

        Assert.Empty(findings);
    }

    [Fact]
    public void ASubHeadingInsideTheTopEntryDoesNotEndIt()
    {
        // A session heading has the form `## Session <number>:` (D-18, L-12). A sub-heading that
        // starts with the same words must not end the measured region, or the rule undercounts.
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        List<string> lines = ["# Session handoff", string.Empty];
        lines.AddRange(BuildEntry(2, SizeRules.HandoffEntryLimitBytes));
        lines.Add("## Session numbering rules");
        lines.AddRange(BuildEntry(1, 40));
        checkout.Write("docs/session-handoff.md", [.. lines]);

        IReadOnlyList<Finding> findings = SizeRules.Check(DocumentSet.Read(checkout.Root));

        Finding found = Assert.Single(findings);
        Assert.Equal("SIZE 2", found.Rule);
        Assert.Contains("the top entry holds 5147 bytes", found.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void ASkillFileAboveTheLimitIsAFinding()
    {
        // Exit test 4 of PR-84. Every `.md` file of a skill folder takes the rule (D-21).
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.WriteOfSize(".claude/skills/pr-review/references/review-record.md", SizeRules.SkillLimitBytes + 1);

        IReadOnlyList<Finding> findings = SizeRules.Check(DocumentSet.Read(checkout.Root));

        Finding found = Assert.Single(findings);
        Assert.Equal("SIZE 3", found.Rule);
        Assert.Equal(".claude/skills/pr-review/references/review-record.md", found.File);
        Assert.Contains($"the limit is {SizeRules.SkillLimitBytes} bytes", found.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void AFileOfTheExactLimitPasses()
    {
        // Exit test 5 of PR-84: the limit is the largest size that passes.
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.WriteOfSize("CLAUDE.md", SizeRules.InstructionsLimitBytes);
        checkout.WriteOfSize(".claude/skills/ste-writing/SKILL.md", SizeRules.SkillLimitBytes);
        checkout.Write("docs/session-handoff.md", [.. BuildHandoff(SizeRules.HandoffEntryLimitBytes, 40)]);

        IReadOnlyList<Finding> findings = SizeRules.Check(DocumentSet.Read(checkout.Root));

        Assert.Empty(findings);
    }

    [Fact]
    public void ThreeFilesAboveTheLimitGiveThreeFindings()
    {
        // One finding for each file above its limit (D-611).
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.WriteOfSize("CLAUDE.md", SizeRules.InstructionsLimitBytes + 64);
        checkout.WriteOfSize("AGENTS.md", SizeRules.InstructionsLimitBytes + 64);
        checkout.WriteOfSize(".claude/skills/ste-writing/SKILL.md", SizeRules.SkillLimitBytes + 64);

        IReadOnlyList<Finding> findings = SizeRules.Check(DocumentSet.Read(checkout.Root));

        Assert.Equal(3, findings.Count);
        string[] expected = ["AGENTS.md", "CLAUDE.md", ".claude/skills/ste-writing/SKILL.md"];
        Assert.Equal(expected, Paths(findings));
    }

    [Fact]
    public void AnAbsentInstructionsFileStopsTheRun()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        File.Delete(Path.Combine(checkout.Root, "AGENTS.md"));

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(
            () => SizeRules.Check(DocumentSet.Read(checkout.Root)));

        Assert.Contains("AGENTS.md", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ACheckoutWithNoSkillFileStopsTheRun()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        Directory.Delete(Path.Combine(checkout.Root, ".claude"), recursive: true);

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(
            () => SizeRules.Check(DocumentSet.Read(checkout.Root)));

        Assert.Contains(SizeRules.SkillsFolder, fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AHandoffWithNoEntryStopsTheRun()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write("docs/session-handoff.md", "# Session handoff", string.Empty, "No entry yet.");

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(
            () => SizeRules.Check(DocumentSet.Read(checkout.Root)));

        Assert.Contains("## Session ", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheByteCountGivesOneByteForEachLineEnding()
    {
        // The count reads the lines, so a checkout with a carriage return gives the same number.
        Assert.Equal(6, SizeRules.ByteCount(["ab", "cd"]));
        Assert.Equal(3, SizeRules.ByteCount(["é"]));
    }

    /// <summary>Builds a handoff of two entries, each of an exact byte count.</summary>
    /// <param name="topBytes">The count of the top entry, 30 bytes at least.</param>
    /// <param name="olderBytes">The count of the older entry, 30 bytes at least.</param>
    /// <returns>The lines of the file. The first entry heading is on line 3.</returns>
    private static IReadOnlyList<string> BuildHandoff(int topBytes, int olderBytes)
    {
        List<string> lines = ["# Session handoff", string.Empty];
        lines.AddRange(BuildEntry(2, topBytes));
        lines.AddRange(BuildEntry(1, olderBytes));
        return lines;
    }

    private static IReadOnlyList<string> BuildEntry(int number, int bytes)
    {
        string heading = $"## Session {number}: 2026-09-13, Codex";
        int remaining = bytes - SizeRules.ByteCount([heading]);
        Assert.True(remaining >= 0, $"An entry of {bytes} bytes cannot hold the heading of session {number}.");

        List<string> lines = [heading];
        while (remaining > 64)
        {
            lines.Add(new string('x', 63));
            remaining -= 64;
        }

        if (remaining > 0)
        {
            lines.Add(new string('x', remaining - 1));
        }

        return lines;
    }

    private static IReadOnlyList<string> Paths(IReadOnlyList<Finding> findings)
    {
        List<string> paths = [];
        foreach (Finding finding in findings)
        {
            paths.Add(finding.File);
        }

        return paths;
    }
}
