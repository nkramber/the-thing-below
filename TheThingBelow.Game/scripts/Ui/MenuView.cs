using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>What one input event did to a window of the menu stack (D-211).</summary>
public enum ViewOutcome
{
    /// <summary>The window stays open.</summary>
    Stay,

    /// <summary>The player pressed back, and the window closes (D-211).</summary>
    Back,

    /// <summary>The player chose the thing under the cursor.</summary>
    Chose,
}

/// <summary>One window of the menu stack on screen (D-211). <see cref="MenuHost"/> holds each one.</summary>
/// <remarks>
/// The `ui_*` actions and the mouse drive every window, as they drive the settings screen
/// (D-862, D-872). A window takes a read of the state of the run on each frame, so it shows a
/// change that a tick made, such as a row that the party window moved (D-558).
/// </remarks>
public interface IMenuView
{
    /// <summary>Gives one input event to the window.</summary>
    /// <param name="signal">The event.</param>
    /// <param name="fit">The fit of the frame, which turns a mouse point into a frame point (D-872).</param>
    /// <returns>What the event did.</returns>
    ViewOutcome Read(InputEvent signal, ScreenFit fit);

    /// <summary>Draws the window from the state of the run now.</summary>
    void Show();

    /// <summary>Removes every node of the window.</summary>
    void Free();
}

/// <summary>The shared parts of the menu windows: the layer, the panel, the title, and the paint of a line (D-211, D-220).</summary>
public static class MenuNodes
{
    /// <summary>Builds the layer of one window over the frame, with the theme of the UI.</summary>
    /// <param name="frame">The frame, whose UI layer takes the window.</param>
    /// <param name="ui">The atlas, the theme, and the text helper.</param>
    /// <returns>The layer, which the window frees when it closes.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static Control Layer(FrameRoot frame, UiBase ui)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(ui);

        var layer = new Control
        {
            Position = Vector2.Zero,
            Size = new Vector2(ScreenFit.FrameWidth, ScreenFit.FrameHeight),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Theme = ui.Theme.Theme,
        };
        frame.Layer.AddChild(layer);
        return layer;
    }

    /// <summary>Adds one panel with the window frame of the style file (D-220).</summary>
    /// <param name="layer">The layer of the window.</param>
    /// <param name="box">The place of the panel.</param>
    public static void Panel(Control layer, FrameBox box)
    {
        ArgumentNullException.ThrowIfNull(layer);

        layer.AddChild(new Panel
        {
            Position = new Vector2(box.X, box.Y),
            Size = new Vector2(box.Width, box.Height),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        });
    }

    /// <summary>Adds the title of a task window at the top left of its box.</summary>
    /// <param name="layer">The layer of the window.</param>
    /// <param name="ui">The text helper, which puts the string.</param>
    /// <param name="box">The place of the window.</param>
    /// <param name="id">The string id of the title (G-7).</param>
    public static void Title(Control layer, UiBase ui, FrameBox box, ContentId id)
    {
        ArgumentNullException.ThrowIfNull(layer);
        ArgumentNullException.ThrowIfNull(ui);

        var title = new Label
        {
            Position = new Vector2(box.X + MenuLayout.Pad, box.Y + MenuLayout.Pad),
            ThemeTypeVariation = UiTheme.TitleVariation,
        };
        ui.Text.Put(title, id);
        layer.AddChild(title);
    }

    /// <summary>Adds one line of text.</summary>
    /// <param name="layer">The layer of the window.</param>
    /// <param name="left">The left edge, in frame pixels.</param>
    /// <param name="top">The top edge, in frame pixels.</param>
    /// <param name="width">The width, in frame pixels.</param>
    /// <param name="line">The height of a line, in frame pixels.</param>
    /// <returns>The label, with no text yet.</returns>
    public static Label Line(Control layer, int left, int top, int width, int line)
    {
        ArgumentNullException.ThrowIfNull(layer);

        var label = new Label { Position = new Vector2(left, top), Size = new Vector2(width, line) };
        layer.AddChild(label);
        return label;
    }

    /// <summary>Paints a line as the cursor line, as a dim line, or as a plain line.</summary>
    /// <param name="label">The line.</param>
    /// <param name="color">The color of the role, or no value for the color of the theme.</param>
    public static void Paint(Label label, Color? color)
    {
        ArgumentNullException.ThrowIfNull(label);

        if (color is Color painted)
        {
            label.AddThemeColorOverride("font_color", painted);
        }
        else
        {
            label.RemoveThemeColorOverride("font_color");
        }
    }

    /// <summary>Gives the place of the line under a mouse point, or no value (D-872).</summary>
    /// <param name="lines">The lines, in the order of the list.</param>
    /// <param name="mouse">The mouse event.</param>
    /// <param name="fit">The fit of the frame, which turns the point of the window into a frame point.</param>
    /// <returns>The place of the line, or no value outside every line.</returns>
    public static int? LineUnder(IReadOnlyList<Label> lines, InputEventMouse mouse, ScreenFit fit)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(mouse);
        ArgumentNullException.ThrowIfNull(fit);

        if (fit.ToFrame((int)mouse.Position.X, (int)mouse.Position.Y) is not (int x, int y))
        {
            return null;
        }

        for (int index = 0; index < lines.Count; index += 1)
        {
            Label line = lines[index];
            if (x >= line.Position.X && x < line.Position.X + line.Size.X && y >= line.Position.Y && y < line.Position.Y + line.Size.Y)
            {
                return index;
            }
        }

        return null;
    }

    /// <summary>Tells whether a mouse event is a press of the left button (D-872).</summary>
    /// <param name="mouse">The mouse event.</param>
    /// <returns>True for a press of the left button.</returns>
    public static bool IsClick(InputEventMouse mouse) =>
        mouse is InputEventMouseButton click && click.Pressed && click.ButtonIndex == MouseButton.Left;
}
