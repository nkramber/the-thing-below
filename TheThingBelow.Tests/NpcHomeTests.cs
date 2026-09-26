using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Story;
using TheThingBelow.Core.Streams;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The walk home of an NPC after a story scene (D-1140): exit test 16 of PR-14. The search and
/// the walk, a blocked step, the rejoin of a route, and the snapshot of an NPC on its way home.
/// </summary>
/// <remarks>
/// The porter stands at (3, 4), west of the wall block of the room of <see cref="HubMaps"/>. A talk
/// with it starts a story scene that walks it around the block to (6, 4), east of the wall. The
/// straight line home crosses the wall, so the shortest path goes south around it: 5 steps.
/// </remarks>
public sealed class NpcHomeTests
{
    private const ulong Seed = 20260925;

    /// <summary>The ticks of one step of the porter (D-821).</summary>
    private const int PorterStepTicks = 32;

    /// <summary>The porter, a route NPC of one tile that stands at (3, 4) (D-740).</summary>
    private const string Porter = """{ "id": "npc.hub_porter", "facing": "north", "step_ticks": 32, "move": "route", "tiles": [{ "x": 3, "y": 4, "wait_ticks": 0 }] }""";

    /// <summary>The story scene that walks the porter to the other side of the wall block.</summary>
    private const string AwayFile = """
    {
     "comment": "The porter walks around the wall block to (6, 4).",
     "id": "scene.test_away",
     "steps": [
      { "id": "step.porter_walks", "kind": "move", "actor": "npc.hub_porter", "path": ["south", "east", "east", "east", "north"] }
     ]
    }
    """;

    private static readonly TilePoint Home = new(3, 4);

    private static readonly TilePoint Away = new(6, 4);

    private static readonly Intent StepEnd = Intent.OfPlayer(IntentIds.StoryStepEnd);

    private static readonly StoryContent Story = StoryContent.Load(
        FlagList.Read(Encoding.UTF8.GetBytes(TestBattles.NoFlagsFile), FlagList.Path),
        [TestStory.Scene(AwayFile, "away")],
        TestBattles.Content);

    [Fact]
    public void TheSearchFindsTheShortestPathAroundTheWall()
    {
        // D-1140: the fixed direction order takes the path south of the block, 5 steps, and the
        // path north of it takes 7.
        GameMap map = AwayMap();
        Npc porter = map.Npcs[0];

        Assert.True(NpcPaths.TryFirstStepHome(map, porter, Away, out StepDirection first, out int distance));

        Assert.Equal((StepDirection.South, 5), (first, distance));
    }

    [Fact]
    public void AfterAStorySceneAnNpcOutsideItsRangeWalksHomeOnAShortestPath()
    {
        // Exit test 16 of PR-14 (D-1140): the porter walks home on the first world tick after the
        // story scene, and the ticks to arrive equal the search distance times its step ticks.
        Simulation run = StartAway();
        NpcState porter = run.State.Party.Npcs.All[0];
        Assert.True(NpcPaths.TryFirstStepHome(run.State.Party.Map, porter.Npc, porter.At, out _, out int distance));

        List<TilePoint> path = [];
        long? began = null;
        long arrived = 0;
        for (int tick = 0; porter.WalksHome; tick += 1)
        {
            Assert.True(tick < 1000, "The porter did not arrive home in 1000 ticks.");
            TilePoint before = porter.At;
            run.Step([]);
            began ??= porter.Stepping is not null ? run.Tick : null;
            if (porter.At != before)
            {
                path.Add(porter.At);
                arrived = run.Tick;
            }
        }

        Assert.Equal([new TilePoint(6, 5), new TilePoint(5, 5), new TilePoint(4, 5), new TilePoint(3, 5), Home], path);
        Assert.Equal(distance, path.Count);
        Assert.Equal(distance * PorterStepTicks, arrived - began);
        Assert.True(porter.Stands);
    }

    [Fact]
    public void AReplayOfTheWalkHomeGivesTheSameStateHash()
    {
        // Exit test 16 of PR-14 (G-5): two runs of one seed and one intent list keep one hash, and
        // a run resumed from the text of a snapshot on the way home keeps it too.
        Simulation first = StartAway();
        Simulation second = StartAway();
        for (int tick = 0; tick < 40; tick += 1)
        {
            first.Step([]);
            second.Step([]);
        }

        string line = RunSnapshotText.Write(first.Snapshot());
        var reader = new ContentReader(Encoding.UTF8.GetBytes(line), "the test");
        Simulation resumed = Simulation.Resume(Seed, RunSnapshotText.Read(ref reader), AwayMap(), TestBattles.Content, TestBattles.Notices, Story, DebugIntentHandlers.None);
        Assert.Contains("\"walks_home\":true", line, StringComparison.Ordinal);

        for (int tick = 0; tick < 300; tick += 1)
        {
            first.Step([]);
            second.Step([]);
            resumed.Step([]);
            Assert.True(first.StateHash() == second.StateHash(), $"The replay left the run at tick {first.Tick}.");
            Assert.True(first.StateHash() == resumed.StateHash(), $"The resumed run left the run at tick {first.Tick}.");
        }

        Assert.False(first.State.Party.Npcs.All[0].WalksHome);
    }

    [Fact]
    public void ABlockedStepWaitsOneStepAndThenTheNpcSearchesAgain()
    {
        // D-1140: the lead stands on (3, 5), the tile before home, so the porter waits there one
        // step and searches again. It walks on when the lead leaves.
        Simulation run = StartAway();
        NpcState porter = run.State.Party.Npcs.All[0];
        HubWalks.Walk(run, StepDirection.South, 1);
        HubWalks.Walk(run, StepDirection.East, 1);
        Assert.Equal(new TilePoint(3, 5), run.State.Party.LeadAt);

        bool waited = false;
        for (int tick = 0; tick < 200 && !waited; tick += 1)
        {
            run.Step([]);
            waited = porter.At == new TilePoint(4, 5) && porter.Stepping is null && porter.WaitTicks > 0;
        }

        Assert.True(waited, "The porter never waited before the lead.");
        Assert.Equal((StepDirection.West, PorterStepTicks), (porter.Facing, porter.WaitTicks));

        HubWalks.Walk(run, StepDirection.West, 1);
        for (int tick = 0; porter.WalksHome; tick += 1)
        {
            Assert.True(tick < 400, "The porter did not arrive home after the lead left.");
            run.Step([]);
        }

        Assert.Equal(Home, porter.At);
    }

    [Fact]
    public void AWanderNpcWalksToTheNearestTileOfItsRectanglesAndThenWanders()
    {
        // D-1140: the nearest tile of the rectangle at (6, 5) from (2, 6) is (6, 6), 4 steps east.
        GameMap map = HubMaps.Of(npcs: HubMaps.Wanderer());
        MapState party = MapState.Enter(map);
        NpcState dog = party.Npcs.All[0];
        dog.MoveInScene(new TilePoint(2, 6), StepDirection.West);
        dog.SettleAfterScene();
        Assert.True(dog.WalksHome);

        int ticks = Walk(party, npc => !npc.WalksHome);

        // The first step starts on the first tick, and each of the 4 steps takes the step ticks.
        Assert.Equal(new TilePoint(6, 6), dog.At);
        Assert.Equal(1 + (4 * dog.Npc.StepTicks), ticks);
        Assert.True(dog.Npc.RangeHolds(dog.At));
    }

    [Fact]
    public void ARouteNpcThatReachesItsRouteRejoinsIt()
    {
        // D-1140 and D-739: the barmaid walks from (1, 2) to (3, 2) and on to (3, 5). Moved to
        // (5, 2), she walks back to (3, 2), a route tile, waits there, and takes the next leg.
        GameMap map = HubMaps.Of(npcs: HubMaps.Walker());
        MapState party = MapState.Enter(map);
        NpcState barmaid = party.Npcs.All[0];
        barmaid.MoveInScene(new TilePoint(5, 2), StepDirection.East);
        barmaid.SettleAfterScene();

        Walk(party, npc => !npc.WalksHome);

        Assert.Equal(new TilePoint(3, 2), barmaid.At);
        Assert.Equal((2, true, 0), (barmaid.Target, barmaid.Forward, barmaid.WaitTicks));
        Walk(party, npc => npc.At == new TilePoint(3, 5));
        Assert.Equal((1, false, 60), (barmaid.Target, barmaid.Forward, barmaid.WaitTicks));
    }

    [Fact]
    public void AnNpcThatAStorySceneLeavesInItsHomeWalksOnWithNoWalkHome()
    {
        // D-1140: the dog stays inside its rectangle, so it walks in its normal way at once.
        GameMap map = HubMaps.Of(npcs: HubMaps.Wanderer());
        NpcState dog = MapState.Enter(map).Npcs.All[0];

        dog.MoveInScene(new TilePoint(8, 6), StepDirection.East);
        dog.SettleAfterScene();

        Assert.False(dog.WalksHome);
        Assert.Equal(0, dog.WaitTicks);
    }

    [Fact]
    public void AnNpcWithNoPathHomeFailsWithTheNpcAndTheTile()
    {
        // T-2: two service points close the spawn corner, so no open ground leads out of it.
        GameMap map = HubMaps.Of(
            npcs: HubMaps.Wanderer(),
            things: """{ "id": "service_point.hub_east", "kind": "service_point", "x": 2, "y": 1 }, { "id": "service_point.hub_south", "kind": "service_point", "x": 1, "y": 2 }""",
            services: """{ "id": "service.hub_rest", "kind": "rest", "thing": "service_point.hub_east", "condition": { "always": true } }, { "id": "service.hub_save", "kind": "save", "thing": "service_point.hub_south", "condition": { "always": true } }""");
        MapState party = MapState.Enter(map);
        NpcState dog = party.Npcs.All[0];
        dog.MoveInScene(new TilePoint(1, 1), StepDirection.North);
        dog.SettleAfterScene();

        Assert.False(NpcPaths.TryFirstStepHome(map, dog.Npc, dog.At, out _, out _));
        SimulationException error = Assert.Throws<SimulationException>(() => Walk(party, _ => false));

        Assert.Contains("the NPC 'npc.hub_dog' walks home from (1, 1), and no path of open ground leads to its home", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void StoredValuesOfAWalkHomeResumeOutsideTheHomeAndFailInsideIt()
    {
        // D-1140 and T-2: an NPC on its way home stands outside its home, and the walk ends on arrival.
        Npc porter = AwayMap().Npcs[0];
        var outside = new NpcValues(porter.Id, 6, 4, StepDirection.South, StepDirection.South, 3, 0, true, 0, true);
        var inside = outside with { X = 3, Y = 4, Stepping = null, StepTicks = 0 };

        Assert.Equal(outside, NpcState.Resume(porter, AwayMap(), outside, "the save").Values());
        ArgumentException error = Assert.Throws<ArgumentException>(() => NpcState.Resume(porter, AwayMap(), inside, "the save"));

        Assert.Contains("walks home, and it stands at (3, 4), which its home holds", error.Message, StringComparison.Ordinal);
    }

    /// <summary>The hub of these tests: the porter, with a talk trigger that starts the walk away.</summary>
    private static GameMap AwayMap()
    {
        string trigger = """{ "id": "trigger.test_away", "kind": "talk", "npc": "npc.hub_porter", "scene": "scene.test_away", "condition": { "always": true } }""";
        return TestMaps.Of("hub-test.json", HubMaps.Text(npcs: Porter).Replace("\"triggers\": []", $"\"triggers\": [{trigger}]", StringComparison.Ordinal));
    }

    /// <summary>
    /// Starts a run on the hub of these tests, talks with the porter from (2, 4), and ends the move
    /// step, so the story scene ends with the porter at (6, 4).
    /// </summary>
    private static Simulation StartAway()
    {
        Simulation run = Simulation.Start(Seed, AwayMap(), TestBattles.Content, TestBattles.Notices, Story, DebugIntentHandlers.None);
        HubWalks.Walk(run, StepDirection.South, 3);
        HubWalks.Walk(run, StepDirection.East, 1);
        HubWalks.Face(run, StepDirection.East);
        HubWalks.Confirm(run);
        run.Step([StepEnd]);

        NpcState porter = run.State.Party.Npcs.All[0];
        Assert.False(run.State.Story.Running);
        Assert.Equal((Away, true), (porter.At, porter.WalksHome));
        return run;
    }

    /// <summary>Walks the NPCs alone until the first NPC meets a condition, and gives the count of ticks.</summary>
    private static int Walk(MapState party, Func<NpcState, bool> done)
    {
        RandomStream stream = RandomStreams.Open(Seed, StreamId.Npc);
        List<NpcState> moved = [];
        for (int tick = 1; tick <= 2000; tick += 1)
        {
            party.Npcs.Walk(party.Map, party, stream, new RunContext(Seed, tick, "the test"), moved);
            if (done(party.Npcs.All[0]))
            {
                return tick;
            }
        }

        throw new InvalidOperationException("The NPC did not meet the condition in 2000 ticks.");
    }
}
