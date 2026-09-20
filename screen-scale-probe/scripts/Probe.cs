using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;

namespace ScreenScaleProbe;

/// <summary>
/// The screen scale probe of D-621. It draws one mock frame of 1280 by 720 at four scale states
/// and in two fit modes, it measures the apparent size of a sprite and of a line of body text,
/// and it writes a report.
/// </summary>
/// <remarks>
/// The frame goes into a viewport of 1280 by 720, at one viewport pixel for each frame pixel. A
/// second viewport holds that picture at a whole-number scale, with the Nearest filter. The two
/// viewports are the two steps of D-573, and the presenter draws the second one to the screen.
/// </remarks>
public sealed partial class Probe : Node2D
{
    private const string Root = "res://";
    private const int ChromePadding = 6;

    /// <summary>The width of the chrome panel in characters of the body font.</summary>
    private const int ChromeColumns = 62;

    /// <summary>The count of squares across the native pixel check of the chrome panel.</summary>
    private const int CheckSquares = 16;

    private ProbeOptions _options = null!;
    private FontFile _body = null!;
    private FontFile _title = null!;
    private ScreenFacts _facts = null!;
    private SubViewport _frameView = null!;
    private SubViewport _stepView = null!;
    private TextureRect _stepRect = null!;
    private FrameDrawer _drawer = null!;
    private Vector2I _lastWindow;
    private int _state;
    private FitMode _fit;
    private bool _chrome = true;
    private string? _worldPick;
    private string? _fontPick;
    private string _message = string.Empty;
    private bool _stopped;

    /// <inheritdoc/>
    public override void _Ready()
    {
        TextureFilter = TextureFilterEnum.Nearest;
        try
        {
            _options = ProbeOptions.Parse(OS.GetCmdlineUserArgs());
            MockFrame frame = MockFrame.Bake(Root);
            _body = LoadFont($"{Root}fonts/TerminusTTF.ttf");
            _title = LoadFont($"{Root}fonts/TerminusTTF-Bold.ttf");
            _state = _options.StartState - 1;
            _fit = _options.StartFit;
            BuildViewports(frame);
            ApplyWindowMode();
            Refresh();
        }
        catch (Exception error)
        {
            _stopped = true;
            GD.PushError(error.Message);
            GD.PrintErr($"the probe stopped: {error.Message}");
            GD.PrintErr("flags: --screen=name --diagonal=inches --distance=cm"
                + $" [--windowed[=WxH]] [--state=1..{ScaleState.All.Length}] [--fit=whole|fill]");
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
        if (_stopped)
        {
            return;
        }

        // The Deck in desktop mode has no keyboard, so the buttons of the pad do the same work.
        switch (@event)
        {
            case InputEventKey key when key.Pressed && !key.Echo:
                ReadCommand(KeyCommand(key.Keycode));
                break;
            case InputEventJoypadButton button when button.Pressed:
                ReadCommand(ButtonCommand(button.ButtonIndex));
                break;
            default:
                break;
        }
    }

    private static ProbeCommand KeyCommand(Key code) => code switch
    {
        Key.Space or Key.Right => ProbeCommand.NextState,
        Key.Left => ProbeCommand.StateBefore,
        Key.W => ProbeCommand.WorldPick,
        Key.U => ProbeCommand.FontPick,
        Key.F => ProbeCommand.NextFitMode,
        Key.H => ProbeCommand.HidePanel,
        Key.R => ProbeCommand.Report,
        Key.Escape or Key.Q => ProbeCommand.ReportAndQuit,
        _ => ProbeCommand.None,
    };

    private static ProbeCommand ButtonCommand(JoyButton button) => button switch
    {
        JoyButton.A => ProbeCommand.NextState,
        JoyButton.B => ProbeCommand.StateBefore,
        JoyButton.X => ProbeCommand.WorldPick,
        JoyButton.Y => ProbeCommand.FontPick,
        JoyButton.RightShoulder => ProbeCommand.NextFitMode,
        JoyButton.Back => ProbeCommand.HidePanel,
        JoyButton.Start => ProbeCommand.ReportAndQuit,
        _ => ProbeCommand.None,
    };

    private void ReadCommand(ProbeCommand command)
    {
        switch (command)
        {
            case ProbeCommand.NextState:
                Step(1);
                break;
            case ProbeCommand.StateBefore:
                Step(-1);
                break;
            case ProbeCommand.NextFitMode:
                _fit = _fit == FitMode.Whole ? FitMode.Fill : FitMode.Whole;
                _message = $"the fit mode is {FitName(_fit)}";
                Present();
                break;
            case ProbeCommand.WorldPick:
                _worldPick = WorldPickName();
                _message = $"the world pick is {_worldPick}";
                QueueRedraw();
                break;
            case ProbeCommand.FontPick:
                _fontPick = TextPickName();
                _message = $"the text pick is {_fontPick}";
                QueueRedraw();
                break;
            case ProbeCommand.HidePanel:
                _chrome = !_chrome;
                QueueRedraw();
                break;
            case ProbeCommand.Report:
                WriteReport();
                break;
            case ProbeCommand.ReportAndQuit:
                WriteReport();
                GetTree().Quit();
                break;
            case ProbeCommand.None:
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

        DrawRect(new Rect2(Vector2.Zero, _facts.WindowPixels), Colors.Black);
        Rect2 frame = _facts.FrameRect(_fit);
        Texture2D picture = _fit == FitMode.Whole ? _frameView.GetTexture() : _stepView.GetTexture();
        DrawTextureRect(picture, frame, false);
        if (_chrome)
        {
            DrawChrome(frame);
        }
    }

    private void BuildViewports(MockFrame frame)
    {
        _frameView = new SubViewport
        {
            Size = new Vector2I(ScreenFacts.FrameWidth, ScreenFacts.FrameHeight),
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
            Disable3D = true,
        };
        _drawer = new FrameDrawer();
        _frameView.AddChild(_drawer);
        AddChild(_frameView);
        _drawer.Configure(frame, _body, _title);

        // The second viewport is the first step of D-573: a whole-number scale with Nearest.
        _stepView = new SubViewport
        {
            Size = new Vector2I(ScreenFacts.FrameWidth, ScreenFacts.FrameHeight),
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
            Disable3D = true,
        };
        _stepRect = new TextureRect
        {
            Texture = _frameView.GetTexture(),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.Scale,
            TextureFilter = TextureFilterEnum.Nearest,
        };
        _stepView.AddChild(_stepRect);
        AddChild(_stepView);
    }

    private static FontFile LoadFont(string path)
    {
        var font = GD.Load<FontFile>(path);
        if (font is null)
        {
            throw new InvalidOperationException($"cannot load the font at {path}");
        }

        font.Antialiasing = TextServer.FontAntialiasing.None;
        font.SubpixelPositioning = TextServer.SubpixelPositioning.Disabled;
        font.Hinting = TextServer.Hinting.None;
        font.MultichannelSignedDistanceField = false;
        font.GenerateMipmaps = false;
        return font;
    }

    private ScaleState Current() => ScaleState.All[_state];

    private static string FitName(FitMode mode) => mode == FitMode.Whole ? "whole" : "fill";

    private string WorldPickName() =>
        $"world {Current().WorldScale:0.#}x, fit mode {FitName(_fit)}";

    private string TextPickName() =>
        $"body {Current().BodyPixels}, title {Current().TitlePixels}";

    private void Step(int direction)
    {
        int count = ScaleState.All.Length;
        _state = ((_state + direction) % count + count) % count;
        _message = string.Empty;
        _drawer.Show(Current());
        QueueRedraw();
    }

    private void ApplyWindowMode()
    {
        if (_options.WindowedSize is Vector2I size)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
            DisplayServer.WindowSetSize(size);
            return;
        }

        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
    }

    private void Refresh()
    {
        _lastWindow = DisplayServer.WindowGetSize();
        _facts = ScreenFacts.Measure(_options);
        _drawer.Show(Current());
        Present();
    }

    /// <summary>Sizes the step viewport for this screen, and sets the filter of the last step.</summary>
    private void Present()
    {
        var size = new Vector2I(
            ScreenFacts.FrameWidth * _facts.NearestSteps,
            ScreenFacts.FrameHeight * _facts.NearestSteps);
        _stepView.Size = size;
        _stepRect.Size = size;

        // The last step of D-573 is a linear scale down. A whole-number fit needs no scale at all,
        // and the Nearest filter then keeps every pixel edge hard.
        TextureFilter = _fit == FitMode.Fill && !_facts.FitIsWhole
            ? TextureFilterEnum.Linear
            : TextureFilterEnum.Nearest;
        QueueRedraw();
    }

    private void DrawChrome(Rect2 frame)
    {
        int unit = _facts.WholeFit;
        int fontSize = ScreenFacts.FontPixels * unit;
        int line = (ScreenFacts.FontPixels + 4) * unit;
        List<string> rows = Wrap(ChromeRows(), ChromeColumns);
        int pad = ChromePadding * unit;
        int rulerHeight = _facts.MmPerPixel is null ? 0 : 26 * unit;
        int checkHeight = (CheckSquares * unit) + (6 * unit);
        int band = (int)(FrameDrawer.PartyRowBand(Current()) * _facts.Scale(_fit));
        var box = new Rect2I(
            new Vector2I((int)frame.Position.X + (8 * unit), (int)frame.Position.Y + band + (8 * unit)),
            new Vector2I((ChromeColumns * fontSize / 2) + (pad * 2),
                (rows.Count * line) + (pad * 2) + rulerHeight + checkHeight));
        DrawRect(box, new Color("1a1823"));
        DrawRect(box, new Color("d6d0d8"), false, unit);

        var pen = box.Position + new Vector2I(pad, pad);
        foreach (string row in rows)
        {
            Color color = row.StartsWith("note", StringComparison.Ordinal) ? new Color("e8734f") : Colors.White;
            DrawText(pen, row, fontSize, color);
            pen.Y += line;
        }

        if (_facts.MmPerPixel is double millimeters)
        {
            DrawRuler(pen, millimeters, unit, fontSize);
            pen.Y += rulerHeight;
        }

        DrawPixelCheck(pen, unit);
        DrawText(new Vector2I(pen.X + (CheckSquares * unit) + (6 * unit), pen.Y),
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

    private List<string> ChromeRows()
    {
        ScaleState state = Current();
        double? spriteMm = _facts.SpriteMm(state, _fit);
        double? glyphMm = _facts.GlyphMm(state, _fit);
        double? titleMm = _facts.TitleMm(state, _fit);
        var rows = new List<string>
        {
            $"STATE {_state + 1} of {ScaleState.All.Length}: {state.Name}",
            $"FIT MODE {FitName(_fit)}: the frame draws at {Format(_facts.Scale(_fit), "0.###")}x"
                + $"  (whole {_facts.WholeFit}x, D-573 {Format(_facts.FitFactor, "0.###")}x)",
            $"screen {_facts.ScreenPixels.X}x{_facts.ScreenPixels.Y}"
                + $"  window {_facts.WindowPixels.X}x{_facts.WindowPixels.Y}",
            $"tiles {Format(ScreenFacts.TilesAcross(state), "0.##")} x {Format(ScreenFacts.TilesDown(state), "0.##")}"
                + $"  device px per art px: world {Format(_facts.Scale(_fit) * state.WorldScale, "0.##")},"
                + $" glyph {Format(_facts.Scale(_fit) * state.BodyUnit, "0.##")}",
            $"sprite of 32 px: {Millimeters(spriteMm)}, {Arcminutes(spriteMm)}",
            $"body line of {state.BodyPixels} px: {Millimeters(glyphMm)}, {Arcminutes(glyphMm)}",
            $"title line of {state.TitlePixels} px: {Millimeters(titleMm)}, {Arcminutes(titleMm)}",
            $"the dialogue box holds {ScreenFacts.DialogueColumns(state)} characters, and the"
                + $" frame holds {ScreenFacts.FrameColumns(state)}",
            $"picks: world {_worldPick ?? "none"}, text {_fontPick ?? "none"}",
            "A or space: next | X or W: world | Y or U: text | R1 or F: fit mode",
            "View or H: hide | R: report | Menu or Esc: report and quit",
        };

        int needed = ProbeText.WrappedDialogue(ScreenFacts.DialogueColumns(state)).Count;
        if (needed > ProbeText.DialogueLines)
        {
            rows.Add($"note the sample dialogue needs {needed} lines at this body size, and the"
                + $" box holds {ProbeText.DialogueLines}. The text of the game would be cut (D-635)");
        }

        if (!state.BodyMeetsFloor)
        {
            rows.Add($"note the body draws the {state.BodyNative} strike at 1x, so a stem is one"
                + " frame pixel. It is one device pixel on the Deck, below the floor of D-639."
                + " The glyph carries four times the detail of the finer strike doubled");
        }

        if (!state.WorldIsExact)
        {
            rows.Add("note world 1.5x is the one combination that is not pixel-exact: an art"
                + " pixel covers 1 or 2 frame pixels, and the edges are uneven (D-573)");
        }

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

    private void DrawText(Vector2I topLeft, string text, int fontSize, Color color)
    {
        float baseline = _body.GetAscent(fontSize);
        DrawString(_body, new Vector2(topLeft.X, topLeft.Y + baseline), text,
            HorizontalAlignment.Left, -1, fontSize, color);
    }

    /// <summary>Draws a checkerboard of one device pixel for each square, with no texture.</summary>
    private void DrawPixelCheck(Vector2I topLeft, int unit)
    {
        for (int y = 0; y < CheckSquares * unit; y++)
        {
            for (int x = 0; x < CheckSquares * unit; x++)
            {
                if ((x + y) % 2 == 0)
                {
                    DrawRect(new Rect2I(topLeft.X + x, topLeft.Y + y, 1, 1), Colors.White);
                }
            }
        }
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

        DrawText(new Vector2I(topLeft.X, barY + (2 * unit)), $"{millimeters} mm on the glass", fontSize, Colors.White);
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
            string path = ProbeReport.Write(folder, _options, _facts, _worldPick, _fontPick);
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
