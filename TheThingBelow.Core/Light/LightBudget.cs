using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Light;

/// <summary>The highest count of lights in one window, and the place of that window.</summary>
/// <param name="Left">The column of the west edge of the window, in art pixels of the map.</param>
/// <param name="Top">The row of the north edge of the window, in art pixels of the map.</param>
/// <param name="Count">The count of lights whose reach meets the window.</param>
public sealed record LightCount(int Left, int Top, int Count);

/// <summary>
/// The count of lights that the budget test reads: the worst view of the Deck, and the lights
/// on one canvas item (D-523, D-842, F-46).
/// </summary>
/// <remarks>
/// A light reaches a window when the square of its range meets the window. Godot culls a light
/// by that square too, so the count matches the lights that the engine gives each canvas item.
/// The load gives each torch the range of the widest step of its fire, so the count holds each
/// step (D-891).
/// <para>
/// The carried light follows the lead, so it can stand in any view and over any canvas item.
/// Each count adds it (D-847).
/// </para>
/// </remarks>
public static class LightBudget
{
    /// <summary>The width of one view, in art pixels: the frame of 1280 by 720 at 2x (D-568, D-634).</summary>
    public const int ViewWidth = 640;

    /// <summary>The height of one view, in art pixels (D-568, D-634).</summary>
    public const int ViewHeight = 360;

    /// <summary>
    /// The side of one quadrant of the ground layer, in tiles. Godot draws a quadrant as one
    /// canvas item, and Game sets this size on the layer (F-46).
    /// </summary>
    public const int QuadrantTiles = 16;

    /// <summary>
    /// The side of the largest sprite of a map, in art pixels: a body of three tiles with a
    /// picture one tile taller (D-206). Each sprite is one canvas item.
    /// </summary>
    public const int LargestSprite = 4 * AtlasPages.TileSize;

    /// <summary>Gives the window of one size over the map that meets the most lights.</summary>
    /// <param name="lights">The fixed lights of the map.</param>
    /// <param name="mapWidth">The width of the map, in art pixels.</param>
    /// <param name="mapHeight">The height of the map, in art pixels.</param>
    /// <param name="width">The width of the window, in art pixels.</param>
    /// <param name="height">The height of the window, in art pixels.</param>
    /// <returns>The place of one window with the highest count, and that count.</returns>
    /// <exception cref="ArgumentNullException">The list is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">A size is less than 1 (T-2).</exception>
    /// <remarks>
    /// The count over the edge of a window changes only where the window passes the reach of a
    /// light. Thus the highest count lies at a window whose west edge meets the east reach of a
    /// light, or at an edge of the map, and the same holds for the rows. The method tries those
    /// places alone, so a large map costs no walk of each pixel.
    /// </remarks>
    public static LightCount WorstWindow(IReadOnlyList<MapLight> lights, int mapWidth, int mapHeight, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(lights);
        ArgumentOutOfRangeException.ThrowIfLessThan(mapWidth, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(mapHeight, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);

        List<int> lefts = Candidates(lights, mapWidth, width, light => light.X);
        List<int> tops = Candidates(lights, mapHeight, height, light => light.Y);

        var worst = new LightCount(0, 0, 0);
        foreach (int left in lefts)
        {
            foreach (int top in tops)
            {
                int count = CountReaching(lights, left, top, width, height);
                if (count > worst.Count)
                {
                    worst = new LightCount(left, top, count);
                }
            }
        }

        return worst;
    }

    /// <summary>Gives the quadrant of the ground layer that meets the most lights (F-46).</summary>
    /// <param name="lights">The fixed lights of the map.</param>
    /// <param name="mapWidth">The width of the map, in art pixels.</param>
    /// <param name="mapHeight">The height of the map, in art pixels.</param>
    /// <returns>The place of one quadrant with the highest count, and that count.</returns>
    /// <exception cref="ArgumentNullException">The list is null (T-2).</exception>
    public static LightCount WorstQuadrant(IReadOnlyList<MapLight> lights, int mapWidth, int mapHeight)
    {
        ArgumentNullException.ThrowIfNull(lights);

        const int Side = QuadrantTiles * AtlasPages.TileSize;
        var worst = new LightCount(0, 0, 0);
        for (int top = 0; top < mapHeight; top += Side)
        {
            for (int left = 0; left < mapWidth; left += Side)
            {
                int count = CountReaching(lights, left, top, Side, Side);
                if (count > worst.Count)
                {
                    worst = new LightCount(left, top, count);
                }
            }
        }

        return worst;
    }

    /// <summary>Counts the lights whose reach meets one window.</summary>
    /// <param name="lights">The lights.</param>
    /// <param name="left">The column of the west edge of the window.</param>
    /// <param name="top">The row of the north edge of the window.</param>
    /// <param name="width">The width of the window.</param>
    /// <param name="height">The height of the window.</param>
    /// <returns>The count.</returns>
    /// <exception cref="ArgumentNullException">The list is null (T-2).</exception>
    public static int CountReaching(IReadOnlyList<MapLight> lights, int left, int top, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(lights);

        int count = 0;
        foreach (MapLight light in lights)
        {
            int reach = light.Light.Range;
            bool meetsColumns = light.X - reach <= left + width - 1 && light.X + reach >= left;
            bool meetsRows = light.Y - reach <= top + height - 1 && light.Y + reach >= top;
            if (meetsColumns && meetsRows)
            {
                count += 1;
            }
        }

        return count;
    }

    private static List<int> Candidates(IReadOnlyList<MapLight> lights, int mapSize, int windowSize, Func<MapLight, int> center)
    {
        // A map no larger than the window shows whole, and Game centers it (F-52).
        if (mapSize <= windowSize)
        {
            var whole = new List<int>();
            whole.Add(0);
            return whole;
        }

        int last = mapSize - windowSize;
        var places = new List<int>();
        places.Add(0);
        places.Add(last);
        foreach (MapLight light in lights)
        {
            int eastReach = checked(center(light) + light.Light.Range);
            places.Add(Math.Clamp(eastReach, 0, last));
        }

        return places;
    }
}
