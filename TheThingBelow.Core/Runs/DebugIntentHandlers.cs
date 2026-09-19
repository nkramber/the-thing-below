using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Runs;

/// <summary>
/// One handler of a debug intent. The debug assembly of a development build writes each
/// handler, and Core names that assembly nowhere (D-260, D-492).
/// </summary>
/// <param name="state">The state of the run, which the command changes.</param>
/// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
public delegate void DebugIntentHandler(RunState state, RunContext context);

/// <summary>
/// The extra intent handlers that the host gives to a run at its start. A development build
/// passes the handlers of the debug console, and a release build passes
/// <see cref="None"/> (D-171, D-260, D-492).
/// </summary>
/// <remarks>
/// Core holds no debug command and no conditional compilation, which F-27 asked for. A
/// record of a run with a debug intent replays on a development host, and it fails on a
/// release host with a report that names the intent and the tick (T-2).
/// <para>
/// PR-45 creates the debug assembly and the console. The capture of PR-74 and the sound room
/// of PR-71 pass their handlers through this same seam (D-439, D-546, D-551).
/// </para>
/// </remarks>
public sealed class DebugIntentHandlers
{
    /// <summary>The set that a release build passes: no handler at all (D-260).</summary>
    public static readonly DebugIntentHandlers None = new([]);

    // The key is the text of the id, and the order is ordinal, so every machine walks the
    // set in one order (F-39, G-4). A lookup by key stays legal in Core (D-615).
    private readonly SortedDictionary<string, DebugIntentHandler> handlers;

    /// <summary>Makes the set from the handlers of the host.</summary>
    /// <param name="handlers">One pair for each debug intent, with no repeated id.</param>
    /// <exception cref="ArgumentNullException">The list or one handler is null (T-2).</exception>
    /// <exception cref="ArgumentException">Two pairs carry the same id (T-2).</exception>
    public DebugIntentHandlers(IReadOnlyList<KeyValuePair<ContentId, DebugIntentHandler>> handlers)
    {
        ArgumentNullException.ThrowIfNull(handlers);

        this.handlers = new SortedDictionary<string, DebugIntentHandler>(StringComparer.Ordinal);
        foreach (KeyValuePair<ContentId, DebugIntentHandler> pair in handlers)
        {
            ArgumentNullException.ThrowIfNull(pair.Key);
            ArgumentNullException.ThrowIfNull(pair.Value);

            if (!this.handlers.TryAdd(pair.Key.Value, pair.Value))
            {
                throw new ArgumentException(
                    $"The host passed two handlers for the debug intent '{pair.Key.Value}'.",
                    nameof(handlers));
            }
        }
    }

    /// <summary>The count of handlers that the host passed.</summary>
    public int Count => this.handlers.Count;

    /// <summary>Finds the handler of one debug intent.</summary>
    /// <param name="action">The id of the debug intent.</param>
    /// <param name="handler">The handler, or null when the host passed none.</param>
    /// <returns>True when the host passed a handler for that id.</returns>
    /// <exception cref="ArgumentNullException">The action is null (T-2).</exception>
    public bool TryFind(ContentId action, out DebugIntentHandler? handler)
    {
        ArgumentNullException.ThrowIfNull(action);

        return this.handlers.TryGetValue(action.Value, out handler);
    }
}
