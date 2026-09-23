using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Hashing;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Battles;

/// <summary>Where a combatant is (D-36, D-758).</summary>
public enum CombatantPlace
{
    /// <summary>On the field, on the timeline, and a legal target.</summary>
    Field,

    /// <summary>An enemy that waits off the field for a fall (D-778).</summary>
    Waiting,

    /// <summary>Down, off the timeline (D-36).</summary>
    Down,
}

/// <summary>How a battle ends (D-36, D-378).</summary>
public enum BattleOutcome
{
    /// <summary>The battle runs.</summary>
    Running,

    /// <summary>Every enemy is down.</summary>
    Won,

    /// <summary>A flee worked (D-378).</summary>
    Fled,

    /// <summary>Every character who fights is down (D-397).</summary>
    Wiped,
}

/// <summary>The stored values of one combatant (D-531, D-765).</summary>
/// <param name="Side">The side.</param>
/// <param name="Slot">The slot, from zero.</param>
/// <param name="Id">The id of the character or the enemy record.</param>
/// <param name="Health">The health now.</param>
/// <param name="Row">The row now.</param>
/// <param name="Place">Where the combatant is.</param>
/// <param name="ReadyAt">The tick of the timeline of its next turn.</param>
/// <param name="Defending">True while a defend holds (D-755).</param>
/// <param name="Statuses">The statuses, in the order of D-75, each with its end (D-798).</param>
/// <remarks>
/// The push rate follows from haste and slow, so the values hold no push rate from save
/// format 5 (D-768, D-800).
/// </remarks>
public sealed record CombatantValues(
    BattleSide Side,
    int Slot,
    ContentId Id,
    int Health,
    BattleRow Row,
    CombatantPlace Place,
    long ReadyAt,
    bool Defending,
    IReadOnlyList<StatusValues> Statuses);

/// <summary>The stored values of one battle (D-531).</summary>
/// <param name="Enemy">The id of the map enemy of the encounter (D-749).</param>
/// <param name="Group">The id of the group (D-753).</param>
/// <param name="Now">The tick of the timeline of the last turn.</param>
/// <param name="Outcome">How the battle ended, or `running`.</param>
/// <param name="Combatants">Every combatant, the party first, each side in slot order.</param>
public sealed record BattleValues(
    ContentId Enemy,
    ContentId Group,
    long Now,
    BattleOutcome Outcome,
    IReadOnlyList<CombatantValues> Combatants);

/// <summary>One character or one enemy in a battle.</summary>
public sealed class Combatant
{
    internal Combatant(BattleSide side, int slot, ContentId id, int fullHealth, int attack, int defense, int speed, ElementTable elements, IReadOnlyList<StatusKind> immune)
    {
        this.Side = side;
        this.Slot = slot;
        this.Id = id;
        this.FullHealth = fullHealth;
        this.Attack = attack;
        this.Defense = defense;
        this.Speed = speed;
        this.PushRate = BasisPoints.One;
        this.Elements = elements;
        this.Immune = immune;
    }

    /// <summary>The side.</summary>
    public BattleSide Side { get; }

    /// <summary>The slot, from zero (D-760).</summary>
    public int Slot { get; }

    /// <summary>The id of the character or the enemy record.</summary>
    public ContentId Id { get; }

    /// <summary>The full health of the record, or of the level of a character (D-966).</summary>
    public int FullHealth { get; private set; }

    /// <summary>The attack (D-771).</summary>
    public int Attack { get; private set; }

    /// <summary>The defense (D-771).</summary>
    public int Defense { get; private set; }

    /// <summary>The speed (D-768, D-769).</summary>
    public int Speed { get; private set; }

    /// <summary>The health now.</summary>
    public int Health { get; internal set; }

    /// <summary>The row now (D-377, D-380).</summary>
    public BattleRow Row { get; internal set; }

    /// <summary>Where the combatant is.</summary>
    public CombatantPlace Place { get; internal set; }

    /// <summary>The tick of the timeline of the next turn (D-376).</summary>
    public long ReadyAt { get; internal set; }

    /// <summary>The rate on each push, in basis points: haste, slow, or none (D-768). Haste and slow set it (D-800).</summary>
    public int PushRate { get; internal set; }

    /// <summary>True while a defend holds, until the next turn of the combatant (D-755).</summary>
    public bool Defending { get; internal set; }

    /// <summary>The affinity to each element: the table of the enemy record, or every element normal for a character until PR-13 (D-790, D-794).</summary>
    public ElementTable Elements { get; }

    /// <summary>The statuses that do nothing to this combatant (D-805). A character refuses none.</summary>
    public IReadOnlyList<StatusKind> Immune { get; }

    /// <summary>The statuses that the combatant holds (D-75, D-798).</summary>
    public StatusSet Statuses { get; } = new();

    /// <summary>The target of this combatant.</summary>
    public BattleTarget Target => new(this.Side, this.Slot);

    /// <summary>
    /// Gives a character the stats of a new level, with full health, after a battle won (D-973).
    /// The fight is over, so the new stats reach no turn, and the battle and the party then agree
    /// until the map runs again.
    /// </summary>
    /// <param name="stats">The stats of the new level.</param>
    internal void Raise(StatRow stats)
    {
        this.FullHealth = stats.Health;
        this.Attack = stats.Attack;
        this.Defense = stats.Defense;
        this.Speed = stats.Speed;
        this.Health = stats.Health;
    }
}

/// <summary>
/// The state of one battle: the combatants, the timeline, and the outcome (D-29, D-376,
/// D-531). `BattleTurns` changes it.
/// </summary>
/// <remarks>
/// The timeline counts its own ticks, apart from the tick of the run. Core resolves each
/// turn at once, so a battle takes no tick of the run (D-532). Each combatant holds the
/// tick of its next turn, and the lowest one acts, in the order of D-769 on a tie.
/// </remarks>
public sealed class Battle
{
    /// <summary>The count of turns that the strip shows (D-756).</summary>
    public const int StripTurns = 6;

    private readonly Combatant[] party;
    private readonly Combatant[] enemies;

    private Battle(ContentId enemy, GroupRecord group, Combatant[] party, Combatant[] enemies)
    {
        this.Enemy = enemy;
        this.Group = group;
        this.party = party;
        this.enemies = enemies;
        this.Outcome = BattleOutcome.Running;
    }

    /// <summary>The id of the map enemy of the encounter (D-749).</summary>
    public ContentId Enemy { get; }

    /// <summary>The group (D-766).</summary>
    public GroupRecord Group { get; }

    /// <summary>The characters, in slot order.</summary>
    public IReadOnlyList<Combatant> Party => this.party;

    /// <summary>The enemies, in the order of the group (D-760).</summary>
    public IReadOnlyList<Combatant> Enemies => this.enemies;

    /// <summary>The tick of the timeline of the last turn.</summary>
    public long Now { get; internal set; }

    /// <summary>How the battle ended, or `running`.</summary>
    public BattleOutcome Outcome { get; internal set; }

    /// <summary>
    /// Starts a battle from an encounter (D-765, D-766, D-770). A down character stays down
    /// and takes no turn. Each combatant on the field starts one attack push out, and the side
    /// that came from behind starts at tick 0 (D-770).
    /// </summary>
    /// <param name="content">The battle content of the run.</param>
    /// <param name="encounter">The encounter of the map.</param>
    /// <param name="partyState">The characters of the party.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <returns>The battle, before its first turn.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static Battle Start(BattleContent content, MapEncounter encounter, PartyState partyState, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(encounter);
        ArgumentNullException.ThrowIfNull(partyState);
        ArgumentNullException.ThrowIfNull(context);

        GroupRecord group = content.Group(encounter.Group);
        List<Combatant> party = [];
        for (int slot = 0; slot < partyState.Members.Count; slot += 1)
        {
            PartyMember member = partyState.Members[slot];
            StatRow stats = member.Stats;
            Combatant combatant = new(BattleSide.Party, slot, member.Record.Id, stats.Health, stats.Attack, stats.Defense, stats.Speed, ElementTable.AllNormal, [])
            {
                Health = member.Health,
                Row = member.Row,
                Place = member.Down ? CombatantPlace.Down : CombatantPlace.Field,
            };

            // Poison, blind, and silence follow a character into the fight (D-390, D-792).
            foreach (StatusKind status in member.Statuses)
            {
                combatant.Statuses.Put(status, null);
            }

            party.Add(combatant);
        }

        List<Combatant> enemies = [];
        for (int slot = 0; slot < group.Entries.Count; slot += 1)
        {
            GroupEntry entry = group.Entries[slot];
            EnemyRecord record = content.Enemy(entry.Enemy);
            Combatant combatant = new(BattleSide.Enemy, slot, record.Id, record.Health, record.Attack, record.Defense, record.Speed, record.Elements, record.Immune)
            {
                Health = record.Health,
                Row = entry.Row,
                Place = entry.Waits ? CombatantPlace.Waiting : CombatantPlace.Field,
            };
            enemies.Add(combatant);
        }

        var battle = new Battle(encounter.Enemy, group, [.. party], [.. enemies]);
        foreach (Combatant combatant in battle.All())
        {
            bool behind = (encounter.Behind == EncounterSide.Party && combatant.Side == BattleSide.Party)
                || (encounter.Behind == EncounterSide.Enemy && combatant.Side == BattleSide.Enemy);
            combatant.ReadyAt = behind
                ? 0
                : Push(content.Rules.AttackDelay, combatant.Speed, combatant.PushRate, context);
        }

        return battle;
    }

    /// <summary>
    /// Makes the check fight of D-948: one enemy of a group on the field in its row, against
    /// one character of the fixture. The load builds the legal actions of the enemy in it.
    /// </summary>
    /// <param name="group">The group of the entry.</param>
    /// <param name="entry">The entry, which the check puts on the field even when it waits.</param>
    /// <param name="enemy">The enemy record of the entry.</param>
    /// <param name="character">The character of the fixture.</param>
    /// <returns>The fight, before its first turn.</returns>
    internal static Battle CheckFight(GroupRecord group, GroupEntry entry, EnemyRecord enemy, CharacterRecord character)
    {
        var single = new GroupRecord(group.Id, group.Boss, new List<GroupEntry> { entry with { Waits = false } });
        StatRow stats = character.At(character.JoinLevel);
        Combatant member = new(BattleSide.Party, 0, character.Id, stats.Health, stats.Attack, stats.Defense, stats.Speed, ElementTable.AllNormal, [])
        {
            Health = stats.Health,
            Row = character.Row,
            Place = CombatantPlace.Field,
        };
        Combatant foe = new(BattleSide.Enemy, 0, enemy.Id, enemy.Health, enemy.Attack, enemy.Defense, enemy.Speed, enemy.Elements, enemy.Immune)
        {
            Health = enemy.Health,
            Row = entry.Row,
            Place = CombatantPlace.Field,
        };
        return new Battle(entry.Enemy, single, [member], [foe]);
    }

    /// <summary>Puts a battle back from the values of a snapshot (D-166, D-531).</summary>
    /// <param name="content">The battle content of this build.</param>
    /// <param name="values">The stored values.</param>
    /// <param name="partyState">The party of the snapshot, whose levels give the stats of each character (D-966).</param>
    /// <param name="source">What the values came from, such as `the save`, for an error (T-2).</param>
    /// <returns>The battle.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no battle of this content (T-2).</exception>
    public static Battle Resume(BattleContent content, BattleValues values, PartyState partyState, string source)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(partyState);
        ArgumentException.ThrowIfNullOrEmpty(source);

        GroupRecord group = content.Group(values.Group);
        List<Combatant> party = [];
        List<Combatant> enemies = [];
        foreach (CombatantValues stored in values.Combatants)
        {
            ArgumentNullException.ThrowIfNull(stored);
            List<Combatant> side = stored.Side == BattleSide.Party ? party : enemies;
            Refuse(stored.Slot != side.Count, source, $"the combatant '{stored.Id.Value}' holds the slot {stored.Slot}, and the next slot of its side is {side.Count}");
            Combatant combatant = RecordOf(content, group, partyState, stored, source);
            Refuse(
                stored.Health < 0 || stored.Health > combatant.FullHealth,
                source,
                $"the combatant '{stored.Id.Value}' holds the health {stored.Health}, and the range is 0 to {combatant.FullHealth}");
            Refuse(
                (stored.Health == 0) != (stored.Place == CombatantPlace.Down),
                source,
                $"the combatant '{stored.Id.Value}' holds the health {stored.Health} and the place '{PlaceName(stored.Place)}', and a down holds zero health alone");
            Refuse(stored.ReadyAt < 0, source, $"the next turn of '{stored.Id.Value}' is at {stored.ReadyAt}, which is below zero");
            combatant.Health = stored.Health;
            combatant.Row = stored.Row;
            combatant.Place = stored.Place;
            combatant.ReadyAt = stored.ReadyAt;
            combatant.Defending = stored.Defending;
            PutStatuses(combatant, stored, values.Now, source);
            combatant.PushRate = PushRateOf(combatant, content.Rules);
            side.Add(combatant);
        }

        Refuse(enemies.Count != group.Entries.Count, source, $"it holds {enemies.Count} enemies, and the group '{group.Id.Value}' holds {group.Entries.Count}");
        Refuse(values.Now < 0, source, $"the timeline is at {values.Now}, which is below zero");

        return new Battle(values.Enemy, group, [.. party], [.. enemies])
        {
            Now = values.Now,
            Outcome = values.Outcome,
        };
    }

    /// <summary>Gives the rate on each push that the statuses of a combatant set: haste, slow, or none (D-768, D-800).</summary>
    /// <param name="combatant">The combatant.</param>
    /// <param name="rules">The rules, for the two rates.</param>
    /// <returns>The rate, in basis points.</returns>
    public static int PushRateOf(Combatant combatant, BattleRules rules)
    {
        ArgumentNullException.ThrowIfNull(combatant);
        ArgumentNullException.ThrowIfNull(rules);

        if (combatant.Statuses.Holds(StatusKind.Haste))
        {
            return rules.HasteRate;
        }

        return combatant.Statuses.Holds(StatusKind.Slow) ? rules.SlowRate : BasisPoints.One;
    }

    /// <summary>Gives the push of one action: the delay times 100, divided by the speed, times the rate, and at least 1 (D-768).</summary>
    /// <param name="delay">The delay of the action, in ticks at speed 100.</param>
    /// <param name="speed">The speed of the user. It must be above zero.</param>
    /// <param name="pushRate">The haste or slow rate, in basis points (D-768).</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <returns>The push, in ticks of the timeline.</returns>
    /// <exception cref="SimulationException">A value is out of its range (T-2).</exception>
    public static long Push(int delay, int speed, int pushRate, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (delay <= 0 || speed <= 0 || pushRate <= 0)
        {
            throw new SimulationException(
                $"a push of the delay {delay} at the speed {speed} and the rate {pushRate}, and each value must be above zero (D-768)",
                context);
        }

        // Each value is at most 100000, so the product stays far inside a `long` (T-2).
        long push = checked((long)delay * 100 * pushRate / ((long)speed * BasisPoints.One));
        return push < 1 ? 1 : push;
    }

    /// <summary>Gives every combatant, the party first, each side in slot order (G-4).</summary>
    /// <returns>The combatants.</returns>
    public IReadOnlyList<Combatant> All()
    {
        // A spread of an array into a list makes the compiler call `System.Linq`, and G-1 keeps
        // that assembly out of the reference list of Core.
        var all = new List<Combatant>(this.party);
        all.AddRange(this.enemies);
        return all;
    }

    /// <summary>Finds one combatant.</summary>
    /// <param name="target">The side and the slot.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <returns>The combatant.</returns>
    /// <exception cref="SimulationException">The side holds no such slot (T-2).</exception>
    public Combatant At(BattleTarget target, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Combatant[] side = target.Side == BattleSide.Party ? this.party : this.enemies;
        if (target.Slot < 0 || target.Slot >= side.Length)
        {
            throw new SimulationException(
                $"the target {target.Describe()}, and that side holds the slots 0 to {side.Length - 1}",
                context);
        }

        return side[target.Slot];
    }

    /// <summary>Gives the combatant whose turn comes next, or null when no combatant stands (D-769).</summary>
    /// <returns>The combatant with the lowest next turn, by the order of D-769 on a tie.</returns>
    public Combatant? Next()
    {
        Combatant? next = null;
        foreach (Combatant combatant in this.All())
        {
            if (combatant.Place == CombatantPlace.Field && (next is null || ComesFirst(combatant.ReadyAt, combatant, next.ReadyAt, next)))
            {
                next = combatant;
            }
        }

        return next;
    }

    /// <summary>
    /// Gives the turns that the strip shows (D-756). The strip reads ahead as though each
    /// combatant attacks on each turn, so an action of another delay moves it (D-376).
    /// </summary>
    /// <param name="rules">The rules, for the delay of the attack.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <returns>The next six turns, or fewer when no combatant stands.</returns>
    public IReadOnlyList<BattleTarget> Strip(BattleRules rules, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(context);

        List<Combatant> standing = [];
        List<long> readyAt = [];
        foreach (Combatant combatant in this.All())
        {
            if (combatant.Place == CombatantPlace.Field)
            {
                standing.Add(combatant);
                readyAt.Add(combatant.ReadyAt);
            }
        }

        List<BattleTarget> strip = [];
        while (standing.Count > 0 && strip.Count < StripTurns)
        {
            int first = 0;
            for (int index = 1; index < standing.Count; index += 1)
            {
                if (ComesFirst(readyAt[index], standing[index], readyAt[first], standing[first]))
                {
                    first = index;
                }
            }

            strip.Add(standing[first].Target);
            readyAt[first] = checked(readyAt[first] + Push(rules.AttackDelay, standing[first].Speed, standing[first].PushRate, context));
        }

        return strip;
    }

    /// <summary>
    /// Gives the targets that melee reaches on one side: the front row while anyone stands in
    /// it, and the back row after that (D-377).
    /// </summary>
    /// <param name="side">The side of the targets.</param>
    /// <returns>The targets, in slot order.</returns>
    public IReadOnlyList<Combatant> MeleeTargets(BattleSide side)
    {
        Combatant[] members = side == BattleSide.Party ? this.party : this.enemies;
        List<Combatant> front = [];
        List<Combatant> back = [];
        foreach (Combatant combatant in members)
        {
            if (combatant.Place != CombatantPlace.Field)
            {
                continue;
            }

            if (combatant.Row == BattleRow.Front)
            {
                front.Add(combatant);
            }
            else
            {
                back.Add(combatant);
            }
        }

        return front.Count > 0 ? front : back;
    }

    /// <summary>Gives the stored values of the battle (D-531).</summary>
    /// <returns>The values.</returns>
    public BattleValues Values()
    {
        List<CombatantValues> combatants = [];
        foreach (Combatant combatant in this.All())
        {
            combatants.Add(new CombatantValues(
                combatant.Side,
                combatant.Slot,
                combatant.Id,
                combatant.Health,
                combatant.Row,
                combatant.Place,
                combatant.ReadyAt,
                combatant.Defending,
                combatant.Statuses.Values()));
        }

        return new BattleValues(this.Enemy, this.Group.Id, this.Now, this.Outcome, combatants);
    }

    /// <summary>Adds every value of the battle to the state hash, in one fixed order (G-5).</summary>
    /// <param name="hasher">The hasher of the state.</param>
    public void Hash(StateHasher hasher)
    {
        ArgumentNullException.ThrowIfNull(hasher);

        hasher.AddText(this.Enemy.Value);
        hasher.AddText(this.Group.Id.Value);
        hasher.AddInt64(this.Now);
        hasher.AddInt32((int)this.Outcome);
        foreach (Combatant combatant in this.All())
        {
            hasher.AddInt32((int)combatant.Side);
            hasher.AddInt32(combatant.Slot);
            hasher.AddInt32(combatant.Health);
            hasher.AddInt32((int)combatant.Row);
            hasher.AddInt32((int)combatant.Place);
            hasher.AddInt64(combatant.ReadyAt);
            hasher.AddInt32(combatant.PushRate);
            hasher.AddBoolean(combatant.Defending);
            combatant.Statuses.Hash(hasher);
        }
    }

    /// <summary>Gives the name of a place, for a snapshot and an error (T-2).</summary>
    /// <param name="place">The place.</param>
    /// <returns>The name, such as `waiting`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no place (T-2).</exception>
    public static string PlaceName(CombatantPlace place) => place switch
    {
        CombatantPlace.Field => "field",
        CombatantPlace.Waiting => "waiting",
        CombatantPlace.Down => "down",
        _ => throw new ArgumentOutOfRangeException(nameof(place), place, "the value names no place of a combatant (D-758)"),
    };

    /// <summary>Gives the name of an outcome, for a snapshot and an error (T-2).</summary>
    /// <param name="outcome">The outcome.</param>
    /// <returns>The name, such as `won`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no outcome (T-2).</exception>
    public static string OutcomeName(BattleOutcome outcome) => outcome switch
    {
        BattleOutcome.Running => "running",
        BattleOutcome.Won => "won",
        BattleOutcome.Fled => "fled",
        BattleOutcome.Wiped => "wiped",
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "the value names no outcome of a battle (D-36)"),
    };

    /// <summary>
    /// True when the first combatant acts before the second: the lower tick, then the higher
    /// speed, then the party, then the lower slot (D-769).
    /// </summary>
    private static bool ComesFirst(long firstAt, Combatant first, long secondAt, Combatant second)
    {
        if (firstAt != secondAt)
        {
            return firstAt < secondAt;
        }

        if (first.Speed != second.Speed)
        {
            return first.Speed > second.Speed;
        }

        if (first.Side != second.Side)
        {
            return first.Side == BattleSide.Party;
        }

        return first.Slot < second.Slot;
    }

    private static Combatant RecordOf(BattleContent content, GroupRecord group, PartyState partyState, CombatantValues stored, string source)
    {
        if (stored.Side == BattleSide.Party)
        {
            // The stats of a character follow its level, which the party holds (D-966).
            Refuse(
                stored.Slot >= partyState.Members.Count,
                source,
                $"the party slot {stored.Slot} is past the {partyState.Members.Count} characters of the party");
            PartyMember member = partyState.Members[stored.Slot];
            Refuse(
                string.CompareOrdinal(member.Record.Id.Value, stored.Id.Value) != 0,
                source,
                $"the party slot {stored.Slot} holds '{stored.Id.Value}', and the party holds '{member.Record.Id.Value}' there");
            StatRow stats = member.Stats;
            return new Combatant(BattleSide.Party, stored.Slot, member.Record.Id, stats.Health, stats.Attack, stats.Defense, stats.Speed, ElementTable.AllNormal, []);
        }

        Refuse(stored.Slot >= group.Entries.Count, source, $"the enemy slot {stored.Slot} is past the group '{group.Id.Value}'");
        ContentId expected = group.Entries[stored.Slot].Enemy;
        Refuse(
            string.CompareOrdinal(expected.Value, stored.Id.Value) != 0,
            source,
            $"the enemy slot {stored.Slot} holds '{stored.Id.Value}', and the group '{group.Id.Value}' holds '{expected.Value}' there");
        EnemyRecord enemy = content.Enemy(stored.Id);
        return new Combatant(BattleSide.Enemy, stored.Slot, enemy.Id, enemy.Health, enemy.Attack, enemy.Defense, enemy.Speed, enemy.Elements, enemy.Immune);
    }

    /// <summary>
    /// Puts the stored statuses on a combatant, and refuses a set that no run can make: a
    /// status off the field, a repeat, an end at or before the timeline, a wrong end, a status
    /// that the enemy refuses, or haste with slow (D-390, D-798, D-800, D-801, D-805).
    /// </summary>
    private static void PutStatuses(Combatant combatant, CombatantValues stored, long now, string source)
    {
        string who = stored.Id.Value;
        Refuse(
            stored.Statuses.Count > 0 && stored.Place != CombatantPlace.Field,
            source,
            $"the combatant '{who}' holds a status in the place '{PlaceName(stored.Place)}', and a status holds on the field alone (D-801)");
        foreach (StatusValues status in stored.Statuses)
        {
            ArgumentNullException.ThrowIfNull(status);
            string name = Statuses.NameOf(status.Status);
            Refuse(combatant.Statuses.Holds(status.Status), source, $"the combatant '{who}' holds '{name}' two times (D-800)");
            Refuse(
                Statuses.Lasts(status.Status) != (status.EndsAt is null),
                source,
                $"the status '{name}' of '{who}' takes {(Statuses.Lasts(status.Status) ? "no end" : "an end")} (D-390, D-798)");
            Refuse(
                status.EndsAt is long ends && ends <= now,
                source,
                $"the status '{name}' of '{who}' ends at {status.EndsAt}, and the timeline is at {now}, past its end (D-798)");
            Refuse(Statuses.Refuses(combatant.Immune, status.Status), source, $"the enemy '{who}' holds '{name}', which it refuses (D-805)");
            combatant.Statuses.Put(status.Status, status.EndsAt);
        }

        Refuse(
            combatant.Statuses.Holds(StatusKind.Haste) && combatant.Statuses.Holds(StatusKind.Slow),
            source,
            $"the combatant '{who}' holds haste and slow, and each removes the other (D-800)");
    }

    private static void Refuse(bool broken, string source, string reason)
    {
        if (broken)
        {
            throw new ArgumentException($"The battle of {source} is not a state of a run: {reason}.");
        }
    }
}
