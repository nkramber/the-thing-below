using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Core.Battles;

/// <summary>The count of one item or one piece of gear in the pack at the start of a run (D-775, D-1038).</summary>
/// <param name="Id">The id of the item or the piece, of the kind `item` or `gear`.</param>
/// <param name="Count">The count, from 1.</param>
public sealed record PackEntry(ContentId Id, int Count);

/// <summary>The gear that one character of the start party wears at the start of a run (D-44).</summary>
/// <param name="Character">The id of the character.</param>
/// <param name="Gear">The ids of the pieces. Each piece goes to the first empty slot of its kind.</param>
public sealed record StartGear(ContentId Character, IReadOnlyList<ContentId> Gear);

/// <summary>The lessons that one character of the start party carries at the start of a run, in slot order (D-1030).</summary>
/// <param name="Character">The id of the character.</param>
/// <param name="Lessons">The ids of the lessons, one for each slot from the first.</param>
public sealed record StartLessons(ContentId Character, IReadOnlyList<ContentId> Lessons);

/// <summary>
/// The fixture file of the battle core: the characters and the start of a run (D-765,
/// D-775). The groups live in the group file of each region (D-957). The file is `content/rules/fixtures/battle.json`.
/// </summary>
/// <remarks>
/// PR-67 gave each character a join level and a stat curve in place of its fixed stats
/// (D-363, D-966). PR-13 moved the items to the item file, and added the gear in the pack and
/// the start gear (D-1038). Each id stays. PR-11 moved the groups to the group file of each region
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

    /// <summary>The most characters in the party (D-31, D-336).</summary>
    public const int MostCharacters = 3;

    /// <summary>The most enemies on the field (D-31, D-759). The group file reads it (D-957).</summary>
    public const int MostOnField = 6;

    /// <summary>
    /// The most height of the column of the waiting enemies, in art pixels (D-953, D-963). It
    /// is the room at the left edge of the field, between the top margin of the frame and the
    /// message line. A test of Game proves that the layout gives the column this room. PR-12 took
    /// it from 288 to 270, because the command menu grew to two rows (D-1034, D-1035).
    /// </summary>
    public const int MostWaitingHeight = 270;

    /// <summary>The most enemies in a group, the waiting ones included (D-762).</summary>
    public const int MostInGroup = 12;

    /// <summary>The largest health, attack, defense, speed, heal, or count, which keeps every product inside a `long` (T-2).</summary>
    public const int MostStat = 100_000;

    private BattleFixture(
        IReadOnlyList<CharacterRecord> characters,
        IReadOnlyList<ContentId> startParty,
        IReadOnlyList<PackEntry> pack,
        IReadOnlyList<StartGear> startGear,
        IReadOnlyList<StartLessons> startLessons,
        IReadOnlyList<ContentId> lessonPack)
    {
        this.Characters = characters;
        this.StartParty = startParty;
        this.Pack = pack;
        this.StartGear = startGear;
        this.StartLessons = startLessons;
        this.LessonPack = lessonPack;
    }

    /// <summary>Every character, in the order of the file.</summary>
    public IReadOnlyList<CharacterRecord> Characters { get; }

    /// <summary>The characters of the party at the start of a run, in slot order (D-336).</summary>
    public IReadOnlyList<ContentId> StartParty { get; }

    /// <summary>The items and the spare gear of the pack at the start of a run, in the order of the file (D-775, D-1038).</summary>
    public IReadOnlyList<PackEntry> Pack { get; }

    /// <summary>The gear of each character of the start party, in the order of the file (D-44).</summary>
    public IReadOnlyList<StartGear> StartGear { get; }

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
        List<ContentId>? startParty = null;
        List<PackEntry>? pack = null;
        List<StartGear>? startGear = null;
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
                case "start_party":
                    startParty = ReadList(ref reader, ReadCharacterId);
                    break;
                case "pack":
                    pack = ReadList(ref reader, ReadPackEntry);
                    break;
                case "start_gear":
                    startGear = ReadList(ref reader, ReadStartGear);
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
            reader.Require(startParty, depth, "start_party"),
            reader.Require(pack, depth, "pack"),
            reader.Require(startGear, depth, "start_gear"),
            reader.Require(startLessons, depth, "start_lessons"),
            reader.Require(lessonPack, depth, "lesson_pack"));
        reader.ReadFileEnd();

        fixture.CheckIds(file);
        return fixture;
    }

    /// <summary>Gives every content id that the file defines, in the order of the file (D-166).</summary>
    /// <returns>The ids of the characters.</returns>
    public IReadOnlyList<ContentId> DefinedIds()
    {
        List<ContentId> ids = [];
        foreach (CharacterRecord character in this.Characters)
        {
            ids.Add(character.Id);
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

    /// <summary>Reads one pack entry: an `item` or a `gear` id, and a count from 1 (D-1038).</summary>
    private static PackEntry ReadPackEntry(ref ContentReader reader)
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
                case "gear":
                    gear = reader.ReadContentId(GearList.Kind);
                    break;
                case "count":
                    count = ReadStat(ref reader, 1);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        if ((item is null) == (gear is null))
        {
            throw reader.RefuseField(depth, "item", "a pack entry names one item or one piece of gear, and not both (D-1038)");
        }

        return new PackEntry(item ?? gear!, reader.RequireInt(count, depth, "count"));
    }

    private static StartGear ReadStartGear(ref ContentReader reader)
    {
        ContentId? character = null;
        List<ContentId>? gear = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "character":
                    character = reader.ReadContentId(CharacterKind);
                    break;
                case "gear":
                    gear = ReadList(ref reader, ReadGearId);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new StartGear(reader.Require(character, depth, "character"), reader.Require(gear, depth, "gear"));
    }

    private static ContentId ReadGearId(ref ContentReader reader) => reader.ReadContentId(GearList.Kind);

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

        // The battle content checks each id of the pack and of the start gear against the
        // item file and the gear file, and each stack limit (D-1038).
        var packed = new SortedSet<string>(StringComparer.Ordinal);
        foreach (PackEntry entry in this.Pack)
        {
            if (!packed.Add(entry.Id.Value))
            {
                throw ContentException.ForField(file, "pack", $"the pack holds '{entry.Id.Value}' two times, and one entry holds each item or piece");
            }
        }

        var wearers = new SortedSet<string>(StringComparer.Ordinal);
        foreach (StartGear entry in this.StartGear)
        {
            if (!started.Contains(entry.Character.Value))
            {
                throw ContentException.ForField(file, "start_gear", $"the character '{entry.Character.Value}' wears start gear, and it is not in the start party (D-44)");
            }

            if (!wearers.Add(entry.Character.Value))
            {
                throw ContentException.ForField(file, "start_gear", $"the character '{entry.Character.Value}' has two entries of start gear");
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
