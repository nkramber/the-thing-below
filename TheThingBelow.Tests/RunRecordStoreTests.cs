using System;
using System.IO;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using TheThingBelow.Storage;
using TheThingBelow.Tools.Bots;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

public sealed class RunRecordStoreTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "records-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void AWrittenRecordReadsBackTheSame()
    {
        ContentSet content = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));
        BotResult result = BotRun.Play(content, 3, new RandomPolicy(3), AcceptedSources.OfCore, Path.Combine(this.folder, "saves"));
        RunRecordStore store = new(this.folder);

        string path = store.Write("random-3", result.Record);

        Assert.Equal(Path.Combine(this.folder, "random-3.record"), path);
        Assert.Equal(RunRecordText.Write(result.Record), RunRecordText.Write(RunRecordStore.Read(path)));
    }

    [Theory]
    [InlineData("a/b")]
    [InlineData("a\\b")]
    [InlineData("c:d")]
    [InlineData("name.record")]
    public void ANameWithAFolderOrAFileTypeIsAnError(string name)
    {
        RunRecordStore store = new(this.folder);
        RunRecorder recorder = new(RunHeader.ForThisBuild("hash", 1), SnapshotOfAStart());

        Assert.Throws<ArgumentException>(() => store.Write(name, recorder.Build()));
    }

    [Fact]
    public void AnAbsentFileIsAnError()
    {
        StorageException fault = Assert.Throws<StorageException>(() => RunRecordStore.Read(Path.Combine(this.folder, "none.record")));

        Assert.Contains("none.record", fault.Message, StringComparison.Ordinal);
    }

    public void Dispose()
    {
        if (Directory.Exists(this.folder))
        {
            Directory.Delete(this.folder, recursive: true);
        }
    }

    private static RunSnapshot SnapshotOfAStart() => BattleRuns.IntoBattle(1, "group.fixture_pair").Snapshot();
}
