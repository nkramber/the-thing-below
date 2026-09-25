namespace TheThingBelow.Core.Saves;

/// <summary>Picks the save that a wipe reloads (D-231, D-776, D-1114).</summary>
public static class SavePick
{
    /// <summary>
    /// Gives the newer of the slot save and the autosave of one run: the one whose snapshot holds
    /// the later tick (D-231). A save of another seed is another run, and the pick drops it
    /// (D-1114). The tick is the one time line of a run, so the pick reads no clock and no file
    /// time (D-650, G-3).
    /// </summary>
    /// <param name="slot">The slot save, or no value when none exists.</param>
    /// <param name="autosave">The autosave, or no value when none exists.</param>
    /// <param name="seed">The seed of the run that wiped.</param>
    /// <returns>The newer save of the run, or no value when neither is of the run. Game then starts the run again from its start (D-776).</returns>
    /// <remarks>
    /// Two saves of one tick hold one state of the run, so the pick gives the slot save, which
    /// the player made.
    /// </remarks>
    public static SaveDocument? NewerOf(SaveDocument? slot, SaveDocument? autosave, ulong seed)
    {
        SaveDocument? kept = OfRun(slot, seed) ? slot : null;
        SaveDocument? keptAuto = OfRun(autosave, seed) ? autosave : null;
        if (kept is null)
        {
            return keptAuto;
        }

        if (keptAuto is null)
        {
            return kept;
        }

        return keptAuto.Snapshot.Tick > kept.Snapshot.Tick ? keptAuto : kept;
    }

    /// <summary>Tells whether a save belongs to the run of one seed (D-1114).</summary>
    /// <param name="save">The save, or no value.</param>
    /// <param name="seed">The seed of the run.</param>
    /// <returns>True when a save exists and its header holds the seed.</returns>
    public static bool OfRun(SaveDocument? save, ulong seed) => save is not null && save.Header.Seed == seed;
}
