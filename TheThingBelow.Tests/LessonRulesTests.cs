using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The rules of the lessons: the use of a form in a fight, the aptitude bonus, the points of a
/// battle won, the growth of each character, the owned lesson set, the swap anywhere outside a fight, and the cast
/// from the menu (D-356 to D-361, D-391, D-393, D-1018 to D-1031). Each exit test of PR-12 in
/// `phase-2-first-playable.md` names its test here.
/// </summary>
public sealed class LessonRulesTests
{
    private const ulong Seed = 12;

    private static readonly ContentId Hew = Id("lesson.fixture_hew");
    private static readonly ContentId Cinder = Id("lesson.fixture_cinder");
    private static readonly ContentId Salve = Id("lesson.fixture_salve");
    private static readonly ContentId Purge = Id("lesson.fixture_purge");
    private static readonly ContentId Quicken = Id("lesson.fixture_quicken");
    private static readonly ContentId Bolt = Id("lesson.fixture_bolt");
    private static readonly BattleTarget FirstEnemy = new(BattleSide.Enemy, 0);
    private static readonly BattleTarget Marrek = new(BattleSide.Party, 0);

    [Fact]
    public void ACharacterUsesTheFirstFormOfAnEquippedLessonInAFixtureBattle()
    {
        // Exit test 1 of PR-12 (D-1027): the rite spends its MP and strikes with its power.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.test_pair", TestBattles.ExactWithParty(1));
        _ = run.TakeBattleEvents();

        run.Step([Intent.OfBattleLesson(Cinder, 0, FirstEnemy)]);

        List<BattleEvent> events = [.. run.TakeBattleEvents()];
        BattleEvent used = events.Find(one => one.Kind == BattleEventKind.Lesson) ?? throw new InvalidOperationException("No lesson event.");
        Assert.Equal(("ability.fixture_cinder", 4), (used.Ability?.Value, used.Amount));
        BattleEvent hit = events.Find(one => one.Kind == BattleEventKind.Hit && one.Actor == Marrek) ?? throw new InvalidOperationException("No hit.");

        // Harm is no aptitude of Marrek: 12 x 14000 x 100 / (102 x 10000) rounds down to 16 (D-771).
        Assert.Equal(16, hit.Amount);
        Assert.Equal(TestBattles.MarrekAt(1).Mp - 4, run.State.Characters.Members[0].Mp);
    }

    [Fact]
    public void TheMainAptitudeRaisesThePowerOfAStrikeBy2500BasisPoints()
    {
        // D-1028: the blade is the main aptitude of Marrek, so the hew strikes at 18750 in place of 15000.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.test_pair", TestBattles.ExactWithParty(1));
        _ = run.TakeBattleEvents();

        run.Step([Intent.OfBattleLesson(Hew, 0, FirstEnemy)]);

        BattleEvent hit = Find(run.TakeBattleEvents(), BattleEventKind.Hit, Marrek);
        Assert.Equal(12 * 18750 * 100 / (102 * 10000), hit.Amount);
    }

    [Fact]
    public void ASideAptitudeGivesHalfTheBonusOnceItsStoryFlagIsOn()
    {
        // Exit test 2 of PR-12 (D-360, D-538): the guard of Marrek gives nothing until its flag is on.
        Simulation run = Start(TestBattles.Content, TestMaps.Room, party => party);
        CharacterRecord marrek = run.State.Characters.Members[0].Record;
        BattleRules rules = run.State.BattleContent.Rules;

        Assert.Equal(2500, LessonRules.BonusOf(marrek, AptitudeKind.Blade, rules, run.State.Story.Flags));
        Assert.Equal(0, LessonRules.BonusOf(marrek, AptitudeKind.Guard, rules, run.State.Story.Flags));
        Assert.Equal(0, LessonRules.BonusOf(marrek, AptitudeKind.Harm, rules, run.State.Story.Flags));

        Assert.True(run.State.Story.Flags.TurnOn(marrek.SideFlag));

        Assert.Equal(1250, LessonRules.BonusOf(marrek, AptitudeKind.Guard, rules, run.State.Story.Flags));
        Assert.Equal(0, LessonRules.BonusOf(marrek, AptitudeKind.Harm, rules, run.State.Story.Flags));
    }

    [Fact]
    public void TheBonusRaisesAStatusChanceToASureHitAtMost()
    {
        // D-1028: the bonus raises the chance of a status too, and no chance passes 10000.
        RunContext context = new(Seed, 0, "test");

        Assert.Equal(7500, LessonRules.RaisedChance(6000, 12500, context));
        Assert.Equal(6750, LessonRules.RaisedChance(6000, 11250, context));
        Assert.Equal(10000, LessonRules.RaisedChance(9000, 12500, context));
        Assert.Equal(6000, LessonRules.RaisedChance(6000, 10000, context));
    }

    [Fact]
    public void ALessonUseRefusesAnUnopenedFormALessonOfNoSlotAndTooLittleMp()
    {
        Simulation run = BattleRuns.IntoBattle(Seed, "group.test_pair", TestBattles.ExactWithParty(1));

        Assert.Contains("opened the forms 0 to 0", Refusal(run, Cinder, 1, FirstEnemy), StringComparison.Ordinal);
        Assert.Contains("which no slot of 'character.marrek' holds", Refusal(run, Salve, 0, Marrek), StringComparison.Ordinal);
        Assert.Null(BattleTurns.RefusalOf(run.State, Choice(Cinder, 0, FirstEnemy)));

        // Two rites of 4 MP spend the 8 MP of level 1, and a third costs more than Marrek holds (D-42).
        run.Step([Intent.OfBattleLesson(Cinder, 0, FirstEnemy)]);
        AssertTheTurnOfMarrek(run);
        run.Step([Intent.OfBattleLesson(Cinder, 0, FirstEnemy)]);
        AssertTheTurnOfMarrek(run);

        Assert.Equal(0, run.State.Characters.Members[0].Mp);
        Assert.Contains("costs 4 MP", Refusal(run, Cinder, 0, FirstEnemy), StringComparison.Ordinal);
    }

    [Fact]
    public void SilenceRefusesARiteInAFightAndLeavesADrill()
    {
        // D-806: silence holds a mark, and PR-12 refuses a rite of the holder.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.test_pair", TestBattles.ExactWithParty(1));
        BattleTurns.GiveStatus(run.State, Marrek, StatusKind.Silence, run.State.Context("test"));

        Assert.Contains("who holds silence", Refusal(run, Cinder, 0, FirstEnemy), StringComparison.Ordinal);
        Assert.Null(BattleTurns.RefusalOf(run.State, Choice(Hew, 0, FirstEnemy)));
    }

    [Fact]
    public void ACureEndsItsStatusesAndABoonGivesItsStatusInAFight()
    {
        // D-1029: the purge ends poison on Marrek, and the quicken gives haste.
        BattleContent content = TestBattles.WithLessonFiles(fixture: WithStartLessons("\"lesson.fixture_purge\", \"lesson.fixture_quicken\""));
        Simulation run = Start(content, BattleRuns.Map("group.test_pair"), party => party);
        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        BattleTurns.GiveStatus(run.State, Marrek, StatusKind.Poison, run.State.Context("test"));
        _ = run.TakeBattleEvents();

        run.Step([Intent.OfBattleLesson(Purge, 0, Marrek)]);

        Assert.Equal(StatusKind.Poison, Find(run.TakeBattleEvents(), BattleEventKind.StatusOff, Marrek).Status);
        Assert.False(BattleRuns.BattleOf(run).Party[0].Statuses.Holds(StatusKind.Poison));

        AssertTheTurnOfMarrek(run);
        run.Step([Intent.OfBattleLesson(Quicken, 0, Marrek)]);

        Assert.Equal(StatusKind.Haste, Find(run.TakeBattleEvents(), BattleEventKind.StatusOn, Marrek).Status);
        Assert.True(BattleRuns.BattleOf(run).Party[0].Statuses.Holds(StatusKind.Haste));
    }

    [Fact]
    public void AHealOrACureAimsAtTheParty()
    {
        BattleContent content = TestBattles.WithLessonFiles(fixture: WithStartLessons("\"lesson.fixture_purge\", \"lesson.fixture_quicken\""));
        Simulation run = Start(content, BattleRuns.Map("group.test_pair"), party => party);
        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);

        Assert.Contains("aims at a character slot", Refusal(run, Purge, 0, FirstEnemy), StringComparison.Ordinal);
        Simulation striker = BattleRuns.IntoBattle(Seed, "group.test_pair", TestBattles.ExactWithParty(1));
        Assert.Contains("a strike aims at an enemy slot", Refusal(striker, Hew, 0, Marrek), StringComparison.Ordinal);
    }

    [Fact]
    public void EveryEquippedLessonGainsPointsFromABattleWonUsedOrNot()
    {
        // Exit test 3 of PR-12 (D-357, D-1019): the attack alone wins, and both lessons gain the
        // six of each grunt, because a lesson at level 1 stands at the level of the grunt.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.test_pair", TestBattles.ExactWithParty(1));

        Assert.Equal(BattleOutcome.Won, BattleRuns.FightToEnd(run, Seed));

        PartyMember marrek = run.State.Characters.Members[0];
        Assert.Equal(12, marrek.PointsOf(Hew));
        Assert.Equal(12, marrek.PointsOf(Cinder));
    }

    [Fact]
    public void ALevelOneLessonOnALevelFortyCharacterGainsTheFullExperienceOfALevelTenEnemy()
    {
        // Exit test 8 of PR-12 (D-1020): the character stands 30 levels above the grunt and earns
        // nothing, and the lesson stands below it and earns the base in full.
        string grunt = TestBattles.GruntFile.Replace("\"level\": 1,", "\"level\": 10,", StringComparison.Ordinal);
        BattleContent content = TestBattles.WithLessonFiles(grunt: grunt);
        int top = TestBattles.TotalOf(StatCurve.HighestLevel);
        StatRow full = TestBattles.MarrekAt(StatCurve.HighestLevel);
        Simulation run = Start(content, BattleRuns.Map("group.test_pair"), party => WithMarrek(party, values => values with
        {
            Health = full.Health,
            Growth = new GrowthValues(StatCurve.HighestLevel, top, full.Mp),
            Lessons = TestBattles.LessonsAtLevel(values.Lessons, StatCurve.HighestLevel),
        }));
        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);

        Assert.Equal(BattleOutcome.Won, BattleRuns.FightToEnd(run, Seed));

        PartyMember marrek = run.State.Characters.Members[0];
        Assert.Equal(top, marrek.Experience);
        Assert.Equal(12, marrek.PointsOf(Hew));
        Assert.Equal(12, marrek.PointsOf(Cinder));
    }

    [Fact]
    public void TheLessonLevelFollowsTheExperienceTable()
    {
        // D-1020: 60 points reach level 3, and the shrink then cuts a grunt of level 1 by two steps.
        BattleRules rules = TestBattles.Content.Rules;

        Assert.Equal(1, LessonRules.LessonLevelOf(0, rules));
        Assert.Equal(2, LessonRules.LessonLevelOf(59, rules));
        Assert.Equal(3, LessonRules.LessonLevelOf(60, rules));
        Assert.Equal(6 * 7000 / 10000, Experience.Shrunk(6, LessonRules.LessonLevelOf(60, rules), 1, rules));
    }

    [Fact]
    public void ADownedCharacterGainsNoLessonPointsAndALessonStopsAtItsLastForm()
    {
        // Exit test 9 of PR-12 (D-1021, D-1022). The second character carries the bolt and is down.
        string fixture = WithStartLessons("\"lesson.fixture_hew\", \"lesson.fixture_cinder\"")
            .Replace("\"start_party\": [\"character.marrek\"]", "\"start_party\": [\"character.marrek\", \"character.test_second\"]", StringComparison.Ordinal)
            .Replace("\"lessons\": [\"lesson.fixture_hew\", \"lesson.fixture_cinder\"] }]", "\"lessons\": [\"lesson.fixture_hew\", \"lesson.fixture_cinder\"] }, { \"character\": \"character.test_second\", \"lessons\": [\"lesson.fixture_bolt\"] }]", StringComparison.Ordinal)
            .Replace(", \"lesson.fixture_bolt\"]", "]", StringComparison.Ordinal);
        BattleContent content = TestBattles.WithLessonFiles(fixture: fixture, exact: true);
        Simulation run = Start(content, BattleRuns.Map("group.test_pair"), party =>
        {
            PartySnapshot changed = WithMarrek(party, values => values with
            {
                Lessons = values.Lessons! with { Points = [new LessonPoints(Cinder, 0), new LessonPoints(Hew, 55)] },
            });
            List<CharacterValues> characters = [changed.Characters[0], changed.Characters[1] with { Health = 0 }];
            return changed with { Characters = characters };
        });
        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        _ = run.TakeBattleEvents();

        Assert.Equal(BattleOutcome.Won, BattleRuns.FightToEnd(run, Seed));

        Assert.Equal(0, run.State.Characters.Members[1].PointsOf(Bolt));
        Assert.Equal(60, run.State.Characters.Members[0].PointsOf(Hew));
        BattleEvent opened = Find(run.TakeBattleEvents(), BattleEventKind.FormOpened, Marrek);
        Assert.Equal(("ability.fixture_cleave", 60), (opened.Ability?.Value, opened.Amount));
    }

    [Fact]
    public void ALessonPassedToANewCharacterStartsAtItsFirstFormAndResumesWhenItComesBack()
    {
        // Exit tests 4 and 5 of PR-12 (D-361): the points belong to the character, not to the lesson.
        BattleContent content = TestBattles.WithParty(2);
        Simulation run = Start(content, TestMaps.Room, party => WithMarrek(party, values => values with
        {
            Lessons = values.Lessons! with { Points = [new LessonPoints(Cinder, 120), new LessonPoints(Hew, 0)] },
        }));
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        Assert.Equal(2, OpenedOf(run, 0, Cinder));

        run.Step([Intent.OfLessonSwap(0, 1, null), Intent.OfLessonSwap(1, 0, Cinder)]);

        Assert.Equal(0, run.State.Characters.Members[1].PointsOf(Cinder));
        Assert.Contains("opened the forms 0 to 0", LessonRules.RefusalOfForm(run.State, 1, Cinder, 1, false), StringComparison.Ordinal);
        Assert.Equal(120, run.State.Characters.Members[0].PointsOf(Cinder));

        run.Step([Intent.OfLessonSwap(1, 0, null), Intent.OfLessonSwap(0, 1, Cinder)]);

        Assert.Equal(2, OpenedOf(run, 0, Cinder));
        Assert.Equal(Cinder.Value, run.State.Characters.Members[0].Slots[1]?.Value);
    }

    /// <summary>Gives the count of the forms that one character opened for one lesson (D-361, D-539).</summary>
    private static int OpenedOf(Simulation run, int character, ContentId lesson) =>
        run.State.BattleContent.Lessons.Lesson(lesson).OpenedAt(run.State.Characters.Members[character].PointsOf(lesson));

    [Fact]
    public void CoreRefusesASwapOfLessonsInAFight()
    {
        // D-1050: a swap works anywhere outside a fight, and a fight refuses it.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.Content);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfLessonSwap(0, 0, Salve)]));

        Assert.Contains("a battle holds the run", error.Message, StringComparison.Ordinal);
        Assert.Equal(Hew.Value, run.State.Characters.Members[0].Slots[0]?.Value);
    }

    [Fact]
    public void ASwapOnTheMapMovesTheSlotLessonToTheEndOfTheLessonPack()
    {
        Simulation run = Start(TestBattles.Content, TestMaps.Room, party => party);
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);

        run.Step([Intent.OfLessonSwap(0, 0, Salve)]);

        PartyState party = run.State.Characters;
        Assert.Equal(Salve.Value, party.Members[0].Slots[0]?.Value);
        Assert.Equal(["lesson.fixture_purge", "lesson.fixture_rot", "lesson.fixture_quicken", "lesson.fixture_bolt", "lesson.fixture_hew"], Values(party.LessonPack));
        Assert.Contains("the lesson pack does not hold", party.RefusalOfSwap(0, 1, Cinder), StringComparison.Ordinal);
        Assert.Contains("holds the slots 0 to 1", party.RefusalOfSwap(0, 2, Hew), StringComparison.Ordinal);
    }

    [Fact]
    public void TheOwnedLessonSetRefusesASecondCopyAndTheErrorNamesTheLesson()
    {
        // Exit test 10 of PR-12 (D-1023, D-1024). The bolt starts outside the lesson pack here.
        BattleContent content = TestBattles.WithLessonFiles(fixture: TestBattles.FixtureFile.Replace(", \"lesson.fixture_bolt\"]", "]", StringComparison.Ordinal));
        Simulation run = Start(content, TestMaps.Room, party => party);
        PartyState party = run.State.Characters;
        LessonRecord bolt = content.Lessons.Lesson(Bolt);
        Assert.False(party.Owns(Bolt));

        party.AddLesson(bolt, run.State.Context("test"));
        Assert.True(party.Owns(Bolt));

        SimulationException error = Assert.Throws<SimulationException>(() => party.AddLesson(bolt, run.State.Context("test")));
        Assert.Contains("lesson.fixture_bolt", error.Message, StringComparison.Ordinal);
        SimulationException carried = Assert.Throws<SimulationException>(() => party.AddLesson(content.Lessons.Lesson(Hew), run.State.Context("test")));
        Assert.Contains("never owns two copies", carried.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ALevelUpOpensAnEmptySlotAndKeepsEachLesson()
    {
        // D-1018: a grunt pair gives 12, and Marrek at 199 experience reaches level 5 and a third slot.
        Simulation run = Start(TestBattles.ExactWithParty(1), BattleRuns.Map("group.test_pair"), party => WithMarrek(party, values => values with
        {
            Growth = new GrowthValues(4, 199, TestBattles.MarrekAt(4).Mp),
            Health = TestBattles.MarrekAt(4).Health,
        }));
        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);

        Assert.Equal(BattleOutcome.Won, BattleRuns.FightToEnd(run, Seed));

        PartyMember marrek = run.State.Characters.Members[0];
        Assert.Equal(5, marrek.Level);
        Assert.Equal([Hew.Value, Cinder.Value, null], [marrek.Slots[0]?.Value, marrek.Slots[1]?.Value, marrek.Slots[2]?.Value]);
    }

    [Fact]
    public void ACureRiteWorksFromTheMenuOutsideBattleAndSilenceStopsIt()
    {
        // Exit test 6 of PR-12 (D-391, D-393). The exact rules hold the hit factor of the heal at
        // 10000, so the salve heals its base of 30 (D-1057, D-1059).
        BattleContent content = TestBattles.WithLessonFiles(fixture: WithStartLessons("\"lesson.fixture_purge\", \"lesson.fixture_salve\""), exact: true);
        Simulation run = Start(content, TestMaps.Room, party => WithMarrek(party, values => values with { Health = 20, Statuses = [StatusKind.Poison] }));
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);

        run.Step([Intent.OfMenuCast(0, Purge, 0, 0), Intent.OfMenuCast(0, Salve, 0, 0)]);

        PartyMember marrek = run.State.Characters.Members[0];
        Assert.Empty(marrek.Statuses);
        Assert.Equal(50, marrek.Health);
        Assert.Equal(TestBattles.MarrekAt(1).Mp - 2 - 3, marrek.Mp);

        Simulation silenced = Start(content, TestMaps.Room, party => WithMarrek(party, values => values with { Statuses = [StatusKind.Poison, StatusKind.Silence] }));
        silenced.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        SimulationException error = Assert.Throws<SimulationException>(() => silenced.Step([Intent.OfMenuCast(0, Purge, 0, 0)]));
        Assert.Contains("who holds silence", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStrikeAClosedMenuAndADownTargetEachRefuseACastFromTheMenu()
    {
        Simulation run = Start(TestBattles.WithLessonFiles(fixture: WithStartLessons("\"lesson.fixture_purge\", \"lesson.fixture_cinder\"")), TestMaps.Room, party => party);

        Assert.Contains("no menu is open", LessonRules.RefusalOfMenuCast(run.State, 0, Purge, 0, 0), StringComparison.Ordinal);
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        Assert.Contains("a heal or a cure alone", LessonRules.RefusalOfMenuCast(run.State, 0, Cinder, 0, 0), StringComparison.Ordinal);
        Assert.Contains("the target slot 1", LessonRules.RefusalOfMenuCast(run.State, 0, Purge, 0, 1), StringComparison.Ordinal);
        Assert.Null(LessonRules.RefusalOfMenuCast(run.State, 0, Purge, 0, 0));
    }

    [Fact]
    public void ASnapshotHoldsTheLessonsThePointsAndTheLessonPackAndNoSwapPlace()
    {
        Simulation run = Start(TestBattles.Content, TestMaps.Room, party => WithMarrek(party, values => values with
        {
            Lessons = values.Lessons! with { Points = [new LessonPoints(Cinder, 30), new LessonPoints(Hew, 7)] },
        }));
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        run.Step([Intent.OfLessonSwap(0, 1, null)]);

        string line = RunSnapshotText.Write(run.Snapshot());
        Simulation resumed = Simulation.Resume(Seed, ReadLine(line), TestMaps.Room, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

        Assert.Equal(line, RunSnapshotText.Write(resumed.Snapshot()));
        Assert.Equal(run.StateHash(), resumed.StateHash());
        Assert.Contains("\"slots\":[{\"slot\":0,\"lesson\":\"lesson.fixture_hew\"}]", line, StringComparison.Ordinal);
        Assert.DoesNotContain("swap_place", line, StringComparison.Ordinal);
        Assert.Equal(30, resumed.State.Characters.Members[0].PointsOf(Cinder));
    }

    [Theory]
    [InlineData("\"slot_count\":2", "\"slot_count\":3", "holds 3 lesson slots, and level 1 gives 2")]
    [InlineData("{\"lesson\":\"lesson.fixture_hew\",\"points\":0}", "{\"lesson\":\"lesson.fixture_hew\",\"points\":61}", "the range is 0 to 60")]
    [InlineData(",{\"lesson\":\"lesson.fixture_hew\",\"points\":0}", "", "with no points")]
    [InlineData("\"lesson_pack\":[\"lesson.fixture_salve\"", "\"lesson_pack\":[\"lesson.fixture_hew\"", "never owns two copies")]
    [InlineData("\"lesson_pack\":[\"lesson.fixture_salve\"", "\"lesson_pack\":[\"lesson.fixture_absent\"", "which the lesson file lacks")]
    [InlineData("{\"slot\":0,\"lesson\":\"lesson.fixture_hew\"},{\"slot\":1", "{\"slot\":1,\"lesson\":\"lesson.fixture_hew\"},{\"slot\":1", "repeats, falls")]
    public void StoredLessonsThatNoRunCanMakeAreRefused(string from, string to, string reason)
    {
        string line = RunSnapshotText.Write(Start(TestBattles.Content, TestMaps.Room, party => party).Snapshot());
        Assert.Contains(from, line, StringComparison.Ordinal);
        string broken = line.Replace(from, to, StringComparison.Ordinal);

        Exception error = Assert.ThrowsAny<Exception>(() => Simulation.Resume(
            Seed, ReadLine(broken), TestMaps.Room, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None));

        Assert.True(error is ArgumentException or ContentException, $"The error is a {error.GetType().Name}: {error.Message}");
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    private static ContentId Id(string value) => ContentId.Parse(value, "test", "id");

    [Fact]
    public void FormatElevenNeedsTheSwapPlaceAndThisFormatRefusesIt()
    {
        // D-166, D-1050: format 10 and 11 hold the swap place, and the read drops it. Format 12
        // dropped the field, so a swap place in it is an unknown field (T-2).
        string line = RunSnapshotText.Write(Start(TestBattles.Content, TestMaps.Room, party => party).Snapshot());
        string withPlace = line.Replace(",\"gold\":", ",\"swap_place\":true,\"gold\":", StringComparison.Ordinal);
        Assert.NotEqual(line, withPlace);

        var eleven = new ContentReader(System.Text.Encoding.UTF8.GetBytes(withPlace), "the test");
        RunSnapshot dropped = RunSnapshotText.ReadFormatEleven(ref eleven);
        Assert.Equal(line, RunSnapshotText.Write(dropped));

        ContentException absent = Assert.Throws<ContentException>(() =>
        {
            var reader = new ContentReader(System.Text.Encoding.UTF8.GetBytes(line), "the test");
            _ = RunSnapshotText.ReadFormatEleven(ref reader);
        });
        Assert.Contains("swap_place", absent.Message, StringComparison.Ordinal);

        ContentException unknown = Assert.Throws<ContentException>(() => ReadLine(withPlace));
        Assert.Contains("swap_place", unknown.Message, StringComparison.Ordinal);
    }

    private static RunSnapshot ReadLine(string line)
    {
        var reader = new ContentReader(System.Text.Encoding.UTF8.GetBytes(line), "the test");
        return RunSnapshotText.Read(ref reader);
    }

    private static BattleChoice Choice(ContentId lesson, int form, BattleTarget target) => new(BattleAction.Lesson, target, null, lesson, form);

    private static string Refusal(Simulation run, ContentId lesson, int form, BattleTarget target) =>
        BattleTurns.RefusalOf(run.State, Choice(lesson, form, target)) ?? throw new InvalidOperationException($"The rules take form {form} of '{lesson.Value}'.");

    /// <summary>Gives the fixture of the tests with other start lessons of Marrek, which leave the lesson pack.</summary>
    private static string WithStartLessons(string lessons)
    {
        string fixture = TestBattles.FixtureFile.Replace(
            "\"lessons\": [\"lesson.fixture_hew\", \"lesson.fixture_cinder\"]",
            $"\"lessons\": [{lessons}]",
            StringComparison.Ordinal);
        List<string> pack = [];
        foreach (string id in new[] { "hew", "cinder", "salve", "purge", "rot", "quicken", "bolt" })
        {
            if (!lessons.Contains($"lesson.fixture_{id}", StringComparison.Ordinal))
            {
                pack.Add($"\"lesson.fixture_{id}\"");
            }
        }

        int start = fixture.IndexOf("\"lesson_pack\": [", StringComparison.Ordinal);
        int end = fixture.IndexOf(']', start) + 1;
        return string.Concat(fixture.AsSpan(0, start), $"\"lesson_pack\": [{string.Join(", ", pack)}]", fixture.AsSpan(end));
    }

    /// <summary>Starts a run, changes the party of its first snapshot, and resumes from it, so each value comes through the checks of a load (D-166).</summary>
    private static Simulation Start(BattleContent content, GameMap map, Func<PartySnapshot, PartySnapshot> change)
    {
        var story = TheThingBelow.Core.Story.StoryContent.Load(
            TheThingBelow.Core.Story.FlagList.Read(System.Text.Encoding.UTF8.GetBytes(TestBattles.NoFlagsFile), TheThingBelow.Core.Story.FlagList.Path),
            [],
            content);
        RunSnapshot start = Simulation.Start(Seed, map, content, TestBattles.Notices, story, DebugIntentHandlers.None).Snapshot();
        PartySnapshot party = start.Characters ?? throw new InvalidOperationException("The snapshot holds no party.");
        return Simulation.Resume(Seed, start with { Characters = change(party) }, map, content, TestBattles.Notices, story, DebugIntentHandlers.None);
    }

    private static PartySnapshot WithMarrek(PartySnapshot party, Func<CharacterValues, CharacterValues> change)
    {
        List<CharacterValues> characters = [change(party.Characters[0])];
        for (int slot = 1; slot < party.Characters.Count; slot += 1)
        {
            characters.Add(party.Characters[slot]);
        }

        return party with { Characters = characters };
    }

    /// <summary>Asserts that the turn of Marrek is open again. Each enemy acts in the step of the choice before it (D-532).</summary>
    private static void AssertTheTurnOfMarrek(Simulation run)
    {
        Battle battle = BattleRuns.BattleOf(run);
        Assert.Equal(BattleOutcome.Running, battle.Outcome);
        Assert.Same(battle.Party[0], battle.Next());
        _ = run.TakeBattleEvents();
    }

    private static BattleEvent Find(IReadOnlyList<BattleEvent> events, BattleEventKind kind, BattleTarget actor)
    {
        foreach (BattleEvent one in events)
        {
            if (one.Kind == kind && one.Actor == actor)
            {
                return one;
            }
        }

        throw new InvalidOperationException($"No event of the kind {BattleEvents.NameOf(kind)} for {actor.Describe()}.");
    }

    private static List<string> Values(IReadOnlyList<ContentId> ids)
    {
        List<string> values = [];
        foreach (ContentId id in ids)
        {
            values.Add(id.Value);
        }

        return values;
    }
}
