using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Notices;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Saves;
using TheThingBelow.Game.Ui;
using TheThingBelow.Storage;

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
    private readonly BattleEffects pace;
    private readonly TransitionContent transitions;
    private readonly ScreenHandOff handOff;
    private readonly StringTable strings;
    private readonly NoticeQueue notices;
    private ContentId? lastCommon;
    private BattleView? view;
    private BattleEvent? playing;
    private IReadOnlyList<StartMember>? startParty;
    private long playingSince;

    private GameRun(Simulation simulation, RunRecorder recorder, ContentSet content, MessageSpeed messageSpeed)
    {
        this.simulation = simulation;
        this.recorder = recorder;
        this.pace = content.Effects.Battle;
        this.transitions = content.Effects.Transitions;
        this.handOff = new ScreenHandOff(content.Effects.Transitions.Table.FadeTicks);
        this.strings = content.Strings;
        this.notices = new NoticeQueue(content.Style.Notice);
        this.MessageSpeed = messageSpeed;
    }

    /// <summary>The message speed of the battle group, which the settings screen changes (D-866, D-873).</summary>
    /// <remarks>
    /// The speed sets when the input gate of a fight opens, and the record holds the intent
    /// that the player then makes. Thus a replay needs no setting (T-7).
    /// </remarks>
    public MessageSpeed MessageSpeed { get; set; }

    /// <summary>The pace of the fights on screen, from the battle file of the content set (D-829, D-883).</summary>
    public BattleEffects Pace => this.pace;

    /// <summary>The transition into a fight, the fade into it, and the fade back to the map (D-938, D-939).</summary>
    public ScreenHandOff HandOff => this.handOff;

    /// <summary>The transitions and their table, which the screen reads to draw a phase of the hand-off (D-195).</summary>
    public TransitionContent Transitions => this.transitions;

    /// <summary>
    /// True while the screen shows the fight: from the end of the transition into it to the start of
    /// the fade back to the map (D-938, D-939).
    /// </summary>
    public bool ShowsBattle => this.view is not null && !this.handOff.ShowsMapAgain;

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

    /// <summary>True while the party holds the torch out, which the carried light and the torch in the hand follow (D-847, D-1064).</summary>
    public bool TorchHeld => this.simulation.State.Characters.TorchHeld;

    /// <summary>
    /// True when the party holds the torch out for the next tick, with the queued intents of
    /// this frame applied (D-1064), as <see cref="MenuOpenNextTick"/> reads the menu.
    /// </summary>
    public bool TorchHeldNextTick
    {
        get
        {
            bool held = this.TorchHeld;
            foreach (Intent queued in this.queued)
            {
                if (string.CompareOrdinal(queued.Action.Value, IntentIds.HoldTorch.Value) == 0)
                {
                    held = true;
                }
                else if (string.CompareOrdinal(queued.Action.Value, IntentIds.PutTorchAway.Value) == 0)
                {
                    held = false;
                }
            }

            return held;
        }
    }

    /// <summary>
    /// True when the torch action makes an intent: on the walk, with no menu, no fight, no
    /// encounter, and no story scene, and with the torch in the pack (D-1071).
    /// </summary>
    /// <remarks>The rules refuse the intent at any other time, so the host sends none then (T-2).</remarks>
    public bool TorchWorks =>
        !this.MenuOpenNextTick &&
        !this.InBattle &&
        !this.StoryRunning &&
        this.simulation.State.Characters.CountOf(TorchRules.Torch) > 0;

    /// <summary>True while a story scene runs, which takes no step and no menu intent of the player (D-1009).</summary>
    public bool StoryRunning => this.simulation.State.Story.Running;

    /// <summary>
    /// True when the menu action and the map action open a window: on the walk, with no menu, no
    /// fight, no encounter, and no story scene (D-162, D-986, D-1009).
    /// </summary>
    /// <remarks>
    /// The rules refuse the open of the menu while a story scene runs, so the host opens no
    /// window then (T-2). No map of this build holds a trigger yet, and the first one would
    /// otherwise crash at the menu button.
    /// </remarks>
    public bool MenuWorks => !this.MenuOpenNextTick && !this.InBattle && !this.StoryRunning;

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

    /// <summary>The fight as the screen shows it, or no value outside a battle (D-532).</summary>
    public BattleView? BattleView => this.view;

    /// <summary>The battle event that the screen plays or played last, or no value (D-532).</summary>
    /// <remarks>The message line keeps this event on screen until the next one (D-213).</remarks>
    public BattleEvent? PlayingEvent => this.playing;

    /// <summary>The ticks since the screen started <see cref="PlayingEvent"/> (D-829).</summary>
    public int PlayingTicks => this.playing is null ? 0 : (int)Math.Min(int.MaxValue, this.FightTick - this.playingSince);

    /// <summary>Ends the hold of the event that the screen plays, so the next one plays on the next tick (D-866).</summary>
    /// <remarks>
    /// A press of confirm skips a message. The skip moves the start of the hold back and changes
    /// no state of the run, so the record holds only the command that the player makes after it.
    /// </remarks>
    /// <returns>True when an event played and its hold ended, and false when no event holds.</returns>
    public bool SkipPlayingEvent()
    {
        if (this.playing is null || this.PlayedOut)
        {
            return false;
        }

        this.playingSince = this.FightTick - BattleTimes.HoldTicksOf(this.pace, this.playing.Kind, this.MessageSpeed);
        return true;
    }

    /// <summary>
    /// True when the screen has played every event and a character has the turn. Game takes
    /// the next command of the player only then, so this is the input gate of a battle (D-532).
    /// </summary>
    /// <remarks>
    /// A pause of the fight closes the gate, because the rules refuse a battle intent while the
    /// menu is open, and a command then crashed the session (D-162, D-1083).
    /// </remarks>
    public bool TakesBattleCommand =>
        this.EventsPlayed
        && !this.MenuOpenNextTick
        && this.simulation.State.Battle is Battle battle
        && battle.Outcome == BattleOutcome.Running
        && battle.Next() is Combatant next
        && next.Side == BattleSide.Party;

    /// <summary>
    /// The clock of a fight on screen: the count of ticks in which the world ran (D-650). The
    /// playback of the events, the hand-off, and the battle screen read it, so the pause of a
    /// fight holds each of them (D-1083).
    /// </summary>
    /// <remarks>
    /// The tick of the run rises under a menu, and a fight that counted it played its events
    /// on under the pause and then opened the command menu (D-1083).
    /// </remarks>
    public long FightTick => this.simulation.State.WorldTick;

    /// <summary>True while the menu pauses a fight or its encounter, with the queued intents of this frame applied (D-1083).</summary>
    public bool FightPaused => this.InBattle && this.MenuOpenNextTick;

    /// <summary>
    /// Gives the intent that the menu action or the back action makes in a fight (D-1083). The
    /// menu action pauses the fight, and the menu action or the back action ends the pause.
    /// </summary>
    /// <param name="action">The name of the action, such as `menu`.</param>
    /// <returns>The open or the close of the menu, or no value when the action makes no intent now.</returns>
    /// <exception cref="ArgumentException">The name is empty (T-2).</exception>
    /// <remarks>
    /// The method gives no intent while the queue holds a menu intent that the rules have yet to
    /// apply, because a second open meets the refusal of the rules (D-1083, T-2). It gives none
    /// while the queue holds the wait intent of the end of the fight either, because the fight
    /// would end under the pause, and the map would come back held with no window. A story
    /// scene takes no menu intent (D-1009), so the method gives none while one runs.
    /// </remarks>
    public Intent? PauseIntentOf(string action)
    {
        ArgumentException.ThrowIfNullOrEmpty(action);

        bool menu = string.CompareOrdinal(action, InputActions.Menu) == 0;
        bool back = string.CompareOrdinal(action, InputActions.Cancel) == 0;
        if ((!menu && !back)
            || !this.InBattle
            || this.simulation.State.Story.Running
            || this.Queued(IntentIds.OpenMenu)
            || this.Queued(IntentIds.CloseMenu)
            || this.Queued(IntentIds.WaitBattleEnd))
        {
            return null;
        }

        if (this.MenuOpen)
        {
            return Intent.OfPlayer(IntentIds.CloseMenu);
        }

        return menu ? Intent.OfPlayer(IntentIds.OpenMenu) : null;
    }

    /// <summary>True when the screen has played the events of a wipe, and the host reloads (D-397, D-776).</summary>
    public bool WipeReady =>
        this.EventsPlayed
        && this.simulation.State.Battle is Battle battle
        && battle.Outcome == BattleOutcome.Wiped;

    /// <summary>True when the queue is empty and the last event played all its ticks (D-532, D-829).</summary>
    private bool EventsPlayed => this.events.Empty && this.PlayedOut;

    /// <summary>True when no event plays, or the one that plays reached its end (D-829).</summary>
    private bool PlayedOut =>
        this.playing is null
        || this.FightTick - this.playingSince >= BattleTimes.HoldTicksOf(this.pace, this.playing.Kind, this.MessageSpeed);

    /// <summary>Starts a run over a content set.</summary>
    /// <param name="content">The content of this build, which gives the content hash (D-648).</param>
    /// <param name="seed">The seed of the run (G-3, G-4).</param>
    /// <param name="debugHandlers">
    /// The extra intent handlers of this build, which <see cref="DebugSeam.Handlers"/> gives.
    /// A development build passes the handlers of the console, and a release build passes
    /// <see cref="DebugIntentHandlers.None"/> (D-260, D-492).
    /// </param>
    /// <param name="messageSpeed">The message speed of the settings (D-866).</param>
    /// <returns>The run, at tick zero, on the first map (D-528).</returns>
    /// <exception cref="ArgumentNullException">The content set or the handler set is null (T-2).</exception>
    /// <exception cref="ContentException">This build holds no first map (T-2).</exception>
    public static GameRun Start(ContentSet content, ulong seed, DebugIntentHandlers debugHandlers, MessageSpeed messageSpeed)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(debugHandlers);

        RunHeader header = RunHeader.ForThisBuild(content.Hash, seed);
        Simulation simulation = Simulation.Start(seed, content.Map(MapIds.FirstMap), content.Battle, content.Notices, content.Story, debugHandlers);
        return new GameRun(simulation, new RunRecorder(header, simulation.Snapshot()), content, messageSpeed);
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
    /// <param name="messageSpeed">The message speed of the settings (D-866).</param>
    /// <returns>The run, with a new record that starts at its first tick.</returns>
    /// <exception cref="ArgumentNullException">The content or the handlers are null (T-2).</exception>
    public static GameRun Reload(
        ContentSet content,
        SaveDocument? slot,
        SaveDocument? autosave,
        ulong seed,
        DebugIntentHandlers debugHandlers,
        MessageSpeed messageSpeed)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(debugHandlers);

        if (SavePick.NewerOf(slot, autosave) is not SaveDocument save)
        {
            return Start(content, seed, debugHandlers, messageSpeed);
        }

        RunSnapshot snapshot = save.Snapshot;
        Simulation simulation = Simulation.Resume(
            save.Header.Seed,
            snapshot,
            content.Map(snapshot.MapIdOrFirst),
            content.Battle,
            content.Notices,
            content.Story,
            debugHandlers);
        RunHeader header = RunHeader.ForThisBuild(content.Hash, save.Header.Seed);
        return new GameRun(simulation, new RunRecorder(header, simulation.Snapshot()), content, messageSpeed);
    }

    /// <summary>
    /// Gives the intent that one action of the input map makes now (D-493, D-561). The menu
    /// action makes two intents, because one button opens the menu and closes it (D-162). The
    /// torch action makes two too, because one button holds the torch out and puts it away (D-1064).
    /// </summary>
    /// <param name="action">The name of the action, such as `menu`.</param>
    /// <returns>The intent of the player.</returns>
    /// <exception cref="ArgumentException">The name is not an action of the input map (T-2).</exception>
    /// <remarks>
    /// The method reads the menu state of the run, so the host never holds a copy of it. A
    /// host that passed a constant would send `intent.open_menu` for every press, and the
    /// player could not leave the menu with its own button (T-2). The torch reads the queued
    /// intents of this frame, so a second press before the tick puts the torch away again.
    /// </remarks>
    public Intent IntentOf(string action) => Intent.OfPlayer(InputActions.IntentOf(action, this.MenuOpen, this.TorchHeldNextTick));

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

            // The start view of a fight reads the party from before the tick that starts it, because
            // an ambush that wipes the party ends the fight inside that tick (D-776).
            IReadOnlyList<StartMember>? before = this.simulation.State.Battle is null ? BattleView.PartyOf(this.simulation.State) : null;

            this.recorder.Step(this.simulation.Tick + 1, intents);
            log.AddRange(this.simulation.Step(intents));

            // The wait intent ends a fight inside this tick, and the next tick of the same frame can
            // start another one. So the screen leaves the fight here, and not once for each frame (T-2).
            if (this.simulation.State.Battle is null)
            {
                this.LeaveFight();
            }

            // A posted notice joins the queue of the notice box on its own tick, so the notice box starts it
            // at the tick of the world where the rule posted it (D-221, D-994).
            foreach (NoticeRecord posted in this.simulation.TakeNotices())
            {
                this.notices.Add(posted.Id, this.strings.Text(posted.Id).Length, this.simulation.State.WorldTick);
            }

            IReadOnlyList<BattleEvent> taken = this.simulation.TakeBattleEvents();
            if (StartsFight(taken))
            {
                this.startParty = before ?? throw new InvalidOperationException(
                    $"A fight started at tick {this.simulation.Tick} while another fight held the run (D-531, T-2).");
                log.Add(this.StartTransition());
            }

            this.events.Add(taken);
        }

        if (ticks > 0)
        {
            this.queued.Clear();
        }

        this.PlayBattleEvents(log);

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

    /// <summary>
    /// Starts each battle event whose turn came: the next event starts when the one before
    /// played all its ticks, and an event of no ticks lets the next one start at once (D-532,
    /// D-829). Each start applies the event to the view of the screen and writes one log line.
    /// No event plays while the transition into the fight runs (D-939). When the queue drains after
    /// a win or a flee, the map fades back in, and the run then takes the wait intent, so the map
    /// runs again on the next tick (D-522, D-938).
    /// </summary>
    /// <remarks>
    /// The pace and the hand-off count ticks of the run and never the frames of the host, so a
    /// faster screen plays a fight in the same time (D-266). The rules never read the pace: a
    /// command of the player waits for it, and each command is an intent of the record (D-522, T-7).
    /// </remarks>
    private void PlayBattleEvents(List<LogEntry> log)
    {
        this.FollowBattle();
        this.handOff.Follow(this.FightTick);
        if (this.handOff.HoldsEvents)
        {
            return;
        }

        while (this.PlayedOut && !this.events.Empty)
        {
            BattleEvent played = this.events.PlayNext();
            if (played.Kind == BattleEventKind.Started)
            {
                this.view = BattleView.AtStart(this.simulation.State, this.startParty ?? throw new InvalidOperationException(
                    $"The screen plays the start of a fight at tick {this.simulation.Tick}, and the run kept no party of the tick before it (D-776, T-2)."));
            }

            BattleView shown = this.view ?? throw new InvalidOperationException(
                $"The screen plays the battle event '{played.Describe()}' at tick {this.simulation.Tick}, "
                + "and no view of the fight exists, because no start event came first (D-532, T-2).");

            shown.Apply(played);
            this.playing = played;
            this.playingSince = this.FightTick;
            log.Add(new LogEntry(
                LogLevel.Info,
                "the screen played a battle event",
                this.simulation.Tick,
                LogSubsystems.Battle,
                [new LogField("event", played.Describe())]));
        }

        if (!this.PlayedOut
            || !this.events.Empty
            || this.simulation.State.Battle is not Battle battle
            || (battle.Outcome != BattleOutcome.Won && battle.Outcome != BattleOutcome.Fled))
        {
            return;
        }

        // The map fades back in before the world runs again, so the wait intent goes out when the
        // fade ends (D-522, D-938). A fight that a load resumed had no transition, so it fades back too.
        if (this.handOff.Phase == HandOffPhase.None)
        {
            this.handOff.StartBack(this.FightTick);
            log.Add(this.HandOffEntry("the screen started the fade back to the map", LogEntry.NoFields));
            return;
        }

        if (this.handOff.Phase != HandOffPhase.Waiting)
        {
            return;
        }

        // A frame can run no tick, and the queue then still holds the wait intent of an
        // earlier frame. A second one in one tick meets the refusal of the rules (T-2). The fight
        // also ends under no pause, because the map would come back held with no window (D-1083).
        if (this.Queued(IntentIds.WaitBattleEnd) || this.MenuOpenNextTick)
        {
            return;
        }

        this.queued.Add(Intent.OfPlayer(IntentIds.WaitBattleEnd));
    }

    /// <summary>Tells whether the queue of this frame holds an intent of one action.</summary>
    private bool Queued(ContentId action)
    {
        foreach (Intent waiting in this.queued)
        {
            if (string.CompareOrdinal(waiting.Action.Value, action.Value) == 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Drops the view when the fight ended, and builds it from the state when a fight runs
    /// with no event to play, as after the load of a save inside a fight (D-531).
    /// </summary>
    private void FollowBattle()
    {
        if (this.simulation.State.Battle is null)
        {
            this.LeaveFight();
            return;
        }

        if (this.view is null && this.events.Empty)
        {
            this.view = BattleView.Of(this.simulation.State);
        }
    }

    /// <summary>Drops the view of a fight that left the run, and ends the hand-off, so a new fight starts clean (D-522, D-939).</summary>
    private void LeaveFight()
    {
        this.view = null;
        this.playing = null;
        this.handOff.End(this.FightTick);
    }

    /// <summary>Tells whether the events of one tick start a fight, which a transition leads into (D-939).</summary>
    private static bool StartsFight(IReadOnlyList<BattleEvent> events)
    {
        foreach (BattleEvent taken in events)
        {
            if (taken.Kind == BattleEventKind.Started)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Picks the transition of the fight that started on this tick, and starts it (D-934, D-935,
    /// D-937). The kind comes from the boss flag of the group, the side of the encounter, and the
    /// size of its patrol.
    /// </summary>
    /// <returns>The log entry of the start, with the kind and the transition.</returns>
    /// <exception cref="InvalidOperationException">The run holds no fight or no encounter, or no patrol has the id of the encounter (T-2).</exception>
    private LogEntry StartTransition()
    {
        RunState state = this.simulation.State;
        long tick = this.simulation.Tick;
        Battle battle = state.Battle ?? throw new InvalidOperationException(
            $"A start event came at tick {tick}, and the run holds no fight (D-532, T-2).");
        MapEncounter encounter = state.Party.Patrols.Encounter ?? throw new InvalidOperationException(
            $"The fight of the group '{battle.Group.Id.Value}' started at tick {tick}, and the map holds no encounter (D-531, T-2).");

        EncounterKind kind = EncounterKinds.Of(battle.Group.Boss, encounter.Behind, SizeOf(state.Party.Patrols, encounter, tick));
        Transition picked = this.transitions.Pick(kind, state.Party.Map.Id, state.Seed, tick, this.lastCommon);
        if (kind == EncounterKind.Common)
        {
            this.lastCommon = picked.Id;
        }

        this.handOff.StartInto(picked, this.FightTick);
        return this.HandOffEntry(
            "the screen started the transition into a fight",
            [new LogField("kind", EncounterKinds.NameOf(kind)), new LogField("transition", picked.Id.Value)]);
    }

    /// <summary>Gives the size of the patrol of an encounter, which the kind of the encounter reads (D-937).</summary>
    private static EnemySize SizeOf(MapPatrols patrols, MapEncounter encounter, long tick)
    {
        foreach (PatrolState patrol in patrols.All)
        {
            if (string.CompareOrdinal(patrol.Patrol.Id.Value, encounter.Enemy.Value) == 0)
            {
                return patrol.Patrol.Size;
            }
        }

        throw new InvalidOperationException(
            $"The encounter at tick {tick} names the patrol '{encounter.Enemy.Value}', and the map holds no such patrol (D-752, T-2).");
    }

    /// <summary>Gives the log entry of a phase of the hand-off (D-179).</summary>
    private LogEntry HandOffEntry(string message, IReadOnlyList<LogField> fields) =>
        new(LogLevel.Info, message, this.simulation.Tick, LogSubsystems.Game, fields);

    /// <summary>Gives what the notice box shows at the tick of the world now (D-994, D-995).</summary>
    /// <param name="charactersPerSecond">The text speed of the settings (D-864).</param>
    /// <returns>The frame of the notice on screen, or no value when none shows.</returns>
    /// <remarks>
    /// A notice counts the ticks of the world, so it waits while a menu pauses the world (D-995).
    /// </remarks>
    public NoticeFrame? NoticeAt(int charactersPerSecond) => this.notices.FrameAt(this.simulation.State.WorldTick, charactersPerSecond);

    /// <summary>Gives the record of the run as it stands now (G-5, D-651).</summary>
    /// <returns>The record, which the crash file of D-170 carries.</returns>
    public RunRecord Record() => this.recorder.Build();

    /// <summary>Gives the state hash of the run, which a replay compares (G-5).</summary>
    /// <returns>The hash.</returns>
    public ulong StateHash() => this.simulation.StateHash();
}
