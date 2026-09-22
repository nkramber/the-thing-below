using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Effects;

/// <summary>One pair of colors under a fog that fails the contrast test (D-886).</summary>
/// <param name="Outline">The palette key of the outline of the enemy.</param>
/// <param name="Floor">The palette key of the floor.</param>
/// <param name="Gap">The luma gap of the two colors under the fog, from 0 to 255.</param>
public sealed record FogFault(char Outline, char Floor, int Gap);

/// <summary>
/// The contrast test of the fog (D-885, D-886, D-892): the strongest band of a fog over each
/// outline color of an enemy, and over each floor color of the map. For each floor color, the
/// outline key with the largest luma gap must keep a gap of <see cref="LeastGap"/> under the
/// fog, so fog never hides an enemy that the player must see (D-37, D-187).
/// </summary>
/// <remarks>
/// The test counts the loss that the fog makes (D-892). A floor color whose gap is below
/// <see cref="LeastGap"/> with no fog takes no test, because the art sets that gap, and not the
/// fog. A fog over the enemy and over the floor moves both colors toward the color of the fog,
/// so it shrinks each gap by the same part, and the color of the fog never changes the result.
/// <para>
/// The luma is the one of Rec. 601, from 0 to 255, in integer math (T-7). Luma ignores hue, so
/// a red enemy on a green floor scores lower than it looks (D-886). An outline pixel is a
/// pixel of a drawing next to a transparent pixel or to the edge, because the outline of each
/// material is its dark shade (D-201).
/// </para>
/// </remarks>
public static class FogContrast
{
    /// <summary>The least luma gap of an outline color and a floor color under a fog: the first floor of D-886, which the owner reads on the Deck.</summary>
    public const int LeastGap = 24;

    /// <summary>Gives the Rec. 601 luma of a color, from 0 to 255, rounded to the nearest whole number.</summary>
    /// <param name="red">The red part, from 0 to 255.</param>
    /// <param name="green">The green part, from 0 to 255.</param>
    /// <param name="blue">The blue part, from 0 to 255.</param>
    /// <returns>The luma.</returns>
    public static int Luma(int red, int green, int blue) =>
        checked((299 * red) + (587 * green) + (114 * blue) + 500) / 1000;

    /// <summary>Gives one part of a color under a fog: the part of the art moves toward the part of the fog by the strength.</summary>
    /// <param name="art">The part of the art, from 0 to 255.</param>
    /// <param name="fog">The part of the fog, from 0 to 255.</param>
    /// <param name="strength">The strength of the fog, in basis points (D-169).</param>
    /// <returns>The part under the fog, rounded to the nearest whole number.</returns>
    public static int Blend(int art, int fog, int strength) =>
        checked((art * (10000 - strength)) + (fog * strength) + 5000) / 10000;

    /// <summary>Gives the luma of one palette color under a fog.</summary>
    /// <param name="art">The color of the art.</param>
    /// <param name="fog">The color of the fog.</param>
    /// <param name="strength">The strength of the fog, in basis points.</param>
    /// <returns>The luma under the fog.</returns>
    /// <exception cref="ArgumentNullException">A color is null (T-2).</exception>
    public static int LumaUnder(PaletteColor art, PaletteColor fog, int strength)
    {
        ArgumentNullException.ThrowIfNull(art);
        ArgumentNullException.ThrowIfNull(fog);

        return Luma(
            Blend(art.Red, fog.Red, strength),
            Blend(art.Green, fog.Green, strength),
            Blend(art.Blue, fog.Blue, strength));
    }

    /// <summary>Gives the palette keys of the outline of a drawing: each key next to a transparent pixel or to the edge, in any frame.</summary>
    /// <param name="drawing">The drawing.</param>
    /// <returns>The keys, in the order of the key.</returns>
    /// <exception cref="ArgumentNullException">The drawing is null (T-2).</exception>
    public static SortedSet<char> OutlineOf(Drawing drawing)
    {
        ArgumentNullException.ThrowIfNull(drawing);

        var keys = new SortedSet<char>();
        foreach (DrawingFrame frame in drawing.Frames)
        {
            for (int row = 0; row < frame.Rows.Count; row += 1)
            {
                for (int column = 0; column < frame.Rows[row].Length; column += 1)
                {
                    char key = frame.Rows[row][column];
                    if (key != Drawing.Transparent && BesideClear(frame.Rows, column, row))
                    {
                        keys.Add(key);
                    }
                }
            }
        }

        return keys;
    }

    /// <summary>Gives every palette key that a drawing holds, in any frame.</summary>
    /// <param name="drawing">The drawing.</param>
    /// <returns>The keys, in the order of the key.</returns>
    /// <exception cref="ArgumentNullException">The drawing is null (T-2).</exception>
    public static SortedSet<char> KeysOf(Drawing drawing)
    {
        ArgumentNullException.ThrowIfNull(drawing);

        var keys = new SortedSet<char>();
        foreach (DrawingFrame frame in drawing.Frames)
        {
            foreach (string row in frame.Rows)
            {
                foreach (char key in row)
                {
                    if (key != Drawing.Transparent)
                    {
                        keys.Add(key);
                    }
                }
            }
        }

        return keys;
    }

    /// <summary>Gives the first floor key that the fog pulls below <see cref="LeastGap"/> (D-892).</summary>
    /// <param name="fog">The layer of fog, at its strongest band.</param>
    /// <param name="outline">The outline keys of the enemy.</param>
    /// <param name="floor">The keys of the floor.</param>
    /// <param name="palette">The palette (D-181).</param>
    /// <returns>
    /// The first floor key, in the order of the keys, whose best outline key holds a gap of
    /// <see cref="LeastGap"/> or more with no fog and a smaller gap under the fog. No value when
    /// each floor key passes, or when the art alone holds the key below the floor of the gap.
    /// </returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">The palette holds no key of the fog, the outline, or the floor (T-2).</exception>
    public static FogFault? FirstFault(FogLayer fog, SortedSet<char> outline, SortedSet<char> floor, Palette palette)
    {
        ArgumentNullException.ThrowIfNull(fog);
        ArgumentNullException.ThrowIfNull(outline);
        ArgumentNullException.ThrowIfNull(floor);
        ArgumentNullException.ThrowIfNull(palette);

        PaletteColor fogColor = ColorOf(palette, fog.Key);
        int strength = fog.Strongest;
        foreach (char ground in floor)
        {
            PaletteColor groundColor = ColorOf(palette, ground);
            int clearLuma = Luma(groundColor.Red, groundColor.Green, groundColor.Blue);
            int foggedLuma = LumaUnder(groundColor, fogColor, strength);
            char best = ' ';
            int clearGap = 0;
            int foggedGap = 0;
            foreach (char edge in outline)
            {
                PaletteColor edgeColor = ColorOf(palette, edge);
                int gap = Math.Abs(Luma(edgeColor.Red, edgeColor.Green, edgeColor.Blue) - clearLuma);
                if (gap > clearGap)
                {
                    best = edge;
                    clearGap = gap;
                    foggedGap = Math.Abs(LumaUnder(edgeColor, fogColor, strength) - foggedLuma);
                }
            }

            if (clearGap >= LeastGap && foggedGap < LeastGap)
            {
                return new FogFault(best, ground, foggedGap);
            }
        }

        return null;
    }

    private static bool BesideClear(IReadOnlyList<string> rows, int column, int row)
    {
        return IsClear(rows, column - 1, row) || IsClear(rows, column + 1, row)
            || IsClear(rows, column, row - 1) || IsClear(rows, column, row + 1);
    }

    private static bool IsClear(IReadOnlyList<string> rows, int column, int row)
    {
        bool outside = row < 0 || row >= rows.Count || column < 0 || column >= rows[row].Length;
        return outside || rows[row][column] == Drawing.Transparent;
    }

    private static PaletteColor ColorOf(Palette palette, char key)
    {
        return palette.TryColorOf(key, out PaletteColor? found)
            ? found
            : throw ContentException.ForFile(Palette.Path, $"the palette holds no key '{key}', and the fog test reads each key (D-181, D-886)");
    }
}
