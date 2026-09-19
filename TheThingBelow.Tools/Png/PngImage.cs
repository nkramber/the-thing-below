using System;

namespace TheThingBelow.Tools.Png;

/// <summary>
/// One decoded image: the size, the color kind, and the pixel bytes in the row order of the
/// PNG, with no filter byte and no padding (D-176).
/// </summary>
/// <remarks>
/// A test compares the pixels of this class and never the bytes of a PNG file, because the
/// compressed bytes depend on the encoder (F-19).
/// </remarks>
public sealed class PngImage
{
    /// <summary>The largest width and the largest height that the code reads or writes.</summary>
    /// <remarks>
    /// The PNG specification allows a size up to 2147483647, and a file of that size gives an
    /// allocation that no machine of this project can hold. The largest picture of the game
    /// covers the frame of 1280 by 720 (D-568), and an atlas of every sprite stays far below
    /// this limit. A file above it fails with the file and the reason (T-2).
    /// </remarks>
    public const int MaxSize = 65535;

    private readonly byte[] pixels;

    /// <summary>Makes an image from a copy of the pixel bytes.</summary>
    /// <param name="width">The count of pixels in one row, from 1 to <see cref="MaxSize"/>.</param>
    /// <param name="height">The count of rows, from 1 to <see cref="MaxSize"/>.</param>
    /// <param name="colors">The color kind of the pixels.</param>
    /// <param name="pixels">
    /// The pixel bytes, row after row, with `width * BytesPerPixelOf(colors)` bytes in each
    /// row. The constructor copies them, so a later change of the array reaches no image.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">A size is outside the limits.</exception>
    /// <exception cref="ArgumentException">The count of bytes does not match the size.</exception>
    public PngImage(int width, int height, PngColorKind colors, ReadOnlySpan<byte> pixels)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(width, MaxSize);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(height, MaxSize);

        // The product of two sizes at the limit passes the range of an int, so the count of
        // bytes takes long math. An image above the array limit thus fails here (T-2).
        int bytesPerPixel = BytesPerPixelOf(colors);
        long needed = (long)width * height * bytesPerPixel;
        if (pixels.Length != needed)
        {
            throw new ArgumentException(
                $"An image of {width} by {height} pixels with {colors} needs {needed} bytes, and the call gave {pixels.Length} (T-2).",
                nameof(pixels));
        }

        this.Width = width;
        this.Height = height;
        this.Colors = colors;
        this.pixels = pixels.ToArray();
    }

    /// <summary>The count of pixels in one row.</summary>
    public int Width { get; }

    /// <summary>The count of rows.</summary>
    public int Height { get; }

    /// <summary>The color kind of every pixel.</summary>
    public PngColorKind Colors { get; }

    /// <summary>The count of bytes of one pixel: 3 for RGB, and 4 for RGBA.</summary>
    public int BytesPerPixel => BytesPerPixelOf(this.Colors);

    /// <summary>The count of bytes of one row of pixels, with no filter byte.</summary>
    public int Stride => this.Width * this.BytesPerPixel;

    /// <summary>The pixel bytes, row after row.</summary>
    public ReadOnlySpan<byte> Pixels => this.pixels;

    /// <summary>Gives the count of bytes of one pixel of a color kind.</summary>
    /// <param name="colors">The color kind.</param>
    /// <returns>3 for RGB, and 4 for RGBA.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value is neither kind (T-2).</exception>
    public static int BytesPerPixelOf(PngColorKind colors) => colors switch
    {
        PngColorKind.Rgb => 3,
        PngColorKind.Rgba => 4,
        _ => throw new ArgumentOutOfRangeException(
            nameof(colors), colors, "The color kind is neither RGB nor RGBA (T-2)."),
    };

    /// <summary>Gives the pixel bytes of one row.</summary>
    /// <param name="y">The row, from 0 to `Height - 1`.</param>
    /// <returns>The bytes of the row, with no filter byte.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The row is outside the image.</exception>
    public ReadOnlySpan<byte> Row(int y)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(y, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(y, this.Height);

        return this.pixels.AsSpan(y * this.Stride, this.Stride);
    }
}
