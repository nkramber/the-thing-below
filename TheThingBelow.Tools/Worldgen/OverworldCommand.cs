using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Tools.Worldgen;

/// <summary>
/// The `overworld` command. It reads the settings of the generator, makes the land, and writes
/// the terrain rows, the zone grid, and the tile of each thing of the settings into the map file
/// of the overworld (D-1294, D-1295). Every other field of the map file stays as the author
/// wrote it. The `--check` option compares the map file with the output and writes nothing.
/// </summary>
public static class OverworldCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = "overworld";

    /// <summary>The option that names the root of the checkout.</summary>
    public const string RootOption = "--root";

    /// <summary>The option that compares the map file with the output and writes nothing.</summary>
    public const string CheckOption = "--check";

    /// <summary>The tile of a thing on its line of the map file.</summary>
    private static readonly Regex TilePattern = new("\"x\": -?[0-9]+, \"y\": -?[0-9]+", RegexOptions.CultureInvariant);

    /// <summary>Writes the map of the overworld, or compares it with the output of the generator.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer that takes each line of the report.</param>
    /// <param name="errors">The writer that takes each fault.</param>
    /// <returns>0 when the run holds, and 1 on any fault.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(Name, args, [RootOption], [CheckOption], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string root = options.ValueOr(RootOption, ".");
        try
        {
            return Build(root, options.Holds(CheckOption), output, errors);
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException or ContentException or InvalidOperationException)
        {
            errors.WriteLine($"Error: {Name} stopped on the root '{root}': {fault.Message}");
            return Program.FaultExitCode;
        }
    }

    /// <summary>Gives the text of a map file with the rows and the thing tiles of the generator in it.</summary>
    /// <param name="text">The text of the map file.</param>
    /// <param name="generated">The output of the generator.</param>
    /// <param name="file">The path of the map file, for each error.</param>
    /// <returns>The new text. The line ends and every other line stay as they were.</returns>
    /// <exception cref="ContentException">The file holds no terrain block, no zone grid block, or no line of a thing of the settings (T-2).</exception>
    public static string Rewrite(string text, GeneratedOverworld generated, string file)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(generated);
        ArgumentNullException.ThrowIfNull(file);

        string newline = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        List<string> lines = [.. text.Split(newline)];
        ReplaceBlock(lines, "terrain", generated.Terrain, file);
        ReplaceBlock(lines, "zone_grid", generated.Zones, file);
        foreach ((ContentId id, TilePoint at) in generated.Things)
        {
            int index = lines.FindIndex(line => line.Contains($"\"id\": \"{id.Value}\"", StringComparison.Ordinal));
            if (index < 0 || !TilePattern.IsMatch(lines[index]))
            {
                throw ContentException.ForField(file, "things", $"the file holds no line of the thing '{id.Value}' with its 'x' and its 'y', and the settings place it (D-1295)");
            }

            lines[index] = TilePattern.Replace(lines[index], $"\"x\": {at.X}, \"y\": {at.Y}", 1);
        }

        return string.Join(newline, lines);
    }

    private static int Build(string root, bool check, TextWriter output, TextWriter errors)
    {
        string content = Path.Combine(root, "content");
        OverworldPlan plan = OverworldPlan.Read(File.ReadAllBytes(Path.Combine(content, OverworldPlan.Path)), OverworldPlan.Path);
        (string path, string relative) = MapFileOf(content, plan.Map);
        string text = File.ReadAllText(path, Encoding.UTF8);
        string written = Rewrite(text, OverworldGenerator.Generate(plan), relative);
        if (check)
        {
            if (string.CompareOrdinal(text, written) != 0)
            {
                errors.WriteLine($"{Name}: '{relative}' differs from the output of '{OverworldPlan.Path}'. Run `{Name} --root .` and read the new map (D-1295).");
                return Program.FaultExitCode;
            }

            output.WriteLine($"{Name}: '{relative}' matches the output of '{OverworldPlan.Path}'.");
            return 0;
        }

        File.WriteAllText(path, written, new UTF8Encoding(false));
        output.WriteLine($"{Name}: wrote '{relative}' from '{OverworldPlan.Path}'.");
        return 0;
    }

    /// <summary>Finds the map file whose id the settings name, among the map files of the rule folder.</summary>
    private static (string Path, string Relative) MapFileOf(string content, ContentId map)
    {
        string folder = Path.Combine(content, "rules", "maps");
        string[] files = Directory.GetFiles(folder, "*.json");
        Array.Sort(files, StringComparer.Ordinal);
        foreach (string path in files)
        {
            if (File.ReadAllText(path, Encoding.UTF8).Contains($"\"id\": \"{map.Value}\"", StringComparison.Ordinal))
            {
                return (path, $"rules/maps/{Path.GetFileName(path)}");
            }
        }

        throw ContentException.ForField(OverworldPlan.Path, "map", $"no file of 'rules/maps' holds the map '{map.Value}' (D-1295)");
    }

    /// <summary>Replaces the rows of one array of strings of the map file, one row on each line.</summary>
    private static void ReplaceBlock(List<string> lines, string field, IReadOnlyList<string> rows, string file)
    {
        int start = lines.FindIndex(line => string.CompareOrdinal(line.Trim(), $"\"{field}\": [") == 0);
        int end = start < 0 ? -1 : lines.FindIndex(start, line => line.Trim().StartsWith(']'));
        if (start < 0 || end < 0)
        {
            throw ContentException.ForField(file, field, $"the file holds no block of '{field}' with one row on each line (D-1295)");
        }

        string indent = lines[start][..(lines[start].Length - lines[start].TrimStart().Length)] + " ";
        List<string> block = [];
        for (int row = 0; row < rows.Count; row += 1)
        {
            block.Add($"{indent}\"{rows[row]}\"{(row < rows.Count - 1 ? "," : string.Empty)}");
        }

        lines.RemoveRange(start + 1, end - start - 1);
        lines.InsertRange(start + 1, block);
    }

}
