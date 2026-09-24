using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Saves;
using TheThingBelow.Core.Story;
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
/// migration starts each character at its join level with full MP (D-363, D-966). Format 7 and
/// older predate the notice log, and the migration starts the log empty (D-985). Format 8 and
/// older predate the story state, and the migration starts with no flag on, no story scene, and
/// no entry to read (D-540, D-1004). Format 9 and older predate the lessons, and the migration gives
/// each character its start lessons at zero points and the lesson pack of the fixture (D-1018,
/// D-1030). Format 10 and older predate the gear, and the migration gives each character the
/// start gear of the fixture, the party no gold, and a fight no steal try (D-1038, D-1043, D-1045).
/// Format 10 and 11 hold the swap place, and the read drops it, because a swap of lessons needs no
/// place (D-1050).
/// </para>
/// </remarks>
public sealed class SaveFixtureTests
{
    private const string FixtureFolder = "TheThingBelow.Tests/saves";

    /// <summary>The story state that the migration of format 8 and older gives: no flag, no story scene, and no entry (D-1004).</summary>
    private static readonly StoryValues MigratedStory = new([], null, false, false, null);

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
            save.Header.Seed, save.Snapshot, TestMaps.FixtureDungeon, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

        Assert.Equal(120, run.Tick);
    }

    [Fact]
    public void TheStoredSaveOfFormatOnePutsThePartyOnTheSpawnOfTheFirstMap()
    {
        // The migration of D-166: format 1 predates the tile map, so the party enters the
        // first map at its spawn point with that tile walked and no other (D-654).
        SaveDocument save = ReadFormat(1);

        Simulation run = Simulation.Resume(
            save.Header.Seed, save.Snapshot, TestMaps.FixtureDungeon, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

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
            save.Header.Seed, save.Snapshot, TestMaps.Room, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

        Assert.Equal(SaveRuns.FixtureTicks, run.Tick);
        Assert.Equal(save.Snapshot.Map.Walked, run.State.Party.Walked.Rows());
    }

    [Fact]
    public void TheStoredSaveOfFormatOneRunsAgainFromItsTick()
    {
        SaveDocument save = ReadFormat(1);
        Simulation run = Simulation.Resume(
            save.Header.Seed, save.Snapshot, TestMaps.FixtureDungeon, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

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

        // Format 10 sorts before format 2 in the ordinal order of the names, so both lists sort.
        wanted.Sort(StringComparer.Ordinal);
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
            save.Header.Seed, save.Snapshot, TestMaps.Patrolled, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

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
            save.Header.Seed, save.Snapshot, TestMaps.Patrolled, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

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
            save.Header.Seed, save.Snapshot, BattleRuns.Map("group.test_pair"), TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

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
            save.Header.Seed, save.Snapshot, BattleRuns.Map("group.test_pair"), TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

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
            save.Header.Seed, save.Snapshot, BattleRuns.Map("group.test_pair"), TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

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
            save.Header.Seed, save.Snapshot, TestMaps.Patrolled, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

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
        Assert.Equal(RunSnapshotText.Write(save.Snapshot with { Notices = [], Story = MigratedStory, Characters = MigratedParty(save.Snapshot.Characters), Battle = WithSteals(save.Snapshot.Battle) }), RunSnapshotText.Write(run.Snapshot()));
    }

    [Fact]
    public void TheStoredSaveOfFormatSevenStartsTheNoticeLogEmpty()
    {
        // D-985: format 7 predates the notice log, so the migration starts it empty.
        SaveDocument save = ReadFormat(7);
        Assert.Null(save.Snapshot.Notices);

        Simulation run = ResumeInBattle(save);

        Assert.Empty(run.State.NoticeLog.Entries);
    }

    [Fact]
    public void TheStoredSaveOfFormatEightHoldsTheNoticeLog()
    {
        // Exit test 8 of PR-62: the notice log survives a save and a load (D-985). PR-62 wrote
        // format 8 from the migrated run of format 7, with one notice that logs and one that does not.
        SaveDocument save = ReadFormat(8);
        Simulation run = ResumeInBattle(save);

        Assert.Equal([TestBattles.KeptNotice.Value], Values(run.State.NoticeLog.Entries));
        Assert.Equal(20, save.Header.SimulationVersion);
        Assert.Equal(RunSnapshotText.Write(save.Snapshot with { Story = MigratedStory, Characters = MigratedParty(save.Snapshot.Characters), Battle = WithSteals(save.Snapshot.Battle) }), RunSnapshotText.Write(run.Snapshot()));
    }

    [Fact]
    public void TheStoredSaveOfFormatEightReadsTheSameRunAsItsMigratedFormatSevenAsideFromTheLog()
    {
        SaveDocument seven = ReadFormat(7);
        SaveDocument eight = ReadFormat(8);

        Assert.Equal(
            RunSnapshotText.Write(ResumeInBattle(seven).Snapshot() with { Notices = [TestBattles.KeptNotice] }),
            RunSnapshotText.Write(ResumeInBattle(eight).Snapshot()));
    }

    [Fact]
    public void TheStoredSaveOfFormatEightStartsWithNoFlagAndNoStoryScene()
    {
        // D-540: format 8 predates the story state. A load is not an entry to the map, so no
        // entry trigger fires on the next world step (D-1004).
        SaveDocument save = ReadFormat(8);
        Assert.Null(save.Snapshot.Story);

        Simulation run = ResumeInBattle(save);

        Assert.Equal(0, run.State.Story.Flags.Count);
        Assert.False(run.State.Story.Running);
        Assert.False(run.State.Story.EntryPending);
    }

    [Fact]
    public void TheStoredSaveOfFormatNineHoldsTheFlagsAndThePausedStoryScene()
    {
        // Exit tests 7 and 8 of PR-68: the snapshot carries the flag set, the story scene, the
        // pause, and the party that the join made (D-166, D-563). PR-68 wrote format 9 from the
        // story fixture, paused on the first step of the fight.
        SaveDocument save = ReadFormat(9);
        Simulation run = TestStory.Resume(save.Header.Seed, save.Snapshot);

        StoryState story = run.State.Story;
        Assert.Equal(21, save.Header.SimulationVersion);
        Assert.Equal(["flag.test_met", "flag.test_yes"], Values(story.Flags.Values(FlagList.Path)));
        Assert.Equal(TestStory.Fight.Value, story.Scene?.Id.Value);
        Assert.Equal(0, story.Step);
        Assert.Equal(ScenePhase.WaitIntent, story.Phase);
        Assert.True(story.Paused);
        Assert.Equal(2, run.State.Characters.Members.Count);
        Assert.Equal(RunSnapshotText.Write(save.Snapshot with { Characters = MigratedParty(save.Snapshot.Characters), Battle = WithSteals(save.Snapshot.Battle) }), RunSnapshotText.Write(run.Snapshot()));
    }

    [Fact]
    public void TheStoredSaveOfFormatNinePlaysOnToTheEndOfTheFight()
    {
        SaveDocument save = ReadFormat(9);
        Simulation run = TestStory.Resume(save.Header.Seed, save.Snapshot);

        run.Step([Intent.OfPlayer(IntentIds.StoryResume)]);
        StoryRulesTests.PlayWhile(run, () => run.State.Story.Running);

        Assert.True(run.State.Story.Flags.IsOn(ContentId.Parse("flag.test_done", "test", "flag")));
    }

    [Fact]
    public void TheStoredSaveOfFormatTenHoldsTheLessonsThePointsAndThePackAndDropsTheSwapPlace()
    {
        // PR-12 wrote format 10 after one fight won and a swap at a swap place: the salve took the
        // second slot of Marrek, and the cinder went to the end of the lesson pack (D-361, D-1030).
        SaveDocument save = ReadFormat(10);
        Simulation run = ResumeInBattle(save);

        PartyMember marrek = Assert.Single(run.State.Characters.Members);
        Assert.Equal(22, save.Header.SimulationVersion);
        Assert.Equal(["lesson.fixture_hew", "lesson.fixture_salve"], [marrek.Slots[0]?.Value, marrek.Slots[1]?.Value]);
        Assert.Equal((12, 12, 0), (marrek.PointsOf(Lesson("hew")), marrek.PointsOf(Lesson("cinder")), marrek.PointsOf(Lesson("salve"))));
        Assert.Equal(
            ["lesson.fixture_purge", "lesson.fixture_rot", "lesson.fixture_quicken", "lesson.fixture_bolt", "lesson.fixture_cinder"],
            Values(run.State.Characters.LessonPack));
        Assert.DoesNotContain("swap_place", RunSnapshotText.Write(run.Snapshot()), StringComparison.Ordinal);
        Assert.Equal(RunSnapshotText.Write(save.Snapshot with { Characters = WithGear(save.Snapshot.Characters), Battle = WithSteals(save.Snapshot.Battle) }), RunSnapshotText.Write(run.Snapshot()));
    }

    [Fact]
    public void TheStoredSaveOfFormatElevenHoldsTheGearThePackTheGoldAndTheSteals()
    {
        // Exit test 7 of PR-13: the snapshot holds the pack, the gold, and the gear slots. PR-13
        // wrote format 11 from a fight of the tests: Marrek wears the blade and the resist ring,
        // the pack holds the spare shield, and the first steal took the 5 gold of the attacker.
        SaveDocument save = ReadFormat(11);
        Simulation run = ResumeInBattle(save);

        PartyMember marrek = Assert.Single(run.State.Characters.Members);
        Assert.Equal(23, save.Header.SimulationVersion);
        Assert.Equal("gear.test_blade", marrek.Gear[0]?.Value);
        Assert.Equal("gear.test_resist_ring", marrek.Gear[4]?.Value);
        Assert.Equal(1, run.State.Characters.CountOf(ContentId.Parse("gear.test_shield", "test", "gear")));
        Assert.Equal(3, run.State.Characters.CountOf(ContentId.Parse("item.fixture_draught", "test", "item")));
        Assert.Equal(5, run.State.Characters.Gold);

        Battle battle = BattleRuns.BattleOf(run);
        Assert.Equal(1, battle.StealTries);
        Assert.Equal([new StolenEntry(0, 1)], battle.Stolen);

        // The fight reads the gear: the blade adds 5 to attack, and the ring resists fire (D-1036, D-1037).
        Combatant fighter = battle.Party[0];
        Assert.Equal(TestBattles.MarrekAt(1).Attack + 5, fighter.Attack);

        // Exit test 4 of PR-99: format 11 holds no magic and no resistance, and each takes its
        // value from the curve of the tests (D-1052, G-5).
        Assert.Equal((TestBattles.MarrekAt(1).Magic, TestBattles.MarrekAt(1).Resistance), (fighter.Magic, fighter.Resistance));
        Assert.Equal(Affinity.Resist, fighter.Elements.Of(Element.Fire));
        Assert.Equal(RunSnapshotText.Write(save.Snapshot), RunSnapshotText.Write(run.Snapshot()));
    }

    [Fact]
    public void TheStoredSaveOfFormatTwelveHoldsNoSwapPlaceAndReadsTheSevenStatsFromTheCurve()
    {
        // PR-99 wrote format 12 from the save of format 11: the same fight, with no swap place
        // (D-1050). The snapshot holds no stat, so the fight reads each stat from the curve and
        // the gear (D-1052, G-5).
        SaveDocument save = ReadFormat(12);
        Simulation run = ResumeInBattle(save);

        Assert.Equal(24, save.Header.SimulationVersion);
        Assert.DoesNotContain("swap_place", RunSnapshotText.Write(save.Snapshot), StringComparison.Ordinal);
        Combatant fighter = BattleRuns.BattleOf(run).Party[0];
        StatRow curve = TestBattles.MarrekAt(1);
        Assert.Equal((curve.Attack + 5, curve.Magic, curve.Defense, curve.Resistance), (fighter.Attack, fighter.Magic, fighter.Defense, fighter.Resistance));
        Assert.Equal(RunSnapshotText.Write(save.Snapshot), RunSnapshotText.Write(run.Snapshot()));
    }

    [Fact]
    public void TheStoredSaveOfFormatTenGetsNoGearNoGoldAndNoStealTry()
    {
        // D-166: format 10 predates the gear. The fixture of the tests gives Marrek no start gear.
        SaveDocument save = ReadFormat(10);
        Assert.Null(save.Snapshot.Characters?.Gold);

        Simulation run = ResumeInBattle(save);

        PartyState party = run.State.Characters;
        Assert.All(party.Members[0].Gear, Assert.Null);
        Assert.Equal(0, party.Gold);
    }

    [Fact]
    public void TheStoredSaveOfFormatNineGetsTheStartLessonsOfTheFixture()
    {
        // D-166: format 9 predates the lessons, so each character gets its start lessons, and the
        // lesson pack of the fixture holds the rest.
        SaveDocument save = ReadFormat(9);
        Assert.Null(save.Snapshot.Characters?.LessonPack);

        Simulation run = TestStory.Resume(save.Header.Seed, save.Snapshot);

        PartyState party = run.State.Characters;
        Assert.Equal(["lesson.fixture_hew", "lesson.fixture_cinder"], [party.Members[0].Slots[0]?.Value, party.Members[0].Slots[1]?.Value]);
        Assert.Equal(Values(TestBattles.Content.Fixture.LessonPack), Values(party.LessonPack));
    }

    private static ContentId Lesson(string name) => ContentId.Parse($"lesson.fixture_{name}", "test", "lesson");

    private static List<string> Values(IReadOnlyList<ContentId> ids)
    {
        List<string> values = [];
        foreach (ContentId id in ids)
        {
            values.Add(id.Value);
        }

        return values;
    }

    /// <summary>
    /// Gives the party of a save of format 9 or older as the migration of format 11 gives it:
    /// each character with the slots of its level and its start lessons at zero points, the
    /// lesson pack of the fixture, no gear, and no gold (D-166, D-1018, D-1038, D-1043).
    /// </summary>
    private static PartySnapshot? MigratedParty(PartySnapshot? party)
    {
        if (party is null)
        {
            return null;
        }

        BattleContent content = TestBattles.Content;
        List<CharacterValues> characters = [];
        foreach (CharacterValues stored in party.Characters)
        {
            int level = stored.Growth?.Level ?? content.Character(stored.Character).JoinLevel;
            var slots = new ContentId?[content.Rules.SlotsAt(level)];
            var points = new SortedDictionary<string, LessonPoints>(StringComparer.Ordinal);
            foreach (StartLessons entry in content.Fixture.StartLessons)
            {
                if (string.CompareOrdinal(entry.Character.Value, stored.Character.Value) != 0)
                {
                    continue;
                }

                for (int index = 0; index < entry.Lessons.Count; index += 1)
                {
                    slots[index] = entry.Lessons[index];
                    points.Add(entry.Lessons[index].Value, new LessonPoints(entry.Lessons[index], 0));
                }
            }

            characters.Add(stored with { Lessons = new LessonValues(slots, [.. points.Values]) });
        }

        return WithGear(party with { Characters = characters, LessonPack = content.Fixture.LessonPack });
    }

    /// <summary>
    /// Gives the party of a save of format 10 or older as the migration of format 11 gives it:
    /// each character with the start gear of the fixture of the tests, which is none, and no
    /// gold (D-166, D-1038, D-1043).
    /// </summary>
    private static PartySnapshot? WithGear(PartySnapshot? party)
    {
        if (party is null)
        {
            return null;
        }

        List<CharacterValues> characters = [];
        foreach (CharacterValues stored in party.Characters)
        {
            characters.Add(stored with { Gear = stored.Gear ?? new ContentId?[GearRules.SlotCount] });
        }

        return party with { Characters = characters, Gold = party.Gold ?? 0 };
    }

    /// <summary>Gives the battle of a save of format 10 or older as the migration gives it: no steal try (D-166, D-1045).</summary>
    private static BattleValues? WithSteals(BattleValues? battle) =>
        battle is null ? null : battle with { Steals = battle.Steals ?? new StealValues(0, []) };

    private static Simulation ResumeInBattle(SaveDocument save) =>
        Simulation.Resume(save.Header.Seed, save.Snapshot, BattleRuns.Map("group.test_pair"), TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);


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
