using System;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The share of each enemy that the dark leaves on the screen, in thousandths (D-1062). Core
/// decides the sight of the party in whole tiles, and this type draws each crossing of its edge
/// as a fade, so no enemy pops in or out.
/// </summary>
/// <remarks>
/// Three fades meet here:
/// <list type="bullet">
/// <item>The distance fade: an enemy fades out across one tile past the range, as the enemy or the party moves.</item>
/// <item>The wall fade: an enemy fades over <see cref="FadeTicks"/> ticks when a wall corner opens or closes the line between it and the lead.</item>
/// <item>The range fade: the range moves to its new value over <see cref="RangeFadeTicks"/> ticks when the party holds the torch out or puts it away, so the dark closes in and never snaps.</item>
/// </list>
/// Each fade counts the ticks of the run and never a clock, so one tick gives one picture in
/// each capture (T-7, D-172). This type holds no Godot value, so a test reads it with no engine
/// (D-614).
/// </remarks>
public sealed class SightFade
{
    /// <summary>The share of an enemy that draws in full.</summary>
    public const int Full = 1000;

    /// <summary>The ticks of a wall fade: a quarter second at 60 ticks (D-1062).</summary>
    public const int FadeTicks = 15;

    /// <summary>
    /// The ticks of a range fade: one second at 60 ticks (D-1062). The range moves 4 tiles, so a
    /// shorter fade would clear an enemy at the edge in a few ticks, which reads as a pop.
    /// </summary>
    public const int RangeFadeTicks = 60;

    private readonly bool[] clear;
    private readonly int[] shareAtChange;
    private readonly long[] changedAt;
    private int rangeFrom;
    private int rangeTo;
    private long rangeChangedAt;

    /// <summary>Starts the fades of one map, with each fade at rest.</summary>
    /// <param name="clear">For each enemy, true when no wall stands between it and the lead now.</param>
    /// <param name="rangePixels">The sight range of the party now, in art pixels.</param>
    /// <exception cref="ArgumentNullException">The list is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The range is below zero (T-2).</exception>
    /// <remarks>A map that the party enters shows its enemies at rest, so no fade runs at the entry.</remarks>
    public SightFade(bool[] clear, int rangePixels)
    {
        ArgumentNullException.ThrowIfNull(clear);
        ArgumentOutOfRangeException.ThrowIfNegative(rangePixels);

        this.clear = (bool[])clear.Clone();
        this.shareAtChange = new int[clear.Length];
        this.changedAt = new long[clear.Length];
        for (int index = 0; index < clear.Length; index += 1)
        {
            this.shareAtChange[index] = clear[index] ? Full : 0;
            this.changedAt[index] = long.MinValue;
        }

        this.rangeFrom = rangePixels;
        this.rangeTo = rangePixels;
        this.rangeChangedAt = long.MinValue;
    }

    /// <summary>Gives the share of one enemy at one tick, and records a change of its wall line.</summary>
    /// <param name="index">The index of the enemy, in the order of the map file.</param>
    /// <param name="clearNow">True when no wall stands between the enemy and the lead at this tick.</param>
    /// <param name="distance">The reach from the lead to the nearest pixel of the body, in art pixels.</param>
    /// <param name="tick">The tick of the run, which never goes back.</param>
    /// <returns>The share, from 0 to <see cref="Full"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index names no enemy, or the distance is below zero (T-2).</exception>
    public int ShareOf(int index, bool clearNow, int distance, long tick)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.clear.Length);
        ArgumentOutOfRangeException.ThrowIfNegative(distance);

        if (clearNow != this.clear[index])
        {
            // A change inside a fade starts from the share of that moment, so the enemy never jumps.
            this.shareAtChange[index] = this.WallShare(index, tick);
            this.changedAt[index] = tick;
            this.clear[index] = clearNow;
        }

        return this.WallShare(index, tick) * DistanceShare(distance, this.RangeAt(tick)) / Full;
    }

    /// <summary>Moves the range to a new value over <see cref="RangeFadeTicks"/> ticks (D-1063).</summary>
    /// <param name="rangePixels">The sight range of the party now, in art pixels.</param>
    /// <param name="tick">The tick of the run.</param>
    /// <exception cref="ArgumentOutOfRangeException">The range is below zero (T-2).</exception>
    public void MoveRange(int rangePixels, long tick)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(rangePixels);

        if (rangePixels == this.rangeTo)
        {
            return;
        }

        this.rangeFrom = this.RangeAt(tick);
        this.rangeTo = rangePixels;
        this.rangeChangedAt = tick;
    }

    /// <summary>Gives the share that the distance leaves: full inside the range, and none one tile past it.</summary>
    /// <param name="distance">The reach from the lead to the nearest pixel of the body, in art pixels.</param>
    /// <param name="rangePixels">The sight range of the party, in art pixels.</param>
    /// <returns>The share, from 0 to <see cref="Full"/>.</returns>
    public static int DistanceShare(int distance, int rangePixels)
    {
        int past = distance - rangePixels;
        if (past <= 0)
        {
            return Full;
        }

        return past >= MapCamera.TilePixels ? 0 : (MapCamera.TilePixels - past) * Full / MapCamera.TilePixels;
    }

    /// <summary>
    /// Gives the reach from the lead to the nearest pixel of a body, in art pixels: the larger of
    /// the two axis gaps, as the reach of Core counts tiles (D-716).
    /// </summary>
    /// <param name="leadX">The drawn x of the north-west pixel of the lead.</param>
    /// <param name="leadY">The drawn y of the north-west pixel of the lead.</param>
    /// <param name="bodyX">The drawn x of the north-west pixel of the body.</param>
    /// <param name="bodyY">The drawn y of the north-west pixel of the body.</param>
    /// <param name="side">The count of tiles on one side of the body (D-206).</param>
    /// <returns>The reach, from zero.</returns>
    public static int Reach(int leadX, int leadY, int bodyX, int bodyY, int side)
    {
        int across = Gap(leadX, bodyX, side);
        int down = Gap(leadY, bodyY, side);
        return Math.Max(across, down);
    }

    private static int Gap(int lead, int body, int side)
    {
        int far = body + ((side - 1) * MapCamera.TilePixels);
        if (lead < body)
        {
            return body - lead;
        }

        return lead > far ? lead - far : 0;
    }

    private int WallShare(int index, long tick)
    {
        long ticks = this.changedAt[index] == long.MinValue ? FadeTicks : Math.Min(FadeTicks, tick - this.changedAt[index]);
        int moved = (int)(ticks * Full / FadeTicks);
        return this.clear[index]
            ? Math.Min(Full, this.shareAtChange[index] + moved)
            : Math.Max(0, this.shareAtChange[index] - moved);
    }

    private int RangeAt(long tick)
    {
        if (this.rangeChangedAt == long.MinValue || tick - this.rangeChangedAt >= RangeFadeTicks)
        {
            return this.rangeTo;
        }

        long ticks = tick - this.rangeChangedAt;
        return this.rangeFrom + (int)((this.rangeTo - this.rangeFrom) * ticks / RangeFadeTicks);
    }
}
