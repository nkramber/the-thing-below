using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Tools.Content;

/// <summary>
/// The one reader of the `content/` folder. Tools and Tests read the folder through it, and
/// Game reads the resources of its own assembly instead (D-508).
/// </summary>
public static class ContentFolder
{
    /// <summary>The name of the folder that holds the content, under the checkout root.</summary>
    public const string FolderName = "content";

    /// <summary>Reads every content file of a checkout.</summary>
    /// <param name="root">The root of the checkout, which holds the `content` folder.</param>
    /// <returns>Each file, in ordinal order of its path under `content/`.</returns>
    /// <exception cref="DirectoryNotFoundException">The root holds no `content` folder (T-2).</exception>
    public static IReadOnlyList<ContentFile> Read(string root)
    {
        ArgumentException.ThrowIfNullOrEmpty(root);

        string folder = Path.Combine(root, FolderName);
        if (!Directory.Exists(folder))
        {
            throw new DirectoryNotFoundException(
                $"The content folder '{folder}' is absent, so no content can load (T-2).");
        }

        List<ContentFile> files = [];
        foreach (string file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
        {
            string path = ContentPath(folder, file);
            files.Add(new ContentFile(path, File.ReadAllBytes(file)));
        }

        // An ordinal order, because the order of the file system follows the platform, and
        // the content hash and every match test read the same order everywhere (G-4, F-39).
        files.Sort(static (first, second) => string.CompareOrdinal(first.Path, second.Path));
        return files;
    }

    /// <summary>Gives the path of a file under `content/`, with `/` separators.</summary>
    /// <param name="folder">The `content` folder of the checkout.</param>
    /// <param name="file">The full path of the file.</param>
    /// <returns>The path that a <see cref="ContentFile"/> carries.</returns>
    /// <remarks>
    /// Windows gives `\` in a path, and the content hash and the resource names read `/` on
    /// every platform (T-7).
    /// </remarks>
    private static string ContentPath(string folder, string file)
    {
        string relative = Path.GetRelativePath(folder, file);
        return relative.Replace(Path.DirectorySeparatorChar, '/');
    }
}
