using System;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Runs;

/// <summary>
/// One choice of the player in a tick. An intent names the choice in content ids and state
/// ids, and it never names a key, a button, or a screen position (D-84, D-493).
/// </summary>
/// <remarks>
/// Game makes an intent from each input event, and Core reads intents alone (D-493, F-50).
/// The input map, the remap, and the device kind stay in Game, so a rebind never changes a
/// record (D-214).
/// <para>
/// A debug intent carries the mark of <see cref="IsDebug"/>, so a run that used a debug
/// command still replays and the record says so (D-171). A host with no handler for that
/// action refuses the record, and the report names the intent and the tick (D-492, T-2).
/// </para>
/// </remarks>
/// <param name="Action">The id of the choice, such as `intent.open_menu`.</param>
/// <param name="IsDebug">
/// True when the debug console of a development build made the intent (D-171, D-260).
/// </param>
public sealed record Intent(ContentId Action, bool IsDebug)
{
    /// <summary>Makes an intent that the player made through a screen of the game.</summary>
    /// <param name="action">The id of the choice, such as `intent.open_menu`.</param>
    /// <returns>The intent, with no debug mark.</returns>
    /// <exception cref="ArgumentNullException">The action is null (T-2).</exception>
    public static Intent OfPlayer(ContentId action)
    {
        ArgumentNullException.ThrowIfNull(action);

        return new Intent(action, false);
    }

    /// <summary>Makes an intent that the debug console of a development build made (D-171).</summary>
    /// <param name="action">The id of the command, which a host handler reads (D-260).</param>
    /// <returns>The intent, with the debug mark.</returns>
    /// <exception cref="ArgumentNullException">The action is null (T-2).</exception>
    public static Intent OfDebugConsole(ContentId action)
    {
        ArgumentNullException.ThrowIfNull(action);

        return new Intent(action, true);
    }

    /// <summary>Gives the intent as one line for an error message and a log line (T-2).</summary>
    /// <returns>The action, and the debug mark when the intent carries one.</returns>
    public string Describe() => this.IsDebug ? $"{this.Action.Value} (debug)" : this.Action.Value;
}
