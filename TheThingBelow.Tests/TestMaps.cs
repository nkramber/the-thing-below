using System.IO;
using System.Text;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Tests;

/// <summary>
/// The maps that the tests of the run and of the map read. Each one is a map file, read by
/// the one reader of Core, so a test never builds a map by another path (T-1, D-528).
/// </summary>
/// <remarks>
/// A test that needs the real dungeon reads it from the content set of the checkout. These
/// maps hold the shapes that a rule test needs: a room with a pillar, a map larger than the
/// view, a map smaller than the view, and a room with enemies (F-52, D-738).
/// </remarks>
public static class TestMaps
{
    /// <summary>The path of the first dungeon in the checkout.</summary>
    private const string FixtureDungeonPath = "content/rules/maps/fixture-dungeon.json";

    /// <summary>A room of 12 by 9 tiles with a pillar, a doorway, and a locked door.</summary>
    public static GameMap Room { get; } = Of(
        "test-room.json",
        """
        {
         "comment": "A room with a pillar, for the tests of the step rule and the sight rule.",
         "id": "map.test_room",
         "region": "region.test",
         "label": "label.test_room",
         "time": "day",
         "dark": false,
         "terrain": [
          "############",
          "#..........#",
          "#..........#",
          "#....##....#",
          "#....##....+",
          "#..........#",
          "#..........#",
          "#..........#",
          "############"
         ],
         "things": [
          { "id": "spawn_point.test_room_start", "kind": "spawn_point", "x": 2, "y": 2 },
          { "id": "door.test_room_east", "kind": "door", "x": 11, "y": 4 },
          { "id": "lock.test_room_east", "kind": "lock", "x": 11, "y": 4, "pickable": false }
         ],
         "enemies": [], "triggers": []
        }
        """);

    /// <summary>
    /// A room of 16 by 12 tiles with three enemies: one that walks a route, one that stands
    /// still, and one elite in an area (D-739, D-740, D-741).
    /// </summary>
    public static GameMap Patrolled { get; } = Of(
        "test-patrolled.json",
        """
        {
         "comment": "A room with three enemies, for the tests of the walk rule and the sight rule.",
         "id": "map.test_patrolled",
         "region": "region.test",
         "label": "label.test_patrolled",
         "time": "day",
         "dark": false,
         "terrain": [
          "################",
          "#..............#",
          "#..............#",
          "#..............#",
          "#..............#",
          "#..............#",
          "#..............#",
          "#..............#",
          "#..............#",
          "#..............#",
          "#..............#",
          "################"
         ],
         "things": [
          { "id": "spawn_point.test_patrolled_start", "kind": "spawn_point", "x": 1, "y": 1 }
         ],
         "enemies": [
          {
           "id": "patrol.test_walker",
           "group": "group.test_pair",
           "size": "common",
           "facing": "east",
           "step_ticks": 32,
           "sight_range": 3,
           "routes": [
            { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 4, "y": 5 }, { "x": 10, "y": 5 }] }
           ]
          },
          {
           "id": "patrol.test_guard",
           "group": "group.test_pair",
           "size": "common",
           "facing": "north",
           "step_ticks": 32,
           "sight_range": 2,
           "routes": [
            { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 13, "y": 9 }] }
           ]
          },
          {
           "id": "patrol.test_elite",
           "group": "group.test_elite",
           "size": "elite",
           "facing": "south",
           "step_ticks": 32,
           "sight_range": 2,
           "areas": [
            { "times": ["dawn", "day", "dusk", "night"], "x": 2, "y": 7, "width": 5, "height": 3 }
           ]
          }
         ], "triggers": []
        }
        """);

    /// <summary>A map of 40 by 24 tiles, which is larger than the view on both sides (D-633).</summary>
    public static GameMap Large { get; } = Of("test-large.json", Open("map.test_large", "label.test_large", 40, 24));

    /// <summary>A map of 9 by 5 tiles, which is smaller than the view on both sides (F-52).</summary>
    public static GameMap Small { get; } = Of("test-small.json", Open("map.test_small", "label.test_small", 9, 5));

    /// <summary>
    /// The first dungeon of the checkout (D-528). A test that reads a save of format 1 takes
    /// this map, because the migration of that format puts the party on the first map
    /// (D-166, D-654).
    /// </summary>
    public static GameMap FixtureDungeon { get; } = GameMap.Read(
        File.ReadAllBytes(RepositoryRoot.PathTo(FixtureDungeonPath)),
        FixtureDungeonPath);

    /// <summary>Reads one map from the text of a map file (D-528).</summary>
    /// <param name="file">The name of the file, which each error names (T-2).</param>
    /// <param name="text">The text of the map file.</param>
    /// <returns>The map.</returns>
    public static GameMap Of(string file, string text) => GameMap.Read(Encoding.UTF8.GetBytes(text), file);

    /// <summary>Gives an open map of one size: a wall on each edge, floor inside, and the spawn point at (1, 1).</summary>
    /// <param name="id">The id of the map, of the kind `map`.</param>
    /// <param name="width">The count of columns.</param>
    /// <param name="height">The count of rows.</param>
    /// <returns>The map.</returns>
    public static GameMap OpenOf(string id, int width, int height) => Of($"{id}.json", Open(id, "label.test_open", width, height));

    /// <summary>Makes the text of an open map of one size, with a wall around its edge.</summary>
    private static string Open(string id, string label, int width, int height)
    {
        var text = new StringBuilder();
        text.Append("{\n \"comment\": \"An open map for the tests of the camera.\",\n");
        text.Append($" \"id\": \"{id}\",\n \"region\": \"region.test\",\n \"label\": \"{label}\",\n \"time\": \"day\",\n \"dark\": false,\n \"terrain\": [\n");
        for (int row = 0; row < height; row += 1)
        {
            bool edge = row == 0 || row == height - 1;
            string line = edge ? new string('#', width) : "#" + new string('.', width - 2) + "#";
            text.Append($"  \"{line}\"{(row == height - 1 ? string.Empty : ",")}\n");
        }

        text.Append(" ],\n \"things\": [\n");
        text.Append($"  {{ \"id\": \"spawn_point.{id[4..]}_start\", \"kind\": \"spawn_point\", \"x\": 1, \"y\": 1 }}\n");
        text.Append(" ],\n \"enemies\": [],\n \"triggers\": []\n}\n");
        return text.ToString();
    }
}
