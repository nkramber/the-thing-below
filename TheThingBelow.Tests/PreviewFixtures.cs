using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Edges;
using TheThingBelow.Core.Light;
using TheThingBelow.Core.Maps;
using TheThingBelow.Tools.Png;

namespace TheThingBelow.Tests;

/// <summary>
/// A small atlas of solid colors and a hub map with one sprite of each kind, for the tests of the
/// map preview (D-1317). Each drawing holds one color, so a test names the color that each pixel
/// of the preview must hold.
/// </summary>
/// <remarks>
/// The map is the room of <see cref="HubMaps"/>. It holds an NPC that faces east at (2, 2), an
/// NPC of two tiles in height at (4, 5), the waystone at (8, 1), a trap at (6, 2), and an enemy
/// at (7, 6). The decor hangs a torch of two tiles in height on the wall at (4, 4), over the
/// tall NPC, and a torch on the north wall at (1, 0), which the top edge of the preview clips. The
/// edge file lays a north edge piece on the floor at (3, 6) and under the enemy at (7, 6).
/// </remarks>
public static class PreviewFixtures
{
    /// <summary>The color of the floor tile.</summary>
    public static readonly (byte R, byte G, byte B) Floor = (10, 20, 30);

    /// <summary>The color of the wall tile.</summary>
    public static readonly (byte R, byte G, byte B) Wall = (40, 40, 40);

    /// <summary>The color of the west half of the drawing of the NPC that faces east.</summary>
    public static readonly (byte R, byte G, byte B) Npc = (200, 0, 0);

    /// <summary>The color of the square of the waystone.</summary>
    public static readonly (byte R, byte G, byte B) Save = (0, 200, 0);

    /// <summary>The color of the enemy.</summary>
    public static readonly (byte R, byte G, byte B) Enemy = (0, 0, 200);

    /// <summary>The color of the trap.</summary>
    public static readonly (byte R, byte G, byte B) Trap = (200, 200, 0);

    /// <summary>The color of the tall NPC.</summary>
    public static readonly (byte R, byte G, byte B) Tall = (200, 0, 200);

    /// <summary>The color of each torch.</summary>
    public static readonly (byte R, byte G, byte B) Torch = (0, 200, 200);

    /// <summary>The color of the strip of the edge piece, in the 4 north rows of its tile.</summary>
    public static readonly (byte R, byte G, byte B) Edge = (90, 60, 30);

    /// <summary>The count of the rows of the strip of the edge piece.</summary>
    public const int EdgeRows = 4;

    /// <summary>The id of the NPC that faces east.</summary>
    public const string EastNpcId = "npc.preview_east";

    /// <summary>The id of the tall NPC.</summary>
    public const string TallNpcId = "npc.preview_tall";

    /// <summary>Gives the text of the atlas index.</summary>
    /// <param name="withWall">False to leave out the drawing of the wall tile.</param>
    /// <returns>The text of the file.</returns>
    public static string IndexText(bool withWall = true)
    {
        string wall = withWall
            ? """{ "id": "drawing.preview_wall", "page": "tiles", "width": 32, "height": 32, "draws": [{ "content": "tile.wall", "use": "map" }], "frames": [{ "x": 32, "y": 0, "ticks": 0 }] },"""
            : string.Empty;
        return $$"""
            {
             "comment": "the atlas of the preview tests",
             "pages": [
              { "kind": "tiles", "number": 1, "width": 96, "height": 32 },
              { "kind": "map_sprites", "number": 1, "width": 160, "height": 64 },
              { "kind": "pieces", "number": 1, "width": 32, "height": 64 }
             ],
             "drawings": [
              { "id": "drawing.preview_floor", "page": "tiles", "width": 32, "height": 32, "draws": [{ "content": "tile.floor", "use": "map" }], "frames": [{ "x": 0, "y": 0, "ticks": 0 }] },
              {{wall}}
              { "id": "drawing.preview_edge", "page": "tiles", "width": 32, "height": 32, "draws": [{ "content": "edge.preview_north", "use": "map" }], "frames": [{ "x": 64, "y": 0, "ticks": 0 }] },
              { "id": "drawing.preview_npc", "page": "map_sprites", "width": 32, "height": 32, "draws": [{ "content": "{{EastNpcId}}", "use": "map_front" }], "frames": [{ "x": 0, "y": 0, "ticks": 0 }] },
              { "id": "drawing.preview_save", "page": "map_sprites", "width": 32, "height": 32, "draws": [{ "content": "save_point.hub_waystone", "use": "map" }], "frames": [{ "x": 32, "y": 0, "ticks": 0 }] },
              { "id": "drawing.preview_patrol", "page": "map_sprites", "width": 32, "height": 32, "draws": [{ "content": "patrol.preview", "use": "map_front" }], "frames": [{ "x": 64, "y": 0, "ticks": 0 }] },
              { "id": "drawing.preview_trap", "page": "map_sprites", "width": 32, "height": 32, "draws": [{ "content": "trap.preview", "use": "map" }], "frames": [{ "x": 96, "y": 0, "ticks": 0 }] },
              { "id": "drawing.preview_tall", "page": "map_sprites", "width": 32, "height": 64, "draws": [{ "content": "{{TallNpcId}}", "use": "map_front" }], "frames": [{ "x": 128, "y": 0, "ticks": 0 }] },
              { "id": "drawing.preview_torch", "page": "pieces", "width": 32, "height": 64, "draws": [{ "content": "decor.preview_torch", "use": "map" }], "frames": [{ "x": 0, "y": 0, "ticks": 0 }] }
             ]
            }
            """;
    }

    /// <summary>Reads the atlas index with every drawing.</summary>
    public static AtlasIndex Index() => Index(IndexText());

    /// <summary>Reads the text of an atlas index.</summary>
    /// <param name="text">The text of the file.</param>
    /// <returns>The index.</returns>
    public static AtlasIndex Index(string text) => AtlasIndex.Read(Encoding.UTF8.GetBytes(text), AtlasIndex.Path);

    /// <summary>Gives each page of the atlas, by its name.</summary>
    /// <returns>The pages: `tiles`, `map_sprites`, and `pieces`.</returns>
    public static SortedDictionary<string, PngImage> Pages()
    {
        var tiles = new Page(96, 32);
        tiles.Fill(0, 0, 32, 32, Floor);
        tiles.Fill(32, 0, 32, 32, Wall);
        tiles.Fill(64, 0, 32, EdgeRows, Edge);

        var sprites = new Page(160, 64);
        sprites.Fill(0, 0, 16, 32, Npc);
        sprites.Fill(32 + 12, 24, 8, 8, Save);
        sprites.Fill(64, 0, 32, 32, Enemy);
        sprites.Fill(96, 0, 32, 32, Trap);
        sprites.Fill(128, 0, 32, 64, Tall);

        var pieces = new Page(32, 64);
        pieces.Fill(0, 0, 32, 64, Torch);

        return new SortedDictionary<string, PngImage>(StringComparer.Ordinal)
        {
            ["tiles"] = tiles.ToImage(),
            ["map_sprites"] = sprites.ToImage(),
            ["pieces"] = pieces.ToImage(),
        };
    }

    /// <summary>Reads the hub map with one sprite of each kind.</summary>
    /// <returns>The map, with the id `map.hub_test`.</returns>
    public static GameMap Map() => HubMaps.Of(
        npcs: $$"""
            { "id": "{{EastNpcId}}", "facing": "east", "step_ticks": 32, "move": "route", "tiles": [{ "x": 2, "y": 2, "wait_ticks": 0 }] },
            { "id": "{{TallNpcId}}", "facing": "south", "step_ticks": 32, "move": "route", "tiles": [{ "x": 4, "y": 5, "wait_ticks": 0 }] }
            """,
        things: $$"""
            {{HubMaps.Waystone}},
            { "id": "trap.preview", "kind": "trap", "x": 6, "y": 2, "harm": "damage", "share": 2500 }
            """,
        enemies: PatrolMaps.Enemy(
            id: "patrol.preview",
            stations: """
               "routes": [
                { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 7, "y": 6 }, { "x": 8, "y": 6 }] }
               ]
              """));

    /// <summary>Reads the edge file of the hub map: a north edge piece at (3, 6) and at (7, 6).</summary>
    /// <returns>The edge file.</returns>
    public static EdgeFile Edges() => Edges("map.hub_test");

    /// <summary>Reads an edge file with the two edge pieces.</summary>
    /// <param name="map">The id of the map that the file serves.</param>
    /// <returns>The edge file.</returns>
    public static EdgeFile Edges(string map) => EdgeFile.Read(
        Encoding.UTF8.GetBytes($$"""
            {
             "comment": "the edge pieces of the preview tests",
             "map": "{{map}}",
             "tiles": [
              { "x": 3, "y": 6, "pieces": ["edge.preview_north"] },
              { "x": 7, "y": 6, "pieces": ["edge.preview_north"] }
             ]
            }
            """),
        EdgeFile.Folder + "hub-test.json");

    /// <summary>Reads the decor file of the hub map: a torch on the wall block and a torch on the north wall.</summary>
    /// <returns>The decor file.</returns>
    public static DecorFile Decor() => Decor("map.hub_test");

    /// <summary>Reads a decor file with the two torches.</summary>
    /// <param name="map">The id of the map that the file serves.</param>
    /// <returns>The decor file.</returns>
    public static DecorFile Decor(string map) => DecorFile.Read(
        Encoding.UTF8.GetBytes($$"""
            {
             "comment": "the torches of the preview tests",
             "map": "{{map}}",
             "pieces": [
              { "id": "piece.preview_block", "kind": "decor.preview_torch", "x": 4, "y": 4 },
              { "id": "piece.preview_north", "kind": "decor.preview_torch", "x": 1, "y": 0 }
             ],
             "shafts": []
            }
            """),
        DecorFile.Folder + "hub-test.json");

    /// <summary>The pixels of one page, transparent until a fill.</summary>
    public sealed class Page(int width, int height)
    {
        private readonly byte[] pixels = new byte[width * height * 4];

        /// <summary>Fills a rectangle with one opaque color.</summary>
        public void Fill(int left, int top, int wide, int tall, (byte R, byte G, byte B) color)
        {
            for (int y = top; y < top + tall; y += 1)
            {
                for (int x = left; x < left + wide; x += 1)
                {
                    this.Set(x, y, color.R, color.G, color.B, byte.MaxValue);
                }
            }
        }

        /// <summary>Sets one pixel.</summary>
        public void Set(int x, int y, byte red, byte green, byte blue, byte alpha)
        {
            int at = ((y * width) + x) * 4;
            this.pixels[at] = red;
            this.pixels[at + 1] = green;
            this.pixels[at + 2] = blue;
            this.pixels[at + 3] = alpha;
        }

        /// <summary>Gives the page as an image.</summary>
        public PngImage ToImage() => new(width, height, PngColorKind.Rgba, this.pixels);
    }
}
