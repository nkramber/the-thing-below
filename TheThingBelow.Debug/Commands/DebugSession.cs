using System;
using System.Collections.Generic;
using System.Globalization;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
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
    /// A battle command that aims takes one argument, the slot of its target, the go-to-map
    /// command takes the id of its map, and every other command takes none. A line of the wrong
    /// shape is a fault, and the answer names it (D-767, D-1133, T-2). The item command uses the
    /// first item that the pack holds (D-780).
    /// </remarks>
    public IReadOnlyList<string> Run(string line)
    {
        ArgumentNullException.ThrowIfNull(line);

        string[] words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return [];
        }

        string name = words[0];
        string shown = string.Join(' ', words);
        if (words.Length > 2)
        {
            return [$"> {shown}", "a command takes one argument at most, and this line holds more (D-767)"];
        }

        if (!DebugCommands.TryFind(name, out DebugCommand? command) || command is null)
        {
            return [$"> {shown}", $"no command takes the name '{name}'. {Names()}"];
        }

        if (command.TakesMap)
        {
            if (!TryMap(command, words, out ContentId? map, out string mapFault))
            {
                return [$"> {shown}", mapFault];
            }

            // The handler checks the map against the maps of the run on the tick of the rules,
            // because the state can change before that tick (D-1133, T-7).
            this.queue(Intent.OfDebugMap(command.Action!, map!));
            return [$"> {shown}", $"the record takes the intent '{command.Action!.Value}' to '{map!.Value}' on the next tick"];
        }

        if (!TryTarget(command, words, out BattleTarget? target, out string fault))
        {
            return [$"> {shown}", fault];
        }

        ContentId? item = null;
        if (command.Action is ContentId action && string.CompareOrdinal(action.Value, DebugCommandIds.BattleItem.Value) == 0)
        {
            item = FirstItem(this.state());
            if (item is null)
            {
                return [$"> {shown}", "the pack holds no item, and the command sent nothing (D-775)"];
            }
        }

        List<string> answer = [$"> {shown}"];
        if (command.MakesIntent)
        {
            // The command changes the run on a tick of the rules, and never here. The host
            // takes the intent into the next tick, and the record holds it (D-171, T-7).
            this.queue(Intent.OfDebugConsole(command.Action!, target, item));
            answer.Add($"the record takes the intent '{command.Action!.Value}' on the next tick");
            return answer;
        }

        answer.AddRange(command.Report(this.state()).Split(LineBreak, StringSplitOptions.None));
        return answer;
    }

    /// <summary>Reads the slot argument of a command that aims, and refuses an argument on any other (D-767).</summary>
    private static bool TryTarget(DebugCommand command, string[] words, out BattleTarget? target, out string fault)
    {
        target = null;
        fault = string.Empty;
        if (command.TargetSide is not BattleSide side)
        {
            if (words.Length == 2)
            {
                fault = $"the command '{command.Name}' takes no argument (D-724)";
                return false;
            }

            return true;
        }

        if (words.Length != 2 || !int.TryParse(words[1], NumberStyles.None, CultureInfo.InvariantCulture, out int slot))
        {
            fault = $"the command '{command.Name}' takes the slot of its target, a whole number from 0 (D-767)";
            return false;
        }

        target = new BattleTarget(side, slot);
        return true;
    }

    /// <summary>Reads the map argument of the go-to-map command: one id of the kind `map` (D-646, D-1133).</summary>
    private static bool TryMap(DebugCommand command, string[] words, out ContentId? map, out string fault)
    {
        map = null;
        fault = $"the command '{command.Name}' takes the id of its map, such as '{MapIds.FirstMap.Value}' (D-1133)";
        if (words.Length != 2 || !ContentId.IsWellFormed(words[1]))
        {
            return false;
        }

        ContentId id = ContentId.Parse(words[1], "the debug console", command.Name);
        if (string.CompareOrdinal(id.Kind, GameMap.IdKind) != 0)
        {
            return false;
        }

        map = id;
        fault = string.Empty;
        return true;
    }

    /// <summary>Gives the first used-up item of the pack, in the ordinal order of the ids, or no value (D-775, D-1038). The pack holds spare gear too, which no fight uses.</summary>
    private static ContentId? FirstItem(RunState state)
    {
        foreach (PackValues entry in state.Characters.Pack)
        {
            if (string.CompareOrdinal(entry.Id.Kind, ItemList.Kind) == 0 && state.BattleContent.Item(entry.Id) is UsedUpItem)
            {
                return entry.Id;
            }
        }

        return null;
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
