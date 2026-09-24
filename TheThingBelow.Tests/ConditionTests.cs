using System;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Story;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The one condition form of content: a tree of all, any, and not over flag leaves (D-543, D-1001, D-1002).</summary>
public sealed class ConditionTests
{
    private const string File = "rules/maps/condition.json";

    [Fact]
    public void TheAlwaysLeafHolds()
    {
        Condition condition = Read("""{ "always": true }""");

        Assert.Equal(ConditionKind.Always, condition.Kind);
        Assert.True(condition.Holds(FlagSet.Empty()));
    }

    [Fact]
    public void AFlagLeafHoldsWhenItsFlagIsOn()
    {
        Condition condition = Read("""{ "flag": "flag.test_met" }""");

        Assert.False(condition.Holds(FlagSet.Empty()));
        Assert.True(condition.Holds(On("flag.test_met")));
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("flag.test_met", false)]
    [InlineData("flag.test_met flag.test_yes", true)]
    public void AnAllNodeHoldsWhenEachChildHolds(string on, bool holds)
    {
        Condition condition = Read("""{ "all": [{ "flag": "flag.test_met" }, { "flag": "flag.test_yes" }] }""");

        Assert.Equal(holds, condition.Holds(On(on)));
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("flag.test_yes", true)]
    [InlineData("flag.test_met flag.test_yes", true)]
    public void AnAnyNodeHoldsWhenOneChildHolds(string on, bool holds)
    {
        Condition condition = Read("""{ "any": [{ "flag": "flag.test_met" }, { "flag": "flag.test_yes" }] }""");

        Assert.Equal(holds, condition.Holds(On(on)));
    }

    [Fact]
    public void ANotNodeHoldsWhenItsChildFails()
    {
        Condition condition = Read("""{ "not": { "flag": "flag.test_met" } }""");

        Assert.True(condition.Holds(FlagSet.Empty()));
        Assert.False(condition.Holds(On("flag.test_met")));
    }

    [Fact]
    public void ANestedTreeReadsEachLevel()
    {
        Condition condition = Read("""{ "all": [{ "flag": "flag.test_met" }, { "not": { "any": [{ "flag": "flag.test_yes" }, { "flag": "flag.test_no" }] } }] }""");

        Assert.True(condition.Holds(On("flag.test_met")));
        Assert.False(condition.Holds(On("flag.test_met flag.test_no")));
    }

    [Theory]
    [InlineData("""{ }""", "holds no field")]
    [InlineData("""{ "flag": "flag.test_met", "always": true }""", "second field 'always'")]
    [InlineData("""{ "some": [] }""", "an unknown field")]
    [InlineData("""{ "always": false }""", "holds false")]
    [InlineData("""{ "all": [] }""", "holds no child")]
    [InlineData("""{ "any": [] }""", "holds no child")]
    [InlineData("""{ "flag": "notice.test_kept" }""", "the kind 'flag'")]
    public void AMalformedNodeFailsWithTheReason(string text, string reason)
    {
        ContentException error = Assert.Throws<ContentException>(() => Read(text));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
        Assert.Contains(File, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ATreeDeeperThanTheLimitFails()
    {
        string text = """{ "flag": "flag.test_met" }""";
        for (int level = 0; level < Condition.MostDepth; level += 1)
        {
            text = $$"""{ "not": {{text}} }""";
        }

        ContentException error = Assert.Throws<ContentException>(() => Read(text));

        Assert.Contains($"deeper than {Condition.MostDepth} levels", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ATreeAtTheLimitReads()
    {
        string text = """{ "flag": "flag.test_met" }""";
        for (int level = 1; level < Condition.MostDepth; level += 1)
        {
            text = $$"""{ "not": {{text}} }""";
        }

        _ = Read(text);
    }

    [Fact]
    public void AnUndeclaredFlagDeepInATreeFailsWithTheFileAndTheId()
    {
        // Exit test 4 of PR-68: a condition that names an undeclared flag fails at load (D-542).
        Condition condition = Read("""{ "all": [{ "flag": "flag.test_met" }, { "not": { "flag": "flag.test_lost" } }] }""");

        ContentException error = Assert.Throws<ContentException>(
            () => condition.RequireDeclared(TestStory.Flags, File, "triggers.trigger.x.condition"));

        Assert.Contains("flag.test_lost", error.Message, StringComparison.Ordinal);
        Assert.Contains(File, error.Message, StringComparison.Ordinal);
        Assert.Contains("D-1003", error.Message, StringComparison.Ordinal);
    }

    private static Condition Read(string text)
    {
        var reader = new ContentReader(Encoding.UTF8.GetBytes($$"""{ "condition": {{text}} }"""), File);
        Condition? condition = null;
        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string _))
        {
            condition = Condition.Read(ref reader);
        }

        reader.ReadFileEnd();
        return condition ?? throw new InvalidOperationException("The test text holds no condition.");
    }

    private static FlagSet On(string ids)
    {
        FlagSet flags = FlagSet.Empty();
        foreach (string id in ids.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            _ = flags.TurnOn(ContentId.Parse(id, "test", "flag"));
        }

        return flags;
    }
}
