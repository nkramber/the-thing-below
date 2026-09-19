using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TheThingBelow.Tests;

/// <summary>
/// Loads the Game assembly that the build wrote. Tests takes no reference to Game, because
/// the Game assemblies would then join the assembly list of the test host, and the det-lint
/// fixture reads that list as the framework set (D-614).
/// </summary>
public static class GameAssemblyFile
{
    /// <summary>The metadata key that the test project writes with the path of the assembly.</summary>
    private const string PathKey = "GameAssemblyPath";

    /// <summary>Loads the Game assembly that the build wrote.</summary>
    /// <returns>The assembly, with its content resources and its types.</returns>
    /// <exception cref="InvalidOperationException">The build wrote no such file (T-2).</exception>
    public static Assembly Load()
    {
        AssemblyMetadataAttribute? entry = typeof(GameAssemblyFile).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => attribute.Key == PathKey);

        if (entry?.Value is null)
        {
            throw new InvalidOperationException(
                $"The build wrote no '{PathKey}' into the test assembly (T-2).");
        }

        string path = Path.GetFullPath(entry.Value);
        if (!File.Exists(path))
        {
            throw new InvalidOperationException(
                $"The Game assembly '{path}' does not exist. Build the solution first (T-2).");
        }

        return Assembly.LoadFrom(path);
    }

    /// <summary>Finds one type of the Game assembly by its full name.</summary>
    /// <param name="fullName">The full name, such as `TheThingBelow.Game.FixedStepLoop`.</param>
    /// <returns>The type.</returns>
    /// <exception cref="InvalidOperationException">The assembly holds no such type (T-2).</exception>
    public static Type Type(string fullName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fullName);

        return Load().GetType(fullName)
            ?? throw new InvalidOperationException($"The Game assembly holds no type '{fullName}' (T-2).");
    }
}
