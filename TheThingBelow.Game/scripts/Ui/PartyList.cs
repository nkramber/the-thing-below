using System;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The cursor of the party window, over the characters in slot order, and the intent of a
/// choice (D-377, D-558). The keyboard, the gamepad, and the mouse drive the one cursor (D-872).
/// </summary>
/// <remarks>
/// A move of the cursor makes no intent. A choice makes the row intent of the character under
/// the cursor, so the record holds the choice alone (D-493). This type holds no Godot value, so
/// a test reads it with no engine (D-614).
/// </remarks>
public sealed class PartyList
{
    /// <summary>Opens the cursor on the first character.</summary>
    /// <param name="count">The count of characters of the party, from 1 to 3 (D-31).</param>
    /// <exception cref="ArgumentOutOfRangeException">The count is outside the size of a party (T-2).</exception>
    public PartyList(int count)
    {
        if (count < 1 || count > BattleFixture.MostCharacters)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, $"A party holds 1 to {BattleFixture.MostCharacters} characters (D-31, T-2).");
        }

        this.Count = count;
    }

    /// <summary>The count of characters.</summary>
    public int Count { get; }

    /// <summary>The slot of the character under the cursor.</summary>
    public int Cursor { get; private set; }

    /// <summary>Moves the cursor by one character, and wraps at each end.</summary>
    /// <param name="step">-1 for up, and 1 for down.</param>
    /// <exception cref="ArgumentOutOfRangeException">The step is not -1 or 1 (T-2).</exception>
    public void Move(int step)
    {
        if (step != -1 && step != 1)
        {
            throw new ArgumentOutOfRangeException(nameof(step), step, "The cursor moves one character up or down (T-2).");
        }

        this.Cursor = (this.Cursor + step + this.Count) % this.Count;
    }

    /// <summary>Puts the cursor on the character under the mouse pointer (D-872).</summary>
    /// <param name="slot">The slot of the character.</param>
    /// <exception cref="ArgumentOutOfRangeException">The slot holds no character (T-2).</exception>
    public void Point(int slot)
    {
        if (slot < 0 || slot >= this.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(slot), slot, $"The party holds {this.Count} characters (T-2).");
        }

        this.Cursor = slot;
    }

    /// <summary>Gives the intent that moves the character under the cursor to the other row (D-558).</summary>
    /// <returns>The row intent, with the character as a target of the party side.</returns>
    public Intent Choose() => Intent.OfPlayer(IntentIds.PartyRow, new BattleTarget(BattleSide.Party, this.Cursor), null);
}
