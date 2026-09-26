using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Runs;
using TheThingBelow.Tools.Bots;
using Xunit;

namespace TheThingBelow.Tests;

public sealed class BattleTallyTests
{
    private static readonly BattleTarget Hero = new(BattleSide.Party, 0);

    private static readonly BattleTarget Rat = new(BattleSide.Enemy, 0);

    private static readonly BattleTarget OtherRat = new(BattleSide.Enemy, 1);

    [Fact]
    public void EachCommandAndEachEnemyActionCountsOneTurn()
    {
        // D-1182: a turn is one action of a character or an enemy. A strike of one enemy on two
        // targets gives two events and one turn.
        BattleTally tally = new();
        tally.Read(null, [new BattleEvent(BattleEventKind.Started, Hero, null, 0), new BattleEvent(BattleEventKind.Turn, Hero, null, 0)]);
        tally.Read(Intent.OfPlayer(IntentIds.BattleAttack, Rat, null), [
            new BattleEvent(BattleEventKind.Hit, Hero, Rat, 3),
            new BattleEvent(BattleEventKind.Hit, Rat, Hero, 1),
            new BattleEvent(BattleEventKind.Miss, Rat, Hero, 0),
            new BattleEvent(BattleEventKind.Defend, OtherRat, null, 0),
            new BattleEvent(BattleEventKind.Turn, Hero, null, 0),
        ]);
        tally.Read(Intent.OfPlayer(IntentIds.BattleDefend), [new BattleEvent(BattleEventKind.Defend, Hero, null, 0), new BattleEvent(BattleEventKind.Won, Hero, null, 0)]);

        BattleCount battle = Assert.Single(tally.Battles);
        Assert.Equal(4, battle.Turns);
        Assert.Equal(BattleOutcome.Won, battle.Outcome);
    }

    [Fact]
    public void AnIntentOutsideABattleCountsNoTurn()
    {
        BattleTally tally = new();
        tally.Read(Intent.OfPlayer(IntentIds.BattleDefend), []);
        tally.Read(null, [new BattleEvent(BattleEventKind.Started, Hero, null, 0), new BattleEvent(BattleEventKind.Fled, Hero, null, 0)]);

        Assert.Equal(0, Assert.Single(tally.Battles).Turns);
    }

    [Fact]
    public void AnOutcomeWithNoStartIsAnError()
    {
        BattleTally tally = new();

        Assert.Throws<System.InvalidOperationException>(() => tally.Read(null, [new BattleEvent(BattleEventKind.Wiped, Hero, null, 0)]));
    }
}
