using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Tests;

/// <summary>
/// A content set with two pieces on the `pieces` page and one large picture of them (D-516,
/// D-817, D-818). A test of a picture rule changes one file and keeps the rest.
/// </summary>
/// <remarks>
/// Piece A is 2 by 2 art pixels with rows `kw` and `k.`, so its lower right pixel is a dot.
/// Piece B is 3 by 1 art pixels with the row `DDD`.
/// </remarks>
public static class PictureFixtures
{
    /// <summary>The id of the picture of the default set.</summary>
    public const string PictureId = "picture.test";

    /// <summary>The path of the picture file of the default set.</summary>
    public const string PicturePath = "sprites/pictures/test.json";

    /// <summary>The id of the piece of 2 by 2 pixels.</summary>
    public const string PieceA = "drawing.piece_a";

    /// <summary>The id of the piece of 3 by 1 pixels.</summary>
    public const string PieceB = "drawing.piece_b";

    /// <summary>
    /// The places of the default picture of 6 by 4 pixels. Piece B fills row 0 in two copies.
    /// Piece A takes three copies across and two down, so its second row of copies clips at
    /// the bottom edge. A last copy of piece B covers the left of row 3. The picture is:
    /// <code>
    /// DDDDDD
    /// kwkwkw
    /// k.k.k.
    /// DDDwkw
    /// </code>
    /// </summary>
    public const string DefaultPlaces =
        """
        { "piece": "drawing.piece_b", "x": 0, "y": 0, "across": 2, "down": 1 },
        { "piece": "drawing.piece_a", "x": 0, "y": 1, "across": 3, "down": 2 },
        { "piece": "drawing.piece_b", "x": 0, "y": 3, "across": 1, "down": 1 }
        """;

    /// <summary>Makes the body of a picture file.</summary>
    /// <param name="places">The text of the entries of `places`.</param>
    /// <param name="width">The width of the picture.</param>
    /// <param name="height">The height of the picture.</param>
    /// <param name="id">The id of the picture.</param>
    /// <returns>The JSON body of the file.</returns>
    public static string PictureBody(string places = DefaultPlaces, int width = 6, int height = 4, string id = PictureId) =>
        $$"""
        {
         "id": "{{id}}",
         "width": {{width}},
         "height": {{height}},
         "places": [ {{places}} ]
        }
        """;

    /// <summary>Gives every file of a set with the two pieces and one picture.</summary>
    /// <param name="picture">The body of the picture file.</param>
    /// <param name="indexEntries">More entries of the atlas index, each with a comma after it.</param>
    /// <param name="extra">Other files of the set.</param>
    /// <returns>The files.</returns>
    public static IReadOnlyList<ContentFile> Files(string? picture = null, string indexEntries = "", params ContentFile[] extra)
    {
        List<ContentFile> files =
        [
            File(Palette.Path, DrawingFixtures.PaletteBody),
            File(StringTable.Path, """{ "comment": "a test table", "strings": [ ] }"""),
            File(AtlasIndex.Path, IndexBody(indexEntries)),
            File("sprites/drawings/pieces/a.json", PieceBody(PieceA, 2, 2, "\"kw\", \"k.\"", "a")),
            File("sprites/drawings/pieces/b.json", PieceBody(PieceB, 3, 1, "\"DDD\"", "b")),

            // Core reads no pixel of a page, so the bytes are free (D-517).
            File("sprites/atlas-pieces.png", "png"),
            File(PicturePath, picture ?? PictureBody()),
        ];

        files.AddRange(UiContentFixtures.Files());
        files.AddRange(TestBattles.Files());
        files.AddRange(extra);
        return files;
    }

    /// <summary>Makes a content id of a test.</summary>
    /// <param name="value">The text of the id.</param>
    /// <returns>The id.</returns>
    public static ContentId Id(string value) => ContentId.Parse(value, PicturePath, "id");

    /// <summary>Makes a content file of a text body.</summary>
    /// <param name="path">The path under `content/`.</param>
    /// <param name="body">The text of the file.</param>
    /// <returns>The file.</returns>
    public static ContentFile File(string path, string body) => new(path, Encoding.UTF8.GetBytes(body));

    private static string PieceBody(string id, int width, int height, string rows, string use) =>
        $$"""
        {
         "id": "{{id}}",
         "page": "pieces",
         "width": {{width}},
         "height": {{height}},
         "draws": [ { "content": "picture.test", "use": "{{use}}" } ],
         "frames": [ { "ticks": 0, "rows": [ {{rows}} ] } ]
        }
        """;

    private static string IndexBody(string extraEntries) =>
        $$"""
        {
         "comment": "a test index",
         "pages": [ { "kind": "pieces", "number": 1, "width": 5, "height": 2 }, {{UiContentFixtures.AtlasPageRecord}} ],
         "drawings": [
          {
           "id": "{{PieceA}}",
           "page": "pieces",
           "width": 2,
           "height": 2,
           "draws": [ { "content": "picture.test", "use": "a" } ],
           "frames": [ { "x": 0, "y": 0, "ticks": 0 } ]
          },
          {
           "id": "{{PieceB}}",
           "page": "pieces",
           "width": 3,
           "height": 1,
           "draws": [ { "content": "picture.test", "use": "b" } ],
           "frames": [ { "x": 2, "y": 0, "ticks": 0 } ]
          },
        {{extraEntries}}
        {{UiContentFixtures.AtlasEntries}}
         ]
        }
        """;
}
