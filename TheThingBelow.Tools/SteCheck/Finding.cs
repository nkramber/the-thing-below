namespace TheThingBelow.Tools.SteCheck;

/// <summary>One rule violation, in one document, on one line.</summary>
/// <param name="File">The path of the document, relative to the root of the checkout.</param>
/// <param name="Line">The 1-based number of the line that holds the violation.</param>
/// <param name="Rule">The id of the rule, as the `ste-writing` skill names it.</param>
/// <param name="Detail">What the rule saw, for the reader who must correct the line (T-2).</param>
public sealed record Finding(string File, int Line, string Rule, string Detail)
{
    /// <summary>Gives the one output line of this finding.</summary>
    /// <returns>The path, the line number, the rule id, and the detail.</returns>
    public override string ToString() => $"{File}:{Line}: rule {Rule}: {Detail}";
}
