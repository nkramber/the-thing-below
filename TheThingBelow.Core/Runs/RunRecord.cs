using System;
using System.Collections.Generic;

namespace TheThingBelow.Core.Runs;

/// <summary>The intents of one tick of a run (D-493).</summary>
/// <param name="Tick">The tick that these intents belong to (D-650).</param>
/// <param name="Intents">The intents, in the order that the host made them. It holds one at least.</param>
public sealed record TickIntents(long Tick, IReadOnlyList<Intent> Intents);

/// <summary>
/// The record of one run: the header, one snapshot, and every intent after that snapshot
/// (G-5, F-10, D-651).
/// </summary>
/// <remarks>
/// A tick with no intent takes no line, because the tick of each line gives the time (D-650).
/// A replay steps the rules from the snapshot to <see cref="EndTick"/>, and it applies the
/// intents of each tick that the record names. The end tick is its own value, because a run
/// can take many ticks after its last intent.
/// <para>
/// The record takes a new snapshot at each save, and it drops every intent before that
/// snapshot, so its size stays bounded over a long run (F-10, D-651). PR-43 writes the save
/// that makes each snapshot, and PR-44 writes the crash file that carries the record.
/// </para>
/// </remarks>
public sealed class RunRecord
{
    /// <summary>Makes a record, and fails when its lines cannot describe a run (T-2).</summary>
    /// <param name="header">The header of the run (G-5).</param>
    /// <param name="snapshot">The state that the intents below start from (D-651).</param>
    /// <param name="ticks">The intents after the snapshot, with a rising tick on each entry.</param>
    /// <param name="endTick">The tick that the run reached, which no intent needs to name.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The snapshot or a tick entry breaks a rule (T-2).</exception>
    public RunRecord(
        RunHeader header,
        RunSnapshot snapshot,
        IReadOnlyList<TickIntents> ticks,
        long endTick)
    {
        ArgumentNullException.ThrowIfNull(header);
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(ticks);
        snapshot.Check("the record");

        long last = snapshot.Tick;
        foreach (TickIntents entry in ticks)
        {
            ArgumentNullException.ThrowIfNull(entry);
            ArgumentNullException.ThrowIfNull(entry.Intents);

            if (entry.Tick <= last)
            {
                throw new ArgumentException(
                    $"The record holds tick {entry.Tick} after tick {last}, and each tick must rise.",
                    nameof(ticks));
            }

            if (entry.Intents.Count == 0)
            {
                throw new ArgumentException(
                    $"The record holds tick {entry.Tick} with no intent, and such a tick takes no line (F-10).",
                    nameof(ticks));
            }

            foreach (Intent intent in entry.Intents)
            {
                ArgumentNullException.ThrowIfNull(intent);
            }

            last = entry.Tick;
        }

        if (endTick < last)
        {
            throw new ArgumentException(
                $"The record ends at tick {endTick}, and it holds a line at tick {last}.",
                nameof(endTick));
        }

        this.Header = header;
        this.Snapshot = snapshot;
        this.Ticks = ticks;
        this.EndTick = endTick;
    }

    /// <summary>The header of the run (G-5, D-448).</summary>
    public RunHeader Header { get; }

    /// <summary>The state that the intents of this record start from (D-651).</summary>
    public RunSnapshot Snapshot { get; }

    /// <summary>Each tick that holds an intent, in the order of the run.</summary>
    public IReadOnlyList<TickIntents> Ticks { get; }

    /// <summary>The tick that the run reached. A replay steps the rules up to this tick.</summary>
    public long EndTick { get; }
}
