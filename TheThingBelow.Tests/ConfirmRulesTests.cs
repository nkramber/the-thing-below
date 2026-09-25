using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Story;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The confirm rule of the map (D-1131): exit test 8 and exit test 15 of PR-14, the place that
/// PR-16 fills with the door, the lock, the chest, and the save point, and the confirm in each
/// state that holds the world.
/// </summary>
/// <remarks>
/// The lead enters each map at the spawn point (1, 1) and faces south, so the tile (1, 2) is the
/// faced tile until the lead moves.
/// </remarks>
public sealed class ConfirmRulesTests
{
    private const ulong Seed = 20260925;

    /// <summary>The flag that closes a service of these tests, which the flag file of the tests declares.</summary>
    private static readonly ContentId ClosingFlag = ContentId.Parse("flag.test_marrek_side", "test", "flag");

    /// <summary>The porter walks from (1, 2) to (3, 2) and back with no wait, so it starts a step on the first tick.</summary>
    private const string Porter = """{ "id": "npc.hub_porter", "facing": "east", "step_ticks": 32, "move": "route", "tiles": [{ "x": 1, "y": 2, "wait_ticks": 0 }, { "x": 3, "y": 2, "wait_ticks": 0 }] }""";

    /// <summary>The rest of the porter, which no flag gates.</summary>
    private const string RestOnPorter = """{ "id": "service.hub_porter_rest", "kind": "rest", "npc": "npc.hub_porter", "condition": { "always": true } }""";

    private static readonly Intent Confirm = Intent.OfPlayer(IntentIds.Confirm);

    private static readonly Intent OpenMenu = Intent.OfPlayer(IntentIds.OpenMenu);

    [Fact]
    public void AStoryFlagClosesTheServiceOfAnNpcAndTheHubRefusesIt()
    {
        // Exit test 8 of PR-14 (D-543): the flag is on, so the condition of the rest fails, the
        // notice of a closed service shows, and no window opens.
        Simulation run = WithFlag(ClosingRest());
        HubRestTests.WalkToKeeper(run);
        _ = run.TakeNotices();

        IReadOnlyList<LogEntry> log = HubWalks.Confirm(run);

        Assert.False(run.State.MenuOpen);
        Assert.Empty(run.TakeOpenedServices());
        Assert.Equal(ServiceRules.ClosedNotice.Value, Assert.Single(run.TakeNotices()).Id.Value);
        Assert.Contains("the condition of a service failed, and the service stayed closed", HubWalks.Messages(log));
    }

    [Fact]
    public void TheSameServiceOpensWhileTheFlagIsOff()
    {
        // D-543: the condition holds with the flag off, so the rest of the same hub opens.
        Simulation run = Simulation.Start(Seed, ClosingRest(), TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
        HubRestTests.WalkToKeeper(run);

        HubWalks.Confirm(run);

        Assert.True(run.State.MenuOpen);
        Assert.Equal("service.hub_rest", Assert.Single(run.TakeOpenedServices()).Id.Value);
        Assert.Empty(run.TakeNotices());
    }

    [Fact]
    public void AStoryFlagClosesTheServiceOfAServicePoint()
    {
        // Exit test 8 of PR-14 (D-543, D-1142): a service point takes the condition as an NPC does.
        GameMap map = HubMaps.Of(
            things: HubMaps.Bed,
            services: $$"""{ "id": "service.hub_save", "kind": "save", "thing": "service_point.hub_bed", "condition": { "not": { "flag": "{{ClosingFlag.Value}}" } } }""");
        Simulation run = WithFlag(map);
        HubWalks.Walk(run, StepDirection.East, 6);
        HubWalks.Face(run, StepDirection.East);

        HubWalks.Confirm(run);

        Assert.False(run.State.MenuOpen);
        Assert.Empty(run.TakeOpenedServices());
        Assert.Equal(ServiceRules.ClosedNotice.Value, Assert.Single(run.TakeNotices()).Id.Value);
    }

    [Fact]
    public void ATalkTurnsAMovingNpcToTheLeadAndTheNpcStandsStillWhileTheWindowIsOpen()
    {
        // Exit test 15 of PR-14 (D-1139): the porter starts its step east on the first tick, and
        // the confirm reads it on its tile, ends its step, and turns it north to the lead.
        Simulation run = Start(HubMaps.Of(npcs: Porter, services: RestOnPorter));
        run.Step([]);
        NpcState porter = Assert.Single(run.State.Party.Npcs.All);
        Assert.Equal((new TilePoint(1, 2), StepDirection.East), (porter.At, porter.Stepping));

        HubWalks.Confirm(run);

        Assert.True(run.State.MenuOpen);
        Assert.Equal("service.hub_porter_rest", Assert.Single(run.TakeOpenedServices()).Id.Value);
        NpcValues held = porter.Values();
        Assert.Equal((1, 2, StepDirection.North, (StepDirection?)null), (held.X, held.Y, held.Facing, held.Stepping));
        for (int tick = 0; tick < 300; tick += 1)
        {
            run.Step([]);
            Assert.Equal(held, porter.Values());
        }

        run.Step([Intent.OfPlayer(IntentIds.CloseMenu)]);
        run.Step([]);

        Assert.Equal(StepDirection.East, porter.Stepping);
    }

    [Fact]
    public void ATalkReadsTheTileOfTheNpcAtItsTickAndNotTheEndOfItsStep()
    {
        // D-1139: the porter steps from (2, 2) into the faced tile (1, 2), and Core holds it on
        // (2, 2) through the step, so the confirm finds nobody on the faced tile.
        string toward = """{ "id": "npc.hub_porter", "facing": "west", "step_ticks": 32, "move": "route", "tiles": [{ "x": 2, "y": 2, "wait_ticks": 0 }, { "x": 1, "y": 2, "wait_ticks": 0 }] }""";
        Simulation run = Start(HubMaps.Of(npcs: toward, services: RestOnPorter));
        run.Step([]);
        NpcState porter = Assert.Single(run.State.Party.Npcs.All);
        Assert.Equal((new TilePoint(2, 2), StepDirection.West), (porter.At, porter.Stepping));

        IReadOnlyList<LogEntry> log = HubWalks.Confirm(run);

        Assert.False(run.State.MenuOpen);
        Assert.Equal(StepDirection.West, porter.Stepping);
        Assert.Contains("the lead confirmed at a tile with nothing to confirm", HubWalks.Messages(log));
    }

    [Fact]
    public void AnNpcWithNoTalkAndNoServiceTurnsAndLogsThePlaceOfItsHubLines()
    {
        // D-1131: PR-36 adds the hub lines of the dialogue box, so the talk logs that place.
        string standing = """{ "id": "npc.hub_porter", "facing": "east", "step_ticks": 32, "move": "route", "tiles": [{ "x": 1, "y": 2, "wait_ticks": 0 }] }""";
        Simulation run = Start(HubMaps.Of(npcs: standing));

        IReadOnlyList<LogEntry> log = HubWalks.Confirm(run);

        Assert.False(run.State.MenuOpen);
        Assert.Equal(StepDirection.North, Assert.Single(run.State.Party.Npcs.All).Facing);
        Assert.Contains("the NPC has no talk that holds and no service, and PR-36 adds its hub lines", HubWalks.Messages(log));
    }

    [Theory]
    [InlineData("chest")]
    [InlineData("save_point")]
    [InlineData("door")]
    [InlineData("lock")]
    public void AConfirmAtAThingOfPr16LogsThePlaceOfItsRule(string kind)
    {
        // D-1131: PR-16 adds the chest, the door, the lock, and the save point to the confirm rule.
        // A door and a lock stand in a doorway, so the faced tile becomes one for them (D-528).
        // A lock holds a door shut, and the confirm reads the first thing of the tile (D-386).
        string thing = kind == "lock"
            ? """{ "id": "lock.hub_south", "kind": "lock", "pickable": false, "x": 1, "y": 2 }, { "id": "door.hub_south", "kind": "door", "x": 1, "y": 2 }"""
            : $$"""{ "id": "{{kind}}.hub_south", "kind": "{{kind}}", "x": 1, "y": 2 }""";
        string text = HubMaps.Text(things: thing);
        if (kind is "door" or "lock")
        {
            text = text.Replace("\"#........#\",\n  \"#...##...#\"", "\"#+.......#\",\n  \"#...##...#\"", StringComparison.Ordinal);
        }

        Simulation run = Start(TestMaps.Of("hub-test.json", text));

        IReadOnlyList<LogEntry> log = HubWalks.Confirm(run);

        Assert.False(run.State.MenuOpen);
        Assert.Empty(run.TakeNotices());
        Assert.Contains($"the lead confirmed at a {kind}, and PR-16 adds its rule", HubWalks.Messages(log));
    }

    [Fact]
    public void AConfirmWhileTheMenuIsOpenFails()
    {
        Simulation run = Start(HubMaps.Inn);
        run.Step([OpenMenu]);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Confirm]));

        Assert.Contains("a confirm of the map while the menu is open", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-1131", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AConfirmInABattleFails()
    {
        Simulation run = BattleRuns.IntoBattle(Seed, "group.fixture_pair");

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Confirm]));

        Assert.Contains("a confirm of the map while a battle holds the run", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AConfirmInAStorySceneFails()
    {
        // D-1009: a story scene takes the step end, the pick, and the pause alone.
        Simulation run = TestStory.Start(Seed);
        run.Step([]);
        Assert.True(run.State.Story.Running);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Confirm]));

        Assert.Contains("D-1009", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AConfirmInThePauseOfAStorySceneFails()
    {
        // D-1010: the pause takes its end alone.
        Simulation run = TestStory.Start(Seed);
        run.Step([]);
        run.Step([Intent.OfPlayer(IntentIds.StoryPause)]);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Confirm]));

        Assert.Contains("D-1010", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AConfirmWhileTheLeadStepsEndsWithALogLine()
    {
        Simulation run = Start(HubMaps.Of(npcs: Porter, services: RestOnPorter));
        run.Step([HubWalks.Move(StepDirection.East)]);
        Assert.NotNull(run.State.Party.Stepping);

        IReadOnlyList<LogEntry> log = HubWalks.Confirm(run);

        Assert.False(run.State.MenuOpen);
        Assert.Contains("a confirm of the map came while the lead stepped, and it ended with the tick", HubWalks.Messages(log));
    }

    [Fact]
    public void AConfirmOnTheTickOfAnEncounterEndsWithALogLine()
    {
        // D-531: the step into the guard starts the encounter and its battle on this tick, so the
        // world step reads no confirm.
        Simulation run = Simulation.Start(Seed, BattleRuns.Map("group.fixture_pair"), TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

        IReadOnlyList<LogEntry> log = run.Step([HubWalks.Move(StepDirection.East), Confirm]);

        Assert.NotNull(run.State.Battle);
        Assert.Contains("a confirm of the map found the world held, and it ended with the tick", HubWalks.Messages(log));
    }

    [Fact]
    public void AConfirmLastsItsOwnTickAlone()
    {
        // T-7: the menu of the same tick holds the world, so the confirm ends, and the close of the
        // menu on the next tick opens no service.
        Simulation run = Start(HubMaps.Inn);
        HubRestTests.WalkToKeeper(run);

        IReadOnlyList<LogEntry> log = run.Step([Confirm, OpenMenu]);
        run.Step([Intent.OfPlayer(IntentIds.CloseMenu)]);
        run.Step([]);

        Assert.Contains("a confirm of the map found the world held, and it ended with the tick", HubWalks.Messages(log));
        Assert.False(run.State.MenuOpen);
        Assert.Empty(run.TakeOpenedServices());
    }

    /// <summary>A hub with the keeper, whose rest the flag of these tests closes.</summary>
    private static GameMap ClosingRest() =>
        HubMaps.Of(
            npcs: HubMaps.Keeper,
            services: $$"""{ "id": "service.hub_rest", "kind": "rest", "npc": "npc.hub_keeper", "condition": { "not": { "flag": "{{ClosingFlag.Value}}" } } }""");

    private static Simulation Start(GameMap map) =>
        Simulation.Start(Seed, map, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

    /// <summary>Starts a run on one map, with the flag of these tests on from the first tick.</summary>
    private static Simulation WithFlag(GameMap map)
    {
        RunSnapshot start = Start(map).Snapshot();
        StoryValues story = start.Story ?? throw new InvalidOperationException("The start snapshot holds no story state.");
        return Simulation.Resume(Seed, start with { Story = story with { Flags = [ClosingFlag] } }, map, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
    }
}
