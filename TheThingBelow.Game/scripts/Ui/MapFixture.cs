using System;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;
using TheThingBelow.Core.Light;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The running screen of the game: the map in the world viewport (D-568, D-815). The play
/// session builds it, and the capture session of the screen-test job builds the same nodes
/// (D-172, D-734).
/// </summary>
/// <remarks>
/// The map draws in the world viewport at 1x, and the frame shows that viewport at 2x
/// (D-633, D-634). No row of button prompts draws on the frame, because the game shows no
/// button prompt (D-815).
/// </remarks>
public static class MapFixture
{
    /// <summary>Builds the map into one frame.</summary>
    /// <param name="frame">The frame that holds the world viewport and the UI layer.</param>
    /// <param name="base">The atlas and the theme.</param>
    /// <param name="run">The run, whose party names the map and the places, and whose torch sets the carried light (D-1064).</param>
    /// <param name="content">The content set, for the decor and the light (D-843).</param>
    /// <param name="ambient">The weather that the screen draws, or no value for the weather of the map (D-202, D-889).</param>
    /// <param name="seekParticles">True for a capture, which seeks each stream to the tick of the frame (D-172).</param>
    /// <param name="mode">The mode of the passes of the HD-2D look that a capture shows, or no value for the mode of the file (D-917).</param>
    /// <returns>The map, which the caller keeps to draw each later frame.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static MapScreen Build(
        FrameRoot frame,
        UiBase @base,
        GameRun run,
        ContentSet content,
        AmbientEffect? ambient = null,
        bool seekParticles = false,
        PassMode? mode = null)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(@base);
        ArgumentNullException.ThrowIfNull(run);
        ArgumentNullException.ThrowIfNull(content);

        MapState party = run.Party;
        frame.ShowGlow(content.Light.Glow);

        // A capture can show the other mode, so the owner reads both (D-917).
        Hd2dPasses passes = mode is PassMode shown ? content.Light.Passes.WithMode(shown) : content.Light.Passes;
        frame.ShowPasses(passes, content.Palette);
        var drawn = new MapScreen();
        frame.World.AddChild(drawn);
        drawn.Build(@base.Atlas, @base.Theme, party, content, ambient ?? content.Effects.Ambient.WeatherOf(party.Map.Id), passes);
        drawn.ShowParty(party, 0, run.Tick, run.TorchHeld);
        drawn.SeekParticles = seekParticles;
        drawn.ShowWeather(0, seekParticles);
        return drawn;
    }
}
