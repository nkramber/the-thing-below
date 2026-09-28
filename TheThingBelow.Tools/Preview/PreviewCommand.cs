using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Tools.Content;
using TheThingBelow.Tools.Png;

namespace TheThingBelow.Tools.Preview;

/// <summary>
/// The `preview` command: it renders each map of a checkout as a PNG from the committed atlas,
/// for the approval of the owner (D-165, D-1319). The session attaches each preview to the PR
/// description, and no preview enters git (D-514, D-1320).
/// </summary>
public static class PreviewCommand
{
    /// <summary>The name of the command.</summary>
    public const string Name = "preview";

    /// <summary>The option that names the root of the checkout.</summary>
    public const string RootOption = "--root";

    /// <summary>The option that names the folder that takes one PNG for each map.</summary>
    public const string OutOption = "--out";

    /// <summary>The option that names the one map to render (D-1319).</summary>
    public const string MapOption = "--map";

    /// <summary>Renders each map of the content set, or the one map of <see cref="MapOption"/>, into the output folder.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer that takes one line for each file.</param>
    /// <param name="errors">The writer that takes each fault.</param>
    /// <returns>0 when each map renders, and 1 on any fault.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(Name, args, [RootOption, OutOption, MapOption], [], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string root = options.ValueOr(RootOption, ".");
        string? folder = options.Value(OutOption);
        if (folder is null)
        {
            errors.WriteLine($"Error: {Name} takes {OutOption} <folder>, the folder of the previews (T-2).");
            return Program.FaultExitCode;
        }

        try
        {
            return Write(root, folder, options.Value(MapOption), output, errors);
        }
        catch (Exception fault) when (
            fault is IOException or UnauthorizedAccessException or ContentException
                or PngException or InvalidOperationException)
        {
            errors.WriteLine($"Error: {Name} stopped on the root '{root}': {fault.Message}");
            return Program.FaultExitCode;
        }
    }

    private static int Write(string root, string folder, string? mapText, TextWriter output, TextWriter errors)
    {
        ContentSet content = ContentSet.Load(ContentFolder.Read(root));
        List<GameMap> maps = MapsToRender(content, mapText);
        if (maps.Count == 0)
        {
            // A run that writes nothing reads as a pass, so the command fails (T-2).
            errors.WriteLine($"Error: {Name} found no map under '{GameMap.Folder}' of the root '{root}' (T-2).");
            return Program.FaultExitCode;
        }

        SortedDictionary<string, PngImage> pages = MapPreview.ReadPages(root, content.Atlas);
        Directory.CreateDirectory(folder);
        foreach (GameMap map in maps)
        {
            PngImage image = MapPreview.Render(map, content.Light.DecorOf(map.Id), content.Atlas, pages);
            string path = Path.Combine(folder, $"{map.Id.Name}.png");
            PngWriter.WriteFile(path, image);
            output.WriteLine($"{Name}: wrote {path}, {image.Width} by {image.Height} pixels.");
        }

        output.WriteLine($"{Name}: maps {maps.Count}.");
        return 0;
    }

    /// <summary>Gives each map of the set in the order of its id, or the one map that the option names (D-1319).</summary>
    /// <exception cref="ContentException">The option holds a malformed id, or the set holds no map with that id (T-2).</exception>
    private static List<GameMap> MapsToRender(ContentSet content, string? mapText)
    {
        if (mapText is not null)
        {
            return [content.Map(ContentId.Parse(mapText, Name, MapOption))];
        }

        var maps = new List<GameMap>(content.Maps);
        maps.Sort(static (first, second) => string.CompareOrdinal(first.Id.Value, second.Id.Value));
        return maps;
    }
}
