using System;

namespace TheThingBelow.Core.Content;

/// <summary>One file of `content/`, as the host read it.</summary>
/// <remarks>
/// Core reads no file and no folder (G-1, D-100). Game gives the bytes from the resources of
/// its own assembly, and Tools gives them from the `content/` folder (D-508).
/// </remarks>
/// <param name="Path">
/// The path under `content/`, with `/` separators and no leading slash, such as
/// `rules/fixtures/light.json`. Two hosts on two platforms give the same path for one file,
/// so the content hash reads the same bytes everywhere (T-7).
/// </param>
/// <param name="Bytes">The bytes of the file, as the host read them.</param>
public sealed record ContentFile(string Path, byte[] Bytes);

/// <summary>The paths of `content/` that a rule of the project names.</summary>
public static class ContentPaths
{
    /// <summary>
    /// The one folder that holds every file a rule reads. The content hash covers this
    /// folder alone (D-495, D-648).
    /// </summary>
    public const string RuleFolder = "rules/";

    /// <summary>The start of the path of every page of the atlas (D-666).</summary>
    public const string AtlasPagePrefix = "sprites/atlas-";

    /// <summary>The folder that holds the two font files of the game (D-713).</summary>
    public const string FontFolder = "fonts/";

    /// <summary>The file type of every font file (D-263, D-264).</summary>
    public const string FontFileType = ".ttf";

    /// <summary>The folder that holds the files of the UI base (D-527).</summary>
    public const string UiFolder = "ui/";

    /// <summary>Tells whether a content path lies inside the rule folder.</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True when the content hash covers the file.</returns>
    public static bool IsRuleFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return path.StartsWith(RuleFolder, StringComparison.Ordinal);
    }

    /// <summary>Tells whether a content path is a page of the atlas (D-666).</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True when the path is a PNG of the atlas.</returns>
    /// <remarks>
    /// A page is an image, so no record of Core reads it. The atlas index records the size
    /// of each page and the place of each frame on it (D-517).
    /// </remarks>
    public static bool IsAtlasPage(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return path.StartsWith(AtlasPagePrefix, StringComparison.Ordinal) &&
            path.EndsWith(".png", StringComparison.Ordinal);
    }

    /// <summary>Tells whether a content path is a font file (D-713).</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True when the path is a font of the fonts folder.</returns>
    /// <remarks>
    /// A font file carries the glyph bitmaps, so no JSON record holds its content. The
    /// reader of <see cref="FontStrikes"/> reads the size of each bitmap strike, and the UI
    /// style file names the two fonts (D-517, D-527).
    /// </remarks>
    public static bool IsFontFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return path.StartsWith(FontFolder, StringComparison.Ordinal) &&
            path.EndsWith(FontFileType, StringComparison.Ordinal);
    }
}
