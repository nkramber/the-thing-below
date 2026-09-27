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
/// the key items and the Keyring in lists of their own, and the target of a use outside a fight
/// (D-382, D-1039, D-1046, D-1049, D-1219).
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
    private readonly Label quantity;
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

        // The caption of the column of the owned count and the limit, such as "3/5" (D-1172).
        this.quantity = MenuNodes.Line(this.layer, left + leftWidth, first, inner - leftWidth, line);
        MenuNodes.Paint(this.quantity, this.dimColor);
        this.ui.Text.Put(this.quantity, Id("menu.item_quantity"));

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
        this.ui.Text.Put(this.caption, Id(this.Cursor.Stage switch
        {
            ItemStage.Item => "menu.item_list",
            ItemStage.KeyItems => "menu.key_items",
            ItemStage.Keyring => "menu.keyring",
            _ => "menu.item_target",
        }));
        this.quantity.Visible = this.Cursor.Stage != ItemStage.Target;

        List<Entry> entries = this.Entries();
        int shown = this.lefts.Count;
        this.top = Math.Clamp(this.top, Math.Max(0, this.Cursor.Cursor - shown + 1), this.Cursor.Cursor);
        for (int index = 0; index < shown; index += 1)
        {
            int at = this.top + index;
            bool filled = at < entries.Count;
            this.lefts[index].Visible = filled;
            this.rights[index].Visible = filled && entries[at].Right is not null;
            if (!filled)
            {
                continue;
            }

            Entry entry = entries[at];
            this.ui.Text.Put(this.lefts[index], entry.Left);
            if (entry.Right is ContentId right)
            {
                this.ui.Text.Put(this.rights[index], right, entry.RightValues);
            }

            Color? color = at == this.Cursor.Cursor ? this.chosenColor : entry.Allowed ? null : this.dimColor;
            MenuNodes.Paint(this.lefts[index], color);
            MenuNodes.Paint(this.rights[index], color);
        }

        this.ui.Text.Put(this.help, this.HelpId());
    }

    /// <inheritdoc/>
    public void Free() => this.layer.QueueFree();

    private static ContentId Id(string value) => ContentId.Parse(value, StringTable.Path, nameof(ItemsView));

    private static readonly IReadOnlyDictionary<string, string> NoValues = new Dictionary<string, string>(StringComparer.Ordinal);

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

    /// <summary>
    /// Gives the entries of the list of the stage: each item with its owned count and limit, and
    /// the entry of the key items or of the Keyring, or each character with the value that the
    /// item restores (D-1219).
    /// </summary>
    private List<Entry> Entries()
    {
        var entries = new List<Entry>();
        if (this.Cursor.Stage != ItemStage.Target)
        {
            IReadOnlyList<ItemLine> lines = this.Cursor.Lines;
            for (int index = 0; index < lines.Count; index += 1)
            {
                entries.Add(lines[index] switch
                {
                    { Kind: ItemLineKind.KeyItems } => new Entry(Id("menu.key_items"), null, NoValues, true),
                    { Kind: ItemLineKind.Keyring } => new Entry(Id("menu.keyring"), null, NoValues, true),
                    { Entry: PackValues item } => this.ItemEntry(item, this.Cursor.AllowsItem(index)),
                    _ => throw new InvalidOperationException($"The line {index} of the item window holds no item (T-2)."),
                });
            }

            return entries;
        }

        // A restore shows the AP of each character, and every other item the health (D-1046).
        bool restore = this.state.BattleContent.Item(this.Cursor.Chosen!) is RestoreItem;
        IReadOnlyList<PartyMember> members = this.state.Characters.Members;
        for (int slot = 0; slot < members.Count; slot += 1)
        {
            PartyMember member = members[slot];
            entries.Add(restore
                ? new Entry(BattleMessages.NameIdOf(member.Record.Id), Id("battle.ap"), Values(("ap", Number(member.Ap)), ("full", Number(member.Stats.Ap))), this.Cursor.AllowsTarget(slot))
                : new Entry(BattleMessages.NameIdOf(member.Record.Id), Id("battle.health"), Values(("health", Number(member.Health)), ("full", Number(member.Stats.Health))), this.Cursor.AllowsTarget(slot)));
        }

        return entries;
    }

    /// <summary>One item with its owned count against its stack limit (D-1039).</summary>
    private Entry ItemEntry(PackValues item, bool allowed)
    {
        ItemRecord record = this.state.BattleContent.Item(item.Id);
        return new Entry(
            BattleMessages.NameIdOf(record.Id),
            Id("menu.item_count"),
            Values(("count", Number(this.state.Characters.OwnedCount(record.Id))), ("limit", Number(record.Limit))),
            allowed);
    }

    /// <summary>
    /// Gives the last line: the line of the item under the cursor or of the chosen item, the line
    /// of the entry of the key items or of the Keyring, or a line for an empty pack (D-1219).
    /// </summary>
    private ContentId HelpId()
    {
        if (this.Cursor.Stage == ItemStage.Target)
        {
            return this.Cursor.Chosen!;
        }

        IReadOnlyList<ItemLine> lines = this.Cursor.Lines;
        if (lines.Count == 0)
        {
            return Id("menu.item_none");
        }

        return lines[this.Cursor.Cursor] switch
        {
            { Kind: ItemLineKind.KeyItems } => Id("menu.key_items_help"),
            { Kind: ItemLineKind.Keyring } => Id("menu.keyring_help"),
            { Entry: PackValues item } => item.Id,
            _ => throw new InvalidOperationException($"The line {this.Cursor.Cursor} of the item window holds no item (T-2)."),
        };
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

    /// <summary>One line of the list: the left text, the right text or none, and whether the rules take the entry now.</summary>
    private sealed record Entry(ContentId Left, ContentId? Right, IReadOnlyDictionary<string, string> RightValues, bool Allowed);
}
