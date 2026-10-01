using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Story;
using Xunit;

namespace TheThingBelow.Tests;

public sealed class BotRulesTests
{
    private const string Valid = """
        { "comment": "c", "start_maps": ["map.a", "map.b"], "goal_flag": "flag.goal", "greedy_tick_budget": 900, "random_tick_budget": 300 }
        """;

    [Fact]
    public void TheReaderGivesEachField()
    {
        BotRules rules = Read(Valid);

        Assert.Equal(2, rules.StartMaps.Count);
        Assert.Equal("map.a", rules.StartMaps[0].Value);
        Assert.Equal("map.b", rules.StartMaps[1].Value);
        Assert.Equal("flag.goal", rules.GoalFlag.Value);
        Assert.Equal(900, rules.GreedyBudget);
        Assert.Equal(300, rules.RandomBudget);
    }

    [Theory]
    [InlineData(0UL, "map.a")]
    [InlineData(1UL, "map.b")]
    [InlineData(6UL, "map.a")]
    [InlineData(7UL, "map.b")]
    public void TheSeedPicksTheStartMapInTurn(ulong seed, string map)
    {
        // D-1185: an even seed starts on the first map, and an odd seed on the second.
        Assert.Equal(map, Read(Valid).StartOf(seed).Value);
    }

    [Theory]
    [InlineData("""{ "comment": "c", "start_maps": ["map.a"], "goal_flag": "flag.goal", "random_tick_budget": 9 }""", "greedy_tick_budget")]
    [InlineData("""{ "comment": "c", "start_maps": ["map.a"], "goal_flag": "flag.goal", "greedy_tick_budget": 9 }""", "random_tick_budget")]
    [InlineData("""{ "comment": "c", "start_maps": ["map.a"], "greedy_tick_budget": 9, "random_tick_budget": 9 }""", "goal_flag")]
    [InlineData("""{ "comment": "c", "goal_flag": "flag.goal", "greedy_tick_budget": 9, "random_tick_budget": 9 }""", "start_maps")]
    [InlineData("""{ "comment": "c", "start_maps": [], "goal_flag": "flag.goal", "greedy_tick_budget": 9, "random_tick_budget": 9 }""", "start_maps")]
    [InlineData("""{ "comment": "c", "start_maps": ["map.a"], "goal_flag": "flag.goal", "greedy_tick_budget": 9, "random_tick_budget": 0 }""", "random_tick_budget")]
    [InlineData("""{ "comment": "c", "start_maps": ["map.a"], "goal_flag": "flag.goal", "greedy_tick_budget": 9, "random_tick_budget": 9, "extra": 1 }""", "extra")]
    public void TheReaderRefusesAnAbsentEmptyOrUnknownField(string text, string field)
    {
        ContentException fault = Assert.Throws<ContentException>(() => Read(text));

        Assert.Contains(field, fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AGoalFlagThatTheFlagFileLacksFailsTheLoad()
    {
        FlagList flags = FlagList.Read(Encoding.UTF8.GetBytes("""{ "comment": "c", "flags": [] }"""), FlagList.Path);

        ContentException fault = Assert.Throws<ContentException>(() => Read(Valid).RequireGoalOf(flags));

        Assert.Contains("flag.goal", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStartMapThatTheContentLacksFailsTheLoad()
    {
        ContentException fault = Assert.Throws<ContentException>(() => Read(Valid).RequireStartsOf(MapSet.Of(ContentSet.Load(TheThingBelow.Tools.Content.ContentFolder.Read(RepositoryRoot.Find())).Maps)));

        Assert.Contains("start_maps[0]", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheBotRulesOfTheContentStartOnMapsOfTheContent()
    {
        ContentSet content = ContentSet.Load(TheThingBelow.Tools.Content.ContentFolder.Read(RepositoryRoot.Find()));

        content.Bots.RequireStartsOf(MapSet.Of(content.Maps));
        // D-1185: the fixture dungeon and the fixture hub come first, and PR-17 adds the five places
        // of the first playable, so the bots play each new map (exit test 5 of PR-17).
        Assert.Equal(
            ["map.fixture_dungeon", "map.fixture_hub", "map.village", "map.village_pasture", "map.mining_town", "map.cells_upper", "map.cells_lower"],
            content.Bots.StartMaps.Select(map => map.Value));
    }

    private static BotRules Read(string text) => BotRules.Read(Encoding.UTF8.GetBytes(text), BotRules.Path);
}
