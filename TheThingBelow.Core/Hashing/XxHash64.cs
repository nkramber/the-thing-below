using System;

namespace TheThingBelow.Core.Hashing;

/// <summary>
/// The 64-bit hash function that Core holds (D-644). It makes the state hash and the seed
/// of every random stream (G-5, D-643).
/// </summary>
/// <remarks>
/// Core calls no .NET hash class and no `GetHashCode`. The .NET hash classes defer to the
/// OS libraries, and a string hash code can differ between two runs (F-35, G-1).
/// <para>
/// This is XXH64. The algorithm and the constants come from the specification at
/// `https://github.com/Cyan4973/xxHash/blob/dev/doc/xxhash_spec.md`, read 2026-09-18. The
/// test class holds the published vectors of the reference implementation.
/// </para>
/// </remarks>
public static class XxHash64
{
    private const ulong Prime1 = 11400714785074694791UL;
    private const ulong Prime2 = 14029467366897019727UL;
    private const ulong Prime3 = 1609587929392839161UL;
    private const ulong Prime4 = 9650029242287828579UL;
    private const ulong Prime5 = 2870177450012600261UL;

    /// <summary>The size of one block of the main loop, in bytes.</summary>
    private const int BlockSize = 32;

    /// <summary>Gives the 64-bit hash of the bytes.</summary>
    /// <param name="data">The bytes to hash. An empty span is legal.</param>
    /// <param name="seed">The seed of the hash. Two seeds give two different functions.</param>
    /// <returns>The hash value.</returns>
    /// <remarks>
    /// Every operation below wraps on purpose, which is the definition of the algorithm.
    /// The `unchecked` blocks say so, because a reader of this repository expects `checked`
    /// arithmetic in Core (T-2).
    /// </remarks>
    public static ulong Compute(ReadOnlySpan<byte> data, ulong seed)
    {
        ulong accumulator;
        int read = 0;

        if (data.Length >= BlockSize)
        {
            accumulator = ConsumeBlocks(data, seed, out read);
        }
        else
        {
            accumulator = unchecked(seed + Prime5);
        }

        accumulator = unchecked(accumulator + (ulong)data.Length);
        return Avalanche(ConsumeTail(data[read..], accumulator));
    }

    private static ulong ConsumeBlocks(ReadOnlySpan<byte> data, ulong seed, out int read)
    {
        ulong first = unchecked(seed + Prime1 + Prime2);
        ulong second = unchecked(seed + Prime2);
        ulong third = seed;
        ulong fourth = unchecked(seed - Prime1);

        read = 0;
        while (data.Length - read >= BlockSize)
        {
            first = Round(first, ReadUInt64(data, read));
            second = Round(second, ReadUInt64(data, read + 8));
            third = Round(third, ReadUInt64(data, read + 16));
            fourth = Round(fourth, ReadUInt64(data, read + 24));
            read += BlockSize;
        }

        ulong accumulator = unchecked(
            RotateLeft(first, 1) + RotateLeft(second, 7) +
            RotateLeft(third, 12) + RotateLeft(fourth, 18));

        accumulator = MergeRound(accumulator, first);
        accumulator = MergeRound(accumulator, second);
        accumulator = MergeRound(accumulator, third);
        return MergeRound(accumulator, fourth);
    }

    private static ulong ConsumeTail(ReadOnlySpan<byte> tail, ulong accumulator)
    {
        int read = 0;
        while (tail.Length - read >= 8)
        {
            accumulator ^= Round(0, ReadUInt64(tail, read));
            accumulator = unchecked((RotateLeft(accumulator, 27) * Prime1) + Prime4);
            read += 8;
        }

        if (tail.Length - read >= 4)
        {
            accumulator ^= unchecked(ReadUInt32(tail, read) * Prime1);
            accumulator = unchecked((RotateLeft(accumulator, 23) * Prime2) + Prime3);
            read += 4;
        }

        while (read < tail.Length)
        {
            accumulator ^= unchecked(tail[read] * Prime5);
            accumulator = unchecked(RotateLeft(accumulator, 11) * Prime1);
            read += 1;
        }

        return accumulator;
    }

    private static ulong Round(ulong accumulator, ulong lane) => unchecked(
        RotateLeft(accumulator + (lane * Prime2), 31) * Prime1);

    private static ulong MergeRound(ulong accumulator, ulong lane) => unchecked(
        ((accumulator ^ Round(0, lane)) * Prime1) + Prime4);

    private static ulong Avalanche(ulong value)
    {
        unchecked
        {
            value ^= value >> 33;
            value *= Prime2;
            value ^= value >> 29;
            value *= Prime3;
            return value ^ (value >> 32);
        }
    }

    private static ulong RotateLeft(ulong value, int count) =>
        (value << count) | (value >> (64 - count));

    // The algorithm reads little-endian lanes. This method reads the bytes one at a time,
    // so a big-endian machine gives the same value as a little-endian one (T-7).
    private static ulong ReadUInt64(ReadOnlySpan<byte> data, int start)
    {
        ulong value = 0;
        for (int offset = 7; offset >= 0; offset -= 1)
        {
            value = (value << 8) | data[start + offset];
        }

        return value;
    }

    private static ulong ReadUInt32(ReadOnlySpan<byte> data, int start)
    {
        uint value = 0;
        for (int offset = 3; offset >= 0; offset -= 1)
        {
            value = (value << 8) | data[start + offset];
        }

        return value;
    }
}
