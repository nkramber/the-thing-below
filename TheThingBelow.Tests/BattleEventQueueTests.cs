using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Saves;
using TheThingBelow.Storage;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The event queue of Game, which is the input gate of a battle, and the run of Game through a
/// whole battle (D-532, D-776). The tests read the built Game assembly, because Tests takes no
/// reference to Game (D-614).
/// </summary>
public sealed class BattleEventQueueTests
{
    private const ulong Seed = 20260918;

    private const double OneTick = 1.0 / 60;

    /// <summary>The most frames that a run of Game walks or fights before a test fails (T-2).</summary>
    private const int FrameLimit = 6000;

    private static readonly Lazy<ContentSet> Content =
        new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    [Fact]
    public void TheEventQueueAlwaysDrainsOverOneThousandSeeds()
    {
        // Exit test 6 of PR-9 (D-532): the screen plays one event on each frame, so a queue of
        // any size empties, and the next command then goes through.
        Type type = GameAssemblyFile.Type("TheThingBelow.Game.BattleEventQueue");
        MethodInfo add = type.GetMethod("Add", [typeof(IReadOnlyList<BattleEvent>)]) ?? throw Absent("Add");
        MethodInfo play = type.GetMethod("PlayNext", Type.EmptyTypes) ?? throw Absent("PlayNext");
        PropertyInfo empty = type.GetProperty("Empty") ?? throw Absent("Empty");

        for (ulong seed = 0; seed < 1000; seed += 1)
        {
            object queue = Activator.CreateInstance(type) ?? throw Absent("the constructor");
            int waiting = 0;
            for (int frame = 0; frame < (int)(seed % 13); frame += 1)
            {
                int count = (int)((seed + (ulong)frame) % 5);
                add.Invoke(queue, [EventsOf(count)]);
                waiting += count;
                if (!(bool)empty.GetValue(queue)!)
                {
                    play.Invoke(queue, Type.EmptyTypes);
                    waiting -= 1;
                }
            }

            int frames = 0;
            while (!(bool)empty.GetValue(queue)!)
            {
                play.Invoke(queue, Type.EmptyTypes);
                frames += 1;
            }

            Assert.True(frames == waiting, $"Seed {seed}: the queue drained in {frames} frames, and it held {waiting} events (D-532).");
        }
    }

    [Fact]
    public void APlayOfAnEmptyQueueIsAnError()
    {
        Type type = GameAssemblyFile.Type("TheThingBelow.Game.BattleEventQueue");
        object queue = Activator.CreateInstance(type) ?? throw Absent("the constructor");
        MethodInfo play = type.GetMethod("PlayNext", Type.EmptyTypes) ?? throw Absent("PlayNext");

        TargetInvocationException error = Assert.Throws<TargetInvocationException>(() => play.Invoke(queue, Type.EmptyTypes));

        Assert.IsType<InvalidOperationException>(error.InnerException);
    }

    [Fact]
    public void GameFightsABattleOfTheFirstMapToItsEndAndTheMapRunsAgain()
    {
        // D-532, D-767: the run of Game walks into the hall patrol, takes each command when
        // the queue is empty alone, plays every event, and sends the wait intent itself.
        GameRunView run = GameRunView.Start();
        int frame = 0;
        bool played = false;

        for (; frame < FrameLimit && !run.InBattle; frame += 1)
        {
            run.Queue(Intent.OfPlayer(WalkOf(run.Party)));
            run.Advance(OneTick);
        }

        Assert.True(run.InBattle, "The walk of the first map met no enemy (D-767).");
        for (; frame < FrameLimit && run.InBattle && !run.WipeReady; frame += 1)
        {
            if (run.TakesBattleCommand)
            {
                Assert.Equal(0, run.WaitingEvents);
                Battle battle = run.State.Battle!;
                run.Queue(Intent.OfPlayer(IntentIds.BattleAttack, battle.MeleeTargets(BattleSide.Enemy)[0].Target, null));
            }

            foreach (LogEntry entry in run.Advance(OneTick))
            {
                played |= entry.Subsystem == LogSubsystems.Battle && entry.Message.Contains("played a battle event", StringComparison.Ordinal);
            }
        }

        Assert.True(played, "Game played no battle event (D-532).");
        Assert.True(!run.InBattle || run.WipeReady, $"The battle reached no end in {FrameLimit} frames (T-2).");
        Assert.Equal(0, run.WaitingEvents);
    }

    [Fact]
    public void AConfirmSkipEndsTheHoldOfTheEventThatPlays()
    {
        // D-866: a press of confirm shows the next battle message. The skip changes no state
        // of the rules, so the tick and the record stay as they are.
        GameRunView run = GameRunView.Start();
        int frame = 0;
        for (; frame < FrameLimit && (run.PlayingEvent is null || run.PlayingTicks > 0); frame += 1)
        {
            if (!run.InBattle)
            {
                run.Queue(Intent.OfPlayer(WalkOf(run.Party)));
            }

            run.Advance(OneTick);
        }

        Assert.NotNull(run.PlayingEvent);
        long tick = run.Tick;

        Assert.True(run.SkipPlayingEvent());
        Assert.False(run.SkipPlayingEvent());
        Assert.Equal(tick, run.Tick);
    }

    [Fact]
    public void EachEventPlaysAllItsTicksAndTheMenuWaitsForTheLast()
    {
        // D-532, D-829: the next event starts when the one before played its ticks, and the
        // command of the player waits until the last one ends. At each command the view of
        // the screen matches the state of the rules.
        GameRunView run = GameRunView.Start();
        int frame = 0;
        for (; frame < FrameLimit && !run.InBattle; frame += 1)
        {
            run.Queue(Intent.OfPlayer(WalkOf(run.Party)));
            run.Advance(OneTick);
        }

        MethodInfo ticksOf = GameAssemblyFile.Type("TheThingBelow.Game.Ui.BattleTimes").GetMethod("TicksOf")!;
        BattleEvent? before = null;
        int beforeTicks = 0;
        int changes = 0;
        for (; frame < FrameLimit && run.InBattle && !run.WipeReady; frame += 1)
        {
            BattleEvent? playing = run.PlayingEvent;
            if (playing is not null && before is not null && !ReferenceEquals(playing, before))
            {
                changes += 1;
                int needed = (int)ticksOf.Invoke(null, [before.Kind])!;
                Assert.True(beforeTicks >= needed - 1, $"The event '{before.Kind}' played {beforeTicks + 1} ticks, and it needs {needed}.");
            }

            if (run.TakesBattleCommand)
            {
                Assert.True(playing is null || run.PlayingTicks >= (int)ticksOf.Invoke(null, [playing.Kind])!, "The menu opened while an event played.");
                AssertViewMatches(run);
                Battle battle = run.State.Battle!;
                run.Queue(Intent.OfPlayer(IntentIds.BattleAttack, battle.MeleeTargets(BattleSide.Enemy)[0].Target, null));
            }

            before = playing;
            beforeTicks = run.PlayingTicks;
            run.Advance(OneTick);
        }

        Assert.True(changes > 5, $"The fight played {changes} events.");
    }

    [Fact]
    public void GameSendsTheWaitIntentAfterAFleeAndTheMapRunsAgain()
    {
        // D-522, D-532: the queue drains after the flee, Game sends the wait intent itself, and
        // the grace time of the enemy starts (D-381).
        GameRunView run = GameRunView.Start();
        int frame = 0;
        for (; frame < FrameLimit && !run.InBattle; frame += 1)
        {
            run.Queue(Intent.OfPlayer(WalkOf(run.Party)));
            run.Advance(OneTick);
        }

        for (; frame < FrameLimit && run.InBattle && !run.WipeReady; frame += 1)
        {
            if (run.TakesBattleCommand)
            {
                run.Queue(Intent.OfPlayer(IntentIds.BattleFlee));
            }

            run.Advance(OneTick);
        }

        Assert.False(run.InBattle, "The flees of the fixture seed ended in no flee (D-378).");
        Assert.Null(run.State.Battle);
        Assert.Contains(run.Party.Patrols.All, patrol => patrol.GraceTicks > 0);
    }

    [Fact]
    public void AReloadWithNoSaveStartsANewRun()
    {
        // D-776: no save exists before PR-16, so a wipe starts the run again from its start.
        GameRunView run = GameRunView.Reload(null, null);

        Assert.Equal(0, run.Tick);
        Assert.False(run.InBattle);
    }

    [Fact]
    public void AReloadResumesTheNewerSave()
    {
        // D-231: the later tick of the run wins.
        RunSnapshot start = Simulation.Start(Seed, Content.Value.Map(MapIds.FirstMap), Content.Value.Battle, DebugIntentHandlers.None).Snapshot();
        SaveDocument older = new(SaveHeader.ForThisBuild(Content.Value.Hash, Seed), start with { Tick = 40 });
        SaveDocument newer = new(SaveHeader.ForThisBuild(Content.Value.Hash, Seed), start with { Tick = 90 });

        GameRunView run = GameRunView.Reload(older, newer);

        Assert.Equal(90, run.Tick);
    }

    /// <summary>The walk of the smoke session: east along the hall, then south, then west (D-767).</summary>
    private static ContentId WalkOf(MapState party)
    {
        if (party.LeadAt.X < 26 && party.LeadAt.Y < 7)
        {
            return IntentIds.MoveEast;
        }

        return party.LeadAt.Y < 7 ? IntentIds.MoveSouth : IntentIds.MoveWest;
    }

    private static List<BattleEvent> EventsOf(int count)
    {
        List<BattleEvent> events = [];
        for (int index = 0; index < count; index += 1)
        {
            events.Add(new BattleEvent(BattleEventKind.Hit, new BattleTarget(BattleSide.Party, 0), new BattleTarget(BattleSide.Enemy, index), index + 1));
        }

        return events;
    }

    /// <summary>Checks that the view of the screen shows the health of each combatant of the state.</summary>
    private static void AssertViewMatches(GameRunView run)
    {
        object view = run.BattleView ?? throw new InvalidOperationException("The run holds no view of the fight.");
        Battle battle = run.State.Battle!;
        foreach ((string side, IReadOnlyList<Combatant> combatants) in new[] { ("Party", battle.Party), ("Enemies", battle.Enemies) })
        {
            IList shown = (IList)view.GetType().GetProperty(side)!.GetValue(view)!;
            for (int slot = 0; slot < combatants.Count; slot += 1)
            {
                int health = (int)shown[slot]!.GetType().GetProperty("Health")!.GetValue(shown[slot])!;
                Assert.Equal(combatants[slot].Health, health);
            }
        }
    }

    private static InvalidOperationException Absent(string member) =>
        new($"The Game assembly holds no '{member}' of the battle event queue (T-2).");

    /// <summary>The run of the built Game assembly, through its public members.</summary>
    private sealed class GameRunView
    {
        private readonly Type type;
        private readonly object instance;

        private GameRunView(Type type, object instance)
        {
            this.type = type;
            this.instance = instance;
        }

        public long Tick => (long)this.Read("Tick");

        public bool SkipPlayingEvent() => (bool)this.type.GetMethod("SkipPlayingEvent")!.Invoke(this.instance, null)!;

        public bool InBattle => (bool)this.Read("InBattle");

        public bool TakesBattleCommand => (bool)this.Read("TakesBattleCommand");

        public bool WipeReady => (bool)this.Read("WipeReady");

        public int WaitingEvents => (int)this.Read("WaitingEvents");

        public RunState State => (RunState)this.Read("State");

        public MapState Party => (MapState)this.Read("Party");

        public BattleEvent? PlayingEvent => (BattleEvent?)this.type.GetProperty("PlayingEvent")!.GetValue(this.instance);

        public int PlayingTicks => (int)this.Read("PlayingTicks");

        public object? BattleView => this.type.GetProperty("BattleView")!.GetValue(this.instance);

        public static GameRunView Start()
        {
            Type type = GameAssemblyFile.Type("TheThingBelow.Game.GameRun");
            MethodInfo start = type.GetMethod("Start", [typeof(ContentSet), typeof(ulong), typeof(DebugIntentHandlers), typeof(MessageSpeed)])
                ?? throw new InvalidOperationException("The run holds no 'Start' method (T-2).");
            return new GameRunView(type, start.Invoke(null, [Content.Value, Seed, DebugIntentHandlers.None, MessageSpeed.Normal])!);
        }

        public static GameRunView Reload(SaveDocument? slot, SaveDocument? autosave)
        {
            Type type = GameAssemblyFile.Type("TheThingBelow.Game.GameRun");
            MethodInfo reload = type.GetMethod("Reload", [typeof(ContentSet), typeof(SaveDocument), typeof(SaveDocument), typeof(ulong), typeof(DebugIntentHandlers), typeof(MessageSpeed)])
                ?? throw new InvalidOperationException("The run holds no 'Reload' method (T-2).");
            return new GameRunView(type, reload.Invoke(null, [Content.Value, slot, autosave, Seed, DebugIntentHandlers.None, MessageSpeed.Normal])!);
        }

        public void Queue(Intent intent)
        {
            MethodInfo queue = this.type.GetMethod("Queue", [typeof(Intent)])
                ?? throw new InvalidOperationException("The run holds no 'Queue' method (T-2).");
            queue.Invoke(this.instance, [intent]);
        }

        public IReadOnlyList<LogEntry> Advance(double seconds)
        {
            // A test holds no key, so no tick takes a held step (D-820).
            MethodInfo advance = this.type.GetMethod("Advance", [typeof(double), typeof(Func<Intent>)])
                ?? throw new InvalidOperationException("The run holds no 'Advance' method (T-2).");
            return (IReadOnlyList<LogEntry>)advance.Invoke(this.instance, [seconds, null])!;
        }

        private object Read(string name)
        {
            PropertyInfo property = this.type.GetProperty(name)
                ?? throw new InvalidOperationException($"The run holds no '{name}' value (T-2).");
            return property.GetValue(this.instance)
                ?? throw new InvalidOperationException($"The run gave no value for '{name}' (T-2).");
        }
    }
}
