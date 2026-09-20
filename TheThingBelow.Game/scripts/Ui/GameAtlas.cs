using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The pages of the atlas as textures, built from the bytes of the Game assembly (D-508,
/// D-666). Game draws every sprite, tile, and UI part from these pages, and no Godot resource
/// file holds art (G-6).
/// </summary>
/// <remarks>
/// Each texture draws with the Nearest filter, which the project setting of F-45 sets for
/// every canvas texture. `ImageTexture.CreateFromImage` reports a failure in the log alone,
/// so the load checks its result right after the call (T-2, F-45).
/// </remarks>
public sealed class GameAtlas
{
    private readonly AtlasIndex index;
    private readonly SortedDictionary<string, Texture2D> pages;

    private GameAtlas(AtlasIndex index, SortedDictionary<string, Texture2D> pages)
    {
        this.index = index;
        this.pages = pages;
    }

    /// <summary>Loads every page of the atlas from the resources of the Game assembly.</summary>
    /// <param name="index">The atlas index of the content set (D-666).</param>
    /// <returns>The pages, ready to draw.</returns>
    /// <exception cref="ArgumentNullException">The index is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">A page does not load, or it is the wrong size (T-2).</exception>
    public static GameAtlas Load(AtlasIndex index)
    {
        ArgumentNullException.ThrowIfNull(index);

        // An ordinal order, so two platforms build the pages in one order (F-39, G-4).
        var pages = new SortedDictionary<string, Texture2D>(StringComparer.Ordinal);
        foreach (AtlasPage page in index.Pages)
        {
            pages.Add(page.Name, LoadPage(page));
        }

        return new GameAtlas(index, pages);
    }

    /// <summary>The index of the atlas, which holds the place of every frame (D-666).</summary>
    public AtlasIndex Index => this.index;

    /// <summary>Gives the texture of one page.</summary>
    /// <param name="name">The name of the page, such as `ui`.</param>
    /// <returns>The texture of the whole page.</returns>
    /// <exception cref="InvalidOperationException">The atlas holds no such page (T-2).</exception>
    public Texture2D Page(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        if (!this.pages.TryGetValue(name, out Texture2D? texture))
        {
            throw new InvalidOperationException($"The atlas holds no page '{name}' (T-2, D-666).");
        }

        return texture;
    }

    /// <summary>Gives the texture of one frame of one drawing.</summary>
    /// <param name="drawing">The id of the drawing, such as `drawing.ui_window_frame`.</param>
    /// <param name="frame">The number of the frame, which starts at zero.</param>
    /// <returns>A texture that shows that frame of the page.</returns>
    /// <exception cref="ArgumentNullException">The id is null (T-2).</exception>
    /// <exception cref="ContentException">The atlas holds no such drawing (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The drawing has no such frame (T-2).</exception>
    public AtlasTexture Frame(ContentId drawing, int frame)
    {
        ArgumentNullException.ThrowIfNull(drawing);

        AtlasEntry entry = this.index.Entry(drawing);
        ArgumentOutOfRangeException.ThrowIfNegative(frame);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(frame, entry.Frames.Count);

        AtlasFrame place = entry.Frames[frame];
        return new AtlasTexture
        {
            Atlas = this.Page(entry.Page),
            Region = new Rect2(place.X, place.Y, entry.Width, entry.Height),
            FilterClip = true,
        };
    }

    private static Texture2D LoadPage(AtlasPage page)
    {
        byte[] bytes = EmbeddedContent.ReadFile(page.File);

        var picture = new Image();
        Error read = picture.LoadPngFromBuffer(bytes);
        if (read != Error.Ok)
        {
            throw new InvalidOperationException(
                $"The page '{page.File}' of the Game assembly is not a PNG that Godot reads: {read} (T-2, D-508).");
        }

        if (picture.GetWidth() != page.Width || picture.GetHeight() != page.Height)
        {
            throw new InvalidOperationException(
                $"The page '{page.File}' is {picture.GetWidth()} by {picture.GetHeight()} pixels, "
                + $"and the atlas index holds {page.Width} by {page.Height}. Run the atlas command again (T-2, D-666).");
        }

        // The call reports a failure in the log alone, so the result takes a check (F-45, T-2).
        ImageTexture? texture = ImageTexture.CreateFromImage(picture);
        return texture ?? throw new InvalidOperationException(
            $"Godot made no texture from the page '{page.File}' (T-2, F-45).");
    }
}
