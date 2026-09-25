using System;

namespace TheThingBelow.Core.Maps;

/// <summary>The four directions of one step, and of the facing that carries sight (D-716).</summary>
/// <remarks>
/// A step goes north, south, east, or west, and a diagonal walk takes two steps (D-716).
/// Thus every step costs the same count of ticks, and no corner rule is necessary.
/// <para>
/// The same four values give the facing of a patrol, which carries its sight (D-208, D-718).
/// PR-8 walks each patrol and reads that facing.
/// </para>
/// </remarks>
public enum StepDirection
{
    /// <summary>Up the screen, which lowers the row.</summary>
    North,

    /// <summary>Down the screen, which raises the row.</summary>
    South,

    /// <summary>To the right of the screen, which raises the column.</summary>
    East,

    /// <summary>To the left of the screen, which lowers the column.</summary>
    West,
}

/// <summary>The names and the count of the step directions (D-716).</summary>
public static class StepDirections
{
    /// <summary>Every direction, in one fixed order for a walk of them (G-4).</summary>
    public static readonly StepDirection[] All =
    [
        StepDirection.North,
        StepDirection.South,
        StepDirection.East,
        StepDirection.West,
    ];

    /// <summary>The names of every direction, for the error of an unknown name (T-2).</summary>
    public const string EveryName = "north, south, east, west";

    /// <summary>Gives the direction of one name, as a map file writes a facing (D-716).</summary>
    /// <param name="name">The name, such as `north`.</param>
    /// <param name="direction">The direction of that name, when the name names one.</param>
    /// <returns>True when the name names a direction.</returns>
    /// <exception cref="ArgumentNullException">The name is null (T-2).</exception>
    public static bool TryOf(string name, out StepDirection direction)
    {
        ArgumentNullException.ThrowIfNull(name);

        foreach (StepDirection candidate in All)
        {
            if (string.CompareOrdinal(NameOf(candidate), name) == 0)
            {
                direction = candidate;
                return true;
            }
        }

        direction = StepDirection.North;
        return false;
    }

    /// <summary>Gives the name of one direction, for an error and for a log field (T-2).</summary>
    /// <param name="direction">The direction.</param>
    /// <returns>The name, such as `north`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no direction (T-2).</exception>
    public static string NameOf(StepDirection direction) => direction switch
    {
        StepDirection.North => "north",
        StepDirection.South => "south",
        StepDirection.East => "east",
        StepDirection.West => "west",
        _ => throw new ArgumentOutOfRangeException(
            nameof(direction),
            direction,
            "the value names no step direction (D-716)"),
    };
}
