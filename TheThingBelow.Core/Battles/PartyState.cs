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
public sealed record CharacterValues(ContentId Character, int Health, BattleRow Row, IReadOnlyList<StatusKind> Statuses, GrowthValues? Growth);

/// <summary>The stored level, experience, and MP of one character (D-34, D-42, D-966).</summary>
/// <param name="Level">The character level, from 1 to 40 (D-972).</param>
/// <param name="Experience">The total experience, which gives the level through the table of the rules (D-971).</param>
/// <param name="Mp">The MP now, from zero to the full MP of the level (D-42).</param>
public sealed record GrowthValues(int Level, int Experience, int Mp);

/// <summary>The stored count of one item of the pack (D-775).</summary>
/// <param name="Item">The id of the item.</param>
/// <param name="Count">The count now.</param>
public sealed record PackValues(ContentId Item, int Count);

/// <summary>One character of the party, with the level, the health, the MP, and the row that last between battles (D-34, D-36, D-42, D-765).</summary>
public sealed class PartyMember
{
    internal PartyMember(CharacterRecord record, GrowthValues growth, int health, BattleRow row, IReadOnlyList<StatusKind> statuses)
    {
        this.Record = record;
        this.Level = growth.Level;
        this.Experience = growth.Experience;
        this.Mp = growth.Mp;
        this.Health = health;
        this.Row = row;
        this.Statuses = statuses;
    }

    /// <summary>The join level and the stat curve of the character (D-363, D-966).</summary>
    public CharacterRecord Record { get; }

    /// <summary>The character level, from 1 to 40 (D-34, D-972).</summary>
    public int Level { get; private set; }

    /// <summary>The total experience, which gives the level (D-971).</summary>
    public int Experience { get; private set; }

    /// <summary>The stats of the level now (D-966).</summary>
    public StatRow Stats => this.Record.At(this.Level);

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
/// The characters of the party and their pack, which last between battles (D-36, D-765,
/// D-775). The snapshot holds them from save format 4, the statuses that last from save format 5 (D-792), and the level, the experience, and the MP from save format 7 (D-966).
/// </summary>
public sealed class PartyState
{
    private readonly PackValues[] pack;
    private PartyMember[] members;

    private PartyState(PartyMember[] members, PackValues[] pack)
    {
        this.members = members;
        this.pack = pack;
    }

    /// <summary>The characters, in slot order (D-336).</summary>
    public IReadOnlyList<PartyMember> Members => this.members;

    /// <summary>The pack, in the order of the fixture file (D-775).</summary>
    public IReadOnlyList<PackValues> Pack => this.pack;

    /// <summary>Starts the party of a new run: the start party of the fixture at full health, and the start pack (D-336, D-765).</summary>
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
            members.Add(new PartyMember(record, PartyMember.JoinValues(record, content.Rules), record.At(record.JoinLevel).Health, record.Row, []));
        }

        List<PackValues> pack = [];
        foreach (PackEntry entry in content.Fixture.Pack)
        {
            pack.Add(new PackValues(entry.Item, entry.Count));
        }

        return new PartyState([.. members], [.. pack]);
    }

    /// <summary>Puts the party back from the values of a snapshot (D-166, D-765).</summary>
    /// <param name="content">The battle content of this build.</param>
    /// <param name="characters">The stored characters, in slot order.</param>
    /// <param name="pack">The stored pack.</param>
    /// <param name="source">What the values came from, such as `the save`, for an error (T-2).</param>
    /// <returns>The party.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no party of this content (T-2).</exception>
    public static PartyState Resume(
        BattleContent content,
        IReadOnlyList<CharacterValues> characters,
        IReadOnlyList<PackValues> pack,
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
            members.Add(new PartyMember(record, growth, stored.Health, stored.Row, stored.Statuses));
        }

        List<PackValues> items = [];
        foreach (PackValues stored in pack)
        {
            ArgumentNullException.ThrowIfNull(stored);
            _ = content.Item(stored.Item);
            Refuse(stored.Count < 0, source, $"the pack holds {stored.Count} of '{stored.Item.Value}', which is below zero");
            items.Add(stored);
        }

        return new PartyState([.. members], [.. items]);
    }

    /// <summary>Gives the count of one item in the pack (D-775).</summary>
    /// <param name="item">The id of the item.</param>
    /// <returns>The count, which is zero when the pack holds no entry for the item.</returns>
    public int CountOf(ContentId item)
    {
        ArgumentNullException.ThrowIfNull(item);

        foreach (PackValues entry in this.pack)
        {
            if (string.CompareOrdinal(entry.Item.Value, item.Value) == 0)
            {
                return entry.Count;
            }
        }

        return 0;
    }

    /// <summary>Gives the stored values of every character, in slot order (D-765).</summary>
    /// <returns>The values.</returns>
    public IReadOnlyList<CharacterValues> CharacterValues()
    {
        List<CharacterValues> values = [];
        foreach (PartyMember member in this.members)
        {
            values.Add(new CharacterValues(member.Record.Id, member.Health, member.Row, member.Statuses, new GrowthValues(member.Level, member.Experience, member.Mp)));
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
    public IReadOnlyList<PackValues> PackValues()
    {
        List<PackValues> values = [];
        foreach (PackValues entry in this.pack)
        {
            values.Add(entry);
        }

        return values;
    }

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
        }

        hasher.AddInt32(this.pack.Length);
        foreach (PackValues entry in this.pack)
        {
            hasher.AddText(entry.Item.Value);
            hasher.AddInt32(entry.Count);
        }
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
        var joined = new PartyMember(record, growth, record.At(growth.Level).Health, record.Row, []);
        this.members = [.. this.members, joined];
    }

    /// <summary>Takes one item from the pack (D-775).</summary>
    /// <param name="item">The id of the item.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <exception cref="SimulationException">The pack holds none of the item (T-2).</exception>
    internal void Take(ContentId item, RunContext context)
    {
        for (int index = 0; index < this.pack.Length; index += 1)
        {
            PackValues entry = this.pack[index];
            if (string.CompareOrdinal(entry.Item.Value, item.Value) != 0)
            {
                continue;
            }

            if (entry.Count == 0)
            {
                break;
            }

            this.pack[index] = entry with { Count = entry.Count - 1 };
            return;
        }

        throw new SimulationException($"a use of the item '{item.Value}', and the pack holds none (D-775)", context);
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
