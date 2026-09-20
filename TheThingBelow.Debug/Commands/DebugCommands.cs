using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Debug.Commands;

/// <summary>
/// The commands of the debug console of this build (D-171, D-724). A development build alone
/// holds this class, because the assembly that carries it never reaches a release export
/// (D-260, D-492).
/// </summary>
/// <remarks>
/// One command changes the run: `reveal`. It sends a debug intent, and the handler of that
/// intent marks every tile of the map as walked (D-567). Three commands report and change
/// nothing: `help`, `hash`, and `where` (D-724).
/// <para>
/// Each later PR that gives the rules a new value adds its own commands here, such as the
/// health of a character in PR-67 and the story flags of PR-68.
/// </para>
/// </remarks>
public static class DebugCommands
{
    /// <summary>The name of the command that lists every command.</summary>
    public const string HelpName = "help";

    /// <summary>The name of the command that reveals the map.</summary>
    public const string RevealName = "reveal";

    /// <summary>The name of the command that gives the state hash.</summary>
    public const string HashName = "hash";

    /// <summary>The name of the command that gives the place of the party.</summary>
    public const string WhereName = "where";

    // The order of this list is the order of `help`, and it never follows a hash of a name
    // (G-4). The list is short, so a walk of it reads better than a map of one entry (T-1).
    private static readonly IReadOnlyList<DebugCommand> Commands =
    [
        DebugCommand.OfIntent(
            RevealName,
            "marks every tile of the map as walked",
            DebugCommandIds.RevealMap,
            RevealMap),
        DebugCommand.OfReport(HashName, "gives the state hash of the run", HashOf),
        DebugCommand.OfReport(WhereName, "gives the tick and the place of the party", PlaceOf),
        DebugCommand.OfReport(HelpName, "lists every command", HelpOf),
    ];

    /// <summary>Every command of this build, in the order that `help` prints (D-724).</summary>
    public static IReadOnlyList<DebugCommand> All => Commands;

    /// <summary>
    /// The handlers that a development build passes to <see cref="Simulation.Start"/> through
    /// the seam of D-260. A release build passes <see cref="DebugIntentHandlers.None"/>.
    /// </summary>
    /// <returns>One handler for each command that sends an intent.</returns>
    public static DebugIntentHandlers Handlers()
    {
        List<KeyValuePair<ContentId, DebugIntentHandler>> handlers = [];
        foreach (DebugCommand command in Commands)
        {
            if (command.Action is not null && command.Handler is not null)
            {
                handlers.Add(new KeyValuePair<ContentId, DebugIntentHandler>(
                    command.Action,
                    command.Handler));
            }
        }

        return new DebugIntentHandlers(handlers);
    }

    /// <summary>Finds the command of one name.</summary>
    /// <param name="name">The word that the person typed.</param>
    /// <param name="command">The command, or null when no command takes that name.</param>
    /// <returns>True when a command takes that name.</returns>
    /// <exception cref="ArgumentException">The name is empty (T-2).</exception>
    public static bool TryFind(string name, out DebugCommand? command)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        foreach (DebugCommand each in Commands)
        {
            if (string.CompareOrdinal(each.Name, name) == 0)
            {
                command = each;
                return true;
            }
        }

        command = null;
        return false;
    }

    /// <summary>
    /// Marks every tile of the map as walked (D-567). The map HUD of PR-64 draws the record
    /// of the walked tiles, so the command shows the whole map.
    /// </summary>
    private static void RevealMap(RunState state, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);

        MapState party = state.Party;
        for (int row = 0; row < party.Map.Height; row += 1)
        {
            for (int column = 0; column < party.Map.Width; column += 1)
            {
                party.Walked.Mark(new TilePoint(column, row));
            }
        }
    }

    /// <summary>Gives the state hash of the run, which a replay compares (G-5).</summary>
    private static string HashOf(RunState state) => $"the state hash is 0x{state.StateHash():x16}";

    /// <summary>Gives the tick, the map, the tile of the lead, and the count of walked tiles.</summary>
    private static string PlaceOf(RunState state) =>
        $"tick {state.Tick} on the map '{state.Party.Map.Id.Value}', "
        + $"the lead at {state.Party.LeadAt} facing {state.Party.Facing}, "
        + $"with {state.Party.Walked.Count} walked tiles";

    /// <summary>Lists every command of this build, one on each line.</summary>
    private static string HelpOf(RunState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        List<string> lines = [];
        foreach (DebugCommand command in Commands)
        {
            string mark = command.MakesIntent ? " (the record holds it)" : string.Empty;
            lines.Add($"{command.Name}: {command.Summary}{mark}");
        }

        // The console splits a report on this character, and a log line of the smoke session
        // then reads the same on every system (D-117).
        return string.Join(DebugSession.LineBreak, lines);
    }
}
