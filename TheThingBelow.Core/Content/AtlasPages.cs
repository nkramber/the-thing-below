using System;

namespace TheThingBelow.Core.Content;

/// <summary>The kind of atlas page that one drawing belongs to (D-666).</summary>
/// <remarks>
/// The atlas takes one page for each kind, so a change to one kind rewrites one page and the
/// git history grows slower than one page for every drawing (D-666).
/// </remarks>
public enum AtlasPageKind
{
    /// <summary>A tile of a map, on the strict grid of D-667.</summary>
    Tiles,

    /// <summary>A party member, an enemy, or an NPC as the map draws it (D-199, D-207).</summary>
    MapSprites,

    /// <summary>A party member or an enemy as the battle draws it (D-200, D-236).</summary>
    BattleSprites,

    /// <summary>A portrait of the dialogue box, 64 by 64 pixels (D-234).</summary>
    Portraits,

    /// <summary>A window frame, an icon, or a glyph of the interface (D-214, D-220, D-222).</summary>
    Ui,
}

/// <summary>The sizes and the names of the atlas pages (D-666, D-667).</summary>
public static class AtlasPages
{
    /// <summary>The width and the height of one page, in pixels (D-666).</summary>
    /// <remarks>
    /// The Godot docs warn that a texture past 8192 by 8192 "may not be supported on older
    /// devices", and a page of this size stays far below that number. A kind that fills its
    /// page gains a second page (D-666).
    /// </remarks>
    public const int Size = 2048;

    /// <summary>The width and the height of one cell of a tile page, in pixels (D-228).</summary>
    public const int TileSize = 32;

    /// <summary>The count of cells in one row of a tile page.</summary>
    public const int TileColumns = Size / TileSize;

    /// <summary>The name of the kind, as a drawing file and the atlas index write it.</summary>
    /// <param name="kind">The kind of the page.</param>
    /// <returns>The name, such as `map_sprites`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind.</exception>
    public static string NameOf(AtlasPageKind kind) => kind switch
    {
        AtlasPageKind.Tiles => "tiles",
        AtlasPageKind.MapSprites => "map_sprites",
        AtlasPageKind.BattleSprites => "battle_sprites",
        AtlasPageKind.Portraits => "portraits",
        AtlasPageKind.Ui => "ui",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no atlas page kind (D-666)"),
    };

    /// <summary>Gives the kind of a name, and fails on any other text.</summary>
    /// <param name="name">The name that the file holds, such as `tiles`.</param>
    /// <param name="found">The kind, when the name is one of the five.</param>
    /// <returns>True when the name is one of the five kinds.</returns>
    public static bool TryParse(string name, out AtlasPageKind found)
    {
        ArgumentNullException.ThrowIfNull(name);

        switch (name)
        {
            case "tiles": found = AtlasPageKind.Tiles; return true;
            case "map_sprites": found = AtlasPageKind.MapSprites; return true;
            case "battle_sprites": found = AtlasPageKind.BattleSprites; return true;
            case "portraits": found = AtlasPageKind.Portraits; return true;
            case "ui": found = AtlasPageKind.Ui; return true;
            default: found = AtlasPageKind.Tiles; return false;
        }
    }

    /// <summary>Gives every name, in the order of the kinds, for an error message.</summary>
    /// <returns>The names, separated by a comma and a space.</returns>
    public static string Names() => "tiles, map_sprites, battle_sprites, portraits, ui";
}
