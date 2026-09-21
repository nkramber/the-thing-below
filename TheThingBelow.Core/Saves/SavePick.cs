namespace TheThingBelow.Core.Saves;

/// <summary>Picks the save that a wipe reloads (D-231, D-776).</summary>
public static class SavePick
{
    /// <summary>
    /// Gives the newer of the slot save and the autosave: the one whose snapshot holds the
    /// later tick of the run (D-231). The tick is the one time line of a run, so the pick reads
    /// no clock and no file time (D-650, G-3).
    /// </summary>
    /// <param name="slot">The slot save, or no value when none exists.</param>
    /// <param name="autosave">The autosave, or no value when none exists.</param>
    /// <returns>The newer save, or no value when neither exists. Game then starts the run again from its start (D-776).</returns>
    /// <remarks>
    /// Two saves of one tick hold one state of the run, so the pick gives the slot save, which
    /// the player made.
    /// </remarks>
    public static SaveDocument? NewerOf(SaveDocument? slot, SaveDocument? autosave)
    {
        if (slot is null)
        {
            return autosave;
        }

        if (autosave is null)
        {
            return slot;
        }

        return autosave.Snapshot.Tick > slot.Snapshot.Tick ? autosave : slot;
    }
}
