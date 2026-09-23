using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Battles;

/// <summary>What an enemy strike reaches (D-377, D-955).</summary>
public enum AbilityReach
{
    /// <summary>The front row of the other side while anyone stands in it, as the basic attack (D-377).</summary>
    Melee,

    /// <summary>Either row of the other side, as a shot drill or a rite (D-377).</summary>
    Any,
}

/// <summary>One entry of the ability file: an id and the effect that an enemy move gives (D-785, D-955).</summary>
/// <param name="Id">The id, of the kind `ability`.</param>
/// <param name="Delay">The delay of the move, in ticks at speed 100 (D-768).</param>
public abstract record AbilityRecord(ContentId Id, int Delay);

/// <summary>A move that strikes one combatant of the other side (D-955).</summary>
/// <param name="Id">The id, of the kind `ability`.</param>
/// <param name="Delay">The delay of the move, in ticks at speed 100 (D-768).</param>
/// <param name="Power">The power, in basis points, which the hit of D-771 reads.</param>
/// <param name="Element">The one element of the move, or no value for none (D-796).</param>
/// <param name="Reach">The row that the strike reaches (D-377).</param>
public sealed record StrikeAbility(ContentId Id, int Delay, int Power, Element? Element, AbilityReach Reach) : AbilityRecord(Id, Delay);

/// <summary>A move that heals one combatant of its own side on the field, in either row (D-955).</summary>
/// <param name="Id">The id, of the kind `ability`.</param>
/// <param name="Delay">The delay of the move, in ticks at speed 100 (D-768).</param>
/// <param name="Heal">The health that the move restores, up to the full health of the target.</param>
public sealed record HealAbility(ContentId Id, int Delay, int Heal) : AbilityRecord(Id, Delay);

/// <summary>
/// The ability file: the id of each ability and the effect of its enemy move (D-785, D-955).
/// The file is `content/rules/abilities.json`. An enemy record names ids from this list
/// (D-557).
/// </summary>
/// <remarks>
/// PR-11 gives an enemy ability its effect, and PR-12 adds the fields of a lesson to each
/// entry. Each id stays (D-166, D-787, D-955).
/// </remarks>
public sealed class AbilityList
{
    /// <summary>The path of the file under the content folder (D-785).</summary>
    public const string Path = "rules/abilities.json";

    /// <summary>The kind of an ability id (D-646).</summary>
    public const string Kind = "ability";

    /// <summary>The name of the kind of a strike, in the file.</summary>
    public const string StrikeName = "strike";

    /// <summary>The name of the kind of a heal, in the file.</summary>
    public const string HealName = "heal";

    /// <summary>The name of the element of a move with no element, in the file (D-796).</summary>
    public const string NoElementName = "none";

    private AbilityList(string file, IReadOnlyList<AbilityRecord> records)
    {
        this.File = file;
        this.Records = records;
        List<ContentId> ids = [];
        foreach (AbilityRecord record in records)
        {
            ids.Add(record.Id);
        }

        this.Ids = ids;
    }

    /// <summary>The path of the file, for an error that names an absent id (T-2).</summary>
    public string File { get; }

    /// <summary>Every ability, in the order of the file.</summary>
    public IReadOnlyList<AbilityRecord> Records { get; }

    /// <summary>Every ability id, in the order of the file.</summary>
    public IReadOnlyList<ContentId> Ids { get; }

    /// <summary>Reads the ability file, and refuses a repeated id (T-2, D-166).</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, for an error.</param>
    /// <returns>The list.</returns>
    /// <exception cref="ContentException">
    /// A field is absent, unknown, repeated, or out of its range, a field of the other kind of
    /// move is present, or an id repeats (G-6, T-2).
    /// </exception>
    public static AbilityList Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        string? comment = null;
        List<AbilityRecord>? records = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "abilities":
                    records = ReadAbilities(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        var list = new AbilityList(file, reader.Require(records, depth, "abilities"));
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

        return this.Find(id) is not null;
    }

    /// <summary>Finds an ability by id.</summary>
    /// <param name="id">The id.</param>
    /// <returns>The record.</returns>
    /// <exception cref="ContentException">The file holds no such ability (T-2).</exception>
    public AbilityRecord Ability(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return this.Find(id)
            ?? throw ContentException.ForField(this.File, id.Value, "the ability file holds no ability with this id (T-2, D-785)");
    }

    private AbilityRecord? Find(ContentId id)
    {
        foreach (AbilityRecord record in this.Records)
        {
            if (string.CompareOrdinal(record.Id.Value, id.Value) == 0)
            {
                return record;
            }
        }

        return null;
    }

    private static List<AbilityRecord> ReadAbilities(ref ContentReader reader)
    {
        List<AbilityRecord> records = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, records.Count))
        {
            records.Add(ReadAbility(ref reader));
        }

        return records;
    }

    /// <summary>
    /// Reads one entry. A strike takes a power, an element, and a reach, and a heal takes a
    /// heal. A field of the other kind is an error, so no field of an entry goes unread (D-955).
    /// </summary>
    private static AbilityRecord ReadAbility(ref ContentReader reader)
    {
        ContentId? id = null;
        string? kind = null;
        int? delay = null;
        int? power = null;
        int? heal = null;
        string? element = null;
        string? reach = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(Kind);
                    break;
                case "kind":
                    kind = reader.ReadString();
                    break;
                case "delay":
                    delay = BattleFixture.ReadStat(ref reader, 1);
                    break;
                case "power":
                    power = BattleFixture.ReadStat(ref reader, 1);
                    break;
                case "heal":
                    heal = BattleFixture.ReadStat(ref reader, 1);
                    break;
                case "element":
                    element = reader.ReadString();
                    break;
                case "reach":
                    reach = reader.ReadString();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        ContentId readId = reader.Require(id, depth, "id");
        string readKind = reader.Require(kind, depth, "kind");
        int readDelay = reader.RequireInt(delay, depth, "delay");
        if (string.CompareOrdinal(readKind, StrikeName) == 0)
        {
            RefusePresent(ref reader, depth, heal is not null, "heal", readId, StrikeName);
            return new StrikeAbility(
                readId,
                readDelay,
                reader.RequireInt(power, depth, "power"),
                ElementOf(ref reader, depth, reader.Require(element, depth, "element")),
                ReachOf(ref reader, depth, reader.Require(reach, depth, "reach")));
        }

        if (string.CompareOrdinal(readKind, HealName) == 0)
        {
            RefusePresent(ref reader, depth, power is not null, "power", readId, HealName);
            RefusePresent(ref reader, depth, element is not null, "element", readId, HealName);
            RefusePresent(ref reader, depth, reach is not null, "reach", readId, HealName);
            return new HealAbility(readId, readDelay, reader.RequireInt(heal, depth, "heal"));
        }

        throw reader.RefuseField(depth, "kind", $"the kind '{readKind}' of '{readId.Value}' is not one of {StrikeName}, {HealName} (D-955)");
    }

    private static void RefusePresent(ref ContentReader reader, int depth, bool present, string field, ContentId id, string kind)
    {
        if (present)
        {
            throw reader.RefuseField(depth, field, $"the {kind} '{id.Value}' takes no field '{field}', which another kind of move reads (D-955)");
        }
    }

    private static Element? ElementOf(ref ContentReader reader, int depth, string name)
    {
        if (string.CompareOrdinal(name, NoElementName) == 0)
        {
            return null;
        }

        if (!Elements.TryOf(name, out Element element))
        {
            throw reader.RefuseField(depth, "element", $"the element '{name}' is not '{NoElementName}' or the name of one of the eight elements (D-74, D-796)");
        }

        return element;
    }

    private static AbilityReach ReachOf(ref ContentReader reader, int depth, string name)
    {
        if (string.CompareOrdinal(name, "melee") == 0)
        {
            return AbilityReach.Melee;
        }

        if (string.CompareOrdinal(name, "any") == 0)
        {
            return AbilityReach.Any;
        }

        throw reader.RefuseField(depth, "reach", $"the reach '{name}' is not one of melee, any (D-377, D-955)");
    }

    private void RefuseRepeatedId()
    {
        var seen = new SortedSet<string>(StringComparer.Ordinal);
        foreach (AbilityRecord record in this.Records)
        {
            if (!seen.Add(record.Id.Value))
            {
                throw ContentException.ForField(this.File, record.Id.Value, "the file defines this id two times, and an id is permanent (D-166)");
            }
        }
    }
}
