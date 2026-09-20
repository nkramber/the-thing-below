using System;
using System.Collections.Generic;
using System.Reflection;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Runs;
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
    public void AFrameOfOneTickRecordsThatTick()
    {
        Run run = Run.Start();

        run.Advance(OneTick);

        Assert.Equal(1, run.Tick);
        Assert.Equal(1, run.Record().EndTick);
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
            () => RunReplay.Play(record, record.Header.ContentHash, DebugIntentHandlers.None));
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
    private sealed class Run
    {
        private readonly object instance;
        private readonly MethodInfo queue;
        private readonly MethodInfo advance;
        private readonly MethodInfo record;
        private readonly PropertyInfo tick;

        private Run(Type type, object instance)
        {
            this.instance = instance;
            this.queue = type.GetMethod("Queue", [typeof(Intent)])
                ?? throw new InvalidOperationException("The run holds no 'Queue' method (T-2).");
            this.advance = type.GetMethod("Advance", [typeof(double)])
                ?? throw new InvalidOperationException("The run holds no 'Advance' method (T-2).");
            this.record = type.GetMethod("Record", Type.EmptyTypes)
                ?? throw new InvalidOperationException("The run holds no 'Record' method (T-2).");
            this.tick = type.GetProperty("Tick")
                ?? throw new InvalidOperationException("The run holds no 'Tick' value (T-2).");
        }

        public long Tick => (long)(this.tick.GetValue(this.instance)
            ?? throw new InvalidOperationException("The tick has no value (T-2)."));

        public static Run Start()
        {
            Type type = GameAssemblyFile.Type(RunTypeName);
            MethodInfo start = type.GetMethod("Start", [typeof(ContentSet), typeof(ulong)])
                ?? throw new InvalidOperationException("The run holds no 'Start' method (T-2).");
            object instance = start.Invoke(null, [Content.Value, Seed])
                ?? throw new InvalidOperationException("The 'Start' method gave no run (T-2).");
            return new Run(type, instance);
        }

        public void Queue(Intent intent) => this.queue.Invoke(this.instance, [intent]);

        public IReadOnlyList<LogEntry> Advance(double seconds) =>
            (IReadOnlyList<LogEntry>)(this.advance.Invoke(this.instance, [seconds])
                ?? throw new InvalidOperationException("The 'Advance' method gave nothing (T-2)."));

        public RunRecord Record() =>
            (RunRecord)(this.record.Invoke(this.instance, Type.EmptyTypes)
                ?? throw new InvalidOperationException("The 'Record' method gave nothing (T-2)."));
    }
}
