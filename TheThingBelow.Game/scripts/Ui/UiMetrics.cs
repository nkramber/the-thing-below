using System;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The widths of the text of the game in frame pixels. Terminus is monospace, and the
/// advance of every glyph is half the body size (D-263, F-69).
/// </summary>
/// <remarks>
/// The probe of D-621 measured both numbers on four screens on 2026-09-19. A body of 24 puts
/// 106 characters across the frame of 1280 pixels, and a body of 32 puts 80. The dialogue box
/// is narrower than the frame, and it holds 104 characters and 76 (D-635, M-8).
/// <para>
/// A panel sizes to hold its longest string at each body size, and a test proves it (D-241,
/// D-708). This type holds no Godot value, so that test reads it with no engine (D-614).
/// </para>
/// </remarks>
public static class UiMetrics
{
    /// <summary>The count that divides a body size to give the advance of one glyph.</summary>
    public const int AdvanceDivisor = 2;

    /// <summary>Gives the advance of one glyph, in frame pixels.</summary>
    /// <param name="bodySize">The body size, in frame pixels (D-707).</param>
    /// <returns>Half the body size.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The size is not above zero (T-2).</exception>
    public static int AdvanceOf(int bodySize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(bodySize, 1);

        return bodySize / AdvanceDivisor;
    }

    /// <summary>Gives the width of a line of text, in frame pixels.</summary>
    /// <param name="bodySize">The body size, in frame pixels.</param>
    /// <param name="characters">The count of characters of the line.</param>
    /// <returns>The width that the line needs.</returns>
    /// <exception cref="ArgumentOutOfRangeException">A count is below zero, or the size is not above zero (T-2).</exception>
    public static int WidthOf(int bodySize, int characters)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(characters);

        return checked(AdvanceOf(bodySize) * characters);
    }

    /// <summary>Gives the count of characters that one width holds.</summary>
    /// <param name="bodySize">The body size, in frame pixels.</param>
    /// <param name="width">The width of the place, in frame pixels.</param>
    /// <returns>The count of whole characters.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The width is below zero, or the size is not above zero (T-2).</exception>
    public static int CharactersAcross(int bodySize, int width)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(width);

        return width / AdvanceOf(bodySize);
    }
}
