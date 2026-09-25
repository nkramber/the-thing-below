using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Core;
using TheThingBelow.Core.Crashes;
using TheThingBelow.Core.Runs;
using TheThingBelow.Storage;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The crash files: the folder, the name from the time, the content of the file, and the count
/// of files that the folder keeps (D-170, D-658, D-659).
/// </summary>
/// <remarks>
/// Every test writes to a folder of its own under the temporary folder of the machine, and no
/// test writes the folder of the person (D-465).
/// </remarks>
public sealed class CrashStoreTests : IDisposable
{
    /// <summary>The seed of the runs of these tests.</summary>
    private const ulong Seed = 0x00000000000c4a54;

    /// <summary>The time of the crash of these tests.</summary>
    private static readonly DateTime Moment = new(2026, 9, 18, 1, 42, 53, DateTimeKind.Utc);

    private readonly string folder;
    private readonly CrashStore store;

    public CrashStoreTests()
    {
        this.folder = Path.Combine(Path.GetTempPath(), "the-thing-below-tests", Guid.NewGuid().ToString("n"));
        this.store = new CrashStore(Path.Combine(this.folder, CrashStore.FolderName));
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
    public void TheCrashesFolderSitsInTheFolderOfTheGame()
    {
        // The crash files take their own folder beside the saves folder (D-656, D-658).
        Assert.Equal("crashes", CrashStore.FolderName);
        Assert.Equal(
            Path.Combine(SaveFolder.OfThisSystem(), CrashStore.FolderName),
            CrashStore.OfThisSystem().Folder);
    }

    [Fact]
    public void AWriteRemovesTheTemporaryFileOfATornWrite()
    {
        // A torn safe write leaves its temporary file, and the name pattern of the folder
        // never matches one, so the next write removes it (D-178).
        Directory.CreateDirectory(this.store.Folder);
        string torn = Path.Combine(this.store.Folder, "crash-20260918-014200.json" + SafeWrite.TemporarySuffix);
        File.WriteAllText(torn, "{");

        this.store.Write(CoreFault(), null, Moment);

        Assert.False(File.Exists(torn));
    }

    [Fact]
    public void AWriteWithAClockBehindTheOlderFilesKeepsItsOwnFile()
    {
        for (int minute = 1; minute <= CrashStore.KeepCount; minute += 1)
        {
            this.store.Write(CoreFault(), null, Moment.AddMinutes(minute));
        }

        string path = this.store.Write(CoreFault(), null, Moment);

        Assert.True(File.Exists(path));
        Assert.Equal(CrashStore.KeepCount + 1, this.store.Names().Count);
    }

    [Fact]
    public void AWriteMakesTheFolderAndTheNameOfTheTime()
    {
        Assert.False(Directory.Exists(this.store.Folder));

        string path = this.store.Write(CoreFault(), null, Moment);

        Assert.Equal("crash-20260918-014253.json", Path.GetFileName(path));
        Assert.True(File.Exists(path));
    }

    [Fact]
    public void ACrashOfCoreWritesAFileThatHoldsItsContext()
    {
        // Exit test 1 of section 7.15: an error in Core writes a crash file that holds its
        // context (D-170, T-2).
        SimulationException fault = CoreFault();

        CrashReport read = this.store.Read(this.store.Write(fault, Record(), Moment));

        Assert.Equal(CrashFormat.Current, read.FormatVersion);
        Assert.Equal("2026-09-18T01:42:53Z", read.Time);
        Assert.Equal(nameof(SimulationException), read.ErrorType);
        Assert.Contains($"seed {Seed}", read.Error, StringComparison.Ordinal);
        Assert.Equal(SimulationVersion.Current, read.SimulationVersion);
        Assert.Equal(GameVersion.Current, read.GameVersion);
        Assert.NotNull(read.Record);
    }

    [Fact]
    public void AFailedAssertionWritesTheSameFile()
    {
        // Exit test 2 of section 7.15: a failed assertion writes the same file (T-2).
        SimulationException fault = Assert.Throws<SimulationException>(
            () => CoreAssert.That(false, "the party holds one member", new RunContext(Seed, 12, "party")));

        CrashReport read = this.store.Read(this.store.Write(fault, Record(), Moment));

        Assert.Equal(nameof(SimulationException), read.ErrorType);
        Assert.Contains("assertion failed", read.Error, StringComparison.Ordinal);
        Assert.Contains("tick 12", read.Error, StringComparison.Ordinal);
        Assert.NotNull(read.Record);
    }

    [Fact]
    public void TheFileHoldsNoFolderOfThePerson()
    {
        // Exit test 3 of section 7.15: the crash file holds no personal data (D-170). A file
        // error carries its path, and that path names the account of the person.
        StorageException fault = StorageException.ForPath(
            Path.Combine(SaveFolder.OfThisSystem(), "saves", "slot.json"),
            "the game could not write the file of a save");

        string path = this.store.Write(fault, Record(), Moment);
        string text = File.ReadAllText(path);
        CrashReport read = this.store.Read(path);

        Assert.Contains(PersonalPaths.Placeholder, read.Error, StringComparison.Ordinal);
        foreach (string personal in PersonalPaths.FoldersOfThisSystem())
        {
            // The text of the file and each field that a reader reads hold no folder of the
            // person. Windows writes a backslash, and the JSON text escapes it, so the read
            // fields carry the check of that system (D-170).
            Assert.DoesNotContain(personal, text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(personal, read.Error, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(personal, read.Stack, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void TheRecordOfTheFileReachesTheStateHashOfTheRun()
    {
        // Exit test 4 of section 7.15: a test loads the record of a crash file and reaches the
        // same state hash (G-5).
        (Simulation run, RunRecorder recorder) = RunScripts.Play(
            Seed, SaveRuns.ContentHash, RunScripts.Make(Seed, 300), saveEvery: 0);

        CrashReport read = this.store.Read(this.store.Write(CoreFault(), recorder.Build(), Moment));

        Assert.Equal(
            run.StateHash(),
            RunReplay.Play(read.Record!, SaveRuns.ContentHash, TestMaps.Room, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None).StateHash());
    }

    [Fact]
    public void ACrashWithNoRunWritesAFileWithNoRecord()
    {
        // The crash path runs with no content loaded, because a load failure can start it.
        CrashReport read = this.store.Read(
            this.store.Write(new InvalidOperationException("the content is absent"), null, Moment));

        Assert.Null(read.Record);
        Assert.Equal(nameof(InvalidOperationException), read.ErrorType);
    }

    [Fact]
    public void TwoCrashesOfOneSecondTakeTwoFiles()
    {
        string first = this.store.Write(CoreFault(), null, Moment);
        string second = this.store.Write(CoreFault(), null, Moment);

        Assert.NotEqual(first, second);
        Assert.Equal("crash-20260918-014253-2.json", Path.GetFileName(second));
        Assert.True(File.Exists(first));
    }

    [Fact]
    public void TheFolderKeepsTheNewestFilesAlone()
    {
        // The game removes every file after the newest ten (D-659).
        for (int minute = 0; minute < CrashStore.KeepCount + 4; minute += 1)
        {
            this.store.Write(CoreFault(), null, Moment.AddMinutes(minute));
        }

        IReadOnlyList<string> names = this.store.Names();

        Assert.Equal(CrashStore.KeepCount, names.Count);
        Assert.Equal("crash-20260918-015553.json", names[0]);
        Assert.Equal("crash-20260918-014653.json", names[^1]);
        Assert.Null(this.store.CleanupFault);
    }

    [Fact]
    public void ARemovalThatFailsKeepsTheCrashFileAndReportsTheFault()
    {
        // Finding P3-3 of the repository review: a removal of an older file that failed made the
        // write throw after the crash file existed, and the crash screen then named no file. The
        // write returns its path, and the fault of the removal goes to the caller (D-659, T-2).
        CrashStore locked = new(Path.Combine(this.folder, CrashStore.FolderName), path => throw new IOException($"the file '{path}' is open"));
        for (int minute = 0; minute < CrashStore.KeepCount; minute += 1)
        {
            locked.Write(CoreFault(), null, Moment.AddMinutes(minute));
            Assert.Null(locked.CleanupFault);
        }

        string written = locked.Write(CoreFault(), null, Moment.AddMinutes(CrashStore.KeepCount));

        Assert.True(File.Exists(written));
        StorageException cleanup = Assert.IsType<StorageException>(locked.CleanupFault);
        Assert.EndsWith("crash-20260918-014253.json", cleanup.Path, StringComparison.Ordinal);
        Assert.Contains("D-659", cleanup.Message, StringComparison.Ordinal);
        Assert.Equal(CrashStore.KeepCount + 1, locked.Names().Count);
    }

    [Fact]
    public void ARemovalThatFailsStillRemovesTheTemporaryFileOfATornWrite()
    {
        // A finding of the Gitar pass on PR #80: both cleanups shared one try block, so a locked old
        // file also kept the temporary file of a torn write on every later crash (D-178, D-659).
        CrashStore locked = new(Path.Combine(this.folder, CrashStore.FolderName), path => throw new IOException($"the file '{path}' is open"));
        for (int minute = 0; minute < CrashStore.KeepCount; minute += 1)
        {
            locked.Write(CoreFault(), null, Moment.AddMinutes(minute));
        }

        string torn = Path.Combine(locked.Folder, "crash-20260918-014200.json" + SafeWrite.TemporarySuffix);
        File.WriteAllText(torn, "{");

        locked.Write(CoreFault(), null, Moment.AddMinutes(CrashStore.KeepCount));

        Assert.False(File.Exists(torn));
        Assert.EndsWith("crash-20260918-014253.json", Assert.IsType<StorageException>(locked.CleanupFault).Path, StringComparison.Ordinal);
    }

    [Fact]
    public void ANamesCallOfAnAbsentFolderGivesNoName()
    {
        Assert.Empty(this.store.Names());
    }

    [Fact]
    public void AReadOfAnAbsentFileIsAnError()
    {
        string path = Path.Combine(this.store.Folder, "crash-20260918-014253.json");

        StorageException error = Assert.Throws<StorageException>(() => this.store.Read(path));

        Assert.Contains(path, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AReadOfABrokenFileNamesTheFile()
    {
        string path = this.store.Write(CoreFault(), null, Moment);
        File.WriteAllText(path, "{\"format\":1}\n");

        CrashException error = Assert.Throws<CrashException>(() => this.store.Read(path));

        Assert.Contains(path, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFolderOfNoCharacterIsAnError()
    {
        Assert.Throws<ArgumentException>(() => new CrashStore(string.Empty));
    }

    [Fact]
    public void ATimeOfTheMachineIsAnError()
    {
        Assert.Throws<ArgumentException>(
            () => this.store.Write(CoreFault(), null, new DateTime(2026, 9, 18, 1, 42, 53, DateTimeKind.Local)));
    }

    [Fact]
    public void AnErrorThatIsNullIsAnError()
    {
        Assert.Throws<ArgumentNullException>(() => this.store.Write(null!, null, Moment));
    }

    private static SimulationException CoreFault() => Assert.Throws<SimulationException>(
        () => Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None).Step([Intent.OfPlayer(IntentIds.CloseMenu)]));

    private static RunRecord Record()
    {
        IReadOnlyList<IReadOnlyList<Intent>> script = RunScripts.Make(Seed, 120);
        return RunScripts.Play(Seed, SaveRuns.ContentHash, script, saveEvery: 0).Recorder.Build();
    }
}
