using System;
using System.Collections.Generic;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Debug.Commands;

/// <summary>
/// The console over one run: it reads a typed line, and it gives the lines that the console
/// prints (D-171, D-724). This class holds no engine type, so a test runs every command with
/// no Godot session (D-614).
/// </summary>
/// <remarks>
/// A command that changes the run sends its intent to the host, which queues it for the next
/// tick. The change lands in the rules on that tick, and the run record holds the line, so a
/// run that used the console still replays (D-171, T-7). The session never changes the state
/// itself, because a change outside a tick would leave the record behind the state (T-2).
/// <para>
/// A command that reports reads the state of the run and sends no intent (D-724).
/// </para>
/// </remarks>
public sealed class DebugSession
{
    /// <summary>The character that separates two lines of one report (D-117).</summary>
    public const string LineBreak = "\n";

    private readonly Func<RunState> state;
    private readonly Action<Intent> queue;

    /// <summary>Makes the session over one run.</summary>
    /// <param name="state">Gives the state of the run now, which a report command reads.</param>
    /// <param name="queue">Takes an intent of the console into the next tick of the run.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public DebugSession(Func<RunState> state, Action<Intent> queue)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(queue);

        this.state = state;
        this.queue = queue;
    }

    /// <summary>Runs one typed line.</summary>
    /// <param name="line">The text of the line, which can hold spaces at either end.</param>
    /// <returns>
    /// The lines that the console prints. The first line repeats the command, and an unknown
    /// name gives the fault and the list of the names (T-2).
    /// </returns>
    /// <exception cref="ArgumentNullException">The line is null (T-2).</exception>
    /// <remarks>
    /// An intent of this build carries an id and no value, so a command takes no argument. A
    /// line with more than one word is thus a fault, and the answer names the word that the
    /// session read (D-493, T-2).
    /// </remarks>
    public IReadOnlyList<string> Run(string line)
    {
        ArgumentNullException.ThrowIfNull(line);

        string name = line.Trim();
        if (name.Length == 0)
        {
            return [];
        }

        if (name.Contains(' ', StringComparison.Ordinal))
        {
            return [$"> {name}", "no command takes an argument, and this line holds more than one word (D-724)"];
        }

        if (!DebugCommands.TryFind(name, out DebugCommand? command) || command is null)
        {
            return [$"> {name}", $"no command takes the name '{name}'. {Names()}"];
        }

        List<string> answer = [$"> {name}"];
        if (command.MakesIntent)
        {
            // The command changes the run on a tick of the rules, and never here. The host
            // takes the intent into the next tick, and the record holds it (D-171, T-7).
            this.queue(Intent.OfDebugConsole(command.Action!));
            answer.Add($"the record takes the intent '{command.Action!.Value}' on the next tick");
            return answer;
        }

        answer.AddRange(command.Report(this.state()).Split(LineBreak, StringSplitOptions.None));
        return answer;
    }

    /// <summary>Gives the names of every command, for the answer to an unknown name (T-2).</summary>
    private static string Names()
    {
        List<string> names = [];
        foreach (DebugCommand command in DebugCommands.All)
        {
            names.Add(command.Name);
        }

        return $"This build holds {string.Join(", ", names)}.";
    }
}
