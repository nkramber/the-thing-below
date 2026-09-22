using System.Collections.Generic;
using TheThingBelow.Core.Light;
using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The shadow shape of each wall, from the terrain alone (D-845, D-852).</summary>
public sealed class WallShadowsTests
{
    /// <summary>
    /// Two rooms split by a wall of one tile in row 4, with a thick wall mass around them. The
    /// spawn point of the fixture map stands at (1, 1).
    /// </summary>
    private static readonly string[] TwoRooms =
    [
        "##########",
        "#........#",
        "#........#",
        "#........#",
        "##########",
        "#........#",
        "#........#",
        "##########",
        "##########",
    ];

    [Fact]
    public void AWallThatFacesAFloorToItsSouthLeavesItsFaceOutOfTheShape()
    {
        GameMap map = Map(LightFixtures.Room);

        WallShadow? shape = WallShadows.ShapeOf(map, new TilePoint(4, 0));

        // The south face shows the height of the wall: 24 pixels take light, and 8 stop it.
        Assert.Equal(new WallShadow(new TilePoint(4, 0), 0, 0, 32, 8), shape);
    }

    [Fact]
    public void AWallThatFacesAFloorToItsEastLeavesItsEastFaceOut()
    {
        GameMap map = Map(LightFixtures.Room);

        WallShadow? shape = WallShadows.ShapeOf(map, new TilePoint(0, 5));

        Assert.Equal(new WallShadow(new TilePoint(0, 5), 0, 0, 24, 32), shape);
    }

    [Fact]
    public void AWallOfOneTileKeepsAStripInItsMiddle()
    {
        // D-852: no light reaches a wall or a floor behind a wall of one tile.
        GameMap map = Map(TwoRooms);

        WallShadow? shape = WallShadows.ShapeOf(map, new TilePoint(4, 4));

        Assert.Equal(new WallShadow(new TilePoint(4, 4), 0, 15, 32, 17), shape);
    }

    [Fact]
    public void AWallAtTheCornerOfARoomTakesTheFullTile()
    {
        GameMap map = Map(LightFixtures.Room);

        WallShadow? shape = WallShadows.ShapeOf(map, new TilePoint(0, 0));

        Assert.Equal(new WallShadow(new TilePoint(0, 0), 0, 0, 32, 32), shape);
    }

    [Fact]
    public void AWallInsideAWallMassTakesNoShape()
    {
        GameMap map = Map(TwoRooms);

        Assert.Null(WallShadows.ShapeOf(map, new TilePoint(4, 8)));
    }

    [Fact]
    public void EachRowOfAWallOfOneTileStopsEveryRayThatCrossesIt()
    {
        // D-852: a ray from a light in the north room to any point of the south room crosses
        // row 4. Each wall of row 4 holds a strip across its whole width, and the strips meet,
        // so the row blocks the light from edge to edge.
        GameMap map = Map(TwoRooms);
        IReadOnlyList<WallShadow> shapes = WallShadows.Of(map);

        int covered = 0;
        foreach (WallShadow shape in shapes)
        {
            if (shape.Tile.Y == 4)
            {
                Assert.True(shape.Top <= 15 && shape.Bottom >= 17, $"the wall at {shape.Tile} leaves a gap across row 4");
                Assert.True(shape.Left == 0 && shape.Right == 32, $"the wall at {shape.Tile} leaves a gap at its side");
                covered += 1;
            }
        }

        Assert.Equal(map.Width, covered);
    }

    private static GameMap Map(string[] terrain)
    {
        SortedDictionary<string, GameMap> maps = LightFixtures.Maps(terrain);
        return maps[LightFixtures.MapId];
    }
}
