using System;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Edges;
using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The strict reader of an edge file (D-501, D-1326).</summary>
public sealed class EdgeFileTests
{
    [Fact]
    public void AFileReadsEachTileWithItsPiecesInOrder()
    {
        EdgeFile file = EdgeFixtures.File(EdgeFixtures.FileText("""
            { "x": 3, "y": 1, "pieces": ["edge.water_north", "edge.water_west"] },
            { "x": 0, "y": 2, "pieces": ["edge.water_south_east"] }
            """));

        Assert.Equal(EdgeFixtures.MapId, file.Map.Value);
        Assert.Equal(2, file.Tiles.Count);
        Assert.Equal(new TilePoint(3, 1), file.Tiles[0].At);
        Assert.Equal(["edge.water_north", "edge.water_west"], [file.Tiles[0].Pieces[0].Value, file.Tiles[0].Pieces[1].Value]);
        Assert.Equal(new TilePoint(0, 2), file.Tiles[1].At);
    }

    [Fact]
    public void AFileWithNoTileReadsAsAnEmptyList()
    {
        EdgeFile file = EdgeFixtures.File(EdgeFixtures.FileText(string.Empty));

        Assert.Empty(file.Tiles);
    }

    [Theory]
    [InlineData("""{ "x": 1, "y": 1, "pieces": [] }""", "[0].pieces", "1 to 4")]
    [InlineData("""{ "x": 1, "y": 1, "pieces": ["edge.a", "edge.b", "edge.c", "edge.d", "edge.e"] }""", "[0].pieces", "1 to 4")]
    [InlineData("""{ "x": 1, "y": 1, "pieces": ["tile.water"] }""", "[0].pieces[0]", "the kind 'edge'")]
    [InlineData("""{ "x": 1, "pieces": ["edge.a"] }""", "[0].y", "")]
    [InlineData("""{ "x": 2, "y": 1, "pieces": ["edge.a"] }, { "x": 1, "y": 1, "pieces": ["edge.a"] }""", "[1]", "the order of the rows")]
    [InlineData("""{ "x": 1, "y": 1, "pieces": ["edge.a"] }, { "x": 1, "y": 1, "pieces": ["edge.a"] }""", "[1]", "one time each")]
    [InlineData("""{ "x": 1, "y": 2, "pieces": ["edge.a"] }, { "x": 5, "y": 1, "pieces": ["edge.a"] }""", "[1]", "the order of the rows")]
    public void AWrongTileFailsWithTheFileAndTheTile(string tiles, string field, string reason)
    {
        ContentException error = Assert.Throws<ContentException>(() => EdgeFixtures.File(EdgeFixtures.FileText(tiles)));

        Assert.Equal(EdgeFixtures.EdgePath, error.File);
        Assert.Equal($"tiles{field}", error.Field);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFileWithNoMapFails()
    {
        string text = EdgeFixtures.FileText(string.Empty).Replace($" \"map\": \"{EdgeFixtures.MapId}\",\n", string.Empty, StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => EdgeFixtures.File(text));

        Assert.Equal(EdgeFixtures.EdgePath, error.File);
    }

    [Fact]
    public void AnEdgePathLiesInTheFolderOfTheEdgeFiles()
    {
        Assert.True(EdgeFile.IsEdgeFile("edges/maps/overworld.json"));
        Assert.False(EdgeFile.IsEdgeFile("edges/kinds/water.json"));
        Assert.False(EdgeFile.IsEdgeFile("decor/maps/overworld.json"));
    }
}
