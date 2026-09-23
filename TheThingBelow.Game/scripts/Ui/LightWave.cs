using System;
using System.Text;
using TheThingBelow.Core;
using TheThingBelow.Core.Hashing;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The slow wave of the tick that swells and fades a light: the pulse of each glow and the
/// shimmer of each light shaft (D-913, D-921).
/// </summary>
/// <remarks>
/// The wave is a cosine, so the light swells and fades with no jump, where a torch light steps
/// (D-891). The phase comes from the id of the source, so two sources never swell together.
/// </remarks>
public static class LightWave
{
    /// <summary>Gives the part of the light of one source that shows at one tick: 1 at the top of the wave, and 1 less the depth at the low.</summary>
    /// <param name="id">The id of the source, which sets the phase of its wave.</param>
    /// <param name="waveTicks">The ticks of one wave: 2 or more.</param>
    /// <param name="depth">The part of the light that the wave takes away at the low, in basis points.</param>
    /// <param name="tick">The tick of the run, from 0.</param>
    /// <param name="seed">The seed of the hash of the phase, one for each kind of wave.</param>
    /// <returns>The part of the light that shows.</returns>
    /// <exception cref="ArgumentException">The id is null or empty (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The wave is below 2 ticks, the depth is below zero, or the tick is below zero (T-2).</exception>
    public static float PartOf(string id, int waveTicks, int depth, long tick, ulong seed)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentOutOfRangeException.ThrowIfLessThan(waveTicks, 2);
        ArgumentOutOfRangeException.ThrowIfNegative(depth);
        ArgumentOutOfRangeException.ThrowIfNegative(tick);

        ulong hash = XxHash64.Compute(Encoding.UTF8.GetBytes(id), seed);
        long phase = (long)(hash % (ulong)waveTicks);
        long step = (tick + phase) % waveTicks;
        float wave = (1f - MathF.Cos(2f * MathF.PI * step / waveTicks)) / 2f;
        return 1f - (depth / (float)BasisPoints.One * wave);
    }
}
