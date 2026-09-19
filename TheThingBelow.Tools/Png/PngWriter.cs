using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace TheThingBelow.Tools.Png;

/// <summary>
/// The PNG writer of Tools. It writes an 8-bit RGB or RGBA image, with the row filter 0 and
/// no chunk beyond IHDR, IDAT, and IEND (D-176, D-664).
/// </summary>
/// <remarks>
/// The writer holds no date, no name of a tool, and no other text, so two runs of a tool on
/// the same pixels give the same file. The compressed bytes still depend on the version of
/// `ZLibStream`, so every test of a picture compares decoded pixels and never the bytes of a
/// file (F-19).
/// </remarks>
public static class PngWriter
{
    /// <summary>Writes a PNG file to the disk.</summary>
    /// <param name="path">The path of the file.</param>
    /// <param name="image">The image to write.</param>
    /// <exception cref="PngException">The writer cannot write the file.</exception>
    public static void WriteFile(string path, PngImage image)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(image);

        byte[] bytes = Write(image);
        try
        {
            File.WriteAllBytes(path, bytes);
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException)
        {
            throw PngException.For(path, $"the writer cannot write the file. {fault.Message}", fault);
        }
    }

    /// <summary>Writes a PNG to bytes.</summary>
    /// <param name="image">The image to write.</param>
    /// <returns>The whole file, from the signature to the IEND chunk.</returns>
    public static byte[] Write(PngImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        using MemoryStream file = new();
        file.Write(PngReader.Signature);

        Span<byte> header = stackalloc byte[PngReader.HeaderSize];
        BinaryPrimitives.WriteUInt32BigEndian(header, (uint)image.Width);
        BinaryPrimitives.WriteUInt32BigEndian(header[4..], (uint)image.Height);
        header[8] = 8;
        header[9] = (byte)image.Colors;
        header[10] = 0;
        header[11] = 0;
        header[12] = 0;
        WriteChunk(file, "IHDR", header);

        WriteChunk(file, "IDAT", Compress(Filter(image)));
        WriteChunk(file, "IEND", ReadOnlySpan<byte>.Empty);
        return file.ToArray();
    }

    /// <summary>Puts the filter byte 0 in front of each row of pixels.</summary>
    /// <param name="image">The image to read.</param>
    /// <returns>One filter byte and then one row of pixels, for each row.</returns>
    private static byte[] Filter(PngImage image)
    {
        int stride = image.Stride;
        byte[] filtered = new byte[(stride + 1) * image.Height];
        for (int y = 0; y < image.Height; y += 1)
        {
            int rowStart = y * (stride + 1);
            filtered[rowStart] = PngRowFilter.None;
            image.Row(y).CopyTo(filtered.AsSpan(rowStart + 1, stride));
        }

        return filtered;
    }

    /// <summary>Compresses the filtered bytes as the IDAT chunk holds them.</summary>
    /// <param name="filtered">The filter byte and the row of each row of the image.</param>
    /// <returns>The zlib data of the IDAT chunk.</returns>
    private static byte[] Compress(byte[] filtered)
    {
        using MemoryStream compressed = new();

        // The smallest setting keeps a committed picture, such as the atlas, small in git
        // (D-107). It changes no pixel, and a test compares pixels alone (F-19).
        using (ZLibStream deflate = new(compressed, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            deflate.Write(filtered);
        }

        return compressed.ToArray();
    }

    /// <summary>Writes one chunk: the length, the type, the data, and the CRC-32 (D-663).</summary>
    /// <param name="file">The stream of the file.</param>
    /// <param name="name">The four letters of the chunk type.</param>
    /// <param name="data">The data of the chunk, which can hold no byte.</param>
    private static void WriteChunk(Stream file, string name, ReadOnlySpan<byte> data)
    {
        Span<byte> field = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(field, (uint)data.Length);
        file.Write(field);

        // The CRC-32 covers the type and the data, and never the length (D-663).
        byte[] typeAndData = new byte[4 + data.Length];
        Encoding.ASCII.GetBytes(name, typeAndData);
        data.CopyTo(typeAndData.AsSpan(4));
        file.Write(typeAndData);

        BinaryPrimitives.WriteUInt32BigEndian(field, Crc32.Compute(typeAndData));
        file.Write(field);
    }
}
