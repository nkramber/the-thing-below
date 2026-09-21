using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The text of a run record: one JSON object on each line (D-652). A person must read a
/// record in a diff, in a review, and in a crash email with no tool (D-170, D-473).
/// </summary>
public sealed class RunRecordTextTests
{
    private const ulong Seed = 20260918;
    private const string ContentHash = "a-content-hash";

    [Fact]
    public void TheTextTakesOneObjectOnEachLine()
    {
        string text = RunRecordText.Write(SmallRecord());

        string[] lines = text.TrimEnd('\n').Split('\n');
        Assert.Equal(4, lines.Length);
        foreach (string line in lines)
        {
            Assert.StartsWith("{", line, StringComparison.Ordinal);
            Assert.EndsWith("}", line, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void TheHeaderLineHoldsEveryFieldOfTheRecordHeader()
    {
        // G-5 and D-448: the format version, the simulation version, the content hash, the
        // seed, and the game version.
        string header = RunRecordText.Write(SmallRecord()).Split('\n')[0];

        Assert.Equal(
            "{\"format\":2,\"simulation\":" + SimulationVersion.Current +
            ",\"content\":\"a-content-hash\",\"seed\":\"0x0000000001352836\",\"game\":\"" +
            GameVersion.Current + "\"}",
            header);
    }

    [Fact]
    public void ATextThatThisBuildWroteReadsBackToTheSameText()
    {
        string first = RunRecordText.Write(SmallRecord());

        string second = RunRecordText.Write(RunRecordText.Read(first));

        Assert.Equal(first, second);
    }

    [Fact]
    public void AReadRecordHoldsEveryValueOfTheRecord()
    {
        RunRecord written = SmallRecord();

        RunRecord read = RunRecordText.Read(RunRecordText.Write(written));

        Assert.Equal(written.Header, read.Header);
        Assert.Equal(written.EndTick, read.EndTick);
        Assert.Equal(written.Snapshot.Tick, read.Snapshot.Tick);
        Assert.Equal(written.Snapshot.MenuOpen, read.Snapshot.MenuOpen);
        Assert.Equal(written.Snapshot.Streams.Count, read.Snapshot.Streams.Count);
        Assert.Equal(written.Ticks.Count, read.Ticks.Count);
        Assert.Equal(written.Ticks[0].Intents[0].Action.Value, read.Ticks[0].Intents[0].Action.Value);
        Assert.Equal(written.Ticks[0].Intents[0].IsDebug, read.Ticks[0].Intents[0].IsDebug);
    }

    [Fact]
    public void ADebugIntentCarriesItsMarkThroughTheText()
    {
        // D-171: the record marks a debug intent, so a run with a cheat says so.
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, new DebugIntentHandlers(
        [
            new KeyValuePair<Core.Content.ContentId, DebugIntentHandler>(
                RunScripts.DebugStepEast,
                (state, intent, context, log) => state.WantStep(StepDirection.East, context)),
        ]));
        RunRecorder recorder = new(RunHeader.ForThisBuild(ContentHash, Seed), run.Snapshot());

        Intent[] intents = [Intent.OfDebugConsole(RunScripts.DebugStepEast)];
        run.Step(intents);
        recorder.Step(run.Tick, intents);

        string text = RunRecordText.Write(recorder.Build());

        Assert.Contains("\"action\":\"debug.step_east\",\"debug\":true", text, StringComparison.Ordinal);
    }

    [Fact]
    public void ASeedThatFillsTheSixtyFourBitsSurvivesTheText()
    {
        // A JSON number of that size loses its top bits in a reader that holds numbers as a
        // fraction, so the seed takes the hexadecimal text form (T-7).
        Simulation run = Simulation.Start(ulong.MaxValue, TestMaps.Room, TestBattles.Content, DebugIntentHandlers.None);
        RunRecorder recorder = new(RunHeader.ForThisBuild(ContentHash, ulong.MaxValue), run.Snapshot());
        run.Step([]);
        recorder.Step(run.Tick, []);

        RunRecord read = RunRecordText.Read(RunRecordText.Write(recorder.Build()));

        Assert.Equal(ulong.MaxValue, read.Header.Seed);
    }

    [Theory]
    [InlineData("", "it holds 0 lines")]
    [InlineData("{\"format\":1}\n", "it holds 1 lines")]
    public void ARecordWithTooFewLinesIsAnError(string text, string reason)
    {
        RunRecordException error = Assert.Throws<RunRecordException>(() => RunRecordText.Read(text));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ACarriageReturnIsAnError()
    {
        string text = RunRecordText.Write(SmallRecord()).Replace("\n", "\r\n", StringComparison.Ordinal);

        RunRecordException error = Assert.Throws<RunRecordException>(() => RunRecordText.Read(text));

        Assert.Contains("carriage return", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEmptyLineInsideTheRecordIsAnError()
    {
        string[] lines = RunRecordText.Write(SmallRecord()).TrimEnd('\n').Split('\n');
        string text = string.Join('\n', lines[0], string.Empty, lines[1], lines[2], lines[3]) + "\n";

        RunRecordException error = Assert.Throws<RunRecordException>(() => RunRecordText.Read(text));

        Assert.Equal(2, error.Line);
    }

    [Fact]
    public void AnAbsentFieldOfTheHeaderIsAnErrorThatNamesTheLineAndTheField()
    {
        string[] lines = RunRecordText.Write(SmallRecord()).TrimEnd('\n').Split('\n');
        lines[0] = "{\"format\":2,\"simulation\":3,\"content\":\"a\",\"seed\":\"0x0000000000000001\"}";

        RunRecordException error = Assert.Throws<RunRecordException>(
            () => RunRecordText.Read(string.Join('\n', lines) + "\n"));

        Assert.Equal(1, error.Line);
        Assert.Contains("game", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnknownFieldOfATickLineIsAnError()
    {
        string[] lines = RunRecordText.Write(SmallRecord()).TrimEnd('\n').Split('\n');
        lines[2] = "{\"tick\":1,\"intents\":[],\"device\":\"a gamepad\"}";

        RunRecordException error = Assert.Throws<RunRecordException>(
            () => RunRecordText.Read(string.Join('\n', lines) + "\n"));

        // The record holds no device kind, so a rebind never changes a record (D-493).
        Assert.Equal(3, error.Line);
        Assert.Contains("device", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("\"0x1\"")]
    [InlineData("\"0X000000000135335A\"")]
    [InlineData("\"0x00000000013533ZZ\"")]
    [InlineData("1234")]
    public void ASeedOfAnotherFormIsAnError(string seed)
    {
        string[] lines = RunRecordText.Write(SmallRecord()).TrimEnd('\n').Split('\n');
        lines[0] = "{\"format\":2,\"simulation\":3,\"content\":\"a\",\"seed\":" + seed + ",\"game\":\"0.1.0\"}";

        RunRecordException error = Assert.Throws<RunRecordException>(
            () => RunRecordText.Read(string.Join('\n', lines) + "\n"));

        Assert.Equal(1, error.Line);
    }

    [Fact]
    public void ASnapshotWithTooFewStreamsIsAnErrorOfTheSnapshotLine()
    {
        string[] lines = RunRecordText.Write(SmallRecord()).TrimEnd('\n').Split('\n');
        lines[1] = "{\"tick\":0,\"menu\":false,\"world\":0,\"map\":{\"id\":\"map.test_room\",\"x\":2,\"y\":2,"
            + "\"facing\":\"south\",\"step_ticks\":0,\"walked\":[\"x\"],\"enemies\":[]},"
            + "\"party\":{\"characters\":[{\"id\":\"character.marrek\",\"health\":60,\"row\":\"front\"}],\"pack\":[]},\"streams\":[]}";

        RunRecordException error = Assert.Throws<RunRecordException>(
            () => RunRecordText.Read(string.Join('\n', lines) + "\n"));

        Assert.Equal(2, error.Line);
        Assert.Contains("streams", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStreamIncrementThatIsEvenIsAnErrorOfTheSnapshotLine()
    {
        // A regression test for P2-1 of `docs/reviews/pr-29.md`. Every PCG32 increment is
        // odd. The reader accepted an even one, and the fault then came out of
        // `Pcg32.FromSnapshot` as a bare error with no line of the record (T-2, G-18).
        string[] lines = RunRecordText.Write(SmallRecord()).TrimEnd('\n').Split('\n');
        lines[1] = ReplaceFirstIncrement(lines[1], "0x0000000000000000");

        RunRecordException error = Assert.Throws<RunRecordException>(
            () => RunRecordText.Read(string.Join('\n', lines) + "\n"));

        Assert.Equal(2, error.Line);
        Assert.Contains("increment", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEvenStreamIncrementNeverReachesAReplay()
    {
        // The same fault must stop at the read, so no replay starts on a stream that gives
        // another sequence than the run (T-7, G-5).
        string[] lines = RunRecordText.Write(SmallRecord()).TrimEnd('\n').Split('\n');
        lines[1] = ReplaceFirstIncrement(lines[1], "0x00000000000000f0");
        string text = string.Join('\n', lines) + "\n";

        Assert.Throws<RunRecordException>(
            () => RunReplay.Play(RunRecordText.Read(text), ContentHash, TestMaps.Room, TestBattles.Content, DebugIntentHandlers.None));
    }

    [Fact]
    public void AStreamIncrementOfAnotherStreamIsAnErrorOfTheSnapshotLine()
    {
        // The increment of a stream comes from its number alone, so an odd increment of
        // another number is no state of this build either (T-7).
        string[] lines = RunRecordText.Write(SmallRecord()).TrimEnd('\n').Split('\n');
        lines[1] = ReplaceFirstIncrement(lines[1], "0x0000000000000011");

        RunRecordException error = Assert.Throws<RunRecordException>(
            () => RunRecordText.Read(string.Join('\n', lines) + "\n"));

        Assert.Equal(2, error.Line);
        Assert.Contains("increment", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFieldOfTheHeaderTwoTimesIsAnErrorThatNamesTheLine()
    {
        string[] lines = RunRecordText.Write(SmallRecord()).TrimEnd('\n').Split('\n');
        lines[0] = lines[0].Replace("{\"format\":2,", "{\"format\":2,\"format\":1,", StringComparison.Ordinal);

        RunRecordException error = Assert.Throws<RunRecordException>(
            () => RunRecordText.Read(string.Join('\n', lines) + "\n"));

        Assert.Equal(1, error.Line);
        Assert.Contains("two times", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Puts another increment in the first stream of a snapshot line.</summary>
    /// <param name="line">The snapshot line of a record.</param>
    /// <param name="increment">The hexadecimal text of the new increment.</param>
    /// <returns>The line, with the first increment changed.</returns>
    private static string ReplaceFirstIncrement(string line, string increment)
    {
        const string field = "\"increment\":\"";
        int start = line.IndexOf(field, StringComparison.Ordinal) + field.Length;
        return string.Concat(line.AsSpan(0, start), increment, line.AsSpan(start + increment.Length));
    }

    [Fact]
    public void ATextWithNoEndLineIsAnError()
    {
        string[] lines = RunRecordText.Write(SmallRecord()).TrimEnd('\n').Split('\n');

        RunRecordException error = Assert.Throws<RunRecordException>(
            () => RunRecordText.Read(string.Join('\n', lines[0], lines[1], lines[2]) + "\n"));

        // The tick line then reads as the end line, and it holds no `end` field.
        Assert.Equal(3, error.Line);
        Assert.Contains("the end tick alone", error.Message, StringComparison.Ordinal);
    }

    private static RunRecord SmallRecord()
    {
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, DebugIntentHandlers.None);
        RunRecorder recorder = new(RunHeader.ForThisBuild(ContentHash, Seed), run.Snapshot());

        Intent[] open = [Intent.OfPlayer(IntentIds.OpenMenu)];
        run.Step(open);
        recorder.Step(run.Tick, open);
        run.Step([]);
        recorder.Step(run.Tick, []);

        return recorder.Build();
    }
}
