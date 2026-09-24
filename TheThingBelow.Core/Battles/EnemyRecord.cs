using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Battles;

/// <summary>
/// The record of one enemy: its body size, its stats, the ids of its abilities, its element
/// table, and the statuses that it refuses (D-557, D-754, D-794, D-805). Each enemy has one file under `content/rules/enemies/` (D-786).
/// </summary>
/// <remarks>
/// The fight reads the stats alone. Each enemy keeps the basic attack until PR-11 picks an
/// action and PR-12 gives an ability its effect (D-787).
/// </remarks>
public sealed class EnemyRecord
{
    /// <summary>The folder of the enemy files under the content folder (D-786).</summary>
    public const string Folder = "rules/enemies/";

    /// <summary>The kind of an enemy id (D-646).</summary>
    public const string Kind = "enemy";

    private EnemyRecord(string file, ContentId id, EnemySize size, int level, int experience, int health, int attack, int magic, int defense, int resistance, int speed, IReadOnlyList<ContentId> abilities, ElementTable elements, IReadOnlyList<StatusKind> immune)
    {
        this.File = file;
        this.Id = id;
        this.Size = size;
        this.Level = level;
        this.Experience = experience;
        this.Health = health;
        this.Attack = attack;
        this.Magic = magic;
        this.Defense = defense;
        this.Resistance = resistance;
        this.Speed = speed;
        this.Abilities = abilities;
        this.Elements = elements;
        this.Immune = immune;
    }

    /// <summary>The path of the file, for an error that names this record (T-2).</summary>
    public string File { get; }

    /// <summary>The id, of the kind `enemy`.</summary>
    public ContentId Id { get; }

    /// <summary>The size of the body, which each map patrol of a group of this enemy agrees with (D-754, D-788).</summary>
    public EnemySize Size { get; }

    /// <summary>The level, which the shrink of the experience reads (D-968).</summary>
    public int Level { get; }

    /// <summary>The base experience, which a character at the level of the enemy or below it earns in full (D-968).</summary>
    public int Experience { get; }

    /// <summary>The full health.</summary>
    public int Health { get; }

    /// <summary>The attack, which a physical hit reads (D-771, D-1053).</summary>
    public int Attack { get; }

    /// <summary>The magic, which a magic hit and a heal read (D-1053, D-1057).</summary>
    public int Magic { get; }

    /// <summary>The defense, which guards against a physical hit (D-771, D-1053).</summary>
    public int Defense { get; }

    /// <summary>The resistance, which guards against a magic hit (D-1052, D-1053).</summary>
    public int Resistance { get; }

    /// <summary>The speed (D-768).</summary>
    public int Speed { get; }

    /// <summary>The ids of the abilities, in the order of the file. The list can be empty, because every enemy has the basic attack (D-787).</summary>
    public IReadOnlyList<ContentId> Abilities { get; }

    /// <summary>The affinity to each of the eight elements (D-74, D-794).</summary>
    public ElementTable Elements { get; }

    /// <summary>The statuses that do nothing to this enemy, in the order of the file. The list can be empty (D-805).</summary>
    public IReadOnlyList<StatusKind> Immune { get; }

    /// <summary>Tells whether a content path is an enemy file (D-786).</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True when the path is a JSON file of the enemy folder.</returns>
    public static bool IsEnemyFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return path.StartsWith(Folder, StringComparison.Ordinal) &&
            path.EndsWith(".json", StringComparison.Ordinal);
    }

    /// <summary>Reads one enemy file, and refuses an ability id that the record names two times (T-2).</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, for an error.</param>
    /// <returns>The record.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, repeated, or out of its range, or an ability id repeats, or a status of the immune list repeats (G-6, T-2).</exception>
    /// <remarks>The content set checks each ability id against the ability file (D-785).</remarks>
    public static EnemyRecord Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        string? comment = null;
        ContentId? id = null;
        EnemySize? size = null;
        int? level = null;
        int? experience = null;
        int? health = null;
        int? attack = null;
        int? magic = null;
        int? defense = null;
        int? resistance = null;
        int? speed = null;
        List<ContentId>? abilities = null;
        ElementTable? elements = null;
        IReadOnlyList<StatusKind>? immune = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "id":
                    id = reader.ReadContentId(Kind);
                    break;
                case "size":
                    size = ReadSize(ref reader);
                    break;
                case "level":
                    level = BattleFixture.ReadLevel(ref reader);
                    break;
                case "experience":
                    experience = BattleFixture.ReadStat(ref reader, 0);
                    break;
                case "health":
                    health = BattleFixture.ReadStat(ref reader, 1);
                    break;
                case "attack":
                    attack = BattleFixture.ReadStat(ref reader, 0);
                    break;
                case "magic":
                    magic = BattleFixture.ReadStat(ref reader, 0);
                    break;
                case "defense":
                    defense = BattleFixture.ReadStat(ref reader, 0);
                    break;
                case "resistance":
                    resistance = BattleFixture.ReadStat(ref reader, 0);
                    break;
                case "speed":
                    speed = BattleFixture.ReadStat(ref reader, 1);
                    break;
                case "abilities":
                    abilities = ReadAbilityIds(ref reader);
                    break;
                case "elements":
                    elements = ElementTable.Read(ref reader);
                    break;
                case "immune":
                    immune = Statuses.ReadList(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        var record = new EnemyRecord(
            file,
            reader.Require(id, depth, "id"),
            reader.RequireValue(size, depth, "size"),
            reader.RequireInt(level, depth, "level"),
            reader.RequireInt(experience, depth, "experience"),
            reader.RequireInt(health, depth, "health"),
            reader.RequireInt(attack, depth, "attack"),
            reader.RequireInt(magic, depth, "magic"),
            reader.RequireInt(defense, depth, "defense"),
            reader.RequireInt(resistance, depth, "resistance"),
            reader.RequireInt(speed, depth, "speed"),
            reader.Require(abilities, depth, "abilities"),
            reader.Require(elements, depth, "elements"),
            reader.Require(immune, depth, "immune"));
        reader.ReadFileEnd();

        record.RefuseRepeatedAbility();
        return record;
    }

    private static EnemySize ReadSize(ref ContentReader reader)
    {
        string name = reader.ReadString();
        if (!EnemySizes.TryOf(name, out EnemySize size))
        {
            throw reader.Refuse($"the size '{name}' is not one of {EnemySizes.EveryName} (D-206)");
        }

        return size;
    }

    private static List<ContentId> ReadAbilityIds(ref ContentReader reader)
    {
        List<ContentId> ids = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, ids.Count))
        {
            ids.Add(reader.ReadContentId(AbilityList.Kind));
        }

        return ids;
    }

    private void RefuseRepeatedAbility()
    {
        var seen = new SortedSet<string>(StringComparer.Ordinal);
        foreach (ContentId ability in this.Abilities)
        {
            if (!seen.Add(ability.Value))
            {
                throw ContentException.ForField(this.File, "abilities", $"the record names '{ability.Value}' two times");
            }
        }
    }
}
