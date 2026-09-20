using System;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The text of one snapshot: one JSON object on one line (D-652). The run record and the save
/// file both hold it, so one writer and one reader serve both (D-494, D-655, T-1).
/// </summary>
public sealed class RunSnapshotTextTests
{
    private const string SourceName = "the source of this test";

    [Fact]
    public void TheTextTakesOneObjectOnOneLine()
    {
        string line = RunSnapshotText.Write(SaveRuns.Play(20).Snapshot());

        Assert.StartsWith("{", line, StringComparison.Ordinal);
        Assert.EndsWith("}", line, StringComparison.Ordinal);
        Assert.DoesNotContain("\n", line, StringComparison.Ordinal);
    }

    [Fact]
    public void ASnapshotThatThisBuildWroteReadsBackToTheSameSnapshot()
    {
        RunSnapshot written = SaveRuns.Play(40).Snapshot();

        RunSnapshot read = Read(RunSnapshotText.Write(written));

        Assert.Equal(written.Tick, read.Tick);
        Assert.Equal(written.MenuOpen, read.MenuOpen);
        Assert.Equal(written.WorldTick, read.WorldTick);
        Assert.NotNull(written.Map);
        Assert.NotNull(read.Map);
        Assert.Equal(written.Map.Map.Value, read.Map.Map.Value);
        Assert.Equal(written.Map.LeadX, read.Map.LeadX);
        Assert.Equal(written.Map.LeadY, read.Map.LeadY);
        Assert.Equal(written.Map.Facing, read.Map.Facing);
        Assert.Equal(written.Map.Stepping, read.Map.Stepping);
        Assert.Equal(written.Map.StepTicks, read.Map.StepTicks);
        Assert.Equal(written.Map.Walked, read.Map.Walked);
        Assert.Equal(written.Streams, read.Streams);
    }

    [Fact]
    public void TheRecordAndTheSaveWriteOneSnapshotLine()
    {
        // The record holds the snapshot on line 2, as a save does, and one writer makes both
        // (D-494, D-655, T-1). A second copy of the writer would drift from this one.
        RunSnapshot snapshot = SaveRuns.Play(20).Snapshot();
        RunRecord record = new(
            RunHeader.ForThisBuild(SaveRuns.ContentHash, SaveRuns.Seed), snapshot, [], snapshot.Tick);

        string line = RunRecordText.Write(record).Split('\n')[1];

        Assert.Equal(RunSnapshotText.Write(snapshot), line);
    }

    [Fact]
    public void AReadNamesTheSourceOfTheLineInTheErrorOfAValue()
    {
        // The reader of a record names the record, and the reader of a save names the file,
        // so every fault of a snapshot carries the place that holds it (T-2).
        string line = WithField(RunSnapshotText.Write(SaveRuns.Play(20).Snapshot()), "world", "-1");

        ArgumentException error = Assert.Throws<ArgumentException>(() => Read(line));

        Assert.Contains(SourceName, error.Message, StringComparison.Ordinal);
        Assert.Contains("which is below zero", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AReadRefusesAnUnknownField()
    {
        string line = RunSnapshotText.Write(SaveRuns.Play(20).Snapshot())
            .Replace("{\"tick\":", "{\"fog\":1,\"tick\":", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => Read(line));

        Assert.Contains("fog", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AReadRefusesAnAbsentField()
    {
        string line = WithoutField(RunSnapshotText.Write(SaveRuns.Play(20).Snapshot()), "menu");

        ContentException error = Assert.Throws<ContentException>(() => Read(line));

        Assert.Contains("menu", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Gives the line again with another value in one field before the streams.</summary>
    private static string WithField(string line, string field, string value)
    {
        int start = line.IndexOf($"\"{field}\":", StringComparison.Ordinal);
        int end = line.IndexOf(',', start);
        return line[..start] + $"\"{field}\":{value}" + line[end..];
    }

    /// <summary>Gives the line again with no such field before the streams.</summary>
    private static string WithoutField(string line, string field)
    {
        int start = line.IndexOf($"\"{field}\":", StringComparison.Ordinal);
        int end = line.IndexOf(',', start) + 1;
        return line[..start] + line[end..];
    }

    private static RunSnapshot Read(string line)
    {
        var reader = new ContentReader(System.Text.Encoding.UTF8.GetBytes(line), SourceName);
        RunSnapshot snapshot = RunSnapshotText.Read(ref reader);
        reader.ReadFileEnd();
        return snapshot;
    }
}
