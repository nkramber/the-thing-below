using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Atlas;
using TheThingBelow.Tools.NormalMaps;
using TheThingBelow.Tools.Png;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The review sheet of the normal maps (D-521, D-841): one row for each drawing, with no
/// light and then lit from eight fixed sides, at 3x.
/// </summary>
public sealed class NormalSheetTests
{
    private static readonly SortedDictionary<string, NormalOverride> NoOverrides = new(StringComparer.Ordinal);

    [Fact]
    public void TheSheetHoldsOneRowOfNineCopiesForEachDrawing()
    {
        Drawing one = DrawingFixtures.Solid("one");
        Drawing two = DrawingFixtures.Solid("two");
        AtlasLayout layout = AtlasLayout.Build([one, two]);

        PngImage sheet = Assert.Single(
            NormalSheet.Render(AtlasPageKind.MapSprites, layout, AtlasCommand.ById([one, two]), DrawingFixtures.Palette(), NoOverrides));

        // Nine copies of 32 pixels at 3x, each with its pad, fit across the sheet.
        Assert.True(sheet.Width >= 9 * 32 * NormalSheet.Scale);
        Assert.True(sheet.Height >= 2 * 32 * NormalSheet.Scale);
    }

    [Fact]
    public void AKindThatTakesNoLightHasNoSheet()
    {
        Drawing icon = DrawingFixtures.Solid("icon", page: "ui", width: 16, height: 16);
        AtlasLayout layout = AtlasLayout.Build([icon]);

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(
            () => NormalSheet.Render(AtlasPageKind.Ui, layout, AtlasCommand.ById([icon]), DrawingFixtures.Palette(), NoOverrides));

        Assert.Contains("D-210", error.Message);
    }

    [Fact]
    public void AKindWithNoDrawingFails()
    {
        Drawing one = DrawingFixtures.Solid("one");
        AtlasLayout layout = AtlasLayout.Build([one]);

        Assert.Throws<InvalidOperationException>(
            () => NormalSheet.Render(AtlasPageKind.Tiles, layout, AtlasCommand.ById([one]), DrawingFixtures.Palette(), NoOverrides));
    }

    /// <summary>A pixel that faces the viewer takes the same light from each of the eight sides.</summary>
    [Fact]
    public void AFlatPixelTakesTheSameLightFromEachSide()
    {
        var normals = new NormalFrame(1, 1);
        (int red, int green, int blue) = NormalMap.Encode(0, 0, 1);
        normals.Set(0, 0, red, green, blue);

        int first = NormalSheet.LightOf(normals, 0, 0, 0);
        for (int light = 1; light < NormalSheet.Lights.Count; light += 1)
        {
            Assert.Equal(first, NormalSheet.LightOf(normals, 0, 0, light));
        }

        Assert.True(first > NormalSheet.Ambient);
    }

    /// <summary>A pixel that faces east takes the most light from the east, and the ambient light alone from the west.</summary>
    [Fact]
    public void APixelThatFacesEastIsLitFromTheEastAlone()
    {
        var normals = new NormalFrame(1, 1);
        (int red, int green, int blue) = NormalMap.Encode(1, 0, 1);
        normals.Set(0, 0, red, green, blue);

        int east = NormalSheet.LightOf(normals, 0, 0, IndexOf("E"));
        int west = NormalSheet.LightOf(normals, 0, 0, IndexOf("W"));
        int north = NormalSheet.LightOf(normals, 0, 0, IndexOf("N"));

        Assert.True(east > 990, $"The east light gave {east}.");
        Assert.Equal(NormalSheet.Ambient, west);
        Assert.True(north > west && north < east);
    }

    private static int IndexOf(string side)
    {
        for (int index = 0; index < NormalSheet.Lights.Count; index += 1)
        {
            if (string.CompareOrdinal(NormalSheet.Lights[index].Name, side) == 0)
            {
                return index;
            }
        }

        throw new ArgumentException($"No light has the side '{side}'.", nameof(side));
    }
}
