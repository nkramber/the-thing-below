using System;

namespace TheThingBelow.Tools.Bots;

/// <summary>The two policies of the bots of PR-15 (D-64, D-1183).</summary>
public enum BotPolicyKind
{
    /// <summary>Picks a kind of accepted intent at random, then one intent of that kind.</summary>
    Random = 1,

    /// <summary>Walks to the nearest target that it did not reach, and wins each battle fast (D-1183).</summary>
    Greedy = 2,
}

/// <summary>The names of the policies, as the command line and the result lines write them.</summary>
public static class BotPolicyKinds
{
    /// <summary>Gives the name of a policy.</summary>
    /// <param name="kind">The policy.</param>
    /// <returns>The name, such as `random`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no policy (T-2).</exception>
    public static string NameOf(BotPolicyKind kind) => kind switch
    {
        BotPolicyKind.Random => "random",
        BotPolicyKind.Greedy => "greedy",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no bot policy (D-64)"),
    };

    /// <summary>Reads the name of a policy.</summary>
    /// <param name="name">The name.</param>
    /// <param name="kind">The policy, when the name is known.</param>
    /// <returns>True when the name names a policy.</returns>
    public static bool TryOf(string name, out BotPolicyKind kind)
    {
        foreach (BotPolicyKind known in new[] { BotPolicyKind.Random, BotPolicyKind.Greedy })
        {
            if (string.CompareOrdinal(name, NameOf(known)) == 0)
            {
                kind = known;
                return true;
            }
        }

        kind = BotPolicyKind.Random;
        return false;
    }
}
