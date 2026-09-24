using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Battles;

/// <summary>
/// The weight of each term of the score of the evaluator (D-65, D-377, D-958). The score of an
/// action is the sum of each weight times its term, and the area file `area-battle.md` gives
/// the unit of each term.
/// </summary>
/// <param name="Damage">The weight of the expected health that the action takes from the other side.</param>
/// <param name="Kills">The weight of the expected kills, in basis points of one kill.</param>
/// <param name="Threat">The weight of the expected health that the reply of D-960 takes from this side. The score subtracts it.</param>
/// <param name="Healing">The weight of the health that the action restores to this side.</param>
/// <param name="Timeline">The weight of the push of the action on its user, in ticks. The score subtracts it.</param>
/// <param name="Row">The weight of the change in the count of this side that the melee of the other side cannot reach.</param>
public sealed record ScoreWeights(int Damage, int Kills, int Threat, int Healing, int Timeline, int Row);

/// <summary>One entry of a steal list: an item or some gold (D-383). PR-13 builds the steal (D-950, D-1044).</summary>
public abstract record StealEntry;

/// <summary>An item that a steal takes (D-383).</summary>
/// <param name="Item">The id of the item.</param>
public sealed record StealItem(ContentId Item) : StealEntry;

/// <summary>Some gold that a steal takes (D-383).</summary>
/// <param name="Gold">The amount of gold, above zero.</param>
public sealed record StealGold(int Gold) : StealEntry;

/// <summary>One entry of a drop list: an item and its own chance at a win (D-1042).</summary>
/// <param name="Item">The id of the item.</param>
/// <param name="Chance">The chance of the drop, in basis points, from 1 to 10000.</param>
public sealed record DropEntry(ContentId Item, int Chance);

/// <summary>
/// The personality profile of an enemy: the weights of the score terms, the base chance of a
/// steal, the steal list, and the drop list (D-65, D-383, D-949, D-956, D-958, D-1042). Each
/// profile has one file under `content/rules/profiles/`, and each entry of a group names a
/// profile id.
/// </summary>
/// <remarks>
/// PR-11 ships the weights alone, and the first trait comes with the first enemy that needs
/// one (D-958). PR-13 builds the steal action on the base chance and the list, and adds the
/// drop list (D-950, D-1042).
/// </remarks>
public sealed class ProfileRecord
{
    /// <summary>The folder of the profile files under the content folder (D-956).</summary>
    public const string Folder = "rules/profiles/";

    /// <summary>The kind of a profile id (D-646).</summary>
    public const string Kind = "profile";

    /// <summary>The largest weight of one term, which keeps each score inside a `long` (T-2).</summary>
    public const int MostWeight = BasisPoints.One;

    private ProfileRecord(string file, ContentId id, ScoreWeights weights, int stealChance, IReadOnlyList<StealEntry> steal, IReadOnlyList<DropEntry> drops)
    {
        this.File = file;
        this.Id = id;
        this.Weights = weights;
        this.StealChance = stealChance;
        this.Steal = steal;
        this.Drops = drops;
    }

    /// <summary>The path of the file, for an error that names this profile (T-2).</summary>
    public string File { get; }

    /// <summary>The id, of the kind `profile`.</summary>
    public ContentId Id { get; }

    /// <summary>The weight of each term of the score (D-958).</summary>
    public ScoreWeights Weights { get; }

    /// <summary>The base chance of a steal, in basis points (D-949). An empty list takes 0, and a list with an entry takes more.</summary>
    public int StealChance { get; }

    /// <summary>The entries that a steal can take, in the order of the file (D-383). The list can be empty.</summary>
    public IReadOnlyList<StealEntry> Steal { get; }

    /// <summary>The items that a win can drop, each with its own chance, in the order of the file (D-1042). The list can be empty.</summary>
    public IReadOnlyList<DropEntry> Drops { get; }

    /// <summary>Tells whether a content path is a profile file (D-956).</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True when the path is a JSON file of the profile folder.</returns>
    public static bool IsProfileFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return path.StartsWith(Folder, StringComparison.Ordinal) &&
            path.EndsWith(".json", StringComparison.Ordinal);
    }

    /// <summary>Reads one profile file (T-2).</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, for an error.</param>
    /// <returns>The profile.</returns>
    /// <exception cref="ContentException">
    /// A field is absent, unknown, repeated, or out of its range, a steal entry holds other
    /// than one item or one gold, or the base chance disagrees with the list (G-6, T-2).
    /// </exception>
    /// <remarks>The battle content checks each item of the steal list and of the drop list against the items (D-383, D-1042).</remarks>
    public static ProfileRecord Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        string? comment = null;
        ContentId? id = null;
        ScoreWeights? weights = null;
        int? stealChance = null;
        List<StealEntry>? steal = null;
        List<DropEntry>? drops = null;

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
                case "weights":
                    weights = ReadWeights(ref reader);
                    break;
                case "steal_chance":
                    stealChance = ReadChance(ref reader);
                    break;
                case "steal":
                    steal = ReadSteal(ref reader);
                    break;
                case "drops":
                    drops = ReadDrops(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        var profile = new ProfileRecord(
            file,
            reader.Require(id, depth, "id"),
            reader.Require(weights, depth, "weights"),
            reader.RequireInt(stealChance, depth, "steal_chance"),
            reader.Require(steal, depth, "steal"),
            reader.Require(drops, depth, "drops"));
        profile.RefuseChanceOfList(ref reader, depth);
        reader.ReadFileEnd();

        return profile;
    }

    private static ScoreWeights ReadWeights(ref ContentReader reader)
    {
        int? damage = null;
        int? kills = null;
        int? threat = null;
        int? healing = null;
        int? timeline = null;
        int? row = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "damage":
                    damage = ReadWeight(ref reader);
                    break;
                case "kills":
                    kills = ReadWeight(ref reader);
                    break;
                case "threat":
                    threat = ReadWeight(ref reader);
                    break;
                case "healing":
                    healing = ReadWeight(ref reader);
                    break;
                case "timeline":
                    timeline = ReadWeight(ref reader);
                    break;
                case "row":
                    row = ReadWeight(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new ScoreWeights(
            reader.RequireInt(damage, depth, "damage"),
            reader.RequireInt(kills, depth, "kills"),
            reader.RequireInt(threat, depth, "threat"),
            reader.RequireInt(healing, depth, "healing"),
            reader.RequireInt(timeline, depth, "timeline"),
            reader.RequireInt(row, depth, "row"));
    }

    private static int ReadWeight(ref ContentReader reader)
    {
        int value = reader.ReadInt();
        if (value < 0 || value > MostWeight)
        {
            throw reader.Refuse($"the weight {value} is outside 0 to {MostWeight} (T-2, D-958)");
        }

        return value;
    }

    private static int ReadChance(ref ContentReader reader)
    {
        int value = reader.ReadInt();
        if (value < 0 || value > BasisPoints.One)
        {
            throw reader.Refuse($"the chance {value} is outside 0 to {BasisPoints.One} basis points (T-2, D-949)");
        }

        return value;
    }

    private static List<StealEntry> ReadSteal(ref ContentReader reader)
    {
        List<StealEntry> entries = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, entries.Count))
        {
            entries.Add(ReadStealEntry(ref reader));
        }

        return entries;
    }

    private static List<DropEntry> ReadDrops(ref ContentReader reader)
    {
        List<DropEntry> entries = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, entries.Count))
        {
            entries.Add(ReadDropEntry(ref reader));
        }

        return entries;
    }

    /// <summary>Reads one drop entry: an item, and a chance from 1 to 10000 basis points (D-1042).</summary>
    private static DropEntry ReadDropEntry(ref ContentReader reader)
    {
        ContentId? item = null;
        int? chance = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "item":
                    item = reader.ReadContentId(ItemList.Kind);
                    break;
                case "chance":
                    chance = ReadChance(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        int readChance = reader.RequireInt(chance, depth, "chance");
        if (readChance == 0)
        {
            throw reader.RefuseField(depth, "chance", "the chance is 0, so the entry never drops (D-1042)");
        }

        return new DropEntry(reader.Require(item, depth, "item"), readChance);
    }

    private static StealEntry ReadStealEntry(ref ContentReader reader)
    {
        ContentId? item = null;
        int? gold = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "item":
                    item = reader.ReadContentId(ItemList.Kind);
                    break;
                case "gold":
                    gold = BattleFixture.ReadStat(ref reader, 1);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        if (item is not null && gold is null)
        {
            return new StealItem(item);
        }

        if (gold is int amount && item is null)
        {
            return new StealGold(amount);
        }

        throw reader.RefuseField(depth, "item", "a steal entry holds one item or one gold, and not both or neither (D-383)");
    }

    /// <summary>Refuses a base chance of 0 with a list that holds an entry, and a chance above 0 with an empty list (D-949).</summary>
    private void RefuseChanceOfList(ref ContentReader reader, int depth)
    {
        if (this.Steal.Count == 0 && this.StealChance != 0)
        {
            throw reader.RefuseField(depth, "steal_chance", $"the chance is {this.StealChance}, and the steal list is empty, so no steal can take an entry (D-949)");
        }

        if (this.Steal.Count > 0 && this.StealChance == 0)
        {
            throw reader.RefuseField(depth, "steal_chance", "the chance is 0, and the steal list holds an entry that no steal can take (D-949)");
        }
    }
}
