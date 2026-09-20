using System;
using System.Collections.Generic;
using Godot;

namespace ScreenScaleProbe;

/// <summary>
/// Draws the mock frame into a viewport of 1280 by 720, at one viewport pixel for each frame
/// pixel. The presenter then puts that picture on the screen in one of the two fit modes.
/// </summary>
public sealed partial class FrameDrawer : Node2D
{
    private MockFrame _frame = null!;
    private FontFile _body = null!;
    private FontFile _title = null!;
    private ScaleState _state = ScaleState.All[0];

    /// <summary>Gives the drawer the baked world and the two fonts, before the first draw.</summary>
    public void Configure(MockFrame frame, FontFile body, FontFile title)
    {
        TextureFilter = TextureFilterEnum.Nearest;
        _frame = frame;
        _body = body;
        _title = title;
    }

    /// <summary>Sets the combination that the next draw shows.</summary>
    public void Show(ScaleState state)
    {
        _state = state;
        QueueRedraw();
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        var frame = new Rect2I(0, 0, ScreenFacts.FrameWidth, ScreenFacts.FrameHeight);
        DrawRect(frame, Colors.Black);
        DrawWorld(frame);
        DrawTitle(frame);
        DrawPartyRow(frame);
        DrawDialogue(frame);
    }

    /// <summary>The height of the title band, the party row, and their margins, in frame pixels.</summary>
    public static int PartyRowBand(ScaleState state)
    {
        int unit = state.LayoutUnit;
        return (8 * unit) + state.TitlePixels + (6 * unit) + state.BodyPixels + (14 * unit);
    }

    private void DrawWorld(Rect2I frame)
    {
        Rect2I region = _frame.VisibleRegion(_state);
        DrawTextureRectRegion(_frame.World, frame, region);
    }

    private void DrawTitle(Rect2I frame)
    {
        int unit = _state.LayoutUnit;
        var pen = frame.Position + new Vector2I(8 * unit, 8 * unit);
        DrawText(pen, ProbeText.Title, _title, _state.TitleNative, _state.TitleUnit, new Color("e6c85a"));
    }

    private void DrawPartyRow(Rect2I frame)
    {
        int unit = _state.LayoutUnit;
        int top = (8 * unit) + _state.TitlePixels + (6 * unit);

        // The row takes the members that fit, so the panel never passes the edge of the frame.
        int columns = (frame.Size.X - (24 * unit)) / (_state.BodyPixels / 2);
        string text = ProbeText.PartyRow(columns);
        float width = TextWidth(text, _body, _state.BodyNative, _state.BodyUnit);
        var box = new Rect2I(
            frame.Position + new Vector2I(8 * unit, top),
            new Vector2I((int)width + (8 * unit), _state.BodyPixels + (6 * unit)));
        DrawPanel(box);
        DrawText(box.Position + new Vector2I(4 * unit, 3 * unit), text,
            _body, _state.BodyNative, _state.BodyUnit, Colors.White);
    }

    private void DrawDialogue(Rect2I frame)
    {
        int unit = _state.LayoutUnit;
        int line = _state.BodyPixels;
        int gap = 4 * unit;
        // The box keeps the three lines of the game-text-style limit, whatever the body size.
        // A sample that needs more lines does not grow the box. The chrome reports the overflow.
        List<string> wrapped = ProbeText.WrappedDialogue(ScreenFacts.DialogueColumns(_state));
        List<string> dialogue = wrapped.GetRange(0, Math.Min(ProbeText.DialogueLines, wrapped.Count));
        int rows = dialogue.Count + 1;
        int height = (rows * line) + ((rows - 1) * gap) + (12 * unit);
        var box = new Rect2I(
            new Vector2I(frame.Position.X + (8 * unit), frame.End.Y - (8 * unit) - height),
            new Vector2I(frame.Size.X - (16 * unit), height));
        DrawPanel(box);

        var pen = box.Position + new Vector2I(6 * unit, 6 * unit);
        DrawText(pen, ProbeText.Speaker, _body, _state.BodyNative, _state.BodyUnit, new Color("e6c85a"));
        pen.Y += line + gap;
        foreach (string text in dialogue)
        {
            DrawText(pen, text, _body, _state.BodyNative, _state.BodyUnit, Colors.White);
            pen.Y += line + gap;
        }
    }

    private void DrawPanel(Rect2I box)
    {
        DrawRect(box, new Color("1a1823"));
        DrawRect(box, new Color("d6d0d8"), false, _state.BorderPixels);
    }

    /// <summary>The width of a line in frame pixels, at the strike and the scale of the state.</summary>
    private static float TextWidth(string text, FontFile font, int native, int unit) =>
        font.GetStringSize(text, HorizontalAlignment.Left, -1, native).X * unit;

    /// <summary>
    /// Draws one line at a bitmap strike of the font, under a whole-number scale. The size that
    /// reaches the rasterizer is the strike, never the strike times the scale, because a size
    /// with no strike falls back to the traced outline and loses the square pixel (F-49, D-230).
    /// </summary>
    private void DrawText(Vector2I topLeft, string text, FontFile font, int native, int unit, Color color)
    {
        float baseline = font.GetAscent(native);
        DrawSetTransform(topLeft, 0, new Vector2(unit, unit));
        DrawString(font, new Vector2(0, baseline), text, HorizontalAlignment.Left, -1, native, color);
        DrawSetTransform(Vector2.Zero, 0, Vector2.One);
    }
}
