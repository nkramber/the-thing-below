using TheThingBelow.Core.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// A content id is a lowercase kind, a dot, and a lowercase name (D-646). The id is
/// permanent, and no later entry takes it (D-166).
/// </summary>
public sealed class ContentIdTests
{
    [Theory]
    [InlineData("enemy.cave_rat")]
    [InlineData("item.rusted_key")]
    [InlineData("a.b")]
    [InlineData("lesson.ember_step_2")]
    [InlineData("map.well_3")]
    public void AWellFormedIdParses(string value)
    {
        ContentId id = ContentId.Parse(value, "rules/fixtures/tools.json", "fixtures[0].id");

        Assert.Equal(value, id.Value);
        Assert.Equal(value, id.ToString());
    }

    [Fact]
    public void TheKindAndTheNameSplitAtTheDot()
    {
        ContentId id = ContentId.Parse("enemy.cave_rat", "rules/fixtures/tools.json", "fixtures[0].id");

        Assert.Equal("enemy", id.Kind);
        Assert.Equal("cave_rat", id.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("enemy")]
    [InlineData(".cave_rat")]
    [InlineData("enemy.")]
    [InlineData("enemy..cave_rat")]
    [InlineData("enemy.cave.rat")]
    [InlineData("Enemy.cave_rat")]
    [InlineData("enemy.Cave_rat")]
    [InlineData("enemy.cave-rat")]
    [InlineData("enemy.cave rat")]
    [InlineData("2enemy.cave_rat")]
    [InlineData("enemy.2cave_rat")]
    [InlineData("_enemy.cave_rat")]
    [InlineData("enemy.cave_rat ")]
    [InlineData("enemy/cave_rat")]
    public void AnIdOfAnotherFormFails(string value)
    {
        Assert.False(ContentId.IsWellFormed(value));

        ContentException error = Assert.Throws<ContentException>(
            () => ContentId.Parse(value, "rules/fixtures/tools.json", "fixtures[0].id"));

        Assert.Equal("rules/fixtures/tools.json", error.File);
        Assert.Equal("fixtures[0].id", error.Field);
        Assert.Contains("D-646", error.Message);
    }

    [Fact]
    public void TheErrorNamesTheIdThatTheFileHolds()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => ContentId.Parse("Enemy.Rat", "rules/fixtures/tools.json", "fixtures[3].id"));

        Assert.Contains("Enemy.Rat", error.Message);
        Assert.Contains("rules/fixtures/tools.json", error.Message);
        Assert.Contains("fixtures[3].id", error.Message);
    }
}
