using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Edges;

/// <summary>
/// The edge rule of one tile kind: the kinds that join it with no edge, and its 8 edge pieces
/// (D-1321, D-1322). No rule of the simulation reads it, so the file lies outside the rule folder
/// and the content hash never reads it (D-495, D-501).
/// </summary>
/// <remarks>
/// A kind always joins itself, and a neighbour outside the map joins every kind (D-1324). The
/// rule names the id of each piece, and a drawing file names the same id with the use `map`
/// (D-519). A rule with an absent piece fails the load, so each pattern of the 8 neighbours has
/// its pieces (D-1321).
/// </remarks>
public sealed class EdgeRule
{
    /// <summary>The folder of the edge rules, under `content/` (D-1327).</summary>
    public const string Folder = "edges/kinds/";

    /// <summary>The kind of the id of each edge piece (D-646).</summary>
    public const string PieceKind = "edge";

    private readonly SortedDictionary<EdgePlace, ContentId> pieces;

    private EdgeRule(string file, TileKind kind, IReadOnlyList<TileKind> joins, SortedDictionary<EdgePlace, ContentId> pieces)
    {
        this.File = file;
        this.Kind = kind;
        this.Joins = joins;
        this.pieces = pieces;
    }

    /// <summary>The path of the file, under `content/`, which every error names (T-2).</summary>
    public string File { get; }

    /// <summary>The kind whose tiles carry the edge pieces of this rule (D-1322).</summary>
    public TileKind Kind { get; }

    /// <summary>Each other kind that joins <see cref="Kind"/> with no edge, in the order of the file (D-1325).</summary>
    public IReadOnlyList<TileKind> Joins { get; }

    /// <summary>Every piece of the rule, in the order of <see cref="EdgePlaces.All"/>.</summary>
    public IEnumerable<ContentId> Pieces => this.pieces.Values;

    /// <summary>Tells whether a content path is an edge rule.</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True when the path lies in the folder of the edge rules.</returns>
    public static bool IsRuleFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return path.StartsWith(Folder, StringComparison.Ordinal);
    }

    /// <summary>Reads one edge rule from its bytes.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The edge rule.</returns>
    /// <exception cref="ContentException">
    /// The file breaks a rule of the reader, names an unknown kind, names a join two times or
    /// its own kind as a join, or lacks a piece (G-6, T-2).
    /// </exception>
    public static EdgeRule Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        EdgeRule rule = Read(ref reader);
        reader.ReadFileEnd();
        return rule;
    }

    /// <summary>Tells whether a tile of one kind joins a tile of this kind with no edge (D-1322).</summary>
    /// <param name="neighbour">The kind of the neighbour tile.</param>
    /// <returns>True for <see cref="Kind"/> itself and for each kind of <see cref="Joins"/>.</returns>
    public bool Joined(TileKind neighbour)
    {
        if (neighbour == this.Kind)
        {
            return true;
        }

        foreach (TileKind join in this.Joins)
        {
            if (join == neighbour)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Gives the piece of one place.</summary>
    /// <param name="place">The place on the tile.</param>
    /// <returns>The content id of the piece, such as `edge.water_north`.</returns>
    public ContentId PieceOf(EdgePlace place) => this.pieces[place];

    private static EdgeRule Read(ref ContentReader reader)
    {
        string? comment = null;
        TileKind? kind = null;
        List<TileKind>? joins = null;
        SortedDictionary<EdgePlace, ContentId>? pieces = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "kind":
                    kind = ReadKind(ref reader);
                    break;
                case "joins":
                    joins = ReadJoins(ref reader);
                    break;
                case "pieces":
                    pieces = ReadPieces(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        TileKind readKind = reader.RequireValue(kind, depth, "kind");
        List<TileKind> readJoins = reader.Require(joins, depth, "joins");
        for (int index = 0; index < readJoins.Count; index += 1)
        {
            if (readJoins[index] == readKind)
            {
                throw reader.RefuseField(
                    depth,
                    $"joins[{index}]",
                    $"the rule of '{TileKinds.NameOf(readKind)}' names its own kind as a join, and a kind always joins itself (D-1322)");
            }
        }

        return new EdgeRule(reader.File, readKind, readJoins, reader.Require(pieces, depth, "pieces"));
    }

    private static TileKind ReadKind(ref ContentReader reader)
    {
        string name = reader.ReadString();
        return TileKinds.TryOfName(name, out TileKind kind)
            ? kind
            : throw reader.Refuse($"'{name}' names no tile kind (D-528)");
    }

    private static List<TileKind> ReadJoins(ref ContentReader reader)
    {
        var joins = new List<TileKind>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, joins.Count))
        {
            TileKind join = ReadKind(ref reader);
            if (joins.Contains(join))
            {
                throw reader.Refuse($"the join '{TileKinds.NameOf(join)}' comes two times, and a rule names each join one time");
            }

            joins.Add(join);
        }

        return joins;
    }

    private static SortedDictionary<EdgePlace, ContentId> ReadPieces(ref ContentReader reader)
    {
        var pieces = new SortedDictionary<EdgePlace, ContentId>();
        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            EdgePlace? place = PlaceOf(field);
            if (place is null)
            {
                throw reader.UnknownField(field);
            }

            pieces.Add(place.Value, reader.ReadContentId(PieceKind));
        }

        // Each pattern of the 8 neighbours needs its pieces, so every place takes one (D-1321).
        foreach (EdgePlace place in EdgePlaces.All)
        {
            if (!pieces.ContainsKey(place))
            {
                throw reader.RefuseField(depth, EdgePlaces.NameOf(place), "the rule has no piece for this place, and each rule has 8 pieces (D-1321)");
            }
        }

        // One piece at two places would give a tile the same piece two times, and the tile set
        // one tile two times (D-1321, T-2).
        var seen = new SortedDictionary<string, EdgePlace>(StringComparer.Ordinal);
        foreach (EdgePlace place in EdgePlaces.All)
        {
            if (!seen.TryAdd(pieces[place].Value, place))
            {
                throw reader.RefuseField(
                    depth,
                    EdgePlaces.NameOf(place),
                    $"'{pieces[place].Value}' is also the piece of the place '{EdgePlaces.NameOf(seen[pieces[place].Value])}', and each place of a rule has its own piece (D-1321)");
            }
        }

        return pieces;
    }

    private static EdgePlace? PlaceOf(string name)
    {
        foreach (EdgePlace place in EdgePlaces.All)
        {
            if (string.CompareOrdinal(EdgePlaces.NameOf(place), name) == 0)
            {
                return place;
            }
        }

        return null;
    }
}
