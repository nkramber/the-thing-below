using System;
using System.Collections.Generic;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The recorder and the compaction of a snapshot plus the intents after it (F-10, D-651).
/// </summary>
public sealed class RunRecorderTests
{
    private const ulong Seed = 20260918;
    private const string ContentHash = "a-content-hash";

    private static readonly IReadOnlyList<Intent> NoIntents = [];

    [Fact]
    public void ATickWithNoIntentTakesNoLine()
    {
        // The tick of each line gives the time, so a tick with no intent needs none (D-650).
        (Simulation run, RunRecorder recorder) = Start();

        for (int step = 0; step < 100; step += 1)
        {
            run.Step(NoIntents);
            recorder.Step(run.Tick, NoIntents);
        }

        Assert.Equal(0, recorder.LineCount);
        Assert.Equal(100, recorder.Build().EndTick);
    }

    [Fact]
    public void ATickWithAnIntentTakesOneLine()
    {
        (Simulation run, RunRecorder recorder) = Start();
        Intent[] open = [Intent.OfPlayer(IntentIds.OpenMenu)];

        run.Step(open);
        recorder.Step(run.Tick, open);

        RunRecord record = recorder.Build();
        TickIntents entry = Assert.Single(record.Ticks);
        Assert.Equal(1, entry.Tick);
        Assert.Equal(IntentIds.OpenMenu, entry.Intents[0].Action);
    }

    [Fact]
    public void ASaveDropsEveryIntentBeforeIt()
    {
        // D-651. The record keeps the snapshot of the save and the intents after it.
        IReadOnlyList<IReadOnlyList<Intent>> script = RunScripts.Make(Seed, 300);
        (Simulation run, RunRecorder recorder) = RunScripts.Play(Seed, ContentHash, script, 0);

        recorder.Save(run.Snapshot());

        RunRecord record = recorder.Build();
        Assert.Empty(record.Ticks);
        Assert.Equal(300, record.Snapshot.Tick);
        Assert.Equal(300, record.EndTick);
    }

    [Fact]
    public void TheRecordSizeStaysBoundedOverALongRun()
    {
        // Exit test 5 of section 7.13 of `phase-1-foundations.md`. The run below takes 60000
        // ticks, which is about 17 minutes of play, and it saves once a minute (F-10, D-651).
        const int tickCount = 60_000;
        const int saveEvery = 3_600;

        IReadOnlyList<IReadOnlyList<Intent>> script = RunScripts.Make(Seed, tickCount);
        (_, RunRecorder recorder) = RunScripts.Play(Seed, ContentHash, script, saveEvery);

        RunRecord record = recorder.Build();
        string text = RunRecordText.Write(record);

        Assert.Equal(tickCount, record.EndTick);
        Assert.True(
            record.Ticks.Count <= saveEvery,
            $"The record holds {record.Ticks.Count} lines after a save every {saveEvery} ticks.");
        Assert.True(text.Length < 65_536, $"The record text holds {text.Length} characters.");
    }

    [Fact]
    public void ARecordWithNoSaveGrowsWithTheIntentsOfTheRun()
    {
        // The bound of F-10 comes from the save, and the test above proves it. This test
        // proves that the bound is not an accident of a short script.
        IReadOnlyList<IReadOnlyList<Intent>> script = RunScripts.Make(Seed, 60_000);
        (_, RunRecorder recorder) = RunScripts.Play(Seed, ContentHash, script, 0);

        Assert.True(recorder.LineCount > 1_000, $"The record holds {recorder.LineCount} lines.");
    }

    [Fact]
    public void AStepThatDoesNotFollowTheLastTickIsAnError()
    {
        (_, RunRecorder recorder) = Start();

        recorder.Step(1, NoIntents);

        Assert.Throws<ArgumentException>(() => recorder.Step(1, NoIntents));
    }

    [Fact]
    public void ASaveOutsideTheRecordedRunIsAnError()
    {
        (Simulation run, RunRecorder recorder) = Start();
        run.Step(NoIntents);
        recorder.Step(run.Tick, NoIntents);

        RunSnapshot later = run.Snapshot() with { Tick = 500 };

        ArgumentException error = Assert.Throws<ArgumentException>(() => recorder.Save(later));
        Assert.Contains("500", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARecordWithATickThatDoesNotRiseIsAnError()
    {
        RunSnapshot start = Simulation.Start(Seed, DebugIntentHandlers.None).Snapshot();
        RunHeader header = RunHeader.ForThisBuild(ContentHash, Seed);
        Intent[] intents = [Intent.OfPlayer(IntentIds.OpenMenu)];

        Assert.Throws<ArgumentException>(() => new RunRecord(
            header,
            start,
            [new TickIntents(5, intents), new TickIntents(5, intents)],
            10));
    }

    [Fact]
    public void ARecordWithATickWithNoIntentIsAnError()
    {
        RunSnapshot start = Simulation.Start(Seed, DebugIntentHandlers.None).Snapshot();
        RunHeader header = RunHeader.ForThisBuild(ContentHash, Seed);

        Assert.Throws<ArgumentException>(() => new RunRecord(
            header, start, [new TickIntents(5, [])], 10));
    }

    [Fact]
    public void ARecordThatEndsBeforeItsLastLineIsAnError()
    {
        RunSnapshot start = Simulation.Start(Seed, DebugIntentHandlers.None).Snapshot();
        RunHeader header = RunHeader.ForThisBuild(ContentHash, Seed);

        Assert.Throws<ArgumentException>(() => new RunRecord(
            header, start, [new TickIntents(5, [Intent.OfPlayer(IntentIds.OpenMenu)])], 4));
    }

    private static (Simulation Run, RunRecorder Recorder) Start()
    {
        Simulation run = Simulation.Start(Seed, DebugIntentHandlers.None);
        return (run, new RunRecorder(RunHeader.ForThisBuild(ContentHash, Seed), run.Snapshot()));
    }
}
