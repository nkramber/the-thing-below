using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Light;
using TheThingBelow.Core.Maps;
using TheThingBelow.Tools.Content;
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

    /// <summary>
    /// Two rooms joined by a passage of two tiles in column 3. The walls at (2, 3) and (4, 3)
    /// stand beside the south end of the passage, and a floor lies to the south of each.
    /// </summary>
    private static readonly string[] Doorway =
    [
        "#######",
        "#.....#",
        "###.###",
        "###.###",
        "#.....#",
        "#######",
    ];

    private const int TilePixels = AtlasPages.TileSize * AtlasPages.TileSize;

    [Fact]
    public void AWallThatFacesAFloorToItsSouthLeavesItsFaceOutOfTheShape()
    {
        GameMap map = Map(LightFixtures.Room);

        WallShadow? shape = WallShadows.ShapeOf(map, new TilePoint(4, 0));

        // The south face shows the height of the wall: 24 pixels take light, and 8 stop it.
        Assert.Equal(new WallShadow(new TilePoint(4, 0), 0, 0, 32, 8, WallColumn.None), shape);
    }

    [Fact]
    public void AWallThatFacesAFloorToItsEastLeavesItsEastFaceOut()
    {
        GameMap map = Map(LightFixtures.Room);

        WallShadow? shape = WallShadows.ShapeOf(map, new TilePoint(0, 5));

        Assert.Equal(new WallShadow(new TilePoint(0, 5), 0, 0, 24, 32, WallColumn.None), shape);
    }

    [Fact]
    public void AWallOfOneTileKeepsAStripInItsMiddle()
    {
        // D-852: no light reaches a wall or a floor behind a wall of one tile.
        GameMap map = Map(TwoRooms);

        WallShadow? shape = WallShadows.ShapeOf(map, new TilePoint(4, 4));

        Assert.Equal(new WallShadow(new TilePoint(4, 4), 0, 15, 32, 17, WallColumn.None), shape);
    }

    [Fact]
    public void AWallAtTheCornerOfARoomTakesTheFullTile()
    {
        GameMap map = Map(LightFixtures.Room);

        WallShadow? shape = WallShadows.ShapeOf(map, new TilePoint(0, 0));

        Assert.Equal(new WallShadow(new TilePoint(0, 0), 0, 0, 32, 32, WallColumn.None), shape);
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

    [Fact]
    public void AWallBesideADoorwayTakesAnLDownToTheSouthEdgeOfTheTile()
    {
        GameMap map = Map(Doorway);

        WallShadow? west = WallShadows.ShapeOf(map, new TilePoint(2, 3));
        WallShadow? east = WallShadows.ShapeOf(map, new TilePoint(4, 3));

        // The band at the top stays, and a column of 2 pixels runs down at the inner edge of
        // the side face, so the front face and the side face keep their light (D-852).
        Assert.Equal(new WallShadow(new TilePoint(2, 3), 0, 0, 24, 8, WallColumn.East), west);
        Assert.Equal(new WallShadow(new TilePoint(4, 3), 8, 0, 32, 8, WallColumn.West), east);
        Assert.Equal([(0, 0), (24, 0), (24, 32), (22, 32), (22, 8), (0, 8)], west!.Outline());
        Assert.Equal([(8, 0), (32, 0), (32, 8), (10, 8), (10, 32), (8, 32)], east!.Outline());
    }

    [Fact]
    public void AWallWithAFloorToItsSouthAndOnBothSidesTakesNoColumn()
    {
        GameMap map = Map(["#####", "#...#", "#.#.#", "#...#", "#####"]);

        WallShadow? shape = WallShadows.ShapeOf(map, new TilePoint(2, 2));

        Assert.Equal(WallColumn.None, shape!.Column);
        Assert.Equal(4, shape.Outline().Count);
    }

    [Fact]
    public void AWallTorchBesideADoorwayLightsNoPixelOfThePassageAboveIt()
    {
        // Regression: a wall beside the doorway (6, 12) stopped the light in its top band
        // alone, and the torch at (4, 12) lit 249 pixels of the passage tile (6, 11).
        ContentSet set = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));
        GameMap map = set.Map(ContentId.Parse("map.fixture_dungeon", "test", "id"));
        IReadOnlyList<WallShadow> shapes = WallShadows.Of(map);
        MapLight torch = Assert.Single(
            set.Light.LightsOf(map.Id, TimeOfDay.Night),
            light => light.Id.Value == "piece.fixture_dungeon_pit_west");

        Assert.Equal(0, LitPixels(shapes, torch.X, torch.Y, new TilePoint(6, 11)));
        Assert.Equal(0, LitPixels(shapes, torch.X, torch.Y, new TilePoint(6, 10)));

        // A small glow stays in the doorway, and the front face of the wall beside it keeps
        // its light: 22 by 24 pixels, all but the column (D-852).
        Assert.InRange(LitPixels(shapes, torch.X, torch.Y, new TilePoint(6, 12)), 1, 128);
        Assert.Equal(22 * 24, LitPixels(shapes, torch.X, torch.Y, new TilePoint(5, 12)));
    }

    [Fact]
    public void TheCarriedLightInADoorwayLightsThePassageAboveIt()
    {
        // The lead stands in the doorway (6, 12), and its feet are on the south edge of the tile.
        ContentSet set = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));
        GameMap map = set.Map(ContentId.Parse("map.fixture_dungeon", "test", "id"));
        IReadOnlyList<WallShadow> shapes = WallShadows.Of(map);
        int x = (6 * AtlasPages.TileSize) + set.Light.Carried.X;
        int y = (13 * AtlasPages.TileSize) + set.Light.Carried.Y;

        Assert.Equal(TilePixels, LitPixels(shapes, x, y, new TilePoint(6, 11)));
        Assert.Equal(TilePixels, LitPixels(shapes, x, y, new TilePoint(6, 12)));
    }

    /// <summary>
    /// Counts the pixels of a tile whose middle takes a straight line from the light that
    /// crosses no edge of a wall shape. Each value doubles, so each pixel middle is a whole number.
    /// </summary>
    private static int LitPixels(IReadOnlyList<WallShadow> shapes, int lightX, int lightY, TilePoint tile)
    {
        const int Size = AtlasPages.TileSize;
        var light = (X: 2L * lightX, Y: 2L * lightY);
        int lit = 0;
        for (int row = 0; row < Size; row += 1)
        {
            for (int column = 0; column < Size; column += 1)
            {
                var pixel = (X: (2L * ((tile.X * Size) + column)) + 1, Y: (2L * ((tile.Y * Size) + row)) + 1);
                if (!CrossesAShape(shapes, light, pixel))
                {
                    lit += 1;
                }
            }
        }

        return lit;
    }

    /// <summary>Tells whether the line from one point to another crosses an edge of a wall shape, in doubled pixels.</summary>
    private static bool CrossesAShape(IReadOnlyList<WallShadow> shapes, (long X, long Y) from, (long X, long Y) to)
    {
        const int Size = AtlasPages.TileSize;
        foreach (WallShadow shape in shapes)
        {
            IReadOnlyList<(int X, int Y)> outline = shape.Outline();
            for (int index = 0; index < outline.Count; index += 1)
            {
                (int X, int Y) start = outline[index];
                (int X, int Y) end = outline[(index + 1) % outline.Count];
                var edgeStart = (X: 2L * ((shape.Tile.X * Size) + start.X), Y: 2L * ((shape.Tile.Y * Size) + start.Y));
                var edgeEnd = (X: 2L * ((shape.Tile.X * Size) + end.X), Y: 2L * ((shape.Tile.Y * Size) + end.Y));
                bool apart = Turn(from, to, edgeStart) != Turn(from, to, edgeEnd);
                bool across = Turn(edgeStart, edgeEnd, from) != Turn(edgeStart, edgeEnd, to);
                if (apart && across)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>Gives the side of the line from a to b that holds c: -1, 0, or 1.</summary>
    private static int Turn((long X, long Y) a, (long X, long Y) b, (long X, long Y) c)
    {
        return Math.Sign(((b.X - a.X) * (c.Y - a.Y)) - ((b.Y - a.Y) * (c.X - a.X)));
    }

    private static GameMap Map(string[] terrain)
    {
        SortedDictionary<string, GameMap> maps = LightFixtures.Maps(terrain);
        return maps[LightFixtures.MapId];
    }
}
