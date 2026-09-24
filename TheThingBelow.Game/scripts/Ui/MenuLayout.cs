using System;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The places of the menu windows and of the notice box on the frame of 1280 by 720, at
/// each body size (D-211, D-568, D-707).
/// </summary>
/// <remarks>
/// The main list stands at the left edge, and each task window stacks to its right, so the
/// main list and the map stay visible behind it (D-211). The font gives each glyph half the
/// body size, so a width holds a known count of characters (D-707). This type holds no Godot
/// value, so a test reads it with no engine (D-614).
/// </remarks>
public static class MenuLayout
{
    /// <summary>The frame pixels between the frame of a window and its text.</summary>
    public const int Pad = 24;

    /// <summary>The frame pixels between two lines of text.</summary>
    public const int LineGap = 4;

    /// <summary>The width of the main list, in frame pixels. It holds a menu label of 16 characters at a body of 32.</summary>
    public const int MainListWidth = 320;

    /// <summary>The frame pixels between two windows of the stack.</summary>
    public const int WindowGap = 8;

    /// <summary>The width of the notice box, in frame pixels.</summary>
    public const int NoticeWidth = 960;

    /// <summary>The frame pixels between the frame of the notice box and its line.</summary>
    public const int NoticePad = 16;

    /// <summary>The characters of the place of one word of the key of the dungeon map screen, with the gap to the next square.</summary>
    public const int KeyWordCharacters = 10;

    /// <summary>The count of columns of the status window: one for each character of a full party (D-31, D-991).</summary>
    public const int StatusColumns = 3;

    /// <summary>Gives the height of one line of text at a body size.</summary>
    /// <param name="body">The body size, in frame pixels (D-707).</param>
    /// <returns>The body size and the gap.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The size is not above zero (T-2).</exception>
    public static int LineOf(int body)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(body, 1);

        return body + LineGap;
    }

    /// <summary>Gives the place of the main list, which holds one line for each entry (D-992).</summary>
    /// <param name="body">The body size, in frame pixels.</param>
    /// <returns>The box at the top left of the frame.</returns>
    public static FrameBox MainListBox(int body) =>
        new(UiMetrics.EdgePixels, UiMetrics.EdgePixels, MainListWidth, (Pad * 2) + (LineOf(body) * MainList.Entries.Count));

    /// <summary>Gives the place of a task window, to the right of the main list, from the top edge to the bottom edge.</summary>
    /// <returns>The box.</returns>
    public static FrameBox TaskBox()
    {
        int left = UiMetrics.EdgePixels + MainListWidth + WindowGap;
        return new(left, UiMetrics.EdgePixels, ScreenFit.FrameWidth - UiMetrics.EdgePixels - left, ScreenFit.FrameHeight - (UiMetrics.EdgePixels * 2));
    }

    /// <summary>Gives the place of the notice box at the top edge, when it has slid all the way in (D-221).</summary>
    /// <param name="body">The body size, in frame pixels.</param>
    /// <returns>The box, in the middle of the top edge.</returns>
    public static FrameBox NoticePlace(int body) =>
        new((ScreenFit.FrameWidth - NoticeWidth) / 2, UiMetrics.EdgePixels, NoticeWidth, LineOf(body) + (NoticePad * 2));

    /// <summary>Gives the count of characters that one line of a task window holds at a body size.</summary>
    /// <param name="body">The body size, in frame pixels.</param>
    /// <returns>The count of whole characters.</returns>
    public static int TaskLineCharacters(int body) => UiMetrics.CharactersAcross(body, TaskBox().Width - (Pad * 2));

    /// <summary>Gives the count of characters that the line of the notice box holds at a body size.</summary>
    /// <param name="body">The body size, in frame pixels.</param>
    /// <returns>The count of whole characters.</returns>
    public static int NoticeCharacters(int body) => UiMetrics.CharactersAcross(body, NoticeWidth - (NoticePad * 2));

    /// <summary>Gives the count of characters that one column of the status window holds at a body size (D-991).</summary>
    /// <param name="body">The body size, in frame pixels.</param>
    /// <returns>The count of whole characters.</returns>
    public static int StatusColumnCharacters(int body) =>
        UiMetrics.CharactersAcross(body, (TaskBox().Width - (Pad * 2)) / StatusColumns);

    /// <summary>Gives the top of the first line under the title of a task window.</summary>
    /// <param name="body">The body size, in frame pixels.</param>
    /// <param name="titleSize">The title size, in frame pixels (D-707).</param>
    /// <returns>The row, in frame pixels.</returns>
    public static int FirstLineTop(int body, int titleSize) => TaskBox().Y + Pad + titleSize + LineOf(body);

    /// <summary>Gives the count of lines of the log window under its title (D-987).</summary>
    /// <param name="body">The body size, in frame pixels.</param>
    /// <param name="titleSize">The title size, in frame pixels.</param>
    /// <returns>The count of whole lines.</returns>
    public static int LogLines(int body, int titleSize)
    {
        FrameBox window = TaskBox();
        int bottom = window.Y + window.Height - Pad;
        return (bottom - FirstLineTop(body, titleSize)) / LineOf(body);
    }
}
