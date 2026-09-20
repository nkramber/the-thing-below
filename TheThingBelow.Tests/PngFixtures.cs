using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using TheThingBelow.Tools.Png;

namespace TheThingBelow.Tests;

/// <summary>
/// Builds PNG bytes for the reader tests. Each failure case of PR-47 is bytes that this class
/// makes, so a reader of the test sees why the file must fail (D-665).
/// </summary>
/// <remarks>
/// The filter code here is the forward direction, and <see cref="PngRowFilter"/> holds the
/// reverse direction. The two are separate code on purpose, so a test proves the reader and
/// never a copy of it.
/// <para>
/// The chunk builder calls <see cref="Crc32"/>, which two other tests prove: the published
/// check values of <c>Crc32Tests</c>, and the two committed files that Apple ImageIO wrote,
/// which the reader accepts only when its CRC-32 agrees with the CRC-32 of that encoder.
/// </para>
/// </remarks>
internal static class PngFixtures
{
    /// <summary>The folder of the committed files, from the root of the checkout.</summary>
    public const string Folder = "TheThingBelow.Tests/png";

    /// <summary>The name that an error message of a test carries, in place of a path.</summary>
    public const string SourceName = "the test bytes";

    /// <summary>The eight bytes that start every PNG file.</summary>
    public static ReadOnlySpan<byte> Signature => [137, 80, 78, 71, 13, 10, 26, 10];

    /// <summary>Gives the path of a committed file of the fixture folder.</summary>
    /// <param name="name">The file name, such as `rgb-4x4.png`.</param>
    /// <returns>The full path of the file.</returns>
    public static string PathOf(string name) => RepositoryRoot.PathTo($"{Folder}/{name}");

    /// <summary>Makes the data of an IHDR chunk.</summary>
    /// <param name="width">The count of pixels in one row.</param>
    /// <param name="height">The count of rows.</param>
    /// <param name="bitDepth">The bit depth, such as 8 or 16.</param>
    /// <param name="colorType">The color type, such as 2, 3, or 6.</param>
    /// <param name="interlace">0 for an image that is not interlaced, and 1 for one that is.</param>
    /// <returns>The 13 bytes of the IHDR data.</returns>
    public static byte[] Header(int width, int height, byte bitDepth, byte colorType, byte interlace = 0)
    {
        byte[] data = new byte[13];
        WriteBigEndian(data.AsSpan(0, 4), (uint)width);
        WriteBigEndian(data.AsSpan(4, 4), (uint)height);
        data[8] = bitDepth;
        data[9] = colorType;
        data[12] = interlace;
        return data;
    }

    /// <summary>Makes one chunk with the CRC-32 that its bytes give.</summary>
    /// <param name="name">The four letters of the chunk type.</param>
    /// <param name="data">The data of the chunk.</param>
    /// <returns>The length, the type, the data, and the CRC-32.</returns>
    public static byte[] Chunk(string name, ReadOnlySpan<byte> data)
    {
        byte[] typeAndData = new byte[4 + data.Length];
        Encoding.ASCII.GetBytes(name, typeAndData);
        data.CopyTo(typeAndData.AsSpan(4));

        byte[] chunk = new byte[12 + data.Length];
        WriteBigEndian(chunk.AsSpan(0, 4), (uint)data.Length);
        typeAndData.CopyTo(chunk.AsSpan(4));
        WriteBigEndian(chunk.AsSpan(8 + data.Length, 4), Crc32.Compute(typeAndData));
        return chunk;
    }

    /// <summary>Joins the signature and each chunk into one file.</summary>
    /// <param name="chunks">The chunks, in the order of the file.</param>
    /// <returns>The bytes of the file.</returns>
    public static byte[] File(params byte[][] chunks)
    {
        List<byte> file = [.. Signature];
        foreach (byte[] chunk in chunks)
        {
            file.AddRange(chunk);
        }

        return [.. file];
    }

    /// <summary>Makes a whole file of one filter type, with the pixels of the caller.</summary>
    /// <param name="width">The count of pixels in one row.</param>
    /// <param name="height">The count of rows.</param>
    /// <param name="colorType">2 for RGB, and 6 for RGBA.</param>
    /// <param name="filterType">The filter of every row, from 0 to 4.</param>
    /// <param name="pixels">The pixel bytes, row after row.</param>
    /// <returns>The bytes of the file.</returns>
    public static byte[] Image(int width, int height, byte colorType, byte filterType, ReadOnlySpan<byte> pixels)
    {
        int bytesPerPixel = colorType == 6 ? 4 : 3;
        byte[] filtered = Filter(width, height, bytesPerPixel, filterType, pixels);
        return File(
            Chunk("IHDR", Header(width, height, 8, colorType)),
            Chunk("IDAT", Deflate(filtered)),
            Chunk("IEND", ReadOnlySpan<byte>.Empty));
    }

    /// <summary>Makes pixel bytes that differ in every channel and in every row.</summary>
    /// <param name="width">The count of pixels in one row.</param>
    /// <param name="height">The count of rows.</param>
    /// <param name="bytesPerPixel">3 for RGB, and 4 for RGBA.</param>
    /// <returns>The pixel bytes, row after row.</returns>
    public static byte[] Pixels(int width, int height, int bytesPerPixel)
    {
        byte[] pixels = new byte[width * height * bytesPerPixel];
        for (int index = 0; index < pixels.Length; index += 1)
        {
            pixels[index] = (byte)((index * 37) + (index / 5));
        }

        return pixels;
    }

    /// <summary>Puts each row of pixels through the forward direction of one filter.</summary>
    /// <param name="width">The count of pixels in one row.</param>
    /// <param name="height">The count of rows.</param>
    /// <param name="bytesPerPixel">3 for RGB, and 4 for RGBA.</param>
    /// <param name="filterType">The filter of every row, from 0 to 4.</param>
    /// <param name="pixels">The pixel bytes, row after row.</param>
    /// <returns>One filter byte and then one filtered row, for each row.</returns>
    public static byte[] Filter(
        int width, int height, int bytesPerPixel, byte filterType, ReadOnlySpan<byte> pixels)
    {
        int stride = width * bytesPerPixel;
        byte[] filtered = new byte[(stride + 1) * height];
        for (int y = 0; y < height; y += 1)
        {
            ReadOnlySpan<byte> row = pixels.Slice(y * stride, stride);
            ReadOnlySpan<byte> above = y == 0 ? new byte[stride] : pixels.Slice((y - 1) * stride, stride);
            filtered[y * (stride + 1)] = filterType;
            for (int index = 0; index < stride; index += 1)
            {
                int left = index >= bytesPerPixel ? row[index - bytesPerPixel] : 0;
                int up = above[index];
                int upLeft = index >= bytesPerPixel ? above[index - bytesPerPixel] : 0;
                int prediction = filterType switch
                {
                    0 => 0,
                    1 => left,
                    2 => up,
                    3 => (left + up) / 2,
                    4 => Paeth(left, up, upLeft),
                    _ => throw new ArgumentOutOfRangeException(nameof(filterType), filterType, "The filters are 0 to 4."),
                };

                filtered[(y * (stride + 1)) + 1 + index] = (byte)((row[index] - prediction) & 0xff);
            }
        }

        return filtered;
    }

    /// <summary>Compresses bytes as the IDAT chunk of a PNG holds them.</summary>
    /// <param name="raw">The filtered bytes of the image.</param>
    /// <returns>The zlib data.</returns>
    public static byte[] Deflate(ReadOnlySpan<byte> raw)
    {
        using MemoryStream compressed = new();
        using (ZLibStream deflate = new(compressed, CompressionLevel.Optimal, leaveOpen: true))
        {
            deflate.Write(raw);
        }

        return compressed.ToArray();
    }

    /// <summary>The Paeth prediction of the specification, written for the test alone.</summary>
    private static int Paeth(int left, int up, int upLeft)
    {
        int estimate = left + up - upLeft;
        int fromLeft = Math.Abs(estimate - left);
        int fromUp = Math.Abs(estimate - up);
        int fromUpLeft = Math.Abs(estimate - upLeft);
        if (fromLeft <= fromUp && fromLeft <= fromUpLeft)
        {
            return left;
        }

        return fromUp <= fromUpLeft ? up : upLeft;
    }

    private static void WriteBigEndian(Span<byte> field, uint value)
    {
        field[0] = (byte)(value >> 24);
        field[1] = (byte)(value >> 16);
        field[2] = (byte)(value >> 8);
        field[3] = (byte)value;
    }
}
