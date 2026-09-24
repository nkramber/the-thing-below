using System;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The row intent of the party window: a character moves to the other row while a menu is
/// open, and the next fight starts it there (D-377, D-558).
/// </summary>
public sealed class PartyRowTests
{
    private const ulong Seed = 20260923;

    private static readonly Intent FirstRow = Intent.OfPlayer(IntentIds.PartyRow, new BattleTarget(BattleSide.Party, 0), null);

    [Fact]
    public void TheRowIntentMovesTheCharacterToTheOtherRowAndBack()
    {
        Simulation run = Start(TestMaps.Room);
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        Assert.Equal(BattleRow.Front, run.State.Characters.Members[0].Row);

        run.Step([FirstRow]);
        Assert.Equal(BattleRow.Back, run.State.Characters.Members[0].Row);

        run.Step([FirstRow]);
        Assert.Equal(BattleRow.Front, run.State.Characters.Members[0].Row);
    }

    [Fact]
    public void AFightStartsWithTheRowThatThePartyWindowSet()
    {
        // Exit test 7 of PR-62 (D-558): the step east meets the guard, and the fight reads the row.
        Simulation run = Start(BattleRuns.Map("group.test_pair"));
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        run.Step([FirstRow]);
        run.Step([Intent.OfPlayer(IntentIds.CloseMenu)]);

        Battle battle = BattleRuns.BattleOf(Walk(run));

        Assert.Equal(BattleRow.Back, battle.Party[0].Row);
    }

    [Fact]
    public void TheRowSurvivesASnapshotAndAResume()
    {
        // Exit test 8 of PR-62 (D-558): the snapshot keeps the row.
        Simulation run = Start(TestMaps.Room);
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        run.Step([FirstRow]);

        Simulation resumed = Simulation.Resume(Seed, run.Snapshot(), TestMaps.Room, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

        Assert.Equal(BattleRow.Back, resumed.State.Characters.Members[0].Row);
        Assert.Equal(run.StateHash(), resumed.StateHash());
    }

    [Fact]
    public void ARowIntentWithNoMenuOpenIsAnErrorWithTheTick()
    {
        // T-2: the party window lives in a menu, so a row intent on the walk points at a fault.
        Simulation run = Start(TestMaps.Room);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([FirstRow]));

        Assert.Contains("no menu is open", error.Message, StringComparison.Ordinal);
        Assert.Contains("tick", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARowIntentInsideAFightIsAnError()
    {
        // D-380: the row step of a fight is the control there, so the party window never reaches a fight.
        Simulation run = Walk(Start(BattleRuns.Map("group.test_pair")));
        Assert.NotNull(run.State.Battle);
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([FirstRow]));

        Assert.Contains("D-380", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(BattleSide.Enemy, 0)]
    [InlineData(BattleSide.Party, 1)]
    [InlineData(BattleSide.Party, -1)]
    public void ARowIntentOfNoCharacterOfThePartyIsAnError(BattleSide side, int slot)
    {
        Simulation run = Start(TestMaps.Room);
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);

        SimulationException error = Assert.Throws<SimulationException>(
            () => run.Step([Intent.OfPlayer(IntentIds.PartyRow, new BattleTarget(side, slot), null)]));

        Assert.Contains("D-558", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARowIntentWithNoTargetIsAnError()
    {
        Simulation run = Start(TestMaps.Room);
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);

        SimulationException error = Assert.Throws<SimulationException>(
            () => run.Step([Intent.OfPlayer(IntentIds.PartyRow)]));

        Assert.Contains("names no character", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARowIntentThatCarriesAnItemIsAnError()
    {
        Simulation run = Start(TestMaps.Room);
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);

        SimulationException error = Assert.Throws<SimulationException>(
            () => run.Step([Intent.OfPlayer(IntentIds.PartyRow, new BattleTarget(BattleSide.Party, 0), ContentId.Parse("item.fixture_draught", "PartyRowTests", "item"))]));

        Assert.Contains("item", error.Message, StringComparison.Ordinal);
    }

    private static Simulation Start(Core.Maps.GameMap map) =>
        Simulation.Start(Seed, map, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

    /// <summary>Steps east until the guard starts a fight, for 120 ticks at most.</summary>
    private static Simulation Walk(Simulation run)
    {
        for (int step = 0; step < 120 && run.State.Battle is null; step += 1)
        {
            run.Step(run.State.Party.Patrols.Encounter is null ? [Intent.OfPlayer(IntentIds.MoveEast)] : []);
        }

        return run;
    }
}
