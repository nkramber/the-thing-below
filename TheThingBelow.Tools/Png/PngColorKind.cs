namespace TheThingBelow.Tools.Png;

/// <summary>
/// The two color types that the PNG code reads and writes, with the code that the IHDR chunk
/// holds for each one (D-176). A PNG of another color type is an error (T-2).
/// </summary>
public enum PngColorKind
{
    /// <summary>Three 8-bit channels for each pixel: red, green, and blue.</summary>
    Rgb = 2,

    /// <summary>Four 8-bit channels for each pixel: red, green, blue, and alpha.</summary>
    Rgba = 6,
}
