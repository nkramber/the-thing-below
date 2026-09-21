using System;
using TheThingBelow.Tools.Png;

namespace TheThingBelow.Tools.Screens;

/// <summary>One pixel that a capture and its baseline do not share (D-172, F-19).</summary>
/// <param name="X">The pixel column, from the left edge.</param>
/// <param name="Y">The pixel row, from the top edge.</param>
/// <param name="Baseline">The channels of the baseline pixel, such as `12,34,56,255`.</param>
/// <param name="Capture">The channels of the captured pixel.</param>
public sealed record PixelDifference(int X, int Y, string Baseline, string Capture);

/// <summary>
/// The frame compare of the screen tests (D-172). It compares the decoded pixels of a capture
/// with its committed baseline, and never the bytes of the two files, because the compressed
/// bytes of a PNG depend on the encoder (F-19).
/// </summary>
public static class ScreenCompare
{
    /// <summary>Counts the pixels that a capture and its baseline do not share.</summary>
    /// <param name="baseline">The committed baseline image.</param>
    /// <param name="capture">The image that the capture session wrote.</param>
    /// <param name="first">The first pixel that differs, in row order, or null when none does.</param>
    /// <returns>The count of pixels that differ.</returns>
    /// <exception cref="ArgumentNullException">An image is null (T-2).</exception>
    /// <exception cref="ArgumentException">The two images hold another size or another color kind (T-2).</exception>
    public static int Differences(PngImage baseline, PngImage capture, out PixelDifference? first)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(capture);

        if (baseline.Width != capture.Width || baseline.Height != capture.Height)
        {
            throw new ArgumentException(
                $"The baseline is {baseline.Width} by {baseline.Height} pixels, and the capture is " +
                $"{capture.Width} by {capture.Height} (T-2).",
                nameof(capture));
        }

        if (baseline.Colors != capture.Colors)
        {
            throw new ArgumentException(
                $"The baseline holds {baseline.Colors} pixels, and the capture holds {capture.Colors} (T-2).",
                nameof(capture));
        }

        int bytesPerPixel = baseline.BytesPerPixel;
        int count = 0;
        first = null;
        for (int y = 0; y < baseline.Height; y++)
        {
            ReadOnlySpan<byte> baselineRow = baseline.Row(y);
            ReadOnlySpan<byte> captureRow = capture.Row(y);
            if (baselineRow.SequenceEqual(captureRow))
            {
                continue;
            }

            for (int x = 0; x < baseline.Width; x++)
            {
                int at = x * bytesPerPixel;
                if (baselineRow.Slice(at, bytesPerPixel).SequenceEqual(captureRow.Slice(at, bytesPerPixel)))
                {
                    continue;
                }

                count++;
                first ??= new PixelDifference(
                    x,
                    y,
                    ChannelsOf(baselineRow.Slice(at, bytesPerPixel)),
                    ChannelsOf(captureRow.Slice(at, bytesPerPixel)));
            }
        }

        return count;
    }

    /// <summary>Writes the channels of one pixel as numbers, such as `12,34,56,255`.</summary>
    /// <param name="pixel">The bytes of one pixel.</param>
    /// <returns>The channels, separated by commas.</returns>
    private static string ChannelsOf(ReadOnlySpan<byte> pixel)
    {
        var text = new System.Text.StringBuilder();
        for (int channel = 0; channel < pixel.Length; channel++)
        {
            if (channel > 0)
            {
                text.Append(',');
            }

            text.Append(pixel[channel]);
        }

        return text.ToString();
    }
}
