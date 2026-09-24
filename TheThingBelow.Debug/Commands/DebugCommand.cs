using System;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Debug.Commands;

/// <summary>
/// One command of the debug console (D-171). A command changes the run through an intent, or
/// reports a value of the run and changes nothing (D-724).
/// </summary>
/// <remarks>
/// A command that changes the run carries the id of its intent and the handler of that
/// intent. The console sends the intent, the host queues it, and the handler of the seam
/// changes the state on the tick that the record holds. Thus a run that used the command
/// still replays (D-171, D-260, T-7).
/// <para>
/// A command that reports changes no state, so it makes no intent and the record stays free
/// of a line that no rule reads (D-724).
/// </para>
/// <para>
/// A battle command that aims takes one argument: the slot of its target, on the side that the
/// command names. The intent carries the target, so the record holds it (D-764, D-767). No
/// other command takes an argument.
/// </para>
/// </remarks>
public sealed class DebugCommand
{
    private readonly Func<RunState, string>? report;

    private DebugCommand(
        string name,
        string summary,
        ContentId? action,
        DebugIntentHandler? handler,
        Func<RunState, string>? report,
        BattleSide? targetSide)
    {
        this.TargetSide = targetSide;
        this.Name = name;
        this.Summary = summary;
        this.Action = action;
        this.Handler = handler;
        this.report = report;
    }

    /// <summary>The word that the person types, such as `reveal`.</summary>
    public string Name { get; }

    /// <summary>One line that says what the command does, which `help` prints.</summary>
    public string Summary { get; }

    /// <summary>The id of the intent, or no value when the command reports alone.</summary>
    public ContentId? Action { get; }

    /// <summary>The handler of the intent, or no value when the command reports alone.</summary>
    public DebugIntentHandler? Handler { get; }

    /// <summary>The side of the slot that the command takes as its argument, or no value when it takes none (D-767).</summary>
    public BattleSide? TargetSide { get; }

    /// <summary>Makes a command that changes the run through a debug intent (D-171).</summary>
    /// <param name="name">The word that the person types, such as `reveal`.</param>
    /// <param name="summary">One line for `help`.</param>
    /// <param name="action">The id of the intent, which <see cref="DebugCommandIds"/> holds.</param>
    /// <param name="handler">The rule of the command, which the seam of D-260 calls.</param>
    /// <returns>The command.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The name or the summary is empty (T-2).</exception>
    public static DebugCommand OfIntent(
        string name,
        string summary,
        ContentId action,
        DebugIntentHandler handler)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(summary);
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(handler);

        return new DebugCommand(name, summary, action, handler, null, null);
    }

    /// <summary>Makes a battle command that takes the slot of its target as its argument (D-767).</summary>
    /// <param name="name">The word that the person types, such as `attack`.</param>
    /// <param name="summary">One line for `help`.</param>
    /// <param name="action">The id of the intent, which <see cref="DebugCommandIds"/> holds.</param>
    /// <param name="handler">The rule of the command, which the seam of D-260 calls.</param>
    /// <param name="targetSide">The side of the slot that the argument names.</param>
    /// <returns>The command.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The name or the summary is empty (T-2).</exception>
    public static DebugCommand OfAimedIntent(
        string name,
        string summary,
        ContentId action,
        DebugIntentHandler handler,
        BattleSide targetSide)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(summary);
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(handler);

        return new DebugCommand(name, summary, action, handler, null, targetSide);
    }

    /// <summary>Makes a command that reads the run and changes nothing (D-724).</summary>
    /// <param name="name">The word that the person types, such as `hash`.</param>
    /// <param name="summary">One line for `help`.</param>
    /// <param name="report">The line that the command prints, from the state of the run.</param>
    /// <returns>The command.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The name or the summary is empty (T-2).</exception>
    public static DebugCommand OfReport(string name, string summary, Func<RunState, string> report)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(summary);
        ArgumentNullException.ThrowIfNull(report);

        return new DebugCommand(name, summary, null, null, report, null);
    }

    /// <summary>True when the command sends an intent that the run record holds (D-171).</summary>
    public bool MakesIntent => this.Action is not null;

    /// <summary>Reads the run and gives the line of a report command (D-724).</summary>
    /// <param name="state">The state of the run, which the command reads and never changes.</param>
    /// <returns>The line for the console.</returns>
    /// <exception cref="ArgumentNullException">The state is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">The command sends an intent and reports nothing (T-2).</exception>
    public string Report(RunState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (this.report is null)
        {
            throw new InvalidOperationException(
                $"The command '{this.Name}' sends the intent '{this.Action?.Value}' and reports no line (T-2).");
        }

        return this.report(state);
    }
}
