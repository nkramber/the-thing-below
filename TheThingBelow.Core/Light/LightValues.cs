using System;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Light;

/// <summary>The color of one light: a key of the palette and a strength (D-846).</summary>
/// <param name="Key">The key of a palette color, such as `j` for a flame (L-10).</param>
/// <param name="Strength">
/// The strength in basis points, where 10000 gives the palette color at full strength (D-169).
/// </param>
/// <remarks>
/// Every color of content comes from the one palette, and a lit pixel on screen can still
/// reach any color, because Godot adds each light to the pixel (D-181).
/// </remarks>
public sealed record LightColor(char Key, int Strength);

/// <summary>The values of one point light: its color, its range, and its height (D-183, F-46).</summary>
/// <param name="Color">The palette key and the strength of the light (D-846).</param>
/// <param name="Range">The distance in art pixels from the center of the light to the end of its reach.</param>
/// <param name="Height">
/// The height of the light over the ground, in art pixels. A light at height 0 gives no light
/// to a flat pixel of a normal-mapped sprite, so the height is 1 or more (F-46).
/// </param>
public sealed record PointLightValues(LightColor Color, int Range, int Height);

/// <summary>
/// The limits of each light value, and the reads that every light file shares (D-846, F-46).
/// </summary>
/// <remarks>
/// A light file names its color as a palette key and a strength, and never as a free color
/// (D-846). The load checks each key against the palette after every file is read, because the
/// palette is a file of its own (<see cref="LightContent"/>).
/// </remarks>
public static class LightValues
{
    /// <summary>The highest strength of a point light, in basis points: four times full strength.</summary>
    public const int MostLightStrength = 4 * BasisPoints.One;

    /// <summary>The highest strength of an ambient light, in basis points: the palette color itself.</summary>
    public const int MostAmbientStrength = BasisPoints.One;

    /// <summary>The longest range of a point light, in art pixels: the width of the world view (D-634).</summary>
    public const int MostRange = 640;

    /// <summary>The greatest height of a point light, in art pixels.</summary>
    public const int MostHeight = 256;

    /// <summary>Checks the `color` and the `strength` of a light, and makes its color (D-846).</summary>
    /// <param name="reader">The reader, after the object that holds the two fields.</param>
    /// <param name="depth">The depth of that object.</param>
    /// <param name="key">The palette key that the read set, or null when the file has none.</param>
    /// <param name="strength">The strength that the read set.</param>
    /// <param name="mostStrength">The highest strength that this light takes, in basis points.</param>
    /// <returns>The color.</returns>
    /// <exception cref="ContentException">A field is absent, or a value is outside its limits (T-2).</exception>
    public static LightColor BuildColor(ref ContentReader reader, int depth, string? key, int? strength, int mostStrength)
    {
        string text = reader.Require(key, depth, "color");
        if (text.Length != 1)
        {
            throw reader.RefuseField(depth, "color", $"the color is '{text}', and a light names one palette key of one character (D-846)");
        }

        int value = reader.RequireInt(strength, depth, "strength");
        if (value < 0 || value > mostStrength)
        {
            throw reader.RefuseField(
                depth,
                "strength",
                $"the strength is {value}, and it takes 0 to {mostStrength} basis points (D-169, D-846)");
        }

        return new LightColor(text[0], value);
    }

    /// <summary>Checks the range and the height of a point light, and makes its values.</summary>
    /// <param name="reader">The reader, after the object of the light.</param>
    /// <param name="depth">The depth of that object.</param>
    /// <param name="color">The color of the light.</param>
    /// <param name="range">The range that the read set, or null when the file has none.</param>
    /// <param name="height">The height that the read set, or null when the file has none.</param>
    /// <returns>The values of the light.</returns>
    /// <exception cref="ContentException">A field is absent, or a value is outside its limits (T-2).</exception>
    public static PointLightValues BuildPoint(ref ContentReader reader, int depth, LightColor color, int? range, int? height)
    {
        ArgumentNullException.ThrowIfNull(color);

        int reach = reader.RequireInt(range, depth, "range");
        if (reach < 1 || reach > MostRange)
        {
            throw reader.RefuseField(depth, "range", $"the range is {reach}, and it takes 1 to {MostRange} art pixels (D-634)");
        }

        int lift = reader.RequireInt(height, depth, "height");
        if (lift < 1 || lift > MostHeight)
        {
            throw reader.RefuseField(
                depth,
                "height",
                $"the height is {lift}, and it takes 1 to {MostHeight}, because a light at height 0 gives no light to a flat pixel (F-46)");
        }

        return new PointLightValues(color, reach, lift);
    }
}

/// <summary>
/// The four fields of a point light inside one object of a light file: `color`, `strength`,
/// `range`, and `height` (D-846, F-46). Each object that holds a light reads its own other
/// fields, and it gives the four fields to this reader.
/// </summary>
/// <remarks>
/// One reader for the four fields keeps each limit in one place (T-1). An object that holds
/// the fields reads them in its own `switch`, so an unknown field stays an error there.
/// </remarks>
public sealed class PointLightFields
{
    private string? color;
    private int? strength;
    private int? range;
    private int? height;

    /// <summary>Reads the value of one field when it is a field of a point light.</summary>
    /// <param name="reader">The reader, at the value of the field.</param>
    /// <param name="field">The name of the field.</param>
    /// <returns>True when the field was one of the four, and false when the caller reads it.</returns>
    /// <exception cref="ContentException">The value has the wrong type (T-2).</exception>
    public bool TryRead(ref ContentReader reader, string field)
    {
        switch (field)
        {
            case "color":
                this.color = reader.ReadString();
                return true;
            case "strength":
                this.strength = reader.ReadInt();
                return true;
            case "range":
                this.range = reader.ReadInt();
                return true;
            case "height":
                this.height = reader.ReadInt();
                return true;
            default:
                return false;
        }
    }

    /// <summary>Checks the four fields after the end of the object, and makes the light.</summary>
    /// <param name="reader">The reader, after the object.</param>
    /// <param name="depth">The depth of the object.</param>
    /// <returns>The values of the light.</returns>
    /// <exception cref="ContentException">A field is absent, or a value is outside its limits (T-2).</exception>
    public PointLightValues Build(ref ContentReader reader, int depth)
    {
        LightColor tint = LightValues.BuildColor(ref reader, depth, this.color, this.strength, LightValues.MostLightStrength);
        return LightValues.BuildPoint(ref reader, depth, tint, this.range, this.height);
    }
}
