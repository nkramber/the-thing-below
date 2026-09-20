using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using TheThingBelow.Tools.Png;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The PNG writer of Tools (D-176). Each test reads the pixels of the file that the writer
/// made, and no test compares the bytes of a PNG file (F-19).
/// </summary>
public sealed class PngWriterTests
{
    [Theory]
    [InlineData(PngColorKind.Rgb)]
    [InlineData(PngColorKind.Rgba)]
    public void AnImageThroughTheWriterAndTheReaderGivesTheSamePixels(PngColorKind colors)
    {
        int bytesPerPixel = PngImage.BytesPerPixelOf(colors);
        byte[] pixels = PngFixtures.Pixels(9, 6, bytesPerPixel);
        PngImage image = new(9, 6, colors, pixels);

        PngImage again = PngReader.Read(PngWriter.Write(image), "written.png");

        Assert.Equal(9, again.Width);
        Assert.Equal(6, again.Height);
        Assert.Equal(colors, again.Colors);
        Assert.Equal(pixels, again.Pixels.ToArray());
    }

    /// <summary>A round trip of each committed file gives the same pixels (D-665, F-19).</summary>
    [Theory]
    [InlineData("rgb-4x4.png")]
    [InlineData("rgba-4x4.png")]
    public void ARoundTripOfACommittedFileGivesTheSamePixels(string name)
    {
        PngImage image = PngReader.ReadFile(PngFixtures.PathOf(name));

        PngImage again = PngReader.Read(PngWriter.Write(image), name);

        Assert.Equal(image.Width, again.Width);
        Assert.Equal(image.Height, again.Height);
        Assert.Equal(image.Colors, again.Colors);
        Assert.Equal(image.Pixels.ToArray(), again.Pixels.ToArray());
    }

    [Fact]
    public void AnImageOfOnePixelGivesThatPixelAgain()
    {
        PngImage image = new(1, 1, PngColorKind.Rgba, new byte[] { 7, 8, 9, 10 });

        PngImage again = PngReader.Read(PngWriter.Write(image), "one-pixel.png");

        Assert.Equal(new byte[] { 7, 8, 9, 10 }, again.Pixels.ToArray());
    }

    /// <summary>The writer holds no text and no date, so two runs give the same bytes (D-176).</summary>
    [Fact]
    public void TwoRunsOfTheWriterGiveTheSameBytes()
    {
        PngImage image = new(5, 5, PngColorKind.Rgb, PngFixtures.Pixels(5, 5, 3));

        Assert.Equal(PngWriter.Write(image), PngWriter.Write(image));
    }

    /// <summary>The file holds the signature and the three chunks of the writer alone.</summary>
    [Fact]
    public void TheFileHoldsTheHeaderTheImageDataAndTheEndChunk()
    {
        PngImage image = new(4, 4, PngColorKind.Rgba, PngFixtures.Pixels(4, 4, 4));

        byte[] file = PngWriter.Write(image);

        Assert.Equal(PngFixtures.Signature.ToArray(), file[..8]);
        Assert.Equal(new[] { "IHDR", "IDAT", "IEND" }, ChunkNames(file));
    }

    /// <summary>The writer uses the filter 0 in every row (D-664).</summary>
    [Fact]
    public void EveryRowOfTheFileTakesTheFilterZero()
    {
        PngImage image = new(4, 3, PngColorKind.Rgb, PngFixtures.Pixels(4, 3, 3));

        byte[] filtered = Inflate(ImageData(PngWriter.Write(image)));

        Assert.Equal(3 * ((4 * 3) + 1), filtered.Length);
        Assert.Equal(PngRowFilter.None, filtered[0]);
        Assert.Equal(PngRowFilter.None, filtered[13]);
        Assert.Equal(PngRowFilter.None, filtered[26]);
    }

    [Fact]
    public void TheWriterAndTheReaderPassAFileThroughTheDisk()
    {
        string folder = Path.Combine(Path.GetTempPath(), "png-" + Path.GetRandomFileName());
        Directory.CreateDirectory(folder);
        try
        {
            string path = Path.Combine(folder, "written.png");
            byte[] pixels = PngFixtures.Pixels(3, 3, 4);
            PngWriter.WriteFile(path, new PngImage(3, 3, PngColorKind.Rgba, pixels));

            Assert.Equal(pixels, PngReader.ReadFile(path).Pixels.ToArray());
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void AFolderThatIsAbsentFailsWithThePath()
    {
        string path = Path.Combine(Path.GetTempPath(), "png-no-folder", "written.png");

        PngException fault = Assert.Throws<PngException>(
            () => PngWriter.WriteFile(path, new PngImage(1, 1, PngColorKind.Rgb, new byte[3])));

        Assert.Equal(path, fault.File);
        Assert.Contains("cannot write the file", fault.Message, StringComparison.Ordinal);
    }

    /// <summary>Gives the type of each chunk of a file, in the order of the file.</summary>
    private static string[] ChunkNames(byte[] file)
    {
        List<string> names = [];
        int offset = 8;
        while (offset < file.Length)
        {
            int length = (file[offset] << 24) | (file[offset + 1] << 16) | (file[offset + 2] << 8) | file[offset + 3];
            names.Add(Encoding.ASCII.GetString(file, offset + 4, 4));
            offset += 12 + length;
        }

        return [.. names];
    }

    /// <summary>Gives the data of the one IDAT chunk of a file that the writer made.</summary>
    private static byte[] ImageData(byte[] file)
    {
        int offset = 8;
        while (offset < file.Length)
        {
            int length = (file[offset] << 24) | (file[offset + 1] << 16) | (file[offset + 2] << 8) | file[offset + 3];
            if (Encoding.ASCII.GetString(file, offset + 4, 4) == "IDAT")
            {
                return file[(offset + 8)..(offset + 8 + length)];
            }

            offset += 12 + length;
        }

        throw new InvalidOperationException("The file holds no IDAT chunk (T-2).");
    }

    /// <summary>Decompresses the image data of a file that the writer made.</summary>
    private static byte[] Inflate(byte[] compressed)
    {
        using MemoryStream source = new(compressed, writable: false);
        using ZLibStream inflate = new(source, CompressionMode.Decompress);
        using MemoryStream raw = new();
        inflate.CopyTo(raw);
        return raw.ToArray();
    }
}
