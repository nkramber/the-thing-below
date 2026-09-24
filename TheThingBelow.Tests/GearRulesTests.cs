using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The rules of the worn gear: the slots, the sum of the amounts, the floor, and the element levels (D-44, D-1036, D-1037, D-1047).</summary>
public sealed class GearRulesTests
{
    private static readonly GearList Gear = GearList.Read(Encoding.UTF8.GetBytes(TestBattles.GearFile), GearList.Path);

    [Theory]
    [InlineData(0, GearSlotKind.Weapon)]
    [InlineData(1, GearSlotKind.OffHand)]
    [InlineData(2, GearSlotKind.Head)]
    [InlineData(3, GearSlotKind.Body)]
    [InlineData(4, GearSlotKind.Accessory)]
    [InlineData(5, GearSlotKind.Accessory)]
    public void EachSlotHoldsItsKind(int slot, GearSlotKind kind)
    {
        Assert.Equal(kind, GearRules.KindOf(slot));
    }

    [Fact]
    public void ASlotOutsideTheSixIsAnError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GearRules.KindOf(6));
    }

    [Fact]
    public void TheAmountsOfEachPieceAdd()
    {
        // D-1037: the amounts of the six slots add. D-1036: health and MP never change.
        var curve = new StatRow(60, 8, 12, 4, 100);

        StatRow stats = GearRules.StatsOf(curve, Worn("gear.test_blade", "gear.test_shield", "gear.test_weak_charm", "gear.test_weak_charm"), Gear);

        Assert.Equal(new StatRow(60, 8, 12 + 5 + 1 + 1, 4 + 3, 100 - 3 + 2 + 2), stats);
    }

    [Fact]
    public void EachStatKeepsAFloorOfOne()
    {
        // D-1047: the mail costs 99 speed, and a speed of 1 keeps the timeline alive.
        StatRow stats = GearRules.StatsOf(new StatRow(60, 8, 12, 4, 20), Worn("gear.test_mail"), Gear);

        Assert.Equal(GearRules.LowestStat, stats.Speed);
        Assert.Equal(8, stats.Defense);
    }

    [Theory]
    [InlineData(new string[0], Affinity.Normal)]
    [InlineData(new[] { "resist" }, Affinity.Resist)]
    [InlineData(new[] { "resist", "resist" }, Affinity.Resist)]
    [InlineData(new[] { "resist", "weak" }, Affinity.Normal)]
    [InlineData(new[] { "absorb", "absorb", "weak" }, Affinity.Resist)]
    [InlineData(new[] { "resist", "resist", "resist", "resist", "resist", "weak" }, Affinity.Normal)]
    [InlineData(new[] { "absorb", "weak", "weak" }, Affinity.Normal)]
    [InlineData(new[] { "resist", "weak", "weak" }, Affinity.Weak)]
    [InlineData(new[] { "weak" }, Affinity.Weak)]
    [InlineData(new[] { "weak", "weak" }, Affinity.Weak)]
    [InlineData(new[] { "absorb", "resist" }, Affinity.Absorb)]
    public void TheBestProtectionAppliesAndEachWeakPieceStepsItDown(string[] levels, Affinity expected)
    {
        // Exit test 5 of PR-13: each example of D-1037, with two or more pieces of one element.
        var worn = new List<ContentId?>();
        foreach (string level in levels)
        {
            worn.Add(Id(level switch
            {
                "resist" => "gear.test_resist_ring",
                "absorb" => "gear.test_absorb_ring",
                _ => "gear.test_weak_charm",
            }));
        }

        ElementTable table = GearRules.ElementsOf(worn, Gear);

        Assert.Equal(expected, table.Of(Element.Fire));
        Assert.Equal(Affinity.Normal, table.Of(Element.Ice));
    }

    [Fact]
    public void TwoAccessoriesFillBothAccessorySlots()
    {
        ContentId?[] slots = GearRules.SlotsOf([Id("gear.test_resist_ring"), Id("gear.test_blade"), Id("gear.test_weak_charm")], Gear, "test.json", "character.marrek");

        Assert.Equal("gear.test_blade", slots[0]?.Value);
        Assert.Equal("gear.test_resist_ring", slots[4]?.Value);
        Assert.Equal("gear.test_weak_charm", slots[5]?.Value);
        Assert.Null(slots[1]);
    }

    [Fact]
    public void AThirdAccessoryFindsNoSlot()
    {
        ContentException error = Assert.Throws<ContentException>(() =>
            GearRules.SlotsOf([Id("gear.test_resist_ring"), Id("gear.test_absorb_ring"), Id("gear.test_weak_charm")], Gear, "test.json", "character.marrek"));

        Assert.Contains("no empty slot of the kind 'accessory'", error.Message, StringComparison.Ordinal);
        Assert.Contains("gear.test_weak_charm", error.Message, StringComparison.Ordinal);
    }

    private static List<ContentId?> Worn(params string[] ids)
    {
        var worn = new List<ContentId?>();
        foreach (string id in ids)
        {
            worn.Add(Id(id));
        }

        return worn;
    }

    private static ContentId Id(string value) => ContentId.Parse(value, "test", "gear");
}
