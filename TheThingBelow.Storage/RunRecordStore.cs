using System;
using System.IO;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Storage;

/// <summary>
/// The files of run records in one folder, such as the records of the failed bot runs that the bot
/// job uploads (D-494, D-1181, T-7). It writes a file and reads it, and Core makes and reads its
/// text.
/// </summary>
/// <remarks>
/// The caller names each file, because the name of a bot record carries the policy and the seed
/// of its run. The folder keeps every file, and the caller owns the folder.
/// </remarks>
public sealed class RunRecordStore
{
    /// <summary>The file type of a record, which is the line text of `RunRecordText` (D-651).</summary>
    public const string FileExtension = ".record";

    /// <summary>The largest record file that the store reads, in bytes: 256 MiB, as for a crash file.</summary>
    public const long MostBytes = 256L * 1024 * 1024;

    private readonly string folder;

    /// <summary>Makes a store of record files in one folder.</summary>
    /// <param name="folder">The full path of the folder.</param>
    /// <exception cref="ArgumentException">The folder has no character (T-2).</exception>
    public RunRecordStore(string folder)
    {
        ArgumentException.ThrowIfNullOrEmpty(folder);

        this.folder = folder;
    }

    /// <summary>The folder that holds the record files.</summary>
    public string Folder => this.folder;

    /// <summary>Writes one record file, and replaces a file of the same name (D-178).</summary>
    /// <param name="name">The name of the file with no folder and no file type, such as `random-0000002a`.</param>
    /// <param name="record">The record.</param>
    /// <returns>The full path of the file.</returns>
    /// <exception cref="ArgumentException">The name has no character, or it holds a character that no file name takes (T-2).</exception>
    /// <exception cref="ArgumentNullException">The record is null (T-2).</exception>
    /// <exception cref="StorageException">The system refused the folder or the write (T-2).</exception>
    public string Write(string name, RunRecord record)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(record);
        if (name.IndexOfAny(['/', '\\', ':', '.']) >= 0)
        {
            throw new ArgumentException($"The record name '{name}' holds a folder, a drive, or a file type, and a name holds none of them (T-2).", nameof(name));
        }

        FolderFiles.MakeFolder(this.folder);
        string path = Path.Combine(this.folder, name + FileExtension);

        // The safe write of D-178 gives the file in one step, and its check reads the text back
        // as a record, so a torn file never reaches the upload (T-2).
        SafeWrite.Replace(path, RunRecordText.Write(record), text => RunRecordText.Read(text));
        return path;
    }

    /// <summary>Reads one record file.</summary>
    /// <param name="path">The full path of the file.</param>
    /// <returns>The record.</returns>
    /// <exception cref="ArgumentException">The path has no character (T-2).</exception>
    /// <exception cref="StorageException">The file is absent, too large, or the system refused it (T-2).</exception>
    /// <exception cref="RunRecordException">The text of the file breaks a rule of a record (T-2).</exception>
    public static RunRecord Read(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        if (!File.Exists(path))
        {
            throw StorageException.ForPath(path, "the store found no record file");
        }

        return RunRecordText.Read(FileText.Read(path, MostBytes, "the file of a run record"));
    }
}
