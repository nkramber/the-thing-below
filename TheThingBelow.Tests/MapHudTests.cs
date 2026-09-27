using System;
using System.Reflection;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The rules of the map HUD of Game: when the block shows, and the fill of a health bar (D-1237).
/// The test reads the built Game assembly, as each test of Game does.
/// </summary>
public sealed class MapHudTests
{
    private const string HudTypeName = "TheThingBelow.Game.Ui.MapHud";

    private const ulong Seed = 0x64;

    [Fact]
    public void TheHudShowsForAHurtADownOrAStatusAndHidesForAFullParty()
    {
        // D-1237: a party at full health with no status hides the block.
        Assert.False(Wanted(TestParty.StartEach(Seed, (slot, stored) => stored, TestBattles.WithParty(2)).State));
        Assert.True(Wanted(TestParty.StartEach(Seed, (slot, stored) => slot == 1 ? stored with { Health = stored.Health - 1 } : stored, TestBattles.WithParty(2)).State));
        Assert.True(Wanted(TestParty.StartEach(Seed, (slot, stored) => slot == 1 ? stored with { Health = 0 } : stored, TestBattles.WithParty(2)).State));
        Assert.True(Wanted(TestParty.StartEach(Seed, (slot, stored) => slot == 0 ? stored with { Statuses = [StatusKind.Blind] } : stored, TestBattles.WithParty(2)).State));
    }

    [Fact]
    public void AHurtInTheReserveAloneLeavesTheHudHidden()
    {
        // D-1237: the block shows the characters who fight, so the reserve never shows it.
        Simulation run = TestParty.StartFour(Seed, TestMaps.Room, TestParty.FourContent, (place, stored) => place == 3 ? stored with { Health = 1 } : stored);

        Assert.False(Wanted(run.State));
    }

    [Theory]
    [InlineData(0, 100, 0)]
    [InlineData(1, 100, 1)]
    [InlineData(50, 100, 31)]
    [InlineData(100, 100, 62)]
    public void TheFillOfABarKeepsOnePixelForALiveCharacter(int health, int full, int fill)
    {
        Assert.Equal(fill, FillOf(health, full));
    }

    [Fact]
    public void AFillOutsideTheFullHealthIsAnError()
    {
        TargetInvocationException error = Assert.Throws<TargetInvocationException>(() => FillOf(101, 100));
        Assert.IsType<ArgumentOutOfRangeException>(error.InnerException);
    }

    private static bool Wanted(RunState state) =>
        (bool)(GameAssemblyFile.Type(HudTypeName).GetMethod("Wanted")!.Invoke(null, [state.Characters])
            ?? throw new InvalidOperationException("The 'Wanted' method gave nothing (T-2)."));

    private static int FillOf(int health, int full) =>
        (int)(GameAssemblyFile.Type(HudTypeName).GetMethod("FillOf")!.Invoke(null, [health, full])
            ?? throw new InvalidOperationException("The 'FillOf' method gave nothing (T-2)."));
}
