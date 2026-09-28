using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Edges;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Tests;

/// <summary>
/// The edge rules, the edge files, the maps, and the atlas of the tests of the tile-edge tool
/// (D-501, D-1321 to D-1327).
/// </summary>
/// <remarks>
/// The water rule joins the bridge, as the rule of the checkout does (D-1325). The gorge rule
/// joins nothing. The atlas draws each piece of both rules as a tile of the tile page.
/// </remarks>
public static class EdgeFixtures
{
    /// <summary>The path of the water rule under `content/`.</summary>
    public const string WaterRulePath = "edges/kinds/water.json";

    /// <summary>The path of the gorge rule under `content/`.</summary>
    public const string GorgeRulePath = "edges/kinds/gorge.json";

    /// <summary>The path of the edge file of <see cref="MapOf"/> under `content/`.</summary>
    public const string EdgePath = "edges/maps/edge-test.json";

    /// <summary>The id of the map of <see cref="MapOf"/>.</summary>
    public const string MapId = "map.edge_test";

    /// <summary>The text of the water rule.</summary>
    public static string WaterRuleText => RuleText("water", "\"bridge\"");

    /// <summary>The text of the gorge rule.</summary>
    public static string GorgeRuleText => RuleText("gorge", string.Empty);

    /// <summary>Makes the text of an edge rule with a piece for each place.</summary>
    /// <param name="kind">The name of the kind, such as `water`.</param>
    /// <param name="joins">The items of the `joins` array, such as `"bridge"`.</param>
    /// <returns>The text of the file.</returns>
    public static string RuleText(string kind, string joins) => $$"""
        {
         "comment": "a test edge rule",
         "kind": "{{kind}}",
         "joins": [{{joins}}],
         "pieces": {
          "north": "edge.{{kind}}_north",
          "east": "edge.{{kind}}_east",
          "south": "edge.{{kind}}_south",
          "west": "edge.{{kind}}_west",
          "north_east": "edge.{{kind}}_north_east",
          "south_east": "edge.{{kind}}_south_east",
          "south_west": "edge.{{kind}}_south_west",
          "north_west": "edge.{{kind}}_north_west"
         }
        }
        """;

    /// <summary>Reads an edge rule from its text.</summary>
    /// <param name="text">The text of the file.</param>
    /// <param name="path">The path of the file under `content/`.</param>
    /// <returns>The rule.</returns>
    public static EdgeRule Rule(string text, string path = WaterRulePath) => EdgeRule.Read(Encoding.UTF8.GetBytes(text), path);

    /// <summary>Gives the water rule and the gorge rule, by their kinds.</summary>
    /// <returns>The two rules.</returns>
    public static SortedDictionary<TileKind, EdgeRule> Rules() => new()
    {
        [TileKind.Water] = Rule(WaterRuleText, WaterRulePath),
        [TileKind.Gorge] = Rule(GorgeRuleText, GorgeRulePath),
    };

    /// <summary>Reads an edge file from its text.</summary>
    /// <param name="text">The text of the file.</param>
    /// <returns>The edge file.</returns>
    public static EdgeFile File(string text) => EdgeFile.Read(Encoding.UTF8.GetBytes(text), EdgePath);

    /// <summary>Makes the text of an edge file of <see cref="MapId"/>.</summary>
    /// <param name="tiles">The items of the `tiles` array.</param>
    /// <returns>The text of the file.</returns>
    public static string FileText(string tiles) => $$"""
        {
         "comment": "a test edge file",
         "map": "{{MapId}}",
         "tiles": [{{tiles}}]
        }
        """;

    /// <summary>Makes a map of terrain rows, with its spawn point on the first tile of grass, floor, road, or snowfield.</summary>
    /// <param name="rows">The terrain rows, one character for one tile (D-528).</param>
    /// <returns>The map, with the id <see cref="MapId"/>.</returns>
    public static GameMap MapOf(params string[] rows) => TestMaps.Of("edge-test.json", MapText(rows));

    /// <summary>Makes the text of the map of <see cref="MapOf"/>.</summary>
    /// <param name="rows">The terrain rows, one character for one tile (D-528).</param>
    /// <returns>The text of the map file.</returns>
    public static string MapText(params string[] rows)
    {
        TilePoint spawn = FirstWalkable(rows);
        var terrain = new StringBuilder();
        var zones = new StringBuilder();
        for (int row = 0; row < rows.Length; row += 1)
        {
            string end = row == rows.Length - 1 ? string.Empty : ",";
            terrain.Append($"  \"{rows[row]}\"{end}\n");
            zones.Append($"  \"{ZoneRow(rows[row])}\"{end}\n");
        }

        return $$"""
            {
             "comment": "A map for the tests of the tile-edge tool.",
             "id": "{{MapId}}",
             "region": "region.test",
             "label": "label.edge_test",
             "kind": "overworld",
             "time": "day",
             "dark": false,
             "terrain": [
            {{terrain}} ],
             "things": [
              { "id": "spawn_point.edge_test_start", "kind": "spawn_point", "x": {{spawn.X}}, "y": {{spawn.Y}} }
             ],
             "zones": [
              { "id": "zone.edge_test", "key": "r", "region": "region.test", "rate": 0, "groups": [], "condition": { "always": true } }
             ],
             "zone_grid": [
            {{zones}} ],
             "enemies": [], "npcs": [], "services": [], "reopen": [], "triggers": []
            }
            """;
    }

    /// <summary>Makes the text of an atlas index that draws each piece of the water rule and the gorge rule.</summary>
    /// <param name="left">The id of one piece to leave out, or an empty string.</param>
    /// <param name="page">The page of each piece, `tiles` for a legal atlas.</param>
    /// <returns>The text of the file.</returns>
    public static string AtlasText(string left = "", string page = "tiles")
    {
        var drawings = new List<string>();
        int cell = 0;
        foreach (string kind in new[] { "water", "gorge" })
        {
            foreach (EdgePlace place in EdgePlaces.All)
            {
                string piece = $"edge.{kind}_{EdgePlaces.NameOf(place)}";
                if (string.CompareOrdinal(piece, left) != 0)
                {
                    drawings.Add($$"""{ "id": "drawing.{{piece[5..]}}", "page": "{{page}}", "width": 32, "height": 32, "draws": [{ "content": "{{piece}}", "use": "map" }], "frames": [{ "x": {{cell * 32}}, "y": 0, "ticks": 0 }] }""");
                }

                cell += 1;
            }
        }

        return $$"""
            {
             "comment": "the atlas of the edge tests",
             "pages": [
              { "kind": "tiles", "number": 1, "width": {{cell * 32}}, "height": 32 },
              { "kind": "pieces", "number": 1, "width": {{cell * 32}}, "height": 32 }
             ],
             "drawings": [
              {{string.Join(",\n  ", drawings)}}
             ]
            }
            """;
    }

    /// <summary>Reads the text of an atlas index.</summary>
    /// <param name="text">The text of the file.</param>
    /// <returns>The index.</returns>
    public static AtlasIndex Atlas(string text) => AtlasIndex.Read(Encoding.UTF8.GetBytes(text), AtlasIndex.Path);

    /// <summary>Makes one content file from its path and its text.</summary>
    /// <param name="path">The path under `content/`.</param>
    /// <param name="text">The text of the file.</param>
    /// <returns>The file.</returns>
    public static ContentFile Of(string path, string text) => new(path, Encoding.UTF8.GetBytes(text));

    /// <summary>Gives the zone row of one terrain row: each tile that the party can walk holds the one zone (D-1262).</summary>
    private static string ZoneRow(string terrain)
    {
        var row = new StringBuilder();
        foreach (char character in terrain)
        {
            row.Append(TileKinds.TryOf(character, out TileKind kind) && TileKinds.CanWalk(kind) ? 'r' : '.');
        }

        return row.ToString();
    }

    private static TilePoint FirstWalkable(string[] rows)
    {
        for (int row = 0; row < rows.Length; row += 1)
        {
            for (int column = 0; column < rows[row].Length; column += 1)
            {
                if (TileKinds.TryOf(rows[row][column], out TileKind kind) && kind is TileKind.Grass or TileKind.Floor or TileKind.Road or TileKind.Snowfield)
                {
                    return new TilePoint(column, row);
                }
            }
        }

        throw new InvalidOperationException("The test map holds no tile of grass, floor, road, or snowfield, so it has no place for its spawn point (D-1256).");
    }
}
