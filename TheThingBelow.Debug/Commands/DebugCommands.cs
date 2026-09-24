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
/// change nothing: `help`, `hash`, `where`, and `battle` (D-724). The `torch` command changes
/// the view alone: it turns the carried light on or off, and it sends no intent (D-851).
/// <para>
/// PR-9 replaced the `flee` command of PR-8, which ended an encounter with no battle, with the
/// flee of the battle rules (D-378, D-767).
/// </para>
/// <para>
/// Each later PR that gives the rules a new value can add its own commands here, such as the
/// story flags of PR-68. PR-67 added the level, the experience, and the MP, and no command. PR-12 added
/// the `swap` command, which marks a swap place of lessons (D-1030).
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

    /// <summary>The name of the command that turns the carried light on or off (D-851).</summary>
    public const string TorchName = "torch";

    /// <summary>The name of the command that posts a notice that logs (D-989).</summary>
    public const string NoticeName = "notice";

    /// <summary>The name of the command that posts a notice that does not log (D-989).</summary>
    public const string AsideName = "aside";

    /// <summary>The name of the command that marks a swap place of lessons (D-1030).</summary>
    public const string SwapName = "swap";

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
        DebugCommand.OfIntent(SwapName, "marks the place of the party as a swap place, until the next step (D-1030)", DebugCommandIds.SwapPlace, MarkSwapPlace),
        DebugCommand.OfView(TorchName, "turns the carried light on or off, and sends no intent (D-847, D-851)"),
        DebugCommand.OfReport(BattleName, "gives each combatant, the turn, and the strip", BattleOf),
        DebugCommand.OfReport(HashName, "gives the state hash of the run", HashOf),
        DebugCommand.OfReport(WhereName, "gives the tick and the place of the party", PlaceOf),
        DebugCommand.OfReport(HelpName, "lists every command", HelpOf),
    ];

    /// <summary>Every command of this build, in the order that `help` prints (D-724).</summary>
    public static IReadOnlyList<DebugCommand> All => Commands;

    /// <summary>
    /// The handlers that a development build passes to <see cref="Simulation.Start"/> through
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
    /// Marks the place of the party as a swap place, so a person swaps lessons before PR-14 and
    /// PR-16 mark the hubs and the save points (D-1030). The next step of the lead leaves it.
    /// </summary>
    private static void MarkSwapPlace(RunState state, Intent intent, RunContext context, List<LogEntry> log) =>
        state.Characters.MarkSwapPlace();

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
            log.Add(new LogEntry(
                LogLevel.Warning,
                $"the command found {refusal}, and it changed nothing",
                state.Tick,
                LogSubsystems.Run,
                [new LogField("context", context.Describe())]));
            return;
        }

        BattleTurns.Act(state, choice, context, log);
    }

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
