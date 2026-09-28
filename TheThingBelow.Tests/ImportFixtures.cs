using System;
using System.IO;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Png;

namespace TheThingBelow.Tests;

/// <summary>
/// Builds the pictures and the checkouts of the tests of the PNG import (D-688, D-1311). A
/// picture starts with every pixel at alpha 0, as a picture of the generator does.
/// </summary>
public sealed class ImportPicture
{
    private readonly byte[] pixels;

    /// <summary>Makes a picture of transparent pixels.</summary>
    /// <param name="width">The count of pixels in one row.</param>
    /// <param name="height">The count of rows.</param>
    public ImportPicture(int width, int height)
    {
        this.Width = width;
        this.Height = height;
        this.pixels = new byte[width * height * 4];
    }

    /// <summary>The count of pixels in one row.</summary>
    public int Width { get; }

    /// <summary>The count of rows.</summary>
    public int Height { get; }

    /// <summary>Sets one pixel.</summary>
    /// <param name="x">The column.</param>
    /// <param name="y">The row.</param>
    /// <param name="hex">The color as six hexadecimal digits, such as `0b0a0f`.</param>
    /// <param name="alpha">The alpha, 255 for an opaque pixel.</param>
    /// <returns>This picture, for the next call.</returns>
    public ImportPicture Set(int x, int y, string hex, int alpha = 255)
    {
        int start = ((y * this.Width) + x) * 4;
        this.pixels[start] = Convert.ToByte(hex[..2], 16);
        this.pixels[start + 1] = Convert.ToByte(hex[2..4], 16);
        this.pixels[start + 2] = Convert.ToByte(hex[4..6], 16);
        this.pixels[start + 3] = (byte)alpha;
        return this;
    }

    /// <summary>Fills a rectangle with one opaque color.</summary>
    /// <param name="x">The column of the left edge.</param>
    /// <param name="y">The row of the top edge.</param>
    /// <param name="width">The count of columns.</param>
    /// <param name="height">The count of rows.</param>
    /// <param name="hex">The color as six hexadecimal digits.</param>
    /// <returns>This picture, for the next call.</returns>
    public ImportPicture Fill(int x, int y, int width, int height, string hex)
    {
        for (int row = 0; row < height; row += 1)
        {
            for (int column = 0; column < width; column += 1)
            {
                this.Set(x + column, y + row, hex);
            }
        }

        return this;
    }

    /// <summary>Gives the picture as a decoded RGBA image.</summary>
    /// <returns>The image.</returns>
    public PngImage ToImage() => new(this.Width, this.Height, PngColorKind.Rgba, this.pixels);
}

/// <summary>A checkout in a temporary folder, with the palette of the drawing fixtures.</summary>
public sealed class ImportCheckout : IDisposable
{
    /// <summary>Makes the folder and writes the palette of <see cref="DrawingFixtures"/>.</summary>
    public ImportCheckout()
    {
        this.WriteContent(Palette.Path, DrawingFixtures.PaletteBody);
    }

    /// <summary>The root of the checkout.</summary>
    public string Root { get; } = Path.Combine(Path.GetTempPath(), $"import-{Guid.NewGuid():N}");

    /// <summary>Removes the folder.</summary>
    public void Dispose()
    {
        if (Directory.Exists(this.Root))
        {
            Directory.Delete(this.Root, recursive: true);
        }
    }

    /// <summary>Writes a file under `content/`.</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <param name="body">The text of the file.</param>
    /// <returns>The full path of the file.</returns>
    public string WriteContent(string path, string body)
    {
        string full = this.PathOf(path);
        Directory.CreateDirectory(Path.GetDirectoryName(full) ?? this.Root);
        File.WriteAllText(full, body);
        return full;
    }

    /// <summary>Writes a PNG at the root of the checkout.</summary>
    /// <param name="name">The file name, such as `edit.png`.</param>
    /// <param name="image">The image.</param>
    /// <returns>The full path of the file.</returns>
    public string WritePng(string name, PngImage image)
    {
        string full = Path.Combine(this.Root, name);
        PngWriter.WriteFile(full, image);
        return full;
    }

    /// <summary>Reads a drawing file of the checkout through the reader of Core.</summary>
    /// <param name="full">The full path of the file.</param>
    /// <returns>The drawing.</returns>
    public static Drawing ReadDrawing(string full) => Drawing.Read(File.ReadAllBytes(full), full);

    /// <summary>Gives the full path of a file under `content/`.</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>The full path.</returns>
    public string PathOf(string path) =>
        Path.Combine(this.Root, "content", path.Replace('/', Path.DirectorySeparatorChar));
}
