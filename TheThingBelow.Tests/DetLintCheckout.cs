using System;
using System.IO;
using TheThingBelow.Tools.DetLint;

namespace TheThingBelow.Tests;

/// <summary>
/// A small checkout in a temporary folder, with a Core project, a Game project, the commands
/// folder of the debug assembly, and a Game build output folder that holds the Godot assembly.
/// A test writes the files that it reads, and the folder goes away at the end of the test.
/// </summary>
public sealed class DetLintCheckout : IDisposable
{
    /// <summary>The configuration name that the fixture output folder takes.</summary>
    public const string Configuration = "Debug";

    private DetLintCheckout(string root) => Root = root;

    /// <summary>The full path of the root of the fixture checkout.</summary>
    public string Root { get; }

    /// <summary>
    /// Builds a checkout with each project folder. The Game output folder holds a copy of
    /// `GodotSharp.dll`, which the text rule needs (D-614).
    /// </summary>
    /// <returns>The fixture checkout. The caller disposes it.</returns>
    public static DetLintCheckout Build()
    {
        string root = Path.Combine(Path.GetTempPath(), "det-lint-" + Guid.NewGuid().ToString("N"));
        DetLintCheckout checkout = new DetLintCheckout(root);
        Directory.CreateDirectory(Path.Combine(root, DetLintCommand.CoreProject));
        Directory.CreateDirectory(Path.Combine(root, DetLintCommand.GameProject));
        Directory.CreateDirectory(Path.Combine(root, DetLintCommand.DebugCommandsFolder));

        string output = DetLintCommand.GameOutputFolder(root, Configuration);
        Directory.CreateDirectory(output);
        string godot = Path.Combine(DetLintFixture.GameOutputFolder(), "GodotSharp.dll");
        File.Copy(godot, Path.Combine(output, "GodotSharp.dll"));
        return checkout;
    }

    /// <summary>Writes one file of the fixture checkout.</summary>
    /// <param name="relativePath">The path under the root, with forward slashes.</param>
    /// <param name="text">The text of the file.</param>
    public void Write(string relativePath, string text)
    {
        ArgumentException.ThrowIfNullOrEmpty(relativePath);
        ArgumentNullException.ThrowIfNull(text);

        string full = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        string? folder = Path.GetDirectoryName(full);
        if (folder is not null)
        {
            Directory.CreateDirectory(folder);
        }

        File.WriteAllText(full, text);
    }

    /// <summary>Removes the Game build output folder, as a checkout with no build has it.</summary>
    public void RemoveGameOutput() =>
        Directory.Delete(DetLintCommand.GameOutputFolder(Root, Configuration), recursive: true);

    /// <summary>Removes the fixture checkout from the temporary folder.</summary>
    public void Dispose()
    {
        if (Directory.Exists(Root))
        {
            Directory.Delete(Root, recursive: true);
        }
    }
}
