using System;

namespace TheThingBelow.Game;

/// <summary>
/// Turns the linear color of an HDR 2D window into the sRGB bytes of a PNG (D-172, D-188).
/// </summary>
/// <remarks>
/// The project turns on HDR 2D for the glow of D-188, so the root viewport renders in linear
/// color, and Godot gives the image of its texture in the format `Rgbh`: three half floats of
/// linear light for each pixel. The screen shows the sRGB form of that light. A capture that
/// saves the linear values as bytes shows every color much darker than the screen: the shadow
/// `2b2836` of the palette reads `06 05 09` (T-2).
/// <para>
/// This type holds no Godot value, so a test reads it with no engine (D-614).
/// </para>
/// </remarks>
public static class CaptureColors
{
    /// <summary>The count of bytes of one pixel of an `Rgbh` image: three half floats.</summary>
    public const int LinearBytesPerPixel = 6;

    /// <summary>The count of bytes of one pixel of the result: red, green, blue, and alpha.</summary>
    public const int SrgbBytesPerPixel = 4;

    /// <summary>Gives the sRGB bytes of an image of linear half floats.</summary>
    /// <param name="linear">The bytes of the `Rgbh` image, in the order of its rows.</param>
    /// <param name="pixels">The count of pixels of the image.</param>
    /// <returns>Four bytes for each pixel: red, green, and blue in sRGB, and an alpha of 255.</returns>
    /// <exception cref="ArgumentNullException">The bytes are null (T-2).</exception>
    /// <exception cref="ArgumentException">The count of bytes is not six for each pixel (T-2).</exception>
    public static byte[] ToSrgb(byte[] linear, int pixels)
    {
        ArgumentNullException.ThrowIfNull(linear);
        ArgumentOutOfRangeException.ThrowIfNegative(pixels);
        if (linear.Length != (long)pixels * LinearBytesPerPixel)
        {
            throw new ArgumentException(
                $"An image of {pixels} pixels in the format Rgbh holds {(long)pixels * LinearBytesPerPixel} bytes, and this one holds {linear.Length} (T-2).",
                nameof(linear));
        }

        byte[] srgb = new byte[pixels * SrgbBytesPerPixel];
        for (int pixel = 0; pixel < pixels; pixel += 1)
        {
            for (int channel = 0; channel < 3; channel += 1)
            {
                int start = (pixel * LinearBytesPerPixel) + (channel * 2);
                ushort bits = (ushort)(linear[start] | (linear[start + 1] << 8));
                srgb[(pixel * SrgbBytesPerPixel) + channel] = ToSrgbByte((float)BitConverter.UInt16BitsToHalf(bits));
            }

            srgb[(pixel * SrgbBytesPerPixel) + 3] = 255;
        }

        return srgb;
    }

    /// <summary>Gives the sRGB byte of one channel of linear light, with the curve of the sRGB standard.</summary>
    /// <param name="linear">The linear value, where 1 is full light. A value outside 0 to 1 clamps.</param>
    /// <returns>The byte, from 0 to 255.</returns>
    public static byte ToSrgbByte(float linear)
    {
        double light = Math.Clamp((double)linear, 0.0, 1.0);
        double encoded = light <= 0.0031308 ? light * 12.92 : (1.055 * Math.Pow(light, 1.0 / 2.4)) - 0.055;
        return (byte)Math.Round(encoded * 255.0, MidpointRounding.AwayFromZero);
    }
}
