using System;
using System.Collections;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The cursor of the gear window: the six slots, the pieces of the pack that fit a slot, and the
/// intent of a change (D-44, D-1048). The tests read the built Game assembly, because Tests takes
/// no reference to Game (D-614).
/// </summary>
public sealed class GearCursorTests
{
    private const ulong Seed = 7;

    private static readonly ContentId Blade = ContentId.Parse("gear.test_blade", "test", "gear");
    private static readonly ContentId Resist = ContentId.Parse("gear.test_resist_ring", "test", "gear");
    private static readonly ContentId Charm = ContentId.Parse("gear.test_weak_charm", "test", "gear");

    [Fact]
    public void ASlotWithNoPieceOfItsKindOffersNothing()
    {
        // D-44: Marrek of the tests wears nothing, and the pack holds no piece.
        Simulation run = InMenu();
        GameValue cursor = GameValue.New("GearCursor", run.State);

        Assert.Equal(GearRules.SlotCount, cursor.Read<int>("Count"));
        Assert.False((bool)cursor.Call("AllowsSlot", 0)!);
        Assert.Null(cursor.Call("Confirm"));
        Assert.Equal("Slot", cursor.Name("Stage"));
    }

    [Fact]
    public void AnAccessorySlotListsThePiecesOfItsKindAndPutsOneOn()
    {
        Simulation run = InMenu();
        Stock(run, Blade, Resist, Charm);
        GameValue cursor = GameValue.New("GearCursor", run.State);
        cursor.Call("Point", 4);

        Assert.Null(cursor.Call("Confirm"));

        Assert.Equal("Pack", cursor.Name("Stage"));
        IList entries = (IList)cursor.Read<object>("PackEntries");
        Assert.Equal(3, entries.Count);
        Assert.Null(entries[0]);
        Assert.Equal([Resist.Value, Charm.Value], [((ContentId)entries[1]!).Value, ((ContentId)entries[2]!).Value]);
        Assert.False((bool)cursor.Call("AllowsPackEntry", 0)!);
        Assert.Equal(1, cursor.Read<int>("Cursor"));

        Intent made = (Intent?)cursor.Call("Confirm") ?? throw new InvalidOperationException("The window sent no intent.");

        Assert.Equal(IntentIds.GearWear.Value, made.Action.Value);
        Assert.Equal((0, 4, Resist.Value), (made.Actor, made.Option, made.Item?.Value));
        Assert.Equal("Slot", cursor.Name("Stage"));
        run.Step([made]);
        Assert.Equal(Resist.Value, run.State.Characters.Members[0].Gear[4]?.Value);
    }

    [Fact]
    public void AFilledSlotTakesThePieceOff()
    {
        Simulation run = InMenu();
        Stock(run, Blade);
        run.Step([Intent.OfGearWear(0, 0, Blade)]);
        GameValue cursor = GameValue.New("GearCursor", run.State);

        Assert.Null(cursor.Call("Confirm"));
        Assert.Equal(0, cursor.Read<int>("Cursor"));
        Intent made = (Intent?)cursor.Call("Confirm") ?? throw new InvalidOperationException("The window sent no intent.");

        Assert.Equal((0, 0), (made.Actor, made.Option));
        Assert.Null(made.Item);
        run.Step([made]);
        Assert.Null(run.State.Characters.Members[0].Gear[0]);
        Assert.Equal(1, run.State.Characters.CountOf(Blade));
    }

    [Fact]
    public void CancelGoesBackToTheSlotsThenClosesTheWindow()
    {
        Simulation run = InMenu();
        Stock(run, Blade);
        GameValue cursor = GameValue.New("GearCursor", run.State);
        Assert.Null(cursor.Call("Confirm"));

        Assert.False((bool)cursor.Call("Cancel")!);
        Assert.Equal("Slot", cursor.Name("Stage"));
        Assert.True((bool)cursor.Call("Cancel")!);
    }

    [Fact]
    public void EveryIntentOfTheWindowPassesTheRules()
    {
        // D-100: the window reads the refusal of the party state, so a confirm never makes an intent that the rules refuse.
        Simulation run = InMenu();
        Stock(run, Blade, Resist, Charm);
        for (int slot = 0; slot < GearRules.SlotCount; slot += 1)
        {
            GameValue cursor = GameValue.New("GearCursor", run.State);
            cursor.Call("Point", slot);
            _ = cursor.Call("Confirm");
            if (cursor.Name("Stage") != "Pack")
            {
                continue;
            }

            for (int entry = 0; entry < cursor.Read<int>("Count"); entry += 1)
            {
                cursor.Call("Point", entry);
                if (cursor.Call("Confirm") is Intent made)
                {
                    Assert.Null(run.State.Characters.RefusalOfWear(made.Actor!.Value, made.Option!.Value, made.Item, run.State.BattleContent));
                    _ = cursor.Call("Confirm");
                }
            }
        }
    }

    [Fact]
    public void TheTrialStatsHoldTheWornStatsInTheSlotsAndThePieceUnderTheCursorInThePack()
    {
        // D-1060: the line under the worn stats shows each stat with the piece under the cursor.
        // The charm adds 1 attack, 2 magic, and 2 speed, and it costs 1 resistance.
        Simulation run = InMenu();
        Stock(run, Resist, Charm);
        GameValue cursor = GameValue.New("GearCursor", run.State);
        StatRow worn = run.State.Characters.Members[0].StatsWith(run.State.BattleContent.Gear);
        cursor.Call("Point", 4);

        Assert.Equal(worn, (StatRow)cursor.Call("TrialStats")!);

        cursor.Call("Confirm");
        cursor.Call("Point", 2);
        StatRow trial = (StatRow)cursor.Call("TrialStats")!;

        Assert.Equal(worn with { Attack = worn.Attack + 1, Magic = worn.Magic + 2, Resistance = worn.Resistance - 1, Speed = worn.Speed + 2 }, trial);
        Assert.Equal("menu.gear_gain", ((ContentId)GameValue.Static("GearView", "TrialIdOf", worn.Magic, trial.Magic)!).Value);
        Assert.Equal("menu.gear_loss", ((ContentId)GameValue.Static("GearView", "TrialIdOf", worn.Resistance, trial.Resistance)!).Value);
        Assert.Equal("menu.gear_same", ((ContentId)GameValue.Static("GearView", "TrialIdOf", worn.Defense, trial.Defense)!).Value);
    }

    private static Simulation InMenu()
    {
        Simulation run = TestParty.Start(Seed, marrek => marrek);
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        return run;
    }

    private static void Stock(Simulation run, params ContentId[] pieces)
    {
        foreach (ContentId piece in pieces)
        {
            Assert.Equal(0, run.State.Characters.Pick(piece, 1, run.State.BattleContent));
        }
    }
}
