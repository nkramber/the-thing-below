using System;
using System.IO;
using System.Text;
using TheThingBelow.Core.Saves;

namespace TheThingBelow.Storage;

/// <summary>The three saves of the game (D-62, D-258, D-656).</summary>
public enum SaveKind
{
    /// <summary>The one slot save, which a save point writes by the choice of the player (D-62).</summary>
    Slot,

    /// <summary>The autosave, which a hub and a node of the region map write (D-224).</summary>
    Autosave,

    /// <summary>The resume file of a quit, which one load reads and removes (D-258).</summary>
    Resume,
}

/// <summary>
/// The three save files in one folder (D-62, D-258, D-656). It reads and writes the files,
/// and Core makes and reads their text (D-494).
/// </summary>
/// <remarks>
/// Every write takes the safe write of D-178, so a crash during a save leaves the old save
/// whole. Every read gives the header and the snapshot of the save, and a load resumes the
/// run from that snapshot alone (D-259).
/// <para>
/// The moment of each save is the work of Game, and PR-16 sets it (D-224, D-651).
/// </para>
/// </remarks>
public sealed class SaveStore
{
    private readonly string folder;

    /// <summary>Makes a store of the saves in one folder.</summary>
    /// <param name="folder">The full path of the folder of the saves.</param>
    /// <exception cref="ArgumentException">The folder has no character (T-2).</exception>
    public SaveStore(string folder)
    {
        ArgumentException.ThrowIfNullOrEmpty(folder);

        this.folder = folder;
    }

    /// <summary>The folder that holds the three saves.</summary>
    public string Folder => this.folder;

    /// <summary>Makes the store of the saves of the person on this system (D-465, D-656).</summary>
    /// <returns>The store.</returns>
    /// <exception cref="StorageException">The environment names no data folder (T-2).</exception>
    public static SaveStore OfThisSystem() => new(SaveFolder.SavesOfThisSystem());

    /// <summary>Gives the file name of one save (D-656).</summary>
    /// <param name="kind">The save.</param>
    /// <returns>The name of the file, with the `.json` extension of its text (D-652).</returns>
    /// <exception cref="ArgumentOutOfRangeException">The game holds no such save (T-2).</exception>
    public static string FileNameOf(SaveKind kind) => kind switch
    {
        SaveKind.Slot => "slot.json",
        SaveKind.Autosave => "autosave.json",
        SaveKind.Resume => "resume.json",
        _ => throw new ArgumentOutOfRangeException(
            nameof(kind), kind, $"The game holds no save with the number {(int)kind} (D-62, D-258)."),
    };

    /// <summary>Gives the full path of one save.</summary>
    /// <param name="kind">The save.</param>
    /// <returns>The path of the file, in the folder of this store.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The game holds no such save (T-2).</exception>
    public string PathOf(SaveKind kind) => Path.Combine(this.folder, FileNameOf(kind));

    /// <summary>Says whether one save exists.</summary>
    /// <param name="kind">The save.</param>
    /// <returns>True when the file exists.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The game holds no such save (T-2).</exception>
    public bool Exists(SaveKind kind) => File.Exists(this.PathOf(kind));

    /// <summary>Writes one save, and keeps the old one until the write is whole (D-178).</summary>
    /// <param name="kind">The save.</param>
    /// <param name="document">The header and the snapshot of the run (D-259).</param>
    /// <exception cref="ArgumentNullException">The document is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The game holds no such save (T-2).</exception>
    /// <exception cref="StorageException">The system refused the folder or the write (T-2).</exception>
    public void Write(SaveKind kind, SaveDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        string path = this.PathOf(kind);
        this.MakeFolder();
        SafeWrite.Replace(path, SaveText.Write(document));
    }

    /// <summary>Reads one save, and leaves the file (D-258).</summary>
    /// <param name="kind">The save.</param>
    /// <returns>The header and the snapshot of the save.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The game holds no such save (T-2).</exception>
    /// <exception cref="StorageException">The file is absent, or the system refused it (T-2).</exception>
    /// <exception cref="SaveException">The text of the file breaks a rule of a save (T-2).</exception>
    /// <remarks>
    /// A read never removes the resume file. A file that parses can still fail the resume, for
    /// example against the content of a newer build, and the player then sends it with a report.
    /// The caller removes it with <see cref="RemoveResume"/> after the run resumed (D-258, T-2).
    /// </remarks>
    public SaveDocument Read(SaveKind kind)
    {
        string path = this.PathOf(kind);
        if (!File.Exists(path))
        {
            string extra = kind == SaveKind.Resume
                ? ". A load of a resume file removes it, so one resume file serves one load (D-258)"
                : string.Empty;
            throw StorageException.ForPath(path, $"the game found no file for {Describe(kind)}{extra}");
        }

        return SaveText.Read(ReadText(path), path);
    }

    /// <summary>Removes the resume file after the run resumed from it, so one resume file serves one load (D-258).</summary>
    /// <exception cref="StorageException">The file is absent, or the system refused the removal (T-2).</exception>
    public void RemoveResume()
    {
        string path = this.PathOf(SaveKind.Resume);
        if (!File.Exists(path))
        {
            throw StorageException.ForPath(path, "the game found no resume file to remove after the resume (D-258)");
        }

        Remove(path);
    }

    private static string Describe(SaveKind kind) => kind switch
    {
        SaveKind.Slot => "the slot save",
        SaveKind.Autosave => "the autosave",
        SaveKind.Resume => "the resume file",
        _ => throw new ArgumentOutOfRangeException(
            nameof(kind), kind, $"The game holds no save with the number {(int)kind} (D-62, D-258)."),
    };

    private static string ReadText(string path)
    {
        try
        {
            return Encoding.UTF8.GetString(File.ReadAllBytes(path));
        }
        catch (Exception fault) when (StorageFaults.IsFileFault(fault))
        {
            throw StorageException.ForPath(path, "the game could not read the file of a save", fault);
        }
    }

    private static void Remove(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception fault) when (StorageFaults.IsFileFault(fault))
        {
            throw StorageException.ForPath(path, "the run resumed from the resume file, and the game could not remove it (D-258)", fault);
        }
    }

    private void MakeFolder()
    {
        try
        {
            Directory.CreateDirectory(this.folder);
        }
        catch (Exception fault) when (StorageFaults.IsFileFault(fault))
        {
            throw StorageException.ForPath(this.folder, "the game could not make the folder of the saves", fault);
        }
    }
}
