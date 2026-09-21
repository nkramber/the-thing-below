using System;
using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The sight of the party and the sight of a patrol (D-718, D-719). A wall stops each one,
/// and the rule reads whole tiles alone (T-7).
/// </summary>
/// <remarks>
/// The map below is a room with a pillar at the tiles (5, 3), (6, 3), (5, 4), and (6, 4).
/// The pillar gives the party a place to stand out of the sight of a patrol.
/// </remarks>
public sealed class MapSightTests
{
    private static readonly GameMap Room = TestMaps.Room;

    [Fact]
    public void ThePartySeesEveryDirectionInsideItsRange()
    {
        // D-719: the party takes no facing, so the player never turns the lead to look.
        TilePoint lead = new(3, 6);

        Assert.True(MapSight.PartySees(Room, lead, new TilePoint(3, 5), 2));
        Assert.True(MapSight.PartySees(Room, lead, new TilePoint(3, 7), 2));
        Assert.True(MapSight.PartySees(Room, lead, new TilePoint(1, 6), 2));
        Assert.True(MapSight.PartySees(Room, lead, new TilePoint(5, 6), 2));
        Assert.True(MapSight.PartySees(Room, lead, new TilePoint(1, 7), 2));
    }

    [Fact]
    public void ThePartySeesNoTilePastItsRange()
    {
        TilePoint lead = new(3, 6);

        Assert.False(MapSight.PartySees(Room, lead, new TilePoint(7, 6), 3));
        Assert.True(MapSight.PartySees(Room, lead, new TilePoint(6, 6), 3));
    }

    [Fact]
    public void ThePartySeesItsOwnTile()
    {
        Assert.True(MapSight.PartySees(Room, Room.Spawn, Room.Spawn, 0));
    }

    [Fact]
    public void AWallStopsTheSightOfTheParty()
    {
        // The pillar stands between the two tiles, so the party sees nothing past it.
        Assert.False(MapSight.PartySees(Room, new TilePoint(5, 1), new TilePoint(5, 6), 8));
        Assert.True(MapSight.PartySees(Room, new TilePoint(3, 1), new TilePoint(3, 6), 8));
    }

    [Fact]
    public void APatrolSeesTheQuarterOfTheMapThatItFaces()
    {
        // D-718: the tile lies ahead of the patrol, and no further to the side than ahead.
        TilePoint guard = new(3, 5);

        // Ahead, and no further to the side than ahead.
        Assert.True(MapSight.PatrolSees(Room, guard, StepDirection.North, 4, new TilePoint(3, 2)));
        Assert.True(MapSight.PatrolSees(Room, guard, StepDirection.North, 4, new TilePoint(1, 3)));

        // Further to the side than ahead, and outside the tiles that touch the patrol.
        Assert.False(MapSight.PatrolSees(Room, guard, StepDirection.North, 4, new TilePoint(1, 4)));

        // Behind the patrol.
        Assert.False(MapSight.PatrolSees(Room, guard, StepDirection.North, 4, new TilePoint(3, 7)));
    }

    [Fact]
    public void APatrolSeesNoTileBehindItPastTheTilesThatTouchIt()
    {
        // The sneak of D-265 needs three safe quarters, so a patrol never sees behind it.
        TilePoint guard = new(3, 5);

        Assert.False(MapSight.PatrolSees(Room, guard, StepDirection.North, 6, new TilePoint(3, 7)));
        Assert.False(MapSight.PatrolSees(Room, guard, StepDirection.East, 6, new TilePoint(1, 5)));
        Assert.False(MapSight.PatrolSees(Room, guard, StepDirection.South, 6, new TilePoint(3, 3)));
    }

    [Fact]
    public void APatrolNoticesEveryTileThatTouchesIt()
    {
        // D-718: the ring of one tile stops a walk along the flank of a patrol.
        TilePoint guard = new(3, 6);

        foreach (TilePoint beside in new TilePoint[]
        {
            new(2, 5), new(3, 5), new(4, 5),
            new(2, 6), new(4, 6),
            new(2, 7), new(3, 7), new(4, 7),
        })
        {
            Assert.True(MapSight.PatrolSees(Room, guard, StepDirection.North, 1, beside));
        }
    }

    [Fact]
    public void APatrolSeesNoTilePastItsRange()
    {
        TilePoint guard = new(3, 7);

        Assert.True(MapSight.PatrolSees(Room, guard, StepDirection.North, 3, new TilePoint(3, 4)));
        Assert.False(MapSight.PatrolSees(Room, guard, StepDirection.North, 3, new TilePoint(3, 3)));
    }

    [Fact]
    public void AWallStopsTheSightOfAPatrol()
    {
        Assert.False(MapSight.PatrolSees(Room, new TilePoint(5, 6), StepDirection.North, 6, new TilePoint(5, 1)));
        Assert.True(MapSight.PatrolSees(Room, new TilePoint(3, 6), StepDirection.North, 6, new TilePoint(3, 1)));
    }

    [Fact]
    public void TheWallRuleReadsTheSameForBothDirections()
    {
        // A line that read one way alone would let a patrol see a party that sees no patrol,
        // which a player reads as a fault of the game (T-2).
        for (int y = 1; y < Room.Height - 1; y += 1)
        {
            for (int x = 1; x < Room.Width - 1; x += 1)
            {
                TilePoint one = new(x, y);
                for (int otherY = 1; otherY < Room.Height - 1; otherY += 1)
                {
                    for (int otherX = 1; otherX < Room.Width - 1; otherX += 1)
                    {
                        TilePoint other = new(otherX, otherY);
                        Assert.Equal(MapSight.Clear(Room, one, other), MapSight.Clear(Room, other, one));
                    }
                }
            }
        }
    }

    [Fact]
    public void TheQuarterBehindAFacingIsTheMirrorOfTheQuarterThatItSees()
    {
        // D-746: one shape serves the sight of a patrol and the approach of each side, so the
        // quarter behind a facing is the mirror of the quarter of D-718.
        TilePoint at = new(5, 5);

        Assert.True(MapSight.BehindFacing(at, StepDirection.North, new TilePoint(5, 6)));
        Assert.True(MapSight.BehindFacing(at, StepDirection.North, new TilePoint(6, 7)));
        Assert.True(MapSight.BehindFacing(at, StepDirection.South, new TilePoint(5, 4)));
        Assert.True(MapSight.BehindFacing(at, StepDirection.East, new TilePoint(4, 5)));
        Assert.True(MapSight.BehindFacing(at, StepDirection.West, new TilePoint(6, 5)));
    }

    [Fact]
    public void NoTileAheadOfAFacingOrBesideItLiesBehindIt()
    {
        TilePoint at = new(5, 5);

        Assert.False(MapSight.BehindFacing(at, StepDirection.North, new TilePoint(5, 4)));
        Assert.False(MapSight.BehindFacing(at, StepDirection.North, new TilePoint(8, 5)));
        Assert.False(MapSight.BehindFacing(at, StepDirection.North, new TilePoint(8, 6)));
        Assert.False(MapSight.BehindFacing(at, StepDirection.North, at));
    }

    [Fact]
    public void ThePartAheadAndThePartBehindNeverHoldOneTile()
    {
        // A seed loop over every tile of the room: no tile lies in the quarter that a facing
        // sees and in the quarter behind it (D-718, D-746).
        foreach (StepDirection facing in StepDirections.All)
        {
            for (int y = 1; y < Room.Height - 1; y += 1)
            {
                for (int x = 1; x < Room.Width - 1; x += 1)
                {
                    TilePoint other = new(x, y);
                    bool ahead = MapSight.PatrolSees(Room, new TilePoint(5, 5), facing, 40, other) &&
                        MapSight.Reach(new TilePoint(5, 5), other) > MapSight.PatrolTouchRange;
                    Assert.False(ahead && MapSight.BehindFacing(new TilePoint(5, 5), facing, other));
                }
            }
        }
    }

    [Fact]
    public void AFacingThatNamesNoDirectionIsAnError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => MapSight.BehindFacing(new TilePoint(1, 1), (StepDirection)9, new TilePoint(2, 2)));
    }

    [Fact]
    public void TheReachOfTwoTilesIsTheLargerAxisDistance()
    {
        Assert.Equal(0, MapSight.Reach(new TilePoint(3, 3), new TilePoint(3, 3)));
        Assert.Equal(1, MapSight.Reach(new TilePoint(3, 3), new TilePoint(4, 4)));
        Assert.Equal(4, MapSight.Reach(new TilePoint(3, 3), new TilePoint(7, 5)));
        Assert.Equal(4, MapSight.Reach(new TilePoint(7, 5), new TilePoint(3, 3)));
    }
}
