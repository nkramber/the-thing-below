using System;
using System.IO;
using System.Text;

namespace TheThingBelow.Storage;

/// <summary>
/// The one write of a file that never destroys the old file: a temporary file beside the
/// target, and then one rename (D-178, D-494).
/// </summary>
/// <remarks>
/// Every file of the game that a later session reads takes this write. A crash, a full disk,
/// or a power loss before the rename thus leaves the old file whole, and the reader of the
/// next session finds a file that one write made (D-178, T-2).
/// <para>
/// The rename replaces the old file in one step, so no reader ever sees a part of a file.
/// The checksum of a save guards the bytes after the rename, against a bad sector and against
/// a hand edit (D-655).
/// </para>
/// </remarks>
public static class SafeWrite
{
    /// <summary>The text that the name of the temporary file adds to the name of the target.</summary>
    public const string TemporarySuffix = ".new";

    /// <summary>Writes the text, and replaces the file at the path with it.</summary>
    /// <param name="path">The full path of the file.</param>
    /// <param name="text">The text of the file, which the writer takes as UTF-8 with no mark.</param>
    /// <exception cref="ArgumentException">The path has no character (T-2).</exception>
    /// <exception cref="ArgumentNullException">The text is null (T-2).</exception>
    /// <exception cref="StorageException">The system refused the write or the rename (T-2).</exception>
    public static void Replace(string path, string text) => Replace(path, text, static () => { });

    /// <summary>
    /// The same write, with a seam between the temporary file and the rename. The test of
    /// the torn write of D-178 stops the write there, and no other caller exists (T-3).
    /// </summary>
    /// <param name="path">The full path of the file.</param>
    /// <param name="text">The text of the file.</param>
    /// <param name="betweenSteps">What runs after the temporary file and before the rename.</param>
    internal static void Replace(string path, string text, Action betweenSteps)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(betweenSteps);

        string temporaryPath = path + TemporarySuffix;
        byte[] bytes = Encoding.UTF8.GetBytes(text);

        try
        {
            using (FileStream file = new(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                file.Write(bytes, 0, bytes.Length);

                // The bytes reach the disk before the rename, so a power loss right after
                // the rename never leaves a file with no content (D-178).
                file.Flush(flushToDisk: true);
            }
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException)
        {
            throw StorageException.ForPath(
                temporaryPath, "the game could not write the temporary file of the write", fault);
        }

        betweenSteps();

        try
        {
            File.Move(temporaryPath, path, overwrite: true);
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException)
        {
            throw StorageException.ForPath(
                path, $"the game could not replace the file with the temporary file '{temporaryPath}'", fault);
        }
    }
}
