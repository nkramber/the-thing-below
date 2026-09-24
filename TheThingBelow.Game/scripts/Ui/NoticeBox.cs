using System;
using Godot;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The notice box at the top edge of the frame: it slides in, types out its line, holds, and
/// fades, and play keeps going (D-221, D-994).
/// </summary>
/// <remarks>
/// <see cref="NoticeQueue"/> gives each frame of the notice box from the ticks of the world, so the
/// notice box reads no clock of the engine (T-7). The notice box hides under a menu, and the queue waits
/// with the world (D-995). The line holds its layout as it types, so no word moves (D-709).
/// </remarks>
public sealed class NoticeBox
{
    private readonly UiBase ui;
    private readonly Control layer;
    private readonly Label line;
    private readonly FrameBox box;
    private ContentId? shown;

    /// <summary>Builds the notice box over the frame, hidden.</summary>
    /// <param name="frame">The frame, whose UI layer takes the notice box.</param>
    /// <param name="ui">The atlas, the theme, and the text helper.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public NoticeBox(FrameRoot frame, UiBase ui)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(ui);

        this.ui = ui;
        this.box = MenuLayout.NoticePlace(ui.Theme.BodySize);
        this.layer = new Control
        {
            Position = new Vector2(this.box.X, this.box.Y),
            Size = new Vector2(this.box.Width, this.box.Height),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Theme = ui.Theme.Theme,
            Visible = false,
        };
        frame.Layer.AddChild(this.layer);
        MenuNodes.Panel(this.layer, new FrameBox(0, 0, this.box.Width, this.box.Height));

        this.line = MenuNodes.Line(
            this.layer, MenuLayout.NoticePad, MenuLayout.NoticePad, this.box.Width - (MenuLayout.NoticePad * 2), MenuLayout.LineOf(ui.Theme.BodySize));
        this.line.VisibleCharactersBehavior = TextServer.VisibleCharactersBehavior.CharsAfterShaping;
    }

    /// <summary>Draws one frame of the notice box.</summary>
    /// <param name="frame">What the queue shows at this tick of the world, or no value when no notice shows.</param>
    /// <param name="underMenu">True while a menu is open, which hides the notice box (D-995).</param>
    public void Show(NoticeFrame? frame, bool underMenu)
    {
        if (frame is null || underMenu)
        {
            this.layer.Visible = false;
            return;
        }

        if (this.shown is null || string.CompareOrdinal(this.shown.Value, frame.Notice.Value) != 0)
        {
            this.ui.Text.Put(this.line, frame.Notice);
            this.shown = frame.Notice;
        }

        this.line.VisibleCharacters = frame.Characters;
        int travel = this.box.Y + this.box.Height;
        this.layer.Position = new Vector2(this.box.X, this.box.Y - (frame.Hidden * travel / 1000));
        this.layer.Modulate = new Color(1f, 1f, 1f, frame.Opacity / 1000f);
        this.layer.Visible = true;
    }

    /// <summary>Removes every node of the notice box.</summary>
    public void Free() => this.layer.QueueFree();
}
