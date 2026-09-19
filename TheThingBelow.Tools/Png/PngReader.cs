using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace TheThingBelow.Tools.Png;

/// <summary>
/// The PNG reader of Tools. It reads an 8-bit RGB or RGBA image, and every other kind of PNG
/// fails with the file and the reason (D-176, T-2).
/// </summary>
/// <remarks>
/// The file layout comes from the PNG specification of the W3C,
/// `https://www.w3.org/TR/png-3/`, read 2026-09-14: the signature, then a chain of chunks.
/// Each chunk holds a length, a four-letter type, the data, and a CRC-32 of the type and the
/// data (D-663). The reader skips each chunk that no rule of this project reads, such as the
/// `sRGB` and `iTXt` chunks that an image editor writes, and it fails on a critical chunk
/// that it cannot read.
/// <para>
/// The reader restores every row filter, because an image editor writes filtered rows
/// (D-664). The `ZLibStream` class of .NET decompresses the image data, and that class needs
/// no package.
/// </para>
/// </remarks>
public static class PngReader
{
    /// <summary>The eight bytes that start every PNG file.</summary>
    public static ReadOnlySpan<byte> Signature => [137, 80, 78, 71, 13, 10, 26, 10];

    /// <summary>The length field, the type, and the CRC-32 of one chunk, in bytes.</summary>
    public const int ChunkFrameSize = 12;

    /// <summary>The count of bytes of the IHDR data.</summary>
    public const int HeaderSize = 13;

    /// <summary>The largest count of filtered bytes that the reader decompresses.</summary>
    /// <remarks>
    /// The limit bounds the allocation that a header of a file drives. An atlas of 4096 by
    /// 4096 RGBA pixels needs about 67 MB of filtered bytes, well below this limit (D-107).
    /// </remarks>
    public const int MaxPixelBytes = 256 * 1024 * 1024;

    /// <summary>Reads a PNG file from the disk.</summary>
    /// <param name="path">The path of the file.</param>
    /// <returns>The decoded image.</returns>
    /// <exception cref="PngException">The file is absent, or it is not a PNG of this kind.</exception>
    public static PngImage ReadFile(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        byte[] bytes;
        try
        {
            bytes = File.ReadAllBytes(path);
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException)
        {
            throw PngException.For(path, $"the reader cannot read the file. {fault.Message}", fault);
        }

        return Read(bytes, path);
    }

    /// <summary>Reads a PNG from the bytes of a file.</summary>
    /// <param name="bytes">The whole file, from the signature to the IEND chunk.</param>
    /// <param name="file">The path of the file, or a name for the bytes, for each error (T-2).</param>
    /// <returns>The decoded image.</returns>
    /// <exception cref="PngException">The bytes are not a PNG of this kind.</exception>
    public static PngImage Read(ReadOnlySpan<byte> bytes, string file)
    {
        ArgumentException.ThrowIfNullOrEmpty(file);
        CheckSignature(bytes, file);

        PngHeader header = default;
        bool haveHeader = false;
        bool haveData = false;
        bool dataEnded = false;
        using MemoryStream data = new();

        int offset = Signature.Length;
        while (true)
        {
            PngChunk chunk = NextChunk(bytes, offset, file);
            if (!haveHeader && chunk.Name != "IHDR")
            {
                throw PngException.For(
                    file, $"the first chunk is '{chunk.Name}', and a PNG starts with the IHDR chunk");
            }

            switch (chunk.Name)
            {
                case "IHDR":
                    if (haveHeader)
                    {
                        throw PngException.For(file, "the file holds a second IHDR chunk");
                    }

                    header = ReadHeader(chunk.Data, file);
                    haveHeader = true;
                    break;

                case "IDAT":
                    if (dataEnded)
                    {
                        throw PngException.For(
                            file, "the file holds an IDAT chunk after another chunk, and the IDAT chunks come one after the other");
                    }

                    haveData = true;
                    data.Write(chunk.Data);
                    break;

                case "IEND":
                    if (chunk.Data.Length != 0)
                    {
                        throw PngException.For(
                            file, $"the IEND chunk holds {chunk.Data.Length} bytes of data, and it holds none");
                    }

                    if (!haveData)
                    {
                        throw PngException.For(file, "the file holds no IDAT chunk, so it holds no image data");
                    }

                    break;

                default:
                    // A critical chunk, other than the suggested palette of a PNG of this kind,
                    // changes what the pixels mean. A reader that skips one reads the wrong
                    // image, so the reader stops (T-2).
                    if (chunk.IsCritical && chunk.Name != "PLTE")
                    {
                        throw PngException.For(
                            file, $"the file holds the critical chunk '{chunk.Name}', and this reader reads IHDR, PLTE, IDAT, and IEND");
                    }

                    break;
            }

            if (haveData && chunk.Name != "IDAT")
            {
                dataEnded = true;
            }

            offset += ChunkFrameSize + chunk.Data.Length;
            if (chunk.Name == "IEND")
            {
                break;
            }
        }

        if (offset != bytes.Length)
        {
            throw PngException.For(
                file, $"the file holds {bytes.Length - offset} byte(s) after the IEND chunk");
        }

        byte[] filtered = Inflate(data.ToArray(), header.FilteredSize, file);
        return RestoreRows(filtered, header, file);
    }

    /// <summary>The fields of the IHDR chunk that the reader needs.</summary>
    /// <param name="Width">The count of pixels in one row.</param>
    /// <param name="Height">The count of rows.</param>
    /// <param name="Colors">The color kind of the pixels.</param>
    private readonly record struct PngHeader(int Width, int Height, PngColorKind Colors)
    {
        /// <summary>The count of bytes of one row of pixels, with no filter byte.</summary>
        public int Stride => this.Width * PngImage.BytesPerPixelOf(this.Colors);

        /// <summary>The count of bytes that the image data holds after it decompresses.</summary>
        public int FilteredSize => (this.Stride + 1) * this.Height;
    }

    /// <summary>One chunk of the file: its type, its data, and whether it is critical.</summary>
    private readonly ref struct PngChunk(string name, ReadOnlySpan<byte> data, bool isCritical)
    {
        /// <summary>The four letters of the chunk type, such as `IHDR`.</summary>
        public string Name { get; } = name;

        /// <summary>The data of the chunk, with no length, no type, and no CRC-32.</summary>
        public ReadOnlySpan<byte> Data { get; } = data;

        /// <summary>True when the first letter of the type is a capital letter.</summary>
        public bool IsCritical { get; } = isCritical;
    }

    private static void CheckSignature(ReadOnlySpan<byte> bytes, string file)
    {
        if (bytes.Length < Signature.Length)
        {
            throw PngException.For(
                file, $"the file holds {bytes.Length} byte(s), and the PNG signature alone holds {Signature.Length}");
        }

        if (!bytes[..Signature.Length].SequenceEqual(Signature))
        {
            throw PngException.For(file, "the first bytes of the file are not the PNG signature");
        }
    }

    /// <summary>
    /// Reads the chunk that starts at an offset, and it checks the length and the CRC-32.
    /// </summary>
    /// <param name="bytes">The whole file.</param>
    /// <param name="offset">The offset of the length field of the chunk.</param>
    /// <param name="file">The file, for each error (T-2).</param>
    /// <returns>The type, the data, and the class of the chunk.</returns>
    /// <exception cref="PngException">The chunk is short, its type is not letters, or the CRC-32 fails.</exception>
    private static PngChunk NextChunk(ReadOnlySpan<byte> bytes, int offset, string file)
    {
        if (bytes.Length - offset < ChunkFrameSize)
        {
            throw PngException.For(
                file, $"the file ends {bytes.Length - offset} byte(s) after the offset {offset}, and one chunk needs {ChunkFrameSize} bytes and its data");
        }

        uint declaredLength = BinaryPrimitives.ReadUInt32BigEndian(bytes[offset..]);
        if (declaredLength > int.MaxValue)
        {
            throw PngException.For(
                file, $"a chunk at the offset {offset} names {declaredLength} bytes of data, and the largest chunk holds {int.MaxValue}");
        }

        int length = (int)declaredLength;
        if (bytes.Length - offset - ChunkFrameSize < length)
        {
            throw PngException.For(
                file, $"a chunk at the offset {offset} names {length} bytes of data, and the file holds {bytes.Length - offset - ChunkFrameSize} byte(s) for the data and the CRC-32");
        }

        ReadOnlySpan<byte> typeAndData = bytes.Slice(offset + 4, 4 + length);
        foreach (byte letter in typeAndData[..4])
        {
            if (!char.IsAsciiLetter((char)letter))
            {
                throw PngException.For(
                    file, $"the type of the chunk at the offset {offset} holds the byte {letter}, and a chunk type holds four letters");
            }
        }

        string name = Encoding.ASCII.GetString(typeAndData[..4]);
        uint declaredCrc = BinaryPrimitives.ReadUInt32BigEndian(bytes[(offset + 8 + length)..]);
        uint bytesCrc = Crc32.Compute(typeAndData);
        if (declaredCrc != bytesCrc)
        {
            throw PngException.For(
                file, $"the chunk '{name}' carries the CRC-32 0x{declaredCrc:x8}, and its bytes give 0x{bytesCrc:x8}");
        }

        return new PngChunk(name, typeAndData[4..], (typeAndData[0] & 0x20) == 0);
    }

    /// <summary>Reads the IHDR data, and it refuses every PNG that is not 8-bit RGB or RGBA.</summary>
    /// <param name="data">The data of the IHDR chunk.</param>
    /// <param name="file">The file, for each error (T-2).</param>
    /// <returns>The size and the color kind.</returns>
    /// <exception cref="PngException">A field holds a value that this reader refuses.</exception>
    private static PngHeader ReadHeader(ReadOnlySpan<byte> data, string file)
    {
        if (data.Length != HeaderSize)
        {
            throw PngException.For(
                file, $"the IHDR chunk holds {data.Length} bytes, and it holds {HeaderSize}");
        }

        uint width = BinaryPrimitives.ReadUInt32BigEndian(data);
        uint height = BinaryPrimitives.ReadUInt32BigEndian(data[4..]);
        if (width < 1 || width > PngImage.MaxSize || height < 1 || height > PngImage.MaxSize)
        {
            throw PngException.For(
                file, $"the image is {width} by {height} pixels, and this reader reads 1 to {PngImage.MaxSize} in each direction");
        }

        byte bitDepth = data[8];
        if (bitDepth != 8)
        {
            throw PngException.For(
                file, $"the bit depth is {bitDepth}, and this reader reads the bit depth 8 alone (D-176)");
        }

        byte colorType = data[9];
        if (colorType != (byte)PngColorKind.Rgb && colorType != (byte)PngColorKind.Rgba)
        {
            throw PngException.For(
                file, $"the color type is {colorType} ({ColorTypeName(colorType)}), and this reader reads 2 (RGB) and 6 (RGBA) alone (D-176)");
        }

        if (data[10] != 0)
        {
            throw PngException.For(
                file, $"the compression method is {data[10]}, and the specification defines the method 0 alone");
        }

        if (data[11] != 0)
        {
            throw PngException.For(
                file, $"the filter method is {data[11]}, and the specification defines the method 0 alone");
        }

        if (data[12] != 0)
        {
            throw PngException.For(
                file, $"the interlace method is {data[12]}, and this reader reads an image that is not interlaced (D-176)");
        }

        PngHeader header = new((int)width, (int)height, (PngColorKind)colorType);
        long filtered = ((long)header.Stride + 1) * header.Height;
        if (filtered > MaxPixelBytes)
        {
            throw PngException.For(
                file, $"the image data holds {filtered} bytes after it decompresses, and the limit is {MaxPixelBytes}");
        }

        return header;
    }

    /// <summary>Gives the name of a color type of the specification, for an error message.</summary>
    private static string ColorTypeName(byte colorType) => colorType switch
    {
        0 => "grayscale",
        3 => "indexed",
        4 => "grayscale with alpha",
        _ => "no color type of the specification",
    };

    /// <summary>Decompresses the image data, and it refuses a short or a long result.</summary>
    /// <param name="compressed">The data of every IDAT chunk, one after the other.</param>
    /// <param name="filteredSize">The count of bytes that the IHDR fields need.</param>
    /// <param name="file">The file, for each error (T-2).</param>
    /// <returns>The filtered bytes: one filter byte and then one row, for each row.</returns>
    /// <exception cref="PngException">The data is not zlib data, or it gives another count of bytes.</exception>
    private static byte[] Inflate(byte[] compressed, int filteredSize, string file)
    {
        byte[] filtered = new byte[filteredSize];
        try
        {
            using MemoryStream source = new(compressed, writable: false);
            using ZLibStream inflate = new(source, CompressionMode.Decompress);
            int read = inflate.ReadAtLeast(filtered, filteredSize, throwOnEndOfStream: false);
            if (read < filteredSize)
            {
                throw PngException.For(
                    file, $"the image data gives {read} bytes, and the IHDR fields need {filteredSize}");
            }

            if (inflate.ReadByte() >= 0)
            {
                throw PngException.For(
                    file, $"the image data gives more than the {filteredSize} bytes that the IHDR fields need");
            }
        }
        catch (InvalidDataException fault)
        {
            throw PngException.For(file, $"the image data is not zlib data. {fault.Message}", fault);
        }

        return filtered;
    }

    /// <summary>Turns the filtered bytes into the pixels of the image.</summary>
    /// <param name="filtered">One filter byte and then one row of bytes, for each row.</param>
    /// <param name="header">The size and the color kind of the IHDR chunk.</param>
    /// <param name="file">The file, for each error (T-2).</param>
    /// <returns>The image.</returns>
    /// <exception cref="PngException">A row names a filter that the specification does not define.</exception>
    private static PngImage RestoreRows(byte[] filtered, PngHeader header, string file)
    {
        int stride = header.Stride;
        int bytesPerPixel = PngImage.BytesPerPixelOf(header.Colors);
        byte[] pixels = new byte[stride * header.Height];
        byte[] firstRowAbove = new byte[stride];

        for (int y = 0; y < header.Height; y += 1)
        {
            int rowStart = y * (stride + 1);
            byte filterType = filtered[rowStart];
            Span<byte> row = pixels.AsSpan(y * stride, stride);
            filtered.AsSpan(rowStart + 1, stride).CopyTo(row);
            ReadOnlySpan<byte> above = y == 0 ? firstRowAbove : pixels.AsSpan((y - 1) * stride, stride);
            PngRowFilter.Restore(filterType, row, above, bytesPerPixel, file);
        }

        return new PngImage(header.Width, header.Height, header.Colors, pixels);
    }
}
