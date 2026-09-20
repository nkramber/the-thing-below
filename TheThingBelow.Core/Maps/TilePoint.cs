using System;

namespace TheThingBelow.Core.Maps;

/// <summary>One tile of a map, by its column and its row (D-106).</summary>
/// <remarks>
/// Core keeps every position on a whole tile, and Game slides a sprite between two tiles
/// across the ticks of one step (D-203). Thus no position that a rule reads holds a part of
/// a tile, and a replay lands on the same tile on every platform (T-7).
/// </remarks>
/// <param name="X">The column, which counts from zero at the west edge.</param>
/// <param name="Y">The row, which counts from zero at the north edge.</param>
public readonly record struct TilePoint(int X, int Y)
{
    /// <summary>Gives the tile one step away in one direction (D-716).</summary>
    /// <param name="direction">The direction of the step.</param>
    /// <returns>The tile that the step reaches, which can lie outside the map.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no direction (T-2).</exception>
    /// <exception cref="OverflowException">The column or the row passes the range of an `int` (T-2).</exception>
    public TilePoint Step(StepDirection direction) => direction switch
    {
        StepDirection.North => new TilePoint(this.X, checked(this.Y - 1)),
        StepDirection.South => new TilePoint(this.X, checked(this.Y + 1)),
        StepDirection.East => new TilePoint(checked(this.X + 1), this.Y),
        StepDirection.West => new TilePoint(checked(this.X - 1), this.Y),
        _ => throw new ArgumentOutOfRangeException(
            nameof(direction),
            direction,
            "the value names no step direction (D-716)"),
    };

    /// <summary>The text of the tile for an error message and for a log field (T-2).</summary>
    /// <returns>The column and the row, such as `(3, 7)`.</returns>
    public override string ToString() => $"({this.X}, {this.Y})";
}
