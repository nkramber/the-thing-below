using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using TheThingBelow.Tools;
using TheThingBelow.Tools.Bots;
using TheThingBelow.Tools.Content;
using TheThingBelow.Tools.Night;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The `night` command: one leg of the night job, and its night record (D-507, D-509, D-1190, D-1191).</summary>
public sealed class NightCommandTests : IDisposable
{
    private const string Leg = "ubuntu-24.04";

    private static readonly Lazy<ContentSet> Content =
        new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    private readonly string folder = Path.Combine(Path.GetTempPath(), "night-command-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void AGreenNightWritesTheRecordOfItsLeg()
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = NightCommand.Play(Content.Value, this.Plan(2, 1), AcceptedSources.OfCore, BotsCommand.PolicyOf, output, errors);

        Assert.True(exitCode == 0, errors.ToString());
        NightRecord record = NightRecord.Read(this.RecordPath());
        Assert.Equal(NightGateFixture.MainCommit, record.Commit);
        Assert.Equal(Leg, record.Leg);
        Assert.Equal(NightGateFixture.FirstSeed, record.FirstSeed);
        Assert.Equal([BotPolicyKind.Greedy, BotPolicyKind.Random], [record.Policies[0].Policy, record.Policies[1].Policy]);
        Assert.Equal([2L, 1L], [record.Policies[0].Runs, record.Policies[1].Runs]);
        Assert.Equal(NightRecord.Success, record.Status);
        Assert.Contains($"### Bot runs: greedy on {Leg}", output.ToString(), StringComparison.Ordinal);
        Assert.Contains($"### Bot runs: random on {Leg}", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("with the status success.", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void EachPolicyPlaysFromTheFirstSeedOfTheNight()
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        NightCommand.Play(Content.Value, this.Plan(2, 1), AcceptedSources.OfCore, BotsCommand.PolicyOf, output, errors);

        Assert.StartsWith($"greedy seed={NightGateFixture.FirstSeed} ", File.ReadAllLines(Path.Combine(this.folder, "results-greedy.txt"))[0], StringComparison.Ordinal);
        Assert.StartsWith($"random seed={NightGateFixture.FirstSeed} ", File.ReadAllLines(Path.Combine(this.folder, "results-random.txt"))[0], StringComparison.Ordinal);
    }

    [Fact]
    public void ASoftlockFailsTheNightAndTheRecordKeepsTheCounts()
    {
        // The random runs play after the greedy runs failed, so one night reports both.
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = NightCommand.Play(Content.Value, this.Plan(1, 2), NoIntent, BotsCommand.PolicyOf, output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        NightRecord record = NightRecord.Read(this.RecordPath());
        Assert.Equal(NightRecord.Failure, record.Status);
        Assert.Equal([1L, 2L], [record.Policies[0].Softlock, record.Policies[1].Softlock]);
        Assert.Contains($"the night on the leg {Leg} at the commit {NightGateFixture.MainCommit} has a softlock or a crash", errors.ToString(), StringComparison.Ordinal);
        Assert.Contains($"the random bot run of seed {NightGateFixture.FirstSeed + 1} on the leg {Leg} ended as softlock", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheCommandLinePlaysTheLeg()
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = Program.Run(this.Arguments("1", "1", Leg, NightGateFixture.MainCommit), output, errors);

        Assert.True(exitCode == 0, errors.ToString());
        Assert.Equal(NightRecord.Success, NightRecord.Read(this.RecordPath()).Status);
    }

    [Fact]
    public void TheCommandLineRefusesALegOutsideTheNight()
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = Program.Run(this.Arguments("1", "1", "ubuntu-22.04", NightGateFixture.MainCommit), output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("needs --leg with the label of a leg of the night: macos-26, ubuntu-24.04, windows-2025.", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheCommandLineRefusesAShortCommit()
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = Program.Run(this.Arguments("1", "1", Leg, "d4e5f60"), output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("needs --commit with the full hash of the commit, and it read 'd4e5f60'.", errors.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("0", "1", "--greedy-runs")]
    [InlineData("1", "many", "--random-runs")]
    public void TheCommandLineRefusesACountBelowOne(string greedyRuns, string randomRuns, string option)
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = Program.Run(this.Arguments(greedyRuns, randomRuns, Leg, NightGateFixture.MainCommit), output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"needs {option} with a whole number of 1 or more (D-1191).", errors.ToString(), StringComparison.Ordinal);
    }

    public void Dispose()
    {
        if (Directory.Exists(this.folder))
        {
            Directory.Delete(this.folder, recursive: true);
        }
    }

    private static IReadOnlyList<Intent> NoIntent(Simulation simulation) => [];

    private NightPlan Plan(int greedyRuns, int randomRuns) =>
        new(greedyRuns, randomRuns, NightGateFixture.FirstSeed, Leg, NightGateFixture.MainCommit, this.folder);

    private string RecordPath() => Path.Combine(this.folder, NightLegs.FileOf(Leg));

    private string[] Arguments(string greedyRuns, string randomRuns, string leg, string commit) =>
    [
        NightCommand.Name,
        "--root", RepositoryRoot.Find(),
        "--greedy-runs", greedyRuns,
        "--random-runs", randomRuns,
        "--first-seed", NightGateFixture.FirstSeed.ToString(System.Globalization.CultureInfo.InvariantCulture),
        "--leg", leg,
        "--commit", commit,
        "--out", this.folder,
    ];
}
