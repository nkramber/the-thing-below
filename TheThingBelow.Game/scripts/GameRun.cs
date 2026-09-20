using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Game;

/// <summary>
/// One run in the game: the fixed-step clock, the simulation, and the recorder of the run
/// (D-164, G-5). Game holds no rule, and it calls Core on each tick (D-100).
/// </summary>
/// <remarks>
/// A release build passes no debug handler, so a debug intent in a record fails here with
/// the intent and the tick (D-260, D-492). PR-45 creates the debug assembly, and a
/// development build then passes its handlers to <see cref="Simulation.Start"/>.
/// <para>
/// PR-61 makes an intent from each input event and gives it to <see cref="Queue"/>. No
/// intent comes from a Godot timer, a physics step, or a poll of the input singleton (G-23,
/// F-50).
/// </para>
/// <para>
/// <see cref="Advance"/> gives the log entries of the ticks that it ran, and <see cref="Boot"/>
/// writes each one to the log file of the session with the time of the host (D-179). A crash
/// takes <see cref="Record"/> into the crash file (D-170).
/// </para>
/// </remarks>
public sealed class GameRun
{
    private readonly FixedStepLoop loop = new();
    private readonly List<Intent> queued = [];
    private readonly Simulation simulation;
    private readonly RunRecorder recorder;

    private GameRun(Simulation simulation, RunRecorder recorder)
    {
        this.simulation = simulation;
        this.recorder = recorder;
    }

    /// <summary>The count of ticks since the start of the run (D-164).</summary>
    public long Tick => this.simulation.Tick;

    /// <summary>The count of ticks that the record holds a line for (F-10).</summary>
    public int RecordedLines => this.recorder.LineCount;

    /// <summary>Starts a run over a content set.</summary>
    /// <param name="content">The content of this build, which gives the content hash (D-648).</param>
    /// <param name="seed">The seed of the run (G-3, G-4).</param>
    /// <returns>The run, at tick zero.</returns>
    /// <exception cref="ArgumentNullException">The content set is null (T-2).</exception>
    public static GameRun Start(ContentSet content, ulong seed)
    {
        ArgumentNullException.ThrowIfNull(content);

        RunHeader header = RunHeader.ForThisBuild(content.Hash, seed);
        Simulation simulation = Simulation.Start(seed, DebugIntentHandlers.None);
        return new GameRun(simulation, new RunRecorder(header, simulation.Snapshot()));
    }

    /// <summary>Adds an intent that the next tick applies (D-493).</summary>
    /// <param name="intent">The intent, which Game made from one input event (F-50).</param>
    /// <exception cref="ArgumentNullException">The intent is null (T-2).</exception>
    public void Queue(Intent intent)
    {
        ArgumentNullException.ThrowIfNull(intent);

        this.queued.Add(intent);
    }

    /// <summary>Runs the ticks that the time of one frame gives (D-164).</summary>
    /// <param name="seconds">The time of the frame, in seconds.</param>
    /// <returns>The log entries of every tick of this frame, in the order of the ticks (D-179).</returns>
    /// <exception cref="ArgumentOutOfRangeException">The time is below zero (T-2).</exception>
    /// <remarks>
    /// The queued intents go to the first tick of the frame, because they came before it.
    /// Each later tick of the same frame takes no intent.
    /// </remarks>
    public IReadOnlyList<LogEntry> Advance(double seconds)
    {
        int ticks = this.loop.Advance(seconds);
        List<LogEntry> log = [];
        for (int step = 0; step < ticks; step += 1)
        {
            Intent[] intents = step == 0 ? [.. this.queued] : [];
            log.AddRange(this.simulation.Step(intents));
            this.recorder.Step(this.simulation.Tick, intents);
        }

        if (ticks > 0)
        {
            this.queued.Clear();
        }

        return log;
    }

    /// <summary>Gives the record of the run as it stands now (G-5, D-651).</summary>
    /// <returns>The record, which the crash file of D-170 carries.</returns>
    public RunRecord Record() => this.recorder.Build();

    /// <summary>Gives the state hash of the run, which a replay compares (G-5).</summary>
    /// <returns>The hash.</returns>
    public ulong StateHash() => this.simulation.StateHash();
}
