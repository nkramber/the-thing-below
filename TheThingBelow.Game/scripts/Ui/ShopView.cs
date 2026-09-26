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
/// The shop window: the shop menu with the gold under it, the list window to its right, the stats
/// of a piece of gear at the bottom left, and the popup of the equip step after a buy of gear
/// (D-1149 to D-1160, D-1164 to D-1167).
/// </summary>
/// <remarks>
/// The shop menu stands where the main list stands, and the list window stands where a task window
/// stands, so the shop reads as the menu of the walk (D-211, D-1164). The list window shows after a
/// choice of buy or sell. Its list starts right under the title, and each value stands in a column
/// of its own (D-1165). The window makes one intent for each whole choice and never changes the run
/// itself. The change lands on the next tick, and the window shows it on the frame after (D-493,
/// T-7). Every label takes its text from the string table through the text helper (G-7, D-499).
/// </remarks>
public sealed class ShopView : IMenuView
{
    /// <summary>The share of the inner width of the list window that the name column takes, in hundredths.</summary>
    private const int NameShare = 45;

    /// <summary>The share of the inner width of the list window that the price column takes, in hundredths.</summary>
    private const int PriceShare = 25;

    /// <summary>The most lines of the popup under its title: two rows for each character of a full party (D-31, D-1167).</summary>
    private const int PopupRows = BattleFixture.MostCharacters * 2;

    /// <summary>The characters of the name column of the popup of the characters: a name, or the name of a worn piece (D-1168).</summary>
    private const int NameCharacters = 12;

    /// <summary>The characters of one stat cell of the popup: a stat of 999 and a change of 99, and a gap (D-1168).</summary>
    private const int CellCharacters = 8;

    /// <summary>The string id of the name of each stat that gear changes, in the order of the cells (D-1052, D-1056).</summary>
    private static readonly string[] StatNames = ["battle.stat_atk", "battle.stat_mag", "battle.stat_def", "battle.stat_res", "battle.stat_spd"];

    private readonly UiBase ui;
    private readonly StringTable strings;
    private readonly RunState state;
    private readonly Control layer;
    private readonly Control listWindow;
    private readonly Control statsPanel;
    private readonly Control askPopup;
    private readonly Label askTitle;
    private readonly List<Label> askLines = [];
    private readonly Control whoPopup;
    private readonly Panel whoPanel;
    private readonly List<Label> modeLines = [];
    private readonly GoldPanel gold;
    private readonly Label title;
    private readonly List<Label> names = [];
    private readonly List<Label> prices = [];
    private readonly List<Label> amounts = [];
    private readonly List<Label> statLines = [];
    private readonly Label popupTitle;
    private readonly List<Label> popupStatNames = [];
    private readonly List<Label> popupNames = [];
    private readonly List<List<Label>> popupCells = [];
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
    /// <param name="strings">The string table, which gives the name of each stat (D-1056).</param>
    /// <param name="state">The state of the run, which the window reads on each frame and never changes.</param>
    /// <param name="cursor">The cursor, which the window keeps when the screen builds again.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public ShopView(FrameRoot frame, UiBase ui, StringTable strings, RunState state, ShopCursor cursor)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(ui);
        ArgumentNullException.ThrowIfNull(strings);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(cursor);

        this.ui = ui;
        this.strings = strings;
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

        // The stats of a piece stand in a panel of their own at the bottom left (D-1165).
        this.statsPanel = SubLayer(this.layer);
        FrameBox stats = MenuLayout.StatsBox(body, StatNames.Length);
        MenuNodes.Panel(this.statsPanel, stats);
        for (int stat = 0; stat < StatNames.Length; stat += 1)
        {
            this.statLines.Add(MenuNodes.Line(this.statsPanel, stats.X + MenuLayout.Pad, stats.Y + MenuLayout.Pad + (stat * line), stats.Width - (MenuLayout.Pad * 2), line));
        }

        // The list window shows after a choice of buy or sell.
        this.listWindow = SubLayer(this.layer);
        FrameBox box = MenuLayout.TaskBox();
        MenuNodes.Panel(this.listWindow, box);
        this.title = TitleLine(this.listWindow, box);
        int left = box.X + MenuLayout.Pad;
        int inner = box.Width - (MenuLayout.Pad * 2);
        int nameWidth = inner * NameShare / 100;
        int priceWidth = inner * PriceShare / 100;
        int first = MenuLayout.FirstLineTop(body, ui.Theme.TitleSize);

        // The list takes each line between the title and the count line above the line of help.
        int listLines = MenuLayout.LogLines(body, ui.Theme.TitleSize) - 2;
        for (int index = 0; index < listLines; index += 1)
        {
            int row = first + (line * index);
            this.names.Add(MenuNodes.Line(this.listWindow, left, row, nameWidth, line));
            this.prices.Add(MenuNodes.Line(this.listWindow, left + nameWidth, row, priceWidth, line));
            this.amounts.Add(MenuNodes.Line(this.listWindow, left + nameWidth + priceWidth, row, inner - nameWidth - priceWidth, line));
        }

        int bottom = box.Y + box.Height - MenuLayout.Pad - line;
        this.countLine = MenuNodes.Line(this.listWindow, left, bottom - line, inner, line);
        MenuNodes.Paint(this.countLine, this.chosenColor);
        this.help = MenuNodes.Line(this.listWindow, left, bottom, inner, line);

        // The question of the equip step is a small box in the middle of the screen (D-1168).
        this.askPopup = SubLayer(this.layer);
        FrameBox ask = AskBox(body);
        MenuNodes.Panel(this.askPopup, ask);
        this.askTitle = MenuNodes.Line(this.askPopup, ask.X + MenuLayout.Pad, ask.Y + MenuLayout.Pad, ask.Width - (MenuLayout.Pad * 2), line);
        ui.Text.Put(this.askTitle, Id("menu.shop_equip_ask"));
        foreach (string choice in new[] { "menu.yes", "menu.no" })
        {
            Label label = MenuNodes.Line(this.askPopup, ask.X + MenuLayout.Pad, ask.Y + MenuLayout.Pad + (line * (this.askLines.Count + 2)), ask.Width - (MenuLayout.Pad * 2), line);
            ui.Text.Put(label, Id(choice));
            this.askLines.Add(label);
        }

        // The characters of the equip step stand in a box in the middle of the screen, as wide as
        // its columns. Each show places the box and its lines for the rows that it holds (D-1168).
        this.whoPopup = SubLayer(this.layer);
        this.whoPanel = new Panel { MouseFilter = Control.MouseFilterEnum.Ignore };
        this.whoPopup.AddChild(this.whoPanel);
        int glyph = body / 2;
        this.popupTitle = MenuNodes.Line(this.whoPopup, 0, 0, (NameCharacters + (CellCharacters * StatNames.Length)) * glyph, line);
        for (int stat = 0; stat < StatNames.Length; stat += 1)
        {
            Label name = MenuNodes.Line(this.whoPopup, 0, 0, CellCharacters * glyph, line);
            ui.Text.Put(name, Id(StatNames[stat]));
            MenuNodes.Paint(name, this.dimColor);
            this.popupStatNames.Add(name);
        }

        for (int row = 0; row < PopupRows; row += 1)
        {
            this.popupNames.Add(MenuNodes.Line(this.whoPopup, 0, 0, NameCharacters * glyph, line));
            List<Label> cells = [];
            for (int stat = 0; stat < StatNames.Length; stat += 1)
            {
                cells.Add(MenuNodes.Line(this.whoPopup, 0, 0, CellCharacters * glyph, line));
            }

            this.popupCells.Add(cells);
        }

        this.Show();
    }

    /// <summary>The cursor of the window.</summary>
    public ShopCursor Cursor { get; }

    /// <summary>Gives the count of characters that one stat cell of the popup holds at a body size (D-1167, D-1168).</summary>
    /// <param name="body">The body size, in frame pixels (D-707).</param>
    /// <returns>The count of whole characters.</returns>
    public static int StatCellCharacters(int body) => UiMetrics.CharactersAcross(body, CellCharacters * (body / 2));

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
    /// <returns>The buy intent, the sale intent, or the wear intent, or no value when the last read made none.</returns>
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
        this.askPopup.Visible = cursor.Stage == ShopStage.EquipAsk;
        this.whoPopup.Visible = cursor.Stage is ShopStage.EquipWho or ShopStage.EquipSlot;
        this.ShowStats();
        if (cursor.Stage == ShopStage.Mode)
        {
            return;
        }

        this.ui.Text.Put(this.title, ModeIdOf(cursor.Mode));
        this.ShowList();
        this.ShowCount();
        ContentId? refusal = cursor.Refusal();
        this.ui.Text.Put(this.help, refusal ?? cursor.EquipPiece ?? cursor.Thing ?? ModeHelpOf(cursor.Mode));
        MenuNodes.Paint(this.help, refusal is null ? this.dimColor : this.warningColor);
        if (this.askPopup.Visible)
        {
            this.ShowAsk();
        }

        if (this.whoPopup.Visible)
        {
            this.ShowWho();
        }
    }

    /// <inheritdoc/>
    public void Free() => this.layer.QueueFree();

    private static Control SubLayer(Control parent)
    {
        var sub = new Control
        {
            Position = Vector2.Zero,
            Size = new Vector2(ScreenFit.FrameWidth, ScreenFit.FrameHeight),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        parent.AddChild(sub);
        return sub;
    }

    private static Label TitleLine(Control parent, FrameBox box)
    {
        var label = new Label
        {
            Position = new Vector2(box.X + MenuLayout.Pad, box.Y + MenuLayout.Pad),
            ThemeTypeVariation = UiTheme.TitleVariation,
        };
        parent.AddChild(label);
        return label;
    }

    /// <summary>Gives the place of the question of the equip step: as wide as the main list, with the question, a gap, yes, and no, in the middle of the screen (D-1168).</summary>
    private static FrameBox AskBox(int body)
    {
        int height = (MenuLayout.Pad * 2) + (MenuLayout.LineOf(body) * 4);
        return new FrameBox((ScreenFit.FrameWidth - MenuLayout.MainListWidth) / 2, (ScreenFit.FrameHeight - height) / 2, MenuLayout.MainListWidth, height);
    }

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

    /// <summary>Gives the piece of gear under the cursor of the list, or the piece of the equip step, or no value.</summary>
    private GearRecord? ShownPiece()
    {
        GearList gear = this.state.BattleContent.Gear;
        ContentId? thing = this.Cursor.EquipPiece ?? (this.Cursor.Stage == ShopStage.Mode ? null : this.Cursor.Thing);
        return thing is ContentId id && gear.Holds(id) ? gear.Piece(id) : null;
    }

    /// <summary>Shows the stats of the piece of gear under the cursor, and of no character (D-1165).</summary>
    private void ShowStats()
    {
        GearRecord? piece = this.ShownPiece();
        this.statsPanel.Visible = piece is not null;
        if (piece is null)
        {
            return;
        }

        int[] values = [piece.Attack, piece.Magic, piece.Defense, piece.Resistance, piece.Speed];
        for (int stat = 0; stat < StatNames.Length; stat += 1)
        {
            this.ui.Text.Put(this.statLines[stat], Id("menu.stat"), Values(("stat", this.strings.Text(Id(StatNames[stat]))), ("value", Number(values[stat]))));
            MenuNodes.Paint(this.statLines[stat], values[stat] == 0 ? this.dimColor : null);
        }
    }

    /// <summary>Shows the lines of the list of the mode, from the line at the top of the scroll. Each value stands in its own column (D-1165).</summary>
    private void ShowList()
    {
        List<Entry> entries = this.Entries();
        int shown = this.names.Count;
        int cursor = Math.Min(this.Cursor.Cursor, Math.Max(0, entries.Count - 1));
        this.top = Math.Clamp(this.top, Math.Max(0, cursor - shown + 1), cursor);
        for (int index = 0; index < shown; index += 1)
        {
            int at = this.top + index;
            bool empty = entries.Count == 0 && index == 0;
            bool filled = at < entries.Count;
            this.names[index].Visible = filled || empty;
            this.prices[index].Visible = filled && entries[at].Price is not null;
            this.amounts[index].Visible = filled && entries[at].Amount is not null;
            if (empty)
            {
                this.ui.Text.Put(this.names[index], Id("menu.shop_empty"));
                MenuNodes.Paint(this.names[index], this.dimColor);
                continue;
            }

            if (!filled)
            {
                continue;
            }

            Entry entry = entries[at];
            this.ui.Text.Put(this.names[index], BattleMessages.NameIdOf(entry.Thing));
            if (entry.Price is (ContentId price, IReadOnlyDictionary<string, string> priceValues))
            {
                this.ui.Text.Put(this.prices[index], price, priceValues);
            }

            if (entry.Amount is (ContentId amount, IReadOnlyDictionary<string, string> amountValues))
            {
                this.ui.Text.Put(this.amounts[index], amount, amountValues);
            }

            Color? color = at == cursor ? this.chosenColor : entry.Allowed ? null : this.dimColor;
            MenuNodes.Paint(this.names[index], color);
            MenuNodes.Paint(this.prices[index], color);
            MenuNodes.Paint(this.amounts[index], color);
        }
    }

    /// <summary>Shows the count and its total in the count stage, and no line in the other stages (D-1158).</summary>
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

    /// <summary>Shows the question of the equip step, with the cursor on yes or no (D-1167, D-1168).</summary>
    private void ShowAsk()
    {
        for (int index = 0; index < this.askLines.Count; index += 1)
        {
            MenuNodes.Paint(this.askLines[index], index == this.Cursor.AskCursor ? this.chosenColor : null);
        }
    }

    /// <summary>
    /// Shows the popup of the characters: each character with the change of each slot that the
    /// piece can take, or the two full accessory slots with the change of the slot under the cursor
    /// alone. The box stands in the middle of the screen, as tall as its rows (D-1167, D-1168).
    /// </summary>
    private void ShowWho()
    {
        ShopCursor cursor = this.Cursor;
        IReadOnlyList<PartyMember> members = this.state.Characters.Members;
        int rows = 0;
        if (cursor.Stage == ShopStage.EquipWho)
        {
            foreach (PartyMember member in members)
            {
                rows += cursor.SlotsFor(member).Count;
            }
        }
        else
        {
            rows = cursor.SlotsFor(members[cursor.WhoCursor]).Count;
        }

        this.PlaceWho(rows);
        int row = 0;
        switch (cursor.Stage)
        {
            case ShopStage.EquipWho:
                this.ui.Text.Put(this.popupTitle, Id("menu.shop_equip_who"));
                for (int who = 0; who < members.Count; who += 1)
                {
                    IReadOnlyList<int> slots = cursor.SlotsFor(members[who]);
                    for (int place = 0; place < slots.Count; place += 1)
                    {
                        ContentId? name = place == 0 ? BattleMessages.NameIdOf(members[who].Record.Id) : null;
                        this.PutTrial(row++, name, members[who], slots[place], who == cursor.WhoCursor);
                    }
                }

                break;
            default:
                this.ui.Text.Put(this.popupTitle, Id("menu.shop_replace"));
                PartyMember wearer = members[cursor.WhoCursor];
                IReadOnlyList<int> full = cursor.SlotsFor(wearer);
                for (int place = 0; place < full.Count; place += 1)
                {
                    ContentId worn = wearer.Gear[full[place]] ?? throw new InvalidOperationException($"The accessory slot {full[place]} of '{wearer.Record.Id.Value}' holds no piece, and the window asks which piece to replace (T-2).");
                    bool chosen = place == cursor.SlotCursor;
                    if (chosen)
                    {
                        this.PutTrial(row++, BattleMessages.NameIdOf(worn), wearer, full[place], true);
                    }
                    else
                    {
                        this.PutChoice(row++, BattleMessages.NameIdOf(worn), false);
                    }
                }

                break;
        }

        for (; row < PopupRows; row += 1)
        {
            this.popupNames[row].Visible = false;
            this.SetCells(row, false);
        }
    }

    /// <summary>Puts one row of the popup with a name alone.</summary>
    private void PutChoice(int row, ContentId name, bool chosen)
    {
        this.popupNames[row].Visible = true;
        this.ui.Text.Put(this.popupNames[row], name);
        MenuNodes.Paint(this.popupNames[row], chosen ? this.chosenColor : null);
        this.SetCells(row, false);
    }

    /// <summary>
    /// Puts one row of the popup: a name or none, and the stats of the character with the piece in
    /// one slot, green with the gain and red with the loss, as the gear window shows them (D-1060).
    /// </summary>
    private void PutTrial(int row, ContentId? name, PartyMember member, int slot, bool chosen)
    {
        this.popupNames[row].Visible = name is not null;
        if (name is ContentId shown)
        {
            this.ui.Text.Put(this.popupNames[row], shown);
            MenuNodes.Paint(this.popupNames[row], chosen ? this.chosenColor : null);
        }

        this.SetCells(row, true);
        int[] worn = ValuesOf(member.StatsWith(this.state.BattleContent.Gear));
        int[] trial = ValuesOf(this.Cursor.TrialOf(member, slot));
        for (int stat = 0; stat < StatNames.Length; stat += 1)
        {
            Label cell = this.popupCells[row][stat];
            IReadOnlyDictionary<string, string> values = trial[stat] == worn[stat]
                ? Values(("value", Number(trial[stat])))
                : Values(("change", Number(Math.Abs(trial[stat] - worn[stat]))), ("value", Number(trial[stat])));
            this.ui.Text.Put(cell, GearView.TrialIdOf(worn[stat], trial[stat]), values);
            MenuNodes.Paint(cell, trial[stat] > worn[stat] ? this.gainColor : trial[stat] < worn[stat] ? this.lossColor : this.dimColor);
        }
    }

    /// <summary>
    /// Places the popup of the characters in the middle of the screen for a count of rows: the title,
    /// a gap, the names of the stats, and the rows, as wide as the name column and the stat cells (D-1168).
    /// </summary>
    private void PlaceWho(int rows)
    {
        int body = this.ui.Theme.BodySize;
        int line = MenuLayout.LineOf(body);
        int glyph = body / 2;
        int nameWidth = NameCharacters * glyph;
        int cellWidth = CellCharacters * glyph;
        int width = (MenuLayout.Pad * 2) + nameWidth + (cellWidth * StatNames.Length);
        int height = (MenuLayout.Pad * 2) + (line * (rows + 3));
        int x = (ScreenFit.FrameWidth - width) / 2;
        int y = (ScreenFit.FrameHeight - height) / 2;
        this.whoPanel.Position = new Vector2(x, y);
        this.whoPanel.Size = new Vector2(width, height);
        int left = x + MenuLayout.Pad;
        int top = y + MenuLayout.Pad;
        this.popupTitle.Position = new Vector2(left, top);
        for (int stat = 0; stat < StatNames.Length; stat += 1)
        {
            this.popupStatNames[stat].Position = new Vector2(left + nameWidth + (cellWidth * stat), top + (line * 2));
        }

        for (int row = 0; row < PopupRows; row += 1)
        {
            this.popupNames[row].Position = new Vector2(left, top + (line * (row + 3)));
            for (int stat = 0; stat < StatNames.Length; stat += 1)
            {
                this.popupCells[row][stat].Position = new Vector2(left + nameWidth + (cellWidth * stat), top + (line * (row + 3)));
            }
        }
    }

    private void SetCells(int row, bool visible)
    {
        foreach (Label cell in this.popupCells[row])
        {
            cell.Visible = visible;
        }
    }

    /// <summary>Gives the entries of the list of the mode, with the price column and the amount column of each (D-1165).</summary>
    private List<Entry> Entries()
    {
        List<Entry> entries = [];
        if (this.Cursor.Mode == ShopMode.Sell)
        {
            foreach (ContentId thing in this.Cursor.SellEntries)
            {
                int each = ShopRules.SaleOf(this.state, this.Cursor.Shop, thing);
                (ContentId, IReadOnlyDictionary<string, string>) price = each > 0
                    ? (Id("menu.shop_price"), Values(("price", Number(each))))
                    : (Id("menu.shop_unwanted"), Values());
                entries.Add(new Entry(thing, price, (Id("menu.shop_held"), Values(("count", Number(this.state.Characters.CountOf(thing))))), each > 0));
            }

            return entries;
        }

        foreach (StockEntry stock in this.Cursor.BuyEntries)
        {
            (ContentId, IReadOnlyDictionary<string, string>)? left = this.state.Shops.LeftOf(this.Cursor.Shop, stock) is int count
                ? (Id("menu.shop_left"), Values(("left", Number(count))))
                : null;
            entries.Add(new Entry(stock.Thing, (Id("menu.shop_price"), Values(("price", Number(stock.Price)))), left, true));
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

    /// <summary>Points the cursor of the shop menu or of the list at the line under the mouse, and confirms it on a click (D-872). The count and the popup take a click alone.</summary>
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

        if (this.Cursor.Stage != ShopStage.List)
        {
            return MenuNodes.IsClick(mouse) ? this.Confirm() : ViewOutcome.Stay;
        }

        if ((MenuNodes.LineUnder(this.names, mouse, fit) ?? MenuNodes.LineUnder(this.prices, mouse, fit) ?? MenuNodes.LineUnder(this.amounts, mouse, fit)) is not int line
            || this.top + line >= this.Cursor.ListCount)
        {
            return ViewOutcome.Stay;
        }

        this.Cursor.Point(this.top + line);
        return MenuNodes.IsClick(mouse) ? this.Confirm() : ViewOutcome.Stay;
    }

    /// <summary>Confirms the line under the cursor: leave closes the window, and a whole choice makes its intent.</summary>
    private ViewOutcome Confirm()
    {
        if (this.Cursor.LeaveChosen)
        {
            return ViewOutcome.Back;
        }

        this.made = this.Cursor.Confirm();
        return this.made is null ? ViewOutcome.Stay : ViewOutcome.Chose;
    }

    /// <summary>One line of the list: the thing, the price column, the amount column, and whether the party can take it.</summary>
    private sealed record Entry(
        ContentId Thing,
        (ContentId Id, IReadOnlyDictionary<string, string> Values)? Price,
        (ContentId Id, IReadOnlyDictionary<string, string> Values)? Amount,
        bool Allowed);
}
