using System;
using System.IO;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Saves;
using TheThingBelow.Storage;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The three save files, the safe write of D-178, and the one-use resume file of D-258.
/// Every test writes to a folder of its own under the temporary folder of the machine, and
/// no test reads or writes the folder of the person (D-465).
/// </summary>
public sealed class SaveStoreTests : IDisposable
{
    private readonly string folder;
    private readonly SaveStore store;

    public SaveStoreTests()
    {
        this.folder = Path.Combine(Path.GetTempPath(), "the-thing-below-tests", Guid.NewGuid().ToString("n"));
        this.store = new SaveStore(Path.Combine(this.folder, SaveFolder.SavesName));
    }

    /// <summary>Removes the folder of this test.</summary>
    public void Dispose()
    {
        if (Directory.Exists(this.folder))
        {
            Directory.Delete(this.folder, recursive: true);
        }
    }

    [Fact]
    public void EachSaveTakesItsOwnFileOfJson()
    {
        Assert.Equal("slot.json", SaveStore.FileNameOf(SaveKind.Slot));
        Assert.Equal("autosave.json", SaveStore.FileNameOf(SaveKind.Autosave));
        Assert.Equal("resume.json", SaveStore.FileNameOf(SaveKind.Resume));
    }

    [Fact]
    public void AWriteMakesTheFolderOfTheSaves()
    {
        Assert.False(Directory.Exists(this.store.Folder));

        this.store.Write(SaveKind.Slot, SaveRuns.SaveAfter(20));

        Assert.True(File.Exists(this.store.PathOf(SaveKind.Slot)));
        Assert.True(this.store.Exists(SaveKind.Slot));
    }

    [Fact]
    public void ASaveReloadsToTheSameStateHash()
    {
        // Exit test 1 of section 7.14: a save reloads to the state that wrote it (D-259).
        Simulation run = SaveRuns.Play(60);
        this.store.Write(
            SaveKind.Slot,
            new SaveDocument(SaveHeader.ForThisBuild(SaveRuns.ContentHash, SaveRuns.Seed), run.Snapshot()));

        SaveDocument read = this.store.Read(SaveKind.Slot);

        Assert.Equal(
            run.StateHash(),
            Simulation.Resume(read.Header.Seed, read.Snapshot, TestMaps.Room, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None).StateHash());
    }

    [Fact]
    public void EachSaveHoldsItsOwnState()
    {
        this.store.Write(SaveKind.Slot, SaveRuns.SaveAfter(20));
        this.store.Write(SaveKind.Autosave, SaveRuns.SaveAfter(60));

        Assert.Equal(20, this.store.Read(SaveKind.Slot).Snapshot.Tick);
        Assert.Equal(60, this.store.Read(SaveKind.Autosave).Snapshot.Tick);
    }

    [Fact]
    public void AWriteThatStopsBeforeTheRenameLeavesTheOldSaveWhole()
    {
        // Exit test 2 of section 7.14, and the test of D-178: the write stops between the
        // temporary file and the rename, as a crash or a power loss does.
        this.store.Write(SaveKind.Slot, SaveRuns.SaveAfter(20));
        string path = this.store.PathOf(SaveKind.Slot);
        string old = File.ReadAllText(path);

        Assert.Throws<InvalidOperationException>(() => SafeWrite.Replace(
            path,
            SaveText.Write(SaveRuns.SaveAfter(60)),
            static () => throw new InvalidOperationException("the machine stopped")));

        Assert.Equal(old, File.ReadAllText(path));
        Assert.Equal(20, this.store.Read(SaveKind.Slot).Snapshot.Tick);
    }

    [Fact]
    public void TheTemporaryFileOfAStoppedWriteIsNoSave()
    {
        this.store.Write(SaveKind.Slot, SaveRuns.SaveAfter(20));
        string path = this.store.PathOf(SaveKind.Slot);

        Assert.Throws<InvalidOperationException>(() => SafeWrite.Replace(
            path,
            SaveText.Write(SaveRuns.SaveAfter(60)),
            static () => throw new InvalidOperationException("the machine stopped")));

        // The temporary file sits beside the save, and the reader of a save never reads it.
        Assert.True(File.Exists(path + SafeWrite.TemporarySuffix));
        Assert.Equal(20, this.store.Read(SaveKind.Slot).Snapshot.Tick);

        // A later write replaces the save, and the temporary file of it as well.
        this.store.Write(SaveKind.Slot, SaveRuns.SaveAfter(60));
        Assert.Equal(60, this.store.Read(SaveKind.Slot).Snapshot.Tick);
    }

    [Fact]
    public void AChangedFileFailsTheChecksumAndNamesThePathAndTheReason()
    {
        // Exit test 3 of section 7.14: a checksum failure names the file and the reason.
        this.store.Write(SaveKind.Autosave, SaveRuns.SaveAfter(20));
        string path = this.store.PathOf(SaveKind.Autosave);
        string[] lines = File.ReadAllText(path).TrimEnd('\n').Split('\n');
        File.WriteAllText(path, lines[0] + "\n" + lines[1].Replace("\"world\"", "\"world\" ", StringComparison.Ordinal) + "\n");

        SaveException error = Assert.Throws<SaveException>(() => this.store.Read(SaveKind.Autosave));

        Assert.Equal(path, error.File);
        Assert.Contains(path, error.Message, StringComparison.Ordinal);
        Assert.Contains("checksum", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AHalfWrittenFileFailsTheLoadAndNamesThePath()
    {
        this.store.Write(SaveKind.Slot, SaveRuns.SaveAfter(20));
        string path = this.store.PathOf(SaveKind.Slot);
        string text = File.ReadAllText(path);
        File.WriteAllText(path, text.Substring(0, text.Length / 2));

        SaveException error = Assert.Throws<SaveException>(() => this.store.Read(SaveKind.Slot));

        Assert.Contains(path, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheResumeFileServesOneLoad()
    {
        // Exit test 6 of section 7.14, and D-258: a load of the resume file removes it, so a
        // wipe never reloads it.
        this.store.Write(SaveKind.Resume, SaveRuns.SaveAfter(60));

        Assert.Equal(60, this.store.Read(SaveKind.Resume).Snapshot.Tick);
        this.store.RemoveResume();

        Assert.False(this.store.Exists(SaveKind.Resume));
        StorageException error = Assert.Throws<StorageException>(() => this.store.Read(SaveKind.Resume));
        Assert.Contains("resume file", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-258", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AResumeFileThatParsesStaysUntilTheRunResumes()
    {
        // Finding P3-13 of the repository review: the read removed a resume file that parsed, and
        // a resume that then failed lost the only copy of the quit save (D-258, T-2).
        this.store.Write(SaveKind.Resume, SaveRuns.SaveAfter(60));

        this.store.Read(SaveKind.Resume);

        Assert.True(this.store.Exists(SaveKind.Resume));
        Assert.Equal(60, this.store.Read(SaveKind.Resume).Snapshot.Tick);
    }

    [Fact]
    public void ARemovalOfAnAbsentResumeFileNamesThePath()
    {
        StorageException error = Assert.Throws<StorageException>(() => this.store.RemoveResume());

        Assert.Equal(this.store.PathOf(SaveKind.Resume), error.Path);
        Assert.Contains("D-258", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARemovalOfTheResumeFileLeavesTheOtherSaves()
    {
        this.store.Write(SaveKind.Slot, SaveRuns.SaveAfter(20));
        this.store.Write(SaveKind.Resume, SaveRuns.SaveAfter(60));

        this.store.Read(SaveKind.Resume);
        this.store.RemoveResume();

        Assert.True(this.store.Exists(SaveKind.Slot));
    }

    [Fact]
    public void AFailedReadOfTheResumeFileLeavesIt()
    {
        // The player can then send the file with a report, and no read destroys evidence (T-2).
        this.store.Write(SaveKind.Resume, SaveRuns.SaveAfter(20));
        string path = this.store.PathOf(SaveKind.Resume);
        File.WriteAllText(path, "{}\n");

        Assert.Throws<SaveException>(() => this.store.Read(SaveKind.Resume));

        Assert.True(this.store.Exists(SaveKind.Resume));
    }

    [Fact]
    public void AReadOfAnAbsentSaveNamesThePath()
    {
        StorageException error = Assert.Throws<StorageException>(() => this.store.Read(SaveKind.Slot));

        Assert.Equal(this.store.PathOf(SaveKind.Slot), error.Path);
        Assert.Contains("the slot save", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheStoreOfThisSystemSitsInTheFolderOfThePerson() =>
        Assert.Equal(SaveFolder.SavesOfThisSystem(), SaveStore.OfThisSystem().Folder);
}
