using System;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Battles;

/// <summary>
/// The content that a battle reads: the rules file and the fixture file (D-757, D-765,
/// D-766, D-775). A run holds one, from the content set of its build.
/// </summary>
/// <param name="Rules">The numbers of the rules.</param>
/// <param name="Fixture">The characters, the enemies, the groups, and the items.</param>
public sealed record BattleContent(BattleRules Rules, BattleFixture Fixture)
{
    /// <summary>Finds a character by id.</summary>
    /// <param name="id">The id.</param>
    /// <returns>The record.</returns>
    /// <exception cref="ContentException">The fixture holds no such character (T-2).</exception>
    public CharacterRecord Character(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        foreach (CharacterRecord character in this.Fixture.Characters)
        {
            if (string.CompareOrdinal(character.Id.Value, id.Value) == 0)
            {
                return character;
            }
        }

        throw Absent(id, "character");
    }

    /// <summary>Finds an enemy by id.</summary>
    /// <param name="id">The id.</param>
    /// <returns>The record.</returns>
    /// <exception cref="ContentException">The fixture holds no such enemy (T-2).</exception>
    public EnemyRecord Enemy(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        foreach (EnemyRecord enemy in this.Fixture.Enemies)
        {
            if (string.CompareOrdinal(enemy.Id.Value, id.Value) == 0)
            {
                return enemy;
            }
        }

        throw Absent(id, "enemy");
    }

    /// <summary>Finds a group by id (D-766).</summary>
    /// <param name="id">The id, which a map names (D-753).</param>
    /// <returns>The record.</returns>
    /// <exception cref="ContentException">The fixture holds no such group (T-2).</exception>
    public GroupRecord Group(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        foreach (GroupRecord group in this.Fixture.Groups)
        {
            if (string.CompareOrdinal(group.Id.Value, id.Value) == 0)
            {
                return group;
            }
        }

        throw Absent(id, "group");
    }

    /// <summary>Finds an item by id (D-775).</summary>
    /// <param name="id">The id.</param>
    /// <returns>The record.</returns>
    /// <exception cref="ContentException">The fixture holds no such item (T-2).</exception>
    public ItemRecord Item(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        foreach (ItemRecord item in this.Fixture.Items)
        {
            if (string.CompareOrdinal(item.Id.Value, id.Value) == 0)
            {
                return item;
            }
        }

        throw Absent(id, "item");
    }

    /// <summary>Refuses a map that names a group which the fixture does not hold (D-766).</summary>
    /// <param name="map">The map.</param>
    /// <exception cref="ContentException">An enemy of the map names an absent group, and the error names the map, the enemy, and the group (T-2).</exception>
    public void RequireGroupsOf(GameMap map)
    {
        ArgumentNullException.ThrowIfNull(map);

        foreach (Patrol patrol in map.Patrols)
        {
            if (!this.HoldsGroup(patrol.Group))
            {
                throw ContentException.ForField(
                    $"{GameMap.Folder}{map.Id.Value}",
                    patrol.Id.Value,
                    $"the enemy names the group '{patrol.Group.Value}', and '{BattleFixture.Path}' holds no such group (D-753, D-766)");
            }
        }
    }

    private bool HoldsGroup(ContentId id)
    {
        foreach (GroupRecord group in this.Fixture.Groups)
        {
            if (string.CompareOrdinal(group.Id.Value, id.Value) == 0)
            {
                return true;
            }
        }

        return false;
    }

    private static ContentException Absent(ContentId id, string kind) =>
        ContentException.ForField(BattleFixture.Path, id.Value, $"the fixture holds no {kind} with this id (T-2)");
}
