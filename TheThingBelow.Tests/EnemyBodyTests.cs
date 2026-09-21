using System;
using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The tiles of one enemy: the block, the sort value, and the tile that carries the sight
/// (D-206, D-737).
/// </summary>
public sealed class EnemyBodyTests
{
    [Fact]
    public void ACommonBodyHoldsItsAnchorTileAlone()
    {
        EnemyBody body = new(new TilePoint(4, 6), EnemySize.Common);

        Assert.Equal(1, body.Side);
        Assert.True(body.Holds(new TilePoint(4, 6)));
        Assert.False(body.Holds(new TilePoint(5, 6)));
        Assert.False(body.Holds(new TilePoint(4, 7)));
    }

    [Fact]
    public void AnEliteBodyHoldsFourTilesFromItsAnchor()
    {
        // The anchor is the north-west tile, so the body covers the tiles to the east and to
        // the south of it (D-206, D-737).
        EnemyBody body = new(new TilePoint(4, 6), EnemySize.Elite);

        Assert.Equal(2, body.Side);
        Assert.True(body.Holds(new TilePoint(4, 6)));
        Assert.True(body.Holds(new TilePoint(5, 6)));
        Assert.True(body.Holds(new TilePoint(4, 7)));
        Assert.True(body.Holds(new TilePoint(5, 7)));
        Assert.False(body.Holds(new TilePoint(6, 6)));
        Assert.False(body.Holds(new TilePoint(3, 6)));
    }

    [Fact]
    public void ABossBodyHoldsNineTiles()
    {
        EnemyBody body = new(new TilePoint(0, 0), EnemySize.Boss);

        Assert.Equal(3, body.Side);
        Assert.True(body.Holds(new TilePoint(2, 2)));
        Assert.False(body.Holds(new TilePoint(3, 2)));
    }

    [Fact]
    public void TheSortValueComesFromTheFrontRowOfTheBody()
    {
        // D-737: the sort value comes from the front row, so the body draws in front of what
        // it stands before. Godot sorts each canvas item by one Y value.
        Assert.Equal(6, new EnemyBody(new TilePoint(4, 6), EnemySize.Common).SortRow);
        Assert.Equal(7, new EnemyBody(new TilePoint(4, 6), EnemySize.Elite).SortRow);
        Assert.Equal(8, new EnemyBody(new TilePoint(4, 6), EnemySize.Boss).SortRow);
    }

    [Fact]
    public void TheSightStartsAtTheTileOfTheBodyNearestTheOtherTile()
    {
        // D-737: the body sees where it blocks, so the range floor of D-720 reads from the
        // edge of the body.
        EnemyBody body = new(new TilePoint(4, 4), EnemySize.Boss);

        Assert.Equal(new TilePoint(4, 4), body.Nearest(new TilePoint(0, 0)));
        Assert.Equal(new TilePoint(6, 6), body.Nearest(new TilePoint(9, 9)));
        Assert.Equal(new TilePoint(5, 4), body.Nearest(new TilePoint(5, 1)));
        Assert.Equal(new TilePoint(6, 5), body.Nearest(new TilePoint(8, 5)));
    }

    [Fact]
    public void TheNearestTileOfATileOfTheBodyIsThatTile()
    {
        EnemyBody body = new(new TilePoint(4, 4), EnemySize.Elite);

        Assert.Equal(new TilePoint(5, 5), body.Nearest(new TilePoint(5, 5)));
    }

    [Fact]
    public void AStepMovesTheWholeBody()
    {
        EnemyBody body = new(new TilePoint(4, 4), EnemySize.Elite);

        EnemyBody moved = body.Step(StepDirection.East);

        Assert.Equal(new TilePoint(5, 4), moved.Anchor);
        Assert.Equal(EnemySize.Elite, moved.Size);
        Assert.True(moved.Holds(new TilePoint(6, 5)));
    }

    [Fact]
    public void TheTextOfABodyNamesItsAnchorAndItsSide()
    {
        Assert.Equal("(4, 6) by 2", new EnemyBody(new TilePoint(4, 6), EnemySize.Elite).ToString());
    }

    [Fact]
    public void ASizeThatNamesNoBodyIsAnError()
    {
        EnemyBody body = new(new TilePoint(0, 0), (EnemySize)9);

        Assert.Throws<ArgumentOutOfRangeException>(() => body.Side);
    }
}
