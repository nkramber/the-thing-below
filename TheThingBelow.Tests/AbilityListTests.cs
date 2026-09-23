using System;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The reader of the ability file: the id and the effect of each enemy move (D-785, D-955). Each error names the file (T-2).</summary>
public sealed class AbilityListTests
{
    private const string Bash = "{ \"id\": \"ability.fixture_bash\", \"kind\": \"strike\", \"delay\": 100, \"power\": 5000, \"element\": \"none\", \"reach\": \"melee\" }";

    [Fact]
    public void EachKindOfMoveReadsItsFields()
    {
        AbilityList list = Read(TestBattles.AbilitiesFile);

        StrikeAbility bash = Assert.IsType<StrikeAbility>(list.Ability(Id("ability.fixture_bash")));
        Assert.Equal((100, 5000, (Element?)null, AbilityReach.Melee), (bash.Delay, bash.Power, bash.Element, bash.Reach));
        HealAbility mend = Assert.IsType<HealAbility>(list.Ability(Id("ability.test_mend")));
        Assert.Equal((100, 20), (mend.Delay, mend.Heal));
        Assert.Equal(new[] { "ability.fixture_bash", "ability.test_mend", "ability.test_shot" }, IdsOf(list));
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
