using System.Collections.Generic;
using TheThingBelow.Tools.SteCheck;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The session number check of D-18, D-607, and L-12. Two providers picked the same number on
/// one day, so a machine reads the handoff and the archive as one list, newest first.
/// </summary>
public sealed class SteCheckSessionNumberTests
{
    [Fact]
    public void AHandoffInOrderGivesNoFinding()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();

        IReadOnlyList<Finding> findings = SessionNumberRules.Check(DocumentSet.Read(checkout.Root));

        Assert.Empty(findings);
    }

    [Fact]
    public void ARepeatedSessionNumberIsAFinding()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write(
            "docs/session-handoff.md",
            "# Session handoff",
            string.Empty,
            "## Session 2: 2026-09-13, Codex",
            string.Empty,
            "## Session 2: 2026-09-13, Claude Code",
            string.Empty,
            "## Session 1: 2026-09-12, Claude Code");

        IReadOnlyList<Finding> findings = SessionNumberRules.Check(DocumentSet.Read(checkout.Root));

        Finding found = Assert.Single(findings);
        Assert.Equal("HANDOFF 1", found.Rule);
        Assert.Equal(5, found.Line);
        Assert.Contains("session 2 appears again", found.Detail);
    }

    /// <summary>A heading of another level hides an entry from every other rule (D-18).</summary>
    [Fact]
    public void AnEntryUnderAHeadingOfAnotherLevelIsAFinding()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Append(
            "docs/session-handoff-archive.md",
            string.Empty,
            "# Session 3: 2026-09-14, Codex",
            string.Empty,
            "The third session, under a heading of the wrong level.");

        IReadOnlyList<Finding> findings = SessionNumberRules.Check(DocumentSet.Read(checkout.Root));

        Finding found = Assert.Single(findings);
        Assert.Equal("HANDOFF 4", found.Rule);
        Assert.Equal("docs/session-handoff-archive.md", found.File);
        Assert.Contains("session 3", found.Detail);
    }

    [Fact]
    public void AnEntryOutOfOrderIsAFinding()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write(
            "docs/session-handoff.md",
            "# Session handoff",
            string.Empty,
            "## Session 1: 2026-09-12, Claude Code",
            string.Empty,
            "## Session 2: 2026-09-13, Codex");

        IReadOnlyList<Finding> findings = SessionNumberRules.Check(DocumentSet.Read(checkout.Root));

        Finding found = Assert.Single(findings);
        Assert.Equal("HANDOFF 2", found.Rule);
        Assert.Equal(5, found.Line);
    }

    [Fact]
    public void AnArchiveEntryAboveTheHandoffIsAFinding()
    {
        // The two files hold one descending list, and the archive continues it (D-18).
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Append("docs/session-handoff-archive.md", string.Empty, "## Session 3: 2026-09-14, Codex");

        IReadOnlyList<Finding> findings = SessionNumberRules.Check(DocumentSet.Read(checkout.Root));

        Finding found = Assert.Single(findings);
        Assert.Equal("HANDOFF 2", found.Rule);
        Assert.Equal("docs/session-handoff-archive.md", found.File);
    }

    [Fact]
    public void MoreEntriesThanTheLimitIsAFinding()
    {
        // D-607: the handoff keeps the 10 newest entries, and every older entry moves (D-18).
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        List<string> lines = ["# Session handoff", string.Empty];
        for (int number = SessionNumberRules.HandoffEntryLimit + 1; number >= 1; number--)
        {
            lines.Add($"## Session {number}: 2026-09-13, Codex");
            lines.Add(string.Empty);
        }

        checkout.Write("docs/session-handoff.md", [.. lines]);

        IReadOnlyList<Finding> findings = SessionNumberRules.Check(DocumentSet.Read(checkout.Root));

        Finding found = Assert.Single(findings);
        Assert.Equal("HANDOFF 3", found.Rule);
        Assert.Contains("holds 11 entries", found.Detail);
    }

    [Fact]
    public void AHandoffWithNoEntryStopsTheRun()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write("docs/session-handoff.md", "# Session handoff", string.Empty, "No entry yet.");

        System.InvalidOperationException fault = Assert.Throws<System.InvalidOperationException>(
            () => SessionNumberRules.Check(DocumentSet.Read(checkout.Root)));

        Assert.Contains("## Session ", fault.Message);
    }
}
