using System;
using System.Globalization;
using System.IO;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Content;

namespace TheThingBelow.Tools.Import;

/// <summary>
/// One frame of one drawing file, as the `import` command and the `frame-png` command name
/// it with the `--drawing` and the `--frame` options (D-1311, D-1313).
/// </summary>
/// <param name="Path">The path of the drawing file, as the command line gives it.</param>
/// <param name="Bytes">The bytes of the drawing file.</param>
/// <param name="Drawing">The drawing, as the reader of Core reads it.</param>
/// <param name="Frame">The position of the frame, which starts at 0.</param>
public sealed record FrameTarget(string Path, byte[] Bytes, Drawing Drawing, int Frame)
{
    /// <summary>Reads the frame number of the command line.</summary>
    /// <param name="text">The value of the `--frame` option.</param>
    /// <param name="frame">The position of the frame, 0 or more.</param>
    /// <returns>True when the text is a whole number of 0 or more.</returns>
    public static bool TryParseFrame(string text, out int frame) =>
        int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out frame);

    /// <summary>Reads a drawing file and checks that it holds the frame.</summary>
    /// <param name="path">The path of the drawing file.</param>
    /// <param name="frame">The position of the frame, which starts at 0.</param>
    /// <returns>The target.</returns>
    /// <exception cref="IOException">The file cannot be read.</exception>
    /// <exception cref="ContentException">The file breaks a rule of the reader (G-6, T-2).</exception>
    /// <exception cref="ImportException">The drawing holds no such frame (T-2).</exception>
    public static FrameTarget Read(string path, int frame)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentOutOfRangeException.ThrowIfNegative(frame);

        byte[] bytes = File.ReadAllBytes(path);
        Drawing drawing = Drawing.Read(bytes, path);
        if (frame >= drawing.Frames.Count)
        {
            throw ImportException.For(
                path,
                $"the drawing holds {drawing.Frames.Count} frames, from 0 to {drawing.Frames.Count - 1}, and it has no frame {frame}");
        }

        return new FrameTarget(path, bytes, drawing, frame);
    }

    /// <summary>Reads the palette of a checkout.</summary>
    /// <param name="root">The root of the checkout, which holds the `content` folder.</param>
    /// <returns>The palette.</returns>
    /// <exception cref="IOException">The file cannot be read.</exception>
    /// <exception cref="ContentException">The palette breaks a rule of the reader (T-2).</exception>
    public static Palette ReadPalette(string root)
    {
        ArgumentException.ThrowIfNullOrEmpty(root);

        string path = System.IO.Path.Combine(root, ContentFolder.FolderName, Palette.Path);
        return Palette.Read(File.ReadAllBytes(path), Palette.Path);
    }
}
