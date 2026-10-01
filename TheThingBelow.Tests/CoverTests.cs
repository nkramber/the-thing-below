using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The cover, the effect of a Guard drill (D-1352): the holder takes each melee strike that aims
/// at the ally that it covers, until its next turn, its fall, or the end of the battle.
/// </summary>
/// <remarks>
/// The party of these tests is Marrek in the front row and the third character in the back row.
/// The party steps into the guard from behind, so each character acts at tick 0, the third
/// character first at speed 120, and the grunt at tick 111 (D-768, D-769, D-770). Each character
/// carries a cover drill of delay 200, which holds past the next turn of the grunt, and every
/// roll is exact.
/// </remarks>
public sealed class CoverTests
{
    private const ulong Seed = 20260928;

    private static readonly ContentId Cover = ContentId.Parse("lesson.test_cover", "test", "lesson");

    private static readonly ContentId Guard = ContentId.Parse("lesson.test_guard", "test", "lesson");

    private static readonly BattleTarget Marrek = new(BattleSide.Party, 0);

    private static readonly BattleTarget Third = new(BattleSide.Party, 1);

    private static readonly BattleTarget Grunt = new(BattleSide.Enemy, 0);

    private static readonly Intent Defend = Intent.OfPlayer(IntentIds.BattleDefend);

    [Fact]
    public void AHolderTakesAMeleeStrikeAtTheAllyThatItCovers()
    {
        // D-1352: the grunt reaches Marrek alone, and the blow meets the third character in the back row.
        Simulation run = IntoFight(ContentWith(TestBattles.GruntFile));
        run.Step([Intent.OfBattleLesson(Cover, 0, Marrek)]);
        Assert.Equal(0, BattleRuns.BattleOf(run).Party[1].Covering);
        List<BattleEvent> covered = [.. run.TakeBattleEvents()];
        Assert.Contains(covered, one => one.Kind == BattleEventKind.Cover && one.Actor == Third && one.Target == Marrek);

        int marrek = BattleRuns.BattleOf(run).Party[0].Health;
        int third = BattleRuns.BattleOf(run).Party[1].Health;
        List<BattleEvent> events = StepUntilGruntStrikes(run, Defend);

        int taken = events.FindIndex(one => one.Kind == BattleEventKind.TakeBlow);
        Assert.True(taken >= 0, "No blow was taken.");
        Assert.Equal((Third, (BattleTarget?)Marrek), (events[taken].Actor, events[taken].Target));
        BattleEvent hit = events[taken + 1];
        Assert.Equal((BattleEventKind.Hit, Grunt, (BattleTarget?)Third), (hit.Kind, hit.Actor, hit.Target));
        Assert.Equal(marrek, BattleRuns.BattleOf(run).Party[0].Health);
        Assert.Equal(third - hit.Amount, BattleRuns.BattleOf(run).Party[1].Health);
    }

    [Theory]
    [InlineData("ability.test_shot")]
    [InlineData("ability.fixture_cinder")]
    public void AStrikeOfAnyReachGoesWhereItAims(string ability)
    {
        // D-377, D-1352: a shot and a rite reach either row, so Marrek covers the third character
        // for nothing. Marrek stands at 5 health, so the grunt expects the most damage on the
        // third character (D-959).
        string grunt = TestBattles.GruntFile.Replace("\"abilities\": []", $"\"abilities\": [\"{ability}\"]", StringComparison.Ordinal);
        Simulation run = IntoFight(ContentWith(grunt), slot => slot == 0 ? 5 : null);
        run.Step([Intent.OfPlayer(IntentIds.BattleAttack, Grunt, null)]);
        run.Step([Intent.OfBattleLesson(Guard, 0, Third)]);
        Assert.Equal(1, BattleRuns.BattleOf(run).Party[0].Covering);

        List<BattleEvent> events = StepUntilGruntStrikes(run, Intent.OfPlayer(IntentIds.BattleAttack, Grunt, null));

        Assert.Equal(1, BattleRuns.BattleOf(run).Party[0].Covering);
        Assert.DoesNotContain(events, one => one.Kind == BattleEventKind.TakeBlow);
        BattleEvent hit = events.Find(one => one.Kind == BattleEventKind.Hit && one.Actor == Grunt) ?? throw new InvalidOperationException("The grunt struck nobody.");
        Assert.Equal(Third, hit.Target);
        Assert.Equal(5, BattleRuns.BattleOf(run).Party[0].Health);
    }

    [Fact]
    public void TheCoverEndsAtTheNextTurnOfTheHolder()
    {
        // D-1352: the cover holds until the next turn of the holder, and the blow after it meets Marrek.
        Simulation run = IntoFight(ContentWith(TestBattles.GruntFile));
        run.Step([Intent.OfBattleLesson(Cover, 0, Marrek)]);
        StepUntilTurnOf(run, Third);

        Assert.Null(BattleRuns.BattleOf(run).Party[1].Covering);
        Assert.Contains(BattleEventKind.TakeBlow, BattleRuns.Kinds(run));

        int marrek = BattleRuns.BattleOf(run).Party[0].Health;
        List<BattleEvent> events = StepUntilGruntStrikes(run, Defend);
        Assert.DoesNotContain(events, one => one.Kind == BattleEventKind.TakeBlow);
        BattleEvent hit = events.Find(one => one.Kind == BattleEventKind.Hit && one.Actor == Grunt) ?? throw new InvalidOperationException("The grunt struck nobody.");
        Assert.Equal(Marrek, hit.Target);
        Assert.True(BattleRuns.BattleOf(run).Party[0].Health < marrek);
    }

    [Fact]
    public void TheCoverEndsWhenTheHolderFalls()
    {
        // D-1352: the holder at 1 health falls to the blow, and the next blow meets Marrek.
        BattleContent content = ContentWith(TestBattles.GruntFile);
        Simulation run = IntoFight(content, slot => slot == 1 ? 1 : null);
        run.Step([Intent.OfBattleLesson(Cover, 0, Marrek)]);

        List<BattleEvent> events = StepUntilGruntStrikes(run, Defend);

        int down = events.FindIndex(one => one.Kind == BattleEventKind.Down && one.Actor == Third);
        Assert.True(down > events.FindIndex(one => one.Kind == BattleEventKind.TakeBlow), "The holder did not fall to the blow.");
        Assert.Null(BattleRuns.BattleOf(run).Party[1].Covering);

        int marrek = BattleRuns.BattleOf(run).Party[0].Health;
        List<BattleEvent> after = StepUntilGruntStrikes(run, Defend);
        Assert.DoesNotContain(after, one => one.Kind == BattleEventKind.TakeBlow);
        Assert.True(BattleRuns.BattleOf(run).Party[0].Health < marrek, "The blow after the fall did not meet Marrek.");
    }

    [Fact]
    public void TheCoverEndsWithTheBattle()
    {
        // D-1352: no cover lasts past the end of a fight, and the values of an ended fight hold none.
        Simulation run = IntoFight(ContentWith(TestBattles.GruntFile));
        run.Step([Intent.OfBattleLesson(Cover, 0, Marrek)]);

        Assert.Equal(BattleOutcome.Won, BattleRuns.FightToEnd(run, Seed));

        Battle battle = BattleRuns.BattleOf(run);
        Assert.Null(battle.Party[1].Covering);
        Assert.Empty(battle.Values().Covers!);
    }

    [Fact]
    public void ASnapshotInTheMiddleOfACoverResumesToTheSameStateHash()
    {
        // D-166, D-1352: the snapshot and the state hash hold the cover, and the blow after the resume meets the holder.
        BattleContent content = ContentWith(TestBattles.GruntFile);
        Simulation run = IntoFight(content);
        run.Step([Intent.OfBattleLesson(Cover, 0, Marrek)]);

        string line = RunSnapshotText.Write(run.Snapshot());
        Assert.Contains("\"covers\":[{\"side\":\"party\",\"holder\":1,\"ally\":0}]", line, StringComparison.Ordinal);
        var reader = new ContentReader(Encoding.UTF8.GetBytes(line), "the test");
        Simulation resumed = Simulation.Resume(Seed, RunSnapshotText.Read(ref reader), BattleRuns.Map("group.one"), content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

        Assert.Equal(run.StateHash(), resumed.StateHash());
        Assert.Equal(0, BattleRuns.BattleOf(resumed).Party[1].Covering);
        Assert.Contains(StepUntilGruntStrikes(resumed, Defend), one => one.Kind == BattleEventKind.TakeBlow);
    }

    [Fact]
    public void TheStateHashReadsTheCover()
    {
        // G-5: two fights that differ in the cover alone give two hashes.
        BattleContent content = ContentWith(TestBattles.GruntFile);
        Simulation run = IntoFight(content);
        run.Step([Intent.OfBattleLesson(Cover, 0, Marrek)]);
        RunSnapshot snapshot = run.Snapshot();
        Simulation bare = Simulation.Resume(Seed, snapshot with { Battle = snapshot.Battle! with { Covers = [] } }, BattleRuns.Map("group.one"), content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

        Assert.NotEqual(run.StateHash(), bare.StateHash());
    }

    [Fact]
    public void TheLatestCoverOfAnAllyWins()
    {
        // D-1352: two holders never cover one ally, so the cover of the second character ends the cover of the third.
        string fixture = FixtureWith("\"start_party\": [\"character.marrek\", \"character.test_second\", \"character.test_third\"]")
            .Replace("{ \"character\": \"character.test_third\", \"lessons\": [\"lesson.test_cover\"] }", "{ \"character\": \"character.test_third\", \"lessons\": [\"lesson.test_cover\"] }, { \"character\": \"character.test_second\", \"lessons\": [\"lesson.test_ward\"] }", StringComparison.Ordinal);
        Simulation run = IntoFight(ContentWith(TestBattles.GruntFile, fixture));
        Assert.Same(BattleRuns.BattleOf(run).Party[2], BattleRuns.BattleOf(run).Next());
        run.Step([Intent.OfBattleLesson(Cover, 0, Marrek)]);
        Assert.Same(BattleRuns.BattleOf(run).Party[1], BattleRuns.BattleOf(run).Next());

        run.Step([Intent.OfBattleLesson(ContentId.Parse("lesson.test_ward", "test", "lesson"), 0, Marrek)]);

        Battle battle = BattleRuns.BattleOf(run);
        Assert.Null(battle.Party[2].Covering);
        Assert.Equal(0, battle.Party[1].Covering);
        Assert.Equal([new CoverValues(BattleSide.Party, 1, 0)], battle.Values().Covers!);
    }

    [Fact]
    public void ACoverAimsAtAnotherAllyWhoStands()
    {
        // D-1352: the holder never covers itself, an enemy, or a fallen ally.
        Simulation run = IntoFight(ContentWith(TestBattles.GruntFile), slot => slot == 0 ? 0 : null);

        Assert.Contains("a cover aims at another ally (D-1352)", BattleTurns.RefusalOf(run.State, Choice(Third)), StringComparison.Ordinal);
        Assert.Contains("aims at a character slot", BattleTurns.RefusalOf(run.State, Choice(Grunt)), StringComparison.Ordinal);
        Assert.Contains("reaches a character who stands", BattleTurns.RefusalOf(run.State, Choice(Marrek)), StringComparison.Ordinal);
        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfBattleLesson(Cover, 0, Third)]));
        Assert.Contains("D-1352", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("\"kind\": \"cover\", \"delay\": 200 }", "\"kind\": \"cover\", \"delay\": 200, \"power\": 100 }", "takes no field 'power'")]
    [InlineData("\"kind\": \"cover\", \"delay\": 200 }", "\"kind\": \"cover\", \"delay\": 200, \"reach\": \"melee\" }", "takes no field 'reach'")]
    [InlineData("\"kind\": \"cover\", \"delay\": 200 }", "\"kind\": \"cover\", \"delay\": 200, \"status\": \"haste\" }", "takes no field 'status'")]
    [InlineData("\"kind\": \"cover\", \"delay\": 200 }", "\"kind\": \"cover\" }", "delay")]
    [InlineData("\"kind\": \"cover\", \"delay\": 200 }", "\"kind\": \"cover\", \"delay\": 0 }", "outside 1 to")]
    [InlineData("\"kind\": \"cover\", \"delay\": 200 }", "\"kind\": \"shield\", \"delay\": 100 }", "steal, cover")]
    public void AnAbilityFileWithABadCoverFails(string from, string to, string named)
    {
        string text = Abilities().Replace(from, to, StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => AbilityList.Read(Encoding.UTF8.GetBytes(text), AbilityList.Path));

        Assert.Equal(AbilityList.Path, error.File);
        Assert.Contains(named, error.Field + " " + error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheAbilityFileReadsACover()
    {
        AbilityList list = AbilityList.Read(Encoding.UTF8.GetBytes(Abilities()), AbilityList.Path);

        CoverAbility cover = Assert.IsType<CoverAbility>(list.Ability(ContentId.Parse("ability.test_cover", "test", "ability")));
        Assert.Equal(200, cover.Delay);
    }

    [Fact]
    public void AnEnemyThatNamesACoverFailsAtTheLoad()
    {
        // D-955, D-1352: the evaluator scores a strike and a heal alone, so no enemy takes a cover.
        string grunt = TestBattles.GruntFile.Replace("\"abilities\": []", "\"abilities\": [\"ability.test_cover\"]", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => ContentWith(grunt));

        Assert.Equal((TestBattles.GruntPath, "ability.test_cover"), (error.File, error.Field));
        Assert.Contains("an enemy move is a strike or a heal", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheEvaluatorAndTheLegalIntentsHoldTheCover()
    {
        // D-65, D-1352: the grunt scores its turn beside a cover, and the accepted intents offer
        // the cover on Marrek alone, so a bot never sends a refused cover.
        Simulation run = IntoFight(ContentWith(TestBattles.GruntFile));
        run.Step([Intent.OfBattleLesson(Cover, 0, Marrek)]);
        Battle battle = BattleRuns.BattleOf(run);
        IReadOnlyList<ScoredAction> scored = BattleEvaluator.Score(battle, battle.Enemies[0], run.State.BattleContent, BattleEvaluator.StrikesOf(run.State, battle), run.State.Context("test"));
        Assert.NotEmpty(scored);

        List<Intent> covers = [];
        foreach (Intent intent in AcceptedIntents.Of(run.State))
        {
            if (string.CompareOrdinal(intent.Action.Value, IntentIds.BattleLesson.Value) == 0 && intent.Lesson is ContentId lesson && string.CompareOrdinal(lesson.Value, Guard.Value) == 0)
            {
                covers.Add(intent);
            }
        }

        Assert.Equal([Third], TargetsOf(covers));
    }

    [Fact]
    public void ASnapshotOfFormatTwentyWithACoverIsAnError()
    {
        // D-166, D-1352: format 20 predates the covers, so the field fails the read of that format.
        Simulation run = IntoFight(ContentWith(TestBattles.GruntFile));
        run.Step([Intent.OfBattleLesson(Cover, 0, Marrek)]);
        string line = RunSnapshotText.Write(run.Snapshot());
        string older = SnapshotLines.AsFormatTwenty(line);
        Assert.DoesNotContain("\"covers\"", older, StringComparison.Ordinal);
        string withCovers = older.Replace("\"steals\":", "\"covers\":[],\"steals\":", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => ReadAsFormatTwenty(withCovers));

        Assert.Contains("covers", error.Field + " " + error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(1, 1, "a cover names another slot")]
    [InlineData(1, 5, "a cover names another slot")]
    [InlineData(4, 0, "which the fight lacks")]
    public void AStoredCoverThatNoFightMakesFailsTheResume(int holder, int ally, string named)
    {
        // D-1352: the resume refuses a cover of the holder itself, of an absent slot, or of an absent holder.
        BattleContent content = ContentWith(TestBattles.GruntFile);
        Simulation run = IntoFight(content);
        RunSnapshot snapshot = run.Snapshot();
        RunSnapshot broken = snapshot with { Battle = snapshot.Battle! with { Covers = [new CoverValues(BattleSide.Party, holder, ally)] } };

        ArgumentException error = Assert.Throws<ArgumentException>(() => Simulation.Resume(Seed, broken, BattleRuns.Map("group.one"), content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None));

        Assert.Contains(named, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStoredCoverOfAFallenHolderFailsTheResume()
    {
        // D-1352: a fall ends a cover, so a fallen holder holds none.
        BattleContent content = ContentWith(TestBattles.GruntFile);
        Simulation run = IntoFight(content, slot => slot == 1 ? 0 : null);
        RunSnapshot snapshot = run.Snapshot();
        RunSnapshot broken = snapshot with { Battle = snapshot.Battle! with { Covers = [new CoverValues(BattleSide.Party, 1, 0)] } };

        ArgumentException error = Assert.Throws<ArgumentException>(() => Simulation.Resume(Seed, broken, BattleRuns.Map("group.one"), content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None));

        Assert.Contains("covers from the place 'down'", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Gives the ability file of the tests with three covers, one for each lesson, because each form has a flash of its own (D-1032).</summary>
    private static string Abilities() => TestBattles.AbilitiesFile.Replace(
        "{ \"id\": \"ability.test_pilfer\", \"kind\": \"steal\", \"delay\": 100 }",
        "{ \"id\": \"ability.test_pilfer\", \"kind\": \"steal\", \"delay\": 100 },\n      { \"id\": \"ability.test_cover\", \"kind\": \"cover\", \"delay\": 200 },\n" +
        "      { \"id\": \"ability.test_guard\", \"kind\": \"cover\", \"delay\": 200 },\n      { \"id\": \"ability.test_ward\", \"kind\": \"cover\", \"delay\": 200 }",
        StringComparison.Ordinal);

    /// <summary>Gives the lesson file of the tests with three Guard drills of the cover.</summary>
    private static string Lessons() => TestBattles.LessonsFile.Replace(
        "{ \"ability\": \"ability.test_pilfer\", \"points\": 0, \"ap\": 2, \"description\": \"lesson.test_pilfer\" } ] }",
        "{ \"ability\": \"ability.test_pilfer\", \"points\": 0, \"ap\": 2, \"description\": \"lesson.test_pilfer\" } ] },\n" +
        "      { \"id\": \"lesson.test_cover\", \"kind\": \"guard\", \"forms\": [{ \"ability\": \"ability.test_cover\", \"points\": 0, \"ap\": 1, \"description\": \"lesson.test_cover\" }] },\n" +
        "      { \"id\": \"lesson.test_guard\", \"kind\": \"guard\", \"forms\": [{ \"ability\": \"ability.test_guard\", \"points\": 0, \"ap\": 1, \"description\": \"lesson.test_guard\" }] },\n" +
        "      { \"id\": \"lesson.test_ward\", \"kind\": \"guard\", \"forms\": [{ \"ability\": \"ability.test_ward\", \"points\": 0, \"ap\": 1, \"description\": \"lesson.test_ward\" }] }",
        StringComparison.Ordinal);

    /// <summary>
    /// Gives the fixture of the tests with one start party: Marrek carries the hew and the guard,
    /// and the third character carries the cover.
    /// </summary>
    private static string FixtureWith(string startParty) => TestBattles.FixtureFile
        .Replace("\"start_party\": [\"character.marrek\"]", startParty, StringComparison.Ordinal)
        .Replace(
            "\"start_lessons\": [{ \"character\": \"character.marrek\", \"lessons\": [\"lesson.fixture_hew\", \"lesson.fixture_cinder\"] }]",
            "\"start_lessons\": [{ \"character\": \"character.marrek\", \"lessons\": [\"lesson.fixture_hew\", \"lesson.test_guard\"] }, { \"character\": \"character.test_third\", \"lessons\": [\"lesson.test_cover\"] }]",
            StringComparison.Ordinal);

    private static BattleContent ContentWith(string grunt, string? fixture = null) =>
        TestBattles.WithLessonFiles(
            fixture ?? FixtureWith("\"start_party\": [\"character.marrek\", \"character.test_third\"]"),
            Lessons(),
            Abilities(),
            grunt,
            exact: true);

    /// <summary>Starts the run, sets the health of a character by its slot where the change gives one, and steps into the fight with one grunt.</summary>
    private static Simulation IntoFight(BattleContent content, Func<int, int?>? health = null)
    {
        GameMap map = BattleRuns.Map("group.one");
        Simulation run = TestParty.StartEach(Seed, (slot, stored) => health?.Invoke(slot) is int set ? stored with { Health = set } : stored, content, map);
        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        Assert.NotNull(run.State.Battle);
        _ = run.TakeBattleEvents();
        return run;
    }

    /// <summary>Defends on each turn of another character until the turn of the character opens.</summary>
    private static void StepUntilTurnOf(Simulation run, BattleTarget character)
    {
        for (int turn = 0; turn < 10; turn += 1)
        {
            if (BattleRuns.BattleOf(run).Next()?.Target == character)
            {
                return;
            }

            run.Step([Defend]);
        }

        throw new InvalidOperationException($"Seed {Seed}: the turn of {character.Describe()} never opened.");
    }

    /// <summary>Sends one intent on each turn of a character until the grunt strikes, and gives each event since the last take.</summary>
    private static List<BattleEvent> StepUntilGruntStrikes(Simulation run, Intent act)
    {
        List<BattleEvent> events = [.. run.TakeBattleEvents()];
        for (int turn = 0; turn < 10; turn += 1)
        {
            run.Step([act]);
            events.AddRange(run.TakeBattleEvents());
            if (events.Exists(one => one.Actor == Grunt && one.Kind is BattleEventKind.Hit or BattleEventKind.Miss))
            {
                return events;
            }
        }

        throw new InvalidOperationException($"Seed {Seed}: the grunt never struck.");
    }

    private static RunSnapshot ReadAsFormatTwenty(string line)
    {
        var reader = new ContentReader(Encoding.UTF8.GetBytes(line), "the test");
        return RunSnapshotText.ReadFormatTwenty(ref reader);
    }

    private static BattleChoice Choice(BattleTarget target) => new(BattleAction.Lesson, target, null, Cover, 0);

    private static List<BattleTarget> TargetsOf(List<Intent> intents)
    {
        List<BattleTarget> targets = [];
        foreach (Intent intent in intents)
        {
            targets.Add(intent.Target ?? throw new InvalidOperationException("A lesson intent names no target."));
        }

        return targets;
    }
}
