using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Core.Battles;

/// <summary>
/// The content that a battle reads: the rules file, the fixture file, the enemy records, the
/// ability file, the lesson file, the item file, the gear file, the group file of each region,
/// and the profiles (D-557, D-757, D-765, D-775, D-1026, D-785, D-786, D-956, D-957, D-1036,
/// D-1038). A run holds one, from the content set of its build.
/// </summary>
public sealed class BattleContent
{
    /// <summary>Holds the battle files, and checks each id that one file names in another (T-2).</summary>
    /// <param name="rules">The numbers of the rules.</param>
    /// <param name="fixture">The characters and the start of a run.</param>
    /// <param name="enemies">The enemy records, one for each file, in the order of the paths (D-786).</param>
    /// <param name="abilities">The ability file (D-785).</param>
    /// <param name="lessons">The lesson file (D-1026).</param>
    /// <param name="items">The item file (D-1038, D-1046).</param>
    /// <param name="gear">The gear file (D-1036).</param>
    /// <param name="groups">The group files, one for each region, in the order of the paths (D-957).</param>
    /// <param name="profiles">The profiles, one for each file, in the order of the paths (D-956).</param>
    /// <exception cref="ContentException">
    /// Two records take one id, two files take one region, a record names an absent ability,
    /// a group names an absent enemy or profile, the waiting column of a group is taller than the
    /// field, a steal list or a drop list names an absent item, the pack or the start gear names
    /// an absent id, passes a stack limit, or puts a piece in no slot of its kind, or an enemy of
    /// a group has no legal action. The error names the file and the id (T-2, D-166, D-948,
    /// D-963, D-1038).
    /// </exception>
    public BattleContent(
        BattleRules rules,
        BattleFixture fixture,
        IReadOnlyList<EnemyRecord> enemies,
        AbilityList abilities,
        LessonList lessons,
        ItemList items,
        GearList gear,
        IReadOnlyList<GroupFile> groups,
        IReadOnlyList<ProfileRecord> profiles)
    {
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(fixture);
        ArgumentNullException.ThrowIfNull(enemies);
        ArgumentNullException.ThrowIfNull(abilities);
        ArgumentNullException.ThrowIfNull(lessons);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(gear);
        ArgumentNullException.ThrowIfNull(groups);
        ArgumentNullException.ThrowIfNull(profiles);

        this.Rules = rules;
        this.Fixture = fixture;
        this.Enemies = enemies;
        this.Abilities = abilities;
        this.Lessons = lessons;
        this.Items = items;
        this.Gear = gear;
        this.GroupFiles = groups;
        this.Profiles = profiles;

        this.RefuseRepeatedEnemy();
        this.RefuseAbsentAbility();
        this.RefuseWrongLessonAbility();
        this.RefuseWrongStartLessons();
        this.RefuseRepeatedRegionOrGroup();
        this.RefuseRepeatedProfile();
        this.RefuseAbsentEnemyOrProfile();
        this.RefuseTallWaitingColumn();
        this.RefuseAbsentStealItem();
        this.RefuseAbsentDropItem();
        this.RefuseWrongStartGear();
        this.RefuseWrongPack();
        this.RefuseWrongTorch();
        this.RequireEveryEntryActs();
    }

    /// <summary>The numbers of the rules.</summary>
    public BattleRules Rules { get; }

    /// <summary>The characters and the start of a run.</summary>
    public BattleFixture Fixture { get; }

    /// <summary>Every enemy record, in the order of the paths (D-786).</summary>
    public IReadOnlyList<EnemyRecord> Enemies { get; }

    /// <summary>The ability file (D-785).</summary>
    public AbilityList Abilities { get; }

    /// <summary>The lesson file (D-1026).</summary>
    public LessonList Lessons { get; }

    /// <summary>The item file (D-1038, D-1046).</summary>
    public ItemList Items { get; }

    /// <summary>The gear file (D-1036).</summary>
    public GearList Gear { get; }

    /// <summary>The group file of each region, in the order of the paths (D-957).</summary>
    public IReadOnlyList<GroupFile> GroupFiles { get; }

    /// <summary>Every profile, in the order of the paths (D-956).</summary>
    public IReadOnlyList<ProfileRecord> Profiles { get; }

    /// <summary>
    /// Refuses an enemy of a group whose check fight gives no legal action (D-948, D-962).
    /// The load calls it for each entry of each group, and a test calls it with an empty list.
    /// </summary>
    /// <param name="actions">The legal actions of the enemy in its check fight.</param>
    /// <param name="file">The group file, for the error.</param>
    /// <param name="group">The group.</param>
    /// <param name="entry">The entry of the enemy.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">The list is empty. The error names the group, the enemy, and the profile (D-948).</exception>
    public static void RequireLegalAction(IReadOnlyList<EnemyAction> actions, string file, GroupRecord group, GroupEntry entry)
    {
        ArgumentNullException.ThrowIfNull(actions);
        ArgumentException.ThrowIfNullOrEmpty(file);
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(entry);

        if (actions.Count == 0)
        {
            throw ContentException.ForField(
                file,
                group.Id.Value,
                $"the enemy '{entry.Enemy.Value}' with the profile '{entry.Profile.Value}' has no legal action in its check fight, so it can never act (D-948, G-21)");
        }
    }

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

    /// <summary>Finds a group by id, in the group file of any region (D-957).</summary>
    /// <param name="id">The id, which a map names (D-753).</param>
    /// <returns>The record.</returns>
    /// <exception cref="ContentException">No group file holds such a group (T-2).</exception>
    public GroupRecord Group(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        foreach (GroupFile file in this.GroupFiles)
        {
            if (file.Find(id) is GroupRecord group)
            {
                return group;
            }
        }

        throw ContentException.ForField(GroupFile.Folder, id.Value, "no group file holds a group with this id (T-2, D-957)");
    }

    /// <summary>Finds a profile by id (D-956).</summary>
    /// <param name="id">The id, which an entry of a group names.</param>
    /// <returns>The record.</returns>
    /// <exception cref="ContentException">No profile file has this id (T-2).</exception>
    public ProfileRecord Profile(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return this.FindProfile(id)
            ?? throw ContentException.ForField(ProfileRecord.Folder, id.Value, "no profile file has this id (T-2, D-956)");
    }

    /// <summary>Finds an item by id (D-775, D-1038).</summary>
    /// <param name="id">The id.</param>
    /// <returns>The record.</returns>
    /// <exception cref="ContentException">The item file holds no such item (T-2).</exception>
    public ItemRecord Item(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return this.Items.Item(id);
    }

    /// <summary>Finds a piece of gear by id (D-1036).</summary>
    /// <param name="id">The id.</param>
    /// <returns>The record.</returns>
    /// <exception cref="ContentException">The gear file holds no such piece (T-2).</exception>
    public GearRecord Piece(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return this.Gear.Piece(id);
    }

    /// <summary>Gives the stack limit of an item or a piece of gear, by the kind of its id (D-1038).</summary>
    /// <param name="id">The id, of the kind `item` or `gear`.</param>
    /// <returns>The limit.</returns>
    /// <exception cref="ContentException">The item file or the gear file holds no such id, or the id is of another kind (T-2).</exception>
    public int LimitOf(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (string.CompareOrdinal(id.Kind, ItemList.Kind) == 0)
        {
            return this.Items.Item(id).Limit;
        }

        if (string.CompareOrdinal(id.Kind, GearList.Kind) == 0)
        {
            return this.Gear.Piece(id).Limit;
        }

        throw ContentException.ForField(BattleFixture.Path, id.Value, $"the id is of the kind '{id.Kind}', and the pack holds the kinds '{ItemList.Kind}' and '{GearList.Kind}' alone (D-1038)");
    }

    /// <summary>
    /// Refuses a map whose region has no group file, a map that names a group which the file
    /// of its region does not hold (D-957), and a map whose patrol size differs from the
    /// largest enemy record of its group (D-754, D-788).
    /// </summary>
    /// <param name="map">The map.</param>
    /// <exception cref="ContentException">
    /// The region of the map has no group file, an enemy of the map names a group absent from
    /// that file, or takes another size than the largest enemy of its group. The error names
    /// the map, the patrol, and the group, and a size error also names the enemy and both
    /// sizes (T-2).
    /// </exception>
    public void RequireGroupsOf(GameMap map)
    {
        ArgumentNullException.ThrowIfNull(map);

        GroupFile region = this.FindRegion(map.Region)
            ?? throw ContentException.ForField(
                map.File,
                "region",
                $"the map names the region '{map.Region.Value}', and no file of '{GroupFile.Folder}' holds it (D-957)");
        foreach (Patrol patrol in map.Patrols)
        {
            GroupRecord group = region.Find(patrol.Group)
                ?? throw ContentException.ForField(
                    map.File,
                    patrol.Id.Value,
                    $"the enemy names the group '{patrol.Group.Value}', and '{region.File}' of the region '{map.Region.Value}' holds no such group (D-753, D-957)");

            EnemyRecord largest = this.LargestOf(group, region.File);
            if (patrol.Size != largest.Size)
            {
                throw ContentException.ForField(
                    map.File,
                    patrol.Id.Value,
                    $"the patrol takes the size '{EnemySizes.NameOf(patrol.Size)}', and the largest enemy of the group '{patrol.Group.Value}' is '{largest.Id.Value}' with the size '{EnemySizes.NameOf(largest.Size)}' (D-754, D-788)");
            }
        }
    }

    /// <summary>
    /// Gives the enemy record of the largest body in a group, the waiting enemies included
    /// (D-788). Of two records with one size, the first in the group wins, so the error of a
    /// size names the same enemy on every machine (T-7).
    /// </summary>
    private EnemyRecord LargestOf(GroupRecord group, string file)
    {
        EnemyRecord? largest = null;
        foreach (GroupEntry entry in group.Entries)
        {
            EnemyRecord enemy = this.Enemy(entry.Enemy);

            // The order of the enum is the order of the bodies: common, elite, boss (D-236).
            if (largest is null || enemy.Size > largest.Size)
            {
                largest = enemy;
            }
        }

        // A group holds at least one enemy on the field, and the reader refuses an empty group (D-759).
        return largest ?? throw ContentException.ForField(file, group.Id.Value, "the group holds no enemy (D-759)");
    }

    private GroupFile? FindRegion(ContentId region)
    {
        foreach (GroupFile file in this.GroupFiles)
        {
            if (string.CompareOrdinal(file.Region.Value, region.Value) == 0)
            {
                return file;
            }
        }

        return null;
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

    private ProfileRecord? FindProfile(ContentId id)
    {
        foreach (ProfileRecord profile in this.Profiles)
        {
            if (string.CompareOrdinal(profile.Id.Value, id.Value) == 0)
            {
                return profile;
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

                // The evaluator of PR-11 scores a strike and a heal alone. A cure and a boon
                // serve the lessons of PR-12 (D-955, D-1029).
                if (this.Abilities.Ability(ability) is not (StrikeAbility or HealAbility))
                {
                    throw ContentException.ForField(
                        enemy.File,
                        ability.Value,
                        $"the enemy '{enemy.Id.Value}' names this ability, and an enemy move is a strike or a heal (D-955, D-1029)");
                }
            }
        }
    }

    /// <summary>Refuses a form of a lesson that names an absent ability (T-2, D-785, D-1026).</summary>
    private void RefuseWrongLessonAbility()
    {
        foreach (LessonRecord lesson in this.Lessons.Records)
        {
            foreach (LessonForm form in lesson.Forms)
            {
                if (!this.Abilities.Holds(form.Ability))
                {
                    throw ContentException.ForField(
                        this.Lessons.File,
                        lesson.Id.Value,
                        $"a form names the ability '{form.Ability.Value}', and '{this.Abilities.File}' holds no such id (T-2, D-785)");
                }
            }
        }
    }

    /// <summary>
    /// Refuses a start lesson or a lesson of the lesson pack that the lesson file does not hold,
    /// and a character that starts with more lessons than its slots at its join level (D-1018,
    /// D-1030).
    /// </summary>
    private void RefuseWrongStartLessons()
    {
        foreach (StartLessons entry in this.Fixture.StartLessons)
        {
            CharacterRecord character = this.Character(entry.Character);
            int slots = this.Rules.SlotsAt(character.JoinLevel);
            if (entry.Lessons.Count > slots)
            {
                throw ContentException.ForField(
                    BattleFixture.Path,
                    "start_lessons",
                    $"the character '{character.Id.Value}' starts with {entry.Lessons.Count} lessons, and it has {slots} slots at level {character.JoinLevel} (D-1018)");
            }

            foreach (ContentId lesson in entry.Lessons)
            {
                this.RequireLesson(lesson, "start_lessons");
            }
        }

        foreach (ContentId lesson in this.Fixture.LessonPack)
        {
            this.RequireLesson(lesson, "lesson_pack");
        }
    }

    private void RequireLesson(ContentId lesson, string field)
    {
        if (!this.Lessons.Holds(lesson))
        {
            throw ContentException.ForField(
                BattleFixture.Path,
                field,
                $"the fixture names the lesson '{lesson.Value}', and '{this.Lessons.File}' holds no such id (T-2, D-1026)");
        }
    }

    /// <summary>Refuses two group files of one region, and one group id in two files (D-166, D-957).</summary>
    private void RefuseRepeatedRegionOrGroup()
    {
        var regions = new SortedSet<string>(StringComparer.Ordinal);
        var groups = new SortedSet<string>(StringComparer.Ordinal);
        foreach (GroupFile file in this.GroupFiles)
        {
            if (!regions.Add(file.Region.Value))
            {
                throw ContentException.ForField(file.File, "region", $"a second group file takes the region '{file.Region.Value}', and a region has one file (D-957)");
            }

            foreach (GroupRecord group in file.Groups)
            {
                if (!groups.Add(group.Id.Value))
                {
                    throw ContentException.ForField(file.File, group.Id.Value, "a second group file takes this group id, and an id is permanent (D-166)");
                }
            }
        }
    }

    private void RefuseRepeatedProfile()
    {
        var seen = new SortedSet<string>(StringComparer.Ordinal);
        foreach (ProfileRecord profile in this.Profiles)
        {
            if (!seen.Add(profile.Id.Value))
            {
                throw ContentException.ForField(profile.File, profile.Id.Value, "a second profile file takes this id, and an id is permanent (D-166)");
            }
        }
    }

    /// <summary>Refuses a group that names an enemy with no record, or a profile with no file (T-2, D-786, D-956).</summary>
    private void RefuseAbsentEnemyOrProfile()
    {
        foreach (GroupFile file in this.GroupFiles)
        {
            foreach (GroupRecord group in file.Groups)
            {
                foreach (GroupEntry entry in group.Entries)
                {
                    if (this.FindEnemy(entry.Enemy) is null)
                    {
                        throw ContentException.ForField(
                            file.File,
                            entry.Enemy.Value,
                            $"the group '{group.Id.Value}' names this enemy, and no file of '{EnemyRecord.Folder}' holds its record (T-2, D-786)");
                    }

                    if (this.FindProfile(entry.Profile) is null)
                    {
                        throw ContentException.ForField(
                            file.File,
                            entry.Profile.Value,
                            $"the group '{group.Id.Value}' names this profile for '{entry.Enemy.Value}', and no file of '{ProfileRecord.Folder}' holds it (T-2, D-956)");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Refuses a group whose waiting enemies stand in a column taller than the room at the
    /// left edge of the field (D-953, D-963). Each waiting body takes the height of its size:
    /// 32 art pixels for a common enemy, 64 for an elite, and 96 for a boss (D-206).
    /// </summary>
    private void RefuseTallWaitingColumn()
    {
        foreach (GroupFile file in this.GroupFiles)
        {
            foreach (GroupRecord group in file.Groups)
            {
                int height = 0;
                foreach (GroupEntry entry in group.Entries)
                {
                    if (entry.Waits)
                    {
                        EnemyRecord enemy = this.Enemy(entry.Enemy);
                        height = checked(height + (EnemySizes.SideOf(enemy.Size) * AtlasPages.TileSize));
                    }
                }

                if (height > BattleFixture.MostWaitingHeight)
                {
                    throw ContentException.ForField(
                        file.File,
                        group.Id.Value,
                        $"the waiting enemies of the group stand in a column of {height} art pixels, and the field holds a column of {BattleFixture.MostWaitingHeight} at most. Split the wave (D-963, T-2)");
                }
            }
        }
    }

    /// <summary>
    /// Refuses a torch of another kind than a key item. The torch rule reads the kind at each
    /// press, so the load names the fault before the first press can meet it (D-1065, P3-18).
    /// </summary>
    private void RefuseWrongTorch()
    {
        if (this.Items.Holds(TorchRules.Torch) && this.Items.Item(TorchRules.Torch) is not KeyItem)
        {
            throw ContentException.ForField(
                ItemList.Path,
                "items",
                $"the item '{TorchRules.Torch.Value}' is not a key item, and the torch rule reads the kind `key` (D-1065)");
        }
    }

    /// <summary>Refuses a steal list that names an item or a piece of gear with no record (T-2, D-383, D-1051).</summary>
    private void RefuseAbsentStealItem()
    {
        foreach (ProfileRecord profile in this.Profiles)
        {
            foreach (StealEntry entry in profile.Steal)
            {
                if (entry is StealItem stolen && !this.Items.Holds(stolen.Item))
                {
                    throw ContentException.ForField(
                        profile.File,
                        stolen.Item.Value,
                        $"the steal list of '{profile.Id.Value}' names this item, and '{ItemList.Path}' holds no such item (T-2, D-383)");
                }

                if (entry is StealGear gear && !this.Gear.Holds(gear.Gear))
                {
                    throw ContentException.ForField(
                        profile.File,
                        gear.Gear.Value,
                        $"the steal list of '{profile.Id.Value}' names this piece, and '{GearList.Path}' holds no such piece (T-2, D-1051)");
                }
            }
        }
    }

    /// <summary>Refuses a drop list that names an item with no record (T-2, D-1042).</summary>
    private void RefuseAbsentDropItem()
    {
        foreach (ProfileRecord profile in this.Profiles)
        {
            foreach (DropEntry entry in profile.Drops)
            {
                if (!this.Items.Holds(entry.Item))
                {
                    throw ContentException.ForField(
                        profile.File,
                        entry.Item.Value,
                        $"the drop list of '{profile.Id.Value}' names this item, and '{ItemList.Path}' holds no such item (T-2, D-1042)");
                }
            }
        }
    }

    /// <summary>
    /// Refuses start gear that names an absent piece, or a piece with no empty slot of its
    /// kind: one weapon, one off-hand, one head, one body, and two accessories (D-44).
    /// </summary>
    private void RefuseWrongStartGear()
    {
        foreach (StartGear entry in this.Fixture.StartGear)
        {
            _ = GearRules.SlotsOf(entry.Gear, this.Gear, BattleFixture.Path, entry.Character.Value);
        }
    }

    /// <summary>
    /// Refuses a pack entry that names an absent item or piece, and a start that owns more
    /// copies than the stack limit: the pack and the start gear together (D-1038, D-1039).
    /// </summary>
    private void RefuseWrongPack()
    {
        var owned = new SortedDictionary<string, int>(StringComparer.Ordinal);
        foreach (PackEntry entry in this.Fixture.Pack)
        {
            _ = this.LimitOf(entry.Id);
            owned[entry.Id.Value] = entry.Count;
        }

        foreach (StartGear entry in this.Fixture.StartGear)
        {
            foreach (ContentId piece in entry.Gear)
            {
                owned[piece.Value] = (owned.TryGetValue(piece.Value, out int count) ? count : 0) + 1;
            }
        }

        foreach (KeyValuePair<string, int> entry in owned)
        {
            ContentId id = ContentId.Parse(entry.Key, BattleFixture.Path, "pack");
            int limit = this.LimitOf(id);
            if (entry.Value > limit)
            {
                throw ContentException.ForField(
                    BattleFixture.Path,
                    entry.Key,
                    $"the party starts with {entry.Value} copies, the pack and the start gear together, and the stack limit is {limit} (D-1038, D-1039)");
            }
        }
    }

    /// <summary>
    /// Builds the legal actions of each enemy of each group in its check fight, against the
    /// first character of the start party, and refuses an empty list (D-948, D-962).
    /// </summary>
    private void RequireEveryEntryActs()
    {
        CharacterRecord character = this.Character(this.Fixture.StartParty[0]);
        foreach (GroupFile file in this.GroupFiles)
        {
            foreach (GroupRecord group in file.Groups)
            {
                foreach (GroupEntry entry in group.Entries)
                {
                    EnemyRecord enemy = this.Enemy(entry.Enemy);
                    Battle check = Battle.CheckFight(group, entry, enemy, character);
                    RequireLegalAction(BattleEvaluator.LegalActions(check, check.Enemies[0], this), file.File, group, entry);
                }
            }
        }
    }

    private static ContentException Absent(ContentId id, string kind) =>
        ContentException.ForField(BattleFixture.Path, id.Value, $"the fixture holds no {kind} with this id (T-2)");
}
