using System;
using TheThingBelow.Core.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The phases of a notice on screen: the slide, the type-out at the text speed, the hold, the
/// fade, and the queue of the next one (D-994, D-995). The tests read the built Game assembly (D-614).
/// </summary>
public sealed class NoticeQueueTests
{
    private static readonly NoticeTiming Timing = new(12, 180, 30);

    private static readonly ContentId First = ContentId.Parse("notice.test_first", "test", "notice");

    private static readonly ContentId Second = ContentId.Parse("notice.test_second", "test", "notice");

    [Theory]
    [InlineData(100, "Slide", 1000, 0, 1000)]
    [InlineData(106, "Slide", 500, 0, 1000)]
    [InlineData(112, "Type", 0, 0, 1000)]
    [InlineData(122, "Type", 0, 10, 1000)]
    [InlineData(152, "Hold", 0, 40, 1000)]
    [InlineData(331, "Hold", 0, 40, 1000)]
    [InlineData(332, "Fade", 0, 40, 1000)]
    [InlineData(347, "Fade", 0, 40, 500)]
    public void EachPhaseTakesItsTicks(long worldTick, string phase, int hidden, int characters, int opacity)
    {
        // D-994 at the normal speed of 60 characters a second, one character a tick: the slide of
        // 12 ticks, the type-out of 40 ticks, the hold of 180, and the fade of 30.
        GameValue queue = GameValue.New("NoticeQueue", Timing);
        queue.Call("Add", First, 40, 100L);

        object frame = queue.Call("FrameAt", worldTick, 60)!;

        Assert.Equal(phase, Read<object>(frame, "Phase").ToString());
        Assert.Equal(hidden, Read<int>(frame, "Hidden"));
        Assert.Equal(characters, Read<int>(frame, "Characters"));
        Assert.Equal(opacity, Read<int>(frame, "Opacity"));
    }

    [Theory]
    [InlineData(30, 20)]
    [InlineData(60, 40)]
    [InlineData(120, 80)]
    public void TheTypeOutFollowsTheTextSpeed(int speed, int charactersAfterFortyTicks)
    {
        // D-864: slow, normal, and fast take 30, 60, and 120 characters a second.
        GameValue queue = GameValue.New("NoticeQueue", Timing);
        queue.Call("Add", First, 200, 0L);

        object frame = queue.Call("FrameAt", 12L + 40, speed)!;

        Assert.Equal(charactersAfterFortyTicks, Read<int>(frame, "Characters"));
    }

    [Fact]
    public void ASecondNoticeWaitsAndStartsOnTheTickWhereTheFirstEnds()
    {
        GameValue queue = GameValue.New("NoticeQueue", Timing);
        queue.Call("Add", First, 10, 0L);
        queue.Call("Add", Second, 10, 5L);
        Assert.Equal(1, queue.Read<int>("Waiting"));

        // The first takes 12 + 10 + 180 + 30 ticks.
        object last = queue.Call("FrameAt", 231L, 60)!;
        object next = queue.Call("FrameAt", 232L, 60)!;

        Assert.Equal(First, Read<ContentId>(last, "Notice"));
        Assert.Equal(Second, Read<ContentId>(next, "Notice"));
        Assert.Equal(1000, Read<int>(next, "Hidden"));
        Assert.Null(queue.Call("FrameAt", 232L + 232, 60));
    }

    [Fact]
    public void AnEmptyQueueShowsNothing()
    {
        Assert.Null(GameValue.New("NoticeQueue", Timing).Call("FrameAt", 0L, 60));
    }

    [Fact]
    public void AWorldTickBeforeTheStartIsAnError()
    {
        // T-2: the tick of the world never goes back.
        GameValue queue = GameValue.New("NoticeQueue", Timing);
        queue.Call("Add", First, 10, 50L);

        Assert.Throws<ArgumentOutOfRangeException>(() => queue.Call("FrameAt", 49L, 60));
    }

    [Fact]
    public void AnEmptyLineIsAnError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GameValue.New("NoticeQueue", Timing).Call("Add", First, 0, 0L));
    }

    private static T Read<T>(object value, string name) => (T)value.GetType().GetProperty(name)!.GetValue(value)!;
}
