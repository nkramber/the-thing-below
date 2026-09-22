using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TheThingBelow.Storage;

/// <summary>
/// The settings file, `settings.json`, at the root of the folder of the game (D-860). The
/// file sits beside the folder of the saves, so a removal of the saves keeps the settings.
/// </summary>
/// <remarks>
/// Every write takes the safe write of D-178, so a crash during a write leaves the old file
/// whole. No setting reaches a run record, because Core takes no reference to Storage (T-7).
/// </remarks>
public sealed class SettingsStore
{
    /// <summary>The name of the settings file (D-860).</summary>
    public const string FileName = "settings.json";

    private readonly string folder;

    /// <summary>Makes the store of the settings file in one folder.</summary>
    /// <param name="folder">The full path of the folder of the game.</param>
    /// <exception cref="ArgumentException">The folder has no character (T-2).</exception>
    public SettingsStore(string folder)
    {
        ArgumentException.ThrowIfNullOrEmpty(folder);

        this.folder = folder;
    }

    /// <summary>The full path of the settings file.</summary>
    public string Path => System.IO.Path.Combine(this.folder, FileName);

    /// <summary>Makes the store of the person on this system (D-465, D-860).</summary>
    /// <returns>The store.</returns>
    /// <exception cref="StorageException">The environment names no data folder (T-2).</exception>
    public static SettingsStore OfThisSystem() => new(SaveFolder.OfThisSystem());

    /// <summary>Says whether the settings file exists. A first start finds none.</summary>
    /// <returns>True when the file exists.</returns>
    public bool Exists() => File.Exists(this.Path);

    /// <summary>Reads the settings file.</summary>
    /// <returns>The settings.</returns>
    /// <exception cref="StorageException">
    /// The file is absent, the system refused the read, or the text breaks a rule (T-2).
    /// </exception>
    public GameSettings Read()
    {
        string path = this.Path;
        if (!File.Exists(path))
        {
            throw StorageException.ForPath(path, "the game found no settings file");
        }

        string text;
        try
        {
            text = Encoding.UTF8.GetString(File.ReadAllBytes(path));
        }
        catch (Exception fault) when (StorageFaults.IsFileFault(fault))
        {
            throw StorageException.ForPath(path, "the game could not read the settings file", fault);
        }

        return SettingsText.Read(text, path);
    }

    /// <summary>Writes the settings file, and keeps the old one until the write is whole (D-178).</summary>
    /// <param name="settings">The settings.</param>
    /// <exception cref="ArgumentNullException">The settings are null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">A value is outside its range (T-2).</exception>
    /// <exception cref="InvalidOperationException">
    /// Two actions hold one binding. The settings screen refuses that save first (D-862).
    /// </exception>
    /// <exception cref="StorageException">The system refused the folder or the write (T-2).</exception>
    public void Write(GameSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        IReadOnlyList<BindingConflict> conflicts = settings.Controls.Bindings.FindConflicts();
        if (conflicts.Count > 0)
        {
            BindingConflict first = conflicts[0];
            throw new InvalidOperationException(
                $"The settings hold {conflicts.Count} binding conflicts, and a save waits until none remains. " +
                $"The first is {first.Binding} on {string.Join(" and ", first.Actions)} (D-862).");
        }

        string text = SettingsText.Write(settings);
        try
        {
            Directory.CreateDirectory(this.folder);
        }
        catch (Exception fault) when (StorageFaults.IsFileFault(fault))
        {
            throw StorageException.ForPath(this.folder, "the game could not make the folder of the settings file", fault);
        }

        SafeWrite.Replace(this.Path, text);
    }
}
