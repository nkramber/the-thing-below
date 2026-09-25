using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Streams;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// Exit test 14 of PR-14: an NPC never steps onto the lead, a wall, a thing, or another NPC, and
/// the lead never walks through an NPC (D-1139). An NPC and an enemy block each other too.
/// </summary>
/// <remarks>
/// The seed loop plays the yard of <see cref="HubMaps"/> with a marker inside the range of the dog
/// and the child, and the lead walks at random through the script of <see cref="RunScripts"/>.
/// Each failure names its seed (T-3).
/// </remarks>
public sealed class NpcBlockTests
{
    /// <summary>The count of seeds of the property test of this class (T-3).</summary>
    private const int SeedCount = 200;

    /// <summary>The count of ticks of each run of the seed loop.</summary>
    private const int TickCount = 1500;

    /// <summary>A marker inside the range of the dog and the child: a thing that is not solid.</summary>
    private const string MarkerInRange = """{ "id": "marker.hub_yard", "kind": "marker", "x": 7, "y": 6 }""";

    /// <summary>The porter walks from (3, 1) to (2, 1), east of the spawn point, with no wait.</summary>
    private const string Porter = """{ "id": "npc.hub_porter", "facing": "west", "step_ticks": 32, "move": "route", "tiles": [{ "x": 3, "y": 1, "wait_ticks": 0 }, { "x": 2, "y": 1, "wait_ticks": 0 }] }""";

    [Fact]
    public void NoNpcSharesATileWithTheLeadAWallAThingOrAnotherNpc()
    {
        GameMap map = HubMaps.Of(
            npcs: $"{HubMaps.Keeper}, {HubMaps.Walker()}, {HubMaps.Wanderer()}, {HubMaps.Chaser()}",
            things: MarkerInRange);
        for (ulong seed = 1; seed <= SeedCount; seed += 1)
        {
            Simulation run = Simulation.Start(seed, map, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
            foreach (IReadOnlyList<Intent> intents in RunScripts.Make(seed, TickCount))
            {
                run.Step(intents);
                AssertApart(run.State.Party, seed, run.Tick);
            }
        }
    }

    [Fact]
    public void AStepOfTheLeadIntoAnNpcTurnsTheLeadAlone()
    {
        // D-1139: the lead faces the NPC and never walks through it, and no bump follows.
        GameMap map = HubMaps.Of(npcs: Standing("npc.hub_porter", 1, 2));
        MapState party = MapState.Enter(map);

        for (int tick = 0; tick < 40; tick += 1)
        {
            party.Want(StepDirection.South);
            PartyStep step = party.Advance();

            Assert.Null(step.Started);
            Assert.Null(step.Bumped);
        }

        Assert.Equal(map.Spawn, party.LeadAt);
        Assert.Equal(StepDirection.South, party.Facing);
        Assert.Null(party.Stepping);
    }

    [Fact]
    public void TheLeadNeverStepsIntoTheTileWhereTheStepOfAnNpcEnds()
    {
        // D-1139: the porter starts a step from (3, 1) into (2, 1) on the first tick, so the tile
        // east of the spawn point is the end of its step when the lead asks for it.
        Simulation run = Start(HubMaps.Of(npcs: Porter));

        run.Step([]);
        NpcState porter = Assert.Single(run.State.Party.Npcs.All);
        Assert.Equal(StepDirection.West, porter.Stepping);

        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);

        Assert.Null(run.State.Party.Stepping);
        Assert.Equal(StepDirection.East, run.State.Party.Facing);
        Assert.Equal(run.State.Party.Map.Spawn, run.State.Party.LeadAt);
    }

    [Fact]
    public void ARouteNpcWaitsForTheLeadAndThenWalksOn()
    {
        // D-1139: the lead starts its step into (2, 1) first, so the porter turns to it and waits,
        // and a blocked route step tries again on each tick. The porter walks on when the lead
        // leaves its route.
        Simulation run = Start(HubMaps.Of(npcs: Porter));

        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        for (int tick = 0; tick < 40; tick += 1)
        {
            run.Step([]);
        }

        NpcState porter = Assert.Single(run.State.Party.Npcs.All);
        Assert.Equal(new TilePoint(2, 1), run.State.Party.LeadAt);
        Assert.Equal((new TilePoint(3, 1), StepDirection.West), (porter.At, porter.Facing));
        Assert.Null(porter.Stepping);

        // The lead holds (2, 1) until its step ends on the 16th tick after the intent (D-203).
        run.Step([Intent.OfPlayer(IntentIds.MoveSouth)]);
        for (int tick = 0; tick < MapRules.TicksPerStep - 1; tick += 1)
        {
            run.Step([]);
        }

        Assert.Null(porter.Stepping);
        run.Step([]);

        Assert.Equal(StepDirection.West, porter.Stepping);
    }

    [Fact]
    public void AnNpcNeverStepsOntoTheBodyOfAnEnemy()
    {
        // D-1139: the child chases the post at (8, 6), and the only step that brings it closer
        // inside its row is the tile of the enemy, so it pauses.
        GameMap map = HubMaps.Of(
            npcs: $"{Standing("npc.hub_post", 8, 6)}, {ChaserInRow("npc.hub_post")}",
            enemies: StandingEnemy(7, 5));
        MapState party = MapState.Enter(map);

        Walk(party, 200);

        NpcState child = party.Npcs.All[1];
        Assert.Equal((new TilePoint(6, 5), StepDirection.West), (child.At, child.Facing));
        Assert.Null(child.Stepping);
    }

    [Fact]
    public void AChaserWithNoEnemyInItsWayStepsOn()
    {
        // The control of the test above: with no enemy, the same chaser steps east.
        MapState party = MapState.Enter(HubMaps.Of(npcs: $"{Standing("npc.hub_post", 8, 6)}, {ChaserInRow("npc.hub_post")}"));

        Walk(party, 1);

        Assert.Equal(StepDirection.East, party.Npcs.All[1].Stepping);
    }

    [Fact]
    public void AnEnemyNeverStepsOntoAnNpc()
    {
        // D-1139: the enemy walks a route from (1, 5) to (3, 5), and a porter stands at (2, 5).
        GameMap map = HubMaps.Of(
            npcs: Standing("npc.hub_porter", 2, 5),
            enemies: PatrolMaps.Enemy(stations: """
               "routes": [
                { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 1, "y": 5 }, { "x": 3, "y": 5 }] }
               ]
              """));
        MapState party = MapState.Enter(map);
        RandomStream stream = RandomStreams.Open(SaveRuns.Seed, StreamId.Exploration);
        List<PatrolState> moved = [];

        for (int tick = 0; tick < 200; tick += 1)
        {
            party.Patrols.Walk(map, party, stream, new RunContext(SaveRuns.Seed, tick, "the test"), moved);
        }

        PatrolState enemy = Assert.Single(party.Patrols.All);
        Assert.Empty(moved);
        Assert.Equal((new TilePoint(1, 5), StepDirection.East), (enemy.At, enemy.Facing));
        Assert.Null(enemy.Stepping);
    }

    private static string Standing(string id, int x, int y) =>
        $$"""{ "id": "{{id}}", "facing": "north", "step_ticks": 32, "move": "route", "tiles": [{ "x": {{x}}, "y": {{y}}, "wait_ticks": 0 }] }""";

    private static string ChaserInRow(string target) =>
        $$"""{ "id": "npc.hub_child", "facing": "west", "step_ticks": 32, "move": "chase", "x": 6, "y": 5, "areas": [{ "x": 6, "y": 5, "width": 3, "height": 1 }], "pace_ticks": 32, "target": "{{target}}" }""";

    private static string StandingEnemy(int x, int y) => PatrolMaps.Enemy(stations: $$"""
         "routes": [
          { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": {{x}}, "y": {{y}} }] }
         ]
        """);

    private static Simulation Start(GameMap map) =>
        Simulation.Start(SaveRuns.Seed, map, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

    /// <summary>Walks the NPCs alone for a count of ticks, with no sight and no encounter.</summary>
    private static void Walk(MapState party, int ticks)
    {
        RandomStream stream = RandomStreams.Open(SaveRuns.Seed, StreamId.Npc);
        List<NpcState> moved = [];
        for (int tick = 0; tick < ticks; tick += 1)
        {
            party.Npcs.Walk(party.Map, party, stream, new RunContext(SaveRuns.Seed, tick, "the test"), moved);
        }
    }

    private static void AssertApart(MapState party, ulong seed, long tick)
    {
        List<TilePoint> lead = [party.LeadAt];
        if (party.Stepping is StepDirection walking)
        {
            lead.Add(party.LeadAt.Step(walking));
        }

        IReadOnlyList<NpcState> npcs = party.Npcs.All;
        for (int index = 0; index < npcs.Count; index += 1)
        {
            NpcState npc = npcs[index];
            List<TilePoint> held = [npc.At];
            if (npc.StepEnd is TilePoint end)
            {
                held.Add(end);
            }

            foreach (TilePoint at in held)
            {
                string name = $"Seed {seed}, tick {tick}: the NPC '{npc.Npc.Id.Value}' holds {at}";
                Assert.True(NpcState.IsOpen(party.Map, at), $"{name}, which is a wall or a thing.");
                Assert.False(lead.Contains(at), $"{name}, which the lead holds.");
                for (int other = 0; other < npcs.Count; other += 1)
                {
                    Assert.False(other != index && npcs[other].Holds(at), $"{name}, which the NPC '{npcs[other].Npc.Id.Value}' holds.");
                }
            }
        }
    }
}
