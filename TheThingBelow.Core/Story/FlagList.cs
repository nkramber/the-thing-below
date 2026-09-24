using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Story;

/// <summary>One story flag of the flag file: its id, and one line of prose for the owner (D-1003).</summary>
/// <param name="Id">The id, of the kind `flag`. The id is permanent, because a save of the prologue carries it into the full game (D-163, D-166).</param>
/// <param name="Note">What the flag remembers, in one line for the owner. No player reads it.</param>
public sealed record FlagRecord(ContentId Id, string Note);

/// <summary>
/// The flag file: every story flag id of the game, with one line of prose for each (D-542,
/// D-1003). The file is `content/rules/flags.json`.
/// </summary>
/// <remarks>
/// A flag is a name that is on or off, and no count or value hides in one (D-329, D-542). A
/// condition, a set flag step, or a choose step that names an id this file lacks fails the
/// load (T-2). The file can hold no flag, because no story PR before PR-17 writes one.
/// </remarks>
public sealed class FlagList
{
    /// <summary>The path of the file under the content folder (D-1003).</summary>
    public const string Path = "rules/flags.json";

    /// <summary>The kind of a flag id (D-646).</summary>
    public const string Kind = "flag";

    private FlagList(string file, IReadOnlyList<FlagRecord> records)
    {
        this.File = file;
        this.Records = records;
    }

    /// <summary>The path of the file, for an error that names an absent id (T-2).</summary>
    public string File { get; }

    /// <summary>Every flag, in the order of the file.</summary>
    public IReadOnlyList<FlagRecord> Records { get; }

    /// <summary>Gives every flag id, in the order of the file.</summary>
    /// <returns>The ids.</returns>
    public IReadOnlyList<ContentId> Ids()
    {
        List<ContentId> ids = [];
        foreach (FlagRecord record in this.Records)
        {
            ids.Add(record.Id);
        }

        return ids;
    }

    /// <summary>Reads the flag file, and refuses a repeated id or an empty note (T-2, D-166).</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, for an error.</param>
    /// <returns>The list.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or repeated, a note is empty, or an id repeats (G-6, T-2).</exception>
    public static FlagList Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        string? comment = null;
        List<FlagRecord>? records = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "flags":
                    records = ReadFlags(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        var list = new FlagList(file, reader.Require(records, depth, "flags"));
        reader.ReadFileEnd();

        list.RefuseRepeatedId();
        return list;
    }

    /// <summary>Tells whether the file declares a flag id.</summary>
    /// <param name="id">The id.</param>
    /// <returns>True when an entry of the file has this id.</returns>
    /// <exception cref="ArgumentNullException">The id is null (T-2).</exception>
    public bool Declares(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        foreach (FlagRecord record in this.Records)
        {
            if (string.CompareOrdinal(record.Id.Value, id.Value) == 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Fails when the file does not declare a flag id (D-542, T-2).</summary>
    /// <param name="id">The id that a reader names.</param>
    /// <param name="file">The file of the reader, for the error.</param>
    /// <param name="field">The field of the reader, for the error.</param>
    /// <exception cref="ContentException">The flag file holds no such id.</exception>
    public void RequireDeclared(ContentId id, string file, string field)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (!this.Declares(id))
        {
            throw ContentException.ForField(
                file,
                field,
                $"the flag '{id.Value}' has no line in '{this.File}', and that file declares every flag (D-542, D-1003)");
        }
    }

    private static List<FlagRecord> ReadFlags(ref ContentReader reader)
    {
        List<FlagRecord> records = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, records.Count))
        {
            records.Add(ReadFlag(ref reader));
        }

        return records;
    }

    private static FlagRecord ReadFlag(ref ContentReader reader)
    {
        ContentId? id = null;
        string? note = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(Kind);
                    break;
                case "note":
                    note = reader.ReadString();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        string readNote = reader.Require(note, depth, "note");
        if (readNote.Trim().Length == 0)
        {
            throw reader.RefuseField(depth, "note", "the note is empty, and each flag holds one line of prose (D-1003)");
        }

        return new FlagRecord(reader.Require(id, depth, "id"), readNote);
    }

    private void RefuseRepeatedId()
    {
        for (int index = 0; index < this.Records.Count; index += 1)
        {
            for (int earlier = 0; earlier < index; earlier += 1)
            {
                if (string.CompareOrdinal(this.Records[earlier].Id.Value, this.Records[index].Id.Value) == 0)
                {
                    throw ContentException.ForField(
                        this.File,
                        $"flags[{index}].id",
                        $"the flag file holds the id '{this.Records[index].Id.Value}' two times, and an id names one flag (D-166)");
                }
            }
        }
    }
}
