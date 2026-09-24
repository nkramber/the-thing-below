using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Battles;

/// <summary>The kind of gear slot that a piece fits (D-44).</summary>
public enum GearSlotKind
{
    /// <summary>The weapon.</summary>
    Weapon,

    /// <summary>The shield or the off-hand.</summary>
    OffHand,

    /// <summary>The head.</summary>
    Head,

    /// <summary>The body.</summary>
    Body,

    /// <summary>An accessory, which fits either of the two accessory slots.</summary>
    Accessory,
}

/// <summary>One piece of gear: its slot kind, its stack limit, its stat amounts, and its element table (D-1036, D-1038).</summary>
/// <param name="Id">The id, of the kind `gear`.</param>
/// <param name="Slot">The kind of gear slot that the piece fits.</param>
/// <param name="Limit">The most copies that the party owns, worn copies included, from 1 to 3 (D-1038, D-1039).</param>
/// <param name="Attack">The amount that the piece adds to attack, from -99 to 99 (D-1047).</param>
/// <param name="Magic">The amount that the piece adds to magic, from -99 to 99 (D-1047, D-1052).</param>
/// <param name="Defense">The amount that the piece adds to defense, from -99 to 99 (D-1047).</param>
/// <param name="Resistance">The amount that the piece adds to resistance, from -99 to 99 (D-1047, D-1052).</param>
/// <param name="Speed">The amount that the piece adds to speed, from -99 to 99 (D-1047).</param>
/// <param name="Elements">The element level of the piece for each element. A plain piece holds normal for each (D-794, D-1036).</param>
public sealed record GearRecord(ContentId Id, GearSlotKind Slot, int Limit, int Attack, int Magic, int Defense, int Resistance, int Speed, ElementTable Elements);

/// <summary>
/// The gear file: each piece of gear (D-44, D-1036). The file is `content/rules/gear.json`.
/// The pack and the start gear of the battle fixture name ids from this list.
/// </summary>
/// <remarks>A piece never changes health or MP (D-1036). It adds to each other stat (D-1052).</remarks>
public sealed class GearList
{
    /// <summary>The path of the file under the content folder.</summary>
    public const string Path = "rules/gear.json";

    /// <summary>The kind of a gear id (D-646).</summary>
    public const string Kind = "gear";

    /// <summary>The highest stack limit of a piece: one copy for each character in a fight (D-1038).</summary>
    public const int HighestLimit = 3;

    /// <summary>The largest amount that a piece adds to a stat, and the negative of the lowest (D-1047).</summary>
    public const int MostAmount = 99;

    /// <summary>The names of the slot kinds, in the order of the enum, for an error.</summary>
    public const string EverySlotName = "weapon, off_hand, head, body, accessory";

    /// <summary>Every slot kind, in the order of the enum.</summary>
    public static readonly IReadOnlyList<GearSlotKind> AllSlotKinds = [GearSlotKind.Weapon, GearSlotKind.OffHand, GearSlotKind.Head, GearSlotKind.Body, GearSlotKind.Accessory];

    private GearList(string file, IReadOnlyList<GearRecord> records)
    {
        this.File = file;
        this.Records = records;
        List<ContentId> ids = [];
        foreach (GearRecord record in records)
        {
            ids.Add(record.Id);
        }

        this.Ids = ids;
    }

    /// <summary>The path of the file, for an error that names an absent id (T-2).</summary>
    public string File { get; }

    /// <summary>Every piece, in the order of the file.</summary>
    public IReadOnlyList<GearRecord> Records { get; }

    /// <summary>Every gear id, in the order of the file.</summary>
    public IReadOnlyList<ContentId> Ids { get; }

    /// <summary>Reads the gear file, and refuses a repeated id (T-2, D-166).</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, for an error.</param>
    /// <returns>The list.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, repeated, or out of its range, or an id repeats (G-6, T-2).</exception>
    public static GearList Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        string? comment = null;
        List<GearRecord>? records = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "gear":
                    records = ReadPieces(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        var list = new GearList(file, reader.Require(records, depth, "gear"));
        reader.ReadFileEnd();

        list.RefuseRepeatedId();
        return list;
    }

    /// <summary>Gives the name of a slot kind, as the file writes it.</summary>
    /// <param name="slot">The slot kind.</param>
    /// <returns>The name.</returns>
    public static string NameOf(GearSlotKind slot) => slot switch
    {
        GearSlotKind.Weapon => "weapon",
        GearSlotKind.OffHand => "off_hand",
        GearSlotKind.Head => "head",
        GearSlotKind.Body => "body",
        GearSlotKind.Accessory => "accessory",
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "The slot kind is not one of the five kinds (D-44)."),
    };

    /// <summary>Tells whether the file holds a gear id.</summary>
    /// <param name="id">The id.</param>
    /// <returns>True when an entry of the file has this id.</returns>
    public bool Holds(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return this.Find(id) is not null;
    }

    /// <summary>Finds a piece by id.</summary>
    /// <param name="id">The id.</param>
    /// <returns>The record.</returns>
    /// <exception cref="ContentException">The file holds no such piece (T-2).</exception>
    public GearRecord Piece(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return this.Find(id)
            ?? throw ContentException.ForField(this.File, id.Value, "the gear file holds no piece with this id (T-2, D-1036)");
    }

    private GearRecord? Find(ContentId id)
    {
        foreach (GearRecord record in this.Records)
        {
            if (string.CompareOrdinal(record.Id.Value, id.Value) == 0)
            {
                return record;
            }
        }

        return null;
    }

    private static List<GearRecord> ReadPieces(ref ContentReader reader)
    {
        List<GearRecord> records = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, records.Count))
        {
            records.Add(ReadPiece(ref reader));
        }

        return records;
    }

    private static GearRecord ReadPiece(ref ContentReader reader)
    {
        ContentId? id = null;
        GearSlotKind? slot = null;
        int? limit = null;
        int? attack = null;
        int? magic = null;
        int? defense = null;
        int? resistance = null;
        int? speed = null;
        ElementTable? elements = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(Kind);
                    break;
                case "slot":
                    slot = ReadSlot(ref reader);
                    break;
                case "limit":
                    limit = ReadInRange(ref reader, 1, HighestLimit, "D-1038");
                    break;
                case "attack":
                    attack = ReadInRange(ref reader, -MostAmount, MostAmount, "D-1047");
                    break;
                case "magic":
                    magic = ReadInRange(ref reader, -MostAmount, MostAmount, "D-1047");
                    break;
                case "defense":
                    defense = ReadInRange(ref reader, -MostAmount, MostAmount, "D-1047");
                    break;
                case "resistance":
                    resistance = ReadInRange(ref reader, -MostAmount, MostAmount, "D-1047");
                    break;
                case "speed":
                    speed = ReadInRange(ref reader, -MostAmount, MostAmount, "D-1047");
                    break;
                case "elements":
                    elements = ElementTable.Read(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new GearRecord(
            reader.Require(id, depth, "id"),
            reader.RequireValue(slot, depth, "slot"),
            reader.RequireInt(limit, depth, "limit"),
            reader.RequireInt(attack, depth, "attack"),
            reader.RequireInt(magic, depth, "magic"),
            reader.RequireInt(defense, depth, "defense"),
            reader.RequireInt(resistance, depth, "resistance"),
            reader.RequireInt(speed, depth, "speed"),
            reader.Require(elements, depth, "elements"));
    }

    private static GearSlotKind ReadSlot(ref ContentReader reader)
    {
        string name = reader.ReadString();
        foreach (GearSlotKind slot in AllSlotKinds)
        {
            if (string.CompareOrdinal(name, NameOf(slot)) == 0)
            {
                return slot;
            }
        }

        throw reader.Refuse($"the slot '{name}' is not one of {EverySlotName} (D-44)");
    }

    private static int ReadInRange(ref ContentReader reader, int lowest, int highest, string decision)
    {
        int value = reader.ReadInt();
        if (value < lowest || value > highest)
        {
            throw reader.Refuse($"the value {value} is outside {lowest} to {highest} ({decision})");
        }

        return value;
    }

    private void RefuseRepeatedId()
    {
        var seen = new SortedSet<string>(StringComparer.Ordinal);
        foreach (GearRecord record in this.Records)
        {
            if (!seen.Add(record.Id.Value))
            {
                throw ContentException.ForField(this.File, record.Id.Value, "the file defines this id two times, and an id is permanent (D-166)");
            }
        }
    }
}
