namespace TheThingBelow.Tools.ReviewGate;

/// <summary>The result of one rule of the review gate.</summary>
public enum GateResult
{
    /// <summary>The rule read the pull request and found no fault.</summary>
    Pass,

    /// <summary>The rule found a fault, and the command gives the fault exit code.</summary>
    Fault,

    /// <summary>Another rule makes this rule inapplicable, such as the override label.</summary>
    Skip,
}

/// <summary>One rule of the review gate, with what it saw.</summary>
/// <param name="Rule">The id of the rule, from RG 1 to RG 8.</param>
/// <param name="Result">The result of the rule.</param>
/// <param name="Detail">What the rule saw, for the reader who must correct the PR (T-2).</param>
public sealed record GateCheck(string Rule, GateResult Result, string Detail)
{
    /// <summary>Gives the one output line of this rule.</summary>
    /// <returns>The rule id, the result, and the detail.</returns>
    public override string ToString() => $"{Rule} {Result.ToString().ToLowerInvariant()}: {Detail}";
}
