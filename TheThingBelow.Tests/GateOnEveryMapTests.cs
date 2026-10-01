using System;
using System.Linq;
using TheThingBelow.Core;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// A gate on a hub and on a dungeon, which PR-17 allows for the paid door of the hanging cells
/// (D-1347). The gate keeps the rules of the overworld gate: a shut gate refuses the step, and a
/// confirm at it posts its notice (D-1243, D-1257).
/// </summary>
public sealed class GateOnEveryMapTests
{
    private const ulong Seed = 20260928;

    /// <summary>A dungeon with a gate two tiles east of the spawn point, and an exit to the test overworld.</summary>
    private static readonly GameMap DungeonWithGate = TestMaps.Of("test-place.json", PlaceText("dungeon"));

    [Theory]
    [InlineData("dungeon")]
    [InlineData("hub")]
    public void AHubOrADungeonCanHoldAGate(string kind)
    {
        GameMap map = TestMaps.Of("test-place.json", PlaceText(kind));

        MapThing gate = map.GateAt(new TilePoint(3, 1))
            ?? throw new InvalidOperationException($"The {kind} holds no gate at 3,1 (T-2).");
        Assert.Equal("gate.test_place_door", gate.Id.Value);
    }

    [Fact]
    public void AShutGateOnADungeonRefusesTheStepAndAConfirmPostsItsNotice()
    {
        // Exit test 10 of PR-17 (D-1257, D-1347).
        Simulation run = Start();
        HubWalks.Walk(run, StepDirection.East, 1);
        HubWalks.Face(run, StepDirection.East);
        Assert.Empty(run.TakeNotices());

        HubWalks.Confirm(run);

        Assert.Equal(new TilePoint(2, 1), run.State.Party.LeadAt);
        Assert.Equal([OverworldMaps.GateNotice], run.TakeNotices().Select(notice => notice.Id.Value));
    }

    [Fact]
    public void AnOpenGateOnADungeonLetsTheLeadPass()
    {
        // Exit test 10 of PR-17 (D-543, D-1347).
        Simulation run = Start();
        HubWalks.Walk(run, StepDirection.East, 1);
        run.State.Story.Flags.TurnOn(OverworldMaps.Id(OverworldMaps.GateFlag));

        HubWalks.Walk(run, StepDirection.East, 1);

        Assert.Equal(new TilePoint(3, 1), run.State.Party.LeadAt);
    }

    private static Simulation Start() =>
        Simulation.Start(
            Seed,
            MapSet.Of([DungeonWithGate, OverworldMaps.Overworld]),
            DungeonWithGate.Id,
            TestBattles.Content,
            OverworldMaps.Notices,
            TestStory.Content,
            DebugIntentHandlers.None);

    private static string PlaceText(string kind) => $$"""
        {
         "comment": "A place with a gate for the tests of PR-17.",
         "id": "map.test_place",
         "region": "region.test",
         "label": "label.test_place",
         "kind": "{{kind}}",
         "time": "day",
         "dark": false,
         "terrain": [
          "#######",
          "#.....#",
          "#######"
         ],
         "things": [
          { "id": "spawn_point.test_place_start", "kind": "spawn_point", "x": 1, "y": 1 },
          { "id": "gate.test_place_door", "kind": "gate", "x": 3, "y": 1, "condition": { "flag": "{{OverworldMaps.GateFlag}}" }, "notice": "{{OverworldMaps.GateNotice}}" },
          { "id": "exit.test_place_out", "kind": "exit", "x": 5, "y": 1, "to": "map.test_overworld", "arrive": "marker.test_overworld_place" }
         ],
         "enemies": [], "npcs": [], "services": [], "zones": [], "zone_grid": [], "time_changes": [], "reopen": [], "triggers": []
        }
        """;
}
