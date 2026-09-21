using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Battles;

/// <summary>
/// The content that a battle reads: the rules file, the fixture file, the enemy records, and
/// the ability file (D-557, D-757, D-765, D-766, D-775, D-785, D-786). A run holds one, from
/// the content set of its build.
/// </summary>
public sealed class BattleContent
{
    /// <summary>Holds the battle files, and checks each id that one file names in another (T-2).</summary>
    /// <param name="rules">The numbers of the rules.</param>
    /// <param name="fixture">The characters, the groups, and the items.</param>
    /// <param name="enemies">The enemy records, one for each file, in the order of the paths (D-786).</param>
    /// <param name="abilities">The ability file (D-785).</param>
    /// <exception cref="ContentException">
    /// Two records take one enemy id, a record names an absent ability, or a group names an
    /// absent enemy. The error names the file and the id (T-2, D-166).
    /// </exception>
    public BattleContent(BattleRules rules, BattleFixture fixture, IReadOnlyList<EnemyRecord> enemies, AbilityList abilities)
    {
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(fixture);
        ArgumentNullException.ThrowIfNull(enemies);
        ArgumentNullException.ThrowIfNull(abilities);

        this.Rules = rules;
        this.Fixture = fixture;
        this.Enemies = enemies;
        this.Abilities = abilities;

        this.RefuseRepeatedEnemy();
        this.RefuseAbsentAbility();
        this.RefuseAbsentEnemy();
    }

    /// <summary>The numbers of the rules.</summary>
    public BattleRules Rules { get; }

    /// <summary>The characters, the groups, and the items.</summary>
    public BattleFixture Fixture { get; }

    /// <summary>Every enemy record, in the order of the paths (D-786).</summary>
    public IReadOnlyList<EnemyRecord> Enemies { get; }

    /// <summary>The ability file (D-785).</summary>
    public AbilityList Abilities { get; }

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
    /// <exception cref="ContentException">No enemy record has this id (T-2).</exception>
    public EnemyRecord Enemy(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        EnemyRecord? enemy = this.FindEnemy(id);
        return enemy ?? throw ContentException.ForField(EnemyRecord.Folder, id.Value, "no enemy record has this id (T-2, D-786)");
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

    private EnemyRecord? FindEnemy(ContentId id)
    {
        foreach (EnemyRecord enemy in this.Enemies)
        {
            if (string.CompareOrdinal(enemy.Id.Value, id.Value) == 0)
            {
                return enemy;
            }
        }

        return null;
    }

    /// <summary>
    /// Refuses two records with one id. The content set refuses a repeated id across every
    /// file, and this check holds the same rule for battle content that a test or the
    /// identity set builds (D-166).
    /// </summary>
    private void RefuseRepeatedEnemy()
    {
        var seen = new SortedSet<string>(StringComparer.Ordinal);
        foreach (EnemyRecord enemy in this.Enemies)
        {
            if (!seen.Add(enemy.Id.Value))
            {
                throw ContentException.ForField(enemy.File, enemy.Id.Value, "a second enemy record takes this id, and an id is permanent (D-166)");
            }
        }
    }

    /// <summary>Refuses a record that names an ability which the ability file lacks (exit test 3 of PR-80, D-785).</summary>
    private void RefuseAbsentAbility()
    {
        foreach (EnemyRecord enemy in this.Enemies)
        {
            foreach (ContentId ability in enemy.Abilities)
            {
                if (!this.Abilities.Holds(ability))
                {
                    throw ContentException.ForField(
                        enemy.File,
                        ability.Value,
                        $"the enemy '{enemy.Id.Value}' names this ability, and '{this.Abilities.File}' holds no such id (T-2, D-785)");
                }
            }
        }
    }

    /// <summary>Refuses a group that names an enemy with no record (T-2, D-786).</summary>
    private void RefuseAbsentEnemy()
    {
        foreach (GroupRecord group in this.Fixture.Groups)
        {
            foreach (GroupEntry entry in group.Entries)
            {
                if (this.FindEnemy(entry.Enemy) is null)
                {
                    throw ContentException.ForField(
                        BattleFixture.Path,
                        entry.Enemy.Value,
                        $"the group '{group.Id.Value}' names this enemy, and no file of '{EnemyRecord.Folder}' holds its record (T-2, D-786)");
                }
            }
        }
    }

    private static ContentException Absent(ContentId id, string kind) =>
        ContentException.ForField(BattleFixture.Path, id.Value, $"the fixture holds no {kind} with this id (T-2)");
}
