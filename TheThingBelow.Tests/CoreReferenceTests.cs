using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
using TheThingBelow.Core;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// Core has no engine dependency and no file, network, clock, or OS dependency (G-1, D-100).
/// This test reads the reference list of the Core assembly and fails on any other reference.
/// </summary>
public sealed class CoreReferenceTests
{
    /// <summary>
    /// The assemblies that Core can reference. Each one is a base library of .NET with no
    /// engine, file, network, clock, or OS code. A new name needs a decision entry (G-13).
    /// </summary>
    private static readonly IReadOnlySet<string> AllowedReferences =
        new SortedSet<string>(StringComparer.Ordinal)
        {
            "System.Collections",
            "System.Memory",
            "System.Runtime",
        };

    [Fact]
    public void CoreReferencesTheAllowedAssembliesAlone()
    {
        IReadOnlyList<string> references = ReadReferenceNames();

        string[] notAllowed = references.Where(name => !AllowedReferences.Contains(name)).ToArray();

        Assert.True(
            notAllowed.Length == 0,
            $"Core references {string.Join(", ", notAllowed)}. G-1 allows " +
            $"{string.Join(", ", AllowedReferences)} alone.");
    }

    [Fact]
    public void CoreReferencesNoGodotAssembly()
    {
        IReadOnlyList<string> references = ReadReferenceNames();

        Assert.DoesNotContain(
            references,
            name => name.StartsWith("Godot", StringComparison.Ordinal));
    }

    [Fact]
    public void TheCoreProjectFileDeclaresNoReference()
    {
        // The compiler writes no metadata entry for a reference that no code uses, so the
        // two tests above cannot see an unused reference. This test reads the project file.
        XDocument project = XDocument.Load(
            RepositoryRoot.PathTo("TheThingBelow.Core/TheThingBelow.Core.csproj"));

        string[] declared = project.Descendants()
            .Where(element =>
                element.Name.LocalName is "ProjectReference" or "PackageReference"
                or "Reference")
            .Select(element =>
                $"{element.Name.LocalName} {element.Attribute("Include")?.Value ?? "(no Include)"}")
            .ToArray();

        Assert.True(
            declared.Length == 0,
            $"The Core project declares {string.Join(", ", declared)}. G-1 allows no " +
            "reference, and a new one needs a decision entry (G-13).");
    }

    /// <summary>
    /// Reads the reference list from the file that the Core project built. It never reads
    /// the loaded assembly, because a coverage run instruments the copy in this output
    /// folder and adds references to it (F-61).
    /// </summary>
    /// <returns>The name of each assembly that the Core assembly references.</returns>
    private static IReadOnlyList<string> ReadReferenceNames()
    {
        string corePath = ReadCoreAssemblyPath();
        using FileStream file = File.OpenRead(corePath);
        using PEReader peReader = new PEReader(file);
        MetadataReader metadata = peReader.GetMetadataReader();

        return metadata.AssemblyReferences
            .Select(handle => metadata.GetString(metadata.GetAssemblyReference(handle).Name))
            .ToArray();
    }

    /// <summary>Reads the path that the build wrote into this test assembly.</summary>
    /// <returns>The full path of the Core assembly that the Core project built.</returns>
    /// <exception cref="InvalidOperationException">The path is absent or names no file.</exception>
    private static string ReadCoreAssemblyPath()
    {
        AssemblyMetadataAttribute? entry = typeof(CoreReferenceTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => attribute.Key == "CoreAssemblyPath");

        if (entry?.Value is null)
        {
            throw new InvalidOperationException(
                "The build wrote no 'CoreAssemblyPath' into the test assembly (T-2).");
        }

        string path = Path.GetFullPath(entry.Value);
        if (Path.GetFileNameWithoutExtension(path) != CoreAssembly.Name)
        {
            throw new InvalidOperationException(
                $"The path '{path}' does not name the assembly '{CoreAssembly.Name}' (T-2).");
        }

        if (!File.Exists(path))
        {
            throw new InvalidOperationException(
                $"The Core assembly '{path}' does not exist. Build the solution first (T-2).");
        }

        return path;
    }
}
