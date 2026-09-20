using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Core.Runs;
using TheThingBelow.Debug.Commands;
using TheThingBelow.Debug.Console;

namespace TheThingBelow.Debug;

/// <summary>
/// The one entry of this assembly. Game loads this assembly by name and calls these members
/// by reflection, so Game names no type of this assembly and it compiles with no reference to
/// it (D-260, D-492, D-723).
/// </summary>
/// <remarks>
/// A member of this class is a contract with `DebugSeam` of Game. A test of Tests reads the
/// names that `DebugSeam` holds and finds each member here, so a rename of one side alone
/// fails the build of the other side at the test and not in a play session (T-2, T-3).
/// <para>
/// The capture of PR-74 and the sound room of PR-71 add their own members here, behind the
/// same seam (D-439, D-546, D-551).
/// </para>
/// </remarks>
public static class DebugAssembly
{
    /// <summary>The name of this assembly, which the host passes to `Assembly.Load`.</summary>
    public const string Name = "TheThingBelow.Debug";

    /// <summary>
    /// The handlers of every debug intent of this build, which the host passes to
    /// <see cref="Simulation.Start"/> (D-260, D-492).
    /// </summary>
    /// <returns>One handler for each command that sends an intent.</returns>
    public static DebugIntentHandlers Handlers() => DebugCommands.Handlers();

    /// <summary>Builds the console over one run, and gives its node (D-171).</summary>
    /// <param name="state">Gives the state of the run now, which a report command reads.</param>
    /// <param name="queue">Takes an intent of the console into the next tick of the run.</param>
    /// <returns>The node of the console, which the host adds to the frame layer (D-568).</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static Control Create(Func<RunState> state, Action<Intent> queue) =>
        new DebugConsole(new DebugSession(state, queue)).Root;

    /// <summary>
    /// Runs one command with no console on the screen. The smoke session of CI runs each
    /// command this way, and the headless runner of PR-15 can drive a bot with it (D-64, D-117).
    /// </summary>
    /// <param name="line">The text of the line, such as `reveal`.</param>
    /// <param name="state">Gives the state of the run now.</param>
    /// <param name="queue">Takes an intent of the console into the next tick of the run.</param>
    /// <returns>The lines that a console would print.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static IReadOnlyList<string> Run(string line, Func<RunState> state, Action<Intent> queue) =>
        new DebugSession(state, queue).Run(line);

    /// <summary>The name of each command of this build, which the smoke session runs (D-117).</summary>
    /// <returns>One name for each command, in the order of `help`.</returns>
    public static IReadOnlyList<string> CommandNames()
    {
        List<string> names = [];
        foreach (DebugCommand command in DebugCommands.All)
        {
            names.Add(command.Name);
        }

        return names;
    }
}
