using System.Text;
using TheThingBelow.Core.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The reader of an override grid, and its check against its drawing (D-839). A grid holds a
/// numpad digit or the dot for each pixel, and it fits its drawing frame by frame.
/// </summary>
public sealed class NormalOverrideTests
{
    [Fact]
    public void AWellFormedGridReads()
    {
        NormalOverride grid = DrawingFixtures.Override("block", ["8.", ".5"]);

        Assert.Equal("drawing.block", grid.Drawing.Value);
        Assert.Equal(DrawingFixtures.OverridePathOf("block"), grid.File);
        Assert.Equal(["8.", ".5"], grid.Frames[0].Rows);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("a")]
    [InlineData("k")]
    public void ACharacterOtherThanADigitOrTheDotFails(string character)
    {
        ContentException error = Assert.Throws<ContentException>(
            () => DrawingFixtures.Override("block", [$"8{character}"]));

        Assert.Contains("column 1", error.Message);
        Assert.Contains("D-839", error.Message);
    }

    [Fact]
    public void AnEmptyListOfFramesFails()
    {
        const string body = """{ "drawing": "drawing.block", "frames": [] }""";

        ContentException error = Assert.Throws<ContentException>(
            () => NormalOverride.Read(Encoding.UTF8.GetBytes(body), "sprites/normals/block.json"));

        Assert.Equal("frames", error.Field);
    }

    [Fact]
    public void AnUnknownFieldFails()
    {
        const string body = """{ "drawing": "drawing.block", "frames": [ { "rows": [ "5" ] } ], "note": "x" }""";

        Assert.Throws<ContentException>(
            () => NormalOverride.Read(Encoding.UTF8.GetBytes(body), "sprites/normals/block.json"));
    }

    [Fact]
    public void AGridOfAnotherHeightFails()
    {
        Drawing drawing = DrawingFixtures.FromRows("block", ["kk", "kk"]);
        NormalOverride grid = DrawingFixtures.Override("block", ["55"]);

        ContentException error = Assert.Throws<ContentException>(() => grid.RefuseWrongShape(drawing));

        Assert.Equal("frames[0].rows", error.Field);
        Assert.Contains("1 rows", error.Message);
    }

    [Fact]
    public void ARowOfAnotherWidthFails()
    {
        Drawing drawing = DrawingFixtures.FromRows("block", ["kk", "kk"]);
        NormalOverride grid = DrawingFixtures.Override("block", ["55", "555"]);

        ContentException error = Assert.Throws<ContentException>(() => grid.RefuseWrongShape(drawing));

        Assert.Equal("frames[0].rows[1]", error.Field);
        Assert.Contains("3 pixels", error.Message);
    }

    [Fact]
    public void AGridOfAnotherFrameCountFails()
    {
        Drawing drawing = DrawingFixtures.Solid("block", width: 2, height: 2, frames: 2);
        NormalOverride grid = DrawingFixtures.Override("block", ["55", "55"]);

        ContentException error = Assert.Throws<ContentException>(() => grid.RefuseWrongShape(drawing));

        Assert.Equal("frames", error.Field);
        Assert.Contains("1 frames", error.Message);
    }

    /// <summary>An icon takes no scene light, so it has no normal map to correct (D-210).</summary>
    [Fact]
    public void AGridOfADrawingThatTakesNoLightFails()
    {
        Drawing icon = DrawingFixtures.FromRows("block", ["kk", "kk"], page: "ui");
        NormalOverride grid = DrawingFixtures.Override("block", ["55", "55"]);

        ContentException error = Assert.Throws<ContentException>(() => grid.RefuseWrongShape(icon));

        Assert.Contains("D-210", error.Message);
    }
}
