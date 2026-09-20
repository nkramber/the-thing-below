using System;
using System.Collections.Generic;

namespace TheThingBelow.Storage;

/// <summary>
/// Hides the folders of the person in a text that a crash file carries (D-170).
/// </summary>
/// <remarks>
/// A crash file goes to the studio by mail, and it holds no personal data (D-170, D-473). A
/// file error carries its path, and that path holds the name of the account of the person, so
/// the writer of a crash file replaces each folder of the person with
/// <see cref="Placeholder"/> (T-2).
/// <para>
/// The rule reads the folders of the environment, and it replaces the longest one first, so a
/// folder inside another folder never leaves a part of the outer path in the text.
/// </para>
/// </remarks>
internal static class PersonalPaths
{
    /// <summary>
    /// The text that stands in the place of a folder of the person. The text holds no
    /// character that the JSON writer escapes, so a person reads the path in the file (D-170).
    /// </summary>
    internal const string Placeholder = "(user-folder)";

    /// <summary>
    /// The shortest folder that the rule replaces. A shorter value, such as a home folder of
    /// one character, would match text that names no folder at all.
    /// </summary>
    internal const int ShortestFolder = 4;

    /// <summary>The variable that names the profile folder of the person on Windows.</summary>
    internal const string WindowsProfileVariable = "USERPROFILE";

    /// <summary>Hides every folder of the person of this system in the text (D-170).</summary>
    /// <param name="text">The text of an error, of a stack, or of a message.</param>
    /// <returns>The text, with each folder of the person replaced.</returns>
    /// <exception cref="ArgumentNullException">The text is null (T-2).</exception>
    internal static string Hide(string text) => Hide(text, FoldersOfThisSystem());

    /// <summary>Hides the folders of a list in the text.</summary>
    /// <param name="text">The text of an error, of a stack, or of a message.</param>
    /// <param name="folders">The folders of the person. An empty or a short value is skipped.</param>
    /// <returns>The text, with each folder replaced.</returns>
    /// <exception cref="ArgumentNullException">The text or the list is null (T-2).</exception>
    internal static string Hide(string text, IReadOnlyList<string> folders)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(folders);

        // Windows reads a path without case, and an error of the engine and an error of the
        // file code can spell one folder two ways.
        StringComparison comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        string hidden = text;
        foreach (string folder in Longest(folders))
        {
            hidden = hidden.Replace(folder, Placeholder, comparison);

            // Godot writes a slash in every path, also on Windows, and the environment of
            // Windows writes a backslash. Thus the rule reads both spellings (F-33).
            string slashes = folder.Replace('\\', '/');
            if (string.CompareOrdinal(slashes, folder) != 0)
            {
                hidden = hidden.Replace(slashes, Placeholder, comparison);
            }
        }

        return hidden;
    }

    /// <summary>Gives the folders of the person that this system names (D-465).</summary>
    /// <returns>The folders, with no empty value and no short value.</returns>
    internal static IReadOnlyList<string> FoldersOfThisSystem()
    {
        List<string> folders = [];
        AddVariable(folders, SaveFolder.WindowsVariable);
        AddVariable(folders, WindowsProfileVariable);
        AddVariable(folders, SaveFolder.LinuxDataVariable);
        AddVariable(folders, SaveFolder.HomeVariable);
        return folders;
    }

    private static void AddVariable(List<string> folders, string variable)
    {
        string? value = Environment.GetEnvironmentVariable(variable);
        if (!string.IsNullOrEmpty(value) && value.Length >= ShortestFolder)
        {
            folders.Add(value.TrimEnd('/', '\\'));
        }
    }

    /// <summary>
    /// Gives the folders in the order of the longest one first, so the game folder inside the
    /// data folder never leaves the data folder in the text.
    /// </summary>
    private static IReadOnlyList<string> Longest(IReadOnlyList<string> folders)
    {
        List<string> sorted = [];
        foreach (string folder in folders)
        {
            if (!string.IsNullOrEmpty(folder) && folder.Length >= ShortestFolder)
            {
                sorted.Add(folder);
            }
        }

        // The list holds four folders at most, so one insertion sort reads clearly and needs
        // no comparer (T-1).
        for (int index = 1; index < sorted.Count; index += 1)
        {
            string folder = sorted[index];
            int place = index;
            while (place > 0 && sorted[place - 1].Length < folder.Length)
            {
                sorted[place] = sorted[place - 1];
                place -= 1;
            }

            sorted[place] = folder;
        }

        return sorted;
    }
}
