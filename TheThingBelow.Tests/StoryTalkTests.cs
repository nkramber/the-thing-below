using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Story;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The talk trigger and the NPC as a story scene actor (D-1005, D-1006, D-1139): exit test 9 of
/// PR-14. A talk with the keeper fires its trigger, and the story scene moves the keeper, turns
/// it, and shows a scene-only NPC on a marker, who leaves at the end.
/// </summary>
/// <remarks>
/// The hub of these tests is the room of <see cref="HubMaps"/> with the keeper at (2, 6), the dog
/// that wanders at (6, 5), and the marker at (1, 6). The lead talks with the keeper from (2, 5).
/// </remarks>
public sealed class StoryTalkTests
{
    private const ulong Seed = 20260925;

    /// <summary>The story scene of the talk: a line of the keeper, a step, a turn, a show, and a wait.</summary>
    public const string TalkFile = """
    {
     "comment": "The keeper speaks, steps east, turns back, and a stranger shows in the corner.",
     "id": "scene.test_talk",
     "steps": [
      { "id": "step.greet", "kind": "say", "speaker": "npc.hub_keeper", "line": "line.test_greet" },
      { "id": "step.keeper_steps", "kind": "move", "actor": "npc.hub_keeper", "path": ["east"] },
      { "id": "step.keeper_turns", "kind": "face", "actor": "npc.hub_keeper", "facing": "west" },
      { "id": "step.stranger", "kind": "show", "actor": "npc.hub_stranger", "at": "marker.hub_corner", "facing": "east" },
      { "id": "step.stranger_turns", "kind": "face", "actor": "npc.hub_stranger", "facing": "north" },
      { "id": "step.pause", "kind": "wait", "ticks": 5 }
     ]
    }
    """;

    private static readonly ContentId Keeper = ContentId.Parse("npc.hub_keeper", "test", "npc");

    private static readonly ContentId Stranger = ContentId.Parse("npc.hub_stranger", "test", "npc");

    private static readonly Intent StepEnd = Intent.OfPlayer(IntentIds.StoryStepEnd);

    [Fact]
    public void ATalkWithAnNpcFiresItsTriggerAndAStepOfTheStorySceneMovesTheNpc()
    {
        // Exit test 9 of PR-14 (D-1005, D-1006): the keeper turns to the lead, its trigger starts
        // the story scene, and the move step walks the keeper one tile east.
        Simulation run = Start(Talk("""{ "always": true }"""));
        HubRestTests.WalkToKeeper(run);
        NpcState keeper = KeeperOf(run);

        HubWalks.Confirm(run);

        Assert.Equal("scene.test_talk", run.State.Story.Scene?.Id.Value);
        Assert.Equal(StepDirection.North, keeper.Facing);
        Assert.False(run.State.MenuOpen);

        run.Step([StepEnd]);
        Assert.Equal((new TilePoint(3, 6), StepDirection.East), (keeper.At, keeper.Facing));

        run.Step([StepEnd]);
        Assert.Equal(StepDirection.West, keeper.Facing);
    }

    [Fact]
    public void ASceneOnlyNpcShowsOnAMarkerAndLeavesAtTheEndOfTheStoryScene()
    {
        // D-1006: the map places no stranger, so a show puts it on the marker, a face turns it, and
        // it leaves with the end of the story scene.
        Simulation run = Start(Talk("""{ "always": true }"""));
        HubRestTests.WalkToKeeper(run);
        HubWalks.Confirm(run);
        run.Step([StepEnd]);
        run.Step([StepEnd]);
        run.Step([StepEnd]);

        ActorValues shown = Assert.Single(run.State.Story.Actors);
        Assert.Equal((Stranger.Value, new TilePoint(1, 6), StepDirection.North), (shown.Actor.Value, shown.At, shown.Facing));

        RunToEnd(run);

        Assert.Empty(run.State.Story.Actors);
        Assert.False(run.State.Party.Npcs.TryFind(Stranger, out _));
    }

    [Fact]
    public void EachNpcHoldsStillDuringTheStorySceneExceptForItsSteps()
    {
        // D-1139: the dog wanders before the talk, and it holds still on its tile while the story
        // scene waits for Game, whatever the count of ticks.
        Simulation run = Start(Talk("""{ "always": true }"""));
        HubRestTests.WalkToKeeper(run);
        HubWalks.Confirm(run);
        NpcState dog = run.State.Party.Npcs.All[1];
        NpcValues held = dog.Values();
        Assert.Null(held.Stepping);

        for (int tick = 0; tick < 400; tick += 1)
        {
            run.Step([]);
            Assert.Equal(held, dog.Values());
        }
    }

    [Fact]
    public void TheTalkEntryFiresTheTriggerOfTheNamedNpc()
    {
        // Exit test 9 of PR-14: the public entry of the talk takes the id of the NPC (D-1005).
        Simulation run = Start(Talk("""{ "always": true }"""));
        List<LogEntry> log = [];

        Assert.False(StoryRules.FireTalk(run.State, ContentId.Parse("npc.hub_dog", "test", "npc"), log));
        Assert.True(StoryRules.FireTalk(run.State, Keeper, log));

        Assert.Equal("scene.test_talk", run.State.Story.Scene?.Id.Value);
        Assert.Contains("a story scene started", HubWalks.Messages(log));
    }

    [Fact]
    public void ATalkWhoseConditionFailsOpensTheServiceOfTheNpc()
    {
        // D-1131: with no talk trigger that holds, the service on the NPC opens.
        Simulation run = Start(Talk("""{ "flag": "flag.test_marrek_side" }"""));
        HubRestTests.WalkToKeeper(run);

        HubWalks.Confirm(run);

        Assert.False(run.State.Story.Running);
        Assert.True(run.State.MenuOpen);
        Assert.Equal("service.hub_rest", Assert.Single(run.TakeOpenedServices()).Id.Value);
    }

    [Fact]
    public void ASnapshotInTheStorySceneKeepsTheMovedNpcAndTheShownNpc()
    {
        // D-166 and G-5: the snapshot holds the keeper outside its home and the stranger on the
        // marker, so the resumed run and the live run keep one state hash.
        GameMap map = Talk("""{ "always": true }""");
        Simulation live = Start(map);
        HubRestTests.WalkToKeeper(live);
        HubWalks.Confirm(live);
        live.Step([StepEnd]);
        live.Step([StepEnd]);
        live.Step([StepEnd]);
        string line = RunSnapshotText.Write(live.Snapshot());
        var reader = new ContentReader(Encoding.UTF8.GetBytes(line), "the test");
        Simulation resumed = Simulation.Resume(Seed, RunSnapshotText.Read(ref reader), map, TestBattles.Content, TestBattles.Notices, Story, DebugIntentHandlers.None);

        Assert.Contains("\"npc\":\"npc.hub_stranger\"", line, StringComparison.Ordinal);
        Assert.Contains("\"walks_home\":true", line, StringComparison.Ordinal);
        for (int tick = 0; tick < 200; tick += 1)
        {
            IReadOnlyList<Intent> intents = live.State.Story.Phase == ScenePhase.WaitIntent ? [StepEnd] : [];
            live.Step(intents);
            resumed.Step(intents);
            Assert.True(live.StateHash() == resumed.StateHash(), $"The resumed run left the live run at tick {live.Tick}.");
        }
    }

    [Fact]
    public void ASnapshotThatShowsAnNpcOfTheMapFails()
    {
        // D-1006: a show puts a scene-only NPC on the map, so a stored show of the keeper describes
        // no state of a run.
        GameMap map = Talk("""{ "always": true }""");
        Simulation live = Start(map);
        HubRestTests.WalkToKeeper(live);
        HubWalks.Confirm(live);
        RunSnapshot snapshot = live.Snapshot();
        StoryValues story = snapshot.Story ?? throw new InvalidOperationException("The snapshot holds no story state.");
        SceneValues scene = story.Scene ?? throw new InvalidOperationException("The snapshot holds no story scene.");
        RunSnapshot broken = snapshot with { Story = story with { Scene = scene with { Actors = [new ActorValues(Keeper, new TilePoint(1, 6), StepDirection.East)] } } };

        ArgumentException error = Assert.Throws<ArgumentException>(
            () => Simulation.Resume(Seed, broken, map, TestBattles.Content, TestBattles.Notices, Story, DebugIntentHandlers.None));

        Assert.Contains("it shows the NPC 'npc.hub_keeper', which the map 'map.hub_test' places", error.Message, StringComparison.Ordinal);
    }

    /// <summary>The story content of these tests: the flags of the tests and the talk (D-1003).</summary>
    internal static StoryContent Story { get; } = StoryContent.Load(
        FlagList.Read(Encoding.UTF8.GetBytes(TestBattles.NoFlagsFile), FlagList.Path),
        [TestStory.Scene(TalkFile, "talk")],
        TestBattles.Content);

    /// <summary>Gives the hub of these tests with a talk trigger on the keeper under one condition.</summary>
    private static GameMap Talk(string condition)
    {
        string text = HubMaps.Text(
            npcs: $"{HubMaps.Keeper}, {HubMaps.Wanderer()}",
            services: HubMaps.RestOnKeeper,
            things: HubMaps.Marker);
        string trigger = $$"""{ "id": "trigger.test_talk", "kind": "talk", "npc": "npc.hub_keeper", "scene": "scene.test_talk", "condition": {{condition}} }""";
        return TestMaps.Of("hub-test.json", text.Replace("\"triggers\": []", $"\"triggers\": [{trigger}]", StringComparison.Ordinal));
    }

    private static Simulation Start(GameMap map) =>
        Simulation.Start(Seed, map, TestBattles.Content, TestBattles.Notices, Story, DebugIntentHandlers.None);

    private static NpcState KeeperOf(Simulation run) =>
        run.State.Party.Npcs.TryFind(Keeper, out NpcState? keeper) ? keeper! : throw new InvalidOperationException("The map places no keeper.");

    /// <summary>Ends each step that waits for Game until the story scene ends.</summary>
    private static void RunToEnd(Simulation run)
    {
        for (int tick = 0; run.State.Story.Running; tick += 1)
        {
            Assert.True(tick < 100, "The story scene did not end in 100 ticks.");
            run.Step(run.State.Story.Phase == ScenePhase.WaitIntent ? [StepEnd] : []);
        }
    }
}
