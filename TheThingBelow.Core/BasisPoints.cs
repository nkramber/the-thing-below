using System;

namespace TheThingBelow.Core;

/// <summary>
/// The fixed-point math of Core. A fraction is an integer in basis points, where 10000
/// means 100% (D-169, G-2). Every system calls the operations below, and no system writes
/// its own multiply or divide (D-641, T-1).
/// </summary>
/// <remarks>
/// Every result rounds toward zero (D-641). The C# division operator on integers already
/// rounds that way, so no operation below adds a round step. A system that must never give
/// nothing sets its own floor, such as a minimum of 1 point of damage.
/// </remarks>
public static class BasisPoints
{
    /// <summary>The value of 100%, the scale of every rate in content (D-169).</summary>
    public const int One = 10000;

    /// <summary>Applies a rate in basis points to a value.</summary>
    /// <param name="value">The value that the rate reads, such as a damage total.</param>
    /// <param name="rate">The rate in basis points. 10000 gives the value back.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <returns>The product, rounded toward zero (D-641).</returns>
    /// <exception cref="SimulationException">The result does not fit in an `int`.</exception>
    public static int Apply(int value, int rate, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // The product of two `int` values always fits in a `long`, so this multiply cannot
        // overflow. The cast at the end is the one step that can (T-2).
        long product = (long)value * rate;
        return ToInt(product / One, $"a rate of {rate} on {value}", context);
    }

    /// <summary>Divides one value by another.</summary>
    /// <param name="value">The value to divide.</param>
    /// <param name="divisor">The divisor. A zero divisor is an error (T-2).</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <returns>The quotient, rounded toward zero (D-641).</returns>
    /// <exception cref="SimulationException">The divisor is zero, or the result does not fit.</exception>
    public static int Divide(int value, int divisor, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (divisor == 0)
        {
            throw new SimulationException($"a division of {value} by zero", context);
        }

        // `int.MinValue / -1` is the one quotient of two `int` values that no `int` holds,
        // and the `long` below carries it to the range check of `ToInt` (T-2).
        long quotient = (long)value / divisor;
        return ToInt(quotient, $"a division of {value} by {divisor}", context);
    }

    /// <summary>Gives the rate of a part against a whole, in basis points.</summary>
    /// <param name="part">The part, such as the current health.</param>
    /// <param name="whole">The whole, such as the maximum health. A zero whole is an error.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <returns>The rate in basis points, rounded toward zero (D-641).</returns>
    /// <exception cref="SimulationException">The whole is zero, or the result does not fit.</exception>
    public static int RateOf(int part, int whole, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (whole == 0)
        {
            throw new SimulationException($"a rate of {part} against a whole of zero", context);
        }

        long scaled = (long)part * One;
        return ToInt(scaled / whole, $"a rate of {part} against {whole}", context);
    }

    private static int ToInt(long value, string operation, RunContext context)
    {
        try
        {
            return checked((int)value);
        }
        catch (OverflowException error)
        {
            // Core never wraps an overflow in silence. The error names the operation, the
            // value that did not fit, and the run context (T-2, G-18).
            throw new SimulationException(
                $"{operation} gave {value}, which no `int` holds",
                context,
                error);
        }
    }
}
