using System;
using System.IO;
using TheThingBelow.Tools.Bots;
using TheThingBelow.Tools.Night;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The night record: its form, and each error of a read (D-509, T-2).</summary>
public sealed class NightRecordTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "night-record-" + Guid.NewGuid().ToString("N"));

    public NightRecordTests() => Directory.CreateDirectory(this.folder);

    [Fact]
    public void ARecordReadsBackAsItWasWritten()
    {
        NightRecord record = NightGateFixture.RedRecord(NightGateFixture.MainCommit, "macos-26");
        string path = Path.Combine(this.folder, "record.json");

        record.Write(path);
        NightRecord read = NightRecord.Read(path);

        Assert.Equal(record.Commit, read.Commit);
        Assert.Equal(record.Leg, read.Leg);
        Assert.Equal(record.FirstSeed, read.FirstSeed);
        Assert.Equal(record.Policies, read.Policies);
        Assert.Equal(NightRecord.Failure, read.Status);
    }

    [Fact]
    public void TheFileUsesTheLineEndOfGitOnEverySystem()
    {
        string path = Path.Combine(this.folder, "record.json");

        NightGateFixture.GreenRecord(NightGateFixture.MainCommit, "windows-2025").Write(path);

        string text = File.ReadAllText(path);
        Assert.DoesNotContain("\r", text, StringComparison.Ordinal);
        Assert.EndsWith("}\n", text, StringComparison.Ordinal);
        Assert.Contains("\"status\": \"success\"", text, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAbsentFieldIsAnError()
    {
        string path = this.WriteText("""{ "commit": "d4e5f60718293a4b5c6d7e8f90123456789ab2c3", "leg": "macos-26", "firstSeed": 1, "policies": [] }""");

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => NightRecord.Read(path));

        Assert.Contains("holds no field 'status'", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnknownFieldIsAnError()
    {
        string path = Path.Combine(this.folder, "record.json");
        NightGateFixture.GreenRecord(NightGateFixture.MainCommit, "macos-26").Write(path);
        File.WriteAllText(path, File.ReadAllText(path).Replace("\"leg\":", "\"passed\": true, \"leg\":", StringComparison.Ordinal));

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => NightRecord.Read(path));

        Assert.Contains("holds the unknown field 'passed'", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStatusThatTheCountsDoNotGiveIsAnError()
    {
        string path = Path.Combine(this.folder, "record.json");
        NightGateFixture.RedRecord(NightGateFixture.MainCommit, "macos-26").Write(path);
        File.WriteAllText(path, File.ReadAllText(path).Replace("\"failure\"", "\"success\"", StringComparison.Ordinal));

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => NightRecord.Read(path));

        Assert.Contains("holds 'success', and its counts give 'failure'", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EndsThatDoNotAddUpToTheRunsAreAnError()
    {
        NightRecord record = new(
            NightGateFixture.MainCommit,
            "macos-26",
            1,
            [new NightPolicyCounts(BotPolicyKind.Greedy, 30, 20, 0, 0, 2, 11), new NightPolicyCounts(BotPolicyKind.Random, 20, 1, 0, 0, 19, 9)]);
        string path = Path.Combine(this.folder, "record.json");
        record.Write(path);

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => NightRecord.Read(path));

        Assert.Contains("count 22 runs, and its field 'runs' gives 30", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePoliciesInAnotherOrderAreAnError()
    {
        NightRecord record = new(
            NightGateFixture.MainCommit,
            "macos-26",
            1,
            [new NightPolicyCounts(BotPolicyKind.Random, 20, 1, 0, 0, 19, 9), new NightPolicyCounts(BotPolicyKind.Greedy, 30, 28, 0, 0, 2, 11)]);
        string path = Path.Combine(this.folder, "record.json");
        record.Write(path);

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => NightRecord.Read(path));

        Assert.Contains("holds 'random', and the night gate needs 'greedy' there", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AShortCommitIsAnError()
    {
        string path = Path.Combine(this.folder, "record.json");
        NightGateFixture.GreenRecord("d4e5f60", "macos-26").Write(path);

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => NightRecord.Read(path));

        Assert.Contains("holds 'd4e5f60', and the night gate needs a full commit hash", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFileWithNoValidJsonIsAnErrorThatNamesTheFile()
    {
        string path = this.WriteText("{ \"commit\": ");

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => NightRecord.Read(path));

        Assert.Contains($"the night record '{path}' holds no valid JSON", fault.Message, StringComparison.Ordinal);
    }

    public void Dispose()
    {
        if (Directory.Exists(this.folder))
        {
            Directory.Delete(this.folder, recursive: true);
        }
    }

    private string WriteText(string text)
    {
        string path = Path.Combine(this.folder, "record.json");
        File.WriteAllText(path, text);
        return path;
    }
}
