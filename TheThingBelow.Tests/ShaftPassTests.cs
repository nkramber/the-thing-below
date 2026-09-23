using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The place of the shaft pass in the overlay (D-919).</summary>
public sealed class ShaftPassTests
{
    [Fact]
    public void TheShaftPassDrawsAboveTheFogAndTheHitBursts()
    {
        // D-919: a beam lights the fog, so the pass draws above the fog and the bursts, in the overlay.
        int shaft = (int)GameAssemblyFile.Type("TheThingBelow.Game.Ui.ShaftPass").GetField("ShaftZIndex")!.GetValue(null)!;
        int fog = (int)GameAssemblyFile.Type("TheThingBelow.Game.Ui.AmbientLayer").GetField("FogZIndex")!.GetValue(null)!;
        int burst = (int)GameAssemblyFile.Type("TheThingBelow.Game.Ui.HitBurst").GetField("BurstZIndex")!.GetValue(null)!;

        Assert.True(shaft > fog, $"the shafts take Z {shaft}, at or below the fog of Z {fog}");
        Assert.True(shaft > burst, $"the shafts take Z {shaft}, at or below the bursts of Z {burst}");
    }
}
