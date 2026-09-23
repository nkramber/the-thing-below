using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Light;

/// <summary>
/// One kind of light shaft: a still beam of light from an opening in a wall, such as a window,
/// into the room to its south (D-849, D-918, D-924, D-925). No rule reads a shaft, so the file
/// lies outside the rule folder (D-495).
/// </summary>
/// <remarks>
/// The decor file of a map places each shaft on a wall, as it places each torch (D-844, D-918).
/// A drawing of the opening draws the kind, and Game draws it on the wall, so no beam comes out
/// of a bare wall (D-924). A shaft has no Godot light. The shaft pass of Game draws every shaft of
/// the view in one full-screen pass, above the fog, so a beam never glows (D-916, D-919).
/// </remarks>
public sealed class ShaftKind
{
    /// <summary>The folder of the shaft files, under `content/`.</summary>
    public const string Folder = "decor/shafts/";

    /// <summary>The kind of the id of each shaft kind (D-646).</summary>
    public const string IdKind = "shaft";

    /// <summary>The most shafts of one map: the size of the arrays of the shaft shader.</summary>
    public const int MostShaftsOnMap = 8;

    /// <summary>The full-screen passes of the shafts of one map, which the effect budget counts on a map with a shaft (D-523, D-918).</summary>
    public const int FullScreenPasses = 1;

    /// <summary>The widest beam at its top, in art pixels: one tile.</summary>
    public const int MostWidth = 32;

    /// <summary>The longest beam, in art pixels: the height of the view.</summary>
    public const int MostLength = 360;

    private ShaftKind(string file, ContentId id, char key, int strength, int width, int length, int slant, int x, int y)
    {
        this.File = file;
        this.Id = id;
        this.Key = key;
        this.Strength = strength;
        this.Width = width;
        this.Length = length;
        this.Slant = slant;
        this.X = x;
        this.Y = y;
    }

    /// <summary>The path of the file, under `content/`, which every error names (T-2).</summary>
    public string File { get; }

    /// <summary>The id of the kind, such as `shaft.fixture_window`. A drawing of the opening draws it (D-924).</summary>
    public ContentId Id { get; }

    /// <summary>The palette key of the light of the beam (D-181).</summary>
    public char Key { get; }

    /// <summary>The light of the beam at its top, in basis points of its palette color. The light fades to nothing at its foot.</summary>
    public int Strength { get; }

    /// <summary>The width of the beam at its top, in art pixels. The beam grows a little wider as it falls.</summary>
    public int Width { get; }

    /// <summary>The length of the beam, from its top to its foot, in art pixels.</summary>
    public int Length { get; }

    /// <summary>The art pixels that the beam moves east from its top to its foot. A value below 0 moves it west.</summary>
    public int Slant { get; }

    /// <summary>The column of the top of the beam, in art pixels from the west edge of the tile of its piece.</summary>
    public int X { get; }

    /// <summary>
    /// The row of the top of the beam, in art pixels from the north edge of the tile of its piece.
    /// A value of the tile size or more puts the top over the tile to the south.
    /// </summary>
    public int Y { get; }

    /// <summary>Tells whether a content path is a shaft file.</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True when the path lies in the folder of the shaft files.</returns>
    public static bool IsShaftFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return path.StartsWith(Folder, StringComparison.Ordinal);
    }

    /// <summary>Reads one shaft kind from the bytes of its file.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The kind.</returns>
    /// <exception cref="ContentException">The file breaks a rule of the reader (G-6, T-2).</exception>
    public static ShaftKind Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        ShaftKind kind = Read(ref reader);
        reader.ReadFileEnd();
        return kind;
    }

    private static ShaftKind Read(ref ContentReader reader)
    {
        string? comment = null;
        ContentId? id = null;
        string? key = null;
        var values = new SortedDictionary<string, int>(StringComparer.Ordinal);

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "id":
                    id = reader.ReadContentId(IdKind);
                    break;
                case "color":
                    key = reader.ReadString();
                    break;
                case "strength" or "width" or "length" or "slant" or "x" or "y":
                    values[field] = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        ContentId read = reader.Require(id, depth, "id");
        string text = reader.Require(key, depth, "color");
        if (text.Length != 1)
        {
            throw reader.RefuseField(depth, "color", $"the color is '{text}', and a shaft names one palette key of one character (D-181)");
        }

        // The slant takes the length as its limit, so the length comes first.
        int length = InRange(ref reader, depth, values, "length", 1, MostLength, "a beam reaches the height of the view at most");
        return new ShaftKind(
            reader.File,
            read,
            text[0],
            InRange(ref reader, depth, values, "strength", 1, BasisPoints.One, "a beam of no light draws nothing (T-2), and a beam adds full white at most"),
            InRange(ref reader, depth, values, "width", 1, MostWidth, "a beam is one tile wide at most"),
            length,
            InRange(ref reader, depth, values, "slant", -length, length, "a beam slants by its length at most"),
            InRange(ref reader, depth, values, "x", 0, AtlasPages.TileSize - 1, "the top lies inside the tile"),
            InRange(ref reader, depth, values, "y", 0, (2 * AtlasPages.TileSize) - 1, "the top lies in the tile, or the tile to its south"));
    }

    private static int InRange(ref ContentReader reader, int depth, SortedDictionary<string, int> values, string field, int least, int most, string reason)
    {
        int? value = values.TryGetValue(field, out int found) ? found : null;
        int read = reader.RequireInt(value, depth, field);
        if (read < least || read > most)
        {
            throw reader.RefuseField(depth, field, $"the value is {read}, and it takes {least} to {most}: {reason}");
        }

        return read;
    }
}
