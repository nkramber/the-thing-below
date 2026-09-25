using System;
using System.Collections.Generic;
using System.Text;

namespace TheThingBelow.Core.Content;

/// <summary>
/// The sizes of the glyph bitmaps that a font file carries (D-710, F-49). Terminus TTF
/// carries a bitmap strike for each size of <see cref="Sizes"/>, and a traced outline for
/// every other size.
/// </summary>
/// <remarks>
/// A rasterizer that gets a size with no strike falls back to the outline in silence, and
/// the glyph then loses its square pixel. The fallback looks almost right, which cost a
/// session a day (F-49). Thus Game asks this record for the size before it builds a font,
/// and <see cref="RequireSize"/> turns the silent fallback into an error (T-2).
/// <para>
/// The record reads the `EBLC` table of the file, which holds one 48-byte record for each
/// strike. The last four bytes of each record are the horizontal size in pixels, the
/// vertical size in pixels, the bit depth, and the flags. Source: the OpenType
/// specification, the `EBLC` chapter, read 2026-09-20.
/// </para>
/// <para>
/// No rule of the game reads a font, so the content hash covers no font file (D-495,
/// D-648). Core holds this reader because Core holds the record of every content file
/// (D-517).
/// </para>
/// </remarks>
public sealed class FontStrikes
{
    /// <summary>The path of the body font, under `content/` (D-263, D-713).</summary>
    public const string BodyPath = "fonts/TerminusTTF.ttf";

    /// <summary>The path of the title font, under `content/` (D-264, D-713).</summary>
    public const string TitlePath = "fonts/TerminusTTF-Bold.ttf";

    /// <summary>The four bytes that name the table of the bitmap sizes.</summary>
    private const string SizeTableTag = "EBLC";

    /// <summary>The count of bytes of one record of the bitmap size table.</summary>
    private const int SizeRecordLength = 48;

    /// <summary>The count of bytes of one record of the table directory.</summary>
    private const int DirectoryRecordLength = 16;

    /// <summary>The bit depth of a strike of a pixel font: one bit for each pixel.</summary>
    private const int PixelFontBitDepth = 1;

    private readonly List<int> sizes;

    private FontStrikes(string file, List<int> sizes)
    {
        this.File = file;
        this.sizes = sizes;
    }

    /// <summary>The path of the font file, under `content/`.</summary>
    public string File { get; }

    /// <summary>Every bitmap size of the file, in pixels, from the smallest to the largest.</summary>
    public IReadOnlyList<int> Sizes => this.sizes;

    /// <summary>The largest bitmap size of the file, in pixels.</summary>
    public int LargestSize => this.sizes[this.sizes.Count - 1];

    /// <summary>Reads the bitmap sizes from the bytes of a font file.</summary>
    /// <param name="bytes">The bytes of the file.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The sizes of the file.</returns>
    /// <exception cref="ContentException">
    /// The bytes are not a font file, the file carries no bitmap table, or a strike is not
    /// square or not one bit deep (T-2).
    /// </exception>
    public static FontStrikes Read(ReadOnlySpan<byte> bytes, string file)
    {
        ArgumentException.ThrowIfNullOrEmpty(file);

        int tableStart = FindSizeTable(bytes, file);
        int count = (int)ReadUInt32(bytes, tableStart + 4, file, "the count of bitmap sizes");

        // The count comes from the file, so the reader checks it against the length before
        // it makes a list of that size. A broken file would otherwise ask for the memory of
        // four thousand million records (T-2).
        if (count < 0 || (long)tableStart + 8 + ((long)count * SizeRecordLength) > bytes.Length)
        {
            throw ContentException.ForField(
                file,
                SizeTableTag,
                $"the table names {count} bitmap sizes, and the file holds {bytes.Length} bytes");
        }

        var sizes = new List<int>(count);
        for (int strike = 0; strike < count; strike += 1)
        {
            sizes.Add(ReadStrikeSize(bytes, tableStart + 8 + (strike * SizeRecordLength), strike, file));
        }

        if (sizes.Count == 0)
        {
            throw ContentException.ForFile(
                file,
                $"the '{SizeTableTag}' table holds no bitmap size, and a pixel font carries one for each size it draws (F-49)");
        }

        sizes.Sort();
        RefuseRepeatedSize(sizes, file);
        return new FontStrikes(file, sizes);
    }

    /// <summary>Tells whether the file carries a bitmap of one size.</summary>
    /// <param name="size">The size in pixels, such as 24.</param>
    /// <returns>True when a strike of that size exists.</returns>
    public bool Has(int size) => this.sizes.Contains(size);

    /// <summary>
    /// Refuses a size that the file has no bitmap for. Game calls this before it builds a
    /// font, so no glyph ever draws from the traced outline (F-49, T-2).
    /// </summary>
    /// <param name="size">The size in pixels that the caller wants to draw.</param>
    /// <exception cref="ContentException">The file carries no bitmap of that size (T-2).</exception>
    public void RequireSize(int size)
    {
        if (this.Has(size))
        {
            return;
        }

        throw ContentException.ForField(
            this.File,
            SizeTableTag,
            $"the font carries no bitmap of {size} pixels, and it carries {this.Describe()}. "
            + "A size with no bitmap draws the traced outline, which loses the square pixel (F-49, D-710)");
    }

    /// <summary>Gives every size as one line, for an error message and a review sheet (T-2).</summary>
    /// <returns>The sizes, separated by a comma and a space.</returns>
    public string Describe()
    {
        StringBuilder text = new();
        for (int size = 0; size < this.sizes.Count; size += 1)
        {
            if (size > 0)
            {
                text.Append(", ");
            }

            text.Append(this.sizes[size]);
        }

        return text.ToString();
    }

    /// <summary>
    /// Finds the start of the bitmap size table in the table directory of the file. The
    /// directory holds a 12-byte header, then one 16-byte record for each table: the four
    /// bytes of the tag, the checksum, the start, and the length.
    /// </summary>
    private static int FindSizeTable(ReadOnlySpan<byte> bytes, string file)
    {
        int count = (int)ReadUInt16(bytes, 4, file, "the count of tables");
        for (int table = 0; table < count; table += 1)
        {
            int record = 12 + (table * DirectoryRecordLength);
            if (!HasTag(bytes, record, file))
            {
                continue;
            }

            int start = (int)ReadUInt32(bytes, record + 8, file, $"the start of the '{SizeTableTag}' table");
            if (start < 0 || (long)start + 8 > bytes.Length)
            {
                throw ContentException.ForField(
                    file,
                    SizeTableTag,
                    $"the table starts at byte {start}, and the file holds {bytes.Length} bytes");
            }

            return start;
        }

        throw ContentException.ForFile(
            file,
            $"the font carries no '{SizeTableTag}' table, so it carries no glyph bitmap. "
            + "Every font of this game is a pixel font with its own bitmaps (D-263, F-49)");
    }

    private static bool HasTag(ReadOnlySpan<byte> bytes, int record, string file)
    {
        if (record + DirectoryRecordLength > bytes.Length)
        {
            throw ContentException.ForFile(
                file,
                $"the table directory reaches byte {record + DirectoryRecordLength}, and the file holds {bytes.Length} bytes");
        }

        for (int letter = 0; letter < SizeTableTag.Length; letter += 1)
        {
            if (bytes[record + letter] != (byte)SizeTableTag[letter])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Reads one record of the bitmap size table, and gives the size of that strike in
    /// pixels. The record ends with the horizontal size, the vertical size, the bit depth,
    /// and the flags, one byte each.
    /// </summary>
    private static int ReadStrikeSize(ReadOnlySpan<byte> bytes, int record, int strike, string file)
    {
        if (record + SizeRecordLength > bytes.Length)
        {
            throw ContentException.ForField(
                file,
                $"{SizeTableTag}[{strike}]",
                $"the record reaches byte {record + SizeRecordLength}, and the file holds {bytes.Length} bytes");
        }

        int across = bytes[record + 44];
        int down = bytes[record + 45];
        int depth = bytes[record + 46];

        if (across != down)
        {
            throw ContentException.ForField(
                file,
                $"{SizeTableTag}[{strike}]",
                $"the strike is {across} by {down} pixels, and a square pixel needs the two sizes equal (D-230)");
        }

        if (across <= 0)
        {
            throw ContentException.ForField(
                file,
                $"{SizeTableTag}[{strike}]",
                $"the strike is {across} pixels, and a size is above zero");
        }

        if (depth != PixelFontBitDepth)
        {
            throw ContentException.ForField(
                file,
                $"{SizeTableTag}[{strike}]",
                $"the strike is {depth} bits deep, and a pixel font of this game is {PixelFontBitDepth} bit deep (D-263)");
        }

        return across;
    }

    private static void RefuseRepeatedSize(List<int> sizes, string file)
    {
        for (int size = 1; size < sizes.Count; size += 1)
        {
            if (sizes[size] == sizes[size - 1])
            {
                throw ContentException.ForField(
                    file,
                    SizeTableTag,
                    $"the font carries two bitmap strikes of {sizes[size]} pixels, and a size names one strike");
            }
        }
    }

    private static uint ReadUInt32(ReadOnlySpan<byte> bytes, int start, string file, string what)
    {
        RequireBytes(bytes, start, 4, file, what);

        // A font file writes each number with the high byte first.
        return ((uint)bytes[start] << 24)
            | ((uint)bytes[start + 1] << 16)
            | ((uint)bytes[start + 2] << 8)
            | bytes[start + 3];
    }

    private static uint ReadUInt16(ReadOnlySpan<byte> bytes, int start, string file, string what)
    {
        RequireBytes(bytes, start, 2, file, what);

        return ((uint)bytes[start] << 8) | bytes[start + 1];
    }

    private static void RequireBytes(ReadOnlySpan<byte> bytes, int start, int length, string file, string what)
    {
        if (start < 0 || (long)start + length > bytes.Length)
        {
            throw ContentException.ForFile(
                file,
                $"{what} sits at byte {start}, and the file holds {bytes.Length} bytes. The file is not a font (T-2)");
        }
    }
}
