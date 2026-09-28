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

    /// <summary>The id of deep snow (D-1233).</summary>
    public static readonly ContentId Snow = ContentId.Parse("tile.snow", Source, nameof(Snow));

    /// <summary>The id of ice (D-1232).</summary>
    public static readonly ContentId Ice = ContentId.Parse("tile.ice", Source, nameof(Ice));

    /// <summary>The id of ground under bad air (D-1235).</summary>
    public static readonly ContentId BadAir = ContentId.Parse("tile.bad_air", Source, nameof(BadAir));

    /// <summary>The id of grass of the overworld (D-1256).</summary>
    public static readonly ContentId Grass = ContentId.Parse("tile.grass", Source, nameof(Grass));

    /// <summary>The id of forest of the overworld (D-1256).</summary>
    public static readonly ContentId Forest = ContentId.Parse("tile.forest", Source, nameof(Forest));

    /// <summary>The id of a mountain of the overworld (D-1256).</summary>
    public static readonly ContentId Mountain = ContentId.Parse("tile.mountain", Source, nameof(Mountain));

    /// <summary>The id of water of the overworld (D-1256).</summary>
    public static readonly ContentId Water = ContentId.Parse("tile.water", Source, nameof(Water));

    /// <summary>Gives the content id of one tile kind (D-519).</summary>
    /// <param name="kind">The kind of the tile.</param>
    /// <returns>The id that the drawing of that kind names.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind (T-2).</exception>
    public static ContentId Of(TileKind kind) => kind switch
    {
        TileKind.Floor => Floor,
        TileKind.Wall => Wall,
        TileKind.Doorway => Doorway,
        TileKind.Snow => Snow,
        TileKind.Ice => Ice,
        TileKind.BadAir => BadAir,
        TileKind.Grass => Grass,
        TileKind.Forest => Forest,
        TileKind.Mountain => Mountain,
        TileKind.Water => Water,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no tile kind (D-528)"),
    };
}
