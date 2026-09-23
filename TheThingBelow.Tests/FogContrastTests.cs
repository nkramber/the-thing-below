using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The contrast test of the fog: the luma gap of an outline color and a floor color (D-885, D-886, D-892).</summary>
public sealed class FogContrastTests
{
    /// <summary>A palette of five colors: black, white, a middle gray, and two grays near the white.</summary>
    private const string PaletteBody =
        """
        {
         "comment": "a test palette",
         "colors": [
          { "index": 0, "key": "k", "hex": "000000", "name": "black", "height": 0 },
          { "index": 1, "key": "w", "hex": "ffffff", "name": "white", "height": 0 },
          { "index": 2, "key": "g", "hex": "808080", "name": "gray", "height": 0 },
          { "index": 3, "key": "l", "hex": "e0e0e0", "name": "palegray", "height": 0 },
          { "index": 4, "key": "L", "hex": "f0f0f0", "name": "palergray", "height": 0 }
         ]
        }
        """;

    [Fact]
    public void TheLumaOfAColorFollowsRecSixHundredAndOne()
    {
        Assert.Equal(0, FogContrast.Luma(0, 0, 0));
        Assert.Equal(255, FogContrast.Luma(255, 255, 255));
        Assert.Equal(76, FogContrast.Luma(255, 0, 0));
    }

    [Fact]
    public void AFogMovesAColorTowardTheColorOfTheFog()
    {
        Assert.Equal(0, FogContrast.Blend(0, 255, 0));
        Assert.Equal(128, FogContrast.Blend(0, 255, 5000));
        Assert.Equal(255, FogContrast.Blend(0, 255, 10000));
    }

    [Fact]
    public void AFogInsideTheFloorOfTheGapPasses()
    {
        // D-892: black on white keeps a gap of 255, and a fog of 800 basis points takes little.
        Assert.Null(FogContrast.FirstFault(Fog(800), Keys("k"), Keys("w"), Palette()));
    }

    [Fact]
    public void AFogThatPullsAGapBelowTheFloorFails()
    {
        // D-892: the fog shrinks each gap by its strength, so a thick fog hides the outline.
        FogFault fault = Assert.IsType<FogFault>(FogContrast.FirstFault(Fog(9600), Keys("k"), Keys("w"), Palette()));

        Assert.Equal('k', fault.Outline);
        Assert.Equal('w', fault.Floor);
        Assert.True(fault.Gap < FogContrast.LeastGap, $"the gap of the fault is {fault.Gap}");
    }

    [Fact]
    public void AGapThatTheArtHoldsBelowTheFloorTakesNoTest()
    {
        // D-892: the art sets that gap, and not the fog. The two pale grays hold a gap of 16.
        Assert.Null(FogContrast.FirstFault(Fog(8000), Keys("L"), Keys("l"), Palette()));
    }

    [Fact]
    public void TheTestReadsTheOutlineKeyWithTheLargestGap()
    {
        // D-892: one outline key that stands out on a floor color is enough.
        Assert.Null(FogContrast.FirstFault(Fog(2000), Keys("gk"), Keys("w"), Palette()));

        FogFault fault = Assert.IsType<FogFault>(FogContrast.FirstFault(Fog(9500), Keys("gk"), Keys("w"), Palette()));
        Assert.Equal('k', fault.Outline);
    }

    [Fact]
    public void TheOutlineOfADrawingIsEachKeyBesideAClearPixelOrTheEdge()
    {
        // D-201: each material is outlined in a dark shade of itself.
        Drawing drawing = Drawing.Read(
            Encoding.UTF8.GetBytes(
                """
                {
                 "id": "drawing.test_body",
                 "page": "map_sprites",
                 "width": 5,
                 "height": 5,
                 "draws": [ { "content": "patrol.test", "use": "map_front" } ],
                 "frames": [ { "ticks": 0, "rows": [ ".....", ".kkk.", ".kwk.", ".kkk.", "....." ] } ]
                }
                """),
            Drawing.Folder + "test-body.json");

        Assert.Equal(Keys("k"), FogContrast.OutlineOf(drawing));
        Assert.Equal(Keys("kw"), FogContrast.KeysOf(drawing));
    }

    private static FogLayer Fog(int strength) =>
        new('w', 5000, 6000, strength, 32, 0, 0, 0);

    private static SortedSet<char> Keys(string keys) => new(keys);

    private static Palette Palette() => Core.Content.Palette.Read(Encoding.UTF8.GetBytes(PaletteBody), Core.Content.Palette.Path);
}
