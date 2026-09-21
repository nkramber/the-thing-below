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
/// group that a map names (D-757, D-762, D-766, D-778). Each error names the file and the
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
    [InlineData("\"miss_ceiling\": 1500,", "\"miss_ceiling\": 10001,", "miss_ceiling", "outside 0 to 10000")]
    [InlineData("\"hit_high\": 11000,", "\"hit_high\": 8000,", "hit_high", "outside 9000 to")]
    [InlineData("\"item_rate\": 5000", "\"item_rate\": 5000, \"crit_rate\": 1", "crit_rate", "unknown field")]
    public void ARulesFileThatBreaksARuleFailsWithTheField(string from, string to, string field, string reason)
    {
        string text = TestBattles.RulesFile.Replace(from, to, StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => BattleRules.Read(Encoding.UTF8.GetBytes(text), "rules.json"));

        Assert.Contains(field, error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

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
        BattleFixture fixture = ReadGroup(Entries(6, 6));

        Assert.Equal(12, fixture.Groups[0].Entries.Count);
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

    [Theory]
    [InlineData("\"start_party\": [\"character.marrek\"]", "\"start_party\": []", "start_party")]
    [InlineData("\"start_party\": [\"character.marrek\"]", "\"start_party\": [\"character.marrek\", \"character.marrek\"]", "two times")]
    [InlineData("\"item\": \"item.fixture_draught\"", "\"item\": \"item.absent\"", "item.absent")]
    [InlineData("\"row\": \"front\"", "\"row\": \"middle\"", "front, back")]
    [InlineData("\"health\": 60", "\"health\": 0", "outside 1 to")]
    public void AFixtureThatBreaksARuleFailsWithTheFieldOrTheId(string from, string to, string named)
    {
        string text = ReplaceFirst(TestBattles.FixtureFile, from, to);

        ContentException error = Assert.Throws<ContentException>(() => BattleFixture.Read(Encoding.UTF8.GetBytes(text), "fixture.json"));

        Assert.Contains(named, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnIdThatTheFixtureDefinesTwoTimesFails()
    {
        string text = ReplaceFirst(TestBattles.FixtureFile, "\"id\": \"group.other\"", "\"id\": \"group.one\"");

        ContentException error = Assert.Throws<ContentException>(() => BattleFixture.Read(Encoding.UTF8.GetBytes(text), "fixture.json"));

        Assert.Contains("two times", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AMapThatNamesAnAbsentGroupFailsTheContentSet()
    {
        // D-766: the test of D-753 lands in PR-9.
        List<ContentFile> files = [.. ContentFolder.Read(RepositoryRoot.Find())];
        int index = files.FindIndex(file => string.CompareOrdinal(file.Path, BattleFixture.Path) == 0);
        string fixture = Encoding.UTF8.GetString(files[index].Bytes).Replace("group.fixture_pair", "group.renamed", StringComparison.Ordinal);
        files[index] = new ContentFile(BattleFixture.Path, Encoding.UTF8.GetBytes(fixture));

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(files));

        Assert.Contains("group.fixture_pair", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-766", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AContentSetWithNoBattleRulesFails()
    {
        List<ContentFile> files = [.. ContentFolder.Read(RepositoryRoot.Find())];
        files.RemoveAll(file => string.CompareOrdinal(file.Path, BattleRules.Path) == 0);

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(files));

        Assert.Equal(BattleRules.Path, error.File);
    }

    private static BattleFixture ReadGroup(string entries)
    {
        string text = $$"""
        {
         "comment": "A fixture with one group.",
         "characters": [{ "id": "character.marrek", "health": 60, "attack": 12, "defense": 4, "speed": 100, "row": "front" }],
         "groups": [{ "id": "group.big", "boss": false, "enemies": [{{entries}}] }],
         "items": [],
         "start_party": ["character.marrek"],
         "pack": []
        }
        """;
        return BattleFixture.Read(Encoding.UTF8.GetBytes(text), "fixture.json");
    }

    private static string Entries(int standing, int waiting)
    {
        List<string> entries = [];
        for (int index = 0; index < standing + waiting; index += 1)
        {
            string waits = index < standing ? "false" : "true";
            entries.Add($"{{ \"enemy\": \"enemy.fixture_grunt\", \"row\": \"front\", \"waits\": {waits} }}");
        }

        return string.Join(", ", entries);
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
