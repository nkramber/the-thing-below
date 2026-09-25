using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The message of a start whose settings file failed to load (D-1099, D-1100). It names the
/// field that failed and the name under which the game kept the file, and it says that the game
/// runs on the defaults. The world holds until the player presses a button.
/// </summary>
/// <remarks>
/// The fallback is loud, so no choice of the player goes away in silence (T-2, D-570). Every line
/// goes through the one text helper, so det-lint reads no player string in this file (G-7, D-499).
/// </remarks>
public partial class SettingsNotice : PanelContainer
{
    /// <summary>The string id of the first line.</summary>
    public const string TitleId = "settings.refused_title";

    /// <summary>The string id of the line that names the field that failed.</summary>
    public const string FieldId = "settings.refused_field";

    /// <summary>The string id of the line that names the kept file.</summary>
    public const string KeptId = "settings.refused_kept";

    /// <summary>The string id of the line that says the game runs on the defaults.</summary>
    public const string DefaultsId = "settings.refused_defaults";

    /// <summary>The string id of the last line, which tells the player how to go on.</summary>
    public const string GoOnId = "settings.refused_go_on";

    /// <summary>The name of the place that the field line fills in.</summary>
    public const string FieldPlace = "field";

    /// <summary>The name of the place that the kept line fills in.</summary>
    public const string FilePlace = "file";

    /// <summary>The count of frame pixels between two lines of the message.</summary>
    public const int LineGapPixels = 8;

    /// <summary>Builds the message.</summary>
    /// <param name="base">The theme, the text helper, and the atlas (D-499, D-527).</param>
    /// <param name="field">The field of the file that failed, or the words for the whole file.</param>
    /// <param name="keptFileName">The name of the kept file, with no folder (D-170).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">The string table lacks a line of the message (T-2).</exception>
    public void Build(UiBase @base, string field, string keptFileName)
    {
        ArgumentNullException.ThrowIfNull(@base);
        ArgumentNullException.ThrowIfNull(field);
        ArgumentNullException.ThrowIfNull(keptFileName);

        this.Theme = @base.Theme.Theme;

        var lines = new VBoxContainer();
        lines.AddThemeConstantOverride("separation", LineGapPixels);

        var title = new Label { ThemeTypeVariation = UiTheme.TitleVariation };
        @base.Text.Put(title, Id(TitleId));
        lines.AddChild(title);

        var fieldLine = new Label();
        @base.Text.Put(
            fieldLine,
            Id(FieldId),
            new SortedDictionary<string, string>(StringComparer.Ordinal) { [FieldPlace] = field });
        lines.AddChild(fieldLine);

        var kept = new Label();
        @base.Text.Put(
            kept,
            Id(KeptId),
            new SortedDictionary<string, string>(StringComparer.Ordinal) { [FilePlace] = keptFileName });
        lines.AddChild(kept);

        var defaults = new Label();
        @base.Text.Put(defaults, Id(DefaultsId));
        lines.AddChild(defaults);

        var goOn = new Label();
        @base.Text.Put(goOn, Id(GoOnId));
        lines.AddChild(goOn);

        this.AddChild(lines);
    }

    /// <summary>Gives the content id of one line of the message.</summary>
    /// <param name="value">The string id, such as `settings.refused_title`.</param>
    /// <returns>The id.</returns>
    /// <exception cref="ContentException">The text is not a well-formed id (T-2).</exception>
    public static ContentId Id(string value) =>
        ContentId.Parse(value, "TheThingBelow.Game/scripts/Ui/SettingsNotice.cs", nameof(SettingsNotice));
}
