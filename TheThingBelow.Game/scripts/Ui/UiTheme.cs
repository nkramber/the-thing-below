using System;
using Godot;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The Godot `Theme` of the game, built in code from the UI style file at load (D-527, G-6).
/// No theme resource file exists, and the project setting for a theme names a path, so it
/// stays empty.
/// </summary>
/// <remarks>
/// The theme carries the two fonts at the body size that the player set, the colors of the
/// style file as palette colors, and the window frame of the atlas as a nine-part box
/// (D-181, D-220, D-707). A change of the body size builds a new theme, because each size
/// pins its own bitmap strike (D-710).
/// </remarks>
public sealed class UiTheme
{
    /// <summary>The name of the label variation that draws a title (D-264, D-707).</summary>
    public const string TitleVariation = "TitleLabel";

    /// <summary>The count of pixels of each edge of the window frame drawing (D-220).</summary>
    public const int FrameEdge = 8;

    private readonly UiStyle style;
    private readonly Palette palette;

    private UiTheme(UiStyle style, Palette palette, Theme theme, int bodySize, int titleSize)
    {
        this.style = style;
        this.palette = palette;
        this.Theme = theme;
        this.BodySize = bodySize;
        this.TitleSize = titleSize;
    }

    /// <summary>The theme that every screen of the game takes.</summary>
    public Theme Theme { get; }

    /// <summary>The body size of the text, in frame pixels (D-707).</summary>
    public int BodySize { get; }

    /// <summary>The title size of the text, in frame pixels (D-707).</summary>
    public int TitleSize { get; }

    /// <summary>Builds the theme of one body size.</summary>
    /// <param name="content">The content set of this build.</param>
    /// <param name="atlas">The pages of the atlas, which hold the window frame (D-666).</param>
    /// <param name="bodySize">The body size that the player set, in frame pixels (D-707).</param>
    /// <returns>The theme, with the fonts, the colors, and the window frame.</returns>
    /// <exception cref="ArgumentNullException">The content set or the atlas is null (T-2).</exception>
    /// <exception cref="ContentException">
    /// The style file names a size, a color, or a drawing that the build lacks (T-2).
    /// </exception>
    public static UiTheme Build(ContentSet content, GameAtlas atlas, int bodySize)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(atlas);

        UiStyle style = content.Style;
        int titleSize = style.TitleSizeOf(bodySize);

        (int bodyStrike, _) = GameFonts.StrikeFor(content.BodyFont, bodySize);
        (int titleStrike, _) = GameFonts.StrikeFor(content.TitleFont, titleSize);
        FontFile body = GameFonts.Build(FontStrikes.BodyPath, content.BodyFont, bodyStrike);
        FontFile title = GameFonts.Build(FontStrikes.TitlePath, content.TitleFont, titleStrike);

        var theme = new Theme
        {
            DefaultFont = body,
            DefaultFontSize = bodySize,
        };

        var built = new UiTheme(style, content.Palette, theme, bodySize, titleSize);
        built.AddLabels(body, title);
        built.AddPanel(atlas);
        return built;
    }

    /// <summary>Gives one color of the style file as a Godot color (D-181, D-527).</summary>
    /// <param name="role">The role, such as `text`.</param>
    /// <returns>The color of the palette key of that role.</returns>
    /// <exception cref="ContentException">The style file holds no color of that role (T-2).</exception>
    public Color ColorOf(string role)
    {
        UiColor color = this.style.ColorOf(role);
        if (!this.palette.TryColorOf(color.KeyCharacter, out PaletteColor? found))
        {
            throw ContentException.ForField(
                UiStyle.Path,
                role,
                $"the palette holds no color with the key '{color.Key}' (D-181)");
        }

        return Color.Color8((byte)found.Red, (byte)found.Green, (byte)found.Blue);
    }

    private void AddLabels(FontFile body, FontFile title)
    {
        this.Theme.SetFont("font", "Label", body);
        this.Theme.SetFontSize("font_size", "Label", this.BodySize);
        this.Theme.SetColor("font_color", "Label", this.ColorOf("text"));

        // A title is the bold face at twice the body, and it takes a variation of the label
        // so a screen names the look and never the size (D-264, D-707).
        this.Theme.SetTypeVariation(TitleVariation, "Label");
        this.Theme.SetFont("font", TitleVariation, title);
        this.Theme.SetFontSize("font_size", TitleVariation, this.TitleSize);
        this.Theme.SetColor("font_color", TitleVariation, this.ColorOf("text_chosen"));
    }

    /// <summary>
    /// Adds the window frame as a nine-part box. The drawing holds each corner at its own
    /// size, so the edges repeat and the corners never stretch (D-220).
    /// </summary>
    private void AddPanel(GameAtlas atlas)
    {
        UiFrame frame = this.style.FrameOf(UiStyle.WindowFrameRole);
        var box = new StyleBoxTexture
        {
            Texture = atlas.Frame(frame.Drawing, 0),
            TextureMarginLeft = FrameEdge,
            TextureMarginRight = FrameEdge,
            TextureMarginTop = FrameEdge,
            TextureMarginBottom = FrameEdge,
            ContentMarginLeft = FrameEdge,
            ContentMarginRight = FrameEdge,
            ContentMarginTop = FrameEdge,
            ContentMarginBottom = FrameEdge,
            AxisStretchHorizontal = StyleBoxTexture.AxisStretchMode.Tile,
            AxisStretchVertical = StyleBoxTexture.AxisStretchMode.Tile,
        };

        this.Theme.SetStylebox("panel", "PanelContainer", box);
        this.Theme.SetStylebox("panel", "Panel", box);
    }
}
