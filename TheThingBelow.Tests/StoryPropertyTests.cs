using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>Property tests of the story scene runner over a seed loop (T-3, G-5).</summary>
public sealed class StoryPropertyTests
{
    private const int SeedCount = 1000;

    private const int TickCount = 400;

    /// <summary>The tick of the save in the middle of each run, inside the meeting or the walk.</summary>
    private const int SaveTick = 20;

    [Fact]
    public void EveryRunWithStorySceneReplaysToTheSameEndStateHash()
    {
        // Exit test 1 of PR-68: the record holds each wait intent and each pick, so a replay
        // plays every story scene and reproduces every flag (D-540, T-7).
        ContentId met = ContentId.Parse("flag.test_met", "test", "flag");
        for (ulong seed = 0; seed < SeedCount; seed += 1)
        {
            Simulation run = TestStory.Start(seed);
            RunRecorder recorder = new(RunHeader.ForThisBuild(SaveRuns.ContentHash, seed), run.Snapshot());
            for (int tick = 0; tick < TickCount; tick += 1)
            {
                IReadOnlyList<Intent> intents = TestStory.BotIntents(run.State);
                run.Step(intents);
                recorder.Step(run.Tick, intents);
                if (run.Tick == SaveTick)
                {
                    recorder.Save(run.Snapshot());
                }
            }

            RunRecord record = RunRecordText.Read(RunRecordText.Write(recorder.Build()));
            RunState replayed = RunReplay.Play(
                record, SaveRuns.ContentHash, TestStory.Map, TestBattles.Content, TestBattles.Notices, TestStory.Content, DebugIntentHandlers.None);

            Assert.True(run.State.Story.Flags.IsOn(met), $"Seed {seed}: the run never played the meeting.");
            Assert.True(run.StateHash() == replayed.StateHash(), $"Seed {seed}: the replay gave another end-state hash.");
        }
    }
}
