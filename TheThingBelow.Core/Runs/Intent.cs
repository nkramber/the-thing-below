using System;
using TheThingBelow.Core.Battles;
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
/// <para>
/// A battle intent names its target, and an item use names its item too (D-764, D-780). The
/// row intent of the party window names its character as a target of the party side (D-558).
/// The pick intent of a choose step names the index of its option (D-1007). Every other intent
/// carries none of the three.
/// </para>
/// </remarks>
/// <param name="Action">The id of the choice, such as `intent.open_menu`.</param>
/// <param name="IsDebug">
/// True when the debug console of a development build made the intent (D-171, D-260).
/// </param>
/// <param name="Target">The side and the slot that a battle intent or a row intent aims at, or no value (D-558, D-764).</param>
/// <param name="Item">The item of an item use, or no value (D-780).</param>
/// <param name="Option">The index of the option of a pick, from zero, or no value (D-1007).</param>
public sealed record Intent(ContentId Action, bool IsDebug, BattleTarget? Target = null, ContentId? Item = null, int? Option = null)
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

    /// <summary>Makes a battle intent that the player made, with its target and its item (D-764, D-780).</summary>
    /// <param name="action">The id of the choice, such as `intent.battle_attack`.</param>
    /// <param name="target">The target, or no value.</param>
    /// <param name="item">The item, or no value.</param>
    /// <returns>The intent, with no debug mark.</returns>
    /// <exception cref="ArgumentNullException">The action is null (T-2).</exception>
    public static Intent OfPlayer(ContentId action, BattleTarget? target, ContentId? item)
    {
        ArgumentNullException.ThrowIfNull(action);

        return new Intent(action, false, target, item);
    }

    /// <summary>Makes a battle intent that the debug console made, with its target and its item (D-767).</summary>
    /// <param name="action">The id of the command, which a host handler reads (D-260).</param>
    /// <param name="target">The target, or no value.</param>
    /// <param name="item">The item, or no value.</param>
    /// <returns>The intent, with the debug mark.</returns>
    /// <exception cref="ArgumentNullException">The action is null (T-2).</exception>
    public static Intent OfDebugConsole(ContentId action, BattleTarget? target, ContentId? item)
    {
        ArgumentNullException.ThrowIfNull(action);

        return new Intent(action, true, target, item);
    }

    /// <summary>Makes the pick intent of a choose step that the player made (D-1007).</summary>
    /// <param name="option">The index of the option, from zero.</param>
    /// <returns>The intent, with no debug mark.</returns>
    public static Intent OfPick(int option) => new(IntentIds.StoryPick, false, null, null, option);

    /// <summary>Gives the intent as one line for an error message and a log line (T-2).</summary>
    /// <returns>The action, the item, the target, the option, and the debug mark, each when the intent carries it.</returns>
    public string Describe()
    {
        string target = this.Target is BattleTarget aimed ? $" at {aimed.Describe()}" : string.Empty;
        string item = this.Item is ContentId used ? $" with {used.Value}" : string.Empty;
        string option = this.Option is int picked ? $" option {picked}" : string.Empty;
        string mark = this.IsDebug ? " (debug)" : string.Empty;
        return $"{this.Action.Value}{item}{target}{option}{mark}";
    }
}
