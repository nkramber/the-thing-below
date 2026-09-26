using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Game.Ui;

/// <summary>The list that the cursor of the party window stands in (D-558, D-1134).</summary>
public enum PartyStage
{
    /// <summary>The characters of the party, in slot order.</summary>
    Browse,

    /// <summary>The actions on the chosen character: the row and the swap. Only a party with a reserve shows it.</summary>
    Action,

    /// <summary>The characters of the reserve, for the one who comes into the party.</summary>
    Incoming,
}

/// <summary>One action on a character of the party (D-558, D-1134).</summary>
public enum PartyAction
{
    /// <summary>The move to the other row (D-558).</summary>
    Row,

    /// <summary>The swap with a character of the reserve (D-1134).</summary>
    Swap,
}

/// <summary>
/// The cursor of the party window: the characters of the party, the actions on one of them, and
/// the reserve character who comes in on a swap (D-377, D-558, D-1134, D-1135). The keyboard, the
/// gamepad, and the mouse drive the one cursor (D-872).
/// </summary>
/// <remarks>
/// With no reserve, a confirm on a character makes the row intent at once, as before the reserve
/// existed, and the window shows no swap. With a reserve, a confirm opens the actions: the row,
/// and the swap. The swap shows dim when each reserve character is down, because a downed
/// character never comes in (D-1135, D-1136). A downed reserve line shows dim, and a confirm on it
/// does nothing, as a dim entry of the item window does (D-1049).
/// <para>
/// A move of the cursor makes no intent, and a whole choice makes one, so the record holds the
/// choice alone (D-493). The window reads each answer from the refusal of the party state, and no
/// rule lives here (D-100). This type holds no Godot value, so a test reads it with no engine
/// (D-614).
/// </para>
/// </remarks>
public sealed class PartyList
{
    private static readonly PartyAction[] AllActions = [PartyAction.Row, PartyAction.Swap];

    private readonly RunState state;

    /// <summary>Opens the cursor on the first character of the party.</summary>
    /// <param name="state">The run, which the cursor reads and never changes.</param>
    /// <exception cref="ArgumentNullException">The state is null (T-2).</exception>
    public PartyList(RunState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        this.state = state;
    }

    /// <summary>The actions on a character of a party with a reserve, in the order of the window.</summary>
    public static IReadOnlyList<PartyAction> Actions => AllActions;

    /// <summary>The list that the cursor stands in.</summary>
    public PartyStage Stage { get; private set; } = PartyStage.Browse;

    /// <summary>The place of the cursor in the list of the stage.</summary>
    public int Cursor { get; private set; }

    /// <summary>The party slot of the character that the actions and the swap read, which the browse stage chose.</summary>
    public int Chosen { get; private set; }

    /// <summary>The characters of the party, in slot order.</summary>
    public IReadOnlyList<PartyMember> Members => this.state.Characters.Members;

    /// <summary>The characters of the reserve, in reserve order. The list is empty for a party with no reserve (D-1136).</summary>
    public IReadOnlyList<PartyMember> Reserve => this.state.Characters.Reserve;

    /// <summary>True when the party has a reserve, so the window shows the reserve and the actions (D-1144).</summary>
    public bool HasReserve => this.Reserve.Count > 0;

    /// <summary>The count of entries of the list that the cursor stands in.</summary>
    public int Count => this.Stage switch
    {
        PartyStage.Browse => this.Members.Count,
        PartyStage.Action => AllActions.Length,
        _ => this.Reserve.Count,
    };

    /// <summary>Moves the cursor by one entry, and wraps at each end.</summary>
    /// <param name="step">-1 for up, and 1 for down.</param>
    /// <exception cref="ArgumentOutOfRangeException">The step is not -1 or 1 (T-2).</exception>
    public void Move(int step)
    {
        if (step != -1 && step != 1)
        {
            throw new ArgumentOutOfRangeException(nameof(step), step, "The cursor moves one entry up or down (T-2).");
        }

        this.Cursor = (this.Cursor + step + this.Count) % this.Count;
    }

    /// <summary>Puts the cursor on the entry under the mouse pointer (D-872).</summary>
    /// <param name="entry">The entry of the list of the stage.</param>
    /// <exception cref="ArgumentOutOfRangeException">The list holds no such entry (T-2).</exception>
    public void Point(int entry)
    {
        if (entry < 0 || entry >= this.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(entry), entry, $"The list of the stage {this.Stage} holds {this.Count} entries (T-2).");
        }

        this.Cursor = entry;
    }

    /// <summary>Tells whether the rules take a swap of the chosen character with at least one character of the reserve (D-1135).</summary>
    /// <returns>False when the party has no reserve or each reserve character is down, so the swap shows dim.</returns>
    public bool AllowsSwap()
    {
        for (int reserve = 0; reserve < this.Reserve.Count; reserve += 1)
        {
            if (this.AllowsIncoming(reserve))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Tells whether the rules take one reserve character into the slot of the chosen character (D-1135).</summary>
    /// <param name="reserve">The index of the character in the reserve.</param>
    /// <returns>False for a downed character, who never comes in, and the line shows dim.</returns>
    public bool AllowsIncoming(int reserve) => this.state.Characters.RefusalOfReserveSwap(this.Chosen, reserve) is null;

    /// <summary>Confirms the entry under the cursor.</summary>
    /// <returns>
    /// The row intent or the swap intent of a whole choice, or no value when the window moves to
    /// its next list or refuses the entry.
    /// </returns>
    public Intent? Confirm()
    {
        switch (this.Stage)
        {
            case PartyStage.Browse:
                // A party with no reserve moves the character to the other row at once (D-558).
                if (!this.HasReserve)
                {
                    return RowIntentOf(this.Cursor);
                }

                this.Chosen = this.Cursor;
                this.Stage = PartyStage.Action;
                this.Cursor = 0;
                return null;
            case PartyStage.Action:
                if (AllActions[this.Cursor] == PartyAction.Row)
                {
                    this.BackToBrowse();
                    return RowIntentOf(this.Chosen);
                }

                if (this.AllowsSwap())
                {
                    this.Stage = PartyStage.Incoming;
                    this.Cursor = 0;
                }

                return null;
            default:
                if (!this.AllowsIncoming(this.Cursor))
                {
                    return null;
                }

                Intent made = Intent.OfPartySwap(this.Chosen, this.Cursor);
                this.BackToBrowse();
                return made;
        }
    }

    /// <summary>Goes back one list.</summary>
    /// <returns>True when the cursor stood in the party list, so the window closes.</returns>
    public bool Cancel()
    {
        switch (this.Stage)
        {
            case PartyStage.Browse:
                return true;
            case PartyStage.Action:
                this.BackToBrowse();
                return false;
            default:
                this.Stage = PartyStage.Action;
                this.Cursor = Array.IndexOf(AllActions, PartyAction.Swap);
                return false;
        }
    }

    /// <summary>Gives the intent that moves the character of one slot to the other row (D-558).</summary>
    private static Intent RowIntentOf(int slot) => Intent.OfPlayer(IntentIds.PartyRow, new BattleTarget(BattleSide.Party, slot), null);

    /// <summary>Puts the cursor back on the chosen character in the party list.</summary>
    private void BackToBrowse()
    {
        this.Stage = PartyStage.Browse;
        this.Cursor = this.Chosen;
    }
}
