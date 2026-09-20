using System;
using System.Collections.Generic;
using Godot;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The step actions that the player holds down now (D-493, F-50). The party walks while a
/// direction is held, so the map needs the state of each key and not the press alone.
/// </summary>
/// <remarks>
/// The state comes from the press events and the release events, and never from a poll of
/// the input singleton. A poll ignores what a menu already took, and it would make a second
/// intent for one press (F-50).
/// <para>
/// The newest held action wins, so a player who presses a second direction turns at once and
/// never waits for the first key to come up. A step goes in four directions alone, so no two
/// held keys make a diagonal (D-716).
/// </para>
/// <para>
/// <see cref="Press"/>, <see cref="Release"/>, and <see cref="Newest"/> hold no Godot value,
/// so a test reads them with no engine (D-614).
/// </para>
/// </remarks>
public sealed class HeldSteps
{
    // The oldest held action sits first, and the newest sits last (G-4).
    private readonly List<string> held = [];

    /// <summary>The step action that the player pressed last, or no value while none is down.</summary>
    public string? Newest => this.held.Count == 0 ? null : this.held[^1];

    /// <summary>Reads one input event, and records each step action that it changed.</summary>
    /// <param name="signal">The event of this frame.</param>
    /// <returns>True when the set of held actions changed.</returns>
    /// <exception cref="ArgumentNullException">The event is null (T-2).</exception>
    public bool Read(InputEvent signal)
    {
        ArgumentNullException.ThrowIfNull(signal);

        bool changed = false;
        foreach (string action in InputActions.StepNames)
        {
            if (signal.IsActionPressed(action))
            {
                changed |= this.Press(action);
            }
            else if (signal.IsActionReleased(action))
            {
                changed |= this.Release(action);
            }
        }

        return changed;
    }

    /// <summary>Records that the player pressed one step action.</summary>
    /// <param name="action">The name of the action, such as `step_north`.</param>
    /// <returns>True when the action was not held before.</returns>
    /// <exception cref="ArgumentException">The name is empty (T-2).</exception>
    public bool Press(string action)
    {
        ArgumentException.ThrowIfNullOrEmpty(action);

        if (this.held.Contains(action))
        {
            return false;
        }

        this.held.Add(action);
        return true;
    }

    /// <summary>Records that the player released one step action.</summary>
    /// <param name="action">The name of the action, such as `step_north`.</param>
    /// <returns>True when the action was held before.</returns>
    /// <exception cref="ArgumentException">The name is empty (T-2).</exception>
    public bool Release(string action)
    {
        ArgumentException.ThrowIfNullOrEmpty(action);

        return this.held.Remove(action);
    }

    /// <summary>Forgets every held action, which a screen change needs (T-2).</summary>
    public void Clear() => this.held.Clear();
}
