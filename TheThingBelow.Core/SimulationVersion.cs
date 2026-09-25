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
    /// the reader of the glow file and of the glow of each fire, the bound of the lit art below the
    /// glow threshold, the count of the glow pass in the budget, and the refusal of a lit fog
    /// (D-910, D-912, D-913, D-916). PR-92 raised it to 15: the reader of the shaft kinds, the shafts of
    /// a decor file, and the file of the passes of the HD-2D look, and the count of the passes of each
    /// map in the budget (D-917, D-918, D-920). PR-60 raised it to 16: the reader of the transition files and
    /// the transition table, the kind of an encounter, the pick of a transition, and the count of the
    /// transition pass of each map in the budget (D-934 to D-941). PR-11 raised it to 17: the
    /// evaluator chooses the turn of each enemy, with its own stream for a tie, an enemy can strike
    /// with an ability, heal, defend, and step, and the groups and the profiles live in their own
    /// files (D-947, D-955 to D-960). PR-98 raised it to 18: the load refuses a group whose waiting
    /// column is taller than the field (D-963). PR-67 raised it to 19: the stats of a character follow the curve of its
    /// level, a battle won gives experience with the shrink of each enemy, a level-up fills the health and the MP, and
    /// the party holds the level, the experience, and the MP (D-966 to D-974). PR-62 raised it to 20: the party
    /// window moves a character to the other row, a rule posts a notice, and the run holds the notice log (D-558,
    /// D-983 to D-985, D-989). PR-68 raised it to 21: a map fires its story scene triggers, Core runs each step of a story scene
    /// and holds the flags, a step starts a battle that no party flees, a cast member joins the party, and the pause holds a
    /// story scene (D-540, D-997 to D-1013). PR-12 raised it to 22: each character carries lessons in the slots of
    /// its level, a form of a lesson acts in a fight with the aptitude bonus, a Mend rite and a cure rite act from the menu,
    /// each equipped lesson gains points from a battle won, and a swap of lessons needs a swap place (D-1018 to D-1033).
    /// PR-13 raised it to 23: each character wears gear in six slots, which adds to its stats and gives its element
    /// table, the pack holds each item and piece to its stack limit, an item heals, restores, cures, or revives, a
    /// Theft drill steals three times in a fight at most, and a win rolls the drops of each profile (D-1036 to D-1050).
    /// PR-99 raised it to 24: each stat row holds magic and resistance, a strike reads the attack or the magic of its
    /// stat, a heal reads the magic and draws the hit factor, an absorb heals a quarter of the hit, and a swap of lessons
    /// needs no place (D-1050, D-1052 to D-1059).
    /// PR-91 raised it to 25: a map file names each dark map, the party sees 2 tiles there with the torch put away and
    /// 6 tiles with it held out, a held torch gives each patrol of a dark map 4 tiles more, and the party holds the
    /// torch out or puts it away on the walk (D-1062 to D-1064, D-1071).
    /// PR-101 raised it to 26: the glow of a fire is a soft halo up to 128 pixels wide, and the load refuses a halo
    /// that passes the glow threshold in place of one that stays below it (D-1075).
    /// PR-103 raised it to 27: a step into a group inside its grace time starts no encounter, and a move intent ends
    /// with its tick when a battle, a menu, or a story scene holds the world (D-1085).
    /// </summary>
    /// <remarks>
    /// A run record carries this number, and a replay of a record with another number
    /// reports the two numbers and refuses the record (G-5, `RunHeader`). A save carries it
    /// as a label alone: a load reads the snapshot on the rules of this build (D-259). A
    /// change of this number also changes the expected hashes of the identity file (D-504).
    /// </remarks>
    public const int Current = 27;
}
