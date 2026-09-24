using System;
using System.Collections.Generic;
using System.Text.Json;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Story;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Core.Battles;

/// <summary>
/// The text of the party and the battle inside one snapshot line, from save format 4 (D-531,
/// D-652, D-765). `RunSnapshotText` calls it for the two objects. Save format 5 adds the
/// statuses of each character and each combatant, and drops the push rate, which haste and
/// slow now set (D-792, D-800).
/// </summary>
public static class BattleSnapshotText
{
    private static readonly CombatantPlace[] Places = [CombatantPlace.Field, CombatantPlace.Waiting, CombatantPlace.Down];

    private static readonly BattleOutcome[] Outcomes = [BattleOutcome.Running, BattleOutcome.Won, BattleOutcome.Fled, BattleOutcome.Wiped];

    /// <summary>Writes the party as the object `party`.</summary>
    /// <param name="writer">The writer of the snapshot line.</param>
    /// <param name="party">The party.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static void WriteParty(Utf8JsonWriter writer, PartySnapshot party)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(party);

        writer.WriteStartObject("party");
        writer.WriteStartArray("characters");
        foreach (CharacterValues character in party.Characters)
        {
            writer.WriteStartObject();
            writer.WriteString("id", character.Character.Value);
            writer.WriteNumber("health", character.Health);
            GrowthValues growth = character.Growth ?? throw new ArgumentException($"The character '{character.Character.Value}' holds no level, and a snapshot of this build writes the level of each character (D-966).", nameof(party));
            writer.WriteNumber("level", growth.Level);
            writer.WriteNumber("experience", growth.Experience);
            writer.WriteNumber("mp", growth.Mp);
            writer.WriteString("row", BattleSides.NameOf(character.Row));
            writer.WriteStartArray("statuses");
            foreach (StatusKind status in character.Statuses)
            {
                writer.WriteStringValue(Statuses.NameOf(status));
            }

            writer.WriteEndArray();
            LessonValues lessons = character.Lessons ?? throw new ArgumentException($"The character '{character.Character.Value}' holds no lessons, and a snapshot of this build writes the lessons of each character (D-1018).", nameof(party));
            WriteLessons(writer, lessons);
            IReadOnlyList<ContentId?> gear = character.Gear ?? throw new ArgumentException($"The character '{character.Character.Value}' holds no gear slots, and a snapshot of this build writes the gear of each character (D-44).", nameof(party));
            WriteGear(writer, gear);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteStartArray("pack");
        foreach (PackValues entry in party.Pack)
        {
            // The kind of the id names the field, as in the fixture file (D-1038).
            writer.WriteStartObject();
            writer.WriteString(entry.Id.Kind, entry.Id.Value);
            writer.WriteNumber("count", entry.Count);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        IReadOnlyList<ContentId> lessonPack = party.LessonPack ?? throw new ArgumentException("The party holds no lesson pack, and a snapshot of this build writes it (D-1024).", nameof(party));
        writer.WriteStartArray("lesson_pack");
        foreach (ContentId lesson in lessonPack)
        {
            writer.WriteStringValue(lesson.Value);
        }

        writer.WriteEndArray();
        writer.WriteBoolean("swap_place", party.AtSwapPlace);
        int gold = party.Gold ?? throw new ArgumentException("The party holds no gold, and a snapshot of this build writes it (D-1043).", nameof(party));
        writer.WriteNumber("gold", gold);
        writer.WriteEndObject();
    }

    /// <summary>
    /// Writes the gear of one character as the array `gear`: each filled gear slot with its
    /// index (D-44). The reader of content holds no null, so an empty slot takes no entry.
    /// </summary>
    private static void WriteGear(Utf8JsonWriter writer, IReadOnlyList<ContentId?> gear)
    {
        writer.WriteStartArray("gear");
        for (int index = 0; index < gear.Count; index += 1)
        {
            if (gear[index] is ContentId piece)
            {
                writer.WriteStartObject();
                writer.WriteNumber("slot", index);
                writer.WriteString("gear", piece.Value);
                writer.WriteEndObject();
            }
        }

        writer.WriteEndArray();
    }

    /// <summary>
    /// Writes the lessons of one character as the object `lessons`: the slot count, each filled
    /// slot with its index, and the points of each carried lesson (D-361, D-1018). The reader of
    /// content holds no null, so an empty slot takes no entry.
    /// </summary>
    private static void WriteLessons(Utf8JsonWriter writer, LessonValues lessons)
    {
        writer.WriteStartObject("lessons");
        writer.WriteNumber("slot_count", lessons.Slots.Count);
        writer.WriteStartArray("slots");
        for (int index = 0; index < lessons.Slots.Count; index += 1)
        {
            if (lessons.Slots[index] is ContentId held)
            {
                writer.WriteStartObject();
                writer.WriteNumber("slot", index);
                writer.WriteString("lesson", held.Value);
                writer.WriteEndObject();
            }
        }

        writer.WriteEndArray();
        writer.WriteStartArray("points");
        foreach (LessonPoints entry in lessons.Points)
        {
            writer.WriteStartObject();
            writer.WriteString("lesson", entry.Lesson.Value);
            writer.WriteNumber("points", entry.Points);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    /// <summary>Writes the battle as the object `battle`.</summary>
    /// <param name="writer">The writer of the snapshot line.</param>
    /// <param name="battle">The battle.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static void WriteBattle(Utf8JsonWriter writer, BattleValues battle)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(battle);

        writer.WriteStartObject("battle");
        writer.WriteString("enemy", battle.Enemy.Value);
        writer.WriteString("group", battle.Group.Value);
        writer.WriteNumber("now", battle.Now);
        writer.WriteString("outcome", Battle.OutcomeName(battle.Outcome));
        writer.WriteStartArray("combatants");
        foreach (CombatantValues combatant in battle.Combatants)
        {
            writer.WriteStartObject();
            writer.WriteString("side", BattleSides.NameOf(combatant.Side));
            writer.WriteNumber("slot", combatant.Slot);
            writer.WriteString("id", combatant.Id.Value);
            writer.WriteNumber("health", combatant.Health);
            writer.WriteString("row", BattleSides.NameOf(combatant.Row));
            writer.WriteString("place", Battle.PlaceName(combatant.Place));
            writer.WriteNumber("ready_at", combatant.ReadyAt);
            writer.WriteBoolean("defending", combatant.Defending);
            writer.WriteStartArray("statuses");
            foreach (StatusValues status in combatant.Statuses)
            {
                writer.WriteStartObject();
                writer.WriteString("status", Statuses.NameOf(status.Status));
                if (status.EndsAt is long ends)
                {
                    writer.WriteNumber("ends_at", ends);
                }

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        StealValues steals = battle.Steals ?? throw new ArgumentException("The battle holds no steals, and a snapshot of this build writes them (D-1045).", nameof(battle));
        writer.WriteStartObject("steals");
        writer.WriteNumber("tries", steals.Tries);
        writer.WriteStartArray("taken");
        foreach (StolenEntry entry in steals.Taken)
        {
            writer.WriteStartObject();
            writer.WriteNumber("enemy", entry.Enemy);
            writer.WriteNumber("entry", entry.Entry);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.WriteEndObject();
    }

    /// <summary>Reads the object `party`.</summary>
    /// <param name="reader">The reader of the snapshot line.</param>
    /// <param name="format">The save format of the line, 4 or later. Format 4 holds no status, and each character then holds none (D-792).</param>
    /// <returns>The party.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed (T-2).</exception>
    public static PartySnapshot ReadParty(ref ContentReader reader, int format)
    {
        List<CharacterValues>? characters = null;
        List<PackValues>? pack = null;
        List<ContentId>? lessonPack = null;
        bool? atSwapPlace = null;
        int? gold = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                // Save format 10 adds the lesson pack and the swap place (D-1024, D-1030).
                case "lesson_pack" when format >= 10:
                    lessonPack = [];
                    int lessonDepth = reader.ReadArrayStart();
                    while (reader.ReadNextElement(lessonDepth, lessonPack.Count))
                    {
                        lessonPack.Add(reader.ReadContentId(LessonList.Kind));
                    }

                    break;
                case "swap_place" when format >= 10:
                    atSwapPlace = reader.ReadBoolean();
                    break;

                // Save format 11 adds the gold, the gear slots, and the spare gear (D-1038, D-1043).
                case "gold" when format >= 11:
                    gold = reader.ReadInt();
                    break;
                case "characters":
                    characters = [];
                    int charactersDepth = reader.ReadArrayStart();
                    while (reader.ReadNextElement(charactersDepth, characters.Count))
                    {
                        characters.Add(ReadCharacter(ref reader, format));
                    }

                    break;
                case "pack":
                    pack = [];
                    int packDepth = reader.ReadArrayStart();
                    while (reader.ReadNextElement(packDepth, pack.Count))
                    {
                        pack.Add(ReadPackEntry(ref reader, format));
                    }

                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new PartySnapshot(
            reader.Require(characters, depth, "characters"),
            reader.Require(pack, depth, "pack"),
            format >= 10 ? reader.Require(lessonPack, depth, "lesson_pack") : null,
            format >= 10 && reader.RequireValue(atSwapPlace, depth, "swap_place"),
            format >= 11 ? reader.RequireInt(gold, depth, "gold") : null);
    }

    /// <summary>
    /// Reads the id of what started the battle: a patrol of the map, or from save format 9 the
    /// story scene of a start battle step (D-749, D-998).
    /// </summary>
    private static ContentId ReadEnemy(ref ContentReader reader, int format)
    {
        ContentId id = reader.ReadContentId();
        bool patrol = string.CompareOrdinal(id.Kind, Patrol.IdKind) == 0;
        bool scene = format >= 9 && string.CompareOrdinal(id.Kind, StoryScene.Kind) == 0;
        if (!patrol && !scene)
        {
            throw reader.Refuse($"the battle names '{id.Value}', and a battle of save format {format} names a patrol{(format >= 9 ? " or a story scene" : string.Empty)} (D-749, D-998)");
        }

        return id;
    }

    /// <summary>Reads the object `battle`.</summary>
    /// <param name="reader">The reader of the snapshot line.</param>
    /// <param name="format">The save format of the line, 4 or later. Format 4 holds a push rate and no status (D-792).</param>
    /// <returns>The battle.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or malformed (T-2).</exception>
    public static BattleValues ReadBattle(ref ContentReader reader, int format)
    {
        ContentId? enemy = null;
        ContentId? group = null;
        long? now = null;
        BattleOutcome? outcome = null;
        List<CombatantValues>? combatants = null;
        StealValues? steals = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "enemy":
                    enemy = ReadEnemy(ref reader, format);
                    break;
                case "group":
                    group = reader.ReadContentId(Patrol.GroupKind);
                    break;
                case "now":
                    now = reader.ReadLong();
                    break;
                case "outcome":
                    outcome = ReadOutcome(ref reader);
                    break;

                // Save format 11 adds the steal tries and the stolen entries (D-1044, D-1045).
                case "steals" when format >= 11:
                    steals = ReadSteals(ref reader);
                    break;
                case "combatants":
                    combatants = [];
                    int combatantsDepth = reader.ReadArrayStart();
                    while (reader.ReadNextElement(combatantsDepth, combatants.Count))
                    {
                        combatants.Add(ReadCombatant(ref reader, format));
                    }

                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new BattleValues(
            reader.Require(enemy, depth, "enemy"),
            reader.Require(group, depth, "group"),
            reader.RequireValue(now, depth, "now"),
            reader.RequireValue(outcome, depth, "outcome"),
            reader.Require(combatants, depth, "combatants"),
            format >= 11 ? reader.Require(steals, depth, "steals") : null);
    }

    private static StealValues ReadSteals(ref ContentReader reader)
    {
        int? tries = null;
        List<StolenEntry>? taken = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "tries":
                    tries = reader.ReadInt();
                    break;
                case "taken":
                    taken = [];
                    int takenDepth = reader.ReadArrayStart();
                    while (reader.ReadNextElement(takenDepth, taken.Count))
                    {
                        taken.Add(ReadStolenEntry(ref reader));
                    }

                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new StealValues(reader.RequireInt(tries, depth, "tries"), reader.Require(taken, depth, "taken"));
    }

    private static StolenEntry ReadStolenEntry(ref ContentReader reader)
    {
        int? enemy = null;
        int? entry = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "enemy":
                    enemy = reader.ReadInt();
                    break;
                case "entry":
                    entry = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new StolenEntry(reader.RequireInt(enemy, depth, "enemy"), reader.RequireInt(entry, depth, "entry"));
    }

    private static CharacterValues ReadCharacter(ref ContentReader reader, int format)
    {
        ContentId? id = null;
        int? health = null;
        BattleRow? row = null;
        IReadOnlyList<StatusKind>? statuses = null;
        int? level = null;
        int? experience = null;
        int? mp = null;
        LessonValues? lessons = null;
        IReadOnlyList<ContentId?>? gear = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(BattleFixture.CharacterKind);
                    break;
                case "lessons" when format >= 10:
                    lessons = ReadLessons(ref reader);
                    break;
                case "gear" when format >= 11:
                    gear = ReadGear(ref reader);
                    break;
                case "health":
                    health = reader.ReadInt();
                    break;
                case "row":
                    row = ReadRow(ref reader);
                    break;
                case "level" when format >= 7:
                    level = reader.ReadInt();
                    break;
                case "experience" when format >= 7:
                    experience = reader.ReadInt();
                    break;
                case "mp" when format >= 7:
                    mp = reader.ReadInt();
                    break;
                case "statuses" when format >= 5:
                    statuses = Statuses.ReadList(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new CharacterValues(
            reader.Require(id, depth, "id"),
            reader.RequireInt(health, depth, "health"),
            reader.RequireValue(row, depth, "row"),
            format >= 5 ? reader.Require(statuses, depth, "statuses") : [],
            format >= 7 ? ReadGrowth(ref reader, depth, level, experience, mp) : null,
            format >= 10 ? reader.Require(lessons, depth, "lessons") : null,
            format >= 11 ? reader.Require(gear, depth, "gear") : null);
    }

    /// <summary>Reads the gear of one character (D-44). Each filled gear slot names its index, from 0 to 5, in order and with no repeat.</summary>
    private static ContentId?[] ReadGear(ref ContentReader reader)
    {
        var gear = new ContentId?[GearRules.SlotCount];
        int last = -1;
        int count = 0;
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, count))
        {
            int? slot = null;
            ContentId? piece = null;
            int entryDepth = reader.ReadObjectStart();
            while (reader.ReadNextField(entryDepth, out string field))
            {
                switch (field)
                {
                    case "slot":
                        slot = reader.ReadInt();
                        break;
                    case "gear":
                        piece = reader.ReadContentId(GearList.Kind);
                        break;
                    default:
                        throw reader.UnknownField(field);
                }
            }

            int readSlot = reader.RequireInt(slot, entryDepth, "slot");
            if (readSlot <= last || readSlot >= GearRules.SlotCount)
            {
                throw reader.RefuseField(entryDepth, "slot", $"the gear slot {readSlot} repeats, leaves the order, or is outside 0 to {GearRules.SlotCount - 1} (D-44)");
            }

            gear[readSlot] = reader.Require(piece, entryDepth, "gear");
            last = readSlot;
            count += 1;
        }

        return gear;
    }

    /// <summary>
    /// Reads the lessons of one character (D-361, D-1018). Each filled slot names its index,
    /// and the slots rise with no repeat inside the slot count. The resume checks the count
    /// against the level and each id against the lesson file.
    /// </summary>
    private static LessonValues ReadLessons(ref ContentReader reader)
    {
        int? count = null;
        List<(int Slot, ContentId Lesson)>? filled = null;
        List<LessonPoints>? points = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "slot_count":
                    count = reader.ReadInt();
                    break;
                case "slots":
                    filled = [];
                    int slotsDepth = reader.ReadArrayStart();
                    while (reader.ReadNextElement(slotsDepth, filled.Count))
                    {
                        filled.Add(ReadFilledSlot(ref reader));
                    }

                    break;
                case "points":
                    points = [];
                    int pointsDepth = reader.ReadArrayStart();
                    while (reader.ReadNextElement(pointsDepth, points.Count))
                    {
                        points.Add(ReadLessonPoints(ref reader));
                    }

                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        int slotCount = reader.RequireInt(count, depth, "slot_count");
        if (slotCount < 0 || slotCount > StatCurve.HighestLevel)
        {
            throw reader.RefuseField(depth, "slot_count", $"the slot count {slotCount} is outside 0 to {StatCurve.HighestLevel} (D-1018)");
        }

        var slots = new ContentId?[slotCount];
        int last = -1;
        foreach ((int slot, ContentId lesson) in reader.Require(filled, depth, "slots"))
        {
            if (slot <= last || slot >= slotCount)
            {
                throw reader.RefuseField(depth, "slots", $"the slot {slot} repeats, falls, or lies outside 0 to {slotCount - 1} (D-1018)");
            }

            slots[slot] = lesson;
            last = slot;
        }

        return new LessonValues(slots, reader.Require(points, depth, "points"));
    }

    private static (int Slot, ContentId Lesson) ReadFilledSlot(ref ContentReader reader)
    {
        int? slot = null;
        ContentId? lesson = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "slot":
                    slot = reader.ReadInt();
                    break;
                case "lesson":
                    lesson = reader.ReadContentId(LessonList.Kind);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return (reader.RequireInt(slot, depth, "slot"), reader.Require(lesson, depth, "lesson"));
    }

    private static LessonPoints ReadLessonPoints(ref ContentReader reader)
    {
        ContentId? lesson = null;
        int? points = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "lesson":
                    lesson = reader.ReadContentId(LessonList.Kind);
                    break;
                case "points":
                    points = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new LessonPoints(reader.Require(lesson, depth, "lesson"), reader.RequireInt(points, depth, "points"));
    }

    // Save format 7 adds the level, the experience, and the MP (D-966). An older snapshot
    // holds none, and the resume starts the character at its join level (D-166, D-363).
    private static GrowthValues ReadGrowth(ref ContentReader reader, int depth, int? level, int? experience, int? mp) =>
        new(
            reader.RequireInt(level, depth, "level"),
            reader.RequireInt(experience, depth, "experience"),
            reader.RequireInt(mp, depth, "mp"));

    /// <summary>Reads one pack entry: an item, or from save format 11 an item or a piece of gear (D-1038).</summary>
    private static PackValues ReadPackEntry(ref ContentReader reader, int format)
    {
        ContentId? item = null;
        ContentId? gear = null;
        int? count = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "item":
                    item = reader.ReadContentId(ItemList.Kind);
                    break;
                case "gear" when format >= 11:
                    gear = reader.ReadContentId(GearList.Kind);
                    break;
                case "count":
                    count = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        if ((item is null) == (gear is null))
        {
            throw reader.RefuseField(depth, "item", "a pack entry names one item or one piece of gear, and not both (D-1038)");
        }

        return new PackValues(item ?? gear!, reader.RequireInt(count, depth, "count"));
    }

    private static CombatantValues ReadCombatant(ref ContentReader reader, int format)
    {
        BattleSide? side = null;
        int? slot = null;
        ContentId? id = null;
        int? health = null;
        BattleRow? row = null;
        CombatantPlace? place = null;
        long? readyAt = null;
        int? pushRate = null;
        bool? defending = null;
        List<StatusValues>? statuses = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "side":
                    side = ReadSide(ref reader);
                    break;
                case "slot":
                    slot = reader.ReadInt();
                    break;
                case "id":
                    id = reader.ReadContentId();
                    break;
                case "health":
                    health = reader.ReadInt();
                    break;
                case "row":
                    row = ReadRow(ref reader);
                    break;
                case "place":
                    place = ReadPlace(ref reader);
                    break;
                case "ready_at":
                    readyAt = reader.ReadLong();
                    break;
                case "push_rate" when format == 4:
                    pushRate = reader.ReadInt();
                    break;
                case "defending":
                    defending = reader.ReadBoolean();
                    break;
                case "statuses" when format >= 5:
                    statuses = [];
                    int statusesDepth = reader.ReadArrayStart();
                    while (reader.ReadNextElement(statusesDepth, statuses.Count))
                    {
                        statuses.Add(ReadStatus(ref reader));
                    }

                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        // Save format 4 predates the statuses, and its push rate came from a test seam alone.
        // A rate other than 10000 names haste or slow with no end, which no status of format
        // 5 can hold, so the migration refuses it (T-2, D-798).
        if (format == 4)
        {
            int rate = reader.RequireInt(pushRate, depth, "push_rate");
            if (rate != BasisPoints.One)
            {
                throw reader.RefuseField(
                    depth,
                    "push_rate",
                    $"the push rate {rate} of save format 4 names haste or slow with no end, and format 5 holds each status with its end (D-798)");
            }
        }

        return new CombatantValues(
            reader.RequireValue(side, depth, "side"),
            reader.RequireInt(slot, depth, "slot"),
            reader.Require(id, depth, "id"),
            reader.RequireInt(health, depth, "health"),
            reader.RequireValue(row, depth, "row"),
            reader.RequireValue(place, depth, "place"),
            reader.RequireValue(readyAt, depth, "ready_at"),
            reader.RequireValue(defending, depth, "defending"),
            format >= 5 ? reader.Require(statuses, depth, "statuses") : []);
    }

    private static StatusValues ReadStatus(ref ContentReader reader)
    {
        StatusKind? status = null;
        long? endsAt = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "status":
                    status = ReadStatusName(ref reader);
                    break;
                case "ends_at":
                    endsAt = reader.ReadLong();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        StatusKind held = reader.RequireValue(status, depth, "status");
        if (!Statuses.Lasts(held))
        {
            _ = reader.RequireValue(endsAt, depth, "ends_at");
        }
        else if (endsAt is not null)
        {
            throw reader.RefuseField(depth, "ends_at", $"the status '{Statuses.NameOf(held)}' lasts until a cure and holds no end (D-390)");
        }

        return new StatusValues(held, endsAt);
    }

    private static StatusKind ReadStatusName(ref ContentReader reader)
    {
        string name = reader.ReadString();
        if (!Statuses.TryOf(name, out StatusKind status))
        {
            throw reader.Refuse($"the status '{name}' is not one of {Statuses.EveryName} (D-75)");
        }

        return status;
    }

    private static BattleSide ReadSide(ref ContentReader reader)
    {
        string name = reader.ReadString();
        if (!BattleSides.TrySideOf(name, out BattleSide side))
        {
            throw reader.Refuse($"the side '{name}' is not one of {BattleSides.EverySideName} (D-31)");
        }

        return side;
    }

    private static BattleRow ReadRow(ref ContentReader reader)
    {
        string name = reader.ReadString();
        if (!BattleSides.TryRowOf(name, out BattleRow row))
        {
            throw reader.Refuse($"the row '{name}' is not one of {BattleSides.EveryRowName} (D-377)");
        }

        return row;
    }

    private static CombatantPlace ReadPlace(ref ContentReader reader)
    {
        string name = reader.ReadString();
        foreach (CombatantPlace place in Places)
        {
            if (string.CompareOrdinal(Battle.PlaceName(place), name) == 0)
            {
                return place;
            }
        }

        throw reader.Refuse($"the place '{name}' is not one of field, waiting, down (D-758)");
    }

    private static BattleOutcome ReadOutcome(ref ContentReader reader)
    {
        string name = reader.ReadString();
        foreach (BattleOutcome outcome in Outcomes)
        {
            if (string.CompareOrdinal(Battle.OutcomeName(outcome), name) == 0)
            {
                return outcome;
            }
        }

        throw reader.Refuse($"the outcome '{name}' is not one of running, won, fled, wiped (D-36)");
    }
}
