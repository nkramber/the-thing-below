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

    /// <summary>Draws the review sheet of one kind of drawing.</summary>
    /// <param name="kind">The kind of page whose drawings the sheet shows.</param>
    /// <param name="layout">The layout that names each drawing and its page.</param>
    /// <param name="drawings">Every drawing of the layout, by its id.</param>
    /// <param name="palette">The palette that gives the color of each key.</param>
    /// <returns>The image of the sheet.</returns>
    /// <exception cref="InvalidOperationException">The layout holds no drawing of that kind (T-2).</exception>
    public static PngImage Render(
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

        string title = Title(kind, batch.Count);
        var shape = new SheetShape(batch, title);
        var canvas = new AtlasCanvas(shape.Width, shape.Height);
        PaletteColor night = palette.ColorNamed("night");
        PaletteColor snow = palette.ColorNamed("snow");
        PaletteColor text = palette.ColorNamed("chalk");

        canvas.Fill(0, 0, shape.Width, shape.Height, palette.ColorNamed("shadow"));
        SheetFont.Draw(canvas, title, Margin, Margin, text);

        for (int index = 0; index < batch.Count; index += 1)
        {
            int x = Margin + ((index % shape.Columns) * shape.CellWidth);
            int y = Margin + TitleHeight + ((index / shape.Columns) * shape.CellHeight);
            DrawCell(canvas, batch[index], palette, x, y, shape, night, snow, text);
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

    private static string Title(AtlasPageKind kind, int count) =>
        $"{AtlasPages.NameOf(kind)} batch, {count} drawings. each one at 1x and {LargeScale}x, on night and on snow";

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

    /// <summary>The size of one cell of the sheet, and the count of cells in one row.</summary>
    private sealed class SheetShape
    {
        public SheetShape(List<Drawing> batch, string title)
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
            int rows = (batch.Count + this.Columns - 1) / this.Columns;

            // The sheet is never narrower than its title, so a batch of small drawings
            // still holds the line that names the batch (T-2).
            int cells = Math.Min(this.Columns, batch.Count) * this.CellWidth;
            this.Width = (Margin * 2) + Math.Max(cells, SheetFont.WidthOf(title));
            this.Height = (Margin * 2) + TitleHeight + (rows * this.CellHeight);
        }

        public int GroundWidth { get; }

        public int GroundHeight { get; }

        public int CellWidth { get; }

        public int CellHeight { get; }

        public int Columns { get; }

        public int Width { get; }

        public int Height { get; }
    }
}
