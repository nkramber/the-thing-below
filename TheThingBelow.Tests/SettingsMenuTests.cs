using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TheThingBelow.Storage;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The rows, the cursor, the values, and the remap of the settings screen (D-226, D-862,
/// D-872). The tests read the built Game assembly, because Tests takes no reference to Game
/// (D-614).
/// </summary>
public sealed class SettingsMenuTests
{
    private const string MenuTypeName = "TheThingBelow.Game.Ui.SettingsMenu";
    private const string SlotTypeName = "TheThingBelow.Game.Ui.BindingSlot";

    [Fact]
    public void TheRowsHoldEverySettingAndOneRemapRowForEachActionOfTheGame()
    {
        // D-226, D-435, D-861, D-866, D-874, and D-863 to D-865. D-870 removed the shape icons.
        List<string> items = [];
        List<string> actions = [];
        foreach (object row in Rows())
        {
            string item = Read<object>(row, "Item").ToString()!;
            items.Add(item);
            if (item == "Remap")
            {
                actions.Add(Read<string>(row, "Action"));
            }
        }

        Assert.Equal(
            [
                "Window", "Fit", "Body", "Master", "Music", "Effects", "Ambience", "MuteInBackground", "Mono",
                "Messages", "RememberCursor", "EffectLevel", "TextSpeed", "DeadZone", "Vibration",
            ],
            items.GetRange(0, 15));
        Assert.Equal(["step_north", "step_south", "step_east", "step_west", "confirm", "cancel", "menu"], actions);
    }

    [Fact]
    public void TheCursorWrapsAtEachEnd()
    {
        Menu menu = Menu.Open();

        menu.Move(-1);

        Assert.Equal(Rows().Count - 1, menu.Cursor);
        menu.Move(1);
        Assert.Equal(0, menu.Cursor);
    }

    [Fact]
    public void AChangeStopsAtEachEndAndAChoiceWraps()
    {
        // The master volume stands at 8 of 10 (D-867).
        Menu menu = Menu.Open();
        menu.Point(RowOfItem("Master"), keyboard: true);

        menu.Change(1);
        menu.Change(1);
        menu.Change(1);
        Assert.Equal(10, menu.Settings.Audio.Master);

        menu.Choose();
        Assert.Equal(0, menu.Settings.Audio.Master);
    }

    [Fact]
    public void TheDeadZoneStepsByFiveHundredthsFromTwentyToEighty()
    {
        // D-861.
        Menu menu = Menu.Open();
        menu.Point(RowOfItem("DeadZone"), keyboard: true);

        menu.Change(-1);
        Assert.Equal(45, menu.Settings.Controls.DeadZone);

        for (int step = 0; step < 20; step += 1)
        {
            menu.Change(-1);
        }

        Assert.Equal(20, menu.Settings.Controls.DeadZone);
    }

    [Fact]
    public void EachValueRowStepsItsSetting()
    {
        Menu menu = Menu.Open();
        foreach (string item in new[] { "Window", "Fit", "Body", "MuteInBackground", "Mono", "Vibration", "Messages", "RememberCursor", "EffectLevel", "TextSpeed" })
        {
            menu.Point(RowOfItem(item), keyboard: true);
            menu.Choose();
        }

        GameSettings changed = menu.Settings;
        Assert.Equal(WindowSetting.Window, changed.Display.Window);
        Assert.Equal(FitSetting.WholePixels, changed.Display.Fit);
        Assert.Equal(BodySetting.Small, changed.Display.Body);
        Assert.False(changed.Audio.MuteInBackground);
        Assert.True(changed.Audio.Mono);
        Assert.False(changed.Controls.Vibration);
        Assert.Equal(MessageSpeed.Fast, changed.Battle.Messages);
        Assert.True(changed.Battle.RememberCursor);
        Assert.Equal(EffectLevel.Reduced, changed.Access.Effects);
        Assert.Equal(TextSpeed.Fast, changed.Access.Text);
    }

    [Fact]
    public void ARemapReplacesTheBindingOfItsSlot()
    {
        // Exit test 2 starts here: the menu changes the bindings, and the store writes them.
        Menu menu = Menu.Open();
        menu.Point(RowOfAction("confirm"), keyboard: true);

        menu.Choose();
        Assert.True(menu.Capturing);
        Assert.True(menu.Capture(InputBinding.OfKey(SettingsFixtures.W + 1)));

        Assert.False(menu.Capturing);
        Assert.Equal(InputBinding.OfKey(SettingsFixtures.W + 1), menu.Settings.Controls.Bindings.Of("confirm")[0]);
        Assert.True(menu.CanClose);
    }

    [Fact]
    public void AnInputOfTheOtherDeviceStopsTheWaitWithNoChange()
    {
        Menu menu = Menu.Open();
        menu.Point(RowOfAction("confirm"), keyboard: false);
        GameSettings before = menu.Settings;

        menu.Choose();
        menu.Capture(InputBinding.OfKey(SettingsFixtures.W + 1));

        Assert.False(menu.Capturing);
        Assert.Equal(before, menu.Settings);
    }

    [Fact]
    public void AConflictBlocksTheCloseUntilThePlayerMovesOneBinding()
    {
        // Exit test 3 (D-862): the back action takes the button of confirm.
        Menu menu = Menu.Open();
        menu.Point(RowOfAction("cancel"), keyboard: false);
        menu.Choose();
        menu.Capture(InputBinding.OfButton(SettingsFixtures.ButtonA));

        Assert.False(menu.CanClose);

        menu.Choose();
        menu.Capture(InputBinding.OfButton(SettingsFixtures.ButtonB));
        Assert.True(menu.CanClose);
    }

    [Fact]
    public void AMoveWhileTheRowWaitsLeavesTheCursor()
    {
        Menu menu = Menu.Open();
        menu.Point(RowOfAction("menu"), keyboard: true);
        menu.Choose();

        menu.Move(1);
        menu.Change(1);

        Assert.Equal(RowOfAction("menu"), menu.Cursor);
        Assert.True(menu.Capturing);
    }

    [Fact]
    public void ALeftAndARightOnARemapRowMoveBetweenTheSlots()
    {
        Menu menu = Menu.Open();
        menu.Point(RowOfAction("confirm"), keyboard: true);

        menu.Change(1);
        Assert.Equal("Gamepad", menu.Slot);
        menu.Change(-1);
        Assert.Equal("Keyboard", menu.Slot);
    }

    private static IList Rows() => (IList)GameAssemblyFile.Type(MenuTypeName).GetProperty("Rows")!.GetValue(null)!;

    private static int RowOfItem(string item)
    {
        IList rows = Rows();
        for (int index = 0; index < rows.Count; index += 1)
        {
            if (Read<object>(rows[index]!, "Item").ToString() == item)
            {
                return index;
            }
        }

        throw new InvalidOperationException($"The settings menu holds no row '{item}' (T-2).");
    }

    private static int RowOfAction(string action) =>
        (int)GameAssemblyFile.Type(MenuTypeName).GetMethod("RowOf")!.Invoke(null, [action])!;

    private static T Read<T>(object value, string name) => (T)value.GetType().GetProperty(name)!.GetValue(value)!;

    /// <summary>The settings menu, read from the Game assembly with no engine (D-614).</summary>
    private sealed class Menu
    {
        private readonly object value;

        private Menu(object value) => this.value = value;

        public int Cursor => Read<int>(this.value, "Cursor");

        public bool Capturing => Read<bool>(this.value, "Capturing");

        public bool CanClose => Read<bool>(this.value, "CanClose");

        public string Slot => Read<object>(this.value, "Slot").ToString()!;

        public GameSettings Settings => Read<GameSettings>(this.value, "Settings");

        public static Menu Open() =>
            new(Activator.CreateInstance(GameAssemblyFile.Type(MenuTypeName), [SettingsFixtures.DefaultsOfTheGame()])!);

        public void Move(int step) => this.Call("Move", step);

        public void Change(int step) => this.Call("Change", step);

        public void Choose() => this.Call("Choose");

        public bool Capture(InputBinding binding) => (bool)this.Call("Capture", binding)!;

        public void Point(int row, bool keyboard)
        {
            object slot = Enum.Parse(GameAssemblyFile.Type(SlotTypeName), keyboard ? "Keyboard" : "Gamepad");
            this.Call("Point", row, slot);
        }

        private object? Call(string name, params object[] arguments)
        {
            try
            {
                return this.value.GetType().GetMethod(name)!.Invoke(this.value, arguments);
            }
            catch (TargetInvocationException thrown) when (thrown.InnerException is not null)
            {
                throw thrown.InnerException;
            }
        }
    }
}
