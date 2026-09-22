using System;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Light;

/// <summary>
/// The effect budget: the load that the Deck test measured at 60 frames per second (D-523,
/// D-617). PR-56 writes the light row, and PR-57 to PR-60 and PR-92 add their rows.
/// </summary>
/// <remarks>
/// Each row is a floor, and not the limit of the Deck, because no stage of the sweep missed the
/// target (F-66). A change of a row cites a Deck measurement before and after it (G-14).
/// </remarks>
public sealed class EffectBudget
{
    /// <summary>The path of the file, under `content/`.</summary>
    public const string Path = "effects/budget.json";

    /// <summary>
    /// The most lights that Godot draws on one canvas item. The loop of the canvas renderer
    /// stops at `MAX_LIGHTS_PER_ITEM - 1`, with no message (F-46).
    /// </summary>
    public const int GodotLightsPerItem = 15;

    /// <summary>
    /// The Godot lights of each light source of a map: one for the floor and the walls, and one
    /// for the figures, so no figure darkens itself (D-853).
    /// </summary>
    public const int LightsPerSource = 2;

    private EffectBudget(int lightsInView)
    {
        this.LightsInView = lightsInView;
    }

    /// <summary>The most Godot lights with shadows whose range reaches one view of the Deck, two for each source (D-842, D-853, D-854).</summary>
    public int LightsInView { get; }

    /// <summary>Reads the budget from the bytes of its file.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The budget.</returns>
    /// <exception cref="ContentException">The file breaks a rule of the reader (G-6, T-2).</exception>
    public static EffectBudget Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        EffectBudget budget = Read(ref reader);
        reader.ReadFileEnd();
        return budget;
    }

    private static EffectBudget Read(ref ContentReader reader)
    {
        string? comment = null;
        int? lights = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "lights_in_view":
                    lights = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        int count = reader.RequireInt(lights, depth, "lights_in_view");
        if (count < 1)
        {
            throw reader.RefuseField(depth, "lights_in_view", $"the row is {count}, and a budget row is 1 or more (D-523)");
        }

        return new EffectBudget(count);
    }
}
