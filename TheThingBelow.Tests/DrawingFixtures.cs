using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Tests;

/// <summary>
/// Builds drawing files for the tests of the atlas (D-515, D-666). Each fixture goes through
/// the reader of Core, so every test holds a drawing that a file could hold.
/// </summary>
public static class DrawingFixtures
{
    /// <summary>The palette that each fixture names, with one key for each test color.</summary>
    public const string PaletteBody =
        """
        {
         "comment": "a test palette",
         "colors": [
          { "index": 0, "key": "k", "hex": "0b0a0f", "name": "ink" },
          { "index": 1, "key": "K", "hex": "1a1823", "name": "night" },
          { "index": 2, "key": "d", "hex": "2b2836", "name": "shadow" },
          { "index": 3, "key": "D", "hex": "403c4d", "name": "slate" },
          { "index": 4, "key": "w", "hex": "f2eeea", "name": "chalk" },
          { "index": 5, "key": "e", "hex": "e8f2f7", "name": "snow" },
          { "index": 6, "key": "x", "hex": "c8353a", "name": "red" }
         ]
        }
        """;

    /// <summary>Reads the palette of the fixtures.</summary>
    /// <returns>The palette.</returns>
    public static Palette Palette() =>
        Core.Content.Palette.Read(Encoding.UTF8.GetBytes(PaletteBody), Core.Content.Palette.Path);

    /// <summary>Builds one drawing of a solid block of one key.</summary>
    /// <param name="name">The name part of the id, such as `test_map_front`.</param>
    /// <param name="page">The name of the page kind, such as `map_sprites`.</param>
    /// <param name="width">The width of each frame in pixels.</param>
    /// <param name="height">The height of each frame in pixels.</param>
    /// <param name="frames">The count of frames.</param>
    /// <param name="key">The palette key of every pixel.</param>
    /// <returns>The drawing, as the reader of Core gives it.</returns>
    public static Drawing Solid(
        string name,
        string page = "map_sprites",
        int width = 32,
        int height = 32,
        int frames = 1,
        char key = 'k') =>
        Drawing.Read(Encoding.UTF8.GetBytes(Body(name, page, width, height, frames, key)), PathOf(name));

    /// <summary>Gives the text of a drawing file of a solid block.</summary>
    /// <param name="name">The name part of the id.</param>
    /// <param name="page">The name of the page kind.</param>
    /// <param name="width">The width of each frame in pixels.</param>
    /// <param name="height">The height of each frame in pixels.</param>
    /// <param name="frames">The count of frames.</param>
    /// <param name="key">The palette key of every pixel.</param>
    /// <returns>The text of the file.</returns>
    public static string Body(
        string name,
        string page = "map_sprites",
        int width = 32,
        int height = 32,
        int frames = 1,
        char key = 'k')
    {
        string row = new(key, width);
        var rows = new List<string>(height);
        for (int index = 0; index < height; index += 1)
        {
            rows.Add($"\"{row}\"");
        }

        var parts = new List<string>(frames);
        for (int index = 0; index < frames; index += 1)
        {
            int ticks = frames == 1 ? 0 : index + 1;
            parts.Add($"  {{ \"ticks\": {ticks}, \"rows\": [ {string.Join(", ", rows)} ] }}");
        }

        return $$"""
            {
             "id": "drawing.{{name}}",
             "page": "{{page}}",
             "width": {{width}},
             "height": {{height}},
             "draws": [
              { "content": "cast.{{name}}", "use": "map_front" }
             ],
             "frames": [
            {{string.Join(",\n", parts)}}
             ]
            }
            """;
    }

    /// <summary>Gives the content path of a fixture drawing file.</summary>
    /// <param name="name">The name part of the id.</param>
    /// <returns>The path under `content/`.</returns>
    public static string PathOf(string name) => $"{Drawing.Folder}{name}.json";
}
