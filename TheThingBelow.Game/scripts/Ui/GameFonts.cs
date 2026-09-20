using System;
using Godot;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The fonts of the game, built from the bytes of the Game assembly (D-263, D-264, D-508,
/// D-710). The body is Terminus TTF, and the title is Terminus TTF Bold.
/// </summary>
/// <remarks>
/// Godot 4.7.2 has no method that loads a font from a byte array, so Game makes an empty
/// `FontFile` and writes the bytes into its data (F-49). The file then lives inside the
/// assembly, as every other content file does, and the export needs no filter (D-508).
/// <para>
/// WARNING. A size with no bitmap strike falls back to the traced outline in silence, and the
/// glyph loses its square pixel. The fallback looks almost right (F-49). Thus each font here
/// pins one strike with `FixedSize`, and `FixedSizeScaleMode` scales that strike by a whole
/// number alone. A title of 48 is then the strike of 24 at a scale of 2 (D-707, D-710).
/// </para>
/// </remarks>
public static class GameFonts
{
    /// <summary>Builds one font of the game, pinned to one bitmap strike.</summary>
    /// <param name="contentPath">The path of the font under `content/`, such as `fonts/TerminusTTF.ttf`.</param>
    /// <param name="strikes">The bitmap sizes of that file, which the content set read (D-710).</param>
    /// <param name="strike">The bitmap size to pin, in pixels.</param>
    /// <returns>The font, with the six settings of a pixel font.</returns>
    /// <exception cref="ArgumentNullException">The strikes are null (T-2).</exception>
    /// <exception cref="ContentException">The file carries no bitmap of that size (T-2, F-49).</exception>
    /// <exception cref="InvalidOperationException">The assembly carries no such font (T-2).</exception>
    public static FontFile Build(string contentPath, FontStrikes strikes, int strike)
    {
        ArgumentException.ThrowIfNullOrEmpty(contentPath);
        ArgumentNullException.ThrowIfNull(strikes);

        strikes.RequireSize(strike);

        var font = new FontFile
        {
            // Each setting below fights a Godot default that suits a vector font (D-710, F-49).
            Antialiasing = TextServer.FontAntialiasing.None,
            Hinting = TextServer.Hinting.None,
            SubpixelPositioning = TextServer.SubpixelPositioning.Disabled,
            MultichannelSignedDistanceField = false,
            GenerateMipmaps = false,

            // The rasterizer draws this strike alone, and a larger size repeats each pixel a
            // whole number of times. No size ever reaches the traced outline (F-49).
            FixedSize = strike,
            FixedSizeScaleMode = TextServer.FixedSizeScaleMode.IntegerOnly,

            // A font of the system carries no bitmap of this game, so a fallback would draw
            // a smooth glyph beside a hard one (T-2).
            AllowSystemFallback = false,
            Data = EmbeddedContent.ReadFile(contentPath),
        };

        if (font.Data.Length == 0)
        {
            throw new InvalidOperationException(
                $"The font '{contentPath}' of the Game assembly holds no bytes (T-2, D-508).");
        }

        return font;
    }

    /// <summary>
    /// Gives the bitmap strike that draws one size, and the whole number that scales it
    /// (D-710). The method reads no engine value, so a test drives it with no engine (D-614).
    /// </summary>
    /// <param name="strikes">The bitmap sizes of the font file.</param>
    /// <param name="wanted">The size that the layout wants, in frame pixels.</param>
    /// <returns>The strike to pin, and the whole number that scales it to the wanted size.</returns>
    /// <exception cref="ArgumentNullException">The strikes are null (T-2).</exception>
    /// <exception cref="ContentException">
    /// No whole multiple of a strike gives the wanted size, so the glyph would draw from the
    /// traced outline (F-49, T-2).
    /// </exception>
    /// <remarks>
    /// The search takes the largest strike first, because a larger strike carries the shape
    /// that its designer drew and a scaled smaller one repeats pixels. A title of 48 takes
    /// the strike of 24 at a scale of 2, and a title of 64 takes the strike of 32 (D-707).
    /// </remarks>
    public static (int Strike, int Scale) StrikeFor(FontStrikes strikes, int wanted)
    {
        ArgumentNullException.ThrowIfNull(strikes);
        ArgumentOutOfRangeException.ThrowIfLessThan(wanted, 1);

        for (int scale = 1; scale <= wanted; scale += 1)
        {
            if (wanted % scale == 0 && strikes.Has(wanted / scale))
            {
                return (wanted / scale, scale);
            }
        }

        throw ContentException.ForField(
            strikes.File,
            "size",
            $"no whole multiple of a bitmap of this font gives {wanted} pixels, and the font carries {strikes.Describe()}. "
            + "A size with no bitmap draws the traced outline, which loses the square pixel (F-49, D-710)");
    }
}
