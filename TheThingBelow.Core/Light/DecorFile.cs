using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Light;

/// <summary>One decor piece of a map: its id, its kind, and its tile (D-844).</summary>
/// <param name="Id">The id of the piece, such as `piece.fixture_dungeon_hall_torch`. A light setup names it (D-843).</param>
/// <param name="Kind">The id of the decor kind, such as `decor.wall_torch`.</param>
/// <param name="Tile">The tile that holds the piece.</param>
public sealed record DecorPiece(ContentId Id, ContentId Kind, TilePoint Tile);

/// <summary>
/// The decor file of one map: each decor piece at its tile (D-844). No rule reads a piece, so
/// the file lies outside the rule folder, and a new or moved torch never changes the content
/// hash (D-495).
/// </summary>
/// <remarks>
/// The file points at the tiles of its map, as the edge file of D-501 does. The load checks
/// each tile against the map after every file is read (<see cref="LightContent"/>).
/// </remarks>
public sealed class DecorFile
{
    /// <summary>The folder of the decor files, under `content/`.</summary>
    public const string Folder = "decor/maps/";

    /// <summary>The kind of the id of each decor piece (D-646).</summary>
    public const string PieceKind = "piece";

    private DecorFile(string file, ContentId map, IReadOnlyList<DecorPiece> pieces)
    {
        this.File = file;
        this.Map = map;
        this.Pieces = pieces;
    }

    /// <summary>The path of the file, under `content/`, which every error names (T-2).</summary>
    public string File { get; }

    /// <summary>The id of the map that the pieces decorate, such as `map.fixture_dungeon`.</summary>
    public ContentId Map { get; }

    /// <summary>Every piece of the map, in the order of the file.</summary>
    public IReadOnlyList<DecorPiece> Pieces { get; }

    /// <summary>Tells whether a content path is a decor file.</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True when the path lies in the folder of the decor files.</returns>
    public static bool IsDecorFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return path.StartsWith(Folder, StringComparison.Ordinal);
    }

    /// <summary>Reads one decor file from its bytes.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The decor file.</returns>
    /// <exception cref="ContentException">
    /// The file breaks a rule of the reader, or two pieces take one id (G-6, T-2).
    /// </exception>
    public static DecorFile Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        DecorFile decor = Read(ref reader);
        reader.ReadFileEnd();
        return decor;
    }

    /// <summary>
    /// Refuses a piece that its map cannot hold: a tile outside the map, a tile that is not a
    /// wall, or a wall with no floor or doorway to its south (D-844).
    /// </summary>
    /// <param name="map">The map that <see cref="Map"/> names.</param>
    /// <exception cref="ContentException">A piece does not fit the map (T-2).</exception>
    /// <remarks>
    /// Each kind of PR-56 hangs on a wall and faces the tile to its south (<see cref="DecorKind"/>).
    /// A torch in the middle of a wall would light a place that no one can stand in.
    /// </remarks>
    public void RefuseWrongTile(GameMap map)
    {
        ArgumentNullException.ThrowIfNull(map);

        for (int index = 0; index < this.Pieces.Count; index += 1)
        {
            DecorPiece piece = this.Pieces[index];
            TilePoint tile = piece.Tile;
            TilePoint south = tile.Step(StepDirection.South);
            string field = $"pieces[{index}]";

            if (!map.Holds(tile))
            {
                throw ContentException.ForField(
                    this.File,
                    field,
                    $"the piece '{piece.Id.Value}' lies at {tile}, outside the map '{map.Id.Value}' of {map.Width} by {map.Height} tiles");
            }

            if (map.TileAt(tile) != TileKind.Wall)
            {
                throw ContentException.ForField(
                    this.File,
                    field,
                    $"the piece '{piece.Id.Value}' lies on a {TileKinds.NameOf(map.TileAt(tile))} tile at {tile}, and a decor piece hangs on a wall (D-844)");
            }

            if (!map.Holds(south) || !TileKinds.CanWalk(map.TileAt(south)))
            {
                throw ContentException.ForField(
                    this.File,
                    field,
                    $"the piece '{piece.Id.Value}' at {tile} has no floor or doorway to its south, and a decor piece faces a tile that the party can stand on (D-844)");
            }
        }
    }

    private static DecorFile Read(ref ContentReader reader)
    {
        string? comment = null;
        ContentId? map = null;
        List<DecorPiece>? pieces = null;

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
                case "pieces":
                    pieces = ReadPieces(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        List<DecorPiece> read = reader.Require(pieces, depth, "pieces");
        RefuseRepeatedId(ref reader, depth, read);
        return new DecorFile(reader.File, reader.Require(map, depth, "map"), read);
    }

    private static List<DecorPiece> ReadPieces(ref ContentReader reader)
    {
        var pieces = new List<DecorPiece>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, pieces.Count))
        {
            pieces.Add(ReadPiece(ref reader));
        }

        return pieces;
    }

    private static DecorPiece ReadPiece(ref ContentReader reader)
    {
        ContentId? id = null;
        ContentId? kind = null;
        int? x = null;
        int? y = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(PieceKind);
                    break;
                case "kind":
                    kind = reader.ReadContentId(DecorKind.IdKind);
                    break;
                case "x":
                    x = reader.ReadInt();
                    break;
                case "y":
                    y = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new DecorPiece(
            reader.Require(id, depth, "id"),
            reader.Require(kind, depth, "kind"),
            new TilePoint(reader.RequireInt(x, depth, "x"), reader.RequireInt(y, depth, "y")));
    }

    private static void RefuseRepeatedId(ref ContentReader reader, int depth, List<DecorPiece> pieces)
    {
        var seen = new SortedSet<string>(StringComparer.Ordinal);
        for (int index = 0; index < pieces.Count; index += 1)
        {
            if (!seen.Add(pieces[index].Id.Value))
            {
                throw reader.RefuseField(
                    depth,
                    $"pieces[{index}].id",
                    $"the id '{pieces[index].Id.Value}' names two pieces, and one id names one piece (D-166, D-843)");
            }
        }
    }
}
