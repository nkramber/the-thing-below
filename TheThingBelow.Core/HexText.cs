using System.Globalization;

namespace TheThingBelow.Core;

/// <summary>
/// The text form of a 64-bit value in a record, a save, or a log line: `0x` and 16 lowercase
/// hexadecimal digits (D-652).
/// </summary>
/// <remarks>
/// A seed and the position of a stream fill 64 bits, and a decimal number of that size reads
/// as noise. One spelling on every machine also lets a diff compare a file by character, so
/// no reader needs a tool to see that two runs differ (T-7, T-1).
/// </remarks>
public static class HexText
{
    /// <summary>The count of hexadecimal digits after the `0x`.</summary>
    public const int DigitCount = 16;

    /// <summary>Writes a 64-bit value as `0x` and 16 lowercase hexadecimal digits.</summary>
    /// <param name="value">The value.</param>
    /// <returns>The text, for example `0x0000000000bada55`.</returns>
    public static string Of(ulong value) => "0x" + value.ToString("x16", CultureInfo.InvariantCulture);
}
