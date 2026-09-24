using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Game.Ui;

/// <summary>One combatant as the battle screen shows it now (D-111).</summary>
/// <remarks>
/// The values follow the events that the screen played, and never the state of the rules,
/// which can stand several events ahead (D-532).
/// </remarks>
public sealed class ShownCombatant
{
    private readonly List<StatusKind> statuses;

    internal ShownCombatant(
        BattleTarget target,
        ContentId id,
        int fullHealth,
        int health,
        BattleRow row,
        CombatantPlace place,
        IReadOnlyList<StatusKind> statuses)
    {
        this.Target = target;
        this.Id = id;
        this.FullHealth = fullHealth;
        this.Health = health;
        this.Row = row;
        this.Place = place;
        this.statuses = [.. statuses];
    }

    /// <summary>The side and the slot of the combatant.</summary>
    public BattleTarget Target { get; }

    /// <summary>The content id of the character or of the enemy, such as `enemy.fixture_grunt`.</summary>
    public ContentId Id { get; }

    /// <summary>The full health of the combatant, which a level-up raises (D-973).</summary>
    public int FullHealth { get; internal set; }

    /// <summary>The health that the screen shows.</summary>
    public int Health { get; internal set; }

    /// <summary>The row that the screen shows (D-377).</summary>
    public BattleRow Row { get; internal set; }

    /// <summary>The place that the screen shows: on the field, down, or waiting (D-758).</summary>
    public CombatantPlace Place { get; internal set; }

    /// <summary>The statuses that the screen shows, in the order of <see cref="Core.Battles.Statuses.All"/>.</summary>
    public IReadOnlyList<StatusKind> Statuses => this.statuses;

    /// <summary>The join level and the curve of a character, and no value for an enemy (D-966).</summary>
    public CharacterRecord? Record { get; private set; }

    /// <summary>The level of a character, and zero for an enemy (D-34).</summary>
    public int Level { get; private set; }

    /// <summary>The MP of a character, and zero for an enemy (D-42).</summary>
    public int Mp { get; internal set; }

    /// <summary>The full MP of a character, and zero for an enemy (D-42).</summary>
    public int FullMp { get; private set; }

    /// <summary>The level before the last level-up, which the stat lines of the summary read (D-975).</summary>
    public int LevelBefore { get; private set; }

    /// <summary>The health before the last level-up, where the fill of the bar starts (D-975).</summary>
    public int HealthBefore { get; private set; }

    /// <summary>The MP before the last level-up, where the fill of the bar starts (D-975).</summary>
    public int MpBefore { get; private set; }

    internal void Put(StatusKind status)
    {
        if (!this.statuses.Contains(status))
        {
            this.statuses.Add(status);
            this.statuses.Sort();
        }
    }

    internal void Remove(StatusKind status) => this.statuses.Remove(status);

    internal void RemoveAll() => this.statuses.Clear();

    /// <summary>Gives the combatant the level and the MP of a character (D-966).</summary>
    internal void Grow(CharacterRecord record, int level, int mp)
    {
        this.Record = record;
        this.Level = level;
        this.LevelBefore = level;
        this.Mp = mp;
        this.FullMp = record.At(level).Mp;
    }

    /// <summary>Raises the character to a new level, and fills its health and its MP (D-973).</summary>
    internal void LevelUp(int level)
    {
        CharacterRecord record = this.Record ?? throw new InvalidOperationException(
            $"The battle screen raises '{this.Id.Value}' to level {level}, and only a character holds a level (D-34, T-2).");
        StatRow stats = record.At(level);
        this.LevelBefore = this.Level;
        this.HealthBefore = this.Health;
        this.MpBefore = this.Mp;
        this.Level = level;
        this.FullHealth = stats.Health;
        this.FullMp = stats.Mp;
        this.Health = stats.Health;
        this.Mp = stats.Mp;
    }
}

/// <summary>
/// The fight as the screen shows it: each combatant with the health, the row, the place, and
/// the statuses of the events that the screen played (D-111, D-532).
/// </summary>
/// <remarks>
/// The rules resolve each enemy turn at once, so the state of a battle can stand several
/// events ahead of the screen. The screen thus starts from the values of the start of the
/// fight and applies each event when it plays it. After the last event the view matches the
/// state, and a test proves it over many seeds (T-3).
/// <para>
/// This type holds no Godot value, so a test reads it from the built Game assembly with no
/// engine (D-614).
/// </para>
/// </remarks>
public sealed class BattleView
{
    private readonly ShownCombatant[] party;
    private readonly ShownCombatant[] enemies;

    private BattleView(ShownCombatant[] party, ShownCombatant[] enemies)
    {
        this.party = party;
        this.enemies = enemies;
    }

    /// <summary>The characters, in slot order.</summary>
    public IReadOnlyList<ShownCombatant> Party => this.party;

    /// <summary>The enemies of the group, in slot order.</summary>
    public IReadOnlyList<ShownCombatant> Enemies => this.enemies;

    /// <summary>
    /// Builds the view of the start of a fight: the characters as they entered it, and every
    /// enemy at full health in the row of its group entry (D-535, D-760).
    /// </summary>
    /// <param name="state">The run, with the battle that started.</param>
    /// <returns>The view before the first event after the start.</returns>
    /// <exception cref="ArgumentNullException">The state is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">No battle runs (T-2).</exception>
    /// <remarks>
    /// The party state keeps the values of the start until the fight ends, because the rules
    /// copy the health and the statuses back at the end alone (D-776). A group entry names the
    /// row and the wait of each enemy (D-760).
    /// </remarks>
    public static BattleView AtStart(RunState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        Battle battle = state.Battle ?? throw new InvalidOperationException(
            $"The battle screen builds the start of a fight at tick {state.Tick}, and no battle runs (T-2).");

        var shownParty = new ShownCombatant[battle.Party.Count];
        for (int slot = 0; slot < shownParty.Length; slot += 1)
        {
            Combatant combatant = battle.Party[slot];
            PartyMember member = state.Characters.Members[slot];
            shownParty[slot] = new ShownCombatant(
                combatant.Target,
                combatant.Id,
                combatant.FullHealth,
                member.Health,
                member.Row,
                member.Down ? CombatantPlace.Down : CombatantPlace.Field,
                member.Statuses);
            shownParty[slot].Grow(member.Record, member.Level, member.Mp);
        }

        var shownEnemies = new ShownCombatant[battle.Enemies.Count];
        for (int slot = 0; slot < shownEnemies.Length; slot += 1)
        {
            Combatant combatant = battle.Enemies[slot];
            GroupEntry entry = battle.Group.Entries[slot];
            shownEnemies[slot] = new ShownCombatant(
                combatant.Target,
                combatant.Id,
                combatant.FullHealth,
                combatant.FullHealth,
                entry.Row,
                entry.Waits ? CombatantPlace.Waiting : CombatantPlace.Field,
                []);
        }

        return new BattleView(shownParty, shownEnemies);
    }

    /// <summary>
    /// Builds the view of a fight as it stands now. A run that loads a save inside a fight
    /// has no event to play, so the state is the view (D-531).
    /// </summary>
    /// <param name="state">The run, with the battle and the party.</param>
    /// <returns>The view of the battle.</returns>
    /// <exception cref="ArgumentNullException">The state is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">No battle runs (T-2).</exception>
    public static BattleView Of(RunState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        Battle battle = state.Battle ?? throw new InvalidOperationException(
            $"The battle screen builds the view of a fight at tick {state.Tick}, and no battle runs (T-2).");
        ShownCombatant[] party = ShownOf(battle.Party);
        for (int slot = 0; slot < party.Length; slot += 1)
        {
            PartyMember member = state.Characters.Members[slot];
            party[slot].Grow(member.Record, member.Level, member.Mp);
        }

        return new BattleView(party, ShownOf(battle.Enemies));
    }

    /// <summary>Gives the combatant at one side and one slot.</summary>
    /// <param name="target">The side and the slot.</param>
    /// <returns>The combatant.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The side holds no such slot (T-2).</exception>
    public ShownCombatant At(BattleTarget target)
    {
        ShownCombatant[] side = target.Side == BattleSide.Party ? this.party : this.enemies;
        if (target.Slot < 0 || target.Slot >= side.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(target),
                target,
                $"The battle screen holds {side.Length} combatants on the side '{BattleSides.NameOf(target.Side)}', and no slot {target.Slot} (T-2).");
        }

        return side[target.Slot];
    }

    /// <summary>Applies one event that the screen starts to play (D-532).</summary>
    /// <param name="played">The event.</param>
    /// <exception cref="ArgumentNullException">The event is null (T-2).</exception>
    /// <exception cref="ArgumentException">An event that needs a target or a status holds none (T-2).</exception>
    /// <remarks>
    /// A hit takes the health down to zero at most, as the rules do. The amount of a hit is
    /// the damage of the blow, which can pass the health that the target held.
    /// </remarks>
    public void Apply(BattleEvent played)
    {
        ArgumentNullException.ThrowIfNull(played);

        ShownCombatant actor = this.At(played.Actor);
        switch (played.Kind)
        {
            case BattleEventKind.Hit:
                ShownCombatant struck = this.At(TargetOf(played));
                struck.Health = Math.Max(0, struck.Health - played.Amount);
                return;
            case BattleEventKind.Absorb:
            case BattleEventKind.Item:
            case BattleEventKind.Heal:
                ShownCombatant healed = this.At(TargetOf(played));
                healed.Health = checked(healed.Health + played.Amount);
                return;
            case BattleEventKind.StatusHurt:
                actor.Health = Math.Max(0, actor.Health - played.Amount);
                return;
            case BattleEventKind.StatusHeal:
                actor.Health = checked(actor.Health + played.Amount);
                return;
            case BattleEventKind.Step:
                actor.Row = BattleSides.Other(actor.Row);
                return;
            case BattleEventKind.Down:
                actor.Place = CombatantPlace.Down;
                actor.RemoveAll();
                return;
            case BattleEventKind.StepIn:
                actor.Place = CombatantPlace.Field;
                return;
            case BattleEventKind.StatusOn:
                actor.Put(StatusOf(played));
                return;
            case BattleEventKind.StatusOff:
                actor.Remove(StatusOf(played));
                return;
            case BattleEventKind.LevelUp:
                actor.LevelUp(played.Amount);
                return;
            case BattleEventKind.Lesson:
                // The amount of a lesson event is the MP that the form spent (D-1027).
                actor.Mp = Math.Max(0, actor.Mp - played.Amount);
                return;
            default:
                // A start, a turn, a miss, a defend, a failed flee, an immune status, a sleep,
                // the three ends, and the experience change no value that the screen shows.
                return;
        }
    }

    private static ShownCombatant[] ShownOf(IReadOnlyList<Combatant> side)
    {
        var shown = new ShownCombatant[side.Count];
        for (int slot = 0; slot < shown.Length; slot += 1)
        {
            Combatant combatant = side[slot];
            var statuses = new List<StatusKind>();
            foreach (StatusKind status in Core.Battles.Statuses.All)
            {
                if (combatant.Statuses.Holds(status))
                {
                    statuses.Add(status);
                }
            }

            shown[slot] = new ShownCombatant(
                combatant.Target,
                combatant.Id,
                combatant.FullHealth,
                combatant.Health,
                combatant.Row,
                combatant.Place,
                statuses);
        }

        return shown;
    }

    private static BattleTarget TargetOf(BattleEvent played) =>
        played.Target ?? throw new ArgumentException(
            $"The battle event '{BattleEvents.NameOf(played.Kind)}' of {played.Actor.Describe()} holds no target (T-2).",
            nameof(played));

    private static StatusKind StatusOf(BattleEvent played) =>
        played.Status ?? throw new ArgumentException(
            $"The battle event '{BattleEvents.NameOf(played.Kind)}' of {played.Actor.Describe()} holds no status (T-2).",
            nameof(played));
}
