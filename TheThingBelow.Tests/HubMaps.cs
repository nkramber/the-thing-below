using TheThingBelow.Core.Maps;

namespace TheThingBelow.Tests;

/// <summary>
/// The hub maps that the tests of PR-14 read. Each one is a map file, read by the one reader of
/// Core, so a test never builds an NPC or a service by another path (T-1, D-1131, D-1137).
/// </summary>
/// <remarks>
/// The ground is a room of 10 by 8 tiles with a block of wall in the middle, as the room of
/// <see cref="PatrolMaps"/>. The spawn point sits at (1, 1), and the bed, a service point, sits
/// at (8, 1).
/// </remarks>
public static class HubMaps
{
    /// <summary>The bed, a service point in the north-east corner (D-1142).</summary>
    public const string Bed = """{ "id": "service_point.hub_bed", "kind": "service_point", "x": 8, "y": 1 }""";

    /// <summary>A marker in the south-west corner, which is a thing that is not solid.</summary>
    public const string Marker = """{ "id": "marker.hub_corner", "kind": "marker", "x": 1, "y": 6 }""";

    /// <summary>The keeper, a route NPC of one tile that stands still at (2, 6) (D-740).</summary>
    public const string Keeper = """{ "id": "npc.hub_keeper", "facing": "north", "step_ticks": 32, "move": "route", "tiles": [{ "x": 2, "y": 6, "wait_ticks": 0 }] }""";

    /// <summary>The rest of the keeper, which no flag gates.</summary>
    public const string RestOnKeeper = """{ "id": "service.hub_rest", "kind": "rest", "npc": "npc.hub_keeper", "condition": { "always": true } }""";

    /// <summary>The save of the bed, which no flag gates.</summary>
    public const string SaveOnBed = """{ "id": "service.hub_save", "kind": "save", "thing": "service_point.hub_bed", "condition": { "always": true } }""";

    /// <summary>A hub with the keeper and the bed, and a service on each.</summary>
    public static GameMap Inn => Of(npcs: Keeper, services: $"{RestOnKeeper}, {SaveOnBed}", things: Bed);

    /// <summary>Makes the text of one wander NPC, with a default for each field (D-1138).</summary>
    /// <param name="id">The id of the NPC, of the kind `npc`.</param>
    /// <param name="x">The column of the start tile.</param>
    /// <param name="y">The row of the start tile.</param>
    /// <param name="areas">The text of the `areas` array, without its brackets.</param>
    /// <param name="paceTicks">The ticks from one choice to the next.</param>
    /// <param name="stepTicks">The ticks of one step (D-821).</param>
    /// <param name="extra">More fields of the record, each with a comma after it, for a test of a refusal.</param>
    /// <returns>The text of the record, for the `npcs` array of a map file.</returns>
    public static string Wanderer(
        string id = "npc.hub_dog",
        int x = 6,
        int y = 5,
        string areas = """{ "x": 6, "y": 5, "width": 3, "height": 2 }""",
        int paceTicks = 64,
        int stepTicks = 32,
        string extra = "") =>
        $$"""{ "id": "{{id}}", "facing": "south", "step_ticks": {{stepTicks}}, "move": "wander", {{extra}} "x": {{x}}, "y": {{y}}, "areas": [{{areas}}], "pace_ticks": {{paceTicks}} }""";

    /// <summary>Makes the text of one route NPC (D-739, D-1138).</summary>
    /// <param name="id">The id of the NPC, of the kind `npc`.</param>
    /// <param name="tiles">The text of the `tiles` array, without its brackets.</param>
    /// <returns>The text of the record, for the `npcs` array of a map file.</returns>
    public static string Walker(
        string id = "npc.hub_barmaid",
        string tiles = """{ "x": 1, "y": 2, "wait_ticks": 30 }, { "x": 3, "y": 2, "wait_ticks": 0 }, { "x": 3, "y": 5, "wait_ticks": 60 }""") =>
        $$"""{ "id": "{{id}}", "facing": "east", "step_ticks": 16, "move": "route", "tiles": [{{tiles}}] }""";

    /// <summary>Makes the text of one chaser, which closes on the wanderer by default (D-1138).</summary>
    /// <param name="id">The id of the NPC, of the kind `npc`.</param>
    /// <param name="target">The id of the NPC that it chases.</param>
    /// <returns>The text of the record, for the `npcs` array of a map file.</returns>
    public static string Chaser(string id = "npc.hub_child", string target = "npc.hub_dog") =>
        $$"""{ "id": "{{id}}", "facing": "west", "step_ticks": 32, "move": "chase", "x": 8, "y": 6, "areas": [{ "x": 6, "y": 5, "width": 3, "height": 2 }], "pace_ticks": 32, "target": "{{target}}" }""";

    /// <summary>Reads a map of 10 by 8 tiles with the NPCs, the services, and the things of one test.</summary>
    /// <param name="npcs">The text of the `npcs` array, without its brackets.</param>
    /// <param name="services">The text of the `services` array, without its brackets.</param>
    /// <param name="things">The things beside the spawn point, each with no comma after the last.</param>
    /// <param name="kind">The kind of the map (D-112).</param>
    /// <param name="enemies">The text of the `enemies` array, without its brackets.</param>
    /// <returns>The map.</returns>
    public static GameMap Of(string npcs = "", string services = "", string things = "", string kind = "hub", string enemies = "") =>
        TestMaps.Of("hub-test.json", Text(npcs, services, things, kind, enemies));

    /// <summary>Gives the text of the map of <see cref="Of"/>, for a test that changes it.</summary>
    /// <param name="npcs">The text of the `npcs` array, without its brackets.</param>
    /// <param name="services">The text of the `services` array, without its brackets.</param>
    /// <param name="things">The things beside the spawn point, each with no comma after the last.</param>
    /// <param name="kind">The kind of the map (D-112).</param>
    /// <param name="enemies">The text of the `enemies` array, without its brackets.</param>
    /// <returns>The text of the map file.</returns>
    public static string Text(string npcs = "", string services = "", string things = "", string kind = "hub", string enemies = "") =>
        $$"""
        {
         "comment": "a room with a wall block and the NPCs and the services of one test",
         "id": "map.hub_test",
         "region": "region.test",
         "label": "label.hub_test",
         "kind": "{{kind}}",
         "time": "day",
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
          { "id": "spawn_point.hub_test_start", "kind": "spawn_point", "x": 1, "y": 1 }{{(things.Length == 0 ? string.Empty : ", " + things)}}
         ],
         "enemies": [{{enemies}}],
         "npcs": [{{npcs}}],
         "services": [{{services}}],
         "triggers": []
        }
        """;
}
