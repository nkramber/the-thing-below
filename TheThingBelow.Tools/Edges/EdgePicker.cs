using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Edges;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Tools.Edges;

/// <summary>
/// The pick of the edge pieces of one map from its terrain and the edge rules (D-204, D-1321),
/// and the text of its edge file (D-1326).
/// </summary>
/// <remarks>
/// A tile of a kind with an edge rule takes a side piece where the neighbour on that side does
/// not join its kind. It takes an inner corner piece where the two sides of that corner join and
/// the diagonal neighbour does not. A tile of a kind with no rule takes no piece. A neighbour
/// outside the map joins every kind (D-1322, D-1324).
/// </remarks>
public static class EdgePicker
{
    /// <summary>Gives each tile of a map that takes an edge piece, in the order of the rows, then of the columns.</summary>
    /// <param name="map">The map.</param>
    /// <param name="rules">Each edge rule, by its kind.</param>
    /// <returns>Each tile with its pieces, in the order of <see cref="EdgePlaces.All"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static List<EdgeTile> Pick(GameMap map, IReadOnlyDictionary<TileKind, EdgeRule> rules)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(rules);

        var tiles = new List<EdgeTile>();
        for (int row = 0; row < map.Height; row += 1)
        {
            for (int column = 0; column < map.Width; column += 1)
            {
                var at = new TilePoint(column, row);
                if (!rules.TryGetValue(map.TileAt(at), out EdgeRule? rule))
                {
                    continue;
                }

                List<ContentId> pieces = PiecesOf(map, rule, at);
                if (pieces.Count > 0)
                {
                    tiles.Add(new EdgeTile(at, pieces));
                }
            }
        }

        return tiles;
    }

    /// <summary>Gives the text of the edge file of one map, as the repository commits it (D-1326).</summary>
    /// <param name="map">The id of the map.</param>
    /// <param name="tiles">The tiles of <see cref="Pick"/>.</param>
    /// <returns>The text, with one tile on each line and a line end after the last line.</returns>
    public static string TextOf(ContentId map, IReadOnlyList<EdgeTile> tiles)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(tiles);

        var text = new StringBuilder();
        text.Append("{\n");
        text.Append($" \"comment\": \"The edge pieces of the map '{map.Value}' (D-501, D-1326). The `edges` command of Tools writes this file from the map and the edge rules, and a test compares it with the output of the command. Run `make edges` after a change of the map or of an edge rule.\",\n");
        text.Append($" \"map\": \"{map.Value}\",\n");
        if (tiles.Count == 0)
        {
            text.Append(" \"tiles\": []\n");
        }
        else
        {
            text.Append(" \"tiles\": [\n");
            for (int index = 0; index < tiles.Count; index += 1)
            {
                EdgeTile tile = tiles[index];
                string pieces = string.Join(", ", PieceTexts(tile.Pieces));
                string end = index == tiles.Count - 1 ? string.Empty : ",";
                text.Append($"  {{ \"x\": {tile.At.X}, \"y\": {tile.At.Y}, \"pieces\": [{pieces}] }}{end}\n");
            }

            text.Append(" ]\n");
        }

        text.Append("}\n");
        return text.ToString();
    }

    private static List<ContentId> PiecesOf(GameMap map, EdgeRule rule, TilePoint at)
    {
        bool north = Joins(map, rule, at, 0, -1);
        bool east = Joins(map, rule, at, 1, 0);
        bool south = Joins(map, rule, at, 0, 1);
        bool west = Joins(map, rule, at, -1, 0);

        // The order follows EdgePlaces.All, which is the order of the draw (D-1321).
        var pieces = new List<ContentId>();
        AddWhen(pieces, rule, EdgePlace.North, !north);
        AddWhen(pieces, rule, EdgePlace.East, !east);
        AddWhen(pieces, rule, EdgePlace.South, !south);
        AddWhen(pieces, rule, EdgePlace.West, !west);
        AddWhen(pieces, rule, EdgePlace.NorthEast, north && east && !Joins(map, rule, at, 1, -1));
        AddWhen(pieces, rule, EdgePlace.SouthEast, south && east && !Joins(map, rule, at, 1, 1));
        AddWhen(pieces, rule, EdgePlace.SouthWest, south && west && !Joins(map, rule, at, -1, 1));
        AddWhen(pieces, rule, EdgePlace.NorthWest, north && west && !Joins(map, rule, at, -1, -1));
        return pieces;
    }

    /// <summary>Tells whether the neighbour at one offset joins the kind of the rule. A tile outside the map joins (D-1324).</summary>
    private static bool Joins(GameMap map, EdgeRule rule, TilePoint at, int right, int down)
    {
        var neighbour = new TilePoint(at.X + right, at.Y + down);
        return !map.Holds(neighbour) || rule.Joined(map.TileAt(neighbour));
    }

    private static void AddWhen(List<ContentId> pieces, EdgeRule rule, EdgePlace place, bool draws)
    {
        if (draws)
        {
            pieces.Add(rule.PieceOf(place));
        }
    }

    private static IEnumerable<string> PieceTexts(IReadOnlyList<ContentId> pieces)
    {
        foreach (ContentId piece in pieces)
        {
            yield return $"\"{piece.Value}\"";
        }
    }
}
