using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Runs;

/// <summary>
/// The ids of every intent that the rules of this build read (D-493, D-646). Each later
/// Core PR adds the ids of its own screen, such as the steps of the map in PR-7.
/// </summary>
/// <remarks>
/// The kind of each id is `intent`. An id is permanent, so no later entry takes one (D-166).
/// The first two ids open and close the menu, which pauses the world (D-162, D-650).
/// <para>
/// PR-61 added the choice ids and the four step ids. The input map of Game makes each one
/// from an input event, and no rule of this build reads them (D-561, F-50). A record that
/// carries one of them meets the refusal of `Simulation`, which names the intent and the
/// tick, until PR-7 adds the map rules that read them (T-2).
/// </para>
/// </remarks>
public static class IntentIds
{
    /// <summary>The kind of every intent id (D-646).</summary>
    public const string Kind = "intent";

    /// <summary>The file that holds these ids, for the error of a malformed id (T-2).</summary>
    private const string Source = "TheThingBelow.Core/Runs/IntentIds.cs";

    /// <summary>The player opened a menu, and the world pauses from this tick (D-162).</summary>
    public static readonly ContentId OpenMenu = ContentId.Parse("intent.open_menu", Source, nameof(OpenMenu));

    /// <summary>The player closed the menu, and the world runs again from this tick (D-162).</summary>
    public static readonly ContentId CloseMenu = ContentId.Parse("intent.close_menu", Source, nameof(CloseMenu));

    /// <summary>The player chose the thing under the cursor (D-84, D-493).</summary>
    public static readonly ContentId Confirm = ContentId.Parse("intent.confirm", Source, nameof(Confirm));

    /// <summary>The player went back one step (D-84, D-493).</summary>
    public static readonly ContentId Cancel = ContentId.Parse("intent.cancel", Source, nameof(Cancel));

    /// <summary>The player went one step to the north (D-84, D-493).</summary>
    public static readonly ContentId MoveNorth = ContentId.Parse("intent.move_north", Source, nameof(MoveNorth));

    /// <summary>The player went one step to the south (D-84, D-493).</summary>
    public static readonly ContentId MoveSouth = ContentId.Parse("intent.move_south", Source, nameof(MoveSouth));

    /// <summary>The player went one step to the east (D-84, D-493).</summary>
    public static readonly ContentId MoveEast = ContentId.Parse("intent.move_east", Source, nameof(MoveEast));

    /// <summary>The player went one step to the west (D-84, D-493).</summary>
    public static readonly ContentId MoveWest = ContentId.Parse("intent.move_west", Source, nameof(MoveWest));

    /// <summary>The character whose turn it is attacks one enemy (D-359, D-764).</summary>
    public static readonly ContentId BattleAttack = ContentId.Parse("intent.battle_attack", Source, nameof(BattleAttack));

    /// <summary>The character whose turn it is defends (D-755).</summary>
    public static readonly ContentId BattleDefend = ContentId.Parse("intent.battle_defend", Source, nameof(BattleDefend));

    /// <summary>The character whose turn it is steps to the other row (D-380).</summary>
    public static readonly ContentId BattleStep = ContentId.Parse("intent.battle_step", Source, nameof(BattleStep));

    /// <summary>The character whose turn it is uses one item on one character (D-382, D-780).</summary>
    public static readonly ContentId BattleItem = ContentId.Parse("intent.battle_item", Source, nameof(BattleItem));

    /// <summary>The character whose turn it is tries to flee (D-378).</summary>
    public static readonly ContentId BattleFlee = ContentId.Parse("intent.battle_flee", Source, nameof(BattleFlee));

    /// <summary>
    /// The screen of a won or fled battle is done, and the map runs again. Game sends it when
    /// its event queue drains, and a bot sends it at once (D-522, D-532).
    /// </summary>
    public static readonly ContentId WaitBattleEnd = ContentId.Parse("intent.wait_battle_end", Source, nameof(WaitBattleEnd));

    /// <summary>
    /// The party window of a menu moved one character to the other row. The intent names the
    /// character by its slot, as a battle target of the party side (D-377, D-558).
    /// </summary>
    public static readonly ContentId PartyRow = ContentId.Parse("intent.party_row", Source, nameof(PartyRow));
}
