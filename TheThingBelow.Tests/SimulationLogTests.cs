using System;
using System.Collections.Generic;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The log entries that a step of Core returns: the menu at the info level, and a beat of the
/// patrol at the debug level (D-179, D-660).
/// </summary>
/// <remarks>
/// Core keeps no entry, and it adds no wall-clock time and no file path (G-1, G-3). Thus the
/// entries of one seed are the same on every machine and in every replay (T-7).
/// </remarks>
public sealed class SimulationLogTests
{
    /// <summary>The seed of the runs of these tests.</summary>
    private const ulong Seed = 0x00000000000c10c5;

    /// <summary>The intents of a tick that holds none.</summary>
    private static readonly IReadOnlyList<Intent> NoIntents = [];

    [Fact]
    public void AStepOfNoChangeReturnsNoEntry()
    {
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, TestBattles.Notices, DebugIntentHandlers.None);

        Assert.Empty(run.Step(NoIntents));
    }

    [Fact]
    public void AMenuChangeReturnsOneInfoEntryOfTheRun()
    {
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, TestBattles.Notices, DebugIntentHandlers.None);

        LogEntry opened = Assert.Single(run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]));

        Assert.Equal(LogLevel.Info, opened.Level);
        Assert.Equal("the menu opened", opened.Message);
        Assert.Equal(LogSubsystems.Run, opened.Subsystem);
        Assert.Equal(run.Tick, opened.Tick);
        Assert.Equal("action", Assert.Single(opened.Fields).Name);
        Assert.Equal(IntentIds.OpenMenu.Value, Assert.Single(opened.Fields).Value);
    }

    [Fact]
    public void ACloseOfTheMenuReturnsItsOwnEntry()
    {
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, TestBattles.Notices, DebugIntentHandlers.None);
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);

        LogEntry closed = Assert.Single(run.Step([Intent.OfPlayer(IntentIds.CloseMenu)]));

        Assert.Equal("the menu closed", closed.Message);
        Assert.Equal(LogLevel.Info, closed.Level);
    }

    [Fact]
    public void TheStartOfAStepReturnsOneDebugEntryOfTheWorld()
    {
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, TestBattles.Notices, DebugIntentHandlers.None);

        LogEntry started = Assert.Single(run.Step([Intent.OfPlayer(IntentIds.MoveEast)]));

        Assert.Equal(LogLevel.Debug, started.Level);
        Assert.Equal("the party started a step to the east", started.Message);
        Assert.Equal(LogSubsystems.World, started.Subsystem);
        Assert.Equal(1, started.Tick);
        Assert.Equal(["x", "y", "direction"], Names(started));
    }

    [Fact]
    public void TheEndOfAStepReturnsOneDebugEntryWithTheWalkedCount()
    {
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, TestBattles.Notices, DebugIntentHandlers.None);
        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        for (int tick = 1; tick < MapRules.TicksPerStep; tick += 1)
        {
            run.Step(NoIntents);
        }

        LogEntry reached = Assert.Single(run.Step(NoIntents));

        Assert.Equal("the party reached a tile", reached.Message);
        Assert.Equal(["x", "y", "walked", "world-tick"], Names(reached));
        Assert.Equal("2", reached.Fields[2].Value);
    }

    [Fact]
    public void AMenuHoldsTheStepOfTheParty()
    {
        // A menu pauses the world, so no step logs while the menu is open (D-162, D-650).
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, TestBattles.Notices, DebugIntentHandlers.None);
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);

        for (int tick = 0; tick < MapRules.TicksPerStep * 2; tick += 1)
        {
            Assert.Empty(run.Step(NoIntents));
        }
    }

    [Fact]
    public void TheEntriesOfOneSeedAreTheSameOnEveryRun()
    {
        // A replay of a seed gives the same lines, because Core reads no clock (T-7).
        Assert.Equal(Describe(Play(Seed, 200)), Describe(Play(Seed, 200)));
    }

    [Fact]
    public void NoEntryOfARunHoldsAPathOrATimeField()
    {
        // Exit test 6 of section 7.15: Core adds no time value and no file path (G-1, G-3).
        foreach (LogEntry entry in Play(Seed, 400))
        {
            foreach (LogField field in entry.Fields)
            {
                Assert.DoesNotContain('/', field.Value);
                Assert.DoesNotContain('\\', field.Value);
                Assert.NotEqual("time", field.Name);
            }
        }
    }

    [Fact]
    public void AStepReturnsTheEntriesOfThatStepAlone()
    {
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, TestBattles.Notices, DebugIntentHandlers.None);
        Assert.Single(run.Step([Intent.OfPlayer(IntentIds.MoveEast)]));

        // The step after the start of a step holds no entry, so Core kept none of the
        // entries before it.
        Assert.Empty(run.Step(NoIntents));
    }

    [Fact]
    public void TheWorldRefusesAListThatIsNull()
    {
        RunState state = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, TestBattles.Notices, DebugIntentHandlers.None).State;

        Assert.Throws<ArgumentNullException>(() => WorldRules.Step(state, null!));
    }

    private static IReadOnlyList<LogEntry> Play(ulong seed, int tickCount)
    {
        Simulation run = Simulation.Start(seed, TestMaps.Room, TestBattles.Content, TestBattles.Notices, DebugIntentHandlers.None);
        List<LogEntry> entries = [];
        foreach (IReadOnlyList<Intent> intents in RunScripts.Make(seed, tickCount))
        {
            entries.AddRange(run.Step(intents));
        }

        return entries;
    }

    private static IReadOnlyList<string> Names(LogEntry entry)
    {
        List<string> names = [];
        foreach (LogField field in entry.Fields)
        {
            names.Add(field.Name);
        }

        return names;
    }

    private static string Describe(IReadOnlyList<LogEntry> entries)
    {
        List<string> lines = [];
        foreach (LogEntry entry in entries)
        {
            lines.Add(LogLineText.Write(new LogLine("2026-09-18T00:00:00Z", entry)));
        }

        return string.Join('\n', lines);
    }
}
