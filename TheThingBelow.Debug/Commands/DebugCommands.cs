using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Notices;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Debug.Commands;

/// <summary>
/// The commands of the debug console of this build (D-171, D-724). A development build alone
/// holds this class, because the assembly that carries it never reaches a release export
/// (D-260, D-492).
/// </summary>
/// <remarks>
/// Six commands change the run. The `reveal` command marks every tile of the map as walked
/// (D-567). The five battle commands take the turn of a character until the battle screen of
/// PR-10: `attack`, `defend`, `step`, `item`, and `flee` (D-767). Four commands report and
/// change nothing: `help`, `hash`, `where`, and `battle` (D-724). PR-91 removed the `torch`
/// command of PR-56, because the torch action of the player replaced it (D-1071).
/// <para>
/// PR-9 replaced the `flee` command of PR-8, which ended an encounter with no battle, with the
/// flee of the battle rules (D-378, D-767).
/// </para>
/// <para>
/// Each later PR that gives the rules a new value can add its own commands here, such as the
/// story flags of PR-68. PR-67 added the level, the experience, and the MP, and no command. PR-12 added
/// the `swap` command, which marked a swap place of lessons, and PR-99 removed it, because a
/// swap of lessons needs no place (D-1030, D-1050). PR-13 added the `stock` command,
/// which fills the pack for a test of the gear window and the item window (D-1038). PR-14 added
/// the `goto` command, which takes the id of a map and puts the party on its spawn point, so a
/// build reaches a hub before the travel of PR-35 (D-1133).
/// </para>
/// </remarks>
public static class DebugCommands
{
    /// <summary>The name of the command that lists every command.</summary>
    public const string HelpName = "help";

    /// <summary>The name of the command that reveals the map.</summary>
    public const string RevealName = "reveal";

    /// <summary>The name of the command that gives the state hash.</summary>
    public const string HashName = "hash";

    /// <summary>The name of the command that gives the place of the party.</summary>
    public const string WhereName = "where";

    /// <summary>The name of the command that attacks one enemy (D-767).</summary>
    public const string AttackName = "attack";

    /// <summary>The name of the command that defends (D-755, D-767).</summary>
    public const string DefendName = "defend";

    /// <summary>The name of the command that steps to the other row (D-380, D-767).</summary>
    public const string StepName = "step";

    /// <summary>The name of the command that uses an item on one character (D-767, D-780).</summary>
    public const string ItemName = "item";

    /// <summary>The name of the command that tries to flee (D-378, D-767).</summary>
    public const string FleeName = "flee";

    /// <summary>The name of the command that reports the battle (D-767).</summary>
    public const string BattleName = "battle";

    /// <summary>The name of the command that posts a notice that logs (D-989).</summary>
    public const string NoticeName = "notice";

    /// <summary>The name of the command that posts a notice that does not log (D-989).</summary>
    public const string AsideName = "aside";

    /// <summary>The name of the command that puts one copy of each item and each piece of gear in the pack (D-1038).</summary>
    public const string StockName = "stock";

    /// <summary>The name of the command that puts the party on the spawn point of one map (D-1133).</summary>
    public const string GoToName = "goto";

    // The order of this list is the order of `help`, and it never follows a hash of a name
    // (G-4). The list is short, so a walk of it reads better than a map of one entry (T-1).
    private static readonly IReadOnlyList<DebugCommand> Commands =
    [
        DebugCommand.OfIntent(
            RevealName,
            "marks every tile of the map as walked",
            DebugCommandIds.RevealMap,
            RevealMap),
        DebugCommand.OfAimedIntent(
            AttackName,
            "attacks the enemy of one slot on the turn of a character",
            DebugCommandIds.BattleAttack,
            Attack,
            BattleSide.Enemy),
        DebugCommand.OfIntent(DefendName, "defends on the turn of a character", DebugCommandIds.BattleDefend, Defend),
        DebugCommand.OfIntent(StepName, "steps to the other row on the turn of a character", DebugCommandIds.BattleStep, Step),
        DebugCommand.OfAimedIntent(
            ItemName,
            "uses the first item of the pack on the character of one slot",
            DebugCommandIds.BattleItem,
            UseItem,
            BattleSide.Party),
        DebugCommand.OfIntent(FleeName, "tries to flee on the turn of a character", DebugCommandIds.BattleFlee, Flee),
        DebugCommand.OfIntent(NoticeName, "posts the first notice of the notice file that logs", DebugCommandIds.NoticeLogged, PostLogged),
        DebugCommand.OfIntent(AsideName, "posts the first notice of the notice file that does not log", DebugCommandIds.NoticePlain, PostPlain),
        DebugCommand.OfIntent(StockName, "puts one copy of each item and each piece of gear in the pack, to each stack limit (D-1038)", DebugCommandIds.Stock, Stock),
        DebugCommand.OfMapIntent(GoToName, "puts the party on the spawn point of the map of one id (D-1133)", DebugCommandIds.GoToMap, GoToMap),
        DebugCommand.OfReport(BattleName, "gives each combatant, the turn, and the strip", BattleOf),
        DebugCommand.OfReport(HashName, "gives the state hash of the run", HashOf),
        DebugCommand.OfReport(WhereName, "gives the tick and the place of the party", PlaceOf),
        DebugCommand.OfReport(HelpName, "lists every command", HelpOf),
    ];

    /// <summary>Every command of this build, in the order that `help` prints (D-724).</summary>
    public static IReadOnlyList<DebugCommand> All => Commands;

    /// <summary>
    /// The handlers that a development build passes to the start of a <see cref="Simulation"/> through
    /// the seam of D-260. A release build passes <see cref="DebugIntentHandlers.None"/>.
    /// </summary>
    /// <returns>One handler for each command that sends an intent.</returns>
    public static DebugIntentHandlers Handlers()
    {
        List<KeyValuePair<ContentId, DebugIntentHandler>> handlers = [];
        foreach (DebugCommand command in Commands)
        {
            if (command.Action is not null && command.Handler is not null)
            {
                handlers.Add(new KeyValuePair<ContentId, DebugIntentHandler>(
                    command.Action,
                    command.Handler));
            }
        }

        return new DebugIntentHandlers(handlers);
    }

    /// <summary>Finds the command of one name.</summary>
    /// <param name="name">The word that the person typed.</param>
    /// <param name="command">The command, or null when no command takes that name.</param>
    /// <returns>True when a command takes that name.</returns>
    /// <exception cref="ArgumentException">The name is empty (T-2).</exception>
    public static bool TryFind(string name, out DebugCommand? command)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        foreach (DebugCommand each in Commands)
        {
            if (string.CompareOrdinal(each.Name, name) == 0)
            {
                command = each;
                return true;
            }
        }

        command = null;
        return false;
    }

    /// <summary>
    /// Marks every tile of the map as walked (D-567). The map HUD of PR-64 draws the record
    /// of the walked tiles, so the command shows the whole map.
    /// </summary>
    private static void RevealMap(RunState state, Intent intent, RunContext context, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(log);

        MapState party = state.Party;
        for (int row = 0; row < party.Map.Height; row += 1)
        {
            for (int column = 0; column < party.Map.Width; column += 1)
            {
                party.Walked.Mark(new TilePoint(column, row));
            }
        }
    }

    /// <summary>
    /// Puts one copy of each item and each piece of gear in the pack, so a person tries the gear
    /// window and the item window before PR-16 builds the chests (D-1038). A copy over the stack
    /// limit stays out of the pack, and a line names it (D-385).
    /// </summary>
    private static void Stock(RunState state, Intent intent, RunContext context, List<LogEntry> log)
    {
        var ids = new List<ContentId>(state.BattleContent.Items.Ids);
        ids.AddRange(state.BattleContent.Gear.Ids);
        foreach (ContentId id in ids)
        {
            if (state.Characters.Pick(id, 1, state.BattleContent) > 0)
            {
                log.Add(new LogEntry(
                    LogLevel.Info,
                    "the pack holds the stack limit, and the copy stays out of it (D-385)",
                    state.Tick,
                    LogSubsystems.Run,
                    [new LogField("id", id.Value), new LogField("context", context.Describe())]));
            }
        }
    }

    /// <summary>Posts the first notice that logs, so a person sees the notice and its entry in the log (D-989).</summary>
    private static void PostLogged(RunState state, Intent intent, RunContext context, List<LogEntry> log) =>
        NoticeRules.Post(state, state.Notices.FirstThatLogs(true).Id, context, log);

    /// <summary>Posts the first notice that does not log, so a person sees a notice that the log leaves out (D-989).</summary>
    private static void PostPlain(RunState state, Intent intent, RunContext context, List<LogEntry> log) =>
        NoticeRules.Post(state, state.Notices.FirstThatLogs(false).Id, context, log);

    private static void Attack(RunState state, Intent intent, RunContext context, List<LogEntry> log) =>
        ActOrWarn(state, new BattleChoice(BattleAction.Attack, intent.Target, null), context, log);

    private static void Defend(RunState state, Intent intent, RunContext context, List<LogEntry> log) =>
        ActOrWarn(state, new BattleChoice(BattleAction.Defend, null, null), context, log);

    private static void Step(RunState state, Intent intent, RunContext context, List<LogEntry> log) =>
        ActOrWarn(state, new BattleChoice(BattleAction.Step, null, null), context, log);

    private static void UseItem(RunState state, Intent intent, RunContext context, List<LogEntry> log) =>
        ActOrWarn(state, new BattleChoice(BattleAction.Item, intent.Target, intent.Item), context, log);

    private static void Flee(RunState state, Intent intent, RunContext context, List<LogEntry> log) =>
        ActOrWarn(state, new BattleChoice(BattleAction.Flee, null, null), context, log);

    /// <summary>
    /// Puts the party on the spawn point of the map that the intent names, so a build reaches a
    /// hub before the travel of PR-35 (D-1133). A person can type the command in a battle, in a
    /// story scene, with the menu open, or with an id that no map of the run takes. That is a fault
    /// of the person, so the command changes nothing and writes a warning with the reason (D-179,
    /// T-2). The map that the party leaves keeps no memory, because PR-35 owns the memory of each
    /// map, and the entry to a hub asks for the autosave (D-224, D-1132).
    /// </summary>
    /// <exception cref="SimulationException">The intent names no map, which points at a fault of the record (T-2).</exception>
    private static void GoToMap(RunState state, Intent intent, RunContext context, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(intent);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(log);

        ContentId map = intent.Map ?? throw new SimulationException("a go-to-map intent that names no map (D-1133)", context);
        string? refusal = state.RefusalOfEnter(map);
        if (refusal is not null)
        {
            Warn(state, refusal, context, log);
            return;
        }

        state.EnterMap(map, context);
        log.Add(new LogEntry(
            LogLevel.Info,
            "the command put the party on the spawn point of a map",
            state.Tick,
            LogSubsystems.Run,
            [new LogField("map", map.Value), new LogField("kind", MapKinds.NameOf(state.Party.Map.Kind))]));
    }

    /// <summary>
    /// Takes the turn of a character with one choice. A person can type a battle command with
    /// no battle, on the turn of an enemy, or at a slot that melee does not reach. That is a fault
    /// of the person and never of the build, so the command changes nothing and writes a warning
    /// with the reason (D-179, T-2). The smoke session and a bot of PR-15 send every command.
    /// </summary>
    private static void ActOrWarn(RunState state, BattleChoice choice, RunContext context, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(log);

        string? refusal = BattleTurns.RefusalOf(state, choice);
        if (refusal is not null)
        {
            Warn(state, refusal, context, log);
            return;
        }

        BattleTurns.Act(state, choice, context, log);
    }

    /// <summary>Writes the warning of a command that a rule refused, which changed nothing (D-179, T-2).</summary>
    private static void Warn(RunState state, string refusal, RunContext context, List<LogEntry> log) =>
        log.Add(new LogEntry(
            LogLevel.Warning,
            $"the command found {refusal}, and it changed nothing",
            state.Tick,
            LogSubsystems.Run,
            [new LogField("context", context.Describe())]));

    /// <summary>Gives each combatant with its slot, the combatant whose turn it is, and the strip (D-756, D-767).</summary>
    private static string BattleOf(RunState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (state.Battle is not Battle battle)
        {
            return "no battle runs";
        }

        List<string> lines = [$"the battle of '{battle.Group.Id.Value}' is {Battle.OutcomeName(battle.Outcome)}"];
        foreach (Combatant combatant in battle.All())
        {
            lines.Add(
                $"{combatant.Target.Describe()}: {combatant.Id.Value}, health {combatant.Health} of {combatant.FullHealth}, "
                + $"{BattleSides.NameOf(combatant.Row)} row, {Battle.PlaceName(combatant.Place)}, next turn at {combatant.ReadyAt}");
        }

        if (battle.Outcome == BattleOutcome.Running && battle.Next() is Combatant next)
        {
            List<string> strip = [];
            foreach (BattleTarget turn in battle.Strip(state.BattleContent.Rules, state.Context("console/battle")))
            {
                strip.Add(turn.Describe());
            }

            lines.Add($"the turn of {next.Target.Describe()}, then {string.Join(", ", strip)}");
        }

        return string.Join(DebugSession.LineBreak, lines);
    }

    /// <summary>Gives the state hash of the run, which a replay compares (G-5).</summary>
    private static string HashOf(RunState state) => $"the state hash is 0x{state.StateHash():x16}";

    /// <summary>Gives the tick, the map, the tile of the lead, and the count of walked tiles.</summary>
    private static string PlaceOf(RunState state) =>
        $"tick {state.Tick} on the map '{state.Party.Map.Id.Value}', "
        + $"the lead at {state.Party.LeadAt} facing {state.Party.Facing}, "
        + $"with {state.Party.Walked.Count} walked tiles";

    /// <summary>Lists every command of this build, one on each line.</summary>
    private static string HelpOf(RunState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        List<string> lines = [];
        foreach (DebugCommand command in Commands)
        {
            string mark = command.MakesIntent ? " (the record holds it)" : string.Empty;
            lines.Add($"{command.Name}: {command.Summary}{mark}");
        }

        // The console splits a report on this character, and a log line of the smoke session
        // then reads the same on every system (D-117).
        return string.Join(DebugSession.LineBreak, lines);
    }
}
