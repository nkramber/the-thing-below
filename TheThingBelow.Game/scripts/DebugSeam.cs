using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Godot;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Game;

/// <summary>
/// The one place where Game reaches the debug assembly of D-260. A development build loads
/// that assembly by name and reads its entry, and a release build loads nothing (D-171,
/// D-492, D-723).
/// </summary>
/// <remarks>
/// Game names no type of the debug assembly, and it holds the names of the entry as text
/// below. The Game project references the assembly in every configuration except
/// `ExportRelease`, so Game compiles for a release export with no reference and with no
/// conditional compilation (D-723, F-27).
/// <para>
/// A development build that cannot load the assembly, or that finds no member of the entry,
/// fails with the name of the assembly, the type, and the member. A silent fall back to a run
/// with no console would hide a broken build (T-2).
/// </para>
/// <para>
/// A test of Tests reads these names from the built Game assembly and finds each member in the
/// built debug assembly, so a rename of one side alone fails a test and never a play session
/// (T-3, D-614).
/// </para>
/// </remarks>
public static class DebugSeam
{
    /// <summary>The name of the debug assembly (D-217, D-260).</summary>
    public const string AssemblyName = "TheThingBelow.Debug";

    /// <summary>The type that holds every entry of that assembly.</summary>
    public const string EntryTypeName = "TheThingBelow.Debug.DebugAssembly";

    /// <summary>The member that gives the handlers of every debug intent (D-260).</summary>
    public const string HandlersMember = "Handlers";

    /// <summary>The member that builds the console and gives its node (D-171).</summary>
    public const string ConsoleMember = "Create";

    /// <summary>The member that gives the lines that a console shows now (D-117, D-725).</summary>
    public const string ShownLinesMember = "ShownLines";

    /// <summary>The member that runs one command with no console on the screen (D-117).</summary>
    public const string RunMember = "Run";

    /// <summary>The member that names every command of the build (D-117).</summary>
    public const string CommandNamesMember = "CommandNames";

    /// <summary>
    /// The feature of a development build. Godot gives `debug` to an editor session and to a
    /// build of the debug export, and `release` to a release export (the Godot docs, "Feature
    /// tags").
    /// </summary>
    public const string DevelopmentFeature = "debug";

    /// <summary>True in a development build, which carries the debug assembly (D-260).</summary>
    public static bool IsDevelopmentBuild => OS.HasFeature(DevelopmentFeature);

    /// <summary>
    /// The handlers of every debug intent, which the host passes to the start of a run
    /// (D-260, D-492).
    /// </summary>
    /// <returns>
    /// The handlers of the console in a development build, and
    /// <see cref="DebugIntentHandlers.None"/> in a release build.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// A development build holds no debug assembly, or its entry lost a member (T-2).
    /// </exception>
    public static DebugIntentHandlers Handlers() =>
        IsDevelopmentBuild
            ? Entry<Func<DebugIntentHandlers>>(HandlersMember)()
            : DebugIntentHandlers.None;

    /// <summary>Builds the console of a development build (D-171).</summary>
    /// <param name="state">Gives the state of the run now, which a report command reads.</param>
    /// <param name="queue">Takes an intent of the console into the next tick of the run.</param>
    /// <param name="console">The node of the console, or null in a release build.</param>
    /// <returns>True in a development build, which built the console.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">
    /// A development build holds no debug assembly, or its entry lost a member (T-2).
    /// </exception>
    public static bool TryBuildConsole(
        Func<RunState> state,
        Action<Intent> queue,
        out Control? console)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(queue);

        if (!IsDevelopmentBuild)
        {
            console = null;
            return false;
        }

        console = Entry<Func<Func<RunState>, Action<Intent>, Control>>(ConsoleMember)(state, queue);
        return true;
    }

    /// <summary>
    /// Gives the lines that a console shows now. The smoke session reads them after it types a
    /// line through key events (D-117, D-725).
    /// </summary>
    /// <param name="console">The node of <see cref="TryBuildConsole"/>.</param>
    /// <returns>The lines of the console, from the oldest to the newest.</returns>
    /// <exception cref="ArgumentNullException">The node is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">
    /// The build is a release build, the entry lost a member, or the node is no console (T-2).
    /// </exception>
    public static IReadOnlyList<string> ShownLines(Control console)
    {
        ArgumentNullException.ThrowIfNull(console);
        RefuseReleaseBuild(nameof(ShownLines));

        return Entry<Func<Control, IReadOnlyList<string>>>(ShownLinesMember)(console);
    }

    /// <summary>Runs one command of the console with no console on the screen (D-117).</summary>
    /// <param name="line">The text of the line, such as `reveal`.</param>
    /// <param name="state">Gives the state of the run now.</param>
    /// <param name="queue">Takes an intent of the console into the next tick of the run.</param>
    /// <returns>The lines that a console would print.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">
    /// The build is a release build, it holds no debug assembly, or the entry lost a member (T-2).
    /// </exception>
    public static IReadOnlyList<string> Run(string line, Func<RunState> state, Action<Intent> queue)
    {
        ArgumentNullException.ThrowIfNull(line);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(queue);
        RefuseReleaseBuild(nameof(Run));

        return Entry<Func<string, Func<RunState>, Action<Intent>, IReadOnlyList<string>>>(RunMember)(
            line,
            state,
            queue);
    }

    /// <summary>Names every command of the console (D-117).</summary>
    /// <returns>One name for each command, in the order of `help`.</returns>
    /// <exception cref="InvalidOperationException">
    /// The build is a release build, it holds no debug assembly, or the entry lost a member (T-2).
    /// </exception>
    public static IReadOnlyList<string> CommandNames()
    {
        RefuseReleaseBuild(nameof(CommandNames));

        return Entry<Func<IReadOnlyList<string>>>(CommandNamesMember)();
    }

    /// <summary>
    /// Gives one member of the entry as a delegate. The call then runs with no wrapper around
    /// its errors, so a fault of a command reaches the crash file with its own type (D-170, T-2).
    /// </summary>
    /// <typeparam name="TEntry">The delegate type of the member.</typeparam>
    /// <param name="member">The name of the member.</param>
    /// <returns>The delegate of that member.</returns>
    /// <exception cref="InvalidOperationException">
    /// The assembly is absent, the type is absent, the member is absent, or the member takes
    /// another signature (T-2).
    /// </exception>
    private static TEntry Entry<TEntry>(string member)
        where TEntry : Delegate
    {
        Type entry = EntryType();
        MethodInfo found = entry.GetMethod(member, BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException(
                $"The type '{EntryTypeName}' of the assembly '{AssemblyName}' holds no public "
                + $"static member '{member}'. `{nameof(DebugSeam)}` and that type are one "
                + $"contract (D-723, T-2).");

        try
        {
            return found.CreateDelegate<TEntry>();
        }
        catch (ArgumentException fault)
        {
            throw new InvalidOperationException(
                $"The member '{EntryTypeName}.{member}' does not take the form of "
                + $"'{typeof(TEntry)}'. `{nameof(DebugSeam)}` and that type are one contract "
                + $"(D-723, T-2). {fault.Message}",
                fault);
        }
    }

    /// <summary>Loads the debug assembly and gives its entry type (D-260, D-723).</summary>
    /// <exception cref="InvalidOperationException">The assembly or the type is absent (T-2).</exception>
    private static Type EntryType()
    {
        Assembly loaded;
        try
        {
            loaded = Assembly.Load(AssemblyName);
        }
        catch (Exception fault) when (fault is FileNotFoundException or FileLoadException or BadImageFormatException)
        {
            throw new InvalidOperationException(
                $"This build has the feature '{DevelopmentFeature}', and it could not load the "
                + $"assembly '{AssemblyName}'. A development build references it, and the "
                + $"`ExportRelease` configuration does not (D-260, D-723, T-2). {fault.Message}",
                fault);
        }

        return loaded.GetType(EntryTypeName)
            ?? throw new InvalidOperationException(
                $"The assembly '{AssemblyName}' holds no type '{EntryTypeName}' (D-723, T-2).");
    }

    /// <summary>Refuses a call that a release build cannot answer (T-2).</summary>
    /// <param name="member">The member of this class that the host called.</param>
    /// <exception cref="InvalidOperationException">The build is a release build (T-2).</exception>
    private static void RefuseReleaseBuild(string member)
    {
        if (!IsDevelopmentBuild)
        {
            throw new InvalidOperationException(
                $"`{nameof(DebugSeam)}.{member}` needs the debug assembly, and this build has "
                + $"no feature '{DevelopmentFeature}'. A release build holds no debug code "
                + $"(D-171, D-260, T-2).");
        }
    }
}
