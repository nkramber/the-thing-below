using System;

namespace TheThingBelow.Core.Logging;

/// <summary>
/// One line of a log file: the wall-clock time of the host, and the entry that a step of Core
/// returned (D-179).
/// </summary>
/// <remarks>
/// Core reads no clock, so the host gives the time as text and Core holds that text alone
/// (G-3, D-179). Storage writes one line for each pair, and the tick of the entry gives the
/// order inside a run (D-650, D-494).
/// </remarks>
public sealed class LogLine
{
    /// <summary>Makes one line from a time and an entry.</summary>
    /// <param name="time">The wall-clock time of the host, as text, such as `2026-09-18T01:42:53Z`.</param>
    /// <param name="entry">The entry that a step returned.</param>
    /// <exception cref="ArgumentException">The time has no character (T-2).</exception>
    /// <exception cref="ArgumentNullException">The entry is null (T-2).</exception>
    public LogLine(string time, LogEntry entry)
    {
        ArgumentException.ThrowIfNullOrEmpty(time);
        ArgumentNullException.ThrowIfNull(entry);

        this.Time = time;
        this.Entry = entry;
    }

    /// <summary>The wall-clock time of the host, which Core never makes (G-3).</summary>
    public string Time { get; }

    /// <summary>The entry of the line.</summary>
    public LogEntry Entry { get; }
}
