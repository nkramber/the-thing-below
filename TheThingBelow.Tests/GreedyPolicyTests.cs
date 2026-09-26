using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Runs;
using TheThingBelow.Tools.Bots;
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
}
