using System;
using Godot;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The row of button prompts at the foot of a screen. Each prompt shows the glyph of the last
/// device that the player touched, and the label of that button (D-222, D-711).
/// </summary>
/// <remarks>
/// A glyph is a drawing of 16 by 16 pixels in the atlas, and it draws at the body size so it
/// stands beside its label (D-222, D-707). The row draws again when the player picks up
/// another device, which <see cref="LastDevice.Read"/> reports.
/// <para>
/// The labels come from the string table through the one text helper, so det-lint reads no
/// player string in this file (G-7, D-499).
/// </para>
/// </remarks>
public partial class PromptBar : HBoxContainer
{
    /// <summary>The count of frame pixels between a glyph and its label.</summary>
    public const int GapPixels = 4;

    /// <summary>The count of frame pixels between two prompts.</summary>
    public const int PromptGapPixels = 16;

    private UiBase? ui;

    /// <summary>Builds the row for one UI base, and draws every button of the device table.</summary>
    /// <param name="base">The atlas, the theme, the text helper, and the device tracker.</param>
    /// <exception cref="ArgumentNullException">The UI base is null (T-2).</exception>
    public void Build(UiBase @base)
    {
        ArgumentNullException.ThrowIfNull(@base);

        this.ui = @base;
        this.AddThemeConstantOverride("separation", PromptGapPixels);
        this.DrawPrompts();
    }

    /// <summary>Draws every prompt again in the glyph set of the last device (D-222).</summary>
    /// <remarks>The name is not `Draw`, because `CanvasItem` already holds that member.</remarks>
    /// <exception cref="InvalidOperationException">The row has no UI base yet (T-2).</exception>
    public void DrawPrompts()
    {
        UiBase @base = this.ui ?? throw new InvalidOperationException(
            $"The prompt row drew before {nameof(this.Build)} gave it the UI base (T-2).");

        foreach (Node old in this.GetChildren())
        {
            old.QueueFree();
            this.RemoveChild(old);
        }

        foreach (string prompt in @base.Devices.Prompts)
        {
            this.AddChild(this.BuildPrompt(@base, prompt));
        }
    }

    private Control BuildPrompt(UiBase @base, string prompt)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", GapPixels);

        int size = @base.Theme.BodySize;
        var glyph = new TextureRect
        {
            Texture = @base.GlyphOf(prompt),
            CustomMinimumSize = new Vector2(size, size),
            StretchMode = TextureRect.StretchModeEnum.Scale,
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
        };

        var label = new Label();
        @base.Text.Put(label, @base.LabelOf(prompt));

        row.AddChild(glyph);
        row.AddChild(label);
        return row;
    }
}
