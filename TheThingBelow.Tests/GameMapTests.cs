using System;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Story;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The reader of a map rule file (D-528). Every rule of a map fails the load with the map,
/// the tile, and the reason, and no rule of a map passes in silence (T-2).
/// </summary>
public sealed class GameMapTests
{
    [Fact]
    public void AMapHoldsItsTerrainItsThingsAndItsTime()
    {
        GameMap map = TestMaps.Room;

        Assert.Equal("map.test_room", map.Id.Value);
        Assert.Equal("label.test_room", map.Label.Value);
        Assert.Equal(TimeOfDay.Day, map.BaseTime);
        Assert.Equal(12, map.Width);
        Assert.Equal(9, map.Height);
        Assert.Equal(new TilePoint(2, 2), map.Spawn);
        Assert.Equal(3, map.Things.Count);
    }

    [Fact]
    public void ATileReadsBackItsKind()
    {
        GameMap map = TestMaps.Room;

        Assert.Equal(TileKind.Wall, map.TileAt(new TilePoint(0, 0)));
        Assert.Equal(TileKind.Floor, map.TileAt(new TilePoint(2, 2)));
        Assert.Equal(TileKind.Wall, map.TileAt(new TilePoint(5, 3)));
        Assert.Equal(TileKind.Doorway, map.TileAt(new TilePoint(11, 4)));
    }

    [Fact]
    public void ATileOutsideTheMapIsAnError()
    {
        ArgumentOutOfRangeException error = Assert.Throws<ArgumentOutOfRangeException>(
            () => TestMaps.Room.TileAt(new TilePoint(12, 0)));

        Assert.Contains("12 by 9", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ALockedDoorGivesTwoThingsOnOneTile()
    {
        // A lock sits on the door that it holds shut, and no other pair shares a tile
        // (D-386).
        Assert.Equal(2, TestMaps.Room.ThingsAt(new TilePoint(11, 4)).Count);
        Assert.Empty(TestMaps.Room.ThingsAt(new TilePoint(3, 3)));
    }

    [Fact]
    public void AThingOnATileOfTheWrongKindFailsTheLoadWithTheMapThePositionAndTheKind()
    {
        // Exit test 5 of section 7.3 of `phase-2-first-playable.md` (T-2, D-528).
        ContentException error = Assert.Throws<ContentException>(
            () => TestMaps.Of("bad-tile.json", Map(things: """
             { "id": "spawn_point.bad_start", "kind": "spawn_point", "x": 1, "y": 1 },
             { "id": "chest.bad_store", "kind": "chest", "x": 0, "y": 0 }
            """)));

        Assert.Contains("bad-tile.json", error.Message, StringComparison.Ordinal);
        Assert.Contains("(0, 0)", error.Message, StringComparison.Ordinal);
        Assert.Contains("wall", error.Message, StringComparison.Ordinal);
        Assert.Contains("chest", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ADoorOnOpenGroundFailsTheLoad()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => TestMaps.Of("bad-door.json", Map(things: """
             { "id": "spawn_point.bad_start", "kind": "spawn_point", "x": 1, "y": 1 },
             { "id": "door.bad_hall", "kind": "door", "x": 2, "y": 1 }
            """)));

        Assert.Contains("doorway", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AMapWithNoSpawnPointFailsTheLoad()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => TestMaps.Of("no-spawn.json", Map(things: """
             { "id": "marker.bad_store", "kind": "marker", "x": 1, "y": 1 }
            """)));

        Assert.Contains("0 spawn points", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AMapWithTwoSpawnPointsFailsTheLoad()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => TestMaps.Of("two-spawns.json", Map(things: """
             { "id": "spawn_point.bad_one", "kind": "spawn_point", "x": 1, "y": 1 },
             { "id": "spawn_point.bad_two", "kind": "spawn_point", "x": 2, "y": 1 }
            """)));

        Assert.Contains("2 spawn points", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoThingsOnOneTileFailTheLoad()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => TestMaps.Of("crowded.json", Map(things: """
             { "id": "spawn_point.bad_start", "kind": "spawn_point", "x": 1, "y": 1 },
             { "id": "marker.bad_store", "kind": "marker", "x": 1, "y": 1 }
            """)));

        Assert.Contains("(1, 1)", error.Message, StringComparison.Ordinal);
        Assert.Contains("2 things", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ALockOnNoDoorFailsTheLoad()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => TestMaps.Of("lone-lock.json", Map(things: """
             { "id": "spawn_point.bad_start", "kind": "spawn_point", "x": 1, "y": 1 },
             { "id": "lock.bad_gate", "kind": "lock", "x": 5, "y": 1, "pickable": true, "key": "none" }
            """)));

        Assert.Contains("sits on no door", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ALockWithNoPickableFieldFailsTheLoad()
    {
        // D-386: the layout marks every lock, so the field is never absent.
        ContentException error = Assert.Throws<ContentException>(
            () => TestMaps.Of("unmarked-lock.json", Map(things: """
             { "id": "spawn_point.bad_start", "kind": "spawn_point", "x": 1, "y": 1 },
             { "id": "door.bad_gate", "kind": "door", "x": 5, "y": 1 },
             { "id": "lock.bad_gate", "kind": "lock", "x": 5, "y": 1 }
            """)));

        Assert.Contains("pickable", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void APickableFieldOnAThingThatIsNoLockFailsTheLoad()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => TestMaps.Of("pickable-chest.json", Map(things: """
             { "id": "spawn_point.bad_start", "kind": "spawn_point", "x": 1, "y": 1 },
             { "id": "chest.bad_store", "kind": "chest", "x": 2, "y": 1, "pickable": true }
            """)));

        Assert.Contains("a lock alone holds", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AThingWhoseIdKindDisagreesWithItsKindFailsTheLoad()
    {
        // D-646: the kind of an id agrees with the record that holds the entry.
        ContentException error = Assert.Throws<ContentException>(
            () => TestMaps.Of("wrong-kind.json", Map(things: """
             { "id": "spawn_point.bad_start", "kind": "spawn_point", "x": 1, "y": 1 },
             { "id": "marker.bad_store", "kind": "chest", "x": 2, "y": 1 }
            """)));

        Assert.Contains("marker", error.Message, StringComparison.Ordinal);
        Assert.Contains("chest", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AThingOutsideTheMapFailsTheLoad()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => TestMaps.Of("outside.json", Map(things: """
             { "id": "spawn_point.bad_start", "kind": "spawn_point", "x": 1, "y": 1 },
             { "id": "marker.bad_far", "kind": "marker", "x": 40, "y": 1 }
            """)));

        Assert.Contains("(40, 1)", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoThingsOfOneIdFailTheLoad()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => TestMaps.Of("repeated.json", Map(things: """
             { "id": "spawn_point.bad_start", "kind": "spawn_point", "x": 1, "y": 1 },
             { "id": "marker.bad_one", "kind": "marker", "x": 2, "y": 1 },
             { "id": "marker.bad_one", "kind": "marker", "x": 3, "y": 1 }
            """)));

        Assert.Contains("marker.bad_one", error.Message, StringComparison.Ordinal);
        Assert.Contains("permanent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARowOfAnotherLengthFailsTheLoad()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => TestMaps.Of("ragged.json", Map(terrain: """
              "######",
              "#....#",
              "#...#"
            """)));

        Assert.Contains("row 2", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnknownTerrainCharacterFailsTheLoad()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => TestMaps.Of("odd.json", Map(terrain: """
              "######",
              "#..?.#",
              "######"
            """)));

        Assert.Contains("'?'", error.Message, StringComparison.Ordinal);
        Assert.Contains("(3, 1)", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnknownTimeOfDayFailsTheLoad()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => TestMaps.Of("noon.json", Map(time: "noon")));

        Assert.Contains("noon", error.Message, StringComparison.Ordinal);
        Assert.Contains("dawn, day, dusk, night", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnknownThingKindFailsTheLoad()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => TestMaps.Of("altar.json", Map(things: """
             { "id": "spawn_point.bad_start", "kind": "spawn_point", "x": 1, "y": 1 },
             { "id": "altar.bad_stone", "kind": "altar", "x": 2, "y": 1 }
            """)));

        Assert.Contains("altar", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnknownFieldFailsTheLoad()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => TestMaps.Of("weather.json", Map().Replace("\"time\":", "\"weather\": \"rain\",\n \"time\":", StringComparison.Ordinal)));

        Assert.Contains("weather", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAbsentFieldFailsTheLoad()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => TestMaps.Of("no-time.json", Map().Replace(" \"time\": \"day\",\n", string.Empty, StringComparison.Ordinal)));

        Assert.Contains("time", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AMapWithNoDarkFieldFailsTheLoad()
    {
        // D-1062, G-6: an absent field is an error, and never a map that is not dark.
        ContentException error = Assert.Throws<ContentException>(
            () => TestMaps.Of("no-dark.json", Map().Replace(" \"dark\": false,\n", string.Empty, StringComparison.Ordinal)));

        Assert.Contains("dark", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void TheDarkFieldGivesTheDarkOfTheMap(string field, bool dark)
    {
        GameMap map = TestMaps.Of("dark.json", Map().Replace("\"dark\": false", $"\"dark\": {field}", StringComparison.Ordinal));

        Assert.Equal(dark, map.Dark);
    }

    [Fact]
    public void ADarkFieldThatHoldsNoBooleanFailsTheLoad()
    {
        Assert.Throws<ContentException>(
            () => TestMaps.Of("dark-text.json", Map().Replace("\"dark\": false", "\"dark\": \"yes\"", StringComparison.Ordinal)));
    }

    [Theory]
    [InlineData("hub", MapKind.Hub)]
    [InlineData("dungeon", MapKind.Dungeon)]
    public void TheKindFieldGivesTheKindOfTheMap(string field, MapKind kind)
    {
        // D-112: a hub and a dungeon take one reader and one code path.
        GameMap map = TestMaps.Of("kind.json", Map().Replace("\"kind\": \"dungeon\"", $"\"kind\": \"{field}\"", StringComparison.Ordinal));

        Assert.Equal(kind, map.Kind);
    }

    [Fact]
    public void AKindThatNamesNoMapKindFailsTheLoad()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => TestMaps.Of("kind-town.json", Map().Replace("\"kind\": \"dungeon\"", "\"kind\": \"town\"", StringComparison.Ordinal)));

        Assert.Contains("the kind 'town', and a map takes one of hub, dungeon", error.Message, StringComparison.Ordinal);
        Assert.Contains("kind-town.json", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AMapWithNoKindFieldFailsTheLoad()
    {
        // G-6: an absent field is an error, and never a map of a default kind.
        ContentException error = Assert.Throws<ContentException>(
            () => TestMaps.Of("no-kind.json", Map().Replace("\"kind\": \"dungeon\", ", string.Empty, StringComparison.Ordinal)));

        Assert.Contains("kind", error.Message, StringComparison.Ordinal);
        Assert.Contains("absent", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(MapKind.Hub, "hub")]
    [InlineData(MapKind.Dungeon, "dungeon")]
    public void EachMapKindReadsBackItsName(MapKind kind, string name)
    {
        Assert.Equal(name, MapKinds.NameOf(kind));
        Assert.True(MapKinds.TryOf(name, out MapKind parsed));
        Assert.Equal(kind, parsed);
        Assert.Throws<ArgumentOutOfRangeException>(() => MapKinds.NameOf((MapKind)9));
    }

    [Fact]
    public void TheFirstDungeonIsADungeonWithNoNpcAndNoService()
    {
        Assert.Equal(MapKind.Dungeon, TestMaps.FixtureDungeon.Kind);
        Assert.Empty(TestMaps.FixtureDungeon.Npcs);
        Assert.Empty(TestMaps.FixtureDungeon.Services);
    }

    [Fact]
    public void AServicePointADoorAChestAndASavePointAreTheSolidThings()
    {
        // D-1142, D-1222: PR-16 adds the door, the chest, and the save point to the solid things.
        Assert.True(MapThingKinds.CanSitOn(MapThingKind.ServicePoint, TileKind.Floor));
        Assert.Equal("service_point", MapThingKinds.NameOf(MapThingKind.ServicePoint));
        foreach (MapThingKind kind in MapThingKinds.All)
        {
            bool solid = kind is MapThingKind.ServicePoint or MapThingKind.Door or MapThingKind.Chest or MapThingKind.SavePoint;
            Assert.Equal(solid, MapThingKinds.IsSolid(kind));
        }
    }

    [Fact]
    public void TheFirstDungeonIsDark()
    {
        // D-1067: the fixture dungeon is the dark map of PR-91.
        Assert.True(TestMaps.FixtureDungeon.Dark);
    }

    [Theory]
    [InlineData("rules/maps/deep-mine.json", true)]
    [InlineData("rules/maps/nested/deep-mine.json", true)]
    [InlineData("rules/fixtures/parts.json", false)]
    [InlineData("rules/maps/notes.txt", false)]
    public void AMapFileIsAJsonFileOfTheMapFolder(string path, bool wanted)
    {
        Assert.Equal(wanted, GameMap.IsMapFile(path));
    }

    [Fact]
    public void AMapHoldsItsTimeChangesInTheOrderOfTheFile()
    {
        // D-1349: a change names a time and a condition in the one form of a condition (D-543).
        GameMap map = TimeMaps.NightOnFlag;

        TimeChange change = Assert.Single(map.TimeChanges);
        Assert.Equal(TimeOfDay.Night, change.Time);
        Assert.Equal(ConditionKind.Flag, change.Condition.Kind);
        Assert.Equal(TimeMaps.Flag, change.Condition.Flag!.Value);
        Assert.Equal(TimeOfDay.Day, map.BaseTime);
    }

    [Fact]
    public void AMapWithNoTimeChangeListIsAnError()
    {
        // D-1349, T-2: an absent list is an error, never an empty list.
        string text = Map().Replace("\"time_changes\": [], ", string.Empty, StringComparison.Ordinal);
        ContentException error = Assert.Throws<ContentException>(() => TestMaps.Of("bad.json", text));

        Assert.Contains("field time_changes", error.Message, StringComparison.Ordinal);
        Assert.Contains("the field is absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ATimeChangeOfAnUnknownTimeIsAnError()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => TimeMaps.Of("day", """[{ "time": "noon", "condition": { "flag": "flag.test_victor" } }]"""));

        Assert.Contains("time_changes[0].time", error.Message, StringComparison.Ordinal);
        Assert.Contains("'noon'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ATimeChangeOnTheAlwaysLeafIsAnError()
    {
        // D-1349, D-1002: the always leaf holds on each entry, so the base time never shows.
        ContentException error = Assert.Throws<ContentException>(
            () => TimeMaps.Of("day", """[{ "time": "night", "condition": { "always": true } }]"""));

        Assert.Contains("time_changes[0].condition", error.Message, StringComparison.Ordinal);
        Assert.Contains("base time", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ATimeChangeWithNoConditionIsAnError()
    {
        ContentException error = Assert.Throws<ContentException>(() => TimeMaps.Of("day", """[{ "time": "night" }]"""));

        Assert.Contains("time_changes[0].condition", error.Message, StringComparison.Ordinal);
        Assert.Contains("the field is absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheFirstTimeChangeThatHoldsSetsTheTime()
    {
        // D-1349: the list reads in the order of the file, and the base time holds when no change does.
        GameMap map = TimeMaps.Of(
            "day",
            """
            [
             { "time": "dusk", "condition": { "all": [{ "flag": "flag.test_victor" }, { "flag": "flag.test_done" }] } },
             { "time": "night", "condition": { "flag": "flag.test_victor" } },
             { "time": "dusk", "condition": { "flag": "flag.test_met" } }
            ]
            """);
        FlagSet flags = FlagSet.Empty();

        Assert.Equal(TimeOfDay.Day, map.TimeFor(flags));
        _ = flags.TurnOn(ContentId.Parse("flag.test_victor", "the test", "flag"));
        Assert.Equal(TimeOfDay.Night, map.TimeFor(flags));
        _ = flags.TurnOn(ContentId.Parse("flag.test_done", "the test", "flag"));
        Assert.Equal(TimeOfDay.Dusk, map.TimeFor(flags));
    }

    [Fact]
    public void TheTimesOfAMapHoldTheBaseTimeThenEachOtherTimeOnce()
    {
        // D-1349: the load checks the light, the enemies, and the NPCs at each of these times.
        GameMap map = TimeMaps.Of(
            "dusk",
            """
            [
             { "time": "night", "condition": { "flag": "flag.test_victor" } },
             { "time": "dusk", "condition": { "flag": "flag.test_done" } },
             { "time": "night", "condition": { "flag": "flag.test_met" } },
             { "time": "dawn", "condition": { "flag": "flag.test_yes" } }
            ]
            """);

        Assert.Equal([TimeOfDay.Dusk, TimeOfDay.Night, TimeOfDay.Dawn], map.Times);
        Assert.True(map.CanTake(TimeOfDay.Dawn));
        Assert.False(map.CanTake(TimeOfDay.Day));
        Assert.Equal([TimeOfDay.Day], TestMaps.Room.Times);
    }

    [Fact]
    public void AnEnemyThatSeesPastThePartyAtTheTimeOfAChangeIsAnError()
    {
        // D-720, D-1349: the sight floor holds at each time that the map can take, so a day map
        // that a flag turns to night takes no enemy that sees farther than the party at night.
        string text = TimeMaps.Text("day", """[{ "time": "night", "condition": { "flag": "flag.test_victor" } }]""")
            .Replace("\"sight_range\": 3", "\"sight_range\": 6", StringComparison.Ordinal);
        ContentException error = Assert.Throws<ContentException>(() => TestMaps.Of("time-test.json", text));

        Assert.Contains("a map set to night", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoEnemiesThatStartOnOneTileAtTheTimeOfAChangeAreAnError()
    {
        // D-206, D-1349: the start check reads each time that the map can take.
        string text = TimeMaps.Text("day", """[{ "time": "night", "condition": { "flag": "flag.test_victor" } }]""")
            .Replace("\"times\": [\"dawn\", \"day\", \"dusk\"]", "\"times\": [\"dawn\", \"day\", \"dusk\", \"night\"]", StringComparison.Ordinal)
            .Replace("{ \"x\": 1, \"y\": 6 }, { \"x\": 3, \"y\": 6 }", "{ \"x\": 1, \"y\": 1 }, { \"x\": 1, \"y\": 2 }", StringComparison.Ordinal);
        ContentException error = Assert.Throws<ContentException>(() => TestMaps.Of("time-test.json", text));

        Assert.Contains("at the time night", error.Message, StringComparison.Ordinal);
    }

    /// <summary>The text of a small map file, with one part of it changed for a test.</summary>
    private static string Map(
        string terrain = """
          "######",
          "#....+",
          "######"
        """,
        string things = """
         { "id": "spawn_point.bad_start", "kind": "spawn_point", "x": 1, "y": 1 }
        """,
        string time = "day",
        string enemies = "") =>
        $$"""
        {
         "comment": "a map for one test",
         "id": "map.bad",
         "region": "region.test",
         "label": "label.bad",
         "time": "{{time}}",
         "dark": false,
         "kind": "dungeon", "npcs": [], "services": [], "zones": [], "zone_grid": [], "time_changes": [], "reopen": [],
         "terrain": [
        {{terrain}}
         ],
         "things": [
        {{things}}
         ],
         "enemies": [
        {{enemies}}
         ], "triggers": []
        }
        """;
}
