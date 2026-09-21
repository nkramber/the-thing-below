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
/// <para>
/// A page that takes scene light is a `CanvasTexture` of the color page and its normal-map
/// page, so each sprite, tile, and piece catches light on the side that faces it (D-183,
/// D-184). Both pages draw with the Nearest filter (F-45). A page of portraits or of the UI
/// takes no scene light and has no normal map (D-210).
/// </para>
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
            Texture2D color = LoadPage(page, page.File);
            pages.Add(page.Name, AtlasPages.TakesLight(page.Kind) ? Lit(color, LoadPage(page, page.NormalFile)) : color);
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

    /// <summary>Joins a color page and its normal-map page into one texture that takes scene light (D-183).</summary>
    /// <remarks>
    /// The canvas texture takes the Linear filter by default, and pixel art takes the Nearest
    /// filter (F-45). Its default shininess of 1.0 gives no specular light, and no decision asks
    /// for one (D-183).
    /// </remarks>
    private static CanvasTexture Lit(Texture2D color, Texture2D normal) => new()
    {
        DiffuseTexture = color,
        NormalTexture = normal,
        TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
    };

    private static Texture2D LoadPage(AtlasPage page, string file)
    {
        byte[] bytes = EmbeddedContent.ReadFile(file);

        var picture = new Image();
        Error read = picture.LoadPngFromBuffer(bytes);
        if (read != Error.Ok)
        {
            throw new InvalidOperationException(
                $"The page '{file}' of the Game assembly is not a PNG that Godot reads: {read} (T-2, D-508).");
        }

        if (picture.GetWidth() != page.Width || picture.GetHeight() != page.Height)
        {
            throw new InvalidOperationException(
                $"The page '{file}' is {picture.GetWidth()} by {picture.GetHeight()} pixels, "
                + $"and the atlas index holds {page.Width} by {page.Height}. Run the atlas command again (T-2, D-666).");
        }

        // The call reports a failure in the log alone, so the result takes a check (F-45, T-2).
        ImageTexture? texture = ImageTexture.CreateFromImage(picture);
        return texture ?? throw new InvalidOperationException(
            $"Godot made no texture from the page '{file}' (T-2, F-45).");
    }
}
