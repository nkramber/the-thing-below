using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using TheThingBelow.Tools;
using TheThingBelow.Tools.Bots;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

public sealed class BotsCommandTests : IDisposable
{
    private const string Leg = "test-leg";

    private static readonly Lazy<ContentSet> Content =
        new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    private readonly string folder = Path.Combine(Path.GetTempPath(), "bots-command-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void AGreenPlayWritesTheResultsAndTheSummary()
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = BotsCommand.Play(Content.Value, this.Plan(BotPolicyKind.Greedy, 4, 1), AcceptedSources.OfCore, BotsCommand.PolicyOf, output, errors);

        Assert.Equal(0, exitCode);
        Assert.Equal(string.Empty, errors.ToString());
        Assert.Equal(4, File.ReadAllLines(Path.Combine(this.folder, "results-greedy.txt")).Length);
        Assert.Contains("- Runs: 4, in ", output.ToString(), StringComparison.Ordinal);
        // Seeds 1 to 4 start on the fixture hub, the village, the pasture, and the town (D-1185).
        // The hub run reaches the goal, and each run of region one ends as budget, because no way
        // leads from region one to the fixture hub (PR-17).
        Assert.Contains("budget 3, complete 1", output.ToString(), StringComparison.Ordinal);
        Assert.False(Directory.Exists(Path.Combine(this.folder, "saves", "greedy-1")), "The folder of the saves of a run stays after the run.");
    }

    [Fact]
    public void APlantedSoftlockFailsTheJobWithItsSeedItsPolicyAndItsLeg()
    {
        // PR-15 exit test 2 (D-1179).
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = BotsCommand.Play(Content.Value, this.Plan(BotPolicyKind.Random, 1, 7), NoIntent, BotsCommand.PolicyOf, output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"the random bot run of seed 7 on the leg {Leg} ended as softlock", errors.ToString(), StringComparison.Ordinal);
        Assert.True(File.Exists(this.RecordPath("random-7")), "The job wrote no record of the failed run.");
    }

    [Fact]
    public void APlantedCrashFailsTheJobTheSameWay()
    {
        // PR-15 exit test 3: a policy that closes a closed menu makes the rules refuse the intent.
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = BotsCommand.Play(Content.Value, this.Plan(BotPolicyKind.Greedy, 1, 9), AcceptedSources.OfCore, (_, _) => new CrashPolicy(), output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"the greedy bot run of seed 9 on the leg {Leg} ended as crash at tick {CrashPolicy.CrashTick}", errors.ToString(), StringComparison.Ordinal);
        Assert.Contains("SimulationException", errors.ToString(), StringComparison.Ordinal);
        Assert.True(File.Exists(this.RecordPath("greedy-9")), "The job wrote no record of the failed run.");
    }

    [Fact]
    public void TheReplayOfTheRecordOfACrashRepeatsTheCrash()
    {
        // PR-15 exit test 4 (T-7).
        using StringWriter output = new();
        using StringWriter errors = new();
        BotsCommand.Play(Content.Value, this.Plan(BotPolicyKind.Greedy, 1, 9), AcceptedSources.OfCore, (_, _) => new CrashPolicy(), output, errors);
        using StringWriter replayOutput = new();
        using StringWriter replayErrors = new();

        int exitCode = BotsCommand.Replay(Content.Value, this.RecordPath("greedy-9"), AcceptedSources.OfCore, replayOutput, replayErrors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("repeats the crash", replayErrors.ToString(), StringComparison.Ordinal);
        Assert.Contains("intent.close_menu", replayErrors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheReplayOfTheRecordOfASoftlockRepeatsTheSoftlock()
    {
        // PR-15 exit test 4 (T-7, D-1179): the replay reads the same source as the run.
        using StringWriter output = new();
        using StringWriter errors = new();
        BotsCommand.Play(Content.Value, this.Plan(BotPolicyKind.Random, 1, 7), NoIntent, BotsCommand.PolicyOf, output, errors);
        using StringWriter replayOutput = new();
        using StringWriter replayErrors = new();

        int exitCode = BotsCommand.Replay(Content.Value, this.RecordPath("random-7"), NoIntent, replayOutput, replayErrors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("repeats the softlock", replayErrors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheCommandLineRefusesAnAbsentPolicy()
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = Program.Run([BotsCommand.Name, "--root", RepositoryRoot.Find(), "--runs", "1", "--out", this.folder], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("needs --policy with the value random or greedy", errors.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-3")]
    [InlineData("many")]
    public void TheCommandLineRefusesACountBelowOne(string runs)
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = Program.Run([BotsCommand.Name, "--root", RepositoryRoot.Find(), "--policy", "random", "--runs", runs, "--out", this.folder], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("needs --runs with a whole number of 1 or more", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheCommandLineRefusesAnAbsentOutputFolder()
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = Program.Run([BotsCommand.Name, "--root", RepositoryRoot.Find(), "--policy", "random", "--runs", "1"], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("needs --out with the output folder", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheCommandLinePlaysTheRuns()
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = Program.Run([BotsCommand.Name, "--root", RepositoryRoot.Find(), "--policy", "random", "--runs", "2", "--first-seed", "4", "--leg", Leg, "--out", this.folder], output, errors);

        Assert.True(exitCode == 0, errors.ToString());
        Assert.Contains($"### Bot runs: random on {Leg}", output.ToString(), StringComparison.Ordinal);
        Assert.StartsWith("random seed=4 ", File.ReadAllLines(Path.Combine(this.folder, "results-random.txt"))[0], StringComparison.Ordinal);
    }

    [Fact]
    public void ASoftlockThatClearsOnTheNextTickFailsTheJobAtItsTick()
    {
        // P2-1 of the review of PR #87: the runner checks each state, so a softlock at a tick
        // that no sample reads, and that clears on the next tick, still ends the run there. The
        // replay reads the same source and repeats the softlock.
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = BotsCommand.Play(Content.Value, this.Plan(BotPolicyKind.Greedy, 1, 5), NoIntentAtTick7, BotsCommand.PolicyOf, output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"the greedy bot run of seed 5 on the leg {Leg} ended as softlock at tick 7", errors.ToString(), StringComparison.Ordinal);
        using StringWriter replayOutput = new();
        using StringWriter replayErrors = new();
        Assert.Equal(Program.FaultExitCode, BotsCommand.Replay(Content.Value, this.RecordPath("greedy-5"), NoIntentAtTick7, replayOutput, replayErrors));
        Assert.Contains("repeats the softlock at tick 7", replayErrors.ToString(), StringComparison.Ordinal);
    }

    public void Dispose()
    {
        if (Directory.Exists(this.folder))
        {
            Directory.Delete(this.folder, recursive: true);
        }
    }

    private static IReadOnlyList<Intent> NoIntent(Simulation simulation) => [];

    /// <summary>A planted source: no intent at tick 7 alone, and the query of Core at each other tick.</summary>
    private static IReadOnlyList<Intent> NoIntentAtTick7(Simulation simulation) => simulation.Tick == 7 ? [] : simulation.Accepted();

    private BotPlan Plan(BotPolicyKind policy, int runs, ulong firstSeed) => new(policy, runs, firstSeed, this.folder, Leg);

    private string RecordPath(string name) => Path.Combine(this.folder, BotsCommand.RecordsFolder, name + ".record");

    /// <summary>A planted policy: the greedy policy, then a close of the closed menu at one tick, which the rules refuse (T-2).</summary>
    private sealed class CrashPolicy : IBotPolicy
    {
        public const long CrashTick = 30;

        private readonly GreedyPolicy greedy = new();

        public BotPolicyKind Kind => BotPolicyKind.Greedy;

        public Intent? Choose(RunState state, IReadOnlyList<Intent> accepted) =>
            state.Tick + 1 == CrashTick && !state.MenuOpen ? Intent.OfPlayer(IntentIds.CloseMenu) : this.greedy.Choose(state, accepted);
    }
}
