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
/// One strike: its delay, its power, its element, and the status that it gives on a hit
/// (D-376, D-793). The basic attack is one move, and a lesson of PR-12 adds others, such as a
/// heavy blow or a rite of fire.
/// </summary>
/// <param name="Delay">The delay, in ticks at speed 100 (D-768).</param>
/// <param name="Power">The power, in basis points (D-771).</param>
/// <param name="Element">The one element of the move, or none (D-796).</param>
/// <param name="Status">The status that a hit gives, with its chance, or none (D-807).</param>
public sealed record BattleMove(int Delay, int Power, Element? Element, StatusChance? Status)
{
    /// <summary>Gives the basic attack of the rules (D-359).</summary>
    /// <param name="rules">The rules.</param>
    /// <returns>The move.</returns>
    public static BattleMove BasicAttack(BattleRules rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        return new BattleMove(rules.AttackDelay, rules.AttackPower, null, null);
    }
}
