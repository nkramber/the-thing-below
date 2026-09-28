using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Tests;

/// <summary>
/// The maps of the tests of PR-64: a hall of four traps, and a yard of deep snow, ice, and bad air
/// (D-1226, D-1227).
/// </summary>
/// <remarks>
/// The hall is 13 by 3 tiles. The lead starts at (1, 1) and walks east over each trap in turn: a
/// damage trap of 2500 basis points at (3, 1), a trap of poison at (5, 1), a trap of silence at
/// (7, 1), and an encounter trap of the group `group.one` at (9, 1).
/// <para>
/// The yard is 12 by 7 tiles. The lead starts at (1, 1). Deep snow lies at (3, 1) and (4, 1). A row
/// of ice lies from (2, 3) to (5, 3), and a wall at (6, 3) ends it. Bad air lies at (1, 5) and (2, 5).
/// </para>
/// </remarks>
public static class TrapMaps
{
    /// <summary>The tile of the damage trap of the hall.</summary>
    public static readonly TilePoint Blade = new(3, 1);

    /// <summary>The tile of the trap of poison of the hall.</summary>
    public static readonly TilePoint Needle = new(5, 1);

    /// <summary>The tile of the trap of silence of the hall.</summary>
    public static readonly TilePoint Dust = new(7, 1);

    /// <summary>The tile of the encounter trap of the hall.</summary>
    public static readonly TilePoint Alarm = new(9, 1);

    /// <summary>The text of the hall map file.</summary>
    public const string HallFile = """
    {
     "comment": "A hall of four traps, for the tests of PR-64.",
     "id": "map.test_hall",
     "region": "region.test",
     "label": "label.test_room",
     "time": "day",
     "dark": false,
     "kind": "dungeon", "npcs": [], "services": [], "zones": [], "zone_grid": [], "reopen": [],
     "terrain": [
      "#############",
      "#...........#",
      "#############"
     ],
     "things": [
      { "id": "spawn_point.test_hall_start", "kind": "spawn_point", "x": 1, "y": 1 },
      { "id": "trap.test_hall_blade", "kind": "trap", "x": 3, "y": 1, "harm": "damage", "share": 2500 },
      { "id": "trap.test_hall_needle", "kind": "trap", "x": 5, "y": 1, "harm": "status", "status": "poison" },
      { "id": "trap.test_hall_dust", "kind": "trap", "x": 7, "y": 1, "harm": "status", "status": "silence" },
      { "id": "trap.test_hall_alarm", "kind": "trap", "x": 9, "y": 1, "harm": "encounter", "group": "group.one" }
     ],
     "enemies": [], "triggers": []
    }
    """;

    /// <summary>The text of the yard map file.</summary>
    public const string YardFile = """
    {
     "comment": "A yard of deep snow, ice, and bad air, for the tests of PR-64.",
     "id": "map.test_yard",
     "region": "region.test",
     "label": "label.test_room",
     "time": "day",
     "dark": false,
     "kind": "dungeon", "npcs": [], "services": [], "zones": [], "zone_grid": [], "reopen": [],
     "terrain": [
      "############",
      "#..**......#",
      "#..........#",
      "#.====#....#",
      "#..........#",
      "#~~........#",
      "############"
     ],
     "things": [
      { "id": "spawn_point.test_yard_start", "kind": "spawn_point", "x": 1, "y": 1 }
     ],
     "enemies": [], "triggers": []
    }
    """;

    /// <summary>The hall, as a run reads it.</summary>
    public static GameMap Hall { get; } = TestMaps.Of("test-hall.json", HallFile);

    /// <summary>The yard, as a run reads it.</summary>
    public static GameMap Yard { get; } = TestMaps.Of("test-yard.json", YardFile);

    /// <summary>The id of one trap of the hall, from its tile.</summary>
    /// <param name="at">The tile of the trap.</param>
    /// <returns>The id.</returns>
    public static ContentId TrapOf(TilePoint at) => Hall.TrapAt(at)!.Id;

    /// <summary>Walks the lead east to one tile of the hall, one step at a time.</summary>
    /// <param name="run">The run, with the lead standing on the row of the hall.</param>
    /// <param name="column">The column to reach.</param>
    public static void WalkTo(Simulation run, int column) =>
        HubWalks.Walk(run, StepDirection.East, column - run.State.Party.LeadAt.X);
}
