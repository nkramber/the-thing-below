using System;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Edges;
using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The strict reader of an edge rule (D-1321, D-1322, D-1327).</summary>
public sealed class EdgeRuleTests
{
    [Fact]
    public void ARuleReadsItsKindItsJoinsAndAPieceForEachPlace()
    {
        EdgeRule rule = EdgeFixtures.Rule(EdgeFixtures.WaterRuleText);

        Assert.Equal(TileKind.Water, rule.Kind);
        Assert.Equal([TileKind.Bridge], rule.Joins);
        foreach (EdgePlace place in EdgePlaces.All)
        {
            Assert.Equal($"edge.water_{EdgePlaces.NameOf(place)}", rule.PieceOf(place).Value);
        }
    }

    [Fact]
    public void AKindJoinsItselfAndEachJoinAndNoOtherKind()
    {
        EdgeRule rule = EdgeFixtures.Rule(EdgeFixtures.WaterRuleText);

        Assert.True(rule.Joined(TileKind.Water));
        Assert.True(rule.Joined(TileKind.Bridge));
        Assert.False(rule.Joined(TileKind.Grass));
        Assert.False(rule.Joined(TileKind.Mountain));
    }

    [Theory]
    [InlineData("north")]
    [InlineData("south_west")]
    public void ARuleWithAnAbsentPieceFailsWithTheFileAndThePlace(string place)
    {
        // Exit test 2 of PR-53: each pattern of the 8 neighbours needs its pieces, so a rule
        // with an absent piece fails the load and never leaves a pattern with no piece (D-1321).
        string text = EdgeFixtures.WaterRuleText.Replace($"  \"{place}\": \"edge.water_{place}\",\n", string.Empty, StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => EdgeFixtures.Rule(text));

        Assert.Equal(EdgeFixtures.WaterRulePath, error.File);
        Assert.Equal($"pieces.{place}", error.Field);
        Assert.Contains("8 pieces", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("\"kind\": \"water\"", "\"kind\": \"lava\"", "kind", "names no tile kind")]
    [InlineData("\"joins\": [\"bridge\"]", "\"joins\": [\"water\"]", "joins[0]", "its own kind")]
    [InlineData("\"joins\": [\"bridge\"]", "\"joins\": [\"bridge\", \"bridge\"]", "joins[1]", "two times")]
    [InlineData("\"joins\": [\"bridge\"]", "\"joins\": [\"moat\"]", "joins[0]", "names no tile kind")]
    [InlineData("\"north\": \"edge.water_north\"", "\"north\": \"tile.water_north\"", "pieces.north", "the kind 'edge'")]
    [InlineData("\"north\": \"edge.water_north\"", "\"up\": \"edge.water_north\"", "pieces.up", "unknown field")]
    public void AWrongValueFailsWithTheFileAndTheField(string from, string to, string field, string reason)
    {
        string text = EdgeFixtures.WaterRuleText.Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(EdgeFixtures.WaterRuleText, text);

        ContentException error = Assert.Throws<ContentException>(() => EdgeFixtures.Rule(text));

        Assert.Equal(EdgeFixtures.WaterRulePath, error.File);
        Assert.Equal(field, error.Field);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(" \"comment\": \"a test edge rule\",\n")]
    [InlineData(" \"joins\": [\"bridge\"],\n")]
    public void ARuleWithAnAbsentFieldFails(string removed)
    {
        // T-2: an absent value is an error, never a default. A rule with no joins writes `[]`.
        string text = EdgeFixtures.WaterRuleText.Replace(removed, string.Empty, StringComparison.Ordinal);
        Assert.NotEqual(EdgeFixtures.WaterRuleText, text);

        ContentException error = Assert.Throws<ContentException>(() => EdgeFixtures.Rule(text));

        Assert.Equal(EdgeFixtures.WaterRulePath, error.File);
    }

    [Fact]
    public void ARulePathLiesInTheFolderOfTheRules()
    {
        Assert.True(EdgeRule.IsRuleFile("edges/kinds/water.json"));
        Assert.False(EdgeRule.IsRuleFile("edges/maps/overworld.json"));
        Assert.False(EdgeRule.IsRuleFile("rules/maps/overworld.json"));
    }
}
