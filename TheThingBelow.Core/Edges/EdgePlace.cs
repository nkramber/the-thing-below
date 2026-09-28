using System;
using System.Collections.Generic;

namespace TheThingBelow.Core.Edges;

/// <summary>The place of one edge piece on its tile: a side, or an inner corner (D-1321).</summary>
/// <remarks>
/// The order of the values is the order of the pieces of one tile in an edge file, and the
/// order of the draw. A later piece draws over an earlier one.
/// </remarks>
public enum EdgePlace
{
    /// <summary>The north side. It draws where the neighbour to the north does not join the kind.</summary>
    North,

    /// <summary>The east side.</summary>
    East,

    /// <summary>The south side.</summary>
    South,

    /// <summary>The west side.</summary>
    West,

    /// <summary>The inner corner to the north-east. It draws where the north and the east join and the north-east does not.</summary>
    NorthEast,

    /// <summary>The inner corner to the south-east.</summary>
    SouthEast,

    /// <summary>The inner corner to the south-west.</summary>
    SouthWest,

    /// <summary>The inner corner to the north-west.</summary>
    NorthWest,
}

/// <summary>The name and the order of each edge place (D-1321).</summary>
public static class EdgePlaces
{
    /// <summary>Every place, in the order of the draw.</summary>
    public static IReadOnlyList<EdgePlace> All { get; } =
    [
        EdgePlace.North,
        EdgePlace.East,
        EdgePlace.South,
        EdgePlace.West,
        EdgePlace.NorthEast,
        EdgePlace.SouthEast,
        EdgePlace.SouthWest,
        EdgePlace.NorthWest,
    ];

    /// <summary>Gives the name of one place, as the `pieces` object of an edge rule writes it.</summary>
    /// <param name="place">The place.</param>
    /// <returns>The name, such as `north_east`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no place (T-2).</exception>
    public static string NameOf(EdgePlace place) => place switch
    {
        EdgePlace.North => "north",
        EdgePlace.East => "east",
        EdgePlace.South => "south",
        EdgePlace.West => "west",
        EdgePlace.NorthEast => "north_east",
        EdgePlace.SouthEast => "south_east",
        EdgePlace.SouthWest => "south_west",
        EdgePlace.NorthWest => "north_west",
        _ => throw new ArgumentOutOfRangeException(nameof(place), place, "the value names no edge place (D-1321)"),
    };
}
