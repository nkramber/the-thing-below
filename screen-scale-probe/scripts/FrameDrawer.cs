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
    private ScaleState _state = ScaleState.All[0];

    /// <summary>Gives the drawer the baked world and the body font, before the first draw.</summary>
    public void Configure(MockFrame frame, FontFile body)
    {
        TextureFilter = TextureFilterEnum.Nearest;
        _frame = frame;
        _body = body;
    }

    /// <summary>Sets the scale state that the next draw shows.</summary>
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
        DrawPartyRow(frame);
        DrawDialogue(frame);
    }

    /// <summary>The height of the party row and its margin, in frame pixels.</summary>
    public static int PartyRowBand(ScaleState state) => 30 * state.Ui;

    private void DrawWorld(Rect2I frame)
    {
        Rect2I region = _frame.VisibleRegion(_state);
        DrawTextureRectRegion(_frame.World, frame, region);
    }

    private void DrawPartyRow(Rect2I frame)
    {
        int unit = _state.Ui;
        int fontSize = ScreenFacts.FontPixels * unit;
        float width = _body.GetStringSize(ProbeText.PartyRow, HorizontalAlignment.Left, -1, fontSize).X;
        var box = new Rect2I(
            frame.Position + new Vector2I(8 * unit, 8 * unit),
            new Vector2I((int)width + (8 * unit), ScreenFacts.FontPixels * unit + (6 * unit)));
        DrawPanel(box, unit);
        DrawText(box.Position + new Vector2I(4 * unit, 3 * unit), ProbeText.PartyRow, fontSize, Colors.White);
    }

    private void DrawDialogue(Rect2I frame)
    {
        int unit = _state.Ui;
        int fontSize = ScreenFacts.FontPixels * unit;
        int line = ScreenFacts.FontPixels * unit;
        int gap = 4 * unit;
        int rows = ProbeText.Dialogue.Length + 1;
        int height = (rows * line) + ((rows - 1) * gap) + (12 * unit);
        var box = new Rect2I(
            new Vector2I(frame.Position.X + (8 * unit), frame.End.Y - (8 * unit) - height),
            new Vector2I(frame.Size.X - (16 * unit), height));
        DrawPanel(box, unit);

        var pen = box.Position + new Vector2I(6 * unit, 6 * unit);
        DrawText(pen, ProbeText.Speaker, fontSize, new Color("e6c85a"));
        pen.Y += line + gap;
        foreach (string text in ProbeText.Dialogue)
        {
            DrawText(pen, text, fontSize, Colors.White);
            pen.Y += line + gap;
        }
    }

    private void DrawPanel(Rect2I box, int unit)
    {
        DrawRect(box, new Color("1a1823"));
        DrawRect(box, new Color("d6d0d8"), false, unit);
    }

    private void DrawText(Vector2I topLeft, string text, int fontSize, Color color)
    {
        float baseline = _body.GetAscent(fontSize);
        DrawString(_body, new Vector2(topLeft.X, topLeft.Y + baseline), text,
            HorizontalAlignment.Left, -1, fontSize, color);
    }
}
