using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>The phase of a notice on screen (D-994).</summary>
public enum NoticePhase
{
    /// <summary>The notice slides in at the top edge.</summary>
    Slide,

    /// <summary>The line types out at the text speed (D-709, D-864).</summary>
    Type,

    /// <summary>The whole line holds.</summary>
    Hold,

    /// <summary>The notice fades out.</summary>
    Fade,
}

/// <summary>What the notice box shows of one notice at one tick of the world (D-994).</summary>
/// <param name="Notice">The id of the notice, which is the string id of its line (G-7).</param>
/// <param name="Phase">The phase.</param>
/// <param name="Hidden">The part of the notice box above the top edge, in thousandths: 1000 at the start of the slide, and 0 after it.</param>
/// <param name="Characters">The count of characters of the line that show.</param>
/// <param name="Opacity">The opacity, in thousandths: 1000 until the fade, and less inside it.</param>
public sealed record NoticeFrame(ContentId Notice, NoticePhase Phase, int Hidden, int Characters, int Opacity);

/// <summary>
/// The notices that wait for the notice box, and the phase of the one on screen (D-221, D-994).
/// </summary>
/// <remarks>
/// A notice counts the ticks of the world, so it stops while a menu pauses the world, and it
/// goes on after the close (D-995). The ticks of the world come from Core, so one run shows one
/// notice at one tick on every machine. The notice box reads no clock of the engine (T-7).
/// <para>
/// A second notice waits until the first one ends, and then it starts at once (D-994). Confirm
/// never skips a notice, so this type takes no input (D-996). It holds no Godot value, so a
/// test reads it with no engine (D-614).
/// </para>
/// </remarks>
public sealed class NoticeQueue
{
    /// <summary>The ticks of the world in one second (D-164).</summary>
    private const int TicksPerSecond = 60;

    private readonly NoticeTiming timing;
    private readonly Queue<(ContentId Notice, int Length)> waiting = new();
    private (ContentId Notice, int Length)? current;
    private long start;

    /// <summary>Makes an empty queue.</summary>
    /// <param name="timing">The time of each phase, from the UI style file (D-994).</param>
    /// <exception cref="ArgumentNullException">The timing is null (T-2).</exception>
    public NoticeQueue(NoticeTiming timing)
    {
        ArgumentNullException.ThrowIfNull(timing);

        this.timing = timing;
    }

    /// <summary>The count of notices that wait behind the one on screen.</summary>
    public int Waiting => this.waiting.Count;

    /// <summary>Adds one notice. It shows at once when no notice is on screen.</summary>
    /// <param name="notice">The id of the notice.</param>
    /// <param name="length">The count of characters of its line, which the type-out reads.</param>
    /// <param name="worldTick">The tick of the world now.</param>
    /// <exception cref="ArgumentNullException">The id is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The line holds no character (T-2).</exception>
    public void Add(ContentId notice, int length, long worldTick)
    {
        ArgumentNullException.ThrowIfNull(notice);
        if (length < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(length), length, $"The line of the notice '{notice.Value}' holds no character (T-2).");
        }

        if (this.current is null)
        {
            this.current = (notice, length);
            this.start = worldTick;
            return;
        }

        this.waiting.Enqueue((notice, length));
    }

    /// <summary>Gives what the notice box shows at one tick of the world, and starts each next notice whose turn came.</summary>
    /// <param name="worldTick">The tick of the world now, which never goes back.</param>
    /// <param name="charactersPerSecond">The text speed of the settings (D-864).</param>
    /// <returns>The frame of the notice on screen, or no value when none shows.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The speed is not above zero, or the tick is before the start of the notice on screen (T-2).</exception>
    public NoticeFrame? FrameAt(long worldTick, int charactersPerSecond)
    {
        if (charactersPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(charactersPerSecond), charactersPerSecond, "A text speed is above zero (D-864, T-2).");
        }

        while (this.current is (ContentId notice, int length))
        {
            if (worldTick < this.start)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(worldTick), worldTick, $"The notice '{notice.Value}' started at the world tick {this.start}, and the tick of the world never goes back (T-2).");
            }

            long elapsed = worldTick - this.start;
            long typeTicks = TypeTicks(length, charactersPerSecond);
            long total = this.timing.SlideTicks + typeTicks + this.timing.HoldTicks + this.timing.FadeTicks;
            if (elapsed < total)
            {
                return this.FrameOf(notice, length, elapsed, typeTicks, charactersPerSecond);
            }

            // The next notice starts on the tick where this one ended, so a long menu visit
            // never shows two at once (D-994).
            this.start += total;
            this.current = this.waiting.Count > 0 ? this.waiting.Dequeue() : null;
        }

        return null;
    }

    /// <summary>Gives the ticks that the type-out of a line takes: the ticks of its last character, at the least one.</summary>
    private static long TypeTicks(int length, int charactersPerSecond) =>
        Math.Max(1, (((long)length * TicksPerSecond) + charactersPerSecond - 1) / charactersPerSecond);

    private NoticeFrame FrameOf(ContentId notice, int length, long elapsed, long typeTicks, int charactersPerSecond)
    {
        if (elapsed < this.timing.SlideTicks)
        {
            int hidden = (int)((this.timing.SlideTicks - elapsed) * 1000 / this.timing.SlideTicks);
            return new NoticeFrame(notice, NoticePhase.Slide, hidden, 0, 1000);
        }

        long typed = elapsed - this.timing.SlideTicks;
        if (typed < typeTicks)
        {
            int shown = (int)Math.Min(length, typed * charactersPerSecond / TicksPerSecond);
            return new NoticeFrame(notice, NoticePhase.Type, 0, shown, 1000);
        }

        long held = typed - typeTicks;
        if (held < this.timing.HoldTicks)
        {
            return new NoticeFrame(notice, NoticePhase.Hold, 0, length, 1000);
        }

        long faded = held - this.timing.HoldTicks;
        int opacity = (int)(1000 - (faded * 1000 / this.timing.FadeTicks));
        return new NoticeFrame(notice, NoticePhase.Fade, 0, length, opacity);
    }
}
