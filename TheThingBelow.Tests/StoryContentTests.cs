using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Story;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The load checks of the story content: each id that a story scene or a trigger shares with another file (T-2, D-542, D-543).</summary>
public sealed class StoryContentTests
{
    [Fact]
    public void TheFixtureLoadsEveryStoryScene()
    {
        Assert.Equal(["scene.test_fight", "scene.test_meet", "scene.test_victory"], Ids(TestStory.Content));
        TestStory.Content.RequireScenesOf(TestStory.Map);
    }

    [Theory]
    [InlineData("""{ "id": "step.s1", "kind": "set_flag", "flag": "flag.test_lost" }""", "steps[0].flag", "flag.test_lost")]
    [InlineData("""{ "id": "step.s2", "kind": "choose", "options": [{ "line": "line.test_yes", "flag": "flag.test_yes" }, { "line": "line.test_no", "flag": "flag.test_lost" }] }""", "steps[0].options[1].flag", "flag.test_lost")]
    [InlineData("""{ "id": "step.s3", "kind": "join", "character": "character.test_absent" }""", "steps[0].character", "character.test_absent")]
    [InlineData("""{ "id": "step.s4", "kind": "say", "speaker": "character.test_absent", "line": "line.test_greet" }""", "steps[0].speaker", "character.test_absent")]
    [InlineData("""{ "id": "step.s5", "kind": "start_battle", "group": "group.test_absent" }""", "steps[0].group", "group.test_absent")]
    [InlineData("""{ "id": "step.s6", "kind": "hide", "actor": "character.test_second" }""", "steps[0].actor", "is not on the map at this step")]
    [InlineData("""{ "id": "step.s7", "kind": "move", "actor": "character.test_second", "path": ["west"] }""", "steps[0].actor", "is not on the map at this step")]
    [InlineData("""{ "id": "step.s8", "kind": "face", "actor": "character.test_second", "facing": "west" }""", "steps[0].actor", "is not on the map at this step")]
    public void AStepThatNamesAnIdOfNoOtherFileFailsWithTheStorySceneTheStepAndTheId(string step, string field, string reason)
    {
        ContentException error = Assert.Throws<ContentException>(() => Load(Scene(step)));

        Assert.Contains("rules/scenes/probe.json", error.Message, StringComparison.Ordinal);
        Assert.Contains($"scene.test_probe.{field}", error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASecondShowOfACastMemberOnTheMapFails()
    {
        string show = """{ "id": "step.s9", "kind": "show", "actor": "character.test_second", "at": "marker.test_story_door", "facing": "west" }""";
        string again = show.Replace("step.s9", "step.s9_again", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => Load(Scene(show, again)));

        Assert.Contains("steps[1].actor", error.Message, StringComparison.Ordinal);
        Assert.Contains("already on the map", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AShowAfterAHideOfTheSameCastMemberLoads()
    {
        string show = """{ "id": "step.s10", "kind": "show", "actor": "character.test_second", "at": "marker.test_story_door", "facing": "west" }""";
        string hide = """{ "id": "step.s11", "kind": "hide", "actor": "character.test_second" }""";

        string again = show.Replace("step.s10", "step.s10_again", StringComparison.Ordinal);

        _ = Load(Scene(show, hide, again));
    }

    [Fact]
    public void TwoStoryScenesWithOneIdFail()
    {
        ContentException error = Assert.Throws<ContentException>(() => StoryContent.Load(
            TestStory.Flags,
            [TestStory.Scene(TestStory.MeetFile, "one"), TestStory.Scene(TestStory.MeetFile, "two")],
            TestBattles.Content));

        Assert.Contains("scene.test_meet", error.Message, StringComparison.Ordinal);
        Assert.Contains("permanent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ATriggerOfAnAbsentStorySceneFailsWithTheMapAndTheTrigger()
    {
        StoryContent none = TestBattles.Story;

        ContentException error = Assert.Throws<ContentException>(() => none.RequireScenesOf(TestStory.Map));

        Assert.Contains("rules/maps/test-story.json", error.Message, StringComparison.Ordinal);
        Assert.Contains("trigger.test_story_meet", error.Message, StringComparison.Ordinal);
        Assert.Contains("scene.test_meet", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARunRefusesAMapWhoseTriggerNamesAnAbsentStoryScene()
    {
        Assert.Throws<ContentException>(() => Simulation.Start(
            1, TestStory.Map, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None));
    }

    [Fact]
    public void AConditionThatNamesAnUndeclaredFlagFailsWithTheMapAndTheId()
    {
        // Exit test 4 of PR-68 (D-542, D-543).
        GameMap map = MapWithCondition("""{ "any": [{ "flag": "flag.test_met" }, { "flag": "flag.test_lost" }] }""");

        ContentException error = Assert.Throws<ContentException>(() => TestStory.Content.RequireScenesOf(map));

        Assert.Contains("rules/maps/probe.json", error.Message, StringComparison.Ordinal);
        Assert.Contains("triggers.trigger.test_story_meet.condition", error.Message, StringComparison.Ordinal);
        Assert.Contains("flag.test_lost", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AServiceConditionThatNamesADeclaredFlagPasses()
    {
        TestStory.Content.RequireServicesOf(ServiceMap("flag.test_met"));
    }

    [Fact]
    public void AServiceConditionThatNamesAnUndeclaredFlagFailsWithTheMapAndTheService()
    {
        // D-543, D-1131: a story flag closes a service through a flag of the flag file alone.
        ContentException error = Assert.Throws<ContentException>(() => TestStory.Content.RequireServicesOf(ServiceMap("flag.test_lost")));

        Assert.Contains("hub-test.json", error.Message, StringComparison.Ordinal);
        Assert.Contains("services.service.hub_rest.condition", error.Message, StringComparison.Ordinal);
        Assert.Contains("flag.test_lost", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARunRefusesAHubWhoseServiceNamesAnUndeclaredFlag()
    {
        Assert.Throws<ContentException>(() => Simulation.Start(
            1, ServiceMap("flag.test_lost"), TestBattles.Content, TestBattles.Notices, TestStory.Content, DebugIntentHandlers.None));
    }

    [Fact]
    public void AStorySceneOfATriggerThatNamesAMarkerTheMapLacksFails()
    {
        GameMap map = GameMap.Read(
            Encoding.UTF8.GetBytes(TestStory.MapFile.Replace(
                """{ "id": "marker.test_story_door", "kind": "marker", "x": 7, "y": 1 }""",
                """{ "id": "marker.test_story_gate", "kind": "marker", "x": 7, "y": 1 }""",
                StringComparison.Ordinal)),
            "rules/maps/probe.json");

        ContentException error = Assert.Throws<ContentException>(() => TestStory.Content.RequireScenesOf(map));

        Assert.Contains("marker.test_story_door", error.Message, StringComparison.Ordinal);
        Assert.Contains("step 0", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStepThatNamesAnAbsentStringFailsWithTheStorySceneTheStepAndTheId()
    {
        // Exit test 3 of PR-68 (G-7).
        StringTable strings = StringTable.Read(
            Encoding.UTF8.GetBytes("""{ "comment": "c", "strings": [ { "id": "line.test_greet", "text": "You came." }, { "id": "line.test_yes", "text": "Yes." }, { "id": "line.test_no", "text": "No." } ] }"""),
            StringTable.Path);

        ContentException error = Assert.Throws<ContentException>(() => TestStory.Content.RequireStrings(strings));

        Assert.Contains("rules/scenes/fight.json", error.Message, StringComparison.Ordinal);
        Assert.Contains("scene.test_fight.steps[0].line", error.Message, StringComparison.Ordinal);
        Assert.Contains("line.test_ambush", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnOptionThatNamesAnAbsentStringFails()
    {
        StringTable strings = StringTable.Read(
            Encoding.UTF8.GetBytes("""{ "comment": "c", "strings": [ { "id": "line.test_greet", "text": "You came." }, { "id": "line.test_yes", "text": "Yes." } ] }"""),
            StringTable.Path);

        ContentException error = Assert.Throws<ContentException>(() => Load(TestStory.Scene(TestStory.MeetFile, "meet")).RequireStrings(strings));

        Assert.Contains("scene.test_meet.steps[6].options[1].line", error.Message, StringComparison.Ordinal);
        Assert.Contains("line.test_no", error.Message, StringComparison.Ordinal);
    }

    private static StoryContent Load(StoryScene scene) => StoryContent.Load(TestStory.Flags, [scene], TestBattles.Content);

    private static StoryScene Scene(params string[] steps) =>
        StoryScene.Read(
            Encoding.UTF8.GetBytes($$"""{ "comment": "c", "id": "scene.test_probe", "steps": [{{string.Join(", ", steps)}}] }"""),
            "rules/scenes/probe.json");

    private static GameMap MapWithCondition(string condition) =>
        GameMap.Read(
            Encoding.UTF8.GetBytes(TestStory.MapFile.Replace(
                "\"condition\": { \"not\": { \"flag\": \"flag.test_met\" } }",
                "\"condition\": " + condition,
                StringComparison.Ordinal)),
            "rules/maps/probe.json");

    private static string[] Ids(StoryContent content)
    {
        List<string> ids = [];
        foreach (StoryScene scene in content.Scenes)
        {
            ids.Add(scene.Id.Value);
        }

        return [.. ids];
    }

    private static GameMap ServiceMap(string flag) => HubMaps.Of(
        npcs: HubMaps.Keeper,
        services: $$"""{ "id": "service.hub_rest", "kind": "rest", "npc": "npc.hub_keeper", "condition": { "flag": "{{flag}}" } }""");
}
