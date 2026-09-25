using System;
using System.IO;
using System.Text;

namespace TheThingBelow.Storage;

/// <summary>
/// The one read of the text of a file of the game: a cap on its size, and a strict UTF-8
/// decode that names the path and the byte (T-2).
/// </summary>
/// <remarks>
/// The reads used the replacing decoder of .NET, which turns a bad byte into U+FFFD. The
/// decoded text was then valid, so the strict check of the content reader never saw the bad
/// byte, and a header field or a crash line took the corruption in silence. A read with no cap
/// also took any size into memory.
/// </remarks>
internal static class FileText
{
    /// <summary>The decoder that throws on a byte that is not UTF-8.</summary>
    internal static readonly UTF8Encoding Strict = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    /// <summary>The size of one read of a stream, in bytes.</summary>
    private const int ChunkBytes = 64 * 1024;

    /// <summary>Reads the text of one file.</summary>
    /// <param name="path">The full path of the file.</param>
    /// <param name="mostBytes">The largest size that the kind of the file takes, in bytes.</param>
    /// <param name="what">The kind of the file, for the error, such as `the file of a save`.</param>
    /// <returns>The text.</returns>
    /// <exception cref="StorageException">The file is too large, the system refused the read, or a byte is not UTF-8 (T-2).</exception>
    internal static string Read(string path, long mostBytes, string what)
    {
        try
        {
            using FileStream file = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (file.Length > mostBytes)
            {
                throw StorageException.ForPath(path, $"{what} holds {file.Length} bytes, and the game reads {mostBytes} bytes at most");
            }

            return ReadStream(file, path, mostBytes, what);
        }
        catch (Exception fault) when (StorageFaults.IsFileFault(fault))
        {
            throw StorageException.ForPath(path, $"the game could not read {what}", fault);
        }
    }

    /// <summary>Reads the text of one open stream, and holds no more than the cap in memory.</summary>
    /// <param name="stream">The stream, which can grow while the read runs.</param>
    /// <param name="path">The full path of the file, for the error.</param>
    /// <param name="mostBytes">The largest size that the kind of the file takes, in bytes.</param>
    /// <param name="what">The kind of the file, for the error.</param>
    /// <returns>The text.</returns>
    /// <exception cref="StorageException">The stream holds more bytes than the cap, or a byte is not UTF-8 (T-2).</exception>
    /// <remarks>
    /// A check of the length before the read left a gap: another program could grow the file
    /// after the check, and the read took each byte. The read therefore counts each byte, and it
    /// stops at the first byte past the cap.
    /// </remarks>
    internal static string ReadStream(Stream stream, string path, long mostBytes, string what)
    {
        using MemoryStream bytes = new();
        byte[] chunk = new byte[ChunkBytes];
        long total = 0;
        int read;
        while ((read = stream.Read(chunk, 0, (int)Math.Min(chunk.Length, mostBytes + 1 - total))) > 0)
        {
            total += read;
            if (total > mostBytes)
            {
                throw StorageException.ForPath(path, $"{what} holds more than {mostBytes} bytes, and the game reads {mostBytes} bytes at most");
            }

            bytes.Write(chunk, 0, read);
        }

        try
        {
            return Strict.GetString(bytes.GetBuffer(), 0, (int)bytes.Length);
        }
        catch (DecoderFallbackException fault)
        {
            throw StorageException.ForPath(path, $"{what} holds a byte that is not UTF-8 at byte {fault.Index}", fault);
        }
    }
}
