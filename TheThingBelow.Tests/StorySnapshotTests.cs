using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Saves;
using TheThingBelow.Core.Story;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The story state in a snapshot, a save, and a record (D-166, D-540, G-5).</summary>
public sealed class StorySnapshotTests
{
    private const ulong Seed = 0x5106;

    [Fact]
    public void AStorySceneWithAShownActorRunsOnFromItsSnapshotText()
    {
        Simulation run = TestStory.Start(Seed);
        run.Step([]);
        run.Step([Intent.OfPlayer(IntentIds.StoryStepEnd)]);
        Assert.Single(run.State.Story.Actors);

        Simulation copy = TestStory.Resume(Seed, ReadText(RunSnapshotText.Write(run.Snapshot())));

        Assert.Equal(run.StateHash(), copy.StateHash());
        Assert.Equal(RunSnapshotText.Write(run.Snapshot()), RunSnapshotText.Write(copy.Snapshot()));
        StoryRulesTests.PlayWhile(run, () => run.State.Story.Running);
        StoryRulesTests.PlayWhile(copy, () => copy.State.Story.Running);
        Assert.Equal(run.StateHash(), copy.StateHash());
    }

    [Fact]
    public void AStorySceneThatWaitsForItsBattleRunsOnFromItsSnapshotText()
    {
        Simulation run = TestStory.Start(Seed);
        run.Step([]);
        StoryRulesTests.PlayWhile(run, () => run.State.Battle is null);

        Simulation copy = TestStory.Resume(Seed, ReadText(RunSnapshotText.Write(run.Snapshot())));

        Assert.True(BattleRuns.BattleOf(copy).FromStoryScene);
        Assert.Equal(run.StateHash(), copy.StateHash());
    }

    [Fact]
    public void AWinThatWaitsForItsTriggerFiresAfterTheLoad()
    {
        // D-1011: the win against a patrol waits in the snapshot for the next world step.
        RunSnapshot met = MetSnapshot();
        RunSnapshot waiting = met with { Story = met.Story! with { WonPatrol = ContentId.Parse("patrol.test_story_guard", "test", "patrol") } };

        Simulation run = TestStory.Resume(Seed, ReadText(RunSnapshotText.Write(waiting)));
        run.Step([]);

        Assert.True(run.State.Story.Flags.IsOn(ContentId.Parse("flag.test_victor", "test", "flag")));
    }

    [Theory]
    [MemberData(nameof(BrokenStories))]
    public void AStoryStateThatNoRunMakesFailsTheLoad(string name, Func<StoryValues, StoryValues> breakIt, string reason)
    {
        RunSnapshot met = MetSnapshot();
        RunSnapshot broken = met with { Story = breakIt(met.Story!) };

        ArgumentException error = Assert.Throws<ArgumentException>(() => TestStory.Resume(Seed, broken));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
        Assert.False(string.IsNullOrEmpty(name));
    }

    /// <summary>Each story state that no run makes, with the words of its error.</summary>
    /// <returns>The cases: a name, the change, and the reason.</returns>
    public static TheoryData<string, Func<StoryValues, StoryValues>, string> BrokenStories() =>
        new()
        {
            { "a pause with no story scene", story => story with { Paused = true }, "the pause holds a story scene alone" },
            { "an absent patrol", story => story with { WonPatrol = Id("patrol.test_absent") }, "places no such patrol" },
            { "an undeclared flag", story => story with { Flags = [Id("flag.test_lost")] }, "does not declare" },
            { "flags out of order", story => story with { Flags = [Id("flag.test_yes"), Id("flag.test_met")] }, "leave the ordinal order" },
            { "an absent story scene", story => story with { Scene = Scene("scene.test_absent", 0, ScenePhase.Ready, 0) }, "which no story scene of this build holds" },
            { "an entry and a story scene", story => story with { EntryPending = true, Scene = Scene("scene.test_fight", 0, ScenePhase.WaitIntent, 0) }, "an entry to read and a story scene" },
            { "a step past the end", story => story with { Scene = Scene("scene.test_fight", 6, ScenePhase.Ready, 0) }, "the step is 6" },
            { "a phase of another step", story => story with { Scene = Scene("scene.test_fight", 0, ScenePhase.Pick, 0) }, "is a 'say' step, and its phase is 'pick'" },
            { "ticks on a step that waits for Game", story => story with { Scene = Scene("scene.test_fight", 0, ScenePhase.WaitIntent, 3) }, "a wait step alone counts ticks" },
            { "a phase past the end", story => story with { Scene = Scene("scene.test_fight", 5, ScenePhase.WaitIntent, 0) }, "past the last step" },
            { "ticks above the wait", story => story with { Scene = Scene("scene.test_meet", 5, ScenePhase.Ticks, 6) }, "the range is 1 to 5" },
            { "an actor on a wall", story => story with { Scene = Scene("scene.test_meet", 1, ScenePhase.WaitIntent, 0, new ActorValues(Id("character.test_second"), new TilePoint(0, 0), StepDirection.West)) }, "which no actor can stand on" },
            { "an actor that no character of this build holds", story => story with { Scene = Scene("scene.test_meet", 1, ScenePhase.WaitIntent, 0, new ActorValues(Id("character.unknown"), new TilePoint(7, 1), StepDirection.West)) }, "it shows 'character.unknown', which no character of this build holds" },
            { "an actor twice", story => story with { Scene = Scene("scene.test_meet", 1, ScenePhase.WaitIntent, 0, Ally(7), Ally(6)) }, "two times" },
            { "two actors on one tile", story => story with { Scene = Scene("scene.test_meet", 1, ScenePhase.WaitIntent, 0, Ally(7), new ActorValues(Id("character.test_third"), new TilePoint(7, 1), StepDirection.West)) }, "two actors stand at" },
            { "a battle with no battle", story => story with { Scene = Scene("scene.test_fight", 1, ScenePhase.Battle, 0) }, "it holds no battle" },
        };

    [Fact]
    public void ABattleOfAStorySceneWithNoStepThatWaitsForItFailsTheLoad()
    {
        Simulation run = TestStory.Start(Seed);
        run.Step([]);
        StoryRulesTests.PlayWhile(run, () => run.State.Battle is null);
        RunSnapshot inBattle = run.Snapshot();
        RunSnapshot broken = inBattle with { Story = inBattle.Story! with { Scene = inBattle.Story.Scene! with { Step = 0, Phase = ScenePhase.WaitIntent } } };

        ArgumentException error = Assert.Throws<ArgumentException>(() => TestStory.Resume(Seed, broken));

        Assert.Contains("no encounter and no story scene that waits for it", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ABattleOfAStorySceneWithAnotherGroupThanItsStepFailsTheLoad()
    {
        Simulation run = TestStory.Start(Seed);
        run.Step([]);
        StoryRulesTests.PlayWhile(run, () => run.State.Battle is null);
        RunSnapshot inBattle = run.Snapshot();
        RunSnapshot broken = inBattle with { Battle = inBattle.Battle! with { Group = Id("group.test_pair") } };

        ArgumentException error = Assert.Throws<ArgumentException>(() => TestStory.Resume(Seed, broken));

        Assert.Contains("no start battle step of that story scene waits for it", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASnapshotLineOfFormatNineWithNoStoryFails()
    {
        string line = RunSnapshotText.Write(MetSnapshot());
        int start = line.IndexOf(",\"story\":", StringComparison.Ordinal);
        int end = line.IndexOf(",\"streams\":", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => ReadText(line[..start] + line[end..]));

        Assert.Contains("field story", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASaveOfFormatEightThatHoldsAStoryFails()
    {
        string path = RepositoryRoot.PathTo("TheThingBelow.Tests/saves/format-8.json");
        string[] lines = File.ReadAllText(path).Split('\n');
        lines[1] = lines[1].Replace(",\"streams\":", ",\"story\":{\"flags\":[],\"paused\":false,\"entry\":false},\"streams\":", StringComparison.Ordinal);
        string text = string.Join('\n', lines[0].Replace(ChecksumOf(lines[0]), SaveText.ChecksumOf(lines[1]), StringComparison.Ordinal), lines[1], string.Empty);

        SaveException error = Assert.Throws<SaveException>(() => SaveText.Read(text, path));

        Assert.Contains("that format predates it (D-540)", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ABattleOfFormatEightThatNamesAStorySceneFails()
    {
        string path = RepositoryRoot.PathTo("TheThingBelow.Tests/saves/format-8.json");
        string[] lines = File.ReadAllText(path).Split('\n');
        lines[1] = lines[1].Replace("\"battle\":{\"enemy\":\"patrol.test_guard\"", "\"battle\":{\"enemy\":\"scene.test_fight\"", StringComparison.Ordinal);
        string text = string.Join('\n', lines[0].Replace(ChecksumOf(lines[0]), SaveText.ChecksumOf(lines[1]), StringComparison.Ordinal), lines[1], string.Empty);

        SaveException error = Assert.Throws<SaveException>(() => SaveText.Read(text, path));

        Assert.Contains("names a patrol (D-749, D-998)", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void APickKeepsItsOptionThroughTheRecordText()
    {
        Simulation run = TestStory.Start(Seed);
        RunRecorder recorder = new(RunHeader.ForThisBuild(SaveRuns.ContentHash, Seed), run.Snapshot());
        for (int tick = 0; tick < 100 && run.State.Story.Phase != ScenePhase.Pick; tick += 1)
        {
            IReadOnlyList<Intent> intents = TestStory.BotIntents(run.State);
            run.Step(intents);
            recorder.Step(run.Tick, intents);
        }

        IReadOnlyList<Intent> pick = [Intent.OfPick(1)];
        run.Step(pick);
        recorder.Step(run.Tick, pick);

        string text = RunRecordText.Write(recorder.Build());
        RunRecord read = RunRecordText.Read(text);

        Assert.Contains("\"option\":1", text, StringComparison.Ordinal);
        Intent readPick = read.Ticks[^1].Intents[0];
        Assert.Equal(1, readPick.Option);
        Assert.Equal("intent.story_pick option 1", readPick.Describe());
    }

    [Fact]
    public void AnOptionOnAnIntentThatIsNotAPickFails()
    {
        Simulation run = TestStory.Start(Seed);

        SimulationException error = Assert.Throws<SimulationException>(
            () => run.Step([new Intent(IntentIds.MoveEast, false, null, null, 0)]));

        Assert.Contains("an option, a lesson, or an actor that no rule of its action reads", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void APickWithNoOptionFails()
    {
        Simulation run = TestStory.Start(Seed);
        run.Step([]);
        StoryRulesTests.PlayWhile(run, () => run.State.Story.Phase != ScenePhase.Pick);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfPlayer(IntentIds.StoryPick)]));

        Assert.Contains("a pick that names no option", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A snapshot after the meeting, with its flags on and no story scene.</summary>
    private static RunSnapshot MetSnapshot()
    {
        Simulation run = TestStory.Start(Seed);
        run.Step([]);
        StoryRulesTests.PlayWhile(run, () => run.State.Story.Running);
        return run.Snapshot();
    }

    private static RunSnapshot ReadText(string line)
    {
        var reader = new ContentReader(Encoding.UTF8.GetBytes(line), "the test");
        return RunSnapshotText.Read(ref reader);
    }

    private static string ChecksumOf(string header)
    {
        const string Key = "\"checksum\":\"";
        int start = header.IndexOf(Key, StringComparison.Ordinal) + Key.Length;
        return header[start..header.IndexOf('"', start)];
    }

    private static SceneValues Scene(string id, int step, ScenePhase phase, int ticks, params ActorValues[] actors) =>
        new(Id(id), step, phase, ticks, actors);

    private static ActorValues Ally(int x) => new(Id("character.test_second"), new TilePoint(x, 1), StepDirection.West);

    private static ContentId Id(string value) => ContentId.Parse(value, "test", "id");
}
