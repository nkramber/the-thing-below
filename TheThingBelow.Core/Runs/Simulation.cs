using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Notices;
using TheThingBelow.Core.Story;

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
/// The lesson window casts a Mend rite or a cure rite and swaps a lesson while a menu is open,
/// and a swap needs a swap place (D-391, D-1030). A battle intent can use a form of a lesson
/// (D-1027, D-1031).
/// </para>
/// <para>
/// A battle intent resolves the turn of a character at once, with each enemy turn up to the
/// next turn of a character (D-532). The events wait in the run until the host takes them
/// with <see cref="TakeBattleEvents"/>. The wait intent of a battle ends a win or a flee,
/// and the map runs again from the next tick (D-522).
/// </para>
/// <para>
/// While a story scene runs, the intents of the player are the step end, the pick, and the
/// pause, and a battle of the story scene takes the battle intents. Any other intent is an
/// error. The pause holds the world step, and it takes the end of the pause alone (D-1009,
/// D-1010).
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
    /// <param name="story">The story content of this build (D-1004).</param>
    /// <param name="debugHandlers">
    /// The extra intent handlers of the host. A release build passes
    /// <see cref="DebugIntentHandlers.None"/> (D-260, D-492).
    /// </param>
    /// <returns>The run.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static Simulation Start(ulong seed, GameMap map, BattleContent battleContent, NoticeList notices, StoryContent story, DebugIntentHandlers debugHandlers)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(battleContent);
        ArgumentNullException.ThrowIfNull(notices);
        ArgumentNullException.ThrowIfNull(story);
        ArgumentNullException.ThrowIfNull(debugHandlers);

        return new Simulation(RunState.Start(seed, map, battleContent, notices, story), debugHandlers);
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
    /// <param name="story">The story content of this build (D-166).</param>
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
        StoryContent story,
        DebugIntentHandlers debugHandlers)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(battleContent);
        ArgumentNullException.ThrowIfNull(notices);
        ArgumentNullException.ThrowIfNull(story);
        ArgumentNullException.ThrowIfNull(debugHandlers);

        return new Simulation(RunState.Resume(seed, snapshot, map, battleContent, notices, story), debugHandlers);
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

        // A menu and the pause of a story scene each hold the world still (D-162, D-1010).
        if (!this.State.MenuOpen && !this.State.Story.Paused)
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

        if (intent.Target is not null || intent.Item is not null || intent.Option is not null || intent.Lesson is not null || intent.Actor is not null)
        {
            RefuseOutsideBattle(intent, context);
        }

        if (this.TryStoryIntent(intent, context, log))
        {
            return;
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

        if (this.TryLessonIntent(intent, context, log))
        {
            return;
        }

        if (this.TryPackIntent(intent, context, log))
        {
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
    /// Applies an intent of a story scene, and refuses any other intent of the player while a
    /// story scene runs (D-1009, D-1010).
    /// </summary>
    /// <returns>True when the intent was an intent of a story scene, which this method applied.</returns>
    private bool TryStoryIntent(Intent intent, RunContext context, List<LogEntry> log)
    {
        StoryState story = this.State.Story;
        if (Is(intent, IntentIds.StoryResume))
        {
            StoryRules.Resume(this.State, context);
            log.Add(StoryEntry("the pause of a story scene ended", this.State.Tick, intent));
            return true;
        }

        if (story.Paused)
        {
            throw new SimulationException($"the intent '{intent.Action.Value}' while the story scene is paused, and the pause takes its end alone (D-1010)", context);
        }

        if (Is(intent, IntentIds.StoryPause))
        {
            StoryRules.Pause(this.State, context);
            log.Add(StoryEntry("the player paused a story scene", this.State.Tick, intent));
            return true;
        }

        if (Is(intent, IntentIds.StoryStepEnd))
        {
            StoryRules.EndStep(this.State, context, log);
            return true;
        }

        if (Is(intent, IntentIds.StoryPick))
        {
            int option = intent.Option ?? throw new SimulationException("a pick that names no option (D-1007)", context);
            StoryRules.Pick(this.State, option, context, log);
            return true;
        }

        // A battle of a story scene takes the battle intents, and the wait intent of its end
        // lets the story scene go on (D-999).
        bool battleIntent = TryBattleChoice(intent, out _) || Is(intent, IntentIds.WaitBattleEnd);
        if (story.Running && !(story.Phase == ScenePhase.Battle && battleIntent))
        {
            throw new SimulationException(
                $"the intent '{intent.Action.Value}' while a story scene runs, and a story scene takes the step end, the pick, and the pause alone (D-1009)",
                context);
        }

        return false;
    }

    private static bool Is(Intent intent, ContentId action) => string.CompareOrdinal(intent.Action.Value, action.Value) == 0;

    private static LogEntry StoryEntry(string message, long tick, Intent intent) =>
        new(
            LogLevel.Info,
            message,
            tick,
            LogSubsystems.Story,
            [new LogField("action", intent.Action.Value)]);

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
        else if (string.CompareOrdinal(intent.Action.Value, IntentIds.BattleLesson.Value) == 0)
        {
            action = BattleAction.Lesson;
        }

        choice = action is BattleAction chosen ? new BattleChoice(chosen, intent.Target, intent.Item, intent.Lesson, chosen == BattleAction.Lesson ? intent.Option : null) : null;
        return choice is not null;
    }

    /// <summary>
    /// Refuses a target on an intent that is not an attack, an item use, or a row change, an item
    /// on an intent that is not an item use, and an option on an intent that is not a pick. A value that no rule reads points at a fault in
    /// the screen that made the intent (T-2).
    /// </summary>
    private static void RefuseOutsideBattle(Intent intent, RunContext context)
    {
        bool attack = Is(intent, IntentIds.BattleAttack);
        bool item = Is(intent, IntentIds.BattleItem);
        bool row = Is(intent, IntentIds.PartyRow);
        bool pick = Is(intent, IntentIds.StoryPick);
        bool use = Is(intent, IntentIds.BattleLesson);
        bool cast = Is(intent, IntentIds.MenuCast);
        bool swap = Is(intent, IntentIds.LessonSwap);
        bool menuItem = Is(intent, IntentIds.MenuItem);
        bool wear = Is(intent, IntentIds.GearWear);
        bool unread =
            (intent.Target is not null && !attack && !item && !row && !use && !cast && !menuItem) ||
            (intent.Item is not null && !item && !menuItem && !wear) ||
            (intent.Option is not null && !pick && !use && !cast && !swap && !wear) ||
            (intent.Lesson is not null && !use && !cast && !swap) ||
            (intent.Actor is not null && !cast && !swap && !wear);
        if (unread)
        {
            throw new SimulationException(
                "an intent that carries a target, an item, an option, a lesson, or an actor that no rule of its action reads (D-558, D-764, D-780, D-1007, D-1027, D-1030)",
                context);
        }
    }

    /// <summary>
    /// Applies an item use of the item window or a change of gear of the gear window (D-1048,
    /// D-1049). Both need the open menu and no battle, as the row change does (D-558).
    /// </summary>
    /// <returns>True when the intent was one of the two, which this method applied.</returns>
    private bool TryPackIntent(Intent intent, RunContext context, List<LogEntry> log)
    {
        bool use = Is(intent, IntentIds.MenuItem);
        bool wear = Is(intent, IntentIds.GearWear);
        if (!use && !wear)
        {
            return false;
        }

        if (!this.State.MenuOpen || this.State.Battle is not null)
        {
            throw new SimulationException($"the intent '{intent.Action.Value}' while no menu is open or a battle holds the run, and the item and gear windows make it (D-1048, D-1049)", context);
        }

        if (use)
        {
            ContentId item = intent.Item ?? throw new SimulationException("an item use from the menu that names no item (D-1046)", context);
            BattleTarget target = intent.Target ?? throw new SimulationException("an item use from the menu that names no target (D-1046)", context);
            if (target.Side != BattleSide.Party)
            {
                throw new SimulationException($"an item use from the menu at {target.Describe()}, and the item window aims at the party (D-1046)", context);
            }

            ItemRules.UseFromMenu(this.State, item, target.Slot, context);
            log.Add(new LogEntry(LogLevel.Info, "a character used an item from the menu", this.State.Tick, LogSubsystems.Run, [new LogField("intent", intent.Describe())]));
            return true;
        }

        int actor = intent.Actor ?? throw new SimulationException("a change of gear that names no character (D-1048)", context);
        int slot = intent.Option ?? throw new SimulationException("a change of gear that names no gear slot (D-1048)", context);
        this.State.Characters.Wear(actor, slot, intent.Item, this.State.BattleContent, context);
        log.Add(new LogEntry(LogLevel.Info, "a character changed gear", this.State.Tick, LogSubsystems.Run, [new LogField("intent", intent.Describe())]));
        return true;
    }

    /// <summary>
    /// Applies a cast from the menu or a swap of lessons (D-391, D-1030). Both need the open
    /// menu and no battle, as the row change does (D-558).
    /// </summary>
    /// <returns>True when the intent was one of the two, which this method applied.</returns>
    private bool TryLessonIntent(Intent intent, RunContext context, List<LogEntry> log)
    {
        bool cast = Is(intent, IntentIds.MenuCast);
        bool swap = Is(intent, IntentIds.LessonSwap);
        if (!cast && !swap)
        {
            return false;
        }

        if (!this.State.MenuOpen || this.State.Battle is not null)
        {
            throw new SimulationException($"the intent '{intent.Action.Value}' while no menu is open or a battle holds the run, and the lesson window makes it (D-391, D-1030)", context);
        }

        int actor = intent.Actor ?? throw new SimulationException($"the intent '{intent.Action.Value}' names no character (D-391, D-1030)", context);
        int option = intent.Option ?? throw new SimulationException($"the intent '{intent.Action.Value}' names no form or slot (D-1027, D-1030)", context);
        if (cast)
        {
            ContentId lesson = intent.Lesson ?? throw new SimulationException("a cast from the menu that names no lesson (D-391)", context);
            BattleTarget target = intent.Target ?? throw new SimulationException("a cast from the menu that names no target (D-391)", context);
            if (target.Side != BattleSide.Party)
            {
                throw new SimulationException($"a cast from the menu at {target.Describe()}, and a cast from the menu aims at the party (D-391)", context);
            }

            LessonRules.CastFromMenu(this.State, actor, lesson, option, target.Slot, context);
            log.Add(new LogEntry(LogLevel.Info, "a character cast a rite from the menu", this.State.Tick, LogSubsystems.Run, [new LogField("intent", intent.Describe())]));
            return true;
        }

        this.State.Characters.Swap(actor, option, intent.Lesson, context);
        log.Add(new LogEntry(LogLevel.Info, "a character swapped a lesson", this.State.Tick, LogSubsystems.Run, [new LogField("intent", intent.Describe())]));
        return true;
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
