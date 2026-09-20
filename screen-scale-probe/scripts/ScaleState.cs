using System;
using System.Collections.Generic;

namespace ScreenScaleProbe;

/// <summary>
/// One of the nine combinations of the probe: a scale for the world, and a size for the text.
/// Every text size is a bitmap strike of Terminus at a whole-number scale, because the font
/// carries strikes at 12, 14, 16, 18, 20, 22, 24, 28, and 32 pixels alone (D-263, F-49). A size
/// with no strike falls back to the traced outline, which breaks the one-glyph-pixel rule (D-230).
/// </summary>
public sealed class ScaleState
{
    private ScaleState(int worldHalves, int bodyNative, int titleNative, int titleUnit)
    {
        WorldHalves = worldHalves;
        BodyNative = bodyNative;
        TitleNative = titleNative;
        TitleUnit = titleUnit;
    }

    /// <summary>Half-steps of the world scale: 2 is 1x, 3 is 1.5x, and 4 is 2x.</summary>
    public int WorldHalves { get; }

    /// <summary>The bitmap strike that draws the body text, in pixels.</summary>
    public int BodyNative { get; }

    /// <summary>The bitmap strike that draws the title text, in pixels.</summary>
    public int TitleNative { get; }

    /// <summary>The whole-number scale of a title glyph.</summary>
    public int TitleUnit { get; }

    /// <summary>The whole-number scale of a body glyph. D-639 takes the floor of 2 device pixels.</summary>
    public int BodyUnit => 2;

    /// <summary>The height of a body line in frame pixels: 32, 48, or 64.</summary>
    public int BodyPixels => BodyNative * BodyUnit;

    /// <summary>The height of a title line in frame pixels: 48, 64, or 96.</summary>
    public int TitlePixels => TitleNative * TitleUnit;

    /// <summary>Frame pixels for each art pixel of the world.</summary>
    public double WorldScale => WorldHalves / 2.0;

    /// <summary>
    /// The unit of every panel border and margin, in frame pixels. It keeps the chrome in
    /// proportion to the text, and it holds the layout of the earlier runs at a body of 32.
    /// The floor of 2 holds D-639 for a border: a border of one frame pixel is one device pixel
    /// on the Deck, where the fit is 1x.
    /// </summary>
    public int LayoutUnit => Math.Max(2, BodyPixels / 16);

    /// <summary>True when one art pixel of the world covers a whole number of frame pixels.</summary>
    public bool WorldIsExact => WorldHalves % 2 == 0;

    /// <summary>The name of the combination on the screen and in the report.</summary>
    public string Name =>
        $"world {WorldScale:0.#}x, body {BodyPixels} ({BodyNative}x{BodyUnit}),"
        + $" title {TitlePixels} ({TitleNative}x{TitleUnit})";

    /// <summary>The nine combinations, in the order that one key steps through.</summary>
    public static ScaleState[] All { get; } = Build();

    private static ScaleState[] Build()
    {
        int[] worlds = [2, 3, 4];

        // Body 24, 32, and 48, each a strike doubled. The title takes one step up the same ladder.
        (int Body, int TitleNative, int TitleUnit)[] texts = [(12, 16, 2), (16, 24, 2), (24, 32, 2)];

        var all = new List<ScaleState>(worlds.Length * texts.Length);
        foreach (int world in worlds)
        {
            foreach ((int body, int titleNative, int titleUnit) in texts)
            {
                all.Add(new ScaleState(world, body, titleNative, titleUnit));
            }
        }

        return [.. all];
    }
}
