using System;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Tests;

/// <summary>
/// The overworlds of the tests of PR-109: grass of 7 by 3 tiles inside mountains, with the zones
/// that a test gives (D-1249, D-1262). Each one is a map file, read by the one reader of Core (T-1).
/// </summary>
/// <remarks>
/// The spawn point sits at (1, 2), and the entrance of the test place at (1, 1). The zone grid of
/// each map gives the column of the entrance and the spawn to the road, at rate zero, and each
/// other walkable tile to the zone of the test. The test place of <see cref="OverworldMaps"/>
/// leads back onto the marker at (1, 3).
/// </remarks>
public static class EncounterMaps
{
    /// <summary>The flag of the zone with a condition, which the flag file of the test story declares.</summary>
    public const string ZoneFlag = OverworldMaps.GateFlag;

    /// <summary>The column where the walk of a test turns back to the east.</summary>
    public const int WestColumn = 2;

    /// <summary>The column where the walk of a test turns back to the west.</summary>
    public const int EastColumn = 7;

    /// <summary>The road at rate zero, on the column of the entrance, the spawn, and the marker.</summary>
    private const string Road = """{ "id": "zone.test_road", "key": "r", "region": "region.test", "rate": 0, "groups": [], "condition": { "always": true } }""";

    /// <summary>The zone grid of each test overworld: the road on the west column, and the zone `z` east of it.</summary>
    public static readonly string[] Grid = [".........", ".rzzzzzz.", ".rzzzzzz.", ".rzzzzzz.", "........."];

    /// <summary>Gives the text of an overworld whose tiles east of the road form one zone.</summary>
    /// <param name="zone">The JSON object of that zone, with the key `z`.</param>
    /// <param name="grid">The rows of the zone grid, or no value for <see cref="Grid"/>.</param>
    /// <returns>The text of the map file.</returns>
    public static string TextOf(string zone, string[]? grid = null) => $$"""
        {
         "comment": "An overworld for the tests of PR-109.",
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
          { "id": "spawn_point.test_overworld_start", "kind": "spawn_point", "x": 1, "y": 2 },
          { "id": "entrance.test_overworld_place", "kind": "entrance", "x": 1, "y": 1, "to": "map.test_place" },
          { "id": "marker.test_overworld_place", "kind": "marker", "x": 1, "y": 3 }
         ],
         "zones": [
          {{Road}},
          {{zone}}
         ],
         "zone_grid": [{{string.Join(", ", Array.ConvertAll(grid ?? Grid, row => $"\"{row}\""))}}],
         "enemies": [], "npcs": [], "services": [], "reopen": [], "triggers": []
        }
        """;

    /// <summary>Gives the JSON object of a zone with the key `z` and no condition.</summary>
    /// <param name="rate">The rate, in basis points.</param>
    /// <param name="groups">The JSON array of the groups.</param>
    /// <returns>The object.</returns>
    public static string Zone(int rate, string groups) =>
        $$"""{ "id": "zone.test_wild", "key": "z", "region": "region.test", "rate": {{rate}}, "groups": {{groups}}, "condition": { "always": true } }""";

    /// <summary>Gives the JSON object of a zone with the key `z` that runs while the zone flag is on.</summary>
    /// <param name="rate">The rate, in basis points.</param>
    /// <returns>The object, whose one group is `group.one`.</returns>
    public static string FlaggedZone(int rate) =>
        $$"""{ "id": "zone.test_wild", "key": "z", "region": "region.test", "rate": {{rate}}, "groups": [{ "group": "group.one", "weight": 1 }], "condition": { "flag": "{{ZoneFlag}}" } }""";

    /// <summary>Reads an overworld from its text.</summary>
    /// <param name="text">The text of the map file.</param>
    /// <returns>The map.</returns>
    public static GameMap Read(string text) => GameMap.Read(Encoding.UTF8.GetBytes(text), "test-overworld.json");

    /// <summary>Gives an overworld whose zone east of the road takes one rate and one list of groups.</summary>
    /// <param name="rate">The rate, in basis points.</param>
    /// <param name="groups">The JSON array of the groups.</param>
    /// <returns>The map.</returns>
    public static GameMap Of(int rate, string groups) => Read(TextOf(Zone(rate, groups)));

    /// <summary>Gives an overworld whose zone east of the road fights `group.one` at one rate.</summary>
    /// <param name="rate">The rate, in basis points.</param>
    /// <returns>The map.</returns>
    public static GameMap Of(int rate) => Of(rate, """[{ "group": "group.one", "weight": 1 }]""");

    /// <summary>Starts a run on an overworld, beside the test place (D-1255).</summary>
    /// <param name="seed">The seed of the run.</param>
    /// <param name="overworld">The overworld, where the run starts.</param>
    /// <param name="battle">The battle content, or no value for the content of the tests.</param>
    /// <returns>The run, at tick zero.</returns>
    public static Simulation Start(ulong seed, GameMap overworld, BattleContent? battle = null) =>
        Simulation.Start(seed, MapSet.Of([OverworldMaps.Place, overworld]), overworld.Id, battle ?? TestBattles.Content, OverworldMaps.Notices, TestStory.Content, DebugIntentHandlers.None);

    /// <summary>Resumes a run of an overworld from a snapshot.</summary>
    /// <param name="seed">The seed of the run.</param>
    /// <param name="overworld">The overworld of the run.</param>
    /// <param name="snapshot">The snapshot.</param>
    /// <returns>The run.</returns>
    public static Simulation Resume(ulong seed, GameMap overworld, RunSnapshot snapshot) =>
        Simulation.Resume(seed, snapshot, MapSet.Of([OverworldMaps.Place, overworld]), TestBattles.Content, OverworldMaps.Notices, TestStory.Content, DebugIntentHandlers.None);

    /// <summary>
    /// Walks the lead one tile, and runs the ticks of the step up to the arrival. A fight that the
    /// arrival starts holds the run, so the walk stops there.
    /// </summary>
    /// <param name="run">The run, with the lead standing and no fight.</param>
    /// <param name="direction">The direction of the step.</param>
    /// <exception cref="InvalidOperationException">The step does not start or does not end (T-2).</exception>
    public static void StepOnce(Simulation run, StepDirection direction)
    {
        TilePoint from = run.State.Party.LeadAt;
        run.Step([HubWalks.Move(direction)]);
        if (run.State.Party.Stepping is null)
        {
            throw new InvalidOperationException($"The lead at {from} started no step to the {StepDirections.NameOf(direction)} at tick {run.Tick}.");
        }

        for (int tick = 0; run.State.Party.Stepping is not null; tick += 1)
        {
            if (tick == 64)
            {
                throw new InvalidOperationException($"The step of the lead from {from} did not end in 64 ticks.");
            }

            run.Step([]);
        }
    }

    /// <summary>
    /// Walks the lead back and forth on its row, between <see cref="WestColumn"/> and
    /// <see cref="EastColumn"/>, until a fight starts or the count of steps ends.
    /// </summary>
    /// <param name="run">The run, with the lead standing on its row, inside the two columns.</param>
    /// <param name="steps">The most steps to walk.</param>
    /// <returns>The count of steps up to the arrival that started a fight, or no value when no fight started.</returns>
    public static int? WalkUntilFight(Simulation run, int steps)
    {
        StepDirection direction = StepDirection.East;
        for (int step = 1; step <= steps; step += 1)
        {
            int x = run.State.Party.LeadAt.X;
            if (x >= EastColumn)
            {
                direction = StepDirection.West;
            }
            else if (x <= WestColumn)
            {
                direction = StepDirection.East;
            }

            StepOnce(run, direction);
            if (run.State.Battle is not null)
            {
                return step;
            }
        }

        return null;
    }
}
