using System;
using System.Collections.Generic;
using System.Globalization;
using TheThingBelow.Core.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The lines of the notice log window: newest first, and a page that scrolls one line and stops
/// at each end (D-984, D-987). The tests read the built Game assembly (D-614).
/// </summary>
public sealed class LogPageTests
{
    [Fact]
    public void TheWindowListsTheEntriesNewestFirst()
    {
        GameValue page = GameValue.New("LogPage", Ids(3), 10);

        Assert.Equal(["notice.test_2", "notice.test_1", "notice.test_0"], Values(page.Call("Shown")!));
    }

    [Fact]
    public void ThePageScrollsOneLineAndStopsAtEachEnd()
    {
        GameValue page = GameValue.New("LogPage", Ids(5), 3);

        page.Call("Scroll", -1);
        Assert.Equal(0, page.Read<int>("First"));
        page.Call("Scroll", 1);
        page.Call("Scroll", 1);
        page.Call("Scroll", 1);
        Assert.Equal(2, page.Read<int>("First"));
        Assert.Equal(["notice.test_2", "notice.test_1", "notice.test_0"], Values(page.Call("Shown")!));
    }

    [Fact]
    public void AnEmptyLogShowsNoLineAndNeverScrolls()
    {
        GameValue page = GameValue.New("LogPage", Ids(0), 3);

        page.Call("Scroll", 1);

        Assert.Empty(Values(page.Call("Shown")!));
        Assert.Equal(0, page.Read<int>("First"));
    }

    [Fact]
    public void AWindowOfNoLineIsAnError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GameValue.New("LogPage", Ids(1), 0));
    }

    private static IReadOnlyList<ContentId> Ids(int count)
    {
        List<ContentId> ids = [];
        for (int index = 0; index < count; index += 1)
        {
            ids.Add(ContentId.Parse($"notice.test_{index.ToString(CultureInfo.InvariantCulture)}", "test", "notice"));
        }

        return ids;
    }

    private static List<string> Values(object shown)
    {
        List<string> values = [];
        foreach (ContentId id in (IReadOnlyList<ContentId>)shown)
        {
            values.Add(id.Value);
        }

        return values;
    }
}
