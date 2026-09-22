using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The remembered cursor of the command menu: the last action of each character (D-226).
/// The setting starts off (D-868).
/// </summary>
/// <remarks>
/// The memory lives in Game for the whole session, so it lasts from one fight to the next.
/// A move of the cursor makes no intent, so the memory never reaches a run record (D-493,
/// T-7). The memory keeps each action while the setting is off, so a switch on shows the
/// last action at once.
/// <para>
/// This type holds no Godot value, so a test reads it from the built Game assembly with no
/// engine (D-614).
/// </para>
/// </remarks>
public sealed class CommandMemory
{
    private readonly SortedDictionary<string, BattleAction> actions = new(StringComparer.Ordinal);

    /// <summary>Makes an empty memory.</summary>
    /// <param name="enabled">The remembered cursor setting of the battle group (D-226).</param>
    public CommandMemory(bool enabled) => this.Enabled = enabled;

    /// <summary>True when the menu opens on the last action of the character (D-226).</summary>
    public bool Enabled { get; set; }

    /// <summary>Gives the action that the cursor opens on for one character.</summary>
    /// <param name="character">The content id of the character.</param>
    /// <returns>The last action of the character with the setting on, and no value otherwise.</returns>
    public BattleAction? StartOf(ContentId character)
    {
        if (!this.Enabled)
        {
            return null;
        }

        return this.actions.TryGetValue(character.Value, out BattleAction last) ? last : null;
    }

    /// <summary>Keeps the action of a whole choice of one character.</summary>
    /// <param name="character">The content id of the character.</param>
    /// <param name="action">The action of the choice.</param>
    public void Keep(ContentId character, BattleAction action) => this.actions[character.Value] = action;
}
