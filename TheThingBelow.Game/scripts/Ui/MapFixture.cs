using System;
using Godot;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The running screen of the game: the map in the world viewport, and the row of button
/// prompts on the frame layer (D-568, D-722). The play session builds it, and the capture
/// session of the screen-test job builds the same nodes (D-172, D-734).
/// </summary>
/// <remarks>
/// The map draws in the world viewport at 1x, and the frame shows that viewport at 2x
/// (D-633, D-634). The prompts draw on the frame layer, so the text of a prompt matches the
/// art pixel of the frame (D-230).
/// </remarks>
/// <param name="Map">The map of the world viewport, which shows the party each frame.</param>
/// <param name="Prompts">The row of button prompts at the foot of the frame.</param>
public sealed record MapFixture(MapScreen Map, PromptBar Prompts)
{
    /// <summary>Builds the map and the row of prompts into one frame.</summary>
    /// <param name="frame">The frame that holds the world viewport and the UI layer.</param>
    /// <param name="base">The atlas, the theme, the text helper, and the device tracker.</param>
    /// <param name="party">The party of the run, which names the map and the places.</param>
    /// <returns>The two nodes, which the caller keeps to draw each later frame.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static MapFixture Build(FrameRoot frame, UiBase @base, MapState party)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(@base);
        ArgumentNullException.ThrowIfNull(party);

        var drawn = new MapScreen();
        frame.World.AddChild(drawn);
        drawn.Build(@base.Atlas, party.Map);
        drawn.ShowParty(party);

        var row = new PromptBar
        {
            Position = new Vector2(
                UiMetrics.EdgePixels,
                ScreenFit.FrameHeight - UiMetrics.EdgePixels - @base.Theme.BodySize),
        };

        frame.Layer.AddChild(row);
        row.Build(@base);
        return new MapFixture(drawn, row);
    }
}
