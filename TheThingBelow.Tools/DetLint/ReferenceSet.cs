using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;

namespace TheThingBelow.Tools.DetLint;

/// <summary>
/// The assemblies that a scan compiles against. The framework set comes from the runtime of
/// this process, which `global.json` pins to the target framework of the solution. A project
/// set adds the assemblies of the build output of that project (D-614).
/// </summary>
public static class ReferenceSet
{
    private const string TrustedAssemblies = "TRUSTED_PLATFORM_ASSEMBLIES";

    /// <summary>Gives the assemblies of the framework that runs this process.</summary>
    /// <returns>One reference for each assembly of the framework.</returns>
    /// <exception cref="InvalidOperationException">The runtime named no assembly (T-2).</exception>
    public static IReadOnlyList<MetadataReference> Framework()
    {
        if (AppContext.GetData(TrustedAssemblies) is not string list || list.Length == 0)
        {
            throw new InvalidOperationException(
                $"The runtime gave no '{TrustedAssemblies}' list, so det-lint has no framework reference.");
        }

        List<MetadataReference> references = [];
        foreach (string file in list.Split(Path.PathSeparator))
        {
            if (file.EndsWith(".dll", StringComparison.Ordinal) && File.Exists(file))
            {
                references.Add(MetadataReference.CreateFromFile(file));
            }
        }

        return references;
    }

    /// <summary>
    /// Gives the framework assemblies and each assembly of a build output folder. The Game
    /// folder holds `GodotSharp.dll` from the NuGet restore, so each CI leg reads one assembly
    /// set (D-614).
    /// </summary>
    /// <param name="outputFolder">The build output folder of the project.</param>
    /// <param name="ownAssembly">The assembly name of the project, which the scan compiles.</param>
    /// <returns>The framework references and the references of the folder.</returns>
    /// <exception cref="InvalidOperationException">The folder is absent or empty (T-2).</exception>
    public static IReadOnlyList<MetadataReference> WithOutputOf(string outputFolder, string ownAssembly)
    {
        ArgumentException.ThrowIfNullOrEmpty(outputFolder);
        ArgumentException.ThrowIfNullOrEmpty(ownAssembly);

        if (!Directory.Exists(outputFolder))
        {
            throw new InvalidOperationException(
                $"The build output folder '{outputFolder}' is absent. Run `dotnet build TheThingBelow.slnx` before det-lint.");
        }

        List<MetadataReference> references = [.. Framework()];
        HashSet<string> names = ReadFrameworkNames(references);
        int added = 0;
        foreach (string file in Directory.GetFiles(outputFolder, "*.dll", SearchOption.TopDirectoryOnly))
        {
            string name = Path.GetFileNameWithoutExtension(file);
            if (name == ownAssembly || !names.Add(name))
            {
                continue;
            }

            references.Add(MetadataReference.CreateFromFile(file));
            added++;
        }

        if (added == 0)
        {
            throw new InvalidOperationException(
                $"The build output folder '{outputFolder}' holds no assembly beside the framework. Run `dotnet build TheThingBelow.slnx` before det-lint.");
        }

        return references;
    }

    private static HashSet<string> ReadFrameworkNames(IReadOnlyList<MetadataReference> references)
    {
        HashSet<string> names = new(StringComparer.Ordinal);
        foreach (MetadataReference reference in references)
        {
            if (reference.Display is string path)
            {
                names.Add(Path.GetFileNameWithoutExtension(path));
            }
        }

        return names;
    }
}
