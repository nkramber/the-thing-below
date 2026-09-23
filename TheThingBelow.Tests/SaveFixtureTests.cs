using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
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
/// the spawn point of the first map (D-654). Format 2 predates the enemies, and its migration
/// puts each enemy of the map on the start tile of its station (D-750). Format 3 predates the
/// party, and its migration starts the party of the fixture at full health (D-765). Format 4
/// predates the statuses, and its migration gives each character and each combatant none (D-792).
/// Format 5 and older predate the stream of the evaluator, and the migration opens it at its
/// first value from the seed of the header (D-947). Format 6 and older predate the level, and the
/// migration starts each character at its join level with full MP (D-363, D-966).
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
                EvaluatorAtFirstValue(save.Header.Seed),
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
            save.Header.Seed, save.Snapshot, TestMaps.FixtureDungeon, TestBattles.Content, DebugIntentHandlers.None);

        Assert.Equal(120, run.Tick);
    }

    [Fact]
    public void TheStoredSaveOfFormatOnePutsThePartyOnTheSpawnOfTheFirstMap()
    {
        // The migration of D-166: format 1 predates the tile map, so the party enters the
        // first map at its spawn point with that tile walked and no other (D-654).
        SaveDocument save = ReadFormat(1);

        Simulation run = Simulation.Resume(
            save.Header.Seed, save.Snapshot, TestMaps.FixtureDungeon, TestBattles.Content, DebugIntentHandlers.None);

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
            save.Header.Seed, save.Snapshot, TestMaps.Room, TestBattles.Content, DebugIntentHandlers.None);

        Assert.Equal(SaveRuns.FixtureTicks, run.Tick);
        Assert.Equal(save.Snapshot.Map.Walked, run.State.Party.Walked.Rows());
    }

    [Fact]
    public void TheStoredSaveOfFormatOneRunsAgainFromItsTick()
    {
        SaveDocument save = ReadFormat(1);
        Simulation run = Simulation.Resume(
            save.Header.Seed, save.Snapshot, TestMaps.FixtureDungeon, TestBattles.Content, DebugIntentHandlers.None);

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

    [Fact]
    public void TheStoredSaveOfFormatThreeHoldsEachEnemyOfItsMap()
    {
        // D-750: a load puts each patrol back where it stood, so a save and a reload never
        // move a fight.
        SaveDocument save = ReadFormat(3);

        Assert.Equal(3, save.Header.FormatVersion);
        Assert.NotNull(save.Snapshot.Map);
        Assert.Equal(TestMaps.Patrolled.Id.Value, save.Snapshot.Map.Map.Value);
        Assert.NotNull(save.Snapshot.Map.Enemies);
        Assert.Equal(3, save.Snapshot.Map.Enemies.Count);

        Simulation run = Simulation.Resume(
            save.Header.Seed, save.Snapshot, TestMaps.Patrolled, TestBattles.Content, DebugIntentHandlers.None);

        Assert.Equal(SaveRuns.FixtureTicks, run.Tick);
        IReadOnlyList<PatrolState> enemies = run.State.Party.Patrols.All;
        for (int index = 0; index < enemies.Count; index += 1)
        {
            PatrolValues stored = save.Snapshot.Map.Enemies[index];
            Assert.Equal(stored.Enemy.Value, enemies[index].Patrol.Id.Value);
            Assert.Equal(new TilePoint(stored.X, stored.Y), enemies[index].At);
            Assert.Equal(stored.Facing, enemies[index].Facing);
            Assert.Equal(stored.Stepping, enemies[index].Stepping);
            Assert.Equal(stored.StepTicks, enemies[index].StepTicks);
            Assert.Equal(stored.Target, enemies[index].Target);
        }
    }

    [Fact]
    public void TheStoredSaveOfFormatThreeStartsThePartyOfTheFixture()
    {
        // D-765: format 3 predates the party, so the migration gives the start party of the
        // fixture at full health, and the start pack.
        SaveDocument save = ReadFormat(3);
        Assert.Null(save.Snapshot.Characters);

        Simulation run = Simulation.Resume(
            save.Header.Seed, save.Snapshot, TestMaps.Patrolled, TestBattles.Content, DebugIntentHandlers.None);

        PartyMember marrek = Assert.Single(run.State.Characters.Members);
        Assert.Equal("character.marrek", marrek.Record.Id.Value);
        Assert.Equal(marrek.Stats.Health, marrek.Health);
        Assert.Equal(1, marrek.Level);
        Assert.Equal(marrek.Stats.Mp, marrek.Mp);
        Assert.Equal(3, run.State.Characters.CountOf(ContentId.Parse("item.fixture_draught", "test", "item")));
    }

    [Fact]
    public void TheStoredSaveOfFormatFourHoldsTheBattleThatRuns()
    {
        // D-531: one snapshot covers the map and the battle, so a save inside a battle
        // resumes the same battle.
        SaveDocument save = ReadFormat(4);
        Assert.NotNull(save.Snapshot.Battle);
        Assert.NotNull(save.Snapshot.Characters);

        Simulation run = Simulation.Resume(
            save.Header.Seed, save.Snapshot, BattleRuns.Map("group.test_pair"), TestBattles.Content, DebugIntentHandlers.None);

        Battle battle = BattleRuns.BattleOf(run);
        Assert.Equal(BattleOutcome.Running, battle.Outcome);
        Assert.Equal(19, battle.Enemies[0].Health);
        Assert.Equal(30, battle.Enemies[1].Health);
        Assert.Equal(60, battle.Party[0].Health);
        Assert.Equal(2, run.Tick);
    }

    [Fact]
    public void TheStoredSaveOfFormatFourFightsOnFromItsTick()
    {
        SaveDocument save = ReadFormat(4);
        Simulation run = Simulation.Resume(
            save.Header.Seed, save.Snapshot, BattleRuns.Map("group.test_pair"), TestBattles.Content, DebugIntentHandlers.None);

        BattleOutcome outcome = BattleRuns.FightToEnd(run, save.Header.Seed);

        Assert.NotEqual(BattleOutcome.Running, outcome);
    }

    [Fact]
    public void TheStoredSaveOfFormatFiveHoldsTheStatusesOfTheFight()
    {
        // D-792, D-798: Marrek holds poison with no end and haste to tick 400, and the second
        // grunt holds slow to tick 400. Poison took 3 at the start of the turn of Marrek at 75.
        SaveDocument save = ReadFormat(5);
        Simulation run = Simulation.Resume(
            save.Header.Seed, save.Snapshot, BattleRuns.Map("group.test_pair"), TestBattles.Content, DebugIntentHandlers.None);

        Battle battle = BattleRuns.BattleOf(run);
        Assert.Equal([new StatusValues(StatusKind.Poison, null), new StatusValues(StatusKind.Haste, 400)], battle.Party[0].Statuses.Values());
        Assert.Equal([new StatusValues(StatusKind.Slow, 400)], battle.Enemies[1].Statuses.Values());
        Assert.Equal(7500, battle.Party[0].PushRate);
        Assert.Equal(57, battle.Party[0].Health);
        Assert.NotEqual(BattleOutcome.Running, BattleRuns.FightToEnd(run, save.Header.Seed));
    }

    [Fact]
    public void TheStoredSaveOfFormatThreeRunsAgainFromItsTick()
    {
        SaveDocument save = ReadFormat(3);
        Simulation run = Simulation.Resume(
            save.Header.Seed, save.Snapshot, TestMaps.Patrolled, TestBattles.Content, DebugIntentHandlers.None);

        run.Step([]);

        Assert.Equal(SaveRuns.FixtureTicks + 1, run.Tick);
    }

    [Fact]
    public void TheStoredSaveOfFormatFiveGainsTheStreamOfTheEvaluatorAtItsFirstValue()
    {
        // D-947: no build before PR-11 drew from the stream of the evaluator, so the migration
        // opens it from the seed of the header, and each older stream keeps its position.
        SaveDocument save = ReadFormat(5);

        Assert.Equal(RandomStreams.All.Count, save.Snapshot.Streams.Count);
        Assert.Equal(EvaluatorAtFirstValue(save.Header.Seed), save.Snapshot.Streams[^1]);
        Assert.Equal(new StreamPosition(StreamId.Battle, 0xc4ea2e33f9109a59, 0x0000000000000005), save.Snapshot.Streams[1]);
    }

    [Fact]
    public void TheStoredSaveOfFormatSixReadsTheSameRunAsItsMigratedFormatFive()
    {
        // PR-11 wrote format 6 from the migrated snapshot of format 5, so the two resume alike.
        SaveDocument five = ReadFormat(5);
        SaveDocument six = ReadFormat(6);

        Assert.Equal(RunSnapshotText.Write(ResumeInBattle(five).Snapshot()), RunSnapshotText.Write(ResumeInBattle(six).Snapshot()));
        Assert.Equal(17, six.Header.SimulationVersion);
    }

    [Fact]
    public void TheStoredSaveOfFormatSixStartsEachCharacterAtItsJoinLevelWithFullMp()
    {
        // D-363, D-966: format 6 predates the level, so the migration gives the join level,
        // the total of that level, and full MP. The health of the save stays.
        SaveDocument save = ReadFormat(6);
        CharacterValues stored = Assert.Single(save.Snapshot.Characters!.Characters);
        Assert.Null(stored.Growth);

        PartyMember marrek = Assert.Single(ResumeInBattle(save).State.Characters.Members);

        Assert.Equal(1, marrek.Level);
        Assert.Equal(0, marrek.Experience);
        Assert.Equal(8, marrek.Mp);
        Assert.Equal(stored.Health, marrek.Health);
    }

    [Fact]
    public void TheStoredSaveOfFormatSevenHoldsTheLevelTheExperienceAndTheMp()
    {
        // Exit test 6 of PR-67: the snapshot holds the level, the experience, and the MP.
        SaveDocument save = ReadFormat(7);
        Simulation run = ResumeInBattle(save);

        PartyMember marrek = Assert.Single(run.State.Characters.Members);
        Assert.Equal(2, marrek.Level);
        Assert.Equal(25, marrek.Experience);
        Assert.Equal(5, marrek.Mp);

        // The fight that runs reads the stats of level 2 (D-966).
        Combatant fighter = BattleRuns.BattleOf(run).Party[0];
        Assert.Equal(TestBattles.MarrekAt(2).Health, fighter.FullHealth);
        Assert.Equal(TestBattles.MarrekAt(2).Attack, fighter.Attack);
        Assert.Equal(RunSnapshotText.Write(save.Snapshot), RunSnapshotText.Write(run.Snapshot()));
    }

    private static Simulation ResumeInBattle(SaveDocument save) =>
        Simulation.Resume(save.Header.Seed, save.Snapshot, BattleRuns.Map("group.test_pair"), TestBattles.Content, DebugIntentHandlers.None);


    private static StreamPosition EvaluatorAtFirstValue(ulong seed)
    {
        RandomStream evaluator = RandomStreams.Open(seed, StreamId.Evaluator);
        return new StreamPosition(StreamId.Evaluator, evaluator.Generator.State, evaluator.Generator.Increment);
    }

    private static SaveDocument ReadFormat(int version)
    {
        string path = PathOfFormat(version);
        return SaveText.Read(File.ReadAllText(path), path);
    }

    private static string PathOfFormat(int version) =>
        RepositoryRoot.PathTo($"{FixtureFolder}/format-{version.ToString(System.Globalization.CultureInfo.InvariantCulture)}.json");
}
