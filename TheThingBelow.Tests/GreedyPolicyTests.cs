using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Tools.Bots;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

public sealed class GreedyPolicyTests
{
    [Theory]
    [InlineData(11UL)]
    [InlineData(12UL)]
    [InlineData(13UL)]
    public void InABattleThePolicyAttacksTheEnemyWithTheLeastHealthAndNeverFlees(ulong seed)
    {
        // D-1183.
        Simulation run = BattleRuns.IntoBattle(seed, "group.fixture_pair");
        run.TakeBattleEvents();
        GreedyPolicy policy = new();
        Battle battle = BattleRuns.BattleOf(run);

        Intent chosen = policy.Choose(run.State, run.Accepted()) ?? throw new System.InvalidOperationException($"Seed {seed}: the policy chose no command.");

        Assert.Equal(IntentIds.BattleAttack.Value, chosen.Action.Value);
        int least = int.MaxValue;
        foreach (Combatant enemy in battle.Enemies)
        {
            if (BattleTurns.RefusalOf(run.State, new BattleChoice(BattleAction.Attack, enemy.Target, null)) is null && enemy.Health < least)
            {
                least = enemy.Health;
            }
        }

        Assert.Equal(least, battle.Enemies[chosen.Target!.Value.Slot].Health);
    }

    [Fact]
    public void ThePolicyAnswersAWaitIntentFirst()
    {
        // D-540: a bot answers each wait intent at once.
        Simulation run = BattleRuns.IntoBattle(11, "group.fixture_pair");
        IReadOnlyList<Intent> accepted = [Intent.OfPlayer(IntentIds.OpenMenu), Intent.OfPlayer(IntentIds.WaitBattleEnd)];

        Assert.Equal(IntentIds.WaitBattleEnd.Value, new GreedyPolicy().Choose(run.State, accepted)!.Action.Value);
    }

    [Fact]
    public void APathToAnotherTargetCrossesNoEntrance()
    {
        // D-1243: a step onto an entrance leaves the map. From the marker of the cut, the shortest
        // path to the inn would start north onto the mark of the cut.
        Simulation run = OnTheMarkerOfTheCut();
        WalkTarget inn = Assert.Single(WalkTargets.Of(run.State), target => target.Key == "entrance.fixture_overworld_inn");

        StepDirection? step = WalkTargets.FirstStep(run.State.Party, run.State.Story.Flags, inn, out int length);

        Assert.Equal(StepDirection.East, step);
        Assert.Equal(16, length);
    }

    [Fact]
    public void OnTheOverworldThePolicyTakesTheEntranceToAMapThatItNeverEntered()
    {
        // D-1243: the walk of a dungeon run leaves the cut behind and crosses to the inn.
        GreedyPolicy policy = new();
        Simulation dungeon = Start("map.fixture_dungeon");
        _ = policy.Choose(dungeon.State, dungeon.Accepted());
        Simulation run = OnTheMarkerOfTheCut();

        Intent chosen = policy.Choose(run.State, run.Accepted()) ?? throw new System.InvalidOperationException("The policy chose no step on the overworld.");

        Assert.Equal(IntentIds.MoveEast.Value, chosen.Action.Value);
    }

    private static readonly System.Lazy<ContentSet> Content =
        new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    private static Simulation Start(string map) =>
        Simulation.Start(1, MapSet.Of(Content.Value.Maps), ContentId.Parse(map, "test", "map"), Content.Value.Battle, Content.Value.Notices, Content.Value.Story, DebugIntentHandlers.None);

    /// <summary>Starts a run on the fixture overworld, on the marker below the mark of the cut (D-1255).</summary>
    private static Simulation OnTheMarkerOfTheCut()
    {
        Simulation run = Start("map.fixture_overworld");
        run.State.EnterMapAt(ContentId.Parse("map.fixture_overworld", "test", "map"), ContentId.Parse("marker.fixture_overworld_cut", "test", "marker"), run.State.Context("test"));
        return run;
    }
}
