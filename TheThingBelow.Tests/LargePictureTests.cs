using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheThingBelow.Core.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The format of a large picture, the checks of its pieces over the content set, and the copies
/// that the render and the draw read (D-516, D-812, D-816 to D-818).
/// </summary>
public sealed class LargePictureTests
{
    [Fact]
    public void AWellFormedPictureReadsItsSizeAndItsPlaces()
    {
        LargePicture picture = Read(PictureFixtures.PictureBody());

        Assert.Equal(PictureFixtures.PictureId, picture.Id.Value);
        Assert.Equal(6, picture.Width);
        Assert.Equal(4, picture.Height);
        Assert.Equal(3, picture.Places.Count);
        PicturePlace place = picture.Places[1];
        Assert.Equal(PictureFixtures.PieceA, place.Piece.Value);
        Assert.Equal((0, 1, 3, 2), (place.X, place.Y, place.Across, place.Down));
    }

    [Theory]
    [InlineData("id")]
    [InlineData("width")]
    [InlineData("height")]
    [InlineData("places")]
    public void AnAbsentFieldFailsWithItsName(string field)
    {
        var fields = new List<string>
        {
            $"\"id\": \"{PictureFixtures.PictureId}\"",
            "\"width\": 6",
            "\"height\": 4",
            $"\"places\": [ {PictureFixtures.DefaultPlaces} ]",
        };
        fields.RemoveAll(text => text.StartsWith($"\"{field}\"", StringComparison.Ordinal));

        ContentException error = Assert.Throws<ContentException>(() => Read($"{{ {string.Join(", ", fields)} }}"));

        Assert.Contains(field, error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(8193)]
    public void ASizeOutsideOneTo8192Fails(int width)
    {
        ContentException error = Assert.Throws<ContentException>(() => Read(PictureFixtures.PictureBody(width: width)));

        Assert.Contains("D-816", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("\"across\": 0, \"down\": 1")]
    [InlineData("\"across\": 1, \"down\": 0")]
    public void ACountBelowOneFails(string counts)
    {
        string places = $$"""{ "piece": "drawing.piece_a", "x": 0, "y": 0, {{counts}} }""";

        ContentException error = Assert.Throws<ContentException>(() => Read(PictureFixtures.PictureBody(places)));

        Assert.Contains("D-817", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void APictureWithNoPlaceFails()
    {
        ContentException error = Assert.Throws<ContentException>(() => Read(PictureFixtures.PictureBody(string.Empty)));

        Assert.Equal("places", error.Field);
    }

    [Theory]
    [InlineData(6, 0)]
    [InlineData(0, 4)]
    [InlineData(-1, 0)]
    public void AFirstCopyThatStartsOutsideThePictureFails(int x, int y)
    {
        // D-817: a copy that starts outside the picture draws nothing, so the load refuses it.
        string places = $$"""{ "piece": "drawing.piece_a", "x": {{x}}, "y": {{y}}, "across": 1, "down": 1 }""";

        ContentException error = Assert.Throws<ContentException>(() => Read(PictureFixtures.PictureBody(places)));

        Assert.Equal("places[0]", error.Field);
        Assert.Equal(PictureFixtures.PicturePath, error.File);
    }

    [Fact]
    public void AMirrorFieldIsUnknown()
    {
        // D-812: a picture offers a place and a repeat alone.
        string places = """{ "piece": "drawing.piece_a", "x": 0, "y": 0, "across": 1, "down": 1, "mirror": true }""";

        ContentException error = Assert.Throws<ContentException>(() => Read(PictureFixtures.PictureBody(places)));

        Assert.Contains("mirror", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void APieceIdOfAnotherKindFails()
    {
        string places = """{ "piece": "tile.floor", "x": 0, "y": 0, "across": 1, "down": 1 }""";

        Assert.Throws<ContentException>(() => Read(PictureFixtures.PictureBody(places)));
    }

    [Fact]
    public void TheContentSetHoldsThePicture()
    {
        ContentSet set = ContentSet.Load(PictureFixtures.Files());

        LargePicture picture = set.PictureOf(PictureFixtures.Id(PictureFixtures.PictureId));

        Assert.Equal(PictureFixtures.PicturePath, picture.File);
        Assert.Single(set.Pictures);
    }

    [Fact]
    public void AnIdThatNoPictureHoldsFails()
    {
        ContentSet set = ContentSet.Load(PictureFixtures.Files());

        ContentException error = Assert.Throws<ContentException>(() => set.PictureOf(PictureFixtures.Id("picture.absent")));

        Assert.Equal("picture.absent", error.Field);
    }

    [Fact]
    public void AnAbsentPieceFailsWithThePictureAndTheEntry()
    {
        string places = """{ "piece": "drawing.piece_absent", "x": 0, "y": 0, "across": 1, "down": 1 }""";

        ContentException error = Assert.Throws<ContentException>(
            () => ContentSet.Load(PictureFixtures.Files(PictureFixtures.PictureBody(places))));

        Assert.Equal(PictureFixtures.PicturePath, error.File);
        Assert.Equal("places[0]", error.Field);
        Assert.Contains("drawing.piece_absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void APieceOnAnotherPageFails()
    {
        // D-818: the window frame of the UI base lies on the page `ui`.
        string places = $$"""{ "piece": "{{UiContentFixtures.WindowDrawingId}}", "x": 0, "y": 0, "across": 1, "down": 1 }""";

        ContentException error = Assert.Throws<ContentException>(
            () => ContentSet.Load(PictureFixtures.Files(PictureFixtures.PictureBody(places))));

        Assert.Equal("places[0]", error.Field);
        Assert.Contains("D-818", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void APieceOfTwoFramesFails()
    {
        // D-818: a backdrop moves by the drift of its layer, and a piece holds one frame.
        const string moving = """
            {
             "id": "drawing.piece_moving",
             "page": "pieces",
             "width": 1,
             "height": 1,
             "draws": [ { "content": "picture.test", "use": "moving" } ],
             "frames": [ { "ticks": 5, "rows": [ "k" ] }, { "ticks": 5, "rows": [ "w" ] } ]
            }
            """;
        const string entry = """
            {
             "id": "drawing.piece_moving",
             "page": "pieces",
             "width": 1,
             "height": 1,
             "draws": [ { "content": "picture.test", "use": "moving" } ],
             "frames": [ { "x": 0, "y": 1, "ticks": 5 }, { "x": 1, "y": 1, "ticks": 5 } ]
            },
            """;
        string places = """{ "piece": "drawing.piece_moving", "x": 0, "y": 0, "across": 1, "down": 1 }""";

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(PictureFixtures.Files(
            PictureFixtures.PictureBody(places),
            entry,
            PictureFixtures.File("sprites/drawings/pieces/moving.json", moving))));

        Assert.Equal("places[0]", error.Field);
        Assert.Contains("2 frames", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0, 0, 4, 1)]
    [InlineData(0, 0, 1, 3)]
    [InlineData(5, 0, 2, 1)]
    public void ALastCopyThatStartsOutsideThePictureFails(int x, int y, int across, int down)
    {
        // D-817. Piece A is 2 by 2, so in a picture of 6 by 4 a fourth copy across starts at
        // 6, a third copy down starts at 4, and a second copy from column 5 starts at 7.
        string places = $$"""{ "piece": "drawing.piece_a", "x": {{x}}, "y": {{y}}, "across": {{across}}, "down": {{down}} }""";

        ContentException error = Assert.Throws<ContentException>(
            () => ContentSet.Load(PictureFixtures.Files(PictureFixtures.PictureBody(places))));

        Assert.Equal("places[0]", error.Field);
        Assert.Contains("last copy", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoPicturesOfOneIdFail()
    {
        // D-166: an id is permanent, so no two files take one.
        ContentFile second = PictureFixtures.File("sprites/pictures/second.json", PictureFixtures.PictureBody());

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(PictureFixtures.Files(null, string.Empty, second)));

        Assert.Contains("D-166", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCopiesComeInTheOrderOfTheEntriesThenRowByRow()
    {
        ContentSet set = ContentSet.Load(PictureFixtures.Files());
        LargePicture picture = set.PictureOf(PictureFixtures.Id(PictureFixtures.PictureId));

        IReadOnlyList<PictureCopy> copies = PictureCopies.Of(picture, set);

        string[] places = copies.Select(copy => $"{copy.Piece.Name} {copy.X},{copy.Y} {copy.Width}x{copy.Height}").ToArray();
        Assert.Equal(
            [
                "piece_b 0,0 3x1", "piece_b 3,0 3x1",
                "piece_a 0,1 2x2", "piece_a 2,1 2x2", "piece_a 4,1 2x2",
                "piece_a 0,3 2x1", "piece_a 2,3 2x1", "piece_a 4,3 2x1",
                "piece_b 0,3 3x1",
            ],
            places);
    }

    [Fact]
    public void ACopyAtTheRightEdgeKeepsThePartInsideThePicture()
    {
        // D-817: the picture clips a copy at its edge.
        string places = """{ "piece": "drawing.piece_b", "x": 4, "y": 0, "across": 1, "down": 1 }""";
        ContentSet set = ContentSet.Load(PictureFixtures.Files(PictureFixtures.PictureBody(places)));

        PictureCopy copy = Assert.Single(PictureCopies.Of(set.PictureOf(PictureFixtures.Id(PictureFixtures.PictureId)), set));

        Assert.Equal(2, copy.Width);
        Assert.Equal(1, copy.Height);
    }

    private static LargePicture Read(string body) =>
        LargePicture.Read(Encoding.UTF8.GetBytes(body), PictureFixtures.PicturePath);
}
