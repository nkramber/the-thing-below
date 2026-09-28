using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Edges;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Streams;
using TheThingBelow.Tools.Edges;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The pick of the edge pieces of a map (D-204, D-1321, D-1322, D-1324, D-1325).</summary>
public sealed class EdgePickerTests
{
    [Fact]
    public void AWaterTileAloneInGrassTakesEachSidePiece()
    {
        List<EdgeTile> tiles = Pick(",,,", ",-,", ",,,");

        EdgeTile tile = Assert.Single(tiles);
        Assert.Equal(new TilePoint(1, 1), tile.At);
        Assert.Equal(["edge.water_north", "edge.water_east", "edge.water_south", "edge.water_west"], Values(tile));
    }

    [Fact]
    public void AnInnerCornerDrawsWhereBothSidesJoinAndTheDiagonalDoesNot()
    {
        // The water fills each tile but the north-east one, so the tile at (1, 1) sees water on
        // its north and its east, and grass on its north-east.
        List<EdgeTile> tiles = Pick("--,", "---", "---");

        Assert.Equal(["edge.water_north_east"], Values(TileAt(tiles, 1, 1)));
        Assert.Equal(["edge.water_north"], Values(TileAt(tiles, 2, 1)));
        Assert.Equal(["edge.water_east"], Values(TileAt(tiles, 1, 0)));
    }

    [Fact]
    public void ATileTakesFourInnerCornersAtMost()
    {
        List<EdgeTile> tiles = Pick(",-,", "---", ",-,");

        Assert.Equal(
            ["edge.water_north_east", "edge.water_south_east", "edge.water_south_west", "edge.water_north_west"],
            Values(TileAt(tiles, 1, 1)));
    }

    [Fact]
    public void ANeighbourOutsideTheMapJoinsEveryKind()
    {
        // D-1324: water that runs off the map draws no edge along the border. The lake touches
        // the north, the west, and the south border, and grass lies to its east.
        List<EdgeTile> tiles = Pick("--,", "--,");

        Assert.Equal(["edge.water_east"], Values(TileAt(tiles, 1, 0)));
        Assert.Equal(["edge.water_east"], Values(TileAt(tiles, 1, 1)));
        Assert.Equal(2, tiles.Count);
    }

    [Fact]
    public void TheBridgeJoinsTheWaterAndTheGorgeJoinsNothing()
    {
        // D-1325: the water runs on under the bridge, and the gorge shows its lip at the bridge.
        List<EdgeTile> tiles = Pick("-H-,", ":H:,");

        Assert.Equal(["edge.water_south"], Values(TileAt(tiles, 0, 0)));
        Assert.Equal(["edge.gorge_north", "edge.gorge_east"], Values(TileAt(tiles, 0, 1)));
    }

    [Fact]
    public void AKindWithNoRuleTakesNoPiece()
    {
        List<EdgeTile> tiles = Pick(",,,", ",^,", ",,,");

        Assert.Empty(tiles);
    }

    [Fact]
    public void TheTilesComeInTheOrderOfTheRowsThenOfTheColumns()
    {
        List<EdgeTile> tiles = Pick(",-,-", "-,,,");

        Assert.Equal([new TilePoint(1, 0), new TilePoint(3, 0), new TilePoint(0, 1)], [tiles[0].At, tiles[1].At, tiles[2].At]);
    }

    [Fact]
    public void EachPieceFollowsItsNeighboursOnRandomMaps()
    {
        // A seed loop over maps of random terrain (T-3). Each side piece draws where that side
        // does not join, each inner corner where both sides join and the diagonal does not, and a
        // tile holds 4 pieces at most (D-1321).
        const string Kinds = "-:,H^";
        SortedDictionary<TileKind, EdgeRule> rules = EdgeFixtures.Rules();
        for (ulong seed = 1; seed <= 200; seed += 1)
        {
            Pcg32 random = Pcg32.FromSeed(seed, 53);
            var rows = new string[6];
            for (int row = 0; row < rows.Length; row += 1)
            {
                var line = new StringBuilder();
                for (int column = 0; column < 7; column += 1)
                {
                    line.Append(Kinds[(int)(random.Next() % (uint)Kinds.Length)]);
                }

                rows[row] = line.ToString();
            }

            // The spawn point needs one tile that the party can walk.
            rows[0] = "," + rows[0][1..];
            GameMap map = EdgeFixtures.MapOf(rows);
            var picked = new SortedDictionary<int, List<string>>();
            foreach (EdgeTile tile in EdgePicker.Pick(map, rules))
            {
                Assert.True(tile.Pieces.Count is >= 1 and <= EdgeFile.MostPiecesOnTile, $"Seed {seed}: the tile {tile.At} takes {tile.Pieces.Count} pieces.");
                picked.Add((tile.At.Y * map.Width) + tile.At.X, Values(tile));
            }

            for (int row = 0; row < map.Height; row += 1)
            {
                for (int column = 0; column < map.Width; column += 1)
                {
                    var at = new TilePoint(column, row);
                    List<string> wanted = Wanted(map, rules, at);
                    List<string> got = picked.TryGetValue((row * map.Width) + column, out List<string>? found) ? found : [];
                    Assert.True(
                        string.Join(" ", wanted) == string.Join(" ", got),
                        $"Seed {seed}: the tile {at} of '{string.Join("/", rows)}' takes [{string.Join(" ", got)}], and its neighbours give [{string.Join(" ", wanted)}].");
                }
            }
        }
    }

    [Fact]
    public void TheTextOfAnEdgeFileReadsBackAsTheSameTiles()
    {
        GameMap map = EdgeFixtures.MapOf(",,,,", ",--,", ",,:,");
        List<EdgeTile> tiles = EdgePicker.Pick(map, EdgeFixtures.Rules());

        EdgeFile file = EdgeFile.Read(Encoding.UTF8.GetBytes(EdgePicker.TextOf(map.Id, tiles)), EdgeFixtures.EdgePath);

        Assert.Equal(EdgeFixtures.MapId, file.Map.Value);
        Assert.Equal(tiles.Count, file.Tiles.Count);
        for (int index = 0; index < tiles.Count; index += 1)
        {
            Assert.Equal(tiles[index].At, file.Tiles[index].At);
            Assert.Equal(Values(tiles[index]), Values(file.Tiles[index]));
        }
    }

    [Fact]
    public void TheTextOfAMapWithNoEdgeHoldsAnEmptyList()
    {
        string text = EdgePicker.TextOf(ContentId.Parse(EdgeFixtures.MapId, "test", "map"), []);

        Assert.Contains(" \"tiles\": []\n}\n", text, StringComparison.Ordinal);
        Assert.Empty(EdgeFixtures.File(text).Tiles);
    }

    private static List<EdgeTile> Pick(params string[] rows) => EdgePicker.Pick(EdgeFixtures.MapOf(rows), EdgeFixtures.Rules());

    private static EdgeTile TileAt(List<EdgeTile> tiles, int x, int y) =>
        tiles.Find(tile => tile.At == new TilePoint(x, y)) ?? throw new InvalidOperationException($"The pick gave no tile at ({x}, {y}).");

    private static List<string> Values(EdgeTile tile)
    {
        var values = new List<string>();
        foreach (ContentId piece in tile.Pieces)
        {
            values.Add(piece.Value);
        }

        return values;
    }

    /// <summary>Gives the pieces of one tile from the words of D-1321, place by place.</summary>
    private static List<string> Wanted(GameMap map, SortedDictionary<TileKind, EdgeRule> rules, TilePoint at)
    {
        var wanted = new List<string>();
        if (!rules.TryGetValue(map.TileAt(at), out EdgeRule? rule))
        {
            return wanted;
        }

        foreach (EdgePlace place in EdgePlaces.All)
        {
            (int right, int down, bool corner) = place switch
            {
                EdgePlace.North => (0, -1, false),
                EdgePlace.East => (1, 0, false),
                EdgePlace.South => (0, 1, false),
                EdgePlace.West => (-1, 0, false),
                EdgePlace.NorthEast => (1, -1, true),
                EdgePlace.SouthEast => (1, 1, true),
                EdgePlace.SouthWest => (-1, 1, true),
                _ => (-1, -1, true),
            };

            bool draws = corner
                ? Joins(map, rule, at, right, 0) && Joins(map, rule, at, 0, down) && !Joins(map, rule, at, right, down)
                : !Joins(map, rule, at, right, down);
            if (draws)
            {
                wanted.Add(rule.PieceOf(place).Value);
            }
        }

        return wanted;
    }

    private static bool Joins(GameMap map, EdgeRule rule, TilePoint at, int right, int down)
    {
        var neighbour = new TilePoint(at.X + right, at.Y + down);
        return !map.Holds(neighbour) || rule.Joined(map.TileAt(neighbour));
    }
}
