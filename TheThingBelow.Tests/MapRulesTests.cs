using System;
using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The numbers of the map: the length of a step, and the sight range (D-164, D-193).</summary>
public sealed class MapRulesTests
{
    [Fact]
    public void AStepTakesAQuarterOfASecond()
    {
        // The loop runs 60 ticks a second (D-164), so the party walks four tiles a second.
        Assert.Equal(15, MapRules.TicksPerStep);
    }

    [Theory]
    [InlineData(TimeOfDay.Dawn, 8)]
    [InlineData(TimeOfDay.Day, 12)]
    [InlineData(TimeOfDay.Dusk, 8)]
    [InlineData(TimeOfDay.Night, 5)]
    public void TheTimeOfDayGivesTheSightRangeOfTheParty(TimeOfDay time, int range)
    {
        Assert.Equal(range, MapRules.PartySightRange(time));
    }

    [Fact]
    public void ADayMapReachesPastTheFrameAndANightMapDoesNot()
    {
        // The frame holds 20 by 11.25 tiles (D-633). A day map hides no enemy that the
        // player can see, and a night map hides a patrol until it comes close (D-719).
        const int halfOfTheFrameWidth = 10;

        Assert.True(MapRules.PartySightRange(TimeOfDay.Day) > halfOfTheFrameWidth);
        Assert.True(MapRules.PartySightRange(TimeOfDay.Night) < halfOfTheFrameWidth);
    }

    [Fact]
    public void ATimeThatNamesNoValueIsAnError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MapRules.PartySightRange((TimeOfDay)9));
    }

    [Fact]
    public void ThePartyEntersOpenGroundAndADoorwayAndNoWall()
    {
        Assert.True(MapRules.CanEnter(TestMaps.Room, new TilePoint(2, 2)));
        Assert.True(MapRules.CanEnter(TestMaps.Room, new TilePoint(11, 4)));
        Assert.False(MapRules.CanEnter(TestMaps.Room, new TilePoint(5, 3)));
    }

    [Fact]
    public void ThePartyEntersNoTileOutsideTheMap()
    {
        Assert.False(MapRules.CanEnter(TestMaps.Room, new TilePoint(-1, 2)));
        Assert.False(MapRules.CanEnter(TestMaps.Room, new TilePoint(2, 9)));
    }
}
