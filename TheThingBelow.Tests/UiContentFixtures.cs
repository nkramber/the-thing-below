using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Light;

namespace TheThingBelow.Tests;

/// <summary>
/// The smallest UI base that a content set needs: the style file, both fonts, and the drawing
/// that the style names (D-527, D-710). It also holds the two light files that every set needs:
/// the carried light and the effect budget (D-523, D-847). A test of another rule adds these
/// files, so it fails on its own rule and never on an absent UI file (T-2).
/// </summary>
public static class UiContentFixtures
{
    /// <summary>The id of the window frame drawing that the fixture style names.</summary>
    public const string WindowDrawingId = "drawing.ui_window_frame";

    /// <summary>The name of the page of the UI drawing.</summary>
    public const string PageName = "ui";

    /// <summary>The path of the page file of the UI drawing.</summary>
    public const string PageFile = "sprites/atlas-ui.png";

    /// <summary>The body of the fixture style file.</summary>
    public const string StyleBody =
        """
        {
         "comment": "a test style",
         "small_body": 24,
         "large_body": 32,
         "title_scale": 2,
         "border_pixels": 1,
         "colors": [ { "role": "text", "key": "k" } ],
         "frames": [ { "role": "window", "drawing": "drawing.ui_window_frame" } ]
        }
        """;

    /// <summary>The entry that the fixture atlas index holds for the UI drawing.</summary>
    public const string AtlasEntries =
        """
          {
           "id": "drawing.ui_window_frame",
           "page": "ui",
           "width": 1,
           "height": 1,
           "draws": [ { "content": "ui.window", "use": "frame" } ],
           "frames": [ { "x": 0, "y": 0, "ticks": 0 } ]
          }
        """;

    /// <summary>The fire of a test torch: two levels and one stream of ink, which every test palette holds (D-890, D-891).</summary>
    public const string FireBody =
        """{ "step_ticks": 4, "levels": [ { "strength": 10000, "range": 10000 }, { "strength": 9000, "range": 9500 } ], "jump": 1, "emitters": [ { "amount": 6, "lifetime_ticks": 12, "colors": ["k"], "size": 1, "x": 0, "y": -2, "half_width": 1, "half_height": 0, "direction": -90, "spread": 10, "slowest_speed": 10, "fastest_speed": 20, "gravity": -10 } ] }""";

    /// <summary>The body of the fixture carried light (D-847).</summary>
    public const string CarriedBody =
        """{ "comment": "a test carried light", "color": "k", "strength": 10000, "range": 64, "height": 16, "x": 16, "y": -16, "fire": """ + FireBody + " }";

    /// <summary>The body of the fixture effect budget (D-523).</summary>
    public const string BudgetBody = """{ "comment": "a test budget", "lights_in_view": 15, "live_particles": 8192, "full_screen_passes": 3 }""";

    /// <summary>The page record that the fixture atlas index holds.</summary>
    public const string AtlasPageRecord = """{ "kind": "ui", "number": 1, "width": 1, "height": 1 }""";

    /// <summary>Every file of the UI base, for a content set that tests another rule.</summary>
    /// <returns>The style file, both fonts, the drawing, the page file, the carried light, and the budget.</returns>
    public static IReadOnlyList<ContentFile> Files() =>
    [
        Of(UiStyle.Path, StyleBody),
        new ContentFile(FontStrikes.BodyPath, FontBytes(12, 16, 24, 32)),
        new ContentFile(FontStrikes.TitlePath, FontBytes(12, 16, 24, 32)),
        Of("sprites/drawings/ui/window-frame.json", DrawingBody(WindowDrawingId, "ui.window", "frame")),

        // The page is an image, and the content set records its path alone (D-517).
        new ContentFile(PageFile, [0]),
        Of(CarriedLight.Path, CarriedBody),
        Of(EffectBudget.Path, BudgetBody),
    ];

    /// <summary>
    /// Makes the decor file and the light setup of one map, with no piece and no light: the two
    /// files that each map of a content set needs (D-442, D-844).
    /// </summary>
    /// <param name="stem">The name of both files, such as `one`.</param>
    /// <param name="map">The id of the map, such as `map.one`.</param>
    /// <param name="time">The time of day of the map, such as `day`.</param>
    /// <returns>The decor file and the light setup.</returns>
    public static IReadOnlyList<ContentFile> LightFilesOf(string stem, string map, string time) =>
    [
        Of($"{DecorFile.Folder}{stem}.json", $$"""{ "comment": "a test decor file", "map": "{{map}}", "pieces": [] }"""),
        Of(
            $"{LightSetup.Folder}{stem}-{time}.json",
            $$"""
            {
             "comment": "a test light setup",
             "map": "{{map}}",
             "time": "{{time}}",
             "ambient": { "color": "k", "strength": 10000 },
             "battle": { "color": "k", "strength": 10000, "range": 64, "height": 32 },
             "changes": [],
             "added": []
            }
            """),
    ];

    /// <summary>
    /// Makes the bytes of a font file that carries one bitmap strike for each size (D-710).
    /// The file holds the table directory and the `EBLC` table alone, which is every part
    /// that <see cref="FontStrikes"/> reads.
    /// </summary>
    /// <param name="sizes">The size in pixels of each strike, from the smallest.</param>
    /// <returns>The bytes of the file.</returns>
    public static byte[] FontBytes(params int[] sizes)
    {
        ArgumentNullException.ThrowIfNull(sizes);

        const int directoryStart = 12;
        const int recordLength = 16;
        const int strikeLength = 48;
        int tableStart = directoryStart + recordLength;
        int length = tableStart + 8 + (sizes.Length * strikeLength);

        byte[] bytes = new byte[length];
        WriteUInt32(bytes, 0, 0x00010000);
        WriteUInt16(bytes, 4, 1);

        WriteTag(bytes, directoryStart, "EBLC");
        WriteUInt32(bytes, directoryStart + 8, (uint)tableStart);
        WriteUInt32(bytes, directoryStart + 12, (uint)(length - tableStart));

        WriteUInt32(bytes, tableStart, 0x00020000);
        WriteUInt32(bytes, tableStart + 4, (uint)sizes.Length);
        for (int strike = 0; strike < sizes.Length; strike += 1)
        {
            int record = tableStart + 8 + (strike * strikeLength);
            bytes[record + 44] = (byte)sizes[strike];
            bytes[record + 45] = (byte)sizes[strike];
            bytes[record + 46] = 1;
        }

        return bytes;
    }

    /// <summary>Makes the body of a drawing file of one pixel on the UI page.</summary>
    /// <param name="id">The id of the drawing.</param>
    /// <param name="content">The content id that the drawing draws (D-519).</param>
    /// <param name="use">The name of the use, such as `frame`.</param>
    /// <returns>The JSON body of the file.</returns>
    public static string DrawingBody(string id, string content, string use) =>
        $$"""
        {
         "id": "{{id}}",
         "page": "ui",
         "width": 1,
         "height": 1,
         "draws": [ { "content": "{{content}}", "use": "{{use}}" } ],
         "frames": [ { "ticks": 0, "rows": [ "k" ] } ]
        }
        """;

    private static ContentFile Of(string path, string body) => new(path, Encoding.UTF8.GetBytes(body));

    private static void WriteTag(byte[] bytes, int start, string tag)
    {
        for (int letter = 0; letter < tag.Length; letter += 1)
        {
            bytes[start + letter] = (byte)tag[letter];
        }
    }

    private static void WriteUInt32(byte[] bytes, int start, uint value)
    {
        // A font file writes each number with the high byte first.
        bytes[start] = (byte)(value >> 24);
        bytes[start + 1] = (byte)(value >> 16);
        bytes[start + 2] = (byte)(value >> 8);
        bytes[start + 3] = (byte)value;
    }

    private static void WriteUInt16(byte[] bytes, int start, ushort value)
    {
        bytes[start] = (byte)(value >> 8);
        bytes[start + 1] = (byte)value;
    }
}
