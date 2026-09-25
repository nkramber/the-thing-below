using System;
using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The numbers of the map: the length of a step, and the sight range (D-164, D-193).</summary>
public sealed class MapRulesTests
{
    [Fact]
    public void AStepMovesTheLeadTwoPixelsOnEachTick()
    {
        // D-821. The loop runs 60 ticks a second (D-164), so the party walks 3.75 tiles a
        // second, and each tick of a step of 32 art pixels moves the lead by 2.
        Assert.Equal(16, MapRules.TicksPerStep);
        Assert.Equal(0, 32 % MapRules.TicksPerStep);
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
    public void ANightMapHoldsPatrolsThatSeeLessFarThanADayMap()
    {
        // The range of the party is the ceiling of the sight of each patrol (D-720), and it
        // decides nothing that Game draws (D-814).
        Assert.True(MapRules.PartySightRange(TimeOfDay.Night) < MapRules.PartySightRange(TimeOfDay.Dusk));
        Assert.True(MapRules.PartySightRange(TimeOfDay.Dusk) < MapRules.PartySightRange(TimeOfDay.Day));
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
    public void AServicePointBlocksAStepOntoItsTile()
    {
        // D-1142: a service point is solid, and the ground under it is floor.
        GameMap inn = HubMaps.Inn;
        var bed = new TilePoint(8, 1);

        Assert.Equal(TileKind.Floor, inn.TileAt(bed));
        Assert.True(inn.HoldsSolidThing(bed));
        Assert.False(MapRules.CanEnter(inn, bed));
        Assert.True(MapRules.CanEnter(inn, new TilePoint(7, 1)));
    }

    [Fact]
    public void AThingThatIsNotSolidBlocksNoStep()
    {
        GameMap map = HubMaps.Of(things: HubMaps.Marker);

        Assert.False(map.HoldsSolidThing(new TilePoint(1, 6)));
        Assert.True(MapRules.CanEnter(map, new TilePoint(1, 6)));
        Assert.True(MapRules.CanEnter(map, map.Spawn));
        Assert.False(map.HoldsSolidThing(new TilePoint(-1, 6)));
    }

    [Fact]
    public void ThePartyEntersNoTileOutsideTheMap()
    {
        Assert.False(MapRules.CanEnter(TestMaps.Room, new TilePoint(-1, 2)));
        Assert.False(MapRules.CanEnter(TestMaps.Room, new TilePoint(2, 9)));
    }
}
