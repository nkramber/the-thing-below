using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The rule of every shader file of the Game project on lit art (D-183, D-825).</summary>
public sealed class GameShaderTests
{
    private const string ShaderFolder = "TheThingBelow.Game/shaders";

    [Fact]
    public void NoShaderOfTheGameWritesTheNormalMap()
    {
        // D-183: Godot corrects the normal of a flipped draw before the shader code, and a
        // write of NORMAL_MAP replaces that correction, so a lit sprite takes a wrong light.
        string[] files = Directory.GetFiles(Path.Combine(RepositoryRoot.Find(), ShaderFolder), "*.gdshader");
        Assert.NotEmpty(files);

        var failures = new List<string>();
        foreach (string file in files)
        {
            if (CodeOf(File.ReadAllText(file)).Contains("NORMAL_MAP", StringComparison.Ordinal))
            {
                failures.Add(Path.GetFileName(file));
            }
        }

        Assert.True(failures.Count == 0, $"these shaders write NORMAL_MAP: {string.Join(", ", failures)} (D-183)");
    }

    /// <summary>Removes each line comment, so a comment that names the member passes.</summary>
    private static string CodeOf(string text)
    {
        var lines = new List<string>();
        foreach (string line in text.Split('\n'))
        {
            if (!line.TrimStart().StartsWith("//", StringComparison.Ordinal))
            {
                lines.Add(line);
            }
        }

        return string.Join('\n', lines);
    }
}
