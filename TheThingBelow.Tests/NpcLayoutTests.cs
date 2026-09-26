using System;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The rules that bind the NPCs of a map to its ground, its things, and each other (D-739,
/// D-1138, D-1139). Each fault fails the load with the file, the NPC, and the tile (T-2).
/// </summary>
public sealed class NpcLayoutTests
{
    [Fact]
    public void AWandererARouteAndAChaserLoadOnOneMap()
    {
        GameMap map = HubMaps.Of(npcs: $"{HubMaps.Wanderer()}, {HubMaps.Walker()}, {HubMaps.Chaser()}, {HubMaps.Keeper}");

        Assert.Equal(4, map.Npcs.Count);
        Assert.True(map.PlacesNpc(Id("npc.hub_child")));
        Assert.False(map.PlacesNpc(Id("npc.hub_absent")));
    }

    [Fact]
    public void ARectangleCanHoldAThingThatIsNotSolid()
    {
        // D-1139: the walk treats the tile of the marker as a blocked step, which becomes a pause.
        GameMap map = HubMaps.Of(
            npcs: HubMaps.Wanderer(x: 2, y: 5, areas: """{ "x": 1, "y": 5, "width": 3, "height": 2 }"""),
            things: HubMaps.Marker);

        Assert.Single(map.Npcs);
    }

    [Theory]
    [InlineData("""{ "x": 6, "y": 5, "width": 5, "height": 2 }""", 6, 5, "and the map is 10 by 8 tiles")]
    [InlineData("""{ "x": 3, "y": 2, "width": 3, "height": 2 }""", 3, 2, "its tile (4, 3) takes no step")]
    [InlineData("""{ "x": 6, "y": 5, "width": 3, "height": 2 }""", 5, 5, "starts at (5, 5), and no rectangle of its range holds that tile")]
    [InlineData("""{ "x": 1, "y": 1, "width": 3, "height": 1 }""", 1, 1, "starts on (1, 1), which the thing 'spawn_point.hub_test_start' holds")]
    public void ARangeThatBreaksARuleFailsWithTheNpcAndTheTile(string areas, int x, int y, string reason)
    {
        ContentException error = Assert.Throws<ContentException>(
            () => HubMaps.Of(npcs: HubMaps.Wanderer(x: x, y: y, areas: areas)));

        Assert.Contains("npc.hub_dog", error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
        Assert.Contains("hub-test.json", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARectangleOverAServicePointFailsTheLoad()
    {
        // D-1142: a service point is solid, so no range holds its tile.
        ContentException error = Assert.Throws<ContentException>(() => HubMaps.Of(
            npcs: $"{HubMaps.Keeper}, {HubMaps.Wanderer(x: 7, y: 1, areas: """{ "x": 7, "y": 1, "width": 2, "height": 1 }""")}",
            services: $"{HubMaps.RestOnKeeper}, {HubMaps.SaveOnBed}",
            things: HubMaps.Bed));

        Assert.Contains("its tile (8, 1) takes no step", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("""{ "x": 1, "y": 2, "wait_ticks": 0 }, { "x": 3, "y": 5, "wait_ticks": 0 }""", "each leg of a route is straight and on one axis")]
    [InlineData("""{ "x": 1, "y": 2, "wait_ticks": 0 }, { "x": 1, "y": 2, "wait_ticks": 0 }""", "each leg of a route is straight and on one axis")]
    [InlineData("""{ "x": 3, "y": 2, "wait_ticks": 0 }, { "x": 5, "y": 2, "wait_ticks": 0 }, { "x": 5, "y": 5, "wait_ticks": 0 }""", "the route tile (5, 3), which takes no step")]
    [InlineData("""{ "x": 0, "y": 2, "wait_ticks": 0 }""", "the route tile (0, 2), which takes no step")]
    [InlineData("""{ "x": 1, "y": 6, "wait_ticks": 0 }""", "walks across (1, 6), which the thing 'marker.hub_corner' holds")]
    [InlineData("""{ "x": 1, "y": 4, "wait_ticks": 0 }, { "x": 1, "y": 6, "wait_ticks": 0 }""", "walks across (1, 6), which the thing 'marker.hub_corner' holds")]
    [InlineData("""{ "x": 1, "y": 3, "wait_ticks": 0 }, { "x": 1, "y": 0, "wait_ticks": 0 }""", "the route tile (1, 0), which takes no step")]
    public void ARouteThatBreaksARuleFailsWithTheNpcAndTheTile(string tiles, string reason)
    {
        ContentException error = Assert.Throws<ContentException>(
            () => HubMaps.Of(npcs: HubMaps.Walker(tiles: tiles), things: HubMaps.Marker));

        Assert.Contains("npc.hub_barmaid", error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARouteThatCrossesTheSpawnPointFailsTheLoad()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => HubMaps.Of(npcs: HubMaps.Walker(tiles: """{ "x": 1, "y": 3, "wait_ticks": 0 }, { "x": 1, "y": 1, "wait_ticks": 0 }""")));

        Assert.Contains("spawn_point.hub_test_start", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AChaserOfAnAbsentNpcFailsTheLoad()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => HubMaps.Of(npcs: HubMaps.Chaser(target: "npc.hub_cat")));

        Assert.Contains("chases 'npc.hub_cat', and this map places no such NPC", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoNpcsWithOneIdFailTheLoad()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => HubMaps.Of(npcs: $"{HubMaps.Keeper}, {HubMaps.Walker(id: "npc.hub_keeper")}"));

        Assert.Contains("two NPCs of this map take the id 'npc.hub_keeper'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoNpcsOnOneStartTileFailTheLoad()
    {
        // D-1139: an NPC is solid, so no two NPCs share a tile.
        ContentException error = Assert.Throws<ContentException>(
            () => HubMaps.Of(npcs: $"{HubMaps.Wanderer()}, {HubMaps.Wanderer(id: "npc.hub_cat")}"));

        Assert.Contains("the NPC 'npc.hub_cat' starts at (6, 5), and the NPC 'npc.hub_dog' starts there too", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnNpcOnTheStartOfAnEnemyFailsTheLoad()
    {
        // D-1139: the body of an enemy blocks an NPC as it blocks the lead.
        ContentException error = Assert.Throws<ContentException>(() => HubMaps.Of(
            npcs: HubMaps.Walker(tiles: """{ "x": 1, "y": 2, "wait_ticks": 0 }"""),
            kind: "dungeon",
            enemies: PatrolMaps.Enemy(stations: """
               "routes": [
                { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 1, "y": 2 }, { "x": 3, "y": 2 }] }
               ]
              """)));

        Assert.Contains("the enemy 'patrol.one' starts on that tile", error.Message, StringComparison.Ordinal);
    }

    private static ContentId Id(string value) => ContentId.Parse(value, "test", "id");
}
