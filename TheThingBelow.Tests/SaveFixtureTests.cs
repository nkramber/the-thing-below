using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Core;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Saves;
using TheThingBelow.Core.Streams;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The stored save of each format version (D-166, D-654). Each file sits in the checkout, and
/// no test writes one, so a change of the reader that breaks an old save fails here.
/// </summary>
/// <remarks>
/// The PR that next changes the snapshot raises <see cref="SaveFormat.Current"/>, adds a
/// reader of each older version, and commits a fixture save of the version that it leaves
/// (D-166, D-654). <see cref="EveryFormatVersionHasAStoredSave"/> fails a raise with no
/// fixture, and the values below then prove that the migration keeps the state of the run.
/// <para>
/// The stored save of format 1 names simulation version 1 and the game version 0.1.0. This
/// build runs another simulation version, and the load reads the snapshot alone, so the save
/// still loads (D-259). Format 1 predates the tile map, so its migration puts the party on
/// the spawn point of the first map (D-654).
/// </para>
/// </remarks>
public sealed class SaveFixtureTests
{
    private const string FixtureFolder = "TheThingBelow.Tests/saves";

    [Fact]
    public void EveryFormatVersionHasAStoredSave()
    {
        for (int version = SaveFormat.Oldest; version <= SaveFormat.Current; version += 1)
        {
            string path = PathOfFormat(version);
            Assert.True(
                File.Exists(path),
                $"Format version {version} has no stored save at '{path}'. A PR that raises " +
                $"the format version commits a fixture save of the version that it leaves (D-166, D-654).");
        }
    }

    [Fact]
    public void EveryStoredSaveReadsWithThisBuild()
    {
        for (int version = SaveFormat.Oldest; version <= SaveFormat.Current; version += 1)
        {
            string path = PathOfFormat(version);
            SaveDocument save = SaveText.Read(File.ReadAllText(path), path);

            Assert.Equal(version, save.Header.FormatVersion);
        }
    }

    [Fact]
    public void TheStoredSaveOfFormatOneHoldsTheStateOfItsRun()
    {
        SaveDocument save = ReadFormat(1);

        Assert.Equal(SaveRuns.Seed, save.Header.Seed);
        Assert.Equal(SaveRuns.ContentHash, save.Header.ContentHash);
        Assert.Equal(120, save.Snapshot.Tick);
        Assert.False(save.Snapshot.MenuOpen);
        Assert.Equal(55, save.Snapshot.WorldTick);
        Assert.Null(save.Snapshot.Map);
        Assert.Equal(
            new List<StreamPosition>
            {
                new(StreamId.Exploration, 0x933723557a21fe37, 0x0000000000000003),
                new(StreamId.Battle, 0x9ed29fecf890e3fb, 0x0000000000000005),
                new(StreamId.Progression, 0xd9107d3e42db0c22, 0x0000000000000007),
                new(StreamId.Story, 0x9881a20135b0f29e, 0x0000000000000009),
            },
            save.Snapshot.Streams);
    }

    [Fact]
    public void TheStoredSaveOfAnOlderSimulationVersionLoads()
    {
        // Exit test 5 of section 7.14, and D-259: a load reads the snapshot alone, so a patch
        // of the rules never refuses a save that an older build wrote.
        SaveDocument save = ReadFormat(1);

        Assert.NotEqual(SimulationVersion.Current, save.Header.SimulationVersion);
        Assert.Equal(1, save.Header.SimulationVersion);

        Simulation run = Simulation.Resume(
            save.Header.Seed, save.Snapshot, TestMaps.FixtureDungeon, DebugIntentHandlers.None);

        Assert.Equal(120, run.Tick);
    }

    [Fact]
    public void TheStoredSaveOfFormatOnePutsThePartyOnTheSpawnOfTheFirstMap()
    {
        // The migration of D-166: format 1 predates the tile map, so the party enters the
        // first map at its spawn point with that tile walked and no other (D-654).
        SaveDocument save = ReadFormat(1);

        Simulation run = Simulation.Resume(
            save.Header.Seed, save.Snapshot, TestMaps.FixtureDungeon, DebugIntentHandlers.None);

        Assert.Equal(TestMaps.FixtureDungeon.Spawn, run.State.Party.LeadAt);
        Assert.Equal(1, run.State.Party.Walked.Count);
        Assert.Equal(MapIds.FirstMap.Value, run.State.Party.Map.Id.Value);
    }

    [Fact]
    public void TheStoredSaveOfFormatTwoHoldsThePartyOnItsMap()
    {
        SaveDocument save = ReadFormat(2);

        Assert.Equal(2, save.Header.FormatVersion);
        Assert.NotNull(save.Snapshot.Map);
        Assert.Equal(TestMaps.Room.Id.Value, save.Snapshot.Map.Map.Value);

        Simulation run = Simulation.Resume(
            save.Header.Seed, save.Snapshot, TestMaps.Room, DebugIntentHandlers.None);

        Assert.Equal(SaveRuns.FixtureTicks, run.Tick);
        Assert.Equal(save.Snapshot.Map.Walked, run.State.Party.Walked.Rows());
    }

    [Fact]
    public void TheStoredSaveOfFormatOneRunsAgainFromItsTick()
    {
        SaveDocument save = ReadFormat(1);
        Simulation run = Simulation.Resume(
            save.Header.Seed, save.Snapshot, TestMaps.FixtureDungeon, DebugIntentHandlers.None);

        run.Step([]);

        Assert.Equal(121, run.Tick);
    }

    [Fact]
    public void TheFolderOfTheStoredSavesHoldsNoOtherFile()
    {
        // A file with no test would hide a format that the reader no longer reads (D-166).
        List<string> wanted = [];
        for (int version = SaveFormat.Oldest; version <= SaveFormat.Current; version += 1)
        {
            wanted.Add(Path.GetFileName(PathOfFormat(version)));
        }

        List<string> found = [];
        foreach (string file in Directory.GetFiles(RepositoryRoot.PathTo(FixtureFolder)))
        {
            found.Add(Path.GetFileName(file));
        }

        found.Sort(StringComparer.Ordinal);
        Assert.Equal(wanted, found);
    }

    private static SaveDocument ReadFormat(int version)
    {
        string path = PathOfFormat(version);
        return SaveText.Read(File.ReadAllText(path), path);
    }

    private static string PathOfFormat(int version) =>
        RepositoryRoot.PathTo($"{FixtureFolder}/format-{version.ToString(System.Globalization.CultureInfo.InvariantCulture)}.json");
}
