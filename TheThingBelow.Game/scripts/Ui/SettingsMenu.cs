using System;
using System.Collections.Generic;
using TheThingBelow.Storage;

namespace TheThingBelow.Game.Ui;

/// <summary>The five groups of the settings screen: the four of D-226, and accessibility (D-214).</summary>
public enum SettingsGroup
{
    /// <summary>The window mode, the fit, and the body size (D-618, D-874).</summary>
    Display,

    /// <summary>The four volumes, the mute in the background, and the mono toggle (D-435).</summary>
    Audio,

    /// <summary>The dead zone, the vibration, and the remap (D-434, D-861, D-862).</summary>
    Controls,

    /// <summary>The message speed and the remembered cursor (D-226).</summary>
    Battle,

    /// <summary>The flash and shake reduction and the text speed (D-214, D-870).</summary>
    Access,
}

/// <summary>One setting of the settings screen.</summary>
public enum SettingsItem
{
    /// <summary>The window mode (D-865).</summary>
    Window,

    /// <summary>The fit of the frame (D-232).</summary>
    Fit,

    /// <summary>The body size (D-874).</summary>
    Body,

    /// <summary>The master volume (D-867).</summary>
    Master,

    /// <summary>The music volume.</summary>
    Music,

    /// <summary>The volume of the sound effects.</summary>
    Effects,

    /// <summary>The ambience volume.</summary>
    Ambience,

    /// <summary>The mute while the window has no focus.</summary>
    MuteInBackground,

    /// <summary>The mono toggle.</summary>
    Mono,

    /// <summary>The dead zone of a stick (D-861).</summary>
    DeadZone,

    /// <summary>The vibration of the gamepad (D-434).</summary>
    Vibration,

    /// <summary>The bindings of one action of the game (D-862).</summary>
    Remap,

    /// <summary>The message speed of the battle screen (D-866, D-873).</summary>
    Messages,

    /// <summary>The remembered cursor of the command menu (D-226).</summary>
    RememberCursor,

    /// <summary>The flash and shake reduction (D-863).</summary>
    EffectLevel,

    /// <summary>The text speed (D-864).</summary>
    TextSpeed,
}

/// <summary>The two bindings that a remap row shows: the keyboard one and the gamepad one.</summary>
public enum BindingSlot
{
    /// <summary>The first key of the action.</summary>
    Keyboard,

    /// <summary>The first button or stick direction of the action.</summary>
    Gamepad,
}

/// <summary>One row of the settings screen.</summary>
/// <param name="Group">The group that holds the row.</param>
/// <param name="Item">The setting of the row.</param>
/// <param name="Action">The action of a remap row, and null for every other row.</param>
public sealed record SettingsRow(SettingsGroup Group, SettingsItem Item, string? Action);

/// <summary>
/// The rows, the cursor, and the changed values of the settings screen (D-226, D-872).
/// </summary>
/// <remarks>
/// The menu changes a copy of the settings, and the screen applies and writes the copy when
/// it closes, so no half of a change reaches the game. No move and no change makes an intent,
/// and no setting reaches a run record (D-493, T-7).
/// <para>
/// A remap can put one binding on two actions, and the screen then refuses to close until
/// the player moves one of them (D-862). The `ui_*` actions drive the screen and take no
/// remap, so the player can always reach every row (D-862).
/// </para>
/// <para>
/// A remap row waits for one input after a confirm. An input of the other device stops the
/// wait with no change, so a keyboard player on the gamepad slot is never stuck.
/// </para>
/// <para>
/// This type holds no Godot value, so a test reads it from the built Game assembly with no
/// engine (D-614).
/// </para>
/// </remarks>
public sealed class SettingsMenu
{
    private static readonly SettingsRow[] FixedRows =
    [
        new(SettingsGroup.Display, SettingsItem.Window, null),
        new(SettingsGroup.Display, SettingsItem.Fit, null),
        new(SettingsGroup.Display, SettingsItem.Body, null),
        new(SettingsGroup.Audio, SettingsItem.Master, null),
        new(SettingsGroup.Audio, SettingsItem.Music, null),
        new(SettingsGroup.Audio, SettingsItem.Effects, null),
        new(SettingsGroup.Audio, SettingsItem.Ambience, null),
        new(SettingsGroup.Audio, SettingsItem.MuteInBackground, null),
        new(SettingsGroup.Audio, SettingsItem.Mono, null),
        new(SettingsGroup.Battle, SettingsItem.Messages, null),
        new(SettingsGroup.Battle, SettingsItem.RememberCursor, null),
        new(SettingsGroup.Access, SettingsItem.EffectLevel, null),
        new(SettingsGroup.Access, SettingsItem.TextSpeed, null),
        new(SettingsGroup.Controls, SettingsItem.DeadZone, null),
        new(SettingsGroup.Controls, SettingsItem.Vibration, null),
    ];

    /// <summary>Opens the menu over a copy of the settings, with the cursor on the first row.</summary>
    /// <param name="settings">The settings in use.</param>
    /// <exception cref="ArgumentNullException">The settings are null (T-2).</exception>
    public SettingsMenu(GameSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        this.Settings = settings;
    }

    /// <summary>
    /// The rows in the order of the cursor: display, audio, battle, and accessibility in the
    /// left column, and the controls with one remap row for each action in the right column.
    /// </summary>
    public static IReadOnlyList<SettingsRow> Rows { get; } = BuildRows();

    /// <summary>The settings with every change of this menu.</summary>
    public GameSettings Settings { get; private set; }

    /// <summary>The row of the cursor.</summary>
    public int Cursor { get; private set; }

    /// <summary>The binding of a remap row that the cursor stands on.</summary>
    public BindingSlot Slot { get; private set; } = BindingSlot.Keyboard;

    /// <summary>True while a remap row waits for the input of the player (D-862).</summary>
    public bool Capturing { get; private set; }

    /// <summary>The bindings that two or more actions hold, which block the close (D-862).</summary>
    public IReadOnlyList<BindingConflict> Conflicts => this.Settings.Controls.Bindings.FindConflicts();

    /// <summary>True when no conflict stays, so the screen can close and write the file (D-862).</summary>
    public bool CanClose => this.Conflicts.Count == 0;

    /// <summary>
    /// The conflict that the line of the screen names: the conflict of the action under the
    /// cursor, or else the first, with its place among all conflicts (D-862, D-1119). Each other
    /// conflict shows as a marked cell of the grid.
    /// </summary>
    /// <returns>The conflict, its place from 1, and the count, or no value when no conflict stays.</returns>
    public ShownConflict? ConflictToName()
    {
        IReadOnlyList<BindingConflict> conflicts = this.Conflicts;
        if (conflicts.Count == 0)
        {
            return null;
        }

        string? action = this.Current.Action;
        for (int index = 0; index < conflicts.Count; index += 1)
        {
            if (action is not null && Holds(conflicts[index].Actions, action))
            {
                return new ShownConflict(conflicts[index], index + 1, conflicts.Count);
            }
        }

        return new ShownConflict(conflicts[0], 1, conflicts.Count);
    }

    /// <summary>
    /// Tells whether the binding that one slot of one action shows sits in a conflict, so the
    /// screen marks that cell (D-862, D-1119).
    /// </summary>
    /// <param name="bindings">The bindings.</param>
    /// <param name="action">The action of the row.</param>
    /// <param name="slot">The slot of the cell.</param>
    /// <returns>True when a conflict holds the shown binding and the action.</returns>
    public static bool InConflict(ControlBindings bindings, string action, BindingSlot slot)
    {
        ArgumentNullException.ThrowIfNull(bindings);
        ArgumentException.ThrowIfNullOrEmpty(action);

        if (BindingIn(bindings, action, slot) is not InputBinding shown)
        {
            return false;
        }

        foreach (BindingConflict conflict in bindings.FindConflicts())
        {
            if (conflict.Binding.Equals(shown) && Holds(conflict.Actions, action))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>The row of the cursor.</summary>
    public SettingsRow Current => Rows[this.Cursor];

    /// <summary>Gives the place of the remap row of one action in <see cref="Rows"/>.</summary>
    /// <param name="action">The name of the action.</param>
    /// <returns>The place of the row.</returns>
    /// <exception cref="ArgumentException">No remap row holds the action (T-2).</exception>
    public static int RowOf(string action)
    {
        for (int index = 0; index < Rows.Count; index += 1)
        {
            if (string.CompareOrdinal(Rows[index].Action, action) == 0)
            {
                return index;
            }
        }

        throw new ArgumentException($"No remap row holds the action '{action}'. The actions are {InputActions.Describe()} (T-2).", nameof(action));
    }

    private static bool Holds(IReadOnlyList<string> actions, string action)
    {
        foreach (string held in actions)
        {
            if (string.CompareOrdinal(held, action) == 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Gives the binding that one slot of one action shows.</summary>
    /// <param name="bindings">The bindings.</param>
    /// <param name="action">The action.</param>
    /// <param name="slot">The slot.</param>
    /// <returns>The first key for the keyboard, the first button or stick for the gamepad, or null when the action has none.</returns>
    public static InputBinding? BindingIn(ControlBindings bindings, string action, BindingSlot slot)
    {
        ArgumentNullException.ThrowIfNull(bindings);

        foreach (InputBinding binding in bindings.Of(action))
        {
            if (SlotOf(binding) == slot)
            {
                return binding;
            }
        }

        return null;
    }

    /// <summary>Gives the slot of one binding: a key is the keyboard, and a button or a stick is the gamepad.</summary>
    /// <param name="binding">The binding.</param>
    /// <returns>The slot.</returns>
    public static BindingSlot SlotOf(InputBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);

        return binding.Kind == BindingKind.Key ? BindingSlot.Keyboard : BindingSlot.Gamepad;
    }

    /// <summary>Moves the cursor by one row, and wraps at each end. A wait for a remap ignores it.</summary>
    /// <param name="step">-1 for the row above, and 1 for the row below.</param>
    /// <exception cref="ArgumentOutOfRangeException">The step is not -1 or 1 (T-2).</exception>
    public void Move(int step)
    {
        CheckStep(step);
        if (this.Capturing)
        {
            return;
        }

        this.Cursor = (this.Cursor + step + Rows.Count) % Rows.Count;
    }

    /// <summary>Puts the cursor on one row and one slot, which the mouse points at (D-872).</summary>
    /// <param name="row">The place of the row in <see cref="Rows"/>.</param>
    /// <param name="slot">The slot under the pointer, which a remap row alone reads.</param>
    /// <exception cref="ArgumentOutOfRangeException">The row is outside the list (T-2).</exception>
    public void Point(int row, BindingSlot slot)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(row);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, Rows.Count);
        if (this.Capturing)
        {
            return;
        }

        this.Cursor = row;
        this.Slot = slot;
    }

    /// <summary>
    /// Changes the value of the row by one step, and stops at each end. On a remap row, the
    /// step moves between the two slots.
    /// </summary>
    /// <param name="step">-1 for the left, and 1 for the right.</param>
    /// <exception cref="ArgumentOutOfRangeException">The step is not -1 or 1 (T-2).</exception>
    public void Change(int step)
    {
        CheckStep(step);
        if (this.Capturing)
        {
            return;
        }

        if (this.Current.Item == SettingsItem.Remap)
        {
            this.Slot = step < 0 ? BindingSlot.Keyboard : BindingSlot.Gamepad;
            return;
        }

        this.Settings = this.Stepped(this.Current.Item, step, wrap: false);
    }

    /// <summary>
    /// Chooses the row: a value row steps to its next value and wraps, and a remap row waits
    /// for the input of the player (D-872).
    /// </summary>
    public void Choose()
    {
        if (this.Capturing)
        {
            return;
        }

        if (this.Current.Item == SettingsItem.Remap)
        {
            this.Capturing = true;
            return;
        }

        this.Settings = this.Stepped(this.Current.Item, 1, wrap: true);
    }

    /// <summary>Gives the input of the player to a remap row that waits for it (D-862).</summary>
    /// <param name="binding">The key, the button, or the stick direction that the player pressed.</param>
    /// <returns>True when the menu took the input: a remap, or the stop of the wait.</returns>
    /// <exception cref="ArgumentNullException">The binding is null (T-2).</exception>
    /// <remarks>
    /// An input of the slot replaces the binding of that slot, and it can sit on another action
    /// too. An input of the other device stops the wait with no change.
    /// </remarks>
    public bool Capture(InputBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);
        if (!this.Capturing)
        {
            return false;
        }

        this.Capturing = false;
        if (SlotOf(binding) != this.Slot)
        {
            return true;
        }

        string action = this.Current.Action ?? throw new InvalidOperationException(
            $"The remap row {this.Cursor} names no action (T-2).");
        ControlBindings bindings = this.Settings.Controls.Bindings;
        ControlBindings changed = bindings.Rebind(action, BindingIn(bindings, action, this.Slot), binding);
        this.Settings = this.Settings with { Controls = this.Settings.Controls with { Bindings = changed } };
        return true;
    }

    private static IReadOnlyList<SettingsRow> BuildRows()
    {
        List<SettingsRow> rows = new(FixedRows);
        foreach (string action in InputActions.Names)
        {
            rows.Add(new SettingsRow(SettingsGroup.Controls, SettingsItem.Remap, action));
        }

        return rows;
    }

    private static void CheckStep(int step)
    {
        if (step != -1 && step != 1)
        {
            throw new ArgumentOutOfRangeException(nameof(step), step, "A step of the settings menu is -1 or 1 (T-2).");
        }
    }

    /// <summary>Gives the next value of a list, with a stop or a wrap at each end.</summary>
    private static T Next<T>(T[] values, T value, int step, bool wrap)
        where T : struct, Enum
    {
        int place = Array.IndexOf(values, value);
        if (place < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "The value is not in its list (T-2).");
        }

        int next = place + step;
        if (wrap)
        {
            next = (next + values.Length) % values.Length;
        }

        return values[Math.Clamp(next, 0, values.Length - 1)];
    }

    /// <summary>Gives the next number of a range, with a stop or a wrap at each end.</summary>
    private static int NextNumber(int value, int lowest, int highest, int size, int step, bool wrap)
    {
        int next = value + (step * size);
        if (wrap && next > highest)
        {
            return lowest;
        }

        return Math.Clamp(next, lowest, highest);
    }

    private GameSettings Stepped(SettingsItem item, int step, bool wrap)
    {
        GameSettings now = this.Settings;
        DisplaySettings display = now.Display;
        AudioSettings audio = now.Audio;
        ControlSettings controls = now.Controls;
        BattleSettings battle = now.Battle;
        AccessSettings access = now.Access;
        int lowest = GameSettings.LowestVolume;
        int highest = GameSettings.HighestVolume;

        return item switch
        {
            SettingsItem.Window => now with { Display = display with { Window = Next(Enum.GetValues<WindowSetting>(), display.Window, step, wrap) } },
            SettingsItem.Fit => now with { Display = display with { Fit = Next(Enum.GetValues<FitSetting>(), display.Fit, step, wrap) } },
            SettingsItem.Body => now with { Display = display with { Body = Next(Enum.GetValues<BodySetting>(), display.Body, step, wrap) } },
            SettingsItem.Master => now with { Audio = audio with { Master = NextNumber(audio.Master, lowest, highest, 1, step, wrap) } },
            SettingsItem.Music => now with { Audio = audio with { Music = NextNumber(audio.Music, lowest, highest, 1, step, wrap) } },
            SettingsItem.Effects => now with { Audio = audio with { Effects = NextNumber(audio.Effects, lowest, highest, 1, step, wrap) } },
            SettingsItem.Ambience => now with { Audio = audio with { Ambience = NextNumber(audio.Ambience, lowest, highest, 1, step, wrap) } },
            SettingsItem.MuteInBackground => now with { Audio = audio with { MuteInBackground = !audio.MuteInBackground } },
            SettingsItem.Mono => now with { Audio = audio with { Mono = !audio.Mono } },
            SettingsItem.DeadZone => now with
            {
                Controls = controls with
                {
                    DeadZone = NextNumber(
                        controls.DeadZone, GameSettings.LowestDeadZone, GameSettings.HighestDeadZone, GameSettings.DeadZoneStep, step, wrap),
                },
            },
            SettingsItem.Vibration => now with { Controls = controls with { Vibration = !controls.Vibration } },
            SettingsItem.Messages => now with { Battle = battle with { Messages = Next(Enum.GetValues<MessageSpeed>(), battle.Messages, step, wrap) } },
            SettingsItem.RememberCursor => now with { Battle = battle with { RememberCursor = !battle.RememberCursor } },
            SettingsItem.EffectLevel => now with { Access = access with { Effects = Next(Enum.GetValues<EffectLevel>(), access.Effects, step, wrap) } },
            SettingsItem.TextSpeed => now with { Access = access with { Text = Next(Enum.GetValues<TextSpeed>(), access.Text, step, wrap) } },
            _ => throw new ArgumentOutOfRangeException(nameof(item), item, "The row holds no value to step (T-2)."),
        };
    }
}

/// <summary>The conflict that the line of the settings screen names (D-862, D-1119).</summary>
/// <param name="Conflict">The binding and the actions that hold it.</param>
/// <param name="Place">The place of the conflict among all conflicts, from 1.</param>
/// <param name="Count">The count of conflicts.</param>
public sealed record ShownConflict(BindingConflict Conflict, int Place, int Count);
