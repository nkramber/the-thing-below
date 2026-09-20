using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Debug.Commands;

namespace TheThingBelow.Debug.Console;

/// <summary>
/// The screen of the debug console (D-171). It builds engine nodes, it reads the typed line
/// from a `LineEdit`, and it prints the answer of <see cref="DebugSession"/> above it.
/// </summary>
/// <remarks>
/// No type of this assembly derives from a Godot node. This project takes no source generator
/// of Godot, so the engine calls no virtual member of a type of ours, and a node of this
/// console is an engine type with its own signal (D-723). A `LineEdit` with the focus takes
/// every key of the person, so the game makes no intent while the console is open (D-725).
/// <para>
/// The console draws in the frame layer of 1280 by 720, above the world (D-568). It takes the
/// font of the engine and not the font of the game, because this assembly never references
/// Game (D-723).
/// </para>
/// </remarks>
public sealed class DebugConsole
{
    /// <summary>The width of the frame layer, in frame pixels (D-568).</summary>
    private const int ScreenWidth = 1280;

    /// <summary>The height of the console on the frame, in frame pixels (D-568).</summary>
    public const int Height = 320;

    /// <summary>The height of the line that the person types in, in frame pixels.</summary>
    public const int EntryHeight = 32;

    /// <summary>The space between the text and the edge of the console, in frame pixels.</summary>
    public const int EdgePixels = 8;

    /// <summary>The count of lines that the console keeps.</summary>
    public const int MaxLines = 12;

    /// <summary>The first line, which the console prints when it opens.</summary>
    public const string OpeningLine = "the debug console of a development build. `help` lists every command.";

    /// <summary>The name of the node that the person types in.</summary>
    public const string EntryName = "DebugConsoleEntry";

    /// <summary>The name of the node that holds the lines of the console.</summary>
    public const string OutputName = "DebugConsoleOutput";

    private readonly DebugSession session;
    private readonly Label output;
    private readonly LineEdit entry;
    private readonly List<string> lines = [];

    /// <summary>Builds the console over one run.</summary>
    /// <param name="session">The session that runs each typed line.</param>
    /// <exception cref="ArgumentNullException">The session is null (T-2).</exception>
    public DebugConsole(DebugSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        this.session = session;

        this.Root = new Panel
        {
            Name = "DebugConsole",
            Visible = false,
            OffsetRight = ScreenWidth,
            OffsetBottom = Height,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };

        this.output = new Label
        {
            Name = OutputName,
            OffsetLeft = EdgePixels,
            OffsetTop = EdgePixels,
            OffsetRight = ScreenWidth - EdgePixels,
            OffsetBottom = Height - EntryHeight - EdgePixels,
            VerticalAlignment = VerticalAlignment.Bottom,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };

        this.entry = new LineEdit
        {
            Name = EntryName,
            OffsetLeft = EdgePixels,
            OffsetTop = Height - EntryHeight - EdgePixels,
            OffsetRight = ScreenWidth - EdgePixels,
            OffsetBottom = Height - EdgePixels,
            PlaceholderText = "command",
            ClearButtonEnabled = false,
        };

        this.Root.AddChild(this.output);
        this.Root.AddChild(this.entry);

        this.entry.TextSubmitted += this.OnLineSubmitted;
        this.Root.VisibilityChanged += this.OnVisibilityChanged;

        this.Write([OpeningLine]);
    }

    /// <summary>The node of the console, which the host adds to the frame layer (D-568).</summary>
    public Panel Root { get; }

    /// <summary>The lines that the console shows now, from the oldest to the newest.</summary>
    public IReadOnlyList<string> Lines => this.lines;

    /// <summary>Runs one line, as a submit of the entry does (D-724).</summary>
    /// <param name="line">The text of the line.</param>
    /// <returns>The lines of the answer.</returns>
    /// <exception cref="ArgumentNullException">The line is null (T-2).</exception>
    public IReadOnlyList<string> Run(string line)
    {
        IReadOnlyList<string> answer = this.session.Run(line);
        this.Write(answer);
        return answer;
    }

    /// <summary>
    /// Types one line in the entry of an open console and submits it, as the person does
    /// (D-724). The smoke session of CI runs this check inside the engine, where no test of
    /// Tests reaches (D-117, F-23, T-3).
    /// </summary>
    /// <param name="root">The node that <c>DebugAssembly.Create</c> gave, and the tree holds.</param>
    /// <param name="line">The text of the line, such as `help`.</param>
    /// <returns>The lines that the console shows after the submit.</returns>
    /// <exception cref="ArgumentNullException">The node or the line is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">
    /// The node is no console of this assembly, the console is closed, the entry holds no
    /// focus, or the console read no submit (T-2).
    /// </exception>
    public static IReadOnlyList<string> Submit(Control root, string line)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(line);

        if (root.GetNodeOrNull<LineEdit>(EntryName) is not LineEdit entry
            || root.GetNodeOrNull<Label>(OutputName) is not Label output)
        {
            throw new InvalidOperationException(
                $"The node '{root.Name}' holds no '{EntryName}' and no '{OutputName}', so it is "
                + $"no console of this assembly (T-2).");
        }

        if (!root.Visible)
        {
            throw new InvalidOperationException(
                $"The console '{root.Name}' is closed, and a closed console takes no line (D-725, T-2).");
        }

        // The console takes the focus when it opens, so every key of the person reaches the
        // entry and the game makes no intent (D-725).
        Control? focused = root.GetViewport().GuiGetFocusOwner();
        if (focused != entry)
        {
            throw new InvalidOperationException(
                $"The open console left the focus on '{focused?.Name.ToString() ?? "no node"}', "
                + $"and the person types in '{EntryName}' (D-725, T-2).");
        }

        entry.Text = line;
        entry.EmitSignal(LineEdit.SignalName.TextSubmitted, line);

        // The console clears the entry after each line, so text that stays there says that no
        // handler of the signal ran (T-2).
        if (entry.Text.Length > 0)
        {
            throw new InvalidOperationException(
                $"The console kept the text '{entry.Text}' in '{EntryName}', so it read no "
                + $"submit of that line (D-723, T-2).");
        }

        return output.Text.Split(DebugSession.LineBreak, StringSplitOptions.None);
    }

    private void OnLineSubmitted(string line)
    {
        this.Run(line);
        this.entry.Clear();
    }

    /// <summary>
    /// Takes the focus when the console opens, so every key of the person reaches the entry
    /// and the game makes no intent (D-725).
    /// </summary>
    private void OnVisibilityChanged()
    {
        if (this.Root.Visible)
        {
            this.entry.GrabFocus();
            return;
        }

        this.entry.ReleaseFocus();
        this.entry.Clear();
    }

    private void Write(IReadOnlyList<string> written)
    {
        this.lines.AddRange(written);
        if (this.lines.Count > MaxLines)
        {
            this.lines.RemoveRange(0, this.lines.Count - MaxLines);
        }

        this.output.Text = string.Join(DebugSession.LineBreak, this.lines);
    }
}
