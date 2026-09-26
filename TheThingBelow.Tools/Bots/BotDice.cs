using System;
using TheThingBelow.Core.Streams;

namespace TheThingBelow.Tools.Bots;

/// <summary>
/// The random numbers of a bot policy. They come from a generator of their own, outside every
/// rule stream, so a policy never moves a roll of the rules (G-4, D-64).
/// </summary>
/// <remarks>
/// The stream number of each policy lies far above the numbers of the rule streams, and a test
/// asserts that no rule stream shares its increment. The run record holds the intents, so a
/// replay reads no number of this generator (T-7).
/// </remarks>
public sealed class BotDice
{
    /// <summary>The stream number of the first policy. The text "bots" in ASCII, as a number.</summary>
    public const ulong FirstSequence = 0x626F_7473_0000_0000;

    private readonly Pcg32 generator;

    /// <summary>Makes the numbers of one policy in one run.</summary>
    /// <param name="seed">The seed of the run.</param>
    /// <param name="policy">The policy.</param>
    public BotDice(ulong seed, BotPolicyKind policy)
    {
        this.generator = Pcg32.FromSeed(seed, SequenceOf(policy));
    }

    /// <summary>Gives the stream number of the generator of a policy.</summary>
    /// <param name="policy">The policy.</param>
    /// <returns>The stream number.</returns>
    public static ulong SequenceOf(BotPolicyKind policy) => FirstSequence + (ulong)policy;

    /// <summary>Picks a number from zero to one below the count, with no bias.</summary>
    /// <param name="count">The count of choices.</param>
    /// <returns>The pick.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The count is below one (T-2).</exception>
    public int Pick(int count)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

        // A draw at or above the last whole multiple of the count tries again, so each pick
        // has the same chance.
        uint bound = (uint)count;
        uint limit = uint.MaxValue - (uint.MaxValue % bound);
        uint draw = this.generator.Next();
        while (draw >= limit)
        {
            draw = this.generator.Next();
        }

        return (int)(draw % bound);
    }
}
