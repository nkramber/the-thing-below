using System;

namespace TheThingBelow.Tools.Png;

/// <summary>
/// The CRC-32 that each PNG chunk carries. Tools holds this function, and it takes no
/// package (D-663, G-13).
/// </summary>
/// <remarks>
/// A PNG chunk ends with "A four-byte CRC calculated on the preceding bytes in the chunk",
/// which covers the chunk type and the chunk data and not the length. Source: the PNG
/// specification of the W3C, `https://www.w3.org/TR/png-3/`, read 2026-09-14.
/// <para>
/// The algorithm is the CRC-32 of that specification: the polynomial 0xedb88320 in its
/// reflected form, the start value with every bit set, and a complement of the result. The
/// test class holds the published check values.
/// </para>
/// </remarks>
public static class Crc32
{
    /// <summary>The reflected polynomial of the PNG specification, annex D.</summary>
    private const uint Polynomial = 0xedb88320u;

    /// <summary>The start value and the final mask of the algorithm.</summary>
    private const uint AllBits = 0xffffffffu;

    /// <summary>One entry for each value of the low byte of the register.</summary>
    private static readonly uint[] Table = BuildTable();

    /// <summary>Gives the CRC-32 of the bytes.</summary>
    /// <param name="data">The bytes to read. An empty span is legal, and it gives 0.</param>
    /// <returns>The CRC-32, as the four bytes of a chunk hold it in the big-endian order.</returns>
    public static uint Compute(ReadOnlySpan<byte> data)
    {
        uint register = AllBits;
        foreach (byte value in data)
        {
            register = Table[(register ^ value) & 0xff] ^ (register >> 8);
        }

        return register ^ AllBits;
    }

    private static uint[] BuildTable()
    {
        uint[] table = new uint[256];
        for (uint index = 0; index < 256; index += 1)
        {
            uint value = index;
            for (int bit = 0; bit < 8; bit += 1)
            {
                value = (value & 1) != 0 ? Polynomial ^ (value >> 1) : value >> 1;
            }

            table[index] = value;
        }

        return table;
    }
}
