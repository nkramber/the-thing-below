using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Atlas;
using TheThingBelow.Tools.NormalMaps;
using TheThingBelow.Tools.Png;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The builder of the normal maps (D-184). The height of a pixel is its distance to the edge,
/// up to the rim of D-840, plus the height of its color (D-838). An override grid sets the
/// direction of a pixel (D-839).
/// </summary>
/// <remarks>
/// A channel maps -1 to 1 onto 1 to 255, with 128 for 0. Red is the part to the right, and
/// green is the part up the screen, as the canvas shader of Godot 4.7.2 reads them.
/// </remarks>
public sealed class NormalMapTests
{
    private const int Middle = 128;

    [Fact]
    public void AFlatDirectionFacesTheViewer()
    {
        Assert.Equal((128, 128, 255), NormalMap.Encode(0, 0, 1));
    }

    /// <summary>A part and its opposite sit at the same distance from the middle.</summary>
    [Fact]
    public void OppositeDirectionsGiveMirroredChannels()
    {
        (int right, int _, int blue) = NormalMap.Encode(1, 0, 1);
        (int left, int _, int sameBlue) = NormalMap.Encode(-1, 0, 1);

        Assert.Equal(218, right);
        Assert.Equal(38, left);
        Assert.Equal(blue, sameBlue);
    }

    /// <summary>
    /// A solid tile has no transparent pixel, and the border of a frame is no edge, so a floor
    /// of tiles shows no seam (D-840).
    /// </summary>
    [Fact]
    public void ASolidTileIsFlatToItsBorder()
    {
        Drawing tile = DrawingFixtures.Solid("floor", page: "tiles");

        NormalFrame normals = NormalMap.Build(tile, 0, DrawingFixtures.Palette(), null);

        for (int y = 0; y < tile.Height; y += 1)
        {
            for (int x = 0; x < tile.Width; x += 1)
            {
                Assert.Equal(Middle, normals.Channel(x, y, 0));
                Assert.Equal(Middle, normals.Channel(x, y, 1));
                Assert.Equal(255, normals.Channel(x, y, 2));
            }
        }
    }

    [Fact]
    public void TheHeightClimbsFromTheEdgeUpToTheRim()
    {
        Drawing drawing = DrawingFixtures.FromRows("strip", ["..........kkkkkkkkkk.........."]);

        int[,] heights = NormalMap.HeightsOf(drawing, 0, DrawingFixtures.Palette());

        Assert.Equal(0, heights[9, 0]);
        Assert.Equal(1, heights[10, 0]);
        Assert.Equal(2, heights[11, 0]);
        Assert.Equal(3, heights[12, 0]);
        Assert.Equal(NormalMap.RimDepth, heights[13, 0]);
        Assert.Equal(NormalMap.RimDepth, heights[14, 0]);
    }

    [Fact]
    public void TheHeightOfTheColorAddsToTheHeightFromTheEdge()
    {
        Palette palette = Palette.Read(
            Encoding.UTF8.GetBytes(DrawingFixtures.PaletteBody.Replace("\"red\", \"height\": 0", "\"red\", \"height\": 3")),
            Palette.Path);
        Drawing drawing = DrawingFixtures.FromRows("belt", ["kkkkk", "kkxkk", "kkkkk"]);

        int[,] heights = NormalMap.HeightsOf(drawing, 0, palette);

        Assert.Equal(NormalMap.RimDepth, heights[0, 0]);
        Assert.Equal(NormalMap.RimDepth + 3, heights[2, 1]);
    }

    /// <summary>A left edge faces left, a right edge faces right, a top edge faces up.</summary>
    [Fact]
    public void EachEdgeFacesAwayFromTheShape()
    {
        var rows = new List<string>();
        for (int y = 0; y < 12; y += 1)
        {
            rows.Add(y is >= 2 and < 10 ? "..kkkkkkkk.." : "............");
        }

        Drawing block = DrawingFixtures.FromRows("block", rows);

        NormalFrame normals = NormalMap.Build(block, 0, DrawingFixtures.Palette(), null);

        Assert.True(normals.Channel(2, 5, 0) < Middle, "the left edge faces left");
        Assert.True(normals.Channel(9, 5, 0) > Middle, "the right edge faces right");
        Assert.True(normals.Channel(5, 2, 1) > Middle, "the top edge faces up");
        Assert.True(normals.Channel(5, 9, 1) < Middle, "the bottom edge faces down");
        Assert.False(normals.IsOpaque(0, 0));
    }

    /// <summary>The regression test of exit test 3: an override grid replaces the built normal of its pixel.</summary>
    [Fact]
    public void AnOverrideReplacesTheBuiltNormalOfItsPixelAlone()
    {
        Drawing block = DrawingFixtures.FromRows("block", [".kkkk.", ".kkkk.", ".kkkk."]);
        NormalOverride grid = DrawingFixtures.Override("block", ["..6...", "......", "......"]);
        Palette palette = DrawingFixtures.Palette();

        NormalFrame built = NormalMap.Build(block, 0, palette, null);
        NormalFrame corrected = NormalMap.Build(block, 0, palette, grid.Frames[0]);

        Assert.True(built.Channel(2, 0, 0) <= Middle);
        Assert.Equal(NormalMap.Encode(1, 0, 1), Channels(corrected, 2, 0));
        Assert.Equal(Channels(built, 3, 1), Channels(corrected, 3, 1));
    }

    /// <summary>Each digit points the way of its key on a numpad, from 5 (D-839).</summary>
    [Theory]
    [InlineData('7', -1, 1)]
    [InlineData('8', 0, 1)]
    [InlineData('9', 1, 1)]
    [InlineData('4', -1, 0)]
    [InlineData('5', 0, 0)]
    [InlineData('6', 1, 0)]
    [InlineData('1', -1, -1)]
    [InlineData('2', 0, -1)]
    [InlineData('3', 1, -1)]
    public void EachDigitPointsTheWayOfItsKey(char digit, int right, int up)
    {
        Drawing pixel = DrawingFixtures.FromRows("dot", ["k"]);
        NormalOverride grid = DrawingFixtures.Override("dot", [digit.ToString()]);

        NormalFrame normals = NormalMap.Build(pixel, 0, DrawingFixtures.Palette(), grid.Frames[0]);

        Assert.Equal(NormalMap.Encode(right, up, 1), Channels(normals, 0, 0));
    }

    /// <summary>
    /// A seed loop over drawings of random shapes. Each opaque pixel holds a normal of unit
    /// length that faces the viewer, and each transparent pixel holds none.
    /// </summary>
    [Fact]
    public void EachBuiltNormalHasUnitLengthAndFacesTheViewer()
    {
        Palette palette = DrawingFixtures.Palette();
        for (int seed = 0; seed < 200; seed += 1)
        {
            var random = new Random(seed);
            var rows = new List<string>();
            for (int y = 0; y < 16; y += 1)
            {
                var row = new StringBuilder();
                for (int x = 0; x < 16; x += 1)
                {
                    row.Append("..kKdDwx"[random.Next(8)]);
                }

                rows.Add(row.ToString());
            }

            Drawing drawing = DrawingFixtures.FromRows("random", rows);
            NormalFrame normals = NormalMap.Build(drawing, 0, palette, null);
            for (int y = 0; y < 16; y += 1)
            {
                for (int x = 0; x < 16; x += 1)
                {
                    bool opaque = rows[y][x] != Drawing.Transparent;
                    Assert.True(normals.IsOpaque(x, y) == opaque, $"Seed {seed}: the pixel {x},{y}.");
                    if (!opaque)
                    {
                        continue;
                    }

                    int red = normals.Channel(x, y, 0) - Middle;
                    int green = normals.Channel(x, y, 1) - Middle;
                    int blue = normals.Channel(x, y, 2) - Middle;
                    int square = (red * red) + (green * green) + (blue * blue);
                    Assert.True(blue > 0, $"Seed {seed}: the pixel {x},{y} faces away from the viewer.");
                    Assert.True(Math.Abs(square - (127 * 127)) <= 3 * 127, $"Seed {seed}: the pixel {x},{y} has the squared length {square}.");
                }
            }
        }
    }

    [Fact]
    public void APageOfAKindThatTakesNoLightHasNoNormalMap()
    {
        Drawing icon = DrawingFixtures.Solid("icon", page: "ui", width: 16, height: 16);
        AtlasLayout layout = AtlasLayout.Build([icon]);

        ArgumentException error = Assert.Throws<ArgumentException>(
            () => NormalMap.RenderPage(layout.Pages[0], layout, AtlasCommand.ById([icon]), DrawingFixtures.Palette(), new SortedDictionary<string, NormalOverride>()));

        Assert.Contains("D-210", error.Message);
    }

    /// <summary>Each frame sits at its place on the color page, so both pages hold one shape (D-184).</summary>
    [Fact]
    public void TheNormalPageCoversTheSamePixelsAsTheColorPage()
    {
        Drawing one = DrawingFixtures.FromRows("one", ["..kk", ".kkk", "kkk."]);
        Drawing two = DrawingFixtures.FromRows("two", ["k..k", "kkkk", ".kk."]);
        AtlasLayout layout = AtlasLayout.Build([one, two]);
        SortedDictionary<string, Drawing> byId = AtlasCommand.ById([one, two]);
        Palette palette = DrawingFixtures.Palette();

        PngImage color = AtlasCommand.RenderPage(layout.Pages[0], layout, byId, palette);
        PngImage normal = NormalMap.RenderPage(layout.Pages[0], layout, byId, palette, new SortedDictionary<string, NormalOverride>());

        Assert.Equal(color.Width, normal.Width);
        Assert.Equal(color.Height, normal.Height);
        for (int index = 3; index < color.Pixels.Length; index += 4)
        {
            Assert.Equal(color.Pixels[index], normal.Pixels[index]);
        }
    }

    private static (int, int, int) Channels(NormalFrame normals, int x, int y) =>
        (normals.Channel(x, y, 0), normals.Channel(x, y, 1), normals.Channel(x, y, 2));
}
