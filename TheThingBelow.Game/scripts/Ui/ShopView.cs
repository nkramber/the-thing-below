using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Shops;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The shop window: the shop menu with the gold under it, and the list window to its right with
/// the change of each fighter for a piece of gear, the list, the count, and the line of help
/// (D-1149 to D-1160, D-1164).
/// </summary>
/// <remarks>
/// The shop menu stands where the main list stands, and the list window stands where a task window
/// stands, so the shop reads as the menu of the walk (D-211, D-1164). The list window shows after
/// a choice of buy or sell. The window makes one intent for each count that the player confirms,
/// and never changes the run itself. The change lands on the next tick, and the window shows it on
/// the frame after (D-493, T-7). Every label takes its text from the string table through the text
/// helper (G-7, D-499).
/// </remarks>
public sealed class ShopView : IMenuView
{
    /// <summary>The lines above the list: the names of the stats, and one line for each of the three fighters (D-1159).</summary>
    private const int HeadLines = 4;

    /// <summary>The share of the inner width of the list window that the left column of the list takes, in hundredths.</summary>
    private const int LeftShare = 45;

    /// <summary>The string id of the name of each stat that gear changes, in the order of the cells (D-1052, D-1056).</summary>
    private static readonly string[] StatNames = ["battle.stat_atk", "battle.stat_mag", "battle.stat_def", "battle.stat_res", "battle.stat_spd"];

    private readonly UiBase ui;
    private readonly RunState state;
    private readonly Control layer;
    private readonly Control listWindow;
    private readonly List<Label> modeLines = [];
    private readonly GoldPanel gold;
    private readonly Label title;
    private readonly List<Label> statNames = [];
    private readonly List<Label> fighterNames = [];
    private readonly List<List<Label>> fighterCells = [];
    private readonly List<Label> lefts = [];
    private readonly List<Label> rights = [];
    private readonly Label countLine;
    private readonly Label help;
    private readonly Color chosenColor;
    private readonly Color dimColor;
    private readonly Color gainColor;
    private readonly Color lossColor;
    private readonly Color warningColor;
    private Intent? made;
    private int top;

    /// <summary>Builds the shop window, with the cursor on the buy of the shop menu.</summary>
    /// <param name="frame">The frame, whose UI layer takes the window.</param>
    /// <param name="ui">The atlas, the theme, and the text helper.</param>
    /// <param name="state">The state of the run, which the window reads on each frame and never changes.</param>
    /// <param name="cursor">The cursor, which the window keeps when the screen builds again.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public ShopView(FrameRoot frame, UiBase ui, RunState state, ShopCursor cursor)
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
        this.gainColor = ui.Theme.ColorOf("text_gain");
        this.lossColor = ui.Theme.ColorOf("text_loss");
        this.warningColor = ui.Theme.ColorOf("text_warning");
        this.layer = MenuNodes.Layer(frame, ui);

        int body = ui.Theme.BodySize;
        int line = MenuLayout.LineOf(body);
        FrameBox menu = MenuLayout.ShopMenuBox(body);
        MenuNodes.Panel(this.layer, menu);
        for (int index = 0; index < ShopCursor.Modes.Count; index += 1)
        {
            Label label = MenuNodes.Line(this.layer, menu.X + MenuLayout.Pad, menu.Y + MenuLayout.Pad + (index * line), menu.Width - (MenuLayout.Pad * 2), line);
            ui.Text.Put(label, ModeIdOf(ShopCursor.Modes[index]));
            this.modeLines.Add(label);
        }

        this.gold = new GoldPanel(this.layer, ui, state, menu);

        // The list window hangs on a layer of its own, which shows after a choice of buy or sell.
        this.listWindow = new Control
        {
            Position = Vector2.Zero,
            Size = new Vector2(ScreenFit.FrameWidth, ScreenFit.FrameHeight),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        this.layer.AddChild(this.listWindow);
        FrameBox box = MenuLayout.TaskBox();
        MenuNodes.Panel(this.listWindow, box);
        this.title = new Label
        {
            Position = new Vector2(box.X + MenuLayout.Pad, box.Y + MenuLayout.Pad),
            ThemeTypeVariation = UiTheme.TitleVariation,
        };
        this.listWindow.AddChild(this.title);

        int left = box.X + MenuLayout.Pad;
        int inner = box.Width - (MenuLayout.Pad * 2);
        int first = MenuLayout.FirstLineTop(body, ui.Theme.TitleSize);
        int cell = CellWidth();
        int nameWidth = inner - (cell * StatNames.Length);
        for (int stat = 0; stat < StatNames.Length; stat += 1)
        {
            Label name = MenuNodes.Line(this.listWindow, left + nameWidth + (cell * stat), first, cell, line);
            ui.Text.Put(name, Id(StatNames[stat]));
            MenuNodes.Paint(name, this.dimColor);
            this.statNames.Add(name);
        }

        for (int fighter = 0; fighter < BattleFixture.MostCharacters; fighter += 1)
        {
            int row = first + (line * (fighter + 1));
            this.fighterNames.Add(MenuNodes.Line(this.listWindow, left, row, nameWidth, line));
            List<Label> cells = [];
            for (int stat = 0; stat < StatNames.Length; stat += 1)
            {
                cells.Add(MenuNodes.Line(this.listWindow, left + nameWidth + (cell * stat), row, cell, line));
            }

            this.fighterCells.Add(cells);
        }

        // The list takes each line between the fighters and the count line above the line of help.
        int leftWidth = inner * LeftShare / 100;
        int listLines = MenuLayout.LogLines(body, ui.Theme.TitleSize) - HeadLines - 2;
        for (int index = 0; index < listLines; index += 1)
        {
            int row = first + (line * (HeadLines + index));
            this.lefts.Add(MenuNodes.Line(this.listWindow, left, row, leftWidth, line));
            this.rights.Add(MenuNodes.Line(this.listWindow, left + leftWidth, row, inner - leftWidth, line));
        }

        int bottom = box.Y + box.Height - MenuLayout.Pad - line;
        this.countLine = MenuNodes.Line(this.listWindow, left, bottom - line, inner, line);
        MenuNodes.Paint(this.countLine, this.chosenColor);
        this.help = MenuNodes.Line(this.listWindow, left, bottom, inner, line);
        this.Show();
    }

    /// <summary>The cursor of the window.</summary>
    public ShopCursor Cursor { get; }

    /// <summary>Gives the count of characters that one cell of a line of stats holds at a body size (D-1159).</summary>
    /// <param name="body">The body size, in frame pixels (D-707).</param>
    /// <returns>The count of whole characters.</returns>
    public static int StatCellCharacters(int body) => UiMetrics.CharactersAcross(body, CellWidth());

    /// <summary>Gives the string id of the label of one choice of the shop menu (G-7).</summary>
    /// <param name="mode">The choice.</param>
    /// <returns>The id, such as `menu.shop_buy`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no choice (T-2).</exception>
    public static ContentId ModeIdOf(ShopMode mode) => Id(mode switch
    {
        ShopMode.Buy => "menu.shop_buy",
        ShopMode.Sell => "menu.shop_sell",
        ShopMode.Leave => "menu.leave",
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "The shop menu holds no such choice (T-2)."),
    });

    /// <summary>Gives the string id of the line of help of one choice of the shop menu (G-7).</summary>
    /// <param name="mode">The choice.</param>
    /// <returns>The id, such as `menu.shop_buy_help`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no choice (T-2).</exception>
    public static ContentId ModeHelpOf(ShopMode mode) => Id(mode switch
    {
        ShopMode.Buy => "menu.shop_buy_help",
        ShopMode.Sell => "menu.shop_sell_help",
        ShopMode.Leave => "menu.leave_help",
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "The shop menu holds no such choice (T-2)."),
    });

    /// <summary>Takes the intent of the last whole choice, once.</summary>
    /// <returns>The buy intent or the sale intent, or no value when the last read made none.</returns>
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
        ShopCursor cursor = this.Cursor;
        for (int index = 0; index < this.modeLines.Count; index += 1)
        {
            MenuNodes.Paint(this.modeLines[index], index == cursor.ModeCursor ? this.chosenColor : null);
        }

        this.gold.Show();
        this.listWindow.Visible = cursor.Stage != ShopStage.Mode;
        if (cursor.Stage == ShopStage.Mode)
        {
            return;
        }

        this.ui.Text.Put(this.title, ModeIdOf(cursor.Mode));
        this.ShowFighters();
        this.ShowList();
        this.ShowCount();
        ContentId? refusal = cursor.Refusal();
        this.ui.Text.Put(this.help, refusal ?? cursor.Thing ?? ModeHelpOf(cursor.Mode));
        MenuNodes.Paint(this.help, refusal is null ? this.dimColor : this.warningColor);
    }

    /// <inheritdoc/>
    public void Free() => this.layer.QueueFree();

    private static int CellWidth() => (MenuLayout.TaskBox().Width - (MenuLayout.Pad * 2)) / (StatNames.Length + 1);

    private static ContentId Id(string value) => ContentId.Parse(value, StringTable.Path, nameof(ShopView));

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static int[] ValuesOf(StatRow stats) => [stats.Attack, stats.Magic, stats.Defense, stats.Resistance, stats.Speed];

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
    /// Shows the change of each fighter for the piece of gear under the cursor of the buy list: the
    /// whole stat, green with the gain and red with the loss (D-1159). Another thing shows no line.
    /// </summary>
    private void ShowFighters()
    {
        GearRecord? piece = this.Cursor.Mode == ShopMode.Buy && this.Cursor.Thing is ContentId thing && this.state.BattleContent.Gear.Holds(thing)
            ? this.state.BattleContent.Gear.Piece(thing)
            : null;
        foreach (Label name in this.statNames)
        {
            name.Visible = piece is not null;
        }

        IReadOnlyList<PartyMember> members = this.state.Characters.Members;
        for (int fighter = 0; fighter < this.fighterNames.Count; fighter += 1)
        {
            bool shown = piece is not null && fighter < members.Count;
            this.fighterNames[fighter].Visible = shown;
            foreach (Label cell in this.fighterCells[fighter])
            {
                cell.Visible = shown;
            }

            if (!shown)
            {
                continue;
            }

            PartyMember member = members[fighter];
            this.ui.Text.Put(this.fighterNames[fighter], BattleMessages.NameIdOf(member.Record.Id));
            int[] worn = ValuesOf(member.StatsWith(this.state.BattleContent.Gear));
            int[] trial = ValuesOf(this.Cursor.TrialOf(member, piece!));
            for (int stat = 0; stat < StatNames.Length; stat += 1)
            {
                Label cell = this.fighterCells[fighter][stat];
                IReadOnlyDictionary<string, string> values = trial[stat] == worn[stat]
                    ? Values(("value", Number(trial[stat])))
                    : Values(("change", Number(Math.Abs(trial[stat] - worn[stat]))), ("value", Number(trial[stat])));
                this.ui.Text.Put(cell, GearView.TrialIdOf(worn[stat], trial[stat]), values);
                MenuNodes.Paint(cell, trial[stat] > worn[stat] ? this.gainColor : trial[stat] < worn[stat] ? this.lossColor : this.dimColor);
            }
        }
    }

    /// <summary>Shows the lines of the list of the mode, from the line at the top of the scroll.</summary>
    private void ShowList()
    {
        List<Entry> entries = this.Entries();
        int shown = this.lefts.Count;
        int cursor = Math.Min(this.Cursor.Cursor, Math.Max(0, entries.Count - 1));
        this.top = Math.Clamp(this.top, Math.Max(0, cursor - shown + 1), cursor);
        for (int index = 0; index < shown; index += 1)
        {
            int at = this.top + index;
            bool empty = entries.Count == 0 && index == 0;
            bool filled = at < entries.Count;
            this.lefts[index].Visible = filled || empty;
            this.rights[index].Visible = filled;
            if (empty)
            {
                this.ui.Text.Put(this.lefts[index], Id("menu.shop_empty"));
                MenuNodes.Paint(this.lefts[index], this.dimColor);
                continue;
            }

            if (!filled)
            {
                continue;
            }

            Entry entry = entries[at];
            this.ui.Text.Put(this.lefts[index], BattleMessages.NameIdOf(entry.Thing));
            this.ui.Text.Put(this.rights[index], entry.Right, entry.RightValues);
            Color? color = at == cursor ? this.chosenColor : entry.Allowed ? null : this.dimColor;
            MenuNodes.Paint(this.lefts[index], color);
            MenuNodes.Paint(this.rights[index], color);
        }
    }

    /// <summary>Shows the count and its total in the count stage, and no line in the list stage (D-1158).</summary>
    private void ShowCount()
    {
        ShopCursor cursor = this.Cursor;
        this.countLine.Visible = cursor.Stage == ShopStage.Count && cursor.Thing is not null;
        if (!this.countLine.Visible || cursor.Thing is not ContentId thing)
        {
            return;
        }

        int each = cursor.Mode == ShopMode.Sell
            ? ShopRules.SaleOf(this.state, cursor.Shop, thing)
            : (cursor.Shop.EntryOf(thing) ?? throw new InvalidOperationException($"The shop '{cursor.Shop.Id.Value}' sells no '{thing.Value}', and its list shows it (T-2).")).Price;
        this.ui.Text.Put(this.countLine, Id("menu.shop_count"), Values(("count", Number(cursor.Count)), ("total", Number(checked(each * cursor.Count)))));
    }

    /// <summary>Gives the entries of the list of the mode, with the right column of each.</summary>
    private List<Entry> Entries()
    {
        List<Entry> entries = [];
        if (this.Cursor.Mode == ShopMode.Sell)
        {
            foreach (ContentId thing in this.Cursor.SellEntries)
            {
                int each = ShopRules.SaleOf(this.state, this.Cursor.Shop, thing);
                string held = Number(this.state.Characters.CountOf(thing));
                entries.Add(each > 0
                    ? new Entry(thing, Id("menu.shop_sale"), Values(("each", Number(each)), ("count", held)), true)
                    : new Entry(thing, Id("menu.shop_unwanted"), Values(("count", held)), false));
            }

            return entries;
        }

        foreach (StockEntry stock in this.Cursor.BuyEntries)
        {
            bool allowed = ShopRules.LimitOf(this.state, this.Cursor.Shop, stock).Refusal == BuyRefusal.None;
            string price = Number(stock.Price);
            if (stock.Kind == StockKind.Lesson)
            {
                entries.Add(new Entry(stock.Thing, Id("menu.shop_lesson"), Values(("price", price)), allowed));
                continue;
            }

            string count = Number(this.state.Characters.OwnedCount(stock.Thing));
            string limit = Number(this.state.BattleContent.LimitOf(stock.Thing));
            entries.Add(this.state.Shops.LeftOf(this.Cursor.Shop, stock) is int left
                ? new Entry(stock.Thing, Id("menu.shop_entry_left"), Values(("price", price), ("count", count), ("limit", limit), ("left", Number(left))), allowed)
                : new Entry(stock.Thing, Id("menu.shop_entry"), Values(("price", price), ("count", count), ("limit", limit)), allowed));
        }

        return entries;
    }

    private ViewOutcome ReadEvent(InputEvent signal, ScreenFit fit)
    {
        if (signal is InputEventMouse mouse)
        {
            return this.ReadMouse(mouse, fit);
        }

        if (signal.IsActionPressed("ui_up"))
        {
            this.Cursor.Move(-1);
        }
        else if (signal.IsActionPressed("ui_down"))
        {
            this.Cursor.Move(1);
        }
        else if (signal.IsActionPressed("ui_left"))
        {
            this.Cursor.Step(-1);
        }
        else if (signal.IsActionPressed("ui_right"))
        {
            this.Cursor.Step(1);
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

    /// <summary>Points the cursor of the stage at the line under the mouse, and confirms it on a click (D-872). The count stage takes no point.</summary>
    private ViewOutcome ReadMouse(InputEventMouse mouse, ScreenFit fit)
    {
        if (this.Cursor.Stage == ShopStage.Mode)
        {
            if (MenuNodes.LineUnder(this.modeLines, mouse, fit) is not int mode)
            {
                return ViewOutcome.Stay;
            }

            this.Cursor.Point(mode);
            return MenuNodes.IsClick(mouse) ? this.Confirm() : ViewOutcome.Stay;
        }

        if (this.Cursor.Stage == ShopStage.Count)
        {
            return MenuNodes.IsClick(mouse) ? this.Confirm() : ViewOutcome.Stay;
        }

        if ((MenuNodes.LineUnder(this.lefts, mouse, fit) ?? MenuNodes.LineUnder(this.rights, mouse, fit)) is not int line
            || this.top + line >= this.Cursor.ListCount)
        {
            return ViewOutcome.Stay;
        }

        this.Cursor.Point(this.top + line);
        return MenuNodes.IsClick(mouse) ? this.Confirm() : ViewOutcome.Stay;
    }

    /// <summary>Confirms the line under the cursor: leave closes the window, and a count makes its intent.</summary>
    private ViewOutcome Confirm()
    {
        if (this.Cursor.LeaveChosen)
        {
            return ViewOutcome.Back;
        }

        this.made = this.Cursor.Confirm();
        return this.made is null ? ViewOutcome.Stay : ViewOutcome.Chose;
    }

    /// <summary>One line of the list: the thing, the right text with its values, and whether the party can take it now.</summary>
    private sealed record Entry(ContentId Thing, ContentId Right, IReadOnlyDictionary<string, string> RightValues, bool Allowed);
}
