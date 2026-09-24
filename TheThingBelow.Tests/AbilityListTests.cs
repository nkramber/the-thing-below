using System;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The reader of the ability file: the id and the effect of each move (D-785, D-955, D-1029). Each error names the file (T-2).</summary>
public sealed class AbilityListTests
{
    private const string Bash = "{ \"id\": \"ability.fixture_bash\", \"kind\": \"strike\", \"delay\": 100, \"power\": 5000, \"element\": \"none\", \"reach\": \"melee\", \"status\": \"none\" }";

    [Fact]
    public void EachKindOfMoveReadsItsFields()
    {
        AbilityList list = Read(TestBattles.AbilitiesFile);

        StrikeAbility bash = Assert.IsType<StrikeAbility>(list.Ability(Id("ability.fixture_bash")));
        Assert.Equal((100, 5000, (Element?)null, AbilityReach.Melee), (bash.Delay, bash.Power, bash.Element, bash.Reach));
        HealAbility mend = Assert.IsType<HealAbility>(list.Ability(Id("ability.test_mend")));
        Assert.Equal((100, 20), (mend.Delay, mend.Heal));
        Assert.Equal("ability.fixture_bash", IdsOf(list)[0]);
        Assert.Null(bash.Status);
        CureAbility purge = Assert.IsType<CureAbility>(list.Ability(Id("ability.fixture_purge")));
        Assert.Equal(new[] { StatusKind.Poison, StatusKind.Blind, StatusKind.Silence }, purge.Statuses);
        BoonAbility quicken = Assert.IsType<BoonAbility>(list.Ability(Id("ability.fixture_quicken")));
        Assert.Equal((100, StatusKind.Haste), (quicken.Delay, quicken.Status));
        StrikeAbility rot = Assert.IsType<StrikeAbility>(list.Ability(Id("ability.fixture_rot")));
        Assert.Equal(new StatusChance(StatusKind.Poison, 6000), rot.Status);
    }

    [Fact]
    public void AStrikeReadsItsElementAndItsReach()
    {
        string text = TestBattles.AbilitiesFile.Replace("\"element\": \"none\", \"reach\": \"melee\"", "\"element\": \"fire\", \"reach\": \"any\"", StringComparison.Ordinal);

        StrikeAbility bash = Assert.IsType<StrikeAbility>(Read(text).Ability(Id("ability.fixture_bash")));

        Assert.Equal(Element.Fire, bash.Element);
        Assert.Equal(AbilityReach.Any, bash.Reach);
    }

    [Theory]
    [InlineData(Bash, Bash + ", " + Bash, "two times")]
    [InlineData("\"id\": \"ability.fixture_bash\"", "\"id\": \"item.fixture_bash\"", "ability")]
    [InlineData("\"kind\": \"strike\"", "\"kind\": \"blade\"", "kind")]
    [InlineData("\"delay\": 100, \"power\": 5000", "\"power\": 5000", "absent")]
    [InlineData("\"delay\": 100, \"power\": 5000", "\"delay\": 0, \"power\": 5000", "outside 1 to")]
    [InlineData("\"power\": 5000", "\"power\": 5000, \"heal\": 4", "takes no field 'heal'")]
    [InlineData("\"element\": \"none\"", "\"element\": \"steam\"", "the element 'steam'")]
    [InlineData("\"reach\": \"melee\"", "\"reach\": \"far\"", "the reach 'far'")]
    [InlineData("\"reach\": \"melee\"", "\"reach\": \"melee\", \"range\": 2", "unknown field")]
    [InlineData("\"heal\": 20", "\"heal\": 20, \"power\": 100", "takes no field 'power'")]
    [InlineData("\"heal\": 20", "\"heal\": 20, \"reach\": \"any\"", "takes no field 'reach'")]
    [InlineData("\"delay\": 100, \"heal\": 20", "\"delay\": 100", "absent")]
    [InlineData("\"reach\": \"melee\", \"status\": \"none\"", "\"reach\": \"melee\"", "absent")]
    [InlineData("\"reach\": \"melee\", \"status\": \"none\"", "\"reach\": \"melee\", \"status\": \"doom\"", "the status 'doom'")]
    [InlineData("\"reach\": \"melee\", \"status\": \"none\"", "\"reach\": \"melee\", \"status\": \"none\", \"chance\": 10", "takes no field 'chance'")]
    [InlineData("\"status\": \"poison\", \"chance\": 6000", "\"status\": \"poison\"", "absent")]
    [InlineData("\"status\": \"poison\", \"chance\": 6000", "\"status\": \"poison\", \"chance\": 0", "outside 1 to 10000")]
    [InlineData("\"status\": \"poison\", \"chance\": 6000", "\"status\": \"poison\", \"chance\": 10001", "outside 1 to 10000")]
    [InlineData("\"heal\": 20", "\"heal\": 20, \"status\": \"none\"", "takes no field 'status'")]
    [InlineData("\"statuses\": [\"poison\", \"blind\", \"silence\"]", "\"statuses\": []", "at least one status")]
    [InlineData("\"statuses\": [\"poison\", \"blind\", \"silence\"]", "\"statuses\": [\"blind\", \"poison\"]", "leaves the order")]
    [InlineData("\"statuses\": [\"poison\", \"blind\", \"silence\"]", "\"statuses\": [\"poison\", \"doom\"]", "the status 'doom'")]
    [InlineData("\"statuses\": [\"poison\", \"blind\", \"silence\"]", "\"heal\": 5, \"statuses\": [\"poison\"]", "takes no field 'heal'")]
    [InlineData("\"kind\": \"cure\", \"delay\": 100, \"statuses\": [\"poison\", \"blind\", \"silence\"]", "\"kind\": \"cure\", \"delay\": 100", "absent")]
    [InlineData("\"status\": \"haste\"", "\"status\": \"poison\"", "is not one of haste, regen, shell")]
    [InlineData("\"status\": \"haste\"", "\"status\": \"haste\", \"statuses\": [\"poison\"]", "takes no field 'statuses'")]
    [InlineData("\"kind\": \"heal\"", "\"kind\": \"ward\"", "strike, heal, cure, boon")]
    public void AnAbilityFileThatBreaksARuleFailsWithTheFile(string from, string to, string reason)
    {
        // D-955: a field of the other kind of move is an error, so no field goes unread (T-2).
        int at = TestBattles.AbilitiesFile.IndexOf(from, StringComparison.Ordinal);
        Assert.True(at >= 0, $"The ability text holds no '{from}'.");
        string text = string.Concat(TestBattles.AbilitiesFile.AsSpan(0, at), to, TestBattles.AbilitiesFile.AsSpan(at + from.Length));

        ContentException error = Assert.Throws<ContentException>(() => Read(text));

        Assert.Equal(AbilityList.Path, error.File);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAbsentIdFailsWithTheFileAndTheId()
    {
        ContentException error = Assert.Throws<ContentException>(() => Read(TestBattles.AbilitiesFile).Ability(Id("ability.absent")));

        Assert.Equal(AbilityList.Path, error.File);
        Assert.Contains("ability.absent", error.Message, StringComparison.Ordinal);
    }

    private static AbilityList Read(string text) => AbilityList.Read(Encoding.UTF8.GetBytes(text), AbilityList.Path);

    private static ContentId Id(string value) => ContentId.Parse(value, "test", "id");

    private static string[] IdsOf(AbilityList list)
    {
        string[] ids = new string[list.Ids.Count];
        for (int index = 0; index < ids.Length; index += 1)
        {
            ids[index] = list.Ids[index].Value;
        }

        return ids;
    }
}
