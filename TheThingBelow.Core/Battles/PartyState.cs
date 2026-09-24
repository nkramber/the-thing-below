using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Hashing;

namespace TheThingBelow.Core.Battles;

/// <summary>The stored values of one character of the party (D-765).</summary>
/// <param name="Character">The id of the character.</param>
/// <param name="Health">The health now. Zero is a down (D-36).</param>
/// <param name="Row">The row now (D-558).</param>
/// <param name="Statuses">Poison, blind, and silence, in the order of D-75, which last past a fight (D-390, D-792).</param>
/// <param name="Growth">The level, the experience, and the MP, from save format 7 (D-966). Null in a snapshot of an older format, and the resume then starts the character at its join level with full MP (D-166, D-363).</param>
/// <param name="Lessons">The lesson slots and the points of each lesson, from save format 10 (D-361, D-1018). Null in a snapshot of an older format, and the resume then gives the start lessons of the fixture (D-166).</param>
/// <param name="Gear">The six gear slots, each with a gear id or no value for an empty slot, from save format 11 (D-44). Null in a snapshot of an older format, and the resume then gives the start gear of the fixture (D-166).</param>
public sealed record CharacterValues(ContentId Character, int Health, BattleRow Row, IReadOnlyList<StatusKind> Statuses, GrowthValues? Growth, LessonValues? Lessons, IReadOnlyList<ContentId?>? Gear);

/// <summary>The stored lessons of one character (D-356, D-361).</summary>
/// <param name="Slots">The lesson of each slot, and no value for an empty slot. The count is the slot count of the level (D-1018).</param>
/// <param name="Points">The points of each lesson that the character ever carried, in the ordinal order of the lesson ids (D-361).</param>
public sealed record LessonValues(IReadOnlyList<ContentId?> Slots, IReadOnlyList<LessonPoints> Points);

/// <summary>The points of one character for one lesson (D-357, D-361).</summary>
/// <param name="Lesson">The id of the lesson.</param>
/// <param name="Points">The points, from zero to the total of the last form (D-1021).</param>
public sealed record LessonPoints(ContentId Lesson, int Points);

/// <summary>The stored level, experience, and MP of one character (D-34, D-42, D-966).</summary>
/// <param name="Level">The character level, from 1 to 40 (D-972).</param>
/// <param name="Experience">The total experience, which gives the level through the table of the rules (D-971).</param>
/// <param name="Mp">The MP now, from zero to the full MP of the level (D-42).</param>
public sealed record GrowthValues(int Level, int Experience, int Mp);

/// <summary>The stored count of one item or one piece of spare gear in the pack (D-775, D-1038).</summary>
/// <param name="Id">The id of the item or the piece, of the kind `item` or `gear`.</param>
/// <param name="Count">The count now, from 1.</param>
public sealed record PackValues(ContentId Id, int Count);

/// <summary>One character of the party, with the level, the health, the MP, and the row that last between battles (D-34, D-36, D-42, D-765).</summary>
public sealed class PartyMember
{
    private readonly SortedDictionary<string, LessonPoints> points = new(StringComparer.Ordinal);
    private readonly ContentId?[] gear;
    private ContentId?[] slots;

    internal PartyMember(CharacterRecord record, GrowthValues growth, int health, BattleRow row, IReadOnlyList<StatusKind> statuses, LessonValues lessons, IReadOnlyList<ContentId?> gear)
    {
        this.gear = CopyOf(gear);
        this.Record = record;
        this.Level = growth.Level;
        this.Experience = growth.Experience;
        this.Mp = growth.Mp;
        this.Health = health;
        this.Row = row;
        this.Statuses = statuses;
        this.slots = CopyOf(lessons.Slots);
        foreach (LessonPoints entry in lessons.Points)
        {
            this.points.Add(entry.Lesson.Value, entry);
        }
    }

    /// <summary>The lesson of each slot, and no value for an empty slot. The count grows with the level (D-356, D-1018).</summary>
    public IReadOnlyList<ContentId?> Slots => this.slots;

    /// <summary>The six gear slots: the weapon, the off-hand, the head, the body, and two accessories. Each holds a gear id, or no value when it is empty (D-44).</summary>
    public IReadOnlyList<ContentId?> Gear => this.gear;

    /// <summary>The points of each lesson that the character ever carried, in the ordinal order of the lesson ids (D-361).</summary>
    public IReadOnlyList<LessonPoints> Points
    {
        get
        {
            List<LessonPoints> all = [];
            foreach (LessonPoints entry in this.points.Values)
            {
                all.Add(entry);
            }

            return all;
        }
    }

    /// <summary>The join level and the stat curve of the character (D-363, D-966).</summary>
    public CharacterRecord Record { get; }

    /// <summary>The character level, from 1 to 40 (D-34, D-972).</summary>
    public int Level { get; private set; }

    /// <summary>The total experience, which gives the level (D-971).</summary>
    public int Experience { get; private set; }

    /// <summary>The stats of the level now, with no gear (D-966). The full health and the full MP come from it, because gear never changes them (D-1036).</summary>
    public StatRow Stats => this.Record.At(this.Level);

    /// <summary>Gives the stats of the level now with the worn gear (D-1036, D-1047).</summary>
    /// <param name="gear">The gear file.</param>
    /// <returns>The stats. The attack, the defense, and the speed hold the gear.</returns>
    public StatRow StatsWith(GearList gear) => GearRules.StatsOf(this.Stats, this.gear, gear);

    /// <summary>Gives the element table of the worn gear (D-1037).</summary>
    /// <param name="gear">The gear file.</param>
    /// <returns>The table.</returns>
    public ElementTable ElementsWith(GearList gear) => GearRules.ElementsOf(this.gear, gear);

    /// <summary>The health now, from zero to the full health of the level.</summary>
    public int Health { get; internal set; }

    /// <summary>The MP now, from zero to the full MP of the level (D-42). PR-12 gives the rites that spend it.</summary>
    public int Mp { get; internal set; }

    /// <summary>The row now, which the next battle starts from (D-558).</summary>
    public BattleRow Row { get; internal set; }

    /// <summary>Poison, blind, and silence, in the order of D-75, until a cure or a rest at a hub (D-390, D-792). PR-64 makes them act on the map.</summary>
    public IReadOnlyList<StatusKind> Statuses { get; internal set; }

    /// <summary>True while the character is down, which lasts until a hub or a rare item (D-36).</summary>
    public bool Down => this.Health == 0;

    /// <summary>
    /// Gives the points of the character for one lesson (D-361). A lesson that the character
    /// never carried starts at its first form for that character, so its points are zero.
    /// </summary>
    /// <param name="lesson">The id of the lesson.</param>
    /// <returns>The points, from zero.</returns>
    public int PointsOf(ContentId lesson)
    {
        ArgumentNullException.ThrowIfNull(lesson);

        return this.points.TryGetValue(lesson.Value, out LessonPoints? entry) ? entry.Points : 0;
    }

    /// <summary>Gives the slot that holds a lesson, or no value when no slot holds it.</summary>
    /// <param name="lesson">The id of the lesson.</param>
    /// <returns>The slot index, from zero.</returns>
    public int? SlotOf(ContentId lesson)
    {
        ArgumentNullException.ThrowIfNull(lesson);

        for (int index = 0; index < this.slots.Length; index += 1)
        {
            if (this.slots[index] is ContentId held && string.CompareOrdinal(held.Value, lesson.Value) == 0)
            {
                return index;
            }
        }

        return null;
    }

    /// <summary>Copies a list of slots into a new array. A spread of a list would make the compiler call `System.Linq`, which G-1 keeps out of Core.</summary>
    /// <param name="slots">The slots.</param>
    /// <returns>A new array, which a later swap never changes.</returns>
    internal static ContentId?[] CopyOf(IReadOnlyList<ContentId?> slots)
    {
        var copy = new ContentId?[slots.Count];
        for (int index = 0; index < copy.Length; index += 1)
        {
            copy[index] = slots[index];
        }

        return copy;
    }

    /// <summary>Gives the start values of the lessons of a character who joins: empty slots of the join level, and no points (D-1018).</summary>
    /// <param name="level">The level of the character.</param>
    /// <param name="rules">The rules, which hold the slot levels.</param>
    /// <returns>The values.</returns>
    internal static LessonValues EmptyLessons(int level, BattleRules rules) => new(new ContentId?[rules.SlotsAt(level)], []);

    /// <summary>
    /// Puts a lesson in a slot, and starts its points at zero when the character never carried
    /// it (D-361). The party state checks the swap place and the owned set first.
    /// </summary>
    /// <param name="slot">The slot index, which the party state checked.</param>
    /// <param name="lesson">The lesson, or no value to empty the slot.</param>
    internal void Put(int slot, ContentId? lesson)
    {
        this.slots[slot] = lesson;
        if (lesson is ContentId carried && !this.points.ContainsKey(carried.Value))
        {
            this.points.Add(carried.Value, new LessonPoints(carried, 0));
        }
    }

    /// <summary>Puts a piece in a gear slot, or empties the slot. The party state checks the slot kind and the pack first (D-44, D-1048).</summary>
    /// <param name="slot">The gear slot, which the party state checked.</param>
    /// <param name="piece">The piece, or no value to empty the slot.</param>
    internal void Wear(int slot, ContentId? piece) => this.gear[slot] = piece;

    /// <summary>Adds points to one carried lesson, to the total of its last form at most (D-357, D-1021).</summary>
    /// <param name="lesson">The lesson, which a slot holds.</param>
    /// <param name="earned">The points of the battle, from zero.</param>
    /// <returns>The points added.</returns>
    internal int GainPoints(LessonRecord lesson, int earned)
    {
        int before = this.PointsOf(lesson.Id);
        int after = (int)Math.Min((long)before + earned, lesson.MostPoints);
        this.points[lesson.Id.Value] = new LessonPoints(lesson.Id, after);
        return after - before;
    }

    /// <summary>Gives the start values of a character who joins: the join level, the total of that level, and full MP (D-363, D-971).</summary>
    /// <param name="record">The character.</param>
    /// <param name="rules">The rules, which hold the experience table.</param>
    /// <returns>The values.</returns>
    internal static GrowthValues JoinValues(CharacterRecord record, BattleRules rules) =>
        new(record.JoinLevel, rules.LevelExperience[record.JoinLevel - 1], record.At(record.JoinLevel).Mp);

    /// <summary>
    /// Adds experience, to the total of the highest level at most (D-972). A new level fills the
    /// health and the MP (D-973).
    /// </summary>
    /// <param name="earned">The experience of the battle, above zero.</param>
    /// <param name="rules">The rules, which hold the experience table.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <returns>The experience added, which is less than the earned experience at the top of the table.</returns>
    /// <exception cref="SimulationException">The character is down, which earns no experience (D-974, T-2).</exception>
    internal int Gain(int earned, BattleRules rules, RunContext context)
    {
        if (this.Down)
        {
            throw new SimulationException($"the down character '{this.Record.Id.Value}' gains {earned} experience, and a down earns none (D-974)", context);
        }

        int most = rules.LevelExperience[^1];
        int before = this.Experience;
        this.Experience = (int)Math.Min((long)this.Experience + earned, most);
        int level = Battles.Experience.LevelOf(this.Experience, rules);
        if (level > this.Level)
        {
            this.Level = level;
            this.Fill();

            // D-1018: a new slot opens empty, and each slot keeps its lesson.
            int count = rules.SlotsAt(level);
            if (count > this.slots.Length)
            {
                ContentId?[] grown = new ContentId?[count];
                Array.Copy(this.slots, grown, this.slots.Length);
                this.slots = grown;
            }
        }

        return this.Experience - before;
    }

    /// <summary>Fills the health and the MP to the full values of the level (D-967, D-973).</summary>
    internal void Fill()
    {
        this.Health = this.Stats.Health;
        this.Mp = this.Stats.Mp;
    }
}

/// <summary>
/// The characters of the party, their pack, and their gold, which last between battles (D-36,
/// D-765, D-775, D-1043). The snapshot holds them from save format 4, the statuses that last
/// from save format 5 (D-792), the level, the experience, and the MP from save format 7
/// (D-966), and the gear slots, the spare gear, and the gold from save format 11 (D-44, D-1038).
/// </summary>
public sealed class PartyState
{
    private readonly SortedDictionary<string, PackValues> pack;
    private readonly List<ContentId> lessonPack;
    private PartyMember[] members;

    private PartyState(PartyMember[] members, SortedDictionary<string, PackValues> pack, List<ContentId> lessonPack, bool atSwapPlace, int gold)
    {
        this.members = members;
        this.pack = pack;
        this.lessonPack = lessonPack;
        this.AtSwapPlace = atSwapPlace;
        this.Gold = gold;
    }

    /// <summary>The characters, in slot order (D-336).</summary>
    public IReadOnlyList<PartyMember> Members => this.members;

    /// <summary>The items and the spare gear of the pack, in the ordinal order of the ids. Each entry holds a count from 1 (D-775, D-1038).</summary>
    public IReadOnlyList<PackValues> Pack
    {
        get
        {
            List<PackValues> all = [];
            foreach (PackValues entry in this.pack.Values)
            {
                all.Add(entry);
            }

            return all;
        }
    }

    /// <summary>The gold of the party, from zero. A steal adds to it, and PR-65 shows it and spends it (D-1043).</summary>
    public int Gold { get; private set; }

    /// <summary>The owned lessons that no character carries, in the order that they entered the pack (D-1024).</summary>
    public IReadOnlyList<ContentId> LessonPack => this.lessonPack;

    /// <summary>
    /// True while the party stands at a swap place: a hub or a save point, where a swap of
    /// lessons is legal (D-356, D-1030). PR-14 and PR-16 mark the places, and a debug command
    /// marks one. A step of the lead leaves the place.
    /// </summary>
    public bool AtSwapPlace { get; private set; }

    /// <summary>
    /// Starts the party of a new run: the start party of the fixture at full health, the start
    /// pack, the start gear, the start lessons, the lesson pack, and no gold (D-336, D-765,
    /// D-1030, D-1038, D-1043).
    /// </summary>
    /// <param name="content">The battle content of the run.</param>
    /// <returns>The party.</returns>
    /// <exception cref="ArgumentNullException">The content is null (T-2).</exception>
    public static PartyState Start(BattleContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        List<PartyMember> members = [];
        foreach (ContentId id in content.Fixture.StartParty)
        {
            CharacterRecord record = content.Character(id);
            LessonValues lessons = StartLessonsOf(record, record.JoinLevel, content);
            members.Add(new PartyMember(record, PartyMember.JoinValues(record, content.Rules), record.At(record.JoinLevel).Health, record.Row, [], lessons, StartGearOf(record, content)));
        }

        var pack = new SortedDictionary<string, PackValues>(StringComparer.Ordinal);
        foreach (PackEntry entry in content.Fixture.Pack)
        {
            pack.Add(entry.Id.Value, new PackValues(entry.Id, entry.Count));
        }

        return new PartyState([.. members], pack, new List<ContentId>(content.Fixture.LessonPack), false, 0);
    }

    /// <summary>Puts the party back from the values of a snapshot (D-166, D-765).</summary>
    /// <param name="content">The battle content of this build.</param>
    /// <param name="characters">The stored characters, in slot order.</param>
    /// <param name="pack">The stored pack.</param>
    /// <param name="source">What the values came from, such as `the save`, for an error (T-2).</param>
    /// <returns>The party.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no party of this content (T-2).</exception>
    /// <remarks>
    /// A snapshot of save format 9 or older holds no lesson (D-166). Each character then gets
    /// its start lessons of the fixture, and the lesson pack gets the lesson pack of the fixture
    /// and the start lessons of each start character outside the party. Thus the player owns
    /// each start lesson one time (D-1023).
    /// </remarks>
    /// <param name="lessonPack">The stored lesson pack, or no value for a snapshot of save format 9 or older.</param>
    /// <param name="atSwapPlace">True when the party stood at a swap place (D-1030).</param>
    /// <param name="gold">The stored gold, or no value for a snapshot of save format 10 or older, which starts at zero (D-1043).</param>
    public static PartyState Resume(
        BattleContent content,
        IReadOnlyList<CharacterValues> characters,
        IReadOnlyList<PackValues> pack,
        IReadOnlyList<ContentId>? lessonPack,
        bool atSwapPlace,
        int? gold,
        string source)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(characters);
        ArgumentNullException.ThrowIfNull(pack);
        ArgumentException.ThrowIfNullOrEmpty(source);

        Refuse(
            characters.Count == 0 || characters.Count > BattleFixture.MostCharacters,
            source,
            $"it holds {characters.Count} characters, and a party holds 1 to {BattleFixture.MostCharacters} (D-31)");

        List<PartyMember> members = [];
        foreach (CharacterValues stored in characters)
        {
            ArgumentNullException.ThrowIfNull(stored);
            CharacterRecord record = content.Character(stored.Character);
            GrowthValues growth = stored.Growth ?? PartyMember.JoinValues(record, content.Rules);
            CheckGrowth(record, growth, content.Rules, source);
            StatRow full = record.At(growth.Level);
            Refuse(
                stored.Health < 0 || stored.Health > full.Health,
                source,
                $"the character '{record.Id.Value}' holds the health {stored.Health}, and the range at level {growth.Level} is 0 to {full.Health}");
            foreach (PartyMember earlier in members)
            {
                Refuse(
                    string.CompareOrdinal(earlier.Record.Id.Value, record.Id.Value) == 0,
                    source,
                    $"it holds the character '{record.Id.Value}' two times");
            }

            CheckStatuses(stored, source);
            Refuse(
                (stored.Lessons is null) != (lessonPack is null),
                source,
                $"the character '{record.Id.Value}' and the lesson pack differ on the save format: one holds lessons and one does not (D-166)");
            LessonValues lessons = stored.Lessons ?? StartLessonsOf(record, growth.Level, content);
            CheckLessons(record, growth.Level, lessons, content, source);
            Refuse(
                (stored.Gear is null) != (gold is null),
                source,
                $"the character '{record.Id.Value}' and the gold differ on the save format: one holds gear and one does not (D-166)");
            IReadOnlyList<ContentId?> worn = stored.Gear ?? StartGearOf(record, content);
            CheckGear(record, worn, content, source);
            members.Add(new PartyMember(record, growth, stored.Health, stored.Row, stored.Statuses, lessons, worn));
        }

        // A snapshot of save format 10 or older can hold an item at zero, and the pack now
        // keeps no empty entry (D-166, D-1038).
        var items = new SortedDictionary<string, PackValues>(StringComparer.Ordinal);
        foreach (PackValues stored in pack)
        {
            ArgumentNullException.ThrowIfNull(stored);
            _ = content.LimitOf(stored.Id);
            Refuse(stored.Count < 0, source, $"the pack holds {stored.Count} of '{stored.Id.Value}', which is below zero");
            Refuse(items.ContainsKey(stored.Id.Value), source, $"the pack holds '{stored.Id.Value}' in two entries");
            if (stored.Count > 0)
            {
                items.Add(stored.Id.Value, stored);
            }
        }

        Refuse(gold < 0, source, $"it holds the gold {gold}, which is below zero (D-1043)");

        List<ContentId> owned = lessonPack is null ? OlderLessonPack(members, content) : new List<ContentId>(lessonPack);
        foreach (ContentId lesson in owned)
        {
            ArgumentNullException.ThrowIfNull(lesson);
            Refuse(!content.Lessons.Holds(lesson), source, $"the lesson pack holds '{lesson.Value}', which the lesson file lacks (D-1026)");
        }

        var party = new PartyState([.. members], items, owned, atSwapPlace, gold ?? 0);
        party.CheckOneCopy(source);
        party.CheckStackLimits(content, source);
        return party;
    }

    /// <summary>Tells whether the player owns a lesson: in the lesson pack, or in a slot of a character of the party (D-1023, D-1024).</summary>
    /// <param name="lesson">The id of the lesson.</param>
    /// <returns>True when the player owns the lesson.</returns>
    /// <remarks>No rule of this build moves a character to the reserve of D-58, so the party holds each character (D-563).</remarks>
    public bool Owns(ContentId lesson)
    {
        ArgumentNullException.ThrowIfNull(lesson);

        foreach (ContentId packed in this.lessonPack)
        {
            if (string.CompareOrdinal(packed.Value, lesson.Value) == 0)
            {
                return true;
            }
        }

        foreach (PartyMember member in this.members)
        {
            if (member.SlotOf(lesson) is not null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Adds a lesson to the lesson pack. The player never owns two copies of one lesson, so a
    /// lesson that the player owns is an error (D-1023, D-1024). PR-16 and PR-65 call it for a
    /// chest and a shop, and each checks <see cref="Owns"/> first.
    /// </summary>
    /// <param name="lesson">The lesson.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <exception cref="SimulationException">The player owns the lesson (D-1023).</exception>
    public void AddLesson(LessonRecord lesson, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(lesson);
        ArgumentNullException.ThrowIfNull(context);

        if (this.Owns(lesson.Id))
        {
            throw new SimulationException($"a second copy of the lesson '{lesson.Id.Value}', and the player never owns two copies of one lesson (D-1023, D-1024)", context);
        }

        this.lessonPack.Add(lesson.Id);
    }

    /// <summary>
    /// Gives the reason that the rules refuse a swap of lessons now, or no value when the swap
    /// is legal (D-356, D-1030). A swap puts a lesson of the lesson pack in a slot, or empties a
    /// slot. The lesson window reads it to show each legal swap (T-2).
    /// </summary>
    /// <param name="character">The slot of the character in the party.</param>
    /// <param name="slot">The lesson slot of the character.</param>
    /// <param name="lesson">The lesson of the lesson pack, or no value to empty the slot.</param>
    /// <returns>The reason, such as `no swap place`, or no value.</returns>
    public string? RefusalOfSwap(int character, int slot, ContentId? lesson)
    {
        if (!this.AtSwapPlace)
        {
            return "a swap of lessons outside a swap place, and a swap needs a hub or a save point (D-356, D-1030)";
        }

        if (character < 0 || character >= this.members.Length)
        {
            return $"the character slot {character}, and the party holds the slots 0 to {this.members.Length - 1}";
        }

        PartyMember member = this.members[character];
        if (slot < 0 || slot >= member.Slots.Count)
        {
            return $"the lesson slot {slot} of '{member.Record.Id.Value}', who holds the slots 0 to {member.Slots.Count - 1} at level {member.Level} (D-1018)";
        }

        if (lesson is ContentId put && this.IndexInPack(put) < 0)
        {
            return $"the lesson '{put.Value}', which the lesson pack does not hold (D-1024)";
        }

        if (lesson is null && member.Slots[slot] is null)
        {
            return $"an empty of the lesson slot {slot} of '{member.Record.Id.Value}', which holds no lesson";
        }

        return null;
    }

    /// <summary>
    /// Swaps the lesson of one slot: the lesson of the lesson pack goes in, and the lesson of
    /// the slot goes to the end of the lesson pack (D-356, D-1030). The points of each lesson
    /// stay with the character (D-361).
    /// </summary>
    /// <param name="character">The slot of the character in the party.</param>
    /// <param name="slot">The lesson slot of the character.</param>
    /// <param name="lesson">The lesson of the lesson pack, or no value to empty the slot.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <exception cref="SimulationException">The rules refuse the swap, and the error names the reason (T-2).</exception>
    public void Swap(int character, int slot, ContentId? lesson, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (this.RefusalOfSwap(character, slot, lesson) is string refusal)
        {
            throw new SimulationException($"a swap of lessons, and the rules refuse it: {refusal}", context);
        }

        PartyMember member = this.members[character];
        ContentId? held = member.Slots[slot];
        if (lesson is ContentId put)
        {
            this.lessonPack.RemoveAt(this.IndexInPack(put));
        }

        member.Put(slot, lesson);
        if (held is ContentId removed)
        {
            this.lessonPack.Add(removed);
        }
    }

    /// <summary>Gives the index of a lesson in the lesson pack, or -1. `ContentId` compares by reference, so the walk compares the text (F-39).</summary>
    private int IndexInPack(ContentId lesson)
    {
        for (int index = 0; index < this.lessonPack.Count; index += 1)
        {
            if (string.CompareOrdinal(this.lessonPack[index].Value, lesson.Value) == 0)
            {
                return index;
            }
        }

        return -1;
    }

    /// <summary>Marks the place of the party as a swap place (D-1030). PR-14 calls it at a hub, PR-16 at a save point, and a debug command anywhere.</summary>
    public void MarkSwapPlace() => this.AtSwapPlace = true;

    /// <summary>Leaves the swap place. The world rules call it at each step of the lead (D-1030).</summary>
    public void LeaveSwapPlace() => this.AtSwapPlace = false;

    /// <summary>Gives a copy of the lesson pack for a snapshot (D-1024).</summary>
    /// <returns>A new list, which a later swap never changes.</returns>
    public IReadOnlyList<ContentId> LessonPackValues() => new List<ContentId>(this.lessonPack);

    /// <summary>Gives the count of one item or one piece of gear in the pack (D-775).</summary>
    /// <param name="id">The id of the item or the piece.</param>
    /// <returns>The count, which is zero when the pack holds no entry for the id.</returns>
    public int CountOf(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return this.pack.TryGetValue(id.Value, out PackValues? entry) ? entry.Count : 0;
    }

    /// <summary>Gives the count of the copies that the party owns: in the pack, and in the gear slots of each character (D-1039).</summary>
    /// <param name="id">The id of the item or the piece.</param>
    /// <returns>The count, from zero. The stack limit holds it.</returns>
    public int OwnedCount(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        int owned = this.CountOf(id);
        foreach (PartyMember member in this.members)
        {
            foreach (ContentId? piece in member.Gear)
            {
                if (piece is not null && string.CompareOrdinal(piece.Value, id.Value) == 0)
                {
                    owned += 1;
                }
            }
        }

        return owned;
    }

    /// <summary>
    /// Puts copies of an item or a piece of gear in the pack, up to the stack limit, and gives
    /// the count that did not fit (D-385, D-1038, D-1039). The copies that did not fit stay
    /// where they were: in a chest, or with an enemy. PR-16 calls it for a chest.
    /// </summary>
    /// <param name="id">The id of the item or the piece.</param>
    /// <param name="count">The count of the find, from 1.</param>
    /// <param name="content">The battle content, which holds the stack limit.</param>
    /// <returns>The remainder, from zero to the count.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The count is below 1 (T-2).</exception>
    /// <exception cref="ContentException">The item file or the gear file holds no such id (T-2).</exception>
    public int Pick(ContentId id, int count, BattleContent content)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

        int room = Math.Max(0, content.LimitOf(id) - this.OwnedCount(id));
        int taken = Math.Min(room, count);
        if (taken > 0)
        {
            this.Put(id, taken);
        }

        return count - taken;
    }

    /// <summary>Adds gold to the party, from a steal (D-1043).</summary>
    /// <param name="gold">The gold, above zero.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <exception cref="SimulationException">The gold is below 1, or the sum passes an `int` (T-2).</exception>
    public void AddGold(int gold, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (gold < 1 || gold > int.MaxValue - this.Gold)
        {
            throw new SimulationException($"an add of {gold} gold to {this.Gold}, which is below 1 or passes the largest gold (T-2)", context);
        }

        this.Gold += gold;
    }

    /// <summary>
    /// Gives the reason that the rules refuse a change of gear now, or no value when the change
    /// is legal (D-44, D-1048). A change puts a piece of the pack in a gear slot of its kind, or
    /// empties a slot. The piece of the slot goes back to the pack. The gear window reads it.
    /// </summary>
    /// <param name="character">The slot of the character in the party.</param>
    /// <param name="slot">The gear slot, from 0 to 5.</param>
    /// <param name="piece">The piece of the pack, or no value to empty the slot.</param>
    /// <param name="content">The battle content, which holds the gear file.</param>
    /// <returns>The reason, or no value.</returns>
    public string? RefusalOfWear(int character, int slot, ContentId? piece, BattleContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (character < 0 || character >= this.members.Length)
        {
            return $"the character slot {character}, and the party holds the slots 0 to {this.members.Length - 1}";
        }

        if (slot < 0 || slot >= GearRules.SlotCount)
        {
            return $"the gear slot {slot}, and a character holds the gear slots 0 to {GearRules.SlotCount - 1} (D-44)";
        }

        PartyMember member = this.members[character];
        if (piece is null)
        {
            return member.Gear[slot] is null ? $"an empty of the gear slot {slot} of '{member.Record.Id.Value}', which holds no piece" : null;
        }

        if (string.CompareOrdinal(piece.Kind, GearList.Kind) != 0 || !content.Gear.Holds(piece))
        {
            return $"the id '{piece.Value}', which the gear file does not hold (D-1036)";
        }

        if (this.CountOf(piece) == 0)
        {
            return $"the piece '{piece.Value}', which the pack does not hold (D-1038)";
        }

        GearSlotKind kind = content.Piece(piece).Slot;
        if (kind != GearRules.KindOf(slot))
        {
            return $"the piece '{piece.Value}' of the kind '{GearList.NameOf(kind)}' in the gear slot {slot} of the kind '{GearList.NameOf(GearRules.KindOf(slot))}' (D-44)";
        }

        return null;
    }

    /// <summary>
    /// Changes the gear of one slot: the piece of the pack goes in, and the piece of the slot
    /// goes to the pack (D-44, D-1048). The owned count never changes, so no stack limit
    /// applies (D-1039).
    /// </summary>
    /// <param name="character">The slot of the character in the party.</param>
    /// <param name="slot">The gear slot, from 0 to 5.</param>
    /// <param name="piece">The piece of the pack, or no value to empty the slot.</param>
    /// <param name="content">The battle content, which holds the gear file.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <exception cref="SimulationException">The rules refuse the change, and the error names the reason (T-2).</exception>
    public void Wear(int character, int slot, ContentId? piece, BattleContent content, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(context);

        if (this.RefusalOfWear(character, slot, piece, content) is string refusal)
        {
            throw new SimulationException($"a change of gear, and the rules refuse it: {refusal}", context);
        }

        PartyMember member = this.members[character];
        ContentId? held = member.Gear[slot];
        if (piece is ContentId put)
        {
            this.Take(put, context);
        }

        member.Wear(slot, piece);
        if (held is ContentId removed)
        {
            this.Put(removed, 1);
        }
    }

    /// <summary>Gives the stored values of every character, in slot order (D-765).</summary>
    /// <returns>The values.</returns>
    public IReadOnlyList<CharacterValues> CharacterValues()
    {
        List<CharacterValues> values = [];
        foreach (PartyMember member in this.members)
        {
            values.Add(new CharacterValues(
                member.Record.Id,
                member.Health,
                member.Row,
                member.Statuses,
                new GrowthValues(member.Level, member.Experience, member.Mp),
                new LessonValues(PartyMember.CopyOf(member.Slots), member.Points),
                PartyMember.CopyOf(member.Gear)));
        }

        return values;
    }

    /// <summary>Gives a copy of the pack for a snapshot (D-775).</summary>
    /// <returns>A new list, which a later use of an item never changes.</returns>
    /// <remarks>
    /// A snapshot must hold the values of its tick. A list that shares the array of the pack
    /// would change with each later use, and a record that starts from it replays another run
    /// (G-5, T-2).
    /// </remarks>
    public IReadOnlyList<PackValues> PackValues() => this.Pack;

    /// <summary>Adds every value of the party to the state hash, in slot order (G-5).</summary>
    /// <param name="hasher">The hasher of the state.</param>
    public void Hash(StateHasher hasher)
    {
        ArgumentNullException.ThrowIfNull(hasher);

        hasher.AddInt32(this.members.Length);
        foreach (PartyMember member in this.members)
        {
            hasher.AddText(member.Record.Id.Value);
            hasher.AddInt32(member.Level);
            hasher.AddInt32(member.Experience);
            hasher.AddInt32(member.Health);
            hasher.AddInt32(member.Mp);
            hasher.AddInt32((int)member.Row);
            hasher.AddInt32(member.Statuses.Count);
            foreach (StatusKind status in member.Statuses)
            {
                hasher.AddInt32((int)status);
            }

            hasher.AddInt32(member.Slots.Count);
            foreach (ContentId? lesson in member.Slots)
            {
                hasher.AddText(lesson?.Value ?? string.Empty);
            }

            IReadOnlyList<LessonPoints> points = member.Points;
            hasher.AddInt32(points.Count);
            foreach (LessonPoints entry in points)
            {
                hasher.AddText(entry.Lesson.Value);
                hasher.AddInt32(entry.Points);
            }

            foreach (ContentId? piece in member.Gear)
            {
                hasher.AddText(piece?.Value ?? string.Empty);
            }
        }

        hasher.AddInt32(this.pack.Count);
        foreach (PackValues entry in this.pack.Values)
        {
            hasher.AddText(entry.Id.Value);
            hasher.AddInt32(entry.Count);
        }

        hasher.AddInt32(this.Gold);

        hasher.AddInt32(this.lessonPack.Count);
        foreach (ContentId lesson in this.lessonPack)
        {
            hasher.AddText(lesson.Value);
        }

        hasher.AddInt32(this.AtSwapPlace ? 1 : 0);
    }

    /// <summary>
    /// The restore of a save point: each character gets full MP, and no health (D-389, D-967).
    /// PR-16 calls it once for each place, until a story event reopens the place (D-555, D-970).
    /// </summary>
    public void RestoreAtSavePoint()
    {
        foreach (PartyMember member in this.members)
        {
            member.Mp = member.Stats.Mp;
        }
    }

    /// <summary>
    /// The rest at a hub: each character gets full health and full MP, a down character stands
    /// again, and poison, blind, and silence end (D-36, D-390, D-967). PR-14 calls it (D-970).
    /// </summary>
    public void RestAtHub()
    {
        foreach (PartyMember member in this.members)
        {
            member.Fill();
            member.Statuses = [];
        }
    }

    /// <summary>
    /// Adds a cast member to the last slot of the party, at its join level with full health
    /// and full MP, in the row of its record (D-363, D-563).
    /// </summary>
    /// <param name="record">The cast member.</param>
    /// <param name="rules">The rules, which hold the experience table.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <exception cref="SimulationException">The cast member is in the party, or the party is full (T-2).</exception>
    /// <remarks>
    /// A party holds three characters at most (D-31), and no rule of this build moves a character
    /// to the reserve of D-58. The story adds each character in its order, so a join into a full
    /// party points at a fault in the content (D-342).
    /// </remarks>
    internal void Join(CharacterRecord record, BattleRules rules, RunContext context)
    {
        foreach (PartyMember member in this.members)
        {
            if (string.CompareOrdinal(member.Record.Id.Value, record.Id.Value) == 0)
            {
                throw new SimulationException($"a join of '{record.Id.Value}', who is already in the party (D-563)", context);
            }
        }

        if (this.members.Length >= BattleFixture.MostCharacters)
        {
            throw new SimulationException(
                $"a join of '{record.Id.Value}', and the party already holds {this.members.Length} characters, the most that it holds (D-31, D-563)",
                context);
        }

        GrowthValues growth = PartyMember.JoinValues(record, rules);
        var joined = new PartyMember(record, growth, record.At(growth.Level).Health, record.Row, [], PartyMember.EmptyLessons(growth.Level, rules), new ContentId?[GearRules.SlotCount]);
        this.members = [.. this.members, joined];
    }

    /// <summary>Takes one copy of an item or a piece from the pack. An entry at zero leaves the pack (D-775).</summary>
    /// <param name="id">The id of the item or the piece.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <exception cref="SimulationException">The pack holds none of the id (T-2).</exception>
    internal void Take(ContentId id, RunContext context)
    {
        if (!this.pack.TryGetValue(id.Value, out PackValues? entry))
        {
            throw new SimulationException($"a take of '{id.Value}' from the pack, and the pack holds none (D-775)", context);
        }

        if (entry.Count == 1)
        {
            _ = this.pack.Remove(id.Value);
            return;
        }

        this.pack[id.Value] = entry with { Count = entry.Count - 1 };
    }

    /// <summary>Puts copies in the pack. The caller checked the stack limit, or keeps the owned count (D-1039).</summary>
    private void Put(ContentId id, int count)
    {
        int before = this.CountOf(id);
        this.pack[id.Value] = new PackValues(id, checked(before + count));
    }

    /// <summary>Gives the start gear of a character: the pieces of the fixture, each in the first empty slot of its kind (D-44).</summary>
    private static ContentId?[] StartGearOf(CharacterRecord record, BattleContent content)
    {
        foreach (StartGear entry in content.Fixture.StartGear)
        {
            if (string.CompareOrdinal(entry.Character.Value, record.Id.Value) == 0)
            {
                return GearRules.SlotsOf(entry.Gear, content.Gear, BattleFixture.Path, record.Id.Value);
            }
        }

        return new ContentId?[GearRules.SlotCount];
    }

    /// <summary>Refuses stored gear that no run can make: another count than six slots, an absent piece, or a piece in a slot of another kind (D-44).</summary>
    private static void CheckGear(CharacterRecord record, IReadOnlyList<ContentId?> worn, BattleContent content, string source)
    {
        string who = record.Id.Value;
        Refuse(worn.Count != GearRules.SlotCount, source, $"the character '{who}' holds {worn.Count} gear slots, and a character holds {GearRules.SlotCount} (D-44)");
        for (int slot = 0; slot < worn.Count; slot += 1)
        {
            if (worn[slot] is not ContentId piece)
            {
                continue;
            }

            Refuse(
                string.CompareOrdinal(piece.Kind, GearList.Kind) != 0 || !content.Gear.Holds(piece),
                source,
                $"the character '{who}' wears '{piece.Value}', which the gear file lacks (D-1036)");
            GearSlotKind kind = content.Piece(piece).Slot;
            Refuse(
                kind != GearRules.KindOf(slot),
                source,
                $"the character '{who}' wears '{piece.Value}' of the kind '{GearList.NameOf(kind)}' in the gear slot {slot} (D-44)");
        }
    }

    /// <summary>Refuses a party that owns more copies of an item or a piece than its stack limit, the worn copies included (D-1038, D-1039).</summary>
    private void CheckStackLimits(BattleContent content, string source)
    {
        var ids = new SortedDictionary<string, ContentId>(StringComparer.Ordinal);
        foreach (PackValues entry in this.pack.Values)
        {
            ids[entry.Id.Value] = entry.Id;
        }

        foreach (PartyMember member in this.members)
        {
            foreach (ContentId? piece in member.Gear)
            {
                if (piece is not null)
                {
                    ids[piece.Value] = piece;
                }
            }
        }

        foreach (ContentId id in ids.Values)
        {
            int owned = this.OwnedCount(id);
            int limit = content.LimitOf(id);
            Refuse(owned > limit, source, $"the party owns {owned} copies of '{id.Value}', and the stack limit is {limit} (D-1038, D-1039)");
        }
    }

    /// <summary>
    /// Gives the start lessons of a character: the slots of the level, with the start lessons of
    /// the fixture from the first slot, each at zero points (D-361, D-1030).
    /// </summary>
    private static LessonValues StartLessonsOf(CharacterRecord record, int level, BattleContent content)
    {
        var slots = new ContentId?[content.Rules.SlotsAt(level)];
        var points = new SortedDictionary<string, LessonPoints>(StringComparer.Ordinal);
        foreach (StartLessons entry in content.Fixture.StartLessons)
        {
            if (string.CompareOrdinal(entry.Character.Value, record.Id.Value) != 0)
            {
                continue;
            }

            // The battle content checked that the start lessons fit the slots of the join
            // level, and a later level holds at least as many slots (D-1018).
            for (int index = 0; index < entry.Lessons.Count; index += 1)
            {
                slots[index] = entry.Lessons[index];
                points.Add(entry.Lessons[index].Value, new LessonPoints(entry.Lessons[index], 0));
            }
        }

        return new LessonValues(slots, new List<LessonPoints>(points.Values));
    }

    /// <summary>
    /// Gives the lesson pack of a snapshot of save format 9 or older: the lesson pack of the
    /// fixture, then the start lessons of each start character outside the party (D-166, D-1023).
    /// </summary>
    private static List<ContentId> OlderLessonPack(List<PartyMember> members, BattleContent content)
    {
        List<ContentId> owned = new(content.Fixture.LessonPack);
        foreach (StartLessons entry in content.Fixture.StartLessons)
        {
            bool inParty = false;
            foreach (PartyMember member in members)
            {
                inParty |= string.CompareOrdinal(member.Record.Id.Value, entry.Character.Value) == 0;
            }

            if (!inParty)
            {
                owned.AddRange(entry.Lessons);
            }
        }

        return owned;
    }

    /// <summary>
    /// Refuses stored lessons that no run can make: another slot count than the level gives, a
    /// lesson that the lesson file lacks, a carried lesson with no points, or points out of order,
    /// repeated, or outside zero to the last form (D-361, D-1018, D-1021).
    /// </summary>
    private static void CheckLessons(CharacterRecord record, int level, LessonValues lessons, BattleContent content, string source)
    {
        ArgumentNullException.ThrowIfNull(lessons.Slots);
        ArgumentNullException.ThrowIfNull(lessons.Points);

        string who = record.Id.Value;
        int count = content.Rules.SlotsAt(level);
        Refuse(lessons.Slots.Count != count, source, $"the character '{who}' holds {lessons.Slots.Count} lesson slots, and level {level} gives {count} (D-1018)");
        string? last = null;
        var carried = new SortedSet<string>(StringComparer.Ordinal);
        foreach (LessonPoints entry in lessons.Points)
        {
            ArgumentNullException.ThrowIfNull(entry);
            Refuse(!content.Lessons.Holds(entry.Lesson), source, $"the character '{who}' holds points of '{entry.Lesson.Value}', which the lesson file lacks (D-1026)");
            LessonRecord lesson = content.Lessons.Lesson(entry.Lesson);
            Refuse(
                entry.Points < 0 || entry.Points > lesson.MostPoints,
                source,
                $"the character '{who}' holds {entry.Points} points of '{entry.Lesson.Value}', and the range is 0 to {lesson.MostPoints} (D-1021)");
            Refuse(last is not null && string.CompareOrdinal(entry.Lesson.Value, last) <= 0, source, $"the points of '{who}' repeat or leave the ordinal order of the lesson ids");
            last = entry.Lesson.Value;
            _ = carried.Add(entry.Lesson.Value);
        }

        foreach (ContentId? lesson in lessons.Slots)
        {
            Refuse(
                lesson is not null && !carried.Contains(lesson.Value),
                source,
                $"the character '{who}' carries '{lesson?.Value}' with no points, and a carried lesson holds its points (D-361)");
        }
    }

    /// <summary>Refuses one lesson two times across the lesson pack and the slots, because the player never owns two copies (D-1023).</summary>
    private void CheckOneCopy(string source)
    {
        var owned = new SortedSet<string>(StringComparer.Ordinal);
        foreach (ContentId lesson in this.lessonPack)
        {
            Refuse(!owned.Add(lesson.Value), source, $"the player owns '{lesson.Value}' two times, and the player never owns two copies of one lesson (D-1023)");
        }

        foreach (PartyMember member in this.members)
        {
            foreach (ContentId? lesson in member.Slots)
            {
                Refuse(
                    lesson is not null && !owned.Add(lesson.Value),
                    source,
                    $"the player owns '{lesson?.Value}' two times, and the player never owns two copies of one lesson (D-1023)");
            }
        }
    }

    /// <summary>Refuses a stored status list that no run can make: a status that ends with its fight, a status out of the order of D-75, or a status on a down character (D-390, D-801).</summary>
    private static void CheckStatuses(CharacterValues stored, string source)
    {
        ArgumentNullException.ThrowIfNull(stored.Statuses);

        string who = stored.Character.Value;
        Refuse(stored.Health == 0 && stored.Statuses.Count > 0, source, $"the down character '{who}' holds a status (D-801)");
        int last = -1;
        foreach (StatusKind status in stored.Statuses)
        {
            Refuse(!Battles.Statuses.Lasts(status), source, $"the character '{who}' holds '{Battles.Statuses.NameOf(status)}' outside a fight, and it ends with its fight (D-390)");
            Refuse((int)status <= last, source, $"the statuses of '{who}' repeat or leave the order of D-75");
            last = (int)status;
        }
    }

    /// <summary>Refuses a stored level that no run can make: a level outside the curve, an experience outside the table or of another level, or MP outside the range of the level (D-363, D-971, D-972).</summary>
    private static void CheckGrowth(CharacterRecord record, GrowthValues growth, BattleRules rules, string source)
    {
        string who = record.Id.Value;
        int most = rules.LevelExperience[^1];
        Refuse(
            growth.Level < 1 || growth.Level > StatCurve.HighestLevel,
            source,
            $"the character '{who}' holds level {growth.Level}, and the range is 1 to {StatCurve.HighestLevel} (D-972)");
        Refuse(
            growth.Experience < 0 || growth.Experience > most,
            source,
            $"the character '{who}' holds the experience {growth.Experience}, and the range is 0 to {most} (D-972)");
        int level = Experience.LevelOf(growth.Experience, rules);
        Refuse(
            level != growth.Level,
            source,
            $"the character '{who}' holds level {growth.Level} with the experience {growth.Experience}, which gives level {level} (D-971)");
        int fullMp = record.At(growth.Level).Mp;
        Refuse(
            growth.Mp < 0 || growth.Mp > fullMp,
            source,
            $"the character '{who}' holds the MP {growth.Mp}, and the range at level {growth.Level} is 0 to {fullMp}");
    }

    private static void Refuse(bool broken, string source, string reason)
    {
        if (broken)
        {
            throw new ArgumentException($"The party of {source} is not a state of a run: {reason}.");
        }
    }
}
