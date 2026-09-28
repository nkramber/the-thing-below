using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using TheThingBelow.Core.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// No rule of Core reads an edge file or an edge rule, so a new edge piece never changes the
/// content hash or a replay (D-495, D-501, G-1). Exit test 4 of PR-53.
/// </summary>
/// <remarks>
/// Core holds the record of every content file, a file that no rule reads included (D-517). Thus
/// the records of the edges live in Core, and the content set reads them for Game and the map
/// preview. No other type of Core names them.
/// </remarks>
public sealed class EdgeBoundaryTests
{
    private static readonly Regex EdgeType = new(@"\bEdge(File|Rule|Content|Tile|Place|Places)\b", RegexOptions.CultureInvariant);

    [Fact]
    public void NoTypeOfCoreOutsideTheEdgeRecordsAndTheContentSetNamesAnEdgeType()
    {
        string core = RepositoryRoot.PathTo("TheThingBelow.Core");
        string edges = Path.Combine(core, "Edges") + Path.DirectorySeparatorChar;
        string contentSet = Path.Combine(core, "Content", "ContentSet.cs");
        string[] files = Directory.GetFiles(core, "*.cs", SearchOption.AllDirectories);
        Array.Sort(files, StringComparer.Ordinal);

        var readers = new List<string>();
        int read = 0;
        foreach (string file in files)
        {
            string relative = Path.GetRelativePath(core, file);
            if (relative.StartsWith("obj", StringComparison.Ordinal) || relative.StartsWith("bin", StringComparison.Ordinal))
            {
                continue;
            }

            read += 1;
            if (file.StartsWith(edges, StringComparison.Ordinal) || string.CompareOrdinal(file, contentSet) == 0)
            {
                continue;
            }

            if (EdgeType.IsMatch(File.ReadAllText(file, Encoding.UTF8)))
            {
                readers.Add(relative);
            }
        }

        Assert.True(read > 50, $"The test read {read} files of Core alone.");
        Assert.True(readers.Count == 0, $"These files of Core name an edge type, and no rule reads an edge file (D-501, G-1): {string.Join(", ", readers)}");
    }

    [Fact]
    public void AnEdgeFileAndAnEdgeRuleNeverReachTheContentHash()
    {
        // D-501: a new edge piece never changes the content hash, so a stored replay still runs.
        string before = ContentHash.Compute(
        [
            FileOf("rules/maps/one.json", "a map"),
            FileOf("edges/kinds/water.json", "a rule"),
            FileOf("edges/maps/one.json", "an edge file"),
        ]);

        string after = ContentHash.Compute(
        [
            FileOf("rules/maps/one.json", "a map"),
            FileOf("edges/kinds/water.json", "another rule"),
            FileOf("edges/maps/one.json", "another edge file"),
        ]);

        Assert.Equal(before, after);
    }

    private static ContentFile FileOf(string path, string text) => new(path, Encoding.UTF8.GetBytes(text));
}
