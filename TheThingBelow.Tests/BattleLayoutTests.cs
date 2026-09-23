using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// Where the battle screen puts each part, and the fit of each panel to its longest string
/// (D-111, D-241, D-377, D-708, D-759). The tests read the built Game assembly, because Tests
/// takes no reference to Game (D-614).
/// </summary>
public sealed class BattleLayoutTests
{
    private const string LayoutTypeName = "TheThingBelow.Game.Ui.BattleLayout";

    private const string ViewTypeName = "TheThingBelow.Game.Ui.BattleView";

    /// <summary>The width of a common sprite, in art pixels (D-236).</summary>
    private const int SpriteSize = 32;

    /// <summary>The count of icons that a status line holds beside the name and the health (D-830).</summary>
    private const int StatusIcons = 4;

    /// <summary>The longest name of a character on the bottom line of a battle (D-981).</summary>
    private const int StatusNameLimit = 8;

    /// <summary>The longest name of a menu label, from the `game-text-style` skill (D-241).</summary>
    private const int LabelLimit = 16;

    /// <summary>The limit of a battle message, from the `game-text-style` skill (D-241).</summary>
    private const int MessageLimit = 40;

    private static readonly Lazy<ContentSet> Content =
        new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    [Fact]
    public void NoSpriteOrBarOfARowCoversANeighborOfItsSide()
    {
        // D-759: a row holds up to six, in any split of the rows. Each box is a common sprite
        // and its bar, and no two boxes of one side meet.
        foreach (BattleSide side in new[] { BattleSide.Enemy, BattleSide.Party })
        {
            for (int front = 0; front <= 6; front += 1)
            {
                for (int back = 0; back + front <= 6; back += 1)
                {
                    List<(int X, int Feet)> places = [.. PlacesOf(side, BattleRow.Front, front), .. PlacesOf(side, BattleRow.Back, back)];
                    for (int one = 0; one < places.Count; one += 1)
                    {
                        for (int other = one + 1; other < places.Count; other += 1)
                        {
                            Assert.False(
                                Meet(places[one], places[other]),
                                $"{side}: with {front} in front and {back} behind, the places ({places[one].X}, {places[one].Feet}) and ({places[other].X}, {places[other].Feet}) meet.");
                        }
                    }
                }
            }
        }
    }

    [Fact]
    public void EveryPlaceStandsAboveTheMessageLineAndBelowTheStrip()
    {
        // The icons under the lowest bar end above the message line, and the top of a boss of
        // 96 rows stays below the strip (D-236, D-756).
        FrameBox message = Box("Message");
        FrameBox strip = Box("Strip");
        int iconBottom = Const("BarGap") + Const("BarHeight") + 1;
        foreach (BattleSide side in new[] { BattleSide.Enemy, BattleSide.Party })
        {
            foreach (BattleRow row in new[] { BattleRow.Front, BattleRow.Back })
            {
                for (int count = 1; count <= 6; count += 1)
                {
                    foreach ((int x, int feet) in PlacesOf(side, row, count))
                    {
                        int bottom = ((feet + iconBottom) * 2) + Const("IconSize");
                        Assert.True(bottom <= message.Y, $"{side} {row} of {count}: the icons end at frame row {bottom}, below the message line at {message.Y}.");
                        Assert.True((feet - 96) * 2 >= strip.Y + strip.Height, $"{side} {row} of {count}: a boss at feet {feet} reaches the strip.");
                        Assert.InRange(x, SpriteSize, 640 - SpriteSize);
                    }
                }
            }
        }
    }

    [Fact]
    public void ThePanelsStayInsideTheFrameAndNeverMeet()
    {
        FrameBox[] boxes = [Box("Strip"), Box("Message"), Box("Commands"), Box("Status")];
        foreach (FrameBox box in boxes)
        {
            Assert.InRange(box.X, 16, 1280 - 16 - box.Width);
            Assert.InRange(box.Y, 16, 720 - 16 - box.Height);
        }

        for (int one = 0; one < boxes.Length; one += 1)
        {
            for (int other = one + 1; other < boxes.Length; other += 1)
            {
                bool apart = boxes[one].X + boxes[one].Width <= boxes[other].X
                    || boxes[other].X + boxes[other].Width <= boxes[one].X
                    || boxes[one].Y + boxes[one].Height <= boxes[other].Y
                    || boxes[other].Y + boxes[other].Height <= boxes[one].Y;
                Assert.True(apart, $"The panels {boxes[one]} and {boxes[other]} meet.");
            }
        }
    }

    [Fact]
    public void EveryCharacterNameFitsTheBottomLine()
    {
        // D-981: the bottom line of a battle holds a name of 8 characters beside the numbers.
        ContentSet content = Content.Value;
        foreach (CharacterRecord character in content.Battle.Fixture.Characters)
        {
            string name = content.Strings.Text(ContentId.Parse($"name.{character.Id.Name}", StringTable.Path, character.Id.Value));
            Assert.True(
                name.Length <= StatusNameLimit,
                $"The name '{name}' of '{character.Id.Value}' holds {name.Length} characters, and the bottom line holds {StatusNameLimit} (D-981).");
        }
    }

    [Fact]
    public void EachPanelHoldsItsLongestStringAtBothBodySizes()
    {
        // Exit test 4 of PR-10 (D-241, D-708). The message line holds a battle message of 40
        // characters. The command menu holds the five labels with a body between each. The status
        // holds a name of 8 characters, the health and the MP of the largest pool, and four icons,
        // with half a body between the parts (D-980, D-981).
        StringTable strings = Content.Value.Strings;
        int labels = 0;
        foreach (string id in new[] { "battle.command_attack", "battle.command_defend", "battle.command_step_forward", "battle.command_item", "battle.command_flee" })
        {
            labels += strings.Text(ContentId.Parse(id, StringTable.Path, "id")).Length;
        }

        string most = StatCurve.MostPool.ToString(System.Globalization.CultureInfo.InvariantCulture);
        int health = strings.Text(ContentId.Parse("battle.health", StringTable.Path, "id"))
            .Replace("{health}", most, StringComparison.Ordinal)
            .Replace("{full}", most, StringComparison.Ordinal)
            .Length;
        int mp = strings.Text(ContentId.Parse("battle.mp", StringTable.Path, "id"))
            .Replace("{mp}", most, StringComparison.Ordinal)
            .Replace("{full}", most, StringComparison.Ordinal)
            .Length;

        foreach (int body in new[] { Content.Value.Style.SmallBody, Content.Value.Style.LargeBody })
        {
            int advance = body / 2;
            Assert.True(advance * MessageLimit <= Inside(Box("Message")), $"At a body of {body}, the message line holds no message of {MessageLimit} characters.");
            Assert.True((advance * labels) + (body * 4) <= Inside(Box("Commands")), $"At a body of {body}, the command menu holds no five labels.");
            int status = (advance * (StatusNameLimit + health + mp)) + (advance * 3) + (Const("IconSize") * StatusIcons);
            Assert.True(status <= Inside(Box("Status")), $"At a body of {body}, the status needs {status} pixels, and it holds {Inside(Box("Status"))}.");
            Assert.True(body + (Const("PanelEdge") * 2) <= Box("Message").Height, $"At a body of {body}, a line panel holds no line.");
        }
    }

    [Theory]
    [InlineData(0, 100, 0)]
    [InlineData(100, 100, 24)]
    [InlineData(1, 100_000, 1)]
    [InlineData(50, 100, 12)]
    public void TheBarFillsInProportionAndShowsAnyHealthLeft(int health, int full, int fill)
    {
        // D-826: a live enemy never reads as empty.
        Assert.Equal(fill, (int)Method("BarFill").Invoke(null, [health, full])!);
    }

    [Fact]
    public void ABarOfMoreHealthThanItsFullIsAnError()
    {
        TargetInvocationException thrown = Assert.Throws<TargetInvocationException>(() => Method("BarFill").Invoke(null, [101, 100]));

        Assert.IsType<ArgumentOutOfRangeException>(thrown.InnerException);
    }

    [Fact]
    public void ASeventhPlaceInARowIsAnError()
    {
        // D-759: the field holds six. A seventh place is a fault of the screen, never a place
        // off the ground (T-2).
        TargetInvocationException thrown = Assert.Throws<TargetInvocationException>(
            () => Method("PlaceIn").Invoke(null, [BattleSide.Enemy, BattleRow.Front, 6, 7]));

        Assert.IsType<ArgumentOutOfRangeException>(thrown.InnerException);
    }

    [Fact]
    public void TheRoomOfTheWaitingColumnIsTheLimitOfTheLoad()
    {
        // D-963: Core refuses a column taller than this room, and the room ends at the top of
        // the message line.
        int top = Const("WaitingTop");
        int bottom = (int)GameAssemblyFile.Type(LayoutTypeName).GetProperty("WaitingBottom")!.GetValue(null)!;

        Assert.Equal(BattleFixture.MostWaitingHeight, bottom - top);
        Assert.Equal(Box("Message").Y, bottom * 2);
        Assert.Equal(Const("WaitingX") - (Const("LargestBody") / 2), top);
    }

    [Fact]
    public void NoWaitingBodyMeetsABossInTheBackRow()
    {
        // D-953: the column stands behind the back row. The widest waiting body and the widest
        // body of the outer lane of the back row never meet.
        int columnRight = Const("WaitingX") + (Const("LargestBody") / 2);
        foreach ((int x, int _) in PlacesOf(BattleSide.Enemy, BattleRow.Back, 6))
        {
            Assert.True(x - (Const("LargestBody") / 2) >= columnRight, $"A boss of the back row at column {x} meets the waiting column, which ends at {columnRight}.");
        }
    }

    [Fact]
    public void AColumnStandsOnTheBottomOfItsRoomAndAFullColumnFillsIt()
    {
        // D-953: a short column stands on the ground, and the next enemy stands at its top.
        int top = Const("WaitingTop");
        List<(int X, int Feet)> one = WaitingPlaces([32]);
        List<(int X, int Feet)> full = WaitingPlaces([96, 96, 96]);

        Assert.Equal([(Const("WaitingX"), top + BattleFixture.MostWaitingHeight)], one);
        Assert.Equal([(Const("WaitingX"), top + 96), (Const("WaitingX"), top + 192), (Const("WaitingX"), top + 288)], full);
    }

    [Fact]
    public void AColumnTallerThanItsRoomIsAnError()
    {
        // D-963: the load refuses such a group, so the screen never guesses a place (T-2).
        TargetInvocationException thrown = Assert.Throws<TargetInvocationException>(
            () => Method("WaitingPlaces").Invoke(null, [new List<int> { 64, 64, 64, 64, 64 }]));

        Assert.IsType<ArgumentOutOfRangeException>(thrown.InnerException);
        Assert.Contains("D-963", thrown.InnerException.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AfterAFallTheTopOfTheColumnStepsInAndTheNextEnemyTakesTheTop()
    {
        // Exit tests 2 and 3 of PR-98: the column holds the waiting enemies in the order of the
        // group, and the top one steps in after a fall. The others keep their places on the
        // bottom of the room, and the next one takes the top (D-760, D-778, D-953). The wave holds
        // grunts alone, because the patrol of the test map takes the size of a common enemy.
        BattleContent content = TestBattles.OfGroups(TestBattles.WaveGroupsFile(["enemy.fixture_grunt", "enemy.fixture_grunt", "enemy.fixture_grunt"]));
        Simulation run = BattleRuns.IntoBattle(3, "group.wave", content);
        object view = GameAssemblyFile.Type(ViewTypeName).GetMethod("AtStart")!.Invoke(null, [run.State])!;
        Assert.Equal([1, 2, 3], WaitingSlots(view));

        for (int turn = 0; turn < BattleRuns.TickLimit; turn += 1)
        {
            foreach (BattleEvent played in run.TakeBattleEvents())
            {
                if (played.Kind != BattleEventKind.StepIn)
                {
                    Apply(view, played);
                    continue;
                }

                List<int> before = WaitingSlots(view);
                List<(int X, int Feet)> placesBefore = ColumnOf(view);
                Apply(view, played);
                List<(int X, int Feet)> placesAfter = ColumnOf(view);

                Assert.Equal(before[0], played.Actor.Slot);
                Assert.Equal(before[1..], WaitingSlots(view));
                Assert.Equal(placesBefore[1..], placesAfter);

                return;
            }

            Assert.Equal(BattleOutcome.Running, BattleRuns.BattleOf(run).Outcome);
            run.Step([BattleRuns.AttackFirst(run)]);
        }

        Assert.Fail($"No enemy stepped in within {BattleRuns.TickLimit} turns.");
    }

    /// <summary>Gives the slots of the waiting column of a view, from the top.</summary>
    private static List<int> WaitingSlots(object view)
    {
        var slots = new List<int>();
        foreach (object shown in (IEnumerable)Method("WaitingOf").Invoke(null, [view])!)
        {
            slots.Add(((BattleTarget)shown.GetType().GetProperty("Target")!.GetValue(shown)!).Slot);
        }

        return slots;
    }

    /// <summary>Gives the places of the waiting column of a view, with the height of a grunt and of a brute (D-828).</summary>
    private static List<(int X, int Feet)> ColumnOf(object view)
    {
        var heights = new List<int>();
        foreach (object shown in (IEnumerable)Method("WaitingOf").Invoke(null, [view])!)
        {
            string id = ((ContentId)shown.GetType().GetProperty("Id")!.GetValue(shown)!).Value;
            heights.Add(string.CompareOrdinal(id, "enemy.fixture_brute") == 0 ? 64 : SpriteSize);
        }

        return WaitingPlaces(heights);
    }

    private static List<(int X, int Feet)> WaitingPlaces(List<int> heights)
    {
        var places = new List<(int X, int Feet)>();
        foreach (object place in (IEnumerable)Method("WaitingPlaces").Invoke(null, [heights])!)
        {
            places.Add(((int)place.GetType().GetProperty("X")!.GetValue(place)!, (int)place.GetType().GetProperty("Feet")!.GetValue(place)!));
        }

        return places;
    }

    private static void Apply(object view, BattleEvent played) =>
        view.GetType().GetMethod("Apply")!.Invoke(view, [played]);

    private static List<(int X, int Feet)> PlacesOf(BattleSide side, BattleRow row, int count)
    {
        var places = new List<(int X, int Feet)>();
        for (int index = 0; index < count; index += 1)
        {
            object place = Method("PlaceIn").Invoke(null, [side, row, index, count])!;
            places.Add(((int)place.GetType().GetProperty("X")!.GetValue(place)!, (int)place.GetType().GetProperty("Feet")!.GetValue(place)!));
        }

        return places;
    }

    /// <summary>Tells whether the boxes of two common sprites and their bars meet.</summary>
    private static bool Meet((int X, int Feet) one, (int X, int Feet) other)
    {
        int below = Const("BarGap") + Const("BarHeight");
        int oneTop = one.Feet - SpriteSize;
        int otherTop = other.Feet - SpriteSize;
        bool apartX = Math.Abs(one.X - other.X) >= SpriteSize;
        bool apartY = one.Feet + below <= otherTop || other.Feet + below <= oneTop;
        return !apartX && !apartY;
    }

    private static int Inside(FrameBox box) => box.Width - (Const("PanelEdge") * 2);

    private static FrameBox Box(string name)
    {
        object box = GameAssemblyFile.Type(LayoutTypeName).GetProperty(name)!.GetValue(null)!;
        Type type = box.GetType();
        return new FrameBox(
            (int)type.GetProperty("X")!.GetValue(box)!,
            (int)type.GetProperty("Y")!.GetValue(box)!,
            (int)type.GetProperty("Width")!.GetValue(box)!,
            (int)type.GetProperty("Height")!.GetValue(box)!);
    }

    private static int Const(string name) =>
        (int)(GameAssemblyFile.Type(LayoutTypeName).GetField(name)?.GetValue(null)
            ?? throw new InvalidOperationException($"The layout holds no constant '{name}' (T-2)."));

    private static MethodInfo Method(string name) =>
        GameAssemblyFile.Type(LayoutTypeName).GetMethod(name, BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException($"The layout holds no '{name}' method (T-2).");

    private readonly record struct FrameBox(int X, int Y, int Width, int Height);
}
