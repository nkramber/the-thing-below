using TheThingBelow.Core.Content;

namespace TheThingBelow.Debug.Commands;

/// <summary>
/// The intent id of each console command that changes the state of a run (D-171, D-646).
/// Core reads no id of this class, and the seam of D-260 carries each one to its handler.
/// </summary>
/// <remarks>
/// The kind is `debug`, which names the source of the id: the debug assembly (D-646, D-727).
/// A reader of a record thus names the source of a line from the id alone, beside the debug
/// mark that <see cref="Core.Runs.Intent.IsDebug"/> carries (D-171).
/// <para>
/// An id is permanent, so no later entry takes one (D-166). An id of this class never enters
/// `IntentIds` of Core, because no rule of Core reads it (D-492).
/// </para>
/// </remarks>
public static class DebugCommandIds
{
    /// <summary>The file that holds these ids, for the error of a malformed id (T-2).</summary>
    private const string Source = "TheThingBelow.Debug/Commands/DebugCommandIds.cs";

    /// <summary>The console marked every tile of the map as walked (D-567).</summary>
    public static readonly ContentId RevealMap =
        ContentId.Parse("debug.reveal_map", Source, nameof(RevealMap));

    // PR-8 held the id `debug.flee_encounter`, which ended an encounter as a flee (D-749).
    // The battle of PR-9 replaced it (D-767), and no later entry takes that id (D-166).

    /// <summary>The console attacked one enemy on the turn of a character (D-767).</summary>
    public static readonly ContentId BattleAttack =
        ContentId.Parse("debug.battle_attack", Source, nameof(BattleAttack));

    /// <summary>The console defended on the turn of a character (D-755, D-767).</summary>
    public static readonly ContentId BattleDefend =
        ContentId.Parse("debug.battle_defend", Source, nameof(BattleDefend));

    /// <summary>The console stepped to the other row on the turn of a character (D-380, D-767).</summary>
    public static readonly ContentId BattleStep =
        ContentId.Parse("debug.battle_step", Source, nameof(BattleStep));

    /// <summary>The console used one item on one character (D-767, D-780).</summary>
    public static readonly ContentId BattleItem =
        ContentId.Parse("debug.battle_item", Source, nameof(BattleItem));

    /// <summary>The console tried to flee on the turn of a character (D-378, D-767).</summary>
    public static readonly ContentId BattleFlee =
        ContentId.Parse("debug.battle_flee", Source, nameof(BattleFlee));

    /// <summary>The console posted the first notice of the notice file that logs (D-989).</summary>
    public static readonly ContentId NoticeLogged =
        ContentId.Parse("debug.notice_logged", Source, nameof(NoticeLogged));

    /// <summary>The console posted the first notice of the notice file that does not log (D-989).</summary>
    public static readonly ContentId NoticePlain =
        ContentId.Parse("debug.notice_plain", Source, nameof(NoticePlain));
}
