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

    /// <summary>Gives the place of the view for one state of the party (D-106).</summary>
    /// <param name="party">The party on its map, which Core stepped.</param>
    /// <param name="viewWidth">The width of the world viewport, in art pixels (D-634).</param>
    /// <param name="viewHeight">The height of the world viewport, in art pixels.</param>
    /// <returns>The top-left corner of the view.</returns>
    /// <exception cref="ArgumentNullException">The party is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">A side of the view is below one (T-2).</exception>
    public static CameraPlace Of(MapState party, int viewWidth, int viewHeight)
    {
        ArgumentNullException.ThrowIfNull(party);
        ArgumentOutOfRangeException.ThrowIfLessThan(viewWidth, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(viewHeight, 1);

        return new CameraPlace(
            AxisOf(party.Map.Width * TilePixels, viewWidth, LeadX(party)),
            AxisOf(party.Map.Height * TilePixels, viewHeight, LeadY(party)));
    }

    /// <summary>
    /// Gives the pixel of the west edge of the lead, with the slide of the step that runs
    /// (D-203).
    /// </summary>
    /// <param name="party">The party on its map.</param>
    /// <returns>The pixel, in art pixels of the world viewport.</returns>
    /// <exception cref="ArgumentNullException">The party is null (T-2).</exception>
    public static int LeadX(MapState party)
    {
        ArgumentNullException.ThrowIfNull(party);

        return SlideOf(party.LeadAt.X, AcrossOf(party.Stepping), party.StepTicks);
    }

    /// <summary>
    /// Gives the pixel of the north edge of the lead, with the slide of the step that runs
    /// (D-203).
    /// </summary>
    /// <param name="party">The party on its map.</param>
    /// <returns>The pixel, in art pixels of the world viewport.</returns>
    /// <exception cref="ArgumentNullException">The party is null (T-2).</exception>
    public static int LeadY(MapState party)
    {
        ArgumentNullException.ThrowIfNull(party);

        return SlideOf(party.LeadAt.Y, DownOf(party.Stepping), party.StepTicks);
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

    /// <summary>Gives the pixel of one axis of the lead, with the slide of its step (D-203).</summary>
    /// <param name="tile">The tile of the lead on that axis, which Core holds (D-106).</param>
    /// <param name="step">The step on that axis: -1, 0, or 1.</param>
    /// <param name="stepTicks">The count of ticks of the step that runs.</param>
    /// <returns>The pixel of the near edge of the lead on that axis.</returns>
    public static int SlideOf(int tile, int step, int stepTicks) =>
        (tile * TilePixels) + (step * TilePixels * stepTicks / MapRules.TicksPerStep);

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
