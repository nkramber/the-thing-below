using System;
using Godot;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The wipe on the map: the screen drains to dark, and one terse line appears, as after the last
/// blow of a battle (D-225, D-397). The run then reloads the newer save.
/// </summary>
/// <remarks>
/// A trap, the bad air, or poison can down each character who fights on the map (D-1230, D-1234,
/// D-1235). The world stands still from that tick, and the view counts the ticks of the loop, so
/// it reads no clock of the engine (T-7).
/// </remarks>
public sealed class MapWipeView
{
    /// <summary>The count of ticks from the wipe to the reload. The loop runs 60 ticks a second.</summary>
    public const int HoldTicks = 150;

    /// <summary>The count of ticks of the drain to dark.</summary>
    public const int DrainTicks = 60;

    /// <summary>The count of ticks after the drain in which the line fades in.</summary>
    public const int LineTicks = 30;

    /// <summary>The id of the line of the wipe on the map (D-225).</summary>
    public static readonly ContentId LineId = ContentId.Parse("map.wiped", StringTable.Path, "map wipe");

    private readonly Control layer;
    private readonly ColorRect dark;
    private readonly Label line;

    /// <summary>Builds the view over the frame, hidden.</summary>
    /// <param name="frame">The frame, whose UI layer takes the view.</param>
    /// <param name="ui">The theme and the text helper.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public MapWipeView(FrameRoot frame, UiBase ui)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(ui);

        this.layer = new Control
        {
            Size = new Vector2(ScreenFit.FrameWidth, ScreenFit.FrameHeight),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Theme = ui.Theme.Theme,
            Visible = false,
        };
        frame.Layer.AddChild(this.layer);

        this.dark = new ColorRect
        {
            Color = ui.Theme.ColorOf("panel_shadow"),
            Size = new Vector2(ScreenFit.FrameWidth, ScreenFit.FrameHeight),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        this.layer.AddChild(this.dark);

        int height = MenuLayout.LineOf(ui.Theme.BodySize);
        this.line = MenuNodes.Line(this.layer, 0, (ScreenFit.FrameHeight - height) / 2, ScreenFit.FrameWidth, height);
        this.line.HorizontalAlignment = HorizontalAlignment.Center;
        ui.Text.Put(this.line, LineId);
    }

    /// <summary>Draws one frame of the view.</summary>
    /// <param name="ticks">The count of ticks since the wipe, from 0.</param>
    /// <exception cref="ArgumentOutOfRangeException">The count is below zero (T-2).</exception>
    public void Show(int ticks)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(ticks);

        int drained = Math.Min(ticks, DrainTicks);
        int shown = Math.Clamp(ticks - DrainTicks, 0, LineTicks);
        this.dark.Modulate = new Color(1f, 1f, 1f, drained / (float)DrainTicks);
        this.line.Modulate = new Color(1f, 1f, 1f, shown / (float)LineTicks);
        this.layer.Visible = true;
    }
}
