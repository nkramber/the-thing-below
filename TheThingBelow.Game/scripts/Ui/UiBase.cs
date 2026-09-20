using System;
using Godot;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The pieces of the UI that every screen needs: the atlas, the theme, the text helper, and
/// the device tracker (D-499, D-527, D-711). One load builds them all from the content set.
/// </summary>
/// <remarks>
/// The body size sets the theme, so a change of the setting of D-707 builds a new
/// <see cref="UiBase"/> and every screen takes the new theme. PR-63 adds that setting.
/// </remarks>
public sealed class UiBase
{
    private readonly ContentSet content;

    private UiBase(ContentSet content, GameAtlas atlas, UiTheme theme)
    {
        this.content = content;
        this.Atlas = atlas;
        this.Theme = theme;
        this.Text = new TextHelper(content.Strings);
        this.Device = new LastDevice(content.Devices);
    }

    /// <summary>The pages of the atlas as textures (D-666).</summary>
    public GameAtlas Atlas { get; }

    /// <summary>The Godot theme of the body size in use (D-527).</summary>
    public UiTheme Theme { get; }

    /// <summary>The one helper that puts a player string on screen (G-7, D-499).</summary>
    public TextHelper Text { get; }

    /// <summary>The device that the player touched last (D-222, D-711).</summary>
    public LastDevice Device { get; }

    /// <summary>The device table of the content set, which names every button (D-711).</summary>
    public DeviceNames Devices => this.content.Devices;

    /// <summary>Builds the UI base of one body size.</summary>
    /// <param name="content">The content set of this build.</param>
    /// <param name="bodySize">The body size in frame pixels (D-707).</param>
    /// <returns>The atlas, the theme, the text helper, and the device tracker.</returns>
    /// <exception cref="ArgumentNullException">The content set is null (T-2).</exception>
    public static UiBase Load(ContentSet content, int bodySize)
    {
        ArgumentNullException.ThrowIfNull(content);

        GameAtlas atlas = GameAtlas.Load(content.Atlas);
        return new UiBase(content, atlas, UiTheme.Build(content, atlas, bodySize));
    }

    /// <summary>Gives the glyph of one button in the set of the last device (D-222, D-711).</summary>
    /// <param name="prompt">The name of the button, such as `confirm`.</param>
    /// <returns>The drawing of that button in the glyph set in use.</returns>
    /// <exception cref="ContentException">The build holds no such glyph (T-2).</exception>
    public AtlasTexture GlyphOf(string prompt)
    {
        ArgumentException.ThrowIfNullOrEmpty(prompt);

        string id = DeviceNames.GlyphDrawingId(this.Device.GlyphSet, prompt);
        return this.Atlas.Frame(ContentId.Parse(id, DeviceNames.Path, this.Device.GlyphSet), 0);
    }

    /// <summary>The string id of the label of one button, such as `ui.confirm` (G-7).</summary>
    /// <param name="prompt">The name of the button, such as `confirm`.</param>
    /// <returns>The string id.</returns>
    /// <exception cref="ContentException">The string table holds no label of that button (T-2).</exception>
    public ContentId LabelOf(string prompt)
    {
        ArgumentException.ThrowIfNullOrEmpty(prompt);

        ContentId id = ContentId.Parse($"ui.{prompt}", DeviceNames.Path, "prompts");
        if (!this.content.Strings.Contains(id))
        {
            throw ContentException.ForField(
                StringTable.Path,
                id.Value,
                $"the button '{prompt}' of the device table has no label in the string table (G-7)");
        }

        return id;
    }
}
