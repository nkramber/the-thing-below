using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Atlas;
using TheThingBelow.Tools.Png;

namespace TheThingBelow.Tools.NormalMaps;

/// <summary>
/// The review sheet of the normal maps (D-521, D-841). Each drawing takes one row on a night
/// ground: the drawing with no light, then the same drawing lit from eight fixed sides, each
/// at 3x, with its id under the row.
/// </summary>
/// <remarks>
/// The sheet computes its light in Tools with whole numbers, not in the engine, so a small
/// difference from the game can stay hidden until PR-56 draws the light in Game (D-521). The
/// sheet reads the encoded normal map, so it shows what the atlas holds, an override grid
/// included.
/// <para>
/// One sheet holds one kind of drawing, and a batch that does not fit one sheet takes more
/// sheets, as D-668 splits a batch. Each sheet shows the first frame of each drawing (D-668).
/// </para>
/// </remarks>
public static class NormalSheet
{
    /// <summary>The scale of each copy of a drawing (D-841).</summary>
    public const int Scale = 3;

    /// <summary>
    /// The tallest sheet in pixels. A batch that does not fit one sheet takes more sheets, and
    /// each one holds whole rows (D-668).
    /// </summary>
    public const int MaxHeight = 1600;

    /// <summary>
    /// The light that reaches a pixel which faces away from the light, in thousandths. It
    /// keeps the dark side of a drawing readable on the sheet.
    /// </summary>
    public const int Ambient = 250;

    /// <summary>
    /// The eight fixed sides of the light, in the order of the sheet (D-521). Each light
    /// stands 45 degrees above the plane of the screen. The parts are in thousandths: to the
    /// right, up the screen, and toward the viewer.
    /// </summary>
    public static readonly IReadOnlyList<(string Name, int Right, int Up, int Toward)> Lights =
    [
        ("N", 0, 707, 707),
        ("NE", 500, 500, 707),
        ("E", 707, 0, 707),
        ("SE", 500, -500, 707),
        ("S", 0, -707, 707),
        ("SW", -500, -500, 707),
        ("W", -707, 0, 707),
        ("NW", -500, 500, 707),
    ];

    private const int Pad = 6;
    private const int Margin = 12;
    private const int TitleHeight = 16;
    private const int LabelHeight = SheetFont.GlyphHeight + 4;
    private const int Thousand = 1000;

    // The largest distance of a channel from its middle, as the encoding of NormalMap holds it.
    private const int ChannelReach = 127;
    private const int ChannelMiddle = 128;

    /// <summary>Draws the review sheets of the normal maps of one kind of drawing.</summary>
    /// <param name="kind">The kind of page, one that takes scene light.</param>
    /// <param name="layout">The layout that names each drawing and its page.</param>
    /// <param name="drawings">Every drawing of the layout, by its id.</param>
    /// <param name="palette">The palette that gives the color and the height of each key.</param>
    /// <param name="overrides">Every override grid, by the id of its drawing.</param>
    /// <returns>The image of each sheet, in the order of the drawings.</returns>
    /// <exception cref="InvalidOperationException">
    /// The kind takes no scene light, or the layout holds no drawing of that kind (T-2).
    /// </exception>
    public static IReadOnlyList<PngImage> Render(
        AtlasPageKind kind,
        AtlasLayout layout,
        IReadOnlyDictionary<string, Drawing> drawings,
        Palette palette,
        IReadOnlyDictionary<string, NormalOverride> overrides)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(drawings);
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(overrides);
        if (!AtlasPages.TakesLight(kind))
        {
            throw new InvalidOperationException(
                $"The kind '{AtlasPages.NameOf(kind)}' takes no scene light, so it has no normal map and no sheet of one (D-210).");
        }

        List<Drawing> batch = BatchOf(kind, layout, drawings);
        if (batch.Count == 0)
        {
            throw new InvalidOperationException(
                $"The layout holds no drawing of the kind '{AtlasPages.NameOf(kind)}', so it has no review sheet (T-2).");
        }

        int cellWidth = 0;
        int cellHeight = 0;
        foreach (Drawing drawing in batch)
        {
            cellWidth = Math.Max(cellWidth, (drawing.Width * Scale) + Pad);
            cellHeight = Math.Max(cellHeight, drawing.Height * Scale);
        }

        int rowHeight = Pad + cellHeight + Pad + LabelHeight;
        int top = Margin + TitleHeight + LabelHeight;
        int rowsPerSheet = Math.Max(1, (MaxHeight - top - Margin) / rowHeight);
        int sheetCount = (batch.Count + rowsPerSheet - 1) / rowsPerSheet;

        List<PngImage> sheets = [];
        for (int sheet = 0; sheet < sheetCount; sheet += 1)
        {
            int start = sheet * rowsPerSheet;
            List<Drawing> part = batch.GetRange(start, Math.Min(rowsPerSheet, batch.Count - start));
            string title = Title(kind, batch.Count, sheet + 1, sheetCount);
            int width = (Margin * 2) + Math.Max(((Lights.Count + 1) * cellWidth) + Pad, SheetFont.WidthOf(title));
            int height = top + (part.Count * rowHeight) + Margin;
            var canvas = new AtlasCanvas(width, height);
            canvas.Fill(0, 0, width, height, palette.ColorNamed("shadow"));
            PaletteColor text = palette.ColorNamed("chalk");
            SheetFont.Draw(canvas, title, Margin, Margin, text);
            DrawHeader(canvas, cellWidth, Margin + TitleHeight, text);
            for (int index = 0; index < part.Count; index += 1)
            {
                int y = top + (index * rowHeight);
                overrides.TryGetValue(part[index].Id.Value, out NormalOverride? grid);
                DrawRow(canvas, part[index], grid, palette, cellWidth, cellHeight, y);
            }

            sheets.Add(canvas.ToImage());
        }

        return sheets;
    }

    /// <summary>
    /// Gives the light that reaches one pixel of a normal map from one light, in thousandths:
    /// <see cref="Ambient"/> on a side that faces away, and 1000 on a side that faces the light.
    /// </summary>
    /// <param name="normals">The normal map of the frame.</param>
    /// <param name="x">The pixel column.</param>
    /// <param name="y">The pixel row.</param>
    /// <param name="light">The position of the light in <see cref="Lights"/>.</param>
    /// <returns>The light of the pixel, from <see cref="Ambient"/> to 1000.</returns>
    public static int LightOf(NormalFrame normals, int x, int y, int light)
    {
        ArgumentNullException.ThrowIfNull(normals);

        (_, int right, int up, int toward) = Lights[light];
        int dot = ((normals.Channel(x, y, 0) - ChannelMiddle) * right)
            + ((normals.Channel(x, y, 1) - ChannelMiddle) * up)
            + ((normals.Channel(x, y, 2) - ChannelMiddle) * toward);
        int facing = Math.Max(0, dot);
        int lit = Ambient + ((Thousand - Ambient) * facing / (ChannelReach * Thousand));
        return Math.Min(Thousand, lit);
    }

    private static List<Drawing> BatchOf(
        AtlasPageKind kind,
        AtlasLayout layout,
        IReadOnlyDictionary<string, Drawing> drawings)
    {
        var batch = new List<Drawing>();
        foreach (AtlasEntry entry in layout.Entries)
        {
            Drawing drawing = drawings[entry.Id.Value];
            if (drawing.Page == kind)
            {
                batch.Add(drawing);
            }
        }

        return batch;
    }

    private static string Title(AtlasPageKind kind, int count, int sheet, int sheetCount)
    {
        string part = sheetCount == 1 ? string.Empty : $", sheet {sheet} of {sheetCount}";
        return $"{AtlasPages.NameOf(kind)} normal maps, {count} drawings{part}. each one with no light, then lit from 8 sides, at {Scale}x on night";
    }

    private static void DrawHeader(AtlasCanvas canvas, int cellWidth, int y, PaletteColor text)
    {
        SheetFont.Draw(canvas, "no light", Margin, y, text);
        for (int light = 0; light < Lights.Count; light += 1)
        {
            SheetFont.Draw(canvas, Lights[light].Name, Margin + ((light + 1) * cellWidth), y, text);
        }
    }

    private static void DrawRow(
        AtlasCanvas canvas,
        Drawing drawing,
        NormalOverride? grid,
        Palette palette,
        int cellWidth,
        int cellHeight,
        int y)
    {
        PaletteColor night = palette.ColorNamed("night");
        int groundWidth = ((Lights.Count + 1) * cellWidth) + Pad;
        canvas.Fill(Margin, y, groundWidth, Pad + cellHeight + Pad, night);

        // Each copy sits on the baseline of the row, so drawings of two heights line up.
        int copyY = y + Pad + cellHeight - (drawing.Height * Scale);
        canvas.Draw(drawing, 0, palette, Margin + Pad, copyY, Scale);

        NormalFrame normals = NormalMap.Build(drawing, 0, palette, grid?.Frames[0]);
        for (int light = 0; light < Lights.Count; light += 1)
        {
            int copyX = Margin + ((light + 1) * cellWidth) + Pad;
            DrawLit(canvas, drawing, normals, palette, light, copyX, copyY);
        }

        SheetFont.Draw(canvas, drawing.Id.Name, Margin, y + Pad + cellHeight + Pad + 3, palette.ColorNamed("chalk"));
    }

    private static void DrawLit(
        AtlasCanvas canvas,
        Drawing drawing,
        NormalFrame normals,
        Palette palette,
        int light,
        int left,
        int top)
    {
        IReadOnlyList<string> rows = drawing.Frames[0].Rows;
        for (int y = 0; y < drawing.Height; y += 1)
        {
            for (int x = 0; x < drawing.Width; x += 1)
            {
                char key = rows[y][x];
                if (key == Drawing.Transparent)
                {
                    continue;
                }

                if (!palette.TryColorOf(key, out PaletteColor? color))
                {
                    throw new InvalidOperationException(
                        $"The file '{drawing.File}' holds the key '{key}' at frame 0, row {y}, column {x}, and the palette has no such color (F-20).");
                }

                int lit = LightOf(normals, x, y, light);
                FillSquare(canvas, left + (x * Scale), top + (y * Scale), Shade(color.Red, lit), Shade(color.Green, lit), Shade(color.Blue, lit));
            }
        }
    }

    private static int Shade(int channel, int lit) => channel * lit / Thousand;

    private static void FillSquare(AtlasCanvas canvas, int left, int top, int red, int green, int blue)
    {
        for (int row = 0; row < Scale; row += 1)
        {
            for (int column = 0; column < Scale; column += 1)
            {
                canvas.Set(left + column, top + row, red, green, blue);
            }
        }
    }
}
