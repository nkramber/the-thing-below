using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Light;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Effects;

/// <summary>
/// The content that the checks of the ambient files read: the maps, the fight, the light, the
/// drawings, and the palette (D-523, D-886).
/// </summary>
/// <param name="Maps">Every map, by the value of its id.</param>
/// <param name="Battle">The battle content, for the enemies of each group.</param>
/// <param name="Light">The light content, for the torches, the carried light, and the budget.</param>
/// <param name="Drawings">Every drawing, by the value of its id.</param>
/// <param name="Palette">The palette (D-181).</param>
public sealed record AmbientWorld(
    SortedDictionary<string, GameMap> Maps,
    BattleContent Battle,
    LightContent Light,
    SortedDictionary<string, Drawing> Drawings,
    Palette Palette)
{
    /// <summary>Gives every palette key of the drawings of the ground that a map holds: each tile kind that the party can walk on.</summary>
    /// <param name="map">The map.</param>
    /// <returns>The keys, in the order of the key.</returns>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    /// <exception cref="ContentException">No drawing draws a tile kind of the ground of the map (T-2).</exception>
    public SortedSet<char> FloorKeysOf(GameMap map)
    {
        ArgumentNullException.ThrowIfNull(map);

        var kinds = new SortedSet<string>(StringComparer.Ordinal);
        for (int row = 0; row < map.Height; row += 1)
        {
            for (int column = 0; column < map.Width; column += 1)
            {
                TileKind kind = map.TileAt(new TilePoint(column, row));
                if (TileKinds.CanWalk(kind))
                {
                    kinds.Add(TileIds.Of(kind).Value);
                }
            }
        }

        var keys = new SortedSet<char>();
        foreach (string kind in kinds)
        {
            List<Drawing> drawn = this.DrawingsOf(kind);
            if (drawn.Count == 0)
            {
                throw ContentException.ForFile(Drawing.Folder, $"no drawing draws the tile '{kind}' of the map '{map.Id.Value}', and the fog test reads its colors (D-886)");
            }

            foreach (Drawing drawing in drawn)
            {
                keys.UnionWith(FogContrast.KeysOf(drawing));
            }
        }

        return keys;
    }

    /// <summary>
    /// Gives each drawing of an enemy that a map shows: the map sprite of each patrol, and the
    /// battle sprite of each enemy of the group of each patrol (D-738, D-885).
    /// </summary>
    /// <param name="map">The map.</param>
    /// <returns>The drawings, in the order of the id of the drawing.</returns>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    /// <exception cref="ContentException">A patrol names a group that the fight does not hold (T-2).</exception>
    public IReadOnlyList<Drawing> EnemyDrawingsOf(GameMap map)
    {
        ArgumentNullException.ThrowIfNull(map);

        var shown = new SortedSet<string>(StringComparer.Ordinal);
        foreach (Patrol patrol in map.Patrols)
        {
            shown.Add(patrol.Id.Value);

            // A patrol that names no group fails the check of D-766, which names the map and
            // the group. This test reads the enemies of the groups that the fight holds.
            foreach (GroupRecord group in this.Battle.Fixture.Groups)
            {
                if (string.CompareOrdinal(group.Id.Value, patrol.Group.Value) == 0)
                {
                    foreach (GroupEntry entry in group.Entries)
                    {
                        shown.Add(entry.Enemy.Value);
                    }
                }
            }
        }

        var drawings = new List<Drawing>();
        foreach (Drawing drawing in this.Drawings.Values)
        {
            if (DrawsAny(drawing, shown))
            {
                drawings.Add(drawing);
            }
        }

        return drawings;
    }

    private static bool DrawsAny(Drawing drawing, SortedSet<string> ids)
    {
        foreach (DrawingUse use in drawing.Draws)
        {
            if (ids.Contains(use.Content.Value))
            {
                return true;
            }
        }

        return false;
    }

    private List<Drawing> DrawingsOf(string content)
    {
        var drawn = new List<Drawing>();
        foreach (Drawing drawing in this.Drawings.Values)
        {
            if (DrawsAny(drawing, new SortedSet<string>(StringComparer.Ordinal) { content }))
            {
                drawn.Add(drawing);
            }
        }

        return drawn;
    }
}
