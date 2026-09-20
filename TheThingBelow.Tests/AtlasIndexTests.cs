using System.Text;
using TheThingBelow.Core.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The reader of the atlas index (D-666). Game finds each frame through the index and the
/// content ids of D-519.
/// </summary>
public sealed class AtlasIndexTests
{
    [Fact]
    public void AWellFormedIndexReads()
    {
        AtlasIndex index = Read(Body());

        AtlasPage page = Assert.Single(index.Pages);
        Assert.Equal(AtlasPageKind.MapSprites, page.Kind);
        Assert.Equal("map_sprites", page.Name);
        Assert.Equal("sprites/atlas-map_sprites.png", page.File);
        AtlasEntry entry = Assert.Single(index.Entries);
        Assert.Equal("drawing.test_map_front", entry.Id.Value);
        Assert.Equal(64, Assert.Single(entry.Frames).X);
    }

    [Fact]
    public void ASecondPageOfAKindTakesItsNumberInItsName()
    {
        Assert.Equal("tiles", AtlasPage.NameOf(AtlasPageKind.Tiles, 1));
        Assert.Equal("tiles-2", AtlasPage.NameOf(AtlasPageKind.Tiles, 2));
        Assert.Equal("sprites/atlas-tiles-2.png", new AtlasPage(AtlasPageKind.Tiles, 2, 32, 32).File);
    }

    [Fact]
    public void ADrawingReadsByItsId()
    {
        AtlasIndex index = Read(Body());

        AtlasEntry entry = index.Entry(Id("drawing.test_map_front"));

        Assert.Equal("map_sprites", entry.Page);
    }

    /// <summary>Game finds a drawing by the thing that it draws and the use (D-519).</summary>
    [Fact]
    public void ADrawingReadsByTheThingThatItDraws()
    {
        AtlasIndex index = Read(Body());

        Assert.True(index.Draws(Id("cast.test"), "map_front"));
        Assert.False(index.Draws(Id("cast.test"), "portrait"));
        Assert.Equal("drawing.test_map_front", index.Entry(Id("cast.test"), "map_front").Id.Value);
    }

    [Fact]
    public void AUseThatNoDrawingServesFails()
    {
        AtlasIndex index = Read(Body());

        ContentException error = Assert.Throws<ContentException>(
            () => index.Entry(Id("cast.test"), "portrait"));

        Assert.Contains("cast.test", error.Message);
        Assert.Contains("D-519", error.Message);
    }

    [Fact]
    public void AnIdThatTheIndexLacksFails()
    {
        AtlasIndex index = Read(Body());

        ContentException error = Assert.Throws<ContentException>(
            () => index.Entry(Id("drawing.absent")));

        Assert.Equal("drawing.absent", error.Field);
    }

    [Fact]
    public void APageInTheFileTwoTimesFails()
    {
        ContentException error = Fails(Body(pages: """
              { "kind": "map_sprites", "number": 1, "width": 96, "height": 32 },
              { "kind": "map_sprites", "number": 1, "width": 96, "height": 32 }
            """));

        Assert.Equal("pages", error.Field);
        Assert.Contains("two times", error.Message);
    }

    [Fact]
    public void AnEntryThatNamesAnAbsentPageFails()
    {
        ContentException error = Fails(Body(page: "portraits"));

        Assert.Equal("drawings[0].page", error.Field);
        Assert.Contains("portraits", error.Message);
    }

    [Fact]
    public void AFrameThatLeavesThePageFails()
    {
        ContentException error = Fails(Body(x: 80));

        Assert.Equal("drawings[0].frames[0]", error.Field);
        Assert.Contains("leaves the page", error.Message);
    }

    [Fact]
    public void AnIdInTheFileTwoTimesFails()
    {
        ContentException error = Fails(Body(second: """
             ,{
              "id": "drawing.test_map_front",
              "page": "map_sprites",
              "width": 32,
              "height": 32,
              "draws": [ { "content": "cast.other", "use": "map_front" } ],
              "frames": [ { "x": 0, "y": 0, "ticks": 0 } ]
             }
            """));

        Assert.Equal("drawings[1].id", error.Field);
        Assert.Contains("two times", error.Message);
    }

    /// <summary>One thing and one use take one drawing, so Game never picks between two (D-519).</summary>
    [Fact]
    public void TwoDrawingsOfOneThingAndOneUseFail()
    {
        ContentException error = Fails(Body(second: """
             ,{
              "id": "drawing.other_map_front",
              "page": "map_sprites",
              "width": 32,
              "height": 32,
              "draws": [ { "content": "cast.test", "use": "map_front" } ],
              "frames": [ { "x": 0, "y": 0, "ticks": 0 } ]
             }
            """));

        Assert.Equal("drawings[1].draws[0]", error.Field);
        Assert.Contains("drawing.test_map_front", error.Message);
    }

    [Fact]
    public void AKindThatNoPageNamesFails()
    {
        ContentException error = Fails(Body(pages: """
              { "kind": "banners", "number": 1, "width": 96, "height": 32 }
            """));

        Assert.Equal("pages[0].kind", error.Field);
        Assert.Contains("banners", error.Message);
    }

    private static ContentId Id(string value) => ContentId.Parse(value, AtlasIndex.Path, "id");

    private static AtlasIndex Read(string body) =>
        AtlasIndex.Read(Encoding.UTF8.GetBytes(body), AtlasIndex.Path);

    private static ContentException Fails(string body) =>
        Assert.Throws<ContentException>(() => Read(body));

    private static string Body(
        string? pages = null,
        string page = "map_sprites",
        int x = 64,
        string second = "")
    {
        pages ??= """
              { "kind": "map_sprites", "number": 1, "width": 96, "height": 32 }
            """;

        return $$"""
            {
             "comment": "a test index",
             "pages": [
            {{pages}}
             ],
             "drawings": [
              {
               "id": "drawing.test_map_front",
               "page": "{{page}}",
               "width": 32,
               "height": 32,
               "draws": [ { "content": "cast.test", "use": "map_front" } ],
               "frames": [ { "x": {{x}}, "y": 0, "ticks": 0 } ]
              }
            {{second}}
             ]
            }
            """;
    }
}
