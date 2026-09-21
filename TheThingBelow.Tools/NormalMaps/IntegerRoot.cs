using System;

namespace TheThingBelow.Tools.NormalMaps;

/// <summary>The integer square root of the normal-map command (D-502, F-38).</summary>
/// <remarks>
/// The results of double math "might differ slightly by platform", and the CI legs mix x86_64
/// and Apple silicon (F-38). This root uses whole numbers alone, so every leg gives the same
/// root and the same pixels (D-502, T-7).
/// </remarks>
public static class IntegerRoot
{
    /// <summary>Gives the largest whole number whose square is not more than the value.</summary>
    /// <param name="value">The value, 0 or more.</param>
    /// <returns>The floor of the square root of the value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value is below zero (T-2).</exception>
    /// <remarks>
    /// The method finds one bit of the root on each pass, from the highest bit down, the
    /// method of a root by hand in base 2. It takes no division and no float.
    /// </remarks>
    public static long Floor(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);

        long rest = value;
        long root = 0;

        // The highest power of 4 that is not more than the value.
        long bit = 1L << 62;
        while (bit > rest)
        {
            bit >>= 2;
        }

        while (bit != 0)
        {
            if (rest >= root + bit)
            {
                rest -= root + bit;
                root = (root >> 1) + bit;
            }
            else
            {
                root >>= 1;
            }

            bit >>= 2;
        }

        return root;
    }
}
