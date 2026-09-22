using TheThingBelow.Storage;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The default bindings of the game (D-84, D-862). The test reads the built Game assembly,
/// because Tests takes no reference to Game (D-614).
/// </summary>
public sealed class GameInputMapTests
{
    [Fact]
    public void TheDefaultBindingsHoldNoConflictAndMatchTheFixture()
    {
        // A first start writes these bindings, and a conflict would block the first close of
        // the settings screen (D-862).
        var bindings = (ControlBindings)GameAssemblyFile.Type("TheThingBelow.Game.Ui.GameInputMap")
            .GetMethod("DefaultBindings")!
            .Invoke(null, null)!;

        Assert.Empty(bindings.FindConflicts());
        Assert.Equal(SettingsFixtures.GameBindings(), bindings);
    }
}
