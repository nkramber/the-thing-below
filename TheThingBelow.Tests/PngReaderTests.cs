using System;
using TheThingBelow.Tools.Png;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The PNG reader of Tools (D-176, D-664). Each test compares decoded pixels, and no test
/// compares the bytes of a PNG file (F-19).
/// </summary>
/// <remarks>
/// Two committed files come from an outside encoder, so they prove that the reader reads the
/// files of another program (D-665): `TheThingBelow.Tests/png/rgb-4x4.png` and `rgba-4x4.png`. A
/// session built an uncompressed TIFF of the pixels of <see cref="FixturePixels"/>, and then
/// Apple ImageIO wrote each PNG through `sips -s format png`. ImageIO chose the row filters 1
/// and 4, and it added the `sRGB`, `eXIf`, and `iTXt` chunks, which the reader skips. Neither
/// file holds a date, a path, or a name of a person.
/// </remarks>
public sealed class PngReaderTests
{
    /// <summary>The name that the reader carries in each error of a test of bytes (T-2).</summary>
    private const string Source = PngFixtures.SourceName;

    [Fact]
    public void TheCommittedRgbFileGivesItsPixels()
    {
        PngImage image = PngReader.ReadFile(PngFixtures.PathOf("rgb-4x4.png"));

        Assert.Equal(4, image.Width);
        Assert.Equal(4, image.Height);
        Assert.Equal(PngColorKind.Rgb, image.Colors);
        Assert.Equal(FixturePixels(3), image.Pixels.ToArray());
    }

    [Fact]
    public void TheCommittedRgbaFileGivesItsPixels()
    {
        PngImage image = PngReader.ReadFile(PngFixtures.PathOf("rgba-4x4.png"));

        Assert.Equal(4, image.Width);
        Assert.Equal(4, image.Height);
        Assert.Equal(PngColorKind.Rgba, image.Colors);
        Assert.Equal(FixturePixels(4), image.Pixels.ToArray());
    }

    /// <summary>Every image editor writes filtered rows, so the reader reads all five (D-664).</summary>
    [Theory]
    [InlineData(2, PngRowFilter.None)]
    [InlineData(2, PngRowFilter.Sub)]
    [InlineData(2, PngRowFilter.Up)]
    [InlineData(2, PngRowFilter.Average)]
    [InlineData(2, PngRowFilter.Paeth)]
    [InlineData(6, PngRowFilter.None)]
    [InlineData(6, PngRowFilter.Sub)]
    [InlineData(6, PngRowFilter.Up)]
    [InlineData(6, PngRowFilter.Average)]
    [InlineData(6, PngRowFilter.Paeth)]
    public void EachRowFilterGivesThePixelsOfTheImage(byte colorType, byte filterType)
    {
        int bytesPerPixel = colorType == 6 ? 4 : 3;
        byte[] pixels = PngFixtures.Pixels(7, 5, bytesPerPixel);
        byte[] file = PngFixtures.Image(7, 5, colorType, filterType, pixels);

        PngImage image = PngReader.Read(file, Source);

        Assert.Equal(pixels, image.Pixels.ToArray());
    }

    [Fact]
    public void AnIndexedFileFailsWithTheFileAndTheReason()
    {
        byte[] file = PngFixtures.File(
            PngFixtures.Chunk("IHDR", PngFixtures.Header(4, 4, 8, 3)),
            PngFixtures.Chunk("PLTE", new byte[3]),
            PngFixtures.Chunk("IDAT", PngFixtures.Deflate(new byte[20])),
            PngFixtures.Chunk("IEND", ReadOnlySpan<byte>.Empty));

        PngException fault = Assert.Throws<PngException>(() => PngReader.Read(file, Source));

        Assert.Equal(Source, fault.File);
        Assert.Contains("the color type is 3 (indexed)", fault.Message, StringComparison.Ordinal);
        Assert.Contains(Source, fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASixteenBitFileFailsWithTheFileAndTheReason()
    {
        byte[] file = PngFixtures.File(
            PngFixtures.Chunk("IHDR", PngFixtures.Header(4, 4, 16, 2)),
            PngFixtures.Chunk("IDAT", PngFixtures.Deflate(new byte[100])),
            PngFixtures.Chunk("IEND", ReadOnlySpan<byte>.Empty));

        PngException fault = Assert.Throws<PngException>(() => PngReader.Read(file, Source));

        Assert.Equal(Source, fault.File);
        Assert.Contains("the bit depth is 16", fault.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData((byte)0, "grayscale")]
    [InlineData((byte)4, "grayscale with alpha")]
    public void AFileOfAnotherColorTypeFails(byte colorType, string name)
    {
        byte[] file = PngFixtures.File(
            PngFixtures.Chunk("IHDR", PngFixtures.Header(4, 4, 8, colorType)),
            PngFixtures.Chunk("IDAT", PngFixtures.Deflate(new byte[20])),
            PngFixtures.Chunk("IEND", ReadOnlySpan<byte>.Empty));

        PngException fault = Assert.Throws<PngException>(() => PngReader.Read(file, Source));

        Assert.Contains($"the color type is {colorType} ({name})", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnInterlacedFileFails()
    {
        byte[] file = PngFixtures.File(
            PngFixtures.Chunk("IHDR", PngFixtures.Header(4, 4, 8, 2, interlace: 1)),
            PngFixtures.Chunk("IDAT", PngFixtures.Deflate(new byte[52])),
            PngFixtures.Chunk("IEND", ReadOnlySpan<byte>.Empty));

        PngException fault = Assert.Throws<PngException>(() => PngReader.Read(file, Source));

        Assert.Contains("the interlace method is 1", fault.Message, StringComparison.Ordinal);
    }

    /// <summary>A truncated file fails at every length, and it never reads past the end (T-2).</summary>
    [Fact]
    public void ATruncatedFileFailsWithTheFileAndTheReason()
    {
        byte[] file = PngFixtures.Image(7, 5, 2, PngRowFilter.Sub, PngFixtures.Pixels(7, 5, 3));

        for (int length = 0; length < file.Length; length += 1)
        {
            byte[] shortFile = file[..length];
            PngException fault = Assert.Throws<PngException>(() => PngReader.Read(shortFile, Source));
            Assert.Equal(Source, fault.File);
            Assert.Contains(Source, fault.Message, StringComparison.Ordinal);
        }

        Assert.Equal(7 * 5 * 3, PngReader.Read(file, Source).Pixels.Length);
    }

    [Fact]
    public void AWrongCrcOfTheHeaderFailsWithTheChunkName()
    {
        byte[] file = PngFixtures.Image(4, 4, 2, PngRowFilter.None, PngFixtures.Pixels(4, 4, 3));

        // The IHDR data starts after the signature, the length field, and the type.
        file[16] = (byte)(file[16] ^ 0x01);

        PngException fault = Assert.Throws<PngException>(() => PngReader.Read(file, Source));

        Assert.Contains("the chunk 'IHDR' carries the CRC-32", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AWrongCrcOfTheImageDataFailsWithTheChunkName()
    {
        byte[] data = PngFixtures.Chunk(
            "IDAT",
            PngFixtures.Deflate(PngFixtures.Filter(4, 4, 3, PngRowFilter.None, PngFixtures.Pixels(4, 4, 3))));

        // The byte 8 is the first byte of the data, after the length field and the type.
        data[8] = (byte)(data[8] ^ 0x01);
        byte[] file = PngFixtures.File(
            PngFixtures.Chunk("IHDR", PngFixtures.Header(4, 4, 8, 2)),
            data,
            PngFixtures.Chunk("IEND", ReadOnlySpan<byte>.Empty));

        PngException fault = Assert.Throws<PngException>(() => PngReader.Read(file, Source));

        Assert.Contains("the chunk 'IDAT' carries the CRC-32", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFileThatStartsWithOtherBytesFails()
    {
        byte[] file = PngFixtures.Image(4, 4, 2, PngRowFilter.None, PngFixtures.Pixels(4, 4, 3));
        file[1] = (byte)'X';

        PngException fault = Assert.Throws<PngException>(() => PngReader.Read(file, Source));

        Assert.Contains("are not the PNG signature", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void BytesAfterTheEndChunkFail()
    {
        byte[] file = PngFixtures.Image(4, 4, 2, PngRowFilter.None, PngFixtures.Pixels(4, 4, 3));
        byte[] longer = [.. file, 0, 0, 0, 0];

        PngException fault = Assert.Throws<PngException>(() => PngReader.Read(longer, Source));

        Assert.Contains("4 byte(s) after the IEND chunk", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFileWithNoImageDataFails()
    {
        byte[] file = PngFixtures.File(
            PngFixtures.Chunk("IHDR", PngFixtures.Header(4, 4, 8, 2)),
            PngFixtures.Chunk("IEND", ReadOnlySpan<byte>.Empty));

        PngException fault = Assert.Throws<PngException>(() => PngReader.Read(file, Source));

        Assert.Contains("holds no IDAT chunk", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ACriticalChunkThatTheReaderDoesNotReadFails()
    {
        byte[] file = PngFixtures.File(
            PngFixtures.Chunk("IHDR", PngFixtures.Header(4, 4, 8, 2)),
            PngFixtures.Chunk("ZZZZ", new byte[2]),
            PngFixtures.Chunk("IDAT", PngFixtures.Deflate(PngFixtures.Filter(4, 4, 3, 0, PngFixtures.Pixels(4, 4, 3)))),
            PngFixtures.Chunk("IEND", ReadOnlySpan<byte>.Empty));

        PngException fault = Assert.Throws<PngException>(() => PngReader.Read(file, Source));

        Assert.Contains("the critical chunk 'ZZZZ'", fault.Message, StringComparison.Ordinal);
    }

    /// <summary>An image editor writes chunks that no rule of this project reads (D-176).</summary>
    [Fact]
    public void AnAncillaryChunkIsSkipped()
    {
        byte[] pixels = PngFixtures.Pixels(4, 4, 3);
        byte[] file = PngFixtures.File(
            PngFixtures.Chunk("IHDR", PngFixtures.Header(4, 4, 8, 2)),
            PngFixtures.Chunk("gAMA", new byte[4]),
            PngFixtures.Chunk("IDAT", PngFixtures.Deflate(PngFixtures.Filter(4, 4, 3, 0, pixels))),
            PngFixtures.Chunk("tEXt", new byte[5]),
            PngFixtures.Chunk("IEND", ReadOnlySpan<byte>.Empty));

        PngImage image = PngReader.Read(file, Source);

        Assert.Equal(pixels, image.Pixels.ToArray());
    }

    /// <summary>A PNG of this kind can carry a suggested palette, and the reader skips it.</summary>
    [Fact]
    public void ASuggestedPaletteIsSkipped()
    {
        byte[] pixels = PngFixtures.Pixels(4, 4, 3);
        byte[] file = PngFixtures.File(
            PngFixtures.Chunk("IHDR", PngFixtures.Header(4, 4, 8, 2)),
            PngFixtures.Chunk("PLTE", new byte[6]),
            PngFixtures.Chunk("IDAT", PngFixtures.Deflate(PngFixtures.Filter(4, 4, 3, 0, pixels))),
            PngFixtures.Chunk("IEND", ReadOnlySpan<byte>.Empty));

        PngImage image = PngReader.Read(file, Source);

        Assert.Equal(pixels, image.Pixels.ToArray());
    }

    [Fact]
    public void TheImageDataOfEveryIdatChunkJoins()
    {
        byte[] pixels = PngFixtures.Pixels(6, 4, 4);
        byte[] data = PngFixtures.Deflate(PngFixtures.Filter(6, 4, 4, PngRowFilter.Up, pixels));
        int half = data.Length / 2;
        byte[] file = PngFixtures.File(
            PngFixtures.Chunk("IHDR", PngFixtures.Header(6, 4, 8, 6)),
            PngFixtures.Chunk("IDAT", data[..half]),
            PngFixtures.Chunk("IDAT", data[half..]),
            PngFixtures.Chunk("IEND", ReadOnlySpan<byte>.Empty));

        PngImage image = PngReader.Read(file, Source);

        Assert.Equal(pixels, image.Pixels.ToArray());
    }

    [Fact]
    public void AnImageDataChunkAfterAnotherChunkFails()
    {
        byte[] data = PngFixtures.Deflate(PngFixtures.Filter(4, 4, 3, 0, PngFixtures.Pixels(4, 4, 3)));
        int half = data.Length / 2;
        byte[] file = PngFixtures.File(
            PngFixtures.Chunk("IHDR", PngFixtures.Header(4, 4, 8, 2)),
            PngFixtures.Chunk("IDAT", data[..half]),
            PngFixtures.Chunk("gAMA", new byte[4]),
            PngFixtures.Chunk("IDAT", data[half..]),
            PngFixtures.Chunk("IEND", ReadOnlySpan<byte>.Empty));

        PngException fault = Assert.Throws<PngException>(() => PngReader.Read(file, Source));

        Assert.Contains("the IDAT chunks come one after the other", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFirstChunkThatIsNotTheHeaderFails()
    {
        byte[] file = PngFixtures.File(
            PngFixtures.Chunk("gAMA", new byte[4]),
            PngFixtures.Chunk("IHDR", PngFixtures.Header(4, 4, 8, 2)),
            PngFixtures.Chunk("IEND", ReadOnlySpan<byte>.Empty));

        PngException fault = Assert.Throws<PngException>(() => PngReader.Read(file, Source));

        Assert.Contains("the first chunk is 'gAMA'", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASecondHeaderChunkFails()
    {
        byte[] file = PngFixtures.File(
            PngFixtures.Chunk("IHDR", PngFixtures.Header(4, 4, 8, 2)),
            PngFixtures.Chunk("IHDR", PngFixtures.Header(4, 4, 8, 2)),
            PngFixtures.Chunk("IEND", ReadOnlySpan<byte>.Empty));

        PngException fault = Assert.Throws<PngException>(() => PngReader.Read(file, Source));

        Assert.Contains("a second IHDR chunk", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ImageDataThatIsNotZlibDataFails()
    {
        byte[] file = PngFixtures.File(
            PngFixtures.Chunk("IHDR", PngFixtures.Header(4, 4, 8, 2)),
            PngFixtures.Chunk("IDAT", new byte[] { 1, 2, 3, 4, 5, 6 }),
            PngFixtures.Chunk("IEND", ReadOnlySpan<byte>.Empty));

        PngException fault = Assert.Throws<PngException>(() => PngReader.Read(file, Source));

        Assert.Contains("is not zlib data", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ImageDataOfTooFewBytesFails()
    {
        byte[] shortData = PngFixtures.Filter(4, 4, 3, 0, PngFixtures.Pixels(4, 4, 3))[..30];
        byte[] file = PngFixtures.File(
            PngFixtures.Chunk("IHDR", PngFixtures.Header(4, 4, 8, 2)),
            PngFixtures.Chunk("IDAT", PngFixtures.Deflate(shortData)),
            PngFixtures.Chunk("IEND", ReadOnlySpan<byte>.Empty));

        PngException fault = Assert.Throws<PngException>(() => PngReader.Read(file, Source));

        Assert.Contains("the image data gives 30 bytes, and the IHDR fields need 52", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ImageDataOfTooManyBytesFails()
    {
        byte[] longData = [.. PngFixtures.Filter(4, 4, 3, 0, PngFixtures.Pixels(4, 4, 3)), 0, 0, 0];
        byte[] file = PngFixtures.File(
            PngFixtures.Chunk("IHDR", PngFixtures.Header(4, 4, 8, 2)),
            PngFixtures.Chunk("IDAT", PngFixtures.Deflate(longData)),
            PngFixtures.Chunk("IEND", ReadOnlySpan<byte>.Empty));

        PngException fault = Assert.Throws<PngException>(() => PngReader.Read(file, Source));

        Assert.Contains("more than the 52 bytes", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnImageAboveTheSizeLimitFailsBeforeAnyAllocation()
    {
        byte[] file = PngFixtures.File(
            PngFixtures.Chunk("IHDR", PngFixtures.Header(PngImage.MaxSize + 1, 4, 8, 2)),
            PngFixtures.Chunk("IEND", ReadOnlySpan<byte>.Empty));

        PngException fault = Assert.Throws<PngException>(() => PngReader.Read(file, Source));

        Assert.Contains($"and this reader reads 1 to {PngImage.MaxSize}", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnImageOfNoPixelsFails()
    {
        byte[] file = PngFixtures.File(
            PngFixtures.Chunk("IHDR", PngFixtures.Header(0, 4, 8, 2)),
            PngFixtures.Chunk("IEND", ReadOnlySpan<byte>.Empty));

        PngException fault = Assert.Throws<PngException>(() => PngReader.Read(file, Source));

        Assert.Contains("the image is 0 by 4 pixels", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AChunkTypeThatIsNotFourLettersFails()
    {
        byte[] chunk = PngFixtures.Chunk("IHDR", PngFixtures.Header(4, 4, 8, 2));
        chunk[4] = 0x20;
        byte[] file = PngFixtures.File(chunk, PngFixtures.Chunk("IEND", ReadOnlySpan<byte>.Empty));

        PngException fault = Assert.Throws<PngException>(() => PngReader.Read(file, Source));

        Assert.Contains("a chunk type holds four letters", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AHeaderChunkOfAnotherLengthFails()
    {
        byte[] file = PngFixtures.File(
            PngFixtures.Chunk("IHDR", new byte[10]),
            PngFixtures.Chunk("IEND", ReadOnlySpan<byte>.Empty));

        PngException fault = Assert.Throws<PngException>(() => PngReader.Read(file, Source));

        Assert.Contains("the IHDR chunk holds 10 bytes", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAbsentFileFailsWithThePath()
    {
        string path = PngFixtures.PathOf("no-such-file.png");

        PngException fault = Assert.Throws<PngException>(() => PngReader.ReadFile(path));

        Assert.Equal(path, fault.File);
        Assert.Contains("cannot read the file", fault.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// The pixels of the two committed files. The red channel follows the column, the green
    /// channel follows the row, and the blue channel and the alpha channel fall as the sum of
    /// the two grows. Thus a reader that swaps a channel or a row fails a test.
    /// </summary>
    private static byte[] FixturePixels(int bytesPerPixel)
    {
        byte[] pixels = new byte[4 * 4 * bytesPerPixel];
        int at = 0;
        for (int y = 0; y < 4; y += 1)
        {
            for (int x = 0; x < 4; x += 1)
            {
                pixels[at] = (byte)(16 + (32 * x));
                pixels[at + 1] = (byte)(8 + (48 * y));
                pixels[at + 2] = (byte)(200 - (24 * (x + y)));
                if (bytesPerPixel == 4)
                {
                    pixels[at + 3] = (byte)(255 - (16 * (x + y)));
                }

                at += bytesPerPixel;
            }
        }

        return pixels;
    }
}
