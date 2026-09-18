using System;
using System.Collections.Generic;

namespace TheThingBelow.Core.Content;

/// <summary>
/// The table that maps a string id to the text that the player reads (G-7, D-167). Core
/// events name a string id alone, and Game reads the text for that id.
/// </summary>
/// <remarks>
/// Every string that the player reads lives here, and no C# file holds one (G-7, D-499).
/// The table stays outside the content hash, so a text edit never breaks a stored run record
/// or a replay fixture (D-495, D-648).
/// <para>
/// The prologue ships English alone, and a later language takes a second file (D-167).
/// </para>
/// </remarks>
public sealed class StringTable
{
    /// <summary>The path of the English table, under `content/`.</summary>
    public const string Path = "strings/en.json";

    private readonly SortedDictionary<string, string> text;

    private StringTable(string comment, SortedDictionary<string, string> text)
    {
        this.Comment = comment;
        this.text = text;
    }

    /// <summary>The note at the top of the file.</summary>
    public string Comment { get; }

    /// <summary>The number of entries in the table.</summary>
    public int Count => this.text.Count;

    /// <summary>Every string id of the table, in ordinal order (F-39).</summary>
    public IEnumerable<string> Ids => this.text.Keys;

    /// <summary>Reads the table from the bytes of its file.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The table.</returns>
    /// <exception cref="ContentException">The file breaks a rule of the reader, or repeats an id.</exception>
    public static StringTable Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        StringTable table = Read(ref reader);
        reader.ReadFileEnd();
        return table;
    }

    /// <summary>Tells whether the table holds an id.</summary>
    /// <param name="id">The string id, such as `ui.new_game`.</param>
    /// <returns>True when the table holds the id.</returns>
    public bool Contains(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return this.text.ContainsKey(id.Value);
    }

    /// <summary>Gives the text of a string id.</summary>
    /// <param name="id">The string id, such as `ui.new_game`.</param>
    /// <returns>The text that the player reads.</returns>
    /// <exception cref="ContentException">The table holds no such id (T-2).</exception>
    public string Text(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (!this.text.TryGetValue(id.Value, out string? value))
        {
            throw ContentException.ForField(Path, id.Value, "the string table holds no such id");
        }

        return value;
    }

    private static StringTable Read(ref ContentReader reader)
    {
        string? comment = null;
        SortedDictionary<string, string>? text = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "strings":
                    text = ReadStrings(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new StringTable(
            reader.Require(comment, depth, "comment"),
            reader.Require(text, depth, "strings"));
    }

    private static SortedDictionary<string, string> ReadStrings(ref ContentReader reader)
    {
        // An ordinal order, because the default order of .NET follows the culture of the
        // machine and the version of ICU on it (F-39, G-4).
        var text = new SortedDictionary<string, string>(StringComparer.Ordinal);

        int depth = reader.ReadArrayStart();
        int index = 0;
        while (reader.ReadNextElement(depth, index))
        {
            (ContentId id, string value) = ReadEntry(ref reader);
            if (text.ContainsKey(id.Value))
            {
                throw ContentException.ForField(
                    reader.File,
                    $"strings[{index}].id",
                    $"the table holds the string id '{id.Value}' two times");
            }

            text.Add(id.Value, value);
            index++;
        }

        return text;
    }

    private static (ContentId Id, string Text) ReadEntry(ref ContentReader reader)
    {
        ContentId? id = null;
        string? value = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId();
                    break;
                case "text":
                    value = reader.ReadString();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return (reader.Require(id, depth, "id"), reader.Require(value, depth, "text"));
    }
}
