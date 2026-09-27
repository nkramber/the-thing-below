using System;
using System.Collections.Generic;
using System.Linq;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Notices;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The dungeon parts of PR-16 on the vault of <see cref="PartsMaps"/>: the doors, the locks, the
/// keys, the Theft drill, the chest, the exit, and the memory of the place (D-41, D-385, D-386,
/// D-555, D-1216, D-1219, D-1220).
/// </summary>
public sealed class DungeonPartsTests
{
    private const ulong Seed = 20260927;

    private static readonly ContentId Pilfer = ContentId.Parse("lesson.test_pilfer", "test", "lesson");

    [Fact]
    public void AShutDoorBlocksTheStepAndAConfirmOpensItForGood()
    {
        // D-41, D-1142: a door is solid until a confirm opens it, and the memory keeps it open.
        Simulation run = PartsMaps.Start(Seed);
        ToPlainDoor(run);
        HubWalks.Face(run, StepDirection.South);

        IReadOnlyList<LogEntry> log = HubWalks.Confirm(run);

        Assert.Contains("the lead opened a door", HubWalks.Messages(log));
        Assert.Empty(run.TakeNotices());
        Assert.True(run.State.Party.Place.IsOpen(DoorOf(PartsMaps.PlainDoor)));
        HubWalks.Walk(run, StepDirection.South, 2);
        Assert.Equal(new TilePoint(5, 4), run.State.Party.LeadAt);
    }

    [Fact]
    public void AStoryLockStaysShutWithNoKeyAndWithATheftDrill()
    {
        // Exit test 5 of PR-16 (D-386): a Theft drill never opens a story lock.
        Simulation run = PartsMaps.Start(Seed);
        CarryPilfer(run);
        IntoHall(run);
        HubWalks.Walk(run, StepDirection.West, 3);
        HubWalks.Face(run, StepDirection.South);

        HubWalks.Confirm(run);

        Assert.False(run.State.Party.Place.IsOpen(DoorOf(PartsMaps.StoryDoor)));
        Assert.Equal(DoorRules.LockedNotice.Value, Assert.Single(run.TakeNotices()).Id.Value);
    }

    [Fact]
    public void AKeyOpensTheLockThatNamesItAndStaysOnTheKeyring()
    {
        // Exit test 8 of PR-16 (D-1219): the key stays in the pack after it opens its lock.
        Simulation run = PartsMaps.Start(Seed);
        Assert.Equal(0, run.State.Characters.Pick(PartsMaps.Key, 1, TestBattles.Content));
        IntoHall(run);
        HubWalks.Walk(run, StepDirection.West, 3);
        HubWalks.Face(run, StepDirection.South);

        HubWalks.Confirm(run);

        Assert.True(run.State.Party.Place.IsOpen(DoorOf(PartsMaps.StoryDoor)));
        Assert.Equal(DoorRules.KeyNotice.Value, Assert.Single(run.TakeNotices()).Id.Value);
        Assert.Equal(1, run.State.Characters.CountOf(PartsMaps.Key));
        Assert.True(Assert.IsType<KeyItem>(TestBattles.Content.Items.Item(PartsMaps.Key)).OnRing);
    }

    [Fact]
    public void ATheftDrillOpensAPickableLockAndNothingElseDoes()
    {
        // Exit test 5 of PR-16 (D-386): a Theft drill that a standing character carries opens a pickable lock.
        Simulation shut = PartsMaps.Start(Seed);
        IntoHall(shut);
        HubWalks.Walk(shut, StepDirection.East, 3);
        HubWalks.Face(shut, StepDirection.South);
        HubWalks.Confirm(shut);
        Assert.False(shut.State.Party.Place.IsOpen(DoorOf(PartsMaps.PickedDoor)));
        Assert.Equal(DoorRules.LockedNotice.Value, Assert.Single(shut.TakeNotices()).Id.Value);

        Simulation run = PartsMaps.Start(Seed);
        CarryPilfer(run);
        IntoHall(run);
        HubWalks.Walk(run, StepDirection.East, 3);
        HubWalks.Face(run, StepDirection.South);
        HubWalks.Confirm(run);

        Assert.True(run.State.Party.Place.IsOpen(DoorOf(PartsMaps.PickedDoor)));
        Assert.Equal(DoorRules.PickedNotice.Value, Assert.Single(run.TakeNotices()).Id.Value);
        HubWalks.Walk(run, StepDirection.South, 2);
        Assert.Equal(new TilePoint(8, 6), run.State.Party.LeadAt);
    }

    [Fact]
    public void ADownedThiefPicksNoLock()
    {
        // D-386: the drill of a character who fights opens the lock, and a downed character does not fight.
        Simulation run = PartsMaps.Start(Seed);
        CarryPilfer(run);

        Simulation downed = Resume(run, snapshot => snapshot with
        {
            Characters = snapshot.Characters! with { Characters = [snapshot.Characters.Characters[0] with { Health = 0 }] },
        });

        Assert.False(DoorRules.CarriesTheft(downed.State));
    }

    [Fact]
    public void TheFirstOpenOfAChestGivesItsGoldItsEntriesAndTheFallbackOfAnOwnedLesson()
    {
        // Exit tests 9, 13, and 14 of PR-16 (D-385, D-1024, D-1161, D-1224): the party holds 3 of 5
        // draughts, so 2 fit and 2 stay, the new drill joins the lesson pack, and the owned rite
        // gives its fallback. Each copy posts one notice.
        Simulation run = PartsMaps.Start(Seed);
        int gold = run.State.Characters.Gold;
        FaceChest(run);

        HubWalks.Confirm(run);

        Assert.Equal(gold + 25, run.State.Characters.Gold);
        Assert.Equal(5, run.State.Characters.CountOf(Item("fixture_draught")));
        Assert.Contains(Pilfer, run.State.Characters.LessonPack, IdComparer.Instance);
        Assert.Equal(1, run.State.Characters.CountOf(Item("test_tonic")));
        Assert.Equal(
            [
                "notice.chest_gold 25", "notice.chest_found item.fixture_draught", "notice.chest_found item.fixture_draught",
                "notice.chest_left item.fixture_draught", "notice.chest_left item.fixture_draught",
                "notice.chest_found lesson.test_pilfer", "notice.chest_found item.test_tonic",
            ],
            Posted(run));
        ChestLeft left = Assert.Single(run.State.Party.Place.LeftIn(ChestId())!);
        Assert.Equal(("item.fixture_draught", 2), (left.Thing.Value, left.Count));
    }

    [Fact]
    public void AChestKeepsWhatStaysForALaterConfirmAndThenIsEmpty()
    {
        // Exit test 14 of PR-16 (D-385): the pack takes the rest once it has room.
        Simulation run = PartsMaps.Start(Seed);
        FaceChest(run);
        HubWalks.Confirm(run);
        _ = run.TakeNotices();

        HubWalks.Confirm(run);
        Assert.Equal(["notice.chest_left item.fixture_draught", "notice.chest_left item.fixture_draught"], Posted(run));

        run = Resume(run, snapshot => snapshot with { Characters = snapshot.Characters! with { Pack = [] } });
        HubWalks.Confirm(run);
        Assert.Equal(["notice.chest_found item.fixture_draught", "notice.chest_found item.fixture_draught"], Posted(run));
        Assert.Empty(run.State.Party.Place.LeftIn(ChestId())!);

        HubWalks.Confirm(run);
        Assert.Equal([ChestRules.EmptyNotice.Value], Posted(run));
    }

    [Fact]
    public void TheSaveHoldsTheOpenChestTheOpenDoorAndTheDeadEnemy()
    {
        // Exit test 13 of PR-16 (D-555, D-1161): the snapshot carries the memory of the vault.
        Simulation run = PartsMaps.Start(Seed);
        KillGuard(run);
        HubWalks.Walk(run, StepDirection.North, 2);
        HubWalks.Walk(run, StepDirection.East, 4);
        HubWalks.Face(run, StepDirection.East);
        HubWalks.Confirm(run);

        string line = RunSnapshotText.Write(run.Snapshot());
        var reader = new ContentReader(System.Text.Encoding.UTF8.GetBytes(line), "the test");
        Simulation resumed = Simulation.Resume(Seed, RunSnapshotText.Read(ref reader), PartsMaps.VaultAndRoom, TestBattles.Content, TestBattles.Notices, TestStory.Content, DebugIntentHandlers.None);

        Assert.Contains("\"places\":[{\"map\":\"map.test_vault\",\"dead\":[\"patrol.test_vault_guard\"],\"doors\":[\"door.test_vault_plain\"],\"chests\":[{\"chest\":\"chest.test_vault_store\"", line, StringComparison.Ordinal);
        Assert.Equal(run.StateHash(), resumed.StateHash());
        Assert.Equal(line, RunSnapshotText.Write(resumed.Snapshot()));
    }

    [Fact]
    public void TheExitEntersItsMapAndRestoresNothing()
    {
        // Exit test 4 of PR-16 (D-1216, D-1217): the arrival on the exit enters the room.
        Simulation run = Resume(PartsMaps.Start(Seed), snapshot => snapshot with
        {
            Characters = snapshot.Characters! with { Characters = [snapshot.Characters.Characters[0] with { Health = 7 }] },
        });

        HubWalks.Walk(run, StepDirection.South, 1);

        Assert.Equal(TestMaps.Room.Id.Value, run.State.Party.Map.Id.Value);
        Assert.Equal(TestMaps.Room.Spawn, run.State.Party.LeadAt);
        Assert.Equal(7, run.State.Characters.Members[0].Health);
    }

    [Fact]
    public void AKilledEnemyStaysDeadAfterTheExitAndTheReturn()
    {
        // Exit test 6 of PR-16 (D-555).
        Simulation run = PartsMaps.Start(Seed);
        KillGuard(run);
        HubWalks.Walk(run, StepDirection.North, 3);
        HubWalks.Walk(run, StepDirection.West, 4);
        HubWalks.Walk(run, StepDirection.South, 1);
        Assert.Equal(TestMaps.Room.Id.Value, run.State.Party.Map.Id.Value);

        run.State.EnterMap(PartsMaps.Vault.Id, run.State.Context("test"));

        Assert.True(Assert.Single(run.State.Party.Patrols.All).Dead);
    }

    [Fact]
    public void AStoryEventThatReopensThePlaceBringsItsEnemyBackAtTheNextEntry()
    {
        // Exit test 7 of PR-16 (D-555): the victory flag reopens the vault, and the guard comes back
        // at the next entry, but not in front of the party.
        Simulation run = PartsMaps.Start(Seed);
        KillGuard(run);

        HubWalks.Walk(run, StepDirection.West, 4);
        RunToSceneEnd(run);

        Assert.False(run.State.Party.Place.IsDead(PartsMaps.Guard));
        Assert.True(Assert.Single(run.State.Party.Patrols.All).Dead);
        run.State.EnterMap(TestMaps.Room.Id, run.State.Context("test"));
        run.State.EnterMap(PartsMaps.Vault.Id, run.State.Context("test"));
        Assert.False(Assert.Single(run.State.Party.Patrols.All).Dead);
    }

    [Fact]
    public void ATwoCharacterPartyAfterADownReachesTheExitOfTheFixtureDungeon()
    {
        // Exit test 2 of PR-16 (F-7, D-1216): the way to the exit needs no key, no drill, and no fight.
        GameMap dungeon = PartsMaps.FixtureDungeonToRoom;
        Simulation start = Simulation.Start(Seed, MapSet.Of([dungeon, TestMaps.Room]), dungeon.Id, TestBattles.WithParty(2), TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
        Simulation run = Resume(start, snapshot => snapshot with
        {
            Characters = snapshot.Characters! with
            {
                Characters = [snapshot.Characters.Characters[0], snapshot.Characters.Characters[1] with { Health = 0 }],
            },
        });
        Assert.Equal(2, run.State.Characters.Members.Count);

        HubWalks.Walk(run, StepDirection.East, 12);
        HubWalks.Walk(run, StepDirection.North, 2);

        Assert.Equal(TestMaps.Room.Id.Value, run.State.Party.Map.Id.Value);
        Assert.Null(run.State.Battle);
    }

    [Fact]
    public void TheFixtureDungeonHoldsEachPartOfPr16()
    {
        // D-386, D-1216, D-1219, D-1220: a plain door, a story lock with its key in a chest, a
        // pickable lock, a chest with gold and an owned lesson, a save point, and the exit to the hub.
        GameMap map = TestMaps.FixtureDungeon;

        MapThing exit = Assert.Single(map.Things, thing => thing.Kind == MapThingKind.Exit);
        Assert.Equal("map.fixture_hub", exit.To?.Value);
        Assert.Contains(map.Things, thing => thing.Kind == MapThingKind.Lock && !thing.Pickable && thing.Key?.Value == "item.fixture_iron_key");
        Assert.Contains(map.Things, thing => thing.Kind == MapThingKind.Lock && thing.Pickable && thing.Key is null);
        Assert.Contains(map.Things, thing => thing.Contents is ChestContents contents && contents.Gold > 0 && contents.Entries.Any(entry => entry.Fallback is not null));
        Assert.Contains(map.Things, thing => thing.Contents is ChestContents contents && contents.Entries.Any(entry => entry.Thing.Value == "item.fixture_iron_key"));
        Assert.Equal(["flag.fixture_hub_yes"], HubWalks.Values(map.ReopenFlags));
    }

    /// <summary>
    /// Plays the vault to a state that holds each part of the memory of a map: the guard dead, the
    /// plain door open, and the chest opened with two draughts left in it. The fixture save of format
    /// 18 holds this state (D-166, D-555).
    /// </summary>
    /// <returns>The run, with the lead at (9, 2) facing the chest.</returns>
    internal static Simulation VaultWithMemory()
    {
        Simulation run = Simulation.Start(SaveRuns.Seed, PartsMaps.VaultAndRoom, PartsMaps.Vault.Id, TestBattles.Content, TestBattles.Notices, TestStory.Content, DebugIntentHandlers.None);
        KillGuard(run);
        HubWalks.Walk(run, StepDirection.North, 2);
        HubWalks.Walk(run, StepDirection.East, 4);
        HubWalks.Face(run, StepDirection.East);
        HubWalks.Confirm(run);
        _ = run.TakeNotices();
        return run;
    }

    /// <summary>Walks from the spawn point to (5, 2), north of the plain door.</summary>
    private static void ToPlainDoor(Simulation run)
    {
        HubWalks.Walk(run, StepDirection.East, 4);
        HubWalks.Walk(run, StepDirection.South, 1);
    }

    /// <summary>Opens the plain door, and walks into the hall to (5, 4).</summary>
    private static void IntoHall(Simulation run)
    {
        ToPlainDoor(run);
        HubWalks.Face(run, StepDirection.South);
        HubWalks.Confirm(run);
        HubWalks.Walk(run, StepDirection.South, 2);
    }

    /// <summary>Walks from the spawn point to (9, 2), and turns to the chest at (10, 2).</summary>
    private static void FaceChest(Simulation run)
    {
        HubWalks.Walk(run, StepDirection.East, 8);
        HubWalks.Walk(run, StepDirection.South, 1);
        HubWalks.Face(run, StepDirection.East);
    }

    /// <summary>
    /// Opens the plain door, walks the hall east to (8, 4), and steps east until the guard at (10, 4)
    /// meets the party. Then it wins the fight, ends it, and walks back to (5, 4).
    /// </summary>
    private static void KillGuard(Simulation run)
    {
        IntoHall(run);
        HubWalks.Walk(run, StepDirection.East, 3);
        for (int tick = 0; tick < 200 && run.State.Battle is null; tick += 1)
        {
            run.Step([HubWalks.Move(StepDirection.East)]);
        }

        Assert.Equal(BattleOutcome.Won, BattleRuns.FightToEnd(run, Seed));
        run.Step([Intent.OfPlayer(IntentIds.WaitBattleEnd)]);
        Assert.Null(run.State.Battle);
        Assert.True(run.State.Party.Place.IsDead(PartsMaps.Guard));
        HubWalks.Walk(run, StepDirection.West, run.State.Party.LeadAt.X - 5);
    }

    /// <summary>Puts the Theft drill of the tests in the second slot of Marrek from the menu (D-1050).</summary>
    private static void CarryPilfer(Simulation run)
    {
        run.State.Characters.AddLesson(TestBattles.Content.Lessons.Lesson(Pilfer), run.State.Context("test"));
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        run.Step([Intent.OfLessonSwap(0, 1, Pilfer)]);
        run.Step([Intent.OfPlayer(IntentIds.CloseMenu)]);
        Assert.True(DoorRules.CarriesTheft(run.State));
    }

    /// <summary>Runs the ticks of the one-step story scene of the victory flag.</summary>
    private static void RunToSceneEnd(Simulation run)
    {
        for (int tick = 0; tick < 4 && run.State.Story.Running; tick += 1)
        {
            run.Step([]);
        }

        Assert.False(run.State.Story.Running);
    }

    private static Simulation Resume(Simulation run, Func<RunSnapshot, RunSnapshot> change) =>
        Simulation.Resume(Seed, change(run.Snapshot()), run.State.Maps, run.State.BattleContent, TestBattles.Notices, TestStory.Content, DebugIntentHandlers.None);

    private static ContentId DoorOf(TilePoint at) => PartsMaps.Vault.DoorAt(at)!.Id;

    private static ContentId ChestId() => PartsMaps.Vault.ThingsAt(PartsMaps.Chest)[0].Id;

    private static ContentId Item(string name) => ContentId.Parse($"item.{name}", "test", "item");

    /// <summary>Gives each posted notice as its id, then its thing or its count.</summary>
    private static List<string> Posted(Simulation run)
    {
        List<string> lines = [];
        foreach (PostedNotice notice in run.TakeNotices())
        {
            string value = notice.Thing?.Value ?? notice.Count?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            lines.Add(value.Length == 0 ? notice.Id.Value : $"{notice.Id.Value} {value}");
        }

        return lines;
    }

    /// <summary>Compares two ids by their value, because a `ContentId` compares by reference (F-39).</summary>
    private sealed class IdComparer : IEqualityComparer<ContentId>
    {
        public static readonly IdComparer Instance = new();

        public bool Equals(ContentId? one, ContentId? other) => string.CompareOrdinal(one?.Value, other?.Value) == 0;

        public int GetHashCode(ContentId id) => StringComparer.Ordinal.GetHashCode(id.Value);
    }
}
