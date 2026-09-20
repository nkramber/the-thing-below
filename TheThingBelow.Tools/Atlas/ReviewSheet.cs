using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Png;

namespace TheThingBelow.Tools.Atlas;

/// <summary>
/// The review sheet of an art batch (D-668). It shows each drawing at 1x and at 6x, on a
/// night ground and on a snow ground, with its id under it.
/// </summary>
/// <remarks>
/// The owner approved this form for the cast sample of D-402. The scale of 1x proves that a
/// drawing reads at the size of play, and 6x shows each pixel. The two grounds prove each
/// outline against a dark background and a light background.
/// <para>
/// The session attaches each sheet to the PR description, and no sheet enters git (D-514,
/// G-25). One sheet holds one kind of drawing, so a batch of tiles and a batch of sprites
/// reach the owner as two pictures.
/// </para>
/// </remarks>
public static class ReviewSheet
{
    /// <summary>The scale of the large copy of each drawing.</summary>
    public const int LargeScale = 6;

    /// <summary>The widest sheet in pixels, so a sheet reads on one screen.</summary>
    public const int MaxWidth = 1600;

    /// <summary>
    /// The tallest sheet in pixels. A batch that does not fit one sheet takes more sheets
    /// (D-668), and each one holds whole rows of cells.
    /// </summary>
    public const int MaxHeight = 1600;

    private const int Pad = 6;
    private const int LabelHeight = SheetFont.GlyphHeight + 4;
    private const int Margin = 12;
    private const int TitleHeight = 16;

    /// <summary>Gives each kind of page that a layout holds, in the order of the kinds.</summary>
    /// <param name="layout">The layout of the atlas.</param>
    /// <returns>Each kind that holds one drawing at least.</returns>
    public static IReadOnlyList<AtlasPageKind> KindsOf(AtlasLayout layout)
    {
        ArgumentNullException.ThrowIfNull(layout);

        var kinds = new List<AtlasPageKind>();
        foreach (AtlasPage page in layout.Pages)
        {
            if (!kinds.Contains(page.Kind))
            {
                kinds.Add(page.Kind);
            }
        }

        return kinds;
    }

    /// <summary>Draws the review sheets of one kind of drawing.</summary>
    /// <param name="kind">The kind of page whose drawings the sheets show.</param>
    /// <param name="layout">The layout that names each drawing and its page.</param>
    /// <param name="drawings">Every drawing of the layout, by its id.</param>
    /// <param name="palette">The palette that gives the color of each key.</param>
    /// <returns>
    /// The image of each sheet, in the order of the drawings. A batch that fits one sheet gives
    /// one image, and a larger batch gives one image for each <see cref="MaxHeight"/> (D-668).
    /// </returns>
    /// <exception cref="InvalidOperationException">The layout holds no drawing of that kind (T-2).</exception>
    public static IReadOnlyList<PngImage> Render(
        AtlasPageKind kind,
        AtlasLayout layout,
        IReadOnlyDictionary<string, Drawing> drawings,
        Palette palette)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(drawings);
        ArgumentNullException.ThrowIfNull(palette);

        List<Drawing> batch = BatchOf(kind, layout, drawings);
        if (batch.Count == 0)
        {
            throw new InvalidOperationException(
                $"The layout holds no drawing of the kind '{AtlasPages.NameOf(kind)}', so it has no review sheet (T-2).");
        }

        var shape = new SheetShape(batch);
        int rowsPerSheet = Math.Max(1, (MaxHeight - (Margin * 2) - TitleHeight) / shape.CellHeight);
        int perSheet = rowsPerSheet * shape.Columns;
        int sheetCount = (batch.Count + perSheet - 1) / perSheet;

        List<PngImage> sheets = [];
        for (int sheet = 0; sheet < sheetCount; sheet += 1)
        {
            int start = sheet * perSheet;
            List<Drawing> part = batch.GetRange(start, Math.Min(perSheet, batch.Count - start));
            string title = Title(kind, batch.Count, sheet + 1, sheetCount);
            sheets.Add(RenderSheet(part, title, shape, palette));
        }

        return sheets;
    }

    private static PngImage RenderSheet(List<Drawing> part, string title, SheetShape shape, Palette palette)
    {
        int width = shape.WidthOf(part.Count, title);
        int height = shape.HeightOf(part.Count);
        var canvas = new AtlasCanvas(width, height);
        PaletteColor night = palette.ColorNamed("night");
        PaletteColor snow = palette.ColorNamed("snow");
        PaletteColor text = palette.ColorNamed("chalk");

        canvas.Fill(0, 0, width, height, palette.ColorNamed("shadow"));
        SheetFont.Draw(canvas, title, Margin, Margin, text);

        for (int index = 0; index < part.Count; index += 1)
        {
            int x = Margin + ((index % shape.Columns) * shape.CellWidth);
            int y = Margin + TitleHeight + ((index / shape.Columns) * shape.CellHeight);
            DrawCell(canvas, part[index], palette, x, y, shape, night, snow, text);
        }

        return canvas.ToImage();
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
        return $"{AtlasPages.NameOf(kind)} batch, {count} drawings{part}. each one at 1x and {LargeScale}x, on night and on snow";
    }

    private static void DrawCell(
        AtlasCanvas canvas,
        Drawing drawing,
        Palette palette,
        int x,
        int y,
        SheetShape shape,
        PaletteColor night,
        PaletteColor snow,
        PaletteColor text)
    {
        int groundWidth = shape.GroundWidth;
        int groundHeight = shape.GroundHeight;
        canvas.Fill(x, y, groundWidth, groundHeight, night);
        canvas.Fill(x + groundWidth, y, groundWidth, groundHeight, snow);

        // The small copy sits on the baseline of the large one, so the eye compares them.
        int smallY = y + Pad + ((drawing.Height * LargeScale) - drawing.Height);
        DrawPair(canvas, drawing, palette, x, y, smallY);
        DrawPair(canvas, drawing, palette, x + groundWidth, y, smallY);

        SheetFont.Draw(canvas, drawing.Id.Name, x, y + groundHeight + 3, text);
    }

    private static void DrawPair(AtlasCanvas canvas, Drawing drawing, Palette palette, int x, int y, int smallY)
    {
        canvas.Draw(drawing, 0, palette, x + Pad, smallY, 1);
        canvas.Draw(drawing, 0, palette, x + Pad + drawing.Width + Pad, y + Pad, LargeScale);
    }

    /// <summary>
    /// The size of one cell of the sheets of a batch, and the count of cells in one row. Every
    /// sheet of one batch takes the same cell, so the drawings read at one size across sheets.
    /// </summary>
    private sealed class SheetShape
    {
        public SheetShape(List<Drawing> batch)
        {
            int width = 0;
            int height = 0;
            int label = 0;
            foreach (Drawing drawing in batch)
            {
                width = Math.Max(width, drawing.Width);
                height = Math.Max(height, drawing.Height);
                label = Math.Max(label, SheetFont.WidthOf(drawing.Id.Name));
            }

            this.GroundWidth = Pad + width + Pad + (width * LargeScale) + Pad;
            this.GroundHeight = Pad + (height * LargeScale) + Pad;
            this.CellWidth = Math.Max((this.GroundWidth * 2) + Pad, label + Pad);
            this.CellHeight = this.GroundHeight + LabelHeight;

            int room = (MaxWidth - (Margin * 2)) / this.CellWidth;
            this.Columns = Math.Max(1, room);
        }

        public int GroundWidth { get; }

        public int GroundHeight { get; }

        public int CellWidth { get; }

        public int CellHeight { get; }

        public int Columns { get; }

        /// <summary>
        /// The width of a sheet of a count of cells. The sheet is never narrower than its
        /// title, so a batch of small drawings still holds the line that names it (T-2).
        /// </summary>
        public int WidthOf(int count, string title)
        {
            int cells = Math.Min(this.Columns, count) * this.CellWidth;
            return (Margin * 2) + Math.Max(cells, SheetFont.WidthOf(title));
        }

        public int HeightOf(int count)
        {
            int rows = (count + this.Columns - 1) / this.Columns;
            return (Margin * 2) + TitleHeight + (rows * this.CellHeight);
        }
    }
}
