using System;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The reader of the NPC records of a map file (D-1137, D-1138). Each move reads its own fields
/// alone, and each fault fails the load with the file, the NPC, and the reason (T-2).
/// </summary>
public sealed class NpcTests
{
    [Fact]
    public void AWanderNpcReadsItsStartItsRectanglesAndItsPace()
    {
        GameMap map = HubMaps.Of(npcs: HubMaps.Wanderer(areas: """{ "x": 6, "y": 5, "width": 3, "height": 2 }, { "x": 1, "y": 5, "width": 2, "height": 1 }"""));

        Npc npc = Assert.Single(map.Npcs);
        Assert.Equal("npc.hub_dog", npc.Id.Value);
        Assert.Equal(NpcMove.Wander, npc.Move);
        Assert.Equal(StepDirection.South, npc.Facing);
        Assert.Equal(32, npc.StepTicks);
        Assert.Equal(new TilePoint(6, 5), npc.Start);
        Assert.Equal([new TileArea(6, 5, 3, 2), new TileArea(1, 5, 2, 1)], npc.Areas);
        Assert.Equal(64, npc.PaceTicks);
        Assert.Empty(npc.Route);
        Assert.Null(npc.Target);
    }

    [Fact]
    public void ARouteNpcStartsOnItsFirstTileAndWaitsAtEachTile()
    {
        GameMap map = HubMaps.Of(npcs: HubMaps.Walker());

        Npc npc = Assert.Single(map.Npcs);
        Assert.Equal(NpcMove.Route, npc.Move);
        Assert.Equal(new TilePoint(1, 2), npc.Start);
        Assert.Equal(
            [new RouteStop(new TilePoint(1, 2), 30), new RouteStop(new TilePoint(3, 2), 0), new RouteStop(new TilePoint(3, 5), 60)],
            npc.Route);
        Assert.Empty(npc.Areas);
        Assert.Null(npc.PaceTicks);
        Assert.Null(npc.Target);
    }

    [Fact]
    public void ARouteOfOneTileStandsStill()
    {
        // D-740: one record serves a moving NPC and a standing one.
        Npc npc = Assert.Single(HubMaps.Of(npcs: HubMaps.Keeper).Npcs);

        Assert.Equal(new TilePoint(2, 6), npc.Start);
        Assert.Single(npc.Route);
    }

    [Fact]
    public void AChaserNamesItsTarget()
    {
        GameMap map = HubMaps.Of(npcs: $"{HubMaps.Wanderer()}, {HubMaps.Chaser()}");

        Npc chaser = map.Npcs[1];
        Assert.Equal(NpcMove.Chase, chaser.Move);
        Assert.Equal("npc.hub_dog", chaser.Target?.Value);
        Assert.Equal(new TilePoint(8, 6), chaser.Start);
    }

    [Fact]
    public void TheRangeOfAnNpcHoldsTheTilesOfItsRectanglesAlone()
    {
        Npc npc = Assert.Single(HubMaps.Of(npcs: HubMaps.Wanderer()).Npcs);

        Assert.True(npc.RangeHolds(new TilePoint(8, 6)));
        Assert.False(npc.RangeHolds(new TilePoint(5, 5)));
        Assert.False(npc.RangeHolds(new TilePoint(6, 7)));
    }

    [Theory]
    [InlineData("""{ "id": "npc.a", "facing": "south", "step_ticks": 32, "move": "fly", "tiles": [{ "x": 2, "y": 6, "wait_ticks": 0 }] }""", "one of wander, route, chase")]
    [InlineData("""{ "id": "npc.a", "facing": "up", "step_ticks": 32, "move": "route", "tiles": [{ "x": 2, "y": 6, "wait_ticks": 0 }] }""", "a facing is one of north, south, east, west")]
    [InlineData("""{ "id": "npc.a", "facing": "south", "step_ticks": 20, "move": "route", "tiles": [{ "x": 2, "y": 6, "wait_ticks": 0 }] }""", "steps in 20 ticks")]
    [InlineData("""{ "id": "npc.a", "facing": "south", "step_ticks": 32, "move": "route", "tiles": [] }""", "holds no route tile")]
    [InlineData("""{ "id": "npc.a", "facing": "south", "step_ticks": 32, "move": "route", "tiles": [{ "x": 2, "y": 6, "wait_ticks": -1 }] }""", "waits -1 ticks")]
    [InlineData("""{ "id": "npc.a", "facing": "south", "step_ticks": 32, "move": "route", "tiles": [{ "x": 2, "y": 6 }] }""", "wait_ticks)")]
    [InlineData("""{ "id": "npc.a", "facing": "south", "step_ticks": 32, "move": "route", "pace_ticks": 64, "tiles": [{ "x": 2, "y": 6, "wait_ticks": 0 }] }""", "reads no field 'pace_ticks'")]
    [InlineData("""{ "id": "npc.a", "facing": "south", "step_ticks": 32, "move": "route", "x": 2, "tiles": [{ "x": 2, "y": 6, "wait_ticks": 0 }] }""", "reads no field 'x'")]
    [InlineData("""{ "id": "npc.a", "facing": "south", "step_ticks": 32, "move": "wander", "x": 6, "y": 5, "areas": [{ "x": 6, "y": 5, "width": 3, "height": 2 }], "pace_ticks": 64, "tiles": [] }""", "reads no field 'tiles'")]
    [InlineData("""{ "id": "npc.a", "facing": "south", "step_ticks": 32, "move": "wander", "x": 6, "y": 5, "areas": [{ "x": 6, "y": 5, "width": 3, "height": 2 }], "pace_ticks": 64, "target": "npc.b" }""", "reads no field 'target'")]
    [InlineData("""{ "id": "npc.a", "facing": "south", "step_ticks": 32, "move": "wander", "x": 6, "y": 5, "pace_ticks": 64 }""", "areas)")]
    [InlineData("""{ "id": "npc.a", "facing": "south", "step_ticks": 32, "move": "wander", "x": 6, "y": 5, "areas": [{ "x": 6, "y": 5, "width": 3, "height": 2 }] }""", "pace_ticks)")]
    [InlineData("""{ "id": "npc.a", "facing": "south", "step_ticks": 32, "move": "wander", "y": 5, "areas": [{ "x": 6, "y": 5, "width": 3, "height": 2 }], "pace_ticks": 64 }""", "x)")]
    [InlineData("""{ "id": "npc.a", "facing": "south", "step_ticks": 32, "move": "wander", "x": 6, "y": 5, "areas": [], "pace_ticks": 64 }""", "holds no rectangle")]
    [InlineData("""{ "id": "npc.a", "facing": "south", "step_ticks": 32, "move": "wander", "x": 6, "y": 5, "areas": [{ "x": 6, "y": 5, "width": 0, "height": 2 }], "pace_ticks": 64 }""", "holds no tile")]
    [InlineData("""{ "id": "npc.a", "facing": "south", "step_ticks": 32, "move": "wander", "x": 6, "y": 5, "areas": [{ "x": 6, "y": 5, "width": 3, "height": 2 }], "pace_ticks": 16 }""", "a pace of 16 ticks")]
    [InlineData("""{ "id": "npc.a", "facing": "south", "step_ticks": 32, "move": "chase", "x": 6, "y": 5, "areas": [{ "x": 6, "y": 5, "width": 3, "height": 2 }], "pace_ticks": 64 }""", "target)")]
    [InlineData("""{ "id": "npc.a", "facing": "south", "step_ticks": 32, "move": "chase", "x": 6, "y": 5, "areas": [{ "x": 6, "y": 5, "width": 3, "height": 2 }], "pace_ticks": 64, "target": "npc.a" }""", "chases itself")]
    [InlineData("""{ "id": "npc.a", "facing": "south", "step_ticks": 32, "move": "route", "speed": 2, "tiles": [{ "x": 2, "y": 6, "wait_ticks": 0 }] }""", "an unknown field")]
    [InlineData("""{ "id": "npc.a", "id": "npc.b", "facing": "south", "step_ticks": 32, "move": "route", "tiles": [{ "x": 2, "y": 6, "wait_ticks": 0 }] }""", "id")]
    [InlineData("""{ "facing": "south", "step_ticks": 32, "move": "route", "tiles": [{ "x": 2, "y": 6, "wait_ticks": 0 }] }""", "id)")]
    [InlineData("""{ "id": "npc.a", "facing": "south", "step_ticks": 32, "tiles": [{ "x": 2, "y": 6, "wait_ticks": 0 }] }""", "move)")]
    [InlineData("""{ "id": "patrol.a", "facing": "south", "step_ticks": 32, "move": "route", "tiles": [{ "x": 2, "y": 6, "wait_ticks": 0 }] }""", "entries of the kind 'npc'")]
    public void AMalformedNpcFailsWithTheFileAndTheReason(string npc, string reason)
    {
        ContentException error = Assert.Throws<ContentException>(() => HubMaps.Of(npcs: npc));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
        Assert.Contains("hub-test.json", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AMapWithNoNpcsFieldFailsTheLoad()
    {
        // G-6: an absent field is an error, and an empty list names a map with no NPC.
        string text = HubMaps.Text().Replace(" \"npcs\": [],\n", string.Empty, StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => TestMaps.Of("no-npcs.json", text));

        Assert.Contains("npcs", error.Message, StringComparison.Ordinal);
        Assert.Contains("absent", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(NpcMove.Wander, "wander")]
    [InlineData(NpcMove.Route, "route")]
    [InlineData(NpcMove.Chase, "chase")]
    public void EachMoveReadsBackItsName(NpcMove move, string name)
    {
        Assert.Equal(name, NpcMoves.NameOf(move));
        Assert.True(NpcMoves.TryOf(name, out NpcMove parsed));
        Assert.Equal(move, parsed);
    }

    [Fact]
    public void AValueThatNamesNoMoveIsAnError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => NpcMoves.NameOf((NpcMove)9));
    }
}
