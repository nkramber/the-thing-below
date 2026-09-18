using System;
using System.Collections.Generic;

namespace TheThingBelow.Core.Content;

/// <summary>One color of the palette, as `content/sprites/palette.json` holds it.</summary>
/// <param name="Index">The position of the color in the palette, which starts at zero.</param>
/// <param name="Key">The one character that a sprite grid writes for this color (D-107).</param>
/// <param name="Hex">The color as six hexadecimal digits, such as `0b0a0f`.</param>
/// <param name="Name">The name of the color, such as `ink`, for a review sheet and a diff.</param>
public sealed record PaletteColor(int Index, string Key, string Hex, string Name);

/// <summary>
/// The palette of the game (D-89, D-121). No rule reads it, and Core holds its record
/// because Core holds the record of every content file (D-517).
/// </summary>
/// <remarks>
/// The palette stays outside the content hash, so a color change never breaks a stored run
/// record or a replay fixture (D-495). The file lives outside `content/rules/` (D-648).
/// </remarks>
public sealed class Palette
{
    /// <summary>The path of the palette file, under `content/`.</summary>
    public const string Path = "sprites/palette.json";

    private Palette(string comment, IReadOnlyList<PaletteColor> colors)
    {
        this.Comment = comment;
        this.Colors = colors;
    }

    /// <summary>The note at the top of the file, which names the decisions of the palette.</summary>
    public string Comment { get; }

    /// <summary>Every color, in the order of the file.</summary>
    public IReadOnlyList<PaletteColor> Colors { get; }

    /// <summary>Reads the palette from the bytes of its file.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The palette.</returns>
    /// <exception cref="ContentException">The file breaks a rule of the reader (G-6, T-2).</exception>
    public static Palette Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        Palette palette = Read(ref reader);
        reader.ReadFileEnd();
        return palette;
    }

    private static Palette Read(ref ContentReader reader)
    {
        string? comment = null;
        List<PaletteColor>? colors = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "colors":
                    colors = ReadColors(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new Palette(
            reader.Require(comment, depth, "comment"),
            reader.Require(colors, depth, "colors"));
    }

    private static List<PaletteColor> ReadColors(ref ContentReader reader)
    {
        var colors = new List<PaletteColor>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, colors.Count))
        {
            colors.Add(ReadColor(ref reader));
        }

        return colors;
    }

    private static PaletteColor ReadColor(ref ContentReader reader)
    {
        int? index = null;
        string? key = null;
        string? hex = null;
        string? name = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "index":
                    index = reader.ReadInt();
                    break;
                case "key":
                    key = reader.ReadString();
                    break;
                case "hex":
                    hex = reader.ReadString();
                    break;
                case "name":
                    name = reader.ReadString();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new PaletteColor(
            reader.RequireInt(index, depth, "index"),
            reader.Require(key, depth, "key"),
            reader.Require(hex, depth, "hex"),
            reader.Require(name, depth, "name"));
    }
}
