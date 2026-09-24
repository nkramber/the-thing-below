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
/// the notice log of the run (D-983). No rule of PR-62 posts a notice. The debug console posts
/// one, and the chest, the door, the task, and the key item of later PRs call this rule (D-989).
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
    public static void Post(RunState state, ContentId notice, RunContext context, List<LogEntry> log)
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
        state.AddNotice(record);
        log.Add(new LogEntry(
            LogLevel.Info,
            record.Logs ? "a notice showed and entered the log" : "a notice showed",
            state.Tick,
            LogSubsystems.Run,
            [new LogField("notice", notice.Value)]));
    }
}
