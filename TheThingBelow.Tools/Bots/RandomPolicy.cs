using System;
using System.Collections.Generic;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Tools.Bots;

/// <summary>
/// The random policy: on each tick it picks one kind of accepted intent, then one intent of that
/// kind, both at random (D-64).
/// </summary>
/// <remarks>
/// The pick of the kind comes first, so the many counts of a buy of a shop weigh no more than one
/// step of the walk.
/// </remarks>
public sealed class RandomPolicy : IBotPolicy
{
    private readonly BotDice dice;

    /// <summary>Makes the policy of one run.</summary>
    /// <param name="seed">The seed of the run.</param>
    public RandomPolicy(ulong seed)
    {
        this.dice = new BotDice(seed, BotPolicyKind.Random);
    }

    /// <inheritdoc/>
    public BotPolicyKind Kind => BotPolicyKind.Random;

    /// <inheritdoc/>
    public Intent? Choose(RunState state, IReadOnlyList<Intent> accepted)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(accepted);

        if (accepted.Count == 0)
        {
            return null;
        }

        List<string> kinds = [];
        foreach (Intent intent in accepted)
        {
            if (!kinds.Contains(intent.Action.Value))
            {
                kinds.Add(intent.Action.Value);
            }
        }

        string kind = kinds[this.dice.Pick(kinds.Count)];
        List<Intent> ofKind = [];
        foreach (Intent intent in accepted)
        {
            if (string.CompareOrdinal(intent.Action.Value, kind) == 0)
            {
                ofKind.Add(intent);
            }
        }

        return ofKind[this.dice.Pick(ofKind.Count)];
    }
}
