using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Story;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The story scene runner: each step, each trigger, and each intent of a story scene (D-540, D-997 to D-1013).</summary>
public sealed class StoryRulesTests
{
    private const ulong Seed = 0x5105;

    private const int TickLimit = 2000;

    [Fact]
    public void TheEntryTriggerStartsTheMeetingOnTheFirstWorldTick()
    {
        Simulation run = TestStory.Start(Seed);

        run.Step([]);

        StoryState story = run.State.Story;
        Assert.Equal(TestStory.Meet.Value, story.Scene?.Id.Value);

        // The show runs at once, and the camera waits for Game (D-1013).
        Assert.Equal(1, story.Step);
        Assert.Equal(ScenePhase.WaitIntent, story.Phase);
        ActorValues ally = Assert.Single(story.Actors);
        Assert.Equal(TestStory.DoorTile, ally.At);
        Assert.Equal(StepDirection.West, ally.Facing);
    }

    [Fact]
    public void ABotThatAnswersEachWaitAtOncePlaysTheMeetingToItsEnd()
    {
        // Exit test 2 of PR-68, and exit test 8: the join adds the ally to the party (D-563).
        Simulation run = TestStory.Start(Seed);
        run.Step([]);

        PlayWhile(run, () => run.State.Story.Running);

        FlagSet flags = run.State.Story.Flags;
        Assert.True(flags.IsOn(Flag("flag.test_met")));
        Assert.True(flags.IsOn(Flag("flag.test_yes")));
        Assert.False(flags.IsOn(Flag("flag.test_no")));
        Assert.Empty(run.State.Story.Actors);
        Assert.Equal(StepDirection.East, run.State.Party.Facing);
        Assert.Equal(
            ["character.marrek", TestStory.Ally.Value],
            MemberIds(run.State.Characters));

        // The ally joins at its join level with full health and full MP (D-363).
        PartyMember ally = run.State.Characters.Members[1];
        Assert.Equal(1, ally.Level);
        Assert.Equal(ally.Stats.Health, ally.Health);
        Assert.Equal(ally.Stats.Mp, ally.Mp);
    }

    [Fact]
    public void AMoveStepPutsTheActorOnTheEndOfItsPathBeforeGameDrawsIt()
    {
        Simulation run = TestStory.Start(Seed);
        run.Step([]);
        run.Step([Intent.OfPlayer(IntentIds.StoryStepEnd)]);

        StoryState story = run.State.Story;
        Assert.Equal(2, story.Step);
        Assert.Equal(ScenePhase.WaitIntent, story.Phase);
        Assert.Equal(new TilePoint(5, 1), Assert.Single(story.Actors).At);
    }

    [Fact]
    public void AWaitStepEndsAfterItsCountOfWorldTicks()
    {
        Simulation run = TestStory.Start(Seed);
        run.Step([]);
        PlayWhile(run, () => run.State.Story.Phase != ScenePhase.Ticks);
        long start = run.Tick;

        PlayWhile(run, () => run.State.Story.Phase == ScenePhase.Ticks);

        // The wait of the meeting holds 5 ticks (D-1000).
        Assert.Equal(5, run.Tick - start);
        Assert.Equal(ScenePhase.Pick, run.State.Story.Phase);
    }

    [Fact]
    public void ThePauseHoldsTheWaitAndRefusesEveryIntentButItsEnd()
    {
        // D-1009, D-1010: the pause holds the world step, so the wait stops its count.
        Simulation run = TestStory.Start(Seed);
        run.Step([]);
        PlayWhile(run, () => run.State.Story.Phase != ScenePhase.Ticks);
        run.Step([Intent.OfPlayer(IntentIds.StoryPause)]);
        int left = run.State.Story.TicksLeft;
        long world = run.State.WorldTick;

        run.Step([]);
        run.Step([]);

        Assert.True(run.State.Story.Paused);
        Assert.Equal(left, run.State.Story.TicksLeft);
        Assert.Equal(world, run.State.WorldTick);
        SimulationException refused = Assert.Throws<SimulationException>(() => run.Step([Intent.OfPlayer(IntentIds.StoryStepEnd)]));
        Assert.Contains("D-1010", refused.Message, StringComparison.Ordinal);

        run.Step([Intent.OfPlayer(IntentIds.StoryResume)]);

        Assert.False(run.State.Story.Paused);
        Assert.Equal(left - 1, run.State.Story.TicksLeft);
    }

    [Fact]
    public void APauseInTheBattleOfAStorySceneFails()
    {
        // D-1010: the pause holds a story scene, and a battle of the story scene is a battle.
        Simulation run = TestStory.Start(Seed);
        run.Step([]);
        PlayWhile(run, () => run.State.Battle is null);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfPlayer(IntentIds.StoryPause)]));

        Assert.Contains("'battle'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheEndOfAPauseWithNoPauseFails()
    {
        Simulation run = TestStory.Start(Seed);
        run.Step([]);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfPlayer(IntentIds.StoryResume)]));

        Assert.Contains("no pause holds", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("intent.move_north")]
    [InlineData("intent.open_menu")]
    [InlineData("intent.battle_defend")]
    [InlineData("intent.confirm")]
    public void AnIntentOfThePlayerOutsideTheStorySceneFailsWhileOneRuns(string action)
    {
        Simulation run = TestStory.Start(Seed);
        run.Step([]);

        SimulationException error = Assert.Throws<SimulationException>(
            () => run.Step([Intent.OfPlayer(ContentId.Parse(action, "test", "action"))]));

        Assert.Contains(action, error.Message, StringComparison.Ordinal);
        Assert.Contains("D-1009", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStepEndWithNoStepThatWaitsFails()
    {
        Simulation run = TestStory.Start(Seed);
        run.Step([]);
        PlayWhile(run, () => run.State.Story.Running);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfPlayer(IntentIds.StoryStepEnd)]));

        Assert.Contains("No story scene runs", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void APickOfAnAbsentOptionFailsWithTheRangeOfTheChoice()
    {
        Simulation run = TestStory.Start(Seed);
        run.Step([]);
        PlayWhile(run, () => run.State.Story.Phase != ScenePhase.Pick);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfPick(2)]));

        Assert.Contains("the options 0 to 1", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void APickWhileNoChoiceWaitsFails()
    {
        Simulation run = TestStory.Start(Seed);
        run.Step([]);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfPick(0)]));

        Assert.Contains("wait_intent", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePickOfTheSecondOptionTurnsOnItsOwnFlag()
    {
        Simulation run = TestStory.Start(Seed);
        run.Step([]);
        PlayWhile(run, () => run.State.Story.Phase != ScenePhase.Pick);

        run.Step([Intent.OfPick(1)]);

        Assert.True(run.State.Story.Flags.IsOn(Flag("flag.test_no")));
        Assert.False(run.State.Story.Flags.IsOn(Flag("flag.test_yes")));
    }

    [Fact]
    public void TheTileTriggerStartsTheFightWhenTheLeadArrives()
    {
        Simulation run = TestStory.Start(Seed);
        run.Step([]);
        PlayWhile(run, () => run.State.Story.Scene?.Id.Value != TestStory.Fight.Value);

        Assert.Equal(TestStory.FightTile, run.State.Party.LeadAt);

        // The step that the same tick started ends, and the lead stands on the tile (D-1009).
        Assert.Null(run.State.Party.Stepping);
    }

    [Fact]
    public void TheBattleOfAStorySceneRefusesAFleeAndTheStorySceneGoesOnAfterTheWin()
    {
        // D-998, D-999, D-1008.
        Simulation run = TestStory.Start(Seed);
        run.Step([]);
        PlayWhile(run, () => run.State.Battle is null);
        Battle battle = BattleRuns.BattleOf(run);
        Assert.True(battle.FromStoryScene);
        Assert.Equal(TestStory.Fight.Value, battle.Enemy.Value);
        Assert.Equal(ScenePhase.Battle, run.State.Story.Phase);
        PlayWhile(run, () => BattleRuns.BattleOf(run).Next()?.Side != BattleSide.Party);

        SimulationException flee = Assert.Throws<SimulationException>(() => run.Step([Intent.OfPlayer(IntentIds.BattleFlee)]));
        Assert.Contains("D-1008", flee.Message, StringComparison.Ordinal);

        TilePoint before = run.State.Party.LeadAt;
        PlayWhile(run, () => run.State.Battle is not null);

        // The move of the lead after the start battle step runs at once, and Game ends it.
        Assert.Equal(2, run.State.Story.Step);
        Assert.Equal(before.Step(StepDirection.East), run.State.Party.LeadAt);
        PlayWhile(run, () => run.State.Story.Running);
        Assert.True(run.State.Story.Flags.IsOn(Flag("flag.test_done")));
    }

    [Fact]
    public void AStorySceneThatPlayedOnceNeverPlaysAgain()
    {
        // Exit test 5 of PR-68: the fight sets its flag, and its condition then refuses it (D-542).
        Simulation run = TestStory.Start(Seed);
        run.Step([]);
        PlayFightToEnd(run);
        Assert.Equal(new TilePoint(4, 3), run.State.Party.LeadAt);

        WalkTo(run, StepDirection.West, TestStory.FightTile);

        Assert.False(run.State.Story.Running);
        Assert.Equal(TestStory.FightTile, run.State.Party.LeadAt);
    }

    [Fact]
    public void AWinAgainstThePatrolOfABattleEndTriggerPlaysItsStoryScene()
    {
        // D-1011: the trigger fires on the world step after the wait intent of the win.
        Simulation run = TestStory.Start(Seed);
        run.Step([]);
        PlayFightToEnd(run);
        WalkTo(run, StepDirection.South, new TilePoint(4, 4));
        for (int tick = 0; tick < TickLimit && run.State.Battle is null; tick += 1)
        {
            run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        }

        Assert.False(BattleRuns.BattleOf(run).FromStoryScene);
        PlayWhile(run, () => run.State.Battle is not null);

        // The victory holds one set flag step, so it starts and ends on that tick.
        Assert.True(run.State.Story.Flags.IsOn(Flag("flag.test_victor")));
        Assert.False(run.State.Story.Running);
        Assert.Null(run.State.Story.WonPatrol);
    }

    [Fact]
    public void TwoRunsOfOneSeedStartEachStorySceneOnTheSameTick()
    {
        // Exit test 6 of PR-68: a trigger fires from the tick of Core (T-7).
        Assert.Equal(StartTicks(Seed), StartTicks(Seed));
    }

    [Fact]
    public void AJoinOfACharacterInThePartyFailsWithTheCharacter()
    {
        BattleContent content = TestBattles.ExactWithParty(2);
        Simulation run = Simulation.Start(Seed, TestStory.Map, content, TestBattles.Notices, TestStory.ContentOf(content), DebugIntentHandlers.None);
        run.Step([]);

        SimulationException error = Assert.Throws<SimulationException>(() => PlayWhile(run, () => run.State.Story.Running));

        Assert.Contains($"a join of '{TestStory.Ally.Value}', who is already in the party", error.Message, StringComparison.Ordinal);
        Assert.Contains("scene.test_meet/steps[8]", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AJoinIntoAFullPartyFails()
    {
        BattleContent content = TestBattles.Of(TestBattles.FixtureFile.Replace(
            "\"start_party\": [\"character.marrek\"]",
            "\"start_party\": [\"character.marrek\", \"character.test_third\"]",
            StringComparison.Ordinal));
        StoryScene join = TestStory.Scene(
            """
            {
             "comment": "Two joins, the second into a full party.",
             "id": "scene.test_meet",
             "steps": [{ "kind": "join", "character": "character.test_second" }, { "kind": "join", "character": "character.test_second" }]
            }
            """,
            "full");
        Simulation run = Simulation.Start(Seed, TestStory.Map, content, TestBattles.Notices, StoryOf(content, join), DebugIntentHandlers.None);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([]));

        Assert.Contains("already in the party", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AMoveIntoAWallFailsWithTheStorySceneTheStepAndTheTile()
    {
        StoryScene walk = TestStory.Scene(
            """
            {
             "comment": "The lead walks into the north wall.",
             "id": "scene.test_meet",
             "steps": [{ "kind": "move", "actor": "lead", "path": ["north"] }]
            }
            """,
            "wall");
        Simulation run = Simulation.Start(Seed, TestStory.Map, TestBattles.Content, TestBattles.Notices, StoryOf(TestBattles.Content, walk), DebugIntentHandlers.None);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([]));

        Assert.Contains("scene.test_meet/steps[0]", error.Message, StringComparison.Ordinal);
        Assert.Contains("(1, 0)", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-1012", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AShowOnTheTileOfTheLeadFails()
    {
        StoryScene show = TestStory.Scene(
            """
            {
             "comment": "The ally appears on the door, where the lead stands.",
             "id": "scene.test_meet",
             "steps": [
              { "kind": "move", "actor": "lead", "path": ["east", "east", "east", "east", "east", "east"] },
              { "kind": "show", "actor": "character.test_second", "at": "marker.test_story_door", "facing": "west" }
             ]
            }
            """,
            "crowd");
        Simulation run = Simulation.Start(Seed, TestStory.Map, TestBattles.Content, TestBattles.Notices, StoryOf(TestBattles.Content, show), DebugIntentHandlers.None);
        run.Step([]);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfPlayer(IntentIds.StoryStepEnd)]));

        Assert.Contains("which holds the lead", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheStateHashReadsTheStoryState()
    {
        Simulation one = TestStory.Start(Seed);
        Simulation other = TestStory.Start(Seed);
        one.Step([]);
        other.Step([]);
        Assert.Equal(one.StateHash(), other.StateHash());

        one.Step([Intent.OfPlayer(IntentIds.StoryPause)]);
        other.Step([]);

        Assert.NotEqual(one.StateHash(), other.StateHash());
    }

    [Fact]
    public void ASnapshotWithAnOpenMenuAndARunningStorySceneFailsTheResume()
    {
        // P3-18 (D-1009): Core refuses the close of the menu while a story scene runs, so the
        // pair would hold the run for good. The same snapshot with the menu shut resumes.
        Simulation run = TestStory.Start(Seed);
        run.Step([]);
        Assert.True(run.State.Story.Running);
        RunSnapshot snapshot = run.Snapshot();

        ArgumentException error = Assert.Throws<ArgumentException>(() => TestStory.Resume(Seed, snapshot with { MenuOpen = true }));

        Assert.Contains("an open menu and a running story scene", error.Message, StringComparison.Ordinal);
        Assert.Equal(run.StateHash(), TestStory.Resume(Seed, snapshot).StateHash());
    }

    [Fact]
    public void AnArrivalOnATriggerTilePlaysItsSceneBeforeTheStepIntoTheGuardBesideIt()
    {
        // D-1103, P3-19: the lead holds east onto the trigger tile, and the guard stands on the
        // next tile. One tick arrives and steps into the guard. The scene plays first, and the
        // held direction starts the fight after it.
        Simulation run = BesideTheGuard("""{ "not": { "flag": "flag.test_victor" } }""");
        WalkOnto(run, Trigger);

        // The scene of one step plays to its end on the tick that fires it.
        Assert.Null(run.State.Battle);
        Assert.Null(run.State.Party.Patrols.Encounter);
        Assert.True(run.State.Story.Flags.IsOn(Flag("flag.test_victor")));

        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);

        Assert.NotNull(run.State.Battle);
    }

    [Fact]
    public void AnArrivalOnATriggerTileWhoseConditionFailsStepsIntoTheGuardOnTheSameTick()
    {
        // The boundary of the rule above: no scene fires, so the step into the guard starts
        // the fight on the tick of the arrival (D-747).
        Simulation run = BesideTheGuard("""{ "flag": "flag.test_victor" }""");
        WalkOnto(run, Trigger);

        Assert.False(run.State.Story.Running);
        Assert.NotNull(run.State.Battle);
    }

    /// <summary>The tile trigger of <see cref="BesideTheGuard"/>, two tiles east of the spawn point.</summary>
    private static readonly TilePoint Trigger = new(3, 1);

    /// <summary>
    /// The story map with the guard on the tile east of the tile trigger, no entry scene, and
    /// the one-step victory scene on the tile trigger under the condition (D-1103).
    /// </summary>
    private static Simulation BesideTheGuard(string condition)
    {
        string text = TestStory.MapFile
            .Replace("\"tiles\": [{ \"x\": 8, \"y\": 4 }]", "\"tiles\": [{ \"x\": 4, \"y\": 1 }]", StringComparison.Ordinal)
            .Replace("\"condition\": { \"not\": { \"flag\": \"flag.test_met\" } }", "\"condition\": { \"flag\": \"flag.test_met\" }", StringComparison.Ordinal)
            .Replace("\"y\": 3,", "\"y\": 1,", StringComparison.Ordinal)
            .Replace("\"scene\": \"scene.test_fight\"", "\"scene\": \"scene.test_victory\"", StringComparison.Ordinal)
            .Replace("\"condition\": { \"all\": [{ \"flag\": \"flag.test_met\" }, { \"not\": { \"flag\": \"flag.test_done\" } }] }", $"\"condition\": {condition}", StringComparison.Ordinal);
        GameMap map = GameMap.Read(System.Text.Encoding.UTF8.GetBytes(text), "rules/maps/test-story.json");
        return Simulation.Start(Seed, map, TestBattles.Content, TestBattles.Notices, TestStory.Content, DebugIntentHandlers.None);
    }

    /// <summary>Holds east, and stops on the tick that the lead stands on the tile or a battle starts.</summary>
    private static void WalkOnto(Simulation run, TilePoint tile)
    {
        for (int tick = 0; tick < TickLimit; tick += 1)
        {
            MapState party = run.State.Party;
            if (run.State.Battle is not null || (party.LeadAt == tile && party.Stepping is null))
            {
                return;
            }

            run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        }

        throw new InvalidOperationException($"The lead did not reach {tile} in {TickLimit} ticks.");
    }

    /// <summary>Plays the bot of the fixture while the condition holds, and fails on a run that never ends it.</summary>
    internal static void PlayWhile(Simulation run, Func<bool> condition)
    {
        for (int tick = 0; tick < TickLimit; tick += 1)
        {
            if (!condition())
            {
                return;
            }

            run.Step(TestStory.BotIntents(run.State));
        }

        throw new InvalidOperationException($"The run passed {TickLimit} ticks at tick {run.Tick}, and the condition still held.");
    }

    private static void PlayFightToEnd(Simulation run)
    {
        PlayWhile(run, () => !run.State.Story.Flags.IsOn(Flag("flag.test_done")));
        PlayWhile(run, () => run.State.Story.Running);
    }

    private static void WalkTo(Simulation run, StepDirection direction, TilePoint target)
    {
        Intent step = Intent.OfPlayer(direction switch
        {
            StepDirection.North => IntentIds.MoveNorth,
            StepDirection.South => IntentIds.MoveSouth,
            StepDirection.East => IntentIds.MoveEast,
            _ => IntentIds.MoveWest,
        });
        for (int tick = 0; tick < TickLimit; tick += 1)
        {
            if (run.State.Party.LeadAt == target && run.State.Party.Stepping is null)
            {
                return;
            }

            bool standing = run.State.Party.Stepping is null;
            run.Step(standing && run.State.Party.LeadAt != target ? [step] : []);
        }

        throw new InvalidOperationException($"The lead never reached {target}.");
    }

    private static List<long> StartTicks(ulong seed)
    {
        Simulation run = TestStory.Start(seed);
        List<long> starts = [];
        bool running = false;
        for (int tick = 0; tick < 600; tick += 1)
        {
            run.Step(TestStory.BotIntents(run.State));
            if (run.State.Story.Running && !running)
            {
                starts.Add(run.Tick);
            }

            running = run.State.Story.Running;
        }

        return starts;
    }

    private static StoryContent StoryOf(BattleContent content, StoryScene meet) =>
        StoryContent.Load(TestStory.Flags, [meet, TestStory.Scene(TestStory.FightFile, "fight"), TestStory.Scene(TestStory.VictoryFile, "victory")], content);

    private static List<string> MemberIds(PartyState party)
    {
        List<string> ids = [];
        foreach (PartyMember member in party.Members)
        {
            ids.Add(member.Record.Id.Value);
        }

        return ids;
    }

    private static ContentId Flag(string value) => ContentId.Parse(value, "test", "flag");
}
