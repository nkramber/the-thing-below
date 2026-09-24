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

/// <summary>The stats that a strike reads: the attack against the defense, or the magic against the resistance (D-1053).</summary>
public enum StrikeStat
{
    /// <summary>A physical hit: the attack of the attacker against the defense of the target, as the basic attack.</summary>
    Attack,

    /// <summary>A magic hit: the magic of the attacker against the resistance of the target.</summary>
    Magic,
}

/// <summary>One entry of the ability file: an id and the effect of a move of an enemy or of a lesson (D-785, D-955, D-1029).</summary>
/// <param name="Id">The id, of the kind `ability`.</param>
/// <param name="Delay">The delay of the move, in ticks at speed 100 (D-768).</param>
public abstract record AbilityRecord(ContentId Id, int Delay);

/// <summary>A move that strikes one combatant of the other side (D-955).</summary>
/// <param name="Id">The id, of the kind `ability`.</param>
/// <param name="Delay">The delay of the move, in ticks at speed 100 (D-768).</param>
/// <param name="Power">The power, in basis points, which the hit of D-771 reads.</param>
/// <param name="Stat">The stats that the hit reads (D-1053).</param>
/// <param name="Element">The one element of the move, or no value for none (D-796).</param>
/// <param name="Reach">The row that the strike reaches (D-377).</param>
/// <param name="Status">The status that a hit gives, with its chance, or no value for none (D-793, D-807).</param>
public sealed record StrikeAbility(ContentId Id, int Delay, int Power, StrikeStat Stat, Element? Element, AbilityReach Reach, StatusChance? Status) : AbilityRecord(Id, Delay);

/// <summary>
/// A move that heals one combatant of its own side on the field, in either row (D-955). The heal
/// is the base plus the magic of the caster times the power, then the hit factor (D-1057).
/// </summary>
/// <param name="Id">The id, of the kind `ability`.</param>
/// <param name="Delay">The delay of the move, in ticks at speed 100 (D-768).</param>
/// <param name="Base">The health that the move restores at a magic of 0, before the hit factor (D-1057).</param>
/// <param name="Power">The share of the magic of the caster that the move adds, in basis points (D-1057).</param>
public sealed record HealAbility(ContentId Id, int Delay, int Base, int Power) : AbilityRecord(Id, Delay);

/// <summary>A move that ends set statuses on one ally on the field. A cure rite of a lesson gives it (D-391, D-1029).</summary>
/// <param name="Id">The id, of the kind `ability`.</param>
/// <param name="Delay">The delay of the move, in ticks at speed 100 (D-768).</param>
/// <param name="Statuses">The statuses that the move ends, in the order of D-75, with no repeat.</param>
public sealed record CureAbility(ContentId Id, int Delay, IReadOnlyList<StatusKind> Statuses) : AbilityRecord(Id, Delay);

/// <summary>A move that gives one timed status to one ally on the field: haste, regen, or shell (D-281, D-1029).</summary>
/// <param name="Id">The id, of the kind `ability`.</param>
/// <param name="Delay">The delay of the move, in ticks at speed 100 (D-768).</param>
/// <param name="Status">The status that the move gives.</param>
public sealed record BoonAbility(ContentId Id, int Delay, StatusKind Status) : AbilityRecord(Id, Delay);

/// <summary>A move that tries to take one entry of the steal list of one enemy on the field. A Theft drill of a lesson gives it (D-383, D-950, D-1029).</summary>
/// <param name="Id">The id, of the kind `ability`.</param>
/// <param name="Delay">The delay of the move, in ticks at speed 100 (D-768).</param>
public sealed record StealAbility(ContentId Id, int Delay) : AbilityRecord(Id, Delay);

/// <summary>
/// The ability file: the id of each ability and its effect (D-785, D-955, D-1029). The file is
/// `content/rules/abilities.json`. An enemy record and a form of a lesson name ids from this
/// list (D-557, D-1026).
/// </summary>
/// <remarks>
/// PR-11 gives an enemy ability its effect. PR-12 adds the status of a strike, the cure, and
/// the boon, and the lesson file holds the fields of a lesson (D-955, D-1026, D-1029). PR-99
/// adds the stat of a strike and the base and the power of a heal (D-1053, D-1057). Each id
/// stays (D-166).
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

    /// <summary>The name of the kind of a cure, in the file (D-1029).</summary>
    public const string CureName = "cure";

    /// <summary>The name of the kind of a boon, in the file (D-1029).</summary>
    public const string BoonName = "boon";

    /// <summary>The name of the kind of a steal, in the file (D-950).</summary>
    public const string StealName = "steal";

    /// <summary>The name of the status of a strike with no status, in the file (D-793).</summary>
    public const string NoStatusName = "none";

    /// <summary>The name of the element of a move with no element, in the file (D-796).</summary>
    public const string NoElementName = "none";

    /// <summary>The name of the stat of a physical strike, in the file (D-1053).</summary>
    public const string AttackStatName = "attack";

    /// <summary>The name of the stat of a magic strike, in the file (D-1053).</summary>
    public const string MagicStatName = "magic";

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
    /// Reads one entry. A strike takes a power, a stat, an element, a reach, and a status with
    /// its chance. A heal takes a base and a power, a cure takes its statuses, a boon takes its
    /// status, and a steal takes the delay alone. A field of another kind is an error, so no
    /// field of an entry goes unread (D-950, D-955, D-1029, D-1053, D-1057).
    /// </summary>
    private static AbilityRecord ReadAbility(ref ContentReader reader)
    {
        ContentId? id = null;
        string? kind = null;
        int? delay = null;
        int? power = null;
        string? stat = null;
        int? healBase = null;
        string? element = null;
        string? reach = null;
        string? status = null;
        int? chance = null;
        List<StatusKind>? statuses = null;

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
                case "stat":
                    stat = reader.ReadString();
                    break;
                case "base":
                    healBase = BattleFixture.ReadStat(ref reader, 1);
                    break;
                case "element":
                    element = reader.ReadString();
                    break;
                case "reach":
                    reach = reader.ReadString();
                    break;
                case "status":
                    status = reader.ReadString();
                    break;
                case "chance":
                    chance = reader.ReadInt();
                    break;
                case "statuses":
                    statuses = ReadStatuses(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        ContentId readId = reader.Require(id, depth, "id");
        string readKind = reader.Require(kind, depth, "kind");
        int readDelay = reader.RequireInt(delay, depth, "delay");

        // The kind decides which fields the entry takes, so an unknown kind fails before any field.
        if (!IsKind(readKind))
        {
            throw reader.RefuseField(depth, "kind", $"the kind '{readKind}' of '{readId.Value}' is not one of {StrikeName}, {HealName}, {CureName}, {BoonName}, {StealName} (D-950, D-955, D-1029)");
        }

        if (string.CompareOrdinal(readKind, StrikeName) == 0)
        {
            RefusePresent(ref reader, depth, healBase is not null, "base", readId, StrikeName);
            RefusePresent(ref reader, depth, statuses is not null, "statuses", readId, StrikeName);
            return new StrikeAbility(
                readId,
                readDelay,
                reader.RequireInt(power, depth, "power"),
                StatOf(ref reader, depth, reader.Require(stat, depth, "stat"), readId),
                ElementOf(ref reader, depth, reader.Require(element, depth, "element")),
                ReachOf(ref reader, depth, reader.Require(reach, depth, "reach")),
                StatusChanceOf(ref reader, depth, reader.Require(status, depth, "status"), chance, readId));
        }

        // A heal, a cure, and a boon take none of the fields of a strike (D-955, D-1029). A heal
        // alone takes a power too, which reads the magic of the caster (D-1057).
        RefusePresent(ref reader, depth, stat is not null, "stat", readId, readKind);
        RefusePresent(ref reader, depth, element is not null, "element", readId, readKind);
        RefusePresent(ref reader, depth, reach is not null, "reach", readId, readKind);
        RefusePresent(ref reader, depth, chance is not null, "chance", readId, readKind);
        if (string.CompareOrdinal(readKind, HealName) == 0)
        {
            RefusePresent(ref reader, depth, status is not null, "status", readId, HealName);
            RefusePresent(ref reader, depth, statuses is not null, "statuses", readId, HealName);
            return new HealAbility(readId, readDelay, reader.RequireInt(healBase, depth, "base"), reader.RequireInt(power, depth, "power"));
        }

        // A cure, a boon, and a steal take neither field of a heal.
        RefusePresent(ref reader, depth, power is not null, "power", readId, readKind);
        RefusePresent(ref reader, depth, healBase is not null, "base", readId, readKind);
        if (string.CompareOrdinal(readKind, CureName) == 0)
        {
            RefusePresent(ref reader, depth, status is not null, "status", readId, CureName);
            return new CureAbility(readId, readDelay, reader.Require(statuses, depth, "statuses"));
        }

        if (string.CompareOrdinal(readKind, BoonName) == 0)
        {
            RefusePresent(ref reader, depth, statuses is not null, "statuses", readId, BoonName);
            return new BoonAbility(readId, readDelay, BoonStatusOf(ref reader, depth, reader.Require(status, depth, "status"), readId));
        }

        // The check of the kind above leaves the steal alone.
        RefusePresent(ref reader, depth, status is not null, "status", readId, StealName);
        RefusePresent(ref reader, depth, statuses is not null, "statuses", readId, StealName);
        return new StealAbility(readId, readDelay);
    }

    private static bool IsKind(string kind) =>
        string.CompareOrdinal(kind, StrikeName) == 0 ||
        string.CompareOrdinal(kind, HealName) == 0 ||
        string.CompareOrdinal(kind, CureName) == 0 ||
        string.CompareOrdinal(kind, BoonName) == 0 ||
        string.CompareOrdinal(kind, StealName) == 0;

    /// <summary>
    /// Gives the status of a strike: none with no chance, or a status with a chance from 1 to
    /// 10000 (D-793, D-807).
    /// </summary>
    private static StatusChance? StatusChanceOf(ref ContentReader reader, int depth, string name, int? chance, ContentId id)
    {
        if (string.CompareOrdinal(name, NoStatusName) == 0)
        {
            RefusePresent(ref reader, depth, chance is not null, "chance", id, $"{StrikeName} with no status");
            return null;
        }

        if (!Statuses.TryOf(name, out StatusKind status))
        {
            throw reader.RefuseField(depth, "status", $"the status '{name}' of '{id.Value}' is not '{NoStatusName}' or one of {Statuses.EveryName} (D-75, D-793)");
        }

        int readChance = reader.RequireInt(chance, depth, "chance");
        if (readChance < 1 || readChance > BasisPoints.One)
        {
            throw reader.RefuseField(depth, "chance", $"the chance {readChance} of '{id.Value}' is outside 1 to {BasisPoints.One} (D-807)");
        }

        return new StatusChance(status, readChance);
    }

    /// <summary>Gives the status of a boon: haste, regen, or shell, the timed statuses that help an ally (D-281, D-1029).</summary>
    private static StatusKind BoonStatusOf(ref ContentReader reader, int depth, string name, ContentId id)
    {
        if (Statuses.TryOf(name, out StatusKind status) &&
            (status == StatusKind.Haste || status == StatusKind.Regen || status == StatusKind.Shell))
        {
            return status;
        }

        throw reader.RefuseField(depth, "status", $"the status '{name}' of the boon '{id.Value}' is not one of haste, regen, shell (D-281, D-1029)");
    }

    /// <summary>Reads the statuses of a cure: at least one, in the order of D-75, with no repeat (D-1029). The item file reads a cure item with this rule too (D-1046).</summary>
    internal static List<StatusKind> ReadStatuses(ref ContentReader reader)
    {
        List<StatusKind> statuses = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, statuses.Count))
        {
            string name = reader.ReadString();
            if (!Statuses.TryOf(name, out StatusKind status))
            {
                throw reader.Refuse($"the status '{name}' is not one of {Statuses.EveryName} (D-75)");
            }

            if (statuses.Count > 0 && (int)status <= (int)statuses[^1])
            {
                throw reader.Refuse($"the status '{name}' repeats or leaves the order of D-75");
            }

            statuses.Add(status);
        }

        if (statuses.Count == 0)
        {
            throw reader.Refuse("a cure ends at least one status (D-1029)");
        }

        return statuses;
    }

    private static void RefusePresent(ref ContentReader reader, int depth, bool present, string field, ContentId id, string kind)
    {
        if (present)
        {
            throw reader.RefuseField(depth, field, $"the {kind} '{id.Value}' takes no field '{field}', which another kind of move reads (D-955)");
        }
    }

    private static StrikeStat StatOf(ref ContentReader reader, int depth, string name, ContentId id)
    {
        if (string.CompareOrdinal(name, AttackStatName) == 0)
        {
            return StrikeStat.Attack;
        }

        if (string.CompareOrdinal(name, MagicStatName) == 0)
        {
            return StrikeStat.Magic;
        }

        throw reader.RefuseField(depth, "stat", $"the stat '{name}' of '{id.Value}' is not one of {AttackStatName}, {MagicStatName} (D-1053)");
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
