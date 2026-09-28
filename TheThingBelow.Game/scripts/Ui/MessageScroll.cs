using System;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The scroll of a battle message of two lines in the message box of one line (D-1356, D-1359).
/// The box shows the first line, holds it, scrolls up by the height of one line, and then shows
/// the second line for the hold of its event.
/// </summary>
/// <remarks>
/// Game counts each tick of the scroll on its fixed-step clock, as it counts every effect, and no
/// timer or tween moves the text (D-266). The event of a message of two lines holds the extra
/// ticks of <see cref="ExtraTicksOf"/>, so the queue of the events waits for the whole scroll.
/// <para>
/// This type holds no Godot value, so a test reads it from the built Game assembly with no
/// engine (D-614).
/// </para>
/// </remarks>
public static class MessageScroll
{
    /// <summary>The ticks that the box holds the first line of a message of two lines before the scroll.</summary>
    public const int FirstLineTicks = 32;

    /// <summary>The ticks of the scroll from the first line to the second line.</summary>
    public const int ScrollTicks = 8;

    /// <summary>
    /// Gives the ticks that a message adds to the hold of its event: none for one line, and the
    /// hold of the first line and the scroll for two lines. The second line then takes the hold of
    /// the event.
    /// </summary>
    /// <param name="lines">The count of lines of the message, 1 or 2.</param>
    /// <returns>The extra ticks.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The count is not 1 or 2 (D-1356, T-2).</exception>
    public static int ExtraTicksOf(int lines) => lines switch
    {
        1 => 0,
        2 => FirstLineTicks + ScrollTicks,
        _ => throw new ArgumentOutOfRangeException(nameof(lines), lines, "A battle message holds 1 or 2 lines (D-1356, T-2)."),
    };

    /// <summary>
    /// Gives how far the text of the box stands scrolled up at one tick of its event, in frame
    /// pixels: 0 while the first line shows, then a whole count of pixels on each tick of the
    /// scroll, then the height of one line.
    /// </summary>
    /// <param name="lines">The count of lines of the message, 1 or 2.</param>
    /// <param name="ticks">The ticks since the event started, from 0.</param>
    /// <param name="lineHeight">The height of one line of the box, in frame pixels, a multiple of <see cref="ScrollTicks"/>.</param>
    /// <returns>The offset, from 0 to the line height.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The count is not 1 or 2, the ticks are negative, or the height gives no whole pixels on each tick (T-2).</exception>
    public static int OffsetAt(int lines, int ticks, int lineHeight)
    {
        _ = ExtraTicksOf(lines);
        ArgumentOutOfRangeException.ThrowIfNegative(ticks);
        if (lineHeight <= 0 || lineHeight % ScrollTicks != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lineHeight), lineHeight, $"The line height of the message box moves no whole count of pixels on each of the {ScrollTicks} ticks of the scroll (D-1359, T-2).");
        }

        if (lines == 1 || ticks < FirstLineTicks)
        {
            return 0;
        }

        int scrolled = ticks - FirstLineTicks;
        return scrolled >= ScrollTicks ? lineHeight : scrolled * (lineHeight / ScrollTicks);
    }
}
