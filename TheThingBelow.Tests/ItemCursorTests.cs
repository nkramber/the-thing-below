using System;
using System.Collections;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The cursor of the item window: the items of the pack, the target of a use, and the refusal of
/// a use that changes nothing (D-1046, D-1049). The tests read the built Game assembly (D-614).
/// </summary>
public sealed class ItemCursorTests
{
    private const ulong Seed = 9;

    private static readonly ContentId Draught = ContentId.Parse("item.fixture_draught", "test", "item");
    private static readonly ContentId Blade = ContentId.Parse("gear.test_blade", "test", "gear");
    private static readonly ContentId Torch = ContentId.Parse("item.torch", "test", "item");
    private static readonly ContentId Key = ContentId.Parse("item.test_key", "test", "item");

    [Fact]
    public void AnItemThatChangesNothingShowsDimAndMakesNoIntent()
    {
        // D-1049: Marrek stands at full health, so the draught changes nothing.
        Simulation run = InMenu(marrek => marrek);
        GameValue cursor = GameValue.New("ItemCursor", run.State);

        Assert.False((bool)cursor.Call("AllowsItem", 0)!);
        Assert.Null(cursor.Call("Confirm"));
        Assert.Equal("Item", cursor.Name("Stage"));
    }

    [Fact]
    public void AnItemTakesATargetThenMakesTheIntent()
    {
        Simulation run = InMenu(marrek => marrek with { Health = 10 });
        GameValue cursor = GameValue.New("ItemCursor", run.State);

        Assert.Null(cursor.Call("Confirm"));
        Assert.Equal("Target", cursor.Name("Stage"));
        Intent made = (Intent?)cursor.Call("Confirm") ?? throw new InvalidOperationException("The window sent no intent.");

        Assert.Equal(IntentIds.MenuItem.Value, made.Action.Value);
        Assert.Equal((Draught.Value, 0), (made.Item?.Value, made.Target?.Slot));
        Assert.Equal("Item", cursor.Name("Stage"));
        run.Step([made]);
        Assert.Equal(40, run.State.Characters.Members[0].Health);
    }

    [Fact]
    public void TheListHoldsTheItemsAloneAndSpareGearStaysInTheGearWindow()
    {
        Simulation run = InMenu(marrek => marrek);
        Assert.Equal(0, run.State.Characters.Pick(Blade, 1, run.State.BattleContent));
        GameValue cursor = GameValue.New("ItemCursor", run.State);

        IList items = (IList)cursor.Read<object>("Items");

        PackValues only = (PackValues)Assert.Single(items)!;
        Assert.Equal(Draught.Value, only.Id.Value);
    }

    [Fact]
    public void AUseOfTheLastCopyKeepsTheCursorInsideTheList()
    {
        // The pack holds 3 draughts, and two hurt characters take them. After the third use, the
        // list is empty, and a move keeps the cursor.
        Simulation run = TestParty.StartEach(Seed, (slot, stored) => stored with { Health = 1 }, TestBattles.WithParty(2));
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        GameValue cursor = GameValue.New("ItemCursor", run.State);
        for (int use = 0; use < 3; use += 1)
        {
            _ = cursor.Call("Confirm");
            cursor.Call("Point", (bool)cursor.Call("AllowsTarget", 0)! ? 0 : 1);
            Intent made = (Intent?)cursor.Call("Confirm") ?? throw new InvalidOperationException($"Use {use}: the window sent no intent.");
            run.Step([made]);
            cursor.Call("Settle");
        }

        Assert.Equal(0, cursor.Read<int>("Count"));
        cursor.Call("Move", 1);
        Assert.Equal(0, cursor.Read<int>("Cursor"));
        Assert.Null(cursor.Call("Confirm"));
    }

    [Fact]
    public void TheKeyItemsSitInTheirOwnListAndEachKeyOnTheKeyring()
    {
        // D-1219: the pack list ends with the entry of the key items, which holds the torch and the
        // Keyring, and the Keyring holds the key. A key item takes no use.
        Simulation run = InMenu(marrek => marrek);
        BattleContent content = run.State.BattleContent;
        Assert.Equal(0, run.State.Characters.Pick(Torch, 1, content));
        Assert.Equal(0, run.State.Characters.Pick(Key, 1, content));
        GameValue cursor = GameValue.New("ItemCursor", run.State);

        Assert.Equal([Draught.Value, "KeyItems"], LineNames(cursor));
        cursor.Call("Point", 1);
        Assert.True((bool)cursor.Call("AllowsItem", 1)!);
        Assert.Null(cursor.Call("Confirm"));
        Assert.Equal("KeyItems", cursor.Name("Stage"));
        Assert.Equal([Torch.Value, "Keyring"], LineNames(cursor));
        Assert.False((bool)cursor.Call("AllowsItem", 0)!);

        cursor.Call("Point", 1);
        Assert.Null(cursor.Call("Confirm"));
        Assert.Equal("Keyring", cursor.Name("Stage"));
        Assert.Equal([Key.Value], LineNames(cursor));
        Assert.Null(cursor.Call("Confirm"));
        Assert.Equal("Keyring", cursor.Name("Stage"));

        Assert.False((bool)cursor.Call("Cancel")!);
        Assert.Equal(("KeyItems", 1), (cursor.Name("Stage"), cursor.Read<int>("Cursor")));
        Assert.False((bool)cursor.Call("Cancel")!);
        Assert.Equal(("Item", 1), (cursor.Name("Stage"), cursor.Read<int>("Cursor")));
        Assert.True((bool)cursor.Call("Cancel")!);
    }

    [Fact]
    public void APackWithNoKeyItemShowsNoEntryOfTheKeyItems()
    {
        // D-1219: the entry shows once the pack holds a key item, and a key alone shows the Keyring alone.
        Simulation run = InMenu(marrek => marrek);
        GameValue cursor = GameValue.New("ItemCursor", run.State);
        Assert.Equal([Draught.Value], LineNames(cursor));

        Assert.Equal(0, run.State.Characters.Pick(Key, 1, run.State.BattleContent));
        cursor.Call("Point", 1);
        Assert.Null(cursor.Call("Confirm"));

        Assert.Equal(["Keyring"], LineNames(cursor));
    }

    private static List<string> LineNames(GameValue cursor)
    {
        List<string> names = [];
        foreach (object? line in (IEnumerable)cursor.Read<object>("Lines"))
        {
            GameValue value = GameValue.Of(line!);
            names.Add(value.Read<object?>("Entry") is PackValues entry ? entry.Id.Value : value.Name("Kind"));
        }

        return names;
    }

    private static Simulation InMenu(Func<CharacterValues, CharacterValues> change)
    {
        Simulation run = TestParty.Start(Seed, change);
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        return run;
    }
}
