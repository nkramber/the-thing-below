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

    /// <summary>Reads the text of one file.</summary>
    /// <param name="path">The full path of the file.</param>
    /// <param name="mostBytes">The largest size that the kind of the file takes, in bytes.</param>
    /// <param name="what">The kind of the file, for the error, such as `the file of a save`.</param>
    /// <returns>The text.</returns>
    /// <exception cref="StorageException">The file is too large, the system refused the read, or a byte is not UTF-8 (T-2).</exception>
    internal static string Read(string path, long mostBytes, string what)
    {
        byte[] bytes;
        try
        {
            long length = new FileInfo(path).Length;
            if (length > mostBytes)
            {
                throw StorageException.ForPath(path, $"{what} holds {length} bytes, and the game reads {mostBytes} bytes at most");
            }

            bytes = File.ReadAllBytes(path);
        }
        catch (Exception fault) when (StorageFaults.IsFileFault(fault))
        {
            throw StorageException.ForPath(path, $"the game could not read {what}", fault);
        }

        try
        {
            return Strict.GetString(bytes);
        }
        catch (DecoderFallbackException fault)
        {
            throw StorageException.ForPath(path, $"{what} holds a byte that is not UTF-8 at byte {fault.Index}", fault);
        }
    }
}
