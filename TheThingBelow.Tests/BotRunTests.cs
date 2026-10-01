using System;
using System.IO;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Tools.Bots;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

public sealed class BotRunTests : IDisposable
{
    private static readonly Lazy<ContentSet> Content =
        new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    private readonly string folder = Path.Combine(Path.GetTempPath(), "bot-run-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void AGreedyRunOnTheHubReachesTheGoalFlag()
    {
        // D-1181, D-1183, D-1185: seed 1 starts on the hub, and the greedy bot walks into the
        // rats scene and wins its battle.
        BotResult result = this.Play(1, new GreedyPolicy());

        Assert.Equal(BotEnd.Complete, result.End);
        Assert.Equal(result.Played, result.GoalPlayed);
        Assert.True(result.Played < Content.Value.Bots.GreedyBudget, $"The run took {result.Played} ticks of the budget of {Content.Value.Bots.GreedyBudget}.");
        Assert.Contains(result.Battles, battle => battle.Outcome == Core.Battles.BattleOutcome.Won);
    }

    [Fact]
    public void AGreedyRunInTheDungeonCrossesTheOverworldToTheGoalFlag()
    {
        // Exit test 9 of PR-35 (D-1185, D-1243): seed 7 starts in the dungeon, the first of the
        // seven start maps, and the only way to the rats of the hub is the exit, the overworld,
        // and the entrance of the inn.
        BotResult result = this.Play(7, new GreedyPolicy());

        Assert.Equal(BotEnd.Complete, result.End);
        Assert.Equal(result.Played, result.GoalPlayed);
        Assert.True(result.Played < Content.Value.Bots.GreedyBudget, $"The run took {result.Played} ticks of the budget of {Content.Value.Bots.GreedyBudget}.");
    }

    [Fact]
    public void ARandomRunEndsAsBudgetOnItsOwnBudget()
    {
        // D-1259: each policy has its own budget, and a random run seldom reaches the goal.
        BotResult result = this.Play(2, new RandomPolicy(2));

        Assert.Equal(BotEnd.Budget, result.End);
        Assert.Equal(Content.Value.Bots.RandomBudget, result.Played);
    }

    [Fact]
    public void TheRecordOfEachRunReplaysToItsStateHash()
    {
        // T-7, G-4: the record holds each intent, and the numbers of a policy never reach a rule,
        // so a replay with no policy gives the state hash of the run. The seed loop covers both
        // policies and each of the seven start maps.
        for (ulong seed = 1; seed <= 8; seed += 1)
        {
            foreach (IBotPolicy policy in new IBotPolicy[] { new RandomPolicy(seed), new GreedyPolicy() })
            {
                BotResult result = this.Play(seed, policy);
                ContentSet content = Content.Value;
                RunRecord record = RunRecordText.Read(RunRecordText.Write(result.Record));

                RunState replayed = RunReplay.Play(record, content.Hash, MapSet.Of(content.Maps), content.Battle, content.Notices, content.Story, DebugIntentHandlers.None);

                Assert.True(
                    replayed.StateHash() == result.StateHash,
                    $"Seed {seed}, policy {BotPolicyKinds.NameOf(policy.Kind)}: the replay gave another state hash at tick {replayed.Tick}.");
            }
        }
    }

    [Theory]
    [InlineData(2UL, "map.village")]
    [InlineData(3UL, "map.village_pasture")]
    [InlineData(4UL, "map.mining_town")]
    [InlineData(5UL, "map.cells_upper")]
    [InlineData(6UL, "map.cells_lower")]
    public void EachPlaceOfTheFirstPlayablePlaysWithNoCrashAndNoSoftlock(ulong seed, string map)
    {
        // Exit test 5 of PR-17 (D-64, D-1185): the bots start on each new map, walk it, fight,
        // and play its story scenes. No way leads from region one to the rats of the fixture hub,
        // so each run ends as budget, and no run crashes or locks.
        Assert.Equal(map, Content.Value.Bots.StartOf(seed).Value);
        foreach (IBotPolicy policy in new IBotPolicy[] { new GreedyPolicy(), new RandomPolicy(seed) })
        {
            BotResult result = this.Play(seed, policy);

            Assert.False(BotEnds.Fails(result.End), $"Seed {seed}, policy {BotPolicyKinds.NameOf(policy.Kind)}: the run ended as {BotEnds.NameOf(result.End)}. {result.Message}");
            Assert.Equal(BotEnd.Budget, result.End);
        }
    }

    [Fact]
    public void AGreedyRunOfTheFirstPlayableWinsFightsOnEachFloorOfTheCells()
    {
        // Exit test 5 of PR-17: the greedy bot meets the patrols of each floor of the hanging
        // cells and wins a fight on each, so a run reads the enemies of region one.
        foreach (ulong seed in new ulong[] { 5, 6 })
        {
            BotResult result = this.Play(seed, new GreedyPolicy());

            Assert.Contains(result.Battles, battle => battle.Outcome == Core.Battles.BattleOutcome.Won);
        }
    }

    [Fact]
    public void TwoPlaysOfOneSeedGiveTheSameRun()
    {
        BotResult first = this.Play(5, new RandomPolicy(5));
        BotResult second = this.Play(5, new RandomPolicy(5));

        Assert.Equal(first.End, second.End);
        Assert.Equal(first.StateHash, second.StateHash);
        Assert.Equal(RunRecordText.Write(first.Record), RunRecordText.Write(second.Record));
    }

    [Fact]
    public void AWipeReloadsAndTheRunGoesOn()
    {
        // D-1181: the random bot wipes in some fights. The runner reloads the run, as Game does,
        // and the run ends with no crash.
        int wipes = 0;
        for (ulong seed = 1; seed <= 40 && wipes == 0; seed += 1)
        {
            BotResult result = this.Play(seed, new RandomPolicy(seed));
            Assert.False(BotEnds.Fails(result.End), $"Seed {seed}: the run ended as {BotEnds.NameOf(result.End)}. {result.Message}");
            wipes += result.Wipes;
        }

        Assert.True(wipes > 0, "No random run of seeds 1 to 40 wiped, so the test read no reload.");
    }

    [Fact]
    public void ARunWithNoAcceptedIntentEndsAsSoftlock()
    {
        // D-1179: a planted source with no intent leaves the player nothing that changes the state.
        BotResult result = BotRun.Play(Content.Value, 3, new GreedyPolicy(), _ => [], this.folder);

        Assert.Equal(BotEnd.Softlock, result.End);
        Assert.Contains("no intent of the 0", result.Message, StringComparison.Ordinal);
    }

    public void Dispose()
    {
        if (Directory.Exists(this.folder))
        {
            Directory.Delete(this.folder, recursive: true);
        }
    }

    private BotResult Play(ulong seed, IBotPolicy policy) =>
        BotRun.Play(Content.Value, seed, policy, AcceptedSources.OfCore, Path.Combine(this.folder, $"{seed}-{BotPolicyKinds.NameOf(policy.Kind)}"));
}
