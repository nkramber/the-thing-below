using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The rectangle that holds a large enemy in its own place (D-209, D-741).</summary>
public sealed class TileAreaTests
{
    [Fact]
    public void AnAreaHoldsEveryTileInsideIt()
    {
        TileArea area = new(3, 5, 4, 2);

        Assert.True(area.Holds(new TilePoint(3, 5)));
        Assert.True(area.Holds(new TilePoint(6, 6)));
        Assert.False(area.Holds(new TilePoint(7, 6)));
        Assert.False(area.Holds(new TilePoint(3, 7)));
        Assert.False(area.Holds(new TilePoint(2, 5)));
    }

    [Fact]
    public void TheAnchorOfAnAreaIsItsNorthWestTile()
    {
        Assert.Equal(new TilePoint(3, 5), new TileArea(3, 5, 4, 2).Anchor);
    }

    [Fact]
    public void AnAreaHoldsABodyOnlyWhenTheWholeBodyFits()
    {
        // D-209: a body fits everywhere in its area, so the area holds every tile of it.
        TileArea area = new(3, 5, 4, 3);

        Assert.True(area.HoldsBody(new EnemyBody(new TilePoint(3, 5), EnemySize.Elite)));
        Assert.True(area.HoldsBody(new EnemyBody(new TilePoint(5, 6), EnemySize.Elite)));
        Assert.False(area.HoldsBody(new EnemyBody(new TilePoint(6, 5), EnemySize.Elite)));
        Assert.False(area.HoldsBody(new EnemyBody(new TilePoint(3, 7), EnemySize.Elite)));
        Assert.True(area.HoldsBody(new EnemyBody(new TilePoint(3, 5), EnemySize.Boss)));
        Assert.True(area.HoldsBody(new EnemyBody(new TilePoint(4, 5), EnemySize.Boss)));
        Assert.False(area.HoldsBody(new EnemyBody(new TilePoint(5, 5), EnemySize.Boss)));
    }

    [Fact]
    public void AnAreaOfNoTileDescribesNoRectangle()
    {
        Assert.True(new TileArea(0, 0, 1, 1).IsRectangle());
        Assert.False(new TileArea(0, 0, 0, 1).IsRectangle());
        Assert.False(new TileArea(0, 0, 1, 0).IsRectangle());
        Assert.False(new TileArea(-1, 0, 1, 1).IsRectangle());
        Assert.False(new TileArea(0, -1, 1, 1).IsRectangle());
    }

    [Fact]
    public void TheTextOfAnAreaNamesItsTileAndItsSize()
    {
        Assert.Equal("(3, 5) by 4 by 2", new TileArea(3, 5, 4, 2).ToString());
    }
}
