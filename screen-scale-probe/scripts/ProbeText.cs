using System.Collections.Generic;

namespace ScreenScaleProbe;

/// <summary>
/// The mock strings of the frame. A shipped string lives in the string table (G-7), and this
/// throwaway project has none. Every line follows the game-text-style skill and its limits.
/// </summary>
public static class ProbeText
{
    /// <summary>The place name that the title font draws. It is 17 characters, and the
    /// narrowest combination holds 25 across the frame.</summary>
    public const string Title = "THE HANGING CELLS";

    /// <summary>The name above the dialogue, at the limit of 16 characters for a label.</summary>
    public const string Speaker = "MARREK";

    /// <summary>The three lines of the dialogue. At the UI scale of 2x the box holds 76 characters.</summary>
    public static string[] Dialogue { get; } =
    [
        "The shaft goes past the water line. Bergit counted the rungs. Forty-one.",
        "Nobody sealed the gate. The bar is on the inside, and it is lifted.",
        "We go down at first light, or we go down cold.",
    ];

    /// <summary>
    /// The party of the map screen, one member each. The row holds the members that fit the
    /// frame at the body size of the combination, so a larger body shows fewer of them.
    /// </summary>
    public static string[] PartyMembers { get; } =
    [
        "MARREK 38/38",
        "BERGIT 31/34",
        "DAGVAR 22/25",
        "OTTILD 29/29",
        "ELIO 18/26",
    ];

    /// <summary>The party row that fits a count of characters, with two spaces between members.</summary>
    public static string PartyRow(int columns)
    {
        var row = new System.Text.StringBuilder();
        foreach (string member in PartyMembers)
        {
            int length = row.Length == 0 ? member.Length : row.Length + 2 + member.Length;
            if (length > columns)
            {
                break;
            }

            if (row.Length > 0)
            {
                row.Append("  ");
            }

            row.Append(member);
        }

        return row.ToString();
    }

    /// <summary>The line limit of a dialogue box in the game-text-style skill (D-635).</summary>
    public const int DialogueLines = 3;

    /// <summary>The dialogue, broken at a space so that no line passes a count of characters.</summary>
    public static List<string> WrappedDialogue(int columns)
    {
        var lines = new List<string>();
        foreach (string paragraph in Dialogue)
        {
            string rest = paragraph;
            while (rest.Length > columns)
            {
                int cut = rest.LastIndexOf(' ', columns);
                if (cut <= 0)
                {
                    cut = columns;
                }

                lines.Add(rest[..cut]);
                rest = rest[cut..].TrimStart();
            }

            lines.Add(rest);
        }

        return lines;
    }
}
