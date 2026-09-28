using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Core.Maps;

/// <summary>One group of a zone and its weight in the pick of a fight (D-1250).</summary>
/// <param name="Group">The id of the enemy group, which the group file of the region holds (D-957).</param>
/// <param name="Weight">The weight of the group, from 1. A group of weight 3 comes three times as often as one of weight 1.</param>
public sealed record ZoneGroup(ContentId Group, int Weight);

/// <summary>
/// One zone of the overworld: its key on the zone grid, its region, its rate, its weighted groups,
/// and its condition (D-1250, D-1251, D-1262, D-1285).
/// </summary>
/// <remarks>
/// Each step onto a tile of a live zone adds the rate to the danger count of the run, and the
/// step then draws against the count (D-1249, D-1261). A zone at rate zero, or a zone whose
/// condition fails, keeps the count and draws nothing (D-1263).
/// <para>
/// A zone at rate zero holds no group, and a zone above rate zero holds one group or more. A
/// group that no fight can pick, and a live zone with nothing to fight, each read like a mistake
/// (T-2).
/// </para>
/// </remarks>
/// <param name="Id">The permanent id of the zone, of the kind `zone` (D-646).</param>
/// <param name="Key">The one character that marks the tiles of the zone on the zone grid (D-1262).</param>
/// <param name="Region">
/// The region of the zone, which names the group file of its groups and the pool of the transition
/// of its common fights (D-1285, D-1289). One overworld holds the land of each region (D-1274).
/// </param>
/// <param name="Rate">The danger that each step adds, in basis points from 0 to 10000 (D-1261).</param>
/// <param name="Groups">The groups of the fights of the zone, in the order of the file (G-4).</param>
/// <param name="Condition">The condition that lets the zone start a fight, which the always leaf writes for a zone that no flag gates (D-1002, D-1269).</param>
public sealed record EncounterZone(ContentId Id, char Key, ContentId Region, int Rate, IReadOnlyList<ZoneGroup> Groups, Condition Condition)
{
    /// <summary>The kind of the id of a zone (D-646).</summary>
    public const string IdKind = "zone";

    /// <summary>The character of the zone grid on a tile that holds no zone: a blocked tile (D-1262).</summary>
    public const char NoZone = '.';

    /// <summary>The largest weight of one group. The sum of the weights of a zone then stays far inside the range of an `int` (T-2).</summary>
    public const int MostWeight = 10000;

    /// <summary>The sum of the weights of the groups of the zone, which the pick of a group draws below (D-1250).</summary>
    public int TotalWeight
    {
        get
        {
            int total = 0;
            foreach (ZoneGroup group in this.Groups)
            {
                total = checked(total + group.Weight);
            }

            return total;
        }
    }

    /// <summary>Tells whether the zone lists one group (D-1250).</summary>
    /// <param name="group">The id of the group.</param>
    /// <returns>True when the groups of the zone hold that id.</returns>
    /// <exception cref="ArgumentNullException">The id is null (T-2).</exception>
    public bool HoldsGroup(ContentId group)
    {
        ArgumentNullException.ThrowIfNull(group);

        foreach (ZoneGroup entry in this.Groups)
        {
            if (string.CompareOrdinal(entry.Group.Value, group.Value) == 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Gives the group that one draw picks (D-1250).</summary>
    /// <param name="draw">The draw, from 0 to one below <see cref="TotalWeight"/>.</param>
    /// <returns>The group whose share of the weights holds the draw.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The draw lies outside the weights (T-2).</exception>
    /// <remarks>The groups hold their shares in the order of the file, so one draw gives one group on each machine (G-4).</remarks>
    public ContentId GroupAt(int draw)
    {
        int below = 0;
        foreach (ZoneGroup group in this.Groups)
        {
            below = checked(below + group.Weight);
            if (draw >= 0 && draw < below)
            {
                return group.Group;
            }
        }

        throw new ArgumentOutOfRangeException(
            nameof(draw),
            draw,
            $"The zone '{this.Id.Value}' holds the weights 0 to {below - 1}, and the draw lies outside them (T-2, D-1250).");
    }

    /// <summary>Reads the `zones` array of a map file (D-1262).</summary>
    /// <param name="reader">The reader of the map file, at the start of the array.</param>
    /// <returns>Each zone, in the order of the file.</returns>
    /// <exception cref="ContentException">
    /// An entry breaks a rule of the reader: a key that is not one character or is the character
    /// of no zone, a rate outside 0 to 10000, a weight below 1, a group two times in one zone, a
    /// group on a zone at rate zero, no group on a zone above it, or a repeated key or id (G-6, T-2).
    /// </exception>
    /// <remarks>
    /// The map checks the zone grid against the terrain and the keys. The content set checks each
    /// group against the group file of the region of its zone, and each flag of each condition (T-2, D-1289).
    /// </remarks>
    public static List<EncounterZone> ReadAll(ref ContentReader reader)
    {
        List<EncounterZone> zones = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, zones.Count))
        {
            EncounterZone zone = Read(ref reader);
            foreach (EncounterZone earlier in zones)
            {
                if (earlier.Key == zone.Key || string.CompareOrdinal(earlier.Id.Value, zone.Id.Value) == 0)
                {
                    throw reader.Refuse(
                        $"the zones '{earlier.Id.Value}' and '{zone.Id.Value}' share an id or the key '{zone.Key}', and each zone takes its own (D-1262)");
                }
            }

            zones.Add(zone);
        }

        return zones;
    }

    private static EncounterZone Read(ref ContentReader reader)
    {
        ContentId? id = null;
        string? key = null;
        ContentId? region = null;
        int? rate = null;
        List<ZoneGroup>? groups = null;
        Condition? condition = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(IdKind);
                    break;
                case "key":
                    key = reader.ReadString();
                    break;
                case "region":
                    region = reader.ReadContentId(Battles.GroupFile.RegionKind);
                    break;
                case "rate":
                    rate = reader.ReadInt();
                    break;
                case "groups":
                    groups = ReadGroups(ref reader);
                    break;
                case "condition":
                    condition = Condition.Read(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        ContentId readId = reader.Require(id, depth, "id");
        string readKey = reader.Require(key, depth, "key");
        if (readKey.Length != 1 || readKey[0] == NoZone)
        {
            throw reader.RefuseField(depth, "key", $"the zone '{readId.Value}' takes the key '{readKey}', and a key is one character other than '{NoZone}', which marks a blocked tile (D-1262)");
        }

        int readRate = reader.RequireInt(rate, depth, "rate");
        if (readRate < 0 || readRate > BasisPoints.One)
        {
            throw reader.RefuseField(depth, "rate", $"the zone '{readId.Value}' takes the rate {readRate}, and a rate is 0 to {BasisPoints.One} basis points (D-1261)");
        }

        List<ZoneGroup> readGroups = reader.Require(groups, depth, "groups");
        if (readRate == 0 && readGroups.Count > 0)
        {
            throw reader.RefuseField(depth, "groups", $"the zone '{readId.Value}' takes the rate 0 and holds {readGroups.Count} groups, and a zone at rate 0 starts no fight, so it holds no group (D-1250, D-1263)");
        }

        if (readRate > 0 && readGroups.Count == 0)
        {
            throw reader.RefuseField(depth, "groups", $"the zone '{readId.Value}' takes the rate {readRate} and holds no group, and a zone above rate 0 holds one group or more (D-1250)");
        }

        return new EncounterZone(readId, readKey[0], reader.Require(region, depth, "region"), readRate, readGroups, reader.Require(condition, depth, "condition"));
    }

    private static List<ZoneGroup> ReadGroups(ref ContentReader reader)
    {
        List<ZoneGroup> groups = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, groups.Count))
        {
            ZoneGroup group = ReadGroup(ref reader);
            foreach (ZoneGroup earlier in groups)
            {
                if (string.CompareOrdinal(earlier.Group.Value, group.Group.Value) == 0)
                {
                    throw reader.Refuse($"the zone names the group '{group.Group.Value}' two times, and one entry takes the whole weight of a group (D-1250)");
                }
            }

            groups.Add(group);
        }

        return groups;
    }

    private static ZoneGroup ReadGroup(ref ContentReader reader)
    {
        ContentId? group = null;
        int? weight = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "group":
                    group = reader.ReadContentId(Patrol.GroupKind);
                    break;
                case "weight":
                    weight = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        ContentId readGroup = reader.Require(group, depth, "group");
        int readWeight = reader.RequireInt(weight, depth, "weight");
        if (readWeight < 1 || readWeight > MostWeight)
        {
            throw reader.RefuseField(depth, "weight", $"the group '{readGroup.Value}' takes the weight {readWeight}, and a weight is 1 to {MostWeight}. A group that no fight picks reads like a mistake, and the limit keeps the sum of a zone inside its range (D-1250, T-2)");
        }

        return new ZoneGroup(readGroup, readWeight);
    }
}
