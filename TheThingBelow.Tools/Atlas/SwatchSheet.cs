using System;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Png;

namespace TheThingBelow.Tools.Atlas;

/// <summary>
/// The swatch sheet of the palette (D-185). The session attaches it to the PR description,
/// and the owner approves the colors there (D-514, G-25).
/// </summary>
/// <remarks>
/// The sheet shows each color as a block with its key on it, and its index and its name
/// under it. No swatch sheet enters git (D-514).
/// </remarks>
public static class SwatchSheet
{
    /// <summary>The name of the file that the command writes.</summary>
    public const string FileName = "palette-swatches.png";

    /// <summary>The count of swatches in one row of the sheet.</summary>
    public const int Columns = 8;

    private const int SwatchSize = 64;
    private const int CellWidth = 112;
    private const int CellHeight = SwatchSize + 22;
    private const int Margin = 12;
    private const int TitleHeight = 16;

    /// <summary>Draws the swatch sheet of a palette.</summary>
    /// <param name="palette">The palette to show.</param>
    /// <returns>The image of the sheet.</returns>
    /// <exception cref="ContentException">The palette lacks a color that the sheet uses (T-2).</exception>
    public static PngImage Render(Palette palette)
    {
        ArgumentNullException.ThrowIfNull(palette);

        PaletteColor ground = palette.ColorNamed("slate");
        PaletteColor text = palette.ColorNamed("chalk");
        PaletteColor edge = palette.ColorNamed("ink");

        string title = Title(palette);
        int rows = (palette.Colors.Count + Columns - 1) / Columns;
        int columns = Math.Min(Columns, palette.Colors.Count);

        // The sheet is never narrower than its title, so a short palette still holds the
        // line that names the sheet (T-2).
        int width = (Margin * 2) + Math.Max(columns * CellWidth, SheetFont.WidthOf(title));
        int height = (Margin * 2) + TitleHeight + (rows * CellHeight);

        var canvas = new AtlasCanvas(width, height);
        canvas.Fill(0, 0, width, height, ground);
        SheetFont.Draw(canvas, title, Margin, Margin, text);

        for (int index = 0; index < palette.Colors.Count; index += 1)
        {
            int x = Margin + ((index % Columns) * CellWidth);
            int y = Margin + TitleHeight + ((index / Columns) * CellHeight);
            DrawSwatch(canvas, palette.Colors[index], x, y, text, edge);
        }

        return canvas.ToImage();
    }

    // The label holds no parenthesis, because the sheet font carries none.
    private static string Title(Palette palette) =>
        $"palette {palette.Colors.Count} colors d-181 d-185. key on the swatch, index and name under it";

    private static void DrawSwatch(
        AtlasCanvas canvas,
        PaletteColor color,
        int x,
        int y,
        PaletteColor text,
        PaletteColor edge)
    {
        canvas.Fill(x, y, SwatchSize, SwatchSize, edge);
        canvas.Fill(x + 1, y + 1, SwatchSize - 2, SwatchSize - 2, color);

        // The key reads on a light color and on a dark one, so the sheet picks the part of
        // the pair that the eye separates from the swatch.
        PaletteColor onSwatch = IsLight(color) ? edge : text;
        int keyX = x + ((SwatchSize - SheetFont.WidthOf(color.Key)) / 2);
        int keyY = y + ((SwatchSize - SheetFont.GlyphHeight) / 2);
        SheetFont.Draw(canvas, color.Key, keyX, keyY, onSwatch);

        SheetFont.Draw(canvas, $"{color.Index} {color.Name}", x, y + SwatchSize + 4, text);
        SheetFont.Draw(canvas, color.Hex, x, y + SwatchSize + 12, text);
    }

    // The weights follow the eye: green carries the most light, and blue the least. The math
    // is whole-number, as every tool with a compared output is (D-502).
    private static bool IsLight(PaletteColor color) =>
        ((color.Red * 2) + (color.Green * 3) + color.Blue) > (6 * 128);
}
