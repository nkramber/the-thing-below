using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TheThingBelow.Core.Crashes;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Storage;

/// <summary>
/// The crash files, in the crashes folder beside the saves folder (D-170, D-658). It writes
/// the file and reads it, and Core makes and reads its text (D-494).
/// </summary>
/// <remarks>
/// A crash file holds the error with its context, the versions, and the run record, and no
/// personal data (D-170). The writer hides every folder of the person in the text of the error
/// and of the stack, because a file error carries its path (T-2).
/// <para>
/// The folder keeps the newest <see cref="KeepCount"/> files, and each write removes the older
/// ones (D-659). PR-61 shows the folder and the address of D-473 on screen, through the text
/// helper (D-559).
/// </para>
/// </remarks>
public sealed class CrashStore
{
    /// <summary>The name of the folder inside the folder of the game (D-658).</summary>
    public const string FolderName = "crashes";

    /// <summary>The first part of the name of a crash file (D-658).</summary>
    public const string FilePrefix = "crash";

    /// <summary>The file type of a crash file, which is JSON text (D-652).</summary>
    public const string FileExtension = ".json";

    /// <summary>The count of crash files that the folder keeps (D-659).</summary>
    public const int KeepCount = 10;

    private readonly string folder;

    /// <summary>Makes a store of the crash files in one folder.</summary>
    /// <param name="folder">The full path of the folder of the crash files.</param>
    /// <exception cref="ArgumentException">The folder has no character (T-2).</exception>
    public CrashStore(string folder)
    {
        ArgumentException.ThrowIfNullOrEmpty(folder);

        this.folder = folder;
    }

    /// <summary>The folder that holds the crash files.</summary>
    public string Folder => this.folder;

    /// <summary>Makes the store of the crash files of the person on this system (D-465, D-658).</summary>
    /// <returns>The store.</returns>
    /// <exception cref="StorageException">The environment names no data folder (T-2).</exception>
    public static CrashStore OfThisSystem() =>
        new(Path.Combine(SaveFolder.OfThisSystem(), FolderName));

    /// <summary>Writes one crash file (D-170).</summary>
    /// <param name="fault">The error that stopped the game.</param>
    /// <param name="record">The record of the run, or null when no run exists (D-170).</param>
    /// <param name="time">The wall-clock time of the crash, in UTC, which the host read (G-3).</param>
    /// <returns>The full path of the file that the game wrote.</returns>
    /// <exception cref="ArgumentNullException">The error is null (T-2).</exception>
    /// <exception cref="ArgumentException">The time is not a UTC time (T-2).</exception>
    /// <exception cref="StorageException">The system refused the folder, the write, or a removal (T-2).</exception>
    public string Write(Exception fault, RunRecord? record, DateTime time)
    {
        ArgumentNullException.ThrowIfNull(fault);

        string stamp = TimeText.Stamp(time);
        FolderFiles.MakeFolder(this.folder);
        string path = FolderFiles.FreePath(this.folder, FilePrefix, stamp, FileExtension);

        // Core runs with no reflection, so the store reads the name of the type of the error
        // and gives it to the report (F-36, D-647).
        CrashReport report = CrashReport.Of(fault, fault.GetType().Name, TimeText.Moment(time), record);
        CrashReport hidden = report with
        {
            Error = PersonalPaths.Hide(report.Error),
            Stack = PersonalPaths.Hide(report.Stack),
        };

        // The safe write of D-178 gives the file in one step, so a reader of the folder never
        // finds a part of a crash file, also when the crash came from a full disk (T-2).
        SafeWrite.Replace(path, CrashText.Write(hidden));

        FolderFiles.KeepNewest(this.folder, FilePrefix, FileExtension, KeepCount, path);
        FolderFiles.RemoveTemporaryFiles(this.folder, FilePrefix, FileExtension);
        return path;
    }

    /// <summary>Reads one crash file.</summary>
    /// <param name="path">The full path of the file.</param>
    /// <returns>The crash and the record of the run.</returns>
    /// <exception cref="ArgumentException">The path has no character (T-2).</exception>
    /// <exception cref="StorageException">The file is absent, or the system refused it (T-2).</exception>
    /// <exception cref="CrashException">The text of the file breaks a rule of a crash file (T-2).</exception>
    public CrashReport Read(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        if (!File.Exists(path))
        {
            throw StorageException.ForPath(path, "the game found no crash file");
        }

        try
        {
            return CrashText.Read(Encoding.UTF8.GetString(File.ReadAllBytes(path)), path);
        }
        catch (Exception fault) when (StorageFaults.IsFileFault(fault))
        {
            throw StorageException.ForPath(path, "the game could not read the file of a crash", fault);
        }
    }

    /// <summary>Gives the names of the crash files, the newest one first (D-659).</summary>
    /// <returns>The names with no folder. The list is empty when the folder is absent.</returns>
    /// <exception cref="StorageException">The system refused the read (T-2).</exception>
    public IReadOnlyList<string> Names() => FolderFiles.Names(this.folder, FilePrefix, FileExtension);
}
