using System;
using System.Linq;
using System.Text;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The overworld of PR-35 on the maps of <see cref="OverworldMaps"/>: the exit onto its marker,
/// the entrance onto the spawn point of a place, the gate, and the autosave (D-1243, D-1246,
/// D-1255, D-1257).
/// </summary>
public sealed class OverworldTests
{
    private const ulong Seed = 20260927;

    [Fact]
    public void AnExitPutsThePartyOnItsMarkerOfTheOverworldAndAsksForTheAutosave()
    {
        // Exit tests 3 and 4 of PR-35 (D-1246, D-1255).
        Simulation run = OverworldMaps.Start(Seed, OverworldMaps.Place);
        Assert.Empty(run.TakeSaveRequests());

        HubWalks.Walk(run, StepDirection.East, 4);

        Assert.Equal("map.test_overworld", run.State.Party.Map.Id.Value);
        Assert.Equal(new TilePoint(1, 2), run.State.Party.LeadAt);
        Assert.Equal([SaveRequestKind.Autosave], run.TakeSaveRequests());
    }

    [Fact]
    public void AnEntrancePutsThePartyOnTheSpawnPointOfItsPlaceWithNoAutosave()
    {
        // Exit test 4 of PR-35 (D-1243): a dungeon takes no autosave on its entry (D-224).
        Simulation run = OverworldMaps.Start(Seed, OverworldMaps.Overworld);
        HubWalks.Walk(run, StepDirection.West, 3);
        HubWalks.Walk(run, StepDirection.North, 1);

        Assert.Equal("map.test_place", run.State.Party.Map.Id.Value);
        Assert.Equal(OverworldMaps.Place.Spawn, run.State.Party.LeadAt);
        Assert.Empty(run.TakeSaveRequests());
    }

    [Fact]
    public void AStepOntoTheMarkerOfAnExitEntersNothing()
    {
        // D-1255: the marker is open ground beside the entrance, so the party can stand on it.
        Simulation run = OverworldMaps.Start(Seed, OverworldMaps.Overworld);
        HubWalks.Walk(run, StepDirection.West, 3);

        Assert.Equal("map.test_overworld", run.State.Party.Map.Id.Value);
        Assert.Equal(new TilePoint(1, 2), run.State.Party.LeadAt);
    }

    [Fact]
    public void AClosedGateRefusesTheStepAndAConfirmPostsItsNotice()
    {
        // Exit test 1 of PR-35 (D-1243, D-1257): the step turns the lead alone, and the confirm
        // at the gate says why.
        Simulation run = OverworldMaps.Start(Seed, OverworldMaps.Overworld);
        HubWalks.Walk(run, StepDirection.East, 1);
        HubWalks.Face(run, StepDirection.East);
        Assert.Empty(run.TakeNotices());

        HubWalks.Confirm(run);

        Assert.Equal(new TilePoint(5, 2), run.State.Party.LeadAt);
        Assert.Equal([OverworldMaps.GateNotice], run.TakeNotices().Select(notice => notice.Id.Value));
    }

    [Fact]
    public void AnOpenGateLetsTheLeadPassAndTakesNoConfirm()
    {
        // Exit test 2 of PR-35 (D-543, D-1243, D-1257).
        Simulation run = OverworldMaps.Start(Seed, OverworldMaps.Overworld);
        HubWalks.Walk(run, StepDirection.East, 1);
        HubWalks.Face(run, StepDirection.East);
        run.State.Story.Flags.TurnOn(OverworldMaps.Id(OverworldMaps.GateFlag));

        HubWalks.Confirm(run);
        Assert.Empty(run.TakeNotices());

        HubWalks.Walk(run, StepDirection.East, 1);
        Assert.Equal(new TilePoint(6, 2), run.State.Party.LeadAt);
    }

    [Fact]
    public void TheSamePathThroughThePlacesGivesTheSameStateHash()
    {
        // Exit test 2 of PR-35 (T-7): two runs of one seed and one path end in one state.
        Simulation first = WalkThroughThePlaces();
        Simulation second = WalkThroughThePlaces();

        Assert.Equal("map.test_place", first.State.Party.Map.Id.Value);
        Assert.Equal(first.Tick, second.Tick);
        Assert.Equal(first.StateHash(), second.StateHash());
    }

    [Fact]
    public void TheAutosaveOfTheOverworldReloadsToTheSameStateHash()
    {
        // Exit test 3 of PR-35 (D-1246): the snapshot that the autosave writes resumes on the
        // marker of the overworld.
        Simulation run = OverworldMaps.Start(Seed, OverworldMaps.Place);
        HubWalks.Walk(run, StepDirection.East, 4);
        Assert.Equal([SaveRequestKind.Autosave], run.TakeSaveRequests());

        string line = RunSnapshotText.Write(run.Snapshot());
        var reader = new ContentReader(Encoding.UTF8.GetBytes(line), "the test");
        Simulation resumed = Simulation.Resume(Seed, RunSnapshotText.Read(ref reader), OverworldMaps.Both, TestBattles.Content, OverworldMaps.Notices, TestStory.Content, DebugIntentHandlers.None);

        Assert.Equal(run.StateHash(), resumed.StateHash());
        Assert.Equal("map.test_overworld", resumed.State.Party.Map.Id.Value);
        Assert.Equal(new TilePoint(1, 2), resumed.State.Party.LeadAt);
    }

    [Fact]
    public void AnEntryOnAMarkerThatTheMapLacksIsAnError()
    {
        // D-1255 and T-2: the content set refuses such an exit, and the run refuses it too.
        Simulation run = OverworldMaps.Start(Seed, OverworldMaps.Place);

        SimulationException error = Assert.Throws<SimulationException>(
            () => run.State.EnterMapAt(OverworldMaps.Overworld.Id, OverworldMaps.Id("marker.test_absent"), run.State.Context("test")));

        Assert.Contains("marker.test_absent", error.Message, StringComparison.Ordinal);
        Assert.Contains("map.test_overworld", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Walks from the place onto the overworld, and back through the entrance.</summary>
    private static Simulation WalkThroughThePlaces()
    {
        Simulation run = OverworldMaps.Start(Seed, OverworldMaps.Place);
        HubWalks.Walk(run, StepDirection.East, 4);
        HubWalks.Walk(run, StepDirection.East, 1);
        HubWalks.Walk(run, StepDirection.West, 1);
        HubWalks.Walk(run, StepDirection.North, 1);
        return run;
    }
}
