using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Png;

namespace TheThingBelow.Tools.Atlas;

/// <summary>
/// An RGBA image that the atlas tool draws into. The pages of the atlas, the swatch sheet,
/// and the review sheets all come from it (D-107, D-185, D-668).
/// </summary>
/// <remarks>
/// Every value is a whole number, and each color comes from the palette, so every CI leg
/// makes the same pixels (D-502, F-19, T-7).
/// </remarks>
public sealed class AtlasCanvas
{
    /// <summary>The count of bytes of one pixel: red, green, blue, and alpha.</summary>
    public const int BytesPerPixel = 4;

    private readonly byte[] pixels;

    /// <summary>Makes a canvas of transparent pixels.</summary>
    /// <param name="width">The count of pixels in one row, 1 or more.</param>
    /// <param name="height">The count of rows, 1 or more.</param>
    public AtlasCanvas(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);

        this.Width = width;
        this.Height = height;
        this.pixels = new byte[(long)width * height * BytesPerPixel];
    }

    /// <summary>The count of pixels in one row.</summary>
    public int Width { get; }

    /// <summary>The count of rows.</summary>
    public int Height { get; }

    /// <summary>Sets one pixel to a color that covers what is under it.</summary>
    /// <param name="x">The pixel column, from 0 to the width less 1.</param>
    /// <param name="y">The pixel row, from 0 to the height less 1.</param>
    /// <param name="red">The red part, from 0 to 255.</param>
    /// <param name="green">The green part, from 0 to 255.</param>
    /// <param name="blue">The blue part, from 0 to 255.</param>
    /// <exception cref="ArgumentOutOfRangeException">The place is outside the canvas (T-2).</exception>
    public void Set(int x, int y, int red, int green, int blue)
    {
        if (x < 0 || x >= this.Width || y < 0 || y >= this.Height)
        {
            throw new ArgumentOutOfRangeException(
                nameof(x),
                $"The place {x},{y} is outside the canvas of {this.Width} by {this.Height} pixels (T-2).");
        }

        int start = ((y * this.Width) + x) * BytesPerPixel;
        this.pixels[start] = (byte)red;
        this.pixels[start + 1] = (byte)green;
        this.pixels[start + 2] = (byte)blue;
        this.pixels[start + 3] = 255;
    }

    /// <summary>Fills a rectangle with one color.</summary>
    /// <param name="x">The pixel column of the left edge.</param>
    /// <param name="y">The pixel row of the top edge.</param>
    /// <param name="width">The count of pixels in one row of the rectangle.</param>
    /// <param name="height">The count of rows of the rectangle.</param>
    /// <param name="color">The color of every pixel of the rectangle.</param>
    public void Fill(int x, int y, int width, int height, PaletteColor color)
    {
        ArgumentNullException.ThrowIfNull(color);

        for (int row = 0; row < height; row += 1)
        {
            for (int column = 0; column < width; column += 1)
            {
                this.Set(x + column, y + row, color.Red, color.Green, color.Blue);
            }
        }
    }

    /// <summary>Draws one frame of a drawing, with each pixel as a square of the scale.</summary>
    /// <param name="drawing">The drawing that holds the frame.</param>
    /// <param name="frame">The position of the frame, which starts at 0.</param>
    /// <param name="palette">The palette that gives the color of each key.</param>
    /// <param name="x">The pixel column of the left edge on the canvas.</param>
    /// <param name="y">The pixel row of the top edge on the canvas.</param>
    /// <param name="scale">The count of canvas pixels of one pixel of the drawing, 1 or more.</param>
    /// <exception cref="InvalidOperationException">A key of the frame is not in the palette (T-2).</exception>
    /// <remarks>
    /// The dot leaves the pixels under it, so a drawing on a ground of a review sheet keeps
    /// that ground (D-107, D-668).
    /// </remarks>
    public void Draw(Drawing drawing, int frame, Palette palette, int x, int y, int scale)
    {
        ArgumentNullException.ThrowIfNull(drawing);
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentOutOfRangeException.ThrowIfLessThan(scale, 1);

        IReadOnlyList<string> rows = drawing.Frames[frame].Rows;
        for (int row = 0; row < rows.Count; row += 1)
        {
            string keys = rows[row];
            for (int column = 0; column < keys.Length; column += 1)
            {
                char key = keys[column];
                if (key == Drawing.Transparent)
                {
                    continue;
                }

                if (!palette.TryColorOf(key, out PaletteColor? color))
                {
                    throw new InvalidOperationException(
                        $"The file '{drawing.File}' holds the key '{key}' at frame {frame}, row {row}, column {column}, and the palette has no such color (F-20).");
                }

                this.Fill(x + (column * scale), y + (row * scale), scale, scale, color);
            }
        }
    }

    /// <summary>Gives the canvas as an image that the PNG writer takes.</summary>
    /// <returns>The image, with four 8-bit channels for each pixel.</returns>
    public PngImage ToImage() => new(this.Width, this.Height, PngColorKind.Rgba, this.pixels);
}
