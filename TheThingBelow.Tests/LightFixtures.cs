using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Light;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Tests;

/// <summary>
/// A small lit place for the tests of the light files: one map, one decor kind, the palette,
/// and the atlas index that draws the kind (D-519, D-843, D-844).
/// </summary>
public static class LightFixtures
{
    /// <summary>The id of the test map.</summary>
    public const string MapId = "map.lit";

    /// <summary>The id of the test decor kind.</summary>
    public const string KindId = "decor.torch";

    /// <summary>The path of the test decor file.</summary>
    public const string DecorPath = "decor/maps/lit.json";

    /// <summary>The path of the test light setup.</summary>
    public const string SetupPath = "light/setups/lit-night.json";

    /// <summary>The path of the test decor kind.</summary>
    public const string KindPath = "decor/kinds/torch.json";

    /// <summary>
    /// A room of 20 by 12 tiles: a wall on each edge and floor inside. A torch at (2, 0) hangs
    /// on the north wall over the floor at (2, 1).
    /// </summary>
    public static readonly string[] Room =
    [
        "####################",
        "#..................#",
        "#..................#",
        "#..................#",
        "#..................#",
        "#..................#",
        "#..................#",
        "#..................#",
        "#..................#",
        "#..................#",
        "#..................#",
        "####################",
    ];

    /// <summary>The body of the test decor kind: a torch with the light 4 pixels over the tile to its south.</summary>
    public const string KindBody =
        """
        {
         "comment": "a test torch",
         "id": "decor.torch",
         "light": { "color": "j", "strength": 12000, "range": 96, "height": 24, "x": 16, "y": 36 }
        }
        """;

    /// <summary>Makes the body of a decor file of the test map.</summary>
    /// <param name="pieces">The JSON of each piece, joined with commas.</param>
    /// <param name="map">The id of the map that the file names.</param>
    /// <returns>The body.</returns>
    public static string DecorBody(string pieces, string map = MapId) =>
        $$"""{ "comment": "a test decor file", "map": "{{map}}", "pieces": [ {{pieces}} ] }""";

    /// <summary>Makes the JSON of one torch piece.</summary>
    /// <param name="name">The name part of the id of the piece.</param>
    /// <param name="x">The column of the tile.</param>
    /// <param name="y">The row of the tile.</param>
    /// <returns>The JSON of the piece.</returns>
    public static string Piece(string name, int x, int y) =>
        $$"""{ "id": "piece.{{name}}", "kind": "decor.torch", "x": {{x}}, "y": {{y}} }""";

    /// <summary>Makes the body of a light setup of the test map.</summary>
    /// <param name="changes">The JSON of each change, joined with commas.</param>
    /// <param name="added">The JSON of each added light, joined with commas.</param>
    /// <param name="map">The id of the map that the setup names.</param>
    /// <param name="time">The time of day of the setup.</param>
    /// <returns>The body.</returns>
    public static string SetupBody(string changes = "", string added = "", string map = MapId, string time = "night") =>
        $$"""
        {
         "comment": "a test light setup",
         "map": "{{map}}",
         "time": "{{time}}",
         "ambient": { "color": "k", "strength": 5000 },
         "battle": { "color": "j", "strength": 10000, "range": 480, "height": 96 },
         "changes": [ {{changes}} ],
         "added": [ {{added}} ]
        }
        """;

    /// <summary>Makes the JSON of one added light at the center of a tile.</summary>
    /// <param name="name">The name part of the id of the light.</param>
    /// <param name="x">The column of the tile.</param>
    /// <param name="y">The row of the tile.</param>
    /// <param name="range">The range of the light, in art pixels.</param>
    /// <returns>The JSON of the light.</returns>
    public static string Added(string name, int x, int y, int range = 32) =>
        $$"""{ "id": "light.{{name}}", "x": {{x}}, "y": {{y}}, "color": "j", "strength": 8000, "range": {{range}}, "height": 16 }""";

    /// <summary>Makes the body of an effect budget.</summary>
    /// <param name="lightsInView">The light row.</param>
    /// <returns>The body.</returns>
    public static string BudgetBody(int lightsInView) =>
        $$"""{ "comment": "a test budget", "lights_in_view": {{lightsInView}} }""";

    /// <summary>Makes the light files of the test place: the kind, the decor file, the setup, the carried light, and the budget.</summary>
    /// <param name="decor">The body of the decor file.</param>
    /// <param name="setup">The body of the light setup.</param>
    /// <param name="budget">The body of the effect budget.</param>
    /// <returns>The files.</returns>
    public static List<ContentFile> Files(string decor, string setup, string? budget = null) =>
    [
        File(KindPath, KindBody),
        File(DecorPath, decor),
        File(SetupPath, setup),
        File(CarriedLight.Path, UiContentFixtures.CarriedBody),
        File(EffectBudget.Path, budget ?? BudgetBody(15)),
    ];

    /// <summary>Reads the test map, with the terrain that the test gives.</summary>
    /// <param name="terrain">The terrain rows, with a floor at (1, 1) for the spawn point.</param>
    /// <returns>Every map of the test, by its id.</returns>
    public static SortedDictionary<string, GameMap> Maps(string[]? terrain = null)
    {
        string rows = string.Join(", ", System.Array.ConvertAll(terrain ?? Room, row => $"\"{row}\""));
        string body =
            $$"""
            {
             "comment": "a test map",
             "id": "map.lit",
             "label": "label.lit",
             "time": "night",
             "terrain": [ {{rows}} ],
             "things": [ { "id": "spawn_point.lit_start", "kind": "spawn_point", "x": 1, "y": 1 } ],
             "enemies": []
            }
            """;
        GameMap map = GameMap.Read(Encoding.UTF8.GetBytes(body), "rules/maps/lit.json");
        return new SortedDictionary<string, GameMap>(System.StringComparer.Ordinal) { [map.Id.Value] = map };
    }

    /// <summary>The palette of the test: ink for the ambient light and a flame for each light.</summary>
    /// <returns>The palette.</returns>
    public static Palette Palette() => Core.Content.Palette.Read(
        Encoding.UTF8.GetBytes(
            """
            {
             "comment": "a test palette",
             "colors": [
              { "index": 0, "key": "k", "hex": "0b0a0f", "name": "ink", "height": 0 },
              { "index": 1, "key": "j", "hex": "fb9b21", "name": "flame", "height": 0 }
             ]
            }
            """),
        Core.Content.Palette.Path);

    /// <summary>The atlas index of the test, with one drawing of the decor kind (D-519).</summary>
    /// <param name="draws">The content id that the drawing draws.</param>
    /// <returns>The index.</returns>
    public static AtlasIndex Atlas(string draws = KindId) => AtlasIndex.Read(
        Encoding.UTF8.GetBytes(
            $$"""
            {
             "comment": "a test index",
             "pages": [ { "kind": "map_sprites", "number": 1, "width": 32, "height": 32 } ],
             "drawings": [
              {
               "id": "drawing.torch",
               "page": "map_sprites",
               "width": 32,
               "height": 32,
               "draws": [ { "content": "{{draws}}", "use": "map" } ],
               "frames": [ { "x": 0, "y": 0, "ticks": 0 } ]
              }
             ]
            }
            """),
        AtlasIndex.Path);

    /// <summary>Loads the light files of the test place against the test map, palette, and atlas.</summary>
    /// <param name="files">The light files.</param>
    /// <param name="terrain">The terrain of the map, or the room when no value is given.</param>
    /// <returns>The light content.</returns>
    public static LightContent Load(List<ContentFile> files, string[]? terrain = null) =>
        LightContent.Load(files, Maps(terrain), Palette(), Atlas());

    /// <summary>Makes a content file from its path and its text.</summary>
    /// <param name="path">The path under `content/`.</param>
    /// <param name="body">The text of the file.</param>
    /// <returns>The file.</returns>
    public static ContentFile File(string path, string body) => new(path, Encoding.UTF8.GetBytes(body));
}
