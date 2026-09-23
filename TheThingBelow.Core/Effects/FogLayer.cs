using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Effects;

/// <summary>
/// One layer of fog: a fractal noise over the world, which the fog shader of Game fades from
/// clear to the strength of the layer, in one palette key, and drifts slowly (D-897, D-900).
/// The layer draws above the figures (D-885).
/// </summary>
/// <remarks>
/// Below the noise level <see cref="From"/> the layer is clear, and at <see cref="To"/> and above
/// it takes its full strength. Between the two it fades smoothly, at the pixel size of the art
/// (D-900, D-901). All the layers of one fog draw in one full-screen pass of the effect budget,
/// and where they overlap, the strongest layer wins (D-898, D-899). No rule of Core reads the
/// noise, and this record holds the values that Game gives the shader.
/// </remarks>
/// <param name="Key">The palette key of the fog (D-181).</param>
/// <param name="From">The noise level where the layer starts to show, in basis points of the range of the noise (D-169).</param>
/// <param name="To">The noise level where the layer reaches its full strength, in basis points. It is above <see cref="From"/>.</param>
/// <param name="Strength">The full strength of the layer, in basis points, where 10000 covers the art in full (D-169).</param>
/// <param name="Scale">The size of the largest shapes of the noise, in art pixels.</param>
/// <param name="Seed">The seed of the noise, so two layers of one fog draw different shapes.</param>
/// <param name="DriftX">The drift of the layer to the east, in art pixels in each second of 60 ticks. A negative value drifts west.</param>
/// <param name="DriftY">The drift of the layer down the screen, in art pixels in each second. A negative value drifts up.</param>
public sealed record FogLayer(char Key, int From, int To, int Strength, int Scale, int Seed, int DriftX, int DriftY)
{
    /// <summary>The most layers of one fog, which the shader draws in one pass (D-898).</summary>
    public const int MostLayers = 3;

    /// <summary>
    /// The highest strength of a layer, in basis points. The contrast test of D-886 reads each
    /// fog against each enemy, so this limit only stops a fog that covers the art in full.
    /// </summary>
    public const int MostStrength = 8000;

    /// <summary>The fastest drift, in art pixels in each second.</summary>
    public const int MostDrift = 60;

    /// <summary>The least scale of the noise, in art pixels.</summary>
    public const int LeastScale = 8;

    /// <summary>The largest scale of the noise, in art pixels: about the height of the view (D-842).</summary>
    public const int MostScale = 256;

    /// <summary>The largest seed of the noise.</summary>
    public const int MostSeed = 65535;

    /// <summary>Reads a list of layers: the value of a `fogs` field.</summary>
    /// <param name="reader">The reader, at the start of the array.</param>
    /// <returns>The layers, in the order of the file. The list can be empty.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or outside its limits, or the list holds too many layers (G-6, T-2).</exception>
    public static List<FogLayer> ReadList(ref ContentReader reader)
    {
        var layers = new List<FogLayer>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, layers.Count))
        {
            if (layers.Count == MostLayers)
            {
                throw reader.Refuse($"the fog holds more than {MostLayers} layers, and the shader draws 1 to {MostLayers} in one pass (D-898)");
            }

            layers.Add(Read(ref reader));
        }

        return layers;
    }

    private static FogLayer Read(ref ContentReader reader)
    {
        string? key = null;
        int? from = null;
        int? to = null;
        int? strength = null;
        int? scale = null;
        int? seed = null;
        int? driftX = null;
        int? driftY = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "key":
                    key = reader.ReadString();
                    break;
                case "from":
                    from = reader.ReadInt();
                    break;
                case "to":
                    to = reader.ReadInt();
                    break;
                case "strength":
                    strength = reader.ReadInt();
                    break;
                case "scale":
                    scale = reader.ReadInt();
                    break;
                case "seed":
                    seed = reader.ReadInt();
                    break;
                case "drift_x":
                    driftX = reader.ReadInt();
                    break;
                case "drift_y":
                    driftY = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        string color = reader.Require(key, depth, "key");
        if (color.Length != 1)
        {
            throw reader.RefuseField(depth, "key", $"the key is '{color}', and a fog names one palette key of one character (D-181)");
        }

        int start = Within(ref reader, depth, "from", from, 0, BasisPoints.One - 1);
        int full = Within(ref reader, depth, "to", to, 1, BasisPoints.One);
        if (full <= start)
        {
            throw reader.RefuseField(depth, "to", $"the fog is full at {full} and starts at {start}, and a layer is full above the level where it starts (D-900)");
        }

        return new FogLayer(
            color[0],
            start,
            full,
            Within(ref reader, depth, "strength", strength, 1, MostStrength),
            Within(ref reader, depth, "scale", scale, LeastScale, MostScale),
            Within(ref reader, depth, "seed", seed, 0, MostSeed),
            Within(ref reader, depth, "drift_x", driftX, -MostDrift, MostDrift),
            Within(ref reader, depth, "drift_y", driftY, -MostDrift, MostDrift));
    }

    private static int Within(ref ContentReader reader, int depth, string field, int? value, int least, int most)
    {
        int read = reader.RequireInt(value, depth, field);
        if (read < least || read > most)
        {
            throw reader.RefuseField(depth, field, $"the value is {read}, and it takes {least} to {most}");
        }

        return read;
    }
}
