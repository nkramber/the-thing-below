using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TheThingBelow.Tools.SteCheck;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The reference check of D-605. It reads an id that no register holds, a path of this
/// repository with no file, and a superseded decision that names no successor.
/// </summary>
public sealed class SteCheckReferenceRuleTests
{
    [Fact]
    public void AnIdThatNoRegisterHoldsIsAFinding()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write("docs/note.md", "The note applies D-2 and OQ-9.");

        IReadOnlyList<Finding> findings = Check(checkout, "docs/note.md");

        Assert.Contains(findings, found => found.Rule == "REF 1" && found.Detail.Contains("OQ-9"));
        Assert.DoesNotContain(findings, found => found.Detail.Contains("D-2"));
    }

    [Fact]
    public void EveryRegisterOfTheProjectDefinesItsPrefix()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write("docs/note.md", "The note applies D-2, OQ-1, F-1, G-1, T-1, L-1, M-1, and PR-2.");

        IReadOnlyList<Finding> findings = Check(checkout, "docs/note.md");

        Assert.Empty(findings);
    }

    [Fact]
    public void ARetiredPullRequestKeepsItsIdInTheRegister()
    {
        // A retired PR keeps its entry in a phase file and leaves the order of section 8 (G-10).
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write("docs/note.md", "The note names PR-3, which is retired.");

        IReadOnlyList<Finding> findings = Check(checkout, "docs/note.md");

        Assert.Empty(findings);
    }

    [Fact]
    public void APathOfThisRepositoryWithNoFileIsAFinding()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write("docs/note.md", "The note names `docs/absent.md` and `docs/design.md`.");

        IReadOnlyList<Finding> findings = Check(checkout, "docs/note.md");

        Finding found = Assert.Single(findings);
        Assert.Equal("REF 2", found.Rule);
        Assert.Contains("docs/absent.md", found.Detail);
    }

    [Fact]
    public void ALineThatNamesThePullRequestOfAFileMarksThatPath()
    {
        // G-16: a document names the PR that creates a file that no commit holds yet.
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write("docs/note.md", "PR-2 creates `docs/absent.md`.");

        IReadOnlyList<Finding> findings = Check(checkout, "docs/note.md");

        Assert.Empty(findings);
    }

    [Fact]
    public void APathOutsideThisRepositoryTakesNoRule()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write(
            "docs/note.md",
            "The source is `scene/2d/camera_2d.cpp` of `godotengine/godot`, and the branch is `spike/deck-test`.");

        IReadOnlyList<Finding> findings = Check(checkout, "docs/note.md");

        Assert.Empty(findings);
    }

    [Fact]
    public void ADocumentCitesASiblingFileByItsNameAlone()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write("docs/roadmaps/area-tools.md", "This file cites `phase-1-foundations.md`.");

        IReadOnlyList<Finding> findings = Check(checkout, "docs/roadmaps/area-tools.md");

        Assert.Empty(findings);
    }

    [Fact]
    public void ASupersededDecisionCitedAloneIsAFinding()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write("docs/note.md", "The note applies D-1.");

        IReadOnlyList<Finding> findings = Check(checkout, "docs/note.md");

        Finding found = Assert.Single(findings);
        Assert.Equal("REF 3", found.Rule);
        Assert.Contains("D-2", found.Detail);
    }

    /// <summary>
    /// The path rule reads a path whose first part is a top-level folder of the repository, so
    /// the list holds every project folder of the solution and every other folder that a
    /// document cites (D-605).
    /// </summary>
    [Fact]
    public void EveryProjectFolderOfTheSolutionIsARepositoryRoot()
    {
        string solution = File.ReadAllText(RepositoryRoot.PathTo("TheThingBelow.slnx"));
        MatchCollection projects = Regex.Matches(solution, "Path=\"([^/\"]+)/");

        Assert.NotEmpty(projects);
        foreach (Match project in projects)
        {
            Assert.Contains(project.Groups[1].Value, ReferenceRules.RepositoryRoots);
        }

        Assert.Contains(".githooks", ReferenceRules.RepositoryRoots);
        Assert.Contains("licenses", ReferenceRules.RepositoryRoots);
    }

    /// <summary>The register also writes "D-N supersedes D-M" on the row of D-M (D-606).</summary>
    [Fact]
    public void ASupersededDecisionWithTheSuccessorOfTheSupersedesFormPasses()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Append(
            "docs/decisions.md",
            "| D-3 | 2026-09-14 | A third answer | The third answer. | Superseded by D-4 on 2026-09-15. D-5 supersedes D-3 on 2026-09-16. |",
            "| D-4 | 2026-09-15 | A fourth answer | The fourth answer. | Stands. |",
            "| D-5 | 2026-09-16 | A fifth answer | The fifth answer. | Stands. |");
        checkout.Write("docs/note.md", "The note applies D-3, which D-5 superseded.");

        IReadOnlyList<Finding> findings = Check(checkout, "docs/note.md");

        Assert.Empty(findings);
    }

    [Fact]
    public void ASupersededDecisionWithItsSuccessorPasses()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write("docs/note.md", "The note applies D-1, which D-2 superseded.");

        IReadOnlyList<Finding> findings = Check(checkout, "docs/note.md");

        Assert.Empty(findings);
    }

    [Fact]
    public void TheDecisionRegisterTakesNoSupersededCitationRule()
    {
        // D-606: the Effect column of the register records each supersession.
        using SteCheckCheckout checkout = SteCheckCheckout.Build();

        IReadOnlyList<Finding> findings = Check(checkout, ReferenceRules.DecisionRegisterPath);

        Assert.Empty(findings);
    }

    [Fact]
    public void AFencedBlockTakesNoReferenceRule()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write(
            "docs/note.md",
            "The command:",
            string.Empty,
            "```",
            "run --id OQ-9 --path docs/absent.md --decision D-1",
            "```");

        IReadOnlyList<Finding> findings = Check(checkout, "docs/note.md");

        Assert.Empty(findings);
    }

    private static IReadOnlyList<Finding> Check(SteCheckCheckout checkout, string document)
    {
        DocumentSet documents = DocumentSet.Read(checkout.Root);
        return ReferenceRules.Check(document, documents, IdRegister.Read(documents));
    }
}
