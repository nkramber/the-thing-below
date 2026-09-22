using System;
using System.Collections.Generic;
using TheThingBelow.Storage;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The bindings of the remap, and the conflict rule of D-862.</summary>
public sealed class ControlBindingsTests
{
    [Fact]
    public void TheFixtureBindingsHoldNoConflict()
    {
        Assert.Empty(SettingsFixtures.Bindings().FindConflicts());
    }

    [Fact]
    public void ARebindCanPutOneButtonOnTwoActions()
    {
        // D-862: the remap screen accepts one button on two actions and names each conflict.
        ControlBindings bindings = SettingsFixtures.Bindings()
            .Rebind("cancel", InputBinding.OfButton(SettingsFixtures.ButtonB), InputBinding.OfButton(SettingsFixtures.ButtonA));

        BindingConflict conflict = Assert.Single(bindings.FindConflicts());
        Assert.Equal(InputBinding.OfButton(SettingsFixtures.ButtonA), conflict.Binding);
        Assert.Equal(["cancel", "confirm"], conflict.Actions);
    }

    [Fact]
    public void ARebindLeavesTheOldBindingsAsTheyAre()
    {
        ControlBindings old = SettingsFixtures.Bindings();

        old.Rebind("cancel", InputBinding.OfKey(SettingsFixtures.Escape), InputBinding.OfKey(SettingsFixtures.W));

        Assert.Equal(InputBinding.OfKey(SettingsFixtures.Escape), old.Of("cancel")[0]);
    }

    [Fact]
    public void ASecondRebindClearsTheConflict()
    {
        ControlBindings bindings = SettingsFixtures.Bindings()
            .Rebind("cancel", InputBinding.OfButton(SettingsFixtures.ButtonB), InputBinding.OfButton(SettingsFixtures.ButtonA))
            .Rebind("confirm", InputBinding.OfButton(SettingsFixtures.ButtonA), InputBinding.OfButton(SettingsFixtures.ButtonB));

        Assert.Empty(bindings.FindConflicts());
    }

    [Fact]
    public void AMenuActionTakesNoRemap()
    {
        // D-862: the `ui_*` actions keep their default buttons, so the menus always answer.
        ArgumentException error = Assert.Throws<ArgumentException>(() => new ControlBindings(
            new SortedDictionary<string, IReadOnlyList<InputBinding>>
            {
                ["ui_accept"] = [InputBinding.OfKey(SettingsFixtures.Enter)],
            }));

        Assert.Contains("ui_accept", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-862", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnActionWithNoBindingFails()
    {
        ArgumentException error = Assert.Throws<ArgumentException>(() => new ControlBindings(
            new SortedDictionary<string, IReadOnlyList<InputBinding>> { ["confirm"] = [] }));

        Assert.Contains("confirm", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void OneActionCannotHoldOneBindingTwice()
    {
        ArgumentException error = Assert.Throws<ArgumentException>(() => new ControlBindings(
            new SortedDictionary<string, IReadOnlyList<InputBinding>>
            {
                ["confirm"] = [InputBinding.OfKey(SettingsFixtures.Enter), InputBinding.OfKey(SettingsFixtures.Enter)],
            }));

        Assert.Contains("places 0 and 1", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARebindToABindingOfTheSameActionKeepsOneCopy()
    {
        ControlBindings bindings = SettingsFixtures.Bindings()
            .Rebind("confirm", InputBinding.OfButton(SettingsFixtures.ButtonA), InputBinding.OfKey(SettingsFixtures.Enter));

        Assert.Equal([InputBinding.OfKey(SettingsFixtures.Enter)], bindings.Of("confirm"));
    }

    [Fact]
    public void ARebindWithNoOldBindingAddsTheNewOneAtTheEnd()
    {
        ControlBindings bindings = SettingsFixtures.Bindings()
            .Rebind("confirm", null, InputBinding.OfKey(SettingsFixtures.W + 1));

        Assert.Equal(InputBinding.OfKey(SettingsFixtures.W + 1), bindings.Of("confirm")[2]);
    }

    [Fact]
    public void ARebindOfABindingThatTheActionLacksFails()
    {
        ArgumentException error = Assert.Throws<ArgumentException>(() => SettingsFixtures.Bindings()
            .Rebind("confirm", InputBinding.OfKey(SettingsFixtures.W), InputBinding.OfKey(SettingsFixtures.Enter)));

        Assert.Contains("does not hold", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(BindingKind.Stick, 1, 0)]
    [InlineData(BindingKind.Stick, 1, 2)]
    [InlineData(BindingKind.Key, 87, 1)]
    [InlineData(BindingKind.Button, -1, 0)]
    [InlineData((BindingKind)7, 1, 0)]
    public void ABindingThatBreaksARuleOfItsKindFails(BindingKind kind, int code, int direction)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new InputBinding(kind, code, direction).Check("confirm"));
    }

    [Fact]
    public void AnUnknownActionNamesTheActionsThatExist()
    {
        KeyNotFoundException error = Assert.Throws<KeyNotFoundException>(() => SettingsFixtures.Bindings().Of("jump"));

        Assert.Contains("cancel, confirm, step_north", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoSetsWithTheSameListsAreEqual()
    {
        Assert.Equal(SettingsFixtures.Bindings(), SettingsFixtures.Bindings());
        Assert.NotEqual(
            SettingsFixtures.Bindings(),
            SettingsFixtures.Bindings().Rebind("confirm", InputBinding.OfKey(SettingsFixtures.Enter), InputBinding.OfKey(SettingsFixtures.W + 1)));
    }
}
