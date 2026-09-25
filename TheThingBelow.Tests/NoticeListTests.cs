using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Notices;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The reader of the notice file: the id of each notice and its log mark (D-983, D-989). Each error names the file (T-2).</summary>
public sealed class NoticeListTests
{
    [Fact]
    public void EachNoticeReadsItsIdAndItsLogMark()
    {
        NoticeList list = Read(TestBattles.NoticesFile);

        Assert.Equal(["notice.test_kept", "notice.test_plain", "notice.service_closed", "notice.rested", "notice.saved"], IdsOf(list));
        Assert.True(list.Notice(TestBattles.KeptNotice).Logs);
        Assert.False(list.Notice(TestBattles.PlainNotice).Logs);
    }

    [Fact]
    public void TheFirstNoticeOfEachMarkIsTheOneThatTheConsolePosts()
    {
        // D-989: the console posts the first notice of each kind, and names no content id.
        NoticeList list = Read(TestBattles.NoticesFile);

        Assert.Equal("notice.test_kept", list.FirstThatLogs(true).Id.Value);
        Assert.Equal("notice.test_plain", list.FirstThatLogs(false).Id.Value);
    }

    [Theory]
    [InlineData("{ \"id\": \"notice.test_kept\", \"log\": true }", "{ \"id\": \"notice.test_kept\" }", "log")]
    [InlineData("\"log\": true", "\"log\": true, \"color\": \"red\"", "color")]
    [InlineData("\"notice.test_plain\"", "\"notice.test_kept\"", "two times")]
    [InlineData("\"notice.test_kept\"", "\"label.test_kept\"", "notice")]
    public void AFaultOfTheFileNamesTheFile(string from, string to, string named)
    {
        // D-983 and G-6: the log mark is required, and an unknown field fails the load.
        string text = TestBattles.NoticesFile.Replace(from, to, StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => Read(text));

        Assert.Equal(NoticeList.Path, error.File);
        Assert.Contains(named, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFileWithNoNoticeFails()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => Read("{ \"comment\": \"none\", \"notices\": [] }"));

        Assert.Contains("no notice", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAbsentIdNamesTheFileAndTheId()
    {
        NoticeList list = Read(TestBattles.NoticesFile);

        ContentException error = Assert.Throws<ContentException>(
            () => list.Notice(ContentId.Parse("notice.test_absent", "test", "notice")));

        Assert.Equal(NoticeList.Path, error.File);
        Assert.Contains("notice.test_absent", error.Message, StringComparison.Ordinal);
        Assert.False(list.Holds(ContentId.Parse("notice.test_absent", "test", "notice")));
    }

    [Fact]
    public void AFileWithNoNoticeOfOneMarkFailsTheSearchOfThatMark()
    {
        NoticeList list = Read("{ \"comment\": \"one\", \"notices\": [ { \"id\": \"notice.test_kept\", \"log\": true } ] }");

        ContentException error = Assert.Throws<ContentException>(() => list.FirstThatLogs(false));

        Assert.Contains("does not log", error.Message, StringComparison.Ordinal);
    }

    private static NoticeList Read(string text) => NoticeList.Read(Encoding.UTF8.GetBytes(text), NoticeList.Path);

    private static List<string> IdsOf(NoticeList list)
    {
        List<string> ids = [];
        foreach (ContentId id in list.Ids)
        {
            ids.Add(id.Value);
        }

        return ids;
    }
}
