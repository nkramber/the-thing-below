namespace ScreenScaleProbe;

/// <summary>
/// The mock strings of the frame. A shipped string lives in the string table (G-7), and this
/// throwaway project has none. Every line follows the game-text-style skill and its limits.
/// </summary>
public static class ProbeText
{
    /// <summary>The name above the dialogue, at the limit of 16 characters for a label.</summary>
    public const string Speaker = "MARREK";

    /// <summary>The three lines of the dialogue. At the UI scale of 2x the box holds 76 characters.</summary>
    public static string[] Dialogue { get; } =
    [
        "The shaft goes past the water line. Bergit counted the rungs. Forty-one.",
        "Nobody sealed the gate. The bar is on the inside, and it is lifted.",
        "We go down at first light, or we go down cold.",
    ];

    /// <summary>The party row of the map screen. It gives the owner the smallest real numbers.</summary>
    public const string PartyRow = "MARREK 38/38  BERGIT 31/34  DAGVAR 22/25  OTTILD 29/29  ELIO 18/26";
}
