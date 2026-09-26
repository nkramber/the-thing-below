using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Story;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The readers of the story files: the flag file, a story scene, and the triggers of a map (G-6, D-997, D-1003, D-1004).</summary>
public sealed class StoryFileTests
{
    private const string ScenePath = "rules/scenes/test.json";

    [Fact]
    public void TheFlagFileReadsEachFlagInTheOrderOfTheFile()
    {
        Assert.Equal(
            ["flag.test_met", "flag.test_yes", "flag.test_no", "flag.test_done", "flag.test_victor", "flag.test_marrek_side", "flag.test_second_side", "flag.test_third_side", "flag.test_fourth_side"],
            Values(TestStory.Flags.Ids()));
        Assert.Equal("The ally met Marrek.", TestStory.Flags.Records[0].Note);
        Assert.True(TestStory.Flags.Declares(ContentId.Parse("flag.test_no", "test", "flag")));
    }

    [Fact]
    public void TheCheckoutFlagFileHoldsTheFlagOfTheSideAptitudeAlone()
    {
        // PR-12 adds the fixture flag of the side aptitude of Marrek (D-538, D-556).
        FlagList flags = FlagList.Read(System.IO.File.ReadAllBytes(RepositoryRoot.PathTo("content/rules/flags.json")), FlagList.Path);

        Assert.Equal(["flag.fixture_marrek_side"], Values(flags.Ids()));
    }

    [Theory]
    [InlineData("""{ "comment": "c", "flags": [{ "id": "flag.a", "note": "A." }, { "id": "flag.a", "note": "B." }] }""", "two times")]
    [InlineData("""{ "comment": "c", "flags": [{ "id": "flag.a", "note": "  " }] }""", "the note is empty")]
    [InlineData("""{ "comment": "c", "flags": [{ "id": "flag.a" }] }""", ".note)")]
    [InlineData("""{ "comment": "c", "flags": [{ "id": "notice.a", "note": "A." }] }""", "the kind 'flag'")]
    [InlineData("""{ "flags": [] }""", "field comment")]
    [InlineData("""{ "comment": "c", "flags": [], "other": 1 }""", "an unknown field")]
    public void AMalformedFlagFileFailsWithTheReason(string text, string reason)
    {
        ContentException error = Assert.Throws<ContentException>(() => FlagList.Read(Encoding.UTF8.GetBytes(text), FlagList.Path));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
        Assert.Contains(FlagList.Path, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EachStepKindReadsItsRecord()
    {
        StoryScene meet = TestStory.Scene(TestStory.MeetFile, "meet");
        StoryScene fight = TestStory.Scene(TestStory.FightFile, "fight");

        ShowStep show = Assert.IsType<ShowStep>(meet.Steps[0]);
        Assert.Equal("marker.test_story_door", show.Marker.Value);
        Assert.Equal(StepDirection.West, show.Facing);
        Assert.Equal("marker.test_story_door", Assert.IsType<CameraStep>(meet.Steps[1]).Marker.Value);
        MoveStep move = Assert.IsType<MoveStep>(meet.Steps[2]);
        Assert.Equal([StepDirection.West, StepDirection.West], move.Path);
        Assert.Equal(TestStory.Ally.Value, move.Actor.Id?.Value);
        FaceStep face = Assert.IsType<FaceStep>(meet.Steps[3]);
        Assert.True(face.Actor.IsLead);
        Assert.Equal(TestStory.Ally.Value, Assert.IsType<SayStep>(meet.Steps[4]).Speaker?.Id?.Value);
        Assert.Equal(5, Assert.IsType<WaitStep>(meet.Steps[5]).Ticks);
        Assert.Equal(2, Assert.IsType<ChooseStep>(meet.Steps[6]).Options.Count);
        Assert.Equal("flag.test_met", Assert.IsType<SetFlagStep>(meet.Steps[7]).Flag.Value);
        Assert.Equal(TestStory.Ally.Value, Assert.IsType<JoinStep>(meet.Steps[8]).Character.Value);
        Assert.Equal(TestStory.Ally.Value, Assert.IsType<HideStep>(meet.Steps[9]).Actor.Value);
        Assert.Null(Assert.IsType<SayStep>(fight.Steps[0]).Speaker);
        Assert.Equal("group.one", Assert.IsType<StartBattleStep>(fight.Steps[1]).Group.Value);
        Assert.True(Assert.IsType<SayStep>(fight.Steps[3]).Speaker?.IsLead);
    }

    [Theory]
    [InlineData(SceneStepKind.Move, SceneStepEnd.WaitIntent)]
    [InlineData(SceneStepKind.Face, SceneStepEnd.WaitIntent)]
    [InlineData(SceneStepKind.Say, SceneStepEnd.WaitIntent)]
    [InlineData(SceneStepKind.Camera, SceneStepEnd.WaitIntent)]
    [InlineData(SceneStepKind.Wait, SceneStepEnd.Ticks)]
    [InlineData(SceneStepKind.Choose, SceneStepEnd.Pick)]
    [InlineData(SceneStepKind.StartBattle, SceneStepEnd.Battle)]
    [InlineData(SceneStepKind.SetFlag, SceneStepEnd.AtOnce)]
    [InlineData(SceneStepKind.Join, SceneStepEnd.AtOnce)]
    [InlineData(SceneStepKind.Show, SceneStepEnd.AtOnce)]
    [InlineData(SceneStepKind.Hide, SceneStepEnd.AtOnce)]
    public void EachStepKindEndsAsTheDecisionsSay(SceneStepKind kind, SceneStepEnd end)
    {
        // D-1000, D-1007, D-1013: a step that Game animates waits, and one that it does not never waits.
        Assert.Equal(end, SceneStepKinds.EndOf(kind));
        Assert.True(SceneStepKinds.TryOf(SceneStepKinds.NameOf(kind), out SceneStepKind named));
        Assert.Equal(kind, named);
    }

    [Theory]
    [InlineData("""{ "kind": "dance" }""", "one of move, face")]
    [InlineData("""{ "id": "step.s1", "kind": "wait", "ticks": 0 }""", "a wait lasts one tick or more")]
    [InlineData("""{ "id": "step.s2", "kind": "wait", "ticks": 5, "line": "line.test_greet" }""", "reads no field 'line'")]
    [InlineData("""{ "id": "step.s3", "kind": "move", "actor": "lead", "path": [] }""", "the path holds no direction")]
    [InlineData("""{ "id": "step.s4", "kind": "move", "actor": "lead", "path": ["up"] }""", "the direction is 'up'")]
    [InlineData("""{ "id": "step.s5", "kind": "move", "actor": "enemy.test", "path": ["north"] }""", "or an id of the kind 'npc'")]
    [InlineData("""{ "id": "step.s6", "kind": "face", "actor": "lead" }""", ".facing)")]
    [InlineData("""{ "id": "step.s7", "kind": "say", "speaker": "notice.test_kept", "line": "line.test_greet" }""", "the speaker 'notice.test_kept'")]
    [InlineData("""{ "id": "step.s8", "kind": "say", "line": "line.test_greet" }""", ".speaker)")]
    [InlineData("""{ "id": "step.s9", "kind": "choose", "options": [{ "line": "line.test_yes", "flag": "flag.test_yes" }] }""", "holds 1 options")]
    [InlineData("""{ "id": "step.s10", "kind": "choose", "options": [{ "line": "line.test_yes", "flag": "flag.test_yes" }, { "line": "line.test_no", "flag": "flag.test_yes" }] }""", "two options set the flag")]
    [InlineData("""{ "id": "step.s11", "kind": "show", "actor": "lead", "at": "marker.test_story_door", "facing": "west" }""", "the lead stays on the map")]
    [InlineData("""{ "id": "step.s12", "kind": "hide", "actor": "lead" }""", "the lead stays on the map")]
    [InlineData("""{ "id": "step.s13", "kind": "camera", "at": "door.test_room_east" }""", "the kind 'marker'")]
    [InlineData("""{ "id": "step.s14", "kind": "start_battle", "group": "enemy.fixture_grunt" }""", "the kind 'group'")]
    [InlineData("""{ "id": "step.s15", "kind": "set_flag" }""", ".flag)")]
    [InlineData("""{ "kind": "wait", "ticks": 5 }""", ".id)")]
    [InlineData("""{ "id": "step.end", "kind": "wait", "ticks": 5 }""", "marks the end of a story scene")]
    [InlineData("""{ "id": "scene.s16", "kind": "wait", "ticks": 5 }""", "the kind 'step'")]
    public void AMalformedStepFailsWithTheReason(string step, string reason)
    {
        ContentException error = Assert.Throws<ContentException>(() => TestStory.Scene(SceneOf(step), "test"));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
        Assert.Contains(ScenePath, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoStepsWithOneIdFail()
    {
        // D-1112: a snapshot names a step by its id, so each id names one step alone.
        ContentException error = Assert.Throws<ContentException>(() => TestStory.Scene(
            """{ "comment": "c", "id": "scene.test", "steps": [{ "id": "step.one", "kind": "wait", "ticks": 5 }, { "id": "step.one", "kind": "wait", "ticks": 6 }] }""",
            "test"));

        Assert.Contains("two steps take the id 'step.one'", error.Message, StringComparison.Ordinal);
        Assert.Contains("steps[1]", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EachStepKeepsItsIdAtItsIndex()
    {
        StoryScene scene = TestStory.Scene(TestStory.FightFile, "fight");

        Assert.Equal(["step.ambush", "step.fight", "step.lead_steps", "step.after", "step.done"], Values(scene.StepIds));
        Assert.True(scene.TryIndexOfStep(ContentId.Parse("step.after", "test", "id"), out int found));
        Assert.Equal(3, found);
        Assert.False(scene.TryIndexOfStep(ContentId.Parse("step.gone", "test", "id"), out int none));
        Assert.Equal(-1, none);
    }

    [Fact]
    public void AStorySceneWithNoStepFails()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => TestStory.Scene("""{ "comment": "c", "id": "scene.test", "steps": [] }""", "test"));

        Assert.Contains("holds no step", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("rules/scenes/a.json", true)]
    [InlineData("rules/scenes/a.txt", false)]
    [InlineData("rules/maps/a.json", false)]
    public void TheStorySceneFolderHoldsEachStoryScene(string path, bool scene)
    {
        Assert.Equal(scene, StoryScene.IsSceneFile(path));
    }

    [Fact]
    public void AMapReadsEachTriggerKindWithItsFields()
    {
        GameMap map = TestStory.Map;

        Assert.Equal(3, map.Triggers.Count);
        Assert.Equal(TriggerKind.Entry, map.Triggers[0].Kind);
        Assert.Null(map.Triggers[0].At);
        Assert.Equal(TestStory.FightTile, map.Triggers[1].At);
        Assert.Equal("patrol.test_story_guard", map.Triggers[2].Patrol?.Value);
        Assert.Equal(ConditionKind.Always, map.Triggers[2].Condition.Kind);
    }

    [Fact]
    public void ATalkTriggerReadsItsNpc()
    {
        // D-1005: PR-68 reads the talk kind, and PR-14 fires it with the NPCs of the map.
        GameMap map = MapWith(
            """{ "id": "trigger.test_talk", "kind": "talk", "npc": "npc.test_elder", "scene": "scene.test_meet", "condition": { "always": true } }""",
            """{ "id": "npc.test_elder", "facing": "south", "step_ticks": 32, "move": "route", "tiles": [{ "x": 5, "y": 2, "wait_ticks": 0 }] }""");

        SceneTrigger talk = Assert.Single(map.Triggers);
        Assert.Equal(TriggerKind.Talk, talk.Kind);
        Assert.Equal("npc.test_elder", talk.Npc?.Value);
    }

    [Theory]
    [InlineData("""{ "id": "trigger.a", "kind": "tile", "x": 0, "y": 0, "scene": "scene.test_meet", "condition": { "always": true } }""", "which the party cannot stand on")]
    [InlineData("""{ "id": "trigger.a", "kind": "tile", "x": 2, "scene": "scene.test_meet", "condition": { "always": true } }""", ".y)")]
    [InlineData("""{ "id": "trigger.a", "kind": "entry", "x": 2, "y": 2, "scene": "scene.test_meet", "condition": { "always": true } }""", "reads no field 'x'")]
    [InlineData("""{ "id": "trigger.a", "kind": "battle_end", "patrol": "patrol.test_absent", "scene": "scene.test_meet", "condition": { "always": true } }""", "places no such patrol")]
    [InlineData("""{ "id": "trigger.a", "kind": "battle_end", "scene": "scene.test_meet", "condition": { "always": true } }""", ".patrol)")]
    [InlineData("""{ "id": "trigger.a", "kind": "talk", "scene": "scene.test_meet", "condition": { "always": true } }""", ".npc)")]
    [InlineData("""{ "id": "trigger.a", "kind": "talk", "npc": "npc.test_absent", "scene": "scene.test_meet", "condition": { "always": true } }""", "names the NPC 'npc.test_absent', and this map places no such NPC")]
    [InlineData("""{ "id": "trigger.a", "kind": "entry", "scene": "scene.test_meet" }""", ".condition)")]
    [InlineData("""{ "id": "trigger.a", "kind": "leave", "scene": "scene.test_meet", "condition": { "always": true } }""", "one of tile, talk, entry, battle_end")]
    [InlineData("""{ "id": "trigger.a", "kind": "entry", "scene": "scene.test_meet", "condition": { "always": true } }, { "id": "trigger.a", "kind": "entry", "scene": "scene.test_meet", "condition": { "always": true } }""", "two triggers of this map take the id")]
    public void AMalformedTriggerFailsWithTheMapAndTheReason(string triggers, string reason)
    {
        ContentException error = Assert.Throws<ContentException>(() => MapWith(triggers));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
        Assert.Contains("test-triggers.json", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnNpcIsAnActorOfAMoveAFaceAShowAndALine()
    {
        // D-1006: a step names an NPC by an id of the kind `npc`.
        StoryScene talk = TestStory.Scene(StoryTalkTests.TalkFile, "talk");

        Assert.Equal("npc.hub_keeper", Assert.IsType<SayStep>(talk.Steps[0]).Speaker?.Id?.Value);
        Assert.True(Assert.IsType<MoveStep>(talk.Steps[1]).Actor.IsNpc);
        Assert.True(Assert.IsType<FaceStep>(talk.Steps[2]).Actor.IsNpc);
        Assert.Equal("npc.hub_stranger", Assert.IsType<ShowStep>(talk.Steps[3]).Actor.Value);
        Assert.False(new SceneActor(TestStory.Ally).IsNpc);
    }

    [Fact]
    public void AStorySceneOfASceneOnlyNpcLoadsAgainstAMapThatDoesNotPlaceIt()
    {
        // D-1006: a show puts the stranger on the marker, a move walks it, and a hide takes it off.
        StoryContent story = NpcStory(
            """{ "id": "step.s1", "kind": "show", "actor": "npc.hub_stranger", "at": "marker.hub_corner", "facing": "east" }""",
            """{ "id": "step.s2", "kind": "move", "actor": "npc.hub_stranger", "path": ["north"] }""",
            """{ "id": "step.s3", "kind": "say", "speaker": "npc.hub_stranger", "line": "line.test_greet" }""",
            """{ "id": "step.s4", "kind": "hide", "actor": "npc.hub_stranger" }""",
            """{ "id": "step.s5", "kind": "face", "actor": "npc.hub_keeper", "facing": "west" }""");

        story.RequireScenesOf(NpcMap());
    }

    [Theory]
    [InlineData("""{ "id": "step.s1", "kind": "show", "actor": "npc.hub_keeper", "at": "marker.hub_corner", "facing": "east" }""", "shows the NPC 'npc.hub_keeper', which this map places")]
    [InlineData("""{ "id": "step.s1", "kind": "move", "actor": "npc.hub_stranger", "path": ["east"] }""", "names the NPC 'npc.hub_stranger', which this map does not place and no earlier show put on the map")]
    [InlineData("""{ "id": "step.s1", "kind": "face", "actor": "npc.hub_stranger", "facing": "east" }""", "names the NPC 'npc.hub_stranger', which this map does not place")]
    public void AnNpcActorThatBreaksTheRulesOfItsMapFailsWithTheMapAndTheStep(string step, string reason)
    {
        // D-1006: an NPC of the map acts where it stands, and a show names a scene-only NPC.
        StoryContent story = NpcStory(step);

        ContentException error = Assert.Throws<ContentException>(() => story.RequireScenesOf(NpcMap()));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
        Assert.Contains("hub-test.json", error.Message, StringComparison.Ordinal);
        Assert.Contains("step 0", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ALineOfAnNpcWithNoBodyOnTheMapLoads()
    {
        // D-1146: a voice through a door speaks with no body on the map, so the load checks the
        // kind of the id alone.
        StoryContent story = NpcStory("""{ "id": "step.s1", "kind": "say", "speaker": "npc.hub_voice", "line": "line.test_greet" }""");

        story.RequireScenesOf(NpcMap());
        Assert.Equal("npc.hub_voice", Assert.IsType<SayStep>(Assert.Single(story.Scenes).Steps[0]).Speaker?.Id?.Value);
    }

    [Fact]
    public void ALineWithASpeakerOfAnotherKindFails()
    {
        // D-997 and D-1146: a speaker is `none`, `lead`, a cast member, or an NPC, and no other kind.
        ContentException error = Assert.Throws<ContentException>(() => NpcStory("""{ "id": "step.s1", "kind": "say", "speaker": "patrol.hub_voice", "line": "line.test_greet" }"""));

        Assert.Contains("the speaker 'patrol.hub_voice' is not", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("""{ "id": "step.s1", "kind": "hide", "actor": "npc.hub_keeper" }""", "the actor 'npc.hub_keeper' is not on the map at this step, and a hide takes an actor that a show step put there")]
    [InlineData("""{ "id": "step.s1", "kind": "show", "actor": "npc.hub_stranger", "at": "marker.hub_corner", "facing": "east" }, { "id": "step.s2", "kind": "show", "actor": "npc.hub_stranger", "at": "marker.hub_corner", "facing": "east" }""", "the actor 'npc.hub_stranger' is already on the map")]
    public void AHideOrASecondShowOfAnNpcFailsTheLoad(string steps, string reason)
    {
        // D-1006: a hide takes a shown actor alone, and an NPC of the map stays on it.
        ContentException error = Assert.Throws<ContentException>(() => NpcStory(steps));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AMapWithNoTriggersFieldFails()
    {
        // D-1002 and G-6: an absent field is an error, and an empty list names a map with no trigger.
        ContentException error = Assert.Throws<ContentException>(() => GameMap.Read(
            Encoding.UTF8.GetBytes(TestStory.MapFile.Replace("\"triggers\": [", "\"other\": [", StringComparison.Ordinal)),
            "test-triggers.json"));

        Assert.Contains("an unknown field", error.Message, StringComparison.Ordinal);
    }

    private static List<string> Values(IReadOnlyList<ContentId> ids)
    {
        List<string> values = [];
        foreach (ContentId id in ids)
        {
            values.Add(id.Value);
        }

        return values;
    }

    private static string SceneOf(string step) =>
        $$"""{ "comment": "c", "id": "scene.test", "steps": [{{step}}] }""";

    /// <summary>Gives the story content of one story scene with the steps of a test, and the flags and the cast of the tests.</summary>
    private static StoryContent NpcStory(params string[] steps) =>
        StoryContent.Load(
            FlagList.Read(Encoding.UTF8.GetBytes(TestBattles.NoFlagsFile), FlagList.Path),
            [TestStory.Scene($$"""{ "comment": "c", "id": "scene.test_npc", "steps": [{{string.Join(", ", steps)}}] }""", "npc")],
            TestBattles.Content);

    /// <summary>Gives the hub of the tests with the keeper, the corner marker, and a talk trigger that starts the story scene of <see cref="NpcStory"/>.</summary>
    private static GameMap NpcMap()
    {
        string trigger = """{ "id": "trigger.test_npc", "kind": "talk", "npc": "npc.hub_keeper", "scene": "scene.test_npc", "condition": { "always": true } }""";
        string text = HubMaps.Text(npcs: HubMaps.Keeper, things: HubMaps.Marker);
        return TestMaps.Of("hub-test.json", text.Replace("\"triggers\": []", $"\"triggers\": [{trigger}]", StringComparison.Ordinal));
    }

    private static GameMap MapWith(string triggers, string npcs = "")
    {
        string map = TestStory.MapFile.Replace("\"npcs\": []", $"\"npcs\": [{npcs}]", StringComparison.Ordinal);
        int start = map.IndexOf("\"triggers\": [", StringComparison.Ordinal);
        string text = map[..start] + $$"""
            "triggers": [{{triggers}}]
            }
            """;
        return GameMap.Read(Encoding.UTF8.GetBytes(text), "test-triggers.json");
    }
}
