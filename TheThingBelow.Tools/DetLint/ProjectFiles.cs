using System;
using System.Collections.Generic;
using System.IO;

namespace TheThingBelow.Tools.DetLint;

/// <summary>The files of one project that the lint reads, with paths from the checkout root.</summary>
public static class ProjectFiles
{
    private static readonly string[] BuildFolders = ["obj", "bin", ".godot"];

    /// <summary>Reads every C# file of a project folder, and no build output.</summary>
    /// <param name="root">The root of the checkout.</param>
    /// <param name="project">The folder of the project, relative to the root.</param>
    /// <returns>One source for each file, with a path from the root, in a fixed order.</returns>
    /// <exception cref="InvalidOperationException">The project folder is absent (T-2).</exception>
    public static IReadOnlyList<ScanSource> ReadCode(string root, string project)
    {
        List<ScanSource> sources = [];
        foreach (string file in Files(root, project, "*.cs"))
        {
            sources.Add(new ScanSource(Relative(root, file), File.ReadAllText(file)));
        }

        return sources;
    }

    /// <summary>Reads every Godot scene file of a project folder.</summary>
    /// <param name="root">The root of the checkout.</param>
    /// <param name="project">The folder of the project, relative to the root.</param>
    /// <returns>The path of each scene file, from the root, in a fixed order.</returns>
    /// <exception cref="InvalidOperationException">The project folder is absent (T-2).</exception>
    public static IReadOnlyList<string> ReadScenes(string root, string project)
    {
        List<string> scenes = [];
        foreach (string file in Files(root, project, "*.tscn"))
        {
            scenes.Add(Relative(root, file));
        }

        return scenes;
    }

    /// <summary>
    /// Gives the path of each binary scene file and each binary resource file of one project:
    /// a `.scn` file and a `.res` file (D-825).
    /// </summary>
    /// <param name="root">The root of the checkout.</param>
    /// <param name="project">The folder of the project, from the root.</param>
    /// <returns>Each path, relative to the root, with a forward slash between the parts.</returns>
    public static IReadOnlyList<string> ReadBinaryScenes(string root, string project)
    {
        List<string> files = [];
        foreach (string extension in new[] { ".scn", ".res" })
        {
            foreach (string file in Files(root, project, "*" + extension))
            {
                // A pattern of three letters after the dot also matches a longer extension on
                // Windows, such as `.resx`, so the name check reads the whole extension.
                if (string.Equals(Path.GetExtension(file), extension, StringComparison.Ordinal))
                {
                    files.Add(Relative(root, file));
                }
            }
        }

        files.Sort(StringComparer.Ordinal);
        return files;
    }

    /// <summary>Tells whether a path of the checkout is inside a folder of the checkout.</summary>
    /// <param name="path">The path of a file, with a forward slash between the parts.</param>
    /// <param name="folder">The folder, with a forward slash between the parts.</param>
    /// <returns>True when the folder holds the file.</returns>
    public static bool IsInside(string path, string folder)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentException.ThrowIfNullOrEmpty(folder);
        return path.StartsWith(folder + "/", StringComparison.Ordinal);
    }

    private static List<string> Files(string root, string project, string pattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(root);
        ArgumentException.ThrowIfNullOrEmpty(project);

        string folder = Path.Combine(root, project);
        if (!Directory.Exists(folder))
        {
            throw new InvalidOperationException($"The project folder '{folder}' is absent (T-2).");
        }

        List<string> files = [];
        foreach (string file in Directory.GetFiles(folder, pattern, SearchOption.AllDirectories))
        {
            if (!IsBuildOutput(Path.GetRelativePath(folder, file)))
            {
                files.Add(file);
            }
        }

        files.Sort(StringComparer.Ordinal);
        return files;
    }

    private static bool IsBuildOutput(string relativePath)
    {
        foreach (string part in relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
        {
            if (Array.IndexOf(BuildFolders, part) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static string Relative(string root, string file) =>
        Path.GetRelativePath(root, file).Replace(Path.DirectorySeparatorChar, '/');
}
