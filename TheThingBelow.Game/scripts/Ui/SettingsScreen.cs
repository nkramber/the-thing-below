using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using TheThingBelow.Core.Content;
using TheThingBelow.Storage;

namespace TheThingBelow.Game.Ui;

/// <summary>What one input event did to the settings screen.</summary>
public enum SettingsOutcome
{
    /// <summary>The screen stays open.</summary>
    Stay,

    /// <summary>The player closed the screen, and no conflict stays (D-862).</summary>
    Close,
}

/// <summary>
/// The settings screen: five groups in two columns over the paused world (D-226, D-871). The
/// `ui_*` actions and the mouse drive it (D-862, D-872).
/// </summary>
/// <remarks>
/// The screen draws the rows of <see cref="SettingsMenu"/>, and it holds no rule of its own.
/// The screen builds its nodes in code, and every label takes its text from the string table
/// through the text helper (G-7, D-499).
/// <para>
/// The screen shows the frame through a texture, so a mouse event of the window carries a
/// device pixel. The screen reads the frame pixel from <see cref="ScreenFit.ToFrame"/>, and it
/// finds the row under the pointer by the place of each label (D-872).
/// </para>
/// </remarks>
public sealed class SettingsScreen
{
    /// <summary>The left edge of each column, in frame pixels.</summary>
    private static readonly int[] ColumnLefts = [48, 672];

    /// <summary>The width of a label of a value row, in frame pixels.</summary>
    private const int NameWidth = 300;

    /// <summary>The width of the value of a value row, in frame pixels.</summary>
    private const int ValueWidth = 260;

    /// <summary>The width of the name of a remap row, in frame pixels.</summary>
    private const int ActionWidth = 180;

    /// <summary>
    /// The width of the keyboard slot of a remap row, in frame pixels. The gamepad slot takes
    /// the rest of the column, because the longest button name holds 13 characters.
    /// </summary>
    private const int SlotWidth = 150;

    /// <summary>The width of the gamepad slot of a remap row, to the inner edge of the panel.</summary>
    private const int PadWidth = 214;

    /// <summary>The top of the first line under the title, in frame pixels.</summary>
    private const int FirstLineTop = 104;

    /// <summary>The frame pixels between two lines. A body of 32 fills the left column to row 644 with it.</summary>
    private const int LineGap = 4;

    /// <summary>The top of the help line, in frame pixels.</summary>
    private const int HelpTop = 668;

    /// <summary>The group that starts the right column. The rows before it fill the left column.</summary>
    private static readonly SettingsGroup FirstRightGroup = SettingsGroup.Access;

    private readonly UiBase ui;
    private readonly StringTable strings;
    private readonly Control layer;
    private readonly List<RowNodes> rows = [];
    private readonly Label help;
    private readonly Color chosenColor;
    private readonly Color dimColor;
    private readonly Color warningColor;
    private readonly int autoBody;

    private SettingsScreen(UiBase ui, StringTable strings, FrameRoot frame, GameSettings settings, int autoBody)
    {
        this.ui = ui;
        this.strings = strings;
        this.autoBody = autoBody;
        this.Menu = new SettingsMenu(settings);
        this.chosenColor = ui.Theme.ColorOf("text_chosen");
        this.dimColor = ui.Theme.ColorOf("text_dim");
        this.warningColor = ui.Theme.ColorOf("text_warning");

        this.layer = new Control
        {
            Position = Vector2.Zero,
            Size = new Vector2(ScreenFit.FrameWidth, ScreenFit.FrameHeight),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Theme = ui.Theme.Theme,
        };
        frame.Layer.AddChild(this.layer);

        var panel = new Panel
        {
            Position = new Vector2(UiTheme.FrameEdge * 2, UiTheme.FrameEdge * 2),
            Size = new Vector2(ScreenFit.FrameWidth - (UiTheme.FrameEdge * 4), ScreenFit.FrameHeight - (UiTheme.FrameEdge * 4)),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        this.layer.AddChild(panel);

        var title = new Label { Position = new Vector2(ColumnLefts[0], 32), ThemeTypeVariation = UiTheme.TitleVariation };
        ui.Text.Put(title, Id("settings.title"));
        this.layer.AddChild(title);

        this.help = new Label { Position = new Vector2(ColumnLefts[0], HelpTop), Size = new Vector2(1184, ui.Theme.BodySize) };
        this.layer.AddChild(this.help);

        this.BuildRows();
    }

    /// <summary>The rows, the cursor, and the changed settings.</summary>
    public SettingsMenu Menu { get; }

    /// <summary>Builds the settings screen over the frame, with the cursor on the first row.</summary>
    /// <param name="frame">The frame, whose UI layer takes the screen.</param>
    /// <param name="ui">The atlas, the theme, and the text helper.</param>
    /// <param name="strings">The string table, which the conflict line reads the names from.</param>
    /// <param name="settings">The settings in use.</param>
    /// <param name="autoBody">The body size that auto gives on this screen now, which the body row shows (D-874).</param>
    /// <returns>The screen, which the caller frees when it closes.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static SettingsScreen Build(FrameRoot frame, UiBase ui, StringTable strings, GameSettings settings, int autoBody)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(ui);
        ArgumentNullException.ThrowIfNull(strings);
        ArgumentNullException.ThrowIfNull(settings);

        var screen = new SettingsScreen(ui, strings, frame, settings, autoBody);
        screen.Show();
        return screen;
    }

    /// <summary>Removes every node of the screen.</summary>
    public void Free()
    {
        this.layer.QueueFree();
    }

    /// <summary>Gives one input event to the screen (D-862, D-872).</summary>
    /// <param name="signal">The event.</param>
    /// <param name="fit">The fit of the frame, which turns a mouse point into a frame point.</param>
    /// <param name="menuAction">The name of the menu action of the game, which also closes the screen.</param>
    /// <returns>Whether the screen closes.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <remarks>
    /// A remap row that waits takes the next key, button, or stick push first, so the player
    /// can bind any input, the keys of the menus included.
    /// </remarks>
    public SettingsOutcome Read(InputEvent signal, ScreenFit fit, string menuAction)
    {
        ArgumentNullException.ThrowIfNull(signal);
        ArgumentNullException.ThrowIfNull(fit);
        ArgumentException.ThrowIfNullOrEmpty(menuAction);

        SettingsOutcome outcome = this.ReadEvent(signal, fit, menuAction);
        this.Show();
        return outcome;
    }

    /// <summary>Draws each row with its value, the cursor, and the help line or the conflict.</summary>
    public void Show()
    {
        GameSettings settings = this.Menu.Settings;
        for (int index = 0; index < this.rows.Count; index += 1)
        {
            RowNodes nodes = this.rows[index];
            bool chosen = index == this.Menu.Cursor;
            this.Paint(nodes.Name, chosen && nodes.Row.Item != SettingsItem.Remap);

            if (nodes.Row.Item == SettingsItem.Remap)
            {
                string action = nodes.Row.Action!;
                this.ShowSlot(nodes.Key!, settings.Controls.Bindings, action, BindingSlot.Keyboard, chosen);
                this.ShowSlot(nodes.Pad!, settings.Controls.Bindings, action, BindingSlot.Gamepad, chosen);
                continue;
            }

            (string id, Dictionary<string, string> values) = this.ValueOf(nodes.Row.Item, settings);
            this.ui.Text.Put(nodes.Value!, Id(id), values);
            this.Paint(nodes.Value!, chosen);
        }

        this.ShowHelp();
    }

    private static ContentId Id(string value) => ContentId.Parse(value, StringTable.Path, nameof(SettingsScreen));

    private static Dictionary<string, string> Values(params (string Key, string Value)[] pairs)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach ((string key, string value) in pairs)
        {
            values.Add(key, value);
        }

        return values;
    }

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static string OnOff(bool on) => on ? "settings.on" : "settings.off";

    private static string SpeedId(int place) => place switch
    {
        0 => "settings.speed_slow",
        1 => "settings.speed_normal",
        2 => "settings.speed_fast",
        _ => throw new ArgumentOutOfRangeException(nameof(place), place, "A speed takes one of three places (T-2)."),
    };

    private static string NameIdOf(SettingsRow row) => row.Item switch
    {
        SettingsItem.Window => "settings.window",
        SettingsItem.Fit => "settings.fit",
        SettingsItem.Body => "settings.body",
        SettingsItem.Master => "settings.master",
        SettingsItem.Music => "settings.music",
        SettingsItem.Effects => "settings.effects",
        SettingsItem.Ambience => "settings.ambience",
        SettingsItem.MuteInBackground => "settings.mute_background",
        SettingsItem.Mono => "settings.mono",
        SettingsItem.DeadZone => "settings.dead_zone",
        SettingsItem.Vibration => "settings.vibration",
        SettingsItem.Remap => $"settings.action_{row.Action}",
        SettingsItem.Messages => "settings.messages",
        SettingsItem.RememberCursor => "settings.remember_cursor",
        SettingsItem.EffectLevel => "settings.effect_level",
        SettingsItem.TextSpeed => "settings.text_speed",
        _ => throw new ArgumentOutOfRangeException(nameof(row), row.Item, "The row has no label (T-2)."),
    };

    private static string GroupIdOf(SettingsGroup group) => group switch
    {
        SettingsGroup.Display => "settings.group_display",
        SettingsGroup.Audio => "settings.group_audio",
        SettingsGroup.Controls => "settings.group_controls",
        SettingsGroup.Battle => "settings.group_battle",
        SettingsGroup.Access => "settings.group_access",
        _ => throw new ArgumentOutOfRangeException(nameof(group), group, "The group has no label (T-2)."),
    };

    /// <summary>Gives the string id and the values of the name of one binding.</summary>
    private static (string Id, Dictionary<string, string> Values) BindingText(InputBinding? binding)
    {
        if (binding is null)
        {
            return ("settings.binding_none", Values());
        }

        switch (binding.Kind)
        {
            case BindingKind.Key:
                return ("settings.binding_key", Values(("key", OS.GetKeycodeString((Godot.Key)binding.Code))));
            case BindingKind.Button when binding.Code <= (int)JoyButton.Touchpad:
                return ($"settings.button_{Number(binding.Code)}", Values());
            case BindingKind.Button:
                return ("settings.button_other", Values(("code", Number(binding.Code))));
            case BindingKind.Stick when binding.Code <= (int)JoyAxis.TriggerRight
                && (binding.Direction > 0 || binding.Code <= (int)JoyAxis.RightY):
                return ($"settings.stick_{Number(binding.Code)}_{(binding.Direction < 0 ? "neg" : "pos")}", Values());
            default:
                return ("settings.stick_other", Values(("code", Number(binding.Code)), ("sign", binding.Direction < 0 ? "-" : "+")));
        }
    }

    private void BuildRows()
    {
        int column = 0;
        int top = FirstLineTop;
        int line = this.ui.Theme.BodySize + LineGap;
        SettingsGroup? group = null;

        foreach (SettingsRow row in SettingsMenu.Rows)
        {
            if (row.Group != group)
            {
                if (row.Group == FirstRightGroup)
                {
                    column = 1;
                    top = FirstLineTop;
                }
                else if (group is not null)
                {
                    top += line / 2;
                }

                group = row.Group;
                var heading = new Label { Position = new Vector2(ColumnLefts[column], top) };
                this.ui.Text.Put(heading, Id(GroupIdOf(row.Group)));
                heading.AddThemeColorOverride("font_color", this.dimColor);
                this.layer.AddChild(heading);
                top += line;
            }

            this.rows.Add(this.BuildRow(row, ColumnLefts[column] + this.ui.Theme.BodySize, top, line));
            top += line;
        }
    }

    private RowNodes BuildRow(SettingsRow row, int left, int top, int line)
    {
        var name = new Label { Position = new Vector2(left, top), Size = new Vector2(NameWidth, line) };
        this.ui.Text.Put(name, Id(NameIdOf(row)));
        this.layer.AddChild(name);

        if (row.Item != SettingsItem.Remap)
        {
            var value = new Label { Position = new Vector2(left + NameWidth, top), Size = new Vector2(ValueWidth, line) };
            this.layer.AddChild(value);
            return new RowNodes(row, name, value, null, null);
        }

        name.Size = new Vector2(ActionWidth, line);
        var key = new Label { Position = new Vector2(left + ActionWidth, top), Size = new Vector2(SlotWidth, line) };
        var pad = new Label { Position = new Vector2(left + ActionWidth + SlotWidth, top), Size = new Vector2(PadWidth, line) };
        this.layer.AddChild(key);
        this.layer.AddChild(pad);
        return new RowNodes(row, name, null, key, pad);
    }

    private SettingsOutcome ReadEvent(InputEvent signal, ScreenFit fit, string menuAction)
    {
        if (this.Menu.Capturing)
        {
            if (GameInputMap.BindingOf(signal, this.Menu.Settings.Controls.DeadZone) is InputBinding pressed)
            {
                this.Menu.Capture(pressed);
            }

            return SettingsOutcome.Stay;
        }

        if (signal is InputEventMouse mouse)
        {
            this.ReadMouse(mouse, fit);
            return SettingsOutcome.Stay;
        }

        if (signal.IsActionPressed("ui_up"))
        {
            this.Menu.Move(-1);
        }
        else if (signal.IsActionPressed("ui_down"))
        {
            this.Menu.Move(1);
        }
        else if (signal.IsActionPressed("ui_left"))
        {
            this.Menu.Change(-1);
        }
        else if (signal.IsActionPressed("ui_right"))
        {
            this.Menu.Change(1);
        }
        else if (signal.IsActionPressed("ui_accept"))
        {
            this.Menu.Choose();
        }
        else if (signal.IsActionPressed("ui_cancel") || signal.IsActionPressed(menuAction))
        {
            return this.Menu.CanClose ? SettingsOutcome.Close : SettingsOutcome.Stay;
        }

        return SettingsOutcome.Stay;
    }

    private void ReadMouse(InputEventMouse mouse, ScreenFit fit)
    {
        if (fit.ToFrame((int)mouse.Position.X, (int)mouse.Position.Y) is not (int X, int Y) point)
        {
            return;
        }

        var place = new Vector2(point.X, point.Y);
        for (int index = 0; index < this.rows.Count; index += 1)
        {
            RowNodes nodes = this.rows[index];
            if (!nodes.Covers(place))
            {
                continue;
            }

            BindingSlot slot = nodes.Pad is Label pad && place.X >= pad.Position.X ? BindingSlot.Gamepad : BindingSlot.Keyboard;
            this.Menu.Point(index, slot);
            if (mouse is InputEventMouseButton click && click.Pressed && click.ButtonIndex == MouseButton.Left)
            {
                this.Menu.Choose();
            }

            return;
        }
    }

    private (string Id, Dictionary<string, string> Values) ValueOf(SettingsItem item, GameSettings settings) => item switch
    {
        SettingsItem.Window => (settings.Display.Window == WindowSetting.Borderless ? "settings.window_borderless" : "settings.window_window", Values()),
        SettingsItem.Fit => (settings.Display.Fit == FitSetting.Fill ? "settings.fit_fill" : "settings.fit_whole", Values()),
        SettingsItem.Body => settings.Display.Body switch
        {
            BodySetting.Auto => ("settings.body_auto", Values(("size", Number(this.autoBody)))),
            BodySetting.Small => ("settings.body_small", Values()),
            _ => ("settings.body_large", Values()),
        },
        SettingsItem.Master => ("settings.number", Values(("value", Number(settings.Audio.Master)))),
        SettingsItem.Music => ("settings.number", Values(("value", Number(settings.Audio.Music)))),
        SettingsItem.Effects => ("settings.number", Values(("value", Number(settings.Audio.Effects)))),
        SettingsItem.Ambience => ("settings.number", Values(("value", Number(settings.Audio.Ambience)))),
        SettingsItem.MuteInBackground => (OnOff(settings.Audio.MuteInBackground), Values()),
        SettingsItem.Mono => (OnOff(settings.Audio.Mono), Values()),
        SettingsItem.DeadZone => ("settings.dead_zone_value", Values(("value", Number(settings.Controls.DeadZone)))),
        SettingsItem.Vibration => (OnOff(settings.Controls.Vibration), Values()),
        SettingsItem.Messages => (SpeedId((int)settings.Battle.Messages), Values()),
        SettingsItem.RememberCursor => (OnOff(settings.Battle.RememberCursor), Values()),
        SettingsItem.EffectLevel => (settings.Access.Effects switch
        {
            EffectLevel.Full => "settings.level_full",
            EffectLevel.Reduced => "settings.level_reduced",
            _ => "settings.level_off",
        }, Values()),
        SettingsItem.TextSpeed => (SpeedId((int)settings.Access.Text), Values()),
        _ => throw new ArgumentOutOfRangeException(nameof(item), item, "The row holds no value to show (T-2)."),
    };

    private void ShowSlot(Label label, ControlBindings bindings, string action, BindingSlot slot, bool chosenRow)
    {
        bool chosen = chosenRow && this.Menu.Slot == slot;
        if (chosen && this.Menu.Capturing)
        {
            this.ui.Text.Put(label, Id("settings.capture"));
        }
        else
        {
            (string id, Dictionary<string, string> values) = BindingText(SettingsMenu.BindingIn(bindings, action, slot));
            this.ui.Text.Put(label, Id(id), values);
        }

        this.Paint(label, chosen);

        // Each cell whose binding sits in a conflict takes the warning color, so each conflict
        // shows on the grid, and the line names one of them (D-862, D-1119).
        if (!chosen && SettingsMenu.InConflict(bindings, action, slot))
        {
            label.AddThemeColorOverride("font_color", this.warningColor);
        }
    }

    private void ShowHelp()
    {
        if (this.Menu.ConflictToName() is not ShownConflict shown)
        {
            this.ui.Text.Put(this.help, Id("settings.help"));
            this.Paint(this.help, false);
            return;
        }

        BindingConflict named = shown.Conflict;
        (string inputId, Dictionary<string, string> inputValues) = BindingText(named.Binding);
        ContentId input = Id(inputId);
        string inputText = TextHelper.Fill(this.strings.Text(input), input, inputValues);
        (string Key, string Value)[] pairs =
        [
            ("input", inputText),
            ("first", this.strings.Text(Id($"settings.action_{named.Actions[0]}"))),
            ("second", this.strings.Text(Id($"settings.action_{named.Actions[1]}"))),
        ];

        // With more than one conflict, the line gives the place of this one, such as 1 of 2 (D-1119).
        if (shown.Count == 1)
        {
            this.ui.Text.Put(this.help, Id("settings.conflict"), Values(pairs));
        }
        else
        {
            this.ui.Text.Put(
                this.help,
                Id("settings.conflict_of"),
                Values([.. pairs, ("place", Number(shown.Place)), ("count", Number(shown.Count))]));
        }

        this.help.AddThemeColorOverride("font_color", this.warningColor);
    }

    private void Paint(Label label, bool chosen)
    {
        if (chosen)
        {
            label.AddThemeColorOverride("font_color", this.chosenColor);
        }
        else
        {
            label.RemoveThemeColorOverride("font_color");
        }
    }

    /// <summary>The labels of one row.</summary>
    private sealed record RowNodes(SettingsRow Row, Label Name, Label? Value, Label? Key, Label? Pad)
    {
        /// <summary>Tells whether a frame point falls on the line of the row.</summary>
        public bool Covers(Vector2 place)
        {
            Label last = this.Pad ?? this.Value ?? this.Name;
            float right = last.Position.X + last.Size.X;
            return place.Y >= this.Name.Position.Y
                && place.Y < this.Name.Position.Y + this.Name.Size.Y
                && place.X >= this.Name.Position.X
                && place.X < right;
        }
    }
}
