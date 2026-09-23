using System;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Light;

/// <summary>
/// The effect budget: the load that the Deck test measured at 60 frames per second (D-523,
/// D-617). PR-56 writes the light row, PR-57 writes the particle row, and PR-58 writes the row of full-screen passes. PR-94 counts one pass for each fog, and PR-59 counts one pass for the glow on every map and every fight. PR-92 counts the tilt-shift blur and the vignette on every map and every fight, and the light shafts on a map with a shaft, and it raised the row to 6 from its Deck sweep (D-920, D-923). PR-60 counts the pass of its transition on every map (D-939).
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

    private EffectBudget(int lightsInView, int liveParticles, int fullScreenPasses)
    {
        this.FullScreenPasses = fullScreenPasses;
        this.LightsInView = lightsInView;
        this.LiveParticles = liveParticles;
    }

    /// <summary>The most Godot lights with shadows whose range reaches one view of the Deck, two for each source (D-842, D-853, D-854).</summary>
    public int LightsInView { get; }

    /// <summary>The most live particles on screen at once, the row of the sweep of 2026-09-17 (D-523, D-617).</summary>
    public int LiveParticles { get; }

    /// <summary>The most full-screen passes of one map or one battle, such as the fog, whose layers draw in one pass (D-523, D-617, D-898).</summary>
    public int FullScreenPasses { get; }

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
        int? particles = null;
        int? passes = null;

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
                case "live_particles":
                    particles = reader.ReadInt();
                    break;
                case "full_screen_passes":
                    passes = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        return new EffectBudget(
            Row(ref reader, depth, "lights_in_view", lights),
            Row(ref reader, depth, "live_particles", particles),
            Row(ref reader, depth, "full_screen_passes", passes));
    }

    private static int Row(ref ContentReader reader, int depth, string field, int? value)
    {
        int count = reader.RequireInt(value, depth, field);
        if (count < 1)
        {
            throw reader.RefuseField(depth, field, $"the row is {count}, and a budget row is 1 or more (D-523)");
        }

        return count;
    }
}
