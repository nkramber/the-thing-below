using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Saves;
using TheThingBelow.Game.Ui;

namespace TheThingBelow.Game;

/// <summary>
/// One run in the game: the fixed-step clock, the simulation, and the recorder of the run
/// (D-164, G-5). Game holds no rule, and it calls Core on each tick (D-100).
/// </summary>
/// <remarks>
/// The host passes the handlers of <see cref="DebugSeam.Handlers"/> to the start of the run.
/// A development build passes the handlers of the console, and a release build passes none, so
/// a debug intent in a record fails there with the intent and the tick (D-260, D-492).
/// <para>
/// PR-61 makes an intent from each input event and gives it to <see cref="Queue"/>. No
/// intent comes from a Godot timer, a physics step, or a poll of the input singleton (G-23,
/// F-50).
/// </para>
/// <para>
/// <see cref="Advance"/> gives the log entries of the ticks that it ran, and <see cref="Boot"/>
/// writes each one to the log file of the session with the time of the host (D-179). A crash
/// takes <see cref="Record"/> into the crash file (D-170).
/// </para>
/// </remarks>
public sealed class GameRun
{
    private readonly FixedStepLoop loop = new();
    private readonly List<Intent> queued = [];
    private readonly BattleEventQueue events = new();
    private readonly Simulation simulation;
    private readonly RunRecorder recorder;

    private GameRun(Simulation simulation, RunRecorder recorder)
    {
        this.simulation = simulation;
        this.recorder = recorder;
    }

    /// <summary>The count of ticks since the start of the run (D-164).</summary>
    public long Tick => this.simulation.Tick;

    /// <summary>The count of ticks that the record holds a line for (F-10).</summary>
    public int RecordedLines => this.recorder.LineCount;

    /// <summary>True while a menu is open, which pauses the world (D-162, D-650).</summary>
    /// <remarks>
    /// The state of the run is the one source of this answer. A copy in the host would drift
    /// from it after a replay or a load, and the menu action would then send the wrong intent
    /// (T-2).
    /// </remarks>
    public bool MenuOpen => this.simulation.State.MenuOpen;

    /// <summary>
    /// True when a menu is open for the next tick, with the queued intents of this frame
    /// applied (D-162, D-650).
    /// </summary>
    /// <remarks>
    /// The host reads input before it runs the ticks of a frame, so the queue can already
    /// hold the intent that opens the menu while <see cref="MenuOpen"/> is still false. A
    /// host that read that member alone would send a step intent for the same tick, and the
    /// rules refuse a step while a menu is open (T-2).
    /// </remarks>
    public bool MenuOpenNextTick
    {
        get
        {
            bool open = this.MenuOpen;
            foreach (Intent queued in this.queued)
            {
                if (string.CompareOrdinal(queued.Action.Value, IntentIds.OpenMenu.Value) == 0)
                {
                    open = true;
                }
                else if (string.CompareOrdinal(queued.Action.Value, IntentIds.CloseMenu.Value) == 0)
                {
                    open = false;
                }
            }

            return open;
        }
    }

    /// <summary>
    /// The state of the run, which a report command of the debug console reads (D-171, D-724).
    /// </summary>
    /// <remarks>
    /// The console reads this state and never changes it. A change of the state outside a tick
    /// would leave the run record behind the state, and the run would no longer replay (T-7).
    /// </remarks>
    public RunState State => this.simulation.State;

    /// <summary>The part of the next tick that the frames reached, in thousandths (D-820).</summary>
    public int TickPart => this.loop.TickPart;

    /// <summary>The party on its map, which the map scene draws (D-106, D-203).</summary>
    /// <remarks>
    /// Game reads the tile of the lead and the ticks of the step that runs, and it slides
    /// the sprite between two tiles across those ticks. Core keeps the lead on a whole tile
    /// (D-203).
    /// </remarks>
    public MapState Party => this.simulation.State.Party;

    /// <summary>True while an encounter or a battle holds the map still (D-531).</summary>
    public bool InBattle => this.simulation.State.Battle is not null || this.simulation.State.Party.Patrols.Encounter is not null;

    /// <summary>The count of battle events that the screen has yet to play (D-532).</summary>
    public int WaitingEvents => this.events.Count;

    /// <summary>
    /// True when the screen has played every event and a character has the turn. Game takes
    /// the next command of the player only then, so this is the input gate of a battle (D-532).
    /// </summary>
    public bool TakesBattleCommand =>
        this.events.Empty
        && this.simulation.State.Battle is Battle battle
        && battle.Outcome == BattleOutcome.Running
        && battle.Next() is Combatant next
        && next.Side == BattleSide.Party;

    /// <summary>True when the screen has played the events of a wipe, and the host reloads (D-397, D-776).</summary>
    public bool WipeReady =>
        this.events.Empty
        && this.simulation.State.Battle is Battle battle
        && battle.Outcome == BattleOutcome.Wiped;

    /// <summary>Starts a run over a content set.</summary>
    /// <param name="content">The content of this build, which gives the content hash (D-648).</param>
    /// <param name="seed">The seed of the run (G-3, G-4).</param>
    /// <param name="debugHandlers">
    /// The extra intent handlers of this build, which <see cref="DebugSeam.Handlers"/> gives.
    /// A development build passes the handlers of the console, and a release build passes
    /// <see cref="DebugIntentHandlers.None"/> (D-260, D-492).
    /// </param>
    /// <returns>The run, at tick zero, on the first map (D-528).</returns>
    /// <exception cref="ArgumentNullException">The content set or the handler set is null (T-2).</exception>
    /// <exception cref="ContentException">This build holds no first map (T-2).</exception>
    public static GameRun Start(ContentSet content, ulong seed, DebugIntentHandlers debugHandlers)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(debugHandlers);

        RunHeader header = RunHeader.ForThisBuild(content.Hash, seed);
        Simulation simulation = Simulation.Start(seed, content.Map(MapIds.FirstMap), content.Battle, debugHandlers);
        return new GameRun(simulation, new RunRecorder(header, simulation.Snapshot()));
    }

    /// <summary>
    /// Starts the run again after a wipe: from the newer of the slot save and the autosave, or
    /// from the start of a new run when neither exists (D-231, D-776).
    /// </summary>
    /// <param name="content">The content set of this build.</param>
    /// <param name="slot">The slot save, or no value.</param>
    /// <param name="autosave">The autosave, or no value.</param>
    /// <param name="seed">The seed of a new run, when no save exists.</param>
    /// <param name="debugHandlers">The debug handlers of the host (D-260).</param>
    /// <returns>The run, with a new record that starts at its first tick.</returns>
    /// <exception cref="ArgumentNullException">The content or the handlers are null (T-2).</exception>
    public static GameRun Reload(
        ContentSet content,
        SaveDocument? slot,
        SaveDocument? autosave,
        ulong seed,
        DebugIntentHandlers debugHandlers)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(debugHandlers);

        if (SavePick.NewerOf(slot, autosave) is not SaveDocument save)
        {
            return Start(content, seed, debugHandlers);
        }

        RunSnapshot snapshot = save.Snapshot;
        Simulation simulation = Simulation.Resume(
            save.Header.Seed,
            snapshot,
            content.Map(snapshot.MapIdOrFirst),
            content.Battle,
            debugHandlers);
        RunHeader header = RunHeader.ForThisBuild(content.Hash, save.Header.Seed);
        return new GameRun(simulation, new RunRecorder(header, simulation.Snapshot()));
    }

    /// <summary>
    /// Gives the intent that one action of the input map makes now (D-493, D-561). The menu
    /// action makes two intents, because one button opens the menu and closes it (D-162).
    /// </summary>
    /// <param name="action">The name of the action, such as `menu`.</param>
    /// <returns>The intent of the player.</returns>
    /// <exception cref="ArgumentException">The name is not an action of the input map (T-2).</exception>
    /// <remarks>
    /// The method reads the menu state of the run, so the host never holds a copy of it. A
    /// host that passed a constant would send `intent.open_menu` for every press, and the
    /// player could not leave the menu with its own button (T-2).
    /// </remarks>
    public Intent IntentOf(string action) => Intent.OfPlayer(InputActions.IntentOf(action, this.MenuOpen));

    /// <summary>Adds an intent that the next tick applies (D-493).</summary>
    /// <param name="intent">The intent, which Game made from one input event (F-50).</param>
    /// <exception cref="ArgumentNullException">The intent is null (T-2).</exception>
    public void Queue(Intent intent)
    {
        ArgumentNullException.ThrowIfNull(intent);

        this.queued.Add(intent);
    }

    /// <summary>Runs the ticks that the time of one frame gives (D-164).</summary>
    /// <param name="seconds">The time of the frame, in seconds.</param>
    /// <param name="heldStep">
    /// Gives the step intent of the direction that the player holds, before each tick, or
    /// null when that tick takes no step. A session with no player passes null.
    /// </param>
    /// <returns>The log entries of every tick of this frame, in the order of the ticks (D-179).</returns>
    /// <exception cref="ArgumentOutOfRangeException">The time is below zero (T-2).</exception>
    /// <remarks>
    /// The queued intents go to the first tick of the frame, because they came before it.
    /// The held step goes to each tick of the frame. A frame of two ticks once gave it to the
    /// first tick alone, so the party stood still for a tick when it reached a tile on the
    /// second one (D-820).
    /// <para>
    /// The recorder takes each tick before the step, so the record of a crash holds the
    /// intents of the tick that crashed, and a replay of the crash file reaches the crash
    /// (D-170, G-5). A frame that dropped ticks adds one log entry, so no work of a step
    /// goes in silence (T-2).
    /// </para>
    /// </remarks>
    public IReadOnlyList<LogEntry> Advance(double seconds, Func<Intent?>? heldStep = null)
    {
        long droppedBefore = this.loop.DroppedTicks;
        int ticks = this.loop.Advance(seconds);
        List<LogEntry> log = [];
        for (int step = 0; step < ticks; step += 1)
        {
            List<Intent> intents = step == 0 ? [.. this.queued] : [];
            if (heldStep?.Invoke() is Intent held)
            {
                intents.Add(held);
            }

            this.recorder.Step(this.simulation.Tick + 1, intents);
            log.AddRange(this.simulation.Step(intents));
            this.events.Add(this.simulation.TakeBattleEvents());
        }

        if (ticks > 0)
        {
            this.queued.Clear();
        }

        this.PlayBattleEvent(log);

        long dropped = this.loop.DroppedTicks - droppedBefore;
        if (dropped > 0)
        {
            log.Add(new LogEntry(
                LogLevel.Error,
                "the loop dropped ticks, because one frame took too long",
                this.simulation.Tick,
                LogSubsystems.Game,
                [LogField.OfNumber("dropped", dropped)]));
        }

        return log;
    }

    /// <summary>Gives the record of the run as it stands now (G-5, D-651).</summary>
    /// <returns>The record, which the crash file of D-170 carries.</returns>
    /// <summary>
    /// Plays one battle event on this frame, and writes it as one log line until the battle
    /// screen of PR-10 (D-532, D-767). When the queue drains after a win or a flee, the run
    /// takes the wait intent, and the map runs again on the next tick (D-522).
    /// </summary>
    private void PlayBattleEvent(List<LogEntry> log)
    {
        if (!this.events.Empty)
        {
            BattleEvent played = this.events.PlayNext();
            log.Add(new LogEntry(
                LogLevel.Info,
                "the screen played a battle event",
                this.simulation.Tick,
                LogSubsystems.Battle,
                [new LogField("event", played.Describe())]));
            return;
        }

        if (this.simulation.State.Battle is not Battle battle
            || (battle.Outcome != BattleOutcome.Won && battle.Outcome != BattleOutcome.Fled))
        {
            return;
        }

        // A frame can run no tick, and the queue then still holds the wait intent of an
        // earlier frame. A second one in one tick meets the refusal of the rules (T-2).
        foreach (Intent waiting in this.queued)
        {
            if (string.CompareOrdinal(waiting.Action.Value, IntentIds.WaitBattleEnd.Value) == 0)
            {
                return;
            }
        }

        this.queued.Add(Intent.OfPlayer(IntentIds.WaitBattleEnd));
    }

    public RunRecord Record() => this.recorder.Build();

    /// <summary>Gives the state hash of the run, which a replay compares (G-5).</summary>
    /// <returns>The hash.</returns>
    public ulong StateHash() => this.simulation.StateHash();
}
