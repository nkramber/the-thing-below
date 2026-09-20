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

    private ScreenFacts(
        Vector2I window,
        Vector2I screen,
        float osScale,
        double fitFactor,
        double? mmPerPixel,
        List<string> notes)
    {
        WindowPixels = window;
        ScreenPixels = screen;
        OsScale = osScale;
        FitFactor = fitFactor;
        WholeFit = Math.Max(1, (int)Math.Floor(fitFactor + 0.0001));
        NearestSteps = Math.Max(1, (int)Math.Ceiling(fitFactor - 0.0001));
        MmPerPixel = mmPerPixel;
        Notes = notes;
    }

    /// <summary>The size of the window in the pixels that Godot draws.</summary>
    public Vector2I WindowPixels { get; }

    /// <summary>The size of the screen that the operating system reports.</summary>
    public Vector2I ScreenPixels { get; }

    /// <summary>The scale factor of the screen that the operating system reports.</summary>
    public float OsScale { get; }

    /// <summary>Device pixels for each frame pixel at the fit of D-573. It can be fractional.</summary>
    public double FitFactor { get; }

    /// <summary>The largest whole-number fit of the frame in the window (D-568).</summary>
    public int WholeFit { get; }

    /// <summary>The whole number of the first step of D-573. It is the fit factor, rounded up.</summary>
    public int NearestSteps { get; }

    /// <summary>Millimeters for each device pixel. It is absent when the run gives no diagonal.</summary>
    public double? MmPerPixel { get; }

    /// <summary>Each fact that limits the run, such as an absent diagonal or a window off the screen.</summary>
    public List<string> Notes { get; }

    /// <summary>True when the two fit modes put the same picture on the screen.</summary>
    public bool FitIsWhole => Math.Abs(FitFactor - WholeFit) < 0.0001;

    /// <summary>Reads the window and the screen, and computes the size of a device pixel.</summary>
    public static ScreenFacts Measure(ProbeOptions options)
    {
        Vector2I window = DisplayServer.WindowGetSize();
        int current = DisplayServer.WindowGetCurrentScreen();
        Vector2I screen = DisplayServer.ScreenGetSize(current);
        float osScale = DisplayServer.ScreenGetScale(current);

        var notes = new List<string>();
        double fitFactor = Math.Min((double)window.X / FrameWidth, (double)window.Y / FrameHeight);
        if (fitFactor < 1.0)
        {
            notes.Add($"The window is {window.X} by {window.Y}, which is smaller than the frame of "
                + $"{FrameWidth} by {FrameHeight}. The probe holds the fit at 1x, and the frame runs off the screen.");
            fitFactor = 1.0;
        }

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

        var facts = new ScreenFacts(window, screen, osScale, fitFactor, mmPerPixel, notes);
        if (!facts.FitIsWhole)
        {
            notes.Add($"The fit of this screen is {fitFactor:0.###}x, which is not a whole number. Mode whole "
                + $"draws the frame at {facts.WholeFit}x with wide bars, and mode fill draws the fit of D-573. "
                + "Press F or the right shoulder button to change the mode.");
        }

        return facts;
    }

    /// <summary>Device pixels for each frame pixel in one fit mode.</summary>
    public double Scale(FitMode mode) => mode == FitMode.Whole ? WholeFit : FitFactor;

    /// <summary>The height of a 32-pixel sprite on the screen, in millimeters.</summary>
    public double? SpriteMm(ScaleState state, FitMode mode) =>
        MmPerPixel * GridArt.TilePixels * Scale(mode) * state.WorldScale;

    /// <summary>The height of one line of body text on the screen, in millimeters.</summary>
    public double? GlyphMm(ScaleState state, FitMode mode) =>
        MmPerPixel * state.BodyPixels * Scale(mode);

    /// <summary>The height of one line of title text on the screen, in millimeters.</summary>
    public double? TitleMm(ScaleState state, FitMode mode) =>
        MmPerPixel * state.TitlePixels * Scale(mode);

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
    public static double TilesAcross(ScaleState state) => FrameWidth / (double)GridArt.TilePixels / state.WorldScale;

    /// <summary>The count of tiles down the frame at the world scale of a state.</summary>
    public static double TilesDown(ScaleState state) => FrameHeight / (double)GridArt.TilePixels / state.WorldScale;

    /// <summary>The count of characters that one line of the dialogue box holds at a body size.</summary>
    /// <remarks>The fit of the frame cancels, so the count is the same on every screen.</remarks>
    public static int DialogueColumns(ScaleState state) =>
        (FrameWidth - (28 * state.LayoutUnit)) / (state.BodyPixels / 2);

    /// <summary>The count of body characters that the whole width of the frame holds.</summary>
    public static int FrameColumns(ScaleState state) => FrameWidth / (state.BodyPixels / 2);

    /// <summary>The rectangle of the frame in the window, centered, with black bars around it.</summary>
    public Rect2 FrameRect(FitMode mode)
    {
        double scale = Scale(mode);
        var size = new Vector2((float)(FrameWidth * scale), (float)(FrameHeight * scale));
        var origin = new Vector2((WindowPixels.X - size.X) / 2.0f, (WindowPixels.Y - size.Y) / 2.0f);
        return new Rect2(origin, size);
    }
}
