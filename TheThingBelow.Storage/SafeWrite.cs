using System;
using System.IO;
using System.Text;
using System.Threading;

namespace TheThingBelow.Storage;

/// <summary>
/// The one write of a file that never destroys the old file: a temporary file beside the
/// target, a check of that file, and then one rename (D-178, D-494).
/// </summary>
/// <remarks>
/// Every file of the game that a later session reads takes this write. A crash, a full disk,
/// or a power loss before the rename thus leaves the old file whole, and the reader of the
/// next session finds a file that one write made (D-178, T-2).
/// <para>
/// D-178 checks the temporary file before the rename. The write reads the file back, compares
/// each byte, and gives the text to the reader of its kind. A write that the reader refuses,
/// or a bad sector under the new file, thus never replaces the last good file (T-2).
/// </para>
/// <para>
/// The rename replaces the old file in one step, so no reader ever sees a part of a file.
/// Windows refuses the rename while another program, such as a virus scan or a backup, holds
/// the target open, so a refused rename tries again a few times before it fails. The checksum
/// of a save guards the bytes after the rename, against a bad sector and against a hand edit
/// (D-655).
/// </para>
/// </remarks>
public static class SafeWrite
{
    /// <summary>The text that the name of the temporary file adds to the name of the target.</summary>
    public const string TemporarySuffix = ".new";

    /// <summary>The count of tries of the rename, before the write fails.</summary>
    public const int MoveTries = 5;

    /// <summary>The wait after the first refused rename, in milliseconds. Each later wait adds the same time.</summary>
    public const int MoveWaitMilliseconds = 40;

    /// <summary>Writes the text, checks it, and replaces the file at the path with it.</summary>
    /// <param name="path">The full path of the file.</param>
    /// <param name="text">The text of the file, which the writer takes as UTF-8 with no mark.</param>
    /// <param name="check">
    /// Reads the text of the temporary file with the reader of its kind, and throws when the
    /// reader refuses it, such as `text => SaveText.Read(text, path)`.
    /// </param>
    /// <exception cref="ArgumentException">The path has no character (T-2).</exception>
    /// <exception cref="ArgumentNullException">The text or the check is null (T-2).</exception>
    /// <exception cref="StorageException">
    /// The system refused the write or the rename, or the temporary file failed its check. The
    /// old file stays whole in each case (T-2).
    /// </exception>
    public static void Replace(string path, string text, Action<string> check) =>
        Replace(path, text, check, static () => { }, static (from, to) => File.Move(from, to, overwrite: true));

    /// <summary>
    /// The same write, with two seams. A test stops or changes the write between the temporary
    /// file and its check, and a test makes the rename fail (T-3). No other caller exists.
    /// </summary>
    /// <param name="path">The full path of the file.</param>
    /// <param name="text">The text of the file.</param>
    /// <param name="check">The reader of the kind of the file.</param>
    /// <param name="betweenSteps">What runs after the temporary file and before its check.</param>
    /// <param name="move">The rename of the temporary file over the file.</param>
    internal static void Replace(string path, string text, Action<string> check, Action betweenSteps, Action<string, string> move)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(check);
        ArgumentNullException.ThrowIfNull(betweenSteps);
        ArgumentNullException.ThrowIfNull(move);

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
        catch (Exception fault) when (StorageFaults.IsFileFault(fault))
        {
            throw StorageException.ForPath(
                temporaryPath, "the game could not write the temporary file of the write", fault);
        }

        betweenSteps();
        CheckTemporaryFile(temporaryPath, bytes, check);
        MoveWithTries(temporaryPath, path, move);
    }

    /// <summary>Reads the temporary file back, and gives its text to the reader of its kind (D-178).</summary>
    /// <exception cref="StorageException">The file holds other bytes, or the reader refused it. The file is gone then (T-2).</exception>
    private static void CheckTemporaryFile(string temporaryPath, byte[] written, Action<string> check)
    {
        string? refusal;
        Exception? inner = null;
        try
        {
            byte[] read = File.ReadAllBytes(temporaryPath);
            refusal = read.AsSpan().SequenceEqual(written)
                ? null
                : $"the temporary file holds {read.Length} bytes that differ from the {written.Length} bytes of the write";
            if (refusal is null)
            {
                check(FileText.Strict.GetString(read));
            }
        }
        catch (Exception fault) when (fault is not OutOfMemoryException)
        {
            // Each reader throws the error type of its kind, and the check turns each one into
            // the error of the write, with the reason inside it (T-2).
            refusal = "the temporary file failed the check of its reader";
            inner = fault;
        }

        if (refusal is null)
        {
            return;
        }

        try
        {
            File.Delete(temporaryPath);
        }
        catch (Exception fault) when (StorageFaults.IsFileFault(fault))
        {
            refusal += $", and the game could not remove it ({fault.GetType().Name})";
        }

        throw inner is null
            ? StorageException.ForPath(temporaryPath, $"{refusal}, so the old file stays whole (D-178)")
            : StorageException.ForPath(temporaryPath, $"{refusal}, so the old file stays whole (D-178)", inner);
    }

    /// <summary>Renames the temporary file over the file, and tries again after a refusal of the system.</summary>
    /// <exception cref="StorageException">Each try failed. The error holds the last refusal (T-2).</exception>
    private static void MoveWithTries(string temporaryPath, string path, Action<string, string> move)
    {
        for (int attempt = 1; ; attempt += 1)
        {
            try
            {
                move(temporaryPath, path);
                return;
            }
            catch (Exception fault) when (StorageFaults.IsFileFault(fault))
            {
                if (attempt == MoveTries)
                {
                    throw StorageException.ForPath(
                        path,
                        $"the game could not replace the file with the temporary file '{temporaryPath}' in {MoveTries} tries",
                        fault);
                }

                Thread.Sleep(MoveWaitMilliseconds * attempt);
            }
        }
    }
}
