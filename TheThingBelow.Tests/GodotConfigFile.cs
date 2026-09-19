using System;
using System.Collections.Generic;
using System.IO;

namespace TheThingBelow.Tests;

/// <summary>
/// Reads a Godot configuration file, such as `project.godot` or `export_presets.cfg`. The
/// format is a list of `[section]` headers, each with `key=value` lines and `;` comments.
/// </summary>
/// <remarks>
/// The reader keeps each value as the file writes it, quotation marks included, so a test
/// reads the committed text and not a parsed form of it (T-1).
/// </remarks>
public static class GodotConfigFile
{
    /// <summary>Reads every section of a configuration file of the checkout.</summary>
    /// <param name="relativePath">The path of the file under the root, with forward slashes.</param>
    /// <returns>The keys and the raw values of each section, by the name of the section.</returns>
    /// <exception cref="InvalidOperationException">A section name or a key appears two times.</exception>
    public static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Read(
        string relativePath)
    {
        string path = RepositoryRoot.PathTo(relativePath);
        Dictionary<string, IReadOnlyDictionary<string, string>> sections =
            new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal);

        // The lines before the first header belong to the unnamed section, which
        // `project.godot` uses for `config_version`.
        Dictionary<string, string> current = new Dictionary<string, string>(StringComparer.Ordinal);
        sections.Add(string.Empty, current);
        string name = string.Empty;

        foreach (string raw in File.ReadAllLines(path))
        {
            string line = raw.Trim();
            if (line.Length == 0 || line.StartsWith(';'))
            {
                continue;
            }

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                name = line[1..^1];
                current = new Dictionary<string, string>(StringComparer.Ordinal);
                if (!sections.TryAdd(name, current))
                {
                    throw new InvalidOperationException(
                        $"The file '{path}' holds the section '[{name}]' two times (T-2).");
                }

                continue;
            }

            int split = line.IndexOf('=', StringComparison.Ordinal);
            if (split <= 0)
            {
                throw new InvalidOperationException(
                    $"The line '{line}' of '{path}' is no section and no `key=value` pair (T-2).");
            }

            string key = line[..split].Trim();
            if (!current.TryAdd(key, line[(split + 1)..].Trim()))
            {
                throw new InvalidOperationException(
                    $"The section '[{name}]' of '{path}' holds the key '{key}' two times (T-2).");
            }
        }

        return sections;
    }

    /// <summary>Reads one key of one section, and fails when the file holds neither.</summary>
    /// <param name="sections">The sections that <see cref="Read"/> returned.</param>
    /// <param name="section">The name of the section, without the brackets.</param>
    /// <param name="key">The name of the key.</param>
    /// <returns>The raw value, with the quotation marks of the file.</returns>
    /// <exception cref="InvalidOperationException">The section or the key is absent (T-2).</exception>
    public static string ValueOf(
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> sections,
        string section,
        string key)
    {
        if (!sections.TryGetValue(section, out IReadOnlyDictionary<string, string>? keys))
        {
            throw new InvalidOperationException($"The file holds no section '[{section}]' (T-2).");
        }

        if (!keys.TryGetValue(key, out string? value))
        {
            throw new InvalidOperationException(
                $"The section '[{section}]' holds no key '{key}' (T-2).");
        }

        return value;
    }
}
