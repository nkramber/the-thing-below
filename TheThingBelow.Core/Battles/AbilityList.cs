using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Battles;

/// <summary>
/// The ability file: the id of each ability, and nothing else yet (D-785). The file is
/// `content/rules/abilities.json`. An enemy record names ids from this list (D-557).
/// </summary>
/// <remarks>
/// PR-12 adds the fields of a lesson to each entry, and each id stays (D-166, D-785). No
/// fight reads an ability before PR-11 and PR-12 (D-787).
/// </remarks>
public sealed class AbilityList
{
    /// <summary>The path of the file under the content folder (D-785).</summary>
    public const string Path = "rules/abilities.json";

    /// <summary>The kind of an ability id (D-646).</summary>
    public const string Kind = "ability";

    private AbilityList(string file, IReadOnlyList<ContentId> ids)
    {
        this.File = file;
        this.Ids = ids;
    }

    /// <summary>The path of the file, for an error that names an absent id (T-2).</summary>
    public string File { get; }

    /// <summary>Every ability id, in the order of the file.</summary>
    public IReadOnlyList<ContentId> Ids { get; }

    /// <summary>Reads the ability file, and refuses a repeated id (T-2, D-166).</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, for an error.</param>
    /// <returns>The list.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or repeated, or an id repeats (G-6, T-2).</exception>
    public static AbilityList Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        string? comment = null;
        List<ContentId>? ids = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "abilities":
                    ids = ReadAbilities(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        var list = new AbilityList(file, reader.Require(ids, depth, "abilities"));
        reader.ReadFileEnd();

        list.RefuseRepeatedId();
        return list;
    }

    /// <summary>Tells whether the file holds an ability id.</summary>
    /// <param name="id">The id.</param>
    /// <returns>True when an entry of the file has this id.</returns>
    public bool Holds(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        foreach (ContentId held in this.Ids)
        {
            if (string.CompareOrdinal(held.Value, id.Value) == 0)
            {
                return true;
            }
        }

        return false;
    }

    private static List<ContentId> ReadAbilities(ref ContentReader reader)
    {
        List<ContentId> ids = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, ids.Count))
        {
            ids.Add(ReadAbility(ref reader));
        }

        return ids;
    }

    /// <summary>Reads one entry. An entry is an object, so PR-12 adds a field with no new form (D-785).</summary>
    private static ContentId ReadAbility(ref ContentReader reader)
    {
        ContentId? id = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(Kind);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return reader.Require(id, depth, "id");
    }

    private void RefuseRepeatedId()
    {
        var seen = new SortedSet<string>(StringComparer.Ordinal);
        foreach (ContentId id in this.Ids)
        {
            if (!seen.Add(id.Value))
            {
                throw ContentException.ForField(this.File, id.Value, "the file defines this id two times, and an id is permanent (D-166)");
            }
        }
    }
}
