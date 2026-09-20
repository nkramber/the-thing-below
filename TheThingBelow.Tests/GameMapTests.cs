using System;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
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
        Assert.Equal(TimeOfDay.Day, map.Time);
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
             { "id": "chest.bad_store", "kind": "chest", "x": 1, "y": 1 }
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
             { "id": "chest.bad_store", "kind": "chest", "x": 1, "y": 1 }
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
             { "id": "lock.bad_gate", "kind": "lock", "x": 5, "y": 1, "pickable": true }
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
              "#..~.#",
              "######"
            """)));

        Assert.Contains("'~'", error.Message, StringComparison.Ordinal);
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

    [Theory]
    [InlineData("rules/maps/deep-mine.json", true)]
    [InlineData("rules/maps/nested/deep-mine.json", true)]
    [InlineData("rules/fixtures/parts.json", false)]
    [InlineData("rules/maps/notes.txt", false)]
    public void AMapFileIsAJsonFileOfTheMapFolder(string path, bool wanted)
    {
        Assert.Equal(wanted, GameMap.IsMapFile(path));
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
        string time = "day") =>
        $$"""
        {
         "comment": "a map for one test",
         "id": "map.bad",
         "label": "label.bad",
         "time": "{{time}}",
         "terrain": [
        {{terrain}}
         ],
         "things": [
        {{things}}
         ]
        }
        """;
}
