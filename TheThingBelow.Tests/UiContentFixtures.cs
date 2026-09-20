using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Tests;

/// <summary>
/// The smallest UI base that a content set needs: the style file, the device table, both
/// fonts, and the drawings that they name (D-527, D-710, D-711). A test of another rule adds
/// these files, so it fails on its own rule and never on an absent UI file (T-2).
/// </summary>
public static class UiContentFixtures
{
    /// <summary>The id of the window frame drawing that the fixture style names.</summary>
    public const string WindowDrawingId = "drawing.ui_window_frame";

    /// <summary>The id of the one glyph drawing that the fixture device table needs.</summary>
    public const string GlyphDrawingId = "drawing.ui_glyph_keyboard_confirm";

    /// <summary>The name of the page of the two UI drawings.</summary>
    public const string PageName = "ui";

    /// <summary>The path of the page file of the two UI drawings.</summary>
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

    /// <summary>The body of the fixture device table.</summary>
    public const string DevicesBody =
        """
        {
         "comment": "a test device table",
         "sets": [ "keyboard" ],
         "keyboard_set": "keyboard",
         "default_set": "keyboard",
         "prompts": [ "confirm" ],
         "names": [ ]
        }
        """;

    /// <summary>The entries that the fixture atlas index holds for the two UI drawings.</summary>
    public const string AtlasEntries =
        """
          {
           "id": "drawing.ui_window_frame",
           "page": "ui",
           "width": 1,
           "height": 1,
           "draws": [ { "content": "ui.window", "use": "frame" } ],
           "frames": [ { "x": 0, "y": 0, "ticks": 0 } ]
          },
          {
           "id": "drawing.ui_glyph_keyboard_confirm",
           "page": "ui",
           "width": 1,
           "height": 1,
           "draws": [ { "content": "ui.confirm", "use": "glyph_keyboard" } ],
           "frames": [ { "x": 1, "y": 0, "ticks": 0 } ]
          }
        """;

    /// <summary>The page record that the fixture atlas index holds.</summary>
    public const string AtlasPageRecord = """{ "kind": "ui", "number": 1, "width": 2, "height": 1 }""";

    /// <summary>Every file of the UI base, for a content set that tests another rule.</summary>
    /// <returns>The style file, the device table, both fonts, two drawings, and the page file.</returns>
    public static IReadOnlyList<ContentFile> Files() =>
    [
        Of(UiStyle.Path, StyleBody),
        Of(DeviceNames.Path, DevicesBody),
        new ContentFile(FontStrikes.BodyPath, FontBytes(12, 16, 24, 32)),
        new ContentFile(FontStrikes.TitlePath, FontBytes(12, 16, 24, 32)),
        Of("sprites/drawings/ui/window-frame.json", DrawingBody(WindowDrawingId, "ui.window", "frame")),
        Of("sprites/drawings/ui/keyboard-confirm.json", DrawingBody(GlyphDrawingId, "ui.confirm", "glyph_keyboard")),

        // The page is an image, and the content set records its path alone (D-517).
        new ContentFile(PageFile, [0]),
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
