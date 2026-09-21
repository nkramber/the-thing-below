using System;

namespace TheThingBelow.Core.Maps;

/// <summary>
/// The tiles that one enemy holds: its anchor tile and its size (D-206, D-737).
/// </summary>
/// <remarks>
/// The anchor is the north-west tile of the body, so the body covers the tiles from the
/// anchor to the east and to the south. A body blocks every tile that it holds, its sort
/// value comes from its front row, and its sight starts at the tile nearest the party
/// (D-737).
/// <para>
/// The body takes no center tile, because an elite holds two by two tiles and no center tile
/// exists in an even square (D-206, D-737).
/// </para>
/// </remarks>
/// <param name="Anchor">The north-west tile of the body.</param>
/// <param name="Size">The count of tiles on one side (D-206).</param>
public readonly record struct EnemyBody(TilePoint Anchor, EnemySize Size)
{
    /// <summary>The count of tiles on one side of the body (D-206).</summary>
    public int Side => EnemySizes.SideOf(this.Size);

    /// <summary>
    /// The row that carries the sort value of the sprite, which is the front row of the body
    /// (D-737). Godot sorts each canvas item by one Y value, so the body draws in front of
    /// what it stands before.
    /// </summary>
    public int SortRow => checked(this.Anchor.Y + this.Side - 1);

    /// <summary>Tells whether the body holds one tile (D-737).</summary>
    /// <param name="at">The tile.</param>
    /// <returns>True when that tile is part of the body.</returns>
    public bool Holds(TilePoint at)
    {
        int side = this.Side;
        return at.X >= this.Anchor.X && at.X < checked(this.Anchor.X + side) &&
            at.Y >= this.Anchor.Y && at.Y < checked(this.Anchor.Y + side);
    }

    /// <summary>
    /// Gives the tile of the body nearest another tile, which is where the sight of the
    /// enemy starts (D-737).
    /// </summary>
    /// <param name="at">The other tile, such as the tile of the lead.</param>
    /// <returns>The tile of the body nearest that tile.</returns>
    /// <remarks>
    /// The body sees where it blocks, so the range floor of D-720 reads from the edge of the
    /// body and the player reads the whole body as the threat.
    /// </remarks>
    public TilePoint Nearest(TilePoint at)
    {
        int side = this.Side;
        return new TilePoint(
            Clamp(at.X, this.Anchor.X, checked(this.Anchor.X + side - 1)),
            Clamp(at.Y, this.Anchor.Y, checked(this.Anchor.Y + side - 1)));
    }

    /// <summary>Gives the body one step away in one direction (D-716).</summary>
    /// <param name="direction">The direction of the step.</param>
    /// <returns>The body at the tile that the step reaches.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no direction (T-2).</exception>
    public EnemyBody Step(StepDirection direction) => new(this.Anchor.Step(direction), this.Size);

    /// <summary>The text of the body for an error message and for a log field (T-2).</summary>
    /// <returns>The anchor tile and the side, such as `(3, 7) by 2`.</returns>
    public override string ToString() => $"{this.Anchor} by {this.Side}";

    private static int Clamp(int value, int lowest, int highest)
    {
        if (value < lowest)
        {
            return lowest;
        }

        return value > highest ? highest : value;
    }
}
