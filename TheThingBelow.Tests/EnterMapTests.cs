using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The entry to a map of the set of a run (D-528, D-1133): <see cref="RunState.EnterMap"/> and
/// <see cref="RunState.RefusalOfEnter"/>. The entry puts the party on the spawn point, notes the
/// entry triggers, and asks for the autosave on a hub (D-224, D-1132).
/// </summary>
/// <remarks>
/// The runs start on the room of <see cref="TestMaps"/>, a dungeon, with the inn of
/// <see cref="HubMaps"/> and the story map of <see cref="TestStory"/> to enter.
/// </remarks>
public sealed class EnterMapTests
{
    private const ulong Seed = 20260925;

    private static readonly ContentId Room = TestMaps.Room.Id;

    private static readonly ContentId Inn = HubMaps.Inn.Id;

    private static readonly ContentId StoryMap = TestStory.Map.Id;

    [Fact]
    public void TheEntryToAHubAsksForOneAutosaveAndTheEntryToADungeonForNone()
    {
        // Exit test 17 of PR-14, the Core part: the entry to a hub writes the autosave (D-224, D-1132).
        Simulation run = Start();
        Assert.Empty(run.TakeSaveRequests());

        Enter(run, Inn);

        Assert.Equal([SaveRequestKind.Autosave], run.TakeSaveRequests());
        Assert.Empty(run.TakeSaveRequests());

        Enter(run, Room);

        Assert.Empty(run.TakeSaveRequests());
    }

    [Fact]
    public void TheEntryPutsTheLeadOnTheSpawnPointOfTheMapFacingSouth()
    {
        Simulation run = Start();
        HubWalks.Walk(run, StepDirection.East, 3);

        Enter(run, Inn);

        Assert.Equal(Inn.Value, run.State.Party.Map.Id.Value);
        Assert.Equal(HubMaps.Inn.Spawn, run.State.Party.LeadAt);
        Assert.Equal(StepDirection.South, run.State.Party.Facing);
        Assert.Null(run.State.Party.Stepping);
        Assert.Equal(1, run.State.Party.Walked.Count);
        Assert.Single(run.State.Party.Npcs.Values());
    }

    [Fact]
    public void TheMapThatThePartyLeavesKeepsNoWalkedTile()
    {
        // D-1133: PR-35 owns the memory of each map, so a map entered again starts fresh.
        Simulation run = Start();
        HubWalks.Walk(run, StepDirection.East, 3);
        Assert.True(run.State.Party.Walked.Count > 1);

        Enter(run, Inn);
        Enter(run, Room);

        Assert.Equal(1, run.State.Party.Walked.Count);
        Assert.Equal(TestMaps.Room.Spawn, run.State.Party.LeadAt);
    }

    [Fact]
    public void TheEntryNotesTheEntryTriggersOfTheMap()
    {
        // D-1004: the world step of the tick of the entry reads the entry trigger of the story map.
        Simulation run = Start();
        run.Step([]);
        Assert.False(run.State.Story.EntryPending);

        run.State.EnterMap(StoryMap, run.State.Context("test/enter"));
        Assert.True(run.State.Story.EntryPending);
        run.Step([]);

        Assert.Equal(TestStory.Meet.Value, run.State.Story.Scene?.Id.Value);
    }

    [Fact]
    public void AnOpenMenuRefusesTheEntryAndChangesNothing()
    {
        Simulation run = Start();
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        MapState before = run.State.Party;

        SimulationException error = Assert.Throws<SimulationException>(() => run.State.EnterMap(Inn, run.State.Context("test/enter")));

        Assert.Contains("an entry to the map 'map.hub_test' found an open menu", error.Message, StringComparison.Ordinal);
        Assert.Same(before, run.State.Party);
        Assert.Empty(run.TakeSaveRequests());
    }

    [Fact]
    public void AnIdThatNoMapOfTheRunTakesRefusesTheEntryWithTheIdsOfTheRun()
    {
        Simulation run = Start();

        string? refusal = run.State.RefusalOfEnter(ContentId.Parse("map.nowhere", "test", "map"));

        Assert.Equal($"no map 'map.nowhere' among the maps of the run, which are {HubMaps.Inn.Id.Value}, {Room.Value}, {StoryMap.Value}", refusal);
    }

    [Fact]
    public void ABattleRefusesTheEntry()
    {
        GameMap guarded = BattleRuns.Map("group.one");
        Simulation run = Simulation.Start(Seed, MapSet.Of([guarded, HubMaps.Inn]), guarded.Id, TestBattles.Content, TestBattles.Notices, TestStory.Content, DebugIntentHandlers.None);
        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        Assert.NotNull(run.State.Battle);

        Assert.Equal("a battle that holds the run", run.State.RefusalOfEnter(Inn));
    }

    [Fact]
    public void AStorySceneRefusesTheEntry()
    {
        Simulation run = Start();
        Enter(run, StoryMap);
        Assert.True(run.State.Story.Running);

        Assert.Equal("a story scene that runs, and a story scene holds the map (D-1009)", run.State.RefusalOfEnter(Room));
    }

    [Fact]
    public void TheMapThatThePartyStandsOnTakesTheEntryAgain()
    {
        Simulation run = Start();
        HubWalks.Walk(run, StepDirection.East, 2);

        Enter(run, Room);

        Assert.Equal(TestMaps.Room.Spawn, run.State.Party.LeadAt);
    }

    [Fact]
    public void AnIntentOfThePlayerThatNamesAMapFails()
    {
        // D-1133: the debug console alone names a map.
        Simulation run = Start();

        SimulationException error = Assert.Throws<SimulationException>(
            () => run.Step([new Intent(IntentIds.MoveEast, false, Map: Inn)]));

        Assert.Contains("an intent of the player that names the map 'map.hub_test'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheRunOpensTheFirstMapOfItsIdAndRefusesAnIdThatTheSetLacks()
    {
        Simulation run = Start();
        Assert.Equal(Room.Value, run.State.Party.Map.Id.Value);

        ArgumentException error = Assert.Throws<ArgumentException>(
            () => Simulation.Start(Seed, MapSet.Of([TestMaps.Room]), Inn, TestBattles.Content, TestBattles.Notices, TestStory.Content, DebugIntentHandlers.None));

        Assert.Contains("The run opens the map 'map.hub_test', and the maps of the run are map.test_room", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASetWithTwoMapsOfOneIdFails()
    {
        ArgumentException error = Assert.Throws<ArgumentException>(() => MapSet.Of([TestMaps.Room, TestMaps.Room]));

        Assert.Contains("two maps with the id 'map.test_room'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AResumeRefusesASnapshotOfAMapThatTheSetLacks()
    {
        Simulation run = Start();
        Enter(run, Inn);

        ArgumentException error = Assert.Throws<ArgumentException>(
            () => Simulation.Resume(Seed, run.Snapshot(), TestMaps.Room, TestBattles.Content, TestBattles.Notices, TestStory.Content, DebugIntentHandlers.None));

        Assert.Contains("The snapshot names the map 'map.hub_test', and the maps of the run are map.test_room", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Starts a run on the room, with the inn and the story map to enter.</summary>
    private static Simulation Start() =>
        Simulation.Start(Seed, MapSet.Of([TestMaps.Room, HubMaps.Inn, TestStory.Map]), Room, TestBattles.Content, TestBattles.Notices, TestStory.Content, DebugIntentHandlers.None);

    /// <summary>Enters a map between two ticks, and runs the world step of one tick after it.</summary>
    private static IReadOnlyList<LogEntry> Enter(Simulation run, ContentId map)
    {
        run.State.EnterMap(map, run.State.Context("test/enter"));
        return run.Step([]);
    }
}
