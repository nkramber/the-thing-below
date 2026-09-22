using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Light;

/// <summary>
/// The shadow shape of one wall tile: a rectangle inside the tile, in art pixels from the
/// north-west corner of the tile (D-852).
/// </summary>
/// <param name="Tile">The wall tile.</param>
/// <param name="Left">The west edge of the shape, from 0 to the tile size.</param>
/// <param name="Top">The north edge of the shape.</param>
/// <param name="Right">The east edge of the shape.</param>
/// <param name="Bottom">The south edge of the shape.</param>
public sealed record WallShadow(TilePoint Tile, int Left, int Top, int Right, int Bottom);

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
            return corner ? new WallShadow(tile, 0, 0, Size, Size) : null;
        }

        (int left, int right) = Span(west, EdgeDepth, east, EdgeDepth);
        (int top, int bottom) = Span(north, EdgeDepth, south, FrontDepth);
        return new WallShadow(tile, left, top, right, bottom);
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

    private static bool Walkable(GameMap map, TilePoint tile, int dx, int dy)
    {
        var next = new TilePoint(checked(tile.X + dx), checked(tile.Y + dy));
        return map.Holds(next) && TileKinds.CanWalk(map.TileAt(next));
    }
}
