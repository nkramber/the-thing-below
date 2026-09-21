using System;
using System.Collections.Generic;
using System.Reflection;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
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

    /// <summary>The width of a common sprite, in art pixels (D-236).</summary>
    private const int SpriteSize = 32;

    /// <summary>The count of icons that a status line holds beside the name and the health (D-830).</summary>
    private const int StatusIcons = 4;

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
    public void EachPanelHoldsItsLongestStringAtBothBodySizes()
    {
        // Exit test 4 of PR-10 (D-241, D-708). The message line holds a battle message of 40
        // characters. The command menu holds the five labels with a body between each. The
        // status holds a name of 16 characters, the health of the largest stat, and four icons,
        // with half a body between the parts.
        StringTable strings = Content.Value.Strings;
        int labels = 0;
        foreach (string id in new[] { "battle.command_attack", "battle.command_defend", "battle.command_step", "battle.command_item", "battle.command_flee" })
        {
            labels += strings.Text(ContentId.Parse(id, StringTable.Path, "id")).Length;
        }

        string most = BattleFixture.MostStat.ToString(System.Globalization.CultureInfo.InvariantCulture);
        int health = strings.Text(ContentId.Parse("battle.health", StringTable.Path, "id"))
            .Replace("{health}", most, StringComparison.Ordinal)
            .Replace("{full}", most, StringComparison.Ordinal)
            .Length;

        foreach (int body in new[] { Content.Value.Style.SmallBody, Content.Value.Style.LargeBody })
        {
            int advance = body / 2;
            Assert.True(advance * MessageLimit <= Inside(Box("Message")), $"At a body of {body}, the message line holds no message of {MessageLimit} characters.");
            Assert.True((advance * labels) + (body * 4) <= Inside(Box("Commands")), $"At a body of {body}, the command menu holds no five labels.");
            int status = (advance * (LabelLimit + health)) + (advance * 2) + (Const("IconSize") * StatusIcons);
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
