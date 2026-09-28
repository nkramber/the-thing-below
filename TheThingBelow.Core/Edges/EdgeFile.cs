using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Edges;

/// <summary>One tile of a map with its edge pieces, in the order of the draw (D-1321, D-1326).</summary>
/// <param name="At">The tile.</param>
/// <param name="Pieces">The id of each piece of the tile: 1 to <see cref="EdgeFile.MostPiecesOnTile"/>.</param>
public sealed record EdgeTile(TilePoint At, IReadOnlyList<ContentId> Pieces);

/// <summary>
/// The edge file of one map: each tile that holds an edge piece, with its pieces (D-501, D-1326).
/// The `edges` command of Tools writes it from the map and the edge rules, and the repository
/// commits it. No rule reads it, so a new edge piece never changes the content hash (D-495).
/// </summary>
/// <remarks>
/// The tiles come in the order of the rows, then of the columns, as the command writes them. A
/// test of Tests compares each committed file with the output of the command (D-501).
/// </remarks>
public sealed class EdgeFile
{
    /// <summary>The folder of the edge files, under `content/` (D-1327).</summary>
    public const string Folder = "edges/maps/";

    /// <summary>
    /// The most pieces on one tile. An inner corner draws only where both of its sides draw
    /// nothing, so 4 sides, or 4 corners, is the most (D-1321). Game draws one layer for each.
    /// </summary>
    public const int MostPiecesOnTile = 4;

    private EdgeFile(string file, ContentId map, IReadOnlyList<EdgeTile> tiles)
    {
        this.File = file;
        this.Map = map;
        this.Tiles = tiles;
    }

    /// <summary>The path of the file, under `content/`, which every error names (T-2).</summary>
    public string File { get; }

    /// <summary>The id of the map, such as `map.overworld`.</summary>
    public ContentId Map { get; }

    /// <summary>Every tile with an edge piece, in the order of the rows, then of the columns.</summary>
    public IReadOnlyList<EdgeTile> Tiles { get; }

    /// <summary>Tells whether a content path is an edge file.</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True when the path lies in the folder of the edge files.</returns>
    public static bool IsEdgeFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return path.StartsWith(Folder, StringComparison.Ordinal);
    }

    /// <summary>Reads one edge file from its bytes.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The edge file.</returns>
    /// <exception cref="ContentException">
    /// The file breaks a rule of the reader, a tile holds no piece or too many, or a tile comes
    /// out of the order of the rows and the columns (G-6, T-2).
    /// </exception>
    public static EdgeFile Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        EdgeFile edges = Read(ref reader);
        reader.ReadFileEnd();
        return edges;
    }

    private static EdgeFile Read(ref ContentReader reader)
    {
        string? comment = null;
        ContentId? map = null;
        List<EdgeTile>? tiles = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "map":
                    map = reader.ReadContentId(GameMap.IdKind);
                    break;
                case "tiles":
                    tiles = ReadTiles(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        return new EdgeFile(reader.File, reader.Require(map, depth, "map"), reader.Require(tiles, depth, "tiles"));
    }

    private static List<EdgeTile> ReadTiles(ref ContentReader reader)
    {
        var tiles = new List<EdgeTile>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, tiles.Count))
        {
            EdgeTile tile = ReadTile(ref reader);

            // One order keeps one text for one map, and it refuses a tile that comes two times.
            if (tiles.Count > 0 && !Follows(tiles[^1].At, tile.At))
            {
                throw reader.Refuse(
                    $"the tile {tile.At} comes after the tile {tiles[^1].At}, and the tiles come in the order of the rows, then of the columns, one time each (D-1326)");
            }

            tiles.Add(tile);
        }

        return tiles;
    }

    private static EdgeTile ReadTile(ref ContentReader reader)
    {
        int? x = null;
        int? y = null;
        List<ContentId>? pieces = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "x":
                    x = reader.ReadInt();
                    break;
                case "y":
                    y = reader.ReadInt();
                    break;
                case "pieces":
                    pieces = ReadPieces(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        List<ContentId> read = reader.Require(pieces, depth, "pieces");
        if (read.Count == 0 || read.Count > MostPiecesOnTile)
        {
            throw reader.RefuseField(
                depth,
                "pieces",
                $"the tile holds {read.Count} pieces, and a tile of an edge file holds 1 to {MostPiecesOnTile} (D-1321)");
        }

        return new EdgeTile(new TilePoint(reader.RequireInt(x, depth, "x"), reader.RequireInt(y, depth, "y")), read);
    }

    private static List<ContentId> ReadPieces(ref ContentReader reader)
    {
        var pieces = new List<ContentId>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, pieces.Count))
        {
            pieces.Add(reader.ReadContentId(EdgeRule.PieceKind));
        }

        return pieces;
    }

    private static bool Follows(TilePoint before, TilePoint after) =>
        after.Y > before.Y || (after.Y == before.Y && after.X > before.X);
}
