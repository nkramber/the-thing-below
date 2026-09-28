using System;
using System.Collections.Generic;
using System.Linq;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Story;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The overworld of region one of PR-110 on the content set of the checkout: its size, its marks,
/// its gates, its zones, and the walk from the village to the mining town (D-1270 to D-1296).
/// </summary>
/// <remarks>
/// Each test reads the tile of each mark and each gate from the map, and finds each walk with a
/// search, so a change of the settings of the generator breaks no test that still holds (D-1294).
/// </remarks>
public sealed class RegionOneOverworldTests
{
    private const ulong Seed = 20260927;

    private static readonly Lazy<ContentSet> Content = new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    private static readonly string[] Places =
    [
        "mark.overworld_village", "mark.overworld_town", "mark.overworld_mine", "mark.overworld_gallery",
        "mark.overworld_refuge", "mark.overworld_fort", "mark.overworld_ice_crossing",
        "mark.overworld_broken_waystone", "mark.overworld_dead_mine_head", "mark.overworld_war_graves", "mark.overworld_bandit_lookout",
    ];

    private static GameMap Overworld => Content.Value.Map(Id("map.overworld"));

    [Fact]
    public void TheOverworldHoldsRegionOneAtItsSize()
    {
        // D-1274, D-1297: one overworld, and region one takes 160 by 128 tiles of it.
        GameMap map = Overworld;

        Assert.Equal(MapKind.Overworld, map.Kind);
        Assert.Equal("region.one", map.Region.Value);
        Assert.Equal("label.overworld", map.Label.Value);
        Assert.Equal(160, map.Width);
        Assert.Equal(128, map.Height);
    }

    [Fact]
    public void EachPlaceOfRegionOneIsAMarkWithNoLink()
    {
        // D-1270, D-1271, D-1299: each place and each side landmark is a mark with no link.
        Assert.Equal(
            Places.Order(StringComparer.Ordinal),
            Overworld.Things.Where(thing => thing.Kind == MapThingKind.Mark).Select(thing => thing.Id.Value).Order(StringComparer.Ordinal));
        Assert.DoesNotContain(Overworld.Things, thing => thing.Kind == MapThingKind.Entrance);
    }

    [Fact]
    public void EachZoneOfRegionOneHoldsRateZeroAndNoGroupUntilItsEnemies()
    {
        // D-1283, D-1284, D-1285: six zones of region one, at rate 0 until PR-17.
        Assert.Equal(
            ["zone.overworld_road", "zone.overworld_low_field", "zone.overworld_low_forest", "zone.overworld_valley_field", "zone.overworld_valley_forest", "zone.overworld_pass"],
            Overworld.Zones.Select(zone => zone.Id.Value));
        foreach (EncounterZone zone in Overworld.Zones)
        {
            Assert.Equal(0, zone.Rate);
            Assert.Empty(zone.Groups);
            Assert.Equal("region.one", zone.Region.Value);
        }
    }

    [Fact]
    public void TheLeadWalksFromTheVillageToTheMiningTownOnceBergitJoins()
    {
        // Exit test 1 of PR-110 (D-1270): the walk on the overworld from the mark of the village
        // to the mark of the town, through the two gates of the low pass.
        Simulation run = Start();
        WalkTo(run, At("mark.overworld_village"));
        run.State.Story.Flags.TurnOn(Id("flag.region_one_bergit_joins"));

        WalkTo(run, At("mark.overworld_town"));

        Assert.Equal("map.overworld", run.State.Party.Map.Id.Value);
        Assert.Equal(At("mark.overworld_town"), run.State.Party.LeadAt);
        Assert.Empty(run.TakeNotices());
    }

    [Fact]
    public void AStepOntoAMarkEntersNothingAndPostsNothing()
    {
        // D-1272: the lead walks over a mark, and nothing happens.
        Simulation run = Start();

        WalkTo(run, At("mark.overworld_village"));

        Assert.Equal("map.overworld", run.State.Party.Map.Id.Value);
        Assert.Equal(At("mark.overworld_village"), run.State.Party.LeadAt);
        Assert.Empty(run.TakeNotices());
        Assert.Empty(run.TakeSaveRequests());
    }

    [Fact]
    public void BeforeBergitJoinsTheRoadToTheTownRefusesTheLeadAndSaysWhy()
    {
        // D-1281, D-1288: the gate of the road to the town, and its notice on a confirm (D-1257).
        Simulation run = Start();
        List<StepDirection> steps = PathOf(run.State.Party.LeadAt, At("gate.overworld_town_road"));
        TilePoint before = WalkSteps(run, steps[..^1]);

        HubWalks.Face(run, steps[^1]);
        HubWalks.Confirm(run);

        Assert.Equal(before, run.State.Party.LeadAt);
        Assert.Equal(["notice.overworld_town_road"], run.TakeNotices().Select(notice => notice.Id.Value));
    }

    [Fact]
    public void AfterTheBreakoutTheWayDownToTheVillageShutsAndSaysWhy()
    {
        // D-1281: the way down holds a not node, so it shuts at the breakout.
        Simulation run = Start();
        run.State.Story.Flags.TurnOn(Id("flag.region_one_bergit_joins"));
        WalkTo(run, At("mark.overworld_town"));
        run.State.Story.Flags.TurnOn(Id("flag.region_one_breakout"));
        List<StepDirection> steps = PathOf(run.State.Party.LeadAt, At("gate.overworld_village_road"));
        TilePoint before = WalkSteps(run, steps[..^1]);

        HubWalks.Face(run, steps[^1]);
        HubWalks.Confirm(run);

        Assert.Equal(before, run.State.Party.LeadAt);
        Assert.Equal(["notice.overworld_village_road"], run.TakeNotices().Select(notice => notice.Id.Value));
    }

    [Theory]
    [InlineData("gate.overworld_town_road", "flag.region_one_bergit_joins", false)]
    [InlineData("gate.overworld_mine_mouth", "flag.region_one_ottild_joins", false)]
    [InlineData("gate.overworld_sealed_door", "flag.region_one_breakout", false)]
    [InlineData("gate.overworld_road_up", "flag.region_one_night_pass", false)]
    [InlineData("gate.overworld_village_road", "flag.region_one_breakout", true)]
    public void EachGateOpensOnTheFlagOfItsStoryStep(string gate, string flag, bool shutsOnTheFlag)
    {
        // D-1281, D-1282.
        MapGate found = Overworld.Things.Single(thing => string.CompareOrdinal(thing.Id.Value, gate) == 0).Gate
            ?? throw new InvalidOperationException($"The thing '{gate}' holds no gate.");
        FlagSet flags = FlagSet.Empty();
        Assert.Equal(shutsOnTheFlag, found.Condition.Holds(flags));

        flags.TurnOn(Id(flag));

        Assert.Equal(!shutsOnTheFlag, found.Condition.Holds(flags));
    }

    [Fact]
    public void EachGateIsTheOneWayThroughItsWall()
    {
        // D-1276, D-1281: with each gate shut, the land past it lies out of reach.
        GameMap map = Overworld;
        HashSet<TilePoint> gates = [.. map.Things.Where(thing => thing.Kind == MapThingKind.Gate).Select(thing => thing.At)];

        HashSet<TilePoint> shut = Reach(map, map.Spawn, gates);
        HashSet<TilePoint> open = Reach(map, map.Spawn, []);

        Assert.Contains(At("mark.overworld_village"), shut);
        foreach (string place in new[] { "mark.overworld_town", "mark.overworld_mine", "mark.overworld_gallery", "mark.overworld_fort", "mark.overworld_ice_crossing", "mark.overworld_bandit_lookout" })
        {
            Assert.DoesNotContain(At(place), shut);
            Assert.Contains(At(place), open);
        }
    }

    [Fact]
    public void EachLandmarkStandsOffTheRoadWhereTheLeadCanWalk()
    {
        // D-1299, D-1301: a landmark rewards a trip off the road, so no road tile lies within
        // three tiles of it, and the lead reaches it with each gate open.
        GameMap map = Overworld;
        HashSet<TilePoint> open = Reach(map, map.Spawn, []);
        foreach (string landmark in new[] { "mark.overworld_broken_waystone", "mark.overworld_dead_mine_head", "mark.overworld_war_graves", "mark.overworld_bandit_lookout" })
        {
            TilePoint at = At(landmark);
            Assert.Contains(at, open);
            for (int dy = -3; dy <= 3; dy += 1)
            {
                for (int dx = -3; dx <= 3; dx += 1)
                {
                    var near = new TilePoint(at.X + dx, at.Y + dy);
                    bool inside = near.X >= 0 && near.Y >= 0 && near.X < map.Width && near.Y < map.Height;
                    Assert.False(inside && map.TileAt(near) == TileKind.Road, $"The road at {near} lies within three tiles of '{landmark}'.");
                }
            }
        }
    }

    [Fact]
    public void TheRoadCrossesWaterOnABridge()
    {
        // D-1302: the map holds a bridge, and each bridge stands in the water with the road on it.
        GameMap map = Overworld;
        List<TilePoint> bridges = [];
        for (int y = 0; y < map.Height; y += 1)
        {
            for (int x = 0; x < map.Width; x += 1)
            {
                if (map.TileAt(new TilePoint(x, y)) == TileKind.Bridge)
                {
                    bridges.Add(new TilePoint(x, y));
                }
            }
        }

        Assert.NotEmpty(bridges);
        Assert.Contains(bridges, bridge => Neighbors(bridge).Any(next => map.TileAt(next.Next) == TileKind.Water));
    }

    [Fact]
    public void TheTownGateComesFirstFromTheSouthAndTheVillageGateFirstFromTheNorth()
    {
        // D-1281: each gate of the low pass posts the notice that fits the side of the lead.
        GameMap map = Overworld;

        HashSet<TilePoint> south = Reach(map, map.Spawn, [At("gate.overworld_town_road")]);
        HashSet<TilePoint> north = Reach(map, At("mark.overworld_town"), [At("gate.overworld_village_road")]);

        Assert.DoesNotContain(At("gate.overworld_village_road"), south);
        Assert.DoesNotContain(At("gate.overworld_town_road"), north);
    }

    [Fact]
    public void NoWalkableTileOfTheOverworldReachesTheRefuge()
    {
        // D-1280: the gallery alone leads to the refuge.
        HashSet<TilePoint> open = Reach(Overworld, Overworld.Spawn, []);

        Assert.DoesNotContain(At("mark.overworld_refuge"), open);
    }

    [Fact]
    public void TheTableGivesTheOverworldToRegionOne()
    {
        // D-1289: the region of the map file and the region of the table agree.
        Assert.Equal("region.one", Content.Value.Effects.Transitions.RegionOf(Id("map.overworld")).Value);
    }

    private static Simulation Start() =>
        Simulation.Start(Seed, MapSet.Of(Content.Value.Maps), Id("map.overworld"), Content.Value.Battle, Content.Value.Notices, Content.Value.Story, DebugIntentHandlers.None);

    private static ContentId Id(string value) => ContentId.Parse(value, "test", "id");

    private static TilePoint At(string id) => Overworld.Things.Single(thing => string.CompareOrdinal(thing.Id.Value, id) == 0).At;

    /// <summary>Walks the lead to a tile on the shortest path, and fails when a step starts no walk.</summary>
    private static void WalkTo(Simulation run, TilePoint to)
    {
        TilePoint end = WalkSteps(run, PathOf(run.State.Party.LeadAt, to));
        Assert.Equal(to, end);
    }

    private static TilePoint WalkSteps(Simulation run, List<StepDirection> steps)
    {
        foreach (StepDirection step in steps)
        {
            HubWalks.Walk(run, step, 1);
        }

        return run.State.Party.LeadAt;
    }

    /// <summary>Gives the steps of a shortest path over the walkable tiles, with each gate open.</summary>
    private static List<StepDirection> PathOf(TilePoint from, TilePoint to)
    {
        GameMap map = Overworld;
        var came = new Dictionary<TilePoint, (TilePoint From, StepDirection Step)>();
        var todo = new Queue<TilePoint>([from]);
        var seen = new HashSet<TilePoint> { from };
        while (todo.Count > 0 && !seen.Contains(to))
        {
            TilePoint at = todo.Dequeue();
            foreach ((StepDirection step, TilePoint next) in Neighbors(at))
            {
                if (Walkable(map, next) && seen.Add(next))
                {
                    came[next] = (at, step);
                    todo.Enqueue(next);
                }
            }
        }

        Assert.True(seen.Contains(to), $"No walkable path joins {from} and {to}.");
        List<StepDirection> steps = [];
        for (TilePoint at = to; at != from; at = came[at].From)
        {
            steps.Add(came[at].Step);
        }

        steps.Reverse();
        return steps;
    }

    /// <summary>Gives each tile that the lead reaches from a start with four-way steps, with some tiles shut.</summary>
    private static HashSet<TilePoint> Reach(GameMap map, TilePoint start, HashSet<TilePoint> shut)
    {
        HashSet<TilePoint> seen = [start];
        var todo = new Queue<TilePoint>([start]);
        while (todo.Count > 0)
        {
            foreach ((_, TilePoint next) in Neighbors(todo.Dequeue()))
            {
                if (Walkable(map, next) && !shut.Contains(next) && seen.Add(next))
                {
                    todo.Enqueue(next);
                }
            }
        }

        return seen;
    }

    private static (StepDirection Step, TilePoint Next)[] Neighbors(TilePoint at) =>
    [
        (StepDirection.North, new TilePoint(at.X, at.Y - 1)),
        (StepDirection.South, new TilePoint(at.X, at.Y + 1)),
        (StepDirection.East, new TilePoint(at.X + 1, at.Y)),
        (StepDirection.West, new TilePoint(at.X - 1, at.Y)),
    ];

    private static bool Walkable(GameMap map, TilePoint at) =>
        at.X >= 0 && at.Y >= 0 && at.X < map.Width && at.Y < map.Height && TileKinds.CanWalk(map.TileAt(at));
}
