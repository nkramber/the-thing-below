using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The party window: each character with its level and its row, the reserve under the party when
/// one exists, the actions on a character, and the cursor. A choice moves a character to the
/// other row, or swaps it with a reserve character (D-377, D-558, D-1134, D-1144).
/// </summary>
/// <remarks>
/// The window makes the row intent or the swap intent and never changes the run itself. The
/// change comes on the next tick, and the window shows it on the frame after (D-493, T-7). A
/// downed reserve line and a swap with no reserve character to take in show dim (D-1135).
/// </remarks>
public sealed class PartyView : IMenuView
{
    /// <summary>The share of the inner width of the window that the name takes, in hundredths.</summary>
    private const int NameShare = 40;

    /// <summary>The share of the inner width of the window that the level takes, in hundredths.</summary>
    private const int LevelShare = 25;

    /// <summary>
    /// The lines of the window that are not reserve lines: the three party lines, a gap and the
    /// heading of the reserve, a gap and the two actions, and the line of help.
    /// </summary>
    private const int FixedLines = BattleFixture.MostCharacters + 2 + 3 + 1;

    private readonly UiBase ui;
    private readonly Control layer;
    private readonly Label help;
    private readonly List<Label> names = [];
    private readonly List<Label> levels = [];
    private readonly List<Label> rows = [];
    private readonly List<Label> reserveNames = [];
    private readonly List<Label> reserveLevels = [];
    private readonly List<Label> reserveRows = [];
    private readonly List<Label> actions = [];
    private readonly Color chosenColor;
    private readonly Color dimColor;
    private Intent? made;

    /// <summary>Builds the party window beside the main list, with the cursor on the first character.</summary>
    /// <param name="frame">The frame, whose UI layer takes the window.</param>
    /// <param name="ui">The atlas, the theme, and the text helper.</param>
    /// <param name="list">The cursor, which reads the run on each frame and which the window keeps when the screen builds again.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The reserve holds more characters than the window holds lines at this body size (T-2).</exception>
    public PartyView(FrameRoot frame, UiBase ui, PartyList list)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(ui);
        ArgumentNullException.ThrowIfNull(list);

        int body = ui.Theme.BodySize;
        int room = MostReserveLines(body, ui.Theme.TitleSize);
        if (list.Reserve.Count > room)
        {
            throw new ArgumentException(
                $"The reserve holds {list.Reserve.Count} characters, and the party window holds {room} reserve lines at a body of {body} (D-707, T-2).", nameof(list));
        }

        this.ui = ui;
        this.List = list;
        this.chosenColor = ui.Theme.ColorOf("text_chosen");
        this.dimColor = ui.Theme.ColorOf("text_dim");
        this.layer = MenuNodes.Layer(frame, ui);

        FrameBox box = MenuLayout.TaskBox();
        MenuNodes.Panel(this.layer, box);
        MenuNodes.Title(this.layer, ui, box, Id("menu.party"));

        int line = MenuLayout.LineOf(body);
        int left = box.X + MenuLayout.Pad;
        int inner = box.Width - (MenuLayout.Pad * 2);
        int top = MenuLayout.FirstLineTop(body, ui.Theme.TitleSize);
        for (int slot = 0; slot < list.Members.Count; slot += 1)
        {
            this.AddCharacterLine(this.names, this.levels, this.rows, left, top + (slot * line), inner, line);
        }

        // The reserve stands under the full party, with a gap and its heading (D-1136, D-1144).
        if (list.HasReserve)
        {
            int heading = top + ((BattleFixture.MostCharacters + 1) * line);
            Label caption = MenuNodes.Line(this.layer, left, heading, inner, line);
            ui.Text.Put(caption, Id("menu.reserve"));
            MenuNodes.Paint(caption, this.dimColor);
            for (int index = 0; index < list.Reserve.Count; index += 1)
            {
                this.AddCharacterLine(this.reserveNames, this.reserveLevels, this.reserveRows, left, heading + ((index + 1) * line), inner, line);
            }

            int first = heading + ((list.Reserve.Count + 2) * line);
            for (int index = 0; index < PartyList.Actions.Count; index += 1)
            {
                Label action = MenuNodes.Line(this.layer, left, first + (index * line), inner, line);
                ui.Text.Put(action, ActionIdOf(PartyList.Actions[index]));
                this.actions.Add(action);
            }
        }

        this.help = MenuNodes.Line(this.layer, left, box.Y + box.Height - MenuLayout.Pad - line, inner, line);
        MenuNodes.Paint(this.help, this.dimColor);

        this.Show();
    }

    /// <summary>The cursor over the characters, the actions, and the reserve.</summary>
    public PartyList List { get; }

    /// <summary>Gives the count of reserve lines that the window holds at a body size (D-707).</summary>
    /// <param name="body">The body size, in frame pixels.</param>
    /// <param name="titleSize">The title size, in frame pixels.</param>
    /// <returns>The count of whole lines.</returns>
    public static int MostReserveLines(int body, int titleSize) => MenuLayout.LogLines(body, titleSize) - FixedLines;

    /// <summary>Gives the string id of the name of one row (G-7).</summary>
    /// <param name="row">The row.</param>
    /// <returns>The id.</returns>
    public static ContentId RowIdOf(BattleRow row) => Id(row == BattleRow.Front ? "menu.row_front" : "menu.row_back");

    /// <summary>Gives the string id of the label of one action on a character (G-7).</summary>
    /// <param name="action">The action.</param>
    /// <returns>The id, such as `menu.swap`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no action (T-2).</exception>
    public static ContentId ActionIdOf(PartyAction action) => Id(action switch
    {
        PartyAction.Row => "menu.row",
        PartyAction.Swap => "menu.swap",
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "The party window holds no such action (T-2)."),
    });

    /// <summary>Gives the string id of the line of help of the stage of a cursor (G-7).</summary>
    /// <param name="list">The cursor.</param>
    /// <returns>The id: the row alone with no reserve, the row or the swap with one, and the pick of the reserve on a swap.</returns>
    /// <exception cref="ArgumentNullException">The cursor is null (T-2).</exception>
    public static ContentId HelpIdOf(PartyList list)
    {
        ArgumentNullException.ThrowIfNull(list);

        if (list.Stage == PartyStage.Incoming)
        {
            return Id("menu.swap_help");
        }

        return Id(list.HasReserve ? "menu.party_reserve_help" : "menu.party_help");
    }

    /// <summary>Takes the intent of the last whole choice, once.</summary>
    /// <returns>The intent, or no value when the last read made none.</returns>
    public Intent? TakeIntent()
    {
        Intent? taken = this.made;
        this.made = null;
        return taken;
    }

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
        PartyList list = this.List;
        for (int slot = 0; slot < this.names.Count; slot += 1)
        {
            bool marked = list.Stage == PartyStage.Browse ? slot == list.Cursor : slot == list.Chosen;
            this.ShowCharacter(list.Members[slot], slot, this.names, this.levels, this.rows, marked ? this.chosenColor : null);
        }

        for (int index = 0; index < this.reserveNames.Count; index += 1)
        {
            Color? color = null;
            if (list.Reserve[index].Down)
            {
                color = this.dimColor;
            }

            if (list.Stage == PartyStage.Incoming && index == list.Cursor)
            {
                color = this.chosenColor;
            }

            this.ShowCharacter(list.Reserve[index], index, this.reserveNames, this.reserveLevels, this.reserveRows, color);
        }

        // The actions show while the cursor stands in them or in the reserve of a swap.
        for (int index = 0; index < this.actions.Count; index += 1)
        {
            this.actions[index].Visible = list.Stage != PartyStage.Browse;
            Color? color = null;
            if (PartyList.Actions[index] == PartyAction.Swap && !list.AllowsSwap())
            {
                color = this.dimColor;
            }

            bool onAction = list.Stage == PartyStage.Action && index == list.Cursor;
            bool swapping = list.Stage == PartyStage.Incoming && PartyList.Actions[index] == PartyAction.Swap;
            if (onAction || swapping)
            {
                color = this.chosenColor;
            }

            MenuNodes.Paint(this.actions[index], color);
        }

        this.ui.Text.Put(this.help, HelpIdOf(list));
    }

    /// <inheritdoc/>
    public void Free() => this.layer.QueueFree();

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

    /// <summary>Adds the three labels of one character line: the name, the level, and the row.</summary>
    private void AddCharacterLine(List<Label> lineNames, List<Label> lineLevels, List<Label> lineRows, int left, int row, int inner, int line)
    {
        int nameWidth = inner * NameShare / 100;
        int levelWidth = inner * LevelShare / 100;
        lineNames.Add(MenuNodes.Line(this.layer, left, row, nameWidth, line));
        lineLevels.Add(MenuNodes.Line(this.layer, left + nameWidth, row, levelWidth, line));
        lineRows.Add(MenuNodes.Line(this.layer, left + nameWidth + levelWidth, row, inner - nameWidth - levelWidth, line));
    }

    /// <summary>Puts the name, the level, and the row of one character on its line, in one color.</summary>
    private void ShowCharacter(PartyMember member, int index, List<Label> lineNames, List<Label> lineLevels, List<Label> lineRows, Color? color)
    {
        this.ui.Text.Put(lineNames[index], BattleMessages.NameIdOf(member.Record.Id));
        this.ui.Text.Put(lineLevels[index], Id("menu.level"), Values(("level", Number(member.Level))));
        this.ui.Text.Put(lineRows[index], RowIdOf(member.Row));
        MenuNodes.Paint(lineNames[index], color);
        MenuNodes.Paint(lineLevels[index], color);
        MenuNodes.Paint(lineRows[index], color);
    }

    private ViewOutcome ReadEvent(InputEvent signal, ScreenFit fit)
    {
        if (signal is InputEventMouse mouse)
        {
            if (this.EntryUnder(mouse, fit) is not int entry)
            {
                return ViewOutcome.Stay;
            }

            this.List.Point(entry);
            return MenuNodes.IsClick(mouse) ? this.Confirm() : ViewOutcome.Stay;
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
            return this.Confirm();
        }
        else if (signal.IsActionPressed("ui_cancel"))
        {
            return this.List.Cancel() ? ViewOutcome.Back : ViewOutcome.Stay;
        }

        return ViewOutcome.Stay;
    }

    /// <summary>Gives the entry of the list of the stage under the mouse pointer, or no value (D-872).</summary>
    /// <remarks>The name, the level, and the row each take the pointer, so the whole line of a character does.</remarks>
    private int? EntryUnder(InputEventMouse mouse, ScreenFit fit) => this.List.Stage switch
    {
        PartyStage.Browse => MenuNodes.LineUnder(this.names, mouse, fit) ?? MenuNodes.LineUnder(this.levels, mouse, fit) ?? MenuNodes.LineUnder(this.rows, mouse, fit),
        PartyStage.Action => MenuNodes.LineUnder(this.actions, mouse, fit),
        _ => MenuNodes.LineUnder(this.reserveNames, mouse, fit) ?? MenuNodes.LineUnder(this.reserveLevels, mouse, fit) ?? MenuNodes.LineUnder(this.reserveRows, mouse, fit),
    };

    private ViewOutcome Confirm()
    {
        this.made = this.List.Confirm();
        return this.made is null ? ViewOutcome.Stay : ViewOutcome.Chose;
    }
}
