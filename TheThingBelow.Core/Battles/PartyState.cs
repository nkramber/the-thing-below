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
public sealed record CharacterValues(ContentId Character, int Health, BattleRow Row, IReadOnlyList<StatusKind> Statuses);

/// <summary>The stored count of one item of the pack (D-775).</summary>
/// <param name="Item">The id of the item.</param>
/// <param name="Count">The count now.</param>
public sealed record PackValues(ContentId Item, int Count);

/// <summary>One character of the party, with the health and the row that last between battles (D-36, D-765).</summary>
public sealed class PartyMember
{
    internal PartyMember(CharacterRecord record, int health, BattleRow row, IReadOnlyList<StatusKind> statuses)
    {
        this.Record = record;
        this.Health = health;
        this.Row = row;
        this.Statuses = statuses;
    }

    /// <summary>The fixed stats of the character (D-765).</summary>
    public CharacterRecord Record { get; }

    /// <summary>The health now, from zero to the full health of the record.</summary>
    public int Health { get; internal set; }

    /// <summary>The row now, which the next battle starts from (D-558).</summary>
    public BattleRow Row { get; internal set; }

    /// <summary>Poison, blind, and silence, in the order of D-75, until a cure or a rest at a hub (D-390, D-792). PR-64 makes them act on the map.</summary>
    public IReadOnlyList<StatusKind> Statuses { get; internal set; }

    /// <summary>True while the character is down, which lasts until a hub or a rare item (D-36).</summary>
    public bool Down => this.Health == 0;
}

/// <summary>
/// The characters of the party and their pack, which last between battles (D-36, D-765,
/// D-775). The snapshot holds them from save format 4, and the statuses that last from save format 5 (D-792).
/// </summary>
public sealed class PartyState
{
    private readonly PartyMember[] members;
    private readonly PackValues[] pack;

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
            members.Add(new PartyMember(record, record.Health, record.Row, []));
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
            Refuse(
                stored.Health < 0 || stored.Health > record.Health,
                source,
                $"the character '{record.Id.Value}' holds the health {stored.Health}, and the range is 0 to {record.Health}");
            foreach (PartyMember earlier in members)
            {
                Refuse(
                    string.CompareOrdinal(earlier.Record.Id.Value, record.Id.Value) == 0,
                    source,
                    $"it holds the character '{record.Id.Value}' two times");
            }

            CheckStatuses(stored, source);
            members.Add(new PartyMember(record, stored.Health, stored.Row, stored.Statuses));
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
            values.Add(new CharacterValues(member.Record.Id, member.Health, member.Row, member.Statuses));
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
            hasher.AddInt32(member.Health);
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

    private static void Refuse(bool broken, string source, string reason)
    {
        if (broken)
        {
            throw new ArgumentException($"The party of {source} is not a state of a run: {reason}.");
        }
    }
}
