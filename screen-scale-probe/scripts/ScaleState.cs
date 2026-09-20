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
    private ScaleState(int worldHalves, int bodyNative, int bodyUnit, int titleNative, int titleUnit)
    {
        WorldHalves = worldHalves;
        BodyNative = bodyNative;
        BodyUnit = bodyUnit;
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

    /// <summary>
    /// The whole-number scale of a body glyph. A scale of 1 draws the finer strike and gives a
    /// stem of one frame pixel. That stem is one device pixel on the Deck, below the floor of
    /// D-639. The owner reads both kinds in this run.
    /// </summary>
    public int BodyUnit { get; }

    /// <summary>The height of a body line in frame pixels: 32, 48, or 64.</summary>
    public int BodyPixels => BodyNative * BodyUnit;

    /// <summary>The height of a title line in frame pixels: 48, 64, or 96.</summary>
    public int TitlePixels => TitleNative * TitleUnit;

    /// <summary>Frame pixels for each art pixel of the world.</summary>
    public double WorldScale => WorldHalves / 2.0;

    /// <summary>The width of a panel border. It follows the glyph, so both share one pixel grid.</summary>
    public int BorderPixels => BodyUnit;

    /// <summary>
    /// The unit of every margin and every gap, in frame pixels. It keeps the chrome in
    /// proportion to the text, and it holds the layout of the earlier runs at a body of 32.
    /// </summary>
    public int LayoutUnit => BodyPixels / 16;

    /// <summary>True when a body glyph pixel covers 2 frame pixels, the floor of D-639.</summary>
    public bool BodyMeetsFloor => BodyUnit >= 2;

    /// <summary>True when one art pixel of the world covers a whole number of frame pixels.</summary>
    public bool WorldIsExact => WorldHalves % 2 == 0;

    /// <summary>The name of the combination on the screen and in the report.</summary>
    public string Name =>
        $"world {WorldScale:0.#}x, body {BodyPixels} ({BodyNative}x{BodyUnit}),"
        + $" title {TitlePixels} ({TitleNative}x{TitleUnit})";

    /// <summary>The three combinations, in the order that one key steps through.</summary>
    public static ScaleState[] All { get; } = Build();

    private static ScaleState[] Build()
    {
        // The world draws at 2x on every screen, and no setting changes it (D-633).
        int[] worlds = [4];

        // Body 24, 32, and 48, and a title of twice the body. A body of 24 and a body of 32 draw
        // their own strike at 1x, for the finest glyph. A body of 48 doubles the 24 strike,
        // because 32 is the largest strike. Each title doubles or triples a strike.
        (int Body, int BodyUnit, int TitleNative, int TitleUnit)[] texts =
            [(24, 1, 24, 2), (32, 1, 32, 2), (24, 2, 32, 3)];

        var all = new List<ScaleState>(worlds.Length * texts.Length);
        foreach (int world in worlds)
        {
            foreach ((int body, int bodyUnit, int titleNative, int titleUnit) in texts)
            {
                all.Add(new ScaleState(world, body, bodyUnit, titleNative, titleUnit));
            }
        }

        return [.. all];
    }
}
