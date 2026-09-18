using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace TheThingBelow.Tools.Identity;

/// <summary>
/// The identity file: each run of the replay-identity set and its expected state hash
/// (D-504). The file is text, so a review reads each hash that a PR moves (G-17).
/// </summary>
public static class IdentityFile
{
    /// <summary>The path of the file, from the root of the checkout.</summary>
    public const string Path = "TheThingBelow.Tests/identity/replay-identity.txt";

    /// <summary>The lines above the runs. Each one starts with a number sign.</summary>
    private static readonly IReadOnlyList<string> Header =
    [
        "# The replay-identity set (G-5, D-504). Each CI leg computes every hash below and",
        "# compares it with this file. A mismatch fails the `replay-identity` job.",
        "#",
        "# Each run line holds the name of the run, one space, and the expected state hash.",
        "# A PR that moves a hash also raises the simulation version, and the review reads",
        "# each changed line (G-17).",
        "#",
        "# Write this file again with:",
        "#   dotnet run --project TheThingBelow.Tools/TheThingBelow.Tools.csproj --",
        "#     replay-identity --root . --write",
    ];

    /// <summary>Reads the expected hash of each run.</summary>
    /// <param name="root">The root of the checkout.</param>
    /// <returns>The run names in the order of the file, each with its expected hash.</returns>
    /// <exception cref="FileNotFoundException">The checkout holds no identity file.</exception>
    /// <exception cref="InvalidDataException">A line of the file takes no legal form (T-2).</exception>
    public static IReadOnlyList<KeyValuePair<string, ulong>> Read(string root)
    {
        ArgumentException.ThrowIfNullOrEmpty(root);

        string path = System.IO.Path.Combine(root, Path);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"The checkout holds no identity file at {Path}.", path);
        }

        List<KeyValuePair<string, ulong>> runs = [];
        string[] lines = File.ReadAllLines(path);
        for (int index = 0; index < lines.Length; index += 1)
        {
            string line = lines[index].Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            runs.Add(ParseRun(line, Path, index + 1));
        }

        return runs;
    }

    /// <summary>Writes the file again from the run names and their hashes.</summary>
    /// <param name="root">The root of the checkout.</param>
    /// <param name="runs">The run names, each with the hash to write.</param>
    /// <remarks>The file always ends each line with a line feed, so every leg reads the same bytes.</remarks>
    public static void Write(string root, IReadOnlyList<KeyValuePair<string, ulong>> runs)
    {
        ArgumentException.ThrowIfNullOrEmpty(root);
        ArgumentNullException.ThrowIfNull(runs);

        StringBuilder text = new();
        foreach (string line in Header)
        {
            text.Append(line).Append('\n');
        }

        text.Append('\n');
        foreach (KeyValuePair<string, ulong> run in runs)
        {
            text.Append(run.Key).Append(' ').Append(Format(run.Value)).Append('\n');
        }

        string path = System.IO.Path.Combine(root, Path);
        string? folder = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }

        File.WriteAllText(path, text.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    /// <summary>Gives a hash in the form that the file holds.</summary>
    /// <param name="hash">The hash.</param>
    /// <returns>The hash as 16 hexadecimal digits after `0x`.</returns>
    public static string Format(ulong hash) =>
        "0x" + hash.ToString("X16", CultureInfo.InvariantCulture);

    private static KeyValuePair<string, ulong> ParseRun(string line, string path, int number)
    {
        string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            throw new InvalidDataException(
                $"{path}:{number}: the line holds {parts.Length} words, and a run line holds two.");
        }

        string text = parts[1];
        if (!text.StartsWith("0x", StringComparison.Ordinal) || text.Length != 18)
        {
            throw new InvalidDataException(
                $"{path}:{number}: the hash '{text}' is not `0x` and 16 hexadecimal digits.");
        }

        if (!ulong.TryParse(text[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong hash))
        {
            throw new InvalidDataException(
                $"{path}:{number}: the hash '{text}' holds a character that is not hexadecimal.");
        }

        return new KeyValuePair<string, ulong>(parts[0], hash);
    }
}
