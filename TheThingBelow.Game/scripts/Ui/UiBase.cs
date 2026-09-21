using System;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The pieces of the UI that every screen needs: the atlas, the theme, and the text helper
/// (D-499, D-527). One load builds them all from the content set.
/// </summary>
/// <remarks>
/// The body size sets the theme, so a change of the setting of D-707 builds a new
/// <see cref="UiBase"/> and every screen takes the new theme. PR-63 adds that setting.
/// </remarks>
public sealed class UiBase
{
    private UiBase(ContentSet content, GameAtlas atlas, UiTheme theme)
    {
        this.Atlas = atlas;
        this.Theme = theme;
        this.Text = new TextHelper(content.Strings);
    }

    /// <summary>The pages of the atlas as textures (D-666).</summary>
    public GameAtlas Atlas { get; }

    /// <summary>The Godot theme of the body size in use (D-527).</summary>
    public UiTheme Theme { get; }

    /// <summary>The one helper that puts a player string on screen (G-7, D-499).</summary>
    public TextHelper Text { get; }

    /// <summary>Builds the UI base of one body size.</summary>
    /// <param name="content">The content set of this build.</param>
    /// <param name="bodySize">The body size in frame pixels (D-707).</param>
    /// <returns>The atlas, the theme, and the text helper.</returns>
    /// <exception cref="ArgumentNullException">The content set is null (T-2).</exception>
    public static UiBase Load(ContentSet content, int bodySize)
    {
        ArgumentNullException.ThrowIfNull(content);

        GameAtlas atlas = GameAtlas.Load(content.Atlas);
        return new UiBase(content, atlas, UiTheme.Build(content, atlas, bodySize));
    }
}
