using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Light;

/// <summary>The brightest light that can fall on art in one place, and the tile where it falls.</summary>
/// <param name="Column">The column of the tile, from the west edge of the map.</param>
/// <param name="Row">The row of the tile, from the north edge of the map.</param>
/// <param name="Level">The light, in basis points of linear light, where 10000 lights full white art to full white.</param>
public sealed record LitPeak(int Column, int Row, int Level);

/// <summary>
/// Gives an upper bound of the lit art of a map and of a fight, which the load keeps below the
/// glow threshold, so a sprite or a tile never glows (D-910, F-47).
/// </summary>
/// <remarks>
/// Godot adds each light to a pixel with no upper clamp (F-47). Thus the bound adds the light of
/// the ambient light, of the carried light, and of each point light that can reach the pixel.
/// Each step of the bound can only give more light than Godot gives:
/// <list type="bullet">
/// <item>The art is full white. A lit pixel is its art color times the light, so no art color gives more.</item>
/// <item>Each light color counts its brightest channel, in sRGB. The linear value of a channel is never above its sRGB value.</item>
/// <item>Each light takes the strongest level and the widest range of every fire of the build (D-891).</item>
/// <item>The carried light can stand at any place, so each tile takes its full strength (D-847).</item>
/// <item>A light falls to its edge along a straight line, which is never below the curve of the light texture of Game.</item>
/// <item>The distance to a light is the largest of the column and the row distance, which is never above the true distance.</item>
/// <item>No shadow and no normal map takes light away.</item>
/// </list>
/// The bound reads the middle of each tile, and it moves each light half a tile closer, so the
/// light of each pixel of the tile stays inside it.
/// </remarks>
public static class BrightestLight
{
    /// <summary>The full value of a color channel in sRGB.</summary>
    public const int FullChannel = 255;

    /// <summary>
    /// Gives the brightest light that can fall on art of a map, and the tile where it falls.
    /// </summary>
    /// <param name="lights">The fixed lights of the map.</param>
    /// <param name="ambient">The ambient light of the map.</param>
    /// <param name="carried">The carried light.</param>
    /// <param name="brightest">The strongest strength and the widest range of every fire of the build.</param>
    /// <param name="palette">The palette, which gives each light its color (D-846).</param>
    /// <param name="columns">The width of the map, in tiles.</param>
    /// <param name="rows">The height of the map, in tiles.</param>
    /// <returns>The tile with the brightest bound, and that bound.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The map holds no tile (T-2).</exception>
    /// <exception cref="ContentException">The palette holds no key of a light (T-2).</exception>
    public static LitPeak OnMap(
        IReadOnlyList<MapLight> lights,
        LightColor ambient,
        PointLightValues carried,
        FlickerLevel brightest,
        Palette palette,
        int columns,
        int rows)
    {
        ArgumentNullException.ThrowIfNull(lights);
        ArgumentNullException.ThrowIfNull(ambient);
        ArgumentNullException.ThrowIfNull(carried);
        ArgumentNullException.ThrowIfNull(brightest);
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentOutOfRangeException.ThrowIfLessThan(columns, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(rows, 1);

        long everywhere = checked(Level(ambient, palette, BasisPoints.One) + Level(carried.Color, palette, brightest.Strength));
        var peak = new LitPeak(0, 0, -1);
        for (int row = 0; row < rows; row += 1)
        {
            for (int column = 0; column < columns; column += 1)
            {
                int x = checked((column * AtlasPages.TileSize) + (AtlasPages.TileSize / 2));
                int y = checked((row * AtlasPages.TileSize) + (AtlasPages.TileSize / 2));
                long level = everywhere;
                foreach (MapLight light in lights)
                {
                    level = checked(level + LevelNear(light, x, y, brightest, palette));
                }

                if (level > peak.Level)
                {
                    peak = new LitPeak(column, row, checked((int)level));
                }
            }
        }

        return peak;
    }

    /// <summary>Gives the brightest light that can fall on art of a fight: the ambient light and the key light at full strength (D-850).</summary>
    /// <param name="ambient">The ambient light of the map where the fight began.</param>
    /// <param name="key">The key light of the fight.</param>
    /// <param name="palette">The palette, which gives each light its color (D-846).</param>
    /// <returns>The bound, in basis points of linear light.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">The palette holds no key of a light (T-2).</exception>
    public static int InFight(LightColor ambient, PointLightValues key, Palette palette)
    {
        ArgumentNullException.ThrowIfNull(ambient);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(palette);

        return checked((int)(Level(ambient, palette, BasisPoints.One) + Level(key.Color, palette, BasisPoints.One)));
    }

    /// <summary>Gives the brightest channel of a palette key, in sRGB from 0 to <see cref="FullChannel"/>.</summary>
    /// <param name="key">The palette key.</param>
    /// <param name="palette">The palette.</param>
    /// <returns>The brightest channel.</returns>
    /// <exception cref="ContentException">The palette holds no such key (T-2).</exception>
    public static int BrightestChannelOf(char key, Palette palette)
    {
        ArgumentNullException.ThrowIfNull(palette);

        if (!palette.TryColorOf(key, out PaletteColor? found))
        {
            throw ContentException.ForField(Palette.Path, "colors", $"the palette holds no key '{key}' that a light names (D-846)");
        }

        return Math.Max(found.Red, Math.Max(found.Green, found.Blue));
    }

    /// <summary>Gives the light of one light at full reach, in basis points, rounded up.</summary>
    private static long Level(LightColor color, Palette palette, int part)
    {
        long scaled = checked((long)color.Strength * part * BrightestChannelOf(color.Key, palette));
        return DivideUp(scaled, (long)BasisPoints.One * FullChannel);
    }

    /// <summary>Gives the light of one point light at the tile with its middle at a pixel, rounded up.</summary>
    private static long LevelNear(MapLight light, int x, int y, FlickerLevel brightest, Palette palette)
    {
        long range = DivideUp(checked((long)light.Light.Range * brightest.Range), BasisPoints.One);
        long distance = Math.Max(Math.Abs((long)x - light.X), Math.Abs((long)y - light.Y));
        long reach = Math.Clamp(range - distance + (AtlasPages.TileSize / 2), 0, range);
        if (reach == 0)
        {
            return 0;
        }

        long full = Level(light.Light.Color, palette, brightest.Strength);
        return DivideUp(checked(full * reach), range);
    }

    private static long DivideUp(long value, long divisor) => (value + divisor - 1) / divisor;
}
