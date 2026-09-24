using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The lesson window: one character at a time, with the main aptitude and the side aptitude, the
/// lesson slots, and the lists of a swap and of a cast from the menu (D-356, D-391, D-1027,
/// D-1030, D-1033).
/// </summary>
/// <remarks>
/// The window makes one intent for each whole choice and never changes the run itself. The
/// change lands on the next tick, and the window shows it on the frame after (D-493, T-7). A
/// locked side aptitude shows an empty mark (D-283). The last line holds the description of
/// the form under the cursor, or a line on where a swap works.
/// </remarks>
public sealed class LessonsView : IMenuView
{
    /// <summary>The lines above the list: the character, the aptitudes, and the caption of the list.</summary>
    private const int HeadLines = 3;

    /// <summary>The share of the inner width of the window that the left column of the list takes, in hundredths.</summary>
    private const int LeftShare = 65;

    private readonly UiBase ui;
    private readonly StringTable strings;
    private readonly RunState state;
    private readonly Control layer;
    private readonly Label name;
    private readonly Label level;
    private readonly Label aptitudes;
    private readonly Label caption;
    private readonly Label help;
    private readonly List<Label> lefts = [];
    private readonly List<Label> rights = [];
    private readonly Color chosenColor;
    private readonly Color dimColor;
    private Intent? made;
    private int top;

    /// <summary>Builds the lesson window beside the main list.</summary>
    /// <param name="frame">The frame, whose UI layer takes the window.</param>
    /// <param name="ui">The atlas, the theme, and the text helper.</param>
    /// <param name="strings">The string table, which gives each name inside a line (G-7).</param>
    /// <param name="state">The state of the run, which the window reads on each frame and never changes.</param>
    /// <param name="cursor">The cursor, which the window keeps when the screen builds again.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public LessonsView(FrameRoot frame, UiBase ui, StringTable strings, RunState state, LessonCursor cursor)
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
        this.layer = MenuNodes.Layer(frame, ui);

        int body = ui.Theme.BodySize;
        FrameBox box = MenuLayout.TaskBox();
        MenuNodes.Panel(this.layer, box);
        MenuNodes.Title(this.layer, ui, box, Id("menu.lessons"));
        int line = MenuLayout.LineOf(body);
        int left = box.X + MenuLayout.Pad;
        int inner = box.Width - (MenuLayout.Pad * 2);
        int leftWidth = inner * LeftShare / 100;
        int first = MenuLayout.FirstLineTop(body, ui.Theme.TitleSize);
        this.name = MenuNodes.Line(this.layer, left, first, leftWidth, line);
        this.level = MenuNodes.Line(this.layer, left + leftWidth, first, inner - leftWidth, line);
        this.aptitudes = MenuNodes.Line(this.layer, left, first + line, inner, line);
        this.caption = MenuNodes.Line(this.layer, left, first + (line * 2), inner, line);
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
    public LessonCursor Cursor { get; }

    /// <summary>The count of lines of the list at the body size of this build of the window.</summary>
    public int ListLines => this.lefts.Count;

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
        PartyMember member = this.Cursor.Member;
        this.ui.Text.Put(this.name, BattleMessages.NameIdOf(member.Record.Id));
        this.ui.Text.Put(this.level, Id("menu.level"), Values(("level", Number(member.Level))));
        this.ui.Text.Put(this.aptitudes, Id("menu.lesson_aptitudes"), Values(
            ("main", this.Text(AptitudeIdOf(member.Record.MainAptitude))),
            ("side", this.Text(SideIdOf(member.Record, this.state.Story.Flags)))));
        this.ui.Text.Put(this.caption, CaptionOf(this.Cursor.Stage));

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
            this.ui.Text.Put(this.lefts[index], entry.Left, entry.LeftValues);
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

    /// <summary>Gives the string id of the name of one kind of ability (D-281, D-1033).</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>The id, such as `aptitude.mend`.</returns>
    public static ContentId AptitudeIdOf(AptitudeKind kind) => Id($"aptitude.{Aptitudes.NameOf(kind)}");

    /// <summary>Gives the string id of the side aptitude of a character: its kind once its flag is on, and an empty mark before (D-283, D-538).</summary>
    /// <param name="record">The character.</param>
    /// <param name="flags">The story flags.</param>
    /// <returns>The id, such as `aptitude.guard` or `menu.aptitude_hidden`.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static ContentId SideIdOf(CharacterRecord record, FlagSet flags)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(flags);

        return flags.IsOn(record.SideFlag) ? AptitudeIdOf(record.SideAptitude) : Id("menu.aptitude_hidden");
    }

    private static ContentId CaptionOf(LessonStage stage) => Id(stage switch
    {
        LessonStage.Slot => "menu.lesson_slots",
        LessonStage.Choice => "menu.lesson_choice",
        LessonStage.Pack => "menu.lesson_pack",
        LessonStage.Form => "menu.lesson_forms",
        _ => "menu.lesson_target",
    });

    private static ContentId Id(string value) => ContentId.Parse(value, StringTable.Path, nameof(LessonsView));

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

    private string Text(ContentId id) => this.strings.Text(id);

    /// <summary>Gives the entries of the list of the stage, with the right column of each.</summary>
    private List<Entry> Entries()
    {
        LessonCursor cursor = this.Cursor;
        var entries = new List<Entry>();
        switch (cursor.Stage)
        {
            case LessonStage.Slot:
                for (int slot = 0; slot < cursor.Member.Slots.Count; slot += 1)
                {
                    entries.Add(this.LessonEntry(cursor.Member.Slots[slot], cursor.UsesOf(slot).Count > 0));
                }

                break;
            case LessonStage.Choice:
                foreach (SlotUse use in cursor.Uses)
                {
                    entries.Add(new Entry(Id(use == SlotUse.Cast ? "menu.lesson_use" : "menu.lesson_swap"), Values(), null, Values(), true));
                }

                break;
            case LessonStage.Pack:
                for (int index = 0; index < cursor.PackEntries.Count; index += 1)
                {
                    entries.Add(cursor.PackEntries[index] is ContentId lesson
                        ? this.LessonEntry(lesson, cursor.AllowsPackEntry(index))
                        : new Entry(Id("menu.lesson_clear"), Values(), null, Values(), cursor.AllowsPackEntry(index)));
                }

                break;
            case LessonStage.Form:
                for (int index = 0; index < cursor.Forms.Count; index += 1)
                {
                    LessonForm form = cursor.Forms[index];
                    string formName = this.Text(BattleMessages.NameIdOf(form.Ability));
                    entries.Add(form.Mp == 0
                        ? new Entry(Id("battle.form_entry_free"), Values(("form", formName)), null, Values(), cursor.AllowsForm(index))
                        : new Entry(Id("battle.form_entry"), Values(("form", formName), ("mp", Number(form.Mp))), null, Values(), cursor.AllowsForm(index)));
                }

                break;
            default:
                IReadOnlyList<PartyMember> members = this.state.Characters.Members;
                for (int slot = 0; slot < members.Count; slot += 1)
                {
                    PartyMember member = members[slot];
                    entries.Add(new Entry(
                        BattleMessages.NameIdOf(member.Record.Id),
                        Values(),
                        Id("battle.health"),
                        Values(("health", Number(member.Health)), ("full", Number(member.Stats.Health))),
                        cursor.AllowsTarget(slot)));
                }

                break;
        }

        return entries;
    }

    /// <summary>Gives the entry of one lesson: its name, and the points of the character toward its next form (D-361, D-1021).</summary>
    private Entry LessonEntry(ContentId? lesson, bool allowed)
    {
        if (lesson is not ContentId held)
        {
            return new Entry(Id("menu.slot_empty"), Values(), null, Values(), allowed);
        }

        LessonRecord record = this.state.BattleContent.Lessons.Lesson(held);
        int points = this.Cursor.Member.PointsOf(held);
        int opened = record.OpenedAt(points);

        // A lesson of one form has no next form, so the right column stays empty.
        if (record.Forms.Count == 1)
        {
            return new Entry(BattleMessages.NameIdOf(held), Values(), null, Values(), allowed);
        }

        return opened >= record.Forms.Count
            ? new Entry(BattleMessages.NameIdOf(held), Values(), Id("menu.lesson_full"), Values(), allowed)
            : new Entry(BattleMessages.NameIdOf(held), Values(), Id("menu.lesson_points"), Values(("points", Number(points)), ("next", Number(record.Forms[opened].Points))), allowed);
    }

    /// <summary>Gives the last line: the description of the form under the cursor, or where a swap works (D-1027, D-1050).</summary>
    private ContentId HelpId()
    {
        if (this.Cursor.Stage == LessonStage.Form)
        {
            return this.Cursor.Forms[this.Cursor.Cursor].Description;
        }

        return Id("menu.lesson_help");
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
    private sealed record Entry(
        ContentId Left,
        IReadOnlyDictionary<string, string> LeftValues,
        ContentId? Right,
        IReadOnlyDictionary<string, string> RightValues,
        bool Allowed);
}
