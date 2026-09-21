using System;
using System.Collections.Generic;
using TheThingBelow.Tools.Png;

namespace TheThingBelow.Tools.Screens;

/// <summary>
/// The contact sheet of the screen tests (D-172, D-735). It stacks every capture of one
/// local run into one picture, which the owner reads with the real renderer of the machine.
/// </summary>
/// <remarks>
/// A sheet holds no text, so the PR description names each capture in order, as D-668 asks
/// for an art batch. The sheet scales each capture down by a whole number alone, and it
/// takes every n-th pixel, so no new color enters the picture and the math is integer
/// (D-502, T-1).
/// </remarks>
public static class ContactSheet
{
    /// <summary>The largest width of one capture on the sheet, in pixels.</summary>
    public const int MaxCaptureWidth = 1280;

    /// <summary>The count of pixels between two captures on the sheet.</summary>
    public const int GapPixels = 8;

    /// <summary>The red, green, and blue of the ground of the sheet, which shows the black bars.</summary>
    public const int GroundLevel = 96;

    /// <summary>Builds the sheet of every capture, in the order that the caller gives.</summary>
    /// <param name="captures">The decoded captures, in the order that they stack.</param>
    /// <returns>The sheet, with four channels for each pixel.</returns>
    /// <exception cref="ArgumentNullException">The list is null (T-2).</exception>
    /// <exception cref="ArgumentException">The list is empty (T-2).</exception>
    public static PngImage Build(IReadOnlyList<PngImage> captures)
    {
        ArgumentNullException.ThrowIfNull(captures);

        if (captures.Count == 0)
        {
            throw new ArgumentException("A contact sheet needs one capture or more (T-2).", nameof(captures));
        }

        var steps = new int[captures.Count];
        int width = 1;
        int height = 0;
        for (int at = 0; at < captures.Count; at++)
        {
            steps[at] = StepOf(captures[at].Width);
            width = Math.Max(width, captures[at].Width / steps[at]);
            height += captures[at].Height / steps[at];
            if (at > 0)
            {
                height += GapPixels;
            }
        }

        var sheet = new byte[checked(width * height * PngImage.BytesPerPixelOf(PngColorKind.Rgba))];
        for (int at = 0; at < sheet.Length; at += 4)
        {
            sheet[at] = GroundLevel;
            sheet[at + 1] = GroundLevel;
            sheet[at + 2] = GroundLevel;
            sheet[at + 3] = 255;
        }

        int top = 0;
        for (int at = 0; at < captures.Count; at++)
        {
            int drawnWidth = captures[at].Width / steps[at];
            Draw(sheet, width, captures[at], steps[at], (width - drawnWidth) / 2, top);
            top += (captures[at].Height / steps[at]) + GapPixels;
        }

        return new PngImage(width, height, PngColorKind.Rgba, sheet);
    }

    /// <summary>
    /// Gives the whole number that scales one capture down to <see cref="MaxCaptureWidth"/>
    /// or less.
    /// </summary>
    /// <param name="width">The width of the capture, in pixels.</param>
    /// <returns>1 for a capture that already fits, and a larger whole number for a wider one.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The width is not above zero (T-2).</exception>
    public static int StepOf(int width)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);

        int step = 1;
        while (width / step > MaxCaptureWidth)
        {
            step++;
        }

        return step;
    }

    /// <summary>Draws one capture into the sheet, and takes every n-th pixel of it.</summary>
    /// <param name="sheet">The pixel bytes of the sheet, with four channels for each pixel.</param>
    /// <param name="sheetWidth">The width of the sheet, in pixels.</param>
    /// <param name="capture">The capture to draw.</param>
    /// <param name="step">The whole number that scales the capture down.</param>
    /// <param name="left">The pixel column of the left edge of the drawn capture.</param>
    /// <param name="top">The pixel row of the top edge of the drawn capture.</param>
    private static void Draw(byte[] sheet, int sheetWidth, PngImage capture, int step, int left, int top)
    {
        int channels = capture.BytesPerPixel;
        int rows = capture.Height / step;
        int columns = capture.Width / step;
        for (int y = 0; y < rows; y++)
        {
            ReadOnlySpan<byte> row = capture.Row(y * step);
            int start = (((top + y) * sheetWidth) + left) * 4;
            for (int x = 0; x < columns; x++)
            {
                int read = x * step * channels;
                int write = start + (x * 4);
                sheet[write] = row[read];
                sheet[write + 1] = row[read + 1];
                sheet[write + 2] = row[read + 2];
                sheet[write + 3] = 255;
            }
        }
    }
}
