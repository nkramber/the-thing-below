using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The party window: each character with its level and its row, and the cursor. A choice moves
/// the character under the cursor to the other row (D-377, D-558).
/// </summary>
/// <remarks>
/// The window makes the row intent and never changes the run itself. The row changes on the
/// next tick, and the window shows it on the frame after (D-493, T-7).
/// </remarks>
public sealed class PartyView : IMenuView
{
    /// <summary>The share of the inner width of the window that the name takes, in hundredths.</summary>
    private const int NameShare = 40;

    /// <summary>The share of the inner width of the window that the level takes, in hundredths.</summary>
    private const int LevelShare = 25;

    private readonly UiBase ui;
    private readonly RunState state;
    private readonly Control layer;
    private readonly List<Label> names = [];
    private readonly List<Label> levels = [];
    private readonly List<Label> rows = [];
    private readonly Color chosenColor;

    /// <summary>Builds the party window beside the main list, with the cursor on the first character.</summary>
    /// <param name="frame">The frame, whose UI layer takes the window.</param>
    /// <param name="ui">The atlas, the theme, and the text helper.</param>
    /// <param name="state">The state of the run, which the window reads on each frame and never changes.</param>
    /// <param name="list">The cursor, which the window keeps when the screen builds again.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The cursor counts another party (T-2).</exception>
    public PartyView(FrameRoot frame, UiBase ui, RunState state, PartyList list)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(ui);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(list);
        if (list.Count != state.Characters.Members.Count)
        {
            throw new ArgumentException(
                $"The cursor counts {list.Count} characters, and the party holds {state.Characters.Members.Count} (T-2).", nameof(list));
        }

        this.ui = ui;
        this.state = state;
        this.List = list;
        this.chosenColor = ui.Theme.ColorOf("text_chosen");
        this.layer = MenuNodes.Layer(frame, ui);

        int body = ui.Theme.BodySize;
        FrameBox box = MenuLayout.TaskBox();
        MenuNodes.Panel(this.layer, box);
        MenuNodes.Title(this.layer, ui, box, Id("menu.party"));

        int line = MenuLayout.LineOf(body);
        int left = box.X + MenuLayout.Pad;
        int inner = box.Width - (MenuLayout.Pad * 2);
        int nameWidth = inner * NameShare / 100;
        int levelWidth = inner * LevelShare / 100;
        int top = MenuLayout.FirstLineTop(body, ui.Theme.TitleSize);
        for (int slot = 0; slot < list.Count; slot += 1)
        {
            int row = top + (slot * line);
            this.names.Add(MenuNodes.Line(this.layer, left, row, nameWidth, line));
            this.levels.Add(MenuNodes.Line(this.layer, left + nameWidth, row, levelWidth, line));
            this.rows.Add(MenuNodes.Line(this.layer, left + nameWidth + levelWidth, row, inner - nameWidth - levelWidth, line));
        }

        Label help = MenuNodes.Line(this.layer, left, box.Y + box.Height - MenuLayout.Pad - line, inner, line);
        ui.Text.Put(help, Id("menu.party_help"));
        MenuNodes.Paint(help, ui.Theme.ColorOf("text_dim"));

        this.Show();
    }

    /// <summary>The cursor over the characters.</summary>
    public PartyList List { get; }

    /// <inheritdoc/>
    public ViewOutcome Read(InputEvent signal, ScreenFit fit)
    {
        ArgumentNullException.ThrowIfNull(signal);
        ArgumentNullException.ThrowIfNull(fit);

        ViewOutcome outcome = this.ReadEvent(signal, fit);
        this.Show();
        return outcome;
    }

    /// <inheritdoc/>
    public void Show()
    {
        IReadOnlyList<PartyMember> members = this.state.Characters.Members;
        for (int slot = 0; slot < this.names.Count; slot += 1)
        {
            PartyMember member = members[slot];
            this.ui.Text.Put(this.names[slot], BattleMessages.NameIdOf(member.Record.Id));
            this.ui.Text.Put(this.levels[slot], Id("menu.level"), Values(("level", Number(member.Level))));
            this.ui.Text.Put(this.rows[slot], RowIdOf(member.Row));

            Color? color = slot == this.List.Cursor ? this.chosenColor : null;
            MenuNodes.Paint(this.names[slot], color);
            MenuNodes.Paint(this.levels[slot], color);
            MenuNodes.Paint(this.rows[slot], color);
        }
    }

    /// <inheritdoc/>
    public void Free() => this.layer.QueueFree();

    /// <summary>Gives the string id of the name of one row (G-7).</summary>
    /// <param name="row">The row.</param>
    /// <returns>The id.</returns>
    public static ContentId RowIdOf(BattleRow row) => Id(row == BattleRow.Front ? "menu.row_front" : "menu.row_back");

    private static ContentId Id(string value) => ContentId.Parse(value, StringTable.Path, nameof(PartyView));

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static Dictionary<string, string> Values(params (string Key, string Value)[] pairs)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach ((string key, string value) in pairs)
        {
            values.Add(key, value);
        }

        return values;
    }

    private ViewOutcome ReadEvent(InputEvent signal, ScreenFit fit)
    {
        if (signal is InputEventMouse mouse)
        {
            // The name, the level, and the row each take the pointer, so the whole line of a character does (D-872).
            if ((MenuNodes.LineUnder(this.names, mouse, fit) ?? MenuNodes.LineUnder(this.levels, mouse, fit) ?? MenuNodes.LineUnder(this.rows, mouse, fit)) is not int slot)
            {
                return ViewOutcome.Stay;
            }

            this.List.Point(slot);
            return MenuNodes.IsClick(mouse) ? ViewOutcome.Chose : ViewOutcome.Stay;
        }

        if (signal.IsActionPressed("ui_up"))
        {
            this.List.Move(-1);
        }
        else if (signal.IsActionPressed("ui_down"))
        {
            this.List.Move(1);
        }
        else if (signal.IsActionPressed("ui_accept"))
        {
            return ViewOutcome.Chose;
        }
        else if (signal.IsActionPressed("ui_cancel"))
        {
            return ViewOutcome.Back;
        }

        return ViewOutcome.Stay;
    }
}
