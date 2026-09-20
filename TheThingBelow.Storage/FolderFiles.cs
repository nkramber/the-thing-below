using System;
using System.Collections.Generic;
using System.IO;

namespace TheThingBelow.Storage;

/// <summary>
/// The file rules that the crash folder and the log folder share: one folder, one free name,
/// and the newest files alone (D-658, D-659).
/// </summary>
/// <remarks>
/// Each name is the prefix of the folder, the stamp of the time, and the file type, such as
/// `crash-20260918-014253.json` (D-658). Two files of one second take a count after the
/// stamp, so a write never replaces a file that another write made (T-2).
/// </remarks>
internal static class FolderFiles
{
    /// <summary>The count of names that one stamp can carry, before the write fails (T-2).</summary>
    internal const int MaxNamesOfOneStamp = 100;

    /// <summary>Makes the folder when it is absent.</summary>
    /// <param name="folder">The full path of the folder.</param>
    /// <exception cref="StorageException">The system refused the folder (T-2).</exception>
    internal static void MakeFolder(string folder)
    {
        try
        {
            Directory.CreateDirectory(folder);
        }
        catch (Exception fault) when (StorageFaults.IsFileFault(fault))
        {
            throw StorageException.ForPath(folder, "the game could not make the folder", fault);
        }
    }

    /// <summary>Gives the path of a file that no file of the folder holds.</summary>
    /// <param name="folder">The full path of the folder, which exists.</param>
    /// <param name="prefix">The prefix of the name, such as `crash`.</param>
    /// <param name="stamp">The stamp of the time (<see cref="TimeText.Stamp"/>).</param>
    /// <param name="extension">The file type, with its point, such as `.json`.</param>
    /// <returns>The path, with a count after the stamp when the first name is taken.</returns>
    /// <exception cref="StorageException">
    /// The folder holds every name of that stamp, or the system refused the read (T-2).
    /// </exception>
    internal static string FreePath(string folder, string prefix, string stamp, string extension)
    {
        // `File.Exists` throws nothing. It gives false for a path that the system refuses,
        // and the write that follows then fails with the path (T-2).
        string first = Path.Combine(folder, $"{prefix}-{stamp}{extension}");
        if (!File.Exists(first))
        {
            return first;
        }

        for (int count = 2; count < MaxNamesOfOneStamp; count += 1)
        {
            string path = Path.Combine(folder, $"{prefix}-{stamp}-{count}{extension}");
            if (!File.Exists(path))
            {
                return path;
            }
        }

        throw StorageException.ForPath(
            first,
            $"the folder holds every name of the stamp '{stamp}', and one stamp carries {MaxNamesOfOneStamp - 1} files");
    }

    /// <summary>Removes every file of the prefix after the newest ones (D-659).</summary>
    /// <param name="folder">The full path of the folder.</param>
    /// <param name="prefix">The prefix of the name, such as `crash`.</param>
    /// <param name="extension">The file type, with its point, such as `.json`.</param>
    /// <param name="keep">The count of files that stay. It is 1 at least.</param>
    /// <param name="newest">The full path of the file that the caller made now, which stays.</param>
    /// <exception cref="ArgumentOutOfRangeException">The count is below 1 (T-2).</exception>
    /// <exception cref="StorageException">The system refused the read or a removal (T-2).</exception>
    /// <remarks>
    /// The stamp of a name sorts as the time sorts, so the order of the names is the order of
    /// the times (D-658). Two files of one second can take either order, and the count of the
    /// folder holds either way. The file of the caller stays also when the clock of the
    /// machine runs behind the stamps of the older files, so a write never removes its own
    /// file (T-2).
    /// </remarks>
    internal static void KeepNewest(string folder, string prefix, string extension, int keep, string newest)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(keep, 1);
        ArgumentException.ThrowIfNullOrEmpty(newest);

        IReadOnlyList<string> names = Names(folder, prefix, extension);
        if (names.Count <= keep)
        {
            return;
        }

        string newestName = Path.GetFileName(newest);
        for (int index = keep; index < names.Count; index += 1)
        {
            if (string.CompareOrdinal(names[index], newestName) == 0)
            {
                continue;
            }

            string path = Path.Combine(folder, names[index]);
            try
            {
                File.Delete(path);
            }
            catch (Exception fault) when (StorageFaults.IsFileFault(fault))
            {
                throw StorageException.ForPath(
                    path, $"the game could not remove the file after the newest {keep} files (D-659)", fault);
            }
        }
    }

    /// <summary>
    /// Removes each temporary file that a torn safe write left in the folder (D-178). The
    /// name pattern of the folder never matches one, so no other removal reaches it.
    /// </summary>
    /// <param name="folder">The full path of the folder.</param>
    /// <param name="prefix">The prefix of the name, such as `crash`.</param>
    /// <param name="extension">The file type, with its point, such as `.json`.</param>
    /// <exception cref="StorageException">The system refused the read or a removal (T-2).</exception>
    internal static void RemoveTemporaryFiles(string folder, string prefix, string extension)
    {
        if (!Directory.Exists(folder))
        {
            return;
        }

        string[] paths;
        try
        {
            paths = Directory.GetFiles(folder, $"{prefix}-*{extension}{SafeWrite.TemporarySuffix}");
        }
        catch (Exception fault) when (StorageFaults.IsFileFault(fault))
        {
            throw StorageException.ForPath(folder, "the game could not read the files of the folder", fault);
        }

        foreach (string path in paths)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception fault) when (StorageFaults.IsFileFault(fault))
            {
                throw StorageException.ForPath(
                    path, "the game could not remove the temporary file of a torn write (D-178)", fault);
            }
        }
    }

    /// <summary>Gives the names of the folder, the newest one first (D-659).</summary>
    /// <param name="folder">The full path of the folder.</param>
    /// <param name="prefix">The prefix of the name, such as `crash`.</param>
    /// <param name="extension">The file type, with its point, such as `.json`.</param>
    /// <returns>The names with no folder, in the order of the newest one first.</returns>
    /// <exception cref="StorageException">The system refused the read (T-2).</exception>
    internal static IReadOnlyList<string> Names(string folder, string prefix, string extension)
    {
        if (!Directory.Exists(folder))
        {
            return [];
        }

        string[] paths;
        try
        {
            paths = Directory.GetFiles(folder, $"{prefix}-*{extension}");
        }
        catch (Exception fault) when (StorageFaults.IsFileFault(fault))
        {
            throw StorageException.ForPath(folder, "the game could not read the files of the folder", fault);
        }

        List<string> names = [];
        foreach (string path in paths)
        {
            names.Add(Path.GetFileName(path));
        }

        // The order of the file system is not the order of the names, so the sort is the rule
        // of the order and never the order that the system gave (T-7).
        names.Sort(static (first, second) => string.CompareOrdinal(second, first));
        return names;
    }
}
