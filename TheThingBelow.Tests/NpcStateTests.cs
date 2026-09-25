using System;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The values of one NPC on its map: the entry, the step, the wait, the route leg, and the check
/// of stored values (D-739, D-1138, D-1139).
/// </summary>
public sealed class NpcStateTests
{
    [Fact]
    public void ARouteNpcEntersOnItsFirstTileWithTheWaitOfThatTile()
    {
        NpcState barmaid = NpcState.Enter(Only(HubMaps.Walker()));

        Assert.Equal((new TilePoint(1, 2), StepDirection.East, 1, true, 30), (barmaid.At, barmaid.Facing, barmaid.Target, barmaid.Forward, barmaid.WaitTicks));
        Assert.Null(barmaid.Stepping);
        Assert.False(barmaid.Stands);
    }

    [Fact]
    public void AWanderNpcEntersOnItsStartTileWithNoWait()
    {
        NpcState dog = NpcState.Enter(Only(HubMaps.Wanderer()));

        Assert.Equal((new TilePoint(6, 5), StepDirection.South, 0, true, 0), (dog.At, dog.Facing, dog.Target, dog.Forward, dog.WaitTicks));
    }

    [Fact]
    public void ARouteNpcWaitsOnArrivalAtEachRouteTileAndWalksBackDownTheList()
    {
        // D-739, D-1138: the wait of a tile starts when the step onto it ends, and the NPC then
        // walks back down the list from its last tile.
        NpcState porter = NpcState.Enter(Only(Route("""{ "x": 1, "y": 2, "wait_ticks": 5 }, { "x": 3, "y": 2, "wait_ticks": 7 }""")));

        Assert.Equal(StepDirection.East, porter.NextOnRoute());
        Assert.True(Step(porter, StepDirection.East));
        Assert.Equal((new TilePoint(2, 2), 5), (porter.At, porter.WaitTicks));

        Assert.True(Step(porter, StepDirection.East));
        Assert.Equal((new TilePoint(3, 2), 7, 0, false), (porter.At, porter.WaitTicks, porter.Target, porter.Forward));
        Assert.Equal(StepDirection.West, porter.NextOnRoute());
    }

    [Fact]
    public void TheWaitCountsDownToZeroAndStops()
    {
        NpcState barmaid = NpcState.Enter(Only(HubMaps.Walker()));

        for (int tick = 0; tick < 40; tick += 1)
        {
            barmaid.CountWait();
        }

        Assert.Equal(0, barmaid.WaitTicks);
    }

    [Fact]
    public void AStepHoldsTheTileOfItsEndAndAnEndedStepLeavesTheNpcOnItsTile()
    {
        NpcState dog = NpcState.Enter(Only(HubMaps.Wanderer()));
        dog.Begin(StepDirection.East);
        dog.CountStep();

        Assert.True(dog.Holds(new TilePoint(6, 5)));
        Assert.True(dog.Holds(new TilePoint(7, 5)));
        Assert.False(dog.Holds(new TilePoint(5, 5)));

        dog.EndStep();

        Assert.Equal((new TilePoint(6, 5), 0), (dog.At, dog.StepTicks));
        Assert.Null(dog.StepEnd);
        Assert.False(dog.Holds(new TilePoint(7, 5)));
    }

    [Fact]
    public void AMoveReadsNoRuleOfAnotherMove()
    {
        NpcState dog = NpcState.Enter(Only(HubMaps.Wanderer()));
        NpcState barmaid = NpcState.Enter(Only(HubMaps.Walker()));

        InvalidOperationException leg = Assert.Throws<InvalidOperationException>(() => dog.NextOnRoute());
        InvalidOperationException pace = Assert.Throws<InvalidOperationException>(barmaid.WaitPace);

        Assert.Contains("npc.hub_dog", leg.Message, StringComparison.Ordinal);
        Assert.Contains("npc.hub_barmaid", pace.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStandingNpcHasNoRouteStep()
    {
        NpcState keeper = NpcState.Enter(Only(HubMaps.Keeper));

        Assert.True(keeper.Stands);
        Assert.Null(keeper.NextOnRoute());
    }

    [Theory]
    [InlineData("dog", 5, 5, null, 0, 0, 0, "no rectangle of its range holds that tile")]
    [InlineData("dog", 4, 3, null, 0, 0, 0, "which takes no step or holds a thing")]
    [InlineData("dog", 6, 5, null, 0, 0, 65, "the range of its wait is 0 to 64")]
    [InlineData("dog", 6, 5, null, 3, 0, 0, "stands on no step, and its step ticks are 3")]
    [InlineData("dog", 6, 5, "east", 32, 0, 0, "the range of its step is 0 to 31")]
    [InlineData("dog", 8, 5, "east", 0, 0, 0, "steps east to (9, 5), which it never walks onto")]
    [InlineData("dog", 6, 5, null, 0, 1, 0, "walks a range, and it holds the route leg 1")]
    [InlineData("barmaid", 3, 2, null, 0, 1, 0, "stands on the route tile 1, and a walk takes the next leg on arrival")]
    [InlineData("barmaid", 2, 2, null, 0, 3, 0, "walks toward the route tile 3, and its route holds 3 tiles")]
    [InlineData("barmaid", 4, 3, null, 0, 1, 0, "which takes no step or holds a thing")]
    [InlineData("barmaid", 1, 2, null, 0, 1, 61, "the range of its wait is 0 to 60")]
    public void StoredValuesThatNoRunMakesFailWithTheNpcAndTheReason(string who, int x, int y, string? stepping, int stepTicks, int target, int wait, string reason)
    {
        Npc npc = Only(who == "dog" ? HubMaps.Wanderer() : HubMaps.Walker());
        StepDirection? direction = stepping is null ? null : StepDirection.East;
        var values = new NpcValues(npc.Id, x, y, npc.Facing, direction, stepTicks, target, true, wait);

        ArgumentException error = Assert.Throws<ArgumentException>(() => NpcState.Resume(npc, HubMaps.Yard, values, "the save"));

        Assert.Contains($"npc.hub_{who}", error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
        Assert.Contains("The NPC state of the save", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStoredValueThatARunMakesResumesAsItWas()
    {
        Npc npc = Only(HubMaps.Walker());
        var values = new NpcValues(npc.Id, 3, 3, StepDirection.South, StepDirection.South, 9, 2, true, 0);

        NpcState barmaid = NpcState.Resume(npc, HubMaps.Yard, values, "the save");

        Assert.Equal(values.ToString(), barmaid.Values().ToString());
    }

    [Fact]
    public void AnNpcStandsOnNoThing()
    {
        // D-1139: a marker is no solid thing, and still no NPC stands on its tile.
        GameMap map = HubMaps.Of(things: HubMaps.Marker);

        Assert.True(MapRules.CanEnter(map, new TilePoint(1, 6)));
        Assert.False(NpcState.IsOpen(map, new TilePoint(1, 6)));
        Assert.False(NpcState.IsOpen(map, map.Spawn));
        Assert.True(NpcState.IsOpen(map, new TilePoint(2, 6)));
    }

    private static Npc Only(string npc) => Assert.Single(HubMaps.Of(npcs: npc).Npcs);

    private static string Route(string tiles) => HubMaps.Walker(id: "npc.hub_porter", tiles: tiles);

    /// <summary>Runs one whole step of 16 ticks, the step of the route NPCs of the tests (D-821).</summary>
    private static bool Step(NpcState npc, StepDirection direction)
    {
        npc.Begin(direction);
        bool arrived = false;
        for (int tick = 0; tick < npc.Npc.StepTicks; tick += 1)
        {
            arrived = npc.CountStep();
        }

        return arrived;
    }
}
