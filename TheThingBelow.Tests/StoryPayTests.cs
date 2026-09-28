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
/// The pay step of PR-17 (D-1335): exit tests 2 to 4. A talk with the keeper gives an offer. A
/// yes with enough gold removes the price and turns on the flag. A yes with too little gold, or a
/// no, shows the refusal line and ends the story scene with no change, so a new talk gives the
/// offer again until the flag turns on.
/// </summary>
/// <remarks>
/// The hub of these tests is the room of <see cref="HubMaps"/> with the keeper at (2, 6). The talk
/// trigger holds while the flag of the offer is off, so the rest of the keeper opens after a pay.
/// </remarks>
public sealed class StoryPayTests
{
    private const ulong Seed = 20260928;

    private const int Price = 30;

    /// <summary>The story scene of the talk: the offer, then a line that plays after a pay alone.</summary>
    private const string PayFile = """
    {
     "comment": "The keeper opens the way for a price.",
     "id": "scene.test_pay",
     "steps": [
      { "id": "step.offer", "kind": "pay", "speaker": "npc.hub_keeper", "line": "line.test_greet", "price": 30, "flag": "flag.test_yes", "refusal": "line.test_no" },
      { "id": "step.thanks", "kind": "say", "speaker": "npc.hub_keeper", "line": "line.test_yes" }
     ]
    }
    """;

    private static readonly ContentId Flag = ContentId.Parse("flag.test_yes", "test", "flag");

    private static readonly Intent StepEnd = Intent.OfPlayer(IntentIds.StoryStepEnd);

    private static readonly StoryContent Story = StoryContent.Load(TestStory.Flags, [TestStory.Scene(PayFile, "pay")], TestBattles.Content);

    private static readonly GameMap Map = TalkMap();

    [Fact]
    public void AYesWithEnoughGoldRemovesThePriceTurnsOnTheFlagAndTheStorySceneGoesOn()
    {
        // Exit test 2 of PR-17 (D-1335).
        Simulation run = OfferWithGold(50);

        run.Step([Intent.OfPick(PayStep.PayOption)]);

        Assert.Equal(50 - Price, run.State.Characters.Gold);
        Assert.True(run.State.Story.Flags.IsOn(Flag));
        Assert.Equal((1, ScenePhase.WaitIntent), (run.State.Story.Step, run.State.Story.Phase));
    }

    [Theory]
    [InlineData(PayStep.PayOption, Price - 1)]
    [InlineData(PayStep.DeclineOption, 50)]
    public void ARefusalShowsItsLineThenEndsTheStorySceneWithNoChange(int option, int gold)
    {
        // Exit test 2 of PR-17 (D-1335): a yes with too little gold, or a no.
        Simulation run = OfferWithGold(gold);

        run.Step([Intent.OfPick(option)]);
        Assert.Equal((0, ScenePhase.WaitIntent), (run.State.Story.Step, run.State.Story.Phase));

        run.Step([StepEnd]);

        Assert.False(run.State.Story.Running);
        Assert.Equal(gold, run.State.Characters.Gold);
        Assert.False(run.State.Story.Flags.IsOn(Flag));
    }

    [Fact]
    public void ANewTalkAfterARefusalGivesTheOfferAgain()
    {
        // Exit test 3 of PR-17 (D-1335): the trigger holds while the flag is off.
        Simulation run = OfferWithGold(50);
        run.Step([Intent.OfPick(PayStep.DeclineOption)]);
        run.Step([StepEnd]);
        Assert.False(run.State.Story.Running);

        HubWalks.Confirm(run);

        Assert.Equal("scene.test_pay", run.State.Story.Scene?.Id.Value);
        Assert.Equal((0, ScenePhase.Pick), (run.State.Story.Step, run.State.Story.Phase));
    }

    [Fact]
    public void AfterAPayTheTalkNoLongerGivesTheOffer()
    {
        // D-1131, D-1335: with the flag on, no talk trigger holds, and the rest of the keeper opens.
        Simulation run = OfferWithGold(50);
        run.Step([Intent.OfPick(PayStep.PayOption)]);
        run.Step([StepEnd]);
        Assert.False(run.State.Story.Running);

        HubWalks.Confirm(run);

        Assert.False(run.State.Story.Running);
        Assert.Equal("service.hub_rest", Assert.Single(run.TakeOpenedServices()).Id.Value);
    }

    [Fact]
    public void BothAnswersHoldWithNoGold()
    {
        // D-1335: a yes with too little gold gives the refusal line, so a bot never waits on an offer.
        Simulation run = OfferWithGold(0);

        IReadOnlyList<Intent> accepted = AcceptedIntents.Of(run.State);

        Assert.Contains(Intent.OfPick(PayStep.PayOption), accepted);
        Assert.Contains(Intent.OfPick(PayStep.DeclineOption), accepted);
        Assert.DoesNotContain(Intent.OfPick(2), accepted);
    }

    [Fact]
    public void APickOutsideTheTwoAnswersFailsWithTheOptions()
    {
        // T-2: an offer holds the options 0 and 1 alone.
        Simulation run = OfferWithGold(50);
        List<LogEntry> log = [];

        SimulationException error = Assert.Throws<SimulationException>(
            () => StoryRules.Pick(run.State, 2, run.State.Context("test"), log));

        Assert.Contains("a pick of option 2, and an offer holds the options 0 (yes) and 1 (no)", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(PayStep.DeclineOption)]
    public void ASnapshotAtTheOfferOrAtTheRefusalResumesToTheSameStateHash(int option)
    {
        // Exit test 4 of PR-17 (G-5, T-7): -1 takes the snapshot before the answer.
        Simulation live = OfferWithGold(50);
        if (option >= 0)
        {
            live.Step([Intent.OfPick(option)]);
        }

        string line = RunSnapshotText.Write(live.Snapshot());
        var reader = new ContentReader(Encoding.UTF8.GetBytes(line), "the test");
        Simulation resumed = Simulation.Resume(Seed, RunSnapshotText.Read(ref reader), Map, TestBattles.Content, TestBattles.Notices, Story, DebugIntentHandlers.None);

        Assert.Equal(live.StateHash(), resumed.StateHash());
        for (int tick = 0; tick < 20; tick += 1)
        {
            IReadOnlyList<Intent> intents = live.State.Story.Phase switch
            {
                ScenePhase.WaitIntent => [StepEnd],
                ScenePhase.Pick => [Intent.OfPick(PayStep.PayOption)],
                _ => [],
            };
            live.Step(intents);
            resumed.Step(intents);
            Assert.True(live.StateHash() == resumed.StateHash(), $"The resumed run left the live run at tick {live.Tick}.");
        }
    }

    [Fact]
    public void ASnapshotOfAPayStepInAPhaseItNeverTakesFails()
    {
        // T-2: a pay step waits for a pick or for the end of its refusal line, and never counts ticks.
        Simulation live = OfferWithGold(50);
        RunSnapshot snapshot = live.Snapshot();
        StoryValues story = snapshot.Story ?? throw new InvalidOperationException("The snapshot holds no story state.");
        SceneValues scene = story.Scene ?? throw new InvalidOperationException("The snapshot holds no story scene.");
        RunSnapshot broken = snapshot with { Story = story with { Scene = scene with { Phase = ScenePhase.Battle } } };

        ArgumentException error = Assert.Throws<ArgumentException>(
            () => Simulation.Resume(Seed, broken, Map, TestBattles.Content, TestBattles.Notices, Story, DebugIntentHandlers.None));

        Assert.Contains("is a 'pay' step, and its phase is 'battle'", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Starts a run with an amount of gold, walks to the keeper, and talks, so the offer waits for a pick.</summary>
    private static Simulation OfferWithGold(int gold)
    {
        Simulation run = Simulation.Start(Seed, Map, TestBattles.Content, TestBattles.Notices, Story, DebugIntentHandlers.None);
        if (gold > 0)
        {
            run.State.Characters.AddGold(gold, run.State.Context("test"));
        }

        HubRestTests.WalkToKeeper(run);
        HubWalks.Confirm(run);
        Assert.Equal((0, ScenePhase.Pick), (run.State.Story.Step, run.State.Story.Phase));
        return run;
    }

    /// <summary>Gives the hub of these tests with a talk trigger on the keeper while the flag of the offer is off.</summary>
    private static GameMap TalkMap()
    {
        string text = HubMaps.Text(npcs: HubMaps.Keeper, services: HubMaps.RestOnKeeper, things: HubMaps.Marker);
        const string Trigger = """{ "id": "trigger.test_pay", "kind": "talk", "npc": "npc.hub_keeper", "scene": "scene.test_pay", "condition": { "not": { "flag": "flag.test_yes" } } }""";
        return TestMaps.Of("hub-test.json", text.Replace("\"triggers\": []", $"\"triggers\": [{Trigger}]", StringComparison.Ordinal));
    }
}
