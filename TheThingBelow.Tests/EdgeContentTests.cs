using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Edges;
using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The checks of the edge files and the edge rules across files (D-501, D-1321 to D-1327).</summary>
public sealed class EdgeContentTests
{
    /// <summary>A lake of water with a bridge, in grass, with a gorge of one tile.</summary>
    private static readonly string[] Lake =
    [
        ",,,,,,",
        ",--H-,",
        ",----,",
        ",,,,:,",
    ];

    [Fact]
    public void TheEdgeFileOfAMapLoadsWithItsRules()
    {
        EdgeContent content = Load(EdgeFixtures.FileText("""{ "x": 1, "y": 1, "pieces": ["edge.water_north", "edge.water_west"] }"""));

        EdgeFile file = content.EdgesOf(ContentId.Parse(EdgeFixtures.MapId, "test", "map"));
        Assert.Single(file.Tiles);
        Assert.Equal(2, new List<EdgeRule>(content.Rules).Count);
    }

    [Fact]
    public void AMapWithNoEdgeFileFails()
    {
        List<ContentFile> files = RuleFiles();

        ContentException error = Assert.Throws<ContentException>(() => EdgeContent.Load(files, Maps(), EdgeFixtures.Atlas(EdgeFixtures.AtlasText())));

        Assert.Equal("edge-test.json", error.File);
        Assert.Contains("each map has one", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEdgeFileOfAnAbsentMapFails()
    {
        string text = EdgeFixtures.FileText(string.Empty).Replace(EdgeFixtures.MapId, "map.absent", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => Load(text));

        Assert.Equal(EdgeFixtures.EdgePath, error.File);
        Assert.Equal("map", error.Field);
    }

    [Fact]
    public void TwoEdgeFilesOfOneMapFail()
    {
        List<ContentFile> files = RuleFiles();
        files.Add(EdgeFixtures.Of(EdgeFixtures.EdgePath, EdgeFixtures.FileText(string.Empty)));
        files.Add(EdgeFixtures.Of("edges/maps/second.json", EdgeFixtures.FileText(string.Empty)));

        ContentException error = Assert.Throws<ContentException>(() => EdgeContent.Load(files, Maps(), EdgeFixtures.Atlas(EdgeFixtures.AtlasText())));

        Assert.Equal("edges/maps/second.json", error.File);
        Assert.Contains("a map has one", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoRulesOfOneKindFail()
    {
        List<ContentFile> files = RuleFiles();
        files.Add(EdgeFixtures.Of("edges/kinds/water-again.json", EdgeFixtures.WaterRuleText));

        ContentException error = Assert.Throws<ContentException>(() => EdgeContent.Load(files, Maps(), EdgeFixtures.Atlas(EdgeFixtures.AtlasText())));

        Assert.Equal("edges/kinds/water-again.json", error.File);
        Assert.Contains("a kind has one", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("""{ "x": 6, "y": 1, "pieces": ["edge.water_north"] }""", "outside the map")]
    [InlineData("""{ "x": 0, "y": 0, "pieces": ["edge.water_north"] }""", "is a grass tile, and no edge rule")]
    [InlineData("""{ "x": 1, "y": 1, "pieces": ["edge.gorge_north"] }""", "the piece 'edge.gorge_north'")]
    [InlineData("""{ "x": 1, "y": 1, "pieces": ["edge.water_north", "edge.water_north"] }""", "one time at most")]
    public void ATileThatItsMapAndItsRuleDoNotAllowFailsWithTheFileAndTheTile(string tile, string reason)
    {
        ContentException error = Assert.Throws<ContentException>(() => Load(EdgeFixtures.FileText(tile)));

        Assert.Equal(EdgeFixtures.EdgePath, error.File);
        Assert.Equal("tiles[0]", error.Field);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void APieceWithNoDrawingFailsWithTheRuleAndThePlace()
    {
        List<ContentFile> files = RuleFiles();
        files.Add(EdgeFixtures.Of(EdgeFixtures.EdgePath, EdgeFixtures.FileText(string.Empty)));
        AtlasIndex atlas = EdgeFixtures.Atlas(EdgeFixtures.AtlasText(left: "edge.gorge_south_west"));

        ContentException error = Assert.Throws<ContentException>(() => EdgeContent.Load(files, Maps(), atlas));

        Assert.Equal(EdgeFixtures.GorgeRulePath, error.File);
        Assert.Equal("pieces.south_west", error.Field);
        Assert.Contains("'edge.gorge_south_west'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void APieceOffTheTilePageFailsWithTheRuleAndThePage()
    {
        // Game draws each piece on a layer of tiles, so a piece on another page draws nothing (D-667).
        List<ContentFile> files = RuleFiles();
        files.Add(EdgeFixtures.Of(EdgeFixtures.EdgePath, EdgeFixtures.FileText(string.Empty)));
        AtlasIndex atlas = EdgeFixtures.Atlas(EdgeFixtures.AtlasText(page: "pieces"));

        ContentException error = Assert.Throws<ContentException>(() => EdgeContent.Load(files, Maps(), atlas));

        Assert.Contains("the page 'pieces'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFileOutsideTheEdgeFoldersFails()
    {
        List<ContentFile> files = RuleFiles();
        files.Add(EdgeFixtures.Of("decor/maps/edge-test.json", EdgeFixtures.FileText(string.Empty)));

        ContentException error = Assert.Throws<ContentException>(() => EdgeContent.Load(files, Maps(), EdgeFixtures.Atlas(EdgeFixtures.AtlasText())));

        Assert.Equal("decor/maps/edge-test.json", error.File);
    }

    [Fact]
    public void TheEdgeFileOfAnAbsentMapGivesAnErrorWithTheMap()
    {
        EdgeContent content = Load(EdgeFixtures.FileText(string.Empty));

        ContentException error = Assert.Throws<ContentException>(() => content.EdgesOf(ContentId.Parse("map.absent", "test", "map")));

        Assert.Contains("'map.absent'", error.Message, StringComparison.Ordinal);
    }

    private static EdgeContent Load(string edgeText)
    {
        List<ContentFile> files = RuleFiles();
        files.Add(EdgeFixtures.Of(EdgeFixtures.EdgePath, edgeText));
        return EdgeContent.Load(files, Maps(), EdgeFixtures.Atlas(EdgeFixtures.AtlasText()));
    }

    private static List<ContentFile> RuleFiles() =>
    [
        EdgeFixtures.Of(EdgeFixtures.WaterRulePath, EdgeFixtures.WaterRuleText),
        EdgeFixtures.Of(EdgeFixtures.GorgeRulePath, EdgeFixtures.GorgeRuleText),
    ];

    private static SortedDictionary<string, GameMap> Maps() => new(StringComparer.Ordinal)
    {
        [EdgeFixtures.MapId] = EdgeFixtures.MapOf(Lake),
    };
}
