using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The enemy record, the ability file, and the checks of each id between them and the battle
/// fixture (PR-80, D-557, D-785 to D-787). Each error names the file and the field or the id (T-2).
/// </summary>
public sealed class EnemyRecordTests
{
    private const string BrutePath = "rules/enemies/fixture-brute.json";

    [Fact]
    public void TheCheckoutRecordsHoldTheNumbersOfTheOwner()
    {
        // D-777 and D-786: the two fixture enemies moved to their records with the same ids.
        ContentSet content = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));

        EnemyRecord grunt = content.Battle.Enemy(Id("enemy.fixture_grunt"));
        EnemyRecord brute = content.Battle.Enemy(Id("enemy.fixture_brute"));

        Assert.Equal((30, 8, 2, 90), (grunt.Health, grunt.Attack, grunt.Defense, grunt.Speed));
        Assert.Equal((80, 14, 6, 80), (brute.Health, brute.Attack, brute.Defense, brute.Speed));
        Assert.Empty(grunt.Abilities);
        Assert.Equal("ability.fixture_bash", Assert.Single(brute.Abilities).Value);
        Assert.Equal(BrutePath, brute.File);
    }

    [Fact]
    public void TheFightReadsTheStatsOfTheRecord()
    {
        // Exit test 1: a change of the record alone reaches the enemy of the fight (D-557).
        List<ContentFile> files = [.. ContentFolder.Read(RepositoryRoot.Find())];
        ReplaceIn(files, BrutePath, "\"health\": 80", "\"health\": 55");
        ReplaceIn(files, BrutePath, "\"speed\": 80", "\"speed\": 70");
        ContentSet content = ContentSet.Load(files);

        Battle battle = BattleRuns.BattleOf(BattleRuns.IntoBattle(1, "group.fixture_elite", content.Battle));

        Combatant brute = battle.Enemies[0];
        Assert.Equal("enemy.fixture_brute", brute.Id.Value);
        Assert.Equal(55, brute.FullHealth);
        Assert.Equal(55, brute.Health);
        Assert.Equal(14, brute.Attack);
        Assert.Equal(6, brute.Defense);
        Assert.Equal(70, brute.Speed);
    }

    [Theory]
    [InlineData("comment")]
    [InlineData("id")]
    [InlineData("health")]
    [InlineData("attack")]
    [InlineData("defense")]
    [InlineData("speed")]
    [InlineData("abilities")]
    public void ARecordWithAnAbsentFieldFailsWithTheFileAndTheField(string field)
    {
        // Exit test 2 (T-2).
        string text = WithoutLine(TestBattles.BruteFile, $"\"{field}\":");

        ContentException error = Assert.Throws<ContentException>(() => EnemyRecord.Read(Encoding.UTF8.GetBytes(text), BrutePath));

        Assert.Equal(BrutePath, error.File);
        Assert.Contains(field, error.Message, StringComparison.Ordinal);
        Assert.Contains("absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARecordThatNamesAnAbsentAbilityFailsWithTheFileAndTheId()
    {
        // Exit test 3 (D-785).
        List<ContentFile> files = [.. ContentFolder.Read(RepositoryRoot.Find())];
        ReplaceIn(files, BrutePath, "\"ability.fixture_bash\"", "\"ability.absent\"");

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(files));

        Assert.Equal(BrutePath, error.File);
        Assert.Contains("ability.absent", error.Message, StringComparison.Ordinal);
        Assert.Contains(AbilityList.Path, error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("\"health\": 80", "\"health\": 80.5", "health")]
    [InlineData("\"attack\": 14", "\"attack\": 14.0", "attack")]
    [InlineData("\"speed\": 80", "\"speed\": 8e1", "speed")]
    public void ANumberWithAFractionFailsTheLoad(string from, string to, string field)
    {
        // Exit test 4 (G-2).
        string text = TestBattles.BruteFile.Replace(from, to, StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => EnemyRecord.Read(Encoding.UTF8.GetBytes(text), BrutePath));

        Assert.Equal(BrutePath, error.File);
        Assert.Contains(field, error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("\"health\": 80", "\"health\": 0", "outside 1 to")]
    [InlineData("\"speed\": 80", "\"speed\": 0", "outside 1 to")]
    [InlineData("\"defense\": 6", "\"defense\": -1", "outside 0 to")]
    [InlineData("\"attack\": 14", "\"attack\": 100001", "outside 0 to")]
    [InlineData("\"id\": \"enemy.fixture_brute\"", "\"id\": \"item.fixture_brute\"", "enemy")]
    [InlineData("[\"ability.fixture_bash\"]", "[\"enemy.fixture_bash\"]", "ability")]
    [InlineData("[\"ability.fixture_bash\"]", "[\"ability.fixture_bash\", \"ability.fixture_bash\"]", "two times")]
    [InlineData("\"speed\": 80,", "\"speed\": 80, \"magic\": 3,", "magic")]
    public void ARecordThatBreaksARuleFailsWithTheFile(string from, string to, string reason)
    {
        string text = TestBattles.BruteFile.Replace(from, to, StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => EnemyRecord.Read(Encoding.UTF8.GetBytes(text), BrutePath));

        Assert.Equal(BrutePath, error.File);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("[{ \"id\": \"ability.fixture_bash\" }]", "[{ \"id\": \"ability.fixture_bash\" }, { \"id\": \"ability.fixture_bash\" }]", "two times")]
    [InlineData("[{ \"id\": \"ability.fixture_bash\" }]", "[{ \"id\": \"item.fixture_bash\" }]", "ability")]
    [InlineData("[{ \"id\": \"ability.fixture_bash\" }]", "[{ \"id\": \"ability.fixture_bash\", \"kind\": \"Blade\" }]", "kind")]
    [InlineData("[{ \"id\": \"ability.fixture_bash\" }]", "[{ }]", "absent")]
    public void AnAbilityFileThatBreaksARuleFailsWithTheFile(string from, string to, string reason)
    {
        string text = TestBattles.AbilitiesFile.Replace(from, to, StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => AbilityList.Read(Encoding.UTF8.GetBytes(text), AbilityList.Path));

        Assert.Equal(AbilityList.Path, error.File);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AGroupThatNamesAnEnemyWithNoRecordFails()
    {
        // D-786: the group entry and the record lie in two files, so the battle content checks the id.
        string fixture = TestBattles.FixtureFile.Replace("\"enemy\": \"enemy.fixture_brute\"", "\"enemy\": \"enemy.absent\"", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => TestBattles.Of(fixture));

        Assert.Equal(BattleFixture.Path, error.File);
        Assert.Contains("enemy.absent", error.Message, StringComparison.Ordinal);
        Assert.Contains(EnemyRecord.Folder, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoRecordsWithOneIdFailTheContentSet()
    {
        // D-166: an id is permanent, and one record holds it.
        List<ContentFile> files = [.. ContentFolder.Read(RepositoryRoot.Find())];
        ContentFile brute = files.Find(file => string.CompareOrdinal(file.Path, BrutePath) == 0)
            ?? throw new InvalidOperationException($"The checkout holds no '{BrutePath}'.");
        files.Add(new ContentFile("rules/enemies/second-brute.json", brute.Bytes));

        // The set reads the files in the ordinal order of the paths, so the second file fails.
        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(files));

        Assert.Equal("rules/enemies/second-brute.json", error.File);
        Assert.Contains(BrutePath, error.Message, StringComparison.Ordinal);
        Assert.Contains("enemy.fixture_brute", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AContentSetWithNoAbilityFileFails()
    {
        List<ContentFile> files = [.. ContentFolder.Read(RepositoryRoot.Find())];
        files.RemoveAll(file => string.CompareOrdinal(file.Path, AbilityList.Path) == 0);

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(files));

        Assert.Equal(AbilityList.Path, error.File);
    }

    [Fact]
    public void AnEnemyFileIsAJsonFileOfTheEnemyFolder()
    {
        Assert.True(EnemyRecord.IsEnemyFile("rules/enemies/fixture-grunt.json"));
        Assert.False(EnemyRecord.IsEnemyFile("rules/enemies/notes.txt"));
        Assert.False(EnemyRecord.IsEnemyFile("rules/maps/fixture-dungeon.json"));
        Assert.False(EnemyRecord.IsEnemyFile(AbilityList.Path));
    }

    private static ContentId Id(string value) => ContentId.Parse(value, "test", "id");

    private static void ReplaceIn(List<ContentFile> files, string path, string from, string to)
    {
        int index = files.FindIndex(file => string.CompareOrdinal(file.Path, path) == 0);
        Assert.True(index >= 0, $"The checkout holds no '{path}'.");
        string text = Encoding.UTF8.GetString(files[index].Bytes);
        Assert.Contains(from, text, StringComparison.Ordinal);
        files[index] = new ContentFile(path, Encoding.UTF8.GetBytes(text.Replace(from, to, StringComparison.Ordinal)));
    }

    private static string WithoutLine(string text, string start)
    {
        List<string> kept = [];
        foreach (string line in text.Split('\n'))
        {
            if (!line.TrimStart().StartsWith(start, StringComparison.Ordinal))
            {
                kept.Add(line);
            }
        }

        // The last field keeps no comma after a removal, so the JSON stays whole.
        return string.Join('\n', kept).Replace("80,\n}", "80\n}", StringComparison.Ordinal);
    }
}
