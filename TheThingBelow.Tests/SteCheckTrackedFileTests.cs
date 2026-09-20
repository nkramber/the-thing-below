using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Tools;
using TheThingBelow.Tools.SteCheck;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The file set of the `ste-check` command in a checkout with git data. The command reads
/// the files that git tracks alone, and a `git` run that fails is an error (D-702, T-2).
/// </summary>
public sealed class SteCheckTrackedFileTests
{
    private const string FailingLine = "The note reads the file; the note prints the finding.";

    [Fact]
    public void ATrackedDocumentTakesTheRulesAndAnUntrackedOneTakesNone()
    {
        // The regression test of D-702: the old code read the folder tree, so the untracked
        // note gave a finding, and each commit needed the note outside the tree.
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write("docs/tracked-note.md", FailingLine);
        checkout.TrackEveryFile();
        checkout.Write("HANDOFF-NOTE.md", FailingLine);
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(["ste-check", "--root", checkout.Root], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("docs/tracked-note.md:1: rule STE 8.1", output.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("HANDOFF-NOTE.md", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("1 finding(s)", output.ToString(), StringComparison.Ordinal);
        Assert.Empty(errors.ToString());
    }

    [Fact]
    public void ACheckoutWithGitDataThatPassesEveryRuleGivesTheCodeZero()
    {
        // The rows of DOCS 1 cite folders, so this run also proves that each parent folder of
        // a tracked file resolves as a path.
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.TrackEveryFile();
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(["ste-check", "--root", checkout.Root], output, errors);

        Assert.Equal(0, exitCode);
        Assert.Contains("0 finding(s)", output.ToString(), StringComparison.Ordinal);
        Assert.Empty(errors.ToString());
    }

    [Fact]
    public void AStagedNewDocumentTakesTheRules()
    {
        // The pre-commit hook reads the index, and a staged new file is in the index.
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.TrackEveryFile();
        checkout.Write("docs/staged-note.md", FailingLine);
        checkout.Track("docs/staged-note.md");
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(["ste-check", "--root", checkout.Root], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("docs/staged-note.md:1: rule STE 8.1", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ACitationOfAnUntrackedFileResolvesNoPath()
    {
        // CI holds the tracked files alone, so a local run must refuse the same citation (REF 2).
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write("docs/citing-note.md", "The note cites `docs/untracked-note.md` as a file of the repository.");
        checkout.TrackEveryFile();
        checkout.Write("docs/untracked-note.md", "The note that git does not track.");
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(["ste-check", "--root", checkout.Root], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("docs/citing-note.md:1: rule REF 2", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ATrackedFileThatTheWorkingTreeRemovedIsNoDocument()
    {
        // The index holds the file until the commit. A read of it then fails with no rule context.
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write("docs/removed-note.md", FailingLine);
        checkout.TrackEveryFile();
        File.Delete(Path.Combine(checkout.Root, "docs", "removed-note.md"));

        DocumentSet documents = DocumentSet.Read(checkout.Root);

        Assert.False(documents.Holds("docs/removed-note.md"));
        Assert.DoesNotContain("docs/removed-note.md", documents.LiveDocuments);
    }

    [Fact]
    public void TheFileSetHoldsEachParentFolderOneTime()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.TrackEveryFile();

        DocumentSet documents = DocumentSet.Read(checkout.Root);

        Assert.True(documents.Holds(".claude"));
        Assert.True(documents.Holds(".claude/skills"));
        Assert.True(documents.Holds(".claude/skills/ste-writing"));
        Assert.True(documents.Holds(".claude/skills/ste-writing/SKILL.md"));
        Assert.False(documents.Holds(".git"));
        Assert.Equal(documents.AllPaths.Count, new HashSet<string>(documents.AllPaths, StringComparer.Ordinal).Count);
    }

    [Fact]
    public void TheTrackedSetAndTheFolderTreeGiveTheSameFileSetForOneCheckout()
    {
        // With every file in the index, the two reads differ in nothing, and the order is fixed.
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        DocumentSet fromFolders = DocumentSet.Read(checkout.Root);
        checkout.TrackEveryFile();

        DocumentSet fromGit = DocumentSet.Read(checkout.Root);

        Assert.Equal(fromFolders.AllPaths, fromGit.AllPaths);
        Assert.Equal(fromFolders.Documents, fromGit.Documents);
        Assert.Equal(fromFolders.LiveDocuments, fromGit.LiveDocuments);
    }

    [Fact]
    public void AGitRunThatFailsNamesTheRootAndTheExitCode()
    {
        // No silent read of the folder tree replaces the run (T-2). A `.git` file that names
        // no folder makes `git ls-files` stop with the exit code 128.
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write(".git", "this text names no git folder");
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(["ste-check", "--root", checkout.Root], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("exit code 128", errors.ToString(), StringComparison.Ordinal);
        Assert.Contains(checkout.Root, errors.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("finding(s)", output.ToString(), StringComparison.Ordinal);
    }
}
