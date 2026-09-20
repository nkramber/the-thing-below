using System;

namespace TheThingBelow.Core.Maps;

/// <summary>The time of day of one map, which the story sets and no clock moves (D-442).</summary>
/// <remarks>
/// No day clock runs. A map file gives its time, and a story flag can change that time while
/// the party stands on the map (D-442). The time sets the sight range of the party, and PR-8
/// reads it for the routes and the enemies of the map (D-193).
/// </remarks>
public enum TimeOfDay
{
    /// <summary>First light.</summary>
    Dawn,

    /// <summary>Full light.</summary>
    Day,

    /// <summary>Last light.</summary>
    Dusk,

    /// <summary>No light. The wrong things walk only at dusk and at night (D-442).</summary>
    Night,
}

/// <summary>The names of the times of day, as a map file writes them (D-442).</summary>
public static class TimesOfDay
{
    /// <summary>Every time, in one fixed order for a walk of them (G-4).</summary>
    public static readonly TimeOfDay[] All = [TimeOfDay.Dawn, TimeOfDay.Day, TimeOfDay.Dusk, TimeOfDay.Night];

    /// <summary>The names of every time, for the error of an unknown name (T-2).</summary>
    public const string EveryName = "dawn, day, dusk, night";

    /// <summary>Gives the time of one name.</summary>
    /// <param name="name">The name, such as `night`.</param>
    /// <param name="time">The time of that name, when the name names one.</param>
    /// <returns>True when the name names a time.</returns>
    /// <exception cref="ArgumentNullException">The name is null (T-2).</exception>
    public static bool TryOf(string name, out TimeOfDay time)
    {
        ArgumentNullException.ThrowIfNull(name);

        foreach (TimeOfDay candidate in All)
        {
            if (string.CompareOrdinal(NameOf(candidate), name) == 0)
            {
                time = candidate;
                return true;
            }
        }

        time = TimeOfDay.Day;
        return false;
    }

    /// <summary>Gives the name of one time, which a map file and an error use (T-2).</summary>
    /// <param name="time">The time.</param>
    /// <returns>The name, such as `dusk`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no time (T-2).</exception>
    public static string NameOf(TimeOfDay time) => time switch
    {
        TimeOfDay.Dawn => "dawn",
        TimeOfDay.Day => "day",
        TimeOfDay.Dusk => "dusk",
        TimeOfDay.Night => "night",
        _ => throw new ArgumentOutOfRangeException(nameof(time), time, "the value names no time of day (D-442)"),
    };
}
