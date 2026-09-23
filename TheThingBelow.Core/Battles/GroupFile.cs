using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Battles;

/// <summary>One enemy of a group (D-535, D-760, D-956).</summary>
/// <param name="Enemy">The id of the enemy record (D-786).</param>
/// <param name="Row">The row that the enemy stands in, or steps into (D-760).</param>
/// <param name="Waits">True when the enemy waits off the field for a fall (D-778).</param>
/// <param name="Profile">The id of the profile that the evaluator reads for this enemy (D-956).</param>
public sealed record GroupEntry(ContentId Enemy, BattleRow Row, bool Waits, ContentId Profile);

/// <summary>One enemy group of a region (D-535, D-957).</summary>
/// <param name="Id">The id, of the kind `group`, which a map names (D-753).</param>
/// <param name="Boss">True for a boss group, which no party flees (D-378).</param>
/// <param name="Entries">The enemies, in the order of the file (D-760).</param>
public sealed record GroupRecord(ContentId Id, bool Boss, IReadOnlyList<GroupEntry> Entries);

/// <summary>
/// The group file of one region: each enemy group, with the row, the wait, and the profile of
/// each enemy (D-535, D-957). The file is `content/rules/groups/` and the name of the region,
/// so the region `region.fixture` lives in `rules/groups/fixture.json`.
/// </summary>
/// <remarks>
/// A map names its region and its groups, and the battle content refuses a map whose group is
/// absent from the file of its region (D-957).
/// </remarks>
public sealed class GroupFile
{
    /// <summary>The folder of the group files under the content folder (D-957).</summary>
    public const string Folder = "rules/groups/";

    /// <summary>The kind of a region id (D-957).</summary>
    public const string RegionKind = "region";

    private GroupFile(string file, ContentId region, IReadOnlyList<GroupRecord> groups)
    {
        this.File = file;
        this.Region = region;
        this.Groups = groups;
    }

    /// <summary>The path of the file, for an error that names a group of it (T-2).</summary>
    public string File { get; }

    /// <summary>The region, of the kind `region` (D-957).</summary>
    public ContentId Region { get; }

    /// <summary>Every group, in the order of the file.</summary>
    public IReadOnlyList<GroupRecord> Groups { get; }

    /// <summary>Tells whether a content path is a group file (D-957).</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True when the path is a JSON file of the group folder.</returns>
    public static bool IsGroupFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return path.StartsWith(Folder, StringComparison.Ordinal) &&
            path.EndsWith(".json", StringComparison.Ordinal);
    }

    /// <summary>Reads one group file, and refuses a region that the path does not name (T-2).</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file under `content/`, which names the region.</param>
    /// <returns>The group file.</returns>
    /// <exception cref="ContentException">
    /// A field is absent, unknown, repeated, or out of its range, a group holds no encounter,
    /// a group id repeats, or the region differs from the name of the file (G-6, T-2).
    /// </exception>
    /// <remarks>The battle content checks each enemy and each profile that an entry names (D-786, D-956).</remarks>
    public static GroupFile Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        string? comment = null;
        ContentId? region = null;
        List<GroupRecord>? groups = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "region":
                    region = reader.ReadContentId(RegionKind);
                    break;
                case "groups":
                    groups = ReadGroups(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        ContentId readRegion = reader.Require(region, depth, "region");
        string expected = $"{Folder}{readRegion.Name}.json";
        if (string.CompareOrdinal(file, expected) != 0)
        {
            throw reader.RefuseField(depth, "region", $"the region '{readRegion.Value}' lives in '{expected}', and this file is '{file}' (D-957)");
        }

        var groupFile = new GroupFile(file, readRegion, reader.Require(groups, depth, "groups"));
        reader.ReadFileEnd();

        groupFile.RefuseRepeatedGroup();
        return groupFile;
    }

    /// <summary>Gives every content id that the file defines: the region, then each group (D-166).</summary>
    /// <returns>The ids.</returns>
    public IReadOnlyList<ContentId> DefinedIds()
    {
        var ids = new List<ContentId> { this.Region };
        foreach (GroupRecord group in this.Groups)
        {
            ids.Add(group.Id);
        }

        return ids;
    }

    /// <summary>Finds a group of this region by id.</summary>
    /// <param name="id">The id.</param>
    /// <returns>The group, or no value when the region holds none with this id.</returns>
    public GroupRecord? Find(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        foreach (GroupRecord group in this.Groups)
        {
            if (string.CompareOrdinal(group.Id.Value, id.Value) == 0)
            {
                return group;
            }
        }

        return null;
    }

    private static List<GroupRecord> ReadGroups(ref ContentReader reader)
    {
        List<GroupRecord> groups = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, groups.Count))
        {
            groups.Add(ReadGroup(ref reader));
        }

        return groups;
    }

    private static GroupRecord ReadGroup(ref ContentReader reader)
    {
        ContentId? id = null;
        bool? boss = null;
        List<GroupEntry>? entries = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(Maps.Patrol.GroupKind);
                    break;
                case "boss":
                    boss = reader.ReadBoolean();
                    break;
                case "enemies":
                    entries = ReadEntries(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        var group = new GroupRecord(
            reader.Require(id, depth, "id"),
            reader.RequireValue(boss, depth, "boss"),
            reader.Require(entries, depth, "enemies"));
        CheckGroupSize(ref reader, depth, group);
        return group;
    }

    /// <summary>
    /// Refuses a group that no encounter can hold: no enemy on the field at the start, more
    /// than six there, or more than twelve in all (D-758, D-759, D-762).
    /// </summary>
    private static void CheckGroupSize(ref ContentReader reader, int depth, GroupRecord group)
    {
        int standing = 0;
        foreach (GroupEntry entry in group.Entries)
        {
            standing += entry.Waits ? 0 : 1;
        }

        if (group.Entries.Count > BattleFixture.MostInGroup)
        {
            throw reader.RefuseField(
                depth,
                "enemies",
                $"the group '{group.Id.Value}' holds {group.Entries.Count} enemies, and a group holds at most {BattleFixture.MostInGroup} (D-762)");
        }

        if (standing == 0 || standing > BattleFixture.MostOnField)
        {
            throw reader.RefuseField(
                depth,
                "enemies",
                $"the group '{group.Id.Value}' starts {standing} enemies on the field, and the field holds 1 to {BattleFixture.MostOnField} (D-759, D-778)");
        }
    }

    private static List<GroupEntry> ReadEntries(ref ContentReader reader)
    {
        List<GroupEntry> entries = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, entries.Count))
        {
            entries.Add(ReadEntry(ref reader));
        }

        return entries;
    }

    private static GroupEntry ReadEntry(ref ContentReader reader)
    {
        ContentId? enemy = null;
        BattleRow? row = null;
        bool? waits = null;
        ContentId? profile = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "enemy":
                    enemy = reader.ReadContentId(EnemyRecord.Kind);
                    break;
                case "row":
                    row = BattleFixture.ReadRow(ref reader);
                    break;
                case "waits":
                    waits = reader.ReadBoolean();
                    break;
                case "profile":
                    profile = reader.ReadContentId(ProfileRecord.Kind);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new GroupEntry(
            reader.Require(enemy, depth, "enemy"),
            reader.RequireValue(row, depth, "row"),
            reader.RequireValue(waits, depth, "waits"),
            reader.Require(profile, depth, "profile"));
    }

    private void RefuseRepeatedGroup()
    {
        var seen = new SortedSet<string>(StringComparer.Ordinal);
        foreach (GroupRecord group in this.Groups)
        {
            if (!seen.Add(group.Id.Value))
            {
                throw ContentException.ForField(this.File, group.Id.Value, "the file defines this group two times, and an id is permanent (D-166)");
            }
        }
    }
}
