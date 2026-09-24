using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The item window: the items of the pack, each with the owned count against its stack limit,
/// and the target of a use outside a fight (D-382, D-1039, D-1046, D-1049).
/// </summary>
/// <remarks>
/// The window makes one intent for each whole choice and never changes the run itself (D-493,
/// T-7). An item that the rules refuse on every character shows dim, and a confirm on it does
/// nothing, so a use that changes nothing spends no item (D-1049). The last line holds the line
/// of the item under the cursor.
/// </remarks>
public sealed class ItemsView : IMenuView
{
    /// <summary>The lines above the list: the caption of the list.</summary>
    private const int HeadLines = 1;

    /// <summary>The share of the inner width of the window that the left column of the list takes, in hundredths.</summary>
    private const int LeftShare = 55;

    private readonly UiBase ui;
    private readonly RunState state;
    private readonly Control layer;
    private readonly Label caption;
    private readonly Label help;
    private readonly List<Label> lefts = [];
    private readonly List<Label> rights = [];
    private readonly Color chosenColor;
    private readonly Color dimColor;
    private Intent? made;
    private int top;

    /// <summary>Builds the item window beside the main list.</summary>
    /// <param name="frame">The frame, whose UI layer takes the window.</param>
    /// <param name="ui">The atlas, the theme, and the text helper.</param>
    /// <param name="state">The state of the run, which the window reads on each frame and never changes.</param>
    /// <param name="cursor">The cursor, which the window keeps when the screen builds again.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public ItemsView(FrameRoot frame, UiBase ui, RunState state, ItemCursor cursor)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(ui);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(cursor);

        this.ui = ui;
        this.state = state;
        this.Cursor = cursor;
        this.chosenColor = ui.Theme.ColorOf("text_chosen");
        this.dimColor = ui.Theme.ColorOf("text_dim");
        this.layer = MenuNodes.Layer(frame, ui);

        int body = ui.Theme.BodySize;
        FrameBox box = MenuLayout.TaskBox();
        MenuNodes.Panel(this.layer, box);
        MenuNodes.Title(this.layer, ui, box, Id("menu.items"));
        int line = MenuLayout.LineOf(body);
        int left = box.X + MenuLayout.Pad;
        int inner = box.Width - (MenuLayout.Pad * 2);
        int leftWidth = inner * LeftShare / 100;
        int first = MenuLayout.FirstLineTop(body, ui.Theme.TitleSize);
        this.caption = MenuNodes.Line(this.layer, left, first, inner, line);
        MenuNodes.Paint(this.caption, this.dimColor);

        // The list takes each line between the caption and the last line of the window.
        int listLines = MenuLayout.LogLines(body, ui.Theme.TitleSize) - HeadLines - 1;
        for (int index = 0; index < listLines; index += 1)
        {
            int row = first + (line * (HeadLines + index));
            this.lefts.Add(MenuNodes.Line(this.layer, left, row, leftWidth, line));
            this.rights.Add(MenuNodes.Line(this.layer, left + leftWidth, row, inner - leftWidth, line));
        }

        this.help = MenuNodes.Line(this.layer, left, box.Y + box.Height - MenuLayout.Pad - line, inner, line);
        MenuNodes.Paint(this.help, this.dimColor);
        this.Show();
    }

    /// <summary>The cursor of the window.</summary>
    public ItemCursor Cursor { get; }

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
        this.Cursor.Settle();
        this.ui.Text.Put(this.caption, Id(this.Cursor.Stage == ItemStage.Item ? "menu.item_list" : "menu.item_target"));

        List<Entry> entries = this.Entries();
        int shown = this.lefts.Count;
        this.top = Math.Clamp(this.top, Math.Max(0, this.Cursor.Cursor - shown + 1), this.Cursor.Cursor);
        for (int index = 0; index < shown; index += 1)
        {
            int at = this.top + index;
            bool filled = at < entries.Count;
            this.lefts[index].Visible = filled;
            this.rights[index].Visible = filled;
            if (!filled)
            {
                continue;
            }

            Entry entry = entries[at];
            this.ui.Text.Put(this.lefts[index], entry.Left);
            this.ui.Text.Put(this.rights[index], entry.Right, entry.RightValues);
            Color? color = at == this.Cursor.Cursor ? this.chosenColor : entry.Allowed ? null : this.dimColor;
            MenuNodes.Paint(this.lefts[index], color);
            MenuNodes.Paint(this.rights[index], color);
        }

        this.ui.Text.Put(this.help, this.HelpId());
    }

    /// <inheritdoc/>
    public void Free() => this.layer.QueueFree();

    private static ContentId Id(string value) => ContentId.Parse(value, StringTable.Path, nameof(ItemsView));

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

    /// <summary>Gives the entries of the list of the stage: each item with its owned count and limit, or each character with the value that the item restores.</summary>
    private List<Entry> Entries()
    {
        var entries = new List<Entry>();
        if (this.Cursor.Stage == ItemStage.Item)
        {
            IReadOnlyList<PackValues> items = this.Cursor.Items;
            for (int index = 0; index < items.Count; index += 1)
            {
                ItemRecord record = this.state.BattleContent.Item(items[index].Id);
                entries.Add(new Entry(
                    BattleMessages.NameIdOf(record.Id),
                    Id("menu.item_count"),
                    Values(("count", Number(this.state.Characters.OwnedCount(record.Id))), ("limit", Number(record.Limit))),
                    this.Cursor.AllowsItem(index)));
            }

            return entries;
        }

        // A restore shows the MP of each character, and every other item the health (D-1046).
        bool restore = this.state.BattleContent.Item(this.Cursor.Chosen!) is RestoreItem;
        IReadOnlyList<PartyMember> members = this.state.Characters.Members;
        for (int slot = 0; slot < members.Count; slot += 1)
        {
            PartyMember member = members[slot];
            entries.Add(restore
                ? new Entry(BattleMessages.NameIdOf(member.Record.Id), Id("battle.mp"), Values(("mp", Number(member.Mp)), ("full", Number(member.Stats.Mp))), this.Cursor.AllowsTarget(slot))
                : new Entry(BattleMessages.NameIdOf(member.Record.Id), Id("battle.health"), Values(("health", Number(member.Health)), ("full", Number(member.Stats.Health))), this.Cursor.AllowsTarget(slot)));
        }

        return entries;
    }

    /// <summary>Gives the last line: the line of the item under the cursor or of the chosen item, or a line for an empty pack.</summary>
    private ContentId HelpId()
    {
        if (this.Cursor.Stage == ItemStage.Target)
        {
            return this.Cursor.Chosen!;
        }

        IReadOnlyList<PackValues> items = this.Cursor.Items;
        return items.Count == 0 ? Id("menu.item_none") : items[this.Cursor.Cursor].Id;
    }

    private ViewOutcome ReadEvent(InputEvent signal, ScreenFit fit)
    {
        if (signal is InputEventMouse mouse)
        {
            if ((MenuNodes.LineUnder(this.lefts, mouse, fit) ?? MenuNodes.LineUnder(this.rights, mouse, fit)) is not int line
                || this.top + line >= this.Cursor.Count)
            {
                return ViewOutcome.Stay;
            }

            this.Cursor.Point(this.top + line);
            return MenuNodes.IsClick(mouse) ? this.Confirm() : ViewOutcome.Stay;
        }

        if (signal.IsActionPressed("ui_up"))
        {
            this.Cursor.Move(-1);
        }
        else if (signal.IsActionPressed("ui_down"))
        {
            this.Cursor.Move(1);
        }
        else if (signal.IsActionPressed("ui_accept"))
        {
            return this.Confirm();
        }
        else if (signal.IsActionPressed("ui_cancel"))
        {
            return this.Cursor.Cancel() ? ViewOutcome.Back : ViewOutcome.Stay;
        }

        return ViewOutcome.Stay;
    }

    private ViewOutcome Confirm()
    {
        this.made = this.Cursor.Confirm();
        return this.made is null ? ViewOutcome.Stay : ViewOutcome.Chose;
    }

    /// <summary>One line of the list: the left text, the right text, and whether the rules take the entry now.</summary>
    private sealed record Entry(ContentId Left, ContentId Right, IReadOnlyDictionary<string, string> RightValues, bool Allowed);
}
