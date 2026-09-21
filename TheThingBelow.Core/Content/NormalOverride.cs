using System;
using System.Collections.Generic;

namespace TheThingBelow.Core.Content;

/// <summary>One frame of an override grid, with one character for each pixel (D-839).</summary>
/// <param name="Rows">
/// The rows of the frame, one string for each row. A numpad digit sets the direction of its
/// pixel, and the dot keeps the built normal (D-839).
/// </param>
public sealed record NormalOverrideFrame(IReadOnlyList<string> Rows);

/// <summary>
/// The override grid of one drawing: the direction of each pixel where the built normal map
/// looks wrong (D-184, D-839). The file is optional, and most drawings have none.
/// </summary>
/// <remarks>
/// The grid lives in a file of its own, so no drawing file holds an optional field. An absent
/// field is an error (D-116), and an absent file means that the built normal map stands.
/// <para>
/// The characters are the digits of a numpad, as the arrows of the keys point: 8 up, 2 down,
/// 4 left, 6 right, 7, 9, 1, and 3 the four diagonals, and 5 flat toward the viewer (D-839).
/// </para>
/// <para>
/// Core holds the record because Core holds the record of every content file (D-517). The
/// normal-map command of Tools reads it, and no rule reads it.
/// </para>
/// </remarks>
public sealed class NormalOverride
{
    /// <summary>The folder of the override files, under `content/`.</summary>
    public const string Folder = "sprites/normals/";

    /// <summary>The one character that keeps the built normal of its pixel.</summary>
    public const char Keep = '.';

    /// <summary>The digit that faces the viewer, with no tilt (D-839).</summary>
    public const char Flat = '5';

    private NormalOverride(string file, ContentId drawing, IReadOnlyList<NormalOverrideFrame> frames)
    {
        this.File = file;
        this.Drawing = drawing;
        this.Frames = frames;
    }

    /// <summary>The path of the file, under `content/`, which every error names (T-2).</summary>
    public string File { get; }

    /// <summary>The id of the drawing that this grid corrects, such as `drawing.marrek_map_front`.</summary>
    public ContentId Drawing { get; }

    /// <summary>One grid for each frame of the drawing, in the order of the drawing.</summary>
    public IReadOnlyList<NormalOverrideFrame> Frames { get; }

    /// <summary>Tells whether a content path is an override file.</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True when the path lies in the override folder.</returns>
    public static bool IsOverrideFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return path.StartsWith(Folder, StringComparison.Ordinal);
    }

    /// <summary>Tells whether a character names a direction: a digit from 1 to 9 (D-839).</summary>
    /// <param name="character">The character of one pixel of the grid.</param>
    /// <returns>True for a digit from 1 to 9.</returns>
    public static bool IsDirection(char character) => character is >= '1' and <= '9';

    /// <summary>Reads one override grid from the bytes of its file.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The override grid.</returns>
    /// <exception cref="ContentException">The file breaks a rule of the reader (G-6, T-2).</exception>
    public static NormalOverride Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        NormalOverride grid = Read(ref reader);
        reader.ReadFileEnd();
        return grid;
    }

    /// <summary>
    /// Refuses a grid that does not fit its drawing: another count of frames, another size,
    /// a direction on a transparent pixel, or a drawing that takes no scene light.
    /// </summary>
    /// <param name="drawing">The drawing that <see cref="Drawing"/> names.</param>
    /// <exception cref="ContentException">The grid does not fit the drawing (T-2).</exception>
    /// <remarks>
    /// A direction on a transparent pixel changes no pixel of the game, so it is an error and
    /// never a silent no-op. A portrait, an icon, and a window frame take no scene light, so
    /// they have no normal map (D-210).
    /// </remarks>
    public void RefuseWrongShape(Drawing drawing)
    {
        ArgumentNullException.ThrowIfNull(drawing);

        if (string.CompareOrdinal(drawing.Id.Value, this.Drawing.Value) != 0)
        {
            throw ContentException.ForField(
                this.File,
                "drawing",
                $"the grid names '{this.Drawing.Value}', and the check read the drawing '{drawing.Id.Value}'");
        }

        if (!AtlasPages.TakesLight(drawing.Page))
        {
            throw ContentException.ForField(
                this.File,
                "drawing",
                $"the drawing '{drawing.Id.Value}' lies on the page '{AtlasPages.NameOf(drawing.Page)}', which takes no scene light and has no normal map (D-210)");
        }

        if (this.Frames.Count != drawing.Frames.Count)
        {
            throw ContentException.ForField(
                this.File,
                "frames",
                $"the grid holds {this.Frames.Count} frames, and the drawing '{drawing.Id.Value}' holds {drawing.Frames.Count}");
        }

        for (int frame = 0; frame < this.Frames.Count; frame += 1)
        {
            this.RefuseWrongFrame(drawing, frame);
        }
    }

    private void RefuseWrongFrame(Drawing drawing, int frame)
    {
        IReadOnlyList<string> rows = this.Frames[frame].Rows;
        IReadOnlyList<string> keys = drawing.Frames[frame].Rows;
        if (rows.Count != drawing.Height)
        {
            throw ContentException.ForField(
                this.File,
                $"frames[{frame}].rows",
                $"the grid holds {rows.Count} rows, and the drawing '{drawing.Id.Value}' is {drawing.Height} pixels high");
        }

        for (int row = 0; row < rows.Count; row += 1)
        {
            string line = rows[row];
            if (line.Length != drawing.Width)
            {
                throw ContentException.ForField(
                    this.File,
                    $"frames[{frame}].rows[{row}]",
                    $"the row holds {line.Length} pixels, and the drawing '{drawing.Id.Value}' is {drawing.Width} pixels wide");
            }

            for (int column = 0; column < line.Length; column += 1)
            {
                if (IsDirection(line[column]) && keys[row][column] == Content.Drawing.Transparent)
                {
                    throw ContentException.ForField(
                        this.File,
                        $"frames[{frame}].rows[{row}]",
                        $"column {column} sets a direction on a transparent pixel of the drawing, which changes no pixel (T-2)");
                }
            }
        }
    }

    private static NormalOverride Read(ref ContentReader reader)
    {
        ContentId? drawing = null;
        List<NormalOverrideFrame>? frames = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "drawing":
                    drawing = reader.ReadContentId(Content.Drawing.IdKind);
                    break;
                case "frames":
                    frames = ReadFrames(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        List<NormalOverrideFrame> read = reader.Require(frames, depth, "frames");
        if (read.Count == 0)
        {
            throw reader.RefuseField(depth, "frames", "a grid holds one frame at least, and this list is empty (D-839)");
        }

        return new NormalOverride(reader.File, reader.Require(drawing, depth, "drawing"), read);
    }

    private static List<NormalOverrideFrame> ReadFrames(ref ContentReader reader)
    {
        var frames = new List<NormalOverrideFrame>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, frames.Count))
        {
            frames.Add(ReadFrame(ref reader));
        }

        return frames;
    }

    private static NormalOverrideFrame ReadFrame(ref ContentReader reader)
    {
        List<string>? rows = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "rows":
                    rows = ReadRows(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new NormalOverrideFrame(reader.Require(rows, depth, "rows"));
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
            char character = row[column];
            if (character != Keep && !IsDirection(character))
            {
                throw reader.Refuse(
                    $"column {column} holds '{character}', and a grid holds a digit from 1 to 9 or the dot (D-839)");
            }
        }

        return row;
    }
}
