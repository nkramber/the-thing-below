using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Battles;

/// <summary>
/// The rules of the gear that one character wears: the six gear slots, the sum of the stat
/// amounts, and the element table (D-44, D-1036, D-1037, D-1047).
/// </summary>
public static class GearRules
{
    /// <summary>The number of gear slots of each character (D-44).</summary>
    public const int SlotCount = 6;

    /// <summary>The lowest value of a stat after the gear adds to it (D-1047).</summary>
    public const int LowestStat = 1;

    /// <summary>The levels of an element from the worst to the best, which a `weak` piece steps down (D-1037).</summary>
    private static readonly Affinity[] Ladder = [Affinity.Weak, Affinity.Normal, Affinity.Resist, Affinity.Absorb];

    /// <summary>Gives the kind of piece that a gear slot holds: the weapon, the off-hand, the head, the body, then two accessories (D-44).</summary>
    /// <param name="slot">The gear slot, from 0 to 5.</param>
    /// <returns>The slot kind.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The slot is outside 0 to 5 (T-2).</exception>
    public static GearSlotKind KindOf(int slot) => slot switch
    {
        0 => GearSlotKind.Weapon,
        1 => GearSlotKind.OffHand,
        2 => GearSlotKind.Head,
        3 => GearSlotKind.Body,
        4 or 5 => GearSlotKind.Accessory,
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, $"A character has the gear slots 0 to {SlotCount - 1} (D-44)."),
    };

    /// <summary>
    /// Puts a list of pieces in the six gear slots. Each piece goes to the first empty slot of
    /// its kind, so a second accessory goes to the second accessory slot (D-44).
    /// </summary>
    /// <param name="pieces">The ids of the pieces.</param>
    /// <param name="gear">The gear file.</param>
    /// <param name="file">The file that names the pieces, for an error.</param>
    /// <param name="who">The id of the character, for an error.</param>
    /// <returns>The six slots, each with a gear id or no value.</returns>
    /// <exception cref="ContentException">A piece is absent from the gear file, or no empty slot of its kind is left (T-2).</exception>
    public static ContentId?[] SlotsOf(IReadOnlyList<ContentId> pieces, GearList gear, string file, string who)
    {
        ArgumentNullException.ThrowIfNull(pieces);
        ArgumentNullException.ThrowIfNull(gear);

        var slots = new ContentId?[SlotCount];
        foreach (ContentId id in pieces)
        {
            GearSlotKind kind = gear.Piece(id).Slot;
            int? free = null;
            for (int slot = SlotCount - 1; slot >= 0; slot -= 1)
            {
                if (KindOf(slot) == kind && slots[slot] is null)
                {
                    free = slot;
                }
            }

            if (free is not int open)
            {
                throw ContentException.ForField(file, id.Value, $"the character '{who}' wears this piece, and no empty slot of the kind '{GearList.NameOf(kind)}' is left (D-44)");
            }

            slots[open] = id;
        }

        return slots;
    }

    /// <summary>
    /// Gives the stats of a character with its gear. The attack, magic, defense, resistance, and
    /// speed amounts of each worn piece add, and each of the five keeps a floor of 1. Health and
    /// MP never change (D-1036, D-1037, D-1047, D-1052).
    /// </summary>
    /// <param name="curve">The stats of the character at its level, from its stat curve.</param>
    /// <param name="worn">The six gear slots, each with a gear id or no value for an empty slot.</param>
    /// <param name="gear">The gear file.</param>
    /// <returns>The stats with the gear.</returns>
    /// <exception cref="ContentException">A slot names a piece that the gear file does not hold (T-2).</exception>
    public static StatRow StatsOf(StatRow curve, IReadOnlyList<ContentId?> worn, GearList gear)
    {
        ArgumentNullException.ThrowIfNull(curve);
        ArgumentNullException.ThrowIfNull(worn);
        ArgumentNullException.ThrowIfNull(gear);

        int attack = curve.Attack;
        int magic = curve.Magic;
        int defense = curve.Defense;
        int resistance = curve.Resistance;
        int speed = curve.Speed;
        foreach (ContentId? id in worn)
        {
            if (id is null)
            {
                continue;
            }

            GearRecord piece = gear.Piece(id);
            attack = checked(attack + piece.Attack);
            magic = checked(magic + piece.Magic);
            defense = checked(defense + piece.Defense);
            resistance = checked(resistance + piece.Resistance);
            speed = checked(speed + piece.Speed);
        }

        return curve with
        {
            Attack = Math.Max(LowestStat, attack),
            Magic = Math.Max(LowestStat, magic),
            Defense = Math.Max(LowestStat, defense),
            Resistance = Math.Max(LowestStat, resistance),
            Speed = Math.Max(LowestStat, speed),
        };
    }

    /// <summary>
    /// Gives the element table of a character with its gear. For each element, the best
    /// protection of any worn piece applies first: absorb over resist over normal. Then each
    /// piece with `weak` for that element steps the level down one step, and the floor is
    /// weak (D-1037).
    /// </summary>
    /// <param name="worn">The six gear slots, each with a gear id or no value for an empty slot.</param>
    /// <param name="gear">The gear file.</param>
    /// <returns>The table. A character with no gear takes every element at the normal rate.</returns>
    /// <exception cref="ContentException">A slot names a piece that the gear file does not hold (T-2).</exception>
    public static ElementTable ElementsOf(IReadOnlyList<ContentId?> worn, GearList gear)
    {
        ArgumentNullException.ThrowIfNull(worn);
        ArgumentNullException.ThrowIfNull(gear);

        var table = new Affinity[Elements.All.Count];
        foreach (Element element in Elements.All)
        {
            table[(int)element] = LevelOf(element, worn, gear);
        }

        return ElementTable.Of(table);
    }

    private static Affinity LevelOf(Element element, IReadOnlyList<ContentId?> worn, GearList gear)
    {
        int best = Array.IndexOf(Ladder, Affinity.Normal);
        int weakPieces = 0;
        foreach (ContentId? id in worn)
        {
            if (id is null)
            {
                continue;
            }

            Affinity level = gear.Piece(id).Elements.Of(element);
            if (level == Affinity.Weak)
            {
                weakPieces += 1;
                continue;
            }

            best = Math.Max(best, Array.IndexOf(Ladder, level));
        }

        return Ladder[Math.Max(0, best - weakPieces)];
    }
}
