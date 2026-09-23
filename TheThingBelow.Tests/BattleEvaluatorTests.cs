using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Streams;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The evaluator of the enemies: the legal actions, the expected score, the reply of the next
/// character, and the tie-break (D-65, D-534, D-947, D-955, D-959, D-960). The exit tests of
/// PR-11 live here, and <c>BattleFixtureTests</c> holds exit test 3.
/// </summary>
public sealed class BattleEvaluatorTests
{
    /// <summary>The count of seeds of each property test (T-3).</summary>
    private const int SeedCount = 1000;

    /// <summary>The grunt of the tests, with the mend and the shot (D-955).</summary>
    private static readonly string Mender = TestBattles.GruntFile.Replace(
        "\"abilities\": []", "\"abilities\": [\"ability.test_mend\", \"ability.test_shot\"]", StringComparison.Ordinal);

    private const string ProtectorProfile = """
    {
     "comment": "A protector of the tests: the health of an ally above all.",
     "id": "profile.test_protector",
     "weights": { "damage": 50, "kills": 0, "threat": 0, "healing": 300, "timeline": 0, "row": 0 },
     "steal_chance": 0,
     "steal": []
    }
    """;

    private const string IdleProfile = """
    {
     "comment": "A profile of the tests with no weight, so every action ties.",
     "id": "profile.test_idle",
     "weights": { "damage": 0, "kills": 0, "threat": 0, "healing": 0, "timeline": 0, "row": 0 },
     "steal_chance": 0,
     "steal": []
    }
    """;

    private const string CarefulProfile = """
    {
     "comment": "A careful profile of the tests: damage, the reply, and the rows.",
     "id": "profile.test_careful",
     "weights": { "damage": 100, "kills": 3, "threat": 80, "healing": 150, "timeline": 1, "row": 200 },
     "steal_chance": 0,
     "steal": []
    }
    """;

    /// <summary>The groups of these tests, beside the groups of <see cref="TestBattles.GroupsFile"/>.</summary>
    private const string EvaluatorGroups = """
      {
       "id": "group.test_ward",
       "boss": false,
       "enemies": [
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": false, "profile": "profile.test_attacker" },
        { "enemy": "enemy.fixture_grunt", "row": "back", "waits": false, "profile": "profile.test_protector" }
       ]
      },
      { "id": "group.test_idle", "boss": false, "enemies": [{ "enemy": "enemy.fixture_grunt", "row": "front", "waits": false, "profile": "profile.test_idle" }] },
      {
       "id": "group.test_careful",
       "boss": false,
       "enemies": [
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": false, "profile": "profile.test_careful" },
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": false, "profile": "profile.test_careful" },
        { "enemy": "enemy.fixture_grunt", "row": "back", "waits": false, "profile": "profile.test_protector" },
        { "enemy": "enemy.fixture_grunt", "row": "front", "waits": true, "profile": "profile.test_careful" }
       ]
      },
    """;

    [Fact]
    public void AProtectorHealsItsHurtAllyBeforeItAttacks()
    {
        // Exit test 1 of PR-11 (D-65, D-955): the first act of the protector binds the ally
        // that the party struck, and the heal stops at the full health of that ally.
        Simulation run = BattleRuns.IntoBattle(1, "group.test_ward", Content(exact: true));
        List<BattleEvent> events = PlayUntilEnemyActs(run, attackFirst: true);

        BattleEvent first = FirstActOf(events, new BattleTarget(BattleSide.Enemy, 1));
        Assert.Equal(BattleEventKind.Heal, first.Kind);
        Assert.Equal(new BattleTarget(BattleSide.Enemy, 0), first.Target);

        // The mend of 20 restores what the blow of the party took, and no more (D-955).
        BattleEvent blow = events.Find(played => played.Kind == BattleEventKind.Hit && played.Target == new BattleTarget(BattleSide.Enemy, 0))
            ?? throw new InvalidOperationException("The party struck no enemy.");
        Assert.InRange(blow.Amount, 1, 19);
        Assert.Equal(blow.Amount, first.Amount);
        Assert.Equal(30, BattleRuns.BattleOf(run).Enemies[0].Health);
    }

    [Fact]
    public void AProtectorWithNoHurtAllyAttacks()
    {
        // D-959: a heal of an ally at full health restores nothing and scores no healing.
        Simulation run = BattleRuns.IntoBattle(1, "group.test_ward", Content(exact: true));
        List<BattleEvent> events = PlayUntilEnemyActs(run, attackFirst: false);

        BattleEvent first = FirstActOf(events, new BattleTarget(BattleSide.Enemy, 1));
        Assert.True(first.Kind == BattleEventKind.Hit, $"The protector took '{first.Describe()}', and an attack scores highest with no hurt ally.");
        Assert.Equal(BattleSide.Party, first.Target?.Side);
    }

    [Fact]
    public void AnEmptyListOfActionsFailsTheLoadWithTheGroupTheEnemyAndTheProfile()
    {
        // Exit test 2 of PR-11 (D-948, D-962): no content of PR-11 can empty the list, so the
        // guard takes the list itself.
        BattleContent content = Content(exact: true);
        GroupRecord ward = content.Group(Id("group.test_ward"));

        ContentException error = Assert.Throws<ContentException>(
            () => BattleContent.RequireLegalAction([], TestBattles.GroupsPath, ward, ward.Entries[1]));

        Assert.Equal(TestBattles.GroupsPath, error.File);
        Assert.Contains("group.test_ward", error.Message, StringComparison.Ordinal);
        Assert.Contains("enemy.fixture_grunt", error.Message, StringComparison.Ordinal);
        Assert.Contains("profile.test_protector", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-948", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheLegalActionsFollowTheFixedOrder()
    {
        // D-947 and D-962: each attack, each ability with each target, the defend, and the step.
        // The basic attack, the defend, and the step keep each list from empty.
        BattleContent content = TestBattles.Content;
        Simulation run = BattleRuns.IntoBattle(1, "group.test_elite", content);
        Battle battle = BattleRuns.BattleOf(run);

        IReadOnlyList<EnemyAction> actions = BattleEvaluator.LegalActions(battle, battle.Enemies[0], content);

        Assert.Equal(new List<string> { "attack at party 0", "ability ability.fixture_bash at party 0", "defend", "step" }, Describe(actions));
    }

    [Fact]
    public void AStrikeOfAnyReachAimsAtTheBackRowWhileTheFrontStands()
    {
        // D-377 and D-955: the basic attack reaches the front row alone, and the shot reaches both.
        BattleContent content = Content(exact: true);
        Simulation run = BattleRuns.IntoBattle(1, "group.test_ward", content);
        Battle battle = BattleRuns.BattleOf(run);

        List<string> actions = Describe(BattleEvaluator.LegalActions(battle, battle.Enemies[0], content));

        Assert.Contains("attack at party 0", actions);
        Assert.DoesNotContain("attack at party 2", actions);
        Assert.Contains("ability ability.test_shot at party 2", actions);
        Assert.Contains("ability ability.test_mend at enemy 1", actions);
    }

    [Fact]
    public void TheExpectedDamageOfAnAttackIsTheDamageOfTheBlow()
    {
        // D-959: with no miss and the factor 10000, the expected damage equals the blow. 8 attack
        // at 100 over 104 gives 7 against Marrek, and no kill of 60 health.
        BattleContent content = Content(exact: true);
        Simulation run = BattleRuns.IntoBattle(1, "group.test_idle", content);
        Battle battle = BattleRuns.BattleOf(run);

        ScoredAction attack = ScoreOf(battle, content, run, "attack at party 0");

        Assert.Equal(7, attack.Terms.Damage);
        Assert.Equal(0, attack.Terms.Kills);
        Assert.Equal(0, attack.Terms.Healing);
        Assert.Equal(Battle.Push(100, 90, BasisPoints.One, run.State.Context("test")), attack.Terms.Timeline);
    }

    [Fact]
    public void ADefendCutsTheThreatOfTheReply()
    {
        // D-755 and D-960: the next character strikes the lone grunt, so a defend halves the reply.
        BattleContent content = Content(exact: true);
        Simulation run = BattleRuns.IntoBattle(1, "group.test_idle", content);
        Battle battle = BattleRuns.BattleOf(run);

        ScoredAction attack = ScoreOf(battle, content, run, "attack at party 0");
        ScoredAction defend = ScoreOf(battle, content, run, "defend");

        Assert.True(attack.Terms.Threat > 0, "The reply of the next character takes health from the grunt.");
        Assert.Equal(attack.Terms.Threat / 2, defend.Terms.Threat);
    }

    [Fact]
    public void AStepBehindAnAllyTakesTheStepperOutOfMeleeReach()
    {
        // D-377: two grunts stand in front, so a step back leaves one of them out of reach.
        BattleContent content = Content(exact: true);
        Simulation run = BattleRuns.IntoBattle(1, "group.test_careful", content);
        Battle battle = BattleRuns.BattleOf(run);

        ScoredAction step = ScoreOf(battle, content, run, "step");
        ScoredAction defend = ScoreOf(battle, content, run, "defend");

        Assert.Equal(1, step.Terms.Row);
        Assert.Equal(0, defend.Terms.Row);
    }

    [Fact]
    public void OneStateGivesOneScoreOnEachCall()
    {
        // D-959: a score draws no roll, so two calls on one state agree.
        BattleContent content = Content(exact: false);
        Simulation run = BattleRuns.IntoBattle(5, "group.test_careful", content);
        Battle battle = BattleRuns.BattleOf(run);
        RunContext context = run.State.Context("test");

        IReadOnlyList<ScoredAction> first = BattleEvaluator.Score(battle, battle.Enemies[0], content, context);
        IReadOnlyList<ScoredAction> second = BattleEvaluator.Score(battle, battle.Enemies[0], content, context);

        Assert.Equal(first, second);
    }

    [Fact]
    public void ATieDrawsFromTheStreamOfTheEvaluatorOverEachSeed()
    {
        // Exit test 5 of PR-11 (D-947): with no weight, every legal action ties, and the draw of
        // the stream of the evaluator picks the action by the fixed order of the list.
        BattleContent content = Content(exact: true);
        Simulation run = BattleRuns.IntoBattle(1, "group.test_idle", content);
        Battle battle = BattleRuns.BattleOf(run);
        Combatant grunt = battle.Enemies[0];
        RunContext context = run.State.Context("test");
        IReadOnlyList<EnemyAction> legal = BattleEvaluator.LegalActions(battle, grunt, content);
        var chosen = new SortedSet<string>(StringComparer.Ordinal);

        for (ulong seed = 0; seed < SeedCount; seed += 1)
        {
            int index = RandomStreams.Open(seed, StreamId.Evaluator).NextInt(legal.Count, context);
            EnemyAction action = BattleEvaluator.Choose(battle, grunt, content, RandomStreams.Open(seed, StreamId.Evaluator), context);

            Assert.True(legal[index].Describe() == action.Describe(), $"Seed {seed}: the draw gives '{legal[index].Describe()}', and the evaluator took '{action.Describe()}'.");
            chosen.Add(action.Describe());
        }

        Assert.Equal(legal.Count, chosen.Count);
    }

    [Fact]
    public void AChoiceFromAnotherStreamFailsWithTheStream()
    {
        // D-947 and G-4: a tie never moves a roll of the battle stream.
        BattleContent content = Content(exact: true);
        Simulation run = BattleRuns.IntoBattle(1, "group.test_idle", content);
        Battle battle = BattleRuns.BattleOf(run);

        SimulationException error = Assert.Throws<SimulationException>(
            () => BattleEvaluator.Choose(battle, battle.Enemies[0], content, RandomStreams.Open(1, StreamId.Battle), run.State.Context("test")));

        Assert.Contains("D-947", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheEvaluatorNeverStallsATurnOverEachSeed()
    {
        // Exit test 4 of PR-11: each fight of the careful group ends in a win or a wipe, and
        // each enemy turn does one action. A stall runs past the limit of the turns.
        BattleContent content = Content(exact: false);
        for (ulong seed = 0; seed < SeedCount; seed += 1)
        {
            Simulation run = BattleRuns.IntoBattle(seed, "group.test_careful", content);
            BattleOutcome outcome = BattleRuns.FightToEnd(run, seed);

            Assert.True(outcome == BattleOutcome.Won || outcome == BattleOutcome.Wiped, $"Seed {seed}: the fight ended as '{Battle.OutcomeName(outcome)}'.");
        }
    }

    private static BattleContent Content(bool exact)
    {
        string groups = TestBattles.GroupsFile.Replace("\"groups\": [\n", "\"groups\": [\n" + EvaluatorGroups, StringComparison.Ordinal);
        Assert.NotEqual(TestBattles.GroupsFile, groups);
        return TestBattles.ForEvaluator(Mender, groups, exact, ProtectorProfile, IdleProfile, CarefulProfile);
    }

    /// <summary>
    /// Steps each turn of a character until an enemy acts. The first turn attacks the first
    /// enemy that melee reaches, or defends, and each later turn defends.
    /// </summary>
    private static List<BattleEvent> PlayUntilEnemyActs(Simulation run, bool attackFirst)
    {
        List<BattleEvent> events = [];
        bool first = true;
        for (int turn = 0; turn < BattleRuns.TickLimit; turn += 1)
        {
            Intent intent = first && attackFirst ? BattleRuns.AttackFirst(run) : Intent.OfPlayer(IntentIds.BattleDefend);
            first = false;
            run.Step([intent]);
            foreach (BattleEvent played in run.TakeBattleEvents())
            {
                events.Add(played);
            }

            if (events.Exists(played => played.Actor.Side == BattleSide.Enemy && IsAct(played.Kind)))
            {
                return events;
            }
        }

        throw new InvalidOperationException($"No enemy acted in {BattleRuns.TickLimit} turns.");
    }

    private static BattleEvent FirstActOf(List<BattleEvent> events, BattleTarget actor)
    {
        foreach (BattleEvent played in events)
        {
            if (played.Actor == actor && IsAct(played.Kind))
            {
                return played;
            }
        }

        throw new InvalidOperationException($"{actor.Describe()} took no action in {events.Count} events.");
    }

    private static bool IsAct(BattleEventKind kind) =>
        kind is BattleEventKind.Hit or BattleEventKind.Miss or BattleEventKind.Absorb or BattleEventKind.Heal
            or BattleEventKind.Defend or BattleEventKind.Step;

    private static ScoredAction ScoreOf(Battle battle, BattleContent content, Simulation run, string action)
    {
        foreach (ScoredAction scored in BattleEvaluator.Score(battle, battle.Enemies[0], content, run.State.Context("test")))
        {
            if (string.CompareOrdinal(scored.Action.Describe(), action) == 0)
            {
                return scored;
            }
        }

        throw new InvalidOperationException($"The evaluator scored no action '{action}'.");
    }

    private static List<string> Describe(IReadOnlyList<EnemyAction> actions)
    {
        List<string> described = [];
        foreach (EnemyAction action in actions)
        {
            described.Add(action.Describe());
        }

        return described;
    }

    private static ContentId Id(string value) => ContentId.Parse(value, "test", "id");
}
