using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;

namespace TheThingBelow.Game.Ui;

/// <summary>The place of one combatant on the field, in art pixels of the world viewport (D-634).</summary>
/// <param name="X">The column of the middle of the sprite.</param>
/// <param name="Feet">The row under the feet of the sprite. The sprite draws up from it.</param>
public readonly record struct FieldPlace(int X, int Feet);

/// <summary>A box on the frame, in frame pixels (D-568).</summary>
/// <param name="X">The left column.</param>
/// <param name="Y">The top row.</param>
/// <param name="Width">The width.</param>
/// <param name="Height">The height.</param>
public readonly record struct FrameBox(int X, int Y, int Width, int Height);

/// <summary>
/// Where the battle screen puts each part: the side view on the field, the timeline strip at
/// the top, and the message line, the command menu, and the status at the bottom (D-111,
/// D-377, D-756).
/// </summary>
/// <remarks>
/// The enemies stand on the left and face right, and the party stands on the right and faces
/// left. Each side has a front row near the middle and a back row behind it. A row holds up
/// to six combatants in two lanes, and the outer lane stands out and a little lower, so two
/// neighbors never cover each other (D-759).
/// <para>
/// A place in a row comes from the combatants of that row that stand on the field, in slot
/// order. A character who went down keeps the place, because the character stays on the
/// field. An enemy that went down leaves its row, and the others of the row close up, so a
/// wave never puts more than six in one row. A waiting enemy takes a place when it steps in
/// (D-758, D-759).
/// </para>
/// <para>
/// This type holds no Godot value, so a test reads it from the built Game assembly with no
/// engine (D-614).
/// </para>
/// </remarks>
public static class BattleLayout
{
    /// <summary>The column of the middle of the front row of the enemies.</summary>
    public const int EnemyFrontX = 232;

    /// <summary>The column of the middle of the back row of the enemies.</summary>
    public const int EnemyBackX = 150;

    /// <summary>The column of the middle of the front row of the party.</summary>
    public const int PartyFrontX = 408;

    /// <summary>The column of the middle of the back row of the party.</summary>
    public const int PartyBackX = 490;

    /// <summary>The row under the feet of the middle rank of a row.</summary>
    public const int MiddleFeet = 218;

    /// <summary>The rows between the feet of two ranks of one lane.</summary>
    public const int RankSpacing = 40;

    /// <summary>The columns that the outer lane of a row stands out from the inner lane.</summary>
    public const int LaneStep = 36;

    /// <summary>The rows that the outer lane of a row stands below the inner lane.</summary>
    public const int LaneDrop = 15;

    /// <summary>The width of the health bar of an enemy, in art pixels, with its border (D-826).</summary>
    public const int BarWidth = 26;

    /// <summary>The height of the health bar of an enemy, in art pixels, with its border.</summary>
    public const int BarHeight = 4;

    /// <summary>The rows between the feet of an enemy and the top of its bar.</summary>
    public const int BarGap = 3;

    /// <summary>The size of one icon of a status, in frame pixels (D-214).</summary>
    public const int IconSize = 16;

    /// <summary>The rows between the top of a sprite and the bottom of the pointer (D-833).</summary>
    public const int PointerGap = 2;

    /// <summary>The size of one face of the strip, in frame pixels (D-756).</summary>
    public const int FaceSize = 32;

    /// <summary>The columns between two faces of the strip.</summary>
    public const int FaceGap = 8;

    /// <summary>The frame pixels between the text of a panel and the edge of its frame (D-220).</summary>
    public const int PanelEdge = UiTheme.FrameEdge;

    /// <summary>The frame pixels between two panels.</summary>
    public const int PanelGap = 16;

    /// <summary>The width of the message line and of the command menu, in frame pixels.</summary>
    public const int LeftWidth = 656;

    /// <summary>The height of a panel of one line, at the largest body size (D-707).</summary>
    public const int LineHeight = 32 + (PanelEdge * 2);

    /// <summary>The most characters in a party that the status holds (D-31).</summary>
    public const int StatusLines = BattleFixture.MostCharacters;

    /// <summary>The count of turns that the strip shows (D-756).</summary>
    public const int StripTurns = Battle.StripTurns;

    /// <summary>The timeline strip across the top of the frame (D-111, D-756).</summary>
    public static FrameBox Strip { get; } = StripBox();

    /// <summary>The command menu, at the bottom left of the frame (D-111).</summary>
    public static FrameBox Commands { get; } = new(
        UiMetrics.EdgePixels,
        ScreenFit.FrameHeight - UiMetrics.EdgePixels - LineHeight,
        LeftWidth,
        LineHeight);

    /// <summary>The message line, above the command menu (D-213).</summary>
    public static FrameBox Message { get; } = new(
        UiMetrics.EdgePixels,
        Commands.Y - PanelGap - LineHeight,
        LeftWidth,
        LineHeight);

    /// <summary>The status of the party, at the bottom right of the frame (D-111).</summary>
    public static FrameBox Status { get; } = StatusBox();

    /// <summary>Gives the place of one combatant on the field.</summary>
    /// <param name="view">The view of the fight.</param>
    /// <param name="shown">The combatant, which stands on the field, or a character who went down.</param>
    /// <returns>The place.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The combatant waits off the field, or it is an enemy that went down (T-2).</exception>
    public static FieldPlace PlaceOf(BattleView view, ShownCombatant shown)
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(shown);

        if (!HoldsPlace(shown))
        {
            throw new ArgumentException(
                $"The combatant {shown.Target.Describe()} is {Battle.PlaceName(shown.Place)}, and it has no place on the field (D-758, T-2).",
                nameof(shown));
        }

        IReadOnlyList<ShownCombatant> side = shown.Target.Side == BattleSide.Party ? view.Party : view.Enemies;
        int index = 0;
        int count = 0;
        foreach (ShownCombatant other in side)
        {
            if (other.Row != shown.Row || !HoldsPlace(other))
            {
                continue;
            }

            if (other.Target.Slot < shown.Target.Slot)
            {
                index += 1;
            }

            count += 1;
        }

        return PlaceIn(shown.Target.Side, shown.Row, index, count);
    }

    /// <summary>Gives the place of the combatant at one index of a row.</summary>
    /// <param name="side">The side.</param>
    /// <param name="row">The row.</param>
    /// <param name="index">The index in the row, from zero.</param>
    /// <param name="count">The count of places of the row.</param>
    /// <returns>The place.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the row, or the row holds more than six (T-2).</exception>
    public static FieldPlace PlaceIn(BattleSide side, BattleRow row, int index, int count)
    {
        if (count < 1 || count > BattleFixture.MostOnField || index < 0 || index >= count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(index),
                index,
                $"A row holds 1 to {BattleFixture.MostOnField} places, and the screen asked for place {index} of {count} (D-759, T-2).");
        }

        // A row stands in two lanes: the even places in the inner lane and the odd places in
        // the outer one, a little lower. A sprite of 32 rows and its bar then never cover a
        // neighbor, and six fit on the ground of the backdrop. The ranks stand evenly around
        // the middle, so the offset counts in half spacings: -2, 0, 2 for three ranks.
        int ranks = (count + 1) / 2;
        int rank = index / 2;
        int lane = index % 2;
        int feet = MiddleFeet + (((2 * rank) - (ranks - 1)) * RankSpacing / 2) + (lane * LaneDrop);
        int outward = lane * LaneStep;
        int x = (side, row) switch
        {
            (BattleSide.Enemy, BattleRow.Front) => EnemyFrontX - outward,
            (BattleSide.Enemy, _) => EnemyBackX - outward,
            (_, BattleRow.Front) => PartyFrontX + outward,
            _ => PartyBackX + outward,
        };

        return new FieldPlace(x, feet);
    }

    /// <summary>Gives the step of an enemy lunge: toward the party, which stands on the right (D-832).</summary>
    /// <param name="side">The side of the combatant that acts.</param>
    /// <returns>The columns of the lunge, or zero for a character, which takes its pose instead.</returns>
    public static int LungeOf(BattleSide side) => side == BattleSide.Enemy ? BattleTimes.LungePixels : 0;

    /// <summary>
    /// Gives the width of the fill of a health bar, in art pixels (D-826). A combatant with
    /// any health left shows one pixel at least, so a live enemy never reads as empty.
    /// </summary>
    /// <param name="health">The health that the screen shows.</param>
    /// <param name="fullHealth">The full health.</param>
    /// <returns>The width, from zero to the inside of the bar.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The full health is not above zero, or the health is outside it (T-2).</exception>
    public static int BarFill(int health, int fullHealth)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(fullHealth, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(health);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(health, fullHealth);

        int inside = BarWidth - 2;
        if (health == 0)
        {
            return 0;
        }

        return Math.Max(1, checked(inside * health) / fullHealth);
    }

    /// <summary>Tells whether a combatant holds a place in its row: it stands on the field, or it is a character who went down.</summary>
    /// <param name="shown">The combatant.</param>
    /// <returns>True when the combatant holds a place.</returns>
    public static bool HoldsPlace(ShownCombatant shown)
    {
        ArgumentNullException.ThrowIfNull(shown);

        return shown.Place == CombatantPlace.Field
            || (shown.Place == CombatantPlace.Down && shown.Target.Side == BattleSide.Party);
    }

    private static FrameBox StripBox()
    {
        int inside = (StripTurns * FaceSize) + ((StripTurns - 1) * FaceGap);
        int width = inside + (PanelEdge * 2);
        int height = FaceSize + (PanelEdge * 2);
        return new FrameBox((ScreenFit.FrameWidth - width) / 2, UiMetrics.EdgePixels, width, height);
    }

    private static FrameBox StatusBox()
    {
        int x = UiMetrics.EdgePixels + LeftWidth + PanelGap;
        int height = (StatusLines * 32) + (PanelEdge * 2);
        return new FrameBox(
            x,
            ScreenFit.FrameHeight - UiMetrics.EdgePixels - height,
            ScreenFit.FrameWidth - UiMetrics.EdgePixels - x,
            height);
    }
}
