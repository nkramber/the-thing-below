using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The replay of a run record (G-5, T-7). A replay on the same simulation version and
/// content hash gives the state hash of the run that wrote the record.
/// </summary>
public sealed class RunReplayTests
{
    private const string ContentHash = "a-content-hash";

    [Fact]
    public void AThousandSeedsEachReplayToTheirOwnEndState()
    {
        // Exit test 1 of section 7.13 of `phase-1-foundations.md`. The loop names its seed on
        // a failure (T-3). Each record goes through its text, so the encoding of D-652 takes
        // part in the property.
        for (ulong seed = 1; seed <= 1_000; seed += 1)
        {
            IReadOnlyList<IReadOnlyList<Intent>> script = RunScripts.Make(seed, 200);
            (Simulation run, RunRecorder recorder) = RunScripts.Play(seed, ContentHash, script, 0);

            RunRecord record = RunRecordText.Read(RunRecordText.Write(recorder.Build()));
            RunState replayed = RunReplay.Play(record, ContentHash, TestMaps.Room, DebugIntentHandlers.None);

            Assert.True(
                run.StateHash() == replayed.StateHash(),
                $"The replay of seed {seed} gives another end state than the run.");
        }
    }

    [Fact]
    public void AReplayFromASaveGivesTheEndStateOfTheRun()
    {
        // The compaction of D-651 drops the intents before the snapshot, so a replay from a
        // save must still reach the state of the last tick (F-10).
        const ulong seed = 20260918;
        IReadOnlyList<IReadOnlyList<Intent>> script = RunScripts.Make(seed, 900);
        (Simulation run, RunRecorder recorder) = RunScripts.Play(seed, ContentHash, script, 300);

        RunRecord record = recorder.Build();
        RunState replayed = RunReplay.Play(record, ContentHash, TestMaps.Room, DebugIntentHandlers.None);

        Assert.True(record.Snapshot.Tick >= 900 - 300, "The record kept no recent snapshot.");
        Assert.Equal(run.StateHash(), replayed.StateHash());
        Assert.Equal(run.Tick, replayed.Tick);
    }

    [Fact]
    public void ARecordOfAnotherSimulationVersionFailsWithBothValues()
    {
        // Exit test 2 of section 7.13 of `phase-1-foundations.md`.
        RunRecord record = OneTickRecord(header => header with { SimulationVersion = 1 });

        RunRecordException error = Assert.Throws<RunRecordException>(
            () => RunReplay.Play(record, ContentHash, TestMaps.Room, DebugIntentHandlers.None));

        Assert.Contains("simulation version 1", error.Message, StringComparison.Ordinal);
        Assert.Contains(
            $"simulation version {SimulationVersion.Current}", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARecordOfAnotherContentHashFailsWithBothValues()
    {
        // Exit test 3 of section 7.13 of `phase-1-foundations.md`.
        RunRecord record = OneTickRecord(header => header with { ContentHash = "the-old-content" });

        RunRecordException error = Assert.Throws<RunRecordException>(
            () => RunReplay.Play(record, ContentHash, TestMaps.Room, DebugIntentHandlers.None));

        Assert.Contains("the-old-content", error.Message, StringComparison.Ordinal);
        Assert.Contains(ContentHash, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARecordOfAnotherFormatVersionFailsWithBothValues()
    {
        RunRecord record = OneTickRecord(header => header with { FormatVersion = 99 });

        RunRecordException error = Assert.Throws<RunRecordException>(
            () => RunReplay.Play(record, ContentHash, TestMaps.Room, DebugIntentHandlers.None));

        Assert.Contains("format version 99", error.Message, StringComparison.Ordinal);
        Assert.Contains(
            $"format version {RunRecordFormat.Current}", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARecordWithADebugIntentFailsOnAHostWithNoHandlerAndNamesTheTick()
    {
        // Exit test 4 of section 7.13 of `phase-1-foundations.md`. A release host passes no
        // handler, and it must refuse such a record rather than drop the intent (D-492, T-2).
        RunRecord record = DebugIntentRecord();

        SimulationException error = Assert.Throws<SimulationException>(
            () => RunReplay.Play(record, ContentHash, TestMaps.Room, DebugIntentHandlers.None));

        Assert.Contains("debug.step_east", error.Message, StringComparison.Ordinal);
        Assert.Contains("tick 3", error.Message, StringComparison.Ordinal);
        Assert.Equal(3, error.Context.Tick);
    }

    [Fact]
    public void ARecordWithADebugIntentReplaysOnAHostWithTheHandler()
    {
        // A run that used a debug command still replays, and the record says so (D-171).
        RunRecord record = DebugIntentRecord();
        DebugIntentHandlers handlers = new(
        [
            new KeyValuePair<ContentId, DebugIntentHandler>(
                RunScripts.DebugStepEast,
                (state, context, log) => state.WantStep(StepDirection.East, context)),
        ]);

        RunState replayed = RunReplay.Play(record, ContentHash, TestMaps.Room, handlers);

        Assert.Equal(StepDirection.East, replayed.Party.Facing);
        Assert.Equal(5, replayed.Tick);
    }

    [Fact]
    public void AReplayRunsTheTicksAfterTheLastIntent()
    {
        // The end tick is its own value, because a run takes many ticks after its last
        // intent. A replay that stopped at the last line would give another state.
        const ulong seed = 7;
        Simulation run = Simulation.Start(seed, TestMaps.Room, DebugIntentHandlers.None);
        RunRecorder recorder = new(RunHeader.ForThisBuild(ContentHash, seed), run.Snapshot());

        Intent[] open = [Intent.OfPlayer(IntentIds.OpenMenu)];
        run.Step(open);
        recorder.Step(run.Tick, open);
        for (int step = 0; step < 200; step += 1)
        {
            run.Step([]);
            recorder.Step(run.Tick, []);
        }

        RunState replayed = RunReplay.Play(recorder.Build(), ContentHash, TestMaps.Room, DebugIntentHandlers.None);

        Assert.Equal(201, replayed.Tick);
        Assert.Equal(run.StateHash(), replayed.StateHash());
    }

    [Fact]
    public void AReplayOfAWalkGivesTheSameStateHashAndTheSameWalkedTiles()
    {
        // Exit test 7 of section 7.3 of `phase-2-first-playable.md`. The walk of a run
        // replays to the same tile, the same step, and the same record (D-567, G-5, T-7).
        const ulong seed = 0x0000000000cafe01;
        Simulation run = Simulation.Start(seed, TestMaps.Room, DebugIntentHandlers.None);
        RunRecorder recorder = new(RunHeader.ForThisBuild(ContentHash, seed), run.Snapshot());

        foreach (IReadOnlyList<Intent> intents in RunScripts.Make(seed, 400))
        {
            run.Step(intents);
            recorder.Step(run.Tick, intents);
        }

        RunState replayed = RunReplay.Play(recorder.Build(), ContentHash, TestMaps.Room, DebugIntentHandlers.None);

        Assert.Equal(run.StateHash(), replayed.StateHash());
        Assert.Equal(run.State.Party.LeadAt, replayed.Party.LeadAt);
        Assert.Equal(run.State.Party.Stepping, replayed.Party.Stepping);
        Assert.Equal(run.State.Party.StepTicks, replayed.Party.StepTicks);
        Assert.Equal(run.State.Party.Walked.Rows(), replayed.Party.Walked.Rows());
        Assert.True(replayed.Party.Walked.Count > 1);
    }

    private static RunRecord OneTickRecord(Func<RunHeader, RunHeader> change)
    {
        const ulong seed = 1;
        Simulation run = Simulation.Start(seed, TestMaps.Room, DebugIntentHandlers.None);
        RunHeader header = change(RunHeader.ForThisBuild(ContentHash, seed));
        RunRecorder recorder = new(header, run.Snapshot());

        run.Step([]);
        recorder.Step(run.Tick, []);
        return recorder.Build();
    }

    private static RunRecord DebugIntentRecord()
    {
        const ulong seed = 3;
        DebugIntentHandlers handlers = new(
        [
            new KeyValuePair<ContentId, DebugIntentHandler>(
                RunScripts.DebugStepEast,
                (state, context, log) => state.WantStep(StepDirection.East, context)),
        ]);

        Simulation run = Simulation.Start(seed, TestMaps.Room, handlers);
        RunRecorder recorder = new(RunHeader.ForThisBuild(ContentHash, seed), run.Snapshot());

        for (int tick = 1; tick <= 5; tick += 1)
        {
            Intent[] intents = tick == 3 ? [Intent.OfDebugConsole(RunScripts.DebugStepEast)] : [];
            run.Step(intents);
            recorder.Step(run.Tick, intents);
        }

        return recorder.Build();
    }
}
