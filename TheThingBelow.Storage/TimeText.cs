using System;
using System.Globalization;

namespace TheThingBelow.Storage;

/// <summary>
/// The two text forms of a wall-clock time: the stamp of a file name, and the moment of a log
/// line or a crash line (D-179, D-658).
/// </summary>
/// <remarks>
/// Core reads no clock, so the host reads the time and Storage turns it into text (G-3, D-179).
/// Every form takes the invariant culture and the UTC time zone, so a file name and a line
/// read the same on every machine (T-7).
/// <para>
/// Storage never reads the clock itself. The caller passes the time, so a test of a file name
/// and a test of a line pass a fixed time and need no clock seam (T-3).
/// </para>
/// </remarks>
public static class TimeText
{
    /// <summary>The form of the stamp of a file name, such as `20260918-014253`.</summary>
    public const string StampFormat = "yyyyMMdd-HHmmss";

    /// <summary>The form of a time in a line, such as `2026-09-18T01:42:53Z`.</summary>
    public const string MomentFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";

    /// <summary>Gives the stamp that a file name carries (D-658).</summary>
    /// <param name="time">The time, in UTC.</param>
    /// <returns>The stamp, which sorts as the time sorts.</returns>
    /// <exception cref="ArgumentException">The time is not a UTC time (T-2).</exception>
    public static string Stamp(DateTime time) => Text(time, StampFormat);

    /// <summary>Gives the time that a log line or a crash line carries (D-179).</summary>
    /// <param name="time">The time, in UTC.</param>
    /// <returns>The moment, with the `Z` of UTC at the end.</returns>
    /// <exception cref="ArgumentException">The time is not a UTC time (T-2).</exception>
    public static string Moment(DateTime time) => Text(time, MomentFormat);

    private static string Text(DateTime time, string format)
    {
        // A local time in a file name or a line names two moments two times a year, and the
        // reader of a report cannot tell which one. Thus the caller passes a UTC time (T-2).
        if (time.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                $"The time carries the kind '{time.Kind}', and every time of a file name and of a line is a UTC time (T-2).",
                nameof(time));
        }

        return time.ToString(format, CultureInfo.InvariantCulture);
    }
}
