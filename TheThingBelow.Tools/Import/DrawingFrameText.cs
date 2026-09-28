using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Tools.Import;

/// <summary>
/// Replaces the rows of one frame in the text of a drawing file. Every other byte of the
/// file stays, so the id, the page, the draws, and each tick keep their text (D-1311).
/// </summary>
public static class DrawingFrameText
{
    /// <summary>Gives the text of a drawing file with new rows for one frame.</summary>
    /// <param name="bytes">The bytes of the drawing file, as UTF-8.</param>
    /// <param name="file">The path of the drawing file, for each error.</param>
    /// <param name="frame">The position of the frame, which starts at 0.</param>
    /// <param name="rows">The new rows of the frame.</param>
    /// <returns>The bytes of the new file. The reader of Core reads them back to these rows.</returns>
    /// <exception cref="ImportException">
    /// The file holds no such frame, or the new file does not read back to the rows (T-2).
    /// </exception>
    public static byte[] ReplaceRows(byte[] bytes, string file, int frame, IReadOnlyList<string> rows)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        ArgumentException.ThrowIfNullOrEmpty(file);
        ArgumentNullException.ThrowIfNull(rows);

        (int name, int open, int close) = FindRows(bytes, file, frame);
        byte[] rowsText = Encoding.UTF8.GetBytes(RowsText(IndentOf(bytes, name), rows));

        byte[] result = new byte[open + rowsText.Length + (bytes.Length - close - 1)];
        bytes.AsSpan(0, open).CopyTo(result);
        rowsText.CopyTo(result, open);
        bytes.AsSpan(close + 1).CopyTo(result.AsSpan(open + rowsText.Length));

        RefuseWrongResult(result, file, frame, rows);
        return result;
    }

    // Gives the place of the name "rows" of the frame, of its "[", and of its "]". The
    // frames array lies at depth 1, each frame at depth 2, and each field of a frame at
    // depth 3, as the reader of Core reads the file (D-515).
    private static (int Name, int Open, int Close) FindRows(byte[] bytes, string file, int frame)
    {
        var reader = new Utf8JsonReader(bytes);
        bool inFrames = false;
        int seen = -1;
        try
        {
            while (reader.Read())
            {
                if (reader.CurrentDepth == 1 && reader.TokenType == JsonTokenType.PropertyName)
                {
                    inFrames = reader.ValueTextEquals("frames");
                    continue;
                }

                if (inFrames && reader.CurrentDepth == 2 && reader.TokenType == JsonTokenType.StartObject)
                {
                    seen += 1;
                    continue;
                }

                bool rowsOfFrame = inFrames && seen == frame && reader.CurrentDepth == 3 &&
                    reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals("rows");
                if (!rowsOfFrame)
                {
                    continue;
                }

                int name = (int)reader.TokenStartIndex;
                reader.Read();
                if (reader.TokenType != JsonTokenType.StartArray)
                {
                    throw ImportException.For(file, $"the field frames[{frame}].rows is not a list");
                }

                int open = (int)reader.TokenStartIndex;
                reader.Skip();
                return (name, open, (int)reader.TokenStartIndex);
            }
        }
        catch (JsonException fault)
        {
            throw ImportException.For(file, $"the file is not JSON. {fault.Message}");
        }

        throw ImportException.For(file, $"the drawing holds {seen + 1} frames, and it has no frame {frame}");
    }

    // The files of the repository indent each level by one space (D-515), so each row sits
    // one space deeper than the line of its field name.
    private static int IndentOf(byte[] bytes, int name)
    {
        int lineStart = name;
        while (lineStart > 0 && bytes[lineStart - 1] != (byte)'\n')
        {
            lineStart -= 1;
        }

        int indent = 0;
        while (bytes[lineStart + indent] == (byte)' ')
        {
            indent += 1;
        }

        return indent;
    }

    private static string RowsText(int indent, IReadOnlyList<string> rows)
    {
        var text = new StringBuilder();
        text.Append('[');
        for (int index = 0; index < rows.Count; index += 1)
        {
            text.Append('\n');
            text.Append(' ', indent + 1);
            text.Append('"');

            // A palette key is one printable character of ASCII, so the quote mark and the
            // backslash are the only keys that need an escape in JSON (D-515).
            text.Append(rows[index].Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal));
            text.Append('"');
            if (index < rows.Count - 1)
            {
                text.Append(',');
            }
        }

        text.Append('\n');
        text.Append(' ', indent);
        text.Append(']');
        return text.ToString();
    }

    private static void RefuseWrongResult(byte[] result, string file, int frame, IReadOnlyList<string> rows)
    {
        Drawing written;
        try
        {
            written = Drawing.Read(result, file);
        }
        catch (ContentException fault)
        {
            throw ImportException.For(file, $"the new text of the file does not read back: {fault.Message}");
        }

        IReadOnlyList<string> readBack = written.Frames[frame].Rows;
        for (int row = 0; row < rows.Count; row += 1)
        {
            if (string.CompareOrdinal(readBack[row], rows[row]) != 0)
            {
                throw ImportException.For(file, $"row {row} of frame {frame} reads back as '{readBack[row]}', and the import wrote '{rows[row]}'");
            }
        }
    }
}
