using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheThingBelow.Core;
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
/// The run of Game: the fixed-step clock, the simulation, and the recorder of the run (D-164,
/// G-5). The tests read the built Game assembly, because Tests takes no reference to Game
/// (D-614).
/// </summary>
public sealed class GameRunTests
{
    /// <summary>The name of the type of Game that holds the run.</summary>
    private const string RunTypeName = "TheThingBelow.Game.GameRun";

    /// <summary>The seed of the runs of these tests.</summary>
    private const ulong Seed = 20260918;

    /// <summary>The time of one tick, in seconds.</summary>
    private const double OneTick = 1.0 / 60;

    private static readonly Lazy<ContentSet> Content =
        new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    [Fact]
    public void AQueuedOpenMenuPausesTheWorldForTheNextTick()
    {
        // A regression test of the crash that a press of the menu button with a direction
        // held would give. The host reads input before it runs the ticks of a frame, so the
        // queue holds the open-menu intent while the menu state of the run is still closed.
        // A host that read that state alone would queue a step for the same tick, and the
        // rules refuse a step while a menu is open (D-162, T-2).
        Run run = Run.Start();

        run.Queue(Intent.OfPlayer(IntentIds.OpenMenu));

        Assert.False(run.MenuOpen);
        Assert.True(run.MenuOpenNextTick);
    }

    [Fact]
    public void AQueuedCloseMenuRunsTheWorldAgainOnTheNextTick()
    {
        Run run = Run.Start();
        run.Queue(Intent.OfPlayer(IntentIds.OpenMenu));
        run.Advance(OneTick);
        run.Queue(Intent.OfPlayer(IntentIds.CloseMenu));

        Assert.True(run.MenuOpen);
        Assert.False(run.MenuOpenNextTick);
    }

    [Fact]
    public void AChoiceOfThePartyWindowReachesTheRecordAsOneIntentAndNoCursorMove()
    {
        // Exit test 3 of PR-62 (D-493): the cursor moves make no intent, and the record holds the
        // open, the row change, and the close of the menu alone.
        Run run = Run.Start();
        GameValue list = GameValue.New("PartyList", 1);
        run.Queue(Intent.OfPlayer(IntentIds.OpenMenu));
        list.Call("Move", 1);
        list.Call("Move", -1);
        run.Queue((Intent)list.Call("Choose")!);
        run.Queue(Intent.OfPlayer(IntentIds.CloseMenu));

        run.Advance(OneTick);

        List<string> recorded = [];
        foreach (Intent intent in run.Record().Ticks[0].Intents)
        {
            recorded.Add(intent.Describe());
        }

        Assert.Equal(["intent.open_menu", "intent.party_row at party 0", "intent.close_menu"], recorded);
        Assert.Equal(Core.Battles.BattleRow.Back, run.State.Characters.Members[0].Row);
    }

    [Fact]
    public void TheWorldWaitsWhileAMenuIsOpenAndTheTickRises()
    {
        // Exit test 5 of PR-62 (D-162, D-650).
        Run run = Run.Start();
        run.Advance(OneTick);
        long world = run.State.WorldTick;
        run.Queue(Intent.OfPlayer(IntentIds.OpenMenu));

        for (int tick = 0; tick < 10; tick += 1)
        {
            run.Advance(OneTick);
        }

        Assert.Equal(11, run.Tick);
        Assert.Equal(world, run.State.WorldTick);
    }

    [Fact]
    public void ANoticeWaitsWithTheWorldWhileAMenuIsOpen()
    {
        // D-995: the notice box counts the ticks of the world, so a menu stops the notice where it stands.
        Run run = Run.Start(DebugAssemblyFile.Handlers());
        run.Queue(Intent.OfDebugConsole(ContentId.Parse("debug.notice_logged", "test", "debug")));
        run.Advance(OneTick);
        run.Advance(OneTick);
        object before = run.NoticeAt(60)!;
        run.Queue(Intent.OfPlayer(IntentIds.OpenMenu));

        for (int tick = 0; tick < 30; tick += 1)
        {
            run.Advance(OneTick);
        }

        Assert.Equal(before, run.NoticeAt(60));
        Assert.Contains("notice.fixture_mark", before.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void AnEmptyQueueLeavesTheMenuStateOfTheRun()
    {
        Run run = Run.Start();

        Assert.Equal(run.MenuOpen, run.MenuOpenNextTick);
    }

    [Fact]
    public void AFrameOfOneTickRecordsThatTick()
    {
        Run run = Run.Start();

        run.Advance(OneTick);

        Assert.Equal(1, run.Tick);
        Assert.Equal(1, run.Record().EndTick);
    }

    [Fact]
    public void AHeldStepReachesEachTickOfAFrameOfTwoTicks()
    {
        // D-820. The held step once reached the first tick of a frame alone, so the party
        // stood still for a tick when it reached a tile on the second tick of a frame.
        Run run = Run.Start();
        Intent step = run.IntentOf("step_north");

        run.Advance(OneTick * 2.5, () => step);

        IReadOnlyList<TickIntents> ticks = run.Record().Ticks;
        Assert.Equal(2, ticks.Count);
        foreach (TickIntents tick in ticks)
        {
            Assert.Contains(tick.Intents, intent => string.CompareOrdinal(intent.Action.Value, step.Action.Value) == 0);
        }
    }

    [Fact]
    public void AHeldStepThatGivesNothingAddsNoIntent()
    {
        Run run = Run.Start();

        run.Advance(OneTick, () => null);

        // The record holds a line for a tick with an intent alone.
        Assert.Equal(1, run.Record().EndTick);
        Assert.Empty(run.Record().Ticks);
    }

    [Fact]
    public void TheRecordOfACrashHoldsTheIntentsOfTheTickThatCrashed()
    {
        // A regression test for the audit of 2026-09-20. The run recorded each tick after
        // the step, so the record of a refused intent ended one tick before the crash, and a
        // replay of the crash file never reached the crash (D-170, G-5).
        Run run = Run.Start();
        run.Queue(Intent.OfPlayer(IntentIds.CloseMenu));

        TargetInvocationException thrown = Assert.Throws<TargetInvocationException>(() => run.Advance(OneTick));
        Assert.IsType<SimulationException>(thrown.InnerException);

        RunRecord record = run.Record();
        Assert.Equal(1, run.Tick);
        Assert.Equal(1, record.EndTick);
        TickIntents line = Assert.Single(record.Ticks);
        Assert.Equal(IntentIds.CloseMenu, line.Intents[0].Action);

        SimulationException replay = Assert.Throws<SimulationException>(
            () => RunReplay.Play(record, record.Header.ContentHash, TestMaps.FixtureDungeon, Content.Value.Battle, Content.Value.Notices, Content.Value.Story, DebugIntentHandlers.None));
        Assert.Equal(1, replay.Context.Tick);
    }

    [Fact]
    public void AFrameThatDropsTicksLogsTheCount()
    {
        // No work of a step goes in silence (T-2). A frame of two seconds gives 120 ticks,
        // and the loop runs its maximum and drops the rest.
        Run run = Run.Start();

        IReadOnlyList<LogEntry> log = run.Advance(2.0);

        LogEntry entry = Assert.Single(log, item => item.Level == LogLevel.Error);
        Assert.Equal(LogSubsystems.Game, entry.Subsystem);
        LogField field = Assert.Single(entry.Fields);
        Assert.Equal("dropped", field.Name);
        Assert.Equal("112", field.Value);
    }

    [Fact]
    public void ASaveDropsTheIntentsBeforeItSoTheRecordOfAHeldWalkStaysBounded()
    {
        // F-10, D-651, D-1115: a held direction gives one line for each tick, and only a save
        // drops the lines before it. No caller took a save before PR-105.
        Run run = Run.Start();
        Intent step = run.IntentOf("step_north");
        for (int frame = 0; frame < 600; frame += 1)
        {
            run.Advance(OneTick, () => step);
        }

        Assert.Equal(600, run.RecordedLines);

        SaveDocument save = run.Save();

        Assert.Equal(0, run.RecordedLines);
        Assert.Equal(600, save.Snapshot.Tick);
        Assert.Equal((Seed, Content.Value.Hash, SaveFormat.Current), (save.Header.Seed, save.Header.ContentHash, save.Header.FormatVersion));
        Assert.Equal(600, run.Record().Snapshot.Tick);

        // The boundary: the ticks after the save take their lines again.
        for (int frame = 0; frame < 10; frame += 1)
        {
            run.Advance(OneTick, () => step);
        }

        Assert.Equal(10, run.RecordedLines);
        Assert.Equal(610, run.Record().EndTick);
    }

    [Fact]
    public void AReloadDropsASaveOfAnotherRunAndStartsTheRunAgain()
    {
        // D-1114: the slot save of another seed holds a later tick, and the wipe never loads it.
        Run played = Run.Start();
        for (int frame = 0; frame < 30; frame += 1)
        {
            played.Advance(OneTick);
        }

        SaveDocument other = played.Save() with { Header = played.Save().Header with { Seed = Seed + 1 } };
        List<LogEntry> log = [];

        Run reloaded = Run.Reload(other, null, Seed, log);

        Assert.Equal(0, reloaded.Tick);
        Assert.Empty(log);
    }

    [Fact]
    public void AReloadOfASaveOfAnotherBuildLogsEachChangeOfItsMap()
    {
        // D-1111, D-1113: a patch added an enemy to the map, so the save lacks it. The resume of
        // another build places it on its station and logs a warning, and the same save of this
        // build fails as before.
        Run played = Run.Start();
        played.Advance(OneTick);
        SaveDocument save = played.Save();
        MapSnapshot map = save.Snapshot.Map!;
        SaveDocument lacking = save with { Snapshot = save.Snapshot with { Map = map with { Enemies = [.. map.Enemies!.Skip(1)] } } };
        SaveDocument otherBuild = lacking with { Header = lacking.Header with { ContentHash = "an-older-content-hash" } };
        List<LogEntry> log = [];

        Run reloaded = Run.Reload(otherBuild, null, Seed, log);

        Assert.Equal(1, reloaded.Tick);
        LogEntry entry = Assert.Single(log);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Contains("the save lacks", entry.Message, StringComparison.Ordinal);
        TargetInvocationException refused = Assert.Throws<TargetInvocationException>(() => Run.Reload(lacking, null, Seed, []));
        Assert.IsType<ArgumentException>(refused.InnerException);
    }

    /// <summary>The run of the built Game assembly, through its public members.</summary>
    [Fact]
    public void TheMenuActionOpensTheMenuAndThenClosesIt()
    {
        // The regression test of P1-1 of `docs/reviews/pr-41.md`. The host asks the run for
        // the intent of an action, and the run reads its own menu state (D-162, D-650).
        Run run = Run.Start();

        Intent first = run.IntentOf("menu");
        run.Queue(first);
        run.Advance(OneTick);

        Intent second = run.IntentOf("menu");
        run.Queue(second);
        run.Advance(OneTick);

        Assert.Equal(IntentIds.OpenMenu.Value, first.Action.Value);
        Assert.Equal(IntentIds.CloseMenu.Value, second.Action.Value);
        Assert.False(run.MenuOpen);
    }

    [Fact]
    public void TheMenuStateOfTheRunFollowsTheIntents()
    {
        // D-650. The state of the run is the one source of the menu state, so no host copy
        // can drift from it (T-2).
        Run run = Run.Start();
        Assert.False(run.MenuOpen);

        run.Queue(Intent.OfPlayer(IntentIds.OpenMenu));
        run.Advance(OneTick);
        Assert.True(run.MenuOpen);

        run.Queue(Intent.OfPlayer(IntentIds.CloseMenu));
        run.Advance(OneTick);
        Assert.False(run.MenuOpen);
    }

    [Fact]
    public void EveryOtherActionMakesOneIntentWhateverTheMenuDoes()
    {
        // D-493. The menu action alone reads the menu state.
        Run run = Run.Start();
        Intent shut = run.IntentOf("confirm");

        run.Queue(Intent.OfPlayer(IntentIds.OpenMenu));
        run.Advance(OneTick);

        Assert.True(run.MenuOpen);
        Assert.Equal(shut.Action.Value, run.IntentOf("confirm").Action.Value);
        Assert.Equal(IntentIds.Confirm.Value, shut.Action.Value);
    }

    [Fact]
    public void TheTorchActionHoldsTheTorchOutAndThenPutsItAway()
    {
        // D-1064, D-1068: one button holds the torch out and puts it away. The run reads the
        // queued intents, so a second press before the tick puts the torch away again.
        Run run = Run.Start();
        Assert.False(run.State.Characters.TorchHeld);

        Intent first = run.IntentOf("torch");
        run.Queue(first);
        Assert.Equal(IntentIds.PutTorchAway.Value, run.IntentOf("torch").Action.Value);
        run.Advance(OneTick);

        Assert.Equal(IntentIds.HoldTorch.Value, first.Action.Value);
        Assert.True(run.State.Characters.TorchHeld);
        Assert.Equal(IntentIds.PutTorchAway.Value, run.IntentOf("torch").Action.Value);
    }

    [Fact]
    public void TheTorchWorksOnTheWalkAloneWithTheTorchInThePack()
    {
        // D-1071: the host sends the torch intent on the walk alone, and the fixture pack holds the torch.
        Run run = Run.Start();
        Assert.True(run.TorchWorks);

        run.Queue(Intent.OfPlayer(IntentIds.OpenMenu));

        Assert.False(run.TorchWorks);
    }

    [Fact]
    public void TheCommandOfTheHubCapturePutsThePartyOnTheHubAndTheFrameFallsInsideTheFirstStepOfTheBarmaid()
    {
        // Exit test 18 of PR-14 (D-1133): the capture of the hub and the smoke session type the
        // command of `ScreenCaptures`, and the frame after `HubTicks` shows the barmaid inside her
        // first step, past her wait at the bar, and the lead on the spawn point (D-203, D-1138).
        Type captures = GameAssemblyFile.Type("TheThingBelow.Game.ScreenCaptures");
        string command = (string)captures.GetField("GoToCommand")!.GetRawConstantValue()!;
        string hub = (string)captures.GetField("HubMap")!.GetRawConstantValue()!;
        int hubTicks = (int)captures.GetField("HubTicks")!.GetRawConstantValue()!;
        Run run = Run.Start(DebugAssemblyFile.Handlers());

        _ = DebugAssemblyFile.Run($"{command} {hub}", () => run.State, run.Queue);
        run.Advance(OneTick);

        MapState party = run.State.Party;
        Assert.Equal(hub, party.Map.Id.Value);
        Assert.Equal(MapKind.Hub, party.Map.Kind);
        for (int tick = 0; tick < hubTicks; tick += 1)
        {
            Assert.DoesNotContain(run.Advance(OneTick), entry => entry.Level == LogLevel.Error);
        }

        NpcState barmaid = run.State.Party.Npcs.All.Single(npc => npc.Npc.Id.Value == "npc.fixture_hub_barmaid");
        Assert.NotNull(barmaid.Stepping);
        Assert.Equal(barmaid.Npc.Route[0].At, barmaid.At);
        Assert.Equal(party.Map.Spawn, run.State.Party.LeadAt);
    }

    private sealed class Run
    {
        private readonly object instance;
        private readonly MethodInfo queue;
        private readonly MethodInfo advance;
        private readonly MethodInfo record;
        private readonly MethodInfo intentOf;
        private readonly PropertyInfo tick;
        private readonly PropertyInfo menuOpen;
        private readonly PropertyInfo menuOpenNextTick;

        private Run(Type type, object instance)
        {
            this.instance = instance;
            this.queue = type.GetMethod("Queue", [typeof(Intent)])
                ?? throw new InvalidOperationException("The run holds no 'Queue' method (T-2).");
            this.advance = type.GetMethod("Advance", [typeof(double), typeof(Func<Intent>)])
                ?? throw new InvalidOperationException("The run holds no 'Advance' method (T-2).");
            this.record = type.GetMethod("Record", Type.EmptyTypes)
                ?? throw new InvalidOperationException("The run holds no 'Record' method (T-2).");
            this.intentOf = type.GetMethod("IntentOf", [typeof(string)])
                ?? throw new InvalidOperationException("The run holds no 'IntentOf' method (T-2).");
            this.tick = type.GetProperty("Tick")
                ?? throw new InvalidOperationException("The run holds no 'Tick' value (T-2).");
            this.menuOpen = type.GetProperty("MenuOpen")
                ?? throw new InvalidOperationException("The run holds no 'MenuOpen' value (T-2).");
            this.menuOpenNextTick = type.GetProperty("MenuOpenNextTick")
                ?? throw new InvalidOperationException("The run holds no 'MenuOpenNextTick' value (T-2).");
        }

        public long Tick => (long)(this.tick.GetValue(this.instance)
            ?? throw new InvalidOperationException("The tick has no value (T-2)."));

        public bool MenuOpenNextTick => (bool)(this.menuOpenNextTick.GetValue(this.instance)
            ?? throw new InvalidOperationException("The menu state of the next tick has no value (T-2)."));

        public bool MenuOpen => (bool)(this.menuOpen.GetValue(this.instance)
            ?? throw new InvalidOperationException("The menu state has no value (T-2)."));

        public Intent IntentOf(string action) =>
            (Intent)(this.intentOf.Invoke(this.instance, [action])
                ?? throw new InvalidOperationException("The 'IntentOf' method gave nothing (T-2)."));

        public RunState State => (RunState)(this.instance.GetType().GetProperty("State")!.GetValue(this.instance)
            ?? throw new InvalidOperationException("The run holds no state (T-2)."));

        public bool TorchWorks => (bool)(this.instance.GetType().GetProperty("TorchWorks")!.GetValue(this.instance)
            ?? throw new InvalidOperationException("The torch gate has no value (T-2)."));

        public object? NoticeAt(int charactersPerSecond) =>
            this.instance.GetType().GetMethod("NoticeAt")!.Invoke(this.instance, [charactersPerSecond]);

        public int RecordedLines => (int)(this.instance.GetType().GetProperty("RecordedLines")!.GetValue(this.instance)
            ?? throw new InvalidOperationException("The run holds no count of recorded lines (T-2)."));

        public static Run Start() => Start(DebugIntentHandlers.None);

        public static Run Reload(SaveDocument? slot, SaveDocument? autosave, ulong seed, List<LogEntry> log)
        {
            Type type = GameAssemblyFile.Type(RunTypeName);
            MethodInfo reload = type.GetMethod(
                "Reload",
                [typeof(ContentSet), typeof(SaveDocument), typeof(SaveDocument), typeof(ulong), typeof(DebugIntentHandlers), typeof(MessageSpeed), typeof(List<LogEntry>)])
                ?? throw new InvalidOperationException("The run holds no 'Reload' method (T-2).");
            object instance = reload.Invoke(null, [Content.Value, slot, autosave, seed, DebugIntentHandlers.None, MessageSpeed.Normal, log])
                ?? throw new InvalidOperationException("The 'Reload' method gave no run (T-2).");
            return new Run(type, instance);
        }

        public SaveDocument Save() =>
            (SaveDocument)(this.instance.GetType().GetMethod("Save", Type.EmptyTypes)!.Invoke(this.instance, Type.EmptyTypes)
                ?? throw new InvalidOperationException("The 'Save' method gave nothing (T-2)."));

        public static Run Start(DebugIntentHandlers handlers)
        {
            Type type = GameAssemblyFile.Type(RunTypeName);
            MethodInfo start = type.GetMethod(
                "Start",
                [typeof(ContentSet), typeof(ulong), typeof(DebugIntentHandlers), typeof(MessageSpeed)])
                ?? throw new InvalidOperationException("The run holds no 'Start' method (T-2).");

            // A test run passes no debug handler, as a release build does. The tests of the
            // console pass the handlers of the debug assembly (D-260, D-492).
            object instance = start.Invoke(null, [Content.Value, Seed, handlers, MessageSpeed.Normal])
                ?? throw new InvalidOperationException("The 'Start' method gave no run (T-2).");
            return new Run(type, instance);
        }

        public void Queue(Intent intent) => this.queue.Invoke(this.instance, [intent]);

        public IReadOnlyList<LogEntry> Advance(double seconds, Func<Intent?>? heldStep = null) =>
            (IReadOnlyList<LogEntry>)(this.advance.Invoke(this.instance, [seconds, heldStep])
                ?? throw new InvalidOperationException("The 'Advance' method gave nothing (T-2)."));

        public RunRecord Record() =>
            (RunRecord)(this.record.Invoke(this.instance, Type.EmptyTypes)
                ?? throw new InvalidOperationException("The 'Record' method gave nothing (T-2)."));
    }
}
