using System;

namespace TheThingBelow.Core.Maps;

/// <summary>The kind of the ground of one tile, which the terrain rows of a map give (D-528).</summary>
/// <remarks>
/// The terrain says what the ground is. A door, a chest, or a trap is a thing of the map
/// file, and it sits on a tile of the kind that its own rule names (D-386, D-528).
/// </remarks>
public enum TileKind
{
    /// <summary>Open ground. The party walks it, and sight passes over it.</summary>
    Floor,

    /// <summary>Solid rock or stone. The party cannot walk it, and it stops sight (D-718).</summary>
    Wall,

    /// <summary>The gap in a wall that holds a door or a lock. The party walks it, and sight passes.</summary>
    Doorway,
}

/// <summary>The character, the step rule, and the sight rule of each tile kind (D-515, D-528).</summary>
/// <remarks>
/// A map file writes its terrain as rows of characters, one character for one tile, as a
/// drawing file writes its pixels (D-165, D-515).
/// </remarks>
public static class TileKinds
{
    /// <summary>Open ground, as a terrain row writes it.</summary>
    public const char FloorCharacter = '.';

    /// <summary>A wall, as a terrain row writes it.</summary>
    public const char WallCharacter = '#';

    /// <summary>A doorway, as a terrain row writes it.</summary>
    public const char DoorwayCharacter = '+';

    /// <summary>The characters of every kind, for the error of an unknown character (T-2).</summary>
    public const string EveryCharacter = ".#+";

    /// <summary>Gives the kind of one character of a terrain row.</summary>
    /// <param name="character">The character, such as `#`.</param>
    /// <param name="kind">The kind of that character, when the character names one.</param>
    /// <returns>True when the character names a kind.</returns>
    public static bool TryOf(char character, out TileKind kind)
    {
        switch (character)
        {
            case FloorCharacter:
                kind = TileKind.Floor;
                return true;
            case WallCharacter:
                kind = TileKind.Wall;
                return true;
            case DoorwayCharacter:
                kind = TileKind.Doorway;
                return true;
            default:
                kind = TileKind.Floor;
                return false;
        }
    }

    /// <summary>Gives the character of one kind, which a terrain row and an error use (T-2).</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>The character of that kind.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind (T-2).</exception>
    public static char CharacterOf(TileKind kind) => kind switch
    {
        TileKind.Floor => FloorCharacter,
        TileKind.Wall => WallCharacter,
        TileKind.Doorway => DoorwayCharacter,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no tile kind (D-528)"),
    };

    /// <summary>Gives the name of one kind, for an error and for a log field (T-2).</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>The name, such as `wall`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind (T-2).</exception>
    public static string NameOf(TileKind kind) => kind switch
    {
        TileKind.Floor => "floor",
        TileKind.Wall => "wall",
        TileKind.Doorway => "doorway",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no tile kind (D-528)"),
    };

    /// <summary>Tells whether the party can stand on a tile of this kind.</summary>
    /// <param name="kind">The kind of the tile.</param>
    /// <returns>True when a step onto the tile holds.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind (T-2).</exception>
    public static bool CanWalk(TileKind kind) => kind switch
    {
        TileKind.Floor => true,
        TileKind.Wall => false,
        TileKind.Doorway => true,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no tile kind (D-528)"),
    };

    /// <summary>Tells whether a tile of this kind stops the sight of the party and of a patrol.</summary>
    /// <param name="kind">The kind of the tile.</param>
    /// <returns>True when sight cannot pass over the tile (D-718).</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind (T-2).</exception>
    /// <remarks>
    /// A wall stops sight, so a pillar and a corner give the player a place to hide from a
    /// patrol. The light of the screen never reaches this rule (G-1).
    /// </remarks>
    public static bool StopsSight(TileKind kind) => kind switch
    {
        TileKind.Floor => false,
        TileKind.Wall => true,
        TileKind.Doorway => false,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no tile kind (D-528)"),
    };
}
