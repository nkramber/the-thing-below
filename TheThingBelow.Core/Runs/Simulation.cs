using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Notices;

namespace TheThingBelow.Core.Runs;

/// <summary>
/// One run of the rules. The host calls <see cref="Step"/> 60 times a second on a fixed
/// step, and Core counts the ticks and reads no clock (D-164, G-3).
/// </summary>
/// <remarks>
/// A step applies the intents of its tick in the order of the list, and then it runs the
/// world (D-168). The tick rises on every step, a step with a menu open included, so the
/// tick is the one time line of a run and no intent needs a second order value (D-650).
/// <para>
/// The four step intents move the party one tile, and the world step of the same tick starts
/// that step (D-493, D-716). The confirm intent and the cancel intent reach no rule of this
/// build, and PR-16 gives them the door, the chest, and the save point of a map (D-493).
/// </para>
/// <para>
/// The row intent of the party window moves one character to the other row while a menu is
/// open, and the next battle starts the character there (D-558).
/// </para>
/// <para>
/// A battle intent resolves the turn of a character at once, with each enemy turn up to the
/// next turn of a character (D-532). The events wait in the run until the host takes them
/// with <see cref="TakeBattleEvents"/>. The wait intent of a battle ends a win or a flee,
/// and the map runs again from the next tick (D-522).
/// </para>
/// <para>
/// A debug intent goes to the handlers that the host passed at the start. A host with no
/// handler for that action refuses the intent, and the report names the intent and the tick
/// (D-171, D-260, D-492, T-2).
/// </para>
/// <para>
/// A step returns the log entries of that step, and Core keeps none of them. Core adds no
/// wall-clock time and no file path to an entry, and Game writes each entry to the log file
/// with the time of the host (D-179, G-1, G-3).
/// </para>
/// </remarks>
public sealed class Simulation
{
    private readonly DebugIntentHandlers debugHandlers;

    private Simulation(RunState state, DebugIntentHandlers debugHandlers)
    {
        this.State = state;
        this.debugHandlers = debugHandlers;
    }

    /// <summary>The state of the run.</summary>
    public RunState State { get; }

    /// <summary>The count of ticks since the start of the run (D-164).</summary>
    public long Tick => this.State.Tick;

    /// <summary>Starts a run at tick zero, on one map.</summary>
    /// <param name="seed">The seed of the run (G-3, G-4).</param>
    /// <param name="map">The map that the run opens, with the party on its spawn point (D-528).</param>
    /// <param name="battleContent">The battle rules and the fixture, which hold every group that the map names (D-766).</param>
    /// <param name="notices">The notice file of this build (D-989).</param>
    /// <param name="debugHandlers">
    /// The extra intent handlers of the host. A release build passes
    /// <see cref="DebugIntentHandlers.None"/> (D-260, D-492).
    /// </param>
    /// <returns>The run.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static Simulation Start(ulong seed, GameMap map, BattleContent battleContent, NoticeList notices, DebugIntentHandlers debugHandlers)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(battleContent);
        ArgumentNullException.ThrowIfNull(notices);
        ArgumentNullException.ThrowIfNull(debugHandlers);

        return new Simulation(RunState.Start(seed, map, battleContent, notices), debugHandlers);
    }

    /// <summary>Starts a run again from a snapshot (D-651).</summary>
    /// <param name="seed">The seed of the run, which the record header holds (G-5).</param>
    /// <param name="snapshot">The snapshot that the record or the save holds.</param>
    /// <param name="map">
    /// The map of the snapshot, which the caller read from its content by
    /// <see cref="RunSnapshot.MapIdOrFirst"/> (D-166).
    /// </param>
    /// <param name="battleContent">The battle rules and the fixture of this build (D-766).</param>
    /// <param name="notices">The notice file of this build (D-985).</param>
    /// <param name="debugHandlers">The extra intent handlers of the host (D-260).</param>
    /// <returns>The run, at the tick of the snapshot.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The snapshot is not a state of a run, or the map is another map (T-2).</exception>
    public static Simulation Resume(
        ulong seed,
        RunSnapshot snapshot,
        GameMap map,
        BattleContent battleContent,
        NoticeList notices,
        DebugIntentHandlers debugHandlers)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(battleContent);
        ArgumentNullException.ThrowIfNull(notices);
        ArgumentNullException.ThrowIfNull(debugHandlers);

        return new Simulation(RunState.Resume(seed, snapshot, map, battleContent, notices), debugHandlers);
    }

    /// <summary>Runs one tick of the rules.</summary>
    /// <param name="intents">The intents of this tick, in the order that the host made them.</param>
    /// <returns>The log entries of this tick, which can hold none (D-179).</returns>
    /// <exception cref="ArgumentNullException">The list or one intent is null (T-2).</exception>
    /// <exception cref="SimulationException">
    /// An intent names no rule of this build, a debug intent has no handler, or a rule
    /// refuses the intent. Every message carries the seed, the tick, and the intent (T-2).
    /// </exception>
    public IReadOnlyList<LogEntry> Step(IReadOnlyList<Intent> intents)
    {
        ArgumentNullException.ThrowIfNull(intents);

        List<LogEntry> log = [];

        // The tick rises first, so every error of this step names the tick that the record
        // holds for these intents (D-650, T-2).
        this.State.CountTick();

        foreach (Intent intent in intents)
        {
            ArgumentNullException.ThrowIfNull(intent);
            this.Apply(intent, log);
        }

        if (!this.State.MenuOpen)
        {
            WorldRules.Step(this.State, log);
        }

        return log;
    }

    /// <summary>Takes every battle event since the last take, in the order of the rules (D-532).</summary>
    /// <returns>The events. Game queues them and plays them in order.</returns>
    public IReadOnlyList<BattleEvent> TakeBattleEvents() => this.State.TakeEvents();

    /// <summary>Takes every notice that a rule posted since the last take, in the order of the posts (D-221).</summary>
    /// <returns>The notices. Game queues them and shows them in order (D-994).</returns>
    public IReadOnlyList<NoticeRecord> TakeNotices() => this.State.TakeNotices();

    /// <summary>Stores the whole state of the run (F-10, D-651).</summary>
    /// <returns>The snapshot.</returns>
    public RunSnapshot Snapshot() => this.State.Snapshot();

    /// <summary>Computes the state hash that a replay compares (G-5).</summary>
    /// <returns>The hash of the state.</returns>
    public ulong StateHash() => this.State.StateHash();

    private void Apply(Intent intent, List<LogEntry> log)
    {
        RunContext context = this.State.Context($"intent/{intent.Describe()}");

        if (intent.IsDebug)
        {
            if (!this.debugHandlers.TryFind(intent.Action, out DebugIntentHandler? handler))
            {
                throw new SimulationException(
                    $"the debug intent '{intent.Action.Value}' has no handler, and this host passed {this.debugHandlers.Count} handlers (D-260, D-492)",
                    context);
            }

            handler!(this.State, intent, context, log);
            return;
        }

        if (intent.Target is not null || intent.Item is not null)
        {
            RefuseOutsideBattle(intent, context);
        }

        if (TryBattleChoice(intent, out BattleChoice? choice))
        {
            if (this.State.MenuOpen)
            {
                throw new SimulationException("a battle intent while the menu is open, and a menu pauses the run (D-162)", context);
            }

            BattleTurns.Act(this.State, choice!, context, log);
            return;
        }

        if (string.CompareOrdinal(intent.Action.Value, IntentIds.WaitBattleEnd.Value) == 0)
        {
            BattleTurns.Finish(this.State, context, log);
            return;
        }

        if (string.CompareOrdinal(intent.Action.Value, IntentIds.OpenMenu.Value) == 0)
        {
            this.State.SetMenuOpen(true, context);
            log.Add(MenuEntry("the menu opened", this.State.Tick, intent));
            return;
        }

        if (string.CompareOrdinal(intent.Action.Value, IntentIds.CloseMenu.Value) == 0)
        {
            this.State.SetMenuOpen(false, context);
            log.Add(MenuEntry("the menu closed", this.State.Tick, intent));
            return;
        }

        if (string.CompareOrdinal(intent.Action.Value, IntentIds.PartyRow.Value) == 0)
        {
            BattleTarget target = intent.Target ?? throw new SimulationException("a row change that names no character (D-558)", context);
            this.State.SwapRow(target, context);
            log.Add(new LogEntry(
                LogLevel.Info,
                "a character moved to the other row",
                this.State.Tick,
                LogSubsystems.Run,
                [new LogField("target", target.Describe())]));
            return;
        }

        if (TryStepOf(intent, out StepDirection direction))
        {
            // The rule reads the intent here, and the world step of this tick starts the
            // step. Thus the order of the record and the order of the rules stay the same
            // (D-493, T-7).
            this.State.WantStep(direction, context);
            return;
        }

        throw new SimulationException(
            $"the intent '{intent.Action.Value}' names no rule of this build",
            context);
    }

    /// <summary>
    /// Gives the battle choice of a battle intent (D-764, D-780). The attack and the item take
    /// a target, and the item takes its item. `BattleTurns` refuses a choice that lacks one.
    /// </summary>
    private static bool TryBattleChoice(Intent intent, out BattleChoice? choice)
    {
        BattleAction? action = null;
        if (string.CompareOrdinal(intent.Action.Value, IntentIds.BattleAttack.Value) == 0)
        {
            action = BattleAction.Attack;
        }
        else if (string.CompareOrdinal(intent.Action.Value, IntentIds.BattleDefend.Value) == 0)
        {
            action = BattleAction.Defend;
        }
        else if (string.CompareOrdinal(intent.Action.Value, IntentIds.BattleStep.Value) == 0)
        {
            action = BattleAction.Step;
        }
        else if (string.CompareOrdinal(intent.Action.Value, IntentIds.BattleItem.Value) == 0)
        {
            action = BattleAction.Item;
        }
        else if (string.CompareOrdinal(intent.Action.Value, IntentIds.BattleFlee.Value) == 0)
        {
            action = BattleAction.Flee;
        }

        choice = action is BattleAction chosen ? new BattleChoice(chosen, intent.Target, intent.Item) : null;
        return choice is not null;
    }

    /// <summary>
    /// Refuses a target on an intent that is not an attack, an item use, or a row change, and an
    /// item on an intent that is not an item use. A value that no rule reads points at a fault in
    /// the screen that made the intent (T-2).
    /// </summary>
    private static void RefuseOutsideBattle(Intent intent, RunContext context)
    {
        bool attack = string.CompareOrdinal(intent.Action.Value, IntentIds.BattleAttack.Value) == 0;
        bool item = string.CompareOrdinal(intent.Action.Value, IntentIds.BattleItem.Value) == 0;
        bool row = string.CompareOrdinal(intent.Action.Value, IntentIds.PartyRow.Value) == 0;
        if ((intent.Target is not null && !attack && !item && !row) || (intent.Item is not null && !item))
        {
            throw new SimulationException(
                "an intent that carries a target or an item that no rule of its action reads (D-558, D-764, D-780)",
                context);
        }
    }

    /// <summary>
    /// Gives the direction of a move intent (D-493, D-716). A step goes in four directions
    /// alone, so four ids cover every step of the party.
    /// </summary>
    private static bool TryStepOf(Intent intent, out StepDirection direction)
    {
        if (string.CompareOrdinal(intent.Action.Value, IntentIds.MoveNorth.Value) == 0)
        {
            direction = StepDirection.North;
            return true;
        }

        if (string.CompareOrdinal(intent.Action.Value, IntentIds.MoveSouth.Value) == 0)
        {
            direction = StepDirection.South;
            return true;
        }

        if (string.CompareOrdinal(intent.Action.Value, IntentIds.MoveEast.Value) == 0)
        {
            direction = StepDirection.East;
            return true;
        }

        if (string.CompareOrdinal(intent.Action.Value, IntentIds.MoveWest.Value) == 0)
        {
            direction = StepDirection.West;
            return true;
        }

        direction = StepDirection.North;
        return false;
    }

    /// <summary>
    /// Makes the entry of a menu change. A menu pauses the world, so a report reads the pauses
    /// of a run from these entries alone (D-162, D-179).
    /// </summary>
    private static LogEntry MenuEntry(string message, long tick, Intent intent) =>
        new(
            LogLevel.Info,
            message,
            tick,
            LogSubsystems.Run,
            [new LogField("action", intent.Action.Value)]);
}
