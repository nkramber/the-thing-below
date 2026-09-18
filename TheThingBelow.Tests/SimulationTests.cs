using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
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
        Simulation run = Simulation.Start(Seed, DebugIntentHandlers.None);

        run.Step(NoIntents);
        run.Step(NoIntents);

        Assert.Equal(2, run.Tick);
    }

    [Fact]
    public void TheTickRisesWhileAMenuIsOpen()
    {
        // D-650. The tick is the one time line of a run, so it rises on every step.
        Simulation run = Simulation.Start(Seed, DebugIntentHandlers.None);

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
        Simulation run = Simulation.Start(Seed, DebugIntentHandlers.None);

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
        Simulation run = Simulation.Start(Seed, DebugIntentHandlers.None);

        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        run.Step(NoIntents);
        run.Step([Intent.OfPlayer(IntentIds.CloseMenu)]);
        run.Step(NoIntents);

        Assert.Equal(4, run.Tick);
        Assert.Equal(2, run.State.WorldTick);
    }

    [Fact]
    public void ThePatrolWalksOnEachBeatOfTheWorld()
    {
        Simulation run = Simulation.Start(Seed, DebugIntentHandlers.None);

        for (int step = 0; step < WorldRules.TicksPerPatrolBeat * 3; step += 1)
        {
            run.Step(NoIntents);
        }

        Assert.Equal(3, run.State.PatrolBeats);
        Assert.InRange(run.State.PatrolChoice, 0, WorldRules.PatrolChoiceCount - 1);
    }

    [Fact]
    public void AMenuHoldsTheBeatOfThePatrolBack()
    {
        // A menu pauses the world, so the beats of a run with a menu open fall behind the
        // beats of a run with no menu (D-162, D-650).
        Simulation open = Simulation.Start(Seed, DebugIntentHandlers.None);
        Simulation closed = Simulation.Start(Seed, DebugIntentHandlers.None);

        open.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        closed.Step(NoIntents);
        for (int step = 0; step < WorldRules.TicksPerPatrolBeat * 2; step += 1)
        {
            open.Step(NoIntents);
            closed.Step(NoIntents);
        }

        Assert.Equal(open.Tick, closed.Tick);
        Assert.Equal(0, open.State.PatrolBeats);
        Assert.Equal(2, closed.State.PatrolBeats);
    }

    [Fact]
    public void ThePatrolDrawsFromTheExplorationStreamAlone()
    {
        // Each subsystem draws from its own stream (G-4). The three other streams stay at
        // their first position while the patrol walks.
        Simulation run = Simulation.Start(Seed, DebugIntentHandlers.None);
        RunSnapshot start = run.Snapshot();

        for (int step = 0; step < WorldRules.TicksPerPatrolBeat; step += 1)
        {
            run.Step(NoIntents);
        }

        RunSnapshot after = run.Snapshot();
        for (int index = 0; index < after.Streams.Count; index += 1)
        {
            bool moved = after.Streams[index].State != start.Streams[index].State;
            Assert.Equal(after.Streams[index].Stream == StreamId.Exploration, moved);
        }
    }

    [Fact]
    public void AnIntentThatNamesNoRuleIsAnError()
    {
        Simulation run = Simulation.Start(Seed, DebugIntentHandlers.None);
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
        Simulation run = Simulation.Start(Seed, DebugIntentHandlers.None);
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);

        SimulationException error = Assert.Throws<SimulationException>(
            () => run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]));

        Assert.Contains("already", error.Message, StringComparison.Ordinal);
        Assert.Contains("tick 2", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ACloseOfAClosedMenuIsAnError()
    {
        Simulation run = Simulation.Start(Seed, DebugIntentHandlers.None);

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
                RunScripts.DebugWalkPatrol,
                (state, context) => state.WalkPatrol(context)),
        ]);
        Simulation run = Simulation.Start(Seed, handlers);

        run.Step([Intent.OfDebugConsole(RunScripts.DebugWalkPatrol)]);

        Assert.Equal(1, run.State.PatrolBeats);
    }

    [Fact]
    public void ADebugIntentOnAHostWithNoHandlerNamesTheIntentAndTheTick()
    {
        // Exit test 4 of section 7.13 of `phase-1-foundations.md`. A release host passes no
        // handler (D-260, D-492).
        Simulation run = Simulation.Start(Seed, DebugIntentHandlers.None);
        run.Step(NoIntents);

        SimulationException error = Assert.Throws<SimulationException>(
            () => run.Step([Intent.OfDebugConsole(RunScripts.DebugWalkPatrol)]));

        Assert.Contains("debug.walk_patrol", error.Message, StringComparison.Ordinal);
        Assert.Contains("tick 2", error.Message, StringComparison.Ordinal);
        Assert.Equal(2, error.Context.Tick);
    }

    [Fact]
    public void TwoHandlersOfOneDebugIntentAreAnError()
    {
        static void Handler(RunState state, RunContext context) => state.WalkPatrol(context);

        Assert.Throws<ArgumentException>(() => new DebugIntentHandlers(
        [
            new KeyValuePair<ContentId, DebugIntentHandler>(RunScripts.DebugWalkPatrol, Handler),
            new KeyValuePair<ContentId, DebugIntentHandler>(RunScripts.DebugWalkPatrol, Handler),
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

        Simulation resumed = Simulation.Resume(Seed, run.Snapshot(), DebugIntentHandlers.None);

        Assert.Equal(run.StateHash(), resumed.StateHash());
        Assert.Equal(run.Tick, resumed.Tick);
    }

    [Fact]
    public void TheStateHashHoldsTheSimulationVersion()
    {
        // G-17. A build with other rules never gives the hash of this build by accident.
        Assert.True(SimulationVersion.Current >= 3);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(WorldRules.PatrolChoiceCount)]
    public void ASnapshotWithAPatrolChoiceOutsideItsRangeIsAnError(int choice)
    {
        RunSnapshot start = Simulation.Start(Seed, DebugIntentHandlers.None).Snapshot();
        RunSnapshot broken = start with { PatrolChoice = choice };

        ArgumentException error = Assert.Throws<ArgumentException>(() => broken.Check("the test"));

        Assert.Contains("patrol choice", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASnapshotWithAWorldTickAboveItsTickIsAnError()
    {
        RunSnapshot start = Simulation.Start(Seed, DebugIntentHandlers.None).Snapshot();
        RunSnapshot broken = start with { Tick = 5, WorldTick = 6 };

        Assert.Throws<ArgumentException>(() => broken.Check("the test"));
    }

    [Fact]
    public void ASnapshotWithAnAbsentStreamIsAnError()
    {
        RunSnapshot start = Simulation.Start(Seed, DebugIntentHandlers.None).Snapshot();
        RunSnapshot broken = start with { Streams = [start.Streams[0]] };

        ArgumentException error = Assert.Throws<ArgumentException>(() => broken.Check("the test"));

        Assert.Contains("streams", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARunHoldsNoStreamOfAnotherNumber()
    {
        RunState state = RunState.Start(Seed);

        Assert.Throws<ArgumentOutOfRangeException>(() => state.Stream((StreamId)99));
    }
}
