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
    /// PR-103 raised it to 27: a step into a group inside its grace time starts no encounter, a move intent ends
    /// with its tick when a battle, a menu, or a story scene holds the world (D-1085), an encounter waits for the end of the
    /// step of the lead (D-1094), the reader takes a glow halo up to 192 pixels wide (D-1092), and a content field name of
    /// points alone fails with its file, and the record reader checks its versions before its snapshot and reads format 3.
    /// PR-104 raised it to 28: the reply of the evaluator scores the best legal strike of the next character, lesson
    /// strikes included, and its row term reads their reach (D-1101). A step onto a tile trigger plays its story scene
    /// before a step into an enemy on the same tick (D-1103), and a step into the enemy whose mark runs takes the side
    /// of the beat (D-1104). An absorbed hit rolls no status, the enemy phase runs 1000 turns at most, and no fight
    /// starts or resumes with no character on its feet (D-1105). The load refuses a miss ceiling above 6666 (D-1107),
    /// more lesson slots than the snapshot holds, and a torch that is not a key item, and the resume refuses a mark or
    /// an encounter of a dead enemy, a fight of another party count, and an open menu in a story scene.
    /// PR-105 raised it to 29: each step of a story scene takes an id, and the snapshot stores it (D-1112). A resume
    /// of a save of another build matches each enemy by its id, moves a lead off the edited map to the spawn point,
    /// and finds a moved step by its id (D-1111).
    /// PR-106 raised it to 30: the light budget counts each torch at the widest step of its fire (D-891), and the
    /// particle budget of a fight counts the weather, the largest hit burst, and the largest spell burst at once
    /// (D-1032).
    /// PR-14 raised it to 31: a map file names its kind, a hub or a dungeon, and it holds its NPCs and its services
    /// (D-112, D-1131, D-1137, D-1138). The load checks the range, the route, and the start of each NPC, the host and
    /// the flags of each service, and the NPC of each talk trigger, and it refuses a service on a dungeon. A service
    /// point is a solid thing that blocks each step onto its tile (D-1142). Each NPC walks on the world tick on a
    /// stream of its own: a wander NPC draws a direction or a pause on each pace tick, a route NPC waits at each route
    /// tile, and a chaser takes the step closest to its target (D-1137, D-1138). An NPC is solid: no NPC steps onto
    /// the lead, a wall, a thing, another NPC, or an enemy, no enemy steps onto an NPC, and a step of the lead into an
    /// NPC turns the lead alone. A story scene ends the step of each NPC (D-1139). The state hash and the snapshot
    /// hold each NPC and the NPC stream. The party gains a reserve: a join into a full party goes to the reserve, and
    /// the party swap trades one character of the party with one of the reserve while a menu is open outside a fight
    /// and an encounter (D-1134, D-1136). A downed character can go out, and a downed reserve character never comes
    /// in (D-1135). A rest and a save point restore the reserve too. After a battle won, each reserve character earns
    /// half the experience and half the lesson points, and a downed reserve character earns none (D-73, D-357,
    /// D-974, D-1022). The state hash and the snapshot hold the reserve. The confirm of the player acts on the tile
    /// that the lead faces while it stands: an NPC there ends its step and turns to the lead, and then its talk
    /// trigger fires or its service opens, and a service point opens its service (D-1131, D-1139, D-1142). A service
    /// whose condition fails posts a notice and stays closed, and an open service opens the menu (D-543). The rest
    /// intent fills and cures the party and the reserve at the open rest service, and the save intent emits a request
    /// for the slot save at the open save service (D-390, D-1132, D-1141). A move step and a face step of a story scene
    /// can name an NPC of the map, a show step can put a scene-only NPC on a marker, and an NPC that a story scene
    /// leaves outside its home walks home on a shortest path after it (D-1006, D-1140). The state hash and the
    /// snapshot hold the walk home of each NPC. A line can name an NPC speaker with no body on the map (D-1146). After
    /// a talk, an NPC holds its pace, or a route NPC the wait of its route tile, before it moves again, and the
    /// confirm follows the walk of the NPCs in the world tick (D-1147). A run holds a set of maps, and a debug intent
    /// that names a map puts the party on the spawn point of that map, outside a battle, an encounter, a story scene,
    /// and a menu. The entry notes the entry triggers of the map, the map that the party leaves keeps no memory, and
    /// the entry to a hub asks for the autosave (D-224, D-1132, D-1133).
    /// PR-65 raised it to 32. Each enemy record holds a gold range, and a win draws one gold amount for each fallen
    /// enemy on the battle stream, right after the drops of that enemy, and adds the sum to the party (D-1157). A
    /// rest takes the price of its service (D-1156). A shop service buys a count of one shown entry of its stock for
    /// the price of the entry, and sells a count of one used-up item or spare piece of the pack for the value of the
    /// record at the rate of its shop type, at least 1 gold (D-1149 to D-1155, D-1158). A buy of a counted entry
    /// lowers its count, and the state hash and the snapshot hold each count that a buy changed (D-1152).
    /// PR-36 raised it to 33. A choose step holds two to four options, and the load refuses a fifth (D-1175).
    /// PR-107 raised it to 34. Ability power, AP, replaced MP as the one pool of each character, and each form of a
    /// lesson costs at least 1 AP (D-1197, D-1213). An enemy that falls gives each character who is not down 10% of
    /// full AP, and a basic attack that hits gives the attacker 5%, each rounded down with a floor of 1 and a cap at
    /// full AP (D-1198, D-1210). The rules file holds the two rates, each from 1 to 10000, and a regain event carries each gain (D-1211).
    /// PR-16 raised it to 35. A door, a chest, and a save point are solid. A confirm at a door opens it, or its lock with
    /// its key or a Theft drill of a standing character of the party on a pickable lock (D-386, D-1219). A confirm at a
    /// chest takes its gold, then each entry up to the stack limit, the fallback item of an owned lesson, and keeps the
    /// rest (D-385, D-1024, D-1220). A confirm at a save point opens the save window, and the save restores nothing. The
    /// save service of a hub is gone (D-1221). An arrival on an exit enters its map (D-1216). The memory of each map keeps
    /// each killed enemy, each open door, and each chest past the exit, and the state hash holds it. A flag of the reopen
    /// list of a map brings its killed enemies back at the next entry (D-555).
    /// PR-64 raised it to 36. A trap fires one time when the lead steps onto it: a share of full health, a status that
    /// lasts, or a fight in which the enemies act first, and the memory of the map keeps it spent (D-1226, D-1229 to
    /// D-1231). A trap shows near the lead while a standing fighter carries a Theft drill, and a confirm disarms it
    /// (D-1228). A step on deep snow takes 32 ticks, and the lead slides over ice (D-1232, D-1233). Each 60 world ticks,
    /// poison hurts each poisoned character and bad air hurts each fighter, and a down of each fighter on the map wipes
    /// the party (D-397, D-1234 to D-1236).
    /// PR-35 raised it to 37. A map can be an overworld, with grass, forest, mountain, and water tiles (D-1242, D-1256).
    /// An arrival on an entrance of the overworld enters the spawn point of its place, and an exit to the overworld
    /// puts the party on the marker that it names (D-1243, D-1255). The lead steps onto a gate only while its
    /// condition holds, and a confirm at a closed gate posts its notice (D-1243, D-1257). The entry to the overworld
    /// asks for the autosave (D-1246).
    /// </summary>
    /// <remarks>
    /// A run record carries this number, and a replay of a record with another number
    /// reports the two numbers and refuses the record (G-5, `RunHeader`). A load reads the
    /// snapshot of a save on the rules of this build (D-259), and a save whose number or content
    /// hash differs from this build takes the drift rules of D-1111 and D-1112. A change of this
    /// number also changes the expected hashes of the identity file (D-504).
    /// </remarks>
    public const int Current = 37;
}
