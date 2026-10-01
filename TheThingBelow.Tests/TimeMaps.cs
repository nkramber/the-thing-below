using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Tests;

/// <summary>
/// The maps with time changes that the tests of PR-17 read (D-1349). Each one is a map file, read
/// by the one reader of Core, so a test never builds a time change by another path (T-1).
/// </summary>
/// <remarks>
/// The room holds two enemies: one that walks by day and one that walks at night alone, so the
/// enemies of a map tell its time (D-743). The flag of the change is a flag of the story fixture.
/// </remarks>
public static class TimeMaps
{
    /// <summary>The flag that each change of these maps reads, which the story fixture declares.</summary>
    public const string Flag = "flag.test_victor";

    /// <summary>The enemy that stands on the map at dawn, by day, and at dusk.</summary>
    public const string DayEnemy = "patrol.time_day";

    /// <summary>The enemy that stands on the map at night alone.</summary>
    public const string NightEnemy = "patrol.time_night";

    /// <summary>A room by day that turns to night when <see cref="Flag"/> is on at the entry.</summary>
    public static GameMap NightOnFlag { get; } = Of("day", """[{ "time": "night", "condition": { "flag": "flag.test_victor" } }]""");

    /// <summary>A room at night that stays by day while <see cref="Flag"/> is off at the entry: a change on a not node.</summary>
    public static GameMap DayUntilFlag { get; } = Of("night", """[{ "time": "day", "condition": { "not": { "flag": "flag.test_victor" } } }]""");

    /// <summary>The id of <see cref="Flag"/>.</summary>
    public static ContentId FlagId { get; } = ContentId.Parse(Flag, "the test", "flag");

    /// <summary>Reads the room with one base time and one list of time changes.</summary>
    /// <param name="time">The base time of the map.</param>
    /// <param name="changes">The text of the `time_changes` array.</param>
    /// <returns>The map.</returns>
    public static GameMap Of(string time, string changes) => TestMaps.Of("time-test.json", Text(time, changes));

    /// <summary>Gives the text of the room with one base time and one list of time changes.</summary>
    /// <param name="time">The base time of the map.</param>
    /// <param name="changes">The text of the `time_changes` array.</param>
    /// <returns>The text of the map file.</returns>
    public static string Text(string time, string changes) =>
        $$"""
        {
         "comment": "a room whose time a flag sets, with one enemy by day and one at night",
         "id": "map.time_test",
         "region": "region.test",
         "label": "label.time_test",
         "time": "{{time}}",
         "time_changes": {{changes}},
         "dark": false,
         "kind": "dungeon", "npcs": [], "services": [], "zones": [], "zone_grid": [], "reopen": [],
         "terrain": [
          "##########",
          "#........#",
          "#........#",
          "#...##...#",
          "#...##...#",
          "#........#",
          "#........#",
          "##########"
         ],
         "things": [
          { "id": "spawn_point.time_test_start", "kind": "spawn_point", "x": 8, "y": 6 }
         ],
         "enemies": [
        {{PatrolMaps.Enemy(id: DayEnemy, stations: """
           "routes": [
            { "times": ["dawn", "day", "dusk"], "tiles": [{ "x": 1, "y": 1 }, { "x": 3, "y": 1 }] }
           ]
          """)}},
        {{PatrolMaps.Enemy(id: NightEnemy, stations: """
           "routes": [
            { "times": ["night"], "tiles": [{ "x": 1, "y": 6 }, { "x": 3, "y": 6 }] }
           ]
          """)}}
         ], "triggers": []
        }
        """;
}
