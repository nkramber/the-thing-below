using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Light;

/// <summary>One point light of a map, at its place in the art pixels of the map (D-843).</summary>
/// <param name="Id">The id of the piece that gives the light, or the id of the added light.</param>
/// <param name="X">The column of the center, in art pixels from the west edge of the map.</param>
/// <param name="Y">The row of the center, in art pixels from the north edge of the map.</param>
/// <param name="Light">The values of the light.</param>
public sealed record MapLight(ContentId Id, int X, int Y, PointLightValues Light);

/// <summary>
/// Gives the point lights of one map at one time of day, from its decor pieces and its light
/// setup (D-843, D-844). Game draws these lights, and the budget test counts them (D-842).
/// </summary>
/// <remarks>
/// One torch can take its light from two files: the default of its kind, and a change in the
/// light setup. The change wins, and a test holds that order (D-843, T-3).
/// </remarks>
public static class MapLighting
{
    /// <summary>Gives each point light of one map, pieces first in the order of the decor file, then each added light.</summary>
    /// <param name="decor">The decor file of the map.</param>
    /// <param name="kinds">Every decor kind, by the value of its id.</param>
    /// <param name="setup">The light setup of the map at its time of day.</param>
    /// <returns>The lights.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">A piece names a decor kind that no file holds (T-2).</exception>
    public static IReadOnlyList<MapLight> Resolve(
        DecorFile decor,
        IReadOnlyDictionary<string, DecorKind> kinds,
        LightSetup setup)
    {
        ArgumentNullException.ThrowIfNull(decor);
        ArgumentNullException.ThrowIfNull(kinds);
        ArgumentNullException.ThrowIfNull(setup);

        var lights = new List<MapLight>();
        foreach (DecorPiece piece in decor.Pieces)
        {
            if (!kinds.TryGetValue(piece.Kind.Value, out DecorKind? kind))
            {
                throw ContentException.ForField(
                    decor.File,
                    "pieces",
                    $"the piece '{piece.Id.Value}' names the decor kind '{piece.Kind.Value}', and no file of `{DecorKind.Folder}` holds it (D-843)");
            }

            lights.Add(new MapLight(
                piece.Id,
                checked((piece.Tile.X * AtlasPages.TileSize) + kind.LightX),
                checked((piece.Tile.Y * AtlasPages.TileSize) + kind.LightY),
                ChangeOf(setup, piece) ?? kind.Light));
        }

        const int Half = AtlasPages.TileSize / 2;
        foreach (AddedLight added in setup.Added)
        {
            lights.Add(new MapLight(
                added.Id,
                checked((added.Tile.X * AtlasPages.TileSize) + Half),
                checked((added.Tile.Y * AtlasPages.TileSize) + Half),
                added.Light));
        }

        return lights;
    }

    private static PointLightValues? ChangeOf(LightSetup setup, DecorPiece piece)
    {
        foreach (LightChange change in setup.Changes)
        {
            if (string.CompareOrdinal(change.Piece.Value, piece.Id.Value) == 0)
            {
                return change.Light;
            }
        }

        return null;
    }
}
