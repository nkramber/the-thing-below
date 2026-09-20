using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TheThingBelow.Core.Content;

/// <summary>One color of the palette, as `content/sprites/palette.json` holds it.</summary>
/// <param name="Index">The position of the color in the palette, which starts at zero.</param>
/// <param name="Key">The one character that a drawing file writes for this color (D-107).</param>
/// <param name="Hex">The color as six lowercase hexadecimal digits, such as `0b0a0f`.</param>
/// <param name="Name">The name of the color, such as `ink`, for a review sheet and a diff.</param>
/// <remarks>
/// The reader of <see cref="Palette"/> checks the form of the key and of the hexadecimal
/// text, so <see cref="Red"/>, <see cref="Green"/>, and <see cref="Blue"/> always read a
/// color that came from a file (T-2).
/// </remarks>
public sealed record PaletteColor(int Index, string Key, string Hex, string Name)
{
    /// <summary>The red part of the color, from 0 to 255.</summary>
    public int Red => Channel(this.Hex, 0);

    /// <summary>The green part of the color, from 0 to 255.</summary>
    public int Green => Channel(this.Hex, 2);

    /// <summary>The blue part of the color, from 0 to 255.</summary>
    public int Blue => Channel(this.Hex, 4);

    /// <summary>The one character of the key, which a drawing file writes for this color.</summary>
    public char KeyCharacter => this.Key[0];

    /// <summary>Tells whether the text is six lowercase hexadecimal digits.</summary>
    /// <param name="hex">The text of the `hex` field.</param>
    /// <returns>True when the text takes the form of a color.</returns>
    public static bool IsWellFormedHex(string hex)
    {
        ArgumentNullException.ThrowIfNull(hex);

        if (hex.Length != 6)
        {
            return false;
        }

        foreach (char digit in hex)
        {
            bool legal = digit is (>= '0' and <= '9') or (>= 'a' and <= 'f');
            if (!legal)
            {
                return false;
            }
        }

        return true;
    }

    private static int Channel(string hex, int start)
    {
        if (!IsWellFormedHex(hex))
        {
            throw new ArgumentException(
                $"The color '{hex}' is not six lowercase hexadecimal digits, and a color of a file passes the reader (T-2).",
                nameof(hex));
        }

        return (Digit(hex[start]) * 16) + Digit(hex[start + 1]);
    }

    private static int Digit(char value) => value <= '9' ? value - '0' : value - 'a' + 10;
}

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

    private readonly SortedDictionary<char, PaletteColor> byKey;

    private Palette(
        string comment,
        IReadOnlyList<PaletteColor> colors,
        SortedDictionary<char, PaletteColor> byKey)
    {
        this.Comment = comment;
        this.Colors = colors;
        this.byKey = byKey;
    }

    /// <summary>The note at the top of the file, which names the decisions of the palette.</summary>
    public string Comment { get; }

    /// <summary>Every color, in the order of the file.</summary>
    public IReadOnlyList<PaletteColor> Colors { get; }

    /// <summary>Gives the color of one key of a drawing file.</summary>
    /// <param name="key">The one character that the row of a frame holds.</param>
    /// <param name="found">The color, when the palette holds the key.</param>
    /// <returns>True when the palette holds the key, and false for every other character.</returns>
    /// <remarks>
    /// The dot is transparent and belongs to no color, so this method gives false for it
    /// (D-107, <see cref="Drawing.Transparent"/>).
    /// </remarks>
    public bool TryColorOf(char key, [NotNullWhen(true)] out PaletteColor? found) =>
        this.byKey.TryGetValue(key, out found);

    /// <summary>Gives the color of one name, such as `night`.</summary>
    /// <param name="name">The name of the color, as the file holds it.</param>
    /// <returns>The color.</returns>
    /// <exception cref="ContentException">The palette holds no color of that name (T-2).</exception>
    /// <remarks>
    /// A tool that needs a fixed color, such as the ground of a review sheet, reads it by
    /// name, because a name says what the color is and a key does not (D-668, T-1).
    /// </remarks>
    public PaletteColor ColorNamed(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        foreach (PaletteColor color in this.Colors)
        {
            if (string.CompareOrdinal(color.Name, name) == 0)
            {
                return color;
            }
        }

        throw ContentException.ForField(Path, name, "the palette holds no color with this name");
    }

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

        return Build(
            ref reader,
            depth,
            reader.Require(comment, depth, "comment"),
            reader.Require(colors, depth, "colors"));
    }

    /// <summary>
    /// Checks the whole palette, and builds the lookup by key. A repeated key and a repeated
    /// index each fail with the key and the index, which F-20 found in the interim tool.
    /// </summary>
    private static Palette Build(ref ContentReader reader, int depth, string comment, List<PaletteColor> colors)
    {
        // A char key compares by its code, so the order needs no culture rule (F-39).
        var byKey = new SortedDictionary<char, PaletteColor>();
        for (int position = 0; position < colors.Count; position += 1)
        {
            PaletteColor color = colors[position];
            if (color.Index != position)
            {
                throw reader.RefuseField(
                    depth,
                    $"colors[{position}].index",
                    $"the color '{color.Key}' holds the index {color.Index}, and it is color {position} of the file (D-181)");
            }

            RefuseWrongKey(ref reader, depth, position, color);
            if (!PaletteColor.IsWellFormedHex(color.Hex))
            {
                throw reader.RefuseField(
                    depth,
                    $"colors[{position}].hex",
                    $"the color '{color.Hex}' is not six lowercase hexadecimal digits");
            }

            if (!byKey.TryAdd(color.KeyCharacter, color))
            {
                throw reader.RefuseField(
                    depth,
                    $"colors[{position}].key",
                    $"the key '{color.Key}' is already the key of the color at index {byKey[color.KeyCharacter].Index} (F-20)");
            }
        }

        return new Palette(comment, colors, byKey);
    }

    private static void RefuseWrongKey(ref ContentReader reader, int depth, int position, PaletteColor color)
    {
        // A key is one character, and JSON writes a quote mark and a backslash as two
        // characters, so neither one is a key (D-515). The dot is transparent (D-107).
        string field = $"colors[{position}].key";
        if (color.Key.Length != 1)
        {
            throw reader.RefuseField(
                depth,
                field,
                $"the key '{color.Key}' holds {color.Key.Length} characters, and a key is one character (D-515)");
        }

        char key = color.KeyCharacter;
        if (key == Drawing.Transparent)
        {
            throw reader.RefuseField(depth, field, "the dot is transparent, and no color takes it (D-107)");
        }

        if (key is '"' or '\\')
        {
            throw reader.RefuseField(
                depth,
                field,
                $"JSON writes '{key}' as two characters, and no key is one of them (D-515)");
        }

        if (key is < '!' or > '~')
        {
            throw reader.RefuseField(
                depth,
                field,
                $"the key is the character {(int)key}, and a key is one printable character of ASCII (D-515)");
        }
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
