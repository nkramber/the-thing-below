using System;
using System.Collections.Generic;
using System.IO;

namespace TheThingBelow.Tools.SteCheck;

/// <summary>
/// The files of the checkout that the command reads. It holds every document, the live
/// documents, and the paths that the reference check resolves.
/// </summary>
public sealed class DocumentSet
{
    /// <summary>The four dated records. D-10 keeps each one out of the writing rules.</summary>
    public static readonly IReadOnlyList<string> DatedRecordPaths =
    [
        "docs/reviews/",
        "docs/session-handoff.md",
        "docs/session-handoff-archive.md",
        "docs/archive/",
    ];

    /// <summary>
    /// The folders that hold build output, editor state, or a local run. No document of the
    /// repository lives under one of them.
    /// </summary>
    private static readonly HashSet<string> SkippedFolders = new HashSet<string>(StringComparer.Ordinal)
    {
        ".git", ".godot", ".idea", ".vscode", "bin", "obj", "artifacts", "logs",
        "node_modules", "TestResults",
    };

    private readonly List<string> allPaths = [];
    private readonly List<string> documents = [];
    private readonly List<string> liveDocuments = [];
    private readonly HashSet<string> pathSet = new HashSet<string>(StringComparer.Ordinal);

    private DocumentSet(string root) => Root = root;

    /// <summary>The full path of the root of the checkout.</summary>
    public string Root { get; }

    /// <summary>Every file and folder of the checkout, as a path from the root with forward slashes.</summary>
    public IReadOnlyList<string> AllPaths => allPaths;

    /// <summary>Every `.md` file of the checkout, in a fixed order.</summary>
    public IReadOnlyList<string> Documents => documents;

    /// <summary>Every `.md` file that is not a dated record, in a fixed order.</summary>
    public IReadOnlyList<string> LiveDocuments => liveDocuments;

    /// <summary>Reads the tree of the checkout.</summary>
    /// <param name="root">The path of the root of the checkout.</param>
    /// <returns>The file set of that checkout.</returns>
    /// <exception cref="DirectoryNotFoundException">The root does not exist.</exception>
    public static DocumentSet Read(string root)
    {
        ArgumentException.ThrowIfNullOrEmpty(root);
        string fullRoot = Path.GetFullPath(root);
        if (!Directory.Exists(fullRoot))
        {
            throw new DirectoryNotFoundException($"The root '{fullRoot}' does not exist (T-2).");
        }

        DocumentSet set = new DocumentSet(fullRoot);
        set.ReadFolder(fullRoot, string.Empty);
        set.allPaths.Sort(StringComparer.Ordinal);
        set.documents.Sort(StringComparer.Ordinal);
        set.liveDocuments.Sort(StringComparer.Ordinal);
        return set;
    }

    /// <summary>Reads a path of the repository as a dated record (D-10).</summary>
    /// <param name="path">A path from the root, with forward slashes.</param>
    /// <returns>True when the writing rules skip the path.</returns>
    public static bool IsDatedRecord(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        foreach (string record in DatedRecordPaths)
        {
            bool match = record.EndsWith('/')
                ? path.StartsWith(record, StringComparison.Ordinal)
                : string.Equals(path, record, StringComparison.Ordinal);
            if (match)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Reads whether the checkout holds a file or a folder at this path.</summary>
    /// <param name="path">A path from the root, with forward slashes and no trailing slash.</param>
    /// <returns>True when the path exists in the checkout.</returns>
    public bool Holds(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        return pathSet.Contains(path);
    }

    /// <summary>Counts the files and folders whose path ends with this name.</summary>
    /// <param name="path">A path from the root, with forward slashes and no trailing slash.</param>
    /// <returns>The number of entries of the checkout that end with the path.</returns>
    public int CountEndingWith(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        string suffix = "/" + path;
        int count = 0;
        foreach (string entry in allPaths)
        {
            if (entry.EndsWith(suffix, StringComparison.Ordinal))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>Reads each line of a document of the checkout.</summary>
    /// <param name="path">A path from the root, with forward slashes.</param>
    /// <returns>Each line of the file, in order.</returns>
    public IReadOnlyList<string> ReadLines(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        return File.ReadAllLines(Path.Combine(Root, path.Replace('/', Path.DirectorySeparatorChar)));
    }

    private void ReadFolder(string fullPath, string relativePath)
    {
        foreach (string fullFile in Directory.EnumerateFiles(fullPath))
        {
            string file = Join(relativePath, Path.GetFileName(fullFile));
            Add(file);
            if (file.EndsWith(".md", StringComparison.Ordinal))
            {
                documents.Add(file);
                if (!IsDatedRecord(file))
                {
                    liveDocuments.Add(file);
                }
            }
        }

        foreach (string fullFolder in Directory.EnumerateDirectories(fullPath))
        {
            string name = Path.GetFileName(fullFolder);
            if (SkippedFolders.Contains(name))
            {
                continue;
            }

            string folder = Join(relativePath, name);
            Add(folder);
            ReadFolder(fullFolder, folder);
        }
    }

    private void Add(string path)
    {
        allPaths.Add(path);
        pathSet.Add(path);
    }

    private static string Join(string folder, string name) =>
        folder.Length == 0 ? name : folder + "/" + name;
}
