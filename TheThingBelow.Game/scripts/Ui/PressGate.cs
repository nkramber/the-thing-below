using System;
using System.Collections.Generic;
using Godot;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// Passes the first press of each hold of an action, and stops each later press until every
/// source of the action comes up again (D-1077, F-107).
/// </summary>
/// <remarks>
/// Godot gives a press on each motion event of a stick past the dead zone, so one push of the
/// stick moved the cursor of a menu many lines. A pad that the system reports as two devices
/// gives two presses of one button, so one press of the menu button opened the menu and closed
/// it. The gate reads the press and the release of each source, and a press of an action that
/// a source already holds goes no further (F-107).
/// <para>
/// A source is one key, one button of one pad, or one stick axis of one pad. A release always
/// goes on, so <see cref="HeldSteps"/> never keeps a step that the player let go.
/// </para>
/// <para>
/// <see cref="Press"/>, <see cref="Release"/>, <see cref="Clear"/>, and
/// <see cref="ForgetPad"/> hold no Godot value, so a test reads them with no engine (D-614).
/// </para>
/// </remarks>
public sealed class PressGate
{
    /// <summary>The built-in actions that the menus read (D-862).</summary>
    public static readonly IReadOnlyList<string> MenuNames =
        ["ui_accept", "ui_cancel", "ui_up", "ui_down", "ui_left", "ui_right"];

    /// <summary>The start of the source name of each pad input, before the device number.</summary>
    private const string PadPrefix = "pad ";

    // The sources that hold each action now, by action name (G-4).
    private readonly SortedDictionary<string, List<string>> holders = new(StringComparer.Ordinal);

    /// <summary>Reads one input event, and tells whether it goes on to the game.</summary>
    /// <param name="signal">The event of this frame.</param>
    /// <returns>
    /// False for a press of one or more actions that another press holds already. True for
    /// every other event: a first press, a release, and an event of no action.
    /// </returns>
    /// <exception cref="ArgumentNullException">The event is null (T-2).</exception>
    /// <remarks>
    /// The gate reads an action only when the input map holds it, because a read of an absent
    /// action writes an error line, and the capture session builds no map of the game (T-2).
    /// </remarks>
    public bool Read(InputEvent signal)
    {
        ArgumentNullException.ThrowIfNull(signal);

        string? source = SourceOf(signal);
        if (source is null)
        {
            return true;
        }

        bool pressed = false;
        bool first = false;
        foreach (string action in ReadNames())
        {
            if (!InputMap.HasAction(action))
            {
                continue;
            }

            if (signal.IsActionPressed(action, allowEcho: true))
            {
                pressed = true;
                first |= this.Press(action, source);
            }
            else if (signal.IsActionReleased(action))
            {
                this.Release(action, source);
            }
        }

        return !pressed || first;
    }

    /// <summary>Records that one source pressed one action.</summary>
    /// <param name="action">The name of the action, such as `menu`.</param>
    /// <param name="source">The name of the source, such as `pad 0 button 6`.</param>
    /// <returns>True when no source held the action before, which is the first press of a hold.</returns>
    /// <exception cref="ArgumentException">A name is empty (T-2).</exception>
    public bool Press(string action, string source)
    {
        ArgumentException.ThrowIfNullOrEmpty(action);
        ArgumentException.ThrowIfNullOrEmpty(source);

        if (!this.holders.TryGetValue(action, out List<string>? sources))
        {
            sources = [];
            this.holders.Add(action, sources);
        }

        bool first = sources.Count == 0;
        if (!sources.Contains(source))
        {
            sources.Add(source);
        }

        return first;
    }

    /// <summary>Records that one source released one action.</summary>
    /// <param name="action">The name of the action, such as `menu`.</param>
    /// <param name="source">The name of the source, such as `pad 0 button 6`.</param>
    /// <returns>True when the source held the action before.</returns>
    /// <exception cref="ArgumentException">A name is empty (T-2).</exception>
    public bool Release(string action, string source)
    {
        ArgumentException.ThrowIfNullOrEmpty(action);
        ArgumentException.ThrowIfNullOrEmpty(source);

        return this.holders.TryGetValue(action, out List<string>? sources) && sources.Remove(source);
    }

    /// <summary>Tells whether any source holds one action now (D-1084).</summary>
    /// <param name="action">The name of the action, such as `step_north`.</param>
    /// <returns>True while a key, a button, or a stick holds the action.</returns>
    /// <exception cref="ArgumentException">The name is empty (T-2).</exception>
    public bool Holds(string action)
    {
        ArgumentException.ThrowIfNullOrEmpty(action);

        return this.holders.TryGetValue(action, out List<string>? sources) && sources.Count > 0;
    }

    /// <summary>
    /// Forgets every held source. The window calls it when it loses the focus, because the
    /// system then sends no release (T-2).
    /// </summary>
    public void Clear() => this.holders.Clear();

    /// <summary>Forgets every source of one pad, which a pad that disconnects needs (T-2).</summary>
    /// <param name="device">The device number of the pad.</param>
    /// <returns>The count of sources that the gate forgot.</returns>
    public int ForgetPad(int device)
    {
        string prefix = $"{PadPrefix}{device} ";
        int forgot = 0;
        foreach (List<string> sources in this.holders.Values)
        {
            forgot += sources.RemoveAll(source => source.StartsWith(prefix, StringComparison.Ordinal));
        }

        return forgot;
    }

    /// <summary>Names the source of one event.</summary>
    /// <param name="signal">The event of this frame.</param>
    /// <returns>The name of a key, a pad button, or a stick axis, or null for every other event.</returns>
    private static string? SourceOf(InputEvent signal) => signal switch
    {
        InputEventKey key => $"key {(long)(key.PhysicalKeycode != Godot.Key.None ? key.PhysicalKeycode : key.Keycode)}",
        InputEventJoypadButton button => $"{PadPrefix}{button.Device} button {(long)button.ButtonIndex}",
        InputEventJoypadMotion motion => $"{PadPrefix}{motion.Device} axis {(long)motion.Axis}",
        _ => null,
    };

    /// <summary>Gives each action that the gate reads: the actions of the game, then those of the menus.</summary>
    /// <returns>The names in a fixed order (G-4).</returns>
    private static IEnumerable<string> ReadNames()
    {
        foreach (string action in InputActions.Names)
        {
            yield return action;
        }

        foreach (string action in MenuNames)
        {
            yield return action;
        }
    }
}
