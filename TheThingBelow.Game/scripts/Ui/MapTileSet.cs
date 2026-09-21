using System;
using Godot;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The tile set of a map, built from the tile page of the atlas at load (D-667). No Godot
/// resource file holds art, so this code builds the set from the bytes of the assembly
/// (D-508, G-6).
/// </summary>
/// <remarks>
/// A tile page is a strict grid of 32 by 32 cells with no gap and no margin, so the place of
/// a frame on the page gives the cell of that tile with no translation (D-667).
/// <para>
/// Four Godot defaults meet a tile map, and this code sets each one (F-51). `TileSet.TileSize`
/// and `TileSetAtlasSource.TextureRegionSize` both default to 16 by 16, and the tiles of this
/// game are 32 by 32 (D-228). The collisions and the navigation of a layer both default to
/// on, and `MapScreen` turns both off, because no rule of Core reads them (G-1,
/// G-23).
/// </para>
/// </remarks>
public static class MapTileSet
{
    /// <summary>The width and the height of one tile, in art pixels (D-228, D-667).</summary>
    public const int TilePixels = AtlasPages.TileSize;

    /// <summary>The number of the one atlas source of the set.</summary>
    public const int SourceId = 0;

    /// <summary>Builds the tile set of every tile kind from the tile page (D-667).</summary>
    /// <param name="atlas">The pages of the atlas, as textures (D-666).</param>
    /// <returns>The set, with one tile for each kind of <see cref="TileKind"/>.</returns>
    /// <exception cref="ArgumentNullException">The atlas is null (T-2).</exception>
    /// <exception cref="ContentException">The atlas holds no drawing of a tile kind (T-2).</exception>
    /// <exception cref="InvalidOperationException">A call of the engine made no tile (T-2, F-45).</exception>
    public static TileSet Build(GameAtlas atlas)
    {
        ArgumentNullException.ThrowIfNull(atlas);

        var source = new TileSetAtlasSource
        {
            Texture = atlas.Page(AtlasPages.NameOf(AtlasPageKind.Tiles)),
            TextureRegionSize = new Vector2I(TilePixels, TilePixels),
        };

        foreach (TileKind kind in new[] { TileKind.Floor, TileKind.Wall, TileKind.Doorway })
        {
            Vector2I cell = CellOf(atlas, kind);

            // `CreateTile` returns no value, and it reports a failure in the log alone. Thus
            // the result takes a check right after the call (T-2, F-45, D-667).
            source.CreateTile(cell);
            if (!source.HasTile(cell))
            {
                throw new InvalidOperationException(
                    $"Godot made no tile at the cell {cell} of the tile page for '{TileIds.Of(kind).Value}' (T-2, D-667).");
            }
        }

        var set = new TileSet { TileSize = new Vector2I(TilePixels, TilePixels) };
        set.AddSource(source, SourceId);
        return set;
    }

    /// <summary>Gives the cell of one tile kind on the tile page (D-667).</summary>
    /// <param name="atlas">The pages of the atlas.</param>
    /// <param name="kind">The kind of the tile.</param>
    /// <returns>The column and the row of that tile on the page.</returns>
    /// <exception cref="ArgumentNullException">The atlas is null (T-2).</exception>
    /// <exception cref="ContentException">The atlas holds no drawing of that kind (T-2).</exception>
    /// <exception cref="InvalidOperationException">The drawing does not sit on the grid of a tile page (T-2).</exception>
    public static Vector2I CellOf(GameAtlas atlas, TileKind kind)
    {
        ArgumentNullException.ThrowIfNull(atlas);

        AtlasEntry entry = atlas.Index.Entry(TileIds.Of(kind), TileIds.MapUse);
        AtlasFrame place = entry.Frames[0];
        if (place.X % TilePixels != 0 || place.Y % TilePixels != 0 ||
            entry.Width != TilePixels || entry.Height != TilePixels)
        {
            throw new InvalidOperationException(
                $"The drawing '{entry.Id.Value}' is {entry.Width} by {entry.Height} pixels at ({place.X}, {place.Y}), "
                + $"and a tile page holds a strict grid of {TilePixels} by {TilePixels} cells (T-2, D-667).");
        }

        return new Vector2I(place.X / TilePixels, place.Y / TilePixels);
    }
}
