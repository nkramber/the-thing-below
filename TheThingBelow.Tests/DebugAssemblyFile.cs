using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Tests;

/// <summary>
/// Loads the debug assembly that the build wrote, and gives each member of its entry (D-260,
/// D-723). Tests takes no reference to that project, because it references GodotSharp and
/// those assemblies would join the assembly list of the test host, which the det-lint fixture
/// reads as the framework set (D-614).
/// </summary>
/// <remarks>
/// The members below take the same delegate types as <c>DebugSeam</c> of Game, so a test runs
/// each command through the contract that the game uses and never through a second path (T-3).
/// </remarks>
public static class DebugAssemblyFile
{
    /// <summary>The metadata key that the test project writes with the path of the assembly.</summary>
    private const string PathKey = "DebugAssemblyPath";

    /// <summary>The full name of the type that holds every entry of the assembly.</summary>
    public const string EntryTypeName = "TheThingBelow.Debug.DebugAssembly";

    /// <summary>Loads the debug assembly that the build wrote.</summary>
    /// <returns>The assembly.</returns>
    /// <exception cref="InvalidOperationException">The build wrote no such file (T-2).</exception>
    public static Assembly Load()
    {
        AssemblyMetadataAttribute? entry = typeof(DebugAssemblyFile).Assembly
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
                $"The debug assembly '{path}' does not exist. Build the solution first (T-2).");
        }

        return Assembly.LoadFrom(path);
    }

    /// <summary>Gives the type that holds every entry of the assembly.</summary>
    /// <returns>The entry type.</returns>
    /// <exception cref="InvalidOperationException">The assembly holds no such type (T-2).</exception>
    public static Type EntryType() =>
        Load().GetType(EntryTypeName)
            ?? throw new InvalidOperationException(
                $"The debug assembly holds no type '{EntryTypeName}' (T-2).");

    /// <summary>Gives one public static member of the entry type.</summary>
    /// <param name="member">The name of the member, such as `Run`.</param>
    /// <returns>The member.</returns>
    /// <exception cref="InvalidOperationException">The type holds no such member (T-2).</exception>
    public static MethodInfo Member(string member)
    {
        ArgumentException.ThrowIfNullOrEmpty(member);

        return EntryType().GetMethod(member, BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException(
                $"The type '{EntryTypeName}' holds no public static member '{member}' (T-2).");
    }

    /// <summary>Gives the handlers of every debug intent of the build (D-260).</summary>
    /// <returns>The handler set that a development build passes to the start of a run.</returns>
    public static DebugIntentHandlers Handlers() =>
        Member("Handlers").CreateDelegate<Func<DebugIntentHandlers>>()();

    /// <summary>Runs one line of the console with no console on the screen (D-724).</summary>
    /// <param name="line">The text of the line, such as `reveal`.</param>
    /// <param name="state">Gives the state of the run now.</param>
    /// <param name="queue">Takes an intent of the console into the next tick of the run.</param>
    /// <returns>The lines that a console would print.</returns>
    public static IReadOnlyList<string> Run(string line, Func<RunState> state, Action<Intent> queue) =>
        Member("Run")
            .CreateDelegate<Func<string, Func<RunState>, Action<Intent>, IReadOnlyList<string>>>()(
                line,
                state,
                queue);

    /// <summary>Names every command of the build, in the order of `help` (D-724).</summary>
    /// <returns>One name for each command.</returns>
    public static IReadOnlyList<string> CommandNames() =>
        Member("CommandNames").CreateDelegate<Func<IReadOnlyList<string>>>()();
}
