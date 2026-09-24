using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The pack, the stack limits, the gold, and the six gear slots of the party (D-44, D-385, D-1038, D-1039, D-1043, D-1048).</summary>
public sealed class PackTests
{
    private const ulong Seed = 13;

    private static readonly ContentId Draught = Id("item.fixture_draught");
    private static readonly ContentId Blade = Id("gear.test_blade");
    private static readonly ContentId Shield = Id("gear.test_shield");
    private static readonly ContentId Helm = Id("gear.test_helm");
    private static readonly ContentId Mail = Id("gear.test_mail");
    private static readonly ContentId Resist = Id("gear.test_resist_ring");
    private static readonly ContentId Absorb = Id("gear.test_absorb_ring");

    [Fact]
    public void ACharacterWearsAndRemovesGearInEachOfTheSixSlots()
    {
        // Exit test 1 of PR-13: the six slots of D-44, through the intents of the gear window.
        Simulation run = InMenu();
        ContentId[] pieces = [Blade, Shield, Helm, Mail, Resist, Absorb];
        foreach (ContentId piece in pieces)
        {
            Assert.Equal(0, run.State.Characters.Pick(piece, 1, TestBattles.Content));
        }

        for (int slot = 0; slot < GearRules.SlotCount; slot += 1)
        {
            run.Step([Intent.OfGearWear(0, slot, pieces[slot])]);
            Assert.Equal(pieces[slot].Value, run.State.Characters.Members[0].Gear[slot]?.Value);
            Assert.Equal(0, run.State.Characters.CountOf(pieces[slot]));
        }

        for (int slot = 0; slot < GearRules.SlotCount; slot += 1)
        {
            run.Step([Intent.OfGearWear(0, slot, null)]);
            Assert.Null(run.State.Characters.Members[0].Gear[slot]);
            Assert.Equal(1, run.State.Characters.CountOf(pieces[slot]));
        }
    }

    [Fact]
    public void AChangeOfGearSendsThePieceOfTheSlotBackToThePack()
    {
        Simulation run = InMenu();
        _ = run.State.Characters.Pick(Resist, 2, TestBattles.Content);
        run.Step([Intent.OfGearWear(0, 4, Resist)]);
        _ = run.State.Characters.Pick(Absorb, 1, TestBattles.Content);

        run.Step([Intent.OfGearWear(0, 4, Absorb)]);

        Assert.Equal(Absorb.Value, run.State.Characters.Members[0].Gear[4]?.Value);
        Assert.Equal(2, run.State.Characters.CountOf(Resist));
        Assert.Equal(0, run.State.Characters.CountOf(Absorb));
    }

    [Fact]
    public void TheRulesRefuseAChangeThatNoSlotTakes()
    {
        Simulation run = InMenu();
        PartyState party = run.State.Characters;
        _ = party.Pick(Helm, 1, TestBattles.Content);

        Assert.Contains("of the kind 'head' in the gear slot 0 of the kind 'weapon'", party.RefusalOfWear(0, 0, Helm, TestBattles.Content), StringComparison.Ordinal);
        Assert.Contains("which the pack does not hold", party.RefusalOfWear(0, 0, Blade, TestBattles.Content), StringComparison.Ordinal);
        Assert.Contains("which the gear file does not hold", party.RefusalOfWear(0, 0, Draught, TestBattles.Content), StringComparison.Ordinal);
        Assert.Contains("which holds no piece", party.RefusalOfWear(0, 2, null, TestBattles.Content), StringComparison.Ordinal);
        Assert.Contains("the gear slots 0 to 5", party.RefusalOfWear(0, 6, Helm, TestBattles.Content), StringComparison.Ordinal);
        Assert.Contains("the character slot 1", party.RefusalOfWear(1, 2, Helm, TestBattles.Content), StringComparison.Ordinal);
        Assert.Null(party.RefusalOfWear(0, 2, Helm, TestBattles.Content));

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfGearWear(0, 0, Helm)]));
        Assert.Contains("the rules refuse it", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AChangeOfGearNeedsTheOpenMenu()
    {
        // D-1048: anywhere outside a fight, through the gear window of the menu.
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
        _ = run.State.Characters.Pick(Helm, 1, TestBattles.Content);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfGearWear(0, 2, Helm)]));

        Assert.Contains("gear windows make it", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void APickOverTheStackLimitLeavesARemainder()
    {
        // Exit test 3 of PR-13: the pack holds 3 draughts of a limit of 5, so a find of 4 leaves 2 (D-385).
        PartyState party = InMenu().State.Characters;

        int remainder = party.Pick(Draught, 4, TestBattles.Content);

        Assert.Equal(2, remainder);
        Assert.Equal(5, party.CountOf(Draught));
        Assert.Equal(1, party.Pick(Draught, 1, TestBattles.Content));
    }

    [Fact]
    public void TheStackLimitCountsTheWornCopies()
    {
        // D-1039: the blade has a limit of 2, and a worn blade counts, so a swap opens no room.
        Simulation run = InMenu();
        PartyState party = run.State.Characters;
        Assert.Equal(0, party.Pick(Blade, 1, TestBattles.Content));
        run.Step([Intent.OfGearWear(0, 0, Blade)]);

        int remainder = party.Pick(Blade, 2, TestBattles.Content);

        Assert.Equal(1, remainder);
        Assert.Equal(2, party.OwnedCount(Blade));
        Assert.Equal(1, party.CountOf(Blade));
    }

    [Fact]
    public void GoldAddsAndRefusesANumberBelowOne()
    {
        Simulation run = InMenu();
        PartyState party = run.State.Characters;
        RunContext context = run.State.Context("the test");

        party.AddGold(12, context);
        party.AddGold(5, context);

        Assert.Equal(17, party.Gold);
        Assert.Throws<SimulationException>(() => party.AddGold(0, context));
        Assert.Throws<SimulationException>(() => party.AddGold(int.MaxValue, context));
    }

    [Fact]
    public void TheSnapshotHoldsThePackTheGoldAndTheSlots()
    {
        // Exit test 7 of PR-13: a snapshot read back gives the same party.
        Simulation run = InMenu();
        PartyState party = run.State.Characters;
        _ = party.Pick(Blade, 2, TestBattles.Content);
        _ = party.Pick(Resist, 1, TestBattles.Content);
        run.Step([Intent.OfGearWear(0, 0, Blade)]);
        run.Step([Intent.OfGearWear(0, 5, Resist)]);
        party.AddGold(9, run.State.Context("the test"));

        string text = RunSnapshotText.Write(run.Snapshot());
        var reader = new ContentReader(System.Text.Encoding.UTF8.GetBytes(text), "the test");
        Simulation resumed = Simulation.Resume(Seed, RunSnapshotText.Read(ref reader), TestMaps.Room, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

        Assert.Equal(text, RunSnapshotText.Write(resumed.Snapshot()));
        Assert.Equal(run.StateHash(), resumed.StateHash());
        PartyState back = resumed.State.Characters;
        Assert.Equal(Blade.Value, back.Members[0].Gear[0]?.Value);
        Assert.Equal(Resist.Value, back.Members[0].Gear[5]?.Value);
        Assert.Equal(1, back.CountOf(Blade));
        Assert.Equal(9, back.Gold);
    }

    [Fact]
    public void AResumeRefusesMoreCopiesThanTheStackLimit()
    {
        CharacterValues marrek = Stored([Blade, null, null, null, null, null]);

        ArgumentException error = Assert.Throws<ArgumentException>(() =>
            PartyState.Resume(TestBattles.Content, [marrek], [new PackValues(Blade, 2)], [], false, 0, "the test"));

        Assert.Contains("owns 3 copies of 'gear.test_blade', and the stack limit is 2", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AResumeRefusesAPieceInASlotOfAnotherKind()
    {
        CharacterValues marrek = Stored([null, Blade, null, null, null, null]);

        ArgumentException error = Assert.Throws<ArgumentException>(() =>
            PartyState.Resume(TestBattles.Content, [marrek], [], [], false, 0, "the test"));

        Assert.Contains("of the kind 'weapon' in the gear slot 1", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AResumeRefusesGearWithNoGoldAndGoldBelowZero()
    {
        // D-166: the gear and the gold come with one save format, so a mix is a fault of the file.
        CharacterValues marrek = Stored([null, null, null, null, null, null]);

        ArgumentException mixed = Assert.Throws<ArgumentException>(() =>
            PartyState.Resume(TestBattles.Content, [marrek], [], [], false, null, "the test"));
        ArgumentException below = Assert.Throws<ArgumentException>(() =>
            PartyState.Resume(TestBattles.Content, [marrek], [], [], false, -1, "the test"));

        Assert.Contains("differ on the save format", mixed.Message, StringComparison.Ordinal);
        Assert.Contains("below zero", below.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheWornGearGivesTheStatsAndTheElementsOfTheFight()
    {
        // D-1036, D-1037: the fight reads the gear of each character.
        Simulation run = InMenu();
        _ = run.State.Characters.Pick(Blade, 1, TestBattles.Content);
        _ = run.State.Characters.Pick(Absorb, 1, TestBattles.Content);
        run.Step([Intent.OfGearWear(0, 0, Blade)]);
        run.Step([Intent.OfGearWear(0, 4, Absorb)]);
        run.Step([Intent.OfPlayer(IntentIds.CloseMenu)]);
        PartyMember marrek = run.State.Characters.Members[0];

        Battle battle = Battle.Start(TestBattles.Content, new MapEncounter(Id("patrol.test_guard"), Id("group.test_pair"), EncounterSide.None), run.State.Characters, run.State.Context("the test"));

        Combatant fighter = battle.Party[0];
        Assert.Equal(marrek.Stats.Attack + 5, fighter.Attack);
        Assert.Equal(marrek.Stats.Speed - 3, fighter.Speed);
        Assert.Equal(marrek.Stats.Health, fighter.FullHealth);
        Assert.Equal(Affinity.Absorb, fighter.Elements.Of(Element.Fire));
    }

    [Fact]
    public void ARecordReplaysAChangeOfGearAndAnItemUse()
    {
        // G-5: the item field of an intent carries a piece of gear through the record text.
        Simulation run = TestParty.Start(Seed, marrek => marrek with { Health = 10 });
        _ = run.State.Characters.Pick(Helm, 1, TestBattles.Content);
        RunRecorder recorder = new(RunHeader.ForThisBuild("a-content-hash", Seed), run.Snapshot());
        List<Intent[]> ticks = [[Intent.OfPlayer(IntentIds.OpenMenu)], [Intent.OfGearWear(0, 2, Helm)], [Intent.OfMenuItem(Draught, 0)], [Intent.OfPlayer(IntentIds.CloseMenu)]];
        foreach (Intent[] intents in ticks)
        {
            run.Step(intents);
            recorder.Step(run.Tick, intents);
        }

        RunRecord read = RunRecordText.Read(RunRecordText.Write(recorder.Build()));
        RunState replayed = RunReplay.Play(read, "a-content-hash", TestMaps.Room, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

        Assert.Equal(run.StateHash(), replayed.StateHash());
        Assert.Equal(Helm.Value, replayed.Characters.Members[0].Gear[2]?.Value);
        Assert.Equal(40, replayed.Characters.Members[0].Health);
    }

    [Theory]
    [InlineData("\"pack\": [{ \"item\": \"item.fixture_draught\", \"count\": 3 }]", "\"pack\": [{ \"gear\": \"gear.absent\", \"count\": 1 }]", "the gear file holds no piece")]
    [InlineData("\"pack\": [{ \"item\": \"item.fixture_draught\", \"count\": 3 }]", "\"pack\": [{ \"item\": \"item.fixture_draught\", \"count\": 6 }]", "the stack limit is 5")]
    [InlineData("\"start_gear\": []", "\"start_gear\": [{ \"character\": \"character.marrek\", \"gear\": [\"gear.test_blade\", \"gear.test_blade\"] }]", "no empty slot of the kind 'weapon'")]
    [InlineData("\"pack\": [{ \"item\": \"item.fixture_draught\", \"count\": 3 }],\n \"start_gear\": []", "\"pack\": [{ \"gear\": \"gear.test_mail\", \"count\": 1 }],\n \"start_gear\": [{ \"character\": \"character.marrek\", \"gear\": [\"gear.test_mail\"] }]", "the stack limit is 1")]
    public void AStartThatBreaksAStackLimitOrASlotFailsTheBattleContent(string from, string to, string reason)
    {
        // D-1038, D-1039: the pack and the start gear of the fixture count together.
        string fixture = TestBattles.FixtureFile.Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(TestBattles.FixtureFile, fixture);

        ContentException error = Assert.Throws<ContentException>(() => TestBattles.Of(fixture));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ADropListThatNamesAnAbsentItemFailsTheBattleContent()
    {
        ContentException error = Assert.Throws<ContentException>(() => TestBattles.WithDrops("[{ \"item\": \"item.absent\", \"chance\": 100 }]"));

        Assert.Equal(TestBattles.AttackerProfilePath, error.File);
        Assert.Contains("the drop list of 'profile.test_attacker'", error.Message, StringComparison.Ordinal);
    }

    private static Simulation InMenu()
    {
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        return run;
    }

    private static CharacterValues Stored(ContentId?[] gear) =>
        new(Id("character.marrek"), 60, BattleRow.Front, [], new GrowthValues(1, 0, 8), new LessonValues([null, null], []), gear);

    private static ContentId Id(string value) => ContentId.Parse(value, "test", value[..value.IndexOf('.', StringComparison.Ordinal)]);
}
