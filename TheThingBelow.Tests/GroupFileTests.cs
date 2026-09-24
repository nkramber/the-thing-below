using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The reader of the group file of a region: the size of each group, the profile of each
/// enemy, and the region that the path names (D-535, D-762, D-778, D-956, D-957). Each error
/// names the file and the field (T-2).
/// </summary>
public sealed class GroupFileTests
{
    private const string Path = "rules/groups/big.json";

    [Fact]
    public void AGroupOfThirteenEnemiesFailsTheLoad()
    {
        // D-762: twelve at most, the waiting ones included.
        ContentException error = Assert.Throws<ContentException>(() => ReadGroup(Entries(6, 7)));

        Assert.Contains("13 enemies", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-762", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AGroupOfTwelveLoads()
    {
        GroupFile file = ReadGroup(Entries(6, 6));

        Assert.Equal(12, file.Groups[0].Entries.Count);
        Assert.Equal("region.big", file.Region.Value);
        Assert.Equal("profile.test_attacker", file.Groups[0].Entries[11].Profile.Value);
    }

    [Theory]
    [InlineData(7, 0, "starts 7 enemies")]
    [InlineData(0, 2, "starts 0 enemies")]
    public void AGroupThatNoFieldHoldsFailsTheLoad(int standing, int waiting, string reason)
    {
        // D-759, D-778: the field holds one to six at the start.
        ContentException error = Assert.Throws<ContentException>(() => ReadGroup(Entries(standing, waiting)));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEntryWithNoProfileFailsWithTheField()
    {
        // D-956: each entry names the profile that the evaluator reads.
        string entries = Entries(1, 0).Replace(", \"profile\": \"profile.test_attacker\"", string.Empty, StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => ReadGroup(entries));

        Assert.Equal(Path, error.File);
        Assert.Contains("profile", error.Message, StringComparison.Ordinal);
        Assert.Contains("absent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARegionThatThePathDoesNotNameFails()
    {
        // D-957: the region `region.big` lives in `rules/groups/big.json` alone.
        string text = Text(Entries(1, 0));

        ContentException error = Assert.Throws<ContentException>(() => GroupFile.Read(Encoding.UTF8.GetBytes(text), "rules/groups/other.json"));

        Assert.Equal("rules/groups/other.json", error.File);
        Assert.Contains("region.big", error.Message, StringComparison.Ordinal);
        Assert.Contains(Path, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AGroupThatTheFileDefinesTwoTimesFails()
    {
        string group = $"{{ \"id\": \"group.big\", \"boss\": false, \"enemies\": [{Entries(1, 0)}] }}";
        string text = Text(Entries(1, 0)).Replace("\"groups\": [", $"\"groups\": [{group}, ", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => GroupFile.Read(Encoding.UTF8.GetBytes(text), Path));

        Assert.Contains("group.big", error.Message, StringComparison.Ordinal);
        Assert.Contains("two times", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoFilesOfOneRegionFailTheBattleContent()
    {
        // D-957: a region has one group file, so a map reads one list of groups.
        BattleContent tests = TestBattles.Content;
        GroupFile first = GroupFile.Read(Encoding.UTF8.GetBytes(Text(Entries(1, 0))), Path);
        GroupFile second = GroupFile.Read(Encoding.UTF8.GetBytes(Text(Entries(1, 0)).Replace("group.big", "group.big_second", StringComparison.Ordinal)), Path);

        ContentException error = Assert.Throws<ContentException>(
            () => new BattleContent(tests.Rules, tests.Fixture, tests.Enemies, tests.Abilities, tests.Lessons, [first, second], tests.Profiles));

        Assert.Contains("a second group file takes the region 'region.big'", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(4, 1)]
    [InlineData(0, 9)]
    [InlineData(3, 3)]
    public void AWaitingColumnThatTheFieldHoldsLoads(int elites, int commons)
    {
        // D-963: the column holds 288 art pixels, so four elites and one common fit exactly.
        BattleContent content = TestBattles.OfGroups(WaveFile(elites, commons));

        Assert.Equal(1 + elites + commons, content.Group(ContentId.Parse("group.wave", Path, "id")).Entries.Count);
    }

    [Theory]
    [InlineData(5, 0, 320)]
    [InlineData(0, 10, 320)]
    [InlineData(4, 2, 320)]
    public void AWaitingColumnTallerThanTheFieldFailsWithTheGroupAndTheHeight(int elites, int commons, int height)
    {
        // D-963: four elite bodies fit, and not six. The error names the group, the height,
        // and the limit (T-2).
        ContentException error = Assert.Throws<ContentException>(() => TestBattles.OfGroups(WaveFile(elites, commons)));

        Assert.Equal(TestBattles.GroupsPath, error.File);
        Assert.Contains("group.wave", error.Message, StringComparison.Ordinal);
        Assert.Contains($"a column of {height} art pixels", error.Message, StringComparison.Ordinal);
        Assert.Contains($"{BattleFixture.MostWaitingHeight} at most", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-963", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AGroupFileIsAJsonFileOfTheGroupFolder()
    {
        Assert.True(GroupFile.IsGroupFile("rules/groups/fixture.json"));
        Assert.False(GroupFile.IsGroupFile("rules/groups/notes.txt"));
        Assert.False(GroupFile.IsGroupFile("rules/profiles/fixture-grunt.json"));
    }

    private static GroupFile ReadGroup(string entries) => GroupFile.Read(Encoding.UTF8.GetBytes(Text(entries)), Path);

    private static string Text(string entries) => $$"""
        {
         "comment": "A group file with one group.",
         "region": "region.big",
         "groups": [{ "id": "group.big", "boss": false, "enemies": [{{entries}}] }]
        }
        """;

    /// <summary>Gives a group file of the test region with one wave: the waiting brutes, then the waiting grunts.</summary>
    private static string WaveFile(int elites, int commons)
    {
        List<string> waiting = [];
        for (int index = 0; index < elites + commons; index += 1)
        {
            waiting.Add(index < elites ? "enemy.fixture_brute" : "enemy.fixture_grunt");
        }

        return TestBattles.WaveGroupsFile(waiting);
    }

    private static string Entries(int standing, int waiting)
    {
        List<string> entries = [];
        for (int index = 0; index < standing + waiting; index += 1)
        {
            string waits = index < standing ? "false" : "true";
            entries.Add($"{{ \"enemy\": \"enemy.fixture_grunt\", \"row\": \"front\", \"waits\": {waits}, \"profile\": \"profile.test_attacker\" }}");
        }

        return string.Join(", ", entries);
    }
}
