namespace TheThingBelow.Core.Effects;

/// <summary>
/// One band of a layer of fog: the noise level where the band starts, and the strength of the
/// band (D-897). A pixel whose noise reaches the level takes the band, with a hard edge (G-27).
/// </summary>
/// <param name="From">The noise level where the band starts, in basis points of the range of the noise (D-169).</param>
/// <param name="Strength">The strength of the band, in basis points, where 10000 covers the art in full (D-169).</param>
public sealed record FogBand(int From, int Strength);
