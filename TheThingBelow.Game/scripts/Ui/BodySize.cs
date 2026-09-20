using System;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The body size of the text, which the player sets, and its default on each screen (D-707).
/// The setting takes two values, and the title is twice the body.
/// </summary>
/// <remarks>
/// The owner picked both values on four screens (M-8). The rule is one line and no table of
/// screens: a frame that draws at its own size takes the larger body, and every larger frame
/// takes the smaller one. Each body size draws its own bitmap strike of the font, so a glyph
/// pixel is always one frame pixel (D-230, D-710).
/// <para>
/// The two values and the title count come from the UI style file, so this class holds the
/// rule alone (D-527). The type holds no Godot value, so a test reads it with no engine
/// (D-614).
/// </para>
/// </remarks>
public static class BodySize
{
    /// <summary>Gives the body size that a screen starts with (D-707).</summary>
    /// <param name="drawnFrameHeight">
    /// The height of the drawn frame on the screen, in device pixels, which
    /// <see cref="ScreenFit.Height"/> gives.
    /// </param>
    /// <param name="smallBody">The smaller body size of the style file, in frame pixels.</param>
    /// <param name="largeBody">The larger body size of the style file, in frame pixels.</param>
    /// <returns>The larger body at a fit of 1x, and the smaller body above that fit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">A size is not above zero (T-2).</exception>
    public static int DefaultFor(int drawnFrameHeight, int smallBody, int largeBody)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(drawnFrameHeight, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(smallBody, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(largeBody, smallBody + 1);

        return drawnFrameHeight == ScreenFit.FrameHeight ? largeBody : smallBody;
    }
}
