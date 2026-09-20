using System;
using System.Collections.Generic;

namespace TheThingBelow.Core.Runs;

/// <summary>
/// Writes the record of a run while the run goes on. The host calls <see cref="Step"/> once
/// for each tick, and <see cref="Save"/> at each save (G-5, D-651).
/// </summary>
/// <remarks>
/// The recorder keeps one snapshot and the intents after it. A save replaces the snapshot
/// and drops every intent before it, so the record of a long run stays bounded (F-10,
/// D-651). The distance between two saves thus sets the length of a replay after a crash,
/// and the save points of D-224 set that distance.
/// <para>
/// A tick with no intent takes no line, and the recorder keeps the end tick of the run, so a
/// replay reaches the state of the last tick (D-650).
/// </para>
/// </remarks>
public sealed class RunRecorder
{
    private readonly RunHeader header;
    private readonly List<TickIntents> ticks;
    private RunSnapshot snapshot;
    private long endTick;

    /// <summary>Starts a recorder at a snapshot of the run.</summary>
    /// <param name="header">The header of the run (G-5).</param>
    /// <param name="start">The state that the run starts from, which is tick zero for a new run.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The snapshot is not a state of a run (T-2).</exception>
    public RunRecorder(RunHeader header, RunSnapshot start)
    {
        ArgumentNullException.ThrowIfNull(header);
        ArgumentNullException.ThrowIfNull(start);
        start.Check("the recorder");

        this.header = header;
        this.snapshot = start;
        this.ticks = [];
        this.endTick = start.Tick;
    }

    /// <summary>The count of ticks that the record holds a line for.</summary>
    public int LineCount => this.ticks.Count;

    /// <summary>The tick of the snapshot that the record starts from (D-651).</summary>
    public long SnapshotTick => this.snapshot.Tick;

    /// <summary>Records one tick of the run.</summary>
    /// <param name="tick">The tick that the simulation runs now, which rises by one on each step.</param>
    /// <param name="intents">The intents of that tick, which can hold none.</param>
    /// <exception cref="ArgumentNullException">The list is null (T-2).</exception>
    /// <exception cref="ArgumentException">The tick is not the tick after the last one (T-2).</exception>
    /// <remarks>
    /// The host records a tick before it steps the simulation, so the record of a crash holds
    /// the intents of the tick that crashed and a replay reaches the crash (D-170, G-5).
    /// </remarks>
    public void Step(long tick, IReadOnlyList<Intent> intents)
    {
        ArgumentNullException.ThrowIfNull(intents);

        // A gap would give a record that replays with no error and another state, because
        // the replay steps the missing ticks with no intent (T-2, G-5).
        if (tick != this.endTick + 1)
        {
            throw new ArgumentException(
                $"The recorder reached tick {this.endTick}, and this step gives tick {tick}. Each step gives the tick after the last one.",
                nameof(tick));
        }

        this.endTick = tick;
        if (intents.Count > 0)
        {
            // The recorder copies the list, because the host can use one list again on the
            // next tick. A spread of an `IReadOnlyList` calls `System.Linq`, which Core
            // never references, so the copy below walks the list itself (G-1).
            Intent[] copy = new Intent[intents.Count];
            for (int index = 0; index < copy.Length; index += 1)
            {
                copy[index] = intents[index];
            }

            this.ticks.Add(new TickIntents(tick, copy));
        }
    }

    /// <summary>
    /// Takes a new snapshot, and drops every intent before it. The host calls this method at
    /// each save, and the save writes the same snapshot to its file (D-259, D-651).
    /// </summary>
    /// <param name="snapshot">The state of the run at the end of the tick that it names.</param>
    /// <exception cref="ArgumentNullException">The snapshot is null (T-2).</exception>
    /// <exception cref="ArgumentException">The snapshot is outside the recorded run (T-2).</exception>
    public void Save(RunSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        snapshot.Check("the save");

        if (snapshot.Tick < this.snapshot.Tick || snapshot.Tick > this.endTick)
        {
            throw new ArgumentException(
                $"The save names tick {snapshot.Tick}, and the record holds tick {this.snapshot.Tick} to tick {this.endTick}.",
                nameof(snapshot));
        }

        this.snapshot = snapshot;
        this.ticks.RemoveAll(entry => entry.Tick <= snapshot.Tick);
    }

    /// <summary>Gives the record as it stands now.</summary>
    /// <returns>The record, with the current snapshot and the intents after it.</returns>
    public RunRecord Build() => new(this.header, this.snapshot, [.. this.ticks], this.endTick);
}
