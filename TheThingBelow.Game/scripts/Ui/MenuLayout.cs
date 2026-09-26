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

    /// <summary>The width of the window of a hub service, in frame pixels. A line of help holds 37 characters at a body of 32.</summary>
    public const int ServiceWidth = 640;

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

    /// <summary>The row of the line of help in the window of a hub service: an empty line stands between it and the last choice (D-1172).</summary>
    public static readonly int ServiceHelpRow = ServiceChoice.Options.Count + 1;

    /// <summary>Gives the place of the window of a hub service: one line for each choice, an empty line, and the line of help (D-1131, D-1132, D-1172).</summary>
    /// <param name="body">The body size, in frame pixels.</param>
    /// <returns>The box at the top left of the frame, where the main list stands when it is open.</returns>
    public static FrameBox ServiceBox(int body) =>
        new(UiMetrics.EdgePixels, UiMetrics.EdgePixels, ServiceWidth, (Pad * 2) + (LineOf(body) * (ServiceHelpRow + 1)));

    /// <summary>Gives the place of the shop menu, where the main list stands: one line for each choice (D-1164).</summary>
    /// <param name="body">The body size, in frame pixels.</param>
    /// <returns>The box at the top left of the frame.</returns>
    public static FrameBox ShopMenuBox(int body) =>
        new(UiMetrics.EdgePixels, UiMetrics.EdgePixels, MainListWidth, (Pad * 2) + (LineOf(body) * ShopCursor.Modes.Count));

    /// <summary>Gives the place of the stats panel of the shop at the bottom left: one line for each stat that gear changes (D-1165).</summary>
    /// <param name="body">The body size, in frame pixels.</param>
    /// <param name="lines">The count of lines of the panel.</param>
    /// <returns>The box at the bottom left of the frame, as wide as the main list.</returns>
    public static FrameBox StatsBox(int body, int lines)
    {
        int height = (Pad * 2) + (LineOf(body) * lines);
        return new(UiMetrics.EdgePixels, ScreenFit.FrameHeight - UiMetrics.EdgePixels - height, MainListWidth, height);
    }

    /// <summary>Gives the place of the gold panel, one line under a window of the left column (D-1160).</summary>
    /// <param name="above">The window above the panel: the main list, the shop menu, or the rest window.</param>
    /// <param name="body">The body size, in frame pixels.</param>
    /// <returns>The box under the window, as wide as the main list.</returns>
    public static FrameBox GoldBox(FrameBox above, int body) =>
        new(above.X, above.Y + above.Height + WindowGap, MainListWidth, (Pad * 2) + LineOf(body));

    /// <summary>Gives the count of characters that one line of the window of a hub service holds at a body size.</summary>
    /// <param name="body">The body size, in frame pixels.</param>
    /// <returns>The count of whole characters.</returns>
    public static int ServiceLineCharacters(int body) => UiMetrics.CharactersAcross(body, ServiceWidth - (Pad * 2));

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

    /// <summary>The count of lines of the dialogue box: a dialogue line holds three lines at most (D-635).</summary>
    public const int DialogueLines = 3;

    /// <summary>The side of a portrait on screen: 64 art pixels at 2x (D-234).</summary>
    public const int PortraitPixels = 128;

    /// <summary>The width of the window of the portrait and the name plate: ten characters of a name at a body of 32.</summary>
    public const int PortraitWidth = 192;

    /// <summary>The width of the window of the choices, above the right end of the dialogue box (D-1175).</summary>
    public const int ChoiceWidth = 640;

    /// <summary>Gives the place of the dialogue box at the bottom of the frame: three lines across the width of the frame (D-114, D-223, D-635).</summary>
    /// <param name="body">The body size, in frame pixels.</param>
    /// <returns>The box, from the left edge to the right edge.</returns>
    public static FrameBox DialogueBox(int body)
    {
        int height = (NoticePad * 2) + (LineOf(body) * DialogueLines);
        return new(UiMetrics.EdgePixels, ScreenFit.FrameHeight - UiMetrics.EdgePixels - height, ScreenFit.FrameWidth - (UiMetrics.EdgePixels * 2), height);
    }

    /// <summary>Gives the place of the window of the portrait, with the name plate over the portrait, above the left end of the dialogue box (D-223).</summary>
    /// <param name="body">The body size, in frame pixels.</param>
    /// <returns>The box.</returns>
    public static FrameBox PortraitBox(int body)
    {
        int height = (NoticePad * 2) + LineOf(body) + LineGap + PortraitPixels;
        return new(UiMetrics.EdgePixels, DialogueBox(body).Y - WindowGap - height, PortraitWidth, height);
    }

    /// <summary>Gives the place of the window of the choices, above the right end of the dialogue box: one row for each option (D-1175).</summary>
    /// <param name="body">The body size, in frame pixels.</param>
    /// <param name="options">The count of options, from two to four.</param>
    /// <returns>The box.</returns>
    public static FrameBox ChoiceBox(int body, int options)
    {
        int height = (Pad * 2) + (LineOf(body) * options);
        return new(ScreenFit.FrameWidth - UiMetrics.EdgePixels - ChoiceWidth, DialogueBox(body).Y - WindowGap - height, ChoiceWidth, height);
    }

    /// <summary>Gives the count of characters that one line of the dialogue box holds at a body size (D-635).</summary>
    /// <param name="body">The body size, in frame pixels.</param>
    /// <returns>The count of whole characters.</returns>
    public static int DialogueCharacters(int body) => UiMetrics.CharactersAcross(body, DialogueBox(body).Width - (NoticePad * 2));

    /// <summary>Gives the count of characters that one row of the window of the choices holds at a body size.</summary>
    /// <param name="body">The body size, in frame pixels.</param>
    /// <returns>The count of whole characters.</returns>
    public static int ChoiceCharacters(int body) => UiMetrics.CharactersAcross(body, ChoiceWidth - (Pad * 2));

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
