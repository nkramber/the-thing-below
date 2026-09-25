using System;
using System.Collections.Generic;
using TheThingBelow.Core.Logging;

namespace TheThingBelow.Core.Runs;

/// <summary>The build that wrote a snapshot, as a resume reads it (D-1111).</summary>
public enum SnapshotOrigin
{
    /// <summary>This build: the same simulation version and the same content hash.</summary>
    ThisBuild,

    /// <summary>Another build: another simulation version or another content hash.</summary>
    OtherBuild,
}

/// <summary>
/// How a resume treats a stored value that the maps and the story scenes of this build no
/// longer hold, and the log line of each change that it makes (D-1111 to D-1113).
/// </summary>
/// <remarks>
/// A snapshot of this build gets the strict checks alone, so a value that no run of this build
/// makes is corrupt, and the resume refuses it (T-2). A snapshot of another build can follow an
/// edit of a map or of a story scene. The resume then matches each enemy by its id, puts a lead
/// that stands off the map on the spawn point, and finds each step by its id, and it logs each
/// change as a warning (D-1111, D-1112, D-1113). The party gets no change: a change of a party
/// rule refuses the save, and a migration of that patch reads it (D-1110).
/// </remarks>
public sealed class ResumeDrift
{
    private readonly List<LogEntry> entries = [];

    private ResumeDrift(SnapshotOrigin origin, long tick)
    {
        this.Origin = origin;
        this.Tick = tick;
    }

    /// <summary>The build that wrote the snapshot.</summary>
    public SnapshotOrigin Origin { get; }

    /// <summary>The tick of the snapshot, which each log line names (D-179).</summary>
    public long Tick { get; }

    /// <summary>True when the resume can change a value that an edit of the content left behind.</summary>
    public bool Adjusts => this.Origin == SnapshotOrigin.OtherBuild;

    /// <summary>The log line of each change, in the order of the changes.</summary>
    public IReadOnlyList<LogEntry> Entries => this.entries;

    /// <summary>Makes the drift rule of one resume.</summary>
    /// <param name="origin">The build that wrote the snapshot.</param>
    /// <param name="tick">The tick of the snapshot.</param>
    /// <returns>The rule, with no log line yet.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The origin names no build, or the tick is below zero (T-2).</exception>
    public static ResumeDrift Of(SnapshotOrigin origin, long tick)
    {
        if (origin != SnapshotOrigin.ThisBuild && origin != SnapshotOrigin.OtherBuild)
        {
            throw new ArgumentOutOfRangeException(nameof(origin), origin, "The value names no origin of a snapshot (D-1111).");
        }

        ArgumentOutOfRangeException.ThrowIfNegative(tick);
        return new ResumeDrift(origin, tick);
    }

    /// <summary>Logs one change of a stored value (D-1113).</summary>
    /// <param name="subsystem">The subsystem that made the change (<see cref="LogSubsystems"/>).</param>
    /// <param name="message">What changed and why.</param>
    /// <param name="fields">The ids and the values of the change.</param>
    /// <exception cref="InvalidOperationException">The snapshot came from this build, which takes no change (T-2).</exception>
    public void Note(string subsystem, string message, IReadOnlyList<LogField> fields)
    {
        if (!this.Adjusts)
        {
            throw new InvalidOperationException(
                $"A resume of a snapshot of this build changed a stored value: {message}. A snapshot of this build takes the strict checks alone (D-1111, T-2).");
        }

        this.entries.Add(new LogEntry(LogLevel.Warning, message, this.Tick, subsystem, fields));
    }
}
