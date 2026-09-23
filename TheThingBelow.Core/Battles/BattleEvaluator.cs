using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Streams;

namespace TheThingBelow.Core.Battles;

/// <summary>The kinds of the action of an enemy on its turn (D-359, D-755, D-380, D-955).</summary>
public enum EnemyActionKind
{
    /// <summary>The basic attack, a melee strike (D-359, D-787).</summary>
    Attack,

    /// <summary>A move of the ability file: a strike or a heal (D-955).</summary>
    Ability,

    /// <summary>A defend, which cuts damage until the next turn of the enemy (D-755).</summary>
    Defend,

    /// <summary>A step to the other row (D-380).</summary>
    Step,
}

/// <summary>One action that an enemy can take on its turn (D-65, D-955).</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Ability">The ability of a move, and no value for the other kinds.</param>
/// <param name="Target">The target of an attack or a move, and no value for a defend or a step.</param>
public sealed record EnemyAction(EnemyActionKind Kind, AbilityRecord? Ability, BattleTarget? Target)
{
    /// <summary>Gives the action as one text, for a log field and an error (T-2).</summary>
    /// <returns>The action, such as `ability ability.fixture_mend at enemy 1`.</returns>
    public string Describe()
    {
        string kind = this.Kind switch
        {
            EnemyActionKind.Attack => "attack",
            EnemyActionKind.Ability => $"ability {this.Ability?.Id.Value}",
            EnemyActionKind.Defend => "defend",
            EnemyActionKind.Step => "step",
            _ => $"kind {(int)this.Kind}",
        };
        return this.Target is BattleTarget target ? $"{kind} at {target.Describe()}" : kind;
    }
}

/// <summary>One legal action with its score and the term values of that score (D-65, D-959).</summary>
/// <param name="Action">The action.</param>
/// <param name="Terms">The value of each term, before the weights.</param>
/// <param name="Score">The sum of each weight times its term.</param>
public sealed record ScoredAction(EnemyAction Action, ScoreTerms Terms, long Score);

/// <summary>
/// The value of each term of one score, before the weights (D-65, D-377, D-959, D-960). The
/// area file `area-battle.md` gives the unit of each term.
/// </summary>
/// <param name="Damage">The expected health that the action takes from the other side. An absorb gives a value below zero.</param>
/// <param name="Kills">The expected kills, in basis points of one kill.</param>
/// <param name="Threat">The expected health that the reply of D-960 takes from this side.</param>
/// <param name="Healing">The health that the action restores to this side.</param>
/// <param name="Timeline">The push of the action on its user, in ticks.</param>
/// <param name="Row">The change in the count of this side that the melee of the other side cannot reach.</param>
public sealed record ScoreTerms(long Damage, long Kills, long Threat, long Healing, long Timeline, long Row);

/// <summary>
/// The evaluator of the enemies (D-65, D-534, D-947, D-955, D-959, D-960). It scores each
/// legal action of an enemy by its expected outcome and by the best reply of the next
/// character, and it takes the action with the highest score.
/// </summary>
/// <remarks>
/// The evaluator draws no roll for a score (D-959). Only a tie of the highest score draws,
/// from the stream of the evaluator alone, so a tie never moves a roll of the battle stream
/// (D-947, G-4). The order of the legal actions is fixed: each attack in slot order, each
/// ability in the order of the record with each target in slot order, the defend, and the
/// step. The draw of a tie reads that order (T-7).
/// </remarks>
public static class BattleEvaluator
{
    /// <summary>Gives every legal action of an enemy on the field, in the fixed order of the evaluator (D-377, D-955).</summary>
    /// <param name="battle">The battle.</param>
    /// <param name="enemy">The enemy.</param>
    /// <param name="content">The battle content, which holds the abilities.</param>
    /// <returns>The actions. The basic attack, the defend, and the step keep the list from empty (D-962).</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static IReadOnlyList<EnemyAction> LegalActions(Battle battle, Combatant enemy, BattleContent content)
    {
        ArgumentNullException.ThrowIfNull(battle);
        ArgumentNullException.ThrowIfNull(enemy);
        ArgumentNullException.ThrowIfNull(content);

        List<EnemyAction> actions = [];
        foreach (Combatant target in battle.MeleeTargets(BattleSide.Party))
        {
            actions.Add(new EnemyAction(EnemyActionKind.Attack, null, target.Target));
        }

        foreach (ContentId id in content.Enemy(enemy.Id).Abilities)
        {
            AbilityRecord ability = content.Abilities.Ability(id);
            foreach (Combatant target in TargetsOf(battle, ability))
            {
                actions.Add(new EnemyAction(EnemyActionKind.Ability, ability, target.Target));
            }
        }

        actions.Add(new EnemyAction(EnemyActionKind.Defend, null, null));
        actions.Add(new EnemyAction(EnemyActionKind.Step, null, null));
        return actions;
    }

    /// <summary>Scores every legal action of an enemy, in the fixed order of <see cref="LegalActions"/> (D-959, D-960).</summary>
    /// <param name="battle">The battle.</param>
    /// <param name="enemy">The enemy, on the field.</param>
    /// <param name="content">The battle content, which holds the profile of the enemy.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <returns>Each action with its terms and its score.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">A value of the rules or of the state is out of its range (T-2).</exception>
    public static IReadOnlyList<ScoredAction> Score(Battle battle, Combatant enemy, BattleContent content, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(battle);
        ArgumentNullException.ThrowIfNull(enemy);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(context);

        ScoreWeights weights = content.Profile(battle.Group.Entries[enemy.Slot].Profile).Weights;
        List<ScoredAction> scored = [];
        foreach (EnemyAction action in LegalActions(battle, enemy, content))
        {
            ScoreTerms terms = TermsOf(battle, enemy, action, content.Rules, context);
            scored.Add(new ScoredAction(action, terms, Sum(weights, terms)));
        }

        return scored;
    }

    /// <summary>
    /// Chooses the action of an enemy: the highest score, and a draw from the stream of the
    /// evaluator among the actions that tie for it (D-947, D-959).
    /// </summary>
    /// <param name="battle">The battle.</param>
    /// <param name="enemy">The enemy whose turn it is.</param>
    /// <param name="content">The battle content.</param>
    /// <param name="stream">The stream of the evaluator (D-947).</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <returns>The action.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">The stream is not the stream of the evaluator, or the enemy has no legal action (T-2).</exception>
    public static EnemyAction Choose(Battle battle, Combatant enemy, BattleContent content, RandomStream stream, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(context);

        if (stream.Stream != StreamId.Evaluator)
        {
            throw new SimulationException($"a choice of the evaluator from the stream '{stream.Stream}', and a tie draws from the stream of the evaluator alone (D-947, G-4)", context);
        }

        List<EnemyAction> best = [];
        long bestScore = long.MinValue;
        foreach (ScoredAction scored in Score(battle, enemy, content, context))
        {
            if (scored.Score > bestScore)
            {
                best.Clear();
                bestScore = scored.Score;
            }

            if (scored.Score == bestScore)
            {
                best.Add(scored.Action);
            }
        }

        if (best.Count == 0)
        {
            throw new SimulationException($"a turn of {enemy.Target.Describe()}, which has no legal action (D-948, D-962)", context);
        }

        return best.Count == 1 ? best[0] : best[stream.NextInt(best.Count, context)];
    }

    /// <summary>Gives the combatants that one ability reaches: the other side for a strike, and this side for a heal (D-377, D-955).</summary>
    /// <param name="battle">The battle.</param>
    /// <param name="ability">The ability.</param>
    /// <returns>The targets, in slot order.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The ability is neither a strike nor a heal (T-2).</exception>
    public static IReadOnlyList<Combatant> TargetsOf(Battle battle, AbilityRecord ability)
    {
        ArgumentNullException.ThrowIfNull(battle);
        ArgumentNullException.ThrowIfNull(ability);

        return ability switch
        {
            StrikeAbility { Reach: AbilityReach.Melee } => battle.MeleeTargets(BattleSide.Party),
            StrikeAbility => OnField(battle.Party),
            HealAbility => OnField(battle.Enemies),
            _ => throw new ArgumentException($"The ability '{ability.Id.Value}' is neither a strike nor a heal (D-955).", nameof(ability)),
        };
    }

    private static List<Combatant> OnField(IReadOnlyList<Combatant> side)
    {
        List<Combatant> standing = [];
        foreach (Combatant combatant in side)
        {
            if (combatant.Place == CombatantPlace.Field)
            {
                standing.Add(combatant);
            }
        }

        return standing;
    }

    private static long Sum(ScoreWeights weights, ScoreTerms terms)
    {
        // Each weight is at most 10000 and each term far below 10^9, so the sum stays inside a `long` (T-2).
        return checked(
            (weights.Damage * terms.Damage)
            + (weights.Kills * terms.Kills)
            - (weights.Threat * terms.Threat)
            + (weights.Healing * terms.Healing)
            - (weights.Timeline * terms.Timeline)
            + (weights.Row * terms.Row));
    }

    /// <summary>
    /// Gives the terms of one action (D-959, D-960). The action changes a sketch of this side
    /// alone: the row and the defend of the actor, and the health of a healed ally. The reply
    /// reads that sketch.
    /// </summary>
    private static ScoreTerms TermsOf(Battle battle, Combatant enemy, EnemyAction action, BattleRules rules, RunContext context)
    {
        var sketch = new Sketch(enemy);
        long damage = 0;
        long kills = 0;
        long healing = 0;
        int delay;
        Combatant? struck = null;

        switch (action.Kind)
        {
            case EnemyActionKind.Attack:
                struck = battle.At(TargetOf(action, context), context);
                (damage, kills) = Expected(rules, enemy, struck, rules.AttackPower, null, true, context);
                delay = rules.AttackDelay;
                break;
            case EnemyActionKind.Ability when action.Ability is StrikeAbility strike:
                struck = battle.At(TargetOf(action, context), context);
                (damage, kills) = Expected(rules, enemy, struck, strike.Power, strike.Element, strike.Reach == AbilityReach.Melee, context);
                delay = strike.Delay;
                break;
            case EnemyActionKind.Ability when action.Ability is HealAbility heal:
                Combatant ally = battle.At(TargetOf(action, context), context);
                healing = Math.Min(heal.Heal, ally.FullHealth - ally.Health);
                sketch.Healed = ally;
                sketch.HealedAmount = (int)healing;
                delay = heal.Delay;
                break;
            case EnemyActionKind.Defend:
                sketch.ActorDefends = true;
                delay = rules.DefendDelay;
                break;
            case EnemyActionKind.Step:
                sketch.ActorRow = BattleSides.Other(enemy.Row);
                delay = rules.StepDelay;
                break;
            default:
                throw new SimulationException($"a score of the action {action.Describe()}, which names no rule (T-2)", context);
        }

        long row = Unreached(battle, sketch) - Unreached(battle, new Sketch(enemy));
        long threat = Threat(battle, rules, sketch, struck, kills, context);
        long timeline = Battle.Push(delay, enemy.Speed, enemy.PushRate, context);
        return new ScoreTerms(damage, kills, threat, healing, timeline, row);
    }

    /// <summary>
    /// Gives the expected damage and the expected kills of one strike at the middle hit
    /// factor (D-959): the hit chance times the damage, up to the health of the target. A kill
    /// counts the hit chance when the damage reaches that health. An absorb gives the health
    /// that it restores as damage below zero, and no kill (D-795).
    /// </summary>
    private static (long Damage, long Kills) Expected(BattleRules rules, Combatant attacker, Combatant target, int power, Element? element, bool melee, RunContext context)
    {
        long hitChance = BasisPoints.One - BattleMath.MissChance(rules, attacker, target);
        int middle = (rules.HitLow + rules.HitHigh) / 2;
        long hit = BattleMath.Hit(attacker, target, power, middle);
        Affinity affinity = element is Element named ? target.Elements.Of(named) : Affinity.Normal;
        if (affinity == Affinity.Absorb)
        {
            long restored = Math.Min(checked(hit * rules.AbsorbRate) / BasisPoints.One, target.FullHealth - target.Health);
            return (-restored * hitChance / BasisPoints.One, 0);
        }

        int damage = BattleMath.Damage(rules, attacker, target, hit, affinity, element is not null, melee, target.Defending, context);
        long taken = Math.Min(damage, target.Health);
        return (taken * hitChance / BasisPoints.One, damage >= target.Health ? hitChance : 0);
    }

    /// <summary>
    /// Gives the threat of D-960: the expected health that the basic attack of the next
    /// character takes from the enemy that it reaches best. When the strike of the action can
    /// put that character down, the next character after it takes the share of the kill.
    /// </summary>
    private static long Threat(Battle battle, BattleRules rules, Sketch sketch, Combatant? struck, long kills, RunContext context)
    {
        Combatant? next = NextCharacter(battle, null);
        if (next is null)
        {
            return 0;
        }

        long threat = ReplyOf(battle, rules, sketch, next, context);
        if (struck is null || !ReferenceEquals(struck, next) || kills == 0)
        {
            return threat;
        }

        Combatant? after = NextCharacter(battle, next);
        long afterThreat = after is null ? 0 : ReplyOf(battle, rules, sketch, after, context);
        return ((threat * (BasisPoints.One - kills)) + (afterThreat * kills)) / BasisPoints.One;
    }

    /// <summary>Gives the expected health that the best basic attack of one character takes from this side, on the sketch (D-960).</summary>
    private static long ReplyOf(Battle battle, BattleRules rules, Sketch sketch, Combatant character, RunContext context)
    {
        long best = 0;
        foreach (Combatant target in Reached(battle, sketch))
        {
            long hitChance = BasisPoints.One - BattleMath.MissChance(rules, character, target);
            int middle = (rules.HitLow + rules.HitHigh) / 2;
            long hit = BattleMath.Hit(character, target, rules.AttackPower, middle);
            bool defending = ReferenceEquals(target, sketch.Actor) ? sketch.ActorDefends || target.Defending : target.Defending;
            int damage = BattleMath.Damage(rules, character, target, hit, Affinity.Normal, false, true, defending, context);
            int health = ReferenceEquals(target, sketch.Healed) ? target.Health + sketch.HealedAmount : target.Health;
            long expected = Math.Min(damage, health) * hitChance / BasisPoints.One;
            best = Math.Max(best, expected);
        }

        return best;
    }

    /// <summary>
    /// Gives the character of the party whose turn comes first, by the order of D-769 inside
    /// one side: the lower tick, the higher speed, then the lower slot. A skipped character
    /// takes no turn, because the action puts it down.
    /// </summary>
    private static Combatant? NextCharacter(Battle battle, Combatant? skipped)
    {
        Combatant? next = null;
        foreach (Combatant character in battle.Party)
        {
            if (character.Place != CombatantPlace.Field || ReferenceEquals(character, skipped))
            {
                continue;
            }

            if (next is null
                || character.ReadyAt < next.ReadyAt
                || (character.ReadyAt == next.ReadyAt && character.Speed > next.Speed))
            {
                next = character;
            }
        }

        return next;
    }

    /// <summary>Gives the enemies that the melee of the party reaches on the sketch: the front row while anyone stands in it, and the back row after that (D-377).</summary>
    private static List<Combatant> Reached(Battle battle, Sketch sketch)
    {
        List<Combatant> front = [];
        List<Combatant> back = [];
        foreach (Combatant enemy in battle.Enemies)
        {
            if (enemy.Place != CombatantPlace.Field)
            {
                continue;
            }

            BattleRow row = ReferenceEquals(enemy, sketch.Actor) ? sketch.ActorRow : enemy.Row;
            if (row == BattleRow.Front)
            {
                front.Add(enemy);
            }
            else
            {
                back.Add(enemy);
            }
        }

        return front.Count > 0 ? front : back;
    }

    private static long Unreached(Battle battle, Sketch sketch)
    {
        long standing = 0;
        foreach (Combatant enemy in battle.Enemies)
        {
            standing += enemy.Place == CombatantPlace.Field ? 1 : 0;
        }

        return standing - Reached(battle, sketch).Count;
    }

    private static BattleTarget TargetOf(EnemyAction action, RunContext context) =>
        action.Target ?? throw new SimulationException($"the enemy action {action.Describe()} names no target (D-764)", context);

    /// <summary>
    /// The change that one action makes to the enemy side, for the reply (D-960): the row and
    /// the defend of the actor, and the health that a heal restores to one ally.
    /// </summary>
    private sealed class Sketch
    {
        internal Sketch(Combatant actor)
        {
            this.Actor = actor;
            this.ActorRow = actor.Row;
        }

        internal Combatant Actor { get; }

        internal BattleRow ActorRow { get; set; }

        internal bool ActorDefends { get; set; }

        internal Combatant? Healed { get; set; }

        internal int HealedAmount { get; set; }
    }
}
