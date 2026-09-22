using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Light;

/// <summary>The side of a wall tile that takes a column down to the south edge of the tile.</summary>
public enum WallColumn
{
    /// <summary>The shape is the rectangle alone.</summary>
    None,

    /// <summary>A column at the west edge of the rectangle, for a floor to the west and to the south.</summary>
    West,

    /// <summary>A column at the east edge of the rectangle, for a floor to the east and to the south.</summary>
    East,
}

/// <summary>
/// The shadow shape of one wall tile, in art pixels from the north-west corner of the tile
/// (D-852). The shape is a rectangle, or an L of the rectangle and a column of
/// <see cref="WallShadows.LeastStrip"/> pixels from its south edge down to the south edge of the tile.
/// </summary>
/// <param name="Tile">The wall tile.</param>
/// <param name="Left">The west edge of the rectangle, from 0 to the tile size.</param>
/// <param name="Top">The north edge of the rectangle.</param>
/// <param name="Right">The east edge of the rectangle.</param>
/// <param name="Bottom">The south edge of the rectangle.</param>
/// <param name="Column">The side of the column, or no column.</param>
public sealed record WallShadow(TilePoint Tile, int Left, int Top, int Right, int Bottom, WallColumn Column)
{
    /// <summary>Gives the corners of the shape, clockwise from the north-west corner of the rectangle.</summary>
    /// <returns>Four corners for a rectangle, or six for an L.</returns>
    /// <exception cref="InvalidOperationException">The column holds a value outside the enum (T-2).</exception>
    public IReadOnlyList<(int X, int Y)> Outline()
    {
        const int Size = AtlasPages.TileSize;
        const int Strip = WallShadows.LeastStrip;
        return this.Column switch
        {
            WallColumn.None => [(this.Left, this.Top), (this.Right, this.Top), (this.Right, this.Bottom), (this.Left, this.Bottom)],
            WallColumn.West =>
            [
                (this.Left, this.Top), (this.Right, this.Top), (this.Right, this.Bottom),
                (this.Left + Strip, this.Bottom), (this.Left + Strip, Size), (this.Left, Size),
            ],
            WallColumn.East =>
            [
                (this.Left, this.Top), (this.Right, this.Top), (this.Right, Size),
                (this.Right - Strip, Size), (this.Right - Strip, this.Bottom), (this.Left, this.Bottom),
            ],
            _ => throw new InvalidOperationException($"The wall at {this.Tile} holds the column value {(int)this.Column}, which is not a side (T-2)."),
        };
    }
}

/// <summary>
/// Gives the shadow shape of each wall of a map, from its terrain alone (D-845, D-852). Game
/// builds one occluder from each shape, and no content file names one.
/// </summary>
/// <remarks>
/// A wall that faces a walkable tile takes light on its face, so its shape leaves out the
/// face on each side that faces one. The view looks down at an angle, so the face to the south
/// shows the height of the wall and takes <see cref="FrontDepth"/> pixels. A face to the east,
/// the west, or the north shows the edge of the wall alone and takes <see cref="EdgeDepth"/>. The rest of the tile blocks the
/// light, so no light reaches the tiles behind it. A wall of one tile, with a walkable tile on
/// two opposite sides, keeps a strip of <see cref="LeastStrip"/> pixels in its middle.
/// <para>
/// A wall that touches a walkable tile at a corner alone takes the full tile, so no light
/// passes the gap between two faces into the wall mass. A wall inside a wall mass takes no
/// shape, because the faces around it already stop each light.
/// </para>
/// <para>
/// A wall beside a doorway has a floor to its south and to one side. Its rectangle then stops
/// the light in a band at its top alone, and a light below that band shines past the wall into
/// the passage behind the doorway. Thus the shape takes a column of <see cref="LeastStrip"/>
/// pixels at the edge of the rectangle on that side, down to the south edge of the tile. The
/// front face and the side face stay out of the shape.
/// </para>
/// </remarks>
public static class WallShadows
{
    /// <summary>The depth of the south face of a wall, which shows its height, in art pixels (D-852).</summary>
    public const int FrontDepth = 24;

    /// <summary>The depth of a face to the east, the west, or the north, in art pixels (D-852).</summary>
    public const int EdgeDepth = 8;

    /// <summary>The least width of the strip that blocks the light in a wall of one tile, in art pixels (D-852).</summary>
    public const int LeastStrip = 2;

    /// <summary>Gives the shadow shape of each wall of a map, row by row from the north-west tile.</summary>
    /// <param name="map">The map.</param>
    /// <returns>One shape for each wall that needs one.</returns>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    public static IReadOnlyList<WallShadow> Of(GameMap map)
    {
        ArgumentNullException.ThrowIfNull(map);

        var shapes = new List<WallShadow>();
        for (int row = 0; row < map.Height; row += 1)
        {
            for (int column = 0; column < map.Width; column += 1)
            {
                var tile = new TilePoint(column, row);
                if (map.TileAt(tile) == TileKind.Wall && ShapeOf(map, tile) is WallShadow shape)
                {
                    shapes.Add(shape);
                }
            }
        }

        return shapes;
    }

    /// <summary>Gives the shadow shape of one wall tile, or no value for a wall inside a wall mass.</summary>
    /// <param name="map">The map.</param>
    /// <param name="tile">A wall tile of the map.</param>
    /// <returns>The shape, or no value when no walkable tile touches the wall.</returns>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    /// <exception cref="ArgumentException">The tile is outside the map, or it is not a wall (T-2).</exception>
    public static WallShadow? ShapeOf(GameMap map, TilePoint tile)
    {
        ArgumentNullException.ThrowIfNull(map);
        if (!map.Holds(tile) || map.TileAt(tile) != TileKind.Wall)
        {
            throw new ArgumentException($"The tile {tile} of the map '{map.Id.Value}' is not a wall (T-2, D-852).", nameof(tile));
        }

        bool west = Walkable(map, tile, -1, 0);
        bool east = Walkable(map, tile, 1, 0);
        bool north = Walkable(map, tile, 0, -1);
        bool south = Walkable(map, tile, 0, 1);
        const int Size = AtlasPages.TileSize;

        if (!west && !east && !north && !south)
        {
            bool corner = Walkable(map, tile, -1, -1) || Walkable(map, tile, 1, -1)
                || Walkable(map, tile, -1, 1) || Walkable(map, tile, 1, 1);
            return corner ? new WallShadow(tile, 0, 0, Size, Size, WallColumn.None) : null;
        }

        (int left, int right) = Span(west, EdgeDepth, east, EdgeDepth);
        (int top, int bottom) = Span(north, EdgeDepth, south, FrontDepth);
        return new WallShadow(tile, left, top, right, bottom, ColumnOf(west, east, south));
    }

    /// <summary>Gives the edges of the shape along one axis, from the two faces of that axis.</summary>
    private static (int Start, int End) Span(bool faceAtStart, int startDepth, bool faceAtEnd, int endDepth)
    {
        const int Size = AtlasPages.TileSize;
        if (faceAtStart && faceAtEnd)
        {
            // A wall of one tile keeps a strip in its middle, so it still stops the light.
            int middle = Size / 2;
            return (middle - (LeastStrip / 2), middle + (LeastStrip / 2));
        }

        return (faceAtStart ? startDepth : 0, faceAtEnd ? Size - endDepth : Size);
    }

    /// <summary>Gives the side of the column: the one side face of a wall with a floor to its south.</summary>
    private static WallColumn ColumnOf(bool west, bool east, bool south)
    {
        if (!south || west == east)
        {
            return WallColumn.None;
        }

        return west ? WallColumn.West : WallColumn.East;
    }

    private static bool Walkable(GameMap map, TilePoint tile, int dx, int dy)
    {
        var next = new TilePoint(checked(tile.X + dx), checked(tile.Y + dy));
        return map.Holds(next) && TileKinds.CanWalk(map.TileAt(next));
    }
}
