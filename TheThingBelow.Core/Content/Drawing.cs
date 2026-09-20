using System;
using System.Collections.Generic;

namespace TheThingBelow.Core.Content;

/// <summary>One thing that a drawing draws (D-519).</summary>
/// <param name="Content">
/// The content id of the thing, such as `cast.marrek` or `tile.snow_floor`. A rule file
/// never names art, so the art file carries the link (D-519).
/// </param>
/// <param name="Use">
/// The part that this drawing serves, such as `map_front` or `portrait`. One thing can have
/// more than one drawing, and the use tells them apart (D-519).
/// </param>
public sealed record DrawingUse(ContentId Content, string Use);

/// <summary>One frame of a drawing (D-515, D-669).</summary>
/// <param name="Ticks">
/// The time of the frame in ticks, 60 to a second (D-669). A drawing of one frame holds 0,
/// which means that the frame never advances.
/// </param>
/// <param name="Rows">
/// The rows of the frame, one string for each row, with one palette key for each pixel
/// (D-515). The dot is transparent.
/// </param>
public sealed record DrawingFrame(int Ticks, IReadOnlyList<string> Rows);

/// <summary>
/// One drawing of the game: a tile, a sprite, a portrait, or a piece of a large picture
/// (D-515). The drawing files are the source of the atlas, and nobody edits the atlas by
/// hand (D-107).
/// </summary>
/// <remarks>
/// Core holds this record because Core holds the record of every content file, a file that
/// no rule reads included (D-517). No rule file names a drawing, so new art never changes
/// the content hash and never breaks a replay fixture (D-495, D-519).
/// <para>
/// The reader checks the shape of the file alone. The palette keys of the rows need the
/// palette, and <see cref="ContentSet"/> checks them over the whole set.
/// </para>
/// </remarks>
public sealed class Drawing
{
    /// <summary>The folder of the drawing files, under `content/`.</summary>
    public const string Folder = "sprites/drawings/";

    /// <summary>The kind of the id of every drawing file (D-646).</summary>
    public const string IdKind = "drawing";

    /// <summary>The one character that names a pixel which no color covers (D-107).</summary>
    public const char Transparent = '.';

    private Drawing(
        string file,
        ContentId id,
        AtlasPageKind page,
        int width,
        int height,
        IReadOnlyList<DrawingUse> draws,
        IReadOnlyList<DrawingFrame> frames)
    {
        this.File = file;
        this.Id = id;
        this.Page = page;
        this.Width = width;
        this.Height = height;
        this.Draws = draws;
        this.Frames = frames;
    }

    /// <summary>The path of the file, under `content/`, which every error names (T-2).</summary>
    public string File { get; }

    /// <summary>The permanent id of the drawing, such as `drawing.marrek_map_front`.</summary>
    public ContentId Id { get; }

    /// <summary>The atlas page that holds every frame of this drawing (D-666).</summary>
    public AtlasPageKind Page { get; }

    /// <summary>The count of pixels in one row of a frame.</summary>
    public int Width { get; }

    /// <summary>The count of rows of a frame.</summary>
    public int Height { get; }

    /// <summary>Every thing that this drawing draws, in the order of the file (D-519).</summary>
    public IReadOnlyList<DrawingUse> Draws { get; }

    /// <summary>Every frame, in the order that the animation plays them.</summary>
    public IReadOnlyList<DrawingFrame> Frames { get; }

    /// <summary>Tells whether a content path is a drawing file.</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True when the path lies in the drawing folder.</returns>
    public static bool IsDrawingFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return path.StartsWith(Folder, StringComparison.Ordinal);
    }

    /// <summary>Reads one drawing from the bytes of its file.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The drawing.</returns>
    /// <exception cref="ContentException">The file breaks a rule of the reader (G-6, T-2).</exception>
    public static Drawing Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        Drawing drawing = Read(ref reader);
        reader.ReadFileEnd();
        return drawing;
    }

    private static Drawing Read(ref ContentReader reader)
    {
        ContentId? id = null;
        AtlasPageKind? page = null;
        int? width = null;
        int? height = null;
        List<DrawingUse>? draws = null;
        List<DrawingFrame>? frames = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(IdKind);
                    break;
                case "page":
                    page = ReadPage(ref reader);
                    break;
                case "width":
                    width = ReadSize(ref reader);
                    break;
                case "height":
                    height = ReadSize(ref reader);
                    break;
                case "draws":
                    draws = ReadDraws(ref reader);
                    break;
                case "frames":
                    frames = ReadFrames(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        var drawing = new Drawing(
            reader.File,
            reader.Require(id, depth, "id"),
            reader.RequireValue(page, depth, "page"),
            reader.RequireInt(width, depth, "width"),
            reader.RequireInt(height, depth, "height"),
            reader.Require(draws, depth, "draws"),
            reader.Require(frames, depth, "frames"));

        drawing.RefuseWrongShape(ref reader, depth);
        return drawing;
    }

    private static AtlasPageKind ReadPage(ref ContentReader reader)
    {
        string name = reader.ReadString();
        if (!AtlasPages.TryParse(name, out AtlasPageKind kind))
        {
            throw reader.Refuse($"the page '{name}' is not one of {AtlasPages.Names()} (D-666)");
        }

        return kind;
    }

    private static int ReadSize(ref ContentReader reader)
    {
        int value = reader.ReadInt();
        if (value < 1 || value > AtlasPages.Size)
        {
            throw reader.Refuse(
                $"the size {value} is outside 1 to {AtlasPages.Size}, the size of an atlas page (D-666)");
        }

        return value;
    }

    private static List<DrawingUse> ReadDraws(ref ContentReader reader)
    {
        var draws = new List<DrawingUse>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, draws.Count))
        {
            draws.Add(ReadUse(ref reader));
        }

        return draws;
    }

    private static DrawingUse ReadUse(ref ContentReader reader)
    {
        ContentId? content = null;
        string? use = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "content":
                    content = reader.ReadContentId();
                    break;
                case "use":
                    use = ReadUseName(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new DrawingUse(
            reader.Require(content, depth, "content"),
            reader.Require(use, depth, "use"));
    }

    internal static string ReadUseName(ref ContentReader reader)
    {
        string use = reader.ReadString();
        if (!IsWellFormedUse(use))
        {
            throw reader.Refuse(
                $"the use '{use}' is not a lowercase name of letters, digits, and underscores (D-519)");
        }

        return use;
    }

    // The same character set as the name of a content id (D-646), so one drawing file reads
    // with one rule for every name that it holds.
    internal static bool IsWellFormedUse(string use)
    {
        if (use.Length == 0 || use[0] is < 'a' or > 'z')
        {
            return false;
        }

        foreach (char character in use)
        {
            bool legal = character is (>= 'a' and <= 'z') or (>= '0' and <= '9') or '_';
            if (!legal)
            {
                return false;
            }
        }

        return true;
    }

    private static List<DrawingFrame> ReadFrames(ref ContentReader reader)
    {
        var frames = new List<DrawingFrame>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, frames.Count))
        {
            frames.Add(ReadFrame(ref reader));
        }

        return frames;
    }

    private static DrawingFrame ReadFrame(ref ContentReader reader)
    {
        int? ticks = null;
        List<string>? rows = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "ticks":
                    ticks = ReadTicks(ref reader);
                    break;
                case "rows":
                    rows = ReadRows(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new DrawingFrame(
            reader.RequireInt(ticks, depth, "ticks"),
            reader.Require(rows, depth, "rows"));
    }

    internal static int ReadTicks(ref ContentReader reader)
    {
        int value = reader.ReadInt();
        if (value < 0)
        {
            throw reader.Refuse($"the time {value} is below zero, and a frame holds ticks (D-669)");
        }

        return value;
    }

    private static List<string> ReadRows(ref ContentReader reader)
    {
        var rows = new List<string>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, rows.Count))
        {
            rows.Add(ReadRow(ref reader));
        }

        return rows;
    }

    private static string ReadRow(ref ContentReader reader)
    {
        string row = reader.ReadString();
        for (int column = 0; column < row.Length; column += 1)
        {
            // A key is one printable character of ASCII, so the count of characters of a row
            // is the count of its pixels and a column number points at the pixel (D-515, T-2).
            char key = row[column];
            if (key is < '!' or > '~')
            {
                throw reader.Refuse(
                    $"column {column} holds the character {(int)key}, and a palette key is one printable character of ASCII (D-515)");
            }
        }

        return row;
    }

    private void RefuseWrongShape(ref ContentReader reader, int depth)
    {
        this.RefuseWrongPageSize(ref reader, depth);
        this.RefuseRepeatedUse(ref reader, depth);
        this.RefuseEmptyFrames(ref reader, depth);
        this.RefuseWrongRows(ref reader, depth);
    }

    private void RefuseWrongPageSize(ref ContentReader reader, int depth)
    {
        // A tile page is a grid of 32 by 32 cells, so a tile of another size has no cell
        // (D-667). Every other page packs a drawing of any size up to the page.
        bool wrongTile = this.Page == AtlasPageKind.Tiles &&
            (this.Width != AtlasPages.TileSize || this.Height != AtlasPages.TileSize);
        if (wrongTile)
        {
            throw reader.RefuseField(
                depth,
                "width",
                $"a drawing of the tiles page is {AtlasPages.TileSize} by {AtlasPages.TileSize} pixels, and this one is {this.Width} by {this.Height} (D-667)");
        }
    }

    private void RefuseRepeatedUse(ref ContentReader reader, int depth)
    {
        if (this.Draws.Count == 0)
        {
            throw reader.RefuseField(depth, "draws", "a drawing names the things that it draws, and this list is empty (D-519)");
        }

        for (int later = 1; later < this.Draws.Count; later += 1)
        {
            for (int first = 0; first < later; first += 1)
            {
                DrawingUse one = this.Draws[first];
                DrawingUse other = this.Draws[later];
                bool same = string.CompareOrdinal(one.Content.Value, other.Content.Value) == 0 &&
                    string.CompareOrdinal(one.Use, other.Use) == 0;
                if (same)
                {
                    throw reader.RefuseField(
                        depth,
                        $"draws[{later}]",
                        $"the file already draws '{other.Content.Value}' as '{other.Use}' at draws[{first}] (D-519)");
                }
            }
        }
    }

    private void RefuseEmptyFrames(ref ContentReader reader, int depth)
    {
        if (this.Frames.Count == 0)
        {
            throw reader.RefuseField(depth, "frames", "a drawing holds one frame at least, and this list is empty (D-515)");
        }

        for (int index = 0; index < this.Frames.Count; index += 1)
        {
            int ticks = this.Frames[index].Ticks;

            // One frame never advances, and a drawing of more than one frame must move, so
            // a time of zero inside an animation is an error and never a stop (D-669, T-2).
            if (this.Frames.Count == 1 && ticks != 0)
            {
                throw reader.RefuseField(
                    depth,
                    "frames[0].ticks",
                    $"a drawing of one frame holds 0 ticks, and this one holds {ticks} (D-669)");
            }

            if (this.Frames.Count > 1 && ticks < 1)
            {
                throw reader.RefuseField(
                    depth,
                    $"frames[{index}].ticks",
                    $"a frame of an animation holds 1 tick at least, and this one holds {ticks} (D-669)");
            }
        }
    }

    private void RefuseWrongRows(ref ContentReader reader, int depth)
    {
        for (int index = 0; index < this.Frames.Count; index += 1)
        {
            IReadOnlyList<string> rows = this.Frames[index].Rows;
            if (rows.Count != this.Height)
            {
                throw reader.RefuseField(
                    depth,
                    $"frames[{index}].rows",
                    $"the frame holds {rows.Count} rows, and the height of the drawing is {this.Height}");
            }

            for (int row = 0; row < rows.Count; row += 1)
            {
                if (rows[row].Length != this.Width)
                {
                    throw reader.RefuseField(
                        depth,
                        $"frames[{index}].rows[{row}]",
                        $"the row holds {rows[row].Length} keys, and the width of the drawing is {this.Width}");
                }
            }
        }
    }
}
