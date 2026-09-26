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

    [Fact]
    public void EachMenuTakesWasdAndBackspace()
    {
        // D-1172, from the playtest of PR-65: WASD moves the cursor of every menu as the arrow
        // keys do, and Backspace goes back as Escape does. Godot gives the `ui_*` actions neither.
        var keys = (System.Collections.IEnumerable)GameAssemblyFile.Type("TheThingBelow.Game.Ui.GameInputMap")
            .GetField("MenuKeys")!
            .GetValue(null)!;

        var found = new System.Collections.Generic.List<string>();
        foreach (object entry in keys)
        {
            var pair = (System.Runtime.CompilerServices.ITuple)entry;
            found.Add($"{pair[0]} {pair[1]}");
        }

        Assert.Equal(["ui_up W", "ui_left A", "ui_down S", "ui_right D", "ui_cancel Backspace"], found);
    }
}
