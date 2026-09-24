using System;
using System.Collections.Generic;
using System.Reflection;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Runs;
using TheThingBelow.Storage;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The run of Game: the fixed-step clock, the simulation, and the recorder of the run (D-164,
/// G-5). The tests read the built Game assembly, because Tests takes no reference to Game
/// (D-614).
/// </summary>
public sealed class GameRunTests
{
    /// <summary>The name of the type of Game that holds the run.</summary>
    private const string RunTypeName = "TheThingBelow.Game.GameRun";

    /// <summary>The seed of the runs of these tests.</summary>
    private const ulong Seed = 20260918;

    /// <summary>The time of one tick, in seconds.</summary>
    private const double OneTick = 1.0 / 60;

    private static readonly Lazy<ContentSet> Content =
        new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    [Fact]
    public void AQueuedOpenMenuPausesTheWorldForTheNextTick()
    {
        // A regression test of the crash that a press of the menu button with a direction
        // held would give. The host reads input before it runs the ticks of a frame, so the
        // queue holds the open-menu intent while the menu state of the run is still closed.
        // A host that read that state alone would queue a step for the same tick, and the
        // rules refuse a step while a menu is open (D-162, T-2).
        Run run = Run.Start();

        run.Queue(Intent.OfPlayer(IntentIds.OpenMenu));

        Assert.False(run.MenuOpen);
        Assert.True(run.MenuOpenNextTick);
    }

    [Fact]
    public void AQueuedCloseMenuRunsTheWorldAgainOnTheNextTick()
    {
        Run run = Run.Start();
        run.Queue(Intent.OfPlayer(IntentIds.OpenMenu));
        run.Advance(OneTick);
        run.Queue(Intent.OfPlayer(IntentIds.CloseMenu));

        Assert.True(run.MenuOpen);
        Assert.False(run.MenuOpenNextTick);
    }

    [Fact]
    public void AnEmptyQueueLeavesTheMenuStateOfTheRun()
    {
        Run run = Run.Start();

        Assert.Equal(run.MenuOpen, run.MenuOpenNextTick);
    }

    [Fact]
    public void AFrameOfOneTickRecordsThatTick()
    {
        Run run = Run.Start();

        run.Advance(OneTick);

        Assert.Equal(1, run.Tick);
        Assert.Equal(1, run.Record().EndTick);
    }

    [Fact]
    public void AHeldStepReachesEachTickOfAFrameOfTwoTicks()
    {
        // D-820. The held step once reached the first tick of a frame alone, so the party
        // stood still for a tick when it reached a tile on the second tick of a frame.
        Run run = Run.Start();
        Intent step = run.IntentOf("step_north");

        run.Advance(OneTick * 2.5, () => step);

        IReadOnlyList<TickIntents> ticks = run.Record().Ticks;
        Assert.Equal(2, ticks.Count);
        foreach (TickIntents tick in ticks)
        {
            Assert.Contains(tick.Intents, intent => string.CompareOrdinal(intent.Action.Value, step.Action.Value) == 0);
        }
    }

    [Fact]
    public void AHeldStepThatGivesNothingAddsNoIntent()
    {
        Run run = Run.Start();

        run.Advance(OneTick, () => null);

        // The record holds a line for a tick with an intent alone.
        Assert.Equal(1, run.Record().EndTick);
        Assert.Empty(run.Record().Ticks);
    }

    [Fact]
    public void TheRecordOfACrashHoldsTheIntentsOfTheTickThatCrashed()
    {
        // A regression test for the audit of 2026-09-20. The run recorded each tick after
        // the step, so the record of a refused intent ended one tick before the crash, and a
        // replay of the crash file never reached the crash (D-170, G-5).
        Run run = Run.Start();
        run.Queue(Intent.OfPlayer(IntentIds.CloseMenu));

        TargetInvocationException thrown = Assert.Throws<TargetInvocationException>(() => run.Advance(OneTick));
        Assert.IsType<SimulationException>(thrown.InnerException);

        RunRecord record = run.Record();
        Assert.Equal(1, run.Tick);
        Assert.Equal(1, record.EndTick);
        TickIntents line = Assert.Single(record.Ticks);
        Assert.Equal(IntentIds.CloseMenu, line.Intents[0].Action);

        SimulationException replay = Assert.Throws<SimulationException>(
            () => RunReplay.Play(record, record.Header.ContentHash, TestMaps.FixtureDungeon, TestBattles.Content, TestBattles.Notices, DebugIntentHandlers.None));
        Assert.Equal(1, replay.Context.Tick);
    }

    [Fact]
    public void AFrameThatDropsTicksLogsTheCount()
    {
        // No work of a step goes in silence (T-2). A frame of two seconds gives 120 ticks,
        // and the loop runs its maximum and drops the rest.
        Run run = Run.Start();

        IReadOnlyList<LogEntry> log = run.Advance(2.0);

        LogEntry entry = Assert.Single(log, item => item.Level == LogLevel.Error);
        Assert.Equal(LogSubsystems.Game, entry.Subsystem);
        LogField field = Assert.Single(entry.Fields);
        Assert.Equal("dropped", field.Name);
        Assert.Equal("112", field.Value);
    }

    /// <summary>The run of the built Game assembly, through its public members.</summary>
    [Fact]
    public void TheMenuActionOpensTheMenuAndThenClosesIt()
    {
        // The regression test of P1-1 of `docs/reviews/pr-41.md`. The host asks the run for
        // the intent of an action, and the run reads its own menu state (D-162, D-650).
        Run run = Run.Start();

        Intent first = run.IntentOf("menu");
        run.Queue(first);
        run.Advance(OneTick);

        Intent second = run.IntentOf("menu");
        run.Queue(second);
        run.Advance(OneTick);

        Assert.Equal(IntentIds.OpenMenu.Value, first.Action.Value);
        Assert.Equal(IntentIds.CloseMenu.Value, second.Action.Value);
        Assert.False(run.MenuOpen);
    }

    [Fact]
    public void TheMenuStateOfTheRunFollowsTheIntents()
    {
        // D-650. The state of the run is the one source of the menu state, so no host copy
        // can drift from it (T-2).
        Run run = Run.Start();
        Assert.False(run.MenuOpen);

        run.Queue(Intent.OfPlayer(IntentIds.OpenMenu));
        run.Advance(OneTick);
        Assert.True(run.MenuOpen);

        run.Queue(Intent.OfPlayer(IntentIds.CloseMenu));
        run.Advance(OneTick);
        Assert.False(run.MenuOpen);
    }

    [Fact]
    public void EveryOtherActionMakesOneIntentWhateverTheMenuDoes()
    {
        // D-493. The menu action alone reads the menu state.
        Run run = Run.Start();
        Intent shut = run.IntentOf("confirm");

        run.Queue(Intent.OfPlayer(IntentIds.OpenMenu));
        run.Advance(OneTick);

        Assert.True(run.MenuOpen);
        Assert.Equal(shut.Action.Value, run.IntentOf("confirm").Action.Value);
        Assert.Equal(IntentIds.Confirm.Value, shut.Action.Value);
    }

    private sealed class Run
    {
        private readonly object instance;
        private readonly MethodInfo queue;
        private readonly MethodInfo advance;
        private readonly MethodInfo record;
        private readonly MethodInfo intentOf;
        private readonly PropertyInfo tick;
        private readonly PropertyInfo menuOpen;
        private readonly PropertyInfo menuOpenNextTick;

        private Run(Type type, object instance)
        {
            this.instance = instance;
            this.queue = type.GetMethod("Queue", [typeof(Intent)])
                ?? throw new InvalidOperationException("The run holds no 'Queue' method (T-2).");
            this.advance = type.GetMethod("Advance", [typeof(double), typeof(Func<Intent>)])
                ?? throw new InvalidOperationException("The run holds no 'Advance' method (T-2).");
            this.record = type.GetMethod("Record", Type.EmptyTypes)
                ?? throw new InvalidOperationException("The run holds no 'Record' method (T-2).");
            this.intentOf = type.GetMethod("IntentOf", [typeof(string)])
                ?? throw new InvalidOperationException("The run holds no 'IntentOf' method (T-2).");
            this.tick = type.GetProperty("Tick")
                ?? throw new InvalidOperationException("The run holds no 'Tick' value (T-2).");
            this.menuOpen = type.GetProperty("MenuOpen")
                ?? throw new InvalidOperationException("The run holds no 'MenuOpen' value (T-2).");
            this.menuOpenNextTick = type.GetProperty("MenuOpenNextTick")
                ?? throw new InvalidOperationException("The run holds no 'MenuOpenNextTick' value (T-2).");
        }

        public long Tick => (long)(this.tick.GetValue(this.instance)
            ?? throw new InvalidOperationException("The tick has no value (T-2)."));

        public bool MenuOpenNextTick => (bool)(this.menuOpenNextTick.GetValue(this.instance)
            ?? throw new InvalidOperationException("The menu state of the next tick has no value (T-2)."));

        public bool MenuOpen => (bool)(this.menuOpen.GetValue(this.instance)
            ?? throw new InvalidOperationException("The menu state has no value (T-2)."));

        public Intent IntentOf(string action) =>
            (Intent)(this.intentOf.Invoke(this.instance, [action])
                ?? throw new InvalidOperationException("The 'IntentOf' method gave nothing (T-2)."));

        public static Run Start()
        {
            Type type = GameAssemblyFile.Type(RunTypeName);
            MethodInfo start = type.GetMethod(
                "Start",
                [typeof(ContentSet), typeof(ulong), typeof(DebugIntentHandlers), typeof(MessageSpeed)])
                ?? throw new InvalidOperationException("The run holds no 'Start' method (T-2).");

            // A test run passes no debug handler, as a release build does. The tests of the
            // console pass the handlers of the debug assembly (D-260, D-492).
            object instance = start.Invoke(null, [Content.Value, Seed, DebugIntentHandlers.None, MessageSpeed.Normal])
                ?? throw new InvalidOperationException("The 'Start' method gave no run (T-2).");
            return new Run(type, instance);
        }

        public void Queue(Intent intent) => this.queue.Invoke(this.instance, [intent]);

        public IReadOnlyList<LogEntry> Advance(double seconds, Func<Intent?>? heldStep = null) =>
            (IReadOnlyList<LogEntry>)(this.advance.Invoke(this.instance, [seconds, heldStep])
                ?? throw new InvalidOperationException("The 'Advance' method gave nothing (T-2)."));

        public RunRecord Record() =>
            (RunRecord)(this.record.Invoke(this.instance, Type.EmptyTypes)
                ?? throw new InvalidOperationException("The 'Record' method gave nothing (T-2)."));
    }
}
