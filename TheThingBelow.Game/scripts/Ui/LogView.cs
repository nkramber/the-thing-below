using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The notice log window: the entries of the log, newest first (D-984, D-987). Up and down,
/// and the wheel of the mouse, scroll the page one line (D-872).
/// </summary>
/// <remarks>
/// The window reads the log when it opens. A menu pauses the world, and no rule posts a notice
/// while the world waits, so the log never changes under the window (D-162, D-995).
/// </remarks>
public sealed class LogView : IMenuView
{
    private readonly UiBase ui;
    private readonly Control layer;
    private readonly List<Label> lines = [];
    private readonly Label empty;

    /// <summary>Builds the log window beside the main list, at the newest entry.</summary>
    /// <param name="frame">The frame, whose UI layer takes the window.</param>
    /// <param name="ui">The atlas, the theme, and the text helper.</param>
    /// <param name="state">The state of the run, which holds the log.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public LogView(FrameRoot frame, UiBase ui, RunState state)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(ui);
        ArgumentNullException.ThrowIfNull(state);

        this.ui = ui;
        this.layer = MenuNodes.Layer(frame, ui);

        int body = ui.Theme.BodySize;
        FrameBox box = MenuLayout.TaskBox();
        MenuNodes.Panel(this.layer, box);
        MenuNodes.Title(this.layer, ui, box, ContentId.Parse("menu.log", StringTable.Path, nameof(LogView)));

        int shown = MenuLayout.LogLines(body, ui.Theme.TitleSize);
        this.Page = new LogPage(state.NoticeLog.Entries, shown);
        int line = MenuLayout.LineOf(body);
        int left = box.X + MenuLayout.Pad;
        int width = box.Width - (MenuLayout.Pad * 2);
        int top = MenuLayout.FirstLineTop(body, ui.Theme.TitleSize);
        for (int row = 0; row < shown; row += 1)
        {
            this.lines.Add(MenuNodes.Line(this.layer, left, top + (row * line), width, line));
        }

        this.empty = MenuNodes.Line(this.layer, left, top, width, line);
        ui.Text.Put(this.empty, ContentId.Parse("menu.log_empty", StringTable.Path, nameof(LogView)));
        MenuNodes.Paint(this.empty, ui.Theme.ColorOf("text_dim"));

        this.Show();
    }

    /// <summary>The entries and the part that the window shows.</summary>
    public LogPage Page { get; }

    /// <inheritdoc/>
    public ViewOutcome Read(InputEvent signal, ScreenFit fit)
    {
        ArgumentNullException.ThrowIfNull(signal);
        ArgumentNullException.ThrowIfNull(fit);

        ViewOutcome outcome = ViewOutcome.Stay;
        if (signal is InputEventMouseButton wheel && wheel.Pressed && wheel.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
        {
            this.Page.Scroll(wheel.ButtonIndex == MouseButton.WheelUp ? -1 : 1);
        }
        else if (signal.IsActionPressed("ui_up"))
        {
            this.Page.Scroll(-1);
        }
        else if (signal.IsActionPressed("ui_down"))
        {
            this.Page.Scroll(1);
        }
        else if (signal.IsActionPressed("ui_cancel"))
        {
            outcome = ViewOutcome.Back;
        }

        this.Show();
        return outcome;
    }

    /// <inheritdoc/>
    public void Show()
    {
        IReadOnlyList<ContentId> shown = this.Page.Shown();
        this.empty.Visible = this.Page.Lines.Count == 0;
        for (int row = 0; row < this.lines.Count; row += 1)
        {
            Label line = this.lines[row];
            line.Visible = row < shown.Count;
            if (line.Visible)
            {
                this.ui.Text.Put(line, shown[row]);
            }
        }
    }

    /// <inheritdoc/>
    public void Free() => this.layer.QueueFree();
}
