using System;
using System.Text;

namespace TheThingBelow.Core.Hashing;

/// <summary>
/// Builds the state hash. A caller adds every part of the state in one fixed order, then
/// reads the hash (G-4, G-5). Two machines that hold the same state get the same value.
/// </summary>
/// <remarks>
/// Each `Add` method writes a length-tagged form into a buffer, and <see cref="Finish"/>
/// hashes the whole buffer with <see cref="XxHash64"/> (D-644). The length tag of the text
/// keeps two states apart that hold the same characters in different fields.
/// <para>
/// No `Add` method writes a type tag, so `AddUInt64(1)` and `AddBoolean(true)` give the same
/// bytes. Every caller adds a fixed schema in a fixed order, and a schema whose shape can
/// change, such as a list, adds its count before its items.
/// </para>
/// </remarks>
public sealed class StateHasher
{
    /// <summary>The seed of the state hash. Every state hash of this game uses it.</summary>
    private const ulong StateSeed = 0;

    private byte[] buffer;
    private int length;

    /// <summary>Makes an empty hasher.</summary>
    public StateHasher()
    {
        this.buffer = new byte[64];
        this.length = 0;
    }

    /// <summary>The count of bytes that the hasher holds now.</summary>
    public int ByteCount => this.length;

    /// <summary>Adds a 64-bit value.</summary>
    /// <param name="value">The value.</param>
    public void AddUInt64(ulong value)
    {
        this.MakeRoom(8);
        for (int offset = 0; offset < 8; offset += 1)
        {
            this.buffer[this.length + offset] = (byte)(value >> (offset * 8));
        }

        this.length += 8;
    }

    /// <summary>Adds a signed 64-bit value.</summary>
    /// <param name="value">The value.</param>
    public void AddInt64(long value) => this.AddUInt64(unchecked((ulong)value));

    /// <summary>Adds a signed 32-bit value.</summary>
    /// <param name="value">The value.</param>
    public void AddInt32(int value) => this.AddUInt64(unchecked((ulong)(long)value));

    /// <summary>Adds a true or false value.</summary>
    /// <param name="value">The value.</param>
    public void AddBoolean(bool value) => this.AddUInt64(value ? 1UL : 0UL);

    /// <summary>Adds text as UTF-8 bytes, with the byte count before them.</summary>
    /// <param name="value">The text. An empty text is legal.</param>
    /// <exception cref="ArgumentNullException">The text is null (T-2).</exception>
    /// <remarks>
    /// The byte count comes first, so the two states `["ab", "c"]` and `["a", "bc"]` give
    /// two different hashes. UTF-8 gives the same bytes on every machine, and no culture
    /// and no ICU version reaches the value (F-39).
    /// </remarks>
    public void AddText(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        int count = Encoding.UTF8.GetByteCount(value);
        this.AddUInt64((ulong)count);
        this.MakeRoom(count);
        Encoding.UTF8.GetBytes(value, this.buffer.AsSpan(this.length, count));
        this.length += count;
    }

    /// <summary>Gives the hash of everything that the hasher holds now.</summary>
    /// <returns>The state hash.</returns>
    /// <remarks>This method changes nothing, so a caller can read the hash more than one time.</remarks>
    public ulong Finish() => XxHash64.Compute(this.buffer.AsSpan(0, this.length), StateSeed);

    private void MakeRoom(int count)
    {
        if (this.length + count <= this.buffer.Length)
        {
            return;
        }

        int size = this.buffer.Length;
        while (size < this.length + count)
        {
            size *= 2;
        }

        byte[] grown = new byte[size];
        this.buffer.AsSpan(0, this.length).CopyTo(grown);
        this.buffer = grown;
    }
}
