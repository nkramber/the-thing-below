using System;

namespace TheThingBelow.Game;

/// <summary>
/// The fixed-step clock of the host. It turns the time of a frame into a count of ticks, and
/// the game calls Core that many times (D-164, G-3).
/// </summary>
/// <remarks>
/// The loop lives in Game, because Core reads no clock and counts ticks alone (G-3, T-7). No
/// Godot timer, physics step, or navigation step feeds the simulation, so this class holds
/// the one clock of the simulation (D-100, G-23).
/// <para>
/// A frame that took a long time, such as a frame after a load or after the window of the
/// game came back, gives more ticks than <see cref="MaxTicksInOneFrame"/>. The loop then
/// runs that maximum and drops the rest of the time. A loop that runs every late tick falls
/// further behind on each frame, and it never catches up (F-50).
/// </para>
/// </remarks>
public sealed class FixedStepLoop
{
    /// <summary>The count of ticks in one second (D-164).</summary>
    public const int TicksPerSecond = 60;

    /// <summary>The highest count of ticks that one frame runs.</summary>
    public const int MaxTicksInOneFrame = 8;

    /// <summary>The time of one tick, in seconds.</summary>
    private const double SecondsOfOneTick = 1.0 / TicksPerSecond;

    private double pending;

    /// <summary>The count of ticks that the loop dropped, because a frame took too long.</summary>
    public long DroppedTicks { get; private set; }

    /// <summary>Adds the time of one frame, and gives the count of ticks to run now.</summary>
    /// <param name="seconds">The time of the frame, in seconds. It must not be below zero.</param>
    /// <returns>The count of ticks, from 0 to <see cref="MaxTicksInOneFrame"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The time is below zero (T-2).</exception>
    public int Advance(double seconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(seconds);

        this.pending += seconds;
        long whole = (long)(this.pending / SecondsOfOneTick);
        if (whole <= 0)
        {
            return 0;
        }

        if (whole > MaxTicksInOneFrame)
        {
            this.DroppedTicks += whole - MaxTicksInOneFrame;
            this.pending = 0;
            return MaxTicksInOneFrame;
        }

        this.pending -= whole * SecondsOfOneTick;
        return (int)whole;
    }
}
