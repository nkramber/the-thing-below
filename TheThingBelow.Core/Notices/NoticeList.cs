using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Notices;

/// <summary>One notice of the notice file: its id, and whether it lands in the notice log (D-983).</summary>
/// <param name="Id">The id, of the kind `notice`. The string table holds the line of the notice under the same id (G-7).</param>
/// <param name="Logs">True when the notice also lands in the notice log of the menu (D-221, D-983).</param>
public sealed record NoticeRecord(ContentId Id, bool Logs);

/// <summary>
/// The notice file: each one-line notice that a rule can post, and whether it logs (D-221,
/// D-983). The file is `content/rules/notices.json`.
/// </summary>
/// <remarks>
/// The writer marks each notice, so the `log` field is required, and an absent field fails
/// the load (D-983, G-6). PR-62 holds two fixture notices, one of each kind (D-989). The chest,
/// the door, the task, and the key item of later PRs add their own notices to this file.
/// </remarks>
public sealed class NoticeList
{
    /// <summary>The path of the file under the content folder (D-989).</summary>
    public const string Path = "rules/notices.json";

    /// <summary>The kind of a notice id (D-646).</summary>
    public const string Kind = "notice";

    private NoticeList(string file, IReadOnlyList<NoticeRecord> records)
    {
        this.File = file;
        this.Records = records;
        List<ContentId> ids = [];
        foreach (NoticeRecord record in records)
        {
            ids.Add(record.Id);
        }

        this.Ids = ids;
    }

    /// <summary>The path of the file, for an error that names an absent id (T-2).</summary>
    public string File { get; }

    /// <summary>Every notice, in the order of the file.</summary>
    public IReadOnlyList<NoticeRecord> Records { get; }

    /// <summary>Every notice id, in the order of the file.</summary>
    public IReadOnlyList<ContentId> Ids { get; }

    /// <summary>Reads the notice file, and refuses a repeated id (T-2, D-166).</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, for an error.</param>
    /// <returns>The list.</returns>
    /// <exception cref="ContentException">
    /// A field is absent, unknown, or repeated, the file holds no notice, or an id repeats
    /// (G-6, T-2).
    /// </exception>
    public static NoticeList Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        string? comment = null;
        List<NoticeRecord>? records = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "notices":
                    records = ReadNotices(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        List<NoticeRecord> read = reader.Require(records, depth, "notices");
        if (read.Count == 0)
        {
            throw reader.RefuseField(depth, "notices", "the notice file holds no notice, and a rule posts one of its entries (D-989)");
        }

        var list = new NoticeList(file, read);
        reader.ReadFileEnd();

        list.RefuseRepeatedId();
        return list;
    }

    /// <summary>Finds a notice by id.</summary>
    /// <param name="id">The id.</param>
    /// <returns>The record.</returns>
    /// <exception cref="ContentException">The file holds no such notice (T-2).</exception>
    public NoticeRecord Notice(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return this.Find(id)
            ?? throw ContentException.ForField(this.File, id.Value, "the notice file holds no notice with this id (T-2, D-989)");
    }

    /// <summary>Tells whether the file holds a notice id.</summary>
    /// <param name="id">The id.</param>
    /// <returns>True when an entry of the file has this id.</returns>
    public bool Holds(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return this.Find(id) is not null;
    }

    /// <summary>Gives the first notice of the file whose log mark is the one asked for (D-989).</summary>
    /// <param name="logs">True for the first notice that logs, and false for the first that does not.</param>
    /// <returns>The record.</returns>
    /// <exception cref="ContentException">The file holds no notice with that mark (T-2).</exception>
    /// <remarks>The debug console posts a notice of each kind by this method, so no command names a content id.</remarks>
    public NoticeRecord FirstThatLogs(bool logs)
    {
        foreach (NoticeRecord record in this.Records)
        {
            if (record.Logs == logs)
            {
                return record;
            }
        }

        string kind = logs ? "logs" : "does not log";
        throw ContentException.ForFile(this.File, $"the notice file holds no notice that {kind} (D-989)");
    }

    private NoticeRecord? Find(ContentId id)
    {
        foreach (NoticeRecord record in this.Records)
        {
            if (string.CompareOrdinal(record.Id.Value, id.Value) == 0)
            {
                return record;
            }
        }

        return null;
    }

    private static List<NoticeRecord> ReadNotices(ref ContentReader reader)
    {
        List<NoticeRecord> records = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, records.Count))
        {
            records.Add(ReadNotice(ref reader));
        }

        return records;
    }

    private static NoticeRecord ReadNotice(ref ContentReader reader)
    {
        ContentId? id = null;
        bool? logs = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(Kind);
                    break;
                case "log":
                    logs = reader.ReadBoolean();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new NoticeRecord(reader.Require(id, depth, "id"), reader.RequireValue(logs, depth, "log"));
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
                        $"notices[{index}].id",
                        $"the notice file holds the id '{this.Records[index].Id.Value}' two times, and an id names one notice (D-166)");
                }
            }
        }
    }
}
