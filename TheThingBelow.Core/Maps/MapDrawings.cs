using System;
using TheThingBelow.Core.Light;

namespace TheThingBelow.Core.Maps;

/// <summary>
/// The uses of the map drawings, and the kinds of map thing that draw a sprite (D-519, D-1142,
/// D-1223). The map screen of Game and the map preview of Tools both read them (D-1317).
/// </summary>
/// <remarks>
/// No rule reads these values, so a change of one needs no new simulation version (G-17). A
/// rule file never names art, and the atlas index finds each drawing by a content id and a use
/// (D-519).
/// </remarks>
public static class MapDrawings
{
    /// <summary>The use that the map drawing of a character, an NPC, or an enemy serves (D-519).</summary>
    public const string FigureUse = "map_front";

    /// <summary>The use of the map drawing of the lead with the torch in its hand (D-1069).</summary>
    public const string TorchUse = "map_torch";

    /// <summary>
    /// The use of the map drawing of a thing, in its closed look for a thing with two looks
    /// (D-519, D-1142). A decor piece and a tile take the same use.
    /// </summary>
    public const string ThingUse = LightContent.MapUse;

    /// <summary>The use of the open drawing of a door, a chest, a gate, or a trap (D-1223).</summary>
    public const string OpenUse = "map_open";

    /// <summary>Tells whether a thing of one kind draws a sprite on the map (D-1142, D-1223).</summary>
    /// <param name="kind">The kind of the thing.</param>
    /// <returns>True for a service point, a save point, a door, a chest, a trap, an exit, an entrance, a gate, and a mark.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind (T-2).</exception>
    /// <remarks>A lock draws as its door, and a spawn point and a marker draw nothing.</remarks>
    public static bool Draws(MapThingKind kind) => kind switch
    {
        MapThingKind.ServicePoint or MapThingKind.SavePoint or MapThingKind.Door or MapThingKind.Chest or MapThingKind.Trap
            or MapThingKind.Exit or MapThingKind.Entrance or MapThingKind.Gate or MapThingKind.Mark => true,
        MapThingKind.Lock or MapThingKind.SpawnPoint or MapThingKind.Marker => false,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The value names no map thing kind (D-528, T-2)."),
    };

    /// <summary>Tells whether a thing of one kind takes an open look beside its closed look (D-1223).</summary>
    /// <param name="kind">The kind of the thing.</param>
    /// <returns>True for a door, a chest, a gate, and a trap, whose open look is its sprung look (D-1238, D-1243).</returns>
    public static bool HasOpenLook(MapThingKind kind) => kind is MapThingKind.Door or MapThingKind.Chest or MapThingKind.Trap or MapThingKind.Gate;
}
