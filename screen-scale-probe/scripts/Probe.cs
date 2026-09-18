using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;

namespace ScreenScaleProbe;

/// <summary>
/// The screen scale probe of D-621. It draws one mock frame of 1280 by 720 at four scale states,
/// it measures the apparent size of a sprite and of a line of body text, and it writes a report.
/// </summary>
public sealed partial class Probe : Node2D
{
    private const string Root = "res://";
    private const int ChromePadding = 6;

    /// <summary>The width of the chrome panel in characters of the body font.</summary>
    private const int ChromeColumns = 58;

    private ProbeOptions _options = null!;
    private MockFrame _frame = null!;
    private FontFile _body = null!;
    private ImageTexture _pixelCheck = null!;
    private ScreenFacts _facts = null!;
    private Vector2I _lastWindow;
    private int _state;
    private bool _chrome = true;
    private string? _worldPick;
    private string? _uiPick;
    private string _message = string.Empty;
    private bool _stopped;

    /// <inheritdoc/>
    public override void _Ready()
    {
        TextureFilter = TextureFilterEnum.Nearest;
        try
        {
            _options = ProbeOptions.Parse(OS.GetCmdlineUserArgs());
            _frame = MockFrame.Bake(Root);
            _body = LoadBody($"{Root}fonts/TerminusTTF.ttf");
            _pixelCheck = MakePixelCheck();
            _state = _options.StartState - 1;
            ApplyWindowMode();
            Refresh();
        }
        catch (Exception error)
        {
            _stopped = true;
            GD.PushError(error.Message);
            GD.PrintErr($"the probe stopped: {error.Message}");
            GD.PrintErr("flags: --screen=name --diagonal=inches --distance=cm [--windowed] [--state=1..4]");
            GetTree().Quit(1);
            return;
        }

        foreach (string note in _facts.Notes)
        {
            GD.Print($"note: {note}");
        }

        if (_options.ShotPath is string shot)
        {
            SaveShot(shot);
        }
    }

    /// <inheritdoc/>
    public override void _Process(double delta)
    {
        if (_stopped)
        {
            return;
        }

        if (DisplayServer.WindowGetSize() != _lastWindow)
        {
            Refresh();
        }
    }

    /// <inheritdoc/>
    public override void _UnhandledInput(InputEvent @event)
    {
        if (_stopped || @event is not InputEventKey key || !key.Pressed || key.Echo)
        {
            return;
        }

        switch (key.Keycode)
        {
            case Key.Space:
            case Key.Right:
                Step(1);
                break;
            case Key.Left:
                Step(-1);
                break;
            case Key.W:
                _worldPick = Current().Name;
                _message = $"the world pick is {_worldPick}";
                QueueRedraw();
                break;
            case Key.U:
                _uiPick = Current().Name;
                _message = $"the UI pick is {_uiPick}";
                QueueRedraw();
                break;
            case Key.H:
                _chrome = !_chrome;
                QueueRedraw();
                break;
            case Key.R:
                WriteReport();
                break;
            case Key.Escape:
            case Key.Q:
                WriteReport();
                GetTree().Quit();
                break;
            default:
                break;
        }
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        if (_stopped)
        {
            return;
        }

        ScaleState state = Current();
        Rect2I frame = _facts.FrameRect();
        DrawRect(new Rect2(Vector2.Zero, _facts.WindowPixels), Colors.Black);
        DrawWorld(frame, state);
        Rect2I partyRow = DrawPartyRow(frame, state);
        DrawDialogue(frame, state);
        if (_chrome)
        {
            DrawChrome(partyRow, state);
        }
    }

    private static FontFile LoadBody(string path)
    {
        var font = GD.Load<FontFile>(path);
        if (font is null)
        {
            throw new InvalidOperationException($"cannot load the body font at {path}");
        }

        font.Antialiasing = TextServer.FontAntialiasing.None;
        font.SubpixelPositioning = TextServer.SubpixelPositioning.Disabled;
        font.Hinting = TextServer.Hinting.None;
        font.MultichannelSignedDistanceField = false;
        font.GenerateMipmaps = false;
        return font;
    }

    private static ImageTexture MakePixelCheck()
    {
        const int size = 32;
        Image image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                image.SetPixel(x, y, (x + y) % 2 == 0 ? Colors.White : Colors.Black);
            }
        }

        return ImageTexture.CreateFromImage(image);
    }

    private ScaleState Current() => ScaleState.All[_state];

    private void Step(int direction)
    {
        int count = ScaleState.All.Length;
        _state = ((_state + direction) % count + count) % count;
        _message = string.Empty;
        QueueRedraw();
    }

    private void ApplyWindowMode()
    {
        if (_options.Windowed)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
            DisplayServer.WindowSetSize(new Vector2I(ScreenFacts.FrameWidth, ScreenFacts.FrameHeight));
            return;
        }

        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
    }

    private void Refresh()
    {
        _lastWindow = DisplayServer.WindowGetSize();
        _facts = ScreenFacts.Measure(_options);
        QueueRedraw();
    }

    private void DrawWorld(Rect2I frame, ScaleState state)
    {
        Rect2I region = _frame.VisibleRegion(state);
        DrawTextureRectRegion(_frame.World, frame, region);
    }

    private Rect2I DrawPartyRow(Rect2I frame, ScaleState state)
    {
        int unit = _facts.Fit * state.Ui;
        int fontSize = ScreenFacts.FontPixels * unit;
        float width = _body.GetStringSize(ProbeText.PartyRow, HorizontalAlignment.Left, -1, fontSize).X;
        var box = new Rect2I(
            frame.Position + new Vector2I(8 * unit, 8 * unit),
            new Vector2I((int)width + (8 * unit), ScreenFacts.FontPixels * unit + (6 * unit)));
        DrawPanel(box, unit);
        DrawLine(box.Position + new Vector2I(4 * unit, 3 * unit), ProbeText.PartyRow, fontSize, Colors.White);
        return box;
    }

    private void DrawDialogue(Rect2I frame, ScaleState state)
    {
        int unit = _facts.Fit * state.Ui;
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
        DrawLine(pen, ProbeText.Speaker, fontSize, new Color("e6c85a"));
        pen.Y += line + gap;
        foreach (string text in ProbeText.Dialogue)
        {
            DrawLine(pen, text, fontSize, Colors.White);
            pen.Y += line + gap;
        }
    }

    private void DrawPanel(Rect2I box, int unit)
    {
        DrawRect(box, new Color("1a1823"));
        DrawRect(box, new Color("d6d0d8"), false, unit);
    }

    private void DrawLine(Vector2I topLeft, string text, int fontSize, Color color)
    {
        float baseline = _body.GetAscent(fontSize);
        DrawString(_body, new Vector2(topLeft.X, topLeft.Y + baseline), text,
            HorizontalAlignment.Left, -1, fontSize, color);
    }

    private void DrawChrome(Rect2I partyRow, ScaleState state)
    {
        int unit = _facts.Fit;
        int fontSize = ScreenFacts.FontPixels * unit;
        int line = (ScreenFacts.FontPixels + 4) * unit;
        List<string> rows = Wrap(ChromeRows(state), ChromeColumns);
        int pad = ChromePadding * unit;
        int rulerHeight = _facts.MmPerPixel is null ? 0 : 26 * unit;
        int checkHeight = Math.Max(_pixelCheck.GetHeight(), ScreenFacts.FontPixels * unit) + (6 * unit);
        var box = new Rect2I(
            new Vector2I(partyRow.Position.X, partyRow.End.Y + (8 * unit)),
            new Vector2I((ChromeColumns * fontSize / 2) + (pad * 2),
                (rows.Count * line) + (pad * 2) + rulerHeight + checkHeight));
        DrawPanel(box, unit);

        var pen = box.Position + new Vector2I(pad, pad);
        foreach (string row in rows)
        {
            Color color = row.StartsWith("note", StringComparison.Ordinal) ? new Color("e8734f") : Colors.White;
            DrawLine(pen, row, fontSize, color);
            pen.Y += line;
        }

        if (_facts.MmPerPixel is double millimeters)
        {
            DrawRuler(pen, millimeters, unit, fontSize);
            pen.Y += rulerHeight;
        }

        DrawTextureRect(_pixelCheck, new Rect2(pen.X, pen.Y, _pixelCheck.GetWidth(), _pixelCheck.GetHeight()), false);
        DrawLine(new Vector2I(pen.X + _pixelCheck.GetWidth() + (6 * unit), pen.Y),
            "one device pixel each", fontSize, Colors.White);
    }

    /// <summary>Breaks each row at a space, so no row of the chrome passes the width of its panel.</summary>
    private static List<string> Wrap(List<string> rows, int columns)
    {
        var wrapped = new List<string>();
        foreach (string row in rows)
        {
            string rest = row;
            while (rest.Length > columns)
            {
                int cut = rest.LastIndexOf(' ', columns);
                if (cut <= 0)
                {
                    cut = columns;
                }

                wrapped.Add(rest[..cut]);
                rest = rest[cut..].TrimStart();
            }

            wrapped.Add(rest);
        }

        return wrapped;
    }

    private List<string> ChromeRows(ScaleState state)
    {
        double? spriteMm = _facts.SpriteMm(state);
        double? glyphMm = _facts.GlyphMm(state);
        var rows = new List<string>
        {
            $"STATE {_state + 1} of {ScaleState.All.Length}: {state.Name}",
            $"screen {_facts.ScreenPixels.X}x{_facts.ScreenPixels.Y}  window {_facts.WindowPixels.X}x{_facts.WindowPixels.Y}  fit {_facts.Fit}x",
            $"tiles {Format(ScreenFacts.TilesAcross(state), "0.##")} x {Format(ScreenFacts.TilesDown(state), "0.##")}  device px per art px: world {_facts.Fit * state.World}, UI {_facts.Fit * state.Ui}",
            $"sprite of 32 px: {Millimeters(spriteMm)}, {Arcminutes(spriteMm)}",
            $"body line of 16 px: {Millimeters(glyphMm)}, {Arcminutes(glyphMm)}",
            $"the dialogue box holds {DialogueColumns(state)} characters in one line",
            $"picks: world {_worldPick ?? "none"}, UI {_uiPick ?? "none"}",
            "space next | W world | U UI | H hide | R report | Esc quit",
        };

        if (_message.Length > 0)
        {
            rows.Add(_message);
        }

        foreach (string note in _facts.Notes)
        {
            rows.Add($"note {note}");
        }

        return rows;
    }

    /// <summary>The count of characters that one line of the dialogue box holds at a UI scale.</summary>
    private int DialogueColumns(ScaleState state)
    {
        int unit = _facts.Fit * state.Ui;
        int advance = ScreenFacts.FontPixels * unit / 2;
        return (_facts.FrameRect().Size.X - (28 * unit)) / advance;
    }

    private void DrawRuler(Vector2I topLeft, double millimetersForOnePixel, int unit, int fontSize)
    {
        int room = (ChromeColumns * fontSize / 2) - (4 * unit);
        int millimeters = RulerLength(millimetersForOnePixel, room);
        int length = (int)Math.Round(millimeters / millimetersForOnePixel);
        int barY = topLeft.Y + (10 * unit);
        DrawRect(new Rect2I(topLeft.X, barY, length, unit), Colors.White);
        for (int mark = 0; mark <= millimeters; mark += millimeters / 10)
        {
            int x = topLeft.X + (int)Math.Round(mark / millimetersForOnePixel);
            int height = mark % (millimeters / 2) == 0 ? 8 * unit : 4 * unit;
            DrawRect(new Rect2I(x, barY - height, unit, height), Colors.White);
        }

        DrawLine(new Vector2I(topLeft.X, barY + (2 * unit)), $"{millimeters} mm on the glass", fontSize, Colors.White);
    }

    /// <summary>The longest ruler of 100, 50, or 20 millimeters that fits the chrome panel.</summary>
    private static int RulerLength(double millimetersForOnePixel, int room)
    {
        foreach (int millimeters in new[] { 100, 50, 20 })
        {
            if (millimeters / millimetersForOnePixel <= room)
            {
                return millimeters;
            }
        }

        return 20;
    }

    private void WriteReport()
    {
        string folder = _options.ReportDir ?? $"{OS.GetExecutablePath().GetBaseDir()}/reports";
        if (TryWriteReport(folder))
        {
            return;
        }

        // A folder beside the build can refuse a write, such as a folder on a read-only mount.
        // The fallback keeps the numbers of the run, and the message names the path (T-2).
        if (!TryWriteReport("user://"))
        {
            GD.PrintErr("the probe wrote no report. Read the numbers from the panel of the frame.");
        }

        QueueRedraw();
    }

    private bool TryWriteReport(string folder)
    {
        try
        {
            string path = ProbeReport.Write(folder, _options, _facts, _worldPick, _uiPick);
            _message = $"the report is at {ProjectSettings.GlobalizePath(path)}";
            GD.Print(_message);
            QueueRedraw();
            return true;
        }
        catch (Exception error)
        {
            _message = $"the report failed at {folder}: {error.Message}";
            GD.PrintErr(_message);
            return false;
        }
    }

    private async void SaveShot(string path)
    {
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        Image image = GetViewport().GetTexture().GetImage();
        Error saved = image.SavePng(path);
        if (saved != Error.Ok)
        {
            GD.PrintErr($"cannot save the shot at {path}: {saved}");
            GetTree().Quit(1);
            return;
        }

        GD.Print($"the shot is at {path}");
        WriteReport();
        GetTree().Quit();
    }

    private string Millimeters(double? value) =>
        value is double number ? $"{Format(number, "0.00")} mm" : "no diagonal";

    private string Arcminutes(double? millimeters)
    {
        double? value = ScreenFacts.Arcminutes(millimeters, _options.DistanceCm);
        return value is double number ? $"{Format(number, "0.#")} arcminutes" : "no distance";
    }

    private static string Format(double value, string pattern) =>
        value.ToString(pattern, CultureInfo.InvariantCulture);
}
