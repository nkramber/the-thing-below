using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Hashing;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Streams;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The tick, the intents, and the world of one tick (D-164, D-493, D-650). The seed of every
/// test below is fixed, because a rule of the tick never depends on a draw.
/// </summary>
public sealed class SimulationTests
{
    private const ulong Seed = 20260918;

    private static readonly IReadOnlyList<Intent> NoIntents = [];

    [Fact]
    public void AStepAddsOneToTheTick()
    {
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, DebugIntentHandlers.None);

        run.Step(NoIntents);
        run.Step(NoIntents);

        Assert.Equal(2, run.Tick);
    }

    [Fact]
    public void TheTickRisesWhileAMenuIsOpen()
    {
        // D-650. The tick is the one time line of a run, so it rises on every step.
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, DebugIntentHandlers.None);

        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        run.Step(NoIntents);
        run.Step(NoIntents);

        Assert.True(run.State.MenuOpen);
        Assert.Equal(3, run.Tick);
    }

    [Fact]
    public void TheWorldSkipsItsWorkWhileAMenuIsOpen()
    {
        // D-650 and D-162. The world tick counts the ticks in which the world ran.
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, DebugIntentHandlers.None);

        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        for (int step = 0; step < 10; step += 1)
        {
            run.Step(NoIntents);
        }

        Assert.Equal(11, run.Tick);
        Assert.Equal(0, run.State.WorldTick);
    }

    [Fact]
    public void TheWorldRunsAgainWhenTheMenuCloses()
    {
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, DebugIntentHandlers.None);

        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        run.Step(NoIntents);
        run.Step([Intent.OfPlayer(IntentIds.CloseMenu)]);
        run.Step(NoIntents);

        Assert.Equal(4, run.Tick);
        Assert.Equal(2, run.State.WorldTick);
    }

    [Fact]
    public void ThePartyReachesTheNextTileAfterTheTicksOfAStep()
    {
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, DebugIntentHandlers.None);
        TilePoint start = run.State.Party.LeadAt;

        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        for (int step = 1; step < MapRules.TicksPerStep; step += 1)
        {
            run.Step(NoIntents);
            Assert.Equal(start, run.State.Party.LeadAt);
        }

        run.Step(NoIntents);

        Assert.Equal(start.Step(StepDirection.East), run.State.Party.LeadAt);
        Assert.Null(run.State.Party.Stepping);
    }

    [Fact]
    public void AMenuHoldsTheStepOfThePartyBack()
    {
        // A menu pauses the world, so a party with a menu open never reaches the next tile
        // (D-162, D-650).
        Simulation open = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, DebugIntentHandlers.None);
        Simulation closed = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, DebugIntentHandlers.None);

        closed.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        open.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        open.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        for (int step = 0; step < MapRules.TicksPerStep * 2; step += 1)
        {
            open.Step(NoIntents);
            closed.Step(NoIntents);
        }

        Assert.Equal(TestMaps.Room.Spawn, open.State.Party.LeadAt);
        Assert.NotEqual(TestMaps.Room.Spawn, closed.State.Party.LeadAt);
    }

    [Fact]
    public void AStepIntentWhileAMenuIsOpenIsAnError()
    {
        // A menu takes every input of the player, so a step intent from it points at a fault
        // in the screen that made it (D-162, T-2).
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, DebugIntentHandlers.None);
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);

        SimulationException error = Assert.Throws<SimulationException>(
            () => run.Step([Intent.OfPlayer(IntentIds.MoveNorth)]));

        Assert.Contains("menu is open", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AWalkOnTheMapDrawsFromNoStream()
    {
        // The map rules take no random value, so every stream stays at its first position
        // while the party walks (G-4). PR-8 walks each patrol and draws its route.
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, DebugIntentHandlers.None);
        RunSnapshot start = run.Snapshot();

        for (int step = 0; step < MapRules.TicksPerStep * 3; step += 1)
        {
            run.Step([Intent.OfPlayer(IntentIds.MoveSouth)]);
        }

        RunSnapshot after = run.Snapshot();
        for (int index = 0; index < after.Streams.Count; index += 1)
        {
            Assert.Equal(start.Streams[index].State, after.Streams[index].State);
        }
    }

    [Fact]
    public void AnIntentThatNamesNoRuleIsAnError()
    {
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, DebugIntentHandlers.None);
        ContentId unknown = ContentId.Parse("intent.fly", "test", "action");

        SimulationException error = Assert.Throws<SimulationException>(
            () => run.Step([Intent.OfPlayer(unknown)]));

        Assert.Contains("intent.fly", error.Message, StringComparison.Ordinal);
        Assert.Contains("tick 1", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnOpenOfAnOpenMenuIsAnError()
    {
        // An intent that changes nothing points at a fault in the screen that made it (T-2).
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, DebugIntentHandlers.None);
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);

        SimulationException error = Assert.Throws<SimulationException>(
            () => run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]));

        Assert.Contains("already", error.Message, StringComparison.Ordinal);
        Assert.Contains("tick 2", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ACloseOfAClosedMenuIsAnError()
    {
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, DebugIntentHandlers.None);

        Assert.Throws<SimulationException>(() => run.Step([Intent.OfPlayer(IntentIds.CloseMenu)]));
    }

    [Fact]
    public void ADebugIntentReachesTheHandlerOfTheHost()
    {
        // D-260 and D-492: Core takes the handlers from the host, and it names no debug
        // assembly. The handler below stands for the console of PR-45.
        DebugIntentHandlers handlers = new(
        [
            new KeyValuePair<ContentId, DebugIntentHandler>(
                RunScripts.DebugStepEast,
                (state, intent, context, log) => state.WantStep(StepDirection.East, context)),
        ]);
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, handlers);

        run.Step([Intent.OfDebugConsole(RunScripts.DebugStepEast)]);

        Assert.Equal(StepDirection.East, run.State.Party.Facing);
    }

    [Fact]
    public void ADebugIntentOnAHostWithNoHandlerNamesTheIntentAndTheTick()
    {
        // Exit test 4 of section 7.13 of `phase-1-foundations.md`. A release host passes no
        // handler (D-260, D-492).
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, DebugIntentHandlers.None);
        run.Step(NoIntents);

        SimulationException error = Assert.Throws<SimulationException>(
            () => run.Step([Intent.OfDebugConsole(RunScripts.DebugStepEast)]));

        Assert.Contains("debug.step_east", error.Message, StringComparison.Ordinal);
        Assert.Contains("tick 2", error.Message, StringComparison.Ordinal);
        Assert.Equal(2, error.Context.Tick);
    }

    [Fact]
    public void TwoHandlersOfOneDebugIntentAreAnError()
    {
        static void Handler(RunState state, Intent intent, RunContext context, List<LogEntry> log) =>
            state.WantStep(StepDirection.East, context);

        Assert.Throws<ArgumentException>(() => new DebugIntentHandlers(
        [
            new KeyValuePair<ContentId, DebugIntentHandler>(RunScripts.DebugStepEast, Handler),
            new KeyValuePair<ContentId, DebugIntentHandler>(RunScripts.DebugStepEast, Handler),
        ]));
    }

    [Fact]
    public void AReleaseHostPassesNoHandler()
    {
        Assert.Equal(0, DebugIntentHandlers.None.Count);
    }

    [Fact]
    public void ARunWithTheSameSeedAndScriptGivesTheSameStateHash()
    {
        IReadOnlyList<IReadOnlyList<Intent>> script = RunScripts.Make(Seed, 400);

        (Simulation first, _) = RunScripts.Play(Seed, "hash", script, 0);
        (Simulation second, _) = RunScripts.Play(Seed, "hash", script, 0);

        Assert.Equal(first.StateHash(), second.StateHash());
    }

    [Fact]
    public void TwoSeedsGiveTwoStateHashes()
    {
        IReadOnlyList<IReadOnlyList<Intent>> script = RunScripts.Make(Seed, 400);

        (Simulation first, _) = RunScripts.Play(Seed, "hash", script, 0);
        (Simulation second, _) = RunScripts.Play(Seed + 1, "hash", script, 0);

        Assert.NotEqual(first.StateHash(), second.StateHash());
    }

    [Fact]
    public void ASnapshotStartsTheRunAgainAtTheSameState()
    {
        IReadOnlyList<IReadOnlyList<Intent>> script = RunScripts.Make(Seed, 250);
        (Simulation run, _) = RunScripts.Play(Seed, "hash", script, 0);

        Simulation resumed = Simulation.Resume(Seed, run.Snapshot(), TestMaps.Room, TestBattles.Content, DebugIntentHandlers.None);

        Assert.Equal(run.StateHash(), resumed.StateHash());
        Assert.Equal(run.Tick, resumed.Tick);
    }

    [Fact]
    public void TheStateHashStartsWithTheSimulationVersion()
    {
        // G-17. A build with other rules never gives the hash of this build by accident. The
        // sequence below is the layout of the state hash, so a change of the layout changes
        // this test and the identity file together (D-504).
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, DebugIntentHandlers.None);
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        RunSnapshot snapshot = run.Snapshot();

        StateHasher hasher = new();
        hasher.AddInt32(SimulationVersion.Current);
        hasher.AddUInt64(Seed);
        hasher.AddInt64(snapshot.Tick);
        hasher.AddBoolean(snapshot.MenuOpen);
        hasher.AddInt64(snapshot.WorldTick);
        run.State.Party.Hash(hasher);
        run.State.Characters.Hash(hasher);
        hasher.AddBoolean(false);
        foreach (StreamPosition position in snapshot.Streams)
        {
            hasher.AddInt32((int)position.Stream);
            hasher.AddUInt64(position.State);
            hasher.AddUInt64(position.Increment);
        }

        Assert.Equal(hasher.Finish(), run.StateHash());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(MapRules.TicksPerStep)]
    public void ASnapshotWithStepTicksOutsideTheirRangeIsAnError(int ticks)
    {
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, DebugIntentHandlers.None);
        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        RunSnapshot start = run.Snapshot();
        RunSnapshot broken = start with { Map = start.Map! with { StepTicks = ticks } };

        ArgumentException error = Assert.Throws<ArgumentException>(() => broken.Check("the test"));

        Assert.Contains("step ticks", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASnapshotWithAWorldTickAboveItsTickIsAnError()
    {
        RunSnapshot start = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, DebugIntentHandlers.None).Snapshot();
        RunSnapshot broken = start with { Tick = 5, WorldTick = 6 };

        Assert.Throws<ArgumentException>(() => broken.Check("the test"));
    }

    [Fact]
    public void ASnapshotWithAnAbsentStreamIsAnError()
    {
        RunSnapshot start = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, DebugIntentHandlers.None).Snapshot();
        RunSnapshot broken = start with { Streams = [start.Streams[0]] };

        ArgumentException error = Assert.Throws<ArgumentException>(() => broken.Check("the test"));

        Assert.Contains("streams", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARunHoldsNoStreamOfAnotherNumber()
    {
        RunState state = RunState.Start(Seed, TestMaps.Room, TestBattles.Content);

        Assert.Throws<ArgumentOutOfRangeException>(() => state.Stream((StreamId)99));
    }
}
