using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The commands of the debug console (D-171, D-724). Each test runs a command through the
/// entry of the debug assembly, which is the same contract that Game uses (D-723).
/// </summary>
/// <remarks>
/// The screen of the console needs a Godot session, so the smoke session of CI builds it and
/// runs every command inside the engine (D-117, F-23). These tests read the commands alone.
/// </remarks>
public sealed class DebugConsoleTests
{
    /// <summary>The permanent id of the reveal command (D-166, D-727).</summary>
    private const string RevealId = "debug.reveal_map";

    /// <summary>The seed of the runs of these tests.</summary>
    private const ulong Seed = 20260920;

    /// <summary>The content hash of the records of these tests (G-5).</summary>
    private const string ContentHash = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

    [Fact]
    public void TheHandlerSetHoldsOneHandlerForEachCommandThatSendsAnIntent()
    {
        // The host passes this set to the start of a run, and Core reads no other source of
        // a debug handler (D-260, D-492).
        DebugIntentHandlers handlers = DebugAssemblyFile.Handlers();

        Assert.Equal(1, handlers.Count);
        Assert.True(handlers.TryFind(Id(RevealId), out DebugIntentHandler? found));
        Assert.NotNull(found);
    }

    [Fact]
    public void TheRevealCommandSendsOneDebugIntentAndChangesNoStateItself()
    {
        // The command changes the run on a tick of the rules, and never outside one. A change
        // outside a tick would leave the record behind the state (D-171, T-7).
        Simulation run = Start();
        List<Intent> queued = [];

        IReadOnlyList<string> answer = DebugAssemblyFile.Run("reveal", () => run.State, queued.Add);

        Intent sent = Assert.Single(queued);
        Assert.Equal(RevealId, sent.Action.Value);
        Assert.True(sent.IsDebug);
        Assert.Equal(1, run.State.Party.Walked.Count);
        Assert.Contains(RevealId, string.Join(" ", answer), StringComparison.Ordinal);
    }

    [Fact]
    public void TheRevealIntentMarksEveryTileOfTheMap()
    {
        // Exit test 1 of section 7.4 of `phase-2-first-playable.md`, for the one command of
        // this build that changes the run (D-724).
        Simulation run = Start();
        GameMap map = run.State.Party.Map;

        run.Step([Intent.OfDebugConsole(Id(RevealId))]);

        Assert.Equal(map.Width * map.Height, run.State.Party.Walked.Count);
        Assert.True(run.State.Party.Walked.WasWalked(new TilePoint(0, 0)));
    }

    [Fact]
    public void ARecordOfTheRevealCommandReplaysOnADevelopmentHost()
    {
        // Exit test 3: a run that used a console command still replays, and the record holds
        // the intent with its mark (D-171, G-5).
        RunRecord record = RevealRecord();

        RunState replayed = RunReplay.Play(
            record,
            ContentHash,
            TestMaps.Room,
            DebugAssemblyFile.Handlers());

        Assert.Equal(TestMaps.Room.Width * TestMaps.Room.Height, replayed.Party.Walked.Count);
        Assert.True(record.Ticks[0].Intents[0].IsDebug);
    }

    [Fact]
    public void AReleaseHostRefusesARecordOfTheRevealCommandWithTheTickAndTheIntent()
    {
        // Exit test 4: a host with no debug handler refuses the record, and the report names
        // the intent and the tick (D-260, D-492, T-2).
        RunRecord record = RevealRecord();

        SimulationException error = Assert.Throws<SimulationException>(
            () => RunReplay.Play(record, ContentHash, TestMaps.Room, DebugIntentHandlers.None));

        Assert.Contains(RevealId, error.Message, StringComparison.Ordinal);
        Assert.Contains("tick 1", error.Message, StringComparison.Ordinal);
        Assert.Equal(1, error.Context.Tick);
    }

    [Theory]
    [InlineData("hash")]
    [InlineData("where")]
    [InlineData("help")]
    public void AReportCommandGivesALineAndSendsNoIntent(string name)
    {
        // A command that changes no state makes no intent, so the record holds no line that
        // no rule reads (D-724).
        Simulation run = Start();
        List<Intent> queued = [];

        IReadOnlyList<string> answer = DebugAssemblyFile.Run(name, () => run.State, queued.Add);

        Assert.Empty(queued);
        Assert.Equal($"> {name}", answer[0]);
        Assert.True(answer.Count > 1, $"The command '{name}' gave no line (T-2).");
    }

    [Fact]
    public void TheHashCommandGivesTheStateHashOfTheRun()
    {
        Simulation run = Start();

        IReadOnlyList<string> answer = DebugAssemblyFile.Run("hash", () => run.State, Refuse);

        Assert.Contains($"0x{run.StateHash():x16}", string.Join(" ", answer), StringComparison.Ordinal);
    }

    [Fact]
    public void TheWhereCommandGivesTheTickAndThePlaceOfTheParty()
    {
        Simulation run = Start();
        run.Step([]);

        string answer = string.Join(" ", DebugAssemblyFile.Run("where", () => run.State, Refuse));

        Assert.Contains("tick 1", answer, StringComparison.Ordinal);
        Assert.Contains(run.State.Party.Map.Id.Value, answer, StringComparison.Ordinal);
        Assert.Contains(run.State.Party.LeadAt.ToString(), answer, StringComparison.Ordinal);
    }

    [Fact]
    public void TheHelpCommandNamesEveryCommandOfTheBuild()
    {
        Simulation run = Start();

        string answer = string.Join(" ", DebugAssemblyFile.Run("help", () => run.State, Refuse));

        foreach (string name in DebugAssemblyFile.CommandNames())
        {
            Assert.Contains(name, answer, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void AnUnknownNameGivesTheFaultAndTheNameOfEveryCommand()
    {
        // The answer names what the session read and what it holds (T-2).
        Simulation run = Start();

        string answer = string.Join(" ", DebugAssemblyFile.Run("teleport", () => run.State, Refuse));

        Assert.Contains("teleport", answer, StringComparison.Ordinal);
        foreach (string name in DebugAssemblyFile.CommandNames())
        {
            Assert.Contains(name, answer, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ALineOfMoreThanOneWordGivesTheFault()
    {
        // An intent carries an id and no value, so no command takes an argument (D-493, D-724).
        Simulation run = Start();

        string answer = string.Join(" ", DebugAssemblyFile.Run("reveal all", () => run.State, Refuse));

        Assert.Contains("no command takes an argument", answer, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEmptyLineGivesNoLineAndSendsNoIntent()
    {
        Simulation run = Start();

        Assert.Empty(DebugAssemblyFile.Run("   ", () => run.State, Refuse));
    }

    [Fact]
    public void EveryCommandNameIsOneWordAndTheListHoldsNoRepeatedName()
    {
        // A name with a space would never reach its command, because a line of more than one
        // word is a fault (D-724, T-2).
        SortedSet<string> names = new(StringComparer.Ordinal);

        foreach (string name in DebugAssemblyFile.CommandNames())
        {
            Assert.DoesNotContain(" ", name, StringComparison.Ordinal);
            Assert.True(names.Add(name), $"Two commands take the name '{name}' (T-2).");
        }

        Assert.Equal(4, names.Count);
    }

    private static Simulation Start() =>
        Simulation.Start(Seed, TestMaps.Room, DebugAssemblyFile.Handlers());

    private static ContentId Id(string value) =>
        ContentId.Parse(value, "TheThingBelow.Tests/DebugConsoleTests.cs", nameof(Id));

    /// <summary>Fails a test that sends an intent where the test expects none (T-2).</summary>
    private static void Refuse(Intent intent) =>
        throw new InvalidOperationException(
            $"The command sent the intent '{intent.Describe()}', and it reports alone (D-724, T-2).");

    /// <summary>Records one run that used the reveal command on its first tick (D-171).</summary>
    private static RunRecord RevealRecord()
    {
        Simulation run = Start();
        RunRecorder recorder = new(RunHeader.ForThisBuild(ContentHash, Seed), run.Snapshot());

        Intent[] intents = [Intent.OfDebugConsole(Id(RevealId))];
        run.Step(intents);
        recorder.Step(run.Tick, intents);
        return recorder.Build();
    }
}
