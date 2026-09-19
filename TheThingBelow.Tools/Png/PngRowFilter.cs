using System;

namespace TheThingBelow.Tools.Png;

/// <summary>
/// The five row filters of the PNG specification. The reader restores a row of any filter,
/// and the writer uses <see cref="None"/> alone (D-664).
/// </summary>
/// <remarks>
/// A PNG row starts with a filter byte, and the filter reads the bytes to the left and the
/// bytes above. Every image editor writes filtered rows, so the reader must handle each
/// filter for the PNG import of PR-51 to work. The definitions come from the PNG
/// specification of the W3C, `https://www.w3.org/TR/png-3/`, read 2026-09-14. Each addition
/// wraps to one byte on purpose, which is the definition of the filters.
/// </remarks>
public static class PngRowFilter
{
    /// <summary>Filter 0: the row holds the pixel bytes with no change.</summary>
    public const byte None = 0;

    /// <summary>Filter 1: each byte reads the byte one pixel to the left.</summary>
    public const byte Sub = 1;

    /// <summary>Filter 2: each byte reads the byte above it.</summary>
    public const byte Up = 2;

    /// <summary>Filter 3: each byte reads the mean of the left byte and the byte above.</summary>
    public const byte Average = 3;

    /// <summary>Filter 4: each byte reads the Paeth prediction of its three neighbors.</summary>
    public const byte Paeth = 4;

    /// <summary>
    /// Turns the filtered bytes of one row into the pixel bytes of that row, in place.
    /// </summary>
    /// <param name="filterType">The filter byte of the row, from 0 to 4.</param>
    /// <param name="row">The filtered bytes of the row. The method writes the pixels here.</param>
    /// <param name="previous">
    /// The pixel bytes of the row above, of the same length as <paramref name="row"/>. The
    /// first row of an image takes a span of zero bytes, as the specification states.
    /// </param>
    /// <param name="bytesPerPixel">3 for RGB, and 4 for RGBA.</param>
    /// <param name="file">The file that the row comes from, for the error (T-2).</param>
    /// <exception cref="PngException">The filter byte names no filter of the five.</exception>
    /// <exception cref="ArgumentException">The row above has another length.</exception>
    public static void Restore(
        byte filterType, Span<byte> row, ReadOnlySpan<byte> previous, int bytesPerPixel, string file)
    {
        ArgumentException.ThrowIfNullOrEmpty(file);
        ArgumentOutOfRangeException.ThrowIfLessThan(bytesPerPixel, 1);
        if (previous.Length != row.Length)
        {
            throw new ArgumentException(
                $"The row above holds {previous.Length} bytes, and the row holds {row.Length} (T-2).",
                nameof(previous));
        }

        if (filterType > Paeth)
        {
            throw PngException.For(file, $"the filter byte of a row is {filterType}, and the filters are 0 to 4");
        }

        for (int index = 0; index < row.Length; index += 1)
        {
            int left = index >= bytesPerPixel ? row[index - bytesPerPixel] : 0;
            int above = previous[index];
            int aboveLeft = index >= bytesPerPixel ? previous[index - bytesPerPixel] : 0;
            int prediction = filterType switch
            {
                None => 0,
                Sub => left,
                Up => above,
                Average => (left + above) / 2,
                _ => PaethPrediction(left, above, aboveLeft),
            };

            row[index] = (byte)((row[index] + prediction) & 0xff);
        }
    }

    /// <summary>
    /// Gives the Paeth prediction of the three neighbors: the byte to the left, the byte
    /// above, and the byte above and to the left.
    /// </summary>
    /// <param name="left">The byte one pixel to the left, or 0 at the start of a row.</param>
    /// <param name="above">The byte above, or 0 in the first row.</param>
    /// <param name="aboveLeft">The byte above and one pixel to the left, or 0 at either edge.</param>
    /// <returns>The neighbor that the prediction of the specification picks.</returns>
    public static int PaethPrediction(int left, int above, int aboveLeft)
    {
        int estimate = left + above - aboveLeft;
        int fromLeft = Math.Abs(estimate - left);
        int fromAbove = Math.Abs(estimate - above);
        int fromAboveLeft = Math.Abs(estimate - aboveLeft);
        if (fromLeft <= fromAbove && fromLeft <= fromAboveLeft)
        {
            return left;
        }

        return fromAbove <= fromAboveLeft ? above : aboveLeft;
    }
}
