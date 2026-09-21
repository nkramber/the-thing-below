using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Runs;
using TheThingBelow.Storage;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The log file of one session: the folder, the name from the time, one JSON object on each
/// line, the level filter, and the count of files that the folder keeps (D-179, D-658, D-659,
/// D-660).
/// </summary>
/// <remarks>
/// Every test writes to a folder of its own under the temporary folder of the machine, and no
/// test writes the folder of the person (D-465).
/// </remarks>
public sealed class LogStoreTests : IDisposable
{
    /// <summary>The seed of the runs of these tests.</summary>
    private const ulong Seed = 0x000000000000109f;

    /// <summary>The time of the start of the session of these tests.</summary>
    private static readonly DateTime Moment = new(2026, 9, 18, 1, 42, 53, DateTimeKind.Utc);

    private readonly string folder;
    private readonly LogStore store;

    public LogStoreTests()
    {
        this.folder = Path.Combine(Path.GetTempPath(), "the-thing-below-tests", Guid.NewGuid().ToString("n"));
        this.store = new LogStore(Path.Combine(this.folder, LogStore.FolderName), LogLevel.Info);
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
    public void TheLogsFolderSitsInTheFolderOfTheGame()
    {
        // The log files take their own folder beside the saves folder (D-656, D-658).
        Assert.Equal("logs", LogStore.FolderName);
        Assert.Equal(
            Path.Combine(SaveFolder.OfThisSystem(), LogStore.FolderName),
            LogStore.OfThisSystem(LogLevel.Info).Folder);
    }

    [Fact]
    public void AnOpenMakesTheFolderAndTheFileOfTheSession()
    {
        Assert.False(Directory.Exists(this.store.Folder));

        string path = this.store.Open(Moment);

        Assert.Equal("session-20260918-014253.json", Path.GetFileName(path));
        Assert.Equal(path, this.store.SessionFile);
        Assert.Equal(string.Empty, File.ReadAllText(path));
    }

    [Fact]
    public void EachLineOfTheFileIsOneJsonObject()
    {
        // Exit test 5 of section 7.15: each log line parses as one JSON object (D-179).
        this.store.Open(Moment);
        this.store.Write(Entries(LogLevel.Info, LogLevel.Warning, LogLevel.Error), Moment);

        string[] lines = File.ReadAllText(this.store.SessionFile).TrimEnd('\n').Split('\n');

        Assert.Equal(3, lines.Length);
        foreach (string line in lines)
        {
            using JsonDocument document = JsonDocument.Parse(line);
            Assert.Equal(JsonValueKind.Object, document.RootElement.ValueKind);
        }
    }

    [Fact]
    public void EveryLineCarriesTheTimeOfTheHost()
    {
        this.store.Open(Moment);
        this.store.Write(Entries(LogLevel.Info), Moment.AddSeconds(7));

        Assert.Equal("2026-09-18T01:43:00Z", Assert.Single(this.store.Read()).Time);
    }

    [Fact]
    public void AWriteAddsItsLinesAfterTheLinesBeforeIt()
    {
        this.store.Open(Moment);
        this.store.Write(Entries(LogLevel.Info), Moment);
        this.store.Write(Entries(LogLevel.Warning), Moment);

        IReadOnlyList<LogLine> lines = this.store.Read();

        Assert.Equal(2, lines.Count);
        Assert.Equal(LogLevel.Info, lines[0].Entry.Level);
        Assert.Equal(LogLevel.Warning, lines[1].Entry.Level);
    }

    [Fact]
    public void AnEntryBelowTheMinimumTakesNoLine()
    {
        // The file of a player holds the changes that a report follows (D-660).
        this.store.Open(Moment);

        Assert.Equal(0, this.store.Write(Entries(LogLevel.Debug), Moment));
        Assert.Empty(this.store.Read());
    }

    [Fact]
    public void AStoreOfTheDebugLevelHoldsEveryEntry()
    {
        LogStore debug = new(Path.Combine(this.folder, LogStore.FolderName), LogLevel.Debug);
        debug.Open(Moment);

        Assert.Equal(2, debug.Write(Entries(LogLevel.Debug, LogLevel.Error), Moment));
    }

    [Fact]
    public void TheLinesOfARunReachTheFile()
    {
        // The entries of a step of Core reach the file with no change of their order (D-179).
        this.store.Open(Moment);
        LogStore debug = new(Path.Combine(this.folder, "debug-logs"), LogLevel.Debug);
        debug.Open(Moment);

        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, DebugIntentHandlers.None);
        foreach (IReadOnlyList<Intent> intents in RunScripts.Make(Seed, 200))
        {
            IReadOnlyList<LogEntry> entries = run.Step(intents);
            this.store.Write(entries, Moment);
            debug.Write(entries, Moment);
        }

        IReadOnlyList<LogLine> info = this.store.Read();
        IReadOnlyList<LogLine> all = debug.Read();
        Assert.NotEmpty(info);
        Assert.True(all.Count > info.Count);
        foreach (LogLine line in info)
        {
            Assert.Equal(LogSubsystems.Run, line.Entry.Subsystem);
        }

        long tick = 0;
        foreach (LogLine line in all)
        {
            Assert.True(line.Entry.Tick >= tick);
            tick = line.Entry.Tick;
        }
    }

    [Fact]
    public void TheFolderKeepsTheNewestFilesAlone()
    {
        // The game removes every file after the newest ten (D-659).
        for (int minute = 0; minute < LogStore.KeepCount + 4; minute += 1)
        {
            LogStore session = new(Path.Combine(this.folder, LogStore.FolderName), LogLevel.Info);
            session.Open(Moment.AddMinutes(minute));
        }

        IReadOnlyList<string> names = this.store.Names();

        Assert.Equal(LogStore.KeepCount, names.Count);
        Assert.Equal("session-20260918-015553.json", names[0]);
        Assert.Equal("session-20260918-014653.json", names[^1]);
    }

    [Fact]
    public void AnOpenWithAClockBehindTheOlderFilesKeepsItsOwnFile()
    {
        // A clock that runs behind the stamps of the older files, after a correction of the
        // time, never makes a session remove its own file (T-2).
        for (int minute = 1; minute <= LogStore.KeepCount; minute += 1)
        {
            LogStore later = new(Path.Combine(this.folder, LogStore.FolderName), LogLevel.Info);
            later.Open(Moment.AddMinutes(minute));
        }

        string path = this.store.Open(Moment);

        Assert.True(File.Exists(path));
        Assert.Equal(LogStore.KeepCount + 1, this.store.Names().Count);
    }

    [Fact]
    public void TwoSessionsOfOneSecondTakeTwoFiles()
    {
        LogStore second = new(Path.Combine(this.folder, LogStore.FolderName), LogLevel.Info);

        this.store.Open(Moment);

        Assert.Equal("session-20260918-014253-2.json", Path.GetFileName(second.Open(Moment)));
    }

    [Fact]
    public void AWriteWithNoOpenFileIsAnError()
    {
        StorageException error = Assert.Throws<StorageException>(() => this.store.Write(Entries(LogLevel.Info), Moment));

        Assert.Contains("opened no log file", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASecondOpenOfOneSessionIsAnError()
    {
        this.store.Open(Moment);

        StorageException error = Assert.Throws<StorageException>(() => this.store.Open(Moment));

        Assert.Contains("one session writes one file", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ALevelThatNoNameCarriesIsAnError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LogStore(this.folder, (LogLevel)9));
    }

    [Fact]
    public void AFolderOfNoCharacterIsAnError()
    {
        Assert.Throws<ArgumentException>(() => new LogStore(string.Empty, LogLevel.Info));
    }

    [Fact]
    public void ATimeOfTheMachineIsAnError()
    {
        Assert.Throws<ArgumentException>(
            () => this.store.Open(new DateTime(2026, 9, 18, 1, 42, 53, DateTimeKind.Local)));
    }

    [Fact]
    public void AListThatIsNullIsAnError()
    {
        this.store.Open(Moment);

        Assert.Throws<ArgumentNullException>(() => this.store.Write(null!, Moment));
    }

    private static IReadOnlyList<LogEntry> Entries(params LogLevel[] levels)
    {
        List<LogEntry> entries = [];
        foreach (LogLevel level in levels)
        {
            entries.Add(new LogEntry(
                level,
                $"the test wrote one entry of the {LogLineText.NameOf(level)} level",
                entries.Count,
                LogSubsystems.Game,
                [LogField.OfNumber("index", entries.Count)]));
        }

        return entries;
    }
}
