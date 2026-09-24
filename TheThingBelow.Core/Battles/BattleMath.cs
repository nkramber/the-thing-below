using System;

namespace TheThingBelow.Core.Battles;

/// <summary>
/// The numbers of one strike and one heal: the miss chance, the hit, the damage, the heal of
/// an absorb, and the heal of an ability (D-771 to D-773, D-779, D-806, D-809, D-1055,
/// D-1057). The rules of a turn and the evaluator both read them, so a score and a blow never
/// disagree on a number (D-959).
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
    /// the defense, times the hit factor, with no rate and no floor yet. A magic hit reads the
    /// magic and the resistance in place of the attack and the defense (D-1053).
    /// </summary>
    /// <param name="attacker">The combatant that strikes.</param>
    /// <param name="target">The combatant that the strike hits.</param>
    /// <param name="stat">The stats that the move reads (D-1053).</param>
    /// <param name="power">The power of the move, in basis points.</param>
    /// <param name="factor">The hit factor, in basis points (D-772).</param>
    /// <returns>The hit.</returns>
    internal static long Hit(Combatant attacker, Combatant target, StrikeStat stat, int power, int factor)
    {
        int strength = stat == StrikeStat.Magic ? attacker.Magic : attacker.Attack;
        int guard = stat == StrikeStat.Magic ? target.Resistance : target.Defense;

        // Each factor is at most 100000, so the product stays inside a `long` (T-2).
        long numerator = checked((long)strength * power * 100 * factor);
        long denominator = checked((long)BasisPoints.One * (100 + guard) * BasisPoints.One);
        return numerator / denominator;
    }

    /// <summary>
    /// Gives the heal of an absorbed hit: the hit times the absorb rate, rounded down, and at
    /// least 1 (D-795, D-1055). The caller holds the heal to the health that the target lacks.
    /// </summary>
    /// <param name="rules">The rules.</param>
    /// <param name="hit">The hit of <see cref="Hit"/>.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <returns>The heal.</returns>
    /// <exception cref="SimulationException">The heal passes an `int` (T-2).</exception>
    internal static int AbsorbHeal(BattleRules rules, long hit, RunContext context)
    {
        long heal = checked(hit * rules.AbsorbRate) / BasisPoints.One;
        return heal < 1 ? 1 : ToHealth(heal, context);
    }

    /// <summary>
    /// Gives the amount of a heal ability: the base plus the magic of the caster times the
    /// power, then the aptitude rate and the hit factor, rounded down, and at least 1 (D-1028,
    /// D-1057, D-1058). The caller holds the heal to the health that the target lacks.
    /// </summary>
    /// <param name="heal">The heal ability.</param>
    /// <param name="magic">The magic of the caster.</param>
    /// <param name="rate">The aptitude rate of the caster, in basis points: 10000 plus the bonus of D-1028, or 10000 for an enemy.</param>
    /// <param name="factor">The hit factor, in basis points (D-772).</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <returns>The heal.</returns>
    /// <exception cref="SimulationException">The heal passes an `int` (T-2).</exception>
    internal static int HealAmount(HealAbility heal, int magic, int rate, int factor, RunContext context)
    {
        // One rounding at the end, so no step rounds down before the next (D-169). At the
        // content limits the scaled share times the factor passes a `long`. Thus the share
        // splits at the scale: the whole part times the factor is exact, and the rest times the
        // factor stays below the scale times 100000. The two parts give the one rounded result.
        long scale = (long)BasisPoints.One * BasisPoints.One * BasisPoints.One;
        long share = checked(((long)heal.Base * BasisPoints.One) + ((long)magic * heal.Power));
        long scaled = checked(share * rate);
        long amount = checked(((scaled / scale) * factor) + ((scaled % scale) * factor / scale));
        return amount < 1 ? 1 : ToHealth(amount, context);
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
