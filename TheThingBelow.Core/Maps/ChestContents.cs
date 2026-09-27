using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Maps;

/// <summary>One entry of a chest: an item, a piece of gear, or a lesson, with a count (D-1220).</summary>
/// <param name="Thing">The id of the item, the gear, or the lesson.</param>
/// <param name="Count">The count of copies, at least 1. A lesson entry holds 1, because the player owns one copy of a lesson (D-1023).</param>
/// <param name="Fallback">
/// The item that a lesson entry gives in place of a lesson that the party owns (D-1024). A lesson
/// entry names one, and an entry of another kind names none.
/// </param>
public sealed record ChestEntry(ContentId Thing, int Count, ContentId? Fallback);

/// <summary>What a chest of a map file holds: its entries and its gold (D-1161, D-1220).</summary>
/// <param name="Entries">The entries, in the order of the file (G-4).</param>
/// <param name="Gold">The gold of the chest, from 0.</param>
public sealed record ChestContents(IReadOnlyList<ChestEntry> Entries, int Gold)
{
    /// <summary>The field of a chest line that holds its entries (D-1220).</summary>
    public const string EntriesField = "contents";

    /// <summary>The field of a chest line that holds its gold (D-1161).</summary>
    public const string GoldField = "gold";

    /// <summary>The kinds of id that a chest entry names, for an error (T-2).</summary>
    public const string EveryEntryKind = "item, gear, lesson";

    /// <summary>Reads the entries of one chest, and checks the shape of each one (D-1220, T-2).</summary>
    /// <param name="reader">The reader, at the start of the array.</param>
    /// <returns>The entries.</returns>
    /// <exception cref="ContentException">An entry breaks a rule (G-6, T-2).</exception>
    /// <remarks>
    /// The content set checks that each id names a record of the battle content, because the
    /// map file loads before the items, the gear, and the lessons (T-2).
    /// </remarks>
    internal static List<ChestEntry> ReadEntries(ref ContentReader reader)
    {
        List<ChestEntry> entries = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, entries.Count))
        {
            entries.Add(ReadEntry(ref reader));
        }

        return entries;
    }

    private static ChestEntry ReadEntry(ref ContentReader reader)
    {
        ContentId? thing = null;
        int? count = null;
        ContentId? fallback = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "thing":
                    thing = reader.ReadContentId();
                    break;
                case "count":
                    count = reader.ReadInt();
                    break;
                case "fallback":
                    fallback = reader.ReadContentId(Battles.ItemList.Kind);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        ContentId readThing = reader.Require(thing, depth, "thing");
        int readCount = reader.RequireInt(count, depth, "count");
        bool lesson = string.CompareOrdinal(readThing.Kind, Battles.LessonList.Kind) == 0;
        if (!lesson && string.CompareOrdinal(readThing.Kind, Battles.ItemList.Kind) != 0 && string.CompareOrdinal(readThing.Kind, Battles.GearList.Kind) != 0)
        {
            throw reader.RefuseField(depth, "thing", $"the chest entry names '{readThing.Value}', and an entry names one of {EveryEntryKind} (D-1220)");
        }

        if (readCount < 1)
        {
            throw reader.RefuseField(depth, "count", $"the chest entry '{readThing.Value}' holds the count {readCount}, and an entry holds at least 1 (D-1220)");
        }

        if (lesson && readCount != 1)
        {
            throw reader.RefuseField(depth, "count", $"the lesson entry '{readThing.Value}' holds the count {readCount}, and the player owns one copy of a lesson (D-1023)");
        }

        if (lesson && fallback is null)
        {
            throw reader.RefuseField(depth, "fallback", $"the lesson entry '{readThing.Value}' names no fallback item, which it gives when the party owns the lesson (D-1024, D-1220)");
        }

        if (!lesson && fallback is not null)
        {
            throw reader.RefuseField(depth, "fallback", $"the entry '{readThing.Value}' is not a lesson, and a lesson entry alone names a fallback item (D-1024)");
        }

        return new ChestEntry(readThing, readCount, fallback);
    }

    /// <summary>Gives the fault of a chest that gives nothing or names one thing two times (D-1220, T-2).</summary>
    /// <param name="chest">The id of the chest, for the message.</param>
    /// <returns>The fault, or no value when the chest keeps each rule.</returns>
    internal string? FaultOf(ContentId chest)
    {
        if (this.Gold < 0)
        {
            return $"the chest '{chest.Value}' holds {this.Gold} gold, and a chest holds 0 or more (D-1161)";
        }

        if (this.Entries.Count == 0 && this.Gold == 0)
        {
            return $"the chest '{chest.Value}' holds no entry and no gold, and a chest gives something (D-1220)";
        }

        for (int index = 0; index < this.Entries.Count; index += 1)
        {
            for (int earlier = 0; earlier < index; earlier += 1)
            {
                if (string.CompareOrdinal(this.Entries[earlier].Thing.Value, this.Entries[index].Thing.Value) == 0)
                {
                    return $"the chest '{chest.Value}' names '{this.Entries[index].Thing.Value}' in two entries, and one entry holds each thing (D-1220)";
                }
            }

            // The chest keeps what does not fit by its id, so one id names one entry or one
            // fallback of the chest, and never both (D-385).
            if (this.Entries[index].Fallback is ContentId fallback && this.Names(fallback, index))
            {
                return $"the chest '{chest.Value}' names '{fallback.Value}' as a fallback and in another entry or fallback, and one id names one thing of a chest (D-385, D-1220)";
            }
        }

        return null;
    }

    /// <summary>Tells whether an entry other than one index names an id, as its thing or its fallback.</summary>
    private bool Names(ContentId id, int except)
    {
        for (int index = 0; index < this.Entries.Count; index += 1)
        {
            ChestEntry entry = this.Entries[index];
            if (index == except)
            {
                continue;
            }

            if (string.CompareOrdinal(entry.Thing.Value, id.Value) == 0 || (entry.Fallback is ContentId other && string.CompareOrdinal(other.Value, id.Value) == 0))
            {
                return true;
            }
        }

        return false;
    }
}
