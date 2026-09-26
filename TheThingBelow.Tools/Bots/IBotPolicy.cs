using System.Collections.Generic;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Tools.Bots;

/// <summary>A bot policy: it picks the intent of each tick from the intents that the state accepts (D-64, D-493).</summary>
public interface IBotPolicy
{
    /// <summary>The policy.</summary>
    BotPolicyKind Kind { get; }

    /// <summary>Picks the intent of the next tick.</summary>
    /// <param name="state">The state of the run.</param>
    /// <param name="accepted">The intents that the state accepts, from the query of Core (D-1179).</param>
    /// <returns>One intent of the list, or no value for a tick with no intent.</returns>
    Intent? Choose(RunState state, IReadOnlyList<Intent> accepted);
}
