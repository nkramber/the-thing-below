using TheThingBelow.Core.Maps;

namespace TheThingBelow.Tests;

/// <summary>
/// The maps with enemies that the tests of PR-8 read. Each one is a map file, read by the
/// one reader of Core, so a test never builds an enemy by another path (T-1, D-738).
/// </summary>
/// <remarks>
/// The ground is a room of 10 by 8 tiles with a block of wall in the middle, so a test of
/// the sight rule has a wall to stand behind (D-718).
/// </remarks>
public static class PatrolMaps
{
    /// <summary>One enemy that walks a route of two tiles along the north wall.</summary>
    public static readonly string Walker = Enemy();

    /// <summary>One enemy that paces an area in the southwest of the room.</summary>
    public static readonly string Pacer = Enemy(stations: """
       "areas": [
        { "times": ["dawn", "day", "dusk", "night"], "x": 1, "y": 5, "width": 3, "height": 2 }
       ]
      """);

    /// <summary>Makes the text of one enemy record, with a default for each field.</summary>
    /// <param name="id">The id of the enemy (D-752).</param>
    /// <param name="group">The id of its group (D-753).</param>
    /// <param name="size">The name of its size (D-206).</param>
    /// <param name="facing">The name of its facing (D-718).</param>
    /// <param name="stepTicks">The count of ticks of one step (D-742).</param>
    /// <param name="sightRange">Its sight range, in tiles (D-720).</param>
    /// <param name="stations">The text of the `routes` field or of the `areas` field.</param>
    /// <param name="extra">One more field of the record, for a test of an unknown field.</param>
    /// <returns>The text of the record, for the `enemies` array of a map file.</returns>
    public static string Enemy(
        string id = "patrol.one",
        string group = "group.one",
        string size = "common",
        string facing = "east",
        int stepTicks = 32,
        int sightRange = 3,
        string stations = """
           "routes": [
            { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 1, "y": 1 }, { "x": 3, "y": 1 }] }
           ]
          """,
        string extra = "") =>
        $$"""
          {
           "id": "{{id}}",
           "group": "{{group}}",
           "size": "{{size}}",
           "facing": "{{facing}}",
           "step_ticks": {{stepTicks}},
           "sight_range": {{sightRange}},
        {{extra}}
        {{stations}}
          }
        """;

    /// <summary>Reads a map of 10 by 8 tiles with the enemies of one test (D-738).</summary>
    /// <param name="enemies">The text of the `enemies` array, without its brackets.</param>
    /// <param name="time">The time of day of the map, which picks each station (D-442).</param>
    /// <returns>The map.</returns>
    public static GameMap Of(string enemies, string time = "day") => TestMaps.Of(
        "patrol-test.json",
        $$"""
        {
         "comment": "a room with a wall block and the enemies of one test",
         "id": "map.patrol_test",
         "region": "region.test",
         "label": "label.patrol_test",
         "time": "{{time}}",
         "dark": false,
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
          { "id": "spawn_point.patrol_test_start", "kind": "spawn_point", "x": 8, "y": 6 }
         ],
         "enemies": [
        {{enemies}}
         ], "triggers": []
        }
        """);
}
