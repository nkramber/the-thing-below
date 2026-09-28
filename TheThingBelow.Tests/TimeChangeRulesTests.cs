using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Hashing;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The rule of the time set by a flag (D-442, D-1348, D-1349). The first time change that holds
/// at the entry sets the time of the map, and the time holds while the party stays on the map.
/// </summary>
public sealed class TimeChangeRulesTests
{
    private const ulong Seed = 0x0000000000017349;

    private const string GoToId = "debug.go_to_map";

    [Fact]
    public void ARunStartsAtTheBaseTimeWhenNoChangeHolds()
    {
        Simulation run = Start(TimeMaps.NightOnFlag);

        Assert.Equal(TimeOfDay.Day, run.State.Party.Time);
        Assert.Equal([TimeMaps.DayEnemy], EnemiesOf(run));
    }

    [Fact]
    public void ARunStartsAtTheTimeOfAChangeOnANotNode()
    {
        // D-1349: the start is an entry, and it reads the flags of the new run, where no flag is
        // on, so a change on a not node holds.
        Simulation run = Start(TimeMaps.DayUntilFlag);

        Assert.Equal(TimeOfDay.Night, TimeMaps.DayUntilFlag.BaseTime);
        Assert.Equal(TimeOfDay.Day, run.State.Party.Time);
        Assert.Equal([TimeMaps.DayEnemy], EnemiesOf(run));
    }

    [Fact]
    public void AFlagThatTurnsOnWhileThePartyStaysOnTheMapChangesNeitherTheTimeNorTheEnemies()
    {
        // D-1349: no switch comes while the party stands on the map.
        Simulation run = Start(TimeMaps.NightOnFlag);
        run.State.Story.Flags.TurnOn(TimeMaps.FlagId);
        for (int tick = 0; tick < 40; tick += 1)
        {
            run.Step([]);
        }

        Assert.Equal(TimeOfDay.Night, TimeMaps.NightOnFlag.TimeFor(run.State.Story.Flags));
        Assert.Equal(TimeOfDay.Day, run.State.Party.Time);
        Assert.Equal([TimeMaps.DayEnemy], EnemiesOf(run));
    }

    [Fact]
    public void AnEntryAgainAfterTheFlagTakesTheNewTimeAndItsEnemies()
    {
        // D-1349: the time takes effect at the entry, and it holds on each later entry.
        Simulation run = Simulation.Start(Seed, MapSet.Of([TimeMaps.NightOnFlag, TestMaps.Room]), TimeMaps.NightOnFlag.Id, TestBattles.Content, TestBattles.Notices, TestStory.Content, DebugIntentHandlers.None);
        run.State.Story.Flags.TurnOn(TimeMaps.FlagId);

        run.State.EnterMap(TestMaps.Room.Id, run.State.Context("the test"));
        Assert.Equal(TimeOfDay.Day, run.State.Party.Time);
        run.State.EnterMap(TimeMaps.NightOnFlag.Id, run.State.Context("the test"));
        Assert.Equal(TimeOfDay.Night, run.State.Party.Time);
        Assert.Equal([TimeMaps.NightEnemy], EnemiesOf(run));

        run.State.EnterMap(TestMaps.Room.Id, run.State.Context("the test"));
        run.State.EnterMap(TimeMaps.NightOnFlag.Id, run.State.Context("the test"));
        Assert.Equal(TimeOfDay.Night, run.State.Party.Time);
    }

    [Fact]
    public void TheGoToCommandToTheSameMapTakesTheNewTime()
    {
        // D-1133, D-1349: the go-to command is an entry, also to the map that the party stands on.
        Simulation run = Simulation.Start(Seed, TimeMaps.NightOnFlag, TestBattles.Content, TestBattles.Notices, TestStory.Content, DebugAssemblyFile.Handlers());
        run.State.Story.Flags.TurnOn(TimeMaps.FlagId);

        run.Step([Intent.OfDebugMap(ContentId.Parse(GoToId, "the test", "debug"), TimeMaps.NightOnFlag.Id)]);

        Assert.Equal(TimeOfDay.Night, run.State.Party.Time);
        Assert.Equal([TimeMaps.NightEnemy], EnemiesOf(run));
    }

    [Fact]
    public void ASnapshotAfterTheFlagTurnsOnResumesAtTheTimeOfTheEntryWithTheSameStateHash()
    {
        // D-1349, G-5: the time of the entry lives in the snapshot, because the flags of the
        // snapshot give another time once a flag turns on while the party stands on the map.
        Simulation run = Start(TimeMaps.NightOnFlag);
        run.Step([Intent.OfPlayer(IntentIds.MoveWest)]);
        run.State.Story.Flags.TurnOn(TimeMaps.FlagId);
        run.Step([]);

        RunSnapshot snapshot = ReadLine(RunSnapshotText.Write(run.Snapshot()));
        Simulation resumed = Resume(snapshot, TimeMaps.NightOnFlag);

        Assert.Equal(TimeOfDay.Day, snapshot.Map!.Time);
        Assert.Equal(TimeOfDay.Day, resumed.State.Party.Time);
        Assert.Equal([TimeMaps.DayEnemy], EnemiesOf(resumed));
        Assert.Equal(run.StateHash(), resumed.StateHash());
        for (int tick = 0; tick < 60; tick += 1)
        {
            run.Step([]);
            resumed.Step([]);
        }

        Assert.Equal(run.StateHash(), resumed.StateHash());
    }

    [Fact]
    public void TheStateHashHoldsTheTime()
    {
        // G-5, D-1349: two states that differ by their time alone give two hashes.
        string text = TimeMaps.Text("day", """[{ "time": "night", "condition": { "flag": "flag.test_victor" } }]""")
            .Replace("\"times\": [\"dawn\", \"day\", \"dusk\"]", "\"times\": [\"dawn\", \"day\", \"dusk\", \"night\"]", StringComparison.Ordinal)
            .Replace("\"times\": [\"night\"]", "\"times\": [\"dawn\", \"day\", \"dusk\", \"night\"]", StringComparison.Ordinal);
        GameMap map = TestMaps.Of("time-test.json", text);
        MapState day = MapState.Enter(map, new PlaceState(map.Id), TimeOfDay.Day);
        MapState night = MapState.Enter(map, new PlaceState(map.Id), TimeOfDay.Night);

        Assert.Equal(EnemiesOf(day), EnemiesOf(night));
        Assert.NotEqual(HashOf(day), HashOf(night));
    }

    [Fact]
    public void AnEntryAtATimeThatTheMapCannotTakeIsAnError()
    {
        GameMap map = TimeMaps.NightOnFlag;

        ArgumentException error = Assert.Throws<ArgumentException>(() => MapState.Enter(map, new PlaceState(map.Id), TimeOfDay.Dusk));

        Assert.Contains("'dusk'", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-1349", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASnapshotOfThisBuildAtATimeThatTheMapCannotTakeIsRefused()
    {
        // D-1349, T-2: a save of this build never holds such a time, so it is a fault of the file.
        Simulation run = Start(TimeMaps.NightOnFlag);
        RunSnapshot snapshot = run.Snapshot();
        RunSnapshot dusk = snapshot with { Map = snapshot.Map! with { Time = TimeOfDay.Dusk } };

        ArgumentException error = Assert.Throws<ArgumentException>(() => Resume(dusk, TimeMaps.NightOnFlag));

        Assert.Contains("'dusk'", error.Message, StringComparison.Ordinal);
        Assert.Contains("day, night", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASnapshotOfAnotherBuildAtATimeThatTheMapCannotTakeTakesTheTimeOfItsFlags()
    {
        // D-1111, D-1349: a patch can edit the time changes, so the load takes the time that the
        // flags give and logs the change.
        Simulation run = Start(TimeMaps.NightOnFlag);
        run.State.Story.Flags.TurnOn(TimeMaps.FlagId);
        RunSnapshot snapshot = run.Snapshot();
        RunSnapshot dusk = snapshot with { Map = snapshot.Map! with { Time = TimeOfDay.Dusk } };
        ResumeDrift drift = ResumeDrift.Of(SnapshotOrigin.OtherBuild, dusk.Tick);

        Simulation resumed = Simulation.Resume(Seed, dusk, TimeMaps.NightOnFlag, TestBattles.Content, TestBattles.Notices, TestStory.Content, DebugIntentHandlers.None, drift);

        Assert.Equal(TimeOfDay.Night, resumed.State.Party.Time);
        LogEntry entry = Assert.Single(drift.Entries, each => each.Message.Contains("time", StringComparison.Ordinal));
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Contains(entry.Fields, field => field.Name == "stored_time" && field.Value == "dusk");
        Assert.Contains(entry.Fields, field => field.Name == "time" && field.Value == "night");
    }

    [Fact]
    public void ASnapshotOfAnotherBuildAtATimeThatTheMapCanTakeKeepsItsTime()
    {
        // D-1349: the stored time wins over the flags while the map can take it.
        Simulation run = Start(TimeMaps.NightOnFlag);
        run.State.Story.Flags.TurnOn(TimeMaps.FlagId);
        ResumeDrift drift = ResumeDrift.Of(SnapshotOrigin.OtherBuild, run.State.Tick);

        Simulation resumed = Simulation.Resume(Seed, run.Snapshot(), TimeMaps.NightOnFlag, TestBattles.Content, TestBattles.Notices, TestStory.Content, DebugIntentHandlers.None, drift);

        Assert.Equal(TimeOfDay.Day, resumed.State.Party.Time);
        Assert.Empty(drift.Entries);
    }

    [Fact]
    public void ASnapshotOfFormatTwentyTakesTheTimeThatItsFlagsGive()
    {
        // D-1349: format 20 predates the time of the map, so its migration takes the time of the
        // flags, as an entry does. A save of format 20 comes from another build, whose enemies the
        // load matches by id (D-1111): the enemy of the day leaves, and the enemy of the night
        // starts on its station.
        Simulation run = Start(TimeMaps.NightOnFlag);
        run.State.Story.Flags.TurnOn(TimeMaps.FlagId);
        string line = SnapshotLines.AsFormatTwenty(RunSnapshotText.Write(run.Snapshot()));
        var reader = new ContentReader(Encoding.UTF8.GetBytes(line), "the test");
        RunSnapshot older = RunSnapshotText.ReadFormatTwenty(ref reader);

        Assert.Null(older.Map!.Time);
        ResumeDrift drift = ResumeDrift.Of(SnapshotOrigin.OtherBuild, older.Tick);
        Simulation resumed = Simulation.Resume(Seed, older, TimeMaps.NightOnFlag, TestBattles.Content, TestBattles.Notices, TestStory.Content, DebugIntentHandlers.None, drift);
        Assert.Equal(TimeOfDay.Night, resumed.State.Party.Time);
        Assert.Equal([TimeMaps.NightEnemy], EnemiesOf(resumed));
    }

    [Fact]
    public void ASnapshotOfFormatTwentyWithATimeIsAnError()
    {
        // D-166, D-1349: format 20 predates the time, so the field fails the read of that format.
        string line = RunSnapshotText.Write(Start(TimeMaps.NightOnFlag).Snapshot());
        ContentException error = Assert.Throws<ContentException>(() =>
        {
            var reader = new ContentReader(Encoding.UTF8.GetBytes(line), "the test");
            _ = RunSnapshotText.ReadFormatTwenty(ref reader);
        });

        Assert.Contains("predates it (D-1349)", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASnapshotOfThisFormatWithNoTimeIsAnError()
    {
        // T-2: an absent time is an error, never the base time.
        string line = SnapshotLines.AsFormatTwenty(RunSnapshotText.Write(Start(TimeMaps.NightOnFlag).Snapshot()));

        ContentException error = Assert.Throws<ContentException>(() => ReadLine(line));

        Assert.Contains("map.time", error.Message, StringComparison.Ordinal);
        Assert.Contains("the field is absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASnapshotWithAnUnknownTimeIsAnError()
    {
        string line = RunSnapshotText.Write(Start(TimeMaps.NightOnFlag).Snapshot())
            .Replace("\"time\":\"day\"", "\"time\":\"noon\"", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => ReadLine(line));

        Assert.Contains("'noon'", error.Message, StringComparison.Ordinal);
    }

    private static Simulation Start(GameMap map) =>
        Simulation.Start(Seed, map, TestBattles.Content, TestBattles.Notices, TestStory.Content, DebugIntentHandlers.None);

    private static Simulation Resume(RunSnapshot snapshot, GameMap map) =>
        Simulation.Resume(Seed, snapshot, map, TestBattles.Content, TestBattles.Notices, TestStory.Content, DebugIntentHandlers.None);

    private static List<string> EnemiesOf(Simulation run) => EnemiesOf(run.State.Party);

    private static List<string> EnemiesOf(MapState party)
    {
        List<string> ids = [];
        foreach (PatrolState patrol in party.Patrols.All)
        {
            ids.Add(patrol.Patrol.Id.Value);
        }

        return ids;
    }

    private static ulong HashOf(MapState party)
    {
        var hasher = new StateHasher();
        party.Hash(hasher);
        return hasher.Finish();
    }

    private static RunSnapshot ReadLine(string line)
    {
        var reader = new ContentReader(Encoding.UTF8.GetBytes(line), "the test");
        return RunSnapshotText.Read(ref reader);
    }
}
