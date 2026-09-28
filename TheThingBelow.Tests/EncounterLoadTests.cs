using System;
using System.Collections.Generic;
using System.Linq;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The load of the zones of an overworld: the zone list, the zone grid, the groups of the region,
/// and the flags of each condition (exit test 5 of PR-109, D-1250, D-1251, D-1262, D-1269).
/// </summary>
public sealed class EncounterLoadTests
{
    private const string File = "test-overworld.json";

    private const string OverworldPath = "rules/maps/fixture-overworld.json";

    private const string OneGroup = """[{ "group": "group.one", "weight": 1 }]""";

    /// <summary>A dungeon of one room, which holds no zone.</summary>
    private const string DungeonText = """
        {
         "comment": "A dungeon for the load tests of PR-109.",
         "id": "map.test_cellar",
         "region": "region.test",
         "label": "label.test_cellar",
         "kind": "dungeon",
         "time": "day",
         "dark": false,
         "terrain": [
          "#####",
          "#...#",
          "#####"
         ],
         "things": [
          { "id": "spawn_point.test_cellar_start", "kind": "spawn_point", "x": 1, "y": 1 }
         ],
         "enemies": [], "npcs": [], "services": [], "zones": [], "zone_grid": [], "reopen": [], "triggers": []
        }
        """;

    [Fact]
    public void TheTestOverworldReadsEachZoneOfItsGrid()
    {
        GameMap map = EncounterMaps.Of(100, """[{ "group": "group.one", "weight": 3 }, { "group": "group.other", "weight": 1 }]""");

        Assert.Equal(["zone.test_road", "zone.test_wild"], [map.Zones[0].Id.Value, map.Zones[1].Id.Value]);
        Assert.Equal("zone.test_road", map.ZoneAt(new TilePoint(1, 2))?.Id.Value);
        EncounterZone wild = Assert.IsType<EncounterZone>(map.ZoneAt(new TilePoint(7, 3)));
        Assert.Equal(("zone.test_wild", 'z', 100, 4), (wild.Id.Value, wild.Key, wild.Rate, wild.TotalWeight));
        Assert.Null(map.ZoneAt(new TilePoint(0, 0)));
        Assert.Null(map.ZoneAt(new TilePoint(-1, 2)));
        Assert.Empty(OverworldMaps.Place.Zones);
        Assert.Null(OverworldMaps.Place.ZoneAt(new TilePoint(1, 1)));
    }

    [Theory]
    [InlineData("""[{ "group": "group.one", "weight": 0 }]""", "group.one")]
    [InlineData("""[{ "group": "group.one", "weight": 10001 }]""", "group.one")]
    [InlineData("""[{ "group": "group.one", "weight": 1 }, { "group": "group.one", "weight": 2 }]""", "group.one")]
    [InlineData("""[{ "group": "group.one" }]""", "weight")]
    [InlineData("[]", "zone.test_wild")]
    public void AZoneWithABadListOfGroupsIsAnError(string groups, string named)
    {
        // Exit test 5 of PR-109 (D-1250): a weight of zero, a weight past the limit, a group two
        // times, an absent weight, and no group on a live zone each fail the load with the file.
        ContentException error = Assert.Throws<ContentException>(() => EncounterMaps.Of(100, groups));

        Assert.Contains(File, error.Message, StringComparison.Ordinal);
        Assert.Contains(named, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AZoneThatNamesAnAbsentGroupNamesTheFileAndTheId()
    {
        // Exit test 5 of PR-109 (D-957, D-1250): the group file of the region holds each group.
        GameMap map = EncounterMaps.Of(100, """[{ "group": "group.absent", "weight": 1 }]""");

        ContentException error = Assert.Throws<ContentException>(() => TestBattles.Content.RequireGroupsOf(map));

        Assert.Contains(File, error.Message, StringComparison.Ordinal);
        Assert.Contains("zone.test_wild", error.Message, StringComparison.Ordinal);
        Assert.Contains("group.absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AZoneOfTheCheckoutThatNamesAnAbsentGroupFailsTheContentSet()
    {
        // Exit test 5 of PR-109, on the content set of the checkout.
        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(Changed(
            OverworldPath,
            """ "groups": [{ "group": "group.fixture_pair", "weight": 1 }]""",
            """ "groups": [{ "group": "group.absent", "weight": 1 }]""")));

        Assert.Contains(OverworldPath, error.Message, StringComparison.Ordinal);
        Assert.Contains("zone.fixture_overworld_grass", error.Message, StringComparison.Ordinal);
        Assert.Contains("group.absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AZoneWithAnUndeclaredFlagNamesTheFileAndTheZone()
    {
        // D-543, D-1251: the zone joins the readers of the one condition form.
        GameMap map = EncounterMaps.Read(EncounterMaps.TextOf(EncounterMaps.FlaggedZone(100).Replace(EncounterMaps.ZoneFlag, "flag.absent", StringComparison.Ordinal)));

        ContentException error = Assert.Throws<ContentException>(() => TestStory.Content.RequireGatesAndZonesOf(map));

        Assert.Contains(File, error.Message, StringComparison.Ordinal);
        Assert.Contains("zones.zone.test_wild.condition", error.Message, StringComparison.Ordinal);
        Assert.Contains("flag.absent", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10001)]
    public void ARateOutsideItsRangeIsAnError(int rate)
    {
        // D-1261: a rate is 0 to 10000 basis points.
        ContentException error = Assert.Throws<ContentException>(() => EncounterMaps.Of(rate, OneGroup));

        Assert.Contains("rate", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-1261", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AZoneAtRateZeroWithAGroupIsAnError()
    {
        // D-1263: a zone at rate 0 starts no fight, so a group on it reads like a mistake.
        ContentException error = Assert.Throws<ContentException>(() => EncounterMaps.Of(0, OneGroup));

        Assert.Contains("zone.test_wild", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-1263", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AZoneWithNoConditionIsAnError()
    {
        // D-1002, D-1269: each reader of a condition holds the field, and the always leaf marks a zone that always runs.
        string zone = EncounterMaps.Zone(100, OneGroup).Replace(", \"condition\": { \"always\": true }", string.Empty, StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => EncounterMaps.Read(EncounterMaps.TextOf(zone)));

        Assert.Contains("condition", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("zz", "D-1262")]
    [InlineData(".", "D-1262")]
    [InlineData("r", "the key 'r'")]
    public void AZoneWithABadKeyIsAnError(string key, string reason)
    {
        // D-1262: a key is one character other than the mark of a blocked tile, and each zone takes its own.
        string zone = EncounterMaps.Zone(100, OneGroup).Replace("\"key\": \"z\"", $"\"key\": \"{key}\"", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => EncounterMaps.Read(EncounterMaps.TextOf(zone)));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(2, ".rz.zzzz.", "holds no zone")]
    [InlineData(0, "........z", "a blocked tile holds '.'")]
    [InlineData(2, ".rzzqzzz.", "the key 'q'")]
    [InlineData(2, ".rzzzzzz", "row 2 holds 8 characters")]
    [InlineData(4, null, "holds 4 rows")]
    public void AZoneGridThatDoesNotMatchTheTerrainIsAnError(int row, string? replacement, string reason)
    {
        // D-1262, T-2: a walkable tile with no zone, a zone on a mountain, an unknown key, a short
        // row, and a missing row each fail the load. An absent zone is never a rate of zero.
        List<string> grid = [.. EncounterMaps.Grid];
        if (replacement is null)
        {
            grid.RemoveAt(row);
        }
        else
        {
            grid[row] = replacement;
        }

        ContentException error = Assert.Throws<ContentException>(() => EncounterMaps.Read(EncounterMaps.TextOf(EncounterMaps.Zone(100, OneGroup), [.. grid])));

        Assert.Contains(File, error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AZoneThatHoldsNoTileIsAnError()
    {
        // D-1262: a zone that no step reaches reads like a mistake.
        string text = EncounterMaps.TextOf(EncounterMaps.Zone(100, OneGroup), [".........", ".zzzzzzz.", ".zzzzzzz.", ".zzzzzzz.", "........."]);

        ContentException error = Assert.Throws<ContentException>(() => EncounterMaps.Read(text));

        Assert.Contains("zone.test_road", error.Message, StringComparison.Ordinal);
        Assert.Contains("holds no tile", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ADungeonWithAZoneIsAnError()
    {
        // D-1247: every map other than the overworld keeps its visible enemies alone.
        string text = DungeonText.Replace(
            "\"zones\": [], \"zone_grid\": []",
            "\"zones\": [" + EncounterMaps.Zone(100, OneGroup) + "], \"zone_grid\": []",
            StringComparison.Ordinal);
        Assert.NotEqual(DungeonText, text);

        ContentException error = Assert.Throws<ContentException>(() => TestMaps.Of("test-cellar.json", text));

        Assert.Contains("D-1247", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AMapWithNoZoneListOrNoZoneGridIsAnError()
    {
        // G-6, T-2: an absent field is an error.
        string noZones = DungeonText.Replace("\"zones\": [], ", string.Empty, StringComparison.Ordinal);
        string noGrid = DungeonText.Replace("\"zone_grid\": [], ", string.Empty, StringComparison.Ordinal);

        Assert.Contains("field zones", Assert.Throws<ContentException>(() => TestMaps.Of("test-cellar.json", noZones)).Message, StringComparison.Ordinal);
        Assert.Contains("field zone_grid", Assert.Throws<ContentException>(() => TestMaps.Of("test-cellar.json", noGrid)).Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheFixtureOverworldHoldsTheFourZonesOfItsDecision()
    {
        // D-1267: the road at rate 0, the grass at 100, the forest at 200 with the elite at a
        // quarter, and the valley past the gate at 100 while the rats flag is on.
        GameMap map = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())).Maps.Single(each => each.Id.Value == "map.fixture_overworld");

        Assert.Equal(
            ["zone.fixture_overworld_road 0", "zone.fixture_overworld_grass 100", "zone.fixture_overworld_forest 200", "zone.fixture_overworld_valley 100"],
            [.. map.Zones.Select(zone => $"{zone.Id.Value} {zone.Rate}")]);
        Assert.Equal("zone.fixture_overworld_road", map.ZoneAt(map.Spawn)?.Id.Value);
        foreach (MapThing thing in map.Things)
        {
            if (thing.Kind == MapThingKind.Entrance)
            {
                Assert.Equal("zone.fixture_overworld_road", map.ZoneAt(thing.At)?.Id.Value);
            }
        }

        EncounterZone forest = map.ZoneOf(ContentId.Parse("zone.fixture_overworld_forest", "test", "zone"))!;
        Assert.Equal(4, forest.TotalWeight);
        Assert.Equal("group.fixture_elite", forest.GroupAt(3).Value);
        EncounterZone valley = map.ZoneOf(ContentId.Parse("zone.fixture_overworld_valley", "test", "zone"))!;
        Assert.Equal("flag.fixture_hub_rats", valley.Condition.Flag?.Value);
    }

    /// <summary>Gives the content files of the checkout, with one text changed in one file.</summary>
    private static List<ContentFile> Changed(string path, string old, string replacement)
    {
        var files = new List<ContentFile>();
        bool found = false;
        foreach (ContentFile file in ContentFolder.Read(RepositoryRoot.Find()))
        {
            if (string.CompareOrdinal(file.Path, path) != 0)
            {
                files.Add(file);
                continue;
            }

            string text = System.Text.Encoding.UTF8.GetString(file.Bytes);
            Assert.Contains(old, text, StringComparison.Ordinal);
            files.Add(new ContentFile(file.Path, System.Text.Encoding.UTF8.GetBytes(text.Replace(old, replacement, StringComparison.Ordinal))));
            found = true;
        }

        Assert.True(found, $"The checkout holds no file '{path}'.");
        return files;
    }
}
