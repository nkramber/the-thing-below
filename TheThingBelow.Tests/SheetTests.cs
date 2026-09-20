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
        PngImage sheet = ReviewSheet.Render(AtlasPageKind.MapSprites, layout, AtlasCommand.ById(drawings), palette);

        Assert.Equal([AtlasPageKind.MapSprites, AtlasPageKind.Portraits], kinds);
        Assert.True(sheet.Width > 32 * ReviewSheet.LargeScale);
        Assert.True(sheet.Width <= ReviewSheet.MaxWidth);
    }

    /// <summary>The sheet draws each drawing on a night ground and on a snow ground (D-668).</summary>
    [Fact]
    public void AReviewSheetDrawsBothGrounds()
    {
        Palette palette = DrawingFixtures.Palette();
        List<Drawing> drawings = [DrawingFixtures.Solid("one", "map_sprites", 8, 8)];
        AtlasLayout layout = AtlasLayout.Build(drawings);

        PngImage sheet = ReviewSheet.Render(AtlasPageKind.MapSprites, layout, AtlasCommand.ById(drawings), palette);

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
