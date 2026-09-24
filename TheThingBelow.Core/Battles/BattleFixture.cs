using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Core.Battles;

/// <summary>One item that a character can use in battle, until the items of PR-13 (D-775).</summary>
/// <param name="Id">The id, of the kind `item`.</param>
/// <param name="Heal">The health that the item restores outside a battle (D-382).</param>
/// <param name="Delay">The delay of a use, in ticks at speed 100 (D-757).</param>
public sealed record ItemRecord(ContentId Id, int Heal, int Delay);

/// <summary>The count of one item in the pack at the start of a run (D-775).</summary>
/// <param name="Item">The id of the item.</param>
/// <param name="Count">The count.</param>
public sealed record PackEntry(ContentId Item, int Count);

/// <summary>The lessons that one character of the start party carries at the start of a run, in slot order (D-1030).</summary>
/// <param name="Character">The id of the character.</param>
/// <param name="Lessons">The ids of the lessons, one for each slot from the first.</param>
public sealed record StartLessons(ContentId Character, IReadOnlyList<ContentId> Lessons);

/// <summary>
/// The fixture file of the battle core: the characters, the items, and the start of a run
/// (D-765, D-775). The groups live in the group file of each region (D-957). The file is `content/rules/fixtures/battle.json`.
/// </summary>
/// <remarks>
/// PR-67 gave each character a join level and a stat curve in place of its fixed stats
/// (D-363, D-966). PR-13 replaces the items with the pack (D-775). Each id stays. PR-11 moved the groups to the group file of each region
/// with the same ids (D-766, D-957).
/// PR-80 moved the enemies to the enemy record, and the battle content checks that each
/// group names a record (D-557, D-786).
/// </remarks>
public sealed class BattleFixture
{
    /// <summary>The path of the file under the content folder (D-766).</summary>
    public const string Path = "rules/fixtures/battle.json";

    /// <summary>The kind of a character id.</summary>
    public const string CharacterKind = "character";

    /// <summary>The kind of an item id.</summary>
    public const string ItemKind = "item";

    /// <summary>The most characters in the party (D-31, D-336).</summary>
    public const int MostCharacters = 3;

    /// <summary>The most enemies on the field (D-31, D-759). The group file reads it (D-957).</summary>
    public const int MostOnField = 6;

    /// <summary>
    /// The most height of the column of the waiting enemies, in art pixels (D-953, D-963). It
    /// is the room at the left edge of the field, between the top margin of the frame and the
    /// message line. A test of Game proves that the layout gives the column this room.
    /// </summary>
    public const int MostWaitingHeight = 288;

    /// <summary>The most enemies in a group, the waiting ones included (D-762).</summary>
    public const int MostInGroup = 12;

    /// <summary>The largest health, attack, defense, speed, heal, or count, which keeps every product inside a `long` (T-2).</summary>
    public const int MostStat = 100_000;

    private BattleFixture(
        IReadOnlyList<CharacterRecord> characters,
        IReadOnlyList<ItemRecord> items,
        IReadOnlyList<ContentId> startParty,
        IReadOnlyList<PackEntry> pack,
        IReadOnlyList<StartLessons> startLessons,
        IReadOnlyList<ContentId> lessonPack)
    {
        this.Characters = characters;
        this.Items = items;
        this.StartParty = startParty;
        this.Pack = pack;
        this.StartLessons = startLessons;
        this.LessonPack = lessonPack;
    }

    /// <summary>Every character, in the order of the file.</summary>
    public IReadOnlyList<CharacterRecord> Characters { get; }

    /// <summary>Every item, in the order of the file.</summary>
    public IReadOnlyList<ItemRecord> Items { get; }

    /// <summary>The characters of the party at the start of a run, in slot order (D-336).</summary>
    public IReadOnlyList<ContentId> StartParty { get; }

    /// <summary>The pack at the start of a run, in the order of the file (D-775).</summary>
    public IReadOnlyList<PackEntry> Pack { get; }

    /// <summary>The lessons of each character of the start party, in the order of the file (D-1030).</summary>
    public IReadOnlyList<StartLessons> StartLessons { get; }

    /// <summary>The owned lessons that no character carries at the start of a run, in the order of the file (D-1024).</summary>
    public IReadOnlyList<ContentId> LessonPack { get; }

    /// <summary>Reads the fixture file, and checks every record and every id that one record names (T-2).</summary>
    /// <param name="bytes">The bytes of the file.</param>
    /// <param name="file">The path of the file, for an error.</param>
    /// <returns>The fixture.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, repeated, or out of its range, or an id names no record (G-6, T-2).</exception>
    public static BattleFixture Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        string? comment = null;
        List<CharacterRecord>? characters = null;
        List<ItemRecord>? items = null;
        List<ContentId>? startParty = null;
        List<PackEntry>? pack = null;
        List<StartLessons>? startLessons = null;
        List<ContentId>? lessonPack = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "characters":
                    characters = ReadList(ref reader, ReadCharacter);
                    break;
                case "items":
                    items = ReadList(ref reader, ReadItem);
                    break;
                case "start_party":
                    startParty = ReadList(ref reader, ReadCharacterId);
                    break;
                case "pack":
                    pack = ReadList(ref reader, ReadPackEntry);
                    break;
                case "start_lessons":
                    startLessons = ReadList(ref reader, ReadStartLessons);
                    break;
                case "lesson_pack":
                    lessonPack = ReadList(ref reader, ReadLessonId);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        var fixture = new BattleFixture(
            reader.Require(characters, depth, "characters"),
            reader.Require(items, depth, "items"),
            reader.Require(startParty, depth, "start_party"),
            reader.Require(pack, depth, "pack"),
            reader.Require(startLessons, depth, "start_lessons"),
            reader.Require(lessonPack, depth, "lesson_pack"));
        reader.ReadFileEnd();

        fixture.CheckIds(file);
        return fixture;
    }

    /// <summary>Gives every content id that the file defines, in the order of the file (D-166).</summary>
    /// <returns>The ids of the characters and the items.</returns>
    public IReadOnlyList<ContentId> DefinedIds()
    {
        List<ContentId> ids = [];
        foreach (CharacterRecord character in this.Characters)
        {
            ids.Add(character.Id);
        }

        foreach (ItemRecord item in this.Items)
        {
            ids.Add(item.Id);
        }

        return ids;
    }

    private delegate T ReadOne<T>(ref ContentReader reader);

    private static List<T> ReadList<T>(ref ContentReader reader, ReadOne<T> readOne)
    {
        List<T> list = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, list.Count))
        {
            list.Add(readOne(ref reader));
        }

        return list;
    }

    private static CharacterRecord ReadCharacter(ref ContentReader reader)
    {
        ContentId? id = null;
        BattleRow? row = null;
        int? joinLevel = null;
        List<StatRow>? curve = null;
        AptitudeKind? main = null;
        AptitudeKind? side = null;
        ContentId? sideFlag = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(CharacterKind);
                    break;
                case "row":
                    row = ReadRow(ref reader);
                    break;
                case "join_level":
                    joinLevel = ReadLevel(ref reader);
                    break;
                case "curve":
                    curve = StatCurve.Read(ref reader);
                    break;
                case "main_aptitude":
                    main = ReadAptitude(ref reader);
                    break;
                case "side_aptitude":
                    side = ReadAptitude(ref reader);
                    break;
                case "side_flag":
                    sideFlag = reader.ReadContentId(FlagList.Kind);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        ContentId readId = reader.Require(id, depth, "id");
        AptitudeKind readMain = reader.RequireValue(main, depth, "main_aptitude");
        AptitudeKind readSide = reader.RequireValue(side, depth, "side_aptitude");
        if (readMain == readSide)
        {
            throw reader.RefuseField(
                depth,
                "side_aptitude",
                $"the side aptitude of '{readId.Value}' is '{Aptitudes.NameOf(readSide)}', its main aptitude, and a side aptitude never matches the main aptitude (D-281)");
        }

        return new CharacterRecord(
            readId,
            reader.RequireValue(row, depth, "row"),
            reader.RequireInt(joinLevel, depth, "join_level"),
            reader.Require(curve, depth, "curve"),
            readMain,
            readSide,
            reader.Require(sideFlag, depth, "side_flag"));
    }

    private static AptitudeKind ReadAptitude(ref ContentReader reader)
    {
        string name = reader.ReadString();
        if (!Aptitudes.TryOf(name, out AptitudeKind kind))
        {
            throw reader.Refuse($"the aptitude '{name}' is not one of {Aptitudes.EveryName} (D-281)");
        }

        return kind;
    }

    private static ItemRecord ReadItem(ref ContentReader reader)
    {
        ContentId? id = null;
        int? heal = null;
        int? delay = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(ItemKind);
                    break;
                case "heal":
                    heal = ReadStat(ref reader, 1);
                    break;
                case "delay":
                    delay = ReadStat(ref reader, 1);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new ItemRecord(
            reader.Require(id, depth, "id"),
            reader.RequireInt(heal, depth, "heal"),
            reader.RequireInt(delay, depth, "delay"));
    }

    private static ContentId ReadCharacterId(ref ContentReader reader) => reader.ReadContentId(CharacterKind);

    private static ContentId ReadLessonId(ref ContentReader reader) => reader.ReadContentId(LessonList.Kind);

    private static StartLessons ReadStartLessons(ref ContentReader reader)
    {
        ContentId? character = null;
        List<ContentId>? lessons = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "character":
                    character = reader.ReadContentId(CharacterKind);
                    break;
                case "lessons":
                    lessons = ReadList(ref reader, ReadLessonId);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new StartLessons(reader.Require(character, depth, "character"), reader.Require(lessons, depth, "lessons"));
    }

    private static PackEntry ReadPackEntry(ref ContentReader reader)
    {
        ContentId? item = null;
        int? count = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "item":
                    item = reader.ReadContentId(ItemKind);
                    break;
                case "count":
                    count = ReadStat(ref reader, 0);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new PackEntry(reader.Require(item, depth, "item"), reader.RequireInt(count, depth, "count"));
    }

    /// <summary>Reads a stat, and refuses a value outside the lowest value to <see cref="MostStat"/> (T-2).</summary>
    /// <param name="reader">The reader, at the value.</param>
    /// <param name="lowest">The lowest value of the stat.</param>
    /// <returns>The value.</returns>
    /// <remarks>The enemy record reads its stats with this rule too (D-557).</remarks>
    internal static int ReadStat(ref ContentReader reader, int lowest)
    {
        int value = reader.ReadInt();
        if (value < lowest || value > MostStat)
        {
            throw reader.Refuse($"the value {value} is outside {lowest} to {MostStat} (T-2)");
        }

        return value;
    }

    /// <summary>Reads a character level, and refuses a value outside 1 to <see cref="StatCurve.HighestLevel"/> (D-972).</summary>
    /// <param name="reader">The reader, at the value.</param>
    /// <returns>The level.</returns>
    /// <remarks>The enemy record reads its level with this rule too (D-968).</remarks>
    internal static int ReadLevel(ref ContentReader reader)
    {
        int value = reader.ReadInt();
        if (value < 1 || value > StatCurve.HighestLevel)
        {
            throw reader.Refuse($"the level {value} is outside 1 to {StatCurve.HighestLevel} (D-972)");
        }

        return value;
    }

    /// <summary>Reads a row by its name (D-377). The group file reads each row with this rule too (D-957).</summary>
    /// <param name="reader">The reader, at the value.</param>
    /// <returns>The row.</returns>
    internal static BattleRow ReadRow(ref ContentReader reader)
    {
        string name = reader.ReadString();
        if (!BattleSides.TryRowOf(name, out BattleRow row))
        {
            throw reader.Refuse($"the row '{name}' is not one of {BattleSides.EveryRowName} (D-377)");
        }

        return row;
    }

    /// <summary>
    /// Refuses a repeated id, and an id that names no record of this file. The ids of the
    /// file are few, so a walk of each list reads better than a map (T-1).
    /// </summary>
    private void CheckIds(string file)
    {
        var defined = new SortedSet<string>(StringComparer.Ordinal);
        foreach (ContentId id in this.DefinedIds())
        {
            if (!defined.Add(id.Value))
            {
                throw ContentException.ForField(file, id.Value, "the file defines this id two times, and an id is permanent (D-166)");
            }
        }

        if (this.StartParty.Count == 0 || this.StartParty.Count > MostCharacters)
        {
            throw ContentException.ForField(
                file,
                "start_party",
                $"the party starts with {this.StartParty.Count} characters, and a party holds 1 to {MostCharacters} (D-31, D-336)");
        }

        var started = new SortedSet<string>(StringComparer.Ordinal);
        foreach (ContentId character in this.StartParty)
        {
            Require(defined, file, character, "the start party");
            if (!started.Add(character.Value))
            {
                throw ContentException.ForField(file, "start_party", $"the party starts with '{character.Value}' two times");
            }
        }

        var packed = new SortedSet<string>(StringComparer.Ordinal);
        foreach (PackEntry entry in this.Pack)
        {
            Require(defined, file, entry.Item, "the pack");
            if (!packed.Add(entry.Item.Value))
            {
                throw ContentException.ForField(file, "pack", $"the pack holds '{entry.Item.Value}' two times, and one entry holds each item");
            }
        }

        this.CheckStartLessons(file, started);
    }

    /// <summary>
    /// Refuses start lessons of a character outside the start party, a character with two
    /// entries, and one lesson two times across the start lessons and the lesson pack, because
    /// the player never owns two copies of one lesson (D-1023). The battle content checks each
    /// lesson id and each slot count.
    /// </summary>
    private void CheckStartLessons(string file, SortedSet<string> started)
    {
        var carriers = new SortedSet<string>(StringComparer.Ordinal);
        var owned = new SortedSet<string>(StringComparer.Ordinal);
        foreach (StartLessons entry in this.StartLessons)
        {
            if (!started.Contains(entry.Character.Value))
            {
                throw ContentException.ForField(file, "start_lessons", $"the character '{entry.Character.Value}' carries start lessons, and it is not in the start party (D-1030)");
            }

            if (!carriers.Add(entry.Character.Value))
            {
                throw ContentException.ForField(file, "start_lessons", $"the character '{entry.Character.Value}' has two entries of start lessons");
            }

            foreach (ContentId lesson in entry.Lessons)
            {
                if (!owned.Add(lesson.Value))
                {
                    throw ContentException.ForField(file, "start_lessons", $"the lesson '{lesson.Value}' starts two times, and the player never owns two copies of one lesson (D-1023)");
                }
            }
        }

        foreach (ContentId lesson in this.LessonPack)
        {
            if (!owned.Add(lesson.Value))
            {
                throw ContentException.ForField(file, "lesson_pack", $"the lesson '{lesson.Value}' starts two times, and the player never owns two copies of one lesson (D-1023)");
            }
        }
    }

    private static void Require(SortedSet<string> defined, string file, ContentId id, string owner)
    {
        if (!defined.Contains(id.Value))
        {
            throw ContentException.ForField(file, id.Value, $"{owner} names this id, and the file defines no record with it (T-2)");
        }
    }
}
