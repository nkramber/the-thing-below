using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Edges;

/// <summary>
/// Every edge rule and every edge file of one build, read and checked across files (D-501,
/// D-1321 to D-1327). Game and the map preview read it, and no rule of the simulation does, so
/// the content hash never reads an edge file (D-495).
/// </summary>
/// <remarks>
/// The load checks each file against the maps and the atlas:
/// <list type="bullet">
/// <item>Each map has one edge file, and each edge file names a map that exists.</item>
/// <item>Each kind has one edge rule at most.</item>
/// <item>Each tile of an edge file lies inside its map, on a kind with an edge rule, and each of its pieces comes from that rule.</item>
/// <item>The atlas draws each piece of each rule on the tile page, as a tile.</item>
/// </list>
/// The load does not pick the pieces again. The `edges` command of Tools picks them, and a test
/// compares each committed file with its output, so a stale file fails that test (D-501).
/// </remarks>
public sealed class EdgeContent
{
    private readonly SortedDictionary<TileKind, EdgeRule> rules;

    private readonly SortedDictionary<string, EdgeFile> files;

    private EdgeContent(SortedDictionary<TileKind, EdgeRule> rules, SortedDictionary<string, EdgeFile> files)
    {
        this.rules = rules;
        this.files = files;
    }

    /// <summary>Every edge rule, in the order of its kind.</summary>
    public IEnumerable<EdgeRule> Rules => this.rules.Values;

    /// <summary>Tells whether a content path is an edge rule or an edge file.</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True when the path lies in the folder of the edge rules or of the edge files.</returns>
    public static bool IsEdgeContent(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return EdgeRule.IsRuleFile(path) || EdgeFile.IsEdgeFile(path);
    }

    /// <summary>Gives the edge file of one map.</summary>
    /// <param name="map">The id of the map.</param>
    /// <returns>The edge file.</returns>
    /// <exception cref="ContentException">No edge file names the map (T-2).</exception>
    public EdgeFile EdgesOf(ContentId map)
    {
        ArgumentNullException.ThrowIfNull(map);

        return this.files.TryGetValue(map.Value, out EdgeFile? found)
            ? found
            : throw ContentException.ForFile(EdgeFile.Folder, $"no edge file names the map '{map.Value}' (D-501)");
    }

    /// <summary>Reads every edge rule and every edge file, and checks each one against the maps and the atlas.</summary>
    /// <param name="files">The edge files and rules of the content set, which <see cref="IsEdgeContent"/> picked.</param>
    /// <param name="maps">Every map, by the value of its id.</param>
    /// <param name="atlas">The atlas index, for the drawing of each piece (D-519).</param>
    /// <returns>The edge content.</returns>
    /// <exception cref="ContentException">A file breaks a rule of its reader, or a check across files fails (T-2).</exception>
    public static EdgeContent Load(IReadOnlyList<ContentFile> files, SortedDictionary<string, GameMap> maps, AtlasIndex atlas)
    {
        ArgumentNullException.ThrowIfNull(files);
        ArgumentNullException.ThrowIfNull(maps);
        ArgumentNullException.ThrowIfNull(atlas);

        var rules = new SortedDictionary<TileKind, EdgeRule>();
        var edgeFiles = new SortedDictionary<string, EdgeFile>(StringComparer.Ordinal);
        foreach (ContentFile file in files)
        {
            if (EdgeRule.IsRuleFile(file.Path))
            {
                EdgeRule rule = EdgeRule.Read(file.Bytes, file.Path);
                if (!rules.TryAdd(rule.Kind, rule))
                {
                    throw ContentException.ForField(file.Path, "kind", $"'{TileKinds.NameOf(rule.Kind)}': a second edge rule names this kind, and a kind has one (D-1327)");
                }
            }
            else if (EdgeFile.IsEdgeFile(file.Path))
            {
                EdgeFile read = EdgeFile.Read(file.Bytes, file.Path);
                if (!edgeFiles.TryAdd(read.Map.Value, read))
                {
                    throw ContentException.ForField(file.Path, "map", $"'{read.Map.Value}': a second edge file names this map, and a map has one (D-501)");
                }
            }
            else
            {
                throw ContentException.ForFile(file.Path, "the file is not an edge file or an edge rule, and the content set gave it to the edge reader");
            }
        }

        var content = new EdgeContent(rules, edgeFiles);
        content.RefuseAbsentDrawing(atlas);
        content.RefuseWrongFile(maps);
        return content;
    }

    private void RefuseAbsentDrawing(AtlasIndex atlas)
    {
        string tilePage = AtlasPages.NameOf(AtlasPageKind.Tiles);
        foreach (EdgeRule rule in this.rules.Values)
        {
            foreach (EdgePlace place in EdgePlaces.All)
            {
                ContentId piece = rule.PieceOf(place);
                string field = $"pieces.{EdgePlaces.NameOf(place)}";

                // Game draws each piece on a layer of tiles over the ground, so each piece is a tile (D-667).
                if (!atlas.Draws(piece, TileIds.MapUse))
                {
                    throw ContentException.ForField(rule.File, field, $"no drawing draws '{piece.Value}' for the use '{TileIds.MapUse}', and Game draws each edge piece (D-519)");
                }

                AtlasEntry entry = atlas.Entry(piece, TileIds.MapUse);
                if (string.CompareOrdinal(entry.Page, tilePage) != 0 || entry.Width != AtlasPages.TileSize || entry.Height != AtlasPages.TileSize)
                {
                    throw ContentException.ForField(
                        rule.File,
                        field,
                        $"the drawing of '{piece.Value}' is {entry.Width} by {entry.Height} pixels on the page '{entry.Page}', and an edge piece is a tile of {AtlasPages.TileSize} by {AtlasPages.TileSize} on the page '{tilePage}' (D-667)");
                }
            }
        }
    }

    private void RefuseWrongFile(SortedDictionary<string, GameMap> maps)
    {
        foreach (EdgeFile file in this.files.Values)
        {
            if (!maps.TryGetValue(file.Map.Value, out GameMap? map))
            {
                throw ContentException.ForField(file.File, "map", $"no map file holds the id '{file.Map.Value}' (D-528)");
            }

            for (int index = 0; index < file.Tiles.Count; index += 1)
            {
                this.RefuseWrongTile(file, map, file.Tiles[index], $"tiles[{index}]");
            }
        }

        foreach (GameMap map in maps.Values)
        {
            if (!this.files.ContainsKey(map.Id.Value))
            {
                throw ContentException.ForFile(map.File, $"no file of `{EdgeFile.Folder}` names the map '{map.Id.Value}', and each map has one (D-501)");
            }
        }
    }

    private void RefuseWrongTile(EdgeFile file, GameMap map, EdgeTile tile, string field)
    {
        if (!map.Holds(tile.At))
        {
            throw ContentException.ForField(file.File, field, $"the tile {tile.At} lies outside the map '{map.Id.Value}' of {map.Width} by {map.Height} tiles");
        }

        TileKind kind = map.TileAt(tile.At);
        if (!this.rules.TryGetValue(kind, out EdgeRule? rule))
        {
            throw ContentException.ForField(
                file.File,
                field,
                $"the tile {tile.At} is a {TileKinds.NameOf(kind)} tile, and no edge rule names that kind. Run the `edges` command again (D-501, D-1322)");
        }

        var seen = new SortedSet<string>(StringComparer.Ordinal);
        foreach (ContentId piece in tile.Pieces)
        {
            if (!Holds(rule, piece) || !seen.Add(piece.Value))
            {
                throw ContentException.ForField(
                    file.File,
                    field,
                    $"the tile {tile.At} holds the piece '{piece.Value}', and a {TileKinds.NameOf(kind)} tile holds each piece of its rule one time at most. Run the `edges` command again (D-501, D-1322)");
            }
        }
    }

    private static bool Holds(EdgeRule rule, ContentId piece)
    {
        foreach (ContentId held in rule.Pieces)
        {
            if (string.CompareOrdinal(held.Value, piece.Value) == 0)
            {
                return true;
            }
        }

        return false;
    }
}
