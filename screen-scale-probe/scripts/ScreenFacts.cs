using System;
using System.Collections.Generic;
using Godot;

namespace ScreenScaleProbe;

/// <summary>What the probe knows about the screen, and the apparent sizes that come from it.</summary>
public sealed class ScreenFacts
{
    /// <summary>The frame of the game is 1280 by 720 art pixels (D-568).</summary>
    public const int FrameWidth = 1280;

    /// <summary>The height of the frame in art pixels.</summary>
    public const int FrameHeight = 720;

    /// <summary>The body font of D-263 draws one line in 16 art pixels.</summary>
    public const int FontPixels = 16;

    private ScreenFacts(Vector2I window, Vector2I screen, float osScale, int fit, double? mmPerPixel, List<string> notes)
    {
        WindowPixels = window;
        ScreenPixels = screen;
        OsScale = osScale;
        Fit = fit;
        MmPerPixel = mmPerPixel;
        Notes = notes;
    }

    /// <summary>The size of the window in the pixels that Godot draws.</summary>
    public Vector2I WindowPixels { get; }

    /// <summary>The size of the screen that the operating system reports.</summary>
    public Vector2I ScreenPixels { get; }

    /// <summary>The scale factor of the screen that the operating system reports.</summary>
    public float OsScale { get; }

    /// <summary>Device pixels for each art pixel of the frame, before the scale of the state.</summary>
    public int Fit { get; }

    /// <summary>Millimeters for each device pixel. It is absent when the run gives no diagonal.</summary>
    public double? MmPerPixel { get; }

    /// <summary>Each fact that limits the run, such as an absent diagonal or a window off the screen.</summary>
    public List<string> Notes { get; }

    /// <summary>Reads the window and the screen, and computes the size of a device pixel.</summary>
    public static ScreenFacts Measure(ProbeOptions options)
    {
        Vector2I window = DisplayServer.WindowGetSize();
        int current = DisplayServer.WindowGetCurrentScreen();
        Vector2I screen = DisplayServer.ScreenGetSize(current);
        float osScale = DisplayServer.ScreenGetScale(current);
        int fit = Math.Max(1, Math.Min(window.X / FrameWidth, window.Y / FrameHeight));

        var notes = new List<string>();
        double? mmPerPixel = null;
        if (options.DiagonalInches is double diagonal)
        {
            double pixels = Math.Sqrt((double)screen.X * screen.X + (double)screen.Y * screen.Y);
            mmPerPixel = diagonal * 25.4 / pixels;
        }
        else
        {
            notes.Add("No --diagonal flag: the run gives no size in millimeters and no apparent size.");
        }

        if (options.DistanceCm is null)
        {
            notes.Add("No --distance flag: the run gives no apparent size in arcminutes.");
        }

        if (window != screen)
        {
            notes.Add($"The window is {window.X} by {window.Y}, and the screen is {screen.X} by {screen.Y}. "
                + "The frame does not fill the screen, so exit test 4 of section 7.8 fails for this run.");
        }

        if (Math.Abs(osScale - 1.0f) > 0.01f)
        {
            notes.Add($"The operating system reports a screen scale of {osScale}. Read the native pixel check "
                + "in the corner of the frame, and confirm the pixel count above.");
        }

        return new ScreenFacts(window, screen, osScale, fit, mmPerPixel, notes);
    }

    /// <summary>The height of a 32-pixel sprite on the screen, in millimeters.</summary>
    public double? SpriteMm(ScaleState state) => MmPerPixel * GridArt.TilePixels * Fit * state.World;

    /// <summary>The height of one line of body text on the screen, in millimeters.</summary>
    public double? GlyphMm(ScaleState state) => MmPerPixel * FontPixels * Fit * state.Ui;

    /// <summary>The apparent size in arcminutes of a height in millimeters, at the distance of the run.</summary>
    public static double? Arcminutes(double? millimeters, double? distanceCm)
    {
        if (millimeters is not double size || distanceCm is not double distance)
        {
            return null;
        }

        double radians = 2.0 * Math.Atan(size / 2.0 / (distance * 10.0));
        return radians * 180.0 / Math.PI * 60.0;
    }

    /// <summary>The count of tiles across the frame at the world scale of a state.</summary>
    public static double TilesAcross(ScaleState state) => (double)FrameWidth / GridArt.TilePixels / state.World;

    /// <summary>The count of tiles down the frame at the world scale of a state.</summary>
    public static double TilesDown(ScaleState state) => (double)FrameHeight / GridArt.TilePixels / state.World;

    /// <summary>The rectangle of the frame in the window, centered, with black bars around it.</summary>
    public Rect2I FrameRect()
    {
        var size = new Vector2I(FrameWidth * Fit, FrameHeight * Fit);
        var origin = new Vector2I((WindowPixels.X - size.X) / 2, (WindowPixels.Y - size.Y) / 2);
        return new Rect2I(origin, size);
    }
}
