using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Effects;

/// <summary>
/// One layer of fog: a value noise over the world, which the fog shader of Game cuts into a few
/// bands of one palette key and drifts slowly (D-897). The layer draws above the figures (D-885).
/// </summary>
/// <remarks>
/// Each band has a noise level where it starts and its own strength, so a pixel covers the art
/// under it with the color of the key at that strength, with hard edges and no gradient (G-27,
/// D-622). All the layers of one fog draw in one full-screen pass of the effect budget, and
/// where they overlap, the strongest band wins (D-898, D-899). No rule of Core reads the noise,
/// and this record holds the values that Game gives the shader.
/// </remarks>
/// <param name="Key">The palette key of the fog (D-181).</param>
/// <param name="Bands">The bands, from the lowest noise level. Each band starts higher and draws stronger than the band before it.</param>
/// <param name="Scale">The distance between two points of the noise, in art pixels, so a large scale draws large shapes.</param>
/// <param name="CellSize">The side of each block of the fog, in art pixels. Each block takes one band.</param>
/// <param name="Seed">The seed of the noise, so two layers of one fog draw different shapes.</param>
/// <param name="DriftX">The drift of the layer to the east, in art pixels in each second of 60 ticks. A negative value drifts west.</param>
/// <param name="DriftY">The drift of the layer down the screen, in art pixels in each second. A negative value drifts up.</param>
public sealed record FogLayer(char Key, IReadOnlyList<FogBand> Bands, int Scale, int CellSize, int Seed, int DriftX, int DriftY)
{
    /// <summary>The most layers of one fog, which the shader draws in one pass (D-898).</summary>
    public const int MostLayers = 3;

    /// <summary>The most bands of one layer.</summary>
    public const int MostBands = 3;

    /// <summary>
    /// The highest strength of a band, in basis points. The contrast test of D-886 reads each
    /// fog against each enemy, so this limit only stops a fog that covers the art in full.
    /// </summary>
    public const int MostStrength = 8000;

    /// <summary>The fastest drift, in art pixels in each second.</summary>
    public const int MostDrift = 60;

    /// <summary>The largest side of a block, in art pixels.</summary>
    public const int MostCellSize = 8;

    /// <summary>The least scale of the noise, in art pixels.</summary>
    public const int LeastScale = 8;

    /// <summary>The largest scale of the noise, in art pixels: about the height of the view (D-842).</summary>
    public const int MostScale = 256;

    /// <summary>The largest seed of the noise.</summary>
    public const int MostSeed = 65535;

    /// <summary>The highest strength of the bands, which the contrast test reads (D-886, D-899).</summary>
    public int Strongest
    {
        get
        {
            int most = 0;
            foreach (FogBand band in this.Bands)
            {
                most = Math.Max(most, band.Strength);
            }

            return most;
        }
    }

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
        List<FogBand>? bands = null;
        int? scale = null;
        int? cellSize = null;
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
                case "bands":
                    bands = ReadBands(ref reader);
                    break;
                case "scale":
                    scale = reader.ReadInt();
                    break;
                case "cell_size":
                    cellSize = reader.ReadInt();
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

        List<FogBand> read = reader.Require(bands, depth, "bands");
        RefuseWrongBands(ref reader, depth, read);
        return new FogLayer(
            color[0],
            read,
            ScaleOf(ref reader, depth, scale),
            Within(ref reader, depth, "cell_size", cellSize, 1, MostCellSize),
            Within(ref reader, depth, "seed", seed, 0, MostSeed),
            Within(ref reader, depth, "drift_x", driftX, -MostDrift, MostDrift),
            Within(ref reader, depth, "drift_y", driftY, -MostDrift, MostDrift));
    }

    private static List<FogBand> ReadBands(ref ContentReader reader)
    {
        var bands = new List<FogBand>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, bands.Count))
        {
            bands.Add(ReadBand(ref reader));
        }

        return bands;
    }

    private static FogBand ReadBand(ref ContentReader reader)
    {
        int? from = null;
        int? strength = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "from":
                    from = reader.ReadInt();
                    break;
                case "strength":
                    strength = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new FogBand(
            Within(ref reader, depth, "from", from, 1, BasisPoints.One - 1),
            Within(ref reader, depth, "strength", strength, 1, MostStrength));
    }

    /// <summary>
    /// Refuses a list of the wrong length, and a band that starts no higher or draws no stronger
    /// than the band before it. Each band then lies inside the band before it, and the highest
    /// band that a pixel reaches is its strongest (D-899, T-2).
    /// </summary>
    private static void RefuseWrongBands(ref ContentReader reader, int depth, List<FogBand> bands)
    {
        if (bands.Count < 1 || bands.Count > MostBands)
        {
            throw reader.RefuseField(depth, "bands", $"the layer holds {bands.Count} bands, and it takes 1 to {MostBands}");
        }

        for (int index = 1; index < bands.Count; index += 1)
        {
            FogBand lower = bands[index - 1];
            FogBand band = bands[index];
            if (band.From <= lower.From || band.Strength <= lower.Strength)
            {
                throw reader.RefuseField(
                    depth,
                    "bands",
                    $"the band {index} starts at {band.From} with the strength {band.Strength}, and each band starts higher and draws stronger than the band before it, "
                    + $"which starts at {lower.From} with the strength {lower.Strength} (D-899)");
            }
        }
    }

    /// <summary>Refuses a scale outside its limits, and an odd scale, because the second octave of the noise takes half the scale (D-897).</summary>
    private static int ScaleOf(ref ContentReader reader, int depth, int? value)
    {
        int read = Within(ref reader, depth, "scale", value, LeastScale, MostScale);
        if (read % 2 != 0)
        {
            throw reader.RefuseField(depth, "scale", $"the scale is {read}, and it takes an even value, because the second octave of the noise takes half of it");
        }

        return read;
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
