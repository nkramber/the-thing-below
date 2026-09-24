using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Battles;

/// <summary>One entry of the item file: an id and the stack limit of the item (D-1038).</summary>
/// <param name="Id">The id, of the kind `item`.</param>
/// <param name="Limit">The most copies that the party owns (D-1038, D-1039).</param>
public abstract record ItemRecord(ContentId Id, int Limit);

/// <summary>An item with a stack limit of 1 that a use never spends, such as the torch (D-848, D-1038).</summary>
/// <param name="Id">The id, of the kind `item`.</param>
public sealed record KeyItem(ContentId Id) : ItemRecord(Id, 1);

/// <summary>An item that one use spends: a heal, a restore, a cure, or a revive (D-384, D-1046).</summary>
/// <param name="Id">The id, of the kind `item`.</param>
/// <param name="Limit">The most copies that the party owns, from 3 to 10 (D-1038).</param>
/// <param name="Delay">The delay of a use in a fight, in ticks at speed 100 (D-757).</param>
public abstract record UsedUpItem(ContentId Id, int Limit, int Delay) : ItemRecord(Id, Limit);

/// <summary>An item that restores health to one ally who stands (D-1046).</summary>
/// <param name="Id">The id, of the kind `item`.</param>
/// <param name="Limit">The most copies that the party owns (D-1038).</param>
/// <param name="Delay">The delay of a use in a fight (D-757).</param>
/// <param name="Amount">The health that a use outside a fight restores. The item rate cuts it in a fight (D-382).</param>
public sealed record HealItem(ContentId Id, int Limit, int Delay, int Amount) : UsedUpItem(Id, Limit, Delay);

/// <summary>An item that restores MP to one ally who stands (D-1046).</summary>
/// <param name="Id">The id, of the kind `item`.</param>
/// <param name="Limit">The most copies that the party owns (D-1038).</param>
/// <param name="Delay">The delay of a use in a fight (D-757).</param>
/// <param name="Amount">The MP that a use outside a fight restores. The item rate cuts it in a fight (D-382).</param>
public sealed record RestoreItem(ContentId Id, int Limit, int Delay, int Amount) : UsedUpItem(Id, Limit, Delay);

/// <summary>An item that ends set statuses on one ally who stands. The item rate never changes it (D-1046).</summary>
/// <param name="Id">The id, of the kind `item`.</param>
/// <param name="Limit">The most copies that the party owns (D-1038).</param>
/// <param name="Delay">The delay of a use in a fight (D-757).</param>
/// <param name="Statuses">The statuses that a use ends, in the order of D-75, with no repeat.</param>
public sealed record CureItem(ContentId Id, int Limit, int Delay, IReadOnlyList<StatusKind> Statuses) : UsedUpItem(Id, Limit, Delay);

/// <summary>An item that stands one fallen ally up with a set health (D-36, D-1046).</summary>
/// <param name="Id">The id, of the kind `item`.</param>
/// <param name="Limit">The most copies that the party owns (D-1038).</param>
/// <param name="Delay">The delay of a use in a fight (D-757).</param>
/// <param name="Amount">The health of the ally after a use outside a fight. The item rate cuts it in a fight (D-382).</param>
public sealed record ReviveItem(ContentId Id, int Limit, int Delay, int Amount) : UsedUpItem(Id, Limit, Delay);

/// <summary>
/// The item file: the id, the stack limit, and the effect of each item (D-1038, D-1046). The
/// file is `content/rules/items.json`. The pack, a steal list, and a drop list name ids from
/// this list (D-383, D-1042).
/// </summary>
/// <remarks>PR-13 moved the items here from the battle fixture, and each id stays (D-166).</remarks>
public sealed class ItemList
{
    /// <summary>The path of the file under the content folder.</summary>
    public const string Path = "rules/items.json";

    /// <summary>The kind of an item id (D-646).</summary>
    public const string Kind = "item";

    /// <summary>The lowest stack limit of a used-up item (D-1038).</summary>
    public const int LowestUsedUpLimit = 3;

    /// <summary>The highest stack limit of a used-up item (D-1038).</summary>
    public const int HighestUsedUpLimit = 10;

    /// <summary>The name of the kind of a key item, in the file (D-1038).</summary>
    public const string KeyName = "key";

    /// <summary>The name of the kind of a heal, in the file (D-1046).</summary>
    public const string HealName = "heal";

    /// <summary>The name of the kind of a restore of MP, in the file (D-1046).</summary>
    public const string RestoreName = "restore";

    /// <summary>The name of the kind of a cure, in the file (D-1046).</summary>
    public const string CureName = "cure";

    /// <summary>The name of the kind of a revive, in the file (D-1046).</summary>
    public const string ReviveName = "revive";

    private ItemList(string file, IReadOnlyList<ItemRecord> records)
    {
        this.File = file;
        this.Records = records;
        List<ContentId> ids = [];
        foreach (ItemRecord record in records)
        {
            ids.Add(record.Id);
        }

        this.Ids = ids;
    }

    /// <summary>The path of the file, for an error that names an absent id (T-2).</summary>
    public string File { get; }

    /// <summary>Every item, in the order of the file.</summary>
    public IReadOnlyList<ItemRecord> Records { get; }

    /// <summary>Every item id, in the order of the file.</summary>
    public IReadOnlyList<ContentId> Ids { get; }

    /// <summary>Reads the item file, and refuses a repeated id (T-2, D-166).</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, for an error.</param>
    /// <returns>The list.</returns>
    /// <exception cref="ContentException">
    /// A field is absent, unknown, repeated, or out of its range, a field of another kind of
    /// item is present, or an id repeats (G-6, T-2).
    /// </exception>
    public static ItemList Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        string? comment = null;
        List<ItemRecord>? records = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "items":
                    records = ReadItems(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        var list = new ItemList(file, reader.Require(records, depth, "items"));
        reader.ReadFileEnd();

        list.RefuseRepeatedId();
        return list;
    }

    /// <summary>Tells whether the file holds an item id.</summary>
    /// <param name="id">The id.</param>
    /// <returns>True when an entry of the file has this id.</returns>
    public bool Holds(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return this.Find(id) is not null;
    }

    /// <summary>Finds an item by id.</summary>
    /// <param name="id">The id.</param>
    /// <returns>The record.</returns>
    /// <exception cref="ContentException">The file holds no such item (T-2).</exception>
    public ItemRecord Item(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return this.Find(id)
            ?? throw ContentException.ForField(this.File, id.Value, "the item file holds no item with this id (T-2, D-1038)");
    }

    private ItemRecord? Find(ContentId id)
    {
        foreach (ItemRecord record in this.Records)
        {
            if (string.CompareOrdinal(record.Id.Value, id.Value) == 0)
            {
                return record;
            }
        }

        return null;
    }

    private static List<ItemRecord> ReadItems(ref ContentReader reader)
    {
        List<ItemRecord> records = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, records.Count))
        {
            records.Add(ReadItem(ref reader));
        }

        return records;
    }

    /// <summary>
    /// Reads one entry. A key item takes a limit of 1 and nothing else. A used-up item takes a
    /// limit from 3 to 10 and a delay. A heal, a restore, and a revive take an amount, and a
    /// cure takes its statuses. A field of another kind is an error (D-1038, D-1046).
    /// </summary>
    private static ItemRecord ReadItem(ref ContentReader reader)
    {
        ContentId? id = null;
        string? kind = null;
        int? limit = null;
        int? delay = null;
        int? amount = null;
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
                case "limit":
                    limit = reader.ReadInt();
                    break;
                case "delay":
                    delay = BattleFixture.ReadStat(ref reader, 1);
                    break;
                case "amount":
                    amount = BattleFixture.ReadStat(ref reader, 1);
                    break;
                case "statuses":
                    statuses = AbilityList.ReadStatuses(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        ContentId readId = reader.Require(id, depth, "id");
        string readKind = reader.Require(kind, depth, "kind");
        int readLimit = reader.RequireInt(limit, depth, "limit");
        if (string.CompareOrdinal(readKind, KeyName) == 0)
        {
            if (readLimit != 1)
            {
                throw reader.RefuseField(depth, "limit", $"the key item '{readId.Value}' has the limit {readLimit}, and a key item has the limit 1 (D-1038)");
            }

            RefusePresent(ref reader, depth, delay is not null, "delay", readId, KeyName);
            RefusePresent(ref reader, depth, amount is not null, "amount", readId, KeyName);
            RefusePresent(ref reader, depth, statuses is not null, "statuses", readId, KeyName);
            return new KeyItem(readId);
        }

        if (readLimit < LowestUsedUpLimit || readLimit > HighestUsedUpLimit)
        {
            throw reader.RefuseField(depth, "limit", $"the limit {readLimit} of '{readId.Value}' is outside {LowestUsedUpLimit} to {HighestUsedUpLimit}, the range of a used-up item (D-1038)");
        }

        int readDelay = reader.RequireInt(delay, depth, "delay");
        if (string.CompareOrdinal(readKind, CureName) == 0)
        {
            RefusePresent(ref reader, depth, amount is not null, "amount", readId, CureName);
            return new CureItem(readId, readLimit, readDelay, reader.Require(statuses, depth, "statuses"));
        }

        RefusePresent(ref reader, depth, statuses is not null, "statuses", readId, readKind);
        if (string.CompareOrdinal(readKind, HealName) == 0)
        {
            return new HealItem(readId, readLimit, readDelay, reader.RequireInt(amount, depth, "amount"));
        }

        if (string.CompareOrdinal(readKind, RestoreName) == 0)
        {
            return new RestoreItem(readId, readLimit, readDelay, reader.RequireInt(amount, depth, "amount"));
        }

        if (string.CompareOrdinal(readKind, ReviveName) == 0)
        {
            return new ReviveItem(readId, readLimit, readDelay, reader.RequireInt(amount, depth, "amount"));
        }

        throw reader.RefuseField(depth, "kind", $"the kind '{readKind}' of '{readId.Value}' is not one of {KeyName}, {HealName}, {RestoreName}, {CureName}, {ReviveName} (D-1038, D-1046)");
    }

    private static void RefusePresent(ref ContentReader reader, int depth, bool present, string field, ContentId id, string kind)
    {
        if (present)
        {
            throw reader.RefuseField(depth, field, $"the {kind} item '{id.Value}' takes no field '{field}', which another kind of item reads (D-1046)");
        }
    }

    private void RefuseRepeatedId()
    {
        var seen = new SortedSet<string>(StringComparer.Ordinal);
        foreach (ItemRecord record in this.Records)
        {
            if (!seen.Add(record.Id.Value))
            {
                throw ContentException.ForField(this.File, record.Id.Value, "the file defines this id two times, and an id is permanent (D-166)");
            }
        }
    }
}
