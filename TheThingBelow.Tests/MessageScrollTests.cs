using System;
using System.Collections.Generic;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The scroll of a battle message of two lines in the message box of one line (D-1356, D-1359):
/// the first line holds, the text scrolls up by one line in whole pixels on each tick, and the
/// second line holds. The tests read the built Game assembly, because Tests takes no reference to
/// Game (D-614).
/// </summary>
public sealed class MessageScrollTests
{
    private const string Type = "MessageScroll";

    [Fact]
    public void AMessageOfOneLineNeverScrollsAndAddsNoTicks()
    {
        int height = InsideHeight();
        for (int tick = 0; tick <= 200; tick += 1)
        {
            Assert.Equal(0, OffsetAt(1, tick, height));
        }

        Assert.Equal(0, (int)GameValue.Static(Type, "ExtraTicksOf", 1)!);
    }

    [Fact]
    public void AMessageOfTwoLinesShowsTheFirstLineThenScrollsToTheSecondAtItsTicks()
    {
        // D-1359: the first line holds, then each tick of the scroll lifts the text by whole
        // pixels, and the second line holds from the end of the scroll.
        int first = FirstLineTicks();
        int scroll = ScrollTicks();
        int height = InsideHeight();
        List<int> offsets = [];
        for (int tick = 0; tick <= first + scroll + 4; tick += 1)
        {
            offsets.Add(OffsetAt(2, tick, height));
        }

        for (int tick = 0; tick < first; tick += 1)
        {
            Assert.Equal(0, offsets[tick]);
        }

        for (int step = 0; step < scroll; step += 1)
        {
            Assert.Equal(step * (height / scroll), offsets[first + step]);
        }

        for (int tick = first + scroll; tick < offsets.Count; tick += 1)
        {
            Assert.Equal(height, offsets[tick]);
        }

        Assert.Equal(first + scroll, (int)GameValue.Static(Type, "ExtraTicksOf", 2)!);
    }

    [Fact]
    public void TheBoxOfOneLineHoldsALineAtBothBodySizesAndScrollsByWholePixels()
    {
        // D-707, D-1359: the box keeps the height of one line at a body of 24 and of 32, and the
        // height of that line splits into whole pixels over the ticks of the scroll at 1x.
        int height = InsideHeight();

        Assert.Equal(32, height);
        Assert.Equal(0, height % ScrollTicks());
        foreach (int body in new[] { 24, 32 })
        {
            Assert.True(body <= height, $"At a body of {body}, the box of {height} pixels holds no line.");
        }
    }

    [Fact]
    public void ALineHeightWithNoWholePixelsOnEachTickOrAThirdLineIsAnError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GameValue.Static(Type, "OffsetAt", 2, 40, 30));
        Assert.Throws<ArgumentOutOfRangeException>(() => GameValue.Static(Type, "OffsetAt", 3, 40, 32));
        Assert.Throws<ArgumentOutOfRangeException>(() => GameValue.Static(Type, "ExtraTicksOf", 3));
    }

    [Fact]
    public void ALongLineOfTheCheckoutTakesTwoLinesAndAShortLineOne()
    {
        // D-1356: the longest enemy name with the cap wraps the absorb line into two lines.
        IReadOnlyList<string> two = (IReadOnlyList<string>)GameValue.Static("BattleMessages", "Wrap", "Starved boar B absorbs the hit. Regains 9999.")!;
        IReadOnlyList<string> one = (IReadOnlyList<string>)GameValue.Static("BattleMessages", "Wrap", "Marrek braces.")!;

        Assert.Equal(2, two.Count);
        Assert.Single(one);
    }

    private static int OffsetAt(int lines, int tick, int height) => (int)GameValue.Static(Type, "OffsetAt", lines, tick, height)!;

    private static int FirstLineTicks() => (int)GameValue.Constant(Type, "FirstLineTicks");

    private static int ScrollTicks() => (int)GameValue.Constant(Type, "ScrollTicks");

    /// <summary>The height inside the frame of the message box: its height less the edge on each side.</summary>
    private static int InsideHeight() =>
        (int)GameValue.Constant("BattleLayout", "LineHeight") - ((int)GameValue.Constant("BattleLayout", "PanelEdge") * 2);
}
