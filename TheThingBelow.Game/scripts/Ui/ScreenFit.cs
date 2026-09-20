using System;

namespace TheThingBelow.Game.Ui;

/// <summary>The two ways that the frame fits a screen (D-232).</summary>
public enum FitMode
{
    /// <summary>
    /// The default. The frame grows to the full height or the full width of the screen, and
    /// bars fill the rest. A scale that is not a whole number takes the two steps of D-573.
    /// </summary>
    Fill,

    /// <summary>
    /// The display setting of D-232. The frame grows by a whole number alone, so every pixel
    /// of the frame covers the same square of device pixels, and the bars grow.
    /// </summary>
    WholePixels,
}

/// <summary>
/// Where the frame of 1280 by 720 draws on one screen, and how it gets there (D-568, D-573).
/// Every screen shape other than 16:9 shows black bars, the Steam Deck included (D-228).
/// </summary>
/// <remarks>
/// Godot has no stretch mode that scales up by a whole number and then scales down, so Game
/// builds both steps itself (F-48). <see cref="WholeStep"/> is the first step, with the
/// Nearest filter. <see cref="NeedsSmoothStep"/> tells whether the second step runs, which
/// scales that result down to <see cref="Width"/> by <see cref="Height"/> with a linear
/// filter.
/// <para>
/// Every number here is an integer, because a frame covers whole device pixels and a screen
/// has a whole count of them (T-1). The type holds no Godot value, so a test reads it with
/// no engine (D-614).
/// </para>
/// </remarks>
/// <param name="Width">The width of the drawn frame on the screen, in device pixels.</param>
/// <param name="Height">The height of the drawn frame on the screen, in device pixels.</param>
/// <param name="Left">The device pixel column of the left edge of the frame.</param>
/// <param name="Top">The device pixel row of the top edge of the frame.</param>
/// <param name="WholeStep">
/// The whole number of the first step. The frame draws at this scale with the Nearest
/// filter, which gives <see cref="StepWidth"/> by <see cref="StepHeight"/> pixels.
/// </param>
public sealed record ScreenFit(int Width, int Height, int Left, int Top, int WholeStep)
{
    /// <summary>The width of the frame, in frame pixels (D-568).</summary>
    public const int FrameWidth = 1280;

    /// <summary>The height of the frame, in frame pixels (D-568).</summary>
    public const int FrameHeight = 720;

    /// <summary>The width of the first step, in device pixels.</summary>
    public int StepWidth => this.WholeStep * FrameWidth;

    /// <summary>The height of the first step, in device pixels.</summary>
    public int StepHeight => this.WholeStep * FrameHeight;

    /// <summary>
    /// Tells whether the second step of D-573 runs. It runs when the first step overshoots
    /// the drawn size, such as on a screen of 1920 by 1080, where the frame needs a scale of
    /// 1.5 and the first step gives 2.
    /// </summary>
    public bool NeedsSmoothStep => this.StepHeight != this.Height;

    /// <summary>
    /// Tells whether the frame draws at its own size, one frame pixel to one device pixel.
    /// The Steam Deck at 1280 by 800 is this case, and it sets the default body size (D-707).
    /// </summary>
    public bool IsOneToOne => this.Height == FrameHeight;

    /// <summary>Gives the place of the frame on one screen.</summary>
    /// <param name="mode">The fit of the display setting (D-232).</param>
    /// <param name="screenWidth">The width of the screen, in device pixels.</param>
    /// <param name="screenHeight">The height of the screen, in device pixels.</param>
    /// <returns>The place of the frame, and the whole step that reaches it.</returns>
    /// <exception cref="ArgumentOutOfRangeException">A side of the screen is not above zero (T-2).</exception>
    public static ScreenFit Of(FitMode mode, int screenWidth, int screenHeight)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(screenWidth, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(screenHeight, 1);

        return mode == FitMode.WholePixels
            ? WholePixelFit(screenWidth, screenHeight)
            : FillFit(screenWidth, screenHeight);
    }

    /// <summary>
    /// The default fit. The frame keeps its 16 to 9 shape and grows until one side of it
    /// reaches the screen, so the other side gains bars (D-568).
    /// </summary>
    private static ScreenFit FillFit(int screenWidth, int screenHeight)
    {
        int width;
        int height;

        // The screen is wider than 16 to 9 when its width times 720 is more than its height
        // times 1280. The frame then reaches the full height, and the bars stand at the
        // sides. The products stay inside a long, because a screen side fits in an int.
        if ((long)screenWidth * FrameHeight > (long)screenHeight * FrameWidth)
        {
            height = screenHeight;
            width = (int)((long)screenHeight * FrameWidth / FrameHeight);
        }
        else
        {
            width = screenWidth;
            height = (int)((long)screenWidth * FrameHeight / FrameWidth);
        }

        return Place(width, height, screenWidth, screenHeight, SmallestStepFor(height));
    }

    /// <summary>
    /// The fit of the display setting. The frame grows by a whole number alone, and it never
    /// leaves the screen. A screen smaller than the frame still takes the scale of 1, so the
    /// frame loses its edges rather than its shape (D-232).
    /// </summary>
    private static ScreenFit WholePixelFit(int screenWidth, int screenHeight)
    {
        int across = screenWidth / FrameWidth;
        int down = screenHeight / FrameHeight;
        int step = Math.Max(1, Math.Min(across, down));

        return Place(step * FrameWidth, step * FrameHeight, screenWidth, screenHeight, step);
    }

    /// <summary>
    /// The smallest whole number whose frame reaches a drawn height. A height of 1080 takes
    /// 2, so the first step draws 1440 rows and the second step scales them down (D-573).
    /// </summary>
    private static int SmallestStepFor(int height)
    {
        int step = height / FrameHeight;
        if (step * FrameHeight < height)
        {
            step += 1;
        }

        return Math.Max(1, step);
    }

    private static ScreenFit Place(int width, int height, int screenWidth, int screenHeight, int step) =>
        new(width, height, (screenWidth - width) / 2, (screenHeight - height) / 2, step);
}
