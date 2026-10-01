using System;
using System.IO;
using System.Text;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Tests;

/// <summary>
/// The maps of the tests of the dungeon parts of PR-16: a vault with each door, lock, chest,
/// save point, and exit, and the fixture dungeon with its exit to a test room (D-41, D-1216).
/// </summary>
/// <remarks>
/// The vault is 12 by 8 tiles. The lead starts at (1, 1). A guard stands at the east end of the
/// hall at (10, 4), and it sees the tiles that touch it alone. The chest sits at (10, 2), the save point at (10, 1), and the exit at
/// (1, 2), which leads to <see cref="TestMaps.Room"/>. A plain door at (5, 3) leads south to a
/// hall, where the tile (1, 4) plays the story scene that sets the victory flag, and that flag
/// reopens the vault. From the hall, a story lock at (2, 5) takes the test key, and a pickable
/// lock at (8, 5) takes no key.
/// </remarks>
public static class PartsMaps
{
    /// <summary>The tile of the chest.</summary>
    public static readonly TilePoint Chest = new(10, 2);

    /// <summary>The tile of the plain door.</summary>
    public static readonly TilePoint PlainDoor = new(5, 3);

    /// <summary>The tile of the door of the story lock.</summary>
    public static readonly TilePoint StoryDoor = new(2, 5);

    /// <summary>The tile of the door of the pickable lock.</summary>
    public static readonly TilePoint PickedDoor = new(8, 5);

    /// <summary>The id of the guard.</summary>
    public static readonly ContentId Guard = ContentId.Parse("patrol.test_vault_guard", "test", "guard");

    /// <summary>The id of the key of the story lock, a key of the Keyring (D-1219).</summary>
    public static readonly ContentId Key = ContentId.Parse("item.test_key", "test", "key");

    /// <summary>The text of the vault map file.</summary>
    public const string VaultFile = """
    {
     "comment": "A vault with each part of a dungeon, for the tests of PR-16.",
     "id": "map.test_vault",
     "region": "region.test",
     "label": "label.test_room",
     "time": "day",
     "dark": false,
     "kind": "dungeon", "npcs": [], "services": [], "zones": [], "zone_grid": [], "time_changes": [], "reopen": ["flag.test_victor"],
     "terrain": [
      "############",
      "#..........#",
      "#..........#",
      "#####+######",
      "#..........#",
      "##+#####+###",
      "#...#......#",
      "############"
     ],
     "things": [
      { "id": "spawn_point.test_vault_start", "kind": "spawn_point", "x": 1, "y": 1 },
      { "id": "save_point.test_vault_stone", "kind": "save_point", "x": 10, "y": 1 },
      {
       "id": "chest.test_vault_store", "kind": "chest", "x": 10, "y": 2,
       "contents": [
        { "thing": "item.fixture_draught", "count": 4 },
        { "thing": "lesson.test_pilfer", "count": 1, "fallback": "item.test_salts" },
        { "thing": "lesson.fixture_salve", "count": 1, "fallback": "item.test_tonic" }
       ],
       "gold": 25
      },
      { "id": "exit.test_vault_out", "kind": "exit", "x": 1, "y": 2, "to": "map.test_room" },
      { "id": "door.test_vault_plain", "kind": "door", "x": 5, "y": 3 },
      { "id": "door.test_vault_story", "kind": "door", "x": 2, "y": 5 },
      { "id": "lock.test_vault_story", "kind": "lock", "x": 2, "y": 5, "pickable": false, "key": "item.test_key" },
      { "id": "door.test_vault_picked", "kind": "door", "x": 8, "y": 5 },
      { "id": "lock.test_vault_picked", "kind": "lock", "x": 8, "y": 5, "pickable": true, "key": "none" }
     ],
     "enemies": [
      {
       "id": "patrol.test_vault_guard",
       "group": "group.one",
       "size": "common",
       "facing": "west",
       "step_ticks": 32,
       "sight_range": 0,
       "routes": [{ "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 10, "y": 4 }] }]
      }
     ],
     "triggers": [
      {
       "id": "trigger.test_vault_reopen",
       "kind": "tile",
       "x": 1,
       "y": 4,
       "scene": "scene.test_victory",
       "condition": { "not": { "flag": "flag.test_victor" } }
      }
     ]
    }
    """;

    /// <summary>The vault, as a run reads it.</summary>
    public static GameMap Vault { get; } = TestMaps.Of("test-vault.json", VaultFile);

    /// <summary>The vault and the room that its exit enters.</summary>
    public static MapSet VaultAndRoom => MapSet.Of([Vault, TestMaps.Room]);

    /// <summary>
    /// The fixture dungeon of the checkout, with its exit to <see cref="TestMaps.Room"/> in place of
    /// the fixture hub and no reopen flag, so the content of the tests can run it (D-1216).
    /// </summary>
    public static GameMap FixtureDungeonToRoom { get; } = TestMaps.Of(
        "fixture-dungeon-to-room.json",
        File.ReadAllText(RepositoryRoot.PathTo("content/rules/maps/fixture-dungeon.json"))
            .Replace("\"to\": \"map.fixture_overworld\", \"arrive\": \"marker.fixture_overworld_cut\"", "\"to\": \"map.test_room\"", StringComparison.Ordinal)
            .Replace("\"reopen\": [\"flag.fixture_hub_yes\"]", "\"reopen\": []", StringComparison.Ordinal));

    /// <summary>Starts a run on the vault with the room of its exit, with the story content of the tests.</summary>
    /// <param name="seed">The seed.</param>
    /// <returns>The run.</returns>
    public static Simulation Start(ulong seed) =>
        Simulation.Start(seed, VaultAndRoom, Vault.Id, TestBattles.Content, TestBattles.Notices, TestStory.Content, DebugIntentHandlers.None);

    /// <summary>Gives a text in UTF-8, for a test that reads a changed map file.</summary>
    /// <param name="text">The text.</param>
    /// <returns>The bytes.</returns>
    public static byte[] Bytes(string text) => Encoding.UTF8.GetBytes(text);
}
