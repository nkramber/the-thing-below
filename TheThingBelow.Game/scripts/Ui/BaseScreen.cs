using System;
using Godot;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The one screen of the UI base: a window frame, the title of the game, and the row of
/// button prompts (D-220, D-222, D-527). PR-7 puts the map in its place, and PR-33 builds the
/// title screen.
/// </summary>
/// <remarks>
/// The panel holds its longest string at each body size, and a test proves it (D-241,
/// D-708). Every word comes from the string table through the one text helper (G-7, D-499).
/// </remarks>
public partial class BaseScreen : PanelContainer
{
    /// <summary>The string id of the title of the game (G-7).</summary>
    public const string TitleId = "ui.title";

    /// <summary>The count of frame pixels between the title and the prompt row.</summary>
    public const int LineGapPixels = 16;

    /// <summary>The count of frame pixels between the panel and the edge of the frame.</summary>
    public const int EdgePixels = 32;

    private PromptBar prompts = null!;

    /// <summary>Builds the panel of one UI base.</summary>
    /// <param name="base">The theme, the text helper, the atlas, and the device tracker.</param>
    /// <exception cref="ArgumentNullException">The UI base is null (T-2).</exception>
    public void Build(UiBase @base)
    {
        ArgumentNullException.ThrowIfNull(@base);

        this.Theme = @base.Theme.Theme;
        this.Position = new Vector2(EdgePixels, EdgePixels);
        this.Size = new Vector2(
            ScreenFit.FrameWidth - (EdgePixels * 2),
            ScreenFit.FrameHeight - (EdgePixels * 2));

        var lines = new VBoxContainer();
        lines.AddThemeConstantOverride("separation", LineGapPixels);

        var title = new Label { ThemeTypeVariation = UiTheme.TitleVariation };
        @base.Text.Put(title, ContentId.Parse(TitleId, StringTable.Path, nameof(BaseScreen)));
        lines.AddChild(title);

        this.prompts = new PromptBar();
        this.prompts.Build(@base);
        lines.AddChild(this.prompts);

        this.AddChild(lines);
    }

    /// <summary>Draws the prompts again after the player picked up another device (D-222).</summary>
    public void OnDeviceChanged() => this.prompts.DrawPrompts();
}
