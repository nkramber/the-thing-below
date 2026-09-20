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
