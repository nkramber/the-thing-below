using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Streams;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The NPCs of one map in a run: the NPC stream, the state hash, the snapshot, the resume of a
/// save of this build and of another build, and the hold of a story scene (D-1111, D-1137,
/// D-1139).
/// </summary>
public sealed class MapNpcsTests
{
    private const int Dog = 2;
    private const int Child = 3;

    [Fact]
    public void AWanderNpcDrawsOneValueOnEachPaceTick()
    {
        // D-1137, D-1138: the dog chooses on world tick 1 and then on each 64th tick, a pause and
        // a blocked step included, so ten paces take ten draws and no other rule draws.
        Simulation run = Start(HubMaps.Of(npcs: HubMaps.Wanderer()));
        for (int tick = 0; tick < 640; tick += 1)
        {
            run.Step([]);
        }

        RandomStream expected = RandomStreams.Open(SaveRuns.Seed, StreamId.Npc);
        for (int draw = 0; draw < 10; draw += 1)
        {
            _ = expected.NextInt(MapNpcs.WanderChoices, new RunContext(SaveRuns.Seed, 0, "the test"));
        }

        Assert.Equal(expected.Generator.State, run.State.Stream(StreamId.Npc).Generator.State);
        Assert.Equal(RandomStreams.Open(SaveRuns.Seed, StreamId.Exploration).Generator.State, run.State.Stream(StreamId.Exploration).Generator.State);
    }

    [Fact]
    public void ARouteNpcAndAChaserDrawNothing()
    {
        Simulation run = Start(HubMaps.Of(npcs: $"{HubMaps.Keeper}, {HubMaps.Walker()}, {HubMaps.Chaser(target: "npc.hub_keeper")}"));
        for (int tick = 0; tick < 640; tick += 1)
        {
            run.Step([]);
        }

        Assert.Equal(RandomStreams.Open(SaveRuns.Seed, StreamId.Npc).Generator.State, run.State.Stream(StreamId.Npc).Generator.State);
    }

    [Fact]
    public void TheWalkRefusesAStreamOfAnotherSubsystem()
    {
        MapState party = MapState.Enter(HubMaps.Yard);

        ArgumentException error = Assert.Throws<ArgumentException>(() => party.Npcs.Walk(
            party.Map,
            party,
            RandomStreams.Open(SaveRuns.Seed, StreamId.Exploration),
            new RunContext(SaveRuns.Seed, 0, "the test"),
            []));

        Assert.Contains("draw from the stream 'Npc'", error.Message, StringComparison.Ordinal);
        Assert.Contains("'Exploration'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void HoldStillEndsTheStepOfEachNpcOnItsTile()
    {
        // D-1139: at tick 150 the dog steps south from (6, 5), as the save of format 15 holds.
        Simulation run = Play(HubMaps.Yard, 150);
        NpcState dog = run.State.Party.Npcs.All[Dog];
        Assert.Equal(StepDirection.South, dog.Stepping);

        run.State.Party.Npcs.HoldStill();

        Assert.Equal(new TilePoint(6, 5), dog.At);
        Assert.Null(dog.Stepping);
        Assert.Null(dog.StepEnd);
    }

    [Fact]
    public void EachNpcStandsStillWhileAStorySceneRuns()
    {
        // D-1139: the meeting of the story fixture starts on the first world tick, and the world
        // tick then runs the story scene alone.
        GameMap map = TestMaps.Of("rules/maps/test-story.json", TestStory.MapFile.Replace(
            "\"npcs\": []",
            $"\"npcs\": [{HubMaps.Wanderer(x: 2, y: 3, areas: """{ "x": 1, "y": 3, "width": 3, "height": 1 }""", paceTicks: 32)}]",
            StringComparison.Ordinal));
        Simulation run = Simulation.Start(SaveRuns.Seed, map, TestBattles.Content, TestBattles.Notices, TestStory.Content, DebugIntentHandlers.None);

        run.Step([]);
        Assert.True(run.State.Story.Running);
        string held = Texts(run.State.Party.Npcs.Values());
        for (int tick = 0; tick < 200; tick += 1)
        {
            run.Step([]);
        }

        Assert.True(run.State.Story.Running);
        Assert.Equal(held, Texts(run.State.Party.Npcs.Values()));
        Assert.Equal(RandomStreams.Open(SaveRuns.Seed, StreamId.Npc).Generator.State, run.State.Stream(StreamId.Npc).Generator.State);
    }

    [Fact]
    public void ASnapshotOfTheNpcsResumesTheSameRun()
    {
        // G-5, D-259: the text of the snapshot holds every value that the walk reads, so the live
        // run and the resumed run keep one state hash.
        Simulation live = Play(HubMaps.Yard, 150);
        RunSnapshot read = ReadLine(RunSnapshotText.Write(live.Snapshot()));
        Simulation resumed = Resume(read, HubMaps.Yard);

        Assert.Equal(live.State.StateHash(), resumed.State.StateHash());
        for (int tick = 0; tick < 300; tick += 1)
        {
            live.Step([]);
            resumed.Step([]);
        }

        Assert.Equal(live.State.StateHash(), resumed.State.StateHash());
        Assert.Equal(RunSnapshotText.Write(live.Snapshot()), RunSnapshotText.Write(resumed.Snapshot()));
    }

    [Fact]
    public void TheStateHashReadsEachValueOfAnNpc()
    {
        RunSnapshot snapshot = Play(HubMaps.Yard, 150).Snapshot();
        RunSnapshot waited = WithNpc(snapshot, Child, npc => npc with { WaitTicks = npc.WaitTicks - 1 });

        Assert.NotEqual(Resume(snapshot, HubMaps.Yard).State.StateHash(), Resume(waited, HubMaps.Yard).State.StateHash());
    }

    [Fact]
    public void ThisBuildRefusesAnNpcListOfAnotherShape()
    {
        RunSnapshot snapshot = Play(HubMaps.Yard, 150).Snapshot();
        MapSnapshot map = snapshot.Map!;
        RunSnapshot none = snapshot with { Map = map with { Npcs = [] } };
        RunSnapshot swapped = snapshot with { Map = map with { Npcs = [map.Npcs![1], map.Npcs[0], map.Npcs[2], map.Npcs[3]] } };

        ArgumentException count = Assert.Throws<ArgumentException>(() => Resume(none, HubMaps.Yard));
        ArgumentException order = Assert.Throws<ArgumentException>(() => Resume(swapped, HubMaps.Yard));

        Assert.Contains("it holds 0 NPCs, and the map 'map.hub_test' places 4", count.Message, StringComparison.Ordinal);
        Assert.Contains("the NPC at position 0 is 'npc.hub_barmaid'", order.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ThisBuildRefusesTwoNpcsOnOneTile()
    {
        RunSnapshot snapshot = Start(HubMaps.Yard).Snapshot();
        RunSnapshot crowded = WithNpc(snapshot, Child, npc => npc with { X = 6, Y = 5 });

        ArgumentException error = Assert.Throws<ArgumentException>(() => Resume(crowded, HubMaps.Yard));

        Assert.Contains("the NPC 'npc.hub_child' at (6, 5) and the NPC 'npc.hub_dog' at (6, 5) hold one tile", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ThisBuildRefusesALeadOnAnNpc()
    {
        RunSnapshot snapshot = Start(HubMaps.Yard).Snapshot();
        RunSnapshot onDog = WithLead(snapshot, new TilePoint(6, 5));

        ArgumentException error = Assert.Throws<ArgumentException>(() => Resume(onDog, HubMaps.Yard));

        Assert.Contains("the NPC 'npc.hub_dog' at (6, 5) holds that tile (D-1139)", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnotherBuildMatchesEachNpcByItsIdAndLogsEachChange()
    {
        // D-1111, applied to the NPCs: the dog leaves the edited map, the new cat starts on its
        // start tile, the child keeps its stored place, and the barmaid starts again, because
        // her edited route no longer takes her stored place.
        RunSnapshot snapshot = Play(HubMaps.Yard, 150).Snapshot();
        GameMap edited = HubMaps.Of(npcs: string.Join(
            ", ",
            HubMaps.Keeper,
            HubMaps.Walker(tiles: """{ "x": 1, "y": 2, "wait_ticks": 30 }, { "x": 2, "y": 2, "wait_ticks": 0 }"""),
            HubMaps.Wanderer(id: "npc.hub_cat", x: 1, y: 5, areas: """{ "x": 1, "y": 5, "width": 2, "height": 1 }"""),
            HubMaps.Chaser(target: "npc.hub_cat")));
        ResumeDrift drift = ResumeDrift.Of(SnapshotOrigin.OtherBuild, snapshot.Tick);

        Simulation run = Simulation.Resume(SaveRuns.Seed, snapshot, edited, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None, drift);

        IReadOnlyList<NpcState> npcs = run.State.Party.Npcs.All;
        Assert.Equal((new TilePoint(1, 2), 30), (npcs[1].At, npcs[1].WaitTicks));
        Assert.Equal(new TilePoint(1, 5), npcs[2].At);
        Assert.Equal(new TilePoint(7, 5), npcs[3].At);
        Assert.Equal(3, drift.Entries.Count);
        Assert.Contains("no stored place of the NPC", Message(drift, 0, "npc.hub_barmaid"), StringComparison.Ordinal);
        Assert.Contains("places an NPC that the save lacks", Message(drift, 1, "npc.hub_cat"), StringComparison.Ordinal);
        Assert.Contains("no longer places an NPC of the save", Message(drift, 2, "npc.hub_dog"), StringComparison.Ordinal);
    }

    [Fact]
    public void AnotherBuildMovesALeadOffANewNpcToTheSpawnPoint()
    {
        // D-1111: a build that puts an NPC under the stored lead moves the lead, as a new wall does.
        RunSnapshot snapshot = WithLead(Start(HubMaps.Of()).Snapshot(), new TilePoint(6, 5));
        ResumeDrift drift = ResumeDrift.Of(SnapshotOrigin.OtherBuild, snapshot.Tick);

        Simulation run = Simulation.Resume(SaveRuns.Seed, snapshot, HubMaps.Yard, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None, drift);

        Assert.Equal(HubMaps.Yard.Spawn, run.State.Party.LeadAt);
        Assert.Contains(drift.Entries, entry => entry.Message.Contains("an NPC of this build holds the tile of the lead", StringComparison.Ordinal));
    }

    private static Simulation Start(GameMap map) =>
        Simulation.Start(SaveRuns.Seed, map, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

    private static Simulation Play(GameMap map, int ticks)
    {
        Simulation run = Start(map);
        for (int tick = 0; tick < ticks; tick += 1)
        {
            run.Step([]);
        }

        return run;
    }

    private static Simulation Resume(RunSnapshot snapshot, GameMap map) =>
        Simulation.Resume(SaveRuns.Seed, snapshot, map, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

    private static RunSnapshot ReadLine(string line)
    {
        var reader = new ContentReader(Encoding.UTF8.GetBytes(line), "the test");
        return RunSnapshotText.Read(ref reader);
    }

    private static RunSnapshot WithNpc(RunSnapshot snapshot, int index, Func<NpcValues, NpcValues> change)
    {
        MapSnapshot map = snapshot.Map!;
        List<NpcValues> npcs = [.. map.Npcs!];
        npcs[index] = change(npcs[index]);
        return snapshot with { Map = map with { Npcs = npcs } };
    }

    /// <summary>Puts the stored lead on one tile, and marks that tile walked (D-567).</summary>
    private static RunSnapshot WithLead(RunSnapshot snapshot, TilePoint at)
    {
        MapSnapshot map = snapshot.Map!;
        List<string> rows = [.. map.Walked];
        char[] row = rows[at.Y].ToCharArray();
        row[at.X] = 'x';
        rows[at.Y] = new string(row);
        return snapshot with { Map = map with { LeadX = at.X, LeadY = at.Y, Walked = rows } };
    }

    private static string Texts(IReadOnlyList<NpcValues> npcs) => string.Join(" | ", npcs);

    private static string Message(ResumeDrift drift, int index, string npc)
    {
        LogEntry entry = drift.Entries[index];
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Contains(entry.Fields, field => field.Name == "npc" && field.Value == npc);
        return entry.Message;
    }
}
