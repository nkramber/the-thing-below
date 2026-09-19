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
/// Storage holds the file code, and Game and Tools reference it (D-494). Thus it references
/// Core and the base libraries of .NET alone, and an engine type never reaches it.
/// </summary>
public sealed class StorageReferenceTests
{
    /// <summary>The name of the assembly that holds the file code (D-494).</summary>
    private const string StorageAssemblyName = "TheThingBelow.Storage";

    /// <summary>
    /// The project that Storage references. A second project reference would put the engine
    /// or a tool in the path of every save, and it needs a decision entry (G-13, D-494).
    /// </summary>
    private const string AllowedProjectReference = "../TheThingBelow.Core/TheThingBelow.Core.csproj";

    [Fact]
    public void StorageReferencesNoGodotAssembly()
    {
        // Tools references Storage, and Tools runs with no engine (D-101, D-494).
        IReadOnlyList<string> references = ReadReferenceNames();

        Assert.DoesNotContain(references, name => name.StartsWith("Godot", StringComparison.Ordinal));
    }

    [Fact]
    public void StorageReferencesCoreAloneOfTheProjectsOfTheSolution()
    {
        IReadOnlyList<string> references = ReadReferenceNames();

        string[] projects = references
            .Where(name => name.StartsWith("TheThingBelow.", StringComparison.Ordinal))
            .ToArray();

        Assert.Equal([CoreAssembly.Name], projects);
    }

    [Fact]
    public void TheStorageProjectFileDeclaresCoreAlone()
    {
        // The compiler writes no metadata entry for a reference that no code uses, so the
        // two tests above cannot see an unused reference (F-61).
        XDocument project = XDocument.Load(
            RepositoryRoot.PathTo("TheThingBelow.Storage/TheThingBelow.Storage.csproj"));

        string[] declared = project.Descendants()
            .Where(element => element.Name.LocalName is "ProjectReference" or "PackageReference" or "Reference")
            .Select(element => $"{element.Name.LocalName} {element.Attribute("Include")?.Value ?? "(no Include)"}")
            .ToArray();

        Assert.Equal([$"ProjectReference {AllowedProjectReference}"], declared);
    }

    private static IReadOnlyList<string> ReadReferenceNames()
    {
        using FileStream file = File.OpenRead(ReadStorageAssemblyPath());
        using PEReader peReader = new PEReader(file);
        MetadataReader metadata = peReader.GetMetadataReader();

        return metadata.AssemblyReferences
            .Select(handle => metadata.GetString(metadata.GetAssemblyReference(handle).Name))
            .ToArray();
    }

    private static string ReadStorageAssemblyPath()
    {
        AssemblyMetadataAttribute? entry = typeof(StorageReferenceTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => attribute.Key == "StorageAssemblyPath");

        if (entry?.Value is null)
        {
            throw new InvalidOperationException(
                "The build wrote no 'StorageAssemblyPath' into the test assembly (T-2).");
        }

        string path = Path.GetFullPath(entry.Value);
        if (Path.GetFileNameWithoutExtension(path) != StorageAssemblyName)
        {
            throw new InvalidOperationException(
                $"The path '{path}' does not name the assembly '{StorageAssemblyName}' (T-2).");
        }

        if (!File.Exists(path))
        {
            throw new InvalidOperationException(
                $"The Storage assembly '{path}' does not exist. Build the solution first (T-2).");
        }

        return path;
    }
}
