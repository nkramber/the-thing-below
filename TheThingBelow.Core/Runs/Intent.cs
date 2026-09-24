using System;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Runs;

/// <summary>
/// One choice of the player in a tick. An intent names the choice in content ids and state
/// ids, and it never names a key, a button, or a screen position (D-84, D-493).
/// </summary>
/// <remarks>
/// Game makes an intent from each input event, and Core reads intents alone (D-493, F-50).
/// The input map, the remap, and the device kind stay in Game, so a rebind never changes a
/// record (D-214).
/// <para>
/// A debug intent carries the mark of <see cref="IsDebug"/>, so a run that used a debug
/// command still replays and the record says so (D-171). A host with no handler for that
/// action refuses the record, and the report names the intent and the tick (D-492, T-2).
/// </para>
/// <para>
/// A battle intent names its target, and an item use names its item too (D-764, D-780). The
/// row intent of the party window names its character as a target of the party side (D-558).
/// The pick intent of a choose step names the index of its option (D-1007). A lesson use names its
/// lesson and its form, a cast from the menu also names its caster, and a swap names the
/// character, the slot, and the lesson (D-1027, D-1030). Every other intent carries none of
/// these values.
/// </para>
/// </remarks>
/// <param name="Action">The id of the choice, such as `intent.open_menu`.</param>
/// <param name="IsDebug">
/// True when the debug console of a development build made the intent (D-171, D-260).
/// </param>
/// <param name="Target">The side and the slot that a battle intent or a row intent aims at, or no value (D-558, D-764).</param>
/// <param name="Item">The item of an item use, or the piece of a change of gear, or no value (D-780, D-1048).</param>
/// <param name="Option">An index from zero: the option of a pick (D-1007), the form of a lesson use, the lesson slot of a swap, or the gear slot of a change of gear (D-1027, D-1030, D-1048). No value for the other intents.</param>
/// <param name="Lesson">The lesson of a lesson use or a swap, or no value (D-1026).</param>
/// <param name="Actor">The party slot of the character who casts from the menu, or whose slot a swap or a change of gear changes, or no value (D-391, D-1030, D-1048).</param>
public sealed record Intent(ContentId Action, bool IsDebug, BattleTarget? Target = null, ContentId? Item = null, int? Option = null, ContentId? Lesson = null, int? Actor = null)
{
    /// <summary>Makes an intent that the player made through a screen of the game.</summary>
    /// <param name="action">The id of the choice, such as `intent.open_menu`.</param>
    /// <returns>The intent, with no debug mark.</returns>
    /// <exception cref="ArgumentNullException">The action is null (T-2).</exception>
    public static Intent OfPlayer(ContentId action)
    {
        ArgumentNullException.ThrowIfNull(action);

        return new Intent(action, false);
    }

    /// <summary>Makes an intent that the debug console of a development build made (D-171).</summary>
    /// <param name="action">The id of the command, which a host handler reads (D-260).</param>
    /// <returns>The intent, with the debug mark.</returns>
    /// <exception cref="ArgumentNullException">The action is null (T-2).</exception>
    public static Intent OfDebugConsole(ContentId action)
    {
        ArgumentNullException.ThrowIfNull(action);

        return new Intent(action, true);
    }

    /// <summary>Makes a battle intent that the player made, with its target and its item (D-764, D-780).</summary>
    /// <param name="action">The id of the choice, such as `intent.battle_attack`.</param>
    /// <param name="target">The target, or no value.</param>
    /// <param name="item">The item, or no value.</param>
    /// <returns>The intent, with no debug mark.</returns>
    /// <exception cref="ArgumentNullException">The action is null (T-2).</exception>
    public static Intent OfPlayer(ContentId action, BattleTarget? target, ContentId? item)
    {
        ArgumentNullException.ThrowIfNull(action);

        return new Intent(action, false, target, item);
    }

    /// <summary>Makes a battle intent that the debug console made, with its target and its item (D-767).</summary>
    /// <param name="action">The id of the command, which a host handler reads (D-260).</param>
    /// <param name="target">The target, or no value.</param>
    /// <param name="item">The item, or no value.</param>
    /// <returns>The intent, with the debug mark.</returns>
    /// <exception cref="ArgumentNullException">The action is null (T-2).</exception>
    public static Intent OfDebugConsole(ContentId action, BattleTarget? target, ContentId? item)
    {
        ArgumentNullException.ThrowIfNull(action);

        return new Intent(action, true, target, item);
    }

    /// <summary>Makes the pick intent of a choose step that the player made (D-1007).</summary>
    /// <param name="option">The index of the option, from zero.</param>
    /// <returns>The intent, with no debug mark.</returns>
    public static Intent OfPick(int option) => new(IntentIds.StoryPick, false, null, null, option);

    /// <summary>Makes the battle intent of a lesson use: the form of a lesson of the character whose turn it is, on one target (D-1027, D-1031).</summary>
    /// <param name="lesson">The lesson, which a slot of the character holds.</param>
    /// <param name="form">The index of the form, from zero.</param>
    /// <param name="target">The target.</param>
    /// <returns>The intent, with no debug mark.</returns>
    /// <exception cref="ArgumentNullException">The lesson is null (T-2).</exception>
    public static Intent OfBattleLesson(ContentId lesson, int form, BattleTarget target)
    {
        ArgumentNullException.ThrowIfNull(lesson);
        return new Intent(IntentIds.BattleLesson, false, target, null, form, lesson);
    }

    /// <summary>Makes the intent of a cast from the menu: a Mend rite or a cure rite of one character on one character (D-391).</summary>
    /// <param name="caster">The party slot of the caster.</param>
    /// <param name="lesson">The lesson, which a slot of the caster holds.</param>
    /// <param name="form">The index of the form, from zero.</param>
    /// <param name="target">The party slot of the target.</param>
    /// <returns>The intent, with no debug mark.</returns>
    /// <exception cref="ArgumentNullException">The lesson is null (T-2).</exception>
    public static Intent OfMenuCast(int caster, ContentId lesson, int form, int target)
    {
        ArgumentNullException.ThrowIfNull(lesson);
        return new Intent(IntentIds.MenuCast, false, new BattleTarget(BattleSide.Party, target), null, form, lesson, caster);
    }

    /// <summary>Makes the intent of a swap of lessons: a lesson of the lesson pack in one slot, or an empty slot (D-356, D-1030).</summary>
    /// <param name="character">The party slot of the character.</param>
    /// <param name="slot">The lesson slot, from zero.</param>
    /// <param name="lesson">The lesson of the lesson pack, or no value to empty the slot.</param>
    /// <returns>The intent, with no debug mark.</returns>
    public static Intent OfLessonSwap(int character, int slot, ContentId? lesson) =>
        new(IntentIds.LessonSwap, false, null, null, slot, lesson, character);

    /// <summary>Makes the intent of an item use from the item window: one item on one character (D-1046, D-1049).</summary>
    /// <param name="item">The id of the item.</param>
    /// <param name="target">The party slot of the target.</param>
    /// <returns>The intent, with no debug mark.</returns>
    /// <exception cref="ArgumentNullException">The item is null (T-2).</exception>
    public static Intent OfMenuItem(ContentId item, int target)
    {
        ArgumentNullException.ThrowIfNull(item);
        return new Intent(IntentIds.MenuItem, false, new BattleTarget(BattleSide.Party, target), item);
    }

    /// <summary>Makes the intent of a change of gear: a piece of the pack in one gear slot, or an empty slot (D-44, D-1048).</summary>
    /// <param name="character">The party slot of the character.</param>
    /// <param name="slot">The gear slot, from 0 to 5.</param>
    /// <param name="piece">The id of the piece, or no value to empty the slot.</param>
    /// <returns>The intent, with no debug mark. The item field carries the piece.</returns>
    public static Intent OfGearWear(int character, int slot, ContentId? piece) =>
        new(IntentIds.GearWear, false, null, piece, slot, null, character);

    /// <summary>Gives the intent as one line for an error message and a log line (T-2).</summary>
    /// <returns>The action, the item, the target, the option, and the debug mark, each when the intent carries it.</returns>
    public string Describe()
    {
        string target = this.Target is BattleTarget aimed ? $" at {aimed.Describe()}" : string.Empty;
        string item = this.Item is ContentId used ? $" with {used.Value}" : string.Empty;
        string option = this.Option is int picked ? $" option {picked}" : string.Empty;
        string lesson = this.Lesson is ContentId named ? $" lesson {named.Value}" : string.Empty;
        string actor = this.Actor is int slot ? $" by party {slot}" : string.Empty;
        string mark = this.IsDebug ? " (debug)" : string.Empty;
        return $"{this.Action.Value}{actor}{lesson}{item}{target}{option}{mark}";
    }
}
