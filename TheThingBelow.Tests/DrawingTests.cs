using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The reader of a drawing file (D-515). Every error names the file, the frame, the row, and
/// the column, so a session finds the pixel that failed (T-2).
/// </summary>
public sealed class DrawingTests
{
    private const string Path = "sprites/drawings/cast/test.json";

    [Fact]
    public void AWellFormedDrawingReads()
    {
        Drawing drawing = Read(Body());

        Assert.Equal("drawing.test_map_front", drawing.Id.Value);
        Assert.Equal(AtlasPageKind.MapSprites, drawing.Page);
        Assert.Equal(4, drawing.Width);
        Assert.Equal(2, drawing.Height);
        Assert.Equal(Path, drawing.File);
        DrawingUse use = Assert.Single(drawing.Draws);
        Assert.Equal("cast.test", use.Content.Value);
        Assert.Equal("map_front", use.Use);
        DrawingFrame frame = Assert.Single(drawing.Frames);
        Assert.Equal(0, frame.Ticks);
        Assert.Equal(["k.k.", ".kk."], frame.Rows);
    }

    [Fact]
    public void AnAnimationKeepsTheTimeOfEachFrameInTicks()
    {
        Drawing drawing = Read(Body(frames: """
              { "ticks": 8, "rows": [ "k.k.", ".kk." ] },
              { "ticks": 4, "rows": [ ".kk.", "k.k." ] }
            """));

        Assert.Equal(2, drawing.Frames.Count);
        Assert.Equal(8, drawing.Frames[0].Ticks);
        Assert.Equal(4, drawing.Frames[1].Ticks);
    }

    [Fact]
    public void AnUnknownFieldFails()
    {
        ContentException error = Fails(Body().Replace("\"page\"", "\"sheet\""));

        Assert.Equal("sheet", error.Field);
        Assert.Contains("unknown field", error.Message);
    }

    [Fact]
    public void AnAbsentFieldFails()
    {
        ContentException error = Fails(Body(width: null));

        Assert.Equal("width", error.Field);
        Assert.Contains("absent", error.Message);
    }

    [Fact]
    public void AnIdOfAnotherKindFails()
    {
        ContentException error = Fails(Body(id: "tile.test"));

        Assert.Equal("id", error.Field);
        Assert.Contains("D-646", error.Message);
    }

    [Fact]
    public void APageThatNoKindNamesFails()
    {
        ContentException error = Fails(Body(page: "banners"));

        Assert.Equal("page", error.Field);
        Assert.Contains("banners", error.Message);
        Assert.Contains("map_sprites", error.Message);
    }

    [Fact]
    public void ASizeAboveAPageFails()
    {
        ContentException error = Fails(Body(width: AtlasPages.Size + 1));

        Assert.Equal("width", error.Field);
        Assert.Contains("D-666", error.Message);
    }

    /// <summary>A tile takes one cell of the strict grid, so no other size fits (D-667).</summary>
    [Fact]
    public void ATileOfAnotherSizeFails()
    {
        ContentException error = Fails(Body(page: "tiles"));

        Assert.Equal("width", error.Field);
        Assert.Contains("D-667", error.Message);
    }

    [Fact]
    public void ARowOfTheWrongLengthFailsWithTheFrameAndTheRow()
    {
        ContentException error = Fails(Body(frames: """
              { "ticks": 0, "rows": [ "k.k.", ".kk" ] }
            """));

        Assert.Equal("frames[0].rows[1]", error.Field);
        Assert.Contains("3 keys", error.Message);
        Assert.Contains("width of the drawing is 4", error.Message);
    }

    [Fact]
    public void AFrameOfTheWrongHeightFails()
    {
        ContentException error = Fails(Body(frames: """
              { "ticks": 0, "rows": [ "k.k." ] }
            """));

        Assert.Equal("frames[0].rows", error.Field);
        Assert.Contains("1 rows", error.Message);
    }

    [Fact]
    public void AKeyThatIsNotOnePrintableCharacterFailsWithTheColumn()
    {
        ContentException error = Fails(Body(frames: """
              { "ticks": 0, "rows": [ "k k.", ".kk." ] }
            """));

        Assert.Equal("frames[0].rows[0]", error.Field);
        Assert.Contains("column 1", error.Message);
        Assert.Contains("D-515", error.Message);
    }

    /// <summary>One frame never advances, so a time on it would say nothing (D-669).</summary>
    [Fact]
    public void ATimeOnADrawingOfOneFrameFails()
    {
        ContentException error = Fails(Body(frames: """
              { "ticks": 6, "rows": [ "k.k.", ".kk." ] }
            """));

        Assert.Equal("frames[0].ticks", error.Field);
        Assert.Contains("D-669", error.Message);
    }

    [Fact]
    public void AFrameOfAnAnimationWithNoTimeFails()
    {
        ContentException error = Fails(Body(frames: """
              { "ticks": 8, "rows": [ "k.k.", ".kk." ] },
              { "ticks": 0, "rows": [ ".kk.", "k.k." ] }
            """));

        Assert.Equal("frames[1].ticks", error.Field);
        Assert.Contains("1 tick at least", error.Message);
    }

    [Fact]
    public void ATimeBelowZeroFails()
    {
        ContentException error = Fails(Body(frames: """
              { "ticks": -1, "rows": [ "k.k.", ".kk." ] }
            """));

        Assert.Equal("frames[0].ticks", error.Field);
        Assert.Contains("below zero", error.Message);
    }

    [Fact]
    public void ADrawingThatDrawsNothingFails()
    {
        ContentException error = Fails(Body(draws: string.Empty));

        Assert.Equal("draws", error.Field);
        Assert.Contains("D-519", error.Message);
    }

    [Fact]
    public void ThatSameThingAndUseTwoTimesFails()
    {
        ContentException error = Fails(Body(draws: """
              { "content": "cast.test", "use": "map_front" },
              { "content": "cast.test", "use": "map_front" }
            """));

        Assert.Equal("draws[1]", error.Field);
        Assert.Contains("draws[0]", error.Message);
    }

    [Fact]
    public void AUseThatIsNotALowercaseNameFails()
    {
        ContentException error = Fails(Body(draws: """
              { "content": "cast.test", "use": "Map Front" }
            """));

        Assert.Equal("draws[0].use", error.Field);
        Assert.Contains("D-519", error.Message);
    }

    [Fact]
    public void AFileOfTheDrawingFolderIsADrawingFile()
    {
        Assert.True(Drawing.IsDrawingFile("sprites/drawings/cast/marrek-map-front.json"));
        Assert.False(Drawing.IsDrawingFile("sprites/palette.json"));
    }

    private static Drawing Read(string body) => Drawing.Read(Encoding.UTF8.GetBytes(body), Path);

    private static ContentException Fails(string body) =>
        Assert.Throws<ContentException>(() => Read(body));

    private static string Body(
        string id = "drawing.test_map_front",
        string page = "map_sprites",
        int? width = 4,
        string? draws = null,
        string? frames = null)
    {
        string size = width is null ? string.Empty : $" \"width\": {width},\n";
        draws ??= """
              { "content": "cast.test", "use": "map_front" }
            """;
        frames ??= """
              { "ticks": 0, "rows": [ "k.k.", ".kk." ] }
            """;

        return $$"""
            {
             "id": "{{id}}",
             "page": "{{page}}",
            {{size}} "height": 2,
             "draws": [
            {{draws}}
             ],
             "frames": [
            {{frames}}
             ]
            }
            """;
    }
}
