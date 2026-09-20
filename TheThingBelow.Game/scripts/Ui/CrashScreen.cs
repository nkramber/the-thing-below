using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The message that a crash shows on screen (D-170, D-559). It names the crash file and the
/// address that takes it, and the player sends the file by hand.
/// </summary>
/// <remarks>
/// The address is a placeholder in the reserved `.invalid` top-level domain until the owner
/// names the studio, and the string id is final (D-473, D-712, OQ-57). The repository is
/// public, so no personal address enters a file (D-4, D-450).
/// <para>
/// Every line goes through the one text helper, so det-lint reads no player string in this
/// file (G-7, D-499). The screen shows the name of the crash file and never its folder,
/// because a folder path names the person (D-170).
/// </para>
/// </remarks>
public partial class CrashScreen : PanelContainer
{
    /// <summary>The string id of the first line of the message.</summary>
    public const string TitleId = "crash.title";

    /// <summary>The string id of the line that names the crash file.</summary>
    public const string FileId = "crash.file";

    /// <summary>The string id of the line that names the address (D-473).</summary>
    public const string SendId = "crash.send";

    /// <summary>The string id of the address itself, which OQ-57 closes (D-712).</summary>
    public const string AddressId = "crash.address";

    /// <summary>The string id of the last line, which tells the player how to quit.</summary>
    public const string QuitId = "crash.quit";

    /// <summary>The name of the place that the file line fills in.</summary>
    public const string FilePlace = "file";

    /// <summary>The name of the place that the send line fills in.</summary>
    public const string AddressPlace = "address";

    /// <summary>The count of frame pixels between two lines of the message.</summary>
    public const int LineGapPixels = 8;

    /// <summary>Builds the message of one crash.</summary>
    /// <param name="base">The theme, the text helper, and the atlas (D-499, D-527).</param>
    /// <param name="strings">The string table, which holds the address (G-7).</param>
    /// <param name="crashFileName">The name of the crash file, with no folder (D-170).</param>
    /// <exception cref="ArgumentNullException">The UI base, the table, or the name is null (T-2).</exception>
    /// <exception cref="ContentException">The string table lacks a line of the message (T-2).</exception>
    public void Build(UiBase @base, StringTable strings, string crashFileName)
    {
        ArgumentNullException.ThrowIfNull(@base);
        ArgumentNullException.ThrowIfNull(strings);
        ArgumentNullException.ThrowIfNull(crashFileName);

        this.Theme = @base.Theme.Theme;

        var lines = new VBoxContainer();
        lines.AddThemeConstantOverride("separation", LineGapPixels);

        var title = new Label { ThemeTypeVariation = UiTheme.TitleVariation };
        @base.Text.Put(title, Id(TitleId));
        lines.AddChild(title);

        var file = new Label();
        @base.Text.Put(
            file,
            Id(FileId),
            new SortedDictionary<string, string>(StringComparer.Ordinal) { [FilePlace] = crashFileName });
        lines.AddChild(file);

        var send = new Label();
        @base.Text.Put(
            send,
            Id(SendId),
            new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                [AddressPlace] = strings.Text(Id(AddressId)),
            });
        lines.AddChild(send);

        var quit = new Label();
        @base.Text.Put(quit, Id(QuitId));
        lines.AddChild(quit);

        this.AddChild(lines);
    }

    /// <summary>Gives the content id of one line of the message.</summary>
    /// <param name="value">The string id, such as `crash.title`.</param>
    /// <returns>The id.</returns>
    /// <exception cref="ContentException">The text is not a well-formed id (T-2).</exception>
    public static ContentId Id(string value) =>
        ContentId.Parse(value, "TheThingBelow.Game/scripts/Ui/CrashScreen.cs", nameof(CrashScreen));
}
