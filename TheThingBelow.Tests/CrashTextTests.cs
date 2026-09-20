using System;
using System.Collections.Generic;
using System.Text.Json;
using TheThingBelow.Core;
using TheThingBelow.Core.Crashes;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The text of a crash file: the crash line, and then the lines of the run record (D-170,
/// D-652). A reader of the studio reads the error, the versions, and the replay from one file.
/// </summary>
public sealed class CrashTextTests
{
    /// <summary>The seed of the runs of these tests.</summary>
    private const ulong Seed = 0x00000000000c4a54;

    /// <summary>The wall-clock time that the host gives to these crash files.</summary>
    private const string Moment = "2026-09-18T01:42:53Z";

    [Fact]
    public void TheCrashLineIsOneJsonObjectOnLineOne()
    {
        string text = CrashText.Write(Report(WithRecord()));

        string[] lines = text.TrimEnd('\n').Split('\n');
        using JsonDocument document = JsonDocument.Parse(lines[0]);
        Assert.Equal(JsonValueKind.Object, document.RootElement.ValueKind);
        Assert.True(lines.Length > 1);
    }

    [Fact]
    public void TheCrashLineHoldsTheErrorTheVersionsAndTheTime()
    {
        // Exit test 1 of section 7.15: a crash file holds the context of the error (D-170).
        CrashReport report = Report(WithRecord());

        using JsonDocument document = JsonDocument.Parse(CrashText.Write(report).Split('\n')[0]);
        JsonElement root = document.RootElement;
        Assert.Equal(CrashFormat.Current, root.GetProperty("format").GetInt32());
        Assert.Equal(Moment, root.GetProperty("time").GetString());
        Assert.Equal(nameof(SimulationException), root.GetProperty("type").GetString());
        Assert.Contains("seed", root.GetProperty("error").GetString()!, StringComparison.Ordinal);
        Assert.Equal(SimulationVersion.Current, root.GetProperty("simulation").GetInt32());
        Assert.Equal(GameVersion.Current, root.GetProperty("game").GetString());
        Assert.True(root.GetProperty("record").GetBoolean());
    }

    [Fact]
    public void TheRecordOfACrashFileReachesTheStateHashOfTheRun()
    {
        // Exit test 4 of section 7.15: a test loads the record of a crash file and reaches the
        // same state hash (G-5).
        (Simulation run, RunRecorder recorder) = RunScripts.Play(
            Seed, SaveRuns.ContentHash, RunScripts.Make(Seed, 300), saveEvery: 0);

        CrashReport read = CrashText.Read(
            CrashText.Write(Report(recorder.Build())), "the-file.json");

        RunState replayed = RunReplay.Play(read.Record!, SaveRuns.ContentHash, TestMaps.Room, DebugIntentHandlers.None);
        Assert.Equal(run.StateHash(), replayed.StateHash());
    }

    [Fact]
    public void AFailedAssertionWritesTheSameFile()
    {
        // Exit test 2 of section 7.15: a failed assertion writes the same file (T-2).
        SimulationException fault = Assert.Throws<SimulationException>(
            () => CoreAssert.That(false, "the party holds one member", new RunContext(Seed, 12, "party")));

        CrashReport read = CrashText.Read(
            CrashText.Write(CrashReport.Of(fault, fault.GetType().Name, Moment, WithRecord())), "the-file.json");

        Assert.Equal(nameof(SimulationException), read.ErrorType);
        Assert.Contains("assertion failed", read.Error, StringComparison.Ordinal);
        Assert.NotNull(read.Record);
    }

    [Fact]
    public void ACrashBeforeARunHoldsNoRecord()
    {
        // The crash path runs with no content loaded, because a load failure can start it
        // (D-170, section 7.15).
        CrashReport report = CrashReport.Of(new InvalidOperationException("the content is absent"), nameof(InvalidOperationException), Moment, null);

        string text = CrashText.Write(report);
        CrashReport read = CrashText.Read(text, "the-file.json");

        Assert.Single(text.TrimEnd('\n').Split('\n'));
        Assert.Null(read.Record);
        Assert.Equal(SimulationVersion.Current, read.SimulationVersion);
        Assert.Equal(GameVersion.Current, read.GameVersion);
    }

    [Fact]
    public void TheErrorTextHoldsTheMessageOfEachErrorBelowTheFirst()
    {
        // No error hides the first error (T-2, G-18).
        Exception fault = new InvalidOperationException(
            "the game could not write the save",
            new InvalidOperationException("the disk is full"));

        Assert.Equal(
            $"the game could not write the save{CrashReport.ErrorSeparator}the disk is full",
            CrashReport.Of(fault, fault.GetType().Name, Moment, null).Error);
    }

    [Fact]
    public void AnErrorChainOfNoBoundStopsAtTheBound()
    {
        Exception fault = new InvalidOperationException("the last error");
        for (int count = 0; count < CrashReport.MaxErrorCount + 3; count += 1)
        {
            fault = new InvalidOperationException($"the error {count}", fault);
        }

        string text = CrashReport.Of(fault, fault.GetType().Name, Moment, null).Error;

        Assert.Contains("and more errors below", text, StringComparison.Ordinal);
    }

    [Fact]
    public void TheStackOfTheErrorStaysOnOneLine()
    {
        SimulationException fault = Assert.Throws<SimulationException>(
            () => CoreAssert.That(false, "the party holds one member", new RunContext(Seed, 12, "party")));

        string line = CrashText.Write(CrashReport.Of(fault, fault.GetType().Name, Moment, null)).TrimEnd('\n');

        Assert.Single(line.Split('\n'));
        Assert.Contains(nameof(CoreAssert), CrashText.Read(line + "\n", "the-file.json").Stack, StringComparison.Ordinal);
    }

    [Fact]
    public void AReadOfAWriteGivesTheSameText()
    {
        string first = CrashText.Write(Report(WithRecord()));

        Assert.Equal(first, CrashText.Write(CrashText.Read(first, "the-file.json")));
    }

    [Fact]
    public void AFileOfAnotherFormatVersionIsAnError()
    {
        string text = CrashText.Write(Report(WithRecord()));
        string other = text.Replace("{\"format\":1", "{\"format\":2", StringComparison.Ordinal);

        CrashException error = Assert.Throws<CrashException>(() => CrashText.Read(other, "the-file.json"));

        Assert.Contains("format version 2", error.Message, StringComparison.Ordinal);
        Assert.Contains("the-file.json", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AWriteOfAnotherFormatVersionIsAnError()
    {
        CrashReport report = Report(null) with { FormatVersion = 2 };

        Assert.Throws<ArgumentException>(() => CrashText.Write(report));
    }

    [Fact]
    public void ALineAfterACrashLineThatNamesNoRecordIsAnError()
    {
        string text = CrashText.Write(Report(null)) + "{\"end\":12}\n";

        CrashException error = Assert.Throws<CrashException>(() => CrashText.Read(text, "the-file.json"));

        Assert.Contains("holds no record", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ACrashLineThatNamesARecordWithNoLineAfterItIsAnError()
    {
        string text = CrashText.Write(Report(WithRecord())).Split('\n')[0] + "\n";

        CrashException error = Assert.Throws<CrashException>(() => CrashText.Read(text, "the-file.json"));

        Assert.Contains("holds no line after it", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAbsentFieldOfTheCrashLineIsAnError()
    {
        string text = CrashText.Write(Report(null))
            .Replace($"\"game\":\"{GameVersion.Current}\",", string.Empty, StringComparison.Ordinal);

        CrashException error = Assert.Throws<CrashException>(() => CrashText.Read(text, "the-file.json"));

        Assert.Contains("game", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARecordThatNoReaderReadsIsAnError()
    {
        string[] lines = CrashText.Write(Report(WithRecord())).TrimEnd('\n').Split('\n');
        lines[1] = "{\"tick\":0}";

        CrashException error = Assert.Throws<CrashException>(
            () => CrashText.Read(string.Join('\n', lines) + "\n", "the-file.json"));

        Assert.Contains("are not a record", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFileWithACarriageReturnIsAnError()
    {
        string text = CrashText.Write(Report(WithRecord())).Replace("\n", "\r\n", StringComparison.Ordinal);

        CrashException error = Assert.Throws<CrashException>(() => CrashText.Read(text, "the-file.json"));

        Assert.Contains("carriage return", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFileWithNoLineIsAnError()
    {
        CrashException error = Assert.Throws<CrashException>(() => CrashText.Read(string.Empty, "the-file.json"));

        Assert.Contains("holds no line", error.Message, StringComparison.Ordinal);
    }

    private static CrashReport Report(RunRecord? record)
    {
        SimulationException fault = Assert.Throws<SimulationException>(
            () => Simulation.Start(Seed, TestMaps.Room, DebugIntentHandlers.None).Step([Intent.OfPlayer(IntentIds.CloseMenu)]));

        return CrashReport.Of(fault, fault.GetType().Name, Moment, record);
    }

    private static RunRecord WithRecord()
    {
        IReadOnlyList<IReadOnlyList<Intent>> script = RunScripts.Make(Seed, 120);
        return RunScripts.Play(Seed, SaveRuns.ContentHash, script, saveEvery: 0).Recorder.Build();
    }
}
