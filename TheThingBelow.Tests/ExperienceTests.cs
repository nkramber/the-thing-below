using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The experience of a battle won, the character level, the level-up, and the join level
/// (D-34, D-363, D-968 to D-974). The class covers exit tests 1, 2, 5, and 7 of PR-67.
/// </summary>
public sealed class ExperienceTests
{
    /// <summary>The seeds of the property test (D-6).</summary>
    private const ulong SeedCount = 1000;

    private static BattleRules Rules => TestBattles.Content.Rules;

    [Fact]
    public void TheExperienceOfOneEnemyFallsAsTheLevelOfACharacterRisesAndEndsPastTheGap()
    {
        // Exit test 1 of PR-67 (D-388, D-968). Each seed picks an enemy level and a base, and the
        // shrunk experience never rises from one character level to the next. It is the full base at
        // the level of the enemy or below it, and zero past the gap.
        for (ulong seed = 0; seed < SeedCount; seed += 1)
        {
            int enemyLevel = 1 + (int)(seed % (ulong)StatCurve.HighestLevel);
            int baseExperience = (int)((seed * 7919) % 5000);
            int before = int.MaxValue;
            for (int level = 1; level <= StatCurve.HighestLevel; level += 1)
            {
                int shrunk = Experience.Shrunk(baseExperience, level, enemyLevel, Rules);
                string where = $"Seed {seed}: level {level} against enemy level {enemyLevel} with base {baseExperience}";
                Assert.True(shrunk <= before, $"{where} gives {shrunk}, above the {before} of the level before (D-388).");
                Assert.InRange(shrunk, 0, baseExperience);
                if (level <= enemyLevel)
                {
                    Assert.True(shrunk == baseExperience, $"{where} gives {shrunk}, and a character at or below the enemy earns the base (D-968).");
                }

                if (level - enemyLevel > Rules.ExperienceGap)
                {
                    Assert.True(shrunk == 0, $"{where} gives {shrunk}, and a character past the gap earns none (D-968).");
                }

                before = shrunk;
            }
        }
    }

    [Theory]
    [InlineData(1, 20)]
    [InlineData(2, 17)]
    [InlineData(3, 14)]
    [InlineData(4, 11)]
    [InlineData(5, 8)]
    [InlineData(6, 0)]
    public void EachLevelAboveTheEnemyCutsItsExperienceUntilTheGap(int characterLevel, int shrunk)
    {
        // D-977: a cut of 1500 for each level above, and zero past a gap of 4. Each result rounds down.
        Assert.Equal(shrunk, Experience.Shrunk(20, characterLevel, 1, Rules));
    }

    [Fact]
    public void AReserveCharacterEarnsHalfAndADownCharacterEarnsNone()
    {
        // Exit test 2 of PR-67 (D-73, D-974). The half applies after the shrink and rounds down (D-969).
        Assert.Equal(25, Experience.EarnedBy(25, ExperienceStanding.Fought));
        Assert.Equal(12, Experience.EarnedBy(25, ExperienceStanding.Reserve));
        Assert.Equal(0, Experience.EarnedBy(25, ExperienceStanding.Down));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(19, 1)]
    [InlineData(20, 2)]
    [InlineData(59, 2)]
    [InlineData(60, 3)]
    [InlineData(15599, 39)]
    [InlineData(15600, 40)]
    public void TheLevelOfATotalIsTheHighestLevelThatTheTotalReaches(int experience, int level)
    {
        // D-971, D-977: level n takes 10 x n x (n - 1) in total.
        Assert.Equal(level, Experience.LevelOf(experience, Rules));
    }

    [Fact]
    public void AWonFightGivesEachCharacterTheExperienceOfEveryEnemyAfterTheWin()
    {
        // D-34, D-975: two grunts of level 1 give 6 each to Marrek at level 1. The experience
        // follows the win, and no level-up comes below the 20 of level 2.
        Simulation run = BattleRuns.IntoBattle(3, "group.test_pair", TestBattles.Exact);
        Assert.Equal(BattleOutcome.Won, BattleRuns.FightToEnd(run, 3));

        List<BattleEvent> after = EventsAfterTheWin(run);
        BattleEvent gained = Assert.Single(after);
        Assert.Equal(BattleEventKind.Experience, gained.Kind);
        Assert.Equal(12, gained.Amount);
        PartyMember marrek = run.State.Characters.Members[0];
        Assert.Equal(12, marrek.Experience);
        Assert.Equal(1, marrek.Level);
    }

    [Fact]
    public void ALevelUpFillsTheHealthAndTheMpAndRaisesItsEvent()
    {
        // Exit test 7 of PR-67 (D-422, D-973). The brute of level 3 and a grunt give 26 to each
        // character of level 1, which passes the 20 of level 2.
        Simulation run = BattleRuns.IntoBattle(5, "group.test_elite", TestBattles.ExactWithParty(3));
        Assert.Equal(BattleOutcome.Won, BattleRuns.FightToEnd(run, 5));

        List<BattleEvent> after = EventsAfterTheWin(run);
        for (int slot = 0; slot < run.State.Characters.Members.Count; slot += 1)
        {
            PartyMember member = run.State.Characters.Members[slot];
            if (member.Down)
            {
                continue;
            }

            Assert.Equal(26, member.Experience);
            Assert.Equal(2, member.Level);
            Assert.Equal(member.Stats.Health, member.Health);
            Assert.Equal(member.Stats.Mp, member.Mp);
            Assert.Equal(member.Stats.Health, BattleRuns.BattleOf(run).Party[slot].Health);
            Assert.Contains(after, played => played.Kind == BattleEventKind.LevelUp && played.Actor.Slot == slot && played.Amount == 2);
        }

        Assert.Equal(TestBattles.MarrekAt(2).Health, run.State.Characters.Members[0].Health);

        // Every experience comes before every level-up (D-975).
        int lastExperience = after.FindLastIndex(played => played.Kind == BattleEventKind.Experience);
        int firstLevelUp = after.FindIndex(played => played.Kind == BattleEventKind.LevelUp);
        Assert.True(lastExperience < firstLevelUp);
    }

    [Fact]
    public void ADownCharacterEarnsNothingFromAWonFight()
    {
        // Exit test 2 of PR-67 (D-974): the second character starts the fight down, and the
        // win gives it no experience and no event.
        Simulation run = RunWithParty(9, "group.test_pair", TestBattles.ExactWithParty(2), (slot, values) =>
            slot == 1 ? values with { Health = 0 } : values);
        Assert.Equal(BattleOutcome.Won, BattleRuns.FightToEnd(run, 9));

        List<BattleEvent> after = EventsAfterTheWin(run);
        Assert.DoesNotContain(after, played => played.Actor.Slot == 1);
        Assert.Equal(0, run.State.Characters.Members[1].Experience);
        Assert.Equal(12, run.State.Characters.Members[0].Experience);
    }

    [Fact]
    public void ACharacterAtTheTopOfTheTableGainsNothingAndShowsNoLine()
    {
        // D-972: the experience stops at the total of level 40. The rules of this test take no cut
        // and the widest gap, so each grunt gives its full experience, and the cap alone stops it.
        int top = TestBattles.TotalOf(StatCurve.HighestLevel);
        StatRow full = TestBattles.MarrekAt(StatCurve.HighestLevel);
        BattleContent content = TestBattles.WithRules(("experience_cut", 0), ("experience_gap", StatCurve.HighestLevel - 1));
        Assert.Equal(6, Experience.Shrunk(6, StatCurve.HighestLevel, 1, content.Rules));
        Simulation run = RunWithParty(4, "group.test_pair", content, (_, values) =>
            values with
            {
                Health = full.Health,
                Growth = new GrowthValues(StatCurve.HighestLevel, top, full.Mp),
                Lessons = TestBattles.LessonsAtLevel(values.Lessons, StatCurve.HighestLevel),
            });
        Assert.Equal(BattleOutcome.Won, BattleRuns.FightToEnd(run, 4));

        Assert.Empty(EventsAfterTheWin(run));
        Assert.Equal(top, run.State.Characters.Members[0].Experience);
    }

    [Fact]
    public void ACharacterWhoJoinsLateStartsAtTheLevelThatContentNames()
    {
        // Exit test 5 of PR-67 (D-363): the join level, the total of that level, full health, and full MP.
        string fixture = TestBattles.FixtureFile.Replace(
            "\"id\": \"character.marrek\", \"row\": \"front\", \"join_level\": 1",
            "\"id\": \"character.marrek\", \"row\": \"front\", \"join_level\": 3",
            StringComparison.Ordinal);

        PartyMember marrek = Assert.Single(PartyState.Start(TestBattles.Of(fixture)).Members);

        Assert.Equal(3, marrek.Level);
        Assert.Equal(TestBattles.TotalOf(3), marrek.Experience);
        Assert.Equal(TestBattles.MarrekAt(3).Health, marrek.Health);
        Assert.Equal(TestBattles.MarrekAt(3).Mp, marrek.Mp);
    }

    [Fact]
    public void ASavePointFillsTheMpAndNoHealth()
    {
        // Exit test 3 of PR-67 (D-389, D-967, D-970).
        PartyState party = HurtParty();

        party.RestoreAtSavePoint();

        PartyMember marrek = party.Members[0];
        Assert.Equal(TestBattles.MarrekAt(2).Mp, marrek.Mp);
        Assert.Equal(10, marrek.Health);
        Assert.Equal([StatusKind.Poison], marrek.Statuses);
    }

    [Fact]
    public void ARestAtAHubFillsTheHealthAndTheMpAndEndsTheLastingStatuses()
    {
        // Exit test 3 of PR-67 (D-36, D-390, D-967, D-970).
        PartyState party = HurtParty();

        party.RestAtHub();

        PartyMember marrek = party.Members[0];
        Assert.Equal(TestBattles.MarrekAt(2).Mp, marrek.Mp);
        Assert.Equal(TestBattles.MarrekAt(2).Health, marrek.Health);
        Assert.Empty(marrek.Statuses);
    }

    [Theory]
    [InlineData(2, 19, 5, "gives level 1")]
    [InlineData(2, 25, 11, "the range at level 2 is 0 to 10")]
    [InlineData(41, 25, 5, "the range is 1 to 40")]
    [InlineData(2, 99999, 5, "the range is 0 to 15600")]
    public void AStoredLevelThatNoRunCanMakeIsRefused(int level, int experience, int mp, string reason)
    {
        // D-971, D-972: the level follows the experience, and the MP stays inside the level.
        CharacterValues stored = new(Marrek, 10, BattleRow.Front, [], new GrowthValues(level, experience, mp), null);

        ArgumentException error = Assert.Throws<ArgumentException>(() =>
            PartyState.Resume(TestBattles.Content, [stored], [], null, false, "the test"));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    private static ContentId Marrek => ContentId.Parse("character.marrek", "test", "character");

    private static PartyState HurtParty() =>
        PartyState.Resume(
            TestBattles.Content,
            [new CharacterValues(Marrek, 10, BattleRow.Front, [StatusKind.Poison], new GrowthValues(2, 25, 1), null)],
            [],
            null,
            false,
            "the test");

    /// <summary>Gives the events after the win of the fight, which the run took since its last take.</summary>
    private static List<BattleEvent> EventsAfterTheWin(Simulation run)
    {
        List<BattleEvent> taken = [.. run.TakeBattleEvents()];
        int won = taken.FindIndex(played => played.Kind == BattleEventKind.Won);
        Assert.True(won >= 0, "The events hold no win.");
        return taken[(won + 1)..];
    }

    /// <summary>Starts a run, changes the stored party of its first tick, and steps it into a battle.</summary>
    private static Simulation RunWithParty(ulong seed, string group, BattleContent content, Func<int, CharacterValues, CharacterValues> change)
    {
        GameMap map = BattleRuns.Map(group);
        RunSnapshot start = Simulation.Start(seed, map, content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None).Snapshot();
        PartySnapshot party = start.Characters ?? throw new InvalidOperationException("The snapshot holds no party.");
        List<CharacterValues> characters = [];
        for (int slot = 0; slot < party.Characters.Count; slot += 1)
        {
            characters.Add(change(slot, party.Characters[slot]));
        }

        RunSnapshot changed = start with { Characters = party with { Characters = characters } };
        Simulation run = Simulation.Resume(seed, changed, map, content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        Assert.NotNull(run.State.Battle);
        return run;
    }
}
