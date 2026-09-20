using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Tools;
using TheThingBelow.Tools.ReviewGate;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The exit tests of the `review-gate` command. GitHub starts `pull_request_target` from `main`
/// alone, so the live check cannot run on the pull request that creates it. These fixtures are
/// the proof (F-37, D-500).
/// </summary>
public sealed class ReviewGateCommandTests
{
    [Fact]
    public void APullRequestWithAnApprovedRecordForItsHeadPasses()
    {
        using ReviewGateFixture fixture = ReviewGateFixture.Build();

        (int exitCode, string report) = Run(fixture);

        Assert.Equal(0, exitCode);
        Assert.Contains("review-gate: pass", report, StringComparison.Ordinal);
        Assert.Contains("RG 5 pass", report, StringComparison.Ordinal);
    }

    [Fact]
    public void ARecordThatNamesAnOlderHeadFails()
    {
        using ReviewGateFixture fixture = ReviewGateFixture.Build();
        fixture.WriteFacts(ReviewGateFixture.PassingFacts() with
        {
            Commits =
            [
                new CommitFacts(ReviewGateFixture.OlderSha, ["TheThingBelow.Core/Rules.cs"]),
                new CommitFacts(ReviewGateFixture.HeadSha, ["TheThingBelow.Core/Rules.cs"]),
            ],
        });
        fixture.WriteRecord(ReviewGateFixture.Record(ReviewGateFixture.OlderSha, "Ready for owner merge"));

        (int exitCode, string report) = Run(fixture);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("RG 5 fault", report, StringComparison.Ordinal);
        Assert.Contains(ReviewGateFixture.HeadSha, report, StringComparison.Ordinal);
    }

    [Fact]
    public void ARecordWithNoApprovedVerdictFails()
    {
        using ReviewGateFixture fixture = ReviewGateFixture.Build();
        fixture.WriteRecord(ReviewGateFixture.Record(ReviewGateFixture.HeadSha, "Changes required"));

        (int exitCode, string report) = Run(fixture);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("RG 4 fault", report, StringComparison.Ordinal);
    }

    [Fact]
    public void APullRequestWithNoRecordFails()
    {
        using ReviewGateFixture fixture = ReviewGateFixture.Build();
        fixture.DeleteRecord();

        (int exitCode, string report) = Run(fixture);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("RG 3 fault", report, StringComparison.Ordinal);
        Assert.Contains("RG 4 skip", report, StringComparison.Ordinal);
    }

    [Fact]
    public void ADocumentationPullRequestWithTheLabelAndNoDecisionRowPasses()
    {
        using ReviewGateFixture fixture = ReviewGateFixture.Build();
        fixture.WriteFacts(ReviewGateFixture.LabeledFacts());
        fixture.DeleteRecord();

        (int exitCode, string report) = Run(fixture);

        Assert.Equal(0, exitCode);
        Assert.Contains("RG 3 skip", report, StringComparison.Ordinal);
        Assert.Contains("review-gate: pass", report, StringComparison.Ordinal);
    }

    [Fact]
    public void ALabeledPullRequestThatChangesADecisionRowFails()
    {
        using ReviewGateFixture fixture = ReviewGateFixture.Build();
        fixture.WriteFacts(ReviewGateFixture.LabeledFacts() with
        {
            Files = ["docs/decisions.md", "docs/session-handoff.md"],
            DecisionsDiff = string.Join(
                '\n',
                "--- a/docs/decisions.md",
                "+++ b/docs/decisions.md",
                "@@ -10,3 +10,4 @@",
                " | D-1 | 2026-09-12 | The first answer | The answer. | The effect. |",
                "+| D-2 | 2026-09-17 | The next answer | The answer. | The effect. |"),
        });

        (int exitCode, string report) = Run(fixture);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("RG 2 fault", report, StringComparison.Ordinal);
        Assert.Contains("D-401", report, StringComparison.Ordinal);
    }

    [Fact]
    public void ALabeledPullRequestOutsideTheOverrideSetFails()
    {
        using ReviewGateFixture fixture = ReviewGateFixture.Build();
        fixture.WriteFacts(ReviewGateFixture.LabeledFacts() with
        {
            Files = ["Makefile", "docs/session-handoff.md"],
        });

        (int exitCode, string report) = Run(fixture);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("RG 1 fault", report, StringComparison.Ordinal);
        Assert.Contains("Makefile", report, StringComparison.Ordinal);
    }

    [Fact]
    public void ALabeledPullRequestThatChangesAWorkflowFails()
    {
        using ReviewGateFixture fixture = ReviewGateFixture.Build();
        fixture.WriteFacts(ReviewGateFixture.LabeledFacts() with
        {
            Files = [".github/workflows/ci.yml", "docs/session-handoff.md"],
        });

        (int exitCode, string report) = Run(fixture);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("RG 1 fault", report, StringComparison.Ordinal);
        Assert.Contains("D-560", report, StringComparison.Ordinal);
    }

    [Fact]
    public void APullRequestThatChangesNoHandoffFails()
    {
        using ReviewGateFixture fixture = ReviewGateFixture.Build();
        fixture.WriteFacts(ReviewGateFixture.PassingFacts() with
        {
            Files = ["TheThingBelow.Core/Rules.cs", "docs/reviews/pr-21.md"],
        });

        (int exitCode, string report) = Run(fixture);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("RG 6 fault", report, StringComparison.Ordinal);
    }

    [Fact]
    public void ADocumentsSectionWithNoLineForARowFails()
    {
        using ReviewGateFixture fixture = ReviewGateFixture.Build();
        fixture.WriteFacts(ReviewGateFixture.PassingFacts() with
        {
            Body = ReviewGateFixture.Body(new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["docs/world/"] = null,
            }),
        });

        (int exitCode, string report) = Run(fixture);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("RG 7 fault", report, StringComparison.Ordinal);
        Assert.Contains("docs/world/", report, StringComparison.Ordinal);
    }

    [Fact]
    public void ADocumentsLineInNoFormOfDecision581Fails()
    {
        using ReviewGateFixture fixture = ReviewGateFixture.Build();
        fixture.WriteFacts(ReviewGateFixture.PassingFacts() with
        {
            Body = ReviewGateFixture.Body(new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["docs/runbooks/"] = "no documentation impact",
            }),
        });

        (int exitCode, string report) = Run(fixture);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("RG 7 fault", report, StringComparison.Ordinal);
        Assert.Contains("holds no form of D-581", report, StringComparison.Ordinal);
    }

    [Fact]
    public void ADocumentsLineThatDefersADocumentOfThisPullRequestFails()
    {
        using ReviewGateFixture fixture = ReviewGateFixture.Build();
        fixture.WriteFacts(ReviewGateFixture.PassingFacts() with
        {
            Body = ReviewGateFixture.Body(new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["docs/design.md"] = "Changed: `docs/design.md`. A later PR adds the section.",
            }),
        });

        (int exitCode, string report) = Run(fixture);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("RG 8 fault", report, StringComparison.Ordinal);
        Assert.Contains("later pr", report, StringComparison.Ordinal);
    }

    [Fact]
    public void ADocumentsLineThatNamesThePullRequestOfAnAbsentCheckPasses()
    {
        using ReviewGateFixture fixture = ReviewGateFixture.Build();
        fixture.WriteFacts(ReviewGateFixture.PassingFacts() with
        {
            Body = ReviewGateFixture.Body(new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["docs/roadmaps/"] = "No change needed because `docs/roadmaps/area-ci.md` names PR-46 for the det-lint job (G-16).",
            }),
        });

        (int exitCode, string report) = Run(fixture);

        Assert.Equal(0, exitCode);
        Assert.Contains("RG 8 pass", report, StringComparison.Ordinal);
    }

    [Fact]
    public void AHandoffLineInAnotherFormThanChangedFails()
    {
        using ReviewGateFixture fixture = ReviewGateFixture.Build();
        fixture.WriteFacts(ReviewGateFixture.PassingFacts() with
        {
            Body = ReviewGateFixture.Body(new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["docs/session-handoff.md"] = "No change needed because `docs/session-handoff.md` holds the entry of Session 1.",
            }),
        });

        (int exitCode, string report) = Run(fixture);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("RG 7 fault", report, StringComparison.Ordinal);
        Assert.Contains("D-18", report, StringComparison.Ordinal);
    }

    [Fact]
    public void ADescriptionWithNoDocumentsSectionFails()
    {
        using ReviewGateFixture fixture = ReviewGateFixture.Build();
        fixture.WriteFacts(ReviewGateFixture.PassingFacts() with { Body = "## Summary\n\nThe one concern.\n" });

        (int exitCode, string report) = Run(fixture);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("RG 7 fault", report, StringComparison.Ordinal);
        Assert.Contains("RG 8 skip", report, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAbsentFieldOfTheFactsFileNamesTheField()
    {
        using ReviewGateFixture fixture = ReviewGateFixture.Build();
        File.WriteAllText(fixture.FactsPath, "{ \"number\": 21 }");
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(
            ["review-gate", "--pull-request", fixture.FactsPath, "--head-files", fixture.HeadFiles],
            output,
            errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("holds no field 'commits'", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnknownOptionNamesTheOption()
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(["review-gate", "--every-pull-request", "."], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("'--every-pull-request' is unknown", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ARunWithNoHeadFilesOptionNamesBothOptions()
    {
        using ReviewGateFixture fixture = ReviewGateFixture.Build();
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(["review-gate", "--pull-request", fixture.FactsPath], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("--head-files", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void AHeadFilesFolderThatDoesNotExistNamesTheFolder()
    {
        using ReviewGateFixture fixture = ReviewGateFixture.Build();
        string absent = Path.Combine(fixture.Root, "no-such-folder");
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(
            ["review-gate", "--pull-request", fixture.FactsPath, "--head-files", absent],
            output,
            errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("does not exist", errors.ToString(), StringComparison.Ordinal);
    }

    private static (int ExitCode, string Report) Run(ReviewGateFixture fixture)
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();
        int exitCode = Program.Run(
            ["review-gate", "--pull-request", fixture.FactsPath, "--head-files", fixture.HeadFiles],
            output,
            errors);
        return (exitCode, output.ToString() + errors.ToString());
    }
}
