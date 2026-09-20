using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Atlas;
using TheThingBelow.Tools.Content;
using TheThingBelow.Tools.Png;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The swatch sheet and the review sheets (D-185, D-668). The session attaches each one to
/// the PR description, and no sheet enters git (D-514).
/// </summary>
public sealed class SheetTests
{
    /// <summary>The font must draw every key of the palette of this checkout (D-185).</summary>
    [Fact]
    public void TheFontHoldsAGlyphForEveryKeyOfThePalette()
    {
        Palette palette = AtlasCommand.ReadArt(RepositoryRoot.Find(), out _);

        foreach (PaletteColor color in palette.Colors)
        {
            Assert.True(
                SheetFont.Holds(color.KeyCharacter),
                $"The sheet font has no glyph for the key '{color.Key}' of the color '{color.Name}'.");
        }
    }

    /// <summary>The font must draw the name of every drawing and of every color.</summary>
    [Fact]
    public void TheFontHoldsAGlyphForEveryLabelOfThisCheckout()
    {
        Palette palette = AtlasCommand.ReadArt(RepositoryRoot.Find(), out List<Drawing> drawings);

        var labels = new List<string>();
        foreach (PaletteColor color in palette.Colors)
        {
            labels.Add($"{color.Index} {color.Name}");
            labels.Add(color.Hex);
        }

        foreach (Drawing drawing in drawings)
        {
            labels.Add(drawing.Id.Name);
        }

        foreach (string label in labels)
        {
            foreach (char character in label)
            {
                Assert.True(SheetFont.Holds(character), $"The sheet font has no glyph for '{character}' of '{label}'.");
            }
        }
    }

    [Fact]
    public void TheWidthOfALabelCountsTheGlyphsAndTheGaps()
    {
        Assert.Equal(0, SheetFont.WidthOf(string.Empty));
        Assert.Equal(SheetFont.GlyphWidth, SheetFont.WidthOf("a"));
        Assert.Equal(SheetFont.GlyphWidth + SheetFont.Advance, SheetFont.WidthOf("ab"));
    }

    [Fact]
    public void ACharacterThatTheFontLacksFails()
    {
        var canvas = new AtlasCanvas(64, 16);

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(
            () => SheetFont.Draw(canvas, "aéb", 0, 0, DrawingFixtures.Palette().ColorNamed("chalk")));

        Assert.Contains("no glyph", error.Message);
    }

    [Fact]
    public void TheSwatchSheetShowsEveryColorOfThePalette()
    {
        Palette palette = DrawingFixtures.Palette();

        PngImage sheet = SwatchSheet.Render(palette);

        int rows = (palette.Colors.Count + SwatchSheet.Columns - 1) / SwatchSheet.Columns;
        Assert.True(sheet.Height > rows * 64);
        Assert.Equal(PngColorKind.Rgba, sheet.Colors);
    }

    [Fact]
    public void AReviewSheetHoldsOneKindOfDrawing()
    {
        Palette palette = DrawingFixtures.Palette();
        List<Drawing> drawings =
        [
            DrawingFixtures.Solid("one", "map_sprites"),
            DrawingFixtures.Solid("two", "portraits", 64, 64),
        ];
        AtlasLayout layout = AtlasLayout.Build(drawings);

        IReadOnlyList<AtlasPageKind> kinds = ReviewSheet.KindsOf(layout);
        PngImage sheet = Assert.Single(ReviewSheet.Render(AtlasPageKind.MapSprites, layout, AtlasCommand.ById(drawings), palette));

        Assert.Equal([AtlasPageKind.MapSprites, AtlasPageKind.Portraits], kinds);
        Assert.True(sheet.Width > 32 * ReviewSheet.LargeScale);
        Assert.True(sheet.Width <= ReviewSheet.MaxWidth);
    }

    /// <summary>A batch that does not fit one sheet takes more sheets, each one of whole rows (D-668).</summary>
    [Fact]
    public void ABatchThatDoesNotFitOneSheetTakesMoreSheets()
    {
        Palette palette = DrawingFixtures.Palette();
        List<Drawing> drawings = [];
        for (int index = 0; index < 8; index += 1)
        {
            drawings.Add(DrawingFixtures.Solid($"big{index}", "map_sprites", 64, 64));
        }

        AtlasLayout layout = AtlasLayout.Build(drawings);

        IReadOnlyList<PngImage> sheets = ReviewSheet.Render(AtlasPageKind.MapSprites, layout, AtlasCommand.ById(drawings), palette);

        Assert.True(sheets.Count > 1, $"The batch gave {sheets.Count} sheet.");
        Assert.All(sheets, sheet => Assert.True(sheet.Height <= ReviewSheet.MaxHeight));
    }

    /// <summary>A long color name widens the cell, so no label reaches the next cell or leaves the canvas.</summary>
    [Fact]
    public void ALongColorNameWidensTheSwatchCell()
    {
        Palette palette = Palette.Read(
            System.Text.Encoding.UTF8.GetBytes(
                """
                {
                 "comment": "a test palette",
                 "colors": [
                  { "index": 0, "key": "s", "hex": "2a2f3a", "name": "slate" },
                  { "index": 1, "key": "c", "hex": "e8e4d8", "name": "chalk" },
                  { "index": 2, "key": "i", "hex": "0b0a0f", "name": "ink" },
                  { "index": 3, "key": "l", "hex": "101010", "name": "a name of twenty two glyphs" }
                 ]
                }
                """),
            Palette.Path);

        int width = SwatchSheet.CellWidthOf(palette);
        PngImage sheet = SwatchSheet.Render(palette);

        Assert.True(width > 112, $"The cell is {width} pixels wide.");
        Assert.True(sheet.Width >= width);
    }

    /// <summary>The sheet draws each drawing on a night ground and on a snow ground (D-668).</summary>
    [Fact]
    public void AReviewSheetDrawsBothGrounds()
    {
        Palette palette = DrawingFixtures.Palette();
        List<Drawing> drawings = [DrawingFixtures.Solid("one", "map_sprites", 8, 8)];
        AtlasLayout layout = AtlasLayout.Build(drawings);

        PngImage sheet = Assert.Single(ReviewSheet.Render(AtlasPageKind.MapSprites, layout, AtlasCommand.ById(drawings), palette));

        Assert.True(HoldsColor(sheet, palette.ColorNamed("night")), "the sheet holds no night ground");
        Assert.True(HoldsColor(sheet, palette.ColorNamed("snow")), "the sheet holds no snow ground");
    }

    [Fact]
    public void AKindWithNoDrawingHasNoReviewSheet()
    {
        List<Drawing> drawings = [DrawingFixtures.Solid("one", "map_sprites")];
        AtlasLayout layout = AtlasLayout.Build(drawings);

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(
            () => ReviewSheet.Render(AtlasPageKind.Tiles, layout, AtlasCommand.ById(drawings), DrawingFixtures.Palette()));

        Assert.Contains("tiles", error.Message);
    }

    private static bool HoldsColor(PngImage sheet, PaletteColor color)
    {
        ReadOnlySpan<byte> pixels = sheet.Pixels;
        for (int start = 0; start + 3 < pixels.Length; start += 4)
        {
            bool same = pixels[start] == color.Red &&
                pixels[start + 1] == color.Green &&
                pixels[start + 2] == color.Blue;
            if (same)
            {
                return true;
            }
        }

        return false;
    }
}
