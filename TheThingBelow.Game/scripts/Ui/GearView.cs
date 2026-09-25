using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The gear window: one character at a time, with the stats that the gear gives, the six gear
/// slots, and the pieces of the pack that fit a slot (D-44, D-1036, D-1048). A second line of
/// stats compares the piece under the cursor with the worn piece (D-1060).
/// </summary>
/// <remarks>
/// The window makes one intent for each whole choice and never changes the run itself. The
/// change lands on the next tick, and the window shows it on the frame after (D-493, T-7). An
/// empty slot shows a dash (D-44). The last line holds the line of the piece under the cursor.
/// Each line of stats holds one cell for each of the five stats that gear changes, so the cells
/// of the two lines stand in columns (D-1052).
/// </remarks>
public sealed class GearView : IMenuView
{
    /// <summary>The lines above the list: the character, the worn stats, the trial stats, and the caption of the list.</summary>
    private const int HeadLines = 4;

    /// <summary>The share of the inner width of the window that the left column of the list takes, in hundredths.</summary>
    private const int LeftShare = 45;

    /// <summary>The string id of the name of each stat that gear changes, in the order of the cells (D-1036, D-1052, D-1056).</summary>
    private static readonly string[] StatNames = ["battle.stat_atk", "battle.stat_mag", "battle.stat_def", "battle.stat_res", "battle.stat_spd"];

    private readonly UiBase ui;
    private readonly StringTable strings;
    private readonly RunState state;
    private readonly Control layer;
    private readonly Label name;
    private readonly Label level;
    private readonly List<Label> wornCells = [];
    private readonly List<Label> trialCells = [];
    private readonly Label caption;
    private readonly Label help;
    private readonly List<Label> lefts = [];
    private readonly List<Label> rights = [];
    private readonly Color chosenColor;
    private readonly Color dimColor;
    private readonly Color gainColor;
    private readonly Color lossColor;
    private Intent? made;
    private int top;

    /// <summary>Builds the gear window beside the main list.</summary>
    /// <param name="frame">The frame, whose UI layer takes the window.</param>
    /// <param name="ui">The atlas, the theme, and the text helper.</param>
    /// <param name="strings">The string table, which gives the name of each stat (D-979, D-1056).</param>
    /// <param name="state">The state of the run, which the window reads on each frame and never changes.</param>
    /// <param name="cursor">The cursor, which the window keeps when the screen builds again.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public GearView(FrameRoot frame, UiBase ui, StringTable strings, RunState state, GearCursor cursor)
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
        this.layer = MenuNodes.Layer(frame, ui);

        int body = ui.Theme.BodySize;
        FrameBox box = MenuLayout.TaskBox();
        MenuNodes.Panel(this.layer, box);
        MenuNodes.Title(this.layer, ui, box, Id("menu.gear"));
        int line = MenuLayout.LineOf(body);
        int left = box.X + MenuLayout.Pad;
        int inner = box.Width - (MenuLayout.Pad * 2);
        int leftWidth = inner * LeftShare / 100;
        int first = MenuLayout.FirstLineTop(body, ui.Theme.TitleSize);
        this.name = MenuNodes.Line(this.layer, left, first, leftWidth, line);
        this.level = MenuNodes.Line(this.layer, left + leftWidth, first, inner - leftWidth, line);
        int cell = inner / StatNames.Length;
        for (int stat = 0; stat < StatNames.Length; stat += 1)
        {
            this.wornCells.Add(MenuNodes.Line(this.layer, left + (cell * stat), first + line, cell, line));
            this.trialCells.Add(MenuNodes.Line(this.layer, left + (cell * stat), first + (line * 2), cell, line));
        }

        this.caption = MenuNodes.Line(this.layer, left, first + (line * 3), inner, line);
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
    public GearCursor Cursor { get; }

    /// <summary>Takes the intent of the last whole choice, once.</summary>
    /// <returns>The intent, or no value when the last read made none.</returns>
    public Intent? TakeIntent()
    {
        Intent? taken = this.made;
        this.made = null;
        return taken;
    }

    /// <summary>Gives the count of characters that one cell of a line of stats holds at a body size (D-1060).</summary>
    /// <param name="body">The body size, in frame pixels (D-707).</param>
    /// <returns>The count of whole characters.</returns>
    public static int StatCellCharacters(int body) =>
        UiMetrics.CharactersAcross(body, (MenuLayout.TaskBox().Width - (MenuLayout.Pad * 2)) / StatNames.Length);

    /// <summary>Gives the string id of the label of a gear slot, such as `menu.slot_weapon` (D-44).</summary>
    /// <param name="slot">The gear slot, from 0 to 5.</param>
    /// <returns>The id.</returns>
    public static ContentId SlotIdOf(int slot) => Id($"menu.slot_{GearList.NameOf(GearRules.KindOf(slot))}");

    /// <summary>
    /// Gives the string id of one cell of the trial line: the stat alone when it holds, the gain
    /// and the stat when it rises, and the loss and the stat when it falls (D-1060).
    /// </summary>
    /// <param name="worn">The stat with the worn gear.</param>
    /// <param name="trial">The stat with the piece under the cursor.</param>
    /// <returns>The id of `menu.gear_same`, `menu.gear_gain`, or `menu.gear_loss`.</returns>
    public static ContentId TrialIdOf(int worn, int trial)
    {
        if (trial > worn)
        {
            return Id("menu.gear_gain");
        }

        return trial < worn ? Id("menu.gear_loss") : Id("menu.gear_same");
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
        PartyMember member = this.Cursor.Member;
        StatRow worn = member.StatsWith(this.state.BattleContent.Gear);
        this.ui.Text.Put(this.name, BattleMessages.NameIdOf(member.Record.Id));
        this.ui.Text.Put(this.level, Id("menu.level"), Values(("level", Number(member.Level))));
        int[] wornValues = ValuesOf(worn);
        int[] trialValues = ValuesOf(this.Cursor.TrialStats());
        for (int stat = 0; stat < StatNames.Length; stat += 1)
        {
            string name = this.strings.Text(Id(StatNames[stat]));
            this.ui.Text.Put(this.wornCells[stat], Id("menu.stat"), Values(("stat", name), ("value", Number(wornValues[stat]))));
            this.PutTrial(this.trialCells[stat], wornValues[stat], trialValues[stat]);
        }

        this.ui.Text.Put(this.caption, Id(this.Cursor.Stage == GearStage.Slot ? "menu.gear_slots" : "menu.gear_pack"));

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

    private static ContentId Id(string value) => ContentId.Parse(value, StringTable.Path, nameof(GearView));

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

    /// <summary>Gives the entries of the list of the stage, with the right column of each.</summary>
    private List<Entry> Entries()
    {
        GearCursor cursor = this.Cursor;
        var entries = new List<Entry>();
        if (cursor.Stage == GearStage.Slot)
        {
            for (int slot = 0; slot < GearRules.SlotCount; slot += 1)
            {
                ContentId? piece = cursor.Member.Gear[slot];
                ContentId right = piece is ContentId worn ? BattleMessages.NameIdOf(worn) : Id("menu.slot_empty");
                entries.Add(new Entry(SlotIdOf(slot), right, Values(), cursor.AllowsSlot(slot)));
            }

            return entries;
        }

        for (int index = 0; index < cursor.PackEntries.Count; index += 1)
        {
            entries.Add(cursor.PackEntries[index] is ContentId piece
                ? new Entry(BattleMessages.NameIdOf(piece), null, Values(), cursor.AllowsPackEntry(index))
                : new Entry(Id("menu.gear_remove"), null, Values(), cursor.AllowsPackEntry(index)));
        }

        return entries;
    }

    /// <summary>
    /// Puts one cell of the trial line: grey when the stat holds, green with the gain when it
    /// rises, and red with the loss when it falls. The cell shows the whole stat with the piece
    /// under the cursor (D-1060).
    /// </summary>
    private void PutTrial(Label cell, int worn, int trial)
    {
        // A stat that holds shows its value alone, and its line takes no change (F-128).
        IReadOnlyDictionary<string, string> values = trial == worn
            ? Values(("value", Number(trial)))
            : Values(("change", Number(Math.Abs(trial - worn))), ("value", Number(trial)));
        this.ui.Text.Put(cell, TrialIdOf(worn, trial), values);
        Color color = trial > worn ? this.gainColor : trial < worn ? this.lossColor : this.dimColor;
        MenuNodes.Paint(cell, color);
    }

    /// <summary>Gives the last line: the line of the piece under the cursor, or how the window works.</summary>
    private ContentId HelpId()
    {
        ContentId? piece = this.Cursor.Stage == GearStage.Slot
            ? this.Cursor.Member.Gear[this.Cursor.Cursor]
            : this.Cursor.PackEntries[this.Cursor.Cursor];
        return piece ?? Id("menu.gear_help");
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
        else if (signal.IsActionPressed("ui_left"))
        {
            this.Cursor.Turn(-1);
        }
        else if (signal.IsActionPressed("ui_right"))
        {
            this.Cursor.Turn(1);
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
