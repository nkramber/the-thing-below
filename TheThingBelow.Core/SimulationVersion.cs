namespace TheThingBelow.Core;

/// <summary>
/// The version of the simulation rules. Every change of Core behavior raises this number,
/// and the review of that PR confirms the bump (G-17, D-259).
/// </summary>
public static class SimulationVersion
{
    /// <summary>
    /// The current version. PR-4 set the first value, because it held the first Core rules:
    /// the fixed-point math, the streams, and the state hash. PR-5 raised it to 2, because
    /// the content reader, the content ids, and the content hash are Core rules too. PR-6
    /// raised it to 3, because the tick, the intents, and the world of one tick are the
    /// first rules that change a state (G-17). The audit fixes of 2026-09-20 raised it to
    /// 4: the content reader refuses a repeated field, the recorder refuses a tick gap, and
    /// a snapshot refuses an increment that no stream of this build gives. PR-7 raised it
    /// to 5: the tile map, the step of the party, the sight, and the walked tiles replace
    /// the patrol of the first world (D-106, D-528, D-567, D-716). PR-8 raised it to 6: the
    /// enemies of a map walk their stations, a body blocks a step, a sight starts a beat,
    /// and an encounter holds the map still (D-208, D-531, D-737 to D-751). PR-9 raised it
    /// to 7: the battle core, the timeline, the party and its pack, and the wait intent of a
    /// battle (D-755 to D-780). PR-80 raised it to 8: the enemy record of each enemy file,
    /// the ability file, and the checks of each id between them (D-557, D-785 to D-787).
    /// PR-66 raised it to 9: the eight elements on the enemy record, the ten statuses of a
    /// fight, and the statuses that last past it (D-790 to D-810). PR-55 raised it to 10:
    /// the content reader refuses the device table that the button prompts read, and the
    /// sight of the party leaves Core (D-814, D-815). PR-48 raised it to 11: the reader of the
    /// normal-map pages and the override grids (D-839). PR-56 raised it to 12: the reader of
    /// the light files, the decor files, and the effect budget (D-842 to D-847). PR-94 raised it
    /// to 13: the reader of a layer of fog takes the noise and its bands in place of a text grid,
    /// and the budget counts one pass for each fog (D-897, D-898). PR-59 raised it to 14:
    /// the reader of the glow file and of the glow of each fire, and the count of the glow pass in
    /// the budget (D-912, D-913).
    /// </summary>
    /// <remarks>
    /// A run record carries this number, and a replay of a record with another number
    /// reports the two numbers and refuses the record (G-5, `RunHeader`). A save carries it
    /// as a label alone: a load reads the snapshot on the rules of this build (D-259). A
    /// change of this number also changes the expected hashes of the identity file (D-504).
    /// </remarks>
    public const int Current = 14;
}
