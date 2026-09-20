using System;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Maps;

/// <summary>The content id of each tile kind, which the art of a tile names (D-519, D-646).</summary>
/// <remarks>
/// A drawing file names the content ids that it draws, and a rule file never names art
/// (D-519). Thus a map file writes its terrain as characters, and the atlas index holds the
/// drawing of each tile kind under the id below (D-515, D-667).
/// </remarks>
public static class TileIds
{
    /// <summary>The kind of every tile id (D-646).</summary>
    public const string Kind = "tile";

    /// <summary>The use that a drawing of a tile serves, as the atlas index writes it (D-519).</summary>
    public const string MapUse = "map";

    /// <summary>The file that holds these ids, for the error of a malformed id (T-2).</summary>
    private const string Source = "TheThingBelow.Core/Maps/TileIds.cs";

    /// <summary>The id of open ground.</summary>
    public static readonly ContentId Floor = ContentId.Parse("tile.floor", Source, nameof(Floor));

    /// <summary>The id of a wall.</summary>
    public static readonly ContentId Wall = ContentId.Parse("tile.wall", Source, nameof(Wall));

    /// <summary>The id of a doorway.</summary>
    public static readonly ContentId Doorway = ContentId.Parse("tile.doorway", Source, nameof(Doorway));

    /// <summary>Gives the content id of one tile kind (D-519).</summary>
    /// <param name="kind">The kind of the tile.</param>
    /// <returns>The id that the drawing of that kind names.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind (T-2).</exception>
    public static ContentId Of(TileKind kind) => kind switch
    {
        TileKind.Floor => Floor,
        TileKind.Wall => Wall,
        TileKind.Doorway => Doorway,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no tile kind (D-528)"),
    };
}
