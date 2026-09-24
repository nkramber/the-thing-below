using System;
using System.IO;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The rules that bind the enemies of one map to its ground: the sight range floor, each
/// route, each area, and each start tile (D-720, D-739, D-741).
/// </summary>
public sealed class PatrolLayoutTests
{
    [Fact]
    public void ARouteLegThatBendsFailsTheLoad()
    {
        // D-716: a step goes in four directions alone, so each leg of a route lies on one
        // axis (D-739).
        ContentException error = Refuse(PatrolMaps.Enemy(stations: """
           "routes": [
            { "times": ["day"], "tiles": [{ "x": 1, "y": 1 }, { "x": 3, "y": 2 }] }
           ]
          """));

        Assert.Contains("straight and on one axis", error.Message, StringComparison.Ordinal);
        Assert.Contains("(1, 1)", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARouteTileInAWallFailsTheLoad()
    {
        ContentException error = Refuse(PatrolMaps.Enemy(stations: """
           "routes": [
            { "times": ["day"], "tiles": [{ "x": 4, "y": 3 }] }
           ]
          """));

        Assert.Contains("route tile (4, 3)", error.Message, StringComparison.Ordinal);
        Assert.Contains("takes no step", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARouteTileOutsideTheMapFailsTheLoad()
    {
        ContentException error = Refuse(PatrolMaps.Enemy(stations: """
           "routes": [
            { "times": ["day"], "tiles": [{ "x": 40, "y": 1 }] }
           ]
          """));

        Assert.Contains("route tile (40, 1)", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ALegThatCrossesAWallFailsTheLoad()
    {
        // Exit test 1 of section 7.6: the load proves that a patrol never leaves its route
        // onto a wall (D-739).
        ContentException error = Refuse(PatrolMaps.Enemy(stations: """
           "routes": [
            { "times": ["day"], "tiles": [{ "x": 2, "y": 3 }, { "x": 7, "y": 3 }] }
           ]
          """));

        Assert.Contains("takes no step at (4, 3)", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAreaOutsideTheMapFailsTheLoad()
    {
        ContentException error = Refuse(PatrolMaps.Enemy(stations: """
           "areas": [
            { "times": ["day"], "x": 8, "y": 5, "width": 4, "height": 2 }
           ]
          """));

        Assert.Contains("the map is 10 by 8 tiles", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(2147483640, 1, 100, 2)]
    [InlineData(1, 2147483640, 3, 100)]
    [InlineData(2147483647, 1, 1, 2)]
    public void AnAreaWhoseEdgePassesTheRangeOfAnIntFailsTheLoad(int x, int y, int width, int height)
    {
        // A regression test for P2-1 of `docs/reviews/pr-45.md`. The sum of a coordinate
        // and a side wrapped below zero, so the area passed the edge check and the fit loop
        // ran no pass (D-209, D-741, T-2).
        ContentException error = Refuse(PatrolMaps.Enemy(stations: $$"""
           "areas": [
            { "times": ["day"], "x": {{x}}, "y": {{y}}, "width": {{width}}, "height": {{height}} }
           ]
          """));

        Assert.Contains("patrol.one", error.Message, StringComparison.Ordinal);
        Assert.Contains("the map is 10 by 8 tiles", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAreaSmallerThanTheBodyFailsTheLoad()
    {
        ContentException error = Refuse(PatrolMaps.Enemy(size: "boss", stations: """
           "areas": [
            { "times": ["day"], "x": 1, "y": 5, "width": 2, "height": 2 }
           ]
          """));

        Assert.Contains("body is 3 by 3 tiles", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-209", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAreaThatTheBodyFillsFailsTheLoad()
    {
        // D-209: a large enemy does not stand perfectly still, so its area leaves room to
        // move (D-741).
        ContentException error = Refuse(PatrolMaps.Enemy(size: "elite", stations: """
           "areas": [
            { "times": ["day"], "x": 1, "y": 5, "width": 2, "height": 2 }
           ]
          """));

        Assert.Contains("fills it", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAreaWithAWallInItFailsTheLoad()
    {
        // Exit test 4 of section 7.6: a load proves that the body fits everywhere in its
        // area (D-209, D-741).
        ContentException error = Refuse(PatrolMaps.Enemy(size: "elite", stations: """
           "areas": [
            { "times": ["day"], "x": 3, "y": 2, "width": 4, "height": 3 }
           ]
          """));

        Assert.Contains("fits everywhere in its area", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAreaThatHoldsTheBodyEverywhereLoads()
    {
        GameMap map = PatrolMaps.Of(PatrolMaps.Enemy(size: "elite", stations: """
           "areas": [
            { "times": ["dawn", "day", "dusk", "night"], "x": 1, "y": 1, "width": 3, "height": 2 }
           ]
          """));

        Assert.Equal(EnemySize.Elite, Assert.Single(map.Patrols).Size);
    }

    [Fact]
    public void ASightRangeAboveThePartyRangeFailsTheLoad()
    {
        // D-720: the player never loses to a thing that it could not see.
        ContentException error = Refuse(PatrolMaps.Enemy(sightRange: MapRules.PartySightRange(TimeOfDay.Night) + 1), "night");

        Assert.Contains("the party sees 5 on a map set to night", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-720", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASightRangeAtThePartyRangeLoads()
    {
        GameMap map = PatrolMaps.Of(
            PatrolMaps.Enemy(sightRange: MapRules.PartySightRange(TimeOfDay.Night)),
            "night");

        Assert.Equal(5, Assert.Single(map.Patrols).SightRange);
    }

    [Fact]
    public void ASightRangeAboveTheDarkRangeFailsTheLoadOfADarkMap()
    {
        // D-1063: the floor of D-720 reads the range of a dark map with the torch put away, so
        // the bonus of a held torch keeps each patrol inside the sight of the party.
        ContentException error = Assert.Throws<ContentException>(
            () => PatrolMaps.Of(PatrolMaps.Enemy(sightRange: MapRules.DarkSightRange + 1), "day", dark: true));

        Assert.Contains("the party sees 2 on a dark map with the torch put away", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-1063", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASightRangeAtTheDarkRangeLoadsOnADarkMapOfAnyTime()
    {
        foreach (TimeOfDay time in TimesOfDay.All)
        {
            GameMap map = PatrolMaps.Of(PatrolMaps.Enemy(sightRange: MapRules.DarkSightRange), TimesOfDay.NameOf(time), dark: true);

            Assert.True(map.Dark);
            Assert.Equal(MapRules.DarkSightRange, Assert.Single(map.Patrols).SightRange);
        }
    }

    [Fact]
    public void TwoEnemiesOfOneIdFailTheLoad()
    {
        ContentException error = Refuse($"{PatrolMaps.Enemy()},\n{PatrolMaps.Enemy(stations: Southwest)}");

        Assert.Contains("two enemies of this map take the id 'patrol.one'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEnemyOnTheSpawnPointFailsTheLoad()
    {
        ContentException error = Refuse(PatrolMaps.Enemy(stations: """
           "routes": [
            { "times": ["day"], "tiles": [{ "x": 8, "y": 6 }] }
           ]
          """));

        Assert.Contains("the spawn point (8, 6)", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoEnemiesOnOneStartTileFailTheLoad()
    {
        ContentException error = Refuse($"{PatrolMaps.Enemy()},\n{PatrolMaps.Enemy(id: "patrol.two")}");

        Assert.Contains("the enemy 'patrol.one' holds the body", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoBodiesThatTouchOnOneTileFailTheLoad()
    {
        // The body of an elite holds four tiles, so a start tile one step away still shares
        // a tile with it (D-206).
        string elite = PatrolMaps.Enemy(id: "patrol.big", size: "elite", stations: """
           "areas": [
            { "times": ["day"], "x": 1, "y": 1, "width": 3, "height": 2 }
           ]
          """);
        string common = PatrolMaps.Enemy(id: "patrol.small", stations: """
           "routes": [
            { "times": ["day"], "tiles": [{ "x": 2, "y": 2 }] }
           ]
          """);

        ContentException error = Refuse($"{elite},\n{common}");

        Assert.Contains("patrol.big", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEnemyThatNoTimeOfDayPlacesTakesNoStartTile()
    {
        // D-743: a time that no station names keeps the enemy off the map, so its start tile
        // never meets the check of a crowded tile.
        GameMap map = PatrolMaps.Of(
            $"{PatrolMaps.Enemy()},\n{PatrolMaps.Enemy(id: "patrol.two", stations: NightOnly)}",
            "day");

        Assert.Equal(2, map.Patrols.Count);
        Assert.Null(map.Patrols[1].StationOf(TimeOfDay.Day));
    }

    [Fact]
    public void TheFixtureDungeonHoldsItsEnemiesAndLoads()
    {
        // The content of the checkout meets every rule of a map, so the load of the run of a
        // player never fails (D-528, T-2).
        GameMap map = TestMaps.FixtureDungeon;

        Assert.Equal(3, map.Patrols.Count);
        Assert.Equal("patrol.fixture_dungeon_hall", map.Patrols[0].Id.Value);
        Assert.Equal(EnemySize.Elite, map.Patrols[2].Size);
        foreach (Patrol patrol in map.Patrols)
        {
            Assert.True(patrol.SightRange <= MapRules.PartySightRange(map.Time));
            Assert.NotNull(patrol.StationOf(map.Time));
        }
    }

    [Fact]
    public void EveryMapOfTheCheckoutLoadsWithItsEnemies()
    {
        foreach (string path in Directory.GetFiles(RepositoryRoot.PathTo("content/rules/maps")))
        {
            GameMap map = GameMap.Read(File.ReadAllBytes(path), path);
            Assert.NotNull(map.Patrols);
        }
    }

    private const string Southwest = """
       "routes": [
        { "times": ["day"], "tiles": [{ "x": 1, "y": 6 }] }
       ]
      """;

    private const string NightOnly = """
       "routes": [
        { "times": ["night"], "tiles": [{ "x": 1, "y": 1 }] }
       ]
      """;

    private static ContentException Refuse(string enemies, string time = "day") =>
        Assert.Throws<ContentException>(() => PatrolMaps.Of(enemies, time));
}
