using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Hashing;

namespace TheThingBelow.Core.Maps;

/// <summary>
/// Every tile of one map that the party walked (D-567). The snapshot holds this record, and
/// the dungeon map screen of PR-62 draws those tiles.
/// </summary>
/// <remarks>
/// D-566 removed the fog of war, so the ground of a map is visible from the moment the party
/// enters. This record answers another question: where the party went. A tile that the record
/// holds is never forgotten, because no rule clears one (D-567).
/// <para>
/// The record writes as one string for each row, as a terrain row and a drawing row do
/// (D-165, D-515). Thus a snapshot reads as a picture of the walk.
/// </para>
/// </remarks>
public sealed class WalkedTiles
{
    /// <summary>The character of a tile that the party walked.</summary>
    public const char WalkedCharacter = 'x';

    /// <summary>The character of a tile that the party never walked.</summary>
    public const char FreshCharacter = '.';

    private readonly bool[] walked;

    private WalkedTiles(int width, int height, bool[] walked, int count)
    {
        this.Width = width;
        this.Height = height;
        this.walked = walked;
        this.Count = count;
    }

    /// <summary>The count of tiles from the west edge to the east edge of the map.</summary>
    public int Width { get; }

    /// <summary>The count of tiles from the north edge to the south edge of the map.</summary>
    public int Height { get; }

    /// <summary>The count of tiles that the party walked.</summary>
    public int Count { get; private set; }

    /// <summary>Makes an empty record for one map.</summary>
    /// <param name="width">The width of the map, in tiles.</param>
    /// <param name="height">The height of the map, in tiles.</param>
    /// <returns>The record, with no walked tile.</returns>
    /// <exception cref="ArgumentOutOfRangeException">A side is below one, or above the limit of a map (T-2).</exception>
    public static WalkedTiles Empty(int width, int height)
    {
        RefuseSide(width, nameof(width));
        RefuseSide(height, nameof(height));

        return new WalkedTiles(width, height, new bool[width * height], 0);
    }

    /// <summary>Reads a record from the rows that a snapshot holds (D-567).</summary>
    /// <param name="rows">One string for each row of the map, as <see cref="Rows"/> wrote them.</param>
    /// <param name="source">What the rows came from, such as `the save`, for an error (T-2).</param>
    /// <returns>The record.</returns>
    /// <exception cref="ArgumentNullException">The list or a row is null (T-2).</exception>
    /// <exception cref="ArgumentException">A row is empty, a row has another length, or a character names no state (T-2).</exception>
    public static WalkedTiles OfRows(IReadOnlyList<string> rows, string source)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentException.ThrowIfNullOrEmpty(source);

        if (rows.Count == 0)
        {
            throw new ArgumentException($"The walked tiles of {source} hold no row, and a map holds at least one (T-2, D-567).", nameof(rows));
        }

        ArgumentNullException.ThrowIfNull(rows[0]);
        int width = rows[0].Length;
        int height = rows.Count;
        RefuseSide(width, nameof(rows));
        RefuseSide(height, nameof(rows));

        var walked = new bool[width * height];
        int count = 0;
        for (int row = 0; row < height; row += 1)
        {
            string line = rows[row];
            ArgumentNullException.ThrowIfNull(line);
            if (line.Length != width)
            {
                throw new ArgumentException(
                    $"The walked tiles of {source} hold {line.Length} characters in row {row}, and {width} in row 0 (T-2, D-567).",
                    nameof(rows));
            }

            for (int column = 0; column < width; column += 1)
            {
                switch (line[column])
                {
                    case WalkedCharacter:
                        walked[(row * width) + column] = true;
                        count += 1;
                        break;
                    case FreshCharacter:
                        break;
                    default:
                        throw new ArgumentException(
                            $"The walked tiles of {source} hold the character '{line[column]}' at ({column}, {row}), and a row takes '{WalkedCharacter}' or '{FreshCharacter}' (T-2, D-567).",
                            nameof(rows));
                }
            }
        }

        return new WalkedTiles(width, height, walked, count);
    }

    /// <summary>
    /// Gives a copy of the record on a map of another size (D-1111). Each tile inside both
    /// sizes keeps its state, a tile of the new size alone is fresh, and a tile of the old size
    /// alone leaves.
    /// </summary>
    /// <param name="width">The width of the map of this build, in tiles.</param>
    /// <param name="height">The height of the map of this build, in tiles.</param>
    /// <returns>The copy.</returns>
    /// <exception cref="ArgumentOutOfRangeException">A side is below one, or above the limit of a map (T-2).</exception>
    public WalkedTiles Resized(int width, int height)
    {
        WalkedTiles copy = Empty(width, height);
        for (int row = 0; row < Math.Min(height, this.Height); row += 1)
        {
            for (int column = 0; column < Math.Min(width, this.Width); column += 1)
            {
                var at = new TilePoint(column, row);
                if (this.WasWalked(at))
                {
                    copy.Mark(at);
                }
            }
        }

        return copy;
    }

    /// <summary>Tells whether the party walked one tile.</summary>
    /// <param name="at">The tile.</param>
    /// <returns>True when the record holds that tile.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The tile lies outside the map (T-2).</exception>
    public bool WasWalked(TilePoint at)
    {
        this.RefuseOutside(at);

        return this.walked[(at.Y * this.Width) + at.X];
    }

    /// <summary>Records that the party walked one tile (D-567).</summary>
    /// <param name="at">The tile that the party stands on.</param>
    /// <exception cref="ArgumentOutOfRangeException">The tile lies outside the map (T-2).</exception>
    /// <remarks>A second mark of one tile changes nothing, because no rule forgets a tile.</remarks>
    public void Mark(TilePoint at)
    {
        this.RefuseOutside(at);

        int index = (at.Y * this.Width) + at.X;
        if (this.walked[index])
        {
            return;
        }

        this.walked[index] = true;
        this.Count += 1;
    }

    /// <summary>Writes the record as one string for each row, for a snapshot (D-567).</summary>
    /// <returns>The rows, from the north edge to the south edge.</returns>
    public IReadOnlyList<string> Rows()
    {
        var rows = new List<string>(this.Height);
        for (int row = 0; row < this.Height; row += 1)
        {
            var line = new StringBuilder(this.Width);
            for (int column = 0; column < this.Width; column += 1)
            {
                line.Append(this.walked[(row * this.Width) + column] ? WalkedCharacter : FreshCharacter);
            }

            rows.Add(line.ToString());
        }

        return rows;
    }

    /// <summary>Adds every value of this record to the state hash (G-5).</summary>
    /// <param name="hasher">The hasher of the state.</param>
    /// <exception cref="ArgumentNullException">The hasher is null (T-2).</exception>
    public void Hash(StateHasher hasher)
    {
        ArgumentNullException.ThrowIfNull(hasher);

        hasher.AddInt32(this.Width);
        hasher.AddInt32(this.Height);
        hasher.AddInt32(this.Count);
        foreach (bool tile in this.walked)
        {
            hasher.AddBoolean(tile);
        }
    }

    private static void RefuseSide(int side, string name)
    {
        if (side < 1 || side > GameMap.MaxSide)
        {
            throw new ArgumentOutOfRangeException(
                name,
                side,
                $"A map holds 1 to {GameMap.MaxSide} tiles on each side, and the walked tiles take the size of the map (T-2, D-528).");
        }
    }

    private void RefuseOutside(TilePoint at)
    {
        if (at.X < 0 || at.X >= this.Width || at.Y < 0 || at.Y >= this.Height)
        {
            throw new ArgumentOutOfRangeException(
                nameof(at),
                at,
                $"The walked tiles hold {this.Width} by {this.Height} tiles, and this tile lies outside them (T-2).");
        }
    }
}
