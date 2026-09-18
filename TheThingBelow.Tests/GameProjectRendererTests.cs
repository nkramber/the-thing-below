using System;
using System.IO;
using System.Linq;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The game uses the Mobile renderer, which the Deck test picked (D-160, D-616). This test
/// reads the committed project file, because the Godot editor writes that file and can write
/// the default renderer of Godot into it again (D-599).
/// </summary>
public sealed class GameProjectRendererTests
{
    /// <summary>The project file of the Game project (D-217).</summary>
    private const string ProjectFilePath = "TheThingBelow.Game/project.godot";

    /// <summary>The setting that names the renderer of every shipped platform (D-616).</summary>
    private const string RendererSetting = "renderer/rendering_method=";

    /// <summary>The setting that names the feature tags of the project.</summary>
    private const string FeaturesSetting = "config/features=";

    [Fact]
    public void TheProjectSetsTheMobileRenderer()
    {
        string line = ReadSetting(RendererSetting);

        Assert.Equal("renderer/rendering_method=\"mobile\"", line);
    }

    [Fact]
    public void TheFeatureTagsNameTheMobileRenderer()
    {
        // Godot writes the renderer of the project into the feature tags, and the editor
        // reads them back. A tag of one renderer with the setting of another confuses it.
        string line = ReadSetting(FeaturesSetting);

        Assert.Contains("\"Mobile\"", line, StringComparison.Ordinal);
        Assert.DoesNotContain("Forward Plus", line, StringComparison.Ordinal);
    }

    /// <summary>Reads the one line of the project file that starts with a setting name.</summary>
    /// <param name="settingName">The name of the setting, with its equals sign.</param>
    /// <returns>The whole line, with no leading or trailing space.</returns>
    /// <exception cref="InvalidOperationException">No line, or more than one, starts with it.</exception>
    private static string ReadSetting(string settingName)
    {
        string path = RepositoryRoot.PathTo(ProjectFilePath);
        string[] matches = File.ReadAllLines(path)
            .Select(line => line.Trim())
            .Where(line => line.StartsWith(settingName, StringComparison.Ordinal))
            .ToArray();

        if (matches.Length != 1)
        {
            throw new InvalidOperationException(
                $"The file '{path}' holds {matches.Length} lines that start with " +
                $"'{settingName}'. One line is the contract (T-2).");
        }

        return matches[0];
    }
}
