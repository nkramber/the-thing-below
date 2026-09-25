using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The reader of the battle rules file and the battle fixture file, and the check of each
/// group that a map names (D-757, D-766, D-957). `GroupFileTests` reads the group file. Each error names the file and the
/// field (T-2).
/// </summary>
public sealed class BattleFixtureTests
{
    [Fact]
    public void TheCheckoutHoldsTheNumbersOfTheOwner()
    {
        // D-777, D-779, and D-781: the content of the first playable starts from these numbers, and
        // PR-30 tunes them with a measurement (M-4).
        ContentSet content = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));
        BattleRules rules = content.Battle.Rules;

        Assert.Equal(100, rules.AttackDelay);
        Assert.Equal(60, rules.DefendDelay);
        Assert.Equal(60, rules.StepDelay);
        Assert.Equal(500, rules.MissBase);
        Assert.Equal(1500, rules.MissCeiling);
        Assert.Equal(5000, rules.BackRowRate);
        Assert.Equal(5000, rules.FleeBase);
        Assert.Equal(5000, rules.ItemRate);

        GroupRecord elite = content.Battle.Group(ContentId.Parse("group.fixture_elite", "test", "group"));
        Assert.Equal(2, elite.Entries.Count);
        Assert.Equal(1, CountWaiting(elite));
        Assert.Equal(["character.marrek"], IdsOf(content.Battle.Fixture.StartParty));
    }

    [Theory]
    [InlineData("\"attack_delay\": 100,", "", "attack_delay", "absent")]
    [InlineData("\"attack_delay\": 100,", "\"attack_delay\": 0,", "attack_delay", "outside 1 to")]
    [InlineData("\"miss_ceiling\": 1500,", "\"miss_ceiling\": 10000,", "miss_ceiling", "outside 0 to 6666")]
    [InlineData("\"miss_ceiling\": 1500,", "\"miss_ceiling\": 6667,", "miss_ceiling", "outside 0 to 6666")]
    [InlineData("\"hit_high\": 11000,", "\"hit_high\": 8000,", "hit_high", "outside 9000 to")]
    [InlineData("\"item_rate\": 5000", "\"item_rate\": 5000, \"crit_rate\": 1", "crit_rate", "unknown field")]
    [InlineData("\"experience_gap\": 4,", "\"experience_gap\": 40,", "experience_gap", "outside 0 to 39")]
    [InlineData("\"experience_cut\": 1500,", "\"experience_cut\": 10001,", "experience_cut", "outside 0 to 10000")]
    [InlineData("\"level_experience\": [0, 20,", "\"level_experience\": [5, 20,", "level_experience", "level 1 takes the total 5")]
    [InlineData("[0, 20, 60,", "[0, 60, 20,", "level_experience", "above the total")]
    [InlineData("15600]", "15600, 16000]", "level_experience", "one total for each level")]
    [InlineData("\"level_experience\"", "\"level_totals\"", "level_totals", "unknown field")]
    [InlineData("\"lesson_slots\": 2,", "\"lesson_slots\": 0,", "lesson_slots", "outside 1 to 40")]
    [InlineData("\"aptitude_bonus\": 2500,", "\"aptitude_bonus\": -1,", "aptitude_bonus", "outside 0 to")]
    [InlineData("\"aptitude_bonus\": 2500,", "", "aptitude_bonus", "absent")]
    [InlineData("[5, 12, 20, 30]", "[5, 5, 20, 30]", "slot level 5", "above the one before it")]
    [InlineData("[5, 12, 20, 30]", "[1, 12]", "slot level 1", "outside 2 to 40")]
    [InlineData("[5, 12, 20, 30]", "[5, 41]", "slot level 41", "outside 6 to 40")]
    [InlineData(",\n \"lesson_slot_levels\": [5, 12, 20, 30]", "", "lesson_slot_levels", "absent")]
    [InlineData("\"lesson_slots\": 2,", "\"lesson_slots\": 37,", "lesson_slot_levels", "give 41 slots, and a character holds 40 at most")]
    public void ARulesFileThatBreaksARuleFailsWithTheField(string from, string to, string field, string reason)
    {
        string text = TestBattles.RulesFile.Replace(from, to, StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => BattleRules.Read(Encoding.UTF8.GetBytes(text), "rules.json"));

        Assert.Contains(field, error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheRulesTakeTheMissCeilingAndTheSlotCountAtTheirBounds()
    {
        // P3-18, the boundary of the two rows above: a ceiling of 6666 keeps a third of each
        // strike in reach (D-1107), and 36 slots at level 1 with 4 slot levels give 40.
        string text = TestBattles.RulesFile
            .Replace("\"miss_ceiling\": 1500,", "\"miss_ceiling\": 6666,", StringComparison.Ordinal)
            .Replace("\"lesson_slots\": 2,", "\"lesson_slots\": 36,", StringComparison.Ordinal);

        BattleRules rules = BattleRules.Read(Encoding.UTF8.GetBytes(text), "rules.json");

        Assert.Equal(BattleRules.MostMissCeiling, rules.MissCeiling);
        Assert.Equal(BattleRules.MostLessonSlots, rules.SlotsAt(StatCurve.HighestLevel));
    }

    [Theory]
    [InlineData("\"start_party\": [\"character.marrek\"]", "\"start_party\": []", "start_party")]
    [InlineData("\"start_party\": [\"character.marrek\"]", "\"start_party\": [\"character.marrek\", \"character.marrek\"]", "two times")]
    [InlineData("\"count\": 3 }", "\"count\": 0 }", "outside 1 to")]
    [InlineData("\"count\": 3 }", "\"count\": 3, \"gear\": \"gear.test_blade\" }", "not both")]
    [InlineData("\"start_gear\": []", "\"start_gear\": [{ \"character\": \"character.test_second\", \"gear\": [] }]", "not in the start party")]
    [InlineData("\"start_gear\": [],\n", "", "start_gear")]
    [InlineData("\"row\": \"front\"", "\"row\": \"middle\"", "front, back")]
    [InlineData("\"health\": 60", "\"health\": 0", "outside 1 to")]
    [InlineData("\"health\": 60", "\"health\": 1000", "outside 1 to 999")]
    [InlineData("\"mp\": 8", "\"mp\": 1000", "outside 0 to 999")]
    [InlineData("\"level\": 2, \"health\": 66", "\"level\": 2, \"health\": 66.5", "fraction")]
    [InlineData("\"level\": 2, \"health\": 66", "\"level\": 2, \"health\": 50", "falls from 60 at level 1 to 50 at level 2")]
    [InlineData("\"level\": 2,", "\"level\": 3,", "names level 3")]
    [InlineData("\"join_level\": 1", "\"join_level\": 41", "outside 1 to 40")]
    [InlineData("\"join_level\": 1, ", "", "join_level")]
    [InlineData("\"main_aptitude\": \"blade\"", "\"main_aptitude\": \"stealth\"", "the aptitude 'stealth'")]
    [InlineData("\"main_aptitude\": \"blade\"", "\"main_aptitude\": \"guard\"", "never matches the main aptitude")]
    [InlineData("\"main_aptitude\": \"blade\", ", "", "main_aptitude")]
    [InlineData("\"side_flag\": \"flag.test_marrek_side\"", "\"side_flag\": \"notice.test_marrek_side\"", "the kind 'flag'")]
    [InlineData("\"character\": \"character.marrek\", \"lessons\"", "\"character\": \"character.test_second\", \"lessons\"", "not in the start party")]
    [InlineData("\"lesson_pack\": [\"lesson.fixture_salve\"", "\"lesson_pack\": [\"lesson.fixture_hew\"", "never owns two copies")]
    [InlineData("[\"lesson.fixture_hew\", \"lesson.fixture_cinder\"]", "[\"lesson.fixture_hew\", \"lesson.fixture_hew\"]", "never owns two copies")]
    [InlineData(",\n \"lesson_pack\": [\"lesson.fixture_salve\", \"lesson.fixture_purge\", \"lesson.fixture_rot\", \"lesson.fixture_quicken\", \"lesson.fixture_bolt\"]", "", "lesson_pack")]
    public void AFixtureThatBreaksARuleFailsWithTheFieldOrTheId(string from, string to, string named)
    {
        string text = ReplaceFirst(TestBattles.FixtureFile, from, to);

        ContentException error = Assert.Throws<ContentException>(() => BattleFixture.Read(Encoding.UTF8.GetBytes(text), "fixture.json"));

        Assert.Contains(named, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCheckoutHoldsTheLessonNumbersOfTheOwner()
    {
        // D-1018: two slots at level 1, and one more at 5, 12, 20, and 30. D-1028: 2500 basis points.
        ContentSet content = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));
        BattleRules rules = content.Battle.Rules;
        Assert.Equal(2, rules.LessonSlots);
        Assert.Equal([5, 12, 20, 30], rules.LessonSlotLevels);
        Assert.Equal(2500, rules.AptitudeBonus);

        // The cast file gives Marrek the blade and the guard (D-274, D-281).
        CharacterRecord marrek = content.Battle.Character(ContentId.Parse("character.marrek", "test", "character"));
        Assert.Equal((AptitudeKind.Blade, AptitudeKind.Guard), (marrek.MainAptitude, marrek.SideAptitude));
        Assert.Equal("flag.fixture_marrek_side", marrek.SideFlag.Value);
    }

    [Fact]
    public void ACharacterGainsALessonSlotAtEachLevelOfD1018AndAtNoOtherLevel()
    {
        // Exit test 7 of PR-12 (D-1018).
        BattleRules rules = TestBattles.Content.Rules;
        int[] opening = [5, 12, 20, 30];

        Assert.Equal(2, rules.SlotsAt(1));
        for (int level = 2; level <= StatCurve.HighestLevel; level += 1)
        {
            int gained = rules.SlotsAt(level) - rules.SlotsAt(level - 1);
            Assert.True(gained == (Array.IndexOf(opening, level) >= 0 ? 1 : 0), $"Level {level} gains {gained} slots.");
        }

        Assert.Equal(6, rules.SlotsAt(StatCurve.HighestLevel));
    }

    [Fact]
    public void AFormThatNamesAnAbsentAbilityFailsWithTheLesson()
    {
        string lessons = TestBattles.LessonsFile.Replace("\"ability\": \"ability.fixture_blaze\"", "\"ability\": \"ability.absent\"", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => TestBattles.WithLessonFiles(lessons: lessons));

        Assert.Equal(LessonList.Path, error.File);
        Assert.Contains("lesson.fixture_cinder", error.Message, StringComparison.Ordinal);
        Assert.Contains("ability.absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStartLessonThatTheLessonFileLacksFails()
    {
        string fixture = ReplaceFirst(TestBattles.FixtureFile, "\"lesson.fixture_rot\"", "\"lesson.absent\"");

        ContentException error = Assert.Throws<ContentException>(() => TestBattles.WithLessonFiles(fixture: fixture));

        Assert.Contains("lesson.absent", error.Message, StringComparison.Ordinal);
        Assert.Contains("lesson_pack", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MoreStartLessonsThanTheSlotsOfTheJoinLevelFail()
    {
        // D-1018: Marrek joins at level 1 with two slots.
        string fixture = ReplaceFirst(
            TestBattles.FixtureFile,
            "[\"lesson.fixture_hew\", \"lesson.fixture_cinder\"]",
            "[\"lesson.fixture_hew\", \"lesson.fixture_cinder\", \"lesson.fixture_salve\"]");
        fixture = ReplaceFirst(fixture, "[\"lesson.fixture_salve\", ", "[");

        ContentException error = Assert.Throws<ContentException>(() => TestBattles.WithLessonFiles(fixture: fixture));

        Assert.Contains("starts with 3 lessons, and it has 2 slots at level 1", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEnemyThatNamesACureFailsWithTheEnemy()
    {
        // D-1029: a cure and a boon serve the lessons, and the evaluator scores a strike and a heal alone.
        string grunt = TestBattles.GruntFile.Replace("\"abilities\": []", "\"abilities\": [\"ability.fixture_purge\"]", StringComparison.Ordinal);
        Assert.NotEqual(TestBattles.GruntFile, grunt);

        ContentException error = Assert.Throws<ContentException>(() => TestBattles.WithLessonFiles(grunt: grunt));

        Assert.Contains("an enemy move is a strike or a heal", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnIdThatTheFixtureDefinesTwoTimesFails()
    {
        string text = ReplaceFirst(TestBattles.FixtureFile, "\"id\": \"character.test_second\"", "\"id\": \"character.marrek\"");

        ContentException error = Assert.Throws<ContentException>(() => BattleFixture.Read(Encoding.UTF8.GetBytes(text), "fixture.json"));

        Assert.Contains("two times", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ACurveWithALevelMissingAtTheEndFails()
    {
        // Exit test 4 of PR-67 (D-966, D-972): a curve holds one row for each of the 40 levels.
        string last = $", {{ \"level\": 40, \"health\": {TestBattles.MarrekAt(40).Health},";
        int start = TestBattles.FixtureFile.IndexOf(last, StringComparison.Ordinal);
        int end = TestBattles.FixtureFile.IndexOf('}', start) + 1;
        string text = TestBattles.FixtureFile.Remove(start, end - start);

        ContentException error = Assert.Throws<ContentException>(() => BattleFixture.Read(Encoding.UTF8.GetBytes(text), "fixture.json"));

        Assert.Contains("holds 39 rows", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-972", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCheckoutHoldsTheLevelNumbersOfTheOwner()
    {
        // D-977: the table, the cut, the gap, the enemies, and the curve of Marrek.
        ContentSet content = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));
        BattleRules rules = content.Battle.Rules;
        Assert.Equal(1500, rules.ExperienceCut);
        Assert.Equal(4, rules.ExperienceGap);
        for (int level = 1; level <= StatCurve.HighestLevel; level += 1)
        {
            Assert.Equal(10 * level * (level - 1), rules.LevelExperience[level - 1]);
        }

        EnemyRecord grunt = content.Battle.Enemy(ContentId.Parse("enemy.fixture_grunt", "test", "enemy"));
        EnemyRecord brute = content.Battle.Enemy(ContentId.Parse("enemy.fixture_brute", "test", "enemy"));
        Assert.Equal((1, 6), (grunt.Level, grunt.Experience));
        Assert.Equal((3, 20), (brute.Level, brute.Experience));

        CharacterRecord marrek = content.Battle.Character(ContentId.Parse("character.marrek", "test", "character"));
        Assert.Equal(1, marrek.JoinLevel);
        for (int level = 1; level <= StatCurve.HighestLevel; level += 1)
        {
            // D-977 holds five stats. PR-99 adds the magic and the resistance of D-1052, and the
            // curve of the tests sets them to the attack and the defense.
            StatRow row = marrek.At(level);
            Assert.Equal(TestBattles.MarrekAt(level) with { Magic = row.Magic, Resistance = row.Resistance }, row);
        }

        Assert.Equal((6, 3), (marrek.At(1).Magic, marrek.At(1).Resistance));
        Assert.Equal((26, 18), (marrek.At(StatCurve.HighestLevel).Magic, marrek.At(StatCurve.HighestLevel).Resistance));
    }

    [Fact]
    public void AMapThatNamesAnAbsentGroupFailsTheContentSet()
    {
        // D-766 and D-957: exit test 3 of PR-11. The group file of the region of the map holds
        // each group that the map names.
        List<ContentFile> files = [.. ContentFolder.Read(RepositoryRoot.Find())];
        string path = $"{GroupFile.Folder}fixture.json";
        int index = files.FindIndex(file => string.CompareOrdinal(file.Path, path) == 0);
        string groups = Encoding.UTF8.GetString(files[index].Bytes).Replace("group.fixture_pair", "group.renamed", StringComparison.Ordinal);
        files[index] = new ContentFile(path, Encoding.UTF8.GetBytes(groups));

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(files));

        Assert.Contains("group.fixture_pair", error.Message, StringComparison.Ordinal);
        Assert.Equal("rules/maps/fixture-dungeon.json", error.File);
        Assert.Contains("D-957", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AContentSetWithNoBattleRulesFails()
    {
        List<ContentFile> files = [.. ContentFolder.Read(RepositoryRoot.Find())];
        files.RemoveAll(file => string.CompareOrdinal(file.Path, BattleRules.Path) == 0);

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(files));

        Assert.Equal(BattleRules.Path, error.File);
    }

    private static string ReplaceFirst(string text, string from, string to)
    {
        int at = text.IndexOf(from, StringComparison.Ordinal);
        Assert.True(at >= 0, $"The fixture text holds no '{from}'.");
        return string.Concat(text.AsSpan(0, at), to, text.AsSpan(at + from.Length));
    }

    private static int CountWaiting(GroupRecord group)
    {
        int waiting = 0;
        foreach (GroupEntry entry in group.Entries)
        {
            waiting += entry.Waits ? 1 : 0;
        }

        return waiting;
    }

    private static List<string> IdsOf(IReadOnlyList<ContentId> ids)
    {
        List<string> values = [];
        foreach (ContentId id in ids)
        {
            values.Add(id.Value);
        }

        return values;
    }
}
