using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Story;
using TheThingBelow.Core.Streams;

namespace TheThingBelow.Core.Battles;

/// <summary>
/// The rules of a battle: the start, each turn, and the end (D-29, D-376, D-532). Core
/// resolves each action the moment that its intent arrives, then runs each enemy turn up to
/// the next turn of a character, and emits the events for Game (D-168, D-532).
/// </summary>
/// <remarks>
/// Every roll draws on the battle stream alone (G-4). A roll runs in one fixed order for each
/// strike: the miss, then the hit factor on a hit, then the status chance of the move on a
/// hit (D-772, D-773, D-807).
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

    /// <summary>
    /// Starts the battle of a start battle step, and runs each enemy turn before the first turn
    /// of a character (D-998, D-770). No side comes from behind, and no patrol of the map takes
    /// part, so the battle names the story scene in place of a patrol.
    /// </summary>
    /// <param name="state">The run, which holds no battle and no encounter.</param>
    /// <param name="scene">The story scene of the step.</param>
    /// <param name="group">The enemy group of the step.</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">A battle or an encounter already runs (T-2).</exception>
    public static void BeginStory(RunState state, ContentId scene, ContentId group, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(scene);
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(log);

        RunContext context = state.Context($"battle/begin/{scene.Value}");
        if (state.Battle is not null || state.Party.Patrols.Encounter is not null)
        {
            throw new SimulationException("a start battle step, and a battle or an encounter already runs (D-531, D-998)", context);
        }

        Battle battle = Battle.Start(state.BattleContent, new MapEncounter(scene, group, EncounterSide.None), state.Characters, context);
        state.SetBattle(battle);
        state.AddEvent(new BattleEvent(BattleEventKind.Started, new BattleTarget(BattleSide.Enemy, 0), null, 0));
        log.Add(Entry(state, LogLevel.Info, "a battle of a story scene started", [new LogField("group", battle.Group.Id.Value), new LogField("scene", scene.Value)]));
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
        Combatant actor = ActorOf(battle, BattleSide.Party, context);
        BattleRules rules = state.BattleContent.Rules;

        switch (choice.Action)
        {
            case BattleAction.Attack:
                Strike(state, battle, actor, BattleMove.BasicAttack(rules), TargetOf(choice, context), AbilityReach.Melee, context, log);
                break;
            case BattleAction.Defend:
                Defend(state, actor, context);
                break;
            case BattleAction.Step:
                Step(state, actor, context);
                break;
            case BattleAction.Item:
                UseItem(state, battle, actor, choice, context);
                break;
            case BattleAction.Flee:
                TryFlee(state, battle, actor, context, log);
                break;
            case BattleAction.Lesson:
                UseLesson(state, battle, actor, choice, context, log);
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
            BattleAction.Flee => RefusalOfFlee(battle),
            BattleAction.Lesson => RefusalOfLesson(state, battle, actor, choice),
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
        Combatant actor = ActorOf(battle, BattleSide.Party, context);
        Strike(state, battle, actor, move, target, AbilityReach.Melee, context, log);
        RunUntilCharacter(state, battle, log);
    }

    /// <summary>
    /// Gives one status to one combatant on the field, with no roll (D-793). PR-12 gives a
    /// status through a move, and the tests and the identity run call this. A stun on the
    /// character whose turn is open is an error, because no strike reaches that character.
    /// </summary>
    /// <param name="state">The run.</param>
    /// <param name="target">The combatant.</param>
    /// <param name="status">The status.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">No battle runs, or the target names no combatant on the field, or a stun names the character whose turn is open (T-2).</exception>
    public static void GiveStatus(RunState state, BattleTarget target, StatusKind status, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);

        Battle battle = RunningBattle(state, context);
        Combatant holder = battle.At(target, context);
        if (holder.Place != CombatantPlace.Field)
        {
            throw new SimulationException(
                $"a status '{Statuses.NameOf(status)}' for {target.Describe()}, which is not on the field (D-801)",
                context);
        }

        // No strike reaches the character whose turn is open: enemies act while no turn of a
        // character is open, and no move strikes its user. A stun there would push a turn that
        // already began, and its shares would act two times (D-799, D-802).
        if (status == StatusKind.Stun && ReferenceEquals(battle.Next(), holder) && holder.Side == BattleSide.Party)
        {
            throw new SimulationException(
                $"a stun for {target.Describe()}, whose turn is open, and no strike reaches the combatant whose turn is open (D-799, D-802)",
                context);
        }

        Give(state, battle, holder, status, context);
    }

    /// <summary>
    /// Plays one action of an enemy on the field: the choice of the evaluator, and its effect
    /// with its events and the end check (D-65, D-955). The rules call the same code for the
    /// enemy whose turn begins. The `evaluator-cost` command of Tools times this call on a copy
    /// of a run, so the limit of D-961 reads the whole turn.
    /// </summary>
    /// <param name="state">The run, whose battle runs.</param>
    /// <param name="enemy">The side and the slot of the enemy.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">No battle runs, or the target names no enemy on the field (T-2).</exception>
    public static void EnemyAct(RunState state, BattleTarget enemy, RunContext context, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(log);

        Battle battle = RunningBattle(state, context);
        Combatant actor = battle.At(enemy, context);
        if (enemy.Side != BattleSide.Enemy || actor.Place != CombatantPlace.Field)
        {
            throw new SimulationException($"an enemy action of {enemy.Describe()}, which is no enemy on the field (D-65)", context);
        }

        EnemyTurn(state, battle, actor, context, log);
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

        if (battle.FromStoryScene)
        {
            // A battle of a story scene has no patrol, and its story scene goes on at the step
            // after the start battle step (D-999).
            state.SetBattle(null);
            StoryRules.FinishBattle(state, context);
            log.Add(Entry(
                state,
                LogLevel.Info,
                "the battle of a story scene ended and the story scene goes on",
                [new LogField("scene", battle.Enemy.Value), new LogField("outcome", Battle.OutcomeName(battle.Outcome))]));
            return;
        }

        MapPatrols patrols = state.Party.Patrols;
        if (battle.Outcome == BattleOutcome.Won)
        {
            patrols.Defeat();

            // The next world step reads the battle end triggers of this patrol (D-1011).
            state.Story.NoteWin(battle.Enemy);
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

        if (string.CompareOrdinal(item.Kind, ItemList.Kind) != 0 || state.BattleContent.Item(item) is not UsedUpItem used)
        {
            return $"an item use of '{item.Value}', which is no used-up item (D-384, D-1038)";
        }

        if (choice.Target is not BattleTarget target || target.Side != BattleSide.Party || target.Slot < 0 || target.Slot >= battle.Party.Count)
        {
            return $"an item use that names no character slot from 0 to {battle.Party.Count - 1} (D-764)";
        }

        // A revive reaches a fallen character, and every other item a character who stands.
        // A use that changes nothing still goes through in a fight (D-36, D-1046, D-1049).
        CombatantPlace place = battle.Party[target.Slot].Place;
        if (used is ReviveItem)
        {
            return place == CombatantPlace.Down ? null : $"a revive on {target.Describe()}, who stands (D-36, D-1046)";
        }

        return place == CombatantPlace.Field
            ? null
            : $"an item use on {target.Describe()}, and '{item.Value}' reaches a character who stands (D-36, D-1046)";
    }

    /// <summary>
    /// Gives the reason that the rules refuse a lesson use: the lesson rules of the form, then
    /// the target of its effect. A strike aims at an enemy that its reach reaches, and a heal, a
    /// cure, and a boon aim at a character on the field (D-377, D-955, D-1029).
    /// </summary>
    private static string? RefusalOfLesson(RunState state, Battle battle, Combatant actor, BattleChoice choice)
    {
        if (choice.Lesson is not ContentId lesson || choice.Form is not int form)
        {
            return "a lesson use that names no lesson or no form (D-1027)";
        }

        bool silenced = actor.Statuses.Holds(StatusKind.Silence);
        if (LessonRules.RefusalOfForm(state, actor.Slot, lesson, form, silenced) is string refusal)
        {
            return refusal;
        }

        if (choice.Target is not BattleTarget target)
        {
            return "a lesson use that names no target (D-764)";
        }

        AbilityRecord ability = LessonRules.FormAbility(state, lesson, form);
        if (ability is StealAbility)
        {
            if (LootRules.RefusalOfSteal(battle) is string spent)
            {
                return spent;
            }

            return target.Side == BattleSide.Enemy && target.Slot >= 0 && target.Slot < battle.Enemies.Count && battle.Enemies[target.Slot].Place == CombatantPlace.Field
                ? null
                : $"the target {target.Describe()}, and a steal aims at an enemy on the field (D-383, D-1044)";
        }

        if (ability is StrikeAbility strike)
        {
            if (target.Side != BattleSide.Enemy || target.Slot < 0 || target.Slot >= battle.Enemies.Count)
            {
                return $"the target {target.Describe()}, and a strike aims at an enemy slot from 0 to {battle.Enemies.Count - 1}";
            }

            Combatant enemy = battle.Enemies[target.Slot];
            bool reached = strike.Reach == AbilityReach.Melee ? Reaches(battle.MeleeTargets(BattleSide.Enemy), enemy) : enemy.Place == CombatantPlace.Field;
            return reached ? null : $"the target {target.Describe()}, which the reach of '{ability.Id.Value}' does not reach (D-377, D-955)";
        }

        if (target.Side != BattleSide.Party || target.Slot < 0 || target.Slot >= battle.Party.Count)
        {
            return $"the target {target.Describe()}, and '{ability.Id.Value}' aims at a character slot from 0 to {battle.Party.Count - 1} (D-1029)";
        }

        return battle.Party[target.Slot].Place == CombatantPlace.Field
            ? null
            : $"the target {target.Describe()}, and '{ability.Id.Value}' reaches a character who stands (D-36, D-1029)";
    }

    /// <summary>
    /// Uses one form of a lesson (D-1027). The character spends the MP, and the effect takes the
    /// aptitude bonus: the power and the status chance of a strike, and the health of a heal
    /// (D-1028). A cure ends its statuses on the target, and a boon gives its status (D-1029).
    /// </summary>
    private static void UseLesson(RunState state, Battle battle, Combatant actor, BattleChoice choice, RunContext context, List<LogEntry> log)
    {
        ContentId lesson = choice.Lesson ?? throw new SimulationException("a lesson use that names no lesson (D-1027)", context);
        int form = choice.Form ?? throw new SimulationException("a lesson use that names no form (D-1027)", context);
        BattleTarget aimed = TargetOf(choice, context);
        PartyMember member = state.Characters.Members[actor.Slot];
        LessonRecord record = state.BattleContent.Lessons.Lesson(lesson);
        AbilityRecord ability = LessonRules.FormAbility(state, lesson, form);
        int cost = record.Forms[form].Mp;
        member.Mp -= cost;
        state.AddEvent(new BattleEvent(BattleEventKind.Lesson, actor.Target, aimed, cost, null, Affinity.Normal, ability.Id));

        int rate = BasisPoints.One + LessonRules.BonusOf(member.Record, record.Kind, state.BattleContent.Rules, state.Story.Flags);
        switch (ability)
        {
            case StrikeAbility strike:
                StatusChance? status = strike.Status is StatusChance given
                    ? given with { Chance = LessonRules.RaisedChance(given.Chance, rate, context) }
                    : null;
                BattleMove move = new(strike.Delay, BasisPoints.Apply(strike.Power, rate, context), strike.Stat, strike.Element, status);
                Strike(state, battle, actor, move, aimed, strike.Reach, context, log);
                break;
            case HealAbility heal:
                Heal(state, battle, actor, heal, rate, aimed, context);
                break;
            case CureAbility cure:
                Cure(state, battle, actor, cure, aimed, context);
                break;
            case BoonAbility boon:
                Give(state, battle, battle.At(aimed, context), boon.Status, context);
                PushBack(actor, boon.Delay, context);
                break;
            case StealAbility steal:
                LootRules.Steal(state, battle, actor, aimed, context);
                PushBack(actor, steal.Delay, context);
                break;
            default:
                throw new SimulationException($"the form '{ability.Id.Value}', whose effect names no rule (T-2)", context);
        }
    }

    /// <summary>A cure of an ally on the field: each status of the cure that the ally holds ends (D-1029).</summary>
    private static void Cure(RunState state, Battle battle, Combatant healer, CureAbility cure, BattleTarget aimed, RunContext context)
    {
        Combatant target = battle.At(aimed, context);
        foreach (StatusKind status in cure.Statuses)
        {
            if (target.Statuses.Holds(status))
            {
                target.Statuses.Remove(status);
                state.AddEvent(new BattleEvent(BattleEventKind.StatusOff, target.Target, null, 0, status));
            }
        }

        target.PushRate = Battle.PushRateOf(target, state.BattleContent.Rules);
        PushBack(healer, cure.Delay, context);
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
    /// Gives the combatant whose turn is open. <see cref="RunUntilCharacter"/> began that turn,
    /// so the timeline stands at its tick. The side must match, because Game sends a choice on
    /// the turn of a character alone (D-532).
    /// </summary>
    private static Combatant ActorOf(Battle battle, BattleSide side, RunContext context)
    {
        Combatant actor = battle.Next()
            ?? throw new SimulationException("a turn, and no combatant stands on the field (T-2)", context);
        if (actor.Side != side)
        {
            throw new SimulationException(
                $"a turn of the {BattleSides.NameOf(side)}, and the turn belongs to {actor.Target.Describe()} (D-532)",
                context);
        }

        return actor;
    }

    /// <summary>
    /// Runs each turn up to the turn of a character. Each turn begins before its actor acts,
    /// so a character whose turn is open holds the next turn, and a battle at rest always
    /// waits on the choice of that character (D-532).
    /// </summary>
    private static void RunUntilCharacter(RunState state, Battle battle, List<LogEntry> log)
    {
        while (battle.Outcome == BattleOutcome.Running)
        {
            Combatant next = battle.Next()
                ?? throw new SimulationException("a turn, and no combatant stands on the field (T-2)", state.Context("battle"));
            RunContext context = state.Context($"battle/{next.Target.Describe()}");
            if (!BeginTurn(state, battle, next, context, log))
            {
                continue;
            }

            if (next.Side == BattleSide.Party)
            {
                state.AddEvent(new BattleEvent(BattleEventKind.Turn, next.Target, null, 0));
                return;
            }

            EnemyTurn(state, battle, next, context, log);
        }
    }

    /// <summary>
    /// Begins the turn of the next combatant (D-755, D-798, D-799, D-802). The timeline moves
    /// to its tick, each status whose end the timeline reached ends, and the defend of the
    /// actor ends. Then poison, bleed, and regen act, in the order of D-75. A sleeper passes
    /// its turn with one attack push.
    /// </summary>
    /// <returns>True when the actor acts, and false when a share put it down or it sleeps.</returns>
    private static bool BeginTurn(RunState state, Battle battle, Combatant actor, RunContext context, List<LogEntry> log)
    {
        BattleRules rules = state.BattleContent.Rules;
        battle.Now = actor.ReadyAt;
        EndStatuses(state, battle);
        actor.Defending = false;

        TakeShare(state, battle, actor, StatusKind.Poison, rules.PoisonShare, context, log);
        TakeShare(state, battle, actor, StatusKind.Bleed, rules.BleedShare, context, log);
        if (actor.Place == CombatantPlace.Field && actor.Statuses.Holds(StatusKind.Regen))
        {
            int heal = ShareOf(actor, rules.RegenShare, context);
            int restored = Math.Min(heal, actor.FullHealth - actor.Health);
            actor.Health += restored;
            state.AddEvent(new BattleEvent(BattleEventKind.StatusHeal, actor.Target, null, restored, StatusKind.Regen));
        }

        if (actor.Place != CombatantPlace.Field)
        {
            CheckEnd(state, battle, context, log);
            return false;
        }

        if (actor.Statuses.Holds(StatusKind.Sleep))
        {
            state.AddEvent(new BattleEvent(BattleEventKind.Asleep, actor.Target, null, 0, StatusKind.Sleep));
            PushBack(actor, rules.AttackDelay, context);
            return false;
        }

        return true;
    }

    /// <summary>Ends each timed status whose end the timeline reached, for every combatant, in slot order and in the order of D-75 (D-798, G-4).</summary>
    private static void EndStatuses(RunState state, Battle battle)
    {
        BattleRules rules = state.BattleContent.Rules;
        foreach (Combatant combatant in battle.All())
        {
            foreach (StatusKind status in Statuses.All)
            {
                if (combatant.Statuses.EndOf(status) is long ends && ends <= battle.Now)
                {
                    combatant.Statuses.Remove(status);
                    combatant.PushRate = Battle.PushRateOf(combatant, rules);
                    state.AddEvent(new BattleEvent(BattleEventKind.StatusOff, combatant.Target, null, 0, status));
                }
            }
        }
    }

    /// <summary>Takes the share of poison or bleed from the actor at the start of its turn, at least 1 (D-799, D-803).</summary>
    private static void TakeShare(RunState state, Battle battle, Combatant actor, StatusKind status, int share, RunContext context, List<LogEntry> log)
    {
        if (actor.Place != CombatantPlace.Field || !actor.Statuses.Holds(status))
        {
            return;
        }

        int damage = Math.Min(ShareOf(actor, share, context), actor.Health);
        actor.Health -= damage;
        state.AddEvent(new BattleEvent(BattleEventKind.StatusHurt, actor.Target, null, damage, status));
        if (actor.Health == 0)
        {
            FallDown(state, battle, actor, context, log);
        }
    }

    /// <summary>Gives a share of the full health of a combatant, at least 1 (D-808).</summary>
    private static int ShareOf(Combatant combatant, int share, RunContext context)
    {
        int amount = BasisPoints.Apply(combatant.FullHealth, share, context);
        return amount < 1 ? 1 : amount;
    }

    /// <summary>
    /// Puts one status on a combatant on the field (D-798, D-800, D-805, D-810). An enemy
    /// that refuses the status takes nothing. Haste on a slowed holder removes the slow and
    /// does not land, and slow on a hasted holder does the same. A stun on a holder with no
    /// stun pushes its next turn.
    /// </summary>
    private static void Give(RunState state, Battle battle, Combatant holder, StatusKind status, RunContext context)
    {
        BattleRules rules = state.BattleContent.Rules;
        if (Statuses.Refuses(holder.Immune, status))
        {
            state.AddEvent(new BattleEvent(BattleEventKind.Immune, holder.Target, null, 0, status));
            return;
        }

        StatusKind? opposite = status switch
        {
            StatusKind.Haste => StatusKind.Slow,
            StatusKind.Slow => StatusKind.Haste,
            _ => null,
        };
        if (opposite is StatusKind cancelled && holder.Statuses.Holds(cancelled))
        {
            holder.Statuses.Remove(cancelled);
            holder.PushRate = Battle.PushRateOf(holder, rules);
            state.AddEvent(new BattleEvent(BattleEventKind.StatusOff, holder.Target, null, 0, cancelled));
            return;
        }

        bool stunned = holder.Statuses.Holds(StatusKind.Stun);
        long? ends = Statuses.Lasts(status) ? null : checked(battle.Now + TicksOf(rules, status, context));
        holder.Statuses.Put(status, ends);
        holder.PushRate = Battle.PushRateOf(holder, rules);
        state.AddEvent(new BattleEvent(BattleEventKind.StatusOn, holder.Target, null, 0, status));
        if (status == StatusKind.Stun && !stunned)
        {
            holder.ReadyAt = checked(holder.ReadyAt + rules.StunPush);
        }
    }

    /// <summary>Gives the ticks that one timed status lasts (D-808).</summary>
    private static int TicksOf(BattleRules rules, StatusKind status, RunContext context) => status switch
    {
        StatusKind.Sleep => rules.SleepTicks,
        StatusKind.Slow => rules.SlowTicks,
        StatusKind.Haste => rules.HasteTicks,
        StatusKind.Stun => rules.StunTicks,
        StatusKind.Bleed => rules.BleedTicks,
        StatusKind.Regen => rules.RegenTicks,
        StatusKind.Shell => rules.ShellTicks,
        _ => throw new SimulationException($"the ticks of the status '{Statuses.NameOf(status)}', which lasts until a cure (D-390)", context),
    };

    /// <summary>
    /// The turn of an enemy: the action that the evaluator chooses (D-65, D-534, D-947). The
    /// evaluator draws a tie from its own stream, and each roll of the action draws from the
    /// battle stream (G-4).
    /// </summary>
    private static void EnemyTurn(RunState state, Battle battle, Combatant enemy, RunContext context, List<LogEntry> log)
    {
        BattleContent content = state.BattleContent;
        EnemyAction action = BattleEvaluator.Choose(battle, enemy, content, state.Stream(StreamId.Evaluator), context);
        switch (action.Kind)
        {
            case EnemyActionKind.Attack:
                Strike(state, battle, enemy, BattleMove.BasicAttack(content.Rules), EnemyTargetOf(action, context), AbilityReach.Melee, context, log);
                break;
            case EnemyActionKind.Ability when action.Ability is StrikeAbility strike:
                BattleMove move = new(strike.Delay, strike.Power, strike.Stat, strike.Element, strike.Status);
                Strike(state, battle, enemy, move, EnemyTargetOf(action, context), strike.Reach, context, log);
                break;
            case EnemyActionKind.Ability when action.Ability is HealAbility heal:
                Heal(state, battle, enemy, heal, BasisPoints.One, EnemyTargetOf(action, context), context);
                break;
            case EnemyActionKind.Defend:
                Defend(state, enemy, context);
                break;
            case EnemyActionKind.Step:
                Step(state, enemy, context);
                break;
            default:
                throw new SimulationException($"the enemy action {action.Describe()}, which names no rule (T-2)", context);
        }
    }

    /// <summary>A defend, which cuts damage until the next turn of the actor (D-755).</summary>
    private static void Defend(RunState state, Combatant actor, RunContext context)
    {
        actor.Defending = true;
        state.AddEvent(new BattleEvent(BattleEventKind.Defend, actor.Target, null, 0));
        PushBack(actor, state.BattleContent.Rules.DefendDelay, context);
    }

    /// <summary>A step to the other row (D-380).</summary>
    private static void Step(RunState state, Combatant actor, RunContext context)
    {
        actor.Row = BattleSides.Other(actor.Row);
        state.AddEvent(new BattleEvent(BattleEventKind.Step, actor.Target, null, 0));
        PushBack(actor, state.BattleContent.Rules.StepDelay, context);
    }

    /// <summary>
    /// A heal of an ally on the field, up to its full health. A heal never misses, and it draws
    /// the hit factor on the battle stream (D-955, D-1057, D-1058).
    /// </summary>
    private static void Heal(RunState state, Battle battle, Combatant healer, HealAbility heal, int rate, BattleTarget aimed, RunContext context)
    {
        Combatant target = battle.At(aimed, context);
        if (aimed.Side != healer.Side || target.Place != CombatantPlace.Field)
        {
            throw new SimulationException(
                $"a heal of {healer.Target.Describe()} on {aimed.Describe()}, and a heal reaches an ally on the field (D-955)",
                context);
        }

        BattleRules rules = state.BattleContent.Rules;
        int factor = state.Stream(StreamId.Battle).NextInt(rules.HitLow, rules.HitHigh, context);
        int amount = BattleMath.HealAmount(heal, healer.Magic, rate, factor, context);
        int restored = Math.Min(amount, target.FullHealth - target.Health);
        target.Health += restored;
        state.AddEvent(new BattleEvent(BattleEventKind.Heal, healer.Target, target.Target, restored));
        PushBack(healer, heal.Delay, context);
    }

    private static BattleTarget EnemyTargetOf(EnemyAction action, RunContext context) =>
        action.Target ?? throw new SimulationException($"the enemy action {action.Describe()} names no target (D-764)", context);

    /// <summary>
    /// Resolves one strike (D-376). A melee strike reaches the targets of D-377, and a strike
    /// of any reach reaches each combatant of the other side on the field (D-955).
    /// </summary>
    private static void Strike(
        RunState state,
        Battle battle,
        Combatant attacker,
        BattleMove move,
        BattleTarget aimed,
        AbilityReach reach,
        RunContext context,
        List<LogEntry> log)
    {
        BattleRules rules = state.BattleContent.Rules;
        BattleSide other = attacker.Side == BattleSide.Party ? BattleSide.Enemy : BattleSide.Party;
        Combatant target = battle.At(aimed, context);
        bool melee = reach == AbilityReach.Melee;
        bool reached = melee ? Reaches(battle.MeleeTargets(other), target) : target.Place == CombatantPlace.Field;
        if (aimed.Side != other || !reached)
        {
            throw new SimulationException(
                $"a strike of {attacker.Target.Describe()} at {aimed.Describe()}, which a strike of the reach '{(melee ? "melee" : "any")}' does not reach (D-377, D-955)",
                context);
        }

        RandomStream stream = state.Stream(StreamId.Battle);
        if (stream.NextChance(BattleMath.MissChance(rules, attacker, target), context))
        {
            state.AddEvent(new BattleEvent(BattleEventKind.Miss, attacker.Target, target.Target, 0));
        }
        else
        {
            int factor = stream.NextInt(rules.HitLow, rules.HitHigh, context);
            long hit = BattleMath.Hit(attacker, target, move.Stat, move.Power, factor);
            Affinity affinity = move.Element is Element element ? target.Elements.Of(element) : Affinity.Normal;
            if (affinity == Affinity.Absorb)
            {
                // D-795, D-809, and D-1055: an absorb heals the hit times the absorb rate, and no cut applies.
                int heal = BattleMath.AbsorbHeal(rules, hit, context);
                int restored = Math.Min(heal, target.FullHealth - target.Health);
                target.Health += restored;
                state.AddEvent(new BattleEvent(BattleEventKind.Absorb, attacker.Target, target.Target, restored, null, affinity));
            }
            else
            {
                int damage = BattleMath.Damage(rules, attacker, target, hit, affinity, move.Element is not null, melee, target.Defending, context);
                target.Health = Math.Max(0, target.Health - damage);
                state.AddEvent(new BattleEvent(BattleEventKind.Hit, attacker.Target, target.Target, damage, null, affinity));
                if (target.Health == 0)
                {
                    FallDown(state, battle, target, context, log);
                }
                else if (target.Statuses.Holds(StatusKind.Sleep))
                {
                    // D-802: the damage of a strike wakes a sleeper.
                    target.Statuses.Remove(StatusKind.Sleep);
                    state.AddEvent(new BattleEvent(BattleEventKind.StatusOff, target.Target, null, 0, StatusKind.Sleep));
                }
            }

            RollStatus(state, battle, target, move, stream, context);
        }

        PushBack(attacker, move.Delay, context);
        CheckEnd(state, battle, context, log);
    }

    /// <summary>
    /// Rolls the status of a move after a hit, on the battle stream (D-807). A move with no
    /// status, a target that went down, and an enemy that refuses the status draw no roll.
    /// </summary>
    private static void RollStatus(RunState state, Battle battle, Combatant target, BattleMove move, RandomStream stream, RunContext context)
    {
        if (move.Status is not StatusChance chance || target.Place != CombatantPlace.Field)
        {
            return;
        }

        if (Statuses.Refuses(target.Immune, chance.Status))
        {
            state.AddEvent(new BattleEvent(BattleEventKind.Immune, target.Target, null, 0, chance.Status));
            return;
        }

        if (stream.NextChance(chance.Chance, context))
        {
            Give(state, battle, target, chance.Status, context);
        }
    }

    /// <summary>The flee chance of D-763: the base, plus the rate for each point of the average speed of the party over the field, clamped.</summary>
    private static int FleeChance(BattleRules rules, Battle battle)
    {
        long gap = AverageSpeed(battle.Party) - AverageSpeed(battle.Enemies);
        return BattleMath.Clamp(rules.FleeBase + (gap * rules.FleePerSpeed), rules.FleeFloor, rules.FleeCeiling);
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

    private static void UseItem(RunState state, Battle battle, Combatant actor, BattleChoice choice, RunContext context)
    {
        ContentId itemId = choice.Item
            ?? throw new SimulationException("an item use that names no item (D-780)", context);
        BattleTarget aimed = TargetOf(choice, context);
        Combatant target = battle.At(aimed, context);
        if (state.BattleContent.Item(itemId) is not UsedUpItem item)
        {
            throw new SimulationException($"an item use of '{itemId.Value}', which is no used-up item (D-384, D-1038)", context);
        }

        bool reaches = aimed.Side == BattleSide.Party &&
            target.Place == (item is ReviveItem ? CombatantPlace.Down : CombatantPlace.Field);
        if (!reaches)
        {
            throw new SimulationException($"an item use of '{itemId.Value}' on {aimed.Describe()}, which the item does not reach (D-36, D-1046)", context);
        }

        // The item rate cuts each amount in a fight, and never a cure (D-382, D-1046).
        BattleRules rules = state.BattleContent.Rules;
        state.Characters.Take(item.Id, context);
        switch (item)
        {
            case HealItem heal:
                int restored = Math.Min(BasisPoints.Apply(heal.Amount, rules.ItemRate, context), target.FullHealth - target.Health);
                target.Health += restored;
                state.AddEvent(new BattleEvent(BattleEventKind.Item, actor.Target, target.Target, restored, null, Affinity.Normal, item.Id));
                break;
            case RestoreItem restore:
                PartyMember member = state.Characters.Members[aimed.Slot];
                int mp = Math.Min(BasisPoints.Apply(restore.Amount, rules.ItemRate, context), member.Stats.Mp - member.Mp);
                member.Mp += mp;
                state.AddEvent(new BattleEvent(BattleEventKind.ItemMp, actor.Target, target.Target, mp, null, Affinity.Normal, item.Id));
                break;
            case CureItem cure:
                state.AddEvent(new BattleEvent(BattleEventKind.ItemCure, actor.Target, target.Target, 0, null, Affinity.Normal, item.Id));
                foreach (StatusKind status in cure.Statuses)
                {
                    if (target.Statuses.Holds(status))
                    {
                        target.Statuses.Remove(status);
                        state.AddEvent(new BattleEvent(BattleEventKind.StatusOff, target.Target, null, 0, status));
                    }
                }

                target.PushRate = Battle.PushRateOf(target, rules);
                break;
            case ReviveItem revive:
                // A revive always stands the ally up, so the cut keeps at least 1 health (D-36).
                int health = Math.Min(Math.Max(1, BasisPoints.Apply(revive.Amount, rules.ItemRate, context)), target.FullHealth);
                target.Health = health;
                target.Place = CombatantPlace.Field;
                target.ReadyAt = checked(battle.Now + Battle.Push(rules.AttackDelay, target.Speed, target.PushRate, context));
                state.AddEvent(new BattleEvent(BattleEventKind.Revive, actor.Target, target.Target, health, null, Affinity.Normal, item.Id));
                break;
            default:
                throw new SimulationException($"the item '{item.Id.Value}', whose effect names no rule (T-2)", context);
        }

        PushBack(actor, item.Delay, context);
    }

    /// <summary>Gives the reason that no party flees this battle: a boss, or a battle of a story scene (D-378, D-1008).</summary>
    private static string? RefusalOfFlee(Battle battle)
    {
        if (battle.Group.Boss)
        {
            return $"a boss group, '{battle.Group.Id.Value}', which no party flees (D-378)";
        }

        return battle.FromStoryScene
            ? $"the battle of the story scene '{battle.Enemy.Value}', which no party flees (D-1008)"
            : null;
    }

    private static void TryFlee(RunState state, Battle battle, Combatant actor, RunContext context, List<LogEntry> log)
    {
        if (RefusalOfFlee(battle) is string refusal)
        {
            throw new SimulationException($"a flee from {refusal}", context);
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
    /// Puts a combatant down (D-36), and takes every status off it (D-801). A fallen enemy
    /// lets the next waiting enemy of the group step into its row, one attack push out
    /// (D-761, D-778).
    /// </summary>
    private static void FallDown(RunState state, Battle battle, Combatant fallen, RunContext context, List<LogEntry> log)
    {
        fallen.Place = CombatantPlace.Down;
        fallen.Defending = false;
        foreach (StatusKind status in Statuses.All)
        {
            fallen.Statuses.Remove(status);
        }

        fallen.PushRate = BasisPoints.One;
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

    private static void CheckEnd(RunState state, Battle battle, RunContext context, List<LogEntry> log)
    {
        if (battle.Outcome != BattleOutcome.Running)
        {
            return;
        }

        if (!AnyStands(battle.Enemies, true))
        {
            End(state, battle, BattleOutcome.Won, log);
            state.AddEvent(new BattleEvent(BattleEventKind.Won, new BattleTarget(BattleSide.Party, 0), null, 0));

            // The experience follows the win, so the victory sting plays before a level-up (D-422, D-975).
            Experience.Award(state, battle, context);

            // The points of each lesson follow the experience, and a new form shows after a level-up (D-1019).
            LessonRules.Award(state, battle);

            // The drops follow the summary as message lines (D-975, D-1042).
            LootRules.Drop(state, battle, context);
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

    /// <summary>
    /// Ends the battle, and copies the health of each character back to the party (D-36,
    /// D-765). Poison, blind, and silence go back with it, and every other status ends with
    /// the fight (D-390, D-792).
    /// </summary>
    private static void End(RunState state, Battle battle, BattleOutcome outcome, List<LogEntry> log)
    {
        battle.Outcome = outcome;
        for (int slot = 0; slot < battle.Party.Count; slot += 1)
        {
            Combatant character = battle.Party[slot];
            List<StatusKind> lasting = [];
            foreach (StatusKind status in Statuses.All)
            {
                if (Statuses.Lasts(status) && character.Statuses.Holds(status))
                {
                    lasting.Add(status);
                }
            }

            state.Characters.Members[slot].Health = character.Health;
            state.Characters.Members[slot].Statuses = lasting;
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
