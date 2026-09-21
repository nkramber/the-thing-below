using System;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Battles;

/// <summary>One combatant, by its side and its slot (D-764).</summary>
/// <param name="Side">The side.</param>
/// <param name="Slot">
/// The slot: the place of a character in the party, or the place of an enemy in its group
/// file, from zero (D-760).
/// </param>
public readonly record struct BattleTarget(BattleSide Side, int Slot)
{
    /// <summary>Gives the target as one text, for a log field, a console line, and an error (T-2).</summary>
    /// <returns>The side and the slot, such as `enemy 2`.</returns>
    public string Describe() => $"{BattleSides.NameOf(this.Side)} {this.Slot}";
}

/// <summary>The actions of a character on a turn (D-359, D-378, D-380, D-382, D-755).</summary>
public enum BattleAction
{
    /// <summary>The basic attack with the weapon in hand, a melee attack (D-359, D-377).</summary>
    Attack,

    /// <summary>A defend, which cuts damage until the next turn of the character (D-755).</summary>
    Defend,

    /// <summary>A step to the other row (D-380).</summary>
    Step,

    /// <summary>The use of one item on one character (D-382, D-780).</summary>
    Item,

    /// <summary>A try to flee (D-378).</summary>
    Flee,
}

/// <summary>What the character whose turn it is does (D-532, D-764, D-780).</summary>
/// <param name="Action">The action.</param>
/// <param name="Target">The target of an attack or an item, and no value for the other actions.</param>
/// <param name="Item">The item of an item use, and no value for the other actions.</param>
public sealed record BattleChoice(BattleAction Action, BattleTarget? Target, ContentId? Item);

/// <summary>
/// One strike: its delay, its power, and whether it stuns (D-376). The basic attack is one
/// move, and a lesson of PR-12 adds others, such as a heavy blow.
/// </summary>
/// <param name="Delay">The delay, in ticks at speed 100 (D-768).</param>
/// <param name="Power">The power, in basis points (D-771).</param>
/// <param name="Stun">True when a hit pushes the target back by the stun ticks of the rules (D-376).</param>
public sealed record BattleMove(int Delay, int Power, bool Stun)
{
    /// <summary>Gives the basic attack of the rules (D-359).</summary>
    /// <param name="rules">The rules.</param>
    /// <returns>The move.</returns>
    public static BattleMove BasicAttack(BattleRules rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        return new BattleMove(rules.AttackDelay, rules.AttackPower, false);
    }
}

/// <summary>The rate on each push of one combatant: normal, haste, or slow (D-376, D-768).</summary>
public enum BattlePace
{
    /// <summary>No rate, which is 10000 basis points.</summary>
    Normal,

    /// <summary>The haste rate of the rules, which shortens each push.</summary>
    Haste,

    /// <summary>The slow rate of the rules, which lengthens each push.</summary>
    Slow,
}
