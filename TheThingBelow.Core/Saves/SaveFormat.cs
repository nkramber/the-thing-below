namespace TheThingBelow.Core.Saves;

/// <summary>The format version of a save file. PR-43 wrote the first one (D-166, D-654).</summary>
/// <remarks>
/// A save holds one snapshot, and a load reads that snapshot alone (D-259). Thus the shape of
/// the snapshot line is the format of a save, and a PR that changes the snapshot raises
/// <see cref="Current"/>, keeps a reader for each older version, and commits a fixture save of
/// each one (D-166, D-654). The fixture test of Tests fails a raise with no fixture.
/// </remarks>
public static class SaveFormat
{
    /// <summary>The oldest format version that this build reads.</summary>
    public const int Oldest = 1;

    /// <summary>The format version that this build writes.</summary>
    /// <remarks>
    /// PR-7 raised this number to 2. The snapshot dropped the patrol of the first world and
    /// gained the party on a tile map: the id of the map, the tile of the lead, the facing,
    /// the step that runs, and every walked tile (D-106, D-528, D-567).
    /// <para>
    /// PR-8 raised it to 3. The map object of the snapshot gained the stored values of each
    /// enemy, the mark of a sight, and the encounter (D-750).
    /// </para>
    /// <para>
    /// PR-9 raised it to 4. The snapshot gained the party: the health and the row of each
    /// character, and the pack. It also gained the battle that runs (D-531, D-765).
    /// </para>
    /// <para>
    /// PR-66 raised it to 5. Each character gained the statuses that last past a fight, and
    /// each combatant gained its statuses with their ends in place of its push rate (D-792,
    /// D-798).
    /// </para>
    /// <para>
    /// PR-11 raised it to 6. The streams gained the stream of the evaluator (D-947). A save of an
    /// older format gains that stream at its first value, from the seed of the header, because
    /// no build before PR-11 drew from it.
    /// </para>
    /// <para>
    /// PR-67 raised it to 7. Each character gained its level, its experience, and its MP (D-966).
    /// A save of an older format starts each character at its join level with full MP (D-363).
    /// </para>
    /// <para>
    /// PR-62 raised it to 8. The snapshot gained the notice log (D-985). A save of an older format
    /// starts the log empty.
    /// </para>
    /// <para>
    /// PR-68 raised it to 9. The snapshot gained the story state: the flags, the story scene that
    /// runs, and the events that fire a trigger (D-540, D-542). A save of an older format starts with
    /// no flag on and no story scene.
    /// </para>
    /// <para>
    /// PR-12 raised it to 10. Each character gained its lesson slots and the points of each lesson
    /// that it carried, and the party gained the lesson pack and the swap place (D-361, D-1018,
    /// D-1024, D-1030). A save of an older format gives each character its start lessons.
    /// </para>
    /// <para>
    /// PR-13 raised it to 11. Each character gained its six gear slots, and the party gained the
    /// spare gear in the pack and the gold (D-44, D-1038, D-1043). A save of an older format gives
    /// each character its start gear of the fixture, and the party no gold.
    /// </para>
    /// <para>
    /// PR-99 raised it to 12. The party dropped the swap place, because a swap of lessons needs
    /// no place (D-1050). A save of format 10 or 11 holds the field, and the read drops it.
    /// </para>
    /// <para>
    /// PR-91 raised it to 13. The party gained the state of the torch (D-1064). A save of an older
    /// format puts the torch away, as the first get of the torch does.
    /// </para>
    /// <para>
    /// PR-105 raised it to 14. The story scene that runs gained the id of its step, so a resume
    /// finds a step that an edit of the story scene moved (D-1112). A save of an older format
    /// reads the index of the step alone.
    /// </para>
    /// <para>
    /// PR-14 raised it to 15. The map gained the stored values of each NPC, and the streams gained
    /// the NPC stream (D-1137). A save of an older format puts each NPC of its map on its start
    /// tile, and it opens the NPC stream at its first value from the seed of the header, because no
    /// build before PR-14 drew from it.
    /// </para>
    /// </remarks>
    public const int Current = 15;
}
