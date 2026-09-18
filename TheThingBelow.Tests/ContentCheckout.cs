using System;
using System.IO;
using TheThingBelow.Tools.Content;

namespace TheThingBelow.Tests;

/// <summary>
/// A copy of the `content/` folder and the committed hash file in a temporary folder. A test
/// changes one content file there, and the folder goes away at the end of the test.
/// </summary>
public sealed class ContentCheckout : IDisposable
{
    private ContentCheckout(string root) => this.Root = root;

    /// <summary>The full path of the root of the copy.</summary>
    public string Root { get; }

    /// <summary>Copies the content folder and the hash file of this checkout.</summary>
    /// <returns>The copy. The caller disposes it.</returns>
    public static ContentCheckout Copy()
    {
        string root = Path.Combine(Path.GetTempPath(), "content-" + Guid.NewGuid().ToString("N"));
        ContentCheckout checkout = new(root);

        string source = Path.Combine(RepositoryRoot.Find(), ContentFolder.FolderName);
        foreach (string file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(source, file);
            checkout.CopyInto(Path.Combine(root, ContentFolder.FolderName, relative), file);
        }

        string hashFile = ContentHashCommand.HashFilePath.Replace('/', Path.DirectorySeparatorChar);
        checkout.CopyInto(Path.Combine(root, hashFile), RepositoryRoot.PathTo(ContentHashCommand.HashFilePath));
        return checkout;
    }

    /// <summary>Writes one content file of the copy.</summary>
    /// <param name="relativePath">The path under `content/`, with forward slashes.</param>
    /// <param name="text">The text of the file, with `\n` line ends.</param>
    public void WriteRuleFile(string relativePath, string text)
    {
        ArgumentException.ThrowIfNullOrEmpty(relativePath);
        ArgumentNullException.ThrowIfNull(text);

        string full = Path.Combine(
            this.Root,
            ContentFolder.FolderName,
            relativePath.Replace('/', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, text.ReplaceLineEndings("\n") + "\n");
    }

    /// <summary>Removes the temporary folder of this copy.</summary>
    public void Dispose()
    {
        if (Directory.Exists(this.Root))
        {
            Directory.Delete(this.Root, recursive: true);
        }
    }

    private void CopyInto(string target, string source)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.Copy(source, target);
    }
}
