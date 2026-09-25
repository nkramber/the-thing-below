using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Notices;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The rule that posts a notice, and the notice log of a run: which notices it keeps, how many,
/// and its place in the snapshot (D-221, D-983 to D-985, D-989).
/// </summary>
public sealed class NoticeLogTests
{
    private const ulong Seed = 20260923;

    [Fact]
    public void ANoticeThatLogsShowsAndEntersTheLog()
    {
        // Exit test 10 of PR-62 (D-983).
        Simulation run = Start(TestBattles.Notices);

        Post(run, TestBattles.KeptNotice);

        NoticeRecord shown = Assert.Single(run.TakeNotices());
        Assert.Equal(TestBattles.KeptNotice.Value, shown.Id.Value);
        Assert.Equal(TestBattles.KeptNotice.Value, Assert.Single(run.State.NoticeLog.Entries).Value);
    }

    [Fact]
    public void ANoticeThatDoesNotLogShowsAndStaysOutOfTheLog()
    {
        // Exit test 10 of PR-62 (D-983).
        Simulation run = Start(TestBattles.Notices);

        Post(run, TestBattles.PlainNotice);

        Assert.Equal(TestBattles.PlainNotice.Value, Assert.Single(run.TakeNotices()).Id.Value);
        Assert.Empty(run.State.NoticeLog.Entries);
    }

    [Fact]
    public void TheTakeEmptiesTheNoticesThatWaitForGame()
    {
        Simulation run = Start(TestBattles.Notices);
        Post(run, TestBattles.PlainNotice);

        _ = run.TakeNotices();

        Assert.Empty(run.TakeNotices());
    }

    [Fact]
    public void TheLogKeepsTheThirtyNewestEntries()
    {
        // Exit test 10 of PR-62 (D-984): the 31st entry removes the oldest one.
        NoticeList many = ManyNotices(NoticeLog.MostEntries + 1);
        Simulation run = Start(many);

        foreach (ContentId id in many.Ids)
        {
            Post(run, id);
        }

        IReadOnlyList<ContentId> entries = run.State.NoticeLog.Entries;
        Assert.Equal(NoticeLog.MostEntries, entries.Count);
        Assert.Equal("notice.test_1", entries[0].Value);
        Assert.Equal($"notice.test_{NoticeLog.MostEntries.ToString(CultureInfo.InvariantCulture)}", entries[^1].Value);
    }

    [Fact]
    public void AnAbsentNoticeIsAnErrorWithTheTickAndTheId()
    {
        Simulation run = Start(TestBattles.Notices);
        ContentId absent = ContentId.Parse("notice.test_absent", "test", "notice");

        SimulationException error = Assert.Throws<SimulationException>(() => Post(run, absent));

        Assert.Contains("notice.test_absent", error.Message, StringComparison.Ordinal);
        Assert.Contains("tick", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheLogSurvivesASnapshotAndAResume()
    {
        // Exit test 8 of PR-62 (D-985).
        Simulation run = Start(TestBattles.Notices);
        Post(run, TestBattles.KeptNotice);
        string line = RunSnapshotText.Write(run.Snapshot());

        var reader = new ContentReader(Encoding.UTF8.GetBytes(line), "the test");
        Simulation resumed = Simulation.Resume(Seed, RunSnapshotText.Read(ref reader), TestMaps.Room, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

        Assert.Equal(TestBattles.KeptNotice.Value, Assert.Single(resumed.State.NoticeLog.Entries).Value);
        Assert.Equal(run.StateHash(), resumed.StateHash());
        Assert.Contains("\"notices\":[\"notice.test_kept\"]", line, StringComparison.Ordinal);
    }

    [Fact]
    public void TheStateHashReadsTheLog()
    {
        // G-5: two runs that differ in the log alone give two hashes.
        Simulation plain = Start(TestBattles.Notices);
        Simulation kept = Start(TestBattles.Notices);
        Post(kept, TestBattles.KeptNotice);

        Assert.NotEqual(plain.StateHash(), kept.StateHash());
    }

    [Fact]
    public void AStoredLogOfANoticeThatDoesNotLogFailsTheResume()
    {
        // T-2: no run can put a notice that does not log into the log.
        Simulation run = Start(TestBattles.Notices);
        RunSnapshot broken = run.Snapshot() with { Notices = [TestBattles.PlainNotice] };

        ArgumentException error = Assert.Throws<ArgumentException>(
            () => Simulation.Resume(Seed, broken, TestMaps.Room, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None));

        Assert.Contains("notice.test_plain", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStoredLogPastThirtyEntriesFailsTheCheckOfTheSnapshot()
    {
        Simulation run = Start(TestBattles.Notices);
        List<ContentId> entries = [];
        for (int index = 0; index <= NoticeLog.MostEntries; index += 1)
        {
            entries.Add(TestBattles.KeptNotice);
        }

        RunSnapshot broken = run.Snapshot() with { Notices = entries };

        ArgumentException error = Assert.Throws<ArgumentException>(() => broken.Check("the test"));

        Assert.Contains("D-984", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASnapshotOfFormatSevenThatHoldsALogFailsTheRead()
    {
        // D-985: format 7 predates the log, so a log in it is a fault of the file.
        // The lessons of format 10 and the gear of format 11 come before the log, so the line drops them first (D-1018, D-1038).
        string line = SnapshotLines.AsFormatNine(RunSnapshotText.Write(Start(TestBattles.Notices).Snapshot()));
        var reader = new ContentReader(Encoding.UTF8.GetBytes(line), "the test");

        try
        {
            _ = RunSnapshotText.ReadFormatSeven(ref reader, Seed);
            Assert.Fail("The read of format 7 took a notice log.");
        }
        catch (ContentException error)
        {
            Assert.Contains("D-985", error.Message, StringComparison.Ordinal);
        }
    }

    private static Simulation Start(NoticeList notices) =>
        Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, notices, TestBattles.Story, DebugIntentHandlers.None);

    private static void Post(Simulation run, ContentId notice) =>
        NoticeRules.Post(run.State, notice, run.State.Context("the test"), []);

    private static NoticeList ManyNotices(int count)
    {
        StringBuilder text = new("{ \"comment\": \"many\", \"notices\": [");
        for (int index = 0; index < count; index += 1)
        {
            text.Append(index == 0 ? " " : ", ");
            text.Append(CultureInfo.InvariantCulture, $"{{ \"id\": \"notice.test_{index}\", \"log\": true }}");
        }

        text.Append(" ] }");
        return NoticeList.Read(Encoding.UTF8.GetBytes(text.ToString()), NoticeList.Path);
    }
}
