using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Tools;
using TheThingBelow.Tools.Bots;
using TheThingBelow.Tools.Night;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The promotion of a branch night at the merge, and the `night-promote` command (D-1202, D-1203).</summary>
public sealed class NightPromoteTests : IDisposable
{
    private const string NightCommit = "b0b1b2b3b4b5b6b7b8b9c0c1c2c3c4c5c6c7c8c9";

    private const string CodeCommit = "c0c1c2c3c4c5c6c7c8c9d0d1d2d3d4d5d6d7d8d9";

    private const string Merge = "e0e1e2e3e4e5e6e7e8e9f0f1f2f3f4f5f6f7f8f9";

    private readonly string root = Path.Combine(Path.GetTempPath(), "night-promote-" + Guid.NewGuid().ToString("N"));

    public NightPromoteTests()
    {
        foreach (string folder in new[] { this.MainNight, this.Promotion, this.BranchNight })
        {
            Directory.CreateDirectory(folder);
        }

        // The PR: a code commit with the night, then the review record and the handoff (D-1204).
        NightGateFixture.WriteCommitsFile(this.CommitsPath, new NightCommit(NightCommit, 1, ["TheThingBelow.Core/Simulation.cs"]), new NightCommit(NightGateFixture.Head, 1, ["docs/reviews/pr-99.md"]));
        File.WriteAllLines(this.TreePath, ["docs/reviews/pr-99.md", "docs/session-handoff.md"]);
    }

    private string MainNight => Path.Combine(this.root, "main-night");

    private string Promotion => Path.Combine(this.root, "promotion");

    private string BranchNight => Path.Combine(this.root, "branch-night");

    private string CommitsPath => Path.Combine(this.root, "commits.json");

    private string TreePath => Path.Combine(this.root, "tree-difference.txt");

    private string Out => Path.Combine(this.root, "out");

    public void Dispose()
    {
        Directory.Delete(this.root, recursive: true);
    }

    [Fact]
    public void AGreenBranchNightOnTheFailedRangePromotes()
    {
        this.WriteFailedMainNight();
        NightGateFixture.WriteGreenNight(this.BranchNight, NightCommit, "fix/pr-99-night", NightGateFixture.Now - TimeSpan.FromHours(3));

        int exitCode = this.Run(out string output, out _);

        Assert.Equal(0, exitCode);
        Assert.Contains($"promotes at the merge commit {Merge} over the failed night run 102 (D-1202, D-1203)", output, StringComparison.Ordinal);
        NightPromotion promotion = NightPromotion.Read(this.Out)!;
        Assert.Equal(Merge, promotion.MergeCommit);
        Assert.Equal(99, promotion.PullRequest);
        Assert.Equal(102, promotion.FailedRun);
        Assert.Equal(NightCommit, promotion.Night.Run.Commit);
        Assert.Equal(NightLegs.Labels.Count, promotion.Night.Records.Count);
    }

    [Fact]
    public void AGreenNightOfMainNeedsNoPromotion()
    {
        NightGateFixture.WriteGreenNight(this.MainNight, NightGateFixture.MainCommit, NightGate.MainBranch, NightGateFixture.LastNight);
        NightGateFixture.WriteGreenNight(this.BranchNight, NightCommit, "fix/pr-99-night", NightGateFixture.Now - TimeSpan.FromHours(3));

        this.AssertNoPromotion("and it succeeded, so nothing needs a promotion");
    }

    [Fact]
    public void ANewerPromotionNeedsNoOther()
    {
        this.WriteFailedMainNight();
        NightGateFixture.WritePromotion(this.Promotion, "f0f1f2f3f4f5f6f7f8f9a0a1a2a3a4a5a6a7a8a9", NightCommit, NightGateFixture.Now - TimeSpan.FromHours(5));
        NightGateFixture.WriteGreenNight(this.BranchNight, NightCommit, "fix/pr-99-night", NightGateFixture.Now - TimeSpan.FromHours(3));

        this.AssertNoPromotion("is the promotion of the PR #99", "--promotion-order", "ahead");
    }

    [Fact]
    public void APushWithNoPrPromotesNothing()
    {
        this.WriteFailedMainNight();

        using StringWriter output = new();
        using StringWriter errors = new();
        int exitCode = Program.Run(
            [NightPromoteCommand.Name, "--merge-commit", Merge, "--main-night", this.MainNight, "--promotion", this.Promotion, "--branch-night", this.BranchNight, "--now", NightJson.TextOf(NightGateFixture.Now), "--out", this.Out],
            output,
            errors);

        Assert.Equal(0, exitCode);
        Assert.Contains("holds no merged PR", output.ToString(), StringComparison.Ordinal);
        Assert.False(Directory.Exists(this.Out));
    }

    [Fact]
    public void APrWithNoNightPromotesNothing()
    {
        this.WriteFailedMainNight();

        this.AssertNoPromotion("has no completed night on its head");
    }

    [Fact]
    public void ANightOnAnotherRangeDoesNotPromote()
    {
        this.WriteFailedMainNight();
        NightGateFixture.WriteRun(this.BranchNight, 201, NightCommit, "fix/pr-99-night", NightGate.SuccessConclusion, NightGateFixture.Now - TimeSpan.FromHours(3));
        foreach (string leg in NightLegs.Labels)
        {
            NightGateFixture.WriteRecord(this.BranchNight, NightGateFixture.GreenRecord(NightCommit, leg) with { FirstSeed = 40 * NightLegs.SeedStep });
        }

        this.AssertNoPromotion("wrong range: the branch night played 30 greedy runs and 20 random runs from the seed 40000000000");
    }

    [Fact]
    public void ANightWithOtherCountsDoesNotPromote()
    {
        this.WriteFailedMainNight();
        NightGateFixture.WriteRun(this.BranchNight, 201, NightCommit, "fix/pr-99-night", NightGate.SuccessConclusion, NightGateFixture.Now - TimeSpan.FromHours(3));
        foreach (string leg in NightLegs.Labels)
        {
            NightRecord green = NightGateFixture.GreenRecord(NightCommit, leg);
            NightGateFixture.WriteRecord(this.BranchNight, green with { Policies = [green.Policies[0], new NightPolicyCounts(BotPolicyKind.Random, 10, 1, 0, 0, 9, 9)] });
        }

        this.AssertNoPromotion("wrong range:");
    }

    [Fact]
    public void ACodePathInTheTreeDifferenceDoesNotPromote()
    {
        this.WriteFailedMainNight();
        NightGateFixture.WriteGreenNight(this.BranchNight, NightCommit, "fix/pr-99-night", NightGateFixture.Now - TimeSpan.FromHours(3));
        File.WriteAllLines(this.TreePath, ["docs/design.md", "TheThingBelow.Core/Battle.cs"]);

        this.AssertNoPromotion("wrong tree: the tree of the merge commit");
    }

    [Fact]
    public void AStaleBranchNightDoesNotPromote()
    {
        this.WriteFailedMainNight();
        NightGateFixture.WriteGreenNight(this.BranchNight, NightCommit, "fix/pr-99-night", NightGateFixture.Now - TimeSpan.FromHours(49));

        this.AssertNoPromotion("stale: the night run 101");
    }

    [Fact]
    public void AFailedBranchNightDoesNotPromote()
    {
        this.WriteFailedMainNight();
        NightGateFixture.WriteGreenNight(this.BranchNight, NightCommit, "fix/pr-99-night", NightGateFixture.Now - TimeSpan.FromHours(3));
        NightGateFixture.WriteRecord(this.BranchNight, NightGateFixture.RedRecord(NightCommit, "windows-2025"));

        this.AssertNoPromotion("failed: the random runs of the leg windows-2025");
    }

    [Fact]
    public void ANightBeforeACodeCommitDoesNotPromote()
    {
        this.WriteFailedMainNight();
        NightGateFixture.WriteCommitsFile(this.CommitsPath, new NightCommit(NightCommit, 1, ["TheThingBelow.Core/Simulation.cs"]), new NightCommit(CodeCommit, 1, ["TheThingBelow.Core/Battle.cs"]));
        NightGateFixture.WriteGreenNight(this.BranchNight, NightCommit, "fix/pr-99-night", NightGateFixture.Now - TimeSpan.FromHours(3));

        this.AssertNoPromotion("wrong commit: a later commit of the PR changes a path");
    }

    [Fact]
    public void AFailedNightOnTheMergeCommitDoesNotPromote()
    {
        NightGateFixture.WriteRun(this.MainNight, 102, Merge, NightGate.MainBranch, "failure", NightGateFixture.LastNight);
        NightGateFixture.WriteRecord(this.MainNight, NightGateFixture.RedRecord(Merge, "ubuntu-24.04"));
        NightGateFixture.WriteGreenNight(this.BranchNight, NightCommit, "fix/pr-99-night", NightGateFixture.Now - TimeSpan.FromHours(3));

        this.AssertNoPromotion("wrong order: the failed night run 102 played the merge commit");
    }

    [Fact]
    public void AFailedNightWithNoRecordDoesNotPromote()
    {
        NightGateFixture.WriteRun(this.MainNight, 102, NightGateFixture.MainCommit, NightGate.MainBranch, "failure", NightGateFixture.LastNight);
        NightGateFixture.WriteGreenNight(this.BranchNight, NightCommit, "fix/pr-99-night", NightGateFixture.Now - TimeSpan.FromHours(3));

        this.AssertNoPromotion("holds no night record, so its range is unknown");
    }

    [Fact]
    public void AnOutFolderThatExistsStopsTheCommand()
    {
        Directory.CreateDirectory(this.Out);

        int exitCode = this.Run(out _, out string errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("the folder exists", errors, StringComparison.Ordinal);
    }

    [Fact]
    public void ABranchNightWithNoTreeDifferenceStopsTheCommand()
    {
        this.WriteFailedMainNight();
        NightGateFixture.WriteGreenNight(this.BranchNight, NightCommit, "fix/pr-99-night", NightGateFixture.Now - TimeSpan.FromHours(3));
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = Program.Run(
            [NightPromoteCommand.Name, "--merge-commit", Merge, "--pull-request", "99", "--commits", this.CommitsPath, "--main-night", this.MainNight, "--promotion", this.Promotion, "--branch-night", this.BranchNight, "--now", NightJson.TextOf(NightGateFixture.Now), "--out", this.Out],
            output,
            errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("--tree-difference", errors.ToString(), StringComparison.Ordinal);
    }

    private void WriteFailedMainNight()
    {
        NightGateFixture.WriteRun(this.MainNight, 102, NightGateFixture.MainCommit, NightGate.MainBranch, "failure", NightGateFixture.LastNight);
        foreach (string leg in NightLegs.Labels)
        {
            NightGateFixture.WriteRecord(this.MainNight, NightGateFixture.RedRecord(NightGateFixture.MainCommit, leg));
        }
    }

    private void AssertNoPromotion(string reason, params string[] extra)
    {
        int exitCode = this.Run(out string output, out string errors, extra);

        Assert.True(exitCode == 0, errors);
        Assert.Contains(reason, output, StringComparison.Ordinal);
        Assert.Contains("The push promotes nothing.", output, StringComparison.Ordinal);
        Assert.False(Directory.Exists(this.Out));
    }

    private int Run(out string output, out string errors, params string[] extra)
    {
        List<string> arguments =
        [
            NightPromoteCommand.Name, "--merge-commit", Merge, "--pull-request", "99", "--commits", this.CommitsPath,
            "--main-night", this.MainNight, "--promotion", this.Promotion, "--branch-night", this.BranchNight,
            "--tree-difference", this.TreePath, "--now", NightJson.TextOf(NightGateFixture.Now), "--out", this.Out,
        ];
        arguments.AddRange(extra);
        using StringWriter outputWriter = new();
        using StringWriter errorWriter = new();
        int exitCode = Program.Run([.. arguments], outputWriter, errorWriter);
        output = outputWriter.ToString();
        errors = errorWriter.ToString();
        return exitCode;
    }
}
