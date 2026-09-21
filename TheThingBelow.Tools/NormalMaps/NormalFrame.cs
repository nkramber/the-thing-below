using System;

namespace TheThingBelow.Tools.NormalMaps;

/// <summary>
/// The normal map of one frame of a drawing: three channels and an opaque flag for each pixel
/// (D-184). The page of the normal-map atlas and the review sheet both read it.
/// </summary>
/// <remarks>
/// The channels follow the encoding that the canvas shader of Godot 4.7.2 reads: red is the
/// part to the right, green is the part up the screen, and blue is the part toward the
/// viewer. Each part maps -1 to 1 onto 1 to 255, with 128 for 0 (the external facts of
/// `docs/roadmaps/area-effects.md`).
/// </remarks>
public sealed class NormalFrame
{
    private readonly byte[] channels;
    private readonly bool[] opaque;

    /// <summary>Makes a frame of transparent pixels.</summary>
    /// <param name="width">The count of pixels in one row, 1 or more.</param>
    /// <param name="height">The count of rows, 1 or more.</param>
    public NormalFrame(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);

        this.Width = width;
        this.Height = height;
        this.channels = new byte[width * height * 3];
        this.opaque = new bool[width * height];
    }

    /// <summary>The count of pixels in one row.</summary>
    public int Width { get; }

    /// <summary>The count of rows.</summary>
    public int Height { get; }

    /// <summary>Tells whether a pixel holds a color of the drawing.</summary>
    /// <param name="x">The pixel column.</param>
    /// <param name="y">The pixel row.</param>
    /// <returns>True when the drawing covers the pixel.</returns>
    public bool IsOpaque(int x, int y) => this.opaque[this.PixelOf(x, y)];

    /// <summary>Gives one channel of a pixel.</summary>
    /// <param name="x">The pixel column.</param>
    /// <param name="y">The pixel row.</param>
    /// <param name="channel">0 for red, 1 for green, and 2 for blue.</param>
    /// <returns>The value, from 0 to 255. A transparent pixel holds 0 in each channel.</returns>
    public int Channel(int x, int y, int channel)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(channel);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(channel, 2);

        return this.channels[(this.PixelOf(x, y) * 3) + channel];
    }

    /// <summary>Sets one opaque pixel.</summary>
    /// <param name="x">The pixel column.</param>
    /// <param name="y">The pixel row.</param>
    /// <param name="red">The part to the right, from 0 to 255.</param>
    /// <param name="green">The part up the screen, from 0 to 255.</param>
    /// <param name="blue">The part toward the viewer, from 0 to 255.</param>
    public void Set(int x, int y, int red, int green, int blue)
    {
        int pixel = this.PixelOf(x, y);
        this.channels[pixel * 3] = checked((byte)red);
        this.channels[(pixel * 3) + 1] = checked((byte)green);
        this.channels[(pixel * 3) + 2] = checked((byte)blue);
        this.opaque[pixel] = true;
    }

    private int PixelOf(int x, int y)
    {
        if (x < 0 || x >= this.Width || y < 0 || y >= this.Height)
        {
            throw new ArgumentOutOfRangeException(
                nameof(x),
                $"The place {x},{y} is outside the frame of {this.Width} by {this.Height} pixels (T-2).");
        }

        return (y * this.Width) + x;
    }
}
