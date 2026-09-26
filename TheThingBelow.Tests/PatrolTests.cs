using System;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The record of one enemy that a map places: its fields, its stations, and the rule that
/// the time of day of the map picks one station (D-738 to D-743, D-752).
/// </summary>
public sealed class PatrolTests
{
    [Fact]
    public void AnEnemyReadsEveryFieldOfItsRecord()
    {
        GameMap map = PatrolMaps.Of(PatrolMaps.Walker);

        Patrol patrol = Assert.Single(map.Patrols);
        Assert.Equal("patrol.one", patrol.Id.Value);
        Assert.Equal("group.one", patrol.Group.Value);
        Assert.Equal(EnemySize.Common, patrol.Size);
        Assert.Equal(StepDirection.East, patrol.Facing);
        Assert.Equal(32, patrol.StepTicks);
        Assert.Equal(3, patrol.SightRange);

        PatrolStation station = Assert.Single(patrol.Stations);
        Assert.Equal([new TilePoint(1, 1), new TilePoint(3, 1)], station.Tiles);
        Assert.Null(station.Area);
        Assert.Equal(new TilePoint(1, 1), station.Start);
    }

    [Fact]
    public void AnEnemyOfAnAreaReadsItsRectangle()
    {
        GameMap map = PatrolMaps.Of(PatrolMaps.Pacer);

        PatrolStation station = Assert.Single(Assert.Single(map.Patrols).Stations);
        Assert.Equal(new TileArea(1, 5, 3, 2), station.Area);
        Assert.Empty(station.Tiles);
        Assert.Equal(new TilePoint(1, 5), station.Start);
    }

    [Fact]
    public void TheTimeOfDayOfTheMapPicksTheStation()
    {
        // Exit test 3 of section 7.6 of `phase-2-first-playable.md` (D-193, D-442, D-743).
        string enemy = PatrolMaps.Enemy(stations: """
           "routes": [
            { "times": ["day"], "tiles": [{ "x": 1, "y": 1 }, { "x": 3, "y": 1 }] },
            { "times": ["dawn", "dusk", "night"], "tiles": [{ "x": 1, "y": 6 }, { "x": 3, "y": 6 }] }
           ]
          """);

        Patrol day = Assert.Single(PatrolMaps.Of(enemy, "day").Patrols);
        Patrol night = Assert.Single(PatrolMaps.Of(enemy, "night").Patrols);

        Assert.Equal(new TilePoint(1, 1), day.StationOf(TimeOfDay.Day)!.Start);
        Assert.Equal(new TilePoint(1, 6), night.StationOf(TimeOfDay.Night)!.Start);
        Assert.Equal(new TilePoint(1, 6), day.StationOf(TimeOfDay.Dawn)!.Start);
    }

    [Fact]
    public void ATimeThatNoStationNamesKeepsTheEnemyOffTheMap()
    {
        // D-743: one field carries both parts of D-193, the route by the time and which
        // enemies appear.
        GameMap map = PatrolMaps.Of(
            PatrolMaps.Enemy(stations: """
               "routes": [
                { "times": ["night"], "tiles": [{ "x": 1, "y": 1 }] }
               ]
              """),
            "day");

        Patrol patrol = Assert.Single(map.Patrols);
        Assert.Null(patrol.StationOf(TimeOfDay.Day));
        Assert.NotNull(patrol.StationOf(TimeOfDay.Night));
    }

    [Fact]
    public void AnEnemyWithNoRouteAndNoAreaFailsTheLoad()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => PatrolMaps.Of("""
              {
               "id": "patrol.one",
               "group": "group.one",
               "size": "common",
               "facing": "east",
               "step_ticks": 32,
               "sight_range": 3
              }
             """));

        Assert.Contains("no route and no area", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEnemyWithARouteAndAnAreaFailsTheLoad()
    {
        ContentException error = Refuse(PatrolMaps.Enemy(stations: """
           "routes": [
            { "times": ["day"], "tiles": [{ "x": 1, "y": 1 }] }
           ],
           "areas": [
            { "times": ["night"], "x": 1, "y": 5, "width": 3, "height": 2 }
           ]
          """));

        Assert.Contains("routes and areas", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ALargeEnemyWithARouteFailsTheLoad()
    {
        // D-209 gave a large enemy presence in its own area, and it refused the wide-route
        // form (D-741).
        ContentException error = Refuse(PatrolMaps.Enemy(size: "elite", stations: """
           "routes": [
            { "times": ["day"], "tiles": [{ "x": 1, "y": 1 }, { "x": 3, "y": 1 }] }
           ]
          """));

        Assert.Contains("elite with a route", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-209", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEnemyThatStepsFasterThanThePartyFailsTheLoad()
    {
        // D-742: no enemy outwalks the party, so no patrol can never be walked away from.
        ContentException error = Refuse(PatrolMaps.Enemy(stepTicks: MapRules.TicksPerStep - 1));

        Assert.Contains("steps in 15 ticks", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-742", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(30)]
    [InlineData(40)]
    [InlineData(48)]
    public void AnEnemyStepThatTheTileDoesNotDivideFailsTheLoad(int ticks)
    {
        // D-821: a step of 30 ticks moved the sprite 1 or 2 pixels on each tick, which showed
        // as a hitch at each tile.
        ContentException error = Refuse(PatrolMaps.Enemy(stepTicks: ticks));

        Assert.Contains($"steps in {ticks} ticks", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-821", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(64)]
    public void AnEnemyStepOfTheListLoads(int ticks)
    {
        GameMap map = PatrolMaps.Of(PatrolMaps.Enemy(stepTicks: ticks));

        Assert.Equal(ticks, Assert.Single(map.Patrols).StepTicks);
    }

    [Fact]
    public void AnEnemyThatStepsAtThePartyRateLoads()
    {
        GameMap map = PatrolMaps.Of(PatrolMaps.Enemy(stepTicks: MapRules.TicksPerStep));

        Assert.Equal(MapRules.TicksPerStep, Assert.Single(map.Patrols).StepTicks);
    }

    [Fact]
    public void ASightRangeBelowZeroFailsTheLoad()
    {
        ContentException error = Refuse(PatrolMaps.Enemy(sightRange: -1));

        Assert.Contains("sight range -1", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnknownSizeFailsTheLoad()
    {
        ContentException error = Refuse(PatrolMaps.Enemy(size: "giant"));

        Assert.Contains("giant", error.Message, StringComparison.Ordinal);
        Assert.Contains(EnemySizes.EveryName, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnknownFacingFailsTheLoad()
    {
        ContentException error = Refuse(PatrolMaps.Enemy(facing: "up"));

        Assert.Contains("faces 'up'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnknownTimeOfDayFailsTheLoad()
    {
        ContentException error = Refuse(PatrolMaps.Enemy(stations: """
           "routes": [
            { "times": ["midnight"], "tiles": [{ "x": 1, "y": 1 }] }
           ]
          """));

        Assert.Contains("midnight", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStationWithNoTimeFailsTheLoad()
    {
        ContentException error = Refuse(PatrolMaps.Enemy(stations: """
           "routes": [
            { "times": [], "tiles": [{ "x": 1, "y": 1 }] }
           ]
          """));

        Assert.Contains("names no time of day", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAreaWithNoTimeFailsTheLoad()
    {
        ContentException error = Refuse(PatrolMaps.Enemy(stations: """
           "areas": [
            { "times": [], "x": 1, "y": 5, "width": 3, "height": 2 }
           ]
          """));

        Assert.Contains("names no time of day", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoStationsOfOneTimeFailTheLoad()
    {
        // D-743: one time picks one station, so two stations of one time would leave the
        // pick to the order of the file.
        ContentException error = Refuse(PatrolMaps.Enemy(stations: """
           "routes": [
            { "times": ["day", "night"], "tiles": [{ "x": 1, "y": 1 }] },
            { "times": ["night"], "tiles": [{ "x": 3, "y": 1 }] }
           ]
          """));

        Assert.Contains("names the time 'night' on 2 stations", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARouteWithNoTileFailsTheLoad()
    {
        ContentException error = Refuse(PatrolMaps.Enemy(stations: """
           "routes": [
            { "times": ["day"], "tiles": [] }
           ]
          """));

        Assert.Contains("holds no tile", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEmptyListOfStationsFailsTheLoad()
    {
        ContentException error = Refuse(PatrolMaps.Enemy(stations: "  \"routes\": []"));

        Assert.Contains("empty list of stations", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAreaOfNoTileFailsTheLoad()
    {
        ContentException error = Refuse(PatrolMaps.Enemy(stations: """
           "areas": [
            { "times": ["day"], "x": 1, "y": 5, "width": 0, "height": 2 }
           ]
          """));

        Assert.Contains("holds no tile", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnIdOfAnotherKindFailsTheLoad()
    {
        // The kind of the id names the record type, and a placement takes the kind `patrol`
        // (D-646, D-752).
        ContentException error = Refuse(PatrolMaps.Enemy(id: "enemy.one"));

        Assert.Contains("patrol", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AGroupOfAnotherKindFailsTheLoad()
    {
        ContentException error = Refuse(PatrolMaps.Enemy(group: "patrol.one_group"));

        Assert.Contains("group", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnknownFieldOfAnEnemyFailsTheLoad()
    {
        ContentException error = Refuse(PatrolMaps.Enemy(extra: "  \"speed\": 4,"));

        Assert.Contains("speed", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAbsentFieldOfAnEnemyFailsTheLoad()
    {
        // An absent field is an error, never a default value (T-2, G-6).
        ContentException error = Assert.Throws<ContentException>(
            () => PatrolMaps.Of("""
              {
               "id": "patrol.one",
               "group": "group.one",
               "size": "common",
               "facing": "east",
               "routes": [
                { "times": ["day"], "tiles": [{ "x": 1, "y": 1 }] }
               ]
              }
             """));

        Assert.Contains("step_ticks", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AMapWithNoEnemiesFieldFailsTheLoad()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => TestMaps.Of("no-enemies.json", """
            {
             "comment": "a map with no enemies field",
             "id": "map.no_enemies",
             "region": "region.test",
             "label": "label.no_enemies",
             "time": "day",
             "dark": false,
             "kind": "dungeon", "npcs": [], "services": [],
             "terrain": [ "###", "#.#", "###" ],
             "things": [
              { "id": "spawn_point.no_enemies_start", "kind": "spawn_point", "x": 1, "y": 1 }
             ]
            }
            """));

        Assert.Contains("enemies", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStationWithNoTimeIsAnErrorOfTheType()
    {
        Assert.Throws<ArgumentException>(() => new PatrolStation([], [new TilePoint(0, 0)], null));
    }

    [Fact]
    public void AStationWithARouteAndAnAreaIsAnErrorOfTheType()
    {
        Assert.Throws<ArgumentException>(() => new PatrolStation(
            [TimeOfDay.Day],
            [new TilePoint(0, 0)],
            new TileArea(0, 0, 2, 2)));
    }

    [Fact]
    public void AStationWithNoRouteAndNoAreaIsAnErrorOfTheType()
    {
        Assert.Throws<ArgumentException>(() => new PatrolStation([TimeOfDay.Day], [], null));
    }

    [Fact]
    public void ALegOfARouteIsStraightAndOnOneAxis()
    {
        // D-716: a step goes in four directions alone, so each leg of a route lies on one
        // axis (D-739).
        Assert.True(PatrolStation.TryLeg(new TilePoint(2, 3), new TilePoint(6, 3), out StepDirection east, out int across));
        Assert.Equal(StepDirection.East, east);
        Assert.Equal(4, across);

        Assert.True(PatrolStation.TryLeg(new TilePoint(2, 7), new TilePoint(2, 3), out StepDirection north, out int down));
        Assert.Equal(StepDirection.North, north);
        Assert.Equal(4, down);

        Assert.False(PatrolStation.TryLeg(new TilePoint(2, 3), new TilePoint(4, 5), out _, out _));
        Assert.False(PatrolStation.TryLeg(new TilePoint(2, 3), new TilePoint(2, 3), out _, out _));
    }

    private static ContentException Refuse(string enemy) =>
        Assert.Throws<ContentException>(() => PatrolMaps.Of(enemy));
}
