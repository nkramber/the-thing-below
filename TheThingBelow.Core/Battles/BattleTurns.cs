using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Streams;

namespace TheThingBelow.Core.Battles;

/// <summary>
/// The rules of a battle: the start, each turn, and the end (D-29, D-376, D-532). Core
/// resolves each action the moment that its intent arrives, then runs each enemy turn up to
/// the next turn of a character, and emits the events for Game (D-168, D-532).
/// </summary>
/// <remarks>
/// Every roll draws on the battle stream alone (G-4). A roll runs in one fixed order for each
/// strike: the miss, then the hit factor on a hit (D-772, D-773).
/// <para>
/// A win and a flee end the battle, and the map waits for the wait intent of Game before it
/// runs again (D-522). A wipe ends the run, and Game reloads (D-397, D-776).
/// </para>
/// </remarks>
public static class BattleTurns
{
    /// <summary>Starts the battle of the encounter of the map, and runs each enemy turn before the first turn of a character (D-770).</summary>
    /// <param name="state">The run, whose map holds an encounter and no battle.</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">The map holds no encounter, or a battle runs (T-2).</exception>
    public static void Begin(RunState state, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(log);

        RunContext context = state.Context("battle/begin");
        if (state.Party.Patrols.Encounter is not MapEncounter encounter || state.Battle is not null)
        {
            throw new SimulationException("a start of a battle, and a battle needs an encounter of the map and no battle that runs (D-531)", context);
        }

        Battle battle = Battle.Start(state.BattleContent, encounter, state.Characters, context);
        state.SetBattle(battle);
        state.AddEvent(new BattleEvent(BattleEventKind.Started, new BattleTarget(BattleSide.Enemy, 0), null, 0));
        log.Add(Entry(state, LogLevel.Info, "a battle started", [new LogField("group", battle.Group.Id.Value)]));
        RunUntilCharacter(state, battle, log);
    }

    /// <summary>Resolves the choice of the character whose turn it is, then each enemy turn up to the next turn of a character (D-532).</summary>
    /// <param name="state">The run.</param>
    /// <param name="choice">The choice.</param>
    /// <param name="context">The seed, the tick, and the intent, for an error (T-2).</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">
    /// No battle runs, no character has the turn, or the choice breaks a rule: a target that
    /// melee does not reach, an empty pack, or a flee from a boss (T-2, D-377, D-378).
    /// </exception>
    public static void Act(RunState state, BattleChoice choice, RunContext context, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(choice);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(log);

        if (RefusalOf(state, choice) is string refusal)
        {
            throw new SimulationException($"a battle choice, and the rules refuse it: {refusal}", context);
        }

        Battle battle = RunningBattle(state, context);
        Combatant actor = StartTurn(battle, BattleSide.Party, context);
        BattleRules rules = state.BattleContent.Rules;

        switch (choice.Action)
        {
            case BattleAction.Attack:
                Strike(state, battle, actor, BattleMove.BasicAttack(rules), TargetOf(choice, context), context, log);
                break;
            case BattleAction.Defend:
                actor.Defending = true;
                state.AddEvent(new BattleEvent(BattleEventKind.Defend, actor.Target, null, 0));
                PushBack(actor, rules.DefendDelay, context);
                break;
            case BattleAction.Step:
                actor.Row = BattleSides.Other(actor.Row);
                state.AddEvent(new BattleEvent(BattleEventKind.Step, actor.Target, null, 0));
                PushBack(actor, rules.StepDelay, context);
                break;
            case BattleAction.Item:
                UseItem(state, battle, actor, choice, context);
                break;
            case BattleAction.Flee:
                TryFlee(state, battle, actor, context, log);
                break;
            default:
                throw new SimulationException($"the battle action {choice.Action}, which names no rule (T-2)", context);
        }

        RunUntilCharacter(state, battle, log);
    }

    /// <summary>
    /// Gives the reason that the rules refuse a choice now, or no value when the choice is
    /// legal. The battle screen of PR-10 reads it to offer the legal choices alone (T-2).
    /// </summary>
    /// <param name="state">The run.</param>
    /// <param name="choice">The choice.</param>
    /// <returns>The reason, such as `no battle runs`, or no value.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static string? RefusalOf(RunState state, BattleChoice choice)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(choice);

        if (state.Battle is not Battle battle || battle.Outcome != BattleOutcome.Running)
        {
            return "no battle runs (D-532)";
        }

        if (battle.Next() is not Combatant actor || actor.Side != BattleSide.Party)
        {
            return "no character has the turn (D-532)";
        }

        return choice.Action switch
        {
            BattleAction.Attack => RefusalOfAttack(battle, choice.Target),
            BattleAction.Item => RefusalOfItem(state, battle, choice),
            BattleAction.Flee => battle.Group.Boss ? $"a boss group, '{battle.Group.Id.Value}', which no party flees (D-378)" : null,
            BattleAction.Defend or BattleAction.Step => null,
            _ => $"the action {choice.Action}, which names no rule",
        };
    }

    /// <summary>
    /// Resolves one strike of a move by the character whose turn it is (D-376). The basic
    /// attack is one move, and the lessons of PR-12 add the others, such as a heavy blow.
    /// </summary>
    /// <param name="state">The run.</param>
    /// <param name="move">The move.</param>
    /// <param name="target">The target, which melee must reach (D-377).</param>
    /// <param name="context">The seed, the tick, and the intent, for an error (T-2).</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">No battle runs, no character has the turn, or melee does not reach the target (T-2).</exception>
    public static void StrikeWith(RunState state, BattleMove move, BattleTarget target, RunContext context, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(move);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(log);

        Battle battle = RunningBattle(state, context);
        Combatant actor = StartTurn(battle, BattleSide.Party, context);
        Strike(state, battle, actor, move, target, context, log);
        RunUntilCharacter(state, battle, log);
    }

    /// <summary>Sets the haste or slow of one combatant, which rates each later push (D-376, D-768). The statuses of PR-66 call it.</summary>
    /// <param name="state">The run.</param>
    /// <param name="target">The combatant.</param>
    /// <param name="pace">The pace.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">No battle runs, or the target names no combatant (T-2).</exception>
    public static void SetPace(RunState state, BattleTarget target, BattlePace pace, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);

        Battle battle = RunningBattle(state, context);
        BattleRules rules = state.BattleContent.Rules;
        battle.At(target, context).PushRate = pace switch
        {
            BattlePace.Normal => BasisPoints.One,
            BattlePace.Haste => rules.HasteRate,
            BattlePace.Slow => rules.SlowRate,
            _ => throw new SimulationException($"the pace {pace}, which names no rate (D-768)", context),
        };
    }

    /// <summary>
    /// Ends a battle that the party won or fled, after the screen is done (D-522). A win marks
    /// the map enemy dead, and a flee starts its grace time (D-381, D-748). The map runs again
    /// from the next tick.
    /// </summary>
    /// <param name="state">The run.</param>
    /// <param name="context">The seed, the tick, and the intent, for an error (T-2).</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">No battle ended in a win or a flee (T-2).</exception>
    public static void Finish(RunState state, RunContext context, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(log);

        if (state.Battle is not Battle battle || (battle.Outcome != BattleOutcome.Won && battle.Outcome != BattleOutcome.Fled))
        {
            string outcome = state.Battle is null ? "no battle" : Battle.OutcomeName(state.Battle.Outcome);
            throw new SimulationException(
                $"the wait intent of a battle, and the run holds {outcome}. The wait ends a win or a flee alone, and a wipe reloads (D-522, D-776)",
                context);
        }

        MapPatrols patrols = state.Party.Patrols;
        if (battle.Outcome == BattleOutcome.Won)
        {
            patrols.Defeat();
        }
        else
        {
            _ = patrols.Flee();
        }

        state.SetBattle(null);
        log.Add(Entry(
            state,
            LogLevel.Info,
            "the battle ended and the map runs again",
            [new LogField("enemy", battle.Enemy.Value), new LogField("outcome", Battle.OutcomeName(battle.Outcome))]));
    }

    private static string? RefusalOfAttack(Battle battle, BattleTarget? aimed)
    {
        if (aimed is not BattleTarget target)
        {
            return "an attack that names no target (D-764)";
        }

        if (target.Side != BattleSide.Enemy || target.Slot < 0 || target.Slot >= battle.Enemies.Count)
        {
            return $"the target {target.Describe()}, and an attack aims at an enemy slot from 0 to {battle.Enemies.Count - 1}";
        }

        return Reaches(battle.MeleeTargets(BattleSide.Enemy), battle.Enemies[target.Slot])
            ? null
            : $"the target {target.Describe()}, which melee does not reach (D-377)";
    }

    private static string? RefusalOfItem(RunState state, Battle battle, BattleChoice choice)
    {
        if (choice.Item is not ContentId item)
        {
            return "an item use that names no item (D-780)";
        }

        if (state.Characters.CountOf(item) == 0)
        {
            return $"an item use of '{item.Value}', and the pack holds none (D-775)";
        }

        if (choice.Target is not BattleTarget target || target.Side != BattleSide.Party || target.Slot < 0 || target.Slot >= battle.Party.Count)
        {
            return $"an item use that names no character slot from 0 to {battle.Party.Count - 1} (D-764)";
        }

        return battle.Party[target.Slot].Place == CombatantPlace.Field
            ? null
            : $"an item use on {target.Describe()}, and an item of this build reaches a character who stands (D-36, D-775)";
    }

    private static Battle RunningBattle(RunState state, RunContext context)
    {
        if (state.Battle is not Battle battle)
        {
            throw new SimulationException("a battle action, and no battle runs (D-532)", context);
        }

        if (battle.Outcome != BattleOutcome.Running)
        {
            throw new SimulationException(
                $"a battle action, and the battle ended as '{Battle.OutcomeName(battle.Outcome)}' (D-532)",
                context);
        }

        return battle;
    }

    /// <summary>
    /// Starts the turn of the next combatant: the timeline moves to its tick, and its defend
    /// ends (D-755). The side must match, because Game sends a choice on the turn of a
    /// character alone (D-532).
    /// </summary>
    private static Combatant StartTurn(Battle battle, BattleSide side, RunContext context)
    {
        Combatant actor = battle.Next()
            ?? throw new SimulationException("a turn, and no combatant stands on the field (T-2)", context);
        if (actor.Side != side)
        {
            throw new SimulationException(
                $"a turn of the {BattleSides.NameOf(side)}, and the turn belongs to {actor.Target.Describe()} (D-532)",
                context);
        }

        battle.Now = actor.ReadyAt;
        actor.Defending = false;
        return actor;
    }

    private static void RunUntilCharacter(RunState state, Battle battle, List<LogEntry> log)
    {
        while (battle.Outcome == BattleOutcome.Running)
        {
            Combatant next = battle.Next()
                ?? throw new SimulationException("a turn, and no combatant stands on the field (T-2)", state.Context("battle"));
            if (next.Side == BattleSide.Party)
            {
                state.AddEvent(new BattleEvent(BattleEventKind.Turn, next.Target, null, 0));
                return;
            }

            RunContext context = state.Context($"battle/enemy {next.Slot}");
            Combatant enemy = StartTurn(battle, BattleSide.Enemy, context);
            EnemyTurn(state, battle, enemy, context, log);
        }
    }

    /// <summary>A basic attack on a legal target that the battle stream draws (D-774). The evaluator of PR-11 replaces the draw.</summary>
    private static void EnemyTurn(RunState state, Battle battle, Combatant enemy, RunContext context, List<LogEntry> log)
    {
        IReadOnlyList<Combatant> targets = battle.MeleeTargets(BattleSide.Party);
        Combatant target = targets[state.Stream(StreamId.Battle).NextInt(targets.Count, context)];
        Strike(state, battle, enemy, BattleMove.BasicAttack(state.BattleContent.Rules), target.Target, context, log);
    }

    private static void Strike(
        RunState state,
        Battle battle,
        Combatant attacker,
        BattleMove move,
        BattleTarget aimed,
        RunContext context,
        List<LogEntry> log)
    {
        BattleRules rules = state.BattleContent.Rules;
        BattleSide other = attacker.Side == BattleSide.Party ? BattleSide.Enemy : BattleSide.Party;
        Combatant target = battle.At(aimed, context);
        if (aimed.Side != other || !Reaches(battle.MeleeTargets(other), target))
        {
            throw new SimulationException(
                $"a melee strike of {attacker.Target.Describe()} at {aimed.Describe()}, which melee does not reach (D-377)",
                context);
        }

        RandomStream stream = state.Stream(StreamId.Battle);
        if (stream.NextChance(MissChance(rules, attacker, target), context))
        {
            state.AddEvent(new BattleEvent(BattleEventKind.Miss, attacker.Target, target.Target, 0));
        }
        else
        {
            int factor = stream.NextInt(rules.HitLow, rules.HitHigh, context);
            int damage = Damage(rules, attacker, target, move.Power, factor, context);
            target.Health = Math.Max(0, target.Health - damage);
            state.AddEvent(new BattleEvent(BattleEventKind.Hit, attacker.Target, target.Target, damage));
            if (target.Health == 0)
            {
                FallDown(state, battle, target, context, log);
            }
            else if (move.Stun)
            {
                target.ReadyAt = checked(target.ReadyAt + rules.StunTicks);
            }
        }

        PushBack(attacker, move.Delay, context);
        CheckEnd(state, battle, log);
    }

    /// <summary>
    /// Gives the damage of one hit (D-771, D-772, D-779, D-755): the attack times the power,
    /// times 100 over 100 plus the defense, times the hit factor. A melee attack from the back
    /// row takes the back row rate, and a defend takes the cut. The result is at least 1.
    /// </summary>
    private static int Damage(BattleRules rules, Combatant attacker, Combatant target, int power, int factor, RunContext context)
    {
        // Each factor is at most 100000, so the product stays inside a `long` (T-2).
        long numerator = checked((long)attacker.Attack * power * 100 * factor);
        long denominator = checked((long)BasisPoints.One * (100 + target.Defense) * BasisPoints.One);
        long damage = numerator / denominator;
        if (attacker.Row == BattleRow.Back)
        {
            damage = damage * rules.BackRowRate / BasisPoints.One;
        }

        if (target.Defending)
        {
            damage = damage * (BasisPoints.One - rules.DefendCut) / BasisPoints.One;
        }

        if (damage < 1)
        {
            return 1;
        }

        if (damage > int.MaxValue)
        {
            throw new SimulationException($"a hit of {damage}, which no `int` holds", context);
        }

        return (int)damage;
    }

    /// <summary>The miss chance of D-773: the base, plus the rate for each point that the target is faster, clamped.</summary>
    private static int MissChance(BattleRules rules, Combatant attacker, Combatant target)
    {
        long gap = (long)target.Speed - attacker.Speed;
        return Clamp(rules.MissBase + (gap * rules.MissPerSpeed), rules.MissFloor, rules.MissCeiling);
    }

    /// <summary>The flee chance of D-763: the base, plus the rate for each point of the average speed of the party over the field, clamped.</summary>
    private static int FleeChance(BattleRules rules, Battle battle)
    {
        long gap = AverageSpeed(battle.Party) - AverageSpeed(battle.Enemies);
        return Clamp(rules.FleeBase + (gap * rules.FleePerSpeed), rules.FleeFloor, rules.FleeCeiling);
    }

    private static long AverageSpeed(IReadOnlyList<Combatant> side)
    {
        long sum = 0;
        long count = 0;
        foreach (Combatant combatant in side)
        {
            if (combatant.Place == CombatantPlace.Field)
            {
                sum += combatant.Speed;
                count += 1;
            }
        }

        return count == 0 ? 0 : sum / count;
    }

    private static int Clamp(long value, int floor, int ceiling)
    {
        if (value < floor)
        {
            return floor;
        }

        return value > ceiling ? ceiling : (int)value;
    }

    private static void UseItem(RunState state, Battle battle, Combatant actor, BattleChoice choice, RunContext context)
    {
        ContentId itemId = choice.Item
            ?? throw new SimulationException("an item use that names no item (D-780)", context);
        BattleTarget aimed = TargetOf(choice, context);
        Combatant target = battle.At(aimed, context);
        if (aimed.Side != BattleSide.Party || target.Place != CombatantPlace.Field)
        {
            throw new SimulationException(
                $"an item use on {aimed.Describe()}, and an item of this build reaches a character who stands (D-36, D-775)",
                context);
        }

        ItemRecord item = state.BattleContent.Item(itemId);
        state.Characters.Take(item.Id, context);
        int heal = BasisPoints.Apply(item.Heal, state.BattleContent.Rules.ItemRate, context);
        int restored = Math.Min(heal, target.FullHealth - target.Health);
        target.Health += restored;
        state.AddEvent(new BattleEvent(BattleEventKind.Item, actor.Target, target.Target, restored));
        PushBack(actor, item.Delay, context);
    }

    private static void TryFlee(RunState state, Battle battle, Combatant actor, RunContext context, List<LogEntry> log)
    {
        if (battle.Group.Boss)
        {
            throw new SimulationException(
                $"a flee from the boss group '{battle.Group.Id.Value}', and no party flees from a boss (D-378)",
                context);
        }

        BattleRules rules = state.BattleContent.Rules;
        if (state.Stream(StreamId.Battle).NextChance(FleeChance(rules, battle), context))
        {
            End(state, battle, BattleOutcome.Fled, log);
            state.AddEvent(new BattleEvent(BattleEventKind.Fled, actor.Target, null, 0));
            return;
        }

        state.AddEvent(new BattleEvent(BattleEventKind.FleeFailed, actor.Target, null, 0));
        PushBack(actor, rules.FleeDelay, context);
    }

    /// <summary>
    /// Puts a combatant down (D-36). A fallen enemy lets the next waiting enemy of the group
    /// step into its row, one attack push out (D-761, D-778).
    /// </summary>
    private static void FallDown(RunState state, Battle battle, Combatant fallen, RunContext context, List<LogEntry> log)
    {
        fallen.Place = CombatantPlace.Down;
        fallen.Defending = false;
        state.AddEvent(new BattleEvent(BattleEventKind.Down, fallen.Target, null, 0));
        log.Add(Entry(state, LogLevel.Info, "a combatant went down", [new LogField("combatant", fallen.Target.Describe())]));

        if (fallen.Side != BattleSide.Enemy)
        {
            return;
        }

        foreach (Combatant waiting in battle.Enemies)
        {
            if (waiting.Place != CombatantPlace.Waiting)
            {
                continue;
            }

            waiting.Place = CombatantPlace.Field;
            waiting.ReadyAt = checked(battle.Now + Battle.Push(state.BattleContent.Rules.AttackDelay, waiting.Speed, waiting.PushRate, context));
            state.AddEvent(new BattleEvent(BattleEventKind.StepIn, waiting.Target, null, 0));
            return;
        }
    }

    private static void CheckEnd(RunState state, Battle battle, List<LogEntry> log)
    {
        if (battle.Outcome != BattleOutcome.Running)
        {
            return;
        }

        if (!AnyStands(battle.Enemies, true))
        {
            End(state, battle, BattleOutcome.Won, log);
            state.AddEvent(new BattleEvent(BattleEventKind.Won, new BattleTarget(BattleSide.Party, 0), null, 0));
        }
        else if (!AnyStands(battle.Party, false))
        {
            End(state, battle, BattleOutcome.Wiped, log);
            state.AddEvent(new BattleEvent(BattleEventKind.Wiped, new BattleTarget(BattleSide.Party, 0), null, 0));
        }
    }

    private static bool AnyStands(IReadOnlyList<Combatant> side, bool countWaiting)
    {
        foreach (Combatant combatant in side)
        {
            if (combatant.Place == CombatantPlace.Field || (countWaiting && combatant.Place == CombatantPlace.Waiting))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Ends the battle, and copies the health of each character back to the party (D-36, D-765).</summary>
    private static void End(RunState state, Battle battle, BattleOutcome outcome, List<LogEntry> log)
    {
        battle.Outcome = outcome;
        for (int slot = 0; slot < battle.Party.Count; slot += 1)
        {
            state.Characters.Members[slot].Health = battle.Party[slot].Health;
        }

        log.Add(Entry(state, LogLevel.Info, "a battle ended", [new LogField("outcome", Battle.OutcomeName(outcome))]));
    }

    private static void PushBack(Combatant actor, int delay, RunContext context)
    {
        actor.ReadyAt = checked(actor.ReadyAt + Battle.Push(delay, actor.Speed, actor.PushRate, context));
    }

    private static bool Reaches(IReadOnlyList<Combatant> reachable, Combatant target)
    {
        foreach (Combatant candidate in reachable)
        {
            if (ReferenceEquals(candidate, target))
            {
                return true;
            }
        }

        return false;
    }

    private static BattleTarget TargetOf(BattleChoice choice, RunContext context) =>
        choice.Target ?? throw new SimulationException($"the battle action {choice.Action} names no target (D-764)", context);

    private static LogEntry Entry(RunState state, LogLevel level, string message, LogField[] fields) =>
        new(level, message, state.Tick, LogSubsystems.Battle, fields);
}
