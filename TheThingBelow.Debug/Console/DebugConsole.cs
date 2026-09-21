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
    /// Gives the lines that a console shows now. The smoke session types a line through key
    /// events and reads the answer here, so it proves the path of a real key (D-117, D-725).
    /// </summary>
    /// <param name="root">The node that <c>DebugAssembly.Create</c> gave.</param>
    /// <returns>The lines of the console, from the oldest to the newest.</returns>
    /// <exception cref="ArgumentNullException">The node is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">The node is no console of this assembly (T-2).</exception>
    public static IReadOnlyList<string> ShownLines(Control root)
    {
        ArgumentNullException.ThrowIfNull(root);

        if (root.GetNodeOrNull<Label>(OutputName) is not Label output)
        {
            throw new InvalidOperationException(
                $"The node '{root.Name}' holds no '{OutputName}', so it is no console of this "
                + $"assembly (T-2).");
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
