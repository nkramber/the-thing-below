using System.Collections.Generic;
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
