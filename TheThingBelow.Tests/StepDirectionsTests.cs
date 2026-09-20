using System;
using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The four directions of a step, and the tile that each one reaches (D-716).</summary>
public sealed class StepDirectionsTests
{
    [Fact]
    public void AStepGoesInFourDirectionsAlone()
    {
        // D-716: a diagonal walk takes two steps, so every step costs the same ticks.
        Assert.Equal(4, StepDirections.All.Length);
    }

    [Theory]
    [InlineData(StepDirection.North, 5, 4)]
    [InlineData(StepDirection.South, 5, 6)]
    [InlineData(StepDirection.East, 6, 5)]
    [InlineData(StepDirection.West, 4, 5)]
    public void EachDirectionReachesTheTileBesideIt(StepDirection direction, int x, int y)
    {
        Assert.Equal(new TilePoint(x, y), new TilePoint(5, 5).Step(direction));
    }

    [Theory]
    [InlineData(StepDirection.North, "north")]
    [InlineData(StepDirection.South, "south")]
    [InlineData(StepDirection.East, "east")]
    [InlineData(StepDirection.West, "west")]
    public void EachDirectionNamesItself(StepDirection direction, string name)
    {
        Assert.Equal(name, StepDirections.NameOf(direction));
    }

    [Fact]
    public void AValueThatNamesNoDirectionIsAnError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => StepDirections.NameOf((StepDirection)9));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TilePoint(0, 0).Step((StepDirection)9));
    }

    [Fact]
    public void ATilePointReadsAsItsColumnAndItsRow()
    {
        Assert.Equal("(3, 7)", new TilePoint(3, 7).ToString());
    }
}
