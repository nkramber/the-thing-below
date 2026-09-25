using System;
using Godot;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The pause of a fight: a dim over the whole frame, and "Paused" in the middle (D-1083). The
/// pause of a story scene takes the same look when PR-36 draws it (D-1009, D-1010).
/// </summary>
/// <remarks>
/// The view sits on the top layer of the frame, above the pass of the hand-off, so a pause
/// during a transition still shows its line. The line goes through the one text helper, so
/// det-lint reads no player string in this file (G-7, D-499).
/// </remarks>
public sealed class PauseView
{
    /// <summary>The string id of the line of the pause.</summary>
    public const string TitleId = "pause.title";

    /// <summary>The role of the dim color in the style file (D-527, D-1083).</summary>
    public const string DimRole = "pause_dim";

    /// <summary>The opacity of the dim, in hundredths, so the fight under it still reads.</summary>
    public const int DimPercent = 60;

    private readonly Control layer;

    /// <summary>Builds the pause over the frame, hidden.</summary>
    /// <param name="frame">The frame, whose top layer takes the pause.</param>
    /// <param name="ui">The theme and the text helper.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">The style file lacks the dim color, or the string table lacks the line (T-2).</exception>
    public PauseView(FrameRoot frame, UiBase ui)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(ui);

        Color dim = ui.Theme.ColorOf(DimRole);
        this.layer = new Control
        {
            Position = Vector2.Zero,
            Size = new Vector2(ScreenFit.FrameWidth, ScreenFit.FrameHeight),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Theme = ui.Theme.Theme,
            Visible = false,
        };
        this.layer.AddChild(new ColorRect
        {
            Position = Vector2.Zero,
            Size = new Vector2(ScreenFit.FrameWidth, ScreenFit.FrameHeight),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Color = new Color(dim.R, dim.G, dim.B, DimPercent / 100f),
        });

        var title = new Label
        {
            ThemeTypeVariation = UiTheme.TitleVariation,
            Position = Vector2.Zero,
            Size = new Vector2(ScreenFit.FrameWidth, ScreenFit.FrameHeight),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        ui.Text.Put(title, Id(TitleId));
        this.layer.AddChild(title);
        frame.Top.AddChild(this.layer);
    }

    /// <summary>True while the pause shows.</summary>
    public bool Shown => this.layer.Visible;

    /// <summary>Shows the pause or hides it.</summary>
    /// <param name="paused">True while the fight is paused (D-1083).</param>
    public void Show(bool paused) => this.layer.Visible = paused;

    /// <summary>Removes every node of the pause.</summary>
    public void Free() => this.layer.QueueFree();

    /// <summary>Gives the content id of the line of the pause.</summary>
    /// <param name="value">The string id.</param>
    /// <returns>The id.</returns>
    /// <exception cref="ContentException">The text is not a well-formed id (T-2).</exception>
    public static ContentId Id(string value) =>
        ContentId.Parse(value, "TheThingBelow.Game/scripts/Ui/PauseView.cs", nameof(PauseView));
}
