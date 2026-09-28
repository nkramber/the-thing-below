using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Tools.Import;

/// <summary>
/// Finds the color of the palette for a pixel of a PNG. The hand-edit mode takes an exact
/// color alone, and the generator mode takes the nearest color (D-688, D-1314).
/// </summary>
/// <remarks>
/// Every value is a whole number, so every CI leg picks the same color (D-502).
/// </remarks>
public sealed class PaletteMatch
{
    private readonly Palette palette;
    private readonly SortedDictionary<int, PaletteColor> byRgb = [];

    /// <summary>Makes the match for one palette.</summary>
    /// <param name="palette">The palette of the checkout.</param>
    /// <exception cref="ImportException">Two colors of the palette hold the same RGB value (T-2).</exception>
    public PaletteMatch(Palette palette)
    {
        ArgumentNullException.ThrowIfNull(palette);

        this.palette = palette;
        foreach (PaletteColor color in palette.Colors)
        {
            // Two keys of one color make the key of an exact match a guess, so the palette
            // fails and the import writes nothing (T-2).
            int rgb = Pack(color.Red, color.Green, color.Blue);
            if (!this.byRgb.TryAdd(rgb, color))
            {
                throw ImportException.For(
                    Palette.Path,
                    $"the colors '{this.byRgb[rgb].Key}' and '{color.Key}' both hold {color.Hex}, so an exact match has two keys");
            }
        }
    }

    /// <summary>Finds the color of the palette that holds exactly this RGB value.</summary>
    /// <param name="red">The red part, from 0 to 255.</param>
    /// <param name="green">The green part, from 0 to 255.</param>
    /// <param name="blue">The blue part, from 0 to 255.</param>
    /// <param name="color">The color of the palette, or null when no color matches.</param>
    /// <returns>True when a color of the palette matches.</returns>
    public bool TryExact(int red, int green, int blue, [NotNullWhen(true)] out PaletteColor? color) =>
        this.byRgb.TryGetValue(Pack(red, green, blue), out color);

    /// <summary>
    /// Finds the color of the palette with the smallest squared RGB distance. A tie goes to
    /// the lower index of the palette (D-1314).
    /// </summary>
    /// <param name="red">The red part, from 0 to 255.</param>
    /// <param name="green">The green part, from 0 to 255.</param>
    /// <param name="blue">The blue part, from 0 to 255.</param>
    /// <returns>The nearest color of the palette.</returns>
    public PaletteColor Nearest(int red, int green, int blue)
    {
        // The reader of the palette keeps the colors in the order of their index, so the
        // strict comparison keeps the lower index on a tie.
        PaletteColor best = this.palette.Colors[0];
        int bestDistance = Distance(best, red, green, blue);
        for (int index = 1; index < this.palette.Colors.Count; index += 1)
        {
            PaletteColor color = this.palette.Colors[index];
            int distance = Distance(color, red, green, blue);
            if (distance < bestDistance)
            {
                best = color;
                bestDistance = distance;
            }
        }

        return best;
    }

    private static int Distance(PaletteColor color, int red, int green, int blue)
    {
        int dr = color.Red - red;
        int dg = color.Green - green;
        int db = color.Blue - blue;
        return (dr * dr) + (dg * dg) + (db * db);
    }

    private static int Pack(int red, int green, int blue) => (red << 16) | (green << 8) | blue;
}
