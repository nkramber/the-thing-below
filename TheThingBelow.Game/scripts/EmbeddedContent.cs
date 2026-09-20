using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game;

/// <summary>
/// The content that the Game assembly carries. Game reads the bytes from its own assembly in
/// the editor and in every export (D-508).
/// </summary>
/// <remarks>
/// The Godot export walks the project folder alone, and `content/` lies outside it (F-42).
/// The project file embeds each file of `content/` as a resource, so the export needs no
/// filter and no copy step.
/// <para>
/// The name of each resource is `content/` and the path of the file under it, with `/`
/// separators on every platform. The project file replaces the separator of the build
/// machine, so a Windows build and a Linux build give one set of names (T-7).
/// </para>
/// </remarks>
public static class EmbeddedContent
{
    /// <summary>The start of the name of every content resource of this assembly.</summary>
    public const string ResourcePrefix = "content/";

    /// <summary>Reads every content file from the resources of the Game assembly.</summary>
    /// <returns>Each file, in ordinal order of its path under `content/`.</returns>
    /// <exception cref="InvalidDataException">The assembly carries no content resource (T-2).</exception>
    public static IReadOnlyList<ContentFile> Read()
    {
        Assembly assembly = typeof(EmbeddedContent).Assembly;
        List<ContentFile> files = [];

        foreach (string name in assembly.GetManifestResourceNames())
        {
            if (name.StartsWith(ResourcePrefix, StringComparison.Ordinal))
            {
                string path = name[ResourcePrefix.Length..];
                files.Add(new ContentFile(path, ReadFile(path)));
            }
        }

        if (files.Count == 0)
        {
            throw new InvalidDataException(
                $"The assembly '{assembly.GetName().Name}' carries no resource that starts with " +
                $"'{ResourcePrefix}', so the build embedded no content (T-2, D-508).");
        }

        // An ordinal order, so Game and Tools give one order for one set of files (G-4, F-39).
        files.Sort(static (first, second) => string.CompareOrdinal(first.Path, second.Path));
        return files;
    }

    /// <summary>Reads the bytes of one content file from the Game assembly.</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>The bytes of the file.</returns>
    /// <exception cref="InvalidDataException">
    /// The assembly carries no such resource. The message names the resource (T-2, D-508).
    /// </exception>
    public static byte[] ReadFile(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        Assembly assembly = typeof(EmbeddedContent).Assembly;
        string name = ResourcePrefix + path;

        using Stream? stream = assembly.GetManifestResourceStream(name);
        if (stream is null)
        {
            throw new InvalidDataException(
                $"The assembly '{assembly.GetName().Name}' carries no resource '{name}' (T-2, D-508).");
        }

        using var bytes = new MemoryStream();
        stream.CopyTo(bytes);
        return bytes.ToArray();
    }
}
