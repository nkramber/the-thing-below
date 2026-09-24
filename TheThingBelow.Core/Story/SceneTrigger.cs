using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Story;

/// <summary>What fires a story scene trigger (D-1004).</summary>
public enum TriggerKind
{
    /// <summary>The party arrives on a tile of the map.</summary>
    Tile,

    /// <summary>The player talks with an NPC. PR-14 adds the NPCs and fires this kind (D-1005).</summary>
    Talk,

    /// <summary>The party enters the map.</summary>
    Entry,

    /// <summary>The party wins a battle against a patrol of the map (D-1011).</summary>
    BattleEnd,
}

/// <summary>The names of the trigger kinds (D-1004).</summary>
public static class TriggerKinds
{
    /// <summary>Every kind, in one fixed order for a walk of them (G-4).</summary>
    public static readonly TriggerKind[] All = [TriggerKind.Tile, TriggerKind.Talk, TriggerKind.Entry, TriggerKind.BattleEnd];

    /// <summary>The names of every kind, for the error of an unknown name (T-2).</summary>
    public const string EveryName = "tile, talk, entry, battle_end";

    /// <summary>Gives the kind of one name.</summary>
    /// <param name="name">The name, such as `battle_end`.</param>
    /// <param name="kind">The kind of that name, when the name names one.</param>
    /// <returns>True when the name names a kind.</returns>
    /// <exception cref="ArgumentNullException">The name is null (T-2).</exception>
    public static bool TryOf(string name, out TriggerKind kind)
    {
        ArgumentNullException.ThrowIfNull(name);

        foreach (TriggerKind candidate in All)
        {
            if (string.CompareOrdinal(NameOf(candidate), name) == 0)
            {
                kind = candidate;
                return true;
            }
        }

        kind = TriggerKind.Tile;
        return false;
    }

    /// <summary>Gives the name of one kind, which a map file and an error use (T-2).</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>The name, such as `battle_end`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind (T-2).</exception>
    public static string NameOf(TriggerKind kind) => kind switch
    {
        TriggerKind.Tile => "tile",
        TriggerKind.Talk => "talk",
        TriggerKind.Entry => "entry",
        TriggerKind.BattleEnd => "battle_end",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no trigger kind (D-1004)"),
    };
}

/// <summary>
/// One story scene trigger of a map file: what fires it, the story scene that it starts, and
/// its condition (D-528, D-1004).
/// </summary>
/// <remarks>
/// A trigger fires each time its event happens and its condition holds. A story scene that
/// plays once sets a flag, and the condition of its trigger then refuses it (D-542).
/// </remarks>
/// <param name="Id">The permanent id of the trigger, of the kind `trigger` (D-166).</param>
/// <param name="Kind">What fires the trigger.</param>
/// <param name="Scene">The story scene that the trigger starts.</param>
/// <param name="Condition">The condition, which the always leaf writes for a trigger that no flag gates (D-1002).</param>
/// <param name="At">The tile of a tile trigger, or no value for every other kind.</param>
/// <param name="Npc">The NPC of a talk trigger, or no value for every other kind (D-1005).</param>
/// <param name="Patrol">The patrol of a battle end trigger, or no value for every other kind (D-1011).</param>
public sealed record SceneTrigger(
    ContentId Id,
    TriggerKind Kind,
    ContentId Scene,
    Condition Condition,
    TilePoint? At,
    ContentId? Npc,
    ContentId? Patrol)
{
    /// <summary>The kind of a trigger id (D-646).</summary>
    public const string IdKind = "trigger";

    /// <summary>The kind of an NPC id, which PR-14 gives its records (D-1005).</summary>
    public const string NpcKind = "npc";

    /// <summary>Reads the triggers array of a map file (D-528).</summary>
    /// <param name="reader">The reader, at the value of the triggers field.</param>
    /// <returns>Each trigger, in the order of the file.</returns>
    /// <exception cref="ContentException">A trigger breaks a rule of the reader (G-6, T-2).</exception>
    /// <remarks>
    /// The map checks the tile and the patrol of each trigger against its own terrain and its
    /// own enemies. The content set checks the story scene and the flags (T-2).
    /// </remarks>
    public static List<SceneTrigger> ReadAll(ref ContentReader reader)
    {
        List<SceneTrigger> triggers = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, triggers.Count))
        {
            triggers.Add(Read(ref reader));
        }

        return triggers;
    }

    private static SceneTrigger Read(ref ContentReader reader)
    {
        ContentId? id = null;
        string? kind = null;
        ContentId? scene = null;
        Condition? condition = null;
        int? x = null;
        int? y = null;
        ContentId? npc = null;
        ContentId? patrol = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(IdKind);
                    break;
                case "kind":
                    kind = reader.ReadString();
                    break;
                case "scene":
                    scene = reader.ReadContentId(StoryScene.Kind);
                    break;
                case "condition":
                    condition = Condition.Read(ref reader);
                    break;
                case "x":
                    x = reader.ReadInt();
                    break;
                case "y":
                    y = reader.ReadInt();
                    break;
                case "npc":
                    npc = reader.ReadContentId(NpcKind);
                    break;
                case "patrol":
                    patrol = reader.ReadContentId(Maps.Patrol.IdKind);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        string name = reader.Require(kind, depth, "kind");
        if (!TriggerKinds.TryOf(name, out TriggerKind parsed))
        {
            throw reader.RefuseField(depth, "kind", $"the trigger takes the kind '{name}', and a trigger takes one of {TriggerKinds.EveryName} (D-1004)");
        }

        // Each kind reads its own fields alone, so a field of another kind never passes in
        // silence (T-2).
        bool tile = parsed == TriggerKind.Tile;
        RefuseField(ref reader, depth, "x", x.HasValue, tile, parsed);
        RefuseField(ref reader, depth, "y", y.HasValue, tile, parsed);
        RefuseField(ref reader, depth, "npc", npc is not null, parsed == TriggerKind.Talk, parsed);
        RefuseField(ref reader, depth, "patrol", patrol is not null, parsed == TriggerKind.BattleEnd, parsed);

        return new SceneTrigger(
            reader.Require(id, depth, "id"),
            parsed,
            reader.Require(scene, depth, "scene"),
            reader.Require(condition, depth, "condition"),
            tile ? new TilePoint(x!.Value, y!.Value) : null,
            npc,
            patrol);
    }

    private static void RefuseField(ref ContentReader reader, int depth, string field, bool present, bool wanted, TriggerKind kind)
    {
        if (present && !wanted)
        {
            throw reader.RefuseField(depth, field, $"a trigger of the kind '{TriggerKinds.NameOf(kind)}' reads no field '{field}' (T-2)");
        }

        if (!present && wanted)
        {
            throw reader.RefuseField(depth, field, "the field is absent");
        }
    }
}
