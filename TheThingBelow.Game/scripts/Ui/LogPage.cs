using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The lines of the notice log window: the entries newest first, and the part that the
/// window shows (D-984, D-987).
/// </summary>
/// <remarks>
/// The log keeps 30 entries, and a window at a body of 32 holds fewer lines, so up and down
/// scroll the page one line (D-707). This type holds no Godot value, so a test reads it with
/// no engine (D-614).
/// </remarks>
public sealed class LogPage
{
    private readonly List<ContentId> lines = [];

    /// <summary>Opens the page at the newest entry.</summary>
    /// <param name="entries">The entries of the log, oldest first, as the run holds them.</param>
    /// <param name="linesShown">The count of lines that the window shows at its body size.</param>
    /// <exception cref="ArgumentNullException">The entries are null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The window shows no line (T-2).</exception>
    public LogPage(IReadOnlyList<ContentId> entries, int linesShown)
    {
        ArgumentNullException.ThrowIfNull(entries);
        if (linesShown < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(linesShown), linesShown, "The log window shows one line or more (T-2).");
        }

        for (int index = entries.Count - 1; index >= 0; index -= 1)
        {
            this.lines.Add(entries[index]);
        }

        this.LinesShown = linesShown;
    }

    /// <summary>Every entry, newest first (D-987).</summary>
    public IReadOnlyList<ContentId> Lines => this.lines;

    /// <summary>The count of lines that the window shows.</summary>
    public int LinesShown { get; }

    /// <summary>The place in <see cref="Lines"/> of the first line that the window shows.</summary>
    public int First { get; private set; }

    /// <summary>Gives the lines that the window shows now, newest first.</summary>
    /// <returns>The lines, which can be fewer than <see cref="LinesShown"/>.</returns>
    public IReadOnlyList<ContentId> Shown()
    {
        List<ContentId> shown = [];
        for (int index = this.First; index < this.lines.Count && index < this.First + this.LinesShown; index += 1)
        {
            shown.Add(this.lines[index]);
        }

        return shown;
    }

    /// <summary>Scrolls the page by one line, and stops at each end.</summary>
    /// <param name="step">-1 toward the newest line, and 1 toward the oldest.</param>
    /// <exception cref="ArgumentOutOfRangeException">The step is not -1 or 1 (T-2).</exception>
    public void Scroll(int step)
    {
        if (step != -1 && step != 1)
        {
            throw new ArgumentOutOfRangeException(nameof(step), step, "The page scrolls one line up or down (T-2).");
        }

        int last = Math.Max(0, this.lines.Count - this.LinesShown);
        this.First = Math.Clamp(this.First + step, 0, last);
    }
}
