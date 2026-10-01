using System;
using System.Collections.Generic;
using System.Linq;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Notices;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Story;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The overworld of region one of PR-110 on the content set of the checkout: its size, its marks,
/// its entrances, its gates, its zones, the walk from the village to the mining town, and the
/// treasure of PR-111 (D-1270 to D-1296, D-1304 to D-1308). PR-17 makes the village and the town
/// entrances, adds the entrance of the pasture, and gives the low and the valley zones their
/// beasts (D-1331, D-1332).
/// </summary>
/// <remarks>
/// Each test reads the tile of each mark and each gate from the map, and finds each walk with a
/// search, so a change of the settings of the generator breaks no test that still holds (D-1294).
/// A walk of a test runs on a copy of the overworld with each zone at rate 0, so no fight of a
/// zone stops it.
/// </remarks>
public sealed class RegionOneOverworldTests
{
    private const ulong Seed = 20260927;

    private static readonly Lazy<ContentSet> Content = new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    private static readonly Lazy<MapSet> CalmMaps = new(() => MapSet.Of(Content.Value.Maps.Select(map => string.CompareOrdinal(map.Id.Value, "map.overworld") == 0 ? CalmOf(map) : map)));

    private static readonly string[] Places =
    [
        "mark.overworld_mine", "mark.overworld_gallery",
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

    [Theory]
    [InlineData("entrance.overworld_village", "map.village", "marker.overworld_village")]
    [InlineData("entrance.overworld_pasture", "map.village_pasture", "marker.overworld_pasture")]
    [InlineData("entrance.overworld_town", "map.mining_town", "marker.overworld_town")]
    public void EachPlaceOfTheFirstPlayableIsAnEntranceWithItsArrivalBesideIt(string entrance, string place, string marker)
    {
        // D-1243, D-1331: the PR of a place makes its mark an entrance, and the exit of the place
        // puts the party on the marker next to it (D-1255).
        MapThing found = Overworld.Things.Single(thing => string.CompareOrdinal(thing.Id.Value, entrance) == 0);

        Assert.Equal(MapThingKind.Entrance, found.Kind);
        Assert.Equal(place, found.To?.Value);
        Assert.Equal(1, Math.Abs(found.At.X - At(marker).X) + Math.Abs(found.At.Y - At(marker).Y));
        Assert.Contains(Content.Value.Map(Id(place)).Things, thing => thing.Kind == MapThingKind.Exit && string.CompareOrdinal(thing.Arrive?.Value, marker) == 0);
    }

    [Theory]
    [InlineData("entrance.overworld_village", "map.village", "marker.village_road_in", "exit.village_road")]
    [InlineData("entrance.overworld_pasture", "map.village_pasture", "marker.pasture_way_in", "exit.pasture_way_out")]
    [InlineData("entrance.overworld_town", "map.mining_town", "marker.town_gate_in", "exit.town_gate")]
    public void AnEntranceArrivesBesideTheExitOfItsPlace(string entrance, string place, string marker, string exit)
    {
        // The playtest: a return from the overworld put the party in the house of Marrek, the
        // spawn point where the run starts. An entrance names the marker of its place, and each
        // place of the first playable puts it next to its exit to the overworld (D-1367).
        Simulation run = Start();
        run.State.Story.Flags.TurnOn(Id("flag.region_one_bergit_joins"));
        _ = WalkSteps(run, PathOf(run.State.Party.LeadAt, At(entrance)));

        GameMap map = Content.Value.Map(Id(place));
        TilePoint arrived = map.ThingOf(Id(marker), MapThingKind.Marker)!.At;
        TilePoint way = map.ThingOf(Id(exit), MapThingKind.Exit)!.At;
        Assert.Equal(place, run.State.Party.Map.Id.Value);
        Assert.Equal(arrived, run.State.Party.LeadAt);
        Assert.NotEqual(map.Spawn, run.State.Party.LeadAt);
        Assert.Equal(1, Math.Abs(arrived.X - way.X) + Math.Abs(arrived.Y - way.Y));
    }

    [Fact]
    public void EachOtherPlaceOfRegionOneIsAMarkWithNoLink()
    {
        // D-1270, D-1271, D-1299: each place with no map yet and each side landmark is a mark
        // with no link, and the three places of the first playable are entrances (D-1331).
        Assert.Equal(
            Places.Order(StringComparer.Ordinal),
            Overworld.Things.Where(thing => thing.Kind == MapThingKind.Mark).Select(thing => thing.Id.Value).Order(StringComparer.Ordinal));
        Assert.Equal(
            ["entrance.overworld_pasture", "entrance.overworld_town", "entrance.overworld_village"],
            Overworld.Things.Where(thing => thing.Kind == MapThingKind.Entrance).Select(thing => thing.Id.Value).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void TheLowAndTheValleyZonesFightTheBeastsAndTheRoadAndThePassFightNothing()
    {
        // D-1283 to D-1285, D-1332: the low field and the low forest hold the wolves and the crows
        // at the rates of the fixture overworld, the valley adds the boar, and the road and the
        // pass keep the rate 0.
        Assert.Equal(
            ["zone.overworld_road", "zone.overworld_low_field", "zone.overworld_low_forest", "zone.overworld_valley_field", "zone.overworld_valley_forest", "zone.overworld_pass"],
            Overworld.Zones.Select(zone => zone.Id.Value));
        Assert.Equal([0, 100, 200, 100, 200, 0], Overworld.Zones.Select(zone => zone.Rate));
        string[] low = ["group.one_wolves 3", "group.one_wolf_crows 2", "group.one_crows 2"];
        string[] valley = [.. low, "group.one_boar 1", "group.one_boar_wolves 1"];
        Assert.Empty(Overworld.Zones[0].Groups);
        Assert.Equal(low, GroupsOf(Overworld.Zones[1]));
        Assert.Equal(low, GroupsOf(Overworld.Zones[2]));
        Assert.Equal(valley, GroupsOf(Overworld.Zones[3]));
        Assert.Equal(valley, GroupsOf(Overworld.Zones[4]));
        Assert.Empty(Overworld.Zones[5].Groups);
        foreach (EncounterZone zone in Overworld.Zones)
        {
            Assert.Equal("region.one", zone.Region.Value);
        }
    }

    [Fact]
    public void TheLeadWalksFromTheVillageToTheMiningTownOnceBergitJoins()
    {
        // Exit test 1 of PR-110 (D-1270): the walk on the overworld from the village to the town,
        // through the two gates of the low pass. The entrance of the town enters the town on the
        // marker inside its gate, and no more on the spawn point at the chapel (D-1331, D-1367).
        Simulation run = Start();
        WalkTo(run, At("marker.overworld_village"));
        run.State.Story.Flags.TurnOn(Id("flag.region_one_bergit_joins"));

        _ = WalkSteps(run, PathOf(run.State.Party.LeadAt, At("entrance.overworld_town")));

        GameMap town = Content.Value.Map(Id("map.mining_town"));
        Assert.Equal("map.mining_town", run.State.Party.Map.Id.Value);
        Assert.Equal(town.ThingOf(Id("marker.town_gate_in"), MapThingKind.Marker)!.At, run.State.Party.LeadAt);
        Assert.Empty(run.TakeNotices());
    }

    [Fact]
    public void AStepOntoAMarkEntersNothingAndPostsNothing()
    {
        // D-1272: the lead walks over a mark, and nothing happens.
        Simulation run = Start();

        WalkTo(run, At("mark.overworld_dead_mine_head"));

        Assert.Equal("map.overworld", run.State.Party.Map.Id.Value);
        Assert.Equal(At("mark.overworld_dead_mine_head"), run.State.Party.LeadAt);
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
        WalkTo(run, At("marker.overworld_town"));
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

        Assert.Contains(At("entrance.overworld_village"), shut);
        Assert.Contains(At("entrance.overworld_pasture"), shut);
        foreach (string place in new[] { "entrance.overworld_town", "mark.overworld_mine", "mark.overworld_gallery", "mark.overworld_fort", "mark.overworld_ice_crossing", "mark.overworld_bandit_lookout" })
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
    public void FourTreasuresLieOneInTheLowLandTwoInTheValleyAndOneInThePass()
    {
        // D-1307, D-1308: four cairns, and the valley, which stays open the longest, holds two.
        // The tiles within one tile of a thing take the road zone, so the land comes from the ring
        // two tiles out, where every zone of the ring names one land.
        List<string> lands = [];
        foreach (MapThing chest in Overworld.Things.Where(thing => thing.Kind == MapThingKind.Chest))
        {
            List<string> near = [];
            for (int dy = -2; dy <= 2; dy += 1)
            {
                for (int dx = -2; dx <= 2; dx += 1)
                {
                    string? zone = Math.Max(Math.Abs(dx), Math.Abs(dy)) == 2 ? Overworld.ZoneAt(new TilePoint(chest.At.X + dx, chest.At.Y + dy))?.Id.Value : null;
                    near.AddRange(LandOf(zone));
                }
            }

            string land = Assert.Single(near.Distinct());
            lands.Add(land);
        }

        Assert.Equal(["low", "pass", "valley", "valley"], lands.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void EachTreasureStandsOffTheRoadBeforeEachStoryGateAndCutsNoPath()
    {
        // D-1301, D-1305, D-1306: the land hides a treasure at the end of a side route. The lead
        // reaches it with the mine mouth and the sealed door shut, and the solid cairn closes no
        // tile that the lead reaches past it.
        GameMap map = Overworld;
        HashSet<TilePoint> storyGates = [At("gate.overworld_mine_mouth"), At("gate.overworld_sealed_door")];
        HashSet<TilePoint> open = Reach(map, map.Spawn, storyGates);
        MapThing[] chests = [.. map.Things.Where(thing => thing.Kind == MapThingKind.Chest)];
        Assert.Equal(4, chests.Length);
        foreach (MapThing chest in chests)
        {
            Assert.Contains(chest.At, open);
            for (int dy = -3; dy <= 3; dy += 1)
            {
                for (int dx = -3; dx <= 3; dx += 1)
                {
                    var near = new TilePoint(chest.At.X + dx, chest.At.Y + dy);
                    bool inside = near.X >= 0 && near.Y >= 0 && near.X < map.Width && near.Y < map.Height;
                    Assert.False(inside && map.TileAt(near) == TileKind.Road, $"The road at {near} lies within three tiles of '{chest.Id.Value}'.");
                }
            }
        }

        HashSet<TilePoint> pastCairns = Reach(map, map.Spawn, [.. storyGates, .. chests.Select(chest => chest.At)]);
        open.ExceptWith(chests.Select(chest => chest.At));
        Assert.Equal(open.Count, pastCairns.Count);
        Assert.Subset(open, pastCairns);
    }

    [Fact]
    public void ThePartyTakesATreasureOneTimeAndTheSaveKeepsItTaken()
    {
        // Exit test 1 of PR-111 (D-1298, D-1304): the first confirm gives the chest form of D-1220
        // with the notices of D-1224, and the memory of the place reaches the save and the state hash.
        Simulation run = Start();
        ContentId cairn = Id("chest.overworld_cairn_west_field");
        (TilePoint side, StepDirection face) = OpenSideOf(At(cairn.Value));
        WalkTo(run, side);
        HubWalks.Face(run, face);
        _ = run.TakeNotices();

        HubWalks.Confirm(run);
        Assert.Equal(["notice.chest_gold 12", "notice.chest_found item.poultice", "notice.chest_found item.poultice"], Posted(run));
        HubWalks.Confirm(run);
        Assert.Equal([ChestRules.EmptyNotice.Value], Posted(run));

        string line = RunSnapshotText.Write(run.Snapshot());
        var reader = new ContentReader(System.Text.Encoding.UTF8.GetBytes(line), "the test");
        Simulation resumed = Simulation.Resume(Seed, RunSnapshotText.Read(ref reader), CalmMaps.Value, Content.Value.Battle, Content.Value.Notices, Content.Value.Story, DebugIntentHandlers.None);

        Assert.Contains("\"chests\":[{\"chest\":\"chest.overworld_cairn_west_field\",\"left\":[]}]", line, StringComparison.Ordinal);
        Assert.Equal(run.StateHash(), resumed.StateHash());
        Assert.Empty(resumed.State.Party.Place.LeftIn(cairn)!);
        HubWalks.Confirm(resumed);
        Assert.Equal([ChestRules.EmptyNotice.Value], Posted(resumed));
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
        HashSet<TilePoint> north = Reach(map, At("entrance.overworld_town"), [At("gate.overworld_village_road")]);

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
        Simulation.Start(Seed, CalmMaps.Value, Id("map.overworld"), Content.Value.Battle, Content.Value.Notices, Content.Value.Story, DebugIntentHandlers.None);

    /// <summary>Gives a copy of an overworld of the checkout with each zone at rate 0, so a walk meets no fight (D-1263).</summary>
    private static GameMap CalmOf(GameMap map)
    {
        string text = System.IO.File.ReadAllText(RepositoryRoot.PathTo($"content/{map.File}"));
        return TestMaps.Of(map.File, System.Text.RegularExpressions.Regex.Replace(text, "\"rate\": [0-9]+, \"groups\": \\[[^\\]]*\\]", "\"rate\": 0, \"groups\": []"));
    }

    private static string[] GroupsOf(EncounterZone zone) =>
        [.. zone.Groups.Select(group => $"{group.Group.Value} {group.Weight.ToString(System.Globalization.CultureInfo.InvariantCulture)}")];

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

    /// <summary>Gives the land of a zone of region one, or no land for no zone and for the road zone (D-1283).</summary>
    private static string[] LandOf(string? zone) => zone switch
    {
        null or "zone.overworld_road" => [],
        "zone.overworld_low_field" or "zone.overworld_low_forest" => ["low"],
        "zone.overworld_valley_field" or "zone.overworld_valley_forest" => ["valley"],
        "zone.overworld_pass" => ["pass"],
        _ => throw new InvalidOperationException($"The zone '{zone}' is in no land of region one."),
    };

    /// <summary>Gives the first side of a solid thing, in the order of the neighbors, where the lead stands, and the direction to the thing.</summary>
    private static (TilePoint Side, StepDirection Face) OpenSideOf(TilePoint thing)
    {
        foreach ((StepDirection step, TilePoint next) in Neighbors(thing))
        {
            if (Walkable(Overworld, next) && !Solid(Overworld, next))
            {
                return (next, StepDirections.Opposite(step));
            }
        }

        throw new InvalidOperationException($"No side of the thing at {thing} is open.");
    }

    /// <summary>Gives each posted notice as its id, then its thing or its count.</summary>
    private static List<string> Posted(Simulation run)
    {
        List<string> lines = [];
        foreach (PostedNotice notice in run.TakeNotices())
        {
            string value = notice.Thing?.Value ?? notice.Count?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            lines.Add(value.Length == 0 ? notice.Id.Value : $"{notice.Id.Value} {value}");
        }

        return lines;
    }

    /// <summary>Gives the steps of a shortest path over the walkable tiles, with each gate open and around each solid thing.</summary>
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
                // A step onto an entrance leaves the overworld, so a path crosses none but its end.
                bool leaves = next != to && map.EntranceAt(next) is not null;
                if (Walkable(map, next) && !Solid(map, next) && !leaves && seen.Add(next))
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

    private static bool Solid(GameMap map, TilePoint at) => map.ThingsAt(at).Any(thing => MapThingKinds.IsSolid(thing.Kind));

    private static bool Walkable(GameMap map, TilePoint at) =>
        at.X >= 0 && at.Y >= 0 && at.X < map.Width && at.Y < map.Height && TileKinds.CanWalk(map.TileAt(at));
}
