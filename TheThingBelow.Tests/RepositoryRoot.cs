using System;
using System.IO;

namespace TheThingBelow.Tests;

/// <summary>
/// Finds the root of the checkout from the folder of the test assembly. A test that reads a
/// committed file takes its path from here, so the test runs from any working folder.
/// </summary>
public static class RepositoryRoot
{
    /// <summary>The file that marks the root of the checkout.</summary>
    private const string RootMarker = "TheThingBelow.slnx";

    /// <summary>Joins a path of the repository to the root of the checkout.</summary>
    /// <param name="relativePath">The path under the root, with forward slashes.</param>
    /// <returns>The full path of the file or the folder.</returns>
    public static string PathTo(string relativePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(relativePath);
        return Path.Combine(Find(), relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    /// <summary>Walks up from the test assembly until it finds the root marker.</summary>
    /// <returns>The full path of the root of the checkout.</returns>
    /// <exception cref="InvalidOperationException">No parent folder holds the marker.</exception>
    public static string Find()
    {
        string startFolder = AppContext.BaseDirectory;
        DirectoryInfo? folder = new DirectoryInfo(startFolder);
        while (folder is not null)
        {
            if (File.Exists(Path.Combine(folder.FullName, RootMarker)))
            {
                return folder.FullName;
            }

            folder = folder.Parent;
        }

        throw new InvalidOperationException(
            $"No parent folder of '{startFolder}' holds '{RootMarker}' (T-2).");
    }
}
