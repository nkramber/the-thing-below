using System;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Game.Ui;

/// <summary>The top-left corner of the view, in art pixels of the world viewport (D-634).</summary>
/// <param name="X">The pixel of the west edge of the view.</param>
/// <param name="Y">The pixel of the north edge of the view.</param>
public readonly record struct CameraPlace(int X, int Y);

/// <summary>
/// Where the view of the map sits (D-106, D-717). Game computes the position on each tick,
/// and it never reads the limits or the position smoothing of the Godot camera (F-52).
/// </summary>
/// <remarks>
/// The engine centers a map smaller than the view in one line of its source, and no page of
/// its docs states that. Its smoothing can also run more than once in a frame. Thus Game
/// holds the rule, one code path covers both cases, and a test reads the position with no
/// engine (D-717, F-52).
/// <para>
/// Every value is a whole art pixel of the world viewport, so no sprite of the map draws
/// between two pixels (D-715). The frame shows that viewport at a scale of 2, so a whole
/// pixel of it is a whole frame pixel too (D-634).
/// </para>
/// <para>
/// This type holds no Godot value, so a test reads it with no engine (D-614).
/// </para>
/// </remarks>
public static class MapCamera
{
    /// <summary>The width and the height of one tile, in art pixels (D-228).</summary>
    public const int TilePixels = 32;

    /// <summary>The count of parts of one tick that a slide reads between two ticks (D-820).</summary>
    public const int TickParts = 1000;

    /// <summary>Gives the place of the view for one state of the party (D-106).</summary>
    /// <param name="party">The party on its map, which Core stepped.</param>
    /// <param name="viewWidth">The width of the world viewport, in art pixels (D-634).</param>
    /// <param name="viewHeight">The height of the world viewport, in art pixels.</param>
    /// <param name="tickPart">The part of the next tick that the frame reached, from 0 to 999 (D-820).</param>
    /// <returns>The top-left corner of the view.</returns>
    /// <exception cref="ArgumentNullException">The party is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">A side of the view is below one (T-2).</exception>
    public static CameraPlace Of(MapState party, int viewWidth, int viewHeight, int tickPart)
    {
        ArgumentNullException.ThrowIfNull(party);
        ArgumentOutOfRangeException.ThrowIfLessThan(viewWidth, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(viewHeight, 1);

        return new CameraPlace(
            AxisOf(party.Map.Width * TilePixels, viewWidth, LeadX(party, tickPart)),
            AxisOf(party.Map.Height * TilePixels, viewHeight, LeadY(party, tickPart)));
    }

    /// <summary>
    /// Gives the pixel of the west edge of the lead, with the slide of the step that runs
    /// (D-203).
    /// </summary>
    /// <param name="party">The party on its map.</param>
    /// <param name="tickPart">The part of the next tick that the frame reached, from 0 to 999 (D-820).</param>
    /// <returns>The pixel, in art pixels of the world viewport.</returns>
    /// <exception cref="ArgumentNullException">The party is null (T-2).</exception>
    public static int LeadX(MapState party, int tickPart)
    {
        ArgumentNullException.ThrowIfNull(party);

        return SlideOf(party.LeadAt.X, AcrossOf(party.Stepping), party.StepTicks, MapRules.TicksPerStep, tickPart);
    }

    /// <summary>
    /// Gives the pixel of the north edge of the lead, with the slide of the step that runs
    /// (D-203).
    /// </summary>
    /// <param name="party">The party on its map.</param>
    /// <param name="tickPart">The part of the next tick that the frame reached, from 0 to 999 (D-820).</param>
    /// <returns>The pixel, in art pixels of the world viewport.</returns>
    /// <exception cref="ArgumentNullException">The party is null (T-2).</exception>
    public static int LeadY(MapState party, int tickPart)
    {
        ArgumentNullException.ThrowIfNull(party);

        return SlideOf(party.LeadAt.Y, DownOf(party.Stepping), party.StepTicks, MapRules.TicksPerStep, tickPart);
    }

    /// <summary>
    /// Gives the pixel of the west edge of one enemy, with the slide of the step that runs
    /// (D-203, D-742).
    /// </summary>
    /// <param name="patrol">The enemy on its map.</param>
    /// <param name="tickPart">The part of the next tick that the frame reached, from 0 to 999 (D-820).</param>
    /// <returns>The pixel, in art pixels of the world viewport.</returns>
    /// <exception cref="ArgumentNullException">The enemy is null (T-2).</exception>
    /// <remarks>
    /// The pixel belongs to the anchor tile of the body, which is its north-west tile
    /// (D-737). Each enemy carries the count of ticks of its own step, so the slide reads
    /// that count and not the count of the party (D-742).
    /// </remarks>
    public static int EnemyX(PatrolState patrol, int tickPart)
    {
        ArgumentNullException.ThrowIfNull(patrol);

        return SlideOf(patrol.At.X, AcrossOf(patrol.Stepping), patrol.StepTicks, patrol.Patrol.StepTicks, tickPart);
    }

    /// <summary>
    /// Gives the pixel of the north edge of one enemy, with the slide of the step that runs
    /// (D-203, D-742).
    /// </summary>
    /// <param name="patrol">The enemy on its map.</param>
    /// <param name="tickPart">The part of the next tick that the frame reached, from 0 to 999 (D-820).</param>
    /// <returns>The pixel, in art pixels of the world viewport.</returns>
    /// <exception cref="ArgumentNullException">The enemy is null (T-2).</exception>
    public static int EnemyY(PatrolState patrol, int tickPart)
    {
        ArgumentNullException.ThrowIfNull(patrol);

        return SlideOf(patrol.At.Y, DownOf(patrol.Stepping), patrol.StepTicks, patrol.Patrol.StepTicks, tickPart);
    }

    /// <summary>Gives the place of the view on one axis (D-717).</summary>
    /// <param name="mapPixels">The length of the map on that axis, in art pixels.</param>
    /// <param name="viewPixels">The length of the view on that axis, in art pixels.</param>
    /// <param name="leadPixels">The pixel of the leading edge of the lead on that axis.</param>
    /// <returns>The pixel of the near edge of the view.</returns>
    /// <remarks>
    /// A map larger than the view holds the view inside its edges, and a map smaller than
    /// the view sits centered, which gives a value below zero (F-52).
    /// </remarks>
    public static int AxisOf(int mapPixels, int viewPixels, int leadPixels)
    {
        if (mapPixels <= viewPixels)
        {
            // The map is smaller than the view. Both lengths are whole tiles or whole
            // viewport sides, so the halves below are whole pixels (D-634, D-715).
            return (mapPixels - viewPixels) / 2;
        }

        int wanted = leadPixels + (TilePixels / 2) - (viewPixels / 2);
        return Math.Clamp(wanted, 0, mapPixels - viewPixels);
    }

    /// <summary>Gives the pixel of one axis of a thing that slides between two tiles (D-203).</summary>
    /// <param name="tile">The tile of the thing on that axis, which Core holds (D-106).</param>
    /// <param name="step">The step on that axis: -1, 0, or 1.</param>
    /// <param name="stepTicks">The count of ticks of the step that runs.</param>
    /// <param name="ticksPerStep">The count of ticks of one whole step of this thing (D-742).</param>
    /// <param name="tickPart">The part of the next tick that the frame reached, from 0 to 999 (D-820).</param>
    /// <returns>The pixel of the near edge of the thing on that axis.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The count of ticks of one step is below one, or the part is outside 0 to 999 (T-2).
    /// </exception>
    /// <remarks>
    /// The slide reads the part of a tick, so a screen that shows one tick for two frames or
    /// for three frames still moves the sprite on each frame (D-820). A step always runs on to
    /// its end, so the slide never passes the next tile. The division rounds toward the tile
    /// of the thing on both sides, and each value is a whole art pixel (D-715).
    /// </remarks>
    public static int SlideOf(int tile, int step, int stepTicks, int ticksPerStep, int tickPart)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(ticksPerStep, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(tickPart);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(tickPart, TickParts);

        long reached = ((long)stepTicks * TickParts) + tickPart;
        int slide = (int)(TilePixels * reached / ((long)ticksPerStep * TickParts));
        return (tile * TilePixels) + (step * slide);
    }

    private static int AcrossOf(StepDirection? stepping) => stepping switch
    {
        StepDirection.East => 1,
        StepDirection.West => -1,
        _ => 0,
    };

    private static int DownOf(StepDirection? stepping) => stepping switch
    {
        StepDirection.South => 1,
        StepDirection.North => -1,
        _ => 0,
    };
}
