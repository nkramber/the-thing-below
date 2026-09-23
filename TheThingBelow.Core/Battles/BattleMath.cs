using System;

namespace TheThingBelow.Core.Battles;

/// <summary>
/// The numbers of one strike: the miss chance, the hit, and the damage (D-771 to D-773,
/// D-779, D-806, D-809). The rules of a turn and the evaluator both read them, so a score
/// and a blow never disagree on a number (D-959).
/// </summary>
internal static class BattleMath
{
    /// <summary>
    /// Gives the miss chance of D-773: the base, plus the rate for each point that the target
    /// is faster, clamped. Blind then adds its rate after the clamp, up to 10000 (D-806).
    /// </summary>
    /// <param name="rules">The rules.</param>
    /// <param name="attacker">The combatant that strikes.</param>
    /// <param name="target">The combatant that the strike aims at.</param>
    /// <returns>The chance, in basis points.</returns>
    internal static int MissChance(BattleRules rules, Combatant attacker, Combatant target)
    {
        long gap = (long)target.Speed - attacker.Speed;
        int chance = Clamp(rules.MissBase + (gap * rules.MissPerSpeed), rules.MissFloor, rules.MissCeiling);
        if (attacker.Statuses.Holds(StatusKind.Blind))
        {
            chance = Math.Min(BasisPoints.One, chance + rules.BlindMiss);
        }

        return chance;
    }

    /// <summary>
    /// Gives the hit of D-771 and D-772: the attack times the power, times 100 over 100 plus
    /// the defense, times the hit factor, with no rate and no floor yet.
    /// </summary>
    /// <param name="attacker">The combatant that strikes.</param>
    /// <param name="target">The combatant that the strike hits.</param>
    /// <param name="power">The power of the move, in basis points.</param>
    /// <param name="factor">The hit factor, in basis points (D-772).</param>
    /// <returns>The hit.</returns>
    internal static long Hit(Combatant attacker, Combatant target, int power, int factor)
    {
        // Each factor is at most 100000, so the product stays inside a `long` (T-2).
        long numerator = checked((long)attacker.Attack * power * 100 * factor);
        long denominator = checked((long)BasisPoints.One * (100 + target.Defense) * BasisPoints.One);
        return numerator / denominator;
    }

    /// <summary>
    /// Gives the damage of one hit, in the order of D-809: the hit, times the rate of the
    /// affinity, then the back row rate of a melee strike from the back row (D-779), the
    /// defend cut (D-755), and the shell cut of a move with an element (D-804). The result is
    /// at least 1.
    /// </summary>
    /// <param name="rules">The rules.</param>
    /// <param name="attacker">The combatant that strikes.</param>
    /// <param name="target">The combatant that the strike hits.</param>
    /// <param name="hit">The hit of <see cref="Hit"/>.</param>
    /// <param name="affinity">The affinity of the target to the element of the move, and never absorb (D-795).</param>
    /// <param name="elemental">True for a move with an element.</param>
    /// <param name="melee">True for a melee strike, which takes the back row rate (D-779).</param>
    /// <param name="defending">True when a defend of the target holds (D-755).</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <returns>The damage.</returns>
    /// <exception cref="SimulationException">The affinity is absorb, or the damage passes an `int` (T-2).</exception>
    internal static int Damage(BattleRules rules, Combatant attacker, Combatant target, long hit, Affinity affinity, bool elemental, bool melee, bool defending, RunContext context)
    {
        long damage = affinity switch
        {
            Affinity.Normal => hit,
            Affinity.Weak => checked(hit * rules.WeakRate) / BasisPoints.One,
            Affinity.Resist => checked(hit * rules.ResistRate) / BasisPoints.One,
            _ => throw new SimulationException($"the damage of a hit on the affinity '{Elements.NameOf(affinity)}', which heals (D-795)", context),
        };

        if (melee && attacker.Row == BattleRow.Back)
        {
            damage = damage * rules.BackRowRate / BasisPoints.One;
        }

        if (defending)
        {
            damage = damage * (BasisPoints.One - rules.DefendCut) / BasisPoints.One;
        }

        if (elemental && target.Statuses.Holds(StatusKind.Shell))
        {
            damage = damage * (BasisPoints.One - rules.ShellCut) / BasisPoints.One;
        }

        return damage < 1 ? 1 : ToHealth(damage, context);
    }

    /// <summary>Gives an amount as health, and refuses an amount that no `int` holds (T-2).</summary>
    /// <param name="amount">The amount.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <returns>The amount as an `int`.</returns>
    /// <exception cref="SimulationException">The amount passes an `int` (T-2).</exception>
    internal static int ToHealth(long amount, RunContext context)
    {
        if (amount > int.MaxValue)
        {
            throw new SimulationException($"a hit of {amount}, which no `int` holds", context);
        }

        return (int)amount;
    }

    /// <summary>Holds a value between a floor and a ceiling.</summary>
    /// <param name="value">The value.</param>
    /// <param name="floor">The lowest result.</param>
    /// <param name="ceiling">The highest result.</param>
    /// <returns>The value inside the range.</returns>
    internal static int Clamp(long value, int floor, int ceiling)
    {
        if (value < floor)
        {
            return floor;
        }

        return value > ceiling ? ceiling : (int)value;
    }
}
