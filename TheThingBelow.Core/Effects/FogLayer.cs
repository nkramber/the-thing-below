using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Effects;

/// <summary>
/// One layer of fog: a text grid of a few shapes in one palette key, which Game tiles over
/// the view and drifts slowly (D-887). The layer draws above the figures (D-885).
/// </summary>
/// <remarks>
/// A cell of the grid holds <see cref="Clear"/> or the number of a band, from 1. Each band has
/// its own strength, so a cell covers the art under it with the color of the key at that
/// strength, with hard edges and no gradient (G-27, D-622). Each layer counts as one
/// full-screen pass of the effect budget (D-523).
/// </remarks>
/// <param name="Key">The palette key of the fog (D-181).</param>
/// <param name="Bands">The strength of each band, in basis points, where 10000 covers the art in full (D-169).</param>
/// <param name="DriftX">The drift of the layer to the east, in art pixels in each second of 60 ticks. A negative value drifts west.</param>
/// <param name="DriftY">The drift of the layer down the screen, in art pixels in each second. A negative value drifts up.</param>
/// <param name="CellSize">The side of each cell of the grid, in art pixels, so a small grid draws large blocks.</param>
/// <param name="Rows">The rows of the grid, from the north. Every row has the same length.</param>
public sealed record FogLayer(char Key, IReadOnlyList<int> Bands, int DriftX, int DriftY, int CellSize, IReadOnlyList<string> Rows)
{
    /// <summary>The character of a cell that holds no fog.</summary>
    public const char Clear = '.';

    /// <summary>The most bands of one layer.</summary>
    public const int MostBands = 3;

    /// <summary>
    /// The highest strength of a band, in basis points. The contrast test of D-886 reads each
    /// fog against each enemy, so this limit only stops a fog that covers the art in full.
    /// </summary>
    public const int MostStrength = 8000;

    /// <summary>The fastest drift, in art pixels in each second.</summary>
    public const int MostDrift = 60;

    /// <summary>The largest side of a cell, in art pixels.</summary>
    public const int MostCellSize = 8;

    /// <summary>The least side of the grid, in cells.</summary>
    public const int LeastSide = 8;

    /// <summary>The largest side of the grid, in cells: the width of the view (D-842).</summary>
    public const int MostSide = 640;

    /// <summary>The width of the grid, in cells.</summary>
    public int Width => this.Rows[0].Length;

    /// <summary>The height of the grid, in cells.</summary>
    public int Height => this.Rows.Count;

    /// <summary>The highest strength of the bands, which the contrast test reads (D-886).</summary>
    public int Strongest
    {
        get
        {
            int most = 0;
            foreach (int band in this.Bands)
            {
                most = Math.Max(most, band);
            }

            return most;
        }
    }

    /// <summary>Gives the band of one cell, or zero for a clear cell.</summary>
    /// <param name="column">The column, from the west.</param>
    /// <param name="row">The row, from the north.</param>
    /// <returns>The number of the band, from 1, or zero.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The cell is outside the grid (T-2).</exception>
    public int BandAt(int column, int row)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(column);
        ArgumentOutOfRangeException.ThrowIfNegative(row);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, this.Width);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, this.Height);

        char cell = this.Rows[row][column];
        return cell == Clear ? 0 : cell - '0';
    }

    /// <summary>Reads a list of layers: the value of a `fogs` field.</summary>
    /// <param name="reader">The reader, at the start of the array.</param>
    /// <returns>The layers, in the order of the file. The list can be empty.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or outside its limits (G-6, T-2).</exception>
    public static List<FogLayer> ReadList(ref ContentReader reader)
    {
        var layers = new List<FogLayer>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, layers.Count))
        {
            layers.Add(Read(ref reader));
        }

        return layers;
    }

    private static FogLayer Read(ref ContentReader reader)
    {
        string? key = null;
        List<int>? bands = null;
        int? driftX = null;
        int? driftY = null;
        int? cellSize = null;
        List<string>? rows = null;

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
                case "drift_x":
                    driftX = reader.ReadInt();
                    break;
                case "drift_y":
                    driftY = reader.ReadInt();
                    break;
                case "cell_size":
                    cellSize = reader.ReadInt();
                    break;
                case "rows":
                    rows = ReadRows(ref reader);
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

        List<int> strengths = reader.Require(bands, depth, "bands");
        if (strengths.Count < 1 || strengths.Count > MostBands)
        {
            throw reader.RefuseField(depth, "bands", $"the layer holds {strengths.Count} bands, and it takes 1 to {MostBands}");
        }

        List<string> grid = reader.Require(rows, depth, "rows");
        RefuseWrongGrid(ref reader, depth, grid, strengths.Count);
        return new FogLayer(
            color[0],
            strengths,
            Drift(ref reader, depth, "drift_x", driftX),
            Drift(ref reader, depth, "drift_y", driftY),
            CellSizeOf(ref reader, depth, cellSize),
            grid);
    }

    private static List<int> ReadBands(ref ContentReader reader)
    {
        var bands = new List<int>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, bands.Count))
        {
            int strength = reader.ReadInt();
            if (strength < 1 || strength > MostStrength)
            {
                throw reader.Refuse($"the strength is {strength}, and a band takes 1 to {MostStrength} basis points (D-885)");
            }

            bands.Add(strength);
        }

        return bands;
    }

    private static List<string> ReadRows(ref ContentReader reader)
    {
        var rows = new List<string>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, rows.Count))
        {
            rows.Add(reader.ReadString());
        }

        return rows;
    }

    /// <summary>Refuses a grid of the wrong size, a row of another length, and a cell that names no band (T-2).</summary>
    private static void RefuseWrongGrid(ref ContentReader reader, int depth, List<string> rows, int bands)
    {
        if (rows.Count < LeastSide || rows.Count > MostSide)
        {
            throw reader.RefuseField(depth, "rows", $"the grid holds {rows.Count} rows, and it takes {LeastSide} to {MostSide}");
        }

        int width = rows[0].Length;
        if (width < LeastSide || width > MostSide)
        {
            throw reader.RefuseField(depth, "rows", $"the first row holds {width} cells, and a row takes {LeastSide} to {MostSide}");
        }

        for (int row = 0; row < rows.Count; row += 1)
        {
            if (rows[row].Length != width)
            {
                throw reader.RefuseField(depth, "rows", $"the row {row} holds {rows[row].Length} cells, and the first row holds {width}");
            }

            for (int column = 0; column < width; column += 1)
            {
                char cell = rows[row][column];
                bool band = cell >= '1' && cell <= (char)('0' + bands);
                if (cell != Clear && !band)
                {
                    throw reader.RefuseField(
                        depth,
                        "rows",
                        $"the cell ({column}, {row}) holds '{cell}', and a cell holds '{Clear}' or a band from 1 to {bands}");
                }
            }
        }
    }

    private static int CellSizeOf(ref ContentReader reader, int depth, int? value)
    {
        int read = reader.RequireInt(value, depth, "cell_size");
        if (read < 1 || read > MostCellSize)
        {
            throw reader.RefuseField(depth, "cell_size", $"the cell is {read} pixels, and it takes 1 to {MostCellSize}");
        }

        return read;
    }

    private static int Drift(ref ContentReader reader, int depth, string field, int? value)
    {
        int read = reader.RequireInt(value, depth, field);
        if (read < -MostDrift || read > MostDrift)
        {
            throw reader.RefuseField(depth, field, $"the drift is {read}, and it takes {-MostDrift} to {MostDrift}");
        }

        return read;
    }
}
