using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The reader of a profile file: the weights, the base chance of a steal, and the steal list (D-383, D-949, D-956, D-958).</summary>
public sealed class ProfileRecordTests
{
    [Fact]
    public void TheCheckoutProfilesHoldTheWeightsAndTheStealLists()
    {
        // D-956: one file for each profile, and each group entry names one.
        ContentSet content = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));

        ProfileRecord grunt = content.Battle.Profile(Id("profile.fixture_grunt"));
        ProfileRecord brute = content.Battle.Profile(Id("profile.fixture_brute"));

        Assert.Equal(new ScoreWeights(60, 2, 60, 100, 1, 150), grunt.Weights);
        Assert.Equal(new ScoreWeights(100, 3, 40, 0, 1, 100), brute.Weights);
        Assert.Equal(4000, grunt.StealChance);
        Assert.Equal(new StealGold(6), Assert.Single(grunt.Steal));
        Assert.Equal(2, brute.Steal.Count);
        Assert.Equal("item.fixture_draught", Assert.IsType<StealItem>(brute.Steal[0]).Item.Value);
        Assert.Equal(new StealGold(15), brute.Steal[1]);
        Assert.Equal(("item.fixture_draught", 1000), (Assert.Single(grunt.Drops).Item.Value, grunt.Drops[0].Chance));
        Assert.Equal(("item.fixture_tonic", 500), (Assert.Single(brute.Drops).Item.Value, brute.Drops[0].Chance));
        Assert.Equal("rules/profiles/fixture-grunt.json", grunt.File);
    }

    [Theory]
    [InlineData("\"damage\": 100, ", "", "damage", "absent")]
    [InlineData("\"damage\": 100", "\"damage\": -1", "damage", "outside 0 to 10000")]
    [InlineData("\"row\": 0", "\"row\": 10001", "row", "outside 0 to 10000")]
    [InlineData("\"row\": 0", "\"row\": 0, \"cunning\": 5", "cunning", "unknown field")]
    [InlineData("\"steal_chance\": 3000", "\"steal_chance\": 10001", "steal_chance", "outside 0 to 10000")]
    [InlineData("\"steal_chance\": 3000", "\"steal_chance\": 0", "steal_chance", "no steal can take")]
    [InlineData("[{ \"item\": \"item.fixture_draught\" }, { \"gold\": 5 }]", "[]", "steal_chance", "the steal list is empty")]
    [InlineData("{ \"gold\": 5 }", "{ \"gold\": 5, \"item\": \"item.fixture_draught\" }", "item", "not both or neither")]
    [InlineData("{ \"gold\": 5 }", "{ }", "item", "not both or neither")]
    [InlineData("{ \"gold\": 5 }", "{ \"gold\": 0 }", "gold", "outside 1 to")]
    [InlineData("\"id\": \"profile.test_attacker\"", "\"id\": \"enemy.test_attacker\"", "id", "profile")]
    [InlineData("\"drops\": []", "\"drops\": [{ \"item\": \"item.fixture_draught\", \"chance\": 0 }]", "chance", "never drops")]
    [InlineData("\"drops\": []", "\"drops\": [{ \"item\": \"item.fixture_draught\", \"chance\": 10001 }]", "chance", "outside 0 to 10000")]
    [InlineData("\"drops\": []", "\"drops\": [{ \"item\": \"item.fixture_draught\" }]", "chance", "absent")]
    [InlineData(",\n \"drops\": []", "", "drops", "absent")]
    public void AProfileThatBreaksARuleFailsWithTheFileAndTheField(string from, string to, string field, string reason)
    {
        string text = TestBattles.AttackerProfileFile.Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(TestBattles.AttackerProfileFile, text);

        ContentException error = Assert.Throws<ContentException>(() => ProfileRecord.Read(Encoding.UTF8.GetBytes(text), TestBattles.AttackerProfilePath));

        Assert.Equal(TestBattles.AttackerProfilePath, error.File);
        Assert.Contains(field, error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStealListThatNamesAnAbsentItemFailsTheBattleContent()
    {
        // D-383: a steal takes an item of the fixture, so the list names a real one (T-2).
        List<ContentFile> files = [.. ContentFolder.Read(RepositoryRoot.Find())];
        ReplaceIn(files, "rules/profiles/fixture-brute.json", "item.fixture_draught", "item.absent");

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(files));

        Assert.Equal("rules/profiles/fixture-brute.json", error.File);
        Assert.Contains("item.absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AGroupThatNamesAnAbsentProfileFailsWithTheGroupFile()
    {
        // D-956: the entry and the profile lie in two files, so the battle content checks the id.
        string groups = TestBattles.GroupsFile.Replace("\"profile\": \"profile.test_attacker\" }] },", "\"profile\": \"profile.absent\" }] },", StringComparison.Ordinal);
        Assert.NotEqual(TestBattles.GroupsFile, groups);

        ContentException error = Assert.Throws<ContentException>(() => TestBattles.OfGroups(groups));

        Assert.Equal(TestBattles.GroupsPath, error.File);
        Assert.Contains("profile.absent", error.Message, StringComparison.Ordinal);
        Assert.Contains("group.one", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AProfileFileIsAJsonFileOfTheProfileFolder()
    {
        Assert.True(ProfileRecord.IsProfileFile("rules/profiles/fixture-grunt.json"));
        Assert.False(ProfileRecord.IsProfileFile("rules/profiles/notes.txt"));
        Assert.False(ProfileRecord.IsProfileFile("rules/enemies/fixture-grunt.json"));
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
}
