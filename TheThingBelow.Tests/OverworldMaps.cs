using System.IO;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Notices;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Tests;

/// <summary>
/// The two maps of the tests of the overworld of PR-35: a place with an exit, and an overworld with
/// the entrance of that place, the marker of its exit, and a gate (D-1243, D-1255). Each one is a
/// map file, read by the one reader of Core (T-1).
/// </summary>
/// <remarks>
/// The place is a corridor of five tiles, with the spawn point at (1, 1) and the exit at (5, 1). The
/// overworld is grass of 7 by 3 tiles inside mountains. The entrance sits at (1, 1), the marker of
/// the exit at (1, 2), the spawn point at (4, 2), and the gate at (6, 2).
/// </remarks>
public static class OverworldMaps
{
    /// <summary>The flag that opens the gate of the test overworld, which the checkout declares.</summary>
    public const string GateFlag = "flag.fixture_hub_rats";

    /// <summary>The notice of the gate, which the notice file of the checkout holds (D-1257).</summary>
    public const string GateNotice = "notice.fixture_gate_shut";

    /// <summary>The terrain rows and the things of the test overworld, which a load test changes.</summary>
    public const string OverworldText = """
        {
         "comment": "An overworld for the tests of PR-35.",
         "id": "map.test_overworld",
         "region": "region.test",
         "label": "label.test_overworld",
         "kind": "overworld",
         "time": "day",
         "dark": false,
         "terrain": [
          "^^^^^^^^^",
          "^,,,,,,,^",
          "^,,,,,,,^",
          "^,,,,,,,^",
          "^^^^^^^^^"
         ],
         "things": [
          { "id": "spawn_point.test_overworld_start", "kind": "spawn_point", "x": 4, "y": 2 },
          { "id": "entrance.test_overworld_place", "kind": "entrance", "x": 1, "y": 1, "to": "map.test_place" },
          { "id": "marker.test_overworld_place", "kind": "marker", "x": 1, "y": 2 },
          { "id": "gate.test_overworld_pass", "kind": "gate", "x": 6, "y": 2, "condition": { "flag": "flag.fixture_hub_rats" }, "notice": "notice.fixture_gate_shut" }
         ],
         "enemies": [], "npcs": [], "services": [], "reopen": [], "triggers": []
        }
        """;

    /// <summary>The place, a dungeon whose exit leads to the marker of the test overworld (D-1255).</summary>
    public static GameMap Place { get; } = TestMaps.Of(
        "test-place.json",
        """
        {
         "comment": "A place with an exit to the test overworld.",
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
          { "id": "spawn_point.test_place_start", "kind": "spawn_point", "x": 1, "y": 1 },
          { "id": "exit.test_place_out", "kind": "exit", "x": 5, "y": 1, "to": "map.test_overworld", "arrive": "marker.test_overworld_place" }
         ],
         "enemies": [], "npcs": [], "services": [], "reopen": [], "triggers": []
        }
        """);

    /// <summary>The test overworld.</summary>
    public static GameMap Overworld { get; } = TestMaps.Of("test-overworld.json", OverworldText);

    /// <summary>The notice file of the checkout, which holds the notice of the gate.</summary>
    public static NoticeList Notices { get; } =
        NoticeList.Read(File.ReadAllBytes(RepositoryRoot.PathTo("content/rules/notices.json")), NoticeList.Path);

    /// <summary>The two maps as the maps of a run.</summary>
    public static MapSet Both => MapSet.Of([Place, Overworld]);

    /// <summary>Starts a run on one of the two maps.</summary>
    /// <param name="seed">The seed of the run.</param>
    /// <param name="first">The map where the run starts.</param>
    /// <returns>The run, at tick zero.</returns>
    public static Simulation Start(ulong seed, GameMap first) =>
        Simulation.Start(seed, Both, first.Id, TestBattles.Content, Notices, TestStory.Content, DebugIntentHandlers.None);

    /// <summary>Gives the id of one test string, for an assert.</summary>
    /// <param name="value">The id text.</param>
    /// <returns>The id.</returns>
    public static ContentId Id(string value) => ContentId.Parse(value, "test", "id");

    /// <summary>Reads a changed copy of the test overworld.</summary>
    /// <param name="text">The text of the map file.</param>
    /// <returns>The map.</returns>
    public static GameMap Read(string text) => GameMap.Read(Encoding.UTF8.GetBytes(text), "test-overworld.json");
}
