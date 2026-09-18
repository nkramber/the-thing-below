using System;

namespace TheThingBelow.Core.Hashing;

/// <summary>
/// The SHA-256 that Core holds (D-644). It makes the content hash, and nothing else calls
/// it (D-645, G-5).
/// </summary>
/// <remarks>
/// Core calls no class of `System.Security.Cryptography`. Those classes defer to the OS
/// libraries, so two platforms can take two code paths, against G-1 and T-7 (F-35). The
/// content hash resists a content file that somebody builds to collide, which is the reason
/// that Core holds a second hash function beside XxHash64 (D-644).
/// <para>
/// The algorithm and the constants come from FIPS 180-4, section 6.2. The test class holds
/// the published vectors of appendix B of that standard.
/// </para>
/// </remarks>
public static class Sha256
{
    /// <summary>The size of the digest, in bytes.</summary>
    public const int DigestSize = 32;

    /// <summary>The size of one block of the compression function, in bytes.</summary>
    private const int BlockSize = 64;

    /// <summary>The 64 round constants of FIPS 180-4, section 4.2.2.</summary>
    private static readonly uint[] RoundConstants =
    [
        0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
        0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
        0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
        0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
        0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
        0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
        0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
        0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2,
    ];

    /// <summary>The eight starting words of FIPS 180-4, section 5.3.3.</summary>
    private static readonly uint[] StartState =
    [
        0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a, 0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19,
    ];

    /// <summary>Gives the 32-byte digest of the bytes.</summary>
    /// <param name="data">The bytes to hash. An empty span is legal.</param>
    /// <returns>The digest, in the byte order of FIPS 180-4.</returns>
    /// <remarks>
    /// Every operation of the compression function wraps on purpose, which is the definition
    /// of the algorithm. The `unchecked` blocks say so, because a reader of this repository
    /// expects `checked` arithmetic in Core (T-2).
    /// </remarks>
    public static byte[] Compute(ReadOnlySpan<byte> data)
    {
        uint[] state = (uint[])StartState.Clone();
        Span<byte> block = stackalloc byte[BlockSize];

        int read = 0;
        while (data.Length - read >= BlockSize)
        {
            Compress(state, data.Slice(read, BlockSize));
            read += BlockSize;
        }

        ReadOnlySpan<byte> tail = data[read..];
        int tailLength = tail.Length;
        tail.CopyTo(block);
        block[tailLength] = 0x80;
        block[(tailLength + 1)..].Clear();

        // The length in bits takes the last 8 bytes of the last block. A tail of 56 bytes or
        // more leaves no room for it, so that tail needs one more block (FIPS 180-4, 5.1.1).
        if (tailLength >= BlockSize - 8)
        {
            Compress(state, block);
            block.Clear();
        }

        WriteUInt64(block[(BlockSize - 8)..], (ulong)data.Length * 8);
        Compress(state, block);

        byte[] digest = new byte[DigestSize];
        for (int word = 0; word < state.Length; word++)
        {
            WriteUInt32(digest.AsSpan(word * 4), state[word]);
        }

        return digest;
    }

    /// <summary>Gives the digest of the bytes as 64 lowercase hexadecimal characters.</summary>
    /// <param name="data">The bytes to hash. An empty span is legal.</param>
    /// <returns>The digest as text, which a content hash field holds.</returns>
    public static string ComputeHex(ReadOnlySpan<byte> data)
    {
        byte[] digest = Compute(data);
        char[] text = new char[DigestSize * 2];
        for (int index = 0; index < digest.Length; index++)
        {
            text[index * 2] = HexDigit(digest[index] >> 4);
            text[(index * 2) + 1] = HexDigit(digest[index] & 0x0f);
        }

        return new string(text);
    }

    private static void Compress(uint[] state, ReadOnlySpan<byte> block)
    {
        Span<uint> schedule = stackalloc uint[64];
        for (int index = 0; index < 16; index++)
        {
            schedule[index] = ReadUInt32(block[(index * 4)..]);
        }

        for (int index = 16; index < schedule.Length; index++)
        {
            uint small = SmallSigma0(schedule[index - 15]);
            uint large = SmallSigma1(schedule[index - 2]);
            schedule[index] = unchecked(schedule[index - 16] + small + schedule[index - 7] + large);
        }

        uint a = state[0];
        uint b = state[1];
        uint c = state[2];
        uint d = state[3];
        uint e = state[4];
        uint f = state[5];
        uint g = state[6];
        uint h = state[7];

        for (int index = 0; index < schedule.Length; index++)
        {
            uint first = unchecked(h + LargeSigma1(e) + Choose(e, f, g) + RoundConstants[index] + schedule[index]);
            uint second = unchecked(LargeSigma0(a) + Majority(a, b, c));

            h = g;
            g = f;
            f = e;
            e = unchecked(d + first);
            d = c;
            c = b;
            b = a;
            a = unchecked(first + second);
        }

        unchecked
        {
            state[0] += a;
            state[1] += b;
            state[2] += c;
            state[3] += d;
            state[4] += e;
            state[5] += f;
            state[6] += g;
            state[7] += h;
        }
    }

    private static uint Choose(uint x, uint y, uint z) => (x & y) ^ (~x & z);

    private static uint Majority(uint x, uint y, uint z) => (x & y) ^ (x & z) ^ (y & z);

    private static uint LargeSigma0(uint x) => RotateRight(x, 2) ^ RotateRight(x, 13) ^ RotateRight(x, 22);

    private static uint LargeSigma1(uint x) => RotateRight(x, 6) ^ RotateRight(x, 11) ^ RotateRight(x, 25);

    private static uint SmallSigma0(uint x) => RotateRight(x, 7) ^ RotateRight(x, 18) ^ (x >> 3);

    private static uint SmallSigma1(uint x) => RotateRight(x, 17) ^ RotateRight(x, 19) ^ (x >> 10);

    private static uint RotateRight(uint value, int count) => (value >> count) | (value << (32 - count));

    private static uint ReadUInt32(ReadOnlySpan<byte> source) =>
        ((uint)source[0] << 24) | ((uint)source[1] << 16) | ((uint)source[2] << 8) | source[3];

    private static void WriteUInt32(Span<byte> target, uint value)
    {
        target[0] = (byte)(value >> 24);
        target[1] = (byte)(value >> 16);
        target[2] = (byte)(value >> 8);
        target[3] = (byte)value;
    }

    private static void WriteUInt64(Span<byte> target, ulong value)
    {
        for (int index = 0; index < 8; index++)
        {
            target[index] = (byte)(value >> ((7 - index) * 8));
        }
    }

    private static char HexDigit(int value) => (char)(value < 10 ? '0' + value : 'a' + (value - 10));
}
