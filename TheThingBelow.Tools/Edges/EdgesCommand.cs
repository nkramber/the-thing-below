using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Edges;
using TheThingBelow.Core.Maps;
using TheThingBelow.Tools.Content;

namespace TheThingBelow.Tools.Edges;

/// <summary>
/// The `edges` command. It reads each map and each edge rule, picks the edge pieces of each map,
/// and writes the edge file of each map (D-204, D-501, D-1321). The `--check` option compares
/// each edge file with the output and writes nothing.
/// </summary>
/// <remarks>
/// The command reads the map files and the edge rules alone, and never the whole content set. A
/// content set with a stale edge file fails its load, and this command is the fix of that file.
/// The edge file of a map takes the name of the map file (D-1327).
/// </remarks>
public static class EdgesCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = "edges";

    /// <summary>The option that names the root of the checkout.</summary>
    public const string RootOption = "--root";

    /// <summary>The option that compares each edge file with the output and writes nothing.</summary>
    public const string CheckOption = "--check";

    /// <summary>Writes the edge file of each map, or compares each one with the output of the pick.</summary>
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
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException or ContentException)
        {
            errors.WriteLine($"Error: {Name} stopped on the root '{root}': {fault.Message}");
            return Program.FaultExitCode;
        }
    }

    /// <summary>Gives the path of the edge file of one map file, under `content/` (D-1327).</summary>
    /// <param name="mapPath">The path of the map file, such as `rules/maps/overworld.json`.</param>
    /// <returns>The path of its edge file, such as `edges/maps/overworld.json`.</returns>
    public static string EdgePathOf(string mapPath)
    {
        ArgumentNullException.ThrowIfNull(mapPath);

        return EdgeFile.Folder + mapPath[GameMap.Folder.Length..];
    }

    private static int Build(string root, bool check, TextWriter output, TextWriter errors)
    {
        IReadOnlyList<ContentFile> files = ContentFolder.Read(root);
        SortedDictionary<TileKind, EdgeRule> rules = ReadRules(files);
        var wanted = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (ContentFile file in files)
        {
            if (GameMap.IsMapFile(file.Path))
            {
                GameMap map = GameMap.Read(file.Bytes, file.Path);
                wanted.Add(EdgePathOf(file.Path), EdgePicker.TextOf(map.Id, EdgePicker.Pick(map, rules)));
            }
        }

        if (wanted.Count == 0)
        {
            // A run that writes nothing reads as a pass, so the command fails (T-2).
            errors.WriteLine($"Error: {Name} found no map under '{GameMap.Folder}' of the root '{root}' (T-2).");
            return Program.FaultExitCode;
        }

        int faults = RefuseFileWithNoMap(files, wanted, errors);
        string content = Path.Combine(root, ContentFolder.FolderName);
        foreach ((string path, string text) in wanted)
        {
            string full = Path.Combine(content, path.Replace('/', Path.DirectorySeparatorChar));
            if (check)
            {
                faults += CheckFile(full, path, text, errors);
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(full) ?? throw new IOException($"the path '{full}' has no folder"));
            File.WriteAllText(full, text, new UTF8Encoding(false));
            output.WriteLine($"{Name}: wrote '{path}'.");
        }

        if (faults > 0)
        {
            return Program.FaultExitCode;
        }

        output.WriteLine(check
            ? $"{Name}: each of the {wanted.Count} edge files matches its map and the edge rules."
            : $"{Name}: maps {wanted.Count}.");
        return 0;
    }

    private static SortedDictionary<TileKind, EdgeRule> ReadRules(IReadOnlyList<ContentFile> files)
    {
        var rules = new SortedDictionary<TileKind, EdgeRule>();
        foreach (ContentFile file in files)
        {
            if (!EdgeRule.IsRuleFile(file.Path))
            {
                continue;
            }

            EdgeRule rule = EdgeRule.Read(file.Bytes, file.Path);
            if (!rules.TryAdd(rule.Kind, rule))
            {
                throw ContentException.ForField(file.Path, "kind", $"'{TileKinds.NameOf(rule.Kind)}': a second edge rule names this kind, and a kind has one (D-1327)");
            }
        }

        return rules;
    }

    /// <summary>Counts each edge file whose map file does not exist, and names each one.</summary>
    private static int RefuseFileWithNoMap(IReadOnlyList<ContentFile> files, SortedDictionary<string, string> wanted, TextWriter errors)
    {
        int faults = 0;
        foreach (ContentFile file in files)
        {
            if (EdgeFile.IsEdgeFile(file.Path) && !wanted.ContainsKey(file.Path))
            {
                errors.WriteLine($"{Name}: '{file.Path}' names no map file of '{GameMap.Folder}'. Remove it, or rename it to the name of its map file (D-1327).");
                faults += 1;
            }
        }

        return faults;
    }

    private static int CheckFile(string full, string path, string text, TextWriter errors)
    {
        if (!File.Exists(full))
        {
            errors.WriteLine($"{Name}: '{path}' does not exist. Run `{Name} --root .` and commit the new file (D-501).");
            return 1;
        }

        if (string.CompareOrdinal(File.ReadAllText(full, Encoding.UTF8), text) != 0)
        {
            errors.WriteLine($"{Name}: '{path}' differs from the output of its map and the edge rules. Run `{Name} --root .` and read the new file (D-501).");
            return 1;
        }

        return 0;
    }
}
