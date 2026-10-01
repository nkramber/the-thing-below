using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The load of an overworld and of the things that lead to it: the map reader refuses each
/// thing that the kind of a map cannot hold, and the content set refuses a link to an absent map
/// or marker (exit tests 5 to 7 of PR-35, D-1243, D-1247, D-1255).
/// </summary>
public sealed class OverworldLoadTests
{
    private const string DungeonPath = "rules/maps/fixture-dungeon.json";

    private const string DungeonExit = "\"to\": \"map.fixture_overworld\", \"arrive\": \"marker.fixture_overworld_cut\"";

    private const string OverworldPath = "rules/maps/fixture-overworld.json";

    private const string Walker = """{ "id": "patrol.test_walker", "group": "group.test_pair", "size": "common", "facing": "east", "step_ticks": 32, "sight_range": 3, "routes": [{ "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 2, "y": 3 }, { "x": 5, "y": 3 }] }] }""";

    [Fact]
    public void TheTestOverworldReadsItsEntranceItsMarkerAndItsGate()
    {
        GameMap map = OverworldMaps.Overworld;

        Assert.Equal(MapKind.Overworld, map.Kind);
        Assert.Equal("map.test_place", map.EntranceAt(new TilePoint(1, 1))?.To?.Value);
        MapGate gate = Assert.IsType<MapGate>(map.GateAt(new TilePoint(6, 2))?.Gate);
        Assert.Equal(OverworldMaps.GateNotice, gate.Notice.Value);
        Assert.Equal(TileKind.Mountain, map.TileAt(new TilePoint(0, 0)));
        Assert.Equal("marker.test_overworld_place", OverworldMaps.Place.ExitAt(new TilePoint(5, 1))?.Arrive?.Value);
    }

    [Fact]
    public void AnOverworldWithAnEnemyIsAnError()
    {
        // Exit test 7 of PR-35 (D-1247): the overworld holds no visible enemy.
        string text = OverworldMaps.OverworldText.Replace(
            "\"enemies\": []",
            "\"enemies\": [" + Walker + "]",
            StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => OverworldMaps.Read(text));

        Assert.Contains("test-overworld.json", error.Message, StringComparison.Ordinal);
        Assert.Contains("patrol.test_walker", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-1247", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnOverworldWithAnExitIsAnError()
    {
        // D-1243: the party leaves the overworld through an entrance.
        string text = OverworldMaps.OverworldText.Replace(
            "\"kind\": \"entrance\", \"x\": 1, \"y\": 1, \"to\": \"map.test_place\" }",
            "\"kind\": \"entrance\", \"x\": 1, \"y\": 1, \"to\": \"map.test_place\" },\n  { \"id\": \"exit.test_overworld_out\", \"kind\": \"exit\", \"x\": 2, \"y\": 1, \"to\": \"map.test_place\" }",
            StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => OverworldMaps.Read(text));

        Assert.Contains("exit.test_overworld_out", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("""{ "id": "entrance.test_room_in", "kind": "entrance", "x": 3, "y": 1, "to": "map.test_place" }""", "entrance.test_room_in")]
    [InlineData("""{ "id": "mark.test_room_town", "kind": "mark", "x": 3, "y": 1 }""", "mark.test_room_town")]
    public void APlaceWithAnEntranceOrAMarkIsAnError(string thing, string id)
    {
        // D-1243, D-1271, D-1347: an overworld alone holds entrances and marks.
        string text = PlaceText().Replace(
            "\"kind\": \"spawn_point\", \"x\": 1, \"y\": 1 }",
            $"\"kind\": \"spawn_point\", \"x\": 1, \"y\": 1 }},\n  {thing}",
            StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => GameMap.Read(Encoding.UTF8.GetBytes(text), "test-place.json"));

        Assert.Contains(id, error.Message, StringComparison.Ordinal);
        Assert.Contains("An overworld alone holds entrances and marks", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("\"condition\": { \"flag\": \"flag.fixture_hub_rats\" }, ", "condition")]
    [InlineData(", \"notice\": \"notice.fixture_gate_shut\"", "notice")]
    public void AGateWithNoConditionOrNoNoticeIsAnError(string removed, string field)
    {
        // D-1243, D-1257, T-2: an absent field is an error, never a default.
        string text = OverworldMaps.OverworldText.Replace(removed, string.Empty, StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => OverworldMaps.Read(text));

        Assert.Contains("gate.test_overworld_pass", error.Message, StringComparison.Ordinal);
        Assert.Contains($"holds no field '{field}'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEntranceWithNoMapIsAnError()
    {
        string text = OverworldMaps.OverworldText.Replace(", \"to\": \"map.test_place\"", string.Empty, StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => OverworldMaps.Read(text));

        Assert.Contains("the entrance 'entrance.test_overworld_place' holds no field 'to'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AMarkerThatNamesAnArrivalIsAnError()
    {
        // D-1255: an exit alone names the marker where the party arrives.
        string text = OverworldMaps.OverworldText.Replace(
            "\"kind\": \"marker\", \"x\": 1, \"y\": 2 }",
            "\"kind\": \"marker\", \"x\": 1, \"y\": 2, \"arrive\": \"marker.test_overworld_place\" }",
            StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => OverworldMaps.Read(text));

        Assert.Contains("the field 'arrive', which an exit or an entrance alone holds", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AMarkLoadsOnTheOverworldAndDrawsOnItsTile()
    {
        // D-1271: a mark stands for a place with no map, and no rule reads it.
        string text = OverworldMaps.OverworldText.Replace(
            "\"kind\": \"marker\", \"x\": 1, \"y\": 2 }",
            "\"kind\": \"marker\", \"x\": 1, \"y\": 2 },\n  { \"id\": \"mark.test_overworld_town\", \"kind\": \"mark\", \"x\": 3, \"y\": 3 }",
            StringComparison.Ordinal);

        GameMap map = OverworldMaps.Read(text);

        MapThing mark = Assert.Single(map.ThingsAt(new TilePoint(3, 3)));
        Assert.Equal(MapThingKind.Mark, mark.Kind);
        Assert.Null(mark.To);
        Assert.Null(mark.Gate);
        Assert.False(MapThingKinds.IsSolid(MapThingKind.Mark));
    }

    [Theory]
    [InlineData(", \"to\": \"map.test_place\"", "which an exit or an entrance alone holds")]
    [InlineData(", \"arrive\": \"marker.test_overworld_place\"", "which an exit or an entrance alone holds")]
    [InlineData(", \"condition\": { \"always\": true }", "which a gate alone holds")]
    public void AMarkWithTheFieldOfAnotherKindIsAnError(string field, string reason)
    {
        // D-1271, T-2: a mark holds no link, no arrival, and no condition.
        string text = OverworldMaps.OverworldText.Replace(
            "\"kind\": \"marker\", \"x\": 1, \"y\": 2 }",
            $"\"kind\": \"marker\", \"x\": 1, \"y\": 2 }},\n  {{ \"id\": \"mark.test_overworld_town\", \"kind\": \"mark\", \"x\": 3, \"y\": 3{field} }}",
            StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => OverworldMaps.Read(text));

        Assert.Contains("mark.test_overworld_town", error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData('_')]
    [InlineData(';')]
    public void AThingSitsOnTheRoadAndTheSnowfield(char ground)
    {
        // D-1291: the road and the snowfield are open ground, as the grass is.
        string text = WithSpawnGround(ground);

        GameMap map = OverworldMaps.Read(text);

        Assert.Equal(new TilePoint(4, 2), map.Spawn);
        Assert.True(TileKinds.TryOf(ground, out TileKind kind));
        Assert.Equal(kind, map.TileAt(map.Spawn));
    }

    [Fact]
    public void AThingInTheForestIsAnError()
    {
        // D-1291: the forest stays out of the open ground.
        string text = WithSpawnGround('%');

        ContentException error = Assert.Throws<ContentException>(() => OverworldMaps.Read(text));

        Assert.Contains("on a forest tile", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AThingOnAMountainIsAnError()
    {
        // D-528, D-1256: a thing sits on floor or grass, and a mountain takes no step.
        string text = OverworldMaps.OverworldText.Replace(
            "\"kind\": \"spawn_point\", \"x\": 4, \"y\": 2 }",
            "\"kind\": \"spawn_point\", \"x\": 4, \"y\": 0 }",
            StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => OverworldMaps.Read(text));

        Assert.Contains("on a mountain tile, and a spawn_point sits on a floor, grass, road, or snowfield tile", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCheckoutHoldsTheFixtureOverworldBetweenTheDungeonAndTheHub()
    {
        // Exit test 8 of PR-35 and D-1244: the entrances lead to the dungeon and the hub, the exit
        // of each leads back onto its marker, and the gate reads the flag of the rats.
        ContentSet set = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));
        GameMap overworld = set.Map(OverworldMaps.Id("map.fixture_overworld"));

        Assert.Equal(MapKind.Overworld, overworld.Kind);
        Assert.Equal(TimeOfDay.Day, overworld.BaseTime);
        Assert.Equal(overworld.BaseTime, set.Light.SetupOf(overworld.Id, overworld.BaseTime).Time);
        Assert.Equal(
            ["map.fixture_dungeon", "map.fixture_hub"],
            EntranceTargets(overworld));
        MapThing gate = Assert.Single(overworld.Things, thing => thing.Kind == MapThingKind.Gate);
        Assert.Equal("notice.fixture_gate_shut", gate.Gate?.Notice.Value);
        foreach (string id in new[] { "map.fixture_dungeon", "map.fixture_hub" })
        {
            MapThing exit = Assert.Single(set.Map(OverworldMaps.Id(id)).Things, thing => thing.Kind == MapThingKind.Exit);
            Assert.Equal("map.fixture_overworld", exit.To?.Value);
            Assert.NotNull(overworld.ThingOf(exit.Arrive!, MapThingKind.Marker));
        }
    }

    [Fact]
    public void AnExitToAnAbsentMapNamesTheFileAndTheId()
    {
        // Exit test 5 of PR-35 (D-1216, T-2).
        ContentException error = Assert.Throws<ContentException>(
            () => ContentSet.Load(Changed(DungeonPath, DungeonExit, "\"to\": \"map.absent\"")));

        Assert.Contains(DungeonPath, error.Message, StringComparison.Ordinal);
        Assert.Contains("map.absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEntranceToAnAbsentMapNamesTheFileAndTheId()
    {
        // Exit test 5 of PR-35 (D-1243, T-2).
        ContentException error = Assert.Throws<ContentException>(
            () => ContentSet.Load(Changed(OverworldPath, "\"to\": \"map.fixture_hub\"", "\"to\": \"map.absent\"")));

        Assert.Contains(OverworldPath, error.Message, StringComparison.Ordinal);
        Assert.Contains("the entrance names the map 'map.absent'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnExitToTheOverworldWithNoMarkerIsAnError()
    {
        // Exit test 6 of PR-35 (D-1255).
        ContentException error = Assert.Throws<ContentException>(
            () => ContentSet.Load(Changed(DungeonPath, DungeonExit, "\"to\": \"map.fixture_overworld\"")));

        Assert.Contains(DungeonPath, error.Message, StringComparison.Ordinal);
        Assert.Contains("names no marker", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnExitThatNamesAnAbsentMarkerNamesTheFileAndTheId()
    {
        // Exit test 6 of PR-35 (D-1255).
        ContentException error = Assert.Throws<ContentException>(
            () => ContentSet.Load(Changed(DungeonPath, DungeonExit, "\"to\": \"map.fixture_overworld\", \"arrive\": \"marker.absent\"")));

        Assert.Contains(DungeonPath, error.Message, StringComparison.Ordinal);
        Assert.Contains("marker.absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnExitToAPlaceThatNamesAMarkerIsAnError()
    {
        // D-1255: the party arrives on the spawn point of a map that is not an overworld.
        ContentException error = Assert.Throws<ContentException>(
            () => ContentSet.Load(Changed(DungeonPath, DungeonExit, "\"to\": \"map.fixture_hub\", \"arrive\": \"marker.fixture_hub_corner\"")));

        Assert.Contains("An exit to an overworld alone names a marker", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AGateWithAnAbsentNoticeOrAnUndeclaredFlagIsAnError()
    {
        // D-543, D-1257, T-2.
        ContentException notice = Assert.Throws<ContentException>(
            () => ContentSet.Load(Changed(OverworldPath, "\"notice\": \"notice.fixture_gate_shut\"", "\"notice\": \"notice.absent\"")));
        ContentException flag = Assert.Throws<ContentException>(
            () => ContentSet.Load(Changed(OverworldPath, "\"flag\": \"flag.fixture_hub_rats\"", "\"flag\": \"flag.absent\"")));

        Assert.Contains("notice.absent", notice.Message, StringComparison.Ordinal);
        Assert.Contains(OverworldPath, flag.Message, StringComparison.Ordinal);
        Assert.Contains("flag.absent", flag.Message, StringComparison.Ordinal);
    }

    /// <summary>Gives the map of each entrance of a map, in the order of the file.</summary>
    private static List<string> EntranceTargets(GameMap map)
    {
        var targets = new List<string>();
        foreach (MapThing thing in map.Things)
        {
            if (thing.Kind == MapThingKind.Entrance)
            {
                targets.Add(thing.To!.Value);
            }
        }

        return targets;
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

            string text = Encoding.UTF8.GetString(file.Bytes);
            Assert.Contains(old, text, StringComparison.Ordinal);
            files.Add(new ContentFile(file.Path, Encoding.UTF8.GetBytes(text.Replace(old, replacement, StringComparison.Ordinal))));
            found = true;
        }

        Assert.True(found, $"The checkout holds no file '{path}'.");
        return files;
    }

    /// <summary>Gives the text of the test overworld with another ground under the spawn point, at (4, 2).</summary>
    private static string WithSpawnGround(char ground)
    {
        const string Row = "\"^,,,,,,,^\"";
        int first = OverworldMaps.OverworldText.IndexOf(Row, StringComparison.Ordinal);
        int second = OverworldMaps.OverworldText.IndexOf(Row, first + Row.Length, StringComparison.Ordinal);
        Assert.True(first >= 0 && second > first, "The test overworld holds no second grass row.");
        return string.Concat(OverworldMaps.OverworldText.AsSpan(0, second), $"\"^,,,{ground},,,^\"", OverworldMaps.OverworldText.AsSpan(second + Row.Length));
    }

    /// <summary>Gives the text of the test place, which a load test changes.</summary>
    private static string PlaceText() => """
        {
         "comment": "A place for the load tests of PR-35.",
         "id": "map.test_place",
         "region": "region.test",
         "label": "label.test_place",
         "kind": "dungeon",
         "time": "day",
         "dark": false,
         "terrain": [
          "#######",
          "#.....#",
          "#######"
         ],
         "things": [
          { "id": "spawn_point.test_place_start", "kind": "spawn_point", "x": 1, "y": 1 }
         ],
         "enemies": [], "npcs": [], "services": [], "zones": [], "zone_grid": [], "time_changes": [], "reopen": [], "triggers": []
        }
        """;
}
