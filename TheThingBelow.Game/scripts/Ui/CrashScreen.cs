using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The message that a crash shows on screen (D-170, D-559). It names the crash file, its folder,
/// and the address that takes it, and the player sends the file by hand.
/// </summary>
/// <remarks>
/// The address is a placeholder in the reserved `.invalid` top-level domain until the owner
/// names the studio, and the string id is final (D-473, D-712, OQ-57). The repository is
/// public, so no personal address enters a file (D-4, D-450).
/// <para>
/// Every line goes through the one text helper, so det-lint reads no player string in this
/// file (G-7, D-499). The screen shows the folder in the short form of its system, such as
/// `~/.local/share/the-thing-below/crashes`, and never the folder of the person, because a
/// screenshot of the message would name the account (D-170, D-1102).
/// </para>
/// </remarks>
public partial class CrashScreen : PanelContainer
{
    /// <summary>The string id of the first line of the message.</summary>
    public const string TitleId = "crash.title";

    /// <summary>The string id of the line that names the crash file.</summary>
    public const string FileId = "crash.file";

    /// <summary>The string id of the line that names the folder of the crash files (D-170, D-1102).</summary>
    public const string FolderId = "crash.folder";

    /// <summary>The string id of the line that names the address (D-473).</summary>
    public const string SendId = "crash.send";

    /// <summary>The string id of the address itself, which OQ-57 closes (D-712).</summary>
    public const string AddressId = "crash.address";

    /// <summary>The string id of the last line, which tells the player how to quit.</summary>
    public const string QuitId = "crash.quit";

    /// <summary>The name of the place that the file line fills in.</summary>
    public const string FilePlace = "file";

    /// <summary>The name of the place that the folder line fills in.</summary>
    public const string FolderPlace = "folder";

    /// <summary>The name of the place that the send line fills in.</summary>
    public const string AddressPlace = "address";

    /// <summary>The count of frame pixels between two lines of the message.</summary>
    public const int LineGapPixels = 8;

    private readonly List<Label> shown = [];

    /// <summary>Builds the message of one crash.</summary>
    /// <param name="base">The theme, the text helper, and the atlas (D-499, D-527).</param>
    /// <param name="strings">The string table, which holds the address (G-7).</param>
    /// <param name="crashFileName">The name of the crash file, with no folder (D-170).</param>
    /// <param name="shownFolder">The folder of the crash files, with no account name (D-1102).</param>
    /// <exception cref="ArgumentNullException">The UI base, the table, or the name is null (T-2).</exception>
    /// <exception cref="ContentException">The string table lacks a line of the message (T-2).</exception>
    public void Build(UiBase @base, StringTable strings, string crashFileName, string shownFolder)
    {
        ArgumentNullException.ThrowIfNull(@base);
        ArgumentNullException.ThrowIfNull(strings);
        ArgumentNullException.ThrowIfNull(crashFileName);
        ArgumentNullException.ThrowIfNull(shownFolder);

        this.Theme = @base.Theme.Theme;

        var lines = new VBoxContainer();
        lines.AddThemeConstantOverride("separation", LineGapPixels);

        var title = new Label { ThemeTypeVariation = UiTheme.TitleVariation };
        @base.Text.Put(title, Id(TitleId));
        lines.AddChild(title);
        this.shown.Add(title);

        var file = new Label();
        @base.Text.Put(
            file,
            Id(FileId),
            new SortedDictionary<string, string>(StringComparer.Ordinal) { [FilePlace] = crashFileName });
        lines.AddChild(file);
        this.shown.Add(file);

        var folder = new Label();
        @base.Text.Put(
            folder,
            Id(FolderId),
            new SortedDictionary<string, string>(StringComparer.Ordinal) { [FolderPlace] = shownFolder });
        lines.AddChild(folder);
        this.shown.Add(folder);

        var send = new Label();
        @base.Text.Put(
            send,
            Id(SendId),
            new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                [AddressPlace] = strings.Text(Id(AddressId)),
            });
        lines.AddChild(send);
        this.shown.Add(send);

        var quit = new Label();
        @base.Text.Put(quit, Id(QuitId));
        lines.AddChild(quit);
        this.shown.Add(quit);

        this.AddChild(lines);
    }

    /// <summary>
    /// Gives the text of each line that the message shows, from the top, so the crash fixture reads
    /// back the name of the file and the folder (P3-26, D-1102).
    /// </summary>
    /// <returns>The text of each line.</returns>
    public IReadOnlyList<string> ShownLines()
    {
        var lines = new List<string>(this.shown.Count);
        foreach (Label line in this.shown)
        {
            lines.Add(TextHelper.Shown(line));
        }

        return lines;
    }

    /// <summary>Gives the content id of one line of the message.</summary>
    /// <param name="value">The string id, such as `crash.title`.</param>
    /// <returns>The id.</returns>
    /// <exception cref="ContentException">The text is not a well-formed id (T-2).</exception>
    public static ContentId Id(string value) =>
        ContentId.Parse(value, "TheThingBelow.Game/scripts/Ui/CrashScreen.cs", nameof(CrashScreen));
}
