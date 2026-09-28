using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Png;

namespace TheThingBelow.Tools.Import;

/// <summary>The part of a picture that holds each opaque pixel.</summary>
/// <param name="Left">The column of the first opaque pixel from the left.</param>
/// <param name="Top">The row of the first opaque pixel from the top.</param>
/// <param name="Width">The count of columns from the first opaque column to the last.</param>
/// <param name="Height">The count of rows from the first opaque row to the last.</param>
public sealed record ContentBox(int Left, int Top, int Width, int Height);

/// <summary>The frame that the generator mode makes from one picture (D-688, D-689).</summary>
/// <param name="Rows">The rows of the new frame, one palette key for each pixel.</param>
/// <param name="Content">The part of the picture that the mode kept.</param>
/// <param name="FrameLeft">The column of the frame that takes the left edge of the content.</param>
/// <param name="FrameTop">The row of the frame that takes the top edge of the content.</param>
/// <param name="Mapped">The count of opaque pixels that took the nearest color (D-688).</param>
/// <param name="Opaque">The count of every opaque pixel of the content.</param>
public sealed record GeneratorFrame(
    IReadOnlyList<string> Rows,
    ContentBox Content,
    int FrameLeft,
    int FrameTop,
    int Mapped,
    int Opaque);

/// <summary>
/// Turns the pixels of a PNG into the rows of one frame of a drawing, in the hand-edit mode or
/// in the generator mode of the `import` command (D-688).
/// </summary>
/// <remarks>
/// Alpha 0 is a transparent pixel, alpha 255 is a color, and each other alpha fails in both
/// modes (D-1315). Neither mode scales a picture (D-689).
/// </remarks>
public static class FrameImport
{
    /// <summary>The small frame of the generator mode, in pixels (D-689, D-1310).</summary>
    public const int SmallFrame = 32;

    /// <summary>The large frame of the generator mode, in pixels (D-689, D-1310).</summary>
    public const int LargeFrame = 64;

    /// <summary>
    /// Reads a frame PNG that the owner edited by hand. Each opaque pixel must hold a color of
    /// the palette exactly, and the mode never picks a near color (D-688).
    /// </summary>
    /// <param name="image">The decoded PNG.</param>
    /// <param name="pngFile">The path of the PNG, for each error.</param>
    /// <param name="drawing">The drawing that takes the frame. Its size must equal the PNG.</param>
    /// <param name="palette">The match of the palette of the checkout.</param>
    /// <returns>The rows of the frame.</returns>
    /// <exception cref="ImportException">
    /// The size differs, a pixel holds a partial alpha, or a color is not in the palette (T-2).
    /// </exception>
    public static IReadOnlyList<string> FromHandEdit(
        PngImage image,
        string pngFile,
        Drawing drawing,
        PaletteMatch palette)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentException.ThrowIfNullOrEmpty(pngFile);
        ArgumentNullException.ThrowIfNull(drawing);
        ArgumentNullException.ThrowIfNull(palette);

        if (image.Width != drawing.Width || image.Height != drawing.Height)
        {
            throw ImportException.For(
                pngFile,
                $"the PNG is {image.Width} by {image.Height} pixels, and the drawing '{drawing.File}' is {drawing.Width} by {drawing.Height}. The hand-edit mode crops nothing and scales nothing (D-688, D-689)");
        }

        var rows = new List<string>(image.Height);
        for (int y = 0; y < image.Height; y += 1)
        {
            char[] keys = new char[image.Width];
            for (int x = 0; x < image.Width; x += 1)
            {
                keys[x] = ExactKey(image, pngFile, x, y, palette);
            }

            rows.Add(new string(keys));
        }

        return rows;
    }

    /// <summary>
    /// Reads a picture of the Sprite Fusion generator. The mode removes the blank border,
    /// puts the content at the center of the frame of the drawing, and maps each pixel to the
    /// nearest color of the palette (D-688, D-689, D-1310, D-1312, D-1314).
    /// </summary>
    /// <param name="image">The decoded picture.</param>
    /// <param name="pngFile">The path of the picture, for each error.</param>
    /// <param name="drawing">The drawing that takes the frame, 32 by 32 or 64 by 64 pixels.</param>
    /// <param name="palette">The match of the palette of the checkout.</param>
    /// <returns>The new frame, with the counts of the report.</returns>
    /// <exception cref="ImportException">
    /// The drawing has another size, a pixel holds a partial alpha, the picture holds no
    /// opaque pixel, or the content does not fit the frame (T-2).
    /// </exception>
    public static GeneratorFrame FromGenerator(
        PngImage image,
        string pngFile,
        Drawing drawing,
        PaletteMatch palette)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentException.ThrowIfNullOrEmpty(pngFile);
        ArgumentNullException.ThrowIfNull(drawing);
        ArgumentNullException.ThrowIfNull(palette);

        int frame = drawing.Width;
        bool knownFrame = drawing.Width == drawing.Height && (frame == SmallFrame || frame == LargeFrame);
        if (!knownFrame)
        {
            throw ImportException.For(
                drawing.File,
                $"the generator mode needs a drawing of 32 by 32 or 64 by 64 pixels, and this one is {drawing.Width} by {drawing.Height} (D-689, D-1310)");
        }

        ContentBox content = FindContent(image, pngFile);
        if (content.Width > frame || content.Height > frame)
        {
            throw ImportException.For(
                pngFile,
                $"the content is {content.Width} by {content.Height} pixels, and the frame of the drawing '{drawing.File}' is {frame} by {frame}. Redraw the picture by hand to fit, or call the generator again (D-1310)");
        }

        // The extra pixel of an odd space goes to the right and to the bottom (D-1312).
        int frameLeft = (frame - content.Width) / 2;
        int frameTop = (frame - content.Height) / 2;

        char[][] grid = BlankGrid(frame);
        int mapped = 0;
        int opaque = 0;
        for (int y = 0; y < content.Height; y += 1)
        {
            for (int x = 0; x < content.Width; x += 1)
            {
                Rgb? rgb = ReadPixel(image, pngFile, content.Left + x, content.Top + y);
                if (rgb is null)
                {
                    continue;
                }

                opaque += 1;
                if (!palette.TryExact(rgb.Value.Red, rgb.Value.Green, rgb.Value.Blue, out PaletteColor? color))
                {
                    color = palette.Nearest(rgb.Value.Red, rgb.Value.Green, rgb.Value.Blue);
                    mapped += 1;
                }

                grid[frameTop + y][frameLeft + x] = color.KeyCharacter;
            }
        }

        var rows = new List<string>(frame);
        foreach (char[] row in grid)
        {
            rows.Add(new string(row));
        }

        return new GeneratorFrame(rows, content, frameLeft, frameTop, mapped, opaque);
    }

    /// <summary>Finds the part of a picture that holds each opaque pixel.</summary>
    /// <param name="image">The decoded picture.</param>
    /// <param name="pngFile">The path of the picture, for each error.</param>
    /// <returns>The box of the content.</returns>
    /// <exception cref="ImportException">
    /// A pixel holds a partial alpha, or the picture holds no opaque pixel (T-2).
    /// </exception>
    public static ContentBox FindContent(PngImage image, string pngFile)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentException.ThrowIfNullOrEmpty(pngFile);

        int left = image.Width;
        int top = image.Height;
        int right = -1;
        int bottom = -1;
        for (int y = 0; y < image.Height; y += 1)
        {
            for (int x = 0; x < image.Width; x += 1)
            {
                if (ReadPixel(image, pngFile, x, y) is null)
                {
                    continue;
                }

                left = Math.Min(left, x);
                top = Math.Min(top, y);
                right = Math.Max(right, x);
                bottom = Math.Max(bottom, y);
            }
        }

        if (right < 0)
        {
            throw ImportException.For(pngFile, "the picture holds no pixel of alpha 255, so it has no content (D-689)");
        }

        return new ContentBox(left, top, right - left + 1, bottom - top + 1);
    }

    private static char ExactKey(PngImage image, string pngFile, int x, int y, PaletteMatch palette)
    {
        Rgb? rgb = ReadPixel(image, pngFile, x, y);
        if (rgb is null)
        {
            return Drawing.Transparent;
        }

        if (!palette.TryExact(rgb.Value.Red, rgb.Value.Green, rgb.Value.Blue, out PaletteColor? color))
        {
            throw ImportException.For(
                pngFile,
                $"the pixel at x {x}, y {y} holds the color {rgb.Value.Red:x2}{rgb.Value.Green:x2}{rgb.Value.Blue:x2}, and the palette holds no such color. The hand-edit mode never picks a near color (D-688)");
        }

        return color.KeyCharacter;
    }

    // Gives the red, the green, and the blue of an opaque pixel, or null for a transparent
    // pixel. The color of a pixel of alpha 0 does not count, because a paint program keeps
    // any color under it (D-1315).
    private static Rgb? ReadPixel(PngImage image, string pngFile, int x, int y)
    {
        ReadOnlySpan<byte> pixel = image.Row(y).Slice(x * image.BytesPerPixel, image.BytesPerPixel);
        int alpha = image.Colors == PngColorKind.Rgba ? pixel[3] : 255;
        if (alpha == 0)
        {
            return null;
        }

        if (alpha != 255)
        {
            throw ImportException.For(
                pngFile,
                $"the pixel at x {x}, y {y} holds the alpha {alpha}, and a pixel holds alpha 0 or 255 alone (D-1315)");
        }

        return new Rgb(pixel[0], pixel[1], pixel[2]);
    }

    private readonly record struct Rgb(int Red, int Green, int Blue);

    private static char[][] BlankGrid(int size)
    {
        char[][] grid = new char[size][];
        for (int row = 0; row < size; row += 1)
        {
            grid[row] = new string(Drawing.Transparent, size).ToCharArray();
        }

        return grid;
    }
}
