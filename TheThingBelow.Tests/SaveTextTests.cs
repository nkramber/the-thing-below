using System;
using TheThingBelow.Core;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Saves;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The text of a save file: the header on line 1, and the snapshot on line 2 (D-652, D-655).
/// The header carries the SHA-256 digest of line 2, so a torn write or a hand edit fails the
/// load with the file and both digests (D-178, T-2).
/// </summary>
public sealed class SaveTextTests
{
    private const string FilePath = "/saves/slot.json";

    [Fact]
    public void TheTextTakesTwoObjectsOnTwoLines()
    {
        string text = SaveText.Write(SaveRuns.SaveAfter(20));

        string[] lines = text.TrimEnd('\n').Split('\n');
        Assert.Equal(SaveText.LineCount, lines.Length);
        foreach (string line in lines)
        {
            Assert.StartsWith("{", line, StringComparison.Ordinal);
            Assert.EndsWith("}", line, StringComparison.Ordinal);
        }

        Assert.EndsWith("}\n", text, StringComparison.Ordinal);
    }

    [Fact]
    public void TheHeaderLineHoldsEveryFieldAndTheChecksumOfTheSnapshotLine()
    {
        // D-655: the format version, the simulation version, the content hash, the game
        // version, the seed, and the digest of line 2.
        string[] lines = SaveText.Write(SaveRuns.SaveAfter(20)).TrimEnd('\n').Split('\n');

        Assert.Equal(
            "{\"format\":" + SaveFormat.Current +
            ",\"simulation\":" + SimulationVersion.Current +
            ",\"content\":\"" + SaveRuns.ContentHash +
            "\",\"game\":\"" + GameVersion.Current +
            "\",\"seed\":\"0x0000000000bada55\"" +
            ",\"checksum\":\"" + SaveText.ChecksumOf(lines[1]) + "\"}",
            lines[0]);
    }

    [Fact]
    public void ATextThatThisBuildWroteReadsBackToTheSameText()
    {
        string first = SaveText.Write(SaveRuns.SaveAfter(40));

        string second = SaveText.Write(SaveText.Read(first, FilePath));

        Assert.Equal(first, second);
    }

    [Fact]
    public void AReadSaveHoldsTheHeaderAndTheSnapshotOfTheRun()
    {
        SaveDocument written = SaveRuns.SaveAfter(40);

        SaveDocument read = SaveText.Read(SaveText.Write(written), FilePath);

        Assert.Equal(written.Header, read.Header);
        Assert.Equal(written.Snapshot.Tick, read.Snapshot.Tick);
        Assert.Equal(written.Snapshot.MenuOpen, read.Snapshot.MenuOpen);
        Assert.Equal(written.Snapshot.WorldTick, read.Snapshot.WorldTick);
        Assert.Equal(written.Snapshot.PatrolBeats, read.Snapshot.PatrolBeats);
        Assert.Equal(written.Snapshot.PatrolChoice, read.Snapshot.PatrolChoice);
        Assert.Equal(written.Snapshot.Streams, read.Snapshot.Streams);
    }

    [Fact]
    public void AReadSnapshotResumesTheRunToTheSameStateHash()
    {
        Simulation run = SaveRuns.Play(40);
        SaveDocument save = new(SaveHeader.ForThisBuild(SaveRuns.ContentHash, SaveRuns.Seed), run.Snapshot());

        SaveDocument read = SaveText.Read(SaveText.Write(save), FilePath);

        Assert.Equal(
            run.StateHash(),
            Simulation.Resume(read.Header.Seed, read.Snapshot, DebugIntentHandlers.None).StateHash());
    }

    [Fact]
    public void ASaveOfAnOlderSimulationVersionReads()
    {
        // D-259: a load reads the snapshot alone, so a patch of the rules never refuses a
        // save. The header carries the version for a report, and no rule compares it.
        SaveHeader older = new(
            SaveFormat.Current,
            SimulationVersion.Current - 1,
            "a-content-hash-of-another-build",
            "0.0.9",
            SaveRuns.Seed);
        string text = SaveText.Write(new SaveDocument(older, SaveRuns.Play(20).Snapshot()));

        SaveDocument read = SaveText.Read(text, FilePath);

        Assert.Equal(SimulationVersion.Current - 1, read.Header.SimulationVersion);
        Assert.Equal("a-content-hash-of-another-build", read.Header.ContentHash);
    }

    [Fact]
    public void AWriteRefusesAHeaderOfAnotherFormatVersion()
    {
        SaveHeader newer = new(
            SaveFormat.Current + 1, SimulationVersion.Current, SaveRuns.ContentHash, GameVersion.Current, SaveRuns.Seed);

        ArgumentException error = Assert.Throws<ArgumentException>(
            () => SaveText.Write(new SaveDocument(newer, SaveRuns.Play(1).Snapshot())));

        Assert.Contains($"format version {SaveFormat.Current + 1}", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AChangedSnapshotLineFailsTheChecksumAndNamesTheFileAndBothDigests()
    {
        string text = SaveText.Write(SaveRuns.SaveAfter(40));
        string[] lines = text.TrimEnd('\n').Split('\n');
        string changed = lines[1].Replace("\"beats\":", "\"beats\": ", StringComparison.Ordinal);
        string broken = lines[0] + "\n" + changed + "\n";

        SaveException error = Assert.Throws<SaveException>(() => SaveText.Read(broken, FilePath));

        Assert.Equal(FilePath, error.File);
        Assert.Contains(SaveText.ChecksumOf(lines[1]), error.Message, StringComparison.Ordinal);
        Assert.Contains(SaveText.ChecksumOf(changed), error.Message, StringComparison.Ordinal);
        Assert.Contains(FilePath, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AReadRefusesAFormatVersionThatANewerBuildWrote()
    {
        string text = SaveText.Write(SaveRuns.SaveAfter(20))
            .Replace($"\"format\":{SaveFormat.Current}", $"\"format\":{SaveFormat.Current + 1}", StringComparison.Ordinal);

        SaveException error = Assert.Throws<SaveException>(() => SaveText.Read(text, FilePath));

        Assert.Contains($"format version {SaveFormat.Current + 1}", error.Message, StringComparison.Ordinal);
        Assert.Contains($"format version {SaveFormat.Current}", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AReadRefusesAFormatVersionBelowTheOldestOfThisBuild()
    {
        string text = SaveText.Write(SaveRuns.SaveAfter(20))
            .Replace($"\"format\":{SaveFormat.Current}", "\"format\":0", StringComparison.Ordinal);

        SaveException error = Assert.Throws<SaveException>(() => SaveText.Read(text, FilePath));

        Assert.Contains("format version 0", error.Message, StringComparison.Ordinal);
        Assert.Contains($"format version {SaveFormat.Oldest} and later", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public void AReadRefusesATextOfAnotherCountOfLines(int lineCount)
    {
        string[] lines = SaveText.Write(SaveRuns.SaveAfter(20)).TrimEnd('\n').Split('\n');
        string text = lineCount == 1 ? lines[0] + "\n" : lines[0] + "\n" + lines[1] + "\n" + lines[1] + "\n";

        SaveException error = Assert.Throws<SaveException>(() => SaveText.Read(text, FilePath));

        Assert.Contains($"it holds {lineCount} lines", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AReadRefusesAFieldOfTheHeaderTwoTimes()
    {
        string text = SaveText.Write(SaveRuns.SaveAfter(20));
        string broken = text.Replace("{\"format\":", "{\"format\":1,\"format\":", StringComparison.Ordinal);
        Assert.NotEqual(text, broken);

        SaveException error = Assert.Throws<SaveException>(() => SaveText.Read(broken, FilePath));

        Assert.Contains("two times", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AReadRefusesACarriageReturn()
    {
        string text = SaveText.Write(SaveRuns.SaveAfter(20)).Replace("\n", "\r\n", StringComparison.Ordinal);

        SaveException error = Assert.Throws<SaveException>(() => SaveText.Read(text, FilePath));

        Assert.Contains("carriage return", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AReadRefusesAnUnknownFieldOfTheHeader()
    {
        string text = SaveText.Write(SaveRuns.SaveAfter(20))
            .Replace("{\"format\":", "{\"slot\":1,\"format\":", StringComparison.Ordinal);

        SaveException error = Assert.Throws<SaveException>(() => SaveText.Read(text, FilePath));

        Assert.Contains("slot", error.Message, StringComparison.Ordinal);
        Assert.Contains(FilePath, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AReadRefusesAnAbsentFieldOfTheHeader()
    {
        string text = SaveText.Write(SaveRuns.SaveAfter(20))
            .Replace($"\"format\":{SaveFormat.Current},", string.Empty, StringComparison.Ordinal);

        SaveException error = Assert.Throws<SaveException>(() => SaveText.Read(text, FilePath));

        Assert.Contains("format", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AReadRefusesAChecksumOfAnotherLength()
    {
        string text = SaveText.Write(SaveRuns.SaveAfter(20));
        string[] lines = text.TrimEnd('\n').Split('\n');
        string broken = lines[0].Replace(SaveText.ChecksumOf(lines[1]), "0f0f", StringComparison.Ordinal)
            + "\n" + lines[1] + "\n";

        SaveException error = Assert.Throws<SaveException>(() => SaveText.Read(broken, FilePath));

        Assert.Contains("the checksum holds 4 characters", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AReadRefusesAnEvenIncrementAndNamesTheFile()
    {
        // Every PCG32 increment is odd, and a stream of another sequence would replay to
        // another state (T-7, T-2). The snapshot check runs inside the read of line 2, so
        // the message names the save.
        string[] lines = SaveText.Write(SaveRuns.SaveAfter(20)).TrimEnd('\n').Split('\n');
        string snapshot = lines[1].Replace(
            "\"increment\":\"0x0000000000000003\"", "\"increment\":\"0x0000000000000004\"", StringComparison.Ordinal);
        string text = lines[0].Replace(SaveText.ChecksumOf(lines[1]), SaveText.ChecksumOf(snapshot), StringComparison.Ordinal)
            + "\n" + snapshot + "\n";

        SaveException error = Assert.Throws<SaveException>(() => SaveText.Read(text, FilePath));

        Assert.Contains("takes the increment", error.Message, StringComparison.Ordinal);
        Assert.Contains(FilePath, error.Message, StringComparison.Ordinal);
    }
}
