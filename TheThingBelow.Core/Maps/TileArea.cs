using System;

namespace TheThingBelow.Core.Maps;

/// <summary>
/// A rectangle of tiles on one map. A large enemy keeps its place inside one area, and it
/// never leaves it (D-209, D-741).
/// </summary>
/// <remarks>
/// The area holds the anchor tile of a body, and not the body itself. Thus the load of a map
/// proves that the body fits at every anchor tile of the area, and the walk of the enemy
/// needs one test for each step (D-209, D-741, T-2).
/// </remarks>
/// <param name="X">The column of the north-west tile, which counts from zero.</param>
/// <param name="Y">The row of the north-west tile, which counts from zero.</param>
/// <param name="Width">The count of columns, which is one or more.</param>
/// <param name="Height">The count of rows, which is one or more.</param>
public readonly record struct TileArea(int X, int Y, int Width, int Height)
{
    /// <summary>The north-west tile of the area, where an enemy of the area starts (D-741).</summary>
    public TilePoint Anchor => new(this.X, this.Y);

    /// <summary>Tells whether the area holds one tile.</summary>
    /// <param name="at">The tile.</param>
    /// <returns>True when that tile lies inside the area.</returns>
    public bool Holds(TilePoint at) =>
        at.X >= this.X && at.X < checked(this.X + this.Width) &&
        at.Y >= this.Y && at.Y < checked(this.Y + this.Height);

    /// <summary>Tells whether the area holds every tile of one body (D-209, D-741).</summary>
    /// <param name="body">The body of an enemy.</param>
    /// <returns>True when the whole body lies inside the area.</returns>
    /// <exception cref="OverflowException">A count passes the range of an `int` (T-2).</exception>
    public bool HoldsBody(EnemyBody body)
    {
        int side = body.Side;
        return body.Anchor.X >= this.X && checked(body.Anchor.X + side) <= checked(this.X + this.Width) &&
            body.Anchor.Y >= this.Y && checked(body.Anchor.Y + side) <= checked(this.Y + this.Height);
    }

    /// <summary>Tells whether every value of the area describes a rectangle of tiles (T-2).</summary>
    /// <returns>True when the area starts inside a map and holds at least one tile.</returns>
    public bool IsRectangle() => this.X >= 0 && this.Y >= 0 && this.Width >= 1 && this.Height >= 1;

    /// <summary>The text of the area for an error message and for a log field (T-2).</summary>
    /// <returns>The tile and the size, such as `(3, 7) by 4 by 5`.</returns>
    public override string ToString() => $"{this.Anchor} by {this.Width} by {this.Height}";
}
