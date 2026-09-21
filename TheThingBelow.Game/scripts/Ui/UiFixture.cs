using System;
using Godot;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The `ui` fixture of the screen-test job: one window frame with the title of the game and
/// the longest plain string of the string table, and no world (D-172, D-734).
/// </summary>
/// <remarks>
/// The map fixture hides the window frame and the title face, so a change of either one
/// passes a capture of the map alone. This fixture shows both, and D-241 binds the width:
/// a panel holds its longest string at each body size (D-708).
/// <para>
/// A string with a place to fill, such as `{file}`, needs a value from its caller, so this
/// fixture reads the longest string that holds no place (T-2). Every word comes from the
/// string table through the one text helper (G-7, D-499).
/// </para>
/// </remarks>
public partial class UiFixture : Control
{
    /// <summary>The string id of the title of the game, which draws at the title size (D-708).</summary>
    public const string TitleId = "ui.title";

    /// <summary>Builds the window frame, the title, and the longest plain string.</summary>
    /// <param name="base">The atlas, the theme, the text helper, and the device tracker.</param>
    /// <param name="strings">The string table of the content set (G-7).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">The table holds no plain string (T-2).</exception>
    public void Build(UiBase @base, StringTable strings)
    {
        ArgumentNullException.ThrowIfNull(@base);
        ArgumentNullException.ThrowIfNull(strings);

        this.Theme = @base.Theme.Theme;
        this.Position = new Vector2(UiMetrics.EdgePixels, UiMetrics.EdgePixels);
        this.Size = new Vector2(
            ScreenFit.FrameWidth - (UiMetrics.EdgePixels * 2),
            ScreenFit.FrameHeight - (UiMetrics.EdgePixels * 2));

        var panel = new PanelContainer { Position = Vector2.Zero, Size = this.Size };
        var lines = new VBoxContainer();
        lines.AddThemeConstantOverride("separation", @base.Theme.BodySize);

        var title = new Label { ThemeTypeVariation = UiTheme.TitleVariation };
        @base.Text.Put(title, ContentId.Parse(TitleId, StringTable.Path, "id"));

        var longest = new Label();
        @base.Text.Put(longest, LongestPlainId(strings));

        lines.AddChild(title);
        lines.AddChild(longest);
        panel.AddChild(lines);
        this.AddChild(panel);
    }

    /// <summary>
    /// Gives the id of the longest string of the table that holds no place to fill (D-241).
    /// A tie takes the first id in ordinal order, so every run picks the same string (T-7).
    /// </summary>
    /// <param name="strings">The string table of the content set.</param>
    /// <returns>The id of that string.</returns>
    /// <exception cref="ArgumentNullException">The table is null (T-2).</exception>
    /// <exception cref="ContentException">Every string of the table holds a place (T-2).</exception>
    public static ContentId LongestPlainId(StringTable strings)
    {
        ArgumentNullException.ThrowIfNull(strings);

        ContentId? longest = null;
        int length = -1;
        foreach (string name in strings.Ids)
        {
            ContentId id = ContentId.Parse(name, StringTable.Path, "id");
            string text = strings.Text(id);
            if (text.IndexOf(TextHelper.OpenMark, StringComparison.Ordinal) >= 0)
            {
                continue;
            }

            if (text.Length > length)
            {
                longest = id;
                length = text.Length;
            }
        }

        return longest ?? throw ContentException.ForFile(
            StringTable.Path, "every string of the table holds a place to fill, and the fixture needs one plain string");
    }
}
