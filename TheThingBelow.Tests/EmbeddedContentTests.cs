using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The Game assembly carries every file of `content/`, because the Godot export walks the
/// project folder alone and `content/` lies outside it (D-508, F-42).
/// </summary>
/// <remarks>
/// This project takes no reference to Game. Such a reference puts the Game assemblies in the
/// assembly list of the test host, and the det-lint fixture reads that list as the framework
/// set, so each Game fixture then compiles against no project assembly (D-614). The tests
/// below load the file that the Game project built, as `CoreReferenceTests` does (F-61).
/// </remarks>
public sealed class EmbeddedContentTests
{
    /// <summary>The name of the type of Game that reads the embedded content.</summary>
    private const string ReaderTypeName = "TheThingBelow.Game.EmbeddedContent";

    /// <summary>The start of the name of every content resource of the Game assembly.</summary>
    private const string ResourcePrefix = "content/";

    [Fact]
    public void TheEmbeddedSetMatchesTheFolderByName()
    {
        IReadOnlyList<ContentFile> embedded = ReadEmbedded();
        IReadOnlyList<ContentFile> folder = ContentFolder.Read(RepositoryRoot.Find());

        Assert.Equal(folder.Select(file => file.Path), embedded.Select(file => file.Path));
    }

    [Fact]
    public void TheEmbeddedSetMatchesTheFolderByBytes()
    {
        IReadOnlyList<ContentFile> embedded = ReadEmbedded();
        IReadOnlyList<ContentFile> folder = ContentFolder.Read(RepositoryRoot.Find());

        Assert.Equal(folder.Count, embedded.Count);
        for (int index = 0; index < folder.Count; index++)
        {
            Assert.Equal(folder[index].Path, embedded[index].Path);
            Assert.True(
                folder[index].Bytes.AsSpan().SequenceEqual(embedded[index].Bytes),
                $"The resource '{embedded[index].Path}' differs from the file of the folder (D-508).");
        }
    }

    [Fact]
    public void EachResourceNameHoldsNoBackslash()
    {
        // `%(RecursiveDir)` holds the separator of the build machine, and Windows gives `\`.
        // The project file replaces it, so the three CI legs give one set of names (T-7).
        foreach (ContentFile file in ReadEmbedded())
        {
            Assert.DoesNotContain('\\', file.Path);
        }
    }

    [Fact]
    public void TheEmbeddedContentLoadsAsAContentSet()
    {
        ContentSet set = ContentSet.Load(ReadEmbedded());

        Assert.NotEmpty(set.RuleEntries);
    }

    [Fact]
    public void TheEmbeddedHashMatchesTheFolderHash()
    {
        string fromAssembly = ContentHash.Compute(ReadEmbedded());
        string fromFolder = ContentHash.Compute(ContentFolder.Read(RepositoryRoot.Find()));

        Assert.Equal(fromFolder, fromAssembly);
    }

    [Fact]
    public void TheReaderOfGameGivesTheSameSetAsTheResources()
    {
        // The test reads the resources itself above. This test runs the reader of Game over
        // the same assembly, so a fault of that reader fails here and not in the game.
        IReadOnlyList<ContentFile> fromReader = (IReadOnlyList<ContentFile>)Invoke("Read");

        Assert.Equal(ReadEmbedded().Select(file => file.Path), fromReader.Select(file => file.Path));
    }

    [Fact]
    public void AResourceThatTheAssemblyLacksFailsWithItsName()
    {
        TargetInvocationException thrown = Assert.Throws<TargetInvocationException>(
            () => Invoke("ReadFile", "rules/fixtures/absent.json"));

        InvalidDataException error = Assert.IsType<InvalidDataException>(thrown.InnerException);
        Assert.Contains("content/rules/fixtures/absent.json", error.Message);
        Assert.Contains("D-508", error.Message);
    }

    [Fact]
    public void AResourceOfTheAssemblyReadsByItsPath()
    {
        byte[] bytes = (byte[])Invoke("ReadFile", Palette.Path);

        Palette palette = Palette.Read(bytes, Palette.Path);

        Assert.NotEmpty(palette.Colors);
    }

    /// <summary>Reads each content resource of the Game assembly, with no code of Game.</summary>
    /// <returns>Each file, in ordinal order of its path under `content/`.</returns>
    private static IReadOnlyList<ContentFile> ReadEmbedded()
    {
        Assembly game = GameAssembly();
        List<ContentFile> files = [];

        foreach (string name in game.GetManifestResourceNames())
        {
            if (!name.StartsWith(ResourcePrefix, StringComparison.Ordinal))
            {
                continue;
            }

            using Stream stream = game.GetManifestResourceStream(name)
                ?? throw new InvalidDataException($"The assembly carries no resource '{name}' (T-2).");
            using MemoryStream bytes = new();
            stream.CopyTo(bytes);
            files.Add(new ContentFile(name[ResourcePrefix.Length..], bytes.ToArray()));
        }

        files.Sort(static (first, second) => string.CompareOrdinal(first.Path, second.Path));
        return files;
    }

    /// <summary>Calls one method of the content reader of Game on the built assembly.</summary>
    /// <param name="method">The name of the method, such as `ReadFile`.</param>
    /// <param name="arguments">The arguments of the call.</param>
    /// <returns>The value that the method gave.</returns>
    private static object Invoke(string method, params object[] arguments)
    {
        Type reader = GameAssembly().GetType(ReaderTypeName)
            ?? throw new InvalidOperationException($"The Game assembly holds no type '{ReaderTypeName}' (T-2).");

        MethodInfo call = reader.GetMethod(method, arguments.Select(value => value.GetType()).ToArray())
            ?? throw new InvalidOperationException($"The type '{ReaderTypeName}' holds no method '{method}' (T-2).");

        return call.Invoke(null, arguments)
            ?? throw new InvalidOperationException($"The method '{method}' gave nothing (T-2).");
    }

    /// <summary>Loads the Game assembly that the build wrote.</summary>
    /// <returns>The assembly, with its content resources.</returns>
    private static Assembly GameAssembly()
    {
        AssemblyMetadataAttribute? entry = typeof(EmbeddedContentTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => attribute.Key == "GameAssemblyPath");

        if (entry?.Value is null)
        {
            throw new InvalidOperationException(
                "The build wrote no 'GameAssemblyPath' into the test assembly (T-2).");
        }

        string path = Path.GetFullPath(entry.Value);
        if (!File.Exists(path))
        {
            throw new InvalidOperationException(
                $"The Game assembly '{path}' does not exist. Build the solution first (T-2).");
        }

        return Assembly.LoadFrom(path);
    }
}
