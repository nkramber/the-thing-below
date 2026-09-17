using System;
using System.IO;
using TheThingBelow.Tools;
using TheThingBelow.Tools.SteCheck;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The `ste-check` command end to end. It reads a checkout, prints one line for each finding,
/// and gives the fault code when a rule finds one (D-10, T-2).
/// </summary>
public sealed class SteCheckCommandTests
{
    [Fact]
    public void ACheckoutThatPassesEveryRuleGivesTheCodeZero()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(["ste-check", "--root", checkout.Root], output, errors);

        Assert.Equal(0, exitCode);
        Assert.Contains("0 finding(s)", output.ToString(), StringComparison.Ordinal);
        Assert.Empty(errors.ToString());
    }

    [Fact]
    public void AFindingNamesTheFileTheLineAndTheRule()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write("docs/note.md", "The note reads the file; the note prints the finding.");
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(["ste-check", "--root", checkout.Root], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("docs/note.md:1: rule STE 8.1: semicolon", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("1 finding(s)", output.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("docs/reviews/pr-1.md")]
    [InlineData("docs/session-handoff.md")]
    [InlineData("docs/session-handoff-archive.md")]
    [InlineData("docs/archive/design-v1.md")]
    public void TheCommandSkipsEachDatedRecord(string path)
    {
        // D-10: a dated record is history, and a rewrite falsifies it.
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        const string FailingLine = "The record should hold a semicolon; and the text is written by the session.";
        if (path == "docs/session-handoff.md")
        {
            // The handoff keeps its entries, because the session number check reads them (D-18).
            checkout.Write(
                path,
                FailingLine,
                string.Empty,
                "## Session 2: 2026-09-13, Codex",
                string.Empty,
                "## Session 1: 2026-09-12, Claude Code");
        }
        else
        {
            checkout.Write(path, FailingLine);
        }
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(["ste-check", "--root", checkout.Root], output, errors);

        Assert.Equal(0, exitCode);
        Assert.DoesNotContain(path + ":", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ACheckoutWithNoLiveDocumentNamesTheRoot()
    {
        string root = Path.Combine(Path.GetTempPath(), "ste-check-empty-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using StringWriter output = new StringWriter();
            using StringWriter errors = new StringWriter();

            int exitCode = Program.Run(["ste-check", "--root", root], output, errors);

            Assert.Equal(Program.FaultExitCode, exitCode);
            Assert.Contains("holds no live document", errors.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ACheckoutWithNoRegisterNamesTheRegister()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        File.Delete(Path.Combine(checkout.Root, "docs", "questions.md"));
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(["ste-check", "--root", checkout.Root], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("docs/questions.md", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void EveryLiveDocumentOfThisRepositoryPassesEveryRule()
    {
        // PR-2 exit test 1. The four dated records stay out of the writing rules (D-10).
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(["ste-check", "--root", RepositoryRoot.Find()], output, errors);

        Assert.Equal(0, exitCode);
        Assert.Empty(errors.ToString());
    }
}
