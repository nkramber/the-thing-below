using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Core.Notices;

/// <summary>The rule that posts a one-line notice (D-221, D-989).</summary>
/// <remarks>
/// A posted notice waits in the run until Game takes it with `RunState.TakeNotices`, and Game
/// shows it at the top edge (D-221, D-994). A notice that content marks to log also lands in
/// the notice log of the run (D-983). The debug console posts one, and the services, the chest,
/// the door, the task, and the key item call this rule (D-989). A notice of a chest names a thing
/// or a count, and a notice with a value never lands in the log, because the log holds ids alone
/// (D-985, D-1224).
/// </remarks>
public static class NoticeRules
{
    /// <summary>Posts one notice.</summary>
    /// <param name="state">The run.</param>
    /// <param name="notice">The id of the notice.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <param name="log">The log entries of this tick.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">The notice file holds no notice with that id (T-2).</exception>
    public static void Post(RunState state, ContentId notice, RunContext context, List<LogEntry> log) =>
        Post(state, notice, null, null, context, log);

    /// <summary>Posts one notice with the values that its line reads (D-1224).</summary>
    /// <param name="state">The run.</param>
    /// <param name="notice">The id of the notice.</param>
    /// <param name="thing">The thing that the line names in the singular, or no value.</param>
    /// <param name="count">The count that the line names, or no value.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <param name="log">The log entries of this tick.</param>
    /// <exception cref="ArgumentNullException">An argument other than a value is null (T-2).</exception>
    /// <exception cref="SimulationException">
    /// The notice file holds no notice with that id, or a notice that logs takes a value, which the
    /// notice log cannot hold (D-985, T-2).
    /// </exception>
    public static void Post(RunState state, ContentId notice, ContentId? thing, int? count, RunContext context, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(notice);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(log);

        if (!state.Notices.Holds(notice))
        {
            throw new SimulationException(
                $"a post of the notice '{notice.Value}', and the notice file holds no notice with that id (D-989)",
                context);
        }

        NoticeRecord record = state.Notices.Notice(notice);
        if (record.Logs && (thing is not null || count is not null))
        {
            throw new SimulationException(
                $"a post of the notice '{notice.Value}' with a value, and the notice logs, and the notice log holds the id alone (D-985, D-1224)",
                context);
        }

        state.AddNotice(new PostedNotice(record.Id, record.Logs, thing, count));
        var fields = new List<LogField> { new("notice", notice.Value) };
        if (thing is ContentId named)
        {
            fields.Add(new LogField("thing", named.Value));
        }

        if (count is int number)
        {
            fields.Add(LogField.OfNumber("count", number));
        }

        log.Add(new LogEntry(
            LogLevel.Info,
            record.Logs ? "a notice showed and entered the log" : "a notice showed",
            state.Tick,
            LogSubsystems.Run,
            fields));
    }
}
