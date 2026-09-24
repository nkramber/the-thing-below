using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The main list on screen: one line for each entry, the cursor, and the dim entries of later
/// PRs (D-211, D-988, D-992).
/// </summary>
/// <remarks>
/// The list stands at the left edge of the frame, so the map stays visible to its right, and
/// each task window opens beside it (D-211). Every label takes its text from the string table
/// through the text helper (G-7, D-499).
/// </remarks>
public sealed class MainListView : IMenuView
{
    private readonly UiBase ui;
    private readonly Control layer;
    private readonly List<Label> lines = [];
    private readonly Color chosenColor;
    private readonly Color dimColor;

    /// <summary>Builds the main list over the frame, with the cursor on the first entry.</summary>
    /// <param name="frame">The frame, whose UI layer takes the list.</param>
    /// <param name="ui">The atlas, the theme, and the text helper.</param>
    /// <param name="list">The cursor, which the list keeps when the screen builds again.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public MainListView(FrameRoot frame, UiBase ui, MainList list)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(ui);
        ArgumentNullException.ThrowIfNull(list);

        this.ui = ui;
        this.List = list;
        this.chosenColor = ui.Theme.ColorOf("text_chosen");
        this.dimColor = ui.Theme.ColorOf("text_dim");
        this.layer = MenuNodes.Layer(frame, ui);

        int body = ui.Theme.BodySize;
        FrameBox box = MenuLayout.MainListBox(body);
        MenuNodes.Panel(this.layer, box);
        int line = MenuLayout.LineOf(body);
        for (int index = 0; index < MainList.Entries.Count; index += 1)
        {
            Label label = MenuNodes.Line(
                this.layer, box.X + MenuLayout.Pad, box.Y + MenuLayout.Pad + (index * line), box.Width - (MenuLayout.Pad * 2), line);
            ui.Text.Put(label, LabelOf(MainList.Entries[index]));
            this.lines.Add(label);
        }

        this.Show();
    }

    /// <summary>The entries and the cursor.</summary>
    public MainList List { get; }

    /// <summary>Gives the string id of the label of one entry (G-7).</summary>
    /// <param name="entry">The entry.</param>
    /// <returns>The id, such as `menu.party`.</returns>
    public static ContentId LabelOf(MenuEntry entry) => ContentId.Parse(
        entry switch
        {
            MenuEntry.Party => "menu.party",
            MenuEntry.Lessons => "menu.lessons",
            MenuEntry.Gear => "menu.gear",
            MenuEntry.Items => "menu.items",
            MenuEntry.Status => "menu.status",
            MenuEntry.Log => "menu.log",
            MenuEntry.Save => "menu.save",
            MenuEntry.Settings => "menu.settings",
            _ => throw new ArgumentOutOfRangeException(nameof(entry), entry, "The main list holds no such entry (T-2)."),
        },
        StringTable.Path,
        nameof(MainListView));

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
        for (int index = 0; index < this.lines.Count; index += 1)
        {
            Color? color = null;
            if (!MainList.IsLive(MainList.Entries[index]))
            {
                color = this.dimColor;
            }
            else if (index == this.List.Cursor)
            {
                color = this.chosenColor;
            }

            MenuNodes.Paint(this.lines[index], color);
        }
    }

    /// <inheritdoc/>
    public void Free() => this.layer.QueueFree();

    private ViewOutcome ReadEvent(InputEvent signal, ScreenFit fit)
    {
        if (signal is InputEventMouse mouse)
        {
            if (MenuNodes.LineUnder(this.lines, mouse, fit) is not int place || !this.List.Point(place))
            {
                return ViewOutcome.Stay;
            }

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
