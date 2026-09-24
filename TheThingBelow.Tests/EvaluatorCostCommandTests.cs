using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Runs;
using TheThingBelow.Tools;
using TheThingBelow.Tools.Evaluator;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The `evaluator-cost` command plays the worst fight of D-961 and times each enemy turn
/// (F-53, G-14). A test build is a Debug build, so these tests read the report and never the
/// time against the limit.
/// </summary>
public sealed class EvaluatorCostCommandTests
{
    [Fact]
    public void TheCommandReportsTheActionsAndTheTimesOfTheTurns()
    {
        // Exit test 6 of PR-11: the count of legal actions and the time of a turn.
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = Program.Run([EvaluatorCostCommand.Name, "--root", RepositoryRoot.Find(), "--turns", "40"], output, errors);

        string report = output.ToString();
        Assert.Contains("40 enemy turns of six enemies against three characters", report, StringComparison.Ordinal);
        Assert.Contains("legal actions of one turn: 15 at most", report, StringComparison.Ordinal);
        Assert.Contains("95th percentile", report, StringComparison.Ordinal);
        Assert.True(
            exitCode == 0 || errors.ToString().Contains("passes the limit", StringComparison.Ordinal),
            $"The command gave the exit code {exitCode} and the errors '{errors}'.");
    }

    [Fact]
    public void TheTimedTurnAppliesTheActionOnACopyOfTheRun()
    {
        // P2-1 of the PR-67 review, D-961: the timer reads the whole enemy turn, the choice and
        // its effect with its events, and the fight of the command never reads a timed turn.
        BattleContent content = CostFight.Content(RepositoryRoot.Find());
        Simulation run = Simulation.Start(1, CostFight.Map(), content, CostFight.Notices(), CostFight.Story(content), DebugIntentHandlers.None);
        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        Battle battle = run.State.Battle ?? throw new InvalidOperationException("The step into the guard started no battle.");
        long readyAt = battle.Enemies[0].ReadyAt;
        _ = run.TakeBattleEvents();

        Simulation copy = EvaluatorCostCommand.CopyOf(run, 1, content);
        IReadOnlyList<BattleEvent> events = EvaluatorCostCommand.PlayEnemyTurn(copy, battle.Enemies[0].Target);

        Assert.Contains(events, played => played.Actor == battle.Enemies[0].Target
            && played.Kind is BattleEventKind.Hit or BattleEventKind.Miss or BattleEventKind.Heal or BattleEventKind.Defend or BattleEventKind.Step);
        Assert.True((copy.State.Battle?.Enemies[0].ReadyAt ?? 0) > readyAt, "The action of the timed turn pushed the enemy on the timeline of the copy.");
        Assert.Equal(readyAt, battle.Enemies[0].ReadyAt);
        Assert.Empty(run.TakeBattleEvents());
    }

    [Fact]
    public void TheWorstFightHoldsSixEnemiesOnTheFieldAndThreeCharacters()
    {
        // D-961: six enemies against three characters, each enemy with a move of each kind.
        BattleContent content = CostFight.Content(RepositoryRoot.Find());
        GroupRecord worst = content.Group(Core.Content.ContentId.Parse(CostFight.Group, "test", "group"));

        Assert.Equal(6, worst.Entries.Count);
        Assert.All(worst.Entries, entry => Assert.False(entry.Waits));
        Assert.Equal(3, content.Fixture.StartParty.Count);
        Assert.Equal(3, content.Enemies[0].Abilities.Count);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-3")]
    [InlineData("many")]
    public void ACountOfTurnsThatIsNoCountGivesTheFault(string turns)
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = Program.Run([EvaluatorCostCommand.Name, "--turns", turns], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"it read '{turns}'", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ARootWithNoRulesFileGivesTheFaultAndNotACrash()
    {
        using StringWriter output = new();
        using StringWriter errors = new();
        string empty = Directory.CreateTempSubdirectory("evaluator-cost").FullName;
        try
        {
            int exitCode = Program.Run([EvaluatorCostCommand.Name, "--root", empty], output, errors);

            Assert.Equal(Program.FaultExitCode, exitCode);
            Assert.Contains("reads the rules file of the checkout", errors.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(empty, true);
        }
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(20, 18)]
    [InlineData(100, 94)]
    [InlineData(101, 95)]
    public void ThePercentileTakesTheNearestRank(int count, int index)
    {
        Assert.Equal(index, EvaluatorCostCommand.PercentileIndex(count, 95));
    }
}
