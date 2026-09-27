using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TheThingBelow.Tools.Night;

namespace TheThingBelow.Tools.Watch;

/// <summary>The newest completed night of `main`, as the watcher reads it with `gh`.</summary>
/// <param name="Id">The id of the night run.</param>
/// <param name="Commit">The full hash of the commit of the night.</param>
/// <param name="Conclusion">The conclusion of the run, such as `success` or `failure`.</param>
/// <param name="Link">The link of the page of the run.</param>
public sealed record WatchedNight(long Id, string Commit, string Conclusion, string Link);

/// <summary>What one check of the watcher does: nothing, with the reason, or a new fix session for a night.</summary>
/// <param name="Night">The failed night that gets a fix session, or null for no session.</param>
/// <param name="Reason">One line that names the reason, for the log.</param>
public sealed record WatchStep(WatchedNight? Night, string Reason);

/// <summary>
/// The rules of the night watcher (D-1205). A failed night of `main` gets one fix session, and
/// never a second one. A night that a newer promotion covers gets none, because `main` is green
/// again (D-1202).
/// </summary>
public static class NightWatch
{
    /// <summary>The name of the folder of the handled nights, inside the folder of the watcher.</summary>
    public const string HandledFolder = "handled";

    /// <summary>The name of the folder of the worktrees of the fix sessions.</summary>
    public const string WorktreeFolder = "worktrees";

    /// <summary>The name of the folder of the logs of the fix sessions.</summary>
    public const string LogFolder = "logs";

    /// <summary>The name of the log of the checks.</summary>
    public const string LogFile = "night-watch.log";

    /// <summary>Gives the step of one check.</summary>
    /// <param name="night">The newest completed night of `main`, or null when none exists.</param>
    /// <param name="handled">The id of each night that got a fix session before.</param>
    /// <param name="promotionOrder">
    /// The order of the merge commit of the newest promotion against the commit of the night, or
    /// <see cref="NightOrder.None"/> when no promotion exists.
    /// </param>
    /// <returns>The step.</returns>
    public static WatchStep Decide(WatchedNight? night, SortedSet<long> handled, NightOrder promotionOrder)
    {
        ArgumentNullException.ThrowIfNull(handled);

        if (night is null)
        {
            return new WatchStep(null, "No completed night exists on main.");
        }

        string name = $"The night run {Text(night.Id)} on {Short(night.Commit)}";
        if (string.Equals(night.Conclusion, NightGate.SuccessConclusion, StringComparison.Ordinal))
        {
            return new WatchStep(null, $"{name} succeeded.");
        }

        if (handled.Contains(night.Id))
        {
            return new WatchStep(null, $"{name} ended as '{night.Conclusion}', and it got a fix session before.");
        }

        if (promotionOrder == NightOrder.Ahead)
        {
            return new WatchStep(null, $"{name} ended as '{night.Conclusion}', and a newer promotion covers it (D-1202).");
        }

        return new WatchStep(night, $"{name} ended as '{night.Conclusion}', so the watcher starts a fix session (D-1205).");
    }

    /// <summary>Gives the prompt of a fix session.</summary>
    /// <param name="night">The failed night.</param>
    /// <param name="sessionId">The id of the session, for a resume by the owner.</param>
    /// <param name="worktree">The folder of the worktree of the session.</param>
    /// <returns>The prompt.</returns>
    public static string PromptOf(WatchedNight night, Guid sessionId, string worktree)
    {
        ArgumentNullException.ThrowIfNull(night);
        ArgumentException.ThrowIfNullOrEmpty(worktree);
        return $"""
            Load the `night-fix` skill from `.claude/skills/night-fix/SKILL.md`, and follow it to its end.
            The night watcher started this session for a failed night of main (D-1205).
            Failed night: run {Text(night.Id)}, commit {night.Commit}, conclusion '{night.Conclusion}'.
            Run link: {night.Link}
            Session id: {sessionId:D}
            Worktree: {worktree}
            """;
    }

    /// <summary>Reads the id of each handled night from the folder of the watcher.</summary>
    /// <param name="state">The folder of the watcher.</param>
    /// <returns>The ids, in the order of the ids.</returns>
    /// <exception cref="InvalidOperationException">A file of the folder has a name that is no id (T-2).</exception>
    public static SortedSet<long> ReadHandled(string state)
    {
        ArgumentException.ThrowIfNullOrEmpty(state);

        SortedSet<long> handled = [];
        string folder = Path.Combine(state, HandledFolder);
        if (!Directory.Exists(folder))
        {
            return handled;
        }

        foreach (string path in Directory.GetFiles(folder))
        {
            string name = Path.GetFileName(path);
            if (!long.TryParse(name, NumberStyles.None, CultureInfo.InvariantCulture, out long id))
            {
                throw new InvalidOperationException($"The file '{path}' of the handled nights has a name that is no run id. Move it out of the folder (T-2).");
            }

            handled.Add(id);
        }

        return handled;
    }

    /// <summary>Marks a night as handled before its session starts, so a stop of the session never starts a second one.</summary>
    /// <param name="state">The folder of the watcher.</param>
    /// <param name="night">The night.</param>
    /// <param name="sessionId">The id of the session.</param>
    /// <param name="now">The time of the mark, in UTC.</param>
    public static void MarkHandled(string state, WatchedNight night, Guid sessionId, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrEmpty(state);
        ArgumentNullException.ThrowIfNull(night);

        string folder = Path.Combine(state, HandledFolder);
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, Text(night.Id)), $"{NightJson.TextOf(now)} session {sessionId:D} {night.Link}\n");
    }

    /// <summary>Removes the mark of a night, so the next check starts its session again. The watcher calls it after a fault before the session starts.</summary>
    /// <param name="state">The folder of the watcher.</param>
    /// <param name="night">The night.</param>
    public static void RemoveMark(string state, WatchedNight night)
    {
        ArgumentException.ThrowIfNullOrEmpty(state);
        ArgumentNullException.ThrowIfNull(night);
        File.Delete(Path.Combine(state, HandledFolder, Text(night.Id)));
    }

    private static string Short(string commit) => commit.Length > 12 ? commit[..12] : commit;

    private static string Text(long value) => value.ToString(CultureInfo.InvariantCulture);
}
