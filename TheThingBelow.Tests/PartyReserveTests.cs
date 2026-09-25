using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Hashing;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Story;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The reserve and the party swap of PR-14 (D-58, D-1134 to D-1136). The class covers the Core
/// part of exit tests 4, 10, 11, and 12. The fixture group of four of exit test 1 comes from
/// <see cref="TestParty.StartFour(ulong, GameMap)"/>.
/// </summary>
public sealed class PartyReserveTests
{
    private const ulong Seed = 20260925;

    private static readonly Intent OpenMenu = Intent.OfPlayer(IntentIds.OpenMenu);

    [Fact]
    public void TheGroupOfFourHoldsThreeInThePartyAndOneInTheReserve()
    {
        // Exit test 1 of PR-14 (D-362, D-1136): the party stays full, and the fourth waits.
        Simulation run = TestParty.StartFour(Seed, TestMaps.Room);

        Assert.Equal(["character.marrek", "character.test_second", "character.test_third"], Ids(run.State.Characters.Members));
        Assert.Equal([TestParty.Fourth.Value], Ids(run.State.Characters.Reserve));
    }

    [Fact]
    public void ASwapMovesOneCharacterOutAndOneIn()
    {
        // Exit test 11 of PR-14 (D-1136): the two trade places, so the party stays at three.
        Simulation run = TestParty.StartFour(Seed, TestMaps.Room);
        run.Step([OpenMenu]);

        run.Step([Intent.OfPartySwap(1, 0)]);

        Assert.Equal(["character.marrek", TestParty.Fourth.Value, "character.test_third"], Ids(run.State.Characters.Members));
        Assert.Equal(["character.test_second"], Ids(run.State.Characters.Reserve));
    }

    [Fact]
    public void AJoinIntoAFullPartyGoesToTheReserve()
    {
        // Exit test 11 of PR-14 (D-1136): a join step of a story scene fills the party first, and
        // a party of three sends the new character to the reserve.
        StoryScene join = TestStory.Scene(
            """
            {
             "comment": "The fourth character joins a full party.",
             "id": "scene.test_meet",
             "steps": [{ "id": "step.s1", "kind": "join", "character": "character.test_fourth" }]
            }
            """,
            "join");
        Simulation run = StartWithScene(join);

        run.Step([]);

        Assert.Equal(["character.marrek", "character.test_second", "character.test_third"], Ids(run.State.Characters.Members));
        PartyMember joined = Assert.Single(run.State.Characters.Reserve);
        Assert.Equal((TestParty.Fourth.Value, 1, joined.Stats.Health, joined.Stats.Mp), (joined.Record.Id.Value, joined.Level, joined.Health, joined.Mp));
    }

    [Fact]
    public void AJoinOfACharacterInTheReserveFails()
    {
        // D-563, D-1136: the story adds each character one time, so a second join is a fault of the content.
        StoryScene join = TestStory.Scene(
            """
            {
             "comment": "The fourth character joins two times.",
             "id": "scene.test_meet",
             "steps": [{ "id": "step.s1", "kind": "join", "character": "character.test_fourth" }, { "id": "step.s2", "kind": "join", "character": "character.test_fourth" }]
            }
            """,
            "twice");
        Simulation run = StartWithScene(join);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([]));

        Assert.Contains("'character.test_fourth', who is already in the reserve", error.Message, StringComparison.Ordinal);
        Assert.Contains("scene.test_meet/steps[1]", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ADownedCharacterGoesToTheReserveAndStaysDown()
    {
        // Exit test 10 of PR-14 (D-1135): a down has a replacement, and the down lasts in the reserve.
        Simulation run = TestParty.StartFour(Seed, TestMaps.Room, TestParty.FourContent, (place, stored) => place == 2 ? stored with { Health = 0 } : stored);
        run.Step([OpenMenu]);

        run.Step([Intent.OfPartySwap(2, 0)]);

        Assert.Equal(TestParty.Fourth.Value, run.State.Characters.Members[2].Record.Id.Value);
        PartyMember waiting = Assert.Single(run.State.Characters.Reserve);
        Assert.Equal("character.test_third", waiting.Record.Id.Value);
        Assert.True(waiting.Down);
    }

    [Fact]
    public void ADownedReserveCharacterNeverComesIn()
    {
        // Exit test 10 of PR-14 (D-1135): the rule names the reason, and the intent fails with it.
        Simulation run = TestParty.StartFour(Seed, TestMaps.Room, TestParty.FourContent, (place, stored) => place == 3 ? stored with { Health = 0 } : stored);
        run.Step([OpenMenu]);

        Assert.Contains("D-1135", run.State.Characters.RefusalOfReserveSwap(0, 0), StringComparison.Ordinal);
        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfPartySwap(0, 0)]));

        Assert.Contains("the downed reserve character 'character.test_fourth'", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-1135", error.Message, StringComparison.Ordinal);
        Assert.Equal("character.marrek", run.State.Characters.Members[0].Record.Id.Value);
    }

    [Fact]
    public void ASwapInsideABattleIsRefused()
    {
        // Exit test 12 of PR-14 (D-1134): no swap happens inside a battle, with the menu open too.
        Simulation run = TestParty.StartFour(Seed, BattleRuns.Map("group.test_pair"));
        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        Assert.NotNull(run.State.Battle);
        run.Step([OpenMenu]);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfPartySwap(0, 0)]));

        Assert.Contains("a battle holds the run", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-1134", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASwapWhileAnEncounterLeadsIntoABattleIsRefused()
    {
        // D-1134: an encounter becomes a battle on the next world step, so it is the start of a fight.
        GameMap map = BattleRuns.Map("group.test_pair");
        RunSnapshot start = TestParty.StartFour(Seed, map).Snapshot();
        MapEncounter encounter = new(map.Patrols[0].Id, map.Patrols[0].Group, EncounterSide.None);
        Simulation run = Simulation.Resume(Seed, start with { Map = start.Map! with { Encounter = encounter } }, map, TestParty.FourContent, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
        run.Step([OpenMenu]);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfPartySwap(0, 0)]));

        Assert.Contains("an encounter leads into a battle", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASwapWithNoMenuOpenIsRefused()
    {
        // D-1134: the Party window makes the swap, so a swap on the walk points at a fault.
        Simulation run = TestParty.StartFour(Seed, TestMaps.Room);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfPartySwap(0, 0)]));

        Assert.Contains("no menu is open", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(3, 0, "the party slot 3")]
    [InlineData(-1, 0, "the party slot -1")]
    [InlineData(0, 1, "the reserve index 1")]
    [InlineData(0, -1, "the reserve index -1")]
    public void ASwapOfNoCharacterIsRefused(int slot, int reserve, string reason)
    {
        Simulation run = TestParty.StartFour(Seed, TestMaps.Room);
        run.Step([OpenMenu]);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfPartySwap(slot, reserve)]));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASwapWithAnEmptyReserveIsRefused()
    {
        // D-1136: Marrek alone has no reserve, so no swap is legal.
        Simulation run = Simulation.Start(Seed, TestMaps.Room, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
        run.Step([OpenMenu]);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfPartySwap(0, 0)]));

        Assert.Contains("the reserve holds no character", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASwapIntentThatLacksAnIndexIsRefused()
    {
        // T-2: the swap reads the party slot from the actor and the reserve index from the option.
        Simulation run = TestParty.StartFour(Seed, TestMaps.Room);
        run.Step([OpenMenu]);

        SimulationException noSlot = Assert.Throws<SimulationException>(() => run.Step([new Intent(IntentIds.PartySwap, false, Option: 0)]));
        SimulationException noIndex = Assert.Throws<SimulationException>(() => run.Step([new Intent(IntentIds.PartySwap, false, Actor: 0)]));

        Assert.Contains("names no party slot", noSlot.Message, StringComparison.Ordinal);
        Assert.Contains("names no reserve index", noIndex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheLeadGoesToTheReserveAndStillWalksTheMap()
    {
        // Exit test 4 of PR-14, the Core part (D-292, D-306): the map follows the lead, so a swap
        // of Marrek to the reserve leaves the tile, the facing, and the step that the camera reads.
        Simulation run = TestParty.StartFour(Seed, TestMaps.Room);
        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        run.Step([]);
        run.Step([OpenMenu]);
        MapState map = run.State.Party;
        (TilePoint, StepDirection, StepDirection?, int, ulong) before = (map.LeadAt, map.Facing, map.Stepping, map.StepTicks, MapHash(map));
        Assert.Equal(StepDirection.East, map.Stepping);

        run.Step([Intent.OfPartySwap(0, 0)]);

        Assert.Equal("character.marrek", Assert.Single(run.State.Characters.Reserve).Record.Id.Value);
        Assert.Equal(before, (map.LeadAt, map.Facing, map.Stepping, map.StepTicks, MapHash(map)));
        run.Step([Intent.OfPlayer(IntentIds.CloseMenu)]);
        for (int tick = 0; tick < MapRules.TicksPerStep; tick += 1)
        {
            run.Step([]);
        }

        Assert.Equal(new TilePoint(3, 2), run.State.Party.LeadAt);
    }

    [Fact]
    public void ARestAtAHubFillsAndCuresTheReserve()
    {
        // D-36, D-390, D-1135: a down lasts in the reserve until a rest, and the rest reaches the reserve.
        Simulation run = TestParty.StartFour(Seed, TestMaps.Room, TestParty.FourContent, (place, stored) => place == 3 ? stored with { Health = 0, Growth = stored.Growth! with { Mp = 1 } } : stored);
        PartyMember waiting = Assert.Single(run.State.Characters.Reserve);
        Assert.True(waiting.Down);

        run.State.Characters.RestAtHub();

        Assert.Equal((waiting.Stats.Health, waiting.Stats.Mp), (waiting.Health, waiting.Mp));
        Assert.Empty(waiting.Statuses);
    }

    [Fact]
    public void ARestCuresAReserveCharacterOfPoison()
    {
        // D-390: poison, blind, and silence last past a fight, in the reserve too, until a rest.
        Simulation run = TestParty.StartFour(Seed, TestMaps.Room, TestParty.FourContent, (place, stored) => place == 3 ? stored with { Health = 5, Statuses = [StatusKind.Poison, StatusKind.Silence] } : stored);

        run.State.Characters.RestAtHub();

        PartyMember waiting = Assert.Single(run.State.Characters.Reserve);
        Assert.Empty(waiting.Statuses);
        Assert.Equal(waiting.Stats.Health, waiting.Health);
    }

    [Fact]
    public void ASavePointFillsTheMpOfTheReserve()
    {
        // D-389, D-967: a save point fills the MP of each character and no health.
        Simulation run = TestParty.StartFour(Seed, TestMaps.Room, TestParty.FourContent, (place, stored) => place == 3 ? stored with { Health = 5, Growth = stored.Growth! with { Mp = 1 } } : stored);

        run.State.Characters.RestoreAtSavePoint();

        PartyMember waiting = Assert.Single(run.State.Characters.Reserve);
        Assert.Equal((5, waiting.Stats.Mp), (waiting.Health, waiting.Mp));
    }

    [Fact]
    public void TheOwnedLessonsReadTheReserve()
    {
        // D-1023, D-1024: Marrek keeps his lessons in the reserve, so the player still owns them.
        Simulation run = TestParty.StartFour(Seed, TestMaps.Room);
        run.Step([OpenMenu]);
        run.Step([Intent.OfPartySwap(0, 0)]);

        Assert.True(run.State.Characters.Owns(ContentId.Parse("lesson.fixture_hew", "test", "lesson")));
    }

    [Fact]
    public void AStoredReserveThatCarriesALessonOfThePackIsRefused()
    {
        // D-1023: the player never owns two copies of one lesson, across the party, the reserve, and the pack.
        ContentId salve = ContentId.Parse("lesson.fixture_salve", "test", "lesson");

        ArgumentException error = Assert.Throws<ArgumentException>(() => TestParty.StartFour(Seed, TestMaps.Room, TestParty.FourContent, (place, stored) =>
            place == 3 ? stored with { Lessons = new LessonValues([salve, null], [new LessonPoints(salve, 0)]) } : stored));

        Assert.Contains("owns 'lesson.fixture_salve' two times", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStoredReserveBesideAPartyOfFewerThanThreeIsRefused()
    {
        // D-1136: a join fills the party first, so no run holds a reserve beside a party of two.
        RunSnapshot four = TestParty.StartFour(Seed, TestMaps.Room).Snapshot();
        PartySnapshot party = four.Characters!;
        RunSnapshot two = four with { Characters = party with { Characters = [party.Characters[0], party.Characters[1]] } };

        ArgumentException error = Assert.Throws<ArgumentException>(() =>
            Simulation.Resume(Seed, two, TestMaps.Room, TestParty.FourContent, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None));

        Assert.Contains("1 characters in the reserve while the party holds 2", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-1136", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStoredCharacterInThePartyAndTheReserveIsRefused()
    {
        RunSnapshot four = TestParty.StartFour(Seed, TestMaps.Room).Snapshot();
        PartySnapshot party = four.Characters!;
        RunSnapshot twice = four with { Characters = party with { Reserve = [party.Characters[1]] } };

        ArgumentException error = Assert.Throws<ArgumentException>(() =>
            Simulation.Resume(Seed, twice, TestMaps.Room, TestParty.FourContent, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None));

        Assert.Contains("the character 'character.test_second' two times", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheReserveSurvivesASnapshotTextAndAResume()
    {
        // G-5, D-1136: the snapshot text holds the reserve, and the resume keeps the state hash.
        Simulation run = TestParty.StartFour(Seed, TestMaps.Room);
        run.Step([OpenMenu]);
        run.Step([Intent.OfPartySwap(0, 0)]);
        string text = RunSnapshotText.Write(run.Snapshot());
        var reader = new ContentReader(System.Text.Encoding.UTF8.GetBytes(text), "the test");

        Simulation resumed = Simulation.Resume(Seed, RunSnapshotText.Read(ref reader), TestMaps.Room, TestParty.FourContent, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

        Assert.Contains("\"reserve\":[{\"id\":\"character.marrek\"", text, StringComparison.Ordinal);
        Assert.Equal(["character.marrek"], Ids(resumed.State.Characters.Reserve));
        Assert.Equal(run.StateHash(), resumed.StateHash());
    }

    [Fact]
    public void TheStateHashHoldsTheReserve()
    {
        // G-5: two runs that differ in the reserve alone give two hashes.
        Simulation full = TestParty.StartFour(Seed, TestMaps.Room);
        Simulation hurt = TestParty.StartFour(Seed, TestMaps.Room, TestParty.FourContent, (place, stored) => place == 3 ? stored with { Health = 1 } : stored);

        Assert.NotEqual(full.StateHash(), hurt.StateHash());
    }

    [Fact]
    public void ARecordOfASwapReplaysToTheSameStateHash()
    {
        // G-5, D-1134: the record text carries the party slot and the reserve index of the swap.
        Simulation run = TestParty.StartFour(Seed, TestMaps.Room);
        RunRecorder recorder = new(RunHeader.ForThisBuild("a-content-hash", Seed), run.Snapshot());
        IReadOnlyList<Intent>[] script = [[OpenMenu], [Intent.OfPartySwap(2, 0)], [Intent.OfPlayer(IntentIds.CloseMenu)], [Intent.OfPlayer(IntentIds.MoveSouth)], []];
        foreach (IReadOnlyList<Intent> intents in script)
        {
            run.Step(intents);
            recorder.Step(run.Tick, intents);
        }

        string text = RunRecordText.Write(recorder.Build());
        RunRecord read = RunRecordText.Read(text);
        RunState replayed = RunReplay.Play(read, "a-content-hash", TestMaps.Room, TestParty.FourContent, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

        Assert.Contains("\"action\":\"intent.party_swap\"", text, StringComparison.Ordinal);
        Intent swap = read.Ticks[1].Intents[0];
        Assert.Equal((2, 0), (swap.Actor, swap.Option));
        Assert.Equal(TestParty.Fourth.Value, replayed.Characters.Members[2].Record.Id.Value);
        Assert.Equal(run.StateHash(), replayed.StateHash());
    }

    private static Simulation StartWithScene(StoryScene meet)
    {
        BattleContent content = TestParty.FourContent;
        StoryContent story = StoryContent.Load(TestStory.Flags, [meet, TestStory.Scene(TestStory.FightFile, "fight"), TestStory.Scene(TestStory.VictoryFile, "victory")], content);
        return Simulation.Start(Seed, TestStory.Map, content, TestBattles.Notices, story, DebugIntentHandlers.None);
    }

    private static ulong MapHash(MapState map)
    {
        StateHasher hasher = new();
        map.Hash(hasher);
        return hasher.Finish();
    }

    private static List<string> Ids(IReadOnlyList<PartyMember> members)
    {
        List<string> ids = [];
        foreach (PartyMember member in members)
        {
            ids.Add(member.Record.Id.Value);
        }

        return ids;
    }
}
