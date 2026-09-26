using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    /// <summary>The id of the fixture hub, which the debug command `goto` enters (D-1133).</summary>
    private static readonly ContentId HubId = ContentId.Parse("map.fixture_hub", "test", "map");

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
        GameValue list = GameValue.New("PartyList", run.State);
        run.Queue(Intent.OfPlayer(IntentIds.OpenMenu));
        list.Call("Move", 1);
        list.Call("Move", -1);
        run.Queue((Intent)list.Call("Confirm")!);
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

    [Fact]
    public void TheGroupSwapsOnTheDungeonGoesToTheHubAndSavesAtTheWaystone()
    {
        // Exit test 2 of PR-14 (D-1132, D-1134): the group of four swaps the reserve on the dungeon,
        // enters the hub, confirms the waystone, and saves. The run collects one slot save.
        Run run = GroupRun();

        // The party window picks Marrek, the swap, and the one reserve character (D-1134).
        GameValue list = GameValue.New("PartyList", run.State);
        list.Call("Confirm");
        list.Call("Point", 1);
        list.Call("Confirm");
        run.Queue(Intent.OfPlayer(IntentIds.OpenMenu));
        run.Queue((Intent)list.Call("Confirm")!);
        run.Queue(Intent.OfPlayer(IntentIds.CloseMenu));
        run.Advance(OneTick);

        Assert.Equal(ReserveContent.Added[2], run.State.Characters.Members[0].Record.Id.Value);
        Assert.Equal("character.marrek", run.State.Characters.Reserve[0].Record.Id.Value);

        GoToHub(run);
        Assert.Equal([SaveKind.Autosave], Kinds(run.TakeSaves()));
        FaceTheWaystone(run);
        SaveDocument slot = SaveAtTheWaystone(run);

        Assert.Equal(run.Tick, slot.Snapshot.Tick);
        Assert.Equal(ReserveContent.Added[2], slot.Snapshot.Characters!.Characters[0].Character.Value);
        Assert.Equal(HubId.Value, slot.Snapshot.Map!.Map.Value);
        Assert.False(run.MenuOpen);
    }

    [Fact]
    public void TheSlotSaveOfTheWaystoneWritesReloadsAndGivesTheSameStateHash()
    {
        // Exit test 3 of PR-14 (D-224, D-1132): the save goes to a store of its own, and the reload
        // of that file reaches the state that the run holds.
        Run run = GroupRun();
        GoToHub(run);
        _ = run.TakeSaves();
        FaceTheWaystone(run);
        SaveDocument slot = SaveAtTheWaystone(run);
        string folder = Directory.CreateTempSubdirectory("the-thing-below-hub-save-").FullName;
        try
        {
            var store = new SaveStore(folder);
            store.Write(SaveKind.Slot, slot);

            Run reloaded = Run.Reload(ReserveContent.Content, store.Read(SaveKind.Slot), null, Seed, DebugAssemblyFile.Handlers(), []);

            Assert.Equal(run.Tick, reloaded.Tick);
            Assert.Equal(run.StateHash(), reloaded.StateHash());
            Assert.Single(reloaded.State.Characters.Reserve);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void TheEntryToAHubAndEachSaveOfAHubTakeTheSaveOfTheRunAndDropTheIntentsBeforeIt()
    {
        // Exit test 17 of PR-14 (D-224, D-1115, D-1132): the entry to a hub takes the autosave, and
        // each save calls `GameRun.Save`, so the record drops the intents before it.
        Run run = GroupRun();
        Intent step = run.IntentOf("step_east");
        for (int tick = 0; tick < 30; tick += 1)
        {
            run.Advance(OneTick, () => step);
        }

        Assert.Equal(30, run.RecordedLines);
        GoToHub(run);

        (SaveKind kind, SaveDocument autosave) = Assert.Single(run.TakeSaves());
        Assert.Equal(SaveKind.Autosave, kind);
        Assert.Equal(HubId.Value, autosave.Snapshot.Map!.Map.Value);
        Assert.Equal(run.Tick, autosave.Snapshot.Tick);
        Assert.Equal(0, run.RecordedLines);
        Assert.Equal(run.Tick, run.Record().Snapshot.Tick);
        Assert.Empty(run.TakeSaves());

        FaceTheWaystone(run);
        Assert.True(run.RecordedLines > 0);
        SaveDocument slot = SaveAtTheWaystone(run);

        Assert.Equal(0, run.RecordedLines);
        Assert.Equal(slot.Snapshot.Tick, run.Record().Snapshot.Tick);
    }

    [Theory]
    [InlineData("KeeperRoute", "KeeperStand", StepDirection.North, ServiceKind.Rest, 69)]
    [InlineData("WaystoneRoute", "WaystoneStand", StepDirection.East, ServiceKind.Save, 222)]
    public void EachRouteOfTheServiceCapturesReachesItsHostAndTheConfirmOpensTheService(string route, string stand, StepDirection facing, ServiceKind kind, long arrival)
    {
        // The rest and save captures walk these steps after `goto`, one step intent at a time, and
        // no NPC stands in the way at those ticks (D-1131, D-1138, D-1139). The fixed tick count of the
        // arrival keeps each frame the same on each run (D-172, T-7).
        Type captures = GameAssemblyFile.Type("TheThingBelow.Game.ScreenCaptures");
        var steps = (IReadOnlyList<string>)captures.GetProperty(route)!.GetValue(null)!;
        var end = (TilePoint)captures.GetProperty(stand)!.GetValue(null)!;
        Run run = Run.Start(DebugAssemblyFile.Handlers());
        GoToHub(run);

        foreach (string action in steps)
        {
            TilePoint from = run.State.Party.LeadAt;
            run.Queue(run.IntentOf(action));
            for (int tick = 0; tick < 17 && (tick == 0 || run.State.Party.Stepping is not null); tick += 1)
            {
                run.Advance(OneTick);
            }

            Assert.NotEqual(from, run.State.Party.LeadAt);
        }

        Assert.Equal((end, facing), (run.State.Party.LeadAt, run.State.Party.Facing));
        Assert.Equal(arrival, run.Tick);
        run.Queue(run.IntentOf("confirm"));
        run.Advance(OneTick);
        Assert.Equal(kind, Assert.Single(run.TakeOpenedServices()).Kind);
    }

    [Fact]
    public void AConfirmOnTheKeeperOpensTheRestServiceAndTheRestClosesTheMenu()
    {
        // D-390, D-1131: the confirm opens the service in the rules, and the window of Game sends the
        // rest intent and then the close. A rest writes no save.
        Run run = Run.Start(DebugAssemblyFile.Handlers());
        GoToHub(run);
        _ = run.TakeSaves();

        // The keeper stands at (6, 3) behind the bar, so the lead faces him from (6, 4).
        StepTiles(run, "step_north", 1);
        StepTiles(run, "step_east", 2);
        StepTiles(run, "step_north", 1);
        Assert.Equal(new TilePoint(6, 4), run.State.Party.LeadAt);
        run.Queue(run.IntentOf("confirm"));
        run.Advance(OneTick);

        MapService opened = Assert.Single(run.TakeOpenedServices());
        Assert.Equal(ServiceKind.Rest, opened.Kind);
        Assert.True(run.MenuOpen);
        GameValue choice = GameValue.New("ServiceChoice", opened.Kind);
        run.Queue((Intent)choice.Call("Confirm")!);
        run.Queue(Intent.OfPlayer(IntentIds.CloseMenu));
        run.Advance(OneTick);

        Assert.False(run.MenuOpen);
        Assert.Empty(run.TakeSaves());
        Assert.Empty(run.TakeOpenedServices());
    }

    /// <summary>Starts the run of the group of four: three in the party and one in the reserve, on the first map (exit test 1 of PR-14, D-1144).</summary>
    private static Run GroupRun()
    {
        Run start = Run.Start(ReserveContent.Content, DebugAssemblyFile.Handlers());
        SaveDocument first = start.Save();
        SaveDocument grouped = first with { Snapshot = ReserveContent.WithReserve(first.Snapshot, 1) };
        Run run = Run.Reload(ReserveContent.Content, grouped, null, Seed, DebugAssemblyFile.Handlers(), []);
        Assert.Equal(3, run.State.Characters.Members.Count);
        Assert.Single(run.State.Characters.Reserve);
        return run;
    }

    /// <summary>Types the debug command `goto` with the fixture hub, and runs its tick (D-1133).</summary>
    private static void GoToHub(Run run)
    {
        _ = DebugAssemblyFile.Run($"goto {HubId.Value}", () => run.State, run.Queue);
        run.Advance(OneTick);
        Assert.Equal(HubId.Value, run.State.Party.Map.Id.Value);
    }

    /// <summary>
    /// Walks the lead from the spawn point of the hub to the tile under the waystone at (14, 2),
    /// facing north: one tile north, ten east through the doorway, and two north.
    /// </summary>
    private static void FaceTheWaystone(Run run)
    {
        StepTiles(run, "step_north", 1);
        StepTiles(run, "step_east", 10);
        StepTiles(run, "step_north", 2);
        Assert.Equal(new TilePoint(14, 3), run.State.Party.LeadAt);
        Assert.Equal(StepDirection.North, run.State.Party.Facing);
    }

    /// <summary>
    /// Confirms the waystone, and chooses the save in its window as the menu host does: the save
    /// intent, then the close (D-1132). The run collects one slot save.
    /// </summary>
    private static SaveDocument SaveAtTheWaystone(Run run)
    {
        run.Queue(run.IntentOf("confirm"));
        run.Advance(OneTick);
        MapService opened = Assert.Single(run.TakeOpenedServices());
        Assert.Equal(ServiceKind.Save, opened.Kind);
        Assert.True(run.MenuOpen);

        GameValue choice = GameValue.New("ServiceChoice", opened.Kind);
        run.Queue((Intent)choice.Call("Confirm")!);
        run.Queue(Intent.OfPlayer(IntentIds.CloseMenu));
        run.Advance(OneTick);

        (SaveKind kind, SaveDocument slot) = Assert.Single(run.TakeSaves());
        Assert.Equal(SaveKind.Slot, kind);
        return slot;
    }

    /// <summary>
    /// Steps the lead a count of tiles in one direction, one step intent at a time. An NPC in the
    /// way turns the lead with no step, so the next tick tries again (D-1139).
    /// </summary>
    private static void StepTiles(Run run, string action, int tiles)
    {
        Intent step = run.IntentOf(action);
        for (int tile = 0; tile < tiles; tile += 1)
        {
            TilePoint from = run.State.Party.LeadAt;
            int tick = 0;
            while (run.State.Party.LeadAt == from || run.State.Party.Stepping is not null)
            {
                Assert.True(tick < 600, $"The lead stood at {from} for 600 ticks on the action '{action}' (T-2).");
                bool standing = run.State.Party.Stepping is null && run.State.Party.LeadAt == from;
                run.Advance(OneTick, standing ? () => step : null);
                tick += 1;
            }
        }
    }

    private static List<SaveKind> Kinds(List<(SaveKind Kind, SaveDocument Document)> saves)
    {
        List<SaveKind> kinds = [];
        foreach ((SaveKind kind, SaveDocument _) in saves)
        {
            kinds.Add(kind);
        }

        return kinds;
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

        public static Run Start(DebugIntentHandlers handlers) => Start(Content.Value, handlers);

        public static Run Reload(SaveDocument? slot, SaveDocument? autosave, ulong seed, List<LogEntry> log) =>
            Reload(Content.Value, slot, autosave, seed, DebugIntentHandlers.None, log);

        public static Run Reload(ContentSet content, SaveDocument? slot, SaveDocument? autosave, ulong seed, DebugIntentHandlers handlers, List<LogEntry> log)
        {
            Type type = GameAssemblyFile.Type(RunTypeName);
            MethodInfo reload = type.GetMethod(
                "Reload",
                [typeof(ContentSet), typeof(SaveDocument), typeof(SaveDocument), typeof(ulong), typeof(DebugIntentHandlers), typeof(MessageSpeed), typeof(List<LogEntry>)])
                ?? throw new InvalidOperationException("The run holds no 'Reload' method (T-2).");
            object instance = reload.Invoke(null, [content, slot, autosave, seed, handlers, MessageSpeed.Normal, log])
                ?? throw new InvalidOperationException("The 'Reload' method gave no run (T-2).");
            return new Run(type, instance);
        }

        public SaveDocument Save() =>
            (SaveDocument)(this.instance.GetType().GetMethod("Save", Type.EmptyTypes)!.Invoke(this.instance, Type.EmptyTypes)
                ?? throw new InvalidOperationException("The 'Save' method gave nothing (T-2)."));

        public static Run Start(ContentSet content, DebugIntentHandlers handlers)
        {
            Type type = GameAssemblyFile.Type(RunTypeName);
            MethodInfo start = type.GetMethod(
                "Start",
                [typeof(ContentSet), typeof(ulong), typeof(DebugIntentHandlers), typeof(MessageSpeed)])
                ?? throw new InvalidOperationException("The run holds no 'Start' method (T-2).");

            // A test run passes no debug handler, as a release build does. The tests of the
            // console pass the handlers of the debug assembly (D-260, D-492).
            object instance = start.Invoke(null, [content, Seed, handlers, MessageSpeed.Normal])
                ?? throw new InvalidOperationException("The 'Start' method gave no run (T-2).");
            return new Run(type, instance);
        }

        public void Queue(Intent intent) => this.queue.Invoke(this.instance, [intent]);

        public ulong StateHash() => (ulong)(this.instance.GetType().GetMethod("StateHash", Type.EmptyTypes)!.Invoke(this.instance, Type.EmptyTypes)
            ?? throw new InvalidOperationException("The 'StateHash' method gave nothing (T-2)."));

        public IReadOnlyList<MapService> TakeOpenedServices() =>
            (IReadOnlyList<MapService>)(this.instance.GetType().GetMethod("TakeOpenedServices", Type.EmptyTypes)!.Invoke(this.instance, Type.EmptyTypes)
                ?? throw new InvalidOperationException("The 'TakeOpenedServices' method gave nothing (T-2)."));

        public List<(SaveKind Kind, SaveDocument Document)> TakeSaves()
        {
            var taken = (IEnumerable)(this.instance.GetType().GetMethod("TakeSaves", Type.EmptyTypes)!.Invoke(this.instance, Type.EmptyTypes)
                ?? throw new InvalidOperationException("The 'TakeSaves' method gave nothing (T-2)."));
            List<(SaveKind, SaveDocument)> saves = [];
            foreach (object write in taken)
            {
                saves.Add(((SaveKind)write.GetType().GetProperty("Kind")!.GetValue(write)!, (SaveDocument)write.GetType().GetProperty("Document")!.GetValue(write)!));
            }

            return saves;
        }

        public IReadOnlyList<LogEntry> Advance(double seconds, Func<Intent?>? heldStep = null) =>
            (IReadOnlyList<LogEntry>)(this.advance.Invoke(this.instance, [seconds, heldStep])
                ?? throw new InvalidOperationException("The 'Advance' method gave nothing (T-2)."));

        public RunRecord Record() =>
            (RunRecord)(this.record.Invoke(this.instance, Type.EmptyTypes)
                ?? throw new InvalidOperationException("The 'Record' method gave nothing (T-2)."));
    }
}
